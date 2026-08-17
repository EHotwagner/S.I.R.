namespace SIR.Rules.Governance

open SIR.Domain
open FS.GG.Governance.Kernel
open FS.GG.Governance.Adapters.Spi

type EvidenceState = CurrentPass | CurrentFail | Missing | Malformed | Stale | Synthetic | Unavailable
type Maturity = Warn | BlockOnPr | BlockOnShip
type Boundary = PullRequest | Ship
type EnforcementProfile = Migration | Standard | Strict
type RuntimeKind = DotNet | FableNode | Browser

type EvidenceRef =
    { Kind: string
      Artifact: string
      State: EvidenceState
      Digest: string option
      PackageManifestDigest: string option
      SemanticDigest: string option }

type RuleReceipt =
    { RuleId: string
      Title: string
      Status: string
      SemanticKind: string
      Dependencies: string list
      Supersedes: string list
      SourcePath: string option
      SourceSymbol: string option
      HasXmlDocumentation: bool
      Evidence: EvidenceRef list }

type PackageBinding =
    { EngineIdentity: string
      CompatibilityProfile: string
      PackageVersion: string
      SourceCommit: string
      ImplementationDigest: string
      SemanticDigest: string
      ManifestDigest: string }

type ReceiptPayload =
    { Package: PackageBinding
      Rules: RuleReceipt list
      Surface: EvidenceRef list
      Evidence: EvidenceRef list
      LegacyClassification: string }

type ReceiptEnvelope =
    { Schema: string
      PayloadDigest: string
      Payload: ReceiptPayload }

type GovernedArtifact = Receipt | RuleManifest | PublicSurface | SemanticEvidence | RuntimeParity | GeneratedView | ReplayPackage | ProductionJourney
type GovernedChange = { Paths: Set<string> }
type GovernanceFact = ReceiptFact of ReceiptEnvelope | GovernanceOutcome of RuleOutcome

type GovernedCheck =
    { Id: string
      Maturity: Maturity
      Rule: CheckRule<GovernanceFact> }

type Finding =
    { Id: string
      Maturity: Maturity
      Verdict: Verdict
      Rendered: string
      StructuralHash: string
      Reads: ArtifactRef list
      ExplanationJson: string
      Provenance: ProvenanceStep list
      ReceiptDigest: string
      EffectiveBlocking: bool }

type GovernanceVerdict =
    { Schema: string
      ReceiptDigest: string
      Boundary: Boundary
      Profile: EnforcementProfile
      Findings: Finding list
      Blocked: bool }

type ProtectedBoundary =
    { Schema: string
      SddShipArtifact: string
      SddShipDigest: string
      GovernanceVerdictArtifact: string
      GovernanceVerdictDigest: string
      SddReady: bool
      GovernanceBlocked: bool
      Allowed: bool }

[<RequireQualifiedAccess>]
module Receipt =
    val create: package: RulePackageIdentity -> rules: RuleDefinition list -> surface: EvidenceRef list -> evidence: EvidenceRef list -> legacyClassification: string -> ReceiptEnvelope
    val payloadBytes: ReceiptPayload -> byte array
    val encode: ReceiptEnvelope -> byte array
    val decode: byte array -> Result<ReceiptEnvelope, string>
    val verify: ReceiptEnvelope -> Result<ReceiptEnvelope, string>

[<RequireQualifiedAccess>]
module Policy =
    val adapter: Adapter<GovernanceFact, GovernedArtifact, GovernedChange>
    val checks: GovernedCheck list
    val evaluate: boundary: Boundary -> profile: EnforcementProfile -> receipt: ReceiptEnvelope -> GovernanceVerdict
    val encodeVerdict: GovernanceVerdict -> byte array
    val joinProtectedBoundary: sddShipArtifact: string -> sddShipBytes: byte array -> sddReady: bool -> governanceVerdictArtifact: string -> verdictBytes: byte array -> governanceBlocked: bool -> ProtectedBoundary
    val encodeProtectedBoundary: ProtectedBoundary -> byte array
