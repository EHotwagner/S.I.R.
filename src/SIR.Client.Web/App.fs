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

type Msg =
    | ShellMsg of SIR.Client.Msg
    | BattlefieldChanged of BattlefieldAction
    | FileSelected of File
    | MapFileSelected of File
    | PlaybackPulse
    | EditorPulse
    | KeyPressed of key: string * controlOrMeta: bool * shift: bool
    | KeyReleased of string
    | WorkspaceChanged of WorkspaceMode
    | EditorToolPanelChanged of EditorToolPanel
    | EditorWorkspaceChanged of EditorWorkspaceAction
    | EditorChanged of MapEditorAction
    | ExportMap
    | ExportExperiment
    | AddComparisonBookmark
    | ComparisonViewChanged of ComparisonView
    | ExportEvidenceSvg
    | ExportEvidencePng

and WorkspaceMode =
    | SimulatorWorkspace
    | EditorWorkspace
    | ReplayWorkspace
    | RulesWorkspace

and EditorToolPanel =
    | TerrainTools
    | UnitTools
    | EdgeTools
    | DocumentTools

type Model =
    { Shell: SIR.Client.Model
      Editor: MapEditorState
      Workspace: WorkspaceMode
      EditorToolPanel: EditorToolPanel
      EditorView: EditorWorkspaceState
      EditorSpacePressed: bool
      Battlefield: BattlefieldViewState
      PreviousFrame: RenderFrame option
      PresentationAlpha: float
      ComparisonBookmarks: ComparisonBookmark list
      ComparisonView: ComparisonView }

[<Emit("window.matchMedia('(prefers-reduced-motion: reduce)').matches")>]
let private prefersReducedMotion: bool = jsNative

[<Emit("$0 instanceof HTMLInputElement || $0 instanceof HTMLTextAreaElement || $0 instanceof HTMLSelectElement")>]
let private isTextEntryTarget (target: EventTarget) : bool = jsNative

let private fileBytes (file: File) =
    async {
        let! buffer = file.arrayBuffer () |> Async.AwaitPromise
        let typed = JS.Constructors.Uint8Array.Create(buffer)
        return file.name, Array.init typed.length (fun index -> typed[index])
    }

let private fileText (file: File) =
    async {
        let! text = file.text () |> Async.AwaitPromise
        return text
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

let init () =
    let editor = MapEditor.initial
    let editorView =
        MapEditorWorkspace.initial prefersReducedMotion
        |> MapEditorWorkspace.update
            editor.Map
            (MapEditor.selected editor)
            FitEditorBoard

    { Shell = Shell.init ()
      Editor = editor
      Workspace = SimulatorWorkspace
      EditorToolPanel = TerrainTools
      EditorView = editorView
      EditorSpacePressed = false
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
        model,
        Cmd.OfAsync.perform
            fileBytes
            file
            (fun (name, bytes) -> ShellMsg(ReplayBytesSelected(name, bytes)))
    | MapFileSelected file ->
        model,
        Cmd.OfAsync.perform
            fileText
            file
            (fun text -> EditorChanged(LoadMapText text))
    | WorkspaceChanged workspace ->
        let editor =
            if workspace = SimulatorWorkspace then
                MapEditor.update MarkEditorSimulated model.Editor
            elif workspace = EditorWorkspace then
                MapEditor.update RestoreEditorDraft model.Editor
            else
                MapEditor.update CancelEditorGesture model.Editor
        let editorView =
            if workspace = EditorWorkspace then
                model.EditorView
            else
                MapEditorWorkspace.update
                    model.Editor.Map
                    (MapEditor.selected model.Editor)
                    CancelEditorPointers
                    model.EditorView
        { model with
            Editor = editor
            Workspace = workspace
            EditorView = editorView
            EditorSpacePressed = false },
        Cmd.none
    | EditorToolPanelChanged panel ->
        { model with EditorToolPanel = panel }, Cmd.none
    | EditorWorkspaceChanged action ->
        let editorView =
            MapEditorWorkspace.update
                model.Editor.Map
                (MapEditor.selected model.Editor)
                action
                model.EditorView
        { model with EditorView = editorView }, Cmd.none
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
            Battlefield.reconcile (MapEditor.frame editor) model.Battlefield
        { model with
            Editor = editor
            EditorView = editorView
            Battlefield = battlefield
            PreviousFrame = None
            PresentationAlpha = 1.0 },
        Cmd.none
    | EditorPulse ->
        if model.Editor.IsRunning then
            update (EditorChanged StepEditor) model
        else
            model, Cmd.none
    | ExportMap ->
        let editor = MapEditor.update MarkEditorSaved model.Editor
        { model with Editor = editor }, Cmd.ofEffect (fun _ -> downloadMap editor)
    | ShellMsg shellMsg ->
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
        { model with
            Battlefield = battlefield
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
        if
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
    | KeyPressed(key, controlOrMeta, shift) ->
        if model.Workspace = EditorWorkspace then
            match key, controlOrMeta, shift with
            | ("z" | "Z"), true, true -> update (EditorChanged RedoEditorCommand) model
            | ("z" | "Z"), true, false -> update (EditorChanged UndoEditorCommand) model
            | ("y" | "Y"), true, _ -> update (EditorChanged RedoEditorCommand) model
            | ("c" | "C"), true, _ -> update (EditorChanged CopyEditorSelection) model
            | ("v" | "V"), true, _ -> update (EditorChanged PasteEditorClipboard) model
            | ("d" | "D"), true, _ -> update (EditorChanged DuplicateEditorSelection) model
            | ("a" | "A"), true, _ -> update (EditorChanged SelectAllInActiveDomain) model
            | ("Delete" | "Backspace"), false, _ -> update (EditorChanged DeleteEditorSelection) model
            | " ", false, _ -> { model with EditorSpacePressed = true }, Cmd.none
            | "1", false, true -> update (EditorChanged(ChooseTerrain Open)) model
            | "2", false, true -> update (EditorChanged(ChooseTerrain Rough)) model
            | "3", false, true -> update (EditorChanged(ChooseTerrain Blocked)) model
            | "4", false, true -> update (EditorChanged(ChooseTerrain Objective)) model
            | "!", false, _ -> update (EditorChanged(ChooseTerrain Open)) model
            | "@", false, _ -> update (EditorChanged(ChooseTerrain Rough)) model
            | "#", false, _ -> update (EditorChanged(ChooseTerrain Blocked)) model
            | "$", false, _ -> update (EditorChanged(ChooseTerrain Objective)) model
            | "0", false, _ -> update (EditorWorkspaceChanged FitEditorBoard) model
            | "1", false, _ -> update (EditorWorkspaceChanged ResetEditorCamera) model
            | ("f" | "F"), false, _ -> update (EditorWorkspaceChanged FrameEditorSelection) model
            | ("v" | "V"), false, _ ->
                model
                |> update (EditorChanged(ChooseTool Select))
                |> fst
                |> fun next -> update (EditorToolPanelChanged TerrainTools) next
            | ("t" | "T"), false, _ -> update (EditorToolPanelChanged TerrainTools) model
            | ("p" | "P"), false, _ -> update (EditorChanged(ChooseTool(Terrain PencilTool))) model
            | ("r" | "R"), false, _ -> update (EditorChanged(ChooseTool(Terrain RectangleTool))) model
            | ("l" | "L"), false, _ -> update (EditorChanged(ChooseTool(Terrain LineTool))) model
            | ("g" | "G"), false, _ -> update (EditorChanged(ChooseTool(Terrain FloodFillTool))) model
            | ("i" | "I"), false, _ -> update (EditorChanged(ChooseTool(Terrain EyedropperTool))) model
            | ("x" | "X"), false, _ -> update (EditorChanged(ChooseTool(Terrain EraseTool))) model
            | ("u" | "U"), false, _ -> update (EditorToolPanelChanged UnitTools) model
            | ("e" | "E"), false, _ -> update (EditorToolPanelChanged EdgeTools) model
            | "Escape", false, _ ->
                model
                |> update (EditorWorkspaceChanged CancelEditorPointers)
                |> fst
                |> fun next ->
                    if next.Editor.Gesture <> IdleGesture then
                        update (EditorChanged CancelEditorGesture) next
                    else
                        update (EditorChanged(SelectEditorUnit None)) next
            | _ -> model, Cmd.none
        elif model.Workspace = ReplayWorkspace then
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
        else
            model, Cmd.none
    | KeyReleased key ->
        if key = " " then
            { model with EditorSpacePressed = false }, Cmd.none
        else
            model, Cmd.none
    | ExportExperiment ->
        model,
        (model.Shell.Lab.Report
         |> Option.map (fun report -> Cmd.ofEffect (fun _ -> downloadExperiment report))
         |> Option.defaultValue Cmd.none)
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

    let keyboard dispatch =
        let downHandler =
            fun (event: Event) ->
                let keyboardEvent: KeyboardEvent = unbox event
                if not (isTextEntryTarget keyboardEvent.target) then
                    if model.Workspace = EditorWorkspace && keyboardEvent.key = " " then
                        keyboardEvent.preventDefault ()
                    dispatch (
                        KeyPressed(
                            keyboardEvent.key,
                            keyboardEvent.ctrlKey || keyboardEvent.metaKey,
                            keyboardEvent.shiftKey
                        )
                    )
        let upHandler =
            fun (event: Event) ->
                let keyboardEvent: KeyboardEvent = unbox event
                dispatch (KeyReleased keyboardEvent.key)
        window.addEventListener ("keydown", downHandler)
        window.addEventListener ("keyup", upHandler)

        { new IDisposable with
            member _.Dispose() =
                window.removeEventListener ("keydown", downHandler)
                window.removeEventListener ("keyup", upHandler) }

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
            window.setInterval ((fun () -> dispatch EditorPulse), 500)

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
      if model.Editor.IsRunning && model.Workspace = SimulatorWorkspace then
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

let private statusView model =
    let text, className = status model

    Html.section [
        prop.className ("verification-banner " + className)
        prop.ariaLabel "Replay verification status"
        prop.role.status
        prop.ariaLive.polite
        prop.children [
            Html.strong text
            Html.span [
                prop.className "status-detail"
                prop.text " Browser verification replays accepted kernel inputs; it does not re-run player WASM."
            ]
            Html.span [
                prop.className "status-detail"
                prop.text " Authoritative verification is available only from .NET exact-artifact WASM re-execution."
            ]
        ]
    ]

let private sourcePanel dispatch =
    Html.section [
        prop.className "panel source-panel"
        prop.ariaLabel "Replay source"
        prop.children [
            Html.h2 "Replay package"
            Html.p "Load a bounded .sirr package. Files stay in this browser session."
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
            let radius = half + 3.0
            let x = projected.SymbolCenterX + Math.Cos(angle) * radius
            let y = projected.SymbolCenterY + Math.Sin(angle) * radius
            let tangentX = -Math.Sin(angle) * 4.0
            let tangentY = Math.Cos(angle) * 4.0
            let tipX = projected.SymbolCenterX + Math.Cos(angle) * (radius + 7.0)
            let tipY = projected.SymbolCenterY + Math.Sin(angle) * (radius + 7.0)
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
                    svg.points points
                    svg.fill faction
                    svg.stroke palette.Canvas
                    svg.strokeWidth 1
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
    (dispatch: Msg -> unit)
    =
    let loadedFrame =
        frameOverride
        |> Option.orElseWith (fun () -> Shell.renderFrame shell)
    let frame =
        loadedFrame
        |> Option.defaultValue Battlefield.representativeFrame
    let scene =
        match previousFrame with
        | Some previous when presentationAlpha < 1.0 ->
            Battlefield.interpolatedScene presentationAlpha previous frame state
        | _ -> Battlefield.scene frame state
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
                                                Svg.line [
                                                    svg.custom ("data-action-trace", string trace.EventId)
                                                    svg.custom ("data-action-kind", trace.Kind)
                                                    svg.x1 trace.SourceX
                                                    svg.y1 trace.SourceY
                                                    svg.x2 trace.TargetX
                                                    svg.y2 trace.TargetY
                                                    svg.stroke scene.Palette.HealthActive
                                                    svg.strokeWidth 2
                                                    svg.strokeDasharray [| 3; 3 |]
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
    (clientX - bounds.left) * view.ViewportWidth / width,
    (clientY - bounds.top) * view.ViewportHeight / height

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
        svg.onPointerDown (fun event -> event.stopPropagation ())
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
            Svg.g [
                svg.custom (
                    "transform",
                    "translate("
                    + string (x + size / 2.0)
                    + " "
                    + string (y + size / 2.0)
                    + ") scale("
                    + string (max 1.0 (size / 48.0))
                    + ") translate(-24 -24)"
                )
                svg.children [
                    glyphView
                        palette
                        24
                        24
                        1.0
                        (UnitClassId.resolve unit.ClassId)
                ]
            ]
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
                dispatch (EditorChanged(ChooseTool(Edge(hit.Direction, kind))))
                dispatch (EditorChanged(ActivateCell(hit.Column, hit.Row))))
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
            Html.div [
                prop.className "editor-camera-controls"
                prop.children [
                    button "Undo" "Undo last editor command (Ctrl or Command Z)" state.UndoHistory.IsEmpty (fun _ ->
                        dispatch (EditorChanged UndoEditorCommand))
                    button "Redo" "Redo last editor command (Ctrl or Command Shift Z)" state.RedoHistory.IsEmpty (fun _ ->
                        dispatch (EditorChanged RedoEditorCommand))
                    button "Copy" "Copy selected units (Ctrl or Command C)" state.SelectedUnits.IsEmpty (fun _ ->
                        dispatch (EditorChanged CopyEditorSelection))
                    button "Paste" "Paste copied units (Ctrl or Command V)" state.Clipboard.IsNone (fun _ ->
                        dispatch (EditorChanged PasteEditorClipboard))
                    button "Duplicate" "Duplicate selected units (Ctrl or Command D)" state.SelectedUnits.IsEmpty (fun _ ->
                        dispatch (EditorChanged DuplicateEditorSelection))
                    button "Delete" "Delete selected units" state.SelectedUnits.IsEmpty (fun _ ->
                        dispatch (EditorChanged DeleteEditorSelection))
                    button "Select all" "Select every unit in the active domain" state.Map.Units.IsEmpty (fun _ ->
                        dispatch (EditorChanged SelectAllInActiveDomain))
                    button "Fit" "Fit board to workspace (0)" false (fun _ ->
                        dispatch (EditorWorkspaceChanged FitEditorBoard))
                    button "100%" "Reset camera to one hundred percent (1)" false (fun _ ->
                        dispatch (EditorWorkspaceChanged ResetEditorCamera))
                    button "Frame" "Frame selected unit (F)" state.SelectedUnit.IsNone (fun _ ->
                        dispatch (EditorWorkspaceChanged FrameEditorSelection))
                    button "−" "Zoom out around workspace center" false (fun _ ->
                        dispatch (
                            EditorWorkspaceChanged(
                                ZoomEditorAt(
                                    view.ViewportWidth / 2.0,
                                    view.ViewportHeight / 2.0,
                                    0.8
                                )
                            )
                        ))
                    button "+" "Zoom in around workspace center" false (fun _ ->
                        dispatch (
                            EditorWorkspaceChanged(
                                ZoomEditorAt(
                                    view.ViewportWidth / 2.0,
                                    view.ViewportHeight / 2.0,
                                    1.25
                                )
                            )
                        ))
                ]
            ]
            Html.p [
                prop.className "sr-only"
                prop.ariaLive.polite
                prop.ariaAtomic true
                prop.text state.TerrainAnnouncement
            ]
            Svg.svg [
                svg.className (
                    "editor-battlefield-svg"
                    + if view.ReducedMotion then " reduced-motion" else ""
                )
                svg.custom ("role", "application")
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
                    if event.ctrlKey || event.metaKey then
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
                    elif event.key = "Delete" || event.key = "Backspace" then
                        event.preventDefault ()
                        event.stopPropagation ()
                        dispatch (EditorChanged DeleteEditorSelection)
                    elif
                        match state.Tool with
                        | Terrain _ -> true
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
                    activateAt x y)
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
                        if requestsSelection then
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
                            selectionAt x y ExtendEditorBoxSelection
                        else
                            match state.Tool with
                            | Terrain _ when not previous.RequestsPan ->
                                terrainAt x y ExtendTerrainGesture
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
                            selectionAt x y ExtendEditorBoxSelection
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
                            Svg.rect [
                                svg.custom ("data-layer", "terrain")
                                svg.width boardWidth
                                svg.height boardHeight
                                svg.fill palette.Terrain
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
                                            svg.x (float column * Battlefield.CellSize)
                                            svg.y (float row * Battlefield.CellSize)
                                            svg.width Battlefield.CellSize
                                            svg.height Battlefield.CellSize
                                            svg.fill fill
                                            svg.custom ("opacity", opacity)
                                        ]
                                        match terrain with
                                        | Rough ->
                                            Svg.line [
                                                svg.custom ("data-terrain-pattern", "diagonal-hatch")
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
                            | _ -> ()
                            Svg.g [
                                svg.custom ("data-layer", "grid")
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
                            Svg.g [
                                svg.custom ("data-layer", "units")
                                svg.children [
                                    for _, unit in state.Map.Units |> Map.toList do
                                        editorUnitSvg state palette dispatch unit
                                ]
                            ]
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
    (activePanel: EditorToolPanel)
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

    let placePreset side presetId =
        MapEditor.tryCanonicalFootprintPreset presetId
        |> Option.map (fun preset ->
            Place(side, preset.ClassId, preset.FootprintSize))
        |> Option.defaultWith (fun () ->
            failwith ("Unknown canonical footprint preset: " + presetId))

    let choosePanel (label: string) (panel: EditorToolPanel) =
        Html.button [
            prop.type'.button
            prop.text label
            prop.ariaPressed (Object.Equals(activePanel, panel))
            prop.onClick (fun _ ->
                dispatch (EditorToolPanelChanged panel))
        ]

    Html.section [
        prop.className "panel editor-tools editor-tool-rail"
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
                    choosePanel "Map file" DocumentTools
                ]
            ]
            Html.div [
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
                        Html.div [
                            prop.className "control-row"
                            prop.children [
                                choose "Blue rifleman" (placePreset Blue "human")
                                choose "Blue medic" (Place(Blue, "medic", 2))
                                choose "Red goblin" (placePreset Red "goblin")
                                choose "Red orc" (placePreset Red "orc")
                                choose "Red troll" (placePreset Red "troll")
                                choose "Neutral drone" (placePreset NeutralSide "drone")
                            ]
                        ]
                    | EdgeTools ->
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
                    | DocumentTools ->
                        Html.h3 "Map document"
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
                                    dispatch (EditorChanged ClearMap))
                                button "Export map" "Export the current map document" false (fun _ ->
                                    dispatch ExportMap)
                                Html.label [
                                    prop.className "map-import"
                                    prop.children [
                                        Html.span "Import map"
                                        Html.input [
                                            prop.type'.file
                                            prop.accept ".sir-map,text/plain"
                                            prop.ariaLabel "Import SIR map"
                                            prop.onChange (fun (files: File list) ->
                                                files
                                                |> List.tryHead
                                                |> Option.iter (MapFileSelected >> dispatch))
                                        ]
                                    ]
                                ]
                            ]
                        ]
                ]
            ]
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
                Html.div [
                    prop.className "unit-properties"
                    prop.children [
                        Html.label [ prop.htmlFor "editor-unit-side"; prop.text "Side" ]
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
                        Html.label [ prop.htmlFor "editor-unit-class"; prop.text "Class ID" ]
                        Html.input [
                            prop.id "editor-unit-class"
                            prop.type'.text
                            prop.value unit.ClassId
                            prop.onChange (fun value ->
                                dispatch (EditorChanged(SetSelectedClass value)))
                        ]
                        Html.label [ prop.htmlFor "editor-unit-size"; prop.text "Square size" ]
                        Html.input [
                            prop.id "editor-unit-size"
                            prop.type'.number
                            prop.min 1
                            prop.max 8
                            prop.value unit.Size
                            prop.onChange (fun (value: int) ->
                                dispatch (EditorChanged(SetSelectedSize(int32 value))))
                        ]
                        Html.label [ prop.htmlFor "editor-unit-health"; prop.text "Current HP" ]
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
                        Html.label [ prop.htmlFor "editor-unit-health-max"; prop.text "Maximum HP" ]
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

let private controllerPanel state dispatch =
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
                    Html.article [ Html.h3 "General AI"; Html.p "Approach the nearest hostile and attack when adjacent." ]
                ]
            ]
            match selected with
            | None ->
                Html.p "Select a unit on the map to configure its controller."
            | Some unit ->
                Html.h3 ("Unit " + string unit.Id + " · " + unit.ClassId)
                Html.div [
                    prop.className "unit-properties"
                    prop.children [
                        Html.label [ prop.htmlFor "unit-side"; prop.text "Side" ]
                        Html.select [
                            prop.id "unit-side"
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
                        Html.label [ prop.htmlFor "unit-class"; prop.text "Class ID" ]
                        Html.input [
                            prop.id "unit-class"
                            prop.type'.text
                            prop.value unit.ClassId
                            prop.onChange (fun value ->
                                dispatch (EditorChanged(SetSelectedClass value)))
                        ]
                        Html.label [ prop.htmlFor "unit-size"; prop.text "Square size" ]
                        Html.input [
                            prop.id "unit-size"
                            prop.type'.number
                            prop.min 1
                            prop.max 8
                            prop.value unit.Size
                            prop.onChange (fun (value: int) ->
                                dispatch (EditorChanged(SetSelectedSize(int32 value))))
                        ]
                        Html.label [ prop.htmlFor "unit-health"; prop.text "Current HP" ]
                        Html.input [
                            prop.id "unit-health"
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
                        Html.label [ prop.htmlFor "unit-health-max"; prop.text "Maximum HP" ]
                        Html.input [
                            prop.id "unit-health-max"
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
                    ]
                ]
                Html.label [ prop.htmlFor "unit-controller"; prop.text "Controller" ]
                Html.select [
                    prop.id "unit-controller"
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
                Html.label [ prop.htmlFor "unit-script"; prop.text "Direction script" ]
                Html.input [
                    prop.id "unit-script"
                    prop.key ("unit-script-" + string unit.Id)
                    prop.type'.text
                    prop.defaultValue (MapEditor.scriptText unit.Script)
                    prop.placeholder "N,E,E,S"
                    prop.onChange (fun value ->
                        dispatch (EditorChanged(SetSelectedScript value)))
                ]
                Html.div [
                    prop.className "manual-movement"
                    prop.children [
                        for label, direction in movement do
                            button label ("Move unit " + label) false (fun _ ->
                                dispatch (EditorChanged(MoveSelected direction)))
                    ]
                ]
                button "Remove unit" "Remove selected unit" false (fun _ ->
                    dispatch (EditorChanged RemoveSelectedUnit))
            Html.div [
                prop.className "control-row simulation-controls"
                prop.children [
                    button
                        (if state.IsRunning then "Pause" else "Run")
                        (if state.IsRunning then "Pause map simulation" else "Run map simulation")
                        false
                        (fun _ -> dispatch (EditorChanged ToggleEditorRun))
                    button "Step" "Advance the map simulation one tick" false (fun _ ->
                        dispatch (EditorChanged StepEditor))
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
            item "Editor" EditorWorkspace
            item "Replay" ReplayWorkspace
            item "Rules and data" RulesWorkspace
        ]
    ]

let view model dispatch =
    let shell = model.Shell

    Html.main [
        prop.className "app-shell"
        prop.ariaLabel "S.I.R. simulator and editor"
        prop.children [
            workspaceNavigation model.Workspace dispatch
            match model.Workspace with
            | SimulatorWorkspace ->
                Html.div [
                    prop.className "simulator-workspace"
                    prop.children [
                        controllerPanel model.Editor dispatch
                        battlefieldView
                            shell
                            (Some(MapEditor.frame model.Editor))
                            (Some model.Editor)
                            model.Battlefield
                            None
                            1.0
                            dispatch
                    ]
                ]
            | EditorWorkspace ->
                Html.div [
                    prop.className "editor-workspace"
                    prop.children [
                        editorToolbar
                            model.Editor
                            model.EditorToolPanel
                            dispatch
                        editorBattlefield
                            model.Editor
                            model.EditorView
                            model.EditorSpacePressed
                            dispatch
                        editorGrid model.Editor dispatch
                        Html.aside [
                            prop.className (
                                "editor-inspector"
                                + if model.EditorView.InspectorCollapsed then
                                    " is-collapsed"
                                  else
                                    ""
                            )
                            prop.ariaLabel "Map editor inspector"
                            prop.children [
                                button
                                    (if model.EditorView.InspectorCollapsed then
                                         "Show inspector"
                                     else
                                         "Hide inspector")
                                    "Toggle selected-object inspector"
                                    false
                                    (fun _ ->
                                        dispatch (
                                            EditorWorkspaceChanged
                                                ToggleEditorInspector
                                        ))
                                if not model.EditorView.InspectorCollapsed then
                                    editorUnitPanel model.Editor dispatch
                            ]
                        ]
                    ]
                ]
            | ReplayWorkspace ->
                Html.div [
                    statusView shell
                    workerStatus shell
                    Html.div [
                        prop.className "dashboard"
                        prop.children [
                            sourcePanel dispatch
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
