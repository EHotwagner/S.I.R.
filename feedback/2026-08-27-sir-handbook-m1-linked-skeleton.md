---
feedbackSchema: 2
date: 2026-08-27
workspace: S.I.R-roadmap-m1
cycle: roadmap-sir-combat-quint-handbook-m1-linked-skeleton
lane: sdd
toolVersion: 1.4.0
commit: d62a32dba830d01ad8d5d66336d752371cae143a
---

# Development feedback — combat Quint handbook M1 linked skeleton

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 4
- **zero-event reason:** n/a

This cycle ran on issue #359 and branch `item/359-handbook-m1` from base
`99da2c3d4d014e2aeb7ec525e8623849c793d2c5` through commit
`d62a32dba830d01ad8d5d66336d752371cae143a`. The lifecycle tool reported 1.4.0. The four
checkpoints are in `feedback/checkpoints/roadmap-sir-combat-quint-handbook-m1-linked-skeleton.jsonl`.
Evidence covers the complete SDD lifecycle, the focused structural audit and four mutations, strict
fsdocs rendering, PR #360 creation, and the hosted semantic-anchor defect and repaired `prepare-web` pass. Downstream checks, independent
exact-head review, merge, and board completion were still pending at draft time.

## §2 What worked

The M0 inventory made the M1 skeleton mechanically enumerable. The handbook audit uses one parsed
structural view for the real publication and in-memory mutations, and the lifecycle consumed a JUnit
receipt so every evidence and test obligation was observed.

## §3 What did not

The fresh-worktree `--prepare-site-only` route failed because projects had not been restored. After a
locked restore, the ambient resolver variables selected `/usr/share/dotnet` instead of the repository's
pinned SDK. A command-local pinned host override produced the expected handbook page.

## §4 Findings

#### §4.1 The site-only documentation route is not a fresh-worktree entry point

- **Kind:** friction
- **Impact:** A documentation-only worker encounters multiple avoidable failures before it can render a new page in an isolated worktree.
- **Expected:** The documented site-only route either prepares its prerequisites or names the exact locked restore, Release build, and pinned-host preconditions.
- **Observed:** The first run reported unrestored projects; after restore, reproduction selected a host that could not resolve SDK 10.0.302. Explicit preparation and a command-local host override rendered the handbook.
- **Evidence:** file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m1-linked-skeleton.jsonl; command:./scripts/build-docs.sh --prepare-site-only; command:dotnet restore SIR.slnx --locked-mode
- **Version:** S.I.R. commit `d62a32dba830d01ad8d5d66336d752371cae143a`; fsdocs-tool 22.1.0; pinned SDK 10.0.302.
- **Owner:** EHotwagner/S.I.R. documentation build bootstrap and onboarding guidance
- **Recurrence:** seen again after `feedback/item-185-in-application-docs.md` §3/§11 and `feedback/2026-08-27-sir-handbook-m0-authority-inventory.md` §3/§6; item 277 is related host-root behavior, not this exact route.
- **Avoidable cost:** one failed docs build and one explicit restore retry.
- **Disposition:** accepted

#### §4.2 Tagged clarification decisions turned rejected prose into durable design choices

- **Kind:** positive-pattern
- **Impact:** Manifest location, parser boundary, and exercise layout were fixed before planning, preventing implementation choices from remaining implicit.
- **Expected:** Every blocking AMB identifier is resolved by a uniquely declared decision tag before checklist.
- **Observed:** Three `[AMB:...]` decisions produced zero remaining ambiguities and a green checklist.
- **Evidence:** file:work/359-handbook-m1/clarifications.md; command:fsgg-sdd clarify --work 359-handbook-m1 --text
- **Version:** fsgg-sdd 1.4.0 at commit `d62a32dba830d01ad8d5d66336d752371cae143a`.
- **Owner:** FS-GG/FS.GG.SDD clarify authoring contract
- **Recurrence:** new for this roadmap's three concrete M1 choices; earlier item 181 §4.2 and item 146 document the general clarification grammar.
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.3 A shared structural audit makes link-contract defects cheap to prove

- **Kind:** positive-pattern
- **Impact:** Maintainers can validate 185 definition anchors, fifty chapters, sixteen rule rows, mandatory coverage rows, and controlled prose without maintaining separate mutation fixtures.
- **Expected:** The real handbook passes and missing fragments, duplicate anchors, absent index entries, and unlinked controlled occurrences each fail through their named route.
- **Observed:** One command passed the real handbook and detected all four in-memory mutations.
- **Evidence:** command:node work/359-handbook-m1/audit-handbook-links.mjs; file:work/359-handbook-m1/audit-handbook-links.mjs
- **Version:** S.I.R. commit `d62a32dba830d01ad8d5d66336d752371cae143a`.
- **Owner:** EHotwagner/S.I.R. handbook qualification
- **Recurrence:** extends the JUnit-backed documentation verifier pattern in `feedback/2026-08-27-sir-handbook-m0-authority-inventory.md` §4.1 with structural Markdown and four link-specific controls.
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.4 Explicit semantic anchors were absent from the in-app documentation contract

- **Kind:** defect
- **Impact:** Hosted `prepare-web` rejected the otherwise valid handbook, so the publication could not land even though strict fsdocs rendering passed.
- **Expected:** Local fragment validation recognizes both renderer heading slugs and explicit stable HTML anchors while keeping only real headings in navigation.
- **Observed:** The first hosted run reported `Missing documentation anchor sir-combat-quint-handbook#part-i`; the repaired manifest now carries a separate anchor inventory and the generator plus decoder qualification pass locally.
- **Evidence:** file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m1-linked-skeleton.jsonl; command:node scripts/generate-in-app-docs.mjs artifacts/site; command:node scripts/test-in-app-docs.mjs artifacts/site; file:scripts/generate-in-app-docs.mjs
- **Version:** S.I.R. commit `d62a32dba830d01ad8d5d66336d752371cae143a`.
- **Owner:** EHotwagner/S.I.R. in-app documentation manifest and decoder contract
- **Recurrence:** new; prior feedback search found no explicit-HTML-anchor inventory finding.
- **Avoidable cost:** one failed hosted `prepare-web` run and one parser-contract repair.
- **Disposition:** product fix

## §5 Did not exercise

Runtime gameplay, browser interaction, performance qualification, package publication/upgrades,
exhaustive Quint verification, and M2-M7 handbook substance were outside M1. Hosted downstream checks,
independent review, merge, and board completion were pending at draft time.

## §6 Doc-versus-behavior contradictions

The README presents `./scripts/build-docs.sh` as the complete clean-clone route, while
`--prepare-site-only` is an internal bounded route whose restore/build and host prerequisites are not
described by its usage errors. The cycle used the bounded route to avoid unrelated client work.

## §7 Workarounds still in the tree

None. The SDK resolver override was command-local. Generated site output remains ignored and ephemeral.

## §8 Friction and avoidable cost

One failed bounded render attempt and one explicit restore retry occurred before the correct host
envelope was used. Hosted `prepare-web` failed once on the missing explicit-anchor inventory, then the
manifest, decoder, and qualification contract were repaired. Lifecycle authoring also required replacing
scaffold plan prose with real plan decisions.

## §9 Skill value and gaps

The SDD lifecycle/stage guidance and `fs-gg-feedback-report` were invoked. SDD failed closed on untagged
ambiguity resolution and generic scaffold plan prose, which improved the durable artifacts. The feedback
skill preserved bootstrap friction and the reusable structural-audit pattern. Gameplay, playtest, and
performance skills were not relevant to this documentation-only milestone.

## §10 Outcome markers

- First meaningful test: the focused audit passed 185 definitions, fifty chapters, sixteen rule rows, and four negative controls.
- First rendered state: strict fsdocs emitted `artifacts/site/sir-combat-quint-handbook.html` after explicit bootstrap.
- First green verification: 26/26 obligations ready; 13/13 evidence and test obligations observed.
- Ship readiness: `shipReady`, with no missing, stale, synthetic, or invalid evidence.
- PR: #360 opened against `main`; the first hosted run exposed §4.4 and repaired `prepare-web` passed; downstream checks remain pending.

## §11 Falsifiable improvements

- For §4.1, document or add a clean bounded docs entry point that performs the pinned locked restore and
  required Release build. Acceptance: a fresh isolated worktree with repository-supported prerequisites
  renders one changed Markdown page on the first command without ambient SDK resolver overrides.
- Preserve §4.3 through M6 by integrating the manifest-backed audit into documentation qualification.
  Acceptance: hosted documentation checks fail for each of the four named mutations and pass the restored
  handbook without treating front matter, headings, fenced code, or canonical index rows as prose.
- Preserve the §4.4 fix by keeping explicit anchors separate from navigation headings. Acceptance: a page
  may link to a semantic HTML anchor, the in-app qualification resolves it, and the sidebar remains limited
  to Markdown headings.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | partial | Existing S.I.R. scaffold inspected; no new product scaffold generated. |
| onboarding-guidance | exercised | AGENTS guidance, source design, and roadmap boundary applied. |
| skills | exercised | SDD lifecycle/stage and feedback-report skills drove authoring, evidence, and reporting. |
| sdd-authoring | exercised | Charter through ship completed with six requirements and three clarification decisions. |
| implementation-apis | not-exercised | No runtime or public code API changed. |
| dependencies-build | exercised | Locked restore, Release build, pinned host, and strict fsdocs render exercised. |
| testing | exercised | Positive audit plus four named structural mutations passed. |
| evidence | exercised | 13/13 evidence and test obligations observed; ship ready. |
| runtime-playtest | not-exercised | Documentation-only milestone. |
| performance | not-exercised | No performance claim or measurement changed. |
| documentation | exercised | Handbook and README route rendered under strict fsdocs. |
| packaging-upgrade | not-exercised | No package or pin changed. |
| worker-git-pr | partial | Isolated branch and PR #360 created; hosted defect repaired and `prepare-web` passed; downstream checks, review, and merge pending. |
