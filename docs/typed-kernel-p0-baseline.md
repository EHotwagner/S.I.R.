---
title: Typed protocol kernel P0 baseline
category: Engineering
categoryindex: 6
index: 20
description: Content-addressed S.I.R. corpus evidence and the checked ownership cut for the specification-kernel pilot.
status: accepted
document-type: evidence-report
last-updated: 2026-08-24
---

# Typed protocol kernel P0 baseline

This report implements P0 of the FS-GG typed protocol kernel roadmap. It freezes
the evidence needed to design P1 without changing S.I.R.'s authoritative gameplay
behavior. Standard SDD work item `typed-kernel-p0` owns this slice.

## Reproduce the baseline

Run:

```bash
scripts/capture-typed-kernel-p0.sh --check
scripts/test-typed-kernel-p0.sh
```

The first command regenerates the schema-v1 baseline, checks the existing rules
corpus fixtures, and runs the existing domain .NET/Fable conformance route. The
second command proves that a missing required class, a wrong live-registry kind,
and stale content-addressed evidence each fail with a named diagnostic.

The authored selection is
`tests/fixtures/typed-kernel-p0/selection.json`. Its generated projection is
`tests/fixtures/typed-kernel-p0/baseline.json`. The projection binds nine selected
surfaces and seventeen artifacts to SHA-256, plus the live package implementation,
semantic, and manifest identities. A predicate and a supersession edge are
honestly classified as validation fixtures: neither is promoted into the live
canonical registry by this baseline.

## Current authoring cost

Measurements were taken at S.I.R. `origin/main` commit
`60b3d0f8c0e2608af1615edb2c00a3f5b192ee83` on 2026-08-24. Elapsed values are
observations from this host, not new performance gates.

| Measure | Observed baseline | Reproduction / source |
|---|---:|---|
| Canonical rules | 16 | `jq '.rules | length' tests/fixtures/rules-corpus/v2/manifest.json` |
| Facts / formulas / transitions / algorithms | 2 / 3 / 10 / 1 | group live manifest rules by `.kind` |
| Compact definition region | 50 source lines | `CombatRules.fs` lines 16–65 at the measured commit |
| `metadata` helper uses | 8 | `rg -c 'Metadata = metadata ' src/SIR.Simulation/CombatRules.fs` |
| `transitionRule` helper uses | 9 | `rg -c 'transitionRule "' src/SIR.Simulation/CombatRules.fs` |
| P0 capture without parity | 2.342 s | Bash `time scripts/capture-typed-kernel-p0.sh --check --skip-parity` |
| Complete coherence verifier | 22.434 s | Bash `time scripts/verify-rule-coherence.sh` |

Direct records make every AST field reviewable but repeat metadata and transition
record mechanics. The provisional helpers reduce the 16 nodes to eight general
metadata calls and nine transition-helper calls; they do not provide a semantic
diff or a canonical authoring receipt. Today the reviewable semantic projection is
the generated manifest, while `cmp` reports byte drift rather than a concept-level
change. This is the P1 gap, not a reason to weaken the current byte gate.

The cold `sir-author-rule` forward test in
`readiness/193-rule-authoring-coherence/skill-forward-tests/author-rule.md` recorded
one material question after presenting human and typed proposals. It was a
read-only stop-boundary test, so implementation revisions and end-to-end elapsed
authoring work are unknown. The historical item-193 and item-194 SDD receipts do
not record per-session question or revision counts; those values remain `unknown`
rather than inferred from commits or repair rounds. Available diagnostics include
one cold-restore failure, one non-portable skill-validator path, two environment
bootstrap retries, two player-navigation failures, and independent-review repair
findings recorded in the respective feedback checkpoints. Those are workflow
observations, not authoring-session revision counts.

## Candidate reusable substrate

Only concepts with at least two checked uses are admitted to the P1 candidate set.

| Candidate | Checked use 1 | Checked use 2 | P0 verdict |
|---|---|---|---|
| Stable identity | `RuleId` and unique live registry IDs | SDD `workId` plus task/requirement IDs | Admit |
| Schema version | rule manifest/coverage schema v1 | SDD artifact and evidence schema v1 | Admit |
| Typed references | rule dependencies and supersession fixtures | SDD source IDs and dependency-ordered tasks | Admit |
| Provenance | rule `SourceRef` and package source commit | SDD source snapshots with exact digests | Admit |
| Evidence obligations | rule examples/properties/evidence | SDD verification obligations and observed-run receipts | Admit |
| Deterministic normalization | canonical rule/manifest/application bytes | generated SDD work model and source snapshots | Admit |
| Fingerprints | implementation/semantic/manifest digests | SDD SHA-256 source and evidence bindings | Admit |
| Human projection | generated rule manifest/coverage | generated SDD guidance/work model | Admit |

Vocabulary and typed extension registration remain provisional. The corpus and SDD
both use vocabularies, but P0 has not proved a common vocabulary ownership contract.
Likewise, two domain variants are not evidence for a reusable open-extension
registration mechanism. P1 may test these ideas; P0 does not admit them as solved.

## S.I.R.-owned, non-transferable semantics

The following remain explicitly outside a shared specification substrate:

- `FixedPoint`, rule value/unit meanings, `FormulaExpr`, and formula evaluation;
- gameplay rule kinds and the combat taxonomy;
- `FS.GG.Game.Core.Los.lineOfSightBy` and every registered algorithm implementation;
- attack, consequence, cover-impact, suppression-recovery, and replay interpreters;
- combat state reads/effects/events and balance constants; and
- S.I.R. coherence policy strengths, candidate-pair indexing, and canonicalization policy.

A later shared kernel may carry stable IDs, provenance, evidence, fingerprints, and
extension envelopes around these values. It must never acquire their gameplay
meaning or execute their interpreters.

## P1 handoff

P1 may now compare direct records, a computation expression, and a hybrid surface
against the same content-addressed selection. Its minimum slice must add a semantic
diff and authoring receipt while keeping the existing package identities, generated
manifest, application bytes, replay binding, coherence report, and full .NET/Fable
vector unchanged unless a separately accepted versioned change says otherwise.
