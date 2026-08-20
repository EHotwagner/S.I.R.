---
schemaVersion: 1
workId: 231-svg-pipeline-measurement
title: Measure the complete Chromium SVG rendering pipeline at scalable workloads
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

# Measure the complete Chromium SVG rendering pipeline at scalable workloads Charter

## Identity
- Establish a versioned, independently runnable production-Chromium measurement baseline for the retained SVG tactical workspace before renderer optimization begins.
- Bind every result to the exact candidate, browser/runtime capabilities, fixture definition, journey, and trace artifact so later scaling work can compare like with like.

## Principles
- Measure the complete production route and separate worker compute/transfer, scene projection/allocation, Elmish and React work, SVG style/layout/paint/compositor activity, interaction latency, frame health, DOM structure, and warm memory.
- Vary global project scale independently from the visible working set; fixtures are regression workloads, never permanent supported-size ceilings.
- Prefer deterministic structural counters for cost shape and use production Chromium for browser-owned timings and memory.
- Classify packed transport, typed buffers, and further allocation work only from observed cost.

## Scope Boundaries
- In: versioned scalable fixtures, idle/playback/pan/zoom/selection/modality/dense-overlay journeys, trace capture and summarization, focused tests, exact-candidate evidence, and the performance-intent update.
- Out: renderer optimization, spatial culling, presentation scheduling, layer caching, SVG batching, Canvas/WebGL/WebGPU migration, and a universal project-size ceiling.
- The harness is opt-in and focused; normal small-PR CI does not execute the production Chromium matrix.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Constitution I requires analyzed specification before implementation; VI requires fail-before/pass-after evidence; VIII requires missing browser capability and malformed artifacts to fail visibly.
- `docs/performance-budget.md` is the producer-owned typed performance intent; `docs/scalable-svg-tactical-viewer.md` defines the measurement-first architecture and production route.
- Governance files are optional compatibility pointers; protected-host evidence remains separately identified when available.

## Lifecycle Notes
- Tier 1: this work introduces versioned fixture and trace artifact contracts plus an independently runnable command.
- The pnext performance-first gate requires a pre-implementation smoke and exact-candidate Release Chromium evidence.
- Next lifecycle action: `fsgg-sdd specify --work 231-svg-pipeline-measurement`.
