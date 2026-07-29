---
title: Field Manual
category: Start
categoryindex: 1
index: 1
description: Learn the tactical model from its smallest facts to complete forces, battles, and verified replays.
---

# Understand the battlefield from the inside out

S.I.R. is a deterministic tactical skirmish game. This manual starts with the
smallest authoritative facts—attributes, position, facing, health, and
knowledge—then composes them into equipment, units, formations, forces, and
missions. Architecture and verification material come afterwards, when the
game objects they protect are already familiar.

<div class="sir-hero-actions">
  <a class="sir-action sir-action-primary" href="foundations.html">Start with attributes</a>
  <a class="sir-action" href="interactive-rules-lab.html">Open the replay lab</a>
</div>

<figure class="sir-explainer sir-system-map" data-svg-explainer>
  <div class="sir-explainer-heading">
    <div>
      <p class="sir-kicker">The game model</p>
      <h2>Facts become decisions</h2>
    </div>
    <button class="sir-motion-toggle" type="button" aria-pressed="false">Pause motion</button>
  </div>
  <svg viewBox="0 0 1040 360" role="img" aria-labelledby="model-map-title model-map-desc">
    <title id="model-map-title">The game model from attributes to mission outcomes</title>
    <desc id="model-map-desc">Seven linked stages show attributes composing state, equipment, units, formations, forces, and missions.</desc>
    <defs>
      <marker id="sir-arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse">
        <path d="M 0 0 L 10 5 L 0 10 z" class="sir-arrow-head"/>
      </marker>
      <linearGradient id="sir-signal" x1="0" x2="1">
        <stop offset="0" stop-color="#67e8f9"/>
        <stop offset="1" stop-color="#fbbf24"/>
      </linearGradient>
    </defs>
    <path class="sir-map-line" marker-end="url(#sir-arrow)" d="M145 175 H900 Q970 175 970 241"/>
    <circle class="sir-signal-pulse" cx="145" cy="175" r="7"/>
    <g class="sir-map-node" tabindex="0" data-svg-tip="Bounded numbers such as health, readiness, proficiency, and suppression.">
      <rect x="30" y="118" width="140" height="114" rx="18"/>
      <text x="100" y="157" text-anchor="middle" class="sir-node-index">01</text>
      <text x="100" y="187" text-anchor="middle" class="sir-node-title">Attributes</text>
      <text x="100" y="209" text-anchor="middle" class="sir-node-note">bounded facts</text>
    </g>
    <g class="sir-map-node" tabindex="0" data-svg-tip="Attributes plus position, facing, attention, stance, and local knowledge form state.">
      <rect x="190" y="78" width="140" height="114" rx="18"/>
      <text x="260" y="117" text-anchor="middle" class="sir-node-index">02</text>
      <text x="260" y="147" text-anchor="middle" class="sir-node-title">State</text>
      <text x="260" y="169" text-anchor="middle" class="sir-node-note">at one tick</text>
    </g>
    <g class="sir-map-node" tabindex="0" data-svg-tip="Weapons, armour, sensors, communications, tools, and finite resources add capabilities.">
      <rect x="350" y="158" width="140" height="114" rx="18"/>
      <text x="420" y="197" text-anchor="middle" class="sir-node-index">03</text>
      <text x="420" y="227" text-anchor="middle" class="sir-node-title">Equipment</text>
      <text x="420" y="249" text-anchor="middle" class="sir-node-note">capabilities</text>
    </g>
    <g class="sir-map-node sir-map-node-emphasis" tabindex="0" data-svg-tip="A unit owns state, equipment, knowledge, actions, and one controller.">
      <rect x="510" y="66" width="140" height="166" rx="18"/>
      <path d="M555 113 h50 v50 h-50 z M567 125 h26 v26 h-26 z" class="sir-unit-glyph"/>
      <text x="580" y="190" text-anchor="middle" class="sir-node-title">Unit</text>
      <text x="580" y="212" text-anchor="middle" class="sir-node-note">acting object</text>
    </g>
    <g class="sir-map-node" tabindex="0" data-svg-tip="Formation stations and referents coordinate several autonomous units without merging their knowledge.">
      <rect x="670" y="158" width="140" height="114" rx="18"/>
      <text x="740" y="197" text-anchor="middle" class="sir-node-index">05</text>
      <text x="740" y="227" text-anchor="middle" class="sir-node-title">Formation</text>
      <text x="740" y="249" text-anchor="middle" class="sir-node-note">coordination</text>
    </g>
    <g class="sir-map-node" tabindex="0" data-svg-tip="A force combines formations, command links, logistics, faction capabilities, and shared intent.">
      <rect x="830" y="78" width="140" height="114" rx="18"/>
      <text x="900" y="117" text-anchor="middle" class="sir-node-index">06</text>
      <text x="900" y="147" text-anchor="middle" class="sir-node-title">Force</text>
      <text x="900" y="169" text-anchor="middle" class="sir-node-note">combined arms</text>
    </g>
    <g class="sir-mission-node" tabindex="0" data-svg-tip="Mission outcomes emerge from committed actions resolved against the same deterministic world.">
      <circle cx="970" cy="286" r="43"/>
      <path d="M951 286 l13 13 27-31"/>
      <text x="970" y="345" text-anchor="middle" class="sir-node-title">Mission</text>
    </g>
  </svg>
  <figcaption>Focus or hover any stage for its role. The moving pulse is explanatory only; authoritative time advances in discrete ticks.</figcaption>
</figure>

## Read in layers

<div class="sir-card-grid">
  <a class="sir-card" href="foundations.html">
    <span class="sir-card-index">01 · Foundations</span>
    <strong>Attributes and state</strong>
    <span>Learn the atomic facts before meeting a unit.</span>
  </a>
  <a class="sir-card" href="gameplay-weapons-equipment.html">
    <span class="sir-card-index">02 · Composition</span>
    <strong>Capabilities and equipment</strong>
    <span>See how tools turn facts into possible actions.</span>
  </a>
  <a class="sir-card" href="gameplay-units.html">
    <span class="sir-card-index">03 · Actors</span>
    <strong>Units and forces</strong>
    <span>Compose state, loadout, knowledge, and control.</span>
  </a>
  <a class="sir-card" href="combat-resolution.html">
    <span class="sir-card-index">04 · Systems</span>
    <strong>Battlefield interactions</strong>
    <span>Follow perception, command, combat, and logistics.</span>
  </a>
  <a class="sir-card" href="interactive-rules-lab.html">
    <span class="sir-card-index">05 · Evidence</span>
    <strong>Replay and experiment</strong>
    <span>Inspect exact ticks and clearly labeled simulations.</span>
  </a>
  <a class="sir-card" href="simulation-core-architecture.html">
    <span class="sir-card-index">06 · Engineering</span>
    <strong>Deterministic architecture</strong>
    <span>Understand how the implementation preserves the model.</span>
  </a>
</div>

## One battlefield, four evidence levels

| Surface | What it can establish | Authority |
|---|---|---|
| Design explainer | Meaning and relationships | Explanatory |
| Rules laboratory | Consequences of declared prototype inputs | Exploratory |
| Verified replay | What the accepted engine committed | Verifiable evidence |
| Match host | What actually changes authoritative state | Authoritative |

The interface keeps these levels visibly separate. Animation helps explain a
relationship; it never claims that an interpolated pixel is game state.

## For contributors

The [engineering section](simulation-core-architecture.md) covers the
deterministic kernel, control ABI, public protocol, browser projection, and
performance budgets. Generated [API reference](reference/index.html) remains a
precise companion to the narrative manual.
