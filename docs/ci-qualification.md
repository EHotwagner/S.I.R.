# CI qualification routes

S.I.R. has four explicit qualification routes. They share evidence contracts but not authority.

- Pull requests run the focused lane in `.github/workflows/ci.yml`: changed paths are classified, the always-on integrity floor runs, and route-selected native, Fable, web, server, and documentation producers publish separate narrow content-addressed artifacts. Independent named gates then run concurrently, and `pr-verdict` joins every typed result. Every omitted gate has a route-policy reason; unknown or mixed product paths select the conservative cross-cutting route.
- Protected `main` pushes first run `protected-preflight`, then the stable `full-qualification` context runs `./scripts/qualify-production.sh --protected`, and `protected-verdict` joins both exact-source receipts unconditionally. The preflight owns rules-corpus, spatial-static, and cancellation-mutation proof; core verifies that receipt before running the remaining complete clean-room ship-boundary route. Failed stages upload their own receipt, so the join can name a missing, failed, tampered, or source-mismatched stage instead of exposing only an opaque late failure.
- The nightly schedule uses the same topology and adds the cross-surface full-route mutation to protected preflight, proving that a defect legitimately omitted by a focused route is rejected by the full surface.
- Local development uses `./scripts/qualify-pr.sh route <changed-path-file>`, then `integrity`, `prepare-part <native|fable|web|server|docs>`, and `gate <id>` for the selected gates. Each prepared producer performs locked restore for only its owned project roots (and their transitive project references); the native producer remains the single whole-solution restore/build boundary. `npm run test:ci-route` is the fast route/join/workflow contract suite. `npm run qualify:production` remains the local full qualification command. The historical baseline comparison is an explicit `./scripts/qualify-production.sh --paired-optimization` experiment, not a forever-growing gate on the normal aggregate.

The route receipt is `sir.ci-route/v2`. It records a typed
`productionReview` decision in addition to the ordinary classification. Changes
under `src/SIR.Client` or `src/SIR.Client.Web` select documentation qualification
as well as browser qualification, because the committed map-editor review boards
are hash-bound to the clean production bundle. Pure browser-test and server-only
changes do not acquire that extra subject. The documentation preparer follows the
selected `documentation` output rather than inferring it from the coarse route
class. `npm run test:production-review-freshness-mutations` proves a stale bundle
hash is rejected and the restored exact bundle is accepted.

Successful protected core qualification uploads `protected-qualified-site` with
the site, its content-addressed receipt, and the protected timing receipt. The
GitHub Pages workflow is a restricted `workflow_run` consumer: it accepts only a
successful `main` push, checks out the triggering SHA, downloads from that exact
run, verifies the receipt and all site bytes, and deploys without npm, fsdocs, or
any build command. A failed, PR-originated, wrong-branch, missing, or mismatched
run cannot reach deployment.

`CI Cost Observer` is a read-only reusable workflow and post-run `workflow_run`.
Pull requests call it after `pr-verdict` so a candidate can qualify the observer
before that workflow exists on the default branch; it excludes exactly its own
still-active job and rejects every other incomplete timing. Default-branch and
scheduled runs use the terminal post-run trigger. Both forms download only small
typed receipt artifacts, query the GitHub Jobs and Artifacts APIs for the same
run id/attempt/head, and emit `sir.ci-cost-report/v1`. The report accounts
for full job and step wall time, uploaded bytes, receipt-owned duration, and the
otherwise hidden action/setup/post/transport residual. An incomplete receipt
shape or source mismatch makes the optimization report incomplete; it never
rewrites the completed product verdict.

Integrity has a small unconditional CI-contract floor and a versioned
`sir.ci-integrity-plan/v1` for the slower npm-audit, governance,
dependency-surface, SDD byte-stability, and feedback-audit subjects. Each subject
is recorded as executed or as a measured omission with its matching paths.
Unknown paths, workflow topology changes, and classifier/self-owner changes run
every subject; malformed routes fail before planning. This follows the current
FS-GG conservative-classifier pattern without importing its producer registry or
coordination board into S.I.R.

The PR budget is 300 seconds from its route runner start to the actionable join verdict on `ubuntu-latest`. Representative cross-cutting acceptance is stricter: it must finish within 240 seconds, retaining at least 60 seconds of headroom for observed runner variance and bounded producer-inventory growth. The join reports `feedback-headroom-eroded` above that target and retains `feedback-budget-exceeded` at the unchanged outer boundary; retrying the same slow head is not acceptance evidence. These are runner-feedback contracts, not gameplay-performance assertions. Ordinary PR cross-runtime qualification executes every functional workload and structural assertion but leaves stopwatch thresholds to the protected/full route, where the existing product-performance values—including the representative 50 ms combat/spatial budget—remain unchanged and mandatory. `npm run test:ci-product-performance-route` forces that subject beyond 50 ms and proves the ordinary functional edge passes while the protected edge fails. Gate receipts record queue (null when GitHub does not expose it), setup, restore, build, artifact transport, test, and total durations, cache and receipt reuse, build invocations, retries, failure stage, graph critical path, runner-minutes, and actual/required headroom. Failed evidence and producer commands checkpoint their actual terminal phase. The join recomputes the route and prepared-manifest identities, binds every result to the exact candidate, and aggregates all missing, stale, malformed, duplicate, cancelled, failed, unexpected, or headroom-eroding required results into one typed verdict.

Prepared outputs are acceleration only. Each producer inventories outputs only after its commands finish; `sir.ci-artifact-manifest/v2` chains the exact route to `sir.production-build-receipt/v1`. Its `sir.ci-content-index/v1` stores one object per SHA-256 and maps every logical path, mode, size, and digest back to that object before deterministic tar transport. A consumer verifies the archive and index, rejects missing/extra/corrupt objects, reconstructs into a fresh per-producer staging directory, verifies every reconstructed byte and mode against the build receipt, and only then materializes a mutable working copy. Post-gate verification reads the immutable reconstructed staging copy, so a mutation or named isolated rebuild cannot corrupt the evidence it is meant to reuse. Multipart artifact maps are key-sorted before hashing or joining.

Native and server PR artifacts are explicitly Linux-runner acceleration closures.
After the ordinary build/publish completes, their Wasmtime runtime inventory is
reduced to `linux-x64`; the script rejects absent target bytes, symbolic or
malformed inventories, and any unexpected remaining RID. Protected package and
cross-runtime subjects retain the portable proof. On the measured current output,
the removed non-target directories total about 98.4 MB in each closure before
content-addressed deduplication.

A clean local candidate build of all five producer archives measured
105,605,120 transported tar bytes versus the 351,337,554-byte reference run
(69.94 percent lower). Native reconstruction also collapsed 70,395,259 logical
bytes to 36,213,075 unique object bytes. These figures prove the payload
mechanism and exceed the design threshold locally; they are not final acceptance
evidence until the read-only cost observer reports the same exact hosted head.

Every official action in the CI, observer, and Pages workflows is pinned to a
full commit SHA with its release version in a comment. Product commands still use
the explicitly configured Node 26.5.0. Every job has an explicit timeout, CI and
observation remain read-only, and only the Pages deploy job receives `pages:write`
and `id-token:write`.

The native producer performs one locked Release build of `SIR.slnx` and transports only the Release runtime closures required by consumers. It does not pair a Debug solution build with an accumulating list of serialized Release owner builds; solution growth therefore remains one traced ordinary native invocation. The fixed cancellation mutation proof is a source-bound `cancellation-mutations` helper launched directly after routing, parallel to product preparation. Its isolated real-worker probe needs the current authoritative replay fixture before prepared-native transport exists, so the helper performs one named minimal `SIR.Domain.Tests` Release build under the `cancellation-fixture` exception, then builds the named mutant, requires the smoke to reject it for the cancellation diagnostic, restores the source, and stops. The ordinary cancellation gate later consumes immutable verified native/web staging and runs only the passing restored worker smoke. The deterministic join requires both receipts, so this topology removes the fixed mutant proof from the serial producer-to-smoke path without replacing its real fixture or cancellation evidence. Rules runs the prepared Release targets without rebuilding them. The fixed nine-subject spatial mutation proof is likewise a source-bound helper launched directly after routing; every mutant receives its own source, signature, artifact, and runtime roots while retaining the complete named inventory and exact failure diagnostic. The ordinary spatial gate consumes verified native/Fable artifacts for final conformance, and the deterministic join requires both spatial receipts. Its unreadable-source fail-closed probe mutates only a temporary fixture. The gate verifies immutable reconstructed native/Fable staging before dispatch, then spatial trusts that verified staging instead of rescanning the live server-publish output that production-delivery intentionally composes. Production-delivery can therefore run concurrently with rules, spatial, and cancellation on the shared domain runner without weakening prepared-input identity. Those domain consumers, cross-runtime, and documentation start as soon as their prepared inputs exist; the independent integrity result remains mandatory at the deterministic join rather than gating their start. The ordinary non-prepared mutation paths continue to exercise canonical project sources. Web and server remain parallel producers; browser consumers copy the verified client into the mutable server publish and seal a typed composition receipt before Playwright starts. In CI the growing general inventory is balanced at test granularity into four globally indexed quarters that run on separate hosted runners, one isolated server/browser pair per runner. The fixed Slow-3G/4x-CPU production-delivery proof runs as a fifth source-bound browser receipt over the same verified web/server artifacts. All five browser receipts are required whenever browser qualification applies and run concurrently without waiting for the independent integrity gate; `pr-verdict.needs` names every helper explicitly so it cannot join before a receipt exists. Per-runner capacity remains pair-accounted as `max(1, floor(availableParallelism / 2))`; externally indexed execution schedules exactly one pair on its runner while retaining the global shard denominator for complete, disjoint test-granular coverage. This is workflow concurrency, not a test-count ceiling: new tests remain in the discovered inventory and are assigned to one of the four quarters. Every fragment must be structurally readable, non-empty, internally count-consistent, and unique before all four fragments are sorted and merged. Local/full runs default to the complete one-shard inventory, and ordinary same-runner `SIR_BROWSER_SHARDS` accepts any positive integer no greater than safe pair capacity or the port range. Every actual producer and gate build/Fable invocation records its owner, normalized target, named isolation arguments, and timed interval. The join rejects missing, duplicate, unknown, untimed, stale, or failed helper/build receipts. A consumer never rebuilds silently when reuse fails. The evidence lane restores locked dependencies and verifies only changed work items owned by the route, using tracked evidence that exists in a clean checkout.
