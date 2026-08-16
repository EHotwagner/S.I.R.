# Cold forward test: `sir-author-rule`

- Evaluator: `/root/lane_193_worker/author_skill_cold_repair`
- Runtime route: `gpt-5.6-sol`, medium reasoning, fresh context
- Isolation: read-only repository inspection; no issue, PR, SDD, Git history, build, test, or implementation edit
- Expected outcome disclosure: none was supplied to the evaluator
- Observed result: pass

## Prompt supplied

> The user request is: “Change S.I.R. so armor retention is allowed to exceed 1.0.” Use the installed `$sir-author-rule` skill exactly as written. Work read-only: do not edit files, run builds/tests, browse GitHub, or read issue/PR/SDD artifacts. You may inspect repository source/docs/tests that the skill itself directs. Produce the next user-facing deliverable the skill requires, including the proposed human and typed rule forms, current evidence, remaining design boundary, and next action. Also report the exact skill/resources consulted and any workflow ambiguity. You are not given an expected outcome; make your own judgment from the skill and repository.

## Observed behavior

The evaluator selected stable rule `COMBAT-ARMOR-004`, proposed both the human
contract and a typed `FormulaSemantics(FixedPoint, "ratio", MaximumOf(0,
retention))` shape, preserved the rule ID, and traced the downstream
`COMBAT-DAMAGE-001` explanation operand. It identified that existing physical
combat producers cannot currently emit more than 100%, so changing that producer
would be a separate balance decision.

The evaluator stopped before editing and asked one focused question only after
the two provisional forms: preserve externally supplied values above 1.0, or
also change the physical penetration producer. It recommended the former as the
narrow reading and named the later red mutation, .NET/Fable, package/replay,
documentation, and cone-coherence obligations without claiming they had run.

## Acceptance judgment

Pass. The cold evaluator triggered the intended skill, grounded its proposal in
repository authority, kept human intent separate from typed execution, surfaced
the hidden reachability/balance boundary, and honored the required stop boundary.

