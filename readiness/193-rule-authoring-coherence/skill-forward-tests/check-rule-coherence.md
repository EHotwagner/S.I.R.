# Cold forward test: `sir-check-rule-coherence`

- Evaluator: `/root/lane_193_worker/check_skill_cold_repair`
- Runtime route: `gpt-5.6-sol`, medium reasoning, fresh context
- Isolation: focused command/report only; no issue, PR, SDD, GitHub, broad build, or tracked edit
- Expected diagnosis disclosure: none was supplied to the evaluator
- Observed result: pass

## Prompt supplied

> The user asks: “Can the current combat rules corpus be certified with complete support for canonicalization?” Use the installed `$sir-check-rule-coherence` skill exactly as written. Work read-only except for ignored cache/report output explicitly directed by the skill; do not edit tracked files, run broad builds, browse GitHub, or read issue/PR/SDD artifacts. Run only the focused command(s) the skill selects. Return the bounded verdict, exact command and exit/result facts, scope/strength/witness/cost/cache interpretation, smallest next question, resources consulted, and any workflow ambiguity. You are not given an expected diagnosis; derive it independently.

## Observed behavior

The evaluator independently selected:

```sh
scripts/sir-rules check --mode corpus --block-unknowns \
  --cache artifacts/rule-coherence/cache.json \
  --out artifacts/rule-coherence/report.json
```

The command exited 4 with complete termination, 16 analyzed rules, no pending
shards, and `canonicalizationReady: false`. The evaluator retrieved one bounded
witness by fingerprint
`edd68cb671885c010f1df9d793e82222e60374918d12f15eb1fa0414f46d951e`:
`COMBAT-TRACE-002` has an `interaction` claim of strength `unknown` because its
opaque algorithm boundary lacks a verified read/write/event footprint. It
correctly distinguished unsupported knowledge from a proved contradiction and
made it blocking only because the request required complete support.

The report cost was 14 candidate pairs, 106 pruned pairs, 30 work units, 14
expensive analyses, and zero cache hits. The evaluator did not misrepresent the
cold miss as a warm-cache proof and asked the smallest next question: can the
algorithm receive a verified assume/guarantee footprint?

## Acceptance judgment

Pass. The cold evaluator chose the strict policy without being given the hidden
diagnosis, reported the exact bounded witness and cost, refused an unqualified
coherence claim, and preserved the read-only boundary.

