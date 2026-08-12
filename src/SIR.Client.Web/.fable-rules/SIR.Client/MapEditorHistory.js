
import { sumBy, empty, reverse, tail as tail_2, cons, head, isEmpty } from "../fable_modules/fable-library-js.5.13.0/List.js";

export function withinBounds(maximumCommands, maximumBytes, entries) {
    const keep = (count_mut, bytes_mut, accepted_mut, remaining_mut) => {
        keep:
        while (true) {
            const count = count_mut, bytes = bytes_mut, accepted = accepted_mut, remaining = remaining_mut;
            if (!isEmpty(remaining)) {
                if ((count < maximumCommands) && ((bytes + head(remaining).SerializedBytes) <= maximumBytes)) {
                    count_mut = (count + 1);
                    bytes_mut = (bytes + head(remaining).SerializedBytes);
                    accepted_mut = cons(head(remaining), accepted);
                    remaining_mut = tail_2(remaining);
                    continue keep;
                }
                else {
                    return reverse(accepted);
                }
            }
            else {
                return reverse(accepted);
            }
            break;
        }
    };
    return keep(0, 0, empty(), entries);
}

export function size(entries) {
    return sumBy((_arg) => (_arg.SerializedBytes | 0), entries, {
        GetZero: () => 0,
        Add: (x, y) => ((x + y) | 0),
    }) | 0;
}

