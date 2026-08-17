module SIR.Rules.Governance.Tool.Program

open System
open System.IO
open System.Security.Cryptography
open System.Text.Json
open System.Xml.Linq
open SIR.Rules.Governance
open SIR.Simulation

let private hex (bytes: byte array) = Convert.ToHexString(bytes).ToLowerInvariant()
let private relative (root: string) path = Path.GetRelativePath(root, path).Replace('\\', '/')

type private EvidenceBinding =
    { Artifact: string
      Digest: string
      PackageManifestDigest: string
      SemanticDigest: string }

let private requiredString (element: JsonElement) (name: string) =
    let mutable value = Unchecked.defaultof<JsonElement>
    if element.TryGetProperty(name, &value) && value.ValueKind = JsonValueKind.String then
        value.GetString() |> Option.ofObj |> Option.defaultWith (fun () -> failwithf "null %s" name)
    else failwithf "missing string %s" name

let private loadBindings root =
    let path = Path.Combine(root, "readiness/198-rules-governance-receipts/evidence-bindings.json")
    use document = JsonDocument.Parse(File.ReadAllBytes path)
    if requiredString document.RootElement "schema" <> "sir-rules-evidence-bindings/v1" then failwith "unsupported evidence bindings schema"
    document.RootElement.GetProperty("bindings").EnumerateArray()
    |> Seq.map (fun item ->
        let binding =
            { Artifact = requiredString item "artifact"
              Digest = requiredString item "digest"
              PackageManifestDigest = requiredString item "packageManifestDigest"
              SemanticDigest = requiredString item "semanticDigest" }
        binding.Artifact, binding)
    |> Map.ofSeq

let private junitState (path: string) =
    try
        let document = XDocument.Load(path, LoadOptions.None)
        let suites =
            document.Descendants()
            |> Seq.filter (fun item -> item.Name.LocalName = "testsuite" || item.Name.LocalName = "testsuites")
            |> Seq.toList
        let count name (item: XElement) =
            match item.Attribute(XName.Get name) |> Option.ofObj with
            | Some value -> match Int32.TryParse value.Value with true, parsed when parsed >= 0 -> Some parsed | _ -> None
            | None -> None
        let optionalCount name (item: XElement) =
            if item.Attribute(XName.Get name) |> isNull then Some 0 else count name item
        if List.isEmpty suites then Malformed
        elif suites |> List.exists (fun item -> count "tests" item |> Option.isNone) then Malformed
        elif suites |> List.exists (fun item -> count "failures" item |> Option.isNone) then Malformed
        elif suites |> List.exists (fun item -> optionalCount "errors" item |> Option.isNone || optionalCount "skipped" item |> Option.isNone) then Malformed
        elif suites |> List.exists (fun item -> count "failures" item <> Some 0 || optionalCount "errors" item <> Some 0 || optionalCount "skipped" item <> Some 0) then CurrentFail
        else CurrentPass
    with _ -> Malformed

let private sense root bindings kind relativePath =
    let path = Path.Combine(root, relativePath)
    let binding = Map.tryFind relativePath bindings
    let identities state digest =
        match state, binding with
        | Malformed, _ -> Malformed, None, None
        | CurrentFail, _ -> CurrentFail, None, None
        | _, Some expected when digest = Some expected.Digest -> state, Some expected.PackageManifestDigest, Some expected.SemanticDigest
        | _, Some _ -> Stale, None, None
        | _, None -> Malformed, None, None
    if not (File.Exists path) then
        { Kind = kind; Artifact = relativePath; State = Missing; Digest = None; PackageManifestDigest = None; SemanticDigest = None }
    else
        try
            let bytes = File.ReadAllBytes path
            let digest = Some(SHA256.HashData(bytes) |> hex)
            let parsed = if path.EndsWith(".junit.xml", StringComparison.Ordinal) then junitState path else CurrentPass
            let state, packageDigest, semanticDigest = identities parsed digest
            { Kind = kind; Artifact = relative root path; State = state; Digest = digest; PackageManifestDigest = packageDigest; SemanticDigest = semanticDigest }
        with
        | :? UnauthorizedAccessException -> { Kind = kind; Artifact = relativePath; State = Unavailable; Digest = None; PackageManifestDigest = None; SemanticDigest = None }
        | :? IOException -> { Kind = kind; Artifact = relativePath; State = Unavailable; Digest = None; PackageManifestDigest = None; SemanticDigest = None }
        | _ -> { Kind = kind; Artifact = relativePath; State = Malformed; Digest = None; PackageManifestDigest = None; SemanticDigest = None }

let private generate (root: string) (receiptPath: string) (verdictPath: string) =
    let package = CombatRules.packageIdentity
    let bindings = loadBindings root
    let evidence =
        [ "corpus-manifest", "tests/fixtures/rules-corpus/v2/manifest.json"
          "corpus-coverage", "tests/fixtures/rules-corpus/v2/coverage.json"
          "corpus-implementation", "tests/fixtures/rules-corpus/v2/implementation-sources.json"
          "semantic", "readiness/194-executable-rules-corpus/rules-corpus-canonical.junit.xml"
          "runtime-parity", "readiness/194-executable-rules-corpus/full-conformance.junit.xml"
          "runtime-parity", "readiness/194-executable-rules-corpus/rules-corpus-canonical.junit.xml"
          "runtime-parity", "readiness/194-executable-rules-corpus/rules-corpus-browser.junit.xml"
          "generated-view", "readiness/194-executable-rules-corpus/ship-verdict.json"
          "historical-replay", "readiness/194-executable-rules-corpus/replay-v3.junit.xml"
          "production-journey", "readiness/194-executable-rules-corpus/rules-player-browser.junit.xml" ]
        |> List.map (fun (kind, path) -> sense root bindings kind path)
    let surface =
        [ "src/SIR.Domain/RuleTypes.fsi"; "src/SIR.Domain/Rules.fsi"; "src/SIR.Domain/Governance/RuleGovernance.fsi" ]
        |> List.map (fun path -> sense root bindings "public-surface" path)
    let receipt = Receipt.create package CombatRules.registry surface evidence "legacy"
    let receiptBytes = Receipt.encode receipt
    match Receipt.decode receiptBytes with Error error -> failwith error | Ok _ -> ()
    let verdict = Policy.evaluate Ship Standard receipt
    let verdictBytes = Policy.encodeVerdict verdict
    match Path.GetDirectoryName receiptPath |> Option.ofObj with
    | Some directory -> Directory.CreateDirectory(directory) |> ignore
    | None -> ()
    File.WriteAllBytes(receiptPath, receiptBytes)
    File.WriteAllBytes(verdictPath, verdictBytes)
    printfn "receipt=%s digest=%s rules=%d blocked=%b" receiptPath receipt.PayloadDigest receipt.Payload.Rules.Length verdict.Blocked
    if verdict.Blocked then 3 else 0

let private join (sddPath: string) (verdictPath: string) (outputPath: string) (sddArtifact: string) (verdictArtifact: string) =
    let sddBytes = File.ReadAllBytes sddPath
    let verdictBytes = File.ReadAllBytes verdictPath
    use sdd = JsonDocument.Parse(sddBytes)
    use verdict = JsonDocument.Parse(verdictBytes)
    let sddBlocking = sdd.RootElement.GetProperty("disposition").GetProperty("blockingFindingIds").GetArrayLength()
    let verifyBlocking = sdd.RootElement.GetProperty("verificationReadiness").GetProperty("blockingFindingIds").GetArrayLength()
    let sddReady =
        sdd.RootElement.GetProperty("schemaVersion").GetInt32() = 1
        && sdd.RootElement.GetProperty("stage").GetString() = "ship"
        && sdd.RootElement.GetProperty("workId").GetString() = "198-rules-governance-receipts"
        && sdd.RootElement.GetProperty("readiness").GetString() = "shipReady"
        && sdd.RootElement.GetProperty("disposition").GetProperty("state").GetString() = "shipReady"
        && sdd.RootElement.GetProperty("verificationReadiness").GetProperty("status").GetString() = "verificationReady"
        && sddBlocking = 0 && verifyBlocking = 0
    if not sddReady then failwith "invalid or unready SDD ship artifact"
    if requiredString verdict.RootElement "schema" <> "sir-rules-governance-verdict/v1" then failwith "unsupported governance verdict schema"
    if requiredString verdict.RootElement "boundary" <> "ship" || requiredString verdict.RootElement "profile" <> "standard" then failwith "governance verdict is not the protected ship/standard decision"
    let findings = verdict.RootElement.GetProperty("findings").EnumerateArray() |> Seq.toList
    let expectedIds = Policy.checks |> List.map _.Id |> Set.ofList
    let actualIds = findings |> List.map (fun item -> requiredString item "id") |> Set.ofList
    if actualIds <> expectedIds || findings.Length <> expectedIds.Count then failwith "governance verdict check inventory is incomplete"
    let receiptDigest = requiredString verdict.RootElement "receiptDigest"
    if receiptDigest.Length <> 64 || receiptDigest |> Seq.exists (fun character -> not (Uri.IsHexDigit character)) then failwith "governance verdict receipt digest is invalid"
    let maturities = Policy.checks |> List.map (fun check -> check.Id, check.Maturity) |> Map.ofList
    for finding in findings do
        let id = requiredString finding "id"
        let renderedMaturity = requiredString finding "maturity"
        let expectedMaturity, blocks =
            match maturities[id] with
            | Warn -> "warn", false
            | BlockOnPr -> "block-on-pr", true
            | BlockOnShip -> "block-on-ship", true
        if renderedMaturity <> expectedMaturity then failwithf "governance verdict maturity contradicts check %s" id
        let renderedVerdict = requiredString finding "verdict"
        if not (Set [ "pass"; "fail"; "unknown" ] |> Set.contains renderedVerdict) then failwithf "governance verdict outcome is invalid for %s" id
        let expectedBlocking = blocks && renderedVerdict <> "pass"
        if finding.GetProperty("effectiveBlocking").GetBoolean() <> expectedBlocking then failwithf "governance verdict effective blocking contradicts %s" id
    let calculatedBlocked = findings |> List.exists (fun item -> item.GetProperty("effectiveBlocking").GetBoolean())
    let governanceBlocked = verdict.RootElement.GetProperty("blocked").GetBoolean()
    if governanceBlocked <> calculatedBlocked then failwith "governance verdict blocked state contradicts its findings"
    let boundary = Policy.joinProtectedBoundary sddArtifact sddBytes sddReady verdictArtifact verdictBytes governanceBlocked
    File.WriteAllBytes(outputPath, Policy.encodeProtectedBoundary boundary)
    printfn "protected-boundary=%s allowed=%b" outputPath boundary.Allowed
    if boundary.Allowed then 0 else 3

[<EntryPoint>]
let main arguments =
    try
        match arguments with
        | [| "generate"; root; receiptPath; verdictPath |] -> generate (Path.GetFullPath root) (Path.GetFullPath receiptPath) (Path.GetFullPath verdictPath)
        | [| "join"; sddPath; verdictPath; outputPath; sddArtifact; verdictArtifact |] -> join (Path.GetFullPath sddPath) (Path.GetFullPath verdictPath) (Path.GetFullPath outputPath) sddArtifact verdictArtifact
        | _ -> eprintfn "usage: generate ROOT RECEIPT VERDICT | join SDD_SHIP VERDICT OUTPUT SDD_ARTIFACT VERDICT_ARTIFACT"; 2
    with error -> eprintfn "rules-governance: %s" error.Message; 2
