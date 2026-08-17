namespace SIR.Rules.Governance

open System
open System.IO
open System.Security.Cryptography
open System.Text
open System.Text.Json
open SIR.Domain
open FS.GG.Governance.Kernel
open FS.GG.Governance.Adapters.Spi

type EvidenceState = CurrentPass | CurrentFail | Missing | Malformed | Stale | Synthetic | Unavailable
type Maturity = Warn | BlockOnPr | BlockOnShip
type Boundary = PullRequest | Ship
type EnforcementProfile = Migration | Standard | Strict
type RuntimeKind = DotNet | FableNode | Browser

type EvidenceRef = { Kind: string; Artifact: string; State: EvidenceState; Digest: string option; PackageManifestDigest: string option; SemanticDigest: string option }
type RuleReceipt = { RuleId: string; Title: string; Status: string; SemanticKind: string; Dependencies: string list; Supersedes: string list; SourcePath: string option; SourceSymbol: string option; HasXmlDocumentation: bool; Evidence: EvidenceRef list }
type PackageBinding = { EngineIdentity: string; CompatibilityProfile: string; PackageVersion: string; SourceCommit: string; ImplementationDigest: string; SemanticDigest: string; ManifestDigest: string }
type ReceiptPayload = { Package: PackageBinding; Rules: RuleReceipt list; Surface: EvidenceRef list; Evidence: EvidenceRef list; LegacyClassification: string }
type ReceiptEnvelope = { Schema: string; PayloadDigest: string; Payload: ReceiptPayload }
type GovernedArtifact = Receipt | RuleManifest | PublicSurface | SemanticEvidence | RuntimeParity | GeneratedView | ReplayPackage | ProductionJourney
type GovernedChange = { Paths: Set<string> }
type GovernanceFact = ReceiptFact of ReceiptEnvelope | GovernanceOutcome of RuleOutcome
type GovernedCheck = { Id: string; Maturity: Maturity; Rule: CheckRule<GovernanceFact> }
type Finding = { Id: string; Maturity: Maturity; Verdict: Verdict; Rendered: string; StructuralHash: string; Reads: ArtifactRef list; ExplanationJson: string; Provenance: ProvenanceStep list; ReceiptDigest: string; EffectiveBlocking: bool }
type GovernanceVerdict = { Schema: string; ReceiptDigest: string; Boundary: Boundary; Profile: EnforcementProfile; Findings: Finding list; Blocked: bool }
type ProtectedBoundary = { Schema: string; SddShipArtifact: string; SddShipDigest: string; GovernanceVerdictArtifact: string; GovernanceVerdictDigest: string; SddReady: bool; GovernanceBlocked: bool; Allowed: bool }

module private Canonical =
    let hex (bytes: byte array) : string = Convert.ToHexString(bytes).ToLowerInvariant()
    let sha256 (bytes: byte array) : string = SHA256.HashData(bytes) |> hex
    let state = function CurrentPass -> "current-pass" | CurrentFail -> "current-fail" | Missing -> "missing" | Malformed -> "malformed" | Stale -> "stale" | Synthetic -> "synthetic" | Unavailable -> "unavailable"
    let parseState = function "current-pass" -> Ok CurrentPass | "current-fail" -> Ok CurrentFail | "missing" -> Ok Missing | "malformed" -> Ok Malformed | "stale" -> Ok Stale | "synthetic" -> Ok Synthetic | "unavailable" -> Ok Unavailable | value -> Error("unknown evidence state: " + value)
    let maturity = function Warn -> "warn" | BlockOnPr -> "block-on-pr" | BlockOnShip -> "block-on-ship"
    let boundary = function PullRequest -> "pull-request" | Ship -> "ship"
    let profile = function Migration -> "migration" | Standard -> "standard" | Strict -> "strict"
    let verdict = function Pass -> "pass", None | Fail reason -> "fail", Some reason | Uncertain reason -> "unknown", Some reason
    let option (writer: Utf8JsonWriter) (name: string) (value: string option) =
        match value with
        | Some text -> writer.WriteString(name, text)
        | None -> writer.WriteNull(name)
    let evidence (writer: Utf8JsonWriter) (item: EvidenceRef) =
        writer.WriteStartObject(); writer.WriteString("kind", item.Kind); writer.WriteString("artifact", item.Artifact); writer.WriteString("state", state item.State)
        option writer "digest" item.Digest; option writer "packageManifestDigest" item.PackageManifestDigest; option writer "semanticDigest" item.SemanticDigest; writer.WriteEndObject()
    let evidenceArray (writer: Utf8JsonWriter) (name: string) (items: EvidenceRef list) =
        writer.WriteStartArray(name)
        items |> List.sortBy (fun item -> item.Kind, item.Artifact) |> List.iter (evidence writer)
        writer.WriteEndArray()
    let stringArray (writer: Utf8JsonWriter) (name: string) (values: string list) =
        writer.WriteStartArray(name)
        values |> List.sort |> List.iter writer.WriteStringValue
        writer.WriteEndArray()
    let withWriter (write: Utf8JsonWriter -> unit) =
        use stream = new MemoryStream()
        use writer = new Utf8JsonWriter(stream, JsonWriterOptions(Indented = false))
        write writer
        writer.Flush()
        stream.ToArray()

[<RequireQualifiedAccess>]
module Receipt =
    let payloadBytes (payload: ReceiptPayload) =
        Canonical.withWriter (fun writer ->
            writer.WriteStartObject()
            writer.WriteStartObject("package"); writer.WriteString("engineIdentity", payload.Package.EngineIdentity); writer.WriteString("compatibilityProfile", payload.Package.CompatibilityProfile); writer.WriteString("packageVersion", payload.Package.PackageVersion); writer.WriteString("sourceCommit", payload.Package.SourceCommit); writer.WriteString("implementationDigest", payload.Package.ImplementationDigest); writer.WriteString("semanticDigest", payload.Package.SemanticDigest); writer.WriteString("manifestDigest", payload.Package.ManifestDigest); writer.WriteEndObject()
            writer.WriteStartArray("rules")
            payload.Rules |> List.sortBy _.RuleId |> List.iter (fun rule ->
                writer.WriteStartObject(); writer.WriteString("ruleId", rule.RuleId); writer.WriteString("title", rule.Title); writer.WriteString("status", rule.Status); writer.WriteString("semanticKind", rule.SemanticKind)
                Canonical.stringArray writer "dependencies" rule.Dependencies; Canonical.stringArray writer "supersedes" rule.Supersedes; Canonical.option writer "sourcePath" rule.SourcePath; Canonical.option writer "sourceSymbol" rule.SourceSymbol; writer.WriteBoolean("hasXmlDocumentation", rule.HasXmlDocumentation); Canonical.evidenceArray writer "evidence" rule.Evidence; writer.WriteEndObject())
            writer.WriteEndArray(); Canonical.evidenceArray writer "surface" payload.Surface; Canonical.evidenceArray writer "evidence" payload.Evidence; writer.WriteString("legacyClassification", payload.LegacyClassification); writer.WriteEndObject())

    let create (package: RulePackageIdentity) (rules: RuleDefinition list) (surface: EvidenceRef list) (evidence: EvidenceRef list) (legacyClassification: string) =
        let packageDigest = Canonical.hex package.ManifestDigest
        let semanticDigest = Canonical.hex package.SemanticDigest
        let perRuleEvidence (ruleId: string) =
            evidence
            |> List.filter (fun item ->
                item.Artifact.Contains(ruleId, StringComparison.Ordinal)
                || item.Kind = "semantic"
                || item.Kind.StartsWith("corpus-", StringComparison.Ordinal))
        let mapped =
            rules |> List.map (fun rule ->
                { RuleId = RuleId.value rule.Metadata.Id
                  Title = rule.Metadata.Title
                  Status = (string rule.Metadata.Status).ToLowerInvariant()
                  SemanticKind = (string rule.Metadata.SemanticKind).ToLowerInvariant()
                  Dependencies = rule.Metadata.Dependencies |> List.map RuleId.value
                  Supersedes = rule.Metadata.Supersedes |> List.map RuleId.value
                  SourcePath = rule.Metadata.RuleSource |> Option.map _.RepositoryPath
                  SourceSymbol = rule.Metadata.RuleSource |> Option.map _.Symbol
                  // RuleSource proves a curated signature location, while XML documentation is a
                  // separate public-surface fact. The current corpus has not supplied that fact,
                  // so the migration warning must remain visible instead of inferring it from rationale.
                  HasXmlDocumentation = false
                  Evidence = perRuleEvidence (RuleId.value rule.Metadata.Id) })
        let payload = { Package = { EngineIdentity = package.EngineIdentity; CompatibilityProfile = package.CompatibilityProfile; PackageVersion = package.PackageVersion; SourceCommit = package.SourceCommit; ImplementationDigest = Canonical.hex package.ImplementationDigest; SemanticDigest = semanticDigest; ManifestDigest = packageDigest }; Rules = mapped; Surface = surface; Evidence = evidence; LegacyClassification = legacyClassification }
        { Schema = "sir-rules-governance/v1"; PayloadDigest = payloadBytes payload |> Canonical.sha256; Payload = payload }

    let encode (envelope: ReceiptEnvelope) =
        Canonical.withWriter (fun writer ->
            writer.WriteStartObject()
            writer.WriteString("schema", envelope.Schema)
            writer.WriteString("payloadDigest", envelope.PayloadDigest)
            writer.WritePropertyName("payload")
            use document = JsonDocument.Parse(payloadBytes envelope.Payload)
            document.RootElement.WriteTo(writer)
            writer.WriteEndObject())

    let private optionalString (element: JsonElement) (name: string) =
        let mutable value = Unchecked.defaultof<JsonElement>
        if element.TryGetProperty(name, &value) && value.ValueKind = JsonValueKind.String then value.GetString() |> Option.ofObj
        else None
    let private requiredString (element: JsonElement) (name: string) =
        match optionalString element name with Some value -> Ok value | None -> Error("missing string: " + name)
    let private sequence (results: Result<'a, string> list) : Result<'a list, string> =
        results |> List.fold (fun state next -> match state, next with Ok values, Ok value -> Ok(value :: values) | Error error, _ | _, Error error -> Error error) (Ok []) |> Result.map List.rev
    let private parseEvidence (element: JsonElement) =
        match requiredString element "kind", requiredString element "artifact", requiredString element "state" with
        | Ok kind, Ok artifact, Ok state -> Canonical.parseState state |> Result.map (fun parsed -> { Kind = kind; Artifact = artifact; State = parsed; Digest = optionalString element "digest"; PackageManifestDigest = optionalString element "packageManifestDigest"; SemanticDigest = optionalString element "semanticDigest" })
        | Error error, _, _ | _, Error error, _ | _, _, Error error -> Error error
    let private parseEvidenceArray (element: JsonElement) (name: string) =
        let mutable value = Unchecked.defaultof<JsonElement>
        if not (element.TryGetProperty(name, &value)) || value.ValueKind <> JsonValueKind.Array then Error("missing array: " + name) else value.EnumerateArray() |> Seq.map parseEvidence |> Seq.toList |> sequence
    let private parseStrings (element: JsonElement) (name: string) =
        let mutable value = Unchecked.defaultof<JsonElement>
        if not (element.TryGetProperty(name, &value)) || value.ValueKind <> JsonValueKind.Array then Error("missing array: " + name)
        else value.EnumerateArray() |> Seq.map (fun item -> if item.ValueKind = JsonValueKind.String then match item.GetString() |> Option.ofObj with Some text -> Ok text | None -> Error("null string in " + name) else Error("non-string in " + name)) |> Seq.toList |> sequence
    let verify (envelope: ReceiptEnvelope) =
        if envelope.Schema <> "sir-rules-governance/v1" then
            Error "unsupported receipt schema"
        else
            let actual = payloadBytes envelope.Payload |> Canonical.sha256
            if String.Equals(actual, envelope.PayloadDigest, StringComparison.Ordinal) then Ok envelope
            else Error(sprintf "payload digest mismatch: expected %s actual %s" envelope.PayloadDigest actual)

    let decode (bytes: byte array) : Result<ReceiptEnvelope, string> =
        try
            use document = JsonDocument.Parse(bytes)
            let root = document.RootElement
            match requiredString root "schema", requiredString root "payloadDigest" with
            | Ok schema, Ok digest when schema = "sir-rules-governance/v1" ->
                let payloadElement = root.GetProperty("payload")
                let package = payloadElement.GetProperty("package")
                let packageFields = [ "engineIdentity"; "compatibilityProfile"; "packageVersion"; "sourceCommit"; "implementationDigest"; "semanticDigest"; "manifestDigest" ] |> List.map (requiredString package) |> sequence
                let rules =
                    payloadElement.GetProperty("rules").EnumerateArray() |> Seq.map (fun rule ->
                        match requiredString rule "ruleId", requiredString rule "title", requiredString rule "status", requiredString rule "semanticKind", parseStrings rule "dependencies", parseStrings rule "supersedes", parseEvidenceArray rule "evidence" with
                        | Ok id, Ok title, Ok status, Ok kind, Ok dependencies, Ok supersedes, Ok evidence -> Ok { RuleId = id; Title = title; Status = status; SemanticKind = kind; Dependencies = dependencies; Supersedes = supersedes; SourcePath = optionalString rule "sourcePath"; SourceSymbol = optionalString rule "sourceSymbol"; HasXmlDocumentation = rule.GetProperty("hasXmlDocumentation").GetBoolean(); Evidence = evidence }
                        | Error error, _, _, _, _, _, _ | _, Error error, _, _, _, _, _ | _, _, Error error, _, _, _, _ | _, _, _, Error error, _, _, _ | _, _, _, _, Error error, _, _ | _, _, _, _, _, Error error, _ | _, _, _, _, _, _, Error error -> Error error) |> Seq.toList |> sequence
                match packageFields, rules, parseEvidenceArray payloadElement "surface", parseEvidenceArray payloadElement "evidence", requiredString payloadElement "legacyClassification" with
                | Ok [ engine; profile; version; commit; implementation; semantic; manifest ], Ok rules, Ok surface, Ok evidence, Ok legacy ->
                    let envelope = { Schema = schema; PayloadDigest = digest; Payload = { Package = { EngineIdentity = engine; CompatibilityProfile = profile; PackageVersion = version; SourceCommit = commit; ImplementationDigest = implementation; SemanticDigest = semantic; ManifestDigest = manifest }; Rules = rules; Surface = surface; Evidence = evidence; LegacyClassification = legacy } }
                    verify envelope
                | Error error, _, _, _, _ | _, Error error, _, _, _ | _, _, Error error, _, _ | _, _, _, Error error, _ | _, _, _, _, Error error -> Error error
                | _ -> Error "invalid package binding"
            | Ok _, Ok _ -> Error "unsupported receipt schema"
            | Error error, _ | _, Error error -> Error error
        with error -> Error("malformed receipt: " + error.Message)

module private Checks =
    let receiptRef: ArtifactRef = { Kind = "sir-rules-governance"; Key = "receipt" }
    let surfaceRef: ArtifactRef = { Kind = "fsharp-surface"; Key = "rules" }
    let semanticRef: ArtifactRef = { Kind = "semantic-evidence"; Key = "rules" }
    let parityRef: ArtifactRef = { Kind = "runtime-parity"; Key = "dotnet-fable-node-browser" }
    let viewRef: ArtifactRef = { Kind = "generated-view"; Key = "rules" }
    let replayRef: ArtifactRef = { Kind = "historical-replay"; Key = "rules" }
    let journeyRef: ArtifactRef = { Kind = "production-journey"; Key = "rules" }
    let packageRef: ArtifactRef = { Kind = "package-binding"; Key = "rules" }
    let receipt (facts: FactSet<GovernanceFact>) = facts |> List.tryPick (fun fact -> match fact.Value with ReceiptFact value -> Some value | _ -> None)
    let evidence (kind: string) (envelope: ReceiptEnvelope) = envelope.Payload.Evidence @ envelope.Payload.Surface |> List.filter (fun item -> item.Kind = kind)
    let stateOutcome (label: string) (items: EvidenceRef list) =
        if List.isEmpty items then Unknown(label + " missing")
        elif items |> List.exists (fun item -> item.State = Malformed) then Unknown(label + " malformed")
        elif items |> List.exists (fun item -> item.State = Missing || item.State = Stale || item.State = Unavailable || item.State = Synthetic) then Unknown(label + " unavailable, stale, missing, or synthetic")
        elif items |> List.exists (fun item -> item.State = CurrentFail) then Unmet(label + " failed")
        else Met
    let boundStateOutcome (label: string) (receipt: ReceiptEnvelope) (items: EvidenceRef list) =
        match stateOutcome label items with
        | Met when items |> List.forall (fun item -> item.Digest.IsSome && item.PackageManifestDigest = Some receipt.Payload.Package.ManifestDigest && item.SemanticDigest = Some receipt.Payload.Package.SemanticDigest) -> Met
        | Met -> Unmet(label + " is not bound to the receipt package and semantic identities")
        | other -> other
    let probe (name: string) (reads: ArtifactRef list) (eval: ReceiptEnvelope -> Outcome) = Check.probe name reads [] (fun facts -> match receipt facts with None -> Unknown "receipt unavailable" | Some envelope -> eval envelope)
    let rule (id: string) (maturity: Maturity) (check: Check<GovernanceFact>) =
        let built = CheckRule.rule (FS.GG.Governance.Kernel.RuleId id) Deterministic { Document = "sir-rules-governance/v1"; Section = id } check |> Result.defaultWith (fun error -> failwithf "%A" error)
        { Id = id; Maturity = maturity; Rule = if maturity = Warn then built else CheckRule.blocking built }
    let all =
        [ rule "metadata-complete" Warn (probe "metadata-complete" [ surfaceRef ] (fun receipt -> if receipt.Payload.Rules |> List.forall (fun item -> item.HasXmlDocumentation && not (String.IsNullOrWhiteSpace item.Title)) then Met else Unmet "signature, XML, or descriptive metadata incomplete"))
          rule "legacy-classified" Warn (probe "legacy-classified" [ receiptRef ] (fun receipt ->
              match receipt.Payload.LegacyClassification with
              | "none" -> Met
              | "legacy" -> Unmet "legacy mechanics remain explicitly classified outside canonical authority"
              | _ -> Unknown "legacy authority boundary is not explicitly classified"))
          rule "receipt-well-formed" BlockOnPr (probe "receipt-well-formed" [ receiptRef ] (fun receipt -> match Receipt.verify receipt with Ok _ -> Met | Error error -> Unknown error))
          rule "rule-identities-valid" BlockOnPr (probe "rule-identities-valid" [ receiptRef ] (fun receipt ->
              let ids = receipt.Payload.Rules |> List.map _.RuleId
              let statuses = Set [ "proposed"; "prototype"; "canonical"; "deprecated"; "superseded" ]
              let kinds = Set [ "fact"; "predicate"; "formula"; "transition"; "algorithm"; "narrative" ]
              let authoritative = receipt.Payload.Evidence |> List.filter (fun item -> Set [ "corpus-manifest"; "corpus-coverage"; "corpus-implementation" ] |> Set.contains item.Kind)
              if ids.Length = 16
                 && (ids |> List.distinct |> List.length) = ids.Length
                 && receipt.Payload.Rules |> List.forall (fun item ->
                     item.Dependencies |> List.forall (fun dependency -> List.contains dependency ids)
                     && statuses.Contains item.Status
                     && kinds.Contains item.SemanticKind
                     && item.SourcePath |> Option.exists (fun path -> path.EndsWith(".fs", StringComparison.Ordinal))
                     && item.SourceSymbol |> Option.exists (String.IsNullOrWhiteSpace >> not))
                 && authoritative.Length = 3
                 && boundStateOutcome "authoritative corpus validation" receipt authoritative = Met then Met
              else Unmet "duplicate, dangling, incomplete, invalid, or non-F# rule metadata"))
          rule "surface-current" BlockOnPr (probe "surface-current" [ surfaceRef ] (fun receipt -> boundStateOutcome "public surface" receipt receipt.Payload.Surface))
          rule "generated-views-current" BlockOnPr (probe "generated-views-current" [ viewRef ] (fun receipt -> evidence "generated-view" receipt |> boundStateOutcome "generated view" receipt))
          rule "semantic-evidence-current" BlockOnShip (probe "semantic-evidence-current" [ semanticRef ] (fun receipt -> evidence "semantic" receipt |> boundStateOutcome "semantic evidence" receipt))
          rule "runtime-parity-equal" BlockOnShip (probe "runtime-parity-equal" [ parityRef ] (fun receipt ->
              let items = evidence "runtime-parity" receipt
              match stateOutcome "runtime parity" items with
              | Met ->
                  if items.Length >= 3
                     && items |> List.forall (fun item -> item.Digest.IsSome && item.PackageManifestDigest = Some receipt.Payload.Package.ManifestDigest && item.SemanticDigest = Some receipt.Payload.Package.SemanticDigest) then Met
                  else Unmet "runtime identities diverge"
              | other -> other))
          rule "package-identities-consistent" BlockOnShip (probe "package-identities-consistent" [ packageRef ] (fun receipt ->
              let current = receipt.Payload.Evidence @ receipt.Payload.Surface |> List.filter (fun item -> item.State = CurrentPass)
              if not (List.isEmpty current)
                 && current |> List.forall (fun item -> item.PackageManifestDigest = Some receipt.Payload.Package.ManifestDigest && item.SemanticDigest = Some receipt.Payload.Package.SemanticDigest) then Met
              else Unmet "package or digest inconsistency"))
          rule "historical-replay-exact" BlockOnShip (probe "historical-replay-exact" [ replayRef ] (fun receipt -> evidence "historical-replay" receipt |> boundStateOutcome "historical replay package" receipt))
          rule "production-journey-present" BlockOnShip (probe "production-journey-present" [ journeyRef ] (fun receipt -> evidence "production-journey" receipt |> boundStateOutcome "production journey" receipt)) ]

[<RequireQualifiedAccess>]
module Policy =
    let private identify (fact: GovernanceFact) =
        match fact with
        | ReceiptFact receipt -> FactId("receipt:" + receipt.PayloadDigest)
        | GovernanceOutcome(Decided(FS.GG.Governance.Kernel.RuleId id, _)) -> FactId("governance:decided:" + id)
        | GovernanceOutcome(NeedsReview request) -> FactId("governance:review:" + request.Key)
        | GovernanceOutcome(Reviewed review) -> FactId("governance:reviewed:" + review.Key)
        | GovernanceOutcome(Escalated(FS.GG.Governance.Kernel.RuleId id)) -> FactId("governance:escalated:" + id)
    let checks = Checks.all
    let adapter =
        { Identify = identify
          ToRef = function Receipt -> Checks.receiptRef | RuleManifest -> { Kind = "rule-manifest"; Key = "rules" } | PublicSurface -> Checks.surfaceRef | SemanticEvidence -> Checks.semanticRef | RuntimeParity -> Checks.parityRef | GeneratedView -> Checks.viewRef | ReplayPackage -> Checks.replayRef | ProductionJourney -> Checks.journeyRef
          Probes = checks |> List.choose (fun check -> match check.Rule.Check with Atom probe -> Some probe | _ -> None)
          Rules = checks |> List.map _.Rule
          Fences = [ { Name = "rules-governance"; Trips = fun change -> change.Paths |> Set.exists (fun path -> path.StartsWith("src/SIR.Domain", StringComparison.Ordinal) || path.StartsWith("src/SIR.Simulation", StringComparison.Ordinal) || path.StartsWith("readiness/198-", StringComparison.Ordinal)) } ]
          Bridge =
            { Judge = { ModelId = "sir-rules-governance"; Version = "1" }
              ArtifactHash = fun facts _ -> match Checks.receipt facts with Some receipt -> receipt.PayloadDigest | None -> ""
              Embed = GovernanceOutcome
              Project = function GovernanceOutcome outcome -> Some outcome | _ -> None } }
    let private applies (boundary: Boundary) (profile: EnforcementProfile) (maturity: Maturity) =
        match profile, boundary, maturity with
        | Strict, PullRequest, _ -> true
        | Migration, _, Warn -> false
        | _, PullRequest, BlockOnPr -> true
        | _, Ship, (BlockOnPr | BlockOnShip) -> true
        | Strict, Ship, Warn -> true
        | _ -> false
    let evaluate (boundary: Boundary) (profile: EnforcementProfile) (receipt: ReceiptEnvelope) =
        let facts = [ { Id = FactId("receipt:" + receipt.PayloadDigest); Value = ReceiptFact receipt; Provenance = [] } ]
        let evaluation = FixedPoint.evaluate adapter.Identify (Adapter.toRules adapter) facts
        let decided (governed: GovernedCheck) =
            evaluation.Facts
            |> List.tryPick (fun assertion ->
                match assertion.Value with
                | GovernanceOutcome(Decided(ruleId, verdict)) when ruleId = governed.Rule.Id -> Some(verdict, assertion.Provenance)
                | _ -> None)
            |> Option.defaultValue (Uncertain "governance fixed point did not derive a decision", [])
        let findings =
            checks |> List.sortBy _.Id |> List.map (fun governed ->
                let verdict, provenance = decided governed
                let nonPass = match verdict with Pass -> false | _ -> true
                { Id = governed.Id; Maturity = governed.Maturity; Verdict = verdict; Rendered = Check.render governed.Rule.Check; StructuralHash = Check.hash governed.Rule.Check; Reads = Check.reads governed.Rule.Check; ExplanationJson = Json.ofExplanation (Check.explain facts governed.Rule.Check); Provenance = provenance; ReceiptDigest = receipt.PayloadDigest; EffectiveBlocking = nonPass && applies boundary profile governed.Maturity })
        { Schema = "sir-rules-governance-verdict/v1"; ReceiptDigest = receipt.PayloadDigest; Boundary = boundary; Profile = profile; Findings = findings; Blocked = findings |> List.exists _.EffectiveBlocking }
    let encodeVerdict (verdict: GovernanceVerdict) =
        Canonical.withWriter (fun writer ->
            writer.WriteStartObject()
            writer.WriteString("schema", verdict.Schema)
            writer.WriteString("receiptDigest", verdict.ReceiptDigest)
            writer.WriteString("boundary", Canonical.boundary verdict.Boundary)
            writer.WriteString("profile", Canonical.profile verdict.Profile)
            writer.WriteBoolean("blocked", verdict.Blocked)
            writer.WriteStartArray("findings")
            verdict.Findings
            |> List.sortBy _.Id
            |> List.iter (fun finding ->
                let tag, reason = Canonical.verdict finding.Verdict
                writer.WriteStartObject()
                writer.WriteString("id", finding.Id)
                writer.WriteString("maturity", Canonical.maturity finding.Maturity)
                writer.WriteString("verdict", tag)
                Canonical.option writer "reason" reason
                writer.WriteBoolean("effectiveBlocking", finding.EffectiveBlocking)
                writer.WriteString("rendered", finding.Rendered)
                writer.WriteString("structuralHash", finding.StructuralHash)
                writer.WriteStartArray("reads")
                finding.Reads
                |> List.sortBy (fun item -> item.Kind, item.Key)
                |> List.iter (fun item ->
                    writer.WriteStartObject()
                    writer.WriteString("kind", item.Kind)
                    writer.WriteString("key", item.Key)
                    writer.WriteEndObject())
                writer.WriteEndArray()
                writer.WritePropertyName("explanation")
                use explanation = JsonDocument.Parse(finding.ExplanationJson)
                explanation.RootElement.WriteTo(writer)
                writer.WriteStartArray("provenance")
                finding.Provenance
                |> List.iter (fun step ->
                    let (FS.GG.Governance.Kernel.RuleId ruleId) = step.Rule
                    writer.WriteStartObject()
                    writer.WriteString("rule", ruleId)
                    writer.WriteStartArray("inputs")
                    step.Inputs |> List.iter (fun (FactId input) -> writer.WriteStringValue(input))
                    writer.WriteEndArray()
                    writer.WriteString("note", step.Note)
                    writer.WriteEndObject())
                writer.WriteEndArray()
                writer.WriteString("receiptDigest", finding.ReceiptDigest)
                writer.WriteEndObject())
            writer.WriteEndArray()
            writer.WriteEndObject())
    let joinProtectedBoundary (sddShipArtifact: string) (sddShipBytes: byte array) (sddReady: bool) (governanceVerdictArtifact: string) (verdictBytes: byte array) (governanceBlocked: bool) =
        { Schema = "sir-rules-protected-boundary/v1"; SddShipArtifact = sddShipArtifact; SddShipDigest = Canonical.sha256 sddShipBytes; GovernanceVerdictArtifact = governanceVerdictArtifact; GovernanceVerdictDigest = Canonical.sha256 verdictBytes; SddReady = sddReady; GovernanceBlocked = governanceBlocked; Allowed = sddReady && not governanceBlocked }
    let encodeProtectedBoundary (boundary: ProtectedBoundary) =
        Canonical.withWriter (fun writer -> writer.WriteStartObject(); writer.WriteString("schema", boundary.Schema); writer.WriteString("sddShipArtifact", boundary.SddShipArtifact); writer.WriteString("sddShipDigest", boundary.SddShipDigest); writer.WriteString("governanceVerdictArtifact", boundary.GovernanceVerdictArtifact); writer.WriteString("governanceVerdictDigest", boundary.GovernanceVerdictDigest); writer.WriteBoolean("sddReady", boundary.SddReady); writer.WriteBoolean("governanceBlocked", boundary.GovernanceBlocked); writer.WriteBoolean("allowed", boundary.Allowed); writer.WriteEndObject())
