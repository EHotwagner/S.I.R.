# Focused mutation evidence

- Subject: `ViewportOverscanCells` in `TacticalSceneProjection.fs`.
- Mutation: changed the declared `2.0` cell overscan to `20.0`.
- Command: `dotnet run -c Release --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj --no-restore`.
- Observed result: exit 134; the focused equal-small-viewport structural-budget assertion failed at `TacticalSceneProjectionQualification.fs`.
- Restoration: restored `ViewportOverscanCells = 2.0`; the production value is not mutated in the delivered head.

An exploratory chunk-size mutation (`8.0` to `16.0`) remained green and is not claimed as satisfying mutation evidence; it showed that the acceptance gate correctly targets bounded visible work rather than a particular implementation constant.

## Composed App ownership surface (`scripts/test-composed-app-surface-inversions.mjs`)

Extracting the tactical scene owner out of `App.fs` moved every token it owns
out of the file six source-scanning gates were reading — four of which read
`App.fs` alone. Re-pointing those gates at the composed surface is exactly the
kind of change that silently defangs a check, so the inversion is committed and
executable rather than reported.

- Subject: the six re-pointed ownership gates plus `test-map-editor-qualification`.
- Command: `npm run test:composed-app-surface-inversions` (requires `npm run build:client`
  and current review artifacts, because several gates boot the production bundle
  before reaching their source assertions).
- Mutation: per gate, a real violation of the property that gate guards, introduced
  in `src/SIR.Client.Web/TacticalScenePresentation.fs` — the renamed SVG root, the
  renamed route layer, the renamed presentation coordinate/alpha attributes, an
  obsolete replacement-page branch, a renamed renderer definition, and a resurrected
  retired Editor renderer.
- Observed result: each gate exits non-zero AND names the violated property. The
  harness asserts a GREEN baseline immediately before each mutation, so a gate that
  is red for an unrelated reason cannot be counted as evidence.
- Restoration: every mutation is reverted in a `finally`, and the harness re-reads
  the file afterwards to prove the tree is clean.

Two findings this produced that a transcript of manual runs would have hidden:

- The `m9-acceptance` inversion was initially passing **for the wrong reason** — a
  stale review binding failed before the assertion under test was reached. The
  green-baseline pre-check now refuses that outright.
- `m9-acceptance` **survived** its inversion: its single-owner patterns had no word
  boundary, so renaming `persistentSceneSvg` to `persistentSceneSvgRenamed` still
  matched `let persistentSceneSvg`. Fixed with `\b`. That gate was decorative in
  this respect before the extraction; the extraction only exposed it.

Honest boundary on the proof: `m8-timeline` requires no positive token that moved
out of `App.fs`, so re-pointing it is **inert** for its positive assertions and
strictly stronger for its negative ones. Its inversion demonstrates that the
dead-branch scan now reaches the extracted module — not that a moved M8 token is
still guarded. This is stated in the harness source as well as here.

## Accepted-scene source identity (`TacticalScenePresentation.fs`)

- Subject: the scene owner's `acceptedSourceAccepted` clause.
- Mutation: drop `Simulator` from the identity comparison.
- Command: `npm run build:client && npm run review:tactical-visual`.
- Observed result: committing a second unit's route no longer reaches the DOM —
  `plannedRouteUnits` falls from 2 to 1 and the density boards regain a stale
  `preview` effect lifecycle, because a route changing from preview to planned
  alters no revision term (the count, the editor-derived RevisionIdentity and the
  tick are all unchanged).
- Restoration: the clause is present in the delivered head.

## Board framing under a viewport-pixel viewBox (`MapEditorWorkspace.fs`)

The SVG viewBox moved from BOARD space to VIEWPORT-pixel space so that culling,
pointer hit-testing and the retained camera transform share one finite contract.
A board-sized viewBox centred the board for free through SVG's default
`preserveAspectRatio`; a viewport-pixel one centres nothing. "The board is framed
rather than pinned at the origin" therefore stopped being a geometric consequence
and became a camera property that nothing asserted.

Nothing on this branch caught the loss. It surfaced only as an intercepted click
in an unrelated documentation journey, because the board sat against the SVG's
left edge underneath the sidebar's resize separator.

- Subject: the `ResizeViewport` re-fit for an untouched camera.
- Mutation: replace `if untouched then fitBoard …` with `if false then fitBoard …`.
- Command: `dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release`.
- Observed result: the new framing assertion fails and names the pinned camera —
  `An untouched camera did not frame the board inside the viewport after a resize:
  pan=36.000000,36.000000 zoom=1.000000 board=576.000000x384.000000` — which is the
  untouched initial camera, exactly the regression it guards.
- Restoration: the re-fit is present in the delivered head.

The assertion also pins the other half explicitly: a resize must NOT move a camera
the operator has already moved. Both directions are named, because a fix that
frames the board by overriding a deliberate camera is a trade, not a repair.
