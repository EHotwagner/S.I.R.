---
title: Simulator
category: Tools & Evidence
categoryindex: 5
index: 1
description: Edit maps, run curated deterministic simulations, inspect replay walkthroughs, and test rule parameters.
---

Use the application-level **Samples** tab for curated maps, live controller
simulations, and deterministic replay walkthroughs. The initial collection
includes a three-rifleman versus armored-troll assault, a closed-door breach,
and a typed-objective crossing. Sample walkthroughs carry sandbox disclosure;
load a `.sirr` package in **Replay** when verified replay evidence is required.

The **Rules** workspace also exposes the first executable combat corpus. Open a
rule to inspect its formula or registered implementation, rationale,
dependencies, evidence, and pinned source revision. The catalog is a rendering
of the same F# registry used by simulation and Fable; changing displayed JSON
does not change gameplay. Only engagement preparation, exposed-footprint trace,
armor retention, expected damage, representative weapon/body facts, and attack
resolution are migrated in this slice; other laboratory mechanics remain
explicitly legacy.

<div id="sir-replay-app" aria-label="S.I.R. simulator">
  <p>Loading the Fable browser application…</p>
</div>

<noscript>
  <div class="sir-status sir-status-warning">
    <strong>JavaScript is disabled.</strong>
    The simulator requires JavaScript. The documentation and API reference
    remain available from the sidebar.
  </div>
</noscript>
