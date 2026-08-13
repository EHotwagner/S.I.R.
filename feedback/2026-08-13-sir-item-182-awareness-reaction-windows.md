---
feedbackSchema: 2
date: 2026-08-13
workspace: S.I.R
cycle: item-182-awareness-reaction-windows
lane: sdd
toolVersion: 1.0.1
commit: b9d1d3344b2ccfed81122b652c7ce3b56ffb6a61
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 6
- **zero-event reason:** n/a
- **package/tool pins:** FS.GG.Game.Core 0.13.0, fsgg-sdd 1.0.1, Fable 5.13.0.
- **confidence limits:** Native/Fable equality, replay v1-v6 compatibility, authoritative tick performance, projection disclosure, production browser delivery, full conformance, docs, and 61 observed SDD obligations are green. Independent review, exact-head hosted CI, and guarded landing remain pending.

## §2 What worked

Keeping awareness and engagement state in Simulation, physical reaction resolution in the inherited Combat authority, and observer filtering in Match made the Web boundary auditable. The production browser exercised seven authenticated state-changing requests and rendered a 608-byte local projection. One shared fixture supplies byte-exact native/Fable evidence and protected semantic inversions.

## §3 What did not

The first 200-unit workload exceeded 50 ms until cursor-addressed traversal and allocation reductions brought the full tick below the hard ceiling. Replay v6 required retained v5 coverage for the prior awareness shape. Atomic verification found stale phase oracles, browser source/replay assertions, and hash-bound review manifests. The issue's recovery comment named a preserved dirty checkout that was absent, requiring reconstruction from the immutable remote PR head and review evidence.

## §4 Findings

#### §4.1 Observer-local projection is a reusable disclosure boundary

- **Kind:** positive-pattern
- **Impact:** Browser presentation cannot reconstruct authoritative world geometry or become a second awareness evaluator.
- **Expected:** Server advances authoritative ticks; Match filters one observer; Web only decodes the result.
- **Observed:** Seven authenticated timeline/action requests ended in a 608-byte observer-local projection and rendered acquired contact, resolved engagement, weapon posture, target, and committed→physical→resolved order. Server and browser tests reject `Units`, `Board`, and `SpatialEvidence`; Web source contains no awareness, SpatialQuery, or Combat evaluator.
- **Evidence:** command:./scripts/verify-awareness-reaction.sh
- **Version:** S.I.R implementation commit b9d1d3344b2ccfed81122b652c7ce3b56ffb6a61
- **Owner:** EHotwagner/S.I.R. awareness projection
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Replay version boundaries need predecessor fixtures at every decoder gate

- **Kind:** quality-gap
- **Impact:** A new snapshot field can silently make the immediately preceding format undecodable.
- **Expected:** v5 retains awareness and engagement fields; v6 adds weapon posture and the persisted fairness cursor without invalidating earlier formats.
- **Observed:** Current v6 and retained v5 native/Fable packages round-trip byte-exactly, earlier formats retain their defaults, and version/hash/bounds/posture/cursor mutations are red. No retained pre-fix failure log supports a separate decoder-defect claim.
- **Evidence:** command:dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release -- --print-replay-evidence
- **Version:** replay format 6
- **Owner:** EHotwagner/S.I.R. replay schema
- **Recurrence:** seen again item-181 §4.2; the predecessor-fixture pattern is retained rather than filed again
- **Avoidable cost:** none attributable from retained evidence
- **Disposition:** accepted compatibility observation

#### §4.3 Performance caps require an executable scheduling policy

- **Kind:** positive-pattern
- **Impact:** Structural bounds alone did not keep the 200-unit authoritative tick below 50 ms.
- **Expected:** 200 units, 20,000 candidate pairs, representative p95 ≤20 ms, stress p95 ≤50 ms.
- **Observed:** The final moving 100v100 qualification runs measured p95 34–35 ms and worst 38–40 ms with 20,224 serviced pair slots/LOS checks, all 200 observers covered, 7,900 moves, 6,162 engagement observations, and a persisted cursor of 736. Candidate, fairness, movement, engagement, allocation, evidence-byte, and timing inversions all fail.
- **Evidence:** command:dotnet run --project tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj -c Release -- --awareness
- **Version:** S.I.R implementation commit b9d1d3344b2ccfed81122b652c7ce3b56ffb6a61
- **Owner:** EHotwagner/S.I.R. simulation performance
- **Recurrence:** new
- **Avoidable cost:** three performance iterations
- **Disposition:** accepted bounded scheduler

#### §4.4 Shared bundle evidence and budgets must be rebound together

- **Kind:** friction
- **Impact:** A valid deferred feature invalidates bundle-bound visual evidence and can exceed a historical raw cap.
- **Expected:** Initial and deferred delivery remain bounded, and visual review manifests attest to the current bundle.
- **Observed:** Final delivery is app 1,091,371 raw / 257,460 gzip / 205,920 brotli; deferred RulesExplorer is 62,533 raw / 18,411 gzip / 15,894 brotli. The 65,536/20,000/16,000 caps remain green; map-editor and M9 manifests were regenerated and qualified.
- **Evidence:** command:node scripts/test-production-delivery-budget.mjs; command:node scripts/test-persistent-workspace-m9-acceptance.mjs
- **Version:** S.I.R implementation commit b9d1d3344b2ccfed81122b652c7ce3b56ffb6a61
- **Owner:** EHotwagner/S.I.R. production delivery
- **Recurrence:** duplicate of item-181 §4.4 and earlier item-142/144/148/156 bundle-binding reports
- **Avoidable cost:** two conformance reruns
- **Disposition:** duplicate; current item-specific cap and evidence refresh retained

#### §4.5 Feedback workflow remains absent from projected skills

- **Kind:** capability-gap
- **Impact:** The required schema-v2 workflow cannot be discovered solely from the checkout's projected skills.
- **Expected:** A local feedback-report skill and validator.
- **Observed:** Repository schema-v2 reports and audit examples remain the fallback contract.
- **Evidence:** command:find .agents/skills -path '*/fs-gg-feedback-report/SKILL.md'
- **Version:** current checkout
- **Owner:** FS-GG scaffold skill materialization
- **Recurrence:** duplicate of item-180 §4.5 and item-181 §4.5; existing FS-GG/.github#2380
- **Avoidable cost:** one contract discovery
- **Disposition:** existing FS-GG/.github#2380

#### §4.6 Recovery comments must not rely on an unregistered dirty worktree

- **Kind:** orchestration
- **Impact:** Eleven modified tracked files described as preserved could not be recovered from the named path or Git worktree registry.
- **Expected:** A paused item's durable recovery comment points to retrievable committed or registered state.
- **Observed:** The named checkout was absent; reconstruction used closed PR #202 at immutable head 756efb920fa7ca64d111962b2677445b73009c5a plus its review record, while leaving unrelated checkouts untouched.
- **Evidence:** issue:EHotwagner/S.I.R.#182; issue:EHotwagner/S.I.R.#202; command:git worktree list --porcelain
- **Version:** resumed item-182 cycle
- **Owner:** EHotwagner/S.I.R. board recovery orchestration
- **Recurrence:** new
- **Avoidable cost:** reconstruction of the documented 11-file dirty delta
- **Disposition:** accepted recovery with discrepancy preserved

#### §4.7 Generated SDD guidance references absent authoring examples

- **Kind:** documentation
- **Impact:** Lifecycle reconstruction could not inspect the worked examples and canonical authoring-contract files named by the generated guidance.
- **Expected:** Declared SDD reference paths exist in the generated product workspace.
- **Observed:** Both documented paths were absent; the cycle used the complete skill instructions and recovered typed artifacts instead.
- **Evidence:** command:test -e docs/examples/lifecycle-artifacts/charter.md; command:test -e docs/reference/authoring-contracts.md
- **Version:** fsgg-sdd 1.0.1
- **Owner:** FS.GG.SDD generated-product skill projection
- **Recurrence:** new
- **Avoidable cost:** one reference-discovery retry
- **Disposition:** accepted documentation gap

#### §4.8 Atomic evidence installation prevented partial green receipts

- **Kind:** positive-pattern
- **Impact:** Stale canonical fixtures and bundle bindings were caught without installing a misleading aggregate receipt.
- **Expected:** The evidence generator publishes receipts only after every implementation, conformance, browser, and documentation gate passes.
- **Observed:** The script stages receipts in a temporary directory and copies them only after every gate passes. After phase-oracle, browser assertion, and review-manifest repairs, one exit-0 run installed the six receipts and enabled 61/61 observed SDD obligations. The collector recorded three aggregate reruns, but their failed-run logs were intentionally temporary and are not retained.
- **Evidence:** command:./scripts/generate-item-182-evidence.sh; command:dotnet tool run fsgg-sdd -- ship --work 182-awareness-reaction-windows --json
- **Version:** fsgg-sdd 1.0.1
- **Owner:** EHotwagner/S.I.R. evidence generators
- **Recurrence:** new
- **Avoidable cost:** three aggregate reruns and two focused browser reruns
- **Disposition:** accepted atomic evidence pattern

## §5 Did not exercise

No upstream Game.Core release or compatibility-profile expansion was exercised. Protected landing remains outside this implementation phase.

## §6 Doc-versus-behavior contradictions

None known.

## §7 Workarounds still in the tree

The visible journey uses a canonical server diagnostic scenario rather than live multiplayer transport. Future match controls should reuse the same Simulation/Match authority.

## §8 Friction and avoidable cost

The cycle incurred three performance iterations, one replay decoder repair, worker fixture regeneration, two bundle-manifest regenerations, and several full-gate reruns before atomic receipts were installed.

## §9 Skill value and gaps

Pnext-item, intra-repo parallel work, SDD lifecycle, performance-first, independent-review, merge/release, and Game.Core Fable guidance bounded the work. The feedback-report projection remains absent.

## §10 Outcome markers

The final atomic generator passed focused authority/mutations, normal and forced-fallback rules identity, full conformance, and docs before installing six receipts. Verify reports 122/122 ready findings and 61/61 observed evidence; ship reports `shipReady` with zero warnings or blockers.

## §11 Falsifiable improvements

- Retain exact predecessor replay fixtures; acceptance is byte-exact re-encode and red version/hash/bounds inversions.
- Retain the one-request projection journey and forbidden-field scan; acceptance is red on any authoritative world field or Web evaluator.
- Retain the 65,536/20,000/16,000 deferred budgets and protected raw-cap inversion.
- Materialize the feedback-report skill; acceptance is local clean-checkout discovery and validation.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing repository and isolated worktree used. |
| onboarding-guidance | exercised | Typed route, claim, heartbeat, and delivery contracts used. |
| skills | exercised | SDD, parallel-work, Fable, performance, review, and delivery guidance applied. |
| sdd-authoring | exercised | Charter through ship plus refresh/agents current. |
| implementation-apis | exercised | Public awareness API, Simulation integration, Match projection, Server route, Web renderer shipped. |
| dependencies-build | exercised | Locked package-only native/Fable builds pass. |
| testing | exercised | Semantic, replay, disclosure, delivery, browser, and full conformance gates pass. |
| evidence | exercised | 61/61 observed-run declarations; zero synthetic/self-attested satisfaction. |
| runtime-playtest | exercised | Production Chromium journey passed through seven authority requests and a final 608-byte projection. |
| performance | exercised | 200-unit stress, structural caps, bundle and API bytes measured. |
| documentation | exercised | Full documentation verification/experience/browser/accessibility pass. |
| packaging-upgrade | not-exercised | Game.Core 0.13.0 unchanged. |
| worker-git-pr | partial | Claim, isolated worktree, commits complete; PR/CI/review/landing pending. |
