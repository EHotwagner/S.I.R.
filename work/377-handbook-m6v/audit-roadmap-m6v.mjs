#!/usr/bin/env node
import fs from "node:fs";

const roadmap = fs.readFileSync("docs/sir-combat-quint-handbook-roadmap.md", "utf8");
const required = [
  "### - [x] M6V — Authoritative mechanics and theory diagrams",
  "### - [ ] M7 — Review, publication, and maintenance handoff",
  "roadmap-sir-combat-quint-handbook-m6v-visual-explanations",
  "six source-bound figures",
  "rifleman paths",
  "five pure abstract SVGs",
  "normal, reduced-motion, print, and",
  "effects-off modes",
  "138-element",
  "16,561-byte",
  "p95 62.7 ms",
  "p99 64.1 ms",
  "100/200 ms",
  "no frame-pacing claim is made",
  "M7 remains pending unchanged"
];

function validate(text) {
  const errors = [];
  for (const value of required) if (!text.includes(value)) errors.push(`missing:${value}`);
  if (text.includes("### - [x] M7 — Review, publication, and maintenance handoff")) errors.push("m7-prematurely-checked");
  return errors;
}

if (process.argv.includes("--self-test")) {
  const mutations = [
    ["m6v-unchecked", roadmap.replace("### - [x] M6V", "### - [ ] M6V")],
    ["m7-premature", roadmap.replace("### - [ ] M7", "### - [x] M7")],
    ["timing-evidence-removed", roadmap.replace("p95 62.7 ms", "p95 unavailable")]
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
console.log("roadmap ledger passed: M6V checked with visual/render/performance evidence; M7 remains pending");
