namespace SIR.Domain

/// Canonical byte primitives used by hashes, fixtures, and replay records.
[<RequireQualifiedAccess>]
module CanonicalEncoding =
    let byteValue value = [| value |]

    let int32LittleEndian (value: int32) =
        [| byte value
           byte (value >>> 8)
           byte (value >>> 16)
           byte (value >>> 24) |]

    let boundedInt32 value =
        value |> BoundedInt32.value |> int32LittleEndian

    let fixedPoint value =
        value |> FixedPoint.raw |> int32LittleEndian

    let concatenate (segments: byte array seq) =
        segments |> Seq.collect id |> Seq.toArray

    let direction8 value =
        value |> Direction8.toCode |> byteValue

    let resolvedOrientation value =
        concatenate
            [ match value.MovementDirection with
              | None -> yield byteValue 0uy
              | Some direction ->
                  yield byteValue 1uy
                  yield direction8 direction
              yield direction8 value.BodyFacing
              yield direction8 value.AttentionDirection ]

    /// A provisional non-cryptographic digest for conformance checkpoints.
    /// Replay-format hash selection remains an M7 concern.
    let digest32 (bytes: byte array) =
        bytes
        |> Array.fold
            (fun digest value ->
                let rotated = (digest <<< 5) ||| (digest >>> 27)
                rotated ^^^ uint32 value)
            2_654_435_769u
        |> int32
        |> int32LittleEndian
