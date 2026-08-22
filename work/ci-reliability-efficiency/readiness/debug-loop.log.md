# Debug loop log

## Iteration 1 — 2026-08-22

- Failing command: `node scripts/test-persistent-workspace-m9-acceptance.mjs artifacts/client`
- Failure: `Persistent workspace M9 acceptance failed: review is not bound to the production bundle`
- Diagnosis: the protected production build emitted a new application bundle, while the committed M9 review manifest still named the preceding bundle hash. The live Chromium screenshot, measured geometry, production CSS, and accepted mockup remained byte-identical.
- Patch: regenerated the M9 review through `npm run review:persistent-workspace-m9`, updating the manifest's `productionBundleSha256` binding.
- Verification: rebuilt `SIR.slnx` in Release and reran the unchanged M9 acceptance command; all M0/M4–M9, browser smoke, worker round-trip, portability, layout, projection, and review-binding gates passed.

## Iteration 2 — 2026-08-22

- Hosted failing command: `./scripts/run-protected-stage.sh core artifacts/qualification/stages/core.json -- ./scripts/qualify-production.sh --protected`
- Failure: the shared runner measured the canonical scenario stress route at p95 118.995 ms / p99 133.557 ms against the unchanged 100/125 ms product budget.
- Diagnosis: the preceding protected run measured 91.576/96.387 ms, and an unchanged exact local rerun measured 52.421/54.889 ms. Structural counters were identical in all runs, so the isolated breach is runner contention rather than a semantic or algorithmic regression.
- Patch: none; product timing budgets and assertions remain unchanged.
- Verification: the exact local protected qualification advanced past the stress gate and stopped only when receipt generation correctly rejected the intentionally dirty review manifest. The complete wrapper is rerun after committing the generated manifest so its source/tree receipt can be valid.

## Iteration 3 — 2026-08-22

- Failing command: `./scripts/run-protected-stage.sh core artifacts/qualification/stages/core.json -- ./scripts/qualify-production.sh --protected`
- Failure: after M9 passed, `scripts/test-tactical-visual-review.mjs` reported `review is not bound to the production bundle`.
- Diagnosis: the tactical visual review is a downstream production capture whose manifest still named the prior bundle, stylesheet, and M9 baseline. Its retained 100/200-unit images and structural counts also preceded the current canonical 4×4 unit footprint.
- Patch: regenerated the review through `npm run review:tactical-visual`, refreshing the maintained-simulation capture, 100/200-unit density captures, manifest bindings, structural counts, and telemetry.
- Verification: visually inspected all three captures; `node scripts/test-tactical-visual-review.mjs` reproduced both independent captures byte-for-byte and passed the 100/150 ms input-to-paint plus 17.67 ms frame budgets; `npm run test:tactical-visual-review-mutations` rejected all stylesheet, lifecycle, workload, faction, simultaneous route/attack, one-route, font, timing, and reproduction mutations.

## Iteration 4 — 2026-08-22

- Local failing command: the documentation phase within the exact protected wrapper.
- Failure: fsdocs inherited `DOTNET_HOST_PATH=/usr/share/dotnet/dotnet`; that host lacks the repo-pinned 10.0.302 SDK even though `/home/developer/.dotnet/dotnet` provides it. Hosted CI installs 10.0.302 explicitly.
- Diagnosis: local host selection only; production, browser, tactical review, mutation, cross-runtime, server, and SDD verification gates had already passed.
- Patch: none to repository code or SDK policy. Re-ran the unchanged documentation consumer with `DOTNET_HOST_PATH=/home/developer/.dotnet/dotnet`.
- Verification: strict fsdocs project cracking/build passed for five assemblies; documentation integrity, experience, browser smoke, generated-site mutation, and accessibility gates all passed.

## Iteration 5 — 2026-08-22

- Hosted failing command: final Fable process inventory inside protected full qualification.
- Failure: the split hosted route observed one Replay Web and one Rules Explorer Fable build, while the invariant expected three of each.
- Diagnosis: hosted protected preflight executes the two cancellation-mutation builds in its own job and passes a signed preflight receipt to core. The integrated local protected route executes those builds inside core, so its trace legitimately contains three. Every functional hosted gate, including strict documentation, passed before this accounting-only failure.
- Patch: make the exact expected main-client Fable count route-aware: one when a protected preflight receipt is supplied, three for the integrated local protected route. All three cross-runtime fixture expectations remain exact-once.
- Verification: CI route contract and invocation-trace tests exercise the conditional inventory without relaxing unknown, duplicate, or malformed invocation rejection.

## Iteration 6 — 2026-08-22

- Hosted failing command: scenario-catalog qualification inside protected full qualification.
- Failure: the canonical 80×80/200-unit production route measured p95 101.126 ms / p99 105.072 ms against the unchanged 100/125 ms product budget. This performance signal had recurred on a second non-consecutive hosted run, so another blind retry was not accepted as a fix.
- Diagnosis: simultaneous movement conflict resolution repeatedly constructed immutable 4×4 footprint sets inside move×move and move×unit searches. The canonical stress fixture turns those transient allocations and set intersections into the dominant tick cost.
- Patch: retained the same destination, crossing, occupied-cell, move-away, conflict-precedence, and deterministic unit-order semantics while replacing repeated footprint-set materialization with allocation-free rectangle overlap/intersection checks. Destination multiplicity remains cell-exact.
- Verification: the Release solution builds with zero warnings and errors. Three complete client qualification runs retained the exact structural result (1,562 events, 77 path expansions, 16 peak LOS samples, 19 peak combat resolutions, and 6,533 scene nodes) while stress p95 fell from the prior local 52–56 ms range to 7.827–8.122 ms and p99 to 11.130–13.642 ms. The unchanged 100/125 ms assertion remains in force.

## Iteration 7 — 2026-08-22

- Failing command: M4 map-editor qualification within `./scripts/test-conformance.sh`.
- Failure: `review artifacts were not regenerated from the current production bundle` after the simulation optimization changed the emitted application bundle digest.
- Diagnosis: the runtime and cross-runtime gates had passed, but the maintained M4, M9, and tactical review manifests were still intentionally bound to the preceding bundle. This is the expected fail-closed evidence-freshness boundary after a production source change.
- Patch: regenerated the deterministic map-editor review, live M9 Chromium review, tactical production review, client feature-bundle graph receipt, and F# public-surface freshness projections against the optimized production build.
- Verification: M4 through M9 acceptance passed; both independent tactical reproductions stayed within their unchanged 100/150 ms input-to-paint and 17.67 ms frame budgets; and the full tactical stylesheet, lifecycle, workload, faction, simultaneous-route/attack, one-route, font, timing, and exact-reproduction mutation matrix failed closed.

## Iteration 8 — 2026-08-22

- Hosted failing command: `production-build-receipt.mjs verify` in the post-CI Pages deployment.
- Failure: `receipt-content-address-drift` after protected CI, the cost observer, and the qualified-site upload had all passed.
- Diagnosis: the CI handoff copied the canonical `<sha256>.json` site receipt to a fixed `site-receipt.json` alias. The verifier correctly requires the receipt basename to equal its content digest, so the deployment could never accept that alias.
- Patch: stage the receipt under its original content-addressed basename, rewrite the artifact pointer to the staged path, upload that receipt directory, and make Pages resolve the exact receipt through the pointer. The workflow contract now rejects reintroduction of the fixed alias.
- Verification: Pages handoff, action-pin/permission, CI route, protected-stage receipt, workflow topology, and YAML/static contract checks pass locally. Hosted qualified-site deployment remains pending.
