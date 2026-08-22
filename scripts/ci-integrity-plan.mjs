import { createHash } from "node:crypto";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { pathToFileURL } from "node:url";

export const schema = "sir.ci-integrity-plan/v1";
export const subjectOrder = ["npm-audit", "governance", "dependency-surface", "sdd-byte-stability", "feedback-audit"];
const canonical = (value) => `${JSON.stringify(value, null, 2)}\n`;
const sha256 = (value) => createHash("sha256").update(value).digest("hex");
const under = (path, prefix) => path === prefix || path.startsWith(`${prefix}/`);

const predicates = {
  "npm-audit": (path) => ["package.json", "package-lock.json"].includes(path) || under(path, ".github/scripts") || under(path, ".github/workflows"),
  governance: (path) => under(path, ".fsgg") || /governance/iu.test(path) || ["Directory.Packages.props", "global.json"].includes(path),
  "dependency-surface": (path) => under(path, "docs/dependency-surface") || path.endsWith(".fsproj") || path.endsWith("packages.lock.json") || ["Directory.Packages.props", ".config/dotnet-tools.json"].includes(path),
  "sdd-byte-stability": (path) => under(path, "work/184-scenario-catalog") || under(path, "readiness/184-scenario-catalog") || under(path, ".fsgg") || path === ".config/dotnet-tools.json",
  "feedback-audit": (path) => under(path, "feedback") || under(path, ".github/workflows") || ["scripts/audit-binding-exceptions.json", "scripts/test-feedback-audit-binding-exceptions.sh", "scripts/ci-route.mjs", "scripts/test-ci-route.mjs"].includes(path),
};

export function planFor(route) {
  if (route?.schema !== "sir.ci-route/v2" || !Array.isArray(route.paths) || route.paths.length === 0 || !route.digest) throw new Error("ci-integrity-plan: malformed route");
  const selfChange = route.paths.some((path) => ["scripts/ci-integrity-plan.mjs", "scripts/test-ci-integrity-plan.mjs", "scripts/qualify-pr.sh"].includes(path));
  const topologyChange = route.paths.some((path) => under(path, ".github/workflows"));
  const unknown = route.facts?.some(({ rule }) => rule === "RP-005-unknown-conservative") ?? true;
  const subjects = subjectOrder.map((id) => {
    const matchingPaths = route.paths.filter((path) => predicates[id](path));
    const run = selfChange || topologyChange || unknown || matchingPaths.length > 0;
    return { id, run, reason: selfChange ? "classifier-self-change" : topologyChange ? "topology-change" : unknown ? "unknown-conservative" : run ? "relevant-path" : "measured-omission", matchingPaths };
  });
  const body = { schema, routeDigest: route.digest, source: route.source, alwaysOn: ["ci-contract-floor"], subjects };
  return { ...body, digest: sha256(canonical(body)) };
}

async function main(argv) {
  const routeIndex = argv.indexOf("--route");
  const outputIndex = argv.indexOf("--output");
  if (routeIndex < 0 || outputIndex < 0) throw new Error("ci-integrity-plan: usage --route <path> --output <path>");
  const route = JSON.parse(await readFile(resolve(argv[routeIndex + 1]), "utf8"));
  const plan = planFor(route);
  await mkdir(dirname(resolve(argv[outputIndex + 1])), { recursive: true });
  await writeFile(resolve(argv[outputIndex + 1]), canonical(plan));
  console.log(canonical(plan).trim());
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) main(process.argv.slice(2)).catch((error) => { console.error(error.message); process.exitCode = 1; });
