---
title: S.I.R. Mission Lifecycle and Delivery Sequence
status: proposed
document-type: living-design
version: "0.3"
last-updated: 2026-07-27
related:
  - docs/game-vision.md
  - docs/logistics-architecture.md
  - docs/wasm-control-architecture.md
  - docs/setting-and-factions.md
---

# S.I.R. Mission Lifecycle and Delivery Sequence

## Purpose

This document separates the canonical target architecture for missions and
persistent campaign write-back from the implementation order used to reach it.
The target architecture must not force the project to build campaign,
matchmaking, bidding, and persistence before the tactical game is proven.

## Development priority

The first major playable foundation is a robust skirmish mode supporting both
single-player and multiplayer missions.

“Skirmish” is not a disposable combat sandbox. It must exercise the real:

- authoritative fixed-step simulation;
- grid, movement, collision, perception, combat, and logistics rules;
- WASM control architecture;
- client and public API;
- deployment and force validation;
- objectives and match resolution;
- networking and reconnection;
- deterministic replay and audit;
- mode and ruleset manifests; and
- isolated outcome record.

Persistent campaign state, portal bidding, hidden participant allocation,
scheduled major-mission cadence, and transactional campaign write-back follow
only after this foundation is reliable.

The concrete milestone and acceptance plan is maintained in
[Robust Skirmish Development Plan](skirmish-development-plan.md).

## Delivery sequence

### Stage 1: authoritative skirmish kernel

Build a complete match lifecycle using scenario-provided or standardized catalog
forces:

```text
mode selection
  → force validation
  → deployment
  → live simulation
  → objective resolution
  → outcome record
  → replay
```

The first kernel needs deterministic local execution and automated test
scenarios before it needs persistent accounts or campaign rewards.

### Stage 2: single-player missions

Run the same authoritative server and API against server-controlled opposition.
Single-player is not a separate simulation implementation.

This stage validates:

- player and standard WASM control;
- PvE behavior;
- objectives and deployment;
- continuous real-time flow;
- mission termination;
- casualties and resource consumption inside a match;
- outcome explanation; and
- deterministic replay.

Results are isolated unless a later scenario mode explicitly declares
persistence.

### Stage 3: multiplayer skirmish

Add direct known-participant multiplayer using standardized catalog or
scenario-provided forces. This stage validates:

- network input and output;
- authoritative knowledge filtering;
- simultaneous action resolution;
- reconnect behavior;
- hidden enemy force composition;
- per-player observations and reports;
- WASM fairness and fuel;
- victory and draw handling; and
- replay permissions.

Participants may be known before these early matches. Hidden co-allocation is
not required to validate the multiplayer combat foundation.

### Stage 4: robust mission framework

Generalize skirmish into reusable single-player and multiplayer mission
structures supporting:

- PvE;
- PvP;
- cooperation;
- mixed PvPvE;
- scenario-defined forces;
- standardized point-catalog forces;
- varied objectives;
- deployment and extraction;
- partial success; and
- mode-specific outcome policy.

The standardized skirmish mode remains a maintained competitive, testing, and
benchmark environment after campaign play exists.

### Stage 5: persistent campaign boundary

Introduce account and campaign namespaces, persistent personnel, asset
reservation, mission commitment snapshots, and exactly-once write-back.

Campaign persistence consumes the already proven match outcome record. It does
not permit the match simulation to modify campaign records directly.

### Stage 6: scheduled hidden major missions

Add portal opportunities, bidding, half-hour scheduling, hidden participant
allocation, uncertain PvPvE contact, and campaign consequences only after
ordinary multiplayer mission execution and persistence are robust.

## Canonical target lifecycle

The long-term persistent lifecycle is:

```text
opportunity published
  → eligibility and bid
  → hidden allocation
  → commitment and lock-in
  → deployment
  → live simulation
  → extraction or deadline
  → authoritative outcome ledger
  → campaign write-back
  → aftermath
```

Skirmish and nonpersistent mission modes use a subset:

```text
mode or scenario selected
  → force validation
  → deployment
  → live simulation
  → resolution
  → isolated outcome record and replay
```

## Opportunity and pre-match disclosure

A persistent mission opportunity can disclose:

- mission type;
- known objectives;
- known environment or portal facts;
- entry window;
- deployment limits;
- bid rules;
- potential rewards;
- known risks;
- ruleset and content versions; and
- persistence and write-back policy.

Unknown map state, hostile composition, other bidders, participant count,
co-allocation, and other players' deployment remain hidden unless an explicit
mode says otherwise.

Ordinary direct skirmish can instead identify its known participants while
still hiding their selected force composition and deployment according to
competitive rules.

## Bid and hidden allocation

In the persistent target mode, a player sees only the information needed to
form their own bid and the permitted status of that bid.

The system does not expose:

- other bidders;
- bidder count;
- competing values;
- number of accepted players;
- shared-instance allocation;
- other factions or rosters; or
- other deployment locations.

The bid instrument and selection algorithm remain open.

Each accepted participant receives an opaque mission-session handle and a
knowledge-scoped projection of the shared world. APIs, identifiers, lobby
structures, spawn metadata, and allocation messages must not provide a reliable
semantic side channel for hidden participation.

## Commitment and asset reservation

Persistent mission entry creates an immutable commitment snapshot containing:

- campaign and account scope;
- personnel;
- squad organization and succession;
- progression and injuries;
- equipment and logistics manifest;
- vehicles;
- module artifacts and assignments;
- instance configuration, the per-unit data an artifact is assigned with,
  previously referred to only as initial policies;
- mode and ruleset versions; and
- the campaign-state version being reserved.

Committed assets cannot participate in another campaign operation until the
mission transaction releases them. Module artifacts and assignments cannot be
replaced after live play starts.

Nonpersistent skirmish uses an immutable force snapshot without reserving or
mutating campaign assets.

## Deployment

Deployment is an authoritative phase with a deadline. It validates:

- eligible force and point or deployment budget;
- unit footprints and deployment cells;
- squads and required command roles;
- equipment and supplies;
- module compatibility;
- instance configuration size and structural validity; and
- mode-specific restrictions.

Once live simulation begins, client disconnection does not pause the match.

## Live simulation and reconnection

Server-hosted WASM instances continue operating if the human client
disconnects. Reconnection restores access through the existing HQ communication
endpoint and reveals only information that legitimately reached headquarters.

Disconnection does not:

- pause or roll back the simulation;
- remove units;
- replace custom modules;
- repair communications;
- reveal hidden participants; or
- erase consequences.

## Participant discovery and relationships

Hidden allocation does not itself reveal another player. Discovery occurs
through battlefield perception, reports, physical contact, communications, or
other legitimate mechanics.

The mission framework must support later policies for:

- automatic or conditional hostility;
- cooperation and negotiation;
- betrayal;
- shared or competing objectives;
- faction and campaign relationships;
- company or account identification; and
- reward attribution.

Those relationship policies remain unresolved and are not required for the
first direct multiplayer skirmish.

## Extraction and securing

Mission outcomes apply to individual personnel and assets. Possible results
include:

- extracted;
- secured in a qualifying controlled area;
- active but stranded;
- incapacitated and recovered;
- incapacitated and abandoned;
- captured;
- dead or destroyed; and
- lost in an unresolved portal space.

Extraction is a timed physical action subject to access, interruption, capacity,
and mission rules. Mission completion does not automatically teleport every
survivor and object home.

Early skirmish modes may end through objective resolution without persistent
asset recovery, but the mission framework must be able to represent extraction
before campaign write-back is introduced.

## Outcome record and persistent ledger

Every finished match emits an immutable authoritative outcome record containing
applicable facts such as:

- match, mode, ruleset, and content identity;
- force snapshots;
- objective outcomes;
- personnel outcomes;
- injuries and deaths;
- equipment condition and ownership;
- resource consumption, loss, and recovery;
- rewards and penalties;
- relationship consequences; and
- replay provenance.

For skirmish, the outcome record is retained for result, replay, ranking, or
audit according to its mode and has no campaign mutation.

For persistent play, it becomes the campaign outcome ledger. The ledger is
derived from authoritative simulation events rather than client claims.

## Exactly-once campaign write-back

Persistent write-back is:

- atomic;
- idempotent;
- version checked;
- auditable;
- replayable; and
- scoped to one campaign namespace.

The transaction is:

```text
reserve campaign assets
  → run match
  → finalize ledger
  → verify reservation and campaign version
  → apply ledger exactly once
  → release qualifying assets
```

A stable match identifier prevents duplicate rewards or repeated casualties if
the transaction is retried.

## Failure and recovery

Live mission infrastructure should retain deterministic checkpoints or a
replayable event stream sufficient for the declared recovery policy.

A failure must not let a participant selectively:

- accept or reject a bad outcome;
- replay for a better result;
- duplicate recovered assets; or
- erase casualties.

Checkpoint intervals, unrecoverable-match policy, and compensation remain
operational parameters. The policy must be public and consistent.

## Mode policies

### Resource missions

- Single player against server-controlled opposition.
- Persistent personnel and resources in the eventual campaign mode.
- Ordinary campaign rewards and consequences.
- Authoritative even without another human participant.

### Major missions

- Scheduled on the intended half-hour cadence.
- Can secretly co-allocate several players.
- Carry higher risks, objectives, rewards, and consequences.
- Use the persistent transaction boundary.

### Duel and skirmish

- Use standardized point-catalog or scenario-provided forces.
- Share the production simulation, API, WASM, networking, and replay system.
- Remain completely isolated from persistent campaign state.
- Continue as maintained modes after campaign release.

## Turnaround and automation

A normal 20-minute match inside a 30-minute major-mission rhythm leaves roughly
ten minutes for aftermath, force preparation, bidding, and commitment.

Bulk APIs and automation must support:

- casualty and inventory review;
- squad reorganization;
- supply replenishment;
- loadout selection;
- module and policy assignment; and
- bid preparation.

Time pressure should create strategic choice rather than interface drudgery.

## Open parameters

The architecture does not yet fix:

- bid currency or selection algorithm;
- relationship and hostility rules after player contact;
- precise objective and reward attribution;
- extraction and securing rules by mission type;
- late entry and missed-window behavior;
- checkpoint and unrecoverable-match policy;
- failure compensation;
- expected players per major mission; or
- exact timing of commitment and final lock-in.
