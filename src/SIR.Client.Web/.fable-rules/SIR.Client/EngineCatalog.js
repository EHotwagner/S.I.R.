
import { Record } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { record_type, list_type, int32_type, array_type, uint8_type, string_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { format, join } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { map } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { map as map_1, delay, toArray } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { rangeDouble } from "../fable_modules/fable-library-js.5.13.0/Range.js";
import { contains, tryFind, singleton } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { numberHash } from "../fable_modules/fable-library-js.5.13.0/Util.js";

/**
 * One immutable browser engine retained for replay compatibility.
 */
export class RetainedEngine extends Record {
    constructor(Version, Identity, EngineHash, ReplayFormatVersions, WorkerPath) {
        super();
        this.Version = Version;
        this.Identity = Identity;
        this.EngineHash = EngineHash;
        this.ReplayFormatVersions = ReplayFormatVersions;
        this.WorkerPath = WorkerPath;
    }
}

export function RetainedEngine_$reflection() {
    return record_type("SIR.Client.RetainedEngine", [], RetainedEngine, () => [["Version", string_type], ["Identity", string_type], ["EngineHash", array_type(uint8_type)], ["ReplayFormatVersions", list_type(int32_type)], ["WorkerPath", string_type]]);
}

function EngineCatalog_identity(bytes) {
    return join("", map((value) => format('{0:' + "x2" + '}', value), bytes));
}

export const EngineCatalog_Current = new RetainedEngine("v1", "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20", toArray(delay(() => map_1((value) => (value & 0xFF), rangeDouble(1, 1, 32)))), singleton(2), "engines/0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20/worker.js");

export const EngineCatalog_Retained = singleton(EngineCatalog_Current);

/**
 * Selects only a retained engine whose replay-format contract includes the package.
 */
export function EngineCatalog_tryFind(package$) {
    return tryFind((engine) => {
        if (engine.Identity === EngineCatalog_identity(package$.EngineHash)) {
            return contains(package$.FormatVersion, engine.ReplayFormatVersions, {
                Equals: (x, y) => (x === y),
                GetHashCode: (x) => (numberHash(x) | 0),
            });
        }
        else {
            return false;
        }
    }, EngineCatalog_Retained);
}

