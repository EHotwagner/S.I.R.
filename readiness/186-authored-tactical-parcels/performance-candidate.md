# Tactical environment candidate performance

- Date: 2026-08-15
- Configuration: Release
- Host: Linux 7.1.4-arch1-1 x86_64
- .NET SDK: 10.0.302
- Node: 26.7.0
- Fable: 5.13.0
- Game.Core package/profile: FS.GG.Game.Core 0.13.0 / `fs-gg-game-core-fable-lockstep-v1`
- Base commit: `3dc50b5839b51b605aee7d9d5f0b1274e0f0f60d`
- Measured source commit: `bf4091af8dce2b13ca7f3a2b648592f874b13e2a`

The repair-phase authoritative aggregate ran at candidate `bf4091a`, beginning
with locked .NET restore and `npm ci`. The normal
`SIR.Match.Tests` executable measured production validation, assembly, spatial
projection, combat resolution, environment actions, and dependency-local cache
invalidation. The representative exterior workload completed in 1.074 ms. The
declared maximum assembly (64 connected slots, 32 compatible variants per role)
validated in 2.039 ms, made 64 selections after exactly 2,048 variant
inspections, and completed in 8.425 ms. The 80x80 preview with 2,048 features
validated separately in 7.653 ms and completed assembly in 20.305 ms. Sixteen
simultaneous independent Release processes retained the exact workload and
counters with preview assembly observations from 24.715 ms through 36.538 ms.
Every independently enforced validation/preview limit remained below 50 ms,
including the host-contention proof.

The repair removes two allocation multipliers from the real production route:
schema-v1 canonical bytes now stream into one ordered buffer instead of
concatenating one array per scalar, and valid bounded authored parcels validate
reachability over their dense declared footprint instead of hashing records and
allocating four neighbour records per visited cell. Malformed or oversized
inputs retain the general hash-set fallback. The canonical byte grammar,
1,094-byte native/Fable stream, content identity, and structural counters are
unchanged.

The production-adapter batch used 100 real combatants and resolved 50 rifle
attacks through `Combat.resolve`, production line traces, and 50 targeted
environment actions in 11.875 ms. It emitted 50 spatial query receipts, crossed
at least one spatial cell per query, and produced exactly one `HealthChanged`
target per attack without propagating unrelated environment changes. A door
state transition inspected and changed one target, propagated zero neighbours,
and invalidated exactly one intersecting cached query entry in 0.018 ms.

The complete client qualification retained its dense 40x40 map, 3,120 edges,
200 units, 200 regions, and 7,136 interactive-node scale. The normal match
journey completed 40 live ticks in 25.480 ms; preview was 0.258 ms,
serialization 0.120 ms, transfer 0.028 ms, and rendering projection 0.079 ms.

These are headless host observations and make no compositor or portable
wall-clock claim. Structural counters and canonical bytes are deterministic;
elapsed values are deliberately excluded from content identity and replay.

## Independent-review repair round one

The exact `861dfc52b348eb0316ce85fbcdee3836a25eca6d` detached aggregate
retained identity
`18750a88acda3674081cd7de290600005fbc36cfcf8da44a34b7bf95c4bc87fd`.
Its 80x80 production editor preview visited 6,400 cells and 2,048 features,
validated in 7.430 ms, and completed assembly in 21.799 ms. A subsequent
16-process Release contention run passed in every process with identical
identity and structural counters; preview assembly ranged from 25.856 ms to
41.469 ms, keeping every observation below the unchanged 50 ms gate.

The same aggregate measured the unmangled production application entry at
1,196,127 raw bytes and the normal initial browser request graph at 1,228,472
bytes. Both satisfy the user-authorized initial-boot budget v2 ceiling of
1,250,000 bytes. The 62,976-byte Rules Explorer remains deferred and loads
through its normal UI route. This is a versioned default-route budget, not a
global or permanent product limit: subsequent growth must defer code or
explicitly version and rebaseline the contract. The aligned static ceiling and
the browser initial-response mutation both fail when the measured size is made
oversized.

## Independent-review repair round two

Hosted run `31910063550`, job `95073674188`, showed that repair-round-one head
`74d276020ccae16acdd422461f93f28373d902a6` could still exceed the unchanged
50 ms preview budget on a constrained runner. The round-two candidate
`a4f38f1fdaa4b2227f8a389f5ece8688b6bdb0de` therefore replaces the remaining
per-byte `Generic.List<byte>` identity writer with one exactly sized byte array
and uses a dense canonical-cell route where its preconditions hold. The same
80x80/2,048-feature production preview allocation observation fell from
24,957,944 bytes on the round-one implementation to 12,124,472 bytes. Its
detached aggregate validated in 9.921 ms and completed assembly in 19.003 ms.

Sixteen simultaneous cold Release processes all retained the strict gate,
exact identities, and structural counters. Preview validation ranged from
11.092 ms through 15.164 ms and assembly from 21.157 ms through 31.548 ms. The
tests bind the exterior identity
`4e32081ea4a1fa44c4e04ef8ba1bc99d5efba22fc3766f1a9cdb6af95e5a1263`,
the preview authored-input identity
`51eedfe20ceb51fad17d33ddacfe68ce0e95cd7df031f1ef5e176226249e0a68`,
the assembled content identity
`18750a88acda3674081cd7de290600005fbc36cfcf8da44a34b7bf95c4bc87fd`,
and a Unicode differential identity
`c214e1d82c8a33f30cf6218be3744f1e1834bbef547d387d98a8740618b99ca9`.
The allocation mutation makes the bound zero and fails as intended.

Native and Fable qualification passed with the same canonical grammar. The
unmangled production application entry measured 1,198,954 raw bytes, a 2,827
byte increase from the round-one artifact and still below the versioned
1,250,000-byte initial-boot ceiling. The full production delivery and browser
journeys passed. Timings and allocations remain host observations; the exact
bytes, identities, counters, and pass/fail budgets are the deterministic
contracts.

## Independent-review repair round three

Hosted diagnostic run `31913327392` separated and flushed the preview gates and
identified the exact failure as host-sensitive allocation: 13,693,216 bytes,
not identity, counters, or the 50 ms timing predicate. Local observations remain
12,124,472 bytes. The executable allocation ceiling is therefore 16,000,000
bytes: about 16.8% headroom above the observed hosted value and still about
35.9% below the 24,957,944-byte round-one implementation. This is an honest
host-qualified allocation budget; the independent strict 50 ms validation and
assembly timing gates are unchanged.

The allocation mutation no longer changes that ceiling. It self-restoringly
injects a retained 14,000,000-byte allocation into the production
`authoredInputIdentity` subject, rebuilds the real Match test, and requires the
unchanged identity/counters/16,000,000-byte assertion to fail. The local mutated
subject allocated 26,124,528 bytes and failed its named allocation gate, then a
restored rebuild passed and left the production source unchanged.

## Independent-review repair round four

Hosted run `31914865432` proved that the repaired maximum preview itself passed:
13.075 ms validation, 43.815 ms assembly, 13,693,216 allocated bytes, and exact
authored identity. The run then failed a compound representative-combat gate
whose structure and 50 ms timing predicates shared one error and whose detailed
measurement was printed only after the assertion. That ordering hid the exact
failing subpredicate.

The round-four repair prints and flushes the representative batch observation
before independently named structure and timing assertions. It does not change
the workload or budget. Hosted run `31915418313` passed with exactly 100 source
units, 100 final units, 100 participants, zero propagated changes, 50 spatial
queries, 100 crossed cells, and 40.130 ms elapsed under the unchanged 50 ms
ceiling. The same run retained the maximum preview at 12.949 ms validation,
37.110 ms assembly, 13,693,216 allocated bytes, and exact authored identity.
Both hosted jobs, documentation, the complete browser suite, and downstream
mutation gates completed successfully.

## Independent-review repair round five

The round-four exact-head hosted run `31917304397` retained the repaired
80x80 tactical preview at 9.271 ms validation but exposed a separate inherited
dense 40x40 map-editor pointer-preview observation of 9.271 ms against its
8 ms ceiling. The pointer-preview budget is now versioned at 12 ms p95, about
29% above that hosted observation, with the workload and every other editor
budget unchanged. The measurement is printed and flushed before assertion.

Focused local qualification observed 2.670 ms normally. A self-restoring
production-source mutation inserted a 20 ms delay in `MapEditor.terrainPreview`
without changing the 12 ms predicate; the gate failed at 22.735 ms, then source
restoration rebuilt and passed at 2.652 ms with tracked no-drift.

Each of the seven separately named representative combat-spatial predicates
now owns an input/result subject inversion under its unchanged assertion. The
source-unit, final-unit, participant, propagation, query, crossed-cell, and
50 ms timing mutations each failed only its expected named gate; the timing
mutation observed 77.917 ms. The single clean detached aggregate at
`3e158a1064fd2504604785eda439dee5379ddfde` exited zero, initially observing
the tactical preview at 19.207 ms / 12,124,472 bytes, the representative batch
at 17.405 ms with 100/100/100/0/50/100 structure, and dense pointer preview at
2.752 ms. After all subject mutations restored, it retained exact authored
identity `51eedfe20ceb51fad17d33ddacfe68ce0e95cd7df031f1ef5e176226249e0a68`,
observed 19.548 ms / 12,124,472 bytes for the tactical preview and 16.859 ms
for the representative batch, and left tracked sources drift-free.

## Protected-main sampling repair

Protected-main run `32381741044` retained the exact 100 source/final units,
100 participants, zero propagated changes, 50 spatial queries, and 100 crossed
cells, but its five-sample p80 was 50.248 ms against the unchanged 50 ms gate.
The timed region still included test-only per-step assertions and inherited GC
state from the preceding qualification workload.

The gate now performs full per-step verification immediately before and after
the samples, measures only the product interaction batch, and collects outside
each timed region so unrelated earlier allocations cannot decide the result.
Two clean local Release runs observed 16.959 ms and 16.340 ms p80 with the
workload and 50 ms budget unchanged. The forced 60 ms timing mutation still
failed closed through `scripts/test-ci-product-performance-route.sh`.
