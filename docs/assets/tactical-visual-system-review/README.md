# Tactical visual-system production review

This directory contains the deterministic review package for
`tactical-visual-system-v1`.

- `after-production.png` is an actual Chromium capture of the built production
  shell. `manifest.json` binds it to the exact Fable bundle and stylesheet.
- The `before` entry in the manifest binds the accepted M9 production capture,
  preserving an auditable before/after comparison without duplicating it.
- `density-prototypes.svg` and `.png` show the same production token vocabulary
  at ordinary (20), representative dense (100), and stress (200) unit scales.
  They are design prototypes, not screenshots or simulation truth.
- The focused production browser journey proves live effects, exact ordering,
  reduced-motion causality, disclosure-safe endpoints, structural budgets, and
  animation-frame inspection on the real server route.

Regenerate after building the client:

```shell
npm run review:tactical-visual
npm run test:tactical-visual-review
```
