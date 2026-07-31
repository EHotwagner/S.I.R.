namespace SIR.Client

open System
open System.Globalization

type SceneProjectionOwner =
    | EditorScene
    | PlanningScene
    | SimulatorScene
    | ReviewScene

type ScenePrimitiveId = private ScenePrimitiveId of string

[<RequireQualifiedAccess>]
module ScenePrimitiveId =
    let value (ScenePrimitiveId value) = value

type SceneTerrainProjection =
    { PrimitiveId: ScenePrimitiveId
      Column: int32
      Row: int32
      Kind: string }

type SceneUnitProjection =
    { PrimitiveId: ScenePrimitiveId
      Visual: UnitVisual }

type SceneRouteProjection =
    { PrimitiveId: ScenePrimitiveId
      OwnerUnitId: int32 option
      Kind: string
      Points: float array
      Label: Disclosure<string> }

type SceneAnnotationProjection =
    { PrimitiveId: ScenePrimitiveId
      Kind: string
      Column: int32 option
      Row: int32 option
      Text: Disclosure<string> }

type SceneDisclosureProjection =
    { Source: DisclosureLabel
      PerspectiveFiltered: bool
      PreservesFieldDisclosures: bool }

type SceneCameraProjection =
    { PanX: float
      PanY: float
      Zoom: float }

type SceneSelectionProjection =
    { SelectedUnits: int32 array
      FocusedUnit: int32 option
      SelectedRegion: int32 option
      SelectedCommand: string option
      SelectedEvent: int32 option
      SelectedPrimitiveIds: ScenePrimitiveId array }

type SceneLayerProjection =
    { PrimitiveId: ScenePrimitiveId
      Kind: string
      Order: int
      Visible: bool
      Locked: bool }

type SharedSceneProjection =
    { Owner: SceneProjectionOwner
      RevisionIdentity: string
      Tick: int32
      Board: BoardVisual
      Terrain: SceneTerrainProjection array
      Edges: EdgeVisual array
      Units: SceneUnitProjection array
      Routes: SceneRouteProjection array
      Annotations: SceneAnnotationProjection array
      Disclosure: SceneDisclosureProjection
      Camera: SceneCameraProjection
      Selection: SceneSelectionProjection
      Layers: SceneLayerProjection array }

type EditorProjectionInput =
    { EditorState: MapEditorState
      EditorWorkspace: EditorWorkspaceState
      EditorFocusedUnit: int32 option }

type PlanningProjectionInput =
    { PlanningMap: MapDefinition
      PlanningState: PlanningWorkspaceState
      PlanningCamera: BattlefieldCamera
      PlanningFocusedUnit: int32 option }

type SimulatorProjectionInput =
    { SimulatorHandoff: SimulatorHandoff
      SimulatorSelectedUnit: int32 option
      SimulatorCamera: BattlefieldCamera
      SimulatorFocusedUnit: int32 option }

type AcceptedReviewProjection =
    private
        { AcceptedFrame: RenderFrame
          AcceptedRevisionIdentity: string
          AcceptedSelectedUnit: int32 option
          AcceptedSelectedEvent: int32 option }

type ReviewProjectionInput =
    { AcceptedReview: AcceptedReviewProjection
      ReviewCamera: BattlefieldCamera
      ReviewFocusedUnit: int32 option }

[<RequireQualifiedAccess>]
module TacticalSceneProjection =
    let private invariant (value: int32) =
        value.ToString(CultureInfo.InvariantCulture)

    let private primitive kind identity =
        ScenePrimitiveId(kind + ":" + identity)

    let private boardOfMap (map: MapDefinition) =
        { MinimumColumn = 0
          MinimumRow = 0
          MaximumColumn = map.Width - 1
          MaximumRow = map.Height - 1 }

    let private terrainName = function
        | Open -> "open"
        | Rough -> "rough"
        | Blocked -> "blocked"
        | Objective -> "objective"

    let private terrainOfMap (map: MapDefinition) =
        let width = max 0 (int map.Width)
        let height = max 0 (int map.Height)
        Array.init (width * height) (fun index ->
            let column = int32 (index % width)
            let row = int32 (index / width)
            let terrain =
                map.Terrain
                |> Map.tryFind (column, row)
                |> Option.defaultValue Open
            { PrimitiveId =
                primitive
                    "terrain"
                    (invariant column + ":" + invariant row)
              Column = column
              Row = row
              Kind = terrainName terrain })

    let private copyEdge (edge: EdgeVisual) : EdgeVisual =
        { Id = edge.Id
          Kind = edge.Kind
          State = edge.State
          StartColumn = edge.StartColumn
          StartRow = edge.StartRow
          EndColumn = edge.EndColumn
          EndRow = edge.EndRow }

    let private edgesOfMap (map: MapDefinition) : EdgeVisual array =
        map.Edges
        |> Map.toArray
        |> Array.map (fun ((column, row, direction), (kind, isOpen)) ->
            let startColumn, startRow, endColumn, endRow =
                match direction with
                | EastEdge -> column + 1, row, column + 1, row + 1
                | SouthEdge -> column, row + 1, column + 1, row + 1
            let edge: EdgeVisual =
                { Id =
                    "editor-edge-"
                    + invariant column
                    + "-"
                    + invariant row
                    + "-"
                    + string direction
                  Kind =
                    match kind with
                    | Wall -> "wall"
                    | Door -> "door"
                    | Window -> "window"
                  State = if isOpen then "open" else "solid"
                  StartColumn = startColumn
                  StartRow = startRow
                  EndColumn = endColumn
                  EndRow = endRow }
            edge)

    let private copyVisual (unit: UnitVisual) =
        { unit with StatusIds = Array.copy unit.StatusIds }

    let private unitsOfFrame (frame: RenderFrame) =
        frame.Units
        |> Array.map (fun unit ->
            { PrimitiveId = primitive "unit" (invariant unit.Id)
              Visual = copyVisual unit })

    let private camera (value: BattlefieldCamera) =
        { PanX = value.PanX
          PanY = value.PanY
          Zoom = value.Zoom }

    let private selectedUnits candidates focused (units: SceneUnitProjection array) =
        let visibleIds =
            units |> Array.map (fun unit -> unit.Visual.Id) |> Set.ofArray
        let selected =
            candidates
            |> Seq.filter (fun id -> Set.contains id visibleIds)
            |> Seq.distinct
            |> Seq.sort
            |> Seq.toArray
        selected,
        (focused |> Option.filter (fun id -> Set.contains id visibleIds))

    let private selection selected focused region command event extraPrimitiveIds =
        { SelectedUnits = selected
          FocusedUnit = focused
          SelectedRegion = region
          SelectedCommand = command
          SelectedEvent = event
          SelectedPrimitiveIds =
            [| yield!
                   selected
                   |> Array.map (fun id -> primitive "unit" (invariant id))
               yield! extraPrimitiveIds |]
            |> Array.distinctBy ScenePrimitiveId.value }

    let private disclosure source =
        { Source = source
          PerspectiveFiltered = source = PerspectiveDisclosure
          PreservesFieldDisclosures = true }

    let private layer kind order visible locked =
        { PrimitiveId = primitive "layer" kind
          Kind = kind
          Order = order
          Visible = visible
          Locked = locked }

    let private standardLayers =
        [| layer "terrain" 0 true false
           layer "edges" 1 true false
           layer "units" 2 true false
           layer "routes" 3 true false
           layer "annotations" 4 true false |]

    let private editorLayer domain state order =
        let kind =
            match domain with
            | TerrainDomain -> "terrain"
            | EdgeDomain -> "edges"
            | UnitDomain -> "units"
            | RegionDomain -> "annotations"
            | DocumentDomain -> "document"
        match state with
        | VisibleLayer -> layer kind order true false
        | DimmedLayer -> layer kind order true false
        | HiddenLayer -> layer kind order false false
        | LockedLayer -> layer kind order true true

    let private routePoints (cells: (int32 * int32) array) =
        cells
        |> Array.collect (fun (column, row) ->
            [| float column + 0.5; float row + 0.5 |])

    let private eventAnnotations prefix (events: RenderEventVisual array) =
        events
        |> Array.map (fun event ->
            { PrimitiveId =
                primitive prefix (invariant event.Id)
              Kind = event.Kind
              Column = None
              Row = None
              Text = event.Summary })

    let private regionAnnotations (regions: Map<int32, MapRegion>) =
        regions
        |> Map.toArray
        |> Array.map (fun (id, region) ->
            let column, row =
                match region.Geometry with
                | RegionRectangle(column, row, _, _) -> Some column, Some row
                | RegionPolygon vertices ->
                    vertices
                    |> Array.tryHead
                    |> Option.map (fun value ->
                        Some value.CellColumn, Some value.CellRow)
                    |> Option.defaultValue (None, None)
            { PrimitiveId = primitive "region" (invariant id)
              Kind =
                match region.Purpose with
                | ObjectiveRegion -> "objective-region"
                | DeploymentZone Blue -> "blue-deployment"
                | DeploymentZone Red -> "red-deployment"
                | DeploymentZone NeutralSide -> "neutral-deployment"
              Column = column
              Row = row
              Text = Disclosed("Region " + invariant id) })

    let editor (input: EditorProjectionInput) =
        let frame = MapEditor.frame input.EditorState
        let units = unitsOfFrame frame
        let selected, focused =
            selectedUnits
                (seq {
                    yield! input.EditorState.SelectedUnits
                    yield! input.EditorState.SelectedUnit |> Option.toList
                })
                input.EditorFocusedUnit
                units
        let selectedRegion =
            input.EditorState.SelectedRegion
            |> Option.filter (fun id ->
                Map.containsKey id input.EditorState.Map.Regions)
        let layers =
            input.EditorState.Layers
            |> Map.toArray
            |> Array.sortBy fst
            |> Array.mapi (fun order (domain, state) ->
                editorLayer domain state order)
        { Owner = EditorScene
          RevisionIdentity = input.EditorState.Revision.Digest
          Tick = input.EditorState.Tick
          Board = frame.Board
          Terrain = terrainOfMap input.EditorState.Map
          Edges = frame.Edges |> Array.map copyEdge
          Units = units
          Routes = [||]
          Annotations =
            Array.append
                (regionAnnotations input.EditorState.Map.Regions)
                (eventAnnotations "editor-event" frame.Events)
          Disclosure = disclosure SandboxDisclosure
          Camera = camera input.EditorWorkspace.Camera
          Selection =
            selection
                selected
                focused
                selectedRegion
                None
                None
                (selectedRegion
                 |> Option.map (fun id ->
                     [| primitive "region" (invariant id) |])
                 |> Option.defaultValue [||])
          Layers = layers }

    let private planningUnit
        (map: MapDefinition)
        (member': PlanningRosterMember)
        : SceneUnitProjection
        =
        let authored = Map.tryFind member'.UnitId map.Units
        let faction =
            match authored |> Option.map _.Side with
            | Some Blue -> Human
            | Some Red -> Arcane
            | Some NeutralSide -> Neutral
            | None ->
                match member'.Side.ToLowerInvariant() with
                | "blue" -> Human
                | "red" -> Arcane
                | "neutralside"
                | "neutral" -> Neutral
                | other -> OtherFaction other
        let footprint =
            authored
            |> Option.map _.Size
            |> Option.bind CellExtent.tryCreate
            |> Option.defaultWith (fun () ->
                CellExtent.tryCreate 1
                |> Option.defaultWith (fun () ->
                    invalidOp "One-cell planning footprint was invalid."))
        { PrimitiveId = primitive "unit" (invariant member'.UnitId)
          Visual =
            { Id = member'.UnitId
              AnchorColumn = member'.Column
              AnchorRow = member'.Row
              FootprintWidth = footprint
              FootprintDepth = footprint
              ClassId =
                authored
                |> Option.map _.ClassId
                |> Option.defaultValue member'.Role
                |> UnitClassId.resolve
              Faction = faction
              Health = NotPresent
              Level = NotPresent
              StanceId = NotPresent
              BodyHeading = NotPresent
              SecondaryHeading = NotPresent
              ShortLabel = Disclosed member'.Name
              StatusIds = [| "planning" |] } }

    let private planningAnnotation (command: PlanningCommand) : SceneAnnotationProjection =
        let kind, text =
            match command.Kind with
            | PlannedRoute _ -> "route", "Route"
            | PlannedFacing direction ->
                "facing", "Facing " + string direction
            | PlannedAttention direction ->
                "attention", "Attention " + string direction
            | PlannedStance stance -> "stance", "Stance " + stance
            | PlannedHold -> "hold", "Hold"
            | PlannedEngagement(target, capability) ->
                "engagement",
                "Engage " + invariant target + " with " + capability
            | PlannedSynchronization(marker, deadline) ->
                "synchronization",
                marker + " by " + invariant deadline
        { PrimitiveId = primitive "plan-command" command.Id
          Kind = kind
          Column = None
          Row = None
          Text = Disclosed text }

    let planning (input: PlanningProjectionInput) =
        let units =
            input.PlanningState.Roster
            |> Array.sortBy _.UnitId
            |> Array.map (planningUnit input.PlanningMap)
        let routes =
            input.PlanningState.Commands
            |> List.choose (fun command ->
                match command.Kind with
                | PlannedRoute cells ->
                    Some
                        { PrimitiveId = primitive "route" command.Id
                          OwnerUnitId = Some command.UnitId
                          Kind = "planned"
                          Points = routePoints cells
                          Label =
                            Disclosed(
                                "Planned route for unit "
                                + invariant command.UnitId
                            ) }
                | _ -> None)
            |> List.toArray
        let selected, focused =
            selectedUnits
                (input.PlanningState.SelectedUnit |> Option.toList)
                input.PlanningFocusedUnit
                units
        let selectedCommand, selectedCommandPrimitive =
            input.PlanningState.SelectedCommand
            |> Option.bind (fun selectedId ->
                input.PlanningState.Commands
                |> List.tryFind (fun command -> command.Id = selectedId)
                |> Option.map (fun command ->
                    let kind =
                        match command.Kind with
                        | PlannedRoute _ -> "route"
                        | _ -> "plan-command"
                    selectedId, primitive kind selectedId))
            |> function
                | Some(command, primitiveId) ->
                    Some command, [| primitiveId |]
                | None -> None, [||]
        { Owner = PlanningScene
          RevisionIdentity =
            input.PlanningState.MapRevision
            + ":"
            + input.PlanningState.Digest
          Tick = input.PlanningState.AuthoringTick
          Board = boardOfMap input.PlanningMap
          Terrain = terrainOfMap input.PlanningMap
          Edges = edgesOfMap input.PlanningMap
          Units = units
          Routes = routes
          Annotations =
            input.PlanningState.Commands
            |> List.filter (fun command ->
                match command.Kind with
                | PlannedRoute _ -> false
                | _ -> true)
            |> List.map planningAnnotation
            |> List.toArray
          Disclosure = disclosure SandboxDisclosure
          Camera = camera input.PlanningCamera
          Selection =
            selection
                selected
                focused
                None
                selectedCommand
                None
                selectedCommandPrimitive
          Layers = Array.copy standardLayers }

    let private overlayRoute (overlay: OverlayVisual) : SceneRouteProjection =
        { PrimitiveId = primitive "route" overlay.Id
          OwnerUnitId =
            match overlay.Scope with
            | SelectedUnitOverlay id -> Some id
            | WholeForceOverlay -> None
          Kind = overlay.Kind
          Points = Array.copy overlay.Points
          Label = overlay.Label }

    let private simulatorOverlayRoute
        (overlay: OverlayVisual)
        : SceneRouteProjection
        =
        let ownerIdentity =
            match overlay.Scope with
            | SelectedUnitOverlay id -> invariant id
            | WholeForceOverlay -> "force"
        let slot =
            if
                overlay.Kind.Contains(
                    "preview",
                    StringComparison.OrdinalIgnoreCase
                )
            then
                "preview"
            else
                "planned"
        { PrimitiveId =
            primitive
                "route"
                ("simulator:" + ownerIdentity + ":" + slot)
          OwnerUnitId =
            match overlay.Scope with
            | SelectedUnitOverlay id -> Some id
            | WholeForceOverlay -> None
          Kind = overlay.Kind
          Points = Array.copy overlay.Points
          Label = overlay.Label }

    let simulator (input: SimulatorProjectionInput) =
        let frame =
            MapEditorSimulator.frame
                input.SimulatorSelectedUnit
                input.SimulatorHandoff
        let units = unitsOfFrame frame
        let selected, focused =
            selectedUnits
                (input.SimulatorSelectedUnit |> Option.toList)
                input.SimulatorFocusedUnit
                units
        { Owner = SimulatorScene
          RevisionIdentity = input.SimulatorHandoff.Revision.Digest
          Tick = input.SimulatorHandoff.Tick
          Board = frame.Board
          Terrain = terrainOfMap input.SimulatorHandoff.RuntimeMap
          Edges = frame.Edges |> Array.map copyEdge
          Units = units
          Routes = frame.Overlays |> Array.map simulatorOverlayRoute
          Annotations = eventAnnotations "simulator-event" frame.Events
          Disclosure = disclosure SandboxDisclosure
          Camera = camera input.SimulatorCamera
          Selection =
            selection
                selected
                focused
                None
                None
                None
                [||]
          Layers = Array.copy standardLayers }

    let acceptReview (model: Model) =
        match model.Source, model.Mode, model.Verification, model.Inspection with
        | Loaded metadata, VerifiedReplay, BrowserKernelVerified, Some inspection
            when metadata.Kind = FullReplay
                 && inspection.PerspectiveHash.IsNone ->
            Shell.renderFrame model
            |> Option.map (fun frame ->
                { AcceptedFrame = frame
                  AcceptedRevisionIdentity =
                    "replay:"
                    + metadata.SourceIdentity
                    + ":"
                    + metadata.EngineIdentity
                  AcceptedSelectedUnit = model.Selection.Unit
                  AcceptedSelectedEvent = model.Selection.Event })
        | Loaded metadata, PerspectivePlayback, PerspectiveReady, Some inspection
            when metadata.Kind = PerspectiveReplay
                 && inspection.PerspectiveHash.IsSome
                 && inspection.Units.IsEmpty
                 && inspection.Edges.IsEmpty
                 && inspection.Events.IsEmpty
                 && inspection.Checkpoints.IsEmpty
                 && inspection.BoardMinimumColumn = 0
                 && inspection.BoardMinimumRow = 0
                 && inspection.BoardMaximumColumn = 0
                 && inspection.BoardMaximumRow = 0 ->
            Shell.renderFrame model
            |> Option.map (fun frame ->
                { AcceptedFrame = frame
                  AcceptedRevisionIdentity =
                    "replay:"
                    + metadata.SourceIdentity
                    + ":"
                    + metadata.EngineIdentity
                  AcceptedSelectedUnit = model.Selection.Unit
                  AcceptedSelectedEvent = model.Selection.Event })
        | _ -> None

    let review (input: ReviewProjectionInput) =
        let frame = input.AcceptedReview.AcceptedFrame
        let units = unitsOfFrame frame
        let selected, focused =
            selectedUnits
                (input.AcceptedReview.AcceptedSelectedUnit |> Option.toList)
                input.ReviewFocusedUnit
                units
        let routes, annotations =
            frame.Overlays
            |> Array.partition (fun overlay ->
                overlay.Kind.Contains("route", StringComparison.OrdinalIgnoreCase))
        let overlayAnnotations =
            annotations
            |> Array.map (fun overlay ->
                { PrimitiveId = primitive "overlay" overlay.Id
                  Kind = overlay.Kind
                  Column = None
                  Row = None
                  Text = overlay.Label })
        let eventAnnotations =
            eventAnnotations "review-event" frame.Events
        let visibleEvents =
            frame.Events |> Array.map _.Id |> Set.ofArray
        let selectedEvent =
            input.AcceptedReview.AcceptedSelectedEvent
            |> Option.filter (fun id -> Set.contains id visibleEvents)
        { Owner = ReviewScene
          RevisionIdentity = input.AcceptedReview.AcceptedRevisionIdentity
          Tick = frame.Tick
          Board = frame.Board
          Terrain = [||]
          Edges = frame.Edges |> Array.map copyEdge
          Units = units
          Routes = routes |> Array.map overlayRoute
          Annotations =
            Array.append
                overlayAnnotations
                eventAnnotations
          Disclosure = disclosure frame.Disclosure
          Camera = camera input.ReviewCamera
          Selection =
            selection
                selected
                focused
                None
                None
                selectedEvent
                (selectedEvent
                 |> Option.map (fun id ->
                     [| primitive "review-event" (invariant id) |])
                 |> Option.defaultValue [||])
          Layers = Array.copy standardLayers }

    let primitiveIds (projection: SharedSceneProjection) =
        [| yield! projection.Terrain |> Array.map _.PrimitiveId
           yield!
               projection.Edges
               |> Array.map (fun edge -> primitive "edge" edge.Id)
           yield! projection.Units |> Array.map _.PrimitiveId
           yield! projection.Routes |> Array.map _.PrimitiveId
           yield! projection.Annotations |> Array.map _.PrimitiveId
           yield! projection.Layers |> Array.map _.PrimitiveId |]
