namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

[<RequireQualifiedAccess>]
type DamageType = Ballistic | Explosive | AntiArmor

[<RequireQualifiedAccess>]
type WeaponProfile = Rifle | SupportWeapon | AntiArmor | LobbedArea

type WeaponParameters =
    { Profile: WeaponProfile
      DamageType: DamageType
      BaseDamage: int32
      Penetration: int32
      Suppression: int32
      RangeCells: int32
      AreaRadius: int32
      Lobbed: bool }

[<RequireQualifiedAccess>]
type ArmorArc = Front | RearOrFlank

type ArmorState =
    { FrontRating: int32
      RearRating: int32
      Integrity: int32 }

[<RequireQualifiedAccess>]
type WoundSeverity = Serious | Critical

type Wound =
    { AttackId: string
      Severity: WoundSeverity
      Damage: int32 }

type CombatantState =
    { EntityId: string
      Faction: string
      Cell: Cell
      Facing: Direction8
      Health: int32
      Armor: ArmorState
      Wounds: Wound list
      Incapacitated: bool
      Suppression: int32 }

type CoverState =
    { CoverId: string
      Cell: Cell
      Integrity: int32
      ProjectileBlocking: bool
      Material: string
      PenetrationResistance: int32
      ProtectedDirections: Direction8 list }

type CombatWorld =
    { Spatial: ProjectedSpatialWorld
      Combatants: Map<string, CombatantState>
      Covers: Map<string, CoverState> }

type CombatLimits =
    { MaximumTraceCells: int32
      MaximumAreaCells: int32
      MaximumRecipients: int32
      MaximumFacts: int32
      MaximumExplanationBytes: int32 }

type CombatRequest =
    { AttackId: string
      AttackerId: string
      AimCell: Cell
      Weapon: WeaponProfile
      Limits: CombatLimits }

[<RequireQualifiedAccess>]
type CombatRejection =
    | InvalidRequest of string
    | Ineligible of string
    | OutOfRange of distance: int32 * maximum: int32
    | SpatialUnavailable of SpatialOutcome
    | LimitExceeded of name: string * observed: int32 * maximum: int32

[<RequireQualifiedAccess>]
type CombatFact =
    | Eligible of attackerId: string
    | Committed of profile: WeaponProfile * preparationRaw: int32
    | TraceEvaluated of crossedCells: Cell list * crossedEdges: Edge list * spatialBytes: byte array
    | Contact of entityId: string * cell: Cell * traceIndex: int32
    | CoverResolved of entityId: string * coverId: string option * retainedPercent: int32
    | ArmorResolved of entityId: string * arc: ArmorArc * effectiveRating: int32 * penetration: int32 * retainedPercent: int32
    | HealthChanged of entityId: string * damage: int32 * remainingHealth: int32
    | WoundApplied of entityId: string * severity: WoundSeverity
    | Incapacitated of entityId: string
    | SuppressionChanged of entityId: string * delta: int32 * total: int32
    | CoverDamaged of coverId: string * damage: int32 * remainingIntegrity: int32
    | CoverDestroyed of coverId: string

type CombatResult =
    { SchemaVersion: int32
      Request: CombatRequest
      Parameters: WeaponParameters
      World: CombatWorld
      Facts: CombatFact list
      SpatialEvidence: SpatialQueryResult option
      RuleApplications: RuleApplication list }

[<RequireQualifiedAccess>]
module Combat =
    val schemaVersion: int32
    val compatibilityProfile: string
    val defaultLimits: CombatLimits
    val parameters: WeaponProfile -> WeaponParameters
    val environmentCovers: AssembledEnvironment -> Map<string, CoverState>
    val suppressionEffectivenessPercent: suppression: int32 -> int32
    val suppressionTimingPercent: suppression: int32 -> int32
    val resolve: CombatWorld -> CombatRequest -> Result<CombatResult, CombatRejection>
    val recover: CombatWorld -> CombatWorld * CombatFact list
    val canonicalFactsBytes: CombatFact list -> byte array
    val canonicalResultBytes: CombatResult -> byte array
