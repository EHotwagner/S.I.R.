module SIR.Client.Web.BrowserInfrastructure

open System
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open SIR.Client
open Elmish

let private sizeError label maximum (file: File) =
    try
        let size = float file.size
        if Double.IsNaN size || Double.IsInfinity size || size < 0.0 || size <> floor size then
            Error(label + " has invalid size metadata; the allowed maximum is " + string maximum + " bytes.")
        elif size > float maximum then
            Error(label + " is " + string (int64 size) + " bytes; the allowed maximum is " + string maximum + " bytes.")
        else Ok()
    with _ ->
        Error(label + " has unreadable size metadata; the allowed maximum is " + string maximum + " bytes.")

let fileBytes maximum (file: File) =
    async {
        match sizeError "Replay package" maximum file with
        | Error error -> return Error error
        | Ok() ->
            try
                let! buffer = file.arrayBuffer () |> Async.AwaitPromise
                let typed = JS.Constructors.Uint8Array.Create(buffer)
                return Ok(file.name, Array.init typed.length (fun index -> typed[index]))
            with error -> return Error("Replay package could not be read: " + error.Message)
    }

let fileText maximum (file: File) =
    async {
        match sizeError "Map import" maximum file with
        | Error error -> return Error error
        | Ok() ->
            try
                let! text = file.text () |> Async.AwaitPromise
                return Ok(file.name, text)
            with error -> return Error("Map import could not be read: " + error.Message)
    }

let rasterBytes maximum (file: File) =
    async {
        match sizeError "Raster background" maximum file with
        | Error error -> return Error error
        | Ok() ->
            try
                let! buffer = file.arrayBuffer () |> Async.AwaitPromise
                let typed = JS.Constructors.Uint8Array.Create(buffer)
                return Ok(file.name, file.``type``, Array.init typed.length (fun index -> typed[index]))
            with error -> return Error("Raster background could not be read: " + error.Message)
    }

[<Emit("window.localStorage.getItem('sir.map-editor.autosave.v1')")>]
let readMapAutosave () : string = jsNative

[<Emit("window.localStorage.getItem('sir.tactical-bindings.v1')")>]
let readTacticalBindings () : string = jsNative

[<Emit("window.localStorage.setItem('sir.tactical-bindings.v1', $0)")>]
let writeTacticalBindings (_: string) : unit = jsNative

[<Emit("window.localStorage.getItem('sir.tactical-layout.v1')")>]
let readTacticalLayout () : string = jsNative

[<Emit("window.localStorage.setItem('sir.tactical-layout.v1', $0)")>]
let writeTacticalLayout (_: string) : unit = jsNative

[<Emit("window.localStorage.getItem('sir.desktop-toolbar.v1')")>]
let readDesktopToolbar () : string = jsNative

[<Emit("window.localStorage.setItem('sir.desktop-toolbar.v1', $0)")>]
let writeDesktopToolbar (_: string) : unit = jsNative

let downloadExperiment report =
    let content = Lab.export report
    emitJsStatement content """
        const blob = new Blob([$0], { type: "text/plain;charset=utf-8" });
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = "sir-lab-experiment.sir-lab";
        anchor.click();
        URL.revokeObjectURL(url);
        """

let downloadMap state =
    let content = MapEditor.export state
    emitJsStatement content """
        const blob = new Blob([$0], { type: "text/plain;charset=utf-8" });
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = "battlefield.sir-map";
        anchor.click();
        URL.revokeObjectURL(url);
        """

let runEffects effects =
    effects
    |> List.iter (function
        | Run(operation, request) -> Runner.post operation request)

let effectsToCmd effects =
    Cmd.ofEffect (fun _ -> runEffects effects)
