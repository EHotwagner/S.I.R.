namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

type CombatAttackInput =
    { Attacker: Cell
      TargetFootprint: Cell list
      VisibleSamples: int32
      TotalSamples: int32
      RangeCells: int32
      Suppression: FixedPoint
      BaseDamage: FixedPoint
      ArmorRetention: FixedPoint
      EventId: string }

type CombatAttackResult =
    { Preparation: FixedPoint
      TraceProbability: FixedPoint
      ArmorRetention: FixedPoint
      ExpectedDamage: int32
      Explanation: RuleApplication }

type CombatConsequences =
    { Damage: int32
      RemainingHealth: int32
      WoundSeverityCode: int32 option
      Incapacitated: bool
      SuppressionDelta: int32
      TotalSuppression: int32
      Explanation: RuleApplication }

type CombatCoverImpact =
    { Damage: int32
      RemainingIntegrity: int32
      Destroyed: bool
      StopsProjectile: bool
      Explanation: RuleApplication }

type RuleReplayBinding =
    { BoundEngineIdentity: string
      BoundCompatibilityProfile: string
      BoundPackageVersion: string
      BoundSourceCommit: string
      BoundImplementationDigest: byte array
      BoundSemanticDigest: byte array
      BoundManifestDigest: byte array
      BoundExplanation: RuleApplication }

type RetainedRulePackage =
    { Identity: RulePackageIdentity
      ManifestJson: string
      CoverageJson: string }

type HistoricalRuleResolution =
    | ResolvedHistoricalRulePackage of RetainedRulePackage
    | HistoricalRulePackageUnavailable of manifestDigest: byte array

[<RequireQualifiedAccess>]
module CombatRules =
    val registry: RuleDefinition list
    val implementationArtifacts: (string * byte array) list
    val packageIdentity: RulePackageIdentity
    val retainedPackage: RetainedRulePackage
    val replayBinding: RuleApplication -> RuleReplayBinding
    val resolveHistoricalPackage: RetainedRulePackage list -> RuleReplayBinding -> HistoricalRuleResolution
    val resolveAttack: CombatAttackInput -> Result<CombatAttackResult, string>
    val resolveConsequences: currentHealth: int32 -> currentSuppression: int32 -> suppressionDelta: int32 -> CombatAttackInput -> Result<CombatConsequences, string>
    val resolveCoverImpact: baseDamage: int32 -> currentIntegrity: int32 -> projectileBlocking: bool -> directAttack: bool -> eventId: string -> CombatCoverImpact
    val resolveRecovery: currentSuppression: int32 -> entityId: string -> int32 * RuleApplication option
