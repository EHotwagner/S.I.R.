module SIR.Client.Web.BrowserInfrastructure

open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open SIR.Client
open Elmish

let fileBytes (file: File) =
    async {
        let! buffer = file.arrayBuffer () |> Async.AwaitPromise
        let typed = JS.Constructors.Uint8Array.Create(buffer)
        return file.name, Array.init typed.length (fun index -> typed[index])
    }

let fileText (file: File) =
    async {
        let! text = file.text () |> Async.AwaitPromise
        return file.name, text
    }

let rasterBytes (file: File) =
    async {
        let! buffer = file.arrayBuffer () |> Async.AwaitPromise
        let typed = JS.Constructors.Uint8Array.Create(buffer)
        return file.name, file.``type``, Array.init typed.length (fun index -> typed[index])
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
