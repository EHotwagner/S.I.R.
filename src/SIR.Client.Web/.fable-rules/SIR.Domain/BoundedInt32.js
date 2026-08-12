
import { Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { record_type, union_type, int32_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { op_Subtraction, op_Addition, toInt32_unchecked, fromInt32, toInt64_unchecked, compare } from "../fable_modules/fable-library-js.5.13.0/BigInt.js";
import { comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";

/**
 * Explains why a bounded integer operation could not be performed.
 */
export class BoundedInt32Error extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["InvalidBounds", "OutsideBounds", "BoundsMismatch"];
    }
    static BoundsMismatch = new BoundedInt32Error(2, []);
}

export function BoundedInt32Error_$reflection() {
    return union_type("SIR.Domain.BoundedInt32Error", [], BoundedInt32Error, () => [[["minimum", int32_type], ["maximum", int32_type]], [["minimum", int32_type], ["maximum", int32_type], ["value", int32_type]], []]);
}

/**
 * An authoritative signed integer carrying its inclusive valid range.
 */
export class BoundedInt32 extends Record {
    constructor(Minimum, Maximum, Value) {
        super();
        this.Minimum = (Minimum | 0);
        this.Maximum = (Maximum | 0);
        this.Value = (Value | 0);
    }
}

export function BoundedInt32_$reflection() {
    return record_type("SIR.Domain.BoundedInt32", [], BoundedInt32, () => [["Minimum", int32_type], ["Maximum", int32_type], ["Value", int32_type]]);
}

export function BoundedInt32Module_create(minimum, maximum, value) {
    if (minimum > maximum) {
        return new FSharpResult$2(/* Error */ 1, [new BoundedInt32Error(/* InvalidBounds */ 0, [minimum, maximum])]);
    }
    else if ((value < minimum) ? true : (value > maximum)) {
        return new FSharpResult$2(/* Error */ 1, [new BoundedInt32Error(/* OutsideBounds */ 1, [minimum, maximum, value])]);
    }
    else {
        return new FSharpResult$2(/* Ok */ 0, [new BoundedInt32(minimum, maximum, value)]);
    }
}

export function BoundedInt32Module_minimum(bounded) {
    return bounded.Minimum | 0;
}

export function BoundedInt32Module_maximum(bounded) {
    return bounded.Maximum | 0;
}

export function BoundedInt32Module_value(bounded) {
    return bounded.Value | 0;
}

function BoundedInt32Module_sameBounds(left, right) {
    if (left.Minimum === right.Minimum) {
        return left.Maximum === right.Maximum;
    }
    else {
        return false;
    }
}

function BoundedInt32Module_saturate(minimum, maximum, candidate) {
    if (compare(candidate, toInt64_unchecked(fromInt32(minimum))) < 0) {
        return minimum | 0;
    }
    else if (compare(candidate, toInt64_unchecked(fromInt32(maximum))) > 0) {
        return maximum | 0;
    }
    else {
        return ~~toInt32_unchecked(candidate) | 0;
    }
}

export function BoundedInt32Module_addSaturating(left, right) {
    if (!BoundedInt32Module_sameBounds(left, right)) {
        return new FSharpResult$2(/* Error */ 1, [BoundedInt32Error.BoundsMismatch]);
    }
    else {
        return BoundedInt32Module_create(left.Minimum, left.Maximum, BoundedInt32Module_saturate(left.Minimum, left.Maximum, toInt64_unchecked(op_Addition(toInt64_unchecked(fromInt32(left.Value)), toInt64_unchecked(fromInt32(right.Value))))));
    }
}

export function BoundedInt32Module_subtractSaturating(left, right) {
    if (!BoundedInt32Module_sameBounds(left, right)) {
        return new FSharpResult$2(/* Error */ 1, [BoundedInt32Error.BoundsMismatch]);
    }
    else {
        return BoundedInt32Module_create(left.Minimum, left.Maximum, BoundedInt32Module_saturate(left.Minimum, left.Maximum, toInt64_unchecked(op_Subtraction(toInt64_unchecked(fromInt32(left.Value)), toInt64_unchecked(fromInt32(right.Value))))));
    }
}

/**
 * Compares only authoritative values; bounds must be identical.
 */
export function BoundedInt32Module_compareByValue(left, right) {
    if (!BoundedInt32Module_sameBounds(left, right)) {
        return new FSharpResult$2(/* Error */ 1, [BoundedInt32Error.BoundsMismatch]);
    }
    else {
        return new FSharpResult$2(/* Ok */ 0, [comparePrimitives(left.Value, right.Value)]);
    }
}

