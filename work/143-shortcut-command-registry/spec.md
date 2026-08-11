---
schemaVersion: 1
workId: 143-shortcut-command-registry
title: Shortcut Command Registry
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Shortcut Command Registry Specification

Prose status: specified

## User Value
Players can discover and use every UI command through its displayed keyboard shortcut.

## Scope
- SB-001: SIR.Client and SIR.Client.Web command surfaces plus focused client and browser tests; excludes non-actionable readouts and unrelated UI redesign.

## Non-Goals
- SB-002: Do not implement later lifecycle commands or Governance enforcement in this specification.

## User Stories
- US-001 (P1): As a player, I can discover a command's current keyboard shortcut wherever I can invoke it.
- US-002 (P1): As a keyboard user, I can activate the command using the same binding that its UI presents.
- US-003 (P2): As a user who customizes bindings, I see the replacement shortcut without stale labels or metadata.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given an actionable command is rendered in a menu, when its binding exists, then the menu displays the platform-formatted shortcut in the aligned shortcut slot.
- AC-002 [US-001] [FR-002]: Given an actionable command is rendered as a toolbar, compact, icon-only, button, or command-list control, when its binding exists, then its visible or accessible label includes the formatted shortcut without replacing the command name.
- AC-003 [US-002] [FR-003]: Given a command has a displayed shortcut, when the user sends that keyboard combination, then the same registered command action is invoked.
- AC-004 [US-003] [FR-004]: Given a binding is customized or absent, when the affected command is rendered, then every surface updates to the new platform-formatted label or explicitly exposes it as unassigned.
- AC-005 [US-001] [FR-005]: Given the production browser workspace is loaded, when accessibility and browser tests inspect actionable controls, then discoverability metadata and activation are verified through the production route.

## Functional Requirements
- FR-001: The system MUST define each actionable UI command and its canonical or unassigned shortcut in one customizable command registry used by rendering and keyboard dispatch. (covers AC-001)
- FR-002: The system MUST render a platform-formatted shortcut label for every assigned command in menus, toolbar buttons, compact/icon-only controls, buttons, command lists, and relevant accessible names or tooltips. (covers AC-002)
- FR-003: The system MUST dispatch keyboard activation through the registry command identity that produces the displayed shortcut. (covers AC-003)
- FR-004: The system MUST recompute visible and accessible shortcut labels from customized bindings and explicitly identify unassigned commands. (covers AC-004)
- FR-005: The system MUST provide focused client and browser accessibility regression coverage for shortcut discoverability and activation. (covers AC-005)

## Ambiguities
- AMB-001: The exact set of existing browser controls and their shared rendering seam must be established before implementation.
- AMB-002: The product's platform modifier rendering and customization representation must be confirmed from current code rather than introduced as a parallel format.

## Public Or Tool-Facing Impact
- The browser command registry and rendered accessible names are a public UI contract; their F# surface and browser tests change together.

## Lifecycle Notes
- Tier 1 change. Analyze must reach implementationReady before the declared UI and test paths are edited.
