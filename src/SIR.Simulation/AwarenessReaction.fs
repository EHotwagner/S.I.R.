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
    { ProfileId: string; MaximumRangeCells: int32; ForwardContribution: int32; PeripheralContribution: int32; RearContribution: int32; IdentificationThreshold: int32; DecayPerTick: int32; LastKnownRetentionTicks: int32; MaximumExposureSamples: int32 }

type Stimulus =
    { ObserverId: UnitId; SubjectId: UnitId; Tick: int32; Sector: ObservationSector; Origin: Cell; SubjectCell: Cell; SpatialEvidence: SpatialQueryResult }

type AwarenessContact =
    { SubjectId: UnitId; Level: AwarenessLevel; Acquisition: int32; LastStimulusTick: int32 option; LastKnownCell: Cell option; RetainUntilTick: int32 option; Reason: AwarenessReason }

[<RequireQualifiedAccess>]
type EngagementTarget = KnownUnit of UnitId | CoveredArea of Cell list | GuardedEdge of Edge

[<RequireQualifiedAccess>]
type EngagementPhase = Preparing | ActiveCoverage | TriggerEligible | Committed | Resolved | Interrupted | Recovering

[<RequireQualifiedAccess>]
type ReactionTriggerKind = CoveredAreaEntered | GuardedEdgeCrossed | ValidTargetExposed

[<RequireQualifiedAccess>]
type ReactionReason = PreparingNotComplete | Eligible | CommittedInCanonicalOrder | TargetInvalidated | AttentionChanged | PostureChanged | ReactorIncapacitated | FireBlocked | ResolvedByPhysicalAuthority | RecoveryComplete

type Engagement =
    { EngagementId: string; OwnerId: UnitId; Target: EngagementTarget; RequiredAttention: Direction8; Phase: EngagementPhase; RemainingTicks: int32; Reason: ReactionReason }

type ReactionCandidate =
    { ReactorId: UnitId; EngagementId: string; TriggerKind: ReactionTriggerKind; SourceId: UnitId; SourceCell: Cell; Tick: int32 }

type AwarenessCounters =
    { CandidatePairs: int32; SectorSurvivors: int32; LosEvaluations: int32; Stimuli: int32; AwarenessEpisodes: int32; Engagements: int32; ReactionCandidates: int32 }

[<RequireQualifiedAccess>]
module AwarenessReaction =
    let schemaVersion = 1
    let profileIdentity = "sir-awareness-sensor-infantry-v1"
    let orderingIdentity = "sir-awareness-reaction-order-v1"

    let infantryProfile =
        { ProfileId = profileIdentity; MaximumRangeCells = 60; ForwardContribution = 4; PeripheralContribution = 2; RearContribution = 1; IdentificationThreshold = 8; DecayPerTick = 2; LastKnownRetentionTicks = 20; MaximumExposureSamples = 4 }

    let validateProfile profile =
        if System.String.IsNullOrWhiteSpace profile.ProfileId then Error "Sensor profile identity is required."
        elif profile.MaximumRangeCells <= 0 || profile.MaximumRangeCells > 4096 then Error "Sensor range must be between 1 and 4096 cells."
        elif profile.ForwardContribution <= 0 || profile.PeripheralContribution <= 0 || profile.RearContribution <= 0 then Error "Every observation sector requires a positive contribution."
        elif profile.IdentificationThreshold <= 0 || profile.IdentificationThreshold > 4096 then Error "Identification threshold must be between 1 and 4096."
        elif profile.DecayPerTick <= 0 || profile.DecayPerTick > profile.IdentificationThreshold then Error "Awareness decay must be positive and no greater than the threshold."
        elif profile.LastKnownRetentionTicks < 0 || profile.LastKnownRetentionTicks > 4096 then Error "Last-known retention must be between 0 and 4096 ticks."
        elif profile.MaximumExposureSamples <= 0 || profile.MaximumExposureSamples > 256 then Error "Exposure samples must be between 1 and 256."
        else Ok profile

    let private chebyshev left right = max (abs (int64 right.Col - int64 left.Col)) (abs (int64 right.Row - int64 left.Row))

    let sector attention origin subjectCell =
        match Direction8.tryFromDelta (subjectCell.Col - origin.Col) (subjectCell.Row - origin.Row) with
        | None -> ObservationSector.Forward
        | Some direction ->
            let distance = abs (int (Direction8.toCode direction) - int (Direction8.toCode attention))
            let wrapped = min distance (8 - distance)
            if wrapped <= 1 then ObservationSector.Forward elif wrapped = 2 then ObservationSector.Peripheral else ObservationSector.Rear

    let evaluateVisualStimulus world profile tick observerId attention origin subjectId subjectCell =
        match validateProfile profile with
        | Error error -> Error error
        | Ok profile when chebyshev origin subjectCell > int64 profile.MaximumRangeCells -> Ok(None, AwarenessReason.OutsideRange)
        | Ok profile ->
            let observedSector = sector attention origin subjectCell
            let request =
                { QueryId = $"awareness:{tick}:{UnitId.value observerId}:{UnitId.value subjectId}"
                  QueryKind = SpatialQueryKind.ExactLineOfSight
                  Origin = origin
                  Target = subjectCell
                  Footprint = [ { Col = 0; Row = 0 } ]
                  Profile = { ProfileId = profile.ProfileId; Modality = SpatialModality.Vision; Stance = "sensor"; HeightBand = 1; Facing = attention }
                  Bounds = { SpatialQuery.defaultBounds with MaximumFootprintSamples = profile.MaximumExposureSamples } }
            let result, _ = SpatialQuery.evaluate world request
            if result.Outcome = SpatialOutcome.Found && result.Visible then
                Ok(Some { ObserverId = observerId; SubjectId = subjectId; Tick = tick; Sector = observedSector; Origin = origin; SubjectCell = subjectCell; SpatialEvidence = result }, AwarenessReason.StimulusAccumulated)
            else Ok(None, AwarenessReason.Occluded)

    let emptyContact subjectId =
        { SubjectId = subjectId; Level = AwarenessLevel.Unknown; Acquisition = 0; LastStimulusTick = None; LastKnownCell = None; RetainUntilTick = None; Reason = AwarenessReason.NoStimulus }

    let private contribution profile sector =
        match sector with ObservationSector.Forward -> profile.ForwardContribution | ObservationSector.Peripheral -> profile.PeripheralContribution | ObservationSector.Rear -> profile.RearContribution

    let advanceContact profile tick subjectCell stimulus contact =
        match stimulus with
        | Some stimulus ->
            let next = min profile.IdentificationThreshold (contact.Acquisition + contribution profile stimulus.Sector)
            let acquired = next >= profile.IdentificationThreshold
            { contact with
                Acquisition = next
                Level = if acquired then AwarenessLevel.Acquired else AwarenessLevel.Suspected
                LastStimulusTick = Some tick
                LastKnownCell = if acquired then Some subjectCell else contact.LastKnownCell
                RetainUntilTick = if acquired then Some(tick + profile.LastKnownRetentionTicks) else contact.RetainUntilTick
                Reason = if acquired then AwarenessReason.IdentificationThresholdReached else AwarenessReason.StimulusAccumulated }
        | None ->
            let next = max 0 (contact.Acquisition - profile.DecayPerTick)
            match contact.Level, contact.RetainUntilTick with
            | AwarenessLevel.Acquired, _ -> { contact with Acquisition = next; Level = AwarenessLevel.LostContact; Reason = AwarenessReason.ContactLost }
            | AwarenessLevel.LostContact, Some retainUntil when tick <= retainUntil -> { contact with Acquisition = next; Reason = AwarenessReason.ContactRetained }
            | AwarenessLevel.LostContact, _ -> { contact with Acquisition = next; Level = AwarenessLevel.Unknown; LastKnownCell = None; RetainUntilTick = None; Reason = AwarenessReason.StimulusDecayed }
            | _, _ ->
                { contact with
                    Acquisition = next
                    Level = if next = 0 then AwarenessLevel.Unknown else AwarenessLevel.Suspected
                    Reason = AwarenessReason.StimulusDecayed }

    let private normalizeTarget target =
        match target with
        | EngagementTarget.CoveredArea cells ->
            let normalized = cells |> List.distinct |> List.sortBy (fun cell -> cell.Row, cell.Col)
            if List.isEmpty normalized then Error "Covered area requires at least one cell."
            elif normalized.Length > 256 then Error "Covered area exceeds the 256-cell limit."
            else Ok(EngagementTarget.CoveredArea normalized)
        | EngagementTarget.GuardedEdge edge when edge.Lo = edge.Hi -> Error "Guarded edge endpoints must differ."
        | other -> Ok other

    let declareEngagement engagementId ownerId target requiredAttention =
        if System.String.IsNullOrWhiteSpace engagementId then Error "Engagement identity is required."
        else normalizeTarget target |> Result.map (fun normalized -> { EngagementId = engagementId; OwnerId = ownerId; Target = normalized; RequiredAttention = requiredAttention; Phase = EngagementPhase.Preparing; RemainingTicks = 2; Reason = ReactionReason.PreparingNotComplete })

    let advanceEngagement attentionAligned postureReady ownerCapable triggerEligible engagement =
        if not ownerCapable then { engagement with Phase = EngagementPhase.Interrupted; RemainingTicks = 0; Reason = ReactionReason.ReactorIncapacitated }
        elif not attentionAligned then { engagement with Phase = EngagementPhase.Interrupted; RemainingTicks = 0; Reason = ReactionReason.AttentionChanged }
        elif not postureReady then { engagement with Phase = EngagementPhase.Interrupted; RemainingTicks = 0; Reason = ReactionReason.PostureChanged }
        else
            match engagement.Phase with
            | EngagementPhase.Preparing when engagement.RemainingTicks > 1 -> { engagement with RemainingTicks = engagement.RemainingTicks - 1; Reason = ReactionReason.PreparingNotComplete }
            | EngagementPhase.Preparing -> { engagement with Phase = EngagementPhase.ActiveCoverage; RemainingTicks = 0; Reason = ReactionReason.Eligible }
            | EngagementPhase.ActiveCoverage when triggerEligible -> { engagement with Phase = EngagementPhase.TriggerEligible; Reason = ReactionReason.Eligible }
            | EngagementPhase.TriggerEligible -> { engagement with Phase = EngagementPhase.Committed; RemainingTicks = 1; Reason = ReactionReason.CommittedInCanonicalOrder }
            | EngagementPhase.Committed -> { engagement with Phase = EngagementPhase.Resolved; RemainingTicks = 1; Reason = ReactionReason.ResolvedByPhysicalAuthority }
            | EngagementPhase.Resolved -> { engagement with Phase = EngagementPhase.Recovering; RemainingTicks = 4; Reason = ReactionReason.ResolvedByPhysicalAuthority }
            | EngagementPhase.Recovering when engagement.RemainingTicks > 1 -> { engagement with RemainingTicks = engagement.RemainingTicks - 1 }
            | EngagementPhase.Recovering -> { engagement with Phase = EngagementPhase.ActiveCoverage; RemainingTicks = 0; Reason = ReactionReason.RecoveryComplete }
            | _ -> engagement

    let private triggerCode = function ReactionTriggerKind.CoveredAreaEntered -> 0 | ReactionTriggerKind.GuardedEdgeCrossed -> 1 | ReactionTriggerKind.ValidTargetExposed -> 2
    let orderCandidates candidates = candidates |> List.distinct |> List.sortBy (fun c -> UnitId.value c.ReactorId, c.EngagementId, triggerCode c.TriggerKind, UnitId.value c.SourceId)

    let private i32 = CanonicalEncoding.int32LittleEndian
    let private text (value: string) = let bytes = System.Text.Encoding.UTF8.GetBytes value in CanonicalEncoding.concatenate [ i32 bytes.Length; bytes ]
    let private cell (value: Cell) = CanonicalEncoding.concatenate [ i32 value.Col; i32 value.Row ]
    let private optionI32 (value: int32 option) = match value with None -> [| 0uy |] | Some item -> CanonicalEncoding.concatenate [ [| 1uy |]; i32 item ]
    let private optionCell (value: Cell option) = match value with None -> [| 0uy |] | Some item -> CanonicalEncoding.concatenate [ [| 1uy |]; cell item ]
    let private levelCode = function AwarenessLevel.Unknown -> 0uy | AwarenessLevel.Suspected -> 1uy | AwarenessLevel.Acquired -> 2uy | AwarenessLevel.LostContact -> 3uy
    let private awarenessReasonCode = function AwarenessReason.NoStimulus -> 0uy | AwarenessReason.OutsideRange -> 1uy | AwarenessReason.Occluded -> 2uy | AwarenessReason.StimulusAccumulated -> 3uy | AwarenessReason.IdentificationThresholdReached -> 4uy | AwarenessReason.StimulusDecayed -> 5uy | AwarenessReason.ContactLost -> 6uy | AwarenessReason.ContactRetained -> 7uy | AwarenessReason.InvalidProfile -> 8uy
    let canonicalContactBytes (contact: AwarenessContact) =
        // Contacts dominate large-state snapshots. Encode the fixed schema into
        // one exact-sized array rather than allocating a segment graph per row.
        let optionI32Length = function None -> 1 | Some _ -> 5
        let optionCellLength = function None -> 1 | Some _ -> 9
        let bytes =
            Array.zeroCreate<byte>
                (11
                 + optionI32Length contact.LastStimulusTick
                 + optionCellLength contact.LastKnownCell
                 + optionI32Length contact.RetainUntilTick)
        let mutable offset = 0
        let writeByte value = bytes[offset] <- value; offset <- offset + 1
        let writeI32 (value: int32) =
            bytes[offset] <- byte value
            bytes[offset + 1] <- byte (value >>> 8)
            bytes[offset + 2] <- byte (value >>> 16)
            bytes[offset + 3] <- byte (value >>> 24)
            offset <- offset + 4
        let writeOptionI32 = function
            | None -> writeByte 0uy
            | Some value -> writeByte 1uy; writeI32 value
        let writeOptionCell = function
            | None -> writeByte 0uy
            | Some value -> writeByte 1uy; writeI32 value.Col; writeI32 value.Row
        writeByte (byte schemaVersion)
        writeI32 (UnitId.value contact.SubjectId)
        writeByte (levelCode contact.Level)
        writeI32 contact.Acquisition
        writeOptionI32 contact.LastStimulusTick
        writeOptionCell contact.LastKnownCell
        writeOptionI32 contact.RetainUntilTick
        writeByte (awarenessReasonCode contact.Reason)
        bytes

    let private targetBytes = function
        | EngagementTarget.KnownUnit id -> CanonicalEncoding.concatenate [ [| 0uy |]; i32 (UnitId.value id) ]
        | EngagementTarget.CoveredArea cells -> CanonicalEncoding.concatenate ([ [| 1uy |]; i32 cells.Length ] @ (cells |> List.map cell))
        | EngagementTarget.GuardedEdge edge -> CanonicalEncoding.concatenate [ [| 2uy |]; cell edge.Lo; cell edge.Hi ]
    let private phaseCode = function EngagementPhase.Preparing -> 0uy | EngagementPhase.ActiveCoverage -> 1uy | EngagementPhase.TriggerEligible -> 2uy | EngagementPhase.Committed -> 3uy | EngagementPhase.Resolved -> 4uy | EngagementPhase.Interrupted -> 5uy | EngagementPhase.Recovering -> 6uy
    let private reactionReasonCode = function ReactionReason.PreparingNotComplete -> 0uy | ReactionReason.Eligible -> 1uy | ReactionReason.CommittedInCanonicalOrder -> 2uy | ReactionReason.TargetInvalidated -> 3uy | ReactionReason.AttentionChanged -> 4uy | ReactionReason.PostureChanged -> 5uy | ReactionReason.ReactorIncapacitated -> 6uy | ReactionReason.FireBlocked -> 7uy | ReactionReason.ResolvedByPhysicalAuthority -> 8uy | ReactionReason.RecoveryComplete -> 9uy
    let canonicalEngagementBytes (engagement: Engagement) = CanonicalEncoding.concatenate [ [| byte schemaVersion |]; text engagement.EngagementId; i32 (UnitId.value engagement.OwnerId); targetBytes engagement.Target; CanonicalEncoding.direction8 engagement.RequiredAttention; [| phaseCode engagement.Phase |]; i32 engagement.RemainingTicks; [| reactionReasonCode engagement.Reason |] ]
    let canonicalCandidateBytes (candidate: ReactionCandidate) = CanonicalEncoding.concatenate [ [| byte schemaVersion |]; i32 (UnitId.value candidate.ReactorId); text candidate.EngagementId; [| byte (triggerCode candidate.TriggerKind) |]; i32 (UnitId.value candidate.SourceId); cell candidate.SourceCell; i32 candidate.Tick ]
