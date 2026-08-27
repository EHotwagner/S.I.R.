#!/usr/bin/env node

import { spawnSync } from "node:child_process";
import { existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

const authorityPath = "docs/rules/sir-combat.md";
const handbookPath = "docs/sir-combat-quint-handbook.md";
const receiptPath = "readiness/365-handbook-m4/formal-reasoning.junit.xml";
const authority = readFileSync(authorityPath, "utf8");
const handbook = readFileSync(handbookPath, "utf8");
const temporary = mkdtempSync(join(tmpdir(), "sir-handbook-m4-"));
const cases = [];

function check(name, condition, detail) {
  if (!condition) throw new Error(`${name}: ${detail}`);
  cases.push(name);
}

function section(markdown, startAnchor, endAnchor) {
  const start = markdown.indexOf(`<a id="${startAnchor}"></a>`);
  const end = markdown.indexOf(`<a id="${endAnchor}"></a>`, start + 1);
  check(`section:${startAnchor}`, start >= 0 && end > start, `missing bounded section ${startAnchor}`);
  return markdown.slice(start, end);
}

function extractAuthority(markdown) {
  const blocks = [...markdown.matchAll(/^```quint sir-combat\.qnt \+=\n([\s\S]*?)^```$/gm)].map(match => match[1]);
  check("authority-projection", blocks.length === 2, `expected two additive Quint fences, found ${blocks.length}`);
  return blocks.join("");
}

function run(command, args, expectedStatus = 0) {
  const result = spawnSync(command, args, { encoding: "utf8" });
  const output = `${result.stdout ?? ""}${result.stderr ?? ""}`;
  if (expectedStatus === 0 && result.status !== 0) {
    throw new Error(`${command} ${args.join(" ")} returned ${result.status}\n${output}`);
  }
  if (expectedStatus === "nonzero" && result.status === 0) {
    throw new Error(`${command} ${args.join(" ")} unexpectedly passed\n${output}`);
  }
  return { status: result.status, output };
}

function replaceUnique(text, needle, replacement, mutation) {
  const count = text.split(needle).length - 1;
  check(`mutation-subject:${mutation}`, count === 1, `${mutation} expected one subject, found ${count}`);
  return text.replace(needle, replacement);
}

function detectorArgs(path, detector) {
  return ["test", path, "--main", detector.module, "--backend", "rust", "--seed", "365", "--match", detector.run, "--verbosity", "3"];
}

const detectorModule = `
module SirCombatM4Detectors {
  import SirCombat.*

  run boundedInitialState = init.expect(boundedCombatState)
}
`;

try {
  const model = extractAuthority(authority);
  const cleanPath = join(temporary, "sir-combat-clean.qnt");
  writeFileSync(cleanPath, model + detectorModule);
  check("pinned-quint", run("quint", ["--version"]).output.trim() === "0.32.0", "expected Quint 0.32.0");
  run("quint", ["typecheck", cleanPath]);
  cases.push("authoritative-typecheck");

  const part = section(handbook, "chapter-32-choosing-an-example-witness-or-invariant", "part-vii");
  check("m4-no-placeholders", !part.includes("*Scheduled content:*"), "M4 chapter placeholder remains");
  for (const marker of [
    "Example — one concrete calculation",
    "Reachable [witness](#def-witness) — an existential execution",
    "[Invariant](#def-invariant) — a predicate over every checked state",
    "[Sampled execution](#def-sampled-run) — search evidence, not proof",
    "[Bounded exhaustive verification](#def-bounded-verification) — exhaustive only inside the declared bounds",
    "[Counterexample](#def-counterexample) — one concrete refutation",
    "No sampled trace is the canonical",
    "earliest bad state",
    "is not evidence until the unchanged detector returns green",
    "A green bounded check is not an unbounded theorem",
  ]) check(`teaching:${marker}`, part.includes(marker), `missing teaching marker: ${marker}`);

  const mutationRows = new Map([
    ["threshold", "woundThresholdsAreExact"],
    ["bounds", "boundedInitialState"],
    ["suppression", "suppressionNeedsPositiveDamageAndRecoversFive"],
    ["cover", "destroyingCoverConsumesCurrentCollision"],
    ["collateral", "collateralOutcomeIgnoresFaction"],
    ["catalogue integrity", "representativeDamageIsTwenty"],
  ]);
  for (const [family, detector] of mutationRows) {
    check(`mutation-doc:${family}`, part.includes(family) && part.includes(detector), `missing ${family} detector row`);
  }

  const invariantBindings = new Map([
    ["sixteenRulesDeclared", ["ruleCatalogue"]],
    ["boundedCombatState", ["combat.health", "combat.suppression", "combat.coverIntegrity"]],
    ["incapacityMatchesHealth", ["combat.incapacitated", "combat.health"]],
    ["destroyedCoverIsPermeable", ["combat.coverIntegrity", "combat.coverBlocking"]],
    ["validTraceObservation", ["last.lastAction", "last.traceRaw"]],
    ["suppressionRequiresDamage", ["last.lastAction", "last.damage", "last.suppressionDelta"]],
    ["factionNeutralCollateral", ["nextConsequences", "initialCombat"]],
  ]);
  for (const [property, fields] of invariantBindings) {
    const authorityLine = model.split("\n").find(line => line.includes(`val ${property} =`)) ?? "";
    check(`property-authority:${property}`, authorityLine.length > 0, `${property} missing from authority`);
    check(`property-handbook:${property}`, part.includes(property), `${property} missing from binding table`);
    for (const field of fields) {
      const authoritative = model.includes(field);
      check(`binding:${property}:${field}`, authoritative && part.includes(field), `${property} binding ${field} is not authority-backed and documented`);
    }
  }

  const actionWitnesses = [
    { action: "resolveConsequences", module: "SirCombatTests", run: "representativeDamageIsTwenty" },
    { action: "resolveCoverImpact", module: "SirCombatTests", run: "destroyingCoverConsumesCurrentCollision" },
    { action: "resolveRecovery", module: "SirCombatTests", run: "suppressionNeedsPositiveDamageAndRecoversFive" },
  ];
  for (const witness of actionWitnesses) {
    const result = run("quint", detectorArgs(cleanPath, witness));
    check(`reachable:${witness.action}`, result.output.includes("1 passing"), `${witness.run} did not pass`);
    check(`reachable-doc:${witness.action}`, part.includes(witness.action) && part.includes(witness.run), "witness mapping absent from handbook");
  }

  const mutations = [
    {
      family: "threshold",
      detector: { module: "SirCombatTests", run: "woundThresholdsAreExact" },
      apply: text => replaceUnique(text, "if (damage >= 50) MajorWound else if (damage >= 25) MinorWound else NoWound", "if (damage >= 50) MajorWound else if (damage > 25) MinorWound else NoWound", "threshold"),
    },
    {
      family: "bounds",
      detector: { module: "SirCombatM4Detectors", run: "boundedInitialState" },
      apply: text => replaceUnique(text, "    health: 100,\n    suppression: 0,\n    coverIntegrity: 100,", "    health: 101,\n    suppression: 0,\n    coverIntegrity: 100,", "bounds"),
    },
    {
      family: "suppression",
      detector: { module: "SirCombatTests", run: "suppressionNeedsPositiveDamageAndRecoversFive" },
      apply: text => replaceUnique(text, "if (damage > 0) maximum(0, requestedDelta) else 0", "maximum(0, requestedDelta)", "suppression"),
    },
    {
      family: "cover",
      detector: { module: "SirCombatTests", run: "destroyingCoverConsumesCurrentCollision" },
      apply: text => replaceUnique(text, "coverBlocking: if (remaining == 0) false else current.coverBlocking", "coverBlocking: if (remaining == 0) true else current.coverBlocking", "cover"),
    },
    {
      family: "collateral",
      detector: { module: "SirCombatTests", run: "collateralOutcomeIgnoresFaction" },
      apply: text => replaceUnique(text, "    val damage = damageForAttack(input)\n    val nextHealth = bounded100(current.health - damage)", "    val damage = if (input.attackerFaction == input.targetFaction) 0 else damageForAttack(input)\n    val nextHealth = bounded100(current.health - damage)", "collateral"),
    },
    {
      family: "catalogue-integrity",
      detector: { module: "SirCombatTests", run: "representativeDamageIsTwenty" },
      apply: text => replaceUnique(text, "    { id: \"CONTENT-WEAPON-RIFLE-001\", kind: \"fact\", dependencies: Set(), reads: Set(), effects: Set(), events: List() },\n", "", "catalogue-integrity"),
    },
  ];

  for (const mutation of mutations) {
    const mutantPath = join(temporary, `sir-combat-${mutation.family}.qnt`);
    writeFileSync(mutantPath, mutation.apply(model) + detectorModule);
    const red = run("quint", detectorArgs(mutantPath, mutation.detector), "nonzero");
    check(`observed-red:${mutation.family}`, red.output.includes(mutation.detector.run), `${mutation.detector.run} was not named in red output`);
    const restored = run("quint", detectorArgs(cleanPath, mutation.detector));
    check(`restored-green:${mutation.family}`, restored.output.includes("1 passing"), `${mutation.detector.run} did not restore green`);
  }

  if (process.argv.includes("--require-rendered")) {
    const renderedPath = "artifacts/site/sir-combat-quint-handbook.html";
    check("rendered-handbook", existsSync(renderedPath), `missing ${renderedPath}`);
    const rendered = readFileSync(renderedPath, "utf8");
    check("rendered-m4-markers", rendered.includes("No sampled trace is the canonical") && rendered.includes("catalogue integrity"), "rendered handbook omits M4 markers");
  }

  mkdirSync("readiness/365-handbook-m4", { recursive: true });
  const xmlCases = cases.map(name => `  <testcase classname="SIR.HandbookM4" name="${name.replaceAll("&", "and").replaceAll('"', "'")}"/>`).join("\n");
  writeFileSync(receiptPath, `<?xml version="1.0" encoding="UTF-8"?>\n<testsuite name="sir-combat-quint-handbook-m4" tests="${cases.length}" failures="0" errors="0" skipped="0">\n${xmlCases}\n</testsuite>\n`);
  console.log(`handbook-m4 formal reasoning: PASS (${mutations.length} observed-red/restored-green mutation pairs; ${actionWitnesses.length} major-action witnesses; ${invariantBindings.size} property bindings; ${cases.length} checks)`);
} finally {
  rmSync(temporary, { recursive: true, force: true });
}
