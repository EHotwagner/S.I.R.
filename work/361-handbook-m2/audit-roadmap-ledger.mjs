#!/usr/bin/env node

import { execFileSync } from "node:child_process";
import { createHash } from "node:crypto";
import { readFileSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";

const root = resolve(import.meta.dirname, "../..");
const roadmapPath = resolve(root, "docs/sir-combat-quint-handbook-roadmap.md");
const receiptPath = resolve(root, "readiness/361-handbook-m2/roadmap-ledger.junit.xml");
const roadmap = readFileSync(roadmapPath, "utf8");
const baseline = execFileSync("git", ["show", "origin/main:docs/sir-combat-quint-handbook-roadmap.md"], {
  cwd: root,
  encoding: "utf8",
});

const checked = text =>
  [...text.matchAll(/^### - \[x\] (M\d+) —/gm)].map(match => match[1]);
const baselineChecked = checked(baseline);
const candidateChecked = checked(roadmap);
const newChecked = candidateChecked.filter(id => !baselineChecked.includes(id));

const required = [
  "### - [x] M2 — Representative attack learning spine",
  "Completion evidence (2026-08-27):",
  "work/361-handbook-m2/qualify-handbook-m2.sh",
  "readiness/361-handbook-m2/",
  "feedback/2026-08-27-sir-handbook-m2-representative-attack.md",
  "Independent",
  "exact-head acceptance",
  "PR #362",
  "only when PR #362 merges",
  "issue #361 closure",
  "project status Done",
];

const failures = [];
if (newChecked.length !== 1 || newChecked[0] !== "M2") {
  failures.push(`expected only M2 newly checked; got ${JSON.stringify(newChecked)}`);
}
for (const marker of required) {
  if (!roadmap.includes(marker)) failures.push(`missing ledger marker: ${marker}`);
}

if (failures.length > 0) {
  console.error(failures.join("\n"));
  process.exit(1);
}

const digest = createHash("sha256").update(roadmap).digest("hex");
writeFileSync(
  receiptPath,
  `<?xml version="1.0" encoding="UTF-8"?>\n<testsuite name="sir-handbook-m2-roadmap-ledger" tests="1" failures="0" errors="0" skipped="0">\n  <testcase classname="SIR.HandbookM2" name="only-m2-checked-with-conditional-delivery-evidence"/>\n  <properties><property name="roadmapSha256" value="${digest}"/></properties>\n</testsuite>\n`,
);
console.log(`handbook-m2 roadmap ledger: PASS (only M2 newly checked; sha256=${digest})`);
