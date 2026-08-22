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
