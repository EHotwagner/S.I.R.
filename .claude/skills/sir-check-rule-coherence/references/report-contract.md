# Coherence report v1

`scripts/sir-rules check` emits deterministic JSON with:

- identity: `schemaVersion`, `analyzerVersion`, `mode`, `packageManifestDigest`, `analyzedRuleIds`;
- result: ordered `findings`, `pendingShards`, `termination`, policy-derived `canonicalizationReady`;
- cost: corpus/slice rules, candidate/pruned pairs, work units, expensive analyses, cache hits;
- reusable cache identity bound to analyzer, package/source profile, selected normalized semantics and registered implementation fingerprints, mode, scope, bounds, and policy.

Finding strengths are `proved-structural`, `proved-fragment`, `exhaustive-bounded`, `tested`, `heuristic`, `unknown`, and `failed`. Each finding has a stable fingerprint, dimension, involved rule IDs, message, dependency reason, and optional minimized witness.

Exit status:

- `0`: complete with no failed claim and no policy-blocking unknown;
- `3`: at least one failed claim;
- `4`: incomplete work or a policy-blocking unknown;
- `2`: invalid input or unreadable/malformed cache.

The first analyzer slice completely checks the structural relations it implements and relationally selects transition interactions from typed phase/read/write/event facts. Registered algorithms without trusted interaction summaries remain explicit `unknown`; no bounded result is promoted into an unbounded proof.
