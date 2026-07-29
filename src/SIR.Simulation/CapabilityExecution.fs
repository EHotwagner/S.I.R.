namespace SIR.Simulation

open System
open System.Text
open SIR.ControlAbi
open SIR.Domain

type CapabilityTarget =
    | PointCapabilityTarget of unitId: int32
    | AreaCapabilityTarget of referentId: int32

type CapabilityEngagement =
    { CapabilityId: string
      Target: CapabilityTarget
      RequiredAttention: Direction8
      TraverseTicksRemaining: int32
      PreparationTicksRemaining: int32
      PreparedTicks: int32 }

type CapabilityUnitState =
    { Loadout: AuthoredUnitLoadout
      Cell: int32 * int32
      Attention: Direction8
      Ammunition: Map<string, int32>
      PreservedPreparation: Map<string, int32>
      Engagement: CapabilityEngagement option }

type CapabilityExecutionState =
    { Tick: int32
      Units: Map<int32, CapabilityUnitState>
      Areas: Map<int32, int32 * int32> }

type CapabilityEvent =
    | CapabilityRejected of unitId: int32 * capabilityId: string * reason: string
    | CapabilityTraversing of unitId: int32 * capabilityId: string * ticks: int32
    | CapabilityPrepared of unitId: int32 * capabilityId: string
    | PointEngagementResolved of unitId: int32 * targetUnitId: int32 * capabilityId: string * ammunitionRemaining: int32
    | AreaEngagementResolved of unitId: int32 * referentId: int32 * capabilityId: string * ammunitionRemaining: int32
    | CapabilityInterrupted of unitId: int32 * capabilityId: string * preparationPreserved: bool

type CapabilityTickResult =
    { State: CapabilityExecutionState
      Events: CapabilityEvent list }

type CapabilityReplayRequest =
    { Tick: int32
      UnitId: int32
      Request: Request }

type CapabilityReplayFrame =
    { Tick: int32
      StateDigest: byte array
      EventsDigest: byte array }

[<RequireQualifiedAccess>]
module CapabilityExecution =
    let private utf8 = Encoding.UTF8

    let engagementRequest
        (requestId: uint32)
        (target: CapabilityTarget)
        (capabilityId: string)
        =
        let targetKind, targetId =
            match target with
            | PointCapabilityTarget unitId -> byte TargetKind.KnownUnit, unitId
            | AreaCapabilityTarget referentId -> byte TargetKind.Referent, referentId
        let capability = utf8.GetBytes capabilityId
        if capability.Length > V1Constants.MaximumStringBytes then
            invalidArg (nameof capabilityId) "Capability identifier exceeds the ABI string bound."
        { Kind = RequestKind.SetEngagement
          ModuleRequestId = requestId
          Payload =
            Array.concat
                [ [| byte EngagementOperation.Declare; targetKind |]
                  CanonicalEncoding.int32LittleEndian targetId
                  [| byte capability.Length |]
                  capability ] }

    let cancelRequest requestId =
        { Kind = RequestKind.CancelAction
          ModuleRequestId = requestId
          Payload = [||] }

    let private readI32 (bytes: byte array) offset =
        int32 bytes[offset]
        ||| (int32 bytes[offset + 1] <<< 8)
        ||| (int32 bytes[offset + 2] <<< 16)
        ||| (int32 bytes[offset + 3] <<< 24)

    let private directionIndex direction =
        int32 (Direction8.toCode direction)

    let private directionDistance left right =
        let difference = abs (directionIndex left - directionIndex right)
        min difference (8 - difference)

    let private targetCell state target =
        match target with
        | PointCapabilityTarget unitId ->
            state.Units |> Map.tryFind unitId |> Option.map _.Cell
        | AreaCapabilityTarget referentId -> Map.tryFind referentId state.Areas

    let private directionTo (column, row) (targetColumn, targetRow) =
        Direction8.tryFromDelta
            (compare targetColumn column |> int32)
            (compare targetRow row |> int32)

    let private distance (column, row) (targetColumn, targetRow) =
        max (abs (targetColumn - column)) (abs (targetRow - row))

    let private decodeEngagement (payload: byte array) =
        if payload.Length < 7 || payload[0] <> byte EngagementOperation.Declare then
            Error "Malformed SetEngagement payload."
        else
            let target =
                match payload[1] with
                | value when value = byte TargetKind.KnownUnit ->
                    Ok(PointCapabilityTarget(readI32 payload 2))
                | value when value = byte TargetKind.Referent ->
                    Ok(AreaCapabilityTarget(readI32 payload 2))
                | _ -> Error "Unsupported engagement target kind."

            let length = int payload[6]

            if payload.Length <> 7 + length then
                Error "Malformed capability identifier."
            else
                target
                |> Result.map (fun value ->
                    value, utf8.GetString(payload, 7, length))

    let private validateTarget descriptor target =
        match descriptor.TargetContract, target with
        | PointTarget, PointCapabilityTarget _
        | AreaTarget, AreaCapabilityTarget _ -> Ok()
        | _ -> Error "Target shape is not permitted by the capability descriptor."

    /// Host-side structural validation before ruleset/loadout validation reaches state.
    let validateRequest request =
        match request.Kind with
        | RequestKind.SetEngagement ->
            match decodeEngagement request.Payload with
            | Error reason -> Error reason
            | Ok _ -> Ok()
        | RequestKind.CancelAction when request.Payload.Length = 0 -> Ok()
        | RequestKind.CancelAction -> Error "CancelAction payload must be empty."
        | _ -> Ok()

    let private beginEngagement state unitId target capabilityId unit =
        match HumanCapabilities.tryFind capabilityId with
        | None -> unit, [ CapabilityRejected(unitId, capabilityId, "unknown descriptor") ]
        | Some descriptor when not (Array.contains capabilityId unit.Loadout.CapabilityIds) ->
            unit, [ CapabilityRejected(unitId, capabilityId, "not present in authored loadout") ]
        | Some descriptor ->
            match validateTarget descriptor target, targetCell state target with
            | Error reason, _ -> unit, [ CapabilityRejected(unitId, capabilityId, reason) ]
            | _, None -> unit, [ CapabilityRejected(unitId, capabilityId, "target is unavailable") ]
            | Ok(), Some cell when distance unit.Cell cell > descriptor.MaximumRangeCells ->
                unit, [ CapabilityRejected(unitId, capabilityId, "target is out of range") ]
            | Ok(), Some cell ->
                let targetDirection = directionTo unit.Cell cell
                match targetDirection with
                | None -> unit, [ CapabilityRejected(unitId, capabilityId, "target has no direction") ]
                | Some direction ->
                    let traverse =
                        directionDistance unit.Attention direction
                        * descriptor.TraverseTicksPerDirection
                    let preserved =
                        Map.tryFind capabilityId unit.PreservedPreparation
                        |> Option.defaultValue 0
                    { unit with
                        PreservedPreparation =
                            Map.remove capabilityId unit.PreservedPreparation
                        Engagement =
                            Some
                                { CapabilityId = capabilityId
                                  Target = target
                                  RequiredAttention = direction
                                  TraverseTicksRemaining = traverse
                                  PreparationTicksRemaining =
                                      descriptor.PreparationTicks - preserved
                                  PreparedTicks = preserved } },
                    [ CapabilityTraversing(unitId, capabilityId, traverse) ]

    let private applyRequest state unitId request (unit, events) =
        match request.Kind with
        | RequestKind.SetAttention when request.Payload.Length = 1 ->
            match Direction8.tryFromCode request.Payload[0] with
            | Some direction -> { unit with Attention = direction }, events
            | None -> unit, CapabilityRejected(unitId, "", "invalid attention direction") :: events
        | RequestKind.SetEngagement ->
            match decodeEngagement request.Payload with
            | Ok(target, capabilityId) ->
                let changed, emitted = beginEngagement state unitId target capabilityId unit
                changed, List.rev emitted @ events
            | Error reason -> unit, CapabilityRejected(unitId, "", reason) :: events
        | RequestKind.CancelAction ->
            match unit.Engagement with
            | None -> unit, events
            | Some engagement ->
                let descriptor = HumanCapabilities.tryFind engagement.CapabilityId |> Option.get
                let preserve = descriptor.InterruptionRule = PreservePreparation
                let preserved =
                    if preserve then
                        Map.add
                            engagement.CapabilityId
                            engagement.PreparedTicks
                            unit.PreservedPreparation
                    else
                        Map.remove engagement.CapabilityId unit.PreservedPreparation
                { unit with
                    Engagement = None
                    PreservedPreparation = preserved },
                CapabilityInterrupted(unitId, engagement.CapabilityId, preserve) :: events
        | _ -> unit, events

    let private advance unitId (unit, events) =
        match unit.Engagement with
        | None -> unit, events
        | Some engagement ->
            let descriptor = HumanCapabilities.tryFind engagement.CapabilityId |> Option.get
            if engagement.TraverseTicksRemaining > 0 then
                let next =
                    { engagement with
                        TraverseTicksRemaining =
                            engagement.TraverseTicksRemaining - 1 }
                { unit with
                    Attention =
                        if next.TraverseTicksRemaining = 0 then
                            engagement.RequiredAttention
                        else
                            unit.Attention
                    Engagement = Some next },
                events
            elif engagement.PreparationTicksRemaining > 0 then
                let nextRemaining = engagement.PreparationTicksRemaining - 1
                let nextPrepared =
                    min descriptor.PreparationTicks (engagement.PreparedTicks + 1)
                let next =
                    { engagement with
                        PreparationTicksRemaining = nextRemaining
                        PreparedTicks = nextPrepared }
                let emitted =
                    if nextRemaining = 0 then
                        CapabilityPrepared(unitId, engagement.CapabilityId) :: events
                    else events
                { unit with Engagement = Some next }, emitted
            else
                let available =
                    Map.tryFind engagement.CapabilityId unit.Ammunition
                    |> Option.defaultValue 0
                if available < descriptor.AmmunitionPerResolution then
                    { unit with Engagement = None },
                    CapabilityRejected(unitId, engagement.CapabilityId, "ammunition unavailable") :: events
                else
                    let remaining = available - descriptor.AmmunitionPerResolution
                    let updated =
                        { unit with
                            Ammunition = Map.add engagement.CapabilityId remaining unit.Ammunition
                            PreservedPreparation =
                                Map.remove engagement.CapabilityId unit.PreservedPreparation
                            Engagement = None }
                    let resolved =
                        match engagement.Target with
                        | PointCapabilityTarget target ->
                            PointEngagementResolved(unitId, target, engagement.CapabilityId, remaining)
                        | AreaCapabilityTarget target ->
                            AreaEngagementResolved(unitId, target, engagement.CapabilityId, remaining)
                    updated, resolved :: events

    let runTick state requests =
        let mutable units = state.Units
        let mutable events = []

        requests
        |> List.sortBy (fun (unitId, request: Request) -> unitId, request.ModuleRequestId)
        |> List.iter (fun (unitId, request) ->
            match Map.tryFind unitId units with
            | None -> events <- CapabilityRejected(unitId, "", "unit unavailable") :: events
            | Some unit ->
                let changed, emitted = applyRequest state unitId request (unit, [])
                units <- Map.add unitId changed units
                events <- List.rev emitted @ events)

        units
        |> Map.toList
        |> List.iter (fun (unitId, unit) ->
            let changed, emitted = advance unitId (unit, [])
            units <- Map.add unitId changed units
            events <- List.rev emitted @ events)

        { State = { state with Tick = state.Tick + 1; Units = units }
          Events = List.rev events }

    let private text value = utf8.GetBytes(value + "\n")

    let stateDigest state =
        let targetText target =
            match target with
            | PointCapabilityTarget unitId -> "point:" + string unitId
            | AreaCapabilityTarget referentId -> "area:" + string referentId

        let units =
            state.Units
            |> Map.toList
            |> List.collect (fun (unitId, unit) ->
                [ string unitId
                  string (fst unit.Cell) + "," + string (snd unit.Cell)
                  string (Direction8.toCode unit.Attention)
                  String.concat "," unit.Loadout.CapabilityIds
                  unit.Ammunition
                  |> Map.toList
                  |> List.map (fun (id, amount) -> id + "=" + string amount)
                  |> String.concat ","
                  unit.PreservedPreparation
                  |> Map.toList
                  |> List.map (fun (id, amount) -> id + "=" + string amount)
                  |> String.concat ","
                  match unit.Engagement with
                  | None -> "-"
                  | Some engagement ->
                      String.concat
                          ":"
                          [ engagement.CapabilityId
                            targetText engagement.Target
                            string (Direction8.toCode engagement.RequiredAttention)
                            string engagement.TraverseTicksRemaining
                            string engagement.PreparationTicksRemaining
                            string engagement.PreparedTicks ] ])

        let areas =
            state.Areas
            |> Map.toList
            |> List.map (fun (referentId, (column, row)) ->
                "area=" + string referentId + ":" + string column + "," + string row)

        units @ areas
        |> String.concat "|"
        |> text
        |> CanonicalHash.sha256

    let eventsDigest events =
        events
        |> List.map string
        |> String.concat "|"
        |> text
        |> CanonicalHash.sha256

    let replay
        (initial: CapabilityExecutionState)
        finalTick
        (journal: CapabilityReplayRequest list)
        =
        let mutable state: CapabilityExecutionState = initial
        [ for tick in initial.Tick .. finalTick - 1 do
              let requests =
                  journal
                  |> List.choose (fun entry ->
                      if entry.Tick = tick then Some(entry.UnitId, entry.Request)
                      else None)
              let result = runTick state requests
              state <- result.State
              yield
                  { Tick = state.Tick
                    StateDigest = stateDigest state
                    EventsDigest = eventsDigest result.Events } ]

    let verifyReplay
        (initial: CapabilityExecutionState)
        finalTick
        (journal: CapabilityReplayRequest list)
        (expected: CapabilityReplayFrame list)
        =
        let actual = replay initial finalTick journal
        if actual = expected then Ok actual
        else
            let sharedLength = min actual.Length expected.Length
            let first =
                List.zip
                    (actual |> List.truncate sharedLength)
                    (expected |> List.truncate sharedLength)
                |> List.tryFindIndex (fun (left, right) -> left <> right)
                |> Option.map (fun index -> initial.Tick + index + 1)
                |> Option.defaultValue (initial.Tick + sharedLength + 1)
            Error first
