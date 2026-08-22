# Local implementation qualification

Date: 2026-08-22

This is diagnostic implementation evidence, not SDD stage-8 evidence. Final
FR-005 through FR-007 acceptance still requires one source-frozen exact-head
hosted run and `sir.ci-cost-report/v1` reconciliation.

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

## Deliberately pending

- Exact-head hosted runner time, critical path, action/post-step residual, and
  artifact-byte reconciliation.
- The >=20% hosted runner-time reduction and <=240-second verdict ceiling.
- Any setup-dominated domain-job consolidation. The observer must first show
  that consolidation improves the critical path; retaining the current subject
  jobs is safer than speculating locally.
- SDD evidence, verify, ship, generated-view refresh, and any protected delivery.
