#!/usr/bin/env bash
# Holds docs/coordination-engine-contracts.md to the coordination engine it describes.
#
# WHY THIS EXISTS. S.I.R.#255: the packed review contract documented prose HTML-comment markers while
# the engine enforced `fsgg.coord.review-decision/v2` written through `review wait` / `review record`,
# and nothing measured the gap. Every PR in the workspace stalled at `landable`. Prose that describes a
# machine contract rots silently unless something reds when it drifts, so this script asserts the
# documented contract against the engine's OWN encoders and validator — not against a transcript of
# them — and then inverts every load-bearing claim to prove each assertion can fail.
#
# NO BOARD IO. Every check is pure: it loads FS.GG.Coord.Core.dll and calls it, and runs `facts`, which
# the engine documents as touching no board and no network. This script never claims, posts, or merges,
# so it is safe in CI and safe to run repeatedly.
#
# TWO HALVES, AND BOTH MUST HOLD.
#   1. ENGINE CONFORMANCE — each claim's expected value is compared against the live engine.
#   2. DOC BINDING        — the document must literally still state each claim. An assertion that
#                           passes while the prose it defends has been deleted defends nothing.
#
# INVERSION. A gate that cannot fail is not a gate. Every claim is re-run with its expectation mutated
# and MUST red; every doc literal is deleted from a scratch copy and MUST red. A surviving inversion
# fails this script, because it means the corresponding assertion is vacuous.

set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
doc="$repo_root/docs/coordination-engine-contracts.md"
tmp=$(mktemp -d)
trap 'rm -rf -- "$tmp"' EXIT

[[ -f "$doc" ]] || { echo "missing the document under test: $doc" >&2; exit 2; }

# RESOLVE THE SDK global.json PINS, AND DO NOT TRUST AN INHERITED DOTNET_ROOT.
#
# `global.json` pins an exact SDK with `rollForward: disable`. On this workspace the ambient
# environment exports DOTNET_ROOT=/usr/share/dotnet, which carries OTHER SDKs but not the pinned one,
# while the pinned SDK is installed under ~/.dotnet. Honouring the inherited value therefore fails
# with an SDK-resolution error that never names the real cause — so pick the root that actually has
# the pinned SDK rather than defaulting to whatever was exported.
pinned_sdk=$(python3 - "$repo_root/global.json" <<'PY'
import json, sys
print(json.load(open(sys.argv[1]))["sdk"]["version"])
PY
)
for candidate in "$HOME/.dotnet" "${DOTNET_ROOT:-}" /usr/share/dotnet; do
  [[ -n "$candidate" && -d "$candidate/sdk/$pinned_sdk" ]] || continue
  export DOTNET_ROOT="$candidate"
  export DOTNET_HOST_PATH="$candidate/dotnet"
  export PATH="$candidate:$candidate/tools:$PATH"
  break
done
if [[ ! -d "${DOTNET_ROOT:-}/sdk/$pinned_sdk" ]]; then
  echo "global.json pins SDK $pinned_sdk, which is not installed under any known dotnet root." >&2
  exit 2
fi

# THE ENGINE UNDER TEST IS THE PINNED ONE, read from the manifest rather than hardcoded — a version
# written here would rot exactly the way the contract did, which is the defect this script exists for.
pinned=$(python3 - "$repo_root/.config/dotnet-tools.json" <<'PY'
import json, sys
print(json.load(open(sys.argv[1]))["tools"]["fs.gg.coord.cli"]["version"])
PY
)
core="$HOME/.nuget/packages/fs.gg.coord.cli/$pinned/tools/net10.0/any/FS.GG.Coord.Core.dll"
[[ -f "$core" ]] || {
  echo "the pinned engine ($pinned) is not restored: $core" >&2
  echo "run: dotnet tool restore" >&2
  exit 2
}

# ---------------------------------------------------------------------------------------------------
# Half 1 — engine conformance, with every claim inverted.
# ---------------------------------------------------------------------------------------------------

cat > "$tmp/probe.fsx" <<FSX
#r "$core"
open System
open FS.GG.Coord
open FS.GG.Coord.StructuredDecision

let subject = "EHotwagner/S.I.R.#255"
let head = "1111111111111111111111111111111111111111"
let entered = DateTimeOffset.Parse("2026-08-22T21:40:00Z").ToUniversalTime()

let keysOf (encoded: string) =
  // encode() prefixes the marker line; the JSON is the remainder.
  let json = encoded.Substring(encoded.IndexOf '{')
  use d = Text.Json.JsonDocument.Parse json
  d.RootElement.EnumerateObject() |> Seq.map (fun p -> p.Name) |> Seq.sort |> String.concat ","

let receipt : ReviewWait.WaitReceipt =
  { Item = subject; ClaimGeneration = "5382700300"
    ReviewGeneration = ReviewWait.generationToken head ReviewWait.Kind.InitialReview 0
    Kind = ReviewWait.Kind.InitialReview
    EnteredAt = entered; ExpiresAt = entered.AddMinutes 120.0
    EvidenceRef = "https://example/pr" }

let seal (r: ReviewRecord) = { r with Digest = StructuredDecision.reviewDigest r }

let initialRec : ReviewRecord =
  { Schema = "fsgg.coord.review-decision/v2"; Subject = subject; Revision = 1
    PreviousDigest = None; HeadSha = head; ClaimGeneration = None; BaseSha = None
    Critic = "critic-abcd"; Verdict = ReviewVerdict.Pass; AcceptedExceptions = []
    RouteApplicability = "not-meaningful"; RouteEvidence = [ "documentation-only change" ]
    PolicyVersion = "structured-decisions/1"; Kind = ReviewKind.Initial; Round = 0
    InitialReview = None; PrecedingReview = None; DiffAuditRequired = false
    DiffAuditReceipts = []; Succession = None; Timestamp = "2026-08-22T21:40:00Z"; Digest = "" }

let initial = seal initialRec
let accepts recs = match StructuredDecision.validateReviewLedger subject recs with Ok _ -> "accepted" | Error _ -> "refused"

let acceptance =
  seal { initialRec with
           Revision = 2; PreviousDigest = Some initial.Digest; Kind = ReviewKind.Acceptance
           Verdict = ReviewVerdict.Accepted; ClaimGeneration = Some "5382700300"; BaseSha = Some head
           InitialReview = Some initial.Digest; PrecedingReview = Some "https://example/critic-comment" }

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

let partition () =
  let required, optional =
    draftKeys
    |> List.partition (fun (k, _) ->
        match Driver.decodeStructuredReview (render (draftKeys |> List.filter (fun (n, _) -> n <> k))) with
        | Ok _ -> false
        | Error _ -> true)
  (required |> List.map fst |> String.concat ","), (optional |> List.map fst |> String.concat ",")

let evidenceShape () =
  let at app n =
    let xs = List.init n (fun i -> sprintf "e%d" i)
    accepts [ seal { initialRec with RouteApplicability = app; RouteEvidence = xs } ]
  sprintf "meaningful:%s/%s/%s not-meaningful:%s/%s/%s"
    (at "meaningful" 3) (at "meaningful" 4) (at "meaningful" 5)
    (at "not-meaningful" 0) (at "not-meaningful" 1) (at "not-meaningful" 2)

// (id, what the engine says, what the document claims, the PLAUSIBLE DRIFT that must red)
//
// The mutant is written per claim on purpose. Inverting to an obviously impossible value would prove
// only that the comparison runs; each mutant below is a drift that could really happen — a key quietly
// added or dropped from a documented set, a round number that moved, a refusal that became permissive —
// so a surviving mutant means the document could rot in that exact way undetected.
let claims : (string * (unit -> string) * string * string) list =
  [ "wait-enter-keys",
    (fun () -> keysOf (ReviewWait.encode (ReviewWait.Transition.Enter receipt))),
    "claimGeneration,enteredAt,event,evidenceRef,expiresAt,item,kind,reviewGeneration,schema",
    // drift: the document forgets that a terminal event's `at` is NOT on an enter
    "at,claimGeneration,enteredAt,event,evidenceRef,expiresAt,item,kind,reviewGeneration,schema"

    "wait-terminal-keys",
    (fun () -> keysOf (ReviewWait.encode (ReviewWait.Transition.Complete("g", entered, "e")))),
    "at,event,evidenceRef,reviewGeneration,schema",
    // drift: the document carries `item` over from the enter event, which a terminal event has no room for
    "at,event,evidenceRef,item,reviewGeneration,schema"

    "generation-token-is-literal",
    (fun () -> ReviewWait.generationToken "HEAD" ReviewWait.Kind.InitialReview 0 + " " +
               ReviewWait.generationToken "HEAD" ReviewWait.Kind.RepairConfirmation 2),
    "HEAD:initial-review:0 HEAD:repair-confirmation:2",
    // drift: the document says the initial round is 1, the single most likely off-by-one here
    "HEAD:initial-review:1 HEAD:repair-confirmation:2"

    "draft-required-keys",
    (fun () -> fst (partition ())),
    "schema,subject,revision,headSha,critic,verdict,acceptedExceptions,routeApplicability,routeEvidence,policyVersion,kind,round,timestamp,digest",
    // drift: the document drops `digest` from the required set, believing the engine supplies it —
    // which it does, but the PARSER still refuses a draft that omits the key. This is the exact
    // half-truth that makes "the engine derives it" dangerous to state without qualification.
    "schema,subject,revision,headSha,critic,verdict,acceptedExceptions,routeApplicability,routeEvidence,policyVersion,kind,round,timestamp"

    "draft-optional-keys",
    (fun () -> snd (partition ())),
    "previousDigest,claimGeneration,baseSha,initialReview,precedingReview,diffAuditRequired,diffAuditReceipts,succession",
    // drift: the document promotes `previousDigest` to required
    "claimGeneration,baseSha,initialReview,precedingReview,diffAuditRequired,diffAuditReceipts,succession"

    "digest-ignores-its-own-field",
    (fun () ->
       let a = StructuredDecision.reviewDigest { initialRec with Digest = "" }
       let b = StructuredDecision.reviewDigest { initialRec with Digest = "deadbeef" }
       if a = b then "ignored" else "not-ignored"),
    "ignored",
    "not-ignored"

    "tampered-digest-refused",
    (fun () -> accepts [ { initial with Digest = "deadbeef" } ]),
    "refused", "accepted"

    "initial-round-must-be-zero",
    (fun () -> accepts [ seal { initialRec with Round = 1 } ]),
    "refused", "accepted"

    "acceptance-must-reuse-critic",
    (fun () -> accepts [ initial; seal { acceptance with Critic = "heron-1413" } ]),
    "refused", "accepted"

    "acceptance-must-be-verdict-accepted",
    (fun () -> accepts [ initial; seal { acceptance with Verdict = ReviewVerdict.Pass } ]),
    "refused", "accepted"

    "gen-and-base-not-on-initial",
    (fun () -> accepts [ seal { initialRec with ClaimGeneration = Some "1"; BaseSha = Some head } ]),
    "refused", "accepted"

    "route-evidence-is-cardinality",
    evidenceShape,
    "meaningful:refused/accepted/refused not-meaningful:refused/accepted/refused",
    // drift: the document reads the refusal message literally and believes four is a MINIMUM,
    // so five descriptive entries would also pass. They do not.
    "meaningful:refused/accepted/accepted not-meaningful:refused/accepted/refused"

    "documented-chain-is-accepted",
    (fun () -> accepts [ initial; acceptance ]),
    "accepted", "refused" ]

// Baseline: the engine must agree with the document.
let mutable failures = 0
for (id, actual, expected, _) in claims do
  let got = actual ()
  if got = expected then printfn "  ok      %s" id
  else
    failures <- failures + 1
    printfn "  FAILED  %s" id
    printfn "            document expects: %s" expected
    printfn "            engine says:      %s" got

// Inversion: each claim re-run against its plausible drift MUST fail. An assertion that passes against
// a value the engine does not produce is vacuous, and a vacuous assertion is the bug.
printfn ""
for (id, actual, expected, mutant) in claims do
  if mutant = expected then
    failures <- failures + 1
    printfn "  INVALID MUTANT      %s (mutant equals the expectation, so it tests nothing)" id
  elif actual () = mutant then
    failures <- failures + 1
    printfn "  SURVIVED INVERSION  %s (assertion cannot fail)" id
  else printfn "  reds when inverted  %s" id

exit (if failures = 0 then 0 else 1)
FSX

echo "engine conformance (pinned fs.gg.coord.cli $pinned):"
if ! dotnet fsi "$tmp/probe.fsx"; then
  echo "docs/coordination-engine-contracts.md disagrees with the engine it documents." >&2
  exit 1
fi

# `facts` is the engine describing its own protocol; the document must not contradict it.
facts_schema=$("$repo_root/scripts/fsgg-coord" facts --json | python3 -c 'import json,sys; print(json.load(sys.stdin)["reviewPolicy"]["schema"])')
if ! grep -qF "$facts_schema" "$doc"; then
  echo "the engine's facts report reviewPolicy.schema=$facts_schema, which the document never names." >&2
  exit 1
fi
echo "  ok      facts-reviewPolicy-schema ($facts_schema)"

# ---------------------------------------------------------------------------------------------------
# Half 2 — doc binding, with every literal inverted.
# ---------------------------------------------------------------------------------------------------
#
# Each entry is a literal the document must still contain. These are the sentences the engine-side
# assertions above defend; if one is deleted, the assertion above still passes while the guidance an
# agent actually reads has lost the claim. That is precisely how #255 happened.

literals=(
  'fsgg.coord.review-wait/v1'
  'fsgg.coord.review-decision/v2'
  '<headSha>:<kind>:<round>'
  'every record in one review generation must bind the same critic'
  'initial review round must be zero'
  'acceptance records must carry verdict accepted'
  'live claim marker'"'"'s GitHub comment id'
  'exactly four'
  'exactly one'
  'their **values are discarded**'
  'review wait'
  'review record'
)

echo "doc binding:"
check_literals() {
  local target="$1" missing=0
  for lit in "${literals[@]}"; do
    grep -qF -- "$lit" "$target" || { missing=1; [[ "${2:-}" == "report" ]] && echo "    missing literal: $lit" >&2; }
  done
  return $missing
}

if ! check_literals "$doc" report; then
  echo "docs/coordination-engine-contracts.md no longer states a claim this script defends." >&2
  exit 1
fi
echo "  ok      all ${#literals[@]} documented claims present"

for lit in "${literals[@]}"; do
  cp "$doc" "$tmp/mutant.md"
  grep -vF -- "$lit" "$doc" > "$tmp/mutant.md" || true
  if check_literals "$tmp/mutant.md"; then
    echo "  SURVIVED INVERSION  deleting '$lit' left the doc-binding check green." >&2
    exit 1
  fi
done
echo "  ok      every documented claim reds when deleted"

echo
echo "review-contract coherence passed: docs/coordination-engine-contracts.md matches the pinned engine, and every claim it makes reds when inverted."
