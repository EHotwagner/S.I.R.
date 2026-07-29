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
    const onError = (error) => {
      clearTimeout(timeout);
      worker.off("message", onMessage);
      reject(error);
    };
    const onMessage = (message) => {
      clearTimeout(timeout);
      worker.off("error", onError);
      resolveMessage(message);
    };
    const timeout = setTimeout(
      () => {
        worker.off("message", onMessage);
        worker.off("error", onError);
        reject(new Error("The worker round trip timed out."));
      },
      5_000,
    );

    worker.once("message", onMessage);
    worker.once("error", onError);
  });

const ready = await nextMessage();
if (ready.kind !== "ready") {
  throw new Error("The worker did not initialize.");
}

const request = (tag, fields) => ({ tag, fields });
const envelope = (operationValue, workerRequest) => ({
  ProtocolVersion: 3,
  Operation: operationValue,
  Request: workerRequest,
});

const fullFixture = Uint8Array.from(
  Buffer.from(
    "U0lSUgEAAAAAAQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyBRm0jlXu7so0HFYRkthl8tq0TTRjdnXgCzzz2lcGeRrQFTAAAAAAAAAAAAAAAAAAAAAgAAAAEAAAABAAAAAQAAAAAAAAACAAAAAAAAAAECAAAACgAAAAAAAAAAAAAAAGQAAAAUAAAAAQIAAAAAAAAAZAAAAAAAAAAAAAAABAAAAAEAAAABAAAAAgoAAAAUAAAAAgAAAAIAAAACCgAAABQAAAADAAAAAwAAAAIKAAAAFAAAAAQAAAAEAAAAAgoAAAAUAAAABAAAAAAAAABTAAAAAAAAAAAAAAAAAAAAAgAAAAEAAAABAAAAAQAAAAAAAAACAAAAAAAAAAECAAAACgAAAAAAAAAAAAAAAGQAAAAUAAAAAQIAAAAAAAAAZAAAAAAAAAAqsjm1kXfimMLzMNV9RUNBuZn1xV0TFOItwGzHw4qU/pV7iLEnMOZG4PM9Nhi3ffpXnoIx48WccQS+cWVhHIAnAQAAAFMAAAABAAAAAAAAAAAAAAACAAAAAQAAAAEAAAABAAAAAAAAAAIAAAAAAAAAAQIAAAAKAAAAAAAAAAAAAAAAZAAAABQAAAABAgAAAAAAAABkAAAAAAAAAGvgyj4VcBlj/q8rn1l0tsZf44WfVPeGX10b1QcE6P96lXuIsScw5kbg8z02GLd9+leegjHjxZxxBL5xZWEcgCcCAAAAUwAAAAIAAAAAAAAAAAAAAAIAAAABAAAAAQAAAAEAAAAAAAAAAgAAAAAAAAABAgAAAAoAAAAAAAAAAAAAAABkAAAAFAAAAAECAAAAAAAAAGQAAAAAAAAAtCwQJzVtTWKbcAzNk0Uk4icaGsm1qLT5LnFnK7HKN8mVe4ixJzDmRuDzPTYYt336V56CMePFnHEEvnFlYRyAJwMAAABTAAAAAwAAAAAAAAAAAAAAAgAAAAEAAAABAAAAAQAAAAAAAAACAAAAAAAAAAECAAAACgAAAAAAAAAAAAAAAGQAAAAUAAAAAQIAAAAAAAAAZAAAAAAAAACAEFkVzVmrPoLBUA4TY0/M+4VsqAnE4ZC6eGS7fHDJVJV7iLEnMOZG4PM9Nhi3ffpXnoIx48WccQS+cWVhHIAnBAAAAAEAAAAJuQFY8unb+hDZe6CXfYMOnel5GRWm3H+21r5fUmki6ZV7iLEnMOZG4PM9Nhi3ffpXnoIx48WccQS+cWVhHIAn",
    "base64",
  ),
);
const perspectiveFixture = Uint8Array.from(
  Buffer.from(
    "U0lSUgEAAAABAQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyBRm0jlXu7so0HFYRkthl8tq0TTRjdnXgCzzz2lcGeRrQAFAAAAAAAAAJLxhTMz+7Yqip79XHtI3hG5QAoWs8kWWsJX97HF5+diAQAAABzFqHvDjdif42xRgv3NHXflYslcxWEywQbgvWCFIM2+AgAAAP4v2oYmxxxhG7t7qVXw0rbfHfRA6MXRzUJvZhpz0crhAwAAAAfHslBNObv79g5Qmm8EUAXj33UM+5OpUcso3AKey5e/BAAAAF+bfVlf21lT792KgXeKQa8OL6AKzqPHYDmXhGOIY2u9",
    "base64",
  ),
);

worker.postMessage(envelope(10, request(0, ["full.sirr", fullFixture])));
const fullLoaded = await nextMessage();
if (
  fullLoaded.kind !== "response" ||
  fullLoaded.data?.Operation !== 10 ||
  fullLoaded.data?.Response?.tag !== 0 ||
  fullLoaded.data.Response.fields[1]?.tag !== 0 ||
  fullLoaded.data.Response.fields[2]?.Tick !== 0 ||
  fullLoaded.data.Response.fields[2]?.Units?.length !== 2 ||
  fullLoaded.data.Response.fields[2]?.Edges?.length !== 1
) {
  throw new Error("The full replay fixture did not load into its bounded projection.");
}

const seek = async (operation, tick) => {
  worker.postMessage(envelope(operation, request(2, [tick, 4])));
  const progress = await nextMessage();
  const completed = await nextMessage();
  if (
    progress.data?.Operation !== operation ||
    progress.data?.Response?.tag !== 1 ||
    completed.data?.Operation !== operation ||
    completed.data?.Response?.tag !== 2
  ) {
    throw new Error(`Seek ${operation} lost correlated progress or completion.`);
  }
  return completed.data.Response.fields[1];
};

const fullAtThree = await seek(11, 3);
const fullAtOneLeft = await seek(12, 1);
const fullAtOneRight = await seek(13, 1);
if (
  fullAtThree.Tick !== 3 ||
  fullAtOneLeft.Tick !== 1 ||
  JSON.stringify(fullAtOneLeft) !== JSON.stringify(fullAtOneRight)
) {
  throw new Error("The full replay fixture did not seek deterministically.");
}

worker.postMessage(
  envelope(20, request(0, ["perspective.sirr", perspectiveFixture])),
);
const perspectiveLoaded = await nextMessage();
if (
  perspectiveLoaded.data?.Operation !== 20 ||
  perspectiveLoaded.data?.Response?.tag !== 0 ||
  perspectiveLoaded.data.Response.fields[1]?.tag !== 1 ||
  perspectiveLoaded.data.Response.fields[2]?.Units?.length !== 0 ||
  !perspectiveLoaded.data.Response.fields[2]?.PerspectiveHash
) {
  throw new Error("The perspective replay exposed hidden state or failed to load.");
}

const perspectiveAtThreeLeft = await seek(21, 3);
const perspectiveAtThreeRight = await seek(22, 3);
if (
  perspectiveAtThreeLeft.Tick !== 3 ||
  JSON.stringify(perspectiveAtThreeLeft) !==
    JSON.stringify(perspectiveAtThreeRight)
) {
  throw new Error("The perspective fixture did not seek deterministically.");
}

worker.postMessage(envelope(1, request(4, ["adjacent-duel"])));
const loaded = await nextMessage();

if (
  loaded.kind !== "response" ||
  loaded.data?.Operation !== 1 ||
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
  experimented.data?.Operation !== 2 ||
  experimented.data?.Response?.tag !== 5 ||
  damage !== 100
) {
  throw new Error("The edited experiment did not survive a worker round trip.");
}

await worker.terminate();

console.log(
  "Worker round-trip smoke passed: full and perspective fixtures loaded and sought deterministically with correlated progress, disclosure stayed bounded, and scenario/experiment messages crossed both structured-clone boundaries.",
);
