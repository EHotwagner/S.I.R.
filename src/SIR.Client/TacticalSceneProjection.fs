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

type TacticalOverlayId = private TacticalOverlayId of string

[<RequireQualifiedAccess>]
module TacticalOverlayId =
    let value (TacticalOverlayId value) = value

type TacticalOverlayMode =
    | OverlayOff
    | InspectHeld
    | SelectionScoped
    | Persistent

type TacticalOverlayCategory =
    | UnitOverlay
    | AwarenessOverlay
    | MovementOverlay
    | ProtectionOverlay
    | CombatOverlay
    | CommandOverlay

type TacticalOverlayPayloadKind =
    | FootprintPayload
    | DirectionPayload
    | PolylinePayload
    | AreaPayload
    | TracePayload
    | StatusPayload

type TacticalOverlayAvailability =
    | OverlayAvailable
    | OverlayUnavailable

type TacticalOverlayDisclosurePolicy =
    | DisclosedSceneFactsOnly
    | SelectedDisclosedFactsOnly

type TacticalOverlayDescriptor =
    { Id: TacticalOverlayId
      Label: string
      Category: TacticalOverlayCategory
      DefaultMode: TacticalOverlayMode
      SupportedModes: Set<TacticalOverlayMode>
      CommandId: string
      DefaultGesture: string option
      Availability: TacticalOverlayAvailability
      DisclosurePolicy: TacticalOverlayDisclosurePolicy
      PayloadKind: TacticalOverlayPayloadKind
      Order: int }

type TacticalOverlayPreferences =
    { SchemaVersion: int
      Modes: Map<TacticalOverlayId, TacticalOverlayMode> }

type TacticalOverlayPreferenceDiagnostic =
    | MalformedOverlayPreferences
    | UnsupportedOverlayPreferenceSchema of int

type TacticalOverlayGeometry =
    | FootprintGeometry of centerX: float * centerY: float * width: float * depth: float
    | DirectionGeometry of originX: float * originY: float * headingRadians: float * arcRadians: float option
    | PathGeometry of points: float array * movementCost: int32 option * blockerIds: string array
    | AreaGeometry of centerX: float * centerY: float * radius: float
    | TraceGeometry of points: float array * impactX: float * impactY: float
    | StatusGeometry of anchorX: float * anchorY: float * current: int32 option * maximum: int32 option * tokens: string array

type TacticalOverlayPayload =
    { OverlayId: TacticalOverlayId
      PrimitiveId: ScenePrimitiveId
      SubjectId: string
      Tick: int32
      Kind: string
      PayloadKind: TacticalOverlayPayloadKind
      Geometry: TacticalOverlayGeometry
      Points: float array
      Label: Disclosure<string>
      Priority: int
      Order: int }

type TacticalOverlayCost =
    { RegistryTraversals: int
      DisclosurePasses: int
      CandidatePayloads: int
      EmittedPayloads: int
      EmittedLabels: int
      EstimatedSvgNodes: int }

type TacticalOverlayProjection =
    { Payloads: TacticalOverlayPayload array
      Labels: TacticalOverlayPayload array
      Cost: TacticalOverlayCost }

type TacticalEffectKind =
    | MovementEffect
    | AttackEffect
    | ImpactEffect
    | SuppressionEffect
    | RecoveryEffect
    | SignalEffect
    | ObjectiveEffect
    | GenericEffect

type TacticalEffectLifecycle =
    | PreviewEffect
    | PredictedEffect
    | AcceptedEffect
    | CommittedEffect
    | RejectedEffect
    | HistoricalEffect

type TacticalEffectProjection =
    { PrimitiveId: ScenePrimitiveId
      EventId: int32
      Tick: int32
      Kind: TacticalEffectKind
      Lifecycle: TacticalEffectLifecycle
      SourceUnitId: int32 option
      TargetUnitId: int32 option
      SourcePoint: (float * float) option
      TargetPoint: (float * float) option
      Label: string
      Order: int }

type TacticalVisualDensity =
    | OrdinaryDensity
    | DenseDensity
    | StressDensity

type TacticalVisualSystem =
    { Identity: string
      Palette: PaletteTokens
      Density: TacticalVisualDensity
      ReducedMotion: bool
      TerrainOpen: string
      TerrainRough: string
      TerrainBlocked: string
      TerrainObjective: string
      UnitBody: string
      EdgeWall: string
      EdgeDoor: string
      EdgeWindow: string
      Intent: string
      Impact: string
      Suppression: string
      Recovery: string
      Rejected: string
      UnitCornerRadius: float
      UnitStrokeWidth: float
      SelectedStrokeWidth: float
      TransitionMilliseconds: int
      EffectMilliseconds: int
      MaximumActiveEffects: int
      LayerOrder: string array }

type TacticalVisualCost =
    { UnitCount: int
      EffectInstances: int
      EstimatedSvgNodes: int }

type SceneTerrainProjection =
    { PrimitiveId: ScenePrimitiveId
      Column: int32
      Row: int32
      Kind: string }

type SceneUnitProjection =
    { PrimitiveId: ScenePrimitiveId
      Visual: UnitVisual
      PresentationColumn: float
      PresentationRow: float }

type SceneRouteProjection =
    { PrimitiveId: ScenePrimitiveId
      OwnerUnitId: int32 option
      OverlayId: TacticalOverlayId
      Kind: string
      Points: float array
      MovementCost: int32 option
      BlockerIds: string array
      Label: Disclosure<string> }

type SceneAnnotationProjection =
    { PrimitiveId: ScenePrimitiveId
      Kind: string
      OverlayId: TacticalOverlayId option
      SubjectUnitId: int32 option
      Column: int32 option
      Row: int32 option
      Geometry: TacticalOverlayGeometry option
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
      Effects: TacticalEffectProjection array
      VisualCost: TacticalVisualCost
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
          AcceptedVerificationIdentity: string
          AcceptedVerificationKind: string
          AcceptedSelectedUnit: int32 option
          AcceptedSelectedEvent: int32 option }

type ReviewProjectionInput =
    { AcceptedReview: AcceptedReviewProjection
      ReviewCamera: BattlefieldCamera
      ReviewFocusedUnit: int32 option }

[<RequireQualifiedAccess>]
module TacticalSceneProjection =
    [<Literal>]
    let private OverlayPreferenceSchemaVersion = 1

    [<Literal>]
    let private MaximumOverlayPayloads = 4096

    [<Literal>]
    let private MaximumOverlayLabels = 256

    [<Literal>]
    let private MaximumEffectInstances = 256

    let visualSystem paletteId reducedMotion unitCount =
        let palette =
            ReplayPalettes.all
            |> Array.tryFind (fun candidate -> candidate.Id = paletteId)
            |> Option.defaultValue ReplayPalettes.accessibleDefault
        let density =
            if unitCount > 100 then StressDensity
            elif unitCount > 40 then DenseDensity
            else OrdinaryDensity
        { Identity = "tactical-visual-system-v1"
          Palette = palette
          Density = density
          ReducedMotion = reducedMotion
          TerrainOpen = if palette.Id = "monochrome-pattern" then "#f2f0e8" else "#26332e"
          TerrainRough = if palette.Id = "monochrome-pattern" then "#dedbd0" else "#35443a"
          TerrainBlocked = if palette.Id = "monochrome-pattern" then "#c7c3b8" else "#1b2421"
          TerrainObjective = if palette.Id = "monochrome-pattern" then "#e8e0bd" else "#4b4630"
          UnitBody = palette.Canvas
          EdgeWall = palette.Text
          EdgeDoor = palette.NeutralFaction
          EdgeWindow = palette.HumanFaction
          Intent = palette.HumanFaction
          Impact = palette.HealthActive
          Suppression = palette.ArcaneFaction
          Recovery = if palette.Id = "high-contrast" then "#00ff00" else "#8ce99a"
          Rejected = if palette.Id = "high-contrast" then "#ff0000" else "#ff6b6b"
          UnitCornerRadius = 6.0
          UnitStrokeWidth = 3.0
          SelectedStrokeWidth = 5.0
          TransitionMilliseconds = if reducedMotion then 1 else 160
          EffectMilliseconds = if reducedMotion then 120 else 420
          MaximumActiveEffects = MaximumEffectInstances
          LayerOrder = [| "terrain"; "edges"; "routes"; "units"; "effects"; "selection"; "tactical-overlays"; "annotations" |] }

    let private overlayId value = TacticalOverlayId value

    let private descriptor id label category defaultMode supported payload order disclosure defaultGesture =
        { Id = overlayId id
          Label = label
          Category = category
          DefaultMode = defaultMode
          SupportedModes = Set.ofList (OverlayOff :: supported)
          CommandId = "view.overlay." + id
          DefaultGesture = defaultGesture
          Availability = OverlayAvailable
          DisclosurePolicy = disclosure
          PayloadKind = payload
          Order = order }

    let overlayRegistry =
        [| descriptor "unit.footprints" "Unit footprints" UnitOverlay Persistent [ SelectionScoped; Persistent ] FootprintPayload 10 DisclosedSceneFactsOnly None
           descriptor "unit.body-facing" "Body facing" UnitOverlay SelectionScoped [ InspectHeld; SelectionScoped; Persistent ] DirectionPayload 20 SelectedDisclosedFactsOnly None
           descriptor "movement.reachable-path-cost" "Reachable path and movement cost" MovementOverlay SelectionScoped [ InspectHeld; SelectionScoped ] PolylinePayload 30 SelectedDisclosedFactsOnly None
           descriptor "movement.planned-routes" "Planned routes" MovementOverlay Persistent [ SelectionScoped; Persistent ] PolylinePayload 40 DisclosedSceneFactsOnly None
           descriptor "movement.reservations" "Reservations" MovementOverlay SelectionScoped [ InspectHeld; SelectionScoped; Persistent ] AreaPayload 50 SelectedDisclosedFactsOnly None
           descriptor "awareness.attention-vision" "Attention and vision" AwarenessOverlay SelectionScoped [ InspectHeld; SelectionScoped; Persistent ] DirectionPayload 60 SelectedDisclosedFactsOnly None
           descriptor "spatial.exact-los" "Exact line of sight" AwarenessOverlay InspectHeld [ InspectHeld; SelectionScoped ] PolylinePayload 70 SelectedDisclosedFactsOnly (Some "Alt+L")
           descriptor "cover.exposure" "Cover and exposure" ProtectionOverlay SelectionScoped [ InspectHeld; SelectionScoped; Persistent ] DirectionPayload 80 SelectedDisclosedFactsOnly None
           descriptor "combat.armor-coverage" "Armor coverage" ProtectionOverlay SelectionScoped [ InspectHeld; SelectionScoped; Persistent ] DirectionPayload 90 SelectedDisclosedFactsOnly None
           descriptor "combat.area-engagements" "Area engagements" CombatOverlay SelectionScoped [ InspectHeld; SelectionScoped; Persistent ] AreaPayload 100 SelectedDisclosedFactsOnly None
           descriptor "combat.suppression" "Suppression" CombatOverlay SelectionScoped [ InspectHeld; SelectionScoped; Persistent ] StatusPayload 110 SelectedDisclosedFactsOnly None
           descriptor "combat.attack-traces" "Attack traces and impact" CombatOverlay SelectionScoped [ InspectHeld; SelectionScoped; Persistent ] TracePayload 120 DisclosedSceneFactsOnly None
           descriptor "combat.hp-wounds" "HP and wounds" CombatOverlay SelectionScoped [ InspectHeld; SelectionScoped; Persistent ] StatusPayload 130 SelectedDisclosedFactsOnly None
           descriptor "command.state" "Command state" CommandOverlay SelectionScoped [ InspectHeld; SelectionScoped; Persistent ] StatusPayload 140 SelectedDisclosedFactsOnly None |]

    let initialOverlayPreferences =
        { SchemaVersion = OverlayPreferenceSchemaVersion
          Modes = overlayRegistry |> Array.map (fun value -> value.Id, value.DefaultMode) |> Map.ofArray }

    let private modeToken = function
        | OverlayOff -> "off"
        | InspectHeld -> "held"
        | SelectionScoped -> "selection"
        | Persistent -> "persistent"

    let private tryMode = function
        | "off" -> Some OverlayOff
        | "held" -> Some InspectHeld
        | "selection" -> Some SelectionScoped
        | "persistent" -> Some Persistent
        | _ -> None

    let exportOverlayPreferences preferences =
        let body =
            overlayRegistry
            |> Array.map (fun item ->
                TacticalOverlayId.value item.Id
                + "="
                + (preferences.Modes |> Map.tryFind item.Id |> Option.defaultValue item.DefaultMode |> modeToken))
            |> String.concat ";"
        "v" + string OverlayPreferenceSchemaVersion + "|" + body

    let importOverlayPreferences (source: string) =
        if String.IsNullOrWhiteSpace source || not (source.StartsWith("v", StringComparison.Ordinal)) then
            Error MalformedOverlayPreferences
        else
            let separator = source.IndexOf('|')
            let mutable schema = 0
            if separator < 2 || not (Int32.TryParse(source.Substring(1, separator - 1), &schema)) then
                Error MalformedOverlayPreferences
            elif schema <> OverlayPreferenceSchemaVersion then
                Error(UnsupportedOverlayPreferenceSchema schema)
            else
                let known = overlayRegistry |> Array.map (fun value -> TacticalOverlayId.value value.Id, value) |> Map.ofArray
                let entries = source.Substring(separator + 1).Split(';', StringSplitOptions.RemoveEmptyEntries)
                let folder (state: Result<Map<TacticalOverlayId, TacticalOverlayMode>, TacticalOverlayPreferenceDiagnostic>) (entry: string) =
                    state
                    |> Result.bind (fun modes ->
                        let parts = entry.Split('=')
                        if parts.Length <> 2 then Error MalformedOverlayPreferences
                        else
                            match Map.tryFind parts[0] known, tryMode parts[1] with
                            | Some item, Some mode when Set.contains mode item.SupportedModes -> Ok(Map.add item.Id mode modes)
                            | Some item, Some _ -> Ok(Map.add item.Id item.DefaultMode modes)
                            | None, Some _ -> Ok modes
                            | _, None -> Error MalformedOverlayPreferences)
                entries
                |> Array.fold folder (Ok initialOverlayPreferences.Modes)
                |> Result.map (fun modes -> { SchemaVersion = schema; Modes = modes })

    let effectiveOverlayMode preferences held hasSelection descriptor =
        let requested =
            if Set.contains descriptor.Id held && Set.contains InspectHeld descriptor.SupportedModes then InspectHeld
            elif hasSelection && Set.contains SelectionScoped descriptor.SupportedModes then
                preferences.Modes |> Map.tryFind descriptor.Id |> Option.defaultValue descriptor.DefaultMode
            else preferences.Modes |> Map.tryFind descriptor.Id |> Option.defaultValue descriptor.DefaultMode
        if requested = InspectHeld && not (Set.contains descriptor.Id held) then OverlayOff
        elif requested = SelectionScoped && not hasSelection then OverlayOff
        elif Set.contains requested descriptor.SupportedModes then requested
        else descriptor.DefaultMode

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
              Visual = copyVisual unit
              PresentationColumn = float unit.AnchorColumn
              PresentationRow = float unit.AnchorRow })

    let private effectKind (kind: string) =
        let normalized = kind.ToLowerInvariant()
        if normalized.Contains("move") then MovementEffect
        elif normalized.Contains("attack") || normalized.Contains("trace") || normalized.StartsWith("combat-") then AttackEffect
        elif normalized.Contains("impact") || normalized.Contains("damage") || normalized.Contains("wound") then ImpactEffect
        elif normalized.Contains("suppress") then SuppressionEffect
        elif normalized.Contains("heal") || normalized.Contains("recover") then RecoveryEffect
        elif normalized.Contains("communication") || normalized.Contains("acknowledg") || normalized.Contains("sensor") then SignalEffect
        elif normalized.Contains("objective") then ObjectiveEffect
        else GenericEffect

    let private effectLifecycle historical = function
        | _ when historical -> HistoricalEffect
        | PreviewEvent -> PreviewEffect
        | PredictedEvent -> PredictedEffect
        | AcceptedEvent -> AcceptedEffect
        | CommittedEvent -> CommittedEffect
        | RejectedEvent -> RejectedEffect
        | HistoricalEvent -> HistoricalEffect

    let private effectOrder = function
        | MovementEffect -> 10
        | SignalEffect -> 20
        | ObjectiveEffect -> 30
        | AttackEffect -> 40
        | ImpactEffect -> 50
        | SuppressionEffect -> 60
        | RecoveryEffect -> 70
        | GenericEffect -> 80

    let private effectsOfFrame historical (units: SceneUnitProjection array) (frame: RenderFrame) =
        let centers =
            units
            |> Array.map (fun unit ->
                let width = float (CellExtent.value unit.Visual.FootprintWidth)
                let depth = float (CellExtent.value unit.Visual.FootprintDepth)
                unit.Visual.Id, (unit.PresentationColumn + width / 2.0, unit.PresentationRow + depth / 2.0))
            |> Map.ofArray
        frame.Events
        |> Array.choose (fun event ->
            match event.Summary with
            | Disclosed label ->
                let sourceId = match event.SourceUnitId with Disclosed id when Map.containsKey id centers -> Some id | _ -> None
                let targetId = match event.TargetUnitId with Disclosed id when Map.containsKey id centers -> Some id | _ -> None
                let kind = effectKind event.Kind
                Some
                    { PrimitiveId = primitive "effect" (invariant event.Tick + ":" + invariant event.Id)
                      EventId = event.Id
                      Tick = event.Tick
                      Kind = kind
                      Lifecycle = effectLifecycle historical event.Lifecycle
                      SourceUnitId = sourceId
                      TargetUnitId = targetId
                      SourcePoint = sourceId |> Option.map (fun id -> centers[id])
                      TargetPoint = targetId |> Option.map (fun id -> centers[id])
                      Label = label
                      Order = effectOrder kind }
            | _ -> None)
        // Bound the current causal window, then restore deterministic paint order.
        |> Array.sortByDescending (fun effect -> effect.Tick, effect.EventId)
        |> Array.truncate MaximumEffectInstances
        |> Array.sortBy (fun effect -> effect.Tick, effect.Order, effect.EventId)

    let private routeEffects tick (routes: SceneRouteProjection array) =
        routes
        |> Array.mapi (fun index route ->
            let pointAt offset =
                if route.Points.Length >= offset + 2 then
                    Some(route.Points[offset], route.Points[offset + 1])
                else None
            let lifecycle =
                if route.Kind.Contains("collision", StringComparison.OrdinalIgnoreCase) then RejectedEffect
                elif route.Kind.Contains("preview", StringComparison.OrdinalIgnoreCase) then PreviewEffect
                else PredictedEffect
            { PrimitiveId = primitive "effect" ("route:" + ScenePrimitiveId.value route.PrimitiveId)
              EventId = -1 - int32 index
              Tick = tick
              Kind = MovementEffect
              Lifecycle = lifecycle
              SourceUnitId = route.OwnerUnitId
              TargetUnitId = None
              SourcePoint = pointAt 0
              TargetPoint = pointAt (route.Points.Length - 2)
              Label =
                match route.Label with
                | Disclosed label -> label
                | _ -> route.Kind
              Order = effectOrder MovementEffect })

    let private retainEffects (effects: TacticalEffectProjection array) =
        effects
        |> Array.distinctBy _.PrimitiveId
        |> Array.sortByDescending (fun effect -> effect.Tick, effect.EventId)
        |> Array.truncate MaximumEffectInstances
        |> Array.sortBy (fun effect -> effect.Tick, effect.Order, effect.EventId)

    let private visualCost
        (terrain: SceneTerrainProjection array)
        (edges: EdgeVisual array)
        (units: SceneUnitProjection array)
        (routes: SceneRouteProjection array)
        (annotations: SceneAnnotationProjection array)
        (effects: TacticalEffectProjection array)
        =
        let terrainNodes =
            terrain
            |> Array.sumBy (fun cell ->
                match cell.Kind with
                | "blocked" -> 3
                | "rough"
                | "objective" -> 2
                | _ -> 1)
        let unitNodes =
            units
            |> Array.sumBy (fun unit ->
                4
                + UnitGlyphCatalog.resolve(unit.Visual.ClassId).Primitives.Length
                + (match unit.Visual.BodyHeading with Disclosed _ -> 1 | _ -> 0)
                + (match unit.Visual.SecondaryHeading with Disclosed _ -> 1 | _ -> 0)
                + (match unit.Visual.StanceId with Disclosed _ -> 1 | _ -> 0))
        let effectNodes =
            effects
            |> Array.sumBy (fun effect ->
                1
                + (if effect.SourcePoint.IsSome && effect.TargetPoint.IsSome then 1 else 0)
                + (if effect.SourcePoint.IsSome || effect.TargetPoint.IsSome then 1 else 0))
        { UnitCount = units.Length
          EffectInstances = effects.Length
          EstimatedSvgNodes = 32 + terrainNodes + edges.Length + unitNodes + routes.Length + annotations.Length + effectNodes }

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
            let overlay =
                match event.Kind.ToLowerInvariant() with
                | kind when kind.Contains("attack") || kind.Contains("impact") -> Some(overlayId "combat.attack-traces")
                | kind when kind.Contains("suppress") -> Some(overlayId "combat.suppression")
                | kind when kind.Contains("wound") || kind.Contains("health") -> Some(overlayId "combat.hp-wounds")
                | kind when kind.Contains("command") -> Some(overlayId "command.state")
                | _ -> None
            { PrimitiveId =
                primitive prefix (invariant event.Id)
              Kind = event.Kind
              OverlayId = overlay
              SubjectUnitId =
                match event.TargetUnitId, event.SourceUnitId with
                | Disclosed id, _ -> Some id
                | _, Disclosed id -> Some id
                | _ -> None
              Column = None
              Row = None
              Geometry = None
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
              OverlayId = None
              SubjectUnitId = None
              Column = column
              Row = row
              Geometry = None
              Text = Disclosed("Region " + invariant id) })

    let editor (input: EditorProjectionInput) =
        let frame = MapEditor.frame input.EditorState
        let units = unitsOfFrame frame
        let terrain = terrainOfMap input.EditorState.Map
        let edges = frame.Edges |> Array.map copyEdge
        let annotations =
            Array.append
                (regionAnnotations input.EditorState.Map.Regions)
                (eventAnnotations "editor-event" frame.Events)
        let effects = effectsOfFrame false units frame
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
          Terrain = terrain
          Edges = edges
          Units = units
          Routes = [||]
          Annotations = annotations
          Effects = effects
          VisualCost = visualCost terrain edges units [||] annotations effects
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
        (commands: PlanningCommand list)
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
        let latest choose =
            commands
            |> List.choose (fun command ->
                if command.UnitId = member'.UnitId then choose command.Kind
                else None)
            |> List.tryLast
        let bodyHeading =
            latest (function
                | PlannedFacing direction -> Some direction
                | _ -> None)
            |> Option.map (HeadingRadians.ofDirection8 >> Disclosed)
            |> Option.defaultValue NotPresent
        let attentionHeading =
            latest (function
                | PlannedAttention direction -> Some direction
                | _ -> None)
            |> Option.map (fun direction ->
                Disclosed
                    { Radians = HeadingRadians.ofDirection8 direction
                      Source = AttentionHeading })
            |> Option.defaultValue NotPresent
        let stance =
            latest (function
                | PlannedStance value -> Some value
                | _ -> None)
            |> Option.map Disclosed
            |> Option.defaultValue NotPresent
        let statusIds =
            [| yield "planning"
               if commands |> List.exists (fun command -> command.UnitId = member'.UnitId && command.Kind = PlannedHold) then
                   yield "hold"
               if commands |> List.exists (fun command -> command.UnitId = member'.UnitId && match command.Kind with PlannedEngagement _ -> true | _ -> false) then
                   yield "engagement"
               if commands |> List.exists (fun command -> command.UnitId = member'.UnitId && match command.Kind with PlannedSynchronization _ -> true | _ -> false) then
                   yield "synchronization" |]
        { PrimitiveId = primitive "unit" (invariant member'.UnitId)
          PresentationColumn = float member'.Column
          PresentationRow = float member'.Row
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
              StanceId = stance
              BodyHeading = bodyHeading
              SecondaryHeading = attentionHeading
              ShortLabel = Disclosed member'.Name
              StatusIds = statusIds } }

    let private planningAnnotation
        (roster: PlanningRosterMember array)
        (command: PlanningCommand)
        : SceneAnnotationProjection
        =
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
        let owner = roster |> Array.tryFind (fun unit -> unit.UnitId = command.UnitId)
        let anchor = owner |> Option.map (fun unit -> float unit.Column + 0.5, float unit.Row + 0.5)
        { PrimitiveId = primitive "plan-command" command.Id
          Kind = kind
          OverlayId =
            match command.Kind with
            | PlannedFacing _ -> Some(overlayId "unit.body-facing")
            | PlannedAttention _ -> Some(overlayId "awareness.attention-vision")
            | PlannedEngagement _ -> Some(overlayId "combat.area-engagements")
            | PlannedHold
            | PlannedSynchronization _ -> Some(overlayId "command.state")
            | PlannedStance _
            | PlannedRoute _ -> None
          SubjectUnitId = Some command.UnitId
          Column = owner |> Option.map _.Column
          Row = owner |> Option.map _.Row
          Geometry =
            match command.Kind, anchor with
            | PlannedFacing direction, Some(x, y)
            | PlannedAttention direction, Some(x, y) ->
                Some(DirectionGeometry(x, y, HeadingRadians.ofDirection8 direction |> HeadingRadians.value, None))
            | PlannedHold, Some(x, y)
            | PlannedSynchronization _, Some(x, y) -> Some(StatusGeometry(x, y, None, None, [| kind |]))
            | _ -> None
          Text = Disclosed text }

    let private planningIssueAnnotation
        (state: PlanningWorkspaceState)
        index
        (issue: PlanningIssue)
        =
        let command =
            issue.CommandId
            |> Option.bind (fun commandId ->
                state.Commands |> List.tryFind (fun command -> command.Id = commandId))
        let unitId = issue.UnitId |> Option.orElse (command |> Option.map _.UnitId)
        let owner =
            unitId
            |> Option.bind (fun id -> state.Roster |> Array.tryFind (fun unit -> unit.UnitId = id))
        { PrimitiveId = primitive "planning-issue" (invariant index)
          Kind = "validation"
          OverlayId = None
          SubjectUnitId = unitId
          Column = owner |> Option.map _.Column
          Row = owner |> Option.map _.Row
          Geometry = None
          Text = Disclosed(issue.Code + " · " + issue.Detail) }

    let planning (input: PlanningProjectionInput) =
        let units =
            input.PlanningState.Roster
            |> Array.sortBy _.UnitId
            |> Array.map (planningUnit input.PlanningMap input.PlanningState.Commands)
        let authoredRoutes =
            input.PlanningState.Commands
            |> List.choose (fun command ->
                match command.Kind with
                | PlannedRoute cells ->
                    Some
                        { PrimitiveId = primitive "route" command.Id
                          OwnerUnitId = Some command.UnitId
                          OverlayId = overlayId "movement.planned-routes"
                          Kind = "planned"
                          Points = routePoints cells
                          MovementCost = None
                          BlockerIds = [||]
                          Label =
                            Disclosed(
                                "Planned route for unit "
                                + invariant command.UnitId
                            ) }
                | _ -> None)
            |> List.toArray
        let routes = authoredRoutes
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
        let terrain = terrainOfMap input.PlanningMap
        let edges = edgesOfMap input.PlanningMap
        let annotations =
            Array.concat
                [ input.PlanningState.Commands
                  |> List.filter (fun command ->
                      match command.Kind with
                      | PlannedRoute _ -> false
                      | _ -> true)
                  |> List.map (planningAnnotation input.PlanningState.Roster)
                  |> List.toArray
                  input.PlanningState.Issues
                  |> Array.mapi (planningIssueAnnotation input.PlanningState)
                  input.PlanningState.Predicted
                  |> Option.map (fun prediction ->
                      prediction.Disclosures
                      |> Array.mapi (fun index disclosure ->
                          { PrimitiveId = primitive "prediction" (string prediction.Revision + ":" + invariant index)
                            Kind = "prediction"
                            OverlayId = None
                            SubjectUnitId = None
                            Column = None
                            Row = None
                            Geometry = None
                            Text = Disclosed disclosure }))
                  |> Option.defaultValue [||] ]
        { Owner = PlanningScene
          RevisionIdentity =
            input.PlanningState.MapRevision
            + ":"
            + input.PlanningState.Digest
          Tick = input.PlanningState.AuthoringTick
          Board = boardOfMap input.PlanningMap
          Terrain = terrain
          Edges = edges
          Units = units
          Routes = routes
          Annotations = annotations
          Effects = [||]
          VisualCost = visualCost terrain edges units routes annotations [||]
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
        let normalized = overlay.Kind.ToLowerInvariant()
        { PrimitiveId = primitive "route" overlay.Id
          OwnerUnitId =
            match overlay.Scope with
            | SelectedUnitOverlay id -> Some id
            | WholeForceOverlay -> None
          OverlayId =
            if normalized.Contains("los") then overlayId "spatial.exact-los"
            elif normalized.Contains("reservation") then overlayId "movement.reservations"
            elif normalized.Contains("attack") || normalized.Contains("impact") then overlayId "combat.attack-traces"
            else overlayId "movement.planned-routes"
          Kind = overlay.Kind
          Points = Array.copy overlay.Points
          MovementCost = None
          BlockerIds = [||]
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
          OverlayId =
            if overlay.Kind.Contains("los", StringComparison.OrdinalIgnoreCase) then overlayId "spatial.exact-los"
            elif overlay.Kind.Contains("reservation", StringComparison.OrdinalIgnoreCase) then overlayId "movement.reservations"
            elif overlay.Kind.Contains("attack", StringComparison.OrdinalIgnoreCase) then overlayId "combat.attack-traces"
            else overlayId "movement.planned-routes"
          Kind = overlay.Kind
          Points = Array.copy overlay.Points
          MovementCost = None
          BlockerIds = [||]
          Label = overlay.Label }

    let simulator (input: SimulatorProjectionInput) =
        let frame =
            MapEditorSimulator.frame
                input.SimulatorSelectedUnit
                input.SimulatorHandoff
        let units =
            unitsOfFrame frame
            |> Array.map (fun projected ->
                let visual = projected.Visual
                let column, row =
                    input.SimulatorHandoff.PresentationPositions
                    |> Map.tryFind visual.Id
                    |> Option.defaultValue (
                        float visual.AnchorColumn,
                        float visual.AnchorRow
                    )
                let runtimeStatus =
                    [| yield! visual.StatusIds
                       if Map.containsKey visual.Id input.SimulatorHandoff.MovementProgress then
                           yield "moving"
                       if Map.containsKey visual.Id input.SimulatorHandoff.MovementIntents then
                           yield "movement-intent"
                       if Map.containsKey visual.Id input.SimulatorHandoff.PlannedRoutes then
                           yield "route-planned" |]
                { projected with
                    PresentationColumn = column
                    PresentationRow = row
                    Visual = { visual with StatusIds = runtimeStatus } })
        let selected, focused =
            selectedUnits
                (input.SimulatorSelectedUnit |> Option.toList)
                input.SimulatorFocusedUnit
                units
        let terrain = terrainOfMap input.SimulatorHandoff.RuntimeMap
        let edges = frame.Edges |> Array.map copyEdge
        let routes = frame.Overlays |> Array.map simulatorOverlayRoute
        let annotations =
            Array.append
                (units
                 |> Array.map (fun unit ->
                     let visual = unit.Visual
                     { PrimitiveId = primitive "simulator-state" (invariant visual.Id)
                       Kind = "simulator-state"
                       OverlayId = Some(overlayId "command.state")
                       SubjectUnitId = Some visual.Id
                       Column = Some visual.AnchorColumn
                       Row = Some visual.AnchorRow
                       Geometry = Some(StatusGeometry(unit.PresentationColumn + 0.5, unit.PresentationRow + 0.5, None, None, Array.copy visual.StatusIds))
                       Text = Disclosed("Unit " + invariant visual.Id + " · " + String.concat " · " visual.StatusIds) }))
                (eventAnnotations "simulator-event" frame.Events)
        let effects =
            Array.append (effectsOfFrame false units frame) (routeEffects frame.Tick routes)
            |> retainEffects
        { Owner = SimulatorScene
          RevisionIdentity = input.SimulatorHandoff.Revision.Digest
          Tick = input.SimulatorHandoff.Tick
          Board = frame.Board
          Terrain = terrain
          Edges = edges
          Units = units
          Routes = routes
          Annotations = annotations
          Effects = effects
          VisualCost = visualCost terrain edges units routes annotations effects
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
                  AcceptedVerificationIdentity =
                    metadata.SourceIdentity + " · " + metadata.EngineIdentity
                  AcceptedVerificationKind = "browser-kernel-verified"
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
                  AcceptedVerificationIdentity =
                    metadata.SourceIdentity
                    + " · "
                    + metadata.EngineIdentity
                    + " · "
                    + Option.get inspection.PerspectiveHash
                  AcceptedVerificationKind = "perspective-projection"
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
                  OverlayId = None
                  SubjectUnitId =
                    match overlay.Scope with
                    | SelectedUnitOverlay id -> Some id
                    | WholeForceOverlay -> None
                  Column = None
                  Row = None
                  Geometry = None
                  Text = overlay.Label })
        let eventAnnotations =
            eventAnnotations "review-event" frame.Events
        let verificationAnnotation =
            { PrimitiveId = primitive "review-verification" "accepted"
              Kind = input.AcceptedReview.AcceptedVerificationKind
              OverlayId = None
              SubjectUnitId = None
              Column = None
              Row = None
              Geometry = None
              Text =
                Disclosed(
                    "Verification · "
                    + input.AcceptedReview.AcceptedVerificationIdentity
                ) }
        let visibleEvents =
            frame.Events |> Array.map _.Id |> Set.ofArray
        let selectedEvent =
            input.AcceptedReview.AcceptedSelectedEvent
            |> Option.filter (fun id -> Set.contains id visibleEvents)
        let projectedRoutes = routes |> Array.map overlayRoute
        let projectedAnnotations =
            Array.concat [| overlayAnnotations; eventAnnotations; [| verificationAnnotation |] |]
        let edges = frame.Edges |> Array.map copyEdge
        let effects = effectsOfFrame true units frame
        { Owner = ReviewScene
          RevisionIdentity = input.AcceptedReview.AcceptedRevisionIdentity
          Tick = frame.Tick
          Board = frame.Board
          Terrain = [||]
          Edges = edges
          Units = units
          Routes = projectedRoutes
          Annotations = projectedAnnotations
          Effects = effects
          VisualCost = visualCost [||] edges units projectedRoutes projectedAnnotations effects
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

    /// Interpolates only presentation coordinates for semantic units present
    /// in both accepted Review frames. Current committed identity, tick,
    /// disclosure, visual facts, events, annotations, and selection remain the
    /// authoritative projection. A failed guard returns the exact current
    /// projection with an effective alpha of one.
    let interpolateReviewPresentation
        (previousFrame: RenderFrame)
        (alpha: float)
        (current: SharedSceneProjection)
        =
        let currentIds =
            current.Units |> Array.map _.Visual.Id |> Set.ofArray
        let previousIds =
            previousFrame.Units |> Array.map _.Id |> Set.ofArray
        let valid =
            current.Owner = ReviewScene
            && previousFrame.Tick <> current.Tick
            && previousFrame.Board = current.Board
            && previousFrame.Disclosure = current.Disclosure.Source
            && currentIds = previousIds
            && currentIds.Count = current.Units.Length
            && previousIds.Count = previousFrame.Units.Length
            && not (Double.IsNaN alpha || Double.IsInfinity alpha)

        if not valid then current, 1.0
        else
            let effectiveAlpha = max 0.0 (min 1.0 alpha)
            let previousById =
                previousFrame.Units
                |> Array.map (fun unit -> unit.Id, unit)
                |> Map.ofArray
            let units =
                current.Units
                |> Array.map (fun unit ->
                    let previous = previousById[unit.Visual.Id]
                    { unit with
                        PresentationColumn =
                            float previous.AnchorColumn
                            + (unit.PresentationColumn - float previous.AnchorColumn)
                              * effectiveAlpha
                        PresentationRow =
                            float previous.AnchorRow
                            + (unit.PresentationRow - float previous.AnchorRow)
                              * effectiveAlpha })
            { current with Units = units }, effectiveAlpha

    let private disclosed = function
        | Disclosed _ -> true
        | NotPresent
        | NotApplicable
        | ExplicitlyUnknown -> false

    let private overlayDescriptor id =
        overlayRegistry
        |> Array.find (fun item -> TacticalOverlayId.value item.Id = id)

    let private geometryPoints = function
        | FootprintGeometry(x, y, _, _)
        | DirectionGeometry(x, y, _, _)
        | AreaGeometry(x, y, _)
        | StatusGeometry(x, y, _, _, _) -> [| x; y |]
        | PathGeometry(points, _, _)
        | TraceGeometry(points, _, _) -> Array.copy points

    let private geometryNodeCount = function
        | FootprintGeometry _
        | DirectionGeometry _
        | AreaGeometry _
        | StatusGeometry _ -> 2
        | PathGeometry(_, _, blockers) -> 2 + blockers.Length
        | TraceGeometry _ -> 3

    let private geometryMatches payloadKind geometry =
        match payloadKind, geometry with
        | FootprintPayload, FootprintGeometry _
        | DirectionPayload, DirectionGeometry _
        | PolylinePayload, PathGeometry _
        | AreaPayload, AreaGeometry _
        | TracePayload, TraceGeometry _
        | StatusPayload, StatusGeometry _ -> true
        | _ -> false

    let private overlayPayload descriptor primitiveId subject tick kind geometry label priority =
        { OverlayId = descriptor.Id
          PrimitiveId = primitiveId
          SubjectId = subject
          Tick = tick
          Kind = kind
          PayloadKind = descriptor.PayloadKind
          Geometry = geometry
          Points = geometryPoints geometry
          Label = label
          Priority = priority
          Order = descriptor.Order }

    let projectOverlays preferences held (projection: SharedSceneProjection) =
        // A malformed authority/disclosure envelope is deliberately indistinguishable
        // from an undisclosed one: payload construction never starts.
        if not projection.Disclosure.PreservesFieldDisclosures then
            { Payloads = [||]
              Labels = [||]
              Cost =
                { RegistryTraversals = 1
                  DisclosurePasses = 1
                  CandidatePayloads = 0
                  EmittedPayloads = 0
                  EmittedLabels = 0
                  EstimatedSvgNodes = 0 } }
        else
            let selected = projection.Selection.SelectedUnits |> Set.ofArray
            let hasSelection = not selected.IsEmpty
            let enabled =
                overlayRegistry
                |> Array.choose (fun item ->
                    let mode = effectiveOverlayMode preferences held hasSelection item
                    if mode = OverlayOff then None else Some(item.Id, (item, mode)))
                |> Map.ofArray
            let accepts id subject =
                enabled
                |> Map.tryFind (overlayId id)
                |> Option.filter (fun (descriptor, mode) ->
                    descriptor.Availability = OverlayAvailable
                    && (mode <> SelectionScoped || subject |> Option.exists (fun value -> Set.contains value selected))
                    && (descriptor.DisclosurePolicy = DisclosedSceneFactsOnly
                        || subject |> Option.exists (fun value -> Set.contains value selected)))
            let priority id subject =
                let heldPriority = if Set.contains id held then 0 else 2
                let selectionPriority = if subject |> Option.exists (fun value -> Set.contains value selected) then 0 else 1
                heldPriority + selectionPriority
            let unitPayloads =
                projection.Units
                |> Array.collect (fun unit ->
                    let subject = string unit.Visual.Id
                    let footprintWidth = float (CellExtent.value unit.Visual.FootprintWidth)
                    let footprintDepth = float (CellExtent.value unit.Visual.FootprintDepth)
                    let x = unit.PresentationColumn + footprintWidth / 2.0
                    let y = unit.PresentationRow + footprintDepth / 2.0
                    let direction overlay kind heading label =
                        match accepts overlay (Some unit.Visual.Id), heading with
                        | Some(descriptor, _), Disclosed value ->
                            Some(overlayPayload descriptor unit.PrimitiveId subject projection.Tick kind (DirectionGeometry(x, y, value, None)) (Disclosed label) (priority descriptor.Id (Some unit.Visual.Id)))
                        | _ -> None
                    [| match accepts "unit.footprints" (Some unit.Visual.Id) with
                       | Some(descriptor, _) ->
                           yield overlayPayload descriptor unit.PrimitiveId subject projection.Tick "footprint" (FootprintGeometry(x, y, footprintWidth, footprintDepth)) (Disclosed "Footprint") (priority descriptor.Id (Some unit.Visual.Id))
                       | None -> ()
                       yield! direction "unit.body-facing" "body-facing" (unit.Visual.BodyHeading |> function Disclosed h -> Disclosed(HeadingRadians.value h) | NotPresent -> NotPresent | NotApplicable -> NotApplicable | ExplicitlyUnknown -> ExplicitlyUnknown) "Body facing" |> Option.toArray
                       yield! direction "awareness.attention-vision" "attention-vision" (unit.Visual.SecondaryHeading |> function Disclosed h -> Disclosed(HeadingRadians.value h.Radians) | NotPresent -> NotPresent | NotApplicable -> NotApplicable | ExplicitlyUnknown -> ExplicitlyUnknown) "Attention and vision" |> Option.toArray
                       match accepts "combat.hp-wounds" (Some unit.Visual.Id), unit.Visual.Health with
                       | Some(descriptor, _), Disclosed health ->
                           yield overlayPayload descriptor unit.PrimitiveId subject projection.Tick "hp-wounds" (StatusGeometry(x, y, Some(HealthVisual.remaining health), Some(HealthVisual.maximum health), unit.Visual.StatusIds |> Array.filter (fun token -> token.Contains("wound", StringComparison.OrdinalIgnoreCase)))) (Disclosed("Health " + string (HealthVisual.remaining health) + "/" + string (HealthVisual.maximum health))) (priority descriptor.Id (Some unit.Visual.Id))
                       | _ -> ()
                       let suppression = unit.Visual.StatusIds |> Array.filter (fun token -> token.StartsWith("suppression", StringComparison.OrdinalIgnoreCase))
                       match accepts "combat.suppression" (Some unit.Visual.Id) with
                       | Some(descriptor, _) when suppression.Length > 0 ->
                           yield overlayPayload descriptor unit.PrimitiveId subject projection.Tick "suppression" (StatusGeometry(x, y, None, None, suppression)) (Disclosed(String.concat ", " suppression)) (priority descriptor.Id (Some unit.Visual.Id))
                       | _ -> ()
                       if unit.Visual.StatusIds.Length > 0 then
                           match accepts "command.state" (Some unit.Visual.Id) with
                           | Some(descriptor, _) ->
                               yield overlayPayload descriptor unit.PrimitiveId subject projection.Tick "command-state" (StatusGeometry(x, y, None, None, Array.copy unit.Visual.StatusIds)) (Disclosed(String.concat ", " unit.Visual.StatusIds)) (priority descriptor.Id (Some unit.Visual.Id))
                           | None -> () |])
            let routePayloads =
                projection.Routes
                |> Array.collect (fun route ->
                    let subject = route.OwnerUnitId |> Option.map string |> Option.defaultValue "force"
                    let emit id geometry =
                        match accepts id route.OwnerUnitId with
                        | Some(descriptor, _) when disclosed route.Label && geometryMatches descriptor.PayloadKind geometry -> Some(overlayPayload descriptor route.PrimitiveId subject projection.Tick route.Kind geometry route.Label (priority descriptor.Id route.OwnerUnitId))
                        | _ -> None
                    emit (TacticalOverlayId.value route.OverlayId) (PathGeometry(Array.copy route.Points, route.MovementCost, Array.copy route.BlockerIds))
                    |> Option.toArray)
            let annotationPayloads =
                projection.Annotations
                |> Array.choose (fun annotation ->
                    annotation.OverlayId
                    |> Option.bind (fun id ->
                        match accepts (TacticalOverlayId.value id) annotation.SubjectUnitId with
                        | Some(descriptor, _) when disclosed annotation.Text && (annotation.Geometry |> Option.exists (geometryMatches descriptor.PayloadKind)) ->
                            let geometry = annotation.Geometry.Value
                            Some(overlayPayload descriptor annotation.PrimitiveId (annotation.SubjectUnitId |> Option.map string |> Option.defaultValue (ScenePrimitiveId.value annotation.PrimitiveId)) projection.Tick annotation.Kind geometry annotation.Text (priority descriptor.Id annotation.SubjectUnitId))
                        | _ -> None))
            let candidates =
                Array.concat [ unitPayloads; routePayloads; annotationPayloads ]
                |> Array.sortBy (fun item -> item.Priority, item.Order, item.SubjectId, ScenePrimitiveId.value item.PrimitiveId)
            let payloads =
                let bounded = ResizeArray<TacticalOverlayPayload>()
                let mutable nodes = 0
                for candidate in candidates do
                    let candidateNodes = geometryNodeCount candidate.Geometry
                    if bounded.Count < MaximumOverlayPayloads && nodes + candidateNodes <= 4744 then
                        bounded.Add candidate
                        nodes <- nodes + candidateNodes
                bounded.ToArray()
            let labels =
                payloads
                |> Array.filter (fun item -> disclosed item.Label)
                |> Array.groupBy (fun item ->
                    if item.Points.Length >= 2 then int (Math.Floor item.Points[0]), int (Math.Floor item.Points[1])
                    else item.Order, 0)
                |> Array.map (snd >> Array.head)
                |> Array.sortBy (fun item -> item.Priority, item.Order, item.SubjectId, ScenePrimitiveId.value item.PrimitiveId)
                |> Array.truncate MaximumOverlayLabels
            { Payloads = payloads
              Labels = labels
              Cost =
                { RegistryTraversals = 1
                  DisclosurePasses = 1
                  CandidatePayloads = candidates.Length
                  EmittedPayloads = payloads.Length
                  EmittedLabels = labels.Length
                  EstimatedSvgNodes = (payloads |> Array.sumBy (fun payload -> geometryNodeCount payload.Geometry)) + labels.Length } }

    let primitiveIds (projection: SharedSceneProjection) =
        [| yield! projection.Terrain |> Array.map _.PrimitiveId
           yield!
               projection.Edges
               |> Array.map (fun edge -> primitive "edge" edge.Id)
           yield! projection.Units |> Array.map _.PrimitiveId
           yield! projection.Routes |> Array.map _.PrimitiveId
           yield! projection.Effects |> Array.map _.PrimitiveId
           yield! projection.Annotations |> Array.map _.PrimitiveId
           yield! projection.Layers |> Array.map _.PrimitiveId |]
