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
  //
  // AND THIS LIST IS THE WHOLE SELECTION, which is not true of the five subjects above it: see
  // `costBoundedSubjects` below. They also run under three conservative fallbacks; this one does
  // not, so a path absent from this list does not reach the gate by any other route on a pull
  // request. Read the two together — the predicate alone understates what is excluded.
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

// S.I.R.#265. THE CONSERVATIVE FALLBACKS DO NOT APPLY TO EVERY SUBJECT, AND THIS IS THE MEASURED
// REASON — not a budget being bent to fit a slow gate.
//
// `selfChange`/`topologyChange`/`unknown` below run a subject when the predicates MIGHT have missed
// something. That is right for a subject whose input set is OPEN: `npm-audit` depends on transitive
// npm state, `governance` matches any path containing "governance", and neither can be enumerated.
// `review-contract`'s input set is CLOSED and derivable by reading the script it dispatches — the
// document, the engine pin, the SDK pin, `scripts/fsgg-coord`, and itself. That is exactly why its
// predicate could be derived at all, and it is why "we cannot classify this path" carries no
// information about it.
//
// AND THE COST IS NOT AFFORDABLE ON THE ROUTES THE FALLBACKS FIRE ON. The gate costs ~26s locally
// and 105.6s on a CI runner (measured on run 32645759275: `feedback-audit` ends 14:35:00.47,
// `review-contract` ends 14:36:46.08). The fallbacks fire on `.github/workflows` and unclassified
// paths, which the router classifies `cross-cutting` — the widest route, and the one already
// closest to `ci-route.mjs`'s acceptance target. Three recent GREEN cross-cutting runs measured
// criticalPath 239677ms, 301802ms and 232055ms against an acceptanceTarget of 312000ms: a margin
// between 10.2s and 80s. Adding 105.6s there does not erode the headroom, it deletes it — measured
// as `feedback-headroom-eroded` 317295 > 312000 on run 32645759275.
//
// The other half of the #248/#252 bargain is what makes this safe rather than a hole: the off-PR
// sweep below runs this subject UNCONDITIONALLY, so it still cannot sit red on the default branch
// unobserved. What is given up is a conservative re-run on routes that changed none of its inputs.
// The omission carries its OWN reason code so it is legible in an archived plan instead of being
// indistinguishable from a subject whose predicates simply did not match.
export const costBoundedSubjects = new Set(["review-contract"]);

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
    // One expression for "which conservative fallback fired", so `run` and `reason` cannot disagree
    // about it. `null` for a cost-bounded subject is what makes the exemption a single decision
    // rather than two that must be kept in step. Every non-exempt subject is bit-for-bit unchanged.
    const conservative = costBoundedSubjects.has(id) ? null
      : selfChange ? "classifier-self-change"
      : topologyChange ? "topology-change"
      : unknown ? "unknown-conservative"
      : null;
    const run = conservative !== null || matchingPaths.length > 0;
    const reason = conservative
      ?? (run ? "relevant-path" : costBoundedSubjects.has(id) ? "cost-bounded-omission" : "measured-omission");
    return { id, run, reason, matchingPaths };
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
