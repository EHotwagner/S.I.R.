
import { BoundedInt32Module_value } from "./BoundedInt32.js";
import { FixedPointModule_raw } from "./FixedPoint.js";
import { singleton, append, delay, toList, collect, toArray } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { Direction8Module_toCode } from "./Orientation.js";
import { fold } from "../fable_modules/fable-library-js.5.13.0/Array.js";

export function byteValue(value) {
    return [value];
}

export function int32LittleEndian(value) {
    return new Uint8Array([value & 0xFF, (value >> 8) & 0xFF, (value >> 16) & 0xFF, (value >> 24) & 0xFF]);
}

export function boundedInt32(value) {
    return int32LittleEndian(BoundedInt32Module_value(value));
}

export function fixedPoint(value) {
    return int32LittleEndian(FixedPointModule_raw(value));
}

export function concatenate(segments) {
    return toArray(collect((x) => x, segments));
}

export function direction8(value) {
    return byteValue(Direction8Module_toCode(value));
}

export function resolvedOrientation(value) {
    return concatenate(toList(delay(() => {
        let matchValue, direction;
        return append((matchValue = value.MovementDirection, (matchValue != null) ? ((direction = matchValue, append(singleton(byteValue(1)), delay(() => singleton(direction8(direction)))))) : singleton(byteValue(0))), delay(() => append(singleton(direction8(value.BodyFacing)), delay(() => singleton(direction8(value.AttentionDirection))))));
    })));
}

/**
 * A provisional non-cryptographic digest for conformance checkpoints.
 * Replay-format hash selection remains an M7 concern.
 */
export function digest32(bytes) {
    return int32LittleEndian(~~fold((digest, value) => ((((((digest << 5) >>> 0) | (digest >>> 27)) >>> 0) ^ value) >>> 0), 2654435769, bytes));
}

