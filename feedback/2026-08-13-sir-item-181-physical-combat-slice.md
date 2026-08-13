---
feedbackSchema: 2
date: 2026-08-13
workspace: S.I.R
cycle: item-181-physical-combat-slice
lane: sdd
toolVersion: 1.0.1
commit: d0e44ca2a53942638ccf86c32ccb3c991ee32428
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 7
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-181-physical-combat-slice.jsonl` (7 events).
- **package/tool pins:** FS.GG.Game.Core 0.13.0, fsgg-sdd 1.0.1, Fable 5.13.0.
- **confidence limits:** Repair-round exact .NET/Fable fixtures, replay v4 roundtrip/seek with exact predecessor-v3 compatibility, rules identity, subject mutations, authoritative tick performance, server authority, four-profile browser/replay journey, delivery budgets, full conformance, and documentation are green. The package is pinned to implementation commit 707cfb1 with identity commit d0e44ca. Round-two critic confirmation, hosted exact-head CI, and protected-boundary landing remain pending while this report is authored.

## §2 What worked

The inherited `SpatialQuery` surface supplied the only physical trace and cover evidence, while the expanded executable rules corpus supplied canonical current-source identity. Keeping the evaluator in Simulation and routing the visible drill through Match/Server made the browser boundary auditable: Web performs one authenticated bounded request and only decodes and renders its renderer-neutral result. One canonical fixture stream exercises native and Fable semantics, and subject mutations protect collision, cover, armor, suppression, ordering, identity, and runtime equality.

## §3 What did not

The first browser composition pulled the complete Simulation graph into the initial bundle and exceeded the fixed production budget. Separating the lightweight legacy tick from the authoritative physical tick restored the intended lazy topology. The clarify command accepted a decision punctuation form that the work-model renderer later rejected as six anonymous unknown references. Full conformance also exposed two honest compatibility updates: the expanded UnitState changed pinned simulation checkpoint bytes, and the production bundle change invalidated hash-bound map-editor review boards.

## §4 Findings

#### §4.1 Server authority and renderer-neutral Web projection form a reusable combat boundary

- **Kind:** positive-pattern
- **Impact:** Browser presentation cannot silently become a second combat authority.
- **Expected:** Match/Server own physical evaluation; Web issues one bounded authenticated request and renders the returned facts.
- **Observed:** The focused route submits exactly one `POST /api/combat/physical-drill`, receives status 200, renders all four profiles plus replay evidence, and source/bundle scans find no `Combat.resolve`, physical evaluation, or unique evaluator symbols in Web. The focused response was 5,169 bytes; the published diagnostic response was 8,582 bytes.
- **Evidence:** command:./scripts/verify-physical-combat.sh
- **Version:** S.I.R implementation commit 707cfb1a18df9967e10da5269749e30f7481abc4
- **Owner:** EHotwagner/S.I.R. physical combat boundary
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Fixed delivery budgets forced a structural lazy boundary

- **Kind:** quality-gap
- **Impact:** Adding authority to a shared F# graph can unintentionally make portable simulation code part of startup delivery.
- **Expected:** Initial responses remain at or below 1,150,000 bytes and the Rules explorer deferred chunk remains at or below 60,000 bytes.
- **Observed:** After separating legacy `runTickWithRules` from authoritative `runPhysicalTickWithRules`, the repaired published route measured a 1,122,831-byte initial response set (including the 1,091,206-byte app bundle and initial page/style assets) and a 57,117-byte deferred chunk while retaining the exact-once authority journey.
- **Evidence:** command:node scripts/test-production-delivery-budget.mjs; command:npx playwright test tests/SIR.Browser.Tests/visible-workflows.spec.js --config tests/SIR.Browser.Tests/playwright.config.js --grep "player-visible Rules explorer"
- **Version:** S.I.R implementation commit 707cfb1a18df9967e10da5269749e30f7481abc4
- **Owner:** EHotwagner/S.I.R. production delivery
- **Recurrence:** seen in item-180 and item-194 delivery work
- **Avoidable cost:** one structural refactor and two measurement iterations
- **Disposition:** product fix with retained budget gate

#### §4.3 Clarify and work-model decision grammars disagree on accepted punctuation

- **Kind:** quality-gap
- **Impact:** A lifecycle can report all ambiguities resolved and verify/ship ready while refresh cannot produce the canonical work model.
- **Expected:** Clarify rejects noncanonical decision declarations or work-model diagnostics name each offending source line.
- **Observed:** Six lines shaped `DEC-001 [AMB:AMB-001] ... complete:` passed clarify but produced six `unknownReference` and six `workModelInconsistent` markers only inside generated diagnostics. Normalizing to `DEC-001: [AMB:AMB-001] ... complete.` made clarify, analyze 120/120, refresh, agents, verify 114/114, and ship current with zero blocking diagnostics. Closed upstream [FS.GG.SDD#265](https://github.com/FS-GG/FS.GG.SDD/issues/265) owns convergence of these accepted/authored decision forms.
- **Evidence:** command:dotnet fsgg-sdd clarify --work 181-physical-combat-slice --text; command:dotnet fsgg-sdd refresh --work 181-physical-combat-slice --text
- **Version:** fsgg-sdd 1.0.1
- **Owner:** FS.GG SDD clarification/work-model parser
- **Recurrence:** item-180 §4.4 and earlier S.I.R lifecycle feedback
- **Avoidable cost:** one diagnosis and downstream lifecycle replay
- **Disposition:** duplicate recurrence of FS.GG.SDD#265

#### §4.4 Hash-bound cross-feature review evidence makes shared-bundle changes explicit

- **Kind:** friction
- **Impact:** A valid shared production bundle change cannot pass conformance with visual review artifacts that attest to another bundle.
- **Expected:** Deterministic map-editor review boards bind the current production JavaScript digest.
- **Observed:** Conformance rejected both stale review manifests after physical-combat delivery changed the bundle; `npm run review:map-editor` regenerated seven deterministic SVG/PNG pairs, and `npm run review:persistent-workspace-m9` recaptured the actual 1440×900 Chromium shell. Both focused portability/acceptance qualifications returned green.
- **Evidence:** command:npm run review:map-editor; command:node scripts/test-map-editor-qualification.mjs; command:npm run review:persistent-workspace-m9; command:node scripts/test-persistent-workspace-m9-acceptance.mjs
- **Version:** S.I.R implementation commit 707cfb1a18df9967e10da5269749e30f7481abc4
- **Owner:** EHotwagner/S.I.R. visual review evidence
- **Recurrence:** expected for shared-bundle changes
- **Avoidable cost:** two conformance reruns because the receipts are qualified at different points
- **Disposition:** accepted evidence refresh

#### §4.5 Feedback workflow remains undiscoverable from the projected S.I.R skills

- **Kind:** capability-gap
- **Impact:** A required schema-v2 report and critic audit cannot be executed solely from locally projected skill guidance.
- **Expected:** The product checkout exposes the canonical feedback-report skill and validator.
- **Observed:** `.agents/skills` contains no feedback-report skill; existing schema-v2 reports were used as the discoverable repository contract.
- **Evidence:** command:find .agents/skills -path '*/fs-gg-feedback-report/SKILL.md'
- **Version:** S.I.R at implementation commit 707cfb1a18df9967e10da5269749e30f7481abc4
- **Owner:** FS-GG scaffold skill materialization
- **Recurrence:** item-180 §4.5; open FS-GG/.github#2380
- **Avoidable cost:** one repository-contract discovery
- **Disposition:** existing issue FS-GG/.github#2380

## §5 Did not exercise

No upstream Game.Core release or compatibility-profile expansion was exercised. The protected-boundary merge is intentionally outside the implementation commit and awaits exact-head review and CI.

## §6 Doc-versus-behavior contradictions

None known. Combat, weapons, units, and performance documentation name the shared authority, four profiles, consequence ordering, suppression recovery, visible server route, and measured delivery/performance boundaries implemented by the candidate.

## §7 Workarounds still in the tree

The visible physical drill uses a dedicated authenticated diagnostic endpoint and canonical scenario rather than a complete match command flow. It is renderer-neutral and server-authoritative, but future live-match attack controls should reuse the same Match service instead of treating the drill route as the final multiplayer transport.

## §8 Friction and avoidable cost

The cycle recorded one bundle-topology refactor, two delivery measurement iterations, one rules current-source rebind, one SDD parser diagnosis/replay, one simulation oracle regeneration, and two bundle-bound visual review regenerations.

## §9 Skill value and gaps

Work-board-best, pnext-item, intra-repo parallel work, the complete SDD lifecycle, and Game.Core Fable guidance bounded routing, identity, source ownership, obligations, package-only consumption, and cross-runtime evidence. The absent feedback-report projection remains the only material local skill gap.

## §10 Outcome markers

The repair verifier passes exact .NET/Fable fixtures, replay-v4 combat state roundtrip/checkpoint seek plus byte-exact decoding and re-encoding of the retained 4,450-byte predecessor-v3 package, eight protected combat semantic/identity mutation classes, authoritative one-tick performance (representative p95 16/20 ms and 100-unit/50-attack stress p95 18/50 ms), server auth/bounds tests, source/bundle authority scans, and exact-once four-profile plus replay browser evidence. Normal and forced rules-corpus verification pass with canonical package/source correspondence. SDD evidence, verify, and ship are regenerated only after the executable multi-receipt evidence run.

## §11 Falsifiable improvements

- FS.GG.SDD should reject the noncanonical decision form during clarify or return exact locations during refresh; acceptance is a source-line diagnostic for each malformed declaration.
- S.I.R should retain the exact-once server route and Web authority scans; acceptance is red if physical evaluation enters Web or the real request count differs from one.
- Shared-bundle changes should retain map-editor review digest validation; acceptance is red on a stale manifest and green only after deterministic regeneration.
- The feedback-report skill should be materialized per FS-GG/.github#2380; acceptance is discoverability and validation in a clean S.I.R checkout.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing repository and isolated worktree were used. |
| onboarding-guidance | exercised | Typed scheduling, claim, widening, heartbeat, and delivery routing were used. |
| skills | exercised | Board, SDD, parallel-work, package/Fable, and repository feedback contracts were applied. |
| sdd-authoring | exercised | Charter through ship plus refresh/agents are current; 57 obligations are observed. |
| implementation-apis | exercised | Simulation evaluator, replay state, Match authority, Server endpoint, and renderer-neutral Web projection shipped. |
| dependencies-build | exercised | Locked package-only Game.Core and native/Fable builds pass. |
| testing | exercised | Semantic mutations, exact fixtures, auth/bounds, Match/Server, browser, delivery, and conformance gates were exercised. |
| evidence | exercised | 57/57 observed-run declarations pass; zero synthetic or self-attested satisfaction. |
| runtime-playtest | exercised | Production Rules explorer physical drill passed through exactly one authority request. |
| performance | exercised | Representative, 100-unit/50-attack stress, structural caps, API, initial, and deferred bytes were measured. |
| documentation | exercised | Combat, weapons, units, performance, and hash-bound visual review evidence were updated. |
| packaging-upgrade | not-exercised | Game.Core 0.13.0 was consumed unchanged. |
| worker-git-pr | partial | Identity, claim, isolated worktree, disjoint paths, and implementation/rebind commits are complete; PR/CI/landing remain pending. |
