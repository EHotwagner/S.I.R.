---
schemaVersion: 1
workId: 145-desktop-command-surface
title: Desktop Command Surface
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Desktop Command Surface Specification

Prose status: specified

## User Value
Users can discover, invoke, and personalize stable desktop-style menu and toolbar commands across workspace modes.

## Scope
- SB-001: Browser client menu bar, top toolbars, registry-backed command availability, local customization persistence, and focused client/browser tests; excludes server synchronization and unrelated canvas redesign.

## Non-Goals
- SB-002: No visual clone of VS Code or 3ds Max, server preference synchronization, or parallel command/shortcut model.

## User Stories
- US-001 (P1): As a desktop-oriented user, I can discover commands in stable named menus and invoke them with a mouse or keyboard.
- US-002 (P1): As a user, I can choose which frequent commands appear on the top toolbar and restore the default arrangement.
- US-003 (P1): As a keyboard or assistive-technology user, I can move through menus, hear their meaning and shortcuts, and dismiss them predictably.
- US-004 (P2): As a user in a compact window or another workspace mode, I retain a usable command surface without the work area being replaced.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the browser workspace is open, when a user opens the desktop menu bar, then File, Edit, View, Tools, Simulation, and Help provide registry-backed command entries with visible shortcuts where assigned.
- AC-002 [US-001] [FR-002]: Given a menu is open, when the user uses pointer, arrow, Enter, or Escape input, then focus, invocation, and dismissal behave predictably without leaking commands to the underlying workspace.
- AC-003 [US-002] [FR-003]: Given a user changes the top-toolbar commands or their order, when the page is reloaded, then the arrangement is restored; when reset is selected, the documented default is restored.
- AC-004 [US-003] [FR-004]: Given browser accessibility inspection, when menus and toolbar controls are examined, then they expose menu/menuitem/toolbar semantics, accessible names, and registry-derived shortcut metadata.
- AC-005 [US-004] [FR-005]: Given a mode changes or the viewport is compact, when the command surface is rendered, then common chrome remains stable while unavailable or mode-specific commands adapt and an overflow route remains usable.
- AC-006 [US-001] [FR-006]: Given a menu or toolbar command is activated through a production browser route, when it is available, then it invokes the same registry command action as its shortcut route.

## Functional Requirements
- FR-001: The system MUST render a persistent top menu bar with the six predictable command groups and registry-backed command entries. (covers AC-001)
- FR-002: The system MUST support pointer and keyboard menu navigation, managed focus, Enter activation, and Escape dismissal without dispatching the dismissal key as a workspace command. (covers AC-002)
- FR-003: The system MUST allow users to add, remove, and reorder top-toolbar commands, persist that preference locally, and reset it to the documented default. (covers AC-003)
- FR-004: The system MUST expose accessible menu and toolbar roles, labels, and registry-derived keyboard shortcut metadata. (covers AC-004)
- FR-005: The system MUST retain a stable main surface across modes and present unavailable or mode-specific commands honestly while providing usable compact overflow. (covers AC-005)
- FR-006: The system MUST use the existing shared command registry as the single command identity, availability, shortcut, and execution authority for menus, toolbars, and keyboard dispatch. (covers AC-006)

## Ambiguities
- AMB-001: The precise existing browser component seam that can host the common menu bar and toolbar state must be established from the current client.
- AMB-002: The local storage shape and default toolbar command set must preserve the #143 binding profile and avoid a competing preference contract.
- AMB-003: Compact overflow must be validated through the repository's production browser test route rather than an invented layout target.

## Public Or Tool-Facing Impact
- Browser menu, toolbar, ARIA semantics, and persisted customization are observable UI contracts; their F# surface, client qualification, and real browser tests change together.

## Lifecycle Notes
- Tier 1. Analyze must reach implementationReady before declared client paths are edited. No active typed performance target exists; responsive evidence is structural and browser-observed rather than a fabricated timing budget.
