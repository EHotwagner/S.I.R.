namespace SIR.Client

open System
open System.Globalization
open System.Text
open SIR.Domain

type EvidenceMode =
    | VerifiedReplayEvidence
    | PerspectiveEvidence
    | DerivedSimulationEvidence

type EvidenceProvenance =
    { SourceIdentity: string
      ReplayIdentity: string
      ProjectionIdentity: string
      EngineIdentity: string
      RulesetIdentity: string option
      Tick: int32
      Mode: EvidenceMode
      PaletteIdentity: string
      RendererVersion: string }

type SvgEvidence =
    { FileName: string
      MediaType: string
      Svg: string
      Sha256: string
      Provenance: EvidenceProvenance }

[<RequireQualifiedAccess>]
module EvidenceExport =
    [<Literal>]
    let RendererVersion = "sir-safe-svg-renderer-v1"

    let private invariant (value: float) =
        value.ToString("0.###", CultureInfo.InvariantCulture)

    let private escapeText (value: string) =
        value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;")

    let private boundedText maximum (value: string) =
        value.Substring(0, min maximum value.Length)
        |> Seq.map (fun character ->
            if Char.IsLetterOrDigit character
               || character = ' '
               || character = '-'
               || character = '_'
               || character = '.' then
                character
            else
                ' ')
        |> Seq.toArray
        |> String
        |> escapeText

    let private hex (bytes: byte array) =
        bytes |> Array.map (fun value -> value.ToString("x2", CultureInfo.InvariantCulture)) |> String.concat ""

    let projectionIdentity (frame: RenderFrame) =
        let join segments = CanonicalEncoding.concatenate segments
        let int32Bytes value = CanonicalEncoding.int32LittleEndian value
        let tag value = CanonicalEncoding.byteValue value
        let int64Bytes (value: int64) =
            Array.init 8 (fun shift -> byte (value >>> (shift * 8)))
        let floatBytes value =
            value |> BitConverter.DoubleToInt64Bits |> int64Bytes
        let stringBytes (value: string) =
            let bytes = Encoding.UTF8.GetBytes value
            join [ int32Bytes (int32 bytes.Length); bytes ]
        let arrayBytes encode (values: 'value array) =
            seq {
                yield int32Bytes (int32 values.Length)
                for value in values do
                    yield encode value
            }
            |> join
        let disclosureBytes encode value =
            match value with
            | NotPresent -> tag 0uy
            | NotApplicable -> tag 1uy
            | ExplicitlyUnknown -> tag 2uy
            | Disclosed disclosed -> join [ tag 3uy; encode disclosed ]
        let factionBytes value =
            match value with
            | Human -> tag 0uy
            | Arcane -> tag 1uy
            | Neutral -> tag 2uy
            | OtherFaction identity -> join [ tag 3uy; stringBytes identity ]
        let healthBytes health =
            join
                [ int32Bytes (HealthVisual.remaining health)
                  int32Bytes (HealthVisual.maximum health) ]
        let secondaryBytes secondary =
            join
                [ tag (
                      match secondary.Source with
                      | WeaponHeading -> 0uy
                      | SensorHeading -> 1uy
                  )
                  floatBytes (HeadingRadians.value secondary.Radians) ]
        let unitBytes (unit: UnitVisual) =
            join
                [ int32Bytes unit.Id
                  int32Bytes unit.AnchorColumn
                  int32Bytes unit.AnchorRow
                  int32Bytes (CellExtent.value unit.FootprintWidth)
                  int32Bytes (CellExtent.value unit.FootprintDepth)
                  stringBytes (UnitClassId.value unit.ClassId)
                  factionBytes unit.Faction
                  disclosureBytes healthBytes unit.Health
                  disclosureBytes int32Bytes unit.Level
                  disclosureBytes stringBytes unit.StanceId
                  disclosureBytes
                      (HeadingRadians.value >> floatBytes)
                      unit.BodyHeading
                  disclosureBytes secondaryBytes unit.SecondaryHeading
                  disclosureBytes stringBytes unit.ShortLabel
                  unit.StatusIds |> Array.sort |> arrayBytes stringBytes ]
        let edgeBytes (edge: EdgeVisual) =
            join
                [ stringBytes edge.Id
                  stringBytes edge.Kind
                  stringBytes edge.State
                  int32Bytes edge.StartColumn
                  int32Bytes edge.StartRow
                  int32Bytes edge.EndColumn
                  int32Bytes edge.EndRow ]
        let overlayBytes (overlay: OverlayVisual) =
            join
                [ stringBytes overlay.Id
                  stringBytes overlay.Kind
                  (match overlay.Scope with
                   | SelectedUnitOverlay unitId ->
                       join [ tag 0uy; int32Bytes unitId ]
                   | WholeForceOverlay -> tag 1uy)
                  int32Bytes overlay.GeometryRevision
                  arrayBytes floatBytes overlay.Points
                  disclosureBytes stringBytes overlay.Label ]
        let eventBytes (event: RenderEventVisual) =
            join
                [ int32Bytes event.Id
                  int32Bytes event.Tick
                  stringBytes event.Kind
                  disclosureBytes int32Bytes event.SourceUnitId
                  disclosureBytes int32Bytes event.TargetUnitId
                  disclosureBytes stringBytes event.Summary ]

        join
            [ stringBytes "sir-render-projection-v1"
              int32Bytes frame.Tick
              int32Bytes frame.Board.MinimumColumn
              int32Bytes frame.Board.MinimumRow
              int32Bytes frame.Board.MaximumColumn
              int32Bytes frame.Board.MaximumRow
              frame.Units |> Array.sortBy _.Id |> arrayBytes unitBytes
              frame.Edges |> Array.sortBy _.Id |> arrayBytes edgeBytes
              frame.Overlays |> Array.sortBy _.Id |> arrayBytes overlayBytes
              frame.Events
              |> Array.sortBy (fun event -> event.Tick, event.Id)
              |> arrayBytes eventBytes
              tag (
                  match frame.Disclosure with
                  | FullReplayDisclosure -> 0uy
                  | PerspectiveDisclosure -> 1uy
                  | SandboxDisclosure -> 2uy
              ) ]
        |> CanonicalHash.sha256
        |> hex

    let private modeText mode =
        match mode with
        | VerifiedReplayEvidence -> "verified-replay-evidence"
        | PerspectiveEvidence -> "perspective-evidence"
        | DerivedSimulationEvidence -> "derived-simulation-not-verified"

    let private palette paletteIdentity =
        ReplayPalettes.all
        |> Array.tryFind (fun candidate -> candidate.Id = paletteIdentity)
        |> Option.defaultValue ReplayPalettes.accessibleDefault

    /// Generates a closed, presentation-only SVG. It never serializes DOM or replay markup.
    let svg
        (provenance: EvidenceProvenance)
        (annotation: string option)
        (frame: RenderFrame)
        =
        let palette = palette provenance.PaletteIdentity
        let provenance =
            { provenance with
                ProjectionIdentity = projectionIdentity frame
                Tick = frame.Tick
                PaletteIdentity = palette.Id
                RendererVersion = RendererVersion }
        let scene =
            Battlefield.scene
                frame
                { Battlefield.initial with
                    PaletteId = palette.Id
                    ExactTicks = true
                    ReducedMotion = true }
        let width = max 1.0 scene.Width
        let evidenceHeight = 86.0
        let height = max 1.0 scene.Height + evidenceHeight
        let builder = StringBuilder(4096)
        let append (text: string) = builder.Append(text) |> ignore
        let rect x y w h fill stroke =
            append (
                "<rect x=\"" + invariant x + "\" y=\"" + invariant y
                + "\" width=\"" + invariant w + "\" height=\"" + invariant h
                + "\" fill=\"" + fill + "\" stroke=\"" + stroke + "\"/>"
            )
        let line x1 y1 x2 y2 stroke width =
            append (
                "<line x1=\"" + invariant x1 + "\" y1=\"" + invariant y1
                + "\" x2=\"" + invariant x2 + "\" y2=\"" + invariant y2
                + "\" stroke=\"" + stroke + "\" stroke-width=\"" + invariant width + "\"/>"
            )

        append "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
        append (
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"" + invariant width
            + "\" height=\"" + invariant height + "\" viewBox=\"0 0 "
            + invariant width + " " + invariant height
            + "\" role=\"img\" aria-label=\"SIR evidence export\">"
        )
        append "<metadata>"
        append ("source=" + boundedText 256 provenance.SourceIdentity + "\n")
        append ("replay=" + boundedText 256 provenance.ReplayIdentity + "\n")
        append ("projection=" + boundedText 128 provenance.ProjectionIdentity + "\n")
        append ("engine=" + boundedText 128 provenance.EngineIdentity + "\n")
        append ("ruleset=" + boundedText 128 (provenance.RulesetIdentity |> Option.defaultValue "not-available") + "\n")
        append ("tick=" + string provenance.Tick + "\n")
        append ("mode=" + modeText provenance.Mode + "\n")
        append ("palette=" + boundedText 64 palette.Id + "\n")
        append ("renderer=" + boundedText 64 provenance.RendererVersion)
        append "</metadata>"
        rect 0.0 0.0 width scene.Height palette.Terrain palette.Grid
        let columns = int (frame.Board.MaximumColumn - frame.Board.MinimumColumn + 1)
        let rows = int (frame.Board.MaximumRow - frame.Board.MinimumRow + 1)
        for index in 0 .. columns do
            line (float index * scene.CellSize) 0.0 (float index * scene.CellSize) scene.Height palette.Grid 1.0
        for index in 0 .. rows do
            line 0.0 (float index * scene.CellSize) width (float index * scene.CellSize) palette.Grid 1.0
        for edge in scene.Edges |> Array.sortBy _.Id do
            line
                (float (edge.StartColumn - scene.Board.MinimumColumn) * scene.CellSize)
                (float (edge.StartRow - scene.Board.MinimumRow) * scene.CellSize)
                (float (edge.EndColumn - scene.Board.MinimumColumn) * scene.CellSize)
                (float (edge.EndRow - scene.Board.MinimumRow) * scene.CellSize)
                palette.Text
                3.0
        for unit in scene.Units |> Array.sortBy _.Unit.Id do
            let faction =
                match unit.Unit.Faction with
                | Human -> palette.HumanFaction
                | Arcane -> palette.ArcaneFaction
                | Neutral
                | OtherFaction _ -> palette.NeutralFaction
            rect unit.FootprintX unit.FootprintY unit.FootprintWidth unit.FootprintDepth "none" faction
            rect (unit.SymbolCenterX - 14.0) (unit.SymbolCenterY - 14.0) 28.0 28.0 palette.Canvas faction
            append (
                "<text x=\"" + invariant (unit.SymbolCenterX - 10.0)
                + "\" y=\"" + invariant (unit.SymbolCenterY + 4.0)
                + "\" fill=\"" + palette.Text
                + "\" font-family=\"sans-serif\" font-size=\"10\">"
                + boundedText 12 (string unit.Unit.Id) + "</text>"
            )
        rect 0.0 scene.Height width evidenceHeight palette.Canvas palette.Grid
        let title =
            match provenance.Mode with
            | DerivedSimulationEvidence -> "DERIVED SIMULATION — NOT VERIFIED REPLAY"
            | VerifiedReplayEvidence -> "VERIFIED REPLAY EVIDENCE"
            | PerspectiveEvidence -> "PERSPECTIVE EVIDENCE — HIDDEN STATE OMITTED"
        append (
            "<text x=\"8\" y=\"" + invariant (scene.Height + 20.0)
            + "\" fill=\"" + palette.Text
            + "\" font-family=\"sans-serif\" font-size=\"12\" font-weight=\"bold\">"
            + title + "</text>"
        )
        append (
            "<text x=\"8\" y=\"" + invariant (scene.Height + 40.0)
            + "\" fill=\"" + palette.Text
            + "\" font-family=\"monospace\" font-size=\"9\">tick "
            + string provenance.Tick + " · projection "
            + boundedText 20 provenance.ProjectionIdentity + " · palette "
            + boundedText 32 palette.Id + "</text>"
        )
        annotation
        |> Option.iter (fun value ->
            append (
                "<text x=\"8\" y=\"" + invariant (scene.Height + 60.0)
                + "\" fill=\"" + palette.Text
                + "\" font-family=\"sans-serif\" font-size=\"9\">"
                + boundedText 120 value + "</text>"
            ))
        append "</svg>\n"
        let content = builder.ToString()
        let sha = content |> Encoding.UTF8.GetBytes |> CanonicalHash.sha256 |> hex

        { FileName = "sir-evidence-tick-" + string provenance.Tick + ".svg"
          MediaType = "image/svg+xml;charset=utf-8"
          Svg = content
          Sha256 = sha
          Provenance = provenance }

    let forbiddenTokens =
        [ "<script"
          "onload="
          "onclick="
          "onerror="
          "<foreignObject"
          "href="
          "url("
          "http://"
          "https://"
          "data:"
          "<style"
          "<path"
          " id=" ]

    let isClosedSvg (content: string) =
        let contentWithoutNamespace =
            content.Replace(
                "xmlns=\"http://www.w3.org/2000/svg\"",
                "",
                StringComparison.Ordinal
            )
        forbiddenTokens
        |> List.forall (fun token ->
            contentWithoutNamespace.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0)
