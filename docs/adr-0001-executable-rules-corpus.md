---
title: "ADR-0001: Executable Rules Corpus"
category: Engineering
categoryindex: 6
index: 18
description: Use a layered embedded F# DSL as the canonical, executable, explainable, and historically versioned representation of S.I.R. rules.
status: accepted
decision-status: canonical
document-type: architecture-decision
version: "1.0"
last-updated: 2026-08-16
related:
  - docs/executable-rules-corpus-architecture.md
  - docs/fable-client-and-documentation.md
  - docs/simulation-core-architecture.md
  - docs/gameplay-formulas.md
  - docs/interactive-rules-lab.md
---

# ADR-0001: Executable Rules Corpus

S.I.R. will represent its game rules as a layered embedded F# domain-specific
language. Inspectable facts, predicates, formulas, and transitions are typed
data; complex algorithms remain ordinary registered F# with explicit rule
metadata and explanations. The same corpus drives the authoritative .NET
simulation, Fable execution, generated documentation, simulator explanations,
and content-addressed historical replay references.

The first implementation receipt is issue #194 and source-bearing commit
`87931073b13b3c74b2ce9dc4cd4321e9b237760e`. It establishes manifest schema v1
for one combat slice without claiming that unrelated mechanics have migrated
or stabilizing the provisional authoring builders.

The authoring/coherence implementation receipt is issue #193. It adds the
repository-local authoring and checking workflows plus a deterministic,
bounded analyzer; it does not claim opaque algorithms are proved coherent.

## Context

The documentation is intended to be a comprehensive representation of the
game, not a commentary maintained independently from it. Developers and agents
need to move in both directions:

- from a rule or formula to the authoritative F# implementation;
- from an implementation to its intent, rationale, examples, and tests;
- from a simulation outcome to the exact rules and operands that produced it;
  and
- from a historical replay to the rules, rationale, and source revision that
  applied when it was recorded.

The repository already builds F# API documentation, evaluates literate F#,
publishes a Fable client, renders rule tables, and verifies the combined site.
However, descriptive catalog values, narrative formulas, executable laboratory
formulas, and authoritative simulation behavior can still be represented in
different places. Links between those places improve navigation but cannot by
themselves prevent semantic drift.

The rules are authored only by developers and agents. The design therefore
does not need a non-programmer authoring language or a runtime modding language.
All rules may be visible in documentation and explanations. Historical replays
must remain interpretable after the current implementation and documentation
have changed. Rationale should accompany mechanics when doing so is cheap.

## Decision

### The canonical authoring language is F#

Rules are authored through an internal S.I.R. F# library. The library is a
product subsystem, not a general-purpose requirements or rules framework. Its
public abstractions serve the S.I.R. simulation, documentation, replay, and
client boundaries.

The authoring surface uses explicit discriminated unions, records, and
combinators. It does not depend on parsing controlled English, recovering
semantics from arbitrary source text, or inspecting F# quotations.

### The corpus is layered

One universal rule grammar is rejected. A rule declares one of these semantic
kinds:

1. **Fact** — typed game content or a definition.
2. **Predicate** — an inspectable Boolean eligibility or invariant expression.
3. **Formula** — an inspectable typed numerical or piecewise expression.
4. **Transition** — structured preconditions, state effects, phases, and events.
5. **Algorithm** — ordinary F# registered with explicit metadata, evidence, and
   an explanation projection.
6. **Narrative** — intent or rationale that deliberately carries no executable
   semantics and is labelled as such.

Facts, predicates, formulas, and transitions preserve enough structure to be
evaluated, rendered, validated, serialized, and explained. Algorithms retain
the full expressiveness of F# where a closed expression language would be
unnatural, including geometry, pathfinding, search, and scheduling.

### EARS is a prose convention, not execution semantics

Executable rules include a short controlled statement following the useful
clause ordering from the
[Easy Approach to Requirements Syntax](https://alistairmavin.com/ears/):
optional preconditions, an optional trigger, a system subject, and a required
response. The statement communicates intent and is validated structurally.

No executable behavior is generated from that English statement. Typed rule
semantics remain authoritative.

### Datalog is not the simulation language

A relational sub-language may later be introduced for derived facts, content
validation, dependency analysis, or eligibility queries. Datalog is not the
initial corpus foundation and will not encode ordered ticks, mutation,
deterministic random sampling, numerical simulation, or batched consequences.

### Rule identity is stable and machine-readable

Every rule has a globally unique, stable identifier such as
`COMBAT-ENGAGEMENT-001`. The identifier names one enduring game concept and is
never reused. A rule that replaces the concept records an explicit
`Supersedes` relationship; an incompatible change of meaning receives a new
identifier.

Display titles, documentation locations, and F# symbols may change without
changing the rule identifier. Every authoritative event and structured
explanation references rule identifiers rather than prose or source line
numbers.

### The same semantics execute on .NET and through Fable

Inspectable semantics are interpreted by shared F# code. Registered algorithms
are shared F# functions within the portable simulation boundary. Fable compiles
that code for the browser. S.I.R. does not generate or maintain a separate
TypeScript or JavaScript implementation of game semantics.

TypeScript declarations may describe external data boundaries. They do not own
rules.

### Documentation is generated and verified

The rules corpus produces a deterministic manifest consumed by both the static
documentation and web client. Formula notation, parameter tables, dependency
links, examples, rationales, and implementation links are projections of the
registered rule definitions.

Narrative architecture pages may summarize or organize those definitions, but
must link to them instead of copying authoritative formulas or content values.
CI rejects incomplete registrations, dangling rule references, duplicate IDs,
unresolved implementation symbols, and undocumented canonical mechanics.

### Simulation results carry structured explanations

Rule execution emits structured rule applications containing the rule ID,
typed inputs relevant to the result, the result or state effect, and nested
applications where appropriate. The simulator renders this as a navigable
derivation and links every node to the corresponding historical rule page and
pinned implementation source.

Because all S.I.R. rules are visible, the initial design has one complete
explanation representation. It does not introduce separate player-safe and
developer-only rule descriptions.

### Historical replay interpretation is content-addressed

Published rule packages are immutable and addressed by a digest over a
non-recursive canonical package projection: the digest field itself is omitted
from its hash input. Executable identity also includes a deterministic
implementation digest covering registered algorithm artifacts, so changing an
algorithm cannot preserve the old semantic identity merely because its F#
symbol and metadata remain unchanged. A replay records its engine identity,
rule-package digest, and structured explanations. Historical documentation
resolves against that digest, not against the current `main` branch.

A published package referenced by a retained replay is not replaced in place.
An archival replay bundle may embed its rule manifest so it remains
self-describing when disconnected from the publication site.

### Rationale is proportionate

Canonical mechanics, formulas, transitions, and algorithms require a concise
rationale or a reference to a shared rationale. Repetitive content rows may
inherit the rationale of their governing rule. Superseding changes record why
the old rule was replaced.

### Authoring and coherence checking are separate workflows

Rule authoring is a guided edit workflow; coherence checking is a read-only
analysis workflow. Both consume the typed corpus, but the checker never rewrites
rules and the authoring guide never substitutes prose for executable semantics.

Coherence reports are scoped claims, not a single global proof bit. Changed,
dependency-cone, and corpus modes identify what was analyzed. Each finding
states its evidence strength and a reproducible witness. Work exhaustion,
finding truncation, unresolved references, or policy-blocking unknowns fail
closed.

The checker uses declared dependency and transition-footprint indexes to avoid
an all-pairs default. Complete results may be cached by analyzer policy,
selected semantic content, and implementation identity; incomplete results are
not reusable as authorization. Registered algorithms without trusted
assume/guarantee footprints remain visibly unknown.

## Consequences

### Benefits

- The simulation, Fable client, formulas, rule explorer, and generated
  documentation share one semantic source.
- Formula and predicate structure is available for rendering, plotting, unit
  checks, dependency analysis, and result explanations.
- Complex mechanics retain unrestricted F# expressiveness through registered
  algorithms.
- Stable rule IDs allow source navigation and replay explanations to survive
  refactoring.
- Content-addressed manifests allow historical replays to retain their exact
  rule context.
- CI can enforce completeness and traceability rather than relying on authors
  to notice stale prose.

### Costs

- S.I.R. must maintain the internal rule ASTs, interpreters, canonical codecs,
  documentation projections, and validation diagnostics.
- Authors must classify rules and provide metadata, rationale, examples, and
  explanation projections.
- Registered algorithms cannot receive automatic line-by-line explanations;
  they require deliberately structured explanation output.
- Historical manifests and explanation data become durable publication
  artifacts with retention obligations.
- The design must keep the shared evaluator inside the Fable-compatible subset
  and preserve deterministic encoding across runtimes.

### Risks and mitigations

**The DSL becomes a second programming language.** Keep it small and layered;
use ordinary F# algorithms when the structured notation stops improving
documentation or verification.

**The DSL is expressive but unpleasant to author.** Stabilize exact combinator
syntax only after implementing the combat vertical slice. The architecture and
wire contracts are decided; surface syntax remains provisional until measured
against real rules.

**Explanations become too large.** Use canonical typed values, intern repeated
rule IDs, represent nested derivations as DAGs where useful, and allow summary
and full-detail projections without changing semantics.

**A source link survives but points at changed behavior.** Generate historical
links from the package's pinned commit and implementation symbol; never use an
unpinned branch link as historical evidence.

**A prose-only rule is mistaken for executable authority.** Display rule kind
and status everywhere, and prevent `Narrative` rules from satisfying executable
coverage obligations.

## Alternatives considered

### EARS as the complete corpus

Rejected. EARS improves the consistency of textual intent but does not define
precise arithmetic, state transitions, deterministic random sampling, event
ordering, or geometry. It remains useful as the statement convention within an
executable rule.

### Datalog as the complete corpus

Rejected. Relational fixed-point derivation is valuable for some game queries,
but it is a poor universal representation for S.I.R.'s ordered, numeric,
stateful simulation. A constrained relational sub-language remains an optional
future addition.

### External YAML, JSON, or a custom text language

Rejected for initial authoring. Only developers and agents write rules, so an
external syntax would add a parser, diagnostics, type system, editor tooling,
and code-generation boundary without providing an audience benefit. The corpus
still emits JSON as a derived interchange and publication format.

### Arbitrary F# plus documentation links

Rejected as the sole model. Ordinary F# is fully expressive but does not expose
formula structure, rule dependencies, or explanation shape. Links can remain
valid while descriptions and implementations diverge.

### F# quotations as the semantic representation

Rejected as the foundation. Quotations make arbitrary host-language constructs
appear inspectable without guaranteeing stable canonical encoding, meaningful
documentation, or a simple Fable runtime. Explicit ASTs and registrations make
the supported structure honest.

### Generate F#, TypeScript, and JavaScript implementations

Rejected. Multiple generated implementations expand the conformance surface.
Shared F# compiled normally and through Fable is the canonical runtime path;
generated TypeScript is restricted to boundary declarations.

## Implementation sequence

The first implementation is a vertical slice through engagement preparation,
trace probability, armor retention, expected damage, weapon/body parameters,
one explained attack event, manifest generation, web rendering, and a
historical replay fixture. It validates the authoring experience before the DSL
surface is stabilized or the remaining game corpus is migrated.

The complete target architecture and acceptance boundary are specified in
[Executable Rules Corpus Architecture](executable-rules-corpus-architecture.md).
