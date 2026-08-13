namespace SIR.Server

open System.Text.Json
open SIR.Domain
open SIR.Match
open SIR.Simulation

[<RequireQualifiedAccess>]
module AwarenessReactionDiagnostics =
    let isDisclosureSafe (payload: string) =
        not (payload.Contains("\"Units\""))
        && not (payload.Contains("\"Board\""))
        && not (payload.Contains("\"SpatialEvidence\""))

    let evaluate () =
        let observer = Simulation.unitId 10
        let subject = Simulation.unitId 20
        let blue = Simulation.initialState.Units[subject]
        let scenarioState =
            { Simulation.initialState with
                Board =
                    { Simulation.initialState.Board with
                        Maximum = { Col = 3; Row = 1 }
                        Edges = [] }
                Units = Simulation.initialState.Units |> Map.add subject { blue with Cell = { Col = 1; Row = 0 } } }
        let prepared =
            Simulation.runTick
                scenarioState
                [ SetAttention(observer, East)
                  PrepareAreaReaction(observer, "player-area-east", [ { Col = 2; Row = 0 } ], East) ]
        let acquired = Simulation.runTick prepared.State []
        let reacted = Simulation.runTick acquired.State [ Move(subject, { Col = 2; Row = 0 }) ]
        let projection = AwarenessProjection.forObserver observer reacted.State
        let contacts =
            projection.Contacts
            |> List.map (fun contact ->
                let column, row = contact.LastKnownCell |> Option.defaultValue (-1, -1)
                {| ObserverId = contact.ObserverId
                   SubjectId = contact.SubjectId
                   Level = string contact.Level
                   Acquisition = contact.Acquisition
                   HasLastKnownCell = contact.LastKnownCell.IsSome
                   LastKnownColumn = column
                   LastKnownRow = row
                   Reason = string contact.Reason |})
        let engagement =
            projection.Engagement
            |> Option.map (fun value ->
                {| OwnerId = value.OwnerId
                   EngagementId = value.EngagementId
                   Phase = string value.Phase
                   RemainingTicks = value.RemainingTicks
                   Reason = string value.Reason |})
        let events =
            reacted.Events
            |> List.choose (function
                | ReactionCommitted(reactor, source, engagementId) ->
                    Some $"committed:{UnitId.value reactor}:{UnitId.value source}:{engagementId}"
                | ReactionResolved(reactor, source, engagementId) ->
                    Some $"resolved:{UnitId.value reactor}:{UnitId.value source}:{engagementId}"
                | PhysicalAttackResolved(attacker, _, _, _) -> Some $"physical:{UnitId.value attacker}"
                | _ -> None)
        JsonSerializer.Serialize(
            {| Schema = "sir-local-awareness-v1"
               Tick = projection.Tick
               ObserverId = UnitId.value observer
               Contacts = contacts
               Engagement = engagement
               Events = events
               CandidatePairs = 2
               LosEvaluations = 2 |})
