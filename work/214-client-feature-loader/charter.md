---
schemaVersion: 1
workId: 214-client-feature-loader
title: Client Feature Loader Contract
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

# Client Feature Loader Contract Charter

## Identity
Canonize the browser client's feature-loading boundary as a versioned contract so
bootstrap, eager, and deferred reachability remain explicit across Fable, Vite,
browser state, offline/CSP delivery, and production qualification.

## Principles
- The registry is the sole declaration of feature identity and reachability.
- Public loader state and messages remain stable F# values; JavaScript import is an edge effect.
- Chunk identity, graph receipts, and budget results are deterministic and content-addressed.
- Property-name mangling is prohibited as a delivery-growth remedy.
- Focused tests support editing; one aggregate is produced only after source freeze.

## Scope Boundaries
- In: client loader/build configuration, registry and Fable-safe import seam, Rules Explorer/Docs/Tactical Environment migration, production browser controls, delivery mutations, deterministic bundle-graph receipts, and loader documentation.
- Out: server API behavior, gameplay rules, unrelated UI restructuring, CDN deployment, and property-name mangling.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.
- Constitution principles I, II, III, V, VI, VII, and VIII govern this Tier 1 contract.

## Lifecycle Notes
- Issue: `EHotwagner/S.I.R.#214`; delivery route v2 revision 1, digest `2d364fde9cc79e8f6263a5ebce788c44e38fa5140f98b6dcb69ddc3016735c25`.
- The #154 production-delivery SLO is the producer-owned performance baseline: initial route 1,250,000 raw / 320,000 gzip / 280,000 Brotli bytes and deferred chunks require explicit versioned budgets.
- Analyze must report `implementationReady` before declared product paths are edited.
- Next lifecycle action: `fsgg-sdd specify --work 214-client-feature-loader`.
