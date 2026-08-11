---
schemaVersion: 1
workId: 143-shortcut-command-registry
title: Display keyboard shortcuts on every actionable UI command
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

# Display keyboard shortcuts on every actionable UI command Charter

## Identity
A Tier 1 client command-discovery change that establishes one canonical, customizable
shortcut registry for the SIR browser UI and exposes the same bindings wherever an
action can be invoked.

## Principles
- A command's binding, keyboard activation, visible label, tooltip, and accessible
  name derive from the same registry entry.
- Preserve platform-appropriate modifier rendering and disclose unassigned commands.
- Exercise production browser surfaces and accessibility metadata, not only unit models.

## Scope Boundaries
- In: command registry, shortcut rendering, accessible metadata, keyboard activation,
  customization propagation, and focused client/browser tests.
- Out: assigning shortcuts to non-actionable readouts, redesigning unrelated UI, and
  changing server or simulation behavior.

## Policy Pointers
- Honors Constitution I, II, III, VI, VII, and VIII; the declared API and browser
  accessibility behavior require signatures, tests, and generated evidence together.

## Lifecycle Notes
- The governing delivery route is `sdd-required`; complete through analyze before
  editing the issue-declared client and test paths.
