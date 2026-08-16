import { readFileSync } from "node:fs";

const log = process.argv[2];
if (!log) throw new Error("verify-fable-invocations: log path is required");

const observed = readFileSync(log, "utf8")
  .split(/\r?\n/u)
  .filter(Boolean)
  .map((line) => {
    const [kind, project, extra] = line.split("\t");
    if (kind !== "fable" || !project || extra !== undefined) throw new Error(`verify-fable-invocations: unreadable entry: ${line}`);
    return project;
  });

const expected = [
  "tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj",
  "tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj",
  "src/SIR.Replay.Web/SIR.Replay.Web.fsproj",
  "src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj",
];

const canonicalObserved = [...observed].sort();
const canonicalExpected = [...expected].sort();
if (JSON.stringify(canonicalObserved) !== JSON.stringify(canonicalExpected)) {
  throw new Error(`verify-fable-invocations: expected exact-once set ${canonicalExpected.join(",")}; observed ${canonicalObserved.join(",")}`);
}

console.log(JSON.stringify({ schema: "sir.fable-process-inventory/v1", result: "pass", observed: canonicalObserved }));
