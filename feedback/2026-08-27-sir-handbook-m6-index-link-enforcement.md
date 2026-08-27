---
feedbackSchema: 2
date: 2026-08-27
workspace: S.I.R-roadmap-m6
cycle: roadmap-sir-combat-quint-handbook-m6-index-link-enforcement
lane: sdd
toolVersion: 1.0.1
commit: 99d1106c654bb6190114de4f4f5ce54d16665b96
---

# Development feedback — combat Quint handbook M6 index and link enforcement

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 4
- **zero-event reason:** n/a

This cycle covers issue #375 on isolated branch `item/375-handbook-m6` from base `52b9b1f8f8ba1272e4a07691ac495db403af8387` through the commit above. Four immutable checkpoints in `feedback/checkpoints/roadmap-sir-combat-quint-handbook-m6-index-link-enforcement.jsonl` cover first build, lifecycle authoring, implementation/test evidence, and verify/ship preparation. Hosted PR CI, merge, exact-main validation, issue/project completion, and Pages were pending at draft time.

The existing scaffold is `FS.GG.Workspace.Template` 0.8.0 with provider `fable-game`; the repository pins Fable 5.13.0, fsdocs-tool 22.1.0, `FS.GG.SDD.Cli` 1.0.1, and Quint 0.32.0. M6 changes handbook documentation, its schema-v2 vocabulary manifest, and documentation qualification; it does not change combat semantics, production F#, packages, or visual/runtime performance. The audit derives declaration identity from the literate model but does not make the index a second semantic authority.

## §2 What worked

The roadmap's exact M6 exits translated into seven FR/AC pairs and a focused structural gate. A dependency-free Markdown block/inline AST reconciles 188 canonical definitions, five aliases, 74 top-level Quint declarations, sixteen stable rules, fifty chapters, three reading paths, and all internal fragments. Four isolated mutations each observe a named red and then recheck untouched input green. Composing the audit into `scripts/build-docs.sh` means the same gate runs before every rendered site projection.

The SDD lifecycle failed closed twice during authoring: specify named its colon-delimited input grammar, and analyze identified the exact unedited plan placeholders. Both corrections occurred before implementation. The final work model records twenty-two completed tasks and twenty-two observed, non-synthetic evidence obligations with `shipReady` status.

## §3 What did not

The first strict docs command inherited `DOTNET_HOST_PATH=/usr/share/dotnet/dotnet` and selected a root without the pinned SDK 10.0.302, despite the repository-local build succeeding. Clearing inherited resolver variables restored the pinned path. This is the same bounded documentation-environment friction recorded by M0–M5, not a new defect.

The first AST implementation over-constrained navigational links to canonical definition targets and treated aliases/inline code as ordinary controlled prose. Its initial positive run correctly failed. The policy was narrowed to the roadmap contract: any valid link is linked, aliases use explicit `canonical-index-only` policy, and inline code is a manifest-declared structural exemption. Eligible prose remains fail-closed.

The mandatory commit-aware invalidation command remains red on seventeen historical overbroad/mismatched audit exceptions. The same command on unchanged main reports the inherited set; M6 does not touch those cited paths or exceptions.

## §4 Findings

#### §4.1 AST reconciliation turns the handbook index into a falsifiable navigation contract

- **Kind:** positive-pattern
- **Impact:** Learners have one complete canonical target per controlled term, while maintainers get exact drift detection across vocabulary, model declarations, stable rules, chapters, fragments, aliases, and eligible prose.
- **Expected:** Every controlled term has a substantive index entry and all inventories reconcile; missing fragments, duplicate anchors, absent entries, and unlinked eligible prose are independently detectable before rendering.
- **Observed:** Qualification passed 188 definitions, five aliases, 74 declarations, sixteen rules, fifty chapters, and four detector-specific observed-red/restored-green mutations. Strict fsdocs and the full Quint/runtime regression passed afterward.
- **Evidence:** file:docs/sir-combat-quint-handbook.md; file:docs/sir-combat-quint-vocabulary.json; file:work/375-handbook-m6/audit-handbook-structure.mjs; file:readiness/375-handbook-m6/structure-audit.junit.xml; command:bash work/375-handbook-m6/qualify-handbook-m6.sh
- **Version:** S.I.R. commit `99d1106`; vocabulary schema 2; Quint 0.32.0.
- **Owner:** EHotwagner/S.I.R. handbook documentation qualification
- **Recurrence:** extends `feedback/2026-08-27-sir-handbook-m1-linked-skeleton.md` §4.3 and §11 and closes their M6 enforcement deferral.
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Strict docs still depends on clearing inherited .NET resolver variables

- **Kind:** friction
- **Impact:** In an agent environment carrying the measured resolver variables, a roadmap worker can see a strict-docs failure that incorrectly resembles a missing SDK even after locked restore and a successful Release build.
- **Expected:** The bounded strict-docs route either establishes a compatible pinned host or states its required resolver-variable envelope.
- **Observed:** The first `./scripts/build-docs.sh --prepare-site-only` selected `/usr/share/dotnet` and could not find SDK 10.0.302; clearing `DOTNET_HOST_PATH`, `DOTNET_ROOT_X64`, and `DOTNET_ROOT` made the same build pass.
- **Evidence:** file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m6-index-link-enforcement.jsonl; command:./scripts/build-docs.sh --prepare-site-only; command:unset DOTNET_HOST_PATH DOTNET_ROOT_X64 DOTNET_ROOT && ./scripts/build-docs.sh --prepare-site-only
- **Version:** S.I.R. base `52b9b1f`; .NET SDK pin 10.0.302; fsdocs-tool 22.1.0.
- **Owner:** EHotwagner/S.I.R. — `scripts/build-docs.sh` documentation bootstrap and agent-environment boundary
- **Recurrence:** seen again after `feedback/2026-08-27-sir-handbook-m2-representative-attack.md` §4.3; related closed issues EHotwagner/S.I.R.#256 and #277 do not own this exact docs route; no matching open or closed issue found.
- **Avoidable cost:** one failed docs build and one environment diagnosis.
- **Disposition:** accepted

## §5 Did not exercise

Combat semantic changes, production F# changes, general implementation equivalence, interactive gameplay journeys, package publication/upgrades, M6V SVG mechanics/theory visuals and animation/shader/accessibility/fallback/render/performance gates, and M7 final editorial/publication review were outside M6. No pnext performance gate ran because M6 has no typed performance intent.

## §6 Doc-versus-behavior contradictions

No combat semantic contradiction was found. The original M0 manifest claimed complete planned declaration coverage, while reconciliation found three current literate declarations absent from that inventory (`UINT32_RANGE`, `wrapInt32`, and `damageRoundingPreservesInt32Wrap`). M6 repaired the structured manifest and index instead of weakening declaration reconciliation.

## §7 Workarounds still in the tree

`work/375-handbook-m6/qualify-handbook-m6.sh` clears inherited .NET resolver variables before restore/build/docs. Removal requires a workspace-level docs command that reliably selects the pinned SDK under agent environments. Structural mutations remain in-memory only; generated site output remains ignored. The dependency-free AST parser is intentional product tooling, not a temporary parser shim.

## §8 Friction and avoidable cost

One docs build failed before the inherited resolver workaround was applied. Specify and analyze each required one bounded correction pass. The first AST policy required one focused correction before positive green. The historical feedback invalidation failure required one attribution check. No combat code, model semantics, package lock, or generated site output was rewritten.

## §9 Skill value and gaps

`work-roadmap` enforced fresh-worktree sequencing, canonical issue/project wiring, the roadmap ledger, feedback gates, and merge-boundary proof. `fs-gg-feedback-report` required four immutable checkpoints, schema-v2 synthesis, fresh-context critique, and exact audit binding. The SDD lifecycle carried charter through ship and caught authoring gaps before implementation. No Quint modeling or rule-authoring skill was needed because M6 consumes the existing model without changing it. Visual/game/performance skills remain reserved for M6V, whose complete scope stays pending.

## §10 Outcome markers

- First build: locked restore and Release build passed; first strict docs attempt hit the inherited resolver issue.
- First meaningful focused test: not independently timed; the first AST run failed on over-broad occurrence policy and the corrected run passed.
- Structural result: 188 definitions, five aliases, 74 declarations, sixteen rules, fifty chapters, three reading paths, four mutations.
- Runtime regression: Quint 0.32.0; seven witnesses, eight existing mutations; sampled runtime sixteen traces/144 states at seed 352, maximum eight steps.
- Lifecycle: seven requirements, three decisions, twenty-two tasks, twenty-two observed non-synthetic obligations, `shipReady`.
- Delivery: exact-head review, PR/CI, merge, board Done, and post-merge Pages pending at report draft time.

## §11 Falsifiable improvements

- Preserve §4.1 by running `node work/375-handbook-m6/audit-handbook-structure.mjs` before every docs projection. Acceptance: it reports exactly 188 definitions, five aliases, 74 declarations, sixteen rules, fifty chapters, and four observed-red/restored-green controls until an authoritative input intentionally changes with the manifest/index.
- Close §4.2 at the documentation environment boundary. Acceptance: a fresh worker can run `./scripts/build-docs.sh --prepare-site-only` with the ambient agent environment and resolve SDK 10.0.302 without a milestone-local `unset`, while a deliberate wrong-root mutation remains diagnostic.
- Keep aliases explicit rather than inferred. Acceptance: every schema-v2 alias has one canonical term/anchor, `canonical-index-only` policy, and a matching alias marker in the canonical definition; duplicates or absent targets fail.
- Keep M6V separate. Acceptance: no visual is added without authoritative derivation, accessibility labels/descriptions, reduced-motion/static/print/non-WebGL fallbacks, visual regression/render inspection, and performance qualification.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing product; no scaffold parameter changed. |
| onboarding-guidance | partial | AGENTS, roadmap, issue/project, and predecessor artifacts were used; the inherited resolver friction recurred. |
| skills | exercised | Work-roadmap, feedback-report, and the complete SDD lifecycle shaped delivery. |
| sdd-authoring | exercised | Seven requirements, three decisions, twenty-two tasks, and current analyze/verify/ship views. |
| implementation-apis | not-exercised | No production API changed; existing runtime evidence was regression-only. |
| dependencies-build | exercised | Locked restore, zero-warning Release build, strict fsdocs, and pinned Quint passed. |
| testing | exercised | AST/reconciliation, legacy link compatibility, four structural mutations, strict docs, full Q4/runtime, roadmap, and lifecycle gates passed. |
| evidence | exercised | Twenty-two observed, non-synthetic obligations and dedicated JUnit receipts reached `shipReady`. |
| runtime-playtest | not-exercised | Headless correspondence regression is not an interactive journey. |
| performance | not-exercised | M6 has no typed performance intent; visual performance remains M6V. |
| documentation | exercised | Complete definitions, aliases, manifest reconciliation, AST enforcement, rendered docs, and roadmap evidence landed. |
| packaging-upgrade | not-exercised | No package or lock changed. |
| worker-git-pr | partial | Isolated branch, issue/project, evidence commits, and ship readiness exist; hosted PR delivery was pending at draft time. |
