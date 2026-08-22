import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdirSync, mkdtempSync, readFileSync, rmSync, symlinkSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { resolve } from "node:path";

const root = mkdtempSync(resolve(tmpdir(), "sir-runtime-closure-"));
const script = resolve(import.meta.dirname, "prune-linux-runtime-closure.mjs");
const run = (fixture) => spawnSync(process.execPath, [script, "--root", fixture, "--output", resolve(fixture, "report.json")], { cwd: "/", env: { ...process.env, SIR_RUNTIME_CLOSURE_FIXTURE: "true" }, encoding: "utf8" });
try {
  const fixture = resolve(root, "output");
  for (const rid of ["linux-x64", "linux-arm64", "osx-x64", "win-x64"]) {
    mkdirSync(resolve(fixture, "runtimes", rid), { recursive: true });
    writeFileSync(resolve(fixture, "runtimes", rid, "runtime.bin"), rid.repeat(10));
  }
  const result = run(fixture);
  assert.equal(result.status, 0, result.stderr);
  const report = JSON.parse(readFileSync(resolve(fixture, "report.json"), "utf8"));
  assert.equal(report.result, "pass");
  assert.deepEqual(report.removed.map(({ runtimeIdentifier }) => runtimeIdentifier), ["linux-arm64", "osx-x64", "win-x64"]);
  assert.ok(report.savedBytes > 0 && report.beforeBytes > report.afterBytes);
  assert.equal(readFileSync(resolve(fixture, "runtimes", "linux-x64", "runtime.bin"), "utf8"), "linux-x64".repeat(10));

  const missing = resolve(root, "missing");
  mkdirSync(resolve(missing, "runtimes", "win-x64"), { recursive: true });
  assert.notEqual(run(missing).status, 0);
  const symbolic = resolve(root, "symbolic");
  mkdirSync(resolve(symbolic, "runtimes", "linux-x64"), { recursive: true });
  symlinkSync(resolve(symbolic, "runtimes", "linux-x64"), resolve(symbolic, "runtimes", "win-x64"), "dir");
  assert.notEqual(run(symbolic).status, 0);
  console.log("Linux-x64 acceleration closure removes only non-target runtime directories and fails closed on missing or symbolic runtime inventories.");
} finally {
  rmSync(root, { recursive: true, force: true });
}
