---
schemaVersion: 1
workId: 359-handbook-m1
title: Handbook M1 linked skeleton
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Handbook M1 linked skeleton Specification

Prose status: specified

## User Value
Quint learners and combat reviewers can navigate one publication skeleton through three complete reading paths.

## Scope
- SB-001: Create the handbook hierarchy, semantic anchors, mandatory traceability rows, initial definition index, checked vocabulary manifest, structural link audit, and README link; complete M1 only.

## Non-Goals
- SB-002: Do not implement later lifecycle commands or Governance enforcement in this specification.

## User Stories
- US-001 (P1): As a Quint learner, I can follow a complete handbook hierarchy through a recommended learning path without hidden sections.
- US-002 (P1): As a combat reviewer, I can jump from every planned rule, declaration, or controlled concept to one stable definition anchor.
- US-003 (P1): As a maintainer, I can audit the skeleton's links, vocabulary, and mandatory traceability rows mechanically.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the handbook, when any table-of-contents or reading-path link is followed, then it resolves to the intended section in the complete Part I-VIII hierarchy.
- AC-002 [US-002] [FR-002]: Given the M0 rule, declaration, and vocabulary inventories, when the handbook is audited, then every planned definition has one unique stable semantic anchor and an alphabetical seed index entry.
- AC-003 [US-003] [FR-003]: Given the traceability matrix, when mandatory coverage is counted, then it includes all sixteen rule IDs, DEC-001 through DEC-007, and every named behavior and correspondence obligation from the source design.
- AC-004 [US-003] [FR-004]: Given the checked vocabulary manifest and structural link audit, when the positive and deliberately broken fixtures run, then valid links pass and missing fragments, duplicate anchors, absent index entries, and unlinked controlled prose are detected without treating exempt code or headings as prose occurrences.
- AC-005 [US-001] [FR-005]: Given a clean documentation environment, when the S.I.R. docs pipeline builds, then the handbook is rendered and the README links to its published page.
- AC-006 [US-003] [FR-006]: Given the roadmap ledger, when M1 lands, then only M1 changes from unchecked to checked and concise completion evidence names the implementation and verification artifacts.

## Functional Requirements
- FR-001: The publication MUST provide front matter, title, a complete working table of contents, all fifty planned chapters grouped under Parts I-VIII, and three linked reading-path maps. (covers AC-001)
- FR-002: The publication MUST assign unique explicit semantic anchors to all planned rules, declarations, language terms, concepts, stats, units, properties, runs, and evidence terms from M0 and expose them through an initial alphabetical definition index. (covers AC-002)
- FR-003: The empty/full-shaped traceability matrix MUST include all mandatory source-design rows while marking later-milestone mappings and evidence honestly pending. (covers AC-003)
- FR-004: A checked manifest and structurally aware audit MUST validate fragments, anchor uniqueness, index membership, rule rows, and controlled linked occurrences, with positive and negative controls. (covers AC-004)
- FR-005: The handbook MUST build in the S.I.R. documentation pipeline and the root README MUST link its published route. (covers AC-005)
- FR-006: The roadmap MUST preserve existing wording/history, mark only M1 complete, and append concise implementation, test, lifecycle, feedback, PR, and merge evidence. (covers AC-006)

## Ambiguities
- AMB-001: The manifest lives at `docs/sir-combat-quint-vocabulary.json`; M6 may replace its hand-checked source with a generated projection while preserving its contract.
- AMB-002: The M1 audit is a dependency-free structural tokenizer/state machine rather than a full third-party CommonMark AST. It must distinguish front matter, fenced code, headings, links, and HTML anchors and is explicitly a prototype for M6 integration.
- AMB-003: Exercise solutions remain an appendix section in the single handbook for the first edition.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 359-handbook-m1`.
