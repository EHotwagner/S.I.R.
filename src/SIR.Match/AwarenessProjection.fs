namespace SIR.Match

open SIR.Domain
open SIR.Simulation

type LocalAwarenessProjection =
    { ObserverId: int32
      SubjectId: int32
      Level: AwarenessLevel
      Acquisition: int32
      LastKnownCell: (int32 * int32) option
      Sector: ObservationSector option
      Reason: AwarenessReason }

type LocalEngagementProjection =
    { OwnerId: int32
      EngagementId: string
      Phase: EngagementPhase
      RemainingTicks: int32
      Target: string
      RequiredAttention: Direction8
      WeaponPosture: WeaponPosture
      Reason: ReactionReason }

type LocalAwarenessFrame =
    { Tick: int32
      Contacts: LocalAwarenessProjection list
      Engagement: LocalEngagementProjection option }

[<RequireQualifiedAccess>]
module AwarenessProjection =
    /// Projects only one observer's locally accumulated knowledge. World units,
    /// LOS geometry and other observers' contacts are deliberately unavailable.
    let forObserver observerId (state: SimulationState) =
        let observer = state.Units |> Map.tryFind observerId
        let contacts =
            state.Awareness
            |> Map.toList
            |> List.choose (fun ((ownerId, subjectId), contact) ->
                if ownerId <> observerId then None
                else
                    Some
                        { ObserverId = UnitId.value ownerId
                          SubjectId = UnitId.value subjectId
                          Level = contact.Level
                          Acquisition = contact.Acquisition
                          LastKnownCell = contact.LastKnownCell |> Option.map (fun cell -> cell.Col, cell.Row)
                          Sector =
                            match observer, contact.LastKnownCell with
                            | Some owner, Some cell -> Some(AwarenessReaction.sector owner.AttentionDirection owner.Cell cell)
                            | _ -> None
                          Reason = contact.Reason })
        let engagement =
            state.Engagements
            |> Map.tryFind observerId
            |> Option.map (fun value ->
                let target =
                    match value.Target with
                    | EngagementTarget.KnownUnit targetId -> $"unit:{UnitId.value targetId}"
                    | EngagementTarget.CoveredArea cells ->
                        cells
                        |> List.map (fun cell -> $"{cell.Col},{cell.Row}")
                        |> String.concat ";"
                        |> fun encoded -> "area:" + encoded
                    | EngagementTarget.GuardedEdge edge ->
                        $"edge:{edge.Lo.Col},{edge.Lo.Row}-{edge.Hi.Col},{edge.Hi.Row}"
                { OwnerId = UnitId.value observerId
                  EngagementId = value.EngagementId
                  Phase = value.Phase
                  RemainingTicks = value.RemainingTicks
                  Target = target
                  RequiredAttention = value.RequiredAttention
                  WeaponPosture = observer |> Option.map _.WeaponPosture |> Option.defaultValue WeaponPosture.Mobile
                  Reason = value.Reason })
        { Tick = state.Tick; Contacts = contacts; Engagement = engagement }
