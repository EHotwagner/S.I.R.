---
title: Stakes and Reinforcement
category: Battlefield Systems
categoryindex: 4
index: 19
status: accepted
decision-status: canonical
document-type: living-design
version: "1.3"
related:
  - docs/mission-lifecycle.md
  - docs/game-vision.md
  - docs/setting-and-factions.md
  - docs/communications-network.md
  - docs/arcane-forces.md
last-updated: 2026-07-27
---

# S.I.R. Stakes and Reinforcement

## The proposal

A player may order reinforcements during a mission, paying a distinct currency
to do so. **That currency does not disappear — it enters a common pool, and the
winner takes it.**

Committing more therefore raises what the match is worth and what losing it
costs, simultaneously and for everyone involved.

## Why this is worth taking seriously

### It wagers under fog

This is the strongest argument and it is specific to this game.

Every other wagering mechanic asks a player to bet on a position they can see. A
S.I.R. commander cannot. They hold delayed, partial, possibly stale information
about their own force and considerably less about anyone else's, and the
decision to commit more arrives exactly when the picture is worst.

**The escalation decision therefore runs on the same incomplete information as
everything else in the game.** That is not a bolted-on economy; it is the
central thesis applied to the campaign layer.

### It opens the match without flattening it

A player who starts badly can commit rather than concede, so a poor opening is
not a decided match. But committing is not free optimism: it raises the amount
they forfeit if the read was wrong.

The result is that matches stay live without the outcome being softened.

### It rewards contested matches over walkovers

A pot grows when both sides commit. An easy win pays little; a hard-fought one
pays a great deal. That biases the reward structure toward the matches worth
having.

### It fits the fiction exactly

The player is a mercenary company. Staking capital to keep a contract alive, and
recovering it with interest on success, is what such a company does. Nothing
here requires new fiction.

### It may answer an open question

The vision asks what a portal-access bid commits — currency, resources,
reputation, forces, risk, or reward share — and does not answer it. If the bid
and the stake are the **same currency**, then bidding for access and committing
during a mission become one economy with one scarce resource, and open question
11 is answered rather than joined by a second unanswered one.

## Silence is not death, and the commander cannot tell

The sharpest objection to the mechanic: a player may reinforce because a squad
has gone quiet, when that squad is merely cut off and still fighting.

The ambiguity is real. Casualties are high-significance events and a connected
squad losing people reports it — but a *disconnected* squad's report queues
undelivered, so destruction and isolation both arrive at headquarters as
silence.

**That ambiguity should stay.** It is fog applied to a commander's own force,
which is the most uncomfortable and most interesting fog available, and removing
it would mean telling a player something no one has told them.

What it must not become is a coin flip. The decision has to be **inferential
rather than blind**, which requires that a good commander can reason toward an
answer even when nobody can know one.

### What a commander can reason from

- **Was the squad in contact when it went quiet?** Silence following a contact
  report means something different from silence with no contact reported at all.
- **Is the silence where it was expected?** Signal paths use the same geometry
  as line of sight, so a squad entering a structure or dead ground goes quiet
  predictably, and the map says so in advance.
- **Is headquarters detecting interference in that direction?** Jamming that is
  distinguishable from absence is the most direct evidence available.
- **Was there redundancy?** A second-in-command carrying a spare set changes
  what silence implies.
- **Can anything else see the area?** This is the real answer, and it costs what
  information always costs here — time, a redirected element, and exposure.

### Overcommitment is not free, and saying so was the error

An earlier version of this document argued that reinforcing a squad which turns
out to be alive costs nothing much, because the reinforcements arrive and fight.
That reasoning is wrong, and it is wrong in the direction that breaks the
mechanic.

If more units improve the chance of winning, and winning returns the stake, then
**reinforcing is positive expected value nearly always.** The escalation
decision collapses into "commit the maximum," and the match resolves on who
brought more currency. That is invariant 13 failing outright.

Two costs have to bite for the decision to survive, and neither is optional.

### The pot is not fully returned

A share of every pot is taken by whoever administers portal access — the
authority that licenses mercenary companies, publishes opportunities, and
resolves bids.

That gives committing a cost that survives victory. Raising a stake buys a
better chance at a larger pot and pays a fee for the privilege, so escalating
into a fight you were winning anyway is a small, real loss.

It also answers the vision's twelfth open question in passing, or at least
supplies a reason for the answer to exist: the administrator's cut is what the
administrator is *for*, and it explains why such an institution tolerates
mercenary companies fighting over its portals.

### The force is the real stake, and it does not come back

The stronger constraint, and the one that makes declining genuinely correct
sometimes.

Reinforcement commits **persistent people**. They are recruited individually,
they carry progression and history across a campaign, and losing them is
permanent. No pot compensates that, because the pot is currency and the loss is
personnel.

The campaign cadence sharpens it. Major missions arrive every half hour against
twenty-minute matches, so a force wrecked in one contract is not ready for the
next one. **Winning the pot does not restore people or readiness**, and a
commander who spends both to win a match they were going to win anyway has paid
for it out of the following mission.

So the decision is not "is more force better here" — it always is — but "is this
fight worth the force it will cost me, given what is coming." That is a real
question, and it is one a player can get wrong in both directions.

### Overcommitment must break something, not merely dilute

Even with a fee and permanent personnel losses, a commander weighing *this
fight* against *the next one* can reasonably decide that this fight is the one
that matters, and commit everything. If the only in-match penalty is attention
divided a little further, they should.

So the in-match cost has to be **nonlinear**. Past the capacity of a force's
command structure, the structure fails according to the faction that built it.
This does not cost only the marginal units their effectiveness — it threatens
the **entire force relying on that structure**.

For humans, the command net saturates and collapses. See
[Communications Network](communications-network.md).

For the arcane civilization, the finite anchoring structure becomes
supernaturally unstable. It becomes conspicuous, disrupts the coordination of
the force depending on it, and risks indiscriminate lightning discharge or, at
the severe extreme, an uncontrolled daemon portal hostile to every side. See
[Arcane Civilization Forces](arcane-forces.md).

That converts the reinforcement decision from arithmetic into a threshold
question. More force is better right up until it is catastrophically worse, and
the commander has to know where their own ceiling is.

Three properties make this work rather than merely punish:

- **the cause is deterministic and learnable**, even though the exact message
  lost from a saturated net or hazard released by an unstable anchor is not, so
  a collapse is explicable afterwards rather than arbitrary;
- **capacity can be bought** by investing in the faction's command structure:
  leaders, sets, and relays for humans; additional or stronger anchors,
  components, casters, and defended ground for the arcane. Either competes with
  fighting power and exposure, so the ceiling is a construction decision rather
  than a rule; and
- **load can be shed**. A commander who has overcommitted can send elements home
  and recover the rest, which makes withdrawal a tactical action rather than a
  concession.

An earlier alternative — letting units beyond a cap fall under faction control
rather than the player's — was considered and rejected. Uncontrolled units still
fight for you, so it is not a cost, and it would special-case an architecture in
which every unit has its own control instance.

### An honest consequence, and a declared fallback

Both fixes point the same way: **the currency is the secondary stake and the
force is the primary one.** That is worth stating plainly, because it means the
pot is a framing device for a commitment that would be consequential without it,
rather than the mechanism doing the work.

The fallback follows, and is declared here rather than discovered under
pressure. **If the pot is measured to add nothing beyond framing — if players
commit and decline at the same rates with and without it — the currency is
removed and reinforcement becomes a pure force commitment.** The rest of this
document survives that change intact, because the administrator's cut is the
only part that depends on there being a pot at all.

Recording the fallback is not a hedge against the mechanic. It is the discipline
the performance budget already applies: know in advance what you would do if the
evidence goes the other way, so that a measurement becomes a decision rather
than an argument.

### The exploit is a feature

An opponent who jams a squad *specifically* to make its commander believe it
destroyed, so that they commit capital and stake they did not need to, is
performing **electronic warfare against the campaign economy**.

Nothing else in the design attacks that. Jamming already degrades information
and control; this gives it a third target, and it is available to the human side
in exactly the matchup where their electronic toolkit otherwise applies —
against other humans.

The counterplay is the ordinary one: do not panic, look before committing, and
find the jammer, which is the loudest object on the battlefield.

## What it must not do

### It must not buy people

Personnel are persistent individuals, recruited one at a time, with their own
progression and history. A currency that purchases soldiers would undo that.

**The currency buys insertion, not people.** What a commander pays for is
getting more of their own roster into a fight already in progress — transport,
a landing, an opened corridor — and those people can then die like anyone else.
That keeps the stake meaningful in two currencies at once: capital and lives.

### It must not make reinforcing unconditional

Invariant 13 applies directly, and the naive version of this mechanic fails it —
see above. Forfeiting the stake on a loss is not sufficient, because committing
also raises the chance of not losing.

It survives only through the administrator's cut and the permanence of personnel
losses, and the numbers have to be set so that a commander who reads a situation
correctly and folds is rewarded for it.

### It must not snowball across a campaign

Winning pots yields currency, which funds larger stakes, which wins more pots.
That is a campaign-level snowball even while the mechanic prevents a match-level
one.

The existing structure bounds it: campaigns are fixed-duration and their state
is wiped at the end. Whether that bound is sufficient is a tuning question, and
it is the one most likely to need a second mechanism.

## What canonical status covers

The **mechanism** is settled and later work should build against it:
reinforcement is paid for, payment enters a common pool, a share is taken by the
access administrator, the winner takes the remainder, the committed force is the
primary stake because personnel losses are permanent, and overcommitment
breaks faction-specific command capacity rather than diluting marginal units.
Human nets saturate; arcane anchors become observably unstable and can discharge
indiscriminate lightning or open an uncontrolled daemon portal hostile to every
side. Arcane reinforcements physically arrive through an exposed transit ritual
maintained by a caster quorum. This controlled transit portal transports
precommitted personnel and does not create replacements. Uncontrolled goblin
portals are not reinforcement: their arrivals belong to neither side and consume
no arcane roster, supply, stake, command capacity, or anchor capacity.

The **decisions below are not settled**, and several are genuine forks rather
than values to tune. Canonical status covers the shape, not the numbers, and not
the questions that follow.

## Decisions this needs

None of these should be settled by inference.

**How large is the administrator's cut?** It has to be big enough that
escalating into a won fight is a real loss and small enough that committing
against a genuine threat is still correct. This is the number the mechanic lives
or dies on.

**Winner takes the remainder, or a split among survivors?** Taking the remainder
is cleaner and more dramatic. A split is gentler and reduces variance. This is a
feel decision.

**How does a major mission resolve it?** Major missions may secretly co-allocate
several players, and a player may never meet another. A pool with several
contributors and one winner is coherent for a fought match and strange for one
where nobody found anybody. Resolution when contact never occurs needs its own
rule.

**What does extraction do to a stake?** If a player extracts early they preserve
their people. Forfeiting the stake makes withdrawal a real cost and keeps
extraction from being a free hedge. That seems right but it is a decision.

**Is the stake currency the same as the bid currency?** Unifying them answers
open question 11 and keeps the campaign economy to one scarce resource. Keeping
them separate allows independent tuning. Unification is the stronger option and
the harder one to reverse.

**How do reinforcements physically arrive?** The arcane answer is settled:
multiple casters prepare and maintain a transit portal at an exposed site, and
precommitted personnel cross in one or more waves. The portal transports rostered
units rather than creating them, and arrivals immediately consume local anchor
capacity. Human arrival still needs a place, delay, and exposure; a
reinforcement that materialises is not consistent with anything else in the
design.

## Open parameters

- Stake sizes, and whether they are free-form or banded.
- Whether the pot is visible to participants during a match, which is itself an
  information decision and should probably follow the same disclosure rules as
  everything else.
- Whether reinforcement is limited by roster availability, by insertion
  capacity, or only by currency.
- Arcane transit-portal preparation time, caster quorum, wave cadence, gate
  duration, insertion capacity, exit constraints, and interruption outcomes.
- How faction-specific command load and capacity are exposed to the commander
  before reinforcement is committed.
- The timing and severity of arcane anchor-instability warnings, lightning
  discharge, and uncontrolled daemon breach; their existence and indiscriminate
  character are canonical, while their values and outcome tables are not.
- Timing windows: whether reinforcement is available throughout or only during
  declared phases.
- Whether resource missions use the mechanic at all, given they have no
  opponent to take the pot.
- How reliably interference is distinguishable from absence. This was an open
  parameter of the electronic-warfare model and is now load-bearing, because it
  decides whether the reinforcement decision is inference or a coin flip.
