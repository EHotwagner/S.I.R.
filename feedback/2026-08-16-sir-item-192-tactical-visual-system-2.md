---
feedbackSchema: 2
date: 2026-08-16
workspace: S.I.R-192-tactical-visual-system
cycle: 192-tactical-visual-system
lane: sdd
toolVersion: 1.0.1
commit: 1586fd48ce0b0a6ce9c64cea26dc9b65705e8ac8
---

# Development feedback: tactical visual system repair round 1

## §1 Provenance and confidence

This addendum covers independent-review repair round 1 for issue 192. The reviewed candidate was `835de7d`; product repair commits are `9fe6b12` and `fd5bd9b`, and refreshed SDD evidence closes at the frontmatter commit. The existing cycle checkpoint remains `feedback/checkpoints/192-tactical-visual-system.jsonl`. The report relies on the tracked eight-result focused receipt and production review manifest; hosted replacement CI remains pending.

## §2 What worked

The critic's exact production DOM and protected-stylesheet mutations found gaps that happy-path projection tests could not. Repairing the owning subjects produced one declared-and-painted layer order, independent causal kind/lifecycle attributes, newest-event retention, and a stylesheet-bound review gate. User-visible 100/200-unit Samples routes now provide deterministic production workloads rather than handwritten density prototypes.

## §3 What did not

The initial 200-unit file-import/reset capture stalled Chromium and was stopped. A first production sample used `SIR-MAP 2` with `size 40 40`; legacy migration doubled it to 80×80 and current validation rejected it with a generic 40-cell diagnostic. The final samples use 20×20 legacy coordinates, migrate to 40×40, and retain exactly 100/200 units. Hosted CI also exposed the unrebound rules-corpus source identity, App ownership ceiling, and stale bundle-bound review consumers before repair.

## §4 Findings

#### §4.1 Production-shaped visual review subjects close semantic and performance escapes

- **Kind:** positive-pattern
- **Impact:** Review now detects actual paint-order, lifecycle, stylesheet, density, route, effect, input, and frame regressions at the production boundary.
- **Expected:** Interactive rendering acceptance should mutate and measure the production subjects that own the claim.
- **Observed:** The focused receipt records eight passes; the manifest records exact 100/200-unit production Samples workloads with planned routes, overlays, nonzero effects, DOM nodes, input-to-paint, frame interval, and heap proxies; isolated stylesheet and workload mutations fail closed.
- **Evidence:** file:work/192-tactical-visual-system/test-results/tactical-visual.trx; file:docs/assets/tactical-visual-system-review/manifest.json; file:scripts/test-tactical-visual-review-mutations.sh
- **Version:** fsgg-sdd 1.0.1; Fable 5.13.0; commit 1586fd48ce0b0a6ce9c64cea26dc9b65705e8ac8
- **Owner:** EHotwagner/S.I.R. tactical visual qualification and review surfaces
- **Recurrence:** first seen feedback/2026-08-16-sir-item-192-tactical-visual-system.md §4.1; stronger production-bound evidence
- **Avoidable cost:** one stopped headless capture and one rejected legacy-map fixture
- **Disposition:** accepted

## §5 Did not exercise

Package upgrades were not exercised. The replacement hosted aggregate and merge boundary remain pending.

## §6 Doc-versus-behavior contradictions

None observed.

## §7 Workarounds still in the tree

The production density samples intentionally use 20×20 `SIR-MAP 2` coordinates because that version migrates by a factor of two. This is version semantics, not a bypass; native qualification proves the migrated runtime contains exactly 100/200 units.

## §8 Friction and avoidable cost

One 200-unit import/reset capture was stopped after its bounded timeout. One legacy-map fixture iteration was rejected before the migration factor was identified. No second local broad aggregate was run; focused qualification and metadata-only SDD refresh were used.

## §9 Skill value and gaps

`pnext-item` and `intra-repo-parallel-work` preserved the claim and widened exact repair paths. The SDD lifecycle re-stamped eight focused results across 39 obligations without a rebuild. `fs-gg-playtest` kept browser claims on production routes, and `fs-gg-feedback-report` requires this immutable addendum instead of rewriting the earlier cycle report.

## §10 Outcome markers

- First focused repair green: Release Client qualification after typed renderer extraction and lifecycle separation.
- First production repair green: exact DOM order and 120 ms reduced-motion Playwright journey.
- Production density green: visible Samples routes at exactly 100/200 units.
- Verification: 39/39 evidence and test obligations observed; zero synthetic, deferred, or missing.
- Ship readiness: `shipReady` at the frontmatter commit.
- Merge: pending replacement hosted CI and exact-SHA critic confirmation.

## §11 Falsifiable improvements

Promote the review pattern from §4.1: for a visual acceptance claim, require at least one production semantic subject, one protected visual subject, and one representative/stress route whose owning mutation fails the gate. Acceptance: metadata-only order checks, self-hashed synthetic images, or query-only frame timings cannot satisfy the template without a production DOM/workload subject.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | partial | Existing isolated worktree; no new scaffold. |
| onboarding-guidance | exercised | AGENTS and structured route governed Project 6 and repair scope. |
| skills | exercised | pnext, parallel-work, SDD, playtest, and feedback skills applied. |
| sdd-authoring | exercised | Evidence, verify, ship, and refresh returned current/ready. |
| implementation-apis | exercised | Projection lifecycle, scene adapter, renderer, samples, and CSS changed. |
| dependencies-build | exercised | Focused Release, Fable/Vite, and production Chromium paths passed. |
| testing | exercised | Native, smoke, browser, legacy consumers, verifier, and mutations passed. |
| evidence | exercised | Eight-result TRX backs 39 observed obligations. |
| runtime-playtest | exercised | Maintained and 100/200-unit production Samples routes rendered in Chromium. |
| performance | exercised | DOM, input-to-paint, frame interval, heap, effect, and node budgets recorded. |
| documentation | exercised | Bundle-bound legacy and tactical review packages refreshed. |
| packaging-upgrade | not-exercised | No package upgrade occurred. |
| worker-git-pr | partial | Claim, commits, PR, and review repair exercised; hosted acceptance/merge pending. |
