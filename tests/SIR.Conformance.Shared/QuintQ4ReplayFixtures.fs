namespace SIR.Conformance

open System
open System.IO
open System.Text.Json
open FS.GG.Game.Core
open SIR.Domain
open SIR.Simulation

type private Q4CombatState =
    { Health: int32
      Suppression: int32
      CoverIntegrity: int32
      CoverBlocking: bool
      Incapacitated: bool }

type private Q4Observation =
    { LastAction: string
      Damage: int32
      PreparationRaw: int32
      TraceRaw: int32
      RetentionRaw: int32
      Wound: string
      Contact: bool
      SuppressionDelta: int32
      CoverDamage: int32
      Destroyed: bool
      StopsProjectile: bool
      ExplanationOrder: string list
      EventId: string
      AttackerFaction: string
      TargetFaction: string }

type private Q4ReplayState =
    { Combat: Q4CombatState
      Last: Q4Observation }

type QuintQ4ReplayMutation =
    | WrongActionMapping
    | WrongObservableField
    | CombatBoundaryDefect

[<RequireQualifiedAccess>]
module QuintQ4ReplayFixtures =
    let private adapterSource = "tests/SIR.Conformance.Shared/QuintQ4ReplayFixtures.fs:applyModelAction"
    let private implementationSource = "src/SIR.Simulation/CombatRules.fs:CombatRules"

    let private required = function
        | Ok value -> value
        | Error error -> failwithf "Q4 replay adapter input was invalid: %A" error

    let private cell col row: Cell = { Col = col; Row = row }

    let private initial =
        { Combat =
            { Health = 100
              Suppression = 0
              CoverIntegrity = 100
              CoverBlocking = true
              Incapacitated = false }
          Last =
            { LastAction = "Initialize"
              Damage = 0
              PreparationRaw = 0
              TraceRaw = 0
              RetentionRaw = 0
              Wound = "NoWound"
              Contact = false
              SuppressionDelta = 0
              CoverDamage = 0
              Destroyed = false
              StopsProjectile = false
              ExplanationOrder = []
              EventId = "initialize"
              AttackerFaction = ""
              TargetFaction = "" } }

    let private explanationChildren application =
        let rec preorder item =
            RuleId.value item.RuleId :: (item.Children |> List.collect preorder)

        application.Children |> List.collect preorder

    let private explanationTree application =
        let rec preorder item =
            RuleId.value item.RuleId :: (item.Children |> List.collect preorder)

        preorder application

    let private attackInput visible eventId =
        { Attacker = cell 0 0
          TargetFootprint = [ cell 1 0 ]
          VisibleSamples = visible
          TotalSamples = 10
          RangeCells = 3
          Suppression = FixedPoint.zero
          BaseDamage = FixedPoint.fromRatio 25 1 |> required
          ArmorRetention = FixedPoint.fromRatio 4 5 |> required
          EventId = eventId }

    let private applyConsequencesWith resolveConsequences visible eventId current =
        let outcome =
            attackInput visible eventId
            |> resolveConsequences current.Combat.Health current.Combat.Suppression 12
            |> required

        let attackerFaction, targetFaction = "Blue", "Red"

        { Combat =
            { current.Combat with
                Health = outcome.RemainingHealth
                Suppression = outcome.TotalSuppression
                Incapacitated = outcome.Incapacitated }
          Last =
            { LastAction = "ResolveConsequences"
              Damage = outcome.Damage
              PreparationRaw = FixedPoint.raw (attackInput visible eventId |> CombatRules.resolveAttack |> required).Preparation
              TraceRaw = FixedPoint.raw (attackInput visible eventId |> CombatRules.resolveAttack |> required).TraceProbability
              RetentionRaw = FixedPoint.raw (attackInput visible eventId |> CombatRules.resolveAttack |> required).ArmorRetention
              Wound =
                match outcome.WoundSeverityCode with
                | Some 1 -> "MajorWound"
                | Some 0 -> "MinorWound"
                | _ -> "NoWound"
              Contact = outcome.Damage > 0
              SuppressionDelta = outcome.SuppressionDelta
              CoverDamage = 0
              Destroyed = false
              StopsProjectile = false
              ExplanationOrder = explanationChildren outcome.Explanation
              EventId = eventId
              AttackerFaction = attackerFaction
              TargetFaction = targetFaction } }

    let private applyConsequences visible eventId current =
        applyConsequencesWith CombatRules.resolveConsequences visible eventId current

    let private applyConsequencesWithBoundaryDefect visible eventId current =
        let defectResolver health suppression delta input =
            CombatRules.resolveConsequences health suppression delta input
            |> Result.map (fun outcome -> { outcome with RemainingHealth = -1 })

        applyConsequencesWith defectResolver visible eventId current

    let private applyCover current =
        let outcome = CombatRules.resolveCoverImpact 25 current.Combat.CoverIntegrity true true "cover:sample"

        { Combat =
            { current.Combat with
                CoverIntegrity = outcome.RemainingIntegrity
                CoverBlocking = if outcome.Destroyed then false else current.Combat.CoverBlocking }
          Last =
            { LastAction = "ResolveCoverImpact"
              Damage = 0
              PreparationRaw = 0
              TraceRaw = 0
              RetentionRaw = 0
              Wound = "NoWound"
              Contact = false
              SuppressionDelta = 0
              CoverDamage = outcome.Damage
              Destroyed = outcome.Destroyed
              StopsProjectile = outcome.StopsProjectile
              ExplanationOrder = explanationTree outcome.Explanation
              EventId = "cover:sample"
              AttackerFaction = ""
              TargetFaction = "" } }

    let private applyRecovery current =
        let remaining, explanation = CombatRules.resolveRecovery current.Combat.Suppression "sample"

        { Combat = { current.Combat with Suppression = remaining }
          Last =
            { LastAction = "ResolveRecovery"
              Damage = 0
              PreparationRaw = 0
              TraceRaw = 0
              RetentionRaw = 0
              Wound = "NoWound"
              Contact = false
              SuppressionDelta = remaining - current.Combat.Suppression
              CoverDamage = 0
              Destroyed = false
              StopsProjectile = false
              ExplanationOrder = explanation |> Option.map explanationTree |> Option.defaultValue []
              EventId = "recovery:sample"
              AttackerFaction = ""
              TargetFaction = "" } }

    let private bigint (element: JsonElement) =
        element.GetProperty("#bigint").GetString()
        |> Option.ofObj
        |> Option.defaultWith (fun () -> failwith "Q4 ITF contained a null #bigint value.")
        |> Int32.Parse

    let private strings (element: JsonElement) =
        element.EnumerateArray()
        |> Seq.map (fun item -> item.GetString() |> Option.ofObj |> Option.defaultValue "")
        |> Seq.toList

    let private expectedState (element: JsonElement) =
        let combat = element.GetProperty("combat")
        let last = element.GetProperty("last")

        { Combat =
            { Health = bigint (combat.GetProperty("health"))
              Suppression = bigint (combat.GetProperty("suppression"))
              CoverIntegrity = bigint (combat.GetProperty("coverIntegrity"))
              CoverBlocking = combat.GetProperty("coverBlocking").GetBoolean()
              Incapacitated = combat.GetProperty("incapacitated").GetBoolean() }
          Last =
            { LastAction = last.GetProperty("lastAction").GetString() |> Option.ofObj |> Option.defaultValue ""
              Damage = bigint (last.GetProperty("damage"))
              PreparationRaw = bigint (last.GetProperty("preparationRaw"))
              TraceRaw = bigint (last.GetProperty("traceRaw"))
              RetentionRaw = bigint (last.GetProperty("retentionRaw"))
              Wound = last.GetProperty("wound").GetProperty("tag").GetString() |> Option.ofObj |> Option.defaultValue ""
              Contact = last.GetProperty("contact").GetBoolean()
              SuppressionDelta = bigint (last.GetProperty("suppressionDelta"))
              CoverDamage = bigint (last.GetProperty("coverDamage"))
              Destroyed = last.GetProperty("destroyed").GetBoolean()
              StopsProjectile = last.GetProperty("stopsProjectile").GetBoolean()
              ExplanationOrder = strings (last.GetProperty("explanationOrder"))
              EventId = last.GetProperty("eventId").GetString() |> Option.ofObj |> Option.defaultValue ""
              AttackerFaction = last.GetProperty("attackerFaction").GetString() |> Option.ofObj |> Option.defaultValue ""
              TargetFaction = last.GetProperty("targetFaction").GetString() |> Option.ofObj |> Option.defaultValue "" } }

    let private firstDifference expected actual =
        [ "combat.health", string expected.Combat.Health, string actual.Combat.Health
          "combat.suppression", string expected.Combat.Suppression, string actual.Combat.Suppression
          "combat.coverIntegrity", string expected.Combat.CoverIntegrity, string actual.Combat.CoverIntegrity
          "combat.coverBlocking", string expected.Combat.CoverBlocking, string actual.Combat.CoverBlocking
          "combat.incapacitated", string expected.Combat.Incapacitated, string actual.Combat.Incapacitated
          "last.lastAction", expected.Last.LastAction, actual.Last.LastAction
          "last.damage", string expected.Last.Damage, string actual.Last.Damage
          "last.preparationRaw", string expected.Last.PreparationRaw, string actual.Last.PreparationRaw
          "last.traceRaw", string expected.Last.TraceRaw, string actual.Last.TraceRaw
          "last.retentionRaw", string expected.Last.RetentionRaw, string actual.Last.RetentionRaw
          "last.wound", expected.Last.Wound, actual.Last.Wound
          "last.contact", string expected.Last.Contact, string actual.Last.Contact
          "last.suppressionDelta", string expected.Last.SuppressionDelta, string actual.Last.SuppressionDelta
          "last.coverDamage", string expected.Last.CoverDamage, string actual.Last.CoverDamage
          "last.destroyed", string expected.Last.Destroyed, string actual.Last.Destroyed
          "last.stopsProjectile", string expected.Last.StopsProjectile, string actual.Last.StopsProjectile
          "last.explanationOrder", String.concat "," expected.Last.ExplanationOrder, String.concat "," actual.Last.ExplanationOrder
          "last.eventId", expected.Last.EventId, actual.Last.EventId
          "last.attackerFaction", expected.Last.AttackerFaction, actual.Last.AttackerFaction
          "last.targetFaction", expected.Last.TargetFaction, actual.Last.TargetFaction ]
        |> List.tryFind (fun (_, expectedValue, actualValue) -> expectedValue <> actualValue)

    let private applyModelAction mutation expected current =
        let actual =
            match expected.Last.LastAction, expected.Last.EventId, mutation with
            | "Initialize", "initialize", _ -> initial
            | "ResolveConsequences", "attack:representative", Some WrongActionMapping ->
                applyConsequences 0 "attack:representative" current
            | "ResolveConsequences", "attack:representative", Some CombatBoundaryDefect ->
                applyConsequencesWithBoundaryDefect 10 "attack:representative" current
            | "ResolveConsequences", "attack:representative", _ -> applyConsequences 10 "attack:representative" current
            | "ResolveConsequences", "attack:miss", _ -> applyConsequences 0 "attack:miss" current
            | "ResolveCoverImpact", "cover:sample", _ -> applyCover current
            | "ResolveRecovery", "recovery:sample", _ -> applyRecovery current
            | action, eventId, _ -> failwithf "Unknown Q4 model action/event: %s/%s" action eventId

        match mutation with
        | Some WrongObservableField -> { actual with Last = { actual.Last with TraceRaw = actual.Last.TraceRaw + 1 } }
        | _ -> actual

    let private replayDirectoryWith mutation directory expectedTraceCount =
        let paths = Directory.GetFiles(directory, "trace_*.itf.json") |> Array.sort

        if paths.Length <> expectedTraceCount then
            failwithf "Q4 ITF trace count mismatch: expected=%d actual=%d" expectedTraceCount paths.Length

        let mutable totalStates = 0

        for path in paths do
            use document = JsonDocument.Parse(File.ReadAllBytes path)
            let root = document.RootElement
            let variables =
                root.GetProperty("vars").EnumerateArray()
                |> Seq.map (fun item -> item.GetString() |> Option.ofObj |> Option.defaultValue "")
                |> Seq.toArray

            if variables <> [| "combat"; "last" |] then
                failwithf "Q4 ITF variable projection drifted: %s" path

            let states = root.GetProperty("states").EnumerateArray() |> Seq.toArray
            let mutable current = initial
            totalStates <- totalStates + states.Length

            for index in 0 .. states.Length - 1 do
                let expected = expectedState states[index]
                current <- applyModelAction mutation expected current

                match firstDifference expected current with
                | None -> ()
                | Some(field, expectedValue, actualValue) ->
                    failwithf
                        "Q4 first divergence: fixture=%s pointer=/states/%d/%s transition=%d action=%s expected=%s actual=%s adapter=%s implementation=%s"
                        path index field index expected.Last.LastAction expectedValue actualValue adapterSource implementationSource

        paths.Length, totalStates

    let replayDirectory directory expectedTraceCount =
        replayDirectoryWith None directory expectedTraceCount

    let tryParseMutation = function
        | "wrong-action-mapping" -> Some WrongActionMapping
        | "wrong-observable-field" -> Some WrongObservableField
        | "combat-boundary-defect" -> Some CombatBoundaryDefect
        | _ -> None

    let replayMutation directory expectedTraceCount mutation =
        replayDirectoryWith (Some mutation) directory expectedTraceCount
