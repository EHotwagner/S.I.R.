namespace SIR.Client

open SIR.Domain
open SIR.Simulation

type LabParameter =
    { Key: string
      Label: string
      Minimum: int32
      Maximum: int32
      Step: int32
      DefaultValue: int32 }

type DesignScenario =
    { Identity: string
      Revision: int32
      Title: string
      Description: string
      EngineIdentity: string
      RulesetIdentity: string
      Parameters: LabParameter list }

type ExperimentInput =
    { ScenarioIdentity: string
      ScenarioRevision: int32
      EngineIdentity: string
      RulesetIdentity: string
      Parameters: Map<string, int32> }

type ExperimentResult =
    { Input: ExperimentInput
      ResultIdentity: string
      Metrics: Map<string, int32> }

type ExperimentComparison =
    { Baseline: ExperimentResult
      Fork: ExperimentResult
      Delta: Map<string, int32> }

type SweepResult =
    { Parameter: string
      Results: ExperimentResult list }

type LabReport =
    { Comparison: ExperimentComparison
      Sweep: SweepResult option
      EvidenceLabel: string }

/// A structured-clone-safe key/value entry for the browser worker boundary.
type Int32Entry =
    { Key: string
      Value: int32 }

type DesignScenarioTransport =
    { Identity: string
      Revision: int32
      Title: string
      Description: string
      EngineIdentity: string
      RulesetIdentity: string
      Parameters: LabParameter array }

type ExperimentInputTransport =
    { ScenarioIdentity: string
      ScenarioRevision: int32
      EngineIdentity: string
      RulesetIdentity: string
      Parameters: Int32Entry array }

type ExperimentResultTransport =
    { Input: ExperimentInputTransport
      ResultIdentity: string
      Metrics: Int32Entry array }

type SweepResultTransport =
    { Parameter: string
      Results: ExperimentResultTransport array }

type LabReportTransport =
    { Baseline: ExperimentResultTransport
      Fork: ExperimentResultTransport
      Delta: Int32Entry array
      Sweep: SweepResultTransport option
      EvidenceLabel: string }

[<RequireQualifiedAccess>]
module Lab =
    [<Literal>]
    let ExportFormat = "sir-lab-experiment-v1"

    [<Literal>]
    let EvidenceLabel = "Exploratory balance evidence — not accepted balance"

    let private engineIdentity =
        "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20"

    let private rulesetIdentity =
        "6d31302d72756c65732d6c61622d763100000000000000000000000000000000"

    let private attackParameters attackPower attackCount : LabParameter list =
        [ { Key = "attack-power"
            Label = "Attack power"
            Minimum = 1
            Maximum = 100
            Step = 1
            DefaultValue = attackPower }
          { Key = "attack-count"
            Label = "Attack count"
            Minimum = 1
            Maximum = 8
            Step = 1
            DefaultValue = attackCount } ]

    let catalog: DesignScenario list =
        [ { Identity = "adjacent-duel"
            Revision = 1
            Title = "Four-hit baseline"
            Description = "Four standard attacks establish the immutable comparison baseline."
            EngineIdentity = engineIdentity
            RulesetIdentity = rulesetIdentity
            Parameters = attackParameters 25 4 }
          { Identity = "short-duel"
            Revision = 1
            Title = "Two-hit exchange"
            Description = "Two standard attacks show the target surviving at half health."
            EngineIdentity = engineIdentity
            RulesetIdentity = rulesetIdentity
            Parameters = attackParameters 25 2 }
          { Identity = "single-heavy-strike"
            Revision = 1
            Title = "Single heavy strike"
            Description = "One high-power attack makes damage scaling easy to inspect."
            EngineIdentity = engineIdentity
            RulesetIdentity = rulesetIdentity
            Parameters = attackParameters 60 1 }
          { Identity = "rapid-chip-damage"
            Revision = 1
            Title = "Rapid chip damage"
            Description = "Eight low-power attacks expose accumulation without reaching lethal damage."
            EngineIdentity = engineIdentity
            RulesetIdentity = rulesetIdentity
            Parameters = attackParameters 8 8 }
          { Identity = "lethality-threshold"
            Revision = 1
            Title = "Lethality threshold"
            Description = "Three 34-power attacks cross the exact 100-health defeat threshold."
            EngineIdentity = engineIdentity
            RulesetIdentity = rulesetIdentity
            Parameters = attackParameters 34 3 }
          { Identity = "near-threshold"
            Revision = 1
            Title = "Near-threshold survivor"
            Description = "Three 33-power attacks deliberately leave the target on one health."
            EngineIdentity = engineIdentity
            RulesetIdentity = rulesetIdentity
            Parameters = attackParameters 33 3 } ]

    let tryScenario identity =
        catalog |> List.tryFind (fun scenario -> scenario.Identity = identity)

    let defaults (scenario: DesignScenario) =
        scenario.Parameters
        |> List.map (fun parameter -> parameter.Key, parameter.DefaultValue)
        |> Map.ofList

    let validate (scenario: DesignScenario) (patch: Map<string, int32>) =
        let definitions =
            scenario.Parameters
            |> List.map (fun parameter -> parameter.Key, parameter)
            |> Map.ofList

        patch
        |> Map.toList
        |> List.tryPick (fun (key, value) ->
            match Map.tryFind key definitions with
            | None -> Some("Unknown parameter: " + key)
            | Some parameter when value < parameter.Minimum || value > parameter.Maximum ->
                Some(
                    parameter.Label
                    + " must be between "
                    + string parameter.Minimum
                    + " and "
                    + string parameter.Maximum
                    + "."
                )
            | Some parameter when (value - parameter.Minimum) % parameter.Step <> 0 ->
                Some(parameter.Label + " must use step " + string parameter.Step + ".")
            | Some _ -> None)
        |> function
            | Some error -> Error error
            | None -> Ok(Map.fold (fun values key value -> Map.add key value values) (defaults scenario) patch)

    let private required key (parameters: Map<string, int32>) =
        parameters
        |> Map.tryFind key
        |> Option.defaultWith (fun () -> failwith ("Missing laboratory parameter: " + key))

    let private identityOf (input: ExperimentInput) (metrics: Map<string, int32>) =
        [ input.ScenarioIdentity
          string input.ScenarioRevision
          input.EngineIdentity
          input.RulesetIdentity
          input.Parameters
          |> Map.toList
          |> List.map (fun (key, value) -> key + "=" + string value)
          |> String.concat ";"
          metrics
          |> Map.toList
          |> List.map (fun (key, value) -> key + "=" + string value)
          |> String.concat ";" ]
        |> String.concat "|"
        |> System.Text.Encoding.UTF8.GetBytes
        |> CanonicalHash.sha256
        |> Array.take 8
        |> Array.map (fun value -> value.ToString("x2"))
        |> String.concat ""

    let evaluate
        (scenario: DesignScenario)
        (parameters: Map<string, int32>)
        : ExperimentResult
        =
        let attackPower = required "attack-power" parameters
        let attackCount = required "attack-count" parameters

        let boundedAttackPower =
            BoundedInt32.create 0 100 attackPower
            |> Result.defaultWith (fun error ->
                failwith ("Validated attack power could not enter the kernel: " + string error))

        let rules =
            { Simulation.defaultRules with
                AttackPower = boundedAttackPower }

        let attackOnly =
            Simulation.inputs
            |> List.choose (function
                | Attack _ as attack -> Some attack
                | _ -> None)

        let mutable state = Simulation.initialState

        for index in 1 .. attackCount do
            let journal =
                if index = 1 then Simulation.inputs else attackOnly

            state <- (Simulation.runTickWithRules rules state journal).State

        let remainingHealth =
            state.Units
            |> Map.toList
            |> List.pick (fun (_, unit) ->
                match unit.Side with
                | Blue -> Some(BoundedInt32.value unit.Health)
                | Red -> None)

        let totalDamage = 100 - remainingHealth

        let input: ExperimentInput =
            { ScenarioIdentity = scenario.Identity
              ScenarioRevision = scenario.Revision
              EngineIdentity = scenario.EngineIdentity
              RulesetIdentity = scenario.RulesetIdentity
              Parameters = parameters }

        let metrics =
            [ "attack-events", attackCount
              "remaining-health", remainingHealth
              "total-damage", totalDamage ]
            |> Map.ofList

        { Input = input
          ResultIdentity = identityOf input metrics
          Metrics = metrics }

    let run
        (scenario: DesignScenario)
        (patch: Map<string, int32>)
        (sweepParameter: string option)
        =
        match validate scenario patch with
        | Error error -> Error error
        | Ok forkParameters ->
            let baseline = defaults scenario |> evaluate scenario
            let fork = evaluate scenario forkParameters

            let delta =
                baseline.Metrics
                |> Map.map (fun key value -> Map.find key fork.Metrics - value)

            let comparison: ExperimentComparison =
                { Baseline = baseline
                  Fork = fork
                  Delta = delta }

            let sweep: SweepResult option =
                sweepParameter
                |> Option.bind (fun key ->
                    scenario.Parameters
                    |> List.tryFind (fun parameter -> parameter.Key = key)
                    |> Option.map (fun parameter ->
                        let values =
                            [ parameter.Minimum .. parameter.Step .. parameter.Maximum ]

                        { Parameter = key
                          Results =
                            values
                            |> List.map (fun value ->
                                forkParameters
                                |> Map.add key value
                                |> evaluate scenario) }))

            let report: LabReport =
                { Comparison = comparison
                  Sweep = sweep
                  EvidenceLabel = EvidenceLabel }

            Ok report

    let attackFrames (report: LabReport) =
        let parameters = report.Comparison.Fork.Input.Parameters
        let attackPower = required "attack-power" parameters
        let attackCount = required "attack-count" parameters

        [ 0 .. attackCount ]
        |> List.map (fun attack ->
            attack, max 0 (100 - (attack * attackPower)))

    /// Adapts the disclosed deterministic laboratory result to a sandbox frame.
    /// The fixed cells and edge are facts of the canonical minimal-slice scenario.
    let renderFrame (result: ExperimentResult) =
        let extent =
            CellExtent.tryCreate 1
            |> Option.defaultWith (fun () -> failwith "One-cell laboratory extent is invalid.")
        let visualHealth value =
            HealthVisual.tryCreate value 100
            |> Option.defaultWith (fun () -> failwith "Laboratory health is outside its validated range.")
        let remaining = required "remaining-health" result.Metrics
        let attacks = required "attack-events" result.Metrics

        { Tick = attacks
          Board =
            { MinimumColumn = 0
              MinimumRow = 0
              MaximumColumn = 2
              MaximumRow = 1 }
          Units =
            [| { Id = 10
                 AnchorColumn = 1
                 AnchorRow = 1
                 FootprintWidth = extent
                 FootprintDepth = extent
                 ClassId = UnitClassId.placeholder
                 Faction = Arcane
                 Health = Disclosed(visualHealth 100)
                 Level = NotPresent
                 StanceId = NotPresent
                 BodyHeading = NotPresent
                 SecondaryHeading = NotPresent
                 ShortLabel = Disclosed "10"
                 StatusIds = [||] }
               { Id = 20
                 AnchorColumn = 2
                 AnchorRow = 0
                 FootprintWidth = extent
                 FootprintDepth = extent
                 ClassId = UnitClassId.placeholder
                 Faction = Human
                 Health = Disclosed(visualHealth remaining)
                 Level = NotPresent
                 StanceId = NotPresent
                 BodyHeading = NotPresent
                 SecondaryHeading = NotPresent
                 ShortLabel = Disclosed "20"
                 StatusIds = [||] } |]
          Edges =
            [| { Id = "minimal-slice-blocking-edge"
                 Kind = "wall"
                 State = "blocking"
                 StartColumn = 1
                 StartRow = 0
                 EndColumn = 2
                 EndRow = 0 } |]
          Overlays = [||]
          Events =
            Array.init (int attacks) (fun index ->
                { Id = int32 index
                  Tick = int32 (index + 1)
                  Kind = "derived-attack"
                  SourceUnitId = Disclosed 10
                  TargetUnitId = Disclosed 20
                  Summary = NotPresent })
          Disclosure = SandboxDisclosure }

    let private entriesOf
        (map: Map<string, int32>)
        : Int32Entry array
        =
        map
        |> Map.toArray
        |> Array.map (fun (key, value) -> { Key = key; Value = value })

    let private mapOfEntries
        (entries: Int32Entry array)
        : Map<string, int32>
        =
        entries
        |> Array.map (fun entry -> entry.Key, entry.Value)
        |> Map.ofArray

    let parametersToTransport
        (parameters: Map<string, int32>)
        : Int32Entry array
        =
        entriesOf parameters

    let parametersFromTransport
        (parameters: Int32Entry array)
        : Map<string, int32>
        =
        mapOfEntries parameters

    let scenarioToTransport
        (scenario: DesignScenario)
        : DesignScenarioTransport
        =
        { Identity = scenario.Identity
          Revision = scenario.Revision
          Title = scenario.Title
          Description = scenario.Description
          EngineIdentity = scenario.EngineIdentity
          RulesetIdentity = scenario.RulesetIdentity
          Parameters = List.toArray scenario.Parameters }

    let scenarioFromTransport
        (scenario: DesignScenarioTransport)
        : DesignScenario
        =
        { Identity = scenario.Identity
          Revision = scenario.Revision
          Title = scenario.Title
          Description = scenario.Description
          EngineIdentity = scenario.EngineIdentity
          RulesetIdentity = scenario.RulesetIdentity
          Parameters = Array.toList scenario.Parameters }

    let private resultToTransport
        (result: ExperimentResult)
        : ExperimentResultTransport
        =
        { Input =
            { ScenarioIdentity = result.Input.ScenarioIdentity
              ScenarioRevision = result.Input.ScenarioRevision
              EngineIdentity = result.Input.EngineIdentity
              RulesetIdentity = result.Input.RulesetIdentity
              Parameters = entriesOf result.Input.Parameters }
          ResultIdentity = result.ResultIdentity
          Metrics = entriesOf result.Metrics }

    let private resultFromTransport
        (result: ExperimentResultTransport)
        : ExperimentResult
        =
        { Input =
            { ScenarioIdentity = result.Input.ScenarioIdentity
              ScenarioRevision = result.Input.ScenarioRevision
              EngineIdentity = result.Input.EngineIdentity
              RulesetIdentity = result.Input.RulesetIdentity
              Parameters = mapOfEntries result.Input.Parameters }
          ResultIdentity = result.ResultIdentity
          Metrics = mapOfEntries result.Metrics }

    let reportToTransport
        (report: LabReport)
        : LabReportTransport
        =
        { Baseline = resultToTransport report.Comparison.Baseline
          Fork = resultToTransport report.Comparison.Fork
          Delta = entriesOf report.Comparison.Delta
          Sweep =
            report.Sweep
            |> Option.map (fun sweep ->
                { Parameter = sweep.Parameter
                  Results = sweep.Results |> List.map resultToTransport |> List.toArray })
          EvidenceLabel = report.EvidenceLabel }

    let reportFromTransport
        (report: LabReportTransport)
        : LabReport
        =
        { Comparison =
            { Baseline = resultFromTransport report.Baseline
              Fork = resultFromTransport report.Fork
              Delta = mapOfEntries report.Delta }
          Sweep =
            report.Sweep
            |> Option.map (fun sweep ->
                { Parameter = sweep.Parameter
                  Results = sweep.Results |> Array.map resultFromTransport |> Array.toList })
          EvidenceLabel = report.EvidenceLabel }

    let export (report: LabReport) =
        let resultLines prefix (result: ExperimentResult) =
            [ prefix + ".result=" + result.ResultIdentity
              prefix + ".scenario=" + result.Input.ScenarioIdentity
              prefix + ".revision=" + string result.Input.ScenarioRevision
              prefix + ".engine=" + result.Input.EngineIdentity
              prefix + ".ruleset=" + result.Input.RulesetIdentity ]
            @ (result.Input.Parameters
               |> Map.toList
               |> List.map (fun (key, value) ->
                   prefix + ".parameter." + key + "=" + string value))
            @ (result.Metrics
               |> Map.toList
               |> List.map (fun (key, value) ->
                   prefix + ".metric." + key + "=" + string value))

        ([ "format=" + ExportFormat
           "evidence=" + report.EvidenceLabel ]
         @ resultLines "baseline" report.Comparison.Baseline
         @ resultLines "fork" report.Comparison.Fork
         @ (report.Sweep
            |> Option.map (fun sweep ->
                [ "sweep.parameter=" + sweep.Parameter
                  "sweep.count=" + string (List.length sweep.Results) ]
                @ (sweep.Results
                   |> List.mapi (fun index result ->
                       "sweep."
                       + string index
                       + "="
                       + result.ResultIdentity
                       + ","
                       + string (Map.find sweep.Parameter result.Input.Parameters))))
            |> Option.defaultValue []))
        |> String.concat "\n"
        |> fun content -> content + "\n"
