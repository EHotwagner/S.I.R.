---
schemaVersion: 1
workId: 375-handbook-m6
title: Handbook M6
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/375-handbook-m6/spec.md
publicOrToolFacingImpact: true
---

# Handbook M6 Clarifications

## Source Specification
- work/375-handbook-m6/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001] blocking open: Resolve source ambiguity AMB-001 before checklist.
- CQ-002 [AMB:AMB-002] blocking open: Resolve source ambiguity AMB-002 before checklist.
- CQ-003 [AMB:AMB-003] blocking open: Resolve source ambiguity AMB-003 before checklist.

## Answers
- CQ-001 [AMB:AMB-001] decision: Parse Markdown into deterministic block and inline nodes in a committed dependency-free audit, rather than adding a package or using line suppression.
- CQ-002 [AMB:AMB-002] decision: Exempt front matter, fenced code, headings, raw anchor nodes, canonical index, and explicit manifest exemption regions; links and code spans retain typed node identity.
- CQ-003 [AMB:AMB-003] decision: Definitions explain and link to named authoritative declarations/rules/runtime subjects; they do not copy executable semantics.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001]: Parse Markdown into deterministic block and inline nodes in a committed dependency-free audit, rather than adding a package or using line suppression.
- DEC-002 [CQ-002] [AMB:AMB-002]: Exempt front matter, fenced code, headings, raw anchor nodes, canonical index, and explicit manifest exemption regions; links and code spans retain typed node identity.
- DEC-003 [CQ-003] [AMB:AMB-003]: Definitions explain and link to named authoritative declarations/rules/runtime subjects; they do not copy executable semantics.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
No blocking ambiguity remains.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 375-handbook-m6`.
