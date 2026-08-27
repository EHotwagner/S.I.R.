---
schemaVersion: 1
workId: 366-main-ci-routing
title: Route main-push CI by relevant changes
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/366-main-ci-routing/spec.md
publicOrToolFacingImpact: true
---

# Route main-push CI by relevant changes Clarifications

## Source Specification
- work/366-main-ci-routing/spec.md

## Clarification Questions
- **CQ-001** (AMB-001): Should focused main qualification duplicate the existing routed PR DAG?
- **CQ-002** (AMB-002): How does one stable protected verdict express different required inputs by event?
- **CQ-003** (AMB-003): How does focused documentation qualification hand an exact site to Pages without rebuilding?

## Answers
- CQ-001 → No. Pull requests and ordinary main pushes share the same canonical route, producer derivation, routed DAG, typed gate results, and deterministic join; only exact changed-path discovery differs by event.
- CQ-002 → A versioned protected manifest declares `focused` or `complete` mode and the exact required typed receipts. The protected join derives expectations from the manifest and exact source identity.
- CQ-003 → The routed documentation job packages its already-qualified site and receipts only for a main push whose route selected documentation. Pages consumes the artifact by triggering run id and fails if it is absent or mismatched.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-001] [FR-002]: Extend the existing route/DAG/join across `pull_request` and ordinary `push` events; compute PR paths from base/head and main paths from exact `HEAD^..HEAD`.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-004] [FR-005]: Introduce a versioned protected-run manifest whose mode selects either the focused route verdict or the complete preflight/core receipts; every input remains exact-source and digest-bound.
- **DEC-003** [CQ-003] [AMB:AMB-003] [FR-006] [FR-007]: Stage a qualified-site handoff in the routed documentation job only on main push, bind it to the route and documentation receipts, and let Pages remain deploy-only.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None. AMB-001 through AMB-003 are resolved by DEC-001 through DEC-003.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 366-main-ci-routing`.
