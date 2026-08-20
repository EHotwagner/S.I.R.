# Penpot UI source

`sir-tactical-workspace.svg` is an editable vector representation of the
production S.I.R. persistent tactical Editor workspace. Its 1440×900 frame is
based on the accepted Field Focus capture in
`../persistent-workspace-m9-review/field-focus.png`.

## Open in Penpot

Create or open a Penpot project, then drag `sir-tactical-workspace.svg` onto the
dashboard or use **Import files**. The imported design keeps vector geometry,
text, colors, and named SVG groups editable. Ungroup a section in Penpot when
individual controls need to be changed.

The intended fonts are Inter for interface text and JetBrains Mono or Fira Code
for tactical labels. Penpot will use the available fallbacks if those fonts are
not installed.

## Layer map

- `00 · Page background`
- `01 · Top toolbar`
- `02 · Left sidebar — Roster and tools`
- `03 · Tactical workscreen`
- `04 · Right sidebar — Selection inspector`
- `05 · Unified tactical timeline`

This file is a design asset, not application runtime source. Changes made in
Penpot do not automatically update the Fable UI.
