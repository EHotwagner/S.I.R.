namespace SIR.Server

open System
open System.Text.Json
open System.Text.Json.Serialization
open FS.GG.Game.Core
open SIR.Domain
open SIR.Simulation

[<CLIMutable>]
type SpatialDiagnosticTerrainDto =
    { Column: int32
      Row: int32
      Kind: int32 }

[<CLIMutable>]
type SpatialDiagnosticRequestDto =
    { MapIdentity: string
      SpatialRevision: int64
      Width: int32
      Height: int32
      OriginColumn: int32
      OriginRow: int32
      UnitSize: int32
      Facing: int32
      Terrain: SpatialDiagnosticTerrainDto array }

[<CLIMutable>]
type SpatialDiagnosticResponseDto =
    { QueryKind: string
      Outcome: string
      FootprintSampleCount: int32
      CrossedCellCount: int32
      CrossedEdgeCount: int32
      CoverContributorCount: int32
      ExposureDirections: string
      SpatialRevision: int64
      KnowledgeIdentity: string
      PackageIdentity: string
      CompatibilityProfile: string
      SourceSymbol: string }

[<RequireQualifiedAccess>]
module SpatialDiagnostics =
    // Thoth preserves int64 values as JSON strings in JavaScript; accept that
    // lossless wire form while emitting normal typed response properties.
    let private jsonOptions = JsonSerializerOptions(PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString)
    let private cell col row: Cell = { Col = col; Row = row }

    let evaluate (json: string) =
        try
            let boxedInput = JsonSerializer.Deserialize<SpatialDiagnosticRequestDto>(json, jsonOptions) |> box
            if isNull boxedInput then
                Error "invalid spatial diagnostic identity"
            else
             let input = unbox<SpatialDiagnosticRequestDto> boxedInput
             if String.IsNullOrWhiteSpace input.MapIdentity then
                Error "invalid spatial diagnostic identity"
             elif input.Width <= 0 || input.Height <= 0 || input.Width > 80 || input.Height > 80 then
                Error "invalid spatial diagnostic dimensions"
             elif input.UnitSize <= 0 || input.UnitSize > 16 then
                Error "invalid spatial diagnostic footprint"
             elif input.OriginColumn < 0 || input.OriginRow < 0 || input.OriginColumn >= input.Width || input.OriginRow >= input.Height then
                Error "invalid spatial diagnostic origin"
             elif Object.ReferenceEquals(input.Terrain, null) || input.Terrain.Length > input.Width * input.Height then
                Error "invalid spatial diagnostic terrain"
             else
                match Direction8.tryFromCode (byte input.Facing) with
                | None -> Error "invalid spatial diagnostic facing"
                | Some facing ->
                    let terrain =
                        input.Terrain
                        |> Array.map (fun value ->
                            if value.Column < 0 || value.Row < 0 || value.Column >= input.Width || value.Row >= input.Height then
                                invalidArg "Terrain" "terrain cell is outside the declared map"
                            let kind =
                                match value.Kind with
                                | 0 -> SpatialTerrain.Open
                                | 1 -> SpatialTerrain.Rough
                                | 2 -> SpatialTerrain.Blocked
                                | _ -> invalidArg "Terrain" "terrain kind is unknown"
                            cell value.Column value.Row, kind)
                        |> Map.ofArray
                    if terrain.Count <> input.Terrain.Length then
                        Error "duplicate spatial diagnostic terrain cell"
                    else
                        let identity =
                            SpatialAuthorityIdentity.create input.MapIdentity "sir-spatial-v1" input.SpatialRevision "player-disclosed" input.SpatialRevision
                            |> Result.defaultWith (fun error -> invalidArg "MapIdentity" error)
                        let world =
                            { Identity = identity
                              Minimum = cell 0 0
                              Maximum = cell (input.Width - 1) (input.Height - 1)
                              Terrain = terrain
                              Boundaries = []
                              Occupancy = Map.empty
                              DisclosedRevisionTokens = Set.empty }
                        let origin = cell input.OriginColumn input.OriginRow
                        let target = cell (min (input.Width - 1) (input.OriginColumn + 4)) input.OriginRow
                        let request =
                            { QueryId = "selected-unit-diagnostics"
                              QueryKind = SpatialQueryKind.ExactLineOfSight
                              Origin = origin
                              Target = target
                              Footprint = [ for row in 0 .. input.UnitSize - 1 do for column in 0 .. input.UnitSize - 1 do yield cell column row ]
                              Profile =
                                { ProfileId = "selected-unit-sensor-v1"
                                  Modality = SpatialModality.Vision
                                  Stance = "standing"
                                  HeightBand = 1
                                  Facing = facing }
                              Bounds = SpatialQuery.defaultBounds }
                        let result, _ = SpatialQuery.evaluate world request
                        let response =
                            { QueryKind = string result.Explanation.QueryKind
                              Outcome = string result.Outcome
                              FootprintSampleCount = int32 result.Explanation.FootprintSamples.Length
                              CrossedCellCount = int32 result.Explanation.CrossedCells.Length
                              CrossedEdgeCount = int32 result.Explanation.CrossedEdges.Length
                              CoverContributorCount = int32 result.Explanation.CoverContributors.Length
                              ExposureDirections = result.Explanation.ExposureDirections |> List.map string |> String.concat ", "
                              SpatialRevision = result.Explanation.SpatialRevision
                              KnowledgeIdentity = result.Explanation.KnowledgeIdentity
                              PackageIdentity = SpatialQuery.packageIdentity
                              CompatibilityProfile = SpatialQuery.compatibilityProfile
                              SourceSymbol = result.Explanation.SourceSymbol }
                        Ok(JsonSerializer.Serialize(response, jsonOptions))
        with
        | :? JsonException -> Error "invalid spatial diagnostic JSON"
        | :? ArgumentException -> Error "invalid spatial diagnostic payload"
