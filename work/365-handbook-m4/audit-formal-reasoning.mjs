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

function propertyExpression(text, property) {
  const prefix = `  val ${property} =`;
  const start = text.indexOf(prefix);
  if (start < 0) throw new Error(`${property} missing from authority`);
  const expressionStart = start + prefix.length;
  const remainder = text.slice(expressionStart);
  const nextDefinition = remainder.search(/\n  (?:(?:pure )?(?:val|def)|action|run|var|const|type|assume)\b|\n}\s*$/m);
  if (nextDefinition < 0) throw new Error(`${property} has no bounded definition end`);
  return {
    start: expressionStart,
    end: expressionStart + nextDefinition,
    text: remainder.slice(0, nextDefinition),
  };
}

function bindingDetector(text, property, field) {
  return propertyExpression(text, property).text.includes(field);
}

function detectorArgs(path, detector) {
  return ["test", path, "--main", detector.module, "--backend", "rust", "--seed", "365", "--match", detector.run, "--verbosity", "3"];
}

const detectorModule = `
module SirCombatM4Detectors {
  import SirCombat.*

  run boundedInitialState = init.expect(boundedCombatState)

  run resolveConsequencesReachability =
    init
      .then(resolveConsequences(representativeAttack))
      .expect(and {
        last.lastAction == "ResolveConsequences",
        combat.health == 80,
        combat.suppression == 12,
      })

  run resolveCoverImpactReachability =
    init
      .then(resolveCoverImpact(250, true, true, "cover:destroy"))
      .expect(and {
        last.lastAction == "ResolveCoverImpact",
        combat.coverIntegrity == 0,
        not(combat.coverBlocking),
      })

  run resolveRecoveryReachability =
    init
      .then(resolveConsequences(representativeAttack))
      .expect(combat.suppression == 12)
      .then(resolveRecovery("recovery:target"))
      .expect(and {
        last.lastAction == "ResolveRecovery",
        combat.suppression == 7,
      })
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
  for (const stale of ["belong to M4", "remains M4", "M4 extends this pattern"]) {
    check(`m4-deferral-discharged:${stale}`, !handbook.includes(stale), `stale M4 deferral remains: ${stale}`);
  }

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
  const bindingMutationCount = [...invariantBindings.values()].reduce((total, fields) => total + fields.length, 0);
  for (const [property, fields] of invariantBindings) {
    const expression = propertyExpression(model, property).text;
    check(`property-authority:${property}`, expression.length > 0, `${property} missing from authority`);
    check(`property-handbook:${property}`, part.includes(property), `${property} missing from binding table`);
    for (const field of fields) {
      const authoritative = bindingDetector(model, property, field);
      check(`binding:${property}:${field}`, authoritative && part.includes(field), `${property} binding ${field} is not authority-backed and documented`);
    }
  }

  for (const [property, fields] of invariantBindings) {
    for (const field of fields) {
      const subject = propertyExpression(model, property);
      const occurrences = subject.text.split(field).length - 1;
      check(`binding-mutation-subject:${property}:${field}`, occurrences > 0, `${property} does not bind ${field}`);
      const mutantExpression = subject.text.replaceAll(field, "M4_REMOVED_BINDING");
      const mutant = model.slice(0, subject.start) + mutantExpression + model.slice(subject.end);
      check(`binding-observed-red:${property}:${field}`, !bindingDetector(mutant, property, field), `binding detector did not reject ${property}:${field}`);
      check(`binding-restored-green:${property}:${field}`, bindingDetector(model, property, field), `binding detector did not restore ${property}:${field}`);
    }
  }

  const actionWitnesses = [
    {
      action: "resolveConsequences",
      witness: "representativeDamageIsTwenty",
      detector: { module: "SirCombatM4Detectors", run: "resolveConsequencesReachability" },
      identitySubject: 'lastAction: "ResolveConsequences"',
      deltaSubject: "combat' = nextConsequences(combat, input)",
    },
    {
      action: "resolveCoverImpact",
      witness: "destroyingCoverConsumesCurrentCollision",
      detector: { module: "SirCombatM4Detectors", run: "resolveCoverImpactReachability" },
      identitySubject: 'lastAction: "ResolveCoverImpact"',
      deltaSubject: "combat' = nextCoverImpact(combat, baseDamage)",
    },
    {
      action: "resolveRecovery",
      witness: "suppressionNeedsPositiveDamageAndRecoversFive",
      detector: { module: "SirCombatM4Detectors", run: "resolveRecoveryReachability" },
      identitySubject: 'lastAction: "ResolveRecovery"',
      deltaSubject: "combat' = nextRecovery(combat)",
    },
  ];
  for (const witness of actionWitnesses) {
    const result = run("quint", detectorArgs(cleanPath, witness.detector));
    check(`reachable:${witness.action}`, result.output.includes("1 passing"), `${witness.detector.run} did not pass`);
    check(`reachable-doc:${witness.action}`, part.includes(witness.action) && part.includes(witness.witness), "witness mapping absent from handbook");
  }

  const actionWitnessMutations = actionWitnesses.flatMap(witness => [
    {
      family: `${witness.action}:identity`,
      detector: witness.detector,
      apply: text => replaceUnique(text, witness.identitySubject, 'lastAction: "M4WrongAction"', `${witness.action}:identity`),
    },
    {
      family: `${witness.action}:delta`,
      detector: witness.detector,
      apply: text => replaceUnique(text, witness.deltaSubject, "combat' = combat", `${witness.action}:delta`),
    },
  ]);
  for (const mutation of actionWitnessMutations) {
    const mutantPath = join(temporary, `sir-combat-${mutation.family.replaceAll(":", "-")}.qnt`);
    writeFileSync(mutantPath, mutation.apply(model) + detectorModule);
    const red = run("quint", detectorArgs(mutantPath, mutation.detector), "nonzero");
    check(`action-observed-red:${mutation.family}`, red.output.includes(mutation.detector.run), `${mutation.detector.run} was not named in red output`);
    const restored = run("quint", detectorArgs(cleanPath, mutation.detector));
    check(`action-restored-green:${mutation.family}`, restored.output.includes("1 passing"), `${mutation.detector.run} did not restore green`);
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
  console.log(`handbook-m4 formal reasoning: PASS (${mutations.length} semantic mutation pairs; ${actionWitnessMutations.length} action-witness mutation pairs; ${bindingMutationCount} binding mutation pairs; ${actionWitnesses.length} major-action witnesses; ${invariantBindings.size} property bindings; ${cases.length} checks)`);
} finally {
  rmSync(temporary, { recursive: true, force: true });
}
