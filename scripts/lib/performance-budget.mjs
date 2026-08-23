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

// THE REFRESH RATE IS DECLARED, AND THE FRAME PERIOD IS DERIVED FROM IT -- NOT FROM THE PUBLISHED
// CALLBACK CEILING (S.I.R.#318 round 1).
//
// `callbackMillisecondsCeiling` is 16.67: one frame period at 60 Hz ROUNDED UP to the two decimals
// the table publishes. As a ceiling on a callback DURATION that rounding is conservative and
// harmless -- it admits 0.0033 ms more work than a perfect frame period.
//
// As the UNIT of a cadence measurement it is the opposite of harmless, and the first version of this
// module got it wrong in exactly that way. Doubling a rounded-UP period gives 33.34 ms, which is
// ABOVE a real one-dropped-frame interval at 60.000 Hz (2 x 16.6667 = 33.3333 ms). The gate that
// exists to catch a dropped frame therefore admitted one -- at the nominal refresh the declaration
// itself names, and at every refresh at or above 59.988 Hz. It was not theoretical: this repository's
// own captures at ee6a2df measured periods of 16.602 and 16.642 ms, whose dropped-frame intervals are
// 33.204 and 33.284 ms -- so a scene running at 30 fps, dropping every single frame, passed.
//
// A budget may be rounded for publication. A UNIT may not. The period below is exact.
const displayRefreshHertz = 60;
const framePeriodMilliseconds = 1000 / displayRefreshHertz;

// The number of vsyncs a healthy inter-frame INTERVAL may span. This is a different quantity from a
// callback duration, and it is declared here rather than derived by arithmetic at a call site.
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
//
// HOW THE INTERVAL IS CLASSIFIED. A rAF interval on a vsync-locked clock is an integer multiple of
// the frame period, so the question "was a frame dropped?" is "how many vsyncs elapsed?" -- and the
// discriminating boundary between one and two is the MIDPOINT between them, not two periods. The
// midpoint is maximally far from both candidates, which is exactly what makes the answer survive the
// true period differing a little from nominal; a boundary sitting ON a candidate answers whichever
// way the last digit falls. The first version of this module used two periods and was wrong for that
// reason (see the note above `displayRefreshHertz`).
//
// AND THE RULE HAS A DOMAIN, WHICH IS ENFORCED RATHER THAN DESCRIBED. Nearest-vsync classification is
// correct only while the true frame period is at least half the ceiling and below it -- refresh in
// (40, 80] Hz for the declared 60. Outside that window a healthy interval and a dropped one can both
// land inside the band, so a confident verdict would be an invention. An interval below the floor is
// therefore REFUSED as unclassifiable rather than passed: "I could not evaluate this" is never "I
// evaluated it and it passed" (#266).
export const tacticalFrameCadenceBudget = Object.freeze({
  // Every declared budget object NAMES ITSELF, so a quantity id is read off the declaration rather
  // than supplied by a fallback at the gate. See tacticalDeclaredBudgetObjects below.
  key: "tacticalFrameCadenceBudget",

  displayRefreshHertz,
  framePeriodMilliseconds,
  maximumElapsedVsyncsPerFrame,

  // The midpoint between the largest allowed vsync count and the next one up. Spelled over a common
  // denominator so it is exact in binary floating point and publishes as a clean decimal: doubling
  // and halving a period is where the rounding this budget exists to survive creeps back in.
  intervalCeilingMilliseconds:
    (2 * maximumElapsedVsyncsPerFrame + 1) * 1000 / (2 * displayRefreshHertz),

  // Below this the measurement cannot be a single vsync at any refresh this rule classifies, so the
  // verdict is a refusal rather than a pass. Derived from the ceiling, not authored.
  minimumClassifiableIntervalMilliseconds:
    ((2 * maximumElapsedVsyncsPerFrame + 1) * 1000 / (2 * displayRefreshHertz)) / (maximumElapsedVsyncsPerFrame + 1),

  measures:
    "p80 of the deltas between successive requestAnimationFrame timestamps on the production "
    + "tactical route: the INTERVAL between frames, not the callback duration tacticalFrameBudget "
    + "declares. Not a compositor, paint, GPU or swapchain claim.",
  source: "scripts/lib/performance-budget.mjs (tacticalFrameCadenceBudget) -- the single declaration.",
});

// FIGURES THIS REPOSITORY HAS RETIRED, KEPT ON THE RECORD ON PURPOSE.
//
// A repair can only prove it discriminates by naming the alternative it must now exclude; S.I.R.#299
// established that when it named the superseded 25 ms dropped-frame threshold in its own band
// fixture. Without the superseded figure a band value is just another magic number.
//
// They live here, declared, so the suites that name them are checkable rather than merely commented:
// the no-restatement sweep allows a retired figure to appear ONLY under the identifier declared for
// it here, and refuses it anywhere else. A retired figure nobody names any more is a stale entry and
// reds.
export const supersededTacticalFigures = Object.freeze([
  Object.freeze({
    identifier: "supersededDropThresholdMilliseconds",
    value: 25,
    retiredBy: "S.I.R.#299",
    reason: "the bare inline dropped-frame threshold, which no budget document declared and which contradicted the declared callback ceiling by 8.33 ms",
  }),
  Object.freeze({
    identifier: "supersededToleranceMilliseconds",
    value: 1,
    retiredBy: "S.I.R.#318",
    reason: "measurementToleranceMilliseconds, added to the frame ceiling at the call site so the route enforced a ceiling one millisecond looser than the one its failure message named",
  }),
  Object.freeze({
    identifier: "supersededCadenceCeilingMilliseconds",
    value: 33.34,
    retiredBy: "S.I.R.#318 review round 1",
    reason: "two ROUNDED-UP frame periods, which sits above a real one-dropped-frame interval at 60.000 Hz and so admitted the failure the cadence budget exists to catch",
  }),
]);

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

// THE OVERLAY-LAYER NODE BOUND, AND WHY IT IS ITS OWN QUANTITY (S.I.R.#327).
//
// `#persistent-tactical-overlay-layer` is ONE of the eight layers the scene paints
// (terrain>edges>routes>units>effects>selection>tactical-overlays>annotations). The product publishes
// that layer's cost separately as `data-overlay-node-estimate`, emitted from the overlay set's own
// `Cost.EstimatedSvgNodes`. The WHOLE-SCENE estimate is a different attribute
// (`data-visual-node-estimate`) computed over a different subtree, and `maximumDomNodes` bounds that.
//
// THIS BOUND AND representative100.maximumDomNodes ARE BOTH 5000, AND THAT IS A COINCIDENCE OF
// VALUE, NOT AN IDENTITY. Deriving one from the other would install the "do these agree?" defect at
// the declaration: they agree today, they would move together tomorrow, and no reader could tell a
// shared value from a shared meaning. Measured rather than argued -- toggling overlays moves
// `data-overlay-node-estimate` 23 -> 12 while `data-visual-node-estimate` holds at 479. The value
// sweep in scripts/test-svg-pipeline-measurement.mjs has also carried "an unrelated overlay-layer
// node bound in the browser spec" as an exemption reason since S.I.R.#318, so the two quantities were
// known to be distinct before either was declared.
//
// IT IS A SCALAR, DELIBERATELY. The assertion runs at the product's default scale rather than at
// either declared review workload, and nothing in the product tiers overlay cost by unit count.
// A per-workload row would mean inventing a stress-200 figure this repository has never measured --
// a threshold invented at the point of use, which is half of what S.I.R.#318 repaired -- or writing
// 5000 down twice, which the rule at the top of this file forbids even when the copies agree. The
// precedent for a scalar published into every row of that table is
// tacticalFrameBudget.callbackMillisecondsCeiling.
//
// PROVENANCE. The literal entered at 8cb6449 ("Add configurable tactical analysis overlays",
// 2026-08-13) already bare: no derivation, no comment and no cited source. 1194ce7 later the same day
// MOVED it onto the counted-node identifier when it added the estimate-honesty check. There is no
// recoverable intent for the value 5000 and this module does not invent one; what changes is that the
// number is stated ONCE, published, and enforced HERE rather than at the call site.
const maximumOverlayDomNodes = 5000;

export const tacticalOverlayLayerBudget = Object.freeze({
  key: "tacticalOverlayLayerBudget",

  maximumOverlayDomNodes,

  measures:
    "element count of the live #persistent-tactical-overlay-layer subtree, which the product "
    + "publishes as data-overlay-node-estimate. ONE layer of the tactical scene, not the whole-scene "
    + "node estimate maximumDomNodes bounds, and not a compositor, paint, GPU or swapchain claim.",
  surfacedAs: "data-overlay-node-estimate",
  source: "scripts/lib/performance-budget.mjs (tacticalOverlayLayerBudget) -- the single declaration.",
  publishedAs:
    "docs/performance-budget.md, \"Tactical visual-system budget\" table, \"Overlay-layer SVG nodes\" "
    + "column. Workload-independent, so the same declared cell is projected into every row.",
});

// EVERY TOP-LEVEL DECLARED BUDGET OBJECT, NAMED HERE RATHER THAN LISTED IN THE GATE.
//
// The no-restatement sweep used to build this list itself, and reached a quantity only if somebody
// remembered to add it -- a hand-maintained list of budget objects inside the gate that exists to
// stop hand-maintained copies of budget numbers, which is the same disease one level up that
// S.I.R.#299's critic found in its consumer list and S.I.R.#318 found again in its budget fields.
export const tacticalDeclaredBudgetObjects = Object.freeze([
  ...tacticalWorkloadBudgetList,
  tacticalFrameCadenceBudget,
  tacticalOverlayLayerBudget,
]);

// WHERE EACH BUDGETED QUANTITY IS SURFACED TO A BROWSER CONSUMER.
//
// A consumer that reads one of these attributes off the live DOM is holding the MEASUREMENT, and the
// only thing it may compare that measurement against is the declaration. Bounding it with a numeric
// literal is either a restated budget or -- worse, and this is the half of S.I.R.#318 that had no
// table row at all -- a fresh threshold invented at the point of use. The gate keyed off this list
// refuses a numeric literal on any line that touches the identifier read from these attributes,
// whatever the number is, so an invented bound is caught as readily as a copied one.
export const tacticalBudgetSurfaces = Object.freeze([
  Object.freeze({ quantity: "maximumDomNodes", attribute: "data-visual-node-estimate" }),
  Object.freeze({ quantity: "maximumEffects", attribute: "data-effect-limit" }),
]);

// The manifest block `scripts/generate-tactical-visual-review.mjs` publishes, built HERE so that the
// generator carries no budget key and no budget number at all. `sir-tactical-visual-review-v3`
// renamed `measurementToleranceMilliseconds` to the cadence ceiling this module derives; v2 carried
// the undeclared tolerance, so the shape change is a schema change and is versioned as one.
// ONE PROJECTION LIST, TWO PUBLISHED SURFACES, AND EVERY ENTRY NAMES THE QUANTITY IT PUBLISHES.
//
// The published table and the review manifest used to be built independently, and the guard that
// asked "is every declared budget published?" could only compare NUMBERS -- so an undeclared budget
// whose value happened to equal 256, 16.67 or 150 was reported as published by a surface that had
// never heard of it. That is "do these agree?" one level up, inside the guard written to stop it.
//
// Both surfaces are now generated from this list, and each entry declares the quantity ids it
// projects. The guard asks whether the QUANTITY is projected, never whether its value appears.
const tacticalBudgetProjections = Object.freeze([
  Object.freeze({
    column: "Estimated SVG nodes",
    quantities: (workload) => [`${workload.key}.maximumDomNodes`],
    cell: (workload) => `≤ ${documentedThousands(workload.maximumDomNodes)}`,
    manifest: (workload) => ({ maximumDomNodes: workload.maximumDomNodes }),
  }),
  Object.freeze({
    // WORKLOAD-INDEPENDENT, so the workload argument is deliberately ignored and the one declared
    // cell is projected into every row -- the same shape the frame-ceiling column below uses.
    //
    // It projects into the published TABLE and not into the review manifest, because the tactical
    // review does not measure the overlay layer: publishing a budget into an artifact that never
    // reports it would be a number a reader could not check against anything. `quantities` still
    // names it, so the "declared in code and published nowhere" guard is satisfied by the table.
    column: "Overlay-layer SVG nodes",
    quantities: () => ["tacticalOverlayLayerBudget.maximumOverlayDomNodes"],
    cell: () => `≤ ${documentedThousands(tacticalOverlayLayerBudget.maximumOverlayDomNodes)}`,
    manifest: () => ({}),
  }),
  Object.freeze({
    column: "Active effects",
    quantities: (workload) => [`${workload.key}.maximumEffects`],
    cell: (workload) => `≤ ${workload.maximumEffects}`,
    manifest: (workload) => ({ maximumEffects: workload.maximumEffects }),
  }),
  Object.freeze({
    column: tacticalFrameBudgetDocumentation.column,
    quantities: () => ["tacticalFrameBudget.callbackMillisecondsCeiling"],
    cell: () => documentedFrameCeilingCell(),
    manifest: () => ({ targetAnimationFrameMilliseconds: tacticalFrameBudget.callbackMillisecondsCeiling }),
  }),
  Object.freeze({
    column: "Input-to-paint p80",
    quantities: (workload) => [`${workload.key}.maximumInputToPaintMilliseconds`],
    cell: (workload) => `< ${workload.maximumInputToPaintMilliseconds} ms`,
    manifest: (workload) => ({ maximumInputToPaintMilliseconds: workload.maximumInputToPaintMilliseconds }),
  }),
  Object.freeze({
    // BOTH cadence bounds in one cell, because both are gate boundaries a run can meet and a
    // published budget with an unpublished floor is half a budget.
    column: "Frame cadence p80",
    quantities: () => [
      "tacticalFrameCadenceBudget.minimumClassifiableIntervalMilliseconds",
      "tacticalFrameCadenceBudget.intervalCeilingMilliseconds",
    ],
    cell: () => `≥ ${tacticalFrameCadenceBudget.minimumClassifiableIntervalMilliseconds} ms, < ${tacticalFrameCadenceBudget.intervalCeilingMilliseconds} ms`,
    manifest: () => ({
      minimumClassifiableAnimationFrameIntervalMilliseconds: tacticalFrameCadenceBudget.minimumClassifiableIntervalMilliseconds,
      maximumAnimationFrameIntervalMilliseconds: tacticalFrameCadenceBudget.intervalCeilingMilliseconds,
    }),
  }),
]);

// The quantity ids every published surface projects, for the guard that refuses a budget declared in
// code and published nowhere. Ids, never values.
export const tacticalProjectedQuantities = Object.freeze([...new Set(
  tacticalWorkloadBudgetList.flatMap((workload) => tacticalBudgetProjections.flatMap((projection) => projection.quantities(workload))),
)]);

export const tacticalReviewManifestBudgets = Object.freeze(Object.fromEntries(
  tacticalWorkloadBudgetList.map((workload) => [workload.key, Object.freeze(
    Object.assign({}, ...tacticalBudgetProjections.map((projection) => projection.manifest(workload))),
  )]),
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

// THE OVERLAY-LAYER VERDICT, INCLUDING ITS TELEMETRY CROSS-CHECK (S.I.R.#327).
//
// This is the whole comparison, and the browser spec CALLS it rather than writing one. That is the
// difference between this and every earlier attempt at the same repair, all of which policed how the
// spec was allowed to write the comparison and were escaped six times in four review rounds -- once
// per way of writing it, because that space has no end. There is nothing here for a source scanner to
// police: the spec passes two measurements in and asserts the verdict is null, so there is no literal
// to plant and no statement shape to get wrong.
//
// It also makes the cross-check UNREMOVABLE rather than merely required. An earlier revision demanded
// that the spec contain an equality assertion, and a later revision of the same item silently deleted
// that demand while the declaration still advertised it. Here the cross-check is a branch of this
// function: deleting it means editing the declaration, where it is inverted in-process.
//
// TWO DISTINCT FAILURES, REPORTED DISTINCTLY.
//
//   1. the telemetry disagrees with the count. `data-overlay-node-estimate` is the product's own
//      REPORT of the layer's cost and is not identical to it -- review of this item measured the
//      report under-reporting the count on the static hosting route, while the production .NET route
//      agreed in every state reached. So the report is corroborated against an independent count
//      rather than trusted, and a disagreement is named as a telemetry fault, not as a budget breach.
//   2. the counted nodes exceed the declared bound. THE BUDGET IS ABOUT THE COUNT, not the report: a
//      budget bound to a self-report enforces the report rather than the cost.
//
// The cross-check is checked FIRST, so a wrong report can never be the thing a budget verdict was
// computed from. And an unmeasured input is refused rather than passed: "I could not evaluate this"
// is never "I evaluated it and it passed" (#266).
//
// THE BOUND ON THE CROSS-CHECK, STATED: it corroborates the attribute in the states the caller
// actually reaches. It is not a certificate that the product's estimate is correct everywhere.
export function tacticalOverlayLayerBudgetReason(measured) {
  requireFiniteMeasurement("overlay-layer countedNodes", measured?.countedNodes);
  requireFiniteMeasurement("overlay-layer reportedEstimate", measured?.reportedEstimate);
  if (measured.countedNodes !== measured.reportedEstimate)
    return `overlay-layer telemetry disagrees with the live DOM: ${tacticalOverlayLayerBudget.surfacedAs} reports ${measured.reportedEstimate} and the layer actually contains ${measured.countedNodes} element(s). The budget is applied to the counted nodes, so this is a telemetry fault rather than a budget breach -- but a report that disagrees with what it reports on is not evidence about anything, and it is refused before any verdict is derived from it.`;
  if (measured.countedNodes > tacticalOverlayLayerBudget.maximumOverlayDomNodes)
    return `overlay-layer node budget exceeded: the live #persistent-tactical-overlay-layer subtree contains ${measured.countedNodes} element(s) against the declared ceiling of ${tacticalOverlayLayerBudget.maximumOverlayDomNodes}`;
  return null;
}

// AND THE FORM THE BROWSER CONSUMER CALLS: IT MEASURES, IT THROWS, AND IT RECORDS THAT IT RAN.
//
// Three obligations used to sit at the call site, and each one was a place this item's defect class
// could live. They are removed in order.
//
//   1. THE COMPARISON. Handled by the reason function above: the spec no longer writes one.
//   2. ASSERTING ON THE RESULT. A function returning null-or-reason still requires the caller to
//      assert correctly, and `.toBeDefined()`, `.toBeTruthy()` and discarding the result all passed
//      while a breach returned a reason nobody read. So this throws and returns nothing.
//   3. SUPPLYING THE MEASUREMENTS -- and this is the one review round 1 of the fresh chain found.
//      Taking two numbers meant the call site chose them, so `{ countedNodes: 0, reportedEstimate: 0 }`
//      satisfied the budget while the real layer held 5001 elements, and a repointed locator measured
//      a subtree that does not exist. This takes the LIVE PAGE and measures the subject itself, from
//      selectors declared here. There is nothing for a call site to fabricate.
//
// AND WHETHER IT RAN IS ANSWERED BY EXECUTION, NOT BY READING THE SPEC. Every earlier attempt to
// answer "was this called?" was a regex over the consumer's source, and each spelling closed opened
// another -- commenting the call out, `if (false)`, and `test.skip()` all left the gate printing
// JUSTIFIED while a real 5001-element breach shipped and the browser suite reported 1 passed. A
// counter that only a real invocation can move cannot be satisfied by any way of writing the call,
// because it is not looking at the writing.
export const tacticalOverlayLayerSurface = Object.freeze({
  root: "#persistent-tactical-svg",
  layer: "#persistent-tactical-overlay-layer",
  attribute: "data-overlay-node-estimate",
});

let tacticalOverlayLayerEvaluations = 0;

// An opaque mark, so a caller compares two marks rather than reasoning about a count.
export function tacticalOverlayLayerEvaluationMark() {
  return tacticalOverlayLayerEvaluations;
}

export async function assertTacticalOverlayLayerBudget(page) {
  if (!page || typeof page.locator !== "function")
    throw new Error("the overlay-layer budget assertion measures the live page and must be given one; it does not accept measurements chosen by the caller");
  const root = page.locator(tacticalOverlayLayerSurface.root);
  const layer = root.locator(tacticalOverlayLayerSurface.layer);
  const countedNodes = await layer.locator("*").count();
  // NOT `Number(await ...)` DIRECTLY: `Number(null)` is 0, so an attribute the product has stopped
  // emitting would read as a confident zero and sail under the ceiling instead of being refused.
  // "I could not read this" is never "I read it and it was fine" (#266).
  const reported = await root.getAttribute(tacticalOverlayLayerSurface.attribute);
  if (reported === null || `${reported}`.trim() === "")
    throw new Error(`${tacticalOverlayLayerSurface.attribute} is absent from ${tacticalOverlayLayerSurface.root}, so the overlay layer's reported cost could not be read at all. An absent report is refused, not read as zero.`);
  const reportedEstimate = Number(reported);
  // Recorded BEFORE the throw: a breach is an evaluation that happened and failed, not one that never
  // ran, and the two must never be confused by the guard below.
  tacticalOverlayLayerEvaluations += 1;
  const reason = tacticalOverlayLayerBudgetReason({ countedNodes, reportedEstimate });
  if (reason) throw new Error(reason);
}

// THE REFUSAL THE BROWSER SUITE PERFORMS. A run in which the verdict was never evaluated is refused,
// and it is refused on an observed counter rather than on the shape of the source that should have
// moved it.
export function assertTacticalOverlayLayerBudgetWasEvaluatedSince(mark) {
  if (!Number.isInteger(mark))
    throw new Error("the overlay-layer evaluation guard needs a mark taken before the work ran; without one it cannot tell a fresh evaluation from an older one");
  if (tacticalOverlayLayerEvaluations === mark)
    throw new Error(`the overlay-layer node budget was never evaluated during this test. ${tacticalOverlayLayerSurface.layer} may have grown past its declared ceiling of ${tacticalOverlayLayerBudget.maximumOverlayDomNodes} without anything noticing. A commented-out, short-circuited, body-level-skipped or deleted call reaches this the same way -- the guard observes whether the verdict RAN, not how the call was written. It does NOT reach a test suppressed by the .skip / .fixme MODIFIERS, which Playwright applies before fixture setup so that nothing belonging to the test runs at all; those are refused by tacticalOverlayLayerRunRefusal instead. An earlier revision of this sentence claimed skipping was covered here, which was true of one form and false of the other, and that hid a real escape.`);
}

// THE RUN-LEVEL REFUSAL, BECAUSE A FIXTURE CANNOT POLICE A TEST THAT NEVER STARTS.
//
// The per-test guard above runs as part of the test's lifecycle, so it catches everything that
// happens INSIDE a running test: a commented-out call, `if (false)`, a body-level `test.skip()`
// (setup has already happened by then), a deleted call. It cannot catch the `.skip` / `.fixme`
// MODIFIERS, because Playwright suppresses the test BEFORE fixture setup -- one token,
// `overlayBudgetTest(` to `overlayBudgetTest.skip(`, and a genuine 5001-element breach reported
// `1 skipped`, exit 0, `failures="0"`.
//
// That escape stayed invisible because ONE measurement -- body-level `test.skip()` -- was
// generalised across TWO different mechanisms that look identical and are not. So this second net is
// keyed off the RUN'S OWN ACCOUNTING, which exists whether or not the test ever started.
//
// KEYED ON A TAG, NOT A TITLE. A title is prose and drifts; the tag is a declared identity that
// survives the modifier, since a skipped test is still a declared test with its tags intact.
//
// SHARD-SAFE BY CONSTRUCTION, and that is not incidental: this suite is sharded, so a rule that
// demanded the test be present in every shard would red on correct runs and would be removed. A run
// that carries NO tagged test is a shard that was not given it -- not a suppression -- and is
// allowed. A run that CARRIES one and did not execute it is refused.
export const tacticalOverlayLayerBudgetTag = "@tactical-overlay-budget";

export function tacticalOverlayLayerRunRefusal(outcomes) {
  if (!Array.isArray(outcomes))
    throw new Error("the overlay-layer run refusal needs the run's outcomes; an unreadable run is refused, not passed");
  const tagged = outcomes.filter((outcome) => (outcome?.tags ?? []).includes(tacticalOverlayLayerBudgetTag));
  // Not in this shard. The absence of the test is not evidence that it was suppressed.
  if (tagged.length === 0) return null;
  const notExecuted = tagged.filter((outcome) => outcome.status !== "passed" && outcome.status !== "failed");
  if (notExecuted.length === 0) return null;
  return `${notExecuted.length} test(s) carrying ${tacticalOverlayLayerBudgetTag} were declared in this run and never executed (${notExecuted.map((outcome) => `${outcome.title}: ${outcome.status}`).join("; ")}). The overlay-layer node budget is enforced only by running that test, so a skipped or suppressed one means ${tacticalOverlayLayerSurface.layer} was never measured against its declared ceiling of ${tacticalOverlayLayerBudget.maximumOverlayDomNodes}. A run that did not check is not a run that passed.`;
}

export function tacticalInputToPaintBudgetReason(budget, measuredMilliseconds) {
  requireFiniteMeasurement(`${budget.label} inputToPaintMilliseconds`, measuredMilliseconds);
  if (measuredMilliseconds < budget.maximumInputToPaintMilliseconds) return null;
  return `${budget.units}-unit input-to-paint budget exceeded: measured ${measuredMilliseconds} ms against the declared ceiling of ${budget.maximumInputToPaintMilliseconds} ms`;
}

export function tacticalFrameCadenceBudgetReason(budget, measuredIntervalMilliseconds) {
  requireFiniteMeasurement(`${budget.label} animationFrameIntervalMilliseconds`, measuredIntervalMilliseconds);
  const { minimumClassifiableIntervalMilliseconds: floor, intervalCeilingMilliseconds: ceiling, displayRefreshHertz: hertz, maximumElapsedVsyncsPerFrame: allowed } = tacticalFrameCadenceBudget;
  // NO ARITHMETIC HERE, DELIBERATELY. The removed defect was `measured <= ceiling + tolerance` at
  // the call site: a second, undeclared budget reached by addition instead of by a literal. Both
  // bounds are declared, and the reason names which one the measurement met.
  //
  // The floor comes FIRST, and it is a refusal rather than a pass. An interval below it cannot be a
  // single vsync at any refresh this rule classifies, so the honest answer is that the classification
  // does not apply -- not that the cadence was excellent.
  if (measuredIntervalMilliseconds < floor)
    return `${budget.units}-unit frame cadence is unclassifiable: measured ${measuredIntervalMilliseconds} ms, below the declared floor of ${floor} ms. Nearest-vsync classification holds only while the display's true frame period is at least half the ceiling and below it -- refresh in (${1000 / ceiling}, ${1000 / floor}] Hz for the declared ${hertz} Hz. Outside that window a healthy interval and a dropped one can both land inside the band, so this refuses rather than deciding.`;
  if (measuredIntervalMilliseconds < ceiling) return null;
  return `${budget.units}-unit frame cadence budget exceeded: measured ${measuredIntervalMilliseconds} ms against the declared ceiling of ${ceiling} ms. At ${hertz} Hz the frame period is ${tacticalFrameCadenceBudget.framePeriodMilliseconds} ms, so an interval at or above that ceiling has spanned more than the ${allowed} vsync this budget allows -- a dropped frame.`;
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
    ...Object.fromEntries(tacticalBudgetProjections.map((projection) => [projection.column, projection.cell(workload)])),
  }));
}
