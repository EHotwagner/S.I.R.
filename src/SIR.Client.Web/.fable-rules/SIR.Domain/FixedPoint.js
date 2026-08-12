
import { Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { int32_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { op_Addition, op_Subtraction, op_Multiply, op_UnaryNegation, op_Modulus, op_Division, toInt32_unchecked, fromInt32, toInt64_unchecked, compare } from "../fable_modules/fable-library-js.5.13.0/BigInt.js";
import { FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";

/**
 * Explains why a fixed-point value could not be constructed.
 */
export class FixedPointError extends Union {
    constructor() {
        super();
        this.tag = 0;
        this.fields = [];
    }
    cases() {
        return ["DivisionByZero"];
    }
    static DivisionByZero = new FixedPointError();
}

export function FixedPointError_$reflection() {
    return union_type("SIR.Domain.FixedPointError", [], FixedPointError, () => [[]]);
}

/**
 * A signed four-place base-ten value stored as an authoritative 32-bit integer.
 */
export class FixedPoint extends Union {
    constructor(raw) {
        super();
        this.tag = 0;
        this.fields = [raw];
    }
    cases() {
        return ["FixedPoint"];
    }
}

export function FixedPoint_$reflection() {
    return union_type("SIR.Domain.FixedPoint", [], FixedPoint, () => [[["raw", int32_type]]]);
}

export const FixedPointModule_zero = new FixedPoint(0);

export function FixedPointModule_fromRaw(raw) {
    return new FixedPoint(raw);
}

export function FixedPointModule_raw(_arg) {
    return _arg.fields[0] | 0;
}

function FixedPointModule_saturate(candidate) {
    if (compare(candidate, toInt64_unchecked(fromInt32(-2147483648))) < 0) {
        return -2147483648;
    }
    else if (compare(candidate, toInt64_unchecked(fromInt32(2147483647))) > 0) {
        return 2147483647;
    }
    else {
        return ~~toInt32_unchecked(candidate) | 0;
    }
}

function FixedPointModule_divideRoundedAwayFromZero(numerator, denominator) {
    const quotient = toInt64_unchecked(op_Division(numerator, denominator));
    const remainder = toInt64_unchecked(op_Modulus(numerator, denominator));
    const absoluteRemainder = (compare(remainder, 0n) < 0) ? toInt64_unchecked(op_UnaryNegation(remainder)) : remainder;
    const absoluteDenominator = (compare(denominator, 0n) < 0) ? toInt64_unchecked(op_UnaryNegation(denominator)) : denominator;
    if (compare(toInt64_unchecked(op_Multiply(absoluteRemainder, 2n)), absoluteDenominator) < 0) {
        return quotient;
    }
    else if ((compare(numerator, 0n) < 0) !== (compare(denominator, 0n) < 0)) {
        return toInt64_unchecked(op_Subtraction(quotient, 1n));
    }
    else {
        return toInt64_unchecked(op_Addition(quotient, 1n));
    }
}

export function FixedPointModule_fromRatio(numerator, denominator) {
    if (denominator === 0) {
        return new FSharpResult$2(/* Error */ 1, [FixedPointError.DivisionByZero]);
    }
    else {
        return new FSharpResult$2(/* Ok */ 0, [new FixedPoint(FixedPointModule_saturate(FixedPointModule_divideRoundedAwayFromZero(toInt64_unchecked(op_Multiply(toInt64_unchecked(fromInt32(numerator)), toInt64_unchecked(fromInt32(10000)))), toInt64_unchecked(fromInt32(denominator)))))]);
    }
}

export function FixedPointModule_addSaturating(_arg, _arg_1) {
    return new FixedPoint(FixedPointModule_saturate(toInt64_unchecked(op_Addition(toInt64_unchecked(fromInt32(_arg.fields[0])), toInt64_unchecked(fromInt32(_arg_1.fields[0]))))));
}

export function FixedPointModule_subtractSaturating(_arg, _arg_1) {
    return new FixedPoint(FixedPointModule_saturate(toInt64_unchecked(op_Subtraction(toInt64_unchecked(fromInt32(_arg.fields[0])), toInt64_unchecked(fromInt32(_arg_1.fields[0]))))));
}

export function FixedPointModule_multiplySaturating(_arg, _arg_1) {
    return new FixedPoint(FixedPointModule_saturate(FixedPointModule_divideRoundedAwayFromZero(toInt64_unchecked(op_Multiply(toInt64_unchecked(fromInt32(_arg.fields[0])), toInt64_unchecked(fromInt32(_arg_1.fields[0])))), toInt64_unchecked(fromInt32(10000)))));
}

export function FixedPointModule_compareByRaw(_arg, _arg_1) {
    return comparePrimitives(_arg.fields[0], _arg_1.fields[0]) | 0;
}

