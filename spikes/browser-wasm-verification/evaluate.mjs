import { readFile } from "node:fs/promises";

if (process.argv.length !== 4) {
  throw new Error("Pass the Wasmtime and browser-host result paths.");
}

const authoritative = JSON.parse(await readFile(process.argv[2], "utf8"));
const browser = JSON.parse(await readFile(process.argv[3], "utf8"));
const exactFields = [
  "artifactSha256",
  "decisions",
  "hostCalls",
  "finalCounter",
  "freshInstanceCounter",
  "explicitTrap",
];

for (const field of exactFields) {
  if (JSON.stringify(authoritative[field]) !== JSON.stringify(browser[field])) {
    throw new Error(
      `Browser/Wasmtime mismatch at ${field}: ` +
        `${JSON.stringify(authoritative[field])} != ${JSON.stringify(browser[field])}`,
    );
  }
}

if (!authoritative.fuelMetering || !authoritative.infiniteLoopFuelTrap) {
  throw new Error("The Wasmtime oracle did not enforce deterministic fuel.");
}

if (browser.fuelMetering || browser.infiniteLoopFuelTrap) {
  throw new Error(
    "The native browser contract unexpectedly gained deterministic fuel; " +
      "re-open M14 and qualify its exact semantics.",
  );
}

console.log(
  "M14 result: common ABI/state/host-call/trap vectors match; " +
    "native browser WebAssembly fails deterministic fuel qualification.",
);
