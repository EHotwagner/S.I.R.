---
schemaVersion: 1
workId: 185-in-app-docs-modality
title: In App Docs Modality
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/185-in-app-docs-modality/spec.md
publicOrToolFacingImpact: true
---

# In App Docs Modality Clarifications

## Source Specification
- work/185-in-app-docs-modality/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Which maintained pages/API references qualify and which metadata/status taxonomy determines v1 navigation?
- CQ-002 [AMB:AMB-002]: Which manifest/content representation preserves deterministic safe rendering without parallel prose?
- CQ-003 [AMB:AMB-003]: How does Docs navigation compose with retained tactical state and browser history?
- CQ-004 [AMB:AMB-004]: Which concept/source mappings and revision/line-anchor policy are stable for v1?
- CQ-005 [AMB:AMB-005]: Which sanitizer, CSP, offline, external-link, and contextual-link envelope fails closed?
- CQ-006 [AMB:AMB-006]: Which production workload, counters, caps, and timing posture qualify Docs?
- CQ-007 [AMB:AMB-007]: Which production controls, accessibility checks, runtime comparisons, and mutations prove acceptance?
- CQ-008: How does the in-app Docs modality compose with the canonical deferred FeatureLoader and a route budget that no longer fits the immutable registry-v1 ceiling?

## Answers
- CQ-001 → qualify every top-level maintained `.md`/`.fsx` page carrying valid frontmatter plus selected generated `reference/*.html` API entries; v1 status is the explicit pair `status` and `decision-status`, normalized to `canonical`, `implemented`, `provisional`, or `research`. Navigation orders by category index, page index, title, then slug; malformed metadata fails generation.
- CQ-002 → `scripts/generate-in-app-docs.mjs` reads authored Markdown/literate source and the built FsDocs site, emits schema `sir-in-app-docs-v1` JSON with normalized safe block nodes rather than HTML, and records source/content digests. The browser renders only this closed node vocabulary; the qualification gate compares page identities/digests against FsDocs inputs.
- CQ-003 → extend `TacticalModality` with `Docs` and retain `LastTacticalModality`; Docs page/query/history/anchor state is separate from battlefield/timeline/panel state. The mounted tactical SVG remains present but hidden/inert. Shared commands enter Docs, return to the retained tactical modality, and navigate local history; browser URL state mirrors successful updates but never becomes model authority.
- CQ-004 → v1 maps `combat`, `units`, `maps-spatial`, `simulation`, `planning`, `replay`, `controls`, and `governance` to repository `EHotwagner/S.I.R.`, revision policy `main`, maintained file path, public concept/symbol, and optional checked line anchor. Generation validates path existence and line bounds; URLs use the declared revision policy and encoded path/anchor.
- CQ-005 → parsing discards raw HTML/script/style/event attributes and remote media, admits only headings, paragraphs, lists, tables, fenced code, local images, and typed links, and rejects unsafe URI schemes. Local navigation always works. External links open only after the host reports capability and otherwise produce an announced degraded result. Context actions accept a disclosed concept enum/string only; unknown/unreadable input yields exactly no link and generic diagnostics/counters.
- CQ-006 → run deterministic manifest/search/navigation/update/view over the complete qualified corpus and a fixed LOS/cover/armor query set. Caps: 512 pages, 8,192 blocks, 262,144 search tokens, 200 search results, 128 local-history entries, and 6,000 rendered Docs DOM nodes; Release p95 posture is 20 ms for representative query/update/view and 50 ms for full-corpus construction on the qualification host. The token cap was set after a pre-feature baseline measured 202,423 source tokens. Receipt records definition digest, candidate/runtime/host, counters and explicitly says compositor not measured.
- CQ-007 → Playwright boots the built production entry, captures a battlefield identity/state fingerprint, opens Docs via View menu and effective shortcut, searches LOS/cover/armor, traverses cross-links/anchors/back-forward, inspects a typed GitHub link, exercises degraded external navigation, uses a disclosed contextual link, checks 320 CSS pixels/400% zoom/landmarks/headings/keyboard focus, returns to each tactical modality, and confirms the fingerprint. Native/Fable compare manifest/query/history fingerprints; every new/modified gate has protected-subject and unreadable-input mutants.
- CQ-008 → keep manifest I/O and navigation state in a typed bootstrap contract, place the actual Feliz Docs renderer behind the registered Fable dynamic edge, and preserve registry v1 unchanged. Publish registry v2 for the real `DocsView` identity, rebaseline Rules Explorer Brotli from 16,000 to 16,384 bytes after the source-frozen composition measured 16,040 bytes, and move browser-observed initial/Rules activation ceilings into that same registry. The initial route is explicitly rebaselined from 1,250,000 to 1,310,720 response bytes after hosted Chromium observed 1,281,108; version and route subjects retain named red mutations.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-003] [FR-007]: Qualify metadata-bearing maintained pages plus selected built API entries under the four normalized status values and canonical navigation order; malformed metadata and duplicate identity fail generation.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-003] [FR-005]: Generate one versioned closed-block JSON manifest from the authored corpus and built FsDocs output, with source/content digests and no raw executable HTML or client-authored prose.
- **DEC-003** [CQ-003] [AMB:AMB-003] [FR-001] [FR-002] [FR-004]: Make Docs a modality with separate page/query/history state and a retained last-tactical modality; keep the same mounted tactical SVG/workspace instance hidden and inert, and treat URLs as an edge projection.
- **DEC-004** [CQ-004] [AMB:AMB-004] [FR-007]: Ship eight typed concept mappings against `main`, validate paths and optional line bounds at generation, and construct encoded GitHub URLs from typed fields.
- **DEC-005** [CQ-005] [AMB:AMB-005] [FR-005] [FR-008]: Render only a closed sanitized block/link vocabulary, reject unsafe schemes/remote content, keep local Docs operational offline, and make undisclosed/malformed contextual input indistinguishable from absence.
- **DEC-006** [CQ-006] [AMB:AMB-006] [FR-009]: Adopt the full-corpus and fixed-query production update/view workload with the 512-page, 8,192-block, 262,144-token, 200-result, 128-history, 6,000-node, 20/50-ms caps and explicit compositor-not-measured capability.
- **DEC-007** [CQ-007] [AMB:AMB-007] [FR-010]: Require native/Fable fingerprints plus a real built-entry Playwright control journey covering search/navigation/source/degraded/context/state preservation and 320/400% accessibility, with subject and unreadable-input inversions for every touched gate.
- **DEC-008** [CQ-008] [FR-001] [FR-009] [FR-010]: Integrate Docs through the canonical typed FeatureLoader as one deferred `DocsView` chunk, retain immutable registry v1, and make registry v2 own both feature-chunk and browser-observed route ceilings; explicitly rebaseline the measured Rules Explorer Brotli and initial-route subjects, fail F#/JavaScript/compiled version drift, and invert version/route gates.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None. All blocking ambiguities are resolved by DEC-001 through DEC-008.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 185-in-app-docs-modality`.
