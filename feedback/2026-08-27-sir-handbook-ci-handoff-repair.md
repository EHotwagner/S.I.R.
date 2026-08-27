---
feedbackSchema: 2
date: 2026-08-27
workspace: S.I.R-ci-routing
cycle: roadmap-sir-combat-quint-handbook-ci-handoff-repair
lane: sdd
toolVersion: 1.4.0
commit: 4548757af674c890a412326a6cc841de6baff3c6
---

# Development feedback — routed Pages handoff repair

## §1 Provenance and confidence

- **activation:** active
- **phases:** implementation-test-evidence, verify-ship-pr
- **material events:** 2
- **zero-event reason:** n/a

This repair cycle follows the first production main-push run of issue #366. Run 33093549465 is preserved in issue #368; local strict docs production and focused mutation evidence are reproducible. Replacement hosted PR/main and Pages results remained pending at draft time.

## §2 What worked

The exact route, gate, build-receipt, and protected-join contracts made it possible to add one typed handoff boundary instead of rebuilding documentation in Pages or inventing another path taxonomy.

## §3 What did not

The first main run reverified the docs producer receipt after the documentation consumer had legitimately augmented that producer's site output. The handoff step failed `output-identity-drift`, while both routed and protected verdicts still passed because neither owned the handoff artifact.

## §4 Findings

#### §4.1 A post-gate artifact was outside the protected verdict's required subject set

- **Kind:** defect
- **Impact:** A protected main run could report a passing protected verdict even though its selected documentation site could not be handed to Pages; the overall workflow failed only through the documentation job conclusion.
- **Expected:** When documentation is selected, missing, stale, malformed, or mismatched final-site handoff evidence is a named protected-verdict failure.
- **Observed:** Main run 33093549465 passed the documentation gate, failed final-site staging with `output-identity-drift`, and still produced passing routed and protected verdicts.
- **Evidence:** issue:EHotwagner/S.I.R.#368
- **Version:** S.I.R. merge `ad6aed1540bd6248abdd1228fda23c7300844d13`; protected join `sir.protected-join/v2`.
- **Owner:** EHotwagner/S.I.R CI protected verdict and Pages handoff
- **Recurrence:** new; no prior report covers a selected post-gate artifact absent from the protected join; existing issue #368.
- **Avoidable cost:** one failed post-merge run and one repair PR
- **Disposition:** existing issue

#### §4.2 Typed handoff identity makes Pages reuse and protected ownership independently testable

- **Kind:** positive-pattern
- **Impact:** Maintainers can distinguish final-site freshness, archive corruption, route drift, and missing upload without rebuilding the site or trusting job success alone.
- **Expected:** One exact-source receipt binds route, documentation gate, final-site receipt, and archive; protected CI and Pages consume the same identity.
- **Observed:** Focused contract tests create and verify the typed handoff; archive tampering, stale route/digest, and missing-handoff controls fail named checks. Workflow-contract tests require the protected join and Pages selector to consume that identity without a Pages rebuild. Hosted PR/main and Pages deployment verification remained pending.
- **Evidence:** file:scripts/qualified-site-handoff.mjs; file:scripts/test-qualified-site-handoff.mjs; file:scripts/test-protected-stage-receipts.mjs; file:scripts/test-pages-qualified-handoff.mjs
- **Version:** S.I.R. commit `4548757`; handoff `sir.qualified-site-handoff/v1`.
- **Owner:** EHotwagner/S.I.R CI protected verdict and Pages handoff
- **Recurrence:** extends the exact-source receipt composition accepted in `feedback/2026-08-27-sir-handbook-ci-main-routing.md` §4.1.
- **Avoidable cost:** none
- **Disposition:** accepted

## §5 Did not exercise

Gameplay, Quint semantics, browser play, performance, dependency changes, and handbook M4–M7 content were outside the repair. Replacement hosted PR/main and Pages deployment remained pending.

## §6 Doc-versus-behavior contradictions

No documentation authority contradicted the repair; the defect was missing ownership between the documented routed documentation gate and protected site handoff.

## §7 Workarounds still in the tree

None introduced. Pages still deploys an already-qualified archive and does not rebuild.

## §8 Friction and avoidable cost

One failed post-merge run and one follow-up issue/PR were required. The exact SDK was locally available, but inherited host variables initially selected `/usr/share/dotnet`; clearing `DOTNET_HOST_PATH` and `DOTNET_ROOT_X64` restored the documented pinned SDK path.

## §9 Skill value and gaps

`work-roadmap` kept post-merge proof open instead of treating merge as completion. `fs-gg-feedback-report` preserved the hosted failure and the repair controls. Existing SDD evidence was rebound and remained 15/15 observed. No gameplay, Quint, visual, playtest, or performance skill applied.

## §10 Outcome markers

- Hosted red-before: main run 33093549465 reproduced output-identity drift and the misleading passing protected verdict.
- First focused green-after: typed handoff, protected join, Pages, routing, integrity, cost, pin, and mutation tests passed.
- Local production green-after: exact restore/build, strict docs projection, final-site receipt create/verify, and archive handoff create/verify passed.
- Lifecycle: 15/15 observed; verify and ship converged `noChange`.
- Delivery: issue #368 and replacement hosted proof remained active.

## §11 Falsifiable improvements

- Preserve §4.1 by requiring site-handoff outcome and receipt identity in the focused protected join whenever `documentation` is selected. Acceptance: missing, stale, malformed, and route-mismatched handoffs each produce a failed `sir.protected-join/v2` receipt.
- Preserve §4.2 by keeping Pages on the final-site receipt and typed archive handoff. Acceptance: archive or receipt mutation fails before extraction/deploy, and Pages contains no build command.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing product and CI. |
| onboarding-guidance | partial | Existing CI/docs authority reused; local SDK host cleanup was needed. |
| skills | exercised | Roadmap and feedback contracts governed repair and proof. |
| sdd-authoring | partial | Existing item evidence was rebound; no new product spec was needed. |
| implementation-apis | exercised | Handoff schema, protected join, workflow, and Pages contracts changed. |
| dependencies-build | exercised | Exact locked restore/build and strict docs production passed; no dependency changed. |
| testing | exercised | Positive and negative handoff/protected/Pages suites passed. |
| evidence | exercised | Typed exact-source receipts and 15 observed obligations passed. |
| runtime-playtest | not-exercised | CI infrastructure only. |
| performance | not-exercised | No runtime performance claim. |
| documentation | partial | Documentation build/handoff exercised; handbook content unchanged. |
| packaging-upgrade | not-exercised | No package or lock change. |
| worker-git-pr | partial | Hosted red-before captured; replacement PR/main proof pending. |
