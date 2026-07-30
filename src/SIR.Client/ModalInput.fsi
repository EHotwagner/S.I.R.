namespace SIR.Client

/// A layout-sensitive key value normalized from KeyboardEvent.key. The
/// optional physical code is retained for diagnostics, not binding identity.
type NormalizedKey

[<RequireQualifiedAccess>]
module NormalizedKey =
    val create: key: string -> physicalCode: string option -> NormalizedKey
    val value: key: NormalizedKey -> string
    val physicalCode: key: NormalizedKey -> string option
    val sameProducedKey: left: NormalizedKey -> right: NormalizedKey -> bool

[<StructuralEquality; StructuralComparison>]
type KeyModifiers =
    { ControlOrMeta: bool
      Shift: bool
      Alt: bool }

[<RequireQualifiedAccess>]
module KeyModifiers =
    val none: KeyModifiers

type InputPhase =
    | KeyDown
    | KeyUp

type RepeatPolicy =
    | IgnoreRepeat
    | AllowRepeat

type InputGesture =
    { Key: NormalizedKey
      Modifiers: KeyModifiers
      Phase: InputPhase }

type EditorGestureKind =
    | BoxSelection
    | CommandPreview
    | UnitMovePreview
    | TerrainPreview of TerrainAuthoringTool
    | EdgePolyline

type ModalContext =
    | EditorBase
    | EditorDomain of EditorDomain
    | EditorTool of MapEditorTool
    | EditorGesture of EditorGestureKind
    | EditorPanHeld
    | EditorDestructiveConfirmation
    | SimulatorBase
    | SimulatorPaused
    | SimulatorRunning
    | SimulatorRoutePreview
    | SimulatorRevisionStale
    | SimulatorNoHandoff
    | InputHelpPopup

type ModalContextSelector =
    | AnyEditorContext
    | AnySimulatorContext
    | ExactContext of ModalContext

type ModalPrecedence =
    | WorkspaceCommands
    | ActiveTool
    | ActiveGestureOrPreview
    | HeldLayer
    | TransientPopup
    | InputPopup

type BindingAvailability =
    | Available
    | Unavailable of reason: string

type SimulatorPanel =
    | ControllerPanel
    | EventPanel
    | SimulatorSamplePanel

type ModalCommand =
    | EditorCommand of MapEditorAction
    | EditorWorkspaceCommand of EditorWorkspaceAction
    | ChooseEditorDomain of EditorDomain
    | ToggleEditorCommandPanel
    | ChooseSimulatorPanel of SimulatorPanel
    | ToggleSimulatorCommandPanel
    | SimulatorCommand of SimulatorAction
    | SetEditorPanHeld of bool
    | ToggleInputHelp

type ModalBinding<'command> =
    { Id: string
      Context: ModalContextSelector
      Precedence: ModalPrecedence
      BindingGesture: InputGesture
      Label: string
      Group: string
      Repeat: RepeatPolicy
      Availability: ModalContext list -> BindingAvailability
      Command: 'command }

type PossibleInput<'command> =
    { Id: string
      InputGesture: InputGesture
      Label: string
      Group: string
      Availability: BindingAvailability
      Command: 'command }

type InputResolution<'command> =
    | Resolved of PossibleInput<'command>
    | NoMatch
    | NoAvailableMatch of PossibleInput<'command> list

type CatalogDiagnostic =
    | DuplicateBindingId of id: string
    | EqualPrecedenceGestureConflict of
        firstId: string *
        secondId: string *
        precedence: ModalPrecedence *
        gesture: InputGesture

type EditorModalFacts =
    { Editor: MapEditorState
      ActiveDomain: EditorDomain
      PanHeld: bool
      InputHelpExpanded: bool }

type SimulatorModalFacts =
    { SimulatorHandoffPresent: bool
      SimulatorIsRunning: bool
      SimulatorHasRoutePreview: bool
      SimulatorRevisionIsStale: bool
      InputHelpExpanded: bool }

[<RequireQualifiedAccess>]
module ModalInput =
    val deriveEditorContexts: facts: EditorModalFacts -> ModalContext list
    val deriveSimulatorContexts: facts: SimulatorModalFacts -> ModalContext list

    val resolve:
        contexts: ModalContext list ->
        gesture: InputGesture ->
        isRepeat: bool ->
        catalog: ModalBinding<'command> list ->
        InputResolution<'command>

    val possibleInputs:
        contexts: ModalContext list ->
        catalog: ModalBinding<'command> list ->
        PossibleInput<'command> list

    val validateCatalog:
        catalog: ModalBinding<'command> list -> CatalogDiagnostic list
