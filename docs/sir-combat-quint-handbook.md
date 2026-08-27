---
title: S.I.R. Combat in Quint: From Design Decisions to Executable Models
category: Battlefield Systems
categoryindex: 4
index: 48
description: A navigable handbook for learning Quint through the bounded S.I.R. physical-combat rule corpus.
date: 2026-08-27
status: in-development
document-type: handbook
---

<a id="handbook-top"></a>
# S.I.R. Combat in Quint: From Design Decisions to Executable Models

This first-edition skeleton fixes the handbook's navigation and definition-address contract. Tutorial substance is intentionally reserved for milestones M2-M7; pending entries and chapter notices are honest placeholders, not executable claims.

<a id="table-of-contents"></a>
## Table of contents

- [Reading paths](#reading-paths)
- [Part I: Orientation](#part-i)
  - [1. What this handbook is](#chapter-01-what-this-handbook-is)
  - [2. How to use the three reading paths](#chapter-02-how-to-use-the-three-reading-paths)
  - [3. Sources of authority and their precedence](#chapter-03-sources-of-authority-and-their-precedence)
  - [4. Toolchain setup and a first successful Quint run](#chapter-04-toolchain-setup-and-a-first-successful-quint-run)
  - [5. The complete attack-resolution pipeline at a glance](#chapter-05-the-complete-attack-resolution-pipeline-at-a-gla)
- [Part II: S.I.R. combat domain](#part-ii)
  - [6. Physical-combat design boundary](#chapter-06-physical-combat-design-boundary)
  - [7. The sixteen-rule catalogue](#chapter-07-the-sixteen-rule-catalogue)
  - [8. Rule dependency and explanation-order maps](#chapter-08-rule-dependency-and-explanation-order-maps)
  - [9. Combat state, inputs, observations, and units](#chapter-09-combat-state-inputs-observations-and-units)
  - [10. What the bounded Q4 model includes and excludes](#chapter-10-what-the-bounded-q4-model-includes-and-excludes)
- [Part III: Decisions to model](#part-iii)
  - [11. Extracting entities, operations, assumptions, and properties from design text](#chapter-11-extracting-entities-operations-assumptions-and-p)
  - [12. Mapping rule kinds to Quint constructs](#chapter-12-mapping-rule-kinds-to-quint-constructs)
  - [13. Choosing state shape and action granularity](#chapter-13-choosing-state-shape-and-action-granularity)
  - [14. Fixed-point Q4 arithmetic and rounding](#chapter-14-fixed-point-q4-arithmetic-and-rounding)
  - [15. The external line-of-sight contract boundary](#chapter-15-the-external-line-of-sight-contract-boundary)
  - [16. Atomic aggregate consequences versus focused pure helpers](#chapter-16-atomic-aggregate-consequences-versus-focused-pur)
- [Part IV: Quint foundations through combat](#part-iv)
  - [17. Modules, types, variants, records, sets, and lists](#chapter-17-modules-types-variants-records-sets-and-lists)
  - [18. Constants, model data, and the rule catalogue](#chapter-18-constants-model-data-and-the-rule-catalogue)
  - [19. Pure functions and deterministic damage calculations](#chapter-19-pure-functions-and-deterministic-damage-calculat)
  - [20. Variables, initialization, and cohesive combat state](#chapter-20-variables-initialization-and-cohesive-combat-sta)
  - [21. Guards, actions, primed assignments, and disabled transitions](#chapter-21-guards-actions-primed-assignments-and-disabled-t)
  - [22. Nondeterministic steps and possible combat histories](#chapter-22-nondeterministic-steps-and-possible-combat-histo)
  - [23. Runs, witnesses, invariants, simulations, and bounded verification](#chapter-23-runs-witnesses-invariants-simulations-and-bounde)
- [Part V: Guided walkthroughs](#part-v)
  - [24. Representative rifle damage: 25 x 1.0 x 0.8 = 20](#chapter-24-representative-rifle-damage-25-x-1-0-x-0-8-20)
  - [25. A miss causes neither damage nor suppression](#chapter-25-a-miss-causes-neither-damage-nor-suppression)
  - [26. Wound thresholds at damage 24, 25, and 50](#chapter-26-wound-thresholds-at-damage-24-25-and-50)
  - [27. Health reaching zero and incapacitation](#chapter-27-health-reaching-zero-and-incapacitation)
  - [28. Cover impact, destruction, permeability, and the current collision](#chapter-28-cover-impact-destruction-permeability-and-the-cu)
  - [29. Suppression eligibility and five-point recovery](#chapter-29-suppression-eligibility-and-five-point-recovery)
  - [30. Faction-neutral collateral consequences](#chapter-30-faction-neutral-collateral-consequences)
  - [31. Registered external line-of-sight behavior](#chapter-31-registered-external-line-of-sight-behavior)
- [Part VI: Formal reasoning in practice](#part-vi)
  - [32. Choosing an example, witness, or invariant](#chapter-32-choosing-an-example-witness-or-invariant)
  - [33. Reading an execution trace](#chapter-33-reading-an-execution-trace)
  - [34. Reading and minimizing a counterexample](#chapter-34-reading-and-minimizing-a-counterexample)
  - [35. Mutation laboratory](#chapter-35-mutation-laboratory)
  - [36. Dead actions, accidental stuttering, and terminal states](#chapter-36-dead-actions-accidental-stuttering-and-terminal-)
  - [37. What sampled runs establish and what exhaustive checks add](#chapter-37-what-sampled-runs-establish-and-what-exhaustive-)
- [Part VII: Production correspondence](#part-vii)
  - [38. Mapping Quint records and operations to S.I.R. runtime subjects](#chapter-38-mapping-quint-records-and-operations-to-s-i-r-ru)
  - [39. Literate authority and deterministic .qnt extraction](#chapter-39-literate-authority-and-deterministic-qnt-extract)
  - [40. Exact and sampled ITF replay](#chapter-40-exact-and-sampled-itf-replay)
  - [41. First-divergence reporting](#chapter-41-first-divergence-reporting)
  - [42. Observed-red controls and restored-green evidence](#chapter-42-observed-red-controls-and-restored-green-evidenc)
  - [43. Safely changing a combat rule](#chapter-43-safely-changing-a-combat-rule)
- [Part VIII: Reference](#part-viii)
  - [44. Complete rule reference](#chapter-44-complete-rule-reference)
  - [45. Quint declaration reference](#chapter-45-quint-declaration-reference)
  - [46. Traceability matrix](#chapter-46-traceability-matrix)
  - [47. Command reference](#chapter-47-command-reference)
  - [48. Known limits and future experiments](#chapter-48-known-limits-and-future-experiments)
  - [49. Exercises and solutions](#chapter-49-exercises-and-solutions)
  - [50. Alphabetical definition index](#chapter-50-alphabetical-definition-index)

<a id="reading-paths"></a>
## Reading paths

<a id="reading-path-learn-quint"></a>
### Learn Quint

[Orientation](#chapter-01-what-this-handbook-is) → [representative attack](#chapter-24-representative-rifle-damage-25-x-1-0-x-0-8-20) → [foundations](#part-iv) → [guided walkthroughs](#part-v) → [formal reasoning](#part-vi).

<a id="reading-path-understand-combat"></a>
### Understand combat

[Combat overview](#chapter-06-physical-combat-design-boundary) → [rule catalogue](#chapter-07-the-sixteen-rule-catalogue) → [focused mechanics](#part-v) → [rule reference](#chapter-44-complete-rule-reference).

<a id="reading-path-review-traceability"></a>
### Review traceability

[Source hierarchy](#chapter-03-sources-of-authority-and-their-precedence) → [decision translation](#part-iii) → [traceability matrix](#chapter-46-traceability-matrix) → [runtime evidence](#part-vii) → [known limits](#chapter-48-known-limits-and-future-experiments).

<a id="part-i"></a>
## Part I: Orientation

<a id="chapter-01-what-this-handbook-is"></a>
### 1. What this handbook is

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-02-how-to-use-the-three-reading-paths"></a>
### 2. How to use the three reading paths

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-03-sources-of-authority-and-their-precedence"></a>
### 3. Sources of authority and their precedence

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-04-toolchain-setup-and-a-first-successful-quint-run"></a>
### 4. Toolchain setup and a first successful Quint run

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-05-the-complete-attack-resolution-pipeline-at-a-gla"></a>
### 5. The complete attack-resolution pipeline at a glance

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="part-ii"></a>
## Part II: S.I.R. combat domain

<a id="chapter-06-physical-combat-design-boundary"></a>
### 6. Physical-combat design boundary

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-07-the-sixteen-rule-catalogue"></a>
### 7. The sixteen-rule catalogue

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-08-rule-dependency-and-explanation-order-maps"></a>
### 8. Rule dependency and explanation-order maps

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-09-combat-state-inputs-observations-and-units"></a>
### 9. Combat state, inputs, observations, and units

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-10-what-the-bounded-q4-model-includes-and-excludes"></a>
### 10. What the bounded Q4 model includes and excludes

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="part-iii"></a>
## Part III: Decisions to model

<a id="chapter-11-extracting-entities-operations-assumptions-and-p"></a>
### 11. Extracting entities, operations, assumptions, and properties from design text

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-12-mapping-rule-kinds-to-quint-constructs"></a>
### 12. Mapping rule kinds to Quint constructs

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-13-choosing-state-shape-and-action-granularity"></a>
### 13. Choosing state shape and action granularity

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-14-fixed-point-q4-arithmetic-and-rounding"></a>
### 14. Fixed-point Q4 arithmetic and rounding

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-15-the-external-line-of-sight-contract-boundary"></a>
### 15. The external line-of-sight contract boundary

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-16-atomic-aggregate-consequences-versus-focused-pur"></a>
### 16. Atomic aggregate consequences versus focused pure helpers

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="part-iv"></a>
## Part IV: Quint foundations through combat

<a id="chapter-17-modules-types-variants-records-sets-and-lists"></a>
### 17. Modules, types, variants, records, sets, and lists

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-18-constants-model-data-and-the-rule-catalogue"></a>
### 18. Constants, model data, and the rule catalogue

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-19-pure-functions-and-deterministic-damage-calculat"></a>
### 19. Pure functions and deterministic damage calculations

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-20-variables-initialization-and-cohesive-combat-sta"></a>
### 20. Variables, initialization, and cohesive combat state

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-21-guards-actions-primed-assignments-and-disabled-t"></a>
### 21. Guards, actions, primed assignments, and disabled transitions

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-22-nondeterministic-steps-and-possible-combat-histo"></a>
### 22. Nondeterministic steps and possible combat histories

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-23-runs-witnesses-invariants-simulations-and-bounde"></a>
### 23. Runs, witnesses, invariants, simulations, and bounded verification

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="part-v"></a>
## Part V: Guided walkthroughs

<a id="chapter-24-representative-rifle-damage-25-x-1-0-x-0-8-20"></a>
### 24. Representative rifle damage: 25 x 1.0 x 0.8 = 20

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-25-a-miss-causes-neither-damage-nor-suppression"></a>
### 25. A miss causes neither damage nor suppression

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-26-wound-thresholds-at-damage-24-25-and-50"></a>
### 26. Wound thresholds at damage 24, 25, and 50

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-27-health-reaching-zero-and-incapacitation"></a>
### 27. Health reaching zero and incapacitation

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-28-cover-impact-destruction-permeability-and-the-cu"></a>
### 28. Cover impact, destruction, permeability, and the current collision

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-29-suppression-eligibility-and-five-point-recovery"></a>
### 29. Suppression eligibility and five-point recovery

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-30-faction-neutral-collateral-consequences"></a>
### 30. Faction-neutral collateral consequences

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-31-registered-external-line-of-sight-behavior"></a>
### 31. Registered external line-of-sight behavior

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="part-vi"></a>
## Part VI: Formal reasoning in practice

<a id="chapter-32-choosing-an-example-witness-or-invariant"></a>
### 32. Choosing an example, witness, or invariant

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-33-reading-an-execution-trace"></a>
### 33. Reading an execution trace

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-34-reading-and-minimizing-a-counterexample"></a>
### 34. Reading and minimizing a counterexample

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-35-mutation-laboratory"></a>
### 35. Mutation laboratory

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-36-dead-actions-accidental-stuttering-and-terminal-"></a>
### 36. Dead actions, accidental stuttering, and terminal states

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-37-what-sampled-runs-establish-and-what-exhaustive-"></a>
### 37. What sampled runs establish and what exhaustive checks add

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="part-vii"></a>
## Part VII: Production correspondence

<a id="chapter-38-mapping-quint-records-and-operations-to-s-i-r-ru"></a>
### 38. Mapping Quint records and operations to S.I.R. runtime subjects

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-39-literate-authority-and-deterministic-qnt-extract"></a>
### 39. Literate authority and deterministic .qnt extraction

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-40-exact-and-sampled-itf-replay"></a>
### 40. Exact and sampled ITF replay

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-41-first-divergence-reporting"></a>
### 41. First-divergence reporting

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-42-observed-red-controls-and-restored-green-evidenc"></a>
### 42. Observed-red controls and restored-green evidence

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-43-safely-changing-a-combat-rule"></a>
### 43. Safely changing a combat rule

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="part-viii"></a>
## Part VIII: Reference

<a id="chapter-44-complete-rule-reference"></a>
### 44. Complete rule reference

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-45-quint-declaration-reference"></a>
### 45. Quint declaration reference

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-46-traceability-matrix"></a>
### 46. Traceability matrix

This full-shaped matrix reserves every mandatory obligation. “Pending” means the mapping or evidence belongs to a later milestone; it is not a claim of coverage.

| Source decision | Stable rule | Quint declaration | [Scenario/property](#def-property) | Runtime subject | Evidence | Coverage note |
|---|---|---|---|---|---|---|
| S.I.R. combat registry | [CONTENT-WEAPON-RIFLE-001](#rule-content-weapon-rifle-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [CONTENT-BODY-HUMAN-001](#rule-content-body-human-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-ENGAGEMENT-001](#rule-combat-engagement-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-TRACE-002](#rule-combat-trace-002) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-ARMOR-004](#rule-combat-armor-004) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-DAMAGE-001](#rule-combat-damage-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-COLLISION-001](#rule-combat-collision-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-COVER-003](#rule-combat-cover-003) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-PENETRATION-001](#rule-combat-penetration-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-HEALTH-001](#rule-combat-health-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-WOUND-001](#rule-combat-wound-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-SUPPRESSION-001](#rule-combat-suppression-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-SUPPRESSION-RECOVERY-001](#rule-combat-suppression-recovery-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-COLLATERAL-001](#rule-combat-collateral-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-COVER-DESTRUCTION-001](#rule-combat-cover-destruction-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| S.I.R. combat registry | [COMBAT-ATTACK-RESOLUTION-001](#rule-combat-attack-resolution-001) | Pending | Pending | Pending | Pending | M3 rule mapping; M5 runtime evidence |
| Q4 DEC-001 | — | Pending | Pending | Pending | Pending | M2-M5 decision mapping |
| Q4 DEC-002 | — | Pending | Pending | Pending | Pending | M2-M5 decision mapping |
| Q4 DEC-003 | — | Pending | Pending | Pending | Pending | M2-M5 decision mapping |
| Q4 DEC-004 | — | Pending | Pending | Pending | Pending | M2-M5 decision mapping |
| Q4 DEC-005 | — | Pending | Pending | Pending | Pending | M2-M5 decision mapping |
| Q4 DEC-006 | — | Pending | Pending | Pending | Pending | M2-M5 decision mapping |
| Q4 DEC-007 | — | Pending | Pending | Pending | Pending | M2-M5 decision mapping |
| [Representative damage 20](#qnt-representative-attack) | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| [Wound boundary 24](#stat-wound-threshold) | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| [Wound boundary 25](#stat-wound-threshold) | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| [Wound boundary 50](#stat-wound-threshold) | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| [Zero-health incapacitation](#concept-incapacitation) | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| [Suppression eligibility](#concept-suppression-eligibility) | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| [Five-point suppression recovery](#concept-suppression-recovery) | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| Cover destruction | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| Destroyed-cover permeability | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| Current-collision blocking | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| Faction-neutral collateral | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| Valid trace ratios | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| External line-of-sight boundary | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| Catalogue size and identity | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| Catalogue dependencies | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| Explanation order | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| [Exact runtime replay correspondence](#def-correspondence) | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |
| [Sampled runtime replay correspondence](#def-correspondence) | — | Pending | Pending | Pending | Pending | Reserved mandatory coverage row |

<a id="chapter-47-command-reference"></a>
### 47. Command reference

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-48-known-limits-and-future-experiments"></a>
### 48. Known limits and future experiments

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-49-exercises-and-solutions"></a>
### 49. Exercises and solutions

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-50-alphabetical-definition-index"></a>
### 50. Alphabetical definition index

The seed index is complete for the M0 inventory. Later milestones replace placeholder explanations with domain-aware definitions without changing these anchors.

| Canonical term | Kind | Definition | Declared at | Related terms | Runtime correspondence |
|---|---|---|---|---|---|
| <a id="qnt-absolute"></a>absolute | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-action"></a>action | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-add-fixed"></a>addFixed | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-aggregate-attack-resolution"></a>aggregate attack resolution | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-algorithm-entry"></a>AlgorithmEntry | type | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-allied-attack"></a>alliedAttack | value | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-armor-retention"></a>armor retention | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-attack-input"></a>AttackInput | type | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-base-damage"></a>base damage | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-bounded-verification"></a>bounded verification | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-bounded100"></a>bounded100 | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="property-bounded-combat-state"></a>boundedCombatState | property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="catalogue-property-bounded-combat-state"></a>BoundedCombatState | catalogue property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-cells"></a>cells | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-claim-boundary"></a>claim boundary | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-collateral-consequence"></a>collateral consequence | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="run-collateral-outcome-ignores-faction"></a>collateralOutcomeIgnoresFaction | run | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-combat"></a>combat | variable | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="rule-combat-armor-004"></a>COMBAT-ARMOR-004 | rule | Stable S.I.R. combat formula identity. | Pending | Pending | Pending |
| <a id="rule-combat-attack-resolution-001"></a>COMBAT-ATTACK-RESOLUTION-001 | rule | Stable S.I.R. combat transition identity. | Pending | Pending | Pending |
| <a id="rule-combat-collateral-001"></a>COMBAT-COLLATERAL-001 | rule | Stable S.I.R. combat transition identity. | Pending | Pending | Pending |
| <a id="rule-combat-collision-001"></a>COMBAT-COLLISION-001 | rule | Stable S.I.R. combat transition identity. | Pending | Pending | Pending |
| <a id="rule-combat-cover-003"></a>COMBAT-COVER-003 | rule | Stable S.I.R. combat transition identity. | Pending | Pending | Pending |
| <a id="rule-combat-cover-destruction-001"></a>COMBAT-COVER-DESTRUCTION-001 | rule | Stable S.I.R. combat transition identity. | Pending | Pending | Pending |
| <a id="rule-combat-damage-001"></a>COMBAT-DAMAGE-001 | rule | Stable S.I.R. combat formula identity. | Pending | Pending | Pending |
| <a id="rule-combat-engagement-001"></a>COMBAT-ENGAGEMENT-001 | rule | Stable S.I.R. combat formula identity. | Pending | Pending | Pending |
| <a id="rule-combat-health-001"></a>COMBAT-HEALTH-001 | rule | Stable S.I.R. combat transition identity. | Pending | Pending | Pending |
| <a id="rule-combat-penetration-001"></a>COMBAT-PENETRATION-001 | rule | Stable S.I.R. combat transition identity. | Pending | Pending | Pending |
| <a id="rule-combat-suppression-001"></a>COMBAT-SUPPRESSION-001 | rule | Stable S.I.R. combat transition identity. | Pending | Pending | Pending |
| <a id="rule-combat-suppression-recovery-001"></a>COMBAT-SUPPRESSION-RECOVERY-001 | rule | Stable S.I.R. combat transition identity. | Pending | Pending | Pending |
| <a id="rule-combat-trace-002"></a>COMBAT-TRACE-002 | rule | Stable S.I.R. combat algorithm identity. | Pending | Pending | Pending |
| <a id="rule-combat-wound-001"></a>COMBAT-WOUND-001 | rule | Stable S.I.R. combat transition identity. | Pending | Pending | Pending |
| <a id="qnt-combat-state"></a>CombatState | type | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-consequence-explanation-order"></a>consequenceExplanationOrder | value | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-consequence-observation"></a>consequenceObservation | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-constant"></a>constant | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="rule-content-body-human-001"></a>CONTENT-BODY-HUMAN-001 | rule | Stable S.I.R. combat fact identity. | Pending | Pending | Pending |
| <a id="rule-content-weapon-rifle-001"></a>CONTENT-WEAPON-RIFLE-001 | rule | Stable S.I.R. combat fact identity. | Pending | Pending | Pending |
| <a id="def-correspondence"></a>correspondence | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-counterexample"></a>counterexample | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-cover-blocking"></a>cover blocking | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-cover-damage"></a>cover damage | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-cover-integrity"></a>cover integrity | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-cover-damage"></a>coverDamage | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-cover-observation"></a>coverObservation | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-current-collision-consumption"></a>current-collision consumption | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-damage"></a>damage | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-damage-points"></a>damage points | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-damage-for-attack"></a>damageForAttack | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-destroyed-cover"></a>destroyed cover | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="property-destroyed-cover-is-permeable"></a>destroyedCoverIsPermeable | property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="catalogue-property-destroyed-cover-is-permeable"></a>DestroyedCoverIsPermeable | catalogue property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="run-destroying-cover-consumes-current-collision"></a>destroyingCoverConsumesCurrentCollision | run | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-divide-rounded-away-from-zero"></a>divideRoundedAwayFromZero | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-event-identity"></a>event identity | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-execution-trace"></a>execution trace | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-exhaustive-check"></a>exhaustive check | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-expected-damage"></a>expected damage | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-expected-damage-raw"></a>expectedDamageRaw | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-explanation-order"></a>explanation order | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-external-algorithm-contract"></a>external algorithm contract | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-faction-neutral-consequence"></a>faction-neutral consequence | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="property-faction-neutral-collateral"></a>factionNeutralCollateral | property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="catalogue-property-faction-neutral-collateral"></a>FactionNeutralCollateral | catalogue property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-first-collision"></a>first collision | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-first-divergence"></a>first divergence | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-fixed-point-ratio"></a>fixed-point ratio | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-from-ratio"></a>fromRatio | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-full-damage-attack"></a>fullDamageAttack | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-generated-projection"></a>generated projection | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-guard"></a>guard | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-health"></a>health | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-hit-points"></a>hit points | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-hp"></a>HP | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-human-armor-retention-raw"></a>humanArmorRetentionRaw | value | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-import"></a>import | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-incapacitation"></a>incapacitation | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="property-incapacity-matches-health"></a>incapacityMatchesHealth | property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="catalogue-property-incapacity-matches-health"></a>IncapacityMatchesHealth | catalogue property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-init"></a>init | action | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-initial-combat"></a>initialCombat | value | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-initialization"></a>initialization | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-int32-max"></a>INT32_MAX | constant | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-int32-min"></a>INT32_MIN | constant | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-integrity-points"></a>integrity points | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-invariant"></a>invariant | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-itf-trace"></a>ITF trace | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-last"></a>last | variable | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-list"></a>list | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-major-wound"></a>MajorWound | variant | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-maximum"></a>maximum | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-minimum"></a>minimum | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-minor-wound"></a>MinorWound | variant | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-missed-attack"></a>missedAttack | value | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-module"></a>module | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-multiply-fixed"></a>multiplyFixed | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-mutation"></a>mutation | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-next-consequences"></a>nextConsequences | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-next-cover-impact"></a>nextCoverImpact | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-next-recovery"></a>nextRecovery | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-nondeterminism"></a>nondeterminism | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-no-wound"></a>NoWound | variant | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-observation"></a>Observation | type | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-observed-red-control"></a>observed-red control | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-penetration"></a>penetration | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-physical-shot-trace"></a>physical shot trace | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-prediction"></a>prediction | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-preparation-time"></a>preparation time | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-preparation-raw"></a>preparationRaw | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-primed-assignment"></a>primed assignment | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-projectile-contact"></a>projectile contact | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-property"></a>property | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-property-catalogue"></a>propertyCatalogue | value | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-property-entry"></a>PropertyEntry | type | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-pure-function"></a>pure function | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-pure-value"></a>pure value | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-q4-raw-integer"></a>Q4 raw integer | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-range-cells"></a>range cells | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-range-slope-raw"></a>rangeSlopeRaw | value | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-reachable-state"></a>reachable state | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-record"></a>record | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-recovered-suppression"></a>recoveredSuppression | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-recovery-observation"></a>recoveryObservation | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-registered-line-of-sight-implementation"></a>registered line-of-sight implementation | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-representative-attack"></a>representativeAttack | value | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="run-representative-damage-is-twenty"></a>representativeDamageIsTwenty | run | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-resolve-consequences"></a>resolveConsequences | action | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-resolve-cover-impact"></a>resolveCoverImpact | action | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-resolve-recovery"></a>resolveRecovery | action | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-restored-green"></a>restored green | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-retained-effect"></a>retained effect | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-retained-effect"></a>retainedEffect | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-rifle-damage-raw"></a>rifleDamageRaw | value | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-round-half-away-from-zero"></a>round-half-away-from-zero | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-rounded-damage"></a>roundedDamage | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-rule-catalogue"></a>ruleCatalogue | value | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-rule-entry"></a>RuleEntry | type | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-run"></a>run | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-safety-property"></a>safety property | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-sampled-run"></a>sampled run | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-samples"></a>samples | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-saturate-int32"></a>saturateInt32 | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-scale"></a>SCALE | constant | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-scale-10-000"></a>scale 10,000 | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-seconds"></a>seconds | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-set"></a>set | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-signed-32-bit-saturation"></a>signed 32-bit saturation | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-sir-combat"></a>SirCombat | module | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-sir-combat-tests"></a>SirCombatTests | module | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="property-sixteen-rules-declared"></a>sixteenRulesDeclared | property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="catalogue-property-sixteen-rules-declared"></a>SixteenRulesDeclared | catalogue property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-source-digest"></a>source digest | evidence | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-state-transition"></a>state transition | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-state-variable"></a>state variable | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-step"></a>step | action | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-stuttering"></a>stuttering | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-suppression"></a>suppression | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-suppression-delta"></a>suppression delta | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-suppression-eligibility"></a>suppression eligibility | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="unit-suppression-points"></a>suppression points | unit | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-suppression-recovery"></a>suppression recovery | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-suppression-for-damage"></a>suppressionForDamage | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="run-suppression-needs-positive-damage-and-recovers-five"></a>suppressionNeedsPositiveDamageAndRecoversFive | run | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="property-suppression-requires-damage"></a>suppressionRequiresDamage | property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="catalogue-property-suppression-requires-damage"></a>SuppressionRequiresDamage | catalogue property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-target-footprint"></a>target footprint | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-terminal-state"></a>terminal state | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-total-samples"></a>total samples | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-trace-probability"></a>trace probability | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-trace-algorithm"></a>traceAlgorithm | value | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-trace-raw"></a>traceRaw | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-type"></a>type | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-valid-attack"></a>validAttack | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-valid-trace"></a>validTrace | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="property-valid-trace-observation"></a>validTraceObservation | property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="catalogue-property-valid-trace-observation"></a>ValidTraceObservation | catalogue property | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-variant"></a>variant | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-visible-samples"></a>visible samples | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="def-witness"></a>witness | keyword | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="concept-wound"></a>wound | concept | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-wound"></a>Wound | type | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="stat-wound-threshold"></a>wound threshold | stat | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="qnt-wound-for-damage"></a>woundForDamage | function | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="run-wound-thresholds-are-exact"></a>woundThresholdsAreExact | run | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |
| <a id="run-zero-health-means-incapacitated"></a>zeroHealthMeansIncapacitated | run | Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. | Pending | Pending | Pending |

[Back to the table of contents](#table-of-contents)
