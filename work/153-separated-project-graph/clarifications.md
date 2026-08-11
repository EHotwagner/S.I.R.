---
schemaVersion: 1
workId: 153-separated-project-graph
title: Separated Project Graph
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/153-separated-project-graph/spec.md
publicOrToolFacingImpact: true
---

# Separated Project Graph Clarifications

## Source Specification
- work/153-separated-project-graph/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Does restoring the generated-protocol project require changing the released HTTP/Thoth plus SignalR transport?

## Answers
- CQ-001: No. The board decision restores the ownership boundary, while the published scaffold report confirms HTTP/Thoth plus SignalR is the released replacement for the superseded Fable.Remoting request/response assumption.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-001] [FR-004]: `SIR.Protocol.Generated` will own reproducibly generated or generated-shaped transport records and codecs used by `SIR.Protocol`; the released HTTP/Thoth plus SignalR transport remains unchanged.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 153-separated-project-graph`.
