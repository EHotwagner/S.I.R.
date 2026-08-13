namespace SIR.Conformance

open FS.GG.Game.Core
open SIR.Domain
open SIR.Simulation

[<RequireQualifiedAccess>]
module AwarenessReactionFixtures =
    let private require condition message = if not condition then failwith message
    let private cell col row: Cell = { Col = col; Row = row }
    let private unitId value = UnitId.create value
    let private identity revision = SpatialAuthorityIdentity.create "awareness-fixture" "sir-awareness-v1" revision "observer-local" revision |> Result.defaultWith failwith
    let private world revision boundaries =
        { Identity = identity revision
          Minimum = cell 0 0
          Maximum = cell 12 12
          Terrain = Map.empty
          Boundaries = boundaries
          Occupancy = Map.empty
          DisclosedRevisionTokens = Set.empty }

    let evaluate mutation =
        let profile = AwarenessReaction.infantryProfile
        require (AwarenessReaction.validateProfile profile = Ok profile) "The canonical sensor profile is invalid."
        require (AwarenessReaction.sector North (cell 4 4) (cell 4 1) = ObservationSector.Forward) "Forward sector changed."
        require (AwarenessReaction.sector North (cell 4 4) (cell 7 4) = ObservationSector.Peripheral) "Peripheral sector changed."
        require (AwarenessReaction.sector North (cell 4 4) (cell 4 7) = ObservationSector.Rear) "Rear sector changed."

        let attention = if mutation = Some "facing-attention" then North else East
        require (AwarenessReaction.sector attention (cell 4 4) (cell 7 4) = ObservationSector.Forward) "Attention direction was collapsed into body facing."

        let observer = unitId 10
        let subject = unitId 20
        let stimulus, reason = AwarenessReaction.evaluateVisualStimulus (world 1L []) profile 1 observer East (cell 1 1) subject (cell 4 1) |> Result.defaultWith failwith
        require (reason = AwarenessReason.StimulusAccumulated && stimulus.IsSome) "Visible geometry did not produce a factual stimulus."
        let first = AwarenessReaction.advanceContact profile 1 (cell 4 1) stimulus (AwarenessReaction.emptyContact subject)
        let first = if mutation = Some "los-awareness" then { first with Level = AwarenessLevel.Acquired } else first
        require (first.Level = AwarenessLevel.Suspected && first.LastKnownCell.IsNone) "One LOS stimulus implied immediate identification."
        let acquired = AwarenessReaction.advanceContact profile 2 (cell 4 1) stimulus first
        require (acquired.Level = AwarenessLevel.Acquired && acquired.LastKnownCell = Some(cell 4 1)) "Delayed acquisition did not reach the declared threshold."
        let lost = AwarenessReaction.advanceContact profile 3 (cell 5 1) None acquired
        require (lost.Level = AwarenessLevel.LostContact && lost.LastKnownCell = Some(cell 4 1)) "Lost contact leaked current world position or discarded last-known state."

        let wall =
            let edge = Edges.edgeBetween (cell 2 1) (cell 3 1) |> Option.defaultWith (fun () -> failwith "Fixture edge is not canonical.")
            { Edge = edge; Permeability = { Ground = true; Vision = false; Projectile = true }; RevisionToken = "closed-door" }
        let occluded, occludedReason = AwarenessReaction.evaluateVisualStimulus (world 2L [ wall ]) profile 4 observer East (cell 1 1) subject (cell 4 1) |> Result.defaultWith failwith
        require (occluded.IsNone && occludedReason = AwarenessReason.Occluded) "Occlusion produced a visual stimulus."

        let area = EngagementTarget.CoveredArea [ cell 3 2; cell 2 2; cell 3 2 ]
        let declared = AwarenessReaction.declareEngagement "eng-area" observer area East |> Result.defaultWith failwith
        let declared = if mutation = Some "preparation" then { declared with RemainingTicks = 1 } else declared
        let preparing = AwarenessReaction.advanceEngagement true true true false declared
        require (preparing.Phase = EngagementPhase.Preparing && preparing.RemainingTicks = 1) "Engagement fired without completing preparation."
        let active = AwarenessReaction.advanceEngagement true true true false preparing
        require (active.Phase = EngagementPhase.ActiveCoverage) "Prepared engagement did not become active coverage."
        let eligible = AwarenessReaction.advanceEngagement true true true true active
        let committed = AwarenessReaction.advanceEngagement true true true true eligible
        let resolved = AwarenessReaction.advanceEngagement true true true true committed
        require (eligible.Phase = EngagementPhase.TriggerEligible && committed.Phase = EngagementPhase.Committed && resolved.Phase = EngagementPhase.Resolved) "Reaction phase ordering changed."
        let interrupted = AwarenessReaction.advanceEngagement false true true true active
        require (interrupted.Phase = EngagementPhase.Interrupted && interrupted.Reason = ReactionReason.AttentionChanged) "Attention loss did not interrupt coverage."

        let candidates =
            [ { ReactorId = unitId 20; EngagementId = "b"; TriggerKind = ReactionTriggerKind.ValidTargetExposed; SourceId = unitId 10; SourceCell = cell 1 1; Tick = 5 }
              { ReactorId = unitId 10; EngagementId = "z"; TriggerKind = ReactionTriggerKind.GuardedEdgeCrossed; SourceId = unitId 30; SourceCell = cell 3 1; Tick = 5 }
              { ReactorId = unitId 10; EngagementId = "a"; TriggerKind = ReactionTriggerKind.CoveredAreaEntered; SourceId = unitId 20; SourceCell = cell 2 2; Tick = 5 } ]
        let ordered = AwarenessReaction.orderCandidates candidates
        let ordered = if mutation = Some "ordering" then List.rev ordered else ordered
        require (ordered |> List.map (fun item -> UnitId.value item.ReactorId, item.EngagementId) = [ 10, "a"; 10, "z"; 20, "b" ]) "Reaction candidates lost canonical simultaneous ordering."

        let red = Simulation.initialState.Units[observer]
        let blue = Simulation.initialState.Units[subject]
        let integrationBase =
            { Simulation.initialState with
                Board = { Simulation.initialState.Board with Maximum = cell 4 2; Edges = [] }
                Units =
                    Simulation.initialState.Units
                    |> Map.add observer { red with Cell = cell 0 0; AttentionDirection = East; WeaponPosture = WeaponPosture.Prepared }
                    |> Map.add subject { blue with Cell = cell 1 0 } }
        let prepare input state =
            let first = Simulation.runTick state [ input ]
            Simulation.runTick first.State []
        let areaReady = prepare (PrepareAreaReaction(observer, "integration-area", [ cell 2 0 ], East)) integrationBase
        let areaReaction = Simulation.runTick areaReady.State [ Move(subject, cell 2 0) ]
        require
            (areaReaction.Events |> List.exists (function ReactionResolved(id, source, "integration-area") -> id = observer && source = subject | _ -> false))
            "Hostile covered-area entry did not resolve through physical authority."

        let guarded = Edges.edgeBetween (cell 1 0) (cell 2 0) |> Option.defaultWith (fun () -> failwith "Integration edge is not canonical.")
        let edgeReady = prepare (PrepareEdgeReaction(observer, "integration-edge", guarded, East)) integrationBase
        let edgeReaction = Simulation.runTick edgeReady.State [ Move(subject, cell 2 0) ]
        require
            (edgeReaction.Events |> List.exists (function ReactionResolved(id, source, "integration-edge") -> id = observer && source = subject | _ -> false))
            "Hostile guarded-edge crossing did not resolve through physical authority."

        let acquiredContact =
            { AwarenessReaction.emptyContact subject with
                Level = AwarenessLevel.Acquired
                Acquisition = profile.IdentificationThreshold
                LastKnownCell = Some(cell 1 0)
                Reason = AwarenessReason.IdentificationThresholdReached }
        let unitBase = { integrationBase with Awareness = Map.ofList [ (observer, subject), acquiredContact ] }
        let unitReady = prepare (PrepareUnitReaction(observer, "integration-unit", subject, East)) unitBase
        let unitReaction = Simulation.runTick unitReady.State []
        require
            (unitReaction.Events |> List.exists (function ReactionResolved(id, source, "integration-unit") -> id = observer && source = subject | _ -> false))
            "Known hostile unit exposure did not resolve through physical authority."

        let allyId = unitId 30
        let allyBase =
            { integrationBase with
                Units = integrationBase.Units |> Map.add allyId { red with Id = allyId; Cell = cell 1 1; AttentionDirection = East; WeaponPosture = WeaponPosture.Prepared } }
        let allyReady = prepare (PrepareAreaReaction(observer, "integration-ally", [ cell 2 1 ], East)) allyBase
        let allyMove = Simulation.runTick allyReady.State [ Move(allyId, cell 2 1) ]
        require
            (allyMove.Events |> List.forall (function ReactionCommitted(_, source, _) when source = allyId -> false | _ -> true))
            "An allied covered-area entry produced a hostile reaction."

        let interruptedPosture =
            Simulation.runTick
                (Simulation.runTick integrationBase [ PrepareAreaReaction(observer, "integration-posture", [ cell 2 0 ], East) ]).State
                [ SetWeaponPosture(observer, WeaponPosture.Mobile) ]
        require
            (interruptedPosture.State.Engagements[observer].Reason = ReactionReason.PostureChanged)
            "Losing prepared weapon posture did not interrupt the reaction window."

        [ AwarenessReaction.canonicalContactBytes first
          AwarenessReaction.canonicalContactBytes acquired
          AwarenessReaction.canonicalContactBytes lost
          AwarenessReaction.canonicalEngagementBytes active
          AwarenessReaction.canonicalEngagementBytes committed
          AwarenessReaction.canonicalEngagementBytes resolved
          AwarenessReaction.canonicalEngagementBytes interrupted
          areaReaction.EventBytes
          edgeReaction.EventBytes
          unitReaction.EventBytes
          allyMove.EventBytes
          interruptedPosture.EventBytes
          yield! ordered |> List.map AwarenessReaction.canonicalCandidateBytes ]
        |> CanonicalEncoding.concatenate
