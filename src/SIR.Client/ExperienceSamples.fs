namespace SIR.Client

open System
open System.Text
open SIR.Domain

type ScenarioFamily =
    | FastStartTeaching
    | OpenFieldMovementFire
    | CoverDenseAssaultFlank
    | DoorBreachInteriorClear
    | SupportByFireSuppression
    | ArmoredAntiArmorResponse
    | MultiObjectiveWithdrawalReinforcement

type ExperienceMapSample =
    { Id: string; Title: string; Summary: string; Family: ScenarioFamily; Lesson: string
      Highlights: string list; DesignNotes: string list; MapText: string }

type ExperienceReplaySample =
    { Id: string; Title: string; Summary: string; MapSampleId: string; Ticks: int32 }

type ScenarioIdentity =
    { Engine: string; Ruleset: string; Content: string; MapRevision: string; ContentDigest: string }

type ScenarioForce =
    { UnitId: int32; Capability: string; Loadout: string; InitialFacing: string
      InitialAttention: string; InitialKnowledge: string }

type ScenarioPlan = { Side: string; Name: string; Steps: string list }
type ScenarioObjective = { Id: string; Summary: string; ZoneId: int32 option }
type ScenarioCheckpoint = { Tick: int32; MinimumEvents: int32; VisibleOutcome: string }

type ExperienceScenarioPackage =
    { SchemaVersion: int32; CatalogVersion: string; Identity: ScenarioIdentity; Map: ExperienceMapSample
      Forces: ScenarioForce list; Plans: ScenarioPlan list; Objectives: ScenarioObjective list
      InitialKnowledge: string list; Seed: uint64; RandomAddress: string
      ExpectedCheckpoints: ScenarioCheckpoint list; Replay: ExperienceReplaySample }

type ScenarioValidationError =
    | UnsupportedSchema of int32
    | StaleEngine of string
    | StaleRuleset of string
    | StaleContent of string
    | StaleMapRevision of string
    | StaleContentDigest of string
    | StaleReplayBinding of string
    | MissingScenarioContent of string
    | MalformedScenarioPackage of string

type ScenarioCatalogCost =
    { ScenarioCount: int32; UnitCount: int32; TerrainCount: int32; EdgeCount: int32
      ZoneCount: int32; CheckpointCount: int32; ReplayTickCount: int32; CanonicalBytes: int32 }

[<RequireQualifiedAccess>]
module ExperienceSamples =
    let private engine = EngineCatalog.Current.Identity
    let private ruleset = "sir-rules-v2"
    let private content = "scenario-catalog-v1"

    let private familyName = function
        | FastStartTeaching -> "fast-start-teaching"
        | OpenFieldMovementFire -> "open-field-movement-fire"
        | CoverDenseAssaultFlank -> "cover-dense-assault-flank"
        | DoorBreachInteriorClear -> "door-breach-interior-clear"
        | SupportByFireSuppression -> "support-by-fire-suppression"
        | ArmoredAntiArmorResponse -> "armored-anti-armor-response"
        | MultiObjectiveWithdrawalReinforcement -> "multi-objective-withdrawal-reinforcement"

    let private unitLines count =
        [ for index in 0 .. count - 1 do
              let id = index + 1
              let blue = index < count / 2
              let column = if blue then 1 + (index % 3) * 3 else 25 + (index % 3) * 3
              let row = 1 + (index % 6) * 4
              let side, kind = if blue then "blue", "rifleman" else "red", "goblin"
              yield $"unit {id} {side} {kind} {column} {row} 1 12 12 general -" ]

    let private stressUnitLines =
        [ for index in 0 .. 199 do
              let id = index + 1
              let blue = index < 100
              let local = if blue then index else index - 100
              let column = if blue then local % 20 else 20 + local % 20
              let row = local / 20 * 2
              let side, kind = if blue then "blue", "rifleman" else "red", "goblin"
              yield $"unit {id} {side} {kind} {column} {row} 1 12 12 general -" ]

    let private mapText width height terrain edges zones units =
        [ yield "SIR-MAP 2"
          yield $"size {width} {height}"
          yield! terrain
          yield! edges
          yield! zones
          yield! units ]
        |> String.concat "\n"
        |> fun value -> value + "\n"

    let private map id title summary family lesson highlights notes mapText =
        { Id = id; Title = title; Summary = summary; Family = family; Lesson = lesson
          Highlights = highlights; DesignNotes = notes; MapText = mapText }

    let private replay id title summary mapId ticks =
        { Id = id; Title = title; Summary = summary; MapSampleId = mapId; Ticks = ticks }

    let private teachingMap =
        map "quick-contact" "Quick contact" "A four-unit first contact that reaches a visible outcome immediately."
            FastStartTeaching "Open, run, and inspect one complete deterministic exchange."
            [ "Four-unit onboarding"; "Immediate movement and contact" ]
            [ "Small by design; use the six tactical families for composed scenarios." ]
            (mapText 14 10 [ "terrain 6 4 rough" ] []
                [ "zone 1 objective rectangle 6 4 2 2" ]
                [ "unit 1 blue rifleman 1 2 1 12 12 general -"; "unit 2 blue medic 1 6 1 12 12 general -"
                  "unit 3 red goblin 11 2 1 12 12 general -"; "unit 4 red goblin 11 6 1 12 12 general -" ])

    let private openFieldMap =
        map "open-field-fire" "Open-field movement and fire" "Two sections cross exposed ground and trade fire around a shallow ridge."
            OpenFieldMovementFire "Use spacing, sight lines, and movement timing before committing to fire."
            [ "Thirty-two-column engagement"; "Twelve-unit composed roster" ]
            [ "Sparse rough terrain makes exposure and approach choice legible." ]
            (mapText 32 24 [ for row in 4 .. 20 -> $"terrain 17 {row} rough" ] []
                [ "zone 1 deployment blue rectangle 0 0 6 24"; "zone 2 deployment red rectangle 26 0 6 24" ] (unitLines 12))

    let private coverMap =
        map "cover-flank" "Cover-dense assault and flank" "An assault element fixes defenders while a flank moves through broken cover."
            CoverDenseAssaultFlank "Compare the direct covered lane with the longer exposed flank."
            [ "Alternating cover belts"; "Assault and flank objectives" ]
            [ "The open southern lane deliberately trades distance for fewer crossings." ]
            (mapText 32 24
                [ for column in [ 10; 14; 18; 22 ] do for row in 2 .. 16 -> $"terrain {column} {row} blocked" ] []
                [ "zone 1 objective rectangle 25 5 3 3"; "zone 2 objective rectangle 25 18 3 3" ] (unitLines 12))

    let private breachMap =
        map "breach-corridor" "Door breach and interior clear" "A section must open a semantic door, enter, and clear a defended interior."
            DoorBreachInteriorClear "Synchronize the breach with the entry element instead of feeding the doorway."
            [ "Closed door and interior walls"; "Twelve-unit room-clearing roster" ]
            [ "One door is the intentional choke; the exterior has room to stage." ]
            (mapText 32 24 [ "terrain 15 11 rough"; "terrain 16 11 rough" ]
                [ for row in 0 .. 23 do
                      let edgeKind = if row = 12 then "door" else "wall"
                      yield $"edge 17 {row} east {edgeKind} closed" ]
                [ "zone 1 objective rectangle 23 9 5 7" ] (unitLines 12))

    let private supportMap =
        map "support-by-fire" "Support-by-fire and suppression" "A base-of-fire element supports a maneuver section across a contested lane."
            SupportByFireSuppression "Establish support before the maneuver element crosses the lane."
            [ "Separated support and maneuver routes"; "Observable suppression checkpoint" ]
            [ "The central lane is intentionally exposed to make sequencing visible." ]
            (mapText 32 24 [ for row in 4 .. 21 -> $"terrain 18 {row} rough" ] []
                [ "zone 1 objective rectangle 26 4 3 3"; "zone 2 objective rectangle 26 19 3 3" ] (unitLines 12))

    let private armoredUnits =
        [ "unit 1 blue rifleman 1 0 2 12 12 general -"; "unit 2 blue rifleman 1 4 2 12 12 general -"
          "unit 3 blue rifleman 1 8 2 12 12 general -"; "unit 4 red troll 12 3 3 240 240 general -"
          "unit 5 blue rifleman 1 13 1 12 12 general -"; "unit 6 blue medic 1 17 1 12 12 general -"
          "unit 7 red goblin 29 1 1 12 12 general -"; "unit 8 red goblin 29 5 1 12 12 general -"
          "unit 9 red goblin 29 17 1 12 12 general -"; "unit 10 red orc 25 2 2 35 35 general -"
          "unit 11 red orc 25 18 2 35 35 general -"; "unit 12 blue observation-drone 5 21 1 8 8 general -" ]

    let private armoredMap =
        map "troll-assault" "Armored target and anti-armor response" "A dispersed section identifies and concentrates effects against an armored troll."
            ArmoredAntiArmorResponse "Preserve spacing, identify the armored threat, and concentrate effective fire."
            [ "Large 3×3 armored footprint"; "Mixed twelve-unit roster" ]
            [ "The larger layout deliberately migrates the earlier four-unit fixture." ]
            (mapText 32 24 [ for row in 4 .. 20 -> $"terrain 17 {row} rough" ] []
                [ "zone 1 deployment blue rectangle 0 0 7 24"; "zone 2 deployment red rectangle 24 0 8 24" ] armoredUnits)

    let private withdrawalMap =
        map "objective-crossing" "Withdrawal and reinforcement" "A pressured patrol disengages through two objectives while reinforcements enter."
            MultiObjectiveWithdrawalReinforcement "Trade space deliberately and preserve a route for the reinforcing element."
            [ "Two sequential objectives"; "Withdrawal and reinforcement lanes" ]
            [ "Blocked central cells force a visible choice between two routes." ]
            (mapText 32 24 [ for column in 14 .. 21 -> $"terrain {column} 13 blocked" ] []
                [ "zone 1 objective rectangle 12 5 3 3"; "zone 2 objective rectangle 22 18 3 3" ] (unitLines 12))

    let private mapCatalog = [ teachingMap; openFieldMap; coverMap; breachMap; supportMap; armoredMap; withdrawalMap ]

    let private stressMap =
        map "catalog-stress-80x80" "Catalog stress qualification" "Maximum supported catalog workload."
            OpenFieldMovementFire "Qualify the production import, simulation, and projection route."
            [ "80×80 map"; "200-unit roster" ] [ "Qualification-only; excluded from the curated chooser." ]
            (mapText 40 40 [ for row in 0 .. 39 -> $"terrain 20 {row} rough" ] []
                [ "zone 1 deployment blue rectangle 0 0 20 40"; "zone 2 deployment red rectangle 20 0 20 40" ] stressUnitLines)

    let private replayCatalog =
        [ replay "quick-contact-run" "Quick contact run" "Reach first contact and inspect its ordered events." teachingMap.Id 8
          replay "open-field-run" "Open-field crossing" "Follow movement and fire across the ridge." openFieldMap.Id 20
          replay "cover-flank-run" "Covered flank" "Follow the fixing and flanking elements." coverMap.Id 22
          replay "breach-stalemate" "Door breach" "Inspect the closed breach and entry sequence." breachMap.Id 16
          replay "support-by-fire-run" "Support established" "Inspect support before maneuver." supportMap.Id 20
          replay "troll-contact" "Armored response" "Follow the composed anti-armor response." armoredMap.Id 20
          replay "withdrawal-run" "Fighting withdrawal" "Inspect withdrawal through reinforcement." withdrawalMap.Id 24 ]

    let private forcesFor (sample: ExperienceMapSample) =
        sample.MapText.Split('\n')
        |> Array.choose (fun line ->
            let parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            if parts.Length > 2 && parts[0] = "unit" then
                Some { UnitId = int32 parts[1]; Capability = parts[3]; Loadout = "standard-" + parts[3]
                       InitialFacing = "north"; InitialAttention = "forward"; InitialKnowledge = "own-side-and-objectives" }
            else None)
        |> Array.toList

    let private basePackage (index: int32) (sample: ExperienceMapSample) (replay: ExperienceReplaySample) : ExperienceScenarioPackage =
        { SchemaVersion = 1; CatalogVersion = content
          Identity = { Engine = engine; Ruleset = ruleset; Content = content; MapRevision = sample.Id + "-r1"; ContentDigest = "" }
          Map = sample; Forces = forcesFor sample
          Plans = [ { Side = "blue"; Name = "primary"; Steps = [ sample.Lesson; "Inspect the event timeline." ] }
                    { Side = "red"; Name = "opposition"; Steps = [ "Contest the primary objective." ] } ]
          Objectives = [ { Id = "primary"; Summary = sample.Lesson; ZoneId = Some 1 } ]
          InitialKnowledge = [ "own-side"; "known-objectives"; "fog-preserves-opposition" ]
          Seed = 184000UL + uint64 index; RandomAddress = "scenario/" + sample.Id + "/v1"
          ExpectedCheckpoints =
            [ { Tick = 0; MinimumEvents = 0; VisibleOutcome = "deployment" }
              { Tick = replay.Ticks; MinimumEvents = 1; VisibleOutcome = "tactical-outcome" } ]
          Replay = replay }

    let private field (value: string) = string value.Length + ":" + value
    let canonical (package: ExperienceScenarioPackage) =
        let addList (values: string list) = values |> List.map field |> String.concat ""
        [ string package.SchemaVersion; package.CatalogVersion; package.Identity.Engine; package.Identity.Ruleset
          package.Identity.Content; package.Identity.MapRevision; familyName package.Map.Family; package.Map.Id
          package.Map.Title; package.Map.Summary; package.Map.Lesson; package.Map.MapText
          addList package.Map.Highlights; addList package.Map.DesignNotes
          package.Forces |> List.map (fun value -> $"{value.UnitId}|{value.Capability}|{value.Loadout}|{value.InitialFacing}|{value.InitialAttention}|{value.InitialKnowledge}") |> addList
          package.Plans |> List.map (fun value -> value.Side + "|" + value.Name + "|" + String.concat ">" value.Steps) |> addList
          package.Objectives |> List.map (fun value -> value.Id + "|" + value.Summary + "|" + (value.ZoneId |> Option.map string |> Option.defaultValue "-")) |> addList
          addList package.InitialKnowledge; string package.Seed; package.RandomAddress
          package.ExpectedCheckpoints |> List.map (fun value -> $"{value.Tick}|{value.MinimumEvents}|{value.VisibleOutcome}") |> addList
          package.Replay.Id; package.Replay.MapSampleId; string package.Replay.Ticks ]
        |> List.map field |> String.concat ""

    let private hex (bytes: byte array) =
        let alphabet = "0123456789abcdef"
        bytes |> Array.collect (fun value -> [| alphabet[int value >>> 4]; alphabet[int value &&& 15] |]) |> String

    let digest (package: ExperienceScenarioPackage) = canonical package |> Encoding.UTF8.GetBytes |> CanonicalHash.sha256 |> hex

    let packages =
        List.map3 (fun (index: int32) (sample: ExperienceMapSample) (replay: ExperienceReplaySample) ->
            let value = basePackage index sample replay
            { value with Identity = { value.Identity with ContentDigest = digest value } })
            [ 1 .. mapCatalog.Length ] mapCatalog replayCatalog

    let stressPackage () =
        let stressReplay = replay "catalog-stress-run" "Catalog stress run" "Exercise the supported maximum." stressMap.Id 8
        let value = basePackage 999 stressMap stressReplay
        { value with Identity = { value.Identity with ContentDigest = digest value } }

    let maps = packages |> List.map _.Map
    let replays = packages |> List.map _.Replay

    let validate (package: ExperienceScenarioPackage) =
        let errors = ResizeArray<ScenarioValidationError>()
        let mapUnitIds =
            package.Map.MapText.Split('\n')
            |> Array.choose (fun line ->
                let parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                if parts.Length > 1 && parts[0] = "unit" then Some(int32 parts[1]) else None)
            |> Array.toList
        if package.SchemaVersion <> 1 then errors.Add(UnsupportedSchema package.SchemaVersion)
        if package.Identity.Engine <> engine then errors.Add(StaleEngine package.Identity.Engine)
        if package.Identity.Ruleset <> ruleset then errors.Add(StaleRuleset package.Identity.Ruleset)
        if package.Identity.Content <> content || package.CatalogVersion <> content then errors.Add(StaleContent package.Identity.Content)
        let expectedRevision = package.Map.Id + "-r1"
        let authoritativeMap = mapCatalog @ [ stressMap ] |> List.exists (fun sample -> sample.Id = package.Map.Id)
        if not authoritativeMap || package.Identity.MapRevision <> expectedRevision then
            errors.Add(StaleMapRevision package.Identity.MapRevision)
        if package.Replay.MapSampleId <> package.Map.Id then errors.Add(StaleReplayBinding package.Replay.MapSampleId)
        if List.isEmpty package.Forces || List.isEmpty package.Plans || List.isEmpty package.Objectives || String.IsNullOrWhiteSpace package.Map.MapText then
            errors.Add(MissingScenarioContent package.Map.Id)
        if (package.Forces |> List.map _.UnitId |> List.sort) <> List.sort mapUnitIds then
            errors.Add(MissingScenarioContent(package.Map.Id + ":force-map-mismatch"))
        if package.ExpectedCheckpoints <> (package.ExpectedCheckpoints |> List.sortBy _.Tick)
           || package.ExpectedCheckpoints |> List.exists (fun checkpoint -> checkpoint.Tick < 0 || checkpoint.Tick > package.Replay.Ticks) then
            errors.Add(MissingScenarioContent(package.Map.Id + ":checkpoint-order"))
        if package.Identity.ContentDigest <> digest { package with Identity = { package.Identity with ContentDigest = "" } } then
            errors.Add(StaleContentDigest package.Identity.ContentDigest)
        if errors.Count = 0 then Ok package else Error(List.ofSeq errors)

    let encodePackage package =
        [ string package.SchemaVersion; package.Identity.Engine; package.Identity.Ruleset; package.Identity.Content
          package.Identity.MapRevision; package.Map.Id; package.Replay.MapSampleId; package.Identity.ContentDigest
          canonical package ]
        |> List.map field
        |> String.concat ""
    let importPackage (serialized: string) =
        let rec readFields (offset: int) (values: string list) : Result<string list, ScenarioValidationError list> =
            if offset = serialized.Length then Ok(List.rev values)
            else
                let separator = serialized.IndexOf(':', offset)
                match separator < 0, Int32.TryParse(serialized.Substring(offset, max 0 (separator - offset))) with
                | false, (true, length) when length >= 0 && separator + 1 + length <= serialized.Length ->
                    readFields (separator + 1 + length) (serialized.Substring(separator + 1, length) :: values)
                | _ -> Error [ MalformedScenarioPackage "invalid length-prefixed package envelope" ]
        match readFields 0 [] with
        | Error errors -> Error errors
        | Ok [ schema; serializedEngine; serializedRuleset; serializedContent; revision; mapId; replayMapId; serializedDigest; payload ] ->
            match Int32.TryParse schema, (packages @ [ stressPackage () ] |> List.tryFind (fun package -> package.Map.Id = mapId)) with
            | (true, schemaVersion), Some package ->
                let errors = ResizeArray<ScenarioValidationError>()
                if schemaVersion <> package.SchemaVersion then errors.Add(UnsupportedSchema schemaVersion)
                if serializedEngine <> package.Identity.Engine then errors.Add(StaleEngine serializedEngine)
                if serializedRuleset <> package.Identity.Ruleset then errors.Add(StaleRuleset serializedRuleset)
                if serializedContent <> package.Identity.Content then errors.Add(StaleContent serializedContent)
                if revision <> package.Identity.MapRevision then errors.Add(StaleMapRevision revision)
                if replayMapId <> package.Replay.MapSampleId then errors.Add(StaleReplayBinding replayMapId)
                if serializedDigest <> package.Identity.ContentDigest || payload <> canonical package then errors.Add(StaleContentDigest serializedDigest)
                if errors.Count = 0 then validate package else Error(List.ofSeq errors)
            | (true, schemaVersion), None -> Error [ StaleMapRevision(mapId + ":unknown") ]
            | _ -> Error [ MalformedScenarioPackage "schema is not an integer" ]
        | Ok _ -> Error [ MalformedScenarioPackage "package envelope field count changed" ]
    let tryPackage id = packages |> List.tryFind (fun package -> package.Map.Id = id || package.Replay.Id = id)
    let catalogFingerprint () = packages |> List.map _.Identity.ContentDigest |> String.concat "|"
    let catalogCost (values: ExperienceScenarioPackage list) =
        let count prefix (package: ExperienceScenarioPackage) = package.Map.MapText.Split('\n') |> Array.filter (_.StartsWith(prefix, StringComparison.Ordinal)) |> Array.length
        { ScenarioCount = int32 values.Length; UnitCount = values |> List.sumBy (count "unit ") |> int32
          TerrainCount = values |> List.sumBy (count "terrain ") |> int32; EdgeCount = values |> List.sumBy (count "edge ") |> int32
          ZoneCount = values |> List.sumBy (count "zone ") |> int32; CheckpointCount = values |> List.sumBy (fun value -> value.ExpectedCheckpoints.Length) |> int32
          ReplayTickCount = values |> List.sumBy (fun value -> int value.Replay.Ticks) |> int32
          CanonicalBytes = values |> List.sumBy (canonical >> Encoding.UTF8.GetBytes >> Array.length) |> int32 }

    let tryMap id =
        (maps @ [ stressMap ]) |> List.tryFind (fun sample -> sample.Id = id)

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

    let runtimeFingerprint package =
        let frames = replayFrames package.Replay
        let frameText =
            frames
            |> Array.map (fun frame ->
                let optionValue = Option.map string >> Option.defaultValue "-"
                let events = frame.Events |> List.map (fun event -> $"{event.Tick}|{event.Source}|{event.Summary}|{optionValue event.SourceUnitId}|{optionValue event.TargetUnitId}") |> String.concat ">"
                let checkpoints = frame.Checkpoints |> List.map (fun checkpoint -> $"{checkpoint.Tick}|{checkpoint.StateHash}|{checkpoint.EventHash}") |> String.concat ">"
                $"{frame.Tick}:{frame.Units.Length}:{events}:{checkpoints}")
            |> String.concat "\n"
        package.Identity.ContentDigest + "|" + (frameText |> Encoding.UTF8.GetBytes |> CanonicalHash.sha256 |> hex)
