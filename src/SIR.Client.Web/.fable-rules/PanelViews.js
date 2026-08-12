
import { defaultArg } from "./fable_modules/fable-library-js.5.13.0/Option.js";
import { HtmlHelper_createElement } from "./fable_modules/Feliz.3.3.3/Html.fs.js";
import { int32ToString } from "./fable_modules/fable-library-js.5.13.0/Util.js";
import { createElement } from "react";
import { defaultOf } from "./fable_modules/fable-library-js.5.13.0/Util.js";
import { ofArray, length } from "./fable_modules/fable-library-js.5.13.0/List.js";
import { singleton, delay, toList } from "./fable_modules/fable-library-js.5.13.0/Seq.js";
import { toString } from "./fable_modules/fable-library-js.5.13.0/Types.js";

/**
 * Read-only review panels are kept outside the Elmish composition root so the
 * root only selects a panel and owns dispatch, rather than its view details.
 */
export function reviewLayersPanel(model) {
    let value_3, children_18, value_6, value_9, value_12, value_15, option_6, option_4;
    const inspection = model.Inspection;
    const count = (select) => {
        let option_1;
        return defaultArg((option_1 = inspection, (option_1 != null) ? select(option_1) : undefined), 0) | 0;
    };
    return HtmlHelper_createElement("section", ofArray([["aria-label", "Review projection layers"], ["children", [(value_3 = (("Committed frame · tick " + int32ToString(model.Playback.CurrentTick)) + " · read-only"), createElement("p", defaultOf(), value_3)), (children_18 = ofArray([createElement("dt", defaultOf(), "Units"), (value_6 = int32ToString(count((value_5) => (length(value_5.Units) | 0))), createElement("dd", defaultOf(), value_6)), createElement("dt", defaultOf(), "Edges"), (value_9 = int32ToString(count((value_8) => (length(value_8.Edges) | 0))), createElement("dd", defaultOf(), value_9)), createElement("dt", defaultOf(), "Disclosed events"), (value_12 = int32ToString(count((value_11) => (length(value_11.Events) | 0))), createElement("dd", defaultOf(), value_12)), createElement("dt", defaultOf(), "Perspective"), (value_15 = defaultArg((option_6 = ((option_4 = inspection, (option_4 != null) ? option_4.PerspectiveHash : undefined)), (option_6 != null) ? ("Filtered · " + option_6) : undefined), "Full replay disclosure"), createElement("dd", defaultOf(), value_15))]), createElement("dl", defaultOf(), ...children_18))]]]));
}

export function reviewDocumentPanel(model) {
    return HtmlHelper_createElement("section", ofArray([["aria-label", "Review source and verification identity"], ["children", toList(delay(() => {
        let value_12, value_13, children_20, value_9, value_11;
        const matchValue = model.Source;
        switch (matchValue.tag) {
            case 1:
                return singleton((value_12 = (("Reading " + matchValue.fields[0]) + "."), createElement("p", defaultOf(), value_12)));
            case 3:
                return singleton((value_13 = ((matchValue.fields[0] + " rejected: ") + matchValue.fields[1]), createElement("p", defaultOf(), value_13)));
            case 0:
                return singleton(createElement("p", defaultOf(), "No replay package is loaded."));
            default: {
                const metadata = matchValue.fields[0];
                return singleton((children_20 = ofArray([createElement("dt", defaultOf(), "Source"), createElement("dd", defaultOf(), metadata.SourceName), createElement("dt", defaultOf(), "Source identity"), createElement("dd", defaultOf(), metadata.SourceIdentity), createElement("dt", defaultOf(), "Engine identity"), createElement("dd", defaultOf(), metadata.EngineIdentity), createElement("dt", defaultOf(), "Replay kind"), (value_9 = toString(metadata.Kind), createElement("dd", defaultOf(), value_9)), createElement("dt", defaultOf(), "Committed ticks"), (value_11 = int32ToString(metadata.FinalTick), createElement("dd", defaultOf(), value_11))]), createElement("dl", defaultOf(), ...children_20)));
            }
        }
    }))]]));
}

