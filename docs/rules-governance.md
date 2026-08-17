# Rules governance receipts

S.I.R.'s executable F# rule corpus remains the gameplay authority. The standalone
`SIR.Rules.Governance` adapter observes that corpus through a versioned receipt and
uses the published `FS.GG.Governance.Kernel` and `FS.GG.Governance.Adapters.Spi`
0.1.1 packages to make policy decisions. Neither gameplay runtime project references
Governance.

## Contract and artifacts

`sir-rules-governance/v1` is a canonical, content-addressed JSON envelope. Its
`payloadDigest` is the lower-case SHA-256 of canonical payload bytes. Arrays with set
semantics are sorted before encoding, so input enumeration order cannot change the
receipt. The package binding records engine/profile/version/source commit plus
implementation, semantic, and manifest digests. Each evidence reference records its
artifact digest and the package manifest and semantic identities it claims.

The generated artifacts are:

- `readiness/198-rules-governance-receipts/rules-governance.json`
- `readiness/198-rules-governance-receipts/rules-governance-verdict.json`

Run `./scripts/generate-rules-governance.sh --write` after authoritative evidence
changes and `./scripts/generate-rules-governance.sh --check` to prove the committed
artifacts are current. The latter is the `rules-governance` repository policy command.

## Facts, checks, and enforcement

Evidence state is a closed union: `current-pass`, `current-fail`, `missing`,
`malformed`, `stale`, `synthetic`, or `unavailable`. Required evidence never passes
by omission; missing or unreadable input produces a visible unknown finding. The
adapter maps the receipt into a closed fact union and derives decisions through the
Governance fixed-point kernel. Every finding emits the same check's rendered text,
declared reads, structural hash, explanation, and kernel provenance.

Migration-only metadata and explicitly remaining legacy mechanics warn. Receipt,
identity, surface, and generated-view failures block pull requests. Semantic,
cross-runtime, package-identity, historical-replay, and production-journey failures
block shipping. Migration, standard, and strict profiles change only effective
blocking; they preserve the underlying three-valued verdict.

SDD and Governance are separate authorities. `fsgg-sdd ship` emits SDD readiness;
the adapter emits the Governance verdict. The `sir-rules-protected-boundary/v1` join
records the two source artifact paths and distinct content digests, and allows the
boundary only when SDD is ready and Governance is not blocked.
