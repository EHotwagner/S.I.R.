#!/usr/bin/env node

import { execFileSync } from "node:child_process";
import { createHash } from "node:crypto";
import { readFileSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";

const root = resolve(import.meta.dirname, "../..");
const roadmapPath = resolve(root, "docs/sir-combat-quint-handbook-roadmap.md");
const receiptPath = resolve(root, "readiness/363-handbook-m3/roadmap-ledger.junit.xml");
const roadmap = readFileSync(roadmapPath, "utf8");
const baseline = execFileSync("git", ["show", "origin/main:docs/sir-combat-quint-handbook-roadmap.md"], { cwd: root, encoding: "utf8" });
const checked = text => [...text.matchAll(/^### - \[x\] (M\d+) —/gm)].map(match => match[1]);
const newChecked = checked(roadmap).filter(id => !checked(baseline).includes(id));

const required = [
  "### - [x] M3 — Complete combat-rule walkthroughs",
  "Completion evidence (2026-08-27):",
  "16/16",
  "work/363-handbook-m3/qualify-handbook-m3.sh",
  "readiness/363-handbook-m3/",
  "roadmap-sir-combat-quint-handbook-m3-complete-rules",
  "Independent exact-head acceptance",
  "green exact-head hosted CI",
  "only when the M3 PR merges",
  "issue #363 closure",
  "project status Done",
  "### - [ ] M6V — Authoritative mechanics and theory diagrams",
  "reuse the existing in-game SVG symbology and glyph vocabulary",
  "pure abstract SVG diagrams",
  "reduced-motion, static, print, and non-WebGL fallbacks",
  "derivation or mechanical checking against authoritative rules and the Quint model",
  "visual-regression/render-inspection evidence and performance qualification",
  "Dependency: M6V authoritative mechanics and theory diagrams must be complete before M7 publication.",
];
const failures = [];
if (newChecked.length !== 1 || newChecked[0] !== "M3") failures.push(`expected only M3 newly checked; got ${JSON.stringify(newChecked)}`);
if (/^### - \[x\] M6V —/m.test(roadmap)) failures.push("M6V must remain pending during M3");
for (const marker of required) if (!roadmap.includes(marker)) failures.push(`missing ledger marker: ${marker}`);
if (failures.length) {
  console.error(failures.join("\n"));
  process.exit(1);
}

const digest = createHash("sha256").update(roadmap).digest("hex");
writeFileSync(receiptPath, `<?xml version="1.0" encoding="UTF-8"?>\n<testsuite name="sir-handbook-m3-roadmap-ledger" tests="1" failures="0" errors="0" skipped="0">\n  <testcase classname="SIR.HandbookM3" name="only-m3-checked-with-conditional-delivery-evidence"/>\n  <properties><property name="roadmapSha256" value="${digest}"/></properties>\n</testsuite>\n`);
console.log(`handbook-m3 roadmap ledger: PASS (only M3 newly checked; sha256=${digest})`);
