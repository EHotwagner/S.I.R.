
import { empty, singleton, collect, append, delay, toList } from "./fable_modules/fable-library-js.5.13.0/Seq.js";
import { op_Addition, fromInt32, toInt64_unchecked } from "./fable_modules/fable-library-js.5.13.0/BigInt.js";
import { TacticalTimelineSegment, TacticalTimeChannel } from "./SIR.Client/UnifiedTacticalWorkspace.js";
import { toString } from "./fable_modules/fable-library-js.5.13.0/Types.js";
import { tryFind } from "./fable_modules/fable-library-js.5.13.0/Array.js";
import { int64ToString, equals } from "./fable_modules/fable-library-js.5.13.0/Util.js";

export function projectPlanningSegments(state) {
    return toList(delay(() => append(collect((command) => {
        let option_1;
        const authored = new TacticalTimelineSegment(command.Id, command.UnitId, toInt64_unchecked(fromInt32(command.EarliestTick)), toInt64_unchecked(op_Addition(toInt64_unchecked(fromInt32(command.EarliestTick)), 1n)), TacticalTimeChannel.Authored, toString(command.Kind), (option_1 = tryFind((issue) => equals(issue.CommandId, command.Id), state.Issues), (option_1 != null) ? option_1.Detail : undefined));
        return append(singleton(authored), delay(() => append(equals(state.AcceptedRevision, state.Revision) ? singleton(new TacticalTimelineSegment("accepted:" + command.Id, authored.UnitId, authored.StartTick, authored.EndTick, TacticalTimeChannel.Accepted, "Worker-accepted " + toString(command.Kind), undefined)) : empty(), delay(() => (equals(state.CommittedRevision, state.Revision) ? singleton(new TacticalTimelineSegment("committed:" + command.Id, authored.UnitId, authored.StartTick, authored.EndTick, TacticalTimeChannel.Committed, "Committed " + toString(command.Kind), undefined)) : empty())))));
    }, state.Commands), delay(() => {
        const matchValue = state.Predicted;
        if (matchValue == null) {
            return empty();
        }
        else {
            return singleton(new TacticalTimelineSegment("prediction-" + int64ToString(matchValue.Revision), undefined, 0n, 1n, TacticalTimeChannel.Predicted, "Intent-only predicted state", undefined));
        }
    }))));
}

