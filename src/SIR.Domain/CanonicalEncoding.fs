namespace SIR.Domain

/// Canonical byte primitives used by hashes, fixtures, and replay records.
[<RequireQualifiedAccess>]
module CanonicalEncoding =
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
