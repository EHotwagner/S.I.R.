module Domain

open System

[<RequireQualifiedAccess>]
type Bearing =
    | Front
    | Flank
    | Rear

[<RequireQualifiedAccess>]
type EngagementKind =
    | Point
    | Area

type Position =
    { X: float
      Y: float }

type ArmourProfile =
    { Front: float
      Flank: float
      Rear: float }

type BodyProfile =
    { Name: string
      MaxHp: float
      Armour: ArmourProfile
      SuppressionResistance: float
      RegenerationPerSecond: float }

type WeaponProfile =
    { Name: string
      Kind: EngagementKind
      BaseEngagementSeconds: float
      RangeSlope: float
      RangeExponent: float
      Accuracy: float
      DispersionPerMeter: float
      Damage: float
      Penetration: float
      ShotsPerSecond: float
      EffectDensity: float
      SuppressionPerSecond: float }

type FixedBoardState =
    { Name: string
      Attacker: Position
      Target: Position
      TargetBody: BodyProfile
      Bearing: Bearing
      Exposure: float
      CoverProtection: float
      ExistingSuppression: float }

type BalanceParameters =
    { ExposureFloor: float
      SuppressionThreshold: float
      SuppressionEngagementPenalty: float }

[<RequireQualifiedAccess>]
type ArmourOutcome =
    | Stopped
    | PartiallyMitigated
    | Penetrated
    | Overmatched

type TrialResult =
    { Damage: float
      Suppression: float
      Incapacitated: bool }

type TrialSummary =
    { MeanDamage: float
      P10Damage: float
      P50Damage: float
      P90Damage: float
      MeanSuppression: float
      IncapacitationPercent: float }

let distance a b =
    let dx = b.X - a.X
    let dy = b.Y - a.Y
    sqrt (dx * dx + dy * dy)

let armourAt bearing armour =
    match bearing with
    | Bearing.Front -> armour.Front
    | Bearing.Flank -> armour.Flank
    | Bearing.Rear -> armour.Rear

let withFrontArmour value body =
    { body with
        Armour =
            { body.Armour with
                Front = value } }

/// Small deterministic generator used only for repeatable balance sampling.
type Rng(seed: uint64) =
    let mutable state = seed

    member _.NextFloat() =
        state <- state + 0x9E3779B97F4A7C15UL
        let mutable z = state
        z <- (z ^^^ (z >>> 30)) * 0xBF58476D1CE4E5B9UL
        z <- (z ^^^ (z >>> 27)) * 0x94D049BB133111EBUL
        z <- z ^^^ (z >>> 31)
        float (z >>> 11) * (1.0 / 9007199254740992.0)

    member this.Between(low: float, high: float) =
        low + (high - low) * this.NextFloat()
