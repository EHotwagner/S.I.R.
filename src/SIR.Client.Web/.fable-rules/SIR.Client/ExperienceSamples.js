
import { toString, Record } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { int32_type, record_type, list_type, string_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { empty as empty_1, singleton as singleton_1, append, filter, mapIndexed, map as map_2, tryFind, ofArray } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { initial as initial_1, update } from "./MapEditor.js";
import { MapEdgeDirection, MapEditorState, MapEditorAction } from "./MapEditorTypes.js";
import { map as map_1, tryHead } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { toList, toSeq } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { defaultArg } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { contains, ofList, empty, singleton } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { equals, int32ToString, comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { Result_ToOption } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { SimulatorAction, MapEditorSimulator_update, MapEditorSimulator_tryHandoff } from "./MapEditorSimulator.js";
import { InspectionProjection, CheckpointProjection, EdgeProjection, UnitProjection, EventProjection } from "./Shell.js";
import { Direction8, Direction8Module_toCode } from "../SIR.Domain/Orientation.js";

export class ExperienceMapSample extends Record {
    constructor(Id, Title, Summary, Highlights, MapText) {
        super();
        this.Id = Id;
        this.Title = Title;
        this.Summary = Summary;
        this.Highlights = Highlights;
        this.MapText = MapText;
    }
}

export function ExperienceMapSample_$reflection() {
    return record_type("SIR.Client.ExperienceMapSample", [], ExperienceMapSample, () => [["Id", string_type], ["Title", string_type], ["Summary", string_type], ["Highlights", list_type(string_type)], ["MapText", string_type]]);
}

export class ExperienceReplaySample extends Record {
    constructor(Id, Title, Summary, MapSampleId, Ticks) {
        super();
        this.Id = Id;
        this.Title = Title;
        this.Summary = Summary;
        this.MapSampleId = MapSampleId;
        this.Ticks = (Ticks | 0);
    }
}

export function ExperienceReplaySample_$reflection() {
    return record_type("SIR.Client.ExperienceReplaySample", [], ExperienceReplaySample, () => [["Id", string_type], ["Title", string_type], ["Summary", string_type], ["MapSampleId", string_type], ["Ticks", int32_type]]);
}

export const ExperienceSamples_maps = ofArray([new ExperienceMapSample("troll-assault", "Troll assault", "Three riflemen meet a 240 HP armored troll advancing across open ground.", ofArray(["Large 3×3 footprint versus a dispersed firing line", "General-controller target choice, movement, collision, and attrition", "Useful for exposing the current close-combat controller\'s limits"]), "SIR-MAP 2\nsize 16 10\nterrain 7 2 rough\nterrain 7 3 rough\nterrain 7 4 rough\nterrain 7 5 rough\nterrain 7 6 rough\nterrain 7 7 rough\nzone 1 deployment blue rectangle 0 0 4 10\nzone 2 deployment red rectangle 11 0 5 10\nunit 1 blue rifleman 1 0 2 12 12 general -\nunit 2 blue rifleman 1 4 2 12 12 general -\nunit 3 blue rifleman 1 8 2 12 12 general -\nunit 4 red troll 12 3 3 240 240 general -\n"), new ExperienceMapSample("breach-corridor", "Breach corridor", "A human section and goblin defenders converge on a single semantic door.", ofArray(["Walls, a closed door, and constrained movement", "Rough terrain around the breach", "Controller collision feedback at a bottleneck"]), "SIR-MAP 2\nsize 14 10\nterrain 5 3 rough\nterrain 5 4 rough\nterrain 5 5 rough\nterrain 5 6 rough\nedge 6 0 east wall closed\nedge 6 1 east wall closed\nedge 6 2 east wall closed\nedge 6 3 east wall closed\nedge 6 4 east door closed\nedge 6 5 east wall closed\nedge 6 6 east wall closed\nedge 6 7 east wall closed\nedge 6 8 east wall closed\nedge 6 9 east wall closed\nunit 1 blue rifleman 1 2 2 12 12 general -\nunit 2 blue medic 1 6 2 12 12 general -\nunit 3 red goblin 10 2 1 12 12 general -\nunit 4 red goblin 10 6 1 12 12 general -\n"), new ExperienceMapSample("objective-crossing", "Objective crossing", "Opposing patrols contest a central objective through rough and blocked ground.", ofArray(["Objective and deployment-zone semantics", "Terrain routing around blocked cells", "Mixed unit footprints in a compact encounter"]), "SIR-MAP 2\nsize 12 12\nterrain 4 4 rough\nterrain 5 4 rough\nterrain 6 4 rough\nterrain 7 4 rough\nterrain 5 5 objective\nterrain 6 5 objective\nterrain 5 6 objective\nterrain 6 6 objective\nterrain 4 7 rough\nterrain 5 7 rough\nterrain 6 7 blocked\nterrain 7 7 rough\nzone 1 objective rectangle 5 5 2 2\nzone 2 deployment blue rectangle 0 0 4 4\nzone 3 deployment red rectangle 8 8 4 4\nunit 1 blue rifleman 1 1 2 12 12 general -\nunit 2 blue observation-drone 3 1 1 8 8 general -\nunit 3 red goblin 9 9 1 12 12 general -\nunit 4 red orc 7 8 2 35 35 general -\n")]);

export const ExperienceSamples_replays = ofArray([new ExperienceReplaySample("troll-contact", "Troll reaches the line", "Follow the troll assault from deployment through first contact and early attrition.", "troll-assault", 20), new ExperienceReplaySample("breach-stalemate", "Closed-door stalemate", "Inspect controller events as both sides discover that the closed breach blocks advance.", "breach-corridor", 8)]);

export function ExperienceSamples_tryMap(id) {
    return tryFind((sample) => (sample.Id === id), ExperienceSamples_maps);
}

export function ExperienceSamples_tryReplay(id) {
    return tryFind((sample) => (sample.Id === id), ExperienceSamples_replays);
}

export function ExperienceSamples_editorState(sample) {
    let option_1;
    const state_2 = update(new MapEditorAction(/* SetMapName */ 70, [sample.Title]), update(new MapEditorAction(/* LoadMapText */ 106, [sample.MapText]), initial_1));
    const selected = tryHead(map_1((tuple) => (tuple[0] | 0), toSeq(state_2.Map.Units)));
    return new MapEditorState(state_2.Map, state_2.Tool, state_2.TerrainSelection, state_2.BrushSize, state_2.TerrainCursor, state_2.KeyboardCursor, state_2.KeyboardObject, state_2.LastTerrainPaintTool, state_2.TerrainAnnouncement, state_2.EdgeCursor, state_2.EdgeAnnouncement, state_2.UnitPaletteSearch, state_2.UnitPaletteCursor, state_2.UnitPlacementCursor, state_2.UnitAnnouncement, state_2.RegionAnnouncement, state_2.RegionKeyboardMode, selected, defaultArg((option_1 = selected, (option_1 != null) ? singleton(option_1, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }) : undefined), empty({
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    })), state_2.SelectedRegion, state_2.Gesture, state_2.Revision, state_2.RevisionState, state_2.SavedDigest, state_2.SimulatedDigest, state_2.RecoveredFromDigest, state_2.UndoHistory, state_2.RedoHistory, state_2.HistoryBytes, state_2.Clipboard, state_2.Tick, state_2.IsRunning, state_2.LastEvents, state_2.Validation, state_2.Layers, state_2.Issues, state_2.ActiveIssue, state_2.PendingDestructiveChange, state_2.PendingRecovery, state_2.Authoring);
}

export function ExperienceSamples_simulator(sample) {
    return Result_ToOption(MapEditorSimulator_tryHandoff(ExperienceSamples_editorState(sample)));
}

function ExperienceSamples_combatSource(delivery) {
    switch (delivery.tag) {
        case 1:
            return "combat-projectile";
        case 2:
            return "combat-lobbed-area";
        case 3:
            return "combat-spell-area";
        default:
            return "combat-melee";
    }
}

function ExperienceSamples_inspection(tick, map, events, combatEvents) {
    const combatSummaries = ofList(map_2((_arg) => _arg.Summary, combatEvents), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    const narrativeEvents = mapIndexed((index, summary_1) => (new EventProjection((tick * 100) + index, tick, "sample-simulation", summary_1, undefined, undefined)), filter((summary) => !contains(summary, combatSummaries), events));
    const projectedCombatEvents = mapIndexed((index_1, combat_1) => {
        let matchValue;
        return new EventProjection(((tick * 100) + 50) + index_1, tick, ExperienceSamples_combatSource(combat_1.Delivery), combat_1.Summary, combat_1.SourceUnitId, (matchValue = combat_1.Target, (matchValue.tag === 1) ? undefined : matchValue.fields[0]));
    }, filter((combat) => (combat.Tick === tick), combatEvents));
    return new InspectionProjection(tick, 0, 0, map.Width - 1, map.Height - 1, map_2((tupledArg) => {
        let matchValue_1;
        const unit = tupledArg[1];
        return new UnitProjection(unit.Id, (matchValue_1 = unit.Side, (matchValue_1.tag === 1) ? "Red" : ((matchValue_1.tag === 2) ? "Neutral" : "Blue")), unit.Column, unit.Row, unit.Health, unit.HealthMaximum, undefined, ~~Direction8Module_toCode(Direction8.North), ~~Direction8Module_toCode(Direction8.North));
    }, toList(map.Units)), mapIndexed((index_2, tupledArg_1) => {
        const _arg_2 = tupledArg_1[0];
        const _arg_3 = tupledArg_1[1];
        const row = _arg_2[1] | 0;
        const direction = _arg_2[2];
        const column = _arg_2[0] | 0;
        return new EdgeProjection("sample-edge-" + int32ToString(index_2), toString(_arg_3[0]), _arg_3[1] ? "open" : "closed", column, row, column + (equals(direction, MapEdgeDirection.EastEdge) ? 0 : 1), row + (equals(direction, MapEdgeDirection.SouthEdge) ? 0 : 1));
    }, toList(map.Edges)), append(narrativeEvents, projectedCombatEvents), singleton_1(new CheckpointProjection(tick, "sample-" + int32ToString(tick), "sample-events-" + int32ToString(tick))), undefined);
}

export function ExperienceSamples_replayFrames(replay) {
    let matchValue;
    const option_1 = ExperienceSamples_tryMap(replay.MapSampleId);
    matchValue = ((option_1 != null) ? ExperienceSamples_simulator(option_1) : undefined);
    if (matchValue != null) {
        const frames = [];
        let handoff = matchValue;
        void (frames.push(ExperienceSamples_inspection(0, handoff.RuntimeMap, empty_1(), empty_1())));
        for (let forLoopVar = 1; forLoopVar <= replay.Ticks; forLoopVar++) {
            handoff = MapEditorSimulator_update(SimulatorAction.StepSimulator, tryHead(map_1((tuple) => (tuple[0] | 0), toSeq(handoff.RuntimeMap.Units))), handoff);
            void (frames.push(ExperienceSamples_inspection(handoff.Tick, handoff.RuntimeMap, handoff.LastEvents, handoff.LastCombatEvents)));
        }
        return frames.slice();
    }
    else {
        return [];
    }
}

