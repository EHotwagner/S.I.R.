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

const nonPositiveJudgements = new Map([
  ["ci-handoff-repair#1", ["deduplicated existing issue", "The bound report and audit classify the missing protected site handoff as a new defect already owned by S.I.R. #368; PR #369 repaired that exact output-identity route, so this roll-up deduplicates rather than reopens it."]],
  ["ci-main-routing#2", ["accepted observation", "The bound report accepts this authoring friction because analyze rejected every placeholder and the cycle replaced them with concrete route, receipt, Pages, migration, and verification decisions before implementation; no residual defect or external owner remained."]],
  ["ci-main-routing#4", ["accepted observation", "The bound report keeps the optional refresh side effect as confidence-limited cycle friction: readiness was generated, unrelated projections were removed, and retained evidence is insufficient to assign a distinct actionable root cause."]],
  ["m0-authority-inventory#1", ["accepted observation", "The bound report treats the main-versus-candidate authority distinction as resolved cycle orchestration: M0 inventoried both identities explicitly and the Q4 authority dependency subsequently landed, leaving no independent follow-up."]],
  ["m0-authority-inventory#3", ["accepted observation", "The bound report accepts this first-build prerequisite because locked npm installation restored the documented Vite route in the isolated worktree; it records onboarding cost without claiming a separate product defect."]],
  ["m0-authority-inventory#4", ["deduplicated existing issue", "The bound report says existing issue and its audit says duplicate: the identical seventeen-error unchanged-main result was routed through FS-GG/.github#2856 to the distribution-authority successor FS-GG/.github#2852, so it is not an M0-specific finding."]],
  ["m1-linked-skeleton#1", ["accepted observation", "The bound report accepts the fresh-worktree restore prerequisite as onboarding friction: locked restore made prepare-site-only pass, while the cycle retained the prerequisite rather than attributing a new semantic or renderer defect."]],
  ["m1-linked-skeleton#4", ["structured finding", "The bound report records a product fix and the audit confirms the pre-merge defect: the in-app manifest omitted explicit HTML anchors, then M1 repaired fragment inventory and retained a detector that rejects the original escape."]],
  ["m1-linked-skeleton#5", ["structured finding", "The bound report records a product fix and the audit verifies both hosted policy failures; M1 removed the redundant S.I.R. prefixes and preserved the hosted documentation gate that exposed the bounded local-route gap."]],
  ["m2-representative-attack#3", ["deduplicated existing issue", "The bound report marks this an existing duplicate of the M0/M1 SDK-host envelope and related item 277 analysis; clearing the inherited resolver variables restored rendering, so M2 adds evidence but no new scope."]],
  ["m2-representative-attack#4", ["structured finding", "The bound report accepts the repaired quality gap and its audit verifies the corrected receipts: exact-head review caught over-attribution, then M2 split docs, links, focused mutation, and runtime evidence before emitting the aggregate."]],
  ["m2-representative-attack#5", ["deduplicated existing issue", "The bound report says existing issue and the audit confirms the duplicate unchanged-main invalidation cause; FS-GG/.github#2852 remains the distribution-authority successor, so no M2-specific issue is created."]],
  ["m3-complete-rules#1", ["accepted observation", "The bound report accepts this bounded orchestration exception because the host explicitly dispatched outside pnext, the missing receipt was not fabricated, and the cycle proceeded under that declared boundary without weakening claim semantics."]],
  ["m3-complete-rules#5", ["deduplicated existing issue", "The bound report identifies the same historical invalidation baseline already carried by M2 and the audit accepts that attribution; FS-GG/.github#2852 owns the root cause, so M3 contributes no distinct finding."]],
  ["m3-complete-rules#6", ["structured finding", "The bound report and audit accept the repaired exact-head quality gap: review found a nonexistent recovery subject, stale forward wording, and ambiguous receipt language, all corrected before merge and covered by the final qualification."]],
  ["m4-formal-reasoning#2", ["deduplicated existing issue", "The bound report says existing issue and its audit says duplicate: the absent consumer-relative lifecycle examples recur from earlier S.I.R. reports and closed FS-GG/FS.GG.SDD#539; the M3 corpus supplied the bounded fallback."]],
  ["m4-formal-reasoning#4", ["accepted observation", "The bound report deliberately does not promote the refresh incident: seven transient files were cleaned, but no command transcript or exact before/after inventory was retained, so actionable attribution would exceed the evidence."]],
  ["m5-runtime-correspondence#1", ["structured finding", "The bound report classifies this as an actionable product fix and the audit verifies the milestone-local assertion: inherited semantic gates pass, but the M4 aggregate rejects a descendant solely because M4 is no longer newly checked; this remains an explicit harness finding rather than accepted friction."]],
  ["m5-runtime-correspondence#4", ["accepted observation", "The bound report records procedural friction rather than a product defect: verify correctly rejected self-attestation, and the documented test-report import produced twenty-one observed receipts and shipReady without weakening evidence policy."]],
  ["m6-index-link-enforcement#1", ["accepted observation", "The bound report accepts the recurring SDK resolver friction because no matching open or closed issue owns this exact docs route; the explicit environment cleanup is retained as the bounded qualification envelope rather than falsely deduplicated."]],
  ["m6v-visual-explanations#1", ["accepted observation", "The bound report and audit call the cause a recurrence of closed FS-GG/.github#2698 but explicitly accept the residual gap: that issue added Ready-transition refusal, while neither it nor #2728 owns automatic route authoring, so there is no live issue to deduplicate to."]],
  ["m6v-visual-explanations#3", ["structured finding", "The bound report accepts the repaired quality gap and the audit verifies its measurement boundary: browser-init decode timing plus an in-subject delay control replaced nested-context and caller-delay measurements before landing."]],
  ["m7-publication-handoff#1", ["accepted observation", "The bound report and audit identify recurrence of the closed #2698 residual route-authoring cause yet retain an accepted observation: the recorder still lacks digest authoring, but no current issue owns that bounded CLI gap and the final route is valid."]],
  ["m7-publication-handoff#3", ["accepted observation", "The bound report accepts this self-owned one-cycle orchestration error because issue and route history verify widening before further edits, no external duplicate exists, and the final claim/touch-set is exact."]],
  ["pages-timeout-repair#1", ["deduplicated existing issue", "The bound report and audit classify the external deployment timeout as a new defect already owned by S.I.R. #370; PR #371 raised the bounded Pages timeout and proved deployment, so this roll-up deduplicates it."]],
]);

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
  const judgement = nonPositiveJudgements.get(key);
  need(judgement, `missing checkpoint-specific non-positive judgement: ${key}`);
  return [judgement[0], `${judgement[1]} Retained evidence: ${row.evidence}`];
}

function occurrences(text, value) { return text.split(value).length - 1; }

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
  const nonPositiveRationales = new Set();
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
      const expected = dispositionFor(cycle.short, index + 1, source);
      need(row.disposition === expected[0] && row.rationale === expected[1], `checkpoint-specific rationale mismatch: ${key}`);
      if (source.kind !== "positive-pattern") {
        need(!row.rationale.startsWith("Accepted as a cycle-bounded"), `generic non-positive rationale: ${key}`);
        const judgementText = row.rationale.slice(0, row.rationale.lastIndexOf(" Retained evidence:"));
        need(judgementText.length > 0, `missing checkpoint-specific judgement: ${key}`);
        need(!nonPositiveRationales.has(judgementText), `duplicated non-positive rationale: ${key}`);
        nonPositiveRationales.add(judgementText);
      }
      const visible = checkpointLine(cycle.short, index + 1, source, row.disposition, row.rationale);
      need(occurrences(report, visible) === 1, `visible checkpoint row drift: ${key}`);
      counts[row.disposition] += 1;
    });
    const visibleCycle = cycleLine(cycle);
    need(occurrences(report, visibleCycle) === 1, `visible delivery row drift: ${cycle.cycle}`);
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
function cycleLine(cycle) {
  const d = delivery[cycle.short];
  need(d, `missing delivery mapping: ${cycle.short}`);
  return `| \`${cycle.short}\` | ${linkFile("report", cycle.report)} / ${linkFile("audit", cycle.audit)} / ${linkFile("checkpoints", cycle.checkpoint)} | ${cycle.rows.length} | ${cycle.phases} | ${linkIssue(d[0])} / ${linkPr(d[1])} | \`${d[2]}\` | ${linkRun(d[3])} | ${linkRun(d[4])} |`;
}
function checkpointLine(short, sequence, row, disposition, rationale) {
  return `- **\`${short}#${sequence}\` — ${disposition}.** ${row.summary} Evidence: \`${row.evidence}\` Rationale: ${rationale}`;
}

function render(root) {
  const state = load(root);
  const checkpointCount = state.cycles.reduce((sum, cycle) => sum + cycle.rows.length, 0);
  const counts = Object.fromEntries([...dispositions].map(value => [value, 0]));
  const cycleLines = [];
  const cycleMarkers = [];
  const checkpointLines = [];
  const checkpointMarkers = [];
  for (const cycle of state.cycles) {
    cycleLines.push(cycleLine(cycle));
    cycleMarkers.push(marker("roadmap-rollup-cycle", { cycle: cycle.cycle, report: cycle.report, audit: cycle.audit, checkpoint: cycle.checkpoint, count: cycle.rows.length, phases: cycle.phases }));
    cycle.rows.forEach((row, index) => {
      const [disposition, rationale] = dispositionFor(cycle.short, index + 1, row);
      counts[disposition] += 1;
      checkpointLines.push(checkpointLine(cycle.short, index + 1, row, disposition, rationale));
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

The self-test isolates nine defects: omitted cycle, omitted checkpoint, wrong report/audit binding,
count mismatch, invalid disposition, unchecked milestone, reader-visible delivery drift, generic
rationale, and duplicated rationale. Each must observe its named red before
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
  const pristineRows = markers(pristine, "roadmap-rollup-checkpoint");
  const firstPositive = pristineRows.find(row => row.disposition === "positive pattern");
  const invalid = { ...firstPositive, disposition: "closed" };
  const invalidDispositionReport = pristine.replace(marker("roadmap-rollup-checkpoint", firstPositive), marker("roadmap-rollup-checkpoint", invalid));
  const nonPositive = pristineRows.filter(row => row.disposition !== "positive pattern");
  const genericRationale = `Accepted as a cycle-bounded ${nonPositive[0].kind} after the bound schema-v2 audit; retained evidence: ${nonPositive[0].evidence}`;
  const genericRow = { ...nonPositive[0], rationale: genericRationale };
  const genericRationaleReport = pristine
    .replace(checkpointLine(nonPositive[0].cycle.slice(cyclePrefix.length), nonPositive[0].sequence, nonPositive[0], nonPositive[0].disposition, nonPositive[0].rationale), checkpointLine(nonPositive[0].cycle.slice(cyclePrefix.length), nonPositive[0].sequence, nonPositive[0], nonPositive[0].disposition, genericRationale))
    .replace(marker("roadmap-rollup-checkpoint", nonPositive[0]), marker("roadmap-rollup-checkpoint", genericRow));
  const firstJudgement = nonPositive[0].rationale.slice(0, nonPositive[0].rationale.lastIndexOf(" Retained evidence:"));
  const duplicatedRationale = `${firstJudgement} Retained evidence: ${nonPositive[1].evidence}`;
  const duplicatedRow = { ...nonPositive[1], rationale: duplicatedRationale };
  const duplicatedRationaleReport = pristine
    .replace(checkpointLine(nonPositive[1].cycle.slice(cyclePrefix.length), nonPositive[1].sequence, nonPositive[1], nonPositive[1].disposition, nonPositive[1].rationale), checkpointLine(nonPositive[1].cycle.slice(cyclePrefix.length), nonPositive[1].sequence, nonPositive[1], nonPositive[1].disposition, duplicatedRationale))
    .replace(marker("roadmap-rollup-checkpoint", nonPositive[1]), marker("roadmap-rollup-checkpoint", duplicatedRow));
  const cases = [
    ["omitted-cycle", pristine.replace(/^<!-- roadmap-rollup-cycle .*? -->\n/m, ""), new Map(), /omitted cycle/],
    ["omitted-checkpoint", pristine.replace(/^<!-- roadmap-rollup-checkpoint .*? -->\n/m, ""), new Map(), /omitted checkpoint/],
    ["wrong-report-audit-binding", pristine, (() => { const state = load(root); const first = state.cycles[0]; const value = JSON.parse(readFileSync(join(root, first.audit), "utf8")); value.report = "feedback/wrong.md"; return new Map([[first.audit, JSON.stringify(value)]]); })(), /wrong report\/audit binding/],
    ["count-mismatch", pristine.replace("- **Checkpoint records:** 48", "- **Checkpoint records:** 47"), new Map(), /checkpoint total mismatch/],
    ["invalid-disposition", invalidDispositionReport, new Map(), /invalid disposition/],
    ["unchecked-milestone", pristine, new Map([[roadmapPath, readFileSync(join(root, roadmapPath), "utf8").replace("### - [x] M7", "### - [ ] M7")]]), /unchecked milestone/],
    ["visible-delivery-drift", pristine.replace("[S.I.R. #368](https://github.com/EHotwagner/S.I.R./issues/368)", "[S.I.R. #999](https://github.com/EHotwagner/S.I.R./issues/999)"), new Map(), /visible delivery row drift/],
    ["generic-rationale", genericRationaleReport, new Map(), /checkpoint-specific rationale mismatch|generic non-positive rationale/],
    ["duplicated-rationale", duplicatedRationaleReport, new Map(), /checkpoint-specific rationale mismatch|duplicated non-positive rationale/],
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
