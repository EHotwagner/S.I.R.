---
feedbackSchema: 2
date: 2026-08-12
workspace: S.I.R
cycle: item-194-executable-rules-corpus
lane: sdd
toolVersion: 1.0.1
commit: 0f7128985a14ef3470e92d23ee5786236f97fb97
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 7
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-194-executable-rules-corpus.jsonl` (7 events).
- **confidence limits:** Local full conformance, documentation, focused browser, replay-v3, coverage, identity, delivery, and SDD ship gates passed; hosted exact-head CI and exact-head independent review remain pending.

## §2 What worked

The typed SDD lifecycle kept 13 requirements, 44 tasks, and 44 observed evidence obligations connected through a terminal `shipReady` receipt. The shared F# conformance executable and focused browser journey proved that the same registry drives .NET, Fable, generated projections, explanations, and the player-visible Rules explorer.

## §3 What did not

The isolated worktree needed an initial restore and `npm ci`. The first explorer projection lacked a registry-backed desktop command, the first SDD evidence draft reused one browser receipt for obligations that test could not observe, and the first reviewed head underimplemented historical replay, identity, coverage, and the player journey; independent critique correctly rejected each shortcut.

## §4 Findings

#### §4.1 SDD evidence must bind an observable, acceptance-specific receipt

- **Kind:** quality-gap
- **Impact:** The first ship-ready result overstated verification because one Rules-browser JUnit could not observe replay, codec, mutation, performance, or documentation obligations.
- **Expected:** Each verification obligation binds a parsed receipt whose cases can observe the claimed behavior.
- **Observed:** Independent critique rejected both the single-receipt draft and stale hand-authored labels; `scripts/generate-item-194-evidence.sh` now runs each owning command and writes its acceptance-specific JUnit only after that command passes. The repaired evidence partitions 44 obligations across canonical corpus, replay, browser, performance, delivery, and full-conformance JUnits, and verify reports 44 observed with zero self-attested, stale, synthetic, or invalid evidence.
- **Evidence:** artifact:scripts/generate-item-194-evidence.sh; command:./scripts/generate-item-194-evidence.sh; command:dotnet fsgg-sdd verify --work 194-executable-rules-corpus --text
- **Version:** FS.GG.SDD 1.0.1
- **Owner:** S.I.R SDD evidence authoring
- **Recurrence:** new
- **Avoidable cost:** one independent-review repair round and evidence regeneration
- **Disposition:** product fix

#### §4.2 Shared F# conformance catches unsupported Fable semantics early

- **Kind:** positive-pattern
- **Impact:** One authoritative F# corpus can be qualified without a copied JavaScript semantics implementation.
- **Expected:** Unsupported Fable constructs and canonical-byte divergence fail before delivery.
- **Observed:** The Fable compile exposed unsupported APIs during implementation; the repaired full fixture then produced 25,730 identical canonical bytes in .NET and Fable/Node.
- **Evidence:** command:./scripts/verify-rules-corpus.sh
- **Version:** S.I.R current candidate
- **Owner:** S.I.R cross-runtime conformance
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.3 Generated panels need a registry-backed player route

- **Kind:** quality-gap
- **Impact:** A semantically correct explorer was initially unreachable through the visible desktop UI.
- **Expected:** A player can open the generated Rules explorer through an existing command-registry menu.
- **Observed:** Adding command `panel.data` under the visible View menu enabled the focused Playwright journey without forced clicks, direct dispatch, or DOM injection.
- **Evidence:** artifact:src/SIR.Client.Web/CommandRegistry.fs; command:npx playwright test tests/SIR.Browser.Tests/visible-workflows.spec.js --grep "player-visible Rules explorer"
- **Version:** S.I.R current candidate
- **Owner:** S.I.R web command registry
- **Recurrence:** new
- **Avoidable cost:** two failed navigation attempts and one product correction
- **Disposition:** fixed in candidate

#### §4.4 Production delivery protects the fixed response budget

- **Kind:** positive-pattern
- **Impact:** The explorer could not silently bloat the production startup response.
- **Expected:** The player-visible projection remains under the fixed 1,150,000-byte initial-response cap.
- **Observed:** The corpus projection was moved behind the player-opened Rules route, and complete replay validation remains solely in the retained worker; focused production qualification measured 1,149,378 initial response bytes, 622 below the unchanged cap, with a 302-byte deferred support chunk.
- **Evidence:** command:npm run test:production-delivery-evidence
- **Version:** S.I.R current candidate
- **Owner:** S.I.R production delivery gate
- **Recurrence:** new
- **Avoidable cost:** multiple focused reduction and structural-split iterations
- **Disposition:** accepted

#### §4.5 Feedback skill distribution is incomplete in the consumer checkout

- **Kind:** capability-gap
- **Impact:** The required schema-v2 feedback workflow cannot be discovered solely from this repository's projected skills.
- **Expected:** The S.I.R skill projection includes the feedback-report skill and validator.
- **Observed:** The local skill was absent, so the canonical provider copy from FS.GG.Rendering was required.
- **Evidence:** command:find .agents/skills -path */fs-gg-feedback-report/SKILL.md
- **Version:** S.I.R current candidate
- **Owner:** S.I.R agent skill distribution
- **Recurrence:** new
- **Avoidable cost:** one cross-workspace discovery
- **Disposition:** capability follow-up

#### §4.6 Exact-head critique must probe semantic boundaries, not only green receipts

- **Kind:** quality-gap
- **Impact:** The first reviewed head could pass focused receipts while disabling replay v3 in the production Fable build, accepting an opaque archive, emitting a wholly dangling coverage graph, showing a synthetic attack, and omitting evaluator/codec surfaces from implementation identity.
- **Expected:** Independent review reproduces production-target behavior, validates graph topology and every declared archive identity, traces player input to the authoritative event, and mutates every identity surface.
- **Observed:** The repaired candidate removes the Fable downgrade; adds a typed, bounded, content-addressed archive with per-field rejection; emits 45 unique coverage nodes with zero dangling edges; executes `Simulation.runTick` from a visible command and deep-links from the emitted `AttackResolved`; and verifies an 11-source plus package/fingerprint implementation inventory at immutable source commit `0f7128985a14ef3470e92d23ee5786236f97fb97`. After confirmation critique proved that pinned-object hashing alone allowed a changed current `App.fs` to pass, the verifier added byte-exact current-versus-pin correspondence for every declared source, normalizing only three parsed CombatRules identity metadata literals; App and non-metadata CombatRules mutations now fail while the required metadata rebind passes.
- **Evidence:** command:./scripts/verify-rules-corpus.sh; command:SIR_RULES_FORCE_GREP=1 ./scripts/verify-rules-corpus.sh; command:node scripts/test-production-replay-v3.mjs; command:npx playwright test tests/SIR.Browser.Tests/visible-workflows.spec.js --grep "player-visible Rules explorer"
- **Version:** S.I.R implementation pin 0f7128985a14ef3470e92d23ee5786236f97fb97
- **Owner:** S.I.R replay, rules corpus, and browser acceptance
- **Recurrence:** new
- **Avoidable cost:** one exact-head independent-review repair cycle
- **Disposition:** fixed in candidate

## §5 Did not exercise

Broad migration of mechanics outside the first combat vertical slice was intentionally not exercised.

## §6 Doc-versus-behavior contradictions

None remain. The design documents now identify the implemented combat slice and keep the broader corpus migration explicitly provisional.

## §7 Workarounds still in the tree

None. Historical packages are retained explicitly, browser reachability uses the command registry, and no parallel JavaScript or TypeScript gameplay implementation was introduced.

## §8 Friction and avoidable cost

Fresh-worktree bootstrap, the initially unreachable panel, dishonest first evidence binding, critic-discovered semantic/provenance gaps, and the fixed production response cap caused bounded recovery loops. Each resulting product, contract, or qualification correction is retained in the candidate.

## §9 Skill value and gaps

The SDD, parallel-work, pnext, and Game.Core Fable skills bounded lifecycle, claim, package, and evidence decisions. The absent feedback-report projection remains the only skill-distribution gap encountered.

## §10 Outcome markers

Full conformance and full documentation passed; focused Rules explorer, typed production replay-v3, coverage/identity mutation, and production-delivery gates passed. SDD verify reports 44/44 evidence and 44/44 tests observed with no self-attested, stale, synthetic, missing, or invalid evidence; ship reports `shipReady` with no diagnostics.

## §11 Falsifiable improvements

- SDD evidence authoring should flag high-cardinality obligations bound to a low-cardinality receipt unless the receipt names observable cases for those obligations; acceptance is zero unobservable bindings in `verify`.
- Generated player-facing panels should fail qualification when no visible registry-backed route opens them.
- The feedback-report skill and validator should be projected into every product checkout that requires schema-v2 feedback.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing repository and isolated worktree were used. |
| onboarding-guidance | exercised | Typed route, claim, widening, and heartbeat contracts used. |
| skills | exercised | SDD lifecycle, pnext, parallel work, Game.Core Fable, and feedback workflow used. |
| sdd-authoring | exercised | Charter through ship completed; refresh and agent projections are current. |
| implementation-apis | exercised | Typed corpus, evaluator, registry, algorithms, explanations, and replay binding implemented. |
| dependencies-build | exercised | Locked restore, Game.Core surface receipt, Release builds, and Fable compile passed. |
| testing | exercised | Full conformance, mutations, browser, performance, source, replay, and manifest gates passed. |
| evidence | exercised | 44/44 obligations bind an exact parsed JUnit observed-run receipt. |
| runtime-playtest | exercised | View → Rules data, visible execute command, actual `AttackResolved`, and event-derived rule deep link passed without direct dispatch or forced interaction. |
| performance | exercised | 10,000 complete explained attacks remained within the 2,000ms cap. |
| documentation | exercised | Full fsdocs/API/content/integrity/experience/accessibility route passed. |
| packaging-upgrade | not-exercised | Existing published Game.Core 0.13.0 lockstep package was consumed unchanged. |
| worker-git-pr | exercised | Minted claim, isolated worktree, immutable source commit, and protected-boundary handoff used. |
