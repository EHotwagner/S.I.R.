---
schemaVersion: 1
workId: 150-browser-e2e-workflows
title: Browser E2e Workflows
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Browser E2e Workflows Specification

Prose status: specified

## User Value
Tactical creators can validate core project, simulation, import, layout, and live-authority workflows through their visible browser interface.

## Scope
- SB-001: Production Chromium journeys across the existing client modes and browser-test harness; no new tactical product capability.

## Non-Goals
- SB-002: Do not implement later lifecycle commands or Governance enforcement in this specification.

## User Stories
- US-001 (P1): As a tactical creator, I can move from Editor through Plan, Simulate, Review, and
  Play without losing the visible scene, viewport, or selection context.
- US-002 (P1): As a tactical creator, I can drive simulation playback and understand when commands
  are unavailable.
- US-003 (P1): As a keyboard and assistive-technology user, I can discover and operate menus,
  toolbars, shortcuts, and layout controls.
- US-004 (P1): As a tactical creator, I can import supported map, replay, and background files and
  receive an actionable rejection for unsupported input.
- US-005 (P1): As a live participant, I can see authoritative advances and reconnects reflected in
  the battlefield.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given a selected sample in Editor, when the creator moves through Plan,
  Simulate, Review, and Play, then each mode exposes the same scene/viewport/selection context.
- AC-002 [US-002] [FR-002]: Given a loaded simulator, when the creator uses Play, Pause, Step, and
  Reset, then visible playback state advances and resets through the production command surface.
- AC-003 [US-002] [FR-003]: Given empty or invalid prerequisites, when a command cannot run, then it
  is disabled and exposes an actionable unavailable reason.
- AC-004 [US-003] [FR-004]: Given a desktop command surface, when the creator navigates menus and
  toolbars by keyboard or invokes displayed shortcuts, then the visible command action occurs.
- AC-005 [US-003] [FR-005]: Given a customized layout, when the page reloads, resets, narrows, or is
  zoomed to 400 percent, then persistence, reset, and usable responsive controls remain observable.
- AC-006 [US-004] [FR-006]: Given supported and unsupported map, replay, and background files, when
  they are imported, then success is visible and rejection identifies the reason.
- AC-007 [US-005] [FR-007]: Given an authorized live session, when the authoritative session advances
  or reconnects, then the visible battlefield reflects the new live state.
- AC-008 [US-001] [FR-008]: Given every browser journey, when a console, page, or network failure is
  not explicitly expected, then the scenario fails with the captured diagnostic.

## Functional Requirements
- FR-001: Browser scenarios MUST use visible or accessibility-facing controls for Editor → Plan → Simulate → Review transitions with scene, viewport, and selection continuity. (covers AC-001)
- FR-002: Browser scenarios MUST use visible controls for sample selection, simulator load, and Play/Pause/Step/Reset with visible advancement. (covers AC-002)
- FR-003: Browser scenarios MUST demonstrate disabled commands and actionable unavailable reasons for empty or invalid prerequisites. (covers AC-003)
- FR-004: Browser scenarios MUST demonstrate menu and toolbar keyboard navigation plus displayed, working shortcuts. (covers AC-004)
- FR-005: Browser scenarios MUST demonstrate layout customization, reload persistence, reset, narrow viewport, and 400 percent zoom through user-visible outcomes. (covers AC-005)
- FR-006: Browser scenarios MUST demonstrate map, replay, and background import success and rejection paths through visible feedback. (covers AC-006)
- FR-007: Browser scenarios MUST demonstrate authoritative live advance and reconnect through the visible battlefield rather than private hooks or data attributes. (covers AC-007)
- FR-008: Browser test infrastructure MUST fail each journey on unexpected console, page, or network errors, while allowing only explicitly expected failure responses. (covers AC-008)

## Ambiguities
- AMB-001: The exact accessible labels and production-visible evidence for each imported file and
  live update must be established from the current client before scenarios are authored.

## Public Or Tool-Facing Impact
- Browser tests and their scripts are release evidence. Client accessibility labels or status text may
  be changed only where the existing visible surface cannot express the required user outcome.

## Lifecycle Notes
- Scenario evidence is real Chromium evidence, not a structural/happy-dom substitute.
