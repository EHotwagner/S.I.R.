namespace SIR.Protocol.Realtime

#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif

open SIR.Protocol.Http

/// SignalR carries only this explicit JSON envelope. Inputs are intents; snapshots
/// and resync snapshots remain server-authoritative.
[<RequireQualifiedAccess>]
module RealtimeV1 =

    type AdvanceInput =
        { Version: int
          Sequence: int }

    type ResyncRequest =
        { Version: int
          LastServerSequence: int
          LastProjectionRevision: int }

    type Message =
        | AdvanceInputMessage of AdvanceInput
        | SnapshotMessage of BootstrapV1.Snapshot
        | ResyncRequestMessage of ResyncRequest
        | ResyncSnapshotMessage of BootstrapV1.Snapshot

    let private encodeAdvance (value: AdvanceInput) =
        Encode.object
            [ "version", Encode.int value.Version
              "sequence", Encode.int value.Sequence ]

    let private decodeAdvance: Decoder<AdvanceInput> =
        Decode.object (fun get ->
            { Version = get.Required.Field "version" Decode.int
              Sequence = get.Required.Field "sequence" Decode.int })

    let private encodeResync (value: ResyncRequest) =
        Encode.object
            [ "version", Encode.int value.Version
              "lastServerSequence", Encode.int value.LastServerSequence
              "lastProjectionRevision", Encode.int value.LastProjectionRevision ]

    let private decodeResync: Decoder<ResyncRequest> =
        Decode.object (fun get ->
            { Version = get.Required.Field "version" Decode.int
              LastServerSequence = get.Required.Field "lastServerSequence" Decode.int
              LastProjectionRevision = get.Required.Field "lastProjectionRevision" Decode.int })

    let encodeMessage value =
        let kind, payload =
            match value with
            | AdvanceInputMessage input -> "advance", encodeAdvance input
            | SnapshotMessage snapshot -> "snapshot", BootstrapV1.encodeSnapshot snapshot
            | ResyncRequestMessage request -> "resyncRequest", encodeResync request
            | ResyncSnapshotMessage snapshot -> "resyncSnapshot", BootstrapV1.encodeSnapshot snapshot

        Encode.object [ "kind", Encode.string kind; "payload", payload ]
        |> Encode.toString 0

    let decodeMessage: Decoder<Message> =
        Decode.field "kind" Decode.string
        |> Decode.andThen (fun kind ->
            match kind with
            | "advance" -> Decode.field "payload" decodeAdvance |> Decode.map AdvanceInputMessage
            | "snapshot" -> Decode.field "payload" BootstrapV1.decodeSnapshot |> Decode.map SnapshotMessage
            | "resyncRequest" -> Decode.field "payload" decodeResync |> Decode.map ResyncRequestMessage
            | "resyncSnapshot" -> Decode.field "payload" BootstrapV1.decodeSnapshot |> Decode.map ResyncSnapshotMessage
            | other -> Decode.fail (sprintf "unknown realtime message kind '%s'" other))

    let messageFromJson json = Decode.fromString decodeMessage json
