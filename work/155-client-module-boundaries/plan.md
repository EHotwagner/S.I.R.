---
schemaVersion: 1
workId: 155-client-module-boundaries
title: Client Module Boundaries
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/155-client-module-boundaries/spec.md
sourceClarifications: work/155-client-module-boundaries/clarifications.md
sourceChecklist: work/155-client-module-boundaries/checklist.md
publicOrToolFacingImpact: true
---

# Client Module Boundaries Plan

Prose status: planned

## Source Snapshot
- spec: work/155-client-module-boundaries/spec.md sha256:ce4a19d0cef05e307efadd2a10f12d10b662998eaf4af5a5fa3fcb20402c1048 schemaVersion:1
- clarifications: work/155-client-module-boundaries/clarifications.md sha256:be3e529b1a41224909daff2f533a46085e658991b59fdf319c949c4effb26e2b schemaVersion:1
- checklist: work/155-client-module-boundaries/checklist.md sha256:6199b419ca25a0ed446bfa2007c4790d21ed102d2378ca0c0e29195c7748124a schemaVersion:1

## Plan Scope
- Retain `App.fs` as the only root Elmish composition/program owner while moving public model
  types, shell reconciliation, command registry, mode/scene adapters, review panels, and browser
  persistence/file/download/runner effects into compilation-ordered modules.
- Split MapEditor public types, history, revision, and validation from the domain implementation;
  retain the established `MapEditorInterchange` serialization surface rather than duplicate it.
- Keep the production client executable as the qualification route, with a deterministic JUnit
  output adapter and source-subject anti-regrowth checks.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Compile `AppTypes`, `AppShell`, `CommandRegistry`,
  `ModeAdapters`, `SceneAdapters`, `PanelViews`, and `BrowserInfrastructure` before `App`; the
  root delegates real call sites while preserving its model/program ownership.
- PD-002 [AC-002] [FR-002] complete: Compile MapEditor types, history, revision, and validation
  before the remaining update/projection implementation, documenting `MapEditorInterchange` as
  the existing serialization boundary.
- PD-003 [AC-003] [FR-003] complete: Keep JUnit/report and dense performance qualification in
  dedicated precompiled test modules with narrow shared-fixture arguments; inspect App, MapEditor,
  and Program source ceilings in production qualification. Prove each check red by mutating its
  real subject, then restore and run the real test/player routes.

## Contract Impact
- PC-001 [PD-001] F# module contract: App boundary modules use explicit module names and compile
  before `App.fs`; only `BrowserInfrastructure` may own browser I/O, and `App.fs` remains the sole
  Elmish composition/program surface.
- PC-002 [PD-002] F# module contract: `MapEditorTypes`, `MapEditorHistory`, `MapEditorRevision`,
  and `MapEditorValidation` compile before `MapEditor.fs`; `MapEditorInterchange` remains the
  serialization/import-export contract.

## Verification Obligations
- VO-001 [PD-001] [PC-001] semanticTest: Build the Web client and run the production client test
  executable with `--junit`; reject malformed report arguments before qualification and bind the
  generated report to SDD evidence.
- VO-002 [PD-002] [PC-002] semanticTest: Run the map-editor qualification and deterministic
  interchange/client tests after compilation-order changes.
- VO-003 [PD-003] semanticTest: Mutate App, MapEditor, and Program source subjects past their
  unchanged ceilings and confirm qualification fails, then restore the exact production source.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatibility: Preserve root Elmish behavior, command ids, persistence values,
  browser download behavior, and workspace routes while moving implementation ownership only.
- PM-002 [PC-002] compatibility: Preserve map import/export bytes and public editor behavior by
  retaining MapEditorInterchange as the single serialization owner.

## Generated View Impact
- GV-001 [PD-001] generated guidance: Refresh the SDD work model, summary, and Codex/Claude views
  after evidence/verification so the committed lifecycle receipt remains source-current.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 155-client-module-boundaries`.
