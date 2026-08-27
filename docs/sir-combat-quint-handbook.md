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

This in-progress first edition combines a stable navigation and definition-address contract with one complete learning spine: the representative rifle attack. Chapters still marked “Scheduled content” belong to later roadmap milestones and remain honest placeholders, not executable claims.

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

The first five arrows calculate a completed consequence. The final arrow is one [aggregate attack resolution](#concept-aggregate-attack-resolution): the production interpreter exposes the completed result, so the model does not invent observable intermediate states between [health](#stat-health), [wound](#concept-wound), or [suppression](#stat-suppression) updates. [Cover impact](#concept-cover-blocking) and [suppression recovery](#concept-suppression-recovery) are separate runtime entry points and therefore separate [actions](#def-action); their full walkthroughs remain M3 work.

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

There is one important [claim boundary](#def-claim-boundary): [fromRatio](#qnt-from-ratio), [addFixed](#qnt-add-fixed), and [multiplyFixed](#qnt-multiply-fixed) call [saturateInt32](#qnt-saturate-int32), but [roundedDamage](#qnt-rounded-damage) deliberately models the runtime's signed int32 wrap in `rawDamage + SCALE / 2` before division. The representative `200000 + 5000` is safely inside int32. Do not infer from this example that overflow saturates at the final rounding layer; the separate authority [run](#def-run) `damageRoundingPreservesInt32Wrap` preserves that edge explicitly.

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

Start with [action](#def-action) names, then compare values. In the representative [execution trace](#def-execution-trace), state 0 is `Initialize` and state 1 is `ResolveConsequences`. That tells you which transition to explain before you inspect any number.

Next group fields by role:

1. **Inputs reflected in the observation:** event/factions, raw trace, and retained ratio identify the attempted shot.
2. **Calculated explanation:** preparation `13000`, [damage](#stat-damage) `20`, and ordered rule IDs explain why the transition occurred.
3. **Durable consequences:** [health](#stat-health) `80` and [suppression](#stat-suppression) `12` are the successor [CombatState](#qnt-combat-state).
4. **Unchanged state:** [cover integrity](#stat-cover-integrity)/blocking stay unchanged because this is the aggregate consequence [action](#def-action), not a cover-impact [action](#def-action).

Finally reconcile the records. The [last](#qnt-last) [damage](#stat-damage) field equals `20`, explaining the [health](#stat-health) delta `100 - 20 = 80`; positive [damage](#stat-damage) explains the [suppression delta](#stat-suppression-delta) `12`; [health](#stat-health) above zero explains why [incapacitation](#concept-incapacitation) remains false. A trace is useful when these relationships agree, not merely when it contains many fields.

<a id="chapter-34-reading-and-minimizing-a-counterexample"></a>
### 34. Reading and minimizing a counterexample

*Scheduled content:* this chapter's substantive walkthrough and executable evidence land in the roadmap milestone assigned to it.

<a id="chapter-35-mutation-laboratory"></a>
### 35. Mutation laboratory

M2 uses one deliberately narrow [mutation](#def-mutation): in a disposable extracted model, change the representative `armorRetentionRaw` from `8000` to `7000`. Do not edit the literate authority.

[Prediction](#def-prediction): the model now calculates `25 × 1.0 × 0.7 = 17.5`, and [round-half-away-from-zero](#unit-round-half-away-from-zero) yields `18`. The unchanged [witness](#def-witness) still expects `20`, so [representativeDamageIsTwenty](#run-representative-damage-is-twenty) must fail. That failure is the [observed-red control](#def-observed-red-control): it demonstrates that the named [run](#def-run) detects the semantic defect rather than merely returning success for any input.

The focused M2 audit performs exactly this [mutation](#def-mutation), requires non-zero Quint test status and an observed `18`, discards the disposable file, extracts the untouched authority again, and requires the same [run](#def-run) green. That [last](#qnt-last) execution is [restored green](#def-restored-green). Broader [mutation](#def-mutation) families and [counterexample](#def-counterexample) minimization remain M4 work.

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

This M2 map is intentionally limited to the representative attack. M5 owns the complete [correspondence](#def-correspondence) reference.

| Modeling layer | Quint subject | Current runtime/corpus subject | M2 claim |
|---|---|---|---|
| raw fixed-point values | [SCALE](#qnt-scale), [multiplyFixed](#qnt-multiply-fixed), [roundedDamage](#qnt-rounded-damage) | `SIR.Domain.FixedPoint` and [damage](#stat-damage) helpers used by `src/SIR.Simulation/CombatRules.fs` | The model encodes the documented scale, half-away rounding, saturation helpers, and final int32-wrap boundary explicitly. |
| representative facts | [rifleDamageRaw](#qnt-rifle-damage-raw), [representativeAttack](#qnt-representative-attack) | stable rifle/body facts and `CombatRules` inputs | The learning input is `25`, full trace, and retention `0.8`; this is a bounded representative fixture, not every weapon/body combination. |
| completed consequences | [nextConsequences](#qnt-next-consequences), [resolveConsequences](#qnt-resolve-consequences) | `CombatRules` completed physical-[combat](#qnt-combat) consequence resolution | The [action](#def-action) is atomic because the interpreter exposes completed consequences, not intermediate model states. |
| explanation | [Observation](#qnt-observation), [consequenceExplanationOrder](#qnt-consequence-explanation-order) | [combat](#qnt-combat) facts/events and stable rule metadata | The [observation](#qnt-observation) is a review projection of the completed [action](#def-action); the adapter compares every listed model field for accepted trace actions against its runtime projection. |
| execution evidence | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) plus sampled ITF traces | `tests/SIR.Conformance.Shared/QuintQ4ReplayFixtures.fs` and `CombatRules` | The committed exact trace and 16 seeded sampled traces have replayed through the real interpreter at the qualification boundary. |

[Claim boundary](#def-claim-boundary): a green Quint [run](#def-run) establishes behavior of the extracted model. It does **not** by itself establish production [correspondence](#def-correspondence). The repository qualification separately runs exact and sampled traces through the real interpreter and independently mutates mapping, observable fields, and interpreter results to prove that boundary detects divergence. That evidence is scoped to its pinned source/tool identities and sampled traces; it is neither an exhaustive proof nor automatic equivalence for future changes.

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

For the representative spine, evidence is a three-step sequence, not a permanently broken example:

1. [Run](#def-run) the authority-derived [representativeDamageIsTwenty](#run-representative-damage-is-twenty) [witness](#def-witness) green.
2. Change only retention `8000 → 7000` in a disposable extraction and observe the named [witness](#def-witness) red because actual [damage](#stat-damage) becomes `18` while [expected damage](#stat-expected-damage) remains `20`.
3. Re-extract the untouched authority and [run](#def-run) the [witness](#def-witness) green again.

The audit records all three outcomes. The [mutation](#def-mutation) file is temporary and never becomes an authoring source. M4 extends this pattern to other formal subjects; M5 explains the independent runtime-mapping mutations.

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
| [Representative damage 20](#qnt-representative-attack) | [CONTENT-WEAPON-RIFLE-001](#rule-content-weapon-rifle-001), [COMBAT-TRACE-002](#rule-combat-trace-002), [COMBAT-ARMOR-004](#rule-combat-armor-004), [COMBAT-DAMAGE-001](#rule-combat-damage-001), [COMBAT-ATTACK-RESOLUTION-001](#rule-combat-attack-resolution-001) | [representativeAttack](#qnt-representative-attack), [expectedDamageRaw](#qnt-expected-damage-raw), [damageForAttack](#qnt-damage-for-attack), [resolveConsequences](#qnt-resolve-consequences) | [representativeDamageIsTwenty](#run-representative-damage-is-twenty) | `SIR.Domain.FixedPoint`; `src/SIR.Simulation/CombatRules.fs`; `tests/SIR.Conformance.Shared/QuintQ4ReplayFixtures.fs` | `work/361-handbook-m2/audit-representative-attack.mjs`; `readiness/361-handbook-m2/handbook-m2.junit.xml`; `scripts/qualify-quint-q4-sir-combat.sh` | M2 model/excerpt/[mutation](#def-mutation) evidence plus bounded representative runtime [correspondence](#def-correspondence); full M5 [correspondence](#def-correspondence) remains pending |
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

<a id="qnt-absolute"></a>
**absolute** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-action"></a>
**action** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-add-fixed"></a>
**addFixed** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-aggregate-attack-resolution"></a>
**aggregate attack resolution** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-algorithm-entry"></a>
**AlgorithmEntry** — type. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-allied-attack"></a>
**alliedAttack** — value. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-armor-retention"></a>
**armor retention** — stat. Fraction of traced damage retained after armor, clamped to raw `0..10000`; the representative attack uses `8000` (`0.8`). **Declared at:** `AttackInput.armorRetentionRaw`. **Related terms:** [retainedEffect](#qnt-retained-effect), [expected damage](#stat-expected-damage). **Runtime correspondence:** fixed-point armor/damage handling in `CombatRules`.

<a id="qnt-attack-input"></a>
**AttackInput** — type. Immutable bounded inputs for one attempted attack: target validity, raw damage/retention, trace samples, range, suppression, collision flags, factions, and event identity. **Declared at:** `SirCombat.AttackInput`. **Related terms:** [CombatState](#qnt-combat-state), [Observation](#qnt-observation), [validAttack](#qnt-valid-attack). **Runtime correspondence:** inputs consumed by `CombatRules`; M5 completes the map.

<a id="stat-base-damage"></a>
**base damage** — stat. Weapon damage before trace and armor retention; the representative rifle value is `25`, encoded as raw `250000`. **Declared at:** `rifleDamageRaw` and `AttackInput.baseDamageRaw`. **Related terms:** [expected damage](#stat-expected-damage), [scale 10,000](#unit-scale-10-000). **Runtime correspondence:** rifle fact consumed by `CombatRules`.

<a id="def-bounded-verification"></a>
**bounded verification** — evidence. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-bounded100"></a>
**bounded100** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="property-bounded-combat-state"></a>
**boundedCombatState** — property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="catalogue-property-bounded-combat-state"></a>
**BoundedCombatState** — catalogue property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="unit-cells"></a>
**cells** — unit. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-claim-boundary"></a>
**claim boundary** — evidence. Explicit limit on what an execution or receipt establishes; a green Quint witness establishes model behavior, while runtime correspondence requires separate interpreter replay evidence. **Declared at:** handbook chapters 14 and 38. **Related terms:** [correspondence](#def-correspondence), [witness](#def-witness). **Runtime correspondence:** enforced by exact/sampled Q4 replay and independent divergence mutations.

<a id="concept-collateral-consequence"></a>
**collateral consequence** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="run-collateral-outcome-ignores-faction"></a>
**collateralOutcomeIgnoresFaction** — run. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-combat"></a>
**combat** — variable. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-armor-004"></a>
**COMBAT-ARMOR-004** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-attack-resolution-001"></a>
**COMBAT-ATTACK-RESOLUTION-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-collateral-001"></a>
**COMBAT-COLLATERAL-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-collision-001"></a>
**COMBAT-COLLISION-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-cover-003"></a>
**COMBAT-COVER-003** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-cover-destruction-001"></a>
**COMBAT-COVER-DESTRUCTION-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-damage-001"></a>
**COMBAT-DAMAGE-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-engagement-001"></a>
**COMBAT-ENGAGEMENT-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-health-001"></a>
**COMBAT-HEALTH-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-penetration-001"></a>
**COMBAT-PENETRATION-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-suppression-001"></a>
**COMBAT-SUPPRESSION-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-suppression-recovery-001"></a>
**COMBAT-SUPPRESSION-RECOVERY-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-trace-002"></a>
**COMBAT-TRACE-002** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-combat-wound-001"></a>
**COMBAT-WOUND-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-combat-state"></a>
**CombatState** — type. Cohesive durable combat state containing health, suppression, cover integrity/blocking, and incapacitation. **Declared at:** `SirCombat.CombatState`; initialized by `initialCombat`. **Related terms:** [AttackInput](#qnt-attack-input), [Observation](#qnt-observation), [nextConsequences](#qnt-next-consequences). **Runtime correspondence:** completed state consequences produced by `CombatRules`.

<a id="qnt-consequence-explanation-order"></a>
**consequenceExplanationOrder** — value. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-consequence-observation"></a>
**consequenceObservation** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-constant"></a>
**constant** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-content-body-human-001"></a>
**CONTENT-BODY-HUMAN-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="rule-content-weapon-rifle-001"></a>
**CONTENT-WEAPON-RIFLE-001** — rule. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-correspondence"></a>
**correspondence** — evidence. Checked agreement between model observations and production interpreter outcomes under explicitly identified traces, mappings, versions, and subjects; it is separate from model execution. **Declared at:** chapter 38 and Q4 replay receipts. **Related terms:** [claim boundary](#def-claim-boundary), [execution trace](#def-execution-trace). **Runtime correspondence:** `QuintQ4ReplayFixtures` compares exact and sampled traces with `CombatRules` and reports first divergence.

<a id="def-counterexample"></a>
**counterexample** — evidence. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-cover-blocking"></a>
**cover blocking** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-cover-damage"></a>
**cover damage** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-cover-integrity"></a>
**cover integrity** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-cover-damage"></a>
**coverDamage** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-cover-observation"></a>
**coverObservation** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-current-collision-consumption"></a>
**current-collision consumption** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-damage"></a>
**damage** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="unit-damage-points"></a>
**damage points** — unit. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-damage-for-attack"></a>
**damageForAttack** — function. Composes trace ratio, retained effect, expected raw damage, and final whole-point rounding for one valid input. **Declared at:** `SirCombat.damageForAttack`. **Related terms:** [traceRaw](#qnt-trace-raw), [expectedDamageRaw](#qnt-expected-damage-raw), [roundedDamage](#qnt-rounded-damage). **Runtime correspondence:** representative damage calculation in `CombatRules`.

<a id="concept-destroyed-cover"></a>
**destroyed cover** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="property-destroyed-cover-is-permeable"></a>
**destroyedCoverIsPermeable** — property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="catalogue-property-destroyed-cover-is-permeable"></a>
**DestroyedCoverIsPermeable** — catalogue property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="run-destroying-cover-consumes-current-collision"></a>
**destroyingCoverConsumesCurrentCollision** — run. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-divide-rounded-away-from-zero"></a>
**divideRoundedAwayFromZero** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-event-identity"></a>
**event identity** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-execution-trace"></a>
**execution trace** — evidence. Ordered model states connected by named actions; read the action first, then reconcile input reflection, explanation fields, changed state, and unchanged state. **Declared at:** Quint run output and ITF traces. **Related terms:** [Observation](#qnt-observation), [correspondence](#def-correspondence). **Runtime correspondence:** exact/sampled traces are replayed separately through the real interpreter.

<a id="def-exhaustive-check"></a>
**exhaustive check** — evidence. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-expected-damage"></a>
**expected damage** — stat. Raw base damage multiplied by trace probability and clamped armor retention before conversion to whole damage points; representative raw value `200000` becomes `20`. **Declared at:** `expectedDamageRaw`. **Related terms:** [base damage](#stat-base-damage), [trace probability](#stat-trace-probability), [armor retention](#stat-armor-retention). **Runtime correspondence:** fixed-point damage path in `CombatRules`.

<a id="qnt-expected-damage-raw"></a>
**expectedDamageRaw** — function. Performs the two scale-preserving fixed multiplications `base × trace × retainedEffect(retention)`. **Declared at:** `SirCombat.expectedDamageRaw`. **Related terms:** [multiplyFixed](#qnt-multiply-fixed), [retainedEffect](#qnt-retained-effect), [roundedDamage](#qnt-rounded-damage). **Runtime correspondence:** model-side counterpart of fixed-point damage composition.

<a id="concept-explanation-order"></a>
**explanation order** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-external-algorithm-contract"></a>
**external algorithm contract** — evidence. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-faction-neutral-consequence"></a>
**faction-neutral consequence** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="property-faction-neutral-collateral"></a>
**factionNeutralCollateral** — property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="catalogue-property-faction-neutral-collateral"></a>
**FactionNeutralCollateral** — catalogue property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-first-collision"></a>
**first collision** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-first-divergence"></a>
**first divergence** — evidence. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="unit-fixed-point-ratio"></a>
**fixed-point ratio** — unit. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-from-ratio"></a>
**fromRatio** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-full-damage-attack"></a>
**fullDamageAttack** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-generated-projection"></a>
**generated projection** — evidence. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-guard"></a>
**guard** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-health"></a>
**health** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="unit-hit-points"></a>
**hit points** — unit. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-hp"></a>
**HP** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-human-armor-retention-raw"></a>
**humanArmorRetentionRaw** — value. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-import"></a>
**import** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-incapacitation"></a>
**incapacitation** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="property-incapacity-matches-health"></a>
**incapacityMatchesHealth** — property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="catalogue-property-incapacity-matches-health"></a>
**IncapacityMatchesHealth** — catalogue property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-init"></a>
**init** — action. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-initial-combat"></a>
**initialCombat** — value. Fully specified starting state: health 100, suppression 0, cover integrity 100, blocking true, incapacitated false. **Declared at:** `SirCombat.initialCombat`. **Related terms:** [CombatState](#qnt-combat-state), [initialization](#def-initialization). **Runtime correspondence:** bounded review fixture, not a universal runtime spawn state.

<a id="def-initialization"></a>
**initialization** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-int32-max"></a>
**INT32_MAX** — constant. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-int32-min"></a>
**INT32_MIN** — constant. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="unit-integrity-points"></a>
**integrity points** — unit. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-invariant"></a>
**invariant** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-itf-trace"></a>
**ITF trace** — evidence. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-last"></a>
**last** — variable. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-list"></a>
**list** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-major-wound"></a>
**MajorWound** — variant. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-maximum"></a>
**maximum** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-minimum"></a>
**minimum** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-minor-wound"></a>
**MinorWound** — variant. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-missed-attack"></a>
**missedAttack** — value. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-module"></a>
**module** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-multiply-fixed"></a>
**multiplyFixed** — function. Multiplies two raw fixed-point integers, divides by `SCALE` with half-away rounding, then saturates to signed int32 bounds. **Declared at:** `SirCombat.multiplyFixed`. **Related terms:** [scale 10,000](#unit-scale-10-000), [round-half-away-from-zero](#unit-round-half-away-from-zero), [saturateInt32](#qnt-saturate-int32). **Runtime correspondence:** mirrors `SIR.Domain.FixedPoint` multiplication semantics.

<a id="def-mutation"></a>
**mutation** — evidence. Deliberate temporary defect used to demonstrate that a named detection route goes red; M2 changes representative retention from `8000` to `7000` only in a disposable extraction. **Declared at:** chapter 35 and focused audit. **Related terms:** [observed-red control](#def-observed-red-control), [restored green](#def-restored-green). **Runtime correspondence:** model mutation only; runtime mapping mutations are separate evidence.

<a id="qnt-next-consequences"></a>
**nextConsequences** — function. Purely computes health, eligible suppression, and incapacitation while preserving cover fields for the aggregate consequence action. **Declared at:** `SirCombat.nextConsequences`. **Related terms:** [CombatState](#qnt-combat-state), [damageForAttack](#qnt-damage-for-attack), [resolveConsequences](#qnt-resolve-consequences). **Runtime correspondence:** completed consequence result in `CombatRules`.

<a id="qnt-next-cover-impact"></a>
**nextCoverImpact** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-next-recovery"></a>
**nextRecovery** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-nondeterminism"></a>
**nondeterminism** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-no-wound"></a>
**NoWound** — variant. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-observation"></a>
**Observation** — type. Explanatory projection of the last completed action, including damage arithmetic, wound/contact, suppression/cover outcomes, rule order, event identity, and factions. **Declared at:** `SirCombat.Observation`. **Related terms:** [CombatState](#qnt-combat-state), [AttackInput](#qnt-attack-input), [execution trace](#def-execution-trace). **Runtime correspondence:** compared field by field by the Q4 replay adapter.

<a id="def-observed-red-control"></a>
**observed-red control** — evidence. Recorded failure of a named check after a deliberate mutation; M2 requires the retention mutation to make `representativeDamageIsTwenty` fail with actual damage `18`. **Declared at:** chapter 35 and focused audit. **Related terms:** [mutation](#def-mutation), [restored green](#def-restored-green). **Runtime correspondence:** the full Q4 gate has independent runtime-boundary controls.

<a id="concept-penetration"></a>
**penetration** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-physical-shot-trace"></a>
**physical shot trace** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-prediction"></a>
**prediction** — evidence. Expected inputs, intermediate values, and successor fields written before execution so trace reading tests understanding rather than hindsight. **Declared at:** chapter 24. **Related terms:** [execution trace](#def-execution-trace), [expected damage](#stat-expected-damage). **Runtime correspondence:** predictions remain model claims until separately replayed.

<a id="stat-preparation-time"></a>
**preparation time** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-preparation-raw"></a>
**preparationRaw** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-primed-assignment"></a>
**primed assignment** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-projectile-contact"></a>
**projectile contact** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-property"></a>
**property** — evidence. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-property-catalogue"></a>
**propertyCatalogue** — value. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-property-entry"></a>
**PropertyEntry** — type. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-pure-function"></a>
**pure function** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-pure-value"></a>
**pure value** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="unit-q4-raw-integer"></a>
**Q4 raw integer** — unit. Signed integer encoding of a fixed-point value at scale 10,000; multiply a human value by 10,000, so `25 → 250000` and `0.8 → 8000`. **Declared at:** raw fields and fixed-point helpers. **Related terms:** [scale 10,000](#unit-scale-10-000), [multiplyFixed](#qnt-multiply-fixed). **Runtime correspondence:** mirrors `SIR.Domain.FixedPoint` raw representation.

<a id="stat-range-cells"></a>
**range cells** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-range-slope-raw"></a>
**rangeSlopeRaw** — value. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-reachable-state"></a>
**reachable state** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-record"></a>
**record** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-recovered-suppression"></a>
**recoveredSuppression** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-recovery-observation"></a>
**recoveryObservation** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-registered-line-of-sight-implementation"></a>
**registered line-of-sight implementation** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-representative-attack"></a>
**representativeAttack** — value. Bounded teaching input with rifle raw damage `250000`, full `10/10` trace, range 3, retention `8000`, and suppression delta 12. **Declared at:** `SirCombat.representativeAttack`. **Related terms:** [AttackInput](#qnt-attack-input), [representativeDamageIsTwenty](#run-representative-damage-is-twenty). **Runtime correspondence:** exercised by exact and seeded sampled Q4 replay.

<a id="run-representative-damage-is-twenty"></a>
**representativeDamageIsTwenty** — run. Named witness from initialization through representative consequence resolution, asserting damage 20, raw arithmetic observations, health 80, suppression 12, and core invariants. **Declared at:** `SirCombatTests.representativeDamageIsTwenty`. **Related terms:** [representativeAttack](#qnt-representative-attack), [witness](#def-witness). **Runtime correspondence:** exact model result is included in the current Q4 qualification boundary.

<a id="qnt-resolve-consequences"></a>
**resolveConsequences** — action. Guarded atomic transition assigning both the next combat state and completed consequence observation. **Declared at:** `SirCombat.resolveConsequences`. **Related terms:** [validAttack](#qnt-valid-attack), [nextConsequences](#qnt-next-consequences), [Observation](#qnt-observation). **Runtime correspondence:** aggregate completed-consequence entry point in `CombatRules`.

<a id="qnt-resolve-cover-impact"></a>
**resolveCoverImpact** — action. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-resolve-recovery"></a>
**resolveRecovery** — action. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-restored-green"></a>
**restored green** — evidence. Successful re-execution against the untouched authority after the disposable mutated subject has failed and been discarded. **Declared at:** chapter 35 and focused audit. **Related terms:** [observed-red control](#def-observed-red-control), [mutation](#def-mutation). **Runtime correspondence:** full Q4 qualification returns green after its independent controls.

<a id="concept-retained-effect"></a>
**retained effect** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-retained-effect"></a>
**retainedEffect** — function. Clamps raw armor retention to `0..SCALE` before damage multiplication. **Declared at:** `SirCombat.retainedEffect`. **Related terms:** [armor retention](#stat-armor-retention), [expectedDamageRaw](#qnt-expected-damage-raw). **Runtime correspondence:** bounded retained-effect handling in damage resolution.

<a id="qnt-rifle-damage-raw"></a>
**rifleDamageRaw** — value. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="unit-round-half-away-from-zero"></a>
**round-half-away-from-zero** — unit. Tie-breaking rule in which an exact half advances one integer away from zero; `17.5 → 18` and `-17.5 → -18`. **Declared at:** `divideRoundedAwayFromZero`; positive final whole damage uses the `+SCALE/2` path. **Related terms:** [multiplyFixed](#qnt-multiply-fixed), [roundedDamage](#qnt-rounded-damage). **Runtime correspondence:** mirrors fixed-point rounding, subject to the named final-wrap boundary.

<a id="qnt-rounded-damage"></a>
**roundedDamage** — function. Converts positive raw damage to whole points after applying signed int32 wrap to `rawDamage + SCALE/2`; representative raw `200000` becomes `20`. **Declared at:** `SirCombat.roundedDamage`. **Related terms:** [expectedDamageRaw](#qnt-expected-damage-raw), [round-half-away-from-zero](#unit-round-half-away-from-zero), [claim boundary](#def-claim-boundary). **Runtime correspondence:** preserves the runtime's explicit pre-division int32-wrap behavior.

<a id="qnt-rule-catalogue"></a>
**ruleCatalogue** — value. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-rule-entry"></a>
**RuleEntry** — type. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-run"></a>
**run** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-safety-property"></a>
**safety property** — evidence. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-sampled-run"></a>
**sampled run** — evidence. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="unit-samples"></a>
**samples** — unit. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-saturate-int32"></a>
**saturateInt32** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-scale"></a>
**SCALE** — constant. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="unit-scale-10-000"></a>
**scale 10,000** — unit. Fixed-point denominator named `SCALE`; raw `10000` represents `1.0`, and raw/human conversion moves the decimal four places. **Declared at:** `SirCombat.SCALE`. **Related terms:** [Q4 raw integer](#unit-q4-raw-integer), [multiplyFixed](#qnt-multiply-fixed). **Runtime correspondence:** shared raw fixed-point scale in `SIR.Domain.FixedPoint`.

<a id="unit-seconds"></a>
**seconds** — unit. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-set"></a>
**set** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="unit-signed-32-bit-saturation"></a>
**signed 32-bit saturation** — unit. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-sir-combat"></a>
**SirCombat** — module. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-sir-combat-tests"></a>
**SirCombatTests** — module. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="property-sixteen-rules-declared"></a>
**sixteenRulesDeclared** — property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="catalogue-property-sixteen-rules-declared"></a>
**SixteenRulesDeclared** — catalogue property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-source-digest"></a>
**source digest** — evidence. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-state-transition"></a>
**state transition** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-state-variable"></a>
**state variable** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-step"></a>
**step** — action. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-stuttering"></a>
**stuttering** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-suppression"></a>
**suppression** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-suppression-delta"></a>
**suppression delta** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-suppression-eligibility"></a>
**suppression eligibility** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="unit-suppression-points"></a>
**suppression points** — unit. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-suppression-recovery"></a>
**suppression recovery** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-suppression-for-damage"></a>
**suppressionForDamage** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="run-suppression-needs-positive-damage-and-recovers-five"></a>
**suppressionNeedsPositiveDamageAndRecoversFive** — run. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="property-suppression-requires-damage"></a>
**suppressionRequiresDamage** — property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="catalogue-property-suppression-requires-damage"></a>
**SuppressionRequiresDamage** — catalogue property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-target-footprint"></a>
**target footprint** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-terminal-state"></a>
**terminal state** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-total-samples"></a>
**total samples** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-trace-probability"></a>
**trace probability** — stat. Ratio of visible to total samples after trace validity; representative `10/10` is raw `10000` (`1.0`). **Declared at:** `traceRaw`. **Related terms:** [traceRaw](#qnt-trace-raw), [samples](#unit-samples), [expected damage](#stat-expected-damage). **Runtime correspondence:** trace counts come from the registered external line-of-sight boundary.

<a id="qnt-trace-algorithm"></a>
**traceAlgorithm** — value. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-trace-raw"></a>
**traceRaw** — function. Converts valid visible/total integer samples into a scale-10,000 ratio with fixed-point rounding. **Declared at:** `SirCombat.traceRaw`. **Related terms:** [trace probability](#stat-trace-probability), [validTrace](#qnt-valid-trace), [fromRatio](#qnt-from-ratio). **Runtime correspondence:** models the output contract, not the external supercover traversal.

<a id="def-type"></a>
**type** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-valid-attack"></a>
**validAttack** — function. Guard requiring a target footprint and a valid visible/total sample pair before consequence resolution can fire. **Declared at:** `SirCombat.validAttack`. **Related terms:** [AttackInput](#qnt-attack-input), [validTrace](#qnt-valid-trace), [resolveConsequences](#qnt-resolve-consequences). **Runtime correspondence:** bounded precondition for the modeled aggregate transition.

<a id="qnt-valid-trace"></a>
**validTrace** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="property-valid-trace-observation"></a>
**validTraceObservation** — property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="catalogue-property-valid-trace-observation"></a>
**ValidTraceObservation** — catalogue property. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-variant"></a>
**variant** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-visible-samples"></a>
**visible samples** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="def-witness"></a>
**witness** — keyword. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="concept-wound"></a>
**wound** — concept. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-wound"></a>
**Wound** — type. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="stat-wound-threshold"></a>
**wound threshold** — stat. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="qnt-wound-for-damage"></a>
**woundForDamage** — function. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="run-wound-thresholds-are-exact"></a>
**woundThresholdsAreExact** — run. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

<a id="run-zero-health-means-incapacitated"></a>
**zeroHealthMeansIncapacitated** — run. Planned definition; its full explanation lands in the milestone named by the handbook hierarchy. **Declared at:** Pending. **Related terms:** Pending. **Runtime correspondence:** Pending.

[Back to the table of contents](#table-of-contents)
