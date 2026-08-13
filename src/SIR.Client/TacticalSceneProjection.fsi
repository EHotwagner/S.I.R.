namespace SIR.Client

type SceneProjectionOwner =
    | EditorScene
    | PlanningScene
    | SimulatorScene
    | ReviewScene

type ScenePrimitiveId = private ScenePrimitiveId of string

[<RequireQualifiedAccess>]
module ScenePrimitiveId =
    val value: ScenePrimitiveId -> string

type TacticalOverlayId = private TacticalOverlayId of string

[<RequireQualifiedAccess>]
module TacticalOverlayId =
    val value: TacticalOverlayId -> string

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

type TacticalOverlayPayload =
    { OverlayId: TacticalOverlayId
      PrimitiveId: ScenePrimitiveId
      SubjectId: string
      Tick: int32
      Kind: string
      PayloadKind: TacticalOverlayPayloadKind
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

type AcceptedReviewProjection

type ReviewProjectionInput =
    { AcceptedReview: AcceptedReviewProjection
      ReviewCamera: BattlefieldCamera
      ReviewFocusedUnit: int32 option }

[<RequireQualifiedAccess>]
module TacticalSceneProjection =
    val overlayRegistry: TacticalOverlayDescriptor array
    val initialOverlayPreferences: TacticalOverlayPreferences
    val exportOverlayPreferences: TacticalOverlayPreferences -> string
    val importOverlayPreferences: string -> Result<TacticalOverlayPreferences, TacticalOverlayPreferenceDiagnostic>
    val effectiveOverlayMode:
        preferences: TacticalOverlayPreferences ->
        held: Set<TacticalOverlayId> ->
        hasSelection: bool ->
        descriptor: TacticalOverlayDescriptor ->
            TacticalOverlayMode
    val projectOverlays:
        preferences: TacticalOverlayPreferences ->
        held: Set<TacticalOverlayId> ->
        projection: SharedSceneProjection ->
            TacticalOverlayProjection
    val editor: EditorProjectionInput -> SharedSceneProjection
    val planning: PlanningProjectionInput -> SharedSceneProjection
    val simulator: SimulatorProjectionInput -> SharedSceneProjection
    val acceptReview: Model -> AcceptedReviewProjection option
    val review: ReviewProjectionInput -> SharedSceneProjection
    val interpolateReviewPresentation:
        previousFrame: RenderFrame ->
        alpha: float ->
        current: SharedSceneProjection ->
            SharedSceneProjection * float
    val primitiveIds: SharedSceneProjection -> ScenePrimitiveId array
