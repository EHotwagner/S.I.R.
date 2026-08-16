namespace SIR.Domain

type CoherenceMode = Changed | Cone | Corpus
type ClaimStrength = ProvedStructural | ProvedFragment | ExhaustiveBounded | Tested | Heuristic | Unknown | Failed
type AnalysisTermination = Complete | WorkBudgetExhausted

type CoherenceBounds =
    { MaxWorkUnits: int32
      MaxFindings: int32
      MaxWitnessRules: int32 }

type CoherenceRequest =
    { Mode: CoherenceMode
      ChangedRuleIds: RuleId list
      Bounds: CoherenceBounds
      BlockUnknowns: bool }

type CoherenceWitness =
    { RuleIds: RuleId list
      Fact: string
      Expected: string
      Actual: string }

type CoherenceFinding =
    { Fingerprint: string
      Dimension: string
      Strength: ClaimStrength
      RuleIds: RuleId list
      Message: string
      DependencyReason: string
      Witness: CoherenceWitness option }

type CoherenceCost =
    { RulesInCorpus: int32
      RulesInSlice: int32
      CandidatePairs: int32
      PrunedPairs: int32
      WorkUnits: int32
      ExpensiveAnalyses: int32
      CacheHits: int32 }

type CoherenceCacheEntry =
    { Key: string
      Findings: CoherenceFinding list
      CandidatePairs: int32
      PrunedPairs: int32 }

type CoherenceReport =
    { ReportSchemaVersion: int32
      AnalyzerVersion: string
      Mode: CoherenceMode
      PackageManifestDigest: byte array
      AnalyzedRuleIds: RuleId list
      Findings: CoherenceFinding list
      PendingShards: string list
      Termination: AnalysisTermination
      CanonicalizationReady: bool
      Cost: CoherenceCost
      CacheEntry: CoherenceCacheEntry option }

[<RequireQualifiedAccess>]
module RuleCoherence =
    val defaultBounds: CoherenceBounds
    val analyze: packageIdentity: RulePackageIdentity -> rules: RuleDefinition list -> priorCache: CoherenceCacheEntry option -> request: CoherenceRequest -> CoherenceReport
    val reportJson: CoherenceReport -> string
    val canonicalReportBytes: CoherenceReport -> byte array
