#!/usr/bin/env node
import fs from "node:fs";
const path = "docs/sir-combat-quint-handbook-roadmap.md";
const roadmap = fs.readFileSync(path, "utf8");
const required = [
  "### - [x] M7 — Review, publication, and maintenance handoff",
  "roadmap-sir-combat-quint-handbook-m7-publication-handoff",
  "work/380-handbook-m7/",
  "readiness/380-handbook-m7/",
  "issue: #380"
];
for (const text of required) if (!roadmap.includes(text)) throw new Error(`roadmap M7 evidence missing: ${text}`);
if ((roadmap.match(/### - \[ \] M7/g) ?? []).length) throw new Error("roadmap M7 remains unchecked");
console.log("handbook-m7 roadmap audit: PASS");
