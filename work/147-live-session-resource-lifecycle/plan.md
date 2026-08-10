---
schemaVersion: 1
workId: 147-live-session-resource-lifecycle
title: Live Session Resource Lifecycle
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/147-live-session-resource-lifecycle/spec.md
sourceClarifications: work/147-live-session-resource-lifecycle/clarifications.md
sourceChecklist: work/147-live-session-resource-lifecycle/checklist.md
publicOrToolFacingImpact: true
---

# Live Session Resource Lifecycle Plan

Prose status: planned

## Source Snapshot
- spec: work/147-live-session-resource-lifecycle/spec.md sha256:c1b7466814c97f9a365622e2184aedc97b585a53e05944d1c17497675c38816e schemaVersion:1
- clarifications: work/147-live-session-resource-lifecycle/clarifications.md sha256:9d2718429f78478705f14abdce10c3fa2b91b3b55234728a12e3518def769e98 schemaVersion:1
- checklist: work/147-live-session-resource-lifecycle/checklist.md sha256:88c2535eda3f35c18c7e27caef1360c1fbe31515f42398923e1134cfad54b26c schemaVersion:1

## Plan Scope
- Work item 147-live-session-resource-lifecycle is planned from the current specification, clarification, and checklist facts.
- Requirement count: 1.
- Clarification decision count: 0.
- Checklist result count: 1.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Keep the external bootstrap, hub, and reconnect protocol shapes stable while moving session state behind independently locked session records. Enforce request/actor limits at the HTTP edge, use bounded per-principal admission plus a global capacity, and remove records after the disconnect grace and expiry transition.

## Contract Impact
- PC-001 [PD-001] protocol compatibility: Existing bootstrap, snapshot, advance, reconnect, and SignalR callers retain their documented success payloads; rejected admission and expired-session paths return stable problem/error values rather than allocating or reviving server state.

## Verification Obligations
- VO-001 [PD-001] [PC-001] regression test: Browser/server integration coverage proves body and actor validation, admission rejection, disconnected-session expiry/cleanup, and simultaneous mutation of two distinct sessions. Each new assertion is inverted once to demonstrate a red gate.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] fail-closed: This is in-memory single-process session state. Restart and multi-process routing remain explicitly unsupported; a request whose session record is absent returns the existing stable authorization/session error and never reconstructs a session.

## Generated View Impact
- GV-001 [PD-001] SDD projection: Refresh the work model and both Codex/Claude guidance after authored lifecycle artifacts change, and retain analysis/verify/ship readiness receipts bound to this work item's current sources.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 147-live-session-resource-lifecycle`.
