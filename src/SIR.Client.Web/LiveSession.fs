namespace SIR.Client.Web

open Fable.Core
open SIR.Protocol.Http
open SIR.Protocol.Realtime

/// Elmish-owned live-session state. Transport callbacks only dispatch actions;
/// they do not mount DOM or retain a second application state.
[<RequireQualifiedAccess>]
module LiveSession =

    [<Emit("$0.then($1, $2)")>]
    let private thenBoth (promise: JS.Promise<'T>) (onOk: 'T -> unit) (onError: obj -> unit) : unit = jsNative

    type State =
        { Connection: SignalR.HubConnection option
          Bootstrap: BootstrapV1.Response option
          Snapshot: BootstrapV1.Snapshot option
          NextSequence: int
          ResyncCount: int
          Status: string }

    type Action =
        | Bootstrapped of BootstrapV1.Response
        | BootstrapFailed of string
        | Connected of SignalR.HubConnection
        | ConnectionOpened
        | ConnectionClosed
        | ConnectionFailed of string
        | Received of RealtimeV1.Message
        | DecodeFailed of string

    let initial =
        { Connection = None
          Bootstrap = None
          Snapshot = None
          NextSequence = 1
          ResyncCount = 0
          Status = "bootstrapping" }

    let start dispatch =
        let request: BootstrapV1.Request = { Version = 1; ActorName = "browser-commander" }
        Async.StartImmediate(async {
            try
                let! response = LiveApi.bootstrap request
                dispatch (Bootstrapped response)
            with error ->
                dispatch (BootstrapFailed error.Message)
        })

    let connect (dispatch: Action -> unit) (response: BootstrapV1.Response) =
        let active = SignalR.build "/hub/game" response.AccessToken
        active.on("Message", fun json ->
            match RealtimeV1.messageFromJson json with
            | Ok message -> dispatch (Received message)
            | Error error -> dispatch (DecodeFailed error))
        active.onreconnected(fun _ -> dispatch ConnectionOpened)
        active.onclose(fun _ -> dispatch ConnectionClosed)
        thenBoth (active.start()) (fun () -> dispatch ConnectionOpened) (fun error -> dispatch (ConnectionFailed(string error)))
        active

    let requestResync dispatch state =
        match state.Connection with
        | None -> ()
        | Some active ->
            let snapshot = state.Snapshot
            let request: RealtimeV1.ResyncRequest =
                { Version = 1
                  LastServerSequence = snapshot |> Option.map _.ServerSequence |> Option.defaultValue 0
                  LastProjectionRevision = snapshot |> Option.map _.ProjectionRevision |> Option.defaultValue 0 }
            thenBoth
                (active.invoke("SendMessage", RealtimeV1.encodeMessage (RealtimeV1.ResyncRequestMessage request)))
                ignore
                (fun error -> dispatch (ConnectionFailed(string error)))

    let advance dispatch state =
        match state.Connection with
        | None -> ()
        | Some active ->
            let input: RealtimeV1.AdvanceInput = { Version = 1; Sequence = state.NextSequence }
            thenBoth
                (active.invoke("SendMessage", RealtimeV1.encodeMessage (RealtimeV1.AdvanceInputMessage input)))
                ignore
                (fun error -> dispatch (ConnectionFailed(string error)))

    let reconnect dispatch state =
        match state.Connection with
        | None -> ()
        | Some active ->
            thenBoth
                (active.stop())
                (fun () -> thenBoth (active.start()) (fun () -> requestResync dispatch state) (fun error -> dispatch (ConnectionFailed(string error))))
                (fun error -> dispatch (ConnectionFailed(string error)))
