---
feedbackSchema: 2
date: 2026-08-16
workspace: S.I.R
cycle: item-185-in-application-docs
lane: sdd
toolVersion: 1.0.1
commit: 1f1120ac9ae5cd3174fa9f916b506945940fa333
---

## §1 Provenance and confidence

- **activation:** active; this is the recovery addendum after `feedback/item-185-in-application-docs-2.md`.
- **phases:** implementation-test-evidence, verify-ship-pr.
- **material events:** 8 total checkpoints; this addendum owns the loader-contract event, the historical-audit invalidation boundary, and the recovery from the previously blocked PR head.
- **checkpoint:** `feedback/checkpoints/item-185-in-application-docs.jsonl` (8 records at the described commit).
- **confidence limits:** Release F# compilation, Fable/Vite production output, the focused production browser journeys, feature-loader source/compiled/bundle/budget mutations, actual headless-browser DOM and construction timing, rules-corpus source binding, and SDD 1.0.1 evidence/verify/ship were exercised. No live compositor was available. Hosted CI, fresh exact-head product review, merge, and post-merge checks remain outside this report.

## §2 What worked

The per-feature loader registry provided the stable repair boundary the recovery needed. The real documentation renderer is now one deferred `DocsView` entry behind a typed bootstrap contract, while registry v1 remains byte-identical to `origin/main`. Registry v2 changes the Docs module identity and only the Rules Explorer Brotli ceiling, from 16,000 to 16,384 bytes after a 16,040-byte observation. The Docs chunk itself measured 2,135 Brotli bytes under its unchanged 3,072-byte ceiling. Focused source, compiled-state, bundle-graph, browser-control, and six mutation subjects passed without another broad size optimization.

## §3 What did not

The recovered branch initially exposed two composition errors: a lazy React component received positional names that did not match the deferred component's generated properties, and an obsolete loader toolbar duplicated the canonical tactical Docs control. Typed `Presentation` and `Callbacks` records repaired the generated-property boundary, and removing the obsolete toolbar left one production entry control. The pre-existing 16,000-byte Rules Explorer Brotli ceiling then failed by 40 bytes because shared-chunk composition changed; optimizing unrelated source would have repeated the invasive size response that motivated this recovery.

The consumer's bundled feedback tool predates producer fix FS.GG.Rendering#1243 and still indexes candidate-only audits from the working tree, overreporting 19 invalidations. The fixed producer implementation indexes `origin/main` and reports five genuine base-present item-186 bindings invalidated by the intentional Docs changes. Those old evidence bindings are not current; this recovery records the boundary additively and does not rewrite merged historical reports.

## §4 Findings

#### §4.1 Versioned per-feature budgets contained shared-chunk drift without an unrelated optimization

- **Kind:** positive-pattern
- **Impact:** Future optional features can evolve behind owned chunks and explicit registry versions without forcing the bootstrap ceiling, one global forever-limit, or opportunistic rewrites of unrelated features.
- **Expected:** A new deferred Docs renderer should own its emitted chunk and any neighboring shared-chunk drift should be visible and dispositioned at the affected route boundary.
- **Observed:** Docs emitted as one 2,135-byte Brotli `DocsView` chunk under the unchanged 3,072-byte ceiling. Rules Explorer emitted at 16,040 bytes, 40 bytes above registry v1; registry v1 was preserved and registry v2 rebaselined only that route to 16,384. The registry-version and budget mutations both failed with their named diagnostics before the aggregate recovered green.
- **Evidence:** command:git diff --exit-code origin/main -- src/SIR.Client.Web/feature-registry.v1.json; command:node scripts/test-client-feature-loader.mjs --source-only; command:node scripts/test-client-feature-loader.mjs --no-write; command:node scripts/test-client-feature-loader.mjs --aggregate-trx readiness/185-in-app-docs-modality/client-feature-loader.trx --browser-junit readiness/185-in-app-docs-modality/in-app-docs-browser.junit.xml; file:src/SIR.Client.Web/feature-registry.v2.json; file:scripts/test-client-feature-loader.mjs
- **Version:** S.I.R source-freeze commit 19649f9336cbd3a6388e7add7641cbe5887b1bd4; Node v26.7.0; Vite 8.1.5; Fable 5.13.0.
- **Owner:** EHotwagner/S.I.R. client feature-loader contract
- **Recurrence:** refinement of `feedback/2026-08-15-SIR-186-6.md §4.1` and closed S.I.R. issue #214, which already establish versioned non-global/per-feature budgets; this recovery adds immutable-registry evolution and a typed deferred Docs boundary.
- **Avoidable cost:** one scoped registry-version update; no unrelated source-size optimization in this recovery.
- **Disposition:** positive-pattern; retain immutable prior registries, typed dynamic-component contracts, and per-route budgets.

#### §4.2 Intentional Docs changes invalidate five merged item-186 evidence bindings

- **Kind:** orchestration
- **Impact:** The Docs modality necessarily changes `App.fs` and the smoke inventory, so five digest-bound citations in three merged item-186 audits no longer describe the current files. Treating them as current would be false; rewriting their historical reports would erase what those reports actually reviewed.
- **Expected:** Commit-aware invalidation indexes only audits present in the base tree, reports every genuinely changed bound path, and recovery records the new boundary without altering historical evidence.
- **Observed:** The stale bundled consumer tool reported 19 bindings, including candidate-only item-185 audits. The fixed #1243 producer tool indexed `origin/main` and reduced the result to exactly five genuine bindings: `item-186-2.audit.json §4.7` cites `scripts/smoke-client.mjs` and `src/SIR.Client.Web/App.fs`; `item-186-3.audit.json §4.7` cites the same two paths; `item-186-6.audit.json §4.2` cites `src/SIR.Client.Web/App.fs`. All five are explicitly invalidated by the current intentional Docs implementation.
- **Evidence:** file:feedback/checkpoints/item-185-in-application-docs.jsonl; issue:FS-GG/FS.GG.Rendering#1243; command:dotnet fsi ../FS.GG.Rendering/template/feedback-report/skill/scripts/feedback-tool.fsx -- check-invalidation --base origin/main --head HEAD --root .
- **Version:** bundled consumer tool before FS.GG.Rendering#1243; authoritative producer commit 219b198d; S.I.R metadata commit 1f1120ac9ae5cd3174fa9f916b506945940fa333.
- **Owner:** EHotwagner/S.I.R. feedback evidence boundary and FS-GG/FS.GG.Rendering feedback invalidation checker
- **Recurrence:** continuation of the item-185 blocker recorded in `feedback/item-185-in-application-docs-2.md §4.5`; #1243 fixes candidate-only indexing, revealing these five legitimate base-present bindings.
- **Avoidable cost:** one authoritative recheck after the stale bundled consumer overreported 19 bindings.
- **Disposition:** accepted boundary for #185; do not claim the five historical bindings remain valid, do not rewrite merged reports, and preserve this additive recovery audit.

## §5 Did not exercise

No public network protocol, persistence format, simulation algorithm, dependency version, remote Markdown source, live-compositor frame rate, or package publication changed or is claimed.

## §6 Doc-versus-behavior contradictions

The loader documentation still described Tactical Environment as eager and a toolbar Docs control after the registry and production UI were deferred/canonical. The recovery corrected those statements to match registry v2 and the visible tactical Docs modality.

## §7 Workarounds still in the tree

The consumer-bundled feedback checker remains older than closed producer fix #1243, so this recovery used the adjacent authoritative producer implementation to distinguish candidate-only false positives from five genuine base-present bindings. Registry v1 remains as immutable history rather than being rewritten, and registry v2 is the current contract rather than a compatibility shim.

## §8 Friction and avoidable cost

The first recovery pass spent disproportionate effort considering size reduction for a 40-byte shared-chunk overrun. Freezing source, measuring each registered route, and versioning only the affected budget reduced the final repair to one explicit contract change. SDD source-snapshot acceptance and seven observed-run synchronizations were still mechanical metadata steps after focused evidence was green. The stale bundled invalidation checker required one authoritative producer recheck; historical audits were not rewritten.

## §9 Skill value and gaps

The feedback, SDD lifecycle, pnext, and intra-repository coordination contracts preserved the old PR ancestry, split source freeze from metadata seal, and kept recovery evidence property-owned. The material product pattern came from the loader registry itself: dynamic entries, budgets, and version projection share one inspectable contract.

## §10 Outcome markers

At source freeze, Client.Web Release compilation passed with zero warnings/errors; `build-client.sh` emitted 84 documentation pages, 6,487 blocks, 198,504 search tokens, and eight source mappings. Focused browser qualification passed three tests. DocsView measured 6,687 raw / 2,419 gzip / 2,135 Brotli bytes; Rules Explorer measured 62,188 / 18,447 / 16,040 under registry v2's 65,536 / 20,000 / 16,384 ceilings. Headless Chromium later measured 242 Docs DOM nodes, 0.883 ms representative-search p95, and 11.417 ms full-construction p95; both real-subject performance inversions exited 1. SDD reports 54 observed non-synthetic obligations and `shipReady` after metadata synchronization.

## §11 Falsifiable improvements

- Keep one immutable registry file per published version and fail loader projections when their version differs.
- Require every deferred Fable component to cross the dynamic boundary through one typed record contract; verify the real production control after compilation.
- Rebaseline only a measured owning route when source-frozen shared-chunk attribution crosses a ceiling; preserve every unrelated ceiling byte-for-byte.
- Keep edit-time source/build/browser gates focused, then emit one source-frozen bundle aggregate and perform metadata-only lifecycle sealing without rebuilding.
- Materialize the closed #1243 feedback-tool fix into generated consumers; verify that base/head mode names `origin/main` as its audit index and reports the same five base-present bindings here, never candidate-only audits.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing product recovered in place. |
| onboarding-guidance | not-exercised | Covered by the earlier cycle report. |
| skills | exercised | pnext, intra-repo coordination, SDD, and feedback contracts applied. |
| sdd-authoring | exercised | DEC-008/PD-011/PC-007/VO-010/PM-006 added; 54 obligations observed and shipReady. |
| implementation-apis | exercised | Typed deferred Docs presentation/callback contract compiled in .NET/Fable. |
| dependencies-build | exercised | Focused Release and Fable/Vite builds passed; no dependency changed. |
| testing | exercised | Loader source/compiled/budget mutations and three focused production browser journeys passed. |
| evidence | exercised | Property-owning JUnit/TRX receipts synchronized; bundle receipt is content-addressed; five old item-186 file bindings are explicitly invalidated, not reused. |
| runtime-playtest | exercised | Real controls enter, use, fail externally, and leave Docs while retaining tactical state. |
| performance | exercised | Per-route bytes and actual headless DOM/search/construction subjects passed; no compositor claim. |
| documentation | exercised | Loader architecture documentation corrected and generated Docs manifest qualified. |
| packaging-upgrade | not-exercised | No package or dependency version changed. |
| worker-git-pr | partial | Recovery preserved PR ancestry and source-froze locally; hosted CI, exact-head product review, and merge remain pending. |
