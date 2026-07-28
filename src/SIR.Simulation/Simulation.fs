namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

/// A stable unit identity in the authoritative simulation.
[<Struct>]
type UnitId = private UnitId of int32

/// The two sides used by the minimal simulation slice.
type Side =
    | Red
    | Blue

/// Authoritative state for one unit.
type UnitState =
    { Id: UnitId
      Side: Side
      Cell: Cell
      Health: BoundedInt32 }

/// A canonical boundary with semantics owned by S.I.R.
type SemanticEdge =
    { Edge: Edge
      BlocksMovement: bool }

/// The fixed board and its semantic boundaries.
type Board =
    { Minimum: Cell
      Maximum: Cell
      Edges: SemanticEdge list }

/// Complete authoritative state for the minimal slice.
type SimulationState =
    { Tick: int32
      Board: Board
      Units: Map<UnitId, UnitState>
      Observations: Set<UnitId * UnitId> }

/// Validated replay-driving inputs consumed by the shared kernel.
type KernelInput =
    | Move of unitId: UnitId * destination: Cell
    | Observe of observerId: UnitId * targetId: UnitId
    | Attack of attackerId: UnitId * targetId: UnitId

/// Stable logical phases used by conformance diagnostics.
type SimulationPhase =
    | MovementPhase
    | ObservationPhase
    | AttackPhase
    | CommitPhase

/// The authoritative event stream emitted by the minimal slice.
type SimulationEvent =
    | UnitMoved of unitId: UnitId * origin: Cell * destination: Cell
    | MovementBlockedByEdge of unitId: UnitId * origin: Cell * destination: Cell * edge: Edge
    | UnitObserved of observerId: UnitId * targetId: UnitId * distance: int32
    | AttackResolved of attackerId: UnitId * targetId: UnitId * damage: int32 * remainingHealth: int32

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
      Checkpoints: PhaseCheckpoint list }

/// Deterministic construction and execution of the minimal shared slice.
[<RequireQualifiedAccess>]
module Simulation =
    let unitId value = UnitId value
    let unitIdValue (UnitId value) = value

    let private required result =
        match result with
        | Ok value -> value
        | Error error -> failwithf "Invalid minimal-slice state: %A" error

    let private health value = BoundedInt32.create 0 100 value |> required

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
              Health = health 100 }

        let blue =
            { Id = unitId 20
              Side = Blue
              Cell = cell 2 0
              Health = health 100 }

        let edge =
            { Edge = requiredEdge (cell 1 0) (cell 2 0)
              BlocksMovement = true }

        { Tick = 0
          Board =
            { Minimum = cell 0 0
              Maximum = cell 2 1
              Edges = [ edge ] }
          Units = [ red.Id, red; blue.Id, blue ] |> Map.ofList
          Observations = Set.empty }

    /// The canonical M6 journal. Its list order is deliberately non-semantic.
    let inputs =
        [ Attack(unitId 10, unitId 20)
          Move(unitId 20, cell 1 0)
          Observe(unitId 10, unitId 20)
          Move(unitId 10, cell 1 1) ]

    let private inputCompare (left: KernelInput) (right: KernelInput) =
        let key (input: KernelInput) =
            match input with
            | Move(id, destination) -> 0, unitIdValue id, destination.Col, destination.Row, 0
            | Observe(observerId, targetId) -> 1, unitIdValue observerId, 0, 0, unitIdValue targetId
            | Attack(attackerId, targetId) -> 2, unitIdValue attackerId, 0, 0, unitIdValue targetId

        compare (key left) (key right)

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

    let private tryUnit id state = Map.tryFind id state.Units

    let private replaceUnit unit state =
        { state with Units = Map.add unit.Id unit state.Units }

    let private movementPhase state inputs =
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
                    match movementBlocker state.Board unit.Cell destination with
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
                let visible =
                    Los.lineOfSightBy
                        Supercover
                        (inBounds current.Board)
                        observer.Cell
                        target.Cell

                if visible then
                    let distance = chebyshevDistance observer.Cell target.Cell |> int32
                    let observed = Set.add (observerId, targetId) current.Observations
                    { current with Observations = observed },
                    UnitObserved(observerId, targetId, distance) :: events
                else
                    current, events
            | _ -> current, events)
        |> fun (next, events) -> next, List.rev events

    let private attackPhase state inputs =
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
                let damage = health 25

                let remaining =
                    BoundedInt32.subtractSaturating target.Health damage
                    |> required

                let damaged = { target with Health = remaining }

                replaceUnit damaged current,
                AttackResolved(attackerId, targetId, 25, BoundedInt32.value remaining) :: events
            | _ -> current, events)
        |> fun (next, events) -> next, List.rev events

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

    let private cellBytes position =
        CanonicalEncoding.concatenate
            [ CanonicalEncoding.int32LittleEndian position.Col
              CanonicalEncoding.int32LittleEndian position.Row ]

    let private unitIdBytes id = id |> unitIdValue |> CanonicalEncoding.int32LittleEndian

    let private edgeBytes edge =
        CanonicalEncoding.concatenate [ cellBytes edge.Lo; cellBytes edge.Hi ]

    /// Provisional canonical M6 state encoding. The versioned replay schema is selected in M7.
    let stateBytes state =
        let unitBytes =
            state.Units
            |> Map.toList
            |> List.collect (fun (id, unit) ->
                [ unitIdBytes id
                  CanonicalEncoding.byteValue (sideCode unit.Side)
                  cellBytes unit.Cell
                  CanonicalEncoding.boundedInt32 unit.Health ])

        let observationBytes =
            state.Observations
            |> Set.toList
            |> List.collect (fun (observerId, targetId) ->
                [ unitIdBytes observerId; unitIdBytes targetId ])

        CanonicalEncoding.concatenate
            ([ CanonicalEncoding.byteValue 1uy
               CanonicalEncoding.int32LittleEndian state.Tick
               CanonicalEncoding.int32LittleEndian state.Units.Count ]
             @ unitBytes
             @ [ CanonicalEncoding.int32LittleEndian state.Observations.Count ]
             @ observationBytes)

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
        | AttackResolved(attackerId, targetId, damage, remainingHealth) ->
            CanonicalEncoding.concatenate
                [ CanonicalEncoding.byteValue 3uy
                  unitIdBytes attackerId
                  unitIdBytes targetId
                  CanonicalEncoding.int32LittleEndian damage
                  CanonicalEncoding.int32LittleEndian remainingHealth ]

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

    /// Executes one tick from stable phase inputs and commits each phase as a deterministic batch.
    let runTick (state: SimulationState) (journal: KernelInput list) =
        let canonicalInputs = journal |> List.distinct |> List.sortWith inputCompare
        let nextTick = state.Tick + 1

        let movementState, movementEvents = movementPhase state canonicalInputs

        let movementCheckpoint =
            { Tick = nextTick
              Phase = MovementPhase
              State = movementState
              Events = movementEvents }

        let observationState, observationEvents = observationPhase movementState canonicalInputs
        let throughObservation = movementEvents @ observationEvents

        let observationCheckpoint =
            { Tick = nextTick
              Phase = ObservationPhase
              State = observationState
              Events = throughObservation }

        let attackState, attackEvents = attackPhase observationState canonicalInputs
        let allEvents = throughObservation @ attackEvents

        let attackCheckpoint =
            { Tick = nextTick
              Phase = AttackPhase
              State = attackState
              Events = allEvents }

        let committed = { attackState with Tick = nextTick }

        let commitCheckpoint =
            { Tick = nextTick
              Phase = CommitPhase
              State = committed
              Events = allEvents }

        let canonicalState = stateBytes committed

        { State = committed
          Events = allEvents
          StateBytes = canonicalState
          EventBytes = eventsBytes allEvents
          StateDigest = CanonicalEncoding.digest32 canonicalState
          Checkpoints =
            [ movementCheckpoint
              observationCheckpoint
              attackCheckpoint
              commitCheckpoint ] }
