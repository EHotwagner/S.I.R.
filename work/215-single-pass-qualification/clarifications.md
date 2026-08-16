---
schemaVersion: 1
workId: 215-single-pass-qualification
title: Single-Pass Production Qualification
stage: clarify
sourceSpec: work/215-single-pass-qualification/spec.md
changeTier: tier1
status: clarified
---

# Single-Pass Production Qualification Clarifications

## Source Specification
- `work/215-single-pass-qualification/spec.md`

## Clarification Questions
- **CQ-001** (AMB-001): Which boundary owns receipt production and downstream reuse?
- **CQ-002** (AMB-002): Which inputs and tool identities bind output currency?
- **CQ-003** (AMB-003): How can feedback evidence remain exact-head without rerunning the full aggregate after feedback-only commits?
- **CQ-004** (AMB-004): How is a material comparable wall-time reduction established?
- **CQ-005** (AMB-005): Which mutation proves stale reuse and restoration?

## Answers
- CQ-001 → one qualification script owns build/receipt production; a separate verify-only mode reads the immutable receipt, re-derives identities, and downstream docs/delivery/browser commands consume only verified paths from it.
- CQ-002 → bind Git commit/tree/clean state; enumerated source, configuration, script, project, tool-manifest, package manifest, and dependency-lock digests; Git, .NET SDK, Fable, Node, npm, Vite, and fsdocs identities; the owning command schema; and recursive content identities for every expected output root.
- CQ-003 → a focused command receipt binds exact commit/tree, clean state, command identity, relevant inputs/tools, result, and output evidence. Feedback audit validation verifies that focused receipt at the report head; feedback/report/audit bytes are excluded only when the owning command declares them irrelevant.
- CQ-004 → run the old and candidate production qualification routes from comparable clean worktrees on the same host, record wall-clock seconds plus host/tool/subject inventories, and require the candidate to remove at least one complete duplicate Fable client/Rules build and produce a clearly material reduction (at least 20 percent).
- CQ-005 → copy the immutable receipt to a temporary location, mutate one bound output or input identity in the temporary verification fixture, require the real verifier to fail with the named stale subject, and restore/check the production subject byte-for-byte in a `finally`/trap boundary.

## Decisions
- **DEC-001** [AMB:AMB-001] [FR-001] [FR-003] [FR-004]: The aggregate producer is the sole writer of a canonical content-addressed receipt; all reuse is verify-only and never silently rebuilds or refreshes stale output.
- **DEC-002** [AMB:AMB-002] [FR-002] [FR-003] [FR-006]: Receipt currency is a closed, sorted inventory of revision/tree/clean state, declared input-file digests, exact tool versions, owning command/schema, expected relative output roots, and recursive output digests.
- **DEC-003** [AMB:AMB-003] [FR-008] [FR-010]: Feedback command evidence uses immutable focused receipts whose declared subject excludes feedback-only metadata from product inputs; validator exact-head checks remain fail-closed and never infer a pass from prose.
- **DEC-004** [AMB:AMB-004] [FR-009]: Timing evidence compares baseline and candidate on the same host and equivalent subject inventory, records raw wall time, and treats at least 20 percent reduction plus eliminated duplicate target counts as material.
- **DEC-005** [AMB:AMB-005] [FR-007]: The stale-reuse mutation exercises the production verifier against a temporary changed bound subject, requires the specific red diagnostic, and proves the original tracked/build subject digest is unchanged afterward.

## Accepted Deferrals
- None.

## Remaining Ambiguity
- None. AMB-001 through AMB-005 are resolved by DEC-001 through DEC-005.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 215-single-pass-qualification`.
