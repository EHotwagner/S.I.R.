---
schemaVersion: 1
workId: 150-browser-e2e-workflows
title: User-visible browser E2E coverage for core tactical workflows
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

# User-visible browser E2E coverage for core tactical workflows Charter

## Identity
- Work id: `150-browser-e2e-workflows`
- Lifecycle stage: charter
- Status: chartered

## Principles
- Exercise production Chromium through controls and accessible names a user can reach; do not
  certify a journey through private browser hooks or implementation-only state.
- Preserve the client runtime boundary: browser tests demonstrate existing capabilities and do
  not add tactical behavior solely to make tests convenient.
- Treat unexpected console errors, page errors, and failed network requests as evidence failures.

## Scope Boundaries
- In: visible browser journeys for the existing Editor, Plan, Simulate, Review, and Play modes;
  command surfaces, imports, layout persistence, and the live-authority route.
- Out: new game mechanics, a new browser automation framework, and assertions over private
  `window` hooks or implementation-only data attributes.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- Tier 1 test-architecture change. The implementation will modify only the item-declared browser
  tests, client accessibility/observability seams where required, and test scripts.
