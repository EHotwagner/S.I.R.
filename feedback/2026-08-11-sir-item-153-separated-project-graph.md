---
feedbackSchema: 2
date: 2026-08-11
workspace: S.I.R
cycle: item-153-separated-project-graph
lane: sdd
toolVersion: 1.0.0
commit: 0c3e42e89e527501c71e3fa8ff1c06d18a8c9633
---

## §1 Provenance and confidence
- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 2
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-153-separated-project-graph.jsonl`
- **confidence limits:** Hosted CI remains the authority for the canonical checkout docs title.

## §2 What worked
The separated project extraction retained the published HTTP/Thoth and SignalR route while making the live client simulation-free.

## §3 What did not
Initial extraction coupled browser-only dependencies into the Fable test route; the docs verifier also inferred an API path from assembly ownership rather than the retained public namespace.

## §4 Findings
#### §4.1 Browser-free replay core preserves Fable test isolation
- **Kind:** friction
- **Impact:** Modal Fable tests failed after they referenced the full browser host.
- **Expected:** Replay/editor shared sources compile without Feliz/Thoth browser-host dependencies.
- **Observed:** Introducing `SIR.Replay.Core` restored the focused modal Fable run.
- **Evidence:** command:dotnet fable tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj
- **Version:** Fable 5.13.0
- **Owner:** S.I.R project graph
- **Recurrence:** new
- **Avoidable cost:** one repair round and focused Fable rerun
- **Disposition:** fixed in this item

#### §4.2 API documentation follows public namespace identity
- **Kind:** documentation
- **Impact:** The verifier expected a non-existent Wasm-named MatchReplay page.
- **Expected:** Docs verification asserts the generated `sir-match-matchreplay` page while Wasm is included in docs generation.
- **Observed:** FsDocs retained the `SIR.Match` namespace path after source ownership moved.
- **Evidence:** command:npm run build:docs
- **Version:** FsDocs 22.1.0
- **Owner:** S.I.R documentation verification
- **Recurrence:** new
- **Avoidable cost:** one docs build and verifier repair
- **Disposition:** fixed in this item

## §5 Did not exercise
No external hosted compositor was exercised.

## §6 Doc-versus-behavior contradictions
None after the API reference correction.

## §7 Workarounds still in the tree
None.

## §8 Friction and avoidable cost
Two checkpoints capture the test-isolation and API-path assumptions repaired in this item.

## §9 Skill value and gaps
The external canonical feedback tool validated checkpoints because the documented local feedback skill was absent.

## §10 Outcome markers
Project graph self-tests reject client and simulation forbidden edges; solution build, focused Fable route, and conformance passed.

## §11 Falsifiable improvements
Keep the Simulation-to-Wasm subject mutation in the verifier self-test.

## §12 Development-surface coverage
| Surface | Status | Evidence and result |
|---|---|---|
| sdd-authoring | exercised | charter through ship completed. |
| implementation-apis | exercised | separated project boundaries compiled. |
| testing | exercised | conformance and graph mutations run. |
| evidence | exercised | observed JUnit receipt bound to obligations. |
| documentation | exercised | FsDocs API path verified. |
| worker-git-pr | exercised | isolated claim, PR, and review handoff. |
| scaffolding | not-exercised | Existing repository. |
| onboarding-guidance | exercised | Claim and SDD route read. |
| skills | exercised | SDD, board, and feedback contracts used. |
| dependencies-build | exercised | Locked restore and release build run. |
| runtime-playtest | exercised | Published browser route exercised by conformance. |
| performance | partial | No typed item performance intent exists. |
| packaging-upgrade | not-exercised | Out of scope. |
