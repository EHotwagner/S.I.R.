# Quint Q1 S.I.R. runtime replay

This fixture is S.I.R.'s accept-or-refuse response to
`FS-GG/FS.GG.SDD#922`. It accepts only the producer candidate at commit
`3a0eced13305b146df2febd96698e38335cae99c`, sealed by producer receipt
commit `6cf3f1f0746c817e1171cd3a7b63865c25c1e346` and manifest SHA-256
`0bb0a34f6e93c933b441fd34ebc0bbd521ac792b95955d49ed528cab3c014ddd`.

## Correspondence boundary

The test-only adapter maps the stable model actions `Initialize` and
`ApplyDamage(amount)` to the production `CombatRules.resolveConsequences`
interpreter. It does not reimplement damage or clamping. After each transition,
the replay compares the exact declared observable projection:

- `hitPoints`: the production interpreter's resulting health;
- `lastAction`: the stable action identity from the replayed witness;
- `lastAmount`: `0` for initialization and the requested damage otherwise.

The checked trace is `Initialize; ApplyDamage(3); ApplyDamage(20)`, producing
three states. Normalization is an exact `int32`/string projection with immediate
deterministic quiescence. The envelope and normalized ITF forms must agree before
the production replay begins.

## Selection and refusal

Changes to the combat implementation, adapter, fixture, or qualification script
select this suite. An unrelated client path skips it. The harness refuses SDK,
producer binding, envelope/ITF, digest, order, state, or determinism drift and
reports the first divergent transition.

Five independent mutations must fail: wrong action mapping, omitted action,
wrong observable field, stale expected state, and a combat-boundary defect. The
last mutation wraps the production call, invokes the real interpreter, and
corrupts its returned `CombatConsequences` before adapter projection. It cannot
be satisfied by a post-projection mutation and still contains no copied damage
or clamp implementation. Every failure names the fixture, JSON pointer,
transition, adapter source, and implementation source.

Run the qualification with the repository-pinned .NET 10.0.302 muxer:

```bash
DOTNET_BIN=/path/to/dotnet \
  scripts/qualify-quint-q1-sir-replay.sh
```

On a selected CI route, the exact producer-generated `sir-damage.qnt` runs under
fingerprinted Quint 0.32.0 and evaluator 0.6.0 to generate 64 normalized ITF
traces with seed 92220 and an eight-step bound. The native adapter replays all
576 states through the real interpreter. A change to an unrelated client path
records a skip from the live route rather than running this corpus.

The runtime receipt binds the executed .NET muxer, SDK entry point, hostfxr,
runtime closure, and package locks by digest. Timing is descriptive, not a pass
threshold. Machine-readable runtime, sampled-corpus, and JUnit receipts are stored under
`readiness/353-quint-q1-sir-replay/`.
