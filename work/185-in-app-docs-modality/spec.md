---
schemaVersion: 1
workId: 185-in-app-docs-modality
title: In App Docs Modality
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# In App Docs Modality Specification

Prose status: specified

## User Value
Players, designers, and contributors can move from a visible tactical concept to maintained rules, rationale, examples, and exact source locations without leaving the running application, then return to the battlefield with its identity and interaction state intact.

## Scope
- SB-001: Add Docs as a shared-registry modality in File/Edit/View menus, shortcut configuration, modal navigation, history, and the accessibility model.
- SB-002: Generate one deterministic in-app manifest from qualified maintained `docs/` Markdown/literate pages and selected generated API references, including status, hierarchy, anchors, relations, and typed source mappings.
- SB-003: Render a wiki experience with hierarchy, TOC, breadcrumbs, filter/search, back/forward, in-page anchors, related pages, keyboard navigation, sanitized headings/tables/images/diagrams/code, and accessible local/external links.
- SB-004: Add disclosure-safe contextual Docs links from tactical inspectors/overlays, preserve the full tactical workscreen snapshot while Docs is active, and restore the same battlefield SVG identity/camera/selection/timeline/playback/panels.
- SB-005: Qualify manifest/FsDocs consistency, safety, source currency, duplicate slugs, broken links, offline/degraded external navigation, 320 CSS-pixel and 400% zoom usability, screen-reader structure, native/Fable parity, production journeys, performance, mutations, feedback, and SDD readiness.

## Non-Goals
- SB-006: Do not add in-browser authoring, arbitrary remote Markdown, comments, or a substitute for GitHub history/review.
- SB-007: Do not reimplement tactical rules in Docs, expose authority-only state, or rebuild the tactical workspace as a side effect of switching modality.
- SB-008: Do not claim live-compositor FPS qualification from deterministic/headless/browser structural evidence.

## User Stories
- US-001 (P1): As a player, I can open Docs by menu or shortcut, search tactical concepts, navigate related pages and anchors, and return exactly where I was.
- US-002 (P1): As a designer or contributor, I can distinguish canonical, implemented, provisional, and research material and follow typed links to the maintained source and selected API references.
- US-003 (P1): As a keyboard, screen-reader, low-vision, narrow-screen, or offline user, I can use local documentation with honest external-link behavior.
- US-004 (P1): As a player inspecting a disclosed unit or overlay, I can open relevant documentation without learning any hidden subject or world state.
- US-005 (P1): As an operator, I can reject manifest drift, unsafe content, stale mappings, broken links, duplicate slugs, runtime divergence, inaccessible output, or exceeded production-route budgets.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002]: Given a populated tactical workspace, when pointer and effective-shortcut routes enter Docs and later return to Editor, Plan, Simulate, or Review, then both routes use one command registry and the tactical state/identity fingerprint is unchanged.
- AC-002 [US-001] [US-002] [FR-003] [FR-004]: Given the maintained corpus, when its manifest is generated twice and browsed, then page/slug/status/hierarchy/anchor/related/source entries are byte-identical, searchable for LOS/cover/armor, and resolve to the same qualified FsDocs inputs.
- AC-003 [US-001] [US-003] [FR-005] [FR-006]: Given headings, tables, diagrams/images, code, links, unsafe markup, 320 CSS pixels, and 400% zoom, when Docs renders and keyboard/screen-reader routes operate, then safe structures remain usable with valid landmarks/heading order and unsafe markup is absent.
- AC-004 [US-002] [US-003] [FR-007]: Given typed GitHub source mappings and unavailable/degraded external access, when a user follows a mapping, then repository/revision/file/optional-line identity is explicit, local Docs remains usable, and failure is labelled without implying successful external navigation.
- AC-005 [US-004] [FR-008]: Given two observers with different disclosed tactical state and malformed/unreadable contextual input, when contextual Docs links project, then only public concept identity is emitted and hidden subject/world geometry/count/label/diagnostic/timing distinctions remain absent.
- AC-006 [US-005] [FR-009]: Given the representative corpus and navigation workload, when the production update/view route runs in Release and the built browser executes it, then manifest/search/render structural caps and declared timing posture pass with host/capability facts and no compositor overclaim.
- AC-007 [US-005] [FR-010]: Given mutations for duplicate slug, broken anchor/link, unsafe markup, stale source mapping, FsDocs drift, state rebuild, hidden-subject leak, route divergence, accessibility failure, budget overflow, or unreadable evidence, when each owning gate runs, then it fails and returns green only after restoration.

## Functional Requirements
- FR-001: The system MUST declare Docs once in the shared command registry and derive File/Edit/View menu presence, effective configurable shortcut, modal input, accessibility name, and navigation-history actions from that identity. (Stories: US-001; Acceptance: AC-001)
- FR-002: Entering Docs MUST preserve the existing tactical workscreen object/DOM identity plus camera, selection, timeline, playback, panel layout/visibility, and active-mode state, and returning to Editor/Plan/Simulate/Review MUST restore the exact fingerprint rather than reconstruct defaults. (Stories: US-001; Acceptance: AC-001)
- FR-003: A deterministic generator MUST qualify maintained `docs/` Markdown/literate content and selected generated API references into one versioned manifest with unique normalized slugs, hierarchy/order, metadata status, headings/anchors, related pages, search text, and content/source digests; the client MUST NOT maintain parallel prose. (Stories: US-001, US-002; Acceptance: AC-002)
- FR-004: Docs MUST provide hierarchy, TOC, breadcrumbs, deterministic search/filter for LOS/cover/armor, back/forward history, in-page anchors, related pages, and keyboard navigation over manifest identity with stable native/Fable results. (Stories: US-001, US-002; Acceptance: AC-002)
- FR-005: Rendering MUST sanitize content before DOM construction and support headings, tables, diagrams/images, code blocks, and typed local/GitHub links under the production CSP/offline policy without executing arbitrary HTML/script or loading arbitrary remote Markdown. (Stories: US-003; Acceptance: AC-003)
- FR-006: Docs MUST expose valid landmarks and heading order, labelled link semantics, visible focus, keyboard-only operation, responsive hierarchy/content controls at 320 CSS pixels, and usable output at 400% browser zoom without color-only status meaning. (Stories: US-003; Acceptance: AC-003)
- FR-007: Typed concept-to-source mappings MUST cover combat, units, maps/spatial queries, simulation, planning, replay, controls, and governance, naming repository, revision/branch policy, file, symbol/concept, and stable line anchor when available; unavailable external navigation MUST preserve local content and announce failure honestly. (Stories: US-002, US-003; Acceptance: AC-004)
- FR-008: Tactical inspectors and overlays MUST offer contextual Open documentation actions only from disclosed public concept identifiers; missing, malformed, unreadable, or undisclosed inputs MUST fail closed to the same no-link shape without subject/world/timing/diagnostic leakage. (Stories: US-004; Acceptance: AC-005)
- FR-009: A versioned representative workload MUST traverse production command update, Docs navigation/search, manifest projection, and view construction over the qualified corpus, declaring content/node/result/history caps and timing posture before implementation, recording deterministic counters and browser host/capability facts, and labelling absence of live-compositor measurement. (Stories: US-005; Acceptance: AC-006)
- FR-010: The change MUST ship synchronized public/client contracts, FsDocs and in-app manifest qualification, focused/full native and Fable tests, a real-entry bot-driven production-browser journey through player-visible controls, runtime-route comparison, 320/400% accessibility, schema-v2 feedback/audit, SDD evidence/verify/ship, and protected-subject plus unreadable-input mutation evidence for every added or modified gate. (Stories: US-005; Acceptance: AC-007)

## Ambiguities
- AMB-001: Which maintained documents and generated API references qualify for v1, and which metadata/status taxonomy determines inclusion and navigation order?
- AMB-002: What versioned manifest/content representation supports safe deterministic in-app rendering without duplicating the FsDocs corpus?
- AMB-003: How does Docs modality state compose with the retained tactical workscreen, command registry, browser history, and back/forward behavior?
- AMB-004: Which typed concept-to-source mappings and revision/line-anchor policy are stable enough for v1?
- AMB-005: Which sanitizer/CSP/offline/link semantics and fail-closed contextual-link envelope prevent active-content and hidden-state leaks?
- AMB-006: What representative corpus/search/navigation workload, structural counters, and timing caps extend the producer performance posture?
- AMB-007: Which production controls, viewport/zoom/accessibility checks, runtime comparisons, and mutation subjects constitute sufficient player-visible proof?

## Public Or Tool-Facing Impact
- Additive shared command/navigation/modal-input state and public renderer-neutral documentation manifest, page, link, concept-source, query/history, and cost-counter values.
- Generated Web assets and production menus/views/keyboard/accessibility behavior consume the manifest without becoming content authority.
- CI/docs tooling gains one qualification boundary joining FsDocs and in-app inputs plus link/safety/source/performance/accessibility/browser mutation receipts.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 185-in-app-docs-modality`.
