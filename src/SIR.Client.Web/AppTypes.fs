module SIR.Client.Web.AppTypes

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
open SIR.Client.Web
type Msg =
    | ShellMsg of SIR.Client.Msg
    | BattlefieldChanged of BattlefieldAction
    | FileSelected of File
    | ReplayReadCompleted of Result<string * byte array, string>
    | MapFileSelected of File
    | MapReadCompleted of Result<string * string, string>
    | MapTextRead of sourceName: string * text: string
    | BackgroundFileSelected of File
    | BackgroundReadCompleted of Result<string * string * byte array, string>
    | BackgroundBytesRead of fileName: string * mediaType: string * bytes: byte array
    | AcceptInterchangeReview
    | RejectInterchangeReview
    | PlaybackPulse
    | TacticalTimeChanged of int64
    | TacticalTimeStepped of int64
    | TacticalPlaybackToggled
    | TacticalPulse
    | ToggleTacticalBindings
    | TacticalBindingDraftChanged of commandId: string * gesture: string
    | ApplyTacticalBinding of commandId: string * replaceConflict: bool
    | ClearTacticalBinding of commandId: string
    | RestoreTacticalBinding of commandId: string
    | RestoreTacticalModalityBindings
    | RestoreAllTacticalBindings
    | CycleTacticalOverlay of string
    | BeginTacticalOverlayHold of string
    | EndTacticalOverlayHold of string
    | TacticalBindingImportChanged of string
    | ImportTacticalBindings
    | ToggleLayoutPanelVisibility of string
    | ToggleLayoutPanelCollapsed of string
    | MoveLayoutPanel of panelId: string * side: SidebarSide
    | ReorderLayoutPanel of panelId: string * delta: int
    | ToggleLayoutDrawer of SidebarSide
    | ToggleLayoutBottomPanelVisibility
    | ToggleLayoutBottomPanel
    | BeginLayoutBottomPanelResize
    | ResizeLayoutBottomPanel of height: int
    | EndLayoutBottomPanelResize
    | ResizeLayoutBottomPanelKeyboard of delta: int
    | OpenSupportingPanel of panelId: string
    | ClientFeatureMessage of FeatureLoader.Message
    | ResetTacticalLayout
    | ToggleDesktopToolbarCustomization
    | AddDesktopToolbarCommand of string
    | RemoveDesktopToolbarCommand of string
    | ReorderDesktopToolbarCommand of commandId: string * delta: int
    | ResetDesktopToolbar
    | InvokeTacticalCommand of string
    | InvokeTacticalValueCommand of commandId: string * value: string
    | ExecuteTacticalCommand of string
    | ExecuteModalCommand of ModalCommand
    | EditorPulse
    | SimulatorChanged of SimulatorAction
    | SimulatorUnitSelectionChanged of int32 option
    | BeginSimulatorControllerSelection
    | ChooseSimulatorController of MapController
    | CommitSimulatorControllerSelection
    | CancelSimulatorControllerSelection
    | RequestSimulatorReset
    | ResetSimulator
    | PlanningChanged of PlanningAction
    | InitializePlanningWorker
    | ValidatePlanningRevision
    | PreviewPlanningRevision
    | CommitPlanningRevision
    | PlanningWorkerResponded of SimulatorResponseEnvelope
    | ExportPlanningReview
    | LoadMapSample of editor: MapEditorState * simulator: SimulatorHandoff option
    | LoadSimulationSample of editor: MapEditorState * simulator: SimulatorHandoff option
    | LoadReplaySample of identity: string * title: string * frames: InspectionProjection array
    | KeyPressed of
        key: string *
        controlOrMeta: bool *
        shift: bool *
        alt: bool *
        repeat: bool
    | KeyReleased of string
    | ToggleInputHelp of focusPanel: bool
    | ApplicationFocusLost
    | WorkspaceChanged of WorkspaceMode
    | DocumentationLoaded of Result<DocumentationManifest, string>
    | DocumentationQueryChanged of string
    | DocumentationPageOpened of slug: string * anchor: string option
    | ContextualDocumentationOpened of concept: string
    | DocumentationBack
    | DocumentationForward
    | DocumentationExternalResult of string
    | EditorToolPanelChanged of EditorToolPanel
    | ToggleEditorToolPanelVisibility
    | EditorWorkspaceChanged of EditorWorkspaceAction
    | RecallEditorView of string
    | EditorChanged of MapEditorAction
    | TacticalParcelChanged of TacticalParcelEditor.TacticalParcelEditorAction
    | TacticalParcelImportTextChanged of string
    | ImportTacticalParcelDocument
    | ExportTacticalParcelDocument
    | ExportMap
    | ExportDesignBundle
    | ExportExperiment
    | AddComparisonBookmark
    | ComparisonViewChanged of ComparisonView
    | ExportEvidenceSvg
    | ExportEvidencePng
    | LiveStarted
    | LiveAction of LiveSession.Action
    | AdvanceLiveSession
    | DisconnectLiveSession
    | ReconnectLiveSession

and WorkspaceMode =
    | SimulatorWorkspace
    | PlanningWorkspace
    | EditorWorkspace
    | ReplayWorkspace
    | DocsWorkspace

and EditorToolPanel =
    | TerrainTools
    | UnitTools
    | EdgeTools
    | ZoneTools
    | TacticalEnvironmentTools
    | DocumentTools

type Model =
    { Shell: SIR.Client.Model
      Editor: MapEditorState
      TacticalParcelEditor: TacticalParcelEditor.TacticalParcelEditorState
      TacticalParcelImportText: string
      Simulator: SimulatorHandoff option
      SimulatorSelectedUnit: int32 option
      SimulatorControllerSelection: MapController option
      Planning: PlanningWorkspaceState option
      Tactical: TacticalTimelineState
      TacticalBindings: TacticalBindingProfile
      TacticalBindingDrafts: Map<string, string>
      TacticalBindingImport: string
      TacticalBindingDiagnostics: string list
      TacticalBindingsOpen: bool
      TacticalOverlays: TacticalOverlayPreferences
      HeldTacticalOverlays: Set<TacticalOverlayId>
      TacticalLayout: TacticalLayoutProfile
      TacticalLayoutDiagnostics: string list
      ClientFeatures: Map<FeatureLoader.FeatureId, FeatureLoader.LoadState>
      FeatureLoaderDiagnostic: string option
      DesktopToolbarCommands: string list
      DesktopToolbarCustomizationOpen: bool
      BottomPanelResizeActive: bool
      TacticalSelectedUnit: int32 option
      Workspace: WorkspaceMode
      LastTacticalWorkspace: WorkspaceMode
      Documentation: DocumentationManifest option
      DocumentationNavigation: DocumentationNavigation
      DocumentationError: string option
      DocumentationExternalAnnouncement: string
      EditorToolPanel: EditorToolPanel
      EditorToolPanelVisible: bool
      SampleReplayFrames: InspectionProjection array option
      EditorView: EditorWorkspaceState
      HeldInputs: HeldInputSession
      InputHelpExpanded: bool
      PendingInterchangeReview: InterchangeReview option
      ImportAnnouncement: string option
      Battlefield: BattlefieldViewState
      PreviousFrame: RenderFrame option
      PresentationAlpha: float
      ComparisonBookmarks: ComparisonBookmark list
      ComparisonView: ComparisonView
      Live: LiveSession.State }
