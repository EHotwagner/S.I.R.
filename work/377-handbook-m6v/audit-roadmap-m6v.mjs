#!/usr/bin/env node
import fs from "node:fs";

const roadmap = fs.readFileSync("docs/sir-combat-quint-handbook-roadmap.md", "utf8");
const visual = JSON.parse(fs.readFileSync("readiness/377-handbook-m6v/visual-qualification.json", "utf8"));
const inspection = JSON.parse(fs.readFileSync("readiness/377-handbook-m6v/rendered/inspection.json", "utf8"));
const mutation = JSON.parse(fs.readFileSync("readiness/377-handbook-m6v/timing-overflow-mutation.json", "utf8"));
const modes = inspection.observations?.map(observation => observation.mode) ?? [];
const standaloneScreenshots = inspection.observations?.reduce((count, observation) => count + (observation.diagrams?.length ?? 0), 0) ?? 0;
const handbookScreenshots = inspection.observations?.reduce((count, observation) => count + (observation.diagrams?.filter(diagram => diagram.handbookScreenshotSha256).length ?? 0), 0) ?? 0;
const required = [
  "### - [x] M6V — Authoritative mechanics and theory diagrams",
  "### - [ ] M7 — Review, publication, and maintenance handoff",
  "roadmap-sir-combat-quint-handbook-m6v-visual-explanations",
  "six source-bound figures",
  "rifleman paths",
  "five pure abstract SVGs",
  modes.join(", "),
  `${visual.counters.elements}-element`,
  `${visual.counters.bytes.toLocaleString("en-US")}-byte`,
  `${visual.counters.animatedElements}-animated-element`,
  `${standaloneScreenshots} standalone and ${handbookScreenshots} handbook-image screenshots`,
  `${inspection.timings.sampleCount} warm, no-store navigations`,
  `p95 ${inspection.timings.p95LoadMs} ms`,
  `p99 ${inspection.timings.p99LoadMs} ms`,
  `p95 ${mutation.observation.p95LoadMs} ms and p99 ${mutation.observation.p99LoadMs} ms`,
  `${inspection.timings.maxP95Ms}/${inspection.timings.maxP99Ms} ms budgets`,
  "no frame-pacing claim is made",
  "M7 remains pending unchanged"
];

function validate(text) {
  const errors = [];
  for (const value of required) if (!text.includes(value)) errors.push(`missing:${value}`);
  if (visual.result !== "pass" || inspection.result !== "pass" || inspection.evidenceMode !== "immutable-candidate" || mutation.result !== "observed-red") errors.push("receipt-result-mismatch");
  if (JSON.stringify(modes) !== JSON.stringify(["normal", "reduced-motion", "print", "effects-off", "css-disabled"]) || standaloneScreenshots !== 30 || handbookScreenshots !== 18 || inspection.timings.sampleCount !== 100) errors.push("receipt-scale-mismatch");
  if (text.includes("### - [x] M7 — Review, publication, and maintenance handoff")) errors.push("m7-prematurely-checked");
  return errors;
}

if (process.argv.includes("--self-test")) {
  const mutations = [
    ["m6v-unchecked", roadmap.replace("### - [x] M6V", "### - [ ] M6V")],
    ["m7-premature", roadmap.replace("### - [ ] M7", "### - [x] M7")],
    ["timing-evidence-removed", roadmap.replace(`p95 ${inspection.timings.p95LoadMs} ms`, "p95 unavailable")]
  ];
  for (const [name, mutated] of mutations) {
    if (validate(mutated).length === 0) throw new Error(`${name} mutation remained green`);
    if (validate(roadmap).length !== 0) throw new Error(`${name} did not restore green`);
    console.log(`observed red/restored green: ${name}`);
  }
}

const errors = validate(roadmap);
if (errors.length) {
  for (const error of errors) console.error(`roadmap-m6v:${error}`);
  process.exit(1);
}
console.log("roadmap ledger passed: M6V candidate evidence matches render/performance receipts; M7 remains pending");
