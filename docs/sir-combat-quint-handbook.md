---
title: Combat in Quint: From Design Decisions to Executable Models
category: Battlefield Systems
categoryindex: 4
index: 48
description: A navigable handbook for learning Quint through the bounded S.I.R. physical-combat rule corpus.
date: 2026-08-27
status: in-development
document-type: handbook
---

<a id="handbook-top"></a>
# Combat in Quint: From Design Decisions to Executable Models

This in-progress first edition combines stable navigation, a complete mechanically enforced definition index, a representative rifle-attack learning spine, and complete coverage of all sixteen stable combat rules. Chapters still marked “Scheduled content” belong to later roadmap milestones and remain honest placeholders, not executable claims.

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

For the representative shot, read the pipeline from left to right:

```text
rifle fact 25
  → valid target footprint
  → preparation at range 3
  → trace 10 visible / 10 total = 1.0
  → armor retains 0.8
  → expected damage 25 × 1.0 × 0.8 = 20
  → round to 20 damage points
  → atomically publish health 80, suppression 12, and one observation
```

The first five arrows calculate a completed consequence. The final arrow is one [aggregate attack resolution](#concept-aggregate-attack-resolution): the production interpreter exposes the completed result, so the model does not invent observable intermediate states between [health](#stat-health), [wound](#concept-wound), or [suppression](#stat-suppression) updates. [Cover impact](#concept-cover-blocking) and [suppression recovery](#concept-suppression-recovery) are separate runtime entry points and therefore separate [actions](#def-action); their focused walkthroughs appear in chapters 28 and 29.

<a id="part-ii"></a>
## Part II: S.I.R. combat domain

<a id="chapter-06-physical-combat-design-boundary"></a>
### 6. Physical-combat design boundary

The bounded model answers a deliberately narrow question: given an attempted physical attack, what
completed combat consequences follow? It includes registry identity, range preparation, sampled shot
trace, [armor retention](#stat-armor-retention), whole-point [damage](#stat-damage), [health](#stat-health), wounds, [incapacitation](#concept-incapacitation), [suppression](#stat-suppression), cover impact,
cover destruction, collateral consequences, and an atomic [aggregate attack resolution](#concept-aggregate-attack-resolution).

It does not model aiming UI, projectile flight, geometry traversal, animation, audio, networking, or
entity selection. In particular, the registered [line-of-sight implementation](#concept-registered-line-of-sight-implementation)
remains external. The bounded model accepts valid integer [samples](#unit-samples) and calculates a
[fixed-point ratio](#unit-fixed-point-ratio); it does not copy the supercover algorithm.

The model has three useful scales:

| Domain subject | Quint scale | Why |
|---|---|---|
| rule identity and dependencies | [sets](#def-set) of catalogue [records](#def-record) | inspect completeness and relationships without state |
| arithmetic and classification | [pure functions](#def-pure-function) | ask the same input question without changing combat |
| completed consequences | guarded [actions](#def-action) over one cohesive state | match the runtime's observable entry-point granularity |

That boundary is why a learner can inspect [wound](#concept-wound) or [penetration](#concept-penetration) logic independently while still seeing
only completed consequences in the [execution trace](#def-execution-trace).

<a id="chapter-07-the-sixteen-rule-catalogue"></a>
### 7. The sixteen-rule catalogue

The catalogue is executable model data, not a prose [list](#def-list). Each [`RuleEntry`](#qnt-rule-entry) carries stable identity,
kind, direct dependencies, reads, effects, and events. The sixteen entries below are the complete
registry represented by [ruleCatalogue](#qnt-rule-catalogue).

| Stable rule | Kind | Direct dependencies | Primary Quint visibility |
|---|---|---|---|
| [CONTENT-WEAPON-RIFLE-001](#rule-content-weapon-rifle-001) | fact | none | [rifleDamageRaw](#qnt-rifle-damage-raw) |
| [CONTENT-BODY-HUMAN-001](#rule-content-body-human-001) | fact | none | [humanArmorRetentionRaw](#qnt-human-armor-retention-raw) |
| [COMBAT-ENGAGEMENT-001](#rule-combat-engagement-001) | formula | none | [preparationRaw](#qnt-preparation-raw) |
| [COMBAT-TRACE-002](#rule-combat-trace-002) | algorithm | none | [traceAlgorithm](#qnt-trace-algorithm), [validTrace](#qnt-valid-trace), [traceRaw](#qnt-trace-raw) |
| [COMBAT-ARMOR-004](#rule-combat-armor-004) | formula | none | [retainedEffect](#qnt-retained-effect) |
| [COMBAT-DAMAGE-001](#rule-combat-damage-001) | formula | rifle fact, trace, armor | [expectedDamageRaw](#qnt-expected-damage-raw), [roundedDamage](#qnt-rounded-damage), [damageForAttack](#qnt-damage-for-attack) |
| [COMBAT-COLLISION-001](#rule-combat-collision-001) | transition | trace | `contact`, [coverObservation](#qnt-cover-observation) |
| [COMBAT-COVER-003](#rule-combat-cover-003) | transition | collision | [coverDamage](#qnt-cover-damage), [nextCoverImpact](#qnt-next-cover-impact) |
| [COMBAT-PENETRATION-001](#rule-combat-penetration-001) | transition | cover, armor | [retainedEffect](#qnt-retained-effect), `retentionRaw` |
| [COMBAT-HEALTH-001](#rule-combat-health-001) | transition | [damage](#stat-damage) | [nextConsequences](#qnt-next-consequences), [bounded100](#qnt-bounded100) |
| [COMBAT-WOUND-001](#rule-combat-wound-001) | transition | [health](#stat-health) | [woundForDamage](#qnt-wound-for-damage), [zeroHealthMeansIncapacitated](#run-zero-health-means-incapacitated) |
| [COMBAT-SUPPRESSION-001](#rule-combat-suppression-001) | transition | collision | [suppressionForDamage](#qnt-suppression-for-damage), `suppressionDelta` |
| [COMBAT-SUPPRESSION-RECOVERY-001](#rule-combat-suppression-recovery-001) | transition | [suppression](#stat-suppression) | [recoveredSuppression](#qnt-recovered-suppression), [resolveRecovery](#qnt-resolve-recovery) |
| [COMBAT-COLLATERAL-001](#rule-combat-collateral-001) | transition | collision | [alliedAttack](#qnt-allied-attack), [factionNeutralCollateral](#property-faction-neutral-collateral) |
| [COMBAT-COVER-DESTRUCTION-001](#rule-combat-cover-destruction-001) | transition | cover | `destroyed`, [destroyedCoverIsPermeable](#property-destroyed-cover-is-permeable) |
| [COMBAT-ATTACK-RESOLUTION-001](#rule-combat-attack-resolution-001) | transition | engagement, collision, cover, [penetration](#concept-penetration), [damage](#stat-damage), [wound](#concept-wound), [suppression](#stat-suppression), collateral | [resolveConsequences](#qnt-resolve-consequences), [consequenceObservation](#qnt-consequence-observation) |

“Primary visibility” does not imply exclusive ownership. For example, one
[Observation](#qnt-observation) can explain [damage](#stat-damage), [wound](#concept-wound), [suppression](#stat-suppression), [penetration](#concept-penetration), collision, and
collateral participation in a single atomic consequence.

<a id="chapter-08-rule-dependency-and-explanation-order-maps"></a>
### 8. Rule dependency and explanation-order maps

Direct dependencies say which rule definitions a rule relies on. They do not prescribe execution or
display order.

```text
rifle fact ───────────────┐
trace algorithm ───────┐  ├─> damage ─> health ─> wound
armor formula ──────┐  └──┘
                    └────────> penetration
trace algorithm ─> collision ─> cover ─> cover destruction
                            ├─> suppression ─> suppression recovery
                            └─> collateral
engagement + collision + cover + penetration + damage + wound + suppression + collateral
                            └─> aggregate attack resolution
```

The two facts and engagement formula have no direct dependency. The trace contract depends on no
other registry entry because geometry is external. The graph is acyclic; it can therefore be read from
inputs toward completed consequences.

[Explanation order](#concept-explanation-order) answers a different question: in what stable sequence
should a completed observation cite participating rules? [consequenceExplanationOrder](#qnt-consequence-explanation-order)
starts with collision, then engagement, trace, armor, [damage](#stat-damage), cover, [penetration](#concept-penetration), [health](#stat-health), [wound](#concept-wound),
[suppression](#stat-suppression), and collateral. That [list](#def-list) is deterministic explanatory metadata. It is neither a queue of
runtime calls nor evidence that the aggregate [action](#def-action) exposed eleven intermediate states.

<a id="chapter-09-combat-state-inputs-observations-and-units"></a>
### 9. Combat state, inputs, observations, and units

The learning spine uses three records. [CombatState](#qnt-combat-state) is the durable bounded state before and after an [action](#def-action). [AttackInput](#qnt-attack-input) is an immutable description of one attempted attack. [Observation](#qnt-observation) is the explanation emitted for the completed [action](#def-action); it is not a second source of state.

| Record | Representative fields | Question it answers |
|---|---|---|
| [CombatState](#qnt-combat-state) | [health](#stat-health), [suppression](#stat-suppression), cover fields, `incapacitated` | What is true now? |
| [AttackInput](#qnt-attack-input) | [target footprint](#concept-target-footprint), raw base/retention, trace [samples](#unit-samples), range, factions, event id | What attempt is being resolved? |
| [Observation](#qnt-observation) | [action](#def-action), [damage](#stat-damage), preparation/trace/retention, [wound](#concept-wound), [explanation order](#concept-explanation-order) | What completed result should a reviewer explain? |

These declarations are exact excerpts from the literate authority:

```quint authority=sir-combat
  type CombatState = {
    health: int,
    suppression: int,
    coverIntegrity: int,
    coverBlocking: bool,
    incapacitated: bool,
  }
  type AttackInput = {
    hasTargetFootprint: bool,
    baseDamageRaw: int,
    visibleSamples: int,
    totalSamples: int,
    rangeCells: int,
    armorRetentionRaw: int,
    suppressionDelta: int,
    directAttack: bool,
    projectileBlocking: bool,
    attackerFaction: str,
    targetFaction: str,
    eventId: str,
  }
  type Observation = {
    lastAction: str,
    damage: int,
    preparationRaw: int,
    traceRaw: int,
    retentionRaw: int,
    wound: Wound,
    contact: bool,
    suppressionDelta: int,
    coverDamage: int,
    destroyed: bool,
    stopsProjectile: bool,
    explanationOrder: List[str],
    eventId: str,
    attackerFaction: str,
    targetFaction: str,
  }
```

Fields ending in `Raw` use a [Q4 raw integer](#unit-q4-raw-integer) at [scale 10,000](#unit-scale-10-000). Whole state quantities such as [health](#stat-health), [damage](#stat-damage), and [suppression](#stat-suppression) are bounded integer points. Trace inputs are integer [samples](#unit-samples); range is measured in [cells](#unit-cells).

<a id="chapter-10-what-the-bounded-q4-model-includes-and-excludes"></a>
### 10. What the bounded Q4 model includes and excludes

Included values are intentionally small and reviewable: [health](#stat-health),
[suppression](#stat-suppression), and [cover integrity](#stat-cover-integrity) stay within 0–100;
ratios use signed [Q4 raw integers](#unit-q4-raw-integer); trace inputs are valid non-negative sample
counts; and every observation names one completed [action](#def-action) and [event identity](#concept-event-identity).

The model includes the full sixteen-rule identity and dependency registry, all rule-relevant pure
helpers, three state-changing entry points, seven named positive runs, and state properties for bounds,
incapacity, [destroyed cover](#concept-destroyed-cover), valid trace observations, [suppression eligibility](#concept-suppression-eligibility), and faction-neutral
collateral behavior.

It excludes spatial entity state, maps, ray or supercover steps, projectile locations, armor-location
selection, [wound](#concept-wound) collections, and an event stream. A single [wound](#concept-wound) classification and
boolean [incapacitation](#concept-incapacitation) are sufficient for the bounded registry questions. Adding fields merely to make
a prose walkthrough feel sequential would create a false runtime contract.

<a id="part-iii"></a>
## Part III: Decisions to model

<a id="chapter-11-extracting-entities-operations-assumptions-and-p"></a>
### 11. Extracting entities, operations, assumptions, and properties from design text

Use four questions when turning a combat statement into Quint:

1. **What persists?** Put only durable post-[action](#def-action) facts in [CombatState](#qnt-combat-state).
2. **What describes this attempt?** Put immutable attack facts in [AttackInput](#qnt-attack-input).
3. **What explains the completed result?** Put explanatory output in [Observation](#qnt-observation).
4. **What must always remain true?** Write a [property](#def-property) over state or the latest observation.

“Suppression rises only when [damage](#stat-damage) is positive” becomes the pure helper
[suppressionForDamage](#qnt-suppression-for-damage), an observed `suppressionDelta`, and the
[suppressionRequiresDamage](#property-suppression-requires-damage) [property](#def-property). “[Destroyed cover](#concept-destroyed-cover) no longer
blocks future projectiles, but consumes the collision that destroyed it” becomes
[nextCoverImpact](#qnt-next-cover-impact), `destroyed`, `stopsProjectile`, and
[destroyedCoverIsPermeable](#property-destroyed-cover-is-permeable). The first answer is state; the other
fields explain the current completed [action](#def-action).

<a id="chapter-12-mapping-rule-kinds-to-quint-constructs"></a>
### 12. Mapping rule kinds to Quint constructs

| Rule kind | Default construct | Combat examples | Review question |
|---|---|---|---|
| fact | [pure value](#def-pure-value) plus catalogue row | [rifleDamageRaw](#qnt-rifle-damage-raw), [humanArmorRetentionRaw](#qnt-human-armor-retention-raw) | Is the value stable and unit-labelled? |
| formula | [pure function](#def-pure-function) | [preparationRaw](#qnt-preparation-raw), [retainedEffect](#qnt-retained-effect), [damageForAttack](#qnt-damage-for-attack) | Can any caller inspect the same result without changing state? |
| external algorithm | contract [record](#def-record) plus bounded adapter | [traceAlgorithm](#qnt-trace-algorithm), [traceRaw](#qnt-trace-raw) | Are implementation identity, input units, and result [type](#def-type) explicit? |
| transition | pure next-state/observation helpers plus an [action](#def-action), or participation in an aggregate [action](#def-action) | [nextCoverImpact](#qnt-next-cover-impact), [resolveRecovery](#qnt-resolve-recovery), [resolveConsequences](#qnt-resolve-consequences) | Does granularity match a real observable entry point? |

The mapping is a default, not a one-rule/one-[action](#def-action) quota. A transition such as
[COMBAT-PENETRATION-001](#rule-combat-penetration-001) is visible through retained-effect arithmetic
and the completed attack observation; creating `resolvePenetration` would misrepresent the runtime.

<a id="chapter-13-choosing-state-shape-and-action-granularity"></a>
### 13. Choosing state shape and action granularity

The state shape is cohesive because the invariants relate its fields. Zero [health](#stat-health)
must agree with [incapacitation](#concept-incapacitation); zero [cover integrity](#stat-cover-integrity) must agree with future
[cover blocking](#concept-cover-blocking); every bounded quantity must stay within 0–100.

Action boundaries follow production entry points:

| Action | Atomic update | Separate because |
|---|---|---|
| [resolveConsequences](#qnt-resolve-consequences) | [health](#stat-health), [wound](#concept-wound) explanation, [incapacitation](#concept-incapacitation), [suppression](#stat-suppression), one completed observation | production publishes completed attack consequences |
| [resolveCoverImpact](#qnt-resolve-cover-impact) | [cover integrity](#stat-cover-integrity)/blocking plus current-collision observation | cover impact is a distinct runtime entry point |
| [resolveRecovery](#qnt-resolve-recovery) | [suppression](#stat-suppression) reduction plus recovery observation | recovery is a distinct runtime entry point |

Pure next-state helpers let learners inspect each calculation without adding observable states. This is
the central rule: explanation may be fine-grained; [state transition](#def-state-transition) granularity must remain honest.

<a id="chapter-14-fixed-point-q4-arithmetic-and-rounding"></a>
### 14. Fixed-point Q4 arithmetic and rounding

[Q4 raw integers](#unit-q4-raw-integer) avoid host floating-point behavior. Multiply the human value by the [scale 10,000](#unit-scale-10-000): `25` becomes `250000`, `1.0` becomes `10000`, and `0.8` becomes `8000`.

The ordinary representative path is:

| Layer | Raw calculation | Human value |
|---|---:|---:|
| rifle [base damage](#stat-base-damage) | `250000` | `25` |
| full [trace probability](#stat-trace-probability) | `10 / 10 → 10000` | `1.0` |
| first fixed multiply | `250000 × 10000 / 10000 = 250000` | `25 × 1.0 = 25` |
| retained [armor retention](#stat-armor-retention) | `8000` | `0.8` |
| second fixed multiply | `250000 × 8000 / 10000 = 200000` | `25 × 0.8 = 20` |
| final whole-point rounding | `(200000 + 5000) / 10000 = 20` | `20` |

The authority uses [round-half-away-from-zero](#unit-round-half-away-from-zero) inside fixed-point conversion and multiplication:

```quint authority=sir-combat
  pure def divideRoundedAwayFromZero(numerator: int, denominator: int): int = {
    val quotient = numerator / denominator
    val remainder = numerator % denominator
    if (absolute(remainder) * 2 < absolute(denominator)) quotient
    else if ((numerator < 0) != (denominator < 0)) quotient - 1
    else quotient + 1
  }

  pure def fromRatio(numerator: int, denominator: int): int =
    saturateInt32(divideRoundedAwayFromZero(numerator * SCALE, denominator))

  pure def addFixed(left: int, right: int): int = saturateInt32(left + right)

  pure def multiplyFixed(left: int, right: int): int =
    saturateInt32(divideRoundedAwayFromZero(left * right, SCALE))
```

There is one important [claim boundary](#def-claim-boundary): [fromRatio](#qnt-from-ratio), [addFixed](#qnt-add-fixed), and [multiplyFixed](#qnt-multiply-fixed) call [saturateInt32](#qnt-saturate-int32), but [roundedDamage](#qnt-rounded-damage) deliberately models the runtime's signed int32 wrap in `rawDamage + SCALE / 2` before division. The representative `200000 + 5000` is safely inside int32. Do not infer from this example that overflow saturates at the final rounding layer; the separate authority [damageRoundingPreservesInt32Wrap](#run-damage-rounding-preserves-int32-wrap) [run](#def-run) preserves that edge explicitly.

<a id="chapter-15-the-external-line-of-sight-contract-boundary"></a>
### 15. The external line-of-sight contract boundary

[COMBAT-TRACE-002](#rule-combat-trace-002) names the external contract:

```quint authority=sir-combat
  pure val traceAlgorithm = {
    id: "COMBAT-TRACE-002",
    implementation: "FS.GG.Game.Core.Los.lineOfSightBy",
    fingerprint: "FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover",
    inputs: Set("visible:int:samples", "total:int:samples"),
    result: "fixedPoint:ratio",
    explanationFields: List("visibleSamples", "totalSamples", "lineMode"),
  }
```

The implementation and fingerprint bind the contract to the registered
[line-of-sight implementation](#concept-registered-line-of-sight-implementation). The input [set](#def-set) names
integer [samples](#unit-samples), not [cells](#unit-cells) or pixels. The result names a
[fixed-point ratio](#unit-fixed-point-ratio), not a boolean ray result.

The bounded adapter checks `total > 0` and `0 <= visible <= total`, then converts `visible / total` to
[scale 10,000](#unit-scale-10-000). A 0/10 trace yields 0; 5/10 yields 5000; 10/10 yields 10000. Geometry decides which
[samples](#unit-samples) are visible. Quint checks the ratio contract and downstream consequences. This separation
prevents a second supercover authority from appearing in the handbook.

<a id="chapter-16-atomic-aggregate-consequences-versus-focused-pur"></a>
### 16. Atomic aggregate consequences versus focused pure helpers

The aggregate [action](#def-action) has one [guard](#def-guard) and two primed assignments:

```quint authority=sir-combat
  action resolveConsequences(input: AttackInput): bool = all {
    validAttack(input),
    combat' = nextConsequences(combat, input),
    last' = consequenceObservation(input),
  }
```

The [guard](#def-guard) rejects an absent [target footprint](#concept-target-footprint) or invalid trace. The next-state helper computes bounded
[health](#stat-health), eligible [suppression](#stat-suppression), and [incapacitation](#concept-incapacitation). The observation helper computes [damage](#stat-damage), preparation,
trace, retention, [wound](#concept-wound) classification, contact, [explanation order](#concept-explanation-order), factions, and [event identity](#concept-event-identity).

Those helpers are independently queryable and testable, but the trace sees one before-state and one
after-state. There is no state in which [health](#stat-health) changed while [suppression](#stat-suppression) or [incapacitation](#concept-incapacitation) still carries
the prior consequence. Focused explanations do not weaken aggregate atomicity.

<a id="part-iv"></a>
## Part IV: Quint foundations through combat

<a id="chapter-17-modules-types-variants-records-sets-and-lists"></a>
### 17. Modules, types, variants, records, sets, and lists

The [SirCombat](#qnt-sir-combat) [module](#def-module) holds model declarations; [SirCombatTests](#qnt-sir-combat-tests)
imports them and holds named runs. [`Wound`](#qnt-wound) is a [variant](#def-variant) with
[NoWound](#qnt-no-wound), [MinorWound](#qnt-minor-wound), and [MajorWound](#qnt-major-wound).
[CombatState](#qnt-combat-state), [AttackInput](#qnt-attack-input), and
[Observation](#qnt-observation) are typed records. Catalogue collections use [sets](#def-set) because
identity and membership matter; explanation fields use [lists](#def-list) because stable order matters.

This choice is semantic: reordering a [set](#def-set) changes nothing, while reordering
[consequenceExplanationOrder](#qnt-consequence-explanation-order) changes the explanation contract.

<a id="chapter-18-constants-model-data-and-the-rule-catalogue"></a>
### 18. Constants, model data, and the rule catalogue

Constants name scale and integer boundaries; model data names domain facts and representative inputs.
The catalogue is also a [pure value](#def-pure-value), so completeness can be queried without an
initialized state.

```quint authority=sir-combat
  pure val SCALE = 10000
  pure val INT32_MIN = -2147483648
  pure val INT32_MAX = 2147483647
  pure val UINT32_RANGE = 4294967296
  pure val rifleDamageRaw = 250000
  pure val humanArmorRetentionRaw = SCALE
  pure val rangeSlopeRaw = 1000
```

[rifleDamageRaw](#qnt-rifle-damage-raw) and [humanArmorRetentionRaw](#qnt-human-armor-retention-raw)
make the two fact rules executable. [rangeSlopeRaw](#qnt-range-slope-raw) supports engagement
preparation. The integer boundaries make saturation and the documented pre-division wrap explicit.
[sixteenRulesDeclared](#property-sixteen-rules-declared) then checks that the registry size remains exactly
sixteen.

<a id="chapter-19-pure-functions-and-deterministic-damage-calculat"></a>
### 19. Pure functions and deterministic damage calculations

Pure functions let us inspect the [damage](#stat-damage) calculation without changing model state. A valid full trace converts `10 / 10` to raw `10000`; [retainedEffect](#qnt-retained-effect) clamps retention to `0..10000`; [expectedDamageRaw](#qnt-expected-damage-raw) performs the two fixed multiplications; and [roundedDamage](#qnt-rounded-damage) produces whole [damage points](#unit-damage-points).

```quint authority=sir-combat
  pure def validTrace(visible: int, total: int): bool = and { total > 0, visible >= 0, visible <= total }
  pure def traceRaw(visible: int, total: int): int = fromRatio(visible, total)
  pure def expectedDamageRaw(baseDamageRaw: int, trace: int, retention: int): int =
    multiplyFixed(multiplyFixed(baseDamageRaw, trace), retainedEffect(retention))
  pure def roundedDamage(rawDamage: int): int = wrapInt32(rawDamage + SCALE / 2) / SCALE
```

[damageForAttack](#qnt-damage-for-attack) composes those helpers. For the representative input it applies [roundedDamage](#qnt-rounded-damage) to [expectedDamageRaw](#qnt-expected-damage-raw) with raw arguments `250000`, `10000`, and `8000`, hence `20`.

<a id="chapter-20-variables-initialization-and-cohesive-combat-sta"></a>
### 20. Variables, initialization, and cohesive combat state

The model has two state variables: [combat](#qnt-combat) carries [CombatState](#qnt-combat-state), and [last](#qnt-last) carries the most recent [Observation](#qnt-observation). [initialCombat](#qnt-initial-combat) starts at 100 [health](#stat-health), zero [suppression](#stat-suppression), intact blocking cover, and no [incapacitation](#concept-incapacitation). The [init](#qnt-init) [action](#def-action) assigns both primed variables, so every [run](#def-run) begins from a fully specified state rather than an accidental default.

<a id="chapter-21-guards-actions-primed-assignments-and-disabled-t"></a>
### 21. Guards, actions, primed assignments, and disabled transitions

An [action](#def-action) describes one allowed [state transition](#def-state-transition). [resolveConsequences](#qnt-resolve-consequences) first uses [validAttack](#qnt-valid-attack) as a [guard](#def-guard). If the [target footprint](#concept-target-footprint) or trace is invalid, the [action](#def-action) is disabled. If valid, the primed assignments publish the next cohesive state and its [observation](#qnt-observation) together:

```quint authority=sir-combat
  action resolveConsequences(input: AttackInput): bool = all {
    validAttack(input),
    combat' = nextConsequences(combat, input),
    last' = consequenceObservation(input),
  }
```

The apostrophe means “value in the successor state.” Both assignments belong to one atomic [action](#def-action); the model does not expose a half-updated state in which [health](#stat-health) changed but the explanation did not.

<a id="chapter-22-nondeterministic-steps-and-possible-combat-histo"></a>
### 22. Nondeterministic steps and possible combat histories

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-23-runs-witnesses-invariants-simulations-and-bounde"></a>
### 23. Runs, witnesses, invariants, simulations, and bounded verification

A [run](#def-run) is an executable, named scenario. The representative [run](#def-run) is a [witness](#def-witness) that this path is reachable and produces the predicted result. It is not an [invariant](#def-invariant), and its success alone is not proof that every possible input or the production implementation behaves equivalently.

```quint authority=sir-combat
  run representativeDamageIsTwenty =
    init
      .then(resolveConsequences(representativeAttack))
      .expect(and {
        last.damage == 20,
        last.preparationRaw == 13000,
        last.traceRaw == SCALE,
        last.retentionRaw == 8000,
        combat.health == 80,
        combat.suppression == 12,
        last.explanationOrder == consequenceExplanationOrder,
        sixteenRulesDeclared,
        boundedCombatState,
        incapacityMatchesHealth,
      })
```

The named expectations make a failure local: arithmetic, preparation, trace, retention, state, [explanation order](#concept-explanation-order), catalogue integrity, and bounds each have a visible check.

<a id="part-v"></a>
## Part V: Guided walkthroughs

<a id="chapter-24-representative-rifle-damage-25-x-1-0-x-0-8-20"></a>
### 24. Representative rifle damage: 25 x 1.0 x 0.8 = 20

Use the same four moves whenever you meet a new model path: **predict → [run](#def-run) → observe → explain**.

#### Predict

Before executing, write down the expected values:

- rifle [base damage](#stat-base-damage): `25`, raw `250000`;
- [trace probability](#stat-trace-probability): `10 / 10 = 1.0`, raw `10000`;
- [armor retention](#stat-armor-retention): `0.8`, raw `8000`;
- [expected damage](#stat-expected-damage): `25 × 1.0 × 0.8 = 20`, raw `200000` before whole-point rounding;
- successor: [health](#stat-health) `80`, [suppression](#stat-suppression) `12`, not incapacitated.

The exact representative input is:

```quint authority=sir-combat
  pure val representativeAttack: AttackInput = {
    hasTargetFootprint: true,
    baseDamageRaw: rifleDamageRaw,
    visibleSamples: 10,
    totalSamples: 10,
    rangeCells: 3,
    armorRetentionRaw: 8000,
    suppressionDelta: 12,
    directAttack: true,
    projectileBlocking: true,
    attackerFaction: "Blue",
    targetFaction: "Red",
    eventId: "attack:representative",
  }
```

#### Run

From the repository root, use the pinned end-to-end qualification:

```console
./scripts/qualify-quint-q4-sir-combat.sh
```

For the smallest model-only loop, deterministically extract the `quint sir-combat.qnt +=` fences from `docs/rules/sir-combat.md`, then [run](#def-run):

```console
quint test <extracted-sir-combat.qnt> --main SirCombatTests --backend rust --seed 352 --match representativeDamageIsTwenty --verbosity 3
```

The repository gate performs that extraction for you, requires Quint `0.32.0`, and also restores green after its deliberate negative controls.

#### Observe

Read the [execution trace](#def-execution-trace) as two states:

| Field | After [init](#qnt-init) | After [resolveConsequences(representativeAttack)](#qnt-resolve-consequences) |
|---|---:|---:|
| [combat.health](#stat-health) | `100` | `80` |
| [combat.suppression](#stat-suppression) | `0` | `12` |
| `combat.incapacitated` | `false` | `false` |
| `last.lastAction` | `Initialize` | `ResolveConsequences` |
| [last.damage](#stat-damage) | `0` | `20` |
| `last.preparationRaw` | `0` | `13000` (`1.3`) |
| `last.traceRaw` | `0` | `10000` (`1.0`) |
| `last.retentionRaw` | `0` | `8000` (`0.8`) |
| `last.eventId` | `initialize` | `attack:representative` |

#### Explain

The input supplies `250000`, `10/10`, and `8000`. The trace helper produces `10000`. Two fixed multiplications produce `200000`. Final rounding produces `20`. [nextConsequences](#qnt-next-consequences) subtracts `20` from `100` and applies `12` [suppression](#stat-suppression) because [damage](#stat-damage) is positive. [consequenceObservation](#qnt-consequence-observation) records the same calculation and ordered rule explanation. One arithmetic story now agrees at the domain, encoding, pure-function, [action](#def-action), state, and [observation](#qnt-observation) layers.

#### Penetration and aggregate resolution

[COMBAT-PENETRATION-001](#rule-combat-penetration-001) is intentionally visible at formula and
observation granularity. [retainedEffect](#qnt-retained-effect) bounds the supplied retention to
`0..10000`; [damageForAttack](#qnt-damage-for-attack) applies it; and `last.retentionRaw` publishes the
retained ratio in the completed result. There is no standalone [penetration](#concept-penetration) [action](#def-action),
because the production entry point does not expose a state between [penetration](#concept-penetration) and [damage](#stat-damage).

[COMBAT-ATTACK-RESOLUTION-001](#rule-combat-attack-resolution-001) is visible through the complete
[resolveConsequences](#qnt-resolve-consequences) transition and [consequenceObservation](#qnt-consequence-observation).
The successor simultaneously has [health](#stat-health) `80`, [suppression](#stat-suppression) `12`, and the final [incapacitation](#concept-incapacitation) value;
the explanation lists the participating rules without turning them into extra states.

<a id="chapter-25-a-miss-causes-neither-damage-nor-suppression"></a>
### 25. A miss causes neither damage nor suppression

[missedAttack](#qnt-missed-attack) changes the representative trace to zero [visible samples](#stat-visible-samples) while
leaving requested [suppression](#stat-suppression) at `12`. Predict zero trace, zero [damage](#stat-damage), unchanged [health](#stat-health), and zero
applied [suppression](#stat-suppression).

```quint authority=sir-combat
  pure def suppressionForDamage(damage: int, requestedDelta: int): int =
    if (damage > 0) maximum(0, requestedDelta) else 0
```

This helper encodes [suppression eligibility](#concept-suppression-eligibility): a request is not an
application. [traceRaw](#qnt-trace-raw) produces zero, [damageForAttack](#qnt-damage-for-attack)
produces zero, and [suppressionForDamage](#qnt-suppression-for-damage) therefore produces zero. The
first expectation in [suppressionNeedsPositiveDamageAndRecoversFive](#run-suppression-needs-positive-damage-and-recovers-five)
checks all three facts. The [suppressionRequiresDamage](#property-suppression-requires-damage) [property](#def-property)
generalizes the observation constraint beyond this one [witness](#def-witness).

<a id="chapter-26-wound-thresholds-at-damage-24-25-and-50"></a>
### 26. Wound thresholds at damage 24, 25, and 50

The [wound](#concept-wound) rule classifies whole [damage points](#unit-damage-points), after fixed-point calculation and
rounding:

```quint authority=sir-combat
  pure def woundForDamage(damage: int): Wound =
    if (damage >= 50) MajorWound else if (damage >= 25) MinorWound else NoWound
```

Predict the boundaries before running:

| Damage | Expected classification | Reason |
|---:|---|---|
| `24` | [NoWound](#qnt-no-wound) | below the first inclusive threshold |
| `25` | [MinorWound](#qnt-minor-wound) | exactly the first inclusive threshold |
| `50` | [MajorWound](#qnt-major-wound) | exactly the second inclusive threshold |

[woundThresholdsAreExact](#run-wound-thresholds-are-exact) performs those three consequences in order.
Because state persists across the [run](#def-run), [health](#stat-health) becomes `76`, then `51`, then `1`; the final expectation
also checks that the major [wound](#concept-wound) did not imply [incapacitation](#concept-incapacitation) while [health](#stat-health) remains positive. The [wound](#concept-wound)
is explanatory output for each completed attack, not a newly invented [wound](#concept-wound)-[list](#def-list) state.

<a id="chapter-27-health-reaching-zero-and-incapacitation"></a>
### 27. Health reaching zero and incapacitation

[nextConsequences](#qnt-next-consequences) clamps successor [health](#stat-health) to 0–100 and derives [incapacitation](#concept-incapacitation)
from the same `nextHealth` value:

```quint authority=sir-combat
  pure def nextConsequences(current: CombatState, input: AttackInput): CombatState = {
    val damage = damageForAttack(input)
    val nextHealth = bounded100(current.health - damage)
    val appliedSuppression = suppressionForDamage(damage, input.suppressionDelta)
    {
      health: nextHealth,
      suppression: bounded100(current.suppression + appliedSuppression),
      coverIntegrity: current.coverIntegrity,
      coverBlocking: current.coverBlocking,
      incapacitated: nextHealth == 0,
    }
  }
```

For a 100-point full-trace attack, predict [health](#stat-health) `0` and incapacitated `true` in the same successor
state. [zeroHealthMeansIncapacitated](#run-zero-health-means-incapacitated) witnesses that path, while
[incapacityMatchesHealth](#property-incapacity-matches-health) checks the relationship as a state
[property](#def-property). No intermediate “[health](#stat-health) zero but still active” state is visible.

<a id="chapter-28-cover-impact-destruction-permeability-and-the-cu"></a>
### 28. Cover impact, destruction, permeability, and the current collision

Cover [damage](#stat-damage) is at least one point and otherwise half the supplied whole [base damage](#stat-base-damage). A base value of
`250` therefore gives `125`; bounded subtraction takes integrity from `100` to `0`.

```quint authority=sir-combat
  action resolveCoverImpact(
    baseDamage: int,
    projectileBlocking: bool,
    directAttack: bool,
    eventId: str
  ): bool = all {
    combat' = nextCoverImpact(combat, baseDamage),
    last' = coverObservation(combat, baseDamage, projectileBlocking, directAttack, eventId),
  }
```

The successor cover is permeable: `coverBlocking` becomes false when remaining integrity is zero. The
current observation still reports `stopsProjectile = true` because the direct projectile collided with
blocking cover before that collision destroyed it. This is [current collision consumption](#concept-current-collision-consumption):
future projectiles pass, but the destroying projectile does not retroactively pass through.

[destroyingCoverConsumesCurrentCollision](#run-destroying-cover-consumes-current-collision) checks
[damage](#stat-damage) `125`, integrity `0`, `destroyed`, `stopsProjectile`, and
[destroyedCoverIsPermeable](#property-destroyed-cover-is-permeable) together. The observation's
[explanation order](#concept-explanation-order) names cover destruction then cover impact; it is explanatory order, not two state
transitions.

<a id="chapter-29-suppression-eligibility-and-five-point-recovery"></a>
### 29. Suppression eligibility and five-point recovery

Positive [damage](#stat-damage) may apply a non-negative requested [suppression delta](#stat-suppression-delta); a miss applies none. Recovery is
a separate [action](#def-action) and removes at most five points:

```quint authority=sir-combat
  pure def recoveredSuppression(currentSuppression: int): int = minimum(5, maximum(0, currentSuppression))
```

```quint authority=sir-combat
  action resolveRecovery(eventId: str): bool = all {
    combat' = nextRecovery(combat),
    last' = recoveryObservation(combat, eventId),
  }
```

Follow [suppressionNeedsPositiveDamageAndRecoversFive](#run-suppression-needs-positive-damage-and-recovers-five)
as three checkpoints: the miss leaves [suppression](#stat-suppression) `0`; the representative hit raises it to `12`; one
recovery lowers it to `7` and records delta `-5`. If current [suppression](#stat-suppression) were below five,
[recoveredSuppression](#qnt-recovered-suppression) would remove only what exists. Recovery has its own
[action](#def-action) because it is a separate runtime entry point, not an attack sub-step.

<a id="chapter-30-faction-neutral-collateral-consequences"></a>
### 30. Faction-neutral collateral consequences

[alliedAttack](#qnt-allied-attack) changes only the target faction from `Red` to `Blue`. Damage and
[suppression](#stat-suppression) calculations do not branch on faction. Predict the same [health](#stat-health) `80` and [suppression](#stat-suppression) `12`
as the representative hostile attack.

```quint authority=sir-combat
  pure val alliedAttack: AttackInput = {
    ...representativeAttack,
    targetFaction: "Blue",
    eventId: "collateral:allies",
  }
```

[collateralOutcomeIgnoresFaction](#run-collateral-outcome-ignores-faction) checks both the concrete
successor and [factionNeutralCollateral](#property-faction-neutral-collateral), which compares the two
pure next-state results. Faction identity remains in the observation for explanation and replay, but it
does not silently immunize allies. This is a bounded [collateral consequence](#concept-collateral-consequence), not a target-selection or
policy model.

<a id="chapter-31-registered-external-line-of-sight-behavior"></a>
### 31. Registered external line-of-sight behavior

To review the boundary, separate three claims:

1. [`traceAlgorithm`](#qnt-trace-algorithm) registers `FS.GG.Game.Core.Los.lineOfSightBy` with the
   `FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover` fingerprint.
2. [validTrace](#qnt-valid-trace) accepts only `total > 0` and `0 <= visible <= total`.
3. [traceRaw](#qnt-trace-raw) converts accepted counts into a scale-10,000 ratio used by [damage](#stat-damage).

The representative 10/10 and missed 0/10 paths exercise the two ratio extremes. The
[validTraceObservation](#property-valid-trace-observation) [property](#def-property) bounds emitted trace ratios for
completed consequences. None of these claims proves how supercover chooses [visible samples](#stat-visible-samples); that
behavior belongs to the registered external implementation and its own evidence. The handbook's model
starts after geometry has produced the counts.

<a id="part-vi"></a>
## Part VI: Formal reasoning in practice

<a id="chapter-32-choosing-an-example-witness-or-invariant"></a>
### 32. Choosing an example, witness, or invariant

Choose the weakest claim that answers the question. A stronger-sounding claim is not automatically a
better one: it may cost more, require bounds the model does not have, or say something different from
what the learner needs.

| Evidence form | Precise reading | What it does **not** establish |
|---|---|---|
| Example — one concrete calculation | Evaluating `woundForDamage(25)` illustrates one input/output fact. | It says nothing about other inputs or whether an [action](#def-action) can reach that value. |
| Reachable [witness](#def-witness) — an existential execution | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) shows at least one enabled path from [init](#qnt-init) through [resolveConsequences](#qnt-resolve-consequences) to the expected successor. | It does not say every path has that result. |
| [Invariant](#def-invariant) — a predicate over every checked state | [boundedCombatState](#property-bounded-combat-state) must hold for each [reachable state](#def-reachable-state) explored by the checker. | Its strength is limited by the declared initializer, [step](#qnt-step), and verification bounds. |
| [Sampled execution](#def-sampled-run) — search evidence, not proof | A seeded simulation explores concrete nondeterministic choices and is excellent for learning and finding defects. | Passing [samples](#unit-samples) cannot rule out an unvisited failing history. |
| [Bounded exhaustive verification](#def-bounded-verification) — exhaustive only inside the declared bounds | A checker explores all represented paths within its configured depth/state bounds and either finds a violation or reports none there. | A green bounded check is not an unbounded theorem and does not prove production equivalence. |
| [Counterexample](#def-counterexample) — one concrete refutation | One trace that violates a claimed [property](#def-property) is enough to disprove that claim in the checked model. | It does not by itself diagnose the intended repair or the production implementation. |

Use a concrete calculation to teach arithmetic, a [witness](#def-witness) to establish reachability, an
[invariant](#def-invariant) to constrain all checked successors, and a [counterexample](#def-counterexample)
to learn why a universal claim failed. When reporting results, always name the initializer, transition
relation, bound or sample count, seed when applicable, and exact model revision.

<a id="chapter-33-reading-an-execution-trace"></a>
### 33. Reading an execution trace

Start with [action](#def-action) names, then compare values. In the representative
[execution trace](#def-execution-trace), state 0 is `Initialize` and state 1 is
`ResolveConsequences`. That tells you which transition to explain before you inspect any number.

Next group fields by role:

1. **Inputs reflected in the observation:** event/factions, raw trace, and retained ratio identify the attempted shot.
2. **Calculated explanation:** preparation `13000`, [damage](#stat-damage) `20`, and ordered rule IDs explain why the transition occurred.
3. **Durable consequences:** [health](#stat-health) `80` and [suppression](#stat-suppression) `12` are the successor [CombatState](#qnt-combat-state).
4. **Unchanged state:** [cover integrity](#stat-cover-integrity)/blocking stay unchanged because this is the aggregate consequence [action](#def-action), not a cover-impact [action](#def-action).

Finally reconcile the records. The [last](#qnt-last) [damage](#stat-damage) field equals `20`, explaining the [health](#stat-health) delta `100 - 20 = 80`; positive [damage](#stat-damage) explains the [suppression delta](#stat-suppression-delta) `12`; [health](#stat-health) above zero explains why [incapacitation](#concept-incapacitation) remains false. A trace is useful when these relationships agree, not merely when it contains many fields.

The transition relation is deliberately nondeterministic:

```quint authority=sir-combat
  action step = any {
    resolveConsequences(representativeAttack),
    resolveConsequences(missedAttack),
    resolveCoverImpact(25, true, true, "cover:sample"),
    resolveRecovery("recovery:sample"),
  }
```

At each successor, `any` may choose any enabled branch. A cover-impact trace followed by recovery and a
representative attack followed by recovery can both be valid. No sampled trace is the canonical combat
history. Read each edge with the same five questions:

1. Which [action](#def-action) did `last.lastAction` identify?
2. Which fields changed between predecessor and successor?
3. Which input and [guard](#def-guard) enabled that branch?
4. Which authoritative pure helpers explain the changed values?
5. Which fields stayed unchanged, confirming the [action](#def-action) boundary?

Do not reinterpret [consequenceExplanationOrder](#qnt-consequence-explanation-order) as hidden [state transitions](#def-state-transition).
It orders explanatory rule IDs inside one completed aggregate [action](#def-action); the only observable
successor is the finished [CombatState](#qnt-combat-state) plus [Observation](#qnt-observation).

<a id="chapter-34-reading-and-minimizing-a-counterexample"></a>
### 34. Reading and minimizing a counterexample

Use this fixed path: **Property → earliest bad state → producing [action](#def-action) → changed fields → [guard](#def-guard) and input → authority seam**.

1. Name the failed [property](#def-property) exactly. “The model failed” is too broad.
2. Start at the initialized [record](#def-record) and find the first state where the predicate is false. Later states may
   be consequences, not causes.
3. Read [last](#qnt-last)`.lastAction` on that state and compare only fields its [action](#def-action) may assign.
4. Reconstruct the [guard](#def-guard) and input values. A disabled transition cannot have produced the state.
5. Reduce the trace: keep the shortest prefix that still reaches the violation and the smallest input
   boundary that still fails.
6. Inspect the narrowest literate-authority helper that calculates the bad field. Do not patch the
   generated `.qnt` projection or infer an intermediate production transition.
7. After repair, rerun the unchanged detector and the broader regression suite.

For the threshold defect in the next chapter, changing the minor-[wound](#concept-wound) comparison from `>= 25` to
`> 25` makes [woundThresholdsAreExact](#run-wound-thresholds-are-exact) fail at the [step](#qnt-step) whose input [damage](#stat-damage) is exactly `25`. The earliest
bad field is [`last.wound`](#qnt-last): it is [NoWound](#qnt-no-wound) instead of [MinorWound](#qnt-minor-wound). [Damage](#stat-damage) `24` still passes and [damage](#stat-damage)
`50` still becomes [MajorWound](#qnt-major-wound), so neither belongs in the minimal refutation. The one-step boundary
case is the useful [counterexample](#def-counterexample); a longer trace would add noise.

A minimized [counterexample](#def-counterexample) disproves the checked claim. It does not decide
whether the authority, the detector, or the requirement is wrong. That judgment still comes from the
source-precedence map and the rule's accepted semantics.

<a id="chapter-35-mutation-laboratory"></a>
### 35. Mutation laboratory

The laboratory never edits `docs/rules/sir-combat.md`. It extracts the authoritative [module](#def-module) into a
temporary file, applies exactly one [mutation](#def-mutation), runs the named detector, requires a
non-zero result, discards the fixture, re-extracts the untouched authority, and reruns the identical
detector. This pairs an [observed-red control](#def-observed-red-control) with
[restored green](#def-restored-green), rather than merely showing that the repaired tree happens to pass.

| Family | Deliberate defect in the temporary fixture | Primary detector | Expected red | Restored-green claim |
|---|---|---|---|---|
| threshold | Make minor [wound](#concept-wound) require [damage](#stat-damage) `> 25`. | [woundThresholdsAreExact](#run-wound-thresholds-are-exact) | The exact-25 expectation fails. | Exact 24/25/50 boundaries pass again. |
| bounds | Initialize [health](#stat-health) at `101`. | `boundedInitialState` over [boundedCombatState](#property-bounded-combat-state) | The initialized [health](#stat-health) bound fails. | The same bound accepts authoritative `100`. |
| [suppression](#stat-suppression) | Remove the positive-[damage](#stat-damage) condition from [suppressionForDamage](#qnt-suppression-for-damage). | [suppressionNeedsPositiveDamageAndRecoversFive](#run-suppression-needs-positive-damage-and-recovers-five) | A miss applies [suppression](#stat-suppression). | Miss, hit, and five-point recovery expectations all pass. |
| cover | Keep `coverBlocking = true` when integrity reaches zero. | [destroyingCoverConsumesCurrentCollision](#run-destroying-cover-consumes-current-collision) | Destroyed cover is still blocking. | Destruction is permeable while the current projectile still stops. |
| collateral | Make same-faction input suppress [damage](#stat-damage) in [nextConsequences](#qnt-next-consequences). | [collateralOutcomeIgnoresFaction](#run-collateral-outcome-ignores-faction) | Allied and opposing outcomes diverge. | Both factions again receive identical physical consequences. |
| catalogue integrity | Remove one stable rule row. | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) through [sixteenRulesDeclared](#property-sixteen-rules-declared) | The catalogue cardinality expectation fails. | The exact sixteen-rule [set](#def-set) passes again. |

For each row, the focused qualification records both statuses under one detector identity. The repair
is not evidence until the unchanged detector returns green. The aggregate result therefore contains
six red/green pairs, not six mutations followed by one unrelated successful command.

The earlier `8000 → 7000` retention [mutation](#def-mutation) remains a useful arithmetic lesson:
[prediction](#def-prediction) says `18`, the unchanged representative [witness](#def-witness) expects
`20`, and the fixture fails. M4 adds breadth; it does not replace that already-observed control.

<a id="chapter-36-dead-actions-accidental-stuttering-and-terminal-"></a>
### 36. Dead actions, accidental stuttering, and terminal states

An [action](#def-action) can typecheck yet be unreachable because its [guard](#def-guard) is never true
from [init](#qnt-init). A broad [step](#qnt-step) can also hide accidental [stuttering](#def-stuttering) if a branch reports
success while leaving both variables unchanged. Neither problem is demonstrated by a pure helper test.

M4 therefore requires one reachable [witness](#def-witness) per major state-changing
[action](#def-action):

| Major [action](#def-action) | Named reachable [witness](#def-witness) | Required observable delta |
|---|---|---|
| [resolveConsequences](#qnt-resolve-consequences) | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) | [last](#qnt-last)`.lastAction = "ResolveConsequences"`, [health](#stat-health) `100 → 80`, [suppression](#stat-suppression) `0 → 12`. |
| [resolveCoverImpact](#qnt-resolve-cover-impact) | [destroyingCoverConsumesCurrentCollision](#run-destroying-cover-consumes-current-collision) | [last](#qnt-last)`.lastAction = "ResolveCoverImpact"`, integrity `100 → 0`, blocking `true → false`. |
| [resolveRecovery](#qnt-resolve-recovery) | [suppressionNeedsPositiveDamageAndRecoversFive](#run-suppression-needs-positive-damage-and-recovers-five) | [last](#qnt-last)`.lastAction = "ResolveRecovery"`, [suppression](#stat-suppression) `12 → 7`. |

Those traces establish existence, not universal scheduling. The model has no [terminal state](#def-terminal-state):
after [health](#stat-health) reaches zero, cover impact and recovery remain representable. That is an explicit bounded-model
choice, not a claim that the production session must continue scheduling all [combat](#qnt-combat) commands.

The properties also expose what they constrain instead of relying on suggestive names:

| Model predicate | Kind and authoritative binding |
|---|---|
| [sixteenRulesDeclared](#property-sixteen-rules-declared) | static catalogue integrity over [ruleCatalogue](#qnt-rule-catalogue); it is not a transition-state predicate. |
| [boundedCombatState](#property-bounded-combat-state) | transition [invariant](#def-invariant) over [`combat.health`](#qnt-combat), [`combat.suppression`](#qnt-combat), and [`combat.coverIntegrity`](#qnt-combat). |
| [incapacityMatchesHealth](#property-incapacity-matches-health) | transition [invariant](#def-invariant) over [`combat.incapacitated`](#qnt-combat) and [`combat.health`](#qnt-combat). |
| [destroyedCoverIsPermeable](#property-destroyed-cover-is-permeable) | transition [invariant](#def-invariant) over [`combat.coverIntegrity`](#qnt-combat) and [`combat.coverBlocking`](#qnt-combat). |
| [validTraceObservation](#property-valid-trace-observation) | observation [invariant](#def-invariant) over [`last.lastAction`](#qnt-last) and [`last.traceRaw`](#qnt-last). |
| [suppressionRequiresDamage](#property-suppression-requires-damage) | observation [invariant](#def-invariant) over [`last.lastAction`](#qnt-last), [`last.damage`](#qnt-last), and [`last.suppressionDelta`](#qnt-last). |
| [factionNeutralCollateral](#property-faction-neutral-collateral) | catalogue-classified example comparing [nextConsequences](#qnt-next-consequences)`(initialCombat, alliedAttack)` with [nextConsequences](#qnt-next-consequences)`(initialCombat, representativeAttack)`; it is checked during simulation but is not state-variable reachability. |

Every transition [invariant](#def-invariant) therefore references [combat](#qnt-combat) or [last](#qnt-last) fields directly.
The catalogue cardinality check and faction-neutral example are labelled separately so their broader
command-line grouping cannot silently upgrade their logical kind.

<a id="chapter-37-what-sampled-runs-establish-and-what-exhaustive-"></a>
### 37. What sampled runs establish and what exhaustive checks add

Use simulation first for fast learning and trace variety:

```text
quint run sir-combat.qnt --main SirCombat --backend rust --seed 352 \
  --max-samples 64 --max-steps 8 --invariants boundedCombatState
```

This [sampled run](#def-sampled-run) establishes that 64 attempted executions at seed `352`, up to
eight steps each, exposed no [boundedCombatState](#property-bounded-combat-state) violation. It does not establish that no other seed,
choice sequence, or longer history violates the predicate.

Use the model checker when the question is universal inside an explicit finite search:

```text
quint verify sir-combat.qnt --main SirCombat --init init --step step \
  --invariant boundedCombatState --max-steps 8
```

A successful [bounded verification](#def-bounded-verification) establishes that the checker found no
violation among the represented paths through depth eight under that initializer and transition relation.
Record the backend and model revision too. Increasing the bound strengthens this result; it never turns
the bounded [combat](#qnt-combat) abstraction into proof about omitted geometry, production code, or every future rule.

Use named [runs](#def-run) for crisp boundary [witnesses](#def-witness), seeded simulation for diverse
debugging traces, and [bounded verification](#def-bounded-verification) for universal checked-state
questions. If either universal route finds one bad history, preserve its minimized
[counterexample](#def-counterexample), repair the literate authority, and repeat both the focused detector
and the broader qualification.

<a id="part-vii"></a>
## Part VII: Production correspondence

<a id="chapter-38-mapping-quint-records-and-operations-to-s-i-r-ru"></a>
### 38. Mapping Quint records and operations to S.I.R. runtime subjects

[Correspondence](#def-correspondence) is a checked relation, not shared authority. Use these status words consistently:
`exact` for a normalized field compared directly; `aggregate` when several model subjects meet one
production entry point; `external-contract` when only an identity/interface boundary is checked;
`presentation-only` for review metadata; and `missing` when no runtime comparison exists.

| Stable rule | Quint subject | Current F# subject | Evidence and scope | Status |
|---|---|---|---|---|
| [CONTENT-WEAPON-RIFLE-001](#rule-content-weapon-rifle-001) | [rifleDamageRaw](#qnt-rifle-damage-raw), [representativeAttack](#qnt-representative-attack) | `CombatRules.registry`; `QuintQ4ReplayFixtures.attackInput` | exact representative fixture and sampled attack inputs | exact |
| [CONTENT-BODY-HUMAN-001](#rule-content-body-human-001) | [humanArmorRetentionRaw](#qnt-human-armor-retention-raw) | `CombatRules.registry`; `attackInput.ArmorRetention` | exact representative/sample projection; not all body types | exact |
| [COMBAT-ENGAGEMENT-001](#rule-combat-engagement-001) | [preparationRaw](#qnt-preparation-raw) | `CombatRules.resolveAttack`; `AttackOutcome.Preparation` | [`last.preparationRaw`](#qnt-last) in exact and sampled replay | exact |
| [COMBAT-TRACE-002](#rule-combat-trace-002) | [traceAlgorithm](#qnt-trace-algorithm), [traceRaw](#qnt-trace-raw) | `FS.GG.Game.Core.Los.lineOfSightBy`; `CombatRules.resolveAttack` | fingerprint plus supplied visible/total count replay; geometry generation itself is not replayed | external-contract |
| [COMBAT-ARMOR-004](#rule-combat-armor-004) | [retainedEffect](#qnt-retained-effect) | `AttackInput.ArmorRetention`; `AttackOutcome.ArmorRetention` | [`last.retentionRaw`](#qnt-last) exact/sample comparison | exact |
| [COMBAT-DAMAGE-001](#rule-combat-damage-001) | [expectedDamageRaw](#qnt-expected-damage-raw), [roundedDamage](#qnt-rounded-damage), [damageForAttack](#qnt-damage-for-attack) | `SIR.Domain.FixedPoint`; `CombatRules.resolveAttack` | [`last.damage`](#qnt-last), Q4 arithmetic witnesses, and real interpreter replay | exact |
| [COMBAT-COLLISION-001](#rule-combat-collision-001) | `contact`, [coverObservation](#qnt-cover-observation) | `CombatRules.resolveCoverImpact`; completed attack outcomes | contact is derived from completed [damage](#stat-damage); cover collision uses the focused entry point | aggregate |
| [COMBAT-COVER-003](#rule-combat-cover-003) | [nextCoverImpact](#qnt-next-cover-impact), [resolveCoverImpact](#qnt-resolve-cover-impact) | `CombatRules.resolveCoverImpact` | integrity, [damage](#stat-damage), destroyed, and current-stop fields replayed | exact |
| [COMBAT-PENETRATION-001](#rule-combat-penetration-001) | [retainedEffect](#qnt-retained-effect), `retentionRaw` | `AttackInput.ArmorRetention`; `CombatRules.resolveAttack` | visible only inside completed attack input/outcome | aggregate |
| [COMBAT-HEALTH-001](#rule-combat-health-001) | [nextConsequences](#qnt-next-consequences), [bounded100](#qnt-bounded100) | `CombatRules.resolveConsequences`; `RemainingHealth` | [`combat.health`](#qnt-combat) exact/sample comparison | exact |
| [COMBAT-WOUND-001](#rule-combat-wound-001) | [woundForDamage](#qnt-wound-for-damage), `incapacitated` | `CombatRules.resolveConsequences`; `WoundSeverityCode`; `Incapacitated` | [`last.wound`](#qnt-last) and [`combat.incapacitated`](#qnt-combat) exact/sample comparison | exact |
| [COMBAT-SUPPRESSION-001](#rule-combat-suppression-001) | [suppressionForDamage](#qnt-suppression-for-damage), [nextConsequences](#qnt-next-consequences) | `CombatRules.resolveConsequences`; `SuppressionDelta`; `TotalSuppression` | observation and durable state compared together | exact |
| [COMBAT-SUPPRESSION-RECOVERY-001](#rule-combat-suppression-recovery-001) | [nextRecovery](#qnt-next-recovery), [resolveRecovery](#qnt-resolve-recovery) | `CombatRules.resolveRecovery` | recovery [action](#def-action)/state/observation replay | exact |
| [COMBAT-COLLATERAL-001](#rule-combat-collateral-001) | [alliedAttack](#qnt-allied-attack), [factionNeutralCollateral](#property-faction-neutral-collateral) | `QuintQ4ReplayFixtures.applyConsequences`; completed consequence resolver | factions are reflected and consequence equality is model-checked; production has no faction branch here | aggregate |
| [COMBAT-COVER-DESTRUCTION-001](#rule-combat-cover-destruction-001) | [nextCoverImpact](#qnt-next-cover-impact), [destroyedCoverIsPermeable](#property-destroyed-cover-is-permeable) | `CombatRules.resolveCoverImpact`; `Destroyed`; `StopsProjectile` | focused exact/sample cover replay | exact |
| [COMBAT-ATTACK-RESOLUTION-001](#rule-combat-attack-resolution-001) | [nextConsequences](#qnt-next-consequences), [resolveConsequences](#qnt-resolve-consequences), [Observation](#qnt-observation) | `CombatRules.resolveConsequences`; `QuintQ4ReplayFixtures.applyModelAction` | twenty normalized fields compared after every state | aggregate |

**Missing [correspondence](#def-correspondence) register.** The supercover geometry behind [COMBAT-TRACE-002](#rule-combat-trace-002) is deliberately
`missing`: Q4 accepts visible/[total samples](#stat-total-samples) and checks the pinned `lineOfSightBy` contract identity, but
does not replay the geometry algorithm. No other sixteen-rule row is silently missing. A future missing
row must use the `missing` status, name the absent comparison, and avoid production claims.

[Claim boundary](#def-claim-boundary): a green Quint [run](#def-run) establishes behavior of the extracted model. It does **not** by itself establish production [correspondence](#def-correspondence). The repository qualification separately runs exact and sampled traces through the real interpreter and independently mutates [action](#def-action) mapping, one ephemeral ITF expected observation, and an interpreter result to prove that boundary detects divergence. That evidence is scoped to its pinned source/tool identities and sampled traces; it is neither an exhaustive proof nor automatic equivalence for future changes.

<a id="chapter-39-literate-authority-and-deterministic-qnt-extract"></a>
### 39. Literate authority and deterministic .qnt extraction

`docs/rules/sir-combat.md` is the authored source. Its two additive
````text
```quint sir-combat.qnt +=
```
````
fences are concatenated in document order. `scripts/qualify-quint-q4-sir-combat.sh` extracts twice,
compares the bytes, typechecks the result with Quint `0.32.0`, and rejects stale projections, wrong [module](#def-module)
identity, or a changed external-contract fingerprint. The temporary `.qnt` file is a
[generated projection](#def-generated-projection): inspect it, execute it, and discard it; never edit it
as authority.

[ITF](#def-itf-trace) is the next [generated projection](#def-generated-projection). Quint serializes the variables [combat](#qnt-combat) and [last](#qnt-last); qualification
removes volatile timestamp/description metadata and fixes the source label before replay. The projection
therefore carries executable states, not a new rule definition. Recreate it whenever the literate source,
Quint version, [run](#def-run) seed, sample count, or step bound changes.

<a id="chapter-40-exact-and-sampled-itf-replay"></a>
### 40. Exact and sampled ITF replay

The committed exact corpus is `tests/fixtures/rules-corpus/quint-q4/trace_0.itf.json`: one named trace,
nine states. Run it through the real interpreter with:

```text
dotnet tests/SIR.Domain.Tests/bin/Release/net10.0/SIR.Domain.Tests.dll \
  --quint-q4-exact tests/fixtures/rules-corpus/quint-q4 1
```

`SIR-Q4-EXACT-ACCEPT: traces=1 states=9` means every normalized state field matched. It proves
[correspondence](#def-correspondence) for that exact committed trace only.

The sampled route uses Quint seed `352`, `16` traces, and at most `8` steps. The current deterministic
corpus contains `144` states and must include `attack:representative`. Qualification passes its temporary
directory to `--quint-q4-sampled <directory> 16`; acceptance reads
`SIR-Q4-SAMPLED-ACCEPT: traces=16 states=144`. This is sampled evidence: it does not cover another seed,
longer histories, omitted inputs, or the external supercover geometry.

For each ITF state, `QuintQ4ReplayFixtures.expectedState` normalizes model values,
`applyModelAction` invokes `CombatRules.resolveConsequences`, [resolveCoverImpact](#qnt-resolve-cover-impact), or
[resolveRecovery](#qnt-resolve-recovery), and `firstDifference` compares five durable `combat.*` fields plus fifteen `last.*`
observation fields in stable order. The adapter is [correspondence](#def-correspondence) code; it is not a second [combat](#qnt-combat)
interpreter because every transition delegates to production entry points.

<a id="chapter-41-first-divergence-reporting"></a>
### 41. First-divergence reporting

Stop at the earliest mismatch. Later differences may be consequences of the first one and are poorer
debugging evidence. The Q4 comparator reports:

```text
Q4 first divergence: fixture=<trace> pointer=/states/<n>/<field>
transition=<n> action=<lastAction> expected=<model> actual=<runtime>
adapter=tests/SIR.Conformance.Shared/QuintQ4ReplayFixtures.fs:applyModelAction
implementation=src/SIR.Simulation/CombatRules.fs:CombatRules mutation=<control>
```

Read it left to right: reproduce the fixture, open the JSON pointer, identify the [action](#def-action)/event at that
transition, compare the single field, then inspect the named adapter and production implementation.
Do not repair a later field first. For [`wrong-observable-field`](#def-first-divergence), the first changed projection is
[`last.traceRaw`](#qnt-last); for [`wrong-action-mapping`](#def-first-divergence), the event is deliberately routed with the wrong visible
sample count; for [`combat-boundary-defect`](#def-first-divergence), the production result is deliberately wrapped with an
invalid remaining [health](#stat-health). Each is an independent detector inversion, not evidence that production is
currently wrong.

<a id="chapter-42-observed-red-controls-and-restored-green-evidenc"></a>
### 42. Observed-red controls and restored-green evidence

For the representative spine, evidence is a three-step sequence, not a permanently broken example:

1. [Run](#def-run) the authority-derived [representativeDamageIsTwenty](#run-representative-damage-is-twenty) [witness](#def-witness) green.
2. Change only retention `8000 → 7000` in a disposable extraction and observe the named [witness](#def-witness) red because actual [damage](#stat-damage) becomes `18` while [expected damage](#stat-expected-damage) remains `20`.
3. Re-extract the untouched authority and [run](#def-run) the [witness](#def-witness) green again.

The audit records all three outcomes. The [mutation](#def-mutation) file is temporary and never becomes an authoring source. Chapters 32–37 extend this pattern across the M4 formal subjects.

Runtime [correspondence](#def-correspondence) then runs three independent ephemeral controls:

| Control | Changed seam | Required first-divergence evidence |
|---|---|---|
| [`wrong-action-mapping`](#def-first-divergence) | adapter chooses a zero-visible representative attack | transition/[action](#def-action) plus earliest differing consequence field |
| [`wrong-observable-field`](#def-first-divergence) | temporary sampled ITF adds one to expected [`last.traceRaw`](#qnt-last); adapter and runtime stay untouched | JSON pointer ending in [`last.traceRaw`](#qnt-last), expected, and actual |
| [`combat-boundary-defect`](#def-first-divergence) | wrapped production result reports invalid remaining [health](#stat-health) | JSON pointer ending in [`combat.health`](#qnt-combat), expected, and actual |

Every control must exit red and name both adapter and implementation. The untouched exact and sampled
commands then rerun green. A control that merely fails without a structured [first divergence](#def-first-divergence) is not
accepted evidence; a red left in an authored source is not restored-green evidence.

<a id="chapter-43-safely-changing-a-combat-rule"></a>
### 43. Safely changing a combat rule

Use this order; it keeps authority, projections, and claims from drifting apart:

1. Identify the stable rule and dependency cone in the registry and handbook [correspondence](#def-correspondence) map.
2. Change the owning domain/runtime authority and the literate Quint authority intentionally; do not edit generated `.qnt` or ITF.
3. Re-extract and typecheck the model; inspect the authority/generated byte identities.
4. Update named model witnesses/properties and [run](#def-run) their observed-red-before/pass-after evidence.
5. Regenerate exact fixtures only when their reviewed scenario changes; keep the old failure as review evidence.
6. Regenerate the deterministic sampled corpus with declared Quint version, seed, count, and bound.
7. Replay exact and sampled ITF through the real interpreter; fix the earliest [divergence](#def-first-divergence) rather than weakening the comparator.
8. Run all three [correspondence](#def-correspondence) inversions and restore untouched green.
9. Run the full Q4/runtime and repository documentation qualification; capture scoped receipts and claim limits.
10. Update [correspondence](#def-correspondence) statuses, source/tool identities, traceability, definitions, and the maintenance handoff.

If only one authority changes, [correspondence](#def-correspondence) should go red. That is the safety signal. Review the
semantic change; never make the adapter echo whichever side changed or describe a green simulation as
proof of implementation equivalence.

<a id="part-viii"></a>
## Part VIII: Reference

<a id="chapter-44-complete-rule-reference"></a>
### 44. Complete rule reference

Every row is transcribed from [ruleCatalogue](#qnt-rule-catalogue). “Visibility and verification” names
the smallest existing model surface through which a learner can inspect the rule. Reads/effects/events
are catalogue metadata; they are not additional model variables or actions.

| Rule | Kind; direct dependencies | Reads → effects; events | Visibility and verification | Granularity |
|---|---|---|---|---|
| [CONTENT-WEAPON-RIFLE-001](#rule-content-weapon-rifle-001) | fact; none | none → none; none | [rifleDamageRaw](#qnt-rifle-damage-raw); [representativeDamageIsTwenty](#run-representative-damage-is-twenty) | pure fact value |
| [CONTENT-BODY-HUMAN-001](#rule-content-body-human-001) | fact; none | none → none; none | [humanArmorRetentionRaw](#qnt-human-armor-retention-raw); catalogue/excerpt audit | pure fact value |
| [COMBAT-ENGAGEMENT-001](#rule-combat-engagement-001) | formula; none | `range` → none; none | [preparationRaw](#qnt-preparation-raw); representative preparation `13000` | pure formula |
| [COMBAT-TRACE-002](#rule-combat-trace-002) | algorithm; none | `visible`, `total` → none; none | [traceAlgorithm](#qnt-trace-algorithm), [validTrace](#qnt-valid-trace), [traceRaw](#qnt-trace-raw), [validTraceObservation](#property-valid-trace-observation) | [external algorithm contract](#def-external-algorithm-contract) plus pure ratio adapter |
| [COMBAT-ARMOR-004](#rule-combat-armor-004) | formula; none | `retention` → none; none | [retainedEffect](#qnt-retained-effect), representative `retentionRaw = 8000` | pure formula and completed observation |
| [COMBAT-DAMAGE-001](#rule-combat-damage-001) | formula; rifle fact, trace, armor | `baseDamage`, `trace`, `retention` → none; none | [expectedDamageRaw](#qnt-expected-damage-raw), [damageForAttack](#qnt-damage-for-attack), [representativeDamageIsTwenty](#run-representative-damage-is-twenty) | pure formula and completed observation |
| [COMBAT-COLLISION-001](#rule-combat-collision-001) | transition; trace | `trace.outcome`, `trace.crossings` → `projectile.contact`; `ContactResolved` | `contact`, [coverObservation](#qnt-cover-observation), [destroyingCoverConsumesCurrentCollision](#run-destroying-cover-consumes-current-collision) | participates in aggregate/cover observations; no invented collision state |
| [COMBAT-COVER-003](#rule-combat-cover-003) | transition; collision | `cover.integrity`, `cover.projectileBlocking` → `cover.integrity`; `CoverDamaged` | [coverDamage](#qnt-cover-damage), [nextCoverImpact](#qnt-next-cover-impact), [resolveCoverImpact](#qnt-resolve-cover-impact) | focused cover-impact [action](#def-action) |
| [COMBAT-PENETRATION-001](#rule-combat-penetration-001) | transition; cover, armor | `armor.rating`, [`weapon.penetration`](#concept-penetration) → [`damage.retention`](#stat-damage); `ArmorResolved` | [retainedEffect](#qnt-retained-effect), [damageForAttack](#qnt-damage-for-attack), `retentionRaw` | formula/observation inside atomic aggregate; no standalone [action](#def-action) |
| [COMBAT-HEALTH-001](#rule-combat-health-001) | transition; [damage](#stat-damage) | [`target.health`](#stat-health) → [`target.health`](#stat-health); `HealthChanged` | [nextConsequences](#qnt-next-consequences), [boundedCombatState](#property-bounded-combat-state) | state field in atomic aggregate |
| [COMBAT-WOUND-001](#rule-combat-wound-001) | transition; [health](#stat-health) | [`target.health`](#stat-health), [`damage`](#stat-damage) → `target.wounds`, `target.incapacitated`; `WoundApplied`, `Incapacitated` | [woundForDamage](#qnt-wound-for-damage), [woundThresholdsAreExact](#run-wound-thresholds-are-exact), [incapacityMatchesHealth](#property-incapacity-matches-health) | pure classification plus aggregate observation/state |
| [COMBAT-SUPPRESSION-001](#rule-combat-suppression-001) | transition; collision | [`target.suppression`](#stat-suppression), [`weapon.suppression`](#stat-suppression) → [`target.suppression`](#stat-suppression); `SuppressionChanged` | [suppressionForDamage](#qnt-suppression-for-damage), [suppressionRequiresDamage](#property-suppression-requires-damage) | state field in atomic aggregate |
| [COMBAT-SUPPRESSION-RECOVERY-001](#rule-combat-suppression-recovery-001) | transition; [suppression](#stat-suppression) | [`target.suppression`](#stat-suppression) → [`target.suppression`](#stat-suppression); `SuppressionChanged` | [recoveredSuppression](#qnt-recovered-suppression), [resolveRecovery](#qnt-resolve-recovery), [suppressionNeedsPositiveDamageAndRecoversFive](#run-suppression-needs-positive-damage-and-recovers-five) | focused recovery [action](#def-action) |
| [COMBAT-COLLATERAL-001](#rule-combat-collateral-001) | transition; collision | `target.faction`, `attacker.faction` → [`target.health`](#stat-health), [`target.suppression`](#stat-suppression); `AttackResolved` | [alliedAttack](#qnt-allied-attack), [factionNeutralCollateral](#property-faction-neutral-collateral), [collateralOutcomeIgnoresFaction](#run-collateral-outcome-ignores-faction) | aggregate input/observation and state comparison |
| [COMBAT-COVER-DESTRUCTION-001](#rule-combat-cover-destruction-001) | transition; cover | `cover.integrity` → `cover.projectileBlocking`; `CoverDestroyed` | [nextCoverImpact](#qnt-next-cover-impact), [coverObservation](#qnt-cover-observation), [destroyedCoverIsPermeable](#property-destroyed-cover-is-permeable) | focused cover-impact state and observation |
| [COMBAT-ATTACK-RESOLUTION-001](#rule-combat-attack-resolution-001) | transition; engagement, collision, cover, [penetration](#concept-penetration), [damage](#stat-damage), [wound](#concept-wound), [suppression](#stat-suppression), collateral | attacker/target/weapon/cover/armor state → cover, [health](#stat-health), [wound](#concept-wound), [incapacitation](#concept-incapacitation), [suppression](#stat-suppression); `AttackResolved`, `CoverDestroyed` | [nextConsequences](#qnt-next-consequences), [consequenceObservation](#qnt-consequence-observation), [resolveConsequences](#qnt-resolve-consequences), named runs/properties | one atomic completed consequence; cover/recovery remain separate entry points |

<a id="chapter-45-quint-declaration-reference"></a>
### 45. Quint declaration reference

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-46-traceability-matrix"></a>
### 46. Traceability matrix

Stable-rule rows are complete for M3 model/reference coverage. “Pending” remains only where the source
design explicitly assigns a decision or broad runtime replay claim to a later milestone; it is not a
claim of coverage.

| Source decision | Stable rule | Quint declaration | [Scenario/property](#def-property) | Runtime subject | Evidence | Coverage note |
|---|---|---|---|---|---|---|
| S.I.R. combat registry | [CONTENT-WEAPON-RIFLE-001](#rule-content-weapon-rifle-001) | [ruleCatalogue](#qnt-rule-catalogue), [rifleDamageRaw](#qnt-rifle-damage-raw) | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) | stable registry/weapon fact; see chapter 38 controlled [correspondence](#def-correspondence) status | `work/363-handbook-m3/audit-complete-rules.mjs`; focused M3 and full Q4 receipts | fact; no dependencies; [pure value](#def-pure-value) |
| S.I.R. combat registry | [CONTENT-BODY-HUMAN-001](#rule-content-body-human-001) | [ruleCatalogue](#qnt-rule-catalogue), [humanArmorRetentionRaw](#qnt-human-armor-retention-raw) | catalogue and exact-subject audit | stable registry/body fact; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 receipt | fact; no dependencies; [pure value](#def-pure-value) |
| S.I.R. combat registry | [COMBAT-ENGAGEMENT-001](#rule-combat-engagement-001) | [preparationRaw](#qnt-preparation-raw), [consequenceObservation](#qnt-consequence-observation) | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) | stable registry/range preparation; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | formula; pure preparation plus completed observation |
| S.I.R. combat registry | [COMBAT-TRACE-002](#rule-combat-trace-002) | [traceAlgorithm](#qnt-trace-algorithm), [validTrace](#qnt-valid-trace), [traceRaw](#qnt-trace-raw) | [validTraceObservation](#property-valid-trace-observation) | `FS.GG.Game.Core.Los.lineOfSightBy`; see chapter 38 external-contract/missing boundary | focused M3 and full Q4 receipts | [external algorithm contract](#def-external-algorithm-contract); ratio adapter only |
| S.I.R. combat registry | [COMBAT-ARMOR-004](#rule-combat-armor-004) | [retainedEffect](#qnt-retained-effect), [consequenceObservation](#qnt-consequence-observation) | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) | stable armor-retention subject; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | formula; bounded ratio and observation |
| S.I.R. combat registry | [COMBAT-DAMAGE-001](#rule-combat-damage-001) | [expectedDamageRaw](#qnt-expected-damage-raw), [roundedDamage](#qnt-rounded-damage), [damageForAttack](#qnt-damage-for-attack) | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) | `SIR.Domain.FixedPoint`, `CombatRules`; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | formula; pure calculation and completed observation |
| S.I.R. combat registry | [COMBAT-COLLISION-001](#rule-combat-collision-001) | `contact`, [coverObservation](#qnt-cover-observation) | [destroyingCoverConsumesCurrentCollision](#run-destroying-cover-consumes-current-collision) | stable collision subjects; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | transition participation; no extra collision state |
| S.I.R. combat registry | [COMBAT-COVER-003](#rule-combat-cover-003) | [coverDamage](#qnt-cover-damage), [nextCoverImpact](#qnt-next-cover-impact), [resolveCoverImpact](#qnt-resolve-cover-impact) | [destroyingCoverConsumesCurrentCollision](#run-destroying-cover-consumes-current-collision) | `CombatRules` cover-impact entry point; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | focused [action](#def-action) and observation |
| S.I.R. combat registry | [COMBAT-PENETRATION-001](#rule-combat-penetration-001) | [retainedEffect](#qnt-retained-effect), [damageForAttack](#qnt-damage-for-attack), `retentionRaw` | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) | stable [penetration](#concept-penetration)/armor subjects; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | formula/observation inside atomic aggregate |
| S.I.R. combat registry | [COMBAT-HEALTH-001](#rule-combat-health-001) | [nextConsequences](#qnt-next-consequences), [bounded100](#qnt-bounded100) | [boundedCombatState](#property-bounded-combat-state), [zeroHealthMeansIncapacitated](#run-zero-health-means-incapacitated) | `CombatRules` [health](#stat-health) consequence; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | atomic aggregate state field |
| S.I.R. combat registry | [COMBAT-WOUND-001](#rule-combat-wound-001) | [woundForDamage](#qnt-wound-for-damage), [consequenceObservation](#qnt-consequence-observation) | [woundThresholdsAreExact](#run-wound-thresholds-are-exact), [incapacityMatchesHealth](#property-incapacity-matches-health) | stable [wound](#concept-wound)/incapacity subjects; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | pure classification plus atomic observation/state |
| S.I.R. combat registry | [COMBAT-SUPPRESSION-001](#rule-combat-suppression-001) | [suppressionForDamage](#qnt-suppression-for-damage), [nextConsequences](#qnt-next-consequences) | [suppressionRequiresDamage](#property-suppression-requires-damage) | `CombatRules` [suppression](#stat-suppression) consequence; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | atomic aggregate state field |
| S.I.R. combat registry | [COMBAT-SUPPRESSION-RECOVERY-001](#rule-combat-suppression-recovery-001) | [recoveredSuppression](#qnt-recovered-suppression), [nextRecovery](#qnt-next-recovery), [resolveRecovery](#qnt-resolve-recovery) | [suppressionNeedsPositiveDamageAndRecoversFive](#run-suppression-needs-positive-damage-and-recovers-five) | `CombatRules` recovery entry point; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | focused recovery [action](#def-action) |
| S.I.R. combat registry | [COMBAT-COLLATERAL-001](#rule-combat-collateral-001) | [alliedAttack](#qnt-allied-attack), [nextConsequences](#qnt-next-consequences) | [factionNeutralCollateral](#property-faction-neutral-collateral), [collateralOutcomeIgnoresFaction](#run-collateral-outcome-ignores-faction) | stable faction/consequence subjects; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | aggregate input/observation and pure comparison |
| S.I.R. combat registry | [COMBAT-COVER-DESTRUCTION-001](#rule-combat-cover-destruction-001) | [nextCoverImpact](#qnt-next-cover-impact), [coverObservation](#qnt-cover-observation) | [destroyedCoverIsPermeable](#property-destroyed-cover-is-permeable), [destroyingCoverConsumesCurrentCollision](#run-destroying-cover-consumes-current-collision) | `CombatRules` destruction/blocking subjects; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | focused cover [action](#def-action); future permeability/current blocking |
| S.I.R. combat registry | [COMBAT-ATTACK-RESOLUTION-001](#rule-combat-attack-resolution-001) | [nextConsequences](#qnt-next-consequences), [consequenceObservation](#qnt-consequence-observation), [resolveConsequences](#qnt-resolve-consequences) | all consequence runs and state/observation properties | `CombatRules` completed consequence entry point; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | one atomic consequence; no invented intermediates |
| Q4 DEC-001 | all sixteen rules | [ruleCatalogue](#qnt-rule-catalogue) plus complete model | catalogue, named runs, and properties | `CombatRules.registry` plus mapped entry points | focused M3/M5 and full Q4 receipts | complete bounded sixteen-rule scope |
| Q4 DEC-002 | all sixteen rules | facts, formulae, external contract, helpers, focused and aggregate actions | subject-appropriate evidence | chapter 38 controlled statuses | focused M3/M4/M5 receipts | smallest faithful abstraction per layer |
| Q4 DEC-003 | compatibility subjects | model identity and deterministic projections | stale/identity negative controls | exact legacy subjects plus fingerprinted new projections | full Q4 receipt | byte-exact where named; semantic/fingerprint exact for new products |
| Q4 DEC-004 | [COMBAT-TRACE-002](#rule-combat-trace-002) | [traceAlgorithm](#qnt-trace-algorithm), [traceRaw](#qnt-trace-raw) | contract identity and ratio replay | `FS.GG.Game.Core.Los.lineOfSightBy` | Q4 contract checks; geometry [correspondence](#def-correspondence) classified missing | Quint does not copy supercover traversal |
| Q4 DEC-005 | aggregate/focused transition rules | [resolveConsequences](#qnt-resolve-consequences), [resolveCoverImpact](#qnt-resolve-cover-impact), [resolveRecovery](#qnt-resolve-recovery) | [action](#def-action) witnesses and ITF replay | corresponding `CombatRules` entry points | M4 reachability plus Q4/M5 replay | completed aggregate remains atomic |
| Q4 DEC-006 | [combat](#qnt-combat) records and actions | Q4 raw values, [wound](#concept-wound) variants, [CombatState](#qnt-combat-state), [AttackInput](#qnt-attack-input), [Observation](#qnt-observation) | exact/sample normalized fields | `FixedPoint`, `CombatRules`, `QuintQ4ReplayFixtures` | full Q4 and M5 focused receipts | explicit state/observation mapping |
| Q4 DEC-007 | — | standalone literate model | typecheck/[run](#def-run)/replay only | no canonical Typed SDD consumer-model adoption claim | issue `FS.GG.SDD#932`; Q4/M5 scoped receipts | standalone [correspondence](#def-correspondence) does not impersonate canonical migration |
| [Representative damage 20](#qnt-representative-attack) | [CONTENT-WEAPON-RIFLE-001](#rule-content-weapon-rifle-001), [COMBAT-TRACE-002](#rule-combat-trace-002), [COMBAT-ARMOR-004](#rule-combat-armor-004), [COMBAT-DAMAGE-001](#rule-combat-damage-001), [COMBAT-ATTACK-RESOLUTION-001](#rule-combat-attack-resolution-001) | [representativeAttack](#qnt-representative-attack), [expectedDamageRaw](#qnt-expected-damage-raw), [damageForAttack](#qnt-damage-for-attack), [resolveConsequences](#qnt-resolve-consequences) | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) | `SIR.Domain.FixedPoint`; `src/SIR.Simulation/CombatRules.fs`; `tests/SIR.Conformance.Shared/QuintQ4ReplayFixtures.fs` | `work/361-handbook-m2/audit-representative-attack.mjs`; `readiness/361-handbook-m2/handbook-m2.junit.xml`; `scripts/qualify-quint-q4-sir-combat.sh` | M2 model/excerpt/[mutation](#def-mutation) evidence plus bounded representative runtime [correspondence](#def-correspondence); chapter 38 [correspondence](#def-correspondence) is complete and scoped |
| [Wound boundary 24](#stat-wound-threshold) | [COMBAT-WOUND-001](#rule-combat-wound-001) | [woundForDamage](#qnt-wound-for-damage), [fullDamageAttack](#qnt-full-damage-attack) | [woundThresholdsAreExact](#run-wound-thresholds-are-exact) | stable [wound](#concept-wound) classification; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | `24` → [NoWound](#qnt-no-wound) |
| [Wound boundary 25](#stat-wound-threshold) | [COMBAT-WOUND-001](#rule-combat-wound-001) | [woundForDamage](#qnt-wound-for-damage), [fullDamageAttack](#qnt-full-damage-attack) | [woundThresholdsAreExact](#run-wound-thresholds-are-exact) | stable [wound](#concept-wound) classification; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | `25` → [MinorWound](#qnt-minor-wound) |
| [Wound boundary 50](#stat-wound-threshold) | [COMBAT-WOUND-001](#rule-combat-wound-001) | [woundForDamage](#qnt-wound-for-damage), [fullDamageAttack](#qnt-full-damage-attack) | [woundThresholdsAreExact](#run-wound-thresholds-are-exact) | stable [wound](#concept-wound) classification; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | `50` → [MajorWound](#qnt-major-wound) |
| [Zero-health incapacitation](#concept-incapacitation) | [COMBAT-HEALTH-001](#rule-combat-health-001), [COMBAT-WOUND-001](#rule-combat-wound-001) | [nextConsequences](#qnt-next-consequences) | [zeroHealthMeansIncapacitated](#run-zero-health-means-incapacitated), [incapacityMatchesHealth](#property-incapacity-matches-health) | stable [health](#stat-health)/incapacity subjects; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | zero and [incapacitation](#concept-incapacitation) appear in one successor |
| [Suppression eligibility](#concept-suppression-eligibility) | [COMBAT-SUPPRESSION-001](#rule-combat-suppression-001) | [suppressionForDamage](#qnt-suppression-for-damage) | [suppressionRequiresDamage](#property-suppression-requires-damage) | stable [suppression](#stat-suppression) consequence; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | [damage](#stat-damage) must be positive |
| [Five-point suppression recovery](#concept-suppression-recovery) | [COMBAT-SUPPRESSION-RECOVERY-001](#rule-combat-suppression-recovery-001) | [recoveredSuppression](#qnt-recovered-suppression), [resolveRecovery](#qnt-resolve-recovery) | [suppressionNeedsPositiveDamageAndRecoversFive](#run-suppression-needs-positive-damage-and-recovers-five) | recovery entry point; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | removes `min(5,current)` |
| Cover destruction | [COMBAT-COVER-DESTRUCTION-001](#rule-combat-cover-destruction-001) | [nextCoverImpact](#qnt-next-cover-impact), [coverObservation](#qnt-cover-observation) | [destroyingCoverConsumesCurrentCollision](#run-destroying-cover-consumes-current-collision) | destruction/blocking subjects; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | `destroyed` at zero integrity |
| Destroyed-cover permeability | [COMBAT-COVER-DESTRUCTION-001](#rule-combat-cover-destruction-001) | [nextCoverImpact](#qnt-next-cover-impact) | [destroyedCoverIsPermeable](#property-destroyed-cover-is-permeable) | future projectile blocking; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | zero integrity implies non-blocking |
| Current-collision blocking | [COMBAT-COLLISION-001](#rule-combat-collision-001), [COMBAT-COVER-003](#rule-combat-cover-003) | [coverObservation](#qnt-cover-observation) | [destroyingCoverConsumesCurrentCollision](#run-destroying-cover-consumes-current-collision) | current impact entry point; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | current direct blocking collision still stops |
| Faction-neutral collateral | [COMBAT-COLLATERAL-001](#rule-combat-collateral-001) | [alliedAttack](#qnt-allied-attack), [nextConsequences](#qnt-next-consequences) | [factionNeutralCollateral](#property-faction-neutral-collateral), [collateralOutcomeIgnoresFaction](#run-collateral-outcome-ignores-faction) | faction/consequence subjects; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | allied and hostile consequences match |
| Valid trace ratios | [COMBAT-TRACE-002](#rule-combat-trace-002) | [validTrace](#qnt-valid-trace), [traceRaw](#qnt-trace-raw) | [validTraceObservation](#property-valid-trace-observation) | bounded trace adapter; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | emitted ratio stays `0..10000` |
| External line-of-sight boundary | [COMBAT-TRACE-002](#rule-combat-trace-002) | [traceAlgorithm](#qnt-trace-algorithm) | exact contract/excerpt audit | `FS.GG.Game.Core.Los.lineOfSightBy`; behavior evidence remains external/M5 | focused M3 receipt | fingerprint and sample/result units; no copied supercover |
| Catalogue size and identity | all sixteen registry rules | [ruleCatalogue](#qnt-rule-catalogue) | [sixteenRulesDeclared](#property-sixteen-rules-declared) | stable registry metadata | focused M3 and full Q4 receipts | exactly sixteen unique stable IDs |
| Catalogue dependencies | all sixteen registry rules | [ruleCatalogue](#qnt-rule-catalogue) | focused dependency audit | stable registry dependency metadata | focused M3 receipt | every target declared; no self/duplicate dependency |
| Explanation order | eleven consequence participants | [consequenceExplanationOrder](#qnt-consequence-explanation-order), [consequenceObservation](#qnt-consequence-observation) | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) | stable completed-consequence explanation; see chapter 38 controlled [correspondence](#def-correspondence) status | focused M3 and full Q4 receipts | presentation metadata, not intermediate states |
| [Exact runtime replay correspondence](#def-correspondence) | all replayed transition rules | normalized [combat](#qnt-combat) and [last](#qnt-last) ITF | one committed trace, nine states | `QuintQ4ReplayFixtures.replayDirectory`; real `CombatRules` entry points | `scripts/qualify-quint-q4-sir-combat.sh`; M5 focused receipt | exact only for the named committed fixture |
| [Sampled runtime replay correspondence](#def-correspondence) | all actions reached by the deterministic sample | normalized [combat](#qnt-combat) and [last](#qnt-last) ITF | seed 352; 16 traces; 144 states; max 8 steps | `QuintQ4ReplayFixtures.replayDirectory`; real `CombatRules` entry points | `scripts/qualify-quint-q4-sir-combat.sh`; M5 focused receipt | sampled, deterministic, and explicitly non-exhaustive |

<a id="chapter-47-command-reference"></a>
### 47. Command reference

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-48-known-limits-and-future-experiments"></a>
### 48. Known limits and future experiments

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-49-exercises-and-solutions"></a>
### 49. Exercises and solutions

These exercises use the positive authority as written. Chapters 34 and 35 now teach deliberate
semantic defects, minimized [counterexamples](#def-counterexample), and restored-green evidence.

#### Beginner — predict one helper or completed successor

1. Predict [woundForDamage](#qnt-wound-for-damage) for `24`, `25`, and `50`.
2. Predict [recoveredSuppression](#qnt-recovered-suppression) for `0`, `3`, and `12`.
3. Starting from [initialCombat](#qnt-initial-combat), predict [health](#stat-health) and [suppression](#stat-suppression) after
   [missedAttack](#qnt-missed-attack).
4. A direct projectile destroys blocking cover. Does the current projectile stop? Does the next one
   encounter blocking cover?

**Beginner solutions.** The [wound](#concept-wound) results are no/minor/major. Recovery amounts are `0`, `3`, and `5`.
The miss leaves [health](#stat-health) `100` and [suppression](#stat-suppression) `0`. The current projectile stops because the collision
already occurred; [destroyed cover](#concept-destroyed-cover) becomes non-blocking for future projectiles.

#### Intermediate — read dependencies and observations

1. Starting at [COMBAT-DAMAGE-001](#rule-combat-damage-001), walk backward through every direct
   dependency until reaching rules with none.
2. Explain why [COMBAT-PENETRATION-001](#rule-combat-penetration-001) has no standalone [action](#def-action) and name
   three existing subjects that make it visible.
3. In [suppressionNeedsPositiveDamageAndRecoversFive](#run-suppression-needs-positive-damage-and-recovers-five),
   separate the three completed transitions and reconcile each observation delta with durable state.
4. Compare the dependency graph with [consequenceExplanationOrder](#qnt-consequence-explanation-order).
   Name one reason the two orders must not be treated as identical.

**Intermediate solutions.** Damage depends on the rifle fact, trace, and armor; those three have no
further dependencies. Penetration is embedded in the completed consequence and is visible through
[retainedEffect](#qnt-retained-effect), [damageForAttack](#qnt-damage-for-attack), and
`retentionRaw`. The [suppression](#stat-suppression) [run](#def-run) performs miss, damaging hit, then recovery, producing state values
`0`, `12`, and `7`. Dependencies express semantic reliance; [explanation order](#concept-explanation-order) is stable presentation
metadata and may include participating rules in a reviewer-friendly order.

#### Advanced — design within the authority boundary

1. Describe a positive Quint [run](#def-run) for a half-visible 5/10 trace without adding state. State
   the expected raw trace and the helper/[action](#def-action)/[property](#def-property) subjects you would use.
2. A reviewer asks for `resolvePenetration`, `resolveWound`, and `resolveIncapacitation` actions. Explain
   why adding them would be dishonest, and propose a helper/observation/[property](#def-property) review route instead.
3. Design a catalogue query that checks every dependency points at a declared rule. Explain what such
   an example establishes and what exhaustive verification would add; compare it with the bounded
   catalogue-integrity [mutation](#def-mutation) in chapter 35 and the claim limits in chapter 37.
4. Classify the claim “the 10/10 trace [run](#def-run) proves supercover is correct.” Identify the authority
   boundary and rewrite the claim accurately.

**Advanced solutions.** A 5/10 trace yields raw `5000`; reuse [traceRaw](#qnt-trace-raw), a valid
[AttackInput](#qnt-attack-input), [resolveConsequences](#qnt-resolve-consequences), and
[validTraceObservation](#property-valid-trace-observation). Penetration, [wound](#concept-wound), and incapacity are
parts of one production-visible aggregate consequence, so pure helpers, completed observation fields,
and state properties are the honest review surfaces. A catalogue query over this finite [constant](#def-constant) [set](#def-set)
can [witness](#def-witness) the current declared graph; an [exhaustive check](#def-exhaustive-check) would establish the [property](#def-property) over the
chosen model bounds rather than one example. Finally, the 10/10 [run](#def-run) proves only that supplied counts
map to ratio and consequence correctly; the external `FS.GG.Game.Core.Los.lineOfSightBy` evidence owns
whether supercover produced those counts.

<a id="chapter-50-alphabetical-definition-index"></a>
### 50. Alphabetical definition index

The index reconciles 188 canonical entries: the original M0 address inventory plus three declarations
that entered the literate authority before M6 (`UINT32_RANGE`, `wrapInt32`, and
`damageRoundingPreservesInt32Wrap`). Every entry now supplies a definition, declaration locus, related
canonical links, and a scoped runtime-correspondence statement. Five common aliases are recorded in the
schema-v2 vocabulary manifest and surfaced at their canonical entries; aliases remain search aids, not
parallel definitions.

The documentation build runs a dependency-free Markdown block/inline AST audit before rendering. It
reconciles all literate Quint declarations, all sixteen rule IDs, all fifty chapters, the three reading
paths, aliases, anchors, fragment links, and eligible controlled prose. Occurrence exemptions are
manifest-declared: front matter, fenced code, headings, inline code, this canonical index, aliases used
only as index search aids, and the ambiguous model names `combat`, `last`, and `step`, whose ordinary-English
uses cannot identify a model symbol reliably. Ten isolated mutations prove missing fragments, duplicate
anchors, absent index entries, unlinked eligible prose and model symbols, wrong canonical targets,
insubstantial definitions, missing authoritative declarations, rule-ID drift across model/runtime, and
manifest/index alias drift each observe red before untouched input restores green.

<a id="qnt-absolute"></a>
**absolute** — function. Returns the non-negative magnitude of an integer and supports sign-aware round-half-away-from-zero arithmetic. **Declared at:** literate model `SirCombat.absolute`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-action"></a>
**action** — keyword. A Quint declaration whose guarded execution updates one or more primed state variables atomically. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-add-fixed"></a>
**addFixed** — function. Adds two raw fixed-point integers and clamps the mathematical result to the signed 32-bit range. **Declared at:** literate model `SirCombat.addFixed`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="concept-aggregate-attack-resolution"></a>
**aggregate attack resolution** — concept. One atomic transition that computes and publishes the completed damage, health, wound, incapacity, suppression, and explanation consequences of an attack. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-algorithm-entry"></a>
**AlgorithmEntry** — type. Record describing the registered external trace algorithm by stable ID, version, input and output units, tie-break rule, and source fingerprint. **Declared at:** literate model `SirCombat.AlgorithmEntry`. **Related terms:** [record](#def-record), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-allied-attack"></a>
**alliedAttack** — value. Attack fixture identical to the representative attack except that source and target factions match, used to test faction-neutral consequences. **Declared at:** literate model `SirCombat.alliedAttack`. **Related terms:** [pure value](#def-pure-value), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="stat-armor-retention"></a>
**armor retention** — stat. Fraction of traced damage retained after armor, clamped to raw `0..10000`; the representative attack uses `8000` (`0.8`). **Declared at:** `AttackInput.armorRetentionRaw`. **Related terms:** [retainedEffect](#qnt-retained-effect), [expected damage](#stat-expected-damage). **Runtime correspondence:** fixed-point armor/damage handling in `CombatRules`.

<a id="qnt-attack-input"></a>
**AttackInput** — type. Immutable bounded inputs for one attempted attack: target validity, raw damage/retention, trace samples, range, suppression, collision flags, factions, and event identity. **Declared at:** `SirCombat.AttackInput`. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation), [validAttack](#qnt-valid-attack). **Runtime correspondence:** inputs consumed by `CombatRules`; M5 completes the map.

<a id="stat-base-damage"></a>
**base damage** — stat. Weapon damage before trace and armor retention; the representative rifle value is `25`, encoded as raw `250000`. **Declared at:** `rifleDamageRaw` and `AttackInput.baseDamageRaw`. **Related terms:** [expected damage](#stat-expected-damage), [scale 10,000](#unit-scale-10-000). **Runtime correspondence:** rifle fact consumed by `CombatRules`.

<a id="def-bounded-verification"></a>
**bounded verification** — evidence. Exhaustive exploration of every behavior inside explicitly chosen finite bounds; it proves only the bounded state space. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [claim boundary](#def-claim-boundary), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-bounded100"></a>
**bounded100** — function. Clamps an integer to the inclusive 0–100 range used by health, suppression, and cover integrity. **Declared at:** literate model `SirCombat.bounded100`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="property-bounded-combat-state"></a>
**boundedCombatState** — property. State predicate requiring health, suppression, and cover integrity each to remain within their inclusive 0–100 bounds. **Declared at:** literate model `SirCombat.boundedCombatState`. **Related terms:** [property](#def-property), [bounded verification](#def-bounded-verification). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="catalogue-property-bounded-combat-state"></a>
**BoundedCombatState** — catalogue property. The catalogue identity for the `boundedCombatState` model property, including its declared subjects. **Declared at:** `SirCombat.propertyCatalogue`. **Related terms:** [propertyCatalogue](#qnt-property-catalogue), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="unit-cells"></a>
**cells** — unit. Discrete map-distance units used by engagement preparation and the external trace contract. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [scale 10,000](#unit-scale-10-000), [AttackInput](#qnt-attack-input). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-claim-boundary"></a>
**claim boundary** — evidence. Explicit limit on what an execution or receipt establishes; a green Quint witness establishes model behavior, while runtime correspondence requires separate interpreter replay evidence. **Declared at:** handbook chapters 14 and 38. **Related terms:** [correspondence](#def-correspondence), [witness](#def-witness). **Runtime correspondence:** enforced by exact/sampled Q4 replay and independent divergence mutations.

<a id="concept-collateral-consequence"></a>
**collateral consequence** — concept. The ordinary completed combat consequence of an attack whose source and target share a faction; it is not silently suppressed. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="run-collateral-outcome-ignores-faction"></a>
**collateralOutcomeIgnoresFaction** — run. Executable witness that allied and opposing inputs with equal physical fields produce equal damage and suppression consequences. **Declared at:** literate model `SirCombatTests.collateralOutcomeIgnoresFaction`. **Related terms:** [run](#def-run), [witness](#def-witness). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-combat"></a>
**combat** — variable. Durable `CombatState` variable updated atomically by consequence, cover-impact, and recovery actions. **Declared at:** literate model `SirCombat.combat`. **Related terms:** [state variable](#def-state-variable), [CombatState](#qnt-combat-state). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="rule-combat-armor-004"></a>
**COMBAT-ARMOR-004** — rule. Formula that bounds the retained effect after armor. **Declared at:** `ruleCatalogue`; `retainedEffect`. **Related terms:** [armor retention](#stat-armor-retention), [penetration](#concept-penetration). **Runtime correspondence:** stable armor-retention subject; see the M5 controlled correspondence map.

<a id="rule-combat-attack-resolution-001"></a>
**COMBAT-ATTACK-RESOLUTION-001** — rule. Atomic transition that publishes completed health, wound/incapacity, suppression, and explanation consequences. **Declared at:** `ruleCatalogue`; `resolveConsequences`. **Related terms:** [aggregate attack resolution](#concept-aggregate-attack-resolution), [Observation](#qnt-observation). **Runtime correspondence:** completed `CombatRules` consequence entry point; see the M5 controlled correspondence map.

<a id="rule-combat-collateral-001"></a>
**COMBAT-COLLATERAL-001** — rule. Transition semantics that apply the same bounded damage and suppression consequences regardless of faction. **Declared at:** `ruleCatalogue`; `alliedAttack`; `factionNeutralCollateral`. **Related terms:** [collateral consequence](#concept-collateral-consequence), [faction-neutral consequence](#concept-faction-neutral-consequence). **Runtime correspondence:** stable faction/consequence subjects; see the M5 controlled correspondence map.

<a id="rule-combat-collision-001"></a>
**COMBAT-COLLISION-001** — rule. Transition participation that relates a trace outcome/current crossing to projectile contact. **Declared at:** `ruleCatalogue`; consequence/cover observations. **Related terms:** [projectile contact](#concept-projectile-contact), [current collision consumption](#concept-current-collision-consumption). **Runtime correspondence:** stable collision subjects; see the M5 controlled correspondence map.

<a id="rule-combat-cover-003"></a>
**COMBAT-COVER-003** — rule. Focused cover-impact transition that reduces integrity and explains current projectile blocking. **Declared at:** `ruleCatalogue`; `nextCoverImpact`; `resolveCoverImpact`. **Related terms:** [cover integrity](#stat-cover-integrity), [cover blocking](#concept-cover-blocking). **Runtime correspondence:** `CombatRules` cover-impact entry point; see the M5 controlled correspondence map.

<a id="rule-combat-cover-destruction-001"></a>
**COMBAT-COVER-DESTRUCTION-001** — rule. Transition that makes zero-integrity cover non-blocking for future projectiles while preserving the destroying collision's stop result. **Declared at:** `ruleCatalogue`; `nextCoverImpact`; `coverObservation`. **Related terms:** [destroyed cover](#concept-destroyed-cover), [current collision consumption](#concept-current-collision-consumption). **Runtime correspondence:** stable destruction/blocking subjects; see the M5 controlled correspondence map.

<a id="rule-combat-damage-001"></a>
**COMBAT-DAMAGE-001** — rule. Formula composing base damage, trace ratio, retained effect, and whole-point rounding. **Declared at:** `ruleCatalogue`; `expectedDamageRaw`; `damageForAttack`. **Related terms:** [expected damage](#stat-expected-damage), [Q4 raw integer](#unit-q4-raw-integer). **Runtime correspondence:** `SIR.Domain.FixedPoint` and `CombatRules`; see the M5 controlled correspondence map.

<a id="rule-combat-engagement-001"></a>
**COMBAT-ENGAGEMENT-001** — rule. Formula deriving preparation time from range cells. **Declared at:** `ruleCatalogue`; `preparationRaw`. **Related terms:** [preparation time](#stat-preparation-time), [range cells](#stat-range-cells). **Runtime correspondence:** stable range-preparation subject; see the M5 controlled correspondence map.

<a id="rule-combat-health-001"></a>
**COMBAT-HEALTH-001** — rule. Atomic consequence that subtracts damage and bounds health to 0–100. **Declared at:** `ruleCatalogue`; `nextConsequences`. **Related terms:** [health](#stat-health), [incapacitation](#concept-incapacitation). **Runtime correspondence:** `CombatRules` health consequence; see the M5 controlled correspondence map.

<a id="rule-combat-penetration-001"></a>
**COMBAT-PENETRATION-001** — rule. Transition semantics represented at retained-effect formula and completed-observation granularity, not as a standalone action. **Declared at:** `ruleCatalogue`; `retainedEffect`; `damageForAttack`; `Observation.retentionRaw`. **Related terms:** [penetration](#concept-penetration), [armor retention](#stat-armor-retention). **Runtime correspondence:** stable armor/penetration subjects; see the M5 controlled correspondence map.

<a id="rule-combat-suppression-001"></a>
**COMBAT-SUPPRESSION-001** — rule. Atomic consequence that applies a non-negative requested suppression delta only when damage is positive. **Declared at:** `ruleCatalogue`; `suppressionForDamage`; `nextConsequences`. **Related terms:** [suppression eligibility](#concept-suppression-eligibility), [suppression delta](#stat-suppression-delta). **Runtime correspondence:** `CombatRules` suppression consequence; see the M5 controlled correspondence map.

<a id="rule-combat-suppression-recovery-001"></a>
**COMBAT-SUPPRESSION-RECOVERY-001** — rule. Focused transition that removes up to five suppression points. **Declared at:** `ruleCatalogue`; `recoveredSuppression`; `resolveRecovery`. **Related terms:** [suppression recovery](#concept-suppression-recovery), [suppression](#stat-suppression). **Runtime correspondence:** `CombatRules` recovery entry point; see the M5 controlled correspondence map.

<a id="rule-combat-trace-002"></a>
**COMBAT-TRACE-002** — rule. External algorithm contract relating valid visible/total samples to a fixed-point trace ratio. **Declared at:** `ruleCatalogue`; `traceAlgorithm`; `validTrace`; `traceRaw`. **Related terms:** [physical shot trace](#concept-physical-shot-trace), [external algorithm contract](#def-external-algorithm-contract). **Runtime correspondence:** `FS.GG.Game.Core.Los.lineOfSightBy`; see the M5 controlled correspondence map.

<a id="rule-combat-wound-001"></a>
**COMBAT-WOUND-001** — rule. Transition semantics classifying 24/25/50 damage boundaries and deriving incapacitation from successor health. **Declared at:** `ruleCatalogue`; `woundForDamage`; `consequenceObservation`; `nextConsequences`. **Related terms:** [wound](#concept-wound), [wound threshold](#stat-wound-threshold). **Runtime correspondence:** stable wound/incapacity subjects; see the M5 controlled correspondence map.

<a id="qnt-combat-state"></a>
**CombatState** — type. Cohesive durable combat state containing health, suppression, cover integrity/blocking, and incapacitation. **Declared at:** `SirCombat.CombatState`; initialized by `initialCombat`. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation), [nextConsequences](#qnt-next-consequences). **Runtime correspondence:** completed state consequences produced by `CombatRules`.

<a id="qnt-consequence-explanation-order"></a>
**consequenceExplanationOrder** — value. Ordered stable rule-ID list used when a completed attack observation explains participating consequence rules. **Declared at:** literate model `SirCombat.consequenceExplanationOrder`. **Related terms:** [pure value](#def-pure-value), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-consequence-observation"></a>
**consequenceObservation** — function. Builds the completed immutable attack observation from an input, including damage, trace, retention, wound, suppression, event identity, and explanation order. **Declared at:** literate model `SirCombat.consequenceObservation`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-constant"></a>
**constant** — keyword. A Quint name bound once by the model and unavailable for transition-time reassignment. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="rule-content-body-human-001"></a>
**CONTENT-BODY-HUMAN-001** — rule. Human-body content fact represented by full raw armor retention. **Declared at:** `ruleCatalogue`; `humanArmorRetentionRaw`. **Related terms:** [armor retention](#stat-armor-retention), [scale 10,000](#unit-scale-10-000). **Runtime correspondence:** stable body fact; see the M5 controlled correspondence map.

<a id="rule-content-weapon-rifle-001"></a>
**CONTENT-WEAPON-RIFLE-001** — rule. Rifle content fact represented as raw base damage `250000`, or 25 points. **Declared at:** `ruleCatalogue`; `rifleDamageRaw`. **Related terms:** [base damage](#stat-base-damage), [representativeAttack](#qnt-representative-attack). **Runtime correspondence:** stable rifle fact; see the M5 controlled correspondence map.

<a id="def-correspondence"></a>
**correspondence** — evidence. Checked agreement between model observations and production interpreter outcomes under explicitly identified traces, mappings, versions, and subjects; it is separate from model execution. **Declared at:** chapter 38 and Q4 replay receipts. **Related terms:** [claim boundary](#def-claim-boundary), [execution trace](#def-execution-trace). **Runtime correspondence:** `QuintQ4ReplayFixtures` compares exact and sampled traces with `CombatRules` and reports first divergence.

<a id="def-counterexample"></a>
**counterexample** — evidence. A concrete state/action path returned when a checked property is false inside the explored boundary. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [claim boundary](#def-claim-boundary), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="concept-cover-blocking"></a>
**cover blocking** — concept. Whether intact cover stops the current or a later projectile, kept distinct from cover integrity. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="stat-cover-damage"></a>
**cover damage** — stat. The bounded integrity loss applied to cover by one impact, with a minimum of one point. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="stat-cover-integrity"></a>
**cover integrity** — stat. A bounded 0–100 durability value; reaching zero makes cover non-blocking for future projectiles. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Aliases:** `cover HP`. **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-cover-damage"></a>
**coverDamage** — function. Converts base damage to cover-integrity loss by integer halving with a minimum result of one. **Declared at:** literate model `SirCombat.coverDamage`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-cover-observation"></a>
**coverObservation** — function. Builds the completed cover-impact observation, preserving current-collision blocking separately from successor cover permeability. **Declared at:** literate model `SirCombat.coverObservation`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="concept-current-collision-consumption"></a>
**current-collision consumption** — concept. The rule that a projectile which destroys cover is still stopped by that same collision even though later projectiles may pass. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="stat-damage"></a>
**damage** — stat. Whole combat harm after trace, retention, fixed-point composition, and final rounding. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="unit-damage-points"></a>
**damage points** — unit. Whole-number units used for weapon output and completed attack harm. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [scale 10,000](#unit-scale-10-000), [AttackInput](#qnt-attack-input). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-damage-for-attack"></a>
**damageForAttack** — function. Composes trace ratio, retained effect, expected raw damage, and final whole-point rounding for one valid input. **Declared at:** `SirCombat.damageForAttack`. **Related terms:** [traceRaw](#qnt-trace-raw), [expectedDamageRaw](#qnt-expected-damage-raw), [roundedDamage](#qnt-rounded-damage). **Runtime correspondence:** representative damage calculation in `CombatRules`.

<a id="run-damage-rounding-preserves-int32-wrap"></a>
**damageRoundingPreservesInt32Wrap** — run. Executable edge witness that final damage rounding performs the specified signed-int32 wrap before division rather than saturating that addition. **Declared at:** literate model `SirCombatTests.damageRoundingPreservesInt32Wrap`. **Related terms:** [run](#def-run), [witness](#def-witness). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="concept-destroyed-cover"></a>
**destroyed cover** — concept. Cover whose integrity is zero and whose future blocking flag is therefore false. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="property-destroyed-cover-is-permeable"></a>
**destroyedCoverIsPermeable** — property. State property requiring zero-integrity cover to have `coverBlocking = false` for later projectiles. **Declared at:** literate model `SirCombat.destroyedCoverIsPermeable`. **Related terms:** [property](#def-property), [bounded verification](#def-bounded-verification). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="catalogue-property-destroyed-cover-is-permeable"></a>
**DestroyedCoverIsPermeable** — catalogue property. The catalogue identity for the `destroyedCoverIsPermeable` model property, including its declared subjects. **Declared at:** `SirCombat.propertyCatalogue`. **Related terms:** [propertyCatalogue](#qnt-property-catalogue), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="run-destroying-cover-consumes-current-collision"></a>
**destroyingCoverConsumesCurrentCollision** — run. Executable witness that a destroying direct hit still reports the current projectile blocked while successor cover becomes permeable. **Declared at:** literate model `SirCombatTests.destroyingCoverConsumesCurrentCollision`. **Related terms:** [run](#def-run), [witness](#def-witness). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-divide-rounded-away-from-zero"></a>
**divideRoundedAwayFromZero** — function. Divides two integers and moves an exact or larger half remainder one whole step away from zero. **Declared at:** literate model `SirCombat.divideRoundedAwayFromZero`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="concept-event-identity"></a>
**event identity** — concept. The stable identifier carried through an input and observation so replay can match one completed event. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-execution-trace"></a>
**execution trace** — evidence. Ordered model states connected by named actions; read the action first, then reconcile input reflection, explanation fields, changed state, and unchanged state. **Declared at:** Quint run output and ITF traces. **Related terms:** [Observation](#qnt-observation), [correspondence](#def-correspondence). **Runtime correspondence:** exact/sampled traces are replayed separately through the real interpreter.

<a id="def-exhaustive-check"></a>
**exhaustive check** — evidence. A property evaluation over every reachable state within declared finite bounds, unlike a sampled execution. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [claim boundary](#def-claim-boundary), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="stat-expected-damage"></a>
**expected damage** — stat. Raw base damage multiplied by trace probability and clamped armor retention before conversion to whole damage points; representative raw value `200000` becomes `20`. **Declared at:** `expectedDamageRaw`. **Related terms:** [base damage](#stat-base-damage), [trace probability](#stat-trace-probability), [armor retention](#stat-armor-retention). **Runtime correspondence:** fixed-point damage path in `CombatRules`.

<a id="qnt-expected-damage-raw"></a>
**expectedDamageRaw** — function. Performs the two scale-preserving fixed multiplications `base × trace × retainedEffect(retention)`. **Declared at:** `SirCombat.expectedDamageRaw`. **Related terms:** [multiplyFixed](#qnt-multiply-fixed), [retainedEffect](#qnt-retained-effect), [roundedDamage](#qnt-rounded-damage). **Runtime correspondence:** model-side counterpart of fixed-point damage composition.

<a id="concept-explanation-order"></a>
**explanation order** — concept. The deterministic ordered rule-ID list explaining which stable rules contributed to an observation. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-external-algorithm-contract"></a>
**external algorithm contract** — evidence. A modeled input/output boundary whose implementation is owned outside this Quint model and needs separate evidence. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [claim boundary](#def-claim-boundary), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="concept-faction-neutral-consequence"></a>
**faction-neutral consequence** — concept. The rule that allied and opposing attacks with equal physical inputs receive equal damage and suppression treatment. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Aliases:** `friendly fire`. **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="property-faction-neutral-collateral"></a>
**factionNeutralCollateral** — property. Property equating successor consequences for allied and opposing attacks whose physical inputs are otherwise identical. **Declared at:** literate model `SirCombat.factionNeutralCollateral`. **Related terms:** [property](#def-property), [bounded verification](#def-bounded-verification). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="catalogue-property-faction-neutral-collateral"></a>
**FactionNeutralCollateral** — catalogue property. The catalogue identity for the `factionNeutralCollateral` model property, including its declared subjects. **Declared at:** `SirCombat.propertyCatalogue`. **Related terms:** [propertyCatalogue](#qnt-property-catalogue), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="concept-first-collision"></a>
**first collision** — concept. The earliest blocking contact selected by the registered trace implementation for projectile resolution. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-first-divergence"></a>
**first divergence** — evidence. Earliest ordered model/runtime field mismatch, reported with fixture, JSON pointer, transition, action, expected/actual values, adapter, and implementation. **Declared at:** chapter 41; `QuintQ4ReplayFixtures.firstDifference`. **Related terms:** [ITF trace](#def-itf-trace), [correspondence](#def-correspondence). **Runtime correspondence:** three Q4 inversion controls must report it before untouched replay restores green.

<a id="unit-fixed-point-ratio"></a>
**fixed-point ratio** — unit. A dimensionless ratio encoded as an integer whose denominator is `SCALE` (10,000). **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [scale 10,000](#unit-scale-10-000), [AttackInput](#qnt-attack-input). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-from-ratio"></a>
**fromRatio** — function. Converts an integer numerator/denominator pair to scale-10,000 fixed point with round-half-away-from-zero and signed-int32 saturation. **Declared at:** literate model `SirCombat.fromRatio`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-full-damage-attack"></a>
**fullDamageAttack** — function. Constructs a valid unobstructed `AttackInput` whose raw base damage is the supplied whole damage at full trace and retention. **Declared at:** literate model `SirCombat.fullDamageAttack`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-generated-projection"></a>
**generated projection** — evidence. Deterministic disposable output derived from an authored authority for tools to consume; it must be regenerated, never edited as authority. **Declared at:** chapter 39; `scripts/qualify-quint-q4-sir-combat.sh`. **Related terms:** [claim boundary](#def-claim-boundary), [ITF trace](#def-itf-trace). **Runtime correspondence:** extracted `.qnt` and normalized ITF are mechanically checked inputs to replay.

<a id="def-guard"></a>
**guard** — keyword. A Boolean precondition that must hold before a Quint action may participate in a transition. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="stat-health"></a>
**health** — stat. A bounded 0–100 durable combat value reduced by completed damage. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="unit-hit-points"></a>
**hit points** — unit. Whole-number units used for actor health; zero means incapacitated in this bounded model. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [scale 10,000](#unit-scale-10-000), [AttackInput](#qnt-attack-input). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="stat-hp"></a>
**HP** — stat. A conventional abbreviation for hit points and the handbook's canonical health stat. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-human-armor-retention-raw"></a>
**humanArmorRetentionRaw** — value. Human-body armor-retention fact encoded at full scale (`10000`, or 1.0) for the bounded representative corpus. **Declared at:** literate model `SirCombat.humanArmorRetentionRaw`. **Related terms:** [pure value](#def-pure-value), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-import"></a>
**import** — keyword. A Quint declaration that brings names from another module into the current module's scope. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="concept-incapacitation"></a>
**incapacitation** — concept. The durable state derived exactly from successor health being zero. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="property-incapacity-matches-health"></a>
**incapacityMatchesHealth** — property. State property requiring `incapacitated` to be true exactly when current health is zero. **Declared at:** literate model `SirCombat.incapacityMatchesHealth`. **Related terms:** [property](#def-property), [bounded verification](#def-bounded-verification). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="catalogue-property-incapacity-matches-health"></a>
**IncapacityMatchesHealth** — catalogue property. The catalogue identity for the `incapacityMatchesHealth` model property, including its declared subjects. **Declared at:** `SirCombat.propertyCatalogue`. **Related terms:** [propertyCatalogue](#qnt-property-catalogue), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-init"></a>
**init** — action. Initialization action assigning both durable combat state and the complete neutral `Initialize` observation. **Declared at:** literate model `SirCombat.init`. **Related terms:** [state transition](#def-state-transition), [CombatState](#qnt-combat-state). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-initial-combat"></a>
**initialCombat** — value. Fully specified starting state: health 100, suppression 0, cover integrity 100, blocking true, incapacitated false. **Declared at:** `SirCombat.initialCombat`. **Related terms:** [CombatState](#qnt-combat-state), [initialization](#def-initialization). **Runtime correspondence:** bounded review fixture, not a universal runtime spawn state.

<a id="def-initialization"></a>
**initialization** — keyword. The action that supplies the first values for all model state variables. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-int32-max"></a>
**INT32_MAX** — constant. Largest signed 32-bit integer (`2147483647`) used by the model's saturation and wrap boundaries. **Declared at:** literate model `SirCombat.INT32_MAX`. **Related terms:** [constant](#def-constant), [scale 10,000](#unit-scale-10-000). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-int32-min"></a>
**INT32_MIN** — constant. Smallest signed 32-bit integer (`-2147483648`) used by the model's saturation and wrap boundaries. **Declared at:** literate model `SirCombat.INT32_MIN`. **Related terms:** [constant](#def-constant), [scale 10,000](#unit-scale-10-000). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="unit-integrity-points"></a>
**integrity points** — unit. Whole-number units used for cover durability from 0 through 100. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [scale 10,000](#unit-scale-10-000), [AttackInput](#qnt-attack-input). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-invariant"></a>
**invariant** — keyword. A state predicate expected to hold in every reachable state within the checked boundary. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="def-itf-trace"></a>
**ITF trace** — evidence. Quint's ordered state projection used here with variables `combat` and `last`, normalized to remove volatile metadata before replay. **Declared at:** chapter 40; Q4 fixtures. **Related terms:** [execution trace](#def-execution-trace), [first divergence](#def-first-divergence). **Runtime correspondence:** one committed exact trace and sixteen deterministic sampled traces replay through the real F# interpreter.

<a id="qnt-last"></a>
**last** — variable. Durable `Observation` variable holding the most recently completed modeled action result. **Declared at:** literate model `SirCombat.last`. **Related terms:** [state variable](#def-state-variable), [CombatState](#qnt-combat-state). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-list"></a>
**list** — keyword. An ordered Quint collection; explanation rule IDs use a list because order is observable. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-major-wound"></a>
**MajorWound** — variant. The `MajorWound` case of the authoritative `Wound` sum type used in completed observations. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [variant](#def-variant), [Wound](#qnt-wound). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-maximum"></a>
**maximum** — function. Returns the greater of two integers. **Declared at:** literate model `SirCombat.maximum`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-minimum"></a>
**minimum** — function. Returns the lesser of two integers. **Declared at:** literate model `SirCombat.minimum`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-minor-wound"></a>
**MinorWound** — variant. The `MinorWound` case of the authoritative `Wound` sum type used in completed observations. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [variant](#def-variant), [Wound](#qnt-wound). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-missed-attack"></a>
**missedAttack** — value. Invalid-target attack fixture used to witness that a miss applies neither damage nor suppression. **Declared at:** literate model `SirCombat.missedAttack`. **Related terms:** [pure value](#def-pure-value), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-module"></a>
**module** — keyword. A named Quint namespace containing declarations and an explicit public surface. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-multiply-fixed"></a>
**multiplyFixed** — function. Multiplies two raw fixed-point integers, divides by `SCALE` with half-away rounding, then saturates to signed int32 bounds. **Declared at:** `SirCombat.multiplyFixed`. **Related terms:** [scale 10,000](#unit-scale-10-000), [round-half-away-from-zero](#unit-round-half-away-from-zero), [saturateInt32](#qnt-saturate-int32). **Runtime correspondence:** mirrors `SIR.Domain.FixedPoint` multiplication semantics.

<a id="def-mutation"></a>
**mutation** — evidence. Deliberate temporary defect used to demonstrate that a named detection route goes red; M2 changes representative retention from `8000` to `7000` only in a disposable extraction. **Declared at:** chapter 35 and focused audit. **Related terms:** [observed-red control](#def-observed-red-control), [restored green](#def-restored-green). **Runtime correspondence:** model mutation only; runtime mapping mutations are separate evidence.

<a id="qnt-next-consequences"></a>
**nextConsequences** — function. Purely computes health, eligible suppression, and incapacitation while preserving cover fields for the aggregate consequence action. **Declared at:** `SirCombat.nextConsequences`. **Related terms:** [CombatState](#qnt-combat-state), [damageForAttack](#qnt-damage-for-attack), [resolveConsequences](#qnt-resolve-consequences). **Runtime correspondence:** completed consequence result in `CombatRules`.

<a id="qnt-next-cover-impact"></a>
**nextCoverImpact** — function. Pure successor-state function that subtracts bounded cover damage and disables future blocking when integrity reaches zero. **Declared at:** literate model `SirCombat.nextCoverImpact`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-next-recovery"></a>
**nextRecovery** — function. Pure successor-state function that removes up to five suppression points while preserving all other combat fields. **Declared at:** literate model `SirCombat.nextRecovery`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-nondeterminism"></a>
**nondeterminism** — keyword. A deliberate choice among enabled actions or values; different valid traces need not be failures. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-no-wound"></a>
**NoWound** — variant. The `NoWound` case of the authoritative `Wound` sum type used in completed observations. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [variant](#def-variant), [Wound](#qnt-wound). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-observation"></a>
**Observation** — type. Explanatory projection of the last completed action, including damage arithmetic, wound/contact, suppression/cover outcomes, rule order, event identity, and factions. **Declared at:** `SirCombat.Observation`. **Related terms:** [CombatState](#qnt-combat-state), [AttackInput](#qnt-attack-input), [execution trace](#def-execution-trace). **Runtime correspondence:** compared field by field by the Q4 replay adapter.

<a id="def-observed-red-control"></a>
**observed-red control** — evidence. Recorded failure of a named check after a deliberate mutation; M2 requires the retention mutation to make `representativeDamageIsTwenty` fail with actual damage `18`. **Declared at:** chapter 35 and focused audit. **Related terms:** [mutation](#def-mutation), [restored green](#def-restored-green). **Runtime correspondence:** the full Q4 gate has independent runtime-boundary controls.

<a id="concept-penetration"></a>
**penetration** — concept. Armor interaction represented here by retained effect and completed observations rather than an invented intermediate transition. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="concept-physical-shot-trace"></a>
**physical shot trace** — concept. The registered geometry process that produces visible and total samples before the model consumes their ratio. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-prediction"></a>
**prediction** — evidence. Expected inputs, intermediate values, and successor fields written before execution so trace reading tests understanding rather than hindsight. **Declared at:** chapter 24. **Related terms:** [execution trace](#def-execution-trace), [expected damage](#stat-expected-damage). **Runtime correspondence:** predictions remain model claims until separately replayed.

<a id="stat-preparation-time"></a>
**preparation time** — stat. The fixed-point engagement delay derived from range cells and the registered range slope. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-preparation-raw"></a>
**preparationRaw** — function. Derives engagement preparation as one fixed-point second plus 0.1 second per range cell. **Declared at:** literate model `SirCombat.preparationRaw`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-primed-assignment"></a>
**primed assignment** — keyword. A Quint action assignment such as `combat' = ...` that names a variable's next-state value. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="concept-projectile-contact"></a>
**projectile contact** — concept. The collision event through which trace and cover rules enter completed combat resolution. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-property"></a>
**property** — evidence. A Boolean claim evaluated over model behavior; its strength depends on whether evidence is sampled or exhaustive. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [claim boundary](#def-claim-boundary). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-property-catalogue"></a>
**propertyCatalogue** — value. Finite registry mapping each named model property to its kind and explicit state/declaration subjects. **Declared at:** literate model `SirCombat.propertyCatalogue`. **Related terms:** [pure value](#def-pure-value), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-property-entry"></a>
**PropertyEntry** — type. Record schema for one property-catalogue row: stable ID, property kind, and the set of subjects it constrains. **Declared at:** literate model `SirCombat.PropertyEntry`. **Related terms:** [record](#def-record), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="def-pure-function"></a>
**pure function** — keyword. A Quint definition whose result depends only on its arguments and immutable declarations, without changing model state. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="def-pure-value"></a>
**pure value** — keyword. An immutable Quint value computed without changing model state. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="unit-q4-raw-integer"></a>
**Q4 raw integer** — unit. Signed integer encoding of a fixed-point value at scale 10,000; multiply a human value by 10,000, so `25 → 250000` and `0.8 → 8000`. **Declared at:** raw fields and fixed-point helpers. **Related terms:** [scale 10,000](#unit-scale-10-000), [multiplyFixed](#qnt-multiply-fixed). **Aliases:** `Q4`. **Runtime correspondence:** mirrors `SIR.Domain.FixedPoint` raw representation.

<a id="stat-range-cells"></a>
**range cells** — stat. The non-negative cell distance used to derive engagement preparation time. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-range-slope-raw"></a>
**rangeSlopeRaw** — value. Per-cell engagement preparation slope encoded as raw `1000`, or 0.1 at scale 10,000. **Declared at:** literate model `SirCombat.rangeSlopeRaw`. **Related terms:** [pure value](#def-pure-value), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-reachable-state"></a>
**reachable state** — keyword. A state produced from initialization by zero or more enabled transitions. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="def-record"></a>
**record** — keyword. A Quint value with named fields, used for rules, inputs, state, and observations. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-recovered-suppression"></a>
**recoveredSuppression** — function. Returns the recoverable amount: at most five and never below zero. **Declared at:** literate model `SirCombat.recoveredSuppression`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-recovery-observation"></a>
**recoveryObservation** — function. Builds a completed recovery observation with negative applied suppression delta, event identity, and recovery explanation when applicable. **Declared at:** literate model `SirCombat.recoveryObservation`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="concept-registered-line-of-sight-implementation"></a>
**registered line-of-sight implementation** — concept. The pinned external supercover implementation that owns geometry while Quint owns only its declared sample contract. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Aliases:** `line of sight`, `LoS`. **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-representative-attack"></a>
**representativeAttack** — value. Bounded teaching input with rifle raw damage `250000`, full `10/10` trace, range 3, retention `8000`, and suppression delta 12. **Declared at:** `SirCombat.representativeAttack`. **Related terms:** [AttackInput](#qnt-attack-input), [representativeDamageIsTwenty](#run-representative-damage-is-twenty). **Runtime correspondence:** exercised by exact and seeded sampled Q4 replay.

<a id="run-representative-damage-is-twenty"></a>
**representativeDamageIsTwenty** — run. Named witness from initialization through representative consequence resolution, asserting damage 20, raw arithmetic observations, health 80, suppression 12, and core invariants. **Declared at:** `SirCombatTests.representativeDamageIsTwenty`. **Related terms:** [representativeAttack](#qnt-representative-attack), [witness](#def-witness). **Runtime correspondence:** exact model result is included in the current Q4 qualification boundary.

<a id="qnt-resolve-consequences"></a>
**resolveConsequences** — action. Guarded atomic transition assigning both the next combat state and completed consequence observation. **Declared at:** `SirCombat.resolveConsequences`. **Related terms:** [validAttack](#qnt-valid-attack), [nextConsequences](#qnt-next-consequences), [Observation](#qnt-observation). **Runtime correspondence:** aggregate completed-consequence entry point in `CombatRules`.

<a id="qnt-resolve-cover-impact"></a>
**resolveCoverImpact** — action. Guarded atomic action that publishes `nextCoverImpact` together with its completed cover observation. **Declared at:** literate model `SirCombat.resolveCoverImpact`. **Related terms:** [state transition](#def-state-transition), [CombatState](#qnt-combat-state). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-resolve-recovery"></a>
**resolveRecovery** — action. Atomic action that publishes `nextRecovery` together with its completed recovery observation. **Declared at:** literate model `SirCombat.resolveRecovery`. **Related terms:** [state transition](#def-state-transition), [CombatState](#qnt-combat-state). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-restored-green"></a>
**restored green** — evidence. Successful re-execution against the untouched authority after the disposable mutated subject has failed and been discarded. **Declared at:** chapter 35 and focused audit. **Related terms:** [observed-red control](#def-observed-red-control), [mutation](#def-mutation). **Runtime correspondence:** full Q4 qualification returns green after its independent controls.

<a id="concept-retained-effect"></a>
**retained effect** — concept. The clamped 0–1 fixed-point share of traced damage left after armor. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-retained-effect"></a>
**retainedEffect** — function. Clamps raw armor retention to `0..SCALE` before damage multiplication. **Declared at:** `SirCombat.retainedEffect`. **Related terms:** [armor retention](#stat-armor-retention), [expectedDamageRaw](#qnt-expected-damage-raw). **Runtime correspondence:** bounded retained-effect handling in damage resolution.

<a id="qnt-rifle-damage-raw"></a>
**rifleDamageRaw** — value. Representative rifle base damage encoded as raw `250000`, or 25 whole damage at scale 10,000. **Declared at:** literate model `SirCombat.rifleDamageRaw`. **Related terms:** [pure value](#def-pure-value), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="unit-round-half-away-from-zero"></a>
**round-half-away-from-zero** — unit. Tie-breaking rule in which an exact half advances one integer away from zero; `17.5 → 18` and `-17.5 → -18`. **Declared at:** `divideRoundedAwayFromZero`; positive final whole damage uses the `+SCALE/2` path. **Related terms:** [multiplyFixed](#qnt-multiply-fixed), [roundedDamage](#qnt-rounded-damage). **Runtime correspondence:** mirrors fixed-point rounding, subject to the named final-wrap boundary.

<a id="qnt-rounded-damage"></a>
**roundedDamage** — function. Converts positive raw damage to whole points after applying signed int32 wrap to `rawDamage + SCALE/2`; representative raw `200000` becomes `20`. **Declared at:** `SirCombat.roundedDamage`. **Related terms:** [expectedDamageRaw](#qnt-expected-damage-raw), [round-half-away-from-zero](#unit-round-half-away-from-zero), [claim boundary](#def-claim-boundary). **Runtime correspondence:** preserves the runtime's explicit pre-division int32-wrap behavior.

<a id="qnt-rule-catalogue"></a>
**ruleCatalogue** — value. Finite sixteen-entry registry of stable combat rule IDs, kinds, and direct dependencies consumed by catalogue properties and traceability checks. **Declared at:** literate model `SirCombat.ruleCatalogue`. **Related terms:** [pure value](#def-pure-value), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-rule-entry"></a>
**RuleEntry** — type. Record schema for one stable rule row: rule ID, kind, and its direct dependency set. **Declared at:** literate model `SirCombat.RuleEntry`. **Related terms:** [record](#def-record), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="def-run"></a>
**run** — keyword. A named executable Quint scenario that asks for a concrete satisfying trace or value. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="def-safety-property"></a>
**safety property** — evidence. A property stating that an unwanted state is never reachable inside the checked boundary. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [claim boundary](#def-claim-boundary), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="def-sampled-run"></a>
**sampled run** — evidence. A deterministic but non-exhaustive set of executions identified by seed, count, and step bound. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [claim boundary](#def-claim-boundary), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="unit-samples"></a>
**samples** — unit. Integer trace observations counted as visible and total before conversion to a fixed-point ratio. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [scale 10,000](#unit-scale-10-000), [AttackInput](#qnt-attack-input). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-saturate-int32"></a>
**saturateInt32** — function. Clamps a mathematical integer below `INT32_MIN` or above `INT32_MAX` to the nearest signed-32-bit boundary. **Declared at:** literate model `SirCombat.saturateInt32`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-scale"></a>
**SCALE** — constant. Fixed-point denominator `10000`; one human unit is represented by ten thousand raw units. **Declared at:** literate model `SirCombat.SCALE`. **Related terms:** [constant](#def-constant), [scale 10,000](#unit-scale-10-000). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="unit-scale-10-000"></a>
**scale 10,000** — unit. Fixed-point denominator named `SCALE`; raw `10000` represents `1.0`, and raw/human conversion moves the decimal four places. **Declared at:** `SirCombat.SCALE`. **Related terms:** [Q4 raw integer](#unit-q4-raw-integer), [multiplyFixed](#qnt-multiply-fixed). **Runtime correspondence:** shared raw fixed-point scale in `SIR.Domain.FixedPoint`.

<a id="unit-seconds"></a>
**seconds** — unit. The human-facing time unit represented by the model's fixed-point preparation value. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [scale 10,000](#unit-scale-10-000), [AttackInput](#qnt-attack-input). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-set"></a>
**set** — keyword. An unordered Quint collection with unique members, used for catalogue identity and dependency membership. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="unit-signed-32-bit-saturation"></a>
**signed 32-bit saturation** — unit. Clamping a mathematical integer to `INT32_MIN..INT32_MAX` at the model boundaries where runtime arithmetic saturates. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [scale 10,000](#unit-scale-10-000), [AttackInput](#qnt-attack-input). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-sir-combat"></a>
**SirCombat** — module. Primary Quint module defining the bounded combat types, facts, pure helpers, state variables, actions, and properties. **Declared at:** literate model module `SirCombat`. **Related terms:** [module](#def-module), [type](#def-type). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-sir-combat-tests"></a>
**SirCombatTests** — module. Companion Quint module importing `SirCombat` and defining executable witnesses for representative and boundary behaviors. **Declared at:** literate model module `SirCombatTests`. **Related terms:** [module](#def-module), [type](#def-type). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="property-sixteen-rules-declared"></a>
**sixteenRulesDeclared** — property. Catalogue property requiring exactly sixteen unique stable rule entries. **Declared at:** literate model `SirCombat.sixteenRulesDeclared`. **Related terms:** [property](#def-property), [bounded verification](#def-bounded-verification). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="catalogue-property-sixteen-rules-declared"></a>
**SixteenRulesDeclared** — catalogue property. The catalogue identity for the `sixteenRulesDeclared` model property, including its declared subjects. **Declared at:** `SirCombat.propertyCatalogue`. **Related terms:** [propertyCatalogue](#qnt-property-catalogue), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="def-source-digest"></a>
**source digest** — evidence. A cryptographic identity of an authoritative input used to bind generated projections and evidence to exact content. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [claim boundary](#def-claim-boundary), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="def-state-transition"></a>
**state transition** — keyword. One atomic step from current variable bindings to their primed successor bindings. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="def-state-variable"></a>
**state variable** — keyword. A mutable Quint declaration whose current and next values define model state. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-step"></a>
**step** — action. Nondeterministic transition action choosing one enabled consequence, cover-impact, or recovery branch per atomic successor. **Declared at:** literate model `SirCombat.step`. **Related terms:** [state transition](#def-state-transition), [CombatState](#qnt-combat-state). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-stuttering"></a>
**stuttering** — keyword. A behavior step that leaves relevant state unchanged; it must not be confused with a completed combat action. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="stat-suppression"></a>
**suppression** — stat. A bounded 0–100 durable combat pressure value changed by damaging attacks and recovery. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="stat-suppression-delta"></a>
**suppression delta** — stat. The requested or applied whole-number change in suppression for one observation. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="concept-suppression-eligibility"></a>
**suppression eligibility** — concept. The rule that requested suppression applies only when completed damage is positive. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="unit-suppression-points"></a>
**suppression points** — unit. Whole-number units used for accumulated suppression from 0 through 100. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [scale 10,000](#unit-scale-10-000), [AttackInput](#qnt-attack-input). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="concept-suppression-recovery"></a>
**suppression recovery** — concept. The focused transition that removes at most five current suppression points. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-suppression-for-damage"></a>
**suppressionForDamage** — function. Returns a non-negative requested suppression delta only when completed damage is positive; otherwise returns zero. **Declared at:** literate model `SirCombat.suppressionForDamage`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="run-suppression-needs-positive-damage-and-recovers-five"></a>
**suppressionNeedsPositiveDamageAndRecoversFive** — run. Executable sequence witnessing zero suppression on a miss, positive suppression on a hit, and a five-point recovery. **Declared at:** literate model `SirCombatTests.suppressionNeedsPositiveDamageAndRecoversFive`. **Related terms:** [run](#def-run), [witness](#def-witness). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="property-suppression-requires-damage"></a>
**suppressionRequiresDamage** — property. Observation property requiring zero applied suppression whenever a resolved attack reports non-positive damage. **Declared at:** literate model `SirCombat.suppressionRequiresDamage`. **Related terms:** [property](#def-property), [bounded verification](#def-bounded-verification). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="catalogue-property-suppression-requires-damage"></a>
**SuppressionRequiresDamage** — catalogue property. The catalogue identity for the `suppressionRequiresDamage` model property, including its declared subjects. **Declared at:** `SirCombat.propertyCatalogue`. **Related terms:** [propertyCatalogue](#qnt-property-catalogue), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="concept-target-footprint"></a>
**target footprint** — concept. The Boolean input asserting that a trace intersects a valid target area before attack resolution. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-terminal-state"></a>
**terminal state** — keyword. A state after which the modeled execution has no required successor; evidence must distinguish it from bounded trace termination. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="stat-total-samples"></a>
**total samples** — stat. The positive denominator of a valid trace ratio. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="stat-trace-probability"></a>
**trace probability** — stat. Ratio of visible to total samples after trace validity; representative `10/10` is raw `10000` (`1.0`). **Declared at:** `traceRaw`. **Related terms:** [traceRaw](#qnt-trace-raw), [samples](#unit-samples), [expected damage](#stat-expected-damage). **Runtime correspondence:** trace counts come from the registered external line-of-sight boundary.

<a id="qnt-trace-algorithm"></a>
**traceAlgorithm** — value. Registered metadata contract for `FS.GG.Game.Core.Los.supercover.v1`, including sample units, first-collision tie break, and source fingerprint. **Declared at:** literate model `SirCombat.traceAlgorithm`. **Related terms:** [pure value](#def-pure-value), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-trace-raw"></a>
**traceRaw** — function. Converts valid visible/total integer samples into a scale-10,000 ratio with fixed-point rounding. **Declared at:** `SirCombat.traceRaw`. **Related terms:** [trace probability](#stat-trace-probability), [validTrace](#qnt-valid-trace), [fromRatio](#qnt-from-ratio). **Runtime correspondence:** models the output contract, not the external supercover traversal.

<a id="def-type"></a>
**type** — keyword. A Quint declaration defining the shape and allowed values of model data. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-uint32-range"></a>
**UINT32_RANGE** — constant. Unsigned 32-bit modulus (`4294967296`) used to wrap one-step signed-int32 overflow at final damage rounding. **Declared at:** literate model `SirCombat.UINT32_RANGE`. **Related terms:** [constant](#def-constant), [scale 10,000](#unit-scale-10-000). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-valid-attack"></a>
**validAttack** — function. Guard requiring a target footprint and a valid visible/total sample pair before consequence resolution can fire. **Declared at:** `SirCombat.validAttack`. **Related terms:** [AttackInput](#qnt-attack-input), [validTrace](#qnt-valid-trace), [resolveConsequences](#qnt-resolve-consequences). **Runtime correspondence:** bounded precondition for the modeled aggregate transition.

<a id="qnt-valid-trace"></a>
**validTrace** — function. Accepts a trace exactly when total samples are positive and visible samples lie inclusively between zero and total. **Declared at:** literate model `SirCombat.validTrace`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="property-valid-trace-observation"></a>
**validTraceObservation** — property. Observation property requiring every resolved attack's emitted trace ratio to remain between zero and `SCALE`. **Declared at:** literate model `SirCombat.validTraceObservation`. **Related terms:** [property](#def-property), [bounded verification](#def-bounded-verification). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="catalogue-property-valid-trace-observation"></a>
**ValidTraceObservation** — catalogue property. The catalogue identity for the `validTraceObservation` model property, including its declared subjects. **Declared at:** `SirCombat.propertyCatalogue`. **Related terms:** [propertyCatalogue](#qnt-property-catalogue), [property](#def-property). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="def-variant"></a>
**variant** — keyword. One named case of a Quint sum type, such as `NoWound`, `MinorWound`, or `MajorWound`. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="stat-visible-samples"></a>
**visible samples** — stat. The non-negative numerator of a valid trace ratio, never greater than total samples. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="def-witness"></a>
**witness** — keyword. A concrete execution demonstrating that at least one behavior or state is reachable; it is not a universal proof. **Declared at:** handbook formal-reasoning chapters 18 and 33–45. **Related terms:** [SirCombat](#qnt-sir-combat), [state transition](#def-state-transition). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="concept-wound"></a>
**wound** — concept. The completed damage classification `NoWound`, `MinorWound`, or `MajorWound` at exact thresholds. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-wound"></a>
**Wound** — type. Three-case damage classification type: `NoWound`, `MinorWound`, or `MajorWound`. **Declared at:** literate model `SirCombat.Wound`. **Related terms:** [record](#def-record), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="stat-wound-threshold"></a>
**wound threshold** — stat. A whole-damage boundary: 25 begins a minor wound and 50 begins a major wound. **Declared at:** handbook combat walkthroughs and controlled rule catalogue. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="qnt-wound-for-damage"></a>
**woundForDamage** — function. Classifies whole damage below 25 as no wound, 25–49 as minor, and 50 or more as major. **Declared at:** literate model `SirCombat.woundForDamage`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="run-wound-thresholds-are-exact"></a>
**woundThresholdsAreExact** — run. Executable sequence witnessing the exact 24/25/50 no-wound, minor-wound, and major-wound boundaries. **Declared at:** literate model `SirCombatTests.woundThresholdsAreExact`. **Related terms:** [run](#def-run), [witness](#def-witness). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

<a id="qnt-wrap-int32"></a>
**wrapInt32** — function. Applies one unsigned-32-bit modulus adjustment when a value crosses a signed-int32 boundary, matching the runtime's unchecked final-rounding addition. **Declared at:** literate model `SirCombat.wrapInt32`. **Related terms:** [pure function](#def-pure-function), [SirCombat](#qnt-sir-combat). **Runtime correspondence:** scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit.

<a id="run-zero-health-means-incapacitated"></a>
**zeroHealthMeansIncapacitated** — run. Executable witness that an attack reducing health to zero also sets incapacitation true in the same atomic successor. **Declared at:** literate model `SirCombatTests.zeroHealthMeansIncapacitated`. **Related terms:** [run](#def-run), [witness](#def-witness). **Runtime correspondence:** model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject.

[Back to the table of contents](#table-of-contents)
