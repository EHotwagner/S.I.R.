import { brotliCompressSync, gzipSync } from "node:zlib";
import { readdir, readFile } from "node:fs/promises";
import { createHash } from "node:crypto";
import { resolve } from "node:path";

const root = resolve("artifacts/client");
const maximum = (name) => {
  const configured = process.env[name];
  if (configured === undefined || configured.trim() === "") return undefined;
  const value = Number(configured);
  if (!Number.isFinite(value) || value < 0) {
    throw new Error(`${name} must be a non-negative finite number.`);
  }
  return value;
};
const appPath = resolve(root, "content/sir-client/v1/app.js");
const app = await readFile(appPath);
const entries = await readdir(resolve(root, "content/sir-client/v1"));
const deferredSupport = entries.filter((entry) => entry.startsWith("deferred-delivery-support-") && entry.endsWith(".js"));
const deferredSpatial = entries.filter((entry) => entry.startsWith("RulesExplorer-") && entry.endsWith(".js"));
const enginePath = resolve(root, "engines/0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20/worker.js");
const publication = JSON.parse(await readFile(resolve(root, "publication-manifest.json"), "utf8"));

function requireBudget(label, actual, limit) {
  if (limit !== undefined && actual > limit) {
    throw new Error(`${label} is ${actual} bytes; configured budget is ${limit}.`);
  }
}

// Delivery sizes are evidence, not project-wide fixed ceilings. A deployment
// may opt into a limit for a deliberately bounded surface through the matching
// environment variable; normal product growth remains unconstrained here.
requireBudget("app raw", app.byteLength, maximum("SIR_DELIVERY_BUDGET_MAX_APP_RAW"));
requireBudget("app gzip", gzipSync(app).byteLength, maximum("SIR_DELIVERY_BUDGET_MAX_APP_GZIP"));
requireBudget("app brotli", brotliCompressSync(app).byteLength, maximum("SIR_DELIVERY_BUDGET_MAX_APP_BROTLI"));

if (deferredSupport.length !== 1) throw new Error("Expected exactly one deferred delivery-support chunk.");
if (deferredSpatial.length !== 1) throw new Error("Expected exactly one deferred RulesExplorer spatial chunk.");
if (app.toString("utf8").includes("This support panel loads on demand")) {
  throw new Error("The deferred support panel leaked into the initial application entry.");
}
let spatial = await readFile(resolve(root, "content/sir-client/v1", deferredSpatial[0]));
requireBudget("RulesExplorer raw", spatial.byteLength, maximum("SIR_DELIVERY_BUDGET_MAX_SPATIAL_RAW"));
requireBudget("RulesExplorer gzip", gzipSync(spatial).byteLength, maximum("SIR_DELIVERY_BUDGET_MAX_SPATIAL_GZIP"));
requireBudget("RulesExplorer brotli", brotliCompressSync(spatial).byteLength, maximum("SIR_DELIVERY_BUDGET_MAX_SPATIAL_BROTLI"));
const worker = await readFile(enginePath);
const engine = publication.engines.find((entry) => entry.workerPath === "engines/0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20/worker.js");
if (!engine || engine.bytes !== worker.byteLength || engine.integrity !== `sha384-${createHash("sha384").update(worker).digest("base64")}`) {
  throw new Error("The retained engine does not match the publication integrity manifest.");
}

console.log(JSON.stringify({
  schema: "sir-production-delivery-budget-v1",
  throttle: { network: "CDP Slow 3G", cpuRate: 4 },
  initialRoute: { appRawBytes: app.byteLength, appGzipBytes: gzipSync(app).byteLength, appBrotliBytes: brotliCompressSync(app).byteLength },
  deferredChunks: [...deferredSupport, ...deferredSpatial],
  spatialDeferred: { chunk: deferredSpatial[0], rawBytes: spatial.byteLength, gzipBytes: gzipSync(spatial).byteLength, brotliBytes: brotliCompressSync(spatial).byteLength },
  immutableEngine: enginePath.slice(root.length + 1),
}));
