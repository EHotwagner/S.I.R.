
import { toString } from "./fable_modules/fable-library-js.5.13.0/Types.js";

export function sharedSceneUnitCommand(model, unitId) {
    const matchValue = model.Workspace;
    switch (matchValue.tag) {
        case 1:
            return "planning.roster.select." + toString(unitId);
        case 0:
            return "simulator.scene.select.unit." + toString(unitId);
        case 3:
            return "review.scene.select.unit." + toString(unitId);
        default:
            return "editor.scene.select.unit." + toString(unitId);
    }
}

export function sharedSceneCellCommand(model, column, row) {
    const matchValue = model.Workspace;
    switch (matchValue.tag) {
        case 2:
            return (("editor.scene.cell." + toString(column)) + ".") + toString(row);
        case 1:
            return (("planning.battlefield.cell." + toString(column)) + ".") + toString(row);
        default:
            return undefined;
    }
}

