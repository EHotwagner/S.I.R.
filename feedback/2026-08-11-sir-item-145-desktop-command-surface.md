---
feedbackSchema: 2
date: 2026-08-11
workspace: S.I.R
cycle: item-145-desktop-command-surface
lane: sdd
toolVersion: 1.0.0
commit: pending-pr-head
---

## §1 Provenance and confidence
- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 1
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-145-desktop-command-surface.jsonl`
- **confidence limits:** Chromium was exercised through the locally published production server; CI has not yet run on the candidate PR.

## §2 What worked
The focused production route retained a trace and deterministic JUnit result, making the apparent no-output runner failure diagnosable without weakening the test.

## §3 What did not
The reset assertion used visible text even though the reset control deliberately exposes a longer accessible name. The role query therefore timed out despite the control being rendered.

## §4 Findings
#### §4.1 Accessible-name assertions must target the control's public accessibility contract
- **Kind:** friction
- **Impact:** The toolbar-customization browser test spent its 30-second timeout waiting for a reset button that was present but queried by the wrong accessible name.
- **Expected:** A Playwright role query identifies the reset control through its actual accessibility name and the test proves reorder, persistence, reset, menu focus, Escape dismissal, and compact overflow behavior.
- **Observed:** The reset control has accessible name `Restore the documented default top toolbar`; the test queried `Reset toolbar`. The direct trace showed a real locator timeout, exit 1, and JUnit output. Correcting the query and adding the reorder assertion produced 3 passing focused tests with JUnit output and no lingering browser or server process.
- **Evidence:** command:npx playwright test --config tests/SIR.Browser.Tests/playwright.config.js tests/SIR.Browser.Tests/desktop-command-surface.spec.js --trace on
- **Version:** Playwright 1.62.1; system Chromium 151.0.7922.108
- **Owner:** S.I.R browser-test suite
- **Recurrence:** new
- **Avoidable cost:** one focused reproduction and one locator correction
- **Disposition:** product test fix

## §5 Did not exercise
No live compositor or externally hosted browser route was exercised; the declared acceptance route is the locally published production server.

## §6 Doc-versus-behavior contradictions
None observed.

## §7 Workarounds still in the tree
None.

## §8 Friction and avoidable cost
One material checkpoint records the misleading initial runner presentation. The trace, explicit shell exit, report mtime, and process inspection separated test timeout from server lifetime or process-signal hypotheses.

## §9 Skill value and gaps
The SDD observed-run receipt required the focused JUnit output to be bound to each real verification obligation. The feedback contract made the runner diagnosis durable rather than leaving it only in terminal output.

## §10 Outcome markers
Release publish, client asset build, conformance, and documentation builds completed. The focused published Chromium route passed 3/3 in 2.9 seconds. SDD evidence, verify, ship, and refresh are current; ship reports 13 observed real verification passes.

## §11 Falsifiable improvements
For §4.1, retain role-based queries against explicit accessible names and require the focused test to fail when the shortcut, reorder, persistence, reset, focus/Escape, or overflow contract is mutated.

## §12 Development-surface coverage
| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing workspace and worktree recovery. |
| onboarding-guidance | exercised | Coordination identity, route receipt, and claim were recovered. |
| skills | exercised | SDD, coordination, work-board, and feedback contracts used. |
| sdd-authoring | exercised | Evidence, verify, ship, refresh, and generated guidance are current. |
| implementation-apis | exercised | Shared desktop command surface was inspected through its production browser route. |
| dependencies-build | exercised | Release publish and client asset build completed. |
| testing | exercised | Focused Playwright route passes and a targeted mutation failed. |
| evidence | exercised | Deterministic JUnit is bound to the SDD observed-run receipts. |
| runtime-playtest | exercised | Chromium used the published local server. |
| performance | partial | Existing conformance performance qualifications ran; no item-specific performance intent applies. |
| documentation | exercised | Documentation build and this schema-v2 report completed. |
| packaging-upgrade | not-exercised | Out of scope. |
| worker-git-pr | exercised | Isolated branch, lease, SDD evidence, and review handoff prepared. |
