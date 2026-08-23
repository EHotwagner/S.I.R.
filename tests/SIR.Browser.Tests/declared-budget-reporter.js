import { tacticalOverlayLayerFilterRefusal, tacticalOverlayLayerRunRefusal } from "../../scripts/lib/performance-budget.mjs";

// A RUN THAT DID NOT CHECK IS NOT A RUN THAT PASSED.
//
// Some obligations cannot be enforced from inside the test that carries them, because a test can be
// suppressed before any of its own code runs. `test.skip` / `test.fixme` as MODIFIERS do exactly
// that: Playwright never starts the test, so no fixture, hook or assertion belonging to it can
// object. One token then turns a real breach into `1 skipped`, exit 0, `failures="0"` -- and nothing
// downstream refuses a skipped browser test (`browser-junit.mjs` only counts them, and this suite
// already carries one permanent skip), so a reader sees nothing unusual (S.I.R.#327).
//
// A per-test guard and this are two different nets over two different mechanisms, and the reason
// this file exists is that they were once assumed to be one: a single measurement of a BODY-LEVEL
// `test.skip()` -- which the per-test guard does catch, because setup has already happened by then
// -- was generalised to the modifier, which it cannot catch at all.
//
// This reporter is the accounting that exists whether or not a test ever started. It is deliberately
// ignorant of what the obligation IS: it hands the run's outcomes to the declaration and reports
// whatever the declaration refuses. Adding a second budget with the same property needs no change
// here.
export default class DeclaredBudgetExecutionReporter {
  #outcomes = [];

  // This reporter is an ADDITIONAL net, not a replacement for the run's normal output. Playwright
  // suppresses its default terminal reporter when any configured reporter claims stdio, and the
  // default for this hook is `true` -- so omitting it silently removed the "N passed" summary that
  // both CI logs and a human reader rely on. It reports nothing unless it refuses.
  printsToStdio() {
    return false;
  }

  #filterRefusal = null;

  // The run's own filter, read once it is resolved. `--grep-invert` on the budget tag removes the
  // test entirely, which afterwards is indistinguishable from a shard that never carried it -- the
  // allowance that makes this reporter shard-safe is the same allowance that hides this edit.
  onBegin(config) {
    this.#filterRefusal = tacticalOverlayLayerFilterRefusal(config);
  }

  onTestEnd(test, result) {
    this.#outcomes.push({ title: test.title, tags: test.tags ?? [], status: result.status });
  }

  onEnd() {
    const refusal = this.#filterRefusal ?? tacticalOverlayLayerRunRefusal(this.#outcomes);
    if (!refusal) return undefined;
    console.error(`\n[declared-budget] ${refusal}`);
    // Both, deliberately. The returned status is the documented channel; the exit code is what CI
    // and `test-browser-shards.mjs` actually read, and a refusal that does not change the exit code
    // is a refusal nothing downstream can see.
    process.exitCode = 1;
    return { status: "failed" };
  }
}
