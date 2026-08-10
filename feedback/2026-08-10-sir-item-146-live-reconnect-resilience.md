---
feedbackSchema: 2
date: 2026-08-10
workspace: S.I.R
cycle: item-146-live-reconnect-resilience
lane: sdd
toolVersion: 1.0.0
commit: f02a5596849ae116dba4cb35239ad7e9836cb8d7
---

## §1 Provenance and confidence
- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 3
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-146-live-reconnect-resilience.jsonl` (three validated events)
- **confidence limits:** M9 and generated-view obligations are explicit deferrals because their scripts do not emit an SDD-observable machine receipt.

## §2 What worked
The typed SDD lifecycle and production Chromium journey exposed the separate DOM live slice and verified the Elmish replacement.

## §3 What did not
The route reader crashes for a noncanonical repository ref. An initial SDD generated-view failure was resolved by correcting the authored `DEC-###:` grammar. Stock Playwright JUnit receipts also varied across equivalent runs; the browser suite now owns a deterministic JUnit projection so observed evidence remains reproducible.

## §4 Findings
#### §4.1 Noncanonical delivery-route ref crashes the coordinator reader
- **Kind:** defect
- **Impact:** route admission cannot safely decide until a host diagnoses the ref normalization.
- **Expected:** invalid repository input is refused without an engine exception.
- **Observed:** `EHotwagner/S.I.R#146` reaches a null GraphQL repository and throws in Reads.fs.
- **Evidence:** command:scripts/fsgg-coord delivery-route show EHotwagner/S.I.R#146 --json
- **Version:** current coordinator engine
- **Owner:** FS.GG.Coord GitHub Reads
- **Recurrence:** new
- **Avoidable cost:** one route-read recovery
- **Disposition:** product fix

#### §4.2 SDD hides malformed decision references behind unlocated derived diagnostics
- **Kind:** documentation
- **Impact:** an author must perform producer-source archaeology to resolve an otherwise unlocated generated-view failure.
- **Expected:** diagnostics identify the malformed authored decision reference and its source location.
- **Observed:** verify/ship collapsed three malformed decision references into `unknownReference` and `workModelInconsistent` counts without locations; changing `DEC-001..003` to list-leading `DEC-###:` resolved the current item and refresh now succeeds.
- **Evidence:** command:fsgg-sdd refresh --work 146-authoritative-live-snapshots --text
- **Version:** fsgg-sdd 1.0.0
- **Owner:** S.I.R SDD authoring guidance
- **Recurrence:** new
- **Avoidable cost:** repeated generator attempts
- **Disposition:** doc fix

## §5 Did not exercise
Scaffolding and packaging upgrades were out of scope.

## §6 Doc-versus-behavior contradictions
The local feedback contract names a missing local feedback tool; the canonical template tool was used.

## §7 Workarounds still in the tree
None observed.

## §8 Friction and avoidable cost
Route normalization and browser provisioning each required one recovery loop.

## §9 Skill value and gaps
SDD, pnext-item, and intra-repo coordination were exercised; the repository-local feedback package is absent.

## §10 Outcome markers
Solution build, Fable/Vite build, M4/M9 qualifications, and the system-Chromium browser journey passed. Mutations of `data-live-tick` and the visible Disconnect dispatch made the journey red. The deterministic JUnit report records failure counts and failure nodes while omitting run-varying clock and duration data.

## §11 Falsifiable improvements
Coordinator input validation must return a typed refusal for a repository without its trailing dot; clarification guidance should emphasize the list-leading `DEC-###:` grammar that generated views require.

## §12 Development-surface coverage
| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing workspace. |
| onboarding-guidance | exercised | Typed claim and route gate. |
| skills | exercised | SDD and coordination skills used. |
| sdd-authoring | exercised | Charter through ship completed. |
| implementation-apis | exercised | Elmish live transport boundary. |
| dependencies-build | exercised | Solution and client builds passed. |
| testing | exercised | Browser journey and mutation passed. |
| evidence | exercised | Observed browser JUnit bound. |
| runtime-playtest | exercised | System Chromium journey passed. |
| performance | partial | No typed target; structural route retained. |
| documentation | exercised | Review assets regenerated. |
| packaging-upgrade | not-exercised | Out of scope. |
| worker-git-pr | exercised | Isolated claimed worktree. |
