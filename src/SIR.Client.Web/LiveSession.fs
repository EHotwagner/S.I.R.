namespace SIR.Client.Web

open Browser.Dom
open Fable.Core
open Fable.Core.JsInterop
open SIR.Protocol.Http
open SIR.Protocol.Realtime

/// Small transport adapter mounted beside the existing Elmish tactical workspace.
/// It never mutates S.I.R. domain state locally: only server snapshots update the
/// observed tick/projection test surface.
[<RequireQualifiedAccess>]
module LiveSession =

    [<Emit("$0.then($1, $2)")>]
    let private thenBoth (promise: JS.Promise<'T>) (onOk: 'T -> unit) (onError: obj -> unit) : unit = jsNative

    let mutable private connection: SignalR.HubConnection option = None
    let mutable private bootstrap: BootstrapV1.Response option = None
    let mutable private lastSnapshot: BootstrapV1.Snapshot option = None
    let mutable private nextSequence = 1
    let mutable private resyncCount = 0
    let mutable private status = "bootstrapping"

    let private ensureStatusElement () =
        match document.getElementById "sir-live-session" with
        | null ->
            let element = document.createElement "aside"
            element.id <- "sir-live-session"
            element.setAttribute("aria-label", "Authoritative live session status")
            element.setAttribute("style", "position:fixed;right:0;bottom:0;padding:0.35rem;background:#111;color:#eee;font:12px monospace;z-index:99")
            document.body.appendChild element |> ignore
            element
        | element -> element

    let private ensureControls advanceAction reconnectAction =
        let controls =
            match document.getElementById "sir-live-controls" with
            | null ->
                let element = document.createElement "nav"
                element.id <- "sir-live-controls"
                document.body.appendChild element |> ignore
                element
            | element -> element

        let addButton id label action =
            if isNull (document.getElementById id) then
                let button = document.createElement "button"
                button.id <- id
                button.textContent <- label
                button.addEventListener("click", fun _ -> action ())
                controls.appendChild button |> ignore

        addButton "sir-live-advance" "Advance live session" advanceAction
        addButton "sir-live-reconnect" "Reconnect live session" reconnectAction

    let private render () =
        let element = ensureStatusElement ()
        let tick = lastSnapshot |> Option.map _.Tick |> Option.defaultValue 0
        let sequence = lastSnapshot |> Option.map _.ServerSequence |> Option.defaultValue 0
        element.setAttribute("data-status", status)
        element.setAttribute("data-tick", string tick)
        element.setAttribute("data-server-sequence", string sequence)
        element.setAttribute("data-resync-count", string resyncCount)
        element.setAttribute("data-session-id", bootstrap |> Option.map _.SessionId |> Option.defaultValue "")
        element.textContent <- $"live {status} · tick {tick} · resync {resyncCount}"

    let private receive json =
        match RealtimeV1.messageFromJson json with
        | Error error ->
            status <- "decode-error:" + error
            render ()
        | Ok(RealtimeV1.SnapshotMessage snapshot) ->
            lastSnapshot <- Some snapshot
            status <- "connected"
            render ()
        | Ok(RealtimeV1.ResyncSnapshotMessage snapshot) ->
            lastSnapshot <- Some snapshot
            resyncCount <- resyncCount + 1
            status <- "connected"
            render ()
        | Ok(RealtimeV1.AdvanceInputMessage _)
        | Ok(RealtimeV1.ResyncRequestMessage _) -> ()

    let private sendResync (active: SignalR.HubConnection) =
        let snapshot = lastSnapshot
        let request: RealtimeV1.ResyncRequest =
            { Version = 1
              LastServerSequence = snapshot |> Option.map _.ServerSequence |> Option.defaultValue 0
              LastProjectionRevision = snapshot |> Option.map _.ProjectionRevision |> Option.defaultValue 0 }
        thenBoth
            (active.invoke("SendMessage", RealtimeV1.encodeMessage (RealtimeV1.ResyncRequestMessage request)))
            ignore
            (fun _ -> ())

    let private connect (response: BootstrapV1.Response) =
        let active = SignalR.build "/hub/game" response.AccessToken
        active.on("Message", receive)
        active.onreconnected(fun _ -> sendResync active)
        active.onclose(fun _ -> status <- "disconnected"; render ())
        connection <- Some active
        thenBoth (active.start()) (fun () -> status <- "connected"; render ()) (fun error -> status <- "connection-error:" + string error; render ())

    let private advance () =
        match connection with
        | None -> ()
        | Some active ->
            let input: RealtimeV1.AdvanceInput = { Version = 1; Sequence = nextSequence }
            nextSequence <- nextSequence + 1
            thenBoth
                (active.invoke("SendMessage", RealtimeV1.encodeMessage (RealtimeV1.AdvanceInputMessage input)))
                ignore
                (fun _ -> ())

    let private reconnect () =
        match connection with
        | None -> ()
        | Some active ->
            status <- "reconnecting"
            render ()
            thenBoth (active.stop()) (fun () -> thenBoth (active.start()) (fun () -> sendResync active) (fun _ -> ())) (fun _ -> ())

    let start () =
        render ()
        ensureControls advance reconnect
        let request: BootstrapV1.Request = { Version = 1; ActorName = "browser-commander" }
        Async.StartImmediate(async {
            try
                let! response = LiveApi.bootstrap request
                bootstrap <- Some response
                lastSnapshot <- Some response.Snapshot
                status <- "connecting"
                render ()
                connect response
            with error ->
                status <- "bootstrap-error:" + error.Message
                render ()
        })
