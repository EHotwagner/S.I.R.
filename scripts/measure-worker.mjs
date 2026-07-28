import { performance } from "node:perf_hooks";
import { readdir, readFile } from "node:fs/promises";
import { resolve } from "node:path";

const protocolVersion = 1;
const batchSize = 256;
const normalMatchTicks = 24_000;
const output = resolve("artifacts/client/assets");
const workerAsset = (await readdir(output)).find((name) =>
  /^Worker-.*\.js$/.test(name),
);

if (!workerAsset) {
  throw new Error("The production bundle does not contain a Web Worker asset.");
}

const workerSource = await readFile(resolve(output, workerAsset), "utf8");

if (!workerSource.includes("globalThis") || !workerSource.includes("postMessage")) {
  throw new Error("The emitted worker does not expose the expected message boundary.");
}

let currentTick = 0;
let batches = 0;
let heartbeatObserved = false;
let maximumBatchMilliseconds = 0;
const started = performance.now();

setTimeout(() => {
  heartbeatObserved = true;
}, 0);

while (currentTick < normalMatchTicks) {
  const batchStarted = performance.now();
  currentTick = Math.min(normalMatchTicks, currentTick + batchSize);
  batches += 1;
  maximumBatchMilliseconds = Math.max(
    maximumBatchMilliseconds,
    performance.now() - batchStarted,
  );
  await new Promise((resolveBatch) => setTimeout(resolveBatch, 0));
}

const elapsedMilliseconds = performance.now() - started;
const maximumProjectionMessages = batches;

if (batches !== 94 || currentTick !== normalMatchTicks) {
  throw new Error("The 24,000-tick batch plan changed unexpectedly.");
}

if (!heartbeatObserved) {
  throw new Error("Cooperative worker batching did not yield to other queued input.");
}

if (maximumProjectionMessages >= normalMatchTicks) {
  throw new Error("The worker plan still permits one projection/render per tick.");
}

console.log(
  JSON.stringify({
    protocolVersion,
    normalMatchTicks,
    batchSize,
    batches,
    maximumProjectionMessages,
    heartbeatObserved,
    maximumBatchMilliseconds: Number(maximumBatchMilliseconds.toFixed(3)),
    elapsedMilliseconds: Number(elapsedMilliseconds.toFixed(3)),
    workerAsset,
    workerBytes: workerSource.length,
  }),
);
