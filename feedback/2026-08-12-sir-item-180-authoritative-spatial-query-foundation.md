---
feedbackSchema: 2
date: 2026-08-12
workspace: S.I.R
cycle: item-180-authoritative-spatial-query-foundation
lane: sdd
toolVersion: 1.0.1
commit: 5f2377e9c5a14f2826787675a3c3aaf94de62a91
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 6
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-180-authoritative-spatial-query-foundation.jsonl` (6 events).
- **package/tool pins:** FS.GG.Game.Core 0.13.0, fsgg-sdd 1.0.1, Fable 5.13.0, Terser 5.50.0.
- **confidence limits:** Local exact .NET/Fable, production browser, documentation, delivery, cancellation mutation, and SDD evidence are green at the repair commit. Hosted exact-head CI, independent implementation critique, and protected-boundary landing remain pending. Historical failed candidates were not preserved as commits, so the report does not treat their exact byte counts or output as independently verified.

## §2 What worked

The package-only Game.Core adapter boundary kept Cell, Edges, LOS, and A-star consumption portable while S.I.R retained complete footprint, transition, knowledge, and cache semantics. Focused receipts plus SDD observed-run binding made all 50 obligations traceable without self-attestation. The fixed delivery budget caught eager spatial code before review and drove a real lazy production boundary.

## §3 What did not

The first browser projection made spatial diagnostics eager and exceeded the fixed initial-response budget. The first package A-star integration trusted package neighbours too broadly and required S.I.R transition revalidation. Documentation Happy DOM assumed eagerly rendered data after the product moved to a lazy chunk. A parser-tolerated clarification form let verify/ship warn while refresh could not generate a work model; canonical `DEC-###:` grammar and downstream lifecycle replay fixed the projection. Hosted conformance then exposed a cancellation race: the worker advanced past the caller's observed tick before processing its cancel request. The repair accepts lagging ticks only for cancellation while retaining session, map, and plan identity, and yields between bounded batches. These pre-fix details are checkpoint recollections except for the hosted run and executable cancellation mutation.

## §4 Findings

#### §4.1 The fixed production cap caught another startup-growth boundary

- **Kind:** friction
- **Impact:** New player-facing functionality cannot silently inflate the initial production response past its declared boundary.
- **Expected:** Initial application delivery stays below 1,150,000 transferred bytes; the RulesExplorer spatial chunk stays below 60,000 raw, 20,000 gzip, and 16,000 Brotli bytes.
- **Observed:** Current Release qualification measured 1,148,018 initial bytes, 41,228 deferred RulesExplorer bytes, and an 8,582-byte authenticated diagnostic response; the static budget measured the RulesExplorer chunk at 41,228 raw, 13,305 gzip, and 11,444 Brotli bytes.
- **Evidence:** command:node scripts/test-production-delivery-budget.mjs; command:npx playwright test tests/SIR.Browser.Tests/production-delivery.spec.js --config tests/SIR.Browser.Tests/playwright.config.js
- **Version:** S.I.R implementation commit 4a41a5c6c10bf759269e682f54ea499e7602c046
- **Owner:** EHotwagner/S.I.R. client production delivery
- **Recurrence:** seen again feedback/2026-08-12-sir-item-194-executable-rules-corpus.md §4.3
- **Avoidable cost:** two build and route measurement iterations, recorded by the cycle checkpoint but not independently reproduced
- **Disposition:** duplicate; retain the item-194 production-cap gate

#### §4.2 Package A-star remains safe behind complete S.I.R transition revalidation

- **Kind:** positive-pattern
- **Impact:** Package-only lockstep reuse does not weaken multi-cell footprint, diagonal-envelope, modality, occupancy, or bounded-result semantics.
- **Expected:** Game.Core supplies classified primitives while S.I.R owns and revalidates the product semantic envelope in .NET and Fable.
- **Observed:** The final adapter uses package A-star candidate generation and independently validates every transition before accepting a result; exact canonical bytes agree across .NET and Fable and divergence guards fail at byte zero.
- **Evidence:** command:./scripts/verify-spatial-query.sh
- **Version:** FS.GG.Game.Core 0.13.0; S.I.R implementation commit 4a41a5c6c10bf759269e682f54ea499e7602c046
- **Owner:** EHotwagner/S.I.R. spatial authority adapter
- **Recurrence:** new
- **Avoidable cost:** one performance repair iteration, recorded by the cycle checkpoint
- **Disposition:** accepted

#### §4.3 Non-browser DOM smoke observes registration, not deferred browser activation

- **Kind:** quality-gap
- **Impact:** A green documentation smoke alone cannot prove that a dynamic chunk activates or renders its complete browser content.
- **Expected:** Happy DOM verifies the registered Data panel and immediate results without claiming network activation; production browser evidence separately verifies the player-visible spatial route.
- **Observed:** The current smoke exits green with zero eagerly rendered tables and emits Happy DOM teardown `removeChild` exceptions after its assertions. The focused spatial Playwright test proves authoritative selected-unit diagnostics, knowledge policy, package identity, and evaluator identity, but does not assert the seven catalog tables.
- **Evidence:** command:node scripts/smoke-docs.mjs; command:npx playwright test tests/SIR.Browser.Tests/visible-workflows.spec.js --config tests/SIR.Browser.Tests/playwright.config.js --grep "player-visible spatial diagnostics route"
- **Version:** S.I.R implementation commit 4a41a5c6c10bf759269e682f54ea499e7602c046
- **Owner:** EHotwagner/S.I.R. documentation and browser qualification
- **Recurrence:** new
- **Avoidable cost:** one full evidence rerun
- **Disposition:** actionable documentation/browser qualification gap

#### §4.4 Clarification declaration diagnostics recur across SDD work items

- **Kind:** quality-gap
- **Impact:** A valid-looking decision form can leave the work model unavailable without identifying the authoring line to repair.
- **Expected:** Clarification parsing either rejects noncanonical decision declarations directly or reports the exact unresolved IDs and locations.
- **Observed:** Current `DEC-###:` declarations produce clean zero-warning verify/ship and all generated views current. The pre-fix state in this cycle was not retained as a durable receipt, so only the repaired state and recurrence are independently verified.
- **Evidence:** command:dotnet tool run fsgg-sdd refresh --work 180-authoritative-spatial-query-foundation --text; command:dotnet tool run fsgg-sdd verify --work 180-authoritative-spatial-query-foundation --text; command:dotnet tool run fsgg-sdd ship --work 180-authoritative-spatial-query-foundation --text
- **Version:** fsgg-sdd 1.0.1
- **Owner:** FS.GG.SDD clarification grammar and work-model diagnostics
- **Recurrence:** seen again feedback/2026-08-10-sir-item-146-live-reconnect-resilience.md and feedback/2026-08-12-sir-item-187-fable-game-governance-parity.md §4.2; related closed FS-GG.SDD#265
- **Avoidable cost:** one failed refresh and one downstream lifecycle replay, recorded by the cycle checkpoint
- **Disposition:** duplicate; retain existing upstream diagnostic work

#### §4.5 Feedback skill materialization remains an open upstream gap

- **Kind:** capability-gap
- **Impact:** The schema-v2 workflow and validator cannot be discovered solely from the S.I.R checkout.
- **Expected:** A workspace requiring schema-v2 feedback projects the canonical feedback-report skill and helper.
- **Observed:** `.agents/skills` contains no feedback-report skill, so the canonical provider copy from FS.GG.Rendering was required.
- **Evidence:** command:find .agents/skills -path '*/fs-gg-feedback-report/SKILL.md'; issue:FS-GG/.github#2380
- **Version:** S.I.R implementation commit 4a41a5c6c10bf759269e682f54ea499e7602c046
- **Owner:** FS-GG scaffold skill materialization
- **Recurrence:** seen again feedback/2026-08-12-sir-item-194-executable-rules-corpus.md §4.5 and earlier S.I.R feedback; open FS-GG/.github#2380
- **Avoidable cost:** one cross-workspace discovery
- **Disposition:** existing issue FS-GG/.github#2380

#### §4.6 Cancellation correlation must permit the caller's last observed tick to lag

- **Kind:** defect
- **Impact:** Under hosted-runner contention, an active simulator run could complete while its queued cancellation was rejected as stale, failing conformance and making cancellation latency depend on scheduler timing.
- **Expected:** A cancel from the same session, map, and plan stops its target operation even when progress advances after the caller observed the correlation tick.
- **Observed:** Hosted run 31638616764 emitted every operation-110 progress response through completion, then rejected operation 111; the repair keeps strict workspace identity, permits the cancel tick to lag, and yields between bounded batches. The subject mutation turns the real worker smoke red when that yield is removed, and restored code passes 20/20 constrained runs plus the full 8/8 race stress.
- **Evidence:** command:gh run view 31638616764 --repo EHotwagner/S.I.R. --job 94255169628 --log-failed; command:./scripts/test-worker-cancellation-subject-mutation.sh; command:./scripts/test-conformance.sh
- **Version:** S.I.R repair commit 5f2377e9c5a14f2826787675a3c3aaf94de62a91; Node 26.5.0 hosted failure and Node 26.7.0 local verification
- **Owner:** EHotwagner/S.I.R. browser worker cancellation protocol
- **Recurrence:** new
- **Avoidable cost:** two rejected yield candidates and one constrained-load reproduction cycle
- **Disposition:** product fix

## §5 Did not exercise

No scaffold creation or upstream Game.Core package release was exercised. Protected-boundary merge is intentionally outside the implementation commit and remains pending.

## §6 Doc-versus-behavior contradictions

None remain. Documentation names the shared F# authority, package adapter boundary, knowledge projection, fixed delivery budgets, and visible diagnostic route implemented by the candidate.

## §7 Workarounds still in the tree

The Happy DOM smoke accepts zero eagerly rendered rules-data tables because it does not activate the dynamic import. Removal requires a DOM harness that executes production module loading; until then, browser activation must not be inferred from this smoke. Its post-assertion teardown exceptions are also visible and should not be confused with assertion coverage.

## §8 Friction and avoidable cost

The cycle checkpoints record two delivery-size iterations, one package A-star performance/semantic repair, one documentation evidence rerun, and one downstream SDD replay after the work-model grammar mismatch. Pre-fix candidate outputs were not retained, so these costs remain collector observations rather than independently reproduced history.

## §9 Skill value and gaps

The work-board-best, pnext-item, intra-repo parallel-work, complete SDD lifecycle, Game.Core Fable, and F# API documentation skills bounded routing, claims, authored contracts, package use, cross-runtime evidence, and public signatures. The feedback-report projection was absent and required its canonical provider copy, matching open upstream issue FS-GG/.github#2380.

## §10 Outcome markers

The focused spatial verifier passed all nine spatial subject mutations plus exact .NET/Fable and Release-budget gates. The cancellation subject mutation failed closed and restored green; full conformance, its 8/8 race stress, documentation, delivery, and browser receipts pass with no failures/skips. SDD verify reports 50/50 evidence and tests observed, zero self-attested, and ship reports `shipReady` with all generated views current. The exact production route measured 1,148,018 initial, 41,228 deferred RulesExplorer, and 8,582 diagnostic API bytes.

## §11 Falsifiable improvements

- FS.GG.SDD should name malformed clarification decision lines or reject them at clarify; acceptance is a direct location-bearing diagnostic rather than only derived unresolved references. This is recurrence evidence, not a new filing.
- S.I.R documentation qualification should eliminate post-assertion Happy DOM teardown exceptions and add an explicit browser assertion for complete deferred table content if that content is an acceptance obligation.
- FS-GG scaffold materialization should close FS-GG/.github#2380 by making the feedback-report skill discoverable in a clean product checkout.
- Simulator cancellation gates should retain the subject mutation that removes the between-batch yield; acceptance is a red real-worker smoke followed by restored green, not a predicate-only inversion.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing repository and isolated worktree were used. |
| onboarding-guidance | exercised | Typed route, claim, widening, heartbeat, and source-freeze rules were used. |
| skills | exercised | Board, SDD, package/Fable, API docs, and feedback workflows were applied. |
| sdd-authoring | exercised | Charter through ship plus refresh/agents are current; 50 obligations observed. |
| implementation-apis | exercised | Typed Domain identity, Simulation evaluator/cache, Match service, and client projection shipped. |
| dependencies-build | exercised | Locked restore and package-only Game.Core consumption passed in native and Fable builds. |
| testing | exercised | Spatial and cancellation subject mutations, full conformance, 8/8 race stress, docs, and browser gates pass. |
| evidence | exercised | 50/50 exact-digest observed-run declarations pass with zero self-attestation. |
| runtime-playtest | exercised | Production browser unit selection and View → spatial diagnostics route passed. |
| performance | exercised | Exact Release LOS/route/invalidation/100/200-demand and delivery budgets passed. |
| documentation | exercised | API/content/integrity/experience/smoke/accessibility build passed, with disclosed teardown exceptions. |
| packaging-upgrade | not-exercised | Existing FS.GG.Game.Core 0.13.0 was consumed unchanged. |
| worker-git-pr | partial | Minted claim, isolated worktree, disjoint paths, and implementation commit are complete; PR/CI/landing remain pending. |
