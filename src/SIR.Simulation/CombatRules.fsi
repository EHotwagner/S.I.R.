namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

type CombatAttackInput =
    { Attacker: Cell
      TargetFootprint: Cell list
      IsTransparent: Cell -> bool
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
