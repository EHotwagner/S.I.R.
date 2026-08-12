---
schemaVersion: 1
workId: 194-executable-rules-corpus
title: Executable Rules Corpus
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/194-executable-rules-corpus/spec.md
sourceClarifications: work/194-executable-rules-corpus/clarifications.md
sourceChecklist: work/194-executable-rules-corpus/checklist.md
publicOrToolFacingImpact: true
---

# Executable Rules Corpus Plan

Prose status: planned

## Source Snapshot
- spec: work/194-executable-rules-corpus/spec.md sha256:d844d21fdf36544d64cf81435bed84d39cd23310450887b5c7f25a2ba84baf43 schemaVersion:1
- clarifications: work/194-executable-rules-corpus/clarifications.md sha256:b4f8ff52aa77e659a76291cbbbeb419390ce147687bb1f1e83a4f7fd7b8867a6 schemaVersion:1
- checklist: work/194-executable-rules-corpus/checklist.md sha256:85247c24711d286b974a9583d071d0359980bde86898079545612e521381be34 schemaVersion:1

## Plan Scope
- Declare portable rule/model/evaluator/codec signatures in `SIR.Domain`, then implement deterministic AST evaluation, registry validation, canonical manifest/explanation encodings, and package identity.
- Register the combat facts/formulas/trace algorithm/transition in `SIR.Simulation`, integrate the explained attack with the existing authoritative event route, and extend replay identity/resolution compatibly.
- Generate projection-only rule explorer/source links in client/docs, retain schema-v1 package/replay fixtures, and qualify .NET, package-derived Fable/Node, headless browser, real player journey, coverage, negative gates, performance, and historical resolution.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Declare `Rules.fsi` discriminated unions/records for identities, metadata, typed values, expressions, transitions, algorithms, applications, source/evidence, and registry entries before implementing the compact provisional builders.
- PD-002 [AC-001] [FR-002] [DEC-003] complete: Implement one pure registry validator and expression evaluator over Q4 `FixedPoint`, explicit unit/value-kind identifiers, checked operands, deterministic ordering, total failures, and registered algorithm contracts.
- PD-003 [AC-002] [FR-003] [DEC-002] [DEC-003] complete: Author combat facts/formulas in `CombatRules.fs` and an exposed-footprint trace adapter whose only Game.Core authority is package-restored `Los.lineOfSightBy`; register one attack transition rather than duplicating existing state mechanics.
- PD-004 [AC-002] [FR-004] complete: Extend the authoritative attack event with a package-qualified rule-application root and build its deterministic application graph from actual evaluation outputs, preserving summary/detail as rendering projections.
- PD-005 [AC-003] [FR-005] [DEC-002] complete: Compile shared Domain/Simulation sources for .NET and Fable, restore Game.Core into an isolated package-only consumer, compare complete canonical bytes with product and package oracles on Node and Playwright, and record exact toolchain/browser identities.
- PD-006 [AC-004] [FR-006] [DEC-004] complete: Define contract `rules-manifest-v1` and canonical length-prefixed encoders; hash sorted named implementation artifacts first, executable semantics plus that digest second, and complete manifest payload plus prior identities third, with output fields outside their own inputs.
- PD-007 [AC-005] [FR-007] complete: Generate deterministic JSON plus F# projection records from the registry; render formula notation, typed parameter tables, dependencies, rationale, examples/properties, evidence, status, and coverage in static and web views with no formula interpreter in JavaScript.
- PD-008 [AC-005] [FR-008] complete: Resolve source symbols/repository-relative paths at build time and build current/historical links from the manifest's commit, package digest, rule ID, event/entity, and formula context; fail broken paths/symbols/deep links.
- PD-009 [AC-006] [FR-009] [DEC-004] complete: Extend replay v3 with engine/profile and three package digests plus embedded explanation/package option, retain `tests/fixtures/rules-corpus/v1`, and resolve exact identity or `HistoricalRulePackageUnavailable` without substituting current state.
- PD-010 [AC-007] [FR-010] complete: Add focused validator/codec/replay/link/portability gates and a bounded mutation harness that corrupts each protected subject plus unreadable inputs, records the expected red, then restores the candidate.
- PD-011 [AC-008] [FR-011] complete: Emit a sorted coverage graph from each migrated rule to implementation, event, application, example/property, documentation, source, and replay fixture; classify non-slice authority as legacy and make CI reject unclassified migrated behavior or copied semantics.
- PD-012 [AC-009] [FR-012] [DEC-005] complete: Add a real entry/input Playwright attack journey and Release qualification for one attack and 10,000 evaluations, asserting structural budgets and recording timing/capability facts without noisy wall-clock unit assertions.
- PD-013 [AC-010] [FR-013] [DEC-005] complete: Publish signatures/schema/baselines, document the post-slice authoring/schema review and migration boundary, refresh lifecycle/generated views, and finalize validated schema-v2 feedback before PR handoff.

## Contract Impact
- PC-001 [PD-001] [PD-002] F# API: `SIR.Domain.Rules` declares schema-v1 rule, expression, registry, validation, application, package, coverage, and canonical-codec surfaces in `Rules.fsi`; `SIR.Simulation.CombatRules` declares the registered slice and attack evaluation boundary.
- PC-002 [PD-003] framework: FS.GG.Game.Core@0.13.0#Los.lineOfSightBy
- PC-003 [PD-006] [PD-007] data-contract: `work/194-executable-rules-corpus/contracts/rules-manifest-v1.md` defines canonical JSON/byte ordering and implementation/semantic/manifest digest projections.
- PC-004 [PD-004] [PD-009] data-contract: `work/194-executable-rules-corpus/contracts/rule-application-replay-v1.md` defines application DAG identity, decisive operands, event/package binding, replay v3 compatibility, immutable lookup, and unavailable-package failure.
- PC-005 [PD-011] generated-contract: `work/194-executable-rules-corpus/contracts/rules-coverage-v1.md` defines the machine-readable coverage nodes/edges and legacy classification.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-004] [PC-001] semanticTest: Run focused Domain/Simulation tests for every semantic kind, typed/unit/boundary failures, actual combat values, complete explained attack, and formula-change propagation.
- VO-002 [PD-005] [PC-002] crossRuntimeTest: From clean package caches compare the packed Game.Core oracle and product canonical fixture on .NET, package-derived Fable/Node, and the built ES module in a pinned headless browser.
- VO-003 [PD-006] [PD-009] [PC-003] [PC-004] codecTest: Assert golden canonical bytes, stable ordering, all three identity change matrices, no digest recursion, exact old-package resolution, and typed unavailable state.
- VO-004 [PD-007] [PD-008] [PD-011] [PC-005] generatedViewTest: Generate manifest/explorer/coverage outputs twice, compare bytes, validate source symbols/paths and deep links, and reject independent client semantics or unclassified migrated authority.
- VO-005 [PD-010] gateMutationTest: For every added/changed test, script, schema, parser, fixture, and workflow gate break its protected subject and unreadable-input path once and retain the observed non-zero result.
- VO-006 [PD-012] playerJourneyTest: Boot the production browser entry, issue only player-emittable controls, resolve and inspect the combat attack, and prove package/rule/source navigation without direct `Msg` injection or seeded mid-game state.
- VO-007 [PD-012] performanceTest: Run the exact Release candidate's attack and 10,000-evaluation workloads, assert 32 applications/128 operands/64 KiB explanation/512 KiB manifest/1 MiB replay and 50 ms tick ceilings, and record the 2-second batch observation with host capability.
- VO-008 [PD-013] lifecycleTest: Produce passing TRX/JUnit/manifest/replay/browser reports, bind SDD evidence receipts, refresh/verify/ship, validate feedback schema-v2, and run exact CI before review.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-003] [PC-004] compatible-versioned: Add manifest v1 and replay v3 readers while retaining existing replay v1/v2 decoding; only v3 carries rule-package explanations, and unsupported/missing packages diagnose explicitly.
- PM-002 [PC-005] incremental-authority: Only listed combat rules become `Corpus`; every other mechanic remains `Legacy` and cannot be counted as migrated coverage until a later independently specified item.
- PM-003 [PC-001] provisional-builder: Keep builder combinators internal/provisional while public erased contracts and schema v1 are reviewed after the slice; document findings before broader migration.

## Generated View Impact
- GV-001 [PD-006] [PD-007] [PD-011] registryViews: Manifest JSON, rule explorer, formula documentation, source-link index, coverage graph, and retained package fixture regenerate deterministically from the typed registry and report stale/missing inputs.
- GV-002 [PD-013] lifecycleViews: Analysis, work model, summary, equivalent Claude/Codex guidance, verify, ship, and committed ship verdict regenerate from current authored sources and evidence.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Performance declaration is owned by DEC-005/FR-012 and the executable workload contract because this SDD consumer's seeded spec has no populated typed performance-intent front matter; evidence must still carry exact workload/budget/capability facts.
- Optional Governance pointers remain compatibility facts only; SDD reports readiness and does not substitute for package/browser/player-route evidence.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 194-executable-rules-corpus`.
