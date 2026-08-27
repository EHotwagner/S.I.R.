#!/usr/bin/env node

import { spawnSync } from "node:child_process";
import { existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

const authorityPath = "docs/rules/sir-combat.md";
const handbookPath = "docs/sir-combat-quint-handbook.md";
const receiptPath = "readiness/363-handbook-m3/handbook-m3-rules.junit.xml";
const authority = readFileSync(authorityPath, "utf8");
const handbook = readFileSync(handbookPath, "utf8");
const temporary = mkdtempSync(join(tmpdir(), "sir-handbook-m3-"));
const cases = [];

const ruleIds = [
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

const dependencies = new Map([
  ["CONTENT-WEAPON-RIFLE-001", []],
  ["CONTENT-BODY-HUMAN-001", []],
  ["COMBAT-ENGAGEMENT-001", []],
  ["COMBAT-TRACE-002", []],
  ["COMBAT-ARMOR-004", []],
  ["COMBAT-DAMAGE-001", ["CONTENT-WEAPON-RIFLE-001", "COMBAT-TRACE-002", "COMBAT-ARMOR-004"]],
  ["COMBAT-COLLISION-001", ["COMBAT-TRACE-002"]],
  ["COMBAT-COVER-003", ["COMBAT-COLLISION-001"]],
  ["COMBAT-PENETRATION-001", ["COMBAT-COVER-003", "COMBAT-ARMOR-004"]],
  ["COMBAT-HEALTH-001", ["COMBAT-DAMAGE-001"]],
  ["COMBAT-WOUND-001", ["COMBAT-HEALTH-001"]],
  ["COMBAT-SUPPRESSION-001", ["COMBAT-COLLISION-001"]],
  ["COMBAT-SUPPRESSION-RECOVERY-001", ["COMBAT-SUPPRESSION-001"]],
  ["COMBAT-COLLATERAL-001", ["COMBAT-COLLISION-001"]],
  ["COMBAT-COVER-DESTRUCTION-001", ["COMBAT-COVER-003"]],
  ["COMBAT-ATTACK-RESOLUTION-001", [
    "COMBAT-ENGAGEMENT-001",
    "COMBAT-COLLISION-001",
    "COMBAT-COVER-003",
    "COMBAT-PENETRATION-001",
    "COMBAT-DAMAGE-001",
    "COMBAT-WOUND-001",
    "COMBAT-SUPPRESSION-001",
    "COMBAT-COLLATERAL-001",
  ]],
]);

function check(name, condition, detail) {
  if (!condition) throw new Error(`${name}: ${detail}`);
  cases.push(name);
}

function count(text, needle) {
  return text.split(needle).length - 1;
}

function section(markdown, startAnchor, endAnchor) {
  const start = markdown.indexOf(`<a id="${startAnchor}"></a>`);
  const end = markdown.indexOf(`<a id="${endAnchor}"></a>`, start + 1);
  check(`section:${startAnchor}`, start >= 0 && end > start, `missing bounded section ${startAnchor}`);
  return markdown.slice(start, end);
}

function run(command, args) {
  const result = spawnSync(command, args, { encoding: "utf8" });
  const output = `${result.stdout ?? ""}${result.stderr ?? ""}`;
  if (result.status !== 0) throw new Error(`${command} ${args.join(" ")} returned ${result.status}\n${output}`);
  return output;
}

function extractAuthority(markdown) {
  const blocks = [...markdown.matchAll(/^```quint sir-combat\.qnt \+=\n([\s\S]*?)^```$/gm)].map(match => match[1]);
  check("authority-projection", blocks.length === 2, `expected two additive Quint fences, found ${blocks.length}`);
  return blocks.join("");
}

function auditCompleteRules(candidate) {
  const errors = [];
  const reference = section(candidate, "chapter-44-complete-rule-reference", "chapter-45-quint-declaration-reference");
  const traceability = section(candidate, "chapter-46-traceability-matrix", "chapter-47-command-reference");
  for (const id of ruleIds) {
    const link = `[${id}](#rule-${id.toLowerCase()})`;
    if (count(reference, `| ${link} |`) !== 1) errors.push(`reference:${id}`);
    const row = traceability.split("\n").find(line => line.startsWith(`| S.I.R. combat registry | ${link} |`));
    if (!row) errors.push(`traceability:${id}`);
    else if (row.includes("| Pending |")) errors.push(`traceability-pending:${id}`);
    const indexEntry = `<a id="rule-${id.toLowerCase()}"></a>\n**${id}** — rule.`;
    if (count(candidate, indexEntry) !== 1) errors.push(`index:${id}`);
  }
  for (const chapter of [
    "chapter-07-the-sixteen-rule-catalogue",
    "chapter-08-rule-dependency-and-explanation-order-maps",
    "chapter-15-the-external-line-of-sight-contract-boundary",
    "chapter-16-atomic-aggregate-consequences-versus-focused-pur",
    "chapter-25-a-miss-causes-neither-damage-nor-suppression",
    "chapter-26-wound-thresholds-at-damage-24-25-and-50",
    "chapter-27-health-reaching-zero-and-incapacitation",
    "chapter-28-cover-impact-destruction-permeability-and-the-cu",
    "chapter-29-suppression-eligibility-and-five-point-recovery",
    "chapter-30-faction-neutral-collateral-consequences",
    "chapter-31-registered-external-line-of-sight-behavior",
    "chapter-44-complete-rule-reference",
    "chapter-49-exercises-and-solutions",
  ]) {
    const body = section(candidate, chapter, chapter === "chapter-49-exercises-and-solutions" ? "chapter-50-alphabetical-definition-index" : nextChapterAnchor(chapter));
    if (body.includes("*Scheduled content:*")) errors.push(`placeholder:${chapter}`);
  }
  return errors;
}

function nextChapterAnchor(anchor) {
  const match = anchor.match(/^chapter-(\d+)-/);
  if (!match) throw new Error(`cannot derive next chapter from ${anchor}`);
  const next = String(Number(match[1]) + 1).padStart(2, "0");
  const found = [...handbook.matchAll(/<a id="(chapter-[^"]+)"><\/a>/g)]
    .map(match => match[1])
    .find(value => value.startsWith(`chapter-${next}-`));
  if (!found) throw new Error(`missing next chapter after ${anchor}`);
  return found;
}

try {
  const model = extractAuthority(authority);
  const modelPath = join(temporary, "sir-combat.qnt");
  writeFileSync(modelPath, model);

  const catalogueBlock = model.slice(model.indexOf("pure val ruleCatalogue"), model.indexOf("pure val traceAlgorithm"));
  const declared = [...catalogueBlock.matchAll(/id: "([A-Z0-9-]+)"/g)].map(match => match[1]);
  check("sixteen-unique-rule-ids", declared.length === 16 && new Set(declared).size === 16, `found ${declared.length} entries and ${new Set(declared).size} unique ids`);
  check("rule-id-set", ruleIds.every(id => declared.includes(id)) && declared.every(id => ruleIds.includes(id)), "catalogue ids differ from the stable sixteen-rule set");
  for (const [id, direct] of dependencies) {
    const row = catalogueBlock.split("\n").find(line => line.includes(`id: "${id}"`));
    check(`dependency-row:${id}`, Boolean(row), "catalogue row missing");
    for (const dependency of direct) check(`dependency:${id}:${dependency}`, row.includes(`"${dependency}"`), "direct dependency missing from authority row");
    for (const target of ruleIds.filter(candidate => !direct.includes(candidate) && candidate !== id)) {
      check(`dependency-excludes:${id}:${target}`, !row.includes(`"${target}"`), "unexpected dependency in authority row");
    }
  }

  const shown = [...handbook.matchAll(/^```quint authority=sir-combat\n([\s\S]*?)^```$/gm)].map(match => match[1]);
  check("authority-excerpts", shown.length >= 12, `expected at least twelve exact authority excerpts, found ${shown.length}`);
  for (const [index, excerpt] of shown.entries()) check(`excerpt-${index + 1}`, model.includes(excerpt), "handbook fence is not an exact authority substring");

  for (const marker of [
    "Wound thresholds at damage 24, 25, and 50",
    "current projectile stops",
    "formula/observation inside atomic aggregate",
    "FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover",
    "#### Beginner — predict one helper or completed successor",
    "#### Intermediate — read dependencies and observations",
    "#### Advanced — design within the authority boundary",
  ]) check(`teaching:${marker}`, handbook.includes(marker), `missing teaching marker: ${marker}`);

  const completeErrors = auditCompleteRules(handbook);
  check("complete-rule-reference-and-traceability", completeErrors.length === 0, completeErrors.join(", "));
  const missingReference = handbook.replace(`| [${ruleIds[0]}](#rule-${ruleIds[0].toLowerCase()}) | fact; none |`, `| removed-rule | fact; none |`);
  check("negative-control-missing-reference", auditCompleteRules(missingReference).includes(`reference:${ruleIds[0]}`), "reference deletion was not detected");
  const pendingTrace = handbook.replace("| stable registry/weapon fact; full M5 map pending |", "| Pending |");
  check("negative-control-pending-traceability", auditCompleteRules(pendingTrace).includes(`traceability-pending:${ruleIds[0]}`), "pending stable-rule traceability was not detected");

  check("pinned-quint", run("quint", ["--version"]).trim() === "0.32.0", "expected Quint 0.32.0");
  run("quint", ["typecheck", modelPath]);
  cases.push("authoritative-typecheck");
  for (const namedRun of [
    "representativeDamageIsTwenty",
    "woundThresholdsAreExact",
    "zeroHealthMeansIncapacitated",
    "suppressionNeedsPositiveDamageAndRecoversFive",
    "destroyingCoverConsumesCurrentCollision",
    "collateralOutcomeIgnoresFaction",
  ]) {
    const output = run("quint", ["test", modelPath, "--main", "SirCombatTests", "--backend", "rust", "--seed", "352", "--match", namedRun, "--verbosity", "3"]);
    check(`run:${namedRun}`, output.includes("1 passing"), `${namedRun} did not report one passing test`);
  }

  if (process.argv.includes("--require-rendered")) {
    const renderedPath = "artifacts/site/sir-combat-quint-handbook.html";
    check("rendered-handbook", existsSync(renderedPath), `missing ${renderedPath}`);
    const rendered = readFileSync(renderedPath, "utf8");
    check("rendered-m3-markers", rendered.includes("sixteen-rule catalogue") && rendered.includes("Advanced — design within the authority boundary"), "rendered page omits M3 markers");
  }

  mkdirSync("readiness/363-handbook-m3", { recursive: true });
  const xmlCases = cases.map(name => `  <testcase classname="SIR.HandbookM3" name="${name.replaceAll("&", "and").replaceAll('"', "'")}"/>`).join("\n");
  writeFileSync(receiptPath, `<?xml version="1.0" encoding="UTF-8"?>\n<testsuite name="sir-combat-quint-handbook-m3" tests="${cases.length}" failures="0" errors="0" skipped="0">\n${xmlCases}\n</testsuite>\n`);
  console.log(`handbook-m3-rules: PASS (${ruleIds.length}/16 complete; ${shown.length} exact excerpts; ${cases.length} checks; 6 focused runs)`);
} finally {
  rmSync(temporary, { recursive: true, force: true });
}
