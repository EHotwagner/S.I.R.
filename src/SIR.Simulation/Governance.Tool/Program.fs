module SIR.Rules.Governance.Tool.Program

open System
open System.IO
open System.Security.Cryptography
open System.Text.Json
open SIR.Rules.Governance
open SIR.Simulation

let private hex (bytes: byte array) = Convert.ToHexString(bytes).ToLowerInvariant()
let private relative (root: string) path = Path.GetRelativePath(root, path).Replace('\\', '/')

let private sense root kind relativePath packageDigest semanticDigest =
    let path = Path.Combine(root, relativePath)
    if not (File.Exists path) then
        { Kind = kind; Artifact = relativePath; State = Missing; Digest = None; PackageManifestDigest = Some packageDigest; SemanticDigest = Some semanticDigest }
    else
        try
            let bytes = File.ReadAllBytes path
            let state =
                if path.EndsWith(".junit.xml", StringComparison.Ordinal) then
                    let text = File.ReadAllText path
                    if text.Contains("failures=\"0\"", StringComparison.Ordinal) then CurrentPass else CurrentFail
                else CurrentPass
            { Kind = kind; Artifact = relative root path; State = state; Digest = Some(SHA256.HashData(bytes) |> hex); PackageManifestDigest = Some packageDigest; SemanticDigest = Some semanticDigest }
        with
        | :? UnauthorizedAccessException -> { Kind = kind; Artifact = relativePath; State = Unavailable; Digest = None; PackageManifestDigest = Some packageDigest; SemanticDigest = Some semanticDigest }
        | :? IOException -> { Kind = kind; Artifact = relativePath; State = Unavailable; Digest = None; PackageManifestDigest = Some packageDigest; SemanticDigest = Some semanticDigest }
        | _ -> { Kind = kind; Artifact = relativePath; State = Malformed; Digest = None; PackageManifestDigest = Some packageDigest; SemanticDigest = Some semanticDigest }

let private generate (root: string) (receiptPath: string) (verdictPath: string) =
    let package = CombatRules.packageIdentity
    let packageDigest = hex package.ManifestDigest
    let semanticDigest = hex package.SemanticDigest
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
        |> List.map (fun (kind, path) -> sense root kind path packageDigest semanticDigest)
    let surface =
        [ "src/SIR.Domain/RuleTypes.fsi"; "src/SIR.Domain/Rules.fsi"; "src/SIR.Domain/Governance/RuleGovernance.fsi" ]
        |> List.map (fun path -> sense root "public-surface" path packageDigest semanticDigest)
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

let private join (sddPath: string) (verdictPath: string) (outputPath: string) =
    let sddBytes = File.ReadAllBytes sddPath
    let verdictBytes = File.ReadAllBytes verdictPath
    use sdd = JsonDocument.Parse(sddBytes)
    use verdict = JsonDocument.Parse(verdictBytes)
    let sddReady = sdd.RootElement.GetProperty("readiness").GetString() = "shipReady"
    let governanceBlocked = verdict.RootElement.GetProperty("blocked").GetBoolean()
    let boundary = Policy.joinProtectedBoundary sddPath sddBytes sddReady verdictPath verdictBytes governanceBlocked
    File.WriteAllBytes(outputPath, Policy.encodeProtectedBoundary boundary)
    printfn "protected-boundary=%s allowed=%b" outputPath boundary.Allowed
    if boundary.Allowed then 0 else 3

[<EntryPoint>]
let main arguments =
    try
        match arguments with
        | [| "generate"; root; receiptPath; verdictPath |] -> generate (Path.GetFullPath root) (Path.GetFullPath receiptPath) (Path.GetFullPath verdictPath)
        | [| "join"; sddPath; verdictPath; outputPath |] -> join (Path.GetFullPath sddPath) (Path.GetFullPath verdictPath) (Path.GetFullPath outputPath)
        | _ -> eprintfn "usage: generate ROOT RECEIPT VERDICT | join SDD_SHIP VERDICT OUTPUT"; 2
    with error -> eprintfn "rules-governance: %s" error.Message; 2
