namespace SIR.Client.Web

open System
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open SIR.Client

[<RequireQualifiedAccess>]
module Runner =
    [<Emit("new Worker(new URL('./Worker.js', import.meta.url), { type: 'module', name: 'sir-replay-v1' })")>]
    let private createWorker () : obj = jsNative

    let mutable private worker: obj option = None

    let private current () =
        match worker with
        | Some active -> active
        | None ->
            let active = createWorker ()
            worker <- Some active
            active

    let post operation request =
        let envelope: WorkerRequestEnvelope =
            { ProtocolVersion = int32 WorkerProtocol.CurrentVersion
              Operation = operation
              Request = request }

        (current ())?postMessage (envelope)

    let subscribe (dispatch: SIR.Client.Msg -> unit) =
        let active = current ()

        active?onmessage <-
            fun event ->
                let message = unbox<MessageEvent> event
                let envelope = unbox<WorkerResponseEnvelope> message.data

                if envelope.ProtocolVersion = int32 WorkerProtocol.CurrentVersion then
                    dispatch (RunnerResponded(envelope.Operation, envelope.Response))
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

        dispatch WorkerStarted

        { new IDisposable with
            member _.Dispose() =
                active?terminate ()
                worker <- None }
