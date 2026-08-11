import { spawnSync } from "node:child_process";
import { existsSync } from "node:fs";

const canonicalJunit = "artifacts/test-results/browser.junit.xml";
const diagnosticJunit = "artifacts/test-results/browser-diagnostics-child.junit.xml";

// This is intentionally separate from product/FR evidence. It proves that the
// browser diagnostic fixture rejects an unallowlisted page-originated 418.
const result = spawnSync(
  "npx",
  ["playwright", "test", "tests/SIR.Browser.Tests/visible-workflows.spec.js", "--config", "tests/SIR.Browser.Tests/playwright.config.js", "--grep", "unexpected rejection diagnostics"],
  {
    cwd: process.cwd(),
    env: {
      ...process.env,
      SIR_DIAGNOSTIC_SELF_TEST: "1",
      // The child is expected to fail. It must never replace the SDD-owned
      // canonical all-browser receipt used by evidence --sync-observed-run.
      SIR_JUNIT_OUTPUT: diagnosticJunit,
    },
    encoding: "utf8",
  },
);
const output = `${result.stdout}\n${result.stderr}`;
if (result.status === 0 || !output.includes("unexpected console, page, or network diagnostics") || !existsSync(diagnosticJunit)) {
  process.stderr.write(output);
  throw new Error("diagnostic gate self-test did not reject the controlled unallowlisted 418");
}
console.log("browser diagnostics gate: controlled 418 rejected");
