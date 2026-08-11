---
schemaVersion: 1
workId: 145-desktop-command-surface
title: Add a customizable desktop-style menu bar and top toolbars
stage: charter
changeTier: tier1
status: chartered
policyPointers:
  - .fsgg/sdd.yml
  - .fsgg/agents.yml
  - .fsgg/policy.yml
  - .fsgg/capabilities.yml
  - .fsgg/tooling.yml
---

# Add a customizable desktop-style menu bar and top toolbars Charter

## Identity
Deliver a persistent desktop command surface for the browser workspace: a stable menu bar and configurable top toolbars driven by the existing shared command registry.

## Principles
- Keep command identity, availability, shortcut presentation, and execution registry-owned.
- Preserve keyboard and assistive-technology access while menus open, close, and adapt to the active mode.
- Persist only user customization, with a safe documented default and a reset route.

## Scope Boundaries
- In: menu groups, top-toolbar customization, local persistence/reset, compact overflow, focused client and browser evidence.
- Out: a visual clone of another editor, a second command model, server-side preference synchronization, or unrelated canvas redesign.

## Policy Pointers
- Honors constitution principles I, V, VI, and VIII: specification first, MVU boundaries, executable evidence, and accessible safe failure.
- Reuses the #143 registry contract and the repository browser-test route.

## Lifecycle Notes
- Tier 1 browser UI and persisted-preference surface. The interactive workspace has no new typed performance target; implementation must retain the existing stable working-surface posture without inventing a timing threshold.
