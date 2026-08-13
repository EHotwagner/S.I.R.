namespace SIR.Server

open System.Text.Json
open SIR.Domain
open SIR.Match
open SIR.Simulation

[<RequireQualifiedAccess>]
module AwarenessReactionDiagnostics =
    let private observer = Simulation.unitId 10
    let private subject = Simulation.unitId 20
    let private gate = obj ()
    let mutable private timeline : TickResult list = []

    let private initial () =
        let blue = Simulation.initialState.Units[subject]
        { Simulation.initialState with
            Board = { Simulation.initialState.Board with Maximum = { Col = 3; Row = 1 }; Edges = [] }
            Units = Simulation.initialState.Units |> Map.add subject { blue with Cell = { Col = 1; Row = 0 } } }

    let isDisclosureSafe (payload: string) =
        not (payload.Contains("\"Units\""))
        && not (payload.Contains("\"Board\""))
        && not (payload.Contains("\"SpatialEvidence\""))

    let evaluate body = lock gate (fun () ->
        let action =
            if System.String.IsNullOrWhiteSpace body then "snapshot"
            else
                use document = JsonDocument.Parse body
                document.RootElement.GetProperty("action").GetString()
                |> Option.ofObj
                |> Option.defaultValue "snapshot"
        if action = "reset" || timeline.IsEmpty then
            timeline <- [ Simulation.runTick (initial ()) [] ]
        let append inputs =
            let result = Simulation.runTick timeline.Head.State inputs
            timeline <- result :: timeline
            result
        let reacted =
            match action with
            | "rotate-attention" -> append [ SetAttention(observer, East); SetWeaponPosture(observer, WeaponPosture.Prepared) ]
            | "prepare-coverage" -> append [ PrepareAreaReaction(observer, "player-area-east", [ { Col = 2; Row = 0 } ], East) ]
            | "advance-preparation" -> append []
            | "move-opponent" -> append [ Move(subject, { Col = 2; Row = 0 }) ]
            | "seek-start" -> timeline |> List.last
            | "seek-end" | "snapshot" | "reset" -> timeline.Head
            | _ -> failwith $"Unknown awareness player action: {action}"
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
                   Sector = contact.Sector |> Option.map string |> Option.defaultValue "Unknown"
                   Reason = string contact.Reason |})
        let engagement =
            projection.Engagement
            |> Option.map (fun value ->
                {| OwnerId = value.OwnerId
                   EngagementId = value.EngagementId
                   Phase = string value.Phase
                   RemainingTicks = value.RemainingTicks
                   Target = value.Target
                   RequiredAttention = string value.RequiredAttention
                   WeaponPosture = string value.WeaponPosture
                   Reason = string value.Reason |})
        let stimuli =
            projection.Stimuli
            |> List.map (fun stimulus ->
                let column, row = stimulus.SubjectCell
                {| ObserverId = stimulus.ObserverId
                   SubjectId = stimulus.SubjectId
                   Tick = stimulus.Tick
                   Sector = string stimulus.Sector
                   SubjectColumn = column
                   SubjectRow = row
                   Reason = string stimulus.Reason |})
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
               Stimuli = stimuli
               Engagement = engagement
               Events = events
               CandidatePairs = 2
               LosEvaluations = reacted.AwarenessCounters.LosEvaluations |}))
