#!/usr/bin/env node

import { execFileSync } from "node:child_process";
import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";

const root = resolve(import.meta.dirname, "../..");
const roadmapPath = resolve(root, "docs/sir-combat-quint-handbook-roadmap.md");
const receiptPath = resolve(root, "readiness/365-handbook-m4/roadmap-ledger.junit.xml");
const roadmap = readFileSync(roadmapPath, "utf8");
const baseline = execFileSync("git", ["show", "origin/main:docs/sir-combat-quint-handbook-roadmap.md"], { cwd: root, encoding: "utf8" });
const checked = text => [...text.matchAll(/^### - \[x\] ([A-Z0-9]+) —/gm)].map(match => match[1]);
const newChecked = checked(roadmap).filter(id => !checked(baseline).includes(id));
const required = [
  "### - [x] M4 — Formal reasoning and mutation laboratory",
  "Completion evidence (2026-08-27):",
  "work/365-handbook-m4/audit-formal-reasoning.mjs",
  "readiness/365-handbook-m4/",
  "roadmap-sir-combat-quint-handbook-m4-formal-reasoning",
  "six detector-specific observed-red/restored-green pairs",
  "Independent exact-head acceptance",
  "green exact-head hosted CI",
  "only when the M4 PR merges",
  "issue #365 closure",
  "project status Done",
  "### - [ ] M5 — Runtime correspondence and evidence",
  "### - [ ] M6 — Complete definition index and enforced linkability",
  "### - [ ] M6V — Authoritative mechanics and theory diagrams",
  "### - [ ] M7 — Review, publication, and maintenance handoff",
  "progressive animation/shader enhancement",
  "and performance",
  "qualification before M7 publication",
  "Dependency: M6V authoritative mechanics and theory diagrams must be complete before M7 publication.",
];
const failures = [];
if (newChecked.length !== 1 || newChecked[0] !== "M4") failures.push(`expected only M4 newly checked; got ${JSON.stringify(newChecked)}`);
for (const pending of ["M5", "M6", "M6V", "M7"]) if (new RegExp(`^### - \\[x\\] ${pending} —`, "m").test(roadmap)) failures.push(`${pending} must remain pending during M4`);
for (const marker of required) if (!roadmap.includes(marker)) failures.push(`missing ledger marker: ${marker}`);
if (failures.length) {
  console.error(failures.join("\n"));
  process.exit(1);
}

mkdirSync(resolve(root, "readiness/365-handbook-m4"), { recursive: true });
const digest = createHash("sha256").update(roadmap).digest("hex");
writeFileSync(receiptPath, `<?xml version="1.0" encoding="UTF-8"?>\n<testsuite name="sir-handbook-m4-roadmap-ledger" tests="1" failures="0" errors="0" skipped="0">\n  <testcase classname="SIR.HandbookM4" name="only-m4-checked-with-conditional-delivery-evidence"/>\n  <properties><property name="roadmapSha256" value="${digest}"/></properties>\n</testsuite>\n`);
console.log(`handbook-m4 roadmap ledger: PASS (only M4 newly checked; M5/M6/M6V/M7 pending; sha256=${digest})`);
