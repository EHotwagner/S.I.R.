# Rules coverage graph v1 contract

The generated graph has stable sorted nodes `(kind, identity, authority)` and typed edges. Required node kinds are rule, implementation, event, explanation, example/property, documentation, source, and replay fixture. Authority is `Corpus`, `Legacy`, or `External`.

Every `Corpus` executable rule must reach implementation, event/application, evidence, documentation, source, and replay nodes. Narrative rules cannot satisfy executable edges. Every migrated implementation/event must point back to a registered rule. Mechanics outside the combat slice remain explicit `Legacy`; an absent authority classification is an error.

The graph is a generated view of the registry and declared migration inventory. It carries schema version and package manifest digest and is byte-stable across two runs from identical inputs.
