# Provenance

The archive was assembled on 2026-07-30 from local FS.GG repositories. Files
copied from source repositories are preserved verbatim; only `README.md` and
this provenance record were authored for S.I.R.

The copied FS.GG source and documentation were published under the MIT License.
Its copyright notice and terms are preserved in `SOURCE-LICENSE-MIT.txt`.

## Retired input package

Source repository:
`FS-GG/FS.GG.Rendering`

Archived revision:
`3fa9752341affb6f218a53a60e518deb79df48cd^`

This is the parent of the deletion commit, and therefore the final revision in
which the complete `FS.GG.UI.Input` package and its tests coexist.

| Archived path | Original path |
|---|---|
| `historical/src/Input/Input.fsproj` | `src/Input/Input.fsproj` |
| `historical/src/Input/KeyboardInput.fs` | `src/Input/KeyboardInput.fs` |
| `historical/src/Input/KeyboardInput.fsi` | `src/Input/KeyboardInput.fsi` |
| `historical/src/Input/README.md` | `src/Input/README.md` |
| `historical/tests/Input.Tests/Input.Tests.fsproj` | `tests/Input.Tests/Input.Tests.fsproj` |
| `historical/tests/Input.Tests/KeyboardInputTests.fs` | `tests/Input.Tests/KeyboardInputTests.fs` |
| `historical/tests/Input.Tests/Program.fs` | `tests/Input.Tests/Program.fs` |

Deletion/retirement commit:
`3fa9752341affb6f218a53a60e518deb79df48cd`
(`179: placement & orphan decisions — code-health refactoring phase 2`)

The retirement rationale is also present in:

```text
FS.GG.Rendering/
docs/reports/2026-06-21-05-19-code-health-refactoring-analysis-and-plan.md
section 4.5, "Competing / orphaned abstractions"
```

## Architectural context

| Archived path | Repository and original path | Source revision |
|---|---|---|
| `context/adr-0028-keyboard-input-config-boundary.md` | `FS-GG/.github/docs/adr/0028-keyboard-input-config-mechanism-policy-boundary.md` | local `main` at archive time |
| `context/design-and-controls.md` | `FS-GG/FS.GG.Rendering/docs/imported/design-and-controls.md` | local `main` at archive time |
| `context/current-keyboard-input-guidance.md` | `FS-GG/FS.GG.Rendering/template/product-skills/fs-gg-keyboard-input/SKILL.md` | local `main` at archive time |
| `context/InputCommand.fsi` | `FS-GG/FS.GG.Game/src/Game.Core/InputCommand.fsi` | local `main` at archive time |
| `context/DefaultKeymap.fsi` | `FS-GG/FS.GG.Game/src/Game.Render/DefaultKeymap.fsi` | local `main` at archive time |

Exact local source commits at the time of archival:

```text
FS.GG.Rendering: 57c670b73684ade90307fca9e26edefde821da00
FS.GG.Game:      566667e71ebd676dacce85e0ffe7fea9aad0ad7e
FS-GG/.github:   ca93b71deb91d2d34bf92f23070e3e8a31377ff0
```

The retired files are historical reference material. Later context documents
may evolve in their owning repositories; this directory is a dated snapshot,
not a synchronization contract or frozen mirror.
