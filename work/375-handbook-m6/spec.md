---
schemaVersion: 1
workId: 375-handbook-m6
title: Handbook M6
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Handbook M6 Specification

Prose status: specified

## User Value
A Quint learner can reach a complete definition for every controlled term in one click, while a maintainer can evolve the handbook without silently breaking its controlled vocabulary, declarations, rules, chapters, or internal links.

## Scope
- SB-001: Complete only roadmap M6: definitions, canonical aliases, related links, manifest reconciliation, structural Markdown-AST auditing, negative controls, docs integration, and lifecycle/delivery evidence.

## Non-Goals
- SB-002: Do not alter combat semantics, model declarations, runtime behavior, package pins, or existing evidence claim boundaries.
- SB-003: Do not implement M6V SVG mechanics/theory visuals, progressive effects, accessibility/fallback/render/performance work, or M7 editorial/publication handoff.

## User Stories
- US-001 (P1): As a learner, I can follow any controlled term or alias to one complete canonical definition with useful related links and honest runtime correspondence.
- US-002 (P1): As a maintainer, I can prove the vocabulary manifest, all top-level Quint declarations, sixteen stable rules, fifty chapters, and index entries reconcile exactly.
- US-003 (P1): As a reviewer, I can rely on a structural Markdown audit that excludes only declared syntax regions and rejects broken links, duplicate anchors, absent index entries, and unlinked terms.
- US-004 (P1): As a roadmap owner, I can verify M6 landed alone while M6V and M7 remain pending and fully scoped.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given any controlled term, when its index entry is inspected, then it contains a substantive definition, declaration locus, at least one related canonical link, and an honest runtime-correspondence statement without `Pending` or planned-definition placeholders.
- AC-002 [US-001] [FR-002]: Given a supported alias, when its manifest record or handbook occurrence is inspected, then it points to exactly one canonical term/anchor and the canonical entry exposes the alias relationship.
- AC-003 [US-002] [FR-003]: Given the literate Quint model and stable rule registry, when reconciliation runs, then every top-level declaration and all sixteen rules appear exactly once in the manifest and index with the expected kind/anchor.
- AC-004 [US-002] [FR-004]: Given the handbook, when navigation reconciliation runs, then all fifty chapter targets, three reading paths, every manifest target, and every internal fragment resolve exactly once.
- AC-005 [US-003] [FR-005]: Given prose outside front matter, fences, headings, raw anchors, the canonical index, and explicit per-term exemption regions, when the AST audit runs, then every controlled occurrence is linked to its canonical target.
- AC-006 [US-003] [FR-006]: Given mutations for a missing fragment, duplicate anchor, absent index entry, and unlinked controlled occurrence, when each isolated fixture is audited, then its named detector observes red and the untouched handbook restores green.
- AC-007 [US-004] [FR-007]: Given merge-candidate scope, when dedicated qualification and lifecycle receipts are inspected, then only M6 is checked and M6V/M7 remain pending with their complete requirements unchanged.

## Functional Requirements
- FR-001: Every manifest term MUST have one substantive canonical index entry containing definition, declaration locus, related canonical link, and scoped runtime correspondence; placeholder/Pending text MUST be absent. (covers AC-001)
- FR-002: The manifest and handbook MUST declare aliases explicitly, bind each alias to exactly one canonical term/anchor, and surface alias relationships at the canonical index entry. (covers AC-002)
- FR-003: A mechanical reconciliation MUST derive top-level declarations from `docs/rules/sir-combat.md` and stable rule IDs from the registry/roadmap inventory, then prove exact manifest/index coverage without missing, extra, duplicate, or kind-mismatched entries. (covers AC-003)
- FR-004: Structural auditing MUST prove exactly one target for every internal fragment plus complete coverage of fifty chapters, three reading paths, all manifest anchors, and all mandatory traceability subjects. (covers AC-004)
- FR-005: Controlled-occurrence enforcement MUST operate over a parsed Markdown block/inline structure, exclude only manifest-declared structural regions, and fail on unlinked occurrences outside those exemptions. (covers AC-005)
- FR-006: Qualification MUST run isolated missing-fragment, duplicate-anchor, absent-index-entry, and unlinked-occurrence mutations, require the named detector for each observed red, then rerun untouched inputs green. (covers AC-006)
- FR-007: Qualification MUST own separate strict-docs, structural/reconciliation, negative-control/restoration, roadmap, and lifecycle receipts; the ledger MUST mark only M6 at merge-candidate scope and preserve M6V/M7. (covers AC-007)

## Ambiguities
- AMB-001: The M1 audit approximates structure by lines and regexes; M6 needs a deterministic AST-shaped parser without adding an unpinned package dependency.
- AMB-002: Controlled symbols legitimately occur in code spans and links, while ordinary prose must link them; exemptions must be structural and explicit rather than broad text suppression.
- AMB-003: A complete definition index must be useful without becoming a second semantic authority; declarations and runtime correspondence must point back to named authoritative subjects.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Issue: `EHotwagner/S.I.R.#375`.
- Stable feedback cycle: `roadmap-sir-combat-quint-handbook-m6-index-link-enforcement`.
- This work accepts the M0/M1 address/enforcement and M2–M5 definition/alias deferrals and must close them before ship.
- Next lifecycle action: `fsgg-sdd clarify --work 375-handbook-m6`.
