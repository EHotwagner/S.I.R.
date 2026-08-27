import assert from "node:assert/strict";
import { execFile } from "node:child_process";
import { mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { promisify } from "node:util";
import { fileURLToPath } from "node:url";

const execFileAsync = promisify(execFile);

const args = process.argv.slice(2);
const junitIndex = args.indexOf("--junit");
const junitPath = junitIndex >= 0 ? args[junitIndex + 1] : undefined;
if (junitIndex >= 0) args.splice(junitIndex, 2);
const path = args[0] ?? "docs/sir-combat-quint-handbook-roadmap.md";
const text = await readFile(path, "utf8");

const requiredRuleRows = [
  ["CONTENT-WEAPON-RIFLE-001", "fact", "none"],
  ["CONTENT-BODY-HUMAN-001", "fact", "none"],
  ["COMBAT-ENGAGEMENT-001", "formula", "none"],
  ["COMBAT-TRACE-002", "algorithm", "none"],
  ["COMBAT-ARMOR-004", "formula", "none"],
  ["COMBAT-DAMAGE-001", "formula", "CONTENT-WEAPON-RIFLE-001, COMBAT-TRACE-002, COMBAT-ARMOR-004"],
  ["COMBAT-COLLISION-001", "transition", "COMBAT-TRACE-002"],
  ["COMBAT-COVER-003", "transition", "COMBAT-COLLISION-001"],
  ["COMBAT-PENETRATION-001", "transition", "COMBAT-COVER-003, COMBAT-ARMOR-004"],
  ["COMBAT-HEALTH-001", "transition", "COMBAT-DAMAGE-001"],
  ["COMBAT-WOUND-001", "transition", "COMBAT-HEALTH-001"],
  ["COMBAT-SUPPRESSION-001", "transition", "COMBAT-COLLISION-001"],
  ["COMBAT-SUPPRESSION-RECOVERY-001", "transition", "COMBAT-SUPPRESSION-001"],
  ["COMBAT-COLLATERAL-001", "transition", "COMBAT-COLLISION-001"],
  ["COMBAT-COVER-DESTRUCTION-001", "transition", "COMBAT-COVER-003"],
  ["COMBAT-ATTACK-RESOLUTION-001", "transition", "COMBAT-ENGAGEMENT-001, COMBAT-COLLISION-001, COMBAT-COVER-003, COMBAT-PENETRATION-001, COMBAT-DAMAGE-001, COMBAT-WOUND-001, COMBAT-SUPPRESSION-001, COMBAT-COLLATERAL-001"],
];

const requiredDeclarationRows = [
  ["type", ["RuleEntry", "AlgorithmEntry", "PropertyEntry", "Wound", "CombatState", "AttackInput", "Observation"]],
  ["variant", ["NoWound", "MinorWound", "MajorWound"]],
  ["constant/value", ["SCALE", "INT32_MIN", "INT32_MAX", "rifleDamageRaw", "humanArmorRetentionRaw", "rangeSlopeRaw", "ruleCatalogue", "traceAlgorithm", "consequenceExplanationOrder", "propertyCatalogue", "representativeAttack", "missedAttack", "alliedAttack", "initialCombat"]],
  ["pure function", ["saturateInt32", "absolute", "minimum", "maximum", "divideRoundedAwayFromZero", "fromRatio", "addFixed", "multiplyFixed", "bounded100", "retainedEffect", "preparationRaw", "validTrace", "traceRaw", "expectedDamageRaw", "roundedDamage", "woundForDamage", "validAttack", "damageForAttack", "suppressionForDamage", "nextConsequences", "consequenceObservation", "coverDamage", "nextCoverImpact", "coverObservation", "recoveredSuppression", "nextRecovery", "recoveryObservation", "fullDamageAttack"]],
  ["state variable", ["combat", "last"]],
  ["action", ["init", "resolveConsequences", "resolveCoverImpact", "resolveRecovery", "step"]],
  ["invariant/property", ["sixteenRulesDeclared", "boundedCombatState", "incapacityMatchesHealth", "destroyedCoverIsPermeable", "validTraceObservation", "suppressionRequiresDamage", "factionNeutralCollateral"]],
  ["run/witness", ["representativeDamageIsTwenty", "woundThresholdsAreExact", "zeroHealthMeansIncapacitated", "suppressionNeedsPositiveDamageAndRecoversFive", "destroyingCoverConsumesCurrentCollision", "collateralOutcomeIgnoresFaction"]],
  ["catalogue property ID", ["SixteenRulesDeclared", "BoundedCombatState", "IncapacityMatchesHealth", "DestroyedCoverIsPermeable", "ValidTraceObservation", "SuppressionRequiresDamage", "FactionNeutralCollateral"]],
];

assert.equal((text.match(/^### - \[x\] M\d/gmu) ?? []).length, 1, "only M0 may be complete");
assert.equal((text.match(/^### - \[ \] M[1-7]/gmu) ?? []).length, 7, "M1-M7 must remain unchecked");
assert.match(text, /finished publication target is `docs\/sir-combat-quint-handbook\.md`/u);
assert.doesNotMatch(text, /^# S\.I\.R\. Combat in Quint: From Design Decisions to Executable Models$/mu);

for (const heading of [
  "M0 source and authority inventory",
  "M0 sixteen-rule inventory",
  "M0 Quint declaration and property inventory",
  "M0 controlled-vocabulary inventory",
  "M0 scope exclusions",
  "M0 source disagreements",
]) assert.match(text, new RegExp(`^## ${heading}$`, "mu"), `missing ${heading}`);

const ruleTable = text.match(/## M0 sixteen-rule inventory[\s\S]*?Canonical count by kind:/u)?.[0] ?? "";
const inventoriedRows = [...ruleTable.matchAll(/^\| `([A-Z][A-Z0-9-]+)` \| ([a-z]+) \| (.*?) \|$/gmu)].map(
  ([, id, kind, dependencies]) => [id, kind, dependencies.replaceAll("`", "")],
);
assert.deepEqual(inventoriedRows, requiredRuleRows, "rule inventory must contain the exact ordered IDs, canonical kinds, and dependencies");
const inventoriedIds = inventoriedRows.map(([id]) => id);
assert.equal(new Set(inventoriedIds).size, 16, "rule inventory IDs must be unique");

const declarationTable = text.match(/## M0 Quint declaration and property inventory[\s\S]*?(?=\n## M0 controlled-vocabulary inventory)/u)?.[0] ?? "";
const inventoriedDeclarations = [...declarationTable.matchAll(/^\| ([A-Za-z/ ]+) \| (.*?) \|$/gmu)]
  .filter(([, kind]) => kind !== "Planned index kind")
  .map(([, kind, declarations]) => [kind, [...declarations.matchAll(/`([^`]+)`/gu)].map((match) => match[1])]);
assert.deepEqual(inventoriedDeclarations, requiredDeclarationRows, "declaration inventory must contain every exact categorized candidate declaration");
assert.match(declarationTable, /modules `SirCombat` and `SirCombatTests`/u, "candidate module inventory must be complete");
const declarationCount = inventoriedDeclarations.reduce((count, [, declarations]) => count + declarations.length, 0);
assert.equal(declarationCount, 79, "candidate declaration inventory must contain exactly 79 entries");

assert.match(text, /current S\.I\.R\. `origin\/main`/u);
assert.match(text, /complete Q4 model is candidate material from PR\s+#355/u);
assert.match(text, /none changes the proposed state shape or action\s+granularity/u);

if (process.env.SIR_ROADMAP_MUTATION_CHILD !== "1") {
  const mutationDirectory = await mkdtemp(join(tmpdir(), "sir-handbook-m0-"));
  const mutations = [
    ["dependency", text.replace("`CONTENT-WEAPON-RIFLE-001`, `COMBAT-TRACE-002`, `COMBAT-ARMOR-004`", "none")],
    ["declaration", text.replace("`saturateInt32`, ", "")],
  ];

  try {
    for (const [name, mutatedText] of mutations) {
      assert.notEqual(mutatedText, text, `${name} mutation must alter the roadmap`);
      const mutationPath = join(mutationDirectory, `${name}.md`);
      await writeFile(mutationPath, mutatedText, "utf8");
      await assert.rejects(
        execFileAsync(process.execPath, [fileURLToPath(import.meta.url), mutationPath], {
          env: { ...process.env, SIR_ROADMAP_MUTATION_CHILD: "1" },
        }),
        `${name} mutation must be rejected`,
      );
    }
  } finally {
    await rm(mutationDirectory, { recursive: true, force: true });
  }
}

console.log(`PASS ${path}: M0 ledger has 1 completed milestone, 7 pending milestones, ${inventoriedIds.length} exact rule rows, and ${declarationCount} exact declaration entries.`);

if (junitPath) {
  const xml = `<?xml version="1.0" encoding="utf-8"?>\n<testsuites tests="1" failures="0"><testsuite name="handbook-m0-roadmap" tests="1" failures="0"><testcase classname="SIR.Handbook" name="authority inventory ledger" /></testsuite></testsuites>\n`;
  await writeFile(junitPath, xml, "utf8");
}
