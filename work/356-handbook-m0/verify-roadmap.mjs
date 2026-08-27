import assert from "node:assert/strict";
import { readFile, writeFile } from "node:fs/promises";

const args = process.argv.slice(2);
const junitIndex = args.indexOf("--junit");
const junitPath = junitIndex >= 0 ? args[junitIndex + 1] : undefined;
if (junitIndex >= 0) args.splice(junitIndex, 2);
const path = args[0] ?? "docs/sir-combat-quint-handbook-roadmap.md";
const text = await readFile(path, "utf8");

const requiredRuleIds = [
  "CONTENT-WEAPON-RIFLE-001",
  "CONTENT-BODY-HUMAN-001",
  "COMBAT-ENGAGEMENT-001",
  "COMBAT-TRACE-002",
  "COMBAT-ARMOR-004",
  "COMBAT-DAMAGE-001",
  "COMBAT-COLLISION-001",
  "COMBAT-COVER-003",
  "COMBAT-PENETRATION-001",
  "COMBAT-HEALTH-001",
  "COMBAT-WOUND-001",
  "COMBAT-SUPPRESSION-001",
  "COMBAT-SUPPRESSION-RECOVERY-001",
  "COMBAT-COLLATERAL-001",
  "COMBAT-COVER-DESTRUCTION-001",
  "COMBAT-ATTACK-RESOLUTION-001",
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

const ruleTable = text.match(/## M0 sixteen-rule inventory[\s\S]*?Count by kind:/u)?.[0] ?? "";
const inventoriedIds = [...ruleTable.matchAll(/^\| `([A-Z][A-Z0-9-]+)` \|/gmu)].map((match) => match[1]);
assert.deepEqual(inventoriedIds, requiredRuleIds, "rule inventory must contain the exact ordered sixteen-rule registry");
assert.equal(new Set(inventoriedIds).size, 16, "rule inventory IDs must be unique");

assert.match(text, /current S\.I\.R\. `origin\/main`/u);
assert.match(text, /complete Q4 model is candidate material from PR\s+#355/u);
assert.match(text, /none changes the proposed state shape or action\s+granularity/u);

console.log(`PASS ${path}: M0 ledger has 1 completed milestone, 7 pending milestones, and ${inventoriedIds.length} unique rules.`);

if (junitPath) {
  const xml = `<?xml version="1.0" encoding="utf-8"?>\n<testsuites tests="1" failures="0"><testsuite name="handbook-m0-roadmap" tests="1" failures="0"><testcase classname="SIR.Handbook" name="authority inventory ledger" /></testsuite></testsuites>\n`;
  await writeFile(junitPath, xml, "utf8");
}
