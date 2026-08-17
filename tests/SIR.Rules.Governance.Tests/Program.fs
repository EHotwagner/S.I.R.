module SIR.Rules.Governance.Tests.Program

open System
open System.IO
open FS.GG.Governance.Kernel
open SIR.Rules.Governance
open SIR.Simulation

let private require condition message = if not condition then failwith message
let private hex (bytes: byte array) = Convert.ToHexString(bytes).ToLowerInvariant()

let private evidence state kind artifact package semantic =
    { Kind = kind; Artifact = artifact; State = state; Digest = Some(String.replicate 64 "a"); PackageManifestDigest = Some package; SemanticDigest = Some semantic }

let private baseline () =
    let package = CombatRules.packageIdentity
    let manifest = hex package.ManifestDigest
    let semantic = hex package.SemanticDigest
    let surface = [ evidence CurrentPass "public-surface" "src/SIR.Domain/Rules.fsi" manifest semantic ]
    let facts =
        [ evidence CurrentPass "corpus-manifest" "manifest.json" manifest semantic
          evidence CurrentPass "corpus-coverage" "coverage.json" manifest semantic
          evidence CurrentPass "corpus-implementation" "implementation-sources.json" manifest semantic
          evidence CurrentPass "semantic" "semantic.trx" manifest semantic
          evidence CurrentPass "runtime-parity" "dotnet.trx" manifest semantic
          evidence CurrentPass "runtime-parity" "fable-node.trx" manifest semantic
          evidence CurrentPass "runtime-parity" "browser.junit.xml" manifest semantic
          evidence CurrentPass "generated-view" "rules.json" manifest semantic
          evidence CurrentPass "historical-replay" "replay.trx" manifest semantic
          evidence CurrentPass "production-journey" "journey.junit.xml" manifest semantic ]
    Receipt.create package CombatRules.registry surface facts "legacy"

let private finding id verdict = verdict.Findings |> List.find (fun item -> item.Id = id)
let private isNonPass = function Pass -> false | _ -> true
let private assertMutation id boundary mutate =
    let mutated = baseline () |> mutate
    let verdict = Policy.evaluate boundary Standard mutated
    let result = finding id verdict
    require (isNonPass result.Verdict && result.EffectiveBlocking) (sprintf "%s mutation did not block" id)
    require (result.ReceiptDigest = mutated.PayloadDigest) (sprintf "%s lost receipt provenance" id)

let private replaceEvidence kind replacement receipt =
    let retained = receipt.Payload.Evidence |> List.filter (fun item -> item.Kind <> kind)
    let payload = { receipt.Payload with Evidence = replacement @ retained }
    { receipt with Payload = payload; PayloadDigest = Receipt.payloadBytes payload |> System.Security.Cryptography.SHA256.HashData |> hex }

let private evidenceWithState kind state receipt =
    let item = receipt.Payload.Evidence |> List.find (fun value -> value.Kind = kind)
    { item with State = state }

let private run () =
    let receipt = baseline ()
    let bytes = Receipt.encode receipt
    let reversed = Receipt.create CombatRules.packageIdentity (List.rev CombatRules.registry) (List.rev receipt.Payload.Surface) (List.rev receipt.Payload.Evidence) "legacy" |> Receipt.encode
    require (bytes = reversed) "receipt encoding depends on enumeration order"
    match Receipt.decode bytes with
    | Ok decoded -> require (Receipt.encode decoded = bytes) "receipt did not round-trip canonically"
    | Error error -> failwith error
    let changed = Array.copy bytes
    changed[changed.Length - 2] <- changed[changed.Length - 2] ^^^ 1uy
    require (Receipt.decode changed |> Result.isError) "malformed receipt was accepted"

    let pass = Policy.evaluate Ship Standard receipt
    require (not pass.Blocked) "complete receipt was blocked"
    require (pass.Findings |> List.forall (fun item -> item.Rendered.Length > 0 && item.StructuralHash.Length = 64 && item.ExplanationJson.Contains("verdict", StringComparison.Ordinal) && item.Provenance.Length = 1)) "checks did not share stable render/hash/explanation/provenance projections"
    require (Policy.adapter.Rules.Length = Policy.checks.Length && Policy.adapter.Probes.Length = Policy.checks.Length) "adapter is not total"

    assertMutation "receipt-well-formed" PullRequest (fun item -> { item with PayloadDigest = String.replicate 64 "0" })
    assertMutation "rule-identities-valid" PullRequest (fun item ->
        let duplicate = item.Payload.Rules.Head
        let payload = { item.Payload with Rules = duplicate :: item.Payload.Rules }
        { item with Payload = payload; PayloadDigest = Receipt.payloadBytes payload |> System.Security.Cryptography.SHA256.HashData |> hex })
    assertMutation "rule-identities-valid" PullRequest (fun item ->
        let invalid = { item.Payload.Rules.Head with SemanticKind = "javascript" }
        let payload = { item.Payload with Rules = invalid :: item.Payload.Rules.Tail }
        { item with Payload = payload; PayloadDigest = Receipt.payloadBytes payload |> System.Security.Cryptography.SHA256.HashData |> hex })
    assertMutation "surface-current" PullRequest (fun item -> { item with Payload = { item.Payload with Surface = [ { item.Payload.Surface.Head with State = Stale } ] } } |> fun changed -> { changed with PayloadDigest = Receipt.payloadBytes changed.Payload |> System.Security.Cryptography.SHA256.HashData |> hex })
    assertMutation "generated-views-current" PullRequest (replaceEvidence "generated-view" [ evidenceWithState "generated-view" Malformed receipt ])
    assertMutation "semantic-evidence-current" Ship (replaceEvidence "semantic" [ evidenceWithState "semantic" Synthetic receipt ])
    assertMutation "runtime-parity-equal" Ship (fun item ->
        let parity = item.Payload.Evidence |> List.filter (fun value -> value.Kind = "runtime-parity") |> List.mapi (fun index value -> if index = 0 then { value with PackageManifestDigest = Some(String.replicate 64 "f") } else value)
        replaceEvidence "runtime-parity" parity item)
    assertMutation "package-identities-consistent" Ship (fun item ->
        let semantic = item.Payload.Evidence |> List.find (fun value -> value.Kind = "semantic")
        replaceEvidence "semantic" [ { semantic with PackageManifestDigest = None } ] item)
    assertMutation "historical-replay-exact" Ship (replaceEvidence "historical-replay" [ evidenceWithState "historical-replay" Unavailable receipt ])
    assertMutation "production-journey-present" Ship (replaceEvidence "production-journey" [])

    let migration = Policy.evaluate PullRequest Migration ({ receipt with PayloadDigest = String.replicate 64 "0" })
    let standard = Policy.evaluate PullRequest Standard ({ receipt with PayloadDigest = String.replicate 64 "0" })
    require ((finding "receipt-well-formed" migration).Verdict = (finding "receipt-well-formed" standard).Verdict) "profile hid the underlying verdict"
    require ((finding "metadata-complete" (Policy.evaluate PullRequest Strict receipt)).Rendered = (finding "metadata-complete" pass).Rendered) "profile changed the check contract"

    let verdictBytes = Policy.encodeVerdict pass
    let boundary = Policy.joinProtectedBoundary "ship-verdict.json" [| 1uy; 2uy |] true "rules-governance-verdict.json" verdictBytes pass.Blocked
    require (boundary.Allowed && boundary.SddShipDigest <> boundary.GovernanceVerdictDigest) "SDD and Governance artifacts were conflated"
    require ((Policy.encodeProtectedBoundary boundary).Length > 0) "protected-boundary encoding is empty"
    let reportDirectory = Path.Combine("readiness", "198-rules-governance-receipts")
    let reportPath = Path.Combine(reportDirectory, "rules-governance-tests.junit.xml")
    Directory.CreateDirectory(reportDirectory) |> ignore
    File.WriteAllText(reportPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><testsuite name=\"sir-rules-governance\" tests=\"11\" failures=\"0\" errors=\"0\" skipped=\"0\"><testcase name=\"canonical-receipt\"/><testcase name=\"closed-evidence-states\"/><testcase name=\"fixed-point-provenance\"/><testcase name=\"pr-mutations\"/><testcase name=\"ship-mutations\"/><testcase name=\"runtime-parity\"/><testcase name=\"package-identity\"/><testcase name=\"historical-replay\"/><testcase name=\"production-journey\"/><testcase name=\"profile-invariance\"/><testcase name=\"protected-boundary\"/></testsuite>\n")
    printfn "rules-governance tests passed: rules=%d checks=%d mutations=10" receipt.Payload.Rules.Length Policy.checks.Length

[<EntryPoint>]
let main _ =
    try run (); 0 with error -> eprintfn "%s" error.Message; 1
