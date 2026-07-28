namespace SIR.Client.Web

open System
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open SIR.Client
open SIR.Simulation

[<RequireQualifiedAccess>]
module Runner =
    [<Emit("new Worker(new URL('./Worker.js', import.meta.url), { type: 'module', name: 'sir-engine-v1' })")>]
    let private createCurrentWorker () : obj = jsNative

    let mutable private worker: (string * obj) option = None
    let mutable private subscriber: (SIR.Client.Msg -> unit) option = None

    let private dispatch message =
        subscriber |> Option.iter (fun send -> send message)

    let private bind (active: obj) =
        active?onmessage <-
            fun event ->
                let message = unbox<MessageEvent> event
                let envelope = unbox<WorkerResponseEnvelope> message.data

                if envelope.ProtocolVersion = int32 WorkerProtocol.CurrentVersion then
                    dispatch (
                        RunnerResponded(
                            OperationId.create envelope.Operation,
                            envelope.Response
                        )
                    )
                else
                    dispatch (
                        WorkerTerminated(
                            "protocol "
                            + string envelope.ProtocolVersion
                            + " is incompatible with protocol "
                            + string WorkerProtocol.CurrentVersion
                        )
                    )

        active?onerror <-
            fun event ->
                let error = unbox<ErrorEvent> event
                dispatch (WorkerTerminated error.message)
                true

    let private activate (engine: RetainedEngine) =
        match worker with
        | Some(identity, active) when identity = engine.Identity -> active
        | Some(_, active) ->
            active?terminate ()
            let replacement = createCurrentWorker ()
            bind replacement
            worker <- Some(engine.Identity, replacement)
            replacement
        | None ->
            let active = createCurrentWorker ()
            bind active
            worker <- Some(engine.Identity, active)
            active

    let private engineIdentity (bytes: byte array) =
        bytes
        |> Array.map (fun value -> value.ToString("x2"))
        |> String.concat ""

    let post operation request =
        let envelope: WorkerRequestEnvelope =
            { ProtocolVersion = int32 WorkerProtocol.CurrentVersion
              Operation = OperationId.value operation
              Request = request }

        match request with
        | LoadPackage(_, bytes) ->
            match Replay.decode Replay.defaultLimits bytes with
            | Ok package ->
                match EngineCatalog.tryFind package with
                | Some engine -> (activate engine)?postMessage (envelope)
                | None ->
                    dispatch (
                        RunnerResponded(
                            operation,
                            RunnerUnsupported(
                                "engine "
                                + engineIdentity package.EngineHash
                                + " is not retained by this publication"
                            )
                        )
                    )
            | Error _ ->
                // The retained worker owns detailed validation errors for malformed packages.
                (activate EngineCatalog.Current)?postMessage (envelope)
        | _ -> (activate EngineCatalog.Current)?postMessage (envelope)

    let subscribe (send: SIR.Client.Msg -> unit) =
        subscriber <- Some send
        send WorkerStarted

        { new IDisposable with
            member _.Dispose() =
                worker
                |> Option.iter (fun (_, active) -> active?terminate ())

                worker <- None
                subscriber <- None }
