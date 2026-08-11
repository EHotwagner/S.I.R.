---
schemaVersion: 1
workId: 142-stable-working-surface
title: Preserve a stable working surface across modes
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

# Preserve a stable working surface across modes Charter

## Identity
One mounted tactical work surface must remain useful while the user changes
Editor, Plan, Simulate, or Review modality. The work preserves spatial
continuity rather than treating a temporarily unavailable derived projection as
an empty battlefield.

## Principles
- Preserve the existing shared SVG and camera owner; mode helpers may vary.
- Prefer the last authoritative editor scene as an explicit fallback over a
  synthetic blank scene.
- Browser assertions exercise the production mode-switch route and DOM values.

## Scope Boundaries
- In: shared scene projection selection, mode transition continuity, focused
  client/browser regression coverage.
- Out: simulation generation, replay import semantics, persistence changes, and
  unrelated panel redesign.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- This is Tier 1 interactive behavior: performance intent is absent, so no
  timing target is invented; production browser coverage remains required.
