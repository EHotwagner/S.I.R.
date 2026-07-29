(**
---
title: Executable Combat Formulas
category: Foundations
categoryindex: 2
index: 4
description: Read and execute the provisional F# combat equations against the same source used by the rules laboratory.
---
*)

(**
# Executable combat formulas

This is literate F#: every displayed formula is executable, and the site build
runs it. The current equations are **prototype balance models**, not accepted
match authority. Each documented function is checked against the implementation
loaded directly from `spikes/rules-lab`; if the source and explanation drift,
the documentation build fails.
*)

(*** hide ***)
#load "../spikes/rules-lab/Domain.fs"
#load "../spikes/rules-lab/Combat.fs"
#load "../spikes/rules-lab/Catalog.fs"

open System
open Domain

(**
## Engagement preparation

Preparation grows with range and existing suppression, while greater exposed
area makes a target easier to acquire. The clamp prevents a nearly invisible
sliver from making preparation unbounded.
*)

let engagementSeconds parameters weapon state =
    let range = distance state.Attacker state.Target
    let exposed =
        Combat.clamp parameters.ExposureFloor 1.0 state.Exposure

    let rangeTime =
        weapon.BaseEngagementSeconds
        + weapon.RangeSlope * Math.Pow(range, weapon.RangeExponent)

    let suppressionRatio =
        Combat.clamp
            0.0
            2.0
            (state.ExistingSuppression / parameters.SuppressionThreshold)

    let suppressionMultiplier =
        1.0
        + suppressionRatio * parameters.SuppressionEngagementPenalty

    rangeTime / sqrt exposed * suppressionMultiplier

(*** hide ***)
let close left right = abs (left - right) < 0.000_000_1

let engagementFixture =
    Catalog.state
        "Rifle against partial frontal exposure"
        Catalog.orc
        Bearing.Front
        25.0
        0.35
        22.0

let shownEngagement =
    engagementSeconds Catalog.parameters Catalog.rifle engagementFixture

let actualEngagement =
    Combat.engagementSeconds Catalog.parameters Catalog.rifle engagementFixture

if not (close shownEngagement actualEngagement) then
    failwith "The documented engagement formula drifted from Combat.engagementSeconds."

printfn "Rifle, 25 m, 35%% exposure: %.3f s preparation" shownEngagement
(*** include-output ***)

(**
The displayed implementation and the laboratory implementation produced the
same value during this build.

## Armour and retained effect

Penetration is compared with the protection of the first contacted layer.
Named bands remain useful for explanation, while a continuous retained-effect
curve avoids discontinuous balance jumps at a boundary.
*)

let retainedEffect penetration protection =
    if protection <= 0.0 then
        1.0
    else
        let ratio = max 0.0 (penetration / protection)

        if ratio <= 0.5 then
            0.05 * ratio / 0.5
        elif ratio <= 0.9 then
            0.05 + (ratio - 0.5) / 0.4 * 0.30
        elif ratio <= 1.4 then
            0.35 + (ratio - 0.9) / 0.5 * 0.50
        elif ratio <= 2.0 then
            0.85 + (ratio - 1.4) / 0.6 * 0.15
        else
            1.0

(*** hide ***)
let ratios = [ 0.25; 0.50; 0.90; 1.40; 2.00; 2.50 ]

for ratio in ratios do
    let shown = retainedEffect ratio 1.0
    let actual = Combat.retainedEffect ratio 1.0

    if not (close shown actual) then
        failwithf "The retained-effect formula drifted at ratio %.2f." ratio

    printfn "ratio %4.2f → retained effect %5.1f%%" ratio (shown * 100.0)
(*** include-output ***)

(**
## Expected damage per shot

The diagnostic combines the chance that a physical trace reaches the target,
nominal damage, an area-weapon density factor, and the effect retained after
cover and body armour. It is useful for comparison; it is not a replacement
for discrete traces in the final simulation.
*)

let traceProbability weapon state =
    let range = distance state.Attacker state.Target

    Combat.clamp
        0.0
        1.0
        (weapon.Accuracy
         * exp (-weapon.DispersionPerMeter * range))

let layerFactor weapon state =
    let bodyProtection =
        armourAt state.Bearing state.TargetBody.Armour

    let direct =
        retainedEffect weapon.Penetration bodyProtection

    if state.CoverProtection <= 0.0 then
        direct
    else
        let throughCover =
            retainedEffect weapon.Penetration state.CoverProtection

        let remainingPenetration =
            weapon.Penetration * (0.35 + 0.65 * throughCover)

        let afterArmour =
            retainedEffect remainingPenetration bodyProtection

        state.Exposure * direct
        + (1.0 - state.Exposure) * throughCover * afterArmour

let expectedDamagePerShot weapon state =
    traceProbability weapon state
    * weapon.Damage
    * weapon.EffectDensity
    * layerFactor weapon state

(*** hide ***)
let bearings = [ Bearing.Front; Bearing.Flank; Bearing.Rear ]

for bearing in bearings do
    let state = { engagementFixture with Bearing = bearing }
    let shown = expectedDamagePerShot Catalog.rifle state
    let actual = Combat.expectedDamagePerShot Catalog.rifle state

    if not (close shown actual) then
        failwithf "The expected-damage formula drifted for %A armour." bearing

    printfn "%-5A armour → %5.2f expected damage/shot" bearing shown
(*** include-output ***)

(**
## What is authoritative today?

The minimal shared simulation currently resolves an adjacent observed attack
with bounded integer attack power and saturating subtraction. The richer
functions above are explicitly a balance laboratory awaiting promotion. This
page makes that boundary visible while still letting readers inspect real code
and reproduced results.

Continue with [combat values and formula status](gameplay-formulas.md),
[weapons and equipment](gameplay-weapons-equipment.md), or run the
[interactive rules laboratory](interactive-rules-lab.md).
*)
