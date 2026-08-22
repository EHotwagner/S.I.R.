# Local implementation qualification

Date: 2026-08-22

This records both diagnostic local qualification and the source-frozen hosted
acceptance observation. The compact hosted receipt is
`hosted-observation.json`; its executable validation is
`hosted-verification.sh`.

## Clean producer candidate

All five producer parts were built from a temporary committed clean worktree and
packed through `sir.ci-artifact-manifest/v2` / `sir.ci-content-index/v1`.

| Producer | Archive bytes | Logical bytes | Unique object bytes |
| --- | ---: | ---: | ---: |
| native | 36,341,760 | 70,395,259 | 36,213,075 |
| fable | 3,584,000 | 8,036,693 | 3,362,921 |
| web | 30,504,960 | 34,583,467 | 29,959,186 |
| server | 33,269,760 | 33,205,180 | 33,205,180 |
| docs | 1,904,640 | 6,080,359 | 1,865,955 |
| total | 105,605,120 | 152,300,958 | 104,606,317 |

The local archive total is 69.94% below the 351,337,554-byte reference run.
Native and server pruning each removed 98,367,188 bytes of non-`linux-x64`
runtime assets. Native CAS storage deduplicated a further 34,182,184 logical
bytes. Both archives reconstructed exactly, and the reconstructed server started.

## Passing fail-capable controls

- CI route matrix and route mutations, including production-review selection.
- Stale production-review bundle mutation and restored positive control.
- Protected early/middle/late receipt failures and deterministic join rejection.
- Pages exact-run artifact handoff and no-rebuild topology.
- GitHub Jobs/Artifacts cost reconciliation and incomplete-inventory rejection.
- Missing, malformed, symbolic, and unexpected Linux runtime inventories.
- CAS omitted/corrupt/extra-object, path, mode, and transport mutations.
- Supported full-SHA action pins, version comments, explicit timeouts, and least
  permissions.
- Conservative integrity classification for relevant, unrelated, unknown,
  malformed, rename/delete, workflow, topology, and classifier changes.
- Full repository integrity floor plus every optional integrity subject (the
  candidate changes workflow topology, so no optional subject was omitted).
- Feedback audit-binding exception mutations and `git diff --check`.

## Exact-head hosted acceptance

GitHub Actions run
[32572941237](https://github.com/EHotwagner/S.I.R./actions/runs/32572941237)
completed successfully at candidate `71212b653e5a8c65d04bdab049cebcb71d9c34da`
(tree `ff2152fe0d1f9d0d83b051f1fd9a58c1824094ea`) with no retry.

| Metric | Baseline | Target | Observed | Change |
| --- | ---: | ---: | ---: | ---: |
| Developer wait to PR verdict | n/a | <=240,000 ms | 232,147 ms | target passed by 7,853 ms |
| Aggregate GitHub runner consumption | 1,518,000 ms | <=1,214,400 ms | 1,182,000 ms | -22.134% |
| Uploaded artifact bytes | 351,337,554 | <=281,070,043 | 106,980,560 | -69.550% |

The 232,147 ms verdict is about 3 minutes 52 seconds of elapsed developer wait.
The 1,182,000 ms value is about 19 minutes 42 seconds summed across concurrent
runners; it is consumption, not elapsed wait. The `sir.ci-cost-report/v1`
observer reported `complete`, zero receipt mismatches, and a passing baseline
comparison. All selected rules, spatial, cancellation, mutation, cross-runtime,
browser, documentation, and evidence subjects passed.

The hosted prepared payloads were 39,223,632 native bytes, 4,077,735 Fable
bytes, 31,312,817 web bytes, and 32,347,228 documentation bytes: 106,961,412
bytes in total. Small route, gate, verdict, and cost receipts account for the
remaining observed uploaded bytes.

## Remaining protected-boundary control

The final protected-main, scheduled, and Pages observations in VO-010 remain
pending until this candidate crosses the protected merge boundary. Their
workflow topology and fail-closed mutation controls pass locally, but this PR
run cannot truthfully stand in for those post-merge events. SDD verification
and ship readiness therefore remain open on that single boundary control.
