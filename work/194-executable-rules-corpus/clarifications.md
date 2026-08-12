---
schemaVersion: 1
workId: 194-executable-rules-corpus
title: Executable Rules Corpus
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/194-executable-rules-corpus/spec.md
publicOrToolFacingImpact: true
---

# Executable Rules Corpus Clarifications

## Source Specification
- work/194-executable-rules-corpus/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Which project ownership preserves the existing Domain → Simulation → Client dependency direction and makes the evaluator available to Fable?
- CQ-002 [AMB:AMB-002]: Does the trace slice require a published Game.Core exact surface, and which metadata proves the boundary?
- CQ-003 [AMB:AMB-003]: Which numeric representation and rounding rule is canonical across .NET and JavaScript?
- CQ-004 [AMB:AMB-004]: Which first schema, retention layout, and artifact fingerprint bind an immutable package without stabilizing builder syntax?
- CQ-005 [AMB:AMB-005]: Which representative workload and bounded budgets qualify the slice without replacing structural counters with noisy timing assertions?

## Answers
- CQ-001 → public portable corpus value/AST/codec/evaluator contracts live in `SIR.Domain`; attack registrations and state transition integration live in `SIR.Simulation`; generated presentation adapters live in `SIR.Client`/`SIR.Client.Web`. No new framework project is introduced.
- CQ-002 → exposed-footprint sampling uses only `Los.lineOfSightBy`, explicitly `LockstepExact` in profile `fs-gg-game-core-fable-lockstep-v1`; the consumer pins `FS.GG.Game.Core` 0.13.0 and records its package SHA-256, fixture schema v1, SDK 10.0.302, FSharp.Core 10.1.302, Fable 5.13.0, Node 26, and observed browser identity from the restored package metadata and clean-consumer run.
- CQ-003 → all authoritative slice numerics use signed Q4 `FixedPoint` raw `int32` values with saturating arithmetic and round-to-nearest ties away from zero. Units/value kinds are explicit manifest identifiers; final damage is an explicitly rounded bounded integer. Floating point is presentation/performance measurement only.
- CQ-004 → manifest/explanation/package schemas start at version 1. Package identity uses SHA-256 over canonical length-prefixed little-endian projections: implementation identity covers sorted named .NET/Fable artifact digests and toolchain/profile identity; semantic identity covers executable rules/content plus implementation identity; manifest identity covers the complete payload plus the prior identities while omitting its own output. `tests/fixtures/rules-corpus/v1` retains the package/replay/oracle and pinned source revision; missing identities return a typed unavailable result.
- CQ-005 → the representative structural workload is one fully explained player-reachable attack plus a deterministic 10,000-evaluation batch. One attack may emit at most 32 applications, 128 disclosed operands, and 64 KiB canonical explanation bytes; the v1 manifest is at most 512 KiB and the archival replay/package remains inside the existing 1 MiB replay limit. The 10,000-evaluation Release smoke must remain under 2 seconds on the qualification host and the attack stays inside the existing 50 ms hard tick ceiling; structural counters are the stable gate and timings are recorded capability evidence.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-001] [FR-002]: Keep portable public rule contracts/evaluation in `SIR.Domain`, simulation registrations/transitions in `SIR.Simulation`, and projection-only rendering in client projects; declare signatures before implementation and preserve dependency direction.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-003] [FR-005]: Use only package-restored `FS.GG.Game.Core` 0.13.0 `Los.lineOfSightBy` as the trace algorithm's classified exact dependency, behind an S.I.R. exposed-footprint adapter; capture and compare the package oracle on .NET, Node/Fable, and a headless browser.
- **DEC-003** [CQ-003] [AMB:AMB-003] [FR-002] [FR-003] [FR-006]: Use existing canonical Q4 `FixedPoint` and explicit typed units/bounds throughout the authoritative slice, with its saturating and ties-away-from-zero rules included in schema and parity fixtures.
- **DEC-004** [CQ-004] [AMB:AMB-004] [FR-006] [FR-009]: Adopt schema v1 and three acyclic SHA-256 identities over canonical projections; retain an immutable `tests/fixtures/rules-corpus/v1` package/replay/oracle and expose a typed unavailable-package state. Builder surface syntax remains provisional and is not part of package identity.
- **DEC-005** [CQ-005] [AMB:AMB-005] [FR-012] [FR-013]: Qualify the exact candidate with the declared attack/batch workload, structural size/application budgets, existing replay/tick ceilings, recorded Release timings, and a real browser player journey; revisit builder/schema ergonomics after those measurements and before migration expands.

## Accepted Deferrals
- None. Every ambiguity is resolved for this vertical slice; mechanics outside SB-001 through SB-004 remain explicit non-scope rather than lifecycle deferrals.

## Remaining Ambiguity
- None. AMB-001 through AMB-005 are resolved by DEC-001 through DEC-005.

## Lifecycle Notes
- Decisions are binding implementation inputs; no accepted deferral needs a receiving milestone.
- Next lifecycle action: `fsgg-sdd checklist --work 194-executable-rules-corpus`.
