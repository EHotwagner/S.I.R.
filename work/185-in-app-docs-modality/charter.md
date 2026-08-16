---
schemaVersion: 1
workId: 185-in-app-docs-modality
title: In-application Docs modality with wiki navigation and source links
stage: charter
changeTier: tier1
status: chartered
policyPointers:
  - .fsgg/sdd.yml
  - .fsgg/agents.yml
  - .fsgg/policy.yml
  - .fsgg/capabilities.yml
  - .fsgg/tooling.yml
---

# In-application Docs modality with wiki navigation and source links Charter

## Identity
Deliver Docs as a first-class non-tactical modality inside the shipped browser application, generated from the maintained repository corpus and connected to exact source locations, while preserving every tactical workspace state value when users leave and return.

## Principles
- Maintained `docs/` and selected generated API references remain the sole prose authority; the browser consumes a deterministic generated manifest and does not acquire a parallel hand-maintained corpus.
- Docs navigation is ordinary application state and command routing: menu, shortcut, history, accessibility, and tactical contextual links use the same shared registry and pure update boundary.
- Leaving the tactical workscreen for Docs must not rebuild or mutate battlefield SVG identity, camera, selection, timeline, playback, or panel state.
- Content is sanitized before rendering; CSP/offline behavior, typed local/external link semantics, and status metadata fail closed and remain honest under unavailable GitHub access.
- Public contracts, deterministic manifest qualification, native/Fable/browser parity, 320 CSS-pixel and 400% accessibility, production journeys, performance evidence, mutations, schema-v2 feedback, and SDD readiness ship together.

## Scope Boundaries
- In: Docs modality and shared command/menu/shortcut/history/accessibility integration; deterministic corpus/API manifest; hierarchy, TOC, breadcrumbs, search, history, anchors, related pages, keyboard navigation; sanitized Markdown structures; typed concept-to-source mappings; contextual tactical links; document status presentation; FsDocs consistency qualification.
- In: production-browser journeys, state-preservation fingerprints, offline/degraded external-link disclosure, responsive/zoom/screen-reader checks, deterministic update/view structural workloads, fail-capable regression gates, lifecycle and feedback artifacts.
- Out: browser editing of canonical docs, arbitrary remote Markdown, comments, replacement of GitHub history/review, new tactical rules, exposure of undisclosed state, or a live-compositor claim on a headless host.

## Policy Pointers
- Honor constitution I-III with specification-first work, explicit Tier-1 command/manifest/source-link contracts, and synchronized generated/public surfaces.
- Honor constitution IV-V by keeping navigation/search/state restoration pure and placing Markdown loading, browser APIs, clipboard/external navigation, and storage at host edges.
- Honor constitution VI-VIII with real maintained content, production-entry browser evidence, protected-subject and unreadable-input mutations, deterministic counters, and explicit local/offline fallbacks.
- Apply `.fsgg/sdd.yml`, `.fsgg/agents.yml`, `docs/performance-budget.md`, `docs/gameplay-testing.md`, and `docs/fable-client-and-documentation.md`; Governance remains an optional protected-boundary consumer.

## Lifecycle Notes
- Tier 1: this changes public Client command/navigation state, generated documentation contracts, production rendering/accessibility, browser behavior, CI qualification, and delivery evidence.
- The typed performance posture is the existing 20 Hz authoritative update plus production browser structural budgets; this item will declare a representative Docs update/view workload and will not invent live-compositor evidence.
- The issue's typed delivery route requires implementation-ready SDD before implementation paths are touched.
- Next lifecycle action: `fsgg-sdd specify --work 185-in-app-docs-modality`.
