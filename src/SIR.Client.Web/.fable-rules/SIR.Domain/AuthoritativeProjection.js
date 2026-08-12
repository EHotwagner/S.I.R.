
import { Record } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { uint8_type, array_type, int64_type, record_type, int32_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";

/**
 * One disclosed unit in a host-produced authoritative projection.
 */
export class QualifiedVisibleUnit extends Record {
    constructor(UnitId, DisplayColumn, DisplayRow, Health) {
        super();
        this.UnitId = (UnitId | 0);
        this.DisplayColumn = (DisplayColumn | 0);
        this.DisplayRow = (DisplayRow | 0);
        this.Health = (Health | 0);
    }
}

export function QualifiedVisibleUnit_$reflection() {
    return record_type("SIR.Domain.QualifiedVisibleUnit", [], QualifiedVisibleUnit, () => [["UnitId", int32_type], ["DisplayColumn", int32_type], ["DisplayRow", int32_type], ["Health", int32_type]]);
}

/**
 * Runtime-neutral handoff from the native match host to browser playback.
 * State and event identities cover only this disclosed projection. Complete
 * authoritative kernel identities remain server-side in replay verification,
 * so these fields cannot reveal changes to hidden state.
 */
export class AuthoritativeProjectionFrame extends Record {
    constructor(Tick, ServerSequence, ProjectionRevision, VisibleUnits, StateIdentity, EventIdentity) {
        super();
        this.Tick = (Tick | 0);
        this.ServerSequence = ServerSequence;
        this.ProjectionRevision = ProjectionRevision;
        this.VisibleUnits = VisibleUnits;
        this.StateIdentity = StateIdentity;
        this.EventIdentity = EventIdentity;
    }
}

export function AuthoritativeProjectionFrame_$reflection() {
    return record_type("SIR.Domain.AuthoritativeProjectionFrame", [], AuthoritativeProjectionFrame, () => [["Tick", int32_type], ["ServerSequence", int64_type], ["ProjectionRevision", int64_type], ["VisibleUnits", array_type(QualifiedVisibleUnit_$reflection())], ["StateIdentity", array_type(uint8_type)], ["EventIdentity", array_type(uint8_type)]]);
}

