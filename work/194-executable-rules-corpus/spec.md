---
schemaVersion: 1
workId: 194-executable-rules-corpus
title: Executable Rules Corpus
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Executable Rules Corpus Specification

Prose status: specified

## User Value
Players can inspect why one attack produced its result and navigate to the governing rationale and pinned source. Developers and agents can change one canonical rule definition and obtain aligned .NET/Fable behavior, documentation, explanations, package identity, and replay evidence without maintaining copied semantics.

## Scope
- SB-001: Add S.I.R.-owned typed rule values, metadata, facts, predicates, formulas, transitions, registered algorithms, narrative declarations, registry validation, evaluator, canonical codecs, explanation records, package identities, and coverage graph.
- SB-002: Migrate one coherent combat path: engagement preparation, exposed-footprint trace probability, armor retention, expected damage, representative weapon/body parameters, and one authoritative explained attack event.
- SB-003: Generate deterministic manifest, explorer/documentation, source links, replay bundle, retention fixture, and .NET/Fable/Node/browser qualification from the same F# corpus.
- SB-004: Integrate the slice with the real simulation and player-reachable client route while leaving all mechanics outside the declared combat slice explicitly legacy/unmigrated.

## Non-Goals
- SB-005: Do not create a general-purpose requirements/rules framework, controlled-English executor, Datalog simulation, public modding language, arbitrary-F# introspector, or parallel TypeScript/JavaScript gameplay implementation.
- SB-006: Do not migrate every S.I.R. mechanic, make graphics issue #192 a prerequisite, or make issue #185's Docs presentation shell semantic authority.
- SB-007: Do not stabilize provisional builder punctuation or manifest schema beyond the versioned vertical-slice contract before the required ergonomics review.

## User Stories
- US-001 (P1): As a player reviewing combat, I can open an attack event and inspect its decisive typed operands, formula/algorithm steps, outcome, rationale, evidence, and pinned F# source.
- US-002 (P1): As a rule author, I can change one typed F# parameter or formula and have execution, generated views, explanations, and package identities update together.
- US-003 (P1): As a replay reviewer, I can resolve an old attack against its immutable historical package after current rules and implementation artifacts change, or receive an explicit unavailable-package result.
- US-004 (P1): As a maintainer, I receive actionable qualification failures when registration, units, source/evidence, canonical encoding, package identity, replay binding, projection coverage, or runtime parity is invalid.

## Acceptance Scenarios
- AC-001 [US-002] [FR-001] [FR-002]: Given the combat corpus is registered, when its definitions are enumerated and evaluated, then each stable rule ID has one validated kind, status, controlled statement, rationale, typed dependencies, source/evidence, and executable or explicit narrative semantics.
- AC-002 [US-001] [FR-003] [FR-004]: Given a player starts the product through its real entry and input surface, when one representative attack resolves, then the emitted event carries a complete deterministic explanation from engagement preparation through trace, armor retention, and expected damage with representative weapon/body values.
- AC-003 [US-002] [FR-005]: Given one canonical combat parameter or formula changes, when .NET, Fable/Node, and browser qualification runs, then all routes emit the same canonical values, applications, event, bytes, and digests and no copied TypeScript/JavaScript semantic implementation exists.
- AC-004 [US-002] [FR-006]: Given semantic, implementation-artifact, manifest-only, and volatile-build changes are applied separately, when package identity is generated, then exactly the ADR-defined semantic, implementation, and manifest digests change and no digest includes itself or a covered artifact that embeds the resulting digest.
- AC-005 [US-001] [FR-007] [FR-008]: Given the explained attack is selected, when current or static explorer views render it, then every node navigates to formula notation, parameters, dependencies, rationale, examples/properties, coverage, event/entity context, and commit-pinned F# source without interpreting manifest formulas as gameplay authority.
- AC-006 [US-003] [FR-009]: Given a retained old replay/package and a different current package, when historical resolution runs, then the old replay resolves only its recorded engine/profile/package/source identities; removal produces an explicit unavailable-package state rather than current-rule reinterpretation.
- AC-007 [US-004] [FR-010]: Given duplicate/dangling IDs, incompatible statuses, unresolved implementation or source identities, missing evidence/explanations, unit/finite errors, unsupported Fable constructs, digest recursion or omitted algorithm fingerprints, stale replay bindings, copied semantics, nondeterministic ordering, or broken links, when qualification runs, then the owning gate fails with an actionable diagnostic.
- AC-008 [US-004] [FR-011]: Given the declared coverage boundary, when CI inspects implementation, events, explanations, examples/properties, documentation, sources, and replay fixtures, then every migrated combat authority is connected and every outside mechanic is honestly classified rather than silently claimed.
- AC-009 [US-001] [FR-012]: Given the exact candidate and representative explained replay workload, when release qualification runs, then deterministic size/cost counters remain within the declared budgets and the browser player journey passes from the product entry without direct message injection or seeded mid-game state.
- AC-010 [US-002] [FR-013]: Given the vertical slice is complete, when authors review the builder and schema, then findings and provisional decisions are recorded before broad migration and generated documentation/replay/manifest artifacts remain reproducible from a clean checkout.

## Functional Requirements
- FR-001: The system MUST expose a layered typed F# corpus for facts, predicates, formulas, transitions, registered algorithms, and narrative rules with stable IDs, status/taxonomy, controlled statements, rationale, typed dependencies, supersession, source, examples/properties, and evidence. (covers AC-001)
- FR-002: The registry and evaluator MUST validate typed value kinds, units, bounds, rounding, finite behavior, portable canonical values, deterministic ordering, metadata completeness, dependencies, and algorithm contracts/fingerprints while preserving ordinary F# as the registered-algorithm escape hatch. (covers AC-001)
- FR-003: The first combat slice MUST define engagement preparation, exposed-footprint trace probability through a registered algorithm boundary, armor retention, expected damage, representative weapon/body facts, and one attack-resolution transition using the corpus. (covers AC-002)
- FR-004: The authoritative simulation MUST emit one rule-application graph containing stable rule IDs, decisive typed operands, outcomes/effects, nested applications, event identity, phase/order, and package identity, with summary and detailed renderings that do not alter semantics. (covers AC-002)
- FR-005: The exact same portable F# evaluator and combat definitions MUST execute on .NET and Fable, with package-only use of explicitly classified `FS.GG.Game.Core` surfaces and byte-identical canonical fixture results under pinned .NET, FSharp.Core, Fable, Node, and browser identities. (covers AC-003)
- FR-006: The generator MUST emit deterministic versioned manifest payloads and separate non-recursive implementation, semantic, and manifest digests with canonical ordering/codecs, volatile data excluded, algorithm artifacts covered, and each digest input/output boundary matching ADR-0001. (covers AC-004)
- FR-007: Static documentation and the web rule explorer MUST generate formula notation, parameters, dependencies, rationale, examples/properties, evidence/coverage, implementation links, and stable package-qualified deep links from the registry without independent gameplay formula code. (covers AC-005)
- FR-008: Simulator events, entities, formulas, and replay frames MUST navigate to the governing rule and commit-pinned source symbol/path; historical links MUST use the replay package source revision and never a moving branch. (covers AC-005)
- FR-009: Replay records MUST bind engine, profile, semantic, implementation, manifest, and source identities; retained packages and a self-describing archival fixture MUST resolve immutably, and missing packages MUST yield an explicit unavailable state without reinterpretation. (covers AC-006)
- FR-010: Qualification MUST fail on every invalid-registration, type/unit, portability, digest, fingerprint, determinism, explanation, source, evidence, replay, retention, copied-semantics, or broken-link condition enumerated by AC-007, and every added/changed gate MUST be proven red by a protected-subject mutation. (covers AC-007)
- FR-011: A machine-readable coverage graph and exact CI MUST connect each migrated rule to implementation, events, explanations, examples/properties, documentation, source, and replay fixtures while identifying legacy/outside authority explicitly. (covers AC-008)
- FR-012: Release evidence MUST include the real bot-driven browser journey plus deterministic explanation/replay size and evaluation/application counters for a representative workload, with declared budgets and exact-candidate evidence. (covers AC-009)
- FR-013: The change MUST publish declared F# signatures and versioned schema/baselines, record a post-slice authoring-ergonomics/schema review, refresh SDD/generated views, and produce schema-v2 development feedback before review handoff. (covers AC-010)

## Ambiguities
- AMB-001: Which existing project owns the initial public corpus types and evaluator while preserving dependency direction and Fable compilation?
- AMB-002: Which exact `FS.GG.Game.Core` LockstepExact operation, if any, is genuinely required for the exposed-footprint/trace algorithm, and how is the package profile/toolchain identity captured?
- AMB-003: What bounded canonical numeric representation and rounding rule will the vertical slice use for probabilities, durations, retention, and damage?
- AMB-004: What exact manifest/replay schema version, retention layout, and implementation-artifact fingerprint form are sufficient for an immutable first fixture without prematurely freezing the builder API?
- AMB-005: What performance budgets and workload counters apply to corpus evaluation, explanation size, manifest size, and retained replay size for this slice?

## Public Or Tool-Facing Impact
- Tier 1: new public F# signatures, manifest/explanation/replay schemas, generated documentation and web projection, source links, fixtures, exact CI gates, and historical package retention policy.
- Existing external simulation and replay protocols remain compatible unless their versioned boundary is deliberately extended with explicit migration and baselines.

## Lifecycle Notes
- All AMB items require recorded decisions in clarify; none may be silently deferred to a later milestone.
- Next lifecycle action: `fsgg-sdd clarify --work 194-executable-rules-corpus`.
