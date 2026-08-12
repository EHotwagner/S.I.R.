namespace SIR.Conformance

open FS.GG.Game.Core
open SIR.Domain
open SIR.Simulation

[<RequireQualifiedAccess>]
module RulesCorpusFixtures =
    let private require condition message = if not condition then failwith message
    let private fp numerator denominator = FixedPoint.fromRatio numerator denominator |> Result.defaultWith (fun _ -> failwith "invalid fixture ratio")
    let private attack eventId =
        CombatRules.resolveAttack
            { Attacker = { Col = 0; Row = 0 }
              TargetFootprint = [ { Col = 1; Row = 0 }; { Col = 1; Row = 1 } ]
              IsTransparent = fun _ -> true
              RangeCells = 1
              Suppression = FixedPoint.zero
              BaseDamage = fp 25 1
              ArmorRetention = fp 4 5
              EventId = eventId }
        |> Result.defaultWith failwith

    let manifestJson () = CombatRules.retainedPackage.ManifestJson
    let coverageJson () = CombatRules.retainedPackage.CoverageJson
    let representativeApplicationBytes () = (attack "fixture-attack-1").Explanation |> Rules.canonicalApplicationBytes

    let evaluate injectDivergence =
        require (CombatRules.registry.Length = 7) "The combat registry must cover two facts, three formulas, one algorithm, and one transition."
        require (CombatRules.registry |> List.map (fun rule -> RuleId.value rule.Metadata.Id) |> List.distinct |> List.length = 7) "Rule IDs are not unique."

        let result = attack "fixture-attack-1"
        require (FixedPoint.raw result.TraceProbability = FixedPoint.Scale) "The fully exposed footprint did not produce probability one."
        require (FixedPoint.raw result.ArmorRetention = 8_000) "Armor retention changed."
        require (result.ExpectedDamage = 20) "Expected damage must be 25 × 1.0 × 0.8 = 20."
        require (result.Explanation.Children.Length = 4) "The attack explanation must expose engagement, trace, armor, and damage."
        require (result.Explanation.Children |> List.exists (fun application -> RuleId.value application.RuleId = "COMBAT-TRACE-002")) "The registered trace algorithm is absent from the derivation."

        let binding = CombatRules.replayBinding result.Explanation
        match CombatRules.resolveHistoricalPackage [ CombatRules.retainedPackage ] binding with
        | ResolvedHistoricalRulePackage package -> require (package.Identity.ManifestDigest = binding.BoundManifestDigest) "Historical lookup returned another package."
        | HistoricalRulePackageUnavailable _ -> failwith "The retained historical package was unavailable."
        match CombatRules.resolveHistoricalPackage [] binding with
        | HistoricalRulePackageUnavailable digest -> require (digest = binding.BoundManifestDigest) "Unavailable state lost the recorded package identity."
        | ResolvedHistoricalRulePackage _ -> failwith "Missing historical content was silently substituted."

        require (CombatRules.retainedPackage.ManifestJson.Contains "\"COMBAT-DAMAGE-001\"") "Generated manifest omitted the damage rule."
        require (CombatRules.retainedPackage.ManifestJson.Contains "FS.GG.Game.Core.Los.lineOfSightBy") "Generated manifest omitted the registered algorithm symbol."
        require (CombatRules.retainedPackage.CoverageJson.Contains "\"outside\":\"legacy\"") "Coverage did not classify the outside authority boundary."
        require ((System.Text.Encoding.UTF8.GetBytes CombatRules.retainedPackage.ManifestJson).Length < 524_288) "Manifest exceeded the 512 KiB budget."
        require ((Rules.canonicalApplicationBytes result.Explanation).Length < 65_536) "Explanation exceeded the 64 KiB budget."

        let duplicate = CombatRules.registry.Head :: CombatRules.registry
        match Rules.validate duplicate with Error errors when errors |> List.exists (function DuplicateRuleId _ -> true | _ -> false) -> () | verdict -> failwithf "Duplicate registration was accepted: %A" verdict

        let baseline = CombatRules.packageIdentity
        let changedArtifact = Rules.packageIdentity baseline.EngineIdentity baseline.CompatibilityProfile baseline.PackageVersion baseline.SourceCommit [ "combat-rules", [| 1uy |] ] CombatRules.registry
        require (baseline.ImplementationDigest <> changedArtifact.ImplementationDigest && baseline.SemanticDigest <> changedArtifact.SemanticDigest && baseline.ManifestDigest <> changedArtifact.ManifestDigest) "Algorithm artifact change did not invalidate all package identities."

        let first = CombatRules.registry.Head
        let documentationOnly = { first with Metadata = { first.Metadata with Title = first.Metadata.Title + " (documented)" } } :: CombatRules.registry.Tail
        let changedDocumentation = Rules.packageIdentity baseline.EngineIdentity baseline.CompatibilityProfile baseline.PackageVersion baseline.SourceCommit [ "combat-rules", CanonicalHash.sha256 (System.Text.Encoding.UTF8.GetBytes "combat-rules-v1") ] documentationOnly
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
