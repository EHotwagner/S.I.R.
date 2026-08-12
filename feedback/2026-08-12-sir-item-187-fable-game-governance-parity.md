---
feedbackSchema: 2
date: 2026-08-12
workspace: S.I.R
cycle: item-187-fable-game-governance-parity
lane: sdd
toolVersion: 1.0.1
commit: pending-pr-head
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 4
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-187-fable-game-governance-parity.jsonl` (4 events).
- **confidence limits:** Local exact-head conformance passed; hosted CI and fresh independent review remain pending.

## §2 What worked

The published `FS.GG.Governance.FSharpSurfaceCommand` 1.12.1 generated the real receipt that SDD consumes. The SDD lifecycle then regenerated a current work model, ship verdict, summary, and equivalent Claude/Codex guidance.

## §3 What did not

An omitted trailing dot in the GitHub repository identifier crashed the coordination route reader. The SDD diagnostic also identified only the derived work-model failure until the clarification declaration grammar was inspected.

## §4 Findings

#### §4.1 Noncanonical repository input crashes the coordination reader

- **Kind:** defect
- **Impact:** A worker cannot safely obtain the delivery route or claim receipt from a repository identifier missing its required trailing dot.
- **Expected:** A malformed repository identifier is rejected with a typed input error.
- **Observed:** `EHotwagner/S.I.R#187` produced a null-payload engine exception.
- **Evidence:** command:scripts/fsgg-coord delivery-route show EHotwagner/S.I.R#187 --json
- **Version:** FS.GG.Coord current engine
- **Owner:** FS.GG.Coord GitHub Reads
- **Recurrence:** seen again feedback/2026-08-10-sir-item-146-live-reconnect-resilience.md
- **Avoidable cost:** one recovery diagnosis
- **Disposition:** existing issue

#### §4.2 Clarification declaration grammar needs direct diagnostics

- **Kind:** documentation
- **Impact:** A valid-looking decision can prevent current generated views without naming the authoring line that must change.
- **Expected:** The lifecycle identifies the malformed `DEC-###:` declaration alongside derived reference diagnostics.
- **Observed:** Correcting `DEC-001` to the list-leading declaration grammar restored work-model generation and refresh.
- **Evidence:** command:dotnet fsgg-sdd refresh --work 187-fable-game-governance-parity --root . --text
- **Version:** FS.GG.SDD 1.0.1
- **Owner:** FS.GG.SDD authoring diagnostics
- **Recurrence:** seen again feedback/2026-08-10-sir-item-146-live-reconnect-resilience.md
- **Avoidable cost:** multiple lifecycle regeneration attempts and source inspection
- **Disposition:** doc fix

#### §4.3 Producer-owned surface command gives SDD honest public-surface evidence

- **Kind:** positive-pattern
- **Impact:** The consumer can prove F# public-surface applicability without copying or fabricating producer output.
- **Expected:** Published governance tooling writes the receipt consumed by SDD ship.
- **Observed:** `fsgg-fsharp-surface` generated `readiness/fsharp-public-surface.json`, after which SDD ship became ready.
- **Evidence:** command:dotnet tool run fsgg-fsharp-surface -- --root . --project src/SIR.Simulation/SIR.Simulation.fsproj; command:dotnet fsgg-sdd ship --work 187-fable-game-governance-parity --root . --text
- **Version:** FS.GG.Governance.FSharpSurfaceCommand 1.12.1
- **Owner:** FS.GG.Governance FSharpSurfaceCommand
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.4 Full conformance kept package, governance, and runtime checks coupled

- **Kind:** positive-pattern
- **Impact:** The recovered implementation was verified through the same clean route used by CI.
- **Expected:** The conformance route restores tools, validates governance, builds, and compares .NET/Fable outputs.
- **Observed:** `./scripts/test-conformance.sh` passed after the producer adoption and SDD repair.
- **Evidence:** command:./scripts/test-conformance.sh
- **Version:** S.I.R current candidate
- **Owner:** S.I.R conformance scripts
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

## §5 Did not exercise

No new player-reachable game route was added, so a bot-driven gameplay journey was not applicable.

## §6 Doc-versus-behavior contradictions

None observed.

## §7 Workarounds still in the tree

None observed.

## §8 Friction and avoidable cost

The coordination identifier and undeclared clarification grammar each required a recovery loop; both are captured above.

## §9 Skill value and gaps

`pnext-item`, `fs-gg-game-fable`, and the SDD lifecycle skills bounded recovery work. The external feedback tool was required because this checkout does not materialize it under `.agents/skills`.

## §10 Outcome markers

The governance verifier passed, the skill-equivalence subject mutation reddened the verifier, `fsgg-sdd ship` reported `shipReady`, refresh reported all generated views current, and full conformance passed.

## §11 Falsifiable improvements

- The coordinator must reject a repository identifier lacking its trailing dot without throwing.
- SDD must name malformed clarification declaration lines when they cause unresolved decision references.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing repository; no scaffold operation in this lane. |
| onboarding-guidance | exercised | Typed claim, route, and recovery contract used. |
| skills | exercised | Fable package-consumer and SDD skills used. |
| sdd-authoring | exercised | Canonical 187 lifecycle regenerated through ship and refresh. |
| implementation-apis | exercised | The governance verifier now produces and validates the public-surface receipt. |
| dependencies-build | exercised | Published Governance tool 1.12.1 restored and full build passed. |
| testing | exercised | `./scripts/test-conformance.sh` passed; skill-equivalence mutation red. |
| evidence | exercised | Public-surface receipt and observed SDD evidence are current. |
| runtime-playtest | not-exercised | No new reachable gameplay was added. |
| performance | exercised | Existing conformance performance qualifications passed. |
| documentation | exercised | Governance parity documentation remains part of the conformance route. |
| packaging-upgrade | exercised | Governance CLI/tool pins were upgraded to published 1.12.1. |
| worker-git-pr | exercised | Isolated recovery claim and preserved-PR replacement workflow used. |
