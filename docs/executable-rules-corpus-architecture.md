---
title: Executable Rules Corpus Architecture
category: Engineering
categoryindex: 6
index: 19
description: Architecture for making the S.I.R. web client, simulation, documentation, source, and historical replays projections of one typed F# rules corpus.
status: accepted
decision-status: canonical
implementation-status: planned
document-type: living-architecture
version: "1.0"
last-updated: 2026-08-12
related:
  - docs/adr-0001-executable-rules-corpus.md
  - docs/codebase-architecture.md
  - docs/fable-client-and-documentation.md
  - docs/simulation-core-architecture.md
  - docs/gameplay-reference.md
  - docs/gameplay-formulas.md
  - docs/interactive-rules-lab.md
  - docs/simulator-worker-protocol.md
---

# Executable Rules Corpus Architecture

The S.I.R. rules corpus is a typed F# product subsystem from which simulation
semantics, Fable execution, reference documentation, interactive explanations,
source navigation, and historical replay context are projected. It uses
inspectable representations where they produce trustworthy formulas and
derivations, and registered ordinary F# where game mechanics need unrestricted
algorithmic expression. This page defines the target architecture; it does not
claim that the current rules catalog and simulation have already migrated.

## Goals

The completed system must make these statements true:

1. The documented corpus represents every authoritative game rule and clearly
   distinguishes accepted, proposed, prototype, and superseded material.
2. A formula or parameter displayed in the web client is derived from the same
   typed rule definition evaluated by the simulation.
3. A developer can navigate from a rule to its F# implementation, examples,
   properties, dependants, and rationale.
4. A developer can navigate from a simulation event to the exact rules,
   operands, and intermediate results that produced it.
5. A historical replay resolves to the immutable rule corpus, explanation
   vocabulary, and pinned source revision used when it was recorded.
6. The authoritative behavior is implemented once in shared F#, executing on
   .NET and through Fable rather than being reproduced in TypeScript or
   JavaScript.
7. CI fails when executable behavior and the declared corpus lose coverage or
   traceability.

## Non-goals

The initial subsystem is not:

- a runtime modding language;
- a non-programmer rule editor;
- a reusable FS.GG or general requirements framework;
- a parser that turns controlled English into executable behavior;
- a Datalog implementation of the whole simulation;
- a mechanism for introspecting arbitrary F# into perfect explanations;
- a second authoritative TypeScript or JavaScript simulation; or
- a promise that every design note is executable.

## Architectural position

The corpus sits between domain vocabulary and the deterministic simulation. It
is consumed by build-time and runtime projections without depending on a UI or
transport.

```text
SIR.Domain values, identifiers, units, canonical codecs
                         │
                         ▼
              S.I.R. rules corpus
        facts · predicates · formulas
       transitions · algorithms · prose
          │              │             │
          ▼              ▼             ▼
 authoritative       manifest       evidence and
 simulation          generator      validation
          │              │             │
          ├──────┬───────┴──────┬──────┘
          ▼      ▼              ▼
       .NET    Fable          fsdocs and
       kernel  browser        web rule explorer
          │      │              │
          └──────┴──────┬───────┘
                        ▼
             versioned replay explanation
```

The rules subsystem may depend on `SIR.Domain`. `SIR.Simulation` depends on the
rules subsystem. Client and documentation projections consume stable manifest
and explanation contracts. The rules subsystem must not depend on browser,
React, networking, filesystem, wall-clock, locale, or process-global random
state.

The exact project split is an implementation decision for the vertical slice.
If the corpus grows into a separate project, it remains named and scoped as a
S.I.R. subsystem. The architecture does not introduce an independent framework.

## Rule model

### Identity

A rule ID is a stable, opaque identifier with a readable taxonomy:

```text
<AREA>-<CONCEPT>-<SEQUENCE>

COMBAT-ENGAGEMENT-001
COMBAT-ARMOR-004
OBSERVATION-SOLUTION-003
MOVEMENT-PATH-004
```

The readable prefix supports navigation but does not encode behavior. IDs are
case-sensitive canonical strings, are never reused, and are validated for
uniqueness over the complete corpus.

A rule ID continues across compatible clarification and tuning. A change that
replaces the concept or invalidates the old meaning creates a new ID and an
explicit supersession edge. Revisions within a historical package are
identified by canonical content hashes rather than mutable version labels.

### Common metadata

The conceptual common record is:

```fsharp
type RuleStatus =
    | Proposed
    | Prototype
    | Canonical
    | Deprecated
    | Superseded

type RuleMetadata =
    { Id: RuleId
      Title: string
      Status: RuleStatus
      Statement: ControlledStatement
      Rationale: RationaleRef
      Tags: RuleTag list
      RelatedRules: RuleId list
      Supersedes: RuleId list
      Source: SourceRef
      Evidence: EvidenceRef list }
```

The exact F# names may change during the first slice. The obligations may not:

- stable identity;
- explicit status and semantic kind;
- controlled intent statement;
- concise rationale or shared-rationale reference;
- typed semantics or explicit narrative-only classification;
- related and superseded rule references;
- implementation source identity where executable;
- executable examples/properties or an applicable evidence reference.

### Controlled statements

The human-readable statement follows an EARS-like ordered structure without
being parsed as code:

```fsharp
type ControlledStatement =
    { Preconditions: Clause list
      Trigger: Clause option
      System: SystemSubject
      Responses: Clause list }
```

This supports ubiquitous, state-driven, event-driven, unwanted-behavior, and
optional-feature phrasings while preserving machine-checkable clause roles.
Rendered prose may read:

> While the attacker maintains a valid targeting solution, when engagement
> preparation completes, the simulation shall resolve the declared attack.

The statement explains expected behavior. A predicate, formula, transition, or
algorithm registration defines it.

### Rationale

Rationale is part of the corpus because it explains the intended player-facing
effect and prevents later agents from “simplifying” away deliberate behavior.
It has two economical forms:

```fsharp
type RationaleRef =
    | Inline of string
    | Shared of RationaleId
```

Canonical mechanics, formulas, transitions, and algorithms require rationale.
Content facts may refer to the rule governing the table or parameter family.
Superseding rules additionally state why replacement was necessary.

## Semantic kinds

### Facts

Facts represent typed definitions and game content:

```fsharp
fact {
    id "CONTENT-WEAPON-RIFLE-001"
    value Rifle
    governedBy "COMBAT-WEAPON-PROFILE-001"
    rationale (shared "RATIONALE-WEAPON-ROLES-001")
}
```

Facts must not use presentation strings as their authoritative numerical
representation. Display formatting belongs to a manifest or UI projection.

Appropriate uses include:

- weapon and body profiles;
- ability and equipment definitions;
- categorical relationships;
- bounded parameters;
- unit-aware constants; and
- canonical vocabulary definitions.

### Predicates

Predicates are closed, typed Boolean expression trees. They express invariants,
eligibility, guards, and other conditions whose structure is useful to render
and explain.

```fsharp
predicate {
    all [
        call "OBSERVATION-SOLUTION-003" targetingSolutionMaintained
        lessThanOrEqual range effectiveEngagementLimit
        not' attackerIsIncapacitated
    ]
}
```

The implementation may use generic typed authoring combinators, but the erased
canonical representation must retain stable input IDs, operator IDs, units,
and referenced rule IDs.

### Formulas

Formulas are closed typed expression trees interpreted by shared F#. The
minimum useful operator vocabulary includes:

- constants and typed inputs;
- addition, subtraction, multiplication, and division;
- minimum, maximum, absolute value, clamping, and square root;
- integer powers and explicitly supported general powers;
- comparisons and piecewise branches;
- calls to other registered formulas; and
- deterministic distribution descriptions where randomness is part of a
  formula rather than an algorithm.

Illustrative authoring syntax for engagement preparation is:

```fsharp
formula {
    id "COMBAT-ENGAGEMENT-001"
    input "range" Metres
    input "exposure" Ratio
    input "existing-suppression" SuppressionPoints

    result Seconds

    expression
        ((baseTime + rangeSlope * pow range exponent)
         / sqrt (max exposureFloor exposure)
         * suppressionMultiplier)
}
```

This syntax is intentionally provisional. The formula AST, canonical encoding,
unit obligations, dependency edges, and single-evaluator rule are architectural
requirements; computation-expression punctuation is not.

Formula evaluation produces both the value and an optional structured
derivation. The renderer uses the same tree to produce accessible prose,
mathematical notation, charts, parameter controls, and dependency links.

### Transitions

Transitions describe stateful rule structure without attempting to replace F#
as a programming language. Their inspectable vocabulary covers:

- named preconditions and guards;
- authoritative phase and ordering identity;
- declared state reads;
- state effects expressed as registered operations;
- event emission;
- deterministic random-sample requests with named purposes;
- cancellation, suspension, and recovery outcomes;
- batched or simultaneous consequence groups; and
- calls to predicates, formulas, transitions, and algorithms.

Illustrative syntax is:

```fsharp
transition {
    id "COMBAT-ATTACK-RESOLUTION-001"
    phase AttackPhase

    require "OBSERVATION-SOLUTION-003" targetingSolutionMaintained
    calculate "COMBAT-TRACE-002" traceOutcome
    calculate "COMBAT-ARMOR-004" retainedEffect
    update targetHealth
    emit AttackResolved
}
```

Registered state operations remain typed F# functions. The transition
definition exposes sequencing and rule calls for documentation and explanation
without requiring every collection traversal or update operation to become a
new AST node.

### Algorithms

Algorithms are the full-expressiveness escape hatch:

```fsharp
algorithmRule {
    id "MOVEMENT-PATH-004"
    implementation Pathfinding.findPath
    explain Pathfinding.explain
    examples Pathfinding.examples
    properties Pathfinding.properties
}
```

Typical algorithm rules include:

- pathfinding and spatial search;
- line, polygon, and volume intersection;
- visibility-region construction;
- collision and reservation resolution;
- scheduling and optimization; and
- procedural generation.

An algorithm registration must declare the authoritative source symbol,
rule-call dependencies that cannot be discovered mechanically, examples or
properties, and an explanation projection. The explanation may summarize an
algorithm rather than expose every internal operation, but it must identify the
decisive inputs and result.

### Narrative rules

Narrative rules preserve definitions, constraints on future design, or
rationale that intentionally has no executable representation. They are
visible and linkable but cannot satisfy coverage for an executable obligation.
The UI always displays their kind to avoid confusing them with implemented
authority.

## Type and unit discipline

The authoring API uses domain-specific types wherever practical, such as
ticks, distances, ratios, bounded health, suppression, penetration, and damage.
F# units of measure may improve authoring, but canonical manifests cannot rely
on erased compiler metadata. Every serialized input, output, and constant has a
stable value-kind and unit identifier.

The formula registry validates:

- compatible operands for arithmetic and comparison;
- declared result units;
- bounded-domain requirements;
- conversions as explicit named operations;
- total piecewise definitions or an explicit failure outcome;
- finite-value behavior where the domain requires it; and
- deterministic numeric representation across .NET and Fable.

Floating-point arithmetic must not silently enter the authoritative lockstep
boundary. Existing fixed-point and bounded domain values remain authoritative
where required. Prototype laboratories may use floating point only when their
status and non-authoritative boundary are explicit.

## Runtime execution

### One F# semantic implementation

The runtime path is:

```text
typed F# rule definitions
          │
          ├─ compiled by .NET → server/tests/tools
          └─ compiled by Fable → browser worker/client
```

The web client does not translate manifest formulas into an independent
JavaScript implementation when the same shared evaluator can be compiled by
Fable. The manifest may support visualization and offline inspection, but it
does not create a second semantic authority.

If TypeScript consumers require declarations for manifests or explanations,
those declarations are generated from the stable boundary schema and verified
against canonical fixtures.

### Determinism

Rule evaluation inherits the deterministic simulation constraints:

- no process clock, locale, filesystem, network, or environment input;
- no process-global random source;
- named and replayable random-sample purposes;
- canonical ordering for sets, maps, dependencies, and emitted explanations;
- canonical numeric and text encoding;
- identical checked failure behavior across .NET and Fable; and
- conformance fixtures that compare values, events, explanations, and digests.

### Rule application identity

Runtime execution reports applications of registered rules:

```fsharp
type RuleApplication =
    { RuleId: RuleId
      Inputs: ExplanationField list
      Outcome: ExplanationOutcome
      Children: RuleApplicationId list }
```

The actual representation may intern rule IDs and use a DAG to avoid repeated
subtrees. Its canonical semantics must preserve:

- which rule was applied;
- which disclosed operands were decisive;
- the result, decision, or state effect;
- nested rule applications in deterministic order; and
- a stable link to the event or phase the application explains.

## Explanation architecture

### Event-to-rule navigation

Authoritative or review-facing simulation events carry an explanation root or
an explicit reason why no rule application is applicable. A rendered attack
might be:

```text
Attack resolved: 22 damage
├─ COMBAT-ENGAGEMENT-001 · preparation completed
├─ COMBAT-TRACE-002 · trace connected
├─ COMBAT-COVER-003 · target was 65% exposed
├─ COMBAT-ARMOR-004 · impact was partially mitigated
└─ COMBAT-DAMAGE-001 · retained effect × base damage = 22
```

Each node links to:

- the rule statement and status;
- the formula, predicate, transition, or algorithm summary;
- the actual typed inputs and result;
- rationale;
- related and dependent rules;
- examples and properties; and
- the pinned F# implementation source.

### Summary and detail

One canonical explanation can have multiple presentation depths:

- **summary** — decisive rule IDs and outcomes;
- **calculation** — operands, intermediate values, and formula branches; and
- **engineering** — source, package identity, evidence, phase, and canonical
  encoding details.

Presentation depth does not change the underlying result or hide rules. All
rules are visible by product decision.

### Explanation storage

Historical explanations cannot be reconstructed safely by running current code
against old events. The replay therefore retains the canonical structured
explanation that was produced during execution, or an equivalent deterministic
application stream from which it is reconstructed using the pinned historical
package.

Repeated rule IDs, field names, and shared sub-derivations may be interned.
Size budgets and compression are measured during the first slice rather than
addressed by discarding provenance.

## Manifest and historical package

### Package identity

A rules package has at least:

```fsharp
type RulePackageIdentity =
    { SchemaVersion: int
      EngineIdentity: string
      SourceCommit: string
      ImplementationDigest: CanonicalHash
      SemanticDigest: CanonicalHash
      ManifestDigest: CanonicalHash }
```

`ImplementationDigest` fingerprints the deterministic runtime-artifact set
containing registered algorithms, including its runtime profile and relevant
toolchain identity. Every algorithm registration maps to a component covered
by this digest. Changing compiled algorithm behavior therefore changes package
semantic identity even when the algorithm keeps the same F# symbol,
dependencies, and registration metadata.

`SemanticDigest` identifies executable semantics and authoritative content. It
is computed from the canonical semantic projection of facts, predicates,
formulas, transitions, and algorithm registrations together with
`ImplementationDigest`. It may conservatively change when a runtime artifact
changes without changing observable behavior; it must never remain unchanged
when registered executable behavior changes.

`ManifestDigest` identifies the complete published representation, including
statements, rationale, source references, and documentation metadata. Its
canonical hash input includes the package metadata, manifest payload,
`ImplementationDigest`, and `SemanticDigest`, but omits `ManifestDigest` itself.
The digest therefore does not recursively contain the value being computed.
The published package is an envelope containing the computed identity and the
hashed manifest payload.

This separation allows diagnostics to say whether a change affects registered
runtime artifacts, inspectable execution semantics, explanation only, or more
than one of those surfaces.

All canonical digest projections exclude volatile build timestamps and
absolute paths. The implementation-artifact set is ordered and encoded
canonically. Package identity is reproducible from the same repository state
and toolchain contract. Runtime artifacts do not embed the final package
digests in bytes covered by `ImplementationDigest`; the outer package envelope
performs that binding after artifact fingerprinting and avoids a second digest
cycle.

### Manifest contents

The deterministic manifest payload includes:

- manifest schema version and engine compatibility profile;
- every rule's metadata, kind, and status;
- canonical facts and content values;
- predicate and formula ASTs;
- structured transition descriptions;
- algorithm registrations and declared dependencies;
- controlled statements and rationales;
- source symbols and repository-relative paths;
- examples, properties, and evidence references;
- supersession and relationship edges;
- presentation labels and unit definitions; and
- canonical explanation vocabulary.

The outer package envelope contains `RulePackageIdentity`; digest values are not
duplicated inside the hashed payload. The manifest does not contain executable
JavaScript as a second authority. Compiled .NET and Fable engine artifacts are
members of the deterministic set covered by `ImplementationDigest` and are
bound to the manifest by the outer package identity.

### Replay binding

Every replay records:

- replay schema and identity;
- engine artifact identity;
- rule-package semantic and manifest digests;
- authoritative inputs and events under the replay contract;
- structured rule applications needed for explanations; and
- any compatibility metadata required to locate the historical viewer and
  engine.

Opening a replay first resolves its manifest digest. The UI must never silently
substitute the current rule package. If the exact package is unavailable, the
viewer reports that historical documentation is unavailable while continuing
to show any explanation text and identifiers embedded in the replay.

An archival bundle may embed the complete manifest. Published packages are
immutable, and packages referenced by supported replay fixtures or releases
are retained by digest.

### Source links

`SourceRef` identifies a compiler/API symbol and repository-relative source
location. Publication turns it into:

- an fsdocs API cross-reference where the symbol is public;
- a repository source link pinned to `SourceCommit`; and
- a readable source label for offline manifests.

Line numbers are navigational hints, not identity. Builds validate symbol and
path references. Historical pages never use an unpinned `main` link as their
only implementation reference.

## Documentation and web-client projections

### Rule explorer

The web client presents the manifest as the comprehensive rule corpus with:

- search by title, ID, concept, tag, status, and semantic kind;
- navigation by game subsystem and dependency graph;
- clear canonical/prototype/proposed/superseded labels;
- typed fact tables generated from authoritative values;
- rendered predicates and formulas;
- interactive formula inputs and plots where meaningful;
- transition flow and phase placement;
- algorithm summaries and source links;
- rationale and supersession history;
- examples, properties, and evidence;
- “used by” links to mechanics, content, and scenarios; and
- deep links stable within a package digest.

The static fsdocs pages and embedded Fable client consume the same generated
manifest. They may optimize presentation differently but must not maintain
independent copies of rules.

### Literate documentation

Literate pages remain valuable for tutorials, worked scenarios, architecture,
and narrative ordering. They execute examples against the rule registry and
embed rule projections by ID. They do not reimplement formulas to demonstrate
that copied code happens to produce the same value.

The eventual migration should replace manually duplicated formula bodies in
literate scripts with registered formula evaluation and rendering. Existing
pages remain until their corresponding subsystem is migrated.

### Simulation integration

The simulator timeline, scene events, inspector, and comparison laboratory link
directly to rule applications. Selecting an outcome preserves tick, event,
entity, and rule context. Selecting a formula operand may highlight its source
in the battlefield or relevant unit/equipment inspector when such a projection
exists.

Historical replay mode pins all rule navigation to the replay's manifest
digest. Current-rule comparison is an explicit secondary action, never an
implicit replacement.

## Coverage and validation

### Coverage graph

The build constructs a graph over:

```text
controlled intent
      ↓
rule definition
      ↓
implementation and rule calls
      ↓
examples / properties / scenarios
      ↓
events and explanations
      ↓
documentation projections
```

Not every node has a one-to-one relationship. The graph must nevertheless make
orphans and unsupported claims visible.

### Build failures

CI rejects at least:

- duplicate, malformed, or reused rule IDs;
- dangling `RelatedRules`, `Supersedes`, governing-rule, or explanation IDs;
- a canonical executable rule without rationale;
- a formula or predicate with invalid units or an unregistered operation;
- a transition that references an unknown phase, effect, event, or rule;
- an algorithm without source identity, explanation projection, and evidence;
- an algorithm registration not mapped to a component covered by
  `ImplementationDigest`;
- a narrative rule used to satisfy executable coverage;
- an authoritative event with a missing or invalid explanation obligation;
- a source symbol or repository path that cannot be resolved;
- nondeterministic manifest ordering or digest generation;
- a digest projection that contains its own output field or a runtime artifact
  that embeds a digest covering that artifact;
- .NET/Fable disagreement over canonical fixtures;
- a replay whose declared package identity disagrees with embedded or resolved
  content;
- a generated rule page or deep link that is missing from the publication; and
- an accepted content table that falls back to presentation strings as its
  numeric authority.

### Evidence expectations

Evidence is proportional to semantic kind:

| Kind | Minimum evidence |
|---|---|
| Fact | codec/validation fixture and governing-rule reference |
| Predicate | truth-table examples and boundary properties |
| Formula | example values, boundary properties, and runtime parity |
| Transition | scenario trace, ordering assertions, and event explanation |
| Algorithm | examples, properties, deterministic fixture, explanation check |
| Narrative | review and relationship validation |

## First vertical slice

The engagement/combat slice is selected because the repository already has
descriptive weapon/body tables, prose formulas, an executable literate example,
and a rules-laboratory implementation. It can prove the architecture against a
real drift boundary.

### Included rules

The slice covers:

1. engagement preparation time;
2. trace probability;
3. armor outcome bands and retained-effect curve;
4. cover-path damage factor;
5. expected damage per shot;
6. selected weapon, body, and shared combat parameters;
7. attack-resolution transition registration; and
8. one complete explained attack event.

### Included projections

The slice delivers:

- typed F# facts and formula/predicate ASTs;
- shared .NET/Fable evaluation;
- a deterministic JSON manifest and canonical digest;
- generated rule and formula views in the web client;
- source, rationale, dependency, and evidence links;
- a simulator explanation tree for an attack;
- a pinned historical package fixture and replay deep link; and
- CI validation for identity, coverage, source resolution, publication, and
  cross-runtime parity.

### Migration boundary

Within the slice, selected values cease to be authoritative strings in the
client catalog, and selected formulas cease to be duplicated between prose,
literate scripts, and executable code. Compatibility projections may preserve
the current table and page appearance while consuming the new corpus.

Mechanics outside the slice keep their current representation and are labelled
unmigrated. The first PR must not claim comprehensive corpus coverage before the
coverage graph can prove it.

### Acceptance criteria

The slice is accepted when:

1. changing a selected typed parameter updates .NET behavior, Fable behavior,
   the manifest, the rule explorer, and relevant documentation in one build;
2. changing a selected formula changes one inspectable AST rather than copied
   F#, algebra, or JavaScript;
3. .NET and Fable produce identical canonical values, rule applications,
   events, and digests for the slice fixtures;
4. the simulator exposes a navigable attack derivation with actual operands;
5. every derivation node resolves to rationale, evidence, and pinned F# source;
6. a historical replay fixture continues to resolve its original package after
   a current-package fixture is introduced;
7. deleting or corrupting a rule, source reference, explanation, or generated
   page makes CI fail with an actionable diagnostic;
8. replay and explanation size measurements are recorded against an explicit
   budget; and
9. authoring review confirms that formula and transition syntax improves
   understanding over direct F# for the selected rules.

## Migration strategy

After the combat slice validates the contracts:

1. stabilize the builder syntax and manifest schema;
2. migrate observation and targeting, which exercise predicates and rule
   dependency explanations;
3. migrate movement and geometry, which exercise registered algorithms;
4. migrate suppression and action lifecycles, which exercise transitions and
   deterministic sampling;
5. migrate equipment, abilities, communications, logistics, magic, and mission
   rules by subsystem;
6. replace remaining copied formula/catalog projections with generated views;
7. introduce relational derivation only if repeated eligibility and dependency
   problems demonstrate its value; and
8. declare the corpus comprehensive only when coverage validation reports no
   unclassified authoritative behavior.

Every migration is incremental. A subsystem records whether each authority is
`Corpus`, `Legacy`, or deliberately `External`, allowing honest progress
reporting without maintaining two canonical implementations.

## Open implementation choices

These choices are intentionally deferred to the vertical slice:

- exact computation-expression and operator syntax;
- whether the rules subsystem initially lives in `SIR.Domain`,
  `SIR.Simulation`, or a small `SIR.Rules` project;
- the erased runtime representation used behind generic typed builders;
- expression DAG interning and formula optimization;
- explanation compression and replay embedding thresholds;
- JSON-schema or generated TypeScript declaration tooling;
- the exact static-site route layout for package digests; and
- whether a relational predicate sub-language is justified after observation
  migration.

They may change without revisiting ADR-0001 provided the stable identity,
single-semantics, explainability, history, and validation contracts remain
intact.

## Relationship to current S.I.R. documentation

The current [Gameplay Combat and Formula Reference](gameplay-formulas.md),
[Executable Combat Formulas](combat-formulas.fsx), and
[interactive rules laboratory](interactive-rules-lab.md) remain valuable input
and migration evidence. The target corpus eventually supplies their
authoritative values and executable expressions.

The [Simulation Core](simulation-core-architecture.md) continues to own
deterministic phase, state, event, and replay behavior. This architecture adds
rule identity and explanation to that boundary; it does not replace the
simulation architecture.

The [Fable Client and Documentation](fable-client-and-documentation.md)
continues to own publication and browser delivery. This architecture defines
the versioned manifest and rule-explorer input that delivery must publish.

The governing decision is
[ADR-0001: Executable Rules Corpus](adr-0001-executable-rules-corpus.md).
