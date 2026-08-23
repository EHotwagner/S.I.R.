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
# Exit code is the number of unexpected outcomes, so 0 is green.

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

FAILURES=0
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

fresh() { # fresh <HOME> <PATH> <DOTNET_ROOT> <BASH_ENV> <script>
  env -i HOME="$1" USER="${USER:-runner}" TERM=dumb \
      PATH="$2" DOTNET_ROOT="$3" BASH_ENV="$4" \
      DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 \
      FSGG_COORD_OWNER_TYPE=user FSGG_COORD_OWNER=EHotwagner FSGG_COORD_PROJECT="S.I.R." \
      GH_TOKEN="${GH_TOKEN:-}" GITHUB_TOKEN="${GITHUB_TOKEN:-}" \
      bash --noprofile --norc -c "cd '$ROOT' && $5" 2>&1
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
mv "$SHIM" "$TMP/agent-env.sh.bak"
run fail "wired, but the shim is deleted: dotnet --version" "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'dotnet --version'
mv "$TMP/agent-env.sh.bak" "$SHIM"
run pass "shim restored: dotnet --version"                  "$H" "$FRESH_PATH" /usr/share/dotnet "$BASH_ENV_VALUE" 'dotnet --version >/dev/null'

section "I. DOTNET_ROOT — the exported root is what an APPHOST consults (S.I.R.#277)"
# WHY THIS SECTION EXISTS. Before it did, `export DOTNET_ROOT="$candidate"` could be deleted from the
# shim and all 36 checks above stayed green (measured as mutation S8 in PR #260 review round 1, and
# reproduced at this head). That is not because the line does nothing — it is because every probe
# above reaches the SDK through the MUXER, and the muxer resolves SDKs relative to its own location
# and ignores DOTNET_ROOT for that. The file therefore asserted a purpose for that line which nothing
# here could falsify. These two checks are that falsifier.
#
# WHAT DOTNET_ROOT ACTUALLY DECIDES. A framework-dependent APPHOST does not go through the muxer: it
# reads DOTNET_ROOT to locate hostfxr, and falls back to the global install location only when that
# directory does not exist. The apphosts on THIS workspace's hot path are the `dotnet tool install
# -g` shims in `$HOME/.dotnet/tools`, which step 2 of the shim deliberately keeps on PATH. A built
# `fsgg-coord-engine` would be one as well, but not in this repository: only the repo owning coord's
# source resolves `scripts/fsgg-coord` at tier 2, and this one is a receiver whose engine comes from
# `.config/dotnet-tools.json` at tier 4, through the muxer.
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

printf '\n==============================================================================\nRESULT: %s unexpected outcome(s)\n==============================================================================\n' "$FAILURES"
exit "$FAILURES"
