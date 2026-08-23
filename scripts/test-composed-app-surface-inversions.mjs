// Gate-inversion evidence for the composed App ownership surface.
//
// WHY THIS EXISTS.  App.fs grew past its 8,200-line ownership ceiling, so the
// tactical scene owner was extracted into src/SIR.Client.Web/TacticalScenePresentation.fs
// and its shared primitives into src/SIR.Client.Web/TacticalSharedControls.fs.
// Six source-scanning gates asserted their ownership invariants against App.fs
// -- four of them against App.fs ALONE -- and every token that moved out of that
// file would have stopped being guarded the moment the extraction landed, while
// each gate went on reporting green.  Those gates were re-pointed at the composed
// surface (the same thing M0 and M7 already documented doing for PanelViews.fs).
//
// A re-pointed source scanner is exactly the shape of change that silently
// defangs a check: it keeps passing, it looks like housekeeping, and nobody
// notices it stopped biting.  So this harness proves each one still bites.  For
// every gate it takes a property that gate guards, introduces a real violation of
// it IN THE EXTRACTED MODULE, runs the gate, and requires the gate to go red and
// to name what broke.  The mutation is always reverted, including on failure.
//
// A gate that survives its own inversion is decorative.  This script fails when
// that happens, which is the only way that fact reaches anyone.
//
// Run: node scripts/test-composed-app-surface-inversions.mjs
// Requires a built client (npm run build:client) because several of these gates
// boot the production bundle before they reach their source assertions.

import { readFile, writeFile } from "node:fs/promises";
import { spawn, execFileSync } from "node:child_process";
import { resolve } from "node:path";

// The gates this harness drives are not all side-effect-free -- at least one
// rewrites a review telemetry artifact with machine-dependent timing and heap
// values just by running. Restoring only the file WE mutate would leave the tree
// dirty and call it clean, so snapshot the whole tracked working tree and report
// any path this run left changed. Visible beats tidy.
const trackedStatus = () =>
  execFileSync("git", ["status", "--porcelain"], { encoding: "utf8" }).trim();

const PRESENTATION = "src/SIR.Client.Web/TacticalScenePresentation.fs";

// Each case: the gate, the file it must now be reading, the exact violation, and
// the words the gate's own refusal has to contain.  Asserting on the message is
// what stops a gate that fails for an unrelated reason from counting as evidence
// that this property is guarded.
const cases = [
  {
    gate: "scripts/test-persistent-workspace-m0-baseline.mjs",
    guards: "the single persistent SVG root is present in the composed render surface",
    file: PRESENTATION,
    from: 'svg.id "persistent-tactical-svg"',
    to: 'svg.id "persistent-tactical-svg-inverted"',
    expect: "persistent-tactical-svg",
  },
  {
    gate: "scripts/test-planning-workspace-m5-qualification.mjs",
    guards: "the shared route layer is owned by the persistent renderer",
    file: PRESENTATION,
    from: 'svg.id "persistent-layer-routes"',
    to: 'svg.id "persistent-layer-routes-inverted"',
    expect: "persistent-layer-routes",
  },
  {
    gate: "scripts/test-simulator-workspace-m6-qualification.mjs",
    guards: "runtime units project their presentation coordinates",
    file: PRESENTATION,
    from: '"data-presentation-column"',
    to: '"data-presentation-column-inverted"',
    expect: "data-presentation-column",
  },
  {
    gate: "scripts/test-review-workspace-m7-qualification.mjs",
    guards: "review interpolation publishes its presentation alpha",
    file: PRESENTATION,
    from: '"data-presentation-alpha"',
    to: '"data-presentation-alpha-inverted"',
    expect: "data-presentation-alpha",
  },
  {
    // M8 requires no positive token that moved out of App.fs, so re-pointing it
    // is inert for its positive assertions -- and STRICTER for its negative ones,
    // which is the half proven here.  Recorded plainly rather than dressed up:
    // this inversion demonstrates the dead-branch scan reaches the new module,
    // not that a moved M8 token is still guarded.
    gate: "scripts/test-timeline-supporting-panels-m8-qualification.mjs",
    guards: "obsolete replacement-page branches cannot hide in the extracted module",
    file: PRESENTATION,
    from: "let tacticalSceneOwner =",
    to: 'let obsoleteBranchProbe = "RulesWorkspace"\nlet tacticalSceneOwner =',
    expect: "RulesWorkspace",
  },
  {
    gate: "scripts/test-persistent-workspace-m9-acceptance.mjs",
    guards: "exactly one persistent scene renderer definition exists",
    file: PRESENTATION,
    from: "let persistentSceneSvg",
    to: "let persistentSceneSvgRenamed",
    expect: "renderer definition",
  },
  {
    gate: "scripts/test-map-editor-qualification.mjs",
    guards: "the retired Editor renderer cannot reappear in the extracted module",
    file: PRESENTATION,
    from: "let tacticalSceneOwner =",
    to: "let private editorBattlefield = ignore\nlet tacticalSceneOwner =",
    expect: "dead Editor renderer source remains",
  },
];

const runGate = (gate) =>
  new Promise((done) => {
    const child = spawn(process.execPath, [gate], { cwd: process.cwd() });
    let output = "";
    child.stdout.on("data", (chunk) => (output += chunk));
    child.stderr.on("data", (chunk) => (output += chunk));
    child.on("close", (code) => done({ code, output }));
  });

const failures = [];
const statusBefore = trackedStatus();
for (const probe of cases) {
  const path = resolve(probe.file);
  const original = await readFile(path, "utf8");
  if (!original.includes(probe.from)) {
    failures.push(
      `${probe.gate}: inversion is stale -- ${probe.file} no longer contains ${JSON.stringify(probe.from)}, so this gate's evidence proves nothing`,
    );
    continue;
  }
  // A red gate is only evidence of biting if it was GREEN immediately before the
  // violation.  Without this pair, a gate that is red for an unrelated reason --
  // a stale review binding, a broken prerequisite -- reads as a passing
  // inversion, and the harness certifies a check it never actually exercised.
  const baseline = await runGate(probe.gate);
  if (baseline.code !== 0) {
    failures.push(
      `${probe.gate}: NOT GREEN before its inversion, so nothing it does under mutation is evidence. Fix the gate (build the client and regenerate review artifacts) and re-run. Tail: ${baseline.output.slice(-300)}`,
    );
    continue;
  }
  await writeFile(path, original.replace(probe.from, probe.to), "utf8");
  let result;
  try {
    result = await runGate(probe.gate);
  } finally {
    await writeFile(path, original, "utf8");
  }
  if (result.code === 0) {
    failures.push(
      `${probe.gate}: SURVIVED its inversion (${probe.guards}). The gate is decorative on the extracted module.`,
    );
  } else if (!result.output.includes(probe.expect)) {
    failures.push(
      `${probe.gate}: went red but never named ${JSON.stringify(probe.expect)}, so it did not fail for the inverted property. Tail: ${result.output.slice(-400)}`,
    );
  } else {
    process.stdout.write(`inverted ${probe.gate}: red as required (${probe.guards})\n`);
  }
}

// Every mutation is reverted above; prove the tree really is clean rather than
// asserting it, because a harness that corrupts the tree on the way to a green
// result is worse than no harness.
const restored = await readFile(resolve(PRESENTATION), "utf8");
for (const probe of cases) {
  if (probe.file === PRESENTATION && !restored.includes(probe.from)) {
    failures.push(`${probe.file} was not restored after inverting ${probe.gate}`);
  }
}

const statusAfter = trackedStatus();
if (statusAfter !== statusBefore) {
  const before = new Set(statusBefore.split("\n"));
  const introduced = statusAfter.split("\n").filter((line) => line && !before.has(line));
  failures.push(
    `this run left the working tree changed; restore with git checkout -- <path> and re-read the gates that did it:\n      ${introduced.join("\n      ")}`,
  );
}

if (failures.length > 0) {
  throw new Error(`Composed App surface inversion evidence failed:\n  - ${failures.join("\n  - ")}`);
}
process.stdout.write(
  `Composed App ownership surface inversions passed: ${cases.length} gates each go red when the extracted module violates the property they guard.\n`,
);
