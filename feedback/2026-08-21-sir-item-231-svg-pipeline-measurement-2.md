---
feedbackSchema: 2
date: 2026-08-21
workspace: S.I.R
cycle: item-231-svg-pipeline-measurement
lane: sdd
toolVersion: 1.1.0
commit: 110065d29302ff28406ce283a50218781408e3ce
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** implementation-test-evidence, verify-ship-pr
- **material events:** 8
- **zero-event reason:** not applicable

This recovery resumed issue #231 and PR #238 after blocker #220 landed. The stable checkpoint is `feedback/checkpoints/item-231-svg-pipeline-measurement.jsonl`. SDD CLI 1.1.0, Node v26.7.0, and Google Chrome for Testing 151.0.7922.34 were exercised. The measured candidate is `946c7b3e533f2408c4d092f4c2260916ed970f51`; matrix evidence commit `31a2757600c453321b581fc2ad6e972ebccf94fa`, SDD evidence commit `ade4a6417d339ff774d72a12460a3b0979d95a7e`, and route-repair commit `110065d29302ff28406ce283a50218781408e3ce` are descendants that do not change the measured runtime subject. The authority binds the Release client manifest, server assembly, fixture definition, 56 ordered raw traces, and raw summary. Confidence is limited to the production journeys and Chromium trace categories in the matrix. Worker-transfer, projection-allocation, source-isolated Elmish/React attribution, and GPU counters remain unavailable and unresolved.

## §2 What worked

Focused mutation gates exposed stale evidence, invalid fixture capacity, duplicate unit placement, uncontrolled axis changes, absent raw bytes, changed raw bytes, and coordinated receipt reseals before authority was accepted. The expensive production matrix ran only after implementation and focused gates stabilized. The completed 8×7 matrix retained 56 content-addressed gzip traces, and the focused validator decompressed and hashed every one. SDD verify reported 25 observed evidence obligations and 25 observed test obligations with zero invalid entries; ship reported `shipReady`.

## §3 What did not

The first measurement design injected a continuous `requestAnimationFrame` sampler into the renderer it measured, so an idle trace attributed sampler activity to product main-thread work. The prior compact receipt and authority agreed with each other but did not prove the ignored raw trace archive existed or matched. The first expanded global-unit fixture reused occupied cells, and a later 80×80 composite import announced success while the production scene retained its default four units. The first supposedly controlled final matrix then compared declarations rather than the production-visible working set: its visible-density pair rendered 40/40 glyphs and its global-count pair rendered 40/200. Review exposed that false green; a 300-unit diagnostic subsequently imported, rendered, and simulated successfully, disproving the inferred fixed 256-unit protocol ceiling. The first hosted `performance` route classified the change correctly and completed its integrity and evidence work, but skipped documentation because the new route was absent from the web/docs preparer conditions; the same policy also gave performance changes the generic 300-second target instead of the designed 240-second target.

## §4 Findings

#### §4.1 An injected frame sampler contaminated the renderer measurement

- **Kind:** defect
- **Impact:** Renderer maintainers could have optimized a measurement observer instead of the product because idle sampler work appeared as main-thread script cost.
- **Expected:** Frame health and input-to-paint evidence observe production work without adding a continuous renderer workload inside the measured window.
- **Observed:** Previous PR head `1f6de7c0b81b5bf11cb3c4022435f645948aa758` installed a continuous RAF loop with `addInitScript`. The repaired candidate derives timing from clock-sync-bounded Chromium trace events, and the focused source gate rejects reintroduction of the sampler.
- **Evidence:** file:scripts/measure-svg-pipeline.mjs; file:scripts/lib/svg-pipeline-measurement.mjs; command:node scripts/test-svg-pipeline-measurement.mjs
- **Version:** measurement schema v1; defective PR head 1f6de7c0b81b5bf11cb3c4022435f645948aa758; repaired candidate 946c7b3e533f2408c4d092f4c2260916ed970f51
- **Owner:** EHotwagner/S.I.R. SVG pipeline measurement harness
- **Recurrence:** new
- **Avoidable cost:** unquantified bounded idle-smoke iterations; exact count not retained
- **Disposition:** product fix

#### §4.2 Compact authority without retained raw bytes was not independently auditable

- **Kind:** quality-gap
- **Impact:** Reviewers could verify mutually consistent derived digests while the underlying ignored raw archive was not clean-checkout durable or hash-complete.
- **Expected:** Exact-candidate evidence remains available after a clean checkout and fails closed when any retained trace is absent or changed.
- **Observed:** The repair committed 56 content-addressed gzip traces plus an exact fixture/journey manifest. The focused validator reads, decompresses, hashes, and parses every retained trace and rejects missing or changed bytes.
- **Evidence:** file:work/231-svg-pipeline-measurement/raw-trace-manifest.json; file:work/231-svg-pipeline-measurement/production-chromium-authority.json; command:node scripts/test-svg-pipeline-measurement.mjs
- **Version:** raw-trace manifest v1 and measurement authority v1; evidence commit 31a2757600c453321b581fc2ad6e972ebccf94fa
- **Owner:** EHotwagner/S.I.R. SVG pipeline evidence contract
- **Recurrence:** first explicitly disclosed in `feedback/2026-08-21-sir-item-231-svg-pipeline-measurement.md §1`; the 2026-08-20 authority retained trace identities without clean-checkout raw bytes
- **Avoidable cost:** one complete 56-route evidence regeneration
- **Disposition:** product fix

#### §4.3 Declared one-factor fixtures did not prove a production-observed control

- **Kind:** defect
- **Impact:** A report could claim visible-density and global-count isolation while production Chromium saw no visible-density change and exposed every additional global unit in the viewport.
- **Expected:** Every axis remains a declared one-factor pair, but visible density is the count of production unit-glyph bounds intersecting the SVG viewport after one identical camera sequence; the visible pair changes that count and the global-count pair holds it.
- **Observed:** Review replayed the retained candidate and found visible density 40/40 and global count 40/200 despite declared targets of 40/20 and 40/40. The repair centers the intended visible cluster, places global-only units from the map's far edge, applies the same fit plus 15 center-anchored zoom inputs, and records both viewport-intersecting and projected glyph counts. The exact 56-run matrix observed baseline 40/40, visible-density 20/40, and global-count 40/200 before every journey; both former escapes are rejected by focused mutations.
- **Evidence:** file:scripts/svg-pipeline-fixtures.v1.json; file:scripts/test-svg-pipeline-measurement.mjs; file:work/231-svg-pipeline-measurement/production-chromium-summary.json; file:feedback/checkpoints/item-231-svg-pipeline-measurement.jsonl
- **Version:** fixture schema v1; candidate 946c7b3e533f2408c4d092f4c2260916ed970f51
- **Owner:** EHotwagner/S.I.R. SVG pipeline fixtures and production runner
- **Recurrence:** new; prior reports described declared control but did not replay production-observed structural counters
- **Avoidable cost:** one complete 56-route false-green matrix plus a critic repair round
- **Disposition:** product fix

#### §4.4 The new performance route skipped documentation producers and its intended budget

- **Kind:** orchestration
- **Impact:** An SVG-only performance change reached the hosted boundary without running documentation or its prepared web/docs inputs, and the route received a looser target than its documented 240-second boundary.
- **Expected:** The `performance` class owns documentation and evidence, prepares both the web and documentation inputs they consume, and passes only at or below 240 seconds with at least 60 seconds of headroom.
- **Observed:** Hosted run 32454476879 selected `performance` and completed integrity/evidence in 174199 ms, but skipped both preparers and documentation. The repaired route keeps broad policy files cross-cutting, includes `performance` in both preparer conditions, applies the 240000/60000 predicate to the class, and ships focused positive and mutation coverage for each boundary.
- **Evidence:** review:https://github.com/EHotwagner/S.I.R./pull/238#issuecomment-5366102135; file:feedback/checkpoints/item-231-svg-pipeline-measurement.jsonl; command:node scripts/test-ci-route.mjs; command:./scripts/test-ci-route-mutations.sh
- **Version:** defective PR head 6596dc64718f3020b1586a23b64e588c5dc69e75; repaired route commit 110065d29302ff28406ce283a50218781408e3ce
- **Owner:** EHotwagner/S.I.R. path-aware PR workflow
- **Recurrence:** new
- **Avoidable cost:** one hosted repair run and one successor-review round
- **Disposition:** product fix

## §5 Did not exercise

No scaffold creation, package publication, dependency upgrade, gameplay rule change, live GPU tooling, or compatibility migration was exercised. A hosted run exercised the defective route, but exact-head hosted confirmation of the repaired route, renewed critic confirmation, host acceptance, merge, and post-merge done remained pending at this report boundary.

## §6 Doc-versus-behavior contradictions

The earlier report inferred a fixed 256-unit projection ceiling and treated declared one-factor equality as production-observed control. Both contradicted production behavior. The performance contract and §4.3 now require production glyph observations and make no protocol-size claim.

## §7 Workarounds still in the tree

None. The retained gzip files are required evidence, not a temporary bypass. Unavailable trace stages remain explicitly unresolved rather than estimated.

## §8 Friction and avoidable cost

Multiple bounded idle smokes isolated observer overhead. One partial matrix stopped after 21 routes when the initial 300-unit generator reused occupied cells; a second stopped after 48 when the 80×80 import announcement did not correspond to a changed scene. The repaired exact matrix completed in about fifteen and a half minutes. The first hosted performance-route run consumed 174199 ms but exposed missing preparer ownership and required one successor-review repair round. No broad aggregate CI was run locally; exact-head hosted confirmation of the repaired route remains pending.

## §9 Skill value and gaps

`work-board-best`, `pnext-item`, intra-repository coordination, the performance-first contract, the SDD lifecycle skills, and `fs-gg-feedback-report` were exercised. Performance-first guidance correctly reserved the complete matrix for a frozen candidate, while focused production smokes found subject defects earlier. The independent-review contract preserved the original critic and its round rather than replacing a changes-required decision with a new reviewer.

## §10 Outcome markers

The final focused validator passed 25 gates, including both former production-observation escapes, all six declared-axis inversions, and raw-byte absence/change mutations. The exact production matrix passed 56/56 routes from 2026-08-21T06:00:20Z through 06:15:46Z. The retained archive contains exactly 56 traces. SDD verify and ship are refreshed after the final evidence bindings. Hosted run 32454476879 classified the candidate as `performance` and finished in 174199 ms but failed its missing-producer bindings; exact-head confirmation of the repaired route, critic confirmation, acceptance, merge, and done remain pending.

## §11 Falsifiable improvements

- For §4.1, keep the source inversion that fails if `addInitScript`, the RAF sampler, or its global interval array returns. Acceptance is a nonzero focused-test exit on reintroduction.
- For §4.2, require exact manifest cross-product equality and byte validation from a clean checkout. Acceptance is a nonzero focused-test exit when one gzip file is missing, changed, renamed away from its content digest, or unparsable.
- For §4.3, keep one-factor mutations for all six axes, wait for a changed production scene revision, and reject summaries unless the visible pair changes viewport-intersecting glyph count while the global pair holds it. Acceptance is a nonzero run for either exact former structural mutant.
- For §4.4, keep broad route-policy files cross-cutting and require the performance class to prepare web/docs, run documentation/evidence, and satisfy the 240000/60000 predicate. Acceptance is a nonzero focused or mutation run when any preparer condition, conservative fallback, or performance headroom clause is removed.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing S.I.R. scaffold retained. |
| onboarding-guidance | partial | Current route, claim, isolated worktree, and lease were verified; no fresh scaffold onboarding occurred. |
| skills | exercised | Board, item, performance-first, SDD, feedback, and independent-review contracts drove recovery. |
| sdd-authoring | exercised | Spec, clarification, checklist, plan, tasks, evidence, verify, and ship were refreshed to the repaired contract. |
| implementation-apis | partial | Production UI, import, projection, and trace interfaces were exercised; gameplay APIs were unchanged. |
| dependencies-build | exercised | Locked Node dependencies and the Release client/server build completed once. |
| testing | exercised | 25 focused gates and 56 production Chromium journeys passed. |
| evidence | exercised | 56 raw traces, manifest, summary, receipt, authority, and SDD bindings were validated. |
| runtime-playtest | exercised | Production controls drove idle, playback, pan, zoom, selection, modality, and overlay journeys. |
| performance | exercised | Eight fixtures by seven journeys measured the production SVG path. |
| documentation | exercised | Performance budget and complete lifecycle artifacts were updated. |
| packaging-upgrade | not-exercised | No package version or publication changed. |
| worker-git-pr | exercised | Fresh identity, explicit claim, rebase, heartbeat, hosted route diagnostics, and successor-review recovery were exercised. |
