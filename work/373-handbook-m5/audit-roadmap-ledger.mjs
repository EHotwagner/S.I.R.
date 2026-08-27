#!/usr/bin/env node

import { execFileSync } from "node:child_process";
import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";

const root = resolve(import.meta.dirname, "../..");
const roadmapPath = resolve(root, "docs/sir-combat-quint-handbook-roadmap.md");
const receiptPath = resolve(root, "readiness/373-handbook-m5/roadmap-ledger.junit.xml");
const roadmap = readFileSync(roadmapPath, "utf8");
const baseline = execFileSync("git", ["show", "origin/main:docs/sir-combat-quint-handbook-roadmap.md"], { cwd: root, encoding: "utf8" });
const checked = text => [...text.matchAll(/^### - \[x\] ([A-Z0-9]+) —/gm)].map(match => match[1]);
const newChecked = checked(roadmap).filter(id => !checked(baseline).includes(id));
const required = [
  "### - [x] M5 — Runtime correspondence and evidence",
  "work/373-handbook-m5/audit-runtime-correspondence.mjs",
  "readiness/373-handbook-m5/",
  "roadmap-sir-combat-quint-handbook-m5-runtime-correspondence",
  "Independent feedback",
  "independent exact-head acceptance",
  "green hosted PR CI",
  "post-merge protected/Pages",
  "issue #373 closure",
  "project status Done",
  "### - [ ] M6 — Complete definition index and enforced linkability",
  "### - [ ] M6V — Authoritative mechanics and theory diagrams",
  "### - [ ] M7 — Review, publication, and maintenance handoff",
  "progressive animation/shader enhancement",
  "visual regression",
  "performance qualification",
];
const failures = [];
if (newChecked.length !== 1 || newChecked[0] !== "M5") failures.push(`expected only M5 newly checked; got ${JSON.stringify(newChecked)}`);
for (const pending of ["M6", "M6V", "M7"]) if (new RegExp(`^### - \\[x\\] ${pending} —`, "m").test(roadmap)) failures.push(`${pending} must remain pending during M5`);
for (const marker of required) if (!roadmap.includes(marker)) failures.push(`missing ledger marker: ${marker}`);
if (failures.length) {
  console.error(failures.join("\n"));
  process.exit(1);
}

mkdirSync(resolve(root, "readiness/373-handbook-m5"), { recursive: true });
const digest = createHash("sha256").update(roadmap).digest("hex");
writeFileSync(receiptPath, `<?xml version="1.0" encoding="UTF-8"?>\n<testsuite name="sir-handbook-m5-roadmap-ledger" tests="1" failures="0" errors="0" skipped="0">\n  <testcase classname="SIR.HandbookM5" name="only-m5-checked-with-conditional-delivery-evidence"/>\n  <properties><property name="roadmapSha256" value="${digest}"/></properties>\n</testsuite>\n`);
console.log(`handbook-m5 roadmap ledger: PASS (only M5 newly checked; M6/M6V/M7 pending; sha256=${digest})`);
