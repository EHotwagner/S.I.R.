module SIR.Client.Web.TacticalScenePresentation
open System
open Browser.Dom
open Browser.Types
open Elmish
open Elmish.React
open Fable.Core
open Fable.Core.JsInterop
open Feliz
open SIR.Client
open SIR.Domain
open SIR.Protocol.Http
open SIR.Protocol.Realtime
open SIR.Client.Web.BrowserInfrastructure
open SIR.Client.Web.AppTypes
open SIR.Client.Web.AppShell
open SIR.Client.Web.ClientFeatureRuntime
open SIR.Client.Web.CommandRegistry
open SIR.Client.Web.ModeAdapters
open SIR.Client.Web.TacticalOverlayView
open SIR.Client.Web.SceneAdapters
open SIR.Client.Web.PanelViews
open SIR.Client.Web.TacticalSharedControls

/// The retained tactical scene: viewport chunk facts, semantic-zoom tiers, per-layer
/// revision ownership, the persistent SVG, and the frame-coalesced presentation owner.
/// Extracted from App as a typed boundary; App keeps the shell and the Elmish program.

let editorLayerDisplay domain state =
    if MapEditor.layerState domain state = HiddenLayer then "none" else "inline"

let editorLayerOpacity domain state =
    if MapEditor.layerState domain state = DimmedLayer then "0.28" else "1"


[<Emit("$0.target.closest('[data-unit-id]')?.getAttribute('data-unit-id') ?? null")>]
let pointerEditorUnitId (event: Browser.Types.Event) : string = jsNative

let editorPointerKind (value: string) =
    match value with
    | "touch" -> TouchPointer
    | "pen" -> PenPointer
    | _ -> MousePointer

[<Emit("Number.isFinite($0.viewBox?.baseVal?.width) && $0.viewBox.baseVal.width > 0 ? $0.viewBox.baseVal.width : $1")>]
let currentSvgViewportWidth (_target: EventTarget) (_fallback: float) : float = jsNative

[<Emit("Number.isFinite($0.viewBox?.baseVal?.height) && $0.viewBox.baseVal.height > 0 ? $0.viewBox.baseVal.height : $1")>]
let currentSvgViewportHeight (_target: EventTarget) (_fallback: float) : float = jsNative

let editorScreenPoint
    (view: EditorWorkspaceState)
    (target: EventTarget)
    clientX
    clientY
    =
    let element: Element = unbox target
    let bounds = element.getBoundingClientRect ()
    let width = max 1.0 bounds.width
    let height = max 1.0 bounds.height
    MapEditorWorkspace.clientToViewportPoint
        (currentSvgViewportWidth target view.ViewportWidth)
        (currentSvgViewportHeight target view.ViewportHeight)
        width
        height
        (clientX - bounds.left)
        (clientY - bounds.top)


let sceneDisclosureText = function
    | Disclosed value -> value
    | ExplicitlyUnknown -> "Unknown"
    | NotApplicable -> "Not applicable"
    | NotPresent -> ""

let sharedSceneClaimsKeyboard
    model
    key
    controlOrMeta
    shift
    alt
    =
    let produced =
        [ if controlOrMeta then "Ctrl"
          if alt then "Alt"
          if shift && key <> "?" then "Shift"
          if key = " " then "Space"
          elif key.Length = 1 then key.ToUpperInvariant()
          else key ]
        |> String.concat "+"
        |> _.ToUpperInvariant()
    activeTacticalRegistry model
    |> List.exists (fun command ->
        Set.contains model.Tactical.Modality command.Modalities
        && tacticalCommandAvailable model command
        && (UnifiedTacticalWorkspace.effectiveGesture
                model.TacticalBindings
                command
            |> Option.exists (fun gesture ->
                ClientModuleBoundaries.canonicalGesture gesture = produced)))

let viewportChunkFacts (camera: BattlefieldCamera) viewportWidth viewportHeight =
    let chunkCells = 8.0
    let overscanCells = 2.0
    let finite fallback value =
        if Double.IsNaN value || Double.IsInfinity value then fallback else value
    let zoom = max 0.000001 (finite 1.0 camera.Zoom)
    let width = max 1.0 (finite MapEditorWorkspace.DefaultViewportWidth viewportWidth)
    let height = max 1.0 (finite MapEditorWorkspace.DefaultViewportHeight viewportHeight)
    let minimumX = (0.0 - finite 0.0 camera.PanX) / zoom / Battlefield.CellSize
    let minimumY = (0.0 - finite 0.0 camera.PanY) / zoom / Battlefield.CellSize
    let maximumX = (width - finite 0.0 camera.PanX) / zoom / Battlefield.CellSize
    let maximumY = (height - finite 0.0 camera.PanY) / zoom / Battlefield.CellSize
    let chunk value = int (Math.Floor(value / chunkCells))
    let firstColumn, lastColumn = chunk (minimumX - overscanCells), chunk (maximumX + overscanCells)
    let firstRow, lastRow = chunk (minimumY - overscanCells), chunk (maximumY + overscanCells)
    let queried = max 0 (lastColumn - firstColumn + 1) * max 0 (lastRow - firstRow + 1)
    (minimumX, minimumY, maximumX, maximumY), queried

let semanticTier (camera: BattlefieldCamera) =
    let projectedCell =
        if Double.IsNaN camera.Zoom || Double.IsInfinity camera.Zoom then 64.0
        else Battlefield.CellSize * max 0.0 camera.Zoom
    if projectedCell < 20.0 then "overview"
    elif projectedCell < 48.0 then "tactical"
    else "detail"

type TacticalSceneRevisions =
    { AcceptedScene: string
      TerrainContent: string
      EdgesContent: string
      UnitsContent: string
      Terrain: string
      Edges: string
      Units: string
      Routes: string
      Annotations: string
      Effects: string
      Overlays: string
      Accessibility: string
      Interaction: string
      Camera: string }

type TacticalSceneOwnerProps =
    { Model: Model
      Projection: SharedSceneProjection option
      PresentationAlpha: float
      Revisions: TacticalSceneRevisions
      Dispatch: Msg -> unit }

let mutable latestTacticalModel: Model option = None
let mutable presentationGestureCameras: Map<int32, BattlefieldCamera> = Map.empty
let mutable pendingUnitMovePointers: Set<int32> = Set.empty
let mutable cachedTacticalTerrainLayer: (string * ReactElement) option = None
let mutable tacticalTerrainLayerConstructionCount = 0
let mutable cachedTacticalEdgesLayer: (string * ReactElement) option = None
let mutable tacticalEdgesLayerConstructionCount = 0
let mutable cachedTacticalUnitGlyphs: (string * Map<string, ReactElement>) option = None
let mutable tacticalUnitGlyphConstructionCount = 0

let retainTacticalTerrainLayer revision build =
    match cachedTacticalTerrainLayer with
    | Some(cachedRevision, element) when cachedRevision = revision -> element
    | _ ->
        tacticalTerrainLayerConstructionCount <- tacticalTerrainLayerConstructionCount + 1
        let element = build tacticalTerrainLayerConstructionCount
        cachedTacticalTerrainLayer <- Some(revision, element)
        element

let retainTacticalEdgesLayer revision build =
    match cachedTacticalEdgesLayer with
    | Some(cachedRevision, element) when cachedRevision = revision -> element
    | _ ->
        tacticalEdgesLayerConstructionCount <- tacticalEdgesLayerConstructionCount + 1
        let element = build tacticalEdgesLayerConstructionCount
        cachedTacticalEdgesLayer <- Some(revision, element)
        element

let retainTacticalUnitGlyphs revision build =
    match cachedTacticalUnitGlyphs with
    | Some(cachedRevision, glyphs) when cachedRevision = revision -> glyphs
    | _ ->
        tacticalUnitGlyphConstructionCount <- tacticalUnitGlyphConstructionCount + 1
        let glyphs = build ()
        cachedTacticalUnitGlyphs <- Some(revision, glyphs)
        glyphs

let mutable editorMigratedLayersConstructionToken: obj = box ()
let mutable editorMigratedLayersConstructionCount = 0

let persistentSceneSvg
    (model: Model)
    (projection: SharedSceneProjection option)
    (presentationAlpha: float)
    (revisions: TacticalSceneRevisions)
    (presentationScheduler: PresentationFrameScheduler<BattlefieldCamera>)
    dispatch
    =
    let cellSize = Battlefield.CellSize
    let board =
        projection
        |> Option.map _.Board
        |> Option.defaultValue
            { MinimumColumn = 0
              MinimumRow = 0
              MaximumColumn = 0
              MaximumRow = 0 }
    let boardWidth =
        float (max 1 (board.MaximumColumn - board.MinimumColumn + 1))
        * cellSize
    let boardHeight =
        float (max 1 (board.MaximumRow - board.MinimumRow + 1))
        * cellSize
    // The retained workscreen owns one spatial camera even while a modality
    // projection is temporarily unavailable (for example before Simulate or
    // Review has accepted input).
    let camera = model.EditorView.Camera
    let viewportBounds, queriedChunks =
        viewportChunkFacts camera model.EditorView.ViewportWidth model.EditorView.ViewportHeight
    let viewportMinimumX, viewportMinimumY, viewportMaximumX, viewportMaximumY = viewportBounds
    let viewportIntersects minimumX minimumY maximumX maximumY =
        minimumX < viewportMaximumX
        && maximumX > viewportMinimumX
        && minimumY < viewportMaximumY
        && maximumY > viewportMinimumY
    let regionIntersectsViewport (region: MapRegion) =
        match region.Geometry with
        | RegionRectangle(column, row, width, height) ->
            viewportIntersects
                (float column)
                (float row)
                (float (column + width))
                (float (row + height))
        | RegionPolygon vertices when not (Array.isEmpty vertices) ->
            let columns = vertices |> Array.map (fun vertex -> float vertex.CellColumn)
            let rows = vertices |> Array.map (fun vertex -> float vertex.CellRow)
            viewportIntersects (Array.min columns) (Array.min rows) (Array.max columns) (Array.max rows)
        | RegionPolygon _ -> false
    let tier = semanticTier camera
    let unitCount = projection |> Option.map _.Units.Length |> Option.defaultValue 0
    let visualSystem =
        TacticalSceneProjection.visualSystem
            model.Battlefield.PaletteId
            model.Battlefield.ReducedMotion
            unitCount
    let densityToken = tacticalDensityToken visualSystem.Density
    let tacticalOverlays =
        projection
        |> Option.map (TacticalSceneProjection.projectOverlays model.TacticalOverlays model.HeldTacticalOverlays)
        |> Option.defaultValue
            { Payloads = [||]
              Labels = [||]
              Cost =
                { RegistryTraversals = 1
                  DisclosurePasses = 1
                  CandidatePayloads = 0
                  EmittedPayloads = 0
                  EmittedLabels = 0
                  EstimatedSvgNodes = 0 } }
    let selectedPrimitiveIds =
        projection
        |> Option.map (fun scene ->
            [ yield!
                  scene.Selection.SelectedPrimitiveIds
                  |> Array.map ScenePrimitiveId.value
              match scene.Selection.FocusedUnit with
              | Some unitId -> yield "unit:" + string unitId
              | None -> () ]
            |> Set.ofList)
        |> Option.defaultValue Set.empty
    let layerVisible kind =
        projection
        |> Option.bind (fun scene ->
            scene.Layers
            |> Array.tryFind (fun layer -> layer.Kind = kind))
        |> Option.map _.Visible
        |> Option.defaultValue true
    let editorLayerVisible domain =
        model.Workspace <> EditorWorkspace
        || MapEditor.layerState domain model.Editor <> HiddenLayer
    let editorLayerOpacityValue domain =
        if
            model.Workspace = EditorWorkspace
            && MapEditor.layerState domain model.Editor = DimmedLayer
        then 0.28
        else 1.0
    let editorLayerDisplayValue domain sharedKind =
        if layerVisible sharedKind && editorLayerVisible domain then "inline" else "none"
    let owner =
        projection
        |> Option.map (fun scene -> string scene.Owner)
        |> Option.defaultValue "Unavailable"
    let availableCommandIds =
        activeTacticalRegistry model
        |> List.filter (fun command ->
            Set.contains model.Tactical.Modality command.Modalities
            && tacticalCommandAvailable model command)
        |> List.map _.Id
        |> Set.ofList
    let commandAvailable commandId =
        Set.contains commandId availableCommandIds
    let invoke commandId =
        if commandAvailable commandId then
            dispatch (InvokeTacticalCommand commandId)
    let selected primitiveId =
        Set.contains (ScenePrimitiveId.value primitiveId) selectedPrimitiveIds
    let visibleUnits =
        projection
        |> Option.map (fun scene ->
            scene.Units
            |> Array.filter (fun unit ->
                viewportIntersects
                    unit.PresentationColumn
                    unit.PresentationRow
                    (unit.PresentationColumn + float (CellExtent.value unit.Visual.FootprintWidth))
                    (unit.PresentationRow + float (CellExtent.value unit.Visual.FootprintDepth))))
        |> Option.defaultValue Array.empty
    let candidatePrimitiveCount =
        projection
        |> Option.map (fun scene ->
            scene.Terrain.Length
            + scene.Edges.Length
            + scene.Units.Length
            + scene.Routes.Length
            + scene.Annotations.Length
            + scene.Effects.Length)
        |> Option.defaultValue 0
    let candidateUnitCount =
        projection |> Option.map (fun scene -> scene.Units.Length) |> Option.defaultValue 0
    let emittedPrimitiveCount =
        candidatePrimitiveCount - candidateUnitCount + visibleUnits.Length
    let globalPrimitiveCount =
        if model.Workspace = ReplayWorkspace then emittedPrimitiveCount
        else
            int model.Editor.Map.Width * int model.Editor.Map.Height
            + model.Editor.Map.Edges.Count
            + model.Editor.Map.Units.Count
            + model.Editor.Map.Regions.Count
    let accessibleSelection =
        projection
        |> Option.bind (fun scene -> scene.Selection.FocusedUnit |> Option.orElse (scene.Selection.SelectedUnits |> Array.tryHead))
        |> Option.map (fun unitId ->
            match Map.tryFind unitId model.Editor.Map.Units with
            | Some unit ->
                "Selected unit " + string unit.Id
                + ", class " + unit.ClassId
                + ", faction " + string unit.Side
                + ", footprint " + string unit.Size + " by " + string unit.Size
                + ", cell " + string unit.Column + "," + string unit.Row + "."
            | None -> "Selected disclosed unit " + string unitId + "; complete facts remain available in the roster and inspector.")
        |> Option.defaultValue "No tactical unit selected."
    let editorSelectionAt screenX screenY action =
        MapEditorWorkspace.tryHitCell
            model.Editor.Map.Width
            model.Editor.Map.Height
            model.EditorView.Camera
            screenX
            screenY
        |> Option.iter (fun hit ->
            { CellColumn = hit.Column; CellRow = hit.Row }
            |> action
            |> EditorChanged
            |> dispatch)
    let editorTerrainAt screenX screenY action =
        editorSelectionAt screenX screenY action
    let terrainInteractionAvailable =
        model.Workspace = EditorWorkspace
        || (model.Workspace = PlanningWorkspace
            && model.Planning |> Option.bind _.SelectedUnit |> Option.isSome)
    let invokeCurrentCell column row =
        latestTacticalModel
        |> Option.iter (fun current ->
            sharedSceneCellCommand current column row
            |> Option.iter (fun commandId ->
                let available =
                    activeTacticalRegistry current
                    |> List.exists (fun command ->
                        command.Id = commandId
                        && Set.contains current.Tactical.Modality command.Modalities
                        && tacticalCommandAvailable current command)
                if available then dispatch (InvokeTacticalCommand commandId)))
    let invokeCurrentUnit shift unitId =
        latestTacticalModel
        |> Option.iter (fun current ->
            if current.Workspace = EditorWorkspace && shift then
                dispatch (EditorChanged(ToggleEditorUnitSelection unitId))
            else
                sharedSceneUnitCommand current unitId
                |> Option.iter (fun commandId ->
                    let available =
                        activeTacticalRegistry current
                        |> List.exists (fun command ->
                            command.Id = commandId
                            && Set.contains current.Tactical.Modality command.Modalities
                            && tacticalCommandAvailable current command)
                    if available then dispatch (InvokeTacticalCommand commandId)))

    Svg.svg [
        svg.id "persistent-tactical-svg"
        svg.custom ("data-work-surface-root", "persistent-svg")
        svg.custom ("data-render-contract", "shared-scene-projection-v1")
        svg.custom ("data-visual-system", visualSystem.Identity)
        svg.custom ("data-visual-density", densityToken)
        svg.custom ("data-motion", if visualSystem.ReducedMotion then "reduced" else "full")
        svg.custom ("data-effect-count", projection |> Option.map _.Effects.Length |> Option.defaultValue 0 |> string)
        svg.custom ("data-effect-limit", string visualSystem.MaximumActiveEffects)
        svg.custom ("data-visual-unit-count", string visibleUnits.Length)
        // The visible working set and the accepted scene's total are now
        // different numbers, and a reader that can only see the first cannot
        // tell "culled correctly" from "lost units".  Report both.
        svg.custom ("data-visual-global-unit-count", string candidateUnitCount)
        svg.custom ("data-visual-node-estimate", projection |> Option.map _.VisualCost.EstimatedSvgNodes |> Option.defaultValue 0 |> string)
        svg.custom ("data-layer-order", String.concat ">" visualSystem.LayerOrder)
        unbox<ISvgAttribute> (prop.style [
            style.custom ("--sir-canvas", visualSystem.Palette.Canvas)
            style.custom ("--sir-text", visualSystem.Palette.Text)
            style.custom ("--sir-grid", visualSystem.Palette.Grid)
            style.custom ("--sir-focus", visualSystem.Palette.Focus)
            style.custom ("--sir-intent", visualSystem.Intent)
            style.custom ("--sir-impact", visualSystem.Impact)
            style.custom ("--sir-suppression", visualSystem.Suppression)
            style.custom ("--sir-recovery", visualSystem.Recovery)
            style.custom ("--sir-rejected", visualSystem.Rejected)
            style.custom ("--sir-motion-ms", string visualSystem.TransitionMilliseconds + "ms")
            style.custom ("--sir-effect-ms", string visualSystem.EffectMilliseconds + "ms")
        ])
        svg.custom ("data-scene-owner", owner)
        svg.custom ("data-editor-projection-constructions", string editorSceneProjectionConstructionCount)
        svg.custom (
            "data-scene-disclosure",
            projection
            |> Option.map (fun scene -> string scene.Disclosure.Source)
            |> Option.defaultValue "unavailable"
        )
        svg.custom (
            "data-scene-revision",
            projection
            |> Option.map _.RevisionIdentity
            |> Option.defaultValue "unavailable"
        )
        svg.custom ("data-accepted-scene-revision", revisions.AcceptedScene)
        svg.custom ("data-layer-revision-terrain", revisions.Terrain)
        svg.custom ("data-layer-revision-edges", revisions.Edges)
        svg.custom ("data-layer-revision-units", revisions.Units)
        svg.custom ("data-layer-revision-routes", revisions.Routes)
        svg.custom ("data-layer-revision-annotations", revisions.Annotations)
        svg.custom ("data-layer-revision-effects", revisions.Effects)
        svg.custom ("data-layer-revision-overlays", revisions.Overlays)
        svg.custom ("data-layer-revision-accessibility", revisions.Accessibility)
        svg.custom ("data-scene-tick", projection |> Option.map _.Tick |> Option.defaultValue 0 |> string)
        svg.custom ("data-live-status", model.Live.Status)
        svg.custom ("data-live-tick", model.Live.Snapshot |> Option.map _.Tick |> Option.defaultValue 0 |> string)
        svg.custom ("data-live-server-sequence", model.Live.Snapshot |> Option.map _.ServerSequence |> Option.defaultValue 0 |> string)
        svg.custom ("data-live-projection-revision", model.Live.Snapshot |> Option.map _.ProjectionRevision |> Option.defaultValue 0 |> string)
        svg.custom ("data-presentation-alpha", string presentationAlpha)
        svg.custom ("data-presentation-frame-counters", presentationFrameCounters presentationScheduler)
        svg.custom ("data-camera-pan-x", string camera.PanX)
        svg.custom ("data-camera-pan-y", string camera.PanY)
        svg.custom ("data-camera-zoom", string camera.Zoom)
        svg.custom ("data-viewport-chunk-cells", "8")
        svg.custom ("data-viewport-overscan-cells", "2")
        svg.custom ("data-viewport-queried-chunks", string queriedChunks)
        svg.custom ("data-viewport-candidate-primitives", string candidatePrimitiveCount)
        svg.custom ("data-viewport-emitted-primitives", string emittedPrimitiveCount)
        svg.custom ("data-viewport-global-primitives", string globalPrimitiveCount)
        svg.custom ("data-semantic-tier", tier)
        svg.custom ("data-overlay-payload-count", string tacticalOverlays.Cost.EmittedPayloads); svg.custom ("data-overlay-label-count", string tacticalOverlays.Cost.EmittedLabels)
        svg.custom ("data-overlay-node-estimate", string tacticalOverlays.Cost.EstimatedSvgNodes)
        svg.custom ("data-overlay-registry-traversals", string tacticalOverlays.Cost.RegistryTraversals); svg.custom ("data-overlay-disclosure-passes", string tacticalOverlays.Cost.DisclosurePasses)
        svg.custom ("data-overlay-preferences", TacticalSceneProjection.exportOverlayPreferences model.TacticalOverlays)
        svg.custom (
            "data-semantic-selection-unit",
            projection
            |> Option.bind _.Selection.FocusedUnit
            |> Option.map string
            |> Option.defaultValue ""
        )
        svg.custom ("data-keyboard-intent-boundary", "tactical-command-registry")
        svg.custom ("role", "application")
        svg.custom (
            "aria-label",
            "Persistent shared tactical SVG work surface for " + owner
        )
        svg.tabIndex 0
        svg.onContextMenu (fun event -> event.preventDefault ())
        svg.onKeyDown (fun event ->
            let controlOrMeta = event.ctrlKey || event.metaKey
            if
                sharedSceneClaimsKeyboard
                    model
                    event.key
                    controlOrMeta
                    event.shiftKey
                    event.altKey
            then
                event.preventDefault ()
            event.stopPropagation ()
            dispatch (
                KeyPressed(
                    event.key,
                    controlOrMeta,
                    event.shiftKey,
                    event.altKey,
                    event.repeat
                )
            ))
        svg.onKeyUp (fun event ->
            event.stopPropagation ()
            dispatch (KeyReleased event.key))
        svg.onWheel (fun event ->
            let eventModel = latestTacticalModel |> Option.defaultValue model
            let x, y =
                editorScreenPoint
                    eventModel.EditorView
                    event.currentTarget
                    event.clientX
                    event.clientY
            let factor = if event.deltaY < 0.0 then 1.12 else 1.0 / 1.12
            dispatch (EditorWorkspaceChanged(ZoomEditorAt(x, y, factor))))
        svg.onPointerDown (fun event ->
            let eventModel = latestTacticalModel |> Option.defaultValue model
            let editorActive = eventModel.Workspace = EditorWorkspace
            let kind = editorPointerKind event.pointerType
            let terrainToolActive =
                editorActive
                &&
                (
                    match eventModel.Editor.Tool with
                    | Terrain _ -> true
                    | _ -> false)
            let requestsPan =
                event.button = 1
                || event.button = 2
                || (kind = TouchPointer
                    && (not editorActive
                        || not terrainToolActive
                        || not eventModel.EditorView.CapturedPointers.IsEmpty))
                || (editorActive && editorPanHeld eventModel)
            let requestsSelection =
                editorActive
                && eventModel.Editor.Tool = Select
                && kind <> TouchPointer
                && event.button = 0
                && not requestsPan
            let movementUnit =
                if requestsSelection then
                    match Int32.TryParse(pointerEditorUnitId event) with
                    | true, unitId -> Some unitId
                    | _ -> None
                else None
            let requestsTerrain =
                editorActive
                &&
                (match eventModel.Editor.Tool with
                 | Terrain _ -> event.button = 0 && not requestsPan
                 | _ -> false)
            if requestsPan || requestsSelection || requestsTerrain then
                event.preventDefault ()
                capturePointer event.currentTarget (int event.pointerId)
                if requestsPan && kind <> TouchPointer then
                    // The retained handler may run before Elmish has drained
                    // StartEditorPointer. Seed the presentation gesture from
                    // the authoritative model visible at pointer-down so the
                    // first coalesced move cannot fall back to module defaults.
                    let gestureCamera =
                        currentTacticalCamera eventModel.EditorView.Camera
                    presentationGestureCameras <-
                        Map.add
                            (int32 event.pointerId)
                            gestureCamera
                            presentationGestureCameras
                    // Mark the already-visible camera as accepted before the
                    // Start action reconciles it into Elmish state. This is a
                    // no-op transform and lets the root cache avoid rebuilding
                    // merely to acknowledge the presentation baseline.
                    presentTacticalCamera gestureCamera.PanX gestureCamera.PanY gestureCamera.Zoom
                let x, y =
                    editorScreenPoint eventModel.EditorView event.currentTarget event.clientX event.clientY
                dispatch (
                    EditorWorkspaceChanged(
                        StartEditorPointer
                            { Id = int32 event.pointerId
                              Kind = kind
                              X = x
                              Y = y
                              RequestsPan = requestsPan }
                    )
                )
                match movementUnit with
                | Some unitId ->
                    if not (Set.contains unitId eventModel.Editor.SelectedUnits) then
                        dispatch (EditorChanged(SelectEditorUnit(Some unitId)))
                    pendingUnitMovePointers <- Set.add (int32 event.pointerId) pendingUnitMovePointers
                | None when requestsSelection ->
                    editorSelectionAt x y BeginEditorBoxSelection
                | None when requestsTerrain ->
                    editorTerrainAt x y BeginTerrainGesture
                | _ when requestsPan && kind = TouchPointer && terrainToolActive ->
                    dispatch (EditorChanged CancelEditorGesture)
                | _ -> ())
        svg.onPointerMove (fun event ->
            let eventModel = latestTacticalModel |> Option.defaultValue model
            if Map.containsKey (int32 event.pointerId) eventModel.EditorView.CapturedPointers then
                event.preventDefault ()
                let x, y =
                    editorScreenPoint eventModel.EditorView event.currentTarget event.clientX event.clientY
                let previous =
                    Map.find (int32 event.pointerId) eventModel.EditorView.CapturedPointers
                let pointerId = int32 event.pointerId
                let startsUnitMove =
                    Set.contains pointerId pendingUnitMovePointers
                    && (abs (x - previous.X) >= 0.5 || abs (y - previous.Y) >= 0.5)
                if startsUnitMove then
                    pendingUnitMovePointers <- Set.remove pointerId pendingUnitMovePointers
                    editorSelectionAt previous.X previous.Y BeginUnitMove
                if previous.RequestsPan && previous.Kind <> TouchPointer then
                    let gestureCamera =
                        Map.tryFind (int32 event.pointerId) presentationGestureCameras
                        |> Option.defaultValue (currentTacticalCamera eventModel.EditorView.Camera)
                    { gestureCamera with
                        PanX = gestureCamera.PanX + x - previous.X
                        PanY = gestureCamera.PanY + y - previous.Y }
                    |> enqueuePresentationFrame presentationScheduler
                else
                    dispatch (EditorWorkspaceChanged(MoveEditorPointer { previous with X = x; Y = y }))
                if eventModel.Workspace = EditorWorkspace && eventModel.Editor.Tool = Select && not previous.RequestsPan then
                    if startsUnitMove then editorSelectionAt x y ExtendUnitMove
                    elif Set.contains pointerId pendingUnitMovePointers then ()
                    else
                        match eventModel.Editor.Gesture with
                        | UnitMoveGesture _ -> editorSelectionAt x y ExtendUnitMove
                        | _ -> editorSelectionAt x y ExtendEditorBoxSelection
                else
                    match eventModel.Editor.Tool with
                    | Terrain _ when not previous.RequestsPan ->
                        editorTerrainAt x y ExtendTerrainGesture
                    | _ -> ()
            elif eventModel.Workspace = EditorWorkspace then
                match eventModel.Editor.Tool with
                | Place _ ->
                    let x, y =
                        editorScreenPoint eventModel.EditorView event.currentTarget event.clientX event.clientY
                    editorSelectionAt x y PreviewUnitPlacement
                | _ -> ())
        svg.onPointerUp (fun event ->
            let eventModel = latestTacticalModel |> Option.defaultValue model
            if Map.containsKey (int32 event.pointerId) eventModel.EditorView.CapturedPointers then
                let previous =
                    Map.find (int32 event.pointerId) eventModel.EditorView.CapturedPointers
                let x, y =
                    editorScreenPoint eventModel.EditorView event.currentTarget event.clientX event.clientY
                let pointerId = int32 event.pointerId
                let pendingUnitMove = Set.contains pointerId pendingUnitMovePointers
                pendingUnitMovePointers <- Set.remove pointerId pendingUnitMovePointers
                let gestureCamera =
                    Map.tryFind (int32 event.pointerId) presentationGestureCameras
                    |> Option.defaultValue (currentTacticalCamera eventModel.EditorView.Camera)
                if previous.RequestsPan && previous.Kind <> TouchPointer then
                    let finalCamera =
                        { gestureCamera with
                            PanX = gestureCamera.PanX + x - previous.X
                            PanY = gestureCamera.PanY + y - previous.Y }
                    // Pointer-up is the authoritative flush boundary. Present
                    // synchronously before dispatch so a lifecycle cleanup of
                    // the retained RAF scheduler cannot discard the last
                    // accepted camera.
                    presentTacticalCamera finalCamera.PanX finalCamera.PanY finalCamera.Zoom
                    dispatch (EditorWorkspaceChanged(MoveEditorPointer { previous with X = x; Y = y }))
                    presentationGestureCameras <-
                        Map.remove (int32 event.pointerId) presentationGestureCameras
                dispatch (EditorWorkspaceChanged(EndEditorPointer(int32 event.pointerId)))
                if eventModel.Workspace = EditorWorkspace && eventModel.Editor.Tool = Select && not previous.RequestsPan then
                    if not pendingUnitMove then
                        match eventModel.Editor.Gesture with
                        | UnitMoveGesture _ -> editorSelectionAt x y ExtendUnitMove
                        | _ -> editorSelectionAt x y ExtendEditorBoxSelection
                        dispatch (EditorChanged CommitEditorGesture)
                else
                    match eventModel.Workspace, eventModel.Editor.Tool with
                    | EditorWorkspace, Terrain _ when not previous.RequestsPan ->
                        editorTerrainAt x y ExtendTerrainGesture
                        dispatch (EditorChanged CommitEditorGesture)
                    | _ -> ()
                releasePointer event.currentTarget (int event.pointerId))
        svg.onLostPointerCapture (fun event ->
            let eventModel = latestTacticalModel |> Option.defaultValue model
            let lostPointer =
                Map.tryFind (int32 event.pointerId) eventModel.EditorView.CapturedPointers
            match lostPointer with
            | None -> ()
            | Some pointer ->
                cancelPresentationFrame presentationScheduler
                pendingUnitMovePointers <- Set.remove (int32 event.pointerId) pendingUnitMovePointers
                presentationGestureCameras <-
                    Map.remove (int32 event.pointerId) presentationGestureCameras
                dispatch (EditorWorkspaceChanged(LoseEditorPointerCapture(int32 event.pointerId)))
                if eventModel.Workspace = EditorWorkspace && not pointer.RequestsPan then
                    match eventModel.Editor.Tool with
                    | Select
                    | Terrain _ -> dispatch (EditorChanged CancelEditorGesture)
                    | _ -> ())
        // Camera coordinates are expressed in viewport pixels; keeping the SVG
        // viewBox in that same space makes culling, pointer hit-testing, and the
        // retained camera transform share one finite viewport contract.
        svg.viewBox (
            0,
            0,
            max 1 (int model.EditorView.ViewportWidth),
            max 1 (int model.EditorView.ViewportHeight)
        )
        svg.children [
            Svg.title "Persistent tactical battlefield"
            Svg.desc ("Shared scene layers; input passes through the command registry. " + accessibleSelection)
            Svg.g [
                svg.id "persistent-scene-camera"
                svg.custom ("data-scene-layer", "camera")
                svg.custom (
                    "transform",
                    "translate("
                    + string camera.PanX
                    + " "
                    + string camera.PanY
                    + ") scale("
                    + string camera.Zoom
                    + ")"
                )
                svg.children [
                    Svg.g [
                        svg.id "persistent-live-authority-layer"
                        svg.custom ("data-live-layer", "live-authority")
                        svg.custom ("data-live-projection", "accepted-server-snapshot")
                        svg.custom ("pointer-events", "none")
                        svg.children [
                            match model.Live.Snapshot with
                            | Some snapshot ->
                                for unit in snapshot.VisibleUnits do
                                    if viewportIntersects (float unit.Column) (float unit.Row) (float unit.Column + 1.0) (float unit.Row + 1.0) then
                                        Svg.g [
                                            svg.key ("live-unit:" + string unit.UnitId)
                                            svg.custom ("data-primitive-id", "live-unit:" + string unit.UnitId)
                                            svg.custom ("data-live-unit-id", string unit.UnitId)
                                            svg.custom ("data-live-health", string unit.Health)
                                            svg.custom ("data-live-column", string unit.Column)
                                            svg.custom ("data-live-row", string unit.Row)
                                            svg.children [
                                                Svg.circle [
                                                    svg.cx (float unit.Column * cellSize + cellSize / 2.0)
                                                    svg.cy (float unit.Row * cellSize + cellSize / 2.0)
                                                    svg.r (cellSize / 4.0)
                                                    svg.fill "#ff8c42"
                                                    svg.stroke "#fff4e6"
                                                    svg.strokeWidth 2
                                                ]
                                                Svg.text [
                                                    svg.x (float unit.Column * cellSize + cellSize / 2.0)
                                                    svg.y (float unit.Row * cellSize + cellSize / 2.0 + 5.0)
                                                    svg.custom ("text-anchor", "middle")
                                                    svg.fill "#101916"
                                                    svg.fontSize 13
                                                    svg.children [ Html.text (string unit.UnitId) ]
                                                ]
                                            ]
                                        ]
                            | None -> ()
                        ]
                    ]
                    Svg.g [
                        svg.id "persistent-editor-background"
                        svg.custom ("data-editor-layer", "background")
                        svg.custom ("pointer-events", "none")
                        svg.custom ("aria-hidden", "true")
                        svg.children [
                            if model.Workspace = EditorWorkspace then
                                match model.EditorView.Background with
                                | Some background ->
                                    let x, y, width, height =
                                        MapEditorWorkspace.backgroundRenderBox
                                            model.Editor.Map.Width
                                            model.Editor.Map.Height
                                            background
                                    match background.Crop with
                                    | Some crop ->
                                        Svg.svg [
                                            svg.custom ("data-layer", "local-raster-background")
                                            svg.custom ("x", string x)
                                            svg.custom ("y", string y)
                                            svg.custom ("width", string width)
                                            svg.custom ("height", string height)
                                            svg.custom ("viewBox", string crop.Left + " " + string crop.Top + " " + string crop.Width + " " + string crop.Height)
                                            svg.custom ("overflow", "hidden")
                                            svg.custom ("opacity", string background.Opacity)
                                            svg.children [
                                                Svg.image [
                                                    svg.custom ("href", background.DataUrl)
                                                    svg.custom ("width", string background.PixelWidth)
                                                    svg.custom ("height", string background.PixelHeight)
                                                    svg.custom ("preserveAspectRatio", "none")
                                                ]
                                            ]
                                        ]
                                    | None ->
                                        Svg.image [
                                            svg.custom ("data-layer", "local-raster-background")
                                            svg.custom ("href", background.DataUrl)
                                            svg.custom ("x", string x)
                                            svg.custom ("y", string y)
                                            svg.custom ("width", string width)
                                            svg.custom ("height", string height)
                                            svg.custom ("opacity", string background.Opacity)
                                            svg.custom ("preserveAspectRatio", if background.Fit = FillAndCrop then "xMidYMid slice" else "none")
                                        ]
                                | None -> ()
                        ]
                    ]
                    let terrainGeometry =
                        retainTacticalTerrainLayer
                            (revisions.TerrainContent + ":" + visualSystem.Identity)
                            (fun constructionCount ->
                                Svg.g [
                                    svg.id "persistent-layer-terrain-geometry"
                                    svg.custom ("data-geometry-constructions", string constructionCount)
                                    svg.children [
                                        match projection with
                                        | Some scene ->
                                            for terrain in scene.Terrain do
                                                Svg.rect [
                                                    svg.key (ScenePrimitiveId.value terrain.PrimitiveId)
                                                    svg.custom (
                                                        "data-primitive-id",
                                                        ScenePrimitiveId.value terrain.PrimitiveId
                                                    )
                                                    svg.custom ("data-terrain", terrain.Kind)
                                                    svg.custom ("data-command-available", "true")
                                                    svg.x (float terrain.Column * cellSize)
                                                    svg.y (float terrain.Row * cellSize)
                                                    svg.width cellSize
                                                    svg.height cellSize
                                                    svg.fill (
                                                        match terrain.Kind with
                                                        | "rough" -> visualSystem.TerrainRough
                                                        | "blocked" -> visualSystem.TerrainBlocked
                                                        | "objective" -> visualSystem.TerrainObjective
                                                        | _ -> visualSystem.TerrainOpen
                                                    )
                                                    svg.stroke visualSystem.Palette.Grid
                                                    svg.strokeWidth 1
                                                    svg.custom ("role", "button")
                                                    svg.custom ("data-binding-state", "unassigned")
                                                    svg.custom ("aria-description", "Shortcut: Unassigned")
                                                    svg.custom ("aria-label", "Activate cell " + string terrain.Column + "," + string terrain.Row)
                                                    svg.onClick (fun _ -> invokeCurrentCell terrain.Column terrain.Row)
                                                ]
                                                match terrain.Kind with
                                                | "rough" ->
                                                    Svg.line [
                                                        svg.key (ScenePrimitiveId.value terrain.PrimitiveId + ":hatch")
                                                        svg.custom ("data-terrain-pattern", "diagonal-hatch")
                                                        svg.x1 (float terrain.Column * cellSize + 8.0)
                                                        svg.y1 (float (terrain.Row + 1) * cellSize - 8.0)
                                                        svg.x2 (float (terrain.Column + 1) * cellSize - 8.0)
                                                        svg.y2 (float terrain.Row * cellSize + 8.0)
                                                        svg.stroke visualSystem.Palette.Text
                                                        svg.strokeWidth 3
                                                        svg.custom ("pointer-events", "none")
                                                    ]
                                                | "blocked" ->
                                                    for index, first, last in
                                                        [ 0, 9.0, cellSize - 9.0
                                                          1, cellSize - 9.0, 9.0 ] do
                                                        Svg.line [
                                                            svg.key (ScenePrimitiveId.value terrain.PrimitiveId + ":cross:" + string index)
                                                            svg.custom ("data-terrain-pattern", "cross-hatch")
                                                            svg.x1 (float terrain.Column * cellSize + first)
                                                            svg.y1 (float terrain.Row * cellSize + 9.0)
                                                            svg.x2 (float terrain.Column * cellSize + last)
                                                            svg.y2 (float (terrain.Row + 1) * cellSize - 9.0)
                                                            svg.stroke visualSystem.Rejected
                                                            svg.strokeWidth 3
                                                            svg.custom ("pointer-events", "none")
                                                        ]
                                                | "objective" ->
                                                    Svg.rect [
                                                        svg.key (ScenePrimitiveId.value terrain.PrimitiveId + ":ring")
                                                        svg.custom ("data-terrain-pattern", "inset-ring")
                                                        svg.x (float terrain.Column * cellSize + 7.0)
                                                        svg.y (float terrain.Row * cellSize + 7.0)
                                                        svg.width (cellSize - 14.0)
                                                        svg.height (cellSize - 14.0)
                                                        svg.fill "none"
                                                        svg.stroke visualSystem.Palette.NeutralFaction
                                                        svg.strokeWidth 3
                                                        svg.custom ("pointer-events", "none")
                                                    ]
                                                | _ -> ()
                                        | None -> ()
                                    ]
                                ])
                    Svg.g [
                        svg.id "persistent-layer-terrain"
                        svg.custom ("data-scene-layer", "terrain")
                        svg.custom ("data-layer-visible", string (layerVisible "terrain"))
                        svg.custom ("data-layer-constructions", string tacticalTerrainLayerConstructionCount)
                        svg.custom ("display", editorLayerDisplayValue TerrainDomain "terrain")
                        svg.custom ("opacity", string (editorLayerOpacityValue TerrainDomain))
                        svg.custom ("pointer-events", if terrainInteractionAvailable then "auto" else "none")
                        svg.custom ("aria-hidden", string (not terrainInteractionAvailable))
                        svg.children [ terrainGeometry ]
                    ]
                    let edgesGeometry =
                        retainTacticalEdgesLayer
                            (revisions.EdgesContent + ":" + visualSystem.Identity)
                            (fun constructionCount ->
                                Svg.g [
                                    svg.id "persistent-layer-edges-geometry"
                                    svg.custom ("data-geometry-constructions", string constructionCount)
                                    svg.custom ("pointer-events", "none")
                                    svg.children [
                                        match projection with
                                        | Some scene ->
                                            for edge in scene.Edges do
                                                Svg.line [
                                                    svg.key edge.Id
                                                    svg.custom ("data-primitive-id", "edge:" + edge.Id)
                                                    svg.custom ("data-edge-kind", edge.Kind)
                                                    svg.custom ("data-edge-state", if edge.State = "open" then "open" else "closed")
                                                    svg.x1 (float edge.StartColumn * cellSize)
                                                    svg.y1 (float edge.StartRow * cellSize)
                                                    svg.x2 (float edge.EndColumn * cellSize)
                                                    svg.y2 (float edge.EndRow * cellSize)
                                                    svg.stroke (
                                                        match edge.Kind with
                                                        | "door" -> visualSystem.EdgeDoor
                                                        | "window" -> visualSystem.EdgeWindow
                                                        | _ -> visualSystem.EdgeWall
                                                    )
                                                    svg.strokeWidth (if edge.Kind = "wall" then 6 else 5)
                                                    svg.custom (
                                                        "stroke-dasharray",
                                                        match edge.Kind, edge.State with
                                                        | "door", "open" -> "8 5"
                                                        | "window", _ -> "3 3"
                                                        | _ -> "none"
                                                    )
                                                ]
                                        | None -> ()
                                    ]
                                ])
                    Svg.g [
                        svg.id "persistent-layer-edges"
                        svg.custom ("data-scene-layer", "edges")
                        svg.custom ("data-layer-visible", string (layerVisible "edges"))
                        svg.custom ("data-layer-constructions", string tacticalEdgesLayerConstructionCount)
                        svg.custom ("display", editorLayerDisplayValue EdgeDomain "edges")
                        svg.custom ("opacity", string (editorLayerOpacityValue EdgeDomain))
                        svg.custom ("pointer-events", "none")
                        svg.children [ edgesGeometry ]
                    ]
                    Svg.g [
                        svg.id "persistent-layer-routes"
                        svg.custom ("data-scene-layer", "routes")
                        svg.custom ("data-layer-visible", string (layerVisible "routes"))
                        svg.custom ("display", if layerVisible "routes" then "inline" else "none")
                        svg.custom ("pointer-events", "none")
                        svg.children [
                            match projection with
                            | Some scene ->
                                for route in scene.Routes do
                                    Svg.polyline [
                                        svg.key (ScenePrimitiveId.value route.PrimitiveId)
                                        svg.custom ("data-primitive-id", ScenePrimitiveId.value route.PrimitiveId)
                                        svg.custom ("data-route-kind", route.Kind)
                                        svg.custom ("aria-label", sceneDisclosureText route.Label)
                                        svg.points (
                                            route.Points
                                            |> Array.chunkBySize 2
                                            |> Array.choose (fun pair ->
                                                if pair.Length = 2 then
                                                    Some(string (pair[0] * cellSize) + "," + string (pair[1] * cellSize))
                                                else None)
                                            |> String.concat " "
                                        )
                                        svg.fill "none"
                                        svg.stroke (
                                            if selected route.PrimitiveId then "#ffd166"
                                            elif route.Kind = "predicted" then "#e5b8ff"
                                            else "#77bdf2"
                                        )
                                        svg.strokeWidth (if selected route.PrimitiveId then 7 else 5)
                                        svg.custom (
                                            "stroke-dasharray",
                                            if route.Kind = "predicted" then "4 5" else "10 6"
                                        )
                                    ]
                            | None -> ()
                        ]
                    ]
                    let unitGlyphs =
                        // The cached glyphs BAKE IN unit.PresentationColumn/Row, which are
                        // interpolated coordinates.  Replay ramps PresentationAlpha 0.25 -> 1.0
                        // at a CONSTANT tick, so without alpha in the key every step of that
                        // ramp reuses the first frame's geometry: the units stop moving on
                        // screen while the freshly built wrapper keeps publishing the true
                        // data-presentation-column, which is the only attribute any test reads.
                        // A stale-geometry cache that stays green is worse than no cache.
                        retainTacticalUnitGlyphs
                            (revisions.UnitsContent
                             + ":" + visualSystem.Identity
                             + ":" + tier
                             + ":" + string presentationAlpha)
                            (fun () ->
                                visibleUnits
                                |> Array.map (fun unit ->
                                    let visual = unit.Visual
                                    let presentationX = unit.PresentationColumn * cellSize
                                    let presentationY = unit.PresentationRow * cellSize
                                    let width = float (CellExtent.value visual.FootprintWidth) * cellSize
                                    let depth = float (CellExtent.value visual.FootprintDepth) * cellSize
                                    ScenePrimitiveId.value unit.PrimitiveId,
                                    Svg.g [
                                        svg.custom ("data-unit-geometry", string visual.Id)
                                        svg.children [
                                            Svg.rect [
                                                svg.x (presentationX + 5.0)
                                                svg.y (presentationY + 5.0)
                                                svg.width (width - 10.0)
                                                svg.height (depth - 10.0)
                                                svg.rx visualSystem.UnitCornerRadius
                                                svg.fill visualSystem.UnitBody
                                                svg.stroke (
                                                    match visual.Faction with
                                                    | Human -> visualSystem.Palette.HumanFaction
                                                    | Arcane -> visualSystem.Palette.ArcaneFaction
                                                    | Neutral -> visualSystem.Palette.NeutralFaction
                                                    | OtherFaction _ -> visualSystem.Palette.NeutralFaction
                                                )
                                                svg.strokeWidth visualSystem.UnitStrokeWidth
                                            ]
                                            if not (Array.isEmpty visual.StatusIds) then
                                                Svg.circle [
                                                    svg.custom ("data-semantic-alert", "true")
                                                    svg.cx (presentationX + width - 9.0)
                                                    svg.cy (presentationY + 9.0)
                                                    svg.r 5.0
                                                    svg.fill visualSystem.Suppression
                                                    svg.custom ("pointer-events", "none")
                                                ]
                                            Svg.g [
                                                svg.custom ("data-unit-glyph", UnitClassId.value visual.ClassId)
                                                svg.custom ("pointer-events", "none")
                                                svg.children [
                                                    glyphView visualSystem.Palette (presentationX + width / 2.0) (presentationY + depth / 2.0) (max 1.0 ((min width depth - 16.0) / 24.0)) visual.ClassId
                                                ]
                                            ]
                                            Svg.g [
                                                svg.custom ("class", "semantic-unit-detail")
                                                svg.custom ("data-semantic-detail", "status-heading-identity")
                                                svg.custom ("pointer-events", "none")
                                                svg.children [
                                                    yield! TacticalUnitSymbolView.channels visualSystem presentationX presentationY width depth visual
                                                    Svg.text [
                                                        svg.x (presentationX + width - 9.0)
                                                        svg.y (presentationY + depth - 9.0)
                                                        svg.custom ("text-anchor", "end")
                                                        svg.fill visualSystem.Palette.Text
                                                        svg.fontSize 13
                                                        svg.text (string visual.Id)
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ])
                                |> Map.ofArray)
                    Svg.g [
                        svg.id "persistent-layer-units"
                        svg.custom ("data-scene-layer", "units")
                        svg.custom ("data-layer-visible", string (layerVisible "units"))
                        svg.custom ("data-geometry-constructions", string tacticalUnitGlyphConstructionCount)
                        svg.custom ("display", editorLayerDisplayValue UnitDomain "units")
                        svg.custom ("opacity", string (editorLayerOpacityValue UnitDomain))
                        svg.children [
                            for unit in visibleUnits do
                                let visual = unit.Visual
                                let command = sharedSceneUnitCommand model visual.Id
                                let available = command |> Option.exists commandAvailable
                                let isSelected = selected unit.PrimitiveId
                                Svg.g [
                                    svg.key (ScenePrimitiveId.value unit.PrimitiveId)
                                    svg.custom ("data-primitive-id", ScenePrimitiveId.value unit.PrimitiveId)
                                    svg.custom ("data-unit-id", string visual.Id)
                                    svg.custom ("data-unit-class", UnitClassId.value visual.ClassId)
                                    svg.custom ("data-unit-footprint", string (CellExtent.value visual.FootprintWidth) + "x" + string (CellExtent.value visual.FootprintDepth))
                                    svg.custom ("data-unit-status", String.concat " " visual.StatusIds)
                                    svg.custom ("data-presentation-column", string unit.PresentationColumn)
                                    svg.custom ("data-presentation-row", string unit.PresentationRow)
                                    svg.custom ("data-semantic-selected", string isSelected)
                                    match visual.StanceId with
                                    | Disclosed stance -> svg.custom ("data-unit-stance", stance)
                                    | _ -> ()
                                    svg.custom ("data-command-available", string available)
                                    svg.tabIndex (if isSelected then 0 else -1)
                                    match command with
                                    | Some _ when available ->
                                        svg.custom ("role", "button")
                                        svg.custom ("data-binding-state", "unassigned")
                                        svg.custom ("aria-description", "Shortcut: Unassigned")
                                        svg.custom ("aria-label", "Select tactical unit " + string visual.Id + ", " + UnitClassId.value visual.ClassId + ", " + string (CellExtent.value visual.FootprintWidth) + " by " + string (CellExtent.value visual.FootprintDepth))
                                        svg.onClick (fun event -> event.stopPropagation (); invokeCurrentUnit event.shiftKey visual.Id)
                                        svg.onKeyDown (fun event ->
                                            if event.key = "Enter" || event.key = " " then
                                                event.preventDefault ()
                                                event.stopPropagation ()
                                                invokeCurrentUnit event.shiftKey visual.Id)
                                    | _ -> svg.custom ("aria-hidden", "true")
                                    svg.children [ Map.find (ScenePrimitiveId.value unit.PrimitiveId) unitGlyphs ]
                                ]
                        ]
                    ]
                    tacticalEffectLayer cellSize visualSystem projection
                    Svg.g [
                        svg.id "persistent-layer-selection"
                        svg.custom ("data-scene-layer", "selection")
                        svg.custom ("display", if editorLayerVisible UnitDomain then "inline" else "none")
                        svg.custom ("opacity", string (editorLayerOpacityValue UnitDomain))
                        svg.custom ("pointer-events", "none")
                        svg.children [
                            match projection with
                            | Some scene ->
                                for unit in scene.Units do
                                    if selected unit.PrimitiveId then
                                        Svg.rect [
                                            svg.key ("selection:" + ScenePrimitiveId.value unit.PrimitiveId)
                                            svg.custom ("data-selection-for", ScenePrimitiveId.value unit.PrimitiveId)
                                            svg.x (unit.PresentationColumn * cellSize + 1.0)
                                            svg.y (unit.PresentationRow * cellSize + 1.0)
                                            svg.width (float (CellExtent.value unit.Visual.FootprintWidth) * cellSize - 2.0)
                                            svg.height (float (CellExtent.value unit.Visual.FootprintDepth) * cellSize - 2.0)
                                            svg.rx 8
                                            svg.fill "none"
                                            svg.stroke "#ffd166"
                                            svg.strokeWidth 3
                                            svg.custom ("stroke-dasharray", "6 3")
                                        ]
                            | None -> ()
                        ]
                    ]
                    Svg.g [
                        svg.id "persistent-tactical-overlay-layer"
                        svg.custom ("data-scene-layer", "tactical-overlays"); svg.custom ("data-overlay-order", "registry")
                        svg.custom ("data-overlay-contrast", model.Battlefield.PaletteId); svg.custom ("data-overlay-patterns", "non-color-only"); svg.custom ("pointer-events", "none")
                        svg.children [
                            for payload in tacticalOverlays.Payloads do
                                Svg.g [
                                    svg.key (TacticalOverlayId.value payload.OverlayId + ":" + ScenePrimitiveId.value payload.PrimitiveId)
                                    svg.custom ("data-overlay-id", TacticalOverlayId.value payload.OverlayId); svg.custom ("data-overlay-kind", payload.Kind)
                                    svg.custom ("data-overlay-payload-kind", string payload.PayloadKind); svg.custom ("data-overlay-order", string payload.Order)
                                    svg.custom ("data-overlay-priority", string payload.Priority); svg.custom ("data-overlay-pattern", "directional-hatch")
                                    svg.children (payloadChildren cellSize payload)
                                ]
                            for label in tacticalOverlays.Labels do
                                if label.Points.Length >= 2 then
                                    Svg.text [
                                        svg.key ("overlay-label:" + TacticalOverlayId.value label.OverlayId + ":" + label.SubjectId)
                                        svg.custom ("data-overlay-label", TacticalOverlayId.value label.OverlayId)
                                        svg.x (label.Points[0] * cellSize + 8.0); svg.y (label.Points[1] * cellSize - 8.0); svg.fill "currentColor"
                                        svg.text (sceneDisclosureText label.Label)
                                    ]
                        ]
                    ]
                    Svg.g [
                        svg.id "persistent-layer-annotations"
                        svg.custom ("data-scene-layer", "annotations")
                        svg.custom ("data-layer-visible", string (layerVisible "annotations"))
                        svg.custom ("display", editorLayerDisplayValue RegionDomain "annotations")
                        svg.custom ("opacity", string (editorLayerOpacityValue RegionDomain))
                        svg.custom ("pointer-events", "none")
                        svg.children [
                            match projection with
                            | Some scene ->
                                for index, annotation in Array.indexed scene.Annotations do
                                    let x =
                                        annotation.Column
                                        |> Option.map (fun column -> float column * cellSize + 12.0)
                                        |> Option.defaultValue 12.0
                                    let y =
                                        annotation.Row
                                        |> Option.map (fun row -> float row * cellSize + 20.0)
                                        |> Option.defaultValue (20.0 + float index * 20.0)
                                    Svg.text [
                                        svg.key (ScenePrimitiveId.value annotation.PrimitiveId)
                                        svg.custom ("data-primitive-id", ScenePrimitiveId.value annotation.PrimitiveId)
                                        svg.custom ("data-annotation-kind", annotation.Kind)
                                        svg.custom ("aria-label", sceneDisclosureText annotation.Text)
                                        svg.x x
                                        svg.y y
                                        svg.fill (
                                            match annotation.Kind with
                                            | "validation" -> "#ff6b6b"
                                            | "prediction" -> "#e5b8ff"
                                            | "facing"
                                            | "attention" -> "#9bd1ff"
                                            | "stance" -> "#8ce99a"
                                            | "engagement" -> "#ff9f7f"
                                            | "synchronization" -> "#c3a6ff"
                                            | _ -> "#ffd166"
                                        )
                                        svg.fontSize 13
                                        svg.text (sceneDisclosureText annotation.Text)
                                    ]
                            | None -> ()
                        ]
                    ]
                    let state = model.Editor
                    let editorLayersActive = model.Workspace = EditorWorkspace
                    let editorLayersToken =
                        box (
                            state,
                            int (Math.Floor viewportMinimumX),
                            int (Math.Floor viewportMinimumY),
                            int (Math.Ceiling viewportMaximumX),
                            int (Math.Ceiling viewportMaximumY)
                        )
                    if editorMigratedLayersConstructionCount = 0
                       || not (Unchecked.equals editorMigratedLayersConstructionToken editorLayersToken) then
                        editorMigratedLayersConstructionToken <- editorLayersToken
                        editorMigratedLayersConstructionCount <- editorMigratedLayersConstructionCount + 1
                    Svg.g [
                            svg.id "persistent-editor-migrated-layers"
                            svg.custom ("data-editor-renderer", "persistent-svg-v1")
                            svg.custom ("data-editor-layers-active", string editorLayersActive)
                            svg.custom ("data-editor-layer-constructions", string editorMigratedLayersConstructionCount)
                            svg.custom ("display", if editorLayersActive then "inline" else "none")
                            svg.custom ("aria-hidden", string (not editorLayersActive))
                            svg.custom ("pointer-events", if editorLayersActive then "auto" else "none")
                            svg.custom ("focusable", "false")
                            svg.children [
                                Svg.g [
                                    svg.custom ("data-editor-layer", "guides")
                                    svg.custom ("display", editorLayerDisplay DocumentDomain state)
                                    svg.custom ("opacity", editorLayerOpacity DocumentDomain state)
                                    svg.custom ("pointer-events", "none")
                                    svg.children [
                                        let firstVisibleColumn = max 0 (int (Math.Floor viewportMinimumX))
                                        let lastVisibleColumn = min (int state.Map.Width) (int (Math.Ceiling viewportMaximumX))
                                        let firstVisibleRow = max 0 (int (Math.Floor viewportMinimumY))
                                        let lastVisibleRow = min (int state.Map.Height) (int (Math.Ceiling viewportMaximumY))
                                        for column in firstVisibleColumn .. lastVisibleColumn do
                                            Svg.line [
                                                svg.x1 (float column * cellSize)
                                                svg.y1 0
                                                svg.x2 (float column * cellSize)
                                                svg.y2 boardHeight
                                                svg.stroke "#52675d"
                                                svg.strokeWidth 1
                                            ]
                                        for row in firstVisibleRow .. lastVisibleRow do
                                            Svg.line [
                                                svg.x1 0
                                                svg.y1 (float row * cellSize)
                                                svg.x2 boardWidth
                                                svg.y2 (float row * cellSize)
                                                svg.stroke "#52675d"
                                                svg.strokeWidth 1
                                            ]
                                    ]
                                ]
                                Svg.g [
                                    svg.custom ("data-editor-layer", "regions")
                                    svg.custom ("display", editorLayerDisplay RegionDomain state)
                                    svg.custom ("opacity", editorLayerOpacity RegionDomain state)
                                    svg.children [
                                        for _, region in state.Map.Regions |> Map.toList |> List.filter (snd >> regionIntersectsViewport) do
                                            let isSelected = state.SelectedRegion = Some region.Id
                                            let color =
                                                match region.Purpose with
                                                | ObjectiveRegion -> "#ffd166"
                                                | DeploymentZone Blue -> "#67b7ff"
                                                | DeploymentZone Red -> "#e384ff"
                                                | DeploymentZone NeutralSide -> "#c9d7d0"
                                            match region.Geometry with
                                            | RegionRectangle(column, row, width, height) ->
                                                Svg.rect [
                                                    svg.key ("editor-region:" + string region.Id)
                                                    svg.custom ("data-primitive-id", "region:" + string region.Id)
                                                    svg.custom ("data-region-id", string region.Id)
                                                    svg.custom ("data-region-purpose", MapEditor.regionPurposeLabel region.Purpose)
                                                    svg.custom ("data-selected", string isSelected)
                                                    svg.custom ("role", "button")
                                                    svg.custom ("data-binding-state", "unassigned")
                                                    svg.custom ("aria-description", "Shortcut: Unassigned")
                                                    svg.custom ("aria-label", "Select region " + string region.Id + ", " + MapEditor.regionPurposeLabel region.Purpose)
                                                    svg.tabIndex (if isSelected then 0 else -1)
                                                    svg.x (float column * cellSize)
                                                    svg.y (float row * cellSize)
                                                    svg.width (float width * cellSize)
                                                    svg.height (float height * cellSize)
                                                    svg.fill color
                                                    svg.fillOpacity 0.18
                                                    svg.stroke (if isSelected then "#ffffff" else color)
                                                    svg.strokeWidth (if isSelected then 5 else 3)
                                                    svg.onClick (fun event ->
                                                        event.stopPropagation ()
                                                        dispatch (EditorChanged(SelectEditorRegion(Some region.Id))))
                                                    svg.onKeyDown (fun event ->
                                                        match event.key with
                                                        | "Enter"
                                                        | " " ->
                                                            event.preventDefault ()
                                                            event.stopPropagation ()
                                                            dispatch (EditorChanged(SelectEditorRegion(Some region.Id)))
                                                        | "Escape" ->
                                                            event.preventDefault ()
                                                            event.stopPropagation ()
                                                            dispatch (EditorChanged(SelectEditorRegion None))
                                                        | _ -> ())
                                                ]
                                            | RegionPolygon vertices ->
                                                Svg.polygon [
                                                    svg.key ("editor-region:" + string region.Id)
                                                    svg.custom ("data-primitive-id", "region:" + string region.Id)
                                                    svg.custom ("data-region-id", string region.Id)
                                                    svg.custom ("data-region-purpose", MapEditor.regionPurposeLabel region.Purpose)
                                                    svg.custom ("data-selected", string isSelected)
                                                    svg.custom ("role", "button")
                                                    svg.custom ("data-binding-state", "unassigned")
                                                    svg.custom ("aria-description", "Shortcut: Unassigned")
                                                    svg.custom ("aria-label", "Select region " + string region.Id + ", " + MapEditor.regionPurposeLabel region.Purpose)
                                                    svg.tabIndex (if isSelected then 0 else -1)
                                                    svg.points (
                                                        vertices
                                                        |> Array.map (fun vertex -> string (float vertex.CellColumn * cellSize) + "," + string (float vertex.CellRow * cellSize))
                                                        |> String.concat " "
                                                    )
                                                    svg.fill color
                                                    svg.fillOpacity 0.18
                                                    svg.stroke (if isSelected then "#ffffff" else color)
                                                    svg.strokeWidth (if isSelected then 5 else 3)
                                                    svg.onClick (fun event ->
                                                        event.stopPropagation ()
                                                        dispatch (EditorChanged(SelectEditorRegion(Some region.Id))))
                                                    svg.onKeyDown (fun event ->
                                                        match event.key with
                                                        | "Enter"
                                                        | " " ->
                                                            event.preventDefault ()
                                                            event.stopPropagation ()
                                                            dispatch (EditorChanged(SelectEditorRegion(Some region.Id)))
                                                        | "Escape" ->
                                                            event.preventDefault ()
                                                            event.stopPropagation ()
                                                            dispatch (EditorChanged(SelectEditorRegion None))
                                                        | _ -> ())
                                                ]
                                    ]
                                ]
                                match MapEditor.terrainPreview state with
                                | Some(terrain, addresses, isValid) ->
                                    Svg.g [
                                        svg.custom ("data-editor-layer", "terrain-preview")
                                        svg.custom ("display", editorLayerDisplay TerrainDomain state)
                                        svg.custom ("opacity", editorLayerOpacity TerrainDomain state)
                                        svg.custom ("data-preview-valid", string isValid)
                                        svg.custom ("pointer-events", "none")
                                        svg.children [
                                            for address in addresses do
                                                Svg.rect [
                                                    svg.custom ("data-preview-terrain", MapEditor.terrainLabel terrain)
                                                    svg.x (float address.CellColumn * cellSize + 3.0)
                                                    svg.y (float address.CellRow * cellSize + 3.0)
                                                    svg.width (cellSize - 6.0)
                                                    svg.height (cellSize - 6.0)
                                                    svg.fill "none"
                                                    svg.stroke (if isValid then "#ffd166" else "#ff6b6b")
                                                    svg.strokeWidth 4
                                                    svg.custom ("stroke-dasharray", if isValid then "8 4" else "3 3")
                                                ]
                                        ]
                                    ]
                                | None -> ()
                                match MapEditor.unitPreview state with
                                | Some(units, isValid) ->
                                    Svg.g [
                                        svg.custom ("data-editor-layer", "placement-preview")
                                        svg.custom ("display", editorLayerDisplay UnitDomain state)
                                        svg.custom ("opacity", editorLayerOpacity UnitDomain state)
                                        svg.custom ("data-preview-valid", string isValid)
                                        svg.custom ("pointer-events", "none")
                                        svg.children [
                                            for unit in units do
                                                let size = float unit.Size * cellSize
                                                Svg.rect [
                                                    svg.custom ("data-preview-unit", unit.ClassId)
                                                    svg.x (float unit.Column * cellSize + 3.0)
                                                    svg.y (float unit.Row * cellSize + 3.0)
                                                    svg.width (size - 6.0)
                                                    svg.height (size - 6.0)
                                                    svg.fill "none"
                                                    svg.stroke (if isValid then "#ffd166" else "#ff6b6b")
                                                    svg.strokeWidth 4
                                                    svg.custom ("stroke-dasharray", if isValid then "8 4" else "3 3")
                                                ]
                                        ]
                                    ]
                                | None -> ()
                                match state.Gesture with
                                | EdgePolylineGesture(kind, segments) ->
                                    Svg.g [
                                        svg.custom ("data-editor-layer", "edge-preview")
                                        svg.custom ("display", editorLayerDisplay EdgeDomain state)
                                        svg.custom ("opacity", editorLayerOpacity EdgeDomain state)
                                        svg.custom ("pointer-events", "none")
                                        svg.children [
                                            for column, row, direction in segments do
                                                let x1, y1, x2, y2 =
                                                    match direction with
                                                    | EastEdge ->
                                                        let x = float (column + 1) * cellSize
                                                        x, float row * cellSize, x, float (row + 1) * cellSize
                                                    | SouthEdge ->
                                                        let y = float (row + 1) * cellSize
                                                        float column * cellSize, y, float (column + 1) * cellSize, y
                                                Svg.line [
                                                    svg.custom ("data-edge-preview", string kind)
                                                    svg.x1 x1
                                                    svg.y1 y1
                                                    svg.x2 x2
                                                    svg.y2 y2
                                                    svg.stroke "#ffd166"
                                                    svg.strokeWidth 7
                                                    svg.custom ("stroke-dasharray", "7 4")
                                                ]
                                        ]
                                    ]
                                | BoxSelectionGesture(anchor, current) ->
                                    Svg.rect [
                                        svg.custom ("data-editor-layer", "selection-gesture")
                                        svg.custom ("display", editorLayerDisplay UnitDomain state)
                                        svg.custom ("opacity", editorLayerOpacity UnitDomain state)
                                        svg.custom ("data-editor-gesture", "box-selection")
                                        svg.x (float (min anchor.CellColumn current.CellColumn) * cellSize)
                                        svg.y (float (min anchor.CellRow current.CellRow) * cellSize)
                                        svg.width (float (abs (current.CellColumn - anchor.CellColumn) + 1) * cellSize)
                                        svg.height (float (abs (current.CellRow - anchor.CellRow) + 1) * cellSize)
                                        svg.fill "none"
                                        svg.stroke "#ffd166"
                                        svg.strokeWidth 2
                                        svg.custom ("stroke-dasharray", "6 4")
                                    ]
                                | _ -> ()
                                Svg.rect [
                                    svg.custom ("data-editor-layer", "cursor-guide")
                                    svg.custom ("data-editor-cursor", if state.Tool = Select then "selection" else "authoring")
                                    let cursor = if state.Tool = Select then state.KeyboardCursor.Cell else state.TerrainCursor
                                    svg.x (float cursor.CellColumn * cellSize + 5.0)
                                    svg.y (float cursor.CellRow * cellSize + 5.0)
                                    svg.width (cellSize - 10.0)
                                    svg.height (cellSize - 10.0)
                                    svg.fill "none"
                                    svg.stroke "#ffd166"
                                    svg.strokeWidth 2
                                    svg.custom ("stroke-dasharray", "5 3")
                                ]
                                match state.ActiveIssue with
                                | Some index when index >= 0 && index < state.Issues.Length ->
                                    let issue = state.Issues[index]
                                    Svg.text [
                                        svg.custom ("data-editor-layer", "validation-overlay")
                                        svg.x 18
                                        svg.y 31
                                        svg.fill "#ffb4a2"
                                        svg.fontSize 15
                                        svg.text (issue.Code + " · " + issue.Message)
                                    ]
                                | _ -> ()
                            ]
                    ]
                ]
            ]
        ]
    ]

let tacticalSceneRevisions (model: Model) (projection: SharedSceneProjection option) =
    let acceptedScene =
        projection
        |> Option.map (fun scene -> string scene.Owner + ":" + scene.RevisionIdentity + ":" + string scene.Tick)
        |> Option.defaultValue "unavailable"
    let viewportBounds, _ =
        viewportChunkFacts
            model.EditorView.Camera
            model.EditorView.ViewportWidth
            model.EditorView.ViewportHeight
    let minimumX, minimumY, maximumX, maximumY = viewportBounds
    let chunk value = int (Math.Floor(value / 8.0))
    let viewport =
        String.concat
            ":"
            [ string (chunk minimumX)
              string (chunk minimumY)
              string (chunk maximumX)
              string (chunk maximumY)
              semanticTier model.EditorView.Camera ]
    let length select = projection |> Option.map select |> Option.defaultValue 0 |> string
    let layer name count = acceptedScene + ":" + viewport + ":" + name + ":" + count
    let contentScene =
        projection
        |> Option.map (fun scene -> scene.RevisionIdentity)
        |> Option.defaultValue "unavailable"
    let contentLayer name count = contentScene + ":" + viewport + ":" + name + ":" + count
    // Terrain and edges are NOT viewport-culled -- only units are (see
    // emittedPrimitiveCount above).  Including the chunk window and tier in
    // their CONTENT key therefore rebuilt their retained geometry on every
    // camera move, for a layer whose content the camera cannot change, which is
    // the opposite of "camera motion changes only visible-chunk-dependent
    // presentation".  Their presentation keys still carry the viewport; only the
    // retained geometry is freed from it.
    let unculledContentLayer name count = contentScene + ":" + name + ":" + count
    let contentTick =
        projection |> Option.map (fun scene -> string scene.Tick) |> Option.defaultValue "0"
    let selection =
        projection
        |> Option.map (fun scene ->
            String.concat
                ","
                ([ yield! scene.Selection.SelectedPrimitiveIds |> Array.map ScenePrimitiveId.value
                   yield! scene.Selection.SelectedUnits |> Array.map string
                   match scene.Selection.FocusedUnit with
                   | Some unitId -> yield string unitId
                   | None -> () ]))
        |> Option.defaultValue ""
    let interaction =
        String.concat
            ":"
            [ string model.Workspace
              model.Editor.Revision.Digest
              string model.Editor.Tool
              string model.Tactical.Modality
              model.Battlefield.PaletteId
              string model.Battlefield.ReducedMotion
              model.Live.Status
              (model.Live.Snapshot |> Option.map (fun value -> string value.ProjectionRevision) |> Option.defaultValue "") ]
    { AcceptedScene = acceptedScene
      TerrainContent = unculledContentLayer "terrain" (length (fun scene -> scene.Terrain.Length))
      EdgesContent = unculledContentLayer "edges" (length (fun scene -> scene.Edges.Length))
      UnitsContent = contentLayer "units" (length (fun scene -> scene.Units.Length)) + ":" + contentTick
      Terrain = layer "terrain" (length (fun scene -> scene.Terrain.Length))
      Edges = layer "edges" (length (fun scene -> scene.Edges.Length))
      Units = layer "units" (length (fun scene -> scene.Units.Length))
      Routes = layer "routes" (length (fun scene -> scene.Routes.Length))
      Annotations = layer "annotations" (length (fun scene -> scene.Annotations.Length))
      Effects = layer "effects" (length (fun scene -> scene.Effects.Length))
      Overlays =
        acceptedScene
        + ":"
        + viewport
        + ":"
        + TacticalSceneProjection.exportOverlayPreferences model.TacticalOverlays
        + ":"
        + string model.HeldTacticalOverlays
      Accessibility = acceptedScene + ":" + selection
      Interaction = interaction
      Camera =
        string model.EditorView.Camera.PanX
        + ":"
        + string model.EditorView.Camera.PanY
        + ":"
        + string model.EditorView.Camera.Zoom }

let tacticalSceneOwnerRender (props: TacticalSceneOwnerProps) =
    let scheduler =
        React.useMemo(fun () ->
            createPresentationFrameScheduler (fun (camera: BattlefieldCamera) ->
                presentTacticalCamera camera.PanX camera.PanY camera.Zoom))
    React.useEffect(
        (fun () -> fun () -> disposePresentationFrameScheduler scheduler),
        [| box scheduler |]
    )
    persistentSceneSvg
        props.Model
        props.Projection
        props.PresentationAlpha
        props.Revisions
        scheduler
        props.Dispatch

let tacticalSceneOwner =
    React.memo(
        tacticalSceneOwnerRender,
        (fun previous current ->
            let previousLayers = { previous.Revisions with Camera = "" }
            let currentLayers = { current.Revisions with Camera = "" }
            let cameraAccepted =
                previous.Revisions.Camera = current.Revisions.Camera
                || isTacticalCameraPresented
                    current.Model.EditorView.Camera.PanX
                    current.Model.EditorView.Camera.PanY
                    current.Model.EditorView.Camera.Zoom
            // The revision strings above are a POSITIVE list of facts, and this
            // scene renders far more editor state than they enumerate: layer
            // domains, the active gesture, terrain/unit/edge/placement previews,
            // cursors, and the raster background.  Enumerating those was tried
            // and it silently froze all of them.  So the editor state is
            // compared by identity instead -- exact by construction under Elmish
            // (a changed record is a new record), O(1), and impossible to leave a
            // fact out of.  The expensive geometry is still NOT rebuilt by this:
            // terrain, edges, and unit glyphs come from retainTactical*Layer,
            // which is keyed on content revisions and survives a re-render.
            let editorAccepted =
                System.Object.ReferenceEquals(previous.Model.Editor, current.Model.Editor)
            // EditorView carries the camera, which legitimately changes every
            // pointer frame and is presented imperatively by the rAF owner --
            // so a camera-only delta stays accepted through cameraAccepted below.
            // Everything ELSE on EditorView (background, viewport, cursors) is a
            // real scene fact and must re-render.
            let editorViewAccepted =
                System.Object.ReferenceEquals(previous.Model.EditorView, current.Model.EditorView)
                || { previous.Model.EditorView with Camera = current.Model.EditorView.Camera }
                   = current.Model.EditorView
            // The layer revisions above count PRIMITIVES, not their content.  A
            // route that goes from preview to planned keeps the route count at
            // two, keeps the simulator projection's RevisionIdentity (which is
            // the EDITOR digest) and keeps the tick -- so every revision term is
            // unchanged and the scene never re-renders.  Measured: committing a
            // second unit's route succeeded in the model and the SVG went on
            // drawing it as a preview, so `route-planned` never reached the DOM
            // and the whole thing read like the domain had rejected the commit.
            //
            // The accepted scene's SOURCES are compared by identity for the same
            // reason model.Editor is: exact under Elmish, O(1), and impossible to
            // leave a fact out of.  Simulator is the one this was measured on;
            // Planning and Shell own the other two workspaces' accepted scenes
            // through exactly the same mechanism and are included so the hole is
            // closed rather than moved.
            let acceptedSourceAccepted =
                System.Object.ReferenceEquals(previous.Model.Simulator, current.Model.Simulator)
                && System.Object.ReferenceEquals(previous.Model.Planning, current.Model.Planning)
                && System.Object.ReferenceEquals(previous.Model.Shell, current.Model.Shell)
            previousLayers = currentLayers
            && previous.PresentationAlpha = current.PresentationAlpha
            && acceptedSourceAccepted
            && editorAccepted
            && editorViewAccepted
            && cameraAccepted)
    )
