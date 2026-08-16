namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

type CombatAttackInput = { Attacker: Cell; TargetFootprint: Cell list; VisibleSamples: int32; TotalSamples: int32; RangeCells: int32; Suppression: FixedPoint; BaseDamage: FixedPoint; ArmorRetention: FixedPoint; EventId: string }
type CombatAttackResult = { Preparation: FixedPoint; TraceProbability: FixedPoint; ArmorRetention: FixedPoint; ExpectedDamage: int32; Explanation: RuleApplication }
type CombatConsequences = { Damage: int32; RemainingHealth: int32; WoundSeverityCode: int32 option; Incapacitated: bool; SuppressionDelta: int32; TotalSuppression: int32; Explanation: RuleApplication }
type CombatCoverImpact = { Damage: int32; RemainingIntegrity: int32; Destroyed: bool; StopsProjectile: bool; Explanation: RuleApplication }
type RuleReplayBinding = { BoundEngineIdentity: string; BoundCompatibilityProfile: string; BoundPackageVersion: string; BoundSourceCommit: string; BoundImplementationDigest: byte array; BoundSemanticDigest: byte array; BoundManifestDigest: byte array; BoundExplanation: RuleApplication }
type RetainedRulePackage = { Identity: RulePackageIdentity; ManifestJson: string; CoverageJson: string }
type HistoricalRuleResolution = ResolvedHistoricalRulePackage of RetainedRulePackage | HistoricalRulePackageUnavailable of manifestDigest: byte array

[<RequireQualifiedAccess>]
module CombatRules =
    let private requiredId value = RuleId.create value |> Result.defaultWith failwith
    let private fixedValue unitName value = { DataKind = RuleValueKind.FixedPoint; Unit = unitName; Value = RuleValue.FixedPointValue value }
    let private integerValue unitName value = { DataKind = RuleValueKind.Integer; Unit = unitName; Value = RuleValue.IntegerValue value }
    let private source symbol = Some { Symbol = symbol; RepositoryPath = "src/SIR.Simulation/CombatRules.fs"; Commit = "e055929f1f958ad9f98f5aac9934ea28cd7a0fbe" }
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
        { Metadata = metadata "COMBAT-ATTACK-RESOLUTION-001" "Resolve one explained attack" Transition "The attack transition exposes its ordered rule calls and authoritative event." [ "COMBAT-ENGAGEMENT-001"; "COMBAT-COLLISION-001"; "COMBAT-COVER-003"; "COMBAT-PENETRATION-001"; "COMBAT-DAMAGE-001"; "COMBAT-WOUND-001"; "COMBAT-SUPPRESSION-001"; "COMBAT-COLLATERAL-001" ] "CombatRules.resolveAttack" "rules-corpus-v2"
          Semantics = TransitionSemantics { Phase = "AttackPhase"; Preconditions = []; Reads = [ "attacker.cell"; "target.footprint"; "weapon"; "cover"; "armor"; "target.health"; "target.suppression" ]; Effects = [ "cover.integrity"; "target.health"; "target.wounds"; "target.incapacitated"; "target.suppression" ]; Events = [ "AttackResolved"; "CoverDestroyed" ] } }

    let private transitionRule id title rationale dependencies symbol reads effects events =
        { Metadata = metadata id title Transition rationale dependencies symbol "rules-corpus-v2"
          Semantics = TransitionSemantics { Phase = "AttackPhase"; Preconditions = []; Reads = reads; Effects = effects; Events = events } }
    let private collision = transitionRule "COMBAT-COLLISION-001" "Resolve first collision" "A direct projectile commits consequences only at the first semantically reachable collision." [ "COMBAT-TRACE-002" ] "CombatRules.resolveConsequences" [ "trace.outcome"; "trace.crossings" ] [ "projectile.contact" ] [ "ContactResolved" ]
    let private cover = transitionRule "COMBAT-COVER-003" "Resolve cover retention" "Cover changes retained effect and blocking state deterministically." [ "COMBAT-COLLISION-001" ] "CombatRules.resolveCoverImpact" [ "cover.integrity"; "cover.projectileBlocking" ] [ "cover.integrity" ] [ "CoverDamaged" ]
    let private penetration = transitionRule "COMBAT-PENETRATION-001" "Resolve armor penetration" "Directional effective armor determines the bounded retained damage ratio." [ "COMBAT-COVER-003"; "COMBAT-ARMOR-004" ] "CombatRules.resolveConsequences" [ "armor.rating"; "weapon.penetration" ] [ "damage.retention" ] [ "ArmorResolved" ]
    let private health = transitionRule "COMBAT-HEALTH-001" "Commit hit-point damage" "Only the executable damage outcome may reduce hit points." [ "COMBAT-DAMAGE-001" ] "CombatRules.resolveConsequences" [ "target.health" ] [ "target.health" ] [ "HealthChanged" ]
    let private wound = transitionRule "COMBAT-WOUND-001" "Resolve wound and incapacity" "Committed damage deterministically creates wounds and zero health incapacitates." [ "COMBAT-HEALTH-001" ] "CombatRules.resolveConsequences" [ "target.health"; "damage" ] [ "target.wounds"; "target.incapacitated" ] [ "WoundApplied"; "Incapacitated" ]
    let private suppression = transitionRule "COMBAT-SUPPRESSION-001" "Commit suppression" "Eligible impacted recipients receive bounded suppression." [ "COMBAT-COLLISION-001" ] "CombatRules.resolveConsequences" [ "target.suppression"; "weapon.suppression" ] [ "target.suppression" ] [ "SuppressionChanged" ]
    let private recovery = transitionRule "COMBAT-SUPPRESSION-RECOVERY-001" "Recover suppression" "Recovery removes at most five suppression points in a distinct transition." [ "COMBAT-SUPPRESSION-001" ] "CombatRules.resolveRecovery" [ "target.suppression" ] [ "target.suppression" ] [ "SuppressionChanged" ]
    let private collateral = transitionRule "COMBAT-COLLATERAL-001" "Resolve collateral recipients" "Area and collision rules apply identically across faction affiliation." [ "COMBAT-COLLISION-001" ] "CombatRules.resolveConsequences" [ "target.faction"; "attacker.faction" ] [ "target.health"; "target.suppression" ] [ "AttackResolved" ]
    let private coverDestruction = transitionRule "COMBAT-COVER-DESTRUCTION-001" "Destroy depleted cover" "Depleted cover stops blocking subsequent projectile queries." [ "COMBAT-COVER-003" ] "CombatRules.resolveCoverImpact" [ "cover.integrity" ] [ "cover.projectileBlocking" ] [ "CoverDestroyed" ]

    let registry =
#if SIR_WEB_CLIENT
        [ weapon; body; engagement; trace; armor; damage; collision; cover; penetration; health; wound; suppression; recovery; collateral; coverDestruction; transition ] |> List.sortBy (fun rule -> RuleId.value rule.Metadata.Id)
#else
        [ weapon; body; engagement; trace; armor; damage; collision; cover; penetration; health; wound; suppression; recovery; collateral; coverDestruction; transition ] |> Rules.validate |> Result.defaultWith (fun errors -> failwithf "Invalid combat registry: %A" errors)
#endif
    let implementationArtifacts =
        [ "implementation", System.Text.Encoding.UTF8.GetBytes "dd0880eb09a4f954ec66686a328997bbf1041d01b87abffcd7041ae80e5021c4" ]
    let packageIdentity = Rules.packageIdentity "sir-simulation-v1" "fs-gg-game-core-fable-lockstep-v1" "FS.GG.Game.Core@0.13.0" "e055929f1f958ad9f98f5aac9934ea28cd7a0fbe" implementationArtifacts registry
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
    let private traceProbability visible total =
        FixedPoint.fromRatio (int visible) (int total)
        |> Result.defaultWith (fun _ -> failwith "Authoritative non-empty sample division failed.")
    let resolveAttack input =
        if List.isEmpty input.TargetFootprint || input.TotalSamples <= 0 || input.VisibleSamples < 0 || input.VisibleSamples > input.TotalSamples then Error "Target footprint and authoritative spatial sample counts must be valid." else
        let visible, total = int input.VisibleSamples, int input.TotalSamples
        let traceValue = traceProbability input.VisibleSamples input.TotalSamples
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

    let private bounded100 value = max 0 value |> min 100
    let resolveConsequences currentHealth currentSuppression suppressionDelta input =
        resolveAttack input
        |> Result.map (fun attack ->
            let damage = attack.ExpectedDamage
            let health = bounded100 (currentHealth - damage)
            let appliedSuppression = if damage > 0 then max 0 suppressionDelta else 0
            let totalSuppression = bounded100 (currentSuppression + appliedSuppression)
            let woundSeverity = if damage >= 50 then Some 1 elif damage >= 25 then Some 0 else None
            let children =
                [ application "COMBAT-COLLISION-001" input.EventId [ "visibleSamples", integerValue "samples" input.VisibleSamples; "totalSamples", integerValue "samples" input.TotalSamples ] (integerValue "contact" (if damage > 0 then 1 else 0)) [] ]
                @ attack.Explanation.Children
                @ [ application "COMBAT-COVER-003" input.EventId [ "retention", fixedValue "ratio" attack.ArmorRetention ] (fixedValue "ratio" attack.ArmorRetention) []
                    application "COMBAT-PENETRATION-001" input.EventId [ "retention", fixedValue "ratio" attack.ArmorRetention ] (fixedValue "ratio" attack.ArmorRetention) []
                    application "COMBAT-HEALTH-001" input.EventId [ "currentHealth", integerValue "health" currentHealth; "damage", integerValue "damage" damage ] (integerValue "health" health) []
                    application "COMBAT-WOUND-001" input.EventId [ "damage", integerValue "damage" damage; "remainingHealth", integerValue "health" health ] (integerValue "severity" (Option.defaultValue -1 woundSeverity)) []
                    application "COMBAT-SUPPRESSION-001" input.EventId [ "currentSuppression", integerValue "suppression" currentSuppression; "delta", integerValue "suppression" appliedSuppression ] (integerValue "suppression" totalSuppression) []
                    application "COMBAT-COLLATERAL-001" input.EventId [] (integerValue "damage" damage) [] ]
            let explanation = { attack.Explanation with Children = children; Outcome = integerValue "damage" damage }
            { Damage = damage; RemainingHealth = health; WoundSeverityCode = woundSeverity; Incapacitated = health = 0; SuppressionDelta = appliedSuppression; TotalSuppression = totalSuppression; Explanation = explanation })

    let resolveCoverImpact baseDamage currentIntegrity projectileBlocking directAttack eventId =
        let damage = max 1 (baseDamage / 2)
        let remaining = bounded100 (currentIntegrity - damage)
        let destroyed = remaining = 0
        // A projectile that destroys its first blocking cover is still consumed by that
        // collision.  The opened permeability is observable by the next attack.
        let stops = directAttack && projectileBlocking
        let coverApplication = application "COMBAT-COVER-003" eventId [ "baseDamage", integerValue "damage" baseDamage; "currentIntegrity", integerValue "integrity" currentIntegrity ] (integerValue "integrity" remaining) []
        let destructionApplication = application "COMBAT-COVER-DESTRUCTION-001" eventId [ "remainingIntegrity", integerValue "integrity" remaining ] (integerValue "destroyed" (if destroyed then 1 else 0)) [ coverApplication ]
        { Damage = damage; RemainingIntegrity = remaining; Destroyed = destroyed; StopsProjectile = stops; Explanation = destructionApplication }

    let resolveRecovery currentSuppression entityId =
        let recovered = min 5 (max 0 currentSuppression)
        let remaining = currentSuppression - recovered
        if recovered = 0 then remaining, None
        else
            let explanation = application "COMBAT-SUPPRESSION-RECOVERY-001" ("recovery:" + entityId) [ "currentSuppression", integerValue "suppression" currentSuppression ] (integerValue "suppression" remaining) []
            remaining, Some explanation
