#!/usr/bin/env bash
# scripts/test-compare-coord-engine-versions.sh — the assertion suite for the argument parser in
# `scripts/compare-coord-engine-versions.sh` (S.I.R.#303).
#
# WHAT WENT WRONG, MEASURED. Every value-taking option in that script had its own case arm ending in
# `shift 2`. Put such an option LAST on the command line and there is no `$2`: `shift 2` fails because
# the shift count exceeds `$#`, the script sets `-uo pipefail` but deliberately not `-e`, so the failure
# is discarded, nothing is shifted, `$1` is still the same option, and the `while` loop re-processes it
# forever. At d500145, before the repair, each of `--old`, `--new`, `--ref`, `--pr` and `--repo` alone
# ran until `timeout 5` killed it: exit 124, five out of five.
#
# WHY THIS FILE IS COMMITTED RATHER THAN RUN ONCE IN A SESSION. The acceptance criterion for #303 is a
# demonstration by inversion — restore the unguarded `shift 2` and show the check red. An inversion run
# in a scratchpad is a sentence in a message; a reviewer at the reviewed head cannot re-execute it. The
# same argument is made at length in the header of `scripts/test-agent-env.sh`, and it applies here for
# the same reason.
#
# THE PROPERTY THIS SUITE ACTUALLY DEFENDS, WHICH IS NOT "`--old` TERMINATES". The defect was never in
# `--old`; it was in an arm shape that five options shared, so a repair naming the options someone
# happened to type leaves the next one live. This suite therefore never carries its own list of
# options. It READS the option names out of the subject's parser block, and asserts the property for
# every name it finds. Add a sixth value-taking option to that block and it is exercised here without
# anyone remembering to register it; that is the difference between a suite that tests the fix and a
# suite that tests the example.
#
# Three inversions are built and required to red, and the first is the least interesting:
#
#   A  the pre-#303 parser, every arm unguarded              — the original defect
#   B  the same parser with ONLY `--old` guarded             — the ENUMERATED repair
#   C  the repaired parser plus a NEW unguarded option       — the next option, added tomorrow
#
# B is the repair a reader gets by fixing the case named in the issue and stopping. It passes for
# `--old` and hangs on the other four, and the suite is required not merely to red on it but to red on
# options OTHER than `--old` — otherwise B could be "caught" for an incidental reason and the
# anti-enumeration claim would be decoration.
#
# C is the same hazard pointed forwards, and it is what makes that claim testable instead of asserted:
# a sixth value-taking option is added with its own unguarded `shift 2` and registered nowhere in this
# file, and the suite must still catch it — on that option and on no other. A suite that only ever
# exercises the names its author typed cannot tell you it will cover the seventh.
#
# A null-mutation control runs the same probe against the unmutated subject and requires green, so a
# red on A, B or C cannot be satisfied by an unrelated breakage in the harness. That control has
# already earned its place: it caught two real defects in this file's own derivation logic during
# authoring, at which point every inversion below was "passing" for the wrong reason.
#
# STRUCTURE IS CHECKED TOO, because behaviour alone cannot see a relapse coming. The repaired parser
# routes every value-taking option through exactly ONE `shift`. If a future edit reintroduces
# per-option arms, the count rises and STEP 2 reds while the behaviour may still, for now, be correct.
#
# Usage: scripts/test-compare-coord-engine-versions.sh [repo-root]
# Exit code is the number of unexpected outcomes, so 0 is green.

set -uo pipefail

ROOT="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"
SUBJECT="$ROOT/scripts/compare-coord-engine-versions.sh"

# `timeout 5` is the figure the acceptance criterion for #303 names, so the subject is measured with it
# verbatim. The mutants are measured with 3s: the defect is an UNBOUNDED loop, so any positive bound
# separates "hung" from "terminated" equally well, and the smaller one buys the suite the second
# argument position on every mutant case instead of half of them.
TIMEOUT=5
MUTANT_TIMEOUT=3

unexpected=0
ok()   { printf 'ok    %s\n' "$*"; }
bad()  { printf 'FAIL  %s\n' "$*" >&2; unexpected=$((unexpected + 1)); }
note() { printf '      %s\n' "$*"; }

[ -f "$SUBJECT" ] || { printf 'FAIL  subject not found: %s\n' "$SUBJECT" >&2; exit 1; }

WORK=$(mktemp -d "${TMPDIR:-/tmp}/sir-test-compare-engines.XXXXXX") || exit 1
trap 'rm -rf -- "$WORK"' EXIT

# The parser block's first line. Every derivation and every mutation below is anchored on it, and each
# use asserts it resolved, so a restructured subject reds here by name instead of silently reducing
# this suite to a set of vacuous checks.
PARSER_ANCHOR='OLD="" NEW="" REF="" PR="" REPO="S.I.R."'

# ---------------------------------------------------------------------------------------------------
# STEP 0 — locate the parser block in an arbitrary copy of the subject.
# Emits "<start> <end>" (1-based, inclusive) or fails.
# ---------------------------------------------------------------------------------------------------
locate_parser() {  # locate_parser <script>
  local s=$1 start end hits
  hits=$(grep -Fxc "$PARSER_ANCHOR" "$s")
  [ "$hits" = 1 ] || return 1
  start=$(grep -Fxn "$PARSER_ANCHOR" "$s" | cut -d: -f1)
  end=$(awk -v s="$start" 'NR>s && $0=="done"{print NR; exit}' "$s")
  [ -n "$end" ] || return 1
  printf '%s %s' "$start" "$end"
}

# ---------------------------------------------------------------------------------------------------
# STEP 1 — derive the value-taking option names FROM the parser block.
#
# Every case-arm pattern in the block is collected, whether it is one combined alternation
# (`--old|--new|--ref|--pr|--repo)`) or five separate arms (`--old)`), so the same derivation works on
# the repaired subject and on both inversions. `*)` carries no leading `--` and is not collected.
# ---------------------------------------------------------------------------------------------------
derive_options() {  # derive_options <script>
  local s=$1 span start end
  span=$(locate_parser "$s") || return 1
  start=${span% *}; end=${span#* }
  sed -n "${start},${end}p" "$s" \
    | grep -oE '^[[:space:]]*--[a-z][a-z-]*(\|--[a-z][a-z-]*)*\)' \
    | sed 's/)$//; s/[[:blank:]]//g' | tr '|' '\n' | sort -u
}

# ---------------------------------------------------------------------------------------------------
# The probe. For a script and its derived options, require that EVERY option, with its value missing,
# terminates with a diagnostic naming it — both alone and last after a complete prefix of the others,
# which is the shape an operator's typo actually takes.
#
# Prints the names of the options that failed, one per line, and returns non-zero if there were any.
# ---------------------------------------------------------------------------------------------------
probe_parser() {  # probe_parser <script> <timeout>
  local s=$1 t=$2 opts opt other out code failed=""
  opts=$(derive_options "$s") || { printf 'DERIVATION-FAILED\n'; return 1; }
  [ -n "$opts" ] || { printf 'NO-OPTIONS-DERIVED\n'; return 1; }
  for opt in $opts; do
    local hit=""
    # Position 1: the option alone. Position 2: the option last, after every other option supplied
    # with a value — the same defect, but reached from a command line that looks complete.
    local -a prefix=()
    for other in $opts; do
      [ "$other" = "$opt" ] || prefix+=("$other" "x")
    done
    local -a argv_alone=("$opt")
    local -a argv_trailing=("${prefix[@]}" "$opt")
    local label
    for label in alone trailing; do
      if [ "$label" = alone ]; then
        out=$(timeout "$t" bash "$s" "${argv_alone[@]}" 2>&1); code=$?
      else
        out=$(timeout "$t" bash "$s" "${argv_trailing[@]}" 2>&1); code=$?
      fi
      # 124 is `timeout` killing a process that never terminated: the defect itself.
      if [ "$code" = 124 ] || [ "$code" = 0 ]; then hit=1; continue; fi
      case "$out" in *"$opt"*) ;; *) hit=1 ;; esac
      case "$out" in *"requires a value"*) ;; *) hit=1 ;; esac
    done
    [ -z "$hit" ] || failed="$failed$opt"$'\n'
  done
  [ -z "$failed" ] && return 0
  printf '%s' "$failed"
  return 1
}

# ---------------------------------------------------------------------------------------------------
# Build a mutant: replace the subject's parser block with a supplied one.
# ---------------------------------------------------------------------------------------------------
make_mutant() {  # make_mutant <name> <block-file>  -> path on stdout
  local name=$1 block=$2 span start end
  local out="$WORK/$name.sh"
  span=$(locate_parser "$SUBJECT") || return 1
  start=${span% *}; end=${span#* }
  { head -n $((start - 1)) "$SUBJECT"; cat "$block"; tail -n +$((end + 1)) "$SUBJECT"; } > "$out"
  bash -n "$out" 2>/dev/null || return 1
  printf '%s' "$out"
}

echo "test-compare-coord-engine-versions: subject $SUBJECT"

# ===================================================================================================
# STEP 0/1 — the subject's parser block is locatable and its option list derivable.
# ===================================================================================================
if span=$(locate_parser "$SUBJECT"); then
  ok "parser block located (lines ${span% *}-${span#* })"
else
  bad "parser block not locatable — the anchor '$PARSER_ANCHOR' is missing or not unique."
  note "Every check below derives from it, so the suite refuses to report rather than pass vacuously."
  exit $((unexpected + 1))
fi

OPTIONS=$(derive_options "$SUBJECT")
OPTION_COUNT=$(printf '%s\n' "$OPTIONS" | grep -c .)
if [ "$OPTION_COUNT" -ge 2 ]; then
  ok "derived $OPTION_COUNT value-taking options from the subject: $(echo $OPTIONS)"
else
  bad "derived only $OPTION_COUNT option(s) from the parser block; the derivation is not working."
fi

# ===================================================================================================
# STEP 2 — structural: the repaired parser routes every option through exactly one `shift`.
# ===================================================================================================
start=${span% *}; end=${span#* }
shift_count=$(sed -n "${start},${end}p" "$SUBJECT" | grep -cE '(^|[^[:alnum:]_])shift([^[:alnum:]_]|$)')
if [ "$shift_count" = 1 ]; then
  ok "the parser contains exactly one \`shift\` (per-option arms have not returned)"
else
  bad "the parser contains $shift_count \`shift\` statements, expected 1."
  note "Per-option shifts are the shape #303 removed: each one is a place the missing-value guard can"
  note "be forgotten. Route the new option through the existing guarded arm instead."
fi

# ===================================================================================================
# STEP 3 — behavioural, on the subject: every derived option, value missing, terminates and says so.
# This is also the NULL-MUTATION CONTROL for the inversions in STEP 5: if it is not green, a red
# there proves nothing.
# ===================================================================================================
if control_failures=$(probe_parser "$SUBJECT" "$TIMEOUT"); then
  ok "every derived option terminates with a diagnostic naming it, alone and in trailing position"
else
  bad "the subject itself fails the probe for: $(echo $control_failures)"
  note "This is the null-mutation control. While it is red, the inversion results below are not"
  note "evidence of anything."
fi

# The acceptance criterion for #303, verbatim, kept as its own named check so a reader can find it.
out=$(timeout "$TIMEOUT" bash "$SUBJECT" --old 2>&1); code=$?
if [ "$code" -ne 0 ] && [ "$code" -ne 124 ] && [ "${out#*--old}" != "$out" ]; then
  ok "acceptance criterion: \`timeout $TIMEOUT bash … --old\` exits $code naming the missing value"
else
  bad "acceptance criterion: \`… --old\` exited $code with: $out"
fi

# ===================================================================================================
# STEP 4 — normal invocations are unregressed.
#
# The subject's own happy path resolves engines over the network, so parsing is measured on a harness
# built by TRUNCATING the subject immediately after its parser block and printing what it assigned.
# The truncation point is derived in STEP 0, not hard-coded, so it moves with the file.
# ===================================================================================================
HARNESS="$WORK/parse-only.sh"
{ head -n "$end" "$SUBJECT"
  printf 'printf "OLD=[%%s] NEW=[%%s] REF=[%%s] PR=[%%s] REPO=[%%s]\\n" "$OLD" "$NEW" "$REF" "$PR" "$REPO"\n'
} > "$HARNESS"

expect_parse() {  # expect_parse <description> <expected> <argv...>
  local what=$1 want=$2; shift 2
  local got; got=$(timeout "$TIMEOUT" bash "$HARNESS" "$@" 2>&1)
  if [ "$got" = "$want" ]; then ok "$what"; else
    bad "$what"; note "expected: $want"; note "observed: $got"
  fi
}

expect_parse "a full valid invocation parses unchanged, --repo defaulting" \
  'OLD=[0.71.0] NEW=[0.73.1] REF=[S.I.R.#250] PR=[257] REPO=[S.I.R.]' \
  --old 0.71.0 --new 0.73.1 --ref "S.I.R.#250" --pr 257

expect_parse "an explicit --repo still overrides the default" \
  'OLD=[0.71.0] NEW=[0.73.1] REF=[S.I.R.#250] PR=[257] REPO=[Other.Repo]' \
  --old 0.71.0 --new 0.73.1 --ref "S.I.R.#250" --pr 257 --repo "Other.Repo"

# The guard tests `$#`, not whether `$2` looks like an option, so a value that happens to be spelled
# like a flag is still consumed as a value — exactly as it was before the repair.
expect_parse "an option-shaped value is still consumed as a value" \
  'OLD=[--new] NEW=[--ref] REF=[--pr] PR=[--repo] REPO=[S.I.R.]' \
  --old --new --new --ref --ref --pr --pr --repo

out=$(timeout "$TIMEOUT" bash "$SUBJECT" --write-probe 2>&1); code=$?
if [ "$code" = 2 ] && [ "${out#*unknown argument}" != "$out" ]; then
  ok "an unknown argument is still refused at exit 2"
else
  bad "unknown argument: exited $code with: $out"
fi

# An option given an explicitly EMPTY value supplied a value, so the new guard must not claim it is
# missing. It falls through to the script's pre-existing required-arguments check, as it did before.
out=$(timeout "$TIMEOUT" bash "$SUBJECT" --old "" --new 1 --ref r --pr 1 2>&1); code=$?
if [ "$code" = 2 ] && [ "${out#*usage:}" != "$out" ] && [ "${out#*requires a value}" = "$out" ]; then
  ok "an explicitly empty value reaches the usage check, not the missing-value guard"
else
  bad "explicit empty value: exited $code with: $out"
fi

# ===================================================================================================
# STEP 5 — inversions. Each restores a defective parser and is REQUIRED to red.
# ===================================================================================================

# A — the pre-#303 parser, exactly as it stood at d500145.
cat > "$WORK/block-a" <<'BLOCK'
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
BLOCK

if mutant=$(make_mutant inversion-a "$WORK/block-a"); then
  if failures=$(probe_parser "$mutant" "$MUTANT_TIMEOUT"); then
    bad "INVERSION A survived: the unguarded parser passed this suite, so the suite cannot fire."
  else
    ok "inversion A (unguarded \`shift 2\`) is caught, on: $(echo $failures)"
  fi
else
  bad "inversion A could not be built — the mutation anchor no longer applies."
fi

# B — the ENUMERATED repair: only `--old`, the option named in the issue, is guarded.
cat > "$WORK/block-b" <<'BLOCK'
OLD="" NEW="" REF="" PR="" REPO="S.I.R."
while [ $# -gt 0 ]; do
  case "$1" in
    --old) [ $# -ge 2 ] || die "option --old requires a value"; OLD="$2"; shift 2 ;;
    --new) NEW="${2:-}"; shift 2 ;;
    --ref) REF="${2:-}"; shift 2 ;;
    --pr) PR="${2:-}"; shift 2 ;;
    --repo) REPO="${2:-}"; shift 2 ;;
    *) die "unknown argument: $1" ;;
  esac
done
BLOCK

if mutant=$(make_mutant inversion-b "$WORK/block-b"); then
  if failures=$(probe_parser "$mutant" "$MUTANT_TIMEOUT"); then
    bad "INVERSION B survived: a repair that guards only --old passed this suite."
    note "That is the failure this suite exists to prevent — the other four options still hang."
  else
    caught=$(echo $failures)
    case " $caught " in
      *" --old "*)
        bad "inversion B was caught, but the failing set includes --old: $caught"
        note "--old IS guarded in that mutant, so a red naming it means the probe is red for some"
        note "reason other than the un-repaired options, and the anti-enumeration claim is unproven." ;;
      *)
        ok "inversion B (only --old guarded) is caught on the options it left behind: $caught" ;;
    esac
  fi
else
  bad "inversion B could not be built — the mutation anchor no longer applies."
fi

# C — the FORWARD case, and the one that makes the anti-enumeration claim testable rather than merely
# asserted. A sixth value-taking option is added the lazy way: its own arm, its own `shift 2`, no
# guard — and nobody registers it anywhere in this file. The suite must catch it anyway, because it
# reads the option list out of the parser instead of carrying one. If this ever survives, the claim in
# the subject's parser comment ("a new value-taking option ... inherits the guard") is false and the
# suite has stopped defending the property it was written for.
#
# The first line is left byte-identical to the anchor so the mutant is still locatable; `EXTRA` is
# declared on its own line.
cat > "$WORK/block-c" <<'BLOCK'
OLD="" NEW="" REF="" PR="" REPO="S.I.R."
EXTRA=""
while [ $# -gt 0 ]; do
  case "$1" in
    --old|--new|--ref|--pr|--repo)
      [ $# -ge 2 ] || die "option $1 requires a value (usage: $1 VALUE)"
      case "$1" in
        --old)  OLD="$2" ;;
        --new)  NEW="$2" ;;
        --ref)  REF="$2" ;;
        --pr)   PR="$2" ;;
        --repo) REPO="$2" ;;
      esac
      shift 2 ;;
    --extra) EXTRA="${2:-}"; shift 2 ;;
    *) die "unknown argument: $1" ;;
  esac
done
BLOCK

if mutant=$(make_mutant inversion-c "$WORK/block-c"); then
  if failures=$(probe_parser "$mutant" "$MUTANT_TIMEOUT"); then
    bad "INVERSION C survived: a newly added unguarded option was not caught."
    note "The suite is only testing the options someone remembered, which is the defect #303 fixed."
  else
    caught=$(echo $failures)
    if [ "$caught" = "--extra" ]; then
      ok "inversion C (a new unguarded option) is caught, and only it: $caught"
    else
      bad "inversion C was caught on '$caught', expected exactly '--extra'."
      note "The five guarded options must keep passing when a sixth arm is added; a wider red means"
      note "the mutation disturbed something other than the option it introduced."
    fi
  fi
else
  bad "inversion C could not be built — the mutation anchor no longer applies."
fi

echo
if [ "$unexpected" -eq 0 ]; then
  echo "test-compare-coord-engine-versions: PASS — 0 unexpected outcomes."
else
  echo "test-compare-coord-engine-versions: FAIL — $unexpected unexpected outcome(s)." >&2
fi
exit "$unexpected"
