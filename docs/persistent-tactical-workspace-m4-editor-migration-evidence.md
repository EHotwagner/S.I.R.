---
title: Persistent Tactical Workspace M4 Editor Migration Evidence
category: Tools & Evidence
categoryindex: 5
index: 16
status: accepted
decision-status: implemented
last-updated: 2026-07-31
---

# Persistent Tactical Workspace M4 Editor Migration Evidence

Milestone 4 moves the complete Editor presentation boundary onto the retained
`svg#persistent-tactical-svg`. The authored `MapEditorState` and
`EditorWorkspaceState` remain authoritative; the persistent renderer only
projects their geometry, selection, camera, and gesture state.

## Renderer and control mapping

| Editor capability | Persistent owner |
|---|---|
| Raster background | `#persistent-editor-background[data-editor-layer=background]` |
| Terrain, edges, and units | stable shared `terrain`, `edges`, and `units` scene layers with Editor layer state |
| Regions | `[data-editor-layer=regions]` |
| Grid and cursor guides | `guides` and `cursor-guide` Editor layers |
| Terrain, edge, placement, movement, and box-selection previews | conditional persistent Editor preview layers |
| Validation overlay | `[data-editor-layer=validation-overlay]` |
| Tool domains | registered `tools` panel |
| Layer state | registered `layers` panel |
| Selected-object inspector | registered `selection` panel |
| Validation navigation | registered `validation` panel |
| Map metadata, file interchange, recovery, and destructive actions | registered `document` panel |

Pointer capture, hit testing, terrain gestures, unit movement, box selection,
placement hover, wheel zoom, context-menu suppression, and camera pan now enter
through the persistent SVG. Keyboard commands still resolve through the one
modal command registry and its availability boundary.

## Parity and removal proof

`scripts/test-map-editor-qualification.mjs` exercises the production Fable
bundle and proves:

- exactly one connected application workscreen and stable semantic scene layers;
- real Editor bodies in the registered tools, layers, selection, validation,
  and document panels;
- atomic terrain, edge, region, and clipboard transactions with exact
  undo/redo behavior;
- real pointer unit movement and box-selection previews/commits, Shift pointer
  and keyboard multi-selection, and keyboard-operable keyed regions;
- live Hidden and Dimmed behavior for terrain, edge, unit, selection, region
  geometry, region annotations, and preview layers;
- diagonal/cross terrain hatches, objective rings, canonical unit glyphs and
  footprint names, and distinct wall, closed/open door, and window visuals;
- camera, Pan-held, native-input, modal-help, focus, destructive-confirmation,
  validation, and accessibility behavior;
- observed autosave storage after an authored change, reviewed non-native
  import cancel/accept, native validation overlay, and both destructive
  confirmation outcomes;
- singleton registered capability landmarks with no duplicate Editor menus,
  quick toolbar, layer controls, validation controls, or document controls;
- absence of the old Editor renderer source, roots, object-list grid, and CSS;
  and
- seven SVG/PNG review boards whose hashes are bound to the production bundle.

The legacy `editorBattlefield`, `editorGrid`, their private rendering helpers,
and their renderer-specific CSS were deleted only after the pre-removal parity
suite passed. Historical M0 fixtures now retain the still-relevant inventory
while replacing superseded Editor-root assertions with persistent landmarks.

The generated review manifest and boards live under
[`docs/assets/map-editor-review`](assets/map-editor-review/README.md). They
include terrain, edges, units, regions, a signature-validated raster background
drawn in the persistent SVG, immutable simulator handoff, and validation/import
review evidence.

## Qualification commands

```text
npm run build:client
npm run review:map-editor
npm run test:map-editor-qualification
npm run test:persistent-workspace-m0
node scripts/smoke-client.mjs
./fake.sh build -t Dev
./fake.sh build -t Test
./fake.sh build -t Verify
```
