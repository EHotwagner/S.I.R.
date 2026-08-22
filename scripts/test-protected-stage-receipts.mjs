import assert from "node:assert/strict";
import { execFileSync, spawnSync } from "node:child_process";
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { resolve } from "node:path";

const root = resolve(import.meta.dirname, "..");
const temporary = mkdtempSync(resolve(tmpdir(), "sir-protected-stage-"));
const receiptTool = resolve(root, "scripts/protected-stage-receipt.mjs");
const runner = resolve(root, "scripts/run-protected-stage.sh");
const workflow = readFileSync(resolve(root, ".github/workflows/ci.yml"), "utf8");
const run = (program, args) => execFileSync(program, args, { cwd: root, encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] });

try {
  const preflight = resolve(temporary, "preflight.json");
  const core = resolve(temporary, "core.json");
  const joined = resolve(temporary, "joined.json");
  run("bash", [runner, "preflight", preflight, "--", "bash", "-c", "true"]);
  run("bash", [runner, "core", core, "--", "bash", "-c", "true"]);
  run(process.execPath, [receiptTool, "join", "--receipt", `preflight=${preflight}`, "--receipt", `core=${core}`, "--output", joined]);
  const verdict = JSON.parse(readFileSync(joined, "utf8"));
  assert.equal(verdict.schema, "sir.protected-join/v1");
  assert.equal(verdict.result, "pass");
  assert.deepEqual(verdict.stages.map(({ stage }) => stage), ["preflight", "core"]);
  assert.ok(verdict.stages.every(({ status, source, digest }) => status === "pass" && /^[0-9a-f]{40}$/u.test(source.commit) && /^[0-9a-f]{64}$/u.test(digest)));

  const failedCore = resolve(temporary, "failed-core.json");
  const failedRun = spawnSync("bash", [runner, "core", failedCore, "--", "bash", "-c", "exit 7"], { cwd: root, encoding: "utf8" });
  assert.equal(failedRun.status, 7);
  const failedJoin = spawnSync(process.execPath, [receiptTool, "join", "--receipt", `preflight=${preflight}`, "--receipt", `core=${failedCore}`, "--output", resolve(temporary, "failed-join.json")], { cwd: root, encoding: "utf8" });
  assert.equal(failedJoin.status, 1);
  assert.match(failedJoin.stdout, /core-fail/u);

  const tampered = JSON.parse(readFileSync(preflight, "utf8"));
  tampered.timingMilliseconds.total += 1;
  writeFileSync(preflight, `${JSON.stringify(tampered, null, 2)}\n`);
  const tamperedJoin = spawnSync(process.execPath, [receiptTool, "join", "--receipt", `preflight=${preflight}`, "--receipt", `core=${core}`, "--output", resolve(temporary, "tampered-join.json")], { cwd: root, encoding: "utf8" });
  assert.equal(tamperedJoin.status, 1);
  assert.match(tamperedJoin.stdout, /preflight-digest-mismatch/u);

  const missingJoin = spawnSync(process.execPath, [receiptTool, "join", "--receipt", `core=${core}`, "--output", resolve(temporary, "missing-join.json")], { cwd: root, encoding: "utf8" });
  assert.equal(missingJoin.status, 1);
  assert.match(missingJoin.stdout, /missing-stage/u);

  assert.match(workflow, /^  protected-preflight:$/mu);
  assert.match(workflow, /^  full-qualification:\n    if:[\s\S]*?needs: protected-preflight/mu);
  assert.match(workflow, /^  protected-verdict:\n    if: always\(\)/mu);
  assert.match(workflow, /pattern: protected-stage-\*/u);
  assert.match(workflow, /name: protected-qualified-site/u);
  console.log("Protected preflight/core receipts, partial failure diagnostics, digest binding, missing-stage rejection, and workflow join topology passed.");
} finally {
  rmSync(temporary, { recursive: true, force: true });
}
