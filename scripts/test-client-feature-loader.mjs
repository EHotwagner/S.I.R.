import { brotliCompressSync, gzipSync } from "node:zlib";
import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";
import { mkdir, readFile, readdir, writeFile } from "node:fs/promises";
import { dirname, relative, resolve } from "node:path";
import { pathToFileURL } from "node:url";

const root = resolve(import.meta.dirname, "..");
const registryPath = resolve(root, "src/SIR.Client.Web/feature-registry.v1.json");
const artifactRoot = resolve(root, "artifacts/client");
const sourceOnly = process.argv.includes("--source-only");
const noWrite = process.argv.includes("--no-write");
const optionValue = (name) => {
  const index = process.argv.indexOf(name);
  return index < 0 ? undefined : process.argv[index + 1];
};
const aggregateTrx = optionValue("--aggregate-trx");
const browserJunitPath = optionValue("--browser-junit");
const mutation = process.env.SIR_FEATURE_LOADER_MUTATION ?? "";
const sha256 = (bytes) => createHash("sha256").update(bytes).digest("hex");

function fail(subject, detail) {
  throw new Error(`client-feature-loader:${subject}: ${detail}`);
}

const registryBytes = await readFile(registryPath);
const registry = JSON.parse(registryBytes);
if (registry.schema !== "sir.client.feature-registry/v1" || registry.version !== 1) fail("registry", "unsupported schema/version");
if (!Array.isArray(registry.features) || registry.features.length !== 5) fail("registry", "expected exactly five v1 features");
const featureIds = registry.features.map((feature) => feature.id);
if (featureIds.join("|") !== [...featureIds].sort().join("|")) fail("registry", "features are not in stable id order");
if (new Set(featureIds).size !== featureIds.length) fail("registry", "duplicate feature id");
const phases = new Set(["bootstrap", "eager", "deferred"]);
for (const feature of registry.features) {
  if (!/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(feature.id) || !phases.has(feature.phase)) fail("registry", `invalid identity/phase for ${feature.id}`);
  for (const field of ["control", "route", "module", "logicalChunk"]) if (typeof feature[field] !== "string" || feature[field].length === 0) fail("registry", `${feature.id} missing ${field}`);
  for (const kind of ["raw", "gzip", "brotli"]) if (!Number.isSafeInteger(feature.budget?.[kind]) || feature.budget[kind] <= 0) fail("budget", `${feature.id} invalid ${kind}`);
}
if (new Set(registry.features.map((feature) => feature.logicalChunk)).size !== 4) fail("registry", "v1 logical chunk projection changed");

const deliverySupportEntryPath = resolve(root, "src/SIR.Client.Web/delivery-support-entry.js");
if (mutation === "eager-import") {
  const original = await readFile(deliverySupportEntryPath, "utf8");
  try {
    await writeFile(deliverySupportEntryPath, `${original.trimEnd()}\nimport "./docs-feature.js";\n`);
    let diagnostic = "";
    try {
      execFileSync(process.execPath, [import.meta.filename, "--source-only"], {
        cwd: root,
        env: { ...process.env, SIR_FEATURE_LOADER_MUTATION: "" },
        encoding: "utf8",
        stdio: "pipe",
      });
    } catch (error) {
      diagnostic = `${error.stdout ?? ""}\n${error.stderr ?? ""}`;
    }
    if (!diagnostic.includes("client-feature-loader:eager-import")) {
      fail("eager-import", "real static Docs import mutation passed or lacked its subject-specific diagnostic");
    }
    fail("eager-import", "self-restoring real static Docs import mutation was rejected");
  } finally {
    await writeFile(deliverySupportEntryPath, original);
  }
}

const loaderSource = await readFile(resolve(root, "src/SIR.Client.Web/feature-loader.js"), "utf8");
const deliverySupportEntrySource = await readFile(deliverySupportEntryPath, "utf8");
const fsharpSource = await readFile(resolve(root, "src/SIR.Client.Web/FeatureLoader.fs"), "utf8");
const appSource = [
  await readFile(resolve(root, "src/SIR.Client.Web/App.fs"), "utf8"),
  await readFile(resolve(root, "src/SIR.Client.Web/ClientFeatureRuntime.fs"), "utf8"),
].join("\n");
const viteSource = await readFile(resolve(root, "src/SIR.Client.Web/vite.config.js"), "utf8");
for (const feature of registry.features) {
  if (!fsharpSource.includes(`\"${feature.id}\"`)) fail("projection", `F# projection missing ${feature.id}`);
  if (feature.phase === "deferred") {
    if (feature.id === "delivery-support") {
      if (!deliverySupportEntrySource.includes('import("./deferred-delivery-support.js")')) {
        fail("eager-import", `${feature.id} lacks its declared literal dynamic import`);
      }
    } else {
      const literal = feature.id === "rules-explorer"
        ? 'import("./.fable/RulesExplorer.js")'
        : `import(\"./${feature.logicalChunk}.js\")`;
      if (!loaderSource.includes(literal)) fail("eager-import", `${feature.id} lacks its declared literal dynamic import`);
      const viewLiteral = feature.id === "docs"
        ? 'React.DynamicImported("../../docs-feature.js")'
        : `React.DynamicImported(\"../${feature.logicalChunk}.js\")`;
      if (!appSource.includes(viewLiteral)) fail("eager-import", `${feature.id} lacks its declared lazy view edge`);
    }
  }
}
const staticImportSubjects = `${loaderSource}\n${deliverySupportEntrySource}`;
if (/^import\s+(?!React\b).*docs-feature\.js/m.test(staticImportSubjects) || /^import\s+(?!React\b).*RulesExplorer\.js/m.test(staticImportSubjects)) {
  fail("eager-import", "deferred feature is statically reachable outside its declared edge");
}
const mangleSubject = mutation === "property-mangle" ? viteSource.replace("properties: false", "properties: true") : viteSource;
if (!/mangle:\s*\{\s*properties:\s*false\s*\}/s.test(mangleSubject)) fail("property-mangle", "property-name mangling must remain explicitly disabled");

async function sourceFiles(directory) {
  const output = [];
  for (const entry of await readdir(directory, { withFileTypes: true })) {
    if ([".fable", ".fable-rules", "bin", "obj"].includes(entry.name)) continue;
    const path = resolve(directory, entry.name);
    if (entry.isDirectory()) output.push(...(await sourceFiles(path)));
    else output.push(path);
  }
  return output;
}

const inputs = [...(await sourceFiles(resolve(root, "src/SIR.Client.Web"))), resolve(root, "scripts/build-client.sh")].sort();
const inputHash = createHash("sha256");
for (const path of inputs) {
  inputHash.update(relative(root, path).replaceAll("\\", "/"));
  inputHash.update("\0");
  inputHash.update(await readFile(path));
  inputHash.update("\0");
}
const buildInputDigest = inputHash.digest("hex");

if (sourceOnly) {
  console.log(JSON.stringify({ schema: "sir.client.feature-loader-source-gate/v1", registryDigest: sha256(registryBytes), buildInputDigest, features: featureIds }));
  process.exit(0);
}

// Exercise the actual Fable projection rather than a JavaScript reimplementation
// of the state machine. These tag assertions are test-local; public consumers use
// the signed F# union cases from FeatureLoader.fsi.
const compiledLoader = await import(pathToFileURL(resolve(root, "src/SIR.Client.Web/.fable/SIR.Client.Web/FeatureLoader.js")));
const compiledResult = await import(pathToFileURL(resolve(root, "src/SIR.Client.Web/.fable/fable_modules/fable-library-js.5.13.0/Result.js")));
const pendingIdentity = compiledLoader.identityFor(compiledLoader.docs);
const loadingStates = compiledLoader.beginLoad(pendingIdentity, compiledLoader.initial);
const loadingState = compiledLoader.stateFor(compiledLoader.docs, loadingStates);
if (loadingState.tag !== 1) fail("state", "request did not enter Loading");
const completed = compiledLoader.reconcile(
  pendingIdentity,
  new compiledResult.FSharpResult$2(0, [pendingIdentity]),
  loadingState,
);
if (completed.tag !== 0 || completed.fields[0].tag !== 2) fail("state", "matching completion did not enter Loaded");
const staleIdentity = new compiledLoader.ChunkIdentity(
  pendingIdentity.RegistryVersion + 1,
  pendingIdentity.Feature,
  pendingIdentity.LogicalChunk,
);
const ignored = compiledLoader.reconcile(
  pendingIdentity,
  new compiledResult.FSharpResult$2(0, [staleIdentity]),
  loadingState,
);
if (ignored.tag !== 1 || !compiledLoader.describeFailure(ignored.fields[0]).startsWith("stale-identity:")) {
  fail("stale-identity", "Fable state machine accepted a mismatched completion");
}
const missing = compiledLoader.reconcile(
  pendingIdentity,
  new compiledResult.FSharpResult$2(1, [new compiledLoader.LoadFailure(0, ["fixture"])]),
  loadingState,
);
if (missing.tag !== 0 || missing.fields[0].tag !== 3 || !compiledLoader.describeFailure(missing.fields[0].fields[1]).startsWith("missing-chunk:")) {
  fail("missing-chunk", "Fable state machine did not retain a stable failure");
}

const contentDirectory = resolve(artifactRoot, "content/sir-client/v1");
const emittedNames = (await readdir(contentDirectory)).filter((name) => name.endsWith(".js")).sort();
const manifest = JSON.parse(await readFile(resolve(artifactRoot, ".vite/manifest.json"), "utf8"));
const manifestEntry = Object.values(manifest).find((entry) => entry.isEntry);
if (!manifestEntry) fail("bundle-graph", "Vite entry is missing");
const dynamicEntries = Object.entries(manifest).filter(([, entry]) => entry.isDynamicEntry);
const dynamicKeys = dynamicEntries.map(([key]) => key).sort();
const dynamicFiles = dynamicEntries.map(([, entry]) => entry.file).sort();
const expectedDynamicKeys = registry.features
  .filter((feature) => feature.phase === "deferred")
  .map((feature) => feature.id === "rules-explorer" ? ".fable/RulesExplorer.js" : feature.module.replace("src/SIR.Client.Web/", ""))
  .sort();
const unregisteredDynamicKeys = dynamicKeys.filter((key) => !expectedDynamicKeys.includes(key));
const missingDynamicKeys = expectedDynamicKeys.filter((key) => !dynamicKeys.includes(key));
if (unregisteredDynamicKeys.length > 0) fail("bundle-graph", `unregistered dynamic identity: ${unregisteredDynamicKeys.join(", ")}`);
if (missingDynamicKeys.length > 0) fail("bundle-graph", `registered dynamic identity missing: ${missingDynamicKeys.join(", ")}`);
const entryDynamicKeys = [...(manifestEntry.dynamicImports ?? [])].sort();
if (entryDynamicKeys.join("|") !== expectedDynamicKeys.join("|")) fail("bundle-graph", "entry dynamic-import inventory disagrees with registry ownership");

const features = [];
for (const feature of registry.features) {
  const expectedPrefix = feature.logicalChunk === "app" ? "app.js" : `${feature.logicalChunk}-`;
  let matches = emittedNames.filter((name) => feature.logicalChunk === "app" ? name === expectedPrefix : name.startsWith(expectedPrefix));
  if (mutation === "missing-chunk" && feature.id === "docs") matches = [];
  if (matches.length !== 1) fail("missing-chunk", `${feature.id} expected one emitted ${feature.logicalChunk} chunk; found ${matches.length}`);
  const emitted = matches[0];
  const bytes = await readFile(resolve(contentDirectory, emitted));
  const measured = { raw: bytes.byteLength, gzip: gzipSync(bytes).byteLength, brotli: brotliCompressSync(bytes).byteLength };
  if (mutation === "budget" && feature.id === "docs") measured.raw = feature.budget.raw + 1;
  for (const kind of ["raw", "gzip", "brotli"]) if (measured[kind] > feature.budget[kind]) fail("budget", `${feature.id} ${kind} ${measured[kind]} exceeds ${feature.budget[kind]}`);
  const logicalChunk = mutation === "stale-identity" && feature.id === "docs" ? "docs-stale" : feature.logicalChunk;
  if (logicalChunk !== feature.logicalChunk) fail("stale-identity", `${feature.id} expected ${feature.logicalChunk}; received ${logicalChunk}`);
  features.push({
    id: feature.id,
    phase: feature.phase,
    route: feature.route,
    control: feature.control,
    logicalChunk,
    emitted: `content/sir-client/v1/${emitted}`,
    bytes: measured,
    budget: feature.budget,
    sha256: sha256(bytes),
  });
}

const receipt = {
  schema: "sir.client.feature-bundle-graph/v1",
  registryVersion: registry.version,
  registryDigest: sha256(registryBytes),
  buildInputDigest,
  entry: manifestEntry.file,
  dynamicEntries: dynamicFiles,
  features,
};
const canonical = `${JSON.stringify(receipt, null, 2)}\n`;
const receiptDigest = sha256(canonical);
const receiptPath = resolve(root, `docs/evidence/client-feature-bundle-graph-v1/${receiptDigest}.json`);
if (!noWrite) {
  await mkdir(dirname(receiptPath), { recursive: true });
  await writeFile(receiptPath, canonical);
  const replay = await readFile(receiptPath, "utf8");
  if (replay !== canonical || sha256(replay) !== receiptDigest) fail("bundle-graph", "content-addressed receipt replay mismatch");
}

if (aggregateTrx) {
  const mutationNames = ["eager-import", "property-mangle", "missing-chunk", "stale-identity", "budget"];
  for (const name of mutationNames) {
    try {
      execFileSync(process.execPath, [import.meta.filename, "--no-write"], {
        cwd: root,
        env: { ...process.env, SIR_FEATURE_LOADER_MUTATION: name },
        encoding: "utf8",
        stdio: "pipe",
      });
      fail(name, "protected mutation unexpectedly passed");
    } catch (error) {
      const output = `${error.stdout ?? ""}\n${error.stderr ?? ""}`;
      if (!output.includes(`client-feature-loader:${name}`)) fail(name, "protected mutation lacked its subject-specific diagnostic");
    }
  }
  if (!browserJunitPath) fail("browser", "--browser-junit is required with --aggregate-trx");
  const browserJunit = await readFile(resolve(root, browserJunitPath), "utf8");
  if (/<failure\b|<error\b/.test(browserJunit)) fail("browser", "browser JUnit contains a failing case");
  const browserNames = [...browserJunit.matchAll(/<testcase\b[^>]*\bname="([^"]+)"/g)].map((match) => match[1]);
  if (!browserNames.some((name) => name.includes("production shell loads registered features"))
      || !browserNames.some((name) => name.includes("stable offline failure"))) {
    fail("browser", "browser JUnit is missing the client feature loader journeys");
  }
  const testNames = [
    "registry, compiled-Fable state, bundle graph, and budgets pass",
    ...mutationNames.map((name) => `${name} mutation is rejected`),
    ...browserNames.filter((name) => name.includes("production shell loads registered features") || name.includes("stable offline failure")),
  ];
  const guid = (value) => {
    const hex = sha256(value).slice(0, 32);
    return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
  };
  const xml = (value) => value.replaceAll("&", "&amp;").replaceAll('"', "&quot;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");
  const results = testNames.map((name) =>
    `    <UnitTestResult executionId="${guid(`execution:${name}`)}" testId="${guid(`test:${name}`)}" testName="${xml(name)}" outcome="Passed" />`,
  ).join("\n");
  const trx = `<?xml version="1.0" encoding="utf-8"?>\n<TestRun id="${guid(testNames.join("\n"))}" name="client-feature-loader aggregate" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">\n  <Results>\n${results}\n  </Results>\n  <ResultSummary outcome="Completed">\n    <Counters total="${testNames.length}" executed="${testNames.length}" passed="${testNames.length}" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" notExecuted="0" />\n  </ResultSummary>\n</TestRun>\n`;
  const trxPath = resolve(root, aggregateTrx);
  await mkdir(dirname(trxPath), { recursive: true });
  await writeFile(trxPath, trx);
}
console.log(JSON.stringify({ schema: receipt.schema, receipt: relative(root, receiptPath), receiptDigest, registryDigest: receipt.registryDigest, buildInputDigest, features }));
