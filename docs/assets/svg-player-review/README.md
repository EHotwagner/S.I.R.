# Static SVG battlefield review evidence

These boards are deterministic presentation evidence for the Phase 1 SVG
battlefield, Phase 3 two-heading/exact-overlay review, and Phase 4 renderer
provenance. They are not
simulation authority. Each board comes from an explicit sandbox review fixture
in the production Fable bundle at exact committed tick 24; no interpolation,
replay reconstruction, inferred attention heading, or replay-supplied SVG is
involved. Production replay adaptation continues to omit overlays and second
headings because the current worker projection does not legitimately disclose
either source.

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

- the one-cell and multi-cell square authoritative bases remain separate from
  the fixed square information symbols;
- exact-class glyphs stay upright while the facing wedge moves around the
  perimeter;
- twelve health positions, elevation stacks, detailed `+N`, and stance remain
  distinguishable;
- wall, open-door, and window geometry remains non-color-readable; and
- faction outlines remain distinguishable in the monochrome/pattern board;
- the selected three-segment line-of-sight overlay remains exact and distinct
  across all palettes; and
- the weapon and sensor centre-out pointers remain distinct from their units'
  perimeter body-facing wedges and from ground attention/overlay geometry.

Generated SVG and PNG files are committed so review does not depend on a local
browser or rasterizer. Re-running the generator should reproduce the hashes in
the manifest for the same source state and toolchain.

## Recorded visual review — 2026-07-29

The three 768 × 768 PNGs were inspected at native size:

- one-cell and 2 × 2 square base outlines remain visibly independent of their
  fixed-size square information symbols;
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
  separable at native size;
- the selected exact line-of-sight polyline remains legible over the grid and
  does not obscure the body-facing or class channels;
- unit 1's disclosed weapon pointer and unit 2's disclosed sensor pointer end
  in dots inside the square, while body facing stays a perimeter wedge; aligned,
  offset, and opposed forms therefore cannot be mistaken for one another; and
- the two action traces use transient-layer dashed lines and remain subordinate
  to both the exact ground overlay and unit symbols.

The manifest pins the exact-overlay identity, revision, three measured path
segments, typed second-heading sources, trace count, lane count, and the
2,000/8,000 segment policies. Pure tests independently prove the selected
overlay stays exact at 1,999 segments while an 8,001-segment whole-force
overlay aggregates to four segments, and that interpolation at alpha 1 is
identical to the exact committed scene.

Phase 4 also pins the production bundle SHA-256, persistent baseline/fork
labels, linked comparison state, divergence/bookmark inspection features, safe
renderer version, PNG derivation rule, complete evidence provenance fields,
and the forbidden replay-controlled payload classes. The palette boards remain
visual-review evidence; the dedicated closed exporter independently generates
download artifacts without serializing these DOM trees.

These boards validate the representative detailed frame, not every target
size. The pure tests separately exercise threshold hysteresis and suppression
at overview/standard tiers; a future browser screenshot matrix can extend the
human review across every catalog glyph and heading.
