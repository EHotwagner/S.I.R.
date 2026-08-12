
import { HeldInput, HeldInputSessionModule_contains } from "./SIR.Client/ModalInput.js";
import { defaultArg } from "./fable_modules/fable-library-js.5.13.0/Option.js";
import { toSeq as toSeq_1, contains, empty, ofSeq } from "./fable_modules/fable-library-js.5.13.0/Set.js";
import { tryHead, map } from "./fable_modules/fable-library-js.5.13.0/Seq.js";
import { toSeq } from "./fable_modules/fable-library-js.5.13.0/Map.js";
import { comparePrimitives } from "./fable_modules/fable-library-js.5.13.0/Util.js";

export function editorPanHeld(model) {
    return HeldInputSessionModule_contains(HeldInput.EditorPan, model.HeldInputs);
}

export function tacticalUnitIds(workspace, model) {
    let option_1, option_4;
    switch (workspace.tag) {
        case 0:
            return defaultArg((option_1 = model.Simulator, (option_1 != null) ? ofSeq(map((tuple_1) => (tuple_1[0] | 0), toSeq(option_1.RuntimeMap.Units)), {
                Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
            }) : undefined), empty({
                Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
            }));
        case 3:
            return defaultArg((option_4 = model.Shell.Inspection, (option_4 != null) ? ofSeq(map((_arg) => (_arg.Id | 0), option_4.Units), {
                Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
            }) : undefined), empty({
                Compare: (x_4, y_4) => (comparePrimitives(x_4, y_4) | 0),
            }));
        default:
            return ofSeq(map((tuple) => (tuple[0] | 0), toSeq(model.Editor.Map.Units)), {
                Compare: (x, y) => (comparePrimitives(x, y) | 0),
            });
    }
}

export function reconcileTacticalSelectedUnit(workspace, model) {
    let option_3;
    const visible = tacticalUnitIds(workspace, model);
    const keep = (candidate) => {
        const option_1 = candidate;
        if (option_1 != null) {
            if (contains(option_1, visible)) {
                return option_1;
            }
            else {
                return undefined;
            }
        }
        else {
            return undefined;
        }
    };
    let option_7;
    const option_5 = keep(model.TacticalSelectedUnit);
    option_7 = ((option_5 != null) ? option_5 : ((workspace.tag === 1) ? keep((option_3 = model.Planning, (option_3 != null) ? option_3.SelectedUnit : undefined)) : ((workspace.tag === 0) ? keep(model.SimulatorSelectedUnit) : ((workspace.tag === 3) ? keep(model.Shell.Selection.Unit) : keep(model.Editor.SelectedUnit)))));
    if (option_7 != null) {
        return option_7;
    }
    else {
        return tryHead(toSeq_1(visible));
    }
}

