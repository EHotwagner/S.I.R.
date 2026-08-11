import { execFileSync } from "node:child_process";
import { readFileSync } from "node:fs";

const exceptionsPath = process.argv[2] ?? ".github/dependency-audit-exceptions.json";
const now = new Date();
let exceptions;

try {
  const document = JSON.parse(readFileSync(exceptionsPath, "utf8"));
  exceptions = document.exceptions;
  if (!Array.isArray(exceptions)) throw new Error("'exceptions' must be an array");
} catch (error) {
  console.error(`dependency-audit policy could not read ${exceptionsPath}: ${error.message}`);
  process.exit(2);
}

const invalid = exceptions.filter(({ id, reason, expires }) =>
  typeof id !== "string" || id.length === 0 ||
  typeof reason !== "string" || reason.length === 0 ||
  typeof expires !== "string" || Number.isNaN(Date.parse(expires)) || new Date(expires) <= now
);
if (invalid.length > 0) {
  console.error(`dependency-audit policy has expired or invalid exceptions: ${invalid.map(({ id }) => id ?? "<missing id>").join(", ")}`);
  process.exit(2);
}

let audit;
let auditCommandFailed = false;
try {
  audit = JSON.parse(execFileSync("npm", ["audit", "--json"], { encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] }));
} catch (error) {
  auditCommandFailed = true;
  try {
    audit = JSON.parse(error.stdout);
  } catch {
    console.error("dependency-audit policy could not parse npm audit output");
    process.exit(2);
  }
}

const isObject = (value) => value !== null && typeof value === "object" && !Array.isArray(value);
const vulnerabilityCounts = audit?.metadata?.vulnerabilities;
const validAuditReport =
  isObject(audit) &&
  Number.isInteger(audit.auditReportVersion) && audit.auditReportVersion > 0 &&
  isObject(audit.vulnerabilities) &&
  isObject(audit.metadata) &&
  isObject(vulnerabilityCounts) &&
  ["high", "critical"].every((severity) => Number.isInteger(vulnerabilityCounts[severity]) && vulnerabilityCounts[severity] >= 0) &&
  !("error" in audit);

if (!validAuditReport) {
  console.error("dependency-audit policy received an invalid npm audit report");
  process.exit(2);
}

if (auditCommandFailed && Object.keys(audit.vulnerabilities).length === 0) {
  console.error("dependency-audit policy received a nonzero npm audit result without actionable advisories");
  process.exit(2);
}

const allowed = new Set(exceptions.map(({ id }) => id));
const actionable = Object.values(audit.vulnerabilities).filter((vulnerability) =>
  ["high", "critical"].includes(vulnerability.severity) && !allowed.has(vulnerability.name)
);

if (actionable.length > 0) {
  console.error(`dependency-audit policy found actionable high/critical advisories: ${actionable.map(({ name, severity }) => `${name} (${severity})`).join(", ")}`);
  process.exit(1);
}

console.log("dependency-audit policy: no actionable high/critical advisories");
