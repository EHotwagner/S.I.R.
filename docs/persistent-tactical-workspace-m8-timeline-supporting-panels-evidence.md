---
title: Persistent Tactical Workspace M8 Timeline and Supporting Panels Evidence
category: Architecture
categoryindex: 1
index: 20
---

# Persistent Tactical Workspace M8 Timeline and Supporting Panels Evidence

Milestone 8 completes the shell-owned timeline and converts Rules, Data, and
Samples from replacement pages into registered supporting panels. These
operations change layout and contextual content only. They do not replace the
tactical shell, SVG, timeline, camera, cursor, or valid semantic selection.

## Timeline ownership and persistence

The bottom panel has one production timeline call site and remains mounted
when hidden or collapsed. Its single ordered lane list renders the shared
authored, predicted, accepted, and committed segment model; simulator and
Review add committed runtime projections to that same list rather than
creating modality-specific lanes. Cursor and committed-through values remain
attributes of this one timeline landmark.

Field Focus defaults the bottom panel to 152 CSS pixels and expanded in every
modality, including Editor. Milestone 9 corrected the earlier collapsed-Editor
default so the shallow real channels are visible in the accepted initial
arrangement. Height is persisted in the strict
versioned layout profile and clamps to 96–480 pixels. The native-focusable
horizontal separator supports pointer drag, Arrow Up/Down, Page Up/Down, Home,
and End. Pointer moves update presentation immediately but coalesce storage to
one write at pointer end; each keyboard resize is one persisted operation and
returns focus to the separator. Visibility uses the hidden state while keeping
the panel and timeline nodes reconciled in place.
Collapsed content is removed from keyboard and accessibility navigation through
its hidden wrapper without unmounting the timeline node.

## Supporting-panel ownership

| Capability | Registered owner | Retained behavior |
|---|---|---|
| Rules | `rules` panel | scenario catalog, worker-backed laboratory results, comparison, sandbox, inspector |
| Data | `data` panel | canonical rules-data catalog and tables |
| Samples | `samples` panel | map/simulator and replay sample catalog and open actions |

The navigation buttons reveal and expand the corresponding registered panel,
open its responsive drawer when required, and move focus to the panel body.
Hiding a panel restores focus to its toolbar toggle. Rules native numeric input
retains browser arrow-key behavior without moving the tactical cursor. At the
320 CSS-pixel browser viewport used to emulate 400% reflow, both drawer
toggles, the bottom panel, and Samples content remain reachable. M8 introduces
no supporting-content modal or overlay; the pre-existing modal input and help
surfaces retain their earlier qualified semantics.

The obsolete `RulesWorkspace` and `SamplesWorkspace` union branches, their
replacement-page view cases, `.dashboard`, and `.samples-workspace` CSS have
been removed after panel parity passed.

## Identity and fail-closed evidence

The browser qualification captures the exact shell, viewport, SVG, all shared
SVG layers, and unified timeline nodes. After pointer and keyboard resize,
collapse, hide/show, panel reorder/visibility, responsive drawer changes,
Rules worker activity and native input, Data inspection, Samples opening, and
return to Editor, it requires those same object references to remain connected.
It also requires the non-default camera transform, semantic unit selection,
cursor, committed-through boundary, and segment/channel snapshot to remain
unchanged where the operation is layout-only. Duplicate application roots,
SVGs, timelines, lane lists, or legacy battlefield landmarks fail the trace.

`test-timeline-supporting-panels-m8-qualification.mjs` fails closed if a legacy
replacement-page branch or CSS selector returns; if the timeline definition,
single call site, channel landmark, mounted-hidden behavior, registered panels,
resize bounds/defaults, coalesced persistence boundary, focus/native-input,
responsive, or exact-reference assertions disappear; or if the production
browser smoke fails.

## Commands

```text
dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj --no-restore
./scripts/build-client.sh
node scripts/test-persistent-workspace-m0-baseline.mjs
node scripts/test-map-editor-qualification.mjs
node scripts/test-planning-workspace-m5-qualification.mjs
node scripts/test-simulator-workspace-m6-qualification.mjs
node scripts/test-review-workspace-m7-qualification.mjs
node scripts/test-timeline-supporting-panels-m8-qualification.mjs
./fake.sh build -t Dev
./fake.sh build -t Test
./fake.sh build -t Verify
```

Milestone 7 replay authority and disclosure evidence is recorded in the
[Review migration evidence](persistent-tactical-workspace-m7-review-migration-evidence.md).
