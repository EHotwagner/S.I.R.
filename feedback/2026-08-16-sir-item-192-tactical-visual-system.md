---
feedbackSchema: 2
date: 2026-08-16
workspace: S.I.R-192-tactical-visual-system
cycle: 192-tactical-visual-system
lane: sdd
toolVersion: 1.0.1
commit: f7eecb4d5d2bc0315d257851bfa318c956b4cde1
---

# Development feedback: tactical visual system

## §1 Provenance and confidence

This cycle implemented issue 192 from the structured-delivery-v2 work item `192-tactical-visual-system`. It used fsgg-sdd 1.0.1 and the repository's pinned Fable 5.13.0 toolchain. The implementation boundary is commits `0c8b1c4` and `98a9436`; lifecycle evidence and its tracked four-result focused TRX close at the frontmatter commit. Three checkpoint events are preserved in `feedback/checkpoints/192-tactical-visual-system.jsonl`.

The complete frozen aggregate stopped on a stale smoke-layer count after all earlier build and Client qualification steps passed. The smoke was repaired and rerun focused; per the lean single-aggregate policy it was not followed by another local aggregate. Hosted CI remains the final full-run authority. The PNG diagnostic was observed before evidence was switched to an SVG artifact; the current tree therefore preserves the checkpoint but not a still-failing authored input.

The commit-aware invalidation check reports 20 historical feedback-audit citations touched by this feature, including earlier tactical-overlay and persistent-workspace reports. This long-lived-source behavior is preserved as a visible failed check rather than suppressed, matching the prior item-186 disposition.

## §2 What worked

Performance-first planning established explicit 100/200-unit node, effect, and Release p95 budgets before rendering edits. One projection-owned token/effect registry then fed all four tactical modalities, while a real server-route Playwright journey checked reduced motion, causal effects, exact order, and frame-callback inspection. The deterministic review generator bound a real Chromium screenshot and 20/100/200 density prototypes to the exact production bundle.

SDD's observed-run receipt made the distinction between declared proof and an actual test run explicit: all 39 obligations reached observed, non-synthetic verification with no deferrals.

## §3 What did not

The first production-browser run exposed an invalid string-valued React SVG style attribute. Replacing it with the typed style-object route fixed both Fable and Chromium.

The only broad aggregate found that `scripts/smoke-client.mjs` still expected eight scene layers after effects intentionally made nine. The repaired smoke now checks both the count and exact semantic order; an intentional order mutation turned it red before restoration.

The evidence command emitted an `undecodableFile` warning for the committed PNG visual-inspection artifact even though the PNG was valid and evidence readiness succeeded. Switching the declaration to the equivalent committed SVG removed the warning.

## §4 Findings

#### §4.1 Performance-first projection and real-route evidence composed effectively

- **Kind:** positive-pattern
- **Impact:** Interactive visual work retained exact disclosure and density while gaining one coherent effect/motion system; the 100/200-unit budgets stayed executable rather than aspirational.
- **Expected:** Rendering changes should preserve authoritative tactical truth and prove representative and stress behavior before review.
- **Observed:** Projection qualification, the reduced-motion production browser journey, the exact-bundle review verifier, and the persistent smoke all passed in the tracked focused receipt.
- **Evidence:** file:work/192-tactical-visual-system/test-results/tactical-visual.trx; file:tests/SIR.Client.Tests/TacticalSceneProjectionQualification.fs; file:tests/SIR.Browser.Tests/visible-workflows.spec.js; file:scripts/test-tactical-visual-review.mjs; file:docs/assets/tactical-visual-system-review/manifest.json
- **Version:** fsgg-sdd 1.0.1; Fable 5.13.0; commit f7eecb4d5d2bc0315d257851bfa318c956b4cde1
- **Owner:** EHotwagner/S.I.R. tactical projection and qualification surfaces
- **Recurrence:** new; no matching prior feedback finding found
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Binary visual evidence produces a misleading decode warning

- **Kind:** defect
- **Impact:** Authors can mistake a valid rendered PNG for malformed evidence or spend time replacing a suitable visual-inspection artifact; this cycle changed 39 artifact locators and reran evidence.
- **Expected:** A visual-inspection obligation should accept a named rendered image without attempting to diagnose its binary bytes as an undecodable text body.
- **Observed:** The contemporaneous checkpoint records that fsgg-sdd 1.0.1 returned `evidenceReady` but also reported the valid production PNG as `undecodableFile` at byte zero. Pointing the declarations at the textual SVG removed the warning; the original PNG declaration/output was not preserved and is not reproducible at this report head.
- **Evidence:** file:feedback/checkpoints/192-tactical-visual-system.jsonl; file:work/192-tactical-visual-system/evidence.yml; issue:FS-GG/FS.GG.SDD#825
- **Version:** reproduced with fsgg-sdd 1.0.1; latest available pin checked was 1.0.1
- **Owner:** FS.GG.SDD evidence artifact diagnostics
- **Recurrence:** seen again issue:FS-GG/FS.GG.SDD#825; merged PR #830 explicitly retained the warning while repairing the existence gate
- **Avoidable cost:** 39 locator edits and one evidence rerun
- **Disposition:** existing issue FS-GG/FS.GG.SDD#825

## §5 Did not exercise

Package upgrade/remediation was not exercised. The local broad aggregate did not progress to its complete browser inventory because it stopped at the repaired smoke contract; focused production Chromium and review routes were exercised instead.

## §6 Doc-versus-behavior contradictions

The evidence skill says SDD does not check that a named visual artifact is an image, while fsgg-sdd 1.0.1 attempted to decode the valid PNG and warned. This contradiction belongs to FS.GG.SDD evidence diagnostics or its documentation.

## §7 Workarounds still in the tree

`work/192-tactical-visual-system/evidence.yml` cites `density-prototypes.svg` instead of the actual Chromium PNG to avoid the binary decode warning. It can return to the production PNG when evidence artifact inspection becomes media-aware or stops text-decoding opaque files.

## §8 Friction and avoidable cost

One aggregate was spent on the stale eight-layer smoke contract, followed by one focused repair/green and one intentional mutation red. The PNG warning caused 39 mechanical artifact-locator edits and one evidence rerun. Initial isolated-worktree setup required one npm install and ordinary targeted restores; these were setup, not product defects.

## §9 Skill value and gaps

`pnext-item` and `intra-repo-parallel-work` kept identity, claim, worktree, touch-set, and handoff boundaries explicit. The SDD lifecycle and per-stage skills prevented source edits before implementation-ready analysis and made every obligation observed. `fs-gg-playtest` correctly routed the user-facing claim to a production browser journey. `fs-gg-feedback-report` preserved the aggregate and PNG friction while fresh. Image generation was not invoked because no bitmap art asset was required; the production token/effect registry and deterministic SVG fixtures were the better fit.

The evidence skill's visual-inspection guidance needs a media-aware diagnostic path for valid binary images.

## §10 Outcome markers

- First focused Release Client green: before production-browser integration.
- First real production render green: Playwright reduced-motion journey, 2.1 seconds.
- Frozen implementation: `0c8b1c4`; focused smoke repair: `98a9436`.
- Verification: 39/39 evidence and 39/39 test obligations observed, zero synthetic/deferred/missing.
- Ship readiness: `shipReady` at the frontmatter commit.
- Merge: pending independent exact-SHA review and hosted CI.

## §11 Falsifiable improvements

For §4.2 and existing issue FS-GG/FS.GG.SDD#825, FS.GG.SDD should classify artifact handling by media intent or treat evidence artifacts as opaque existence-checked files. Acceptance: an evidence-ready visual-inspection obligation naming a valid PNG emits no `undecodableFile` diagnostic, while a genuinely malformed authored YAML/source file still fails closed.

For the smoke friction, S.I.R. now checks the exact layer-order token in addition to the layer count. Acceptance is already met: changing or removing `effects` makes `node scripts/smoke-client.mjs` fail with the curated-map persistent-workscreen diagnostic.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | partial | Existing repository and isolated worktree used; no new scaffold. |
| onboarding-guidance | exercised | Repository AGENTS and issue delivery route governed board/project and workflow. |
| skills | exercised | pnext, intra-repo, SDD lifecycle/stages, playtest, and feedback used. |
| sdd-authoring | exercised | Charter through ship completed; 39 observed obligations. |
| implementation-apis | exercised | Shared projection, Fable SVG renderer, CSS, and review generator changed. |
| dependencies-build | exercised | Targeted restores, Fable/Vite production build, and server publish passed. |
| testing | exercised | Client qualification, production smoke, browser journey, review verifier, and mutations exercised. |
| evidence | exercised | TRX receipt, evidence, verify, ship, and exact-bundle artifacts completed. |
| runtime-playtest | exercised | Real server-route Chromium journey advanced a maintained simulation. |
| performance | exercised | 100/200 projection budgets and browser callback inspection passed. |
| documentation | exercised | Visual direction, workspace, performance budget, and review README updated. |
| packaging-upgrade | not-exercised | No package upgrade or remediation required. |
| worker-git-pr | partial | Claim/worktree/commits exercised; PR/review/merge pending. |
