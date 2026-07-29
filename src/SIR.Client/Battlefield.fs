namespace SIR.Client

open System
open System.Globalization

type SemanticZoom =
    | Overview
    | Standard
    | Detailed

type BattlefieldCamera =
    { PanX: float
      PanY: float
      Zoom: float }

type BattlefieldViewState =
    { Camera: BattlefieldCamera
      SemanticZoom: SemanticZoom
      SelectedUnit: int32 option
      FocusedUnit: int32 option
      PaletteId: string }

type BattlefieldAction =
    | PanBy of x: float * y: float
    | ZoomBy of factor: float
    | SelectUnit of int32 option
    | FocusUnit of int32 option
    | FocusDirection of x: int * y: int
    | ChoosePalette of string
    | ResetCamera

type ProjectedUnit =
    { Unit: UnitVisual
      FootprintX: float
      FootprintY: float
      FootprintWidth: float
      FootprintDepth: float
      SymbolCenterX: float
      SymbolCenterY: float
      HealthSegments: int option
      ElevationBars: int
      ElevationLabel: string option
      ShowStance: bool
      AccessibleLabel: string }

type BattlefieldScene =
    { Tick: int32
      Width: float
      Height: float
      CellSize: float
      Board: BoardVisual
      Units: ProjectedUnit array
      Edges: EdgeVisual array
      Disclosure: DisclosureLabel
      Palette: PaletteTokens
      Camera: BattlefieldCamera
      SemanticZoom: SemanticZoom
      SelectedUnit: int32 option
      FocusedUnit: int32 option
      InteractiveNodeEstimate: int }

[<RequireQualifiedAccess>]
module Battlefield =
    [<Literal>]
    let CellSize = 48.0

    [<Literal>]
    let OverviewThresholdPx = 24.0

    [<Literal>]
    let DetailedThresholdPx = 48.0

    [<Literal>]
    let Hysteresis = 0.10

    let initial =
        { Camera =
            { PanX = 24.0
              PanY = 24.0
              Zoom = 1.0 }
          SemanticZoom = Detailed
          SelectedUnit = Some 1
          FocusedUnit = Some 1
          PaletteId = ReplayPalettes.accessibleDefault.Id }

    let private clamp minimum maximum value =
        max minimum (min maximum value)

    /// Applies the 24/48 px thresholds with a ten-percent dead band.
    let semanticZoom previous cellPixels =
        let lowerEnter = OverviewThresholdPx * (1.0 + Hysteresis)
        let lowerLeave = OverviewThresholdPx * (1.0 - Hysteresis)
        let upperEnter = DetailedThresholdPx * (1.0 + Hysteresis)
        let upperLeave = DetailedThresholdPx * (1.0 - Hysteresis)

        match previous with
        | Overview when cellPixels >= upperEnter -> Detailed
        | Overview when cellPixels >= lowerEnter -> Standard
        | Overview -> Overview
        | Standard when cellPixels < lowerLeave -> Overview
        | Standard when cellPixels >= upperEnter -> Detailed
        | Standard -> Standard
        | Detailed when cellPixels < lowerLeave -> Overview
        | Detailed when cellPixels < upperLeave -> Standard
        | Detailed -> Detailed

    let private palette paletteId =
        ReplayPalettes.all
        |> Array.tryFind (fun candidate -> candidate.Id = paletteId)
        |> Option.defaultValue ReplayPalettes.accessibleDefault

    let private disclosedOr fallback disclosure =
        match disclosure with
        | Disclosed value -> value
        | NotPresent
        | NotApplicable
        | ExplicitlyUnknown -> fallback

    let private healthSegments disclosure =
        match disclosure with
        | Disclosed health ->
            let remaining = int64 (HealthVisual.remaining health)
            let maximum = int64 (HealthVisual.maximum health)
            int ((remaining * 12L + maximum - 1L) / maximum)
            |> clamp 0 12
            |> Some
        | NotPresent
        | NotApplicable
        | ExplicitlyUnknown -> None

    let private compass heading =
        let directions =
            [| "east"
               "south-east"
               "south"
               "south-west"
               "west"
               "north-west"
               "north"
               "north-east" |]

        let octant =
            int (Math.Round(HeadingRadians.value heading / (Math.PI / 4.0))) % 8

        directions[octant]

    let private cellName column row =
        let letter =
            if column >= 0 && column < 26 then
                string (char (int 'A' + column))
            else
                "column " + string column + " "

        letter + string (row + 1)

    let private unitLabel (unit: UnitVisual) =
        let glyph = UnitGlyphCatalog.resolve unit.ClassId
        let faction =
            match unit.Faction with
            | Human -> "Blue"
            | Arcane -> "Arcane"
            | Neutral -> "Neutral"
            | OtherFaction stableId -> stableId

        let identity =
            match unit.ShortLabel with
            | Disclosed value -> " " + value
            | _ -> " " + string unit.Id

        let level =
            match unit.Level with
            | Disclosed value -> ", elevation " + string value
            | NotPresent -> ", elevation not present in this projection"
            | NotApplicable -> ", elevation not applicable"
            | ExplicitlyUnknown -> ", elevation explicitly unknown"

        let health =
            match unit.Health with
            | Disclosed value ->
                let percent =
                    int (
                        Math.Round(
                            float (HealthVisual.remaining value)
                            * 100.0
                            / float (HealthVisual.maximum value)
                        )
                    )
                ", " + string percent + " health"
            | NotPresent -> ", health not present in this projection"
            | NotApplicable -> ", health not applicable"
            | ExplicitlyUnknown -> ", health explicitly unknown"

        let facing =
            match unit.BodyHeading with
            | Disclosed value -> ", facing " + compass value
            | NotPresent -> ", facing not present in this projection"
            | NotApplicable -> ", facing not applicable"
            | ExplicitlyUnknown -> ", facing explicitly unknown"

        faction
        + " "
        + glyph.Name.ToLowerInvariant()
        + identity
        + ", cell "
        + cellName unit.AnchorColumn unit.AnchorRow
        + level
        + health
        + facing

    let private projectUnit
        (board: BoardVisual)
        tier
        (unit: UnitVisual)
        : ProjectedUnit
        =
        let width = float (CellExtent.value unit.FootprintWidth) * CellSize
        let depth = float (CellExtent.value unit.FootprintDepth) * CellSize
        let x = float (unit.AnchorColumn - board.MinimumColumn) * CellSize
        let y = float (unit.AnchorRow - board.MinimumRow) * CellSize
        let level = disclosedOr 0 unit.Level |> max 0

        { Unit = unit
          FootprintX = x
          FootprintY = y
          FootprintWidth = width
          FootprintDepth = depth
          SymbolCenterX = x + width / 2.0
          SymbolCenterY = y + depth / 2.0
          HealthSegments = healthSegments unit.Health
          ElevationBars = min 3 level
          ElevationLabel =
            if tier = Detailed && level > 3 then
                Some("+" + string level)
            else
                None
          ShowStance =
            tier = Detailed
            && match unit.StanceId with
               | Disclosed _ -> true
               | _ -> false
          AccessibleLabel = unitLabel unit }

    let private nodeEstimate tier (units: ProjectedUnit array) edgeCount =
        let perUnit unit =
            // group, footprint, symbol, glyph primitives, health positions,
            // facing, focus/selection, elevation and optional labels/stance.
            18
            + (UnitGlyphCatalog.resolve unit.Unit.ClassId).Primitives.Length
            + (if tier = Overview then
                   0
               else
                   (if unit.HealthSegments.IsSome then 12 else 0)
                   + unit.ElevationBars)
            + (if unit.ElevationLabel.IsSome then 1 else 0)
            + (if tier = Detailed && unit.ShowStance then 1 else 0)

        16 + edgeCount + (units |> Array.sumBy perUnit)

    let scene (frame: RenderFrame) (state: BattlefieldViewState) : BattlefieldScene =
        let tier =
            semanticZoom state.SemanticZoom (CellSize * state.Camera.Zoom)
        let units = frame.Units |> Array.map (projectUnit frame.Board tier)
        let columns = frame.Board.MaximumColumn - frame.Board.MinimumColumn + 1
        let rows = frame.Board.MaximumRow - frame.Board.MinimumRow + 1

        { Tick = frame.Tick
          Width = float columns * CellSize
          Height = float rows * CellSize
          CellSize = CellSize
          Board = frame.Board
          Units = units
          Edges = frame.Edges
          Disclosure = frame.Disclosure
          Palette = palette state.PaletteId
          Camera = state.Camera
          SemanticZoom = tier
          SelectedUnit = state.SelectedUnit
          FocusedUnit = state.FocusedUnit
          InteractiveNodeEstimate = nodeEstimate tier units frame.Edges.Length }

    let private directionalFocus
        xDirection
        yDirection
        (units: UnitVisual array)
        focused
        =
        let current =
            focused
            |> Option.bind (fun id ->
                units |> Array.tryFind (fun unit -> unit.Id = id))
            |> Option.orElseWith (fun () -> units |> Array.tryHead)

        match current with
        | None -> None
        | Some current ->
            units
            |> Array.filter (fun candidate ->
                candidate.Id <> current.Id
                && (xDirection = 0
                    || Math.Sign(candidate.AnchorColumn - current.AnchorColumn)
                       = xDirection)
                && (yDirection = 0
                    || Math.Sign(candidate.AnchorRow - current.AnchorRow)
                       = yDirection))
            |> Array.sortBy (fun candidate ->
                let dx = candidate.AnchorColumn - current.AnchorColumn
                let dy = candidate.AnchorRow - current.AnchorRow
                dx * dx + dy * dy, candidate.Id)
            |> Array.tryHead
            |> Option.map _.Id
            |> Option.orElse (Some current.Id)

    let update
        (frame: RenderFrame)
        action
        (state: BattlefieldViewState)
        : BattlefieldViewState
        =
        match action with
        | PanBy(x, y) ->
            { state with
                Camera =
                    { state.Camera with
                        PanX = state.Camera.PanX + x
                        PanY = state.Camera.PanY + y } }
        | ZoomBy factor ->
            let zoom = clamp 0.35 3.0 (state.Camera.Zoom * factor)
            { state with
                Camera = { state.Camera with Zoom = zoom }
                SemanticZoom =
                    semanticZoom state.SemanticZoom (CellSize * zoom) }
        | SelectUnit unitId -> { state with SelectedUnit = unitId }
        | FocusUnit unitId -> { state with FocusedUnit = unitId }
        | FocusDirection(x, y) ->
            { state with
                FocusedUnit =
                    directionalFocus x y frame.Units state.FocusedUnit }
        | ChoosePalette paletteId ->
            if ReplayPalettes.all |> Array.exists (fun p -> p.Id = paletteId) then
                { state with PaletteId = paletteId }
            else
                state
        | ResetCamera -> { initial with PaletteId = state.PaletteId }

    let private extent value =
        CellExtent.tryCreate value
        |> Option.defaultWith (fun () -> invalidArg "value" "Extent must be positive.")

    let private heading value =
        HeadingRadians.tryCreate value
        |> Option.defaultWith (fun () -> invalidArg "value" "Heading must be finite.")

    let private health remaining =
        HealthVisual.tryCreate remaining 12
        |> Option.defaultWith (fun () -> invalidArg "remaining" "Health must be 0 through 12.")

    let private sampleUnit
        id
        column
        row
        width
        depth
        classId
        faction
        remaining
        level
        stance
        headingRadians
        label
        : UnitVisual
        =
        { Id = id
          AnchorColumn = column
          AnchorRow = row
          FootprintWidth = extent width
          FootprintDepth = extent depth
          ClassId = UnitClassId.resolve classId
          Faction = faction
          Health = Disclosed(health remaining)
          Level = Disclosed level
          StanceId = stance |> Option.map Disclosed |> Option.defaultValue NotApplicable
          BodyHeading = Disclosed(heading headingRadians)
          SecondaryHeading = NotPresent
          ShortLabel = Disclosed label
          StatusIds = [||] }

    /// A committed six-by-six documentation frame. It is never interpolated.
    let representativeFrame =
        { Tick = 24
          Board =
            { MinimumColumn = 0
              MinimumRow = 0
              MaximumColumn = 5
              MaximumRow = 5 }
          Units =
            [| sampleUnit 1 0 0 1 1 "rifleman" Human 12 0 (Some "standing") 0.0 "Bravo 6"
               sampleUnit 2 2 0 1 1 "medic" Human 9 2 (Some "kneeling") (Math.PI / 4.0) "Mercy"
               sampleUnit 3 0 3 2 1 "gunner" Human 6 4 (Some "prone") (Math.PI * 1.5) "Anvil"
               sampleUnit 4 4 1 1 2 "observation-drone" Neutral 11 1 None Math.PI "Kite"
               sampleUnit 5 4 4 1 1 "goblin" Arcane 8 0 (Some "crouched") Math.PI "Needle"
               sampleUnit 6 2 4 2 2 "troll" Arcane 3 7 (Some "braced") (Math.PI * 1.25) "Stone" |]
          Edges =
            [| { Id = "wall-north"
                 Kind = "wall"
                 State = "solid"
                 StartColumn = 0
                 StartRow = 3
                 EndColumn = 3
                 EndRow = 3 }
               { Id = "door-east"
                 Kind = "door"
                 State = "open"
                 StartColumn = 3
                 StartRow = 1
                 EndColumn = 3
                 EndRow = 2 }
               { Id = "window"
                 Kind = "window"
                 State = "intact"
                 StartColumn = 1
                 StartRow = 1
                 EndColumn = 2
                 EndRow = 1 } |]
          Overlays = [||]
          Events = [||]
          Disclosure = SandboxDisclosure }

    let performanceFrame unitCount =
        let columns = 20
        let rows = max 1 ((unitCount + columns - 1) / columns)
        let units =
            Array.init unitCount (fun index ->
                let faction = if index % 2 = 0 then Human else Arcane
                sampleUnit
                    (int32 (index + 1))
                    (int32 (index % columns))
                    (int32 (index / columns))
                    1
                    1
                    (if index % 2 = 0 then "rifleman" else "goblin")
                    faction
                    (int32 (index % 13))
                    (int32 (index % 8))
                    (if index % 3 = 0 then Some "kneeling" else None)
                    (float (index % 8) * Math.PI / 4.0)
                    ("Unit " + string (index + 1)))

        { representativeFrame with
            Tick = 100
            Board =
                { MinimumColumn = 0
                  MinimumRow = 0
                  MaximumColumn = int32 (columns - 1)
                  MaximumRow = int32 (rows - 1) }
            Units = units
            Edges = [||] }

    /// Stable evidence text useful in reviews without treating pixels as authority.
    let deterministicEvidence (scene: BattlefieldScene) =
        let number (value: float) =
            value.ToString("0.###", CultureInfo.InvariantCulture)

        [ "tick=" + string scene.Tick
          "board=" + number scene.Width + "x" + number scene.Height
          "tier=" + string scene.SemanticZoom
          "palette=" + scene.Palette.Id
          "camera=" + number scene.Camera.PanX + "," + number scene.Camera.PanY + "," + number scene.Camera.Zoom
          yield!
              scene.Units
              |> Array.map (fun unit ->
                  "unit="
                  + string unit.Unit.Id
                  + "@"
                  + number unit.FootprintX
                  + ","
                  + number unit.FootprintY
                  + ":"
                  + number unit.FootprintWidth
                  + "x"
                  + number unit.FootprintDepth
                  + ":health="
                  + (unit.HealthSegments |> Option.map string |> Option.defaultValue "omitted")
                  + ":elevation="
                  + string unit.ElevationBars
                  + ":stance="
                  + string unit.ShowStance) ]
        |> String.concat "\n"
