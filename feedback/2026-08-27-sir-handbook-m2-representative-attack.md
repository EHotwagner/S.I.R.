---
feedbackSchema: 2
date: 2026-08-27
workspace: S.I.R-roadmap-m2-v2
cycle: roadmap-sir-combat-quint-handbook-m2-representative-attack
lane: sdd
toolVersion: 1.4.0
commit: ebc0964ff5aae66872a958437baef154a3d6faeb
---

# Development feedback — combat Quint handbook M2 representative attack

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 5
- **zero-event reason:** n/a

This cycle covers issue #361 and PR #362 from base `f0abd0353729712255f21c0d19d40f1ce0798907` through the commit named above. The five validated checkpoints are in `feedback/checkpoints/roadmap-sir-combat-quint-handbook-m2-representative-attack.jsonl`. Evidence covers the composed Q4 gate, SDD clarification, strict docs bootstrap, independent-review correction, immutable receipts, PR creation, and the repository-baseline invalidation check. Exact-head review, final hosted CI, merge, and board completion were pending at draft time.

The existing product came from the `fable-game` provider, contract 1.1.0, in `FS.GG.Workspace.Template` 0.8.0 (`fs-gg-fable-game` at `fef78f5b...`), as recorded by `config/scaffold-provenance.json` and `.fsgg/scaffold-provenance.json`; no scaffold parameters changed in M2. Repository tools pin Fable 5.13.0, fsdocs-tool 22.1.0, and `FS.GG.SDD.Cli` 1.0.1 in `.config/dotnet-tools.json`. The lifecycle commands in this cycle resolved the separately installed global `fsgg-sdd` 1.4.0, so the report does not conflate the repository pin with the invoked binary. The scaffold manifest inventories the generated paths and installed driver/game skills; M2 used only the subset named in §9.

## §2 What worked

The landed Q4 authority and gate made one representative attack teachable without inventing a second model. Explicit clarification decisions kept ordinary Q4 arithmetic, signed-width edge semantics, and the later exhaustive correspondence milestone distinct. A single M2 qualifier now produces narrow receipts and an aggregate receipt only after every claimed gate succeeds.

## §3 What did not

The bounded docs route inherited SDK resolver variables that selected `/usr/share/dotnet` and failed before rendering. The first evidence draft also bound claims for docs, links, and runtime replay to a focused receipt that did not execute those subjects; independent review caught the mismatch. The required feedback invalidation command remains red on an unchanged default-branch comparison because of seventeen historical audit exceptions.

## §4 Findings

#### §4.1 The landed Q4 gate is a reusable executable teaching source

- **Kind:** positive-pattern
- **Impact:** A handbook author can extract the authority, run pinned Quint witnesses and mutations, and compare representative traces with runtime behavior through one established command.
- **Expected:** M2 teaching should be derived from the canonical literate source and should reuse existing semantic qualification rather than duplicate it.
- **Observed:** The qualification reported authority digest `f121c201a6f77d0cfc4c86fe72455e8d821b3d941bf012ab0e3482db103e43e7`, Quint 0.32.0, seven witnesses, eight observed-red mutations, and sixteen sampled traces with 144 runtime-compared states.
- **Evidence:** command:./scripts/qualify-quint-q4-sir-combat.sh; file:readiness/361-handbook-m2/sir-combat-q4.junit.xml; file:docs/rules/sir-combat.md
- **Version:** S.I.R. commit `ebc0964f`; Quint 0.32.0.
- **Owner:** EHotwagner/S.I.R. Quint Q4 authority and qualification
- **Recurrence:** extends the canonical Q4 adoption pattern in `feedback/2026-08-27-sir-item-352-quint-q4-adoption.md`; new as a handbook authoring input.
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Clarification decisions bounded the arithmetic and correspondence claims before implementation

- **Kind:** positive-pattern
- **Impact:** The handbook explains ordinary `25 × 1.0 × 0.8 = 20` arithmetic honestly while retaining the separate signed-int32 wrap boundary and avoiding M5's exhaustive runtime-correspondence claim.
- **Expected:** Ambiguous arithmetic and milestone ownership should become explicit decisions before checklist and plan.
- **Observed:** Three tagged decisions resolved all ambiguities; analyze reported 45 ready findings with zero blockers or stale sources.
- **Evidence:** file:work/361-handbook-m2/clarifications.md; command:fsgg-sdd analyze --work 361-handbook-m2 --json
- **Version:** fsgg-sdd 1.4.0 at S.I.R. commit `ebc0964f`.
- **Owner:** FS-GG/FS.GG.SDD clarification authoring contract
- **Recurrence:** the general value recurs from `feedback/2026-08-27-sir-handbook-m1-linked-skeleton.md` §4.2; these arithmetic and milestone boundaries are new.
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.3 The bounded documentation build still depends on an undisclosed SDK-host envelope

- **Kind:** friction
- **Impact:** A documentation-only worker can complete locked restore and Release build yet still fail before rendering because inherited resolver variables select a host without the pinned SDK.
- **Expected:** The bounded docs route either establishes its pinned host or states the exact environment variables that must be cleared or overridden.
- **Observed:** `./scripts/build-docs.sh --prepare-site-only` selected `/usr/share/dotnet`, where SDK 10.0.302 was unavailable. Clearing `DOTNET_HOST_PATH` and `DOTNET_ROOT_X64`, then using locked restore and Release build, rendered successfully.
- **Evidence:** file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m2-representative-attack.jsonl; file:work/361-handbook-m2/qualify-handbook-m2.sh; file:readiness/361-handbook-m2/docs-build.junit.xml
- **Version:** S.I.R. commit `ebc0964f`; fsdocs-tool 22.1.0; pinned SDK 10.0.302.
- **Owner:** EHotwagner/S.I.R. documentation build bootstrap and onboarding guidance
- **Recurrence:** direct recurrence of `feedback/2026-08-27-sir-handbook-m0-authority-inventory.md` and `feedback/2026-08-27-sir-handbook-m1-linked-skeleton.md` §4.1; related to item 277's host-root analysis.
- **Avoidable cost:** one failed docs-build attempt and SDK-routing investigation.
- **Disposition:** existing issue; duplicate observation, no M2 scope expansion

#### §4.4 SDD accepted an aggregate claim set whose original observed receipt covered only the focused handbook audit

- **Kind:** quality-gap
- **Impact:** Evidence could appear fully observed while strict docs rendering, link policy, and full runtime replay had no receipt bound to those claims.
- **Expected:** Every observed claim points to a receipt that actually executes its subject, and aggregate evidence is emitted only after its component gates pass.
- **Observed:** The checkpoint stream records that the evidence-draft review rejected the focused receipt as support for the broader docs, links, and runtime claim set. The corrected qualifier writes dedicated docs, link, focused mutation, and full Q4/runtime JUnits, then writes a four-case aggregate; the corrected declarations bind to that aggregate and name the qualifier as their source. The review transcript and pre-repair evidence file were not preserved as durable locators, so the exact rejected declaration count is not claimed.
- **Evidence:** file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m2-representative-attack.jsonl; file:work/361-handbook-m2/qualify-handbook-m2.sh; file:readiness/361-handbook-m2/qualification.junit.xml; file:work/361-handbook-m2/evidence.yml; command:fsgg-sdd verify --work 361-handbook-m2 --json; issue:FS-GG/FS.GG.SDD#839
- **Version:** corrected in S.I.R. commit `eb1090be2873c9e606474834b18f6c1dc8feb7a9`; lifecycle sealed at `ebc0964f`.
- **Owner:** FS.GG SDD evidence authoring and EHotwagner/S.I.R. handbook qualification
- **Recurrence:** receipt over-attribution recurs from `feedback/2026-08-12-sir-item-194-executable-rules-corpus.md` §4.1, `feedback/item-185-in-application-docs-2.md`, and `feedback/2026-08-27-sir-item-352-quint-q4-adoption-review-correction.md` §4.1; producer issue FS-GG/FS.GG.SDD#839 remains open.
- **Avoidable cost:** one evidence rewrite, one full qualification rerun, and a replacement exact-head review.
- **Disposition:** accepted

#### §4.5 Feedback invalidation is still red on an unchanged default branch

- **Kind:** defect
- **Impact:** The mandated check cannot distinguish an M2 invalidation from repository-baseline audit debt.
- **Expected:** Comparing `origin/main` to itself exits successfully and establishes a clean baseline.
- **Observed:** Both `origin/main..HEAD` and `origin/main..origin/main` exit 1 with the same seventeen `overbroad or mismatched exception` findings.
- **Evidence:** command:dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- check-invalidation --base origin/main --head origin/main --root .; issue:EHotwagner/S.I.R.#258; issue:FS-GG/.github#2856; issue:FS-GG/.github#2852
- **Version:** feedback tool bundled with this cycle at S.I.R. commit `ebc0964f`.
- **Owner:** FS.GG feedback invalidation semantics and historical S.I.R. audit maintenance
- **Recurrence:** duplicate of `feedback/2026-08-23-sir-item-255-review-contract-divergence-2.md` §4.6 and `feedback/2026-08-27-sir-handbook-m0-authority-inventory.md` §4.3. S.I.R.#258 and FS-GG/.github#2856 are closed after rerouting; FS-GG/.github#2852 is the open distribution-authority successor.
- **Avoidable cost:** one product-head attempt and one baseline attribution check.
- **Disposition:** existing issue

## §5 Did not exercise

M3's full first-time walkthrough, M4 arithmetic pitfall catalog, M5 exhaustive runtime-correspondence chapter, M6 capstone, M7 review, browser gameplay, performance qualification, and package publication/upgrades were outside M2. Merge and board completion remained pending at draft time.

## §6 Doc-versus-behavior contradictions

The bounded documentation command does not describe its locked restore, Release build, and SDK resolver prerequisites. The first evidence draft also described broader success than its focused receipt observed; this was corrected before PR review.

## §7 Workarounds still in the tree

`work/361-handbook-m2/qualify-handbook-m2.sh` clears inherited SDK resolver variables around the bounded docs build. This is an explicit, scoped qualification envelope, not a product runtime change. No generated site output is committed.

## §8 Friction and avoidable cost

One docs-build failure required SDK-routing diagnosis. Independent review required one receipt-attribution repair and a full qualification rerun. The baseline invalidation defect required one control comparison to avoid blaming M2.

## §9 Skill value and gaps

Invoked skills were `work-roadmap` for issue sequencing and ledger policy, the SDD lifecycle plus charter/specify/clarify/checklist/plan/tasks/analyze/evidence/verify/ship references for durable artifacts, and `fs-gg-feedback-report` for five phase checkpoints, cold critique, and schema-v2 validation. Their evidence is the issue/PR route, `work/361-handbook-m2/`, `readiness/361-handbook-m2/`, and the checkpoint stream. The Quint language/modeling skills were not invoked because M2 consumed the already-landed authoritative model rather than authoring a new one; `sir-author-rule`, `sir-check-rule-coherence`, gameplay, playtest, performance, package, and cross-repo skills were irrelevant to this documentation-only milestone. No missing skill blocked delivery. The remaining wanted capability is preventive: SDD evidence authoring should require named case-to-claim coverage before one real receipt can satisfy heterogeneous claims; FS-GG/FS.GG.SDD#839 owns that gap.

## §10 Outcome markers

- First meaningful test: focused M2 audit passed six exact excerpts and 28 checks, including damage 18 under the negative mutation and restored green at 20.
- First rendered state: strict fsdocs emitted the representative handbook page.
- Full semantic gate: seven witnesses, eight mutations, and 144 sampled runtime-compared states passed.
- First green verification: 14/14 evidence and test obligations observed; zero missing, stale, synthetic, or invalid evidence.
- Ship readiness: `shipReady` with zero blockers.
- PR: #362 opened against `main`; routed hosted CI is green. The evidence-draft review covered `eb1090be`; final exact-head acceptance, merge, and board completion remain pending.

## §11 Falsifiable improvements

- Preserve §4.1 by keeping handbook examples mechanically tied to the authority extraction and pinned Q4 gate. Acceptance: changing the armor-retention factor from 8,000 to 7,000 produces observed damage 18, and restoring it returns 20 without editing a second semantic model.
- Preserve §4.2 by requiring tagged clarification decisions for arithmetic representation, overflow boundaries, and cross-milestone claim ownership. Acceptance: analyze reports zero unresolved ambiguities before implementation.
- For §4.3, provide or document a bounded clean-worktree docs entry point that performs locked restore, Release build, and pinned host selection. Acceptance: one command renders a changed Markdown page in a fresh worktree despite ambient resolver variables.
- No new action for duplicate §4.5; close existing issues only when the unchanged-branch command exits zero while still rejecting a genuinely stale touched citation.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing S.I.R. product; no scaffold generated. |
| onboarding-guidance | exercised | AGENTS, source design, roadmap, and first-build route were applied. |
| skills | exercised | Work-roadmap, SDD lifecycle/stages, and feedback-report guidance shaped the cycle. |
| sdd-authoring | exercised | Charter through ship completed with seven requirements, three clarification decisions, and fourteen tasks. |
| implementation-apis | not-exercised | No product or public runtime API changed. |
| dependencies-build | exercised | Locked restore, Release build, SDK environment correction, and strict fsdocs rendering completed. |
| testing | exercised | Focused excerpt/mutation audit, structural links, and full Q4/runtime qualification passed. |
| evidence | exercised | Dedicated receipts compose into fourteen observed obligations; baseline invalidation defect disclosed. |
| runtime-playtest | partial | Existing headless runtime trace correspondence was reused; no user-facing play journey claimed. |
| performance | not-exercised | No performance claim or measurement changed. |
| documentation | exercised | Representative attack handbook content rendered and passed link/vocabulary policy. |
| packaging-upgrade | not-exercised | No package or lock change. |
| worker-git-pr | partial | Isolated branch and PR #362 created; independent exact-head review, final CI, merge, and board completion pending. |
