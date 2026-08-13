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
        // Canonical payloads can contain tens of thousands of small segments.
        // Materialize them once and copy each segment as a block; enumerating
        // every byte through Seq.collect made snapshot cost scale in allocator
        // overhead instead of encoded size.
        let materialized = segments |> Seq.toArray
        let length = materialized |> Array.sumBy _.Length
        let result = Array.zeroCreate<byte> length
        let mutable offset = 0
        for segment in materialized do
            Array.blit segment 0 result offset segment.Length
            offset <- offset + segment.Length
        result

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
