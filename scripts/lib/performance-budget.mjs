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
// production route. 16.67 ms is one frame period at 60 Hz. It is explicitly NOT a compositor, paint,
// GPU or swapchain claim.
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
