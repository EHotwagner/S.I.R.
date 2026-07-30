# Keyboard input algebra archive

This directory preserves the retired `FS.GG.UI.Input` keyboard system and the
design material that explains its place in the wider FS.GG UI architecture.

The historical system was a declarative language for modal keyboard interfaces.
Its central relation was:

```text
(top mode, optional mode state, chord sequence) -> binding outcome
```

At runtime that became a pure state transition:

```text
(mode stack, pressed keys, pending sequence, active layout) + input message
    -> (next runtime, effects)
```

## What the model contains

- **Commands** are semantic actions such as `move.left` or `open.palette`,
  registered independently of physical keys.
- **Layouts** describe physical key positions, their hand/finger/row/column
  geometry, and the labels shown for a named layout.
- **Chords** combine one physical key position with a set of positions that must
  already be held.
- **Modes** provide context. The supported kinds were standard, stateful, popup,
  and temporary-held modes.
- **Bindings** are guarded rules: a mode, optional state, and chord sequence
  produce an outcome.
- **Outcomes** form the closed operation vocabulary: emit a command, change a
  mode's state, switch layout, push a popup, push a held mode, cancel the top
  mode, or do nothing.
- **Effects** report resolved commands, layout-state changes, diagnostics, and
  deterministic input events without performing I/O.

This supports keyboard designs resembling Vim modes, leader-key menus,
temporary navigation layers, state-dependent commands, and discoverable command
palettes rather than only a flat `Ctrl+key -> action` table.

## Important implementation boundary

The types were more ambitious than the working resolver. `BindingDefinition`
contains `Sequence: KeyChord list`, and the runtime contains
`PendingSequence`, but `matchesBinding` only accepts a singleton sequence:

```fsharp
match binding.Sequence with
| [ chord ] -> // match it
| _ -> false
```

Consequently, modes, state guards, chords, held layers, popup lifetimes, layout
switching, diagnostics, display projection, and ergonomic bigram analysis were
implemented. Genuine multi-step sequences were represented but never completed.
`CommandIntent` and `CommandPlan` were likewise mostly future-facing vocabulary.

Do not treat this archive as a supported S.I.R or current FS.GG API.

## Directory map

- [`historical/src/Input/`](historical/src/Input/) is the complete retired
  package at its last revision before deletion: project file, implementation,
  public signature, and package README.
- [`historical/tests/Input.Tests/`](historical/tests/Input.Tests/) is its
  complete test project, including the representative modal YAML configuration.
- [`context/adr-0028-keyboard-input-config-boundary.md`](context/adr-0028-keyboard-input-config-boundary.md)
  records the successor mechanism-versus-policy decision.
- [`context/design-and-controls.md`](context/design-and-controls.md) records the
  rule that semantic controls own input, focus, state, accessibility, and
  command behavior while themes own visual decisions.
- [`context/InputCommand.fsi`](context/InputCommand.fsi) and
  [`context/DefaultKeymap.fsi`](context/DefaultKeymap.fsi) show the surviving
  device-free command vocabulary and the point where product policy meets the
  generic Rendering keymap mechanism.
- [`context/current-keyboard-input-guidance.md`](context/current-keyboard-input-guidance.md)
  is the current generated-product guidance, included to make the distinction
  between the retired algebra and the supported input path explicit.
- [`editor-simulator-modal-input-proposal.md`](editor-simulator-modal-input-proposal.md)
  applies the algebra's design principles to S.I.R without copying its runtime.
- [`editor-simulator-modal-key-vocabulary.md`](editor-simulator-modal-key-vocabulary.md)
  defines the complete proposed keyboard language for terrain, units, edges,
  regions, selection, movement, deletion, and simulator control.
- [`PROVENANCE.md`](PROVENANCE.md) records exact repositories, revisions, and
  source paths.
- [`SOURCE-LICENSE-MIT.txt`](SOURCE-LICENSE-MIT.txt) preserves the license
  shipped by the source Rendering repository.

## Why it was retired

Rendering had two keyboard stacks. This 1,400-line package was referenced only
by its tests, while the smaller `FS.GG.UI.KeyboardInput` package was connected
to SkiaViewer, Controls, Controls.Elmish, samples, templates, and product tests.
The larger stack was removed as an orphaned competing abstraction in Rendering
commit `3fa9752341affb6f218a53a60e518deb79df48cd`.

Retirement says that this implementation had no production owner; it does not
invalidate the underlying UI-design ideas preserved here.
