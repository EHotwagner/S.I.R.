namespace SIR.Client.Web

open System
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open SIR.Client

[<RequireQualifiedAccess>]
module Runner =
    [<Emit("new Worker(new URL('./Worker.js', import.meta.url), { type: 'module', name: 'sir-engine-v1' })")>]
    let private createCurrentWorker () : obj = jsNative

    let mutable private worker: (string * obj) option = None
    let mutable private subscriber: (SIR.Client.Msg -> unit) option = None
    let mutable private simulatorSubscriber: (SimulatorResponseEnvelope -> unit) option = None
    let mutable private simulatorGuard: SimulatorWorkspaceGuard =
        { Active = None
          PendingOperations = Set.empty }

    let private dispatch message =
        subscriber |> Option.iter (fun send -> send message)

    let private bind (active: obj) =
        active?onmessage <-
            fun event ->
                let message = unbox<MessageEvent> event
                let kind: string = message.data?Kind

                if kind = SimulatorProtocol.Kind then
                    let envelope =
                        unbox<SimulatorResponseEnvelope> message.data

                    if SimulatorProtocol.accepts envelope simulatorGuard then
                        simulatorSubscriber
                        |> Option.iter (fun receive -> receive envelope)

                        match envelope.Response with
                        | SimulatorProgress _ -> ()
                        | _ ->
                            simulatorGuard <-
                                SimulatorProtocol.completeOperation
                                    envelope.Correlation.Operation
                                    simulatorGuard
                else
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

    let post operation request =
        let envelope: WorkerRequestEnvelope =
            { ProtocolVersion = int32 WorkerProtocol.CurrentVersion
              Operation = OperationId.value operation
              Request = request }

        match request with
        | LoadPackage _ ->
            // The retained worker owns complete untrusted replay decoding and identity validation.
            (activate EngineCatalog.Current)?postMessage (envelope)
        | _ -> (activate EngineCatalog.Current)?postMessage (envelope)

    /// Sends a simulator-session operation through the retained browser worker.
    /// Responses that no longer match the active workspace correlation are
    /// discarded in `bind` before any UI subscriber can observe them.
    let postSimulator correlation request =
        let envelope: SimulatorRequestEnvelope =
            { Kind = SimulatorProtocol.Kind
              ProtocolVersion = int32 SimulatorProtocol.CurrentVersion
              Correlation = correlation
              Request = request }

        simulatorGuard <-
            match request with
            | InitializeSession _ ->
                SimulatorProtocol.activate correlation
            | _ ->
                SimulatorProtocol.beginOperation correlation simulatorGuard

        (activate EngineCatalog.Current)?postMessage (envelope)

    let subscribeSimulator receive =
        simulatorSubscriber <- Some receive

        { new IDisposable with
            member _.Dispose() =
                simulatorSubscriber <- None
                simulatorGuard <-
                    { Active = None
                      PendingOperations = Set.empty } }

    let subscribe (send: SIR.Client.Msg -> unit) =
        subscriber <- Some send
        send WorkerStarted

        { new IDisposable with
            member _.Dispose() =
                worker
                |> Option.iter (fun (_, active) -> active?terminate ())

                worker <- None
                subscriber <- None }
