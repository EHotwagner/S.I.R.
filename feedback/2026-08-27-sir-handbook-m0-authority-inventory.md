---
feedbackSchema: 2
date: 2026-08-27
workspace: S.I.R-roadmap-m0
cycle: roadmap-sir-combat-quint-handbook-m0-authority-inventory
lane: sdd
toolVersion: 1.4.0
commit: 9dd2afecb91ad6a4a886b80c9c275cf981cb242a
---

# Development feedback — combat Quint handbook M0 authority inventory

## §1 Provenance and confidence

This cycle ran in the existing S.I.R. scaffold on issue #356 and branch `item/356-handbook-m0`, from
base `77e56d11867a5e2e7ad99f4d61b0f0c9fff61a5f` through reviewed cycle commit
`9dd2afecb91ad6a4a886b80c9c275cf981cb242a`. The lifecycle tool reported version 1.4.0. The cycle's
four phase checkpoints are in
`feedback/checkpoints/roadmap-sir-combat-quint-handbook-m0-authority-inventory.jsonl`; the final checkpoint
records PR #357 opening and the baseline invalidation limitation. Evidence covers SDD stages charter through ship,
focused ledger verification and its missing-rule mutation, strict fsdocs generation, and the broader
documentation route through all pre-fsdocs gates. Hosted CI, independent PR review, merge, and
post-merge board state were not yet complete when this draft was written.

## §2 What worked

The imported roadmap operated effectively as the ledger: it kept M1 content out of M0 while making the
candidate-versus-current authority distinction explicit. The focused Node audit produced JUnit that SDD
could consume, so all 17 evidence and test obligations were observed rather than self-attested; verify
reported 34 ready obligations and ship reported `shipReady`.

## §3 What did not

The first documentation attempt reached Vite without a local locked npm installation. After `npm ci`,
the route reached strict fsdocs but inherited the ambient `/usr/share/dotnet/dotnet`, which lacked the
repo-pinned SDK. A bounded rerun with the pinned `DOTNET_HOST_PATH` completed strict fsdocs generation.
Both conditions were environment/bootstrap friction, not product failures.
The first hosted documentation check then rejected the new page's uncurated `Design` category and
sidebar label beginning with the reserved product name. Moving the page into `Battlefield Systems` and
using `Combat in Quint handbook roadmap` as its navigation title restored the checked sidebar contract.

## §4 Findings

#### §4.1 JUnit-backed SDD evidence kept documentation completion observable

- **Kind:** positive-pattern
- **Impact:** All 17 generated evidence and test obligations were independently classified as observed, allowing verify and ship to close without synthetic or self-attested passes.
- **Expected:** Documentation tasks produce a real test receipt that the lifecycle reads.
- **Observed:** The focused roadmap verifier emitted one passing JUnit case after checking milestone state, the exact sixteen-rule inventory, authority status, required sections, and target path; its missing-rule mutation failed through the exact-registry assertion.
- **Evidence:** file:readiness/356-handbook-m0/handbook-m0.junit.xml; file:readiness/356-handbook-m0/ship-verdict.json; file:work/356-handbook-m0/verify-roadmap.mjs
- **Version:** fsgg-sdd 1.4.0 at commit `9dd2afecb91ad6a4a886b80c9c275cf981cb242a`.
- **Owner:** FS-GG/FS.GG.SDD evidence contract and EHotwagner/S.I.R. documentation verification pattern
- **Recurrence:** new for this roadmap cycle; no matching prior handbook-roadmap verifier found.
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Audit invalidation cannot distinguish this branch from the already-red main baseline

- **Kind:** defect
- **Impact:** The mandated pre-commit invalidation check cannot provide a branch-specific verdict; it reports 17 historical exception errors even for `origin/main..origin/main`, so a worker must preserve the failure and compare baselines instead of claiming a green check.
- **Expected:** An empty base/head diff passes, and a candidate diff fails only when the candidate touches digest-bound evidence without resealing its audit.
- **Observed:** Both `--base origin/main --head HEAD` and `--base origin/main --head origin/main` report the same 17 `overbroad or mismatched exception` errors.
- **Evidence:** file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m0-authority-inventory.jsonl; command:dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- check-invalidation --base origin/main --head origin/main; issue:FS-GG/.github#2856; issue:FS-GG/.github#2852
- **Version:** feedback tool committed at `9dd2afecb91ad6a4a886b80c9c275cf981cb242a`; FS-GG/.github#2856 is closed but the S.I.R. distributed copy still reproduces.
- **Owner:** FS-GG feedback invalidation semantics plus EHotwagner/S.I.R. audit-binding exception ledger
- **Recurrence:** duplicate of the cause and exact 17-error empty-diff behavior documented in `feedback/2026-08-23-sir-item-277-agent-env-dotnet-root.md`; FS-GG/.github#2856 was folded into open distribution-authority issue FS-GG/.github#2852.
- **Avoidable cost:** one candidate check and one baseline comparison.
- **Disposition:** existing issue

## §5 Did not exercise

Runtime gameplay/playtest, performance qualification, package publication/upgrades, exhaustive Quint
verification, and the final handbook link-audit were outside M0. PR checks, merge, and post-merge board
state were pending at draft time.

## §6 Doc-versus-behavior contradictions

The general `npm run build:docs` entry point does not itself install locked npm dependencies, while fresh
worktrees do not carry `node_modules`; existing feedback documents `npm ci` as the expected bootstrap.
The agent environment also exposed `DOTNET_HOST_PATH=/usr/share/dotnet/dotnet` despite the repository
requiring SDK 10.0.302 from the pinned user host; this exact behavior is already recorded in the CI
reliability debug log.

## §7 Workarounds still in the tree

None. No source workaround was added. The successful host override was command-local.

## §8 Friction and avoidable cost

Two aggregate documentation attempts failed after substantial prerequisite work. One `npm ci` and one
bounded strict-fsdocs rerun were required. The audit invalidation gate needed one baseline comparison.
No product files were reverted or regenerated by hand.

## §9 Skill value and gaps

The FS.GG SDD lifecycle/stage skills and `fs-gg-feedback-report` were invoked. The lifecycle forced the
authority-status decision, M1 boundary, observed evidence, and committed ship verdict to remain aligned.
The feedback skill preserved bootstrap failures before they were hidden by the eventual green result.
Interactive/gameplay, playtest, and performance skills were not relevant because M0 changed only the
documentation ledger and lifecycle evidence.

## §10 Outcome markers

- First meaningful focused test: ledger verifier passed with 16 unique rules; missing-rule mutation red.
- First green SDD verification: 34/34 obligations ready, 17/17 evidence and test obligations observed.
- Ship readiness: `shipReady`, no blocking findings, no synthetic or missing evidence.
- Documentation: strict fsdocs generated the roadmap with the pinned host after broader browser/docs prerequisites passed.
- Hosted CI, PR review, merge, and board completion: pending at draft time.

## §11 Falsifiable improvements

- Preserve §4.1 by requiring each roadmap milestone verifier to emit a real JUnit receipt and a named
  negative control. Acceptance: deleting one required ledger fact reds the focused verifier, while the
  restored ledger yields observed (not self-attested) SDD verification.
- For §4.2, complete the distributed-skill identity work tracked by open FS-GG/.github#2852 so the
  upstream invalidation semantics materialize in S.I.R.
  Acceptance: `check-invalidation --base origin/main --head origin/main` returns success with zero errors,
  while touching one digest-cited file without resealing still returns the bound audit/finding locator.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | partial | Existing scaffold inspected; no new scaffold generated. |
| onboarding-guidance | exercised | Repository AGENTS guidance and source roadmap boundaries were applied. |
| skills | exercised | SDD lifecycle/stage and feedback-report skills drove authoring and evidence. |
| sdd-authoring | exercised | Charter through tasks and analysis reached implementation readiness. |
| implementation-apis | not-exercised | No production API changed. |
| dependencies-build | exercised | Locked npm bootstrap and pinned .NET host led to green strict fsdocs. |
| testing | exercised | Focused pass plus missing-rule mutation; JUnit consumed by SDD. |
| evidence | exercised | 17/17 evidence and test obligations observed; ship ready. |
| runtime-playtest | not-exercised | Documentation-only milestone. |
| performance | not-exercised | No runtime/performance claim or gate changed. |
| documentation | exercised | Ledger rendered under strict fsdocs; broader docs prerequisites passed. |
| packaging-upgrade | not-exercised | No package or pin changed. |
| worker-git-pr | partial | Isolated worktree and implementation commit complete; PR/merge pending at draft time. |
