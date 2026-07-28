namespace SIR.Domain

/// Explains why a bounded integer operation could not be performed.
type BoundedInt32Error =
    | InvalidBounds of minimum: int32 * maximum: int32
    | OutsideBounds of minimum: int32 * maximum: int32 * value: int32
    | BoundsMismatch

/// An authoritative signed integer carrying its inclusive valid range.
[<Struct; NoEquality; NoComparison>]
type BoundedInt32 =
    private
        { Minimum: int32
          Maximum: int32
          Value: int32 }

/// Exact, saturating operations for authoritative bounded integers.
[<RequireQualifiedAccess>]
module BoundedInt32 =
    let create minimum maximum value =
        if minimum > maximum then
            Error(InvalidBounds(minimum, maximum))
        elif value < minimum || value > maximum then
            Error(OutsideBounds(minimum, maximum, value))
        else
            Ok
                { Minimum = minimum
                  Maximum = maximum
                  Value = value }

    let minimum bounded = bounded.Minimum
    let maximum bounded = bounded.Maximum
    let value bounded = bounded.Value

    let private sameBounds left right =
        left.Minimum = right.Minimum && left.Maximum = right.Maximum

    let private saturate minimum maximum (candidate: int64) =
        if candidate < int64 minimum then minimum
        elif candidate > int64 maximum then maximum
        else int32 candidate

    let addSaturating left right =
        if not (sameBounds left right) then
            Error BoundsMismatch
        else
            create
                left.Minimum
                left.Maximum
                (saturate left.Minimum left.Maximum (int64 left.Value + int64 right.Value))

    let subtractSaturating left right =
        if not (sameBounds left right) then
            Error BoundsMismatch
        else
            create
                left.Minimum
                left.Maximum
                (saturate left.Minimum left.Maximum (int64 left.Value - int64 right.Value))

    /// Compares only authoritative values; bounds must be identical.
    let compareByValue left right =
        if not (sameBounds left right) then
            Error BoundsMismatch
        else
            Ok(compare left.Value right.Value)
