import { Worker } from "node:worker_threads";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";

const workerBundle = pathToFileURL(
  resolve(
    "artifacts/client/engines/0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20/worker.js",
  ),
).href;

const wrapper = `
  import { parentPort } from "node:worker_threads";
  globalThis.postMessage = (data) => parentPort.postMessage({ kind: "response", data });
  parentPort.on("message", (data) => globalThis.onmessage?.({ data }));
  await import(${JSON.stringify(workerBundle)});
  parentPort.postMessage({ kind: "ready" });
`;

const worker = new Worker(
  new URL(`data:text/javascript,${encodeURIComponent(wrapper)}`),
  { type: "module" },
);

const nextMessage = () =>
  new Promise((resolveMessage, reject) => {
    const timeout = setTimeout(
      () => reject(new Error("The worker round trip timed out.")),
      5_000,
    );

    worker.once("message", (message) => {
      clearTimeout(timeout);
      resolveMessage(message);
    });
    worker.once("error", (error) => {
      clearTimeout(timeout);
      reject(error);
    });
  });

const ready = await nextMessage();
if (ready.kind !== "ready") {
  throw new Error("The worker did not initialize.");
}

const operation = (value) => ({ tag: 0, fields: [value] });
const request = (tag, fields) => ({ tag, fields });
const envelope = (operationValue, workerRequest) => ({
  ProtocolVersion: 1,
  Operation: operation(operationValue),
  Request: workerRequest,
});

worker.postMessage(envelope(1, request(4, ["adjacent-duel"])));
const loaded = await nextMessage();

if (
  loaded.kind !== "response" ||
  loaded.data?.Response?.tag !== 4 ||
  loaded.data.Response.fields[1]?.Identity !== "adjacent-duel" ||
  !Array.isArray(loaded.data.Response.fields[2]?.Baseline?.Metrics)
) {
  throw new Error("The scenario response did not survive structured cloning.");
}

worker.postMessage(
  envelope(
    2,
    request(5, [
      "adjacent-duel",
      [{ Key: "attack-power", Value: 30 }],
      undefined,
    ]),
  ),
);
const experimented = await nextMessage();
const forkMetrics = experimented.data?.Response?.fields[1]?.Fork?.Metrics;
const damage = forkMetrics?.find((entry) => entry.Key === "total-damage")?.Value;

if (
  experimented.kind !== "response" ||
  experimented.data?.Response?.tag !== 5 ||
  damage !== 100
) {
  throw new Error("The edited experiment did not survive a worker round trip.");
}

await worker.terminate();

console.log(
  "Worker round-trip smoke passed: scenario and edited experiment crossed both structured-clone boundaries without functions.",
);
