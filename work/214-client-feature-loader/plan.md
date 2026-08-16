---
schemaVersion: 1
workId: 214-client-feature-loader
title: Client Feature Loader
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/214-client-feature-loader/spec.md
sourceClarifications: work/214-client-feature-loader/clarifications.md
sourceChecklist: work/214-client-feature-loader/checklist.md
publicOrToolFacingImpact: true
---

# Client Feature Loader Plan

Prose status: planned

## Source Snapshot
- spec: work/214-client-feature-loader/spec.md sha256:b08dc0183a365dbdebc8284c60934ed769627fc6d08ce7fd5d1dbd322b8a628e schemaVersion:1
- clarifications: work/214-client-feature-loader/clarifications.md sha256:546c514ed53c669f413e656a43ecf8b66deb7dbb4111aa87692600e5a68df4da schemaVersion:1
- checklist: work/214-client-feature-loader/checklist.md sha256:63c16b2e69fea7cbaf546dfbb96672e717c16013d9b4d6d28f7678dd6529cbb3 schemaVersion:1

## Plan Scope
- Add an explicitly signed F# `FeatureLoader` state/request surface before wiring it into `AppTypes.Model` and `Msg`.
- Keep registry v1 in one declarative JSON source; validate it before Vite and consume it from the browser/import edge and post-build graph gate.
- Keep Tactical Environment eagerly available, move Docs behind a small literal dynamic-import target, and route Rules Explorer preloading/completion through the loader state before rendering its existing lazy component.
- Extend the existing production-delivery gate rather than adding a parallel build pipeline; it validates source edges/minifier policy, budgets, mutations, and emits one normalized graph receipt from the already-built artifact.
- Add focused compiled-Fable state, Node, and browser tests while editing, then freeze sources and run one aggregate production build/browser acceptance.

## Technical Context
F# 10/Fable 5.13, Elmish/Feliz, Vite 8.1.5/Rolldown, Node 26, and Playwright. Dynamic import remains a literal JavaScript edge because Fable must not synthesize or reflect import targets.

## Constitution Check
- III Public Surface: add `FeatureLoader.fsi` before implementation and keep public identities/messages explicit.
- V Model–Update–Effect: pure request/result reconciliation in F#; JavaScript import and browser focus are interpreted effects.
- VI Test Evidence: focused state/gate tests, real production browser controls, and eager/missing/stale mutations.
- VIII Safe Failure: missing chunks and stale identities produce stable named failures without replacing current loaded state.

## Design
- `feature-registry.v1.json` is authoritative for schema/version, feature/phase/control/module/logical-chunk ids, and raw/gzip/Brotli budgets.
- `FeatureLoader.fsi/.fs` owns `FeatureId`, `ChunkIdentity`, `LoadRequest`, `LoadFailure`, `LoadState`, and pure `request/complete/fail` transitions.
- `feature-loader.js` is the only literal dynamic-import map. It validates registry/request identity and returns an identity-bearing promise result; no computed URL, `eval`, inline script, or property mangling is used.
- App update requests deferred features from the actual View → Rules data and toolbar Docs controls. The view renders stable loading/failure/loaded regions and ignores stale completion.
- `scripts/test-client-feature-loader.mjs` validates registry/source reachability, Vite minifier posture, built manifest/chunks, per-route budgets, graph normalization, and content-addressed receipt identity. It consumes a build and never builds.
- The receipt is written to `docs/evidence/client-feature-bundle-graph-v1/<sha256>.json`; canonical JSON excludes time, duration, absolute path, and feedback metadata.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Author registry schema v1 plus signed F# identities/state first; JSON is the build authority and F# constants are checked projections, so divergence is a named gate failure.
- PD-002 [AC-002] [FR-002] [DEC-002] complete: Add pure identity-bound request/success/failure transitions and Elmish messages; only matching pending identity changes state, while stale completion preserves current state and records a diagnostic.
- PD-003 [AC-003] [FR-003] [DEC-002] complete: Use a closed literal import switch in `feature-loader.js`, relative Vite-resolved targets, and typed missing/offline failure mapping; never construct an import URL from runtime text.
- PD-004 [AC-004] [FR-004] [DEC-003] complete: Retain #154 initial and Rules Explorer raw/gzip/Brotli ceilings, add explicit Docs limits after focused observation, and enforce every registry budget against emitted bytes.
- PD-005 [AC-004] [FR-005] complete: Keep `terserOptions.mangle.properties` absent/false and make the source gate reject any property-mangling configuration before artifact validation.
- PD-006 [AC-001] [AC-002] [FR-006] [DEC-001] complete: Register shell/bootstrap, tactical-environment/eager, delivery-support/deferred, rules-explorer/deferred, and docs/deferred; source validation rejects feature module imports not owned by the registry/import edge.
- PD-007 [AC-004] [FR-007] [DEC-005] complete: Parse Fable/Vite import edges and compare the complete emitted dynamic-entry inventory with registry ownership, then fail when a deferred module is statically reachable, unregistered, or absent from its declared logical chunk.
- PD-008 [AC-002] [FR-008] [DEC-004] complete: Extend Playwright acceptance to assert shell readiness before deferred requests, then use real Data, Docs, and Editor→Environment controls to reach named production regions.
- PD-009 [AC-005] [FR-009] complete: Provide protected environment mutations `eager-import`, `missing-chunk`, and `stale-identity`; the eager mutation temporarily adds a real static Docs import to the production delivery-support entry, invokes the unchanged source gate, and restores the subject, while every mutation requires a non-zero result containing its subject.
- PD-010 [AC-003] [FR-010] [DEC-005] complete: Sort and canonically serialize registry/source digests, logical/emitted edges, per-route membership, compressed bytes, and SHA-256 content identities; name the receipt by the digest of those exact bytes.
- PD-011 [AC-006] [FR-011] [DEC-005] complete: Keep edit-time commands build-free where possible; after source freeze build once, run Node/F#/browser evidence against that artifact, and keep feedback paths outside product-build workflow filters.

## Contract Impact
- PC-001 [PD-001] feature registry contract: `contracts/client-feature-registry-v1.md` defines schema, phase, identity, budgets, migration, and registry/projection precedence.
- PC-002 [PD-002] [PD-003] loader surface: `src/SIR.Client.Web/FeatureLoader.fsi` declares stable public state and pure transition signatures; the JavaScript edge returns only identity-bearing results.
- PC-003 [PD-010] bundle graph receipt: `contracts/client-feature-bundle-graph-v1.md` defines canonical ordering, digest, exclusions, and content-addressed path.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PC-001] [PC-002] semanticTest: Run focused F# loader transition tests covering request, success, missing failure, and stale identity rejection.
- VO-002 [PD-004] [PD-005] [PD-007] [PC-001] gateTest: Run the build-free registry/source gate and post-build budget/graph gate, then invert eager reachability, property mangling, and budgets to prove red.
- VO-003 [PD-008] [PC-002] browserTest: Run the published Release server in Playwright, observe shell-first requests, and activate all three user features through real controls.
- VO-004 [PD-009] mutationTest: Run eager-import, missing-chunk, and stale-identity protected mutations and require subject-specific red diagnostics.
- VO-005 [PD-010] [PD-011] [PC-003] reproducibilityTest: Run graph generation twice on the frozen artifact and compare exact bytes/path; alter feedback-only metadata and prove the build/receipt input digest is unchanged.

## Performance Intent
The loader preserves the #154 delivery SLO by retaining the 1,250,000 raw /
320,000 gzip / 280,000 Brotli initial ceiling and 65,536 / 20,000 / 16,000
Rules Explorer ceiling. Baseline and source-frozen measurements are recorded in
the content-addressed receipt; structural request ordering replaces timing
assertions for shell-first correctness.

## Migration Posture
- PM-001 [PC-001] compatibility: Registry schema v1 is additive; missing/unsupported schema or unknown phase fails before build, while existing shell and retained-engine identities remain unchanged.
- PM-002 [PC-002] compatibility: Existing panel controls keep their visible semantics; deferred loader failure adds an actionable state rather than silently hiding the panel.

## Generated View Impact
- GV-001 [PD-010] bundleGraph: The graph receipt is generated only from the frozen build and its canonical sources; stale/missing chunks, registry digest, or source digest fail rather than being regenerated from prose.
- GV-002 [PD-011] workModel: SDD analysis/evidence/verify/ship views refresh after authored sources and receipts are final; feedback-only metadata is not a product build input.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- The producer-owned #154 loading SLO is inherited; no compositor/timing claim is added. Baseline on `b3449b2`: app 1,198,954 raw / 278,944 gzip / 222,131 Brotli and Rules Explorer 62,976 / 18,465 / 15,974 bytes.

## Tests
- Focused: compiled-Fable loader transition tests and build-free registry/source validation.
- Artifact: one Vite build, deterministic bundle/budget validator, exact-byte repeat.
- Browser: one production journey through shell, Rules Explorer, Docs, and Tactical Environment controls.
- Aggregate: existing conformance plus this item once after source freeze; one hosted final CI run on the reviewed exact SHA.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 214-client-feature-loader`.
