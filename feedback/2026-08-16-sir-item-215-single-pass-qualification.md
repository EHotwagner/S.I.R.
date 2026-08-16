---
feedbackSchema: 2
date: 2026-08-16
workspace: S.I.R
cycle: item-215-single-pass-qualification
lane: sdd
toolVersion: 1.0.0
commit: 26ed06775d7e78477896a7fd2251808294efeea2
---

## §1 Provenance and confidence

The cycle used the existing S.I.R scaffold, canonical Project 6 route revision 1, source-current coordination engine 0.58.1 for the live v2 route, and SDD stages 1–10. The four tracked checkpoints are in `feedback/checkpoints/item-215-single-pass-qualification.jsonl`. Package/tool pins include Fable 5.13.0, fsdocs 22.1.0, and SDD package 1.0.1 (CLI report version 1.0.0). The clean baseline and candidate ran from detached worktrees at `dab492e` and `5f2f54e`; immutable receipts bind the candidate source and outputs. Local evidence covers conformance, delivery, browser, docs, accessibility, self-restoring stale reuse, 39/39 observed SDD obligations, and ship readiness. Hosted exact-head CI, merge, and post-merge stamp remain pending.

## §2 What worked

The content-addressed receipt separated expensive build outputs from later metadata. A temporary Git fixture proved canonical bytes and fail-closed source, lock, revision, command, tool, and output boundaries cheaply. A second focused receipt, written only after conformance succeeded, bound browser/delivery result files and allowed documentation qualification to reuse six already-attested gates. The final clean aggregate retained every acceptance subject, invoked main and Rules Fable once each, restored the mutation subject, and reduced wall time from 373,848 ms to 263,125 ms.

## §3 What did not

The repository-pinned coordination client again could not read the current v2 route. The first reuse boundary treated compiled client artifacts as sufficient for fsdocs, but conformance deleted its isolated NuGet cache and did not build the docs-only `SIR.Client.dll`; two clean candidates were rejected while those prerequisites were made explicit. A third functionally green candidate saved only 16.86%, exposing six repeated browser gates. Full output-graph inventory led to one focused conformance receipt and the final 29.61% result; the threshold was not weakened.

## §4 Findings

#### §4.1 The pinned coordination client still lags the live route schema

- **Kind:** orchestration
- **Impact:** The worker could not safely claim the routed issue with the repository-pinned client and had to select the source-current engine before authoring.
- **Expected:** The pinned client should parse the canonical `route-decision/v2` receipt.
- **Observed:** The cycle checkpoint records that pinned 0.22.1 treated the valid receipt as missing and source-current 0.58.1 verified revision 1 with digest `a4009e18e5dc8f795a1d59b272838bf7b64bd6bd9cedb9d292e23ba78bc3cbbb` without rewriting it. Only the pinned version and local recurrence are independently reproducible from this report.
- **Evidence:** file:feedback/checkpoints/item-215-single-pass-qualification.jsonl; command:scripts/fsgg-coord --version; file:feedback/2026-08-16-sir-item-214-client-feature-loader.md
- **Version:** pinned 0.22.1; source-current 0.58.1
- **Owner:** existing issue FS-GG/.github#2469
- **Recurrence:** seen again feedback/2026-08-16-sir-item-214-client-feature-loader.md §4.1; existing issue FS-GG/.github#2469
- **Avoidable cost:** checkpoint recollection records one refused claim and one engine-resolution retry; the original command output was not retained
- **Disposition:** existing issue

#### §4.2 Focused receipts removed the aggregate/audit and browser/docs repetition

- **Kind:** positive-pattern
- **Impact:** Qualification preserved all acceptance behavior while saving 110,723 ms (29.61%) and reduced Fable target builds from four to two.
- **Expected:** Expensive outputs and successful focused stages should be reusable only when source, locks, tools, commands, outputs, and result artifacts still match.
- **Observed:** The build receipt binds main Fable, Rules Fable, and production client outputs. The conformance receipt binds test sources/fixtures, the build receipt, bundle graph, browser/delivery JUnit, and client-feature TRX. Docs verifies both, skips only six attested browser gates, and still runs its own assembly, fsdocs, publication, smoke, experience, and accessibility checks. The feedback validator can reverify the focused build receipt after metadata-only commits without another aggregate.
- **Evidence:** file:readiness/215-single-pass-qualification/single-pass-timing.json; file:docs/evidence/production-build-receipt-v1/87cf06f6c4cf32abd997cd6796a62f22ff37022e7bca954cb28c5714a64dfeaf.json; file:docs/evidence/production-build-receipt-v1/1363d1f1e0b1698823038c6a6a3b1f815321b2502bbb89cd857b1c00bff6f99d.json; command:npm run test:production-build-receipt; command:dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- validate-focused-receipt --receipt docs/evidence/production-build-receipt-v1/87cf06f6c4cf32abd997cd6796a62f22ff37022e7bca954cb28c5714a64dfeaf.json --owner-command scripts/qualify-production.sh --allow-metadata-only true
- **Version:** receipt schema `sir.production-build-receipt/v1`; SDD CLI report version 1.0.0
- **Owner:** EHotwagner/S.I.R. production qualification and FS-GG feedback focused-receipt pattern
- **Recurrence:** new
- **Avoidable cost:** none after the consolidated boundary
- **Disposition:** accepted

## §5 Did not exercise

No new scaffold, gameplay/client behavior, package release, or package upgrade was exercised. Hosted exact-head CI, merge, and post-merge done/release remain pending.

## §6 Doc-versus-behavior contradictions

None observed in product documentation. The pinned coordination version contradicted the live route format as described in §4.1.

## §7 Workarounds still in the tree

None. The retained NuGet cache is aggregate-scoped ignored output, the targeted docs assembly is an explicit downstream prerequisite, and both receipt contracts fail closed rather than bypassing validation.

## §8 Friction and avoidable cost

Coordination required one refused claim plus an engine-resolution retry. Build-boundary discovery cost two rejected clean aggregate candidates. The third functionally green candidate missed the declared 20% threshold at 16.86%; after inventorying all duplicated browser outputs, the consolidated receipt boundary produced 29.61% without weakening acceptance. Focused tests ran during edits; only clean aggregate candidates were considered performance evidence.

## §9 Skill value and gaps

The next-item, intra-repository parallel-work, SDD lifecycle/stage, performance, and feedback skills were exercised. The SDD sequence forced acceptance and performance thresholds before source edits. The feedback skill's focused-receipt pattern eliminated the report/audit rebuild loop. Its main gap was product-specific: it could not name fsdocs' retained dependency and docs-only assembly prerequisites, which had to be learned from clean consumer failures. No gameplay, playtest, or packaging skill was invoked because production behavior and releases were outside scope.

## §10 Outcome markers

The unchanged baseline passed in 373,848 ms with four Fable target builds. The focused receipt test first passed during implementation. The final clean aggregate passed in 263,125 ms with two Fable targets and 2,961 basis points reduction. The stale-output mutation was rejected and restored. SDD evidence is 39/39 observed, verification is ready, and ship is ready. Hosted CI, merge, and done stamp remain pending.

## §11 Falsifiable improvements

- For §4.1, FS-GG/.github#2469 remains the owner: acceptance is a fresh S.I.R checkout reading issue #215 route revision 1 with the repository-pinned client and returning digest `a4009e18…` without an override.
- For the incomplete build-boundary observation in §3 and §8, production qualification should retain a consumer inventory test: in an empty-cache detached checkout, `npm run qualify:production` must reach fsdocs with all five requested assemblies resolvable and no second solution/Fable/Vite build. Acceptance is one clean pass whose Fable log is exactly `main-fable`, `rules-fable`.
- For §4.2, keep focused receipt validation as a merge-boundary invariant: after metadata-only feedback/SDD commits it must pass, while a mutation to source, package lock, tool pin, command script, browser/delivery result, or bound output must fail before reuse.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing product scaffold was unchanged. |
| onboarding-guidance | exercised | AGENTS and route receipt selected canonical Project 6 and the SDD lane. |
| skills | exercised | Next-item, parallel-work, SDD, performance, and feedback guidance drove the cycle. |
| sdd-authoring | exercised | Stages 1–10 completed; 39/39 obligations are observed and ship is ready. |
| implementation-apis | exercised | Receipt CLI, qualification wrapper, and feedback focused-receipt command were implemented and tested. |
| dependencies-build | exercised | Locked clean conformance, retained aggregate NuGet cache, targeted client assembly, Fable, Vite, and fsdocs passed. |
| testing | exercised | Focused receipt mutations and one accepted clean aggregate covered all retained subjects. |
| evidence | exercised | Two immutable content-addressed receipts, TRX/JUnit bindings, timing, verify, and ship artifacts are committed. |
| runtime-playtest | not-exercised | No gameplay change or new runner journey was in scope; existing browser behavior was preserved. |
| performance | exercised | Clean baseline/candidate wall time improved 29.61%, exceeding the declared 20% threshold. |
| documentation | exercised | Standalone docs behavior and receipt-reuse docs behavior both remain represented; clean fsdocs and docs gates passed. |
| packaging-upgrade | not-exercised | No package upgrade or release was performed. |
| worker-git-pr | partial | Claim and metadata seal converged; critic, hosted exact-head CI, merge, stamp, and release are pending. |
