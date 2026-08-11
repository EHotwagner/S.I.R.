import { existsSync, rmSync } from "node:fs";
import { spawnSync } from "node:child_process";

// #154 owns this focused receipt.  The full-browser command deliberately
// continues to write browser.junit.xml, so its broader inventory cannot alter
// the exact observed-run bytes bound by this work package.
const outputFile = "artifacts/test-results/production-delivery.junit.xml";
rmSync(outputFile, { force: true });
const result = spawnSync(
  "npx",
  [
    "playwright",
    "test",
    "tests/SIR.Browser.Tests/production-delivery.spec.js",
    "--config",
    "tests/SIR.Browser.Tests/playwright.config.js",
  ],
  {
    cwd: process.cwd(),
    env: { ...process.env, SIR_JUNIT_OUTPUT: outputFile },
    encoding: "utf8",
  },
);

if (result.status !== 0 || !existsSync(outputFile)) {
  process.stderr.write(`${result.stdout}\n${result.stderr}`);
  throw new Error("production-delivery evidence run did not produce its dedicated JUnit receipt");
}

console.log(`production delivery evidence: ${outputFile}`);
