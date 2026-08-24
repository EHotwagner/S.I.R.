import { createRequire } from "node:module";
import {
  tacticalOverlayLayerConfigFilterPatterns,
  tacticalOverlayLayerFilterRefusal,
  tacticalOverlayLayerRunRefusal,
} from "../../scripts/lib/performance-budget.mjs";

// PLAYWRIGHT PARSES AND COMPILES ITS OWN FILTERS. WE ASK IT; WE DO NOT REPRODUCE IT.
//
// An earlier revision walked argv by hand and called `new RegExp(value)`. That was a second,
// hand-authored representation of a fact Playwright already holds -- S.I.R.#334's defect installed in
// the checker meant to catch it -- and it disagreed with the original three ways, each shipping a
// forced breach at exit 0: it did not know the documented `-G` alias, it was case-SENSITIVE where
// `forceRegExp` applies `gi`, and it treated `/…/` literally where `forceRegExp` unwraps it.
//
// Both facts are derived from Playwright itself: the flag names from its registered `test` command,
// and the compilation from the same `forceRegExp` its CLI uses. A drift in either reds
// scripts/test-svg-pipeline-measurement.mjs, which asserts this equivalence rather than assuming it.
const require = createRequire(import.meta.url);
// Resolved through the package root: `playwright/lib/*` is not in the package's `exports` map, so a
// bare specifier is refused. The root is asked for rather than assumed, so this follows the install
// Playwright actually resolves to rather than a path guessed from this file's location.
const playwrightLib = (name) => require(require.resolve(`playwright/package.json`).replace(/package\.json$/, `lib/${name}`));
const { forceRegExp } = playwrightLib("util.js");
const { program } = playwrightLib("program.js");

// THE ARGV GRAMMAR IS THEIRS TOO. WE RUN THEIR PARSER; WE DO NOT DESCRIBE IT.
//
// Deriving the flag names and the compiler still left a THIRD hand-written representation: the
// grammar itself. Recognising only `flag`, `flag=value` and `flag value` is a description of
// commander, and commander admits more than that -- attached short values (`-Gpattern`), clustered
// shorts (`-xGpattern`), a literal `=` after a SHORT flag (which is part of the value, not a
// separator), and last-wins on repetition. Each divergence was a real escape or a real false red.
//
// A list of argv forms cannot fix this, and adding one would be the fourth round of the same move.
// So the grammar is not described here at all: a fresh command is built from Playwright's own
// registered `test` options and asked to parse. What it returns IS what Playwright's CLI sees.
const playwrightTestCommand = (() => {
  const test = program.commands?.find((command) => command.name() === "test");
  if (!test?.options?.length)
    throw new Error("Playwright no longer registers a `test` command with options, so this filter check cannot parse a command line the way Playwright does. Refusing to guess at its grammar.");
  return test;
})();

export const grepInvertFlagNames = (() => {
  const option = playwrightTestCommand.options.find((candidate) => candidate.long === "--grep-invert");
  if (!option?.long)
    throw new Error("Playwright no longer registers a --grep-invert option under its `test` command. Refusing to guess.");
  return [option.long, option.short].filter(Boolean);
})();

// Returns exactly what Playwright's CLI would put in `grepInvert` for these user arguments, including
// last-wins on repetition -- which a collector that gathered every occurrence got wrong in the other
// direction, refusing a run whose earlier pattern was overridden and never applied.
export function playwrightGrepInvertFor(userArguments) {
  const command = playwrightTestCommand.createCommand("test");
  command.exitOverride();
  command.configureOutput({ writeOut() {}, writeErr() {} });
  command.allowUnknownOption(true).allowExcessArguments(true);
  for (const option of playwrightTestCommand.options) command.addOption(option);
  command.action(() => {});
  try {
    command.parse(userArguments, { from: "user" });
  } catch {
    // Playwright's own parser rejected these arguments. Whether the budget test survives is then
    // unknowable rather than fine, so this is surfaced as a refusal rather than as "no filter".
    return { unparseable: true };
  }
  return { value: command.opts().grepInvert };
}

export function tacticalOverlayLayerCommandLineFilterPatterns(argv) {
  // `from: "user"` wants the arguments after the executable and script; a leading `test` subcommand
  // is tolerated as an excess argument rather than stripped by hand.
  const parsed = playwrightGrepInvertFor(argv.slice(2));
  if (parsed.unparseable) return [{ source: grepInvertFlagNames[0], value: argv.slice(2).join(" "), unparseable: true }];
  if (parsed.value === undefined || parsed.value === null) return [];
  try {
    return [{ source: grepInvertFlagNames[0], value: parsed.value, expression: forceRegExp(parsed.value) }];
  } catch {
    return [{ source: grepInvertFlagNames[0], value: parsed.value, unparseable: true }];
  }
}

export function tacticalOverlayLayerFilterPatterns(config, argv) {
  return [...tacticalOverlayLayerConfigFilterPatterns(config), ...tacticalOverlayLayerCommandLineFilterPatterns(argv)];
}

// A RUN THAT DID NOT CHECK IS NOT A RUN THAT PASSED.
//
// Some obligations cannot be enforced from inside the test that carries them, because a test can be
// suppressed before any of its own code runs. `test.skip` / `test.fixme` as MODIFIERS do exactly
// that: Playwright never starts the test, so no fixture, hook or assertion belonging to it can
// object. One token then turns a real breach into `1 skipped`, exit 0, `failures="0"` -- and nothing
// downstream refuses a skipped browser test, so a reader sees nothing unusual (S.I.R.#327).
//
// This reporter is the accounting that exists whether or not a test ever started. It is deliberately
// ignorant of what the obligation IS: it hands the run's outcomes to the declaration and reports
// whatever the declaration refuses.
export default class DeclaredBudgetExecutionReporter {
  #outcomes = [];

  #filterRefusal = null;

  // This reporter is an ADDITIONAL net, not a replacement for the run's normal output. Playwright
  // suppresses its default terminal reporter when any configured reporter claims stdio, and the
  // default for this hook is `true` -- so omitting it silently removed the "N passed" summary.
  printsToStdio() {
    return false;
  }

  onBegin(config) {
    // No sibling test is consulted. The budget test's identity is declared, so this does not depend on
    // any test surviving the very filter it is judging -- which is how an earlier version opened an
    // escape hatch on exactly the input it existed to catch.
    this.#filterRefusal = tacticalOverlayLayerFilterRefusal(tacticalOverlayLayerFilterPatterns(config, process.argv), config);
  }

  onTestEnd(test, result) {
    this.#outcomes.push({ title: test.title, tags: test.tags ?? [], status: result.status });
  }

  onEnd() {
    const refusal = this.#filterRefusal ?? tacticalOverlayLayerRunRefusal(this.#outcomes);
    if (!refusal) return undefined;
    console.error(`\n[declared-budget] ${refusal}`);
    // Both, deliberately. The returned status is the documented channel; the exit code is what CI and
    // `test-browser-shards.mjs` actually read, and a refusal that does not change the exit code is a
    // refusal nothing downstream can see.
    process.exitCode = 1;
    return { status: "failed" };
  }
}
