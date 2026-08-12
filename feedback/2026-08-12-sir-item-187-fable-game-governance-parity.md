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
- **material events:** 7
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-187-fable-game-governance-parity.jsonl` (7 events).
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

#### §4.5 FSharp.Core 10.1.302 cannot yield a stable locked restore

- **Kind:** defect
- **Impact:** The same package version exposes incompatible registration metadata, flat-container bytes, and SDK-pack bytes, making a hash-only lock repair unsound.
- **Expected:** A pinned package version must restore consistently through the locked CI route.
- **Observed:** Clean diagnostics established three incompatible 10.1.302 identities; pinning 10.1.303 and regenerating locks restored clean locked solution restore.
- **Evidence:** command:dotnet restore SIR.slnx --locked-mode --no-cache; artifact:Directory.Packages.props
- **Version:** FSharp.Core 10.1.302 to 10.1.303
- **Owner:** NuGet package metadata and S.I.R dependency maintenance
- **Recurrence:** new
- **Avoidable cost:** two failed exact-head CI restores plus clean-cache diagnosis
- **Disposition:** fixed by compatible patch upgrade

#### §4.6 Durable evidence must be force-tracked beneath ignored test-results

- **Kind:** defect
- **Impact:** SDD accepted the declared Game.Core cross-runtime receipt locally, but a fresh reviewer checkout could not read it because `test-results/` is globally ignored.
- **Expected:** An evidence artifact bound to EV002/EV003 is committed and available from the exact review head.
- **Observed:** Rebuilt the artifact from baseline equality plus all eight .NET/Fable subject-mutation executions and force-added the exact file.
- **Evidence:** artifact:work/187-fable-game-governance-parity/test-results/game-core-cross-runtime.json; command:dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj --no-build --no-restore -- --inject-divergence game-core-cell-order
- **Version:** S.I.R current candidate
- **Owner:** S.I.R SDD evidence packaging
- **Recurrence:** new
- **Avoidable cost:** one critic-confirmation repair round and repeat cross-runtime execution
- **Disposition:** fixed by tracked durable artifact

## §5 Did not exercise

No new player-reachable game route was added, so a bot-driven gameplay journey was not applicable.

## §6 Doc-versus-behavior contradictions

None observed.

## §7 Workarounds still in the tree

None observed.

## §8 Friction and avoidable cost

The coordination identifier, undeclared clarification grammar, corrupt FSharp.Core 10.1.302 package identity, and ignored durable receipt each required a recovery loop; all are captured above.

Independent review also found that the prior generic conformance evidence did not exercise the four promised Game.Core primitives; the repaired route now does so in both .NET and Fable.

## §9 Skill value and gaps

`pnext-item`, `fs-gg-game-fable`, and the SDD lifecycle skills bounded recovery work. The external feedback tool was required because this checkout does not materialize it under `.agents/skills`.

## §10 Outcome markers

The governance verifier passed, the skill-equivalence subject mutation reddened the verifier, `fsgg-sdd ship` reported `shipReady`, refresh reported all generated views current, and full conformance passed.

## §11 Falsifiable improvements

- The coordinator must reject a repository identifier lacking its trailing dot without throwing.
- SDD must name malformed clarification declaration lines when they cause unresolved decision references.
- Package producers must not republish or serve incompatible byte identities for one immutable version.
- Evidence tooling should flag a declared artifact that is ignored and not tracked.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing repository; no scaffold operation in this lane. |
| onboarding-guidance | exercised | Typed claim, route, and recovery contract used. |
| skills | exercised | Fable package-consumer and SDD skills used. |
| sdd-authoring | exercised | Canonical 187 lifecycle regenerated through ship and refresh. |
| implementation-apis | exercised | The governance verifier now produces and validates the public-surface receipt. |
| dependencies-build | exercised | FSharp.Core 10.1.303 lock regeneration and clean locked restore passed. |
| testing | exercised | `./scripts/test-conformance.sh` passed; skill-equivalence mutation red. |
| evidence | exercised | Force-tracked Game.Core baseline and eight-mutation receipt is bound to EV002/EV003. |
| runtime-playtest | not-exercised | No new reachable gameplay was added. |
| performance | exercised | Existing conformance performance qualifications passed. |
| documentation | exercised | Governance parity documentation remains part of the conformance route. |
| packaging-upgrade | exercised | Governance CLI/tool pins were upgraded to published 1.12.1. |
| worker-git-pr | exercised | Isolated recovery claim and preserved-PR replacement workflow used. |
