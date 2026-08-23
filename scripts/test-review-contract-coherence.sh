#!/usr/bin/env bash
# Holds docs/coordination-engine-contracts.md to the coordination engine it describes.
#
# WHY THIS EXISTS. S.I.R.#255: the packed review contract documented prose HTML-comment markers while
# the engine enforced `fsgg:review-decision/v2` written through `review wait` / `review record`, and
# nothing measured the gap. Every PR in the workspace stalled at `landable`. Prose describing a machine
# contract rots silently unless something reds when it drifts.
#
# HOW IT WORKS, AND WHY IT WAS REBUILT (round 1, finding F1).
#
# The first version compared the engine against expectations hard-coded IN THIS FILE, and separately
# checked that a few literal substrings still appeared in the document. Nothing parsed the document, so
# its tables, key lists, vocabularies and examples could all drift freely while the gate stayed green:
# the critic falsified seven load-bearing documented claims and got exit 0 on every one. A gate that
# cannot fail against its own declared subject is decorative.
#
# The subject of this gate is THE DOCUMENT, so the document is now the SOURCE of the expectations:
#
#     docs/coordination-engine-contracts.md  --parse-->  expected.json  --compare-->  live engine
#
# Invert a documented claim and the parsed expectation moves with it, so the comparison against the
# engine fails. That is the property the previous version lacked.
#
# DERIVED vs LITERAL-ONLY. Claims with a machine form are parsed and compared against the engine
# (STEP 2), and each is mutated in the document and required to red (STEP 3). Claims that are prose
# with no machine form — a trap stated in a sentence, a warning — can only be checked for presence
# (STEP 4). Both sets are enumerated, and the document says which is which. An honest partial gate
# beats a total one that is not true; overclaiming here is what F1 was about.
#
# WHAT SEPARATES A SOUND DERIVED CHECK FROM A VACUOUS ONE (rounds 1-4, and the repair phase).
#
# Four consecutive review rounds each found another vacuous assertion filed under DERIVED, and each
# round repaired the instance in front of it. The category was the wrong cut. Measured across all
# twenty-one inversions and a widening sweep over the rows believed sound, the discriminator is:
#
#     does the expectation parsed out of the document carry the claim's EXTENSION, or only its
#     PRESENCE?
#
# An EXTENSION — a set of literals, a number, a shape, a type — moves when the document moves, so the
# comparison against the engine detects drift. Every claim of that shape held up.
#
# A PRESENCE — `"<phrase>" in doc`, `cell.Contains tok`, or a parser regex that spells out the one
# correct answer — is already a CONSTANT by the time STEP 2 compares it, because STEP 1 aborted if it
# were absent. The check degenerates into an engine self-test with a flag stapled to the front. It
# reds when the sentence is DELETED and never when it is WIDENED, and it looks derived because it
# really does call the engine. All of `timestamp`, the two prose invariants, and the generation-token
# shape were this, and nothing in three rounds of review could see it, because the harness scored a
# STEP 1 parse abort exactly as it scored a detection.
#
# So: no expectation in this gate may be presence-shaped. The vocabulary table states both columns of
# every row, the two prose invariants became an outcome table, the wait example binds value TYPES as
# well as key names, and the free-form branch is deleted rather than repaired. STEP 3 then enforces
# the rest by construction — see its header.
#
# NO BOARD IO. Every check is pure: it loads FS.GG.Coord.Core.dll and calls it, and runs `facts`, which
# the engine documents as touching no board and no network. It never claims, posts, or merges.
#
# WHAT IT CANNOT DRIVE. The production CLI gates (`authorizeReviewRecordWait`, the derived-subject
# check, comment-URL equality, `liveAcceptanceBinding`) sit behind a live GitHub transport; exercising
# them would post to a real board. So facts the production route DERIVES are asserted against the
# engine's own derivation inputs — the record subject is built from `Types.Ref.Canonical`, exactly as
# the CLI builds it — and rules the production route enforces that the in-process route does not are
# asserted as KNOWN-BLIND, proving the divergence instead of leaving it a silent gap.

set -euo pipefail

# `--inner <doc>` re-enters this script against a mutated copy of the document (STEP 3).
inner=""
if [[ "${1:-}" == "--inner" ]]; then inner="${2:?--inner needs a document path}"; fi

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
doc="${inner:-$repo_root/docs/coordination-engine-contracts.md}"
tmp=$(mktemp -d)
trap 'rm -rf -- "$tmp"' EXIT

[[ -f "$doc" ]] || { echo "missing the document under test: $doc" >&2; exit 2; }

# RESOLVE THE SDK global.json PINS, AND DO NOT TRUST AN INHERITED DOTNET_ROOT.
# `global.json` pins an exact SDK with `rollForward: disable`. The ambient environment here exports
# DOTNET_ROOT=/usr/share/dotnet, which carries other SDKs but not the pinned one, while the pinned SDK
# lives under ~/.dotnet. Honouring the inherited value fails with an SDK-resolution error that never
# names the real cause, so pick the root that actually has the pinned SDK.
pinned_sdk=$(python3 - "$repo_root/global.json" <<'PY'
import json, sys
print(json.load(open(sys.argv[1]))["sdk"]["version"])
PY
)
for candidate in "$HOME/.dotnet" "${DOTNET_ROOT:-}" /usr/share/dotnet; do
  [[ -n "$candidate" && -d "$candidate/sdk/$pinned_sdk" ]] || continue
  export DOTNET_ROOT="$candidate"; export DOTNET_HOST_PATH="$candidate/dotnet"
  export PATH="$candidate:$candidate/tools:$PATH"; break
done
[[ -d "${DOTNET_ROOT:-}/sdk/$pinned_sdk" ]] || {
  echo "global.json pins SDK $pinned_sdk, which is not installed under any known dotnet root." >&2; exit 2; }

# THE ENGINE UNDER TEST IS THE PINNED ONE, read from the manifest rather than hardcoded — a version
# written here would rot exactly the way the contract did, which is the defect this script exists for.
pinned=$(python3 - "$repo_root/.config/dotnet-tools.json" <<'PY'
import json, sys
print(json.load(open(sys.argv[1]))["tools"]["fs.gg.coord.cli"]["version"])
PY
)
core="$HOME/.nuget/packages/fs.gg.coord.cli/$pinned/tools/net10.0/any/FS.GG.Coord.Core.dll"
[[ -f "$core" ]] || { echo "the pinned engine ($pinned) is not restored: $core" >&2
                      echo "run: dotnet tool restore" >&2; exit 2; }

# ---------------------------------------------------------------------------------------------------
# STEP 1 — parse the document into expectations. Fails closed: a document that cannot be parsed is an
# error, never an empty expectation set that would let everything pass vacuously.
# ---------------------------------------------------------------------------------------------------

python3 - "$doc" "$tmp/expected.json" <<'PY'
import json, re, sys

doc = open(sys.argv[1], encoding="utf-8").read()
out, problems = {}, []
ticked = lambda s: re.findall(r"`([^`]+)`", s)

# NO PARSE BELOW PRODUCES A CHECK'S EXPECTATION FROM A FIRST MATCH. Stated exactly, because the
# previous wording of this banner -- "EVERY PARSE BELOW IS TOTAL OVER ITS CELL. NONE TAKES THE FIRST
# MATCH." -- was FALSE when it was written, and its confidence is what invited the next reader to stop
# looking. Nine first-match parses survived beneath it and three widenings survived at exit 0.
#
# What actually holds, and each clause is checkable:
#
#   1. Every expectation that names a SET -- vocabulary columns, key lists, authorization cells,
#      subject examples, wait-example keys and types -- is parsed with `re.findall` over the whole
#      cell, so an added alternative joins the set.
#   2. NO scalar is parsed from prose at all. The five that were -- the wait-window ceiling, the
#      initial round, the two contiguity rounds, the two cardinalities and the generation-token shape
#      -- are rows in the Scalar invariants table with accepted and refused columns, probed exactly
#      like vocabulary rows. Their old prose regions are `scalar-governed`: each is transcribed and
#      compared for EQUALITY, and exactly one region may bear an id, so a value cannot be restated
#      inside a marked region in any phrasing, nor smuggled in by a second region repeating an id.
#      That is the whole of the property. Prose OUTSIDE every marked region is not governed at all;
#      the marking is authored, so this rule cannot be more total than the marking is. See the
#      boundary clause the passing footer prints -- it is a limit of this gate, not a property of
#      the page, and stating it as though it were the latter is what the footer did three times.
#   3. `re.search` survives only as a LOCATOR: `section()` and `bullet()` find the region to parse.
#      A locator cannot be widened into a false claim, because it produces no expectation.
#   4. `sole()` refuses where a second statement is genuinely ambiguous rather than additive.
#
# The proof is not this comment. STEP 3 requires a widening mutation for every derived check, so if
# any clause above stops holding, the corresponding widening stops reddening and the run fails.
#
# Repair-phase round 1, finding F1. The previous revision closed the SCORING defect (a parse abort no
# longer counts as a detection) but left six checks unable to detect a WIDENING, because their
# expectation was parsed with `re.search(...)` or `[0]` -- the first literal, the rest discarded.
# A widening ADDS a permitted-but-wrong alternative, so under a first-match parse the expectation is
# UNCHANGED and the comparison it feeds stays green on a falsified page. The gate then printed, in one
# run, `ok doc:overwrite-description:revision` and `reds doc:overwrite-description:revision` -- the
# coverage machinery certifying a check as inverted while that check was green on a falsified page.
#
# A first-match parse cannot represent a widening. That is a property of the parse, not of the
# mutation catalogue, so no number of added mutations would have fixed it. The rule is therefore:
# a cell's expectation is the WHOLE cell -- a set of literals, or an exact string -- and never a
# prefix of it.
def all_of(pattern, text, what, flags=0):
    """Every match, as a list. Never `search`; never `[0]` on a longer list."""
    got = re.findall(pattern, text, flags)
    if not got:
        problems.append("%s: no match" % what)
    return got

def sole(pattern, text, what, flags=0):
    """Exactly one match, or the value is ambiguous and the document is refused."""
    got = all_of(pattern, text, what, flags)
    if len(got) == 1:
        return got[0]
    if got:
        problems.append("%s: expected exactly one, found %d (%s) -- a claim stated twice, or stated "
                        "with an alternative, is not one expectation"
                        % (what, len(got), ", ".join(map(str, got))))
    return None

def section(header, stop=("\n## ", "\n### ")):
    i = doc.find(header)
    if i < 0:
        problems.append("section not found: %r" % header); return ""
    j = len(doc)
    for s in stop:
        k = doc.find(s, i + len(header))
        if k >= 0: j = min(j, k)
    return doc[i:j]

draft = section("### Draft keys")
_req = sole(r"\*\*Required\*\*.*?:\s*\n\n(.+?)\n\n", draft, "the required draft-key list", re.S)
out["requiredKeys"] = ticked(_req) if _req else []
_opt = sole(r"\*\*Optional\*\*.*?:\s*\n\n(.+?)\n\n", draft, "the optional draft-key list", re.S)
out["optionalKeys"] = ticked(_opt) if _opt else []

# EVERY row is kept, and every row must state its EXTENSION in both directions. An earlier revision
# dropped rows with no backticked literal via `if vals:`, silently excluding `timestamp`; the revision
# after that kept the row but "checked" its free-form cell for the presence of one token, which
# reduced to `cell.Contains "ISO-8601"` and passed every widening. Presence is not an extension. A row
# that cannot name what the engine accepts AND what it refuses is a parse failure, so the free-form
# branch has no way back in.
vocab = {}
for row in re.finditer(r"^\|\s*`([a-zA-Z]+)`\s*\|\s*(.+?)\s*\|\s*(.+?)\s*\|\s*$",
                       section("### Vocabularies"), re.M):
    key, accepted, refused = row.group(1), ticked(row.group(2)), ticked(row.group(3))
    if not accepted:
        problems.append("vocabulary row `%s` names no accepted literal" % key)
    if not refused:
        problems.append("vocabulary row `%s` names no refused literal -- a row with no counterexample "
                        "cannot be widened-tested, which is the defect this column exists for" % key)
    both = sorted(set(accepted) & set(refused))
    if both:
        problems.append("vocabulary row `%s` lists %s in both columns" % (key, ", ".join(both)))
    vocab[key] = {"accepted": accepted, "refused": refused}
out["vocabularies"] = vocab
if not vocab:
    problems.append("the vocabularies table parsed to no rows at all")

# The outcome table: rules whose claim is a PAIR of engine behaviours rather than a set of values.
# Stated as prose these were checked by `"<phrase>" in doc`, which is a constant by the time it is
# compared -- STEP 1 has already aborted if the phrase is absent -- so the check became an engine
# self-test and every widening survived. A case id and an outcome literal give the claim an extension.
outcomes = {}
for row in re.finditer(r"^\|\s*`([a-z-]+)`\s*\|\s*`([a-z-]+)`\s*\|\s*`(accepted|refused)`\s*\|\s*$",
                       section("### Rules with two outcomes"), re.M):
    if row.group(2) in outcomes:
        problems.append("the outcome table names case `%s` twice" % row.group(2))
    # The `rule` column used to be parsed here and read by nothing -- parsed-but-discarded, the
    # identical defect this file repairs for the authorization table, reintroduced one table over in
    # the table this gate added. It is now bound to each probe's declared rule.
    outcomes[row.group(2)] = {"rule": row.group(1), "outcome": row.group(3)}
out["outcomeRules"] = outcomes
if not outcomes:
    problems.append("the outcome table parsed to no rows at all")

wait = section("## `review wait` — schema `fsgg.coord.review-wait/v1`", stop=("\n## ",))
# EVERY json example in the section, classified by its own `event` field -- not `blocks[0]` and
# `blocks[1]`. A THIRD example was accepted and silently ignored, so a second `enter` block carrying
# `"claimGeneration": 5382700300` (a Number where the engine emits a String) passed the whole gate.
# Round 1 totalised the six cells a critic had measured; this is the same first-match defect at a site
# nobody had measured, which is why the property and not the instance has to be the unit of repair.
blocks = re.findall(r"```json\n(.*?)\n```", wait, re.S)
if len(blocks) >= 2:
    def jkind(v):
        if v is None: return "Null"
        if isinstance(v, bool): return "Boolean"
        if isinstance(v, str): return "String"
        if isinstance(v, (int, float)): return "Number"
        if isinstance(v, list): return "Array"
        return "Object"
    def kindsOfExample(obj):
        return {k: jkind(v) for k, v in obj.items()}
    try:
        parsed = [json.loads(b) for b in blocks]
        enters = [b for b in parsed if b.get("event") == "enter"]
        terminals = [b for b in parsed if b.get("event") in ("complete", "cancel", "timeout")]
        if not enters:
            problems.append("the review wait section carries no `enter` example")
        if not terminals:
            problems.append("the review wait section carries no terminal example")
        enter = enters[0] if enters else {}
        # The union across EVERY enter example, so an added example widens the key set rather than
        # being discarded.
        out["waitEnterKeys"] = sorted({k for e in enters for k in e})
        # KEYS ALONE WERE THE WHOLE CHECK, and `sorted(json.loads(...).keys())` discards every value.
        # `claimGeneration` is a STRING while `claim --json` emits a numeric `markerId`, so following
        # the obvious idiom is refused -- and rewriting this example's "5382700300" to 5382700300 red
        # nothing at all. Parsed-but-unchecked, one level below the row that had the same defect.
        # Per key, EVERY type any enter example gives it. One example -> one type, as before. A second
        # example disagreeing -> a joined string that cannot equal the encoder's single type, so the
        # disagreement reds `doc:wait-enter-value-types` by name instead of being discarded.
        merged = {}
        for e in enters:
            for k, v in kindsOfExample(e).items():
                merged.setdefault(k, set()).add(v)
        out["waitEnterTypes"] = {k: "|".join(sorted(vs)) for k, vs in merged.items()}
        out["waitTerminalKeys"] = sorted({k for tb in terminals for k in tb})
    except Exception as e:
        problems.append("a wait example is not valid JSON: %s" % e)
else:
    problems.append("expected two ```json examples in the review wait section")

over = section("## The one thing to read first")
fields, descriptions = [], {}
for m in re.finditer(r"^\|\s*`([a-zA-Z]+)`\s*\|\s*(.+?)\s*\|\s*$", over, re.M):
    fields.append(m.group(1))
    descriptions[m.group(1)] = m.group(2)
out["overwrittenFields"] = fields
out["overwriteDescriptions"] = descriptions
if not fields: problems.append("could not parse the engine-overwrites table")

inv = section("### Ledger invariants")

# ---- SCALAR INVARIANTS: a table, because prose could not be parsed without guessing its phrasing ----
#
# Repair-phase round 2's critic, finding F5. Five claims here were sentences carrying a number, and
# each round's repair widened the regex to admit the counterexample the previous critic supplied:
# `must be at most (\d+) hours` -> `at most (\d+) hours` -> defeated by spelling the number as a word.
# A prose claim has no enumerable cell, so its parse is a phrasing-specific pattern, and a
# phrasing-specific pattern is presence-shaped for every phrasing it was not written to see. This
# file's own guidance says the fix is not to enumerate the shapes prose takes but to match the
# structure of the thing asserted -- so the five became a two-column table, the one shape that has
# survived every attack in this chain.
scal_text, scal_at = section("### Scalar invariants"), 0
scalars = {}
for row in re.finditer(r"^\|\s*`([a-z-]+)`\s*\|\s*(.+?)\s*\|\s*(.+?)\s*\|\s*$", scal_text, re.M):
    key, accepted, refused = row.group(1), ticked(row.group(2)), ticked(row.group(3))
    if not accepted:
        problems.append("scalar row `%s` names no accepted literal" % key)
    if not refused:
        problems.append("scalar row `%s` names no refused literal" % key)
    if set(accepted) & set(refused):
        problems.append("scalar row `%s` lists a literal in both columns" % key)
    scalars[key] = {"accepted": accepted, "refused": refused}
out["scalars"] = scalars
if not scalars:
    problems.append("the scalar invariants table parsed to no rows at all")

# ---- GOVERNED REGIONS: a rule about WHERE a claim may live, INSIDE THE REGIONS THAT ARE MARKED -----
#
# The prose that used to carry these values is wrapped in `<!-- scalar-governed:<id> -->` markers, and
# no digit, number word, or `<a>:<b>` shape token may appear inside. Four things make that terminate,
# and none of them is a phrasing rule: the region's text is compared to its transcription for
# EQUALITY, an id may be borne by exactly one region, a marker whose id list is unreadable is REFUSED
# rather than skipped, and the values themselves live in the scalar table where both columns are
# probed. What this does NOT do is bound the whole page: prose outside every marked region is not
# examined at all. The marking is authored, so the rule is exactly as total as the marking. Earlier
# versions of this comment claimed the document-wide property -- do not restore it here after the
# footer has retired it.
NUMBER_WORDS = r"zero|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|twenty-four|seventy-two"
violations = []
# One region may govern several ids -- two invariants often share a sentence -- so the marker takes a
# comma-separated list. Every id it names must exist in the table, and every table row must be
# governed somewhere, in both directions.
governed = set()
# TOTAL, AND PER ID. `region_bodies` keeps EVERY region's body under its raw marker label, and
# `id_regions` records every region each id appears in. Round 3 collected regions here with
# `finditer` but then looked the transcription up with a per-id `re.search` -- the FIRST match --
# so a second region bearing an id already governed was compared against nothing and reached only
# the token rules, which the comment below concedes are insufficient. That is `sole()`'s case, and
# it is the defect this chain's own STEP 1 banner forbids at :119. Both dictionaries are built in
# this single pass so no later read can reintroduce a first match.
region_bodies, id_regions = {}, {}
for m in re.finditer(r"<!-- scalar-governed:([a-z,-]+) -->(.*?)<!-- /scalar-governed -->", doc, re.S):
    rids = [r.strip() for r in m.group(1).split(",") if r.strip()]
    body = m.group(2)
    # FAIL CLOSED ON AN UNREADABLE MARKER. `[a-z,-]+` matches a label of separators alone -- `,` or
    # `,,` -- which splits to no ids at all. Every rule below is keyed on an id or on the label, so
    # an empty id list made all of them iterate over nothing: the unknown-id check saw no ids, the
    # uniqueness rule gained no entry, and the raw label is not a GOVERNED_TEXT key, so the equality
    # never compared it. The region reached the token rules alone -- the ones this file concedes are
    # insufficient -- and the run then printed a confident `ok` about a marker whose ids resolved to
    # nothing. That is a FAILED READ reported as a pass (#266), and the answer is to refuse the input
    # rather than to recognise the sentence inside it. Enumerating `,` and `,,` would be the
    # enumeration trap one level down; what is refused here is UNREADABILITY, not a phrasing.
    if not rids:
        violations.append("a `scalar-governed` marker carries no readable id (label %r): the marker "
                          "is UNREADABLE, not empty of claims. A region whose id list resolves to "
                          "nothing is refused rather than skipped -- every rule here is keyed on an "
                          "id, so an unreadable marker would otherwise be governed by nothing while "
                          "the run reported success" % m.group(1))
        continue
    governed.update(rids)
    label = ",".join(rids)
    region_bodies.setdefault(m.group(1), []).append(body)
    for rid in rids:
        id_regions.setdefault(rid, []).append(label)
    for rid in rids:
        if rid not in scalars:
            violations.append("%s: governed region has no row in the scalar table" % rid)
    for bad in re.findall(r"\d+", body):
        violations.append("%s: the digit %s appears in prose; it belongs in the scalar table" % (label, bad))
    for bad in re.findall(r"(?i)\b(%s)\b" % NUMBER_WORDS, body):
        violations.append("%s: the number word '%s' appears in prose; it belongs in the scalar table"
                          % (label, bad))
    for bad in re.findall(r"<[A-Za-z]+>[:\-]<[A-Za-z]+>", body):
        violations.append("%s: the shape token %s appears in prose; it belongs in the scalar table"
                          % (label, bad))
for key in scalars:
    if key not in governed:
        violations.append("%s: scalar row has no governed prose region" % key)
# EXACTLY ONE REGION PER ID -- the `sole()` discipline, applied to governance itself. A second
# region bearing an id the document already governs is not additive; it is an unchecked restatement
# standing beside a checked one, and it is invisible to an equality compared against the first.
for rid, labels in sorted(id_regions.items()):
    if len(labels) > 1:
        violations.append("%s: governed by %d regions (%s) -- exactly one region may bear an id. A "
                          "second region carrying an id already governed restates the value beside "
                          "the transcription rather than inside it; merge it into the governed "
                          "region, or move the claim to the scalar table"
                          % (rid, len(labels), ", ".join(labels)))
# TRANSCRIBED, AND DELIBERATELY SO. The token rules above are a fast, legible first line -- they name
# what went wrong when they fire. They are not sufficient: round 2's critic defeated each previous
# repair by rephrasing, and a purely token-based rule is defeated by the next phrasing nobody listed
# (a Roman numeral; a claim with no numeral at all, "increments it by a single unit"). Enumerating
# more token shapes is the trap this page warns against -- prose has more shapes than anyone
# enumerates.
#
# So a governed region is NOT free prose. Its exact text is transcribed below and compared for
# equality, which is the same repair that fixed the overwrite descriptions and has held since. Any
# edit inside a governed region, in any phrasing, reds -- including one that names no value at all.
# The cost is real and accepted: improving this wording means updating the transcription too, which
# is what "this region carries no claims" has to mean if it is to be enforceable rather than hoped for.
GOVERNED_TEXT = json.loads(r'''{"wait-window-max-hours": "\n- **`expiresAt - enteredAt` has a hard ceiling**, and exceeding it is refused with a message of the\n  form *a review wait may be bounded for at most N hours*, where N is `wait-window-max-hours` in the\n  Scalar invariants table. There is no way to open a longer window.\n", "initial-round": "\n- An `initial` record must carry the `initial-round` value from the Scalar invariants table; the\n  validator refuses any other.\n", "confirmation-round-first,confirmation-round-second": "\n- **`confirmation round must be contiguous within its generation`** \u2014 the first `confirmation` record\n  carries `confirmation-round-first` from the Scalar invariants table, the next carries\n  `confirmation-round-second`, and so on; the validator names the number it expected. A round below\n  the first is refused twice over, also as `confirmation round must be positive`. **A successor critic\n  cannot pick its round freely**, and this is the rule it needs \u2014 ask the review oracle, which reports\n  the round it is waiting for.\n", "meaningful-evidence-count,not-meaningful-evidence-count": "\n- `routeApplicability: \"meaningful\"` requires exactly `meaningful-evidence-count` `routeEvidence`\n  entries, and `\"not-meaningful\"` exactly `not-meaningful-evidence-count` \u2014 both in the Scalar\n  invariants table.\n", "generation-token-shape": "\n**`reviewGeneration` is not a hash either.** `ReviewWait.generationToken` returns a literal string\nwhose shape is `generation-token-shape` in the Scalar invariants table. The ledger reader compares your\nreceipt against the output of that same function, so composing the string by hand is safe \u2014 but\ncalling the function is safer, and it is a public static on `FS.GG.Coord.Core.dll`. Note that it is\n**curried**: `generationToken head kind round`, not a tupled call.\n"}''')
for rid, body in sorted(GOVERNED_TEXT.items()):
    # EVERY region under this label, not the first. If the id-uniqueness rule above has already
    # fired there will be more than one; comparing all of them means the equality still binds each,
    # so neither check depends on the other having run.
    bodies = region_bodies.get(rid, [])
    if not bodies:
        violations.append("%s: governed region is missing entirely" % rid)
    for i, got in enumerate(bodies):
        if got != body:
            violations.append("%s: governed prose differs from its transcription%s -- a governed "
                              "region is not free prose; move the claim to the scalar table, or "
                              "update the transcription deliberately"
                              % (rid, "" if len(bodies) == 1 else " (region %d of %d)"
                                 % (i + 1, len(bodies))))
out["governedViolations"] = violations
out["governedRegions"] = sorted(governed)

rows = {}
for row in re.finditer(r"^\|\s*`([a-z-]+)`\s*\|\s*(.+?)\s*\|\s*(.+?)\s*\|\s*(.+?)\s*\|\s*$",
                       section("### Which wait entry authorizes which record"), re.M):
    # ALL literals in each cell, not the first. `(ticked(...) or [...])[0]` discarded every
    # alternative, so widening the receipt-kind cell to "`repair-confirmation` or `initial-review`"
    # parsed to an unchanged expectation and the row stayed green.
    rows[row.group(1)] = {"waitState": ticked(row.group(2)) or [row.group(2).strip()],
                         "receiptKind": ticked(row.group(3)) or [row.group(3).strip()],
                         "token": ticked(row.group(4))}
out["authorization"] = rows
if "initial" not in rows: problems.append("authorization table has no `initial` row")

subj = section("### `subject` is NOT the item ref")
out["recordSubjectExamples"] = all_of(r"`(EHotwagner/S\.I\.R\.#255/pr/\d+)`", subj,
                                     "the record subject example")
out["waitItemExamples"] = all_of(r"\|\s*`review wait`\s*\|\s*`item`\s*\|[^|]*\|\s*(.+?)\s*\|\s*$",
                                subj, "the wait item example", re.M)
out["waitItemExamples"] = ticked(" ".join(out["waitItemExamples"]))

if problems:
    print("the document could not be parsed into expectations — a failure, not an empty result:", file=sys.stderr)
    for p in problems: print("  " + p, file=sys.stderr)
    sys.exit(1)

json.dump(out, open(sys.argv[2], "w"), indent=2, sort_keys=True)
PY

# ---------------------------------------------------------------------------------------------------
# STEP 2 — check every parsed expectation against the live engine.
#
# QUOTED heredoc: the F# below contains backticks and `$` in comments and string literals. An UNQUOTED
# heredoc makes bash execute backtick-quoted words as command substitutions and silently rewrite this
# probe — measured during round 1. Paths are injected afterwards instead.
# ---------------------------------------------------------------------------------------------------

cat > "$tmp/probe.fsx" <<'FSX'
#r "@@CORE@@"
open System
open System.Text.Json
open FS.GG.Coord
open FS.GG.Coord.StructuredDecision

let expected = JsonDocument.Parse(IO.File.ReadAllText "@@EXPECTED@@").RootElement
let strs (n: string) =
  expected.GetProperty(n).EnumerateArray() |> Seq.map (fun e -> e.GetString()) |> List.ofSeq
let num (n: string) = expected.GetProperty(n).GetInt32()
let str (n: string) = expected.GetProperty(n).GetString()

let itemRef : Types.Ref = { Owner = "EHotwagner"; Repo = "S.I.R."; Number = 255 }
let subject = sprintf "%s/pr/%d" itemRef.Canonical 259
let head = "1111111111111111111111111111111111111111"
let entered = DateTimeOffset.Parse("2026-08-22T21:40:00Z").ToUniversalTime()
let seal (r: ReviewRecord) = { r with Digest = StructuredDecision.reviewDigest r }

let baseRec : ReviewRecord =
  { Schema = "fsgg.coord.review-decision/v2"; Subject = subject; Revision = 1
    PreviousDigest = None; HeadSha = head; ClaimGeneration = None; BaseSha = None
    Critic = "critic-abcd"; Verdict = ReviewVerdict.Pass; AcceptedExceptions = []
    RouteApplicability = "not-meaningful"; RouteEvidence = [ "documentation-only change" ]
    PolicyVersion = "structured-decisions/1"; Kind = ReviewKind.Initial; Round = 0
    InitialReview = None; PrecedingReview = None; DiffAuditRequired = false
    DiffAuditReceipts = []; Succession = None; Timestamp = "2026-08-22T21:40:00Z"; Digest = "" }
let initial = seal baseRec
let accepts recs =
  match StructuredDecision.validateReviewLedger subject recs with Ok _ -> true | Error _ -> false

let keysOf (encoded: string) =
  let json = encoded.Substring(encoded.IndexOf '{')
  use d = JsonDocument.Parse json
  d.RootElement.EnumerateObject() |> Seq.map (fun p -> p.Name) |> Seq.sort |> List.ofSeq

let receipt : ReviewWait.WaitReceipt =
  { Item = itemRef.Canonical; ClaimGeneration = "5382700300"
    ReviewGeneration = ReviewWait.generationToken head ReviewWait.Kind.InitialReview 0
    Kind = ReviewWait.Kind.InitialReview
    EnteredAt = entered; ExpiresAt = entered.AddMinutes 120.0
    EvidenceRef = "https://example/pr" }

let draftKeys =
  [ "schema", "\"fsgg.coord.review-decision/v2\""; "subject", "\"" + subject + "\""
    "revision", "1"; "previousDigest", "null"; "headSha", "\"" + head + "\""
    "claimGeneration", "null"; "baseSha", "null"; "critic", "\"critic-abcd\""
    "verdict", "\"pass\""; "acceptedExceptions", "[]"
    "routeApplicability", "\"not-meaningful\""; "routeEvidence", "[\"x\"]"
    "policyVersion", "\"structured-decisions/1\""; "kind", "\"initial\""; "round", "0"
    "initialReview", "null"; "precedingReview", "null"; "diffAuditRequired", "false"
    "diffAuditReceipts", "[]"; "succession", "null"
    "timestamp", "\"2026-08-22T21:40:00Z\""; "digest", "\"\"" ]
let render xs = "{" + (xs |> List.map (fun (n, v) -> sprintf "\"%s\":%s" n v) |> String.concat ",") + "}"
let engineRequired, engineOptional =
  let r, o =
    draftKeys
    |> List.partition (fun (k, _) ->
        match Driver.decodeStructuredReview (render (draftKeys |> List.filter (fun (n, _) -> n <> k))) with
        | Ok _ -> false | Error _ -> true)
  (r |> List.map fst), (o |> List.map fst)

// Is this value legal for this field, on the engine? Drives the real decoder, then the real validator.
let engineAcceptsVocab (field: string) (value: string) =
  let swap = draftKeys |> List.map (fun (n, v) -> if n = field then n, "\"" + value + "\"" else n, v)
  match Driver.decodeStructuredReview (render swap) with
  | Error _ -> false
  | Ok r ->
      match StructuredDecision.validateReviewLedger subject [ seal r ] with
      | Ok _ -> true
      // A value the decoder accepted but the ledger refuses for an unrelated structural reason is
      // still a legal value for this FIELD; only a refusal naming this field rejects the value itself.
      | Error es -> not (es |> List.exists (fun e -> e.Contains field))

let mutable failures = 0
let check id ok detail =
  if ok then printfn "  ok      %s" id
  else failures <- failures + 1; printfn "  FAILED  %s\n            %s" id detail
let sortedEq (a: string list) b = List.sort a = List.sort b
let j (xs: string list) = String.concat "," xs

// ---- DERIVED FROM THE DOCUMENT, compared against the engine ----
check "doc:required-draft-keys" (sortedEq (strs "requiredKeys") engineRequired)
  (sprintf "document says [%s]; engine requires [%s]" (j (strs "requiredKeys")) (j engineRequired))
check "doc:optional-draft-keys" (sortedEq (strs "optionalKeys") engineOptional)
  (sprintf "document says [%s]; engine treats as optional [%s]" (j (strs "optionalKeys")) (j engineOptional))

let vocab = expected.GetProperty "vocabularies"

// THE CHECKED SET IS DERIVED FROM THE PARSED TABLE, AND SO ARE THE PROBES.
//
// Four rounds found four separate defects in this one loop. Rounds one and two: the loop enumerated
// field names by hand, so rows it did not name went unchecked. Round three bound the row set to the
// document in BOTH directions, which is preserved below and still the right shape. Round four is the
// one that shows the pattern: `timestamp`'s cell was prose, so it took the FREE-FORM branch, whose
// only document-dependent term was `cell.Contains tok` over the single token the engine's refusal
// yielded -- `cell.Contains "ISO-8601"`. Every widening passed it, including the widening that made
// the row FALSE, because the false claim was copied from the message the token came from.
//
// Presence is not an extension. The document now states, per row, what the engine accepts AND what it
// refuses; both columns are probed; and the free-form branch is gone rather than repaired, so no
// future row can reach a substring test by being written as prose.
//
// Which ROWS must exist is still a list here, because nothing in the engine enumerates "the fields
// whose values are drawn from a fixed set" -- a junk-value probe cannot tell a vocabulary from a
// format constraint like `headSha`. That list drifts in neither direction: a row the gate does not
// require fails, and a required row the document drops fails.
let requiredVocabularyRows =
  [ "kind"; "policyVersion"; "routeApplicability"; "schema"; "timestamp"; "verdict" ]

let parsedFields = vocab.EnumerateObject() |> Seq.map (fun p -> p.Name) |> Seq.sort |> List.ofSeq
check "doc:vocabulary-coverage" (parsedFields = List.sort requiredVocabularyRows)
  (sprintf "the document defines rows [%s]; this gate requires rows [%s] -- a row the gate does not require is unchecked, and a required row the document drops means the row was deleted"
     (j parsedFields) (j (List.sort requiredVocabularyRows)))

for field in requiredVocabularyRows do
  let mutable row = Unchecked.defaultof<JsonElement>
  if not (vocab.TryGetProperty(field, &row)) then
    failures <- failures + 1
    printfn "  FAILED  doc:vocabulary:%s\n            the document no longer defines this row" field
  else
    let listOf (n: string) =
      row.GetProperty(n).EnumerateArray() |> Seq.map (fun e -> e.GetString()) |> List.ofSeq
    let accepted, refused = listOf "accepted", listOf "refused"
    // Both directions. Widening the row -- keeping its text and adding a permitted-but-wrong
    // alternative -- moves a literal into `accepted`, and the engine refusing it reds this check.
    let wronglyDocumentedAccepted = accepted |> List.filter (fun v -> not (engineAcceptsVocab field v))
    let wronglyDocumentedRefused = refused |> List.filter (engineAcceptsVocab field)
    check (sprintf "doc:vocabulary:%s" field)
      (List.isEmpty wronglyDocumentedAccepted && List.isEmpty wronglyDocumentedRefused)
      (sprintf "documented as accepted but the engine REFUSES [%s]; documented as refused but the engine ACCEPTS [%s]"
         (j wronglyDocumentedAccepted) (j wronglyDocumentedRefused))

// The empty string is refused for every vocabulary field. It is checked once, here, rather than asked
// of each row, because a backtick pair cannot spell it and a row that tried would parse to nothing.
check "engine:empty-vocabulary-value-refused"
  (requiredVocabularyRows |> List.forall (fun f -> not (engineAcceptsVocab f "")))
  "some vocabulary field now accepts an empty string"

// ---- THE OUTCOME TABLE: rules whose claim is a pair of engine behaviours, not a set of values ----
//
// `acceptedExceptions` ownership and the same-critic exception were both stated as sentences and both
// "checked" by `"<phrase>" in doc`. STEP 1 aborts when such a phrase is absent, so by the time STEP 2
// compared it the flag was a CONSTANT, and the check was an engine self-test with a flag stapled to
// the front. Both were widened in review -- phrase kept, contradicting clause appended -- and both
// stayed green. Giving each rule a case id and an outcome literal gives it an extension to compare.
let confirmationBy (critic: string) (after: ReviewVerdict) =
  let first = seal { baseRec with Verdict = after }
  accepts [ first
            seal { baseRec with Revision = 2; PreviousDigest = Some first.Digest
                                Kind = ReviewKind.Confirmation; Verdict = ReviewVerdict.Pass
                                Round = 1; Critic = critic
                                InitialReview = Some "https://example/c"
                                PrecedingReview = Some "https://example/c" } ]
let exceptionsOn (kind: ReviewKind) (exs: string list) =
  match kind with
  | ReviewKind.Acceptance ->
      let i = seal baseRec
      accepts [ i; seal { baseRec with Revision = 2; PreviousDigest = Some i.Digest
                                       Kind = ReviewKind.Acceptance; Verdict = ReviewVerdict.Accepted
                                       AcceptedExceptions = exs
                                       InitialReview = Some "https://example/c"
                                       PrecedingReview = Some "https://example/c" } ]
  | _ -> accepts [ seal { baseRec with AcceptedExceptions = exs } ]

// What a case MEANS is irreducibly authored -- something must build the situation in F#. What it does
// NOT do any more is drift silently: the case set is compared to the document's in both directions,
// and the outcome each case asserts comes from the validator rather than from this file.
// Each probe declares the RULE it belongs to. The document's `rule` column used to be parsed and read
// by nothing, so renaming a row's rule left the table self-contradictory and the gate green.
let outcomeProbes : (string * string * (unit -> bool)) list =
  [ "successor-critic", "same-critic-after-changes-required",
      fun () -> confirmationBy baseRec.Critic ReviewVerdict.ChangesRequired
    "successor-critic", "different-critic-after-changes-required",
      fun () -> confirmationBy "a-successor-critic" ReviewVerdict.ChangesRequired
    "successor-critic", "same-critic-after-pass",
      fun () -> confirmationBy baseRec.Critic ReviewVerdict.Pass
    "successor-critic", "different-critic-after-pass",
      fun () -> confirmationBy "a-successor-critic" ReviewVerdict.Pass
    "accepted-exceptions", "nonempty-on-initial",
      fun () -> exceptionsOn ReviewKind.Initial [ "waived-x" ]
    "accepted-exceptions", "empty-on-acceptance",
      fun () -> exceptionsOn ReviewKind.Acceptance []
    "accepted-exceptions", "nonempty-on-acceptance",
      fun () -> exceptionsOn ReviewKind.Acceptance [ "waived-x" ] ]

let outcomeRules = expected.GetProperty "outcomeRules"
let documentedCases =
  outcomeRules.EnumerateObject() |> Seq.map (fun p -> p.Name) |> Seq.sort |> List.ofSeq
let probedCases = outcomeProbes |> List.map (fun (_, c, _) -> c) |> List.sort
check "doc:outcome-coverage" (documentedCases = probedCases)
  (sprintf "the document names cases [%s]; this gate probes [%s] -- a documented case with no probe is unchecked, and a probe with no row means the row was deleted"
     (j documentedCases) (j probedCases))

for rule, case, probe in outcomeProbes do
  let mutable row = Unchecked.defaultof<JsonElement>
  if not (outcomeRules.TryGetProperty(case, &row)) then
    failures <- failures + 1
    printfn "  FAILED  doc:outcome:%s\n            the document has no row for this case" case
  else
    let documented = row.GetProperty("outcome").GetString()
    let documentedRule = row.GetProperty("rule").GetString()
    let observed = if probe () then "accepted" else "refused"
    check (sprintf "doc:outcome:%s" case) (documented = observed && documentedRule = rule)
      (sprintf "the document files this case under rule %s and says the validator %s it; the gate probes it under %s and the validator %s it"
         documentedRule documented rule observed)

check "doc:wait-enter-keys"
  (sortedEq (strs "waitEnterKeys") (keysOf (ReviewWait.encode (ReviewWait.Transition.Enter receipt))))
  (sprintf "document's enter example has [%s]; the encoder emits [%s]"
     (j (strs "waitEnterKeys")) (j (keysOf (ReviewWait.encode (ReviewWait.Transition.Enter receipt)))))
// THE EXAMPLE'S VALUE TYPES, not only its key names. `sorted(json.loads(...).keys())` discarded every
// value, so `"claimGeneration": "5382700300"` could become `5382700300` and nothing reddened -- while
// the engine's field is a String and `claim --json` emits a NUMERIC markerId, so the obvious idiom is
// refused and this page's example was the only thing that could have said so. A worker on another
// item paid that cost against the CLI help. Parsed-but-unchecked, one level below the vocabulary row
// that had the same shape.
let kindsOf (encoded: string) =
  let json = encoded.Substring(encoded.IndexOf '{')
  use d = JsonDocument.Parse json
  d.RootElement.EnumerateObject()
  |> Seq.map (fun p ->
      p.Name, (match p.Value.ValueKind with
               | JsonValueKind.True | JsonValueKind.False -> "Boolean"
               | k -> string k))
  |> Map.ofSeq
let documentedEnterTypes =
  expected.GetProperty("waitEnterTypes").EnumerateObject()
  |> Seq.map (fun p -> p.Name, p.Value.GetString()) |> Map.ofSeq
let encoderEnterTypes = kindsOf (ReviewWait.encode (ReviewWait.Transition.Enter receipt))
let typeMismatches =
  documentedEnterTypes
  |> Map.toList
  |> List.choose (fun (k, documented) ->
      match Map.tryFind k encoderEnterTypes with
      | Some emitted when emitted = documented -> None
      | Some emitted -> Some (sprintf "%s documented %s, encoder emits %s" k documented emitted)
      | None -> Some (sprintf "%s is in the example but not in the encoder's output" k))
check "doc:wait-enter-value-types" (List.isEmpty typeMismatches) (j typeMismatches)

check "doc:wait-terminal-keys"
  (sortedEq (strs "waitTerminalKeys")
     (keysOf (ReviewWait.encode (ReviewWait.Transition.Complete("g", entered, "e")))))
  (sprintf "document's terminal example has [%s]; the encoder emits [%s]"
     (j (strs "waitTerminalKeys"))
     (j (keysOf (ReviewWait.encode (ReviewWait.Transition.Complete("g", entered, "e"))))))

// The bullet's extension is EVERY round number it names. An appended "except that in a repair-phase
let evidenceOf app n =
  accepts [ seal { baseRec with RouteApplicability = app
                                RouteEvidence = List.init n (fun i -> sprintf "e%d" i) } ]
let cardinalityHolds app d = evidenceOf app d && not (evidenceOf app (d - 1)) && not (evidenceOf app (d + 1))
let renderShape (shape: string) h (k: ReviewWait.Kind) r =
  let wire = match k with ReviewWait.Kind.InitialReview -> "initial-review" | _ -> "repair-confirmation"
  shape.Replace("<headSha>", h).Replace("<kind>", wire).Replace("<round>", string r)
let auth = expected.GetProperty "authorization"

// THE WAIT-STATE COLUMN IS TRANSCRIBED, NOT DERIVED, AND THAT IS DECLARED RATHER THAN HIDDEN.
// Which wait state authorizes which record kind is decided by `authorizeReviewRecordWait` in the CLI,
// behind a live GitHub transport. Nothing in FS.GG.Coord.Core knows it, so this script cannot derive
// it. An earlier revision "checked" it against `if kindName = "initial" then "Waiting" else "Waiting"`
// — a constant compared to itself, which is exactly the transcribed-expectation defect this gate was
// rebuilt to remove. The table below is an honest transcription of the decompiled CLI switch, and it
// is named as such; the TOKEN column beside it really is derived, from generationToken.
let cliWaitState kind = match kind with | "acceptance" -> "Completed" | _ -> "Waiting"

for kindName, waitKind, round in
    [ "initial", Some ReviewWait.Kind.InitialReview, 0
      "confirmation", Some ReviewWait.Kind.RepairConfirmation, 2
      "escalation", Some ReviewWait.Kind.RepairConfirmation, 2
      "repair-phase", Some ReviewWait.Kind.RepairConfirmation, 2
      // The acceptance row is authorized by a COMPLETED wait and carries no generation token. An
      // earlier revision simply never iterated it, so its whole row could be falsified undetected.
      "acceptance", None, 0 ] do
  let mutable row = Unchecked.defaultof<JsonElement>
  if auth.TryGetProperty(kindName, &row) then
    // EVERY cell is a SET now. Each of these three columns previously parsed to its FIRST literal,
    // so widening any of them -- "`repair-confirmation` or `initial-review`", a second token beside
    // the right one -- parsed to an unchanged expectation and left the row green on a falsified page.
    let cellOf (n: string) =
      row.GetProperty(n).EnumerateArray() |> Seq.map (fun e -> e.GetString()) |> List.ofSeq
    let documentedStates = cellOf "waitState"
    let stateOk = documentedStates = [ cliWaitState kindName ]
    // The receipt-kind column was captured by the row regex and then thrown away, so it belonged to no
    // honesty category and nothing checked it: falsifying it left the document self-contradictory and
    // the gate green. It IS derivable -- the wire names come from the encoder -- so it is Derived.
    let documentedReceiptKinds = cellOf "receiptKind"
    let engineReceiptKind =
      match waitKind with
      | None -> "—"
      | Some wk ->
          let enc = ReviewWait.encode (ReviewWait.Transition.Enter { receipt with Kind = wk })
          use d = JsonDocument.Parse(enc.Substring(enc.IndexOf '{'))
          d.RootElement.GetProperty("kind").GetString()
    let receiptKindOk = documentedReceiptKinds = [ engineReceiptKind ]
    let tokenOk, detail =
      match waitKind with
      | None ->
          // No token is expected on this row; the document must not claim one.
          (List.isEmpty (cellOf "token")), "the acceptance row must carry no generation token"
      | Some wk ->
          // EXACTLY ONE token, and it must render to the engine's. `match toks with t :: _ -> t`
          // took the first and discarded the rest, so adding a second alternative beside the correct
          // one -- the widening -- left this green.
          let toks = cellOf "token"
          let engineToken = ReviewWait.generationToken "HEAD" wk round
          let rendered =
            toks |> List.map (fun d -> d.Replace("<headSha>", "HEAD").Replace("<round>", string round))
          (rendered = [ engineToken ]),
          (sprintf "document renders [%s]; the engine's token is exactly %s" (j rendered) engineToken)
    check (sprintf "doc:authorization:%s" kindName) (stateOk && tokenOk && receiptKindOk)
      (sprintf "wait state documented [%s], CLI requires exactly %s; receipt kind documented [%s], engine emits exactly %s; %s"
         (j documentedStates) (cliWaitState kindName) (j documentedReceiptKinds) engineReceiptKind detail)
  else
    failures <- failures + 1
    printfn "  FAILED  doc:authorization:%s\n            the document has no row for this record kind" kindName

// SETS, not first matches. A second example added beside the right one used to parse to the right one.
check "doc:record-subject-form" (strs "recordSubjectExamples" = [ subject ])
  (sprintf "document's record subject example(s) [%s]; the CLI derives exactly %s"
     (j (strs "recordSubjectExamples")) subject)
check "doc:wait-item-form" (strs "waitItemExamples" = [ itemRef.Canonical ])
  (sprintf "document's wait item example(s) [%s]; the canonical ref is exactly %s"
     (j (strs "waitItemExamples")) itemRef.Canonical)
// TRANSCRIBED, NOT DERIVED -- and an earlier revision filed it under Derived while comparing it to a
// string literal three lines above, which is the same mistake in the same file twice. Which fields
// `review record` overwrites, and what it substitutes, is CLI behaviour behind a live transport;
// FS.GG.Coord.Core cannot report it. The transcription below is from the decompiled
// `recordReview$cont@2412-3` and is labelled as a transcription.
// TRANSCRIBED, and now compared EXACTLY rather than by substring.
//
// This was `text.Contains needle` -- five of the thirty `doc:` checks were a literal substring test
// against a needle hard-coded here, sitting in the Derived bucket under a banner reading "no
// expectation is presence-shaped". `cell.Contains tok` under a new name, and the defect that let
// widening the `revision` row with ", or the draft's own revision when the draft supplies a non-null
// one" leave the whole gate at exit 0 while coverage certified the check as inverted.
//
// The full cell is transcribed from the decompiled CLI and compared for equality, so ANY edit to the
// description -- narrowing, widening, or rewording -- moves the comparison.
let cliOverwrites =
  [ "revision", "`existing.Length + 1` — the count of records already on the PR"
    "previousDigest", "the preceding record's digest"
    "claimGeneration", "the live winning claim marker's comment id (acceptance records only)"
    "baseSha", "the PR's live base tip SHA (acceptance records only)"
    "digest", "`StructuredDecision.reviewDigest(record)`, computed over the rebuilt record" ]
check "doc:overwritten-fields"
  (sortedEq (strs "overwrittenFields") (cliOverwrites |> List.map fst))
  (sprintf "document's overwrite table lists [%s]; the CLI overwrites [%s]"
     (j (strs "overwrittenFields")) (j (cliOverwrites |> List.map fst)))
// The second column carried no check at all, so `existing.Length + 1` could become `existing.Length`
// and `(acceptance records only)` could become `(initial records only)` undetected. Each row's
// description must still contain the substitution it names.
let overwriteRows = expected.GetProperty "overwriteDescriptions"
for field, transcribed in cliOverwrites do
  let mutable desc = Unchecked.defaultof<JsonElement>
  if overwriteRows.TryGetProperty(field, &desc) then
    let text = desc.GetString()
    check (sprintf "doc:overwrite-description:%s" field) (text = transcribed)
      (sprintf "the %s row reads %s; the transcribed CLI behaviour is %s" field text transcribed)
  else
    failures <- failures + 1
    printfn "  FAILED  doc:overwrite-description:%s\n            no row for this field" field

// The wait window ceiling, derived from the document and checked against the pure validator.
let validWindow (hours: float) =
  let entered = DateTimeOffset.UtcNow
  let r : ReviewWait.WaitReceipt =
    { receipt with EnteredAt = entered; ExpiresAt = entered.AddHours hours }
  match ReviewWait.validate (ReviewWait.Transition.Enter r) with Ok _ -> true | Error _ -> false
// ENGINE-SCOPED, and it was misnamed `doc:` until the coverage requirement asked what inverted it.
// Nothing does: the predicate takes no term from the document, so no mutation of the document can
// red it. A `doc:` name on a check with no document input is its own small over-claim -- it reports
// the page as bound where only the engine is being probed.
check "engine:wait-window-must-be-positive"
  (not (validWindow 0.0) && not (validWindow -1.0))
  "the validator no longer requires expiresAt to be later than enteredAt"

// The two invariants this page adds for a successor critic are Derived, so they are checked. Adding
// a documented claim without adding its check is how the previous three rounds each produced an
// unchecked Derived row; a new claim arrives with its probe or it does not arrive.
let crInitial = seal { baseRec with Verdict = ReviewVerdict.ChangesRequired }
let confirmationAt (round: int) (prev: ReviewRecord) (rev: int) (critic: string) =
  seal { baseRec with Revision = rev; PreviousDigest = Some prev.Digest
                      Kind = ReviewKind.Confirmation; Verdict = ReviewVerdict.Pass; Round = round
                      Critic = critic
                      InitialReview = Some "https://example/c"; PrecedingReview = Some "https://example/c" }
let firstConfOk n = accepts [ crInitial; confirmationAt n crInitial 2 baseRec.Critic ]
let c1 = seal { (confirmationAt 1 crInitial 2 baseRec.Critic) with Verdict = ReviewVerdict.ChangesRequired }
let secondConfOk n = accepts [ crInitial; c1; confirmationAt n c1 3 baseRec.Critic ]

// ---- THE SCALAR TABLE, PROBED THE SAME WAY THE VOCABULARY TABLE IS ------------------------------
//
// Five claims that were sentences carrying a number. Each round's repair widened a regex to admit the
// counterexample the previous critic supplied, and the next critic defeated it by rephrasing -- the
// number as a word, in a different position, in a clause the pattern did not anticipate. A prose
// claim has no enumerable cell, so its parse is phrasing-specific, and a phrasing-specific parse is
// presence-shaped for every phrasing nobody thought of. There is no terminating sequence of patches.
//
// So the values live in a table with an accepted and a refused column, and every literal in both is
// probed against the engine -- the identical shape the vocabulary table uses, which is the only shape
// in this file that has never escaped. The probe for each row is here; the SET of rows is compared to
// the document's in both directions, so adding a row without a probe fails and deleting one fails.
let scalars = expected.GetProperty "scalars"
let scalarProbes : (string * (string -> bool)) list =
  [ "wait-window-max-hours",
      fun v -> match Int32.TryParse v with
               | true, h -> validWindow (float h) && not (validWindow (float h + 1.0))
               | _ -> false
    "initial-round",
      fun v -> match Int32.TryParse v with
               | true, r -> accepts [ seal { baseRec with Round = r } ]
               | _ -> false
    "confirmation-round-first",
      fun v -> match Int32.TryParse v with true, r -> firstConfOk r | _ -> false
    "confirmation-round-second",
      fun v -> match Int32.TryParse v with true, r -> secondConfOk r | _ -> false
    "meaningful-evidence-count",
      fun v -> match Int32.TryParse v with true, n -> cardinalityHolds "meaningful" n | _ -> false
    "not-meaningful-evidence-count",
      fun v -> match Int32.TryParse v with true, n -> cardinalityHolds "not-meaningful" n | _ -> false
    "generation-token-shape",
      fun v -> renderShape v "HEAD" ReviewWait.Kind.InitialReview 0
                 = ReviewWait.generationToken "HEAD" ReviewWait.Kind.InitialReview 0
               && renderShape v "HEAD" ReviewWait.Kind.RepairConfirmation 2
                 = ReviewWait.generationToken "HEAD" ReviewWait.Kind.RepairConfirmation 2 ]

let documentedScalars = scalars.EnumerateObject() |> Seq.map (fun p -> p.Name) |> Seq.sort |> List.ofSeq
let probedScalars = scalarProbes |> List.map fst |> List.sort
check "doc:scalar-coverage" (documentedScalars = probedScalars)
  (sprintf "the document names scalar rows [%s]; this gate probes [%s]"
     (j documentedScalars) (j probedScalars))

for key, probe in scalarProbes do
  let mutable row = Unchecked.defaultof<JsonElement>
  if not (scalars.TryGetProperty(key, &row)) then
    failures <- failures + 1
    printfn "  FAILED  doc:scalar:%s\n            the document has no scalar row for this invariant" key
  else
    let listOf (n: string) =
      row.GetProperty(n).EnumerateArray() |> Seq.map (fun e -> e.GetString()) |> List.ofSeq
    let accepted, refused = listOf "accepted", listOf "refused"
    let wrongAccepted = accepted |> List.filter (probe >> not)
    let wrongRefused = refused |> List.filter probe
    check (sprintf "doc:scalar:%s" key)
      (List.isEmpty wrongAccepted && List.isEmpty wrongRefused && not (List.isEmpty accepted))
      (sprintf "documented as the value but the engine REFUSES [%s]; documented as refused but the engine ACCEPTS [%s]"
         (j wrongAccepted) (j wrongRefused))

// ---- GOVERNED REGIONS: no value may be restated inside a MARKED region ---------------------------
//
// SCOPE, because an earlier version of this comment claimed the document-wide property the footer
// has since retired -- and a retired claim left standing in a comment is how the next reader
// reinstates it. What holds: each scalar row has exactly one `scalar-governed` region; every region
// is compared to its transcription for equality; a second region repeating an id is refused; and a
// marker whose id list is unreadable is refused rather than skipped. Prose OUTSIDE every marked
// region is NOT examined by this check, in any phrasing. The passing footer prints that boundary,
// and this comment must not outlive it.
let governedViolations =
  expected.GetProperty("governedViolations").EnumerateArray()
  |> Seq.map (fun e -> e.GetString()) |> List.ofSeq
check "doc:scalar-region-purity" (List.isEmpty governedViolations)
  (sprintf "a governed region restates a value that belongs in the scalar table: %s"
     (String.concat " | " governedViolations))

// ---- engine facts the document does not parameterise ----
check "engine:digest-ignores-its-own-field"
  (StructuredDecision.reviewDigest { baseRec with Digest = "" }
     = StructuredDecision.reviewDigest { baseRec with Digest = "deadbeef" }) ""
check "engine:tampered-digest-refused" (not (accepts [ { initial with Digest = "deadbeef" } ])) ""
check "engine:same-critic-required"
  (let acc = seal { baseRec with Revision = 2; PreviousDigest = Some initial.Digest
                                 Kind = ReviewKind.Acceptance; Verdict = ReviewVerdict.Accepted
                                 InitialReview = Some "u"; PrecedingReview = Some "u" }
   accepts [ initial; acc ] && not (accepts [ initial; seal { acc with Critic = "someone-else" } ])) ""

// ---- KNOWN-BLIND: the in-process route accepts what the production CLI refuses ----
let blindSubject (s: string) =
  match StructuredDecision.validateReviewLedger s [ seal { baseRec with Subject = s } ] with
  | Ok _ -> true | Error _ -> false
check "known-blind:validator-cannot-check-the-subject"
  (blindSubject itemRef.Canonical && blindSubject subject && blindSubject "not-even-a-ref")
  "the in-process validator now discriminates subjects — the document's rule needs revisiting"

exit (if failures = 0 then 0 else 1)
FSX

python3 - "$tmp/probe.fsx" "$core" "$tmp/expected.json" <<'PY'
import sys, pathlib
p = pathlib.Path(sys.argv[1])
p.write_text(p.read_text().replace("@@CORE@@", sys.argv[2]).replace("@@EXPECTED@@", sys.argv[3]))
PY

if [[ -n "$inner" ]]; then
  # Inner run: only the engine comparison matters, and quietly.
  exec dotnet fsi "$tmp/probe.fsx"
fi

echo "engine conformance (pinned fs.gg.coord.cli $pinned) — expectations parsed from the document:"
if ! dotnet fsi "$tmp/probe.fsx" | tee "$tmp/clean.txt"; then
  echo "docs/coordination-engine-contracts.md disagrees with the engine it documents." >&2
  exit 1
fi

facts_schema=$("$repo_root/scripts/fsgg-coord" facts --json \
  | python3 -c 'import json,sys; print(json.load(sys.stdin)["reviewPolicy"]["schema"])')
grep -qF "$facts_schema" "$doc" || {
  echo "the engine's facts report reviewPolicy.schema=$facts_schema, which the document never names." >&2
  exit 1; }
echo "  ok      engine:facts-reviewPolicy-schema ($facts_schema)"

# ---------------------------------------------------------------------------------------------------
# STEP 3 — every DERIVED claim must red when the document is falsified, AND the gate must be able to
# name the check that caught it.
#
# WHAT WENT WRONG HERE, MEASURED. The previous revision ran 21 hand-written mutations and scored each
# one on whether the inner run exited non-zero. Attributing each red to the check it actually
# reddened showed that THREE of the 21 reddened no check at all: they tripped STEP 1's parser, and a
# parse abort scored identically to a detection. `doc:record-subject-form`,
# `doc:accepted-exceptions-owner` and `doc:same-critic-exception` therefore carried inversion evidence
# while never having fired. Two of the three were then shown to be vacuous by a WIDENING mutation --
# keep the sentence, append a contradicting clause -- which is precisely the class a hand-written
# catalogue of deletions and replacements never contained.
#
# The anchor guard is the same defect one layer out: a widening that changes a row's text removes the
# byte-exact anchor of the mutation meant to catch it, `sys.exit` fires, `set -e` aborts the run, and
# the abort is scored a detection. A mechanism whose passing condition is satisfied by its own
# breakage cannot distinguish "the check caught it" from "the fixture no longer applies".
#
# So three properties now hold, and each one failed at least once above:
#
#   1. ATTRIBUTED. Every mutation declares the check id it must red, and that exact id must appear as
#      FAILED. A parse abort reddens nothing and is reported as NO DETECTION, not as evidence.
#   2. COVERED. Every `doc:*` check the clean run emits must be reddened by some mutation, or this
#      step fails naming the uncovered ids. A claim added to the document without an inversion now
#      fails the gate instead of passing silently -- that default is what produced four recurrences.
#   3. DERIVED. The widening mutations are constructed from the document's own `must be refused`
#      column, so a row added later gets its widening for free and no hand-written list can fall
#      behind the table it is supposed to defend.
#
# A missing anchor is a FIXTURE failure and is reported as one. It is never a detection.
# ---------------------------------------------------------------------------------------------------

echo
echo "document inversion — each mutation names the check it must red:"

mutants="$tmp/mutants"; mkdir -p "$mutants"

python3 - "$doc" "$mutants" <<'PY'
import os, pathlib, re, sys

doc_path, outdir = sys.argv[1], sys.argv[2]
text = pathlib.Path(doc_path).read_text(encoding="utf-8")
ticked = lambda s: re.findall(r"`([^`]+)`", s)
VOCAB_ROWS = ("kind", "policyVersion", "routeApplicability", "schema", "timestamp", "verdict")
# A value no row lists in either column, so substituting it can never collide with the document's own
# literals. Every one of these fields is a closed vocabulary or a parsed instant, so it is refused.
NOT_A_VALUE = "not-a-value"

manifest, problems = [], []

# DIRECTION IS PART OF THE EVIDENCE, not an afterthought.
#
# Repair-phase round 1, finding F1. The previous revision required every `doc:*` check to be reddened
# by SOME attributed mutation and said nothing about which KIND. Auto-derived widenings existed for
# three families; 22 of 30 checks were covered by hand-written replacements alone -- the narrowing
# direction, which is the one three ordinary rounds had already caught. Coverage therefore certified
# "inverted" on the strength of the safe direction and stayed silent on the dangerous one, and six
# widenings survived in rows a critic had already verified.
#
# Coverage-by-any-mutation is the presence-carrying defect one level up: it establishes that *a*
# mutation reds, not that the *dangerous* mutation does. So every mutation declares its direction,
# every `doc:*` check must carry BOTH, and a check for which no widening can be constructed must say
# so explicitly with a reason -- never silently.
NARROW, WIDEN = "narrow", "widen"

def emit(check_id, direction, name, mutated):
    slug = re.sub(r"[^a-z0-9]+", "-", ("%s %s %s" % (check_id, direction, name)).lower()).strip("-")[:100]
    path = os.path.join(outdir, slug + ".md")
    pathlib.Path(path).write_text(mutated, encoding="utf-8")
    manifest.append((check_id, direction, name, path))

exact = []
def edit(check_id, direction, name, old, new):
    """An exact (old -> new) edit. A missing anchor is a fixture failure, never a detection."""
    exact.append(check_id)
    if old not in text:
        problems.append("%s (%s): mutation anchor not present: %r" % (check_id, name, old[:70]))
        return
    emit(check_id, direction, name, text.replace(old, new, 1))

def widen_cell(check_id, header, row_key, col, addition, name):
    """Append a permitted-but-wrong alternative to one table cell, keeping every other word of the row.
    This is the shape that survived three ordinary rounds and the first repair-phase attempt."""
    body, at = section(header)
    m = re.search(r"^\|\s*`%s`\s*\|(.+)$" % re.escape(row_key), body, re.M)
    if not m:
        problems.append("%s: no row `%s` in %s to widen" % (check_id, row_key, header))
        return
    cells = [c for c in m.group(1).split("|")]
    if len(cells) <= col:
        problems.append("%s: row `%s` has %d cells, wanted column %d" % (check_id, row_key, len(cells), col))
        return
    cells[col] = cells[col].rstrip() + addition
    s, e = at + m.start(), at + m.end()
    emit(check_id, WIDEN, name, text[:s] + "| `%s` |" % row_key + "|".join(cells) + text[e:])

def section(header, stop=("\n## ", "\n### ")):
    i = text.find(header)
    if i < 0:
        problems.append("section not found: %r" % header)
        return "", 0
    j = len(text)
    for s in stop:
        k = text.find(s, i + len(header))
        if k >= 0:
            j = min(j, k)
    return text[i:j], i

# --- DERIVED: the vocabulary table mutates itself -------------------------------------------------
vocab_text, vocab_at = section("### Vocabularies")
rows = [m for m in re.finditer(r"^\|\s*`([a-zA-Z]+)`\s*\|\s*(.+?)\s*\|\s*(.+?)\s*\|\s*$", vocab_text, re.M)
        if m.group(1) in VOCAB_ROWS]
if len(rows) != len(VOCAB_ROWS):
    problems.append("expected %d vocabulary rows to mutate, found %d" % (len(VOCAB_ROWS), len(rows)))
for m in rows:
    key, acc, ref = m.group(1), m.group(2).strip(), m.group(3).strip()
    accepted, refused = ticked(acc), ticked(ref)
    if not accepted or not refused:
        problems.append("row `%s` cannot be mutated: it names no accepted or no refused literal" % key)
        continue
    s, e = vocab_at + m.start(), vocab_at + m.end()
    # WIDENING. Every word of the row is kept and a permitted-but-wrong alternative is added. This is
    # the class that survived three review rounds, so it is the one the gate derives rather than
    # trusts an author to remember.
    if len(refused) < 2:
        problems.append("row `%s` names only one refused literal, so it cannot be widened without "
                        "emptying its refused column" % key)
        continue
    # The literal MOVES: accepted gains it and refused loses it, which is exactly what an author
    # widening this row would write. Leaving it in both columns is a self-contradiction the parser
    # refuses, and a mutant that cannot be parsed reddens the run without reaching its check.
    ref_without_first = re.sub(r"^`[^`]+`,\s*", "", ref)
    emit("doc:vocabulary:%s" % key, WIDEN, "row %s WIDENED with the refused value %s" % (key, refused[0]),
         text[:s] + "| `%s` | %s, `%s` | %s |" % (key, acc, refused[0], ref_without_first) + text[e:])
    # NARROWING. Substitution, not deletion, so a single-literal row still parses -- a row mutated
    # into an unparseable state would red the run without ever reaching its check.
    emit("doc:vocabulary:%s" % key, NARROW, "row %s narrowed by substituting %s" % (key, NOT_A_VALUE),
         text[:s] + "| `%s` | %s | %s |" % (key, acc.replace("`%s`" % accepted[0], "`%s`" % NOT_A_VALUE, 1), ref) + text[e:])
if rows:
    m = rows[-1]
    s, e = vocab_at + m.start(), vocab_at + m.end()
    emit("doc:vocabulary-coverage", NARROW, "the %s row is deleted outright" % m.group(1),
         text[:s].rstrip("\n") + "\n" + text[e:].lstrip("\n"))

# --- DERIVED: the scalar table mutates itself, exactly as the vocabulary table does ---------------
scal_text, scal_at = section("### Scalar invariants")
scal_rows = list(re.finditer(r"^\|\s*`([a-z-]+)`\s*\|\s*(.+?)\s*\|\s*(.+?)\s*\|\s*$", scal_text, re.M))
if not scal_rows:
    problems.append("the scalar invariants table has no rows to mutate")
for m in scal_rows:
    key, acc, ref = m.group(1), m.group(2).strip(), m.group(3).strip()
    accepted, refused = ticked(acc), ticked(ref)
    if not accepted or len(refused) < 2:
        problems.append("scalar row `%s` cannot be mutated: it needs one accepted and two refused literals" % key)
        continue
    s, e = scal_at + m.start(), scal_at + m.end()
    ref_tail = re.sub(r"^`[^`]+`,\s*", "", ref)
    emit("doc:scalar:%s" % key, WIDEN, "scalar %s WIDENED with the refused value %s" % (key, refused[0]),
         text[:s] + "| `%s` | %s, `%s` | %s |" % (key, acc, refused[0], ref_tail) + text[e:])
    # Substituted value must appear in NEITHER column: putting a documented-refused literal into the
    # accepted cell makes the row self-contradictory, which the parser refuses, and a mutant that
    # cannot be parsed reds the run without ever reaching its check.
    sub_val = "<round>:<kind>:<headSha>" if "<" in accepted[0] else "99"
    emit("doc:scalar:%s" % key, NARROW, "scalar %s narrowed by substituting %s" % (key, sub_val),
         text[:s] + "| `%s` | %s | %s |" % (key, acc.replace("`%s`" % accepted[0], "`%s`" % sub_val, 1), ref) + text[e:])
_last = scal_rows[-1]
_s, _e = scal_at + _last.start(), scal_at + _last.end()
emit("doc:scalar-coverage", NARROW, "the %s scalar row is deleted outright" % _last.group(1),
     text[:_s].rstrip("\n") + "\n" + text[_e:].lstrip("\n"))
emit("doc:scalar-coverage", WIDEN, "the scalar table gains a row nothing probes",
     text[:_s] + "| `repair-phase-round-ceiling` | `10` | `3`, `13` |\n" + text[_s:])

# --- DERIVED: a value smuggled back into governed prose, in three different phrasings -------------
# Round 2's critic defeated each previous repair by REPHRASING, so the purity rule is inverted with
# phrasings no previous round supplied: a bare digit, a spelled-out word, and a shape token.
_gov = list(re.finditer(r"(<!-- scalar-governed:[a-z,-]+ -->)", text))
if _gov:
    _g = _gov[0]
    for _label, _inject in (
        ("a bare digit", " The ceiling is 24 hours."),
        ("a spelled-out number", " The ceiling is twenty-four hours."),
        ("a shape token", " The token renders as <kind>:<headSha>.")):
        emit("doc:scalar-region-purity", WIDEN,
             "%s is smuggled into a governed region" % _label,
             text[:_g.end()] + _inject + text[_g.end():])
    # A SECOND REGION BEARING AN ID THE DOCUMENT ALREADY GOVERNS. Every widening above injects
    # inside `_gov[0]`, so until this one the check had never been shown against a restatement that
    # arrives as its OWN region -- the case round 3 left first-match and round 4 closed. The
    # injected sentence carries no digit, no listed number word and no shape token, so the token
    # rules cannot see it: only the id-uniqueness rule reds, which is the point of adding it.
    _full = re.search(r"<!-- scalar-governed:([a-z,-]+) -->(.*?)<!-- /scalar-governed -->", text, re.S)
    if _full:
        emit("doc:scalar-region-purity", WIDEN,
             "a second region repeats an id already governed and restates the value with no numeral",
             text[:_full.end()]
             + "\n\n<!-- scalar-governed:%s -->\nIn a repair-phase chain the validator increments "
               "that round by a single unit before comparing it.\n<!-- /scalar-governed -->"
               % _full.group(1)
             + text[_full.end():])
    else:
        problems.append("no complete governed region found to duplicate")
    # AN UNREADABLE MARKER. A label of separators alone matches the region pattern but resolves to
    # no ids, so before round 5 every rule keyed on an id iterated over nothing and the run printed
    # a confident `ok`. The injected sentence carries no digit, no listed number word and no shape
    # token, so the token rules cannot see it either: only the fail-closed unreadability predicate
    # reds it. This is the inversion of a FAILED READ, which is the sub-shape a happy-path mutation
    # never reaches -- the gate must refuse what it cannot parse, not decide about it.
    if _full:
        emit("doc:scalar-region-purity", WIDEN,
             "a region marker whose id list is unreadable carries a restated value",
             text[:_full.end()]
             + "\n\n<!-- scalar-governed:, -->\nIn a repair-phase chain the validator increments "
               "that round by a single unit before comparing it.\n<!-- /scalar-governed -->"
             + text[_full.end():])

    # The narrowing direction for purity is removing the governance itself: a scalar row whose prose
    # region has gone unmarked can restate the value freely again.
    emit("doc:scalar-region-purity", NARROW, "a governed region loses its marker",
         text[:_g.start()] + text[_g.end():])
else:
    problems.append("no governed region found to mutate")

# --- DERIVED: the draft-key lists widen with each other's members ---------------------------------
req = re.search(r"\*\*Required\*\*.*?:\s*\n\n(.+?)\n\n", text, re.S)
opt = re.search(r"\*\*Optional\*\*.*?:\s*\n\n(.+?)\n\n", text, re.S)
if req and opt:
    required_keys, optional_keys = ticked(req.group(1)), ticked(opt.group(1))
    if required_keys and optional_keys:
        emit("doc:required-draft-keys", WIDEN,
             "required list WIDENED with the optional key %s" % optional_keys[0],
             text[:req.end(1)] + ", `%s`" % optional_keys[0] + text[req.end(1):])
        emit("doc:optional-draft-keys", WIDEN,
             "optional list WIDENED with the required key %s" % required_keys[0],
             text[:opt.end(1)] + ", `%s`" % required_keys[0] + text[opt.end(1):])
    else:
        problems.append("a draft-key list parsed to no literals")
else:
    problems.append("could not locate the draft-key lists to mutate")

# --- DERIVED: every outcome cell flips ------------------------------------------------------------
oc_text, oc_at = section("### Rules with two outcomes")
oc_rows = list(re.finditer(r"^\|\s*`([a-z-]+)`\s*\|\s*`([a-z-]+)`\s*\|\s*`(accepted|refused)`\s*\|\s*$",
                           oc_text, re.M))
if not oc_rows:
    problems.append("the outcome table has no rows to mutate")
for m in oc_rows:
    rule, case, outcome = m.group(1), m.group(2), m.group(3)
    flipped = "refused" if outcome == "accepted" else "accepted"
    s, e = oc_at + m.start(), oc_at + m.end()
    # Flipping `refused` to `accepted` is the widening direction for a rule: it claims the validator
    # permits something it does not. That is the shape the two prose versions of these rules survived.
    emit("doc:outcome:%s" % case, WIDEN if flipped == "accepted" else NARROW,
         "outcome for %s %s to %s" % (case, "WIDENED" if flipped == "accepted" else "narrowed", flipped),
         text[:s] + "| `%s` | `%s` | `%s` |" % (rule, case, flipped) + text[e:])
if oc_rows:
    m = oc_rows[-1]
    s, e = oc_at + m.start(), oc_at + m.end()
    emit("doc:outcome-coverage", NARROW, "the %s row is deleted outright" % m.group(2),
         text[:s].rstrip("\n") + "\n" + text[e:].lstrip("\n"))

# --- EXACT: claims with no derivable mutation form ------------------------------------------------
edit("doc:optional-draft-keys", NARROW, "the optional key list silently drops succession",
     "`diffAuditRequired`, `diffAuditReceipts`, `succession`",
     "`diffAuditRequired`, `diffAuditReceipts`")
edit("doc:required-draft-keys", NARROW, "D1 required key list silently drops digest",
     "`kind`, `round`, `timestamp`, `digest`", "`kind`, `round`, `timestamp`")
edit("doc:overwritten-fields", NARROW, "D4 overwrite table loses a field",
     "| `previousDigest` |", "| `previousDigestXX` |")
edit("doc:wait-enter-keys", NARROW, "D5 the enter example drops a required key",
     '  "kind": "initial-review",\n', "")
edit("doc:wait-enter-value-types", NARROW, "S17 claimGeneration becomes the number claim --json emits",
     '"claimGeneration": "5382700300"', '"claimGeneration": 5382700300')
edit("doc:authorization:initial", NARROW, "D7 authorization table gives initial the wrong token",
     "| `initial` | `Waiting` | `initial-review` | `<headSha>:initial-review:0` |",
     "| `initial` | `Waiting` | `repair-confirmation` | `<headSha>:repair-confirmation:0` |")
edit("doc:authorization:initial", NARROW, "S10 the authorization receipt-kind column is falsified",
     "| `initial` | `Waiting` | `initial-review` |", "| `initial` | `Waiting` | `repair-confirmation` |")
edit("doc:authorization:acceptance", NARROW, "S4 the acceptance row wait state is falsified",
     "| `acceptance` | `Completed` |", "| `acceptance` | `Waiting` |")
edit("doc:authorization:confirmation", NARROW, "S5 an ordinary row wait state is falsified",
     "| `confirmation` | `Waiting` |", "| `confirmation` | `Completed` |")
# S1 ALTERS the subject rather than deleting it. The deleting form reddened the run through STEP 1's
# parser while `doc:record-subject-form` never fired -- inversion evidence for a check that had never
# been executed, which is the exact shape this step now refuses to accept.
edit("doc:record-subject-form", NARROW, "S1 the record subject names the wrong PR",
     "EHotwagner/S.I.R.#255/pr/259` |", "EHotwagner/S.I.R.#255/pr/999` |")
edit("doc:overwrite-description:revision", NARROW, "S8 the revision substitution loses its off-by-one",
     "`existing.Length + 1` — the count of records", "`existing.Length` — the count of records")
edit("doc:overwrite-description:claimGeneration", NARROW, "S9 an overwrite row is rescoped to the wrong record kind",
     "the live winning claim marker's comment id (acceptance records only)",
     "the live winning claim marker's comment id (initial records only)")
edit("doc:wait-terminal-keys", NARROW, "S18 the terminal example drops a required key",
     '  "at": "2026-08-22T22:10:00.0000000+00:00",\n', "")
edit("doc:authorization:escalation", NARROW, "S22 the escalation row wait state is falsified",
     "| `escalation` | `Waiting` |", "| `escalation` | `Completed` |")
edit("doc:authorization:repair-phase", NARROW, "S23 the repair-phase row wait state is falsified",
     "| `repair-phase` | `Waiting` |", "| `repair-phase` | `Completed` |")
edit("doc:wait-item-form", NARROW, "S24 the wait item example names the wrong item",
     "| `review wait` | `item` | the canonical item ref | `EHotwagner/S.I.R.#255` |",
     "| `review wait` | `item` | the canonical item ref | `EHotwagner/S.I.R.#254` |")
edit("doc:overwrite-description:previousDigest", NARROW, "S25 the previousDigest substitution points the wrong way",
     "| `previousDigest` | the preceding record's digest |",
     "| `previousDigest` | the following record's digest |")
edit("doc:overwrite-description:baseSha", NARROW, "S26 the baseSha row is rescoped to the wrong record kind",
     "the PR's live base tip SHA (acceptance records only)",
     "the PR's live base tip SHA (initial records only)")
edit("doc:overwrite-description:digest", NARROW, "S27 the digest substitution names the wrong function",
     "`StructuredDecision.reviewDigest(record)`, computed over the rebuilt record",
     "`StructuredDecision.recordDigest(record)`, computed over the rebuilt record")

# --- WIDENING SITES: keep every word of the row, add a permitted-but-wrong alternative -------------
for k in ("initial", "confirmation", "escalation", "repair-phase"):
    widen_cell("doc:authorization:%s" % k, "### Which wait entry authorizes which record", k, 1,
               " or `initial-review`", "authorization %s receipt-kind WIDENED" % k)
widen_cell("doc:authorization:acceptance", "### Which wait entry authorizes which record",
           "acceptance", 0, " or `Waiting`", "authorization acceptance wait-state WIDENED")
widen_cell("doc:authorization:escalation", "### Which wait entry authorizes which record",
           "escalation", 2, ", or `<headSha>:escalation:<round>`", "authorization escalation token WIDENED")
for f in ("revision", "previousDigest", "claimGeneration", "baseSha", "digest"):
    widen_cell("doc:overwrite-description:%s" % f, "## The one thing to read first", f, 0,
               ", or the draft's own value when it supplies a non-null one",
               "overwrite description %s WIDENED with an alternative" % f)
widen_cell("doc:record-subject-form", "### `subject` is NOT the item ref", "review record", 2,
           " or `EHotwagner/S.I.R.#255/pr/285`", "record subject example WIDENED with a second ref")
widen_cell("doc:wait-item-form", "### `subject` is NOT the item ref", "review wait", 2,
           " or `EHotwagner/S.I.R.#254`", "wait item example WIDENED with a second ref")
edit("doc:overwritten-fields", WIDEN, "the overwrite table WIDENED with a field the CLI does not overwrite",
     "| `digest` | `StructuredDecision.reviewDigest(record)`, computed over the rebuilt record |",
     "| `digest` | `StructuredDecision.reviewDigest(record)`, computed over the rebuilt record |\n"
     "| `critic` | the critic id, substituted from the wait receipt |")
edit("doc:wait-enter-keys", WIDEN, "the enter example WIDENED with a key the encoder does not emit",
     '  "evidenceRef": "https://github.com/EHotwagner/S.I.R./pull/257"\n',
     '  "evidenceRef": "https://github.com/EHotwagner/S.I.R./pull/257",\n  "criticHint": "optional"\n')
edit("doc:wait-terminal-keys", WIDEN, "the terminal example WIDENED with a key the encoder does not emit",
     '  "evidenceRef": "https://github.com/EHotwagner/S.I.R./pull/257#issuecomment-…"\n',
     '  "evidenceRef": "https://github.com/EHotwagner/S.I.R./pull/257#issuecomment-…",\n  "note": "optional"\n')
edit("doc:vocabulary-coverage", WIDEN, "the vocabulary table WIDENED with a row nothing probes",
     "| `timestamp` |", "| `critic` | `a-critic-id` | `not-a-critic-id` |\n| `timestamp` |")
edit("doc:outcome-coverage", WIDEN, "the outcome table WIDENED with a case nothing probes",
     "| `accepted-exceptions` | `nonempty-on-initial` | `accepted` |",
     "| `accepted-exceptions` | `nonempty-on-initial` | `accepted` |\n"
     "| `accepted-exceptions` | `nonempty-on-escalation` | `accepted` |")

# --- WIDENINGS FOR THE CHECKS ROUND 1 EXEMPTED ON FALSE REASONS (critic F3, X1-X4) --------------
edit("doc:wait-enter-value-types", WIDEN, "X4 a second enter example types claimGeneration as a number",
     "### The `complete`, `cancel` and `timeout` events",
     "Also accepted with `claimGeneration` written as the number `claim --json` emits:\n\n"
     "```json\n{\n  \"schema\": \"fsgg.coord.review-wait/v1\",\n  \"event\": \"enter\",\n"
     "  \"item\": \"EHotwagner/S.I.R.#255\",\n  \"claimGeneration\": 5382700300,\n"
     "  \"reviewGeneration\": \"<headSha>:initial-review:0\",\n  \"kind\": \"initial-review\",\n"
     "  \"enteredAt\": \"2026-08-22T21:40:00.0000000+00:00\",\n"
     "  \"expiresAt\": \"2026-08-22T23:40:00.0000000+00:00\",\n"
     "  \"evidenceRef\": \"https://github.com/EHotwagner/S.I.R./pull/257\"\n}\n```\n\n"
     "### The `complete`, `cancel` and `timeout` events")
# The five outcome rows round 1 chained under one "as above" reason. The critic tested them by
# construction and found the reason unnecessary: widening an `accepted` cell reds via the row's own
# named check. Five exemptions resting on one unverified sentence is a single point of failure.
for _case in ("same-critic-after-changes-required", "different-critic-after-changes-required",
              "same-critic-after-pass", "nonempty-on-initial", "empty-on-acceptance"):
    _rule = "successor-critic" if _case.endswith(("changes-required", "after-pass")) else "accepted-exceptions"
    edit("doc:outcome:%s" % _case, WIDEN,
         "the %s outcome cell gains an alternative" % _case,
         "| `%s` | `%s` | `accepted` |" % (_rule, _case),
         "| `%s` | `%s` | `accepted`, or `refused` once the head is rebased |" % (_rule, _case))

# --- Checks for which NO widening can be constructed, each with its reason. -------------------------
# This registry is the honest half of the direction requirement: `NOT_MEASURED`, never a pass. A check
# that appears in neither the widening manifest nor here fails the run, so a claim added later cannot
# quietly inherit narrowing-only evidence the way 22 checks did.
# The ONLY exemption registry. Its widening counterpart was deleted with F4 and is not coming back:
# a widening is the class that defeated every round here, so it gets no excuse column. A check with
# no narrowing must still say so
# rather than appear covered. Both registries are printed on every pass, so "which direction has this
# check actually been shown to catch?" is answerable by reading the run rather than the source.
NO_NARROWING = {
  "doc:outcome:different-critic-after-pass":
    "the cell states a refusal; narrowing it would mean claiming something MORE is refused, which for "
    "a two-valued outcome cell is the widening of the opposite row and is covered there",
  "doc:outcome:nonempty-on-acceptance": "as above",
}

# NO WIDENING-EXEMPTION REGISTRY EXISTS ANY MORE, AND THAT IS THE REPAIR.
#
# Round 2 shipped `WIDENING_REFUSED_AT_PARSE`: one entry, a hand-written reason, and a proof mutation
# meant to demonstrate it. Round 2's critic found the proof was DEAD CODE -- `manifest.tsv` was
# serialised before the exemption loop appended to `manifest`, so the scoring loop never saw the row
# and `refuses at parse (proven)` had never printed in any run. Worse, with the ordering repaired the
# predicate was `rc != 0 && no FAILED check`, which ANY abort satisfies: a mutation that merely
# deleted the shape earned `proven`.
#
# The entry existed for `doc:generation-token-shape`, whose claim is now a row in the scalar table
# with an accepted and a refused column -- so it has a real widening like every other row, and needs
# no exemption. Rather than repair machinery that defends nothing, the mechanism is deleted. An
# unexercised mechanism is a claim about coverage that nothing tests, which is this item's subject.

if problems:
    print("the mutation catalogue could not be built — a FIXTURE failure, not a detection:", file=sys.stderr)
    for p in problems:
        print("  " + p, file=sys.stderr)
    sys.exit(1)

with open(os.path.join(outdir, "manifest.tsv"), "w", encoding="utf-8") as fh:
    for check_id, direction, name, path in manifest:
        fh.write("%s\t%s\t%s\t%s\n" % (check_id, direction, name, path))
with open(os.path.join(outdir, "no-narrowing.tsv"), "w", encoding="utf-8") as fh:
    for check_id, reason in sorted(NO_NARROWING.items()):
        fh.write("%s\t%s\n" % (check_id, reason))

print("  %d mutations (%d derived from the document, %d exact) — %d widening, %d narrowing"
      % (len(manifest), len(manifest) - len(exact), len(exact),
         sum(1 for m in manifest if m[1] == WIDEN), sum(1 for m in manifest if m[1] == NARROW)))
PY

# The mutants are independent, so run them concurrently. Parallelism is bounded rather than `nproc`:
# each run starts a `dotnet fsi` host, and the memory ceiling binds well before the core count does.
jobs=${SIR_COHERENCE_JOBS:-6}
find "$mutants" -name '*.md' -print0 | xargs -0 -P "$jobs" -I{} bash -c '
  out=$("$0" --inner "$1" 2>&1) && rc=0 || rc=$?
  printf "%s\n" "$out" | sed -n "s/^  FAILED  \(.*\)$/\1/p" > "$1.failed"
  printf "%s" "$rc" > "$1.rc"
' "$repo_root/scripts/test-review-contract-coherence.sh" {}

status=0
narrowed="$tmp/narrowed.txt"; : > "$narrowed"
widened="$tmp/widened.txt"; : > "$widened"
while IFS=$'\t' read -r check_id direction name path; do
  reddened=$(cat "$path.failed" 2>/dev/null || true)
  rc=$(cat "$path.rc" 2>/dev/null || echo 0)
  if [[ -z "$reddened" ]]; then
    printf '  NO DETECTION  %-6s %-52s  the run reddened no check at all — a parse abort is not a detection\n' \
      "$direction" "$name" >&2
    status=1
  elif ! printf '%s\n' "$reddened" | grep -qxF "$check_id"; then
    printf '  WRONG CHECK   %-6s %-52s  expected %s; reddened %s\n' \
      "$direction" "$name" "$check_id" "$(printf '%s' "$reddened" | tr '\n' ' ')" >&2
    status=1
  else
    printf '  reds %-6s %-40s  %s\n' "$direction" "$check_id" "$name"
    printf '%s\n' "$check_id" >> "$tmp/${direction}ed.txt"
  fi
done < "$mutants/manifest.tsv"

# COVERAGE. Every derived check the clean run emitted must have been reddened by something above.
# This is the property whose absence produced four consecutive recurrences: a claim could be added to
# the document, given a check, and never given an inversion, and nothing anywhere noticed.
no_narrow=(); no_widen=()
while read -r id; do
  grep -qxF "$id" "$narrowed" \
    || grep -qP "^\\Q$id\\E\\t" "$mutants/no-narrowing.tsv" \
    || no_narrow+=("$id")
  # No widening-exemption registry exists any more: every derived check must carry a real widening.
  grep -qxF "$id" "$widened" || no_widen+=("$id")
done < <(sed -n 's/^  ok      \(doc:.*\)$/\1/p' "$tmp/clean.txt")
if (( ${#no_narrow[@]} )); then
  echo "  these DERIVED checks have no attributed NARROWING inversion:" >&2
  printf '      %s\n' "${no_narrow[@]}" >&2
  status=1
fi
# DIRECTION IS THE POINT. A check covered only by a replacement has been shown to notice the class
# three ordinary rounds already caught, and nothing about the class that survived all of them.
if (( ${#no_widen[@]} )); then
  echo "  these DERIVED checks have no attributed WIDENING inversion and no declared reason." >&2
  echo "  A widening keeps the claim's text and adds a permitted-but-wrong alternative; it is the" >&2
  echo "  class that survived three ordinary rounds and the first repair-phase attempt. Add one." >&2
  echo "  There is NO widening-exemption registry: the one that existed was deleted deliberately," >&2
  echo "  because each exemption it held stood in for a widening nobody had tried to build. A check" >&2
  echo "  that genuinely cannot carry one has a claim too weak to keep:" >&2
  printf '      %s\n' "${no_widen[@]}" >&2
  status=1
fi
# AN EXEMPTION MAY NEVER COVER BOTH DIRECTIONS. A check excused from one must demonstrably carry the
# other, or it has no inversion evidence at all while appearing twice-explained.
both=()
while IFS=$'\t' read -r id _; do
  [[ -n "$id" ]] && { grep -qxF "$id" "$widened" || both+=("$id (excused narrowing, and no widening)"); }
done < "$mutants/no-narrowing.tsv"
if (( ${#both[@]} )); then
  echo "  these checks are excused one direction and have no evidence for the other:" >&2
  printf '      %s\n' "${both[@]}" >&2
  status=1
fi

if (( ${#no_narrow[@]} == 0 && ${#no_widen[@]} == 0 && ${#both[@]} == 0 )); then
  echo "  ok      every derived check is reddened by a named NARROWING and a named WIDENING mutation,"
  echo "          or carries a declared reason why no widening can be constructed:"
  while IFS=$'\t' read -r id reason; do
    # ONE registry remains, and its guarantee is named exactly: a no-narrowing entry is authored
    # prose, cross-checked only in that the check demonstrably carries a widening. The widening
    # registry is gone entirely -- see the generator's note on why an unexercised exemption
    # mechanism is worse than none.
    [[ -n "$id" ]] && printf '            %-44s no narrowing — reason authored, cross-checked only in that this check carries a widening: %s\n' "$id" "$reason"
  done < "$mutants/no-narrowing.tsv"
fi

[[ $status -eq 0 ]] || { echo "at least one documented claim is not bound to the engine." >&2; exit 1; }

# ---------------------------------------------------------------------------------------------------
# STEP 4 — literal-only claims.
#
# Prose with no machine form: a trap stated in a sentence, a quoted refusal string, a warning. The only
# available check is presence. The document names these as literal-only so nobody reads STEP 3's
# coverage as extending to them — D6 in the round-0 review (trap 2 inverted in prose) is covered HERE,
# by literal, and is deliberately not claimed as a derived check.
# ---------------------------------------------------------------------------------------------------

literals=(
  'fsgg.coord.review-wait/v1'
  'fsgg.coord.review-decision/v2'
  'fsgg:review-decision/v2'
  'the same string, exactly'
  'every record in one review generation must bind the same critic'
  'acceptance records must carry verdict accepted'
  "live claim marker's GitHub comment id"
  'their **values are discarded**'
  'cannot discover the facts the production route supplies to it'
  'not-even-a-ref'
  'production-only'
  'review wait'
  'review record'
)
echo "literal-only claims:"
missing=0
for lit in "${literals[@]}"; do
  grep -qF -- "$lit" "$doc" || { echo "    missing: $lit" >&2; missing=1; }
done
[[ $missing -eq 0 ]] || { echo "the document no longer states a claim this gate defends." >&2; exit 1; }
echo "  ok      all ${#literals[@]} literal-only claims present"
#
# NO INVERSION IS CLAIMED HERE, AND THAT IS THE POINT.
#
# An earlier revision "proved" these reds-when-inverted with a loop that deleted every line containing
# a literal and then checked the literal was gone. That exercises grep, not this gate: the deletion and
# the assertion are the same operation, so it could never fail. A tautology dressed as evidence -- the
# precise failure this gate exists to catch, committed by the gate itself.
#
# The real limit is that these claims have no machine form. Rewriting a trap's sentence to say the
# opposite while keeping its key phrase would NOT be caught, and no amount of substring checking will
# change that. Presence is what can be checked, so presence is what is claimed.

echo
echo "review-contract coherence passed:"
echo "  - every DERIVED claim is parsed out of docs/coordination-engine-contracts.md and compared"
echo "    against the pinned engine. No expectation is presence-shaped: every vocabulary row states"
echo "    both columns of its extension, and the free-form branch does not exist."
echo "  - every derived check is reddened by a NAMED mutation, and a mutant that reddens no check is"
echo "    reported as NO DETECTION rather than counted as one. A parse abort is not evidence."
echo "  - every derived check carries BOTH directions: a narrowing (replace or delete) and a WIDENING"
echo "    (keep the claim's text, add a permitted-but-wrong alternative). A check that can have only"
echo "    one is listed above with the reason, never silently counted as covered. Widening is the"
echo "    class that survived three ordinary rounds and this phase's first attempt, so a check shown"
echo "    to catch only replacements is not shown to work."
echo "  - no scalar value is stated inside a MARKED region. Every value lives in a table cell with"
echo "    an accepted and a refused column; each scalar row has exactly one \`scalar-governed\`"
echo "    region, every such region is compared to its transcription for EQUALITY, and a second"
echo "    region repeating an id is refused — so a value cannot be restated inside a marked region"
echo "    in any phrasing, nor smuggled in beside one."
echo "  - AND THAT IS ALL THIS CHECKS. Prose OUTSIDE every marked region is not examined: a value"
echo "    restated one line after a closing marker, or anywhere else on the page, passes this gate."
echo "    The marking is authored, so the rule cannot be more total than the marking. Three earlier"
echo "    versions of this footer claimed the document-wide property instead, and each was false on"
echo "    the run that printed it — the third while the page itself restated a cardinality in"
echo "    unmarked prose. This clause states the boundary so there is no fourth: what is outside a"
echo "    marked region is reviewed by people, and this gate does not stand behind it."
echo "  - the proof of each clause above is the widening mutations, not this sentence. A footer that"
echo "    records its own past overclaims is not a check; the mutations are the check."
echo "  - every LITERAL-ONLY claim is present, and no inversion is claimed over them."
