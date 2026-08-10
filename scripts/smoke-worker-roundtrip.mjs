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

const startedAt = performance.now();
const queuedMessages = [];
const observedMessages = [];
let workerError;
let workerExit;
let pendingRead;

const describeMessage = (message) => {
  const data = message?.data;
  return {
    kind: message?.kind,
    operation: data?.Correlation?.Operation ?? data?.Operation,
    response: data?.Response?.tag,
  };
};

const diagnostic = (expectation) =>
  `${expectation}; observed=${JSON.stringify(observedMessages)}; queued=${JSON.stringify(
    queuedMessages.map(describeMessage),
  )}; workerExit=${workerExit ?? "running"}; elapsed=${(
    performance.now() - startedAt
  ).toFixed(1)}ms`;

const deliver = () => {
  if (!pendingRead || queuedMessages.length === 0) return;
  const { resolve, timeout } = pendingRead;
  pendingRead = undefined;
  clearTimeout(timeout);
  resolve(queuedMessages.shift());
};

const onMessage = (message) => {
  observedMessages.push(describeMessage(message));
  queuedMessages.push(message);
  deliver();
};
const onError = (error) => {
  workerError = error;
  if (pendingRead) {
    const { reject, timeout } = pendingRead;
    pendingRead = undefined;
    clearTimeout(timeout);
    reject(new Error(`${error.message}; ${diagnostic("worker error")}`));
  }
};
const onExit = (code) => {
  workerExit = code;
  if (pendingRead) {
    const { reject, timeout } = pendingRead;
    pendingRead = undefined;
    clearTimeout(timeout);
    reject(new Error(`Worker exited with ${code}; ${diagnostic("awaiting response")}`));
  }
};

// Register exactly once, before the worker can emit ready/progress/completion.
worker.on("message", onMessage);
worker.on("error", onError);
worker.on("exit", onExit);

const nextMessage = (expectation = "worker response") =>
  new Promise((resolveMessage, reject) => {
    if (workerError) {
      reject(new Error(`${workerError.message}; ${diagnostic(expectation)}`));
      return;
    }
    if (workerExit !== undefined) {
      reject(new Error(`Worker exited with ${workerExit}; ${diagnostic(expectation)}`));
      return;
    }
    if (queuedMessages.length > 0) {
      resolveMessage(queuedMessages.shift());
      return;
    }
    const timeout = setTimeout(() => {
      if (pendingRead?.timeout === timeout) pendingRead = undefined;
      reject(new Error(`The worker round trip timed out: ${diagnostic(expectation)}`));
    }, 5_000);
    pendingRead = { resolve: resolveMessage, reject, timeout };
  });

const responseFor = async (operation, responseTag, expectation) => {
  const message = await nextMessage(expectation);
  const actualOperation = message.data?.Correlation?.Operation ?? message.data?.Operation;
  const actualTag = message.data?.Response?.tag;
  // Exercise the gate's red path in CI/authoring without changing production
  // worker behavior: a response tag mismatch must never be accepted.
  const expectedTag =
    process.env.SIR_WORKER_ROUNDTRIP_INJECT_RESPONSE_TAG === "1"
      ? -1
      : responseTag;
  if (
    message.kind !== "response" ||
    actualOperation !== operation ||
    (expectedTag !== null && actualTag !== expectedTag)
  ) {
    throw new Error(
      `Unexpected worker response: expected operation=${operation}, response=${expectedTag}; received ${JSON.stringify(describeMessage(message))}; ${diagnostic(expectation)}`,
    );
  }
  return message;
};

const cleanup = () => {
  worker.off("message", onMessage);
  worker.off("error", onError);
  worker.off("exit", onExit);
};

try {
const ready = await nextMessage("worker ready handshake");
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
const fullLoaded = await responseFor(10, 0, "load full replay");
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
  const progress = await responseFor(operation, 1, `seek ${operation} progress`);
  // The worker emits completion immediately after yielding. Let it arrive before
  // the next read to prove the persistent listener buffers back-to-back replies.
  await new Promise((resolveTurn) => setTimeout(resolveTurn, 0));
  const completed = await responseFor(operation, 2, `seek ${operation} completion`);
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
const perspectiveLoaded = await responseFor(20, 0, "load perspective replay");
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
const loaded = await responseFor(1, 4, "load lab scenario");

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
const experimented = await responseFor(2, 5, "run lab experiment");
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

const simulatorProjection = structuredClone(fullLoaded.data.Response.fields[2]);
const simulatorCorrelation = (
  Operation,
  Tick = 0,
  MapRevision = "map-revision-a",
  PlanRevision = 0n,
) => ({
  Operation,
  Session: "simulator-session-a",
  MapRevision,
  PlanRevision,
  Tick,
});
const simulatorEnvelope = (correlation, simulatorRequest) => ({
  Kind: "sir-simulator-session",
  ProtocolVersion: 1,
  Correlation: correlation,
  Request: simulatorRequest,
});
const simulatorRequest = (tag, fields = []) => ({ tag, fields });
const postSimulator = async (operation, tag, fields = [], tick = 0) => {
  worker.postMessage(
    simulatorEnvelope(
      simulatorCorrelation(operation, tick),
      simulatorRequest(tag, fields),
    ),
  );
  const message = await responseFor(operation, null, `simulator operation ${operation}`);
  if (
    message.kind !== "response" ||
    message.data?.Kind !== "sir-simulator-session" ||
    message.data?.ProtocolVersion !== 1 ||
    message.data?.Correlation?.Operation !== operation ||
    message.data?.Correlation?.Session !== "simulator-session-a" ||
    message.data?.Correlation?.MapRevision !== "map-revision-a" ||
    message.data?.Correlation?.PlanRevision !== 0n ||
    message.data?.Correlation?.Tick !== tick ||
    typeof message.data?.CurrentTick !== "number"
  ) {
    throw new Error(
      `Simulator operation ${operation} lost structured-clone correlation.`,
    );
  }
  return message.data;
};

const intentOnlyPlan = {
  EncodedDocument: new TextEncoder().encode(
    `SIR-PLAN 1\nplan|00000000000000000000000000000001|0|-|${"00".repeat(32)}|72756c6573|0|6000\n`,
  ),
  HorizonTicks: 6_000,
  PreviewLabel: { tag: 2, fields: [] },
  Assumptions: [],
  Intents: ["unit 10 moves east"],
};
const deterministicPlan = {
  ...intentOnlyPlan,
  PreviewLabel: { tag: 0, fields: [] },
  Intents: [],
};
const assumptionPlan = {
  ...intentOnlyPlan,
  PreviewLabel: { tag: 1, fields: [] },
  Assumptions: ["enemy remains at the disclosed location"],
  Intents: [],
};

const initialized = await postSimulator(100, 0, [
  {
    InitialProjection: simulatorProjection,
    MaximumHorizonTicks: 6_000,
  },
]);
if (
  initialized.Response?.tag !== 0 ||
  initialized.Response.fields[0]?.IsSnapshot !== true ||
  initialized.Response.fields[0]?.Projection?.Tick !== 0
) {
  throw new Error("Simulator initialization did not clone a bounded snapshot.");
}

const invalidValidation = await postSimulator(101, 1, [
  { ...intentOnlyPlan, EncodedDocument: Uint8Array.from([0x53]) },
]);
if (
  invalidValidation.Response?.tag !== 1 ||
  invalidValidation.Response.fields[0] != null ||
  invalidValidation.Response.fields[1]?.[0]?.Code !==
    "SIR.PLAN.STRUCTURAL.BAD_HEADER" ||
  !Array.isArray(invalidValidation.Response.fields[1][0].Fields)
) {
  throw new Error("Structured simulator diagnostics did not clone safely.");
}

const validated = await postSimulator(109, 1, [intentOnlyPlan]);
if (
  validated.Response?.tag !== 1 ||
  validated.Response.fields[0] !== 0n ||
  validated.Response.fields[1]?.length !== 0
) {
  throw new Error("Simulator plan validation did not clone its diagnostics.");
}

for (const [operation, plan, expectedLabel] of [
  [102, deterministicPlan, 0],
  [103, assumptionPlan, 1],
  [104, intentOnlyPlan, 2],
]) {
  const previewed = await postSimulator(operation, 2, [plan, 0, 1_200]);
  const [label, disclosures, updates] = previewed.Response?.fields ?? [];
  if (
    previewed.Response?.tag !== 2 ||
    label?.tag !== expectedLabel ||
    !Array.isArray(disclosures) ||
    !Array.isArray(updates) ||
    updates.length !== 1
  ) {
    throw new Error(`Preview label ${expectedLabel} did not clone safely.`);
  }
  if (
    expectedLabel === 2 &&
    (disclosures.length !== intentOnlyPlan.Intents.length ||
      !disclosures.every((value, index) =>
        value === `Intent only: ${intentOnlyPlan.Intents[index]}`,
      ) ||
      updates[0].Projection.Units.length !== 0 ||
      updates[0].Projection.Edges.length !== 0 ||
      updates[0].Projection.Events.length !== 0 ||
      updates[0].Projection.Checkpoints.length !== 0)
  ) {
    throw new Error("Intent-only preview disclosed hidden entity or event state.");
  }
}

const committed = await postSimulator(105, 3, [intentOnlyPlan]);
if (committed.Response?.tag !== 3 || committed.Response.fields[0] !== 0n) {
  throw new Error("Simulator commit did not survive structured cloning.");
}

const stepped = await postSimulator(106, 4, [1]);
if (
  stepped.Response?.tag !== 4 ||
  stepped.Response.fields[0]?.IsSnapshot !== false ||
  stepped.Response.fields[0]?.Projection?.Tick !== 1
) {
  throw new Error("Simulator step did not clone its bounded delta.");
}

const resetAfterStep = await postSimulator(107, 6, [], 1);
if (
  resetAfterStep.Response?.tag !== 7 ||
  resetAfterStep.Response.fields[0]?.Projection?.Tick !== 0
) {
  throw new Error("Simulator reset did not restore its bounded snapshot.");
}

worker.postMessage(
  simulatorEnvelope(
    simulatorCorrelation(110),
    simulatorRequest(5, [6_000]),
  ),
);
const firstRunProgress = await responseFor(110, 5, "first simulator run-to progress");
if (
  firstRunProgress.data?.Response?.tag !== 5 ||
  firstRunProgress.data.Response.fields[0] !== 1 ||
  firstRunProgress.data.Response.fields[1]?.Projection?.Tick !== 256
) {
  throw new Error("Simulator run-to did not emit correlated progress.");
}

worker.postMessage(
  simulatorEnvelope(
    simulatorCorrelation(111, 256),
    simulatorRequest(7, [110]),
  ),
);
const cancellationMessages = [];
let cancelledAtTick = 256;
while (
  cancellationMessages.length < 30 &&
  (!cancellationMessages.some(
    (message) =>
      message.data?.Correlation?.Operation === 111 &&
      message.data?.Response?.tag === 8,
  ) ||
    !cancellationMessages.some(
      (message) =>
        message.data?.Correlation?.Operation === 110 &&
        message.data?.Response?.tag === 8,
    ))
) {
  const message = await nextMessage("simulator cancellation acknowledgement");
  cancellationMessages.push(message);
  if (
    message.data?.Correlation?.Operation === 110 &&
    message.data?.Response?.tag === 5
  ) {
    cancelledAtTick = message.data.Response.fields[1].Projection.Tick;
  }
}
if (
  !cancellationMessages.some(
    (message) =>
      message.data?.Correlation?.Operation === 111 &&
      message.data?.Response?.tag === 8 &&
      message.data.Response.fields[0] === 110,
  ) ||
  !cancellationMessages.some(
    (message) =>
      message.data?.Correlation?.Operation === 110 &&
      message.data?.Response?.tag === 8 &&
      message.data.Response.fields[0] === 110,
  )
) {
  throw new Error("Simulator cancellation did not stop the active run-to operation.");
}

const resetAfterCancellation = await postSimulator(112, 6, [], cancelledAtTick);
if (resetAfterCancellation.Response?.tag !== 7) {
  throw new Error("Simulator reset after cancellation failed.");
}

const authoritativeUpdates = [];
for (let tick = 256; tick < 6_000; tick += 256) {
  authoritativeUpdates.push({
    Tick: tick,
    ServerSequence: BigInt(tick),
    ProjectionRevision: BigInt(tick),
    VisibleUnits: [{ UnitId: 10, DisplayColumn: (tick / 256) % 3, DisplayRow: 1, Health: 100 }],
    StateIdentity: new Uint8Array(32).fill(tick % 251),
    EventIdentity: new Uint8Array(32).fill((tick + 1) % 251),
  });
}
authoritativeUpdates.push({
  Tick: 6_000,
  ServerSequence: 6_000n,
  ProjectionRevision: 6_000n,
  VisibleUnits: [{ UnitId: 10, DisplayColumn: 2, DisplayRow: 1, Health: 100 }],
  StateIdentity: new Uint8Array(32).fill(17),
  EventIdentity: new Uint8Array(32).fill(18),
});
const malformedAuthoritative = structuredClone(authoritativeUpdates);
malformedAuthoritative[0].StateIdentity = new Uint8Array(31);
const malformedAuthoritativeResult = await postSimulator(113, 8, [
  "match-lock-qualified",
  "replay-qualified",
  malformedAuthoritative,
]);
if (
  malformedAuthoritativeResult.Response?.tag !== 9 ||
  malformedAuthoritativeResult.Response.fields[0] !==
    "SIR.SIMULATOR.AUTHORITATIVE_RUN.INVALID"
) {
  throw new Error("Malformed authoritative projection input crossed the worker boundary.");
}

const authoritativeLoaded = await postSimulator(114, 8, [
  "match-lock-qualified",
  "replay-qualified",
  authoritativeUpdates,
]);
if (
  authoritativeLoaded.Response?.tag !== 10 ||
  authoritativeLoaded.Response.fields[0] !== "match-lock-qualified" ||
  authoritativeLoaded.Response.fields[1] !== "replay-qualified" ||
  authoritativeLoaded.Response.fields[2] !== 6_000
) {
  throw new Error("Qualified authoritative projections did not cross the worker boundary.");
}

const runStarted = performance.now();
worker.postMessage(
  simulatorEnvelope(
    simulatorCorrelation(120),
    simulatorRequest(5, [6_000]),
  ),
);
const runMessages = [];
while (true) {
  const message = await nextMessage("authoritative simulator run progress");
  if (message.data?.Correlation?.Operation !== 120) {
    throw new Error("Normal-horizon run emitted an uncorrelated response.");
  }
  runMessages.push(message.data);
  if (message.data?.Response?.tag === 6) break;
}
const runElapsedMilliseconds = performance.now() - runStarted;
if (
  runMessages.length !== 24 ||
  runMessages.filter((message) => message.Response.tag === 5).length !== 23 ||
  runMessages.at(-1)?.Response?.fields[0]?.Projection?.Tick !== 6_000 ||
  runElapsedMilliseconds > 5_000
) {
  throw new Error(
    `Normal horizon exceeded budget: messages=${runMessages.length}, elapsed=${runElapsedMilliseconds.toFixed(3)}ms.`,
  );
}

worker.postMessage(
  simulatorEnvelope(
    simulatorCorrelation(130, 6_000, "map-revision-stale"),
    simulatorRequest(4, [1]),
  ),
);
const staleRejected = (await responseFor(130, 9, "reject stale map revision")).data;
if (staleRejected.Response?.tag !== 9) {
  throw new Error("A stale simulator request was not explicitly rejected.");
}

worker.postMessage(
  simulatorEnvelope(
    simulatorCorrelation(131, 6_000, "map-revision-a", 1n),
    simulatorRequest(4, [1]),
  ),
);
const stalePlanRejected = (await responseFor(131, 9, "reject stale plan revision")).data;
if (
  stalePlanRejected.Response?.tag !== 9 ||
  stalePlanRejected.Response?.fields?.[0] !== "SIR.SIMULATOR.PLAN.STALE"
) {
  throw new Error("A non-committed plan revision advanced the simulator.");
}

worker.postMessage(
  simulatorEnvelope(
    {
      ...simulatorCorrelation(132, 6_000),
      Session: "foreign-session",
    },
    simulatorRequest(7, [120]),
  ),
);
const foreignCancelRejected = (await responseFor(132, 9, "reject foreign cancellation")).data;
if (
  foreignCancelRejected.Response?.tag !== 9 ||
  foreignCancelRejected.Response?.fields?.[0] !==
    "SIR.SIMULATOR.CORRELATION.STALE"
) {
  throw new Error("A foreign simulator workspace could cancel an operation.");
}

console.log(
  `Worker round-trip smoke passed: persistent correlation-checked listener buffered back-to-back replay progress/completion; replay/lab and all simulator session requests/responses crossed structured clone; a pinned authoritative run loaded, cancellation stopped run-to, stale revisions were rejected, intent-only prediction disclosed only its authored intent text and no entity/event state, and 6,000 ticks used ${runMessages.length} projection messages in ${runElapsedMilliseconds.toFixed(3)} ms.`,
);
} finally {
  cleanup();
  await worker.terminate();
}
