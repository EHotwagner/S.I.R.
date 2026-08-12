# Rule application and replay v1 contract

A `RuleApplication` has a stable application ID, rule ID, decisive typed operands in declared order, typed outcome/effects, ordered child application IDs, authoritative event identity, phase/order identity, and complete rule-package identity. Summary, calculation, and engineering renderings project this record without changing it.

Replay format v3 extends the existing versioned envelope with Game.Core compatibility profile/package identity, rule implementation/semantic/manifest digests, pinned source commit, and an embedded canonical schema-v1 manifest preimage plus canonical rule applications. The decoder reconstructs typed rules from that bounded payload, requires an exact canonical round trip, recomputes the manifest digest from the archived identity and payload, and rejects any application bound to another manifest. The archive is therefore sufficient to render historical rule titles, formulas, rationale, dependencies, and pinned source without consulting current or external content. Existing v1/v2 readers remain byte-compatible.

Historical resolution is exact:

1. validate the replay-declared identity against embedded or retained bytes;
2. render explanation/rule/source links using that package and its pinned commit;
3. never fall back to the current package; and
4. return `HistoricalRulePackageUnavailable` with the requested manifest digest when exact bytes are absent.

`tests/fixtures/rules-corpus/v1` retains the first package, replay, canonical oracle, package SHA-256, source commit, and toolchain/profile manifest. Referenced supported fixtures are immutable.
