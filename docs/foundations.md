---
title: Attributes and State
category: Foundations
categoryindex: 2
index: 1
description: The atomic facts that compose equipment, units, formations, and battlefield decisions.
status: accepted
document-type: reference
---

# Attributes and state

Every complex battlefield object is assembled from a small set of typed,
bounded facts. Start here: a unit is easier to understand after health,
position, facing, attention, knowledge, and capability have distinct meanings.

## The composition rule

```text
identity + physical state + condition + knowledge + capabilities + control
                                  ↓
                                unit
```

No layer silently invents a fact from the next layer. A renderer may show body
facing only when body facing exists; a controller may act only on knowledge
available to that unit; equipment grants capabilities without changing the
unit's permanent class.

## Atomic attributes

<div class="sir-definition-grid">
  <section><span>Identity</span><h3>Who is this?</h3><p>A stable unit identity, ownership, faction, and class.</p></section>
  <section><span>Geometry</span><h3>Where is it?</h3><p>Grid position, footprint, elevation, body facing, and attention.</p></section>
  <section><span>Condition</span><h3>Can it continue?</h3><p>Health, wounds, suppression, readiness, stance, and current action.</p></section>
  <section><span>Knowledge</span><h3>What does it know?</h3><p>Local observations plus reports that physically reached it.</p></section>
  <section><span>Capability</span><h3>What can it do?</h3><p>Class competence, equipment, resources, traits, and abilities.</p></section>
  <section><span>Control</span><h3>What will it attempt?</h3><p>One isolated controller chooses from disclosed observations and legal actions.</p></section>
</div>

### Bounded quantities

Simulation quantities are fixed-size integers or fixed-point values with
explicit bounds. Health cannot exceed its maximum; suppression cannot grow
without limit; a tick is an exact integer. Presentation-only angles and pixels
never cross back into authority.

### Disclosed facts

A fact can be disclosed, explicitly unknown, not applicable, or not present.
Those states are different. “Not present” does not mean zero, and a perspective
client cannot fill a missing value from an earlier frame.

## From attributes to a unit

<figure class="sir-explainer sir-unit-anatomy" data-svg-explainer>
  <div class="sir-explainer-heading">
    <div>
      <p class="sir-kicker">Composed object</p>
      <h2>Anatomy of a unit</h2>
    </div>
    <button class="sir-motion-toggle" type="button" aria-pressed="false">Pause motion</button>
  </div>
  <svg viewBox="0 0 900 500" role="img" aria-labelledby="unit-title unit-desc">
    <title id="unit-title">A unit composed from six independent information channels</title>
    <desc id="unit-desc">A central square unit symbol is connected to identity, geometry, condition, knowledge, capability, and control nodes.</desc>
    <defs>
      <marker id="unit-arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="7" markerHeight="7" orient="auto">
        <path d="M0 0 L10 5 L0 10z" class="sir-arrow-head"/>
      </marker>
      <pattern id="unit-grid" width="34" height="34" patternUnits="userSpaceOnUse">
        <path d="M34 0H0V34" class="sir-grid-line"/>
      </pattern>
    </defs>
    <rect x="286" y="76" width="328" height="328" rx="24" class="sir-board"/>
    <rect x="286" y="76" width="328" height="328" rx="24" fill="url(#unit-grid)"/>
    <path class="sir-attention-cone" d="M450 240 L555 165 A145 145 0 0 1 586 240 Z"/>
    <rect x="398" y="188" width="104" height="104" rx="10" class="sir-footprint"/>
    <rect x="414" y="204" width="72" height="72" rx="7" class="sir-unit-body"/>
    <path d="M450 208 l-10 18 h20z" class="sir-facing"/>
    <path d="M450 240 l30 20" class="sir-secondary-heading"/>
    <circle cx="480" cy="260" r="5" class="sir-secondary-dot"/>
    <g class="sir-health-ring">
      <path d="M409 196h12 M426 196h12 M443 196h12 M460 196h12 M477 196h12"/>
      <path d="M494 211v12 M494 228v12 M494 245v12 M477 284h-12 M460 284h-12 M443 284h-12 M426 284h-12"/>
    </g>
    <text x="450" y="249" text-anchor="middle" class="sir-unit-letter">R</text>
    <g class="sir-attribute-node" tabindex="0" data-svg-tip="Stable identity, faction, class, and ownership.">
      <rect x="28" y="42" width="190" height="70" rx="14"/>
      <text x="48" y="72" class="sir-attribute-label">IDENTITY</text>
      <text x="48" y="94" class="sir-attribute-value">Human · Rifleman · #104</text>
    </g>
    <g class="sir-attribute-node" tabindex="0" data-svg-tip="Position, footprint, elevation, body facing, and attention are separate geometry facts.">
      <rect x="28" y="214" width="190" height="70" rx="14"/>
      <text x="48" y="244" class="sir-attribute-label">GEOMETRY</text>
      <text x="48" y="266" class="sir-attribute-value">B4 · 1×1 · level 0</text>
    </g>
    <g class="sir-attribute-node" tabindex="0" data-svg-tip="Health, wounds, suppression, readiness, stance, and action phase.">
      <rect x="28" y="386" width="190" height="70" rx="14"/>
      <text x="48" y="416" class="sir-attribute-label">CONDITION</text>
      <text x="48" y="438" class="sir-attribute-value">9/12 HP · ready</text>
    </g>
    <g class="sir-attribute-node" tabindex="0" data-svg-tip="Only local observations and physically delivered reports are available to control.">
      <rect x="682" y="42" width="190" height="70" rx="14"/>
      <text x="702" y="72" class="sir-attribute-label">KNOWLEDGE</text>
      <text x="702" y="94" class="sir-attribute-value">2 contacts · 1 report</text>
    </g>
    <g class="sir-attribute-node" tabindex="0" data-svg-tip="Class, weapon, armour, sensors, tools, resources, and learned abilities.">
      <rect x="682" y="214" width="190" height="70" rx="14"/>
      <text x="702" y="244" class="sir-attribute-label">CAPABILITY</text>
      <text x="702" y="266" class="sir-attribute-value">Rifle · radio · smoke</text>
    </g>
    <g class="sir-attribute-node" tabindex="0" data-svg-tip="One isolated control module chooses legal actions from this unit's disclosed state.">
      <rect x="682" y="386" width="190" height="70" rx="14"/>
      <text x="702" y="416" class="sir-attribute-label">CONTROL</text>
      <text x="702" y="438" class="sir-attribute-value">Observe → move → cover</text>
    </g>
    <g class="sir-composition-lines" marker-end="url(#unit-arrow)">
      <path d="M218 77 C300 77 330 160 406 211"/>
      <path d="M218 249 H387"/>
      <path d="M218 421 C300 421 330 320 406 270"/>
      <path d="M682 77 C600 77 570 160 494 211"/>
      <path d="M682 249 H513"/>
      <path d="M682 421 C600 421 570 320 494 270"/>
    </g>
  </svg>
  <figcaption>Each callout is independently inspectable. The visual symbol summarizes disclosed facts; it does not become their source.</figcaption>
</figure>

## Composition order

1. [Combat values and formulas](gameplay-formulas.md) define how bounded facts
   relate.
2. [Weapons and equipment](gameplay-weapons-equipment.md) package capabilities
   and resources.
3. [Units and progression](gameplay-units.md) combine identity, state,
   equipment, knowledge, and control.
4. [Formations and referents](formations-and-referents.md) coordinate units
   without erasing their autonomy.
5. [Command and information](gameplay-command-information.md) explains how
   intent and observations move between them.

## Invariants worth carrying forward

- Body facing, attention, and a weapon or sensor heading are different facts.
- Class is permanent competence; equipment is a reversible capability.
- A footprint is physical occupancy; the square symbol is information.
- A report is not world truth—it is knowledge with a source and delivery path.
- Animation is presentation between ticks; committed state changes only at an
  exact tick.
