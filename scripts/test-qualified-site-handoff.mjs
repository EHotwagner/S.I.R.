import assert from "node:assert/strict";
import { execFileSync, spawnSync } from "node:child_process";
import { mkdtempSync, readFileSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { resolve } from "node:path";
import { gateResult, routePaths } from "./ci-route.mjs";

const root = resolve(import.meta.dirname, "..");
const temporary = mkdtempSync(resolve(tmpdir(), "sir-site-handoff-"));
const tool = resolve(root, "scripts/qualified-site-handoff.mjs");
const run = (args) => execFileSync(process.execPath, [tool, ...args], { cwd: root, encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] });
const canonical = (value) => `${JSON.stringify(value, null, 2)}\n`;

try {
  const commit = execFileSync("git", ["rev-parse", "HEAD"], { cwd: root, encoding: "utf8" }).trim();
  const tree = execFileSync("git", ["rev-parse", "HEAD^{tree}"], { cwd: root, encoding: "utf8" }).trim();
  const source = { commit, tree };
  const route = routePaths(["docs/example.md"], source);
  const routePath = resolve(temporary, "route.json");
  const gatePath = resolve(temporary, "documentation.json");
  const siteReceiptPath = resolve(temporary, "site-receipt.json");
  const archivePath = resolve(temporary, "protected-qualified-site.tar");
  const handoffPath = resolve(temporary, "site-handoff.json");
  writeFileSync(routePath, canonical(route));
  writeFileSync(gatePath, canonical(gateResult("documentation", "pass", { total: 1 }, { source, routeDigest: route.digest })));
  writeFileSync(siteReceiptPath, canonical({ schema: "sir.production-build-receipt/v1", source, outputs: [] }));
  writeFileSync(archivePath, "qualified-site-archive");

  run(["create", "--route", routePath, "--gate", gatePath, "--site-receipt", siteReceiptPath, "--archive", archivePath, "--output", handoffPath]);
  run(["verify", "--handoff", handoffPath, "--route", routePath, "--gate", gatePath, "--site-receipt", siteReceiptPath, "--archive", archivePath, "--commit", commit, "--tree", tree]);
  const handoff = JSON.parse(readFileSync(handoffPath, "utf8"));
  assert.equal(handoff.schema, "sir.qualified-site-handoff/v1");
  assert.equal(handoff.routeDigest, route.digest);

  writeFileSync(archivePath, "tampered-archive");
  const tamperedArchive = spawnSync(process.execPath, [tool, "verify", "--handoff", handoffPath, "--route", routePath, "--archive", archivePath, "--commit", commit, "--tree", tree], { cwd: root, encoding: "utf8" });
  assert.equal(tamperedArchive.status, 1);
  assert.match(tamperedArchive.stderr, /archive-mismatch/u);

  writeFileSync(archivePath, "qualified-site-archive");
  writeFileSync(handoffPath, canonical({ ...handoff, routeDigest: "0".repeat(64) }));
  const tamperedReceipt = spawnSync(process.execPath, [tool, "verify", "--handoff", handoffPath, "--route", routePath, "--archive", archivePath, "--commit", commit, "--tree", tree], { cwd: root, encoding: "utf8" });
  assert.equal(tamperedReceipt.status, 1);
  assert.match(tamperedReceipt.stderr, /digest-mismatch,route-mismatch/u);

  console.log("Qualified site handoff binds exact source, route, documentation gate, final-site receipt, and archive; tampering fails closed.");
} finally {
  execFileSync("rm", ["-rf", temporary]);
}
