---
feedbackSchema: 2
date: 2026-08-15
workspace: S.I.R
cycle: item-184-elaborate-tactical-sample
lane: sdd
toolVersion: 1.0.1
commit: 31aa2743eb520fe0d313907ec7d39c6d12601bea
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** scaffold-onboarding, lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 6
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-184-elaborate-tactical-sample.jsonl` (six validated events)
- **boundary:** issue #184 from claimed SDD-required delivery through independent-review repair-round-2 implementation commit `31aa2743eb520fe0d313907ec7d39c6d12601bea`; confirmation and merge were pending.
- **scaffold:** FS.GG.Workspace.Template 0.8.0, fable-game provider contract 1.1.0, Fable 5.13.0, and FS.GG.SDD 1.0.1.
- **confidence limits:** timings are host PERF-SMOKE evidence, not live-compositor frame-rate claims. Earlier compatibility-preserving source state and stale-manifest failure logs were not retained, so those observations are bounded to the checkpoint and final repaired state. The consumer-local exception-ledger implementation is verified here, but producer materialization of equivalent bytes remains pending FS-GG/.github#2659.

## §2 What worked

Typed performance intent flowed from specification to plan and analysis before implementation. One catalog model supported native/Fable canonical parity, seven player-facing packages, deterministic mutations, production browser loading, and explicit cost evidence. The repo-local feedback tool also resolved a repeated skill-discovery gap.

## §3 What did not

The issue required explicit fixture migration but did not decide the narrower compatibility question: whether existing public sample and replay IDs should survive replacement layouts. The user supplied that decision and it was recorded as DEC-007. Late conformance also required regenerating both bundle-bound Chromium review manifests after the final client build. Independent review then exposed two evidence-quality gaps: all 48 obligations reused one generic browser receipt, and migrated SIR-MAP2 dimensions could fail import while leaving the default editor map available. A second review round proved that the serialized stress fixture was still projection-equivalent rather than literally 80×80, that path/LOS/combat/scene costs were proxies rather than production counters, and that the feedback invalidation tool declared but never consumed its exception ledger.

## §4 Findings

#### §4.1 Typed performance intent made catalog cost falsifiable before release

- **Kind:** positive-pattern
- **Impact:** Interactive work retained an executable 140-run representative workload and an exact 20 ms p95 / 50 ms p99 budget instead of relying on an after-the-fact performance assertion.
- **Expected:** Producer-owned performance intent is projected before implementation and exact-candidate evidence remains machine-readable.
- **Observed:** Analyze is `implementationReady` with 102 ready findings and no stale source; the committed 140-sample receipt claims the typed budget passed, and repeated native runs stayed below the 20/50 ms limits. Production delivery also passed below its 1,160,000-byte cap.
- **Evidence:** command:dotnet fsgg-sdd analyze --work 184-scenario-catalog --text; command:dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release; command:node scripts/test-production-delivery-budget.mjs
- **Version:** FS.GG.SDD 1.0.1; commit 31aa2743eb520fe0d313907ec7d39c6d12601bea
- **Owner:** FS-GG/FS.GG.SDD performance-intent lifecycle and EHotwagner/S.I.R. catalog qualification
- **Recurrence:** related positive composition in `2026-08-13-sir-item-183-tactical-overlays.md`; this catalog workload is new.
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Public identity compatibility required an explicit migration decision

- **Kind:** friction
- **Impact:** Implementation could not safely decide whether replacing layouts also permitted replacing established sample/replay IDs without user input.
- **Expected:** Acceptance authoring distinguishes ID stability from layout and deterministic-fixture migration.
- **Observed:** DEC-007 retains the public sample/replay IDs, replaces the old layouts, updates invariant-based deterministic expectations, and forbids preserving old layouts solely for compatibility. The current qualification passes that migration contract. No intermediate source snapshot proves the pre-decision implementation state.
- **Evidence:** command:git show e3e40e1b2eedf353913ed0aa98198d218b1a4cbc:work/184-scenario-catalog/clarifications.md; issue:EHotwagner/S.I.R.#184; command:dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release
- **Version:** issue #184; commit e3e40e1b2eedf353913ed0aa98198d218b1a4cbc
- **Owner:** EHotwagner/S.I.R. issue authoring and sample-catalog compatibility policy
- **Recurrence:** new; no earlier report described sample-ID/layout compatibility.
- **Avoidable cost:** precise pre-decision edit cost is not independently reconstructable
- **Disposition:** accepted user decision; authoring improvement proposed in §11

#### §4.3 Bundle-bound review generation remained ordering-sensitive

- **Kind:** orchestration
- **Impact:** Release work required focused regeneration of map-editor and M9 review bindings after the final application bundle changed.
- **Expected:** One release route builds the final client, regenerates every bound review artifact, then runs acceptance.
- **Observed:** The validated checkpoint records two late retries; the repaired candidate's generators and M4–M9 gates pass. The failed intermediate manifests/logs were not retained, so the report does not claim a new independently actionable defect.
- **Evidence:** command:git show e3e40e1b2eedf353913ed0aa98198d218b1a4cbc:feedback/checkpoints/item-184-elaborate-tactical-sample.jsonl; command:npm ci --ignore-scripts && ./fake.sh build -t Test
- **Version:** commit e3e40e1b2eedf353913ed0aa98198d218b1a4cbc
- **Owner:** EHotwagner/S.I.R. production review-evidence orchestration
- **Recurrence:** duplicate of `2026-08-10-live-session-authentication.md` §4.2, `2026-08-10-sir-item-144-play-runner-state.md` §4.2, `2026-08-11-sir-item-142-stable-working-surface.md` §4.3, `2026-08-11-sir-item-156-oversized-imports.md` §4.3, `2026-08-13-sir-item-181-physical-combat-slice.md` §4.4, and `2026-08-13-sir-item-182-awareness-reaction-windows.md` §4.3.
- **Avoidable cost:** two checkpoint-recorded late retries; exact elapsed time unavailable
- **Disposition:** duplicate; current candidate repaired, no duplicate issue filed

#### §4.4 Repo-local feedback tooling closed a repeated capability gap

- **Kind:** positive-pattern
- **Impact:** Checkpoint capture and validation were available from the clean product checkout without locating an external skill copy.
- **Expected:** The feedback contract's repo-local command exists and validates the cycle state.
- **Observed:** The feedback tool is tracked and its checkpoint-state validator passes all six events across four phases.
- **Evidence:** command:git ls-files .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx; command:dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- validate-checkpoint-state --cycle item-184-elaborate-tactical-sample; issue:FS-GG/.github#2380; issue:FS-GG/.github#2548
- **Version:** base commit 3dc50b5; commit 31aa2743eb520fe0d313907ec7d39c6d12601bea
- **Owner:** FS-GG/.github feedback-skill projection
- **Recurrence:** resolution of the gap reported by the authentication, item 149, item 150, item 152, and item 182 reports; producer issues #2380 and #2548 are closed.
- **Avoidable cost:** none in this cycle
- **Disposition:** accepted

#### §4.5 Independent review found generic evidence ownership

- **Kind:** friction
- **Impact:** A green SDD aggregate did not prove which gate owned each obligation.
- **Expected:** Evidence receipts identify the exact native, cross-runtime, browser, or rules gate that exercised an obligation.
- **Observed:** Repair round 1 retained four focused JUnit receipts, mapped conformance and browser obligations to their owning receipts, and refreshed analyze/evidence/verify/ship to 48 observed obligations and `shipReady`.
- **Evidence:** command:git show 31aa2743eb520fe0d313907ec7d39c6d12601bea:work/184-scenario-catalog/evidence.yml; command:scripts/generate-item-184-evidence.sh; command:dotnet fsgg-sdd ship --work 184-scenario-catalog --text; issue:EHotwagner/S.I.R.#210
- **Version:** FS.GG.SDD 1.0.1; implementation commit e3e40e1b2eedf353913ed0aa98198d218b1a4cbc
- **Owner:** FS-GG/FS.GG.SDD evidence ownership
- **Recurrence:** new for this cycle.
- **Avoidable cost:** one independent-review repair round
- **Disposition:** repaired in current candidate

#### §4.6 Literal map dimensions are gated at production admission

- **Kind:** positive-pattern
- **Impact:** A catalog qualification that asserts the admitted map dimensions and unit count prevents a fallback/default map from satisfying the maximum-scale claim.
- **Expected:** Supported logical 80×80 maps enter the ordinary production route, and qualification asserts the loaded map dimensions/unit count rather than accepting a fallback state.
- **Observed:** The retained current-format fixture is admitted as a literal 80×80 map with 200 units before the route/simulation/scene workload runs; the native qualification fails unless those exact dimensions and counts survive production import.
- **Evidence:** command:dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-build --no-restore
- **Version:** implementation evidence commit 31aa2743eb520fe0d313907ec7d39c6d12601bea
- **Owner:** EHotwagner/S.I.R. map import diagnostics
- **Recurrence:** new for this cycle.
- **Avoidable cost:** one independent-review repair round, shared with §4.5
- **Disposition:** accepted product gate

#### §4.7 The maximum-scale and immutable-audit receipts were not authoritative

- **Kind:** defect
- **Impact:** A nominal 80×80 performance claim could pass using a version-2 40×40 serialization that scaled on import, projection-derived cost estimates could replace production path/LOS/combat/scene work, and eight superseded immutable audit citations had no supported disposition route.
- **Expected:** The retained maximum-scale fixture is literally 80×80 in its serialized current format; the performance receipt binds deterministic counters from production route search, simulation LOS/combat, and scene projection; the documented audit-binding exception ledger is consumed fail-closed without rewriting immutable reports.
- **Observed:** Repair round 2 serializes SIR-MAP 4 at 80×80 with 200 units. The exact receipt records 1,602 A* expansions, peak 80 LOS evaluations, peak 90 combat resolutions, and 6,601 production scene nodes, with 5.300 ms p95 and 5.362 ms p99 on the retained run. The bounded local ledger implementation makes the eight exact dispositions green while malformed, stale, prior-digest-mismatched, duplicate, and unused/overbroad mutants fail. The producer defect is filed as FS-GG/.github#2659 and #184 is not blocked because the local repair removes the dependency.
- **Evidence:** command:scripts/generate-item-184-evidence.sh; command:scripts/test-feedback-audit-binding-exceptions.sh; command:dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- check-invalidation --repo-root . --base origin/main --head HEAD; issue:FS-GG/.github#2659
- **Version:** FS.GG.SDD 1.0.1; producer blob 31141f928b90f525d30b1ecae0164da396e791c8; commit 31aa2743eb520fe0d313907ec7d39c6d12601bea
- **Owner:** EHotwagner/S.I.R. scenario performance evidence and FS-GG/FS.GG.Rendering `template/feedback-report/skill/scripts/FeedbackReportTool.fs`
- **Recurrence:** new; dedupe across open/closed FS-GG/.github issues/comments found no existing owner, and FS-GG/.github#2659 now owns the producer gap.
- **Avoidable cost:** one second independent-review repair round, one consumer-local validator repair, and one producer recurrence filing
- **Disposition:** product fix in the current candidate; producer issue FS-GG/.github#2659

## §5 Did not exercise

No new scaffold creation, package upgrade, external hosted deployment, live multiplayer route, or live-compositor frame-rate measurement was exercised.

## §6 Doc-versus-behavior contradictions

None observed. §4.2 was missing identity intent, not a contradiction with the requirement to migrate fixtures explicitly.

## §7 Workarounds still in the tree

The materialized consumer validator at `.agents/skills/fs-gg-feedback-report/scripts/FeedbackReportTool.fs` contains the bounded ledger implementation until FS-GG/.github#2659 lands and rematerializes canonical producer bytes. Removal condition: the producer implementation and its negative matrix are available through the normal skill-delivery channel. Risk: the consumer copy can drift from producer semantics, so the upstream issue remains the owner.

## §8 Friction and avoidable cost

The checkpoint records one migration-decision correction, two late review-binding retries, and two independent-review repair rounds: the first for evidence ownership/map admission and the second for authoritative scale/counters, replay-outcome semantics, and audit-ledger consumption. The report does not reconstruct unretained edit counts or elapsed time. One typed performance mapping regeneration and repeated final-source SDD view refresh were required.

## §9 Skill value and gaps

`pnext-item` supplied claim, performance-first, evidence, review, and guarded-landing contracts. The SDD lifecycle made DEC-007, performance intent, 48 tasks, 48 observed evidence obligations, verification, and ship state explicit. `fs-gg-game-core`, `fs-gg-mapcraft`, `fs-gg-grids`, `fs-gg-line-drawing`, `fs-gg-visibility`, `fs-gg-ballistics`, `fs-gg-effects`, `fs-gg-ai`, and `fs-gg-playtest` constrained deterministic content and player-journey evidence. `fs-gg-feedback-report` captured all four required phases, but its declared audit-binding exception ledger was inert until the bounded consumer repair; FS-GG/.github#2659 owns the canonical correction. No relevant named skill was unavailable.

## §10 Outcome markers

- First build: Release solution build passed during onboarding.
- Lifecycle admission: analyze reached `implementationReady` before implementation.
- Player journey: production Chromium used File → Samples, loaded and advanced all seven packages, and ran Quick Contact from tick 0 through tick 8.
- Exact candidate: seven packages, 76 units, a literal current-format 80×80/200-unit stress fixture, 1,016 exact native/Fable catalog/event/checkpoint bytes, authoritative 1,602/80/90/6,601 path/LOS/combat/scene counters, and Release 5.300 ms p95 / 5.362 ms p99.
- Verification: 96 obligations ready; 48 evidence and 48 test dispositions observed.
- Ship: `shipReady`, zero blocking findings.
- Merge: pending independent review and host acceptance.

## §11 Falsifiable improvements

- For §4.2, add a compatibility field to sample-catalog issue authoring. Acceptance: an issue reusing sample IDs states independently whether ID, layout, replay, and deterministic fixture compatibility are retained or migrated before implementation begins.
- Preserve §4.1's typed projection. Acceptance: removing the spec's performance intent or plan mapping makes analyze fail closed before implementation readiness.
- §4.3 is deliberately deduplicated; route any orchestration change through the existing recurrence chain rather than filing another item from this report.
- For §4.7, implement the versioned exception-ledger contract in `FS-GG/FS.GG.Rendering/template/feedback-report/skill/scripts/FeedbackReportTool.fs`. Acceptance: one exact disposition is green while malformed, stale, prior-digest-mismatched, duplicate, and unused/overbroad entries are red, and a clean materialized product tree runs the same matrix without a local fork (FS-GG/.github#2659).

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | partial | Existing migrated scaffold provenance and pins inspected; no scaffold command ran. |
| onboarding-guidance | exercised | Route, guidance, identity, paths, claim, and performance-first gate exercised. |
| skills | exercised | Coordination, SDD, focused game/map/playtest, and feedback skills applied. |
| sdd-authoring | exercised | Charter through ship completed; analyze and ship ready. |
| implementation-apis | exercised | Scenario package, canonical validation, lookup, cost, and UI catalog APIs implemented. |
| dependencies-build | exercised | Release .NET, Fable, Vite, worker, and server builds exercised. |
| testing | exercised | Native, Fable, browser, smoke, race, mutation, and conformance gates exercised. |
| evidence | exercised | Forty-eight observed obligations, authoritative performance receipt/counters, audit-ledger inversions, and browser JUnit produced. |
| runtime-playtest | exercised | Production Chromium completed the Samples-to-Simulate tick journey. |
| performance | exercised | Baseline, 140-run PERF-SMOKE, counters, node and delivery budgets, and inversion exercised. |
| documentation | exercised | Performance budget, SDD, review manifests, and this report authored. |
| packaging-upgrade | not-exercised | No package version changed. |
| worker-git-pr | partial | Claim, isolated branch, heartbeat, and candidate commits complete; PR/merge pending. |
