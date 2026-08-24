# P3 package-bound authoring session

- Rule: `COMBAT-DAMAGE-001`
- Intent: re-run the governed authoring route after replacing the P1 local shared substrate with the published kernel.
- Package authority: exact public `FS.GG.SDD.Artifacts 1.3.0-preview.3` pin.
- Consumer authority: S.I.R.-owned `RuleSpecificationAst`, compiler adapter, canonical `RuleDefinition`, registry, interpreter, and projection.

## Provisional human form

Expected damage remains the weapon effect multiplied once by trace probability and retained armor effect. The P3 migration changes only the shared envelope/compiler boundary; it does not change gameplay semantics, canonical rule bytes, replay identity, explanation operands, or generated human meaning.

## Provisional typed form

Keep stable specification identity `COMBAT-DAMAGE-001`, schema version `1`, existing source provenance, and the S.I.R. formula AST. Construct the package-owned `SpecificationModel<RuleSpecificationAst>` through `RuleSpecification.hybrid`, with the existing reads/writes and evidence obligations, then compile to the sole runtime `RuleDefinition`.

## Material choice and decision

Question: should re-adoption change the selected authoring surface while changing the package boundary?

Decision: no. Retain the hybrid surface because P1 selected it for readable rule authoring, while direct-record and computation-expression forms remain equivalence controls. Changing syntax and ownership in one migration would obscure semantic drift without adding capability.

## Executable acceptance

`work/typed-kernel-p3/generate-evidence.sh` validates this governed skill and reference, refuses the deleted local shared-kernel path, restores the public package, executes direct/hybrid/computation-expression normalization and compilation checks through domain conformance, verifies the frozen corpus and generated projection, runs cone coherence for this rule, and emits `readiness/typed-kernel-p3/agent-authoring-session.json`.
