module SIR.Client.Web.BrowserInfrastructure

open System
open Browser.Dom
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open SIR.Client
open Elmish
open SIR.Client.Web.FeatureLoader

type private ImportedFeatureIdentity =
    abstract registryVersion: int
    abstract featureId: string
    abstract logicalChunk: string

[<Import("loadFeature", "./feature-loader.js")>]
let private loadFeatureJs
    (_registryVersion: int)
    (_featureId: string)
    (_logicalChunk: string)
    : JS.Promise<ImportedFeatureIdentity> =
    jsNative

[<Emit("navigator.onLine")>]
let private browserOnline: bool = jsNative

let loadClientFeature (identity: ChunkIdentity) =
    async {
        try
            let! imported =
                loadFeatureJs
                    identity.RegistryVersion
                    (value identity.Feature)
                    identity.LogicalChunk
                |> Async.AwaitPromise
            return
                Ok
                    { RegistryVersion = imported.registryVersion
                      Feature =
                        if imported.featureId = value rulesExplorer then rulesExplorer
                        elif imported.featureId = value samples then samples
                        elif imported.featureId = value docs then docs
                        else identity.Feature
                      LogicalChunk = imported.logicalChunk }
        with error ->
            let detail = error.Message
            if not browserOnline then return Error(Offline detail)
            elif detail.Contains("stale-identity", StringComparison.Ordinal) then
                return Error(ImportRejected detail)
            elif
                detail.Contains("Failed to fetch dynamically imported module", StringComparison.OrdinalIgnoreCase)
                || detail.Contains("Importing a module script failed", StringComparison.OrdinalIgnoreCase)
            then
                return Error(MissingChunk detail)
            else return Error(ImportRejected detail)
    }

[<Emit("if (!$0.dataset.sirSamplesRendered) { $0.dataset.sirSamplesRendered = 'true'; globalThis.__sirSamplesFeature.render($0,$1); }")>]
let renderSamplesFeature
    (_root: Element)
    (_dispatch: string -> MapEditorState -> SimulatorHandoff option -> string -> InspectionProjection array -> unit)
    : unit =
    jsNative

[<Emit("""
const blob = new Blob([$0], { type: "text/plain;charset=utf-8" });
const url = URL.createObjectURL(blob);
const anchor = document.createElement("a");
anchor.href = url;
anchor.download = "tactical-environment.sir-parcel";
anchor.click();
URL.revokeObjectURL(url);
""")>]
let downloadTacticalEnvironmentDocument (_content: string) : unit = jsNative

[<Emit("(() => { const t = $0; const tag = t && typeof t.tagName === 'string' ? t.tagName.toLowerCase() : ''; return tag === 'input' || tag === 'button' || tag === 'summary' || (tag === 'a' && t.hasAttribute('href')) || tag === 'textarea' || tag === 'select' || (t && t.isContentEditable); })()")>]
let private isNativeInteractiveTarget (_target: EventTarget) : bool = jsNative

let acceptsGlobalKeyboardTarget target allowModifiedShortcut =
    allowModifiedShortcut || not (isNativeInteractiveTarget target)

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

[<Emit("window.localStorage.getItem('sir.tactical-overlays.v1')")>]
let readTacticalOverlays () : string = jsNative

[<Emit("window.localStorage.setItem('sir.tactical-overlays.v1', $0)")>]
let writeTacticalOverlays (_: string) : unit = jsNative

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
