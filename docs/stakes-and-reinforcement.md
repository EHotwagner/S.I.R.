---
title: S.I.R. Stakes and Reinforcement
status: proposed
document-type: living-design
version: "0.1"
related:
  - docs/mission-lifecycle.md
  - docs/game-vision.md
  - docs/setting-and-factions.md
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

## What it must not do

### It must not buy people

Personnel are persistent individuals, recruited one at a time, with their own
progression and history. A currency that purchases soldiers would undo that.

**The currency buys insertion, not people.** What a commander pays for is
getting more of their own roster into a fight already in progress — transport,
a landing, an opened corridor — and those people can then die like anyone else.
That keeps the stake meaningful in two currencies at once: capital and lives.

### It must not make reinforcing unconditional

Invariant 13 applies directly. If committing more is always correct, the pot is
a tax on attention rather than a decision.

It survives only if declining is sometimes right, which requires that
reinforcing into a losing position genuinely compounds the loss. The mechanism
does that by construction — the stake is forfeited with the match — but the
numbers have to be set so that a player who reads the situation correctly and
folds is rewarded for it.

### It must not snowball across a campaign

Winning pots yields currency, which funds larger stakes, which wins more pots.
That is a campaign-level snowball even while the mechanic prevents a match-level
one.

The existing structure bounds it: campaigns are fixed-duration and their state
is wiped at the end. Whether that bound is sufficient is a tuning question, and
it is the one most likely to need a second mechanism.

## Decisions this needs

None of these should be settled by inference.

**Winner takes all, or a split?** Taking all is cleaner and more dramatic. A
split — winner takes most, survivors recover something — is gentler and reduces
variance. This is a feel decision.

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

**How do reinforcements physically arrive?** They enter an authoritative
simulation with deployment rules, so arrival needs a place, a delay, and
exposure. A reinforcement that materialises is not consistent with anything else
in the design. This is the piece with the most implementation weight.

## Open parameters

- Stake sizes, and whether they are free-form or banded.
- Whether the pot is visible to participants during a match, which is itself an
  information decision and should probably follow the same disclosure rules as
  everything else.
- Whether reinforcement is limited by roster availability, by insertion
  capacity, or only by currency.
- Timing windows: whether reinforcement is available throughout or only during
  declared phases.
- Whether resource missions use the mechanic at all, given they have no
  opponent to take the pot.
