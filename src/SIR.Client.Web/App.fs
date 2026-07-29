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
    | PlaybackPulse
    | KeyPressed of string
    | ExportExperiment

type Model =
    { Shell: SIR.Client.Model
      Battlefield: BattlefieldViewState }

let private fileBytes (file: File) =
    async {
        let! buffer = file.arrayBuffer () |> Async.AwaitPromise
        let typed = JS.Constructors.Uint8Array.Create(buffer)
        return file.name, Array.init typed.length (fun index -> typed[index])
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

let init () =
    { Shell = Shell.init ()
      Battlefield = Battlefield.initial },
    Cmd.none

let rec update msg model =
    match msg with
    | FileSelected file ->
        model,
        Cmd.OfAsync.perform
            fileBytes
            file
            (fun (name, bytes) -> ShellMsg(ReplayBytesSelected(name, bytes)))
    | ShellMsg shellMsg ->
        let next, effects = Shell.update shellMsg model.Shell
        let battlefield =
            match Shell.renderFrame next with
            | Some frame -> Battlefield.reconcile frame model.Battlefield
            | None when next.Verification = Loading ->
                { model.Battlefield with
                    SelectedUnit = None
                    FocusedUnit = None }
            | None -> model.Battlefield

        { model with
            Shell = next
            Battlefield = battlefield },
        effectsToCmd effects
    | BattlefieldChanged action ->
        let frame =
            Shell.renderFrame model.Shell
            |> Option.defaultValue Battlefield.representativeFrame

        { model with
            Battlefield =
                Battlefield.update
                    frame
                    action
                    model.Battlefield },
        Cmd.none
    | PlaybackPulse ->
        let next, effects = Shell.playbackTick model.Shell
        { model with Shell = next }, effectsToCmd effects
    | KeyPressed key ->
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
    | ExportExperiment ->
        model,
        (model.Shell.Lab.Report
         |> Option.map (fun report -> Cmd.ofEffect (fun _ -> downloadExperiment report))
         |> Option.defaultValue Cmd.none)

let subscriptions model =
    let runner dispatch =
        Runner.subscribe (fun message -> dispatch (ShellMsg message))

    let keyboard dispatch =
        let handler =
            fun (event: Event) ->
                let keyboardEvent: KeyboardEvent = unbox event
                dispatch (KeyPressed keyboardEvent.key)
        window.addEventListener ("keydown", handler)

        { new IDisposable with
            member _.Dispose() =
                window.removeEventListener ("keydown", handler) }

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

    [ [ "replay-worker-v1" ], runner
      [ "keyboard" ], keyboard
      if model.Shell.Playback.IsPlaying then
          let speedKey =
              match model.Shell.Playback.Speed with
              | Half -> "half"
              | Normal -> "normal"
              | Double -> "two"
              | Maximum -> "maximum"

          [ "playback-pulse"; speedKey ], timer ]

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
    (classId: UnitClassId)
    =
    let glyph = UnitGlyphCatalog.resolve classId
    let transform =
        "translate("
        + string (centerX - 12.0)
        + " "
        + string (centerY - 12.0)
        + ")"

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
        match scene.SemanticZoom with
        | Overview -> 24.0
        | Standard
        | Detailed -> 36.0
    let half = symbolSize / 2.0

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
            glyphView palette projected.SymbolCenterX projected.SymbolCenterY unit.ClassId
            if scene.SemanticZoom <> Overview && projected.HealthSegments.IsSome then
                let healthSegments = Option.get projected.HealthSegments
                for index in 0 .. 11 do
                    Svg.rect [
                        svg.custom ("data-health-position", string index)
                        svg.x (projected.SymbolCenterX - 18.0 + float index * 3.0)
                        svg.y (projected.SymbolCenterY + half + 3.0)
                        svg.width 2.0
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
                    Html.dt "Committed tick"
                    Html.dd (string scene.Tick + " (exact; no interpolation)")
                ]
        ]
    ]

let private battlefieldView
    (shell: SIR.Client.Model)
    (state: BattlefieldViewState)
    (dispatch: Msg -> unit)
    =
    let loadedFrame = Shell.renderFrame shell
    let frame = loadedFrame |> Option.defaultValue Battlefield.representativeFrame
    let scene = Battlefield.scene frame state
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
            if Option.isSome loadedFrame then
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
                                if Option.isSome loadedFrame then
                                    "Loaded bounded worker projection"
                                else
                                    "Static demonstration — no replay loaded"
                            )
                        ]
                        Html.h2 (
                            if Option.isSome loadedFrame then
                                "Replay battlefield"
                            else
                                "Six-by-six SVG battlefield demonstration"
                        )
                        Html.p (
                            disclosure
                            + " · tick "
                            + string scene.Tick
                            + " · exact frame, no interpolation · north is up"
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
                            + " battlefield at exact tick "
                            + string scene.Tick
                            + ", "
                            + string scene.Units.Length
                            + " visible units; selected unit "
                            + (scene.SelectedUnit |> Option.map string |> Option.defaultValue "none")
                        ))
                        svg.viewBox (0, 0, 360, 360)
                        svg.children [
                            Svg.title ("Replay battlefield at tick " + string scene.Tick)
                            Svg.desc "Flat orthographic six-by-six battlefield. Arrow keys move unit focus; Enter selects; Escape clears selection."
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
                            Html.section [
                                prop.className "battlefield-legend"
                                prop.ariaLabel "Battlefield legend"
                                prop.children [
                                    Html.h3 "Legend"
                                    Html.ul [
                                        Html.li "Solid / dashed / dotted faction outlines remain distinct in monochrome."
                                        Html.li "Twelve health positions fill from left to right."
                                        Html.li "Perimeter wedge is body facing; the class glyph stays upright."
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

let private inspector model dispatch =
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

let private workerStatus model =
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

let private sandbox model dispatch =
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

let view model dispatch =
    let shell = model.Shell

    Html.main [
        prop.className "app-shell"
        prop.ariaLabel "Replay and rules laboratory application"
        prop.children [
            battlefieldView shell model.Battlefield dispatch
            scenarioCatalog shell dispatch
            laboratoryResults shell dispatch
            statusView shell
            workerStatus shell
            Html.div [
                prop.className "dashboard"
                prop.children [
                    sourcePanel dispatch
                    controls shell dispatch
                    inspector shell dispatch
                    sandbox shell dispatch
                ]
            ]
            rulesDataCatalog
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
