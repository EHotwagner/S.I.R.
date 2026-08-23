---
feedbackSchema: 2
date: 2026-08-23
workspace: S.I.R
cycle: item-232-viewport-chunking-semantic-zoom
lane: sdd
toolVersion: 1.0.1
commit: 6eb2aa506bec76815174b7e67181610339c695dc
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 23
- **zero-event reason:** n/a

Scaffold/lifecycle: `fs.gg.sdd.cli` 1.0.1 (manifest-pinned; a global 1.1.0 shadows it on `PATH` — §4.3), `fs.gg.coord.cli` 0.71.0, .NET SDK 10.0.302 pinned with `rollForward: disable`. Checkpoints: `feedback/checkpoints/item-232-viewport-chunking-semantic-zoom.jsonl`, 23 events (2 recorded by the prior worker on the same cycle id, 21 by this one).

This cycle **resumed an orphaned in-flight branch** rather than starting fresh: `origin/item/232-viewport-chunking` carried a complete SDD front-half and ~4,500 lines of implementation with no PR, no claim and no lease. Findings below therefore split into two populations — the *scaffold/orchestration* experience of discovering and adopting that branch, and the *product* defects the branch itself carried.

Confidence limits: the item is blocked from merging by S.I.R.#264 and did not reach `verify`/`ship`, so no observation here covers the merge boundary or post-merge obligations. Performance numbers in §4.6 are measured from a harness whose own verdict field is not trustworthy (S.I.R.#268); only the measured values are relied on.

## §2 What worked

- **The `fsgg-sdd analyze` readiness report was accurate and decisive.** One command (`analyze --work 232-viewport-chunking-semantic-zoom --text`) returned `implementationReady`, 84/84 relationships, zero blocking/warning/stale findings, and settled a contested question — whether the branch's SDD package was real — in seconds. It directly contradicted a published instruction to re-author the package from scratch, and it was right.
- **`fsgg-coord widen` with a `#353` collision verdict made scope growth safe to do incrementally.** Fourteen widen calls across the cycle, every one returning `verdict: disjoint`, `collisions: []`, against three concurrently live lanes. Growing a touch-set turned out to be the normal motion of this work rather than an exception, and the tool never once needed a retraction.
- **Per-gate red→green measurement localised a defect class faster than reading could.** The `documentation` gate reports all its failures together rather than throwing on the first. That made "14 of the 16 reported failures cleared by one comparator change" observable as a single measurement, which is what identified the defect class rather than the individual defects. The count is from the gate's own aggregated failure list before and after the change and is not otherwise pinned to a retained artifact.
- **Committed inversion harnesses find things transcripts cannot** — see §4.1.

## §3 What did not

- The first root cause published for the route-commit defect was **wrong** (a stale `disabled` attribute) and had to be retracted after six escalating runtime probes. The correct cause was the opposite: the commit succeeded and the *render* never happened. The retraction is recorded as its own checkpoint rather than by rewriting the first.
- An initial extraction boundary was chosen (`persistentSceneSvg` and the whole retained-scene surface) that turned out to be the *worst* candidate, because five source-scanning gates pin tokens in it and four read `App.fs` alone. The extraction was still correct; the cost was that the gate repairs became a substantial second body of work discovered afterwards rather than planned for.
- Three assertion sets had to be repaired in the review harnesses because the delivered feature legitimately changed what they were measuring (always-mounted inactive panel hosts, retained terrain geometry grouping, a preview polyline satisfying a "committed route" wait). Each looked like a product bug first.

## §4 Findings

#### §4.1 A re-pointed source-scanning gate silently stops biting, and only an executable inversion catches it

- **Kind:** positive-pattern
- **Impact:** Six ownership gates were re-pointed from `App.fs` to a composed surface after an extraction; without proof, all six could have become decorative while reporting green.
- **Expected:** `pnext-item` §3 requires every gate a change modifies to ship with evidence it can fail.
- **Observed:** Writing that evidence as a committed script rather than performing the runs found two defects a transcript would have hidden: one gate's inversion was passing *for the wrong reason* (a stale review binding failed first), and one gate **survived** its inversion entirely — its single-owner regexes had no word boundary, so `persistentSceneSvgRenamed` still matched `let persistentSceneSvg`.
- **Evidence:** command:npm run build:client && npm run review:map-editor && npm run review:persistent-workspace-m9 && npm run review:tactical-visual && node scripts/test-composed-app-surface-inversions.mjs; file:scripts/test-composed-app-surface-inversions.mjs
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — `scripts/test-composed-app-surface-inversions.mjs`
- **Recurrence:** new
- **Avoidable cost:** none — this prevented cost
- **Disposition:** product fix

The generalisable part is the **green-baseline pre-check**: an inversion is only evidence if the gate was green immediately before the violation. Without that pair, a gate red for an unrelated reason reads as a passing inversion and the harness certifies a check it never exercised.

**Three boundaries on this claim, two of which an independent critic had to put back.** First, the harness has a **precondition**: three of the seven gates boot the production bundle and compare hash-bound review artifacts, so without `npm run build:client` and a regeneration of those artifacts they cannot reach a green baseline at all, and the harness correctly refuses to certify them. Run without that precondition it reports 4/7, not 7/7 — the refusal is the pre-check working, but a bare "7/7" without the precondition is not a reproducible claim. Second, **`m8-timeline` requires no positive token that moved out of `App.fs`**, so re-pointing it is *inert* for its positive assertions and only stricter for its negative ones; its inversion demonstrates that the dead-branch scan reaches the extracted module, not that a moved `m8` token is still guarded. Third, the harness is **not side-effect-free**: at least one gate rewrites a review telemetry artifact with machine-dependent timing and heap values simply by running, so a run leaves the tracked tree changed. The harness now snapshots `git status --porcelain` before and after and fails naming any path it left modified — restoring only the file it deliberately mutates while calling the tree clean was itself a check that could not fail. Relatedly, because review artifacts are hash-bound and regenerated per machine, "7/7 at commit X" is reproducible only after a local regeneration that itself produces a diff.

The first two statements are in the harness source and in `mutation-evidence.md`, and an earlier draft of this report omitted both — in the direction that made the work look stronger. The transferable rule is in §9: **when a report cites an artifact that carries a caveat, the caveat travels with the citation.**

#### §4.2 An unclaimed `item/<n>-*` branch is invisible to every scheduler collision check (PARTIALLY SUPPORTED)

- **Kind:** capability-gap
- **Impact:** The board was one dispatch away from re-authoring a complete SDD package and reimplementing ~4,500 lines.
- **Expected:** A branch carrying finished work for an item is discoverable before that item is dispatched.
- **Observed:** `who` reports nothing (no claim marker), `batch --explain` lists the item as schedulable, `delivery-route show` reports `sddPackageReady: false` because it reads the *working tree* rather than the item's branch, and `item-pr-open` keys on an open PR rather than on a branch. All four are individually correct; the composite answer is wrong.
- **Evidence:** command:scripts/fsgg-coord delivery-route show EHotwagner/S.I.R.#232; issue:EHotwagner/S.I.R.#232; issue:EHotwagner/S.I.R.#274
- **Version:** fs.gg.coord.cli 0.71.0
- **Owner:** FS-GG/.github — `fsgg-coord` scheduling and delivery-route package readiness
- **Recurrence:** new
- **Avoidable cost:** one published instruction telling the next claimant to author charter-through-analyze from scratch, plus a correction comment
- **Known unknowns:** **three of the four legs are author-assertion and could not be evidenced.** The `who`, `batch --explain` and `item-pr-open` observations were recorded against a board state that no longer exists — this item is now claimed, so re-running them reports the present state rather than the one observed, and `fsgg-coord` is a compiled engine whose internals are not readable. Only the `delivery-route show` mechanism is independently checkable: it reads the **working tree**, which is why it reports `sddPackageReady: true` from a worktree that carries the package and reported `false` from one that did not. The composite claim — that all four collision checks miss an unclaimed branch simultaneously — rests on the original observer's transcript, which I do not hold. Retained because the consequence was real and is recorded on S.I.R.#274, but it is not verified evidence and is labelled here rather than presented as such.
- **Disposition:** existing issue

#### §4.3 A globally installed lifecycle CLI silently shadows the repository-pinned one

- **Kind:** friction
- **Impact:** A lifecycle stage rewrote a generated artifact's `generator` stamp to a version CI cannot reproduce.
- **Expected:** Running `fsgg-sdd` in a repository with a pinned `.config/dotnet-tools.json` uses the pinned version.
- **Observed:** `fsgg-sdd` on `PATH` is a global 1.1.0; the manifest pins 1.0.1. `analyze` rewrote `"generator": "FS.GG.SDD.Artifacts/1.0.1"` to `1.1.0` with no warning.
- **Evidence:** command:dotnet tool run fsgg-sdd -- --version; file:.config/dotnet-tools.json
- **Version:** reproduced with global 1.1.0 against pinned 1.0.1
- **Owner:** FS-GG/.github — tool resolution guidance; `fs-gg-sdd-lifecycle` skill
- **Recurrence:** new
- **Avoidable cost:** one reverted generated artifact and a switch to `dotnet tool run` for every subsequent stage
- **Disposition:** doc fix

#### §4.4 `pnext-item` §6 claims `verify-paths` catches a missing closing keyword; it does not

- **Kind:** documentation
- **Impact:** A PR that would have delivered its item without closing it, unrepairable after merge under `.github#2107`.
- **Expected:** Per `pnext-item` §6, `verify-paths` run right after opening the PR catches a `Refs`-instead-of-`Closes` body "while it is still free to fix".
- **Observed:** `verify-paths --pr 253` reported touch-set drift only. It resolves the item binding from the *branch name* when the body cannot supply it, so a PR on a correct `item/<n>-*` branch with a `Refs` body passes cleanly and still fails to close its issue.
- **Evidence:** command:scripts/fsgg-coord verify-paths --pr 253 --repo S.I.R.; file:.claude/skills/pnext-item/SKILL.md
- **Version:** fs.gg.coord.cli 0.71.0
- **Owner:** FS-GG/.github — `verify-paths` and the `pnext-item` skill text
- **Recurrence:** new
- **Avoidable cost:** one manual PR body repair
- **Disposition:** doc fix

#### §4.5 A byte-identity source pin blocks any item that touches a pinned file

- **Kind:** defect
- **Impact:** Blocks this item entirely, and with it five dependent board rows. Broader than a growth problem: it blocks any item that *touches* a pinned file.
- **Expected:** An item whose declared `Paths:` include a file can change that file.
- **Observed:** `tests/fixtures/rules-corpus/v2/implementation-sources.json` pins `src/SIR.Client.Web/App.fs` **byte-identical** to its text at `eb0b2c29`. Advancing that pin requires a commit already ancestral to `origin/main` (`scripts/verify-rules-corpus.sh:37,43`), which a PR by definition is not. **The pin therefore forbids any change to a pinned file, in either direction** — this item's `App.fs` is now **7,553 lines**, comfortably under the separate 8,200-line ceiling at `scripts/test-map-editor-qualification.mjs:100`, and `verify-rules-corpus.sh` still fails. An earlier draft of this finding claimed the pin and the ceiling formed an *unsatisfiable pair* with 8 lines of margin; that framing is wrong and the delivered head disproves it. The margin is 7 (the gate counts `split("\n").length`, 8193 for an 8192-line file), and the ceiling is not the mechanism at all.
- **Evidence:** command:bash scripts/verify-rules-corpus.sh; file:tests/fixtures/rules-corpus/v2/implementation-sources.json; issue:EHotwagner/S.I.R.#264
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — rules-corpus source pinning and CI gate composition
- **Recurrence:** seen again S.I.R.#264; also blocks S.I.R.#249 via `Simulation.fs`
- **Avoidable cost:** the item cannot land at all
- **Disposition:** existing issue

#### §4.6 A measurement harness that could not start, whose verdict is a constant

- **Kind:** defect
- **Impact:** The item's declared performance obligation had no runnable measurement route, and the artifact it produces cannot express failure.
- **Expected:** `scripts/measure-svg-pipeline.mjs` produces a performance verdict against the declared `viewport-visible-v1` budget.
- **Observed:** It read `artifacts/publish/.vite/manifest.json`; `dotnet publish` emits that file under `wwwroot/`, so the harness threw before doing any work. Nothing caught it because no workflow job invokes the harness. Separately, its `result` is a hardcoded `"pass"` literal with no budget comparison anywhere in the harness or its library.
- **Evidence:** command:node scripts/measure-svg-pipeline.mjs --out artifacts/232-svg --fixtures all --journeys all; file:scripts/measure-svg-pipeline.mjs; issue:EHotwagner/S.I.R.#268
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — SVG pipeline measurement harness and its CI wiring
- **Recurrence:** new; filed as S.I.R.#268
- **Avoidable cost:** the path repair was required before any measurement could be taken at all
- **Disposition:** existing issue

The path is repaired in this item because it is load-bearing for the item's own obligation. The hardcoded verdict is deliberately **not** repaired here: an item must not repair the gate it is judged by, and that fix needs its own inversion evidence against a fixture that genuinely breaches a budget.

#### §4.7 A memoisation scheme keyed on shape cannot notice content, and every failure is silent

- **Kind:** defect
- **Impact:** Six production regressions from this single defect class, each presenting as an unrelated symptom. (§10 counts **seven** product defects for the branch: these six plus the viewBox framing defect of §4.19, which is a different mechanism.)
- **Expected:** A memo comparator re-renders when the state its subtree renders changes.
- **Observed:** Five comparators and content tokens each omitted state their subtree renders; the sixth, and the root, is that the scene owner's revision keys count *primitives* rather than their content. A route moving from preview to planned keeps the route count, keeps the simulator projection's `RevisionIdentity` (the editor digest, untouched by a route commit) and keeps the tick — so nothing re-renders. Every one of the six failed **silently, as a frozen subtree**, never as an error; the route case presented as the domain rejecting a command, with no event and no error anywhere.
- **Evidence:** file:src/SIR.Client.Web/TacticalScenePresentation.fs; command:node scripts/test-map-editor-qualification.mjs; file:work/232-viewport-chunking-semantic-zoom/mutation-evidence.md
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — client presentation memoisation
- **Recurrence:** new
- **Avoidable cost:** seven red CI gates whose shared cause was invisible; six independent root-cause investigations; one published root cause retracted
- **Disposition:** product fix

The transferable rule, applied throughout the fix: **where the fact set is open, compare identity rather than enumerating facts.** Reference equality on an immutable Elmish record is exact by construction — a changed record is a new record — O(1), and impossible to leave a fact out of. Enumeration was tried first and silently omitted layer domains, gestures, previews, cursors and the raster background.

#### §4.8 A green pure-unit suite cannot observe a memoised renderer

- **Kind:** quality-gap
- **Impact:** The fastest local suite passed while fourteen production DOM behaviours were broken.
- **Expected:** The client qualification suite detects a scene that stops re-rendering.
- **Observed:** The complete .NET suite passed green — including the projection, semantic-zoom hysteresis and node-budget assertions — while the React memo owning the tactical scene skipped every editor re-render. Only the happy-dom bundle gate could see it. **Known unknown:** the count of broken behaviours is taken from the `documentation` gate's aggregated failure list at the time (14 of 16 entries), and that pre-fix run is not retained as an artifact; the claim cannot be re-derived from the repaired head.
- **Evidence:** command:dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release; command:node scripts/test-map-editor-qualification.mjs
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — client qualification coverage for memoised presentation owners
- **Recurrence:** new
- **Avoidable cost:** fourteen simultaneous CI failures invisible to the fastest local loop
- **Disposition:** accepted

#### §4.9 Seven exact assertions were relaxed defensively; none of the relaxations was necessary

- **Kind:** quality-gap
- **Impact:** Selection-only semantic ID invariance had no projection-level test at all.
- **Expected:** An assertion is relaxed only when the delivered behaviour makes it unsatisfiable.
- **Observed:** Seven exact equalities had been turned into ranges and set intersections as a defensive response to viewport culling. Each was restored and the suite re-run: **all seven pass unchanged.**
- **Evidence:** file:tests/SIR.Client.Tests/TacticalSceneProjectionQualification.fs; command:dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — projection qualification suite
- **Recurrence:** new
- **Avoidable cost:** one invariant lost for the life of the branch
- **Disposition:** product fix

The cheap general check: **restore the exact form and run it.** Deciding by argument whether a relaxation is "forced by the new behaviour" is slower and, here, would have been wrong seven times out of seven.

#### §4.10 A Fable target rejects an API the .NET Release build accepts (INHERITED, UNSUPPORTED)

- **Kind:** friction
- **Impact:** A green .NET build is not evidence the production targets compile.
- **Expected:** Shared client source compiles for .NET and both Fable targets alike.
- **Observed:** `Double.IsFinite` was accepted by the .NET Release build and rejected by both production Fable targets, requiring a compatibility edit and a full production rebuild.
- **Evidence:** command:npm run build:client; file:src/SIR.Client/TacticalSceneProjection.fs
- **Version:** Fable 5.13.0
- **Owner:** EHotwagner/S.I.R. — browser-compatible projection API
- **Recurrence:** first seen this cycle (recorded by the prior worker on the same cycle id)
- **Avoidable cost:** one failed production build and one full rerun
- **Known unknowns:** this is inherited from the prior worker's checkpoint and **could not be verified**. `Double.IsFinite` appears in no source file at any ref; `git log -S 'Double.IsFinite'` finds it only in the checkpoint text itself, which merely restates the claim. The file carries `not (Double.IsNaN ... || Double.IsInfinity ...)`, which is the *shape* of the described workaround but is not evidence that the rejection happened. Retained as a disclosed observation, not as an actionable finding.
- **Disposition:** accepted

#### §4.11 Always-mounted inactive panel hosts break first-match DOM selectors in review harnesses

- **Kind:** friction
- **Impact:** Three review-harness assertions failed in ways that read as product bugs.
- **Expected:** A harness selecting `.editor-tools-panel` measures the panel a viewer sees.
- **Observed:** The delivered work mounts all four workspace panel hosts simultaneously, with inactive ones `[hidden]`/`[inert]` and therefore boxless. A first-match selector picks one of those whenever the audited workspace is not Editor and then measures a 0×0 rect against a real host.
- **Evidence:** file:scripts/lib/persistent-workspace-browser-audit.mjs; command:npm run review:tactical-visual
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — persistent workspace browser audit
- **Recurrence:** new
- **Avoidable cost:** three misdiagnosed harness failures
- **Disposition:** product fix

#### §4.12 A "committed route" wait satisfied by an uncommitted preview

- **Kind:** quality-gap
- **Impact:** A review generator measured a scene whose last route was still a preview, and the resulting board recorded it.
- **Expected:** A wait for a committed route observes a committed route.
- **Observed:** The wait counted any `polyline` in the routes layer; an uncommitted preview is a polyline, so it was satisfied before the commit landed. The race had been winning by accident until frame coalescing moved the commit a frame later.
- **Evidence:** file:scripts/generate-tactical-visual-review.mjs
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — tactical visual review generator
- **Recurrence:** new
- **Avoidable cost:** one misdiagnosed product defect
- **Disposition:** product fix

#### §4.13 The declared performance budget is breached on four of seven journeys, and a fifth on input-to-paint

- **Kind:** quality-gap
- **Impact:** The item's own composite acceptance criterion — that the exact candidate satisfies its p95/p99 and catch-up budgets — is not met. This is the single largest product-quality fact of the cycle.
- **Expected:** `viewport-visible-v1` declares p95 <=16 ms, p99 <=32 ms and zero catch-up frames, with `requiredCapability: production-chromium-trace` and `liveCompositorRequired: true`.
- **Observed:** Measured on the shipped candidate (`15cee9e`) over 8 fixtures x 7 journeys — 56 production Chromium runs, AnimationFrame timing records and renderer RunTask slices, no injected frame observer.

  | journey | dropped | p95 ms | p99 ms | worst | max input→paint | breaches |
  |---|---|---|---|---|---|---|
  | playback | 9 | 213.0 | 261.9 | 261.9 | 15.2 | catch-up, p95, p99 |
  | modality-transition | 21 | 37.7 | 45.2 | 45.2 | 15.4 | catch-up, p95, p99 |
  | pan | 0 | 19.9 | 20.8 | 20.8 | **18.4** | p95, input-to-paint |
  | zoom | 0 | 17.1 | 17.2 | 17.2 | 0.4 | p95 |
  | selection | 0 | — | — | — | **20.4** | input-to-paint |
  | idle | 0 | — | — | — | — | clean |
  | dense-overlay | 0 | — | — | — | — | clean |

  Against p95 ≤16 ms, p99 ≤32 ms, zero catch-up frames. **Four journeys breach frame budgets; `pan` breaches input-to-paint as well; `selection` breaches only input-to-paint and drops no frames.** The structural half of the same intent — queried chunks, emitted spatial primitives, semantic duplicates, offscreen focusable nodes — **is** met and is separately gated.
- **Evidence:** file:readiness/232-viewport-chunking-semantic-zoom/performance-evidence.json; file:work/232-viewport-chunking-semantic-zoom/evidence.yml
- **Version:** measured at the delivered head
- **Owner:** EHotwagner/S.I.R. client presentation; upstream FS-GG/FS.GG.Rendering#1256
- **Recurrence:** seen again — the prior worker measured the same shape on one fixture (playback 34 ms / 1 drop, modality 34 ms / 2 drops) and filed the producer capability as FS-GG/FS.GG.Rendering#1256
- **Avoidable cost:** none avoidable here; the missing capability is upstream
- **Disposition:** existing issue

Recorded as an accepted deferral (`EV022`) with all four required fields rather than a pass. A pass would misstate the measurement, and a synthetic pass would not satisfy the obligation anyway. The declaration binds to the measured values and explicitly refuses the producing harness's own verdict field (§4.6).

#### §4.14 The evidence stage demands a performance binding and then rejects the declaration carrying it

- **Kind:** defect
- **Impact:** Applies to any work item declaring an active `performanceIntent`, which on this board is every rendering item. **Corrected scope (see §4.22):** this does not block one obligation — it blocks the *write*, so `--from-test-report` stamps nothing and the observed-run gate behind it can never be satisfied while it stands. **Not the sole blocker here:** the observed-run gate stands behind it, so removing this defect alone would not by itself make `verify` reachable for #232 — but the two are chained, and this one is what stops the receipt route being attempted at all.
- **Expected:** Binding an active `performanceIntent` into `performanceBudget.intent` with a cited measured artifact clears `evidence.performanceIntentUnbound`.
- **Observed:** Bisected over five variants of one declaration:

  | declaration | `evidenceInvalid` | outcome |
  |---|---|---|
  | deferral, no `performanceBudget` | 0 | `evidenceDeferred: 1` — accepted |
  | deferral, `performanceBudget` with nested `intent` | 1 | rejected |
  | deferral, `performanceBudget` without nested `intent` | 1 | rejected |
  | verification / `result: pass`, `performanceBudget` present | 1 | rejected |
  | verification / `result: pass`, `performanceBudget` removed | 0 | `evidenceSupported: 1` — accepted |

  **Any `performanceBudget` block invalidates the declaration** — independent of `kind`, of `result`, and of whether the nested `intent` is present. **The rule is narrower than this bisection could show**: the likely rule is not "any block invalidates" but *a budget cannot be declared while its intent is unbound*. Every variant reachable from this item was unbound, because `production-chromium-trace` does not exist, so **boundness is the one variable this bisection could not vary**. What I verified by reading, and can cite: `work/184-scenario-catalog/evidence.yml` declares a `performanceBudget` with the same `disposition: active`, and `readiness/184-scenario-catalog/performance-evidence.json` exists (7,977 bytes) — so that item's intent binds and mine cannot. I did **not** re-run the stage against that item, because doing so writes into another work item's artifact; that its declaration validates under the pinned tool today is reported to me rather than measured by me, and is not relied on here — while `evidence.performanceIntentUnbound` keeps firing, so the stage never sees the binding it rejected the declaration for carrying. The block was modelled on `work/184-scenario-catalog/evidence.yml`.

  *(An earlier draft also cited a companion `evidence.unsupportedResultState` reporting a phantom `pending`. That was observed against an intermediate state of the work item — plan decisions and task statuses since reverted — and does not reproduce at the delivered head. Withdrawn rather than carried.)*
- **Evidence:** command:dotnet tool run fsgg-sdd -- evidence --work 232-viewport-chunking-semantic-zoom --json; file:work/232-viewport-chunking-semantic-zoom/evidence.yml; file:work/184-scenario-catalog/evidence.yml; issue:EHotwagner/S.I.R.#271
- **Version:** fs.gg.sdd.cli 1.0.1 (manifest-pinned). **Not** re-tested against the global 1.1.0 present on the machine (§4.3), because using it would produce artifacts CI cannot reproduce; whether 1.1.0 behaves differently is a known unknown.
- **Owner:** FS-GG/FS.GG.SDD — evidence stage `performanceBudget` validation
- **Recurrence:** new
- **Avoidable cost:** the lifecycle back half is unreachable for this item, and the performance obligation had to be declared **without** its `performanceBudget` block to be accepted at all — the budget intent relocated to a committed readiness artifact rather than declared where the schema wants it
- **Disposition:** issue

**What was actually shipped.** The obligation is declared as a deferral with all four required fields and **no `performanceBudget` block** — the only form the stage accepts while the intent is unbound — and `evidence` reaches `evidenceReady` (29 supported, 8 deferred, 0 invalid). The budget itself, the measured per-journey breaches, `satisfiesDeclaredBudget: false` and the upstream dependency are recorded in the committed `readiness/232-viewport-chunking-semantic-zoom/performance-evidence.json`, which the deferral's `artifacts:` cites and whose rationale says why the block is absent. That is relocation with a pointer, not omission: nothing about the budget is lost, but it lives outside the field the schema provides for it, which is the cost this defect imposes.

**A second, sharper defect in the same stage, established by a controlled run against a disposable clone** (transcript recorded on S.I.R.#271; two earlier readings of mine and of the host's are marked superseded there). Running the stage against a work item whose recorded source-snapshot digest disagrees with the file it names **reconciles the record to the file in place** — an inspection-shaped invocation is a write. It is idempotent (a second run reports `noChange`) and `--dry-run` suppresses it. The defect is not the write itself:

> the run that wrote reported **`staleCount: 0`**.

A counter reading *"nothing drifted"* while the tool repairs drift in the same invocation is the §4.21 hazard in its purest form — two individually accurate outputs that jointly mislead, where the mislead is in the direction of reassurance. Anyone reading `staleCount` to decide whether an item needs attention gets a clean answer from a run that just changed the item.

Earlier drafts of this report carried a separate finding proposing that the trigger was *stale* snapshots. That reading is refuted by `staleCount: 0` on the writing run and has been withdrawn; my own item's no-op is explained rather than explained away — its records already agreed, because the same tool had authored them minutes earlier.

#### §4.15 Source-isolated presentation timing is unavailable in the production trace

- **Kind:** capability-gap
- **Impact:** Pan cost could be localised only to generic main-thread script, not to a specific presentation owner, so the measured breach in §4.13 cannot be attributed to a component.
- **Expected:** A production Chromium trace attributes presentation cost to the Elmish/React owners that incurred it.
- **Observed:** The exact production trace localised pan cost to generic main-thread script; source-isolated Elmish/React timing was unavailable, and the inherited harness assumed uncropped global unit projection.
- **Evidence:** command:node scripts/measure-svg-pipeline.mjs --out artifacts/232-svg --fixtures all --journeys all; file:scripts/measure-svg-pipeline.mjs
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — SVG pipeline measurement and client render instrumentation
- **Recurrence:** first seen this cycle (recorded by the prior worker on the same cycle id)
- **Avoidable cost:** three aborted trace attempts plus one diagnostic trace, recorded by the prior worker
- **Disposition:** accepted

#### §4.16 A single-source-of-truth refactor is invisible to a text-pinned gate until something forces the gate to run

- **Kind:** quality-gap
- **Impact:** A gate silently stopped covering the thing it names, with no error from either side.
- **Expected:** A gate pinning an ownership token either keeps covering that token or fails.
- **Observed:** Moving the panel-to-feature mapping into `ClientFeatureRuntime.panelFeature` — so the memo comparator and `requestSupportingPanel` cannot drift apart — changed the pinned literal `elif panelId = "samples" then FeatureLoader.samples` into `... then Some FeatureLoader.samples`. The M8 gate pinned the old shape. Nothing reported anything until the inversion harness tried to establish a green baseline for M8 and refused to certify it. The refactor was correct and the gate was correct; the pin just quietly stopped applying.
- **Evidence:** file:scripts/test-timeline-supporting-panels-m8-qualification.mjs; file:src/SIR.Client.Web/ClientFeatureRuntime.fs; command:npm run test:composed-app-surface-inversions
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — text-pinned ownership gates
- **Recurrence:** seen again — the same class as M0's `controls model dispatch` pin, repaired earlier in this cycle
- **Avoidable cost:** none here, because the harness caught it; unbounded without one
- **Disposition:** product fix

Repaired by updating the pin to the new exact shape rather than reverting the refactor: the token stays exact, only its shape moved. The general point is the one worth keeping — **a text pin does not fail when the code it names moves; it just stops matching, and only something that forces the gate to run will say so.** This is the third time in this cycle that the green-baseline pre-check caught a gate that had stopped applying, and all three times it caught the author of the harness.

#### §4.17 Deleting a gate's input makes the gate skip the item

- **Kind:** defect
- **Impact:** Any work item can silence the SDD evidence gate by removing one file. The gate reports nothing rather than failing when the thing it protects is absent.
- **Expected:** A gate that protects an SDD work item's evidence either evaluates it or fails.
- **Observed:** `scripts/qualify-pr.sh:444` is `[[ -f "work/$work_id/evidence.yml" ]] || continue`. Found while separating two blockers by experiment, one variable at a time:

  | state | `verify` |
  |---|---|
  | 37 tasks `done`, 1 obligation declared | blocked — `doneTaskMissingEvidence` (T001–T037) |
  | tasks moved back to `pending`, `evidence.yml` kept | **still blocked** — `evidence.missingRequiredEvidence`, `verifySkillMissing: 8` |
  | `evidence.yml` absent | **the gate skips the work item entirely** |

  The second row settles causation — obligations are required by `tasks.yml`'s own `requiredEvidence` independent of task status, so at that point the 36 then-unauthored obligations blocked on their own. (All 37 were authored later; see §4.22. And the claim in an earlier draft that §4.14 blocks "exactly one of 37" is withdrawn there — it blocks the write, not one obligation.) The third row is the defect, and it is unaffected by either: the gate's coverage is conditional on the presence of the file it audits.
- **Evidence:** file:scripts/qualify-pr.sh; file:work/232-viewport-chunking-semantic-zoom/tasks.yml; issue:EHotwagner/S.I.R.#272
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — `qualify-pr.sh` evidence gate conditionality; filed as **S.I.R.#272**
- **Recurrence:** seen again — same class as the path-conditional integrity subject repaired by S.I.R.#252
- **Avoidable cost:** none here; the removal in this cycle was independently correct, which is how the conditionality surfaced
- **Disposition:** existing issue

Removing the file in this cycle was the honest move — a one-obligation `evidence.yml` asserted a lifecycle stage the item has not reached, and removing it made the recorded state *more* accurate. That it also turned the gate green is the finding: the same action is available to anyone who simply does not want to be audited.

#### §4.18 Measure the baseline before forming a hypothesis, not after the first one fails

- **Kind:** quality-gap
- **Impact:** Three disproven hypotheses and two full build-publish-run cycles on a failure that one detached baseline run settled in four seconds.
- **Expected:** When a gate fails on a branch, establish whether it passes on `origin/main` before theorising about a cause.
- **Observed:** Applied once and skipped once **in the same cycle**. For the `review:tactical-visual` geometry failure I checked `main` first, found the generator green there, and that prevented a wrong repair. For the click-intercept failure I did not, and instead formed and tested three hypotheses in sequence — the rAF presentation owner, the viewport-measurement timing, and the camera transform being applied imperatively — each requiring a build, a publish and a browser run. The eventual baseline run took **4.0 seconds** and produced a bisectable difference immediately.
- **Evidence:** command:npx playwright test in-app-docs --config tests/SIR.Browser.Tests/playwright.config.js; file:src/SIR.Client/MapEditorWorkspace.fs
- **Version:** n/a
- **Owner:** this investigation practice, not a component
- **Recurrence:** new
- **Avoidable cost:** three hypotheses, two build-and-run cycles
- **Disposition:** accepted

What makes the rule worth recording rather than merely recommending is that it is **demonstrably transferable and was demonstrably not transferred**: the same worker applied it correctly to one failure and skipped it on the next, an hour apart. A rule you know and do not carry across is better evidence of its value than one you have only argued for.

**Three occasions in this cycle were corrected by a comparison run rather than by further reasoning**: a published root cause retracted (the route-commit defect was a revision-key failure, not a stale `disabled` attribute); three hypotheses about the click intercept disproven by one 4.0-second detached run on `origin/main`; and an over-strong justification withdrawn when the baseline turned out not to exist at all.

An earlier draft generalised those three into a claim that self-assessed confidence had no relationship to correctness. **That claim does not survive its own evidence**: the three cases were selected *because* they were wrong, and the same report contains confident judgements that were right — the seven restored assertions in §4.9, and the framing latch in §4.19. A relationship cannot be observed to be absent from a sample selected on the outcome. What the three cases support is only the narrower rule already stated above: run the comparison first, because it is cheap and it settles what argument does not.

#### §4.19 Two invariants that appeared not to both hold under a viewport-pixel viewBox

- **Kind:** quality-gap
- **Impact:** A user-visible regression (board pinned under the sidebar, units near the origin unclickable) and a named retained-scene assertion appeared mutually unsatisfiable, costing two failed repair attempts before the right predicate was found.
- **Expected:** Framing the board and keeping a retained scene stable across a resize are independent properties.
- **Observed:** They are not, once the SVG viewBox is expressed in viewport pixels.
  - **A — framed, not pinned.** A viewport-pixel viewBox centres nothing, so the board is framed only if the camera is fitted to the *current* viewport. The boot fit is computed against the 960×640 default (`App.fs:361` already applies `FitEditorBoard` at init) and is not adequate at the real viewport — which is why a guard that only re-fits an *untouched initial* camera is a no-op and changes nothing.
  - **B — retained scene survives a resize.** `tests/SIR.Browser.Tests/in-app-docs.spec.js:108` compares the camera fingerprint after a Docs excursion, and that journey resizes to 320px at line 74 and **never restores it** — one `setViewportSize` in the whole file. Measured divergence with a viewport-tracking camera: PanY 184.9 → 242.3, Zoom 1.566 → 0.413.

  On `origin/main` the question does not arise: a board-sized viewBox centred geometrically, so framing never depended on the camera and the camera could be resize-invariant for free.
- **Evidence:** file:src/SIR.Client/MapEditorWorkspace.fs; file:tests/SIR.Browser.Tests/in-app-docs.spec.js; file:src/SIR.Client.Web/App.fs
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. — tactical scene camera and viewBox contract
- **Recurrence:** new
- **Avoidable cost:** one fix attempt that satisfied A and broke B, and a second that was a silent no-op
- **Disposition:** product fix

That intermediate state fixed A — the two click intercepts and a third failure clear, and the cohort goes from 4 failed / 2.2m to 2 failed / 46.3s, the runtime collapse being three 30-second timeouts disappearing rather than anything skipped. The tension is recorded in the commit message and as a `NOTE` in the source. Three options were refused as resolutions: weakening `in-app-docs` (it passes on `main` and asserts a real property), reverting the viewBox (that is the item's finite-viewport contract), and shipping the board pinned (a user-visible regression, and worse than a failing assertion because a user cannot see why a unit will not click).

**Resolved, and the resolution is worth more than the tension.** Both attempts inferred intent from *camera equality* — "is this still a camera we chose?" — and camera equality cannot express *"have we framed against a real viewport yet?"*, which is the actual question. `App.fs:361` already fits at init, so "is the camera untouched?" is permanently false and the narrowed guard was a silent no-op.

An explicit one-shot latch (`HasFittedRealViewport`) expresses the real predicate: fit once, on the first genuine measurement, then never again. **A** holds because the first real measurement is the true viewport; **B** holds because no later resize moves the camera. The ordering that makes this safe was verified rather than assumed — of four `setViewportSize` call sites in the browser suite, **three** run *before* `goto("/")` — `client-feature-loader.spec.js:4`, `desktop-command-surface.spec.js:52`, `visible-workflows.spec.js:395` — so they merely define what the first measurement *is*, which is what the latch wants; **one**, `in-app-docs.spec.js:74`, runs mid-journey, and that is the one the latch protects. None falls between boot and the first measurement.

  An earlier draft of this entry said "two and two". That was wrong, in the one clause claiming verification over assumption — the `visible-workflows` call sits immediately above its own `goto` at line 396. The safety conclusion is *strengthened* by the correction (fewer mid-journey resizes, not more), which is exactly why a miscount there was worth catching: a "verified rather than assumed" clause that itself fails a check is worse than no clause.

Measured: **4 failed / 2.2m → 0 failed / 40.3s** (41 passed, 1 skipped), with `in-app-docs` passing.

**Residual, stated rather than closed:** the app renders at boot against the provisional 960×640 fit before the observer's first delivery, so at least one unframed frame is rendered by construction. Its visibility is **unmeasured** — every browser assertion runs after settle and the review generators screenshot after an explicit settle step, so nothing in the evidence observes the boot frame and forty passing tests do not contradict a flash. Bounding it properly needs its own instrument. A transient flash was accepted over permanent pinning; eliminating it entirely would need framing restored geometrically, through a presentation-only transform outside the authoritative camera.

#### §4.20 A new mechanism needs an assertion for the mechanism, not only for the properties it preserves

- **Kind:** positive-pattern
- **Impact:** A one-shot latch would have shipped with nothing asserting the one-shot part, and two passing assertions would have concealed it.
- **Expected:** Assertions written alongside a new mechanism cover the mechanism.
- **Observed:** The framing latch introduced in §4.19 was covered by two assertions — an untouched camera frames the board, and a resize does not move an operator-moved camera. **Both pass if the latch never latches**: a camera that re-fits on every resize satisfies both simultaneously. The properties the mechanism was introduced to preserve are satisfiable *without* the mechanism, so only a third clause — a second resize does not re-frame an already-framed camera — actually tests it.
- **Evidence:** file:tests/SIR.Client.Tests/TacticalSceneProjectionQualification.fs; file:src/SIR.Client/MapEditorWorkspace.fs
- **Version:** n/a
- **Owner:** this design practice, not a component
- **Recurrence:** new
- **Avoidable cost:** none — caught at the moment of introduction
- **Disposition:** accepted

This is the cycle's own subject reproduced inside a mechanism invented minutes earlier, which is what makes it worth recording: **the reflex has to fire on new work, not only when auditing old work.** The failure message names both cameras, so a regression reports what changed rather than that something did.

#### §4.21 A check that fails accurately and names the cause wrongly

- **Kind:** quality-gap
- **Impact:** Two repairs aimed at the wrong subsystem before either failure was probed. This is a distinct hazard from a check that cannot fail: the check works, and its message misleads.
- **Expected:** A failure message identifies the subsystem at fault.
- **Observed:** Both remaining browser failures were real, reproducible, and named the wrong cause.
  - `visible View commands` reported a **units-cache** construction count. The units cache was correct; the assertion conflated a viewport-culled layer with unculled ones and the count moved because of a **camera** change later in the same spec.
  - `in-app-docs` reported a **click timeout** on a unit. Nothing was wrong with the click or the unit; the **SVG viewBox contract** had changed from board space to viewport-pixel space, so the board was pinned under the sidebar's resize separator.
  In both cases the assertion was pointing at a genuine defect while describing it as something else.
- **Evidence:** file:tests/SIR.Browser.Tests/visible-workflows.spec.js; file:tests/SIR.Browser.Tests/in-app-docs.spec.js; file:src/SIR.Client/MapEditorWorkspace.fs
- **Version:** n/a
- **Owner:** this investigation practice, not a component
- **Recurrence:** new
- **Avoidable cost:** two repairs attempted against the wrongly-named subsystem before probing
- **Disposition:** accepted

The defence is the same one §4.18 names from the other direction: **do not read a failure message as a diagnosis.** Probing the actual value — the composed cache key printed at each workspace, byte-identical with `constructions=1` throughout — killed the units hypothesis in one run after three rounds of inference had not. A team that trusts failure text as causation will repair the wrong thing with full confidence, because the test really is failing and really is telling them something.

#### §4.22 PRE-REGISTERED PREDICTION — written before the attempt, not after it

- **Kind:** quality-gap
- **Impact:** The prediction held on its main claim and was wrong on a subsidiary one, which is why fixing it in advance was worth the cost.
- **Expected:** (the prediction, written before the attempt) The SDD back half for this item is **not** primarily blocked by S.I.R.#271. I predict, before running it, that the 36 unauthored obligations are individually authorable with honest classifications, and that the wall is instead the **observed-run receipt**: `verify` requires an `observedRun` on every `verification`-kind pass, `--from-test-report` accepts a TRX/JUnit, and this repository's .NET client qualification is a **console application** that emits no such report. If that is right, most obligations will validate at `evidence` and then block at `verify` with `verify.unobservedRequiredTest`.
- **Also predicted:** #271 blocks exactly one obligation (EV022), and the host's discriminator — a budget cannot be declared while its intent is *unbound* — means even that one is authorable now as a deferral without the block.
- **Observed:** (completed after the attempt; the prediction above was committed first, at `2b12731`) **Confirmed, with one correction against my own earlier claim.** All 37 obligations were authorable with honest classifications — 29 verification passes each citing the artifact that asserts its requirement, 8 deferrals with all four fields — reaching `evidenceReady` (`supported 29, deferred 8, invalid 0, blocking 0`). `verify` then blocked on exactly the predicted error, `verify.unobservedRequiredTest`, alongside `evidence.performanceIntentUnbound`.

  **The correction:** I had claimed §4.14 blocks *one* of 37 obligations. It does not. Running `--from-test-report` with a real 42-test JUnit produced `changedArtifacts: []` and **zero** `observedRun` receipts, with no `evidence.testReportUntypedObligations` advisory — which would have fired had the obligations been the problem. The unbound-intent error stops the stage writing at all, so the two blockers are **chained**: §4.14 gates the receipt route for the entire work item, and the observed-run gate stands behind it.

  **What is deliberately not concluded:** whether a *matching* report would stamp receipts once §4.14 is fixed. That cannot be tested on this item while the write is blocked, and no unblocked control exists that I am willing to write into. Asserting "this repository has no test-report route" would name a cause I have not isolated — §4.21's hazard exactly. Next action: fix §4.14, retry, and only then file a separate row if receipts still do not stamp.
- **Evidence:** file:work/232-viewport-chunking-semantic-zoom/tasks.yml; command:dotnet tool run fsgg-sdd -- verify --work 232-viewport-chunking-semantic-zoom --text
- **Version:** fs.gg.sdd.cli 1.0.1 (manifest-pinned)
- **Owner:** FS-GG/FS.GG.SDD — evidence/verify observed-run and boundness interaction
- **Recurrence:** new
- **Avoidable cost:** n/a
- **Disposition:** accepted

Why this is written here rather than only reported: §4.22 records three occasions where a confidently-held answer was wrong, and §4.7 records a defect class that survived because nothing named it in the artifact. A prediction that exists only in an exchange can still be reshaped after the result is known. Fixing it in the artifact first is the cheapest available defence against doing that unconsciously, and it costs nothing if the prediction is right.

**If the prediction fails**, that is the more valuable outcome to have recorded: it means the back half was blocked by something I had already ruled out, and the record will show the commitment rather than a tidied narrative.

#### §4.23 CLI help that names the wrong field's vocabulary

- **Kind:** documentation
- **Impact:** The `review wait` event schema is not authorable from the documentation; it had to be recovered by reflecting on the engine assembly.
- **Expected:** The CLI help describes the fields the command accepts.
- **Observed:** Two independent gaps between the prose and the mechanism.
  - `scripts/fsgg-coord --help` describes the event as *"entry/completion/cancellation/timeout"*, which is the **transition** vocabulary. The field named `kind` takes neither of those: reflection on `FS.GG.Coord.ReviewWait` shows `Kind = InitialReview | RepairConfirmation`, with the transition (`Enter | Complete | Cancel | Timeout`) carried in a **separate** field. Drafts using `kind: "entry"` and `kind: "initial"` are both refused as `unknown kind`.
  - `WaitReceipt.ClaimGeneration` is a **`String`**, while `claim --json` emits `markerId` as a **number**. Following the obvious idiom — passing the value the claim receipt gave you — produces a refusal.
- **Evidence:** command:dotnet fsi reflecting FS.GG.Coord.ReviewWait over FS.GG.Coord.Core.dll 0.71.0; issue:EHotwagner/S.I.R.#255
- **Version:** fs.gg.coord.cli 0.71.0
- **Owner:** FS-GG/.github — coordination engine help text and the packed `independent-review` contract; already routed to S.I.R.#255's repair phase
- **Recurrence:** seen again — S.I.R.#255 exists for exactly this contract-versus-engine divergence, and a second lane reported instances of the same class in the same pass
- **Avoidable cost:** several refused wait-event drafts before the vocabulary was recovered from the assembly
- **Disposition:** existing issue

Worth distinguishing by **direction**, because the two kinds cost differently. Prose *stricter* than the mechanism is the dangerous kind: following it produces a state the engine rejects while the reader believes they were being careful. Both of these are the cheaper kind — an immediate, explicit refusal — but the refusal names a field rather than the misdescription, so the reader re-reads correct-looking help and tries another value. That is the cost signal: the gap was not expensive because it was subtle, it was expensive because the help looked authoritative.

## §5 Did not exercise

- `verify` and `ship` lifecycle stages, and every merge-boundary obligation: blocked by S.I.R.#264 (§4.5).
- Post-merge/downstream obligations, package publication, and the done stamp.
- Hidden-tab presentation convergence: not implemented in the delivered work (#234-AC4), so there was nothing to exercise.
- The SDD `verify` and `ship` stages. #232's delivery route is **`sdd-required`** (route decision revision 3), so these are not optional for the item. All 37 obligations were subsequently authored and `evidence` reached `evidenceReady` (§4.22); `verify` remains blocked by §4.14's write block and the observed-run gate behind it, so the item cannot reach a done stamp.

## §6 Doc-versus-behavior contradictions

- `pnext-item` §6: *"`verify-paths`, run right after opening the PR (§5), now catches this while it is still free to fix"* — it catches the *binding* defect, not the closing-keyword defect. See §4.4. Owner: the `pnext-item` skill text.
- The packed `independent-review` contract documents prose HTML-comment markers, while `landable` requires a `fsgg.coord.review-decision/v2` record written through commands that appear in no packed skill. Already filed as S.I.R.#255 and being repaired in another lane; recorded here only as a second sighting.

## §7 Workarounds still in the tree

- None. The one workaround used during investigation — copying the client manifest to the path the harness expected — was replaced by repairing the harness path itself, and all runtime debug instrumentation added during root-cause work was reverted and verified absent before commit.

## §8 Friction and avoidable cost

- Six independent root-cause investigations for what proved to be one defect class (§4.7), including one published root cause retracted after six escalating runtime probes.
- Fourteen `widen` calls; none contested, but each a round trip.
- One full extraction (~1,800 lines across two new modules) plus repairs to six source-scanning gates that the extraction invalidated — work discovered after the extraction rather than planned with it.
- Four review-artifact sets regenerated repeatedly, because every client rebuild invalidates their bundle hash binding and they must be regenerated last.
- Two lifecycle-tool reruns discarded because of the version shadowing in §4.3.

## §9 Skill value and gaps

- **Invoked with evidence:** `pnext-item` (the worker state machine, followed literally), `fs-gg-feedback-report` (12 checkpoints, this report), `fs-gg-playtest` (invoked for the PERF-PLAN evidence-boundary guidance; its production-journey-versus-component distinction is what justified treating the browser gates rather than the pure suite as the acceptance surface for user-facing claims), `fs-gg-sdd-evidence` (the satisfaction rule and the deferral shape), `fs-gg-sdd-lifecycle`/`analyze`.
- **Relevant, not invoked:** `cross-repo-coordination` — the upstream producer capability (FS-GG/FS.GG.Rendering#1256) is already filed by the prior worker, so no new cross-repo request was warranted.
- **Wanted, absent:** a focused skill for *retained-DOM presentation memoisation* — the defect class in §4.7 is not covered by any packed skill, and `fs-gg-playtest`'s guidance stops at the simulation boundary. The performance-first gate names `fs-gg-scene`/`fs-gg-skiaviewer` for the rendering workspace; neither exists in this product, and the fallback ("use the project's playtest guidance") does not reach React memo semantics.
- **Misleading guidance:** `pnext-item` §6 (§4.4).
- **First reporting rule this cycle produced, kept here rather than as a finding:** *when a report cites an artifact that carries a caveat, the caveat travels with the citation.* An earlier draft of §4.1 dropped two qualifications that the harness source and `mutation-evidence.md` both state plainly — the `m8-timeline` inertness boundary, and the precondition under which "7/7" holds — in the direction that made the work look stronger. Both were caught by a fresh reader comparing artifact to report, not by re-reading the report. Understating your own recorded qualifications is easier to do than to notice.
- **Second reporting rule, same family:** *a correction must be carried to every place the report asserts the claim, not only the copy in front of you.* Three claims here were corrected in one entry and left standing in another — §4.14's scope, while §4.17 went on asserting the withdrawn *"blocks exactly one of 37"*; §4.14's closing three lines, which still described 36 unauthored obligations and a removed `performanceBudget` block while §5 already said all 37 were authored; and §4.19's intermediate cohort result, called *"the shipped state"* two lines above the real figure. The mechanism is that the unfavourable copy is the one you are looking at when you make the correction, so it is the one that gets fixed; the other copies are elsewhere and nothing draws you back to them. **Direction, stated as it actually falls: two of the three surviving copies flattered the work, and the third did the opposite** — it reported 2 failed / 46.3s where the shipped result was 0 failed / 40.3s, understating the outcome. I first wrote this rule as a finding claiming *every* one of the three flattered the work; a fresh reader showed that the entry's own instance list refutes its headline. That correction is itself a fourth instance of the mechanism, caught the same way as the other three. The cheap defence is mechanical rather than attentive: **after correcting a claim, grep the artifact for its distinctive phrase before considering the correction done.** All four were findable that way in seconds and none was found that way; each cost a reader's round instead. One text is deliberately exempt: §4.22 still contains a phrase this report withdrew and is **not** corrected, because a pre-registered prediction was true when written, and editing it would convert a measurement of the author at a past moment into a reconstruction. (For the record, §4.22 did receive one edit: its `Expected` and `Observed` field labels carried parentheticals that the report validator's parser rejects, so the parenthetical text was moved after the colon. Not one word of the prediction or its outcome changed. A label-only normalization is disclosed here rather than left for a reader to find in a diff and read as a silent revision.)
- **Third practice, recorded here because it is a habit rather than a defect:** *the instrument that resolved every disputed claim this cycle was testing the mechanism, and in no case was it re-reading.* Three claims of mine were withdrawn or narrowed after checking: the "unsatisfiable pair" framing in §4.5, which a critic's arithmetic reduced to a 7-line margin and a different mechanism; the direction claim in the rule above, refuted by its own instance list; and a finding about this report's own audit schema, which I drafted, verified, argued *against* myself once the evidence-result vocabulary explained the rule, and then cut on the critic's ruling rather than reframing into the weaker version I preferred. I deliberately do **not** claim a direction for those three. A claim that shrinks under checking has by construction become less impressive, so "they all shrank unfavourably" is close to tautological, and counting only the claims that moved ignores the ones that held — §4.9's seven restored assertions held, and the CI green reproduced first time. What the evidence does support is narrower and more useful: **re-reading a report never once caught an error in it; running the mechanism caught every one.** Budget accordingly — a second reading is nearly free and nearly worthless, and one executed probe is worth an hour of them. The corollary is that a claim nobody can test is a claim nobody will correct, which is the real cost of the process testimony in §4.18.

## §10 Outcome markers

- Time to first green local build: minutes (the branch was already buildable; the blocking environment fact — the pinned SDK not on `PATH` — was supplied in the dispatch brief rather than discovered).
- Seven red CI gates at adoption → six root-caused and green locally; one (`rules`) blocked by §4.5 and one (`integrity`) owned by another item.
- Defects fixed: seven product defects, three harness defects, seven restored assertions.
- Gate-inversion evidence: 7/7 gates red-on-inversion with green baselines **when the harness precondition is met** (current client build plus regenerated review artifacts). Without that precondition three gates cannot reach a green baseline and the harness reports 4/7 and refuses to certify the rest — see §4.1.
- CI gates on the delivered head: **7 red at adoption → 5 green**; the remaining two are blocked on items owned elsewhere (S.I.R.#264 for `rules`, §4.14/S.I.R.#271 for `evidence`), and `pr-verdict` is red only in consequence. `cross-runtime`, `browser`, `browser-general-helper` and `documentation` all recovered, and local green reproduced in CI on the first attempt.
- Browser cohort: **4 failed / 2.2m → 0 failed / 40.3s** (41 passed, 1 skipped). The runtime collapse is three 30-second click-intercept timeouts disappearing, not tests being skipped — the skip count is unchanged.
- Ship readiness: **not reached** — two blockers, neither owned by this cycle: S.I.R.#264 for `rules`, and S.I.R.#271 blocking `verify`. The browser reds this cycle owned are all closed.
- Merge: **not reached.**

## §11 Falsifiable improvements

- **Make `delivery-route show` read the item's branch, not the working tree.** Would have prevented §4.2 outright. Owner: FS-GG/.github. Acceptance: with an unclaimed `item/<n>-*` branch carrying `work/<id>/spec.md`, `delivery-route show` reports `sddPackageReady: true`.
- **Have `verify-paths` report the closing-keyword defect independently of how it resolved the item binding.** Would have prevented §4.4. Owner: FS-GG/.github. Acceptance: a PR on an `item/<n>-*` branch whose body says `Refs #<n>` is reported as not closing its item.
- **Refuse a lifecycle stage when the resolved CLI version differs from the manifest pin.** Would have prevented §4.3. Owner: FS-GG/.github. Acceptance: running a shadowing global CLI in a pinned repository exits non-zero naming both versions.
- **Require every ownership-ceiling gate to name the composed surface it measures, not a single file path.** Would have prevented the discovery-after-the-fact in §3. Owner: EHotwagner/S.I.R. Acceptance: adding a new `.fs` file to `SIR.Client.Web.fsproj` before `App.fs` and moving a pinned token into it fails no gate silently.
- **Make the SDD evidence gate unconditional on the presence of the file it audits.** Would have prevented §4.17. Owner: EHotwagner/S.I.R. Acceptance: deleting `work/<id>/evidence.yml` for a work item whose `tasks.yml` declares `requiredEvidence` fails the gate rather than skipping the item.
- **Accept a `performanceBudget` block on a well-formed declaration.** Would have prevented §4.14. Owner: FS-GG/FS.GG.SDD. Acceptance: the five-row bisection in §4.14 collapses to two rows — with and without the block, both accepted — and `evidence.performanceIntentUnbound` clears when the block carries the declared intent id.
- **Make a text-pinned ownership gate fail when the token it names moves, rather than silently stop matching.** Would have prevented §4.16 and the M0 repair earlier in the same cycle. Owner: EHotwagner/S.I.R. Acceptance: renaming a pinned construct fails its gate without anything else having to force the gate to run.
- **Bind the SDD `performanceBudget` to measured values rather than to a producer's `result` field.** Would have prevented a bound verdict meaning "the program reached line 165" (§4.6). Owner: FS-GG/FS.GG.SDD. Acceptance: an evidence declaration citing an artifact whose verdict field is a constant is refused or flagged.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | The workspace was pre-existing and the branch pre-authored; no scaffold or init was run this cycle. |
| onboarding-guidance | partial | The dispatch brief supplied the three environment facts (SDK not on `PATH`, repo name trailing dot, non-persistent mint) without which nothing runs; the repository's own guidance supplied none of them. §4.2 and §4.4 are onboarding defects. |
| skills | exercised | `pnext-item` followed literally including the performance-first gate; `fs-gg-playtest` and `fs-gg-sdd-evidence` consulted for evidence boundaries. Gap recorded in §9. |
| sdd-authoring | exercised | The item's delivery route is **`sdd-required`** (route decision revision 3), so the back half is on its path to Done, not optional. Front-half inherited and verified `implementationReady`; all 37 obligations then authored with per-task classification, reaching `evidenceReady` — 29 supported, 8 deferred, 0 invalid (§4.22). `verify`/`ship` not reached, blocked by §4.14. |
| implementation-apis | exercised | Seven product defects fixed across memo comparators, content tokens, cache keys and an update branch. §4.7, §4.10. |
| dependencies-build | exercised | .NET Release, both Fable targets, and the production publish all built; §4.10 is the one API-compatibility defect. |
| testing | exercised | Full .NET client suite, seven happy-dom/bundle gates, browser smoke, worker round-trip, and a new inversion harness. §4.1, §4.8, §4.9. |
| evidence | exercised | Mutation evidence extended with three new subjects; gate-inversion evidence committed and executable **under its documented precondition** (§4.1); `evidence` brought to `evidenceReady` with 37 authored obligations (§4.22); the measurement harness carries the defect in §4.6. |
| runtime-playtest | exercised | Production Chromium journeys through the review generators and the SVG pipeline harness located the route-commit defect that no source reading had. |
| performance | partial | PERF-PLAN and PERF-SMOKE completed and recorded. PERF-RELEASE measured over 56 production Chromium runs and four of seven journeys breach their **frame** budgets and a fifth breaches only **input-to-paint** (§4.13); the harness verdict is unusable (§4.6); the obligation is an accepted deferral against FS-GG/FS.GG.Rendering#1256. |
| documentation | exercised | `docs/performance-budget.md` carries the declared workload; the item body's false absorption claim was corrected; §4.4 and §6 are documentation defects. |
| packaging-upgrade | partial | Tool-pin shadowing found and worked around (§4.3); no package was published this cycle. |
| worker-git-pr | exercised | Explicit claim, fourteen disjoint widens, PR body repaired for `Closes`, `verify-paths` OK, six commits pushed. §4.2, §4.4. |
