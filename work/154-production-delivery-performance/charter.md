---
schemaVersion: 1
workId: 154-production-delivery-performance
title: Production delivery performance budgets
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

# Production delivery performance budgets Charter

## Identity
Make the production web delivery surface bounded and cache-correct without
changing authoritative simulation, protocol, or replay semantics.

## Principles
- Compression and cache headers are observable HTTP contracts and are verified
  against a published Release artifact.
- Delivery budgets must measure the files and request graph users actually load;
  neither source-size checks nor localhost-only assertions substitute for that.

## Scope Boundaries
In: server static-file delivery, Vite output partitioning, deterministic bundle
budget checks, throttled browser measurements, and deployment ownership docs.
Out: CDN provisioning, game-engine changes, replay format changes, and changing
the deterministic engine selection/integrity manifest format.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
Tier 1 delivery-contract work. The implementation must demonstrate a Release
artifact, HTTP compression/cache behavior, and a route-level first-load versus
mode-activation measurement.
