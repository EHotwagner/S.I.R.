namespace SIR.Conformance

open FS.GG.Game.Core
open SIR.Domain
open SIR.Simulation

[<RequireQualifiedAccess>]
module RulesCorpusFixtures =
    let private require condition message = if not condition then failwith message
    let private fp numerator denominator = FixedPoint.fromRatio numerator denominator |> Result.defaultWith (fun _ -> failwith "invalid fixture ratio")
    let private hexBytes (value: string) =
        let nibble character =
            if character >= '0' && character <= '9' then int character - int '0'
            else int character - int 'a' + 10
        [| for index in 0 .. 2 .. value.Length - 2 -> byte (nibble value[index] * 16 + nibble value[index + 1]) |]
    let private attack eventId =
        CombatRules.resolveAttack
            { Attacker = { Col = 0; Row = 0 }
              TargetFootprint = [ { Col = 1; Row = 0 }; { Col = 1; Row = 1 } ]
              VisibleSamples = 1
              TotalSamples = 1
              RangeCells = 1
              Suppression = FixedPoint.zero
              BaseDamage = fp 25 1
              ArmorRetention = fp 4 5
              EventId = eventId }
        |> Result.defaultWith failwith

    let private attackWithRetention eventId retention =
        CombatRules.resolveAttack
            { Attacker = { Col = 0; Row = 0 }
              TargetFootprint = [ { Col = 1; Row = 0 }; { Col = 1; Row = 1 } ]
              VisibleSamples = 1
              TotalSamples = 1
              RangeCells = 1
              Suppression = FixedPoint.zero
              BaseDamage = fp 25 1
              ArmorRetention = retention
              EventId = eventId }
        |> Result.defaultWith failwith

    let manifestJson () = CombatRules.retainedPackage.ManifestJson
    let coverageJson () = CombatRules.retainedPackage.CoverageJson
    let representativeApplicationBytes () = (attack "fixture-attack-1").Explanation |> Rules.canonicalApplicationBytes
    let performanceWorkload iterations =
        let mutable checksum = 0
        for index in 1 .. iterations do
            checksum <- checksum + (attack ("performance-" + string index)).ExpectedDamage
        let explanation = (attack "performance-evidence").Explanation
        let rec counts application =
            let childApplications, childOperands =
                application.Children
                |> List.map counts
                |> List.fold (fun (applications, operands) (childApplicationCount, childOperandCount) -> applications + childApplicationCount, operands + childOperandCount) (0, 0)
            1 + childApplications, application.Operands.Length + childOperands
        let applications, operands = counts explanation
        checksum, applications, operands, (Rules.canonicalApplicationBytes explanation).Length, (System.Text.Encoding.UTF8.GetBytes CombatRules.retainedPackage.ManifestJson).Length

    let evaluate injectDivergence =
        require (CombatRules.registry.Length = 7) "The combat registry must cover two facts, three formulas, one algorithm, and one transition."
        require (CombatRules.registry |> List.map (fun rule -> RuleId.value rule.Metadata.Id) |> List.distinct |> List.length = 7) "Rule IDs are not unique."

        let result = attack "fixture-attack-1"
        require (FixedPoint.raw result.TraceProbability = FixedPoint.Scale) "The fully exposed footprint did not produce probability one."
        require (FixedPoint.raw result.ArmorRetention = 8_000) "Armor retention changed."
        require (result.ExpectedDamage = 20) "Expected damage must be 25 × 1.0 × 0.8 = 20."
        require (result.Explanation.Children.Length = 4) "The attack explanation must expose engagement, trace, armor, and damage."
        require (result.Explanation.Children |> List.exists (fun application -> RuleId.value application.RuleId = "COMBAT-TRACE-002")) "The registered trace algorithm is absent from the derivation."

        let overRetained = attackWithRetention "fixture-attack-over-retained" (fp 6 5)
        require (FixedPoint.raw overRetained.ArmorRetention = FixedPoint.Scale) "Armor retention was not clamped to one."
        require (overRetained.ExpectedDamage = 25) "Damage did not consume the clamped armor-rule result."
        let damageApplication = overRetained.Explanation.Children |> List.find (fun application -> RuleId.value application.RuleId = "COMBAT-DAMAGE-001")
        let explainedRetention = damageApplication.Operands |> List.find (fun (name, _) -> name = "retention") |> snd
        match explainedRetention.Value with
        | RuleValue.FixedPointValue value -> require (FixedPoint.raw value = FixedPoint.raw overRetained.ArmorRetention) "The damage operand diverged from authoritative clamped retention."
        | _ -> failwith "The damage retention operand lost its fixed-point kind."

        let binding = CombatRules.replayBinding result.Explanation
        match CombatRules.resolveHistoricalPackage [ CombatRules.retainedPackage ] binding with
        | ResolvedHistoricalRulePackage package -> require (package.Identity.ManifestDigest = binding.BoundManifestDigest) "Historical lookup returned another package."
        | HistoricalRulePackageUnavailable _ -> failwith "The retained historical package was unavailable."
        match CombatRules.resolveHistoricalPackage [] binding with
        | HistoricalRulePackageUnavailable digest -> require (digest = binding.BoundManifestDigest) "Unavailable state lost the recorded package identity."
        | ResolvedHistoricalRulePackage _ -> failwith "Missing historical content was silently substituted."

        let retainedV1Identity =
            { SchemaVersion = 1
              EngineIdentity = "sir-simulation-v1"
              CompatibilityProfile = "fs-gg-game-core-fable-lockstep-v1"
              PackageVersion = "FS.GG.Game.Core@0.13.0"
              SourceCommit = "791ed35fc776eaf7d54ce3ba5dc56f0416853229"
              ImplementationDigest = hexBytes "5b47b1d0ed4ed6207417b32c8a6edc9dad5ffeeb87dc0d1db8c752418805efaf"
              SemanticDigest = hexBytes "0e51bd8761599a98b8fca7db5235e88434d82e4370744c68e59ef896453d516d"
              ManifestDigest = hexBytes "f590b9bfc19766e583c1f2a970c9e0c1ce63ddb64893c502d12a1d9069dd686d" }
        let retainedV1 = { Identity = retainedV1Identity; ManifestJson = "retained:v1:f590b9bf"; CoverageJson = "retained:v1:coverage" }
        let historicalBinding =
            { BoundEngineIdentity = retainedV1Identity.EngineIdentity
              BoundCompatibilityProfile = retainedV1Identity.CompatibilityProfile
              BoundPackageVersion = retainedV1Identity.PackageVersion
              BoundSourceCommit = retainedV1Identity.SourceCommit
              BoundImplementationDigest = retainedV1Identity.ImplementationDigest
              BoundSemanticDigest = retainedV1Identity.SemanticDigest
              BoundManifestDigest = retainedV1Identity.ManifestDigest
              BoundExplanation = result.Explanation }
        match CombatRules.resolveHistoricalPackage [ CombatRules.retainedPackage; retainedV1 ] historicalBinding with
        | ResolvedHistoricalRulePackage package -> require (package.ManifestJson = retainedV1.ManifestJson) "Changed current rules replaced the exact retained v1 package."
        | HistoricalRulePackageUnavailable _ -> failwith "Retained v1 was unavailable after current rules changed."
        match CombatRules.resolveHistoricalPackage [ CombatRules.retainedPackage ] historicalBinding with
        | HistoricalRulePackageUnavailable digest -> require (digest = retainedV1Identity.ManifestDigest) "Unavailable v1 lost its exact digest."
        | ResolvedHistoricalRulePackage _ -> failwith "Current v2 silently reinterpreted retained v1 evidence."

        let changedIdentity =
            Rules.packageIdentity
                CombatRules.packageIdentity.EngineIdentity
                CombatRules.packageIdentity.CompatibilityProfile
                CombatRules.packageIdentity.PackageVersion
                "ffffffffffffffffffffffffffffffffffffffff"
                [ "combat-rules", [| 1uy |] ]
                CombatRules.registry
        let changedPackage =
            { Identity = changedIdentity
              ManifestJson = Rules.manifestJson changedIdentity CombatRules.registry
              CoverageJson = Rules.coverageJson changedIdentity CombatRules.registry }
        match CombatRules.resolveHistoricalPackage [ changedPackage; CombatRules.retainedPackage ] binding with
        | ResolvedHistoricalRulePackage package ->
            require (package.Identity.SourceCommit = binding.BoundSourceCommit) "A changed current package replaced the pinned historical source."
            require (package.ManifestJson = CombatRules.retainedPackage.ManifestJson) "Historical lookup did not return the exact retained manifest."
        | HistoricalRulePackageUnavailable _ -> failwith "The exact old package was not resolved after current artifacts changed."
        match CombatRules.resolveHistoricalPackage [ changedPackage ] binding with
        | HistoricalRulePackageUnavailable digest -> require (digest = binding.BoundManifestDigest) "Changed current content was substituted for an unavailable historical package."
        | ResolvedHistoricalRulePackage _ -> failwith "A changed current package silently reinterpreted historical evidence."

        require (CombatRules.retainedPackage.ManifestJson.Contains "\"COMBAT-DAMAGE-001\"") "Generated manifest omitted the damage rule."
        require (CombatRules.retainedPackage.ManifestJson.Contains "FS.GG.Game.Core.Los.lineOfSightBy") "Generated manifest omitted the registered algorithm symbol."
        require (CombatRules.retainedPackage.CoverageJson.Contains "\"outside\":\"legacy\"") "Coverage did not classify the outside authority boundary."
        for kind in [ "rule"; "implementation"; "event"; "explanation"; "example/property"; "documentation"; "source"; "replay" ] do
            require (CombatRules.retainedPackage.CoverageJson.Contains ("\"kind\":\"" + kind + "\"")) ("Coverage omitted the required " + kind + " node kind.")
        for rule in CombatRules.registry do
            let id = RuleId.value rule.Metadata.Id
            require (CombatRules.retainedPackage.CoverageJson.Contains ("\"from\":\"rule:" + id + "\"")) ("Coverage omitted outgoing reachability for " + id + ".")
        for edgeKind in [ "implementation"; "event/application"; "explanation"; "example"; "property"; "documentation"; "source"; "replay" ] do
            require (CombatRules.retainedPackage.CoverageJson.Contains ("\"kind\":\"" + edgeKind + "\"")) ("Coverage omitted the required " + edgeKind + " edge kind.")
        require ((System.Text.Encoding.UTF8.GetBytes CombatRules.retainedPackage.ManifestJson).Length < 524_288) "Manifest exceeded the 512 KiB budget."
        require ((Rules.canonicalApplicationBytes result.Explanation).Length < 65_536) "Explanation exceeded the 64 KiB budget."

        let duplicate = CombatRules.registry.Head :: CombatRules.registry
        match Rules.validate duplicate with Error errors when errors |> List.exists (function DuplicateRuleId _ -> true | _ -> false) -> () | verdict -> failwithf "Duplicate registration was accepted: %A" verdict
        let first = CombatRules.registry.Head
        let wrongKind = { first with Metadata = { first.Metadata with SemanticKind = Algorithm } }
        match Rules.validate (wrongKind :: CombatRules.registry.Tail) with Error errors when errors |> List.exists (function IncompatibleRuleKind _ -> true | _ -> false) -> () | verdict -> failwithf "Semantic-kind mismatch was accepted: %A" verdict
        let invalidFact = { first with Semantics = FactSemantics { DataKind = RuleValueKind.Text; Unit = "damage"; Value = RuleValue.IntegerValue 25 } }
        match Rules.validate (invalidFact :: CombatRules.registry.Tail) with Error errors when errors |> List.exists (function InvalidTypedValue _ -> true | _ -> false) -> () | verdict -> failwithf "Typed-value mismatch was accepted: %A" verdict
        let invalidStatus = { first with Metadata = { first.Metadata with Status = Superseded; Supersedes = [] } }
        match Rules.validate (invalidStatus :: CombatRules.registry.Tail) with Error errors when errors |> List.exists (function IncompatibleRuleStatus _ -> true | _ -> false) -> () | verdict -> failwithf "Invalid supersession was accepted: %A" verdict

        let baseline = CombatRules.packageIdentity
        let changedArtifact = Rules.packageIdentity baseline.EngineIdentity baseline.CompatibilityProfile baseline.PackageVersion baseline.SourceCommit [ "combat-rules-source-sha256", [| 1uy |] ] CombatRules.registry
        require (baseline.ImplementationDigest <> changedArtifact.ImplementationDigest && baseline.SemanticDigest <> changedArtifact.SemanticDigest && baseline.ManifestDigest <> changedArtifact.ManifestDigest) "Algorithm artifact change did not invalidate all package identities."

        let documentationOnly = { first with Metadata = { first.Metadata with Title = first.Metadata.Title + " (documented)" } } :: CombatRules.registry.Tail
        let changedDocumentation = Rules.packageIdentity baseline.EngineIdentity baseline.CompatibilityProfile baseline.PackageVersion baseline.SourceCommit CombatRules.implementationArtifacts documentationOnly
        require (baseline.ImplementationDigest = changedDocumentation.ImplementationDigest && baseline.SemanticDigest = changedDocumentation.SemanticDigest && baseline.ManifestDigest <> changedDocumentation.ManifestDigest) "Documentation-only change masqueraded as executable semantics."

        let canonical =
            CanonicalEncoding.concatenate
                [ Rules.canonicalManifestPayload 1 baseline.SourceCommit CombatRules.registry
                  Rules.canonicalApplicationBytes result.Explanation
                  System.Text.Encoding.UTF8.GetBytes(CombatRules.retainedPackage.ManifestJson)
                  System.Text.Encoding.UTF8.GetBytes(CombatRules.retainedPackage.CoverageJson)
                  baseline.ImplementationDigest
                  baseline.SemanticDigest
                  baseline.ManifestDigest ]

        if injectDivergence then
            let changed = Array.copy canonical
            changed[changed.Length - 1] <- changed[changed.Length - 1] ^^^ 1uy
            changed
        else canonical
