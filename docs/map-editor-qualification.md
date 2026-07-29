---
title: Map Editor Human Qualification
category: Tools & Evidence
categoryindex: 5
index: 3
status: proposed
decision-status: qualification-required
document-type: test-protocol
version: "1.0"
last-updated: 2026-07-29
description: Reproducible human usability and assistive-technology release protocol for the map editor.
related:
  - docs/map-editor.md
  - docs/2026-07-29-1230-map-editor-vtt-experience-design-report.md
---

# Map Editor Human Qualification

This protocol covers claims that automation cannot honestly establish:
new-user comprehension, time to first valid map, mode errors, screen-reader
comfort, touch ergonomics, and usable reflow at 400% browser zoom.

Before each session, build the current candidate with:

```bash
npm ci
./scripts/test-conformance.sh
./scripts/build-docs.sh
```

Record the commit, browser and version, operating system, input device,
viewport, assistive technology and version, start/end time, mode errors,
undo recoveries, unexplained import fields, and participant comments. Do not
coach beyond: “Create a playable encounter map and send its saved revision to
the simulator.”

## Task script

Ask the participant to complete these tasks without opening documentation:

1. Create a 24×16 map and fit it to the viewport.
2. Paint a rough rectangle, undo it, and redo it.
3. Place goblins, orcs, and a troll with their default footprints.
4. Box-select and duplicate a group, then correct an invalid overlap.
5. Draw a room in one wall gesture, convert one segment to a door, and open it.
6. Find and correct every issue using the issues panel.
7. Save, reload, choose recovery of a newer autosave, and compare the digest.
8. Simulate the immutable revision, edit the draft, and identify stale status.
9. Repeat the essential authoring path using only keyboard and object list.

Pass requires a first valid map under five minutes, no more than one mode error
per task, undo recovery from every accidental mutation, zero unexplained import
loss, identical export/re-import digests, and no state inferred from pixels.

## Accessibility matrix

Run the keyboard task with no pointing device. Then run focused audits with:

- NVDA and Firefox or Chrome on Windows, including object list, issues,
  announcements, unit properties, import review, and simulator handoff;
- VoiceOver and Safari on macOS or iOS for the same reading and operation path;
- touch-only selection, painting, two-finger pan/pinch, long-press actions, and
  cancellation on a device with a viewport no wider than 768 CSS pixels;
- the operating-system forced-colors/high-contrast mode;
- browser zoom at 400%, confirming one-dimensional page flow and no hidden
  command or inspector; and
- reduced-motion preference, confirming immediate camera/state updates and no
  required animation.

## Session record

No human session is recorded yet. Add one row per environment; attach
anonymized notes or issue links rather than participant identity.

| Date | Commit | Environment | Participant profile | First valid map | Mode errors | Result |
|---|---|---|---|---:|---:|---|
| Pending | Pending | Pending | New user | Pending | Pending | **Not qualified** |

The milestone may be marked fully qualified only after the new-user task
session and the keyboard, screen-reader, touch, forced-colors, 400% zoom, and
reduced-motion rows all pass or have tracked blocking defects.
