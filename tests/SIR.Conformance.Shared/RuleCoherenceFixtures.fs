namespace SIR.Conformance

open SIR.Domain
open SIR.Simulation

[<RequireQualifiedAccess>]
module RuleCoherenceFixtures =
    let private require condition message = if not condition then failwith message
    let private requiredId value = RuleId.create value |> Result.defaultWith failwith
    let private request mode changed maxWork blockUnknowns =
        { Mode = mode
          ChangedRuleIds = changed |> List.map requiredId
          Bounds = { RuleCoherence.defaultBounds with MaxWorkUnits = maxWork }
          BlockUnknowns = blockUnknowns }
    let private analyze rules prior analysisRequest = RuleCoherence.analyze CombatRules.packageIdentity rules prior analysisRequest
    let private has dimension strength report = report.Findings |> List.exists (fun finding -> finding.Dimension = dimension && finding.Strength = strength)
    let private transition = function { Semantics = TransitionSemantics contract } -> Some contract | _ -> None

    let private unprunedCandidateCount rules =
        let transitions = rules |> List.choose (fun rule -> transition rule |> Option.map (fun contract -> rule, contract))
        [ for index in 0 .. transitions.Length - 1 do
            for other in index + 1 .. transitions.Length - 1 do
                let _, left = transitions[index]
                let _, right = transitions[other]
                let intersects a b = not (Set.intersect (Set.ofList a) (Set.ofList b) |> Set.isEmpty)
                if left.Phase = right.Phase && (intersects left.Effects right.Effects || intersects left.Effects right.Reads || intersects right.Effects left.Reads || intersects left.Events right.Events) then yield 1 ]
        |> List.length

    let private withRule ruleId update =
        CombatRules.registry |> List.map (fun rule -> if RuleId.value rule.Metadata.Id = ruleId then update rule else rule)

    let private conflictRules () =
        let template = CombatRules.registry |> List.find (fun rule -> match rule.Semantics with TransitionSemantics _ -> true | _ -> false)
        let create ruleId =
            { template with
                Metadata = { template.Metadata with Id = requiredId ruleId; Dependencies = []; Supersedes = [] }
                Semantics = TransitionSemantics { Phase = "TestPhase"; Preconditions = []; Reads = [ "state" ]; Effects = [ "state" ]; Events = [ "StateChanged" ] } }
        [ create "TEST-CONFLICT-A"; create "TEST-CONFLICT-B" ]

    let private scaleRules count =
        let template = CombatRules.registry |> List.find (fun rule -> match rule.Semantics with FactSemantics _ -> true | _ -> false)
        [ for index in 1 .. count ->
            { template with
                Metadata = { template.Metadata with Id = requiredId (sprintf "SCALE-%04d" index); Dependencies = []; Supersedes = [] } } ]

    let private scaleTransitionRules count =
        let template = CombatRules.registry |> List.find (fun rule -> match rule.Semantics with TransitionSemantics _ -> true | _ -> false)
        [ for index in 1 .. count ->
            { template with
                Metadata = { template.Metadata with Id = requiredId (sprintf "SCALE-TRANSITION-%04d" index); Dependencies = []; Supersedes = [] }
                Semantics = TransitionSemantics { Phase = "ScalePhase"; Preconditions = []; Reads = [ sprintf "read-%04d" index ]; Effects = [ sprintf "effect-%04d" index ]; Events = [ sprintf "event-%04d" index ] } } ]

    let evaluateProtectedMutation mutation =
        let baselineRequest = request CoherenceMode.Corpus [] 100_000 false
        let first = CombatRules.registry.Head
        let mutatedReport, expectedDimension =
            match mutation with
            | "contradiction" -> analyze (conflictRules ()) None baselineRequest, "logical-compatibility"
            | "unit-mismatch" ->
                withRule "COMBAT-DAMAGE-001" (fun rule -> { rule with Semantics = FormulaSemantics(RuleValueKind.FixedPoint, "damage", Add(Input("damage", RuleValueKind.FixedPoint, "damage"), Input("ratio", RuleValueKind.FixedPoint, "ratio"))) }) |> fun rules -> analyze rules None baselineRequest, "types-units"
            | "undeclared-dependency" ->
                { first with Metadata = { first.Metadata with Dependencies = [ requiredId "MISSING-RULE-001" ] } } :: CombatRules.registry.Tail |> fun rules -> analyze rules None baselineRequest, "references"
            | "prototype-leakage" ->
                withRule "COMBAT-TRACE-002" (fun rule -> { rule with Metadata = { rule.Metadata with Status = Prototype } }) |> fun rules -> analyze rules None baselineRequest, "dependency-status"
            | "history-mismatch" ->
                { first with Metadata = { first.Metadata with RuleSource = first.Metadata.RuleSource |> Option.map (fun source -> { source with Commit = "0000000000000000000000000000000000000000" }) } } :: CombatRules.registry.Tail |> fun rules -> analyze rules None baselineRequest, "history"
            | "unreachable-transition" ->
                let predicate =
                    { first with
                        Metadata = { first.Metadata with Id = requiredId "TEST-ALWAYS-FALSE"; SemanticKind = RuleKind.Predicate; Dependencies = [] }
                        Semantics = PredicateSemantics(Constant { DataKind = RuleValueKind.Boolean; Unit = "boolean"; Value = BooleanValue false }) }
                let transition = (conflictRules ()).Head
                let unreachable =
                    { transition with
                        Metadata = { transition.Metadata with Dependencies = [ predicate.Metadata.Id ] }
                        Semantics = match transition.Semantics with TransitionSemantics contract -> TransitionSemantics { contract with Preconditions = [ predicate.Metadata.Id ] } | _ -> transition.Semantics }
                analyze [ predicate; unreachable ] None baselineRequest, "reachability"
            | _ -> failwithf "Unknown rule-coherence mutation: %s" mutation
        if has expectedDimension ClaimStrength.Failed mutatedReport then
            eprintfn "first coherence failure: mutation=%s dimension=%s" mutation expectedDimension
            failwith "Rule coherence mutation correctly made the gate red."
        failwithf "Rule coherence mutation was accepted: %s" mutation

    let evaluate () =
        let baselineRequest = request CoherenceMode.Corpus [] 100_000 false
        let baseline = analyze CombatRules.registry None baselineRequest
        require (baseline.Termination = AnalysisTermination.Complete) "Corpus coherence did not complete."
        require (not (has "logical-compatibility" ClaimStrength.Failed baseline)) "The accepted combat corpus contains an unordered transition conflict."
        require (has "interaction" ClaimStrength.Unknown baseline) "Opaque algorithm limits were not reported as unknown."
        require (baseline.Cost.CandidatePairs = int32 (unprunedCandidateCount CombatRules.registry)) "Indexed candidate selection disagrees with the bounded unpruned oracle."
        require (baseline.Cost.WorkUnits = baseline.Cost.RulesInSlice + baseline.Cost.CandidatePairs) "Disjoint pairs consumed analyzer work."
        require (baseline.Cost.PrunedPairs > baseline.Cost.CandidatePairs) "The interaction index did not prune the combat corpus."
        require ((RuleCoherence.canonicalReportBytes baseline).Length < 131_072) "Bounded summary exceeded 128 KiB."

        let reversed = analyze (List.rev CombatRules.registry) None baselineRequest
        require (RuleCoherence.canonicalReportBytes baseline = RuleCoherence.canonicalReportBytes reversed) "Rule input order changed the report."

        let cache = baseline.CacheEntry |> Option.defaultWith (fun () -> failwith "Complete analysis emitted no cache entry.")
        let warm = analyze CombatRules.registry (Some cache) baselineRequest
        require (warm.Cost.WorkUnits = 0 && warm.Cost.ExpensiveAnalyses = 0 && warm.Cost.CacheHits = 1) "Warm unchanged analysis repeated work."

        let first = CombatRules.registry.Head
        let documented = { first with Metadata = { first.Metadata with Title = first.Metadata.Title + " documented" } } :: CombatRules.registry.Tail
        let documentationWarm = analyze documented (Some cache) baselineRequest
        require (documentationWarm.Cost.CacheHits = 1 && documentationWarm.Cost.WorkUnits = 0) "Documentation-only change invalidated semantic analysis."

        let statusChanged =
            withRule "COMBAT-TRACE-002" (fun rule -> { rule with Metadata = { rule.Metadata with Status = Prototype } })
            |> fun rules -> analyze rules (Some cache) baselineRequest
        require (statusChanged.Cost.CacheHits = 0 && has "dependency-status" ClaimStrength.Failed statusChanged) "Status-only mutation reused stale coherence cache state."

        let supersessionChanged =
            let replacement = CombatRules.registry.Tail.Head.Metadata.Id
            { first with Metadata = { first.Metadata with Supersedes = [ replacement ] } } :: CombatRules.registry.Tail
            |> fun rules -> analyze rules (Some cache) baselineRequest
        require (supersessionChanged.Cost.CacheHits = 0) "Supersession-only mutation reused stale coherence cache state."

        let sourceChanged =
            { first with Metadata = { first.Metadata with RuleSource = first.Metadata.RuleSource |> Option.map (fun source -> { source with Commit = String.replicate 40 "0" }) } } :: CombatRules.registry.Tail
            |> fun rules -> analyze rules (Some cache) baselineRequest
        require (sourceChanged.Cost.CacheHits = 0 && has "history" ClaimStrength.Failed sourceChanged) "Source-binding mutation reused stale coherence cache state."

        let evidenceChanged =
            { first with Metadata = { first.Metadata with Evidence = [] } } :: CombatRules.registry.Tail
            |> fun rules -> analyze rules (Some cache) baselineRequest
        require (evidenceChanged.Cost.CacheHits = 0 && has "coverage" ClaimStrength.Failed evidenceChanged) "Evidence metadata mutation reused stale coherence cache state."

        let fingerprintChanged =
            withRule "COMBAT-TRACE-002" (fun rule ->
                match rule.Semantics with
                | AlgorithmSemantics contract -> { rule with Semantics = AlgorithmSemantics { contract with Fingerprint = contract.Fingerprint + ":changed" } }
                | _ -> failwith "Expected algorithm rule.")
        let invalidated = analyze fingerprintChanged (Some cache) baselineRequest
        require (invalidated.Cost.CacheHits = 0 && invalidated.Cost.WorkUnits > 0) "Algorithm fingerprint change reused poisoned cache state."

        let exhausted = analyze CombatRules.registry None (request CoherenceMode.Corpus [] 1 false)
        require (exhausted.Termination = AnalysisTermination.WorkBudgetExhausted && not exhausted.PendingShards.IsEmpty && not exhausted.CanonicalizationReady) "Work exhaustion did not return a deterministic partial report."

        let changed = analyze CombatRules.registry None (request CoherenceMode.Changed [ "CONTENT-WEAPON-RIFLE-001" ] 100 false)
        require (changed.AnalyzedRuleIds |> List.map RuleId.value = [ "CONTENT-WEAPON-RIFLE-001" ] && changed.Cost.WorkUnits = 1) "Changed mode escaped its exact slice."
        let changedCache = changed.CacheEntry |> Option.defaultWith (fun () -> failwith "Changed analysis emitted no cache entry.")
        let unrelatedAlgorithmChange =
            withRule "COMBAT-TRACE-002" (fun rule ->
                match rule.Semantics with
                | AlgorithmSemantics contract -> { rule with Semantics = AlgorithmSemantics { contract with Fingerprint = contract.Fingerprint + ":outside-slice" } }
                | _ -> failwith "Expected algorithm rule.")
        let unchangedSlice = analyze unrelatedAlgorithmChange (Some changedCache) (request CoherenceMode.Changed [ "CONTENT-WEAPON-RIFLE-001" ] 100 false)
        require (unchangedSlice.Cost.CacheHits = 1 && unchangedSlice.Cost.WorkUnits = 0) "Unrelated semantic change invalidated an exact changed-rule slice."
        let unrelatedRule =
            let template = CombatRules.registry |> List.find (fun rule -> match rule.Semantics with FactSemantics _ -> true | _ -> false)
            { template with Metadata = { template.Metadata with Id = requiredId "TEST-UNRELATED-ADDED"; Dependencies = []; Supersedes = [] } }
        let unrelatedAddition = analyze (unrelatedRule :: CombatRules.registry) (Some changedCache) (request CoherenceMode.Changed [ "CONTENT-WEAPON-RIFLE-001" ] 100 false)
        require (unrelatedAddition.Cost.CacheHits = 1 && unrelatedAddition.Cost.WorkUnits = 0) "Unrelated rule addition invalidated an exact changed-rule slice."
        let observedDependencyBaseline = analyze CombatRules.registry None (request CoherenceMode.Changed [ "COMBAT-DAMAGE-001" ] 100 false)
        let observedDependencyCache = observedDependencyBaseline.CacheEntry |> Option.defaultWith (fun () -> failwith "Observed-dependency analysis emitted no cache entry.")
        let observedDependencyChanged =
            withRule "COMBAT-TRACE-002" (fun rule -> { rule with Metadata = { rule.Metadata with Status = Prototype } })
            |> fun rules -> analyze rules (Some observedDependencyCache) (request CoherenceMode.Changed [ "COMBAT-DAMAGE-001" ] 100 false)
        require (observedDependencyChanged.Cost.CacheHits = 0 && has "dependency-status" ClaimStrength.Failed observedDependencyChanged) "Changed-mode cache reused after an observed dependency status mutation outside the exact slice."
        let observedDependencyRemoved =
            CombatRules.registry
            |> List.filter (fun rule -> RuleId.value rule.Metadata.Id <> "COMBAT-TRACE-002")
            |> fun rules -> analyze rules (Some observedDependencyCache) (request CoherenceMode.Changed [ "COMBAT-DAMAGE-001" ] 100 false)
        require (observedDependencyRemoved.Cost.CacheHits = 0 && has "references" ClaimStrength.Failed observedDependencyRemoved) "Changed-mode cache reused after an observed dependency was removed."
        let missingChanged = analyze CombatRules.registry None (request CoherenceMode.Changed [ "MISSING-RULE-001" ] 100 false)
        require (has "references" ClaimStrength.Failed missingChanged && not missingChanged.CanonicalizationReady) "Unknown changed-rule seed was accepted."
        let cone = analyze CombatRules.registry None (request CoherenceMode.Cone [ "COMBAT-DAMAGE-001" ] 1_000 false)
        require (cone.Cost.RulesInSlice > 1 && cone.Cost.RulesInSlice < int32 CombatRules.registry.Length) "Cone mode did not select a bounded dependency/dependant slice."

        let truncated =
            analyze (conflictRules ()) None
                { baselineRequest with Bounds = { baselineRequest.Bounds with MaxFindings = 0 } }
        require (truncated.Termination = AnalysisTermination.WorkBudgetExhausted && truncated.PendingShards = [ "finding-output-truncated" ] && not truncated.CanonicalizationReady && truncated.CacheEntry.IsNone) "Finding truncation was reported as complete."

        let conflict = analyze (conflictRules ()) None baselineRequest
        require (has "logical-compatibility" ClaimStrength.Failed conflict && not conflict.CanonicalizationReady) "Unordered shared-write/event contradiction was accepted."
        let conflictCone = analyze (conflictRules ()) None (request CoherenceMode.Cone [ "TEST-CONFLICT-A" ] 100 false)
        require (conflictCone.AnalyzedRuleIds |> List.map RuleId.value = [ "TEST-CONFLICT-A"; "TEST-CONFLICT-B" ] && has "logical-compatibility" ClaimStrength.Failed conflictCone) "Cone mode omitted an indexed same-phase interaction conflict."

        let dangling =
            { first with Metadata = { first.Metadata with Dependencies = [ requiredId "MISSING-RULE-001" ] } } :: CombatRules.registry.Tail
            |> fun rules -> analyze rules None baselineRequest
        require (has "references" ClaimStrength.Failed dangling) "Dangling dependency was accepted."

        let prototype = withRule "COMBAT-TRACE-002" (fun rule -> { rule with Metadata = { rule.Metadata with Status = Prototype } }) |> fun rules -> analyze rules None baselineRequest
        require (has "dependency-status" ClaimStrength.Failed prototype) "Prototype-to-canonical authority leakage was accepted."

        let unitMismatch =
            withRule "COMBAT-DAMAGE-001" (fun rule ->
                { rule with Semantics = FormulaSemantics(RuleValueKind.FixedPoint, "damage", Add(Input("damage", RuleValueKind.FixedPoint, "damage"), Input("ratio", RuleValueKind.FixedPoint, "ratio"))) })
            |> fun rules -> analyze rules None baselineRequest
        require (has "types-units" ClaimStrength.Failed unitMismatch) "Unit mismatch was accepted."

        let fixedValue unitName raw = { DataKind = RuleValueKind.FixedPoint; Unit = unitName; Value = FixedPointValue(FixedPoint.fromRaw raw) }
        let likeDivision = Divide(Input("left", RuleValueKind.FixedPoint, "damage"), Input("right", RuleValueKind.FixedPoint, "damage"))
        let likeDivisionRules =
            withRule "COMBAT-DAMAGE-001" (fun rule -> { rule with Semantics = FormulaSemantics(RuleValueKind.FixedPoint, "ratio", likeDivision) })
        let likeDivisionReport = analyze likeDivisionRules None baselineRequest
        require (not (has "types-units" ClaimStrength.Failed likeDivisionReport)) "Like-shaped fixed-point division was rejected by coherence inference."
        match Rules.evaluate (Map.ofList [ "left", fixedValue "damage" 200; "right", fixedValue "damage" 100 ]) likeDivision with
        | Ok value -> require (value.DataKind = RuleValueKind.FixedPoint && value.Unit = "ratio") "Executable like-shaped division did not return ratio."
        | Error error -> failwithf "Executable like-shaped division failed: %A" error

        let unlikeDivision = Divide(Input("left", RuleValueKind.FixedPoint, "damage"), Input("right", RuleValueKind.FixedPoint, "ratio"))
        let unlikeDivisionRules =
            withRule "COMBAT-DAMAGE-001" (fun rule -> { rule with Semantics = FormulaSemantics(RuleValueKind.FixedPoint, "ratio", unlikeDivision) })
        let unlikeDivisionReport = analyze unlikeDivisionRules None baselineRequest
        require (has "types-units" ClaimStrength.Failed unlikeDivisionReport) "Damage/ratio division was accepted by coherence inference."
        match Rules.evaluate (Map.ofList [ "left", fixedValue "damage" 200; "right", fixedValue "ratio" 100 ]) unlikeDivision with
        | Error(UnitMismatch _) -> ()
        | verdict -> failwithf "Executable damage/ratio division did not reject the unit mismatch: %A" verdict

        let wronglyDeclaredDivisionRules =
            withRule "COMBAT-DAMAGE-001" (fun rule -> { rule with Semantics = FormulaSemantics(RuleValueKind.FixedPoint, "damage", likeDivision) })
        let wronglyDeclaredDivisionReport = analyze wronglyDeclaredDivisionRules None baselineRequest
        require (has "types-units" ClaimStrength.Failed wronglyDeclaredDivisionReport) "Ratio division was accepted under a declared damage result."

        let nonBooleanPredicateRules =
            withRule "COMBAT-DAMAGE-001" (fun rule ->
                { rule with
                    Metadata = { rule.Metadata with SemanticKind = RuleKind.Predicate }
                    Semantics = PredicateSemantics(Constant(fixedValue "damage" 1)) })
        let nonBooleanPredicateReport = analyze nonBooleanPredicateRules None baselineRequest
        require (has "types-units" ClaimStrength.Failed nonBooleanPredicateReport) "A non-Boolean predicate result was accepted."

        let duplicate = analyze (first :: CombatRules.registry) None baselineRequest
        require (has "identity" ClaimStrength.Failed duplicate) "Duplicate rule identity was accepted."

        let cycleA, cycleB = CombatRules.registry[0], CombatRules.registry[1]
        let cyclic =
            CombatRules.registry
            |> List.map (fun rule ->
                if RuleId.value rule.Metadata.Id = RuleId.value cycleA.Metadata.Id then { rule with Metadata = { rule.Metadata with Dependencies = [ cycleB.Metadata.Id ] } }
                elif RuleId.value rule.Metadata.Id = RuleId.value cycleB.Metadata.Id then { rule with Metadata = { rule.Metadata with Dependencies = [ cycleA.Metadata.Id ] } }
                else rule)
            |> fun rules -> analyze rules None baselineRequest
        require (has "dependency-structure" ClaimStrength.Failed cyclic) "Dependency cycle was accepted."

        let staleSource = { first with Metadata = { first.Metadata with RuleSource = first.Metadata.RuleSource |> Option.map (fun source -> { source with Commit = "0000000000000000000000000000000000000000" }) } } :: CombatRules.registry.Tail |> fun rules -> analyze rules None baselineRequest
        require (has "history" ClaimStrength.Failed staleSource) "Historical source/package mismatch was accepted."

        let scale = analyze (scaleRules 256) None (request CoherenceMode.Corpus [] 1_000 false)
        require (scale.Termination = AnalysisTermination.Complete && scale.Cost.RulesInSlice = 256 && scale.Cost.CandidatePairs = 0 && scale.Cost.WorkUnits = 256) "Disjoint synthetic corpus did not scale with the affected slice."
        let transitionScale = analyze (scaleTransitionRules 256) None (request CoherenceMode.Cone [ "SCALE-TRANSITION-0001" ] 32 false)
        require (transitionScale.Termination = AnalysisTermination.Complete && transitionScale.Cost.RulesInSlice = 1 && transitionScale.Cost.CandidatePairs = 0 && transitionScale.Cost.WorkUnits < 16) "Transition-heavy cone selection did not scale with indexed relevant edges."
        require ((RuleCoherence.canonicalReportBytes scale).Length < 131_072) "Model-facing summary grew with passing corpus detail."

        RuleCoherence.canonicalReportBytes baseline
