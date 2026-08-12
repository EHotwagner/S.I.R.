
import { int64ToString, int32ToString } from "./fable_modules/fable-library-js.5.13.0/Util.js";
import { FSharpResult$2 } from "./fable_modules/fable-library-js.5.13.0/Result.js";
import { isInfinity } from "./fable_modules/fable-library-js.5.13.0/Double.js";
import { fromFloat64, toInt64_unchecked } from "./fable_modules/fable-library-js.5.13.0/BigInt.js";
import { singleton } from "./fable_modules/fable-library-js.5.13.0/AsyncBuilder.js";
import { awaitPromise } from "./fable_modules/fable-library-js.5.13.0/Async.js";
import { initialize } from "./fable_modules/fable-library-js.5.13.0/Array.js";
import { Lab_export } from "./SIR.Client/Lab.js";
import { export$ } from "./SIR.Client/MapEditor.js";
import { iterate } from "./fable_modules/fable-library-js.5.13.0/List.js";
import { post } from "./Runner.js";
import { Cmd_ofEffect } from "./fable_modules/Fable.Elmish.5.0.2/cmd.fs.js";

function sizeError(label, maximum, file) {
    try {
        const size = file.size;
        return (((Number.isNaN(size) ? true : isInfinity(size)) ? true : (size < 0)) ? true : (size !== Math.floor(size))) ? (new FSharpResult$2(/* Error */ 1, [((label + " has invalid size metadata; the allowed maximum is ") + int32ToString(maximum)) + " bytes."])) : ((size > maximum) ? (new FSharpResult$2(/* Error */ 1, [((((label + " is ") + int64ToString(toInt64_unchecked(fromFloat64(size)))) + " bytes; the allowed maximum is ") + int32ToString(maximum)) + " bytes."])) : (new FSharpResult$2(/* Ok */ 0, [undefined])));
    }
    catch (matchValue) {
        return new FSharpResult$2(/* Error */ 1, [((label + " has unreadable size metadata; the allowed maximum is ") + int32ToString(maximum)) + " bytes."]);
    }
}

export function fileBytes(maximum, file) {
    return singleton.Delay(() => {
        const matchValue = sizeError("Replay package", maximum, file);
        return (matchValue.tag === 0) ? singleton.TryWith(singleton.Delay(() => singleton.Bind(awaitPromise(file.arrayBuffer()), (_arg) => {
            const typed = new Uint8Array(_arg);
            return singleton.Return(new FSharpResult$2(/* Ok */ 0, [[file.name, initialize(typed.length, (index) => (typed[index]), Uint8Array)]]));
        })), (_arg_1) => singleton.Return(new FSharpResult$2(/* Error */ 1, ["Replay package could not be read: " + _arg_1.message]))) : singleton.Return(new FSharpResult$2(/* Error */ 1, [matchValue.fields[0]]));
    });
}

export function fileText(maximum, file) {
    return singleton.Delay(() => {
        const matchValue = sizeError("Map import", maximum, file);
        return (matchValue.tag === 0) ? singleton.TryWith(singleton.Delay(() => singleton.Bind(awaitPromise(file.text()), (_arg) => singleton.Return(new FSharpResult$2(/* Ok */ 0, [[file.name, _arg]])))), (_arg_1) => singleton.Return(new FSharpResult$2(/* Error */ 1, ["Map import could not be read: " + _arg_1.message]))) : singleton.Return(new FSharpResult$2(/* Error */ 1, [matchValue.fields[0]]));
    });
}

export function rasterBytes(maximum, file) {
    return singleton.Delay(() => {
        const matchValue = sizeError("Raster background", maximum, file);
        return (matchValue.tag === 0) ? singleton.TryWith(singleton.Delay(() => singleton.Bind(awaitPromise(file.arrayBuffer()), (_arg) => {
            const typed = new Uint8Array(_arg);
            return singleton.Return(new FSharpResult$2(/* Ok */ 0, [[file.name, file.type, initialize(typed.length, (index) => (typed[index]), Uint8Array)]]));
        })), (_arg_1) => singleton.Return(new FSharpResult$2(/* Error */ 1, ["Raster background could not be read: " + _arg_1.message]))) : singleton.Return(new FSharpResult$2(/* Error */ 1, [matchValue.fields[0]]));
    });
}

export function downloadExperiment(report) {
    const content = Lab_export(report);
            const blob = new Blob([content], { type: "text/plain;charset=utf-8" });
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = "sir-lab-experiment.sir-lab";
        anchor.click();
        URL.revokeObjectURL(url);
        ;
}

export function downloadMap(state) {
    const content = export$(state);
            const blob = new Blob([content], { type: "text/plain;charset=utf-8" });
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = "battlefield.sir-map";
        anchor.click();
        URL.revokeObjectURL(url);
        ;
}

export function runEffects(effects) {
    iterate((_arg) => {
        post(_arg.fields[0], _arg.fields[1]);
    }, effects);
}

export function effectsToCmd(effects) {
    return Cmd_ofEffect((_arg) => {
        runEffects(effects);
    });
}

