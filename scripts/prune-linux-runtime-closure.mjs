import { lstat, readdir, rm, writeFile, mkdir } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { pathToFileURL } from "node:url";

export const schema = "sir.ci-linux-runtime-closure/v1";
const allowedRoots = ["tests/SIR.Match.Tests/bin/Release/net10.0", "artifacts/publish"];

async function bytesUnder(path) {
  const information = await lstat(path);
  if (information.isSymbolicLink()) throw new Error(`linux-runtime-closure: symbolic-entry:${path}`);
  if (information.isFile()) return information.size;
  if (!information.isDirectory()) throw new Error(`linux-runtime-closure: unsupported-entry:${path}`);
  let total = 0;
  for (const entry of await readdir(path)) total += await bytesUnder(resolve(path, entry));
  return total;
}

async function main(argv) {
  const rootIndex = argv.indexOf("--root");
  const outputIndex = argv.indexOf("--output");
  if (rootIndex < 0 || outputIndex < 0 || !argv[rootIndex + 1] || !argv[outputIndex + 1]) throw new Error("linux-runtime-closure: usage --root <path> --output <path>");
  const declaredRoot = argv[rootIndex + 1];
  const relativeRoot = declaredRoot.replaceAll("\\", "/").replace(/^\.\//u, "").replace(/\/$/u, "");
  if (!allowedRoots.includes(relativeRoot) && process.env.SIR_RUNTIME_CLOSURE_FIXTURE !== "true") throw new Error(`linux-runtime-closure: unsupported-root:${relativeRoot}`);
  const root = resolve(declaredRoot);
  const runtimes = resolve(root, "runtimes");
  const entries = await readdir(runtimes, { withFileTypes: true });
  if (entries.some((entry) => !entry.isDirectory() || entry.isSymbolicLink())) throw new Error("linux-runtime-closure: malformed-runtime-inventory");
  if (!entries.some((entry) => entry.name === "linux-x64")) throw new Error("linux-runtime-closure: linux-x64-runtime-missing");
  const beforeBytes = await bytesUnder(runtimes);
  const removed = [];
  for (const entry of entries.sort((left, right) => left.name.localeCompare(right.name))) {
    if (entry.name === "linux-x64") continue;
    const path = resolve(runtimes, entry.name);
    removed.push({ runtimeIdentifier: entry.name, bytes: await bytesUnder(path) });
    await rm(path, { recursive: true });
  }
  const remaining = (await readdir(runtimes)).sort();
  if (remaining.length !== 1 || remaining[0] !== "linux-x64") throw new Error("linux-runtime-closure: target-closure-drift");
  const afterBytes = await bytesUnder(runtimes);
  const report = { schema, result: "pass", targetRuntimeIdentifier: "linux-x64", root: relativeRoot, removed, beforeBytes, afterBytes, savedBytes: beforeBytes - afterBytes };
  await mkdir(dirname(resolve(argv[outputIndex + 1])), { recursive: true });
  await writeFile(resolve(argv[outputIndex + 1]), `${JSON.stringify(report, null, 2)}\n`);
  console.log(JSON.stringify(report));
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) main(process.argv.slice(2)).catch((error) => { console.error(error.message); process.exitCode = 1; });
