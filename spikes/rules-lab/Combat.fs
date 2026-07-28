module Combat

open System
open Domain

let clamp low high value = max low (min high value)

let engagementSeconds parameters weapon state =
    let range = distance state.Attacker state.Target
    let exposed = clamp parameters.ExposureFloor 1.0 state.Exposure
    let rangeTime =
        weapon.BaseEngagementSeconds
        + weapon.RangeSlope * Math.Pow(range, weapon.RangeExponent)
    let suppressionRatio =
        clamp 0.0 2.0 (state.ExistingSuppression / parameters.SuppressionThreshold)
    let suppressionMultiplier =
        1.0 + suppressionRatio * parameters.SuppressionEngagementPenalty
    rangeTime / sqrt exposed * suppressionMultiplier

let armourOutcome penetration protection =
    if protection <= 0.0 || penetration / protection > 1.4 then
        ArmourOutcome.Overmatched
    elif penetration / protection > 0.9 then
        ArmourOutcome.Penetrated
    elif penetration / protection > 0.5 then
        ArmourOutcome.PartiallyMitigated
    else
        ArmourOutcome.Stopped

/// Continuous retained-effect curve whose named bands correspond to the
/// canonical qualitative armour outcomes.
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

let traceProbability weapon state =
    let range = distance state.Attacker state.Target
    clamp 0.0 1.0 (weapon.Accuracy * exp (-weapon.DispersionPerMeter * range))

let private damageFactor penetration state =
    let bodyProtection = armourAt state.Bearing state.TargetBody.Armour
    let direct = retainedEffect penetration bodyProtection

    if state.CoverProtection <= 0.0 then
        direct
    else
        let throughCover = retainedEffect penetration state.CoverProtection
        let remainingPenetration = penetration * (0.35 + 0.65 * throughCover)
        let afterArmour = retainedEffect remainingPenetration bodyProtection
        state.Exposure * direct + (1.0 - state.Exposure) * throughCover * afterArmour

let expectedDamagePerShot weapon state =
    traceProbability weapon state
    * weapon.Damage
    * weapon.EffectDensity
    * damageFactor weapon.Penetration state

let expectedDamagePerSecond weapon state =
    expectedDamagePerShot weapon state * weapon.ShotsPerSecond

let expectedSuppression parameters weapon state activeSeconds =
    let exposureFactor = 0.6 + 0.4 * clamp 0.0 1.0 state.Exposure
    weapon.SuppressionPerSecond
    * max 0.0 activeSeconds
    * exposureFactor
    / max 0.1 state.TargetBody.SuppressionResistance
    |> min (parameters.SuppressionThreshold * 2.0)

let shotsResolved parameters weapon state windowSeconds =
    let active = windowSeconds - engagementSeconds parameters weapon state
    if active < 0.0 then
        0
    else
        1 + int (floor (active * weapon.ShotsPerSecond))

let expectedTimeToIncapacitation parameters weapon state =
    let netDamagePerSecond =
        expectedDamagePerSecond weapon state
        - state.TargetBody.RegenerationPerSecond
    if netDamagePerSecond <= 0.0 then
        Double.PositiveInfinity
    else
        engagementSeconds parameters weapon state
        + state.TargetBody.MaxHp / netDamagePerSecond

let private coverPathDamageFactor (rng: Rng) penetration state =
    let variedPenetration = penetration * rng.Between(0.85, 1.15)
    let throughCover = retainedEffect variedPenetration state.CoverProtection
    let remainingPenetration =
        variedPenetration * (0.35 + 0.65 * throughCover)
    let afterArmour =
        retainedEffect
            remainingPenetration
            (armourAt state.Bearing state.TargetBody.Armour)
    throughCover * afterArmour

let runTrial parameters weapon state windowSeconds (rng: Rng) =
    let engagement = engagementSeconds parameters weapon state
    let activeSeconds = max 0.0 (windowSeconds - engagement)
    let shotCount = shotsResolved parameters weapon state windowSeconds
    let mutable damage = 0.0

    for _ in 1 .. shotCount do
        if rng.NextFloat() <= traceProbability weapon state then
            let exposed =
                state.CoverProtection <= 0.0
                || rng.NextFloat() <= clamp 0.0 1.0 state.Exposure
            let factor =
                if exposed then
                    retainedEffect
                        (weapon.Penetration * rng.Between(0.85, 1.15))
                        (armourAt state.Bearing state.TargetBody.Armour)
                else
                    coverPathDamageFactor rng weapon.Penetration state
            damage <-
                damage
                + weapon.Damage
                  * weapon.EffectDensity
                  * rng.Between(0.85, 1.15)
                  * factor

    let regenerated = state.TargetBody.RegenerationPerSecond * activeSeconds
    let finalDamage = max 0.0 (damage - regenerated)
    let suppression = expectedSuppression parameters weapon state activeSeconds
    { Damage = finalDamage
      Suppression = suppression
      Incapacitated = finalDamage >= state.TargetBody.MaxHp }

let private percentile p (values: float array) =
    let sorted = Array.sort values
    if sorted.Length = 0 then
        0.0
    else
        let index =
            int (Math.Round(p * float (sorted.Length - 1), MidpointRounding.AwayFromZero))
        sorted.[clamp 0 (sorted.Length - 1) index]

let sample parameters weapon state windowSeconds samples seed =
    let rng = Rng(seed)
    let trials =
        Array.init samples (fun _ -> runTrial parameters weapon state windowSeconds rng)
    let damage = trials |> Array.map (fun result -> result.Damage)
    { MeanDamage = if damage.Length = 0 then 0.0 else Array.average damage
      P10Damage = percentile 0.10 damage
      P50Damage = percentile 0.50 damage
      P90Damage = percentile 0.90 damage
      MeanSuppression =
        if trials.Length = 0 then 0.0
        else trials |> Array.averageBy (fun result -> result.Suppression)
      IncapacitationPercent =
        if trials.Length = 0 then 0.0
        else
            trials
            |> Array.averageBy (fun result -> if result.Incapacitated then 100.0 else 0.0) }
