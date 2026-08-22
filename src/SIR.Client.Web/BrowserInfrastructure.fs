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

/// Browser-owned latest-value scheduler for high-frequency presentation work.
/// The returned object owns at most one animation-frame handle.  Hidden-page
/// transitions cancel pending work; the next visible enqueue starts a fresh
/// frame rather than replaying stale presentation state.
type PresentationFrameScheduler<'value> = interface end

[<Emit("""
(() => {
  let frame = 0;
  let pending;
  let hasPending = false;
  let disposed = false;
  let accepted = 0;
  let scheduled = 0;
  const accept = $0;
  const cancel = () => {
    if (frame !== 0) window.cancelAnimationFrame(frame);
    frame = 0;
    pending = undefined;
    hasPending = false;
  };
  const run = () => {
    frame = 0;
    if (disposed || document.visibilityState === 'hidden' || !hasPending) return;
    const value = pending;
    pending = undefined;
    hasPending = false;
    accepted += 1;
    accept(value);
  };
  const schedule = () => {
    if (!disposed && document.visibilityState !== 'hidden' && frame === 0 && hasPending) {
      scheduled += 1;
      frame = window.requestAnimationFrame(run);
    }
  };
  const visibility = () => {
    if (document.visibilityState === 'hidden') cancel();
    else schedule();
  };
  document.addEventListener('visibilitychange', visibility);
  return {
    enqueue(value) { pending = value; hasPending = true; schedule(); },
    flush() {
      if (disposed || !hasPending) return;
      if (frame !== 0) window.cancelAnimationFrame(frame);
      frame = 0;
      const value = pending;
      pending = undefined;
      hasPending = false;
      accepted += 1;
      accept(value);
    },
    cancel,
    dispose() {
      if (disposed) return;
      disposed = true;
      cancel();
      document.removeEventListener('visibilitychange', visibility);
    },
    counters() { return `${scheduled}:${accepted}:${frame === 0 ? 0 : 1}:${hasPending ? 1 : 0}`; }
  };
})()
""")>]
let createPresentationFrameScheduler
    (_accept: 'value -> unit)
    : PresentationFrameScheduler<'value> =
    jsNative

[<Emit("$0.enqueue($1)")>]
let enqueuePresentationFrame
    (_scheduler: PresentationFrameScheduler<'value>)
    (_value: 'value)
    : unit =
    jsNative

[<Emit("$0.flush()")>]
let flushPresentationFrame
    (_scheduler: PresentationFrameScheduler<'value>)
    : unit =
    jsNative

[<Emit("$0.cancel()")>]
let cancelPresentationFrame
    (_scheduler: PresentationFrameScheduler<'value>)
    : unit =
    jsNative

[<Emit("$0.dispose()")>]
let disposePresentationFrameScheduler
    (_scheduler: PresentationFrameScheduler<'value>)
    : unit =
    jsNative

[<Emit("$0.counters()")>]
let presentationFrameCounters
    (_scheduler: PresentationFrameScheduler<'value>)
    : string =
    jsNative

[<Emit("""
(() => {
  const root = document.getElementById('persistent-tactical-svg');
  const camera = document.getElementById('persistent-scene-camera');
  if (!root || !camera) return;
  const panX = Number.isFinite($0) ? $0 : 0;
  const panY = Number.isFinite($1) ? $1 : 0;
  const zoom = Number.isFinite($2) && $2 > 0 ? $2 : 1;
  camera.setAttribute('transform', `translate(${panX} ${panY}) scale(${zoom})`);
  root.dataset.cameraPanX = String(panX);
  root.dataset.cameraPanY = String(panY);
  root.dataset.cameraZoom = String(zoom);
  root.dataset.presentationCamera = `${panX}:${panY}:${zoom}`;
})()
""")>]
let presentTacticalCamera
    (_panX: float)
    (_panY: float)
    (_zoom: float)
    : unit =
    jsNative

[<Emit("""
(() => {
  const root = document.getElementById('persistent-tactical-svg');
  if (!root) return false;
  const expected = `${$0}:${$1}:${$2}`;
  return root.dataset.presentationCamera === expected;
})()
""")>]
let isTacticalCameraPresented
    (_panX: float)
    (_panY: float)
    (_zoom: float)
    : bool =
    jsNative

[<Emit("""
(() => {
  const root = document.getElementById('persistent-tactical-svg');
  if (!root) return $0;
  const panX = Number(root.dataset.cameraPanX);
  const panY = Number(root.dataset.cameraPanY);
  const zoom = Number(root.dataset.cameraZoom);
  if (!Number.isFinite(panX) || !Number.isFinite(panY) || !Number.isFinite(zoom) || zoom <= 0) return $0;
  return { PanX: panX, PanY: panY, Zoom: zoom };
})()
""")>]
let currentTacticalCamera
    (_fallback: BattlefieldCamera)
    : BattlefieldCamera =
    jsNative

[<Emit("""
(() => {
  const accept = $0;
  let disposed = false;
  let frame = 0;
  let element;
  const observer = new ResizeObserver((entries) => {
    const rect = entries[entries.length - 1]?.contentRect;
    if (!disposed && rect && Number.isFinite(rect.width) && rect.width > 0 && Number.isFinite(rect.height) && rect.height > 0) {
      accept([rect.width, rect.height]);
    }
  });
  const connect = () => {
    if (disposed) return;
    element = document.getElementById('persistent-tactical-svg');
    if (element) observer.observe(element);
    else frame = window.requestAnimationFrame(connect);
  };
  connect();
  return { Dispose() { disposed = true; if (frame !== 0) window.cancelAnimationFrame(frame); observer.disconnect(); } };
})()
""")>]
let observeTacticalViewport
    (_accept: float * float -> unit)
    : IDisposable =
    jsNative

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
