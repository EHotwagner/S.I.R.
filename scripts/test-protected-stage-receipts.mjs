import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { execFileSync, spawnSync } from "node:child_process";
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { resolve } from "node:path";
import { gateResult, joinRoute, routePaths } from "./ci-route.mjs";

const root = resolve(import.meta.dirname, "..");
const temporary = mkdtempSync(resolve(tmpdir(), "sir-protected-stage-"));
const receiptTool = resolve(root, "scripts/protected-stage-receipt.mjs");
const runner = resolve(root, "scripts/run-protected-stage.sh");
const workflow = readFileSync(resolve(root, ".github/workflows/ci.yml"), "utf8");
const run = (program, args) => execFileSync(program, args, { cwd: root, encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] });
const canonical = (value) => `${JSON.stringify(value, null, 2)}\n`;
const digest = (value) => createHash("sha256").update(value).digest("hex");

try {
  const preflight = resolve(temporary, "preflight.json");
  const core = resolve(temporary, "core.json");
  const joined = resolve(temporary, "joined.json");
  run("bash", [runner, "preflight", preflight, "--", "bash", "-c", "true"]);
  run("bash", [runner, "core", core, "--", "bash", "-c", "true"]);
  run(process.execPath, [receiptTool, "join", "--receipt", `preflight=${preflight}`, "--receipt", `core=${core}`, "--output", joined]);
  const verdict = JSON.parse(readFileSync(joined, "utf8"));
  assert.equal(verdict.schema, "sir.protected-join/v2");
  assert.equal(verdict.mode, "complete");
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

  const source = {
    commit: run("git", ["rev-parse", "HEAD"]).trim(),
    tree: run("git", ["rev-parse", "HEAD^{tree}"]).trim(),
  };
  const route = routePaths(["feedback/example.md"], source);
  const routePath = resolve(temporary, "route.json");
  writeFileSync(routePath, `${JSON.stringify(route, null, 2)}\n`);
  const resultFor = (gate) => gateResult(gate, "pass", { total: 1 }, {
    source,
    routeDigest: route.digest,
  });
  const routed = joinRoute(route, [resultFor("integrity"), resultFor("evidence")], {
    startedAtMilliseconds: 1_900_000_000_000,
    completedAtMilliseconds: 1_900_000_000_002,
  });
  assert.equal(routed.result, "pass");
  const routedPath = resolve(temporary, "routed.json");
  writeFileSync(routedPath, `${JSON.stringify(routed, null, 2)}\n`);
  run(process.execPath, [resolve(root, "scripts/ci-route.mjs"), "verify-join", "--route", routePath, "--join", routedPath, "--commit", source.commit, "--tree", source.tree]);
  const invalidTimingPath = resolve(temporary, "invalid-timing-routed.json");
  writeFileSync(invalidTimingPath, `${JSON.stringify({ ...routed, timing: { ...routed.timing, totalMilliseconds: 3 } }, null, 2)}\n`);
  const invalidTiming = spawnSync(process.execPath, [resolve(root, "scripts/ci-route.mjs"), "verify-join", "--route", routePath, "--join", invalidTimingPath, "--commit", source.commit, "--tree", source.tree], { cwd: root, encoding: "utf8" });
  assert.equal(invalidTiming.status, 1);
  assert.match(invalidTiming.stderr, /routed-join-timing-invalid/u);
  const focusedPath = resolve(temporary, "focused.json");
  run(process.execPath, [receiptTool, "join-focused", "--route", routePath, "--routed", routedPath, "--output", focusedPath]);
  const focused = JSON.parse(readFileSync(focusedPath, "utf8"));
  assert.equal(focused.schema, "sir.protected-join/v2");
  assert.equal(focused.mode, "focused");
  assert.equal(focused.result, "pass");

  const documentationRoute = routePaths(["docs/example.md"], source);
  const documentationRoutePath = resolve(temporary, "documentation-route.json");
  writeFileSync(documentationRoutePath, canonical(documentationRoute));
  const documentationRouted = {
    ...routed,
    routeDigest: documentationRoute.digest,
    classification: documentationRoute.classification,
    selectedGates: documentationRoute.selectedGates,
  };
  const documentationRoutedPath = resolve(temporary, "documentation-routed.json");
  writeFileSync(documentationRoutedPath, canonical(documentationRouted));
  const handoffBody = {
    schema: "sir.qualified-site-handoff/v1",
    result: "pass",
    source,
    routeDigest: documentationRoute.digest,
    documentationGateSha256: "a".repeat(64),
    siteReceiptSha256: "b".repeat(64),
    archive: { name: "protected-qualified-site.tar", sha256: "c".repeat(64) },
  };
  const handoff = { ...handoffBody, digest: digest(canonical(handoffBody)) };
  const handoffPath = resolve(temporary, "site-handoff.json");
  writeFileSync(handoffPath, canonical(handoff));
  const documentationFocusedPath = resolve(temporary, "documentation-focused.json");
  run(process.execPath, [receiptTool, "join-focused", "--route", documentationRoutePath, "--routed", documentationRoutedPath, "--site-handoff", handoffPath, "--site-handoff-status", "success", "--output", documentationFocusedPath]);
  assert.equal(JSON.parse(readFileSync(documentationFocusedPath, "utf8")).result, "pass");
  const malformedHandoffBody = { ...handoffBody };
  delete malformedHandoffBody.documentationGateSha256;
  delete malformedHandoffBody.siteReceiptSha256;
  writeFileSync(handoffPath, canonical({ ...malformedHandoffBody, digest: digest(canonical(malformedHandoffBody)) }));
  const malformedHandoff = spawnSync(process.execPath, [receiptTool, "join-focused", "--route", documentationRoutePath, "--routed", documentationRoutedPath, "--site-handoff", handoffPath, "--site-handoff-status", "success", "--output", resolve(temporary, "malformed-handoff-focused.json")], { cwd: root, encoding: "utf8" });
  assert.equal(malformedHandoff.status, 1);
  assert.match(malformedHandoff.stdout, /site-handoff-malformed-bindings/u);
  writeFileSync(handoffPath, canonical(handoff));
  const missingHandoff = spawnSync(process.execPath, [receiptTool, "join-focused", "--route", documentationRoutePath, "--routed", documentationRoutedPath, "--site-handoff", resolve(temporary, "missing-handoff.json"), "--site-handoff-status", "failure", "--output", resolve(temporary, "missing-handoff-focused.json")], { cwd: root, encoding: "utf8" });
  assert.equal(missingHandoff.status, 1);
  assert.match(missingHandoff.stdout, /site-handoff-step-failed/u);
  assert.match(missingHandoff.stdout, /site-handoff-missing-or-unreadable/u);
  writeFileSync(handoffPath, canonical({ ...handoff, routeDigest: "0".repeat(64) }));
  const staleHandoff = spawnSync(process.execPath, [receiptTool, "join-focused", "--route", documentationRoutePath, "--routed", documentationRoutedPath, "--site-handoff", handoffPath, "--site-handoff-status", "success", "--output", resolve(temporary, "stale-handoff-focused.json")], { cwd: root, encoding: "utf8" });
  assert.equal(staleHandoff.status, 1);
  assert.match(staleHandoff.stdout, /site-handoff-digest-mismatch/u);
  assert.match(staleHandoff.stdout, /site-handoff-route-mismatch/u);

  const cancelledRouted = { ...routed, result: "cancelled" };
  const cancelledPath = resolve(temporary, "cancelled-routed.json");
  writeFileSync(cancelledPath, `${JSON.stringify(cancelledRouted, null, 2)}\n`);
  const cancelledJoin = spawnSync(process.execPath, [receiptTool, "join-focused", "--route", routePath, "--routed", cancelledPath, "--output", resolve(temporary, "cancelled-focused.json")], { cwd: root, encoding: "utf8" });
  assert.equal(cancelledJoin.status, 1);
  assert.match(cancelledJoin.stdout, /routed-verdict-cancelled/u);

  const mismatchedRouted = { ...routed, routeDigest: "0".repeat(64) };
  const mismatchedPath = resolve(temporary, "mismatched-routed.json");
  writeFileSync(mismatchedPath, `${JSON.stringify(mismatchedRouted, null, 2)}\n`);
  const mismatchedJoin = spawnSync(process.execPath, [receiptTool, "join-focused", "--route", routePath, "--routed", mismatchedPath, "--output", resolve(temporary, "mismatched-focused.json")], { cwd: root, encoding: "utf8" });
  assert.equal(mismatchedJoin.status, 1);
  assert.match(mismatchedJoin.stdout, /routed-route-mismatch/u);

  assert.match(workflow, /^  protected-preflight:$/mu);
  assert.match(workflow, /^  protected-preflight:\n    if: github\.event_name == 'schedule' \|\| github\.event_name == 'workflow_dispatch'/mu);
  assert.match(workflow, /^  full-qualification:\n    if: github\.event_name == 'schedule' \|\| github\.event_name == 'workflow_dispatch'[\s\S]*?needs: protected-preflight/mu);
  assert.match(workflow, /^  protected-verdict:\n    if: always\(\)/mu);
  assert.match(workflow, /verify-join[\s\S]*join-focused/u);
  assert.match(workflow, /qualified-site-handoff\.mjs verify[\s\S]*--gate "\$extraction\/artifacts\/ci\/results\/documentation\.json"[\s\S]*--site-receipt "\$extraction\/\$site_receipt_path"/u);
  assert.match(workflow, /pattern: protected-stage-\*/u);
  assert.match(workflow, /name: protected-qualified-site/u);
  console.log("Protected complete and focused receipts, partial/cancelled failure diagnostics, exact route binding, missing-stage rejection, and event topology passed.");
} finally {
  rmSync(temporary, { recursive: true, force: true });
}
