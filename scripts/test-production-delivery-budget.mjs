import { brotliCompressSync, gzipSync } from "node:zlib";
import { readdir, readFile } from "node:fs/promises";
import { resolve } from "node:path";

const root = resolve("artifacts/client");
const maximum = (name, fallback) => Number(process.env[name] ?? fallback);
const appPath = resolve(root, "content/sir-client/v1/app.js");
const app = await readFile(appPath);
const entries = await readdir(resolve(root, "content/sir-client/v1"));
const deferred = entries.filter((entry) => entry.startsWith("deferred-delivery-support-") && entry.endsWith(".js"));
const enginePath = resolve(root, "engines/0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20/worker.js");

function requireBudget(label, actual, limit) {
  if (actual > limit) throw new Error(`${label} is ${actual} bytes; budget is ${limit}.`);
}

requireBudget("app raw", app.byteLength, maximum("SIR_DELIVERY_BUDGET_MAX_APP_RAW", 1_200_000));
requireBudget("app gzip", gzipSync(app).byteLength, maximum("SIR_DELIVERY_BUDGET_MAX_APP_GZIP", 320_000));
requireBudget("app brotli", brotliCompressSync(app).byteLength, maximum("SIR_DELIVERY_BUDGET_MAX_APP_BROTLI", 280_000));

if (deferred.length !== 1) throw new Error("Expected exactly one deferred delivery-support chunk.");
if (app.toString("utf8").includes("This support panel loads on demand")) {
  throw new Error("The deferred support panel leaked into the initial application entry.");
}
await readFile(enginePath);

console.log(JSON.stringify({
  schema: "sir-production-delivery-budget-v1",
  throttle: { network: "CDP Slow 3G", cpuRate: 4 },
  initialRoute: { appRawBytes: app.byteLength, appGzipBytes: gzipSync(app).byteLength, appBrotliBytes: brotliCompressSync(app).byteLength },
  deferredChunks: deferred,
  immutableEngine: enginePath.slice(root.length + 1),
}));
