import { existsSync, readFileSync, rmSync } from "node:fs";
import { createHash } from "node:crypto";
import { spawnSync } from "node:child_process";

// #154 owns this focused receipt.  The full-browser command deliberately
// continues to write browser.junit.xml, so its broader inventory cannot alter
// the exact observed-run bytes bound by this work package.
const outputFile = "artifacts/test-results/production-delivery.junit.xml";
const canonicalOutput = "artifacts/test-results/browser.junit.xml";
const receiptDigest = () => createHash("sha256").update(readFileSync(outputFile)).digest("hex");
const run = (args, env) => spawnSync("npx", args, { cwd: process.cwd(), env, encoding: "utf8" });

rmSync(outputFile, { force: true });
const result = run([
  "playwright",
  "test",
  "tests/SIR.Browser.Tests/production-delivery.spec.js",
  "--config",
  "tests/SIR.Browser.Tests/playwright.config.js",
], { ...process.env, SIR_JUNIT_OUTPUT: outputFile });

if (result.status !== 0 || !existsSync(outputFile)) {
  process.stderr.write(`${result.stdout}\n${result.stderr}`);
  throw new Error("production-delivery evidence run did not produce its dedicated JUnit receipt");
}

const focusedDigest = receiptDigest();
const { SIR_JUNIT_OUTPUT: ignoredJUnitOverride, ...completeSuiteEnvironment } = process.env;
const completeSuite = run([
  "playwright",
  "test",
  "--config",
  "tests/SIR.Browser.Tests/playwright.config.js",
], completeSuiteEnvironment);

if (completeSuite.status !== 0) {
  process.stderr.write(`${completeSuite.stdout}\n${completeSuite.stderr}`);
  throw new Error("complete browser suite did not pass after the focused #154 evidence run");
}

const completeSuiteDigest = receiptDigest();
if (focusedDigest !== completeSuiteDigest || !existsSync(canonicalOutput)) {
  throw new Error(`complete browser suite changed the dedicated #154 JUnit receipt: ${focusedDigest} -> ${completeSuiteDigest}`);
}

console.log(`production delivery evidence: ${outputFile} remained ${focusedDigest} after the full browser suite`);
