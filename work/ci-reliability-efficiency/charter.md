---
schemaVersion: 1
workId: ci-reliability-efficiency
title: Reliable and Efficient CI Qualification
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

# Reliable and Efficient CI Qualification Charter

## Identity
Design the next CI qualification architecture after #220: restore trustworthy
pre-merge and default-branch closure, shorten time-to-first-actionable-failure,
and reduce runner/setup/transfer waste without deleting or weakening evidence.

## Principles
- Correctness precedes acceleration: a PR verdict may not be green when the same
  candidate predictably fails its protected or Pages qualification.
- Optimize ownership, scheduling, artifact shape, and duplicate execution; never
  reduce production workloads, mutation populations, accessibility assertions,
  product-performance budgets, or fail-closed receipt checks.
- Treat exact candidate, tree, tool, lock, command, input, output, and test-result
  identities as immutable reuse boundaries.
- Measure end-to-end hosted job time and bytes, including action setup, artifact
  upload/download, post steps, and failed-run diagnostics—not only script bodies.
- Adopt the producer `.github` CI optimization pattern only where S.I.R. has the
  same proven subject: conservative classifiers default to running evidence,
  stable contexts remain visible, and omission reasons are machine-readable.
- Every optimization is independently reversible and must carry a subject
  mutation or inversion that fails when its evidence is incorrectly omitted.

## Scope Boundaries
- In: `.github/workflows/ci.yml`, `.github/workflows/pages.yml`, CI routing and
  qualification scripts, artifact/receipt schemas, deterministic join logic,
  exact-head timing fixtures, CI documentation, and focused workflow tests.
- In: staged design for protected/scheduled qualification decomposition, review
  artifact freshness, Linux-only/deduplicated prepared payloads, setup/restore
  ownership, end-to-end telemetry, Pages handoff, action runtime upgrades, and
  conservative conditional self-tests.
- Out: gameplay, rendering, simulation, or documentation-content behavior;
  lower product-performance thresholds; smaller browser/scenario/mutation
  workloads; cache-as-authority; secrets or permission expansion; and changes to
  the S.I.R. board or producer-owned `.github` workflows.
- Out for the first implementation milestone: unproven cross-workflow compiled
  artifact reuse, custom runner images, self-hosted runners, or new paid runner
  classes. Those require measured transfer/trust experiments before adoption.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Constitution II, VI, and VIII require versioned contracts, real fail-before /
  pass-after evidence, and actionable fail-closed diagnostics.
- `work/220-bounded-pr-ci/**` and `docs/ci-qualification.md` are the landed
  focused-PR baseline; this work extends rather than silently redefines them.
- FS-GG `.github` coherent release 0.71.0 and its
  `work/ci-runtime-optimization/**` design are producer references, not files to
  import: S.I.R. is a registered non-participant and adopts only applicable
  fail-closed design patterns.

## Lifecycle Notes
- Tier 1: required-check topology, route/receipt schemas, workflow commands,
  timing authority, and delivery behavior are tool-facing contracts.
- Design baseline: successful cross-cutting PR run 32523274261; failing protected
  runs 32525850376 and 32548223694; failing Pages run 32525850336; upstream
  `.github` CI optimization merge bcadfdd1.
- This turn authors through `analyze`; implementation, evidence, verify, and ship
  follow only after implementation-ready design.
- Next lifecycle action: `fsgg-sdd specify --work ci-reliability-efficiency`.
