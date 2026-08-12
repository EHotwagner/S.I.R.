
import { int32LittleEndian, concatenate } from "../SIR.Domain/CanonicalEncoding.js";
import { Cell } from "../fable_modules/FS.GG.Game.Core.0.13.0/Primitives.fs.js";

export function cellBytes(cell) {
    return concatenate([int32LittleEndian(cell.Col), int32LittleEndian(cell.Row)]);
}

export const origin = new Cell(0, 0);

