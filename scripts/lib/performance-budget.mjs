// THE declaration of the tactical SVG main-thread frame budget.
//
// This module is the SINGLE place the ceiling is stated. Before S.I.R.#299 the same measured
// quantity -- Chromium `AnimationFrame` `animation_frame_timing_info.duration_ms` on the production
// retained-SVG route -- was written out independently in five authored places, and one of them
// disagreed with the other four by 8.33 ms. Every one of those sites now derives from this object,
// and the one site that CANNOT import it (the published prose table in docs/performance-budget.md)
// is bound to it by a gate that fails closed. See tacticalFrameBudgetDocumentation below.
//
// Adding a second copy of any number here -- even one that agrees today -- restates the defect this
// module exists to remove. A consumer needs the value: import it.

// The ceiling, in milliseconds, for one animation-frame callback plus tactical DOM inspection on the
// production route. It is one frame period at 60 Hz, and is explicitly NOT a compositor, paint, GPU
// or swapchain claim.
const callbackMillisecondsCeiling = 16.67;

export const tacticalFrameBudget = Object.freeze({
  callbackMillisecondsCeiling,

  // A frame is DROPPED when its duration exceeds the ceiling: at 60 Hz a callback that overruns its
  // frame period has already missed the next vsync. This is the SAME quantity and the SAME array of
  // durations the budget verdict is derived from, so it takes the SAME threshold.
  //
  // S.I.R.#299 decided this deliberately rather than by unification. The previous inline `> 25`
  // threshold was not a different subject measured on purpose: `git log -L` shows it entering with
  // this library's first commit (fd6d0bc) already bare, with no derivation, no comment and no cited
  // source, and every later commit inherited it -- S.I.R.#268 only ANNOTATED it, recording that no
  // budget document declared it. There was no intent to preserve. Its effect was a contradiction:
  // a 20 ms frame breached the declared ceiling and was NOT counted as dropped, while a 26 ms frame
  // was counted against a threshold nothing published.
  droppedFrameCeilingMilliseconds: callbackMillisecondsCeiling,

  measures:
    "Chromium AnimationFrame animation_frame_timing_info.duration_ms: one animation-frame callback "
    + "plus tactical DOM inspection on the production retained-SVG route. Not a compositor, paint, "
    + "GPU or swapchain claim.",
  source: "scripts/lib/performance-budget.mjs (tacticalFrameBudget) -- the single declaration.",
  publishedAs:
    "docs/performance-budget.md, \"Tactical visual-system budget\" table, \"Browser callback/"
    + "main-thread inspection\" column. That prose cell is a PROJECTION of this value, not a second "
    + "declaration: assertDocumentedFrameCeiling() re-reads the table and refuses any divergence.",
});

// Where the published prose states this budget, and how to find the cell. The gate lives in
// scripts/test-svg-pipeline-measurement.mjs, which is already the ledger's declared replacement
// evidence for docs/performance-budget.md.
export const tacticalFrameBudgetDocumentation = Object.freeze({
  path: "docs/performance-budget.md",
  tableHeading: "Tactical visual-system budget",
  column: "Browser callback/main-thread inspection",
});

// Render the declared ceiling exactly as the published table writes it, so the document and the
// declaration cannot drift into two spellings of one number.
export function documentedFrameCeilingCell() {
  return `< ${tacticalFrameBudget.callbackMillisecondsCeiling} ms`;
}

// ---------------------------------------------------------------------------------------------
// S.I.R.#318: the rest of the "Tactical visual-system budget" table, and the two figures that had
// no table row at all.
//
// #299 brought the frame-ceiling COLUMN of that table here. The remaining columns were still
// restated by hand -- the node cap in three places, the effect ceiling in five -- and two further
// thresholds that CI enforces (`maximumInputToPaintMilliseconds`, and a `measurementTolerance`
// silently ADDED to the frame ceiling) were declared in no document whatsoever.
//
// The same rule applies to everything below it that applies above it: this is the ONE place these
// numbers are stated. A consumer needs one: import it.

// One frame period at 60 Hz is `callbackMillisecondsCeiling`, declared above. The number of frame
// periods a healthy inter-frame INTERVAL may span is a different quantity from a callback duration,
// and it is declared here rather than derived by arithmetic at a call site.
const maximumElapsedVsyncsPerFrame = 1;

export const tacticalWorkloadBudgets = Object.freeze({
  representative100: Object.freeze({
    key: "representative100",
    label: "Representative 100-unit replay",
    units: 100,
    maximumDomNodes: 5000,
    maximumEffects: 128,
    maximumInputToPaintMilliseconds: 100,
  }),
  stress200: Object.freeze({
    key: "stress200",
    label: "Stress 200-unit replay",
    units: 200,
    maximumDomNodes: 9000,
    maximumEffects: 256,
    maximumInputToPaintMilliseconds: 150,
  }),
});

export const tacticalWorkloadBudgetList = Object.freeze(Object.values(tacticalWorkloadBudgets));

export function tacticalWorkloadBudgetFor(units) {
  const budget = tacticalWorkloadBudgetList.find((candidate) => candidate.units === units);
  // A workload this table does not declare is an UNANSWERABLE question, not a passing one. Falling
  // back to either row would gate an unknown workload against a budget nobody wrote for it.
  if (!budget) throw new Error(`no declared tactical workload budget for ${units} units; declared workloads are ${tacticalWorkloadBudgetList.map(({ units: declared }) => declared).join(", ")}`);
  return budget;
}

// The row that applies to an observed unit count, for a consumer measuring a live scene rather than
// running one of the two declared review workloads. The smallest declared workload the scene still
// fits, or the largest one if it exceeds them all -- so the tier boundary is the declared `units`
// figure and not a literal repeated at the call site.
export function tacticalWorkloadBudgetAtScale(unitCount) {
  requireDeclaredUnitCount(unitCount);
  return tacticalWorkloadBudgetList.find((candidate) => unitCount <= candidate.units)
    ?? tacticalWorkloadBudgetList[tacticalWorkloadBudgetList.length - 1];
}

const requireDeclaredUnitCount = (unitCount) => {
  if (typeof unitCount !== "number" || !Number.isFinite(unitCount) || unitCount < 0) throw new Error(`cannot choose a tactical workload budget for unit count ${JSON.stringify(unitCount)}; an unreadable scale is refused, not bucketed`);
};

// THE FRAME-CADENCE BUDGET, AND WHY IT IS ITS OWN QUANTITY (S.I.R.#318).
//
// The tactical review route measures `animationFrameIntervalMilliseconds`: the p80 of the deltas
// between successive requestAnimationFrame timestamps. That is the INTERVAL BETWEEN frames -- the
// vsync cadence -- and NOT the callback DURATION that `tacticalFrameBudget` declares. The two are
// different quantities over different clocks, and the difference is not academic: an interval on a
// vsync-locked clock is bounded BELOW by one frame period, so comparing it to a ceiling of exactly
// one frame period is a comparison it can never comfortably win.
//
// That is what the removed `measurementToleranceMilliseconds: 1` was really doing. It entered bare
// in ee6a2df with no derivation, no comment and no cited source -- `git log -S` shows the whole
// budgets block arriving as literals in one commit -- so there is NO recoverable intent for the
// value 1, and this module does not invent one for it. Its only observable effect was that the
// route enforced a ceiling one millisecond LOOSER than the declared one while naming the declared
// one in its failure message, and every retained production measurement since fee32c6 reads
// 16.7 ms: a value that BREACHES the declared 16.67 ms ceiling and was reported green by the slack.
// A gate reporting green on a breach of the number it names is the defect, and adding the slack
// into the ceiling at the call site is what hid it.
//
// So the tolerance is REMOVED rather than re-homed, and the cadence gets the budget it always
// needed: a rAF interval is an integer multiple of the display's vsync period, so "no frame was
// dropped" is exactly "at most one vsync elapsed", i.e. an interval strictly below two frame
// periods. The ceiling below is DERIVED from the declared frame period and the declared vsync
// allowance; no new millisecond figure is authored.
export const tacticalFrameCadenceBudget = Object.freeze({
  maximumElapsedVsyncsPerFrame,

  // Strictly below this, exactly one vsync elapsed between the two callbacks and no frame was
  // dropped. At or above it, at least one whole frame was missed.
  intervalCeilingMilliseconds: (maximumElapsedVsyncsPerFrame + 1) * callbackMillisecondsCeiling,

  measures:
    "p80 of the deltas between successive requestAnimationFrame timestamps on the production "
    + "tactical route: the INTERVAL between frames, not the callback duration tacticalFrameBudget "
    + "declares. Not a compositor, paint, GPU or swapchain claim.",
  source: "scripts/lib/performance-budget.mjs (tacticalFrameCadenceBudget) -- the single declaration.",
});

// THE RUNTIME EFFECT CAP IS THE SAME NUMBER, NOT A NUMBER THAT AGREES (S.I.R.#318).
//
// `TacticalSceneProjection.MaximumEffectInstances` is what the product actually enforces: it
// truncates the effect array to that length and publishes it as `data-effect-limit`. The stress-200
// row's `maximumEffects` is that cap, not a budget that happens to match it. F# cannot import this
// module, so -- exactly as with the published prose table -- the binding is a gate that re-reads the
// source and refuses divergence in either direction. See assertRuntimeEffectCap in
// scripts/test-svg-pipeline-measurement.mjs.
export const tacticalRuntimeEffectCap = Object.freeze({
  path: "src/SIR.Client/TacticalSceneProjection.fs",
  binding: "MaximumEffectInstances",
  surfacedAs: "data-effect-limit",
  maximumEffectInstances: tacticalWorkloadBudgets.stress200.maximumEffects,
});

// The manifest block `scripts/generate-tactical-visual-review.mjs` publishes, built HERE so that the
// generator carries no budget key and no budget number at all. `sir-tactical-visual-review-v3`
// renamed `measurementToleranceMilliseconds` to the cadence ceiling this module derives; v2 carried
// the undeclared tolerance, so the shape change is a schema change and is versioned as one.
export const tacticalReviewManifestBudgets = Object.freeze(Object.fromEntries(
  tacticalWorkloadBudgetList.map((workload) => [workload.key, Object.freeze({
    maximumDomNodes: workload.maximumDomNodes,
    maximumEffects: workload.maximumEffects,
    maximumInputToPaintMilliseconds: workload.maximumInputToPaintMilliseconds,
    targetAnimationFrameMilliseconds: tacticalFrameBudget.callbackMillisecondsCeiling,
    maximumAnimationFrameIntervalMilliseconds: tacticalFrameCadenceBudget.intervalCeilingMilliseconds,
  })]),
));

// THE ENFORCEMENT LIVES HERE, NOT AT THE CALL SITE, AND THAT IS THE POINT.
//
// These three thresholds gate CI from `scripts/test-tactical-visual-review.mjs`, which cannot run
// without a built client and a browser and therefore cannot be inverted in-process. Every
// comparison they make is a pure function of a declared budget and a measured number, so the
// comparison is declared here and the browser-route consumer calls it. The gate that proves these
// can fail is `scripts/test-svg-pipeline-measurement.mjs`, which mutates the SUBJECT -- the declared
// budget or the measurement -- and observes the refusal.
//
// Each returns null when the measurement conforms, and a REASON when it does not; an unmeasured
// input is refused rather than passed, because "I could not evaluate this" is never "I evaluated it
// and it passed" (#266).
const requireFiniteMeasurement = (label, value) => {
  if (typeof value !== "number" || !Number.isFinite(value)) throw new Error(`${label} was not measured (got ${JSON.stringify(value)}); an unmeasured budget is refused, not passed`);
};

export function tacticalStructuralBudgetReason(budget, measured) {
  requireFiniteMeasurement(`${budget.label} domNodes`, measured?.domNodes);
  requireFiniteMeasurement(`${budget.label} effects`, measured?.effects);
  if (measured.domNodes > budget.maximumDomNodes) return `${budget.units}-unit SVG node budget exceeded: measured ${measured.domNodes} against the declared ceiling of ${budget.maximumDomNodes}`;
  if (measured.effects > budget.maximumEffects) return `${budget.units}-unit active-effect budget exceeded: measured ${measured.effects} against the declared ceiling of ${budget.maximumEffects}`;
  return null;
}

export function tacticalInputToPaintBudgetReason(budget, measuredMilliseconds) {
  requireFiniteMeasurement(`${budget.label} inputToPaintMilliseconds`, measuredMilliseconds);
  if (measuredMilliseconds < budget.maximumInputToPaintMilliseconds) return null;
  return `${budget.units}-unit input-to-paint budget exceeded: measured ${measuredMilliseconds} ms against the declared ceiling of ${budget.maximumInputToPaintMilliseconds} ms`;
}

export function tacticalFrameCadenceBudgetReason(budget, measuredIntervalMilliseconds) {
  requireFiniteMeasurement(`${budget.label} animationFrameIntervalMilliseconds`, measuredIntervalMilliseconds);
  // NO ARITHMETIC HERE, DELIBERATELY. The removed defect was `measured <= ceiling + tolerance` at
  // the call site: a second, undeclared budget reached by addition instead of by a literal.
  if (measuredIntervalMilliseconds < tacticalFrameCadenceBudget.intervalCeilingMilliseconds) return null;
  return `${budget.units}-unit frame cadence budget exceeded: measured ${measuredIntervalMilliseconds} ms against the declared ceiling of ${tacticalFrameCadenceBudget.intervalCeilingMilliseconds} ms (${tacticalFrameCadenceBudget.maximumElapsedVsyncsPerFrame} elapsed vsync at ${tacticalFrameBudget.callbackMillisecondsCeiling} ms)`;
}

// --- the published projection of the whole table ------------------------------------------------
// Every column below is a PROJECTION of a declaration above, rendered exactly as the published table
// writes it. `Release projection p95` is deliberately absent: no consumer gates on it, nothing
// restates it, and binding a column this module does not declare would make the gate assert
// authority it does not have.
const documentedThousands = (value) => value.toLocaleString("en-US");

export const tacticalBudgetTableDocumentation = Object.freeze({
  path: "docs/performance-budget.md",
  tableHeading: "Tactical visual-system budget",
  workloadColumn: "Production workload",
});

export function documentedTacticalBudgetRows() {
  return tacticalWorkloadBudgetList.map((workload) => Object.freeze({
    [tacticalBudgetTableDocumentation.workloadColumn]: workload.label,
    "Estimated SVG nodes": `≤ ${documentedThousands(workload.maximumDomNodes)}`,
    "Active effects": `≤ ${workload.maximumEffects}`,
    [tacticalFrameBudgetDocumentation.column]: documentedFrameCeilingCell(),
    "Input-to-paint p80": `< ${workload.maximumInputToPaintMilliseconds} ms`,
    "Frame cadence p80": `< ${tacticalFrameCadenceBudget.intervalCeilingMilliseconds} ms`,
  }));
}
