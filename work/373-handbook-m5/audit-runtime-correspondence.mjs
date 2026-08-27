#!/usr/bin/env node

import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";

const handbookPath = "docs/sir-combat-quint-handbook.md";
const runtimePath = "src/SIR.Simulation/CombatRules.fs";
const adapterPath = "tests/SIR.Conformance.Shared/QuintQ4ReplayFixtures.fs";
const qualifierPath = "scripts/qualify-quint-q4-sir-combat.sh";
const receiptPath = "readiness/373-handbook-m5/runtime-correspondence.junit.xml";
const handbook = readFileSync(handbookPath, "utf8");
const runtime = readFileSync(runtimePath, "utf8");
const adapter = readFileSync(adapterPath, "utf8");
const qualifier = readFileSync(qualifierPath, "utf8");
const cases = [];

function check(name, condition, detail) {
  if (!condition) throw new Error(`${name}: ${detail}`);
  cases.push(name);
}

function section(markdown, startAnchor, endAnchor) {
  const start = markdown.indexOf(`<a id="${startAnchor}"></a>`);
  const end = markdown.indexOf(`<a id="${endAnchor}"></a>`, start + 1);
  check(`section:${startAnchor}`, start >= 0 && end > start, `missing ${startAnchor}`);
  return markdown.slice(start, end);
}

function correspondenceErrors(text) {
  const part = section(text, "part-vii", "part-viii");
  const errors = [];
  if (part.includes("*Scheduled content:*")) errors.push("placeholder");
  for (const marker of [
    "exact", "aggregate", "external-contract", "presentation-only", "missing",
    "tests/SIR.Conformance.Shared/QuintQ4ReplayFixtures.fs",
    "src/SIR.Simulation/CombatRules.fs:CombatRules",
    "SIR-Q4-EXACT-ACCEPT: traces=1 states=9",
    "SIR-Q4-SAMPLED-ACCEPT: traces=16 states=144",
    "seed `352`", "at most `8` steps", "first divergence", "expected=<model>",
    "actual=<runtime>", "wrong-action-mapping", "wrong-observable-field",
    "combat-boundary-defect", "never make the adapter echo whichever side changed",
  ]) if (!part.includes(marker)) errors.push(`missing:${marker}`);
  if (/simulation (?:output )?(?:proves|establishes) implementation equivalence/i.test(part)) {
    errors.push("equivalence-overclaim");
  }
  const map = part.slice(part.indexOf("| Stable rule |"), part.indexOf("**Missing correspondence register.**"));
  const ids = [...map.matchAll(/\| \[((?:CONTENT|COMBAT)-[A-Z0-9-]+)\]\(#rule-[a-z0-9-]+\) \|/g)].map(match => match[1]);
  if (ids.length !== 16 || new Set(ids).size !== 16) errors.push(`rule-count:${ids.length}/${new Set(ids).size}`);
  for (const line of map.split("\n").filter(line => /^\| \[(?:CONTENT|COMBAT)-/.test(line))) {
    const cells = line.split("|").slice(1, -1).map(cell => cell.trim());
    if (cells.length !== 5) errors.push(`columns:${line}`);
    if (!cells[2] || cells[2] === "—") errors.push(`runtime:${cells[0]}`);
    if (!cells[3] || cells[3] === "—") errors.push(`evidence:${cells[0]}`);
    if (!["exact", "aggregate", "external-contract", "presentation-only", "missing"].includes(cells[4])) errors.push(`status:${cells[0]}`);
  }
  return errors;
}

check("focused-positive", correspondenceErrors(handbook).length === 0, correspondenceErrors(handbook).join(", "));

const controls = [
  ["missing-runtime-subject", "`CombatRules.registry`; `QuintQ4ReplayFixtures.attackInput`", ""],
  ["missing-evidence-scope", "exact representative fixture and sampled attack inputs", ""],
  ["missing-status", "| exact |", "| unspecified |"],
  ["missing-sample-bound", "seed `352`", "seed is recorded elsewhere"],
  ["equivalence-overclaim", "It does **not** by itself establish production [correspondence](#def-correspondence).", "Quint simulation proves implementation equivalence."],
];
for (const [name, from, to] of controls) {
  const count = handbook.split(from).length - 1;
  check(`negative-subject:${name}`, count >= 1, `subject absent: ${from}`);
  const mutant = name === "missing-sample-bound" ? handbook.replaceAll(from, to) : handbook.replace(from, to);
  check(`observed-red:${name}`, correspondenceErrors(mutant).length > 0, `${name} escaped the detector`);
  check(`restored-green:${name}`, correspondenceErrors(handbook).length === 0, `${name} did not restore`);
}

for (const subject of ["let registry", "let resolveAttack", "let resolveConsequences", "let resolveCoverImpact", "let resolveRecovery"]) {
  check(`runtime:${subject}`, runtime.includes(subject), `missing current runtime subject ${subject}`);
}
for (const subject of ["let private expectedState", "let private applyModelAction", "let private firstDifference", "let replayDirectory", "Q4 first divergence:"]) {
  check(`adapter:${subject}`, adapter.includes(subject), `missing replay subject ${subject}`);
}
for (const subject of ["--quint-q4-exact", "--quint-q4-sampled", "wrong-action-mapping", "wrong-observable-field", "combat-boundary-defect", "--seed 352", "--max-steps 8"]) {
  check(`qualifier:${subject}`, qualifier.includes(subject), `missing qualifier subject ${subject}`);
}

if (process.argv.includes("--require-rendered")) {
  const renderedPath = "artifacts/site/sir-combat-quint-handbook.html";
  check("rendered-exists", existsSync(renderedPath), `missing ${renderedPath}`);
  const rendered = readFileSync(renderedPath, "utf8");
  check("rendered-correspondence", rendered.includes("supercover geometry behind") && rendered.includes("SIR-Q4-SAMPLED-ACCEPT"), "rendered M5 content missing");
}

mkdirSync("readiness/373-handbook-m5", { recursive: true });
const xml = [
  '<?xml version="1.0" encoding="UTF-8"?>',
  `<testsuite name="sir-handbook-m5-runtime-correspondence" tests="${cases.length}" failures="0" errors="0" skipped="0">`,
  ...cases.map(name => `  <testcase classname="SIR.HandbookM5" name="${name.replaceAll("&", "&amp;").replaceAll('"', "&quot;")}"/>`),
  "</testsuite>",
  "",
].join("\n");
writeFileSync(receiptPath, xml);
console.log(`handbook-m5 runtime correspondence: PASS (${cases.length} checks; 5 observed-red/restored-green controls; 16 rule mappings)`);
