
import { Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { record_type, option_type, union_type, int32_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { item } from "../fable_modules/fable-library-js.5.13.0/Array.js";

/**
 * A stable unit identity shared by plans, simulation, replay, and projections.
 */
export class UnitId extends Union {
    constructor(Item) {
        super();
        this.tag = 0;
        this.fields = [Item];
    }
    cases() {
        return ["UnitId"];
    }
}

export function UnitId_$reflection() {
    return union_type("SIR.Domain.UnitId", [], UnitId, () => [[["Item", int32_type]]]);
}

export function UnitIdModule_create(value) {
    return new UnitId(value);
}

export function UnitIdModule_value(_arg) {
    return _arg.fields[0] | 0;
}

/**
 * The eight canonical compass directions, clockwise from north.
 */
export class Direction8 extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["North", "NorthEast", "East", "SouthEast", "South", "SouthWest", "West", "NorthWest"];
    }
    static North = new Direction8(0, []);
    static NorthEast = new Direction8(1, []);
    static East = new Direction8(2, []);
    static SouthEast = new Direction8(3, []);
    static South = new Direction8(4, []);
    static SouthWest = new Direction8(5, []);
    static West = new Direction8(6, []);
    static NorthWest = new Direction8(7, []);
}

export function Direction8_$reflection() {
    return union_type("SIR.Domain.Direction8", [], Direction8, () => [[], [], [], [], [], [], [], []]);
}

/**
 * A stable, knowledge-filtered area referent. Its interpretation is owned by
 * the map/ruleset that issued it.
 */
export class AreaReferent extends Union {
    constructor(Item) {
        super();
        this.tag = 0;
        this.fields = [Item];
    }
    cases() {
        return ["AreaReferent"];
    }
}

export function AreaReferent_$reflection() {
    return union_type("SIR.Domain.AreaReferent", [], AreaReferent, () => [[["Item", int32_type]]]);
}

/**
 * The three resolved directions disclosed for an authoritative unit.
 */
export class ResolvedOrientation extends Record {
    constructor(MovementDirection, BodyFacing, AttentionDirection) {
        super();
        this.MovementDirection = MovementDirection;
        this.BodyFacing = BodyFacing;
        this.AttentionDirection = AttentionDirection;
    }
}

export function ResolvedOrientation_$reflection() {
    return record_type("SIR.Domain.ResolvedOrientation", [], ResolvedOrientation, () => [["MovementDirection", option_type(Direction8_$reflection())], ["BodyFacing", Direction8_$reflection()], ["AttentionDirection", Direction8_$reflection()]]);
}

/**
 * Durable body-facing intent used by plans and standard controllers.
 */
export class FacingIntent extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["KeepFacing", "FaceFixed", "FaceAlongMovement", "FaceKnownUnit"];
    }
    static KeepFacing = new FacingIntent(0, []);
    static FaceAlongMovement = new FacingIntent(2, []);
}

export function FacingIntent_$reflection() {
    return union_type("SIR.Domain.FacingIntent", [], FacingIntent, () => [[], [["Item", Direction8_$reflection()]], [], [["Item", UnitId_$reflection()]]]);
}

/**
 * Durable attention intent used by plans and standard controllers.
 */
export class AttentionIntent extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["KeepAttention", "AttendFixed", "AttendRelativeToBody", "AttendAlongMovement", "AttendKnownUnit", "AttendKnownArea"];
    }
    static KeepAttention = new AttentionIntent(0, []);
    static AttendAlongMovement = new AttentionIntent(3, []);
}

export function AttentionIntent_$reflection() {
    return union_type("SIR.Domain.AttentionIntent", [], AttentionIntent, () => [[], [["Item", Direction8_$reflection()]], [["Item", Direction8_$reflection()]], [], [["Item", UnitId_$reflection()]], [["Item", AreaReferent_$reflection()]]]);
}

export const Direction8Module_all = [Direction8.North, Direction8.NorthEast, Direction8.East, Direction8.SouthEast, Direction8.South, Direction8.SouthWest, Direction8.West, Direction8.NorthWest];

export function Direction8Module_toCode(direction) {
    switch (direction.tag) {
        case 1:
            return 1;
        case 2:
            return 2;
        case 3:
            return 3;
        case 4:
            return 4;
        case 5:
            return 5;
        case 6:
            return 6;
        case 7:
            return 7;
        default:
            return 0;
    }
}

export function Direction8Module_tryFromCode(code) {
    switch (code) {
        case 0:
            return Direction8.North;
        case 1:
            return Direction8.NorthEast;
        case 2:
            return Direction8.East;
        case 3:
            return Direction8.SouthEast;
        case 4:
            return Direction8.South;
        case 5:
            return Direction8.SouthWest;
        case 6:
            return Direction8.West;
        case 7:
            return Direction8.NorthWest;
        default:
            return undefined;
    }
}

export function Direction8Module_delta(direction) {
    switch (direction.tag) {
        case 1:
            return [1, -1];
        case 2:
            return [1, 0];
        case 3:
            return [1, 1];
        case 4:
            return [0, 1];
        case 5:
            return [-1, 1];
        case 6:
            return [-1, 0];
        case 7:
            return [-1, -1];
        default:
            return [0, -1];
    }
}

/**
 * Resolves an octant relative to body facing. Relative north is forward.
 */
export function Direction8Module_relativeToBody(body, relative) {
    return item((~~Direction8Module_toCode(body) + ~~Direction8Module_toCode(relative)) % Direction8Module_all.length, Direction8Module_all);
}

/**
 * Resolves a non-zero segment delta to its compass octant.
 */
export function Direction8Module_tryFromDelta(columnDelta, rowDelta) {
    const sign = (value) => {
        if (value < 0) {
            return -1;
        }
        else if (value > 0) {
            return 1;
        }
        else {
            return 0;
        }
    };
    const matchValue = sign(columnDelta) | 0;
    const matchValue_1 = sign(rowDelta) | 0;
    let matchResult;
    switch (matchValue) {
        case -1: {
            switch (matchValue_1) {
                case -1: {
                    matchResult = 8;
                    break;
                }
                case 0: {
                    matchResult = 7;
                    break;
                }
                case 1: {
                    matchResult = 6;
                    break;
                }
                default:
                    matchResult = 9;
            }
            break;
        }
        case 0: {
            switch (matchValue_1) {
                case -1: {
                    matchResult = 1;
                    break;
                }
                case 0: {
                    matchResult = 0;
                    break;
                }
                case 1: {
                    matchResult = 5;
                    break;
                }
                default:
                    matchResult = 9;
            }
            break;
        }
        case 1: {
            switch (matchValue_1) {
                case -1: {
                    matchResult = 2;
                    break;
                }
                case 0: {
                    matchResult = 3;
                    break;
                }
                case 1: {
                    matchResult = 4;
                    break;
                }
                default:
                    matchResult = 9;
            }
            break;
        }
        default:
            matchResult = 9;
    }
    switch (matchResult) {
        case 0:
            return undefined;
        case 1:
            return Direction8.North;
        case 2:
            return Direction8.NorthEast;
        case 3:
            return Direction8.East;
        case 4:
            return Direction8.SouthEast;
        case 5:
            return Direction8.South;
        case 6:
            return Direction8.SouthWest;
        case 7:
            return Direction8.West;
        case 8:
            return Direction8.NorthWest;
        default:
            return undefined;
    }
}

export const Direction8Module_defaultOrientation = new ResolvedOrientation(undefined, Direction8.North, Direction8.North);

