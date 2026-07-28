namespace SIR.Domain

/// Explains why a fixed-point value could not be constructed.
type FixedPointError =
    | DivisionByZero

/// A signed four-place base-ten value stored as an authoritative 32-bit integer.
[<Struct; NoEquality; NoComparison>]
type FixedPoint = private FixedPoint of raw: int32

/// Saturating Q4 arithmetic with round-to-nearest, ties away from zero.
[<RequireQualifiedAccess>]
module FixedPoint =
    [<Literal>]
    let Scale = 10_000

    let zero = FixedPoint 0
    let fromRaw raw = FixedPoint raw
    let raw (FixedPoint value) = value

    let private saturate (candidate: int64) =
        if candidate < int64 System.Int32.MinValue then System.Int32.MinValue
        elif candidate > int64 System.Int32.MaxValue then System.Int32.MaxValue
        else int32 candidate

    let private divideRoundedAwayFromZero numerator denominator =
        let quotient = numerator / denominator
        let remainder = numerator % denominator
        let absoluteRemainder = if remainder < 0L then -remainder else remainder
        let absoluteDenominator = if denominator < 0L then -denominator else denominator

        if absoluteRemainder * 2L < absoluteDenominator then
            quotient
        elif (numerator < 0L) <> (denominator < 0L) then
            quotient - 1L
        else
            quotient + 1L

    let fromRatio numerator denominator =
        if denominator = 0 then
            Error DivisionByZero
        else
            let scaled = int64 numerator * int64 Scale
            Ok(FixedPoint(saturate (divideRoundedAwayFromZero scaled (int64 denominator))))

    let addSaturating (FixedPoint left) (FixedPoint right) =
        FixedPoint(saturate (int64 left + int64 right))

    let subtractSaturating (FixedPoint left) (FixedPoint right) =
        FixedPoint(saturate (int64 left - int64 right))

    let multiplySaturating (FixedPoint left) (FixedPoint right) =
        let product = int64 left * int64 right
        FixedPoint(saturate (divideRoundedAwayFromZero product (int64 Scale)))

    let compareByRaw (FixedPoint left) (FixedPoint right) = compare left right
