namespace SIR.Protocol.Http

#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif

/// The versioned plain-HTTP bootstrap boundary adopted from the published
/// fs-gg-fable-game ADR-0073 contract. Named codecs compile on .NET and Fable;
/// no domain type or reflection-driven serializer crosses this boundary.
[<RequireQualifiedAccess>]
module BootstrapV1 =

    type Request =
        { Version: int
          ActorName: string }

    type VisibleUnit =
        { UnitId: int
          Column: int
          Row: int
          Health: int }

    type Snapshot =
        { Version: int
          Tick: int
          ServerSequence: int
          ProjectionRevision: int
          VisibleUnits: VisibleUnit list
          StateIdentity: string }

    type Response =
        { Version: int
          SessionId: string
          ActorId: string
          AccessToken: string
          MatchLock: string
          Snapshot: Snapshot }

    let encodeRequest (value: Request) =
        Encode.object
            [ "version", Encode.int value.Version
              "actorName", Encode.string value.ActorName ]
        |> Encode.toString 0

    let decodeRequest: Decoder<Request> =
        Decode.object (fun get ->
            { Version = get.Required.Field "version" Decode.int
              ActorName = get.Required.Field "actorName" Decode.string })

    let requestFromJson json = Decode.fromString decodeRequest json

    let encodeVisibleUnit (value: VisibleUnit) =
        Encode.object
            [ "unitId", Encode.int value.UnitId
              "column", Encode.int value.Column
              "row", Encode.int value.Row
              "health", Encode.int value.Health ]

    let decodeVisibleUnit: Decoder<VisibleUnit> =
        Decode.object (fun get ->
            { UnitId = get.Required.Field "unitId" Decode.int
              Column = get.Required.Field "column" Decode.int
              Row = get.Required.Field "row" Decode.int
              Health = get.Required.Field "health" Decode.int })

    let encodeSnapshot (value: Snapshot) =
        Encode.object
            [ "version", Encode.int value.Version
              "tick", Encode.int value.Tick
              "serverSequence", Encode.int value.ServerSequence
              "projectionRevision", Encode.int value.ProjectionRevision
              "visibleUnits", Encode.list (value.VisibleUnits |> List.map encodeVisibleUnit)
              "stateIdentity", Encode.string value.StateIdentity ]

    let decodeSnapshot: Decoder<Snapshot> =
        Decode.object (fun get ->
            { Version = get.Required.Field "version" Decode.int
              Tick = get.Required.Field "tick" Decode.int
              ServerSequence = get.Required.Field "serverSequence" Decode.int
              ProjectionRevision = get.Required.Field "projectionRevision" Decode.int
              VisibleUnits = get.Required.Field "visibleUnits" (Decode.list decodeVisibleUnit)
              StateIdentity = get.Required.Field "stateIdentity" Decode.string })

    let encodeResponse (value: Response) =
        Encode.object
            [ "version", Encode.int value.Version
              "sessionId", Encode.string value.SessionId
              "actorId", Encode.string value.ActorId
              "accessToken", Encode.string value.AccessToken
              "matchLock", Encode.string value.MatchLock
              "snapshot", encodeSnapshot value.Snapshot ]
        |> Encode.toString 0

    let decodeResponse: Decoder<Response> =
        Decode.object (fun get ->
            { Version = get.Required.Field "version" Decode.int
              SessionId = get.Required.Field "sessionId" Decode.string
              ActorId = get.Required.Field "actorId" Decode.string
              AccessToken = get.Required.Field "accessToken" Decode.string
              MatchLock = get.Required.Field "matchLock" Decode.string
              Snapshot = get.Required.Field "snapshot" decodeSnapshot })

    let responseFromJson json = Decode.fromString decodeResponse json
