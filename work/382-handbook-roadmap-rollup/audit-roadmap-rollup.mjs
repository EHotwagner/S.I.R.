#!/usr/bin/env node

import { createHash } from "node:crypto";
import { readFileSync, readdirSync, writeFileSync } from "node:fs";
import { basename, join, resolve } from "node:path";

const cyclePrefix = "roadmap-sir-combat-quint-handbook-";
const reportPath = "docs/2026-08-28-sir-combat-quint-handbook-roadmap-final-report.md";
const roadmapPath = "docs/sir-combat-quint-handbook-roadmap.md";
const requiredMilestones = ["M0", "M1", "M2", "M3", "O1", "M4", "M5", "M6", "M6V", "M7"];
const dispositions = new Set(["structured finding", "positive pattern", "accepted observation", "deduplicated existing issue"]);

const delivery = {
  "m0-authority-inventory": [356, "357", "0299ad8", "33059711124 (success)", "not retained"],
  "m1-linked-skeleton": [359, "360", "5f72624", "33065505221 (success)", "33067308926 (success)"],
  "m2-representative-attack": [361, "362", "b109fd2", "33079274657 (success)", "33081634129 (success)"],
  "m3-complete-rules": [363, "364", "a907327", "33086834457 (success)", "33089347740 (success)"],
  "ci-main-routing": [366, "367", "ad6aed1", "33093549465 (failed; repaired by #368)", "33094000893 (skipped)"],
  "ci-handoff-repair": [368, "369", "168b8a8", "33097307202 (success)", "33097748266 (cancelled; repaired by #370)"],
  "pages-timeout-repair": [370, "371", "4a92687", "33099829753 (success)", "33100280817 (success)"],
  "m4-formal-reasoning": [365, "372", "858368e", "33105619380 (success)", "33105974004 (success)"],
  "m5-runtime-correspondence": [373, "374", "52b9b1f", "33111845284 (success)", "33112266975 (success)"],
  "m6-index-link-enforcement": [375, "376", "c47286a", "33119695779 (success)", "33119986146 (success)"],
  "m6v-visual-explanations": [377, "378 + repair 379", "318f07a", "33134277783 (success)", "33134495479 (success)"],
  "m7-publication-handoff": [380, "381", "fd0b3d7", "33139897624 (success)", "33140097398 (success)"],
};

const deduplicated = new Map([
  ["ci-handoff-repair#1", "Existing S.I.R. issue #368 owned the missing protected handoff and delivered PR #369."],
  ["m2-representative-attack#5", "Duplicate of the unchanged-main invalidation cause routed by the bound report to FS-GG/.github#2852."],
  ["m3-complete-rules#5", "Duplicate of the same unchanged-main invalidation cause routed by the bound reports to FS-GG/.github#2852."],
  ["pages-timeout-repair#1", "Existing S.I.R. issue #370 owned the external Pages timeout and delivered PR #371."],
]);
const structured = new Set(["m1-linked-skeleton#4", "m1-linked-skeleton#5", "m2-representative-attack#4", "m3-complete-rules#6", "m6v-visual-explanations#3"]);

function need(condition, message) {
  if (!condition) throw new Error(message);
}

function sha(text) {
  return createHash("sha256").update(text.replaceAll("\r\n", "\n").replaceAll("\r", "\n")).digest("hex");
}

function frontmatter(text) {
  const match = text.match(/^---\n([\s\S]*?)\n---/);
  need(match, "feedback report has no front matter");
  return Object.fromEntries(match[1].split("\n").filter(line => line.includes(":"))
    .map(line => { const at = line.indexOf(":"); return [line.slice(0, at).trim(), line.slice(at + 1).trim()]; }));
}

function markers(text, name) {
  return [...text.matchAll(new RegExp(`<!-- ${name} ([A-Za-z0-9+/=]+) -->`, "g"))]
    .map(match => JSON.parse(Buffer.from(match[1], "base64").toString("utf8")));
}

function marker(name, value) {
  return `<!-- ${name} ${Buffer.from(JSON.stringify(value)).toString("base64")} -->`;
}

function dispositionFor(short, sequence, row) {
  const key = `${short}#${sequence}`;
  if (row.kind === "positive-pattern") return ["positive pattern", `Accepted from the ${row.phase} checkpoint; retained evidence: ${row.evidence}`];
  if (deduplicated.has(key)) return ["deduplicated existing issue", `${deduplicated.get(key)} Retained evidence: ${row.evidence}`];
  if (structured.has(key)) return ["structured finding", `The originating cycle repaired this ${row.kind} before landing; retained repair evidence: ${row.evidence}`];
  return ["accepted observation", `Accepted as a cycle-bounded ${row.kind} after the bound schema-v2 audit; retained evidence: ${row.evidence}`];
}

function load(root, overrides = new Map()) {
  const read = rel => overrides.has(rel) ? overrides.get(rel) : readFileSync(join(root, rel), "utf8");
  const checkpointDir = join(root, "feedback/checkpoints");
  const checkpointFiles = readdirSync(checkpointDir).filter(name => name.startsWith(cyclePrefix) && name.endsWith(".jsonl")).sort();
  const reports = readdirSync(join(root, "feedback")).filter(name => name.endsWith(".md"))
    .map(name => [`feedback/${name}`, read(`feedback/${name}`)])
    .map(([path, text]) => ({ path, text, meta: frontmatter(text) }))
    .filter(item => item.meta.cycle?.startsWith(cyclePrefix));
  const cycles = checkpointFiles.map(name => name.slice(0, -".jsonl".length));
  need(new Set(cycles).size === cycles.length, "duplicate checkpoint cycle");
  need(reports.length === cycles.length, `cycle inventory mismatch: ${cycles.length} checkpoint cycles and ${reports.length} report cycles`);
  const byCycle = new Map(reports.map(item => [item.meta.cycle, item]));
  need(byCycle.size === reports.length, "duplicate feedback report cycle");

  const result = [];
  for (const cycle of cycles) {
    const checkpoint = `feedback/checkpoints/${cycle}.jsonl`;
    const report = byCycle.get(cycle);
    need(report, `cycle omitted from reports: ${cycle}`);
    const audit = `feedback/audits/${basename(report.path, ".md")}.audit.json`;
    const auditValue = JSON.parse(read(audit));
    need(auditValue.report === report.path, `wrong report/audit binding for ${cycle}`);
    need(auditValue.reportSha256 === sha(report.text), `wrong report/audit digest for ${cycle}`);
    const rows = read(checkpoint).trim().split("\n").map((line, index) => {
      const row = JSON.parse(line);
      need(row.cycle === cycle, `checkpoint cycle mismatch for ${cycle}#${index + 1}`);
      return row;
    });
    const declared = Number(report.text.match(/^- \*\*material events:\*\* (\d+)$/m)?.[1]);
    need(declared === rows.length, `count mismatch for ${cycle}: report=${declared} actual=${rows.length}`);
    const phases = report.text.match(/^- \*\*phases:\*\* (.+)$/m)?.[1];
    need(phases, `missing phases for ${cycle}`);
    result.push({ cycle, short: cycle.slice(cyclePrefix.length), checkpoint, report: report.path, audit, rows, phases });
  }
  return { read, cycles: result };
}

function validate(root, reportOverride, overrides = new Map()) {
  const state = load(root, overrides);
  const report = reportOverride ?? state.read(reportPath);
  const roadmap = state.read(roadmapPath);
  for (const id of requiredMilestones) {
    const hits = [...roadmap.matchAll(new RegExp(`^### - \\[x\\] ${id}(?: | —)`, "gm"))];
    need(hits.length === 1, `unchecked milestone or duplicate: ${id}`);
  }
  need(!readdirSync(join(root, "feedback/checkpoints")).some(name => name.includes("roadmap-final-report")), "terminal roll-up created a self-referential checkpoint cycle");

  const cycleRows = markers(report, "roadmap-rollup-cycle");
  const checkpointRows = markers(report, "roadmap-rollup-checkpoint");
  need(cycleRows.length === state.cycles.length, `omitted cycle: report=${cycleRows.length} actual=${state.cycles.length}`);
  const reportCycleSet = new Set(cycleRows.map(row => row.cycle));
  for (const cycle of state.cycles) {
    need(reportCycleSet.has(cycle.cycle), `omitted cycle: ${cycle.cycle}`);
    const cycleRow = cycleRows.find(row => row.cycle === cycle.cycle);
    need(cycleRow.report === cycle.report && cycleRow.audit === cycle.audit && cycleRow.checkpoint === cycle.checkpoint, `wrong report/audit binding in roll-up for ${cycle.cycle}`);
    need(cycleRow.count === cycle.rows.length, `count mismatch in roll-up for ${cycle.cycle}`);
  }
  const expectedCount = state.cycles.reduce((sum, cycle) => sum + cycle.rows.length, 0);
  need(checkpointRows.length === expectedCount, `omitted checkpoint: report=${checkpointRows.length} actual=${expectedCount}`);
  const keys = new Set();
  const counts = Object.fromEntries([...dispositions].map(value => [value, 0]));
  for (const cycle of state.cycles) {
    cycle.rows.forEach((source, index) => {
      const key = `${cycle.cycle}#${index + 1}`;
      const row = checkpointRows.find(item => item.cycle === cycle.cycle && item.sequence === index + 1);
      need(row, `omitted checkpoint: ${key}`);
      need(!keys.has(key), `duplicate checkpoint disposition: ${key}`);
      keys.add(key);
      need(row.phase === source.phase && row.kind === source.kind && row.summary === source.summary && row.evidence === source.evidence, `checkpoint binding mismatch: ${key}`);
      need(dispositions.has(row.disposition), `invalid disposition: ${key}=${row.disposition}`);
      need(typeof row.rationale === "string" && row.rationale.includes(source.evidence), `checkpoint rationale does not cite retained evidence: ${key}`);
      counts[row.disposition] += 1;
    });
  }
  const declaredCycles = Number(report.match(/^- \*\*Cycles:\*\* (\d+)$/m)?.[1]);
  const declaredCheckpoints = Number(report.match(/^- \*\*Checkpoint records:\*\* (\d+)$/m)?.[1]);
  need(declaredCycles === state.cycles.length, "cycle total mismatch");
  need(declaredCheckpoints === expectedCount, "checkpoint total mismatch");
  for (const [kind, count] of Object.entries(counts)) need(report.includes(`- **${kind}:** ${count}`), `disposition count mismatch: ${kind}`);
  need(report.includes("[Combat in Quint handbook roadmap](sir-combat-quint-handbook-roadmap.html)"), "final report does not link the roadmap");
  need(roadmap.includes("[terminal roadmap report](2026-08-28-sir-combat-quint-handbook-roadmap-final-report.html)"), "roadmap does not link the final report");
  return { cycles: state.cycles.length, checkpoints: expectedCount, milestones: requiredMilestones.length, dispositions: counts };
}

function linkIssue(number) { return `[S.I.R. #${number}](https://github.com/EHotwagner/S.I.R./issues/${number})`; }
function linkPr(value) { return value.split(" + ").map(part => { const n = part.match(/\d+/)?.[0]; return n ? `[${part}](https://github.com/EHotwagner/S.I.R./pull/${n})` : part; }).join(" + "); }
function linkRun(value) { const n = value.match(/^\d+/)?.[0]; return n ? `[${value}](https://github.com/EHotwagner/S.I.R./actions/runs/${n})` : value; }
function linkFile(label, path) { return `[${label}](https://github.com/EHotwagner/S.I.R./blob/main/${path})`; }

function render(root) {
  const state = load(root);
  const checkpointCount = state.cycles.reduce((sum, cycle) => sum + cycle.rows.length, 0);
  const counts = Object.fromEntries([...dispositions].map(value => [value, 0]));
  const cycleLines = [];
  const cycleMarkers = [];
  const checkpointLines = [];
  const checkpointMarkers = [];
  for (const cycle of state.cycles) {
    const d = delivery[cycle.short];
    need(d, `missing delivery mapping: ${cycle.short}`);
    cycleLines.push(`| \`${cycle.short}\` | ${linkFile("report", cycle.report)} / ${linkFile("audit", cycle.audit)} / ${linkFile("checkpoints", cycle.checkpoint)} | ${cycle.rows.length} | ${cycle.phases} | ${linkIssue(d[0])} / ${linkPr(d[1])} | \`${d[2]}\` | ${linkRun(d[3])} | ${linkRun(d[4])} |`);
    cycleMarkers.push(marker("roadmap-rollup-cycle", { cycle: cycle.cycle, report: cycle.report, audit: cycle.audit, checkpoint: cycle.checkpoint, count: cycle.rows.length, phases: cycle.phases }));
    cycle.rows.forEach((row, index) => {
      const [disposition, rationale] = dispositionFor(cycle.short, index + 1, row);
      counts[disposition] += 1;
      checkpointLines.push(`- **\`${cycle.short}#${index + 1}\` — ${disposition}.** ${row.summary} Evidence: \`${row.evidence}\` Rationale: ${rationale}`);
      checkpointMarkers.push(marker("roadmap-rollup-checkpoint", { cycle: cycle.cycle, sequence: index + 1, phase: row.phase, kind: row.kind, summary: row.summary, evidence: row.evidence, disposition, rationale }));
    });
  }
  return `---
title: Combat in Quint handbook roadmap final report
category: Battlefield Systems
categoryindex: 4
index: 49
description: Terminal cross-cycle validation and checkpoint disposition report for the Combat in Quint handbook roadmap.
date: 2026-08-28
status: complete
document-type: report
---

# Combat in Quint handbook roadmap final report

This terminal report closes the parent-owned \`$work-roadmap\` reporting obligation for the
[Combat in Quint handbook roadmap](sir-combat-quint-handbook-roadmap.html). It is not a milestone,
semantic authority, or feedback cycle. It validates and summarizes the existing cycle evidence; the
handbook, literate Quint model, combat architecture, runtime registry, and prior feedback artifacts
retain their original authority boundaries.

## Exact coverage

- **Cycles:** ${state.cycles.length}
- **Checkpoint records:** ${checkpointCount}
- **Roadmap milestones/operational units checked:** ${requiredMilestones.length}/${requiredMilestones.length} (M0, M1, M2, M3, O1, M4, M5, M6, M6V, M7)
- **structured finding:** ${counts["structured finding"]}
- **positive pattern:** ${counts["positive pattern"]}
- **accepted observation:** ${counts["accepted observation"]}
- **deduplicated existing issue:** ${counts["deduplicated existing issue"]}

Membership is derived from matching checkpoint streams and independently reconciled with schema-v2
report front matter. The counts above are results, not a hard-coded definition of completion. Each
cycle passed the existing checkpoint validator, exact report/actionability-audit validator, and
activation/binding validator. The two bounded repair cycles (CI handoff and Pages timeout) explicitly
exercise \`implementation-test-evidence, verify-ship-pr\`; the other ten exercise all four roadmap phases.

## Cycle and delivery coverage

| Cycle | Bound feedback evidence | Records | Exercised phases | Issue / PR | Merge | Exact-main CI | Pages |
|---|---|---:|---|---|---|---|---|
${cycleLines.join("\n")}

Failed, skipped, or cancelled hosted runs are labelled as such and are not treated as passing proof.
Their successor repair cycles carry the eventual green evidence. \`not retained\` means this roll-up
found no exact Pages identifier in retained repository/GitHub history; it does not mean success.

${cycleMarkers.join("\n")}

## Individual checkpoint dispositions

The immutable key is the full cycle id plus the one-based source JSONL line. “Rationale” is this
roll-up's judgement; summary and evidence are copied exactly and mechanically compared with the source.

${checkpointLines.join("\n")}

${checkpointMarkers.join("\n")}

## Fail-closed qualification

Run:

\`\`\`sh
node work/382-handbook-roadmap-rollup/audit-roadmap-rollup.mjs
node work/382-handbook-roadmap-rollup/audit-roadmap-rollup.mjs --self-test
bash work/382-handbook-roadmap-rollup/qualify-roadmap-rollup.sh
\`\`\`

The self-test isolates six defects: omitted cycle, omitted checkpoint, wrong report/audit binding,
count mismatch, invalid disposition, and unchecked milestone. Each must observe its named red before
the untouched repository restores green. The owner qualifier also reruns all three existing feedback
validators for every derived cycle and the strict documentation build.

## Maintenance conclusion

Future authority, model, runtime, vocabulary, visual, or toolchain changes follow the
[handbook maintenance checklist](rules/sir-combat-handbook-maintenance.html). This report is refreshed
only when the roadmap's completed-cycle evidence changes; refreshing it never creates a feedback cycle
for the refresh itself.
`;
}

const root = resolve(process.cwd());
if (process.argv.includes("--write-report")) {
  writeFileSync(join(root, reportPath), render(root));
  console.log(`roadmap-rollup: wrote ${reportPath}`);
} else if (process.argv.includes("--self-test")) {
  const pristine = readFileSync(join(root, reportPath), "utf8");
  const firstPositive = markers(pristine, "roadmap-rollup-checkpoint").find(row => row.disposition === "positive pattern");
  const invalid = { ...firstPositive, disposition: "closed" };
  const invalidDispositionReport = pristine.replace(marker("roadmap-rollup-checkpoint", firstPositive), marker("roadmap-rollup-checkpoint", invalid));
  const cases = [
    ["omitted-cycle", pristine.replace(/^<!-- roadmap-rollup-cycle .*? -->\n/m, ""), new Map(), /omitted cycle/],
    ["omitted-checkpoint", pristine.replace(/^<!-- roadmap-rollup-checkpoint .*? -->\n/m, ""), new Map(), /omitted checkpoint/],
    ["wrong-report-audit-binding", pristine, (() => { const state = load(root); const first = state.cycles[0]; const value = JSON.parse(readFileSync(join(root, first.audit), "utf8")); value.report = "feedback/wrong.md"; return new Map([[first.audit, JSON.stringify(value)]]); })(), /wrong report\/audit binding/],
    ["count-mismatch", pristine.replace("- **Checkpoint records:** 48", "- **Checkpoint records:** 47"), new Map(), /checkpoint total mismatch/],
    ["invalid-disposition", invalidDispositionReport, new Map(), /invalid disposition/],
    ["unchecked-milestone", pristine, new Map([[roadmapPath, readFileSync(join(root, roadmapPath), "utf8").replace("### - [x] M7", "### - [ ] M7")]]), /unchecked milestone/],
  ];
  for (const [name, report, overrides, expected] of cases) {
    let observed = "";
    try { validate(root, report, overrides); } catch (error) { observed = error.message; }
    need(expected.test(observed), `${name} did not observe named red; got: ${observed || "green"}`);
    console.log(`roadmap-rollup mutation: ${name}: observed red (${observed})`);
  }
  const result = validate(root);
  console.log(`roadmap-rollup: restored green (${result.cycles} cycles, ${result.checkpoints} checkpoints, ${result.milestones} checked units)`);
} else {
  const result = validate(root);
  console.log(`roadmap-rollup: PASS ${JSON.stringify(result)}`);
}
