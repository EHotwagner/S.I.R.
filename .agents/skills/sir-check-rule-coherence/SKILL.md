---
name: sir-check-rule-coherence
description: Independently check whether a S.I.R. rule, change, dependency cone, subsystem, or complete executable corpus is coherent, consistent, reachable, covered, contradiction-free, and safe to canonicalize. Use for read-only audits, pre-completion authoring gates, PR changed-rule checks, corpus qualification, and requests for bounded counterexamples or witnesses.
---

# Check S.I.R. rule coherence

Remain read-only unless the user separately invokes an authoring/repair workflow. The deterministic F# tool analyzes; the skill selects scope and budgets, reads bounded results, and discusses real design judgments.

## Select scope

- `changed`: quickest PR feedback for named changed rules only.
- `cone`: normal pre-completion check; includes transitive dependencies and dependants plus indexed interactions.
- `corpus`: exhaustive structural and indexed cross-corpus qualification within declared bounds.

Read [references/report-contract.md](references/report-contract.md) before interpreting strengths, unknowns, or cache/cancellation facts.

## Run

Use the repository command; never paste the full corpus into model context:

```sh
scripts/sir-rules check --mode cone --rule COMBAT-DAMAGE-001 \
  --cache artifacts/rule-coherence/cache.json \
  --out artifacts/rule-coherence/report.json
```

Use `--max-work N` to lower the deterministic work budget and `--block-unknowns` only when canonicalization policy requires complete support. Unknown/malformed rule IDs, cache corruption, and incomplete changed/cone scope fail closed.

## Inspect bounded output

Read summary facts first: mode, analyzed IDs/count, termination, canonicalization readiness, finding counts by dimension/strength, pending shards, work/candidate/pruning/cache counters. Retrieve only the selected finding or witness by fingerprint with `jq`; do not load every passing claim or raw corpus/solver trace.

For each material finding, report:

- strength and exact bounds;
- involved rule/invariant IDs and dependency reason;
- minimized witness (`fact`, expected, actual);
- cache/invalidation and cost facts;
- whether the result is mechanical, bounded, heuristic, unknown, or a human design judgment;
- the smallest next question, without choosing game design silently.

## Decision rules

- `failed` is a reproducible violation and blocks.
- `unknown` is never a pass; block it only when the requested canonicalization policy requires that unsupported claim.
- `exhaustive-bounded` proves only the stated scope.
- A warm unchanged check must show zero work/expensive analyses and a cache hit.
- Work exhaustion must preserve completed findings, name pending shards, and return a valid partial report.
- Missing/untrusted footprints must surface as findings/unknowns, not silently expand to the whole corpus or prune candidates.

Do not edit rules, restart identical exhausted work, upgrade confidence in prose, or claim a single unqualified “coherent” Boolean.
