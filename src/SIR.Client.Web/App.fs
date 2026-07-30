module SIR.Client.Web.App

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

type Msg =
    | ShellMsg of SIR.Client.Msg
    | BattlefieldChanged of BattlefieldAction
    | FileSelected of File
    | MapFileSelected of File
    | MapTextRead of sourceName: string * text: string
    | BackgroundFileSelected of File
    | BackgroundBytesRead of fileName: string * mediaType: string * bytes: byte array
    | AcceptInterchangeReview
    | RejectInterchangeReview
    | PlaybackPulse
    | EditorPulse
    | SimulateEditorRevision
    | SimulatorChanged of SimulatorAction
    | SimulatorUnitSelectionChanged of int32 option
    | BeginSimulatorControllerSelection
    | ChooseSimulatorController of MapController
    | CommitSimulatorControllerSelection
    | CancelSimulatorControllerSelection
    | RequestSimulatorReset
    | ResetSimulator
    | SimulatorPanelChanged of SimulatorToolPanel
    | ToggleSimulatorToolPanelVisibility
    | PlanningChanged of PlanningAction
    | InitializePlanningWorker
    | ValidatePlanningRevision
    | PreviewPlanningRevision
    | CommitPlanningRevision
    | PlanningWorkerResponded of SimulatorResponseEnvelope
    | ExportPlanningReview
    | LoadMapSample of string
    | LoadSimulationSample of string
    | LoadReplaySample of string
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
    | EditorToolPanelChanged of EditorToolPanel
    | ToggleEditorToolPanelVisibility
    | EditorWorkspaceChanged of EditorWorkspaceAction
    | RecallEditorView of string
    | EditorChanged of MapEditorAction
    | ExportMap
    | ExportDesignBundle
    | ExportExperiment
    | AddComparisonBookmark
    | ComparisonViewChanged of ComparisonView
    | ExportEvidenceSvg
    | ExportEvidencePng

and WorkspaceMode =
    | SimulatorWorkspace
    | PlanningWorkspace
    | EditorWorkspace
    | ReplayWorkspace
    | RulesWorkspace
    | SamplesWorkspace

and EditorToolPanel =
    | TerrainTools
    | UnitTools
    | EdgeTools
    | ZoneTools
    | DocumentTools

and SimulatorToolPanel =
    | ControllerTools
    | EventTools
    | SimulatorSampleTools

type Model =
    { Shell: SIR.Client.Model
      Editor: MapEditorState
      Simulator: SimulatorHandoff option
      SimulatorSelectedUnit: int32 option
      SimulatorControllerSelection: MapController option
      Planning: PlanningWorkspaceState option
      Workspace: WorkspaceMode
      EditorToolPanel: EditorToolPanel
      EditorToolPanelVisible: bool
      SimulatorToolPanel: SimulatorToolPanel
      SimulatorToolPanelVisible: bool
      SampleReplayFrames: InspectionProjection array option
      EditorView: EditorWorkspaceState
      EditorSpacePressed: bool
      InputHelpExpanded: bool
      PendingInterchangeReview: InterchangeReview option
      Battlefield: BattlefieldViewState
      PreviousFrame: RenderFrame option
      PresentationAlpha: float
      ComparisonBookmarks: ComparisonBookmark list
      ComparisonView: ComparisonView }

[<Emit("window.matchMedia('(prefers-reduced-motion: reduce)').matches")>]
let private prefersReducedMotion: bool = jsNative

[<Emit("(() => { const target = $0; const tag = target && typeof target.tagName === 'string' ? target.tagName.toLowerCase() : ''; return tag === 'input' ? 'input' : tag === 'textarea' ? 'textarea' : tag === 'select' ? 'select' : target && target.isContentEditable ? 'contenteditable' : 'application'; })()")>]
let private currentInputTargetName (target: EventTarget) : string = jsNative

let private currentInputTarget target =
    match currentInputTargetName target with
    | "input" -> InputElement
    | "textarea" -> TextAreaElement
    | "select" -> SelectElement
    | "contenteditable" -> ContentEditableElement
    | _ -> ApplicationElement

[<Emit("(() => { const target = $0; const current = $1; return target === current || (target && typeof target.tagName === 'string' && target.tagName.toLowerCase() === 'svg' && target.getAttribute('role') === 'application'); })()")>]
let private isSimulatorModalTarget
    (target: EventTarget)
    (currentTarget: EventTarget)
    : bool =
    jsNative

[<Emit("""
const target = $0;
if (!(target instanceof Element) || !target.closest("details.desktop-menu")) {
  document.querySelectorAll("details.desktop-menu[open]").forEach(menu => menu.removeAttribute("open"));
}
""")>]
let private dismissDesktopMenus (target: EventTarget) : unit = jsNative

[<Emit("""
const current = $0.closest("details.desktop-menu");
document.querySelectorAll("details.desktop-menu[open]").forEach(menu => {
  if (menu !== current) menu.removeAttribute("open");
});
""")>]
let private closeSiblingDesktopMenus (summary: EventTarget) : unit = jsNative

[<Emit("""
document.querySelectorAll("details.desktop-menu[open]").forEach(menu => menu.removeAttribute("open"));
""")>]
let private closeDesktopMenus () : unit = jsNative

[<Emit("""
setTimeout(() => {
  const target = document.getElementById($0);
  if (target && typeof target.focus === "function") target.focus();
}, 0);
""")>]
let private focusElementAfterRender (id: string) : unit = jsNative

[<Emit("""
setTimeout(() => {
  const target = document.getElementById($0);
  if (target instanceof window.HTMLInputElement && target.type === "file") target.click();
}, 0);
""")>]
let private openFilePickerAfterRender (id: string) : unit = jsNative

let private fileBytes (file: File) =
    async {
        let! buffer = file.arrayBuffer () |> Async.AwaitPromise
        let typed = JS.Constructors.Uint8Array.Create(buffer)
        return file.name, Array.init typed.length (fun index -> typed[index])
    }

let private fileText (file: File) =
    async {
        let! text = file.text () |> Async.AwaitPromise
        return file.name, text
    }

let private rasterBytes (file: File) =
    async {
        let! buffer = file.arrayBuffer () |> Async.AwaitPromise
        let typed = JS.Constructors.Uint8Array.Create(buffer)
        return file.name, file.``type``, Array.init typed.length (fun index -> typed[index])
    }

let private runEffect effect =
    match effect with
    | Run(operation, request) ->
        Runner.post operation request

let private effectsToCmd effects =
    Cmd.ofEffect (fun _ -> effects |> List.iter runEffect)

let private downloadExperiment report =
    let content = Lab.export report
    emitJsStatement
        content
        """
        const blob = new Blob([$0], { type: "text/plain;charset=utf-8" });
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = "sir-lab-experiment.sir-lab";
        anchor.click();
        URL.revokeObjectURL(url);
        """

let private downloadMap state =
    let content = MapEditor.export state
    emitJsStatement
        content
        """
        const blob = new Blob([$0], { type: "text/plain;charset=utf-8" });
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = "battlefield.sir-map";
        anchor.click();
        URL.revokeObjectURL(url);
        """

[<Emit("""
const blob = new Blob([$0], { type: "text/plain;charset=utf-8" });
const url = URL.createObjectURL(blob);
const anchor = document.createElement("a");
anchor.href = url;
anchor.download = "sir-planning-review.sir-planning-review";
anchor.click();
URL.revokeObjectURL(url);
""")>]
let private downloadPlanningReview (_content: string) : unit = jsNative

[<Emit("""
const bundle = {
  format: "sir-map-editor-design",
  version: 1,
  name: $0,
  editor: { digest: $1, revision: $2, map: $3 },
  simulator: $4 == null ? null : { digest: $5, tick: $6, map: $4 }
};
const content = JSON.stringify(bundle, null, 2) + "\n";
const slug = String($0 || "battlefield")
  .trim()
  .toLowerCase()
  .replace(/[^a-z0-9]+/g, "-")
  .replace(/^-+|-+$/g, "") || "battlefield";
const blob = new Blob([content], { type: "application/json;charset=utf-8" });
const url = URL.createObjectURL(blob);
const anchor = document.createElement("a");
anchor.href = url;
anchor.download = `${slug}.sir-design.json`;
anchor.click();
URL.revokeObjectURL(url);
""")>]
let private downloadDesignBundleContent
    (name: string)
    (digest: string)
    (revision: int64)
    (editorMap: string)
    (simulatorMap: string)
    (simulatorDigest: string)
    (simulatorTick: int32)
    : unit =
    jsNative

let private downloadDesignBundle (state: MapEditorState) (simulator: SimulatorHandoff option) =
    let editorMap = MapEditor.export state
    let simulatorMap =
        simulator
        |> Option.map (fun handoff ->
            MapEditor.export { state with Map = handoff.RuntimeMap })
        |> Option.toObj
    let simulatorDigest =
        simulator
        |> Option.map _.Revision.Digest
        |> Option.toObj
    let simulatorTick =
        simulator
        |> Option.map _.Tick
        |> Option.defaultValue -1

    downloadDesignBundleContent
        state.Authoring.Name
        state.Revision.Digest
        state.Revision.Number
        editorMap
        simulatorMap
        simulatorDigest
        simulatorTick

[<Emit("window.localStorage.getItem('sir.map-editor.autosave.v1')")>]
let private readMapAutosave () : string = jsNative

let private scheduleMapAutosave content =
    emitJsStatement
        content
        """
        clearTimeout(window.__sirMapAutosaveTimer);
        window.__sirMapAutosaveTimer = setTimeout(() => {
          window.localStorage.setItem("sir.map-editor.autosave.v1", $0);
        }, 500);
        """

let private downloadEvidenceSvg (evidence: SvgEvidence) =
    emitJsStatement
        evidence
        """
        const content = $0.Svg;
        const fileName = $0.FileName;
        const blob = new Blob([content], { type: "image/svg+xml;charset=utf-8" });
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = fileName;
        anchor.click();
        URL.revokeObjectURL(url);
        """

let private downloadEvidencePng (evidence: SvgEvidence) =
    emitJsStatement
        evidence
        """
        const content = $0.Svg;
        const fileName = $0.FileName.replace(/\.svg$/, ".png");
        const source = new Blob([content], { type: "image/svg+xml;charset=utf-8" });
        const sourceUrl = URL.createObjectURL(source);
        const image = new Image();
        image.onload = () => {
          const canvas = document.createElement("canvas");
          canvas.width = Math.max(1, image.naturalWidth);
          canvas.height = Math.max(1, image.naturalHeight);
          const context = canvas.getContext("2d", { alpha: false });
          context.drawImage(image, 0, 0);
          canvas.toBlob((png) => {
            if (!png) throw new Error("The safe SVG snapshot could not be rasterized.");
            const pngUrl = URL.createObjectURL(png);
            const anchor = document.createElement("a");
            anchor.href = pngUrl;
            anchor.download = fileName;
            anchor.click();
            URL.revokeObjectURL(pngUrl);
            URL.revokeObjectURL(sourceUrl);
          }, "image/png");
        };
        image.onerror = () => {
          URL.revokeObjectURL(sourceUrl);
          throw new Error("The sanitized evidence SVG could not be loaded for PNG export.");
        };
        image.src = sourceUrl;
        """

let private evidenceFor model =
    let emptySandboxFrame =
        { Tick = model.Shell.Playback.CurrentTick
          Board =
            { MinimumColumn = 0
              MinimumRow = 0
              MaximumColumn = 0
              MaximumRow = 0 }
          Units = [||]
          Edges = [||]
          Overlays = [||]
          Events = [||]
          Disclosure = SandboxDisclosure }
    let frame =
        if model.Workspace = SimulatorWorkspace || model.Workspace = EditorWorkspace then
            MapEditor.frame model.Editor
        else
            Shell.renderFrame model.Shell
            |> Option.orElseWith (fun () ->
                model.Shell.Lab.Report
                |> Option.map (fun report -> Lab.renderFrame report.Comparison.Fork))
            |> Option.defaultValue emptySandboxFrame
    let sourceIdentity, replayIdentity, engineIdentity =
        match model.Shell.Source with
        | Loaded metadata ->
            metadata.SourceName, metadata.SourceIdentity, metadata.EngineIdentity
        | Reading name -> name, "not-loaded", "not-loaded"
        | Rejected(name, _) -> name, "rejected", "not-loaded"
        | NoSource -> "built-in-review-board", "no-replay", "built-in"
    let rulesetIdentity =
        model.Shell.Lab.Report
        |> Option.map _.Comparison.Fork.Input.RulesetIdentity
    let mode =
        match model.Shell.Mode with
        | VerifiedReplay -> VerifiedReplayEvidence
        | PerspectivePlayback -> PerspectiveEvidence
        | SandboxFork _
        | ScenarioSandbox _
        | NoRun -> DerivedSimulationEvidence
    let provenance =
        { SourceIdentity = sourceIdentity
          ReplayIdentity = replayIdentity
          ProjectionIdentity = EvidenceExport.projectionIdentity frame
          EngineIdentity = engineIdentity
          RulesetIdentity = rulesetIdentity
          Tick = frame.Tick
          Mode = mode
          PaletteIdentity = model.Battlefield.PaletteId
          RendererVersion = EvidenceExport.RendererVersion }

    EvidenceExport.svg
        provenance
        (Some "Presentation-only evidence; undisclosed and replay-supplied markup omitted.")
        frame

let private sampleReplayShell
    (sample: ExperienceReplaySample)
    (frames: InspectionProjection array)
    =
    let first = Array.head frames
    let identity = "sample-replay-" + sample.Id
    { Shell.init () with
        Source =
            Loaded
                { SourceName = sample.Title
                  SourceIdentity = identity
                  EngineIdentity = "map-editor-sample-simulator-v1"
                  FinalTick = Array.last frames |> _.Tick
                  Kind = DesignScenario }
        Mode = ScenarioSandbox identity
        Verification = SandboxDerived identity
        Playback =
            { CurrentTick = first.Tick
              FinalTick = Array.last frames |> _.Tick
              IsPlaying = false
              Speed = Normal }
        Inspection = Some first
        Worker = WorkerReady
        Announcement = "Loaded curated replay walkthrough “" + sample.Title + "”." }

let private sampleReplayFrameAt tick (frames: InspectionProjection array) =
    frames
    |> Array.filter (fun frame -> frame.Tick <= tick)
    |> Array.tryLast
    |> Option.orElseWith (fun () -> Array.tryHead frames)

let private setSampleReplayTick tick frames shell =
    let tick = max 0 (min shell.Playback.FinalTick tick)
    match sampleReplayFrameAt tick frames with
    | None -> shell
    | Some frame ->
        { shell with
            Playback =
                { shell.Playback with
                    CurrentTick = frame.Tick
                    IsPlaying =
                        shell.Playback.IsPlaying
                        && frame.Tick < shell.Playback.FinalTick }
            Inspection = Some frame
            Announcement = "Sample replay at tick " + string frame.Tick + "." }

let init () =
    let editor =
        let initial = MapEditor.initial
        let autosave = readMapAutosave ()
        if isNull autosave then initial
        else MapEditor.update (OfferCrashRecovery autosave) initial
    let editorView =
        MapEditorWorkspace.initial prefersReducedMotion
        |> MapEditorWorkspace.update
            editor.Map
            (MapEditor.selected editor)
            FitEditorBoard

    { Shell = Shell.init ()
      Editor = editor
      Simulator = None
      SimulatorSelectedUnit = None
      SimulatorControllerSelection = None
      Planning = None
      Workspace = EditorWorkspace
      EditorToolPanel = TerrainTools
      EditorToolPanelVisible = false
      SimulatorToolPanel = ControllerTools
      SimulatorToolPanelVisible = false
      SampleReplayFrames = None
      EditorView = editorView
      EditorSpacePressed = false
      InputHelpExpanded = false
      PendingInterchangeReview = None
      Battlefield =
        { Battlefield.initial with
            ReducedMotion =
                prefersReducedMotion }
      PreviousFrame = None
      PresentationAlpha = 1.0
      ComparisonBookmarks = []
      ComparisonView = Split },
    Cmd.none

let rec update msg model =
    match msg with
    | FileSelected file ->
        { model with SampleReplayFrames = None },
        Cmd.OfAsync.perform
            fileBytes
            file
            (fun (name, bytes) -> ShellMsg(ReplayBytesSelected(name, bytes)))
    | MapFileSelected file ->
        model,
        Cmd.OfAsync.perform
            fileText
            file
            (fun (name, text) -> MapTextRead(name, text))
    | MapTextRead(sourceName, text) ->
        let lower = sourceName.ToLowerInvariant()
        if lower.EndsWith(".sir-map") then
            update (EditorChanged(LoadMapText text)) model
        else
            let format =
                if lower.EndsWith(".dd2vtt") || lower.EndsWith(".uvtt") then UniversalVtt
                elif lower.EndsWith(".xml") then FantasyGroundsImage
                else FoundryScene
            { model with
                PendingInterchangeReview =
                    Some(MapEditorInterchange.evaluate format sourceName text) },
            Cmd.ofEffect (fun _ -> focusElementAfterRender "editor-interchange-review")
    | BackgroundFileSelected file ->
        model,
        Cmd.OfAsync.perform
            rasterBytes
            file
            (fun (name, mediaType, bytes) -> BackgroundBytesRead(name, mediaType, bytes))
    | BackgroundBytesRead(fileName, mediaType, bytes) ->
        update
            (EditorWorkspaceChanged(
                AttachLocalRaster(fileName, mediaType, bytes)
            ))
            model
    | RejectInterchangeReview ->
        { model with PendingInterchangeReview = None },
        Cmd.ofEffect (fun _ -> focusElementAfterRender "editor-map-stage")
    | AcceptInterchangeReview ->
        match model.PendingInterchangeReview with
        | Some review ->
            match MapEditorInterchange.accept review with
            | Ok candidate ->
                let importText =
                    MapEditor.export { model.Editor with Map = candidate }
                let next, command = update (EditorChanged(LoadMapText importText)) model
                { next with PendingInterchangeReview = None },
                Cmd.batch [
                    command
                    Cmd.ofEffect (fun _ -> focusElementAfterRender "editor-map-stage")
                ]
            | Error _ -> model, Cmd.none
        | None -> model, Cmd.none
    | LoadMapSample sampleId ->
        match ExperienceSamples.tryMap sampleId with
        | None -> model, Cmd.none
        | Some sample ->
            let editor = ExperienceSamples.editorState sample
            let editorView =
                MapEditorWorkspace.initial model.EditorView.ReducedMotion
                |> MapEditorWorkspace.update
                    editor.Map
                    (MapEditor.selected editor)
                    FitEditorBoard
            { model with
                Editor = editor
                Simulator = None
                Workspace = EditorWorkspace
                EditorToolPanel = TerrainTools
                EditorToolPanelVisible = false
                EditorView = editorView
                SampleReplayFrames = None
                Battlefield =
                    Battlefield.reconcile
                        (MapEditor.frame editor)
                        model.Battlefield },
            Cmd.none
    | LoadSimulationSample sampleId ->
        match ExperienceSamples.tryMap sampleId with
        | None -> model, Cmd.none
        | Some sample ->
            let editor = ExperienceSamples.editorState sample
            match ExperienceSamples.simulator sample with
            | None ->
                { model with
                    Editor =
                        { editor with
                            Validation = Some "The curated simulation sample could not be validated." } },
                Cmd.none
            | Some simulator ->
                let frame =
                    MapEditorSimulator.frame editor.SelectedUnit simulator
                { model with
                    Editor =
                        { editor with
                            SimulatedDigest = Some simulator.Revision.Digest }
                    Simulator = Some simulator
                    SimulatorSelectedUnit = editor.SelectedUnit
                    SimulatorControllerSelection = None
                    Workspace = SimulatorWorkspace
                    SimulatorToolPanel = ControllerTools
                    SimulatorToolPanelVisible = false
                    SampleReplayFrames = None
                    Battlefield =
                        Battlefield.reconcile frame model.Battlefield
                    PreviousFrame = None
                    PresentationAlpha = 1.0 },
                Cmd.none
    | LoadReplaySample sampleId ->
        match ExperienceSamples.tryReplay sampleId with
        | None -> model, Cmd.none
        | Some sample ->
            let frames = ExperienceSamples.replayFrames sample
            if Array.isEmpty frames then
                model, Cmd.none
            else
                let shell = sampleReplayShell sample frames
                let frame =
                    Shell.renderFrame shell
                    |> Option.defaultValue Battlefield.representativeFrame
                { model with
                    Shell = shell
                    Workspace = ReplayWorkspace
                    SampleReplayFrames = Some frames
                    Battlefield =
                        Battlefield.reconcile frame model.Battlefield
                    PreviousFrame = None
                    PresentationAlpha = 1.0 },
                Cmd.none
    | WorkspaceChanged workspace ->
        let editor =
            if
                workspace = ReplayWorkspace
                || workspace = RulesWorkspace
                || workspace = SamplesWorkspace
            then
                MapEditor.update CancelEditorGesture model.Editor
            else
                model.Editor
        let editorView =
            if workspace = EditorWorkspace then
                model.EditorView
            else
                MapEditorWorkspace.update
                    model.Editor.Map
                    (MapEditor.selected model.Editor)
                    CancelEditorPointers
                    model.EditorView
        let planning, initializePlanning =
            if workspace = PlanningWorkspace then
                match model.Planning with
                | Some current when current.MapRevision = model.Editor.Revision.Digest ->
                    Some current, false
                | _ ->
                    (model.Editor.Map.Units
                     |> Map.toSeq
                     |> Seq.map snd
                     |> PlanningWorkspace.initial model.Editor.Revision.Digest
                     |> Some),
                    true
            else model.Planning, false

        { model with
            Editor = editor
            Planning = planning
            Workspace = workspace
            EditorView = editorView
            InputHelpExpanded = false
            SimulatorControllerSelection = None
            EditorSpacePressed =
                CurrentModalInput.spaceHeldAfterWorkspaceChange
                    model.EditorSpacePressed },
        if initializePlanning then
            Cmd.ofEffect (fun dispatch -> dispatch InitializePlanningWorker)
        else Cmd.none
    | ToggleInputHelp focusPanel ->
        let expanded = not model.InputHelpExpanded
        { model with InputHelpExpanded = expanded },
        if focusPanel then
            Cmd.ofEffect (fun _ ->
                focusElementAfterRender (
                    if expanded then "modal-input-panel" else "modal-input-toggle"
                ))
        else
            Cmd.none
    | ApplicationFocusLost ->
        { model with
            InputHelpExpanded = false
            EditorSpacePressed = false },
        Cmd.none
    | SimulateEditorRevision ->
        match MapEditorSimulator.tryHandoff model.Editor with
        | Error message ->
            { model with
                Editor = { model.Editor with Validation = Some message } },
            Cmd.none
        | Ok simulator ->
            let frame = MapEditorSimulator.frame model.Editor.SelectedUnit simulator
            { model with
                Editor =
                    { model.Editor with
                        SimulatedDigest = Some simulator.Revision.Digest
                        Validation = None }
                Simulator = Some simulator
                SimulatorSelectedUnit = model.Editor.SelectedUnit
                SimulatorControllerSelection = None
                Workspace = SimulatorWorkspace
                Battlefield = Battlefield.reconcile frame model.Battlefield
                PreviousFrame = None
                PresentationAlpha = 1.0 },
            Cmd.none
    | SimulatorChanged action ->
        match model.Simulator with
        | None -> model, Cmd.none
        | Some current ->
            let simulator =
                MapEditorSimulator.update action model.SimulatorSelectedUnit current
            let frame = MapEditorSimulator.frame model.SimulatorSelectedUnit simulator
            { model with
                Simulator = Some simulator
                SimulatorControllerSelection =
                    match action with
                    | ToggleSimulatorRun -> None
                    | _ -> model.SimulatorControllerSelection
                Battlefield = Battlefield.reconcile frame model.Battlefield
                PreviousFrame = None
                PresentationAlpha = 1.0 },
            Cmd.none
    | SimulatorUnitSelectionChanged selected ->
        match model.Simulator with
        | None -> model, Cmd.none
        | Some simulator ->
            let selected =
                selected
                |> Option.filter (fun id ->
                    Map.containsKey id simulator.RuntimeMap.Units)
            let frame = MapEditorSimulator.frame selected simulator
            { model with
                SimulatorSelectedUnit = selected
                SimulatorControllerSelection = None
                Battlefield = Battlefield.reconcile frame model.Battlefield
                PreviousFrame = None
                PresentationAlpha = 1.0 },
            Cmd.none
    | BeginSimulatorControllerSelection ->
        match model.Simulator, model.SimulatorSelectedUnit with
        | Some simulator, Some id when not simulator.IsRunning ->
            match Map.tryFind id simulator.RuntimeMap.Units with
            | Some unit ->
                { model with
                    SimulatorControllerSelection = Some unit.Controller },
                Cmd.none
            | None -> model, Cmd.none
        | _ -> model, Cmd.none
    | ChooseSimulatorController controller ->
        match model.SimulatorControllerSelection with
        | Some _ ->
            { model with
                SimulatorControllerSelection = Some controller },
            Cmd.none
        | None -> model, Cmd.none
    | CommitSimulatorControllerSelection ->
        match model.SimulatorControllerSelection with
        | Some controller ->
            let next, command =
                update
                    (SimulatorChanged(SetSimulatorController controller))
                    model
            { next with SimulatorControllerSelection = None }, command
        | None -> model, Cmd.none
    | CancelSimulatorControllerSelection ->
        { model with SimulatorControllerSelection = None }, Cmd.none
    | RequestSimulatorReset ->
        model,
        Cmd.ofEffect (fun dispatch ->
            if
                window.confirm
                    "Reset runtime-only simulator progress to the immutable editor revision?"
            then
                dispatch ResetSimulator)
    | ResetSimulator ->
        match MapEditorSimulator.tryHandoff model.Editor with
        | Error message ->
            { model with
                Editor = { model.Editor with Validation = Some message } },
            Cmd.none
        | Ok simulator ->
            let frame =
                MapEditorSimulator.frame model.SimulatorSelectedUnit simulator
            { model with
                Simulator = Some simulator
                SimulatorControllerSelection = None
                Battlefield = Battlefield.reconcile frame model.Battlefield
                PreviousFrame = None
                PresentationAlpha = 1.0 },
            Cmd.none
    | SimulatorPanelChanged panel ->
        { model with
            SimulatorToolPanel = panel
            SimulatorToolPanelVisible =
                not (
                    model.SimulatorToolPanelVisible
                    && Object.Equals(model.SimulatorToolPanel, panel)
                ) },
        Cmd.none
    | ToggleSimulatorToolPanelVisibility ->
        { model with
            SimulatorToolPanelVisible =
                not model.SimulatorToolPanelVisible },
        Cmd.none
    | PlanningChanged action ->
        match model.Planning with
        | None -> model, Cmd.none
        | Some planning ->
            { model with Planning = Some(PlanningWorkspace.update action planning) }, Cmd.none
    | InitializePlanningWorker ->
        match model.Planning with
        | None -> model, Cmd.none
        | Some planning ->
            let correlation = PlanningWorkspace.correlation 0 planning
            let units =
                planning.Roster
                |> Array.map (fun unit ->
                    { Id = unit.UnitId
                      Side = unit.Side
                      Column = unit.Column
                      Row = unit.Row
                      Health = 1
                      HealthMaximum = 1
                      MovementDirection = None
                      BodyFacing = 0
                      AttentionDirection = 0 })
            let projection: InspectionProjectionTransport =
                { Tick = 0
                  BoardMinimumColumn = 0
                  BoardMinimumRow = 0
                  BoardMaximumColumn = model.Editor.Map.Width - 1
                  BoardMaximumRow = model.Editor.Map.Height - 1
                  Units = units
                  Edges = [||]
                  Events = [||]
                  Checkpoints = [||]
                  PerspectiveHash = None }
            let next =
                { planning with
                    NextOperation = planning.NextOperation + 1
                    WorkerStatus = "Connecting to planning worker" }
            { model with Planning = Some next },
            Cmd.ofEffect (fun _ ->
                Runner.postSimulator
                    correlation
                    (InitializeSession
                        { InitialProjection = projection
                          MaximumHorizonTicks = SimulatorProtocol.MaximumHorizonTicks }))
    | ValidatePlanningRevision
    | PreviewPlanningRevision
    | CommitPlanningRevision as operation ->
        match model.Planning with
        | None -> model, Cmd.none
        | Some planning ->
            let tick = planning.CommittedTick |> Option.defaultValue 0
            let correlation = PlanningWorkspace.correlation tick planning
            let plan = PlanningWorkspace.planTransport planning
            let request =
                match operation with
                | ValidatePlanningRevision -> ValidatePlan plan
                | PreviewPlanningRevision ->
                    PreviewPlan(
                        plan,
                        tick,
                        min
                            SimulatorProtocol.MaximumHorizonTicks
                            (tick + SimulatorProtocol.MaximumPreviewTicks)
                    )
                | CommitPlanningRevision -> CommitPlan plan
                | _ -> failwith "unreachable planning operation"
            let next =
                { planning with
                    NextOperation = planning.NextOperation + 1
                    WorkerStatus =
                        match operation with
                        | ValidatePlanningRevision -> "Validating authored revision"
                        | PreviewPlanningRevision -> "Requesting intent-only prediction"
                        | _ -> "Committing accepted revision" }
            { model with Planning = Some next },
            Cmd.ofEffect (fun _ -> Runner.postSimulator correlation request)
    | PlanningWorkerResponded envelope ->
        match model.Planning with
        | None -> model, Cmd.none
        | Some planning when not (PlanningWorkspace.acceptsResponse envelope planning) ->
            model, Cmd.none
        | Some planning ->
            { model with Planning = Some(PlanningWorkspace.receive envelope planning) },
            Cmd.none
    | ExportPlanningReview ->
        model,
        (model.Planning
         |> Option.map (fun planning ->
             Cmd.ofEffect (fun _ ->
                 planning
                 |> PlanningWorkspace.reviewArtifact
                 |> downloadPlanningReview))
         |> Option.defaultValue Cmd.none)
    | EditorToolPanelChanged panel ->
        { model with
            EditorToolPanel = panel
            EditorToolPanelVisible =
                not (
                    model.EditorToolPanelVisible
                    && Object.Equals(model.EditorToolPanel, panel)
                ) },
        Cmd.none
    | ToggleEditorToolPanelVisibility ->
        { model with
            EditorToolPanelVisible = not model.EditorToolPanelVisible },
        Cmd.none
    | EditorWorkspaceChanged action ->
        let editorView =
            MapEditorWorkspace.update
                model.Editor.Map
                (MapEditor.selected model.Editor)
                action
                model.EditorView
        { model with EditorView = editorView }, Cmd.none
    | RecallEditorView name ->
        match Map.tryFind name model.Editor.Authoring.SavedViews with
        | None -> model, Cmd.none
        | Some saved ->
            { model with
                EditorView = { model.EditorView with Camera = saved.Camera } },
            Cmd.none
    | EditorChanged action ->
        let editor = MapEditor.update action model.Editor
        let editorView =
            match action with
            | ChooseTool _ ->
                MapEditorWorkspace.update
                    editor.Map
                    (MapEditor.selected editor)
                    CancelEditorPointers
                    model.EditorView
            | _ -> model.EditorView
        let battlefield =
            match model.Workspace, model.Simulator with
            | SimulatorWorkspace, Some simulator ->
                Battlefield.reconcile
                    (MapEditorSimulator.frame model.SimulatorSelectedUnit simulator)
                    model.Battlefield
            | _ ->
                Battlefield.reconcile (MapEditor.frame editor) model.Battlefield
        { model with
            Editor = editor
            EditorView = editorView
            Battlefield = battlefield
            PreviousFrame = None
            PresentationAlpha = 1.0 },
        Cmd.ofEffect (fun _ ->
            scheduleMapAutosave (MapEditor.autosaveText editor)
            match action with
            | RequestNewMap
            | RequestClearMap
            | Resize _ when editor.PendingDestructiveChange.IsSome ->
                focusElementAfterRender "editor-destructive-confirmation"
            | ConfirmDestructiveChange
            | CancelDestructiveChange ->
                focusElementAfterRender "editor-map-stage"
            | _ -> ())
    | EditorPulse ->
        match model.Simulator with
        | Some simulator when simulator.IsRunning ->
            update (SimulatorChanged AdvanceRunningSimulatorTick) model
        | _ -> model, Cmd.none
    | ExportMap ->
        let editor = MapEditor.update MarkEditorSaved model.Editor
        { model with Editor = editor }, Cmd.ofEffect (fun _ -> downloadMap editor)
    | ShellMsg shellMsg ->
        match model.SampleReplayFrames, shellMsg with
        | Some _, ReplayBytesSelected _ ->
            let next, effects = Shell.update shellMsg model.Shell
            { model with
                Shell = next
                SampleReplayFrames = None
                Battlefield =
                    { model.Battlefield with
                        SelectedUnit = None
                        FocusedUnit = None }
                PreviousFrame = None
                PresentationAlpha = 1.0 },
            effectsToCmd effects
        | Some frames, _ ->
            let current = model.Shell.Playback.CurrentTick
            let local =
                match shellMsg with
                | StepForward ->
                    setSampleReplayTick (current + 1) frames model.Shell
                | StepBackward ->
                    setSampleReplayTick (current - 1) frames model.Shell
                | SeekRequested tick ->
                    setSampleReplayTick tick frames model.Shell
                | PreviousEvent ->
                    frames
                    |> Array.filter (fun frame ->
                        frame.Tick < current && not (List.isEmpty frame.Events))
                    |> Array.tryLast
                    |> Option.map (fun frame ->
                        setSampleReplayTick frame.Tick frames model.Shell)
                    |> Option.defaultValue model.Shell
                | NextEvent ->
                    frames
                    |> Array.tryFind (fun frame ->
                        frame.Tick > current && not (List.isEmpty frame.Events))
                    |> Option.map (fun frame ->
                        setSampleReplayTick frame.Tick frames model.Shell)
                    |> Option.defaultValue model.Shell
                | TogglePlayback ->
                    { model.Shell with
                        Playback =
                            { model.Shell.Playback with
                                IsPlaying = model.Shell.Playback.CurrentTick < model.Shell.Playback.FinalTick && not model.Shell.Playback.IsPlaying }
                        Announcement =
                            if model.Shell.Playback.IsPlaying then
                                "Sample replay paused."
                            else
                                "Sample replay started." }
                | CancelRequested ->
                    { model.Shell with
                        Playback =
                            { model.Shell.Playback with IsPlaying = false }
                        Announcement = "Sample replay paused." }
                | _ ->
                    Shell.update shellMsg model.Shell |> fst
            let frame =
                Shell.renderFrame local
                |> Option.defaultValue Battlefield.representativeFrame
            { model with
                Shell = local
                Battlefield =
                    Battlefield.reconcile frame model.Battlefield
                PreviousFrame = None
                PresentationAlpha = 1.0 },
            Cmd.none
        | None, _ ->
            let previousFrame = Shell.renderFrame model.Shell
            let next, effects = Shell.update shellMsg model.Shell
            let nextFrame = Shell.renderFrame next
            let battlefield =
                match nextFrame with
                | Some frame -> Battlefield.reconcile frame model.Battlefield
                | None when next.Verification = Loading ->
                    { model.Battlefield with
                        SelectedUnit = None
                        FocusedUnit = None }
                | None -> model.Battlefield

            let interpolation =
                match previousFrame, nextFrame with
                | Some before, Some after
                    when before.Tick <> after.Tick
                         && next.Playback.IsPlaying
                         && not battlefield.ExactTicks
                         && not battlefield.ReducedMotion
                         && before.Disclosure = after.Disclosure
                         && (before.Units |> Array.map _.Id |> Set.ofArray)
                            = (after.Units |> Array.map _.Id |> Set.ofArray) ->
                    Some before, 0.0
                | _ -> model.PreviousFrame, model.PresentationAlpha

            { model with
                Shell = next
                Battlefield = battlefield
                PreviousFrame = fst interpolation
                PresentationAlpha = snd interpolation },
            effectsToCmd effects
    | BattlefieldChanged action ->
        let frame =
            Shell.renderFrame model.Shell
            |> Option.defaultValue Battlefield.representativeFrame

        let battlefield = Battlefield.update frame action model.Battlefield
        let simulatorSelected =
            match model.Workspace, action with
            | SimulatorWorkspace, SelectUnit selected -> selected
            | _ -> model.SimulatorSelectedUnit
        { model with
            Battlefield = battlefield
            SimulatorSelectedUnit = simulatorSelected
            SimulatorControllerSelection =
                if simulatorSelected <> model.SimulatorSelectedUnit then
                    None
                else
                    model.SimulatorControllerSelection
            PreviousFrame =
                if battlefield.ExactTicks || battlefield.ReducedMotion then
                    None
                else model.PreviousFrame
            PresentationAlpha =
                if battlefield.ExactTicks || battlefield.ReducedMotion then
                    1.0
                else model.PresentationAlpha },
        Cmd.none
    | PlaybackPulse ->
        if model.SampleReplayFrames.IsSome && model.Shell.Playback.IsPlaying then
            let frames = Option.get model.SampleReplayFrames
            let shell =
                setSampleReplayTick
                    (model.Shell.Playback.CurrentTick + 1)
                    frames
                    model.Shell
            let frame =
                Shell.renderFrame shell
                |> Option.defaultValue Battlefield.representativeFrame
            { model with
                Shell = shell
                Battlefield = Battlefield.reconcile frame model.Battlefield
                PreviousFrame = None
                PresentationAlpha = 1.0 },
            Cmd.none
        elif
            model.PreviousFrame.IsSome
            && model.PresentationAlpha < 1.0
            && not model.Battlefield.ExactTicks
            && not model.Battlefield.ReducedMotion
        then
            let alpha = min 1.0 (model.PresentationAlpha + 0.25)
            { model with
                PresentationAlpha = alpha
                PreviousFrame =
                    if alpha >= 1.0 then None else model.PreviousFrame },
            Cmd.none
        else
            let next, effects = Shell.playbackTick model.Shell
            { model with Shell = next }, effectsToCmd effects
    | KeyPressed(key, controlOrMeta, shift, alt, repeat)
        when
            model.Workspace = EditorWorkspace
            && model.Editor.PendingDestructiveChange.IsSome
            && not controlOrMeta
            && not shift
            && not alt
            && (key = "Enter" || key = "Escape")
        ->
        match
            CurrentModalInput.resolvePendingDestructiveKey
                key
                controlOrMeta
                shift
                alt
                repeat
        with
        | Some action -> update (EditorChanged action) model
        | None -> model, Cmd.none
    | KeyPressed(key, controlOrMeta, _, alt, repeat)
        when
            (model.Workspace = EditorWorkspace
             || model.Workspace = SimulatorWorkspace)
            && key = "?"
            && not controlOrMeta
            && not alt
            && not repeat
        ->
        update (ToggleInputHelp true) model
    | KeyPressed(key, controlOrMeta, _, alt, repeat)
        when
            model.InputHelpExpanded
            && key = "Escape"
            && not controlOrMeta
            && not alt
            && not repeat
        ->
        update (ToggleInputHelp true) model
    | KeyPressed(key, controlOrMeta, _, _, true)
        when
            not controlOrMeta
            && ((key = "F2"
                 && (model.Workspace = EditorWorkspace
                     || model.Workspace = SimulatorWorkspace))
                || (key = "F3" && model.Workspace = EditorWorkspace))
        ->
        model, Cmd.none
    | KeyPressed(key, controlOrMeta, shift, alt, repeat) ->
        let currentWorkspace =
            match model.Workspace with
            | EditorWorkspace -> CurrentEditor
            | SimulatorWorkspace -> CurrentSimulator
            | _ -> CurrentOther

        let resolvedM5 =
            if model.Workspace = SimulatorWorkspace && not alt then
                let facts =
                    { SimulatorHandoffPresent = model.Simulator.IsSome
                      SimulatorIsRunning =
                        model.Simulator
                        |> Option.map _.IsRunning
                        |> Option.defaultValue false
                      SimulatorHasRoutePreview =
                        model.Simulator
                        |> Option.bind _.PreviewDestination
                        |> Option.isSome
                      SimulatorControllerSelection =
                        model.SimulatorControllerSelection
                      SimulatorRevisionIsStale =
                        model.Simulator
                        |> Option.map (MapEditorSimulator.isBehindDraft model.Editor)
                        |> Option.defaultValue false
                      InputHelpExpanded = model.InputHelpExpanded }
                let contexts = ModalInput.deriveSimulatorContexts facts
                let input =
                    { Key = NormalizedKey.create key None
                      Modifiers =
                        { ControlOrMeta = controlOrMeta
                          Shift = shift
                          Alt = alt }
                      Phase = KeyDown }
                match
                    ModalInput.resolve
                        contexts
                        input
                        repeat
                        (ModalInput.simulatorCatalog
                            model.SimulatorSelectedUnit
                            model.Simulator
                            model.SimulatorControllerSelection)
                with
                | Resolved resolved -> Some resolved.Command
                | NoMatch
                | NoAvailableMatch _ -> None
            elif model.Workspace <> EditorWorkspace || alt then
                None
            else
                let activeDomain =
                    match model.EditorToolPanel with
                    | TerrainTools -> TerrainDomain
                    | UnitTools -> UnitDomain
                    | EdgeTools -> EdgeDomain
                    | ZoneTools -> RegionDomain
                    | DocumentTools -> DocumentDomain
                let facts =
                    { Editor = model.Editor
                      ActiveDomain = activeDomain
                      PanHeld = model.EditorSpacePressed
                      InputHelpExpanded = model.InputHelpExpanded }
                let contexts = ModalInput.deriveEditorContexts facts
                let input =
                    { Key = NormalizedKey.create key None
                      Modifiers =
                        { ControlOrMeta = controlOrMeta
                          Shift = shift
                          Alt = alt }
                      Phase = KeyDown }
                match ModalInput.resolve contexts input repeat (ModalInput.editorCatalog facts) with
                | Resolved resolved when
                    resolved.Id.StartsWith("editor.edge.", StringComparison.Ordinal)
                    || resolved.Id.StartsWith("editor.region.", StringComparison.Ordinal)
                    || resolved.Id.StartsWith("editor.document.", StringComparison.Ordinal)
                    ->
                    Some resolved.Command
                | _ -> None

        match resolvedM5 with
        | Some(EditorCommand action) ->
            update (EditorChanged action) model
        | Some(EditorWorkspaceCommand action) ->
            update (EditorWorkspaceChanged action) model
        | Some(EditorDocumentCommand ExportMapDocument) ->
            update ExportMap model
        | Some(EditorDocumentCommand ExportRepositoryDesignBundle) ->
            update ExportDesignBundle model
        | Some(EditorDocumentCommand OpenMapImport) ->
            model, Cmd.ofEffect (fun _ -> openFilePickerAfterRender "editor-map-import")
        | Some(EditorDocumentCommand(FocusDocumentControl target)) ->
            let id =
                match target with
                | MapImportControl -> "editor-map-import"
                | LayerStateControls -> "editor-layer-controls"
                | LocalBackgroundControls -> "editor-background-file"
                | MapDimensionControls -> "map-width"
                | SavedViewControls -> "editor-saved-view-controls"
            model, Cmd.ofEffect (fun _ -> focusElementAfterRender id)
        | Some(SimulatorCommand action) ->
            update (SimulatorChanged action) model
        | Some(TraverseSimulatorUnit delta) ->
            match model.Simulator with
            | Some simulator ->
                update
                    (SimulatorUnitSelectionChanged(
                        ModalInput.traverseSimulatorUnit
                            delta
                            model.SimulatorSelectedUnit
                            simulator
                    ))
                    model
            | None -> model, Cmd.none
        | Some ModalCommand.BeginSimulatorControllerSelection ->
            update BeginSimulatorControllerSelection model
        | Some(ModalCommand.ChooseSimulatorController controller) ->
            update (ChooseSimulatorController controller) model
        | Some CommitSimulatorController ->
            update CommitSimulatorControllerSelection model
        | Some CancelSimulatorController ->
            update CancelSimulatorControllerSelection model
        | Some RequestSimulatorSandboxReset ->
            update RequestSimulatorReset model
        | Some(ChooseSimulatorPanel panel) ->
            { model with
                SimulatorToolPanel =
                    match panel with
                    | ControllerPanel -> ControllerTools
                    | EventPanel -> EventTools
                    | SimulatorSamplePanel -> SimulatorSampleTools
                SimulatorToolPanelVisible = true },
            Cmd.none
        | Some ToggleSimulatorCommandPanel ->
            update ToggleSimulatorToolPanelVisibility model
        | Some ModalCommand.ToggleInputHelp ->
            update (ToggleInputHelp true) model
        | Some _ -> model, Cmd.none
        | None when model.Workspace = SimulatorWorkspace -> model, Cmd.none
        | None when alt -> model, Cmd.none
        | None ->
          match
              CurrentModalInput.resolveKeyDown
                  currentWorkspace
                  key
                  controlOrMeta
                  shift
                  repeat
          with
          | Some(CurrentEditorAction action) ->
            update (EditorChanged action) model
          | Some(CurrentEditorWorkspaceAction action) ->
            update (EditorWorkspaceChanged action) model
          | Some(CurrentChooseEditorPanel panel) ->
            let appPanel =
                match panel with
                | CurrentTerrainPanel -> TerrainTools
                | CurrentUnitPanel -> UnitTools
                | CurrentEdgePanel -> EdgeTools
                | CurrentZonePanel -> ZoneTools
                | CurrentDocumentPanel -> DocumentTools

            update (EditorToolPanelChanged appPanel) model
          | Some CurrentChooseSelectAndShowTerrainPanel ->
            let next, command =
                update (EditorChanged(ChooseTool Select)) model

            { next with
                EditorToolPanel = TerrainTools
                EditorToolPanelVisible = true },
            command
          | Some CurrentToggleEditorPanel ->
            update ToggleEditorToolPanelVisibility model
          | Some CurrentToggleSimulatorPanel ->
            update ToggleSimulatorToolPanelVisibility model
          | Some CurrentEscapeEditor ->
            let workspaceAction, editorAction =
                CurrentModalInput.editorEscapeActions model.Editor.Gesture

            model
            |> update (EditorWorkspaceChanged workspaceAction)
            |> fst
            |> update (EditorChanged editorAction)
          | Some(CurrentSetEditorSpaceHeld held) ->
            { model with EditorSpacePressed = held }, Cmd.none
          | Some(CurrentSimulatorAction action) ->
            update (SimulatorChanged action) model
          | None when model.Workspace = PlanningWorkspace ->
            match key, controlOrMeta with
            | ("z" | "Z"), true ->
                update
                    (PlanningChanged(
                        if shift then RedoPlanning else UndoPlanning
                    ))
                    model
            | ("y" | "Y"), true -> update (PlanningChanged RedoPlanning) model
            | ("Delete" | "Backspace"), false ->
                update (PlanningChanged RemoveSelectedPlanningCommand) model
            | "[", false ->
                match model.Planning with
                | Some planning when planning.Issues.Length > 0 ->
                    let current = planning.FocusedIssue |> Option.defaultValue 0
                    update
                        (PlanningChanged(
                            FocusPlanningIssue(
                                (current - 1 + planning.Issues.Length) % planning.Issues.Length
                            )
                        ))
                        model
                | _ -> model, Cmd.none
            | "]", false ->
                match model.Planning with
                | Some planning when planning.Issues.Length > 0 ->
                    let current = planning.FocusedIssue |> Option.defaultValue -1
                    update
                        (PlanningChanged(
                            FocusPlanningIssue((current + 1) % planning.Issues.Length)
                        ))
                        model
                | _ -> model, Cmd.none
            | ("r" | "R"), false -> update (PlanningChanged(ChoosePlanningTool RouteTool)) model
            | ("f" | "F"), false -> update (PlanningChanged(ChoosePlanningTool FacingTool)) model
            | ("a" | "A"), false -> update (PlanningChanged(ChoosePlanningTool AttentionTool)) model
            | ("s" | "S"), false -> update (PlanningChanged(ChoosePlanningTool StanceTool)) model
            | ("h" | "H"), false -> update (PlanningChanged(ChoosePlanningTool HoldTool)) model
            | ("e" | "E"), false -> update (PlanningChanged(ChoosePlanningTool EngagementTool)) model
            | ("m" | "M"), false -> update (PlanningChanged(ChoosePlanningTool SynchronizationTool)) model
            | _ -> model, Cmd.none
          | None when model.Workspace = ReplayWorkspace ->
            match key with
            | " "
            | "k"
            | "K" -> update (ShellMsg TogglePlayback) model
            | "ArrowLeft" -> update (ShellMsg StepBackward) model
            | "ArrowRight" -> update (ShellMsg StepForward) model
            | "[" -> update (ShellMsg PreviousEvent) model
            | "]" -> update (ShellMsg NextEvent) model
            | "Escape" -> update (ShellMsg CancelRequested) model
            | _ -> model, Cmd.none
          | None -> model, Cmd.none
    | KeyReleased key ->
        match CurrentModalInput.resolveKeyUp key with
        | Some(CurrentSetEditorSpaceHeld held) ->
            { model with EditorSpacePressed = held }, Cmd.none
        | _ -> model, Cmd.none
    | ExportExperiment ->
        model,
        (model.Shell.Lab.Report
         |> Option.map (fun report -> Cmd.ofEffect (fun _ -> downloadExperiment report))
         |> Option.defaultValue Cmd.none)
    | ExportDesignBundle ->
        model,
        Cmd.ofEffect (fun _ ->
            downloadDesignBundle model.Editor model.Simulator)
    | AddComparisonBookmark ->
        let tick = model.Shell.Playback.CurrentTick
        let bookmark =
            { Tick = tick
              Label = "Linked comparison at tick " + string tick }
        { model with
            ComparisonBookmarks =
                bookmark
                :: model.ComparisonBookmarks
                |> List.distinctBy (fun item -> item.Tick, item.Label)
                |> List.sortBy (fun item -> item.Tick, item.Label) },
        Cmd.none
    | ComparisonViewChanged view ->
        { model with ComparisonView = view }, Cmd.none
    | ExportEvidenceSvg ->
        model,
        Cmd.ofEffect (fun _ -> model |> evidenceFor |> downloadEvidenceSvg)
    | ExportEvidencePng ->
        model,
        Cmd.ofEffect (fun _ -> model |> evidenceFor |> downloadEvidencePng)

let subscriptions model =
    let runner dispatch =
        Runner.subscribe (fun message -> dispatch (ShellMsg message))

    let planningRunner dispatch =
        Runner.subscribeSimulator (fun envelope ->
            dispatch (PlanningWorkerResponded envelope))

    let keyboard dispatch =
        let downHandler =
            fun (event: Event) ->
                let keyboardEvent: KeyboardEvent = unbox event
                if
                    keyboardEvent.target
                    |> currentInputTarget
                    |> CurrentModalInput.acceptsKeyDown
                then
                    if model.Workspace = EditorWorkspace && keyboardEvent.key = " " then
                        keyboardEvent.preventDefault ()
                    dispatch (
                        KeyPressed(
                            keyboardEvent.key,
                            keyboardEvent.ctrlKey || keyboardEvent.metaKey,
                            keyboardEvent.shiftKey,
                            keyboardEvent.altKey,
                            keyboardEvent.repeat
                        )
                    )
        let upHandler =
            fun (event: Event) ->
                let keyboardEvent: KeyboardEvent = unbox event
                dispatch (KeyReleased keyboardEvent.key)
        let blurHandler =
            fun (_: Event) -> dispatch ApplicationFocusLost
        window.addEventListener ("keydown", downHandler)
        window.addEventListener ("keyup", upHandler)
        window.addEventListener ("blur", blurHandler)

        { new IDisposable with
            member _.Dispose() =
                window.removeEventListener ("keydown", downHandler)
                window.removeEventListener ("keyup", upHandler)
                window.removeEventListener ("blur", blurHandler) }

    let timer dispatch =
        let interval =
            match model.Shell.Playback.Speed with
            | Half -> 100
            | Normal
            | Double
            | Maximum -> 50

        let identifier =
            window.setInterval (
                (fun () -> dispatch PlaybackPulse),
                interval
            )

        { new IDisposable with
            member _.Dispose() = window.clearInterval identifier }

    let editorTimer dispatch =
        let identifier =
            window.setInterval ((fun () -> dispatch EditorPulse), 50)

        { new IDisposable with
            member _.Dispose() = window.clearInterval identifier }

    let editorResize dispatch =
        let notify () =
            dispatch (
                EditorWorkspaceChanged(
                    ResizeViewport(
                        max 320.0 (window.innerWidth - 96.0),
                        max 360.0 (min 760.0 (window.innerHeight - 180.0))
                    )
                )
            )
        let handler = fun (_: Event) -> notify ()
        window.addEventListener ("resize", handler)
        notify ()

        { new IDisposable with
            member _.Dispose() = window.removeEventListener ("resize", handler) }

    [ [ "replay-worker-v1" ], runner
      [ "planning-worker-v1" ], planningRunner
      [ "keyboard" ], keyboard
      if model.Workspace = EditorWorkspace then
          [ "editor-resize" ], editorResize
      if model.Shell.Playback.IsPlaying then
          let speedKey =
              match model.Shell.Playback.Speed with
              | Half -> "half"
              | Normal -> "normal"
              | Double -> "two"
              | Maximum -> "maximum"

          [ "playback-pulse"; speedKey ], timer
      if
          model.Workspace = SimulatorWorkspace
          && (model.Simulator |> Option.exists _.IsRunning)
      then
          [ "editor-pulse" ], editorTimer ]

let private speedText speed =
    match speed with
    | Half -> "½×"
    | Normal -> "1×"
    | Double -> "2×"
    | Maximum -> "Maximum"

let private status model =
    match model.Verification with
    | NotLoaded -> "Ready — choose a scenario or load a replay", "status-neutral"
    | Loading -> "Loading replay", "status-loading"
    | BrowserKernelVerified ->
        "Verified browser-kernel replay", "status-verified"
    | PerspectiveReady ->
        "Perspective playback — hidden state unavailable", "status-perspective"
    | SandboxDerived identity ->
        "Sandbox fork — not authoritative (" + identity + ")", "status-sandbox"
    | Unsupported reason -> "Unsupported replay — " + reason, "status-unsupported"
    | Diverged(tick, phase, detail) ->
        "Diverged at tick "
        + string tick
        + " during "
        + phase
        + " — "
        + detail,
        "status-diverged"
    | Failed reason -> "Replay failed — " + reason, "status-failed"

let private button
    (text: string)
    (label: string)
    (disabled: bool)
    (onClick: MouseEvent -> unit)
    =
    Html.button [
        prop.type'.button
        prop.text text
        prop.ariaLabel label
        prop.disabled disabled
        prop.onClick onClick
    ]

let private statusView (model: SIR.Client.Model) =
    let text, className = status model
    let details =
        match model.Verification with
        | SandboxDerived _ ->
            [ " Curated sandbox projections are explanatory examples, not verified replay evidence." ]
        | _ ->
            [ " Browser verification replays accepted kernel inputs; it does not re-run player WASM."
              " Authoritative verification is available only from .NET exact-artifact WASM re-execution." ]

    Html.section [
        prop.className ("verification-banner " + className)
        prop.ariaLabel "Replay verification status"
        prop.role.status
        prop.ariaLive.polite
        prop.children [
            Html.strong text
            for detail in details do
                Html.span [
                    prop.className "status-detail"
                    prop.text detail
                ]
        ]
    ]

let private sourcePanel (model: SIR.Client.Model) dispatch =
    Html.section [
        prop.className "panel source-panel"
        prop.ariaLabel "Replay source"
        prop.children [
            Html.h2 "Replay package"
            Html.p (
                match model.Source with
                | Loaded metadata ->
                    "Loaded "
                    + metadata.SourceName
                    + " · "
                    + string metadata.Kind
                    + " · "
                    + string metadata.FinalTick
                    + " ticks."
                | Reading name -> "Loading " + name + "."
                | Rejected(name, reason) -> name + " rejected: " + reason
                | NoSource -> "Load a bounded .sirr package. Files stay in this browser session."
            )
            Html.input [
                prop.type'.file
                prop.accept ".sirr,application/octet-stream"
                prop.ariaLabel "Choose replay package"
                prop.onChange (fun (files: File list) ->
                    files
                    |> List.tryHead
                    |> Option.iter (FileSelected >> dispatch))
            ]
        ]
    ]

let private controls model dispatch =
    let atEnd = model.Playback.CurrentTick >= model.Playback.FinalTick
    let atStart = model.Playback.CurrentTick <= 0
    let unavailable = model.Playback.FinalTick <= 0
    let hasEvents =
        model.Inspection
        |> Option.exists (fun inspection -> not (List.isEmpty inspection.Events))
    let checkpoints =
        model.Inspection
        |> Option.map _.Checkpoints
        |> Option.defaultValue []

    Html.section [
        prop.className "panel playback-panel"
        prop.ariaLabel "Replay controls"
        prop.children [
            Html.h2 "Playback"
            Html.div [
                prop.className "control-row"
                prop.children [
                    button
                        (if model.Playback.IsPlaying then "Pause" else "Play")
                        (if model.Playback.IsPlaying then
                             "Pause replay"
                         else
                             "Play replay")
                        unavailable
                        (fun _ -> dispatch (ShellMsg TogglePlayback))
                    button
                        "Previous event"
                        "Go to previous disclosed replay event"
                        (unavailable || not hasEvents)
                        (fun _ -> dispatch (ShellMsg PreviousEvent))
                    button
                        "Back"
                        "Step backward one committed replay tick"
                        (unavailable || atStart)
                        (fun _ -> dispatch (ShellMsg StepBackward))
                    button
                        "Step"
                        "Advance one replay step"
                        (unavailable || atEnd)
                        (fun _ -> dispatch (ShellMsg StepForward))
                    button
                        "Next event"
                        "Go to next disclosed replay event"
                        (unavailable || not hasEvents)
                        (fun _ -> dispatch (ShellMsg NextEvent))
                    button
                        "Cancel"
                        "Cancel current replay operation"
                        (Option.isNone model.ActiveOperation)
                        (fun _ -> dispatch (ShellMsg CancelRequested))
                ]
            ]
            Html.label [
                prop.htmlFor "replay-position"
                prop.text (
                    "Tick "
                    + string model.Playback.CurrentTick
                    + " of "
                    + string model.Playback.FinalTick
                )
            ]
            Html.input [
                prop.id "replay-position"
                prop.type'.range
                prop.min 0
                prop.max (max 1 model.Playback.FinalTick)
                prop.value model.Playback.CurrentTick
                prop.disabled unavailable
                prop.ariaValueText (
                    "Tick "
                    + string model.Playback.CurrentTick
                    + " of "
                    + string model.Playback.FinalTick
                )
                prop.onChange (fun (value: int) ->
                    dispatch (ShellMsg(SeekRequested(int32 value))))
            ]
            if not (List.isEmpty checkpoints) then
                Html.div [
                    prop.className "checkpoint-markers"
                    prop.ariaLabel "Replay checkpoint markers"
                    prop.children [
                        Html.span "Checkpoints:"
                        for checkpoint in checkpoints do
                            button
                                ("T" + string checkpoint.Tick)
                                ("Seek to checkpoint at tick " + string checkpoint.Tick)
                                false
                                (fun _ ->
                                    dispatch (ShellMsg(SeekRequested checkpoint.Tick)))
                    ]
                ]
            match model.Worker with
            | WorkerBusy completed ->
                Html.progress [
                    prop.max (max 1 model.Playback.FinalTick)
                    prop.value model.Playback.CurrentTick
                    prop.ariaLabel "Replay operation progress"
                    prop.text (
                        string model.Playback.CurrentTick
                        + " of "
                        + string model.Playback.FinalTick
                        + "; "
                        + string completed
                        + " batches complete"
                    )
                ]
            | _ -> Html.none
            Html.label [
                prop.htmlFor "playback-speed"
                prop.text "Playback speed"
            ]
            Html.select [
                prop.id "playback-speed"
                prop.value (speedText model.Playback.Speed)
                prop.onChange (fun value ->
                    let speed =
                        match value with
                        | "½×" -> Half
                        | "2×" -> Double
                        | "Maximum" -> Maximum
                        | _ -> Normal

                    dispatch (ShellMsg(SpeedChanged speed)))
                prop.children [
                    Html.option [ prop.value "½×"; prop.text "½×" ]
                    Html.option [ prop.value "1×"; prop.text "1×" ]
                    Html.option [ prop.value "2×"; prop.text "2×" ]
                    Html.option [ prop.value "Maximum"; prop.text "Maximum" ]
                ]
            ]
            Html.p [
                prop.className "keyboard-help"
                prop.text "Keyboard: Space or K plays/pauses; Left/Right Arrow steps; [ and ] navigate events; Escape cancels."
            ]
        ]
    ]

let private factionStyle (palette: PaletteTokens) (faction: FactionVisual) =
    match faction with
    | Human ->
        palette.HumanFaction,
        (if palette.Id = ReplayPalettes.monochromePattern.Id then "none" else "none")
    | Arcane ->
        palette.ArcaneFaction,
        (if palette.Id = ReplayPalettes.monochromePattern.Id then "4 2" else "none")
    | Neutral ->
        palette.NeutralFaction,
        (if palette.Id = ReplayPalettes.monochromePattern.Id then "1 2" else "none")
    | OtherFaction _ -> palette.NeutralFaction, "6 2 1 2"

let private glyphView
    (palette: PaletteTokens)
    (centerX: float)
    (centerY: float)
    (scale: float)
    (classId: UnitClassId)
    =
    let glyph = UnitGlyphCatalog.resolve classId
    let transform =
        "translate("
        + string centerX
        + " "
        + string centerY
        + ") scale("
        + string scale
        + ") translate(-12 -12)"

    Svg.g [
        svg.custom ("transform", transform)
        svg.custom ("data-class-id", UnitClassId.value classId)
        svg.children [
            for primitive in glyph.Primitives do
                match primitive with
                | FilledPath path ->
                    Svg.path [
                        svg.d path
                        svg.fill palette.Text
                    ]
                | StrokedPath path ->
                    Svg.path [
                        svg.d path
                        svg.fill "none"
                        svg.stroke palette.Text
                        svg.strokeWidth 1.8
                        svg.strokeLineCap "round"
                        svg.strokeLineJoin "round"
                    ]
                | Circle(x, y, radius) ->
                    Svg.circle [
                        svg.cx x
                        svg.cy y
                        svg.r radius
                        svg.fill palette.Text
                    ]
        ]
    ]

let private unitView
    (scene: BattlefieldScene)
    (dispatch: Msg -> unit)
    (projected: ProjectedUnit)
    =
    let unit = projected.Unit
    let palette = scene.Palette
    let faction, dash = factionStyle palette unit.Faction
    let selected = scene.SelectedUnit = Some unit.Id
    let focused = scene.FocusedUnit = Some unit.Id
    let symbolSize =
        min projected.FootprintWidth projected.FootprintDepth - 12.0
    let half = symbolSize / 2.0
    let glyphScale = symbolSize / 36.0

    let wedge =
        match unit.BodyHeading with
        | Disclosed heading ->
            let angle = HeadingRadians.value heading
            // Straddle the symbol edge so body facing reads as part of the
            // unit while remaining distinct from the upright class glyph.
            // The high-contrast fill and wider base keep the pip legible on
            // faction borders and at the smallest supported footprint.
            let radius = half - 1.0
            let x = projected.SymbolCenterX + Math.Cos(angle) * radius
            let y = projected.SymbolCenterY + Math.Sin(angle) * radius
            let tangentX = -Math.Sin(angle) * 7.0
            let tangentY = Math.Cos(angle) * 7.0
            let tipX = projected.SymbolCenterX + Math.Cos(angle) * (half + 10.0)
            let tipY = projected.SymbolCenterY + Math.Sin(angle) * (half + 10.0)
            Some (
                string (x + tangentX)
                + ","
                + string (y + tangentY)
                + " "
                + string tipX
                + ","
                + string tipY
                + " "
                + string (x - tangentX)
                + ","
                + string (y - tangentY)
            )
        | _ -> None

    Svg.g [
        svg.custom ("data-unit-id", string unit.Id)
        match projected.HealthSegments with
        | Some segments ->
            svg.custom ("data-health-segments", string segments)
        | None ->
            svg.custom ("data-health-disclosure", "omitted")
        svg.custom ("data-semantic-zoom", string scene.SemanticZoom)
        svg.tabIndex (if focused then 0 else -1)
        svg.custom ("role", "button")
        svg.custom ("aria-label", projected.AccessibleLabel)
        svg.onFocus (fun _ -> dispatch (BattlefieldChanged(FocusUnit(Some unit.Id))))
        svg.onClick (fun _ -> dispatch (BattlefieldChanged(SelectUnit(Some unit.Id))))
        svg.onKeyDown (fun event ->
            match event.key with
            | "Enter" ->
                dispatch (BattlefieldChanged(SelectUnit(Some unit.Id)))
            | "Escape" ->
                dispatch (BattlefieldChanged(SelectUnit None))
            | "ArrowLeft" ->
                event.preventDefault ()
                dispatch (BattlefieldChanged(FocusDirection(-1, 0)))
            | "ArrowRight" ->
                event.preventDefault ()
                dispatch (BattlefieldChanged(FocusDirection(1, 0)))
            | "ArrowUp" ->
                event.preventDefault ()
                dispatch (BattlefieldChanged(FocusDirection(0, -1)))
            | "ArrowDown" ->
                event.preventDefault ()
                dispatch (BattlefieldChanged(FocusDirection(0, 1)))
            | _ -> ())
        svg.children [
            Svg.rect [
                svg.custom ("data-authoritative-footprint", "true")
                svg.x (projected.FootprintX + 2.0)
                svg.y (projected.FootprintY + 2.0)
                svg.width (projected.FootprintWidth - 4.0)
                svg.height (projected.FootprintDepth - 4.0)
                svg.rx 4
                svg.fill "none"
                svg.stroke (if selected then palette.Focus else faction)
                svg.strokeWidth (if selected then 4.0 else 2.0)
                svg.custom ("strokeDasharray", dash)
            ]
            Svg.rect [
                svg.custom ("data-unit-symbol", "true")
                svg.x (projected.SymbolCenterX - half)
                svg.y (projected.SymbolCenterY - half)
                svg.width symbolSize
                svg.height symbolSize
                svg.rx 3
                svg.fill palette.Canvas
                svg.stroke faction
                svg.strokeWidth 3
                svg.custom ("strokeDasharray", dash)
            ]
            glyphView
                palette
                projected.SymbolCenterX
                projected.SymbolCenterY
                glyphScale
                unit.ClassId
            if scene.SemanticZoom <> Overview && projected.HealthSegments.IsSome then
                let healthSegments = Option.get projected.HealthSegments
                let healthSegmentWidth = (symbolSize - 11.0) / 12.0
                for index in 0 .. 11 do
                    Svg.rect [
                        svg.custom ("data-health-position", string index)
                        svg.x (
                            projected.SymbolCenterX
                            - half
                            + float index * (healthSegmentWidth + 1.0)
                        )
                        svg.y (projected.SymbolCenterY + half + 3.0)
                        svg.width healthSegmentWidth
                        svg.height 4.0
                        svg.fill (
                            if index < healthSegments then
                                palette.HealthActive
                            else
                                palette.HealthDepleted
                        )
                    ]
            match wedge with
            | Some points ->
                Svg.polygon [
                    svg.custom ("data-facing-wedge", "body")
                    svg.custom ("data-facing-emphasis", "high-contrast")
                    svg.points points
                    svg.fill palette.Focus
                    svg.stroke faction
                    svg.strokeWidth 2.5
                    svg.strokeLineJoin "round"
                ]
            | None -> Html.none
            match unit.SecondaryHeading with
            | Disclosed secondary ->
                let angle = HeadingRadians.value secondary.Radians
                let clearRadius = 7.0
                let endRadius = half - 4.0
                let startX =
                    projected.SymbolCenterX + Math.Cos(angle) * clearRadius
                let startY =
                    projected.SymbolCenterY + Math.Sin(angle) * clearRadius
                let endX =
                    projected.SymbolCenterX + Math.Cos(angle) * endRadius
                let endY =
                    projected.SymbolCenterY + Math.Sin(angle) * endRadius
                let source =
                    match secondary.Source with
                    | WeaponHeading -> "weapon"
                    | SensorHeading -> "sensor"
                    | AttentionHeading -> "attention"
                Svg.g [
                    svg.custom ("data-secondary-heading", source)
                    svg.custom ("aria-label", source + " heading")
                    svg.children [
                        Svg.line [
                            svg.x1 startX
                            svg.y1 startY
                            svg.x2 endX
                            svg.y2 endY
                            svg.stroke palette.Focus
                            svg.strokeWidth 1.5
                        ]
                        Svg.circle [
                            svg.cx endX
                            svg.cy endY
                            svg.r 2
                            svg.fill palette.Focus
                        ]
                    ]
                ]
            | _ -> Html.none
            if scene.SemanticZoom <> Overview then
                for index in 0 .. projected.ElevationBars - 1 do
                    Svg.line [
                        svg.custom ("data-elevation-bar", string (index + 1))
                        svg.x1 (projected.SymbolCenterX - half + 3.0)
                        svg.y1 (projected.SymbolCenterY - half + 4.0 + float index * 4.0)
                        svg.x2 (projected.SymbolCenterX - half + 10.0)
                        svg.y2 (projected.SymbolCenterY - half + 4.0 + float index * 4.0)
                        svg.stroke palette.Text
                        svg.strokeWidth 2
                    ]
            match projected.ElevationLabel with
            | Some label ->
                Svg.text [
                    svg.custom ("data-elevation-label", "true")
                    svg.x (projected.SymbolCenterX - half + 2.0)
                    svg.y (projected.SymbolCenterY - half + 20.0)
                    svg.fill palette.Text
                    svg.fontSize 7
                    svg.text label
                ]
            | None -> Html.none
            if projected.ShowStance then
                let stance =
                    match unit.StanceId with
                    | Disclosed value -> value.Substring(0, 1).ToUpperInvariant()
                    | _ -> ""
                Svg.text [
                    svg.custom ("data-stance-mark", "true")
                    svg.x (projected.SymbolCenterX + half - 8.0)
                    svg.y (projected.SymbolCenterY - half + 8.0)
                    svg.fill palette.Text
                    svg.fontSize 7
                    svg.text stance
                ]
            if focused then
                Svg.circle [
                    svg.custom ("data-focus-ring", "true")
                    svg.cx projected.SymbolCenterX
                    svg.cy projected.SymbolCenterY
                    svg.r (half + 8.0)
                    svg.fill "none"
                    svg.stroke palette.Focus
                    svg.strokeWidth 2
                    svg.strokeDasharray [| 2; 2 |]
                ]
        ]
    ]

let private battlefieldInspector (scene: BattlefieldScene) =
    let selected =
        scene.SelectedUnit
        |> Option.bind (fun selected ->
            scene.Units
            |> Array.tryFind (fun unit -> unit.Unit.Id = selected))

    Html.aside [
        prop.className "battlefield-inspector"
        prop.ariaLabel "Battlefield unit inspector"
        prop.children [
            Html.h3 "Unit inspector"
            match selected with
            | None -> Html.p "No unit selected."
            | Some unit ->
                Html.p unit.AccessibleLabel
                Html.dl [
                    Html.dt "Exact class"
                    Html.dd (
                        UnitClassId.value unit.Unit.ClassId
                    )
                    Html.dt "Footprint"
                    Html.dd (
                        string (CellExtent.value unit.Unit.FootprintWidth)
                        + " × "
                        + string (CellExtent.value unit.Unit.FootprintDepth)
                        + " cells"
                    )
                    Html.dt "Health"
                    Html.dd (
                        match unit.Unit.Health with
                        | Disclosed value ->
                            string (HealthVisual.remaining value)
                            + " / "
                            + string (HealthVisual.maximum value)
                        | NotPresent -> "Not present in this projection"
                        | NotApplicable -> "Not applicable"
                        | ExplicitlyUnknown -> "Explicitly unknown"
                    )
                    Html.dt "Elevation"
                    Html.dd (
                        match unit.Unit.Level with
                        | Disclosed value -> string value
                        | NotPresent -> "Not present in this projection"
                        | NotApplicable -> "Not applicable"
                        | ExplicitlyUnknown -> "Explicitly unknown"
                    )
                    Html.dt "Stance"
                    Html.dd (
                        match unit.Unit.StanceId with
                        | Disclosed value -> value
                        | NotPresent -> "Not present in this projection"
                        | NotApplicable -> "Not applicable"
                        | ExplicitlyUnknown -> "Explicitly unknown"
                    )
                    Html.dt "Second heading"
                    Html.dd (
                        match unit.Unit.SecondaryHeading with
                        | Disclosed value ->
                            let source =
                                match value.Source with
                                | WeaponHeading -> "weapon"
                                | SensorHeading -> "sensor"
                                | AttentionHeading -> "attention"
                            source
                            + " source explicitly disclosed ("
                            + string (HeadingRadians.value value.Radians)
                            + " radians)"
                        | NotPresent -> "Not present in this projection"
                        | NotApplicable -> "Not applicable"
                        | ExplicitlyUnknown -> "Explicitly unknown"
                    )
                    Html.dt "Selected exact overlays"
                    Html.dd (
                        scene.Overlays
                        |> Array.filter (fun overlay ->
                            match overlay.Overlay.Scope with
                            | SelectedUnitOverlay id -> id = unit.Unit.Id
                            | WholeForceOverlay -> false)
                        |> Array.sumBy _.PathSegments
                        |> fun count -> string count + " rendered path segments"
                    )
                    Html.dt "Committed tick"
                    Html.dd (string scene.Tick)
                ]
        ]
    ]

let private battlefieldView
    (shell: SIR.Client.Model)
    (frameOverride: RenderFrame option)
    (terrainOverride: MapEditorState option)
    (state: BattlefieldViewState)
    (previousFrame: RenderFrame option)
    presentationAlpha
    movementOffsets
    (dispatch: Msg -> unit)
    =
    let loadedFrame =
        frameOverride
        |> Option.orElseWith (fun () -> Shell.renderFrame shell)
    let frame =
        loadedFrame
        |> Option.defaultValue Battlefield.representativeFrame
    let baseScene =
        match previousFrame with
        | Some previous when presentationAlpha < 1.0 ->
            Battlefield.interpolatedScene presentationAlpha previous frame state
        | _ -> Battlefield.scene frame state
    let scene =
        Battlefield.withUnitOffsets movementOffsets baseScene
    let presentationMode, presentationDescription =
        if previousFrame.IsSome && presentationAlpha < 1.0 then
            "interpolated", "interpolated presentation"
        else
            "exact", "exact committed frame"
    let transform =
        "translate("
        + string scene.Camera.PanX
        + " "
        + string scene.Camera.PanY
        + ") scale("
        + string scene.Camera.Zoom
        + ")"
    let disclosure =
        match scene.Disclosure with
        | FullReplayDisclosure -> "Full replay"
        | PerspectiveDisclosure -> "Perspective playback"
        | SandboxDisclosure -> "Simulation sandbox"
    let columns =
        int (scene.Board.MaximumColumn - scene.Board.MinimumColumn + 1)
    let rows =
        int (scene.Board.MaximumRow - scene.Board.MinimumRow + 1)

    Html.section [
        prop.className "panel battlefield-panel"
        prop.ariaLabel (
            if Option.isSome frameOverride then
                "Editable simulation SVG battlefield"
            elif Option.isSome loadedFrame then
                "Loaded replay SVG battlefield"
            else
                "Static SVG battlefield demonstration"
        )
        prop.children [
            Html.div [
                prop.className "battlefield-heading"
                prop.children [
                    Html.div [
                        Html.p [
                            prop.className "eyebrow"
                            prop.text (
                                if Option.isSome frameOverride then
                                    "Editable sandbox projection"
                                elif Option.isSome loadedFrame then
                                    "Loaded bounded worker projection"
                                else
                                    "Static demonstration — no replay loaded"
                            )
                        ]
                        Html.h2 (
                            if Option.isSome frameOverride then
                                "Simulation battlefield"
                            elif Option.isSome loadedFrame then
                                "Replay battlefield"
                            else
                                "SVG battlefield demonstration"
                        )
                        Html.p (
                            disclosure
                            + " · tick "
                            + string scene.Tick
                            + " · "
                            + presentationDescription
                            + " · north is up"
                        )
                    ]
                    Html.div [
                        prop.className "battlefield-controls"
                        prop.children [
                            button "←" "Pan battlefield left" false (fun _ -> dispatch (BattlefieldChanged(PanBy(-24, 0))))
                            button "↑" "Pan battlefield up" false (fun _ -> dispatch (BattlefieldChanged(PanBy(0, -24))))
                            button "↓" "Pan battlefield down" false (fun _ -> dispatch (BattlefieldChanged(PanBy(0, 24))))
                            button "→" "Pan battlefield right" false (fun _ -> dispatch (BattlefieldChanged(PanBy(24, 0))))
                            button "−" "Zoom battlefield out" false (fun _ -> dispatch (BattlefieldChanged(ZoomBy 0.8)))
                            button "+" "Zoom battlefield in" false (fun _ -> dispatch (BattlefieldChanged(ZoomBy 1.25)))
                            button "Reset" "Reset battlefield camera" false (fun _ -> dispatch (BattlefieldChanged ResetCamera))
                        ]
                    ]
                ]
            ]
            Html.div [
                prop.className "battlefield-layout"
                prop.children [
                    Svg.svg [
                        svg.className "battlefield-svg"
                        svg.custom ("role", "application")
                        svg.custom ("aria-label", (
                            disclosure
                            + " battlefield at "
                            + presentationMode
                            + " tick "
                            + string scene.Tick
                            + ", "
                            + string scene.Units.Length
                            + " visible units; selected unit "
                            + (scene.SelectedUnit |> Option.map string |> Option.defaultValue "none")
                        ))
                        svg.viewBox (0, 0, max 1 (int scene.Width), max 1 (int scene.Height))
                        svg.children [
                            Svg.title ("Replay battlefield at tick " + string scene.Tick)
                            Svg.desc "Flat orthographic battlefield sized from the disclosed board. Arrow keys move unit focus; Enter selects; Escape clears selection."
                            Svg.g [
                                svg.custom ("transform", transform)
                                svg.children [
                                    Svg.rect [
                                        svg.custom ("data-layer", "terrain")
                                        svg.x 0
                                        svg.y 0
                                        svg.width scene.Width
                                        svg.height scene.Height
                                        svg.fill scene.Palette.Terrain
                                    ]
                                    match terrainOverride with
                                    | Some editor ->
                                        for row in 0 .. rows - 1 do
                                            for column in 0 .. columns - 1 do
                                                let terrain =
                                                    MapEditor.terrainAt
                                                        (int32 column)
                                                        (int32 row)
                                                        editor
                                                if terrain <> Open then
                                                    let fill, opacity =
                                                        match terrain with
                                                        | Rough -> scene.Palette.Canvas, 0.48
                                                        | Blocked -> scene.Palette.Text, 0.34
                                                        | Objective -> scene.Palette.NeutralFaction, 0.38
                                                        | Open -> scene.Palette.Terrain, 0.0
                                                    Svg.rect [
                                                        svg.custom (
                                                            "data-terrain",
                                                            MapEditor.terrainLabel terrain
                                                        )
                                                        svg.x (float column * scene.CellSize)
                                                        svg.y (float row * scene.CellSize)
                                                        svg.width scene.CellSize
                                                        svg.height scene.CellSize
                                                        svg.fill fill
                                                        svg.custom ("opacity", opacity)
                                                    ]
                                    | None ->
                                        for row in 0 .. rows - 1 do
                                            for column in 0 .. columns - 1 do
                                                if (row + column) % 3 = 0 then
                                                    Svg.rect [
                                                        svg.custom ("data-terrain", "rough")
                                                        svg.x (float column * scene.CellSize)
                                                        svg.y (float row * scene.CellSize)
                                                        svg.width scene.CellSize
                                                        svg.height scene.CellSize
                                                        svg.fill scene.Palette.Canvas
                                                        svg.custom ("opacity", 0.22)
                                                    ]
                                    Svg.g [
                                        svg.custom ("data-layer", "grid")
                                        svg.children [
                                            for index in 0 .. columns do
                                                Svg.line [
                                                    svg.x1 (float index * scene.CellSize)
                                                    svg.y1 0
                                                    svg.x2 (float index * scene.CellSize)
                                                    svg.y2 scene.Height
                                                    svg.stroke scene.Palette.Grid
                                                    svg.strokeWidth 1
                                                ]
                                            for index in 0 .. rows do
                                                Svg.line [
                                                    svg.x1 0
                                                    svg.y1 (float index * scene.CellSize)
                                                    svg.x2 scene.Width
                                                    svg.y2 (float index * scene.CellSize)
                                                    svg.stroke scene.Palette.Grid
                                                    svg.strokeWidth 1
                                                ]
                                        ]
                                    ]
                                    Svg.g [
                                        svg.custom ("data-layer", "overlays")
                                        svg.children [
                                            for overlay in scene.Overlays do
                                                match overlay.Disposition with
                                                | DeclinedUnsafeOverlay _ -> Html.none
                                                | disposition ->
                                                    let points =
                                                        overlay.Points
                                                        |> Array.chunkBySize 2
                                                        |> Array.map (fun pair ->
                                                            string pair[0] + "," + string pair[1])
                                                        |> String.concat " "
                                                    let dispositionName =
                                                        match disposition with
                                                        | ExactOverlay -> "exact"
                                                        | SimplifiedSelectedOverlay _ -> "simplified-selected"
                                                        | AggregatedWholeForceOverlay _ -> "aggregated-whole-force"
                                                        | DeclinedUnsafeOverlay _ -> "declined"
                                                    Svg.polyline [
                                                        svg.custom ("data-overlay-id", overlay.Overlay.Id)
                                                        svg.custom ("data-overlay-kind", overlay.Overlay.Kind)
                                                        svg.custom ("data-overlay-disposition", dispositionName)
                                                        svg.custom ("data-path-segments", string overlay.PathSegments)
                                                        svg.points points
                                                        svg.fill "none"
                                                        svg.stroke scene.Palette.Focus
                                                        svg.strokeWidth 3
                                                        svg.strokeDasharray [| 6; 3 |]
                                                    ]
                                        ]
                                    ]
                                    Svg.g [
                                        svg.custom ("data-layer", "edges")
                                        svg.children [
                                            for edge in scene.Edges do
                                                let color, width, dash =
                                                    match edge.Kind, edge.State with
                                                    | "wall", _ -> scene.Palette.Text, 5.0, "none"
                                                    | "door", "open" -> scene.Palette.NeutralFaction, 3.0, "7 4"
                                                    | "window", _ -> scene.Palette.HumanFaction, 3.0, "2 2"
                                                    | _ -> scene.Palette.Text, 2.0, "3 3"
                                                let endColumn, endRow =
                                                    if edge.Kind = "door" && edge.State = "open" then
                                                        edge.StartColumn
                                                        + edge.EndRow
                                                        - edge.StartRow,
                                                        edge.StartRow
                                                        - edge.EndColumn
                                                        + edge.StartColumn
                                                    else
                                                        edge.EndColumn, edge.EndRow
                                                let startX =
                                                    float (edge.StartColumn - scene.Board.MinimumColumn)
                                                    * scene.CellSize
                                                let startY =
                                                    float (edge.StartRow - scene.Board.MinimumRow)
                                                    * scene.CellSize
                                                let endX =
                                                    float (endColumn - scene.Board.MinimumColumn)
                                                    * scene.CellSize
                                                let endY =
                                                    float (endRow - scene.Board.MinimumRow)
                                                    * scene.CellSize
                                                Svg.line [
                                                    svg.custom ("data-edge-kind", edge.Kind)
                                                    svg.custom ("data-edge-state", edge.State)
                                                    svg.x1 startX
                                                    svg.y1 startY
                                                    svg.x2 endX
                                                    svg.y2 endY
                                                    svg.stroke color
                                                    svg.strokeWidth width
                                                    svg.custom ("strokeDasharray", dash)
                                                ]
                                        ]
                                    ]
                                    Svg.g [
                                        svg.custom ("data-layer", "units")
                                        svg.children [
                                            for unit in scene.Units do
                                                unitView scene dispatch unit
                                        ]
                                    ]
                                    Svg.g [
                                        svg.custom ("data-layer", "effects")
                                        svg.children [
                                            for trace in scene.ActionTraces do
                                                let color, width, dash =
                                                    match trace.Kind with
                                                    | "combat-melee" ->
                                                        scene.Palette.HealthActive, 5, [||]
                                                    | "combat-projectile" ->
                                                        scene.Palette.NeutralFaction, 3, [| 9; 4 |]
                                                    | "combat-lobbed-area" ->
                                                        scene.Palette.HumanFaction, 3, [| 3; 5 |]
                                                    | "combat-spell-area" ->
                                                        scene.Palette.ArcaneFaction, 4, [| 2; 4 |]
                                                    | _ ->
                                                        scene.Palette.HealthActive, 2, [| 3; 3 |]
                                                let projectileX =
                                                    trace.SourceX
                                                    + (trace.TargetX - trace.SourceX) * 0.68
                                                let projectileY =
                                                    trace.SourceY
                                                    + (trace.TargetY - trace.SourceY) * 0.68
                                                Svg.g [
                                                    svg.custom ("data-combat-indicator", trace.Kind)
                                                    svg.custom ("data-action-trace", string trace.EventId)
                                                    svg.children [
                                                        Svg.line [
                                                            svg.custom ("data-action-kind", trace.Kind)
                                                            svg.x1 trace.SourceX
                                                            svg.y1 trace.SourceY
                                                            svg.x2 trace.TargetX
                                                            svg.y2 trace.TargetY
                                                            svg.stroke color
                                                            svg.strokeWidth width
                                                            svg.strokeDasharray dash
                                                        ]
                                                        match trace.Kind with
                                                        | "combat-projectile" ->
                                                            Svg.circle [
                                                                svg.custom ("data-projectile", "true")
                                                                svg.cx projectileX
                                                                svg.cy projectileY
                                                                svg.r 5
                                                                svg.fill color
                                                                svg.stroke scene.Palette.Text
                                                                svg.strokeWidth 1
                                                            ]
                                                            Svg.circle [
                                                                svg.custom ("data-impact", "projectile")
                                                                svg.cx trace.TargetX
                                                                svg.cy trace.TargetY
                                                                svg.r 9
                                                                svg.fill "none"
                                                                svg.stroke color
                                                                svg.strokeWidth 2
                                                            ]
                                                        | "combat-melee" ->
                                                            Svg.line [
                                                                svg.custom ("data-impact", "melee")
                                                                svg.x1 (trace.TargetX - 7.0)
                                                                svg.y1 (trace.TargetY - 7.0)
                                                                svg.x2 (trace.TargetX + 7.0)
                                                                svg.y2 (trace.TargetY + 7.0)
                                                                svg.stroke color
                                                                svg.strokeWidth 4
                                                            ]
                                                            Svg.line [
                                                                svg.x1 (trace.TargetX + 7.0)
                                                                svg.y1 (trace.TargetY - 7.0)
                                                                svg.x2 (trace.TargetX - 7.0)
                                                                svg.y2 (trace.TargetY + 7.0)
                                                                svg.stroke color
                                                                svg.strokeWidth 4
                                                            ]
                                                        | "combat-lobbed-area"
                                                        | "combat-spell-area" ->
                                                            Svg.circle [
                                                                svg.custom ("data-impact", trace.Kind)
                                                                svg.cx trace.TargetX
                                                                svg.cy trace.TargetY
                                                                svg.r 14
                                                                svg.fill "none"
                                                                svg.stroke color
                                                                svg.strokeWidth 3
                                                                svg.strokeDasharray dash
                                                            ]
                                                        | _ -> Html.none
                                                    ]
                                                ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                    Html.div [
                        prop.className "battlefield-sidecar"
                        prop.children [
                            Html.label [
                                prop.htmlFor "battlefield-palette"
                                prop.text "Palette"
                            ]
                            Html.select [
                                prop.id "battlefield-palette"
                                prop.value scene.Palette.Id
                                prop.onChange (fun value -> dispatch (BattlefieldChanged(ChoosePalette value)))
                                prop.children [
                                    for palette in ReplayPalettes.all do
                                        Html.option [
                                            prop.value palette.Id
                                            prop.text palette.Id
                                        ]
                                ]
                            ]
                            Html.p [
                                prop.className "semantic-zoom"
                                prop.text (
                                    string scene.SemanticZoom
                                    + " · "
                                    + string (Math.Round(scene.CellSize * scene.Camera.Zoom))
                                    + " px/cell"
                                )
                            ]
                            if not (Array.isEmpty scene.ActionTraces) then
                                Html.section [
                                    prop.className "combat-indicator-legend"
                                    prop.ariaLabel "Combat indicator legend"
                                    prop.children [
                                        Html.h3 "Combat effects"
                                        Html.span [
                                            Html.i [ prop.className "combat-swatch is-projectile"; prop.ariaHidden true ]
                                            Html.span "Ranged projectile"
                                        ]
                                        Html.span [
                                            Html.i [ prop.className "combat-swatch is-melee"; prop.ariaHidden true ]
                                            Html.span "Melee strike"
                                        ]
                                        Html.span [
                                            Html.i [ prop.className "combat-swatch is-area"; prop.ariaHidden true ]
                                            Html.span "Area delivery"
                                        ]
                                    ]
                                ]
                            Html.label [
                                Html.input [
                                    prop.type'.checkbox
                                    prop.ariaLabel "Use exact-tick playback"
                                    prop.isChecked state.ExactTicks
                                    prop.onChange (fun value ->
                                        dispatch (BattlefieldChanged(ChooseExactTicks value)))
                                ]
                                Html.span " Exact ticks (disable interpolation)"
                            ]
                            Html.label [
                                Html.input [
                                    prop.type'.checkbox
                                    prop.ariaLabel "Use reduced motion"
                                    prop.isChecked state.ReducedMotion
                                    prop.onChange (fun value ->
                                        dispatch (BattlefieldChanged(ChooseReducedMotion value)))
                                ]
                                Html.span " Reduced motion"
                            ]
                            Html.section [
                                prop.className "timeline-lanes"
                                prop.ariaLabel "Semantic replay timeline lanes"
                                prop.children [
                                    Html.h3 "Timeline"
                                    for lane in [ AuthoritativeEvents; UnitActions; Communications ] do
                                        let laneName =
                                            match lane with
                                            | AuthoritativeEvents -> "Disclosed events"
                                            | UnitActions -> "Unit actions"
                                            | Communications -> "Communications"
                                        Html.div [
                                            prop.custom ("data-timeline-lane", laneName)
                                            prop.children [
                                                Html.strong laneName
                                                let items =
                                                    scene.Timeline
                                                    |> Array.filter (fun item -> item.Lane = lane)
                                                if Array.isEmpty items then
                                                    Html.span " — none"
                                                else
                                                    Html.ul [
                                                        for item in items do
                                                            Html.li [
                                                                prop.custom ("data-event-id", string item.EventId)
                                                                prop.text (
                                                                    "T"
                                                                    + string item.Tick
                                                                    + " · "
                                                                    + match item.Summary with
                                                                      | Disclosed text -> text
                                                                      | _ -> "Disclosed event"
                                                                )
                                                            ]
                                                    ]
                                            ]
                                        ]
                                ]
                            ]
                            let adjustedOverlays =
                                scene.Overlays
                                |> Array.filter (fun overlay ->
                                    overlay.Disposition <> ExactOverlay)
                            if not (Array.isEmpty adjustedOverlays) then
                                Html.section [
                                    prop.className "overlay-budget-notices"
                                    prop.ariaLabel "Overlay rendering notices"
                                    prop.children [
                                        Html.h3 "Overlay limits"
                                        for overlay in adjustedOverlays do
                                            Html.p (
                                                match overlay.Disposition with
                                                | SimplifiedSelectedOverlay original ->
                                                    overlay.Overlay.Id
                                                    + ": selected overlay simplified from "
                                                    + string original
                                                    + " to "
                                                    + string overlay.PathSegments
                                                    + " path segments."
                                                | AggregatedWholeForceOverlay original ->
                                                    overlay.Overlay.Id
                                                    + ": whole-force overlays aggregated from "
                                                    + string original
                                                    + " to "
                                                    + string overlay.PathSegments
                                                    + " path segments."
                                                | DeclinedUnsafeOverlay reason ->
                                                    overlay.Overlay.Id
                                                    + ": overlay declined — "
                                                    + reason
                                                    + "."
                                                | ExactOverlay -> ""
                                            )
                                    ]
                                ]
                            Html.section [
                                prop.className "battlefield-legend"
                                prop.ariaLabel "Battlefield legend"
                                prop.children [
                                    Html.h3 "Legend"
                                    Html.ul [
                                        Html.li "Solid / dashed / dotted faction outlines remain distinct in monochrome."
                                        Html.li "Twelve health positions fill from left to right."
                                        Html.li "Perimeter wedge is body facing; the class glyph stays upright."
                                        Html.li "Centre-out dot pointer is an explicitly disclosed weapon or sensor heading, never attention."
                                        Html.li "Ground polylines are bounded exact overlays; whole-force geometry aggregates above its budget."
                                        Html.li "Corner bars show elevation; +N and stance appear only at detailed zoom."
                                        Html.li "Separate ground outline is the authoritative footprint."
                                    ]
                                ]
                            ]
                            battlefieldInspector scene
                        ]
                    ]
                ]
            ]
        ]
    ]

let private inspector (model: SIR.Client.Model) dispatch =
    let inspection =
        model.Inspection
        |> Option.defaultValue
            { Tick = 0
              BoardMinimumColumn = 0
              BoardMinimumRow = 0
              BoardMaximumColumn = 0
              BoardMaximumRow = 0
              Units = []
              Edges = []
              Events = []
              Checkpoints = []
              PerspectiveHash = None }

    let selectedUnit =
        model.Selection.Unit
        |> Option.bind (fun selected ->
            inspection.Units
            |> List.tryFind (fun unit -> unit.Id = selected))

    let selectedEvent =
        model.Selection.Event
        |> Option.bind (fun selected ->
            inspection.Events
            |> List.tryFind (fun event -> event.Id = selected))

    Html.section [
        prop.className "panel inspector-panel"
        prop.ariaLabel "Replay inspector"
        prop.children [
            Html.h2 "Inspector"
            Html.p (
                "Compact projection at tick "
                + string inspection.Tick
                + "; complete world state remains in the worker."
            )
            Html.h3 "Board"
            Html.div [
                prop.className "board"
                prop.role.img
                prop.ariaLabel (
                    "Board from column "
                    + string inspection.BoardMinimumColumn
                    + " row "
                    + string inspection.BoardMinimumRow
                    + " to column "
                    + string inspection.BoardMaximumColumn
                    + " row "
                    + string inspection.BoardMaximumRow
                )
                prop.children (
                    inspection.Units
                    |> List.map (fun unit ->
                        Html.button [
                            prop.type'.button
                            prop.className ("unit-token unit-" + unit.Side.ToLowerInvariant())
                            prop.ariaLabel (
                                "Inspect "
                                + unit.Side
                                + " unit "
                                + string unit.Id
                            )
                            prop.text (unit.Side.Substring(0, 1) + string unit.Id)
                            prop.onClick (fun _ ->
                                dispatch (ShellMsg(UnitSelected(Some unit.Id))))
                        ])
                )
            ]
            Html.h3 "Timeline and events"
            Html.ol [
                prop.className "event-list"
                prop.children (
                    inspection.Events
                    |> List.map (fun event ->
                        Html.li [
                            Html.button [
                                prop.type'.button
                                prop.ariaLabel ("Inspect event " + string event.Id)
                                prop.text (
                                    "T"
                                    + string event.Tick
                                    + " · "
                                    + event.Source
                                    + " · "
                                    + event.Summary
                                )
                                prop.onClick (fun _ ->
                                    dispatch (ShellMsg(EventSelected(Some event.Id))))
                            ]
                        ])
                )
            ]
            Html.h3 "Formula"
            button
                "Attack formula"
                "Inspect attack formula"
                false
                (fun _ ->
                    dispatch (
                        ShellMsg(FormulaSelected(Some "damage = max(0, attack power)"))
                    ))
            Html.h3 "Checkpoints"
            Html.table [
                prop.children [
                    Html.thead [
                        Html.tr [
                            Html.th "Tick"
                            Html.th "State hash"
                            Html.th "Event hash"
                        ]
                    ]
                    Html.tbody [
                        for checkpoint in inspection.Checkpoints do
                            Html.tr [
                                Html.td (string checkpoint.Tick)
                                Html.td checkpoint.StateHash
                                Html.td checkpoint.EventHash
                            ]
                    ]
                ]
            ]
            Html.dl [
                Html.dt "Unit"
                Html.dd (
                    selectedUnit
                    |> Option.map (fun unit ->
                        unit.Side
                        + " "
                        + string unit.Id
                        + " at "
                        + string unit.Column
                        + ","
                        + string unit.Row
                        + "; health "
                        + string unit.Health)
                    |> Option.defaultValue "None"
                )
                Html.dt "Event"
                Html.dd (
                    selectedEvent
                    |> Option.map (fun event -> event.Summary)
                    |> Option.defaultValue "None"
                )
                Html.dt "Formula"
                Html.dd (model.Selection.Formula |> Option.defaultValue "None")
                Html.dt "Perspective hash"
                Html.dd (inspection.PerspectiveHash |> Option.defaultValue "Not applicable")
            ]
        ]
    ]

let private workerStatus (model: SIR.Client.Model) =
    let text =
        match model.Worker with
        | WorkerStarting -> "Worker starting"
        | WorkerReady -> "Worker ready"
        | WorkerBusy batches -> "Worker running · " + string batches + " batches complete"
        | WorkerStopped reason -> "Worker stopped · " + reason

    Html.p [
        prop.className "worker-status"
        prop.text (
            text
            + " · protocol "
            + string WorkerProtocol.CurrentVersion
            + " · batch size "
            + string WorkerProtocol.BatchSize
        )
    ]

let private sandbox (model: SIR.Client.Model) dispatch =
    let scenario = model.Lab.Scenario

    Html.section [
        prop.className "panel sandbox-panel"
        prop.ariaLabel "Sandbox parameters"
        prop.children [
            Html.h2 "Typed parameters"
            Html.p "Every edit creates a derived sandbox; the immutable baseline remains visible in the comparison."
            match scenario with
            | None ->
                Html.p "Load a design scenario or verified replay to edit parameters."
            | Some selected ->
                for parameter in selected.Parameters do
                    let current =
                        model.Patch
                        |> Map.tryFind parameter.Key
                        |> Option.defaultValue parameter.DefaultValue

                    Html.label [
                        prop.htmlFor parameter.Key
                        prop.text (parameter.Label + ": " + string current)
                    ]
                    Html.input [
                        prop.id parameter.Key
                        prop.type'.number
                        prop.min parameter.Minimum
                        prop.max parameter.Maximum
                        prop.step parameter.Step
                        prop.value current
                        prop.onChange (fun (value: int) ->
                            dispatch (
                                ShellMsg(ParameterEdited(parameter.Key, int32 value))
                            ))
                    ]
            model.Lab.ValidationError
            |> Option.map (fun error ->
                Html.p [
                    prop.className "validation-error"
                    prop.role.alert
                    prop.text error
                ])
            |> Option.defaultValue Html.none
        ]
    ]

let private scenarioCatalog model dispatch =
    Html.section [
        prop.className "panel catalog-panel quick-start-panel"
        prop.ariaLabel "Design scenario catalog"
        prop.children [
            Html.p [ prop.className "eyebrow"; prop.text "No replay file required" ]
            Html.h2 "Try an interactive scenario"
            Html.p "Run a fixed example, edit its typed values, compare the fork with the immutable baseline, sweep either parameter, and export the result."
            Html.div [
                prop.className "scenario-grid"
                prop.children [
                    for scenario in Lab.catalog do
                        let selected =
                            model.Lab.Scenario
                            |> Option.exists (fun current ->
                                current.Identity = scenario.Identity)

                        let defaults =
                            scenario.Parameters
                            |> List.map (fun parameter ->
                                parameter.Label
                                + " "
                                + string parameter.DefaultValue)
                            |> String.concat " · "

                        Html.article [
                            prop.className (
                                if selected then
                                    "scenario-card scenario-card-selected"
                                else
                                    "scenario-card"
                            )
                            prop.children [
                                Html.h3 scenario.Title
                                Html.p scenario.Description
                                Html.p [
                                    prop.className "scenario-defaults"
                                    prop.text defaults
                                ]
                                Html.p [
                                    prop.className "identity"
                                    prop.text (
                                        scenario.Identity
                                        + " r"
                                        + string scenario.Revision
                                    )
                                ]
                                button
                                    (if selected then
                                         "Simulate again"
                                     else
                                         "Simulate now")
                                    ("Simulate design scenario " + scenario.Title)
                                    false
                                    (fun _ ->
                                        dispatch (ShellMsg(ScenarioSelected scenario.Identity)))
                            ]
                        ]
                ]
            ]
        ]
    ]

let private resultTable (title: string) (result: ExperimentResult) =
    Html.div [
        Html.h3 title
        Html.p [
            prop.className "identity"
            prop.text (
                "Result "
                + result.ResultIdentity
                + " · engine "
                + result.Input.EngineIdentity.Substring(0, 12)
                + " · rules "
                + result.Input.RulesetIdentity.Substring(0, 12)
            )
        ]
        Html.table [
            Html.thead [
                Html.tr [ Html.th "Metric"; Html.th "Canonical integer result" ]
            ]
            Html.tbody [
                for KeyValue(key, value) in result.Metrics do
                    Html.tr [ Html.td key; Html.td (string value) ]
            ]
        ]
    ]

let private comparisonPanel (model: Model) dispatch =
    let shell = model.Shell
    let report = shell.Lab.Report
    let viewName =
        match model.ComparisonView with
        | Split -> "Linked split"
        | Swipe -> "Linked swipe"
        | DifferenceOverlay -> "Difference overlay"

    let resultPreview label className (result: ExperimentResult) =
        let remaining =
            result.Metrics |> Map.tryFind "remaining-health" |> Option.defaultValue 0
        let frame = Lab.renderFrame result
        let scene = Battlefield.scene frame model.Battlefield
        let transform =
            "translate("
            + string scene.Camera.PanX
            + " "
            + string scene.Camera.PanY
            + ") scale("
            + string scene.Camera.Zoom
            + ")"
        Html.figure [
            prop.className ("comparison-result " + className)
            prop.ariaLabel (label + ", target remaining health " + string remaining)
            prop.children [
                Html.figcaption [
                    Html.strong label
                    Html.span (" · " + result.ResultIdentity)
                ]
                Svg.svg [
                    svg.viewBox (0, 0, 210, 150)
                    svg.custom ("role", "img")
                    svg.custom (
                        "aria-label",
                        label
                        + " battlefield at linked tick "
                        + string shell.Playback.CurrentTick
                        + "; target has "
                        + string remaining
                        + " of 100 health"
                    )
                    svg.custom ("data-comparison-camera", transform)
                    svg.children [
                        Svg.g [
                            svg.custom ("transform", transform)
                            svg.children [
                                Svg.rect [
                                    svg.x 0
                                    svg.y 0
                                    svg.width scene.Width
                                    svg.height scene.Height
                                    svg.fill scene.Palette.Terrain
                                    svg.stroke scene.Palette.Grid
                                ]
                                for column in 0 .. 3 do
                                    Svg.line [
                                        svg.x1 (float column * scene.CellSize)
                                        svg.y1 0
                                        svg.x2 (float column * scene.CellSize)
                                        svg.y2 scene.Height
                                        svg.stroke scene.Palette.Grid
                                    ]
                                for row in 0 .. 2 do
                                    Svg.line [
                                        svg.x1 0
                                        svg.y1 (float row * scene.CellSize)
                                        svg.x2 scene.Width
                                        svg.y2 (float row * scene.CellSize)
                                        svg.stroke scene.Palette.Grid
                                    ]
                                Svg.line [
                                    svg.x1 scene.CellSize
                                    svg.y1 0
                                    svg.x2 (scene.CellSize * 2.0)
                                    svg.y2 0
                                    svg.stroke scene.Palette.Text
                                    svg.strokeWidth 4
                                ]
                                for unit in scene.Units do
                                    let selected =
                                        model.Battlefield.SelectedUnit = Some unit.Unit.Id
                                    let faction =
                                        match unit.Unit.Faction with
                                        | Human -> scene.Palette.HumanFaction
                                        | Arcane -> scene.Palette.ArcaneFaction
                                        | Neutral
                                        | OtherFaction _ -> scene.Palette.NeutralFaction
                                    Svg.g [
                                        svg.custom ("data-comparison-unit", string unit.Unit.Id)
                                        svg.children [
                                            Svg.rect [
                                                svg.x (unit.SymbolCenterX - 14.0)
                                                svg.y (unit.SymbolCenterY - 14.0)
                                                svg.width 28
                                                svg.height 28
                                                svg.fill scene.Palette.Canvas
                                                svg.stroke (if selected then scene.Palette.Focus else faction)
                                                svg.strokeWidth (if selected then 4 else 2)
                                            ]
                                            Svg.text [
                                                svg.x (unit.SymbolCenterX - 8.0)
                                                svg.y (unit.SymbolCenterY + 4.0)
                                                svg.fill scene.Palette.Text
                                                svg.fontSize 10
                                                svg.text (string unit.Unit.Id)
                                            ]
                                        ]
                                    ]
                            ]
                        ]
                        Svg.text [
                            svg.x 8
                            svg.y 142
                            svg.fill "#f5f1e8"
                            svg.fontSize 11
                            svg.text ("Target " + string remaining + " HP")
                        ]
                    ]
                ]
            ]
        ]

    Html.section [
        prop.className "panel comparison-panel"
        prop.ariaLabel "Linked baseline and fork comparison"
        prop.children [
            Html.p [ prop.className "eyebrow"; prop.text "Exploratory simulation comparison" ]
            Html.h2 "Immutable baseline and derived fork"
            Html.p [
                prop.className "comparison-warning"
                prop.role.note
                prop.text "Neither side is a verified replay. Editing always creates a separately identified derived fork; the baseline cannot be edited."
            ]
            match report with
            | None ->
                Html.p "Run a design scenario, then edit a typed parameter to create a linked fork comparison."
            | Some report ->
                let baseline = report.Comparison.Baseline
                let fork = report.Comparison.Fork
                let baselineAttacks =
                    baseline.Metrics |> Map.tryFind "attack-events" |> Option.defaultValue 0
                let forkAttacks =
                    fork.Metrics |> Map.tryFind "attack-events" |> Option.defaultValue 0
                let firstEvent =
                    if baselineAttacks = forkAttacks then "No differing disclosed event"
                    else "Attack event " + string (min baselineAttacks forkAttacks + 1)
                let firstField =
                    report.Comparison.Delta
                    |> Map.toList
                    |> List.tryFind (fun (_, delta) -> delta <> 0)
                    |> Option.map (fun (field, delta) -> field + " (" + (if delta > 0 then "+" else "") + string delta + ")")
                    |> Option.defaultValue "No differing disclosed field"

                Html.div [
                    prop.className (
                        "comparison-viewport comparison-"
                        + (match model.ComparisonView with
                           | Split -> "split"
                           | Swipe -> "swipe"
                           | DifferenceOverlay -> "difference")
                    )
                    prop.custom ("data-linked-camera", "true")
                    prop.custom ("data-linked-selection", "true")
                    prop.custom ("data-linked-tick", string shell.Playback.CurrentTick)
                    prop.custom ("data-linked-overlays", "true")
                    prop.children [
                        resultPreview Comparison.BaselineLabel "comparison-baseline" baseline
                        resultPreview Comparison.ForkLabel "comparison-fork" fork
                    ]
                ]
                Html.p [
                    prop.className "linked-state"
                    prop.text (
                        viewName + " · linked camera, selection, tick "
                        + string shell.Playback.CurrentTick
                        + ", and overlays · selected unit "
                        + (model.Battlefield.SelectedUnit |> Option.map string |> Option.defaultValue "none")
                    )
                ]
                Html.dl [
                    Html.dt "First divergent event"
                    Html.dd firstEvent
                    Html.dt "First differing disclosed field"
                    Html.dd firstField
                    Html.dt "Source link"
                    Html.dd (
                        match shell.Source with
                        | Loaded metadata -> metadata.SourceIdentity
                        | _ -> baseline.Input.ScenarioIdentity
                    )
                ]
                Html.table [
                    Html.caption "Metric deltas (fork − immutable baseline)"
                    Html.thead [ Html.tr [ Html.th "Metric"; Html.th "Delta" ] ]
                    Html.tbody [
                        for KeyValue(metric, delta) in report.Comparison.Delta do
                            Html.tr [ Html.td metric; Html.td (string delta) ]
                    ]
                ]
                Html.div [
                    prop.className "control-row"
                    prop.children [
                        button "Split" "Use linked split comparison" false (fun _ -> dispatch (ComparisonViewChanged Split))
                        button "Swipe" "Use linked swipe comparison" false (fun _ -> dispatch (ComparisonViewChanged Swipe))
                        button "Difference" "Use difference overlay comparison" false (fun _ -> dispatch (ComparisonViewChanged DifferenceOverlay))
                        button "Bookmark" "Bookmark linked comparison tick" false (fun _ -> dispatch AddComparisonBookmark)
                    ]
                ]
                Html.ul [
                    prop.ariaLabel "Comparison bookmarks"
                    prop.children [
                        for bookmark in model.ComparisonBookmarks do
                            Html.li (bookmark.Label + " · tick " + string bookmark.Tick)
                    ]
                ]
            Html.div [
                prop.className "control-row evidence-export-controls"
                prop.children [
                    button "Export safe SVG" "Export sanitized SVG evidence with provenance" false (fun _ -> dispatch ExportEvidenceSvg)
                    button "Export safe PNG" "Export PNG evidence rasterized from sanitized SVG" false (fun _ -> dispatch ExportEvidencePng)
                ]
            ]
            let evidence = evidenceFor model
            Html.p [
                prop.className "evidence-provenance"
                prop.text (
                    "Evidence provenance: source " + evidence.Provenance.SourceIdentity
                    + " · replay " + evidence.Provenance.ReplayIdentity
                    + " · projection " + evidence.Provenance.ProjectionIdentity.Substring(0, 12)
                    + " · palette " + evidence.Provenance.PaletteIdentity
                    + " · renderer " + evidence.Provenance.RendererVersion
                    + " · SHA-256 " + evidence.Sha256.Substring(0, 12)
                )
            ]
        ]
    ]

let private catalogTable
    (label: string)
    (headers: string list)
    (rows: string list list)
    =
    Html.div [
        prop.className "catalog-table-scroll"
        prop.children [
            Html.table [
                prop.ariaLabel label
                prop.children [
                    Html.caption label
                    Html.thead [
                        Html.tr [
                            for header in headers do
                                Html.th header
                        ]
                    ]
                    Html.tbody [
                        for row in rows do
                            Html.tr [
                                for value in row do
                                    Html.td value
                            ]
                    ]
                ]
            ]
        ]
    ]

let private rulesDataCatalog =
    Html.section [
        prop.id "rules-data-tables"
        prop.className "panel rules-data-panel"
        prop.ariaLabel "Rules data tables"
        prop.children [
            Html.p [ prop.className "eyebrow"; prop.text "Inspectable rules catalog" ]
            Html.h2 "Units, perks, weapons, and equipment"
            Html.p "Expand a table below. Canonical roles, proposed content, and prototype balance values are labeled separately; prototype numbers are laboratory inputs, not accepted balance."
            Html.details [
                prop.isOpen true
                prop.children [
                    Html.summary "Units and body profiles"
                    catalogTable
                        "Unit roles"
                        [ "Unit"; "Faction"; "Status"; "Role" ]
                        [ for unit in RulesCatalog.unitRoles do
                              [ unit.Name; unit.Faction; unit.Status; unit.Role ] ]
                    catalogTable
                        "Prototype body profiles"
                        [ "Body"
                          "Status"
                          "HP"
                          "Front armor"
                          "Flank armor"
                          "Rear armor"
                          "Suppression resistance"
                          "Regeneration/s" ]
                        [ for body in RulesCatalog.bodyProfiles do
                              [ body.Name
                                body.Status
                                body.Health
                                body.FrontArmor
                                body.FlankArmor
                                body.RearArmor
                                body.SuppressionResistance
                                body.RegenerationPerSecond ] ]
                ]
            ]
            Html.details [
                Html.summary ("Perks · " + string RulesCatalog.perkProfiles.Length)
                catalogTable
                    "Perk families"
                    [ "Family"; "Perk"; "Tactical change" ]
                    [ for perk in RulesCatalog.perkProfiles do
                          [ perk.Family; perk.Name; perk.TacticalChange ] ]
            ]
            Html.details [
                Html.summary "Weapons and prototype profiles"
                catalogTable
                    "Canonical weapon roles"
                    [ "Weapon"; "Engagement shape"; "Target"; "Tactical role" ]
                    [ for weapon in RulesCatalog.weaponRoles do
                          [ weapon.Name
                            weapon.EngagementShape
                            weapon.Target
                            weapon.TacticalRole ] ]
                catalogTable
                    "Prototype weapon profiles"
                    [ "Weapon"
                      "Kind"
                      "Base engage (s)"
                      "Range slope"
                      "Exponent"
                      "Accuracy"
                      "Dispersion/m"
                      "Damage"
                      "Penetration"
                      "Shots/s"
                      "Effect density"
                      "Suppression/s" ]
                    [ for weapon in RulesCatalog.weaponProfiles do
                          [ weapon.Name
                            weapon.Kind
                            weapon.BaseEngageSeconds
                            weapon.RangeSlope
                            weapon.Exponent
                            weapon.Accuracy
                            weapon.DispersionPerMeter
                            weapon.Damage
                            weapon.Penetration
                            weapon.ShotsPerSecond
                            weapon.EffectDensity
                            weapon.SuppressionPerSecond ] ]
            ]
            Html.details [
                Html.summary "Armor and equipment"
                catalogTable
                    "Human armor packages"
                    [ "Package"; "Coverage"; "Cost" ]
                    [ for armor in RulesCatalog.armorProfiles do
                          [ armor.Name; armor.Coverage; armor.Cost ] ]
                catalogTable
                    "Equipment catalog"
                    [ "Faction"; "Status"; "Category"; "Items" ]
                    [ for equipment in RulesCatalog.equipmentGroups do
                          [ equipment.Faction
                            equipment.Status
                            equipment.Category
                            equipment.Items ] ]
            ]
            Html.p [
                Html.a [
                    prop.href "gameplay-reference.html"
                    prop.text "Read definitions, formulas, and design rationale in the Gameplay Reference."
                ]
            ]
        ]
    ]

let private laboratoryResults model dispatch =
    Html.section [
        prop.className "panel lab-results"
        prop.ariaLabel "Laboratory results"
        prop.children [
            Html.h2 "Simulation result"
            match model.Lab.Report with
            | None ->
                Html.p "Click “Simulate now” on any scenario above. Its deterministic result will appear here."
            | Some report ->
                Html.p [
                    prop.className "evidence-label"
                    prop.text report.EvidenceLabel
                ]
                let remaining =
                    Map.find
                        "remaining-health"
                        report.Comparison.Fork.Metrics

                let damage =
                    Map.find "total-damage" report.Comparison.Fork.Metrics

                let attacks =
                    Map.find
                        "attack-events"
                        report.Comparison.Fork.Metrics

                Html.p [
                    prop.className "simulation-summary"
                    prop.role.status
                    prop.text (
                        string attacks
                        + " attacks resolved · "
                        + string damage
                        + " damage · target finishes on "
                        + string remaining
                        + " HP"
                    )
                ]
                Html.h3 "Attack sequence"
                Html.div [
                    prop.className "simulation-frames"
                    prop.role.img
                    prop.ariaLabel "Target health after each simulated attack"
                    prop.children [
                        for attack, health in Lab.attackFrames report do
                            Html.div [
                                prop.className "simulation-frame"
                                prop.children [
                                    Html.strong (
                                        if attack = 0 then
                                            "Start"
                                        else
                                            "Attack " + string attack
                                    )
                                    Html.meter [
                                        prop.min 0
                                        prop.max 100
                                        prop.value health
                                        prop.ariaLabel (
                                            "Target health after "
                                            + string attack
                                            + " attacks: "
                                            + string health
                                        )
                                    ]
                                    Html.span (string health + " HP")
                                ]
                            ]
                    ]
                ]
                Html.h3 "Baseline and editable fork"
                resultTable "Immutable baseline" report.Comparison.Baseline
                resultTable "Derived fork" report.Comparison.Fork
                Html.h3 "Delta"
                Html.table [
                    Html.thead [
                        Html.tr [ Html.th "Metric"; Html.th "Fork − baseline" ]
                    ]
                    Html.tbody [
                        for KeyValue(key, value) in report.Comparison.Delta do
                            Html.tr [ Html.td key; Html.td (string value) ]
                    ]
                ]
                match model.Lab.Scenario with
                | Some scenario ->
                    Html.div [
                        prop.className "control-row"
                        prop.children [
                            for parameter in scenario.Parameters do
                                button
                                    ("Sweep " + parameter.Label)
                                    ("Run deterministic sweep for " + parameter.Label)
                                    (Option.isSome model.Lab.ValidationError)
                                    (fun _ ->
                                        dispatch (ShellMsg(SweepRequested parameter.Key)))
                        ]
                    ]
                | None -> Html.none
                match report.Sweep with
                | Some sweep ->
                    Html.h3 ("Sweep chart · " + sweep.Parameter)
                    Html.div [
                        prop.className "integer-chart"
                        prop.ariaLabel ("Integer results for " + sweep.Parameter)
                        prop.children [
                            for result in sweep.Results do
                                let parameterValue =
                                    Map.find sweep.Parameter result.Input.Parameters
                                let remaining = Map.find "remaining-health" result.Metrics

                                Html.div [
                                    prop.className "chart-row"
                                    prop.children [
                                        Html.span (string parameterValue)
                                        Html.meter [
                                            prop.min 0
                                            prop.max 200
                                            prop.value remaining
                                            prop.ariaLabel (
                                                string parameterValue
                                                + ": remaining health "
                                                + string remaining
                                            )
                                        ]
                                        Html.span (string remaining)
                                    ]
                                ]
                        ]
                    ]
                | None -> Html.none
                button
                    "Export reproducible experiment"
                    "Export reproducible laboratory experiment"
                    false
                    (fun _ -> dispatch ExportExperiment)
                Html.p "Export includes the exact scenario revision, parameters, engine and ruleset identities, result identities, integer metrics, and optional sweep."
        ]
    ]

let private toolLabel tool =
    match tool with
    | Select -> "Select"
    | Paint terrain -> "Paint " + MapEditor.terrainLabel terrain
    | Terrain tool -> MapEditor.terrainToolLabel tool
    | UnitBrowse -> "Browse unit presets"
    | Place(side, classId, size) ->
        let sideName =
            match side with
            | Blue -> "Blue"
            | Red -> "Red"
            | NeutralSide -> "Neutral"
        "Place " + sideName + " " + classId + " " + string size + "×" + string size
    | Edge(direction, kind) ->
        let directionName =
            match direction with
            | EastEdge -> "east"
            | SouthEdge -> "south"
        let kindName =
            match kind with
            | Wall -> "wall"
            | Door -> "door"
            | Window -> "window"
        "Place " + directionName + " " + kindName

let private editorLayerDisplay domain state =
    if MapEditor.layerState domain state = HiddenLayer then "none" else "inline"

let private editorLayerOpacity domain state =
    if MapEditor.layerState domain state = DimmedLayer then "0.28" else "1"

let private edgeClass state column row direction =
    state.Map.Edges
    |> Map.tryFind (column, row, direction)
    |> Option.map (fun (kind, isOpen) ->
        let kindName =
            match kind with
            | Wall -> "wall"
            | Door -> if isOpen then "door-open" else "door"
            | Window -> "window"
        " edge-"
        + (match direction with
           | EastEdge -> "east-"
           | SouthEdge -> "south-")
        + kindName)
    |> Option.defaultValue ""

let private mapCell state dispatch column row =
    let terrain = MapEditor.terrainAt column row state
    let unit = MapEditor.unitAt column row state
    let label =
        "Cell "
        + string column
        + ","
        + string row
        + "; "
        + MapEditor.terrainLabel terrain
        + (unit
           |> Option.map (fun value ->
               "; unit "
               + string value.Id
               + ", "
               + value.ClassId
               + ", "
               + MapEditor.controllerLabel value.Controller)
           |> Option.defaultValue "")

    Html.button [
        prop.type'.button
        prop.className (
            "map-cell terrain-"
            + MapEditor.terrainLabel terrain
            + edgeClass state column row EastEdge
            + edgeClass state column row SouthEdge
        )
        prop.custom ("data-map-column", string column)
        prop.custom ("data-map-row", string row)
        prop.ariaLabel label
        prop.onClick (fun _ ->
            dispatch (EditorChanged(ActivateCell(column, row))))
    ]

let private mapUnitSymbol (state: MapEditorState) dispatch (unit: EditorUnit) =
    let palette = ReplayPalettes.accessibleDefault
    let faction =
        match unit.Side with
        | Blue -> palette.HumanFaction
        | Red -> palette.ArcaneFaction
        | NeutralSide -> palette.NeutralFaction
    let selected = Set.contains unit.Id state.SelectedUnits

    Html.button [
        prop.type'.button
        prop.className ("map-unit-symbol" + if selected then " is-selected" else "")
        prop.custom ("data-editor-unit-id", string unit.Id)
        prop.style [
            style.custom ("gridColumn", string (unit.Column + 1) + " / span " + string unit.Size)
            style.custom ("gridRow", string (unit.Row + 1) + " / span " + string unit.Size)
        ]
        prop.ariaLabel (
            "Select unit "
            + string unit.Id
            + ", "
            + unit.ClassId
            + ", "
            + string unit.Size
            + " by "
            + string unit.Size
        )
        prop.onClick (fun event ->
            if event.shiftKey then
                dispatch (EditorChanged(ToggleEditorUnitSelection unit.Id))
            else
                dispatch (EditorChanged(SelectEditorUnit(Some unit.Id))))
        prop.children [
            Svg.svg [
                svg.viewBox (0, 0, 48, 48)
                svg.custom ("aria-hidden", "true")
                svg.children [
                    Svg.rect [
                        svg.x 2
                        svg.y 2
                        svg.width 44
                        svg.height 44
                        svg.rx 3
                        svg.fill palette.Canvas
                        svg.stroke faction
                        svg.strokeWidth (if selected then 4 else 2)
                    ]
                    glyphView
                        palette
                        24
                        24
                        1.0
                        (UnitClassId.resolve unit.ClassId)
                ]
            ]
            Html.span [
                prop.className "map-unit-id"
                prop.text (string unit.Id)
            ]
        ]
    ]

[<Emit("$0.setPointerCapture($1)")>]
let private capturePointer (target: EventTarget) (pointerId: int) : unit = jsNative

[<Emit("$0.releasePointerCapture($1)")>]
let private releasePointer (target: EventTarget) (pointerId: int) : unit = jsNative

[<Emit("$0.target.closest('[data-editor-unit-id]')?.getAttribute('data-editor-unit-id') ?? null")>]
let private pointerEditorUnitId (event: Browser.Types.Event) : string = jsNative

[<Emit("(() => { const buttons = Array.from($0.closest('ul').querySelectorAll('button')); const index = buttons.indexOf($0); const target = $1 === -2 ? 0 : ($1 === 2 ? buttons.length - 1 : Math.max(0, Math.min(buttons.length - 1, index + $1))); buttons[target]?.focus(); })()")>]
let private focusEditorObjectList (target: EventTarget) (movement: int) : unit = jsNative

let private editorPointerKind (value: string) =
    match value with
    | "touch" -> TouchPointer
    | "pen" -> PenPointer
    | _ -> MousePointer

let private editorScreenPoint
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
        view.ViewportWidth
        view.ViewportHeight
        width
        height
        (clientX - bounds.left)
        (clientY - bounds.top)

let private editorUnitSvg
    (state: MapEditorState)
    (palette: PaletteTokens)
    dispatch
    (unit: EditorUnit)
    =
    let faction =
        match unit.Side with
        | Blue -> palette.HumanFaction
        | Red -> palette.ArcaneFaction
        | NeutralSide -> palette.NeutralFaction
    let selected = Set.contains unit.Id state.SelectedUnits
    let x = float unit.Column * Battlefield.CellSize
    let y = float unit.Row * Battlefield.CellSize
    let size = float unit.Size * Battlefield.CellSize

    Svg.g [
        svg.key ("editor-unit-" + string unit.Id)
        svg.custom ("data-editor-object", "unit")
        svg.custom ("data-editor-unit-id", string unit.Id)
        svg.custom ("role", "button")
        svg.custom (
            "aria-label",
            "Select unit "
            + string unit.Id
            + ", "
            + unit.ClassId
            + ", "
            + string unit.Size
            + " by "
            + string unit.Size
        )
        svg.tabIndex (if state.SelectedUnit = Some unit.Id then 0 else -1)
        svg.onClick (fun event ->
            event.stopPropagation ()
            if event.shiftKey then
                dispatch (EditorChanged(ToggleEditorUnitSelection unit.Id))
            else
                dispatch (EditorChanged(SelectEditorUnit(Some unit.Id))))
        svg.onKeyDown (fun event ->
            match event.key with
            | "Enter"
            | " " ->
                event.preventDefault ()
                dispatch (
                    EditorChanged(
                        if event.shiftKey then
                            ToggleEditorUnitSelection unit.Id
                        else
                            SelectEditorUnit(Some unit.Id)
                    )
                )
            | "Escape" ->
                event.preventDefault ()
                dispatch (EditorChanged(SelectEditorUnit None))
            | _ -> ())
        svg.children [
            Svg.title (
                "Unit "
                + string unit.Id
                + ": "
                + unit.ClassId
                + ", "
                + string unit.Size
                + " square"
            )
            Svg.rect [
                svg.x (x + 4.0)
                svg.y (y + 4.0)
                svg.width (size - 8.0)
                svg.height (size - 8.0)
                svg.rx 5
                svg.fill palette.Canvas
                svg.stroke (if selected then palette.Focus else faction)
                svg.strokeWidth (if selected then 5 else 3)
                svg.custom ("vector-effect", "non-scaling-stroke")
            ]
            glyphView
                palette
                (x + size / 2.0)
                (y + size / 2.0)
                (max 1.0 ((size - 16.0) / 24.0))
                (UnitClassId.resolve unit.ClassId)
            Svg.text [
                svg.x (x + size - 8.0)
                svg.y (y + size - 8.0)
                svg.custom ("text-anchor", "end")
                svg.fill palette.Text
                svg.custom ("font-size", 12)
                svg.custom ("font-weight", 700)
                svg.text (string unit.Id)
            ]
        ]
    ]

let private editorBattlefield
    (state: MapEditorState)
    (view: EditorWorkspaceState)
    activeDomain
    spacePressed
    dispatch
    =
    let palette = ReplayPalettes.accessibleDefault
    let boardWidth = float state.Map.Width * Battlefield.CellSize
    let boardHeight = float state.Map.Height * Battlefield.CellSize
    let transform =
        "translate("
        + string view.Camera.PanX
        + " "
        + string view.Camera.PanY
        + ") scale("
        + string view.Camera.Zoom
        + ")"
    let activateAt screenX screenY =
        match state.Tool with
        | Edge(_, kind) ->
            MapEditorWorkspace.tryHitEdge
                state.Map.Width
                state.Map.Height
                view.Camera
                MapEditorWorkspace.EdgeTolerancePixels
                screenX
                screenY
            |> Option.iter (fun hit ->
                dispatch (
                    EditorChanged(
                        ActivateEdge(hit.Column, hit.Row, hit.Direction)
                    )
                ))
        | tool ->
            MapEditorWorkspace.tryHitCell
                state.Map.Width
                state.Map.Height
                view.Camera
                screenX
                screenY
            |> Option.iter (fun hit ->
                match tool with
                | Select -> ()
                | Terrain _ -> ()
                | _ -> dispatch (EditorChanged(ActivateCell(hit.Column, hit.Row))))
    let selectionAt screenX screenY action =
        MapEditorWorkspace.tryHitCell
            state.Map.Width
            state.Map.Height
            view.Camera
            screenX
            screenY
        |> Option.iter (fun hit ->
            action
                { CellColumn = hit.Column
                  CellRow = hit.Row }
            |> EditorChanged
            |> dispatch)
    let terrainAt screenX screenY action =
        MapEditorWorkspace.tryHitCell
            state.Map.Width
            state.Map.Height
            view.Camera
            screenX
            screenY
        |> Option.iter (fun hit ->
            action
                { CellColumn = hit.Column
                  CellRow = hit.Row }
            |> EditorChanged
            |> dispatch)

    Html.section [
        prop.className "editor-canvas"
        prop.ariaLabel "SVG tactical map workspace"
        prop.custom ("data-editor-revision", state.Revision.Digest)
        prop.custom ("data-editor-revision-state", string state.RevisionState)
        prop.children [
            Html.p [
                prop.className "sr-only"
                prop.ariaLive.polite
                prop.ariaAtomic true
                prop.text state.TerrainAnnouncement
            ]
            Html.p [
                prop.className "sr-only"
                prop.ariaLive.polite
                prop.ariaAtomic true
                prop.text state.UnitAnnouncement
            ]
            Html.p [
                prop.className "sr-only"
                prop.ariaLive.polite
                prop.ariaAtomic true
                prop.text state.EdgeAnnouncement
            ]
            Html.p [
                prop.className "sr-only"
                prop.ariaLive.polite
                prop.ariaAtomic true
                prop.text state.RegionAnnouncement
            ]
            Html.div [
                prop.className (
                    "editor-context-hud"
                    + if state.SelectedUnits.IsEmpty then " is-hidden" else ""
                )
                prop.role.toolbar
                prop.ariaHidden state.SelectedUnits.IsEmpty
                prop.ariaLabel "Selected unit quick actions"
                prop.children [
                    button "Move" "Begin a resettable movement preview (M)" state.SelectedUnits.IsEmpty (fun _ ->
                        dispatch (
                            EditorChanged(
                                BeginUnitMove state.KeyboardCursor.Cell
                            )
                        ))
                    button "Duplicate" "Duplicate selected formation (Ctrl or Command D)" state.SelectedUnits.IsEmpty (fun _ ->
                        dispatch (EditorChanged DuplicateEditorSelection))
                    button "Delete" "Delete selected formation" state.SelectedUnits.IsEmpty (fun _ ->
                        dispatch (EditorChanged DeleteEditorSelection))
                ]
            ]
            Svg.svg [
                svg.id "editor-map-stage"
                svg.className (
                    "editor-battlefield-svg"
                    + if view.ReducedMotion then " reduced-motion" else ""
                )
                svg.custom ("role", "application")
                svg.tabIndex 0
                svg.custom ("aria-label", (
                    "Editable SVG battlefield, "
                    + string state.Map.Width
                    + " by "
                    + string state.Map.Height
                    + " cells, "
                    + string state.Map.Units.Count
                    + " units. Wheel zooms around the pointer; middle or right drag, Space drag, and two-finger touch pan."
                ))
                svg.viewBox (
                    0,
                    0,
                    max 1 (int view.ViewportWidth),
                    max 1 (int view.ViewportHeight)
                )
                svg.onContextMenu (fun event -> event.preventDefault ())
                svg.onKeyDown (fun event ->
                    let arrowDelta =
                        match event.key with
                        | "ArrowLeft" -> Some(-1, 0)
                        | "ArrowRight" -> Some(1, 0)
                        | "ArrowUp" -> Some(0, -1)
                        | "ArrowDown" -> Some(0, 1)
                        | _ -> None
                    let panelShortcut =
                        match event.key with
                        | "t"
                        | "T" -> Some TerrainTools
                        | "u"
                        | "U" -> Some UnitTools
                        | "e"
                        | "E" -> Some EdgeTools
                        | "z"
                        | "Z" -> Some ZoneTools
                        | "m"
                        | "M" -> Some DocumentTools
                        | _ -> None
                    let m5Command =
                        if
                            state.PendingDestructiveChange.IsSome
                            || event.ctrlKey
                            || event.metaKey
                            || event.altKey
                        then
                            None
                        else
                            let facts =
                                { Editor = state
                                  ActiveDomain = activeDomain
                                  PanHeld = spacePressed
                                  InputHelpExpanded = false }
                            let input =
                                { Key = NormalizedKey.create event.key None
                                  Modifiers =
                                    { ControlOrMeta = false
                                      Shift = event.shiftKey
                                      Alt = false }
                                  Phase = KeyDown }
                            match
                                ModalInput.resolve
                                    (ModalInput.deriveEditorContexts facts)
                                    input
                                    event.repeat
                                    (ModalInput.editorCatalog facts)
                            with
                            | Resolved resolved when
                                resolved.Id.StartsWith("editor.edge.", StringComparison.Ordinal)
                                || resolved.Id.StartsWith("editor.region.", StringComparison.Ordinal)
                                || resolved.Id.StartsWith("editor.document.", StringComparison.Ordinal)
                                ->
                                Some resolved.Command
                            | _ -> None
                    if spacePressed then
                        match arrowDelta with
                        | Some(columnDelta, rowDelta) ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            let distance = if event.shiftKey then 120.0 else 40.0
                            dispatch (
                                EditorWorkspaceChanged(
                                    PanEditorBy(
                                        -float columnDelta * distance,
                                        -float rowDelta * distance
                                    )
                                )
                            )
                        | None when event.key = "Escape" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            dispatch (KeyReleased " ")
                        | None when event.key = " " -> ()
                        | None ->
                            event.preventDefault ()
                            event.stopPropagation ()
                    elif event.key = " " then
                        // Space is exclusively a held pan layer.
                        event.preventDefault ()
                        event.stopPropagation ()
                        if not event.repeat then
                            dispatch (KeyPressed(" ", false, false, false, false))
                    elif m5Command.IsSome then
                        event.preventDefault ()
                        event.stopPropagation ()
                        match m5Command.Value with
                        | EditorCommand action ->
                            dispatch (EditorChanged action)
                        | EditorWorkspaceCommand action ->
                            dispatch (EditorWorkspaceChanged action)
                        | EditorDocumentCommand ExportMapDocument ->
                            dispatch ExportMap
                        | EditorDocumentCommand ExportRepositoryDesignBundle ->
                            dispatch ExportDesignBundle
                        | EditorDocumentCommand OpenMapImport ->
                            openFilePickerAfterRender "editor-map-import"
                        | EditorDocumentCommand(FocusDocumentControl target) ->
                            focusElementAfterRender (
                                match target with
                                | MapImportControl -> "editor-map-import"
                                | LayerStateControls -> "editor-layer-controls"
                                | LocalBackgroundControls -> "editor-background-file"
                                | MapDimensionControls -> "map-width"
                                | SavedViewControls -> "editor-saved-view-controls"
                            )
                        | _ -> ()
                    elif
                        panelShortcut.IsSome
                        && not event.ctrlKey
                        && not event.metaKey
                        && not event.altKey
                        && not (
                            (event.key = "m" || event.key = "M")
                            && state.Tool = Select
                            && not state.SelectedUnits.IsEmpty
                        )
                    then
                        event.preventDefault ()
                        event.stopPropagation ()
                        dispatch (
                            EditorToolPanelChanged panelShortcut.Value
                        )
                        if panelShortcut.Value = TerrainTools then
                            dispatch (
                                EditorChanged(
                                    ChooseTool(Terrain state.LastTerrainPaintTool)
                                )
                            )
                        elif panelShortcut.Value = UnitTools then
                            dispatch (EditorChanged(ChooseTool UnitBrowse))
                    elif event.key = "F2" then
                        event.preventDefault ()
                        event.stopPropagation ()
                        if not event.repeat then
                            dispatch ToggleEditorToolPanelVisibility
                    elif event.key = "F3" then
                        event.preventDefault ()
                        event.stopPropagation ()
                        if not event.repeat then
                            dispatch (
                                EditorWorkspaceChanged
                                    ToggleEditorInspector
                            )
                    elif
                        state.PendingDestructiveChange.IsSome
                        && (event.key = "Enter" || event.key = "Escape")
                        && not event.ctrlKey
                        && not event.metaKey
                        && not event.shiftKey
                        && not event.altKey
                    then
                        event.preventDefault ()
                        event.stopPropagation ()
                        if not event.repeat then
                            dispatch (
                                EditorChanged(
                                    if event.key = "Enter" then
                                        ConfirmDestructiveChange
                                    else
                                        CancelDestructiveChange
                                )
                            )
                    elif event.ctrlKey || event.metaKey then
                        let action =
                            match event.key, event.shiftKey with
                            | ("z" | "Z"), true -> Some RedoEditorCommand
                            | ("z" | "Z"), false -> Some UndoEditorCommand
                            | ("y" | "Y"), _ -> Some RedoEditorCommand
                            | ("c" | "C"), _ -> Some CopyEditorSelection
                            | ("v" | "V"), _ -> Some PasteEditorClipboard
                            | ("d" | "D"), _ -> Some DuplicateEditorSelection
                            | ("a" | "A"), _ -> Some SelectAllInActiveDomain
                            | _ -> None
                        action
                        |> Option.iter (fun editorAction ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            dispatch (EditorChanged editorAction))
                    elif event.altKey then
                        let offset =
                            match event.key with
                            | "ArrowLeft" -> Some(-1, 0)
                            | "ArrowRight" -> Some(1, 0)
                            | "ArrowUp" -> Some(0, -1)
                            | "ArrowDown" -> Some(0, 1)
                            | _ -> None
                        offset
                        |> Option.iter (fun (columnDelta, rowDelta) ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat && not state.SelectedUnits.IsEmpty then
                                let anchor = state.KeyboardCursor.Cell
                                dispatch (EditorChanged(BeginUnitMove anchor))
                                dispatch (
                                    EditorChanged(
                                        ExtendUnitMove
                                            { CellColumn =
                                                max 0 (min (state.Map.Width - 1) (anchor.CellColumn + int32 columnDelta))
                                              CellRow =
                                                max 0 (min (state.Map.Height - 1) (anchor.CellRow + int32 rowDelta)) }
                                    )
                                ))
                    elif event.key = "v" || event.key = "V" then
                        event.preventDefault ()
                        event.stopPropagation ()
                        if not event.repeat then
                            dispatch (EditorChanged(ChooseTool Select))
                    elif event.key = "Delete" || event.key = "Backspace" then
                        match state.Gesture with
                        | TerrainGesture _ when event.key = "Backspace" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            dispatch (EditorChanged ResetTerrainPreview)
                        | UnitMoveGesture _ when event.key = "Backspace" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then
                                dispatch (EditorChanged ResetUnitMovePreview)
                        | _ ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then
                                dispatch (EditorChanged DeleteEditorSelection)
                    elif state.Tool = UnitBrowse then
                        let action =
                            match event.key with
                            | "ArrowUp"
                            | "[" -> Some(MoveUnitPaletteCursor -1)
                            | "ArrowDown"
                            | "]" -> Some(MoveUnitPaletteCursor 1)
                            | "PageUp" -> Some(PageUnitPaletteFaction -1)
                            | "PageDown" -> Some(PageUnitPaletteFaction 1)
                            | "Home" -> Some(SelectUnitPaletteBoundary false)
                            | "End" -> Some(SelectUnitPaletteBoundary true)
                            | "Enter" -> Some ArmUnitPalettePreset
                            | "Escape" -> Some(ChooseTool Select)
                            | _ -> None
                        match action with
                        | Some action ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat
                               || event.key = "ArrowUp"
                               || event.key = "ArrowDown"
                               || event.key = "["
                               || event.key = "]"
                               || event.key = "PageUp"
                               || event.key = "PageDown"
                            then
                                dispatch (EditorChanged action)
                        | None when event.key = "/" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            document.getElementById("editor-unit-preset-search").focus ()
                        | None -> ()
                    elif
                        match state.Tool with
                        | Place _ -> true
                        | _ -> false
                    then
                        let action =
                            match arrowDelta, event.key with
                            | Some(columnDelta, rowDelta), _ ->
                                Some(
                                    MoveUnitPlacementCursor(
                                        int32 columnDelta,
                                        int32 rowDelta
                                    )
                                )
                            | None, "[" -> Some(CycleArmedUnitPreset -1)
                            | None, "]" -> Some(CycleArmedUnitPreset 1)
                            | None, "Enter" ->
                                Some(CommitUnitPlacement event.shiftKey)
                            | None, ("b" | "B" | "Escape") ->
                                Some ReturnToUnitBrowse
                            | _ -> None
                        action
                        |> Option.iter (fun action ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat
                               || arrowDelta.IsSome
                               || event.key = "["
                               || event.key = "]"
                            then
                                dispatch (EditorChanged action))
                    elif state.Tool = Select then
                        match state.Gesture, arrowDelta, event.key with
                        | CommandPreviewGesture _, None, "Enter" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then
                                dispatch (EditorChanged CommitEditorGesture)
                        | CommandPreviewGesture _, None, "Escape" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then
                                dispatch (EditorChanged CancelEditorGesture)
                        | SelectedObjectActionsGesture, None, ("c" | "C") ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then dispatch (EditorChanged CopyEditorSelection)
                        | SelectedObjectActionsGesture, None, ("d" | "D") ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then dispatch (EditorChanged DuplicateEditorSelection)
                        | SelectedObjectActionsGesture, None, ("i" | "I") ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then
                                dispatch (EditorWorkspaceChanged ToggleEditorInspector)
                        | SelectedObjectActionsGesture, None, "Escape" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            dispatch (EditorChanged CancelEditorGesture)
                        | BoxSelectionGesture(_, current), Some(columnDelta, rowDelta), _ ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            dispatch (
                                EditorChanged(
                                    ExtendEditorBoxSelection
                                        { CellColumn =
                                            max 0 (min (state.Map.Width - 1) (current.CellColumn + int32 columnDelta))
                                          CellRow =
                                            max 0 (min (state.Map.Height - 1) (current.CellRow + int32 rowDelta)) }
                                )
                            )
                        | BoxSelectionGesture(anchor, current), None, "Enter" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then
                                let box =
                                    { FirstColumn = anchor.CellColumn
                                      FirstRow = anchor.CellRow
                                      LastColumn = current.CellColumn
                                      LastRow = current.CellRow }
                                dispatch (
                                    EditorChanged(
                                        if event.shiftKey then AddEditorUnitsInBox box
                                        else SelectEditorUnitsInBox box
                                    )
                                )
                        | BoxSelectionGesture _, None, "Escape" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            dispatch (EditorChanged CancelEditorGesture)
                        | UnitMoveGesture(_, current, _, _), Some(columnDelta, rowDelta), _ ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            let distance = if event.shiftKey then 5 else 1
                            dispatch (
                                EditorChanged(
                                    ExtendUnitMove
                                        { CellColumn =
                                            max 0 (min (state.Map.Width - 1) (current.CellColumn + int32 (columnDelta * distance)))
                                          CellRow =
                                            max 0 (min (state.Map.Height - 1) (current.CellRow + int32 (rowDelta * distance))) }
                                )
                            )
                        | UnitMoveGesture _, None, "Enter" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then
                                dispatch (EditorChanged CommitEditorGesture)
                        | UnitMoveGesture _, None, "Escape" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then
                                dispatch (EditorChanged CancelEditorGesture)
                        | IdleGesture, Some(columnDelta, rowDelta), _ ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            dispatch (
                                EditorChanged(
                                    MoveEditorKeyboardCursor(int32 columnDelta, int32 rowDelta)
                                )
                            )
                        | IdleGesture, None, "Enter" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then
                                dispatch (EditorChanged(ActivateEditorKeyboardCursor event.shiftKey))
                        | IdleGesture, None, ("n" | "N") ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then dispatch (EditorChanged(CycleEditorKeyboardObject 1))
                        | IdleGesture, None, ("p" | "P") ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then dispatch (EditorChanged(CycleEditorKeyboardObject -1))
                        | IdleGesture, None, ("b" | "B") ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then dispatch (EditorChanged BeginKeyboardBoxSelection)
                        | IdleGesture, None, ("a" | "A") ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then dispatch (EditorChanged SelectAllInActiveDomain)
                        | IdleGesture, None, ("m" | "M") when not state.SelectedUnits.IsEmpty ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            if not event.repeat then
                                dispatch (
                                    EditorChanged(
                                        BeginUnitMove state.KeyboardCursor.Cell
                                    )
                                )
                        | _ -> ()
                    elif
                        match state.Tool with
                        | Terrain _ -> true
                        | _ -> false
                    then
                        match arrowDelta with
                        | Some(columnDelta, rowDelta) ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            dispatch (
                                EditorChanged(
                                    MoveTerrainCursor(
                                        int32 columnDelta,
                                        int32 rowDelta,
                                        event.shiftKey
                                    )
                                )
                            )
                        | None when event.key = "Enter" || event.key = " " ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            dispatch (EditorChanged ActivateTerrainCursor)
                        | None ->
                            let action =
                                match event.key with
                                | "1" -> Some(ChooseTerrain Open)
                                | "2" -> Some(ChooseTerrain Rough)
                                | "3" -> Some(ChooseTerrain Blocked)
                                | "4" -> Some(ChooseTerrain Objective)
                                | "[" -> Some(SetTerrainBrushSize(state.BrushSize - 1))
                                | "]" -> Some(SetTerrainBrushSize(state.BrushSize + 1))
                                | "p"
                                | "P" -> Some(ChooseTool(Terrain PencilTool))
                                | "r"
                                | "R" -> Some(ChooseTool(Terrain RectangleTool))
                                | "l"
                                | "L" -> Some(ChooseTool(Terrain LineTool))
                                | "g"
                                | "G" -> Some(ChooseTool(Terrain FloodFillTool))
                                | "i"
                                | "I" -> Some(ChooseTool(Terrain EyedropperTool))
                                | "x"
                                | "X" -> Some(ChooseTool(Terrain EraseTool))
                                | "Escape" when state.Gesture = IdleGesture ->
                                    Some(
                                        match state.Tool with
                                        | Terrain EyedropperTool ->
                                            ChooseTool(Terrain state.LastTerrainPaintTool)
                                        | _ -> ChooseTool Select
                                    )
                                | _ -> None
                            action
                            |> Option.iter (fun action ->
                                event.preventDefault ()
                                event.stopPropagation ()
                                if not event.repeat then dispatch (EditorChanged action))
                    elif
                        match state.Tool with
                        | Edge _ -> true
                        | _ -> false
                    then
                        let movement =
                            match event.key with
                            | "ArrowLeft" -> Some(-1, 0)
                            | "ArrowRight" -> Some(1, 0)
                            | "ArrowUp" -> Some(0, -1)
                            | "ArrowDown" -> Some(0, 1)
                            | _ -> None
                        match movement with
                        | Some(columnDelta, rowDelta) ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            dispatch (
                                EditorChanged(
                                    MoveEdgeCursor(
                                        int32 columnDelta,
                                        int32 rowDelta,
                                        event.shiftKey
                                    )
                                )
                            )
                        | None when event.key = "Enter" ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            match state.Gesture with
                            | EdgePolylineGesture _ ->
                                dispatch (EditorChanged FinishEdgePolyline)
                            | _ -> dispatch (EditorChanged ActivateEdgeCursor)
                        | None when event.key = " " ->
                            event.preventDefault ()
                            event.stopPropagation ()
                            dispatch (EditorChanged ActivateEdgeCursor)
                        | None -> ())
                svg.onWheel (fun event ->
                    event.preventDefault ()
                    let x, y =
                        editorScreenPoint
                            view
                            event.currentTarget
                            event.clientX
                            event.clientY
                    let factor = if event.deltaY < 0.0 then 1.12 else 1.0 / 1.12
                    dispatch (EditorWorkspaceChanged(ZoomEditorAt(x, y, factor))))
                svg.onClick (fun event ->
                    let x, y =
                        editorScreenPoint
                            view
                            event.currentTarget
                            event.clientX
                            event.clientY
                    activateAt x y
                    if event.detail >= 2 then
                        match state.Tool with
                        | Edge _ ->
                            event.preventDefault ()
                            dispatch (EditorChanged FinishEdgePolyline)
                        | _ -> ())
                svg.onPointerDown (fun event ->
                    let kind = editorPointerKind event.pointerType
                    let terrainToolActive =
                        match state.Tool with
                        | Terrain _ -> true
                        | _ -> false
                    let requestsPan =
                        (kind = TouchPointer
                         && (not terrainToolActive
                             || not view.CapturedPointers.IsEmpty))
                        || event.button = 1
                        || event.button = 2
                        || spacePressed
                    let requestsSelection =
                        state.Tool = Select
                        && kind <> TouchPointer
                        && event.button = 0
                        && not requestsPan
                    let movementUnit =
                        if requestsSelection then
                            match Int32.TryParse(pointerEditorUnitId event) with
                            | true, unitId -> Some unitId
                            | _ -> None
                        else
                            None
                    let requestsMovement = movementUnit.IsSome
                    let requestsTerrain =
                        match state.Tool with
                        | Terrain _ ->
                            event.button = 0
                            && not requestsPan
                        | _ -> false
                    if requestsPan || requestsSelection || requestsTerrain then
                        event.preventDefault ()
                        capturePointer event.currentTarget (int event.pointerId)
                        let x, y =
                            editorScreenPoint
                                view
                                event.currentTarget
                                event.clientX
                                event.clientY
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
                        if requestsMovement then
                            movementUnit
                            |> Option.iter (fun unitId ->
                                if not (Set.contains unitId state.SelectedUnits) then
                                    dispatch (EditorChanged(SelectEditorUnit(Some unitId))))
                            selectionAt x y BeginUnitMove
                        elif requestsSelection then
                            selectionAt x y BeginEditorBoxSelection
                        elif requestsTerrain then
                            terrainAt x y BeginTerrainGesture
                        elif
                            requestsPan
                            && kind = TouchPointer
                            && terrainToolActive
                        then
                            dispatch (EditorChanged CancelEditorGesture))
                svg.onPointerMove (fun event ->
                    if Map.containsKey (int32 event.pointerId) view.CapturedPointers then
                        event.preventDefault ()
                        let x, y =
                            editorScreenPoint
                                view
                                event.currentTarget
                                event.clientX
                                event.clientY
                        let previous = Map.find (int32 event.pointerId) view.CapturedPointers
                        dispatch (
                            EditorWorkspaceChanged(
                                MoveEditorPointer
                                    { previous with X = x; Y = y }
                            )
                        )
                        if state.Tool = Select && not previous.RequestsPan then
                            match state.Gesture with
                            | UnitMoveGesture _ -> selectionAt x y ExtendUnitMove
                            | _ -> selectionAt x y ExtendEditorBoxSelection
                        else
                            match state.Tool with
                            | Terrain _ when not previous.RequestsPan ->
                                terrainAt x y ExtendTerrainGesture
                            | _ -> ()
                    else
                        match state.Tool with
                        | Place _ ->
                            let x, y =
                                editorScreenPoint
                                    view
                                    event.currentTarget
                                    event.clientX
                                    event.clientY
                            selectionAt x y PreviewUnitPlacement
                        | _ -> ())
                svg.onPointerUp (fun event ->
                    if Map.containsKey (int32 event.pointerId) view.CapturedPointers then
                        let previous = Map.find (int32 event.pointerId) view.CapturedPointers
                        let x, y =
                            editorScreenPoint
                                view
                                event.currentTarget
                                event.clientX
                                event.clientY
                        releasePointer event.currentTarget (int event.pointerId)
                        dispatch (
                            EditorWorkspaceChanged(
                                EndEditorPointer(int32 event.pointerId)
                            )
                        )
                        if state.Tool = Select && not previous.RequestsPan then
                            match state.Gesture with
                            | UnitMoveGesture _ -> selectionAt x y ExtendUnitMove
                            | _ -> selectionAt x y ExtendEditorBoxSelection
                            dispatch (EditorChanged CommitEditorGesture)
                        else
                            match state.Tool with
                            | Terrain _ when not previous.RequestsPan ->
                                terrainAt x y ExtendTerrainGesture
                                dispatch (EditorChanged CommitEditorGesture)
                            | _ -> ())
                svg.onLostPointerCapture (fun event ->
                    dispatch (
                        EditorWorkspaceChanged(
                            LoseEditorPointerCapture(int32 event.pointerId)
                        )
                    )
                    match state.Tool with
                    | Select
                    | Terrain _ -> dispatch (EditorChanged CancelEditorGesture)
                    | _ -> ())
                svg.children [
                    Svg.title "S.I.R. map editor battlefield"
                    Svg.desc "An SVG square-grid battlefield. Empty cells and semantic objects are also available in the object list after the workspace."
                    Svg.rect [
                        svg.width view.ViewportWidth
                        svg.height view.ViewportHeight
                        svg.fill palette.Canvas
                    ]
                    Svg.g [
                        svg.custom ("transform", transform)
                        svg.children [
                            match view.Background with
                            | Some background ->
                                let x, y, width, height =
                                    MapEditorWorkspace.backgroundRenderBox
                                        state.Map.Width
                                        state.Map.Height
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
                                        svg.custom ("pointer-events", "none")
                                        svg.custom ("aria-hidden", "true")
                                        svg.children [
                                            Svg.image [
                                                svg.custom ("href", background.DataUrl)
                                                svg.custom ("x", "0")
                                                svg.custom ("y", "0")
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
                                        svg.custom (
                                            "preserveAspectRatio",
                                            if background.Fit = FillAndCrop then
                                                "xMidYMid slice"
                                            else "none"
                                        )
                                        svg.custom ("pointer-events", "none")
                                        svg.custom ("aria-hidden", "true")
                                    ]
                            | None -> ()
                            Svg.rect [
                                svg.custom ("data-layer", "terrain")
                                svg.custom ("display", editorLayerDisplay TerrainDomain state)
                                svg.custom ("opacity", editorLayerOpacity TerrainDomain state)
                                svg.width boardWidth
                                svg.height boardHeight
                                svg.fill palette.Terrain
                                if view.Background.IsSome then
                                    svg.fillOpacity 0.58
                            ]
                            for row in 0 .. int state.Map.Height - 1 do
                                for column in 0 .. int state.Map.Width - 1 do
                                    let terrain =
                                        MapEditor.terrainAt (int32 column) (int32 row) state
                                    if terrain <> Open then
                                        let fill, opacity =
                                            match terrain with
                                            | Rough -> palette.Canvas, 0.72
                                            | Blocked -> palette.Text, 0.38
                                            | Objective -> palette.NeutralFaction, 0.55
                                            | Open -> palette.Terrain, 0.0
                                        Svg.rect [
                                            svg.custom (
                                                "data-terrain",
                                                MapEditor.terrainLabel terrain
                                            )
                                            svg.custom ("display", editorLayerDisplay TerrainDomain state)
                                            svg.x (float column * Battlefield.CellSize)
                                            svg.y (float row * Battlefield.CellSize)
                                            svg.width Battlefield.CellSize
                                            svg.height Battlefield.CellSize
                                            svg.fill fill
                                            svg.custom (
                                                "opacity",
                                                if MapEditor.layerState TerrainDomain state = DimmedLayer then opacity * 0.28 else opacity
                                            )
                                        ]
                                        match terrain with
                                        | Rough ->
                                            Svg.line [
                                                svg.custom ("data-terrain-pattern", "diagonal-hatch")
                                                svg.custom ("display", editorLayerDisplay TerrainDomain state)
                                                svg.custom ("opacity", editorLayerOpacity TerrainDomain state)
                                                svg.x1 (float column * Battlefield.CellSize + 8.0)
                                                svg.y1 (float (row + 1) * Battlefield.CellSize - 8.0)
                                                svg.x2 (float (column + 1) * Battlefield.CellSize - 8.0)
                                                svg.y2 (float row * Battlefield.CellSize + 8.0)
                                                svg.stroke palette.Grid
                                                svg.strokeWidth 3
                                                svg.custom ("pointer-events", "none")
                                            ]
                                        | Blocked ->
                                            for first, last in [ 9.0, Battlefield.CellSize - 9.0; Battlefield.CellSize - 9.0, 9.0 ] do
                                                Svg.line [
                                                    svg.custom ("data-terrain-pattern", "cross-hatch")
                                                    svg.custom ("display", editorLayerDisplay TerrainDomain state)
                                                    svg.custom ("opacity", editorLayerOpacity TerrainDomain state)
                                                    svg.x1 (float column * Battlefield.CellSize + first)
                                                    svg.y1 (float row * Battlefield.CellSize + 9.0)
                                                    svg.x2 (float column * Battlefield.CellSize + last)
                                                    svg.y2 (float (row + 1) * Battlefield.CellSize - 9.0)
                                                    svg.stroke palette.HealthActive
                                                    svg.strokeWidth 3
                                                    svg.custom ("pointer-events", "none")
                                                ]
                                        | Objective ->
                                            Svg.rect [
                                                svg.custom ("data-terrain-pattern", "inset-ring")
                                                svg.custom ("display", editorLayerDisplay TerrainDomain state)
                                                svg.custom ("opacity", editorLayerOpacity TerrainDomain state)
                                                svg.x (float column * Battlefield.CellSize + 7.0)
                                                svg.y (float row * Battlefield.CellSize + 7.0)
                                                svg.width (Battlefield.CellSize - 14.0)
                                                svg.height (Battlefield.CellSize - 14.0)
                                                svg.fill "none"
                                                svg.stroke palette.NeutralFaction
                                                svg.strokeWidth 3
                                                svg.custom ("pointer-events", "none")
                                            ]
                                        | Open -> ()
                            match MapEditor.terrainPreview state with
                            | Some(terrain, addresses, isValid) ->
                                Svg.g [
                                    svg.custom ("data-layer", "terrain-preview")
                                    svg.custom ("display", editorLayerDisplay TerrainDomain state)
                                    svg.custom ("opacity", editorLayerOpacity TerrainDomain state)
                                    svg.custom ("data-preview-valid", string isValid)
                                    svg.custom ("pointer-events", "none")
                                    svg.children [
                                        for address in addresses do
                                            Svg.rect [
                                                svg.custom (
                                                    "data-preview-terrain",
                                                    MapEditor.terrainLabel terrain
                                                )
                                                svg.x (float address.CellColumn * Battlefield.CellSize + 3.0)
                                                svg.y (float address.CellRow * Battlefield.CellSize + 3.0)
                                                svg.width (Battlefield.CellSize - 6.0)
                                                svg.height (Battlefield.CellSize - 6.0)
                                                svg.fill "none"
                                                svg.stroke (
                                                    if isValid then palette.Focus
                                                    else palette.HealthActive
                                                )
                                                svg.strokeWidth 4
                                                svg.custom ("stroke-dasharray", if isValid then "8 4" else "3 3")
                                                svg.custom ("vector-effect", "non-scaling-stroke")
                                            ]
                                    ]
                                ]
                            | None -> ()
                            match MapEditor.unitPreview state with
                            | Some(units, isValid) ->
                                Svg.g [
                                    svg.custom ("data-layer", "unit-preview")
                                    svg.custom ("display", editorLayerDisplay UnitDomain state)
                                    svg.custom ("opacity", editorLayerOpacity UnitDomain state)
                                    svg.custom ("data-preview-valid", string isValid)
                                    svg.custom ("pointer-events", "none")
                                    svg.children [
                                        for unit in units do
                                            let size =
                                                float unit.Size
                                                * Battlefield.CellSize
                                            Svg.rect [
                                                svg.custom (
                                                    "data-preview-unit",
                                                    unit.ClassId
                                                )
                                                svg.x (
                                                    float unit.Column
                                                    * Battlefield.CellSize
                                                    + 3.0
                                                )
                                                svg.y (
                                                    float unit.Row
                                                    * Battlefield.CellSize
                                                    + 3.0
                                                )
                                                svg.width (size - 6.0)
                                                svg.height (size - 6.0)
                                                svg.fill "none"
                                                svg.stroke (
                                                    if isValid then palette.Focus
                                                    else palette.HealthActive
                                                )
                                                svg.strokeWidth 4
                                                svg.custom (
                                                    "stroke-dasharray",
                                                    if isValid then "8 4" else "3 3"
                                                )
                                                svg.custom ("vector-effect", "non-scaling-stroke")
                                            ]
                                    ]
                                ]
                            | None -> ()
                            match state.Tool with
                            | Terrain _ ->
                                Svg.rect [
                                    svg.custom ("data-terrain-cursor", "true")
                                    svg.x (float state.TerrainCursor.CellColumn * Battlefield.CellSize + 5.0)
                                    svg.y (float state.TerrainCursor.CellRow * Battlefield.CellSize + 5.0)
                                    svg.width (Battlefield.CellSize - 10.0)
                                    svg.height (Battlefield.CellSize - 10.0)
                                    svg.fill "none"
                                    svg.stroke palette.Focus
                                    svg.strokeWidth 2
                                    svg.custom ("vector-effect", "non-scaling-stroke")
                                    svg.custom ("pointer-events", "none")
                                ]
                            | Select ->
                                Svg.rect [
                                    svg.custom ("data-keyboard-map-cursor", "true")
                                    svg.x (float state.KeyboardCursor.Cell.CellColumn * Battlefield.CellSize + 5.0)
                                    svg.y (float state.KeyboardCursor.Cell.CellRow * Battlefield.CellSize + 5.0)
                                    svg.width (Battlefield.CellSize - 10.0)
                                    svg.height (Battlefield.CellSize - 10.0)
                                    svg.fill "none"
                                    svg.stroke palette.Focus
                                    svg.strokeWidth 2
                                    svg.custom ("stroke-dasharray", "5 3")
                                    svg.custom ("vector-effect", "non-scaling-stroke")
                                    svg.custom ("pointer-events", "none")
                                ]
                            | _ -> ()
                            Svg.g [
                                svg.custom ("data-layer", "regions")
                                svg.custom ("display", editorLayerDisplay RegionDomain state)
                                svg.custom ("opacity", editorLayerOpacity RegionDomain state)
                                svg.children [
                                    for _, region in state.Map.Regions |> Map.toList do
                                        let selected = state.SelectedRegion = Some region.Id
                                        let color =
                                            match region.Purpose with
                                            | ObjectiveRegion -> palette.NeutralFaction
                                            | DeploymentZone Blue -> palette.HumanFaction
                                            | DeploymentZone Red -> palette.ArcaneFaction
                                            | DeploymentZone NeutralSide -> palette.Text
                                        let common =
                                            [ svg.custom ("data-region-id", string region.Id)
                                              svg.custom ("data-region-purpose", MapEditor.regionPurposeLabel region.Purpose)
                                              svg.custom ("role", "button")
                                              svg.custom (
                                                  "aria-label",
                                                  "Select region "
                                                  + string region.Id
                                                  + ", "
                                                  + MapEditor.regionPurposeLabel region.Purpose
                                              )
                                              svg.tabIndex (if selected then 0 else -1)
                                              svg.fill color
                                              svg.custom ("fill-opacity", "0.18")
                                              svg.stroke (if selected then palette.Focus else color)
                                              svg.strokeWidth (if selected then 5 else 3)
                                              svg.custom ("vector-effect", "non-scaling-stroke")
                                              svg.onClick (fun event ->
                                                  event.stopPropagation ()
                                                  dispatch (EditorChanged(SelectEditorRegion(Some region.Id))))
                                              svg.onKeyDown (fun event ->
                                                  match event.key with
                                                  | "Enter"
                                                  | " " ->
                                                      event.preventDefault ()
                                                      dispatch (EditorChanged(SelectEditorRegion(Some region.Id)))
                                                  | "Escape" ->
                                                      event.preventDefault ()
                                                      dispatch (EditorChanged(SelectEditorRegion None))
                                                  | _ -> ()) ]
                                        match region.Geometry with
                                        | RegionRectangle(column, row, width, height) ->
                                            Svg.rect (
                                                common
                                                @ [ svg.x (float column * Battlefield.CellSize)
                                                    svg.y (float row * Battlefield.CellSize)
                                                    svg.width (float width * Battlefield.CellSize)
                                                    svg.height (float height * Battlefield.CellSize) ]
                                            )
                                        | RegionPolygon vertices ->
                                            let points =
                                                vertices
                                                |> Array.map (fun vertex ->
                                                    string (float vertex.CellColumn * Battlefield.CellSize)
                                                    + ","
                                                    + string (float vertex.CellRow * Battlefield.CellSize))
                                                |> String.concat " "
                                            Svg.polygon (common @ [ svg.points points ])
                                ]
                            ]
                            Svg.g [
                                svg.custom ("data-layer", "grid")
                                svg.custom ("display", editorLayerDisplay DocumentDomain state)
                                svg.custom ("opacity", editorLayerOpacity DocumentDomain state)
                                svg.custom ("pointer-events", "none")
                                svg.children [
                                    for column in 0 .. int state.Map.Width do
                                        Svg.line [
                                            svg.x1 (float column * Battlefield.CellSize)
                                            svg.y1 0
                                            svg.x2 (float column * Battlefield.CellSize)
                                            svg.y2 boardHeight
                                            svg.stroke palette.Grid
                                            svg.strokeWidth 1
                                            svg.custom ("vector-effect", "non-scaling-stroke")
                                        ]
                                    for row in 0 .. int state.Map.Height do
                                        Svg.line [
                                            svg.x1 0
                                            svg.y1 (float row * Battlefield.CellSize)
                                            svg.x2 boardWidth
                                            svg.y2 (float row * Battlefield.CellSize)
                                            svg.stroke palette.Grid
                                            svg.strokeWidth 1
                                            svg.custom ("vector-effect", "non-scaling-stroke")
                                        ]
                                ]
                            ]
                            Svg.g [
                                svg.custom ("data-layer", "edges")
                                svg.custom ("display", editorLayerDisplay EdgeDomain state)
                                svg.custom ("opacity", editorLayerOpacity EdgeDomain state)
                                svg.custom ("pointer-events", "none")
                                svg.children [
                                    for (column, row, direction), (kind, isOpen) in
                                        state.Map.Edges |> Map.toList do
                                        let x1, y1, x2, y2 =
                                            match direction with
                                            | EastEdge ->
                                                let x =
                                                    float (column + 1)
                                                    * Battlefield.CellSize
                                                x,
                                                float row * Battlefield.CellSize,
                                                x,
                                                float (row + 1) * Battlefield.CellSize
                                            | SouthEdge ->
                                                let y =
                                                    float (row + 1)
                                                    * Battlefield.CellSize
                                                float column * Battlefield.CellSize,
                                                y,
                                                float (column + 1) * Battlefield.CellSize,
                                                y
                                        let color, dash =
                                            match kind, isOpen with
                                            | Wall, _ -> palette.Text, "none"
                                            | Door, true -> palette.NeutralFaction, "8 5"
                                            | Door, false -> palette.NeutralFaction, "none"
                                            | Window, _ -> palette.HumanFaction, "3 3"
                                        Svg.line [
                                            svg.custom ("data-edge-kind", string kind)
                                            svg.custom (
                                                "data-edge-state",
                                                if isOpen then "open" else "closed"
                                            )
                                            svg.custom ("data-edge-direction", string direction)
                                            svg.x1 x1
                                            svg.y1 y1
                                            svg.x2 x2
                                            svg.y2 y2
                                            svg.stroke color
                                            svg.strokeWidth 5
                                            svg.custom ("stroke-dasharray", dash)
                                            svg.custom ("vector-effect", "non-scaling-stroke")
                                    ]
                                ]
                            ]
                            match state.Gesture with
                            | EdgePolylineGesture(kind, segments) ->
                                Svg.g [
                                    svg.custom ("data-layer", "edge-preview")
                                    svg.custom ("display", editorLayerDisplay EdgeDomain state)
                                    svg.custom ("opacity", editorLayerOpacity EdgeDomain state)
                                    svg.custom ("pointer-events", "none")
                                    svg.children [
                                        for column, row, direction in segments do
                                            let x1, y1, x2, y2 =
                                                match direction with
                                                | EastEdge ->
                                                    let x =
                                                        float (column + 1)
                                                        * Battlefield.CellSize
                                                    x,
                                                    float row * Battlefield.CellSize,
                                                    x,
                                                    float (row + 1) * Battlefield.CellSize
                                                | SouthEdge ->
                                                    let y =
                                                        float (row + 1)
                                                        * Battlefield.CellSize
                                                    float column * Battlefield.CellSize,
                                                    y,
                                                    float (column + 1) * Battlefield.CellSize,
                                                    y
                                            Svg.line [
                                                svg.custom ("data-edge-preview", string kind)
                                                svg.x1 x1
                                                svg.y1 y1
                                                svg.x2 x2
                                                svg.y2 y2
                                                svg.stroke palette.Focus
                                                svg.strokeWidth 7
                                                svg.custom ("stroke-dasharray", "7 4")
                                                svg.custom ("vector-effect", "non-scaling-stroke")
                                            ]
                                    ]
                                ]
                            | _ -> Html.none
                            Svg.g [
                                svg.key ("editor-units-" + state.Revision.Digest)
                                svg.custom ("data-layer", "units")
                                svg.custom ("display", editorLayerDisplay UnitDomain state)
                                svg.custom ("opacity", editorLayerOpacity UnitDomain state)
                                svg.children (
                                    state.Map.Units
                                    |> Map.toList
                                    |> List.map (fun (_, unit) ->
                                        editorUnitSvg state palette dispatch unit)
                                )
                            ]
                            match state.ActiveIssue with
                            | Some index when index >= 0 && index < state.Issues.Length ->
                                let issue = state.Issues[index]
                                Svg.g [
                                    svg.custom ("data-layer", "validation-overlay")
                                    svg.custom ("pointer-events", "none")
                                    svg.children [
                                        Svg.rect [
                                            svg.x 6
                                            svg.y 6
                                            svg.width (min 420.0 (boardWidth - 12.0))
                                            svg.height 38
                                            svg.rx 5
                                            svg.fill palette.Canvas
                                            svg.stroke palette.HealthActive
                                            svg.strokeWidth 2
                                            svg.custom ("vector-effect", "non-scaling-stroke")
                                        ]
                                        Svg.text [
                                            svg.x 18
                                            svg.y 31
                                            svg.fill palette.Text
                                            svg.fontSize 15
                                            svg.text (issue.Code + " · " + issue.Message)
                                        ]
                                    ]
                                ]
                            | _ -> Html.none
                            match state.Gesture with
                            | BoxSelectionGesture(anchor, current) ->
                                let firstColumn = min anchor.CellColumn current.CellColumn
                                let firstRow = min anchor.CellRow current.CellRow
                                let lastColumn = max anchor.CellColumn current.CellColumn
                                let lastRow = max anchor.CellRow current.CellRow
                                Svg.rect [
                                    svg.custom ("data-editor-gesture", "box-selection")
                                    svg.custom ("pointer-events", "none")
                                    svg.x (float firstColumn * Battlefield.CellSize)
                                    svg.y (float firstRow * Battlefield.CellSize)
                                    svg.width (float (lastColumn - firstColumn + 1) * Battlefield.CellSize)
                                    svg.height (float (lastRow - firstRow + 1) * Battlefield.CellSize)
                                    svg.fill "none"
                                    svg.stroke palette.Focus
                                    svg.strokeWidth 2
                                    svg.custom ("stroke-dasharray", "6 4")
                                    svg.custom ("vector-effect", "non-scaling-stroke")
                                ]
                            | _ -> Html.none
                        ]
                    ]
                ]
            ]
            Html.div [
                prop.className "editor-status-row"
                prop.role.status
                prop.ariaLive.polite
                prop.children [
                    Html.span ("Active: " + toolLabel state.Tool)
                    Html.span (
                        string (int (view.Camera.Zoom * 100.0))
                        + "% · "
                        + string state.SelectedUnits.Count
                        + " selected · "
                        + string state.Map.Units.Count
                        + " units · "
                        + string state.Map.Edges.Count
                        + " edges"
                    )
                    Html.span (
                        (match state.RevisionState with
                         | DirtyRevision -> "Dirty"
                         | SavedRevision -> "Saved"
                         | SimulatedRevision -> "Simulated"
                         | RecoveredRevision -> "Recovered")
                        + " revision "
                        + string state.Revision.Number
                        + " · "
                        + state.Revision.Digest.Substring(0, 12)
                    )
                ]
            ]
        ]
    ]

let private editorToolbar
    (state: MapEditorState)
    (view: EditorWorkspaceState)
    (activePanel: EditorToolPanel)
    panelVisible
    dispatch
    =
    let choose (label: string) (tool: MapEditorTool) =
        Html.button [
            prop.type'.button
            prop.text label
            prop.ariaPressed (state.Tool = tool)
            prop.onClick (fun _ -> dispatch (EditorChanged(ChooseTool tool)))
        ]

    let chooseTerrainValue shortcut terrain =
        let label =
            MapEditor.terrainLabel terrain
            + " · "
            + MapEditor.terrainPattern terrain
            + " · "
            + shortcut
        Html.button [
            prop.type'.button
            prop.className (
                "terrain-palette-choice terrain-"
                + MapEditor.terrainLabel terrain
            )
            prop.text label
            prop.ariaLabel (
                MapEditor.terrainLabel terrain
                + " terrain, "
                + MapEditor.terrainPattern terrain
                + ", shortcut "
                + shortcut
            )
            prop.ariaPressed (state.TerrainSelection = terrain)
            prop.onClick (fun _ ->
                dispatch (EditorChanged(ChooseTerrain terrain)))
        ]

    let placePreset presetId =
        MapEditor.tryCanonicalFootprintPreset presetId
        |> Option.map (fun preset ->
            Place(preset.Side, preset.ClassId, preset.FootprintSize))
        |> Option.defaultWith (fun () ->
            failwith ("Unknown canonical footprint preset: " + presetId))

    let choosePanel (label: string) (panel: EditorToolPanel) =
        Html.button [
            prop.type'.button
            prop.text label
            prop.ariaPressed (
                panelVisible && Object.Equals(activePanel, panel)
            )
            prop.onClick (fun _ ->
                dispatch (EditorToolPanelChanged panel))
        ]

    Html.section [
        prop.className (
            "panel editor-tools editor-ribbon"
            + if panelVisible then "" else " is-collapsed"
        )
        prop.ariaLabel "Map editing tools"
        prop.children [
            Html.div [
                prop.className "editor-section-heading"
                prop.children [
                    Html.div [
                        Html.p [ prop.className "eyebrow"; prop.text "Author" ]
                        Html.h2 "Map editor"
                    ]
                    Html.p [
                        prop.className "active-tool"
                        prop.text (
                            "Active tool: " + toolLabel state.Tool
                            + " · F2 tools · F3 inspector"
                        )
                    ]
                ]
            ]
            Html.nav [
                prop.className "editor-tool-navigation compact-tool-rail"
                prop.ariaLabel "Map editor tool groups"
                prop.children [
                    choosePanel "Terrain" TerrainTools
                    choosePanel "Units" UnitTools
                    choosePanel "Edges" EdgeTools
                    choosePanel "Zones" ZoneTools
                    choosePanel "Map file" DocumentTools
                    button
                        (if view.InspectorCollapsed then
                             "Show inspector · F3"
                         else
                             "Hide inspector · F3")
                        "Toggle selected-object inspector"
                        false
                        (fun _ ->
                            dispatch (
                                EditorWorkspaceChanged
                                    ToggleEditorInspector
                            ))
                ]
            ]
            if panelVisible then Html.div [
                prop.className "editor-tool-panel editor-context-palette"
                prop.children [
                    match activePanel with
                    | TerrainTools ->
                        Html.h3 "Terrain"
                        Html.div [
                            prop.className "control-row terrain-tool-choices"
                            prop.children [
                                choose "Select" Select
                                for tool in
                                    [ PencilTool
                                      RectangleTool
                                      LineTool
                                      FloodFillTool
                                      EyedropperTool
                                      EraseTool ] do
                                    choose
                                        (MapEditor.terrainToolLabel tool
                                         + " ("
                                         + MapEditor.terrainToolShortcut tool
                                         + ")")
                                        (Terrain tool)
                            ]
                        ]
                        Html.div [
                            prop.className "terrain-palette"
                            prop.role.group
                            prop.ariaLabel "Terrain palette"
                            prop.children [
                                chooseTerrainValue "Shift+1" Open
                                chooseTerrainValue "Shift+2" Rough
                                chooseTerrainValue "Shift+3" Blocked
                                chooseTerrainValue "Shift+4" Objective
                            ]
                        ]
                        Html.div [
                            prop.className "terrain-brush-controls"
                            prop.children [
                                Html.label [
                                    prop.htmlFor "terrain-brush-size"
                                    prop.text "Square brush size"
                                ]
                                Html.input [
                                    prop.id "terrain-brush-size"
                                    prop.type'.number
                                    prop.min 1
                                    prop.max 9
                                    prop.step 1
                                    prop.value state.BrushSize
                                    prop.onChange (fun (value: int) ->
                                        dispatch (
                                            EditorChanged(
                                                SetTerrainBrushSize(int32 value)
                                            )
                                        ))
                                ]
                                button
                                    "Apply preview"
                                    "Commit the terrain preview (Enter)"
                                    (match state.Gesture with
                                     | TerrainGesture _ -> false
                                     | _ -> true)
                                    (fun _ ->
                                        dispatch (EditorChanged CommitEditorGesture))
                                button
                                    "Cancel preview"
                                    "Cancel the terrain preview (Escape)"
                                    (match state.Gesture with
                                     | TerrainGesture _ -> false
                                     | _ -> true)
                                    (fun _ ->
                                        dispatch (EditorChanged CancelEditorGesture))
                            ]
                        ]
                    | UnitTools ->
                        Html.h3 "Unit presets"
                        Html.label [
                            prop.htmlFor "editor-unit-preset-search"
                            prop.text "Search faction, role, class, or glyph"
                        ]
                        Html.input [
                            prop.id "editor-unit-preset-search"
                            prop.type'.search
                            prop.value state.UnitPaletteSearch
                            prop.onChange (fun value ->
                                dispatch (EditorChanged(SetUnitPaletteSearch value)))
                            prop.onKeyDown (fun event ->
                                match event.key with
                                | "Enter" ->
                                    event.preventDefault ()
                                    if not event.repeat then
                                        dispatch (EditorChanged ArmUnitPalettePreset)
                                        document.getElementById("editor-map-stage").focus ()
                                | "Escape" ->
                                    event.preventDefault ()
                                    document.getElementById("editor-map-stage").focus ()
                                | _ -> ())
                        ]
                        Html.div [
                            prop.className "unit-preset-groups"
                            prop.children [
                                for faction, presets in
                                    MapEditor.searchCanonicalUnitPresets state.UnitPaletteSearch
                                    |> List.groupBy _.Faction do
                                    Html.section [
                                        prop.className "unit-preset-group"
                                        prop.ariaLabel (faction + " unit presets")
                                        prop.children [
                                            Html.h4 faction
                                            for preset in presets do
                                                choose
                                                    (preset.Name
                                                     + " · "
                                                     + preset.Role
                                                     + " · "
                                                     + string preset.FootprintSize
                                                     + "×"
                                                     + string preset.FootprintSize
                                                     + " · "
                                                     + string preset.HealthMaximum
                                                     + " HP · "
                                                     + preset.GlyphId
                                                     + " glyph")
                                                    (placePreset preset.Id)
                                        ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "control-row"
                            prop.role.group
                            prop.ariaLabel "Unit preview actions"
                            prop.children [
                                button
                                    "Apply preview"
                                    "Commit the unit placement, paste, or movement preview (Enter)"
                                    (match state.Gesture with
                                     | CommandPreviewGesture _
                                     | UnitMoveGesture _ -> false
                                     | _ -> true)
                                    (fun _ -> dispatch (EditorChanged CommitEditorGesture))
                                button
                                    "Reset move"
                                    "Reset selected units to their original preview positions (Backspace)"
                                    (match state.Gesture with
                                     | UnitMoveGesture _ -> false
                                     | _ -> true)
                                    (fun _ -> dispatch (EditorChanged ResetUnitMovePreview))
                                button
                                    "Cancel preview"
                                    "Cancel the unit preview (Escape)"
                                    (match state.Gesture with
                                     | CommandPreviewGesture _
                                     | UnitMoveGesture _ -> false
                                     | _ -> true)
                                    (fun _ -> dispatch (EditorChanged CancelEditorGesture))
                                button
                                    "Browse presets"
                                    "Return to unit preset browsing (B)"
                                    (state.Tool = UnitBrowse)
                                    (fun _ -> dispatch (EditorChanged ReturnToUnitBrowse))
                            ]
                        ]
                    | EdgeTools ->
                        let edgeColumn, edgeRow, edgeDirection = state.EdgeCursor
                        Html.h3 "Semantic edges"
                        Html.div [
                            prop.className "control-row"
                            prop.children [
                                choose "East wall" (Edge(EastEdge, Wall))
                                choose "South wall" (Edge(SouthEdge, Wall))
                                choose "East door" (Edge(EastEdge, Door))
                                choose "South door" (Edge(SouthEdge, Door))
                                choose "East window" (Edge(EastEdge, Window))
                                choose "South window" (Edge(SouthEdge, Window))
                            ]
                        ]
                        Html.div [
                            prop.className "control-row"
                            prop.role.group
                            prop.ariaLabel "Edge actions at the keyboard cursor"
                            prop.children [
                                button "Finish" "Finish wall polyline (Enter)" false (fun _ ->
                                    dispatch (EditorChanged FinishEdgePolyline))
                                button "Back" "Remove last polyline segment (Escape)" false (fun _ ->
                                    dispatch (EditorChanged BacktrackEdgePolyline))
                                button "Wall" "Convert cursor edge to wall" false (fun _ ->
                                    dispatch (
                                        EditorChanged(
                                            ConvertEdge(edgeColumn, edgeRow, edgeDirection, Wall)
                                        )
                                    ))
                                button "Door" "Convert cursor edge to a closed door" false (fun _ ->
                                    dispatch (
                                        EditorChanged(
                                            ConvertEdge(edgeColumn, edgeRow, edgeDirection, Door)
                                        )
                                    ))
                                button "Window" "Convert cursor edge to a window" false (fun _ ->
                                    dispatch (
                                        EditorChanged(
                                            ConvertEdge(edgeColumn, edgeRow, edgeDirection, Window)
                                        )
                                    ))
                                button "Open/close" "Toggle door open or closed" false (fun _ ->
                                    dispatch (
                                        EditorChanged(
                                            ToggleDoorState(edgeColumn, edgeRow, edgeDirection)
                                        )
                                    ))
                                button "Erase" "Erase cursor edge" false (fun _ ->
                                    dispatch (
                                        EditorChanged(
                                            EraseEdge(edgeColumn, edgeRow, edgeDirection)
                                        )
                                    ))
                                button "Split" "Split an edge run at the cursor" false (fun _ ->
                                    dispatch (
                                        EditorChanged(
                                            SplitEdge(edgeColumn, edgeRow, edgeDirection)
                                        )
                                    ))
                                button "Join" "Join an edge run at the cursor" false (fun _ ->
                                    dispatch (
                                        EditorChanged(
                                            JoinEdge(edgeColumn, edgeRow, edgeDirection)
                                        )
                                    ))
                            ]
                        ]
                    | ZoneTools ->
                        let cursor =
                            { CellColumn =
                                min (state.Map.Width - 2) state.TerrainCursor.CellColumn
                              CellRow =
                                min (state.Map.Height - 2) state.TerrainCursor.CellRow }
                        let rectangle purpose =
                            CreateRectangleRegion(
                                purpose,
                                cursor,
                                { CellColumn = cursor.CellColumn + 1
                                  CellRow = cursor.CellRow + 1 }
                            )
                        let polygon purpose =
                            CreatePolygonRegion(
                                purpose,
                                [| cursor
                                   { CellColumn = cursor.CellColumn + 2
                                     CellRow = cursor.CellRow }
                                   { CellColumn = cursor.CellColumn + 1
                                     CellRow = cursor.CellRow + 2 } |]
                            )
                        Html.h3 "Zones and objectives"
                        Html.p "Create authoritative geometry at the keyboard terrain cursor, then select and edit it below."
                        Html.div [
                            prop.className "control-row"
                            prop.role.group
                            prop.ariaLabel "Create authoritative map region"
                            prop.children [
                                button "Objective rectangle" "Create a two by two objective rectangle" false (fun _ ->
                                    dispatch (EditorChanged(rectangle ObjectiveRegion)))
                                button "Objective polygon" "Create a triangular objective polygon" false (fun _ ->
                                    dispatch (EditorChanged(polygon ObjectiveRegion)))
                                button "Blue deployment" "Create a blue deployment rectangle" false (fun _ ->
                                    dispatch (EditorChanged(rectangle (DeploymentZone Blue))))
                                button "Red deployment" "Create a red deployment rectangle" false (fun _ ->
                                    dispatch (EditorChanged(rectangle (DeploymentZone Red))))
                                button "Blue deployment polygon" "Create a triangular blue deployment zone" false (fun _ ->
                                    dispatch (EditorChanged(polygon (DeploymentZone Blue))))
                                button "Red deployment polygon" "Create a triangular red deployment zone" false (fun _ ->
                                    dispatch (EditorChanged(polygon (DeploymentZone Red))))
                            ]
                        ]
                        match state.SelectedRegion |> Option.bind (fun id -> Map.tryFind id state.Map.Regions) with
                        | None -> Html.p "Select a region in the map or region list to edit it."
                        | Some region ->
                            Html.h4 ("Region " + string region.Id)
                            Html.div [
                                prop.className "control-row"
                                prop.role.group
                                prop.ariaLabel "Selected region purpose"
                                prop.children [
                                    button "Objective" "Set purpose to objective" false (fun _ ->
                                        dispatch (EditorChanged(SetSelectedRegionPurpose ObjectiveRegion)))
                                    button "Blue deploy" "Set purpose to blue deployment" false (fun _ ->
                                        dispatch (EditorChanged(SetSelectedRegionPurpose(DeploymentZone Blue))))
                                    button "Red deploy" "Set purpose to red deployment" false (fun _ ->
                                        dispatch (EditorChanged(SetSelectedRegionPurpose(DeploymentZone Red))))
                                ]
                            ]
                            Html.div [
                                prop.className "control-row"
                                prop.role.group
                                prop.ariaLabel "Move selected region one grid coordinate"
                                prop.children [
                                    button "↑" "Move selected region up" false (fun _ ->
                                        dispatch (EditorChanged(MoveSelectedRegion(0, -1))))
                                    button "←" "Move selected region left" false (fun _ ->
                                        dispatch (EditorChanged(MoveSelectedRegion(-1, 0))))
                                    button "→" "Move selected region right" false (fun _ ->
                                        dispatch (EditorChanged(MoveSelectedRegion(1, 0))))
                                    button "↓" "Move selected region down" false (fun _ ->
                                        dispatch (EditorChanged(MoveSelectedRegion(0, 1))))
                                    button "Delete" "Delete selected region" false (fun _ ->
                                        dispatch (EditorChanged RemoveSelectedRegion))
                                ]
                            ]
                            match region.Geometry with
                            | RegionPolygon vertices ->
                                Html.div [
                                    prop.className "control-row"
                                    prop.role.group
                                    prop.ariaLabel "Move polygon vertices"
                                    prop.children [
                                        for index in 0 .. vertices.Length - 1 do
                                            for label, columnDelta, rowDelta in
                                                [ "up", 0, -1
                                                  "left", -1, 0
                                                  "right", 1, 0
                                                  "down", 0, 1 ] do
                                                button
                                                    ("Vertex " + string (index + 1) + " " + label)
                                                    ("Move polygon vertex " + string (index + 1) + " " + label)
                                                    false
                                                    (fun _ ->
                                                        dispatch (
                                                            EditorChanged(
                                                                MoveSelectedRegionVertex(
                                                                    index,
                                                                    int32 columnDelta,
                                                                    int32 rowDelta
                                                                )
                                                            )
                                                        ))
                                    ]
                                ]
                            | RegionRectangle(column, row, width, height) ->
                                Html.div [
                                    prop.className "control-row"
                                    prop.role.group
                                    prop.ariaLabel "Resize selected region rectangle"
                                    prop.children [
                                        button "Wider" "Increase rectangle width by one cell" false (fun _ ->
                                            dispatch (
                                                EditorChanged(
                                                    SetSelectedRegionGeometry(
                                                        RegionRectangle(column, row, width + 1, height)
                                                    )
                                                )
                                            ))
                                        button "Narrower" "Decrease rectangle width by one cell" (width <= 1) (fun _ ->
                                            dispatch (
                                                EditorChanged(
                                                    SetSelectedRegionGeometry(
                                                        RegionRectangle(column, row, width - 1, height)
                                                    )
                                                )
                                            ))
                                        button "Taller" "Increase rectangle height by one cell" false (fun _ ->
                                            dispatch (
                                                EditorChanged(
                                                    SetSelectedRegionGeometry(
                                                        RegionRectangle(column, row, width, height + 1)
                                                    )
                                                )
                                            ))
                                        button "Shorter" "Decrease rectangle height by one cell" (height <= 1) (fun _ ->
                                            dispatch (
                                                EditorChanged(
                                                    SetSelectedRegionGeometry(
                                                        RegionRectangle(column, row, width, height - 1)
                                                    )
                                                )
                                            ))
                                    ]
                                ]
                    | DocumentTools ->
                        Html.h3 "Map document"
                        Html.fieldSet [
                            Html.legend "Local raster background (presentation only)"
                            Html.p [
                                prop.role.status
                                prop.ariaLive.polite
                                prop.text view.BackgroundAnnouncement
                            ]
                            Html.label [
                                prop.children [
                                    Html.span "Choose local PNG, JPEG, or WebP"
                                    Html.input [
                                        prop.id "editor-background-file"
                                        prop.type'.file
                                        prop.accept "image/png,image/jpeg,image/webp,.png,.jpg,.jpeg,.webp"
                                        prop.ariaLabel "Choose local raster map background"
                                        prop.onChange (fun (files: File list) ->
                                            files
                                            |> List.tryHead
                                            |> Option.iter (BackgroundFileSelected >> dispatch))
                                    ]
                                ]
                            ]
                            match view.Background with
                            | None ->
                                Html.p "No raster background. Remote URLs, SVG, and executable content are never fetched or accepted."
                            | Some background ->
                                Html.p (
                                    background.FileName + " · "
                                    + string background.PixelWidth + "×" + string background.PixelHeight
                                    + " · " + string background.ByteLength + " bytes · "
                                    + background.AssetId.Substring(0, 19)
                                )
                                Html.div [
                                    prop.className "control-row"
                                    prop.role.group
                                    prop.ariaLabel "Background lock and fit"
                                    prop.children [
                                        button
                                            (if background.Locked then "Unlock" else "Lock")
                                            "Toggle local raster background lock"
                                            false
                                            (fun _ -> dispatch (EditorWorkspaceChanged ToggleBackgroundLock))
                                        for fit, label in
                                            [ FitInside, "Fit"
                                              FillAndCrop, "Fill/crop"
                                              StretchToBoard, "Stretch"
                                              NativePixels, "Grid scale" ] do
                                            Html.button [
                                                prop.type'.button
                                                prop.text label
                                                prop.ariaPressed (background.Fit = fit)
                                                prop.disabled background.Locked
                                                prop.onClick (fun _ ->
                                                    dispatch (EditorWorkspaceChanged(SetBackgroundFit fit)))
                                            ]
                                        button "Remove" "Remove local raster background" false (fun _ ->
                                            dispatch (EditorWorkspaceChanged RemoveLocalRaster))
                                    ]
                                ]
                                Html.label [
                                    prop.htmlFor "background-opacity"
                                    prop.text ("Opacity " + string (int (background.Opacity * 100.0)) + "%")
                                ]
                                Html.input [
                                    prop.id "background-opacity"
                                    prop.type'.range
                                    prop.min 0
                                    prop.max 100
                                    prop.value (int (background.Opacity * 100.0))
                                    prop.onChange (fun (value: int) ->
                                        dispatch (EditorWorkspaceChanged(SetBackgroundOpacity(float value / 100.0))))
                                ]
                                Html.div [
                                    prop.className "control-row"
                                    prop.role.group
                                    prop.ariaLabel "Background grid offset"
                                    prop.children [
                                        button "↑" "Nudge unlocked background up one board pixel" background.Locked (fun _ ->
                                            dispatch (EditorWorkspaceChanged(NudgeBackgroundGridOffset(0.0, -1.0))))
                                        button "←" "Nudge unlocked background left one board pixel" background.Locked (fun _ ->
                                            dispatch (EditorWorkspaceChanged(NudgeBackgroundGridOffset(-1.0, 0.0))))
                                        button "→" "Nudge unlocked background right one board pixel" background.Locked (fun _ ->
                                            dispatch (EditorWorkspaceChanged(NudgeBackgroundGridOffset(1.0, 0.0))))
                                        button "↓" "Nudge unlocked background down one board pixel" background.Locked (fun _ ->
                                            dispatch (EditorWorkspaceChanged(NudgeBackgroundGridOffset(0.0, 1.0))))
                                        button "Reset offset" "Reset background grid offset" background.Locked (fun _ ->
                                            dispatch (EditorWorkspaceChanged(SetBackgroundGridOffset(0.0, 0.0))))
                                    ]
                                ]
                                Html.label [
                                    prop.htmlFor "background-pixels-per-cell"
                                    prop.text "Source pixels per grid cell"
                                ]
                                Html.input [
                                    prop.id "background-pixels-per-cell"
                                    prop.type'.number
                                    prop.min 1
                                    prop.max MapEditorWorkspace.MaximumBackgroundDimension
                                    prop.value background.PixelsPerCell
                                    prop.disabled background.Locked
                                    prop.onChange (fun (value: float) ->
                                        dispatch (EditorWorkspaceChanged(SetBackgroundPixelsPerCell value)))
                                ]
                                Html.div [
                                    prop.className "control-row"
                                    prop.children [
                                        button "Use full image" "Clear background crop" background.Locked (fun _ ->
                                            dispatch (EditorWorkspaceChanged(SetBackgroundCrop None)))
                                        button "Crop 10%" "Inset crop by ten percent on every side" background.Locked (fun _ ->
                                            let left = background.PixelWidth / 10
                                            let top = background.PixelHeight / 10
                                            dispatch (
                                                EditorWorkspaceChanged(
                                                    SetBackgroundCrop(
                                                        Some
                                                            { Left = left
                                                              Top = top
                                                              Width = background.PixelWidth - left * 2
                                                              Height = background.PixelHeight - top * 2 }
                                                    )
                                                )
                                            ))
                                        button "Align first cell" "Align a source grid using zero and one source-cell markers" background.Locked (fun _ ->
                                            dispatch (
                                                EditorWorkspaceChanged(
                                                    AlignBackgroundGrid(
                                                        0.0,
                                                        0.0,
                                                        background.PixelsPerCell,
                                                        0.0,
                                                        1
                                                    )
                                                )
                                            ))
                                    ]
                                ]
                        ]
                        Html.label [
                            prop.htmlFor "map-name"
                            prop.text "Map name"
                        ]
                        Html.input [
                            prop.id "map-name"
                            prop.value state.Authoring.Name
                            prop.onChange (fun value ->
                                dispatch (EditorChanged(SetMapName value)))
                        ]
                        Html.div [
                            prop.id "editor-layer-controls"
                            prop.tabIndex 0
                            prop.className "editor-layer-controls"
                            prop.role.group
                            prop.ariaLabel "Editing layer states"
                            prop.children [
                                for domain in
                                    [ TerrainDomain
                                      EdgeDomain
                                      UnitDomain
                                      RegionDomain
                                      DocumentDomain ] do
                                    Html.fieldSet [
                                        Html.legend (string domain)
                                        for value, label in
                                            [ VisibleLayer, "Visible"
                                              DimmedLayer, "Dimmed"
                                              HiddenLayer, "Hidden"
                                              LockedLayer, "Locked" ] do
                                            Html.button [
                                                prop.type'.button
                                                prop.text label
                                                prop.ariaPressed (
                                                    MapEditor.layerState domain state = value
                                                )
                                                prop.onClick (fun _ ->
                                                    dispatch (
                                                        EditorChanged(
                                                            SetEditorLayerState(domain, value)
                                                        )
                                                    ))
                                            ]
                                    ]
                            ]
                        ]
                        Html.div [
                            prop.id "editor-saved-view-controls"
                            prop.tabIndex 0
                            prop.className "control-row"
                            prop.children [
                                button "Save current view" "Save the current camera as a named authoring view" false (fun _ ->
                                    dispatch (
                                        EditorChanged(
                                            SaveMapView(
                                                "View " + string (state.Authoring.SavedViews.Count + 1),
                                                view.Camera
                                            )
                                        )
                                    ))
                                button "Generate thumbnail" "Generate deterministic authoring thumbnail metadata" false (fun _ ->
                                    dispatch (
                                        EditorChanged(
                                            SetMapThumbnail(Some(MapEditor.thumbnailSvg state))
                                        )
                                    ))
                                Html.span (
                                    string state.Authoring.SavedViews.Count
                                    + " saved views · thumbnail "
                                    + if state.Authoring.ThumbnailSvg.IsSome then "ready" else "not generated"
                                )
                            ]
                        ]
                        if not state.Authoring.SavedViews.IsEmpty then
                            Html.ul [
                                prop.ariaLabel "Saved map views"
                                prop.children [
                                    for name, saved in state.Authoring.SavedViews |> Map.toList do
                                        Html.li [
                                            Html.button [
                                                prop.type'.button
                                                prop.text ("Recall " + saved.Name)
                                                prop.onClick (fun _ ->
                                                    dispatch (RecallEditorView name))
                                            ]
                                            Html.button [
                                                prop.type'.button
                                                prop.text ("Remove " + saved.Name)
                                                prop.onClick (fun _ ->
                                                    dispatch (
                                                        EditorChanged(RemoveMapView name)
                                                    ))
                                            ]
                                        ]
                                ]
                            ]
                        Html.div [
                            prop.className "map-size-row"
                            prop.children [
                                Html.label [ prop.htmlFor "map-width"; prop.text "Width" ]
                                Html.input [
                                    prop.id "map-width"
                                    prop.type'.number
                                    prop.min 4
                                    prop.max 40
                                    prop.value state.Map.Width
                                    prop.onChange (fun (value: int) ->
                                        dispatch (EditorChanged(Resize(int32 value, state.Map.Height))))
                                ]
                                Html.label [ prop.htmlFor "map-height"; prop.text "Height" ]
                                Html.input [
                                    prop.id "map-height"
                                    prop.type'.number
                                    prop.min 4
                                    prop.max 40
                                    prop.value state.Map.Height
                                    prop.onChange (fun (value: int) ->
                                        dispatch (EditorChanged(Resize(state.Map.Width, int32 value))))
                                ]
                                button "Clear" "Clear the current map" false (fun _ ->
                                    dispatch (EditorChanged RequestClearMap))
                                button "Export map" "Export the current map document" false (fun _ ->
                                    dispatch ExportMap)
                                button
                                    "Repository bundle"
                                    "Download editor and simulator design work for version-controlled import"
                                    false
                                    (fun _ -> dispatch ExportDesignBundle)
                                Html.label [
                                    prop.className "map-import"
                                    prop.children [
                                        Html.span "Import map"
                                        Html.input [
                                            prop.id "editor-map-import"
                                            prop.type'.file
                                            prop.accept ".sir-map,.dd2vtt,.uvtt,.json,.xml,text/plain,application/json"
                                            prop.ariaLabel "Import SIR map"
                                            prop.title "Import SIR-MAP directly or review a Universal VTT, Foundry, or Fantasy Grounds export"
                                            prop.onChange (fun (files: File list) ->
                                                files
                                                |> List.tryHead
                                                |> Option.iter (MapFileSelected >> dispatch))
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.p [
                            prop.className "repository-transfer-help"
                            prop.children [
                                Html.strong "Version-control transfer: "
                                Html.span "download the repository bundle, then from this checkout run "
                                Html.code "npm run import:map-design -- ~/Downloads/<name>.sir-design.json"
                                Html.span ". Review the resulting designs/map-editor files and commit them through the normal pull-request workflow."
                            ]
                        ]
                        match state.PendingRecovery with
                        | Some draft ->
                            Html.div [
                                prop.className "editor-confirmation"
                                prop.role.alert
                                prop.children [
                                    Html.strong "Crash-recovery draft found"
                                    Html.span ("Draft revision " + draft.SourceDigest.Substring(0, 12))
                                    button "Recover draft" "Replace the current map with the local recovery draft" false (fun _ ->
                                        dispatch (EditorChanged RecoverCrashDraft))
                                    button "Discard draft" "Keep the current map and discard the recovery choice" false (fun _ ->
                                        dispatch (EditorChanged DiscardCrashDraft))
                                ]
                            ]
                        | None -> Html.none
                        Html.section [
                            prop.className "editor-issues-panel"
                            prop.ariaLabel "Map validation issues"
                            prop.children [
                                Html.h4 ("Issues · " + string state.Issues.Length)
                                button "Previous" "Previous validation issue" (Array.isEmpty state.Issues) (fun _ ->
                                    dispatch (EditorChanged SelectPreviousIssue))
                                button "Next" "Next validation issue" (Array.isEmpty state.Issues) (fun _ ->
                                    dispatch (EditorChanged SelectNextIssue))
                                match state.ActiveIssue with
                                | Some index when index >= 0 && index < state.Issues.Length ->
                                    let issue = state.Issues[index]
                                    Html.p [
                                        prop.role.status
                                        prop.ariaLive.polite
                                        prop.text (
                                            string (index + 1) + " of " + string state.Issues.Length
                                            + " · " + issue.Code + " · " + issue.Message
                                        )
                                    ]
                                | _ -> Html.p "No validation issues."
                            ]
                        ]
                ]
            ] else Html.none
        ]
    ]

let private editorGrid state dispatch =
    Html.section [
        prop.className "panel editor-object-list"
        prop.ariaLabel "Map object list fallback"
        prop.children [
            Html.details [
                Html.summary (
                    "Keyboard and screen-reader object list · "
                    + string state.Map.Width
                    + " × "
                    + string state.Map.Height
                    + " cells"
                )
                Html.p [
                    prop.className "keyboard-help"
                    prop.text "Use Tab to reach objects and Enter to activate the current tool. Empty cells remain available here without adding hundreds of SVG tab stops."
                ]
                Html.h3 "Units"
                if state.Map.Units.IsEmpty then
                    Html.p "No units."
                else
                    Html.ul [
                        for _, unit in state.Map.Units |> Map.toList do
                            Html.li [
                                Html.button [
                                    prop.type'.button
                                    prop.ariaPressed (Set.contains unit.Id state.SelectedUnits)
                                    prop.text (
                                        "Unit "
                                        + string unit.Id
                                        + " · "
                                        + unit.ClassId
                                        + " · "
                                        + string unit.Size
                                        + "×"
                                        + string unit.Size
                                        + " at "
                                        + string unit.Column
                                        + ","
                                        + string unit.Row
                                    )
                                    prop.onClick (fun event ->
                                        dispatch (
                                            EditorChanged(
                                                if event.shiftKey then
                                                    ToggleEditorUnitSelection unit.Id
                                                else
                                                    SelectEditorUnit(Some unit.Id)
                                            )
                                        ))
                                    prop.onKeyDown (fun event ->
                                        let move amount =
                                            event.preventDefault ()
                                            focusEditorObjectList event.currentTarget amount
                                        match event.key with
                                        | "ArrowUp" -> move -1
                                        | "ArrowDown" -> move 1
                                        | "Home" -> move -2
                                        | "End" -> move 2
                                        | "Enter" ->
                                            event.preventDefault ()
                                            dispatch (
                                                EditorChanged(
                                                    if event.shiftKey then
                                                        ToggleEditorUnitSelection unit.Id
                                                    else
                                                        SelectEditorUnit(Some unit.Id)
                                                )
                                            )
                                        | _ -> ())
                                ]
                            ]
                    ]
                Html.h3 "Regions"
                if state.Map.Regions.IsEmpty then
                    Html.p "No regions."
                else
                    Html.ul [
                        prop.ariaLabel "Authoritative map regions"
                        prop.children [
                            for _, region in state.Map.Regions |> Map.toList do
                                Html.li [
                                    Html.button [
                                        prop.type'.button
                                        prop.ariaPressed (state.SelectedRegion = Some region.Id)
                                        prop.text (
                                            "Region "
                                            + string region.Id
                                            + " · "
                                            + MapEditor.regionPurposeLabel region.Purpose
                                            + " · "
                                            + (match region.Geometry with
                                               | RegionRectangle(column, row, width, height) ->
                                                   "rectangle "
                                                   + string width + "×" + string height
                                                   + " at " + string column + "," + string row
                                               | RegionPolygon vertices ->
                                                   "polygon with " + string vertices.Length + " vertices")
                                        )
                                        prop.onClick (fun _ ->
                                            dispatch (EditorChanged(SelectEditorRegion(Some region.Id))))
                                        prop.onKeyDown (fun event ->
                                            let move amount =
                                                event.preventDefault ()
                                                focusEditorObjectList event.currentTarget amount
                                            match event.key with
                                            | "ArrowUp" -> move -1
                                            | "ArrowDown" -> move 1
                                            | "Home" -> move -2
                                            | "End" -> move 2
                                            | "Enter" ->
                                                event.preventDefault ()
                                                dispatch (EditorChanged(SelectEditorRegion(Some region.Id)))
                                            | _ -> ())
                                    ]
                                ]
                        ]
                    ]
                Html.h3 "Cells"
                Html.div [
                    prop.className "editor-cell-list"
                    prop.children [
                        for row in 0 .. int state.Map.Height - 1 do
                            for column in 0 .. int state.Map.Width - 1 do
                                mapCell state dispatch (int32 column) (int32 row)
                    ]
                ]
            ]
        ]
    ]

let private editorUnitPanel state dispatch =
    let selectedUnits =
        state.SelectedUnits
        |> Set.toList
        |> List.choose (fun id -> Map.tryFind id state.Map.Units)
    let fieldLabel label projection =
        let values = selectedUnits |> List.map projection |> List.distinct
        label + if values.Length > 1 then " — Multiple" else ""

    Html.section [
        prop.className "panel unit-editor-panel"
        prop.ariaLabel "Selected unit properties"
        prop.children [
            Html.p [ prop.className "eyebrow"; prop.text "Unit" ]
            Html.h2 "Selected unit"
            match MapEditor.selected state with
            | None ->
                Html.p "Select a canonical unit symbol on the map."
            | Some unit ->
                Html.h3 ("Unit " + string unit.Id + " · " + unit.ClassId)
                if state.SelectedUnits.Count > 1 then
                    Html.p (
                        string state.SelectedUnits.Count
                        + " units selected. Inspector edits apply to the complete compatible selection; differing values are Multiple."
                    )
                Html.div [
                    prop.className "unit-properties"
                    prop.children [
                        Html.label [
                            prop.htmlFor "editor-unit-side"
                            prop.text (fieldLabel "Side" _.Side)
                        ]
                        Html.select [
                            prop.id "editor-unit-side"
                            prop.value (
                                match unit.Side with
                                | Blue -> "Blue"
                                | Red -> "Red"
                                | NeutralSide -> "Neutral"
                            )
                            prop.onChange (fun value ->
                                let side =
                                    match value with
                                    | "Red" -> Red
                                    | "Neutral" -> NeutralSide
                                    | _ -> Blue
                                dispatch (EditorChanged(SetSelectedSide side)))
                            prop.children [
                                Html.option [ prop.value "Blue"; prop.text "Blue" ]
                                Html.option [ prop.value "Red"; prop.text "Red" ]
                                Html.option [ prop.value "Neutral"; prop.text "Neutral" ]
                            ]
                        ]
                        Html.label [
                            prop.htmlFor "editor-unit-class"
                            prop.text (fieldLabel "Class ID" _.ClassId)
                        ]
                        Html.input [
                            prop.id "editor-unit-class"
                            prop.type'.text
                            prop.value unit.ClassId
                            prop.onChange (fun value ->
                                dispatch (EditorChanged(SetSelectedClass value)))
                        ]
                        Html.label [
                            prop.htmlFor "editor-unit-size"
                            prop.text (fieldLabel "Square size" _.Size)
                        ]
                        Html.input [
                            prop.id "editor-unit-size"
                            prop.type'.number
                            prop.min 1
                            prop.max 8
                            prop.value unit.Size
                            prop.onChange (fun (value: int) ->
                                dispatch (EditorChanged(SetSelectedSize(int32 value))))
                        ]
                        Html.label [
                            prop.htmlFor "editor-unit-health"
                            prop.text (fieldLabel "Current HP" _.Health)
                        ]
                        Html.input [
                            prop.id "editor-unit-health"
                            prop.type'.number
                            prop.min 0
                            prop.max unit.HealthMaximum
                            prop.value unit.Health
                            prop.onChange (fun (value: int) ->
                                dispatch (
                                    EditorChanged(
                                        SetSelectedHealth(int32 value, unit.HealthMaximum)
                                    )
                                ))
                        ]
                        Html.label [
                            prop.htmlFor "editor-unit-health-max"
                            prop.text (fieldLabel "Maximum HP" _.HealthMaximum)
                        ]
                        Html.input [
                            prop.id "editor-unit-health-max"
                            prop.type'.number
                            prop.min 1
                            prop.value unit.HealthMaximum
                            prop.onChange (fun (value: int) ->
                                dispatch (
                                    EditorChanged(
                                        SetSelectedHealth(unit.Health, int32 value)
                                    )
                                ))
                        ]
                        Html.label [
                            prop.htmlFor "editor-unit-controller"
                            prop.text (fieldLabel "Controller" _.Controller)
                        ]
                        Html.select [
                            prop.id "editor-unit-controller"
                            prop.value (MapEditor.controllerLabel unit.Controller)
                            prop.onChange (fun value ->
                                let controller =
                                    match value with
                                    | "Scripted AI" -> Scripted
                                    | "General AI" -> General
                                    | _ -> Manual
                                dispatch (EditorChanged(SetSelectedController controller)))
                            prop.children [
                                Html.option [ prop.value "Manual"; prop.text "Manual" ]
                                Html.option [ prop.value "Scripted AI"; prop.text "Scripted AI" ]
                                Html.option [ prop.value "General AI"; prop.text "General AI" ]
                            ]
                        ]
                        Html.label [
                            prop.htmlFor "editor-unit-script"
                            prop.text (
                                fieldLabel
                                    "Direction script"
                                    (fun selected -> MapEditor.scriptText selected.Script)
                            )
                        ]
                        Html.input [
                            prop.id "editor-unit-script"
                            prop.type'.text
                            prop.value (MapEditor.scriptText unit.Script)
                            prop.placeholder "N,E,E,S"
                            prop.onChange (fun value ->
                                dispatch (EditorChanged(SetSelectedScript value)))
                        ]
                    ]
                ]
                button "Remove unit" "Remove selected unit" false (fun _ ->
                    dispatch (EditorChanged RemoveSelectedUnit))
            state.Validation
            |> Option.map (fun error ->
                Html.p [
                    prop.className "validation-error"
                    prop.role.alert
                    prop.text error
                ])
            |> Option.defaultValue Html.none
        ]
    ]

let private controllerPanel (handoff: SimulatorHandoff) state dispatch =
    let selected = MapEditor.selected state
    let movement =
        [ "NW", NorthWest; "N", North; "NE", NorthEast
          "W", West; "E", East
          "SW", SouthWest; "S", South; "SE", SouthEast ]

    Html.section [
        prop.className "panel controller-panel"
        prop.ariaLabel "Simulation controllers"
        prop.children [
            Html.p [ prop.className "eyebrow"; prop.text "Execute" ]
            Html.h2 "Controllers"
            Html.div [
                prop.className "controller-modes"
                prop.children [
                    Html.article [ Html.h3 "Manual"; Html.p "Issue explicit movement commands." ]
                    Html.article [ Html.h3 "Scripted AI"; Html.p "Repeat a deterministic direction script." ]
                    Html.article [ Html.h3 "General AI"; Html.p "Approach the nearest hostile until its class-specific melee or ranged attack is in range." ]
                ]
            ]
            match selected with
            | None ->
                Html.p "Select a unit on the map to configure its controller."
            | Some unit ->
                Html.h3 ("Unit " + string unit.Id + " · " + unit.ClassId)
                let movementProfile =
                    MapEditorSimulator.movementProfileFor unit
                let movementCredit =
                    Map.tryFind unit.Id handoff.MovementCreditsMillimeters
                    |> Option.defaultValue 0
                Html.p [
                    prop.className "movement-readout"
                    prop.text (
                        "Movement "
                        + string movementProfile.SpeedMillimetersPerSecond
                        + " mm/s · credit "
                        + string movementCredit
                        + " mm · 500 mm cells · 50 ms ticks · Run preview 1×"
                    )
                ]
                Html.label [ prop.htmlFor "unit-controller"; prop.text "Controller" ]
                Html.select [
                    prop.id "unit-controller"
                    prop.value (MapEditor.controllerLabel unit.Controller)
                    prop.disabled state.IsRunning
                    prop.onKeyDown (fun event -> event.stopPropagation ())
                    prop.onChange (fun value ->
                        let controller =
                            match value with
                            | "Scripted AI" -> Scripted
                            | "General AI" -> General
                            | _ -> Manual
                        dispatch (SimulatorChanged(SetSimulatorController controller)))
                    prop.children [
                        Html.option [ prop.value "Manual"; prop.text "Manual" ]
                        Html.option [ prop.value "Scripted AI"; prop.text "Scripted AI" ]
                        Html.option [ prop.value "General AI"; prop.text "General AI" ]
                    ]
                ]
                Html.label [ prop.htmlFor "unit-script"; prop.text "Direction script" ]
                Html.input [
                    prop.id "unit-script"
                    prop.key ("unit-script-" + string unit.Id)
                    prop.type'.text
                    prop.defaultValue (MapEditor.scriptText unit.Script)
                    prop.disabled state.IsRunning
                    prop.onKeyDown (fun event -> event.stopPropagation ())
                    prop.placeholder "N,E,E,S"
                    prop.onChange (fun value ->
                        dispatch (SimulatorChanged(SetSimulatorScript value)))
                ]
                Html.div [
                    prop.className "manual-movement"
                    prop.children [
                        for label, direction in movement do
                            button label ("Move unit " + label) state.IsRunning (fun _ ->
                                dispatch (SimulatorChanged(MoveSimulatorUnit direction)))
                    ]
                ]
                Html.h3 "Route planner"
                Html.p "Arrow keys choose a destination. The planner routes around terrain, blocking edges, and occupied footprints; Enter queues the accepted route."
                Html.div [
                    prop.className "manual-movement"
                    prop.children [
                        button "←" "Move route preview left" state.IsRunning (fun _ ->
                            dispatch (SimulatorChanged(MoveSimulatorPreview(-1, 0))))
                        button "↑" "Move route preview up" state.IsRunning (fun _ ->
                            dispatch (SimulatorChanged(MoveSimulatorPreview(0, -1))))
                        button "↓" "Move route preview down" state.IsRunning (fun _ ->
                            dispatch (SimulatorChanged(MoveSimulatorPreview(0, 1))))
                        button "→" "Move route preview right" state.IsRunning (fun _ ->
                            dispatch (SimulatorChanged(MoveSimulatorPreview(1, 0))))
                        button "Commit route" "Commit clear route preview (Enter)" (state.IsRunning || handoff.PreviewDestination.IsNone) (fun _ ->
                            dispatch (SimulatorChanged CommitSimulatorPreview))
                        button "Reset route" "Return route preview to unit origin (Backspace)" (state.IsRunning || handoff.PreviewDestination.IsNone) (fun _ ->
                            dispatch (SimulatorChanged ResetSimulatorPreviewToOrigin))
                        button "Cancel route" "Cancel route preview (Escape)" (state.IsRunning || handoff.PreviewDestination.IsNone) (fun _ ->
                            dispatch (SimulatorChanged ResetSimulatorPreview))
                    ]
                ]
                match handoff.PreviewDestination with
                | Some destination ->
                    match MapEditorSimulator.preview state.SelectedUnit destination handoff with
                    | Some preview ->
                        Html.p [
                            prop.role.status
                            prop.ariaLive.polite
                            prop.text (
                                "Distance "
                                + string preview.Distance
                                + " steps / "
                                + string preview.DistanceMillimeters
                                + " mm · cost "
                                + string preview.MovementCostMillimeters
                                + " mm credit · "
                                + (if preview.Collision = RouteClear then
                                       "route clear"
                                   else
                                       "collision: " + string preview.Collision)
                            )
                        ]
                    | None -> Html.none
                | None -> Html.p "No route preview."
            Html.div [
                prop.className "control-row simulation-controls"
                prop.children [
                    button
                        (if state.IsRunning then "Pause" else "Run")
                        (if state.IsRunning then "Pause map simulation" else "Run map simulation")
                        false
                        (fun _ -> dispatch (SimulatorChanged ToggleSimulatorRun))
                    button "Step" "Advance the map simulation one tick" state.IsRunning (fun _ ->
                        dispatch (SimulatorChanged StepSimulator))
                ]
            ]
            state.Validation
            |> Option.map (fun error ->
                Html.p [
                    prop.className "validation-error"
                    prop.role.alert
                    prop.text error
                ])
            |> Option.defaultValue Html.none
            Html.h3 "Perspective and visibility"
            Html.p [
                prop.className "validation-error"
                prop.role.status
                prop.text MapEditorSimulator.PerspectiveUnavailableReason
            ]
            button "Player perspective unavailable" MapEditorSimulator.PerspectiveUnavailableReason true ignore
            Html.p [
                prop.className "validation-error"
                prop.role.status
                prop.text MapEditorSimulator.VisibilityUnavailableReason
            ]
            button "Visibility overlays unavailable" MapEditorSimulator.VisibilityUnavailableReason true ignore
            Html.h3 "Latest tick"
            if List.isEmpty state.LastEvents then
                Html.p "No actions resolved."
            else
                Html.ul [
                    for event in state.LastEvents do
                        Html.li event
                ]
        ]
    ]

let private editorDestructiveConfirmation state dispatch =
    match state.PendingDestructiveChange with
    | None -> Html.none
    | Some pending ->
        let message =
            match pending with
            | ClearPending -> "Clear every object from the current map?"
            | NewMapPending(width, height, name) ->
                "Create “"
                + name
                + "” at "
                + string width
                + "×"
                + string height
                + " and replace the current draft?"
            | ResizePending loss ->
                "Resize to "
                + string loss.TargetWidth
                + "×"
                + string loss.TargetHeight
                + "; remove "
                + string loss.LostTerrainCells
                + " terrain cells, "
                + string loss.LostEdges
                + " edges, "
                + string loss.LostUnits
                + " units, and "
                + string loss.LostRegions
                + " regions?"
            | UnitDeletionPending identifiers ->
                "Delete "
                + string identifiers.Length
                + (if identifiers.Length = 1 then
                       " unit and its attached authoring data?"
                   else
                       " units and their attached authoring data?")
        Html.div [
            prop.className "editor-modal-backdrop"
            prop.children [
                Html.section [
                    prop.id "editor-destructive-confirmation"
                    prop.tabIndex -1
                    prop.className "panel editor-confirmation editor-modal"
                    prop.custom ("role", "alertdialog")
                    prop.ariaLabel "Confirm destructive map command"
                    prop.children [
                        Html.h2 "Confirmation required"
                        Html.p message
                        Html.div [
                            prop.className "control-row"
                            prop.children [
                                button
                                    "Confirm"
                                    "Confirm destructive map command"
                                    false
                                    (fun _ ->
                                        dispatch (
                                            EditorChanged
                                                ConfirmDestructiveChange
                                        ))
                                button
                                    "Cancel"
                                    "Cancel destructive map command"
                                    false
                                    (fun _ ->
                                        dispatch (
                                            EditorChanged
                                                CancelDestructiveChange
                                        ))
                            ]
                        ]
                    ]
                ]
            ]
        ]

let private editorDesktopChrome
    (state: MapEditorState)
    (view: EditorWorkspaceState)
    (simulator: SimulatorHandoff option)
    dispatch
    =
    let menu
        (label: string)
        (children: Fable.React.ReactElement list)
        =
        Html.details [
            prop.className "editor-menu desktop-menu"
            prop.children [
                Html.summary [
                    prop.text label
                    prop.onClick (fun event ->
                        closeSiblingDesktopMenus event.currentTarget)
                ]
                Html.div [
                    prop.className "editor-menu-popover"
                    prop.children children
                ]
            ]
        ]
    let importMapControl (label: string) =
        Html.label [
            prop.className "editor-file-command"
            prop.children [
                Html.span label
                Html.input [
                    prop.type'.file
                    prop.accept ".sir-map,.dd2vtt,.uvtt,.json,.xml,text/plain,application/json"
                    prop.ariaLabel "Import SIR map"
                    prop.title "Import SIR-MAP directly or review a Universal VTT, Foundry, or Fantasy Grounds export"
                    prop.onChange (fun (files: File list) ->
                        closeDesktopMenus ()
                        files
                        |> List.tryHead
                        |> Option.iter (MapFileSelected >> dispatch))
                ]
            ]
        ]
    let command label aria disabled message =
        button label aria disabled (fun _ ->
            closeDesktopMenus ()
            dispatch message)

    Html.section [
        prop.className "editor-desktop-chrome"
        prop.ariaLabel "Map editor menu and toolbar"
        prop.children [
            Html.div [
                prop.className "editor-document-strip"
                prop.children [
                    Html.strong state.Authoring.Name
                    Html.span (
                        (match state.RevisionState with
                         | DirtyRevision -> "Modified"
                         | SavedRevision -> "Saved"
                         | SimulatedRevision -> "Simulated"
                         | RecoveredRevision -> "Recovered")
                        + " · r"
                        + string state.Revision.Number
                    )
                    Html.span (
                        match simulator with
                        | None -> "Not in Simulator"
                        | Some value when
                            MapEditorSimulator.isBehindDraft state value ->
                            "Simulator behind"
                        | Some _ -> "Simulator current"
                    )
                ]
            ]
            Html.nav [
                prop.className "editor-menu-bar"
                prop.ariaLabel "Map editor menus"
                prop.children [
                    menu
                        "File"
                        [ command
                              "New map"
                              "Create a new empty map"
                              false
                              (EditorChanged RequestNewMap)
                          importMapControl "Open / Import…"
                          command
                              "Save map file"
                              "Export the current map document"
                              false
                              ExportMap
                          command
                              "Repository bundle"
                              "Download editor and simulator design work for version-controlled import"
                              false
                              ExportDesignBundle ]
                    menu
                        "Edit"
                        [ command "Undo" "Undo last editor command" state.UndoHistory.IsEmpty (EditorChanged UndoEditorCommand)
                          command "Redo" "Redo last editor command" state.RedoHistory.IsEmpty (EditorChanged RedoEditorCommand)
                          command "Copy" "Copy selected units" state.SelectedUnits.IsEmpty (EditorChanged CopyEditorSelection)
                          command "Paste" "Paste copied units" state.Clipboard.IsNone (EditorChanged PasteEditorClipboard)
                          command "Duplicate" "Duplicate selected units" state.SelectedUnits.IsEmpty (EditorChanged DuplicateEditorSelection)
                          command "Delete" "Delete selected objects" (state.SelectedUnits.IsEmpty && state.SelectedRegion.IsNone) (EditorChanged DeleteEditorSelection)
                          command "Select all" "Select all objects in the active domain" false (EditorChanged SelectAllInActiveDomain) ]
                    menu
                        "View"
                        [ command "Fit map" "Fit the complete map" false (EditorWorkspaceChanged FitEditorBoard)
                          command "Actual size" "Reset map camera to one hundred percent" false (EditorWorkspaceChanged ResetEditorCamera)
                          command "Frame selection" "Frame selected map objects" state.SelectedUnits.IsEmpty (EditorWorkspaceChanged FrameEditorSelection)
                          command "Toggle command panel · F2" "Show or hide the active command panel" false ToggleEditorToolPanelVisibility
                          command
                              (if view.InspectorCollapsed then "Show inspector · F3" else "Hide inspector · F3")
                              "Show or hide the selected-object inspector"
                              false
                              (EditorWorkspaceChanged ToggleEditorInspector) ]
                    menu
                        "Map"
                        [ command "Terrain tools" "Show terrain command panel" false (EditorToolPanelChanged TerrainTools)
                          command "Unit tools" "Show unit command panel" false (EditorToolPanelChanged UnitTools)
                          command "Edge tools" "Show edge command panel" false (EditorToolPanelChanged EdgeTools)
                          command "Zone tools" "Show zone command panel" false (EditorToolPanelChanged ZoneTools)
                          command "Map properties" "Show map document command panel" false (EditorToolPanelChanged DocumentTools)
                          command "Simulate revision" "Validate and send this revision to Simulator" false SimulateEditorRevision ]
                ]
            ]
            Html.div [
                prop.className "editor-quick-toolbar"
                prop.role.toolbar
                prop.ariaLabel "Map editor quick access"
                prop.children [
                    command "New" "Create a new empty map" false (EditorChanged RequestNewMap)
                    importMapControl "Open"
                    command "Save" "Export the current map document" false ExportMap
                    Html.span [ prop.className "toolbar-separator"; prop.ariaHidden true ]
                    command "Undo" "Undo last editor command" state.UndoHistory.IsEmpty (EditorChanged UndoEditorCommand)
                    command "Redo" "Redo last editor command" state.RedoHistory.IsEmpty (EditorChanged RedoEditorCommand)
                    Html.span [ prop.className "toolbar-separator"; prop.ariaHidden true ]
                    command "Select" "Activate selection tool" false (EditorChanged(ChooseTool Select))
                    command "Pencil" "Activate terrain pencil" false (EditorChanged(ChooseTool(Terrain PencilTool)))
                    command "Units" "Toggle unit command panel" false (EditorToolPanelChanged UnitTools)
                    command "Fit" "Fit the complete map" false (EditorWorkspaceChanged FitEditorBoard)
                    command
                        "−"
                        "Zoom out around workspace center"
                        false
                        (EditorWorkspaceChanged(
                            ZoomEditorAt(
                                view.ViewportWidth / 2.0,
                                view.ViewportHeight / 2.0,
                                0.8
                            )
                        ))
                    command
                        "+"
                        "Zoom in around workspace center"
                        false
                        (EditorWorkspaceChanged(
                            ZoomEditorAt(
                                view.ViewportWidth / 2.0,
                                view.ViewportHeight / 2.0,
                                1.25
                            )
                        ))
                    command "Simulate" "Validate and send this revision to Simulator" false SimulateEditorRevision
                ]
            ]
        ]
    ]

let private simulatorDesktopChrome
    (editor: MapEditorState)
    (state: MapEditorState)
    (handoff: SimulatorHandoff)
    panelVisible
    (dispatch: Msg -> unit)
    =
    let menu (label: string) (children: Fable.React.ReactElement list) =
        Html.details [
            prop.className "editor-menu desktop-menu"
            prop.children [
                Html.summary [
                    prop.text label
                    prop.onClick (fun event ->
                        closeSiblingDesktopMenus event.currentTarget)
                ]
                Html.div [
                    prop.className "editor-menu-popover"
                    prop.children children
                ]
            ]
        ]
    let command (label: string) (aria: string) disabled message =
        button label aria disabled (fun _ ->
            closeDesktopMenus ()
            dispatch message)

    Html.section [
        prop.className "editor-desktop-chrome simulator-desktop-chrome"
        prop.ariaLabel "Simulator menu and toolbar"
        prop.children [
            Html.div [
                prop.className "editor-document-strip"
                prop.children [
                    Html.strong editor.Authoring.Name
                    Html.span (
                        "Simulation tick "
                        + string handoff.Tick
                        + " · "
                        + if state.IsRunning then "Running" else "Paused"
                    )
                    Html.span (
                        "Revision "
                        + string handoff.Revision.Number
                        + " · "
                        + handoff.Revision.Digest.Substring(0, 12)
                    )
                ]
            ]
            Html.nav [
                prop.className "editor-menu-bar"
                prop.ariaLabel "Simulator menus"
                prop.children [
                    menu
                        "File"
                        [ command "Open map in Editor" "Open the simulated map in Editor" false (WorkspaceChanged EditorWorkspace)
                          command "Repository bundle" "Download editor and simulator design work" false ExportDesignBundle ]
                    menu
                        "View"
                        [ command "Reset camera" "Reset battlefield camera" false (BattlefieldChanged ResetCamera)
                          command "Zoom in" "Zoom battlefield in" false (BattlefieldChanged(ZoomBy 1.25))
                          command "Zoom out" "Zoom battlefield out" false (BattlefieldChanged(ZoomBy 0.8))
                          command
                              (if panelVisible then "Hide command panel · F2" else "Show command panel · F2")
                              "Show or hide the active simulator command panel"
                              false
                              ToggleSimulatorToolPanelVisibility ]
                    menu
                        "Simulation"
                        [ command
                              (if state.IsRunning then "Pause" else "Run")
                              "Run or pause deterministic simulation"
                              false
                              (SimulatorChanged ToggleSimulatorRun)
                          command "Step" "Advance simulation one tick" state.IsRunning (SimulatorChanged StepSimulator)
                          command "Reset simulation" "Reset runtime state to the immutable editor revision" state.IsRunning RequestSimulatorReset ]
                    menu
                        "Samples"
                        [ for sample in ExperienceSamples.maps do
                              command
                                  sample.Title
                                  ("Load simulation sample: " + sample.Summary)
                                  false
                                  (LoadSimulationSample sample.Id) ]
                ]
            ]
            Html.div [
                prop.className "editor-quick-toolbar"
                prop.role.toolbar
                prop.ariaLabel "Simulator quick access"
                prop.children [
                    command
                        (if state.IsRunning then "Pause" else "Run")
                        "Run or pause deterministic simulation"
                        false
                        (SimulatorChanged ToggleSimulatorRun)
                    command "Step" "Advance simulation one tick" state.IsRunning (SimulatorChanged StepSimulator)
                    command "Reset" "Reset simulation to its immutable revision" state.IsRunning RequestSimulatorReset
                    Html.span [ prop.className "toolbar-separator"; prop.ariaHidden true ]
                    command "Controls" "Toggle simulator controls panel" false (SimulatorPanelChanged ControllerTools)
                    command "Events" "Toggle simulator events panel" false (SimulatorPanelChanged EventTools)
                    command "Samples" "Toggle simulator samples panel" false (SimulatorPanelChanged SimulatorSampleTools)
                    Html.span [ prop.className "toolbar-separator"; prop.ariaHidden true ]
                    command "−" "Zoom battlefield out" false (BattlefieldChanged(ZoomBy 0.8))
                    command "+" "Zoom battlefield in" false (BattlefieldChanged(ZoomBy 1.25))
                    command "Fit" "Reset battlefield camera" false (BattlefieldChanged ResetCamera)
                ]
            ]
        ]
    ]

let private simulatorDock
    (handoff: SimulatorHandoff)
    (state: MapEditorState)
    (activePanel: SimulatorToolPanel)
    panelVisible
    (dispatch: Msg -> unit)
    =
    let choose (label: string) (panel: SimulatorToolPanel) =
        Html.button [
            prop.type'.button
            prop.text label
            prop.ariaPressed (
                panelVisible && Object.Equals(activePanel, panel)
            )
            prop.onClick (fun _ ->
                dispatch (SimulatorPanelChanged panel))
        ]
    Html.section [
        prop.className (
            "panel editor-tools editor-ribbon simulator-ribbon"
            + if panelVisible then "" else " is-collapsed"
        )
        prop.ariaLabel "Simulator command panel"
        prop.children [
            Html.nav [
                prop.className "editor-tool-navigation compact-tool-rail"
                prop.ariaLabel "Simulator command groups"
                prop.children [
                    choose "Controls" ControllerTools
                    choose "Events" EventTools
                    choose "Samples" SimulatorSampleTools
                    button
                        (if panelVisible then "Close · F2" else "Open · F2")
                        "Show or hide the active simulator command panel"
                        false
                        (fun _ -> dispatch ToggleSimulatorToolPanelVisibility)
                ]
            ]
            if panelVisible then
                Html.div [
                    prop.className "editor-tool-panel editor-context-palette simulator-context-palette"
                    prop.children [
                        match activePanel with
                        | ControllerTools ->
                            controllerPanel handoff state dispatch
                        | EventTools ->
                            Html.h3 ("Tick " + string handoff.Tick + " events")
                            if List.isEmpty state.LastEvents then
                                Html.p "No actions resolved on the latest tick."
                            else
                                Html.ol [
                                    prop.ariaLabel "Latest deterministic simulation events"
                                    prop.children [
                                        for event in state.LastEvents do
                                            Html.li event
                                    ]
                                ]
                            if not (List.isEmpty handoff.LastCombatEvents) then
                                Html.h3 "Recent combat"
                                Html.ol [
                                    prop.ariaLabel "Recent combat events"
                                    prop.children [
                                        for combat in handoff.LastCombatEvents do
                                            Html.li (
                                                "Tick "
                                                + string combat.Tick
                                                + " · "
                                                + combat.Summary
                                            )
                                    ]
                                ]
                            Html.h3 "Runtime roster"
                            Html.ul [
                                prop.ariaLabel "Simulation unit health"
                                prop.children [
                                    for _, unit in state.Map.Units |> Map.toList do
                                        Html.li (
                                            "Unit "
                                            + string unit.Id
                                            + " · "
                                            + unit.ClassId
                                            + " · "
                                            + string unit.Health
                                            + "/"
                                            + string unit.HealthMaximum
                                            + " HP · "
                                            + string (
                                                (MapEditorSimulator.movementProfileFor unit)
                                                    .SpeedMillimetersPerSecond
                                            )
                                            + " mm/s"
                                        )
                                ]
                            ]
                        | SimulatorSampleTools ->
                            Html.h3 "Simulation samples"
                            Html.p "Loading a sample replaces the current editor draft and simulator sandbox."
                            for sample in ExperienceSamples.maps do
                                Html.article [
                                    prop.className "sample-compact-card"
                                    prop.children [
                                        Html.h4 sample.Title
                                        Html.p sample.Summary
                                        button
                                            "Load"
                                            ("Load simulation sample " + sample.Title)
                                            false
                                            (fun _ ->
                                                dispatch (LoadSimulationSample sample.Id))
                                    ]
                                ]
                    ]
                ]
        ]
    ]

let private sampleCatalogView (dispatch: Msg -> unit) =
    let mapCard (sample: ExperienceMapSample) =
        Html.details [
            prop.className "panel sample-list-item sample-card"
            prop.children [
                Html.summary [
                    Html.span [ prop.className "sample-kind"; prop.text "Map · Simulation" ]
                    Html.strong sample.Title
                    Html.span [ prop.className "sample-summary"; prop.text sample.Summary ]
                ]
                Html.div [
                    prop.className "sample-list-body"
                    prop.children [
                        Html.ul [
                            for highlight in sample.Highlights do
                                Html.li highlight
                        ]
                        Html.div [
                            prop.className "control-row"
                            prop.children [
                                button "Open map" ("Open " + sample.Title + " in Editor") false (fun _ ->
                                    dispatch (LoadMapSample sample.Id))
                                button "Run simulation" ("Run " + sample.Title + " in Simulator") false (fun _ ->
                                    dispatch (LoadSimulationSample sample.Id))
                            ]
                        ]
                    ]
                ]
            ]
        ]
    let replayCard (sample: ExperienceReplaySample) =
        Html.details [
            prop.className "panel sample-list-item sample-card"
            prop.children [
                Html.summary [
                    Html.span [ prop.className "sample-kind"; prop.text "Replay" ]
                    Html.strong sample.Title
                    Html.span [ prop.className "sample-summary"; prop.text sample.Summary ]
                ]
                Html.div [
                    prop.className "sample-list-body"
                    prop.children [
                        Html.p (
                            string sample.Ticks
                            + " deterministic sample ticks · locally navigable · sandbox evidence"
                        )
                        button "Open replay" ("Open replay walkthrough " + sample.Title) false (fun _ ->
                            dispatch (LoadReplaySample sample.Id))
                    ]
                ]
            ]
        ]
    Html.section [
        prop.className "samples-workspace"
        prop.ariaLabel "Curated maps simulations and replays"
        prop.children [
            Html.div [
                prop.className "samples-heading"
                prop.children [
                    Html.p [ prop.className "eyebrow"; prop.text "Explore mechanics" ]
                    Html.h2 "Curated samples"
                    Html.p "Open a canonical map for editing, run its deterministic controller sandbox, or inspect a precomputed replay walkthrough."
                ]
            ]
            Html.h3 "Maps and simulations"
            Html.div [
                prop.className "sample-list"
                prop.children [
                    for sample in ExperienceSamples.maps do
                        mapCard sample
                ]
            ]
            Html.h3 "Replay walkthroughs"
            Html.div [
                prop.className "sample-list"
                prop.children [
                    for sample in ExperienceSamples.replays do
                        replayCard sample
                ]
            ]
            Html.p [
                prop.className "sample-disclosure"
                prop.text "Curated walkthroughs are deterministic sandbox evidence, not cryptographically verified match replays."
            ]
        ]
    ]

let private planningCommandLabel (command: PlanningCommand) =
    match command.Kind with
    | PlannedRoute cells -> "Route · " + string cells.Length + " waypoints"
    | PlannedFacing direction -> "Facing · " + string direction
    | PlannedAttention direction -> "Attention · " + string direction
    | PlannedStance stance -> "Stance · " + stance
    | PlannedHold -> "Hold"
    | PlannedEngagement(target, capability) ->
        "Engage unit " + string target + " · " + capability
    | PlannedSynchronization(marker, deadline) ->
        "Sync " + marker + " by tick " + string deadline

let private planningWorkspace
    (editor: MapEditorState)
    (state: PlanningWorkspaceState)
    dispatch
    =
    let planningButton label action =
        button label label false (fun _ -> dispatch (PlanningChanged action))

    let directions =
        [ "N", North
          "NE", NorthEast
          "E", East
          "SE", SouthEast
          "S", South
          "SW", SouthWest
          "W", West
          "NW", NorthWest ]

    let selected =
        state.SelectedUnit
        |> Option.bind (fun id -> state.Roster |> Array.tryFind (fun unit -> unit.UnitId = id))

    Html.div [
        prop.className "planning-workspace"
        prop.ariaLabel "Coordinated planning workspace"
        prop.children [
            Html.header [
                prop.className "panel planning-status"
                prop.children [
                    Html.div [
                        Html.p [ prop.className "eyebrow"; prop.text "Authored" ]
                        Html.strong ("Revision " + string state.Revision)
                        Html.span (" · " + state.Digest.Substring(0, 12))
                    ]
                    Html.div [
                        Html.p [ prop.className "eyebrow"; prop.text "Predicted" ]
                        Html.strong (
                            state.Predicted
                            |> Option.map (fun value ->
                                string value.Label + " · revision " + string value.Revision)
                            |> Option.defaultValue "Not previewed"
                        )
                    ]
                    Html.div [
                        Html.p [ prop.className "eyebrow"; prop.text "Accepted" ]
                        Html.strong (
                            state.AcceptedRevision
                            |> Option.map (fun value -> "Revision " + string value)
                            |> Option.defaultValue "Not validated"
                        )
                    ]
                    Html.div [
                        Html.p [ prop.className "eyebrow"; prop.text "Committed" ]
                        Html.strong (
                            match state.CommittedRevision, state.CommittedTick with
                            | Some revision, Some tick ->
                                "Revision " + string revision + " · tick " + string tick
                            | _ -> "Not committed"
                        )
                    ]
                    Html.p [
                        prop.className "planning-worker-status"
                        prop.role.status
                        prop.ariaLive.polite
                        prop.text state.WorkerStatus
                    ]
                ]
            ]
            Html.nav [
                prop.className "panel planning-tools"
                prop.ariaLabel "Battlefield planning tools"
                prop.children [
                    for label, key, tool in
                        [ "Route", "R", RouteTool
                          "Facing", "F", FacingTool
                          "Attention", "A", AttentionTool
                          "Stance", "S", StanceTool
                          "Hold", "H", HoldTool
                          "Engage", "E", EngagementTool
                          "Sync", "M", SynchronizationTool ] do
                        Html.button [
                            prop.type'.button
                            prop.ariaPressed (state.Tool = tool)
                            prop.text (label + " · " + key)
                            prop.onClick (fun _ ->
                                dispatch (PlanningChanged(ChoosePlanningTool tool)))
                        ]
                    button "Undo" "Undo planning edit · Ctrl+Z" (List.isEmpty state.Past) (fun _ ->
                        dispatch (PlanningChanged UndoPlanning))
                    button "Redo" "Redo planning edit · Ctrl+Y" (List.isEmpty state.Future) (fun _ ->
                        dispatch (PlanningChanged RedoPlanning))
                    button "Validate" "Validate authored revision in worker" false (fun _ ->
                        dispatch ValidatePlanningRevision)
                    button "Preview" "Preview authored revision as intent-only prediction" false (fun _ ->
                        dispatch PreviewPlanningRevision)
                    button
                        "Commit"
                        "Commit accepted authored revision"
                        (state.AcceptedRevision <> Some state.Revision)
                        (fun _ -> dispatch CommitPlanningRevision)
                ]
            ]
            Html.aside [
                prop.className "panel planning-roster"
                prop.ariaLabel ("Planning roster, " + string state.Roster.Length + " units")
                prop.children [
                    Html.h2 "Roster"
                    Html.div [
                        prop.className "planning-roster-list"
                        prop.children [
                            for unit in state.Roster do
                                Html.button [
                                    prop.type'.button
                                    prop.ariaPressed (state.SelectedUnit = Some unit.UnitId)
                                    prop.text (
                                        unit.Name + " · " + unit.Side + " · "
                                        + unit.Role + " · "
                                        + String.concat ", " unit.Equipment + " · "
                                        + string unit.Column + "," + string unit.Row
                                    )
                                    prop.onClick (fun _ ->
                                        dispatch (PlanningChanged(SelectPlanningUnit unit.UnitId)))
                                ]
                        ]
                    ]
                ]
            ]
            Html.section [
                prop.className "panel planning-battlefield"
                prop.ariaLabel "Battlefield route authoring"
                prop.children [
                    Html.h2 "Battlefield plan"
                    Html.p (
                        "Selected tool: " + string state.Tool
                        + ". Grid cells are native buttons; Enter or Space performs the same edit as pointer activation."
                    )
                    Html.div [
                        prop.className "planning-cell-grid"
                        prop.children [
                            for row in 0 .. int editor.Map.Height - 1 do
                                for column in 0 .. int editor.Map.Width - 1 do
                                    let occupants =
                                        state.Roster
                                        |> Array.filter (fun unit ->
                                            unit.Column = int32 column && unit.Row = int32 row)
                                    Html.button [
                                        prop.type'.button
                                        prop.custom ("data-planning-column", string column)
                                        prop.custom ("data-planning-row", string row)
                                        prop.ariaLabel (
                                            "Cell " + string column + ", " + string row
                                            + (if Array.isEmpty occupants then ""
                                               else
                                                   "; "
                                                   + (occupants
                                                      |> Array.map _.Name
                                                      |> String.concat ", "))
                                            + "; add route waypoint"
                                        )
                                        prop.text (
                                            if Array.isEmpty occupants then
                                                string column + "," + string row
                                            else
                                                occupants |> Array.map (fun unit -> string unit.UnitId) |> String.concat ","
                                        )
                                        prop.onClick (fun _ ->
                                            dispatch (
                                                PlanningChanged(
                                                    AddRouteWaypoint(int32 column, int32 row)
                                                )
                                            ))
                                    ]
                        ]
                    ]
                ]
            ]
            Html.aside [
                prop.className "panel planning-inspector"
                prop.ariaLabel "Planning inspector"
                prop.children [
                    Html.h2 "Inspector"
                    match selected with
                    | None -> Html.p "Select a roster unit."
                    | Some unit ->
                        Html.h3 unit.Name
                        Html.p ("Map position " + string unit.Column + ", " + string unit.Row)
                        Html.p ("Role: " + unit.Role)
                        Html.p ("Equipment: " + String.concat ", " unit.Equipment)
                        Html.p (
                            "Capability descriptors: "
                            + String.concat ", " unit.CapabilityIds
                        )
                        match state.Tool with
                        | RouteTool ->
                            Html.p "Choose a battlefield cell, or use these keyboard-operable waypoint controls."
                            Html.div [
                                prop.className "planning-direction-grid"
                                prop.children [
                                    planningButton "Waypoint west" (AddRouteWaypoint(unit.Column - 1, unit.Row))
                                    planningButton "Waypoint north" (AddRouteWaypoint(unit.Column, unit.Row - 1))
                                    planningButton "Waypoint south" (AddRouteWaypoint(unit.Column, unit.Row + 1))
                                    planningButton "Waypoint east" (AddRouteWaypoint(unit.Column + 1, unit.Row))
                                ]
                            ]
                        | FacingTool
                        | AttentionTool ->
                            Html.div [
                                prop.className "planning-direction-grid"
                                prop.children [
                                    for label, direction in directions do
                                        planningButton
                                            label
                                            (if state.Tool = FacingTool then
                                                 SetPlanningFacing direction
                                             else SetPlanningAttention direction)
                                ]
                            ]
                        | StanceTool ->
                            planningButton "Standing" (SetPlanningStance "standing")
                            planningButton "Crouched" (SetPlanningStance "crouched")
                            planningButton "Prone" (SetPlanningStance "prone")
                        | HoldTool -> planningButton "Add hold" AddPlanningHold
                        | EngagementTool ->
                            match
                                state.Roster
                                |> Array.tryFind (fun target -> target.UnitId <> unit.UnitId),
                                unit.CapabilityIds |> Array.tryHead
                            with
                            | Some target, Some capability ->
                                planningButton
                                    ("Engage " + target.Name + " with " + capability)
                                    (AddPlanningEngagement(target.UnitId, capability))
                            | None, _ ->
                                Html.p "No other roster unit is available as a disclosed target."
                            | _, None ->
                                Html.p "This authored loadout has no accepted engagement capability."
                        | SynchronizationTool ->
                            planningButton
                                "Add synchronization marker"
                                (AddPlanningSynchronization("sync-" + string state.NextCommand, 600))
                    button
                        "Remove selected command"
                        "Remove selected planning command · Delete"
                        state.SelectedCommand.IsNone
                        (fun _ -> dispatch (PlanningChanged RemoveSelectedPlanningCommand))
                ]
            ]
            Html.section [
                prop.className "panel planning-timeline"
                prop.ariaLabel "Planning timeline lanes"
                prop.children [
                    Html.h2 "Timeline lanes"
                    for unit in state.Roster do
                        let commands: PlanningCommand list =
                            state.Commands
                            |> List.filter (fun command -> command.UnitId = unit.UnitId)
                        Html.div [
                            prop.className "planning-lane"
                            prop.custom ("data-planning-unit", string unit.UnitId)
                            prop.children [
                                Html.strong unit.Name
                                if List.isEmpty commands then Html.span "No authored commands"
                                for command in commands do
                                    Html.button [
                                        prop.type'.button
                                        prop.ariaPressed (state.SelectedCommand = Some command.Id)
                                        prop.text (planningCommandLabel command)
                                        prop.onClick (fun _ ->
                                            dispatch (PlanningChanged(SelectPlanningCommand command.Id)))
                                    ]
                            ]
                        ]
                ]
            ]
            Html.section [
                prop.className "panel planning-validation"
                prop.ariaLabel "Planning validation navigation"
                prop.children [
                    Html.h2 ("Validation · " + string state.Issues.Length + " issues")
                    Html.p "Use the issue buttons or bracket keys to move selection to the affected command."
                    for index, issue in Array.indexed state.Issues do
                        Html.button [
                            prop.type'.button
                            prop.ariaPressed (state.FocusedIssue = Some index)
                            prop.text (issue.Code + " · " + issue.Detail)
                            prop.onClick (fun _ ->
                                dispatch (PlanningChanged(FocusPlanningIssue index)))
                        ]
                ]
            ]
            Html.details [
                Html.summary "Deterministic plan, conflict, and execution review artifact"
                Html.pre (PlanningWorkspace.reviewArtifact state)
                button
                    "Export review artifact"
                    "Export deterministic plan, conflict, and execution evidence"
                    false
                    (fun _ -> dispatch ExportPlanningReview)
            ]
        ]
    ]

let private workspaceNavigation (workspace: WorkspaceMode) dispatch =
    let item (label: string) (value: WorkspaceMode) =
        let isCurrent = workspace = value
        Html.button [
            prop.type'.button
            prop.text label
            prop.ariaPressed isCurrent
            prop.onClick (fun _ -> dispatch (WorkspaceChanged value))
        ]

    Html.nav [
        prop.className "workspace-navigation"
        prop.ariaLabel "Application sections"
        prop.children [
            item "Simulator" SimulatorWorkspace
            item "Planner" PlanningWorkspace
            item "Editor" EditorWorkspace
            item "Replay" ReplayWorkspace
            item "Rules and data" RulesWorkspace
            item "Samples" SamplesWorkspace
        ]
    ]

let private inputGestureText (gesture: InputGesture) =
    let key =
        match NormalizedKey.value gesture.Key with
        | "ArrowLeft" -> "←"
        | "ArrowRight" -> "→"
        | "ArrowUp" -> "↑"
        | "ArrowDown" -> "↓"
        | "Space" -> "Space"
        | "Escape" -> "Esc"
        | value when value.Length = 1 -> value.ToUpperInvariant()
        | value -> value
    [ if gesture.Modifiers.ControlOrMeta then "Ctrl/Cmd"
      if gesture.Modifiers.Alt then "Alt"
      if gesture.Modifiers.Shift && NormalizedKey.value gesture.Key <> "?" then "Shift"
      key
      if gesture.Phase = KeyUp then "release" ]
    |> String.concat "+"

let private ariaShortcut (gesture: InputGesture) =
    let key =
        match NormalizedKey.value gesture.Key with
        | "Space" -> "Space"
        | value when value.Length = 1 -> value.ToUpperInvariant()
        | value -> value
    [ if gesture.Modifiers.ControlOrMeta then "Control"
      if gesture.Modifiers.Alt then "Alt"
      if gesture.Modifiers.Shift && NormalizedKey.value gesture.Key <> "?" then "Shift"
      key ]
    |> String.concat "+"

let private modalInputStrip
    (projection: ModalProjection<ModalCommand>)
    expanded
    dispatch
    =
    Html.section [
        prop.className ("modal-input-strip" + if expanded then " is-expanded" else "")
        prop.ariaLabel "Current input mode"
        prop.children [
            Html.div [
                prop.className "modal-input-summary"
                prop.children [
                    Html.div [
                        prop.className "modal-input-state"
                        prop.children [
                            Html.strong [
                                prop.className "modal-input-headline"
                                prop.text projection.Headline
                            ]
                            Html.span [
                                prop.className "modal-input-detail"
                                prop.text projection.Detail
                            ]
                        ]
                    ]
                    Html.button [
                        prop.id "modal-input-toggle"
                        prop.type'.button
                        prop.className "modal-input-toggle"
                        prop.text "Inputs · ?"
                        prop.ariaExpanded expanded
                        prop.ariaControls "modal-input-panel"
                        prop.custom ("aria-keyshortcuts", "?")
                        prop.onClick (fun _ -> dispatch (ToggleInputHelp false))
                    ]
                ]
            ]
            Html.p [
                prop.className "sr-only"
                prop.role.status
                prop.ariaLive.polite
                prop.ariaAtomic true
                prop.text projection.Headline
            ]
            if expanded then
                Html.section [
                    prop.id "modal-input-panel"
                    prop.className "modal-input-panel"
                    prop.tabIndex -1
                    prop.ariaLabel ("Possible inputs for " + projection.Headline)
                    prop.children [
                        Html.div [
                            prop.className "modal-input-panel-heading"
                            prop.children [
                                Html.h3 "Possible inputs"
                                Html.button [
                                    prop.type'.button
                                    prop.text "Close"
                                    prop.onClick (fun _ -> dispatch (ToggleInputHelp true))
                                ]
                            ]
                        ]
                        if List.isEmpty projection.PossibleInputs then
                            Html.p "No keyboard commands are available in this state."
                        else
                            Html.ul [
                                prop.className "modal-input-list"
                                prop.children [
                                    for input in projection.PossibleInputs do
                                        Html.li [
                                            prop.custom ("data-modal-command", input.Id)
                                            prop.custom ("aria-keyshortcuts", ariaShortcut input.InputGesture)
                                            prop.children [
                                                Html.kbd (inputGestureText input.InputGesture)
                                                Html.span input.Label
                                            ]
                                        ]
                                ]
                            ]
                    ]
                ]
        ]
    ]

let private editorDomain = function
    | TerrainTools -> TerrainDomain
    | UnitTools -> UnitDomain
    | EdgeTools -> EdgeDomain
    | ZoneTools -> RegionDomain
    | DocumentTools -> DocumentDomain

let view model dispatch =
    let shell = model.Shell

    Html.main [
        prop.className "app-shell"
        prop.ariaLabel "S.I.R. simulator and editor"
        prop.onClick (fun event ->
            dismissDesktopMenus event.target)
        prop.children [
            workspaceNavigation model.Workspace dispatch
            match model.Workspace with
            | PlanningWorkspace ->
                match model.Planning with
                | Some planning -> planningWorkspace model.Editor planning dispatch
                | None ->
                    Html.section [
                        prop.className "panel"
                        prop.children [
                            Html.h2 "Planner unavailable"
                            Html.p "Open the planner again to create an authored revision from the current map."
                        ]
                    ]
            | SimulatorWorkspace ->
                match model.Simulator with
                | None ->
                    let facts =
                        { SimulatorHandoffPresent = false
                          SimulatorIsRunning = false
                          SimulatorHasRoutePreview = false
                          SimulatorControllerSelection = None
                          SimulatorRevisionIsStale = false
                          InputHelpExpanded = model.InputHelpExpanded }
                    let catalog =
                        ModalInput.simulatorCatalog
                            model.SimulatorSelectedUnit
                            None
                            model.SimulatorControllerSelection
                    let projection =
                        ModalInput.projectSimulator
                            facts
                            model.SimulatorSelectedUnit
                            None
                            catalog
                    Html.section [
                        prop.className "panel simulator-workspace"
                        prop.ariaLabel "Simulator revision handoff"
                        prop.children [
                            Html.h2 "No simulated revision"
                            Html.p "Return to the editor and choose Simulate to create an immutable sandbox handoff."
                            button "Open Editor" "Open the map editor" false (fun _ ->
                                dispatch (WorkspaceChanged EditorWorkspace))
                            button "Browse samples" "Open curated map and simulation samples" false (fun _ ->
                                dispatch (WorkspaceChanged SamplesWorkspace))
                            modalInputStrip projection model.InputHelpExpanded dispatch
                        ]
                    ]
                | Some simulator ->
                    let simulatorState =
                        MapEditorSimulator.viewState
                            model.SimulatorSelectedUnit
                            simulator
                    let stale =
                        MapEditorSimulator.isBehindDraft model.Editor simulator
                    let facts =
                        { SimulatorHandoffPresent = true
                          SimulatorIsRunning = simulator.IsRunning
                          SimulatorHasRoutePreview = simulator.PreviewDestination.IsSome
                          SimulatorControllerSelection =
                            model.SimulatorControllerSelection
                          SimulatorRevisionIsStale = stale
                          InputHelpExpanded = model.InputHelpExpanded }
                    let catalog =
                        ModalInput.simulatorCatalog
                            model.SimulatorSelectedUnit
                            (Some simulator)
                            model.SimulatorControllerSelection
                    let projection =
                        ModalInput.projectSimulator
                            facts
                            model.SimulatorSelectedUnit
                            (Some simulator)
                            catalog
                    Html.div [
                        prop.className "simulator-workspace"
                        prop.children [
                            simulatorDesktopChrome
                                model.Editor
                                simulatorState
                                simulator
                                model.SimulatorToolPanelVisible
                                dispatch
                            Html.section [
                                prop.className "panel simulator-revision-status"
                                prop.role.status
                                prop.ariaLive.polite
                                prop.children [
                                    Html.h2 (
                                        if stale then
                                            "Simulator behind editor draft"
                                        else
                                            "Simulator matches editor draft"
                                    )
                                    Html.p (
                                        "Simulating immutable revision "
                                        + string simulator.Revision.Number
                                        + " · "
                                        + simulator.Revision.Digest.Substring(0, 12)
                                    )
                                    if stale then
                                        Html.p "Editor changes are preserved separately. Choose Simulate in Editor to reset this sandbox."
                                ]
                            ]
                            Html.div [
                                prop.className "simulator-map-stage"
                                prop.id "simulator-map-stage"
                                prop.tabIndex 0
                                prop.role.application
                                prop.ariaLabel "Keyboard-operable simulator map stage"
                                prop.onKeyDown (fun event ->
                                    if
                                        isSimulatorModalTarget
                                            event.target
                                            event.currentTarget
                                    then
                                        if
                                            not event.ctrlKey
                                            && not event.metaKey
                                            && not event.altKey
                                        then
                                            event.stopPropagation ()
                                            event.preventDefault ()
                                            dispatch (
                                                KeyPressed(
                                                    event.key,
                                                    false,
                                                    event.shiftKey,
                                                    false,
                                                    event.repeat
                                                )
                                            )
                                    else
                                        event.stopPropagation ()
                                    )
                                prop.children [
                                    battlefieldView
                                        shell
                                        (Some(
                                            MapEditorSimulator.frame
                                                model.SimulatorSelectedUnit
                                                simulator
                                        ))
                                        (Some simulatorState)
                                        model.Battlefield
                                        None
                                        1.0
                                        (if
                                             model.Battlefield.ExactTicks
                                             || model.Battlefield.ReducedMotion
                                         then
                                             Map.empty
                                         else
                                             MapEditorSimulator.presentationOffsets simulator)
                                        dispatch
                                    simulatorDock
                                        simulator
                                        simulatorState
                                        model.SimulatorToolPanel
                                        model.SimulatorToolPanelVisible
                                        dispatch
                                    modalInputStrip
                                        projection
                                        model.InputHelpExpanded
                                        dispatch
                                ]
                            ]
                        ]
                    ]
            | EditorWorkspace ->
                let facts =
                    { Editor = model.Editor
                      ActiveDomain = editorDomain model.EditorToolPanel
                      PanHeld = model.EditorSpacePressed
                      InputHelpExpanded = model.InputHelpExpanded }
                let catalog = ModalInput.editorCatalog facts
                let projection = ModalInput.projectEditor facts catalog
                Html.div [
                    prop.className "editor-workspace"
                    prop.children [
                        editorDesktopChrome
                            model.Editor
                            model.EditorView
                            model.Simulator
                            dispatch
                        editorDestructiveConfirmation
                            model.Editor
                            dispatch
                        match model.PendingInterchangeReview with
                        | Some review ->
                            Html.section [
                                prop.id "editor-interchange-review"
                                prop.tabIndex -1
                                prop.className "panel editor-interchange-review"
                                prop.ariaLabel "Interchange import review"
                                prop.role.alert
                                prop.children [
                                    Html.h2 ("Review " + string review.Format + " import")
                                    Html.p (
                                        review.SourceName + " · "
                                        + string (review.Fields |> Array.filter (fun field -> field.Disposition = Mapped) |> Array.length)
                                        + " mapped · "
                                        + string (review.Fields |> Array.filter (fun field -> field.Disposition = Ignored) |> Array.length)
                                        + " ignored · "
                                        + string (review.Fields |> Array.filter (fun field -> field.Disposition = Lossy) |> Array.length)
                                        + " lossy"
                                    )
                                    if not (Array.isEmpty review.Errors) then
                                        Html.ul [
                                            prop.ariaLabel "Import errors"
                                            prop.children [
                                                for error in review.Errors do Html.li error
                                            ]
                                        ]
                                    Html.table [
                                        prop.ariaLabel "Mapped, ignored, lossy, and rejected source fields"
                                        prop.children [
                                            Html.thead [
                                                Html.tr [
                                                    Html.th "Source field"
                                                    Html.th "Disposition"
                                                    Html.th "Meaning"
                                                ]
                                            ]
                                            Html.tbody [
                                                for field in review.Fields do
                                                    Html.tr [
                                                        Html.td field.Path
                                                        Html.td (string field.Disposition)
                                                        Html.td field.Meaning
                                                    ]
                                            ]
                                        ]
                                    ]
                                    button
                                        "Accept reviewed import"
                                        "Accept the reviewed deterministic semantic mappings"
                                        (not (MapEditorInterchange.canAccept review))
                                        (fun _ -> dispatch AcceptInterchangeReview)
                                    button "Cancel import" "Cancel interchange import without changing the map" false (fun _ ->
                                        dispatch RejectInterchangeReview)
                                ]
                            ]
                        | None -> Html.none
                        Html.div [
                            prop.className "editor-map-stage"
                            prop.children [
                                editorBattlefield
                                    model.Editor
                                    model.EditorView
                                    (editorDomain model.EditorToolPanel)
                                    model.EditorSpacePressed
                                    dispatch
                                editorToolbar
                                    model.Editor
                                    model.EditorView
                                    model.EditorToolPanel
                                    model.EditorToolPanelVisible
                                    dispatch
                                if not model.EditorView.InspectorCollapsed then
                                    Html.aside [
                                        prop.className "editor-inspector"
                                        prop.ariaLabel "Map editor inspector"
                                        prop.children [
                                            button
                                                "Hide inspector · F3"
                                                "Toggle selected-object inspector"
                                                false
                                                (fun _ ->
                                                    dispatch (
                                                        EditorWorkspaceChanged
                                                            ToggleEditorInspector
                                                    ))
                                            editorUnitPanel model.Editor dispatch
                                        ]
                                    ]
                                modalInputStrip
                                    projection
                                    model.InputHelpExpanded
                                    dispatch
                            ]
                        ]
                        editorGrid model.Editor dispatch
                    ]
                ]
            | ReplayWorkspace ->
                Html.div [
                    statusView shell
                    workerStatus shell
                    Html.div [
                        prop.className "dashboard"
                        prop.children [
                            sourcePanel shell dispatch
                            controls shell dispatch
                            inspector shell dispatch
                        ]
                    ]
                    battlefieldView
                        shell
                        None
                        None
                        model.Battlefield
                        model.PreviousFrame
                        model.PresentationAlpha
                        Map.empty
                        dispatch
                ]
            | RulesWorkspace ->
                Html.div [
                    scenarioCatalog shell dispatch
                    laboratoryResults shell dispatch
                    comparisonPanel model dispatch
                    Html.div [
                        prop.className "dashboard"
                        prop.children [
                            sandbox shell dispatch
                            inspector shell dispatch
                        ]
                    ]
                    rulesDataCatalog
                ]
            | SamplesWorkspace ->
                sampleCatalogView dispatch
            Html.p [
                prop.className "sr-only"
                prop.role.status
                prop.ariaLive.polite
                prop.text shell.Announcement
            ]
        ]
    ]

if not (isNull (document.getElementById "sir-replay-app")) then
    Program.mkProgram init update view
    |> Program.withSubscription subscriptions
    |> Program.withReactSynchronous "sir-replay-app"
    |> Program.run
