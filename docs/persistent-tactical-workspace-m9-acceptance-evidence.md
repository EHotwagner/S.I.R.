---
title: Persistent Tactical Workspace M9 Acceptance Evidence
category: Architecture
categoryindex: 1
index: 21
status: implemented
decision-status: implemented
version: "1.1"
last-updated: 2026-07-31
---

# Persistent Tactical Workspace M9 Acceptance Evidence

Milestone 9 closes the corrective migration. Production now has one tactical
shell call, one workscreen region, one `persistentSceneSvg` renderer call, and
one `svg#persistent-tactical-svg` root. Editor, Plan, Simulate, Review, layout,
timeline, supporting-panel, overlay, worker, and playback changes reconcile
inside that retained object.

## Final removal

The obsolete battlefield viewport lifecycle landmark, migration disclosure,
compatibility CSS, workspace-content wrapper, Editor page wrapper, extra
supporting-section navigation row, and wrapper-era characterization command
were removed. The view no longer chooses separate tactical-shell branches; it
creates only the modality owner’s transient status/import/confirmation content
and passes that into the single shell path. Rules, Data, and Samples are
compact toolbar controls backed by registered panels.

The M9 gate inspects F# source, emitted production JavaScript, emitted CSS, and
the mounted DOM. It fails if any legacy Editor/Planner/Simulator/Review root,
accessible name, workspace class, dashboard, migration landmark, or alternate
SVG/application root returns. Source must contain exactly one renderer
definition, SVG-root creation, renderer call, workscreen-region definition and
call, and tactical-shell call.

## Acceptance matrix

| Concern | Accepted evidence |
|---|---|
| Workscreen identity | browser retains the exact SVG and seven exact layer objects through every modality round trip, panel/timeline/support operation, overlay, resize, playback, and sample open |
| Renderer uniqueness | source cardinality, production JS/CSS forbidden-token inspection, and DOM singleton application/work-surface counts |
| Field Focus goal | a real Chromium 1440×900 capture measures the production shell: 208 px left, 224 px right, 152 px expanded timeline, a workscreen over 68% of the live layout frame, both default sidebars open, and no landmark or Tools-panel overlap |
| Spatial continuity | non-default camera and valid semantic selection assertions accompany every qualified transition |
| State authority | exact Plan owner/revision/tick/disclosure and terrain/edge/route/unit snapshot survives layout, Rules/Data/Samples, and 400% operations; simulator pinned reset and replay committed-frame tests remain separate |
| Disclosure | pure projection tests reject hidden perspective facts, runtime/editor leakage into Plan, simulator handoff mutation, and cross-owner Review interpolation |
| Panel configuration | strict layout tests cover move, reorder, collapse, hide, restore, persistence, version-zero migration, malformed/future rejection, and reset |
| Focus | contextual help, binding modal, hidden supporting panel, timeline separator, panel movement, reset, and responsive drawer operations restore a deterministic native focus target |
| Timeline | one mounted cursor/lane list retains authored, predicted, accepted, and committed channels; scrubbing remains projection-only and pointer persistence is coalesced |
| Responsiveness | a real Chromium 320 CSS-pixel trace (400% equivalent from 1280 px) proves zero horizontal document/mount/toolbar overflow, in-width workscreen and timeline, and reachable modality, drawer, timeline, supporting-panel, and panel-menu controls at least 44 px high and wide |
| Command authority | pointer and keyboard intents, toolbar/panel controls, help, rebinding, and native-input reservation all use the same availability-qualified stable command IDs |
| Removal | M0 and M4–M8 regression gates plus M9 source/bundle/DOM inspection find no superseded production path |

## Visual review

The [M9 review manifest](assets/persistent-workspace-m9-review/manifest.json)
binds the [actual full-shell Chromium PNG](assets/persistent-workspace-m9-review/field-focus.png)
and its [measured-geometry companion](assets/persistent-workspace-m9-review/field-focus-geometry.svg)
to the production bundle, production CSS, and accepted Field Focus mockup. The
PNG is a 1440×900 screenshot of the mounted production application after a
fresh deterministic load; it is not reconstructed chrome. The manifest stores
live `getBoundingClientRect` values, computed Tools-panel positioning,
singleton counts, overlap results, real timeline-channel geometry, and the
320-pixel reflow audit. Visual review confirmed that the workscreen remains the
primary surface with both sidebars open, the complete compact toolbar is
visible, Editor Tools remain inside their registered panel, and all four real
timeline channels are visible in the shallow expanded panel.

The accepted capture measures a 1411.219 px layout frame, 208 px left sidebar,
224 px right sidebar, 966.438 × 652 px workscreen, 61.563 px complete toolbar,
and 152 px bottom panel. The workscreen occupies 68.5% of the live frame width.
At 320 × 900 CSS pixels, the document and mounted application both have zero
horizontal overflow; the workscreen and timeline remain within the viewport,
and every qualified access control measures at least 44 × 44 CSS pixels. Exact
unrounded browser evidence remains in the manifest.

## Gates

```text
dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj --no-restore
./scripts/build-client.sh
npm run review:map-editor
npm run review:persistent-workspace-m9
node scripts/test-persistent-workspace-m9-acceptance.mjs
./fake.sh build -t Dev
./fake.sh build -t Test
./fake.sh build -t Verify
```

The M9 gate transitively runs the corrective M0 inventory and M4–M8 migration
qualifications. Test and Verify additionally run .NET/Fable parity, full client
conformance, real-worker checks, production browser smoke, generated-site
browser smoke, documentation accessibility, and documentation publication
integrity.
