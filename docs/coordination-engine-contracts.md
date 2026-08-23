# Coordination engine contracts

How to author the documents `fsgg-coord` requires, without guessing a field and without
decompiling anything yourself.

This page exists because the packed coordination skills documented a review protocol the engine does
not accept, and the protocol it does accept appeared in no skill at all. `S.I.R.#255` recorded the
consequence: no PR in this workspace could reach a `landable` green verdict by following the packed
guidance. What follows is the contract the engine actually enforces, recovered from the engine's own
encoders and validators and from its decompiled command gates, at `fs.gg.coord.cli` **0.71.0**
(the version pinned in `.config/dotnet-tools.json`).

---

## The one thing to read first

**You never author a digest, a revision, a `previousDigest`, a `claimGeneration`, or a `baseSha`.**

`review record` rebuilds the record from live state before sealing it, discarding whatever your draft
put in those five fields:

| field | what the engine substitutes |
|---|---|
| `revision` | `existing.Length + 1` — the count of records already on the PR |
| `previousDigest` | the preceding record's digest |
| `claimGeneration` | the live winning claim marker's comment id (acceptance records only) |
| `baseSha` | the PR's live base tip SHA (acceptance records only) |
| `digest` | `StructuredDecision.reviewDigest(record)`, computed over the rebuilt record |

Your draft must **contain** those keys — the parser refuses a document that omits a required key —
but their **values are discarded**. Write `1`, `null`, `null`, `null`, `""` and move on.

This matters more than it looks. The original report of `S.I.R.#255` stopped work rather than write
invented values into a digest-sealed append-only ledger. That was the right instinct and the wrong
premise: the ledger was never going to accept those values in the first place, because it never reads
them. **No invented value can reach the ledger through this path.** The append-only chain is built for
you, and a tampered digest is caught with `revision N digest does not match its structured inputs`.

## The rule this page was written by, and once got wrong

**Converging against a pure validator cannot discover the facts the production route supplies to it.**

Most of this page was established by calling `StructuredDecision.validateReviewLedger` and reading the
invariants it names. That method is cheap, it cannot drift from the engine, and it is still the right
place to start — but it has one limit, and missing it put a wrong `subject` in an earlier revision of
this very document.

`validateReviewLedger` takes `expectedSubject` as a **parameter**. It validates whatever subject you
hand it. So it can never tell you what the production route derives — and the production route derives
something you would not guess:

```
validator told the canonical ref  "EHotwagner/S.I.R.#255"            -> accepted
validator told the production form "EHotwagner/S.I.R.#255/pr/259"    -> accepted
validator told the string          "not-even-a-ref"                  -> accepted
```

All three pass, because both sides of the comparison come from the caller. A method that accepts
`not-even-a-ref` as a subject is not going to tell you the subject rule.

The general form: **a pure function is parameterised on exactly the facts its callers establish, so it
is blind to precisely the facts you most need from its caller.** Before you trust a contract recovered
in-process, ask what the production route passes IN, and go read that. For this engine the production
route is the **CLI command gates** in `FS.GG.Coord.Cli.Client` — `authorizeReviewRecordWait`, the
derived-subject check, the comment-URL equality checks, `liveAcceptanceBinding` — and they are
reachable only with `ilspycmd`, not from the encoders and validators in `FS.GG.Coord.Core`.

Three separate errors in an earlier revision of this page came from that one seam — the wrong
`subject`, a false claim that the review oracle needs a claim you hold, and a set of comment-URL rules
described as mere non-emptiness. All three were written from the Core encoders and validators, none of
which can see what the CLI enforces.

**A worked example, from this page's own review.** A critic checking these very claims tested the
`enter` event against `ReviewWait.validate`, found no five-minute bound on `enteredAt`, and reported
that the rule "does not exist". The rule does exist — in the CLI gate, the layer the pure validator
cannot see. The critic was right that the page was wrong (it omitted the 24-hour ceiling the validator
*does* enforce) and wrong about why. It converged against the pure validator and drew a false negative
by precisely the mechanism described above, while reviewing the paragraph that describes it.

Take the lesson rather than the irony: **"I called the validator and it does not enforce X" is not
evidence that X is unenforced.** It is evidence about one layer. Both layers are enumerated for the
`enter` event below, and where a rule is production-only this page says so.

Everything below marked **production-only** is a rule the in-process validator does not enforce and
cannot reveal.

## The two traps that cost a round

**1. The acceptance record carries the CRITIC's identity, not yours.** `critic` is not "who wrote this
record" — it is "which critic's review generation is this". Every record in one generation must bind the
same `critic`, so a host writing its own worker id into its acceptance draft is refused with:

```
every record in one review generation must bind the same critic
```

which does not obviously point at the host's own draft. Copy the critic's id from the initial record.

**2. The `complete` wait event's `evidenceRef` and the acceptance record's `precedingReview` must be
the same string, exactly.** The acceptance gate compares them directly. Nothing in the field names
suggests this, and the pure ledger validator does not catch it — only the live gate does, with:

```
host acceptance requires the immediately preceding critic record's durable review wait to be completed
```

Use the critic record's comment URL for both and they line up naturally.

---

## `review wait` — schema `fsgg.coord.review-wait/v1`

A durable queue entry. `review record` refuses a critic record that no wait entry authorizes, so this
comes first. The posted comment body is the marker line, a newline, then the JSON:

```
<!-- fsgg:review-wait/v1 -->
{ … }
```

### The `enter` event

All nine keys are required.

```json
{
  "schema": "fsgg.coord.review-wait/v1",
  "event": "enter",
  "item": "EHotwagner/S.I.R.#255",
  "claimGeneration": "5382700300",
  "reviewGeneration": "<headSha>:initial-review:0",
  "kind": "initial-review",
  "enteredAt": "2026-08-22T21:40:00.0000000+00:00",
  "expiresAt": "2026-08-22T23:40:00.0000000+00:00",
  "evidenceRef": "https://github.com/EHotwagner/S.I.R./pull/257"
}
```

The `enter` event is checked by **two different layers**, and neither one enforces the other's rules.
Knowing which is which matters, because only the first is reachable from `dotnet fsi`:

**`ReviewWait.validate` — the pure validator** (measured against it directly):

- **`expiresAt - enteredAt` must be at most 24 hours**, or
  `a review wait may be bounded for at most 24 hours`. There is no way to open a longer window.
- `expiresAt` must be strictly later than `enteredAt`, or `expiresAt must be later than enteredAt`.
- It does **not** check that the window is anywhere near *now*: a receipt entered 30 days in the
  future, or expiring 10 days in the past, passes this layer.

**The CLI gate — production-only**, and the reason the previous bullet is not the whole story:

- `item` must equal the canonical ref exactly — `<owner>/<repo>#<number>`.
- `kind` is `initial-review` or `repair-confirmation`. Nothing else parses.
- `claimGeneration` must equal the **current** claim marker's comment id, or
  `receipt claimGeneration is not current`.
- `enteredAt` must not be more than five minutes in the future (`enteredAt is implausibly in the
  future`), and `expiresAt` must be in the future (`receipt is already expired`).

An earlier revision of this page listed only the CLI rules, attributed them to the validator, and
omitted the 24-hour ceiling entirely — a reader sizing a two-day window would have been refused by a
rule the page said did not exist. That is this page's own subject matter happening to this page; see
the rule at the top.

### The `complete`, `cancel` and `timeout` events

Exactly five keys. There is no `item`, no `kind`, and no `claimGeneration` on a terminal event — the
`reviewGeneration` is what links it back to its entry.

```json
{
  "schema": "fsgg.coord.review-wait/v1",
  "event": "complete",
  "reviewGeneration": "<the same token as the enter>",
  "at": "2026-08-22T22:10:00.0000000+00:00",
  "evidenceRef": "https://github.com/EHotwagner/S.I.R./pull/257#issuecomment-…"
}
```

### What the gate enforces

- `claimGeneration` on the entry must equal the **current** claim marker's comment id, or
  `receipt claimGeneration is not current`.
- One entry per `reviewGeneration`; one terminal transition per `reviewGeneration`.
- A preceding `reviewGeneration` must not be left unconsumed.
- `complete`: `at` must fall inside `[enteredAt, expiresAt]`, **and** the completion must be posted
  while `now < expiresAt`.

> **Operational warning.** That last clause is a wall clock, not a logical one. If the critic takes
> longer than the wait window, `complete` is refused with `completion was not durable before bounded
> review wait expiry` and the generation cannot be completed at all — it can only be cancelled or
> timed out and re-entered. The window is whatever you put in `expiresAt`, and the claim lease
> defaults to 120 minutes, so a wait window sized to match the lease leaves no margin for a slow
> review. **The window is capped at 24 hours** — `a review wait may be bounded for at most 24 hours` —
> so "open a very long window" is not available to you. Size `expiresAt` for the review you expect
> within that ceiling, complete it as soon as the critic's record is posted, and do not let a passing
> review sit uncompleted.
>
> **Measured on a real review:** wait opened 21:51:44Z, critic record posted 22:03:19Z — 11 minutes
> 35 seconds, against a 240-minute window, roughly 20x headroom. The margin is not there for slow
> reviewing; it is there for a critic that stalls, is respawned, or hands off. Size it for the failure,
> not for the happy path.

## `review record` — schema `fsgg.coord.review-decision/v2`

**The file you hand the CLI is BARE JSON. Do not put the marker line in it.** The marker is what the
engine POSTS; it is not what the engine READS. `review record <ref> draft.json --pr <n>` parses
`draft.json` with `Driver.decodeStructuredReview`, which is a JSON parser — give it a leading
`<!-- … -->` line and it refuses with a message that names neither the marker nor the file:

```
'<' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.
```

The same holds for `review wait`. Every JSON document on this page is the file content; the
`<!-- fsgg:review-decision/v2 -->` / `<!-- fsgg:review-wait/v1 -->` line is prepended by the engine
when it posts the comment.

### `subject` is NOT the item ref — and it differs from the wait event's `item`

**production-only.** This is the single most likely thing to get wrong, because the adjacent document
uses the other convention.

| document | field | value | example |
|---|---|---|---|
| `review wait` | `item` | the canonical item ref | `EHotwagner/S.I.R.#255` |
| `review record` | `subject` | the canonical ref **plus `/pr/<n>`** | `EHotwagner/S.I.R.#255/pr/259` |

Two neighbouring artifacts in one protocol, two different spellings of the same item. The record's form
is derived by the CLI as `$"{target.Canonical}/pr/{pr}"`, and a draft carrying anything else is refused:

```
fsgg-coord-engine: review record: subject must be 'EHotwagner/S.I.R.#255/pr/259'.
```

`validateReviewLedger` will happily accept the wrong one, because of the rule at the top of this page.

Note for a rebase: the `/pr/<n>` subject is bound to the PR, not the head, so it **survives a rebase**.
The `reviewGeneration` token does not — it embeds the head SHA. After a rebase you need a fresh
`enter`/`record`/`complete` cycle, but the subject string is unchanged.

### Draft keys

**Required** — the parser refuses the document if any is absent, and names the one it missed:

`schema`, `subject`, `revision`, `headSha`, `critic`, `verdict`, `acceptedExceptions`,
`routeApplicability`, `routeEvidence`, `policyVersion`, `kind`, `round`, `timestamp`, `digest`

**Optional** — may be omitted entirely:

`previousDigest`, `claimGeneration`, `baseSha`, `initialReview`, `precedingReview`,
`diffAuditRequired`, `diffAuditReceipts`, `succession`

Remember that four of the required keys (`revision`, `digest`, and — where present — `claimGeneration`
and `baseSha`) are required to be *present* and are then *ignored*.

### Vocabularies

| key | accepted values |
|---|---|
| `schema` | `fsgg.coord.review-decision/v2` |
| `policyVersion` | `structured-decisions/1` |
| `verdict` | `pass`, `changes-required`, `accepted` |
| `kind` | `initial`, `confirmation`, `escalation`, `repair-phase`, `acceptance` |
| `routeApplicability` | `meaningful`, `not-meaningful` |
| `timestamp` | any ISO-8601 instant |

### Ledger invariants

**This is a selected subset, not the complete set.** `StructuredDecision.validateReviewLedger` enforces
on the order of forty invariants; the ones below are those an author hits while composing a first
record, quoted in the validator's own words. **Do not read a silence here as permission.** When you
need certainty about a rule this page does not state, run the validator against your own draft — it
names what it refuses, which is the whole reason it is the oracle rather than this page:

```fsharp
StructuredDecision.validateReviewLedger subject [ yourRecord ]   // Error carries the exact refusals
```

An earlier revision of this page said the validator "names every invariant it enforces" and then listed
eight. That sentence is why the `acceptedExceptions` reversal above went unnoticed: a list presented as
complete stops a reader from checking.

- `initial review round must be zero` — an `initial` record must carry `"round": 0`.
- **`confirmation round must be contiguous within its generation`** — the first `confirmation` record
  must be `"round": 1`, the second `2`, and so on; the validator names the number it expected. Round
  `0` is refused twice over, also as `confirmation round must be positive`. **A successor critic
  cannot pick its round freely**, and this is the rule it needs — ask the review oracle, which reports
  the round it is waiting for.
- **The same-critic rule has an exception, and the successor handoff depends on it.** `every record in
  one review generation must bind the same critic` holds after a `pass`, but a `confirmation` by a
  *different* critic **is accepted after a `changes-required` verdict**. That is what makes the
  fresh-successor handoff legal at all — and this PR's own chain used it twice.
- **An `acceptance` record's `round` is inert.** No production path reads it: the wait gate's acceptance
  arm never calls `generationMatches`, and the chain projection SYNTHESISES `ReviewChain.Rounds` by
  counting `confirmation` records and emitting `1..N` rather than reading any record's field. Write
  `0`. The corollary is worth knowing: **the round ceiling counts confirmations**, so neither the
  initial record nor the acceptance record consumes a round.
- `every record in one review generation must bind the same critic` — see trap 1 above.
- `acceptance records must carry verdict accepted`.
- Non-initial records must set both `initialReview` and `precedingReview`, each non-empty — and
  **production-only**, each must be an exact comment URL, not merely a non-empty token:
  `initialReview must equal the actual current generation's initial comment URL` and
  `precedingReview must equal the actual immediately preceding structured comment URL`. The
  in-process validator checks only non-emptiness, so it will pass a draft the CLI refuses. Take both
  URLs from the `commentUrl` that `review record` printed for the records concerned.
- `claimGeneration and baseSha belong to the acceptance record` — they must be absent from the
  initial record. (You are not supplying them anyway; the engine does.)
- `routeApplicability: "meaningful"` requires **exactly four** `routeEvidence` entries;
  `"not-meaningful"` requires **exactly one**.

> **The route-evidence refusal message overstates what is checked.** It reads `meaningful route
> evidence must contain built artifact, command, comparison, and result`, but the validator only
> counts: `["a","b","c","d"]` is accepted and four descriptive strings are accepted on identical
> terms, while three or five are refused whatever they say. Treat the four-part structure as a real
> authoring obligation that no tool will enforce for you — a reader who believes the message is
> being checked will record vacuous route evidence in good faith. Filed against the Kit.

### Which wait entry authorizes which record

| record `kind` | required wait state | receipt `kind` | required `reviewGeneration` |
|---|---|---|---|
| `initial` | `Waiting` | `initial-review` | `<headSha>:initial-review:0` |
| `confirmation` | `Waiting` | `repair-confirmation` | `<headSha>:repair-confirmation:<round>` |
| `escalation` | `Waiting` | `repair-confirmation` | `<headSha>:repair-confirmation:<round>` |
| `repair-phase` | `Waiting` | `repair-confirmation` | `<headSha>:repair-confirmation:<round>` |
| `acceptance` | `Completed` | — | see trap 2 above |

## The two generation fields

**`claimGeneration` is the live claim marker's GitHub comment id**, rendered as an invariant-culture
decimal string. It is exactly the `markerId` that `take --json` already hands you, and the same value
`who --json` reports for your claim. It is not a hash and not derived.

**`reviewGeneration` is not a hash either.** `ReviewWait.generationToken` returns the literal string:

```
<headSha>:<kind>:<round>
```

for example `1d8c93d…:initial-review:0`. The ledger reader compares your receipt against the output of
that same function, so composing the string by hand is safe — but calling the function is safer, and
it is a public static on `FS.GG.Coord.Core.dll`. Note that it is **curried**: `generationToken head
kind round`, not a tupled call.

## The command order

From a critic's passing review to a green `landable`:

```sh
# 1. the claim holder opens the durable wait
scripts/fsgg-coord review wait   <ref> enter.json    --pr <n>

# 2. the critic records its verdict      (kind: initial, round: 0, verdict: pass)
scripts/fsgg-coord review record <ref> critic.json   --pr <n>

# 3. the claim holder completes the wait (evidenceRef: the critic record's comment URL)
scripts/fsgg-coord review wait   <ref> complete.json --pr <n>

# 4. the host accepts                    (kind: acceptance, verdict: accepted,
#                                         critic: THE CRITIC'S id,
#                                         precedingReview: that same comment URL)
scripts/fsgg-coord review record <ref> accept.json   --pr <n>

# 5. the verdict
scripts/fsgg-coord landable <n> --repo "S.I.R." --wait
```

### A complete acceptance draft

Step 4 is the one a host has to author from scratch, so here it is whole. Every value is either fixed,
copied from the critic's record, or read off the `review record` result that produced it.

```json
{
  "schema": "fsgg.coord.review-decision/v2",
  "subject": "EHotwagner/S.I.R.#255/pr/259",
  "revision": 1,
  "headSha": "72d40c045b25bf3c9c426bf5a81ace6b735ebea0",
  "critic": "osprey-bbbe",
  "verdict": "accepted",
  "acceptedExceptions": [],
  "routeApplicability": "not-meaningful",
  "routeEvidence": ["host acceptance is a ledger act, not a runtime observation"],
  "policyVersion": "structured-decisions/1",
  "kind": "acceptance",
  "round": 0,
  "initialReview": "https://github.com/EHotwagner/S.I.R./pull/259#issuecomment-5382832710",
  "precedingReview": "https://github.com/EHotwagner/S.I.R./pull/259#issuecomment-5382832710",
  "timestamp": "2026-08-23T00:00:00Z",
  "digest": ""
}
```

Field by field, because three of these are not guessable:

- `subject` — the `/pr/<n>` form. Not the item ref. See above.
- `revision` and `digest` — present because the parser requires the keys; the values are discarded.
  `1` and `""` are correct placeholders whatever the real revision turns out to be.
- `critic` — **the critic's worker id, not yours.** Copy it from the record you are accepting.
- `acceptedExceptions` — required, and on a host acceptance record it must be **`[]`**. This field
  belongs to the **critic**, not to you: a non-empty list on an acceptance record is refused with
  `accepted exceptions belong to critic review records, not host acceptance`, while the same list on
  a critic's record is accepted. An earlier revision of this page described it as "exceptions the host
  is knowingly accepting", which is exactly backwards and would have refused the first acceptance
  draft written from it.
- `routeApplicability` / `routeEvidence` — required on every record, including acceptance. **The host
  did not execute a route comparison; the critic did.** Attesting to a comparison you did not run
  would be false, so use `not-meaningful` with exactly one entry saying what the acceptance actually
  is. Do not copy the critic's four-part evidence into your own record.
- `initialReview` and `precedingReview` — **production-only**: both must be exact comment URLs, and
  `precedingReview` must additionally equal the `complete` wait event's `evidenceRef`. Using the
  critic record's `commentUrl` for all three satisfies every rule at once.
- `round` — `0`. Nothing on the production path reads it; see the invariants above.

`review record` on an acceptance draft pre-validates the resulting chain **before** posting, and
refuses with `resulting accepted chain is invalid: …` rather than writing a bad record. A wrong
acceptance draft costs an error message, not a corrupted ledger.

On success `review record` prints a `fsgg.coord.review-record-result/v2` document carrying
`commentId`, `commentUrl`, `digest`, `revision` and `subject`. **Keep the `commentUrl`** — the
`complete` wait event and every later record's `initialReview`/`precedingReview` all need it.

Its `effectiveChainValidated` field is literally `kind == acceptance`: it reports whether this record's
kind triggered the full accepted-chain pre-validation. `false` on an initial or confirmation record is
expected and does not mean anything failed.

## `scripts/fsgg-coord review <ref> --pr <n>` is the oracle

It requires a **live claim marker to EXIST on the item** — not that you hold it. The engine reads the
winning marker to derive `claimGeneration` and never compares its worker id to `$FSGG_WORKER`, so a
critic holding no claim can run this, and can post its own `review record`. **Measured:** critic
`osprey-bbbe`, holding no claim on `S.I.R.#255`, recorded revision 1 on PR #259 while a different
worker held the claim.

The engine does have a holder check — `live claim belongs to worker '%s', not '%s'` — and applies it to
`delivery`, not to review. Do not assume from that message's existence that review is gated the same
way; it is not. It returns one
typed state and one next action — `DispatchCritic`, `ResumeImplementer`, `DispatchSuccessor`,
`AwaitChecks`, `RequestHostAcceptance`, `EnterRepairPhase`, `EnterCriticSuccession`, `Accept`,
`AuthorizeDelivery`, `Park` — bound to a freshness token that a changed head invalidates. Prefer it
over inferring the protocol from prose, including this page.

## Other contracts the engine publishes but the skills do not

`scripts/fsgg-coord facts --json` is the engine describing its own protocol, and it carries more than
the skills project. Two contracts that agents have had to rediscover:

- **`ledgerPolicy`** is the complete `fsgg.coord.planning-receipt/3` schema — the receipt `driver`
  requires and refuses without. Its `requiredObservations` list is exact: `reconcile-dry-run/clean`,
  `reconcile-apply/applied-or-not-needed`, `reconcile-fresh/clean`, `triage/fresh`,
  `engine-currency/current-scoped`.
- **`reviewPolicy`** carries the review schema and its five kinds — the fact whose absence from the
  packed skills caused `S.I.R.#255`.

When a packed skill and `facts --json` disagree, `facts` is the engine and the skill is a projection.

## How to settle a question this page does not answer

Do not guess a field value, and do not infer a contract from a refusal message — as the route-evidence
message above shows, a message can assert a check that does not exist.

**Reach for `ilspycmd` first.** It is on `PATH` at `~/.dotnet/tools/ilspycmd`, and it answered in one
command what reflection and trial-and-error could not: which fields the engine overwrites, and what
the live gates actually compare.

```sh
ilspycmd -r ~/.nuget/packages/fs.gg.coord.cli/0.71.0/tools/net10.0/any \
         ~/.nuget/packages/fs.gg.coord.cli/0.71.0/tools/net10.0/any/fsgg-coord-engine.dll \
         -t FS.GG.Coord.Cli.Client > client.cs
```

For document shapes, call the engine's own encoder rather than reading its output and copying by eye —
`ReviewWait.encode` and `Driver.encodeStructuredReview` emit the exact wire form, and
`StructuredDecision.validateReviewLedger` names each invariant it refuses on, which is why it answers
questions this page does not:

```fsharp
#r "/home/developer/.nuget/packages/fs.gg.coord.cli/0.71.0/tools/net10.0/any/FS.GG.Coord.Core.dll"
open FS.GG.Coord
ReviewWait.encode (ReviewWait.Transition.Enter receipt)   // the exact wait document
Driver.encodeStructuredReview record                       // the exact record document
StructuredDecision.validateReviewLedger subject records    // the refusals, in its own words
```

Converging against the validator is how the invariants on this page were established. It is cheaper
than reading IL and it cannot drift from the engine, because it *is* the engine.

**But read the rule at the top of this page before you trust what it tells you.** A pure function is
parameterised on the facts its caller establishes, so converging against it is blind to exactly those
facts — which is how an earlier revision of this page documented the wrong `subject`. Use the validator
to learn the invariants; use `ilspycmd` over `FS.GG.Coord.Cli.Client` to learn what the production route
passes into it. You need both, and the second is the one people skip.

`scripts/test-review-contract-coherence.sh` holds this page to that standard, and it is precise about
how far that goes:

- **Derived claims** — the required and optional draft-key lists, **every row** of the vocabulary
  table, both wait examples' key sets, the route-evidence cardinalities, the initial-record round, the
  `reviewGeneration` shape, the wait-window ceiling, the authorization table's **token** and
  **receipt-kind** columns, and the two subject forms — are **parsed out of this document** and
  compared against the live engine. Falsify one here and the gate reds, because its expectation is
  this text.
- **Transcribed claims** — the authorization table's **wait-state** column, and the engine-overwrites
  table (both its field list and each row's description). These are CLI behaviour, decided by
  `authorizeReviewRecordWait` and `recordReview$cont@2412-3` behind a live GitHub transport, so nothing
  in `FS.GG.Coord.Core` knows them and the gate cannot derive them. The gate compares them against an
  explicit transcription of the decompiled code and labels it as one. Falsify them here and the gate
  still reds; falsify the *engine* and it would not notice.
- **Literal-only claims** — the traps, the warnings, the quoted refusal strings, and everything else
  stated as prose with no machine form — are checked for presence only. Rewriting such a sentence to
  say the opposite while keeping its key phrase would not be caught, and the gate claims no inversion
  over them.

An earlier revision claimed it "fails when any load-bearing claim here is inverted". It did not: seven
documented claims were falsified and it stayed green. The distinction above is the repair, and stating
it honestly is part of the repair rather than a caveat on it.

**The repair reproduced the defect in miniature, twice over, across two review rounds.**

Round one's repair iterated only four of the authorization table's five rows, so the `acceptance` row
could be falsified undetected; "checked" the wait-state column against
`if kindName = "initial" then "Waiting" else "Waiting"`, a constant compared with itself; and "proved"
the literal-only claims red-when-deleted with a loop whose mutation and assertion were the same
`grep`.

Round two's review then found three more, all filed under **Derived** while being nothing of the kind:
two vocabulary rows were parsed into the expectation file and never looped over — parsed-but-unchecked,
which is worse than unparsed because the expectation file makes it look covered; the overwrites table
was compared against a string literal inside the gate, three lines below a comment introducing the
Transcribed category it belonged in; and the authorization table's receipt-kind column was captured by
the parser and thrown away, belonging to no category and checked by nothing.

Note where the error concentrated: **the `Derived` bucket, every time.** The Transcribed category held
up under review, with its one member confirmed against the decompiled switch, and the literal-only
claims behaved exactly as this page says they will. Over-claiming is the failure mode, and it lands on
whichever bucket promises the most. **A gate's inversion evidence is itself a claim that can be
vacuous, and it needs the same scrutiny as the thing it defends.**
