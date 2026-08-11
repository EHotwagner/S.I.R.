import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const root = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const read = (path) => readFileSync(resolve(root, path), "utf8");
const fail = (message) => { throw new Error(`architecture graph: ${message}`); };

const projects = {
  client: "src/SIR.Client/SIR.Client.fsproj",
  generated: "src/SIR.Protocol.Generated/SIR.Protocol.Generated.fsproj",
  protocol: "src/SIR.Protocol/SIR.Protocol.fsproj",
  replayCore: "src/SIR.Replay.Core/SIR.Replay.Core.fsproj",
  replay: "src/SIR.Replay.Web/SIR.Replay.Web.fsproj",
  wasm: "src/SIR.Wasm/SIR.Wasm.fsproj",
  match: "src/SIR.Match/SIR.Match.fsproj",
  tools: "src/SIR.Tools/SIR.Tools.fsproj",
};

const references = (xml) => [...xml.matchAll(/<ProjectReference Include="([^"]+)"/g)].map((m) => m[1]);
const has = (xml, text) => xml.includes(text);

const validate = ({ solution, sources, docs }) => {
  for (const path of Object.values(projects)) {
    if (!solution.includes(`Project Path="${path}"`)) fail(`solution omits ${path}`);
  }
  if (solution.includes('Project Path="src/SIR.Client.Web/SIR.Client.Web.fsproj"')) fail("solution retains transitional SIR.Client.Web project");

  const client = sources.client;
  if (references(client).some((reference) => reference.includes("SIR.Simulation"))) fail("SIR.Client references SIR.Simulation");
  if (!references(client).some((reference) => reference.includes("SIR.Protocol"))) fail("SIR.Client lacks its protocol boundary");

  if (!references(sources.protocol).some((reference) => reference.includes("SIR.Protocol.Generated"))) fail("SIR.Protocol lacks SIR.Protocol.Generated");
  if (!references(sources.match).some((reference) => reference.includes("SIR.Wasm"))) fail("SIR.Match lacks SIR.Wasm");
  if (has(sources.match, 'PackageReference Include="Wasmtime"')) fail("SIR.Match owns the concrete Wasmtime package");
  if (!has(sources.wasm, 'PackageReference Include="Wasmtime"')) fail("SIR.Wasm lacks the concrete Wasmtime package");

  const replayReferences = references(sources.replay).join(" ");
  const replayCoreReferences = references(sources.replayCore).join(" ");
  if (!replayReferences.includes("SIR.Replay.Core")) fail("SIR.Replay.Web lacks its replay/editor core");
  if (!replayCoreReferences.includes("SIR.Simulation")) fail("SIR.Replay.Core lacks the shared simulation dependency");
  for (const forbidden of ["SIR.Wasm", "SIR.Match", "SIR.Server"]) {
    if (replayReferences.includes(forbidden) || replayCoreReferences.includes(forbidden)) fail(`SIR.Replay boundary references forbidden ${forbidden}`);
  }

  for (const token of ["SIR.Wasm", "SIR.Protocol.Generated", "SIR.Replay.Web", "SIR.Tools", "SIR.Client` does not reference `SIR.Simulation"])
    if (!docs.architecture.includes(token)) fail(`canonical architecture omits ${token}`);
  for (const token of ["SIR.Protocol.Generated", "HTTP/Thoth", "SignalR"])
    if (!docs.protocol.includes(token)) fail(`canonical protocol omits current boundary ${token}`);
};

const sources = Object.fromEntries(Object.entries(projects).map(([key, path]) => [key, read(path)]));
const input = { solution: read("SIR.slnx"), sources, docs: { architecture: read("docs/codebase-architecture.md"), protocol: read("docs/public-protocol-architecture.md") } };
validate(input);

if (process.argv.includes("--self-test")) {
  const badClient = structuredClone(input);
  badClient.sources.client += '<ProjectReference Include="../SIR.Simulation/SIR.Simulation.fsproj" />';
  try { validate(badClient); fail("forbidden-edge mutation survived"); } catch (error) { if (!String(error.message).includes("SIR.Client references SIR.Simulation")) throw error; }
  const staleDocs = structuredClone(input);
  staleDocs.docs.architecture = staleDocs.docs.architecture.replaceAll("SIR.Replay.Web", "SIR.Replay.Removed");
  try { validate(staleDocs); fail("canonical-document mutation survived"); } catch (error) { if (!String(error.message).includes("canonical architecture omits SIR.Replay.Web")) throw error; }
  console.log("architecture graph self-test: forbidden edge and stale documentation mutations rejected");
}

if (process.env.SIR_JUNIT_OUTPUT) {
  const output = resolve(root, process.env.SIR_JUNIT_OUTPUT);
  mkdirSync(dirname(output), { recursive: true });
  writeFileSync(output, '<testsuite name="SIR.ProjectArchitecture" tests="1" failures="0" errors="0" skipped="0"><testcase name="separated-project-graph" /></testsuite>\n');
}

console.log("architecture graph verified: separated project graph is current");
