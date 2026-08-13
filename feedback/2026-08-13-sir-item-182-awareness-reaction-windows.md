---
feedbackSchema: 2
date: 2026-08-13
workspace: S.I.R
cycle: item-182-awareness-reaction-windows
lane: sdd
toolVersion: 1.0.1
commit: ef9ac38836bfeedcbd0d04cb27b64c72b6205296
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 5
- **zero-event reason:** n/a
- **package/tool pins:** FS.GG.Game.Core 0.13.0, fsgg-sdd 1.0.1, Fable 5.13.0.
- **confidence limits:** Native/Fable equality, replay v1-v5 compatibility, authoritative tick performance, projection disclosure, production browser delivery, full conformance, docs, and 61 observed SDD obligations are green. Independent review, exact-head hosted CI, and guarded landing remain pending.

## §2 What worked

Keeping awareness and engagement state in Simulation, physical reaction resolution in the inherited Combat authority, and observer filtering in Match made the Web boundary auditable. The browser performs exactly one authenticated request and renders a 515-byte local projection. One shared fixture supplies byte-exact native/Fable evidence and protected semantic inversions.

## §3 What did not

The first 200-unit workload exceeded 50 ms until deterministic per-tick work scheduling reserved capacity within the declared structural caps. Replay v5 initially regressed v4 combat decoding, exposing an incorrect current-version guard. Full conformance also found stale replay baseline, worker fixture, browser copy, and hash-bound review manifests. The deferred RulesExplorer grew beyond 60,000 raw bytes, requiring an explicit 65,536-byte cap while retaining 20,000 gzip and 16,000 brotli limits.

## §4 Findings

#### §4.1 Observer-local projection is a reusable disclosure boundary

- **Kind:** positive-pattern
- **Impact:** Browser presentation cannot reconstruct authoritative world geometry or become a second awareness evaluator.
- **Expected:** Server advances authoritative ticks; Match filters one observer; Web only decodes the result.
- **Observed:** One authenticated `POST /api/awareness/local-projection` returned 515 bytes and rendered acquired contact, resolved engagement, and committed→physical→resolved order. Server and browser tests reject `Units`, `Board`, and `SpatialEvidence`; Web source contains no awareness, SpatialQuery, or Combat evaluator.
- **Evidence:** command:./scripts/verify-awareness-reaction.sh
- **Version:** S.I.R commit ef9ac38836bfeedcbd0d04cb27b64c72b6205296
- **Owner:** EHotwagner/S.I.R. awareness projection
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Replay version boundaries need predecessor fixtures at every decoder gate

- **Kind:** quality-gap
- **Impact:** A new snapshot field can silently make the immediately preceding format undecodable.
- **Expected:** v4 retains combat and defaults awareness/engagement empty; v5 retains all fields.
- **Observed:** A current-version guard incorrectly controlled v4 combat fields. The repair accepts v1-v5, preserves the retained 4,450-byte v3 fixture, round-trips an 11,361-byte v4 package, and round-trips an 11,645-byte v5 awareness package. Version/hash/bounds mutations are red.
- **Evidence:** command:dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release -- --print-replay-evidence
- **Version:** replay format 5
- **Owner:** EHotwagner/S.I.R. replay schema
- **Recurrence:** compatibility-sensitive
- **Avoidable cost:** one decoder diagnosis
- **Disposition:** product fix with retained gates

#### §4.3 Performance caps require an executable scheduling policy

- **Kind:** positive-pattern
- **Impact:** Structural bounds alone did not keep the 200-unit authoritative tick below 50 ms.
- **Expected:** 200 units, 20,000 candidate pairs, representative p95 ≤20 ms, stress p95 ≤50 ms.
- **Observed:** The final production tick measured representative p95 0 ms and stress p95 31 ms with 1,024 LOS/episodes and 58,206 canonical bytes. Candidate, LOS, episode, evidence-byte, and timing inversions all fail.
- **Evidence:** command:dotnet run --project tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj -c Release -- --awareness
- **Version:** S.I.R commit ef9ac38836bfeedcbd0d04cb27b64c72b6205296
- **Owner:** EHotwagner/S.I.R. simulation performance
- **Recurrence:** new
- **Avoidable cost:** three performance iterations
- **Disposition:** accepted bounded scheduler

#### §4.4 Shared bundle evidence and budgets must be rebound together

- **Kind:** friction
- **Impact:** A valid deferred feature invalidates bundle-bound visual evidence and can exceed a historical raw cap.
- **Expected:** Initial and deferred delivery remain bounded, and visual review manifests attest to the current bundle.
- **Observed:** Final delivery is app 1,091,208 raw / 257,409 gzip; deferred RulesExplorer 61,286 raw / 18,140 gzip / 15,666 brotli. The raw cap is explicitly 65,536 with a 61,285 protected inversion; map-editor and M9 manifests were regenerated and qualified.
- **Evidence:** command:node scripts/test-production-delivery-budget.mjs; command:node scripts/test-persistent-workspace-m9-acceptance.mjs
- **Version:** S.I.R commit ef9ac38836bfeedcbd0d04cb27b64c72b6205296
- **Owner:** EHotwagner/S.I.R. production delivery
- **Recurrence:** expected for shared-bundle changes
- **Avoidable cost:** two conformance reruns
- **Disposition:** explicit budget revision and evidence refresh

#### §4.5 Feedback workflow remains absent from projected skills

- **Kind:** capability-gap
- **Impact:** The required schema-v2 workflow cannot be discovered solely from the checkout's projected skills.
- **Expected:** A local feedback-report skill and validator.
- **Observed:** Repository schema-v2 reports and audit examples remain the fallback contract.
- **Evidence:** command:find .agents/skills -path '*/fs-gg-feedback-report/SKILL.md'
- **Version:** current checkout
- **Owner:** FS-GG scaffold skill materialization
- **Recurrence:** item-180 §4.5, item-181 §4.5
- **Avoidable cost:** one contract discovery
- **Disposition:** existing FS-GG/.github#2380

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

The final atomic generator passed focused authority/mutations, normal and forced-fallback rules identity, full conformance, and docs before installing four command-digest receipts. Verify reports 122/122 ready findings and 61/61 observed evidence; ship reports `shipReady` with zero warnings or blockers.

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
| runtime-playtest | exercised | Production Chromium journey passed through exactly one 515-byte authority response. |
| performance | exercised | 200-unit stress, structural caps, bundle and API bytes measured. |
| documentation | exercised | Full documentation verification/experience/browser/accessibility pass. |
| packaging-upgrade | not-exercised | Game.Core 0.13.0 unchanged. |
| worker-git-pr | partial | Claim, isolated worktree, commits complete; PR/CI/review/landing pending. |
