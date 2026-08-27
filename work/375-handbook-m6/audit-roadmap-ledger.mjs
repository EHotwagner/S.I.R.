#!/usr/bin/env node

import fs from "node:fs";

const roadmap = fs.readFileSync("docs/sir-combat-quint-handbook-roadmap.md", "utf8");
const required = [
  "### - [x] M6 — Complete definition index and enforced linkability",
  "### - [ ] M6V — Authoritative mechanics and theory diagrams",
  "### - [ ] M7 — Review, publication, and maintenance handoff",
  "roadmap-sir-combat-quint-handbook-m6-index-link-enforcement",
  "188 canonical definitions",
  "74 top-level Quint declarations",
  "five canonical aliases",
  "missing fragments, duplicate anchors, absent index entries",
  "unlinked controlled occurrences"
];
for (const text of required) {
  if (!roadmap.includes(text)) {
    console.error(`roadmap M6 evidence missing: ${text}`);
    process.exit(1);
  }
}
if (!roadmap.includes("animation and shader effects only as progressive enhancement") ||
    !roadmap.includes("reduced-motion, static, print, and non-WebGL fallbacks") ||
    !roadmap.includes("visual-regression/render-inspection evidence and performance qualification")) {
  console.error("M6V visual/effects/accessibility/fallback/performance scope was not preserved");
  process.exit(1);
}
console.log("roadmap ledger passed: only M6 checked; M6V and M7 remain pending with visual scope preserved");
