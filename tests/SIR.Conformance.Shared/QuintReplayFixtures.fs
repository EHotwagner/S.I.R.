namespace SIR.Conformance

open System
open System.IO
open System.Security.Cryptography
open System.Text.Json
open System.Text.Json.Nodes
open FS.GG.Game.Core
open SIR.Domain
open SIR.Simulation

type QuintReplayMutation =
    | WrongActionMapping
    | OmittedAction
    | WrongObservableField
    | StaleExpectedState
    | CombatBoundaryBypass

type QuintReplayDivergence =
    { Transition: int
      Action: string
      Field: string
      Expected: string
      Actual: string
      FixturePath: string
      JsonPointer: string
      AdapterSource: string
      ImplementationSource: string }

type private ModelObservation =
    { HitPoints: int32
      LastAction: string
      LastAmount: int32 }

[<RequireQualifiedAccess>]
module QuintReplayFixtures =
    let private fixtureRoot =
        Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", "fixtures", "rules-corpus", "quint-q1"))

    let private witnessPath = Path.Combine(fixtureRoot, "sir-reviewed-witness.json")
    let private itfPath = Path.Combine(fixtureRoot, "sir-reviewed-witness.itf.json")
    let private manifestPath = Path.Combine(fixtureRoot, "producer-candidate-manifest.json")
    let private candidateReceiptPath = Path.Combine(fixtureRoot, "producer-candidate-receipt.json")
    let private adapterPath = Path.Combine(__SOURCE_DIRECTORY__, "QuintReplayFixtures.fs")
    let private implementationPath =
        Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "src", "SIR.Simulation", "CombatRules.fs"))
    let private fixtureRelativePath = "tests/fixtures/rules-corpus/quint-q1/sir-reviewed-witness.itf.json"
    let private adapterSource = "tests/SIR.Conformance.Shared/QuintReplayFixtures.fs:applyDamageWith"
    let private implementationSource = "src/SIR.Simulation/CombatRules.fs:CombatRules.resolveConsequences"

    let private sha256 path =
        path
        |> File.ReadAllBytes
        |> SHA256.HashData
        |> Convert.ToHexString
        |> fun value -> value.ToLowerInvariant()

    let private required = function
        | Ok value -> value
        | Error error -> failwithf "Quint replay adapter input was invalid: %A" error

    let private text (element: JsonElement) =
        element.GetString()
        |> Option.ofObj
        |> Option.defaultWith (fun () -> failwith "Q1 replay JSON contained a null string.")

    let private cell col row: Cell = { Col = col; Row = row }

    // This is the only product transition adapter. It delegates damage and saturation to the real
    // interpreter; it contains no copied health-subtraction or clamp expression.
    let private applyDamageWith resolveConsequences amount current =
        let input =
            { Attacker = cell 0 0
              TargetFootprint = [ cell 1 0 ]
              VisibleSamples = 1
              TotalSamples = 1
              RangeCells = 1
              Suppression = FixedPoint.zero
              BaseDamage = FixedPoint.fromRatio amount 1 |> required
              ArmorRetention = FixedPoint.fromRatio 1 1 |> required
              EventId = $"quint-q1-damage-{amount}" }

        resolveConsequences current.HitPoints 0 0 input
        |> required
        |> fun outcome ->
            { HitPoints = outcome.RemainingHealth
              LastAction = "ApplyDamage"
              LastAmount = amount }

    let private applyDamage amount current =
        applyDamageWith CombatRules.resolveConsequences amount current

    // Independent implementation-seam mutant: the real interpreter is called first, then its
    // boundary result is corrupted before the adapter projects it. No damage/clamp expression is
    // copied into the test, and a post-projection mutation cannot satisfy this case.
    let private applyDamageWithBoundaryDefect amount current =
        let defectResolver health suppression delta input =
            CombatRules.resolveConsequences health suppression delta input
            |> Result.map (fun outcome -> { outcome with RemainingHealth = -1 })

        applyDamageWith defectResolver amount current

    let private initialize () =
        { HitPoints = 10
          LastAction = "Initialize"
          LastAmount = 0 }

    let private bigint (element: JsonElement) =
        element.GetProperty("#bigint") |> text |> Int32.Parse

    let private observationOfExpected (element: JsonElement) =
        let expected = element.GetProperty("expected")
        { HitPoints = expected.GetProperty("hitPoints").GetInt32()
          LastAction = expected.GetProperty("lastAction") |> text
          LastAmount = expected.GetProperty("lastAmount").GetInt32() }

    let private observationOfItf (element: JsonElement) =
        { HitPoints = bigint (element.GetProperty("hitPoints"))
          LastAction = element.GetProperty("lastAction") |> text
          LastAmount = bigint (element.GetProperty("lastAmount")) }

    let private firstField fixturePath transition action expected actual =
        let mismatch field expectedValue actualValue =
            Some
                { Transition = transition
                  Action = action
                  Field = field
                  Expected = expectedValue
                  Actual = actualValue
                  FixturePath = fixturePath
                  JsonPointer = $"/states/{transition}/{field}"
                  AdapterSource = adapterSource
                  ImplementationSource = implementationSource }

        if expected.HitPoints <> actual.HitPoints then
            mismatch "hitPoints" (string expected.HitPoints) (string actual.HitPoints)
        elif expected.LastAction <> actual.LastAction then
            mismatch "lastAction" expected.LastAction actual.LastAction
        elif expected.LastAmount <> actual.LastAmount then
            mismatch "lastAmount" (string expected.LastAmount) (string actual.LastAmount)
        else
            None

    let private verifyProducerBindings () =
        use witness = JsonDocument.Parse(File.ReadAllBytes witnessPath)
        use manifest = JsonDocument.Parse(File.ReadAllBytes manifestPath)
        use candidate = JsonDocument.Parse(File.ReadAllBytes candidateReceiptPath)
        let witnessRoot = witness.RootElement
        let manifestRoot = manifest.RootElement
        let candidateRoot = candidate.RootElement
        let fingerprints = witnessRoot.GetProperty("fingerprints")
        let documents = manifestRoot.GetProperty("documents")
        let sirSlice =
            manifestRoot.GetProperty("slices").EnumerateArray()
            |> Seq.find (fun item -> item.GetProperty("id") |> text = "sir-damage-rule")

        let requireEqual label expected actual =
            if expected <> actual then
                failwithf "Q1 producer binding mismatch: %s expected=%s actual=%s" label expected actual

        requireEqual "candidateCommit" "3a0eced13305b146df2febd96698e38335cae99c" (candidateRoot.GetProperty("candidateCommit") |> text)
        requireEqual "manifestSha256" (candidateRoot.GetProperty("manifestSha256") |> text) (sha256 manifestPath)
        requireEqual "literateSource" ("sha256:" + (sirSlice.GetProperty("sourceSha256") |> text)) (fingerprints.GetProperty("literateSource") |> text)
        requireEqual "generatedModule" ("sha256:" + (sirSlice.GetProperty("moduleSha256") |> text)) (fingerprints.GetProperty("generatedModule") |> text)
        requireEqual "profileDocument" ("sha256:" + (documents.GetProperty("profileSha256") |> text)) (fingerprints.GetProperty("profileDocument") |> text)
        requireEqual "compiledContractSchema" ("sha256:" + (documents.GetProperty("contractSchemaSha256") |> text)) (fingerprints.GetProperty("compiledContractSchema") |> text)
        requireEqual "compiledContractExample" ("sha256:" + (documents.GetProperty("contractExampleSha256") |> text)) (fingerprints.GetProperty("compiledContractExample") |> text)
        requireEqual "normalizedItf" ("sha256:" + sha256 itfPath) (fingerprints.GetProperty("normalizedItf") |> text)
        requireEqual "seed" "92220" (witnessRoot.GetProperty("seed") |> text)
        requireEqual "transitionBound" "2" (string (witnessRoot.GetProperty("bounds").GetProperty("transitions").GetInt32()))

    let replay mutation =
        verifyProducerBindings ()
        use witness = JsonDocument.Parse(File.ReadAllBytes witnessPath)
        use itf = JsonDocument.Parse(File.ReadAllBytes itfPath)
        let steps = witness.RootElement.GetProperty("steps").EnumerateArray() |> Seq.toArray
        let states = itf.RootElement.GetProperty("states").EnumerateArray() |> Seq.toArray

        if steps.Length <> states.Length then
            failwithf "Q1 trace length mismatch: envelope=%d itf=%d" steps.Length states.Length

        let mutable current = initialize ()
        let mutable divergence = None

        for index in 0 .. steps.Length - 1 do
            if divergence.IsNone then
                let step = steps[index]
                let action = step.GetProperty("action") |> text
                let envelopeExpected = observationOfExpected step
                let itfExpected = observationOfItf states[index]

                match firstField fixtureRelativePath index action envelopeExpected itfExpected with
                | Some mismatch -> failwithf "Q1 envelope/ITF disagreement before runtime replay: %A" mismatch
                | None -> ()

                current <-
                    match action, index, mutation with
                    | "Initialize", 0, _ -> initialize ()
                    | "ApplyDamage", _, Some OmittedAction -> current
                    | "ApplyDamage", _, Some WrongActionMapping ->
                        let amount = step.GetProperty("arguments").GetProperty("amount").GetInt32()
                        applyDamage (amount + 1) current
                    | "ApplyDamage", _, Some CombatBoundaryBypass ->
                        let amount = step.GetProperty("arguments").GetProperty("amount").GetInt32()
                        applyDamageWithBoundaryDefect amount current
                    | "ApplyDamage", _, _ ->
                        let amount = step.GetProperty("arguments").GetProperty("amount").GetInt32()
                        applyDamage amount current
                    | unknown, _, _ -> failwithf "Unknown Q1 action identity at transition %d: %s" index unknown

                let actual =
                    match mutation with
                    | Some WrongObservableField -> { current with LastAmount = current.LastAmount + 1 }
                    | _ -> current

                let expected =
                    match mutation, index with
                    | Some StaleExpectedState, 1 -> { envelopeExpected with HitPoints = envelopeExpected.HitPoints + 1 }
                    | _ -> envelopeExpected

                divergence <- firstField fixtureRelativePath index action expected actual

        divergence

    let verifyExact () =
        match replay None with
        | None -> ()
        | Some divergence -> failwithf "Q1 exact replay diverged: %A" divergence

    let verifyMutation mutation =
        match replay (Some mutation) with
        | Some divergence -> divergence
        | None -> failwithf "Q1 mutation was accepted: %A" mutation

    let verifySampledCorpus directory expectedTraceCount =
        verifyProducerBindings ()
        let paths = Directory.GetFiles(directory, "sample_*.itf.json") |> Array.sort

        if paths.Length <> expectedTraceCount then
            failwithf "Q1 sampled corpus count mismatch: expected=%d actual=%d" expectedTraceCount paths.Length

        let mutable totalStates = 0

        for path in paths do
            use document = JsonDocument.Parse(File.ReadAllBytes path)
            let root = document.RootElement
            let status = root.GetProperty("#meta").GetProperty("status") |> text
            if status <> "ok" then failwithf "Q1 sampled ITF status was not ok: %s" path

            let variables = root.GetProperty("vars").EnumerateArray() |> Seq.map text |> Seq.toArray
            if variables <> [| "hitPoints"; "lastAction"; "lastAmount" |] then
                failwithf "Q1 sampled ITF variable projection drifted: %s" path

            let states = root.GetProperty("states").EnumerateArray() |> Seq.toArray
            if states.Length < 2 then failwithf "Q1 sampled ITF was too short: %s" path
            totalStates <- totalStates + states.Length
            let mutable current = initialize ()

            for index in 0 .. states.Length - 1 do
                let expected = observationOfItf states[index]
                current <-
                    if index = 0 then initialize ()
                    elif expected.LastAction = "ApplyDamage" then applyDamage expected.LastAmount current
                    else failwithf "Unknown sampled Q1 action at %s#/states/%d/lastAction" path index

                match firstField path index expected.LastAction expected current with
                | None -> ()
                | Some divergence -> failwithf "Q1 sampled runtime replay diverged: %A" divergence

        paths.Length, totalStates

    let tryParseMutation = function
        | "wrong-action-mapping" -> Some WrongActionMapping
        | "omitted-action" -> Some OmittedAction
        | "wrong-observable-field" -> Some WrongObservableField
        | "stale-expected-state" -> Some StaleExpectedState
        | "combat-boundary-bypass" -> Some CombatBoundaryBypass
        | _ -> None

    let receiptJson () =
        verifyExact ()
        let receipt = JsonObject()
        receipt["schema"] <- "fsgg.quint.sir-runtime-replay-receipt/q1"
        receipt["producerCandidateCommit"] <- "3a0eced13305b146df2febd96698e38335cae99c"
        receipt["producerReceiptHead"] <- "6cf3f1f0746c817e1171cd3a7b63865c25c1e346"
        receipt["producerManifestSha256"] <- sha256 manifestPath
        receipt["traceSha256"] <- sha256 itfPath
        receipt["adapterSha256"] <- sha256 adapterPath
        receipt["implementationSha256"] <- sha256 implementationPath
        receipt["seed"] <- "92220"
        receipt["transitionBound"] <- 2
        receipt["traceCount"] <- 1
        receipt["normalization"] <- "exact int32/string projection; immediate deterministic quiescence"
        receipt["verdict"] <- "accept-exact-runtime-correspondence"
        receipt.ToJsonString(JsonSerializerOptions(WriteIndented = true))
