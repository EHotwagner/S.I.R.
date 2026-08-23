#!/usr/bin/env bash
# compare-coord-engine-versions.sh — decide whether a coordination-engine version bump is safe to
# adopt in THIS workspace, by measurement rather than by reading release notes.
#
# WHY THIS EXISTS (S.I.R.#250). `.config/dotnet-tools.json` pins `fs.gg.coord.cli`, and every board
# command every concurrent lane runs goes through it. Bumping it mid-wave can strand every live lane
# at once, and "I ran some commands and they worked" cannot tell the difference between "the new
# engine agrees with the old one" and "the new engine answered differently and nobody compared".
#
# WHAT IT MEASURES. Two engine versions are run against THE SAME live board state, the same item and
# the same PR, and their JSON is byte-diffed:
#
#   review <ref> --pr N --json        the review protocol state and next action
#   delivery-route show <ref> --json  the typed delivery receipt, including its digest
#   who --repo <repo> --json          every live claim in the repo
#
# The decisive one is the review ledger. It is append-only and load-bearing for every lane's merge
# gate, so the question is not "does the new engine work" but "do an OLD-engine lane and a NEW-engine
# lane agree about the same ledger".
#
# THIS SCRIPT DOES NOT WRITE THAT ENTRY FOR YOU, AND AN EARLIER VERSION LIED ABOUT IT. It carried a
# `--write-probe` flag whose help said "the NEW engine writes a review-wait entry and BOTH engines are
# then made to read it". It wrote nothing: the flag re-ran the same read and graded it a passing
# comparison, so a green `--write-probe` was evidence of nothing and was *more* confidently worded than
# the unflagged path while checking strictly less. It only ever looked right because a newer-engine
# entry already happened to be on the PR under test. The flag is removed rather than implemented,
# because a comparison tool that mutates the board contradicts confound 3 below.
#
# To answer the ledger question, write the entry yourself through the ordinary route and then run this
# script, which reads:
#
#   scripts/fsgg-coord review wait <ref> <enter-event.json> --pr N   # through the NEW engine
#   scripts/compare-coord-engine-versions.sh --old <OLD> --new <NEW> --ref <ref> --pr N
#   scripts/fsgg-coord review wait <ref> <cancel-event.json> --pr N  # if the entry is not wanted
#
# The entry is durable and yours to cancel. Cancelling is not optional politeness: a stray `waiting`
# entry at the wrong generation blocks critic dispatch until it is cancelled.
#
# HOW IT AVOIDS LYING TO YOU. Three confounds are handled explicitly, because each one produced a
# wrong answer during S.I.R.#250 before it was handled:
#
#   1. THE LIVE BOARD MOVES UNDER YOU. A `who` diff was observed that looked like an engine
#      regression and was actually two other lanes widening their touch-sets between the two
#      captures. Captures are therefore taken BACK TO BACK, and any diff is re-measured once before
#      it is believed. A difference that does not reproduce is board drift, not an engine difference.
#   2. THE SHIM MAY NOT RUN THE ENGINE YOU THINK. `scripts/fsgg-coord` resolves four tiers in
#      priority order, and an explicit bin, a source build, or a global tool all outrank the
#      manifest. This script asserts each engine's own `--version` before trusting a single capture,
#      so a silently-preempted engine aborts rather than producing a confident identical result.
#   3. AN UNADOPTED ENGINE MUST NOT BE ADOPTED BY THE MEASUREMENT. Nothing here edits
#      `.config/dotnet-tools.json`. Both engines are reached through the shim's tier-1
#      FSGG_COORD_ENGINE_BIN override, so a failed comparison leaves the workspace exactly as it was.
#
# WHAT IT CANNOT TELL YOU, AND YOU MUST STILL READ. Command SEMANTICS can change while every output
# compared here stays byte-identical. 0.73.1 redefined `done` from "stamp the item done" to "replay a
# matching typed completion receipt; it cannot mint authority" while its flag set stayed identical to
# 0.71.0's and every comparison below stayed green. So ALSO read the release notes from the nuspec:
#
#   gh release download coherent-set/v<VER> --repo FS-GG/.github --pattern 'FS.GG.Coord.Cli.<VER>.nupkg'
#   unzip -p FS.GG.Coord.Cli.<VER>.nupkg FS.GG.Coord.Cli.nuspec | sed -n 's#.*<releaseNotes>##p'
#
# and diff the two engines' `command-contract --json` and their `--help` PROSE, not only their flags.
#
# EVIDENCE THIS SCRIPT CAN FAIL - every row below was run, and each breaks the gate's SUBJECT rather
# than its predicate. A gate that has never been red cannot be told apart from one that cannot fire.
# Rows marked (R1) were added in repair round 1, after an independent critic found that the first
# decision point carried no recorded inversion at all; the escapes they close are the ones it measured.
#
#   mutation                                            observed
#   --------------------------------------------------- -----------------------------------------------
#   positive control: 0.71.0 vs 0.72.0, valid ref/repo   PASS, exit 0 - all three surfaces evaluated
#   genuinely different engines: 0.58.0 vs 0.72.0  (R1)  DIFFERS review, FAIL, exit 1
#   non-answer: --ref S.I.R.#999999                (R1)  REFUSED review + delivery-route, FAIL, exit 1
#   non-answer: --repo NoSuchRepoAtAll             (R1)  REFUSED who, FAIL, exit 1
#   contract mutant: done and widen --paths removed      REMOVED naming both, exit 1
#   contract unreadable: { this is not json              REFUSED, not graded compatible, exit 1
#   mislabelled engine in a version directory            aborts naming what actually answered, exit 2
#   removed flag: --write-probe                    (R1)  unknown argument, exit 2
#
# The DIFFERS row is the one that matters after the R1 repair: the new exit-code guard must not swallow
# a real engine difference into a refusal. It does not - 0.58.0 and 0.72.0 both evaluate, so the byte
# comparison still fires and still reds.

# USAGE
#   scripts/compare-coord-engine-versions.sh --old 0.71.0 --new 0.73.1 --ref "S.I.R.#250" --pr 257 \
#       [--repo "S.I.R."]
#
# EXIT: 0 all compared surfaces identical; 1 a reproducible difference; 2 a setup/confound failure.

set -uo pipefail

die() { echo "compare-engines: $*" >&2; exit 2; }

OLD="" NEW="" REF="" PR="" REPO="S.I.R."
while [ $# -gt 0 ]; do
  case "$1" in
    --old) OLD="${2:-}"; shift 2 ;;
    --new) NEW="${2:-}"; shift 2 ;;
    --ref) REF="${2:-}"; shift 2 ;;
    --pr) PR="${2:-}"; shift 2 ;;
    --repo) REPO="${2:-}"; shift 2 ;;
    *) die "unknown argument: $1" ;;
  esac
done

[ -n "$OLD" ] && [ -n "$NEW" ] && [ -n "$REF" ] && [ -n "$PR" ] \
  || die "usage: --old VER --new VER --ref REF --pr N [--repo NAME]"

repo_root=$(git rev-parse --show-toplevel 2>/dev/null) || die "not inside a git checkout"
cd "$repo_root" || die "cannot enter $repo_root"

# The pinned SDK is not on PATH in agent sessions; every dotnet call fails with an error that names
# the wrong cause if this is skipped (S.I.R.#256).
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
case ":$PATH:" in *":$HOME/.dotnet:"*) ;; *) PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH" ;; esac
export PATH

work=$(mktemp -d "${TMPDIR:-/tmp}/compare-coord-engines.XXXXXX") || die "cannot create workdir"
trap 'rm -rf -- "$work"' EXIT

# Resolve one version to a runnable shim-compatible executable, preferring the restored local tool
# cache and falling back to the immutable coherent-set release asset.
resolve_engine() {
  local ver=$1 out="$work/engine-$1.sh" dll
  dll="$HOME/.nuget/packages/fs.gg.coord.cli/$ver/tools/net10.0/any/fsgg-coord-engine.dll"
  if [ ! -f "$dll" ]; then
    command -v gh >/dev/null 2>&1 || die "$ver is not in the local package cache and gh is unavailable"
    ( cd "$work" && gh release download "coherent-set/v$ver" --repo FS-GG/.github \
        --pattern "FS.GG.Coord.Cli.$ver.nupkg" --dir . --clobber >/dev/null 2>&1 ) \
      || die "cannot obtain $ver from cache or from coherent-set/v$ver"
    ( cd "$work" && unzip -o -q "FS.GG.Coord.Cli.$ver.nupkg" -d "x$ver" ) || die "cannot unpack $ver"
    dll="$work/x$ver/tools/net10.0/any/fsgg-coord-engine.dll"
    [ -f "$dll" ] || die "$ver package contains no engine at the expected path"
  fi
  printf '#!/usr/bin/env bash\nexec "%s/dotnet" "%s" "$@"\n' "$DOTNET_ROOT" "$dll" > "$out"
  chmod +x "$out" || die "cannot mark $ver wrapper executable"
  printf '%s' "$out"
}

# Confound 2: prove the shim actually resolved the engine we asked for before trusting any capture.
assert_version() {
  local bin=$1 want=$2 got
  got=$(FSGG_COORD_ENGINE_BIN="$bin" scripts/fsgg-coord --version 2>/dev/null | tr -d '[:space:]')
  case "$got" in
    "$want"|"$want".*|"$want".0) ;;
    *) die "expected engine $want but the shim answered '${got:-<nothing>}' — a higher tier preempted it" ;;
  esac
}

capture() {  # capture <engine-bin> <outfile> <argv...>
  local bin=$1 out=$2; shift 2
  FSGG_COORD_ENGINE_BIN="$bin" scripts/fsgg-coord "$@" > "$out" 2>&1
  printf '%s' "$?" > "$out.exit"
}

OLD_BIN=$(resolve_engine "$OLD") || exit 2
NEW_BIN=$(resolve_engine "$NEW") || exit 2
assert_version "$OLD_BIN" "$OLD"
assert_version "$NEW_BIN" "$NEW"

echo "compare-engines: $OLD vs $NEW on $REF / PR #$PR in $REPO"

failures=0

# Confound 1: capture back to back, and re-measure any difference once before believing it.
#
# CONFOUND 4, AND IT IS THE ONE THIS FUNCTION GOT WRONG. Equal bytes and equal exit codes prove the two
# engines said the same thing; they do NOT prove either engine EVALUATED the surface. An earlier version
# required only that the captures match each other, so two identical REFUSALS graded as agreement and the
# run still printed its headline PASS at exit 0. Measured escapes: `--ref 'S.I.R.#999999'` produced
# "identical review (exit 1)" and "identical delivery-route (exit 1)" and PASSed; `--repo 'NoSuchRepoAtAll'`
# produced "identical who (exit 1)" and PASSed. A mistyped ref, a wrong repo, an expired token, or a
# fail-closed protocol state therefore yielded a confident adoption clearance in which not one of the
# three decisive surfaces had been evaluated.
#
# `compare_command_surface` below already applies the right rule — "an unreadable contract is not evidence
# that the surface is compatible" — and this function did not. It does now: a non-zero exit on EITHER side
# is refused, never graded. `.github#266`: "I could not evaluate this" is never "I evaluated it and it
# passed".
#
# Note that `review` returns exit 4 for a fail-closed no-verdict, which is a real protocol state and a
# perfectly ordinary thing to encounter. It is still a non-evaluation, so it is refused rather than
# reported as agreement — re-run when the protocol state permits the surface to be read.
compare_surface() {
  local name=$1; shift
  local a b attempt=1
  while [ "$attempt" -le 2 ]; do
    a="$work/$name.$OLD.$attempt"; b="$work/$name.$NEW.$attempt"
    capture "$NEW_BIN" "$b" "$@"
    capture "$OLD_BIN" "$a" "$@"
    local ea eb
    ea=$(cat "$a.exit"); eb=$(cat "$b.exit")
    if [ "$ea" != 0 ] || [ "$eb" != 0 ]; then
      echo "  REFUSED    $name — $OLD exited $ea and $NEW exited $eb; a surface that did not evaluate" >&2
      echo "             is not evidence that the engines agree about it." >&2
      failures=$((failures + 1))
      return 1
    fi
    if diff -q "$a" "$b" >/dev/null 2>&1; then
      echo "  identical  $name (exit $ea)"
      return 0
    fi
    if [ "$attempt" -eq 1 ]; then
      echo "  differs    $name — re-measuring once, because the live board can move between captures"
    fi
    attempt=$((attempt + 1))
  done
  echo "  DIFFERS    $name — reproduced on a second back-to-back capture, so this is the engine" >&2
  diff "$a" "$b" >&2
  failures=$((failures + 1))
  return 1
}

compare_surface review          review "$REF" --pr "$PR" --json
compare_surface delivery-route  delivery-route show "$REF" --json
compare_surface who             who --repo "$REPO" --json

# The parser contract is compared with DIFFERENT semantics from the surfaces above, deliberately.
# A coherent-set minor is expected to ADD commands and flags, and refusing an adoption for that
# would make this script red on the happy path — the failure mode `.github#266` warns about, where
# red becomes noise. What actually breaks a consumer is a REMOVAL: a command or flag this workspace
# calls that the new engine no longer parses. So additions are reported and removals fail.
#
# Set FSGG_COMPARE_CONTRACT_OVERRIDE_NEW to a JSON file to substitute the "new" engine's contract.
# That exists so this check can be inverted — fed a contract with a command genuinely removed — and
# shown to go red. A check that has never been red cannot be distinguished from one that cannot fire.
compare_command_surface() {
  local a="$work/contract.$OLD" b="$work/contract.$NEW"
  capture "$OLD_BIN" "$a" command-contract --json
  if [ -n "${FSGG_COMPARE_CONTRACT_OVERRIDE_NEW:-}" ]; then
    [ -f "$FSGG_COMPARE_CONTRACT_OVERRIDE_NEW" ] \
      || die "contract override file not found: $FSGG_COMPARE_CONTRACT_OVERRIDE_NEW"
    cp "$FSGG_COMPARE_CONTRACT_OVERRIDE_NEW" "$b" || die "cannot read contract override"
    echo "  NOTE: using contract override for $NEW — this run is an inversion, not a measurement."
  else
    capture "$NEW_BIN" "$b" command-contract --json
  fi

  local report
  report=$(python3 - "$a" "$b" <<'PY'
import json, sys

def surface(path):
    with open(path, encoding="utf-8") as fh:
        doc = json.load(fh)
    found = {}
    def walk(node):
        if isinstance(node, dict):
            if "name" in node and "flags" in node:
                found[node["name"]] = set(node["flags"])
            for value in node.values():
                walk(value)
        elif isinstance(node, list):
            for value in node:
                walk(value)
    walk(doc)
    return found

try:
    old, new = surface(sys.argv[1]), surface(sys.argv[2])
except Exception as exc:                     # unreadable contract is a refusal, never a pass
    print("UNREADABLE %s" % exc)
    raise SystemExit(0)

removed_commands = sorted(set(old) - set(new))
added_commands = sorted(set(new) - set(old))
removed_flags = sorted(
    "%s %s" % (name, flag)
    for name in sorted(set(old) & set(new))
    for flag in sorted(old[name] - new[name])
)
added_flags = sorted(
    "%s %s" % (name, flag)
    for name in sorted(set(old) & set(new))
    for flag in sorted(new[name] - old[name])
)
print("ADDED_COMMANDS %s" % ",".join(added_commands))
print("ADDED_FLAGS %s" % ",".join(added_flags))
print("REMOVED_COMMANDS %s" % ",".join(removed_commands))
print("REMOVED_FLAGS %s" % ",".join(removed_flags))
PY
)

  case "$report" in
    UNREADABLE*)
      echo "  REFUSED    command-contract — ${report#UNREADABLE }" >&2
      echo "             an unreadable contract is not evidence that the surface is compatible." >&2
      failures=$((failures + 1))
      return 1 ;;
  esac

  local added_c added_f removed_c removed_f
  added_c=$(printf '%s\n' "$report" | sed -n 's/^ADDED_COMMANDS //p')
  added_f=$(printf '%s\n' "$report" | sed -n 's/^ADDED_FLAGS //p')
  removed_c=$(printf '%s\n' "$report" | sed -n 's/^REMOVED_COMMANDS //p')
  removed_f=$(printf '%s\n' "$report" | sed -n 's/^REMOVED_FLAGS //p')

  [ -n "$added_c" ] && echo "  added      command-contract commands: $added_c"
  [ -n "$added_f" ] && echo "  added      command-contract flags: $added_f"

  if [ -n "$removed_c" ] || [ -n "$removed_f" ]; then
    echo "  REMOVED    command-contract — $NEW no longer parses what $OLD did" >&2
    [ -n "$removed_c" ] && echo "             commands: $removed_c" >&2
    [ -n "$removed_f" ] && echo "             flags: $removed_f" >&2
    failures=$((failures + 1))
    return 1
  fi
  echo "  compatible command-contract (additive only)"
  return 0
}

compare_command_surface

if [ "$failures" -eq 0 ]; then
  echo "compare-engines: PASS — every compared surface is byte-identical across $OLD and $NEW."
  echo "compare-engines: this does NOT clear a SEMANTIC change; read the nuspec releaseNotes too."
  exit 0
fi

echo "compare-engines: FAIL — $failures surface(s) differ reproducibly. Do not adopt $NEW." >&2
exit 1
