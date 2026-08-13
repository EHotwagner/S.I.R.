namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

/// The two sides used by the minimal simulation slice.
type Side =
    | Red
    | Blue

[<RequireQualifiedAccess>]
type WeaponPosture =
    | Mobile
    | Ready
    | Prepared

/// Authoritative state for one unit.
type UnitState =
    { Id: UnitId
      Side: Side
      Cell: Cell
      Health: BoundedInt32
      Armor: ArmorState
      Wounds: Wound list
      Incapacitated: bool
      Suppression: int32
      BodyFacing: Direction8
      AttentionDirection: Direction8
      WeaponPosture: WeaponPosture }

/// A canonical boundary with semantics owned by S.I.R.
type SemanticEdge =
    { EdgeId: string
      SpatialRevision: int32
      Edge: Edge
      BlocksMovement: bool }

/// The fixed board and its semantic boundaries.
type Board =
    { Minimum: Cell
      Maximum: Cell
      Edges: SemanticEdge list
      Covers: Map<string, CoverState> }

/// Complete authoritative state for the minimal slice.
type SimulationState =
    { Tick: int32
      Board: Board
      Units: Map<UnitId, UnitState>
      Observations: Set<UnitId * UnitId>
      Awareness: Map<UnitId * UnitId, AwarenessContact>
      Engagements: Map<UnitId, Engagement>
      AwarenessCursor: int32 }

/// Validated replay-driving inputs consumed by the shared kernel.
type KernelInput =
    | Move of unitId: UnitId * destination: Cell
    | Observe of observerId: UnitId * targetId: UnitId
    | Attack of attackerId: UnitId * targetId: UnitId
    | PhysicalAttack of attackerId: UnitId * aimCell: Cell * profile: WeaponProfile
    | SetAttention of unitId: UnitId * direction: Direction8
    | SetWeaponPosture of unitId: UnitId * posture: WeaponPosture
    | PrepareAreaReaction of unitId: UnitId * engagementId: string * cells: Cell list * requiredAttention: Direction8
    | PrepareUnitReaction of unitId: UnitId * engagementId: string * targetId: UnitId * requiredAttention: Direction8
    | PrepareEdgeReaction of unitId: UnitId * engagementId: string * edge: Edge * requiredAttention: Direction8

/// Stable logical phases used by conformance diagnostics.
type SimulationPhase =
    | MovementPhase
    | ObservationPhase
    | AttackPhase
    | CommitPhase
    | AwarenessReactionPhase

/// The authoritative event stream emitted by the minimal slice.
type SimulationEvent =
    | UnitMoved of unitId: UnitId * origin: Cell * destination: Cell
    | MovementBlockedByEdge of unitId: UnitId * origin: Cell * destination: Cell * edge: Edge
    | UnitObserved of observerId: UnitId * targetId: UnitId * distance: int32
    | AttackResolved of attackerId: UnitId * targetId: UnitId * damage: int32 * remainingHealth: int32 * explanation: RuleApplication
    | PhysicalAttackResolved of attackerId: UnitId * profile: WeaponProfile * facts: CombatFact list * applications: RuleApplication list
    | PhysicalAttackRejected of attackerId: UnitId * profile: WeaponProfile * rejection: CombatRejection
    | CombatRecoveryCommitted of facts: CombatFact list
    | AwarenessChanged of observerId: UnitId * subjectId: UnitId * level: AwarenessLevel * reason: AwarenessReason
    | EngagementChanged of ownerId: UnitId * engagementId: string * phase: EngagementPhase * reason: ReactionReason
    | ReactionCommitted of reactorId: UnitId * sourceId: UnitId * engagementId: string
    | ReactionResolved of reactorId: UnitId * sourceId: UnitId * engagementId: string

/// One logical-phase checkpoint for first-divergence diagnosis.
type PhaseCheckpoint =
    { Tick: int32
      Phase: SimulationPhase
      State: SimulationState
      Events: SimulationEvent list }

/// Result of one committed simulation tick.
type TickResult =
    { State: SimulationState
      Events: SimulationEvent list
      StateBytes: byte array
      EventBytes: byte array
      StateDigest: byte array
      AwarenessCounters: AwarenessCounters
      Checkpoints: PhaseCheckpoint list }

/// Bounded authoritative rules that may be varied by a derived design scenario.
type SimulationRules =
    { AttackPower: BoundedInt32 }

/// Deterministic construction and execution of the minimal shared slice.
[<RequireQualifiedAccess>]
module Simulation =
    let unitId value = UnitId.create value
    let unitIdValue id = UnitId.value id

    /// Resolves orientation from authoritative body/attention state and an
    /// optional active route segment. Movement direction is never stored.
    let resolvedOrientation origin destination (unit: UnitState) =
        { MovementDirection =
            destination
            |> Option.bind (fun target ->
                Direction8.tryFromDelta
                    (target.Col - origin.Col)
                    (target.Row - origin.Row))
          BodyFacing = unit.BodyFacing
          AttentionDirection = unit.AttentionDirection }

    let private required result =
        match result with
        | Ok value -> value
        | Error error -> failwithf "Invalid minimal-slice state: %A" error

    let private health value = BoundedInt32.create 0 100 value |> required

    let defaultRules =
        { AttackPower = health 25 }

    let private defaultArmor = { FrontRating = 50; RearRating = 20; Integrity = 100 }

    let private cell col row: Cell = { Col = col; Row = row }

    let private requiredEdge left right =
        Edges.edgeBetween left right
        |> Option.defaultWith (fun () -> failwith "The minimal-slice semantic edge must be orthogonal.")

    /// The canonical M6 scenario: two units and one movement-blocking semantic edge.
    let initialState =
        let red =
            { Id = unitId 10
              Side = Red
              Cell = cell 0 0
              Health = health 100
              Armor = defaultArmor
              Wounds = []
              Incapacitated = false
              Suppression = 0
              BodyFacing = North
              AttentionDirection = North
              WeaponPosture = WeaponPosture.Mobile }

        let blue =
            { Id = unitId 20
              Side = Blue
              Cell = cell 2 0
              Health = health 100
              Armor = defaultArmor
              Wounds = []
              Incapacitated = false
              Suppression = 0
              BodyFacing = North
              AttentionDirection = North
              WeaponPosture = WeaponPosture.Mobile }

        let edge =
            { EdgeId = "minimal-east-boundary"
              SpatialRevision = 1
              Edge = requiredEdge (cell 1 0) (cell 2 0)
              BlocksMovement = true }

        { Tick = 0
          Board =
            { Minimum = cell 0 0
              Maximum = cell 2 1
              Edges = [ edge ]
              Covers = Map.empty }
          Units = [ red.Id, red; blue.Id, blue ] |> Map.ofList
          Observations = Set.empty
          Awareness = Map.empty
          Engagements = Map.empty
          AwarenessCursor = 0 }

    /// The canonical M6 journal. Its list order is deliberately non-semantic.
    let inputs =
        [ Attack(unitId 10, unitId 20)
          Move(unitId 20, cell 1 0)
          Observe(unitId 10, unitId 20)
          Move(unitId 10, cell 1 1) ]

    // Every field participates in F# structural comparison. This prevents two
    // distinct preparation commands from comparing equal and making the final
    // one depend on journal insertion order.
    let private inputCompare (left: KernelInput) (right: KernelInput) = compare left right

    let private inBounds board position =
        position.Col >= board.Minimum.Col
        && position.Col <= board.Maximum.Col
        && position.Row >= board.Minimum.Row
        && position.Row <= board.Maximum.Row

    let private chebyshevDistance left right =
        max
            (abs (int64 right.Col - int64 left.Col))
            (abs (int64 right.Row - int64 left.Row))

    let private blockingEdge board left right =
        Edges.edgeBetween left right
        |> Option.bind (fun crossed ->
            board.Edges
            |> List.tryFind (fun semantic -> semantic.BlocksMovement && semantic.Edge = crossed)
            |> Option.map (fun semantic -> semantic.Edge))

    let private spatialIdentity tick =
        SpatialAuthorityIdentity.create "minimal-slice-board" "sir-spatial-v1" (int64 tick) "simulation-authority" (int64 tick)
        |> Result.defaultWith failwith

    let private spatialWorld tick board =
        { Identity = spatialIdentity tick
          Minimum = board.Minimum
          Maximum = board.Maximum
          Terrain = Map.empty
          Boundaries =
            board.Edges
            |> List.map (fun semantic ->
                { Edge = semantic.Edge
                  Permeability = { Ground = not semantic.BlocksMovement; Vision = true; Projectile = true }
                  RevisionToken = $"edge:{semantic.Edge.Lo.Col}:{semantic.Edge.Lo.Row}:{semantic.Edge.Hi.Col}:{semantic.Edge.Hi.Row}" })
          Occupancy = Map.empty
          DisclosedRevisionTokens = Set.empty }

    let private spatialProfile modality =
        { ProfileId = "minimal-unit-v1"
          Modality = modality
          Stance = "standing"
          HeightBand = 1
          Facing = North }

    let private spatialRequest queryId kind modality origin target =
        { QueryId = queryId
          QueryKind = kind
          Origin = origin
          Target = target
          Footprint = [ cell 0 0 ]
          Profile = spatialProfile modality
          Bounds = { SpatialQuery.defaultBounds with MaximumResultCells = 2 } }

    let private diagonalEdges origin destination =
        let horizontal = cell destination.Col origin.Row
        let vertical = cell origin.Col destination.Row

        [ origin, horizontal
          origin, vertical
          horizontal, destination
          vertical, destination ]

    /// Equal-cost Chebyshev movement with a strict no-corner-cutting semantic-edge rule.
    let private movementBlocker board origin destination =
        if not (inBounds board destination) || chebyshevDistance origin destination <> 1L then
            None
        elif origin.Col = destination.Col || origin.Row = destination.Row then
            blockingEdge board origin destination
        else
            diagonalEdges origin destination
            |> List.tryPick (fun (left, right) -> blockingEdge board left right)

    let private authoritativeMovementBlocker world board origin destination =
        let request = spatialRequest "simulation-movement" SpatialQueryKind.MovementCost SpatialModality.GroundMovement origin destination
        let result, _ = SpatialQuery.evaluate world request
        if result.Outcome = SpatialOutcome.Found && result.Path = [ origin; destination ] then None
        else movementBlocker board origin destination

    let private tryUnit id state = Map.tryFind id state.Units

    let private replaceUnit unit state =
        { state with Units = Map.add unit.Id unit state.Units }

    let private movementPhase (state: SimulationState) inputs =
        let movementWorld = spatialWorld state.Tick state.Board
        let moves =
            inputs
            |> List.choose (function
                | Move(unitId, destination) -> Some(unitId, destination)
                | _ -> None)

        let candidates =
            moves
            |> List.choose (fun (unitId, destination) ->
                match tryUnit unitId state with
                | None -> None
                | Some unit ->
                    match authoritativeMovementBlocker movementWorld state.Board unit.Cell destination with
                    | Some edge -> Some(unit, destination, Some edge)
                    | None when
                        inBounds state.Board destination
                        && chebyshevDistance unit.Cell destination = 1L
                        && not (
                            state.Units
                            |> Map.exists (fun otherId other ->
                                otherId <> unitId && other.Cell = destination)
                        )
                        ->
                        Some(unit, destination, None)
                    | _ -> None)

        let destinationCounts =
            candidates
            |> List.choose (fun (_, destination, blocker) ->
                if Option.isNone blocker then Some destination else None)
            |> List.countBy id
            |> Map.ofList

        let committed =
            candidates
            |> List.fold (fun current (unit, destination, blocker) ->
                match blocker with
                | None when Map.find destination destinationCounts = 1 ->
                    replaceUnit { unit with Cell = destination } current
                | _ -> current) state

        let events =
            candidates
            |> List.choose (fun (unit, destination, blocker) ->
                match blocker with
                | Some edge ->
                    Some(MovementBlockedByEdge(unit.Id, unit.Cell, destination, edge))
                | None when Map.find destination destinationCounts = 1 ->
                    Some(UnitMoved(unit.Id, unit.Cell, destination))
                | None -> None)

        committed, events

    let private observationPhase state inputs =
        let observations =
            inputs
            |> List.choose (function
                | Observe(observerId, targetId) -> Some(observerId, targetId)
                | _ -> None)

        ((state, []), observations)
        ||> List.fold (fun (current, events) (observerId, targetId) ->
            match tryUnit observerId current, tryUnit targetId current with
            | Some observer, Some target ->
                let request = spatialRequest "simulation-observation" SpatialQueryKind.ExactLineOfSight SpatialModality.Vision observer.Cell target.Cell
                let visibility, _ = SpatialQuery.evaluate (spatialWorld current.Tick current.Board) request
                let visible = visibility.Outcome = SpatialOutcome.Found && visibility.Visible

                if visible then
                    let distance = chebyshevDistance observer.Cell target.Cell |> int32
                    let observed = Set.add (observerId, targetId) current.Observations
                    { current with Observations = observed },
                    UnitObserved(observerId, targetId, distance) :: events
                else
                    current, events
            | _ -> current, events)
        |> fun (next, events) -> next, List.rev events

    let private attentionAndEngagementPhase state inputs =
        ((state, []), inputs)
        ||> List.fold (fun (current, events) input ->
            match input with
            | SetAttention(unitId, direction) ->
                match tryUnit unitId current with
                | Some unit -> replaceUnit { unit with AttentionDirection = direction } current, events
                | None -> current, events
            | SetWeaponPosture(unitId, posture) ->
                match tryUnit unitId current with
                | Some unit -> replaceUnit { unit with WeaponPosture = posture } current, events
                | None -> current, events
            | PrepareAreaReaction(unitId, engagementId, cells, requiredAttention) ->
                match tryUnit unitId current, AwarenessReaction.declareEngagement engagementId unitId (EngagementTarget.CoveredArea cells) requiredAttention with
                | Some _, Ok engagement ->
                    { current with Engagements = Map.add unitId engagement current.Engagements },
                    EngagementChanged(unitId, engagementId, engagement.Phase, engagement.Reason) :: events
                | _ -> current, events
            | PrepareUnitReaction(unitId, engagementId, targetId, requiredAttention) ->
                match tryUnit unitId current, Map.tryFind (unitId, targetId) current.Awareness, AwarenessReaction.declareEngagement engagementId unitId (EngagementTarget.KnownUnit targetId) requiredAttention with
                | Some _, Some contact, Ok engagement when contact.Level = AwarenessLevel.Acquired ->
                    { current with Engagements = Map.add unitId engagement current.Engagements },
                    EngagementChanged(unitId, engagementId, engagement.Phase, engagement.Reason) :: events
                | _ -> current, events
            | PrepareEdgeReaction(unitId, engagementId, edge, requiredAttention) ->
                let semantic = current.Board.Edges |> List.tryFind (fun candidate -> candidate.Edge = edge)
                match tryUnit unitId current, semantic with
                | Some _, Some declared ->
                    match AwarenessReaction.declareEngagement engagementId unitId (EngagementTarget.GuardedEdge(declared.EdgeId, declared.SpatialRevision, declared.Edge)) requiredAttention with
                    | Error _ -> current, events
                    | Ok engagement ->
                    { current with Engagements = Map.add unitId engagement current.Engagements },
                    EngagementChanged(unitId, engagementId, engagement.Phase, engagement.Reason) :: events
                | _ -> current, events
            | _ -> current, events)
        |> fun (next, events) -> next, List.rev events

    let private awarenessReactionPhase state movementEvents =
        let mutable counters = { CandidatePairs = 0; SectorSurvivors = 0; LosEvaluations = 0; Stimuli = 0; AwarenessEpisodes = 0; Engagements = int32 state.Engagements.Count; ReactionCandidates = 0 }
        let mutable contacts = state.Awareness
        let mutable events = []
        let units = state.Units |> Map.toArray
        let redUnits = units |> Array.filter (fun (_, unit) -> unit.Side = Side.Red)
        let blueUnits = units |> Array.filter (fun (_, unit) -> unit.Side = Side.Blue)
        let pairCount =
            redUnits.Length * blueUnits.Length * 2
        counters <- { counters with CandidatePairs = int32 pairCount }
        let serviceCount = min 256 pairCount
        let start = if pairCount = 0 then 0 else int state.AwarenessCursor % pairCount
        let mutable subjectIndex = 0
        let mutable selectedObserverIndex = 0
        let servicedPairs = System.Collections.Generic.HashSet<UnitId * UnitId>()
        if pairCount > 0 then
            let mutable prefix = start
            selectedObserverIndex <- 0
            while prefix >= (if (snd units[selectedObserverIndex]).Side = Side.Red then blueUnits.Length else redUnits.Length) do
                prefix <- prefix - (if (snd units[selectedObserverIndex]).Side = Side.Red then blueUnits.Length else redUnits.Length)
                selectedObserverIndex <- selectedObserverIndex + 1
            subjectIndex <- prefix
        let advancePair () =
            subjectIndex <- subjectIndex + 1
            let subjects = if (snd units[selectedObserverIndex]).Side = Side.Red then blueUnits else redUnits
            if subjectIndex >= subjects.Length then
                selectedObserverIndex <- (selectedObserverIndex + 1) % units.Length
                subjectIndex <- 0
        // Every stimulus in the phase observes the same authoritative snapshot.
        // Build that immutable adapter once instead of rebuilding its occupancy
        // map for each candidate pair.
        let awarenessWorld = spatialWorld state.Tick state.Board
        for offset in 0 .. serviceCount - 1 do
                    let observerId, observer = units[selectedObserverIndex]
                    let subjects = if observer.Side = Side.Red then blueUnits else redUnits
                    let subjectId, subject = subjects[subjectIndex]
                    servicedPairs.Add((observerId, subjectId)) |> ignore
                    let observedSector = AwarenessReaction.sector observer.AttentionDirection observer.Cell subject.Cell
                    // The 256-slot cursor bounds work while guaranteeing a complete
                    // 20,000-pair sweep within 79 ticks on the declared 100v100 workload.
                    if counters.LosEvaluations < 256 && counters.AwarenessEpisodes < 256 then
                        counters <- { counters with SectorSurvivors = counters.SectorSurvivors + 1; LosEvaluations = counters.LosEvaluations + 1 }
                        let stimulus, _ = AwarenessReaction.evaluateVisualStimulus awarenessWorld AwarenessReaction.infantryProfile state.Tick observerId observer.AttentionDirection observer.Cell subjectId subject.Cell |> Result.defaultWith failwith
                        if stimulus.IsSome then counters <- { counters with Stimuli = counters.Stimuli + 1 }
                        let previousContact = Map.tryFind (observerId, subjectId) contacts
                        let previous = previousContact |> Option.defaultValue (AwarenessReaction.emptyContact subjectId)
                        let next = AwarenessReaction.advanceContact AwarenessReaction.infantryProfile state.Tick subject.Cell stimulus previous
                        // Absence is the canonical representation of a never-known
                        // contact; do not populate the elapsed-time index with inert
                        // out-of-range pairs.
                        if previousContact.IsSome || next.Level <> AwarenessLevel.Unknown || next.LastKnownCell.IsSome then
                            contacts <- Map.add (observerId, subjectId) next contacts
                        counters <- { counters with AwarenessEpisodes = counters.AwarenessEpisodes + 1 }
                        if next.Level <> previous.Level || next.Reason <> previous.Reason then events <- AwarenessChanged(observerId, subjectId, next.Level, next.Reason) :: events
                    advancePair ()

        // Expensive spatial queries are cursor-bounded, but authoritative time
        // is not. Existing knowledge outside this tick's service slice still
        // decays and expires against the current committed tick.
        contacts
        |> Map.iter (fun (observerId, subjectId) previous ->
            if not (servicedPairs.Contains((observerId, subjectId))) then
                let subjectCell =
                    Map.tryFind subjectId state.Units
                    |> Option.map _.Cell
                    |> Option.orElse previous.LastKnownCell
                    |> Option.defaultValue { Col = 0; Row = 0 }
                let next = AwarenessReaction.advanceContact AwarenessReaction.infantryProfile state.Tick subjectCell None previous
                // The no-stimulus transition cannot rewrite retained stimulus facts;
                // compare only the fields it owns and avoid walking retained strings.
                if next.Level = AwarenessLevel.Unknown then
                    contacts <- Map.remove (observerId, subjectId) contacts
                elif next.Acquisition <> previous.Acquisition
                     || next.Level <> previous.Level
                     || next.LastKnownCell <> previous.LastKnownCell
                     || next.RetainUntilTick <> previous.RetainUntilTick
                     || next.Reason <> previous.Reason then
                    contacts <- Map.add (observerId, subjectId) next contacts
                if next.Level <> previous.Level || next.Reason <> previous.Reason then
                    events <- AwarenessChanged(observerId, subjectId, next.Level, next.Reason) :: events)

        let moved =
            movementEvents
            |> List.choose (function UnitMoved(unitId, origin, destination) -> Some(unitId, origin, destination) | _ -> None)
        let mutable engagements = state.Engagements
        let mutable candidates = []
        for KeyValue(ownerId, engagement) in state.Engagements do
            match Map.tryFind ownerId state.Units with
            | None -> ()
            | Some owner ->
                let trigger =
                    match engagement.Target with
                    | EngagementTarget.CoveredArea cells ->
                        moved
                        |> List.tryPick (fun (sourceId, _, destination) ->
                            if sourceId <> ownerId && state.Units[sourceId].Side <> owner.Side && List.contains destination cells then
                                Some(sourceId, destination, ReactionTriggerKind.CoveredAreaEntered)
                            else None)
                    | EngagementTarget.KnownUnit target ->
                        match Map.tryFind (ownerId, target) contacts, Map.tryFind target state.Units with
                        | Some contact, Some subject when contact.Level = AwarenessLevel.Acquired ->
                            Some(target, subject.Cell, ReactionTriggerKind.ValidTargetExposed)
                        | _ -> None
                    | EngagementTarget.GuardedEdge(edgeId, revision, guarded) ->
                        let stillAuthoritative =
                            state.Board.Edges
                            |> List.exists (fun edge -> edge.EdgeId = edgeId && edge.SpatialRevision = revision && edge.Edge = guarded)
                        if not stillAuthoritative then None
                        else
                            moved
                            |> List.tryPick (fun (sourceId, origin, destination) ->
                                match Edges.edgeBetween origin destination with
                                | Some crossed when sourceId <> ownerId && state.Units[sourceId].Side <> owner.Side && crossed = guarded ->
                                    Some(sourceId, destination, ReactionTriggerKind.GuardedEdgeCrossed)
                                | _ -> None)
                let targetValid =
                    match engagement.Target with
                    | EngagementTarget.GuardedEdge(edgeId, revision, guarded) -> state.Board.Edges |> List.exists (fun edge -> edge.EdgeId = edgeId && edge.SpatialRevision = revision && edge.Edge = guarded)
                    | _ -> true
                let maintained =
                    if not targetValid then { engagement with Phase = EngagementPhase.Interrupted; RemainingTicks = 0; Reason = ReactionReason.TargetInvalidated }
                    else AwarenessReaction.advanceEngagement (owner.AttentionDirection = engagement.RequiredAttention) (owner.WeaponPosture = WeaponPosture.Prepared) (not owner.Incapacitated) trigger.IsSome engagement
                let advanced = if maintained.Phase = EngagementPhase.TriggerEligible then AwarenessReaction.advanceEngagement true true true true maintained else maintained
                engagements <- Map.add ownerId advanced engagements
                if advanced.Phase <> engagement.Phase then events <- EngagementChanged(ownerId, advanced.EngagementId, advanced.Phase, advanced.Reason) :: events
                match trigger with
                | Some(sourceId, sourceCell, triggerKind) when advanced.Phase = EngagementPhase.Committed ->
                    candidates <- { ReactorId = ownerId; EngagementId = engagement.EngagementId; TriggerKind = triggerKind; SourceId = sourceId; SourceCell = sourceCell; Tick = state.Tick } :: candidates
                | _ -> ()
        let ordered = AwarenessReaction.orderCandidates candidates
        counters <- { counters with ReactionCandidates = int32 ordered.Length }
        let reactionEvents = ordered |> List.map (fun candidate -> ReactionCommitted(candidate.ReactorId, candidate.SourceId, candidate.EngagementId))
        let nextCursor = if pairCount = 0 then 0 else int32 ((start + serviceCount) % pairCount)
        { state with Awareness = contacts; Engagements = engagements; AwarenessCursor = nextCursor }, List.rev events @ reactionEvents, counters, ordered

    let private attackPhase rules state inputs =
        let attacks =
            inputs
            |> List.choose (function
                | Attack(attackerId, targetId) -> Some(attackerId, targetId)
                | _ -> None)

        ((state, []), attacks)
        ||> List.fold (fun (current, events) (attackerId, targetId) ->
            match tryUnit attackerId current, tryUnit targetId current with
            | Some attacker, Some target when
                Set.contains (attackerId, targetId) current.Observations
                && chebyshevDistance attacker.Cell target.Cell <= 1L
                ->
                let combat =
                    CombatRules.resolveAttack
                        { Attacker = attacker.Cell
                          TargetFootprint = [ target.Cell ]
                          VisibleSamples = 1
                          TotalSamples = 1
                          RangeCells = chebyshevDistance attacker.Cell target.Cell |> int32
                          Suppression = FixedPoint.zero
                          BaseDamage = BoundedInt32.value rules.AttackPower |> fun value -> FixedPoint.fromRatio value 1 |> required
                          ArmorRetention = FixedPoint.fromRatio 1 1 |> required
                          EventId = $"tick-{current.Tick + 1}-attack-{unitIdValue attackerId}-{unitIdValue targetId}" }
                    |> required
                let damage = BoundedInt32.create 0 100 combat.ExpectedDamage |> required

                let remaining =
                    BoundedInt32.subtractSaturating target.Health damage
                    |> required

                let damaged = { target with Health = remaining }

                replaceUnit damaged current,
                AttackResolved(
                    attackerId,
                    targetId,
                    BoundedInt32.value damage,
                    BoundedInt32.value remaining,
                    combat.Explanation
                )
                :: events
            | _ -> current, events)
        |> fun (next, events) -> next, List.rev events

    let private combatWorld (state: SimulationState) =
        let combatants =
            state.Units
            |> Map.toList
            |> List.map (fun (id, unit) ->
                let entityId = string (unitIdValue id)
                entityId,
                { EntityId = entityId
                  Faction = match unit.Side with Red -> "red" | Blue -> "blue"
                  Cell = unit.Cell
                  Facing = unit.BodyFacing
                  Health = BoundedInt32.value unit.Health
                  Armor = unit.Armor
                  Wounds = unit.Wounds
                  Incapacitated = unit.Incapacitated
                  Suppression = unit.Suppression })
            |> Map.ofList
        { Spatial = spatialWorld state.Tick state.Board
          Combatants = combatants
          Covers = state.Board.Covers }

    let private applyCombatWorld (combat: CombatWorld) (state: SimulationState) : SimulationState =
        let units =
            state.Units
            |> Map.map (fun id unit ->
                match Map.tryFind (string (unitIdValue id)) combat.Combatants with
                | None -> unit
                | Some updated ->
                    { unit with
                        Cell = updated.Cell
                        Health = BoundedInt32.create 0 100 updated.Health |> required
                        Armor = updated.Armor
                        Wounds = updated.Wounds
                        Incapacitated = updated.Incapacitated
                        Suppression = updated.Suppression })
        { state with Units = units; Board = { state.Board with Covers = combat.Covers } }

    let private physicalAttackPhase (state: SimulationState) inputs =
        let attacks =
            inputs
            |> List.choose (function PhysicalAttack(attackerId, aim, profile) -> Some(attackerId, aim, profile) | _ -> None)
        ((state, []), attacks)
        ||> List.fold (fun (current, events) (attackerId, aim, profile) ->
            let request =
                { AttackId = $"tick-{current.Tick + 1}-physical-{unitIdValue attackerId}-{aim.Col}-{aim.Row}"
                  AttackerId = string (unitIdValue attackerId)
                  AimCell = aim
                  Weapon = profile
                  Limits = Combat.defaultLimits }
            match Combat.resolve (combatWorld current) request with
            | Error rejection -> current, PhysicalAttackRejected(attackerId, profile, rejection) :: events
            | Ok result -> applyCombatWorld result.World current, PhysicalAttackResolved(attackerId, profile, result.Facts, result.RuleApplications) :: events)
        |> fun (next, events) -> next, List.rev events

    let private recoveryPhase (state: SimulationState) =
        let recovered, facts = Combat.recover (combatWorld state)
        applyCombatWorld recovered state, facts

    let private sideCode side =
        match side with
        | Red -> 0uy
        | Blue -> 1uy

    let private phaseCode phase =
        match phase with
        | MovementPhase -> 0uy
        | ObservationPhase -> 1uy
        | AttackPhase -> 2uy
        | CommitPhase -> 3uy
        | AwarenessReactionPhase -> 4uy

    let private cellBytes position =
        CanonicalEncoding.concatenate
            [ CanonicalEncoding.int32LittleEndian position.Col
              CanonicalEncoding.int32LittleEndian position.Row ]

    let private unitIdBytes id = id |> unitIdValue |> CanonicalEncoding.int32LittleEndian

    let private edgeBytes edge =
        CanonicalEncoding.concatenate [ cellBytes edge.Lo; cellBytes edge.Hi ]

    let private textBytes (value: string) =
        let bytes = System.Text.Encoding.UTF8.GetBytes value
        CanonicalEncoding.concatenate [ CanonicalEncoding.int32LittleEndian bytes.Length; bytes ]

    let private profileCode = function
        | WeaponProfile.Rifle -> 0uy
        | WeaponProfile.SupportWeapon -> 1uy
        | WeaponProfile.AntiArmor -> 2uy
        | WeaponProfile.LobbedArea -> 3uy

    let private woundBytes (wound: Wound) =
        CanonicalEncoding.concatenate
            [ textBytes wound.AttackId
              CanonicalEncoding.byteValue (match wound.Severity with WoundSeverity.Serious -> 0uy | WoundSeverity.Critical -> 1uy)
              CanonicalEncoding.int32LittleEndian wound.Damage ]

    /// Provisional canonical M6 state encoding. The versioned replay schema is selected in M7.
    let stateBytes (state: SimulationState) =
        // Stream the already canonical field encodings into one buffer. Large
        // awareness maps otherwise construct several temporary linked lists and
        // segment arrays for every authoritative tick.
        // Retained factual stimuli make contact records variable-width. Reserve
        // the bounded common-case size so buffer growth does not copy multi-MB
        // authoritative states on the timed path.
        let estimatedCapacity = 64 + state.Units.Count * 64 + state.Awareness.Count * 160 + state.Engagements.Count * 96
#if FABLE_COMPILER
        let buffer = ResizeArray<byte>(estimatedCapacity)
        let append (bytes: byte array) = buffer.AddRange bytes
#else
        use buffer = new System.IO.MemoryStream(estimatedCapacity)
        let append (bytes: byte array) = buffer.Write(bytes, 0, bytes.Length)
#endif
        append (CanonicalEncoding.byteValue 3uy)
        append (CanonicalEncoding.int32LittleEndian state.Tick)
        append (CanonicalEncoding.int32LittleEndian state.Units.Count)
        for KeyValue(id, unit) in state.Units do
            append (unitIdBytes id)
            append (CanonicalEncoding.byteValue (sideCode unit.Side))
            append (cellBytes unit.Cell)
            append (CanonicalEncoding.boundedInt32 unit.Health)
            append (CanonicalEncoding.int32LittleEndian unit.Armor.FrontRating)
            append (CanonicalEncoding.int32LittleEndian unit.Armor.RearRating)
            append (CanonicalEncoding.int32LittleEndian unit.Armor.Integrity)
            append (CanonicalEncoding.int32LittleEndian unit.Wounds.Length)
            for wound in unit.Wounds do append (woundBytes wound)
            append (CanonicalEncoding.byteValue (if unit.Incapacitated then 1uy else 0uy))
            append (CanonicalEncoding.int32LittleEndian unit.Suppression)
            append (CanonicalEncoding.direction8 unit.BodyFacing)
            append (CanonicalEncoding.direction8 unit.AttentionDirection)
            append (CanonicalEncoding.byteValue (match unit.WeaponPosture with WeaponPosture.Mobile -> 0uy | WeaponPosture.Ready -> 1uy | WeaponPosture.Prepared -> 2uy))
        append (CanonicalEncoding.int32LittleEndian state.Board.Covers.Count)
        for KeyValue(id, cover) in state.Board.Covers do
            append (textBytes id)
            append (cellBytes cover.Cell)
            append (CanonicalEncoding.int32LittleEndian cover.Integrity)
            append (CanonicalEncoding.byteValue (if cover.ProjectileBlocking then 1uy else 0uy))
        append (CanonicalEncoding.int32LittleEndian state.Observations.Count)
        for observerId, targetId in state.Observations do
            append (unitIdBytes observerId)
            append (unitIdBytes targetId)
        append (CanonicalEncoding.int32LittleEndian state.Awareness.Count)
        for KeyValue((observerId, subjectId), contact) in state.Awareness do
            append (unitIdBytes observerId)
            append (unitIdBytes subjectId)
            append (AwarenessReaction.canonicalContactBytes contact)
        append (CanonicalEncoding.int32LittleEndian state.Engagements.Count)
        for KeyValue(ownerId, engagement) in state.Engagements do
            append (unitIdBytes ownerId)
            append (AwarenessReaction.canonicalEngagementBytes engagement)
        append (CanonicalEncoding.int32LittleEndian state.AwarenessCursor)
        buffer.ToArray()

    let private eventBytes event =
        match event with
        | UnitMoved(unitId, origin, destination) ->
            CanonicalEncoding.concatenate
                [ CanonicalEncoding.byteValue 0uy
                  unitIdBytes unitId
                  cellBytes origin
                  cellBytes destination ]
        | MovementBlockedByEdge(unitId, origin, destination, edge) ->
            CanonicalEncoding.concatenate
                [ CanonicalEncoding.byteValue 1uy
                  unitIdBytes unitId
                  cellBytes origin
                  cellBytes destination
                  edgeBytes edge ]
        | UnitObserved(observerId, targetId, distance) ->
            CanonicalEncoding.concatenate
                [ CanonicalEncoding.byteValue 2uy
                  unitIdBytes observerId
                  unitIdBytes targetId
                  CanonicalEncoding.int32LittleEndian distance ]
        | AttackResolved(attackerId, targetId, damage, remainingHealth, _) ->
            CanonicalEncoding.concatenate
                [ CanonicalEncoding.byteValue 3uy
                  unitIdBytes attackerId
                  unitIdBytes targetId
                  CanonicalEncoding.int32LittleEndian damage
                  CanonicalEncoding.int32LittleEndian remainingHealth ]
        | PhysicalAttackResolved(attackerId, profile, facts, applications) ->
            CanonicalEncoding.concatenate
                ([ CanonicalEncoding.byteValue 4uy
                   unitIdBytes attackerId
                   CanonicalEncoding.byteValue (profileCode profile)
                   Combat.canonicalFactsBytes facts
                   CanonicalEncoding.int32LittleEndian applications.Length ]
                 @ (applications |> List.map Rules.canonicalApplicationBytes))
        | PhysicalAttackRejected(attackerId, profile, rejection) ->
            let detail = sprintf "%A" rejection
            CanonicalEncoding.concatenate
                [ CanonicalEncoding.byteValue 5uy
                  unitIdBytes attackerId
                  CanonicalEncoding.byteValue (profileCode profile)
                  textBytes detail ]
        | CombatRecoveryCommitted facts ->
            CanonicalEncoding.concatenate
                [ CanonicalEncoding.byteValue 6uy
                  Combat.canonicalFactsBytes facts ]
        | AwarenessChanged(observerId, subjectId, level, reason) ->
            CanonicalEncoding.concatenate
                [ CanonicalEncoding.byteValue 7uy
                  unitIdBytes observerId
                  unitIdBytes subjectId
                  AwarenessReaction.canonicalContactBytes
                      { (AwarenessReaction.emptyContact subjectId) with Level = level; Reason = reason } ]
        | EngagementChanged(ownerId, engagementId, phase, reason) ->
            let placeholder =
                { EngagementId = engagementId
                  OwnerId = ownerId
                  Target = EngagementTarget.KnownUnit ownerId
                  RequiredAttention = North
                  Phase = phase
                  RemainingTicks = 0
                  Reason = reason }
            CanonicalEncoding.concatenate [ CanonicalEncoding.byteValue 8uy; AwarenessReaction.canonicalEngagementBytes placeholder ]
        | ReactionCommitted(reactorId, sourceId, engagementId) ->
            CanonicalEncoding.concatenate [ CanonicalEncoding.byteValue 9uy; unitIdBytes reactorId; unitIdBytes sourceId; textBytes engagementId ]
        | ReactionResolved(reactorId, sourceId, engagementId) ->
            CanonicalEncoding.concatenate [ CanonicalEncoding.byteValue 10uy; unitIdBytes reactorId; unitIdBytes sourceId; textBytes engagementId ]

    /// Provisional canonical M6 event encoding. Event order is phase order then canonical input order.
    let eventsBytes (events: SimulationEvent list) =
        CanonicalEncoding.concatenate
            ([ CanonicalEncoding.byteValue 1uy
               CanonicalEncoding.int32LittleEndian (List.length events) ]
             @ (events |> List.map eventBytes))

    let checkpointBytes (checkpoint: PhaseCheckpoint) =
        CanonicalEncoding.concatenate
            [ CanonicalEncoding.int32LittleEndian checkpoint.Tick
              CanonicalEncoding.byteValue (phaseCode checkpoint.Phase)
              stateBytes checkpoint.State
              eventsBytes checkpoint.Events ]

    let private noPhysicalAttackPhase (state: SimulationState) _ = state, []
    let private noCombatRecovery (state: SimulationState) = state, []

    let private runTickCore
        (physicalPhase: SimulationState -> KernelInput list -> SimulationState * SimulationEvent list)
        (recover: SimulationState -> SimulationState * CombatFact list)
        rules
        (state: SimulationState)
        (journal: KernelInput list)
        =
        let canonicalInputs = journal |> List.distinct |> List.sortWith inputCompare
        let nextTick = state.Tick + 1

        let movementState, movementEvents = movementPhase state canonicalInputs

        let movementCheckpoint =
            { Tick = nextTick
              Phase = MovementPhase
              State = movementState
              Events = movementEvents }

        let commandedState, engagementCommandEvents = attentionAndEngagementPhase movementState canonicalInputs
        let awarenessState, awarenessEvents, awarenessCounters, reactionCandidates = awarenessReactionPhase commandedState movementEvents
        let reactionInputs =
            reactionCandidates
            |> List.map (fun candidate -> PhysicalAttack(candidate.ReactorId, candidate.SourceCell, WeaponProfile.Rifle))
        let physicallyReactedState, reactionPhysicalEvents = physicalPhase awarenessState reactionInputs
        let reactionState, reactionResolutionEvents =
            ((physicallyReactedState, []), List.zip (reactionCandidates |> List.truncate reactionPhysicalEvents.Length) reactionPhysicalEvents)
            ||> List.fold (fun (current, events) (candidate, physicalEvent) ->
                let engagement = Map.tryFind candidate.ReactorId current.Engagements
                match physicalEvent, engagement with
                | PhysicalAttackResolved _, Some active ->
                    let resolved =
                        { active with
                            Phase = EngagementPhase.Resolved
                            RemainingTicks = 0
                            Reason = ReactionReason.ResolvedByPhysicalAuthority }
                    { current with Engagements = Map.add candidate.ReactorId resolved current.Engagements },
                    ReactionResolved(candidate.ReactorId, candidate.SourceId, candidate.EngagementId) :: events
                | PhysicalAttackRejected _, Some active ->
                    let interrupted =
                        { active with
                            Phase = EngagementPhase.Interrupted
                            RemainingTicks = 0
                            Reason = ReactionReason.FireBlocked }
                    { current with Engagements = Map.add candidate.ReactorId interrupted current.Engagements },
                    EngagementChanged(candidate.ReactorId, candidate.EngagementId, interrupted.Phase, interrupted.Reason) :: events
                | _ -> current, events)
            |> fun (current, events) -> current, List.rev events
        let throughAwareness =
            movementEvents
            @ engagementCommandEvents
            @ awarenessEvents
            @ reactionPhysicalEvents
            @ reactionResolutionEvents

        let awarenessCheckpoint =
            { Tick = nextTick
              Phase = AwarenessReactionPhase
              State = reactionState
              Events = throughAwareness }

        let observationState, observationEvents = observationPhase reactionState canonicalInputs
        let throughObservation = throughAwareness @ observationEvents

        let observationCheckpoint =
            { Tick = nextTick
              Phase = ObservationPhase
              State = observationState
              Events = throughObservation }

        let legacyAttackState, legacyAttackEvents =
            attackPhase rules observationState canonicalInputs
        let attackState, physicalAttackEvents = physicalPhase legacyAttackState canonicalInputs
        let allEvents = throughObservation @ legacyAttackEvents @ physicalAttackEvents

        let attackCheckpoint =
            { Tick = nextTick
              Phase = AttackPhase
              State = attackState
              Events = allEvents }

        let recoveredState, recoveryFacts = recover attackState
        let committed = { recoveredState with Tick = nextTick }
        let committedEvents = if List.isEmpty recoveryFacts then allEvents else allEvents @ [ CombatRecoveryCommitted recoveryFacts ]

        let commitCheckpoint =
            { Tick = nextTick
              Phase = CommitPhase
              State = committed
              Events = committedEvents }

        let canonicalState = stateBytes committed

        { State = committed
          Events = committedEvents
          StateBytes = canonicalState
          EventBytes = eventsBytes committedEvents
          StateDigest = CanonicalEncoding.digest32 canonicalState
          AwarenessCounters = awarenessCounters
          Checkpoints =
            [ movementCheckpoint
              awarenessCheckpoint
              observationCheckpoint
              attackCheckpoint
              commitCheckpoint ] }

    /// Executes the original compact laboratory rules without physical-combat authority.
    let runTickWithRules rules state journal =
        runTickCore noPhysicalAttackPhase noCombatRecovery rules state journal

    /// Executes one authoritative tick including physical delivery and suppression recovery.
    let runPhysicalTickWithRules rules state journal =
        runTickCore physicalAttackPhase recoveryPhase rules state journal

    /// Executes the canonical rules used by replay and authoritative hosts.
    let runTick state journal =
        runPhysicalTickWithRules defaultRules state journal
