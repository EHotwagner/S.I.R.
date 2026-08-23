#!/usr/bin/env bash
# scripts/test-agent-env.sh — the assertion suite for `scripts/agent-env.sh` (S.I.R.#256).
#
# WHY THIS FILE IS COMMITTED, WHICH IS THE WHOLE POINT. The first version of this suite lived in a
# scratchpad directory. It really ran and it really passed, and none of that was reproducible by a
# reviewer or by anyone after the session that ran it ended — "28 checks, 0 unexpected outcomes" was
# a number in a message, not evidence. `scripts/agent-env.sh` is sourced by every non-interactive
# bash in an agent session in this workspace, so it is the artifact in this repository with the
# widest blast radius, and it shipped with the least reproducible evidence. Evidence that cannot be
# re-executed at the reviewed head is an assertion.
#
# EVERY CHECK RUNS IN A REPRODUCTION OF A FRESH SESSION, NEVER IN THE CALLER'S SHELL. `env -i` drops
# the caller's variables and `bash --noprofile --norc` stops `~/.bashrc` and `~/.bash_profile` —
# which on the reference machine BOTH export the very repair under test — from smuggling it back in.
# A check that lets a profile load passes for the wrong reason.
#
# Usage: scripts/test-agent-env.sh [repo-root]
# Exit code is the number of unexpected outcomes, so 0 is green. Refusal to run at all exits 99.
#
# ============================================================================================
# INVERSION INVENTORY — WHICH MUTATION REDS WHICH CHECK (S.I.R.#277)
# ============================================================================================
# Every gate this suite adds shipped with a mutation that reds it, and each mutation was run and
# the redding check identified. That claim used to live only in commit messages and review
# transcripts. It is committed here because a claim about the repository that the repository does
# not contain is exactly the defect this suite exists to catch — the same finding, one frame up,
# recurring inside the section that celebrates catching it.
#
# Re-derive any row by applying the mutation to a THROWAWAY COPY (`git worktree add --detach`),
# running the suite from inside it, and reading which labels go WRONG. Do not mutate a live tree:
# the suite moves the real tracked shim, and section J is what stops two runs colliding.
#
#   mutation applied to the subject                                    reds
#   ---------------------------------------------------------------  --------------------
#   delete `export DOTNET_ROOT="$candidate"` from the shim             section I check 1
#   lock_holder_state: treat any existing lock as `dead`               J1, J2
#   lock_holder_state: treat every lock as `live` (no reclaim)         J3
#   delete the startup shim-missing guard                              J3, J4, J8
#   lock_holder_state: classify unreadable pid as `dead` (case arm)    J5, J6
#   lock_holder_state: classify ABSENT pid as `dead` (cat arm)         J7
#   lock_older_than_window: always false (staleness disabled)          J8
#   trap 'on_signal TERM' TERM  ->  trap cleanup TERM  (non-exiting)   J9
#   trap 'on_signal INT'  INT   ->  trap cleanup INT   (non-exiting)   J11
#   cleanup: remove the shim-restore branch                            J10
#   cleanup: remove the lock-release branch                            J9, J10, J11
#   J10's own `saw` precondition inverted (`-eq 0`)                    J10
#
# TWO ENTRIES ARE DELIBERATELY NOT "DELETE THE TRAP". Removing `trap 'on_signal INT' INT` outright
# leaves every check GREEN and is the wrong mutation: the default disposition is what `on_signal`
# re-raises to, so deleting the trap produces the correct behaviour by accident. The mutation that
# proves J9/J11 is the NON-EXITING handler, which is the bug that actually occurred.
#
# ANCHOR YOUR MUTATION ON THE CODE, NOT ON THIS TABLE. The rows below quote the very lines they
# describe, so a naive search-and-replace now matches twice — once in the code and once here. That
# was hit on the first re-derivation after this table was written. Match on a unique code
# neighbourhood (e.g. the `trap` line together with the `HUP` line that follows it).
#
# PROVENANCE, because an inventory is itself a claim. J1, J2, J3, J4, J5, J6, J8 and J11 were
# re-derived by an independent reviewer using mutations it chose without reading this table. J7,
# J9, J10 and the section I row were re-derived by the author at this head. Nothing in this table
# is carried from a transcript alone.

set -uo pipefail

ROOT="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"
H="${HOME}"
PINNED="$(sed -n 's/.*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$ROOT/global.json" | head -n 1)"
SHIM="$ROOT/scripts/agent-env.sh"

# The environment a fresh agent session measurably gets in this workspace.
FRESH_PATH="$H/.local/bin:$H/.dotnet/tools:/usr/share/dotnet:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"
BASH_ENV_VALUE='$(git rev-parse --show-toplevel 2>/dev/null)/scripts/agent-env.sh'
# Reproduces `source <shell-snapshot>; <command>`, whose final line re-exports the host's own PATH
# after BASH_ENV has run. This is the behaviour the `dotnet` function exists to absorb.
CLOBBER='export PATH='"$FRESH_PATH"';'

# THIS SUITE MUTATES THE REAL TRACKED FILE, SO IT MUST NOT OVERLAP WITH ITSELF (S.I.R.#277).
# Section H moves `$SHIM` — `scripts/agent-env.sh` in the working tree, not a copy — out of the way
# to prove the wired case goes red without it. Anything else reading that file during the window
# sees it missing or truncated, so a concurrent run reds section I for a cause that has nothing to
# do with DOTNET_ROOT. That is why the suite refuses rather than emitting a result it cannot
# account for: the answer would be wrong, and it would cost the reader a run to find out.
#
# AN EARLIER VERSION OF THIS COMMENT CLAIMED THE TWO CONDITIONS ARE INDISTINGUISHABLE, AND SAID SO
# AS OBSERVED FACT. IT WAS WRONG AND IS RETRACTED. Nobody had measured it. Measured since, at
# `2e4b07e`, across three collision shapes and two timings, and reproduced independently by a second
# reviewer at a different commit:
#
#     condition                  section I assertion   its non-vacuity control
#     genuine `export` deletion   red, rc=1             GREEN
#     concurrent collision        red, rc=3             ALSO red, rc=3
#
# They are distinguishable on two independent signals. rc=3 is the deliberate build-failure code —
# a collision breaks the probe's `dotnet build`; it does not make the assertion fail — and a real
# inversion breaks only the asserted probe while a collision breaks both. THE 3/4/5 EXIT-CODE
# DISCIPLINE BELOW HAD ALREADY DONE THE JOB THE RETRACTED SENTENCE SAID WAS UNDONE. The claim
# survived three review rounds because it pattern-matched this item's own thesis, which is the
# one shape nobody re-measures. Keep the retraction here: a reader who deletes it will re-derive
# the wrong story from the lock's existence.
#
# The lock lives in the git dir, which is untracked and per-worktree, and it is taken by `mkdir`
# because that is atomic on every filesystem this runs on and needs no `flock`. An unreadable
# holder is bounded by age rather than guessed at, so a crashed run cannot wedge the suite forever.
#
# REFUSAL EXITS 99, NOT 1. This script's contract is "exit code is the number of unexpected
# outcomes", so exiting 1 would be indistinguishable from one failed check — which is the same
# category of defect all over again, one frame up.
GITDIR="$(command git -C "$ROOT" rev-parse --absolute-git-dir 2>/dev/null)" || GITDIR=""
[ -n "$GITDIR" ] || GITDIR="$ROOT"
LOCKDIR="$GITDIR/fsgg-agent-env-suite.lock"
# ONE PREDICATE ANSWERS ONE QUESTION, AND REFUSES TO ANSWER FROM INPUT IT CANNOT READ
# (S.I.R.#277 round 1, finding F1). The previous guard asked
# `kill -0 "$(cat "$LOCKDIR/pid")"` directly. On an EMPTY or NON-NUMERIC pid file that expands to
# `kill -0 ""`, which fails, which the guard read as "the holder is dead" — so it deleted a LIVE
# holder's lock and ran anyway. Measured, both shapes. And with NO pid file the `[ -f ]` test failed
# and nothing was ever reclaimable, which falsified this file's own claim two comments up that a
# crashed run cannot wedge the suite forever.
#
# Both are the same bug and neither is fixed by special-casing empty and non-numeric: a predicate
# that returns a confident answer about input it could not evaluate will have a third input shape.
# So the predicate is typed. It reports `live`, `dead`, or `unreadable`, and `unreadable` is a real
# answer — not a synonym for dead.
lock_holder_state() { # lock_holder_state <lockdir> -> live | dead | unreadable
  local pid
  pid="$(cat "$1/pid" 2>/dev/null)" || { printf 'unreadable'; return 0; }
  # Absent, empty, non-numeric, or leading-zero (which includes "0", whose kill(2) target is the
  # whole process group rather than one process) — liveness is UNKNOWN, and says so.
  case "$pid" in
    ''|*[!0-9]*|0*) printf 'unreadable'; return 0 ;;
  esac
  if kill -0 "$pid" 2>/dev/null; then printf 'live'; else printf 'dead'; fi
}

# AN UNREADABLE LOCK IS BOUNDED BY AGE, NOT GUESSED AT. It is not evidence the holder lives and not
# evidence it died. Refusing forever wedges the suite; reclaiming at once deletes a live holder's
# lock. Age is the only honest discriminator available, so only an unreadable lock older than the
# window is reclaimed, and the window is nameable by a caller that knows its own runtime.
LOCK_STALE_MINUTES="${FSGG_AGENT_ENV_LOCK_STALE_MINUTES:-30}"
lock_older_than_window() { [ -n "$(find "$1" -maxdepth 0 -mmin "+$LOCK_STALE_MINUTES" 2>/dev/null)" ]; }

# `acquired` is tracked explicitly. Testing "does the directory exist?" after a failed `mkdir`
# reports the HOLDER's lock as if it were ours and never refuses — an earlier version of this guard
# did exactly that and passed its own concurrency test. Only the process whose `mkdir` won proceeds.
# THE HANDLERS ARE INSTALLED BEFORE THE LOCK IS TAKEN, NOT AFTER (S.I.R.#277 round 1). Installing
# them afterwards leaves a window between the winning `mkdir` and the `trap`, and a signal landing in
# it kills the process under the DEFAULT disposition — orphaning the lock this suite just created, so
# the next run refuses against a holder that no longer exists. Measured directly: signalling as soon
# as the pid file appeared left `lock=KEPT`. Ordering them first shrinks that window to one variable
# assignment, which is as far as shell can close it; `LOCK_OWNED` is what makes installing them early
# safe, because cleanup will not remove a lock this process has not claimed.
#
# cleanup is written defensively because it can now run before TMP/BAK exist.
LOCK_OWNED=0
cleanup() {
  if [ -n "${BAK:-}" ] && [ -f "$BAK" ] && [ ! -f "$SHIM" ]; then
    mv "$BAK" "$SHIM"
  fi
  [ -n "${TMP:-}" ] && rm -rf "$TMP"
  [ "${LOCK_OWNED:-0}" -eq 1 ] && rm -rf "$LOCKDIR"
  return 0
}
# A SIGNAL MUST STILL KILL THIS SUITE (finding F2). `trap cleanup INT TERM` ran cleanup and then
# RESUMED, because a bash trap handler that does not exit returns to the interrupted line. The suite
# therefore survived SIGINT/SIGTERM and ran to completion — releasing its lock mid-run while section
# H still had the real tracked shim moved aside, and emitting spurious WRONGs indistinguishable from
# real failures. That is the same one-signal-two-meanings confusion exit 99 exists to prevent,
# reintroduced by the handler meant to make interruption safe. Restoring the default disposition and
# re-raising is what makes the process die with the signal's own status (130/143), not a check count.
on_signal() {
  cleanup
  trap - EXIT INT TERM HUP
  kill -s "$1" "$$"
}
trap cleanup EXIT
trap 'on_signal INT'  INT
trap 'on_signal TERM' TERM
trap 'on_signal HUP'  HUP

# ACQUISITION IS ONE HELPER SO THE THREE CALL SITES CANNOT DRIFT APART, and so the gap between
# creating the lock, claiming it, and stamping it is as short as shell allows.
#
# A RESIDUAL RACE REMAINS AND IS DOCUMENTED RATHER THAN DENIED. `mkdir` and the assignment after it
# are separate commands, and bash dispatches a pending trap between commands, so a signal can land
# with the lock created but not yet claimed or stamped. Measured: polling for the directory and
# signalling instantly reproduces it 5 times out of 5. Shell cannot close that window.
#
# What CAN be controlled is the failure mode, and it is bounded by construction: a lock abandoned in
# that window has no pid file, `lock_holder_state` classifies it `unreadable` rather than guessing,
# and the staleness window reclaims it. So the race costs a bounded delay, never a wedge and never a
# deleted live holder's lock. J7 and J8 are the checks that hold that bound.
claim_lock() {
  mkdir "$LOCKDIR" 2>/dev/null || return 1
  LOCK_OWNED=1
  printf '%s' "$$" > "$LOCKDIR/pid"
  return 0
}

acquired=0
refusal=''
if claim_lock; then acquired=1
else
  case "$(lock_holder_state "$LOCKDIR")" in
    dead)
      rm -rf "$LOCKDIR"
      if claim_lock; then acquired=1; fi
      ;;
    unreadable)
      if lock_older_than_window "$LOCKDIR"; then
        rm -rf "$LOCKDIR"
        if claim_lock; then acquired=1; fi
      else
        refusal='unreadable'
      fi
      ;;
    *) refusal='live' ;;
  esac
fi
if [ "$acquired" -ne 1 ]; then
  if [ "$refusal" = unreadable ]; then
    echo "REFUSED: $LOCKDIR exists but its holder cannot be identified."
    echo "  An unreadable pid is not evidence the holder died, so this will not delete a lock it"
    echo "  cannot account for. It is reclaimed automatically once older than"
    echo "  ${LOCK_STALE_MINUTES}m (FSGG_AGENT_ENV_LOCK_STALE_MINUTES) (S.I.R.#277)."
  else
    echo "REFUSED: another $0 run holds $LOCKDIR."
    echo "  Section H moves the real tracked scripts/agent-env.sh; a concurrent run cannot produce a"
    echo "  trustworthy result in EITHER direction, so this refuses instead of guessing (S.I.R.#277)."
  fi
  exit 99
fi
FAILURES=0
TMP="$(mktemp -d)"
BAK="$TMP/agent-env.sh.bak"

# THE SHIM MUST BE PRESENT BEFORE THE SUITE STARTS, and this check lives AFTER the traps on purpose:
# from here on every exit path runs `cleanup`, so the lock is released by one owner in one place
# rather than by each early return remembering to. (Dropping this guard during the round-1 generator
# rewrite is precisely what J3, J4 and J8 caught — the checks that assert it went red while the
# behaviour they describe had silently left the file.)
if [ ! -f "$SHIM" ]; then
  echo "REFUSED: $SHIM is missing before the suite started."
  echo "  An earlier interrupted run may have left it moved aside. Restore it with"
  echo "  'git checkout -- scripts/agent-env.sh' before re-running (S.I.R.#277)."
  exit 99
fi

fresh() { # fresh <HOME> <PATH> <DOTNET_ROOT> <BASH_ENV> <script>
  # THE `cd` MUST HAPPEN BEFORE bash STARTS, NOT INSIDE IT (S.I.R.#277). BASH_ENV's value is
  # `$(git rev-parse --show-toplevel)/scripts/agent-env.sh` — the exact form both host config files
  # set — and bash performs that command substitution AT SHELL STARTUP, before it runs `-c`. With
  # the old `bash -c "cd '$ROOT' && …"` the substitution therefore resolved against the CALLER's
  # cwd, not $ROOT. Whenever the two differ — which is exactly what the documented
  # `scripts/test-agent-env.sh [repo-root]` argument is for — every wired check silently sourced
  # the CALLER's shim and reported on an artifact that was never under test. Measured: with $ROOT's
  # shim moved aside, section H's "wired, but the shim is deleted" still resolved the pinned
  # version, because the caller's intact shim had been sourced instead.
  ( cd "$ROOT" 2>/dev/null || exit 127
    env -i HOME="$1" USER="${USER:-runner}" TERM=dumb \
        PATH="$2" DOTNET_ROOT="$3" BASH_ENV="$4" \
        DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 \
        FSGG_COORD_OWNER_TYPE=user FSGG_COORD_OWNER=EHotwagner FSGG_COORD_PROJECT="S.I.R." \
        GH_TOKEN="${GH_TOKEN:-}" GITHUB_TOKEN="${GITHUB_TOKEN:-}" \
        bash --noprofile --norc -c "$5" ) 2>&1
}

run() { # run <expect pass|fail> <label> <HOME> <PATH> <DOTNET_ROOT> <BASH_ENV> <script>
  local expect="$1" label="$2"; shift 2
  local out rc
  out="$(fresh "$1" "$2" "$3" "$4" "$5")"; rc=$?
  if { [ "$expect" = pass ] && [ "$rc" -eq 0 ]; } || { [ "$expect" = fail ] && [ "$rc" -ne 0 ]; }; then
    printf 'OK    (expected %-4s got rc=%-3s) %s\n' "$expect" "$rc" "$label"
  else
    printf 'WRONG (expected %-4s got rc=%-3s) %s\n' "$expect" "$rc" "$label"
    printf '%s\n' "$out" | sed -n '1,6p' | sed 's/^/        | /'
    FAILURES=$((FAILURES + 1))
  fi
}

section() { printf '\n==============================================================================\n%s\n==============================================================================\n' "$1"; }

[ -n "$PINNED" ] || { echo "cannot read the pinned SDK version from $ROOT/global.json"; exit 1; }
echo "pinned SDK from global.json: $PINNED"

section "A. NEGATIVE CONTROL — a fresh session WITHOUT the wiring must be red"
run fail "dotnet --version"                  "$H" "$FRESH_PATH" /usr/share/dotnet "" 'dotnet --version'
run fail "dotnet fsi"                        "$H" "$FRESH_PATH" /usr/share/dotnet "" 'dotnet fsi --help >/dev/null'
run fail "scripts/fsgg-coord resolves its engine" "$H" "$FRESH_PATH" /usr/share/dotnet "" 'scripts/fsgg-coord --version >/dev/null'
run fail "build entry point: dotnet tool restore" "$H" "$FRESH_PATH" /usr/share/dotnet "" 'dotnet tool restore >/dev/null'

section "B. WIRED — exactly the BASH_ENV value both host config files set"
run pass "dotnet --version resolves the pin"  "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'test "$(dotnet --version)" = "'"$PINNED"'"'
run pass "dotnet fsi"                         "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'dotnet fsi --help >/dev/null'
run pass "dotnet fsi executes a script"       "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'printf "printfn \"fsi-ok\"\n" > "'"$TMP"'/p.fsx" && dotnet fsi "'"$TMP"'/p.fsx" | grep -q fsi-ok'
run pass "scripts/fsgg-coord resolves its engine" "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'scripts/fsgg-coord --version >/dev/null'
run pass "build entry point: dotnet tool restore" "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'dotnet tool restore >/dev/null'
run pass "the shim writes nothing to stdout"  "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'test "$(echo marker)" = marker'
run pass "a nested bash inherits it"          "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'bash -c "dotnet --version" | grep -q "'"$PINNED"'"'
run pass "a bash SCRIPT (build.sh shape) inherits it" "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'printf "#!/usr/bin/env bash\ndotnet --version\n" > "'"$TMP"'/s.sh" && chmod +x "'"$TMP"'/s.sh" && "'"$TMP"'/s.sh" | grep -q "'"$PINNED"'"'
run pass "global.json pin is untouched"       "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'grep -q "\"version\": \"'"$PINNED"'\"" global.json && grep -q "\"rollForward\": \"disable\"" global.json'

section "C. DEGRADATION — it must not touch a machine that does not need it"
# A system root that DOES carry the pin: step 1 must change nothing at all.
FAKE="$TMP/system-dotnet"; mkdir -p "$FAKE/sdk/$PINNED"
printf '#!/usr/bin/env bash\necho %s\n' "$PINNED" > "$FAKE/dotnet"; chmod +x "$FAKE/dotnet"
run pass "step 1: pin already resolved -> DOTNET_ROOT and muxer untouched" \
    "$H" "$FAKE:$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'test "$DOTNET_ROOT" = "/usr/share/dotnet" && test "$(command -v dotnet)" = "'"$FAKE"'/dotnet" && test "$FSGG_AGENT_ENV_APPLIED" = already-resolved'
# A $HOME carrying the WRONG SDK while nothing carries the pin: change nothing.
EMPTY="$TMP/empty-home"; mkdir -p "$EMPTY/.dotnet/sdk/0.0.000"
printf '#!/usr/bin/env bash\necho 0.0.000\n' > "$EMPTY/.dotnet/dotnet"; chmod +x "$EMPTY/.dotnet/dotnet"
run pass "step 3: no candidate carries the pin -> PATH and DOTNET_ROOT byte-identical" \
    "$EMPTY" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'test "$PATH" = "'"$FRESH_PATH"'" && test "$DOTNET_ROOT" = "/usr/share/dotnet" && test -z "${FSGG_AGENT_ENV_APPLIED:-}"'
run pass "outside any git checkout: silent no-op, no stdout and no stderr" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'cd / && out="$(bash -c ":" 2>&1)" && test -z "$out"'

section "D. HOST CLOBBER — a host re-exports its own PATH after BASH_ENV has run"
run pass "clobbered shell: dotnet still resolves the pin" "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' test "$(dotnet --version)" = "'"$PINNED"'"'
run pass "clobbered shell: dotnet fsi"                    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' dotnet fsi --help >/dev/null'
run pass "clobbered shell: scripts/fsgg-coord (step 0 re-heals the child)" "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' scripts/fsgg-coord --version >/dev/null'
run pass "clobbered shell: build entry point"             "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' dotnet tool restore >/dev/null'
run fail "clobbered shell WITHOUT the wiring is still red (control)" "$H" "$FRESH_PATH" /usr/share/dotnet "" "$CLOBBER"' dotnet --version'

section "E. INVOCATION SHAPE — review finding M1: subshells discard the function's side effects"
# THESE ARE CHARACTERIZATION CHECKS, NOT REGRESSION CHECKS, AND THE DIFFERENCE IS WORTH STATING.
# M1's defect was the DOCUMENTATION — it claimed one `dotnet` call repaired the shell permanently.
# The code always behaved as the checks below now pin, so running them against the pre-review shim
# passes too; a test cannot go red against a wrong sentence. What they buy is that the sentence and
# the behaviour can no longer drift apart silently: the claim now has an executable counterpart.
# The CALL must be correct in every shape, because the function invokes the muxer by absolute path.
run pass "M1: command substitution \$(dotnet …) returns the pinned version" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' V="$(dotnet --version)"; test "$V" = "'"$PINNED"'"'
run pass "M1: pipeline dotnet … | cat returns the pinned version" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' test "$(dotnet --version | cat)" = "'"$PINNED"'"'
# The PATH repair is claimed ONLY for a non-subshell call. Assert BOTH directions, so the day the
# behaviour changes the documentation is provably wrong rather than quietly stale.
run pass "M1: a DIRECT call does repair PATH for later execvp children" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' dotnet --version >/dev/null; test "$(env | sed -n "s/^PATH=//p")" != "'"$FRESH_PATH"'"'
run pass "M1: a SUBSHELL call does NOT repair the parent PATH, exactly as documented" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' V="$(dotnet --version)"; test "$(env | sed -n "s/^PATH=//p")" = "'"$FRESH_PATH"'"'
run pass "M1: a subshell call leaves the function defined (it did not falsely self-erase)" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' V="$(dotnet --version)"; declare -F dotnet >/dev/null'

section "F. command -v — review finding M2: a function shadows the path lookup"
# Also characterization, and deliberately so: M2 is not fixable in this file — a shell function
# shadowing `command -v` is POSIX behaviour. What these pin is the BOUND, which is fixable and is
# the thing that could regress: bash-script children must keep resolving a real path, because that
# is what stops SIR_REAL_DOTNET becoming a bare word and the trace shim exec'ing itself.
run pass "M2: while defined, command -v dotnet answers the bare word (POSIX, documented)" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' test "$(command -v dotnet)" = "dotnet"'
run pass "M2: after a direct call the function is gone and command -v answers a real path" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' dotnet --version >/dev/null; case "$(command -v dotnet)" in /*) exit 0 ;; *) exit 1 ;; esac'
# The consumers that make M2 matter: three scripts set SIR_REAL_DOTNET from $(command -v dotnet),
# and scripts/dotnet-invocation-trace.sh execs it. Each is a bash script, so it gets its own
# BASH_ENV pass, returns at step 0 with PATH re-healed, and never defines the function. If that ever
# stops holding, SIR_REAL_DOTNET becomes a bare word and the trace shim re-resolves to itself.
run pass "M2: a bash-script child resolves command -v dotnet to a real path, not a bare word" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' printf "#!/usr/bin/env bash\ncommand -v dotnet\n" > "'"$TMP"'/c.sh" && chmod +x "'"$TMP"'/c.sh" && case "$("'"$TMP"'/c.sh")" in /*) exit 0 ;; *) exit 1 ;; esac'
run pass "M2: that child's resolved binary really is the pinned muxer, not a shim" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" "$CLOBBER"' printf "#!/usr/bin/env bash\nreal=\$(command -v dotnet)\n\"\$real\" --version\n" > "'"$TMP"'/r.sh" && chmod +x "'"$TMP"'/r.sh" && test "$("'"$TMP"'/r.sh")" = "'"$PINNED"'"'
run pass "the three SIR_REAL_DOTNET consumers still resolve it that way" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'grep -lq "command -v dotnet" scripts/qualify-pr.sh scripts/qualify-production.sh scripts/run-ci-gate.sh'

section "G. IDEMPOTENCE AND COST"
run pass "sourcing twice cannot double-prepend PATH" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'before="$PATH"; . scripts/agent-env.sh; . scripts/agent-env.sh; test "$PATH" = "$before"'
run pass "BASH_ENV is rewritten to a literal, so children skip the git call" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'case "$BASH_ENV" in /*agent-env.sh) exit 0 ;; *) exit 1 ;; esac'
run pass "every repo shell entry point is bash, which is what lets step 0 reach them" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'test "$(head -1 build.sh)" = "#!/usr/bin/env bash" && for f in scripts/*.sh; do [ "$f" = scripts/agent-env.sh ] && continue; test "$(head -1 "$f")" = "#!/usr/bin/env bash" || exit 1; done'

section "H. MUTATION — delete the mechanism and the wired case must go red again"
mv "$SHIM" "$BAK"
run fail "wired, but the shim is deleted: dotnet --version" "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'dotnet --version'
mv "$BAK" "$SHIM"
run pass "shim restored: dotnet --version"                  "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'dotnet --version >/dev/null'

section "I. DOTNET_ROOT — the exported root is what an APPHOST consults (S.I.R.#277)"
# WHY THIS SECTION EXISTS. Before it did, `export DOTNET_ROOT="$candidate"` could be deleted from the
# shim and the ENTIRE suite stayed green — 36 checks, 0 unexpected (measured as mutation S8 in PR
# #260 review round 1, and reproduced at base f69f1e6). At THIS head the same deletion still leaves
# all 36 checks above green and reds only this section, which is the whole point: the 36/0 result is
# a property of the base commit and does not reproduce here. That is not because the line does nothing — it is because every probe
# above reaches the SDK through the MUXER, and the muxer resolves SDKs relative to its own location
# and ignores DOTNET_ROOT for that. The file therefore asserted a purpose for that line which nothing
# here could falsify. These two checks are that falsifier.
#
# WHAT DOTNET_ROOT ACTUALLY DECIDES. A framework-dependent APPHOST does not go through the muxer: it
# reads DOTNET_ROOT to locate hostfxr, and falls back to the global install location only when that
# directory does not exist. Step 2 of the shim puts `$HOME/.dotnet/tools` on PATH, so a bare
# `fable` or `fsgg-sdd` in an agent session is such an apphost. NO committed script in this
# repository invokes one that way — they all go through the muxer (`dotnet fable`, `dotnet
# fsgg-sdd`, `dotnet tool run`) — which is why the export protects an ad-hoc path rather than a
# scripted one, and why this section must BUILD an apphost instead of reusing a caller.
# On the reference workspace the session arrives with DOTNET_ROOT=/usr/share/dotnet, which carries
# a DIFFERENT Microsoft.NETCore.App than the $HOME/.dotnet that step 2 selects, so without the export
# the muxer and every apphost load from two different installs. Measured with COREHOST_TRACE=1:
#   export removed: Chose FX version [/usr/share/dotnet/shared/Microsoft.NETCore.App/10.0.11]
#   export present: Chose FX version [$HOME/.dotnet/shared/Microsoft.NETCore.App/10.0.10]
# That divergence is what "so the muxer that PATH now resolves and the root that apphosts consult
# agree" means, and it is observable, so it is checked here rather than asserted in a comment.
#
# THE PROBE IS BUILT, NOT COMMITTED, AND IT IS A REAL APPHOST. Nothing else in this repository is
# guaranteed to be built when this suite runs, and a fake cannot demonstrate host behaviour. It is
# built by the wired session under test, so the build is also the direct `dotnet` call that retires
# the function and re-heals PATH before `command -v dotnet` is asked for a real path.
TFM="net${PINNED%%.*}.0"
mkdir -p "$TMP/apphost"
cat > "$TMP/apphost/apphost.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>$TFM</TargetFramework>
    <AssemblyName>roots</AssemblyName>
  </PropertyGroup>
</Project>
EOF
# The location of System.Private.CoreLib IS the install the host resolved the runtime from.
printf 'class R { static void Main() { System.Console.WriteLine(typeof(object).Assembly.Location); } }\n' > "$TMP/apphost/Program.cs"
PROBE="$TMP/apphost/bin/Release/$TFM/roots"
# Distinct non-1 exit codes so a setup failure is never mistaken for the assertion going red.
BUILD='dotnet build "'"$TMP"'/apphost/apphost.csproj" -c Release -v q --nologo >/dev/null 2>&1 || exit 3; real="$(command -v dotnet)"; case "$real" in /*) ;; *) exit 4 ;; esac; muxroot="$(readlink -f "$real")"; muxroot="${muxroot%/*}";'

run pass "the apphost loads its runtime from the SAME install the resolved muxer lives in" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    "$BUILD"' core="$("'"$PROBE"'")" || exit 5; case "$core" in "$muxroot"/shared/*) exit 0 ;; *) exit 1 ;; esac'
# NON-VACUITY CONTROL. If this machine carries only one usable install, the check above passes no
# matter what the shim exports and proves nothing — so require the probe to actually FOLLOW
# DOTNET_ROOT here, on this machine, at this head. Its inputs are the two roots the shim chooses
# between: the one the session arrived with and the one step 2 selected.
run pass "control: that probe really does follow DOTNET_ROOT, so the check above can fail" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    "$BUILD"' a="$(DOTNET_ROOT=/usr/share/dotnet "'"$PROBE"'")" || exit 5; b="$(DOTNET_ROOT="$muxroot" "'"$PROBE"'")" || exit 5; test "$a" != "$b" && case "$b" in "$muxroot"/shared/*) exit 0 ;; *) exit 1 ;; esac'

# THE FIXTURE COPY MUST NOT RUN THIS SECTION. The fixture is a copy of THIS script, so an
# unguarded section J would build its own fixture and invoke it, without bound. That is not
# hypothetical: it was hit while inverting J1, because disabling the lock is exactly what lets the
# nested run get past the refusal and reach its own section J. The flag is set only on the fixture
# invocations below, so a normal run always executes this section.
if [ -z "${FSGG_AGENT_ENV_SUITE_FIXTURE:-}" ]; then
section "J. THE GUARD ITSELF — the refusal and the trap must be assertable, not just asserted"
# WHY THIS SECTION EXISTS, AND IT IS THE SAME LESSON AS SECTION I (S.I.R.#277). The concurrency
# refusal and the stale-lock reclaim were verified when they were written, in a worker's transcript
# and an independent reviewer's — and NOWHERE ELSE. The commit message said "ships with evidence it
# can fail" and the repository contained no check that could fail. A guard whose evidence lives only
# in a transcript is exactly the "unfalsifiable at rest" condition section I exists to end, one frame
# up: it is unfalsifiable BY THIS SUITE, which is the only reader that outlives the session.
#
# The fixture is a throwaway git root under $TMP carrying a copy of `global.json`, the shim, and this
# script. The startup guard refuses BEFORE any dotnet work, so each check costs a process, not a
# suite run. The fixture also keeps these checks off the real repository's lock, which the outer run
# is holding right now — that is why they cannot simply re-invoke the suite in place.
FX="$TMP/lockfx"
mkdir -p "$FX/scripts"
command git init -q "$FX" >/dev/null 2>&1
cp "$ROOT/global.json" "$FX/global.json"
cp "$SHIM" "$FX/scripts/agent-env.sh"
# THE FIXTURE'S SECTION-H WINDOW IS WIDENED ON PURPOSE, IN THE COPY ONLY. J10 has to interrupt a
# run at the instant the shim is moved aside, and in an unmodified run that window is whatever one
# `run fail` happens to take. Racing it made J10 both FLAKY and unable to fail: when the poll missed
# the window it signalled an already-finished process, found the shim restored by section H's own
# `mv`, and passed — green even with the restore deleted from `cleanup`. Widening the window in the
# throwaway copy makes the interrupt land where it must, so the check tests the trap instead of the
# scheduler. The real script is untouched, and `cleanup` still comes from $ROOT, so an inversion
# there still propagates here.
sed 's|^mv "$SHIM" "$BAK"$|mv "$SHIM" "$BAK"; sleep 3|' \
    "$ROOT/scripts/test-agent-env.sh" > "$FX/scripts/test-agent-env.sh"
chmod +x "$FX/scripts/test-agent-env.sh"
FXGIT="$(command git -C "$FX" rev-parse --absolute-git-dir 2>/dev/null)"
FXLOCK="$FXGIT/fsgg-agent-env-suite.lock"

# A LIVE holder must refuse, and must say so as a LOCK refusal. `$$` is this suite's own pid, which
# is by definition alive, so no sleeper process is needed and nothing can outlive the run.
run pass "J1: a live lock holder is REFUSED at exit 99, naming the lock" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'rm -rf "'"$FXLOCK"'"; mkdir -p "'"$FXLOCK"'"; printf "%s" "$$" > "'"$FXLOCK"'/pid";
     out="$(FSGG_AGENT_ENV_SUITE_FIXTURE=1 "'"$FX"'/scripts/test-agent-env.sh" "'"$FX"'" 2>&1)"; rc=$?;
     test "$rc" -eq 99 && printf "%s" "$out" | grep -q "run holds"'
# A REFUSING run must not delete the holder's lock. Getting this wrong would turn the guard into a
# race amplifier: the second run would clear the first run's lock and both would proceed.
run pass "J2: a refused run leaves the holder's lock in place" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'test -d "'"$FXLOCK"'" && test -f "'"$FXLOCK"'/pid"'
# A DEAD holder must be reclaimed. Proved by CONTRAST rather than by inspecting the lock: with the
# fixture's shim also removed, a reclaimed lock reaches the shim check and refuses with the SHIM
# message, whereas a lock that was NOT reclaimed would still refuse with J1's lock message. Same exit
# code, different cause — which is the distinction this whole section exists to keep legible.
run pass "J3: a dead lock holder is reclaimed, so the refusal comes from the next guard instead" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'rm -rf "'"$FXLOCK"'"; mkdir -p "'"$FXLOCK"'"; printf "%s" 999999 > "'"$FXLOCK"'/pid";
     mv "'"$FX"'/scripts/agent-env.sh" "'"$FX"'/scripts/ae.hold";
     out="$(FSGG_AGENT_ENV_SUITE_FIXTURE=1 "'"$FX"'/scripts/test-agent-env.sh" "'"$FX"'" 2>&1)"; rc=$?;
     mv "'"$FX"'/scripts/ae.hold" "'"$FX"'/scripts/agent-env.sh";
     test "$rc" -eq 99 && printf "%s" "$out" | grep -q "missing before the suite started"'
# A missing shim must refuse rather than be mistaken for the section H mutation.
run pass "J4: a shim absent at startup is REFUSED at exit 99, not treated as the mutation" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'rm -rf "'"$FXLOCK"'"; mv "'"$FX"'/scripts/agent-env.sh" "'"$FX"'/scripts/ae.hold";
     out="$(FSGG_AGENT_ENV_SUITE_FIXTURE=1 "'"$FX"'/scripts/test-agent-env.sh" "'"$FX"'" 2>&1)"; rc=$?;
     mv "'"$FX"'/scripts/ae.hold" "'"$FX"'/scripts/agent-env.sh";
     test "$rc" -eq 99 && printf "%s" "$out" | grep -q "missing before the suite started"'

# --- F1: the predicate must refuse to decide from input it cannot read -----------------------
# Each of these seeds a lock whose holder IS ALIVE ($$ is this suite) but whose pid file cannot be
# evaluated. The old guard read every one of them as "holder is dead", deleted the live holder's
# lock, and ran. The assertion is therefore two-part every time: refuse at 99, AND leave the lock.
run pass "J5: an EMPTY pid file is unreadable, not dead — refuse and keep the lock" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'rm -rf "'"$FXLOCK"'"; mkdir -p "'"$FXLOCK"'"; : > "'"$FXLOCK"'/pid";
     out="$(FSGG_AGENT_ENV_SUITE_FIXTURE=1 "'"$FX"'/scripts/test-agent-env.sh" "'"$FX"'" 2>&1)"; rc=$?;
     test "$rc" -eq 99 && test -d "'"$FXLOCK"'" && printf "%s" "$out" | grep -q "cannot be identified"'
run pass "J6: a NON-NUMERIC pid file is unreadable, not dead — refuse and keep the lock" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'rm -rf "'"$FXLOCK"'"; mkdir -p "'"$FXLOCK"'"; printf "not-a-pid" > "'"$FXLOCK"'/pid";
     out="$(FSGG_AGENT_ENV_SUITE_FIXTURE=1 "'"$FX"'/scripts/test-agent-env.sh" "'"$FX"'" 2>&1)"; rc=$?;
     test "$rc" -eq 99 && test -d "'"$FXLOCK"'" && printf "%s" "$out" | grep -q "cannot be identified"'
run pass "J7: a MISSING pid file inside the window is unreadable, not dead — refuse and keep it" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'rm -rf "'"$FXLOCK"'"; mkdir -p "'"$FXLOCK"'";
     out="$(FSGG_AGENT_ENV_SUITE_FIXTURE=1 "'"$FX"'/scripts/test-agent-env.sh" "'"$FX"'" 2>&1)"; rc=$?;
     test "$rc" -eq 99 && test -d "'"$FXLOCK"'" && printf "%s" "$out" | grep -q "cannot be identified"'
# ...but it must NOT refuse forever, which is the half of F1 that made the old guard incoherent:
# too permissive on unreadable content and too strict on absent content, from the same root. Proved
# by CONTRAST, like J3: aged past the window with the fixture shim also absent, a reclaimed lock
# reaches the next guard and refuses with the SHIM message instead of the lock message.
run pass "J8: an unreadable lock AGED past the window is reclaimed, so it cannot wedge the suite" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'rm -rf "'"$FXLOCK"'"; mkdir -p "'"$FXLOCK"'"; touch -d "2 hours ago" "'"$FXLOCK"'";
     mv "'"$FX"'/scripts/agent-env.sh" "'"$FX"'/scripts/ae.hold";
     out="$(FSGG_AGENT_ENV_SUITE_FIXTURE=1 "'"$FX"'/scripts/test-agent-env.sh" "'"$FX"'" 2>&1)"; rc=$?;
     mv "'"$FX"'/scripts/ae.hold" "'"$FX"'/scripts/agent-env.sh";
     test "$rc" -eq 99 && printf "%s" "$out" | grep -q "missing before the suite started"'

# --- F2: THE TRAP. This section's heading names two obligations and used to discharge one. -----
# A bash trap handler that does not exit RESUMES the interrupted script. So `trap cleanup INT TERM`
# made the suite SURVIVE a signal that kills it at base: it released its lock mid-run while section
# H still had the real tracked shim moved aside, and went on emitting check results whose failures
# are indistinguishable from real ones.
run pass "J9: SIGTERM terminates the suite with the signal's own status and releases the lock" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'rm -rf "'"$FXLOCK"'";
     FSGG_AGENT_ENV_SUITE_FIXTURE=1 "'"$FX"'/scripts/test-agent-env.sh" "'"$FX"'" >/dev/null 2>&1 &
     p=$!; while [ ! -s "'"$FXLOCK"'/pid" ]; do kill -0 "$p" 2>/dev/null || break; sleep 0.02; done;
     kill -TERM "$p" 2>/dev/null; wait "$p"; rc=$?;
     test "$rc" -eq 143 && test ! -d "'"$FXLOCK"'"'
# SIGINT IS ASSERTED SEPARATELY FROM SIGTERM, and that is not redundancy. Reverting only the INT
# handler to the non-exiting form leaves every other check green — measured — so a TERM-only
# assertion would let the interactive Ctrl-C path regress silently. Each trapped signal the repair
# claims to handle needs its own witness.
#
# `set -m` IS LOAD-BEARING HERE, and finding out why cost a red check. A shell starts an ASYNC job
# with SIGINT ignored, and POSIX says a signal ignored on entry cannot be trapped — so without job
# control the fixture physically cannot install the INT handler this check exists to test, and the
# check fails for a reason that has nothing to do with the handler. Job control puts the child in
# its own process group with default dispositions, which is also the shape a real Ctrl-C arrives
# in.
run pass "J11: SIGINT terminates the suite with the signal's own status and releases the lock" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'set -m; rm -rf "'"$FXLOCK"'";
     FSGG_AGENT_ENV_SUITE_FIXTURE=1 "'"$FX"'/scripts/test-agent-env.sh" "'"$FX"'" >/dev/null 2>&1 &
     p=$!; while [ ! -s "'"$FXLOCK"'/pid" ]; do kill -0 "$p" 2>/dev/null || break; sleep 0.02; done;
     kill -INT "$p" 2>/dev/null; wait "$p"; rc=$?;
     test "$rc" -eq 130 && test ! -d "'"$FXLOCK"'"'
# And the restore half: interrupted INSIDE section H, the shim must come back. This is the check
# whose absence made the heading a claim rather than an assertion.
#
# `saw` IS THE DIFFERENCE BETWEEN A CHECK AND A COINCIDENCE. The first version polled for the shim
# to vanish and then asserted it was present again — so when it MISSED the window it signalled a
# process that had already finished, found the shim restored by section H's own `mv`, and passed.
# Deleting the restore from `cleanup` left it green: measured, and it is a could-not-fail check of
# exactly the kind this item exists to eliminate. It now records that it actually observed the shim
# missing, and REDS when it did not — an assertion that cannot confirm its own precondition must
# fail loudly rather than pass quietly.
run pass "J10: a run interrupted while the shim is moved aside restores it" \
    "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" \
    'set -m; rm -rf "'"$FXLOCK"'";
     FSGG_AGENT_ENV_SUITE_FIXTURE=1 "'"$FX"'/scripts/test-agent-env.sh" "'"$FX"'" >/dev/null 2>&1 &
     p=$!; saw=0;
     while kill -0 "$p" 2>/dev/null; do
       if [ ! -f "'"$FX"'/scripts/agent-env.sh" ]; then saw=1; kill -TERM "$p" 2>/dev/null; break; fi;
       sleep 0.01;
     done;
     wait "$p" 2>/dev/null;
     test "$saw" -eq 1 && test -f "'"$FX"'/scripts/agent-env.sh" && test ! -d "'"$FXLOCK"'"'
rm -rf "$FXLOCK"
fi

printf '\n==============================================================================\nRESULT: %s unexpected outcome(s)\n==============================================================================\n' "$FAILURES"
exit "$FAILURES"
