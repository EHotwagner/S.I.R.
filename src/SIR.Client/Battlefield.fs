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
      PaletteId: string
      ExactTicks: bool
      ReducedMotion: bool }

type BattlefieldAction =
    | PanBy of x: float * y: float
    | ZoomBy of factor: float
    | SelectUnit of int32 option
    | FocusUnit of int32 option
    | FocusDirection of x: int * y: int
    | ChoosePalette of string
    | ChooseExactTicks of bool
    | ChooseReducedMotion of bool
    | ResetCamera

type OverlayDisposition =
    | ExactOverlay
    | SimplifiedSelectedOverlay of originalSegments: int
    | AggregatedWholeForceOverlay of originalSegments: int
    | DeclinedUnsafeOverlay of reason: string

type ProjectedOverlay =
    { Overlay: OverlayVisual
      Points: float array
      PathSegments: int
      Disposition: OverlayDisposition }

type ProjectedActionTrace =
    { EventId: int32
      Kind: string
      SourceX: float
      SourceY: float
      TargetX: float
      TargetY: float }

type TimelineLane =
    | AuthoritativeEvents
    | UnitActions
    | Communications

type TimelineItem =
    { EventId: int32
      Lane: TimelineLane
      Tick: int32
      Summary: Disclosure<string> }

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
      Overlays: ProjectedOverlay array
      ActionTraces: ProjectedActionTrace array
      Timeline: TimelineItem array
      WholeForceOverlaySegments: int
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
          PaletteId = ReplayPalettes.accessibleDefault.Id
          ExactTicks = false
          ReducedMotion = false }

    [<Literal>]
    let SelectedOverlaySegmentLimit = 2_000

    [<Literal>]
    let WholeForceOverlaySegmentLimit = 8_000

    [<Literal>]
    let OverlayCoordinateLimit = 100_000

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

    let private overlaySegments (points: float array) =
        if points.Length < 4 || points.Length % 2 <> 0 then 0
        else points.Length / 2 - 1

    let private validOverlayGeometry (points: float array) =
        points.Length >= 4
        && points.Length % 2 = 0
        && points.Length <= OverlayCoordinateLimit
        && points
           |> Array.forall (fun value ->
               not (Double.IsNaN value || Double.IsInfinity value)
               && abs value <= 1_000_000.0)

    let private simplifyTo maximumSegments (points: float array) =
        let vertices = points.Length / 2
        let wantedVertices = maximumSegments + 1
        if vertices <= wantedVertices then Array.copy points
        else
            Array.init (wantedVertices * 2) (fun coordinate ->
                let targetVertex = coordinate / 2
                let sourceVertex =
                    if targetVertex = wantedVertices - 1 then vertices - 1
                    else targetVertex * (vertices - 1) / (wantedVertices - 1)
                points[sourceVertex * 2 + coordinate % 2])

    let private boundingBox (points: float array) =
        let xs = Array.init (points.Length / 2) (fun index -> points[index * 2])
        let ys = Array.init (points.Length / 2) (fun index -> points[index * 2 + 1])
        let minimumX, maximumX = Array.min xs, Array.max xs
        let minimumY, maximumY = Array.min ys, Array.max ys
        [| minimumX; minimumY
           maximumX; minimumY
           maximumX; maximumY
           minimumX; maximumY
           minimumX; minimumY |]

    let private boundingBoxMany (pointSets: float array array) =
        let mutable minimumX = Double.PositiveInfinity
        let mutable minimumY = Double.PositiveInfinity
        let mutable maximumX = Double.NegativeInfinity
        let mutable maximumY = Double.NegativeInfinity
        for points in pointSets do
            for index in 0 .. points.Length / 2 - 1 do
                minimumX <- min minimumX points[index * 2]
                minimumY <- min minimumY points[index * 2 + 1]
                maximumX <- max maximumX points[index * 2]
                maximumY <- max maximumY points[index * 2 + 1]
        boundingBox
            [| minimumX; minimumY
               maximumX; maximumY |]

    let private projectOverlays selectedUnit (overlays: OverlayVisual array) =
        let eligible =
            overlays
            |> Array.filter (fun overlay ->
                match overlay.Scope with
                | SelectedUnitOverlay owner -> selectedUnit = Some owner
                | WholeForceOverlay -> true)

        let valid, invalid =
            eligible |> Array.partition (fun overlay -> validOverlayGeometry overlay.Points)

        let wholeForceSegments =
            valid
            |> Array.fold (fun total overlay ->
                let additional =
                    match overlay.Scope with
                    | WholeForceOverlay -> int64 (overlaySegments overlay.Points)
                    | SelectedUnitOverlay _ -> 0L
                min (int64 Int32.MaxValue) (total + additional)) 0L
            |> int

        let aggregateWholeForcePoints =
            if wholeForceSegments > WholeForceOverlaySegmentLimit then
                valid
                |> Array.choose (fun overlay ->
                    match overlay.Scope with
                    | WholeForceOverlay -> Some overlay.Points
                    | SelectedUnitOverlay _ -> None)
                |> boundingBoxMany
                |> Some
            else
                None

        let mutable emittedWholeForceAggregate = false

        let projectedValid =
            valid
            |> Array.map (fun overlay ->
                let segments = overlaySegments overlay.Points
                match overlay.Scope with
                | SelectedUnitOverlay _ when segments >= SelectedOverlaySegmentLimit ->
                    let points = simplifyTo SelectedOverlaySegmentLimit overlay.Points
                    { Overlay = overlay
                      Points = points
                      PathSegments = overlaySegments points
                      Disposition = SimplifiedSelectedOverlay segments }
                | SelectedUnitOverlay _ ->
                    { Overlay = overlay
                      Points = Array.copy overlay.Points
                      PathSegments = segments
                      Disposition = ExactOverlay }
                | WholeForceOverlay when wholeForceSegments > WholeForceOverlaySegmentLimit ->
                    if emittedWholeForceAggregate then
                        { Overlay = overlay
                          Points = [||]
                          PathSegments = 0
                          Disposition =
                            DeclinedUnsafeOverlay
                                "geometry represented by the combined whole-force aggregate" }
                    else
                        emittedWholeForceAggregate <- true
                        let points = Option.get aggregateWholeForcePoints
                        { Overlay = overlay
                          Points = points
                          PathSegments = overlaySegments points
                          Disposition =
                            AggregatedWholeForceOverlay wholeForceSegments }
                | WholeForceOverlay ->
                    { Overlay = overlay
                      Points = Array.copy overlay.Points
                      PathSegments = segments
                      Disposition = ExactOverlay })

        let projectedInvalid =
            invalid
            |> Array.map (fun overlay ->
                { Overlay = overlay
                  Points = [||]
                  PathSegments = 0
                  Disposition =
                    DeclinedUnsafeOverlay
                        "geometry must contain bounded finite coordinate pairs" })

        Array.append projectedValid projectedInvalid, wholeForceSegments

    let private actionTraces
        (units: ProjectedUnit array)
        (events: RenderEventVisual array)
        =
        let centers =
            units
            |> Array.map (fun unit ->
                unit.Unit.Id, (unit.SymbolCenterX, unit.SymbolCenterY))
            |> Map.ofArray

        events
        |> Array.choose (fun event ->
            match event.SourceUnitId, event.TargetUnitId with
            | Disclosed source, Disclosed target ->
                match Map.tryFind source centers, Map.tryFind target centers with
                | Some(sourceX, sourceY), Some(targetX, targetY) ->
                    Some
                        { EventId = event.Id
                          Kind = event.Kind
                          SourceX = sourceX
                          SourceY = sourceY
                          TargetX = targetX
                          TargetY = targetY }
                | _ -> None
            | _ -> None)

    let private timeline (events: RenderEventVisual array) =
        events
        |> Array.map (fun event ->
            let lane =
                match event.Kind with
                | "communication"
                | "acknowledgement" -> Communications
                | "move"
                | "attack"
                | "heal" -> UnitActions
                | _ -> AuthoritativeEvents
            { EventId = event.Id
              Lane = lane
              Tick = event.Tick
              Summary = event.Summary })

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
        let overlays, wholeForceSegments =
            projectOverlays state.SelectedUnit frame.Overlays
        let columns = frame.Board.MaximumColumn - frame.Board.MinimumColumn + 1
        let rows = frame.Board.MaximumRow - frame.Board.MinimumRow + 1

        { Tick = frame.Tick
          Width = float columns * CellSize
          Height = float rows * CellSize
          CellSize = CellSize
          Board = frame.Board
          Units = units
          Edges = frame.Edges
          Overlays = overlays
          ActionTraces = actionTraces units frame.Events
          Timeline = timeline frame.Events
          WholeForceOverlaySegments = wholeForceSegments
          Disclosure = frame.Disclosure
          Palette = palette state.PaletteId
          Camera = state.Camera
          SemanticZoom = tier
          SelectedUnit = state.SelectedUnit
          FocusedUnit = state.FocusedUnit
          InteractiveNodeEstimate =
            nodeEstimate tier units frame.Edges.Length
            + (overlays |> Array.sumBy (fun overlay -> max 1 overlay.PathSegments))
            + frame.Events.Length }

    /// Deterministic presentation-only translation. Non-position facts stay
    /// on the earlier committed frame until alpha one. Spawn, disappearance,
    /// footprint change, level change, or a move longer than one adjacent cell
    /// is a discontinuity and is never interpolated.
    let interpolatedScene
        alpha
        (previous: RenderFrame)
        (next: RenderFrame)
        (state: BattlefieldViewState)
        =
        let alpha = clamp 0.0 1.0 alpha
        let previousUnitIds =
            previous.Units |> Array.map _.Id |> Set.ofArray
        let nextUnitIds =
            next.Units |> Array.map _.Id |> Set.ofArray
        let sameUnitSet = previousUnitIds = nextUnitIds
        if alpha >= 1.0 || previous.Tick = next.Tick || not sameUnitSet then
            scene next state
        else
            let blockedMove (fromUnit: UnitVisual) (toUnit: UnitVisual) =
                let blocked (edge: EdgeVisual) =
                    match edge.Kind, edge.State with
                    | "door", "open" -> false
                    | "wall", _
                    | "door", _
                    | "fence", "closed" -> true
                    | _ -> false

                let crosses (edge: EdgeVisual) =
                    let fromColumn = fromUnit.AnchorColumn
                    let fromRow = fromUnit.AnchorRow
                    let toColumn = toUnit.AnchorColumn
                    let toRow = toUnit.AnchorRow
                    if fromRow = toRow && abs (toColumn - fromColumn) = 1 then
                        let boundaryColumn = max fromColumn toColumn
                        edge.StartColumn = boundaryColumn
                        && edge.EndColumn = boundaryColumn
                        && min edge.StartRow edge.EndRow <= fromRow
                        && max edge.StartRow edge.EndRow >= fromRow + 1
                    elif fromColumn = toColumn && abs (toRow - fromRow) = 1 then
                        let boundaryRow = max fromRow toRow
                        edge.StartRow = boundaryRow
                        && edge.EndRow = boundaryRow
                        && min edge.StartColumn edge.EndColumn <= fromColumn
                        && max edge.StartColumn edge.EndColumn >= fromColumn + 1
                    else
                        // Diagonal movement is not interpolated because an
                        // unambiguous semantic-edge traversal is unavailable.
                        fromColumn <> toColumn && fromRow <> toRow

                Array.append previous.Edges next.Edges
                |> Array.exists (fun edge -> blocked edge && crosses edge)

            let earlier = scene previous state
            let later = scene next state
            let laterById =
                later.Units |> Array.map (fun unit -> unit.Unit.Id, unit) |> Map.ofArray
            let nextRaw =
                next.Units |> Array.map (fun unit -> unit.Id, unit) |> Map.ofArray
            let units =
                earlier.Units
                |> Array.map (fun unit ->
                    match Map.tryFind unit.Unit.Id laterById, Map.tryFind unit.Unit.Id nextRaw with
                    | Some target, Some targetRaw
                        when unit.Unit.Level = targetRaw.Level
                             && unit.Unit.FootprintWidth = targetRaw.FootprintWidth
                             && unit.Unit.FootprintDepth = targetRaw.FootprintDepth
                             && abs (targetRaw.AnchorColumn - unit.Unit.AnchorColumn)
                                + abs (targetRaw.AnchorRow - unit.Unit.AnchorRow)
                                <= 1
                             && not (blockedMove unit.Unit targetRaw) ->
                        let lerp start finish = start + (finish - start) * alpha
                        let footprintX = lerp unit.FootprintX target.FootprintX
                        let footprintY = lerp unit.FootprintY target.FootprintY
                        { unit with
                            FootprintX = footprintX
                            FootprintY = footprintY
                            SymbolCenterX =
                                footprintX + unit.FootprintWidth / 2.0
                            SymbolCenterY =
                                footprintY + unit.FootprintDepth / 2.0 }
                    | _ -> unit)
            { earlier with
                Units = units
                ActionTraces = actionTraces units previous.Events }

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
        | ChooseExactTicks value -> { state with ExactTicks = value }
        | ChooseReducedMotion value -> { state with ReducedMotion = value }
        | ResetCamera ->
            { initial with
                PaletteId = state.PaletteId
                ExactTicks = state.ExactTicks
                ReducedMotion = state.ReducedMotion }

    /// Removes interaction state for entities that are no longer disclosed.
    let reconcile (frame: RenderFrame) (state: BattlefieldViewState) =
        let disclosed = frame.Units |> Array.map _.Id |> Set.ofArray
        let keep id = id |> Option.filter (fun value -> Set.contains value disclosed)

        { state with
            SelectedUnit = keep state.SelectedUnit
            FocusedUnit = keep state.FocusedUnit }

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
        baseSize
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
          FootprintWidth = extent baseSize
          FootprintDepth = extent baseSize
          ClassId = UnitClassId.resolve classId
          Faction = faction
          Health = Disclosed(health remaining)
          Level = Disclosed level
          StanceId = stance |> Option.map Disclosed |> Option.defaultValue NotApplicable
          BodyHeading = Disclosed(heading headingRadians)
          SecondaryHeading = NotPresent
          ShortLabel = Disclosed label
          StatusIds = [||] }

    let private withSecondary source radians unit =
        { unit with
            SecondaryHeading =
                Disclosed
                    { Radians = heading radians
                      Source = source } }

    /// A committed eight-by-eight documentation frame. It is never interpolated.
    let representativeFrame =
        { Tick = 24
          Board =
            { MinimumColumn = 0
              MinimumRow = 0
              MaximumColumn = 7
              MaximumRow = 7 }
          Units =
            [| sampleUnit 1 0 0 2 "rifleman" Human 12 0 (Some "standing") 0.0 "Bravo 6"
               |> withSecondary WeaponHeading (Math.PI / 4.0)
               sampleUnit 2 3 0 2 "medic" Human 9 2 (Some "kneeling") (Math.PI / 4.0) "Mercy"
               |> withSecondary SensorHeading (Math.PI * 1.25)
               sampleUnit 3 0 3 2 "gunner" Human 6 4 (Some "prone") (Math.PI * 1.5) "Anvil"
               sampleUnit 4 6 2 1 "observation-drone" Neutral 11 1 None Math.PI "Kite"
               sampleUnit 5 6 6 1 "goblin" Arcane 8 0 (Some "crouched") Math.PI "Needle"
               sampleUnit 6 3 5 2 "troll" Arcane 3 7 (Some "braced") (Math.PI * 1.25) "Stone" |]
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
          Overlays =
            [| { Id = "selected-los-1"
                 Kind = "line-of-sight"
                 Scope = SelectedUnitOverlay 1
                 GeometryRevision = 1
                 Points =
                    [| 48.0; 48.0
                       120.0; 72.0
                       216.0; 168.0
                       312.0; 312.0 |]
                 Label = Disclosed "Exact selected line of sight" }
               { Id = "whole-command-1"
                 Kind = "command"
                 Scope = WholeForceOverlay
                 GeometryRevision = 1
                 Points =
                    [| 12.0; 12.0
                       372.0; 12.0
                       372.0; 180.0
                       12.0; 180.0
                       12.0; 12.0 |]
                 Label = Disclosed "Whole-force command area" } |]
          Events =
            [| { Id = 2401
                 Tick = 24
                 Kind = "attack"
                 SourceUnitId = Disclosed 1
                 TargetUnitId = Disclosed 5
                 Summary = Disclosed "Bravo 6 attacks Needle" }
               { Id = 2402
                 Tick = 24
                 Kind = "communication"
                 SourceUnitId = Disclosed 2
                 TargetUnitId = Disclosed 1
                 Summary = Disclosed "Mercy acknowledges Bravo 6" } |]
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
