import { readFile } from "node:fs/promises";
import { createHash } from "node:crypto";

if (process.argv.length !== 3) {
  throw new Error("Pass the shared artifact.b64 path.");
}

const artifact = Buffer.from((await readFile(process.argv[2], "utf8")).trim(), "base64");
const hostCalls = [];
const imports = {
  sir: {
    bias(value) {
      hostCalls.push(value);
      return value * 2;
    },
  },
};

const module = await WebAssembly.compile(artifact);
const instance = await WebAssembly.instantiate(module, imports);
const decisions = [3, 3, -2].map((tick) => instance.exports.decide(tick));

let explicitTrap = false;
try {
  instance.exports.trap();
} catch (error) {
  explicitTrap = error instanceof WebAssembly.RuntimeError;
}

const freshInstance = await WebAssembly.instantiate(module, {
  sir: { bias: (value) => value * 2 },
});

// The browser WebAssembly API exposes modules, instances, memories, tables,
// globals and functions. It exposes no Store, fuel allowance, consumed-fuel
// counter, or deterministic out-of-fuel trap. A Worker can be terminated by
// elapsed host time, but that is not replayable instruction metering.
const fuelMetering =
  "Store" in WebAssembly ||
  "Fuel" in WebAssembly ||
  "setFuel" in WebAssembly.Instance.prototype;

const result = {
  runtime: `javascript-webassembly-${process.version}`,
  artifactSha256: createHash("sha256").update(artifact).digest("hex"),
  decisions,
  hostCalls,
  finalCounter: instance.exports.counter(),
  freshInstanceCounter: freshInstance.exports.counter(),
  explicitTrap,
  fuelMetering,
  infiniteLoopFuelTrap: false,
};

console.log(JSON.stringify(result));
