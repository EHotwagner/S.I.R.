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
        require (first.Level = AwarenessLevel.Suspected && first.LastKnownCell = Some(cell 4 1)) "One LOS stimulus did not remain suspected with its locally known factual cell."
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
        let guardedSemantic = { EdgeId = "integration-edge-east"; SpatialRevision = 7; Edge = guarded; BlocksMovement = false }
        let edgeBase = { integrationBase with Board = { integrationBase.Board with Edges = [ guardedSemantic ] } }
        let edgeReady = prepare (PrepareEdgeReaction(observer, "integration-edge", guarded, East)) edgeBase
        let edgeReaction = Simulation.runTick edgeReady.State [ Move(subject, cell 2 0) ]
        let edgeResolved = edgeReaction.Events |> List.exists (function ReactionResolved(id, source, "integration-edge") -> id = observer && source = subject | _ -> false)
        require edgeResolved "Hostile guarded-edge crossing did not resolve through physical authority."

        let invalidatedBoard =
            match mutation with
            | Some "edge-removal" -> [ guardedSemantic ]
            | Some "edge-revision" -> [ guardedSemantic ]
            | _ -> []
        let removed = Simulation.runTick { edgeReady.State with Board = { edgeReady.State.Board with Edges = invalidatedBoard } } [ Move(subject, cell 2 0) ]
        require
            (not (removed.Events |> List.exists (function ReactionResolved(_, _, "integration-edge") -> true | _ -> false))
             && removed.State.Engagements[observer].Reason = ReactionReason.TargetInvalidated)
            "A removed guarded edge remained triggerable."
        let revisedBoard =
            if mutation = Some "edge-revision" then [ guardedSemantic ]
            else [ { guardedSemantic with SpatialRevision = 8 } ]
        let revised = Simulation.runTick { edgeReady.State with Board = { edgeReady.State.Board with Edges = revisedBoard } } [ Move(subject, cell 2 0) ]
        require
            (not (revised.Events |> List.exists (function ReactionResolved(_, _, "integration-edge") -> true | _ -> false))
             && revised.State.Engagements[observer].Reason = ReactionReason.TargetInvalidated)
            "A revised guarded edge remained triggerable."

        let inventedEdge = Edges.edgeBetween (cell 3 0) (cell 4 0) |> Option.defaultWith (fun () -> failwith "Invented edge is not canonical.")
        let invented = Simulation.runTick integrationBase [ PrepareEdgeReaction(observer, "invented-edge", inventedEdge, East) ]
        require (not (invented.State.Engagements.ContainsKey observer)) "An absent semantic edge was admitted."

        let manyUnits =
            [ for index in 1 .. 300 do
                  let id = unitId (int32 (1000 + index))
                  yield id, { red with Id = id; Side = (if index % 2 = 0 then Side.Red else Side.Blue); Cell = cell (int32 index) 10 } ]
            |> Map.ofList
        let expiryObserver = manyUnits |> Map.toSeq |> Seq.map fst |> Seq.last
        let expirySubject = manyUnits |> Map.toSeq |> Seq.map fst |> Seq.head
        let stale =
            { AwarenessReaction.emptyContact expirySubject with
                Level = AwarenessLevel.LostContact
                Acquisition = 2
                LastKnownCell = Some(cell 299 10)
                LastStimulusTick = Some 0
                RetainUntilTick = Some 20
                Reason = AwarenessReason.ContactRetained }
        let expiryState =
            { integrationBase with Tick = (if mutation = Some "unserviced-expiry" then 19 else 21); Units = manyUnits; Awareness = Map.ofList [ (expiryObserver, expirySubject), stale ]; AwarenessCursor = 0 }
        let expiryResult = Simulation.runTick expiryState []
        let expired = expiryResult.State.Awareness[(expiryObserver, expirySubject)]
        require (expired.Level = AwarenessLevel.Unknown && expired.LastKnownCell.IsNone) "An unserviced contact did not expire by authoritative elapsed tick."

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
