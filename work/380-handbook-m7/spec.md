---
schemaVersion: 1
workId: 380-handbook-m7
title: Handbook M7
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Handbook M7 Specification

Prose status: specified

## User Value
Maintainers and learners can rely on a reviewed, published Combat in Quint handbook with an explicit update owner.

## Scope
- SB-001: Domain, Quint/model, beginner, rendered-document, source/tool identity, and maintenance handoff review; no combat semantic, diagram, performance-budget, package, or cross-cycle-roll-up change.

## Non-Goals
- SB-002: Do not change combat or Quint semantics, runtime implementation, diagram SVG bytes, glyphs, effects, visual budgets, or tool/package pins.
- SB-003: Do not create the parent-owned cross-cycle roadmap roll-up or claim a live compositor/frame-pacing measurement.

## User Stories
- US-001 (P1): As a beginner, I can follow the rendered handbook from setup through prediction, execution, trace reading, and explanation without relying on author context.
- US-002 (P1): As a domain or Quint reviewer, I can approve explicit authority, granularity, evidence, and claim boundaries without discovering a duplicate semantic authority.
- US-003 (P1): As a maintainer, I can identify the exact last-verified source/tool inputs and know which changes require the handbook publication suite to run.
- US-004 (P1): As a publication owner, I can prove the exact merged handbook and its six accessible diagrams are live.

## Acceptance Scenarios
- AC-001 [US-002] [FR-001]: Given the completed handbook, when independent domain and Quint/model reviewers inspect it, then each approves the authority, atomicity, correspondence, and formal-claim boundaries with no unresolved material finding.
- AC-002 [US-001] [FR-002]: Given a fresh beginner, when they follow the rendered learning spine, then every step, prediction, command, expected observation, and next route is complete and link-resolvable.
- AC-003 [US-004] [FR-003]: Given the rendered publication, when all six diagrams are inspected in normal, reduced-motion, print, effects-off, CSS-disabled, static, and non-WebGL meaning routes, then accessibility and semantic equivalence remain approved.
- AC-004 [US-003] [FR-004]: Given a maintainer opens the handbook, when they inspect last verification, then exact Git source identities and pinned .NET, Node, FsDocs, Quint, Playwright, SDD, and coordination versions are mechanically current.
- AC-005 [US-003] [FR-005]: Given an authority, model, runtime registry, vocabulary, diagram, qualification, or toolchain input changes, when maintenance is evaluated beside the model, then the named S.I.R. documentation owner and dependency-ordered checklist require the affected gates.
- AC-006 [US-002] [FR-006]: Given M6/M6V evidence, when M7 qualification runs, then exact structure/link coverage and the existing typed render/performance receipts are replayed or digest-bound without a new performance budget or stronger claim.
- AC-007 [US-003] [FR-007]: Given every new M7 publication gate, when its subject is deliberately mutated in isolation, then the named detector observes red before untouched restoration is green.
- AC-008 [US-004] [FR-008]: Given the candidate and merged main, when qualification and delivery run, then strict docs, lifecycle, feedback, relevant-only CI, exact-main CI, Pages deployment, live content, issue closure, project Done, and ledger evidence are verified.

## Functional Requirements
- FR-001: The publication MUST retain explicit independent domain and Quint/modeling approvals that verify authority, action granularity, correspondence, and sampled-versus-exhaustive claim limits. (covers AC-001)
- FR-002: The publication MUST retain an independent beginner approval over the actual rendered setup-to-explanation walkthrough, including commands, predictions, observations, navigation, and jargon introduction. (covers AC-002)
- FR-003: The publication MUST retain independent rendered-document approval for all six M6V SVGs and their accessible normal, reduced-motion, print, effects-off, CSS-disabled, static, and non-WebGL meaning. (covers AC-003)
- FR-004: The handbook MUST publish mechanically checked exact last-verified Git source identities and pinned toolchain versions without turning the record into semantic authority. (covers AC-004)
- FR-005: A maintenance trigger beside `docs/rules/sir-combat.md` MUST name the S.I.R. documentation owner, triggering paths, ordered update checklist, and claim-limit review. (covers AC-005)
- FR-006: M7 qualification MUST rerun M6 link/definition gates and bind or replay M6V accessibility/render/performance evidence while preserving its declared workload, budgets, and capability limits. (covers AC-006)
- FR-007: Every new M7 review, identity, maintenance, linkage, and publication gate MUST have an isolated observed-red/restored-green control that mutates the claimed subject. (covers AC-007)
- FR-008: The exact-head and exact-main delivery boundary MUST prove strict docs, SDD analyze/verify/ship, feedback state, relevant hosted CI, merge, exact-SHA Pages/live content, roadmap evidence, and Done state. (covers AC-008)

## Ambiguities
- AMB-001: Decide whether reviewer approvals are prose attestations or mechanically structured records with exact source and reviewer boundaries.
- AMB-002: Decide how to report exact last-verified identities without creating a permanently stale handwritten snapshot.
- AMB-003: Decide how M7 consumes M6V performance/render evidence without silently inventing a new performance intent or overclaim.

## Public Or Tool-Facing Impact
- The maintained public handbook, its authority-adjacent maintenance trigger, and its exact-head publication qualification change; no runtime or package surface changes.

## Lifecycle Notes
- Issue: `EHotwagner/S.I.R.#380`; feedback cycle: `roadmap-sir-combat-quint-handbook-m7-publication-handoff`.
- M7 cannot complete the final cross-cycle roll-up, which remains with the roadmap host after this merge.
- Next lifecycle action: `fsgg-sdd clarify --work 380-handbook-m7`.
