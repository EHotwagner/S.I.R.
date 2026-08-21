---
feedbackSchema: 2
date: 2026-08-21
workspace: S.I.R-item-231-svg-measurement-repair-2
cycle: item-231-svg-pipeline-measurement-repair-phase
lane: sdd
toolVersion: 1.0.1
commit: 0c7ee12b9134b2e4ef2a02d1a59eca4af9f81332
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** independent-review-repair-round-4
- **material events:** 2
- **zero-event reason:** not applicable

This round-4 addendum covers two of the seven total events in the stable checkpoint stream: repair-introduced receipt contamination and avoidable stress-fixture reconstruction. The stable checkpoint is `feedback/checkpoints/item-231-svg-pipeline-measurement-repair-phase.jsonl`. Confidence is high for the immutable hosted receipt/join failure, executable extra-manifest isolation, three local unchanged-threshold stress runs, required browser-WASM verification, and coherent SDD views. Replacement exact-head hosted timing and same-critic confirmation remain pending.

## §2 What worked

The typed join rejected the contaminated rules receipt even though both co-scheduled commands passed. The independent review traced that failure to ambient manifest discovery. The stress test retained raw samples and separate p95/p99 ceilings, making the allocation-pressure repair measurable without relaxing acceptance.

## §3 What did not

The round-3 addendum stated that rules and spatial shared only a consumer and prepared inputs. Hosted execution contradicted that claim: `run-ci-gate.sh` discovered every manifest present in the workspace, so rules incorrectly bound native and Fable and failed the join's native-only contract. Separately, the canonical stress qualification recreated the same immutable 80x80/200-unit package on every warm-up and measured iteration, introducing an unmeasured allocation/GC confounder; hosted p95 reached 117.8033 ms and aborted before browser-WASM verification, but the confounder's causal contribution is not quantified.

## §4 Findings

#### §4.1 Ambient manifest discovery contaminated a co-scheduled typed receipt

- **Kind:** defect
- **Impact:** Rules and spatial commands passed, but the exact-head aggregate rejected rules with artifact-binding and artifact-set-digest mismatches, preventing an otherwise timing-green run from qualifying.
- **Expected:** Each typed gate receipt binds exactly the prepared subjects declared by that gate's preflight contract, regardless of unrelated manifests already present in a shared workspace.
- **Observed:** At head `5911bd20a573ac57edf5147d3a18aeb417b9b12c`, rules ran after native and Fable artifacts had both been downloaded. `run-ci-gate.sh` globbed every manifest pointer and emitted native+Fable bindings for the native-only rules receipt. Candidate `0c7ee12b9134b2e4ef2a02d1a59eca4af9f81332` derives bindings only from `preflight_parts`; the candidate's executable fixture verifies that requesting native with an extra Fable manifest present returns only native and rejects a missing requested subject.
- **Evidence:** review:https://github.com/EHotwagner/S.I.R./pull/246#issuecomment-5373815756; run:https://github.com/EHotwagner/S.I.R./actions/runs/32513160418; command:bash scripts/test-ci-gate-artifact-isolation.sh
- **Version:** affected head 5911bd20a573ac57edf5147d3a18aeb417b9b12c; local repair candidate 0c7ee12b9134b2e4ef2a02d1a59eca4af9f81332
- **Owner:** EHotwagner/S.I.R. CI gate receipt binding
- **Recurrence:** new exact receipt-contamination cause within #231/PR #246; earlier feedback documented artifact binding generally but did not identify ambient extra-manifest discovery
- **Avoidable cost:** one terminal hosted run; no unchanged retry
- **Disposition:** product fix under existing #231/PR #246

#### §4.2 Repeated immutable-fixture construction was an unmeasured stress-smoke confounder

- **Kind:** defect
- **Impact:** The hosted route failed p95 at 117.8033 ms and aborted before browser-WASM verification. The old route rebuilt the immutable package before each batch of timed actions, introducing unmeasured allocation/GC sensitivity; its contribution to the hosted failure is not quantified.
- **Expected:** Warm-up and measured iterations execute the same production stress route over one immutable versioned fixture; fixture construction is setup, while p95/p99 retain the existing 100/125 ms ceilings and browser-WASM evidence remains required.
- **Observed:** `runStressRoute` called `ExperienceSamples.stressPackage()` on every invocation despite its caller already constructing the immutable package. Candidate `0c7ee12b9134b2e4ef2a02d1a59eca4af9f81332` passes the single package into every warm-up and measured route. Three critic-verified Release executions retained p95 50.614–52.098 ms and p99 52.866–55.577 ms under the unchanged ceilings; `test-browser-wasm-verification.sh` then built the required project and passed its deterministic vector check. Replacement hosted effectiveness remains pending, and the magnitude of allocation/GC contribution is unknown.
- **Evidence:** review:https://github.com/EHotwagner/S.I.R./pull/246#issuecomment-5373815756; run:https://github.com/EHotwagner/S.I.R./actions/runs/32513160418; command:dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-restore; command:./scripts/test-browser-wasm-verification.sh
- **Version:** affected head 5911bd20a573ac57edf5147d3a18aeb417b9b12c; local repair candidate 0c7ee12b9134b2e4ef2a02d1a59eca4af9f81332; .NET SDK 10.0.302
- **Owner:** EHotwagner/S.I.R. scenario catalog performance qualification
- **Recurrence:** new fixture-construction cause within the recurring hosted-timing problem tracked by closed #220; prior feedback did not identify repeated `stressPackage()` construction
- **Avoidable cost:** one terminal hosted run; no unchanged retry or threshold relaxation
- **Disposition:** product fix under existing #231/PR #246

## §5 Did not exercise

Replacement exact-head hosted qualification, same-critic confirmation round 4, host acceptance, merge, and done remain unexercised.

## §6 Doc-versus-behavior contradictions

Owning documentation: `feedback/2026-08-21-sir-item-231-svg-pipeline-measurement-repair-phase-confirmation-3.md`, §7. It states, “Rules and spatial remain separate typed receipt commands and outputs; only their consumer job and prepared inputs are shared.” Hosted run 32513160418 instead emitted the rules receipt with both native and Fable artifact bindings, and its join reported `artifact-binding-mismatch:rules` plus `artifact-set-digest-mismatch:rules`. The earlier report's “only” claim is therefore contradicted; the immutable earlier report remains preserved.

## §7 Workarounds still in the tree

None. Subject-scoped binding uses the existing gate `preflight_parts` authority, and stress iterations reuse the same immutable value rather than bypassing work or changing ceilings.

## §8 Friction and avoidable cost

One hosted run met timing but failed two substantive receipts. The receipt defect required one executable isolation fixture. Three critic-verified local executions established unchanged-threshold p95/p99 behavior after the stress-fixture repair. No threshold changed and no unchanged hosted retry ran.

## §9 Skill value and gaps

The generated work-model inventory lists automated tests, deterministic JSON, F#, implementation, readiness evidence, and schema versioning. This round exercised executable receipt mutations, F# performance qualification, readiness evidence, and schema-versioned feedback. `pnext-item`, `intra-repo-parallel-work`, the performance-first gate, SDD lifecycle skills, `fs-gg-feedback-report`, and the independent-review repair contract governed the round. No gameplay or package-publication skill applied.

## §10 Outcome markers

The extra-manifest isolation fixture passes and missing requested subjects fail closed. Three critic-verified Release stress executions pass unchanged 100/125 ms ceilings, with observed p95 50.614–52.098 ms and p99 52.866–55.577 ms. Browser-WASM verification builds and passes. F1 hook mutations, route topology mutations, SVG qualification, and audit-binding mutants remain green. Analyze, verify, and ship each return `noChange` with `coherent:true`; verify retains 25 observed evidence and 25 observed tests, and ship is `shipReady`. Replacement hosted proof remains pending, and the fixture reconstruction's causal contribution is not quantified.

## §11 Falsifiable improvements

For §4.1, owner `EHotwagner/S.I.R.` should keep receipt bindings derived exclusively from each gate's declared preflight subjects. Acceptance is that an executable fixture with an unrelated manifest present emits only requested bindings, a missing requested subject fails closed, and exact-head join reports no binding/digest mismatch.

For §4.2, owner `EHotwagner/S.I.R.` should keep immutable fixture construction outside repeated stress-route measurement. Acceptance is unchanged 100/125 ms p95/p99 ceilings passing on a representative changed hosted head and a recorded `build:spikes/browser-wasm-verification/BrowserWasmVerificationSpike.fsproj` invocation.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing S.I.R. scaffold retained. |
| onboarding-guidance | partial | Existing claim and repair-review guidance governed the round. |
| skills | exercised | Coordination, performance-first, SDD, feedback, and repair-review contracts were followed. |
| sdd-authoring | exercised | Canonical lifecycle and exact-commit no-change proof passed. |
| implementation-apis | partial | CI binding and test qualification changed; runtime product APIs did not. |
| dependencies-build | exercised | Browser-WASM project restored/built after the repaired stress test. |
| testing | exercised | Extra-manifest isolation, stress p95/p99, route, F1, SVG, and audit-binding gates passed. |
| evidence | exercised | Hosted receipts, executable mutations, SDD, checkpoint, and invalidation evidence were inspected. |
| runtime-playtest | not-exercised | No gameplay change occurred. |
| performance | exercised | Three local unchanged-threshold runs passed; replacement hosted proof is pending. |
| documentation | partial | SDD plan and this addendum record the corrected contracts and contradiction. |
| packaging-upgrade | not-exercised | No package version or publication changed. |
| worker-git-pr | partial | Live claim and bounded repair discipline were exercised; push and confirmation remain pending. |
