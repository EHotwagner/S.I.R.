
import { Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { list_type, class_type, option_type, record_type, string_type, int32_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { arrayHash, equalArrays, compareArrays, equals, comparePrimitives, int32ToString } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { cons, zip, singleton, empty, collect, sortBy, choose, sort, tryHead, map, ofArray } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { keys, tryFind, ofList } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { max, min } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { ofSeq, union, toList } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { substring, isNullOrWhiteSpace } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { List_distinctBy } from "../fable_modules/fable-library-js.5.13.0/Seq2.js";

export class ComparisonView extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Split", "Swipe", "DifferenceOverlay"];
    }
    static Split = new ComparisonView(0, []);
    static Swipe = new ComparisonView(1, []);
    static DifferenceOverlay = new ComparisonView(2, []);
}

export function ComparisonView_$reflection() {
    return union_type("SIR.Client.ComparisonView", [], ComparisonView, () => [[], [], []]);
}

export class ComparisonBookmark extends Record {
    constructor(Tick, Label) {
        super();
        this.Tick = (Tick | 0);
        this.Label = Label;
    }
}

export function ComparisonBookmark_$reflection() {
    return record_type("SIR.Client.ComparisonBookmark", [], ComparisonBookmark, () => [["Tick", int32_type], ["Label", string_type]]);
}

export class DivergentField extends Record {
    constructor(Tick, UnitId, Field, Baseline, Fork) {
        super();
        this.Tick = (Tick | 0);
        this.UnitId = UnitId;
        this.Field = Field;
        this.Baseline = Baseline;
        this.Fork = Fork;
    }
}

export function DivergentField_$reflection() {
    return record_type("SIR.Client.DivergentField", [], DivergentField, () => [["Tick", int32_type], ["UnitId", option_type(int32_type)], ["Field", string_type], ["Baseline", string_type], ["Fork", string_type]]);
}

export class ComparisonInspection extends Record {
    constructor(FirstDivergentEvent, FirstDifferingField, MetricDeltas) {
        super();
        this.FirstDivergentEvent = FirstDivergentEvent;
        this.FirstDifferingField = FirstDifferingField;
        this.MetricDeltas = MetricDeltas;
    }
}

export function ComparisonInspection_$reflection() {
    return record_type("SIR.Client.ComparisonInspection", [], ComparisonInspection, () => [["FirstDivergentEvent", option_type(int32_type)], ["FirstDifferingField", option_type(DivergentField_$reflection())], ["MetricDeltas", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, int32_type])]]);
}

export class LinkedComparison extends Record {
    constructor(SourceIdentity, BaselineIdentity, ForkIdentity, BaselineLabel, ForkLabel, Tick, SelectedUnit, View, Bookmarks, Inspection) {
        super();
        this.SourceIdentity = SourceIdentity;
        this.BaselineIdentity = BaselineIdentity;
        this.ForkIdentity = ForkIdentity;
        this.BaselineLabel = BaselineLabel;
        this.ForkLabel = ForkLabel;
        this.Tick = (Tick | 0);
        this.SelectedUnit = SelectedUnit;
        this.View = View;
        this.Bookmarks = Bookmarks;
        this.Inspection = Inspection;
    }
}

export function LinkedComparison_$reflection() {
    return record_type("SIR.Client.LinkedComparison", [], LinkedComparison, () => [["SourceIdentity", string_type], ["BaselineIdentity", string_type], ["ForkIdentity", string_type], ["BaselineLabel", string_type], ["ForkLabel", string_type], ["Tick", int32_type], ["SelectedUnit", option_type(int32_type)], ["View", ComparisonView_$reflection()], ["Bookmarks", list_type(ComparisonBookmark_$reflection())], ["Inspection", ComparisonInspection_$reflection()]]);
}

function Comparison_unitFields(unit) {
    return ofArray([["side", unit.Side], ["column", int32ToString(unit.Column)], ["row", int32ToString(unit.Row)], ["health", int32ToString(unit.Health)], ["health-maximum", int32ToString(unit.HealthMaximum)]]);
}

export function Comparison_inspect(baseline, fork, metricDeltas) {
    const baselineEvents = ofList(map((event) => [event.Id, event], baseline.Events), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    const forkEvents = ofList(map((event_1) => [event_1.Id, event_1], fork.Events), {
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    });
    let firstDivergentEvent;
    const option_1 = tryHead(sort(choose((id) => {
        const matchValue = tryFind(id, baselineEvents);
        const matchValue_1 = tryFind(id, forkEvents);
        if (matchValue == null) {
            if (matchValue_1 == null) {
                return undefined;
            }
            else {
                const right_3 = matchValue_1;
                return [right_3.Tick, id];
            }
        }
        else if (matchValue_1 == null) {
            const left_3 = matchValue;
            return [left_3.Tick, id];
        }
        else if (equals(matchValue, matchValue_1)) {
            const left_1 = matchValue;
            const right_1 = matchValue_1;
            return undefined;
        }
        else {
            const left_2 = matchValue;
            const right_2 = matchValue_1;
            return [min(left_2.Tick, right_2.Tick), id];
        }
    }, toList(union(ofSeq(keys(baselineEvents), {
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    }), ofSeq(keys(forkEvents), {
        Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
    })))), {
        Compare: (x_4, y_4) => (compareArrays(x_4, y_4) | 0),
    }));
    firstDivergentEvent = ((option_1 != null) ? option_1[1] : undefined);
    const baselineUnits = ofList(map((unit) => [unit.Id, unit], baseline.Units), {
        Compare: (x_5, y_5) => (comparePrimitives(x_5, y_5) | 0),
    });
    const forkUnits = ofList(map((unit_1) => [unit_1.Id, unit_1], fork.Units), {
        Compare: (x_6, y_6) => (comparePrimitives(x_6, y_6) | 0),
    });
    return new ComparisonInspection(firstDivergentEvent, tryHead(sortBy((difference) => [difference.Tick, difference.UnitId, difference.Field], collect((id_1) => {
        const matchValue_3 = tryFind(id_1, baselineUnits);
        const matchValue_4 = tryFind(id_1, forkUnits);
        if (matchValue_3 == null) {
            if (matchValue_4 == null) {
                return empty();
            }
            else {
                return singleton(new DivergentField(max(baseline.Tick, fork.Tick), id_1, "presence", "absent", "present"));
            }
        }
        else if (matchValue_4 == null) {
            return singleton(new DivergentField(max(baseline.Tick, fork.Tick), id_1, "presence", "present", "absent"));
        }
        else {
            const left_4 = matchValue_3;
            const right_4 = matchValue_4;
            return choose((tupledArg) => {
                const _arg = tupledArg[0];
                const baselineValue = _arg[1];
                const forkValue = tupledArg[1][1];
                if (baselineValue === forkValue) {
                    return undefined;
                }
                else {
                    return new DivergentField(max(baseline.Tick, fork.Tick), id_1, _arg[0], baselineValue, forkValue);
                }
            }, zip(Comparison_unitFields(left_4), Comparison_unitFields(right_4)));
        }
    }, toList(union(ofSeq(keys(baselineUnits), {
        Compare: (x_7, y_7) => (comparePrimitives(x_7, y_7) | 0),
    }), ofSeq(keys(forkUnits), {
        Compare: (x_8, y_8) => (comparePrimitives(x_8, y_8) | 0),
    })))), {
        Compare: (x_9, y_9) => (compareArrays(x_9, y_9) | 0),
    })), metricDeltas);
}

export function Comparison_create(sourceIdentity, baselineIdentity, forkIdentity, tick, selected, inspection) {
    return new LinkedComparison(sourceIdentity, baselineIdentity, forkIdentity, "Immutable baseline — exploratory simulation", "Derived fork — exploratory simulation, not verified replay", tick, selected, ComparisonView.Split, empty(), inspection);
}

export function Comparison_addBookmark(tick, label, comparison) {
    const safeLabel = isNullOrWhiteSpace(label) ? ("Bookmark at tick " + int32ToString(tick)) : substring(label.trim(), 0, min(80, label.trim().length));
    return new LinkedComparison(comparison.SourceIdentity, comparison.BaselineIdentity, comparison.ForkIdentity, comparison.BaselineLabel, comparison.ForkLabel, comparison.Tick, comparison.SelectedUnit, comparison.View, sortBy((item_1) => [item_1.Tick, item_1.Label], List_distinctBy((item) => [item.Tick, item.Label], cons(new ComparisonBookmark(max(0, tick), safeLabel), comparison.Bookmarks), {
        Equals: equalArrays,
        GetHashCode: (x) => (arrayHash(x) | 0),
    }), {
        Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
    }), comparison.Inspection);
}

export function Comparison_setLinkedTick(tick, comparison) {
    return new LinkedComparison(comparison.SourceIdentity, comparison.BaselineIdentity, comparison.ForkIdentity, comparison.BaselineLabel, comparison.ForkLabel, max(0, tick), comparison.SelectedUnit, comparison.View, comparison.Bookmarks, comparison.Inspection);
}

export function Comparison_setLinkedSelection(selected, comparison) {
    return new LinkedComparison(comparison.SourceIdentity, comparison.BaselineIdentity, comparison.ForkIdentity, comparison.BaselineLabel, comparison.ForkLabel, comparison.Tick, selected, comparison.View, comparison.Bookmarks, comparison.Inspection);
}

export function Comparison_setView(view, comparison) {
    return new LinkedComparison(comparison.SourceIdentity, comparison.BaselineIdentity, comparison.ForkIdentity, comparison.BaselineLabel, comparison.ForkLabel, comparison.Tick, comparison.SelectedUnit, view, comparison.Bookmarks, comparison.Inspection);
}

