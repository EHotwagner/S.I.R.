# Production qualification receipts

Production qualification is single-pass within one workspace:

```sh
npm run qualify:production
```

The driver runs the existing conformance, delivery, and browser subjects, builds the main Fable client and Rules Explorer exactly once, creates and verifies an immutable build receipt, then runs the complete documentation build with verified reuse. The two independent production Fable targets run concurrently into isolated output and log paths; their failures are joined in deterministic Replay/Rules order, and Vite starts only after both succeed. A PATH-boundary `dotnet` shim observes every Fable process and refuses any inventory other than the exact four-project qualification set (the two conformance projects plus the two production projects), each exactly once, independent of parallel scheduling order. The documentation command remains independently usable: `npm run build:docs` performs its historical full restore/build/Fable/Vite path when no receipt is supplied.

## Build receipt v1

`scripts/production-build-receipt.mjs create` emits canonical LF-terminated JSON with schema `sir.production-build-receipt/v1` under `docs/evidence/production-build-receipt-v1/<sha256>.json`. The filename is the SHA-256 of the exact receipt bytes. The receipt binds:

- the producing Git commit, tree, and tracked-clean state;
- every enumerated tracked source, project, script, package configuration, and dependency-lock input;
- Git, .NET SDK, Fable, fsdocs, Node, npm, and Vite versions;
- the owning command identity;
- sorted relative file paths, byte sizes, and SHA-256 identities for main Fable, Rules Explorer, and the production client bundle.

Verification is read-only. It re-derives every field and fails on revision/tree, tracked state, source/configuration/lock, tool, command, expected path, missing output, or output-content drift. It never refreshes a stale receipt or rebuilds an output.

After conformance succeeds, the owner creates a second focused receipt over the build receipt, test sources and fixtures, browser and delivery JUnit, client-feature TRX, and bundle-graph receipts. `build-docs.sh --reuse-build-receipt <path> --reuse-conformance-receipt <path>` verifies both receipts before reuse. It skips the restore/solution/Fable/Vite work and map/planning/simulation/review/timeline/workspace gates already attested by conformance. The docs-only client assembly, fsdocs, publication, documentation experience, browser smoke, and accessibility checks still run.

After those documentation checks pass, the owner creates a third receipt that links the build and conformance receipts and binds every published file under `artifacts/site`. Qualification mutates and removes a real site file in separate self-restoring probes; unchanged verification must reject both states before the original bytes are restored.

## Feedback without an aggregate loop

After the source-frozen aggregate, feedback report, audit, checkpoint, SDD work, readiness, and the immutable receipt itself may be committed as metadata. Re-verify the focused receipt at the audit head:

```sh
dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- \
  validate-focused-receipt \
  --receipt docs/evidence/production-build-receipt-v1/<sha256>.json \
  --owner-command scripts/qualify-production.sh \
  --allow-metadata-only true
```

Metadata-only reuse requires the producing commit to be an ancestor and rejects any intervening path outside `feedback/`, `work/`, `readiness/`, or the immutable receipt directory. A product/configuration/lock/script change therefore requires a new aggregate and new receipt; report or audit prose does not.

## Mutation and timing

The aggregate runs stale and missing subject mutations, requires the production verifier's `output-identity-drift` refusal, and restores the original bytes in an unconditional cleanup boundary. The driver also records baseline/candidate wall milliseconds, exact commit/tree/clean identities, one shared host fingerprint, Fable target build counts, all three receipts, retained subject inventory, and reduction basis points under `artifacts/qualification/single-pass-timing.json`. The committed paired baseline receipt must match the candidate host byte-for-byte; a different host or an unclean source is refused before timing begins. Timing is acceptance evidence, not a runtime correctness threshold.
