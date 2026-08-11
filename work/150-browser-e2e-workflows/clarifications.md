---
schemaVersion: 1
workId: 150-browser-e2e-workflows
title: Browser E2e Workflows
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/150-browser-e2e-workflows/spec.md
publicOrToolFacingImpact: true
---

# Browser E2e Workflows Clarifications

## Source Specification
- work/150-browser-e2e-workflows/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001] answered: Which observables bind the browser tests without private state?

## Answers
- CQ-001: Use accessible names, rendered text, enabled/disabled state, focus, screenshots, and
  visible battlefield/preview changes. Inspect the current client to name those controls; do not use
  private `window` hooks or implementation-only `data-*` attributes as assertions.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] resolved: The Playwright harness records unexpected console, page, and network
  failures per journey, while individual rejection-path requests are explicitly allowlisted.
- DEC-002 [CQ-001] resolved: A scenario may prepare input through Playwright's public file chooser,
  but verifies result and rejection through the rendered UI.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 150-browser-e2e-workflows`.
