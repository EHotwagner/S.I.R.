namespace SIR.Domain

open System
open System.Text

[<RequireQualifiedAccess>]
type CoherenceMode = Changed | Cone | Corpus

[<RequireQualifiedAccess>]
type ClaimStrength = ProvedStructural | ProvedFragment | ExhaustiveBounded | Tested | Heuristic | Unknown | Failed

[<RequireQualifiedAccess>]
type AnalysisTermination = Complete | WorkBudgetExhausted

type CoherenceBounds =
    { MaxWorkUnits: int32
      MaxFindings: int32
      MaxWitnessRules: int32 }

type CoherenceRequest =
    { Mode: CoherenceMode
      ChangedRuleIds: RuleId list
      Bounds: CoherenceBounds
      BlockUnknowns: bool }

type CoherenceWitness =
    { RuleIds: RuleId list
      Fact: string
      Expected: string
      Actual: string }

type CoherenceFinding =
    { Fingerprint: string
      Dimension: string
      Strength: ClaimStrength
      RuleIds: RuleId list
      Message: string
      DependencyReason: string
      Witness: CoherenceWitness option }

type CoherenceCost =
    { RulesInCorpus: int32
      RulesInSlice: int32
      CandidatePairs: int32
      PrunedPairs: int32
      WorkUnits: int32
      ExpensiveAnalyses: int32
      CacheHits: int32 }

type CoherenceCacheEntry =
    { Key: string
      Findings: CoherenceFinding list
      CandidatePairs: int32
      PrunedPairs: int32 }

type CoherenceReport =
    { ReportSchemaVersion: int32
      AnalyzerVersion: string
      Mode: CoherenceMode
      PackageManifestDigest: byte array
      AnalyzedRuleIds: RuleId list
      Findings: CoherenceFinding list
      PendingShards: string list
      Termination: AnalysisTermination
      CanonicalizationReady: bool
      Cost: CoherenceCost
      CacheEntry: CoherenceCacheEntry option }

[<RequireQualifiedAccess>]
module RuleCoherence =
    let private analyzerVersion = "sir-rule-coherence/1"
    let defaultBounds = { MaxWorkUnits = 100_000; MaxFindings = 256; MaxWitnessRules = 8 }

    let private hex (bytes: byte array) = bytes |> Array.map (fun value -> value.ToString("x2")) |> String.concat ""
    let private digest (text: string) = text |> Encoding.UTF8.GetBytes |> CanonicalHash.sha256 |> hex
    let private id rule = RuleId.value rule.Metadata.Id
    let private ids rules = rules |> List.map id |> List.sort
    let private distinctSorted values = values |> Set.ofList |> Set.toList
    let private intersects left right = not (Set.intersect (Set.ofList left) (Set.ofList right) |> Set.isEmpty)

    let private frame (value: string) = string (Encoding.UTF8.GetBytes(value).Length) + ":" + value
    let private framedList values = string (List.length values) + ":" + (values |> List.map frame |> String.concat "")
    let private present value = if String.IsNullOrWhiteSpace value then "0" else "1"

    let private statusName = function
        | Proposed -> "proposed"
        | Prototype -> "prototype"
        | Canonical -> "canonical"
        | Deprecated -> "deprecated"
        | Superseded -> "superseded"

    // Rules.packageIdentity deliberately excludes authoring metadata from its semantic digest.
    // The coherence cache has a narrower boundary: retain documentation-only reuse, but bind every
    // metadata field the analyzer reads or reports so a warm result cannot hide a fresh diagnosis.
    let private coherenceMetadataDigest rules =
        rules
        |> List.sortBy id
        |> List.map (fun rule ->
            let metadata = rule.Metadata
            let source =
                match metadata.RuleSource with
                | None -> [ "none" ]
                | Some value -> [ "some"; value.Symbol; value.RepositoryPath; value.Commit ]
            String.concat "|"
                [ frame (id rule)
                  frame (statusName metadata.Status)
                  metadata.Dependencies |> List.map RuleId.value |> List.sort |> framedList
                  metadata.Supersedes |> List.map RuleId.value |> List.sort |> framedList
                  source |> List.map frame |> String.concat ""
                  metadata.Evidence |> framedList
                  frame (present metadata.Title)
                  frame (present metadata.Rationale)
                  frame (if List.isEmpty metadata.Examples then "0" else "1")
                  frame (if List.isEmpty metadata.Properties then "0" else "1") ])
        |> framedList
        |> digest

    let private strengthName = function
        | ClaimStrength.ProvedStructural -> "proved-structural" | ClaimStrength.ProvedFragment -> "proved-fragment"
        | ClaimStrength.ExhaustiveBounded -> "exhaustive-bounded" | ClaimStrength.Tested -> "tested" | ClaimStrength.Heuristic -> "heuristic"
        | ClaimStrength.Unknown -> "unknown" | ClaimStrength.Failed -> "failed"

    let private modeName = function CoherenceMode.Changed -> "changed" | CoherenceMode.Cone -> "cone" | CoherenceMode.Corpus -> "corpus"
    let private terminationName = function AnalysisTermination.Complete -> "complete" | AnalysisTermination.WorkBudgetExhausted -> "work-budget-exhausted"

    let private finding dimension strength ruleIds message reason fact expected actual =
        let sortedIds = ruleIds |> List.sortBy RuleId.value
        let material = String.concat "|" [ dimension; strengthName strength; sortedIds |> List.map RuleId.value |> String.concat ","; message; reason; fact; expected; actual ]
        let witness =
            if String.IsNullOrEmpty fact then None
            else Some { RuleIds = sortedIds; Fact = fact; Expected = expected; Actual = actual }
        { Fingerprint = digest material; Dimension = dimension; Strength = strength; RuleIds = sortedIds; Message = message; DependencyReason = reason; Witness = witness }

    let private directDependencies (rule: RuleDefinition) = rule.Metadata.Dependencies |> List.map RuleId.value |> Set.ofList

    let private dependencyClosure (byId: Map<string, RuleDefinition>) seeds reverse =
        let edges =
            if reverse then
                byId
                |> Map.toList
                |> List.collect (fun (owner, rule) -> rule.Metadata.Dependencies |> List.map (fun dependency -> RuleId.value dependency, owner))
                |> List.groupBy fst
                |> List.map (fun (key, values) -> key, (values |> List.map snd |> Set.ofList))
                |> Map.ofList
            else byId |> Map.map (fun _ rule -> directDependencies rule)
        let rec walk visited frontier =
            match frontier with
            | [] -> visited
            | current :: rest when Set.contains current visited -> walk visited rest
            | current :: rest ->
                let next = Map.tryFind current edges |> Option.defaultValue Set.empty |> Set.toList
                walk (Set.add current visited) (next @ rest)
        walk Set.empty seeds

    let private selectSlice mode changed (byId: Map<string, RuleDefinition>) =
        match mode with
        | CoherenceMode.Corpus -> byId |> Map.toList |> List.map fst |> Set.ofList
        | CoherenceMode.Changed -> changed |> List.map RuleId.value |> Set.ofList
        | CoherenceMode.Cone ->
            let seed = changed |> List.map RuleId.value
            Set.union (dependencyClosure byId seed false) (dependencyClosure byId seed true)

    let private formulaFacts ruleId expression =
        let rec infer = function
            | Constant value -> Ok(value.DataKind, value.Unit)
            | Input(_, kind, unitName) -> Ok(kind, unitName)
            | Add(left, right) | Subtract(left, right) | MinimumOf(left, right) | MaximumOf(left, right) ->
                match infer left, infer right with
                | Ok leftType, Ok rightType when leftType = rightType -> Ok leftType
                | Ok leftType, Ok rightType -> Error(sprintf "incompatible operands %A and %A" leftType rightType)
                | Error error, _ | _, Error error -> Error error
            | Multiply(left, right) ->
                match infer left, infer right with
                | Ok((leftKind, leftUnit) as leftType), Ok((rightKind, rightUnit) as rightType) ->
                    if rightUnit = "ratio" then Ok leftType elif leftUnit = "ratio" then Ok rightType
                    else Error(sprintf "multiplication requires one ratio operand, got %A and %A" (leftKind, leftUnit) (rightKind, rightUnit))
                | Error error, _ | _, Error error -> Error error
            | Divide(left, right) ->
                match infer left, infer right with
                | Ok((RuleValueKind.FixedPoint, _) as leftType), Ok rightType when leftType = rightType -> Ok(RuleValueKind.FixedPoint, "ratio")
                | Ok leftType, Ok rightType -> Error(sprintf "division requires like fixed-point operands, got %A and %A" leftType rightType)
                | Error error, _ | _, Error error -> Error error
            | Clamp(minimum, maximum, value) ->
                match infer minimum, infer maximum, infer value with
                | Ok first, Ok second, Ok third when first = second && second = third -> Ok third
                | first, second, third -> Error(sprintf "clamp operands disagree: %A %A %A" first second third)
            | LessThanOrEqual(left, right) ->
                match infer left, infer right with
                | Ok leftType, Ok rightType when leftType = rightType -> Ok(RuleValueKind.Boolean, "boolean")
                | Ok leftType, Ok rightType -> Error(sprintf "comparison operands disagree: %A and %A" leftType rightType)
                | Error error, _ | _, Error error -> Error error
            | IfThenElse(condition, whenTrue, whenFalse) ->
                match infer condition, infer whenTrue, infer whenFalse with
                | Ok(RuleValueKind.Boolean, _), Ok leftType, Ok rightType when leftType = rightType -> Ok leftType
                | conditionType, leftType, rightType -> Error(sprintf "conditional operands disagree: %A %A %A" conditionType leftType rightType)
        match infer expression with
        | Ok _ -> []
        | Error detail -> [ finding "types-units" ClaimStrength.Failed [ ruleId ] ("Formula is not type/unit coherent: " + detail) "formula inference" "formula" "compatible typed units" detail ]

    let private transition rule = match rule.Semantics with TransitionSemantics contract -> Some contract | _ -> None

    let private pairFacts (left: RuleDefinition) (right: RuleDefinition) =
        match transition left, transition right with
        | Some a, Some b when a.Phase = b.Phase ->
            let sharedWrites = Set.intersect (Set.ofList a.Effects) (Set.ofList b.Effects) |> Set.toList |> List.sort
            let sharedEvents = Set.intersect (Set.ofList a.Events) (Set.ofList b.Events) |> Set.toList |> List.sort
            let connected = intersects a.Effects b.Reads || intersects b.Effects a.Reads || intersects a.Events b.Events || not (List.isEmpty sharedWrites)
            if not connected then None
            else
                let leftId, rightId = id left, id right
                let ordered = Set.contains rightId (directDependencies left) || Set.contains leftId (directDependencies right)
                let conflict = not ordered && not (List.isEmpty sharedWrites) && not (List.isEmpty sharedEvents)
                Some(sharedWrites, sharedEvents, ordered, conflict)
        | _ -> None

    let private requestKey (identity: RulePackageIdentity) (request: CoherenceRequest) selected rules =
        let analyzedSemanticDigest = (Rules.packageIdentity "coherence" "coherence" "1" identity.SourceCommit [] rules).SemanticDigest |> hex
        let blockUnknowns = if request.BlockUnknowns then "1" else "0"
        let requested = request.ChangedRuleIds |> List.map RuleId.value |> List.sort |> String.concat ","
        let packageSourceIdentity = String.concat ":" [ string identity.SchemaVersion; identity.EngineIdentity; identity.CompatibilityProfile; identity.PackageVersion; identity.SourceCommit ]
        let metadataDigest = coherenceMetadataDigest rules
        let requestText = String.concat "|" [ analyzerVersion; modeName request.Mode; string request.Bounds.MaxWorkUnits; string request.Bounds.MaxFindings; string request.Bounds.MaxWitnessRules; blockUnknowns; packageSourceIdentity; analyzedSemanticDigest; metadataDigest; requested; selected |> Set.toList |> List.sort |> String.concat "," ]
        digest requestText

    let analyze (packageIdentity: RulePackageIdentity) (rules: RuleDefinition list) (priorCache: CoherenceCacheEntry option) (request: CoherenceRequest) =
        let sorted = rules |> List.sortBy id
        let byId = sorted |> List.map (fun rule -> id rule, rule) |> Map.ofList
        let selectedIds = selectSlice request.Mode request.ChangedRuleIds byId
        let slice = sorted |> List.filter (id >> selectedIds.Contains)
        let cacheKey = requestKey packageIdentity request selectedIds slice
        match priorCache with
        | Some cached when cached.Key = cacheKey ->
            let hasFailure = cached.Findings |> List.exists (fun item -> item.Strength = ClaimStrength.Failed)
            let hasBlockingUnknown = request.BlockUnknowns && (cached.Findings |> List.exists (fun item -> item.Strength = ClaimStrength.Unknown))
            { ReportSchemaVersion = 1; AnalyzerVersion = analyzerVersion; Mode = request.Mode; PackageManifestDigest = packageIdentity.ManifestDigest; AnalyzedRuleIds = slice |> List.map (fun rule -> rule.Metadata.Id); Findings = cached.Findings; PendingShards = []; Termination = AnalysisTermination.Complete; CanonicalizationReady = not hasFailure && not hasBlockingUnknown; Cost = { RulesInCorpus = int32 sorted.Length; RulesInSlice = int32 slice.Length; CandidatePairs = cached.CandidatePairs; PrunedPairs = cached.PrunedPairs; WorkUnits = 0; ExpensiveAnalyses = 0; CacheHits = 1 }; CacheEntry = Some cached }
        | _ ->
            let mutable work = 0
            let mutable exhausted = false
            let spend amount = if exhausted || work + amount > int request.Bounds.MaxWorkUnits then exhausted <- true; false else work <- work + amount; true
            let mutable findings = []
            let add values = findings <- values @ findings
            let allIds = sorted |> List.map id |> Set.ofList
            let requestedIds = request.ChangedRuleIds |> List.map RuleId.value |> distinctSorted
            for missingId in requestedIds |> List.filter (allIds.Contains >> not) do
                match RuleId.create missingId with
                | Ok missing -> add [ finding "references" ClaimStrength.Failed [ missing ] "Requested changed rule does not resolve." "analysis slice seed" "changedRuleId" "registered rule id" missingId ]
                | Error _ -> ()
            if request.Mode = CoherenceMode.Corpus && not requestedIds.IsEmpty then
                add [ finding "scope" ClaimStrength.Failed request.ChangedRuleIds "Corpus mode does not accept changed-rule seeds." "analysis request" "changedRuleIds" "empty for corpus mode" (String.concat "," requestedIds) ]
            let duplicates = sorted |> List.countBy id |> List.filter (fun (_, count) -> count > 1)
            for duplicateId, count in duplicates do
                match RuleId.create duplicateId with
                | Ok duplicate -> add [ finding "identity" ClaimStrength.Failed [ duplicate ] "Rule identity is registered more than once." "registry identity index" "count" "1" (string count) ]
                | Error _ -> ()
            for rule in slice do
                if spend 1 then
                    let ruleId = rule.Metadata.Id
                    let expectedKind =
                        match rule.Semantics with FactSemantics _ -> RuleKind.Fact | PredicateSemantics _ -> RuleKind.Predicate | FormulaSemantics _ -> RuleKind.Formula | TransitionSemantics _ -> RuleKind.Transition | AlgorithmSemantics _ -> RuleKind.Algorithm | NarrativeSemantics -> RuleKind.Narrative
                    if rule.Metadata.SemanticKind <> expectedKind then add [ finding "identity" ClaimStrength.Failed [ ruleId ] "Declared semantic kind does not match executable semantics." "typed registry" "kind" (sprintf "%A" expectedKind) (sprintf "%A" rule.Metadata.SemanticKind) ]
                    if String.IsNullOrWhiteSpace rule.Metadata.Title || (rule.Metadata.SemanticKind <> Narrative && (String.IsNullOrWhiteSpace rule.Metadata.Rationale || List.isEmpty rule.Metadata.Examples || List.isEmpty rule.Metadata.Properties || List.isEmpty rule.Metadata.Evidence)) then add [ finding "coverage" ClaimStrength.Failed [ ruleId ] "Executable rule metadata is incomplete." "canonicalization metadata policy" "metadata" "title, rationale, examples, properties, evidence" "missing" ]
                    match rule.Metadata.RuleSource with
                    | Some source when source.Commit <> packageIdentity.SourceCommit -> add [ finding "history" ClaimStrength.Failed [ ruleId ] "Rule source commit does not match the analyzed package source identity." "source/package binding" "sourceCommit" packageIdentity.SourceCommit source.Commit ]
                    | None when rule.Metadata.SemanticKind <> Narrative -> add [ finding "references" ClaimStrength.Failed [ ruleId ] "Executable rule has no source binding." "source mapping" "source" "repository path, symbol, commit" "missing" ]
                    | _ -> ()
                    if rule.Metadata.Status = Superseded && List.isEmpty rule.Metadata.Supersedes then add [ finding "history" ClaimStrength.Failed [ ruleId ] "Superseded rule names no replacement history." "status/supersession relation" "supersedes" "at least one registered rule" "empty" ]
                    for dependency in rule.Metadata.Dependencies do
                        if not (Set.contains (RuleId.value dependency) allIds) then add [ finding "references" ClaimStrength.Failed [ ruleId; dependency ] "Rule dependency does not resolve." "declared dependency" "dependency" "registered rule id" (RuleId.value dependency) ]
                    if rule.Metadata.Status = Canonical then
                        for dependency in rule.Metadata.Dependencies do
                            match Map.tryFind (RuleId.value dependency) byId with
                            | Some target when target.Metadata.Status = Proposed || target.Metadata.Status = Prototype -> add [ finding "dependency-status" ClaimStrength.Failed [ ruleId; dependency ] "Canonical rule depends on non-canonical authority." "declared dependency" "status" "canonical/deprecated/superseded dependency" (sprintf "%A" target.Metadata.Status) ]
                            | _ -> ()
                    match rule.Semantics with
                    | FormulaSemantics(_, _, expression) | PredicateSemantics expression -> add (formulaFacts ruleId expression)
                    | AlgorithmSemantics contract when String.IsNullOrWhiteSpace contract.Fingerprint -> add [ finding "history" ClaimStrength.Failed [ ruleId ] "Registered algorithm has no implementation fingerprint." "algorithm contract" "fingerprint" "stable non-empty identity" "missing" ]
                    | AlgorithmSemantics _ -> add [ finding "interaction" ClaimStrength.Unknown [ ruleId ] "Opaque algorithm has no trusted read/write/event footprint for cross-rule pruning." "opaque implementation boundary" "footprint" "verified assume/guarantee summary" "unknown" ]
                    | TransitionSemantics contract when String.IsNullOrWhiteSpace contract.Phase || (List.isEmpty contract.Reads && List.isEmpty contract.Effects && List.isEmpty contract.Events) -> add [ finding "temporal" ClaimStrength.Unknown [ ruleId ] "Transition has an incomplete interaction footprint." "transition contract" "footprint" "phase plus reads/effects/events" "incomplete" ]
                    | TransitionSemantics contract ->
                        for precondition in contract.Preconditions do
                            let preconditionId = RuleId.value precondition
                            if not (rule.Metadata.Dependencies |> List.exists (fun dependency -> RuleId.value dependency = preconditionId)) then add [ finding "dependency-structure" ClaimStrength.Failed [ ruleId; precondition ] "Transition uses an undeclared precondition dependency." "transition precondition index" "dependency" "precondition listed in metadata dependencies" preconditionId ]
                            match Map.tryFind preconditionId byId with
                            | None -> add [ finding "references" ClaimStrength.Failed [ ruleId; precondition ] "Transition precondition does not resolve." "transition precondition index" "precondition" "registered predicate rule" preconditionId ]
                            | Some { Semantics = PredicateSemantics(Constant { DataKind = RuleValueKind.Boolean; Value = BooleanValue false }) } -> add [ finding "reachability" ClaimStrength.Failed [ ruleId; precondition ] "Transition is unreachable because a declared precondition is always false." "bounded constant-predicate evaluation" "precondition" "satisfiable predicate" "false" ]
                            | _ -> ()
                    | _ -> ()
                    let cyclic = rule.Metadata.Dependencies |> List.exists (fun dependency -> dependencyClosure byId [ RuleId.value dependency ] false |> Set.contains (id rule))
                    if cyclic then add [ finding "dependency-structure" ClaimStrength.Failed [ ruleId ] "Rule dependency graph contains a cycle." "transitive dependency index" "cycle" "acyclic dependency path" (id rule) ]
            let transitions = slice |> List.choose (fun rule -> transition rule |> Option.map (fun contract -> id rule, rule, contract))
            let grouped selector =
                transitions
                |> List.collect (fun (ruleId, _, contract) -> selector contract |> List.distinct |> List.map (fun fact -> contract.Phase + "|" + fact, ruleId))
                |> List.groupBy fst
                |> List.map (fun (key, values) -> key, (values |> List.map snd |> Set.ofList))
                |> Map.ofList
            let writes = grouped (fun contract -> contract.Effects)
            let reads = grouped (fun contract -> contract.Reads)
            let events = grouped (fun contract -> contract.Events)
            let within values = [ for left in values do for right in values do if left < right then yield left, right ]
            let across left right = [ for a in left do for b in right do if a <> b then yield if a < b then a, b else b, a ]
            let candidateIds =
                [ for KeyValue(_, values) in writes do yield! within (Set.toList values)
                  for KeyValue(key, values) in events do yield! within (Set.toList values)
                  for KeyValue(key, writeOwners) in writes do
                      match Map.tryFind key reads with
                      | Some readOwners -> yield! across (Set.toList writeOwners) (Set.toList readOwners)
                      | None -> () ]
                |> Set.ofList
                |> Set.toList
                |> List.sort
            let pairs = candidateIds |> List.choose (fun (left, right) -> match Map.tryFind left byId, Map.tryFind right byId with Some a, Some b -> Some(a, b) | _ -> None)
            let mutable candidates = 0
            let totalPairs = slice.Length * (slice.Length - 1) / 2
            let mutable pruned = max 0 (totalPairs - pairs.Length)
            for left, right in pairs do
                if spend 1 then
                    match pairFacts left right with
                    | None -> ()
                    | Some(sharedWrites, sharedEvents, _, conflict) ->
                        candidates <- candidates + 1
                        if conflict then
                            let fact = "writes=" + String.concat "," sharedWrites + ";events=" + String.concat "," sharedEvents
                            add [ finding "logical-compatibility" ClaimStrength.Failed [ left.Metadata.Id; right.Metadata.Id ] "Unordered same-phase transitions share authoritative writes and events." "typed phase/read-write/event candidate" fact "declared dependency or disjoint/commutative contract" "ambiguous ordering" ]
            let allOrderedFindings = findings |> List.sortBy (fun item -> item.Dimension, item.Fingerprint)
            let findingLimit = max 0 (int request.Bounds.MaxFindings)
            let findingsTruncated = allOrderedFindings.Length > findingLimit
            let boundWitness finding =
                { finding with
                    Witness = finding.Witness |> Option.map (fun witness -> { witness with RuleIds = witness.RuleIds |> List.truncate (max 0 (int request.Bounds.MaxWitnessRules)) }) }
            let orderedFindings = allOrderedFindings |> List.truncate findingLimit |> List.map boundWitness
            let incomplete = exhausted || findingsTruncated
            let pending =
                [ if exhausted then "remaining-structural-or-interaction-work"
                  if findingsTruncated then "finding-output-truncated" ]
            let termination = if incomplete then AnalysisTermination.WorkBudgetExhausted else AnalysisTermination.Complete
            let hasFailure = allOrderedFindings |> List.exists (fun item -> item.Strength = ClaimStrength.Failed)
            let hasUnknown = allOrderedFindings |> List.exists (fun item -> item.Strength = ClaimStrength.Unknown)
            let cache = if incomplete then None else Some { Key = cacheKey; Findings = orderedFindings; CandidatePairs = int32 candidates; PrunedPairs = int32 pruned }
            { ReportSchemaVersion = 1; AnalyzerVersion = analyzerVersion; Mode = request.Mode; PackageManifestDigest = packageIdentity.ManifestDigest; AnalyzedRuleIds = slice |> List.map (fun rule -> rule.Metadata.Id); Findings = orderedFindings; PendingShards = pending; Termination = termination; CanonicalizationReady = not incomplete && not hasFailure && not (request.BlockUnknowns && hasUnknown); Cost = { RulesInCorpus = int32 sorted.Length; RulesInSlice = int32 slice.Length; CandidatePairs = int32 candidates; PrunedPairs = int32 pruned; WorkUnits = int32 work; ExpensiveAnalyses = int32 candidates; CacheHits = 0 }; CacheEntry = cache }

    let private escape (value: string) = value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n")
    let private quote value = "\"" + escape value + "\""
    let private jsonArray values = "[" + String.concat "," values + "]"
    let private ruleIdsJson values = values |> List.map (RuleId.value >> quote) |> jsonArray
    let private witnessJson (witness: CoherenceWitness option) =
        match witness with
        | None -> "null"
        | Some value -> "{" + String.concat "," [ "\"ruleIds\":" + ruleIdsJson value.RuleIds; "\"fact\":" + quote value.Fact; "\"expected\":" + quote value.Expected; "\"actual\":" + quote value.Actual ] + "}"
    let private findingJson (value: CoherenceFinding) =
        "{" + String.concat "," [ "\"fingerprint\":" + quote value.Fingerprint; "\"dimension\":" + quote value.Dimension; "\"strength\":" + quote (strengthName value.Strength); "\"ruleIds\":" + ruleIdsJson value.RuleIds; "\"message\":" + quote value.Message; "\"dependencyReason\":" + quote value.DependencyReason; "\"witness\":" + witnessJson value.Witness ] + "}"
    let private cacheJson (cache: CoherenceCacheEntry option) =
        match cache with
        | None -> "null"
        | Some value -> "{" + String.concat "," [ "\"key\":" + quote value.Key; "\"candidatePairs\":" + string value.CandidatePairs; "\"prunedPairs\":" + string value.PrunedPairs ] + "}"
    let reportJson (report: CoherenceReport) =
        let cost = report.Cost
        "{" + String.concat "," [ "\"schemaVersion\":" + string report.ReportSchemaVersion; "\"analyzerVersion\":" + quote report.AnalyzerVersion; "\"mode\":" + quote (modeName report.Mode); "\"packageManifestDigest\":" + quote (hex report.PackageManifestDigest); "\"analyzedRuleIds\":" + ruleIdsJson report.AnalyzedRuleIds; "\"findings\":" + (report.Findings |> List.map findingJson |> jsonArray); "\"pendingShards\":" + (report.PendingShards |> List.map quote |> jsonArray); "\"termination\":" + quote (terminationName report.Termination); "\"canonicalizationReady\":" + (if report.CanonicalizationReady then "true" else "false"); "\"cost\":{" + String.concat "," [ "\"rulesInCorpus\":" + string cost.RulesInCorpus; "\"rulesInSlice\":" + string cost.RulesInSlice; "\"candidatePairs\":" + string cost.CandidatePairs; "\"prunedPairs\":" + string cost.PrunedPairs; "\"workUnits\":" + string cost.WorkUnits; "\"expensiveAnalyses\":" + string cost.ExpensiveAnalyses; "\"cacheHits\":" + string cost.CacheHits ] + "}"; "\"cache\":" + cacheJson report.CacheEntry ] + "}\n"
    let canonicalReportBytes report = reportJson report |> Encoding.UTF8.GetBytes
