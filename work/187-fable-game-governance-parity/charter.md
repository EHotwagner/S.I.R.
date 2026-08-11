---
schemaVersion: 1
workId: 187-fable-game-governance-parity
title: Establish Fable game-skill and governance parity for the tactical phase
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

# Establish Fable game-skill and governance parity for the tactical phase Charter

## Identity
- Work id: `187-fable-game-governance-parity`
- Lifecycle stage: charter
- Status: chartered

## Principles
- Package-only authority: FS.GG.Game.Core is consumed only as the released pinned package.
- Preserve S.I.R.-owned protocol, replay identity, simulation rules, UI, and knowledge policy.
- Treat LockstepExact classification as the sole authority for shared cross-runtime game logic.
- Keep governed configuration, skills, tests, and generated readiness evidence current together.

## Scope Boundaries
- In: Fable/SVG governance parity, controlled imports, package pins, skill materialization,
  cross-runtime fixtures, performance route, CI/local gates, documentation, and migration from
  provisional work id 178.
- Out: cover, armor, awareness, tactical overlays, and new scenarios.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- Tier 1 governed package/runtime boundary. Analyze must reach implementationReady before source edits;
  evidence, verify, ship, refresh, and agents must be current before review.
