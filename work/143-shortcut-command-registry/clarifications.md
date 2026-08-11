---
schemaVersion: 1
workId: 143-shortcut-command-registry
title: Shortcut Command Registry
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/143-shortcut-command-registry/spec.md
publicOrToolFacingImpact: true
---

# Shortcut Command Registry Clarifications

## Source Specification
- work/143-shortcut-command-registry/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Which existing seam can provide registry-backed shortcut metadata to all rendered command controls?
- CQ-002 [AMB:AMB-002]: How do current gesture text, ARIA values, and binding overrides represent platform conventions and customization?

## Answers
- CQ-001 → `UnifiedTacticalWorkspace.commandRegistry`, `activeTacticalRegistry`, and the existing tactical context help own the shared command identity; the common `button` renderer and tactical controls are the first production surfaces to extend.
- CQ-002 → `effectiveGesture` already resolves overrides; textual gestures use `Ctrl`/`Ctrl/Cmd` and ARIA normalizes Control plus named keys. The implementation must centralize that formatting rather than add another representation.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-001] [FR-002] [FR-003] [FR-005]: Extend the existing unified command registry and its active projection as the sole source for shortcut labels, keyboard dispatch, and production accessibility metadata; cover the shared renderer and representative tactical control routes.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-002] [FR-004]: Keep overrides in `TacticalBindingProfile`, derive every presentation value from `effectiveGesture`, and centralize human and ARIA formatting so custom or unassigned values cannot drift.

## Accepted Deferrals
- None.

## Remaining Ambiguity
- None.

## Lifecycle Notes
- Decisions reuse the existing registry and binding profile rather than create a competing UI-command model.
