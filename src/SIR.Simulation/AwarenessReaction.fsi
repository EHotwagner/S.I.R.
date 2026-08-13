namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

[<RequireQualifiedAccess>]
type ObservationSector = Forward | Peripheral | Rear

[<RequireQualifiedAccess>]
type AwarenessLevel = Unknown | Suspected | Acquired | LostContact

[<RequireQualifiedAccess>]
type AwarenessReason = NoStimulus | OutsideRange | Occluded | StimulusAccumulated | IdentificationThresholdReached | StimulusDecayed | ContactLost | ContactRetained | InvalidProfile

type SensorProfile =
    { ProfileId: string
      MaximumRangeCells: int32
      ForwardContribution: int32
      PeripheralContribution: int32
      RearContribution: int32
      IdentificationThreshold: int32
      DecayPerTick: int32
      LastKnownRetentionTicks: int32
      MaximumExposureSamples: int32 }

type Stimulus =
    { ObserverId: UnitId
      SubjectId: UnitId
      Tick: int32
      Sector: ObservationSector
      Origin: Cell
      SubjectCell: Cell
      SpatialEvidence: SpatialQueryResult }

type AwarenessContact =
    { SubjectId: UnitId
      Level: AwarenessLevel
      Acquisition: int32
      LastStimulusTick: int32 option
      LastKnownCell: Cell option
      RetainUntilTick: int32 option
      Reason: AwarenessReason }

[<RequireQualifiedAccess>]
type EngagementTarget = KnownUnit of UnitId | CoveredArea of Cell list | GuardedEdge of edgeId: string * spatialRevision: int32 * Edge

[<RequireQualifiedAccess>]
type EngagementPhase = Preparing | ActiveCoverage | TriggerEligible | Committed | Resolved | Interrupted | Recovering

[<RequireQualifiedAccess>]
type ReactionTriggerKind = CoveredAreaEntered | GuardedEdgeCrossed | ValidTargetExposed

[<RequireQualifiedAccess>]
type ReactionReason = PreparingNotComplete | Eligible | CommittedInCanonicalOrder | TargetInvalidated | AttentionChanged | PostureChanged | ReactorIncapacitated | FireBlocked | ResolvedByPhysicalAuthority | RecoveryComplete

type Engagement =
    { EngagementId: string
      OwnerId: UnitId
      Target: EngagementTarget
      RequiredAttention: Direction8
      Phase: EngagementPhase
      RemainingTicks: int32
      Reason: ReactionReason }

type ReactionCandidate =
    { ReactorId: UnitId
      EngagementId: string
      TriggerKind: ReactionTriggerKind
      SourceId: UnitId
      SourceCell: Cell
      Tick: int32 }

type AwarenessCounters =
    { CandidatePairs: int32
      SectorSurvivors: int32
      LosEvaluations: int32
      Stimuli: int32
      AwarenessEpisodes: int32
      Engagements: int32
      ReactionCandidates: int32 }

[<RequireQualifiedAccess>]
module AwarenessReaction =
    val schemaVersion: int32
    val profileIdentity: string
    val orderingIdentity: string
    val infantryProfile: SensorProfile
    val validateProfile: SensorProfile -> Result<SensorProfile, string>
    val sector: attention: Direction8 -> origin: Cell -> subjectCell: Cell -> ObservationSector
    val evaluateVisualStimulus: ProjectedSpatialWorld -> SensorProfile -> tick: int32 -> observerId: UnitId -> attention: Direction8 -> origin: Cell -> subjectId: UnitId -> subjectCell: Cell -> Result<Stimulus option * AwarenessReason, string>
    val emptyContact: UnitId -> AwarenessContact
    val advanceContact: SensorProfile -> tick: int32 -> subjectCell: Cell -> Stimulus option -> AwarenessContact -> AwarenessContact
    val declareEngagement: engagementId: string -> ownerId: UnitId -> target: EngagementTarget -> requiredAttention: Direction8 -> Result<Engagement, string>
    val advanceEngagement: attentionAligned: bool -> postureReady: bool -> ownerCapable: bool -> triggerEligible: bool -> Engagement -> Engagement
    val orderCandidates: ReactionCandidate list -> ReactionCandidate list
    val canonicalContactBytes: AwarenessContact -> byte array
    val canonicalEngagementBytes: Engagement -> byte array
    val canonicalCandidateBytes: ReactionCandidate -> byte array
