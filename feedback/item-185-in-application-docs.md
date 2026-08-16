---
feedbackSchema: 2
date: 2026-08-15
workspace: S.I.R
cycle: item-185-in-application-docs
lane: sdd
toolVersion: 1.0.1
commit: 9c1f67ae9837d3fc50fb115f08e157986439b83c
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** scaffold-onboarding, lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 4 before PR orchestration
- **checkpoint:** `feedback/checkpoints/item-185-in-application-docs.jsonl` (4 records at this commit)
- **confidence limits:** .NET/Release, Fable/Vite, FsDocs, headless Chromium, structural mutation, deterministic search-performance, SDD verify, and SDD ship are observed. The initial empty-assets build state was not retained and no longer reproduces after restore. No live compositor was available; hosted exact-head CI, independent PR review, and merge remain pending.

## §2 What worked

The existing persistent-workspace qualifications caught registry-inventory drift, an App ownership-ceiling breach, and stale hash-bound visual review artifacts before evidence authoring. The docs feature could then be extracted behind a typed workspace transition module while preserving the single SVG. The production Playwright route observed contextual inspector/overlay entry, local search, history, source links, offline degradation, 320 CSS pixels, 400% zoom, and exact DOM identity/state restoration.

## §3 What did not

The documented first Dev build failed in a clean worktree because it used `--no-restore` before NuGet assets existed. SDD skills pointed to a worked lifecycle corpus absent from this checkout, requiring a prior completed local work package as the shape reference. The first complete docs build needed three attempts because existing ownership and review-binding gates required explicit extraction, regenerated review artifacts, and updated command inventories.

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
- **Evidence:** command:npm run build:docs; file:scripts/test-in-app-docs.mjs; file:src/SIR.Client.Web/WorkspaceTransitions.fs; file:tests/fixtures/persistent-workspace-m0-inventory.json
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
- **Evidence:** file:readiness/185-in-app-docs-modality/in-app-docs-browser.junit.xml; file:readiness/185-in-app-docs-modality/verify.json; command:dotnet fsgg-sdd evidence --work 185-in-app-docs-modality --from-test-report readiness/185-in-app-docs-modality/in-app-docs-browser.junit.xml
- **Version:** FS.GG.SDD 1.0.1
- **Owner:** FS-GG/SDD evidence documentation
- **Recurrence:** new in searched schema-v2 S.I.R. feedback
- **Avoidable cost:** one unnecessary Server.Tests TRX run and one receipt replacement
- **Disposition:** actionable candidate; document JUnit support and examples

## §5 Did not exercise

No simulation-authority algorithm, network protocol, save format, dependency version, remote Markdown, documentation editor, or live-compositor frame-rate changed or is claimed.

## §6 Doc-versus-behavior contradictions

The M0 human command inventory omitted the new Docs commands until its exact fixture rejected the drift. It now lists `workspace.docs` and the four navigation commands. No remaining contradiction was observed.

## §7 Workarounds still in the tree

The focused executable client harness still does not emit its own report. The final SDD receipt therefore observes the production Playwright journey while evidence notes preserve the focused client, manifest, docs, and performance commands.

## §8 Friction and avoidable cost

The missing initial restore and missing lifecycle example each caused one avoidable retry. Product gates then required three complete docs runs, but each failure named a real integration obligation rather than allowing silent drift.

## §9 Skill value and gaps

The pnext, SDD lifecycle, feedback, playtest, game-core, FsDocs build, and technical-documentation skills kept delivery, state preservation, executable proof, and single-source documentation boundaries explicit. The absent lifecycle example corpus is the material skill packaging gap. The playtest doctrine correctly prevented a headless component proof from being presented as the player journey.

## §10 Outcome markers

`npm run build:docs` is green end-to-end. The focused production Playwright journey passes. The generated manifest has 88 pages, 6,598 blocks, 195,249 search tokens, and eight typed source mappings. Search workload p95 is 0.887 ms with no cap failures; `liveCompositorMeasured` remains false. SDD verify reports 49 observed evidence and 49 observed test dispositions with zero blocking findings; ship reports `shipReady`.

## §11 Falsifiable improvements

- Make the first Dev build restore when assets are absent, or name the prerequisite; verify from a clean worktree.
- Package and parser-check the lifecycle example corpus at the paths the generated skills name.
- Keep the five docs-manifest inversions and require a new subject/unreadable inversion for every new manifest gate.
- Preserve the exact production Playwright route for contextual entry, offline source failure, narrow/zoom access, and retained SVG identity/state.

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
| performance | exercised | 0.887 ms p95; structural caps pass; no compositor claim. |
| documentation | exercised | One generated manifest and FsDocs qualification share maintained sources. |
| packaging-upgrade | not-exercised | No dependency or package version changed. |
| worker-git-pr | partial | Claim and exact implementation commit exist; PR/CI/review/merge pending. |
