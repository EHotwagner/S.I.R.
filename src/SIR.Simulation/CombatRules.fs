namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

type CombatAttackInput = { Attacker: Cell; TargetFootprint: Cell list; IsTransparent: Cell -> bool; RangeCells: int32; Suppression: FixedPoint; BaseDamage: FixedPoint; ArmorRetention: FixedPoint; EventId: string }
type CombatAttackResult = { Preparation: FixedPoint; TraceProbability: FixedPoint; ArmorRetention: FixedPoint; ExpectedDamage: int32; Explanation: RuleApplication }
type RuleReplayBinding = { BoundEngineIdentity: string; BoundCompatibilityProfile: string; BoundPackageVersion: string; BoundSourceCommit: string; BoundImplementationDigest: byte array; BoundSemanticDigest: byte array; BoundManifestDigest: byte array; BoundExplanation: RuleApplication }
type RetainedRulePackage = { Identity: RulePackageIdentity; ManifestJson: string; CoverageJson: string }
type HistoricalRuleResolution = ResolvedHistoricalRulePackage of RetainedRulePackage | HistoricalRulePackageUnavailable of manifestDigest: byte array

[<RequireQualifiedAccess>]
module CombatRules =
    let private requiredId value = RuleId.create value |> Result.defaultWith failwith
    let private fixedValue unitName value = { DataKind = RuleValueKind.FixedPoint; Unit = unitName; Value = RuleValue.FixedPointValue value }
    let private integerValue unitName value = { DataKind = RuleValueKind.Integer; Unit = unitName; Value = RuleValue.IntegerValue value }
    let private source symbol = Some { Symbol = symbol; RepositoryPath = "src/SIR.Simulation/CombatRules.fs"; Commit = "0f7128985a14ef3470e92d23ee5786236f97fb97" }
    let private statement trigger response = { Preconditions = []; Trigger = trigger; System = "S.I.R. combat simulation"; Responses = [ response ] }
    let private metadata id title kind rationale dependencies symbol evidence =
        { Id = requiredId id; Title = title; Status = Canonical; SemanticKind = kind; Statement = statement None title; Rationale = rationale; Dependencies = dependencies |> List.map requiredId; Supersedes = []; RuleSource = source symbol; Examples = [ "tests/SIR.Conformance.Shared/RulesCorpusFixtures.fs" ]; Properties = [ "deterministic .NET/Fable canonical equality" ]; Evidence = [ evidence ] }

    let private fp numerator denominator = FixedPoint.fromRatio numerator denominator |> Result.defaultWith (fun _ -> failwith "Invalid combat constant.")
    let private one = fp 1 1
    let private zero = FixedPoint.zero
    let private rangeSlope = fp 1 10

    let private weapon =
        { Metadata = metadata "CONTENT-WEAPON-RIFLE-001" "Representative rifle damage" Fact "The representative rifle anchors the first executable combat slice." [] "CombatRules.weapon" "rules-corpus-v1"
          Semantics = FactSemantics(fixedValue "damage" (fp 25 1)) }
    let private body =
        { Metadata = metadata "CONTENT-BODY-HUMAN-001" "Representative human armor retention" Fact "The representative body retains the full effect when no armor is declared." [] "CombatRules.body" "rules-corpus-v1"
          Semantics = FactSemantics(fixedValue "ratio" one) }
    let private engagement =
        let expression = Add(Constant(fixedValue "seconds" one), Multiply(Input("range", RuleValueKind.FixedPoint, "seconds"), Constant(fixedValue "ratio" rangeSlope)))
        { Metadata = metadata "COMBAT-ENGAGEMENT-001" "Engagement preparation" Formula "Preparation grows deterministically with engagement range." [] "CombatRules.engagement" "rules-corpus-v1"
          Semantics = FormulaSemantics(RuleValueKind.FixedPoint, "seconds", expression) }
    let private trace =
        { Metadata = metadata "COMBAT-TRACE-002" "Exposed-footprint trace probability" Algorithm "Visible target footprint samples produce the explainable trace probability." [] "CombatRules.traceProbability" "fs-gg-game-core-fable-lockstep-v1"
          Semantics = AlgorithmSemantics { ImplementationSymbol = "FS.GG.Game.Core.Los.lineOfSightBy"; Fingerprint = "FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover"; Inputs = [ "visible", RuleValueKind.Integer, "samples"; "total", RuleValueKind.Integer, "samples" ]; ResultKind = RuleValueKind.FixedPoint; ResultUnit = "ratio"; ExplanationFields = [ "visibleSamples"; "totalSamples"; "lineMode" ] } }
    let private armor =
        { Metadata = metadata "COMBAT-ARMOR-004" "Armor retained effect" Formula "Armor retention is an explicit bounded ratio." [] "CombatRules.armor" "rules-corpus-v1"
          Semantics = FormulaSemantics(RuleValueKind.FixedPoint, "ratio", Clamp(Constant(fixedValue "ratio" zero), Constant(fixedValue "ratio" one), Input("retention", RuleValueKind.FixedPoint, "ratio"))) }
    let private damage =
        { Metadata = metadata "COMBAT-DAMAGE-001" "Expected damage" Formula "Expected damage is the weapon effect multiplied once by trace probability and retained armor effect." [ "CONTENT-WEAPON-RIFLE-001"; "COMBAT-TRACE-002"; "COMBAT-ARMOR-004" ] "CombatRules.damage" "rules-corpus-v1"
          Semantics = FormulaSemantics(RuleValueKind.FixedPoint, "damage", Multiply(Multiply(Input("baseDamage", RuleValueKind.FixedPoint, "damage"), Input("trace", RuleValueKind.FixedPoint, "ratio")), Input("retention", RuleValueKind.FixedPoint, "ratio"))) }
    let private transition =
        { Metadata = metadata "COMBAT-ATTACK-RESOLUTION-001" "Resolve one explained attack" Transition "The attack transition exposes its ordered rule calls and authoritative event." [ "COMBAT-ENGAGEMENT-001"; "COMBAT-TRACE-002"; "COMBAT-ARMOR-004"; "COMBAT-DAMAGE-001" ] "CombatRules.resolveAttack" "rules-corpus-v1"
          Semantics = TransitionSemantics { Phase = "AttackPhase"; Preconditions = []; Reads = [ "attacker.cell"; "target.footprint"; "weapon"; "armor" ]; Effects = [ "target.health" ]; Events = [ "AttackResolved" ] } }

    let registry =
#if SIR_WEB_CLIENT
        [ weapon; body; engagement; trace; armor; damage; transition ] |> List.sortBy (fun rule -> RuleId.value rule.Metadata.Id)
#else
        [ weapon; body; engagement; trace; armor; damage; transition ] |> Rules.validate |> Result.defaultWith (fun errors -> failwithf "Invalid combat registry: %A" errors)
#endif
    let implementationArtifacts =
        [ "implementation", System.Text.Encoding.UTF8.GetBytes "7c2d12e48b5516a0327024688e3de66e7078a7996b4f700846319404f527217d" ]
    let packageIdentity = Rules.packageIdentity "sir-simulation-v1" "fs-gg-game-core-fable-lockstep-v1" "FS.GG.Game.Core@0.13.0" "0f7128985a14ef3470e92d23ee5786236f97fb97" implementationArtifacts registry
    let retainedPackage =
#if SIR_WEB_CLIENT
        { Identity = packageIdentity; ManifestJson = ""; CoverageJson = "" }
#else
        { Identity = packageIdentity; ManifestJson = Rules.manifestJson packageIdentity registry; CoverageJson = Rules.coverageJson packageIdentity registry }
#endif

    let replayBinding explanation =
        { BoundEngineIdentity = packageIdentity.EngineIdentity
          BoundCompatibilityProfile = packageIdentity.CompatibilityProfile
          BoundPackageVersion = packageIdentity.PackageVersion
          BoundSourceCommit = packageIdentity.SourceCommit
          BoundImplementationDigest = packageIdentity.ImplementationDigest
          BoundSemanticDigest = packageIdentity.SemanticDigest
          BoundManifestDigest = packageIdentity.ManifestDigest
          BoundExplanation = explanation }

    let resolveHistoricalPackage retained binding =
        retained
        |> List.tryFind (fun package ->
            let identity = package.Identity
            identity.EngineIdentity = binding.BoundEngineIdentity
            && identity.CompatibilityProfile = binding.BoundCompatibilityProfile
            && identity.PackageVersion = binding.BoundPackageVersion
            && identity.SourceCommit = binding.BoundSourceCommit
            && identity.ImplementationDigest = binding.BoundImplementationDigest
            && identity.SemanticDigest = binding.BoundSemanticDigest
            && identity.ManifestDigest = binding.BoundManifestDigest)
        |> function
            | Some package -> ResolvedHistoricalRulePackage package
            | None -> HistoricalRulePackageUnavailable binding.BoundManifestDigest

    let private evaluate expression inputs = Rules.evaluate inputs expression |> Result.mapError (sprintf "%A")
    let private formula id = registry |> List.find (fun rule -> RuleId.value rule.Metadata.Id = id) |> fun rule -> match rule.Semantics with FormulaSemantics(_, _, expression) -> expression | _ -> failwith "Expected formula."
    let private application id eventId operands (outcome: TypedValue) children = { ApplicationId = eventId + ":" + id; RuleId = requiredId id; Operands = operands; Outcome = outcome; Children = children; EventId = eventId; PackageManifestDigest = packageIdentity.ManifestDigest }
    let private traceProbability attacker footprint isTransparent =
        let visible = footprint |> List.filter (Los.lineOfSightBy Supercover isTransparent attacker) |> List.length
        visible, List.length footprint

    let resolveAttack input =
        if List.isEmpty input.TargetFootprint then Error "Target footprint must contain at least one sample." else
        let visible, total = traceProbability input.Attacker input.TargetFootprint input.IsTransparent
        let traceValue = FixedPoint.fromRatio visible total |> Result.defaultWith (fun _ -> failwith "Non-empty footprint division failed.")
        let preparationInputs = Map.ofList [ "range", fixedValue "seconds" (fp input.RangeCells 1) ]
        let armorInputs = Map.ofList [ "retention", fixedValue "ratio" input.ArmorRetention ]
        let fixedOf (typed: TypedValue) = match typed.Value with RuleValue.FixedPointValue value -> value | _ -> failwith "Validated formula returned another kind."
        match evaluate (formula "COMBAT-ENGAGEMENT-001") preparationInputs, evaluate (formula "COMBAT-ARMOR-004") armorInputs with
        | Ok preparation, Ok retained ->
            let damageInputs = Map.ofList [ "baseDamage", fixedValue "damage" input.BaseDamage; "trace", fixedValue "ratio" traceValue; "retention", retained ]
            match evaluate (formula "COMBAT-DAMAGE-001") damageInputs with
            | Error error -> Error error
            | Ok expected ->
                let expectedFixed = fixedOf expected
                let roundedDamage = (FixedPoint.raw expectedFixed + FixedPoint.Scale / 2) / FixedPoint.Scale
                let traceTyped = fixedValue "ratio" traceValue
                let children =
                    [ application "COMBAT-ENGAGEMENT-001" input.EventId [ "rangeCells", integerValue "cells" input.RangeCells; "suppression", fixedValue "suppression" input.Suppression ] preparation []
                      application "COMBAT-TRACE-002" input.EventId [ "visibleSamples", integerValue "samples" visible; "totalSamples", integerValue "samples" total; "lineMode", { DataKind = RuleValueKind.Text; Unit = "name"; Value = RuleValue.TextValue "Supercover" } ] traceTyped []
                      application "COMBAT-ARMOR-004" input.EventId [ "retention", fixedValue "ratio" input.ArmorRetention ] retained []
                      application "COMBAT-DAMAGE-001" input.EventId [ "baseDamage", fixedValue "damage" input.BaseDamage; "trace", traceTyped; "retention", retained ] expected [] ]
                let outcome = integerValue "damage" roundedDamage
                Ok { Preparation = fixedOf preparation; TraceProbability = traceValue; ArmorRetention = fixedOf retained; ExpectedDamage = roundedDamage; Explanation = application "COMBAT-ATTACK-RESOLUTION-001" input.EventId [] outcome children }
        | Error error, _ | _, Error error -> Error error
