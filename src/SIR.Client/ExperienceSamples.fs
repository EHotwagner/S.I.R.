namespace SIR.Client

open SIR.Domain

type ExperienceMapSample =
    { Id: string
      Title: string
      Summary: string
      Highlights: string list
      MapText: string }

type ExperienceReplaySample =
    { Id: string
      Title: string
      Summary: string
      MapSampleId: string
      Ticks: int32 }

[<RequireQualifiedAccess>]
module ExperienceSamples =
    let private tacticalDensitySample unitCount =
        let lines =
            [ yield "SIR-MAP 2"
              yield "size 20 20"
              for index in 0 .. 19 do
                  yield "terrain " + string index + " 5 " + (if index % 7 = 0 then "objective" else "rough")
              for index in 0 .. 9 do
                  yield "edge " + string index + " 6 east " + (if index % 4 = 0 then "door" else "wall") + " closed"
              for index in 0 .. unitCount - 1 do
                  yield
                      "unit " + string (index + 1)
                      + (if index % 2 = 0 then " blue " else " red ")
                      + (if index % 5 = 0 then "medic " else "rifleman ")
                      + string (index % 20) + " " + string (4 + index / 20)
                      + (if index % 3 = 0 then " 1 12 12 scripted E,E,N" else " 1 12 12 general -") ]
        { Id = "tactical-density-" + string unitCount
          Title = "Tactical density " + string unitCount
          Summary = "Production 100/200-unit visual workload with terrain, edges, statuses, overlays, effects, and input."
          Highlights =
            [ "Overlapping opposing formations and semantic terrain"
              "Production projection, SVG layout, input, effects, and overlays"
              "Deterministic representative/stress performance fixture" ]
          MapText = String.concat "\n" lines + "\n" }

    let maps: ExperienceMapSample list =
        [ { Id = "troll-assault"
            Title = "Troll assault"
            Summary = "Three riflemen meet a 240 HP armored troll advancing across open ground."
            Highlights =
                [ "Large 3×3 footprint versus a dispersed firing line"
                  "General-controller target choice, movement, collision, and attrition"
                  "Useful for exposing the current close-combat controller's limits" ]
            MapText =
                """SIR-MAP 2
size 16 10
terrain 7 2 rough
terrain 7 3 rough
terrain 7 4 rough
terrain 7 5 rough
terrain 7 6 rough
terrain 7 7 rough
zone 1 deployment blue rectangle 0 0 4 10
zone 2 deployment red rectangle 11 0 5 10
unit 1 blue rifleman 1 0 2 12 12 general -
unit 2 blue rifleman 1 4 2 12 12 general -
unit 3 blue rifleman 1 8 2 12 12 general -
unit 4 red troll 12 3 3 240 240 general -
""" };
          { Id = "breach-corridor"
            Title = "Breach corridor"
            Summary = "A human section and goblin defenders converge on a single semantic door."
            Highlights =
                [ "Walls, a closed door, and constrained movement"
                  "Rough terrain around the breach"
                  "Controller collision feedback at a bottleneck" ]
            MapText =
                """SIR-MAP 2
size 14 10
terrain 5 3 rough
terrain 5 4 rough
terrain 5 5 rough
terrain 5 6 rough
edge 6 0 east wall closed
edge 6 1 east wall closed
edge 6 2 east wall closed
edge 6 3 east wall closed
edge 6 4 east door closed
edge 6 5 east wall closed
edge 6 6 east wall closed
edge 6 7 east wall closed
edge 6 8 east wall closed
edge 6 9 east wall closed
unit 1 blue rifleman 1 2 2 12 12 general -
unit 2 blue medic 1 6 2 12 12 general -
unit 3 red goblin 10 2 1 12 12 general -
unit 4 red goblin 10 6 1 12 12 general -
""" };
          { Id = "objective-crossing"
            Title = "Objective crossing"
            Summary = "Opposing patrols contest a central objective through rough and blocked ground."
            Highlights =
                [ "Objective and deployment-zone semantics"
                  "Terrain routing around blocked cells"
                  "Mixed unit footprints in a compact encounter" ]
            MapText =
                """SIR-MAP 2
size 12 12
terrain 4 4 rough
terrain 5 4 rough
terrain 6 4 rough
terrain 7 4 rough
terrain 5 5 objective
terrain 6 5 objective
terrain 5 6 objective
terrain 6 6 objective
terrain 4 7 rough
terrain 5 7 rough
terrain 6 7 blocked
terrain 7 7 rough
zone 1 objective rectangle 5 5 2 2
zone 2 deployment blue rectangle 0 0 4 4
zone 3 deployment red rectangle 8 8 4 4
unit 1 blue rifleman 1 1 2 12 12 general -
unit 2 blue observation-drone 3 1 1 8 8 general -
unit 3 red goblin 9 9 1 12 12 general -
unit 4 red orc 7 8 2 35 35 general -
""" };
          tacticalDensitySample 100;
          tacticalDensitySample 200 ]

    let replays: ExperienceReplaySample list =
        [ { Id = "troll-contact"
            Title = "Troll reaches the line"
            Summary = "Follow the troll assault from deployment through first contact and early attrition."
            MapSampleId = "troll-assault"
            Ticks = 20 };
          { Id = "breach-stalemate"
            Title = "Closed-door stalemate"
            Summary = "Inspect controller events as both sides discover that the closed breach blocks advance."
            MapSampleId = "breach-corridor"
            Ticks = 8 } ]

    let tryMap id =
        maps |> List.tryFind (fun sample -> sample.Id = id)

    let tryReplay id =
        replays |> List.tryFind (fun sample -> sample.Id = id)

    let editorState (sample: ExperienceMapSample) =
        MapEditor.initial
        |> MapEditor.update (LoadMapText sample.MapText)
        |> MapEditor.update (SetMapName sample.Title)
        |> fun state ->
            let selected =
                state.Map.Units |> Map.toSeq |> Seq.map fst |> Seq.tryHead
            { state with
                SelectedUnit = selected
                SelectedUnits =
                    selected
                    |> Option.map Set.singleton
                    |> Option.defaultValue Set.empty }

    let simulator (sample: ExperienceMapSample) =
        editorState sample
        |> MapEditorSimulator.tryHandoff
        |> Result.toOption

    let private combatSource delivery =
        match delivery with
        | MeleeDelivery -> "combat-melee"
        | ProjectileDelivery -> "combat-projectile"
        | LobbedAreaDelivery -> "combat-lobbed-area"
        | SpellAreaDelivery -> "combat-spell-area"

    let private inspection
        tick
        (map: MapDefinition)
        events
        (combatEvents: SimulatorCombatEvent list)
        : InspectionProjection
        =
        let combatSummaries =
            combatEvents |> List.map _.Summary |> Set.ofList
        let narrativeEvents =
            events
            |> List.filter (fun summary ->
                not (Set.contains summary combatSummaries))
            |> List.mapi (fun index summary ->
                { Id = tick * 100 + int32 index
                  Tick = tick
                  Source = "sample-simulation"
                  Summary = summary
                  SourceUnitId = None
                  TargetUnitId = None })
        let projectedCombatEvents =
            combatEvents
            |> List.filter (fun combat -> combat.Tick = tick)
            |> List.mapi (fun index combat ->
                { Id = tick * 100 + 50 + int32 index
                  Tick = tick
                  Source = combatSource combat.Delivery
                  Summary = combat.Summary
                  SourceUnitId = Some combat.SourceUnitId
                  TargetUnitId =
                    match combat.Target with
                    | UnitCombatTarget unitId -> Some unitId
                    | AreaCombatTarget _ -> None })
        { Tick = tick
          BoardMinimumColumn = 0
          BoardMinimumRow = 0
          BoardMaximumColumn = map.Width - 1
          BoardMaximumRow = map.Height - 1
          Units =
            map.Units
            |> Map.toList
            |> List.map (fun (_, unit) ->
                { Id = unit.Id
                  Side =
                    match unit.Side with
                    | Blue -> "Blue"
                    | Red -> "Red"
                    | NeutralSide -> "Neutral"
                  Column = unit.Column
                  Row = unit.Row
                  Health = unit.Health
                  HealthMaximum = unit.HealthMaximum
                  MovementDirection = None
                  BodyFacing = int32 (Direction8.toCode North)
                  AttentionDirection = int32 (Direction8.toCode North) })
          Edges =
            map.Edges
            |> Map.toList
            |> List.mapi (fun index ((column, row, direction), (kind, isOpen)) ->
                { Id = "sample-edge-" + string index
                  Kind = string kind
                  State = if isOpen then "open" else "closed"
                  StartColumn = column
                  StartRow = row
                  EndColumn =
                    column
                    + if direction = EastEdge then 0 else 1
                  EndRow =
                    row
                    + if direction = SouthEdge then 0 else 1 })
          Events = List.append narrativeEvents projectedCombatEvents
          Checkpoints =
            [ { Tick = tick
                StateHash = "sample-" + string tick
                EventHash = "sample-events-" + string tick } ]
          PerspectiveHash = None }

    let replayFrames (replay: ExperienceReplaySample) =
        match tryMap replay.MapSampleId |> Option.bind simulator with
        | None -> [||]
        | Some initial ->
            let frames = ResizeArray<InspectionProjection>()
            let mutable handoff = initial
            frames.Add(inspection 0 handoff.RuntimeMap [] [])
            for _ in 1 .. replay.Ticks do
                handoff <-
                    MapEditorSimulator.update
                        StepSimulator
                        (handoff.RuntimeMap.Units |> Map.toSeq |> Seq.map fst |> Seq.tryHead)
                        handoff
                frames.Add(
                    inspection
                        handoff.Tick
                        handoff.RuntimeMap
                        handoff.LastEvents
                        handoff.LastCombatEvents
                )
            frames.ToArray()
