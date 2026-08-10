---
feedbackSchema: 2
date: 2026-08-11
workspace: S.I.R
cycle: item-155-client-module-boundaries
lane: sdd
toolVersion: 1.0.0
commit: pending-pr-head
---

## §1 Provenance and confidence
- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 8
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-155-client-module-boundaries.jsonl` (eight validated events)
- **confidence limits:** The executable emits one suite-level JUnit testcase; individual legacy assertions remain grouped under the production qualification run.

## §2 What worked
The existing deterministic client executable already exercised the production qualification route and became valid SDD evidence once it emitted a stable JUnit receipt. The existing 200-unit route also supplied the retained performance target without inventing a benchmark. Compilation-ordered App, MapEditor, and owned performance-suite modules make the repair reviewable without replacing the root Elmish program.

## §3 What did not
The executable had no machine-readable test result before this item, so SDD correctly refused authored pass claims without an observed-run receipt. The initial narrow seam was insufficient for the requested module decomposition; the repair moved real shell, browser, command, scene, panel, and editor-domain ownership and added subject-based anti-regrowth checks.

## §4 Findings
#### §4.1 Production client qualification lacked an observable test receipt
- **Kind:** capability-gap
- **Impact:** SDD verify could not distinguish a real passing client qualification from self-attested evidence.
- **Expected:** The production client test executable emits a deterministic machine-readable pass/fail receipt.
- **Observed:** `fsgg-sdd verify` reported five self-attested evidence entries with no observed run until `--junit` was added.
- **Evidence:** command:dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -- --junit TestResults/client-boundary.junit.xml
- **Version:** current
- **Owner:** S.I.R client tests
- **Recurrence:** resolved in this item
- **Avoidable cost:** one evidence repair loop
- **Disposition:** product fix

## §5 Did not exercise
Scaffolding, packaging upgrades, and browser runtime playtests are outside this focused client-boundary cut.

## §6 Doc-versus-behavior contradictions
The local feedback skill package is absent; the canonical external feedback tool was used.

## §7 Workarounds still in the tree
None observed.

## §8 Friction and avoidable cost
The first lifecycle pass stopped at the truthful observed-run gate; adding the deterministic JUnit switch removed that repeated manual-evidence cost.

## §9 Skill value and gaps
SDD evidence and verification gates correctly rejected self-attested tests and made the missing reporter actionable.

## §10 Outcome markers
The client qualification, Web Release build, production browser/player route, observed JUnit binding, JUnit malformed-path mutation, App/MapEditor subject-growth mutation receipts, both review-asset bindings, verify, ship, refresh, and generated agent guidance are green.

## §11 Falsifiable improvements
Any new executable test harness should expose a deterministic JUnit or TRX output mode before its results are used as SDD verification evidence.

## §12 Development-surface coverage
| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing workspace. |
| onboarding-guidance | exercised | Claimed route and baseline qualification. |
| skills | exercised | SDD, coordination, and feedback contracts. |
| sdd-authoring | exercised | Charter through ship completed. |
| implementation-apis | exercised | App shell/mode/scene/command/panel/browser and MapEditor type/history/revision/validation ownership extracted. |
| dependencies-build | exercised | Web Release build passed. |
| testing | exercised | Production executable emits deterministic JUnit, owned dense performance qualification, malformed path fails before qualification, and App/MapEditor/Program subject-growth mutations fail unchanged ceilings. |
| evidence | exercised | JUnit observed-run receipt bound to five obligations. |
| runtime-playtest | not-exercised | No browser-route change. |
| performance | exercised | Existing 200-unit production qualification retained its budget. |
| documentation | exercised | SDD and feedback artifacts authored. |
| packaging-upgrade | not-exercised | Out of scope. |
| worker-git-pr | exercised | Isolated claimed worktree prepared for review. |
