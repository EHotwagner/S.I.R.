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
open SIR.Protocol.Http
open SIR.Protocol.Realtime
open SIR.Client.Web.BrowserInfrastructure
open SIR.Client.Web.AppTypes
open SIR.Client.Web.AppShell

open SIR.Client.Web.CommandRegistry
open SIR.Client.Web.ModeAdapters
open SIR.Client.Web.TacticalOverlayView
open SIR.Client.Web.SceneAdapters
open SIR.Client.Web.PanelViews

let private tacticalCommandAvailable model (command: TacticalCommandDefinition) =
    let availability =
        match command.Availability with
        | AlwaysAvailable -> true
        | TimelineEditable ->
            UnifiedTacticalWorkspace.canEditAt model.Tactical.Cursor model.Tactical
        | TimelineSelectionRequired ->
            model.Planning |> Option.bind _.SelectedCommand |> Option.isSome
        | PredictionRequired ->
            model.Planning |> Option.bind _.Predicted |> Option.isSome
        | CommittedHistoryRequired -> model.Tactical.CommittedThrough >= 0L
        | HelpOpenRequired -> model.InputHelpExpanded
        | PlanningAcceptedRequired ->
            model.Planning
            |> Option.exists (fun planning ->
                planning.AcceptedRevision = Some planning.Revision
                && planning.Predicted
                   |> Option.exists (fun preview -> preview.Revision = planning.Revision))
        | PlanningIssuesRequired ->
            model.Planning |> Option.exists (fun planning -> planning.Issues.Length > 0)
        | ReplayLoadedRequired -> model.Shell.Playback.FinalTick > 0
        | ReplayEventsRequired ->
            model.Shell.Inspection
            |> Option.exists (fun inspection -> not (List.isEmpty inspection.Events))
        | ReplayOperationRequired -> model.Shell.ActiveOperation.IsSome

    let currentTick, finalTick, transportReady =
        match model.Workspace, model.SampleReplayFrames, model.Simulator with
        | ReplayWorkspace, _, _ when model.Shell.Playback.FinalTick > 0 ->
            int64 model.Shell.Playback.CurrentTick,
            int64 model.Shell.Playback.FinalTick,
            model.Shell.Playback.FinalTick > 0
        | _, _, Some simulator ->
            int64 simulator.Tick, model.Tactical.Horizon, true
        | ReplayWorkspace, _, None ->
            int64 model.Shell.Playback.CurrentTick,
            int64 model.Shell.Playback.FinalTick,
            model.Shell.Playback.FinalTick > 0
        | _ -> model.Tactical.Cursor, model.Tactical.Horizon, true

    let transportAvailability =
        match command.Id with
        | "timeline.play-toggle" ->
            model.Simulator.IsSome
            || (model.Workspace = ReplayWorkspace && transportReady)
        | "timeline.step-back"
        | "timeline.home" -> currentTick > 0L
        | "timeline.step-forward"
        | "timeline.end" -> transportReady && currentTick < finalTick
        | _ -> true

    let planningAvailability =
        match command.Id, model.Planning with
        | "planning.undo", Some planning -> PlanningWorkspace.canUndo planning
        | "planning.redo", Some planning -> PlanningWorkspace.canRedo planning
        | "planning.validate", Some planning ->
            planning.Predicted
            |> Option.exists (fun preview -> preview.Revision = planning.Revision)
        | ("timeline.move-command" | "timeline.remove-command"), Some planning ->
            planning.SelectedCommand
            |> Option.bind (fun id ->
                planning.Commands |> List.tryFind (fun current -> current.Id = id))
            |> Option.exists (fun selected ->
                planning.CommittedTick
                |> Option.forall (fun boundary ->
                    selected.EarliestTick > boundary
                    && (command.Id <> "timeline.move-command"
                        || model.Tactical.Cursor > int64 boundary)))
        | id, Some _ when
            id.StartsWith("planning.inspector.", StringComparison.Ordinal)
            ->
            UnifiedTacticalWorkspace.canEditAt
                model.Tactical.Cursor
                model.Tactical
        | id, Some planning when
            id.StartsWith("planning.battlefield.cell.", StringComparison.Ordinal)
            ->
            planning.Tool = RouteTool
            && planning.SelectedUnit.IsSome
            && UnifiedTacticalWorkspace.canEditAt
                model.Tactical.Cursor
                model.Tactical
        | _ -> true

    let simulatorAvailability =
        if
            command.Id.StartsWith("simulator.pointer.", StringComparison.Ordinal)
        then
            model.Simulator.IsSome
            && model.SimulatorSelectedUnit.IsSome
            && (model.Simulator |> Option.forall (fun simulator -> not simulator.IsRunning))
        else true

    availability
    && transportAvailability
    && planningAvailability
    && simulatorAvailability

[<Emit("window.matchMedia('(prefers-reduced-motion: reduce)').matches")>]
let private prefersReducedMotion: bool = jsNative

[<Emit("/(Mac|iPhone|iPad|iPod)/.test(navigator.platform)")>]
let private usesMetaShortcutPlatform: bool = jsNative

let private shortcutPlatform = if usesMetaShortcutPlatform then MetaPlatform else ControlPlatform
let private validSimulatorFor editor current =
    match MapEditorSimulator.tryHandoff editor with
    | Error _ -> current
    | Ok initial ->
        current
        |> Option.map (fun simulator -> if MapEditorSimulator.isBehindDraft editor simulator then MapEditorSimulator.reconcile editor simulator else simulator)
        |> Option.orElse (Some initial)

/// Browser `key` represents shifted digits as printable symbols (for example
/// Ctrl+Shift+2 arrives as `@`).  Registry gestures use the physical digit, so
/// normalize that one divergent browser representation before dispatch.
[<Emit("($0.code && /^Digit[0-9]$/.test($0.code) ? $0.code.slice(5) : $0.key)")>]
let private registryKeyboardKey (event: KeyboardEvent) : string = jsNative

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
const menu = $0.closest("details.desktop-menu");
if (menu) {
  menu.removeAttribute("open");
  const trigger = menu.querySelector("summary");
  if (trigger && typeof trigger.focus === "function") trigger.focus();
}
""")>]
let private closeDesktopMenuAndRestoreTrigger (target: EventTarget) : unit = jsNative

[<Emit("""
document.querySelectorAll("details.desktop-menu[open]").forEach(menu => menu.removeAttribute("open"));
""")>]
let private closeDesktopMenus () : unit = jsNative

[<Emit("""
const desktopMenu = $0.closest('[role=menu]') || $0.closest('details.desktop-menu')?.querySelector('[role=menu]');
if (!desktopMenu) return;
const desktopMenuItems = Array.from(desktopMenu.querySelectorAll('[role=menuitem]:not([disabled])'));
const desktopMenuCurrent = desktopMenuItems.indexOf(document.activeElement);
const desktopMenuNext = desktopMenuItems[(desktopMenuCurrent + $1 + desktopMenuItems.length) % desktopMenuItems.length] || desktopMenuItems[0];
desktopMenuItems.forEach(item => item.tabIndex = item === desktopMenuNext ? 0 : -1);
if (desktopMenuNext) desktopMenuNext.focus();
""")>]
let private focusNextDesktopMenuItem (target: EventTarget) (delta: int) : unit = jsNative

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

let private scheduleMapAutosave content =
    emitJsStatement
        content
        """
        window.clearTimeout(window.__sirMapAutosaveTimer);
        window.__sirMapAutosaveTimer = window.setTimeout(() => {
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
    let defaultDesktopToolbarCommands =
        [ "workspace.editor"; "workspace.plan"; "workspace.simulate"; "workspace.review"; "timeline.play-toggle"; "input.help" ]
    let desktopToolbarCommands =
        let stored = readDesktopToolbar ()
        if isNull stored then defaultDesktopToolbarCommands
        elif stored.StartsWith("v1:", StringComparison.Ordinal) then
            let payload = stored.Substring("v1:".Length)
            if payload = "" then [] else payload.Split('|') |> Array.toList |> List.distinct
        else
            let requested = stored.Split('|') |> Array.toList
            let known = UnifiedTacticalWorkspace.commandRegistry |> List.map _.Id |> Set.ofList
            let valid = requested |> List.distinct |> List.filter (fun id -> Set.contains id known)
            if List.isEmpty valid then defaultDesktopToolbarCommands else valid
    let editor =
        let initial = MapEditor.initial
        let autosave = readMapAutosave ()
        if isNull autosave then initial
        else MapEditor.update (OfferCrashRecovery autosave) initial
    let editorView = MapEditorWorkspace.initial prefersReducedMotion |> MapEditorWorkspace.update editor.Map (MapEditor.selected editor) FitEditorBoard
    let simulator = validSimulatorFor editor None
    let tacticalParcelEditor = TacticalParcelEditor.fromCanonicalEditor "Tactical parcel preview ready." editor
    let tacticalParcelText = TacticalParcelEditor.exportTacticalParcelDocument editor.TacticalDocument
    let simulatorSelectedUnit =
        editor.SelectedUnit
        |> Option.filter (fun id ->
            simulator
            |> Option.exists (fun value -> Map.containsKey id value.RuntimeMap.Units))
    let tacticalBindings, tacticalBindingDiagnostics =
        let stored = readTacticalBindings ()
        if isNull stored then
            UnifiedTacticalWorkspace.emptyBindingProfile, []
        else
            match
                UnifiedTacticalWorkspace.importBindings
                    UnifiedTacticalWorkspace.commandRegistry
                    stored
            with
            | Ok profile -> profile, []
            | Error diagnostics ->
                UnifiedTacticalWorkspace.emptyBindingProfile,
                [ "Stored binding overrides were malformed and defaults were restored: "
                  + string diagnostics ]
    let tacticalOverlays =
        let stored = readTacticalOverlays ()
        if isNull stored then TacticalSceneProjection.initialOverlayPreferences
        else
            match TacticalSceneProjection.importOverlayPreferences stored with
            | Ok preferences -> preferences
            | Error _ ->
                let restored = TacticalSceneProjection.initialOverlayPreferences
                writeTacticalOverlays (TacticalSceneProjection.exportOverlayPreferences restored); restored
    let tacticalLayout, tacticalLayoutDiagnostics =
        let stored = readTacticalLayout ()
        if isNull stored then
            TacticalWorkspaceLayout.fieldFocus, []
        else
            match TacticalWorkspaceLayout.importProfile stored with
            | Ok profile -> profile, []
            | Error diagnostics ->
                writeTacticalLayout (
                    TacticalWorkspaceLayout.exportProfile
                        TacticalWorkspaceLayout.fieldFocus
                )
                TacticalWorkspaceLayout.fieldFocus,
                [ "Stored tactical layout was malformed and Field Focus was restored: "
                  + string diagnostics ]

    { Shell = Shell.init ()
      Editor = editor
      TacticalParcelEditor = tacticalParcelEditor
      TacticalParcelImportText = tacticalParcelText
      Simulator = simulator
      SimulatorSelectedUnit = simulatorSelectedUnit
      SimulatorControllerSelection = None
      Planning = None
      Tactical = UnifiedTacticalWorkspace.initial 600L
      TacticalBindings = tacticalBindings
      TacticalBindingDrafts = Map.empty
      TacticalBindingImport = ""
      TacticalBindingDiagnostics = tacticalBindingDiagnostics
      TacticalBindingsOpen = false
      TacticalOverlays = tacticalOverlays
      HeldTacticalOverlays = Set.empty
      TacticalLayout = tacticalLayout
      TacticalLayoutDiagnostics = tacticalLayoutDiagnostics
      DesktopToolbarCommands = desktopToolbarCommands
      DesktopToolbarCustomizationOpen = false
      BottomPanelResizeActive = false
      TacticalSelectedUnit = simulatorSelectedUnit
      Workspace = EditorWorkspace
      EditorToolPanel = TerrainTools
      EditorToolPanelVisible = false
      SampleReplayFrames = None
      EditorView = editorView
      HeldInputs = HeldInputSession.empty
      InputHelpExpanded = false
      PendingInterchangeReview = None
      ImportAnnouncement = None
      Battlefield =
        { Battlefield.initial with
            ReducedMotion =
                prefersReducedMotion }
      PreviousFrame = None
      PresentationAlpha = 1.0
      ComparisonBookmarks = []
      ComparisonView = Split
      Live = LiveSession.initial },
    Cmd.ofEffect (fun dispatch -> LiveSession.start (LiveAction >> dispatch))

let rec update msg model =
    match msg with
    | LiveStarted ->
        model, Cmd.ofEffect (fun dispatch -> LiveSession.start (LiveAction >> dispatch))
    | LiveAction action ->
        match action with
        | LiveSession.Bootstrapped response ->
            let live =
                { model.Live with
                    Bootstrap = Some response
                    Snapshot = Some response.Snapshot
                    Status = "connecting" }
            { model with Live = live },
            Cmd.ofEffect (fun dispatch ->
                let connection = LiveSession.connect (LiveAction >> dispatch) response
                dispatch (LiveAction (LiveSession.Connected connection)))
        | LiveSession.Connected connection ->
            { model with Live = { model.Live with Connection = Some connection } }, Cmd.none
        | LiveSession.ConnectionOpened ->
            let next = { model.Live with Status = "connected" }
            { model with Live = next },
            Cmd.ofEffect (fun dispatch -> LiveSession.requestResync (LiveAction >> dispatch) next)
        | LiveSession.ConnectionClosed ->
            { model with Live = { model.Live with Status = "disconnected" } }, Cmd.none
        | LiveSession.BootstrapFailed error ->
            { model with Live = { model.Live with Status = "bootstrap-error:" + error } }, Cmd.none
        | LiveSession.ConnectionFailed error ->
            { model with Live = { model.Live with Status = "connection-error:" + error } }, Cmd.none
        | LiveSession.DecodeFailed error ->
            { model with Live = { model.Live with Status = "decode-error:" + error } }, Cmd.none
        | LiveSession.Received message ->
            match message with
            | RealtimeV1.SnapshotMessage snapshot ->
                { model with Live = { model.Live with Snapshot = Some snapshot; Status = "connected" } }, Cmd.none
            | RealtimeV1.ResyncSnapshotMessage snapshot ->
                { model with
                    Live =
                        { model.Live with
                            Snapshot = Some snapshot
                            ResyncCount = model.Live.ResyncCount + 1
                            Status = "connected" } }, Cmd.none
            | RealtimeV1.AdvanceInputMessage _
            | RealtimeV1.ResyncRequestMessage _ -> model, Cmd.none
    | AdvanceLiveSession ->
        let next = { model.Live with NextSequence = model.Live.NextSequence + 1 }
        { model with Live = next }, Cmd.ofEffect (fun dispatch -> LiveSession.advance (LiveAction >> dispatch) model.Live)
    | DisconnectLiveSession ->
        { model with Live = { model.Live with Status = "disconnecting" } },
        Cmd.ofEffect (fun dispatch -> LiveSession.disconnect (LiveAction >> dispatch) model.Live)
    | ReconnectLiveSession ->
        { model with Live = { model.Live with Status = "reconnecting" } },
        Cmd.ofEffect (fun dispatch -> LiveSession.reconnect (LiveAction >> dispatch) model.Live)
    | FileSelected file ->
        { model with SampleReplayFrames = None },
        Cmd.OfAsync.perform
            (fileBytes 1_048_576)
            file
            ReplayReadCompleted
    | ReplayReadCompleted result ->
        match result with
        | Ok(name, bytes) -> update (ShellMsg(ReplayBytesSelected(name, bytes))) model
        | Error error ->
            let cancelled, effects = Shell.update CancelRequested model.Shell
            { model with
                Shell =
                    { cancelled with
                        Source = Rejected("Replay package", error)
                        Verification = Failed error
                        Mode = NoRun
                        Inspection = None
                        ActiveOperation = None
                        Playback = { cancelled.Playback with CurrentTick = 0; FinalTick = 0; IsPlaying = false }
                        Announcement = error } },
            effectsToCmd effects
    | MapFileSelected file ->
        model,
        Cmd.OfAsync.perform
            (fileText MapEditor.MaximumImportBytes)
            file
            MapReadCompleted
    | MapReadCompleted result ->
        match result with
        | Ok(name, text) -> update (MapTextRead(name, text)) { model with ImportAnnouncement = None }
        | Error error -> { model with ImportAnnouncement = Some error }, Cmd.none
    | MapTextRead(sourceName, text) ->
        let lower = sourceName.ToLowerInvariant()
        if lower.EndsWith(".sir-map") then
            match MapEditor.tryImport text with
            | Error error -> { model with ImportAnnouncement = Some error }, Cmd.none
            | Ok _ ->
                let next, command = update (EditorChanged(LoadMapText text)) model
                { next with ImportAnnouncement = Some("Imported map " + sourceName + ".") }, command
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
            (rasterBytes MapEditorWorkspace.MaximumBackgroundBytes)
            file
            BackgroundReadCompleted
    | BackgroundReadCompleted result ->
        match result with
        | Ok(name, mediaType, bytes) -> update (BackgroundBytesRead(name, mediaType, bytes)) { model with ImportAnnouncement = None }
        | Error error -> { model with ImportAnnouncement = Some error }, Cmd.none
    | BackgroundBytesRead(fileName, mediaType, bytes) ->
        let next, command =
            update (EditorWorkspaceChanged(AttachLocalRaster(fileName, mediaType, bytes))) model
        { next with ImportAnnouncement = Some("Attached background " + fileName + ".") }, command
    | RejectInterchangeReview ->
        { model with PendingInterchangeReview = None },
        Cmd.ofEffect (fun _ -> focusElementAfterRender "persistent-tactical-svg")
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
                    Cmd.ofEffect (fun _ -> focusElementAfterRender "persistent-tactical-svg")
                ]
            | Error _ -> model, Cmd.none
        | None -> model, Cmd.none
    | LoadMapSample sampleId ->
        match ExperienceSamples.tryMap sampleId with
        | None -> model, Cmd.none
        | Some sample ->
            let editor = ExperienceSamples.editorState sample
            let simulator = validSimulatorFor editor None
            let simulatorSelectedUnit =
                editor.SelectedUnit
                |> Option.filter (fun id ->
                    simulator
                    |> Option.exists (fun value -> Map.containsKey id value.RuntimeMap.Units))
            let editorView =
                MapEditorWorkspace.initial model.EditorView.ReducedMotion
                |> MapEditorWorkspace.update
                    editor.Map
                    (MapEditor.selected editor)
                    FitEditorBoard
            { model with
                Editor = editor
                Simulator = simulator
                SimulatorSelectedUnit = simulatorSelectedUnit
                TacticalSelectedUnit = simulatorSelectedUnit
                Workspace = EditorWorkspace
                HeldInputs = HeldInputSession.recover model.HeldInputs
                EditorToolPanel = TerrainTools
                EditorToolPanelVisible = false
                EditorView = editorView
                SampleReplayFrames = None
                Battlefield =
                    Battlefield.reconcile
                        (simulator
                         |> Option.map (MapEditorSimulator.frame simulatorSelectedUnit)
                         |> Option.defaultValue (MapEditor.frame editor))
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
                    Tactical =
                        model.Tactical
                        |> UnifiedTacticalWorkspace.switchModality Simulate
                    HeldInputs = HeldInputSession.recover model.HeldInputs
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
                    Tactical =
                        model.Tactical
                        |> UnifiedTacticalWorkspace.switchModality Review
                    TacticalSelectedUnit =
                        reconcileTacticalSelectedUnit
                            ReplayWorkspace
                            { model with Shell = shell }
                    HeldInputs = HeldInputSession.recover model.HeldInputs
                    SampleReplayFrames = Some frames
                    Battlefield =
                        Battlefield.reconcile frame model.Battlefield
                    PreviousFrame = None
                    PresentationAlpha = 1.0 },
                Cmd.none
    | WorkspaceChanged workspace ->
        let editor =
            if workspace = ReplayWorkspace then
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
                     |> PlanningWorkspace.update (
                         SetPlanningAuthoringTick(int model.Tactical.Cursor)
                     )
                     |> Some),
                    true
            else model.Planning, false

        let tacticalModality =
            match workspace with
            | EditorWorkspace -> Editor
            | PlanningWorkspace -> Plan
            | SimulatorWorkspace -> Simulate
            | ReplayWorkspace -> Review

        let transitionModel =
            { model with
                Editor = editor
                Planning = planning }
        let tacticalSelectedUnit =
            reconcileTacticalSelectedUnit workspace transitionModel

        { model with
            Editor = editor
            Planning = planning
            Simulator = if workspace = SimulatorWorkspace then validSimulatorFor editor model.Simulator else model.Simulator
            TacticalSelectedUnit = tacticalSelectedUnit
            Workspace = workspace
            Tactical =
                model.Tactical
                |> UnifiedTacticalWorkspace.switchModality tacticalModality
            EditorView = editorView
            InputHelpExpanded = false
            SimulatorControllerSelection = None
            HeldInputs = HeldInputSession.recover model.HeldInputs },
        if initializePlanning then
            Cmd.ofEffect (fun dispatch -> dispatch InitializePlanningWorker)
        else Cmd.none
    | ToggleInputHelp focusPanel ->
        let expanded = not model.InputHelpExpanded
        { model with InputHelpExpanded = expanded },
        if focusPanel then
            Cmd.ofEffect (fun _ ->
                focusElementAfterRender (
                    if expanded then "tactical-input-panel" else "tactical-input-toggle"
                ))
        else
            Cmd.none
    | TacticalTimeChanged tick ->
        let cursor = max 0L (min model.Tactical.Horizon tick)
        let planning =
            model.Planning
            |> Option.map (
                PlanningWorkspace.update (
                    SetPlanningAuthoringTick(int (min (int64 Int32.MaxValue) cursor))
                )
            )
        let projected =
            { model with
                Planning = planning
                Tactical =
                    model.Tactical
                    |> UnifiedTacticalWorkspace.scrub cursor }
        match model.Workspace, projected.SampleReplayFrames, projected.Simulator with
        | ReplayWorkspace, _, _ when model.Shell.Playback.FinalTick > 0 ->
            update (ShellMsg(SeekRequested(int32 (min (int64 Int32.MaxValue) cursor)))) projected
        | _, _, Some simulator ->
            let simulator = MapEditorSimulator.seek (int cursor) simulator
            let selected = projected.SimulatorSelectedUnit |> Option.filter (fun id -> Map.containsKey id simulator.RuntimeMap.Units)
            { projected with
                Simulator = Some simulator
                SimulatorSelectedUnit = selected
                Tactical = projected.Tactical |> UnifiedTacticalWorkspace.scrub (int64 simulator.Tick)
                Battlefield = Battlefield.reconcile (MapEditorSimulator.frame selected simulator) projected.Battlefield }, Cmd.none
        | ReplayWorkspace, _, _ ->
            update (ShellMsg(SeekRequested(int32 (min (int64 Int32.MaxValue) cursor)))) projected
        | _ -> projected, Cmd.none
    | TacticalTimeStepped delta ->
        let origin =
            if model.Workspace = ReplayWorkspace && model.Shell.Playback.FinalTick > 0 then
                int64 model.Shell.Playback.CurrentTick
            elif model.Simulator.IsSome then
                model.Simulator
                |> Option.map (fun simulator -> int64 simulator.Tick)
                |> Option.defaultValue model.Tactical.Cursor
            elif model.Workspace = ReplayWorkspace then
                int64 model.Shell.Playback.CurrentTick
            else model.Tactical.Cursor
        update (TacticalTimeChanged(origin + delta)) model
    | TacticalPlaybackToggled ->
        match model.Workspace, model.SampleReplayFrames, model.Simulator with
        | ReplayWorkspace, _, _ when model.Shell.Playback.FinalTick > 0 ->
            let next, effect = update (ShellMsg TogglePlayback) model
            { next with
                Tactical =
                    next.Tactical
                    |> UnifiedTacticalWorkspace.setPlaying next.Shell.Playback.IsPlaying },
            effect
        | _, _, Some _ ->
            let next, effect = update (SimulatorChanged ToggleSimulatorRun) model
            { next with
                Tactical =
                    next.Tactical
                    |> UnifiedTacticalWorkspace.setPlaying (
                        next.Simulator |> Option.exists _.IsRunning
                    ) },
            effect
        | ReplayWorkspace, _, None ->
            let next, effect = update (ShellMsg TogglePlayback) model
            { next with
                Tactical =
                    next.Tactical
                    |> UnifiedTacticalWorkspace.setPlaying next.Shell.Playback.IsPlaying },
            effect
        | _ ->
            { model with
                Tactical =
                    model.Tactical
                    |> UnifiedTacticalWorkspace.setPlaying (not model.Tactical.IsPlaying) },
            Cmd.none
    | TacticalPulse ->
        let tactical =
            match model.Workspace, model.SampleReplayFrames, model.Simulator with
            | ReplayWorkspace, _, _ when model.Shell.Playback.FinalTick > 0 ->
                model.Tactical
                |> UnifiedTacticalWorkspace.scrub (int64 model.Shell.Playback.CurrentTick)
                |> UnifiedTacticalWorkspace.setPlaying model.Shell.Playback.IsPlaying
            | _, _, Some simulator ->
                model.Tactical
                |> UnifiedTacticalWorkspace.scrub (int64 simulator.Tick)
                |> UnifiedTacticalWorkspace.setPlaying simulator.IsRunning
            | ReplayWorkspace, _, None ->
                model.Tactical
                |> UnifiedTacticalWorkspace.scrub (int64 model.Shell.Playback.CurrentTick)
                |> UnifiedTacticalWorkspace.setPlaying model.Shell.Playback.IsPlaying
            | _ -> UnifiedTacticalWorkspace.pulse model.Tactical
        { model with Tactical = tactical }, Cmd.none
    | ToggleTacticalBindings ->
        let opening = not model.TacticalBindingsOpen
        { model with
            TacticalBindingsOpen = opening
            TacticalBindingDiagnostics = [] },
        Cmd.ofEffect (fun _ ->
            focusElementAfterRender (
                if opening then "tactical-binding-dialog"
                elif model.InputHelpExpanded then "tactical-configure-bindings"
                else "tactical-input-toggle"
            ))
    | TacticalBindingDraftChanged(commandId, gesture) ->
        { model with
            TacticalBindingDrafts =
                Map.add commandId gesture model.TacticalBindingDrafts },
        Cmd.none
    | ApplyTacticalBinding(commandId, replaceConflict) ->
        let gesture =
            model.TacticalBindingDrafts
            |> Map.tryFind commandId
            |> Option.bind (fun value ->
                if String.IsNullOrWhiteSpace value then None
                else Some value)
        match
            UnifiedTacticalWorkspace.setBinding
                (activeTacticalRegistry model)
                commandId
                gesture
                replaceConflict
                model.TacticalBindings
        with
        | Ok bindings ->
            writeTacticalBindings (
                UnifiedTacticalWorkspace.exportBindings bindings
            )
            { model with
                TacticalBindings = bindings
                TacticalBindingDiagnostics = [] },
            Cmd.none
        | Error diagnostics ->
            { model with
                TacticalBindingDiagnostics =
                    diagnostics |> List.map string },
            Cmd.none
    | ClearTacticalBinding commandId ->
        let bindings =
            model.TacticalBindings
            |> UnifiedTacticalWorkspace.setBinding
                (activeTacticalRegistry model)
                commandId
                None
                false
            |> Result.defaultValue model.TacticalBindings
        writeTacticalBindings (UnifiedTacticalWorkspace.exportBindings bindings)
        { model with TacticalBindings = bindings }, Cmd.none
    | RestoreTacticalBinding commandId ->
        let bindings =
            model.TacticalBindings
            |> UnifiedTacticalWorkspace.restoreCommand commandId
        writeTacticalBindings (UnifiedTacticalWorkspace.exportBindings bindings)
        { model with TacticalBindings = bindings }, Cmd.none
    | RestoreTacticalModalityBindings ->
        let bindings =
            model.TacticalBindings
            |> UnifiedTacticalWorkspace.restoreModality
                (activeTacticalRegistry model)
                model.Tactical.Modality
        writeTacticalBindings (UnifiedTacticalWorkspace.exportBindings bindings)
        { model with TacticalBindings = bindings }, Cmd.none
    | RestoreAllTacticalBindings ->
        let bindings =
            UnifiedTacticalWorkspace.restoreAll model.TacticalBindings
        writeTacticalBindings (UnifiedTacticalWorkspace.exportBindings bindings)
        { model with TacticalBindings = bindings }, Cmd.none
    | CycleTacticalOverlay commandId ->
        match TacticalSceneProjection.overlayRegistry |> Array.tryFind (fun overlay -> overlay.CommandId = commandId) with
        | None -> model, Cmd.none
        | Some overlay ->
            let current =
                model.TacticalOverlays.Modes
                |> Map.tryFind overlay.Id
                |> Option.defaultValue overlay.DefaultMode
            let candidates =
                [ OverlayOff; SelectionScoped; Persistent ]
                |> List.filter (fun mode -> Set.contains mode overlay.SupportedModes)
            let next =
                candidates
                |> List.tryFindIndex ((=) current)
                |> Option.map (fun index -> candidates[(index + 1) % candidates.Length])
                |> Option.defaultValue (
                    candidates
                    |> List.tryFind (fun mode -> mode <> OverlayOff)
                    |> Option.defaultValue OverlayOff
                )
            let preferences =
                { model.TacticalOverlays with
                    Modes = Map.add overlay.Id next model.TacticalOverlays.Modes }
            writeTacticalOverlays (TacticalSceneProjection.exportOverlayPreferences preferences)
            { model with TacticalOverlays = preferences }, Cmd.none
    | BeginTacticalOverlayHold commandId ->
        let id =
            TacticalSceneProjection.overlayRegistry
            |> Array.tryFind (fun overlay -> overlay.CommandId = commandId && Set.contains InspectHeld overlay.SupportedModes)
            |> Option.map _.Id
        match id with
        | Some value -> { model with HeldTacticalOverlays = Set.add value model.HeldTacticalOverlays }, Cmd.none
        | None -> model, Cmd.none
    | EndTacticalOverlayHold commandId ->
        let id =
            TacticalSceneProjection.overlayRegistry
            |> Array.tryFind (fun overlay -> overlay.CommandId = commandId)
            |> Option.map _.Id
        match id with
        | Some value -> { model with HeldTacticalOverlays = Set.remove value model.HeldTacticalOverlays }, Cmd.none
        | None -> model, Cmd.none
    | TacticalBindingImportChanged value ->
        { model with TacticalBindingImport = value }, Cmd.none
    | ImportTacticalBindings ->
        match
            UnifiedTacticalWorkspace.importBindings
                (activeTacticalRegistry model)
                model.TacticalBindingImport
        with
        | Ok bindings ->
            writeTacticalBindings (
                UnifiedTacticalWorkspace.exportBindings bindings
            )
            { model with
                TacticalBindings = bindings
                TacticalBindingDiagnostics = [] },
            Cmd.none
        | Error diagnostics ->
            { model with
                TacticalBindingDiagnostics =
                    diagnostics |> List.map string },
            Cmd.none
    | ToggleLayoutPanelVisibility panelId ->
        let layout =
            model.TacticalLayout
            |> TacticalWorkspaceLayout.togglePanelVisibility panelId
        writeTacticalLayout (TacticalWorkspaceLayout.exportProfile layout)
        let visible =
            layout.Placements
            |> List.find (fun panel -> panel.PanelId = panelId)
            |> _.Visible
        { model with TacticalLayout = layout; TacticalLayoutDiagnostics = [] },
        Cmd.ofEffect (fun _ ->
            focusElementAfterRender (
                if visible then "layout-panel-" + panelId + "-collapse"
                else "layout-show-" + panelId
            ))
    | ToggleLayoutPanelCollapsed panelId ->
        let layout =
            model.TacticalLayout
            |> TacticalWorkspaceLayout.togglePanelCollapsed panelId
        writeTacticalLayout (TacticalWorkspaceLayout.exportProfile layout)
        { model with TacticalLayout = layout; TacticalLayoutDiagnostics = [] },
        Cmd.ofEffect (fun _ ->
            focusElementAfterRender ("layout-panel-" + panelId + "-collapse"))
    | MoveLayoutPanel(panelId, side) ->
        let layout =
            model.TacticalLayout
            |> TacticalWorkspaceLayout.movePanel panelId side
        writeTacticalLayout (TacticalWorkspaceLayout.exportProfile layout)
        { model with TacticalLayout = layout; TacticalLayoutDiagnostics = [] },
        Cmd.ofEffect (fun _ ->
            focusElementAfterRender ("layout-panel-" + panelId + "-collapse"))
    | ReorderLayoutPanel(panelId, delta) ->
        let layout =
            model.TacticalLayout
            |> TacticalWorkspaceLayout.reorderPanel panelId delta
        writeTacticalLayout (TacticalWorkspaceLayout.exportProfile layout)
        { model with TacticalLayout = layout; TacticalLayoutDiagnostics = [] },
        Cmd.ofEffect (fun _ ->
            focusElementAfterRender ("layout-panel-" + panelId + "-collapse"))
    | ToggleLayoutDrawer side ->
        let layout =
            model.TacticalLayout
            |> TacticalWorkspaceLayout.toggleDrawer side
        writeTacticalLayout (TacticalWorkspaceLayout.exportProfile layout)
        { model with TacticalLayout = layout; TacticalLayoutDiagnostics = [] },
        Cmd.ofEffect (fun _ ->
            focusElementAfterRender (
                if side = Left then "layout-left-drawer-toggle"
                else "layout-right-drawer-toggle"
            ))
    | ToggleLayoutBottomPanelVisibility ->
        let layout =
            model.TacticalLayout
            |> TacticalWorkspaceLayout.toggleBottomPanelVisibility
        writeTacticalLayout (TacticalWorkspaceLayout.exportProfile layout)
        { model with TacticalLayout = layout; TacticalLayoutDiagnostics = [] },
        Cmd.ofEffect (fun _ ->
            focusElementAfterRender "layout-timeline-visibility-toggle")
    | ToggleLayoutBottomPanel ->
        let layout =
            model.TacticalLayout
            |> TacticalWorkspaceLayout.toggleBottomPanel model.Tactical.Modality
        writeTacticalLayout (TacticalWorkspaceLayout.exportProfile layout)
        { model with TacticalLayout = layout; TacticalLayoutDiagnostics = [] },
        Cmd.ofEffect (fun _ ->
            focusElementAfterRender "layout-timeline-toggle")
    | BeginLayoutBottomPanelResize ->
        { model with BottomPanelResizeActive = true }, Cmd.none
    | ResizeLayoutBottomPanel height when model.BottomPanelResizeActive ->
        let layout =
            model.TacticalLayout
            |> TacticalWorkspaceLayout.resizeBottomPanel height
        { model with
            TacticalLayout = layout
            TacticalLayoutDiagnostics = [] },
        Cmd.none
    | ResizeLayoutBottomPanel _ -> model, Cmd.none
    | EndLayoutBottomPanelResize ->
        if model.BottomPanelResizeActive then
            writeTacticalLayout (
                TacticalWorkspaceLayout.exportProfile model.TacticalLayout
            )
        { model with BottomPanelResizeActive = false }, Cmd.none
    | ResizeLayoutBottomPanelKeyboard delta ->
        let layout =
            model.TacticalLayout
            |> TacticalWorkspaceLayout.resizeBottomPanel (
                model.TacticalLayout.BottomPanel.Height + delta
            )
        writeTacticalLayout (TacticalWorkspaceLayout.exportProfile layout)
        { model with
            TacticalLayout = layout
            TacticalLayoutDiagnostics = [] },
        Cmd.ofEffect (fun _ ->
            focusElementAfterRender "tactical-bottom-panel-resize")
    | OpenSupportingPanel panelId ->
        let placement =
            model.TacticalLayout.Placements
            |> List.find (fun placement -> placement.PanelId = panelId)
        let visible =
            if placement.Visible then model.TacticalLayout
            else
                model.TacticalLayout
                |> TacticalWorkspaceLayout.togglePanelVisibility panelId
        let expanded =
            let current =
                visible.Placements
                |> List.find (fun item -> item.PanelId = panelId)
            if current.Collapsed then
                visible |> TacticalWorkspaceLayout.togglePanelCollapsed panelId
            else visible
        let drawerOpen =
            let current =
                expanded.Placements
                |> List.find (fun item -> item.PanelId = panelId)
            let drawer =
                if current.Side = Left then expanded.LeftSidebar
                else expanded.RightSidebar
            if drawer.DrawerOpen then expanded
            else expanded |> TacticalWorkspaceLayout.toggleDrawer current.Side
        writeTacticalLayout (TacticalWorkspaceLayout.exportProfile drawerOpen)
        { model with
            TacticalLayout = drawerOpen
            TacticalLayoutDiagnostics = [] },
        Cmd.ofEffect (fun _ ->
            focusElementAfterRender ("layout-panel-" + panelId + "-body"))
    | ResetTacticalLayout ->
        let layout = TacticalWorkspaceLayout.reset model.TacticalLayout
        writeTacticalLayout (TacticalWorkspaceLayout.exportProfile layout)
        { model with TacticalLayout = layout; TacticalLayoutDiagnostics = [] },
        Cmd.ofEffect (fun _ -> focusElementAfterRender "layout-reset")
    | ToggleDesktopToolbarCustomization ->
        { model with DesktopToolbarCustomizationOpen = not model.DesktopToolbarCustomizationOpen }, Cmd.none
    | AddDesktopToolbarCommand commandId ->
        let commands = if List.contains commandId model.DesktopToolbarCommands then model.DesktopToolbarCommands else model.DesktopToolbarCommands @ [ commandId ]
        writeDesktopToolbar ("v1:" + String.concat "|" commands)
        { model with DesktopToolbarCommands = commands }, Cmd.none
    | RemoveDesktopToolbarCommand commandId ->
        let commands = model.DesktopToolbarCommands |> List.filter ((<>) commandId)
        writeDesktopToolbar ("v1:" + String.concat "|" commands)
        { model with DesktopToolbarCommands = commands }, Cmd.none
    | ReorderDesktopToolbarCommand(commandId, delta) ->
        let commands =
            match model.DesktopToolbarCommands |> List.tryFindIndex ((=) commandId) with
            | Some current when current + delta >= 0 && current + delta < List.length model.DesktopToolbarCommands ->
                model.DesktopToolbarCommands |> List.mapi (fun i value -> if i = current then model.DesktopToolbarCommands[current + delta] elif i = current + delta then commandId else value)
            | _ -> model.DesktopToolbarCommands
        writeDesktopToolbar ("v1:" + String.concat "|" commands)
        { model with DesktopToolbarCommands = commands }, Cmd.none
    | ResetDesktopToolbar ->
        let commands = [ "workspace.editor"; "workspace.plan"; "workspace.simulate"; "workspace.review"; "timeline.play-toggle"; "input.help" ]
        writeDesktopToolbar ("v1:" + String.concat "|" commands)
        { model with DesktopToolbarCommands = commands; DesktopToolbarCustomizationOpen = false }, Cmd.none
    | ApplicationFocusLost ->
        { model with
            InputHelpExpanded = false
            HeldInputs = HeldInputSession.recover model.HeldInputs },
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
                Tactical =
                    model.Tactical
                    |> UnifiedTacticalWorkspace.scrub (int64 simulator.Tick)
                    |> UnifiedTacticalWorkspace.setPlaying simulator.IsRunning
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
                    "Reset runtime-only simulation progress to the current authored baseline?"
            then
                dispatch ResetSimulator)
    | ResetSimulator ->
        match model.Simulator with
        | None -> model, Cmd.none
        | Some current ->
            let simulator = MapEditorSimulator.reset current
            let selected =
                model.SimulatorSelectedUnit
                |> Option.filter (fun unitId ->
                    Map.containsKey unitId simulator.RuntimeMap.Units)
            let frame =
                MapEditorSimulator.frame selected simulator
            { model with
                Simulator = Some simulator
                SimulatorSelectedUnit = selected
                SimulatorControllerSelection = None
                Tactical =
                    model.Tactical
                    |> UnifiedTacticalWorkspace.scrub 0L
                    |> UnifiedTacticalWorkspace.setPlaying false
                Battlefield = Battlefield.reconcile frame model.Battlefield
                PreviousFrame = None
                PresentationAlpha = 1.0 },
            Cmd.none
    | PlanningChanged action ->
        match model.Planning with
        | None -> model, Cmd.none
        | Some planning ->
            let next = PlanningWorkspace.update action planning
            { model with
                Planning = Some next
                Tactical =
                    { model.Tactical with
                        Segments = projectPlanningSegments next } },
            Cmd.none
    | InitializePlanningWorker ->
        match model.Planning with
        | None -> model, Cmd.none
        | Some planning ->
            let correlation, pending =
                PlanningWorkspace.beginRequest
                    InitializePlanningRequest
                    0
                    planning
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
                { pending with
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
            let tick =
                match operation with
                | PreviewPlanningRevision ->
                    int (min (int64 Int32.MaxValue) model.Tactical.Cursor)
                | _ -> planning.CommittedTick |> Option.defaultValue 0
            let requestKind =
                match operation with
                | ValidatePlanningRevision -> ValidatePlanningRequest
                | PreviewPlanningRevision -> PreviewPlanningRequest
                | CommitPlanningRevision -> CommitPlanningRequest
                | _ -> failwith "unreachable planning operation"
            let correlation, pending =
                PlanningWorkspace.beginRequest requestKind tick planning
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
                { pending with
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
            let received = PlanningWorkspace.receive envelope planning
            let projected =
                { model.Tactical with
                    Segments = projectPlanningSegments received }
            let authoredBoundary =
                received.Commands
                |> List.map (fun command -> int64 command.EarliestTick + 1L)
                |> List.fold max 0L
            let tactical =
                match envelope.Response with
                | PlanValidated(Some revision, diagnostics)
                    when revision = received.Revision && diagnostics.Length = 0 ->
                    projected
                    |> UnifiedTacticalWorkspace.acceptThrough authoredBoundary
                | PlanCommitted revision when received.CommittedRevision = Some revision ->
                    let boundary =
                        received.CommittedTick
                        |> Option.map int64
                        |> Option.defaultValue (int64 envelope.CurrentTick)
                    projected
                    |> UnifiedTacticalWorkspace.commitThrough boundary
                | response ->
                    UnifiedTacticalWorkspace.authoritativeProgressBoundary
                        response
                        envelope.CurrentTick
                    |> Option.map (fun boundary ->
                        projected
                        |> UnifiedTacticalWorkspace.commitThrough boundary)
                    |> Option.defaultValue projected
            { model with
                Planning = Some received
                Tactical = tactical },
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
        let panelId =
            match panel with
            | DocumentTools -> "document"
            | _ -> "tools"
        let layout =
            let placement =
                model.TacticalLayout.Placements
                |> List.find (fun placement -> placement.PanelId = panelId)
            let visible =
                if placement.Visible then model.TacticalLayout
                else
                    model.TacticalLayout
                    |> TacticalWorkspaceLayout.togglePanelVisibility panelId
            if placement.Collapsed then
                visible
                |> TacticalWorkspaceLayout.togglePanelCollapsed panelId
            else visible
        writeTacticalLayout (TacticalWorkspaceLayout.exportProfile layout)
        { model with
            EditorToolPanel = panel
            EditorToolPanelVisible = true
            TacticalLayout = layout },
        Cmd.none
    | ToggleEditorToolPanelVisibility ->
        let panelId =
            if model.EditorToolPanel = DocumentTools then "document" else "tools"
        update (ToggleLayoutPanelVisibility panelId) model
    | EditorWorkspaceChanged action ->
        let editorView =
            MapEditorWorkspace.update
                model.Editor.Map
                (MapEditor.selected model.Editor)
                action
                model.EditorView
        match action with
        | ToggleEditorInspector ->
            update (ToggleLayoutPanelVisibility "selection") { model with EditorView = editorView }
        | _ -> { model with EditorView = editorView }, Cmd.none
    | RecallEditorView name ->
        match Map.tryFind name model.Editor.Authoring.SavedViews with
        | None -> model, Cmd.none
        | Some saved ->
            { model with
                EditorView = { model.EditorView with Camera = saved.Camera } },
            Cmd.none
    | EditorChanged action ->
        let editor = MapEditor.update action model.Editor
        let simulator = validSimulatorFor editor model.Simulator
        let simulatorSelected = (if model.Workspace = EditorWorkspace then editor.SelectedUnit else model.SimulatorSelectedUnit) |> Option.filter (fun id -> simulator |> Option.exists (fun value -> Map.containsKey id value.RuntimeMap.Units))
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
            match simulator with
            | Some simulator ->
                Battlefield.reconcile
                    (MapEditorSimulator.frame simulatorSelected simulator)
                    model.Battlefield
            | _ ->
                Battlefield.reconcile (MapEditor.frame editor) model.Battlefield
        { model with
            Editor = editor
            TacticalParcelEditor = TacticalParcelEditor.fromCanonicalEditor model.TacticalParcelEditor.TacticalAnnouncement editor
            TacticalParcelImportText = TacticalParcelEditor.exportTacticalParcelDocument editor.TacticalDocument
            Simulator = simulator
            SimulatorSelectedUnit = simulatorSelected
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
                focusElementAfterRender "persistent-tactical-svg"
            | _ -> ())
    | TacticalParcelChanged action ->
        match action with
        | TacticalParcelEditor.UndoTacticalParcelEdit
        | TacticalParcelEditor.RedoTacticalParcelEdit ->
            let editorAction = if action = TacticalParcelEditor.UndoTacticalParcelEdit then UndoEditorCommand else RedoEditorCommand
            let editor = MapEditor.update editorAction model.Editor
            let announcement = if action = TacticalParcelEditor.UndoTacticalParcelEdit then "Tactical parcel edit undone." else "Tactical parcel edit redone."
            let tactical = TacticalParcelEditor.fromCanonicalEditor announcement editor
            { model with Editor = editor; TacticalParcelEditor = tactical; TacticalParcelImportText = TacticalParcelEditor.exportTacticalParcelDocument editor.TacticalDocument }, Cmd.none
        | _ ->
            let tactical, text = TacticalParcelEditor.updateWithExport action model.TacticalParcelEditor
            let editor = MapEditor.update (ReplaceTacticalParcelDocument(tactical.TacticalDocument, tactical.TacticalSeed)) model.Editor
            { model with Editor = editor; TacticalParcelEditor = TacticalParcelEditor.fromCanonicalEditor tactical.TacticalAnnouncement editor; TacticalParcelImportText = text }, Cmd.none
    | TacticalParcelImportTextChanged text ->
        { model with TacticalParcelImportText = text }, Cmd.none
    | ImportTacticalParcelDocument ->
        match TacticalParcelEditor.tryImportTacticalParcelDocument model.TacticalParcelImportText with
        | Ok document -> update (TacticalParcelChanged(TacticalParcelEditor.ReplaceTacticalDocument document)) model
        | Error message ->
            { model with
                TacticalParcelEditor =
                    { model.TacticalParcelEditor with
                        TacticalAnnouncement = message } },
            Cmd.none
    | ExportTacticalParcelDocument ->
        model, Cmd.ofEffect (fun _ -> TacticalEnvironmentView.downloadDocument model.TacticalParcelImportText)
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
                | _ -> None, 1.0

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
    | InvokeTacticalCommand commandId ->
        activeTacticalRegistry model
        |> List.tryFind (fun command ->
            command.Id = commandId
            && Set.contains model.Tactical.Modality command.Modalities
            && tacticalCommandAvailable model command)
        |> Option.map (fun _ ->
            let modalCommand =
                match model.Workspace with
                | EditorWorkspace ->
                    let facts =
                        { Editor = model.Editor
                          ActiveDomain =
                            match model.EditorToolPanel with
                            | TerrainTools -> TerrainDomain
                            | UnitTools -> UnitDomain
                            | EdgeTools -> EdgeDomain
                            | ZoneTools -> RegionDomain
                            | TacticalEnvironmentTools -> DocumentDomain
                            | DocumentTools -> DocumentDomain
                          PanHeld = editorPanHeld model
                          InputHelpExpanded = model.InputHelpExpanded }
                    ModalInput.tryAvailableCommandById
                        (ModalInput.deriveEditorContexts facts)
                        commandId
                        (ModalInput.editorCatalog facts)
                | SimulatorWorkspace ->
                    let facts =
                        { SimulatorHandoffPresent = model.Simulator.IsSome
                          SimulatorIsRunning =
                            model.Simulator |> Option.exists _.IsRunning
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
                    ModalInput.tryAvailableCommandById
                        (ModalInput.deriveSimulatorContexts facts)
                        commandId
                        (ModalInput.simulatorCatalog
                            model.SimulatorSelectedUnit
                            model.Simulator
                            model.SimulatorControllerSelection)
                | _ -> None
            match modalCommand with
            | Some command -> update (ExecuteModalCommand command) model
            | None -> update (ExecuteTacticalCommand commandId) model)
        |> Option.defaultValue (model, Cmd.none)
    | InvokeTacticalValueCommand(commandId, value) ->
        activeTacticalRegistry model
        |> List.tryFind (fun command ->
            command.Id = commandId
            && Set.contains model.Tactical.Modality command.Modalities
            && tacticalCommandAvailable model command)
        |> Option.map (fun _ ->
            match commandId with
            | "simulator.pointer.script.set" ->
                update (SimulatorChanged(SetSimulatorScript value)) model
            | _ -> model, Cmd.none)
        |> Option.defaultValue (model, Cmd.none)
    | ExecuteTacticalCommand commandId ->
        match commandId with
        | id when id.StartsWith("view.overlay.", StringComparison.Ordinal) ->
            update (CycleTacticalOverlay id) model
        | "workspace.editor" -> update (WorkspaceChanged EditorWorkspace) model
        | "workspace.plan" -> update (WorkspaceChanged PlanningWorkspace) model
        | "workspace.simulate" -> update (WorkspaceChanged SimulatorWorkspace) model
        | "workspace.review" -> update (WorkspaceChanged ReplayWorkspace) model
        | "environment.editor.open" -> update (EditorToolPanelChanged TacticalEnvironmentTools) model
        | "panel.data" -> update (OpenSupportingPanel "data") model
        | "timeline.play-toggle" -> update TacticalPlaybackToggled model
        | "timeline.step-back" -> update (TacticalTimeStepped -1L) model
        | "timeline.step-forward" -> update (TacticalTimeStepped 1L) model
        | "timeline.home" -> update (TacticalTimeChanged 0L) model
        | "timeline.end" -> update (TacticalTimeChanged model.Tactical.Horizon) model
        | "scene.camera.zoom-out" ->
            update
                (EditorWorkspaceChanged(
                    ZoomEditorAt(
                        model.EditorView.ViewportWidth / 2.0,
                        model.EditorView.ViewportHeight / 2.0,
                        0.8
                    )
                ))
                model
        | "scene.camera.zoom-in" ->
            update
                (EditorWorkspaceChanged(
                    ZoomEditorAt(
                        model.EditorView.ViewportWidth / 2.0,
                        model.EditorView.ViewportHeight / 2.0,
                        1.25
                    )
                ))
                model
        | "scene.camera.fit" ->
            update (EditorWorkspaceChanged FitEditorBoard) model
        | "timeline.move-command" ->
            update
                (PlanningChanged(
                    MoveSelectedPlanningCommandTo(int model.Tactical.Cursor)
                ))
                model
        | "timeline.remove-command" ->
            update (PlanningChanged RemoveSelectedPlanningCommand) model
        | "planning.undo" -> update (PlanningChanged UndoPlanning) model
        | "planning.redo" -> update (PlanningChanged RedoPlanning) model
        | "planning.route" ->
            update (PlanningChanged(ChoosePlanningTool RouteTool)) model
        | "planning.facing" ->
            update (PlanningChanged(ChoosePlanningTool FacingTool)) model
        | "planning.attention" ->
            update (PlanningChanged(ChoosePlanningTool AttentionTool)) model
        | "planning.stance" ->
            update (PlanningChanged(ChoosePlanningTool StanceTool)) model
        | "planning.hold" ->
            update (PlanningChanged(ChoosePlanningTool HoldTool)) model
        | "planning.engagement" ->
            update (PlanningChanged(ChoosePlanningTool EngagementTool)) model
        | "planning.synchronization" ->
            update
                (PlanningChanged(ChoosePlanningTool SynchronizationTool))
                model
        | "planning.validate" -> update ValidatePlanningRevision model
        | "planning.preview" -> update PreviewPlanningRevision model
        | "planning.commit" -> update CommitPlanningRevision model
        | id when
            id.StartsWith(
                "editor.scene.select.unit.",
                StringComparison.Ordinal
            )
            ->
            match
                Int32.TryParse(
                    id.Substring("editor.scene.select.unit.".Length)
                )
            with
            | true, unitId ->
                let selected, command =
                    update (EditorChanged(SelectEditorUnit(Some unitId))) model
                { selected with TacticalSelectedUnit = Some unitId }, command
            | _ -> model, Cmd.none
        | id when
            id.StartsWith("editor.scene.cell.", StringComparison.Ordinal)
            ->
            match id.Substring("editor.scene.cell.".Length).Split('.') with
            | [| column; row |] ->
                match Int32.TryParse column, Int32.TryParse row with
                | (true, columnValue), (true, rowValue) ->
                    update
                        (EditorChanged(ActivateCell(columnValue, rowValue)))
                        model
                | _ -> model, Cmd.none
            | _ -> model, Cmd.none
        | id when
            id.StartsWith(
                "simulator.scene.select.unit.",
                StringComparison.Ordinal
            )
            ->
            match
                Int32.TryParse(
                    id.Substring("simulator.scene.select.unit.".Length)
                )
            with
            | true, unitId ->
                let selected, command =
                    update (SimulatorUnitSelectionChanged(Some unitId)) model
                { selected with TacticalSelectedUnit = Some unitId }, command
            | _ -> model, Cmd.none
        | id when
            id.StartsWith(
                "review.scene.select.unit.",
                StringComparison.Ordinal
            )
            ->
            match
                Int32.TryParse(
                    id.Substring("review.scene.select.unit.".Length)
                )
            with
            | true, unitId ->
                let selected, command =
                    update (ShellMsg(UnitSelected(Some unitId))) model
                { selected with TacticalSelectedUnit = Some unitId }, command
            | _ -> model, Cmd.none
        | id when
            id.StartsWith(
                "review.scene.select.event.",
                StringComparison.Ordinal
            )
            ->
            match
                Int32.TryParse(
                    id.Substring("review.scene.select.event.".Length)
                )
            with
            | true, eventId ->
                update (ShellMsg(EventSelected(Some eventId))) model
            | _ -> model, Cmd.none
        | "planning.issue.previous"
        | "planning.issue.next" ->
            match model.Planning with
            | Some planning when planning.Issues.Length > 0 ->
                let current =
                    planning.FocusedIssue
                    |> Option.defaultValue (
                        if commandId = "planning.issue.previous" then 0 else -1
                    )
                let delta = if commandId = "planning.issue.previous" then -1 else 1
                update
                    (PlanningChanged(
                        FocusPlanningIssue(
                            (current + delta + planning.Issues.Length) % planning.Issues.Length
                        )
                    ))
                    model
            | _ -> model, Cmd.none
        | id when id.StartsWith("planning.roster.select.", StringComparison.Ordinal) ->
            match Int32.TryParse(id.Substring("planning.roster.select.".Length)) with
            | true, unitId ->
                let selected, command =
                    update (PlanningChanged(SelectPlanningUnit unitId)) model
                { selected with TacticalSelectedUnit = Some unitId }, command
            | _ -> model, Cmd.none
        | id when id.StartsWith("planning.timeline.select.", StringComparison.Ordinal) ->
            update
                (PlanningChanged(
                    SelectPlanningCommand(
                        id.Substring("planning.timeline.select.".Length)
                    )
                ))
                model
        | id when id.StartsWith("planning.issue.focus.", StringComparison.Ordinal) ->
            match Int32.TryParse(id.Substring("planning.issue.focus.".Length)) with
            | true, index -> update (PlanningChanged(FocusPlanningIssue index)) model
            | _ -> model, Cmd.none
        | id when
            id.StartsWith("planning.battlefield.cell.", StringComparison.Ordinal)
            ->
            match
                id.Substring("planning.battlefield.cell.".Length).Split('.')
            with
            | [| column; row |] ->
                match Int32.TryParse column, Int32.TryParse row with
                | (true, columnValue), (true, rowValue) ->
                    update
                        (PlanningChanged(
                            AddRouteWaypoint(columnValue, rowValue)
                        ))
                        model
                | _ -> model, Cmd.none
            | _ -> model, Cmd.none
        | id when id.StartsWith("planning.inspector.", StringComparison.Ordinal) ->
            match model.Planning with
            | Some planning ->
                let selected =
                    planning.SelectedUnit
                    |> Option.bind (fun selectedId ->
                        planning.Roster
                        |> Array.tryFind (fun unit -> unit.UnitId = selectedId))
                let direction slug =
                    match slug with
                    | "north" -> Some North
                    | "north-east" -> Some NorthEast
                    | "east" -> Some East
                    | "south-east" -> Some SouthEast
                    | "south" -> Some South
                    | "south-west" -> Some SouthWest
                    | "west" -> Some West
                    | "north-west" -> Some NorthWest
                    | _ -> None
                let action =
                    match id, selected with
                    | "planning.inspector.waypoint.west", Some unit ->
                        Some(AddRouteWaypoint(unit.Column - 1, unit.Row))
                    | "planning.inspector.waypoint.north", Some unit ->
                        Some(AddRouteWaypoint(unit.Column, unit.Row - 1))
                    | "planning.inspector.waypoint.south", Some unit ->
                        Some(AddRouteWaypoint(unit.Column, unit.Row + 1))
                    | "planning.inspector.waypoint.east", Some unit ->
                        Some(AddRouteWaypoint(unit.Column + 1, unit.Row))
                    | "planning.inspector.stance.standing", _ ->
                        Some(SetPlanningStance "standing")
                    | "planning.inspector.stance.crouched", _ ->
                        Some(SetPlanningStance "crouched")
                    | "planning.inspector.stance.prone", _ ->
                        Some(SetPlanningStance "prone")
                    | "planning.inspector.hold", _ -> Some AddPlanningHold
                    | "planning.inspector.synchronization", _ ->
                        Some(
                            AddPlanningSynchronization(
                                "sync-" + string planning.NextCommand,
                                600
                            )
                        )
                    | "planning.inspector.engagement", Some unit ->
                        match
                            planning.Roster
                            |> Array.tryFind (fun target -> target.UnitId <> unit.UnitId),
                            unit.CapabilityIds |> Array.tryHead
                        with
                        | Some target, Some capability ->
                            Some(AddPlanningEngagement(target.UnitId, capability))
                        | _ -> None
                    | value, _ when value.StartsWith("planning.inspector.facing.") ->
                        value.Substring("planning.inspector.facing.".Length)
                        |> direction
                        |> Option.map SetPlanningFacing
                    | value, _ when value.StartsWith("planning.inspector.attention.") ->
                        value.Substring("planning.inspector.attention.".Length)
                        |> direction
                        |> Option.map SetPlanningAttention
                    | _ -> None
                action
                |> Option.map (fun planningAction ->
                    update (PlanningChanged planningAction) model)
                |> Option.defaultValue (model, Cmd.none)
            | None -> model, Cmd.none
        | id when
            id.StartsWith("simulator.pointer.controller.", StringComparison.Ordinal)
            ->
            match id.Substring("simulator.pointer.controller.".Length) with
            | "manual" ->
                update (SimulatorChanged(SetSimulatorController Manual)) model
            | "scripted" ->
                update (SimulatorChanged(SetSimulatorController Scripted)) model
            | "general" ->
                update (SimulatorChanged(SetSimulatorController General)) model
            | _ -> model, Cmd.none
        | id when
            id.StartsWith("simulator.pointer.movement.", StringComparison.Ordinal)
            ->
            let direction =
                match id.Substring("simulator.pointer.movement.".Length) with
                | "north-west" -> Some NorthWest
                | "north" -> Some North
                | "north-east" -> Some NorthEast
                | "west" -> Some West
                | "east" -> Some East
                | "south-west" -> Some SouthWest
                | "south" -> Some South
                | "south-east" -> Some SouthEast
                | _ -> None
            direction
            |> Option.map (fun value ->
                update (SimulatorChanged(MoveSimulatorUnit value)) model)
            |> Option.defaultValue (model, Cmd.none)
        | "review.previous-event" -> update (ShellMsg PreviousEvent) model
        | "review.next-event" -> update (ShellMsg NextEvent) model
        | "review.cancel" -> update (ShellMsg CancelRequested) model
        | "input.help"
        | "input.help.close" -> update (ToggleInputHelp true) model
        | "input.bindings" -> update ToggleTacticalBindings model
        | _ -> model, Cmd.none
    | ExecuteModalCommand command ->
        match command with
        | EditorCommand action -> update (EditorChanged action) model
        | EditorWorkspaceCommand action ->
            update (EditorWorkspaceChanged action) model
        | ChooseEditorDomain domain ->
            let panel, tool =
                match domain with
                | TerrainDomain ->
                    TerrainTools,
                    Some(Terrain model.Editor.LastTerrainPaintTool)
                | UnitDomain -> UnitTools, Some UnitBrowse
                | EdgeDomain -> EdgeTools, None
                | RegionDomain -> ZoneTools, None
                | DocumentDomain -> DocumentTools, None
            let next, panelEffect =
                update (EditorToolPanelChanged panel) model
            match tool with
            | Some value ->
                let changed, toolEffect =
                    update (EditorChanged(ChooseTool value)) next
                changed, Cmd.batch [ panelEffect; toolEffect ]
            | None -> next, panelEffect
        | ToggleEditorCommandPanel ->
            update ToggleEditorToolPanelVisibility model
        | ChooseSimulatorPanel panel ->
            let panelId =
                match panel with
                | ControllerPanel -> "selection"
                | EventPanel -> "validation"
                | SimulatorSamplePanel -> "samples"
            let placement =
                model.TacticalLayout.Placements
                |> List.find (fun placement -> placement.PanelId = panelId)
            let visible =
                if placement.Visible then model.TacticalLayout
                else
                    model.TacticalLayout
                    |> TacticalWorkspaceLayout.togglePanelVisibility panelId
            let layout =
                if placement.Collapsed then
                    visible
                    |> TacticalWorkspaceLayout.togglePanelCollapsed panelId
                else visible
            writeTacticalLayout (TacticalWorkspaceLayout.exportProfile layout)
            { model with TacticalLayout = layout },
            Cmd.none
        | ToggleSimulatorCommandPanel ->
            update (ToggleLayoutPanelVisibility "selection") model
        | SimulatorCommand action -> update (SimulatorChanged action) model
        | TraverseSimulatorUnit delta ->
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
        | ModalCommand.BeginSimulatorControllerSelection ->
            update BeginSimulatorControllerSelection model
        | ModalCommand.ChooseSimulatorController controller ->
            update (ChooseSimulatorController controller) model
        | CommitSimulatorController ->
            update CommitSimulatorControllerSelection model
        | CancelSimulatorController ->
            update CancelSimulatorControllerSelection model
        | RequestSimulatorSandboxReset -> update RequestSimulatorReset model
        | SetEditorPanHeld _ ->
            { model with
                HeldInputs = HeldInputSession.apply command model.HeldInputs },
            Cmd.none
        | FocusUnitPresetSearch ->
            model,
            Cmd.ofEffect (fun _ ->
                focusElementAfterRender "editor-unit-preset-search")
        | EditorDocumentCommand ExportMapDocument -> update ExportMap model
        | EditorDocumentCommand ExportRepositoryDesignBundle ->
            update ExportDesignBundle model
        | EditorDocumentCommand OpenMapImport ->
            model,
            Cmd.ofEffect (fun _ ->
                openFilePickerAfterRender "editor-map-import")
        | EditorDocumentCommand(FocusDocumentControl target) ->
            let id =
                match target with
                | MapImportControl -> "editor-map-import"
                | LayerStateControls -> "editor-layer-controls"
                | LocalBackgroundControls -> "editor-background-file"
                | MapDimensionControls -> "map-width"
                | SavedViewControls -> "editor-saved-view-controls"
            model, Cmd.ofEffect (fun _ -> focusElementAfterRender id)
        | ModalCommand.ToggleInputHelp ->
            update (ToggleInputHelp true) model
    | KeyPressed(key, controlOrMeta, shift, alt, repeat) ->
        let gesture =
            { Key = NormalizedKey.create key None
              Modifiers =
                { ControlOrMeta = controlOrMeta
                  Shift = shift
                  Alt = alt }
              Phase = KeyDown }

        let resolveEditor () =
            let activeDomain =
                match model.EditorToolPanel with
                | TerrainTools -> TerrainDomain
                | UnitTools -> UnitDomain
                | EdgeTools -> EdgeDomain
                | ZoneTools -> RegionDomain
                | TacticalEnvironmentTools -> DocumentDomain
                | DocumentTools -> DocumentDomain
            let facts =
                { Editor = model.Editor
                  ActiveDomain = activeDomain
                  PanHeld = editorPanHeld model
                  InputHelpExpanded = model.InputHelpExpanded }
            ModalInput.resolve
                (ModalInput.deriveEditorContexts facts)
                gesture
                repeat
                (ModalInput.editorCatalog facts
                 |> UnifiedTacticalWorkspace.adaptModalCatalog model.TacticalBindings)

        let resolveSimulator () =
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
                  SimulatorControllerSelection = model.SimulatorControllerSelection
                  SimulatorRevisionIsStale =
                    model.Simulator
                    |> Option.map (MapEditorSimulator.isBehindDraft model.Editor)
                    |> Option.defaultValue false
                  InputHelpExpanded = model.InputHelpExpanded }
            ModalInput.resolve
                (ModalInput.deriveSimulatorContexts facts)
                gesture
                repeat
                (ModalInput.simulatorCatalog
                    model.SimulatorSelectedUnit
                    model.Simulator
                    model.SimulatorControllerSelection
                 |> UnifiedTacticalWorkspace.adaptModalCatalog model.TacticalBindings)

        let applyCommand targetModel command =
            update (ExecuteModalCommand command) targetModel

        let applyResolution resolution =
            match resolution with
            | Resolved input ->
                if
                    model.Workspace = EditorWorkspace
                    && NormalizedKey.value gesture.Key = "Escape"
                then
                    let cleared, pointerEffect =
                        update
                            (EditorWorkspaceChanged CancelEditorPointers)
                            model
                    let result, effect =
                        applyCommand cleared input.Command
                    result, Cmd.batch [ pointerEffect; effect ]
                else
                    applyCommand model input.Command
            | NoMatch
            | NoAvailableMatch _ ->
                model, Cmd.none

        let producedGesture =
            let producedKey =
                match key with
                | " " -> "Space"
                | value when value.Length = 1 ->
                    value.ToUpperInvariant()
                | value -> value
            [ if controlOrMeta then "Ctrl"
              if alt then "Alt"
              if shift && key <> "?" then "Shift"
              producedKey ]
            |> String.concat "+"
            |> fun value -> value.ToUpperInvariant()

        let tacticalCommand: TacticalCommandDefinition option =
            activeTacticalRegistry model
            |> List.filter (fun command ->
                Set.contains model.Tactical.Modality command.Modalities
                && tacticalCommandAvailable model command
                && not (command.Id.StartsWith("editor.", StringComparison.Ordinal))
                && not (command.Id.StartsWith("simulator.", StringComparison.Ordinal))
                // Space remains a held pan gesture in Editor and the simulator
                // run/pause gesture in Simulate. Those modality catalogs retain
                // precedence until they are represented as registry commands.
                && not (
                    command.Id = "timeline.play-toggle"
                    && Set.contains
                        model.Tactical.Modality
                        (Set.ofList [ Editor; Simulate ])
                ))
            |> List.sortByDescending _.Precedence
            |> List.tryFind (fun command ->
                UnifiedTacticalWorkspace.effectiveGesture
                    model.TacticalBindings
                    command
                |> Option.exists (fun binding ->
                    ClientModuleBoundaries.canonicalGesture binding = producedGesture))

        let applyTacticalCommand (command: TacticalCommandDefinition) =
            update (InvokeTacticalCommand command.Id) model

        match tacticalCommand with
        | Some command -> applyTacticalCommand command
        | None ->
            match model.Workspace with
            | EditorWorkspace -> resolveEditor () |> applyResolution
            | SimulatorWorkspace -> resolveSimulator () |> applyResolution
            | PlanningWorkspace
            | ReplayWorkspace ->
                model, Cmd.none
    | KeyReleased key ->
        if model.Workspace = EditorWorkspace && editorPanHeld model then
            let facts =
                { Editor = model.Editor
                  ActiveDomain =
                    match model.EditorToolPanel with
                    | TerrainTools -> TerrainDomain
                    | UnitTools -> UnitDomain
                    | EdgeTools -> EdgeDomain
                    | ZoneTools -> RegionDomain
                    | TacticalEnvironmentTools -> DocumentDomain
                    | DocumentTools -> DocumentDomain
                  PanHeld = true
                  InputHelpExpanded = model.InputHelpExpanded }
            let gesture =
                { Key = NormalizedKey.create key None
                  Modifiers = KeyModifiers.none
                  Phase = KeyUp }
            match
                ModalInput.resolve
                    (ModalInput.deriveEditorContexts facts)
                    gesture
                    false
                    (ModalInput.editorCatalog facts)
            with
            | Resolved input ->
                { model with
                    HeldInputs =
                        HeldInputSession.apply input.Command model.HeldInputs },
                Cmd.none
            | NoMatch
            | NoAvailableMatch _ ->
                model, Cmd.none
        else
            model, Cmd.none
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
        let modalResolution phase key controlOrMeta shift alt repeat =
            let gesture =
                { Key = NormalizedKey.create key None
                  Modifiers =
                    { ControlOrMeta = controlOrMeta
                      Shift = shift
                      Alt = alt }
                  Phase = phase }
            match model.Workspace with
            | EditorWorkspace ->
                let facts =
                    { Editor = model.Editor
                      ActiveDomain =
                        match model.EditorToolPanel with
                        | TerrainTools -> TerrainDomain
                        | UnitTools -> UnitDomain
                        | EdgeTools -> EdgeDomain
                        | ZoneTools -> RegionDomain
                        | TacticalEnvironmentTools -> DocumentDomain
                        | DocumentTools -> DocumentDomain
                      PanHeld = editorPanHeld model
                      InputHelpExpanded = model.InputHelpExpanded }
                ModalInput.resolve
                    (ModalInput.deriveEditorContexts facts)
                    gesture
                    repeat
                    (ModalInput.editorCatalog facts
                     |> UnifiedTacticalWorkspace.adaptModalCatalog model.TacticalBindings)
                |> Some
            | SimulatorWorkspace when phase = KeyDown ->
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
                ModalInput.resolve
                    (ModalInput.deriveSimulatorContexts facts)
                    gesture
                    repeat
                    (ModalInput.simulatorCatalog
                        model.SimulatorSelectedUnit
                        model.Simulator
                        model.SimulatorControllerSelection
                     |> UnifiedTacticalWorkspace.adaptModalCatalog model.TacticalBindings)
                |> Some
            | _ -> None

        let isCatalogGesture = function
            | Some(Resolved _) -> true
            | Some(NoAvailableMatch _)
            | Some NoMatch
            | None -> false

        let isRegistryGesture key controlOrMeta shift alt =
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
                    |> Option.exists (fun value ->
                        ClientModuleBoundaries.canonicalGesture value = produced)))

        let downHandler =
            fun (event: Event) ->
                let keyboardEvent: KeyboardEvent = unbox event
                let key = registryKeyboardKey keyboardEvent
                if TacticalEnvironmentView.acceptsGlobalKeyboardTarget keyboardEvent.target (keyboardEvent.ctrlKey || keyboardEvent.metaKey) then
                    let controlOrMeta =
                        keyboardEvent.ctrlKey || keyboardEvent.metaKey
                    if
                        (modalResolution
                            KeyDown
                            key
                            controlOrMeta
                            keyboardEvent.shiftKey
                            keyboardEvent.altKey
                            keyboardEvent.repeat
                         |> isCatalogGesture)
                        || isRegistryGesture
                            key
                            controlOrMeta
                            keyboardEvent.shiftKey
                            keyboardEvent.altKey
                    then
                        keyboardEvent.preventDefault ()
                    dispatch (
                        KeyPressed(
                            key,
                            controlOrMeta,
                            keyboardEvent.shiftKey,
                            keyboardEvent.altKey,
                            keyboardEvent.repeat
                        )
                    )
        let upHandler =
            fun (event: Event) ->
                let keyboardEvent: KeyboardEvent = unbox event
                if
                    TacticalEnvironmentView.acceptsGlobalKeyboardTarget keyboardEvent.target false
                then
                    if
                        modalResolution
                            KeyUp
                            keyboardEvent.key
                            false
                            false
                            false
                            false
                        |> isCatalogGesture
                    then
                        keyboardEvent.preventDefault ()
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
                (fun () ->
                    dispatch PlaybackPulse
                    dispatch TacticalPulse),
                interval
            )

        { new IDisposable with
            member _.Dispose() = window.clearInterval identifier }

    let editorTimer dispatch =
        let identifier =
            window.setInterval (
                (fun () ->
                    dispatch EditorPulse
                    dispatch TacticalPulse),
                50
            )

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
      // The resolver closes over the active command registry and binding
      // profile. Re-subscribe when either can change so keyboard activation
      // cannot keep dispatching a stale render's command map.
      [ "keyboard"
        string model.Workspace
        string model.Tactical.Modality
        string model.InputHelpExpanded
        string model.TacticalBindings ], keyboard
      if model.Workspace = EditorWorkspace then
          [ "editor-resize" ], editorResize
      if
          model.Shell.Playback.IsPlaying
          || (model.Tactical.IsPlaying
              && model.Workspace <> SimulatorWorkspace)
      then
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

/// The render-time policy for command controls.  A control is either supplied
/// with registry-derived `aria-keyshortcuts` by its family adapter, or is
/// explicitly disclosed as unassigned.  This keeps unbound/dynamic controls
/// out of a silent third state without mutating the DOM after render.
let private commandButton properties =
    let hasRegistryBinding =
        properties
        |> List.exists (fun property ->
            let name, _ = unbox<string * obj> property
            name = "aria-keyshortcuts")
    Html.button (
        if hasRegistryBinding then properties
        else
            properties
            @ [ prop.custom ("data-binding-state", "unassigned")
                prop.custom ("aria-description", "Shortcut: Unassigned") ])

let private button
    (text: string)
    (label: string)
    (disabled: bool)
    (onClick: MouseEvent -> unit)
    =
    commandButton [
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
                        (fun _ -> dispatch (InvokeTacticalCommand "timeline.play-toggle"))
                    button
                        "Previous event"
                        "Go to previous disclosed replay event"
                        (unavailable || not hasEvents)
                        (fun _ -> dispatch (InvokeTacticalCommand "review.previous-event"))
                    button
                        "Back"
                        "Step backward one committed replay tick"
                        (unavailable || atStart)
                        (fun _ -> dispatch (InvokeTacticalCommand "timeline.step-back"))
                    button
                        "Step"
                        "Advance one replay step"
                        (unavailable || atEnd)
                        (fun _ -> dispatch (InvokeTacticalCommand "timeline.step-forward"))
                    button
                        "Next event"
                        "Go to next disclosed replay event"
                        (unavailable || not hasEvents)
                        (fun _ -> dispatch (InvokeTacticalCommand "review.next-event"))
                    button
                        "Cancel"
                        "Cancel current replay operation"
                        (Option.isNone model.ActiveOperation)
                        (fun _ -> dispatch (InvokeTacticalCommand "review.cancel"))
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
                    Svg.path [ svg.d path; svg.fill palette.Text ]
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
                        commandButton [
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
                            commandButton [
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
        prop.ariaLabel "Review worker status"
        prop.text (
            text
            + " · protocol "
            + string WorkerProtocol.CurrentVersion
            + " · batch size "
            + string WorkerProtocol.BatchSize
        )
    ]

let private reviewRosterPanel (model: SIR.Client.Model) dispatch =
    let units =
        model.Inspection
        |> Option.map _.Units
        |> Option.defaultValue []

    Html.section [
        prop.ariaLabel "Review disclosed roster"
        prop.children [
            Html.p "Only units disclosed by the committed replay frame are listed."
            if List.isEmpty units then
                Html.p "No units are disclosed at this tick."
            else
                Html.ul [
                    for unit in units do
                        Html.li [
                            commandButton [
                                prop.type'.button
                                prop.ariaLabel ("Inspect disclosed unit " + string unit.Id)
                                prop.text (
                                    unit.Side
                                    + " "
                                    + string unit.Id
                                    + " · health "
                                    + string unit.Health
                                    + "/"
                                    + string unit.HealthMaximum
                                )
                                prop.onClick (fun _ ->
                                    dispatch (ShellMsg(UnitSelected(Some unit.Id))))
                            ]
                        ]
                ]
        ]
    ]

let private reviewSelectionPanel (model: SIR.Client.Model) dispatch =
    let inspection = model.Inspection
    let selectedUnit =
        model.Selection.Unit
        |> Option.bind (fun id ->
            inspection
            |> Option.bind (fun value ->
                value.Units |> List.tryFind (fun unit -> unit.Id = id)))
    let selectedEvent =
        model.Selection.Event
        |> Option.bind (fun id ->
            inspection
            |> Option.bind (fun value ->
                value.Events |> List.tryFind (fun event -> event.Id = id)))
    let events =
        inspection |> Option.map _.Events |> Option.defaultValue []

    Html.section [
        prop.ariaLabel "Review event inspection"
        prop.children [
            Html.p "Inspection is a read-only view of the accepted committed frame."
            Html.dl [
                Html.dt "Selected unit"
                Html.dd (
                    selectedUnit
                    |> Option.map (fun unit ->
                        unit.Side
                        + " " + string unit.Id
                        + " at " + string unit.Column + "," + string unit.Row)
                    |> Option.defaultValue "None"
                )
                Html.dt "Selected event"
                Html.dd (
                    selectedEvent
                    |> Option.map (fun event ->
                        "T" + string event.Tick + " · " + event.Summary)
                    |> Option.defaultValue "None"
                )
                Html.dt "Formula"
                Html.dd (model.Selection.Formula |> Option.defaultValue "None")
            ]
            Html.h3 "Disclosed events"
            if List.isEmpty events then
                Html.p "No events are disclosed at this tick."
            else
                Html.ol [
                    prop.className "event-list"
                    prop.children [
                        for event in events do
                            Html.li [
                                commandButton [
                                    prop.type'.button
                                    prop.ariaLabel ("Inspect disclosed event " + string event.Id)
                                    prop.text (
                                        "T" + string event.Tick
                                        + " · " + event.Source
                                        + " · " + event.Summary
                                    )
                                    prop.onClick (fun _ ->
                                        dispatch (ShellMsg(EventSelected(Some event.Id))))
                                ]
                            ]
                    ]
                ]
        ]
    ]

let private reviewPanelBody panelId (model: SIR.Client.Model) dispatch =
    match panelId with
    | "roster" -> reviewRosterPanel model dispatch
    | "tools" ->
        Html.div [
            prop.ariaLabel "Review sources and transport"
            prop.children [ sourcePanel model dispatch; controls model dispatch ]
        ]
    | "layers" -> PanelViews.reviewLayersPanel model
    | "selection" -> reviewSelectionPanel model dispatch
    | "validation" -> statusView model
    | "document" -> PanelViews.reviewDocumentPanel model
    | "diagnostics" -> workerStatus model
    | _ ->
        Html.p [
            prop.className "tactical-layout-panel-placeholder"
            prop.text "No Review capability is assigned to this panel."
        ]

let private sandbox (model: SIR.Client.Model) dispatch =
    let scenario = model.Lab.Scenario

    Html.section [
        prop.className "panel sandbox-panel"
        prop.ariaLabel "Sandbox parameters"
        prop.children [
            Html.h2 "Typed parameters"
            Html.p "Edits create a derived sandbox; the baseline stays immutable."
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
            Html.p "Run an example, edit typed values, compare, sweep, and export."
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
                prop.text "Neither side is verified replay evidence; edits create an identified fork."
            ]
            match report with
            | None ->
                Html.p "Run a scenario, then edit a parameter to compare a linked fork."
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

[<ReactLazyComponent>]
let private LazyDeferredDataPanel simulator selectedUnit bootstrap =
    React.DynamicImported("../RulesExplorer.js")
let private laboratoryResults model dispatch =
    Html.section [
        prop.className "panel lab-results"
        prop.ariaLabel "Laboratory results"
        prop.children [
            Html.h2 "Simulation result"
            match model.Lab.Report with
            | None ->
                Html.p "Run a scenario to see its deterministic result."
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
                Html.p "Export preserves revision, identities, parameters, metrics, and sweep."
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

[<Emit("$0.setPointerCapture($1)")>]
let private capturePointer (target: EventTarget) (pointerId: int) : unit = jsNative

[<Emit("$0.releasePointerCapture($1)")>]
let private releasePointer (target: EventTarget) (pointerId: int) : unit = jsNative

[<Emit("$0.target.closest('[data-unit-id]')?.getAttribute('data-unit-id') ?? null")>]
let private pointerEditorUnitId (event: Browser.Types.Event) : string = jsNative

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

let private editorToolbar
    (state: MapEditorState)
    (tactical: TacticalParcelEditor.TacticalParcelEditorState)
    (tacticalImportText: string)
    (view: EditorWorkspaceState)
    (activePanel: EditorToolPanel)
    panelVisible
    (importAnnouncement: string option)
    dispatch
    =
    let choose (label: string) (tool: MapEditorTool) =
        commandButton [
            prop.type'.button
            prop.text label
            prop.ariaPressed (state.Tool = tool)
            prop.onClick (fun _ -> dispatch (EditorChanged(ChooseTool tool)))
        ]

    let chooseTerrainValue terrain =
        let label =
            MapEditor.terrainLabel terrain
            + " · "
            + MapEditor.terrainPattern terrain
        commandButton [
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
        commandButton [
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
            (if activePanel = DocumentTools then "panel editor-document-panel"
             else "panel editor-tools editor-tools-panel")
            + if panelVisible then "" else " is-collapsed"
        )
        prop.ariaLabel (if activePanel = DocumentTools then "Map document controls" else "Map editing tools")
        prop.children [
            if activePanel <> DocumentTools then
                Html.div [
                    prop.className "editor-section-heading"
                    prop.children [
                        Html.div [
                            Html.p [ prop.className "eyebrow"; prop.text "Author" ]
                            Html.h2 "Map editor"
                        ]
                        Html.p [
                            prop.className "active-tool"
                            prop.text ("Active tool: " + toolLabel state.Tool)
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
                        choosePanel "Environment" TacticalEnvironmentTools
                        choosePanel "Zones" ZoneTools
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
                                    choose (MapEditor.terrainToolLabel tool)
                                        (Terrain tool)
                            ]
                        ]
                        Html.div [
                            prop.className "terrain-palette"
                            prop.role.group
                            prop.ariaLabel "Terrain palette"
                            prop.children [
                                chooseTerrainValue Open
                                chooseTerrainValue Rough
                                chooseTerrainValue Blocked
                                chooseTerrainValue Objective
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
                                    "Commit the terrain preview"
                                    (match state.Gesture with
                                     | TerrainGesture _ -> false
                                     | _ -> true)
                                    (fun _ ->
                                        dispatch (EditorChanged CommitEditorGesture))
                                button
                                    "Cancel preview"
                                    "Cancel the terrain preview"
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
                                        document.getElementById("persistent-tactical-svg").focus ()
                                | "Escape" ->
                                    event.preventDefault ()
                                    document.getElementById("persistent-tactical-svg").focus ()
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
                                    "Commit the unit placement, paste, or movement preview"
                                    (match state.Gesture with
                                     | CommandPreviewGesture _
                                     | UnitMoveGesture _ -> false
                                     | _ -> true)
                                    (fun _ -> dispatch (EditorChanged CommitEditorGesture))
                                button
                                    "Reset move"
                                    "Reset selected units to their original preview positions"
                                    (match state.Gesture with
                                     | UnitMoveGesture _ -> false
                                     | _ -> true)
                                    (fun _ -> dispatch (EditorChanged ResetUnitMovePreview))
                                button
                                    "Cancel preview"
                                    "Cancel the unit preview"
                                    (match state.Gesture with
                                     | CommandPreviewGesture _
                                     | UnitMoveGesture _ -> false
                                     | _ -> true)
                                    (fun _ -> dispatch (EditorChanged CancelEditorGesture))
                                button
                                    "Browse presets"
                                    "Return to unit preset browsing"
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
                                button "Finish" "Finish wall polyline" false (fun _ ->
                                    dispatch (EditorChanged FinishEdgePolyline))
                                button "Back" "Remove last polyline segment" false (fun _ ->
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
                    | TacticalEnvironmentTools ->
                        TacticalEnvironmentView.view state tactical tacticalImportText dispatch
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
                        Html.p "Create, select, and edit geometry."
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
                        | None -> Html.p "Select a region to edit it."
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
                        Html.div [
                            prop.className "control-row"
                            prop.role.toolbar
                            prop.ariaLabel "Map editor document actions"
                            prop.children [
                                button "New map" "Create a new empty map" false (fun _ -> dispatch (EditorChanged RequestNewMap))
                                button "Undo" "Undo last editor command" state.UndoHistory.IsEmpty (fun _ -> dispatch (InvokeTacticalCommand "editor.history.undo"))
                                button "Redo" "Redo last editor command" state.RedoHistory.IsEmpty (fun _ -> dispatch (InvokeTacticalCommand "editor.history.redo-shift-z"))
                                button "Copy" "Copy selected units" state.SelectedUnits.IsEmpty (fun _ -> dispatch (InvokeTacticalCommand "editor.selection.copy"))
                                button "Paste" "Paste copied units" state.Clipboard.IsNone (fun _ -> dispatch (InvokeTacticalCommand "editor.selection.paste"))
                                button "Duplicate" "Duplicate selected units" state.SelectedUnits.IsEmpty (fun _ -> dispatch (InvokeTacticalCommand "editor.selection.duplicate"))
                                button "Delete" "Delete selected objects" (state.SelectedUnits.IsEmpty && state.SelectedRegion.IsNone) (fun _ -> dispatch (InvokeTacticalCommand "editor.selection.delete"))
                                button "Select all" "Select all objects in the active domain" false (fun _ -> dispatch (InvokeTacticalCommand "editor.selection.all"))
                                button "Zoom out" "Zoom the tactical workscreen out" false (fun _ -> dispatch (InvokeTacticalCommand "scene.camera.zoom-out"))
                                button "Zoom in" "Zoom the tactical workscreen in" false (fun _ -> dispatch (InvokeTacticalCommand "scene.camera.zoom-in"))
                                button "Fit" "Fit the complete map" false (fun _ -> dispatch (InvokeTacticalCommand "scene.camera.fit"))
                                button "Actual size" "Reset map camera to one hundred percent" false (fun _ -> dispatch (EditorWorkspaceChanged ResetEditorCamera))
                                button "Frame selection" "Frame selected map objects" state.SelectedUnits.IsEmpty (fun _ -> dispatch (EditorWorkspaceChanged FrameEditorSelection))
                            ]
                        ]
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
                                Html.p "Remote or executable backgrounds are rejected."
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
                                            commandButton [
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
                                            commandButton [
                                                prop.type'.button
                                                prop.text ("Recall " + saved.Name)
                                                prop.onClick (fun _ ->
                                                    dispatch (RecallEditorView name))
                                            ]
                                            commandButton [
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
                        match importAnnouncement with
                        | Some announcement ->
                            Html.p [
                                prop.role.alert
                                prop.ariaLive.assertive
                                prop.className "validation-error"
                                prop.text announcement
                            ]
                        | None -> Html.none
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
                ]
            ] else Html.none
        ]
    ]

let private editorUnitPanel (state: MapEditorState) dispatch =
    let selectedUnits =
        state.SelectedUnits
        |> Set.toList
        |> List.choose (fun id -> Map.tryFind id state.Map.Units)
    let fieldLabel label (projection: EditorUnit -> 'value) =
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
                    Html.article [ Html.h3 "General AI"; Html.p "Approach the nearest hostile." ]
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
                        let commandId =
                            match value with
                            | "Scripted AI" ->
                                "simulator.pointer.controller.scripted"
                            | "General AI" ->
                                "simulator.pointer.controller.general"
                            | _ -> "simulator.pointer.controller.manual"
                        dispatch (InvokeTacticalCommand commandId))
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
                        dispatch (
                            InvokeTacticalValueCommand(
                                "simulator.pointer.script.set",
                                value
                            )
                        ))
                ]
                Html.div [
                    prop.className "manual-movement"
                    prop.children [
                        for label, direction in movement do
                            let directionId =
                                match direction with
                                | NorthWest -> "north-west"
                                | North -> "north"
                                | NorthEast -> "north-east"
                                | West -> "west"
                                | East -> "east"
                                | SouthWest -> "south-west"
                                | South -> "south"
                                | SouthEast -> "south-east"
                            button label ("Move unit " + label) state.IsRunning (fun _ ->
                                dispatch (
                                    InvokeTacticalCommand(
                                        "simulator.pointer.movement." + directionId
                                    )
                                ))
                    ]
                ]
                Html.h3 "Route planner"
                Html.p "Choose a destination; routing avoids obstacles."
                Html.div [
                    prop.className "manual-movement"
                    prop.children [
                        button "←" "Move route preview left" state.IsRunning (fun _ ->
                            dispatch (InvokeTacticalCommand "simulator.preview.west"))
                        button "↑" "Move route preview up" state.IsRunning (fun _ ->
                            dispatch (InvokeTacticalCommand "simulator.preview.north"))
                        button "↓" "Move route preview down" state.IsRunning (fun _ ->
                            dispatch (InvokeTacticalCommand "simulator.preview.south"))
                        button "→" "Move route preview right" state.IsRunning (fun _ ->
                            dispatch (InvokeTacticalCommand "simulator.preview.east"))
                        button "Commit route" "Commit clear route preview" (state.IsRunning || handoff.PreviewDestination.IsNone) (fun _ ->
                            dispatch (InvokeTacticalCommand "simulator.preview.commit"))
                        button "Reset route" "Return route preview to unit origin" (state.IsRunning || handoff.PreviewDestination.IsNone) (fun _ ->
                            dispatch (InvokeTacticalCommand "simulator.preview.reset"))
                        button "Cancel route" "Cancel route preview" (state.IsRunning || handoff.PreviewDestination.IsNone) (fun _ ->
                            dispatch (InvokeTacticalCommand "simulator.preview.cancel"))
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
        prop.className "samples-panel-content"
        prop.ariaLabel "Curated maps simulations and replays"
        prop.children [
            Html.div [
                prop.className "samples-heading"
                prop.children [
                    Html.p [ prop.className "eyebrow"; prop.text "Explore mechanics" ]
                    Html.h2 "Curated samples"
                    Html.p "Open a map, run its sandbox, or inspect a replay walkthrough."
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
                prop.text "Walkthroughs are sandbox evidence, not verified match replays."
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

let private planningPanelBody
    (state: PlanningWorkspaceState)
    panelId
    dispatch
    =
    let planningButton label commandId =
        button label label false (fun _ ->
            dispatch (InvokeTacticalCommand commandId))

    let directions =
        [ "N", "north", North
          "NE", "north-east", NorthEast
          "E", "east", East
          "SE", "south-east", SouthEast
          "S", "south", South
          "SW", "south-west", SouthWest
          "W", "west", West
          "NW", "north-west", NorthWest ]

    let selected =
        state.SelectedUnit
        |> Option.bind (fun id -> state.Roster |> Array.tryFind (fun unit -> unit.UnitId = id))

    Html.div [
        prop.className "planning-panel-content"
        prop.children [
            if panelId = "document" then Html.header [
                prop.className "panel planning-status"
                prop.ariaLabel "Planning revision state"
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
            if panelId = "tools" then Html.nav [
                prop.className "panel planning-tools"
                prop.ariaLabel "Battlefield planning tools"
                prop.children [
                    for label, key, commandId, tool in
                        [ "Route", "R", "planning.route", RouteTool
                          "Facing", "F", "planning.facing", FacingTool
                          "Attention", "A", "planning.attention", AttentionTool
                          "Stance", "S", "planning.stance", StanceTool
                          "Hold", "H", "planning.hold", HoldTool
                          "Engage", "E", "planning.engagement", EngagementTool
                          "Sync", "M", "planning.synchronization", SynchronizationTool ] do
                        commandButton [
                            prop.type'.button
                            prop.ariaPressed (state.Tool = tool)
                            prop.text (label + " · " + key)
                            prop.onClick (fun _ ->
                                dispatch (InvokeTacticalCommand commandId))
                        ]
                    button "Undo" "Undo planning edit · Ctrl+Z" (not (PlanningWorkspace.canUndo state)) (fun _ ->
                        dispatch (InvokeTacticalCommand "planning.undo"))
                    button "Redo" "Redo planning edit · Ctrl+Y" (not (PlanningWorkspace.canRedo state)) (fun _ ->
                        dispatch (InvokeTacticalCommand "planning.redo"))
                    button "Preview" "Preview authored revision as intent-only prediction" false (fun _ ->
                        dispatch (InvokeTacticalCommand "planning.preview"))
                    button
                        "Validate"
                        "Validate previewed authored revision in worker"
                        (state.Predicted |> Option.forall (fun preview -> preview.Revision <> state.Revision))
                        (fun _ -> dispatch (InvokeTacticalCommand "planning.validate"))
                    button
                        "Commit"
                        "Commit previewed and accepted authored revision"
                        (state.AcceptedRevision <> Some state.Revision
                         || state.Predicted |> Option.forall (fun preview -> preview.Revision <> state.Revision))
                        (fun _ -> dispatch (InvokeTacticalCommand "planning.commit"))
                ]
            ]
            if panelId = "roster" then Html.aside [
                prop.className "panel planning-roster"
                prop.ariaLabel ("Planning roster, " + string state.Roster.Length + " units")
                prop.children [
                    Html.h2 "Roster"
                    Html.div [
                        prop.className "planning-roster-list"
                        prop.children [
                            for unit in state.Roster do
                                commandButton [
                                    prop.type'.button
                                    prop.ariaPressed (state.SelectedUnit = Some unit.UnitId)
                                    prop.text (
                                        unit.Name + " · " + unit.Side + " · "
                                        + unit.Role + " · "
                                        + String.concat ", " unit.Equipment + " · "
                                        + string unit.Column + "," + string unit.Row
                                    )
                                    prop.onClick (fun _ ->
                                        dispatch (
                                            InvokeTacticalCommand(
                                                "planning.roster.select."
                                                + string unit.UnitId
                                            )
                                        ))
                                ]
                        ]
                    ]
                ]
            ]
            if panelId = "selection" then Html.aside [
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
                            Html.p "Choose a cell or use waypoint controls."
                            Html.div [
                                prop.className "planning-direction-grid"
                                prop.children [
                                    planningButton "Waypoint west" "planning.inspector.waypoint.west"
                                    planningButton "Waypoint north" "planning.inspector.waypoint.north"
                                    planningButton "Waypoint south" "planning.inspector.waypoint.south"
                                    planningButton "Waypoint east" "planning.inspector.waypoint.east"
                                ]
                            ]
                        | FacingTool
                        | AttentionTool ->
                            Html.div [
                                prop.className "planning-direction-grid"
                                prop.children [
                                    for label, slug, _ in directions do
                                        planningButton
                                            label
                                            ("planning.inspector."
                                             + (if state.Tool = FacingTool then "facing." else "attention.")
                                             + slug)
                                ]
                            ]
                        | StanceTool ->
                            planningButton "Standing" "planning.inspector.stance.standing"
                            planningButton "Crouched" "planning.inspector.stance.crouched"
                            planningButton "Prone" "planning.inspector.stance.prone"
                        | HoldTool -> planningButton "Add hold" "planning.inspector.hold"
                        | EngagementTool ->
                            match
                                state.Roster
                                |> Array.tryFind (fun target -> target.UnitId <> unit.UnitId),
                                unit.CapabilityIds |> Array.tryHead
                            with
                            | Some target, Some capability ->
                                planningButton
                                    ("Engage " + target.Name + " with " + capability)
                                    "planning.inspector.engagement"
                            | None, _ ->
                                Html.p "No disclosed target is available."
                            | _, None ->
                                Html.p "This loadout cannot engage."
                        | SynchronizationTool ->
                            planningButton
                                "Add synchronization marker"
                                "planning.inspector.synchronization"
                    button
                        "Remove selected command"
                        "Remove selected planning command · Delete"
                        state.SelectedCommand.IsNone
                        (fun _ ->
                            dispatch (InvokeTacticalCommand "timeline.remove-command"))
                    Html.h3 "Authored commands"
                    if List.isEmpty state.Commands then Html.p "No authored commands."
                    for command in state.Commands do
                        commandButton [
                            prop.type'.button
                            prop.ariaPressed (state.SelectedCommand = Some command.Id)
                            prop.text (planningCommandLabel command)
                            prop.onClick (fun _ ->
                                dispatch (InvokeTacticalCommand("planning.timeline.select." + command.Id)))
                        ]
                ]
            ]
            if panelId = "validation" then Html.section [
                prop.className "panel planning-validation"
                prop.ariaLabel "Planning validation navigation"
                prop.children [
                    Html.h2 ("Validation · " + string state.Issues.Length + " issues")
                    Html.p "Use issue controls to select the affected command."
                    for index, issue in Array.indexed state.Issues do
                        commandButton [
                            prop.type'.button
                            prop.ariaPressed (state.FocusedIssue = Some index)
                            prop.text (issue.Code + " · " + issue.Detail)
                            prop.onClick (fun _ ->
                                dispatch (
                                    InvokeTacticalCommand(
                                        "planning.issue.focus." + string index
                                    )
                                ))
                        ]
                ]
            ]
            if panelId = "document" then Html.details [
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

let private supportingPanelControls (model: Model) dispatch =
    let item (label: string) panelId =
        let placement =
            model.TacticalLayout.Placements
            |> List.find (fun placement -> placement.PanelId = panelId)
        commandButton [
            prop.type'.button
            prop.text label
            prop.ariaPressed placement.Visible
            prop.ariaControls ("layout-panel-" + panelId)
            prop.onClick (fun _ -> dispatch (OpenSupportingPanel panelId))
        ]

    Html.nav [
        prop.className "tactical-supporting-controls"
        prop.ariaLabel "Supporting application sections"
        prop.children [
            item "Rules" "rules"
            item "Data" "data"
            item "Samples" "samples"
        ]
    ]

let private tacticalCommandButton (model: Model) (commandId: string) (text: string) (label: string) disabled onClick =
    let effective =
        activeTacticalRegistry model
        |> List.tryFind (fun command -> command.Id = commandId)
        |> Option.bind (UnifiedTacticalWorkspace.effectiveGesture model.TacticalBindings)
    commandButton [
        prop.type'.button
        prop.text text
        prop.disabled disabled
        prop.title (label + " · " + UnifiedTacticalWorkspace.displayGestureFor shortcutPlatform effective)
        prop.ariaLabel label
        match UnifiedTacticalWorkspace.accessibleGestureFor shortcutPlatform effective with
        | Some shortcut -> prop.custom ("aria-keyshortcuts", shortcut)
        | None -> prop.custom ("data-binding-state", "unassigned")
        prop.onClick onClick
    ]

let private tacticalModalityControls model dispatch =
    let item (label: string) commandId (value: WorkspaceMode) =
        let isCurrent = model.Workspace = value
        let effective =
            activeTacticalRegistry model
            |> List.tryFind (fun command -> command.Id = commandId)
            |> Option.bind (UnifiedTacticalWorkspace.effectiveGesture model.TacticalBindings)
        commandButton [
            prop.type'.button
            prop.text label
            prop.ariaPressed isCurrent
            prop.title ("Switch to " + label + " · " + UnifiedTacticalWorkspace.displayGestureFor shortcutPlatform effective)
            prop.ariaLabel label
            match UnifiedTacticalWorkspace.accessibleGestureFor shortcutPlatform effective with
            | Some shortcut -> prop.custom ("aria-keyshortcuts", shortcut)
            | None -> prop.custom ("data-binding-state", "unassigned")
            prop.onClick (fun _ -> dispatch (InvokeTacticalCommand commandId))
        ]

    Html.nav [
        prop.className "tactical-modality-controls"
        prop.ariaLabel "Tactical modality"
        prop.children [
            item "Editor" "workspace.editor" EditorWorkspace
            item "Plan" "workspace.plan" PlanningWorkspace
            item "Simulate" "workspace.simulate" SimulatorWorkspace
            item "Review" "workspace.review" ReplayWorkspace
        ]
    ]

let private tacticalTimeline model dispatch =
    let state = model.Tactical
    let playUnavailableReason =
        match model.Simulator, model.Workspace with
        | None, ReplayWorkspace when model.Shell.Playback.FinalTick <= 0 ->
            Some "Load a replay package to start playback."
        | None, _ ->
            Some "Correct the current map so a valid simulation can be maintained."
        | _ -> None
    let available commandId =
        activeTacticalRegistry model
        |> List.exists (fun command ->
            command.Id = commandId
            && Set.contains model.Tactical.Modality command.Modalities
            && tacticalCommandAvailable model command)
    let runtime =
        match model.Workspace, model.SampleReplayFrames, model.Simulator with
        | ReplayWorkspace, _, _ when model.Shell.Playback.FinalTick > 0 ->
            [ { Id = "committed-replay"
                UnitId = None
                StartTick = 0L
                EndTick = int64 model.Shell.Playback.FinalTick
                Channel = Committed
                Label = "Verified committed replay"
                Issue = None } ]
        | _, _, Some simulator ->
            [ { Id = "committed-simulator-runtime"
                UnitId = model.SimulatorSelectedUnit
                StartTick = 0L
                EndTick = int64 simulator.Tick
                Channel = Committed
                Label = "Committed simulator execution"
                Issue = None } ]
        | ReplayWorkspace, _, None when model.Shell.Playback.FinalTick > 0 ->
            [ { Id = "committed-replay"
                UnitId = None
                StartTick = 0L
                EndTick = int64 model.Shell.Playback.FinalTick
                Channel = Committed
                Label = "Verified committed replay"
                Issue = None } ]
        | _ -> []
    let segments = state.Segments @ runtime

    Html.section [
        prop.className "tactical-timeline"
        prop.ariaLabel "Unified tactical timeline"
        prop.custom ("data-time-cursor", string state.Cursor)
        prop.custom ("data-committed-through", string state.CommittedThrough)
        prop.custom (
            "data-scrub-semantics",
            if model.Workspace = ReplayWorkspace && model.Shell.Playback.FinalTick > 0 then
                "reconstructed-replay-state-at-cursor"
            elif model.Simulator.IsSome then
                "reconstructed-runtime-state-at-cursor"
            else "projection-only"
        )
        prop.children [
            Html.ul [
                prop.className "tactical-timeline-channel-legend"
                prop.ariaLabel "Timeline channel legend"
                prop.children [
                    for channel in [ Authored; Predicted; Accepted; Committed ] do
                        Html.li [
                            prop.custom ("data-time-channel", string channel)
                            prop.text (string channel)
                        ]
                ]
            ]
            Html.div [
                prop.className "tactical-transport"
                prop.children [
                    tacticalCommandButton model "timeline.play-toggle" (if state.IsPlaying then "Pause" else "Play") (if state.IsPlaying then "Pause tactical timeline" else "Play tactical timeline") (not (available "timeline.play-toggle")) (fun _ -> dispatch (InvokeTacticalCommand "timeline.play-toggle"))
                    tacticalCommandButton model "timeline.home" "Home" "Go to tactical timeline start" (not (available "timeline.home")) (fun _ -> dispatch (InvokeTacticalCommand "timeline.home"))
                    tacticalCommandButton model "timeline.step-back" "−1" "Step tactical timeline backward" (not (available "timeline.step-back")) (fun _ -> dispatch (InvokeTacticalCommand "timeline.step-back"))
                    tacticalCommandButton model "timeline.step-forward" "+1" "Step tactical timeline forward" (not (available "timeline.step-forward")) (fun _ -> dispatch (InvokeTacticalCommand "timeline.step-forward"))
                    tacticalCommandButton model "timeline.end" "End" "Go to tactical timeline end" (not (available "timeline.end")) (fun _ -> dispatch (InvokeTacticalCommand "timeline.end"))
                    commandButton [
                        prop.type'.button
                        prop.text "Move command here"
                        prop.disabled (not (available "timeline.move-command"))
                        prop.onClick (fun _ ->
                            dispatch (InvokeTacticalCommand "timeline.move-command"))
                    ]
                    commandButton [
                        prop.type'.button
                        prop.text "Remove command"
                        prop.disabled (not (available "timeline.remove-command"))
                        prop.onClick (fun _ ->
                            dispatch (InvokeTacticalCommand "timeline.remove-command"))
                    ]
                    Html.label [
                        prop.text "Time"
                        prop.children [
                            Html.input [
                                prop.type'.number
                                prop.min 0
                                prop.max (int state.Horizon)
                                prop.value (int state.Cursor)
                                prop.onChange (fun (value: int) ->
                                    dispatch (TacticalTimeChanged(int64 value)))
                            ]
                        ]
                    ]
                ]
            ]
            Html.input [
                prop.type'.range
                prop.className "tactical-time-ruler"
                prop.ariaLabel "Current tactical time"
                prop.min 0
                prop.max (int state.Horizon)
                prop.value (int state.Cursor)
                prop.onChange (fun (value: int) ->
                    dispatch (TacticalTimeChanged(int64 value)))
            ]
            Html.div [
                prop.className "tactical-time-cursor"
                prop.role.status
                prop.ariaLive.polite
                prop.text (
                    "Current time "
                    + string state.Cursor
                    + " · next editable "
                    + string (UnifiedTacticalWorkspace.nextEditableBoundary state)
                    + (playUnavailableReason
                       |> Option.map (fun reason -> " · Play unavailable: " + reason)
                       |> Option.defaultValue "")
                )
            ]
            Html.ol [
                prop.className "tactical-command-lanes"
                prop.ariaLabel "Authored, predicted, accepted, and committed timeline segments"
                prop.children [
                    for segment in segments do
                        Html.li [
                            prop.custom ("data-segment-id", segment.Id)
                            prop.custom ("data-time-channel", string segment.Channel)
                            prop.children [
                                Html.strong (string segment.Channel)
                                Html.span (
                                    " " + segment.Label + " · "
                                    + string segment.StartTick + "–" + string segment.EndTick
                                )
                                match segment.Issue with
                                | Some issue -> Html.span (" · issue: " + issue)
                                | None -> Html.none
                            ]
                        ]
                ]
            ]
        ]
    ]

let private tacticalBindingDialog model dispatch =
    if not model.TacticalBindingsOpen then Html.none
    else
        let commands =
            activeTacticalRegistry model
            |> List.filter (fun command ->
                Set.contains model.Tactical.Modality command.Modalities
                && not (
                    command.Id.StartsWith(
                        "simulator.pointer.",
                        StringComparison.Ordinal
                    )
                ))
        Html.section [
            prop.id "tactical-binding-dialog"
            prop.className "tactical-binding-dialog"
            prop.role.dialog
            prop.tabIndex -1
            prop.custom ("aria-modal", "true")
            prop.ariaLabel "Configure tactical command bindings"
            prop.children [
                Html.div [
                    prop.className "modal-input-panel-heading"
                    prop.children [
                        Html.h2 "Command bindings"
                        commandButton [
                            prop.type'.button
                            prop.text "Close"
                            prop.onClick (fun _ -> dispatch ToggleTacticalBindings)
                        ]
                    ]
                ]
                Html.p "Capture a gesture; conflicts are checked before saving."
                if not (List.isEmpty model.TacticalBindingDiagnostics) then
                    Html.ul [
                        prop.role.alert
                        prop.children [
                            for diagnostic in model.TacticalBindingDiagnostics do
                                Html.li diagnostic
                        ]
                    ]
                Html.ul [
                    prop.className "tactical-binding-list"
                    prop.children [
                        for command in commands do
                            let effective =
                                UnifiedTacticalWorkspace.effectiveGesture
                                    model.TacticalBindings
                                    command
                            Html.li [
                                prop.custom ("data-binding-command", command.Id)
                                prop.children [
                                    Html.label [
                                        prop.children [
                                            Html.span (command.Label + " · " + command.Category)
                                            Html.input [
                                                prop.type'.text
                                                prop.ariaLabel ("Binding for " + command.Label)
                                                prop.placeholder (
                                                    effective
                                                    |> Option.defaultValue "Unbound"
                                                )
                                                prop.value (
                                                    model.TacticalBindingDrafts
                                                    |> Map.tryFind command.Id
                                                    |> Option.defaultValue ""
                                                )
                                                prop.onChange (fun value ->
                                                    dispatch (
                                                        TacticalBindingDraftChanged(
                                                            command.Id,
                                                            value
                                                        )
                                                    ))
                                                prop.onKeyDown (fun event ->
                                                    event.stopPropagation ()
                                                    let key =
                                                        if event.key = " " then "Space"
                                                        elif event.key.Length = 1 then event.key.ToUpperInvariant()
                                                        else event.key
                                                    let gesture =
                                                        [ if event.ctrlKey || event.metaKey then "Ctrl"
                                                          if event.altKey then "Alt"
                                                          if event.shiftKey && event.key <> "?" then "Shift"
                                                          key ]
                                                        |> String.concat "+"
                                                    let reserved =
                                                        Set.contains
                                                            (gesture.ToUpperInvariant())
                                                            (Set.ofList [
                                                                "CTRL+L"; "CTRL+T"; "CTRL+W"; "CTRL+R"
                                                                "CTRL+SHIFT+R"; "ALT+F4"; "F5"
                                                            ])
                                                    let capture =
                                                        (event.ctrlKey || event.metaKey || event.altKey
                                                         || event.key.StartsWith("F")
                                                         || Set.contains event.key (Set.ofList [
                                                             "ArrowLeft"; "ArrowRight"; "ArrowUp"; "ArrowDown"
                                                             "Home"; "End"; "Delete"; "Backspace"; "Escape"; " "
                                                         ]))
                                                        && event.key <> "Tab"
                                                    if capture && not reserved then
                                                        event.preventDefault ()
                                                        dispatch (
                                                            TacticalBindingDraftChanged(command.Id, gesture)
                                                        ))
                                            ]
                                        ]
                                    ]
                                    Html.span (
                                        effective
                                        |> Option.defaultValue "Unbound"
                                    )
                                    commandButton [
                                        prop.type'.button
                                        prop.text "Apply"
                                        prop.onClick (fun _ ->
                                            dispatch (
                                                ApplyTacticalBinding(
                                                    command.Id,
                                                    false
                                                )
                                            ))
                                    ]
                                    commandButton [
                                        prop.type'.button
                                        prop.text "Replace conflict"
                                        prop.onClick (fun _ ->
                                            dispatch (
                                                ApplyTacticalBinding(
                                                    command.Id,
                                                    true
                                                )
                                            ))
                                    ]
                                    commandButton [
                                        prop.type'.button
                                        prop.text "Clear"
                                        prop.onClick (fun _ ->
                                            dispatch (
                                                ClearTacticalBinding command.Id
                                            ))
                                    ]
                                    commandButton [
                                        prop.type'.button
                                        prop.text "Restore"
                                        prop.onClick (fun _ ->
                                            dispatch (
                                                RestoreTacticalBinding command.Id
                                            ))
                                    ]
                                ]
                            ]
                    ]
                ]
                Html.div [
                    prop.className "tactical-binding-actions"
                    prop.children [
                        commandButton [
                            prop.type'.button
                            prop.text "Restore modality"
                            prop.onClick (fun _ ->
                                dispatch RestoreTacticalModalityBindings)
                        ]
                        commandButton [
                            prop.type'.button
                            prop.text "Restore all"
                            prop.onClick (fun _ ->
                                dispatch RestoreAllTacticalBindings)
                        ]
                    ]
                ]
                Html.label [
                    prop.children [
                        Html.span "Import or export deterministic binding JSON"
                        Html.textarea [
                            prop.ariaLabel "Tactical binding JSON"
                            prop.value (
                                if
                                    String.IsNullOrWhiteSpace
                                        model.TacticalBindingImport
                                then
                                    UnifiedTacticalWorkspace.exportBindings
                                        model.TacticalBindings
                                else model.TacticalBindingImport
                            )
                            prop.onChange (fun value ->
                                dispatch (
                                    TacticalBindingImportChanged value
                                ))
                        ]
                    ]
                ]
                commandButton [
                    prop.type'.button
                    prop.text "Import bindings"
                    prop.onClick (fun _ -> dispatch ImportTacticalBindings)
                ]
            ]
        ]

let private currentModalInputs model =
    match model.Workspace with
    | EditorWorkspace ->
        let facts =
            { Editor = model.Editor
              ActiveDomain =
                match model.EditorToolPanel with
                | TerrainTools -> TerrainDomain
                | UnitTools -> UnitDomain
                | EdgeTools -> EdgeDomain
                | ZoneTools -> RegionDomain
                | TacticalEnvironmentTools -> DocumentDomain
                | DocumentTools -> DocumentDomain
              PanHeld = editorPanHeld model
              InputHelpExpanded = model.InputHelpExpanded }
        let catalog =
            ModalInput.editorCatalog facts
            |> UnifiedTacticalWorkspace.adaptModalCatalog model.TacticalBindings
        ModalInput.possibleInputs (ModalInput.deriveEditorContexts facts) catalog
    | SimulatorWorkspace ->
        let simulator = model.Simulator
        let facts =
            { SimulatorHandoffPresent = simulator.IsSome
              SimulatorIsRunning = simulator |> Option.exists _.IsRunning
              SimulatorHasRoutePreview = simulator |> Option.bind _.PreviewDestination |> Option.isSome
              SimulatorControllerSelection = model.SimulatorControllerSelection
              SimulatorRevisionIsStale =
                simulator
                |> Option.exists (MapEditorSimulator.isBehindDraft model.Editor)
              InputHelpExpanded = model.InputHelpExpanded }
        let catalog =
            ModalInput.simulatorCatalog
                model.SimulatorSelectedUnit
                simulator
                model.SimulatorControllerSelection
            |> UnifiedTacticalWorkspace.adaptModalCatalog model.TacticalBindings
        ModalInput.possibleInputs (ModalInput.deriveSimulatorContexts facts) catalog
    | _ -> []

let private tacticalContextHelp model dispatch =
    let gestureText (gesture: InputGesture) =
        let key =
            match NormalizedKey.value gesture.Key with
            | "ArrowLeft" -> "←"
            | "ArrowRight" -> "→"
            | "ArrowUp" -> "↑"
            | "ArrowDown" -> "↓"
            | "Escape" -> "Esc"
            | value when value.Length = 1 -> value.ToUpperInvariant()
            | value -> value
        [ if gesture.Modifiers.ControlOrMeta then "Ctrl/Cmd"
          if gesture.Modifiers.Alt then "Alt"
          if gesture.Modifiers.Shift && NormalizedKey.value gesture.Key <> "?" then "Shift"
          key ]
        |> String.concat "+"
    let accessibleGesture (gesture: InputGesture) =
        (gestureText gesture)
            .Replace("Ctrl/Cmd", "Control")
            .Replace("←", "ArrowLeft")
            .Replace("→", "ArrowRight")
            .Replace("↑", "ArrowUp")
            .Replace("↓", "ArrowDown")
            .Replace("Esc", "Escape")
    let modalInputs = currentModalInputs model
    let modalIds = modalInputs |> List.map _.Id |> Set.ofList
    let commands =
        activeTacticalRegistry model
        |> List.filter (fun command ->
            Set.contains model.Tactical.Modality command.Modalities
            && tacticalCommandAvailable model command
            && not (command.Id.StartsWith("editor.", StringComparison.Ordinal))
            && (not (command.Id.StartsWith("simulator.", StringComparison.Ordinal))
                || command.Id.StartsWith(
                    "simulator.pointer.",
                    StringComparison.Ordinal
                ))
            && not (Set.contains command.Id modalIds))
        |> List.sortBy (fun command -> command.Category, command.Label)
    Html.section [
        prop.className ("modal-input-strip" + if model.InputHelpExpanded then " is-expanded" else "")
        prop.ariaLabel "Current tactical actions"
        prop.children [
            Html.div [
                prop.className "modal-input-summary"
                prop.children [
                    Html.div [
                        prop.className "modal-input-state"
                        prop.children [
                            Html.strong [
                                prop.className "modal-input-headline"
                                prop.text (string model.Tactical.Modality + " · time " + string model.Tactical.Cursor)
                            ]
                            Html.span [
                                prop.className "modal-input-detail"
                                prop.text (string (commands.Length + modalInputs.Length) + " actions currently executable")
                            ]
                        ]
                    ]
                    commandButton [
                        prop.id "tactical-input-toggle"
                        prop.type'.button
                        prop.className "modal-input-toggle"
                        prop.text "Inputs"
                        prop.ariaExpanded model.InputHelpExpanded
                        prop.ariaControls "tactical-input-panel"
                        prop.onClick (fun _ -> dispatch (ToggleInputHelp false))
                    ]
                    if model.Workspace = SimulatorWorkspace then
                        commandButton [
                            prop.type'.button
                            prop.text "Samples"
                            prop.ariaLabel "Open simulator samples"
                            prop.onClick (fun _ -> dispatch (OpenSupportingPanel "samples"))
                        ]
                    if model.Workspace = SimulatorWorkspace && model.Simulator.IsSome then
                        commandButton [
                            prop.type'.button
                            prop.text "Reset simulation"
                            prop.ariaLabel "Reset simulation to the current authored baseline"
                            prop.onClick (fun _ -> dispatch (InvokeTacticalCommand "simulator.reset.request"))
                        ]
                ]
            ]
            Html.p [
                prop.className "sr-only"
                prop.role.status
                prop.ariaLive.polite
                prop.ariaAtomic true
                prop.text (string model.Tactical.Modality + " action context")
            ]
            if model.InputHelpExpanded then
                Html.section [
                    prop.id "tactical-input-panel"
                    prop.className "modal-input-panel"
                    prop.tabIndex -1
                    prop.ariaLabel ("Executable actions for " + string model.Tactical.Modality)
                    prop.children [
                        Html.div [
                            prop.className "modal-input-panel-heading"
                            prop.children [
                                Html.h3 "Executable actions"
                                commandButton [
                                    prop.id "tactical-configure-bindings"
                                    prop.type'.button
                                    prop.text "Configure bindings"
                                    prop.onClick (fun _ -> dispatch ToggleTacticalBindings)
                                ]
                                commandButton [
                                    prop.type'.button
                                    prop.text "Close"
                                    prop.onClick (fun _ -> dispatch (ToggleInputHelp true))
                                ]
                            ]
                        ]
                        Html.ul [
                            prop.className "modal-input-list"
                            prop.children [
                                for command in commands do
                                    let effective =
                                        UnifiedTacticalWorkspace.effectiveGesture
                                            model.TacticalBindings command
                                    Html.li [
                                        prop.custom ("data-tactical-command", command.Id)
                                        match effective with
                                        | Some _ ->
                                            match UnifiedTacticalWorkspace.accessibleGestureFor shortcutPlatform effective with
                                            | Some shortcut -> prop.custom ("aria-keyshortcuts", shortcut)
                                            | None -> prop.custom ("data-binding-state", "unbound")
                                        | None -> prop.custom ("data-binding-state", "unbound")
                                        prop.children [
                                            Html.kbd (UnifiedTacticalWorkspace.displayGestureFor shortcutPlatform effective)
                                            Html.span (command.Label + " · " + command.Category)
                                            Html.small (
                                                if UnifiedTacticalWorkspace.isRebound model.TacticalBindings command then
                                                    "Rebound"
                                                elif effective.IsNone && command.PointerAvailable then
                                                    "Pointer only / unbound"
                                                else "Default"
                                            )
                                        ]
                                    ]
                                for input in modalInputs do
                                    let effective =
                                        match Map.tryFind input.Id model.TacticalBindings.Overrides with
                                        | Some value -> value
                                        | None ->
                                            Some(
                                                UnifiedTacticalWorkspace.gestureText
                                                    input.InputGesture
                                            )
                                    Html.li [
                                        prop.custom ("data-modal-command", input.Id)
                                        match effective with
                                        | Some _ ->
                                            match UnifiedTacticalWorkspace.accessibleGestureFor shortcutPlatform effective with
                                            | Some shortcut -> prop.custom ("aria-keyshortcuts", shortcut)
                                            | None -> prop.custom ("data-binding-state", "unbound")
                                        | None -> prop.custom ("data-binding-state", "unbound")
                                        prop.children [
                                            Html.kbd (UnifiedTacticalWorkspace.displayGestureFor shortcutPlatform effective)
                                            Html.span (input.Label + " · " + input.Group)
                                            Html.small (
                                                if Map.containsKey input.Id model.TacticalBindings.Overrides then
                                                    if effective.IsSome then "Rebound" else "Pointer only / unbound"
                                                else "Default"
                                            )
                                        ]
                                    ]
                            ]
                        ]
                    ]
                ]
        ]
    ]

let private activeSceneProjection (model: Model) =
    let focusedUnit =
        reconcileTacticalSelectedUnit model.Workspace model
        |> Option.orElseWith (fun () ->
            model.SimulatorSelectedUnit
            |> Option.filter (fun id ->
                model.Simulator
                |> Option.exists (fun simulator ->
                    Map.containsKey id simulator.RuntimeMap.Units)))
    let editorProjection editorFocusedUnit =
        TacticalSceneProjection.editor
            { EditorState = model.Editor
              EditorWorkspace = model.EditorView
              EditorFocusedUnit = editorFocusedUnit }
    let simulatorProjection () =
        model.Simulator
        |> Option.map (fun simulator ->
            TacticalSceneProjection.simulator
                { SimulatorHandoff = simulator
                  SimulatorSelectedUnit = focusedUnit
                  SimulatorCamera = model.EditorView.Camera
                  SimulatorFocusedUnit = focusedUnit })
    let withRuntimeTruth (contextual: SharedSceneProjection) =
        simulatorProjection ()
        |> Option.map (fun runtime ->
            let runtimeUnits =
                runtime.Units
                |> Array.map (fun unit -> unit.Visual.Id, unit)
                |> Map.ofArray
            let units =
                Array.append
                    (contextual.Units
                     |> Array.choose (fun authored ->
                         Map.tryFind authored.Visual.Id runtimeUnits
                         |> Option.map (fun live ->
                             { authored with
                                 PresentationColumn = live.PresentationColumn
                                 PresentationRow = live.PresentationRow
                                 Visual = live.Visual })))
                    (runtime.Units
                     |> Array.filter (fun live ->
                         contextual.Units
                         |> Array.exists (fun authored ->
                             authored.Visual.Id = live.Visual.Id)
                         |> not))
            { contextual with
                RevisionIdentity = runtime.RevisionIdentity
                Tick = runtime.Tick
                Board = runtime.Board
                Terrain = runtime.Terrain
                Edges = runtime.Edges
                Units = units
                Routes = Array.append contextual.Routes runtime.Routes
                Annotations = Array.append contextual.Annotations runtime.Annotations })
        |> Option.defaultValue contextual
    match model.Workspace, TacticalSceneProjection.acceptReview model.Shell with
    | ReplayWorkspace, Some accepted ->
        Some(
            TacticalSceneProjection.review
                { AcceptedReview = accepted
                  ReviewCamera = model.EditorView.Camera
                  ReviewFocusedUnit = focusedUnit }
        )
    | EditorWorkspace, _ ->
        editorProjection focusedUnit |> withRuntimeTruth |> Some
    | PlanningWorkspace, _ ->
        model.Planning
        |> Option.map (fun planning ->
            TacticalSceneProjection.planning
                { PlanningMap = model.Editor.Map
                  PlanningState = planning
                  PlanningCamera = model.EditorView.Camera
                  PlanningFocusedUnit = focusedUnit }
            |> withRuntimeTruth)
    | SimulatorWorkspace, _ ->
        simulatorProjection ()
        |> Option.orElseWith (fun () ->
            reconcileTacticalSelectedUnit EditorWorkspace model
            |> editorProjection
            |> Some)
    | ReplayWorkspace, _ ->
        reconcileTacticalSelectedUnit EditorWorkspace model
        |> editorProjection
        |> withRuntimeTruth
        |> Some

let private activePresentedSceneProjection (model: Model) =
    let projection = activeSceneProjection model
    match model.Workspace, model.PreviousFrame, projection with
    | ReplayWorkspace, Some previous, Some current ->
        let presented, alpha =
            TacticalSceneProjection.interpolateReviewPresentation
                previous
                model.PresentationAlpha
                current
        Some presented, alpha
    | _ -> projection, 1.0

let private sceneDisclosureText = function
    | Disclosed value -> value
    | ExplicitlyUnknown -> "Unknown"
    | NotApplicable -> "Not applicable"
    | NotPresent -> ""

let private sharedSceneClaimsKeyboard
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

let private persistentSceneSvg
    (model: Model)
    (projection: SharedSceneProjection option)
    (presentationAlpha: float)
    dispatch
    =
    let cellSize = 64.0
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

    Svg.svg [
        svg.id "persistent-tactical-svg"
        svg.custom ("data-work-surface-root", "persistent-svg")
        svg.custom ("data-render-contract", "shared-scene-projection-v1")
        svg.custom ("data-scene-owner", owner)
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
        svg.custom ("data-scene-tick", projection |> Option.map _.Tick |> Option.defaultValue 0 |> string)
        svg.custom ("data-live-status", model.Live.Status)
        svg.custom ("data-live-tick", model.Live.Snapshot |> Option.map _.Tick |> Option.defaultValue 0 |> string)
        svg.custom ("data-live-server-sequence", model.Live.Snapshot |> Option.map _.ServerSequence |> Option.defaultValue 0 |> string)
        svg.custom ("data-live-projection-revision", model.Live.Snapshot |> Option.map _.ProjectionRevision |> Option.defaultValue 0 |> string)
        svg.custom ("data-presentation-alpha", string presentationAlpha)
        svg.custom ("data-camera-pan-x", string camera.PanX)
        svg.custom ("data-camera-pan-y", string camera.PanY)
        svg.custom ("data-camera-zoom", string camera.Zoom)
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
        svg.onContextMenu (fun event ->
            if model.Workspace = EditorWorkspace then event.preventDefault ())
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
            if model.Workspace = EditorWorkspace then
                event.preventDefault ()
                let x, y =
                    editorScreenPoint
                        model.EditorView
                        event.currentTarget
                        event.clientX
                        event.clientY
                let factor = if event.deltaY < 0.0 then 1.12 else 1.0 / 1.12
                dispatch (EditorWorkspaceChanged(ZoomEditorAt(x, y, factor))))
        svg.onPointerDown (fun event ->
            if model.Workspace = EditorWorkspace then
                let kind = editorPointerKind event.pointerType
                let terrainToolActive =
                    match model.Editor.Tool with
                    | Terrain _ -> true
                    | _ -> false
                let requestsPan =
                    (kind = TouchPointer
                     && (not terrainToolActive
                         || not model.EditorView.CapturedPointers.IsEmpty))
                    || event.button = 1
                    || event.button = 2
                    || editorPanHeld model
                let requestsSelection =
                    model.Editor.Tool = Select
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
                    match model.Editor.Tool with
                    | Terrain _ -> event.button = 0 && not requestsPan
                    | _ -> false
                if requestsPan || requestsSelection || requestsTerrain then
                    event.preventDefault ()
                    capturePointer event.currentTarget (int event.pointerId)
                    let x, y =
                        editorScreenPoint model.EditorView event.currentTarget event.clientX event.clientY
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
                        if not (Set.contains unitId model.Editor.SelectedUnits) then
                            dispatch (EditorChanged(SelectEditorUnit(Some unitId)))
                        editorSelectionAt x y BeginUnitMove
                    | None when requestsSelection ->
                        editorSelectionAt x y BeginEditorBoxSelection
                    | None when requestsTerrain ->
                        editorTerrainAt x y BeginTerrainGesture
                    | _ when requestsPan && kind = TouchPointer && terrainToolActive ->
                        dispatch (EditorChanged CancelEditorGesture)
                    | _ -> ())
        svg.onPointerMove (fun event ->
            if
                model.Workspace = EditorWorkspace
                && Map.containsKey (int32 event.pointerId) model.EditorView.CapturedPointers
            then
                event.preventDefault ()
                let x, y =
                    editorScreenPoint model.EditorView event.currentTarget event.clientX event.clientY
                let previous =
                    Map.find (int32 event.pointerId) model.EditorView.CapturedPointers
                dispatch (EditorWorkspaceChanged(MoveEditorPointer { previous with X = x; Y = y }))
                if model.Editor.Tool = Select && not previous.RequestsPan then
                    match model.Editor.Gesture with
                    | UnitMoveGesture _ -> editorSelectionAt x y ExtendUnitMove
                    | _ -> editorSelectionAt x y ExtendEditorBoxSelection
                else
                    match model.Editor.Tool with
                    | Terrain _ when not previous.RequestsPan ->
                        editorTerrainAt x y ExtendTerrainGesture
                    | _ -> ()
            elif model.Workspace = EditorWorkspace then
                match model.Editor.Tool with
                | Place _ ->
                    let x, y =
                        editorScreenPoint model.EditorView event.currentTarget event.clientX event.clientY
                    editorSelectionAt x y PreviewUnitPlacement
                | _ -> ())
        svg.onPointerUp (fun event ->
            if
                model.Workspace = EditorWorkspace
                && Map.containsKey (int32 event.pointerId) model.EditorView.CapturedPointers
            then
                let previous =
                    Map.find (int32 event.pointerId) model.EditorView.CapturedPointers
                let x, y =
                    editorScreenPoint model.EditorView event.currentTarget event.clientX event.clientY
                releasePointer event.currentTarget (int event.pointerId)
                dispatch (EditorWorkspaceChanged(EndEditorPointer(int32 event.pointerId)))
                if model.Editor.Tool = Select && not previous.RequestsPan then
                    match model.Editor.Gesture with
                    | UnitMoveGesture _ -> editorSelectionAt x y ExtendUnitMove
                    | _ -> editorSelectionAt x y ExtendEditorBoxSelection
                    dispatch (EditorChanged CommitEditorGesture)
                else
                    match model.Editor.Tool with
                    | Terrain _ when not previous.RequestsPan ->
                        editorTerrainAt x y ExtendTerrainGesture
                        dispatch (EditorChanged CommitEditorGesture)
                    | _ -> ())
        svg.onLostPointerCapture (fun event ->
            if model.Workspace = EditorWorkspace then
                dispatch (EditorWorkspaceChanged(LoseEditorPointerCapture(int32 event.pointerId)))
                match model.Editor.Tool with
                | Select
                | Terrain _ -> dispatch (EditorChanged CancelEditorGesture)
                | _ -> ())
        svg.viewBox (0, 0, max 1 (int boardWidth), max 1 (int boardHeight))
        svg.children [
            Svg.title "Persistent tactical battlefield"
            Svg.desc "Shared scene layers; input passes through the command registry."
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
                        svg.id "persistent-live-authority-layer"
                        svg.custom ("data-live-layer", "live-authority")
                        svg.custom ("data-live-projection", "accepted-server-snapshot")
                        svg.custom ("pointer-events", "none")
                        svg.children [
                            match model.Live.Snapshot with
                            | Some snapshot ->
                                for unit in snapshot.VisibleUnits do
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
                    Svg.g [
                        svg.id "persistent-layer-terrain"
                        svg.custom ("data-scene-layer", "terrain")
                        svg.custom ("data-layer-visible", string (layerVisible "terrain"))
                        svg.custom ("display", editorLayerDisplayValue TerrainDomain "terrain")
                        svg.custom ("opacity", string (editorLayerOpacityValue TerrainDomain))
                        svg.children [
                            match projection with
                            | Some scene ->
                                for terrain in scene.Terrain do
                                    let command =
                                        sharedSceneCellCommand
                                            model
                                            terrain.Column
                                            terrain.Row
                                    let available =
                                        command
                                        |> Option.exists commandAvailable
                                    Svg.rect [
                                        svg.key (ScenePrimitiveId.value terrain.PrimitiveId)
                                        svg.custom (
                                            "data-primitive-id",
                                            ScenePrimitiveId.value terrain.PrimitiveId
                                        )
                                        svg.custom ("data-terrain", terrain.Kind)
                                        svg.custom ("data-command-available", string available)
                                        svg.x (float terrain.Column * cellSize)
                                        svg.y (float terrain.Row * cellSize)
                                        svg.width cellSize
                                        svg.height cellSize
                                        svg.fill (
                                            match terrain.Kind with
                                            | "rough" -> "#35443a"
                                            | "blocked" -> "#202925"
                                            | "objective" -> "#4b4630"
                                            | _ -> "#2b3832"
                                        )
                                        svg.stroke "#52675d"
                                        svg.strokeWidth 1
                                        match command with
                                        | Some commandId when available ->
                                            svg.custom ("role", "button")
                                            svg.custom ("data-binding-state", "unassigned")
                                            svg.custom ("aria-description", "Shortcut: Unassigned")
                                            svg.custom ("aria-label", "Activate cell " + string terrain.Column + "," + string terrain.Row)
                                            svg.onClick (fun _ -> invoke commandId)
                                        | _ -> svg.custom ("aria-hidden", "true")
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
                                            svg.stroke "#9bb0a5"
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
                                                svg.stroke "#ff6b6b"
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
                                            svg.stroke "#ffd166"
                                            svg.strokeWidth 3
                                            svg.custom ("pointer-events", "none")
                                        ]
                                    | _ -> ()
                            | None -> ()
                        ]
                    ]
                    Svg.g [
                        svg.id "persistent-layer-edges"
                        svg.custom ("data-scene-layer", "edges")
                        svg.custom ("data-layer-visible", string (layerVisible "edges"))
                        svg.custom ("display", editorLayerDisplayValue EdgeDomain "edges")
                        svg.custom ("opacity", string (editorLayerOpacityValue EdgeDomain))
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
                                            | "door" -> "#ffd166"
                                            | "window" -> "#67b7ff"
                                            | _ -> "#eef7f2"
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
                    Svg.g [
                        svg.id "persistent-layer-units"
                        svg.custom ("data-scene-layer", "units")
                        svg.custom ("data-layer-visible", string (layerVisible "units"))
                        svg.custom ("display", editorLayerDisplayValue UnitDomain "units")
                        svg.custom ("opacity", string (editorLayerOpacityValue UnitDomain))
                        svg.children [
                            match projection with
                            | Some scene ->
                                for unit in scene.Units do
                                    let visual = unit.Visual
                                    let presentationX = unit.PresentationColumn * cellSize
                                    let presentationY = unit.PresentationRow * cellSize
                                    let width = float (CellExtent.value visual.FootprintWidth) * cellSize
                                    let depth = float (CellExtent.value visual.FootprintDepth) * cellSize
                                    let command = sharedSceneUnitCommand model visual.Id
                                    let available =
                                        command
                                        |> Option.exists commandAvailable
                                    Svg.g [
                                        svg.key (ScenePrimitiveId.value unit.PrimitiveId)
                                        svg.custom ("data-primitive-id", ScenePrimitiveId.value unit.PrimitiveId)
                                        svg.custom ("data-unit-id", string visual.Id)
                                        svg.custom ("data-unit-class", UnitClassId.value visual.ClassId)
                                        svg.custom ("data-unit-footprint", string (CellExtent.value visual.FootprintWidth) + "x" + string (CellExtent.value visual.FootprintDepth))
                                        svg.custom ("data-unit-status", String.concat " " visual.StatusIds)
                                        svg.custom ("data-presentation-column", string unit.PresentationColumn)
                                        svg.custom ("data-presentation-row", string unit.PresentationRow)
                                        match visual.StanceId with
                                        | Disclosed stance -> svg.custom ("data-unit-stance", stance)
                                        | _ -> ()
                                        svg.custom ("data-command-available", string available)
                                        svg.tabIndex (if selected unit.PrimitiveId then 0 else -1)
                                        match command with
                                        | Some commandId when available ->
                                            svg.custom ("role", "button")
                                            svg.custom ("data-binding-state", "unassigned")
                                            svg.custom ("aria-description", "Shortcut: Unassigned")
                                            svg.custom (
                                                "aria-label",
                                                "Select tactical unit " + string visual.Id
                                                + ", " + UnitClassId.value visual.ClassId
                                                + ", " + string (CellExtent.value visual.FootprintWidth)
                                                + " by " + string (CellExtent.value visual.FootprintDepth)
                                            )
                                            svg.onClick (fun event ->
                                                event.stopPropagation ()
                                                if model.Workspace = EditorWorkspace && event.shiftKey then
                                                    dispatch (EditorChanged(ToggleEditorUnitSelection visual.Id))
                                                else invoke commandId)
                                            svg.onKeyDown (fun event ->
                                                if event.key = "Enter" || event.key = " " then
                                                    event.preventDefault ()
                                                    event.stopPropagation ()
                                                    if model.Workspace = EditorWorkspace && event.shiftKey then
                                                        dispatch (EditorChanged(ToggleEditorUnitSelection visual.Id))
                                                    else invoke commandId)
                                        | _ -> svg.custom ("aria-hidden", "true")
                                        svg.children [
                                            Svg.rect [
                                                svg.x (presentationX + 5.0)
                                                svg.y (presentationY + 5.0)
                                                svg.width (width - 10.0)
                                                svg.height (depth - 10.0)
                                                svg.rx 6
                                                svg.fill "#101916"
                                                svg.stroke (
                                                    if selected unit.PrimitiveId then "#ffd166"
                                                    else
                                                        match visual.Faction with
                                                        | Human -> "#67b7ff"
                                                        | Arcane -> "#e384ff"
                                                        | Neutral -> "#c9d7d0"
                                                        | OtherFaction _ -> "#f2a65a"
                                                )
                                                svg.strokeWidth (if selected unit.PrimitiveId then 5 else 3)
                                            ]
                                            Svg.g [
                                                svg.custom ("data-unit-glyph", UnitClassId.value visual.ClassId)
                                                svg.custom ("pointer-events", "none")
                                                svg.children [
                                                    glyphView
                                                        ReplayPalettes.accessibleDefault
                                                        (presentationX + width / 2.0)
                                                        (presentationY + depth / 2.0)
                                                        (max 1.0 ((min width depth - 16.0) / 24.0))
                                                        visual.ClassId
                                                ]
                                            ]
                                            match visual.BodyHeading with
                                            | Disclosed heading ->
                                                let centerX = presentationX + width / 2.0
                                                let centerY = presentationY + depth / 2.0
                                                let radians = HeadingRadians.value heading
                                                Svg.line [
                                                    svg.custom ("data-unit-heading", "facing")
                                                    svg.x1 centerX
                                                    svg.y1 centerY
                                                    svg.x2 (centerX + Math.Cos(radians) * 22.0)
                                                    svg.y2 (centerY + Math.Sin(radians) * 22.0)
                                                    svg.stroke "#eef7f2"
                                                    svg.strokeWidth 4
                                                    svg.custom ("pointer-events", "none")
                                                ]
                                            | _ -> ()
                                            match visual.SecondaryHeading with
                                            | Disclosed heading ->
                                                let centerX = presentationX + width / 2.0
                                                let centerY = presentationY + depth / 2.0
                                                let radians = HeadingRadians.value heading.Radians
                                                Svg.line [
                                                    svg.custom ("data-unit-heading", "attention")
                                                    svg.x1 centerX
                                                    svg.y1 centerY
                                                    svg.x2 (centerX + Math.Cos(radians) * 28.0)
                                                    svg.y2 (centerY + Math.Sin(radians) * 28.0)
                                                    svg.stroke "#e5b8ff"
                                                    svg.strokeWidth 3
                                                    svg.custom ("stroke-dasharray", "3 2")
                                                    svg.custom ("pointer-events", "none")
                                                ]
                                            | _ -> ()
                                            match visual.StanceId with
                                            | Disclosed stance ->
                                                Svg.text [
                                                    svg.custom ("data-unit-stance-label", stance)
                                                    svg.x (presentationX + 9.0)
                                                    svg.y (presentationY + depth - 9.0)
                                                    svg.fill "#8ce99a"
                                                    svg.fontSize 10
                                                    svg.text stance
                                                ]
                                            | _ -> ()
                                            Svg.text [
                                                svg.x (presentationX + width - 9.0)
                                                svg.y (presentationY + depth - 9.0)
                                                svg.custom ("text-anchor", "end")
                                                svg.fill "#eef7f2"
                                                svg.fontSize 13
                                                svg.text (string visual.Id)
                                            ]
                                        ]
                                    ]
                            | None -> ()
                        ]
                    ]
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
                    if model.Workspace = EditorWorkspace then
                        let state = model.Editor
                        Svg.g [
                            svg.id "persistent-editor-migrated-layers"
                            svg.custom ("data-editor-renderer", "persistent-svg-v1")
                            svg.children [
                                Svg.g [
                                    svg.custom ("data-editor-layer", "guides")
                                    svg.custom ("display", editorLayerDisplay DocumentDomain state)
                                    svg.custom ("opacity", editorLayerOpacity DocumentDomain state)
                                    svg.custom ("pointer-events", "none")
                                    svg.children [
                                        for column in 0 .. int state.Map.Width do
                                            Svg.line [
                                                svg.x1 (float column * cellSize)
                                                svg.y1 0
                                                svg.x2 (float column * cellSize)
                                                svg.y2 boardHeight
                                                svg.stroke "#52675d"
                                                svg.strokeWidth 1
                                            ]
                                        for row in 0 .. int state.Map.Height do
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
                                        for _, region in state.Map.Regions |> Map.toList do
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

let private tacticalWorkscreenRegion model dispatch =
    let projection, presentationAlpha = activePresentedSceneProjection model
    Html.section [
        prop.id "tactical-workscreen-region"
        prop.className "tactical-workscreen-region"
        prop.ariaLabel "Tactical workscreen region"
        prop.custom ("data-active-modality", string model.Tactical.Modality)
        prop.children [ persistentSceneSvg model projection presentationAlpha dispatch ]
    ]

let private tacticalLayoutToolbar model dispatch =
    let layout = model.TacticalLayout
    let registry = activeTacticalRegistry model
    let commandEntry command =
        let effective = UnifiedTacticalWorkspace.effectiveGesture model.TacticalBindings command
        let overlay =
            TacticalSceneProjection.overlayRegistry
            |> Array.tryFind (fun value -> value.CommandId = command.Id)
        let overlayMode =
            overlay
            |> Option.map (fun value ->
                TacticalSceneProjection.effectiveOverlayMode
                    model.TacticalOverlays
                    model.HeldTacticalOverlays
                    model.TacticalSelectedUnit.IsSome
                    value)
        commandButton [
            prop.type'.button
            prop.custom ("role", if overlay.IsSome then "menuitemcheckbox" else "menuitem")
            prop.tabIndex -1
            prop.disabled (not (tacticalCommandAvailable model command))
            match overlayMode with
            | Some mode ->
                prop.custom ("aria-checked", (string (mode <> OverlayOff)).ToLowerInvariant())
                prop.custom ("data-overlay-mode", string mode)
            | None -> ()
            prop.ariaLabel (command.Label + " · " + UnifiedTacticalWorkspace.displayGestureFor shortcutPlatform effective)
            match UnifiedTacticalWorkspace.accessibleGestureFor shortcutPlatform effective with
            | Some shortcut -> prop.custom ("aria-keyshortcuts", shortcut)
            | None -> prop.custom ("data-binding-state", "unassigned")
            prop.onClick (fun _ ->
                closeDesktopMenus ()
                dispatch (InvokeTacticalCommand command.Id))
            match overlay with
            | Some value when Set.contains InspectHeld value.SupportedModes ->
                prop.onPointerDown (fun _ -> dispatch (BeginTacticalOverlayHold command.Id))
                prop.onPointerUp (fun _ -> dispatch (EndTacticalOverlayHold command.Id))
                prop.onPointerCancel (fun _ -> dispatch (EndTacticalOverlayHold command.Id))
            | _ -> ()
            prop.children [ Html.span command.Label; Html.kbd (UnifiedTacticalWorkspace.displayGestureFor shortcutPlatform effective) ]
        ]
    let menu (label: string) categories =
        let commands = registry |> List.filter (fun command -> List.contains command.Category categories)
        Html.details [
            prop.className "desktop-menu tactical-desktop-menu"
            prop.children [
                Html.summary [
                    prop.role.button
                    prop.text label
                    prop.custom ("data-binding-state", "unassigned")
                    prop.custom ("aria-description", "Unassigned application menu control")
                    prop.onClick (fun event ->
                        closeSiblingDesktopMenus event.target
                        focusNextDesktopMenuItem event.target 0)
                    prop.onKeyDown (fun event ->
                        if event.key = "ArrowDown" then
                            event.preventDefault ()
                            focusNextDesktopMenuItem event.target 1
                        elif event.key = "ArrowUp" then
                            event.preventDefault ()
                            focusNextDesktopMenuItem event.target -1
                        elif event.key = "Escape" then
                            event.preventDefault ()
                            event.stopPropagation ()
                            closeDesktopMenuAndRestoreTrigger event.target)
                ]
                Html.div [
                    prop.className "desktop-menu-popover tactical-desktop-menu-popover"
                    prop.role.menu
                    prop.ariaLabel (label + " commands")
                    prop.onKeyDown (fun event ->
                        if event.key = "ArrowDown" then
                            event.preventDefault ()
                            event.stopPropagation ()
                            focusNextDesktopMenuItem event.target 1
                        elif event.key = "ArrowUp" then
                            event.preventDefault ()
                            event.stopPropagation ()
                            focusNextDesktopMenuItem event.target -1
                        elif event.key = "Escape" then
                            event.preventDefault ()
                            event.stopPropagation ()
                            closeDesktopMenuAndRestoreTrigger event.target)
                    prop.children [ for command in commands do commandEntry command ]
                ]
            ]
        ]
    let panelToggle (panel: TacticalPanelDefinition) =
        let placement =
            layout.Placements
            |> List.find (fun placement -> placement.PanelId = panel.Id)
        commandButton [
            prop.id ("layout-show-" + panel.Id)
            prop.type'.button
            prop.className "tactical-panel-toggle"
            prop.ariaPressed placement.Visible
            prop.ariaLabel (
                (if placement.Visible then "Hide " else "Show ")
                + panel.Label + " panel"
            )
            prop.text panel.Label
            prop.onClick (fun _ ->
                dispatch (ToggleLayoutPanelVisibility panel.Id))
        ]

    Html.header [
        prop.className "tactical-compact-toolbar"
        prop.ariaLabel "Tactical workspace toolbar"
        prop.children [
            Html.nav [
                prop.className "tactical-desktop-menu-bar"
                prop.ariaLabel "Application menu bar"
                prop.custom ("role", "menubar")
                prop.children [
                    menu "File" [ "Document" ]
                    menu "Edit" [ "Plan" ]
                    menu "View" [ "Modality"; "Shared camera"; "View"; "Analysis overlays" ]
                    menu "Tools" [ "Plan"; "Editor" ]
                    menu "Simulation" [ "Timeline"; "Simulator controllers"; "Simulator movement" ]
                    menu "Help" [ "Help" ]
                ]
            ]
            Html.div [
                prop.className "desktop-command-toolbar"
                prop.role.toolbar
                prop.ariaLabel "Customizable top toolbar"
                prop.children [
                    for commandId in model.DesktopToolbarCommands do
                        match registry |> List.tryFind (fun command -> command.Id = commandId) with
                        | Some command ->
                            tacticalCommandButton model command.Id command.Label command.Label (not (Set.contains model.Tactical.Modality command.Modalities && tacticalCommandAvailable model command)) (fun _ -> dispatch (InvokeTacticalCommand command.Id))
                        | None -> ()
                    commandButton [
                        prop.type'.button
                        prop.ariaExpanded model.DesktopToolbarCustomizationOpen
                        prop.ariaControls "desktop-toolbar-customization"
                        prop.text "Customize toolbar"
                        prop.onClick (fun _ -> dispatch ToggleDesktopToolbarCustomization)
                    ]
                ]
            ]
            if model.DesktopToolbarCustomizationOpen then
                Html.section [
                    prop.id "desktop-toolbar-customization"
                    prop.className "desktop-toolbar-customization"
                    prop.ariaLabel "Customize top toolbar"
                    prop.children [
                        Html.h2 "Customize toolbar"
                        Html.p "Add, reorder, or restore toolbar commands."
                        for commandId in model.DesktopToolbarCommands do
                            match registry |> List.tryFind (fun command -> command.Id = commandId) with
                            | Some command ->
                                Html.div [
                                    prop.className "desktop-toolbar-customization-row"
                                    prop.children [
                                        Html.span command.Label
                                        button "Move earlier" ("Move " + command.Label + " earlier") false (fun _ -> dispatch (ReorderDesktopToolbarCommand(commandId, -1)))
                                        button "Move later" ("Move " + command.Label + " later") false (fun _ -> dispatch (ReorderDesktopToolbarCommand(commandId, 1)))
                                        button "Remove" ("Remove " + command.Label + " from toolbar") false (fun _ -> dispatch (RemoveDesktopToolbarCommand commandId))
                                    ]
                                ]
                            | None -> ()
                        Html.h3 "Available commands"
                        for command in registry |> List.filter (fun command -> not (List.contains command.Id model.DesktopToolbarCommands)) |> List.filter (fun command -> command.PointerAvailable) do
                            button ("Add " + command.Label) ("Add " + command.Label + " to toolbar") false (fun _ -> dispatch (AddDesktopToolbarCommand command.Id))
                        button "Reset toolbar" "Restore the documented default top toolbar" false (fun _ -> dispatch ResetDesktopToolbar)
                    ]
                ]
            tacticalModalityControls model dispatch
            Html.details [
                prop.className "tactical-legacy-controls"
                prop.children [
                    Html.summary [
                        prop.text "Workspace controls"
                        prop.custom ("data-binding-state", "unassigned")
                        prop.custom ("aria-description", "Unassigned workspace control disclosure")
                    ]
                    Html.div [
                        prop.className "tactical-legacy-controls-popover"
                        prop.children [
            Html.div [
                prop.className "tactical-document-identity"
                prop.children [
                    Html.strong model.Editor.Authoring.Name
                    Html.span (
                        " · r" + string model.Editor.Revision.Number
                        + (if model.Editor.RevisionState = SavedRevision then " · saved" else " · modified")
                    )
                ]
            ]
            Html.div [
                prop.className "tactical-toolbar-transport"
                prop.children [
                    tacticalCommandButton
                        model
                        "timeline.play-toggle"
                        (if model.Tactical.IsPlaying then "Pause" else "Play")
                        "Play or pause active tactical modality"
                        false
                        (fun _ -> dispatch (InvokeTacticalCommand "timeline.play-toggle"))
                ]
            ]
            Html.div [
                prop.className "tactical-toolbar-layout"
                prop.ariaLabel "Panel visibility"
                prop.children [
                    commandButton [
                        prop.id "layout-left-drawer-toggle"
                        prop.type'.button
                        prop.className "tactical-drawer-toggle"
                        prop.ariaExpanded layout.LeftSidebar.DrawerOpen
                        prop.ariaControls "tactical-sidebar-left"
                        prop.text "Left"
                        prop.onClick (fun _ -> dispatch (ToggleLayoutDrawer Left))
                    ]
                    commandButton [
                        prop.id "layout-right-drawer-toggle"
                        prop.type'.button
                        prop.className "tactical-drawer-toggle"
                        prop.ariaExpanded layout.RightSidebar.DrawerOpen
                        prop.ariaControls "tactical-sidebar-right"
                        prop.text "Right"
                        prop.onClick (fun _ -> dispatch (ToggleLayoutDrawer Right))
                    ]
                    commandButton [
                        prop.id "layout-timeline-visibility-toggle"
                        prop.type'.button
                        prop.ariaPressed (
                            TacticalWorkspaceLayout.bottomVisible layout
                        )
                        prop.ariaLabel (
                            (if TacticalWorkspaceLayout.bottomVisible layout then
                                 "Hide"
                             else
                                 "Show")
                            + " timeline panel"
                        )
                        prop.text "Timeline"
                        prop.onClick (fun _ ->
                            dispatch ToggleLayoutBottomPanelVisibility)
                    ]
                    commandButton [
                        prop.id "layout-timeline-toggle"
                        prop.type'.button
                        prop.disabled (
                            not (TacticalWorkspaceLayout.bottomVisible layout)
                        )
                        prop.ariaExpanded (
                            not (TacticalWorkspaceLayout.bottomCollapsed model.Tactical.Modality layout)
                        )
                        prop.ariaControls "tactical-bottom-panel"
                        prop.text (
                            if TacticalWorkspaceLayout.bottomCollapsed model.Tactical.Modality layout then
                                "Expand timeline"
                            else
                                "Collapse timeline"
                        )
                        prop.onClick (fun _ -> dispatch ToggleLayoutBottomPanel)
                    ]
                    commandButton [
                        prop.id "layout-reset"
                        prop.type'.button
                        prop.text "Reset layout"
                        prop.onClick (fun _ -> dispatch ResetTacticalLayout)
                    ]
                ]
            ]
            supportingPanelControls model dispatch
            Html.details [
                prop.className "tactical-panel-menu"
                prop.children [
                    Html.summary [
                        prop.text "Panels"
                        prop.custom ("data-binding-state", "unassigned")
                        prop.custom ("aria-description", "Unassigned panel control disclosure")
                    ]
                    Html.div [
                        prop.className "tactical-panel-menu-items"
                        prop.children [
                            for panel in TacticalWorkspaceLayout.panelRegistry do
                                panelToggle panel
                        ]
                    ]
                ]
            ]
            tacticalCommandButton
                model
                "input.help"
                "Actions"
                "Show contextual tactical actions"
                false
                (fun _ -> dispatch (InvokeTacticalCommand "input.help"))
                        ]
                    ]
                ]
            ]
        ]
    ]

let private editorLayerPanel (state: MapEditorState) dispatch =
    Html.div [
        prop.id "editor-layer-controls"
        prop.tabIndex 0
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
                        commandButton [
                            prop.type'.button
                            prop.text label
                            prop.ariaPressed (MapEditor.layerState domain state = value)
                            prop.onClick (fun _ ->
                                dispatch (EditorChanged(SetEditorLayerState(domain, value))))
                        ]
                ]
        ]
    ]

let private editorValidationPanel (state: MapEditorState) dispatch =
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
                    prop.text (string (index + 1) + " of " + string state.Issues.Length + " · " + issue.Code + " · " + issue.Message)
                ]
            | _ -> Html.p "No validation issues."
        ]
    ]

let private editorOutlinerPanel (state: MapEditorState) dispatch =
    Html.div [
        prop.ariaLabel "Editor object outliner"
        prop.children [
            Html.h4 ("Units · " + string state.Map.Units.Count)
            for _, unit in state.Map.Units |> Map.toList do
                commandButton [
                    prop.type'.button
                    prop.ariaPressed (Set.contains unit.Id state.SelectedUnits)
                    prop.text (unit.ClassId + " " + string unit.Id + " · " + string unit.Column + "," + string unit.Row)
                    prop.onClick (fun _ -> dispatch (EditorChanged(SelectEditorUnit(Some unit.Id))))
                ]
            Html.h4 ("Regions · " + string state.Map.Regions.Count)
            for _, region in state.Map.Regions |> Map.toList do
                commandButton [
                    prop.type'.button
                    prop.ariaPressed (state.SelectedRegion = Some region.Id)
                    prop.text ("Region " + string region.Id + " · " + MapEditor.regionPurposeLabel region.Purpose)
                    prop.onClick (fun _ -> dispatch (EditorChanged(SelectEditorRegion(Some region.Id))))
                ]
        ]
    ]

let private simulatorPanelBody
    (editor: MapEditorState)
    (handoff: SimulatorHandoff)
    (selectedUnit: int32 option)
    panelId
    dispatch
    =
    let state = MapEditorSimulator.viewState selectedUnit handoff
    let stale = MapEditorSimulator.isBehindDraft editor handoff
    match panelId with
    | "roster" ->
        Html.div [
            prop.ariaLabel "Simulator runtime roster"
            prop.children [
                Html.h4 ("Disposable runtime units · " + string handoff.RuntimeMap.Units.Count)
                for _, unit in handoff.RuntimeMap.Units |> Map.toList do
                    commandButton [
                        prop.type'.button
                        prop.ariaPressed (Option.contains unit.Id selectedUnit)
                        prop.text (
                            "Unit " + string unit.Id + " · " + unit.ClassId
                            + " · HP " + string unit.Health
                            + " · " + MapEditor.controllerLabel unit.Controller
                        )
                        prop.onClick (fun _ ->
                            dispatch (SimulatorUnitSelectionChanged(Some unit.Id)))
                    ]
            ]
        ]
    | "tools" ->
        Html.section [
            prop.className "simulator-registered-tools"
            prop.ariaLabel "Simulator runtime tools"
            prop.children [
                Html.h4 "Runtime transport"
                Html.p (
                    "Authoritative runtime tick " + string handoff.Tick
                    + " · " + if handoff.IsRunning then "Running" else "Paused"
                )
                match handoff.ReconciliationMessage with
                | Some message -> Html.p [ prop.role "status"; prop.text message ]
                | None -> Html.none
                Html.div [
                    prop.className "control-row simulation-controls"
                    prop.children [
                        button
                            (if handoff.IsRunning then "Pause" else "Run")
                            (if handoff.IsRunning then "Pause map simulation" else "Run map simulation")
                            false
                            (fun _ -> dispatch (InvokeTacticalCommand "simulator.run.toggle-k"))
                        button "Step" "Advance the map simulation one tick" handoff.IsRunning (fun _ ->
                            dispatch (InvokeTacticalCommand "simulator.step"))
                        button "Reset" "Reset simulation to the current authored baseline" handoff.IsRunning (fun _ ->
                            dispatch (InvokeTacticalCommand "simulator.reset.request"))
                    ]
                ]
                Html.h4 "Shared camera"
                Html.div [
                    prop.className "control-row"
                    prop.children [
                        button "−" "Zoom battlefield out" false (fun _ -> dispatch (InvokeTacticalCommand "scene.camera.zoom-out"))
                        button "+" "Zoom battlefield in" false (fun _ -> dispatch (InvokeTacticalCommand "scene.camera.zoom-in"))
                        button "Fit" "Reset battlefield camera" false (fun _ -> dispatch (InvokeTacticalCommand "scene.camera.fit"))
                    ]
                ]
            ]
        ]
    | "layers" ->
        Html.div [
            prop.ariaLabel "Simulator shared layer ownership"
            prop.children [
                Html.p "Runtime positions and controller state use Units."
                Html.p "Queued and preview paths use Routes."
                Html.p "Disclosed runtime events use Annotations."
            ]
        ]
    | "selection" -> controllerPanel handoff state dispatch
    | "validation"
    | "diagnostics" ->
        Html.section [
            prop.className "simulator-registered-diagnostics"
            prop.ariaLabel "Simulator runtime diagnostics"
            prop.children [
                Html.h4 ("Tick " + string handoff.Tick + " diagnostics")
                if List.isEmpty state.LastEvents then
                    Html.p "No actions resolved on the latest authoritative tick."
                else
                    Html.ol [
                        prop.ariaLabel "Latest deterministic simulation events"
                        prop.children [ for event in state.LastEvents do Html.li event ]
                    ]
                if not (List.isEmpty handoff.LastCombatEvents) then
                    Html.h4 "Recent combat"
                    Html.ol [
                        prop.ariaLabel "Recent combat events"
                        prop.children [
                            for combat in handoff.LastCombatEvents do
                                Html.li ("Tick " + string combat.Tick + " · " + combat.Summary)
                        ]
                    ]
                Html.h4 "Disclosure boundary"
                Html.p [
                    prop.role.status
                    prop.text MapEditorSimulator.PerspectiveUnavailableReason
                ]
                Html.p [
                    prop.role.status
                    prop.text MapEditorSimulator.VisibilityUnavailableReason
                ]
            ]
        ]
    | "document" ->
        Html.section [
            prop.className "simulator-registered-revision"
            prop.role.status
            prop.ariaLive.polite
            prop.ariaLabel "Simulator maintained revision state"
            prop.children [
                Html.h4 (
                    if stale then "Simulator retains the last valid editor draft"
                    else "Simulator matches the current editor draft"
                )
                Html.p (
                    "Maintained revision " + string handoff.Revision.Number
                    + " · " + handoff.Revision.Digest.Substring(0, 12)
                )
                Html.p (
                    "Runtime tick " + string handoff.Tick
                    + "; timeline seeking reconstructs deterministic runtime state."
                )
                if stale then
                    Html.p "The draft is invalid; the last valid runtime remains."
                button "Open Editor" "Open the map editor" false (fun _ ->
                    dispatch (WorkspaceChanged EditorWorkspace))
                button "Repository bundle" "Download editor and simulator design work" false (fun _ ->
                    dispatch ExportDesignBundle)
            ]
        ]
    | "samples" ->
        Html.div [
            prop.ariaLabel "Simulator samples"
            prop.children [
                for sample in ExperienceSamples.maps do
                    button sample.Title ("Load simulation sample: " + sample.Summary) false (fun _ ->
                        dispatch (LoadSimulationSample sample.Id))
            ]
        ]
    | _ ->
        Html.p [
            prop.className "tactical-layout-panel-placeholder"
            prop.text "No Simulator capability is assigned to this panel."
        ]

let private tacticalPanelBody panelId model dispatch =
    if panelId = "rules" then
        Html.div [
            prop.ariaLabel "Rules supporting panel"
            prop.children [
                scenarioCatalog model.Shell dispatch
                laboratoryResults model.Shell dispatch
                comparisonPanel model dispatch
                sandbox model.Shell dispatch
                inspector model.Shell dispatch
            ]
        ]
    elif panelId = "data" then
        React.Suspense(
            [ LazyDeferredDataPanel model.Simulator model.SimulatorSelectedUnit model.Live.Bootstrap ],
            fallback = Html.p "Loading authoritative data…"
        )
    elif panelId = "samples" && model.Workspace <> SimulatorWorkspace then
        sampleCatalogView dispatch
    elif model.Workspace = PlanningWorkspace then
        match model.Planning with
        | Some planning when
            panelId = "roster"
            || panelId = "tools"
            || panelId = "selection"
            || panelId = "validation"
            || panelId = "document"
            -> planningPanelBody planning panelId dispatch
        | Some _ ->
            Html.p [
                prop.className "tactical-layout-panel-placeholder"
                prop.text "No Plan capability is assigned to this panel."
            ]
        | None -> Html.p "Planner unavailable for the current map revision."
    elif model.Workspace = SimulatorWorkspace && panelId = "samples" then
        Html.div [
            prop.ariaLabel "Simulator samples"
            prop.children [
                for sample in ExperienceSamples.maps do
                    button sample.Title ("Load simulation sample: " + sample.Summary) false (fun _ ->
                        dispatch (LoadSimulationSample sample.Id))
            ]
        ]
    elif model.Workspace = SimulatorWorkspace then
        match model.Simulator with
        | Some simulator ->
            Html.div [
                if panelId = "tools" then TacticalEnvironmentView.simulationView simulator dispatch
                simulatorPanelBody model.Editor simulator model.SimulatorSelectedUnit panelId dispatch
            ]
        | None ->
            Html.p "Correct the current map so a valid simulation can be maintained."
    elif model.Workspace = EditorWorkspace then
        match panelId with
        | "roster" -> editorOutlinerPanel model.Editor dispatch
        | "tools" ->
            let activePanel =
                match model.EditorToolPanel with
                | DocumentTools -> TerrainTools
                | panel -> panel
            editorToolbar
                model.Editor
                model.TacticalParcelEditor
                model.TacticalParcelImportText
                model.EditorView
                activePanel
                true
                model.ImportAnnouncement
                dispatch
        | "layers" -> editorLayerPanel model.Editor dispatch
        | "selection" -> editorUnitPanel model.Editor dispatch
        | "validation" -> editorValidationPanel model.Editor dispatch
        | "document" ->
            editorToolbar
                model.Editor
                model.TacticalParcelEditor
                model.TacticalParcelImportText
                model.EditorView
                DocumentTools
                true
                model.ImportAnnouncement
                dispatch
        | _ ->
            Html.p [
                prop.className "tactical-layout-panel-placeholder"
                prop.text "No Editor capability is assigned to this panel."
            ]
    elif model.Workspace = ReplayWorkspace then
        reviewPanelBody panelId model.Shell dispatch
    else
        Html.p [
            prop.className "tactical-layout-panel-placeholder"
            prop.text "Panel host reserved for the active modality migration."
        ]

let private tacticalSidebar side model dispatch =
    let sideName = if side = Left then "left" else "right"
    let layout = model.TacticalLayout
    let drawerOpen =
        if side = Left then layout.LeftSidebar.DrawerOpen
        else layout.RightSidebar.DrawerOpen
    let definition panelId =
        TacticalWorkspaceLayout.panelRegistry
        |> List.find (fun panel -> panel.Id = panelId)

    Html.aside [
        prop.id ("tactical-sidebar-" + sideName)
        prop.className (
            "tactical-sidebar tactical-sidebar-" + sideName
            + if drawerOpen then " is-drawer-open" else ""
        )
        prop.ariaLabel ((if side = Left then "Left" else "Right") + " tactical sidebar")
        prop.children [
            for placement in TacticalWorkspaceLayout.panelsOn side layout do
                if placement.Visible then
                    let panel = definition placement.PanelId
                    Html.section [
                        prop.id ("layout-panel-" + panel.Id)
                        prop.ariaLabel (panel.Label + " tactical panel")
                        prop.className (
                            "tactical-layout-panel"
                            + if placement.Collapsed then " is-collapsed" else ""
                        )
                        prop.custom ("data-panel-id", panel.Id)
                        prop.custom ("data-panel-side", sideName)
                        prop.custom ("data-panel-order", string placement.Order)
                        prop.children [
                            Html.header [
                                Html.strong panel.Label
                                Html.div [
                                    prop.className "tactical-layout-panel-actions"
                                    prop.children [
                                        commandButton [
                                            prop.id ("layout-panel-" + panel.Id + "-collapse")
                                            prop.type'.button
                                            prop.ariaExpanded (not placement.Collapsed)
                                            prop.ariaControls ("layout-panel-" + panel.Id + "-body")
                                            prop.ariaLabel (
                                                (if placement.Collapsed then "Expand " else "Collapse ")
                                                + panel.Label + " panel"
                                            )
                                            prop.text (if placement.Collapsed then "Expand" else "Collapse")
                                            prop.onClick (fun _ ->
                                                dispatch (ToggleLayoutPanelCollapsed panel.Id))
                                        ]
                                        commandButton [
                                            prop.type'.button
                                            prop.ariaLabel ("Move " + panel.Label + " panel up")
                                            prop.text "↑"
                                            prop.onClick (fun _ ->
                                                dispatch (ReorderLayoutPanel(panel.Id, -1)))
                                        ]
                                        commandButton [
                                            prop.type'.button
                                            prop.ariaLabel ("Move " + panel.Label + " panel down")
                                            prop.text "↓"
                                            prop.onClick (fun _ ->
                                                dispatch (ReorderLayoutPanel(panel.Id, 1)))
                                        ]
                                        commandButton [
                                            prop.type'.button
                                            prop.ariaLabel (
                                                "Move " + panel.Label + " panel to "
                                                + (if side = Left then "right" else "left")
                                                + " sidebar"
                                            )
                                            prop.text (if side = Left then "→" else "←")
                                            prop.onClick (fun _ ->
                                                dispatch (
                                                    MoveLayoutPanel(
                                                        panel.Id,
                                                        if side = Left then Right else Left
                                                    )
                                                ))
                                        ]
                                        commandButton [
                                            prop.type'.button
                                            prop.ariaLabel ("Hide " + panel.Label + " panel")
                                            prop.text "×"
                                            prop.onClick (fun _ ->
                                                dispatch (ToggleLayoutPanelVisibility panel.Id))
                                        ]
                                    ]
                                ]
                            ]
                            if not placement.Collapsed then
                                Html.div [
                                    prop.id ("layout-panel-" + panel.Id + "-body")
                                    prop.className "tactical-layout-panel-body"
                                    prop.tabIndex -1
                                    prop.children [ tacticalPanelBody panel.Id model dispatch ]
                                ]
                        ]
                    ]
        ]
    ]

let private tacticalShell model dispatch transientContent =
    let layout = model.TacticalLayout
    let bottomVisible = TacticalWorkspaceLayout.bottomVisible layout
    let bottomCollapsed =
        TacticalWorkspaceLayout.bottomCollapsed model.Tactical.Modality layout
    Html.section [
        prop.id "unified-tactical-workspace"
        prop.className "unified-tactical-workspace"
        prop.ariaLabel "Unified tactical workspace"
        prop.custom ("data-mounted-shell", "persistent")
        prop.custom ("data-layout-schema", string layout.SchemaVersion)
        prop.custom ("data-layout-profile", "field-focus")
        prop.style [
            style.custom (
                "--tactical-left-width",
                string layout.LeftSidebar.Width + "px"
            )
            style.custom (
                "--tactical-right-width",
                string layout.RightSidebar.Width + "px"
            )
            style.custom (
                "--tactical-bottom-height",
                string layout.BottomPanel.Height + "px"
            )
        ]
        prop.children [
            tacticalLayoutToolbar model dispatch
            Html.div [
                prop.className "tactical-layout-frame"
                prop.children [
                    tacticalSidebar Left model dispatch
                    tacticalWorkscreenRegion model dispatch
                    tacticalSidebar Right model dispatch
                    Html.section [
                            prop.id "tactical-bottom-panel"
                            prop.hidden (not bottomVisible)
                            prop.ariaHidden (not bottomVisible)
                            prop.className (
                                "tactical-bottom-panel"
                                + if bottomCollapsed then " is-collapsed" else ""
                            )
                            prop.ariaLabel "Tactical bottom panel"
                            prop.children [
                                Html.div [
                                        prop.id "tactical-bottom-panel-resize"
                                        prop.className (
                                            "tactical-bottom-panel-resize"
                                            + if bottomCollapsed then " is-disabled" else ""
                                        )
                                        prop.role.separator
                                        prop.tabIndex (if bottomCollapsed then -1 else 0)
                                        prop.ariaHidden bottomCollapsed
                                        prop.ariaLabel "Resize tactical timeline panel"
                                        prop.custom ("aria-orientation", "horizontal")
                                        prop.ariaValueMin 96
                                        prop.ariaValueMax 480
                                        prop.ariaValueNow layout.BottomPanel.Height
                                        prop.onPointerDown (fun event ->
                                            if not bottomCollapsed then
                                                event.preventDefault ()
                                                capturePointer event.currentTarget (int event.pointerId)
                                                dispatch BeginLayoutBottomPanelResize)
                                        prop.onPointerMove (fun event ->
                                            if model.BottomPanelResizeActive then
                                                dispatch (
                                                    ResizeLayoutBottomPanel(
                                                        int window.innerHeight
                                                        - int event.clientY
                                                    )
                                                ))
                                        prop.onPointerUp (fun event ->
                                            releasePointer event.currentTarget (int event.pointerId)
                                            dispatch EndLayoutBottomPanelResize)
                                        prop.onPointerCancel (fun event ->
                                            releasePointer event.currentTarget (int event.pointerId)
                                            dispatch EndLayoutBottomPanelResize)
                                        prop.onKeyDown (fun event ->
                                            let delta =
                                                match event.key with
                                                | "ArrowUp" -> Some 16
                                                | "ArrowDown" -> Some -16
                                                | "PageUp" -> Some 64
                                                | "PageDown" -> Some -64
                                                | "Home" -> Some(96 - layout.BottomPanel.Height)
                                                | "End" -> Some(480 - layout.BottomPanel.Height)
                                                | _ -> None
                                            delta
                                            |> Option.iter (fun value ->
                                                event.preventDefault ()
                                                event.stopPropagation ()
                                                dispatch (ResizeLayoutBottomPanelKeyboard value)))
                                ]
                                Html.div [
                                    prop.className "tactical-bottom-panel-content"
                                    prop.hidden bottomCollapsed
                                    prop.ariaHidden bottomCollapsed
                                    prop.children [ tacticalTimeline model dispatch ]
                                ]
                            ]
                    ]
                ]
            ]
            Html.div [
                prop.className "tactical-transient-layer"
                prop.children [ transientContent ]
            ]
            if not (List.isEmpty model.TacticalLayoutDiagnostics) then
                Html.p [
                    prop.className "tactical-layout-diagnostics"
                    prop.role.status
                    prop.text (String.concat " " model.TacticalLayoutDiagnostics)
                ]
            tacticalContextHelp model dispatch
            tacticalBindingDialog model dispatch
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
    _
    _
    =
    Html.section [
        prop.className "modal-input-state-strip"
        prop.ariaLabel "Current input mode"
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
            Html.p [
                prop.className "sr-only"
                prop.role.status
                prop.ariaLive.polite
                prop.ariaAtomic true
                prop.text projection.Headline
            ]
        ]
    ]

let view model dispatch =
    let shell = model.Shell
    let transientContent =
        match model.Workspace with
        | PlanningWorkspace when model.Planning.IsNone ->
            Html.section [
                prop.className "panel"
                prop.children [
                    Html.h2 "Planner unavailable"
                    Html.p "Open Plan to create a revision from this map."
                ]
            ]
        | EditorWorkspace ->
            let facts =
                { Editor = model.Editor
                  ActiveDomain = TacticalEnvironmentView.editorDomain model.EditorToolPanel
                  PanHeld = editorPanHeld model
                  InputHelpExpanded = model.InputHelpExpanded }
            let catalog = ModalInput.editorCatalog facts
            let projection = ModalInput.projectEditor facts catalog
            Html.div [
                prop.children [
                    editorDestructiveConfirmation model.Editor dispatch
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
                        prop.className "editor-owner-status"
                        prop.ariaLabel "Editor authoritative status"
                        prop.children [
                            Html.p [ prop.className "sr-only"; prop.ariaLive.polite; prop.text model.Editor.TerrainAnnouncement ]
                            Html.p [ prop.className "sr-only"; prop.ariaLive.polite; prop.text model.Editor.UnitAnnouncement ]
                            Html.p [ prop.className "sr-only"; prop.ariaLive.polite; prop.text model.Editor.EdgeAnnouncement ]
                            Html.p [ prop.className "sr-only"; prop.ariaLive.polite; prop.text model.Editor.RegionAnnouncement ]
                            modalInputStrip projection model.InputHelpExpanded dispatch
                        ]
                    ]
                ]
            ]
        | PlanningWorkspace
        | SimulatorWorkspace
        | ReplayWorkspace -> Html.none

    Html.main [
        prop.className "app-shell"
        prop.ariaLabel "S.I.R. simulator and editor"
        prop.onClick (fun event ->
            dismissDesktopMenus event.target)
        prop.children [
            tacticalShell model dispatch transientContent
            Html.section [
                prop.id "sir-live-session"
                prop.className "panel"
                prop.ariaLabel "Authoritative live session"
                prop.custom ("data-status", model.Live.Status)
                prop.custom ("data-tick", model.Live.Snapshot |> Option.map _.Tick |> Option.defaultValue 0 |> string)
                prop.custom ("data-server-sequence", model.Live.Snapshot |> Option.map _.ServerSequence |> Option.defaultValue 0 |> string)
                prop.custom ("data-resync-count", string model.Live.ResyncCount)
                prop.custom ("data-session-id", model.Live.Bootstrap |> Option.map _.SessionId |> Option.defaultValue "")
                prop.children [
                    Html.h2 "Authoritative live session"
                    Html.p ("live " + model.Live.Status)
                    Html.p (
                        "Authoritative live tick "
                        + string (model.Live.Snapshot |> Option.map _.Tick |> Option.defaultValue 0)
                    )
                    button "Advance live session" "Send the next player-visible live advance command" (model.Live.Connection.IsNone) (fun _ -> dispatch AdvanceLiveSession)
                    button "Disconnect live session" "Disconnect the player-visible live session" (model.Live.Connection.IsNone) (fun _ -> dispatch DisconnectLiveSession)
                    button "Reconnect live session" "Reconnect and request the authoritative live snapshot" (model.Live.Connection.IsNone) (fun _ -> dispatch ReconnectLiveSession)
                ]
            ]
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
