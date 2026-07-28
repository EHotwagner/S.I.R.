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
    | FileSelected of File
    | PlaybackPulse
    | KeyPressed of string

let private fileBytes (file: File) =
    async {
        let! buffer = file.arrayBuffer () |> Async.AwaitPromise
        let typed = JS.Constructors.Uint8Array.Create(buffer)
        return file.name, Array.init typed.length (fun index -> typed[index])
    }

let private runEffect effect =
    match effect with
    | Run(operation, request) ->
        Cmd.OfAsync.perform
            Runner.execute
            request
            (fun response -> ShellMsg(RunnerResponded(operation, response)))

let private effectsToCmd effects =
    effects |> List.map runEffect |> Cmd.batch

let init () =
    Shell.init (), Cmd.none

let rec update msg model =
    match msg with
    | FileSelected file ->
        model,
        Cmd.OfAsync.perform
            fileBytes
            file
            (fun (name, bytes) -> ShellMsg(ReplayBytesSelected(name, bytes)))
    | ShellMsg shellMsg ->
        let next, effects = Shell.update shellMsg model
        next, effectsToCmd effects
    | PlaybackPulse ->
        let next, effects = Shell.playbackTick model
        next, effectsToCmd effects
    | KeyPressed key ->
        match key with
        | " "
        | "k"
        | "K" -> update (ShellMsg TogglePlayback) model
        | "ArrowRight" -> update (ShellMsg StepForward) model
        | "Escape" -> update (ShellMsg CancelRequested) model
        | _ -> model, Cmd.none

let subscriptions model =
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
            match model.Playback.Speed with
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

    [ [ "keyboard" ], keyboard
      if model.Playback.IsPlaying then
          let speedKey =
              match model.Playback.Speed with
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
    | NotLoaded -> "No replay loaded", "status-neutral"
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
        prop.role.status
        prop.ariaLive.polite
        prop.children [
            Html.strong text
            Html.span [
                prop.className "status-detail"
                prop.text " Browser verification replays accepted kernel inputs; it does not re-run player WASM."
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
    let unavailable = model.Playback.FinalTick <= 0

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
                        "Step"
                        "Advance one replay step"
                        (unavailable || atEnd)
                        (fun _ -> dispatch (ShellMsg StepForward))
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
                prop.text "Keyboard: Space or K plays/pauses; Right Arrow steps; Escape cancels."
            ]
        ]
    ]

let private inspector model dispatch =
    Html.section [
        prop.className "panel inspector-panel"
        prop.ariaLabel "Replay inspector"
        prop.children [
            Html.h2 "Inspector"
            Html.p "Selection state is independent from the authoritative kernel."
            Html.div [
                prop.className "control-row"
                prop.children [
                    button
                        "Unit 10"
                        "Inspect unit 10"
                        false
                        (fun _ -> dispatch (ShellMsg(UnitSelected(Some 10))))
                    button
                        "Event 1"
                        "Inspect event 1"
                        false
                        (fun _ -> dispatch (ShellMsg(EventSelected(Some 1))))
                    button
                        "Attack formula"
                        "Inspect attack formula"
                        false
                        (fun _ ->
                            dispatch (
                                ShellMsg(FormulaSelected(Some "attack"))
                            ))
                ]
            ]
            Html.dl [
                Html.dt "Unit"
                Html.dd (model.Selection.Unit |> Option.map string |> Option.defaultValue "None")
                Html.dt "Event"
                Html.dd (model.Selection.Event |> Option.map string |> Option.defaultValue "None")
                Html.dt "Formula"
                Html.dd (model.Selection.Formula |> Option.defaultValue "None")
            ]
        ]
    ]

let private sandbox model dispatch =
    let current =
        model.Patch |> Map.tryFind "attack-power" |> Option.defaultValue 25

    Html.section [
        prop.className "panel sandbox-panel"
        prop.ariaLabel "Sandbox parameters"
        prop.children [
            Html.h2 "Sandbox fork"
            Html.p "The first edit permanently changes this loaded run from verified replay to a derived sandbox."
            Html.label [
                prop.htmlFor "attack-power"
                prop.text ("Attack power: " + string current)
            ]
            Html.input [
                prop.id "attack-power"
                prop.type'.range
                prop.min 1
                prop.max 100
                prop.value current
                prop.disabled (
                    match model.Mode with
                    | VerifiedReplay
                    | SandboxFork _
                    | ScenarioSandbox _ -> false
                    | NoRun
                    | PerspectivePlayback -> true
                )
                prop.onChange (fun (value: int) ->
                    dispatch (
                        ShellMsg(ParameterEdited("attack-power", int32 value))
                    ))
            ]
        ]
    ]

let view model dispatch =
    Html.main [
        prop.className "app-shell"
        prop.children [
            Html.header [
                Html.p [ prop.className "eyebrow"; prop.text "S.I.R. rules laboratory" ]
                Html.h1 "Replay shell"
                Html.p "Inspect deterministic replay state without granting this browser authority."
            ]
            statusView model
            Html.div [
                prop.className "dashboard"
                prop.children [
                    sourcePanel dispatch
                    controls model dispatch
                    inspector model dispatch
                    sandbox model dispatch
                ]
            ]
            Html.p [
                prop.className "sr-only"
                prop.role.status
                prop.ariaLive.polite
                prop.text model.Announcement
            ]
        ]
    ]

Program.mkProgram init update view
|> Program.withSubscription subscriptions
|> Program.withReactSynchronous "sir-replay-app"
|> Program.run
