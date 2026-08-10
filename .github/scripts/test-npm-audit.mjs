import assert from "node:assert/strict";
import { chmodSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { spawnSync } from "node:child_process";

const repoRoot = new URL("../..", import.meta.url).pathname;
const checker = join(repoRoot, ".github/scripts/check-npm-audit.mjs");
const validReport = (vulnerabilities = {}) => JSON.stringify({
  auditReportVersion: 2,
  vulnerabilities,
  metadata: { vulnerabilities: { high: 0, critical: 0 } }
});

const cases = [
  ["clean report", validReport(), 0],
  ["valid advisory report from nonzero npm", validReport({ vulnerable: { name: "vulnerable", severity: "high" } }), 1],
  ["operational E403 report", JSON.stringify({ error: { code: "E403" } }), 2],
  ["missing vulnerabilities", JSON.stringify({ auditReportVersion: 2, metadata: { vulnerabilities: { high: 0, critical: 0 } } }), 2],
  ["invalid metadata", JSON.stringify({ auditReportVersion: 2, vulnerabilities: {}, metadata: { vulnerabilities: { high: "0", critical: 0 } } }), 2],
  ["invalid schema", JSON.stringify({ auditReportVersion: 0, vulnerabilities: {}, metadata: { vulnerabilities: { high: 0, critical: 0 } } }), 2],
  ["malformed JSON", "{", 2]
];

const temp = mkdtempSync(join(tmpdir(), "sir-npm-audit-"));
try {
  const npm = join(temp, "npm");
  for (const [name, stdout, expectedStatus] of cases) {
    writeFileSync(npm, `#!/usr/bin/env sh\nprintf '%s' "$NPM_AUDIT_FIXTURE"\nexit "$NPM_AUDIT_EXIT"\n`);
    chmodSync(npm, 0o755);
    const result = spawnSync(process.execPath, [checker], {
      cwd: repoRoot,
      encoding: "utf8",
      env: { ...process.env, PATH: `${temp}:${process.env.PATH}`, NPM_AUDIT_FIXTURE: stdout, NPM_AUDIT_EXIT: expectedStatus === 1 ? "1" : expectedStatus === 2 && name === "operational E403 report" ? "1" : "0" }
    });
    assert.equal(result.status, expectedStatus, `${name}: ${result.stdout}${result.stderr}`);
  }
  console.log(`npm-audit policy regressions passed: ${cases.length}`);
} finally {
  rmSync(temp, { recursive: true, force: true });
}
