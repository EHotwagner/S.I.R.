import { readFileSync } from "node:fs";

const [log, ...args] = process.argv.slice(2);
if (!log) throw new Error("verify-fable-invocations: log path is required");
const defaults = [
  "tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj",
  "tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj",
  "src/SIR.Replay.Web/SIR.Replay.Web.fsproj",
  "src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj",
];
const expected = new Map();
for (let index = 0; index < args.length; index += 2) {
  if (args[index] !== "--expect" || args[index + 1] === undefined) throw new Error(`verify-fable-invocations: malformed option:${args[index] ?? "missing"}`);
  const declaration = args[index + 1];
  const separator = declaration.lastIndexOf("=");
  const project = declaration.slice(0, separator);
  const count = Number(declaration.slice(separator + 1));
  if (separator <= 0 || !Number.isSafeInteger(count) || count < 1 || expected.has(project)) throw new Error(`verify-fable-invocations: malformed expectation:${declaration}`);
  expected.set(project, count);
}
if (expected.size === 0) for (const project of defaults) expected.set(project, 1);

const observed = new Map();
for (const line of readFileSync(log, "utf8").split(/\r?\n/u).filter(Boolean)) {
  const [kind, project, identity, started, completed, extra] = line.split("\t");
  if (kind !== "fable" || !project || !identity || !/^(-|exception:[a-z0-9-]+)$/u.test(identity)
    || !/^\d+$/u.test(started) || !/^\d+$/u.test(completed) || Number(completed) < Number(started) || extra !== undefined) {
    throw new Error(`verify-fable-invocations: unreadable entry: ${line}`);
  }
  observed.set(project, (observed.get(project) ?? 0) + 1);
}
const canonical = (values) => [...values.entries()].sort(([left], [right]) => left.localeCompare(right)).map(([project, count]) => ({ project, count }));
if (JSON.stringify(canonical(observed)) !== JSON.stringify(canonical(expected))) {
  throw new Error(`verify-fable-invocations: expected ${JSON.stringify(canonical(expected))}; observed ${JSON.stringify(canonical(observed))}`);
}
const inventory = canonical(observed);
console.log(JSON.stringify({ schema: "sir.fable-process-inventory/v1", result: "pass", observed: inventory, total: inventory.reduce((sum, entry) => sum + entry.count, 0) }));
