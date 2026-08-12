import { execFileSync } from "node:child_process";
import { Replay_decode, Replay_defaultLimits, Replay_encode, Replay_resolveRulesArchive } from "../src/SIR.Client.Web/.fable/SIR.Simulation/Replay.js";
import { toArray } from "../src/SIR.Client.Web/.fable/fable_modules/fable-library-js.5.13.0/List.js";

const hex = execFileSync("dotnet", [
  "run", "--project", "tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj",
  "-c", "Release", "--no-build", "--", "--print-replay-package",
], { encoding: "utf8" }).trim();
const bytes = Uint8Array.from(hex.match(/../g).map((value) => Number.parseInt(value, 16)));
const decoded = Replay_decode(Replay_defaultLimits, bytes);
if (decoded.tag !== 0) throw new Error(`production Fable rejected .NET replay v3: ${decoded.fields[0]}`);
const roundTrip = Replay_encode(decoded.fields[0]);
if (Buffer.compare(Buffer.from(bytes), Buffer.from(roundTrip)) !== 0) {
  throw new Error("production Fable replay v3 decode/re-encode changed .NET canonical bytes");
}
if (decoded.fields[0].FormatVersion !== 3 || decoded.fields[0].RulesArchive == null) {
  throw new Error("production Fable replay v3 omitted its typed rules archive");
}
const resolved = Replay_resolveRulesArchive(decoded.fields[0].RulesArchive);
if (resolved.tag !== 0) throw new Error(`production Fable could not resolve replay-owned historical rules: ${resolved.fields[0]}`);
const damage = toArray(resolved.fields[0]).find((rule) => rule.Metadata.Title === "Expected damage");
if (damage == null || damage.Semantics.tag !== 2 || damage.Metadata.Rationale.length === 0) {
  throw new Error("production Fable lost the historical damage formula or rationale");
}
const source = damage.Metadata.RuleSource;
if (source == null || source.RepositoryPath !== "src/SIR.Simulation/CombatRules.fs" || source.Commit !== decoded.fields[0].RulesArchive.Identity.SourceCommit) {
  throw new Error("production Fable lost the historical damage source path or pinned commit");
}
console.log(`production Fable replay v3 accepted ${bytes.length} .NET bytes with typed archive`);
