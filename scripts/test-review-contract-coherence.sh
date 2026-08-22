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
m = re.search(r"\*\*Required\*\*.*?:\s*\n\n(.+?)\n\n", draft, re.S)
if m: out["requiredKeys"] = ticked(m.group(1))
else: problems.append("could not parse the required draft-key list")
m = re.search(r"\*\*Optional\*\*.*?:\s*\n\n(.+?)\n\n", draft, re.S)
if m: out["optionalKeys"] = ticked(m.group(1))
else: problems.append("could not parse the optional draft-key list")

vocab = {}
for row in re.finditer(r"^\|\s*`([a-zA-Z]+)`\s*\|\s*(.+?)\s*\|\s*$", section("### Vocabularies"), re.M):
    vals = ticked(row.group(2))
    if vals: vocab[row.group(1)] = vals
out["vocabularies"] = vocab
for need in ("verdict", "kind", "routeApplicability"):
    if need not in vocab: problems.append("vocabulary row missing from the document: " + need)

wait = section("## `review wait` — schema `fsgg.coord.review-wait/v1`", stop=("\n## ",))
blocks = re.findall(r"```json\n(.*?)\n```", wait, re.S)
if len(blocks) >= 2:
    try:
        out["waitEnterKeys"] = sorted(json.loads(blocks[0]).keys())
        out["waitTerminalKeys"] = sorted(json.loads(blocks[1]).keys())
    except Exception as e:
        problems.append("a wait example is not valid JSON: %s" % e)
else:
    problems.append("expected two ```json examples in the review wait section")

fields = [m.group(1) for m in re.finditer(r"^\|\s*`([a-zA-Z]+)`\s*\|", section("## The one thing to read first"), re.M)]
out["overwrittenFields"] = fields
if not fields: problems.append("could not parse the engine-overwrites table")

inv = section("### Ledger invariants")
words = {"one": 1, "two": 2, "three": 3, "four": 4, "five": 5}
m = re.search(r'requires \*\*exactly (\w+)\*\* `routeEvidence` entries', inv)
n = re.search(r'`"not-meaningful"` requires \*\*exactly (\w+)\*\*', inv)
if m and m.group(1) in words: out["meaningfulEvidenceCount"] = words[m.group(1)]
else: problems.append("could not parse the meaningful route-evidence cardinality")
if n and n.group(1) in words: out["notMeaningfulEvidenceCount"] = words[n.group(1)]
else: problems.append("could not parse the not-meaningful route-evidence cardinality")

m = re.search(r"must be at most (\d+) hours", doc)
if m: out["waitWindowMaxHours"] = int(m.group(1))
else: problems.append("could not parse the review-wait window ceiling")

m = re.search(r'an `initial` record must carry `"round": (\d+)`', inv)
if m: out["initialRound"] = int(m.group(1))
else: problems.append("could not parse the initial-record round claim")

m = re.search(r"```\n(<headSha>:<kind>:<round>)\n```", section("## The two generation fields"))
out["generationTokenShape"] = m.group(1) if m else None
if not m: problems.append("could not parse the reviewGeneration token shape")

rows = {}
for row in re.finditer(r"^\|\s*`([a-z-]+)`\s*\|\s*`(\w+)`\s*\|\s*(.+?)\s*\|\s*(.+?)\s*\|\s*$",
                       section("### Which wait entry authorizes which record"), re.M):
    rows[row.group(1)] = {"waitState": row.group(2), "token": ticked(row.group(4))}
out["authorization"] = rows
if "initial" not in rows: problems.append("authorization table has no `initial` row")

subj = section("### `subject` is NOT the item ref")
m = re.search(r"`(EHotwagner/S\.I\.R\.#255/pr/\d+)`", subj)
out["recordSubjectExample"] = m.group(1) if m else None
m2 = re.search(r"\|\s*`review wait`\s*\|\s*`item`\s*\|[^|]*\|\s*`([^`]+)`", subj)
out["waitItemExample"] = m2.group(1) if m2 else None
if not out["recordSubjectExample"] or not out["waitItemExample"]:
    problems.append("could not parse the subject/item table")

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
for field in [ "verdict"; "kind"; "routeApplicability" ] do
  let documented =
    vocab.GetProperty(field).EnumerateArray() |> Seq.map (fun e -> e.GetString()) |> List.ofSeq
  let probes =
    match field with
    | "verdict" -> [ "pass"; "changes-required"; "accepted"; "rejected"; "fail"; "Pass"; "" ]
    | "kind" -> [ "initial"; "confirmation"; "escalation"; "repair-phase"; "acceptance"
                  "repair"; "host-acceptance"; "Initial" ]
    | _ -> [ "meaningful"; "not-meaningful"; "none"; "n/a"; "unknown"; "" ]
  let engineLegal = probes |> List.filter (engineAcceptsVocab field)
  // One check, both directions: a documented value the engine refuses, or an engine value the
  // document omits, are the same kind of drift.
  check (sprintf "doc:vocabulary:%s" field) (sortedEq documented engineLegal)
    (sprintf "document lists [%s]; engine accepts [%s]" (j documented) (j engineLegal))

check "doc:wait-enter-keys"
  (sortedEq (strs "waitEnterKeys") (keysOf (ReviewWait.encode (ReviewWait.Transition.Enter receipt))))
  (sprintf "document's enter example has [%s]; the encoder emits [%s]"
     (j (strs "waitEnterKeys")) (j (keysOf (ReviewWait.encode (ReviewWait.Transition.Enter receipt)))))
check "doc:wait-terminal-keys"
  (sortedEq (strs "waitTerminalKeys")
     (keysOf (ReviewWait.encode (ReviewWait.Transition.Complete("g", entered, "e")))))
  (sprintf "document's terminal example has [%s]; the encoder emits [%s]"
     (j (strs "waitTerminalKeys"))
     (j (keysOf (ReviewWait.encode (ReviewWait.Transition.Complete("g", entered, "e"))))))

check "doc:initial-round"
  (accepts [ seal { baseRec with Round = num "initialRound" } ]
   && not (accepts [ seal { baseRec with Round = num "initialRound" + 1 } ]))
  (sprintf "document says an initial record's round is %d; the engine disagrees" (num "initialRound"))

let evidenceOf app n =
  accepts [ seal { baseRec with RouteApplicability = app
                                RouteEvidence = List.init n (fun i -> sprintf "e%d" i) } ]
let cardinalityHolds app d = evidenceOf app d && not (evidenceOf app (d - 1)) && not (evidenceOf app (d + 1))
check "doc:meaningful-evidence-cardinality" (cardinalityHolds "meaningful" (num "meaningfulEvidenceCount"))
  (sprintf "document says exactly %d entries; the engine disagrees" (num "meaningfulEvidenceCount"))
check "doc:not-meaningful-evidence-cardinality"
  (cardinalityHolds "not-meaningful" (num "notMeaningfulEvidenceCount"))
  (sprintf "document says exactly %d entries; the engine disagrees" (num "notMeaningfulEvidenceCount"))

let renderShape (shape: string) h (k: ReviewWait.Kind) r =
  let wire = match k with ReviewWait.Kind.InitialReview -> "initial-review" | _ -> "repair-confirmation"
  shape.Replace("<headSha>", h).Replace("<kind>", wire).Replace("<round>", string r)
check "doc:generation-token-shape"
  (renderShape (str "generationTokenShape") "HEAD" ReviewWait.Kind.InitialReview 0
     = ReviewWait.generationToken "HEAD" ReviewWait.Kind.InitialReview 0
   && renderShape (str "generationTokenShape") "HEAD" ReviewWait.Kind.RepairConfirmation 2
     = ReviewWait.generationToken "HEAD" ReviewWait.Kind.RepairConfirmation 2)
  (sprintf "document's shape %s does not render to the engine's token" (str "generationTokenShape"))

let auth = expected.GetProperty "authorization"
for kindName, waitKind, round in
    [ "initial", ReviewWait.Kind.InitialReview, 0
      "confirmation", ReviewWait.Kind.RepairConfirmation, 2
      "escalation", ReviewWait.Kind.RepairConfirmation, 2
      "repair-phase", ReviewWait.Kind.RepairConfirmation, 2 ] do
  let mutable row = Unchecked.defaultof<JsonElement>
  if auth.TryGetProperty(kindName, &row) then
    let toks = row.GetProperty("token").EnumerateArray() |> Seq.map (fun e -> e.GetString()) |> List.ofSeq
    let documented = match toks with t :: _ -> t | [] -> "<none>"
    let rendered = documented.Replace("<headSha>", "HEAD").Replace("<round>", string round)
    let engineToken = ReviewWait.generationToken "HEAD" waitKind round
    let documentedState = row.GetProperty("waitState").GetString()
    let engineState = if kindName = "initial" then "Waiting" else "Waiting"
    check (sprintf "doc:authorization:%s" kindName)
      (rendered = engineToken && documentedState = engineState)
      (sprintf "document renders %s in state %s; engine expects %s in state %s"
         rendered documentedState engineToken engineState)
  else
    failures <- failures + 1
    printfn "  FAILED  doc:authorization:%s\n            the document has no row for this record kind" kindName

check "doc:record-subject-form" (str "recordSubjectExample" = subject)
  (sprintf "document's record subject is %s; the CLI derives %s" (str "recordSubjectExample") subject)
check "doc:wait-item-form" (str "waitItemExample" = itemRef.Canonical)
  (sprintf "document's wait item is %s; the canonical ref is %s" (str "waitItemExample") itemRef.Canonical)
check "doc:overwritten-fields"
  (sortedEq (strs "overwrittenFields") [ "revision"; "previousDigest"; "claimGeneration"; "baseSha"; "digest" ])
  (sprintf "document's overwrite table lists [%s]" (j (strs "overwrittenFields")))

// The wait window ceiling, derived from the document and checked against the pure validator.
let validWindow (hours: float) =
  let entered = DateTimeOffset.UtcNow
  let r : ReviewWait.WaitReceipt =
    { receipt with EnteredAt = entered; ExpiresAt = entered.AddHours hours }
  match ReviewWait.validate (ReviewWait.Transition.Enter r) with Ok _ -> true | Error _ -> false
let ceiling = float (num "waitWindowMaxHours")
check "doc:wait-window-ceiling"
  (validWindow ceiling && not (validWindow (ceiling + 1.0)))
  (sprintf "document says the window is capped at %g hours; the validator disagrees" ceiling)
check "doc:wait-window-must-be-positive"
  (not (validWindow 0.0) && not (validWindow -1.0))
  "the validator no longer requires expiresAt to be later than enteredAt"

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
if ! dotnet fsi "$tmp/probe.fsx"; then
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
# STEP 3 — every DERIVED claim must red when the document is falsified.
#
# Each mutation edits a copy of the DOCUMENT, re-parses it, and re-runs the engine comparison. A
# surviving mutation means that documented claim is not bound to the engine — which was finding F1.
# D1-D7 are the critic's own round-0 mutations, which all survived the previous version.
# ---------------------------------------------------------------------------------------------------

echo "document inversion (the critic's D1-D7, plus the subject regression):"
status=0
mutate_and_expect_red() {
  local name="$1" old="$2" new="$3"
  python3 - "$doc" "$tmp/mutant.md" "$old" "$new" <<'PY'
import pathlib, sys
src, dst, old, new = sys.argv[1:5]
t = pathlib.Path(src).read_text(encoding="utf-8")
if old not in t: sys.exit("mutation anchor not present: " + old[:90])
pathlib.Path(dst).write_text(t.replace(old, new, 1), encoding="utf-8")
PY
  if "$repo_root/scripts/test-review-contract-coherence.sh" --inner "$tmp/mutant.md" >/dev/null 2>&1; then
    echo "  SURVIVED INVERSION  $name — the document can drift here undetected." >&2
    status=1
  else
    echo "  reds when inverted  $name"
  fi
}

mutate_and_expect_red "D1 required key list silently drops \`digest\`" \
  '`kind`, `round`, `timestamp`, `digest`' '`kind`, `round`, `timestamp`'
mutate_and_expect_red "D2 verdict vocabulary drops \`accepted\`" \
  '`pass`, `changes-required`, `accepted`' '`pass`, `changes-required`'
mutate_and_expect_red "D3 kind vocabulary becomes a set the engine refuses" \
  '`initial`, `confirmation`, `escalation`, `repair-phase`, `acceptance`' \
  '`initial`, `confirmation`, `escalation`, `repair`, `host-acceptance`'
mutate_and_expect_red "D4 overwrite table loses a field" \
  '| `previousDigest` |' '| `previousDigestXX` |'
mutate_and_expect_red "D5 the enter example drops a required key" \
  '  "kind": "initial-review",
' ''
mutate_and_expect_red "D7 authorization table gives \`initial\` the wrong token" \
  '| `initial` | `Waiting` | `initial-review` | `<headSha>:initial-review:0` |' \
  '| `initial` | `Waiting` | `repair-confirmation` | `<headSha>:repair-confirmation:0` |'
mutate_and_expect_red "S1 the record subject reverts to the canonical item ref" \
  'EHotwagner/S.I.R.#255/pr/259` |' 'EHotwagner/S.I.R.#255` |'
mutate_and_expect_red "S3 the wait-window ceiling is overstated" \
  'must be at most 24 hours' 'must be at most 48 hours'
mutate_and_expect_red "S2 route-evidence cardinality weakened" \
  'requires **exactly four** `routeEvidence` entries' \
  'requires **exactly five** `routeEvidence` entries'

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
for lit in "${literals[@]}"; do
  grep -vF -- "$lit" "$doc" > "$tmp/lm.md" || true
  grep -qF -- "$lit" "$tmp/lm.md" && { echo "  SURVIVED  deleting '$lit' left it present" >&2; exit 1; }
done
echo "  ok      all ${#literals[@]} literal-only claims present, each reds when deleted"

echo
echo "review-contract coherence passed: every DERIVED claim in docs/coordination-engine-contracts.md is"
echo "parsed from the document, matches the pinned engine, and reds when the document is falsified;"
echo "every LITERAL-ONLY claim is present."
