import { createHash } from "node:crypto";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { pathToFileURL } from "node:url";

export const schema = "sir.ci-integrity-plan/v1";
export const subjectOrder = ["npm-audit", "governance", "dependency-surface", "sdd-byte-stability", "feedback-audit", "review-contract"];
export const planModes = ["pull-request", "sweep"];
export const sweepEnvironmentVariable = "SIR_CI_INTEGRITY_SWEEP";
const canonical = (value) => `${JSON.stringify(value, null, 2)}\n`;
const sha256 = (value) => createHash("sha256").update(value).digest("hex");
const under = (path, prefix) => path === prefix || path.startsWith(`${prefix}/`);

const predicates = {
  "npm-audit": (path) => ["package.json", "package-lock.json"].includes(path) || under(path, ".github/scripts") || under(path, ".github/workflows"),
  governance: (path) => under(path, ".fsgg") || /governance/iu.test(path) || ["Directory.Packages.props", "global.json"].includes(path),
  "dependency-surface": (path) => under(path, "docs/dependency-surface") || path.endsWith(".fsproj") || path.endsWith("packages.lock.json") || ["Directory.Packages.props", ".config/dotnet-tools.json"].includes(path),
  "sdd-byte-stability": (path) => under(path, "work/184-scenario-catalog") || under(path, "readiness/184-scenario-catalog") || under(path, ".fsgg") || path === ".config/dotnet-tools.json",
  "feedback-audit": (path) => under(path, "feedback") || under(path, ".github/workflows") || ["scripts/audit-binding-exceptions.json", "scripts/test-feedback-audit-binding-exceptions.sh", "scripts/ci-route.mjs", "scripts/test-ci-route.mjs"].includes(path),

  // S.I.R.#265. The set below is DERIVED from what scripts/test-review-contract-coherence.sh
  // actually opens, not from what the contract is about — those are different sets, and the
  // difference is the whole point of this row. The gate reads exactly five in-tree paths:
  //   docs/coordination-engine-contracts.md   the document under test (its `doc` variable)
  //   .config/dotnet-tools.json               the engine pin it resolves FS.GG.Coord.Core.dll from
  //   global.json                             the SDK pin it resolves DOTNET_ROOT against
  //   scripts/fsgg-coord                      the resolver it runs `facts --json` through
  //   scripts/test-review-contract-coherence.sh  itself
  // Change any one of those and the gate's verdict can move; that is what makes each a selector.
  //
  // THE PACKED SKILL MIRRORS ARE DELIBERATELY ABSENT, against the literal wording of #265's Scope
  // ("the routes where packed skills … can change"), and the reason is measured rather than
  // argued. `.claude`/`.agents/skills/pnext-item/references/independent-review.md` describe this
  // same contract in prose, but the gate never opens them — the file says so itself ("That gate
  // does not read this file, so this paragraph is not itself bound"). Falsifying a load-bearing
  // claim in the mirror and running the gate at e5bda9d exits 0. A mirror edit that also edits the
  // document is already selected by the document; a mirror edit that does not is a run whose
  // verdict could not have differed. Selecting on it would therefore add cost that cannot ever
  // report a finding — a decorative selector added while removing a decorative gate. If the
  // mirrors are ever brought under a check that reads them, this list is where that path goes.
  "review-contract": (path) => [
    "docs/coordination-engine-contracts.md",
    ".config/dotnet-tools.json",
    "global.json",
    "scripts/fsgg-coord",
    "scripts/test-review-contract-coherence.sh",
  ].includes(path),
};

// Per-PR selection is path-conditional on purpose (#248): a PR pays only for the subjects its own
// changes can break. That leaves a hole the predicates cannot close — a subject no changed path
// selects can stay red on the default branch indefinitely while every run truthfully reports
// `measured-omission` (#252). The sweep is the other half of that bargain: off the pull-request
// path every subject runs unconditionally. It is a separate mode rather than a widened predicate
// so that ordinary per-PR cost is unchanged.
export const sweepRequested = (environment = process.env) => environment[sweepEnvironmentVariable] === "true";

export function planFor(route, { sweep = false } = {}) {
  if (route?.schema !== "sir.ci-route/v2" || !Array.isArray(route.paths) || route.paths.length === 0 || !route.digest) throw new Error("ci-integrity-plan: malformed route");
  const selfChange = route.paths.some((path) => ["scripts/ci-integrity-plan.mjs", "scripts/test-ci-integrity-plan.mjs", "scripts/qualify-pr.sh"].includes(path));
  const topologyChange = route.paths.some((path) => under(path, ".github/workflows"));
  const unknown = route.facts?.some(({ rule }) => rule === "RP-005-unknown-conservative") ?? true;
  const subjects = subjectOrder.map((id) => {
    // `matchingPaths` stays truthful under a sweep: it records what the predicates WOULD have
    // selected, so an archived sweep plan shows subjects running that no changed path selected.
    const matchingPaths = route.paths.filter((path) => predicates[id](path));
    if (sweep) return { id, run: true, reason: "scheduled-sweep", matchingPaths };
    const run = selfChange || topologyChange || unknown || matchingPaths.length > 0;
    return { id, run, reason: selfChange ? "classifier-self-change" : topologyChange ? "topology-change" : unknown ? "unknown-conservative" : run ? "relevant-path" : "measured-omission", matchingPaths };
  });
  const body = { schema, mode: sweep ? "sweep" : "pull-request", routeDigest: route.digest, source: route.source, alwaysOn: ["ci-contract-floor"], subjects };
  return { ...body, digest: sha256(canonical(body)) };
}

async function main(argv) {
  const routeIndex = argv.indexOf("--route");
  const outputIndex = argv.indexOf("--output");
  if (routeIndex < 0 || outputIndex < 0) throw new Error("ci-integrity-plan: usage --route <path> --output <path> [--sweep]");
  const sweep = argv.includes("--sweep") || sweepRequested();
  const route = JSON.parse(await readFile(resolve(argv[routeIndex + 1]), "utf8"));
  const plan = planFor(route, { sweep });
  await mkdir(dirname(resolve(argv[outputIndex + 1])), { recursive: true });
  await writeFile(resolve(argv[outputIndex + 1]), canonical(plan));
  console.log(canonical(plan).trim());
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) main(process.argv.slice(2)).catch((error) => { console.error(error.message); process.exitCode = 1; });
