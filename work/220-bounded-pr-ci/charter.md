---
schemaVersion: 1
workId: 220-bounded-pr-ci
title: Bound PR CI feedback with path-aware receipt reuse
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

# Bounded PR CI Charter

## Identity
Create a permanently bounded, path-aware PR qualification lane that reuses immutable build evidence and runs independent gates concurrently, while retaining the complete protected qualification surface.

## Principles
- Improve scheduling and reuse only; never delete, soften, or infer a passing check.
- Keep every skip explainable and every reuse bound to exact source, configuration, lock, tool, command, revision/tree, and output identities.
- Keep the implementation loop lean: one baseline, focused edit-time tests, one source-frozen aggregate, metadata-only sealing, and one hosted run for the final head.
- Separate runner feedback timing from product performance assertions.

## Scope Boundaries
- In: GitHub Actions topology, path routing, qualification/receipt orchestration, timing schema, fail-closed mutations, tests, lifecycle evidence, feedback, and process documentation.
- Out: production gameplay or visual behavior, reduced evidence obligations, relaxed performance budgets, and replacement of the versioned production-build receipt contract.

## Policy Pointers
- Constitution II requires route/timing/receipt data to be schema-versioned machine contracts; VI requires fail-before/pass-after coverage; VIII requires actionable fail-closed diagnostics.
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`; optional Governance remains the protected-boundary owner rather than being reimplemented here.

## Lifecycle Notes
- Tier 1: this changes required-check topology, commands, generated receipt views, and developer process documentation.
- Issue #220 follows the landed single-pass qualification receipt contract from #215 and starts from main after #192.
- Next lifecycle action: `fsgg-sdd specify --work 220-bounded-pr-ci`.
