# Static SVG battlefield review evidence

These boards are deterministic presentation evidence for the Phase 1 SVG
battlefield. They are not simulation authority. Each board comes from the
production Fable bundle's exact committed tick 24 frame; no interpolation,
replay reconstruction, or replay-supplied SVG is involved.

Regenerate from the repository root:

```text
npm run build:client
npm run review:battlefield
```

The generator mounts the production application in a DOM, selects each
approved palette, serializes the actual battlefield SVG, and rasterizes it
with `rsvg-convert` at 768 × 768. `manifest.json` records the hashes, board
bounds, tick, palette, 24/48 px semantic thresholds, 10% hysteresis, captured
tier, and interpolation state.

Review targets:

- the one-cell and multi-cell authoritative footprints remain separate from
  the fixed square information symbols;
- exact-class glyphs stay upright while the facing wedge moves around the
  perimeter;
- twelve health positions, elevation stacks, detailed `+N`, and stance remain
  distinguishable;
- wall, open-door, and window geometry remains non-color-readable; and
- faction outlines remain distinguishable in the monochrome/pattern board.

Generated SVG and PNG files are committed so review does not depend on a local
browser or rasterizer. Re-running the generator should reproduce the hashes in
the manifest for the same source state and toolchain.

## Recorded visual review — 2026-07-29

The three 768 × 768 PNGs were inspected at native size:

- one-cell, 2 × 1, 1 × 2, and 2 × 2 outlines remain visibly independent of
  their identically sized square symbols;
- all six class silhouettes remain recognizable and upright, including when
  their perimeter wedges point in different directions;
- all twelve health positions can be counted. Active/depleted positions remain
  distinct in default and high contrast, and use black versus gray values in
  monochrome;
- the level-four `+4`, level-seven `+7`, one-to-three bar stacks, and stance
  initials are legible at the captured detailed tier without moving geometry;
- the wall, rotated open-door leaf, and dotted window remain distinct without
  relying only on hue;
- the monochrome board retains solid human, dashed arcane, and dotted neutral
  outlines even though all three faction colors intentionally collapse to
  black; and
- the high-contrast white grid is the busiest presentation, but thicker symbol
  outlines, health values, facing wedges, and class silhouettes remain
  separable at native size.

These boards validate the representative detailed frame, not every target
size. The pure tests separately exercise threshold hysteresis and suppression
at overview/standard tiers; a future browser screenshot matrix can extend the
human review across every catalog glyph and heading.
