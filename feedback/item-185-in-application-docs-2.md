---
feedbackSchema: 2
date: 2026-08-15
workspace: S.I.R
cycle: item-185-in-application-docs
lane: sdd
toolVersion: 1.0.1
commit: c8d6f6d6a86f9591f706ad67c894390b3321bea3
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** scaffold-onboarding, lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 6 through independent-review repair round 1 and producer-blocker diagnosis
- **checkpoint:** `feedback/checkpoints/item-185-in-application-docs.jsonl` (6 records at this revision)
- **confidence limits:** .NET/Release, Fable/Vite, FsDocs, headless Chromium, structural mutation, actual production DOM, deterministic search/full-construction performance, SDD verify, and SDD ship are observed. The initial empty-assets build state was not retained and no longer reproduces after restore. No live compositor was available; repaired exact-head hosted CI, critic confirmation, merge, and resolution of FS-GG/FS.GG.Rendering#1243 remain pending.

## §2 What worked

The existing persistent-workspace qualifications caught registry-inventory drift, an App ownership-ceiling breach, and stale hash-bound visual review artifacts before evidence authoring. Independent review then found that broad green gates had not owned every claimed subject. Repair round 1 connected the manifest to selected FsDocs API identities, real links/anchors and exact source digests; made external navigation host-result based behind production CSP; measured actual DOM/full construction; and split SDD receipts by property. The production Playwright route now observes every tactical mode, camera/selection/timeline/panel state, contextual entry, keyboard focus, degraded external access, bounded 320px/400% geometry, and exact SVG identity restoration.

## §3 What did not

The documented first Dev build failed in a clean worktree because it used `--no-restore` before NuGet assets existed. SDD skills pointed to a worked lifecycle corpus absent from this checkout, requiring a prior completed local work package as the shape reference. The first complete docs build needed three attempts because existing ownership and review-binding gates required explicit extraction, regenerated review artifacts, and updated command inventories. Initial evidence then bulk-bound 49 unrelated obligations to one browser JUnit; it was structurally accepted but independently rejected as false ownership.

## §4 Findings

#### §4.1 Generated SDD skill references an absent worked-example corpus

- **Kind:** documentation
- **Impact:** Lifecycle authors lose the parser-validated example named as authoritative by each stage skill.
- **Expected:** `docs/examples/lifecycle-artifacts/` exists wherever the generated skills direct workers to it.
- **Observed:** The referenced corpus was absent, so `work/183-tactical-overlays/` was used as a current validated local shape and analyze still reached `implementationReady`.
- **Evidence:** command:test -e docs/examples/lifecycle-artifacts/spec.md; file:work/183-tactical-overlays/spec.md; file:readiness/185-in-app-docs-modality/analysis.json
- **Version:** FS.GG.SDD 1.0.1
- **Owner:** FS-GG/SDD skill packaging
- **Recurrence:** duplicate of feedback/2026-08-13-sir-item-182-awareness-reaction-windows.md §4.6 and closed FS-GG/FS.GG.SDD#539
- **Avoidable cost:** one failed reference lookup and manual example selection
- **Disposition:** duplicate; add the S.I.R. recurrence evidence to the existing producer finding only

#### §4.2 Existing product gates made documentation integration drift observable

- **Kind:** positive-pattern
- **Impact:** Registry, shell ownership, visual review, and runtime regressions were repaired before the feature claimed evidence readiness.
- **Expected:** Cross-cutting UI changes should fail the exact product boundary they invalidate.
- **Observed:** The full docs route rejected App.fs above 8,200 lines, stale map/persistent-workspace review hashes, and an outdated static-command fixture. Extraction to `WorkspaceTransitions.fs`, deterministic review regeneration, and inventory updates made every M0/M4–M9 gate green. The docs gate independently rejected unreadable JSON, duplicate slugs, broken related links, unsafe block kinds, and stale source mappings.
- **Evidence:** file:scripts/test-in-app-docs.mjs; file:src/SIR.Client.Web/WorkspaceTransitions.fs; file:tests/fixtures/persistent-workspace-m0-inventory.json
- **Version:** commit 9c1f67ae9837d3fc50fb115f08e157986439b83c
- **Owner:** EHotwagner/S.I.R. documentation and persistent-workspace qualification
- **Recurrence:** new positive composition
- **Avoidable cost:** three full docs attempts and two deterministic review regenerations
- **Disposition:** positive-pattern

#### §4.3 Evidence documentation understates accepted observed-run formats

- **Kind:** documentation
- **Impact:** Workers can run an unrelated Test SDK suite solely to obtain TRX even when the focused product journey already emits a machine-readable JUnit receipt.
- **Expected:** The evidence skill names every observed-run report format accepted by SDD 1.0.1.
- **Observed:** `--from-test-report` accepted the real one-test Playwright JUnit receipt and verify reported all 49 evidence/test obligations observed, although the skill repeatedly describes the receipt as TRX.
- **Evidence:** file:readiness/185-in-app-docs-modality/in-app-docs-browser.junit.xml; file:readiness/185-in-app-docs-modality/verify.json
- **Version:** FS.GG.SDD 1.0.1
- **Owner:** FS-GG/SDD evidence documentation
- **Recurrence:** new in searched schema-v2 S.I.R. feedback
- **Avoidable cost:** one unnecessary Server.Tests TRX run and one receipt replacement
- **Disposition:** actionable candidate; document JUnit support and examples

#### §4.4 Structurally valid evidence can still overclaim one receipt's ownership

- **Kind:** friction
- **Impact:** A green lifecycle can conceal that native, Fable, manifest, mutation, performance, feedback, and browser obligations all point at one unrelated testcase.
- **Expected:** Each obligation binds a retained report emitted by the command that owns that property.
- **Observed:** Independent review found 49 references to one one-test Playwright JUnit. Repair round 1 split them across browser, native, Fable production, manifest mutation, performance, and rules-corpus receipts; verify again reports 49 observed, non-synthetic obligations.
- **Evidence:** review:https://github.com/EHotwagner/S.I.R./pull/211#issuecomment-5302691363; file:work/185-in-app-docs-modality/evidence.yml; file:readiness/185-in-app-docs-modality/verify.json
- **Version:** FS.GG.SDD 1.0.1
- **Owner:** FS-GG/FS.GG.SDD receipt import contract and EHotwagner/S.I.R. evidence authoring
- **Recurrence:** duplicate of feedback/2026-08-12-sir-item-194-executable-rules-corpus.md §4.1 and overlapping open FS-GG/FS.GG.SDD#839
- **Avoidable cost:** one independent-review repair round and regeneration of six owning receipts
- **Disposition:** duplicate; current consumer repair retains the split receipt pattern and adds evidence to the existing producer finding

#### §4.5 Commit-aware invalidation indexes candidate-only audits as merged

- **Kind:** defect
- **Impact:** A review repair that changes evidence cited by its own newly authored audit cannot make the required origin/main-to-HEAD invalidation check green without deleting, renaming, or falsifying durable feedback history.
- **Expected:** Base/head mode indexes digest bindings only from audits present in the supplied merge base, as the skill's "merged feedback audits" contract states.
- **Observed:** The item-185 audit is absent from `origin/main`, but the checker enumerates it from the working tree and rejects six repaired paths as merged bindings.
- **Evidence:** command:git cat-file -e origin/main:feedback/audits/item-185-in-application-docs.audit.json; command:dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- check-invalidation --base origin/main --head HEAD; issue:FS-GG/FS.GG.Rendering#1243
- **Version:** FS.GG.Rendering producer commit 8f74ed7296ad1c7c93389cc1d4f989f47bd6061c materialized in S.I.R. commit 3dc50b5839b51b605aee7d9d5f0b1274e0f0f60d
- **Owner:** FS-GG/FS.GG.Rendering feedback-report tool
- **Recurrence:** adjacent to closed #1178/#1194, but new: those cover later changes to base-present audits, while this audit does not exist in the merge base
- **Avoidable cost:** one bounded source diagnosis and a blocked delivery after all product gates passed
- **Disposition:** actionable producer request FS-GG/FS.GG.Rendering#1243; park the consumer item until the canonical tool can distinguish base-present audits

## §5 Did not exercise

No simulation-authority algorithm, network protocol, save format, dependency version, remote Markdown, documentation editor, or live-compositor frame-rate changed or is claimed.

## §6 Doc-versus-behavior contradictions

The M0 human command inventory omitted the new Docs commands until its exact fixture rejected the drift. It now lists `workspace.docs` and the four navigation commands. No remaining contradiction was observed.

## §7 Workarounds still in the tree

The focused executable client harness still does not emit a report directly. A retained command-specific JUnit now records its successful native boundary, separately from browser, Fable, manifest, performance, and rules-corpus receipts.

## §8 Friction and avoidable cost

The missing initial restore and missing lifecycle example each caused one avoidable retry. Product gates then required three complete docs runs. Independent review required one additional repair round because the initial browser, manifest, performance, CSP, source-pin, historical-invalidation, and SDD evidence claims were materially narrower than their acceptance language. The invalidation self-index defect required one bounded diagnosis and blocks delivery despite green product and SDD gates.

## §9 Skill value and gaps

The pnext, SDD lifecycle, feedback, playtest, game-core, FsDocs build, and technical-documentation skills kept delivery, state preservation, executable proof, and single-source documentation boundaries explicit. The absent lifecycle example corpus is the material skill packaging gap. The playtest doctrine correctly prevented a headless component proof from being presented as the player journey.

## §10 Outcome markers

The repaired Release solution, client build, rules-corpus gate, focused production Playwright journey, and manifest qualifier pass. The generated client manifest has 83 metadata/API pages, 6,433 blocks, 195,912 search tokens, four selected API identities, and eight exact source mappings. The retained performance run measured 238 production DOM nodes, search p95 below 2 ms, and full-construction p95 below 11 ms; both real-subject budget inversions failed as required and `liveCompositorMeasured` remains false. SDD verify reports 49 observed evidence and 49 observed test dispositions across six owning receipts with zero blocking findings; ship reports `shipReady`.

## §11 Falsifiable improvements

- Make the first Dev build restore when assets are absent, or name the prerequisite; verify from a clean worktree.
- Package and parser-check the lifecycle example corpus at the paths the generated skills name.
- Keep the eight docs-manifest inversions and require a new subject/unreadable inversion for every new manifest gate.
- Preserve the exact production Playwright route for contextual entry, host-observed external failure, CSP, narrow/zoom access, every tactical mode, and retained SVG/camera/selection/timeline/panel identity.
- Require property-owning observed-run receipts; reject bulk fan-out from one generic testcase even when structural verification accepts it.
- In base/head invalidation mode, index only audits present in the supplied base tree and retain fail-closed tests for genuinely merged bindings.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing product evolved in place. |
| onboarding-guidance | exercised | First Dev command failed before explicit restore. |
| skills | exercised | pnext, SDD, feedback, playtest, game-core, and FsDocs guidance applied. |
| sdd-authoring | exercised | Complete charter through ship; shipReady. |
| implementation-apis | exercised | Shared typed manifest, navigation, and source mapping compile in .NET/Fable. |
| dependencies-build | exercised | Release, Fable/Vite, FsDocs, and publish pass. |
| testing | exercised | Unit/executable, mutation, full docs, and production Chromium routes pass. |
| evidence | exercised | 49 declarations are real, observed, non-synthetic passes. |
| runtime-playtest | exercised | Player controls drive Docs and return to the exact retained tactical SVG. |
| performance | exercised | Actual DOM, search, and full-construction subjects pass with red inversions; no compositor claim. |
| documentation | exercised | One generated manifest and FsDocs qualification share maintained sources. |
| packaging-upgrade | not-exercised | No dependency or package version changed. |
| worker-git-pr | partial | Claim, PR, independent review, repair commits, and producer blocker #1243 exist; repaired CI/re-review/merge are pending. |
