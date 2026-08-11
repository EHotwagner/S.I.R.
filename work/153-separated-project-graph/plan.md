---
schemaVersion: 1
workId: 153-separated-project-graph
title: Separated Project Graph
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/153-separated-project-graph/spec.md
sourceClarifications: work/153-separated-project-graph/clarifications.md
sourceChecklist: work/153-separated-project-graph/checklist.md
publicOrToolFacingImpact: true
---

# Separated Project Graph Plan

Prose status: planned

## Source Snapshot
- spec: work/153-separated-project-graph/spec.md sha256:516a9acbc98c57e4e21eb1a343dc1c40fba50cbc8c8a24879a7c492d197d4928 schemaVersion:1
- clarifications: work/153-separated-project-graph/clarifications.md sha256:7a579efa101f1cfb33aff28055b5a1f630ac428d4638e164387e036b25e2d76c schemaVersion:1
- checklist: work/153-separated-project-graph/checklist.md sha256:4ca753b0bff4d3bbeee281de0fef555ef4117ea704759fed914b2d342e8f5c6d schemaVersion:1

## Plan Scope
- Extract the Wasmtime control adapter from `SIR.Match` into `SIR.Wasm`, retaining `SIR.Match` as the composition point.
- Introduce an explicit generated-protocol leaf project and a replay-web Fable host, then move existing browser host ownership there without changing the released transport.
- Split live-client-safe source from replay/editor source so `SIR.Client` no longer references simulation.
- Add a solution/documentation dependency graph verifier and validate the normal production route.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Create the four named projects, add them to `SIR.slnx`, and move runtime-specific and browser-host compilation ownership to their named boundaries.
- PD-002 [AC-002] [FR-002] complete: Keep presentation-safe types in `SIR.Client`; move Map Editor, simulation laboratory, replay presentation, and browser host code to `SIR.Replay.Web` so the live client only references Domain and Protocol.
- PD-003 [AC-003] [FR-003] complete: Add a deterministic script that parses solution/project references and required canonical-document tokens, with fixtures that prove forbidden edges and stale canonical graph text fail.
- PD-004 [AC-004] [FR-004] complete: Preserve protocol message semantics and the server publish/browser route, then run focused graph verification plus the existing conformance route against the final candidate.

## Contract Impact
- PC-001 [PD-001] architecture contract: `docs/codebase-architecture.md` is canonical for project responsibilities and its graph is checked against `SIR.slnx` plus project references.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-004] [PC-001] semanticTest: Run the architecture graph verifier with forbidden-edge and stale-document subject mutations, then build the solution and run the existing production conformance route.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatibility: Project extraction preserves public namespaces and transport payload semantics; `SIR.Client.Web` is replaced by the explicitly named `SIR.Replay.Web` host rather than retaining a combined transitional identity.

## Generated View Impact
- GV-001 [PD-001] workModel: Refresh `readiness/153-separated-project-graph/` after every authored SDD change; stale generated views block the final readiness evidence.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 153-separated-project-graph`.
