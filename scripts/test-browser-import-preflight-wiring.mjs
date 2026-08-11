import { readFile } from "node:fs/promises";

const source = await readFile("src/SIR.Client.Web/App.fs", "utf8");
const branch = source.match(/\| ReplayReadCompleted result ->([\s\S]*?)\n    \| MapFileSelected/);

if (!branch || !branch[1].includes("Shell.update CancelRequested model.Shell")) {
  throw new Error("Replay pre-read rejection must cancel the active Shell operation before publishing rejection state.");
}

if (!branch[1].includes("let cancelled, effects = Shell.update CancelRequested model.Shell") || !branch[1].includes("effectsToCmd effects")) {
  throw new Error("Replay pre-read rejection must dispatch the Shell cancellation effects, not discard the worker cancel request.");
}

if (!branch[1].includes("ActiveOperation = None")) {
  throw new Error("Replay pre-read rejection must leave no active operation for stale worker responses.");
}

console.log("Browser import preflight wiring passed.");
