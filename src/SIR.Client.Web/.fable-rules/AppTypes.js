
import { Record, Union } from "./fable_modules/fable-library-js.5.13.0/Types.js";
import { InspectionProjection_$reflection, Model_$reflection as Model_$reflection_1, Msg_$reflection as Msg_$reflection_1 } from "./SIR.Client/Shell.js";
import { BattlefieldViewState_$reflection, BattlefieldAction_$reflection } from "./SIR.Client/Battlefield.js";
import { record_type, float64_type, list_type, option_type, int32_type, bool_type, int64_type, union_type, tuple_type, array_type, uint8_type, string_type, class_type } from "./fable_modules/fable-library-js.5.13.0/Reflection.js";
import { FSharpResult$2 } from "./fable_modules/fable-library-js.5.13.0/Result.js";
import { TacticalLayoutProfile_$reflection, SidebarSide_$reflection } from "./SIR.Client/TacticalWorkspaceLayout.js";
import { HeldInputSession_$reflection, ModalCommand_$reflection } from "./SIR.Client/ModalInput.js";
import { SimulatorHandoff_$reflection, SimulatorAction_$reflection } from "./SIR.Client/MapEditorSimulator.js";
import { MapEditorState_$reflection, MapEditorAction_$reflection, MapController_$reflection } from "./SIR.Client/MapEditorTypes.js";
import { PlanningWorkspaceState_$reflection, PlanningAction_$reflection } from "./SIR.Client/PlanningWorkspace.js";
import { SimulatorResponseEnvelope_$reflection } from "./SIR.Client/SimulatorWorkerProtocol.js";
import { EditorWorkspaceState_$reflection, EditorWorkspaceAction_$reflection } from "./SIR.Client/MapEditorWorkspace.js";
import { ComparisonBookmark_$reflection, ComparisonView_$reflection } from "./SIR.Client/Comparison.js";
import { State_$reflection, Action_$reflection } from "./LiveSession.js";
import { TacticalBindingProfile_$reflection, TacticalTimelineState_$reflection } from "./SIR.Client/UnifiedTacticalWorkspace.js";
import { InterchangeReview_$reflection } from "./SIR.Client/MapEditorInterchange.js";
import { RenderFrame_$reflection } from "./SIR.Client/ReplayPresentation.js";

export class Msg extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ShellMsg", "BattlefieldChanged", "FileSelected", "ReplayReadCompleted", "MapFileSelected", "MapReadCompleted", "MapTextRead", "BackgroundFileSelected", "BackgroundReadCompleted", "BackgroundBytesRead", "AcceptInterchangeReview", "RejectInterchangeReview", "PlaybackPulse", "TacticalTimeChanged", "TacticalTimeStepped", "TacticalPlaybackToggled", "TacticalPulse", "ToggleTacticalBindings", "TacticalBindingDraftChanged", "ApplyTacticalBinding", "ClearTacticalBinding", "RestoreTacticalBinding", "RestoreTacticalModalityBindings", "RestoreAllTacticalBindings", "TacticalBindingImportChanged", "ImportTacticalBindings", "ToggleLayoutPanelVisibility", "ToggleLayoutPanelCollapsed", "MoveLayoutPanel", "ReorderLayoutPanel", "ToggleLayoutDrawer", "ToggleLayoutBottomPanelVisibility", "ToggleLayoutBottomPanel", "BeginLayoutBottomPanelResize", "ResizeLayoutBottomPanel", "EndLayoutBottomPanelResize", "ResizeLayoutBottomPanelKeyboard", "OpenSupportingPanel", "ResetTacticalLayout", "ToggleDesktopToolbarCustomization", "AddDesktopToolbarCommand", "RemoveDesktopToolbarCommand", "ReorderDesktopToolbarCommand", "ResetDesktopToolbar", "InvokeTacticalCommand", "InvokeTacticalValueCommand", "ExecuteTacticalCommand", "ExecuteModalCommand", "EditorPulse", "SimulatorChanged", "SimulatorUnitSelectionChanged", "BeginSimulatorControllerSelection", "ChooseSimulatorController", "CommitSimulatorControllerSelection", "CancelSimulatorControllerSelection", "RequestSimulatorReset", "ResetSimulator", "PlanningChanged", "InitializePlanningWorker", "ValidatePlanningRevision", "PreviewPlanningRevision", "CommitPlanningRevision", "PlanningWorkerResponded", "ExportPlanningReview", "LoadMapSample", "LoadSimulationSample", "LoadReplaySample", "KeyPressed", "KeyReleased", "ToggleInputHelp", "ApplicationFocusLost", "WorkspaceChanged", "EditorToolPanelChanged", "ToggleEditorToolPanelVisibility", "EditorWorkspaceChanged", "RecallEditorView", "EditorChanged", "ExportMap", "ExportDesignBundle", "ExportExperiment", "AddComparisonBookmark", "ComparisonViewChanged", "ExportEvidenceSvg", "ExportEvidencePng", "LiveStarted", "LiveAction", "AdvanceLiveSession", "DisconnectLiveSession", "ReconnectLiveSession"];
    }
    static AcceptInterchangeReview = new Msg(10, []);
    static RejectInterchangeReview = new Msg(11, []);
    static PlaybackPulse = new Msg(12, []);
    static TacticalPlaybackToggled = new Msg(15, []);
    static TacticalPulse = new Msg(16, []);
    static ToggleTacticalBindings = new Msg(17, []);
    static RestoreTacticalModalityBindings = new Msg(22, []);
    static RestoreAllTacticalBindings = new Msg(23, []);
    static ImportTacticalBindings = new Msg(25, []);
    static ToggleLayoutBottomPanelVisibility = new Msg(31, []);
    static ToggleLayoutBottomPanel = new Msg(32, []);
    static BeginLayoutBottomPanelResize = new Msg(33, []);
    static EndLayoutBottomPanelResize = new Msg(35, []);
    static ResetTacticalLayout = new Msg(38, []);
    static ToggleDesktopToolbarCustomization = new Msg(39, []);
    static ResetDesktopToolbar = new Msg(43, []);
    static EditorPulse = new Msg(48, []);
    static BeginSimulatorControllerSelection = new Msg(51, []);
    static CommitSimulatorControllerSelection = new Msg(53, []);
    static CancelSimulatorControllerSelection = new Msg(54, []);
    static RequestSimulatorReset = new Msg(55, []);
    static ResetSimulator = new Msg(56, []);
    static InitializePlanningWorker = new Msg(58, []);
    static ValidatePlanningRevision = new Msg(59, []);
    static PreviewPlanningRevision = new Msg(60, []);
    static CommitPlanningRevision = new Msg(61, []);
    static ExportPlanningReview = new Msg(63, []);
    static ApplicationFocusLost = new Msg(70, []);
    static ToggleEditorToolPanelVisibility = new Msg(73, []);
    static ExportMap = new Msg(77, []);
    static ExportDesignBundle = new Msg(78, []);
    static ExportExperiment = new Msg(79, []);
    static AddComparisonBookmark = new Msg(80, []);
    static ExportEvidenceSvg = new Msg(82, []);
    static ExportEvidencePng = new Msg(83, []);
    static LiveStarted = new Msg(84, []);
    static AdvanceLiveSession = new Msg(86, []);
    static DisconnectLiveSession = new Msg(87, []);
    static ReconnectLiveSession = new Msg(88, []);
}

export function Msg_$reflection() {
    return union_type("SIR.Client.Web.AppTypes.Msg", [], Msg, () => [[["Item", Msg_$reflection_1()]], [["Item", BattlefieldAction_$reflection()]], [["Item", class_type("Browser.Types.File", undefined)]], [["Item", union_type("Microsoft.FSharp.Core.FSharpResult`2", [tuple_type(string_type, array_type(uint8_type)), string_type], FSharpResult$2, () => [[["ResultValue", tuple_type(string_type, array_type(uint8_type))]], [["ErrorValue", string_type]]])]], [["Item", class_type("Browser.Types.File", undefined)]], [["Item", union_type("Microsoft.FSharp.Core.FSharpResult`2", [tuple_type(string_type, string_type), string_type], FSharpResult$2, () => [[["ResultValue", tuple_type(string_type, string_type)]], [["ErrorValue", string_type]]])]], [["sourceName", string_type], ["text", string_type]], [["Item", class_type("Browser.Types.File", undefined)]], [["Item", union_type("Microsoft.FSharp.Core.FSharpResult`2", [tuple_type(string_type, string_type, array_type(uint8_type)), string_type], FSharpResult$2, () => [[["ResultValue", tuple_type(string_type, string_type, array_type(uint8_type))]], [["ErrorValue", string_type]]])]], [["fileName", string_type], ["mediaType", string_type], ["bytes", array_type(uint8_type)]], [], [], [], [["Item", int64_type]], [["Item", int64_type]], [], [], [], [["commandId", string_type], ["gesture", string_type]], [["commandId", string_type], ["replaceConflict", bool_type]], [["commandId", string_type]], [["commandId", string_type]], [], [], [["Item", string_type]], [], [["Item", string_type]], [["Item", string_type]], [["panelId", string_type], ["side", SidebarSide_$reflection()]], [["panelId", string_type], ["delta", int32_type]], [["Item", SidebarSide_$reflection()]], [], [], [], [["height", int32_type]], [], [["delta", int32_type]], [["panelId", string_type]], [], [], [["Item", string_type]], [["Item", string_type]], [["commandId", string_type], ["delta", int32_type]], [], [["Item", string_type]], [["commandId", string_type], ["value", string_type]], [["Item", string_type]], [["Item", ModalCommand_$reflection()]], [], [["Item", SimulatorAction_$reflection()]], [["Item", option_type(int32_type)]], [], [["Item", MapController_$reflection()]], [], [], [], [], [["Item", PlanningAction_$reflection()]], [], [], [], [], [["Item", SimulatorResponseEnvelope_$reflection()]], [], [["Item", string_type]], [["Item", string_type]], [["Item", string_type]], [["key", string_type], ["controlOrMeta", bool_type], ["shift", bool_type], ["alt", bool_type], ["repeat", bool_type]], [["Item", string_type]], [["focusPanel", bool_type]], [], [["Item", WorkspaceMode_$reflection()]], [["Item", EditorToolPanel_$reflection()]], [], [["Item", EditorWorkspaceAction_$reflection()]], [["Item", string_type]], [["Item", MapEditorAction_$reflection()]], [], [], [], [], [["Item", ComparisonView_$reflection()]], [], [], [], [["Item", Action_$reflection()]], [], [], []]);
}

export class WorkspaceMode extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["SimulatorWorkspace", "PlanningWorkspace", "EditorWorkspace", "ReplayWorkspace"];
    }
    static SimulatorWorkspace = new WorkspaceMode(0, []);
    static PlanningWorkspace = new WorkspaceMode(1, []);
    static EditorWorkspace = new WorkspaceMode(2, []);
    static ReplayWorkspace = new WorkspaceMode(3, []);
}

export function WorkspaceMode_$reflection() {
    return union_type("SIR.Client.Web.AppTypes.WorkspaceMode", [], WorkspaceMode, () => [[], [], [], []]);
}

export class EditorToolPanel extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["TerrainTools", "UnitTools", "EdgeTools", "ZoneTools", "DocumentTools"];
    }
    static TerrainTools = new EditorToolPanel(0, []);
    static UnitTools = new EditorToolPanel(1, []);
    static EdgeTools = new EditorToolPanel(2, []);
    static ZoneTools = new EditorToolPanel(3, []);
    static DocumentTools = new EditorToolPanel(4, []);
}

export function EditorToolPanel_$reflection() {
    return union_type("SIR.Client.Web.AppTypes.EditorToolPanel", [], EditorToolPanel, () => [[], [], [], [], []]);
}

export class Model extends Record {
    constructor(Shell, Editor, Simulator, SimulatorSelectedUnit, SimulatorControllerSelection, Planning, Tactical, TacticalBindings, TacticalBindingDrafts, TacticalBindingImport, TacticalBindingDiagnostics, TacticalBindingsOpen, TacticalLayout, TacticalLayoutDiagnostics, DesktopToolbarCommands, DesktopToolbarCustomizationOpen, BottomPanelResizeActive, TacticalSelectedUnit, Workspace, EditorToolPanel, EditorToolPanelVisible, SampleReplayFrames, EditorView, HeldInputs, InputHelpExpanded, PendingInterchangeReview, ImportAnnouncement, Battlefield, PreviousFrame, PresentationAlpha, ComparisonBookmarks, ComparisonView, Live) {
        super();
        this.Shell = Shell;
        this.Editor = Editor;
        this.Simulator = Simulator;
        this.SimulatorSelectedUnit = SimulatorSelectedUnit;
        this.SimulatorControllerSelection = SimulatorControllerSelection;
        this.Planning = Planning;
        this.Tactical = Tactical;
        this.TacticalBindings = TacticalBindings;
        this.TacticalBindingDrafts = TacticalBindingDrafts;
        this.TacticalBindingImport = TacticalBindingImport;
        this.TacticalBindingDiagnostics = TacticalBindingDiagnostics;
        this.TacticalBindingsOpen = TacticalBindingsOpen;
        this.TacticalLayout = TacticalLayout;
        this.TacticalLayoutDiagnostics = TacticalLayoutDiagnostics;
        this.DesktopToolbarCommands = DesktopToolbarCommands;
        this.DesktopToolbarCustomizationOpen = DesktopToolbarCustomizationOpen;
        this.BottomPanelResizeActive = BottomPanelResizeActive;
        this.TacticalSelectedUnit = TacticalSelectedUnit;
        this.Workspace = Workspace;
        this.EditorToolPanel = EditorToolPanel;
        this.EditorToolPanelVisible = EditorToolPanelVisible;
        this.SampleReplayFrames = SampleReplayFrames;
        this.EditorView = EditorView;
        this.HeldInputs = HeldInputs;
        this.InputHelpExpanded = InputHelpExpanded;
        this.PendingInterchangeReview = PendingInterchangeReview;
        this.ImportAnnouncement = ImportAnnouncement;
        this.Battlefield = Battlefield;
        this.PreviousFrame = PreviousFrame;
        this.PresentationAlpha = PresentationAlpha;
        this.ComparisonBookmarks = ComparisonBookmarks;
        this.ComparisonView = ComparisonView;
        this.Live = Live;
    }
}

export function Model_$reflection() {
    return record_type("SIR.Client.Web.AppTypes.Model", [], Model, () => [["Shell", Model_$reflection_1()], ["Editor", MapEditorState_$reflection()], ["Simulator", option_type(SimulatorHandoff_$reflection())], ["SimulatorSelectedUnit", option_type(int32_type)], ["SimulatorControllerSelection", option_type(MapController_$reflection())], ["Planning", option_type(PlanningWorkspaceState_$reflection())], ["Tactical", TacticalTimelineState_$reflection()], ["TacticalBindings", TacticalBindingProfile_$reflection()], ["TacticalBindingDrafts", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, string_type])], ["TacticalBindingImport", string_type], ["TacticalBindingDiagnostics", list_type(string_type)], ["TacticalBindingsOpen", bool_type], ["TacticalLayout", TacticalLayoutProfile_$reflection()], ["TacticalLayoutDiagnostics", list_type(string_type)], ["DesktopToolbarCommands", list_type(string_type)], ["DesktopToolbarCustomizationOpen", bool_type], ["BottomPanelResizeActive", bool_type], ["TacticalSelectedUnit", option_type(int32_type)], ["Workspace", WorkspaceMode_$reflection()], ["EditorToolPanel", EditorToolPanel_$reflection()], ["EditorToolPanelVisible", bool_type], ["SampleReplayFrames", option_type(array_type(InspectionProjection_$reflection()))], ["EditorView", EditorWorkspaceState_$reflection()], ["HeldInputs", HeldInputSession_$reflection()], ["InputHelpExpanded", bool_type], ["PendingInterchangeReview", option_type(InterchangeReview_$reflection())], ["ImportAnnouncement", option_type(string_type)], ["Battlefield", BattlefieldViewState_$reflection()], ["PreviousFrame", option_type(RenderFrame_$reflection())], ["PresentationAlpha", float64_type], ["ComparisonBookmarks", list_type(ComparisonBookmark_$reflection())], ["ComparisonView", ComparisonView_$reflection()], ["Live", State_$reflection()]]);
}

