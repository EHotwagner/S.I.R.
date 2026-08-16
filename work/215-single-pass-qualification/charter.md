---
schemaVersion: 1
workId: 215-single-pass-qualification
title: Make production qualification single-pass and receipt-driven
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

# Single-Pass Qualification Charter

## Identity
Create an immutable build-output receipt that lets production qualification reuse one verified set of Fable, Rules Explorer, documentation, delivery, and browser artifacts without weakening exact-head evidence.

## Principles
- Preserve all production documentation, delivery, browser, and mutation behavior.
- Fail closed when source, configuration, lockfile, tool, or output identity differs from the receipt.
- Keep the workflow lean: focused tests while editing, one clean aggregate after source freeze, metadata-only feedback finalization without a rebuild, and one hosted final CI run.
- Measure before/after wall time on comparable clean qualification routes.

## Scope Boundaries
- In: build and qualification scripts, package commands/locks, documentation integration, feedback receipt validation, tests, lifecycle evidence, and timing evidence.
- Out: production simulation, client/gameplay behavior, delivery semantics, browser journeys, and documentation content behavior beyond consuming verified outputs.

## Policy Pointers
- Constitution II requires the receipt to be a schema-versioned machine contract; VI requires fail-before/pass-after tests and a self-restoring stale-reuse mutation; VIII requires actionable fail-closed diagnostics.
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`; the issue route requires implementation-ready analysis, verify, and ship.

## Lifecycle Notes
- Tier 1: this changes qualification commands, build-output artifact layout, and the feedback evidence contract without changing runtime product behavior.
- Next lifecycle action: `fsgg-sdd specify --work 215-single-pass-qualification`.
