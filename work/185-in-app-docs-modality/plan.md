---
schemaVersion: 1
workId: 185-in-app-docs-modality
title: In App Docs Modality
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/185-in-app-docs-modality/spec.md
sourceClarifications: work/185-in-app-docs-modality/clarifications.md
sourceChecklist: work/185-in-app-docs-modality/checklist.md
publicOrToolFacingImpact: true
---

# In App Docs Modality Plan

Prose status: planned

## Source Snapshot
- spec: work/185-in-app-docs-modality/spec.md sha256:0540a98f00e9133723a0bd5abb28ae3bb1a463ca3c5a0997fae202a68bf8cdcc schemaVersion:1
- clarifications: work/185-in-app-docs-modality/clarifications.md sha256:726b782722840a14260feff0626fcb7b70ea0d6765c5104ab6d876a55dc0a94d schemaVersion:1
- checklist: work/185-in-app-docs-modality/checklist.md sha256:4d063a707700edc76eb103559a54330d853ab96a7c6d7a6d54036402fb041339 schemaVersion:1

## Plan Scope
- Tier-1 additive Client/Web/docs-tooling change over the retained tactical workspace and existing FsDocs publication route.
- Declare public documentation/navigation values in `UnifiedTacticalWorkspace.fs` before behavior; generate the closed manifest through scripts from maintained docs/FsDocs artifacts; then bind Web state, commands, menus, rendering, accessibility, external navigation, contextual links, tests, docs, and evidence.
- The existing mounted battlefield SVG and its Editor/Plan/Simulate/Review models remain the tactical authority; Docs is a separate projection whose activation only changes modality/docs-navigation state.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-003] complete: Add `Docs` modality, shared enter/return/navigation command definitions, availability, effective shortcut and modal-input mapping in `UnifiedTacticalWorkspace`; derive Web File/Edit/View menu and accessible presentation from the registry.
- PD-002 [AC-001] [FR-002] [DEC-003] complete: Retain `LastTacticalModality` plus Docs page/query/history state beside existing battlefield/timeline/panel models; hide/inert the exact mounted tactical workscreen while Docs is visible and prove object/state fingerprint identity across every return route.
- PD-003 [AC-002] [FR-003] [DEC-001] [DEC-002] complete: Add a deterministic Node generator that reads qualified Markdown/literate frontmatter/content and built FsDocs API identities, parses a closed safe block vocabulary, validates metadata/slugs/anchors/links/source paths/line bounds, records digests, and emits `sir-in-app-docs-v1` into the versioned production bundle.
- PD-004 [AC-002] [FR-004] [DEC-003] complete: Add pure manifest lookup/search/history/breadcrumb/TOC/related/anchor transitions in shared F# with stable normalization and ordering; native and Fable tests compare full fingerprints for LOS/cover/armor and navigation scripts.
- PD-005 [AC-003] [FR-005] [DEC-002] [DEC-005] complete: Render only closed manifest blocks and typed links in Feliz; reject raw HTML, script/style/events, unsafe URI schemes, and remote Markdown/media at generation and verification, preserving production CSP/offline constraints.
- PD-006 [AC-003] [FR-006] [DEC-007] complete: Build Docs landmarks, heading hierarchy, labelled local/GitHub links, roving/tree keyboard navigation, visible focus, non-color status tokens, responsive 320 CSS-pixel layout and 400% zoom behavior; verify from the built production browser.
- PD-007 [AC-004] [FR-007] [DEC-004] complete: Generate and validate eight typed concept-source mappings with repository/main/path/concept/symbol/optional line, construct encoded GitHub URLs, and represent external open success/failure as announced host results while local Docs stays live.
- PD-008 [AC-005] [FR-008] [DEC-005] complete: Project contextual actions solely from disclosed public concept identities supplied by existing inspector/overlay routes; unknown, malformed, unreadable and undisclosed input returns one generic no-link/counter shape tested across perspectives.
- PD-009 [AC-006] [FR-009] [DEC-006] complete: First implement an executable production manifest/search/navigation/update/view workload and baseline it before feature behavior; enforce 512/8,192/262,144/200/128/6,000 structural caps plus 20/50-ms Release posture, definition digest, allocation/host/browser capability facts and compositor-not-measured label.
- PD-010 [AC-007] [FR-010] [DEC-007] complete: Add focused/full .NET and Fable tests, docs/FsDocs manifest qualification, exact runtime-route comparison, real-entry bot-driven Playwright journey through player-visible controls, 320/400% accessibility, one subject and unreadable-input mutation per gate, schema-v2 feedback, SDD and exact-head review evidence.

## Contract Impact
- PC-001 [PD-001] [PD-002] navigationSurface: `TacticalModality`, shared command definitions, retained-tactical/docs navigation model and pure transitions are additive public Client surfaces authored in `UnifiedTacticalWorkspace.fs`.
- PC-002 [PD-003] [PD-005] manifestSchema: `sir-in-app-docs-v1` is a generated closed-block/source-digest contract; maintained docs/FsDocs remain authority and raw HTML/remote Markdown never enters the browser model.
- PC-003 [PD-004] historySearch: Search normalization/order, local history bounds, anchors, breadcrumbs, TOC and related-page identity are deterministic shared F# values with native/Fable equality.
- PC-004 [PD-007] sourceMapping: Concept mappings are typed repository/revision/path/concept/symbol/line data validated against the checkout and lowered to GitHub URLs only at the Web edge.
- PC-005 [PD-008] disclosureBoundary: Contextual documentation receives public disclosed concept identity only and returns a constant generic absence for hidden/malformed/unreadable inputs.
- PC-006 [PD-009] performanceContract: `docs/performance-budget.md` owns the Docs workload identity, definition digest, structural caps, Release timing posture, host/capability disclosure and no-compositor limitation.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PC-001] stateRouteTest: Native/Fable tests and built browser controls assert one registry identity, effective rebinding, modality history and exact tactical object/state fingerprint preservation through all return routes.
- VO-002 [PD-003] [PD-005] [PC-002] manifestSafetyTest: Generate twice for byte identity; validate qualified corpus/FsDocs parity, unique slugs, all internal anchors/links/pages/source paths/lines, safe node vocabulary and CSP/offline URI policy.
- VO-003 [PD-004] [PC-003] searchHistoryTest: Compare native/Fable full fingerprints for LOS/cover/armor results, hierarchy/TOC/breadcrumbs/related/anchors, keyboard traversal, back/forward, unknown query/page and bounded history.
- VO-004 [PD-006] accessibilityTest: Built Playwright checks landmarks/heading order/focus/keyboard/link names/status semantics at ordinary, 320 CSS-pixel and 400% zoom routes.
- VO-005 [PD-007] [PC-004] sourceLinkTest: Validate all eight mappings, encoded GitHub targets and optional line bounds; exercise successful-capability and degraded/unavailable external opens without breaking local navigation.
- VO-006 [PD-008] [PC-005] disclosureTest: Two observer fixtures plus hidden/malformed/unreadable inputs assert identical no-link shape/counters and scan built output for forbidden subject/world tokens.
- VO-007 [PD-009] [PC-006] performanceTest: Release production generator/update/search/view and built-browser structural workload enforce caps/timing, stable definition digest and exact candidate/host/runtime/capability receipt without compositor overclaim.
- VO-008 [PD-010] gateMutationTest: Invert duplicate slug, internal link/anchor, sanitizer, source mapping, FsDocs parity, state retention, disclosure, runtime equality, accessibility, performance and unreadable receipts one at a time; each owning gate must red and recover green.
- VO-009 [PD-010] lifecycleTest: Run Dev/Test/Verify, focused TRX, Fable/Node, production browser journey/accessibility, docs, schema-v2 feedback/audit/checkpoints, SDD evidence/verify/ship/refresh/agents, exact-head CI/path/claim and independent review gates.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive-modality: Existing four tactical modality identities and shortcuts remain valid; Docs adds an identity and explicit return target without remapping persisted tactical state.
- PM-002 [PC-002] generated-only: Existing FsDocs inputs remain canonical; in-app JSON is always replaced by the deterministic generator and never hand-edited.
- PM-003 [PC-003] bounded-history: Docs history begins empty and is separate from tactical timeline/browser session state; stale/unknown page identities fall back to the canonical Docs index.
- PM-004 [PC-004] branch-policy: Source links target the declared `main` policy until a future immutable-release mapping is authored; paths/lines are generation-validated.
- PM-005 [PC-005] fail-closed: Existing inspectors without a known disclosed concept emit no contextual link; no heuristic text-to-concept fallback is retained.

## Generated View Impact
- GV-001 [PD-003] docsManifest: production bundle manifest/content/search/source-map JSON and qualification/performance receipts regenerate from current docs, built FsDocs/API output and script identity or fail stale/unreadable.
- GV-002 [PD-010] lifecycleViews: analysis, work model, summary, equivalent Claude/Codex guidance, verify, ship, optional Governance handoff and schema-v2 feedback/audit refresh from current authored sources and observed evidence.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Baseline docs build emits two non-fatal happy-dom `removeChild` DOMExceptions while the owning accessibility gate still passes; this pre-existing behavior is not treated as Docs acceptance evidence and must not be obscured by the new journey.
- Headless browser structural/timing evidence is distinct from live-compositor qualification.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 185-in-app-docs-modality`.
