
import { MapRevision } from "./MapEditorTypes.js";

export function create(number, parent, document$, digest) {
    return new MapRevision(number, parent, document$, digest);
}

