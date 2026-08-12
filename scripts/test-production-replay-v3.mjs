import { execFileSync } from "node:child_process";
import { Replay_decode, Replay_defaultLimits, Replay_encode } from "../src/SIR.Client.Web/.fable/SIR.Simulation/Replay.js";

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
console.log(`production Fable replay v3 accepted ${bytes.length} .NET bytes with typed archive`);
