namespace SIR.Tools

open System
open System.IO
open System.Text.Json
open SIR.Domain
open SIR.Simulation

module Program =
    type private Options =
        { Mode: CoherenceMode
          Rules: RuleId list
          MaxWork: int32
          CachePath: string option
          OutputPath: string option
          BlockUnknowns: bool }

    let private parseRuleId value =
        RuleId.create value |> Result.mapError (fun error -> sprintf "invalid --rule '%s': %s" value error)

    let private parse arguments =
        let rec loop (options: Options) remaining =
            match remaining with
            | [] -> Ok options
            | "--mode" :: "changed" :: tail -> loop { options with Mode = CoherenceMode.Changed } tail
            | "--mode" :: "cone" :: tail -> loop { options with Mode = CoherenceMode.Cone } tail
            | "--mode" :: "corpus" :: tail -> loop { options with Mode = CoherenceMode.Corpus } tail
            | "--rule" :: value :: tail -> parseRuleId value |> Result.bind (fun ruleId -> loop { options with Rules = ruleId :: options.Rules } tail)
            | "--max-work" :: value :: tail ->
                match Int32.TryParse value with
                | true, parsed when parsed > 0 -> loop { options with MaxWork = parsed } tail
                | _ -> Error "--max-work requires a positive integer."
            | "--cache" :: value :: tail when not (String.IsNullOrWhiteSpace value) -> loop { options with CachePath = Some value } tail
            | "--out" :: value :: tail when not (String.IsNullOrWhiteSpace value) -> loop { options with OutputPath = Some value } tail
            | "--block-unknowns" :: tail -> loop { options with BlockUnknowns = true } tail
            | token :: _ -> Error(sprintf "unknown or incomplete argument: %s" token)
        let defaults = { Mode = CoherenceMode.Corpus; Rules = []; MaxWork = RuleCoherence.defaultBounds.MaxWorkUnits; CachePath = None; OutputPath = None; BlockUnknowns = false }
        loop defaults arguments
        |> Result.bind (fun options ->
            if options.Mode <> CoherenceMode.Corpus && List.isEmpty options.Rules then Error "changed and cone modes require at least one --rule."
            elif options.Mode = CoherenceMode.Corpus && not (List.isEmpty options.Rules) then Error "--rule is only valid for changed or cone modes."
            else Ok { options with Rules = List.rev options.Rules })

    let private strength value =
        match value with
        | "proved-structural" -> ClaimStrength.ProvedStructural | "proved-fragment" -> ClaimStrength.ProvedFragment
        | "exhaustive-bounded" -> ClaimStrength.ExhaustiveBounded | "tested" -> ClaimStrength.Tested | "heuristic" -> ClaimStrength.Heuristic
        | "unknown" -> ClaimStrength.Unknown | "failed" -> ClaimStrength.Failed | _ -> failwithf "unknown cached strength: %s" value

    let private stringValue (name: string) (element: JsonElement) =
        element.GetProperty(name).GetString() |> Option.ofObj |> Option.defaultWith (fun () -> failwithf "cached %s is null" name)
    let private ruleIds (element: JsonElement) = element.EnumerateArray() |> Seq.map (fun item -> item.GetString() |> Option.ofObj |> Option.defaultWith (fun () -> failwith "cached rule id is null") |> RuleId.create |> Result.defaultWith failwith) |> Seq.toList
    let private witness (element: JsonElement) =
        if element.ValueKind = JsonValueKind.Null then None
        else Some { RuleIds = ruleIds (element.GetProperty("ruleIds")); Fact = stringValue "fact" element; Expected = stringValue "expected" element; Actual = stringValue "actual" element }
    let private finding (element: JsonElement) =
        { Fingerprint = stringValue "fingerprint" element
          Dimension = stringValue "dimension" element
          Strength = strength (stringValue "strength" element)
          RuleIds = ruleIds (element.GetProperty("ruleIds"))
          Message = stringValue "message" element
          DependencyReason = stringValue "dependencyReason" element
          Witness = witness (element.GetProperty("witness")) }

    let private readCache path =
        if not (File.Exists path) then Ok None else
        try
            use document = JsonDocument.Parse(File.ReadAllText path)
            let root = document.RootElement
            let cache = root.GetProperty("cache")
            if cache.ValueKind = JsonValueKind.Null then Error "cache file contains no reusable complete analysis."
            else
                Ok(Some
                    { Key = stringValue "key" cache
                      Findings = root.GetProperty("findings").EnumerateArray() |> Seq.map finding |> Seq.toList
                      CandidatePairs = cache.GetProperty("candidatePairs").GetInt32()
                      PrunedPairs = cache.GetProperty("prunedPairs").GetInt32() })
        with error -> Error(sprintf "cache is malformed or unreadable: %s" error.Message)

    let private write (path: string) (content: string) =
        let directory = Path.GetDirectoryName(Path.GetFullPath path)
        match directory |> Option.ofObj with
        | Some value when not (Directory.Exists value) -> Directory.CreateDirectory value |> ignore
        | _ -> ()
        File.WriteAllText(path, content)

    let private runCheck options =
        match options.CachePath |> Option.map readCache |> Option.defaultValue (Ok None) with
        | Error error -> eprintfn "%s" error; 2
        | Ok cache ->
            let request =
                { Mode = options.Mode
                  ChangedRuleIds = options.Rules
                  Bounds = { RuleCoherence.defaultBounds with MaxWorkUnits = options.MaxWork }
                  BlockUnknowns = options.BlockUnknowns }
            let report = RuleCoherence.analyze CombatRules.packageIdentity CombatRules.registry cache request
            let json = RuleCoherence.reportJson report
            options.OutputPath |> Option.iter (fun path -> write path json)
            options.CachePath |> Option.iter (fun path -> if report.CacheEntry.IsSome then write path json)
            Console.Out.Write json
            if report.Findings |> List.exists (fun item -> item.Strength = ClaimStrength.Failed) then 3
            elif report.Termination <> AnalysisTermination.Complete || (options.BlockUnknowns && not report.CanonicalizationReady && report.Findings |> List.exists (fun item -> item.Strength = ClaimStrength.Unknown)) then 4
            else 0

    [<EntryPoint>]
    let main arguments =
        match arguments |> Array.toList with
        | "sir-rules" :: "check" :: remaining ->
            match parse remaining with
            | Ok options -> runCheck options
            | Error error -> eprintfn "%s" error; 2
        | _ ->
            eprintfn "Usage: SIR.Tools sir-rules check [--mode changed|cone|corpus] [--rule RULE-ID] [--max-work N] [--cache PATH] [--out PATH] [--block-unknowns]"
            2
