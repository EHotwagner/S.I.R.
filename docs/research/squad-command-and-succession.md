---
title: Squad Command, Identity, and Succession
status: proposed
document-type: research
version: "0.5"
last-updated: 2026-07-29
related:
  - docs/game-vision.md
  - docs/research/progression-systems.md
references:
  - MENACE
  - United States Army infantry squad
  - United States Marine Corps rifle squad
---

# Squad Command, Identity, and Succession

## Purpose

This document examines how S.I.R. can use leaders to give squads distinctive
tactical identities while maintaining clear succession and preserving every
member as a persistent individual.

## Terminology

An **NCO**, or noncommissioned officer, is a category of trained military leader,
not one universal job inside a squad. Squad leader, assistant squad leader, and
team leader are functional assignments commonly filled by NCOs.

S.I.R. should expose functional assignments directly:

- **Squad Leader (SL):** current commander of the complete squad.
- **Second-in-Command (2IC):** designated first successor; may also command a
  subordinate team.
- **Third-in-Command (3IC):** designated second successor; may also command
  another subordinate team.
- **Team Leader:** commands a subdivision of the squad and may simultaneously
  hold the 2IC or 3IC assignment.
- **Acting Leader:** the currently authoritative holder of squad command, which
  may differ from the originally deployed Squad Leader after succession.

Ranks, classes, and leadership qualifications can constrain who is eligible for
these assignments without replacing the functional role identifiers.

## Real-world reference structures

### United States Army

The United States Army rifle squad uses a Squad Leader and two Team Leaders to
provide command and control. The Squad Leader leads through those Team Leaders,
who directly supervise their teams. It does not require three people with the
same squad-leader title; redundancy is embedded in the subordinate command
structure.

### United States Marine Corps

The United States Marine Corps rifle squad similarly uses fire-team leaders.
Its doctrine explicitly designates the senior fire-team leader as the assistant
squad leader. If the Squad Leader is incapacitated or unavailable, that
assistant assumes the Squad Leader's responsibilities.

### General lesson

There is no universal cross-military title for every succession position.
“Second-in-command,” “assistant squad leader,” and senior team leader can
describe similar functions. Clear predesignation and trained subordinate
leaders matter more to S.I.R. than reproducing one service's titles.

## MENACE reference

MENACE places most persistent differentiation in unique Squad Leaders. Generic
Squaddies add firepower and survivability, while leader attributes, perks,
relationships, special weapons, and progression give the formation its tactical
identity. Squaddies die first and can be replaced from Manpower; the leader can
be incapacitated and permanently lost if not stabilized.

This avoids best-member sorting because ordinary Squaddies do not have deep,
individually optimized builds. S.I.R. cannot copy that abstraction because all
of its members are persistent individuals, but it can adapt the principle that
a leader changes how a group operates together.

## Proposed S.I.R. command-team model

### Declared chain of command

Before deployment, a player configures an ordered succession roster:

```text
Squad Leader
  └─ Second-in-Command / Team Leader A
       └─ Third-in-Command / Team Leader B
            └─ additional eligible successors, if configured
```

The ordered roster is authoritative match state. The canonical client and
public API show the order, current eligibility, acting leader, and any reason a
candidate was skipped.

The standard infantry template requires SL, 2IC, and 3IC assignments. It cannot
deploy while any of those assignments is vacant or filled by an ineligible
person. Other formation types may define different command-team requirements,
but their succession policy must remain explicit.

### Role of WebAssembly control

Player-provided modules execute the handover and change local behavior when
leadership changes. They do not gain authority to invent an invisible chain of
command after contact.

The server selects the first eligible person in the declared succession order.
A player-provided module cannot skip that person in favor of a lower-priority
successor. The module receives the authoritative leadership change and adapts
its local behavior to it.

### Eligibility

Potential authoritative eligibility inputs include:

- alive, conscious, and operational state;
- membership in or attachment to the squad;
- leadership qualification or class capability;
- communications reachability;
- physical proximity, if required by the rules;
- possession of required command equipment; and
- effects such as suppression, panic, magic, jamming, or incapacitation.

Local leadership eligibility and HQ communications must remain separate.
Assuming command does not create a radio. The acting leader must already possess
a suitable device, receive one, or recover the former leader's device to restore
the equipment path to headquarters.

## Squad identity model

S.I.R. should separate three sources of identity:

1. **Personal identity:** member class, attributes, abilities, equipment,
   injuries, experience, and history.
2. **Leader-dependent identity:** command style, reactions, formation rules,
   coordination, communications, combined actions, and logistics behavior
   provided by the acting leader.
3. **Squad identity:** shared tactical practice, cohesion, history, traditions,
   and rehearsed behaviors that persist independently of one leader, if this
   layer is adopted.

Leader effects should generally change relationships and behavior rather than
apply universal percentage bonuses. Examples include:

- which reaction plans the squad can coordinate;
- how sectors of attention are assigned;
- whether members can perform a synchronized breach or ambush;
- how reports are summarized and forwarded;
- how logistics priorities are resolved under scarcity;
- how quickly formations reorganize after disruption; and
- what configured behavior is used when contact with HQ is lost.

This creates reasons to assemble different squads without reducing construction
to placing the highest-stat individuals together.

### Primary and secondary command effects

Each command-qualified individual can provide a leadership package with two
expressions:

- A **primary effect** represents how that person commands the complete squad.
  It is active only while the person is the acting Squad Leader.
- A **secondary effect** represents the contribution that person makes as 2IC,
  3IC, or Team Leader. It should be narrower, weaker, more local, or more
  conditional than the primary effect.

Secondary effects may apply to a subordinate team, a defined function, or the
complete squad depending on their contract. Examples include maintaining a
rear-security sector, improving report handling for one team, coordinating a
weapon group, preserving cohesion during movement, or supervising a logistics
task.

When succession occurs:

1. the former acting leader's primary effect ends;
2. the first eligible successor becomes acting Squad Leader;
3. that person's primary effect activates;
4. surviving subordinate roles and their secondary effects are recalculated;
   and
5. the brief handover disturbance is applied.

Primary and secondary effects must declare their scope, prerequisites,
conflicts, combination rules, and WASM-visible behavioral capabilities. The
system should avoid unrestricted stacking of generic numerical bonuses.

### Emergent squad identity hypothesis

A squad should receive a stable identity record when created, but its meaningful
cohesion, traditions, and squad-level traits should provisionally emerge through
shared training, missions, successes, failures, casualties, and command
transitions.

This does not mean a new squad begins without competent control behavior. The
player can assign a functional standard-module posture immediately. Emergent
identity represents how the squad becomes distinctive in practice, not whether
it can perform its basic role.

Potential outputs include:

- familiarity with a rehearsed formation or reaction plan;
- a history-shaped response to ambush, isolation, casualties, or supply loss;
- a narrow team maneuver the squad has repeatedly performed together;
- cohesion that reduces handover disturbance;
- a tradition connected to a past leader or defining mission; and
- compatibility with particular command styles, equipment, or environments.

This layer is promising but risky. It can fail by becoming opaque, generating
arbitrary bonuses, punishing roster changes, encouraging repetitive farming,
or producing so many interacting modifiers that neither a human nor a WASM
policy can explain squad behavior.

The prototype therefore needs the following guardrails:

- core squad competence comes from class, members, equipment, module
  configuration, and command roles rather than accumulated identity;
- emergent traits are few, visible, and behaviorally meaningful;
- every change has an inspectable cause and machine-readable effect;
- farming the same safe action cannot generate unlimited identity progression;
- roster changes do not render a squad unusable; and
- the entire layer can be disabled for comparison without redesigning combat.

Acceptance should depend on whether emergent identity creates memorable,
strategically different squads more effectively than a simpler selected
posture/configuration and leader-effect system.

## Succession consequences

When command changes:

- basic individual capabilities remain available;
- persistent squad history and unlocked capabilities remain with the squad;
- leader-dependent capabilities resolve against the new acting leader;
- incomplete coordination may be cancelled, delayed, or degraded;
- the squad's control modules receive an authoritative leadership-change event;
- communications are recalculated from actual equipment and topology; and
- the client receives the change only if the resulting report reaches HQ.

The severity and duration of handover disruption remain prototype questions.
The intended severity is low: succession should introduce a short, readable
coordination disturbance without making the squad broadly ineffective or
discarding its established plans.

## Design questions

- Can the player override the declared succession order during a match if a
  command reaches the squad?
- Which leader-dependent capabilities transfer immediately, degrade, or require
  preparation under a successor?
- What brief handover effect communicates disruption without creating a severe
  capability loss?
- Which secondary effects apply to one team, one command function, or the whole
  squad, and how do conflicting effects combine?
- How do roster changes preserve, dilute, or transfer emergent cohesion and
  traditions?

## Sources

- [U.S. Army ATP 3-21.8, Infantry Platoon and Squad](https://home.army.mil/benning/8117/7919/3106/20260316_ATP_3-21.8.pdf)
- [U.S. Marine Corps MCRP 3-10A.4, Marine Rifle Squad](https://www.marines.mil/Portals/1/Publications/MCRP%203-10A.4.pdf)
- [MENACE official Squaddies rules](https://wiki.hoodedhorse.com/MENACE/Squaddies)
- [MENACE official character rules](https://wiki.hoodedhorse.com/MENACE/Characters)
