namespace SIR.Match

open System
open System.Text
open SIR.ControlAbi
open SIR.Domain
open SIR.Simulation

/// Every mutable-looking live session is admitted against this immutable identity set.
type LivePinnedIdentities =
    { MapRevision: byte array
      PlanSemantic: byte array
      PlanSource: byte array
      Ruleset: string
      DescriptorSet: string
      ControllerArtifact: byte array
      Engine: byte array
      Replay: byte array
      MatchLock: byte array }

/// Knowledge-scoped state sent across the simulator worker boundary.
type LiveProjectionFrame = AuthoritativeProjectionFrame

type LiveReplay =
    { Identities: LivePinnedIdentities
      Frames: LiveProjectionFrame array
      FinalStateIdentity: byte array
      JournalIdentity: byte array
      ProjectionIdentity: byte array }

type AcceptedPlanningArtifact =
    { PlanId: Guid
      Revision: int64
      SemanticIdentity: byte array
      SourceIdentity: byte array
      MapRevision: byte array
      Ruleset: string
      MatchLock: byte array }

type LiveAdmission =
    { Session: string
      Actor: string
      MatchLock: byte array
      LastServerSequence: int64
      LastProjectionRevision: int64 }

type LiveReconnect =
    | ResumeWith of LiveProjectionFrame array
    | ReplaceWithSnapshot of LiveProjectionFrame

type LiveQualification =
    { PlanSource: byte array
      Artifact: AcceptedPlanningArtifact
      Replay: LiveReplay
      ControllerInvocations: int32
      KernelTicks: int32
      CapabilityEvents: int32 }

[<RequireQualifiedAccess>]
module LiveIntegration =
    let private utf8 = UTF8Encoding(false, true)
    let private ruleset = "sir.continuous.prototype.v1"
    let private descriptorIdentity =
        HumanCapabilities.DescriptorSetId
        + "@"
        + string HumanCapabilities.DescriptorSetVersion
    let private engineIdentity =
        CanonicalHash.sha256 (utf8.GetBytes "sir.map-scale.continuous-engine.v1")

    let private bytes values = CanonicalEncoding.concatenate values
    let private segment (value: byte array) =
        bytes [ CanonicalEncoding.int32LittleEndian value.Length; value ]
    let private i64 value =
        [| for shift in 0 .. 8 .. 56 -> byte (uint64 value >>> shift) |]

    let private projectionBytes (frame: LiveProjectionFrame) =
        bytes
            [ CanonicalEncoding.int32LittleEndian frame.Tick
              i64 frame.ServerSequence
              i64 frame.ProjectionRevision
              CanonicalEncoding.int32LittleEndian frame.VisibleUnits.Length
              for unit in frame.VisibleUnits do
                  CanonicalEncoding.int32LittleEndian unit.UnitId
                  CanonicalEncoding.int32LittleEndian unit.DisplayColumn
                  CanonicalEncoding.int32LittleEndian unit.DisplayRow
                  CanonicalEncoding.int32LittleEndian unit.Health
              frame.StateIdentity
              frame.EventIdentity ]

    let serializeProjection frame = projectionBytes frame

    let private command index kind predecessors =
        { CommandId = Guid.ParseExact(sprintf "%032x" index, "N")
          EarliestStartTick = 1
          Predecessors = predecessors
          InterruptionPolicy = ApplyFallback
          Fallback = HoldPosition
          Kind = kind
          Annotation = "" }

    let private unitPlan unitId origin destination target =
        let move = command (unitId * 100 + 1) (MovePath([| origin; destination |], Balanced)) [||]
        let face = command (unitId * 100 + 2) (SetFacingIntent(FaceFixed East)) [| move.CommandId |]
        let attention =
            command (unitId * 100 + 3) (SetAttentionIntent(AttendFixed East)) [| face.CommandId |]
        let sync =
            command
                (unitId * 100 + 4)
                (Synchronize
                    { MarkerId = "line-ready"
                      Mode = PreloadedClock 12
                      DeadlineTick = 16
                      Timeout = Continue })
                [| attention.CommandId |]
        let engage =
            command
                (unitId * 100 + 5)
                (EngageUnit(target, "human.weapon.rifle"))
                [| sync.CommandId |]
        let hold = command (unitId * 100 + 6) Hold [| engage.CommandId |]
        { UnitId = unitId
          ControllerArtifact = StandardController.artifactBytes ()
          Commands = [| move; face; attention; sync; engage; hold |]
          Fallback = AbortUnitPlan }

    let private representativeMap () =
        let unit id side column row =
            id,
            { Id = id
              Side = side
              ClassId = "rifleman"
              Cell = MapScale.cell column row
              Size = 1
              Health = 20
              Controller = GeneralController
              Script = []
              ScriptIndex = 0
              BodyFacing = North
              AttentionDirection = North }
        { Tick = 0
          Board =
            { Width = 8
              Height = 4
              Terrain = Map.empty
              Edges = Map.empty }
          Units =
            [ unit 1 1 0 1; unit 2 1 0 2; unit 20 2 5 1 ] |> Map.ofList
          MovementCreditsMillimeters = Map.empty
          MovementProgress = Map.empty
          MovementIntents = Map.empty
          PlannedRoutes = Map.empty
          Engagements = Map.empty }

    let private capabilityState () =
        let capabilityUnit id column row =
            let loadout =
                HumanCapabilities.createLoadout id "rifleman" [| "human.weapon.rifle" |]
                |> Result.defaultWith invalidOp
            id,
            { Loadout = loadout
              Cell = column, row
              Attention = North
              Ammunition = Map.ofList [ "human.weapon.rifle", 10 ]
              PreservedPreparation = Map.empty
              Engagement = None }
        { Tick = 0
          Units =
            [ capabilityUnit 1 0 1
              capabilityUnit 2 0 2
              capabilityUnit 20 5 1 ]
            |> Map.ofList
          Areas = Map.empty }

    let private mapIdentity (state: MapScaleState) =
        let checkpoint: MapScaleCheckpoint =
            { Tick = state.Tick
              Phase = MapScalePhase.CommitPhase
              State = state
              Events = [] }
        MapScale.checkpointBytes checkpoint |> CanonicalHash.sha256

    let private plan () =
        let map = representativeMap ()
        let mapRevision = mapIdentity map
        let document =
            { FormatVersion = SirPlan.FormatVersion
              PlanId = Guid.ParseExact("99999999999999999999999999999999", "N")
              Revision = 9L
              ParentDigest = None
              MapRevisionDigest = mapRevision
              RulesetIdentity = ruleset
              StartTick = 1
              HorizonTicks = 40
              UnitPlans =
                [| unitPlan 1 (MapScale.cell 0 1) (MapScale.cell 1 1) 20
                   unitPlan 2 (MapScale.cell 0 2) (MapScale.cell 1 2) 20 |] }
        let context =
            { Map = map
              RulesetIdentity = ruleset
              MapRevisionDigest = mapRevision
              MaximumConfigurationBytes = ControlHost.defaultProfile.MaximumConfigurationBytes }
        document, context

    let private input tick unitId =
        { Kind = MessageKind.Input
          MinorVersion = V1Constants.Minor
          Tick = tick
          UnitId = unitId
          Flags = 0u
          Budget = uint32 ControlHost.defaultProfile.FuelPerInvocation
          Sections =
            [ { Tag = V1Constants.OwnStateTag
                Required = true
                ElementCount = 1
                Payload = [| 1uy |] } ] }
        |> V1Codec.encode
        |> Result.defaultWith (fun error -> failwithf "Could not encode live input: %A" error)

    let private execute () =
        let document, context = plan ()
        let source = SirPlan.encode document |> Result.defaultWith invalidOp
        let decoded = SirPlan.decode source |> Result.defaultWith invalidOp
        let compiled =
            SirPlan.compile context decoded
            |> Result.defaultWith (fun issues -> failwithf "Live plan failed validation: %A" issues)
        let controllerBytes = StandardController.artifactBytes ()
        use artifact =
            ControlHost.compile ControlHost.defaultProfile "sir-standard-live" controllerBytes
        let instances =
            compiled.Units
            |> Array.map (fun unit ->
                unit.UnitId,
                ControlHost.instantiate artifact unit.UnitId unit.Configuration)
        let mutable state = context.Map
        let mutable capabilities = capabilityState ()
        let frames = ResizeArray<LiveProjectionFrame>()
        let authoritativeIdentities = ResizeArray<int32 * byte array * byte array>()
        let mutable invocationCount = 0
        let mutable capabilityEventCount = 0
        try
            for tick in document.StartTick .. document.HorizonTicks do
                let accepted =
                    instances
                    |> Array.map (fun (unitId, instance) ->
                        invocationCount <- invocationCount + 1
                        match ControlHost.invoke tick (input tick unitId) instance with
                        | Accepted(output, _) -> unitId, output.Requests
                        | SleepingUntil _ -> unitId, []
                        | result -> failwithf "Live controller failed at tick %d: %A" tick result)
                    |> Array.toList
                for unitId, requests in accepted do
                    state <-
                        ControlHost.applyToMapScale state unitId requests
                        |> Result.defaultWith invalidOp
                let mapResult = MapScale.tick state
                state <- mapResult.State
                let capabilityResult = ControlHost.applyToCapabilities capabilities accepted
                capabilities <- capabilityResult.State
                capabilityEventCount <- capabilityEventCount + capabilityResult.Events.Length
                let commit =
                    mapResult.Checkpoints
                    |> List.find (fun checkpoint ->
                        checkpoint.Phase = MapScalePhase.CommitPhase)
                let visible =
                    state.Units
                    |> Map.toArray
                    |> Array.choose (fun (_, unit) ->
                        if unit.Side = 1 then
                            Some
                                { UnitId = unit.Id
                                  DisplayColumn = unit.Cell.Col
                                  DisplayRow = unit.Cell.Row
                                  Health = unit.Health }
                        else None)
                let visibleIdentity =
                    visible
                    |> Array.collect (fun unit ->
                        bytes
                            [ CanonicalEncoding.int32LittleEndian unit.UnitId
                              CanonicalEncoding.int32LittleEndian unit.DisplayColumn
                              CanonicalEncoding.int32LittleEndian unit.DisplayRow
                              CanonicalEncoding.int32LittleEndian unit.Health ])
                    |> CanonicalHash.sha256
                let authoritativeState =
                    CanonicalHash.sha256 (
                        bytes
                            [ MapScale.checkpointBytes commit
                              CapabilityExecution.stateDigest capabilities ]
                    )
                let authoritativeEvents =
                    CanonicalHash.sha256 (
                        bytes
                            [ MapScale.checkpointBytes commit
                              CapabilityExecution.eventsDigest capabilityResult.Events ]
                    )
                authoritativeIdentities.Add(tick, authoritativeState, authoritativeEvents)
                frames.Add
                    { Tick = tick
                      ServerSequence = int64 tick
                      ProjectionRevision = int64 tick
                      VisibleUnits = visible
                      StateIdentity = visibleIdentity
                      // This slice discloses no event bodies, so its event identity
                      // is deliberately constant and reveals no hidden activity.
                      EventIdentity = CanonicalHash.sha256 [||] }
        finally
            instances
            |> Array.iter (fun (_, instance) -> (instance :> IDisposable).Dispose())
        document,
        source,
        compiled,
        controllerBytes,
        frames.ToArray(),
        authoritativeIdentities.ToArray(),
        invocationCount,
        capabilityEventCount

    let qualify () =
        let (
            document,
            source,
            compiled,
            controllerBytes,
            frames,
            authoritativeIdentities,
            invocations,
            capabilityEvents
            ) =
            execute ()
        let projectionIdentity =
            frames |> Array.map projectionBytes |> Array.toList |> bytes |> CanonicalHash.sha256
        let journalIdentity =
            authoritativeIdentities
            |> Array.collect (fun (tick, stateIdentity, eventIdentity) ->
                bytes
                    [ CanonicalEncoding.int32LittleEndian tick
                      stateIdentity
                      eventIdentity ])
            |> CanonicalHash.sha256
        let lockIdentity =
            bytes
                [ segment document.MapRevisionDigest
                  segment compiled.SemanticDigest
                  segment compiled.SourceDigest
                  segment (utf8.GetBytes document.RulesetIdentity)
                  segment (utf8.GetBytes descriptorIdentity)
                  segment (CanonicalHash.sha256 controllerBytes)
                  segment engineIdentity ]
            |> CanonicalHash.sha256
        let replayIdentity =
            bytes [ lockIdentity; journalIdentity; projectionIdentity ]
            |> CanonicalHash.sha256
        let identities =
            { MapRevision = document.MapRevisionDigest
              PlanSemantic = compiled.SemanticDigest
              PlanSource = compiled.SourceDigest
              Ruleset = document.RulesetIdentity
              DescriptorSet = descriptorIdentity
              ControllerArtifact = CanonicalHash.sha256 controllerBytes
              Engine = engineIdentity
              Replay = replayIdentity
              MatchLock = lockIdentity }
        let replay =
            { Identities = identities
              Frames = frames
              FinalStateIdentity =
                authoritativeIdentities[authoritativeIdentities.Length - 1]
                |> fun (_, stateIdentity, _) -> stateIdentity
              JournalIdentity = journalIdentity
              ProjectionIdentity = projectionIdentity }
        { PlanSource = source
          Artifact =
            { PlanId = document.PlanId
              Revision = document.Revision
              SemanticIdentity = compiled.SemanticDigest
              SourceIdentity = compiled.SourceDigest
              MapRevision = document.MapRevisionDigest
              Ruleset = document.RulesetIdentity
              MatchLock = lockIdentity }
          Replay = replay
          ControllerInvocations = invocations
          KernelTicks = frames.Length
          CapabilityEvents = capabilityEvents }

    let verify replay =
        let rerun = qualify ()
        let suppliedProjection =
            replay.Frames
            |> Array.map projectionBytes
            |> Array.toList
            |> bytes
            |> CanonicalHash.sha256
        let suppliedReplay =
            bytes
                [ replay.Identities.MatchLock
                  replay.JournalIdentity
                  suppliedProjection ]
            |> CanonicalHash.sha256
        suppliedProjection = replay.ProjectionIdentity
        && suppliedReplay = replay.Identities.Replay
        && rerun.Replay.JournalIdentity = replay.JournalIdentity
        && rerun.Replay.ProjectionIdentity = replay.ProjectionIdentity
        && rerun.Replay.FinalStateIdentity = replay.FinalStateIdentity
        && rerun.Replay.Identities = replay.Identities

    let admit
        session
        actor
        (artifact: AcceptedPlanningArtifact)
        (expected: AcceptedPlanningArtifact)
        =
        if artifact <> expected then Error "SIR.LIVE.ADMISSION.ARTIFACT_MISMATCH"
        elif String.IsNullOrWhiteSpace session || String.IsNullOrWhiteSpace actor then
            Error "SIR.LIVE.ADMISSION.IDENTITY_REQUIRED"
        else
            Ok
                { Session = session
                  Actor = actor
                  MatchLock = expected.MatchLock
                  LastServerSequence = 0L
                  LastProjectionRevision = 0L }

    let reconnect admission replay lastServerSequence lastProjectionRevision =
        if admission.MatchLock <> replay.Identities.MatchLock then
            Error "SIR.LIVE.RECONNECT.MATCH_LOCK_MISMATCH"
        elif lastServerSequence < 0L
             || lastProjectionRevision < 0L
             || lastServerSequence > int64 replay.Frames.Length
             || lastProjectionRevision > int64 replay.Frames.Length then
            Error "SIR.LIVE.RECONNECT.CURSOR_INVALID"
        elif lastServerSequence <> lastProjectionRevision then
            Error "SIR.LIVE.RECONNECT.PROJECTION_GAP"
        else
            let retained =
                replay.Frames
                |> Array.filter (fun frame -> frame.ServerSequence > lastServerSequence)
            if retained.Length <= 16 then Ok(ResumeWith retained)
            else Ok(ReplaceWithSnapshot replay.Frames[replay.Frames.Length - 1])
