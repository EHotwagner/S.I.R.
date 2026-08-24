namespace SIR.Conformance

open FS.GG.Game.Core
open FS.GG.SDD.Artifacts.TypedSpecifications
open SIR.Domain
open SIR.Simulation

[<RequireQualifiedAccess>]
module RulesCorpusFixtures =
    let private require condition message =
        if not condition then
            failwith message

    let private fp numerator denominator =
        FixedPoint.fromRatio numerator denominator
        |> Result.defaultWith (fun _ -> failwith "invalid fixture ratio")

    let private hexBytes (value: string) =
        let nibble character =
            if character >= '0' && character <= '9' then
                int character - int '0'
            else
                int character - int 'a' + 10

        [| for index in 0..2 .. value.Length - 2 -> byte (nibble value[index] * 16 + nibble value[index + 1]) |]

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

    let manifestJson () =
        CombatRules.retainedPackage.ManifestJson

    let coverageJson () =
        CombatRules.retainedPackage.CoverageJson

    let representativeApplicationBytes () =
        (attack "fixture-attack-1").Explanation |> Rules.canonicalApplicationBytes

    let specificationMarkdown () =
        CombatRules.damageSpecification
        |> RuleSpecification.markdownProjection
        |> Result.defaultWith (fun diagnostics -> failwithf "Specification projection failed: %A" diagnostics)

    let specificationReceiptJson () =
        CombatRules.damageSpecification
        |> RuleSpecification.projectionReceiptJson
            "hybrid"
            "tests/fixtures/rules-corpus/v2/combat-damage-001.specification.md"
        |> Result.defaultWith (fun diagnostics -> failwithf "Specification receipt failed: %A" diagnostics)

    let performanceWorkload iterations =
        let mutable checksum = 0

        for index in 1..iterations do
            checksum <- checksum + (attack ("performance-" + string index)).ExpectedDamage

        let explanation = (attack "performance-evidence").Explanation

        let rec counts application =
            let childApplications, childOperands =
                application.Children
                |> List.map counts
                |> List.fold
                    (fun (applications, operands) (childApplicationCount, childOperandCount) ->
                        applications + childApplicationCount, operands + childOperandCount)
                    (0, 0)

            1 + childApplications, application.Operands.Length + childOperands

        let applications, operands = counts explanation

        checksum,
        applications,
        operands,
        (Rules.canonicalApplicationBytes explanation).Length,
        (System.Text.Encoding.UTF8.GetBytes CombatRules.retainedPackage.ManifestJson).Length

    let evaluate injectDivergence =
        let selected = CombatRules.damageSpecification

        let compiled =
            RuleSpecification.compile selected
            |> Result.defaultWith (fun diagnostics ->
                failwithf "Selected specification did not compile: %A" diagnostics)

        require
            (Rules.canonicalRuleBytes compiled = CombatRules.damageReferenceCanonicalBytes)
            "The migrated specification changed COMBAT-DAMAGE-001 canonical bytes."

        let direct: SpecificationModel<RuleSpecificationAst> =
            { Identity = selected.Identity
              SchemaVersion = selected.SchemaVersion
              Provenance = selected.Provenance
              Intent = "Direct-record authoring trial."
              EvidenceObligations = selected.EvidenceObligations
              Extension = selected.Extension }

        let computation =
            RuleSpecification.computation
                selected.Identity
                selected.Provenance
                "Computation-expression authoring trial." {
                definition compiled
                reads selected.Extension.Reads
                writes selected.Extension.Writes
            }

        let normalized model =
            RuleSpecification.normalizedBytes model
            |> Result.defaultWith (fun diagnostics -> failwithf "Normalization failed: %A" diagnostics)

        require
            (normalized direct = normalized selected)
            "Direct-record and selected hybrid authoring normalized differently."

        require
            (normalized computation = normalized selected)
            "Computation-expression and selected hybrid authoring normalized differently."

        let fingerprint model =
            RuleSpecification.fingerprint model
            |> Result.defaultWith (fun diagnostics -> failwithf "Fingerprint failed: %A" diagnostics)

        require
            (fingerprint direct = fingerprint selected
             && fingerprint computation = fingerprint selected)
            "Equivalent authoring forms produced different fingerprints."

        let compiledBytes model =
            RuleSpecification.compile model
            |> Result.defaultWith (fun diagnostics -> failwithf "Compilation failed: %A" diagnostics)
            |> Rules.canonicalRuleBytes

        require
            (compiledBytes direct = compiledBytes selected
             && compiledBytes computation = compiledBytes selected)
            "Equivalent authoring forms compiled differently."

        match RuleSpecification.semanticDiff direct computation with
        | Ok Equivalent -> ()
        | verdict -> failwithf "Equivalent authoring forms produced a semantic diff: %A" verdict

        let invalidProvenance =
            { selected with
                Provenance =
                    { selected.Provenance with
                        SourceRevision = "not-a-git-object" } }

        match RuleSpecification.validate invalidProvenance with
        | diagnostics when
            diagnostics
            |> List.exists (fun item ->
                item.Code = "SPEC-PROVENANCE-REVISION"
                && item.Path = "/provenance/sourceRevision")
            ->
            ()
        | diagnostics -> failwithf "Malformed provenance did not produce the stable diagnostic: %A" diagnostics

        let invalidAst =
            { selected with
                Extension =
                    { selected.Extension with
                        Definition =
                            { selected.Extension.Definition with
                                Metadata =
                                    { selected.Extension.Definition.Metadata with
                                        Evidence = [] } } } }

        match RuleSpecification.validate invalidAst with
        | diagnostics when
            diagnostics
            |> List.exists (fun item ->
                item.Code = "RULE-SPEC-EVIDENCE-REQUIRED"
                && item.Path = "/extension/definition/metadata/evidence")
            ->
            ()
        | diagnostics -> failwithf "Invalid rule AST did not produce the stable diagnostic: %A" diagnostics

        let changed =
            { selected with
                Extension =
                    { selected.Extension with
                        Definition =
                            { selected.Extension.Definition with
                                Metadata =
                                    { selected.Extension.Definition.Metadata with
                                        Rationale = selected.Extension.Definition.Metadata.Rationale + " Changed." } } } }

        match RuleSpecification.semanticDiff selected changed with
        | Ok(Changed changes) when changes |> List.exists (fun change -> change.Path = "/extension") -> ()
        | verdict -> failwithf "Semantic AST change was not reported deterministically: %A" verdict

        let traceDefinition =
            CombatRules.registry
            |> List.find (fun rule -> RuleId.value rule.Metadata.Id = "COMBAT-TRACE-002")

        let traceModel =
            RuleSpecification.hybrid
                (SpecificationId.create "COMBAT-TRACE-002" |> Result.defaultWith failwith)
                { selected.Provenance with
                    Agent = "conformance"
                    Session = "registered-algorithm" }
                "Verify explicit registered-algorithm bindings."
                traceDefinition
                [ "visibleSamples"; "totalSamples" ]
                [ "no-write:pure-result" ]

        match RuleSpecification.tryRegisteredAlgorithm traceModel.Extension with
        | Some algorithm ->
            require
                (algorithm.ImplementationSymbol = "FS.GG.Game.Core.Los.lineOfSightBy")
                "Registered algorithm lost its implementation symbol."

            require
                (algorithm.Inputs.Length = 2
                 && algorithm.Reads.Length = 2
                 && algorithm.Writes = [ "no-write:pure-result" ])
                "Registered algorithm lost explicit input/read/write bindings."

            require
                (not (List.isEmpty algorithm.Evidence)
                 && not (List.isEmpty algorithm.ExplanationFields))
                "Registered algorithm lost evidence or explanation bindings."
        | None -> failwith "Algorithm specification was not exposed as a registered algorithm."

        require
            ((specificationMarkdown ()).Contains "<!-- fsgg-typed-specification/v1 -->")
            "Generated specification Markdown omitted its shared-kernel schema marker."

        require
            ((specificationReceiptJson ()).Contains "\"selectedSurface\":\"hybrid\"")
            "Generated specification receipt omitted the selected authoring surface."

        require
            (CombatRules.registry.Length = 16)
            "The combat registry must cover the complete physical-combat consequence chain."

        require
            (CombatRules.registry
             |> List.map (fun rule -> RuleId.value rule.Metadata.Id)
             |> List.distinct
             |> List.length = 16)
            "Rule IDs are not unique."

        let requiredPhysicalRules =
            [ "COMBAT-COLLISION-001"
              "COMBAT-COVER-003"
              "COMBAT-PENETRATION-001"
              "COMBAT-HEALTH-001"
              "COMBAT-WOUND-001"
              "COMBAT-SUPPRESSION-001"
              "COMBAT-SUPPRESSION-RECOVERY-001"
              "COMBAT-COLLATERAL-001"
              "COMBAT-COVER-DESTRUCTION-001" ]

        let registeredIds =
            CombatRules.registry
            |> List.map (fun rule -> RuleId.value rule.Metadata.Id)
            |> Set.ofList

        for id in requiredPhysicalRules do
            require (Set.contains id registeredIds) ("The executable physical-combat registry omitted " + id + ".")

        let result = attack "fixture-attack-1"

        require
            (FixedPoint.raw result.TraceProbability = FixedPoint.Scale)
            "The fully exposed footprint did not produce probability one."

        require (FixedPoint.raw result.ArmorRetention = 8_000) "Armor retention changed."
        require (result.ExpectedDamage = 20) "Expected damage must be 25 × 1.0 × 0.8 = 20."

        require
            (result.Explanation.Children.Length = 4)
            "The attack explanation must expose engagement, trace, armor, and damage."

        require
            (result.Explanation.Children
             |> List.exists (fun application -> RuleId.value application.RuleId = "COMBAT-TRACE-002"))
            "The registered trace algorithm is absent from the derivation."

        let consequences =
            CombatRules.resolveConsequences
                100
                0
                10
                { Attacker = { Col = 0; Row = 0 }
                  TargetFootprint = [ { Col = 1; Row = 0 } ]
                  VisibleSamples = 1
                  TotalSamples = 1
                  RangeCells = 1
                  Suppression = FixedPoint.zero
                  BaseDamage = fp 25 1
                  ArmorRetention = fp 4 5
                  EventId = "fixture-consequences" }
            |> Result.defaultWith failwith

        let appliedIds =
            consequences.Explanation.Children
            |> List.map (fun application -> RuleId.value application.RuleId)
            |> Set.ofList

        for id in
            [ "COMBAT-COLLISION-001"
              "COMBAT-COVER-003"
              "COMBAT-PENETRATION-001"
              "COMBAT-HEALTH-001"
              "COMBAT-WOUND-001"
              "COMBAT-SUPPRESSION-001"
              "COMBAT-COLLATERAL-001" ] do
            require (Set.contains id appliedIds) ("The authoritative consequence derivation omitted " + id + ".")

        let overRetained = attackWithRetention "fixture-attack-over-retained" (fp 6 5)

        require
            (FixedPoint.raw overRetained.ArmorRetention = FixedPoint.Scale)
            "Armor retention was not clamped to one."

        require (overRetained.ExpectedDamage = 25) "Damage did not consume the clamped armor-rule result."

        let damageApplication =
            overRetained.Explanation.Children
            |> List.find (fun application -> RuleId.value application.RuleId = "COMBAT-DAMAGE-001")

        let explainedRetention =
            damageApplication.Operands
            |> List.find (fun (name, _) -> name = "retention")
            |> snd

        match explainedRetention.Value with
        | RuleValue.FixedPointValue value ->
            require
                (FixedPoint.raw value = FixedPoint.raw overRetained.ArmorRetention)
                "The damage operand diverged from authoritative clamped retention."
        | _ -> failwith "The damage retention operand lost its fixed-point kind."

        let binding = CombatRules.replayBinding result.Explanation

        match CombatRules.resolveHistoricalPackage [ CombatRules.retainedPackage ] binding with
        | ResolvedHistoricalRulePackage package ->
            require
                (package.Identity.ManifestDigest = binding.BoundManifestDigest)
                "Historical lookup returned another package."
        | HistoricalRulePackageUnavailable _ -> failwith "The retained historical package was unavailable."

        match CombatRules.resolveHistoricalPackage [] binding with
        | HistoricalRulePackageUnavailable digest ->
            require (digest = binding.BoundManifestDigest) "Unavailable state lost the recorded package identity."
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

        let retainedV1 =
            { Identity = retainedV1Identity
              ManifestJson = "retained:v1:f590b9bf"
              CoverageJson = "retained:v1:coverage" }

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
        | ResolvedHistoricalRulePackage package ->
            require
                (package.ManifestJson = retainedV1.ManifestJson)
                "Changed current rules replaced the exact retained v1 package."
        | HistoricalRulePackageUnavailable _ -> failwith "Retained v1 was unavailable after current rules changed."

        match CombatRules.resolveHistoricalPackage [ CombatRules.retainedPackage ] historicalBinding with
        | HistoricalRulePackageUnavailable digest ->
            require (digest = retainedV1Identity.ManifestDigest) "Unavailable v1 lost its exact digest."
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
            require
                (package.Identity.SourceCommit = binding.BoundSourceCommit)
                "A changed current package replaced the pinned historical source."

            require
                (package.ManifestJson = CombatRules.retainedPackage.ManifestJson)
                "Historical lookup did not return the exact retained manifest."
        | HistoricalRulePackageUnavailable _ ->
            failwith "The exact old package was not resolved after current artifacts changed."

        match CombatRules.resolveHistoricalPackage [ changedPackage ] binding with
        | HistoricalRulePackageUnavailable digest ->
            require
                (digest = binding.BoundManifestDigest)
                "Changed current content was substituted for an unavailable historical package."
        | ResolvedHistoricalRulePackage _ ->
            failwith "A changed current package silently reinterpreted historical evidence."

        require
            (CombatRules.retainedPackage.ManifestJson.Contains "\"COMBAT-DAMAGE-001\"")
            "Generated manifest omitted the damage rule."

        require
            (CombatRules.retainedPackage.ManifestJson.Contains "FS.GG.Game.Core.Los.lineOfSightBy")
            "Generated manifest omitted the registered algorithm symbol."

        require
            (CombatRules.retainedPackage.CoverageJson.Contains "\"outside\":\"legacy\"")
            "Coverage did not classify the outside authority boundary."

        for kind in
            [ "rule"
              "implementation"
              "event"
              "explanation"
              "example/property"
              "documentation"
              "source"
              "replay" ] do
            require
                (CombatRules.retainedPackage.CoverageJson.Contains("\"kind\":\"" + kind + "\""))
                ("Coverage omitted the required " + kind + " node kind.")

        for rule in CombatRules.registry do
            let id = RuleId.value rule.Metadata.Id

            require
                (CombatRules.retainedPackage.CoverageJson.Contains("\"from\":\"rule:" + id + "\""))
                ("Coverage omitted outgoing reachability for " + id + ".")

        for edgeKind in
            [ "implementation"
              "event/application"
              "explanation"
              "example"
              "property"
              "documentation"
              "source"
              "replay" ] do
            require
                (CombatRules.retainedPackage.CoverageJson.Contains("\"kind\":\"" + edgeKind + "\""))
                ("Coverage omitted the required " + edgeKind + " edge kind.")

        require
            ((System.Text.Encoding.UTF8.GetBytes CombatRules.retainedPackage.ManifestJson).Length < 524_288)
            "Manifest exceeded the 512 KiB budget."

        require
            ((Rules.canonicalApplicationBytes result.Explanation).Length < 65_536)
            "Explanation exceeded the 64 KiB budget."

        let duplicate = CombatRules.registry.Head :: CombatRules.registry

        match Rules.validate duplicate with
        | Error errors when
            errors
            |> List.exists (function
                | DuplicateRuleId _ -> true
                | _ -> false)
            ->
            ()
        | verdict -> failwithf "Duplicate registration was accepted: %A" verdict

        let first = CombatRules.registry.Head

        let wrongKind =
            { first with
                Metadata =
                    { first.Metadata with
                        SemanticKind = Algorithm } }

        match Rules.validate (wrongKind :: CombatRules.registry.Tail) with
        | Error errors when
            errors
            |> List.exists (function
                | IncompatibleRuleKind _ -> true
                | _ -> false)
            ->
            ()
        | verdict -> failwithf "Semantic-kind mismatch was accepted: %A" verdict

        let invalidFact =
            { first with
                Semantics =
                    FactSemantics
                        { DataKind = RuleValueKind.Text
                          Unit = "damage"
                          Value = RuleValue.IntegerValue 25 } }

        match Rules.validate (invalidFact :: CombatRules.registry.Tail) with
        | Error errors when
            errors
            |> List.exists (function
                | InvalidTypedValue _ -> true
                | _ -> false)
            ->
            ()
        | verdict -> failwithf "Typed-value mismatch was accepted: %A" verdict

        let invalidStatus =
            { first with
                Metadata =
                    { first.Metadata with
                        Status = Superseded
                        Supersedes = [] } }

        match Rules.validate (invalidStatus :: CombatRules.registry.Tail) with
        | Error errors when
            errors
            |> List.exists (function
                | IncompatibleRuleStatus _ -> true
                | _ -> false)
            ->
            ()
        | verdict -> failwithf "Invalid supersession was accepted: %A" verdict

        let baseline = CombatRules.packageIdentity

        let changedArtifact =
            Rules.packageIdentity
                baseline.EngineIdentity
                baseline.CompatibilityProfile
                baseline.PackageVersion
                baseline.SourceCommit
                [ "combat-rules-source-sha256", [| 1uy |] ]
                CombatRules.registry

        require
            (baseline.ImplementationDigest <> changedArtifact.ImplementationDigest
             && baseline.SemanticDigest <> changedArtifact.SemanticDigest
             && baseline.ManifestDigest <> changedArtifact.ManifestDigest)
            "Algorithm artifact change did not invalidate all package identities."

        let documentationOnly =
            { first with
                Metadata =
                    { first.Metadata with
                        Title = first.Metadata.Title + " (documented)" } }
            :: CombatRules.registry.Tail

        let changedDocumentation =
            Rules.packageIdentity
                baseline.EngineIdentity
                baseline.CompatibilityProfile
                baseline.PackageVersion
                baseline.SourceCommit
                CombatRules.implementationArtifacts
                documentationOnly

        require
            (baseline.ImplementationDigest = changedDocumentation.ImplementationDigest
             && baseline.SemanticDigest = changedDocumentation.SemanticDigest
             && baseline.ManifestDigest <> changedDocumentation.ManifestDigest)
            "Documentation-only change masqueraded as executable semantics."

        let canonical =
            CanonicalEncoding.concatenate
                [ Rules.canonicalManifestPayload 1 baseline.SourceCommit CombatRules.registry
                  Rules.canonicalApplicationBytes result.Explanation
                  System.Text.Encoding.UTF8.GetBytes(CombatRules.retainedPackage.ManifestJson)
                  System.Text.Encoding.UTF8.GetBytes(CombatRules.retainedPackage.CoverageJson)
                  RuleCoherenceFixtures.evaluate ()
                  baseline.ImplementationDigest
                  baseline.SemanticDigest
                  baseline.ManifestDigest ]

        if injectDivergence then
            let changed = Array.copy canonical
            changed[changed.Length - 1] <- changed[changed.Length - 1] ^^^ 1uy
            changed
        else
            canonical
