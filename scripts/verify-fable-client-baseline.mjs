import { readFile } from "node:fs/promises";
import { resolve } from "node:path";

const root = resolve(import.meta.dirname, "..");
const read = (path) => readFile(resolve(root, path), "utf8");
const baseline = JSON.parse(await read("config/fable-client-baseline.json"));

const requireValue = (condition, message) => {
  if (!condition) throw new Error(`Fable client baseline drift: ${message}.`);
};

const requireSourceValue = (source, pattern, label) => {
  requireValue(pattern.test(source), `${label} does not match the executable source`);
};

requireValue(
  baseline.schema === "sir-fable-client-baseline-v1",
  "the baseline schema is unsupported",
);

const [
  replaySource,
  fixedPointSource,
  shellSource,
  labSource,
  packagesSource,
  tools,
  sdk,
  packageManifest,
  publication,
  accessibilitySource,
  runnerSource,
  workerSource,
  publicationSource,
] = await Promise.all([
  read("src/SIR.Simulation/Replay.fs"),
  read("src/SIR.Domain/FixedPoint.fs"),
  read("src/SIR.Client/Shell.fs"),
  read("src/SIR.Client/Lab.fs"),
  read("Directory.Packages.props"),
  read(".config/dotnet-tools.json").then(JSON.parse),
  read("global.json").then(JSON.parse),
  read("package.json").then(JSON.parse),
  read("config/engine-publication.json").then(JSON.parse),
  read("scripts/test-docs-accessibility.mjs"),
  read("src/SIR.Client.Web/Runner.fs"),
  read("src/SIR.Client.Web/Worker.fs"),
  read("scripts/generate-publication-manifest.mjs"),
]);

const replay = baseline.replay;
requireSourceValue(
  replaySource,
  new RegExp(`let CurrentFormatVersion = ${replay.formatVersion}\\b`),
  "replay format version",
);

const replayLimits = {
  MaxPackageBytes: replay.limits.maxPackageBytes,
  MaxInputs: replay.limits.maxInputs,
  MaxWasmOutputs: replay.limits.maxWasmOutputs,
  MaxCheckpoints: replay.limits.maxCheckpoints,
  MaxPerspectiveFrames: replay.limits.maxPerspectiveFrames,
  MaxUnits: replay.limits.maxUnits,
  MaxEdges: replay.limits.maxEdges,
  MaxObservations: replay.limits.maxObservations,
  MaxAwarenessContacts: replay.limits.maxAwarenessContacts,
  MaxEngagements: replay.limits.maxEngagements,
};

for (const [name, value] of Object.entries(replayLimits)) {
  const formatted = value.toLocaleString("en-US").replaceAll(",", "_");
  requireSourceValue(
    replaySource,
    new RegExp(`${name} = ${formatted}\\b`),
    `replay limit ${name}`,
  );
}

requireValue(
  replay.encoding === "canonical-binary-little-endian" &&
    replay.compression === "none" &&
    replay.stateHash === "sha-256" &&
    replay.eventHash === "sha-256" &&
    replay.hashTree === "none",
  "the replay encoding/hash contract changed without a new baseline schema",
);
requireSourceValue(
  replaySource,
  /let stateHash state\s*=\s*stateHashForFormatVersion CurrentFormatVersion state/,
  "state hash",
);
requireSourceValue(
  replaySource,
  /let eventHash events = events \|> Simulation\.eventsBytes \|> CanonicalHash\.sha256/,
  "event hash",
);

requireSourceValue(
  fixedPointSource,
  new RegExp(`let Scale = ${baseline.numerics.fixedPointScale.toLocaleString("en-US").replaceAll(",", "_")}\\b`),
  "fixed-point scale",
);
requireSourceValue(
  fixedPointSource,
  /ties away from zero/,
  "fixed-point rounding policy",
);
requireSourceValue(
  fixedPointSource,
  /let private saturate \(candidate: int64\)/,
  "fixed-point overflow policy",
);

requireSourceValue(
  shellSource,
  new RegExp(`let CurrentVersion = ${baseline.browser.workerProtocolVersion}\\b`),
  "worker protocol version",
);
requireSourceValue(
  shellSource,
  new RegExp(`let BatchSize = ${baseline.browser.workerBatchTicks}\\b`),
  "worker batch size",
);
requireSourceValue(
  labSource,
  new RegExp(`let ExportFormat = "${baseline.browser.experimentExportSchema}"`),
  "experiment export schema",
);
requireSourceValue(
  runnerSource,
  /let mutable private worker: \(string \* obj\) option = None/,
  "single active versioned worker topology",
);
requireSourceValue(
  workerSource,
  /for batchEnd in WorkerProtocol\.batchEnds currentTick target do/,
  "worker batch execution",
);
requireSourceValue(
  workerSource,
  /RunnerProgress\(/,
  "compact projection progress cadence",
);
requireValue(
  baseline.browser.smokeAndAccessibilityHarness === "happy-dom" &&
    packageManifest.devDependencies["happy-dom"] &&
    accessibilitySource.includes('from "happy-dom"'),
  "the browser accessibility harness changed",
);

requireValue(
  publication.replayRetentionPolicy.id === baseline.publication.retentionPolicy,
  "the engine retention policy changed",
);
requireValue(
  baseline.publication.assetIntegrity === "sha-384",
  "the publication integrity algorithm changed without a new baseline schema",
);
requireSourceValue(
  publicationSource,
  /createHash\("sha384"\)/,
  "publication integrity algorithm",
);

const packageVersion = (name) => {
  const escaped = name.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  return packagesSource.match(
    new RegExp(`<PackageVersion Include="${escaped}" Version="\\[([^\\]]+)\\]"`),
  )?.[1];
};

const toolchain = baseline.toolchain;
requireValue(sdk.sdk.version === toolchain.dotnetSdk, ".NET SDK version changed");
requireValue(tools.tools.fable.version === toolchain.fable, "Fable version changed");
requireValue(tools.tools["fsdocs-tool"].version === toolchain.fsdocs, "fsdocs version changed");
requireValue(packageVersion("FSharp.Core") === toolchain.fsharpCore, "FSharp.Core version changed");
requireValue(packageVersion("FS.GG.Game.Core") === toolchain.fsGgGameCore, "FS.GG.Game.Core version changed");
requireValue(
  baseline.upstream.package === "FS.GG.Game.Core" &&
    baseline.upstream.version === toolchain.fsGgGameCore &&
    baseline.upstream.profile === "fs-gg-game-core-fable-lockstep-v1",
  "the upstream compatibility profile changed without a new baseline schema",
);
requireValue(packageVersion("Fable.Elmish") === toolchain.elmish, "Elmish version changed");
requireValue(
  packageVersion("Fable.Elmish.React") === toolchain.elmishReact,
  "Elmish.React version changed",
);
requireValue(packageManifest.dependencies.react === toolchain.react, "React version changed");
requireValue(packageManifest.devDependencies.vite === toolchain.vite, "Vite version changed");
requireValue(packageManifest.engines.node === toolchain.node, "Node version changed");

requireValue(
  baseline.futureProfiles.length === 3,
  "future profile ownership must remain explicit",
);

console.log(
  `Fable client baseline verified: replay v${replay.formatVersion}, worker protocol v${baseline.browser.workerProtocolVersion}, ${baseline.browser.workerBatchTicks}-tick batches, ${baseline.futureProfiles.length} future profiles.`,
);
