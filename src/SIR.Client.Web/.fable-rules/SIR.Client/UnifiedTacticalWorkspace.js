
import { FSharpRef, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { class_type, list_type, bool_type, record_type, int64_type, option_type, int32_type, string_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { ModalInput_selectorsOverlap, ModalInput_isKnownCommandId, ModalInput_precedenceRank, ModalBinding$1, InputGesture, InputPhase, KeyModifiers, NormalizedKeyModule_create, NormalizedKeyModule_value, InputPhase_$reflection, ModalContextSelector_$reflection } from "./ModalInput.js";
import { ofList as ofList_1, FSharpMap__get_Count, FSharpMap__get_Item, exists as exists_1, toList as toList_1, remove, add, containsKey, tryFind, empty } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { compareArrays, min, max, int32ToString, equals, stringHash, compare, comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { count as count_1, intersect, isEmpty, contains, forAll, ofArray as ofArray_1, singleton, ofList } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { append as append_1, tryFind as tryFind_1, sortBy, tryPick, reverse, cons, empty as empty_2, fold, find, isEmpty as isEmpty_1, filter, singleton as singleton_2, exists, length, item as item_2, map as map_1, choose, ofArray } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { defaultArg, value as value_12 } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { substring, startsWith, isNullOrWhiteSpace, split, join, replace } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { collect, empty as empty_1, singleton as singleton_1, append, delay, toList } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { take, item as item_1, map } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { List_distinctBy } from "../fable_modules/fable-library-js.5.13.0/Seq2.js";
import { rangeDouble } from "../fable_modules/fable-library-js.5.13.0/Range.js";
import { Result_Map, FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { isControl, isDigit, isWhiteSpace } from "../fable_modules/fable-library-js.5.13.0/Char.js";
import { tryParse } from "../fable_modules/fable-library-js.5.13.0/Int32.js";
import { fromInt32, op_Subtraction, min as min_1, compare as compare_1, op_Addition, toInt64_unchecked, max as max_1 } from "../fable_modules/fable-library-js.5.13.0/BigInt.js";

/**
 * The four modalities that share the mounted tactical battlefield and time axis.
 */
export class TacticalModality extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Editor", "Plan", "Simulate", "Review"];
    }
    static Editor = new TacticalModality(0, []);
    static Plan = new TacticalModality(1, []);
    static Simulate = new TacticalModality(2, []);
    static Review = new TacticalModality(3, []);
}

export function TacticalModality_$reflection() {
    return union_type("SIR.Client.TacticalModality", [], TacticalModality, () => [[], [], [], []]);
}

/**
 * Disclosure channels shown on the unified timeline.
 */
export class TacticalTimeChannel extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Authored", "Predicted", "Accepted", "Committed"];
    }
    static Authored = new TacticalTimeChannel(0, []);
    static Predicted = new TacticalTimeChannel(1, []);
    static Accepted = new TacticalTimeChannel(2, []);
    static Committed = new TacticalTimeChannel(3, []);
}

export function TacticalTimeChannel_$reflection() {
    return union_type("SIR.Client.TacticalTimeChannel", [], TacticalTimeChannel, () => [[], [], [], []]);
}

/**
 * A non-authoritative timeline projection. Domain state remains owned by the
 * editor, planner, simulator, and replay models.
 */
export class TacticalTimelineSegment extends Record {
    constructor(Id, UnitId, StartTick, EndTick, Channel, Label, Issue) {
        super();
        this.Id = Id;
        this.UnitId = UnitId;
        this.StartTick = StartTick;
        this.EndTick = EndTick;
        this.Channel = Channel;
        this.Label = Label;
        this.Issue = Issue;
    }
}

export function TacticalTimelineSegment_$reflection() {
    return record_type("SIR.Client.TacticalTimelineSegment", [], TacticalTimelineSegment, () => [["Id", string_type], ["UnitId", option_type(int32_type)], ["StartTick", int64_type], ["EndTick", int64_type], ["Channel", TacticalTimeChannel_$reflection()], ["Label", string_type], ["Issue", option_type(string_type)]]);
}

export class TacticalTimelineState extends Record {
    constructor(Modality, Cursor, Horizon, CommittedThrough, IsPlaying, SelectedSegment, Segments) {
        super();
        this.Modality = Modality;
        this.Cursor = Cursor;
        this.Horizon = Horizon;
        this.CommittedThrough = CommittedThrough;
        this.IsPlaying = IsPlaying;
        this.SelectedSegment = SelectedSegment;
        this.Segments = Segments;
    }
}

export function TacticalTimelineState_$reflection() {
    return record_type("SIR.Client.TacticalTimelineState", [], TacticalTimelineState, () => [["Modality", TacticalModality_$reflection()], ["Cursor", int64_type], ["Horizon", int64_type], ["CommittedThrough", int64_type], ["IsPlaying", bool_type], ["SelectedSegment", option_type(string_type)], ["Segments", list_type(TacticalTimelineSegment_$reflection())]]);
}

export class TimelineEditError extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["InvalidTimelineRange", "CommittedInterval", "DuplicateTimelineSegment", "TimelineSegmentNotFound"];
    }
    static InvalidTimelineRange = new TimelineEditError(0, []);
    static CommittedInterval = new TimelineEditError(1, []);
}

export function TimelineEditError_$reflection() {
    return union_type("SIR.Client.TimelineEditError", [], TimelineEditError, () => [[], [], [["Item", string_type]], [["Item", string_type]]]);
}

export class TacticalCommandAvailability extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["AlwaysAvailable", "TimelineEditable", "TimelineSelectionRequired", "PredictionRequired", "CommittedHistoryRequired", "HelpOpenRequired", "PlanningAcceptedRequired", "PlanningIssuesRequired", "ReplayLoadedRequired", "ReplayEventsRequired", "ReplayOperationRequired"];
    }
    static AlwaysAvailable = new TacticalCommandAvailability(0, []);
    static TimelineEditable = new TacticalCommandAvailability(1, []);
    static TimelineSelectionRequired = new TacticalCommandAvailability(2, []);
    static PredictionRequired = new TacticalCommandAvailability(3, []);
    static CommittedHistoryRequired = new TacticalCommandAvailability(4, []);
    static HelpOpenRequired = new TacticalCommandAvailability(5, []);
    static PlanningAcceptedRequired = new TacticalCommandAvailability(6, []);
    static PlanningIssuesRequired = new TacticalCommandAvailability(7, []);
    static ReplayLoadedRequired = new TacticalCommandAvailability(8, []);
    static ReplayEventsRequired = new TacticalCommandAvailability(9, []);
    static ReplayOperationRequired = new TacticalCommandAvailability(10, []);
}

export function TacticalCommandAvailability_$reflection() {
    return union_type("SIR.Client.TacticalCommandAvailability", [], TacticalCommandAvailability, () => [[], [], [], [], [], [], [], [], [], [], []]);
}

/**
 * Inspectable command metadata shared by dispatch, menus, help, conflict
 * validation, and binding configuration.
 */
export class TacticalCommandDefinition extends Record {
    constructor(Id, Label, Category, Modalities, DefaultGesture, PointerAvailable, Precedence, ModalContext, ModalPhase, Availability) {
        super();
        this.Id = Id;
        this.Label = Label;
        this.Category = Category;
        this.Modalities = Modalities;
        this.DefaultGesture = DefaultGesture;
        this.PointerAvailable = PointerAvailable;
        this.Precedence = (Precedence | 0);
        this.ModalContext = ModalContext;
        this.ModalPhase = ModalPhase;
        this.Availability = Availability;
    }
}

export function TacticalCommandDefinition_$reflection() {
    return record_type("SIR.Client.TacticalCommandDefinition", [], TacticalCommandDefinition, () => [["Id", string_type], ["Label", string_type], ["Category", string_type], ["Modalities", class_type("Microsoft.FSharp.Collections.FSharpSet`1", [TacticalModality_$reflection()])], ["DefaultGesture", option_type(string_type)], ["PointerAvailable", bool_type], ["Precedence", int32_type], ["ModalContext", option_type(ModalContextSelector_$reflection())], ["ModalPhase", option_type(InputPhase_$reflection())], ["Availability", TacticalCommandAvailability_$reflection()]]);
}

export class TacticalBindingOverride extends Record {
    constructor(CommandId, Gesture) {
        super();
        this.CommandId = CommandId;
        this.Gesture = Gesture;
    }
}

export function TacticalBindingOverride_$reflection() {
    return record_type("SIR.Client.TacticalBindingOverride", [], TacticalBindingOverride, () => [["CommandId", string_type], ["Gesture", option_type(string_type)]]);
}

export class TacticalBindingProfile extends Record {
    constructor(SchemaVersion, Overrides) {
        super();
        this.SchemaVersion = (SchemaVersion | 0);
        this.Overrides = Overrides;
    }
}

export function TacticalBindingProfile_$reflection() {
    return record_type("SIR.Client.TacticalBindingProfile", [], TacticalBindingProfile, () => [["SchemaVersion", int32_type], ["Overrides", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, option_type(string_type)])]]);
}

export class TacticalBindingDiagnostic extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["UnknownTacticalCommand", "ReservedTacticalGesture", "TacticalBindingConflict", "MalformedTacticalBindingProfile", "UnsupportedTacticalBindingSchema"];
    }
}

export function TacticalBindingDiagnostic_$reflection() {
    return union_type("SIR.Client.TacticalBindingDiagnostic", [], TacticalBindingDiagnostic, () => [[["Item", string_type]], [["commandId", string_type], ["gesture", string_type]], [["firstCommandId", string_type], ["secondCommandId", string_type], ["gesture", string_type]], [["Item", string_type]], [["Item", int32_type]]]);
}

export class ShortcutPlatform extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ControlPlatform", "MetaPlatform"];
    }
    static ControlPlatform = new ShortcutPlatform(0, []);
    static MetaPlatform = new ShortcutPlatform(1, []);
}

export function ShortcutPlatform_$reflection() {
    return union_type("SIR.Client.ShortcutPlatform", [], ShortcutPlatform, () => [[], []]);
}

export const UnifiedTacticalWorkspace_emptyBindingProfile = new TacticalBindingProfile(1, empty({
    Compare: (x, y) => (comparePrimitives(x, y) | 0),
}));

export const UnifiedTacticalWorkspace_commandRegistry = (() => {
    const all = ofList(ofArray([TacticalModality.Editor, TacticalModality.Plan, TacticalModality.Simulate, TacticalModality.Review]), {
        Compare: (x, y) => (compare(x, y) | 0),
    });
    const command = (id, label, category, modalities, gesture, pointer, precedence, availability) => (new TacticalCommandDefinition(id, label, category, modalities, gesture, pointer, precedence, undefined, undefined, availability));
    return ofArray([command("workspace.editor", "Switch to Editor", "Modality", all, "Ctrl+Shift+1", true, 10, TacticalCommandAvailability.AlwaysAvailable), command("workspace.plan", "Switch to Plan", "Modality", all, "Ctrl+Shift+2", true, 10, TacticalCommandAvailability.AlwaysAvailable), command("workspace.simulate", "Switch to Simulate", "Modality", all, "Ctrl+Shift+3", true, 10, TacticalCommandAvailability.AlwaysAvailable), command("workspace.review", "Switch to Review", "Modality", all, "Ctrl+Shift+4", true, 10, TacticalCommandAvailability.AlwaysAvailable), command("timeline.play-toggle", "Play or pause", "Timeline", all, "Space", true, 20, TacticalCommandAvailability.AlwaysAvailable), command("timeline.step-back", "Step backward", "Timeline", all, "Ctrl+ArrowLeft", true, 20, TacticalCommandAvailability.AlwaysAvailable), command("timeline.step-forward", "Step forward", "Timeline", all, "Ctrl+ArrowRight", true, 20, TacticalCommandAvailability.AlwaysAvailable), command("timeline.home", "Go to timeline start", "Timeline", all, "Ctrl+Home", true, 20, TacticalCommandAvailability.AlwaysAvailable), command("timeline.end", "Go to timeline end", "Timeline", all, "Ctrl+End", true, 20, TacticalCommandAvailability.AlwaysAvailable), command("timeline.move-command", "Move selected command to current time", "Timeline", singleton(TacticalModality.Plan, {
        Compare: (x_1, y_1) => (compare(x_1, y_1) | 0),
    }), undefined, true, 30, TacticalCommandAvailability.TimelineSelectionRequired), command("timeline.remove-command", "Remove selected command", "Timeline", singleton(TacticalModality.Plan, {
        Compare: (x_2, y_2) => (compare(x_2, y_2) | 0),
    }), "Delete", true, 30, TacticalCommandAvailability.TimelineSelectionRequired), command("planning.undo", "Undo plan edit", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_3, y_3) => (compare(x_3, y_3) | 0),
    }), "Ctrl+Z", true, 30, TacticalCommandAvailability.TimelineEditable), command("planning.redo", "Redo plan edit", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_4, y_4) => (compare(x_4, y_4) | 0),
    }), "Ctrl+Shift+Z", true, 30, TacticalCommandAvailability.TimelineEditable), command("planning.route", "Choose route tool", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_5, y_5) => (compare(x_5, y_5) | 0),
    }), "R", true, 30, TacticalCommandAvailability.TimelineEditable), command("planning.facing", "Choose facing tool", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_6, y_6) => (compare(x_6, y_6) | 0),
    }), "F", true, 30, TacticalCommandAvailability.TimelineEditable), command("planning.attention", "Choose attention tool", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_7, y_7) => (compare(x_7, y_7) | 0),
    }), "A", true, 30, TacticalCommandAvailability.TimelineEditable), command("planning.stance", "Choose stance tool", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_8, y_8) => (compare(x_8, y_8) | 0),
    }), "S", true, 30, TacticalCommandAvailability.TimelineEditable), command("planning.hold", "Choose hold tool", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_9, y_9) => (compare(x_9, y_9) | 0),
    }), "H", true, 30, TacticalCommandAvailability.TimelineEditable), command("planning.engagement", "Choose engagement tool", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_10, y_10) => (compare(x_10, y_10) | 0),
    }), "E", true, 30, TacticalCommandAvailability.TimelineEditable), command("planning.synchronization", "Choose synchronization tool", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_11, y_11) => (compare(x_11, y_11) | 0),
    }), "M", true, 30, TacticalCommandAvailability.TimelineEditable), command("planning.validate", "Validate authored revision", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_12, y_12) => (compare(x_12, y_12) | 0),
    }), undefined, true, 40, TacticalCommandAvailability.TimelineEditable), command("planning.preview", "Preview intent-only prediction", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_13, y_13) => (compare(x_13, y_13) | 0),
    }), undefined, true, 40, TacticalCommandAvailability.TimelineEditable), command("planning.commit", "Commit accepted revision", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_14, y_14) => (compare(x_14, y_14) | 0),
    }), undefined, true, 40, TacticalCommandAvailability.PlanningAcceptedRequired), command("planning.issue.previous", "Previous validation issue", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_15, y_15) => (compare(x_15, y_15) | 0),
    }), "[", true, 40, TacticalCommandAvailability.PlanningIssuesRequired), command("planning.issue.next", "Next validation issue", "Plan", singleton(TacticalModality.Plan, {
        Compare: (x_16, y_16) => (compare(x_16, y_16) | 0),
    }), "]", true, 40, TacticalCommandAvailability.PlanningIssuesRequired), command("review.previous-event", "Previous disclosed event", "Review", singleton(TacticalModality.Review, {
        Compare: (x_17, y_17) => (compare(x_17, y_17) | 0),
    }), "[", true, 40, TacticalCommandAvailability.ReplayEventsRequired), command("review.next-event", "Next disclosed event", "Review", singleton(TacticalModality.Review, {
        Compare: (x_18, y_18) => (compare(x_18, y_18) | 0),
    }), "]", true, 40, TacticalCommandAvailability.ReplayEventsRequired), command("review.cancel", "Cancel replay operation", "Review", singleton(TacticalModality.Review, {
        Compare: (x_19, y_19) => (compare(x_19, y_19) | 0),
    }), "Escape", true, 50, TacticalCommandAvailability.ReplayOperationRequired), command("input.help", "Show contextual actions", "Help", all, "?", true, 100, TacticalCommandAvailability.AlwaysAvailable), command("input.help.close", "Close contextual actions", "Help", all, "Escape", true, 110, TacticalCommandAvailability.HelpOpenRequired), command("input.bindings", "Configure command bindings", "Help", all, undefined, true, 100, TacticalCommandAvailability.AlwaysAvailable)]);
})();

export function UnifiedTacticalWorkspace_effectiveGesture(profile, command) {
    const matchValue = tryFind(command.Id, profile.Overrides);
    if (matchValue == null) {
        return command.DefaultGesture;
    }
    else {
        return value_12(matchValue);
    }
}

export function UnifiedTacticalWorkspace_isRebound(profile, command) {
    return containsKey(command.Id, profile.Overrides);
}

/**
 * Formats the registry's effective binding for visible command presentation.
 */
export function UnifiedTacticalWorkspace_displayGesture(gesture) {
    return defaultArg(gesture, "Unassigned");
}

/**
 * Formats the portable registry gesture for the platform that will activate it.
 */
export function UnifiedTacticalWorkspace_displayGestureFor(platform, gesture) {
    const value = UnifiedTacticalWorkspace_displayGesture(gesture);
    if (platform.tag === 1) {
        return replace(replace(value, "Ctrl/Cmd", "Cmd"), "Ctrl", "Cmd");
    }
    else {
        return replace(value, "Ctrl/Cmd", "Ctrl");
    }
}

/**
 * Converts a registry gesture to the token form expected by aria-keyshortcuts.
 */
export function UnifiedTacticalWorkspace_accessibleGesture(gesture) {
    const option_1 = gesture;
    if (option_1 != null) {
        return replace(replace(replace(replace(replace(replace(replace(replace(option_1, "Ctrl/Cmd", "Control"), "Ctrl", "Control"), "Cmd", "Meta"), "Esc", "Escape"), "←", "ArrowLeft"), "→", "ArrowRight"), "↑", "ArrowUp"), "↓", "ArrowDown");
    }
    else {
        return undefined;
    }
}

/**
 * Formats ARIA shortcut tokens for the same platform-specific presentation.
 */
export function UnifiedTacticalWorkspace_accessibleGestureFor(platform, gesture) {
    let option_1, value;
    return UnifiedTacticalWorkspace_accessibleGesture((option_1 = gesture, (option_1 != null) ? ((value = option_1, (platform.tag === 1) ? replace(replace(value, "Ctrl/Cmd", "Meta"), "Ctrl", "Meta") : replace(replace(value, "Ctrl/Cmd", "Control"), "Ctrl", "Control"))) : undefined));
}

function UnifiedTacticalWorkspace_normalizedGesture(gesture) {
    return gesture.trim().toUpperCase();
}

export function UnifiedTacticalWorkspace_gestureText(gesture) {
    let key;
    const matchValue = NormalizedKeyModule_value(gesture.Key);
    key = ((matchValue === "Space") ? "Space" : ((matchValue.length === 1) ? matchValue.toUpperCase() : matchValue));
    return join("+", toList(delay(() => append(gesture.Modifiers.ControlOrMeta ? singleton_1("Ctrl") : empty_1(), delay(() => append(gesture.Modifiers.Alt ? singleton_1("Alt") : empty_1(), delay(() => append((gesture.Modifiers.Shift && (NormalizedKeyModule_value(gesture.Key) !== "?")) ? singleton_1("Shift") : empty_1(), delay(() => singleton_1(key))))))))));
}

export function UnifiedTacticalWorkspace_tryParseGesture(text) {
    let ControlOrMeta, Alt;
    let parts;
    const array_1 = map((_arg) => _arg.trim(), split(text, ["+"], undefined, 1));
    parts = array_1.filter((arg) => !isNullOrWhiteSpace(arg));
    if (parts.length === 0) {
        return undefined;
    }
    else {
        const key = item_1(parts.length - 1, parts);
        const modifiers = ofArray_1(map((_arg_1) => _arg_1.toUpperCase(), take(parts.length - 1, parts)), {
            Compare: (x, y) => (comparePrimitives(x, y) | 0),
        });
        if (!forAll((value_2) => {
            if (((((value_2 === "CTRL") ? true : (value_2 === "CONTROL")) ? true : (value_2 === "CMD")) ? true : (value_2 === "META")) ? true : (value_2 === "ALT")) {
                return true;
            }
            else {
                return value_2 === "SHIFT";
            }
        }, modifiers) ? true : isNullOrWhiteSpace(key)) {
            return undefined;
        }
        else {
            return new InputGesture(NormalizedKeyModule_create(key, undefined), (ControlOrMeta = (((contains("CTRL", modifiers) ? true : contains("CONTROL", modifiers)) ? true : contains("CMD", modifiers)) ? true : contains("META", modifiers)), (Alt = contains("ALT", modifiers), new KeyModifiers(ControlOrMeta, contains("SHIFT", modifiers), Alt))), InputPhase.KeyDown);
        }
    }
}

export function UnifiedTacticalWorkspace_adaptModalCatalog(profile, catalog) {
    return choose((binding) => {
        let gesture;
        const matchValue = tryFind(binding.Id, profile.Overrides);
        if (matchValue == null) {
            return binding;
        }
        else if (value_12(matchValue) != null) {
            const text = value_12(matchValue);
            const option_1 = UnifiedTacticalWorkspace_tryParseGesture(text);
            if (option_1 != null) {
                return (gesture = option_1, new ModalBinding$1(binding.Id, binding.Context, binding.Precedence, new InputGesture(gesture.Key, gesture.Modifiers, binding.BindingGesture.Phase), binding.Label, binding.Group, binding.Repeat, binding.Availability, binding.Command));
            }
            else {
                return undefined;
            }
        }
        else {
            return undefined;
        }
    }, catalog);
}

export function UnifiedTacticalWorkspace_modalCommandDefinitions(modality, catalog) {
    return map_1((binding) => (new TacticalCommandDefinition(binding.Id, binding.Label, binding.Group, singleton(modality, {
        Compare: (x_1, y_1) => (compare(x_1, y_1) | 0),
    }), UnifiedTacticalWorkspace_gestureText(binding.BindingGesture), true, ModalInput_precedenceRank(binding.Precedence), binding.Context, binding.BindingGesture.Phase, TacticalCommandAvailability.AlwaysAvailable)), List_distinctBy((_arg) => _arg.Id, catalog, {
        Equals: (x, y) => (x === y),
        GetHashCode: (x) => (stringHash(x) | 0),
    }));
}

function UnifiedTacticalWorkspace_reservedGesture(gesture) {
    const matchValue = UnifiedTacticalWorkspace_normalizedGesture(gesture);
    switch (matchValue) {
        case "CTRL+L":
        case "CTRL+T":
        case "CTRL+W":
        case "CTRL+R":
        case "CTRL+SHIFT+R":
        case "ALT+F4":
        case "F5":
            return true;
        case "F6":
        case "ALT+ARROWLEFT":
        case "ALT+ARROWRIGHT":
        case "ALT+HOME":
        case "ALT+END":
        case "CTRL+N":
        case "CTRL+P":
        case "CTRL+S":
        case "CTRL+O":
            return true;
        default:
            return false;
    }
}

export function UnifiedTacticalWorkspace_validateBindings(registry, profile) {
    const known = ofList(map_1((_arg) => _arg.Id, registry), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    return toList(delay(() => append(collect((matchValue) => {
        const activePatternResult = matchValue;
        const id = activePatternResult[0];
        return append((!contains(id, known) && !ModalInput_isKnownCommandId(id)) ? singleton_1(new TacticalBindingDiagnostic(/* UnknownTacticalCommand */ 0, [id])) : empty_1(), delay(() => {
            const matchValue_1 = activePatternResult[1];
            let matchResult, value_2, value_3;
            if (matchValue_1 != null) {
                if (isNullOrWhiteSpace(matchValue_1)) {
                    matchResult = 0;
                    value_2 = matchValue_1;
                }
                else if (UnifiedTacticalWorkspace_reservedGesture(matchValue_1)) {
                    matchResult = 1;
                    value_3 = matchValue_1;
                }
                else {
                    matchResult = 2;
                }
            }
            else {
                matchResult = 2;
            }
            switch (matchResult) {
                case 0:
                    return singleton_1(new TacticalBindingDiagnostic(/* MalformedTacticalBindingProfile */ 3, [("Empty gesture for " + id) + "."]));
                case 1:
                    return singleton_1(new TacticalBindingDiagnostic(/* ReservedTacticalGesture */ 1, [id, value_3]));
                default: {
                    return empty_1();
                }
            }
        }));
    }, profile.Overrides), delay(() => {
        const effective = choose((command) => {
            const option_1 = UnifiedTacticalWorkspace_effectiveGesture(profile, command);
            if (option_1 != null) {
                return [command, UnifiedTacticalWorkspace_normalizedGesture(option_1)];
            }
            else {
                return undefined;
            }
        }, registry);
        return collect((firstIndex) => collect((secondIndex) => {
            let matchValue_2, matchValue_3, left, right, matchValue_5, matchValue_6, left_1, right_1;
            const patternInput = item_2(firstIndex, effective);
            const firstGesture = patternInput[1];
            const first = patternInput[0];
            const patternInput_1 = item_2(secondIndex, effective);
            const second = patternInput_1[0];
            return (((((firstGesture === patternInput_1[1]) && !isEmpty(intersect(first.Modalities, second.Modalities))) && (first.Precedence === second.Precedence)) && ((matchValue_2 = first.ModalContext, (matchValue_3 = second.ModalContext, (matchValue_2 != null) ? ((matchValue_3 != null) ? ((left = matchValue_2, (right = matchValue_3, ModalInput_selectorsOverlap(left, right)))) : true) : true)))) && ((matchValue_5 = first.ModalPhase, (matchValue_6 = second.ModalPhase, (matchValue_5 != null) ? ((matchValue_6 != null) ? ((left_1 = matchValue_5, (right_1 = matchValue_6, equals(left_1, right_1)))) : true) : true)))) ? singleton_1(new TacticalBindingDiagnostic(/* TacticalBindingConflict */ 2, [first.Id, second.Id, firstGesture])) : empty_1();
        }, rangeDouble(firstIndex + 1, 1, length(effective) - 1)), rangeDouble(0, 1, length(effective) - 1));
    }))));
}

export function UnifiedTacticalWorkspace_setBinding(registry, commandId, gesture, replaceConflict, profile) {
    if (!exists((command) => (command.Id === commandId), registry)) {
        return new FSharpResult$2(/* Error */ 1, [singleton_2(new TacticalBindingDiagnostic(/* UnknownTacticalCommand */ 0, [commandId]))]);
    }
    else {
        const candidate = new TacticalBindingProfile(profile.SchemaVersion, add(commandId, gesture, profile.Overrides));
        const diagnostics = UnifiedTacticalWorkspace_validateBindings(registry, candidate);
        if (!isEmpty_1(filter((_arg) => {
            let matchResult;
            if (_arg.tag === 2) {
                if (replaceConflict) {
                    matchResult = 0;
                }
                else {
                    matchResult = 1;
                }
            }
            else {
                matchResult = 1;
            }
            switch (matchResult) {
                case 0:
                    return false;
                default:
                    return true;
            }
        }, diagnostics))) {
            return new FSharpResult$2(/* Error */ 1, [diagnostics]);
        }
        else if (replaceConflict) {
            const target = find((command_1) => (command_1.Id === commandId), registry);
            let targetGesture;
            const option_1 = UnifiedTacticalWorkspace_effectiveGesture(candidate, target);
            targetGesture = ((option_1 != null) ? UnifiedTacticalWorkspace_normalizedGesture(option_1) : undefined);
            return new FSharpResult$2(/* Ok */ 0, [new TacticalBindingProfile(candidate.SchemaVersion, fold((overrides, id) => add(id, undefined, overrides), candidate.Overrides, choose((command_2) => {
                let matchValue_2, matchValue_3, left_1, right_1, matchValue_5, matchValue_6, left_2, right_2;
                if (command_2.Id === commandId) {
                    return undefined;
                }
                else {
                    const matchValue = UnifiedTacticalWorkspace_effectiveGesture(candidate, command_2);
                    let matchResult_1, left_3, right_3;
                    if (targetGesture != null) {
                        if (matchValue != null) {
                            if (((((targetGesture === UnifiedTacticalWorkspace_normalizedGesture(matchValue)) && !isEmpty(intersect(target.Modalities, command_2.Modalities))) && (target.Precedence === command_2.Precedence)) && ((matchValue_2 = target.ModalContext, (matchValue_3 = command_2.ModalContext, (matchValue_2 != null) ? ((matchValue_3 != null) ? ((left_1 = matchValue_2, (right_1 = matchValue_3, ModalInput_selectorsOverlap(left_1, right_1)))) : true) : true)))) && ((matchValue_5 = target.ModalPhase, (matchValue_6 = command_2.ModalPhase, (matchValue_5 != null) ? ((matchValue_6 != null) ? ((left_2 = matchValue_5, (right_2 = matchValue_6, equals(left_2, right_2)))) : true) : true)))) {
                                matchResult_1 = 0;
                                left_3 = targetGesture;
                                right_3 = matchValue;
                            }
                            else {
                                matchResult_1 = 1;
                            }
                        }
                        else {
                            matchResult_1 = 1;
                        }
                    }
                    else {
                        matchResult_1 = 1;
                    }
                    switch (matchResult_1) {
                        case 0:
                            return command_2.Id;
                        default:
                            return undefined;
                    }
                }
            }, registry)))]);
        }
        else {
            return new FSharpResult$2(/* Ok */ 0, [candidate]);
        }
    }
}

export function UnifiedTacticalWorkspace_restoreCommand(commandId, profile) {
    return new TacticalBindingProfile(profile.SchemaVersion, remove(commandId, profile.Overrides));
}

export function UnifiedTacticalWorkspace_restoreModality(registry, modality, profile) {
    return new TacticalBindingProfile(profile.SchemaVersion, fold((overrides, id) => remove(id, overrides), profile.Overrides, map_1((_arg) => _arg.Id, filter((command) => contains(modality, command.Modalities), registry))));
}

export function UnifiedTacticalWorkspace_restoreAll(_arg) {
    return UnifiedTacticalWorkspace_emptyBindingProfile;
}

function UnifiedTacticalWorkspace_escapeJson(value) {
    return replace(replace(value, "\\", "\\\\"), "\"", "\\\"");
}

export function UnifiedTacticalWorkspace_exportBindings(profile) {
    const bindings = join(",", map_1((tupledArg) => {
        let option_1;
        return ((("{\"id\":\"" + UnifiedTacticalWorkspace_escapeJson(tupledArg[0])) + "\",\"gesture\":") + defaultArg((option_1 = tupledArg[1], (option_1 != null) ? (("\"" + UnifiedTacticalWorkspace_escapeJson(option_1)) + "\"") : undefined), "null")) + "}";
    }, toList_1(profile.Overrides)));
    return ((("{\"schemaVersion\":" + int32ToString(1)) + ",\"bindings\":[") + bindings) + "]}";
}

class UnifiedTacticalWorkspace_StrictJson extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["JsonNull", "JsonString", "JsonNumber", "JsonArray", "JsonObject"];
    }
    static JsonNull = new UnifiedTacticalWorkspace_StrictJson(0, []);
}

function UnifiedTacticalWorkspace_StrictJson_$reflection() {
    return union_type("SIR.Client.UnifiedTacticalWorkspace.StrictJson", [], UnifiedTacticalWorkspace_StrictJson, () => [[], [["Item", string_type]], [["Item", int32_type]], [["Item", list_type(UnifiedTacticalWorkspace_StrictJson_$reflection())]], [["Item", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, UnifiedTacticalWorkspace_StrictJson_$reflection()])]]]);
}

function UnifiedTacticalWorkspace_parseStrictJson(source) {
    let index = 0;
    const malformed = (detail) => (new FSharpResult$2(/* Error */ 1, [new TacticalBindingDiagnostic(/* MalformedTacticalBindingProfile */ 3, [detail])]));
    const skipWhitespace = () => {
        while ((index < source.length) && isWhiteSpace(source[index])) {
            index = ((index + 1) | 0);
        }
    };
    const parseValue = () => {
        let value, value_1;
        skipWhitespace();
        if (index >= source.length) {
            return malformed("Unexpected end of JSON.");
        }
        else {
            const matchValue = source[index];
            let matchResult, value_2, value_3;
            switch (matchValue) {
                case "\"": {
                    matchResult = 0;
                    break;
                }
                case "[": {
                    matchResult = 2;
                    break;
                }
                case "n": {
                    if (startsWith(substring(source, index), "null", 4)) {
                        matchResult = 3;
                    }
                    else if ((value = matchValue, (value === "-") ? true : isDigit(value))) {
                        matchResult = 4;
                        value_2 = matchValue;
                    }
                    else {
                        matchResult = 5;
                        value_3 = matchValue;
                    }
                    break;
                }
                case "{": {
                    matchResult = 1;
                    break;
                }
                default:
                    if ((value_1 = matchValue, (value_1 === "-") ? true : isDigit(value_1))) {
                        matchResult = 4;
                        value_2 = matchValue;
                    }
                    else {
                        matchResult = 5;
                        value_3 = matchValue;
                    }
            }
            switch (matchResult) {
                case 0:
                    return Result_Map((Item) => (new UnifiedTacticalWorkspace_StrictJson(/* JsonString */ 1, [Item])), parseString());
                case 1:
                    return parseObject();
                case 2:
                    return parseArray();
                case 3: {
                    index = ((index + 4) | 0);
                    return new FSharpResult$2(/* Ok */ 0, [UnifiedTacticalWorkspace_StrictJson.JsonNull]);
                }
                case 4:
                    return Result_Map((Item_1) => (new UnifiedTacticalWorkspace_StrictJson(/* JsonNumber */ 2, [Item_1])), parseNumber());
                default:
                    return malformed(("Unexpected JSON token " + value_3) + ".");
            }
        }
    };
    const parseString = () => {
        index = ((index + 1) | 0);
        let parsed = "";
        let finished = false;
        let error = undefined;
        while (((index < source.length) && !finished) && (error == null)) {
            const matchValue_1 = source[index];
            switch (matchValue_1) {
                case "\"": {
                    finished = true;
                    index = ((index + 1) | 0);
                    break;
                }
                case "\\": {
                    index = ((index + 1) | 0);
                    if (index >= source.length) {
                        error = "Incomplete JSON escape.";
                    }
                    else {
                        let escaped;
                        const matchValue_2 = source[index];
                        escaped = ((matchValue_2 === "\"") ? "\"" : ((matchValue_2 === "/") ? "/" : ((matchValue_2 === "\\") ? "\\" : ((matchValue_2 === "b") ? "\b" : ((matchValue_2 === "f") ? "\f" : ((matchValue_2 === "n") ? "\n" : ((matchValue_2 === "r") ? "\r" : ((matchValue_2 === "t") ? "\t" : undefined))))))));
                        if (escaped == null) {
                            error = "Unsupported or malformed JSON escape.";
                        }
                        else {
                            const value_5 = escaped;
                            parsed = (parsed + value_5);
                            index = ((index + 1) | 0);
                        }
                    }
                    break;
                }
                default:
                    if (isControl(matchValue_1)) {
                        error = "Control character in JSON string.";
                    }
                    else {
                        parsed = (parsed + matchValue_1);
                        index = ((index + 1) | 0);
                    }
            }
        }
        if (error == null) {
            if (!finished) {
                return malformed("Unterminated JSON string.");
            }
            else {
                return new FSharpResult$2(/* Ok */ 0, [parsed]);
            }
        }
        else {
            return malformed(error);
        }
    };
    const parseNumber = () => {
        const start = index | 0;
        if (source[index] === "-") {
            index = ((index + 1) | 0);
        }
        while ((index < source.length) && isDigit(source[index])) {
            index = ((index + 1) | 0);
        }
        let matchValue_3;
        let outArg = 0;
        matchValue_3 = [tryParse(substring(source, start, index - start), 511, false, 32, new FSharpRef(() => (outArg | 0), (v) => {
            outArg = (v | 0);
        })), outArg];
        if (matchValue_3[0]) {
            return new FSharpResult$2(/* Ok */ 0, [matchValue_3[1]]);
        }
        else {
            return malformed("JSON number must be a 32-bit integer.");
        }
    };
    const parseArray = () => {
        index = ((index + 1) | 0);
        skipWhitespace();
        let values = empty_2();
        let done$0027 = false;
        let error_1 = undefined;
        if ((index < source.length) && (source[index] === "]")) {
            index = ((index + 1) | 0);
            done$0027 = true;
        }
        while (!done$0027 && (error_1 == null)) {
            const matchValue_4 = parseValue();
            if (matchValue_4.tag === 0) {
                values = cons(matchValue_4.fields[0], values);
                skipWhitespace();
                if ((index < source.length) && (source[index] === ",")) {
                    index = ((index + 1) | 0);
                }
                else if ((index < source.length) && (source[index] === "]")) {
                    index = ((index + 1) | 0);
                    done$0027 = true;
                }
                else {
                    error_1 = (new TacticalBindingDiagnostic(/* MalformedTacticalBindingProfile */ 3, ["Expected \',\' or \']\' in JSON array."]));
                }
            }
            else {
                error_1 = matchValue_4.fields[0];
            }
        }
        if (error_1 == null) {
            return new FSharpResult$2(/* Ok */ 0, [new UnifiedTacticalWorkspace_StrictJson(/* JsonArray */ 3, [reverse(values)])]);
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [error_1]);
        }
    };
    const parseObject = () => {
        index = ((index + 1) | 0);
        skipWhitespace();
        let fields = empty({
            Compare: (x, y) => (comparePrimitives(x, y) | 0),
        });
        let done$0027_1 = false;
        let error_2 = undefined;
        if ((index < source.length) && (source[index] === "}")) {
            index = ((index + 1) | 0);
            done$0027_1 = true;
        }
        while (!done$0027_1 && (error_2 == null)) {
            const matchValue_5 = parseString();
            if (matchValue_5.tag === 0) {
                if (containsKey(matchValue_5.fields[0], fields)) {
                    error_2 = (new TacticalBindingDiagnostic(/* MalformedTacticalBindingProfile */ 3, [("Duplicate JSON field " + matchValue_5.fields[0]) + "."]));
                }
                else {
                    skipWhitespace();
                    if ((index >= source.length) ? true : (source[index] !== ":")) {
                        error_2 = (new TacticalBindingDiagnostic(/* MalformedTacticalBindingProfile */ 3, ["Expected \':\' in JSON object."]));
                    }
                    else {
                        index = ((index + 1) | 0);
                        const matchValue_6 = parseValue();
                        if (matchValue_6.tag === 0) {
                            fields = add(matchValue_5.fields[0], matchValue_6.fields[0], fields);
                            skipWhitespace();
                            if ((index < source.length) && (source[index] === ",")) {
                                index = ((index + 1) | 0);
                                skipWhitespace();
                            }
                            else if ((index < source.length) && (source[index] === "}")) {
                                index = ((index + 1) | 0);
                                done$0027_1 = true;
                            }
                            else {
                                error_2 = (new TacticalBindingDiagnostic(/* MalformedTacticalBindingProfile */ 3, ["Expected \',\' or \'}\' in JSON object."]));
                            }
                        }
                        else {
                            error_2 = matchValue_6.fields[0];
                        }
                    }
                }
            }
            else {
                error_2 = matchValue_5.fields[0];
            }
        }
        if (error_2 == null) {
            return new FSharpResult$2(/* Ok */ 0, [new UnifiedTacticalWorkspace_StrictJson(/* JsonObject */ 4, [fields])]);
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [error_2]);
        }
    };
    const matchValue_7 = parseValue();
    if (matchValue_7.tag === 0) {
        skipWhitespace();
        if (index === source.length) {
            return new FSharpResult$2(/* Ok */ 0, [matchValue_7.fields[0]]);
        }
        else {
            return malformed("Trailing content after JSON value.");
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [matchValue_7.fields[0]]);
    }
}

export function UnifiedTacticalWorkspace_importBindings(registry, json) {
    let value_1;
    const invalid = (detail) => (new FSharpResult$2(/* Error */ 1, [singleton_2(new TacticalBindingDiagnostic(/* MalformedTacticalBindingProfile */ 3, [detail]))]));
    const matchValue = UnifiedTacticalWorkspace_parseStrictJson(json);
    if (matchValue.tag === 0) {
        if (matchValue.fields[0].tag === 4) {
            const root = matchValue.fields[0].fields[0];
            let version;
            const matchValue_1 = tryFind("schemaVersion", root);
            let matchResult, value;
            if (matchValue_1 != null) {
                if (matchValue_1.tag === 2) {
                    matchResult = 0;
                    value = matchValue_1.fields[0];
                }
                else {
                    matchResult = 1;
                }
            }
            else {
                matchResult = 1;
            }
            switch (matchResult) {
                case 0: {
                    version = value;
                    break;
                }
                default:
                    version = undefined;
            }
            if (version != null) {
                if ((value_1 = (version | 0), (value_1 !== 0) && (value_1 !== 1))) {
                    const value_2 = version | 0;
                    return new FSharpResult$2(/* Error */ 1, [singleton_2(new TacticalBindingDiagnostic(/* UnsupportedTacticalBindingSchema */ 4, [value_2]))]);
                }
                else {
                    const value_3 = version | 0;
                    const bindingsField = (value_3 === 0) ? "overrides" : "bindings";
                    const allowed = ofList(ofArray(["schemaVersion", bindingsField]), {
                        Compare: (x, y) => (comparePrimitives(x, y) | 0),
                    });
                    if (exists_1((name, _arg) => !contains(name, allowed), root)) {
                        return invalid("Unknown root binding-profile field.");
                    }
                    else {
                        const matchValue_2 = tryFind(bindingsField, root);
                        let matchResult_1, entries;
                        if (matchValue_2 != null) {
                            if (matchValue_2.tag === 3) {
                                matchResult_1 = 0;
                                entries = matchValue_2.fields[0];
                            }
                            else {
                                matchResult_1 = 1;
                            }
                        }
                        else {
                            matchResult_1 = 1;
                        }
                        switch (matchResult_1) {
                            case 0: {
                                const parsed = map_1((_arg_1) => {
                                    let fields;
                                    let matchResult_2, fields_1;
                                    if (_arg_1.tag === 4) {
                                        if ((fields = _arg_1.fields[0], ((FSharpMap__get_Count(fields) === 2) && containsKey("id", fields)) && containsKey("gesture", fields))) {
                                            matchResult_2 = 0;
                                            fields_1 = _arg_1.fields[0];
                                        }
                                        else {
                                            matchResult_2 = 1;
                                        }
                                    }
                                    else {
                                        matchResult_2 = 1;
                                    }
                                    switch (matchResult_2) {
                                        case 0: {
                                            const matchValue_3 = FSharpMap__get_Item(fields_1, "id");
                                            const matchValue_4 = FSharpMap__get_Item(fields_1, "gesture");
                                            let matchResult_3, gesture, id, id_1;
                                            if (matchValue_3.tag === 1) {
                                                switch (matchValue_4.tag) {
                                                    case 1: {
                                                        matchResult_3 = 0;
                                                        gesture = matchValue_4.fields[0];
                                                        id = matchValue_3.fields[0];
                                                        break;
                                                    }
                                                    case 0: {
                                                        matchResult_3 = 1;
                                                        id_1 = matchValue_3.fields[0];
                                                        break;
                                                    }
                                                    default:
                                                        matchResult_3 = 2;
                                                }
                                            }
                                            else {
                                                matchResult_3 = 2;
                                            }
                                            switch (matchResult_3) {
                                                case 0:
                                                    return new FSharpResult$2(/* Ok */ 0, [[id, gesture]]);
                                                case 1:
                                                    return new FSharpResult$2(/* Ok */ 0, [[id_1, undefined]]);
                                                default:
                                                    return new FSharpResult$2(/* Error */ 1, ["Binding id must be a string and gesture must be a string or null."]);
                                            }
                                        }
                                        default:
                                            return new FSharpResult$2(/* Error */ 1, ["Each binding must contain exactly id and gesture."]);
                                    }
                                }, entries);
                                const matchValue_6 = tryPick((_arg_2) => {
                                    if (_arg_2.tag === 1) {
                                        return _arg_2.fields[0];
                                    }
                                    else {
                                        return undefined;
                                    }
                                }, parsed);
                                if (matchValue_6 == null) {
                                    const values = choose((_arg_3) => {
                                        if (_arg_3.tag === 0) {
                                            return _arg_3.fields[0];
                                        }
                                        else {
                                            return undefined;
                                        }
                                    }, parsed);
                                    const ids = map_1((tuple) => tuple[0], values);
                                    if (count_1(ofList(ids, {
                                        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
                                    })) !== length(ids)) {
                                        return invalid("Duplicate binding command ID.");
                                    }
                                    else {
                                        const profile = new TacticalBindingProfile(1, ofList_1(values, {
                                            Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
                                        }));
                                        const diagnostics = UnifiedTacticalWorkspace_validateBindings(registry, profile);
                                        if (isEmpty_1(diagnostics)) {
                                            return new FSharpResult$2(/* Ok */ 0, [profile]);
                                        }
                                        else {
                                            return new FSharpResult$2(/* Error */ 1, [diagnostics]);
                                        }
                                    }
                                }
                                else {
                                    return invalid(matchValue_6);
                                }
                            }
                            default:
                                return invalid(bindingsField + " must be an array.");
                        }
                    }
                }
            }
            else {
                return invalid("schemaVersion must be an integer.");
            }
        }
        else {
            return invalid("Binding profile root must be an object.");
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [singleton_2(matchValue.fields[0])]);
    }
}

function UnifiedTacticalWorkspace_clamp(minimum, maximum, value) {
    return max((x_1, y_1) => (compare(x_1, y_1) | 0), minimum, min((x, y) => (compare(x, y) | 0), maximum, value));
}

export function UnifiedTacticalWorkspace_initial(horizon) {
    return new TacticalTimelineState(TacticalModality.Editor, 0n, max_1(0n, horizon), -1n, false, undefined, empty_2());
}

export function UnifiedTacticalWorkspace_switchModality(modality, state) {
    return new TacticalTimelineState(modality, state.Cursor, state.Horizon, state.CommittedThrough, state.IsPlaying, state.SelectedSegment, state.Segments);
}

/**
 * Scrubbing changes projection only. Segments and the committed boundary
 * are deliberately copied unchanged.
 */
export function UnifiedTacticalWorkspace_scrub(tick, state) {
    return new TacticalTimelineState(state.Modality, UnifiedTacticalWorkspace_clamp(0n, state.Horizon, tick), state.Horizon, state.CommittedThrough, false, state.SelectedSegment, state.Segments);
}

export function UnifiedTacticalWorkspace_step(delta, state) {
    return UnifiedTacticalWorkspace_scrub(toInt64_unchecked(op_Addition(state.Cursor, delta)), state);
}

export function UnifiedTacticalWorkspace_home(state) {
    return UnifiedTacticalWorkspace_scrub(0n, state);
}

export function UnifiedTacticalWorkspace_finish(state) {
    return UnifiedTacticalWorkspace_scrub(state.Horizon, state);
}

export function UnifiedTacticalWorkspace_setPlaying(playing, state) {
    return new TacticalTimelineState(state.Modality, state.Cursor, state.Horizon, state.CommittedThrough, playing && (compare_1(state.Cursor, state.Horizon) < 0), state.SelectedSegment, state.Segments);
}

export function UnifiedTacticalWorkspace_pulse(state) {
    if (!state.IsPlaying) {
        return state;
    }
    else if (compare_1(state.Cursor, state.Horizon) >= 0) {
        return new TacticalTimelineState(state.Modality, state.Cursor, state.Horizon, state.CommittedThrough, false, state.SelectedSegment, state.Segments);
    }
    else {
        const next = min_1(state.Horizon, toInt64_unchecked(op_Addition(state.Cursor, 1n)));
        return new TacticalTimelineState(state.Modality, next, state.Horizon, state.CommittedThrough, compare_1(next, state.Horizon) < 0, state.SelectedSegment, state.Segments);
    }
}

export function UnifiedTacticalWorkspace_nextEditableBoundary(state) {
    return max_1(0n, toInt64_unchecked(op_Addition(state.CommittedThrough, 1n)));
}

export function UnifiedTacticalWorkspace_canEditAt(tick, state) {
    if (compare_1(tick, UnifiedTacticalWorkspace_nextEditableBoundary(state)) >= 0) {
        return compare_1(tick, state.Horizon) <= 0;
    }
    else {
        return false;
    }
}

function UnifiedTacticalWorkspace_validSegment(state, segment) {
    if ((compare_1(segment.StartTick, 0n) >= 0) && (compare_1(segment.EndTick, segment.StartTick) >= 0)) {
        return compare_1(segment.EndTick, state.Horizon) <= 0;
    }
    else {
        return false;
    }
}

export function UnifiedTacticalWorkspace_addSegment(segment, state) {
    if (!UnifiedTacticalWorkspace_validSegment(state, segment)) {
        return new FSharpResult$2(/* Error */ 1, [TimelineEditError.InvalidTimelineRange]);
    }
    else if (compare_1(segment.StartTick, state.CommittedThrough) <= 0) {
        return new FSharpResult$2(/* Error */ 1, [TimelineEditError.CommittedInterval]);
    }
    else if (exists((current) => (current.Id === segment.Id), state.Segments)) {
        return new FSharpResult$2(/* Error */ 1, [new TimelineEditError(/* DuplicateTimelineSegment */ 2, [segment.Id])]);
    }
    else {
        return new FSharpResult$2(/* Ok */ 0, [new TacticalTimelineState(state.Modality, state.Cursor, state.Horizon, state.CommittedThrough, state.IsPlaying, segment.Id, sortBy((current_1) => [current_1.StartTick, current_1.EndTick, current_1.UnitId, current_1.Id], cons(segment, state.Segments), {
            Compare: (x, y) => (compareArrays(x, y) | 0),
        }))]);
    }
}

export function UnifiedTacticalWorkspace_moveSegment(id, startTick, state) {
    const matchValue = tryFind_1((segment) => (segment.Id === id), state.Segments);
    if (matchValue != null) {
        const current = matchValue;
        const moved = new TacticalTimelineSegment(current.Id, current.UnitId, startTick, toInt64_unchecked(op_Addition(startTick, toInt64_unchecked(op_Subtraction(current.EndTick, current.StartTick)))), current.Channel, current.Label, current.Issue);
        if (!UnifiedTacticalWorkspace_validSegment(state, moved)) {
            return new FSharpResult$2(/* Error */ 1, [TimelineEditError.InvalidTimelineRange]);
        }
        else if (compare_1(moved.StartTick, state.CommittedThrough) <= 0) {
            return new FSharpResult$2(/* Error */ 1, [TimelineEditError.CommittedInterval]);
        }
        else {
            return new FSharpResult$2(/* Ok */ 0, [new TacticalTimelineState(state.Modality, state.Cursor, state.Horizon, state.CommittedThrough, state.IsPlaying, state.SelectedSegment, sortBy((segment_2) => [segment_2.StartTick, segment_2.EndTick, segment_2.UnitId, segment_2.Id], map_1((segment_1) => {
                if (segment_1.Id === id) {
                    return moved;
                }
                else {
                    return segment_1;
                }
            }, state.Segments), {
                Compare: (x, y) => (compareArrays(x, y) | 0),
            }))]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [new TimelineEditError(/* TimelineSegmentNotFound */ 3, [id])]);
    }
}

export function UnifiedTacticalWorkspace_removeSegment(id, state) {
    let Segments;
    const matchValue = tryFind_1((segment) => (segment.Id === id), state.Segments);
    if (matchValue != null) {
        if (compare_1(matchValue.StartTick, state.CommittedThrough) <= 0) {
            const segment_2 = matchValue;
            return new FSharpResult$2(/* Error */ 1, [TimelineEditError.CommittedInterval]);
        }
        else {
            return new FSharpResult$2(/* Ok */ 0, [(Segments = filter((segment_3) => (segment_3.Id !== id), state.Segments), new TacticalTimelineState(state.Modality, state.Cursor, state.Horizon, state.CommittedThrough, state.IsPlaying, equals(state.SelectedSegment, id) ? undefined : state.SelectedSegment, Segments))]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [new TimelineEditError(/* TimelineSegmentNotFound */ 3, [id])]);
    }
}

export function UnifiedTacticalWorkspace_acceptThrough(tick, state) {
    const boundary = UnifiedTacticalWorkspace_clamp(state.CommittedThrough, state.Horizon, tick);
    return new TacticalTimelineState(state.Modality, state.Cursor, state.Horizon, state.CommittedThrough, state.IsPlaying, state.SelectedSegment, append_1(state.Segments, choose((segment) => {
        const acceptedId = "accepted:" + segment.Id;
        if ((equals(segment.Channel, TacticalTimeChannel.Authored) && (compare_1(segment.EndTick, boundary) <= 0)) && !exists((current) => (current.Id === acceptedId), state.Segments)) {
            return new TacticalTimelineSegment(acceptedId, segment.UnitId, segment.StartTick, segment.EndTick, TacticalTimeChannel.Accepted, "Accepted · " + segment.Label, segment.Issue);
        }
        else {
            return undefined;
        }
    }, state.Segments)));
}

export function UnifiedTacticalWorkspace_commitThrough(tick, state) {
    const boundary = UnifiedTacticalWorkspace_clamp(state.CommittedThrough, state.Horizon, tick);
    const committed = choose((segment_1) => {
        const committedId = "committed:" + segment_1.Id;
        if (exists((current) => (current.Id === committedId), state.Segments)) {
            return undefined;
        }
        else {
            return new TacticalTimelineSegment(committedId, segment_1.UnitId, segment_1.StartTick, segment_1.EndTick, TacticalTimeChannel.Committed, "Committed · " + segment_1.Label, segment_1.Issue);
        }
    }, filter((segment) => {
        if (equals(segment.Channel, TacticalTimeChannel.Authored)) {
            return compare_1(segment.EndTick, boundary) <= 0;
        }
        else {
            return false;
        }
    }, state.Segments));
    return new TacticalTimelineState(state.Modality, max_1(state.Cursor, boundary), state.Horizon, boundary, state.IsPlaying, state.SelectedSegment, append_1(state.Segments, committed));
}

export function UnifiedTacticalWorkspace_authoritativeProgressBoundary(response, currentTick) {
    switch (response.tag) {
        case 3:
        case 4:
        case 5:
        case 6:
        case 7:
        case 10:
            return toInt64_unchecked(fromInt32(currentTick));
        default:
            return undefined;
    }
}

export function UnifiedTacticalWorkspace_projectAt(tick, state) {
    const cursor = UnifiedTacticalWorkspace_clamp(0n, state.Horizon, tick);
    return filter((segment) => {
        if (compare_1(segment.StartTick, cursor) <= 0) {
            return compare_1(cursor, segment.EndTick) <= 0;
        }
        else {
            return false;
        }
    }, state.Segments);
}

export function UnifiedTacticalWorkspace_validate(state) {
    return toList(delay(() => append((compare_1(state.Horizon, 0n) < 0) ? singleton_1("Timeline horizon cannot be negative.") : empty_1(), delay(() => append(((compare_1(state.Cursor, 0n) < 0) ? true : (compare_1(state.Cursor, state.Horizon) > 0)) ? singleton_1("Timeline cursor is outside the bounded horizon.") : empty_1(), delay(() => append(((compare_1(state.CommittedThrough, state.Horizon) >= 0) && (compare_1(state.Horizon, 0n) >= 0)) ? singleton_1("Committed boundary leaves no editable interval.") : empty_1(), delay(() => collect((segment) => (!UnifiedTacticalWorkspace_validSegment(state, segment) ? singleton_1(("Invalid timeline segment " + segment.Id) + ".") : empty_1()), state.Segments)))))))));
}

