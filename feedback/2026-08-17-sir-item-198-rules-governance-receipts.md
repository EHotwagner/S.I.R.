---
feedbackSchema: 2
date: 2026-08-17
workspace: sir-item-198-rules-governance-receipts
cycle: 198-rules-governance-receipts
lane: sdd
toolVersion: 1.0.1
commit: e80909caeec78f439f1b5de97855aa68ab03c841
---

# S.I.R. item 198 development feedback

## §1 Provenance and confidence

This cycle resumed issue 198 from fresh `origin/main` commit `fb0347a175c97a537e19b06faed63abda88eb94e`, after Governance issue 413 published `FS.GG.Governance.Kernel` and `FS.GG.Governance.Adapters.Spi` 0.1.1. It exercised SDD 1.0.1 from analyze through ship, exact package restore, adapter implementation, focused tests, evidence binding, and a source-frozen local aggregate. Three checkpoints are recorded in `feedback/checkpoints/198-rules-governance-receipts.jsonl`. The aggregate limitation is environment-qualified rather than assigned a product root cause; the pinned hosted runner and independent review remain the authority on whether it reproduces.

## §2 What worked

The published package split was sufficient for a product-owned closed fact union, reified check catalog, SPI adapter, fixed-point provenance, and deterministic verdicts without introducing Governance into gameplay projects. SDD dependency-surface captures made the exact external API reviewable, and observed JUnit binding moved all 42 evidence obligations and all 42 test obligations out of self-attested state. The production route sealed an immutable build receipt before the unrelated smoke timeout.

## §3 What did not

The first dependency-surface plan references used F# source names (`Adapter`, `Check.probe`) rather than captured CLR symbols (`Adapter`3`, `CheckModule.probe`), producing one avoidable blocked analyze pass. The local aggregate ran under Node 26.7.0 while the repository pins 26.5.0 and reproducibly timed out waiting for the deferred Samples owner after build-receipt sealing. Policy correctly prevented another full aggregate retry.

## §4 Findings

#### §4.1 Published Governance packages close the external-adapter gap

- **Kind:** positive-pattern
- **Impact:** S.I.R. could implement and test receipt-backed policy as an isolated product adapter while gameplay evaluation remained dependency-free.
- **Expected:** The producer issue should expose the documented Kernel and Adapter SPI through exact public packages usable from a clean external consumer.
- **Observed:** A NuGet.org-only clean locked consumer resolved both 0.1.1 packages exactly and compiled their public types; the product adapter used those surfaces for fixed-point decisions and typed provenance.
- **Evidence:** file:readiness/198-rules-governance-receipts/nuget-org-consumer.json; file:docs/dependency-surface/FS.GG.Governance.Adapters.Spi/0.1.1.json
- **Version:** FS.GG.Governance.Kernel 0.1.1 and FS.GG.Governance.Adapters.Spi 0.1.1
- **Owner:** FS-GG/FS.GG.Governance package surfaces
- **Recurrence:** first seen feedback/checkpoints/item-198-rules-governance-receipts.jsonl as a capability gap; FS-GG/FS.GG.Governance#413 resolved it
- **Avoidable cost:** none in the resumed implementation
- **Disposition:** accepted

#### §4.2 Framework references expose CLR names without an F# authoring bridge

- **Kind:** friction
- **Impact:** SDD analyze blocked despite the intended public members existing in the pinned packages.
- **Expected:** An author should be able to cite a discoverable stable framework member without guessing how an F# type/module is represented in the capture.
- **Observed:** `Adapter` and `Check.probe` were dangling; inspection of the generated captures showed the accepted symbols were ``Adapter`3`` and `CheckModule.probe`.
- **Evidence:** file:readiness/198-rules-governance-receipts/dependency-surface-authoring-limitation.json; file:docs/dependency-surface/FS.GG.Governance.Adapters.Spi/0.1.1.json; file:docs/dependency-surface/FS.GG.Governance.Kernel/0.1.1.json; file:work/198-rules-governance-receipts/plan.md
- **Version:** fsgg-sdd 1.0.1 dependency-surface report 2.6.0
- **Owner:** FS-GG/FS.GG.SDD dependency-surface authoring diagnostics
- **Recurrence:** new; no matching prior report or issue found
- **Avoidable cost:** one blocked analyze retry and two framework-reference corrections
- **Disposition:** product fix

#### §4.3 Local aggregate environment did not match the repository Node pin

- **Kind:** orchestration
- **Impact:** The exact-source route built and sealed its production build receipt but could not finish the deferred-client smoke, so local aggregate evidence is explicitly limited pending hosted qualification.
- **Expected:** Source-frozen qualification should execute with the repository-pinned Node 26.5.0, or refuse before expensive work when the active runtime differs.
- **Observed:** Under Node 26.7.0, both the aggregate and one focused rerun timed out waiting for the deferred Samples owner after the immutable build receipt was written.
- **Evidence:** file:readiness/198-rules-governance-receipts/local-aggregate-limitation.json; file:docs/evidence/production-build-receipt-v1/bcd9125c1cd73c2ff012de6ee9dd48be57eb91998faa41f84d66258ca14752b0.json
- **Version:** expected Node 26.5.0; observed Node 26.7.0
- **Owner:** EHotwagner/S.I.R. qualification environment preflight
- **Recurrence:** new; item 180 records adjacent Node-version differences but not this missing preflight or timeout
- **Avoidable cost:** one source-frozen aggregate; no aggregate rerun
- **Disposition:** product fix

## §5 Did not exercise

No live gameplay playtest or package publication was required. The local aggregate did not reach its browser, documentation-site reuse, mutation, and final timing stages after the client-smoke timeout. The pinned hosted PR route and exact-head critic remain pending.

## §6 Doc-versus-behavior contradictions

The dependency-surface authoring model presents framework references in source-oriented prose, while validation accepts captured CLR symbol identities. The generated capture is authoritative, but the authoring guidance does not surface the translation before analyze.

## §7 Workarounds still in the tree

None. The adapter consumes released packages and carries no DLL hint path, sibling producer project reference, or gameplay runtime dependency. The aggregate limitation is recorded as evidence, not bypassed.

## §8 Friction and avoidable cost

One analyze retry and two plan-member corrections were avoidable. Configuration validation also caught an attempted multi-root package-surface declaration immediately, before build work. One clean aggregate performed the full build/Fable/client path before the environment-qualified smoke timeout; policy prohibited rerunning it.

## §9 Skill value and gaps

`pnext-item`, `intra-repo-parallel-work`, and `cross-repo-coordination` preserved fresh identity, claim, source-current dependency provenance, and an isolated worktree. The SDD lifecycle and stage skills made 94 analysis relationships, 42 observed evidence obligations, and 84 verification findings explicit. `fs-gg-feedback-report` preserved the producer-package success and environment limitation. A dependency-surface authoring example that maps common F# names to captured CLR symbols would remove the only SDD-specific retry.

## §10 Outcome markers

The first meaningful focused adapter test passed before aggregate freeze with 16 rules, 11 checks, and 10 protected-subject mutations. The clean NuGet.org-only exact locked consumer built with zero warnings. SDD verify reached `verificationReady` with 84/84 ready findings, and ship reached `shipReady` with 42/42 observed evidence declarations. Governance produced an unblocked ship verdict with two honest advisory migration findings, and the protected-boundary join allowed the handoff. Merge is pending.

## §11 Falsifiable improvements

For §4.2, FS.GG.SDD should display or emit suggested captured symbols when a framework reference is dangling. Acceptance: a plan that cites `Adapter` against the 0.1.1 capture receives ``Adapter`3`` as a deterministic correction without manually opening the JSON.

For §4.3, S.I.R. qualification should validate the active Node version against the 26.5.0 engine pin in `package.json` before starting build work. Acceptance: Node 26.7.0 with the repository pin at 26.5.0 fails before restore/build with both versions in one diagnostic, while the pinned hosted runner proceeds to client smoke.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing repository and SDD skeleton were reused. |
| onboarding-guidance | exercised | Repository AGENTS and source-current coordination instructions determined the correct board, base, and claim. |
| skills | exercised | Coordination, complete SDD lifecycle, and feedback-report skills governed the cycle. |
| sdd-authoring | exercised | Analyze reached 94/94 ready relationships; evidence, verify, and ship passed. |
| implementation-apis | exercised | Kernel and SPI 0.1.1 supported the closed adapter and fixed-point provenance. |
| dependencies-build | exercised | Exact locks, dependency captures, clean NuGet.org consumer, and solution build passed; aggregate limitation is explicit. |
| testing | exercised | Focused suite passed 16 rules, 11 checks, and 10 mutations. |
| evidence | exercised | 42/42 declarations and 42/42 test obligations are observed, not self-attested. |
| runtime-playtest | not-exercised | No gameplay behavior changed. |
| performance | partial | The aggregate exercised existing performance subjects before smoke timeout; no new runtime performance intent applied. |
| documentation | exercised | Receipt/policy architecture and commands are documented; full docs build generated before smoke timeout. |
| packaging-upgrade | partial | Public 0.1.1 packages were consumed and locked; S.I.R. published no package. |
| worker-git-pr | partial | Claim, worktree, heartbeat, and freeze were exercised; hosted review/merge remained pending at report draft. |
