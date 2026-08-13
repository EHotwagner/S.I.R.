# Tactical analysis overlays

The battlefield's analysis layer is a renderer-neutral projection over the already-disclosed tactical scene. It never evaluates line of sight, cover, movement, damage, or command state in the browser. Exact LOS geometry, including corner and door blocker semantics, is retained only when authority supplied it in the disclosed scene.

The canonical registry lives in `TacticalSceneProjection.overlayRegistry`. Each entry owns its stable ID, label, category, order, command ID, supported modes, default mode, availability, disclosure policy, and payload kind. The View menu and command resolver are projections of that registry; `spatial.exact-los` has the default effective shortcut `Alt+L`.

Modes are `off`, momentary inspect-hold, selection-scoped, and persistent. Unsupported restored modes fall back to the registry default, malformed preference documents fail closed, and unknown IDs are ignored for forward compatibility. Overlay preferences use their own `sir.tactical-overlays.v1` storage entry and do not alter layout or command-customization persistence.

Projection applies the disclosure envelope before mapping any payload. A failed envelope produces no overlay shapes or labels and performs no per-item disclosure filtering. Successful projection is deterministic by registry order, subject, and primitive ID, capped at 4,096 payloads and 256 labels. The 200-unit qualification workload remains below the 20 ms projection budget; the production journey also verifies registry traversal, disclosure-pass counters, non-colour SVG patterns, and usability at 400% zoom.
