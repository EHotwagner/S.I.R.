import { createHash } from "node:crypto";
import { readFile, readdir, stat, writeFile } from "node:fs/promises";
import { resolve, relative, sep } from "node:path";
import { performance } from "node:perf_hooks";

const root = resolve(".");
const site = resolve(process.argv[2] ?? "artifacts/site");
const receiptPath = process.argv[3] ? resolve(process.argv[3]) : undefined;
const manifestPath = resolve(site, "content/sir-client/v1/in-app-docs.json");

const definition = {
  schema: "sir-in-app-docs-performance-v1",
  workload: "qualified-corpus-los-cover-armor-navigation",
  caps: { pages: 512, blocks: 8192, searchTokens: 262144, results: 200, history: 128, domNodes: 6000 },
  timingMs: { representativeP95: 20, fullCorpusCeiling: 50 },
  iterations: 100,
  queries: ["los", "cover", "armor"],
};

const stableValue = (value) => {
  if (Array.isArray(value)) return value.map(stableValue);
  if (value && typeof value === "object") {
    return Object.fromEntries(Object.keys(value).sort().map((key) => [key, stableValue(value[key])]));
  }
  return value;
};
const stable = (value) => JSON.stringify(stableValue(value));
const sha256 = (value) => createHash("sha256").update(value).digest("hex");
const definitionDigest = sha256(stable(definition));

async function filesUnder(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const nested = await Promise.all(entries.map(async (entry) => {
    const path = resolve(directory, entry.name);
    return entry.isDirectory() ? filesUnder(path) : [path];
  }));
  return nested.flat();
}

const sources = (await filesUnder(resolve("docs")))
  .filter((path) => /\.(?:md|fsx)$/i.test(path))
  .sort();
const pages = await Promise.all(sources.map(async (path) => ({
  path: relative(root, path).split(sep).join("/"),
  text: await readFile(path, "utf8"),
})));
const searchRows = JSON.parse(await readFile(resolve(site, "index.json"), "utf8"));
const corpus = pages.map((page) => `${page.path}\n${page.text}`.toLocaleLowerCase("en-US"));
const samples = [];
let maximumResults = 0;
for (let i = 0; i < definition.iterations; i += 1) {
  const started = performance.now();
  for (const query of definition.queries) {
    const results = corpus.filter((text) => text.includes(query));
    maximumResults = Math.max(maximumResults, results.length);
  }
  samples.push(performance.now() - started);
}
samples.sort((a, b) => a - b);
const p95 = samples[Math.ceil(samples.length * 0.95) - 1];
const tokenCount = corpus.reduce((total, text) => total + (text.match(/[\p{L}\p{N}_-]+/gu)?.length ?? 0), 0);

let manifest;
try {
  manifest = JSON.parse(await readFile(manifestPath, "utf8"));
} catch (error) {
  if (error?.code !== "ENOENT") throw error;
}

const counters = {
  pages: manifest?.pages?.length ?? pages.length,
  blocks: manifest?.pages?.reduce((total, page) => total + page.blocks.length, 0) ?? 0,
  searchTokens: manifest?.searchTokenCount ?? tokenCount,
  maximumResults,
  historyLimit: manifest?.limits?.history ?? definition.caps.history,
  generatedSearchRows: searchRows.length,
  domNodes: manifest?.performance?.maximumDomNodes ?? 0,
};
const failures = [
  ["pages", counters.pages, definition.caps.pages],
  ["blocks", counters.blocks, definition.caps.blocks],
  ["searchTokens", counters.searchTokens, definition.caps.searchTokens],
  ["results", counters.maximumResults, definition.caps.results],
  ["history", counters.historyLimit, definition.caps.history],
  ["domNodes", counters.domNodes, definition.caps.domNodes],
].filter(([, observed, cap]) => observed > cap);
if (p95 > definition.timingMs.representativeP95) failures.push(["representativeP95Ms", p95, definition.timingMs.representativeP95]);

const receipt = {
  schema: definition.schema,
  definitionDigest,
  candidate: process.env.GITHUB_SHA ?? "working-tree",
  runtime: process.version,
  host: `${process.platform}-${process.arch}`,
  capability: { headlessBrowser: true, liveCompositorMeasured: false },
  manifestPresent: Boolean(manifest),
  counters,
  timing: { representativeP95Ms: Number(p95.toFixed(3)), fullCorpusCeilingMs: definition.timingMs.fullCorpusCeiling },
  failures: failures.map(([name, observed, cap]) => ({ name, observed, cap })),
};
if (receiptPath) await writeFile(receiptPath, `${JSON.stringify(receipt, null, 2)}\n`);
console.log(JSON.stringify(receipt));
if (failures.length > 0) process.exitCode = 1;
