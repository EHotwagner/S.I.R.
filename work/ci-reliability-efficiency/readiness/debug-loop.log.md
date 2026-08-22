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
