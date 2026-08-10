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
- **material events:** 2
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-146-live-reconnect-resilience.jsonl` (two validated events)
- **confidence limits:** M9 and generated-view obligations are explicit deferrals because their scripts do not emit an SDD-observable machine receipt.

## §2 What worked
The typed SDD lifecycle and production Chromium journey exposed the separate DOM live slice and verified the Elmish replacement.

## §3 What did not
The route reader crashes for a noncanonical repository ref, and the SDD refresh generator remains inconsistent after ship readiness.

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

#### §4.2 SDD refresh remains blocked after shipReady
- **Kind:** defect
- **Impact:** generated guidance cannot be refreshed for an otherwise ship-ready work item.
- **Expected:** refresh recognizes the work model created by verify/ship.
- **Observed:** refresh says analysis waits for work-model while ship reports generated views current.
- **Evidence:** command:fsgg-sdd refresh --work 146-authoritative-live-snapshots --text
- **Version:** fsgg-sdd 1.0.0
- **Owner:** FS.GG SDD generated-view tooling
- **Recurrence:** new
- **Avoidable cost:** repeated generator attempts
- **Disposition:** product fix

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
Solution build, Fable/Vite build, M4/M9 qualifications, and the system-Chromium browser journey passed. Mutation of `data-live-tick` made the journey red.

## §11 Falsifiable improvements
Coordinator input validation must return a typed refusal for a repository without its trailing dot; SDD refresh must emit generated guidance after ship.

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
