---
schemaVersion: 1
workId: 142-stable-working-surface
title: Stable Working Surface
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Stable Working Surface Specification

Prose status: specified

## User Value
A workspace user retains one continuous battlefield surface while changing Plan, Editor, Simulate, and Review modes.

## Scope
- SB-001: Change only the declared client and browser-test Paths.
- SB-002: Preserve the mounted SVG, work-area bounds, editor camera and valid
  selection while switching modes.

## Non-Goals
- Do not create a simulator handoff or replay solely to make a mode render.
- Do not alter persistence, scenario semantics, or unrelated shell layout.

## User Stories
- US-001 (P1): As a workspace user, I can change tools without losing my visual
  orientation when the target tool has no independent scene yet.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given an editor scene and camera are present, when
  the user switches through Plan, Simulate, and Review before their derived
  inputs are available, then the persistent SVG retains the editor scene bounds,
  camera values, and valid focused unit.
- AC-002 [US-001] [FR-002]: Given a mode has an authoritative derived scene,
  when the user switches to it, then it is displayed without replacing the SVG
  or its working-area layout.
- AC-003 [US-001] [FR-003]: Given a selection cannot exist in the target scene,
  when the mode changes, then the UI removes it predictably instead of showing a
  stale selection.

## Functional Requirements
- FR-001: A mode without an authoritative derived projection must render the editor scene through the mounted shared surface. (covers AC-001)
- FR-002: A mode with an authoritative projection must retain the mounted SVG, work-area bounds, and camera owner. (covers AC-002)
- FR-003: Mode transitions must retain a valid focused unit and clear only a selection that the target scene cannot represent. (covers AC-003)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- The browser-visible `data-scene-*`, camera, and selection attributes form the
  regression observation surface; no public API signature changes.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 142-stable-working-surface`.
