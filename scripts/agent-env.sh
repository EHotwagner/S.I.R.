# scripts/agent-env.sh — the workspace's .NET toolchain resolution order, for agent sessions.
#
# SOURCE THIS; DO NOT RUN IT. Agent hosts point `BASH_ENV` at this file (see
# `.claude/settings.json` and `.codex/config.toml`), so bash sources it at the start of every
# non-interactive shell an agent session spawns. Exports from an executed copy would be discarded.
#
# WHY THIS FILE EXISTS AND IS NOT TWO LINES IN A SETTINGS `env` BLOCK (S.I.R.#256).
# `global.json` pins SDK 10.0.302 with `rollForward: disable`, and on the reference workspace the
# session environment arrives with `DOTNET_ROOT=/usr/share/dotnet`, an install carrying only
# 6.0.428 and 10.0.400. The pinned SDK IS installed, under `$HOME/.dotnet`, but nothing pointed a
# session at it — so `scripts/fsgg-coord`, `./build.sh`, `dotnet fsi` and `dotnet fsgg-sdd` all
# failed with an SDK-resolution error that names a version and a `global.json` and therefore steers
# the reader toward editing the pin or installing an SDK. Both are wrong; the pin is what makes
# local and CI builds reproducible.
#
# The obvious fix — record `$HOME/.dotnet` in the host `env` blocks next to the board identity that
# is already there — does not work, and this was MEASURED, not assumed: neither host expands
# variables in those values. Claude Code `.claude/settings.json` `env` and Codex
# `[shell_environment_policy.set]` both deliver `${HOME}/.dotnet` to the process as the literal
# nine-character-prefixed string, not as a path. Hard-coding an absolute home directory was
# ruled out. `BASH_ENV` is the one lever that closes the gap: bash performs parameter expansion and
# command substitution on its value before using it as a filename, so the hosts can name this file
# relative to the repository root without either of them expanding anything.
#
# RESOLUTION ORDER (this is the documented contract; `docs/workspace-onboarding.md` explains it):
#   0. If this shell already applied the wiring, do nothing.
#   1. If the `dotnet` already on PATH resolves the version `global.json` pins, do nothing at all —
#      a machine whose system install carries the pin is left exactly as it was.
#   2. Otherwise take the FIRST candidate root that actually carries `sdk/<pinned>`:
#        $DOTNET_ROOT, $HOME/.dotnet, /usr/share/dotnet, /usr/local/share/dotnet, /opt/dotnet
#      and prepend it to PATH (plus `$HOME/.dotnet/tools`, where dotnet global tools live).
#   3. If no candidate carries the pin, change nothing and let the native error surface.
#
# It prepends a candidate and falls through; it never overrides unconditionally. `DOTNET_ROOT` is
# exported only in case 2, only to the root that was positively confirmed to carry the pinned SDK,
# so the muxer that PATH now resolves and the root that apphosts consult agree.
#
# THAT LAST CLAUSE IS A MEASURED CLAIM, NOT A PLAUSIBLE ONE (S.I.R.#277). It used to be neither
# evidenced nor protected: deleting the `export DOTNET_ROOT` below left the whole suite green,
# because PATH alone satisfies every probe that reaches the SDK through the MUXER — and the muxer
# resolves SDKs relative to its own location and ignores DOTNET_ROOT for that. What DOTNET_ROOT
# decides is APPHOSTS, which do not go through the muxer at all: they read it to find hostfxr, and
# fall back to the global install location only when it names no directory. This workspace runs
# apphosts on two hot paths — the `dotnet tool install -g` shims in `$HOME/.dotnet/tools`, and the
# built `fsgg-coord-engine` that `scripts/fsgg-coord` execs at its tier 2. Without the export they
# keep working while loading from the root the session ARRIVED with, which on the reference
# workspace carries a different Microsoft.NETCore.App than the one PATH now resolves. Measured with
# COREHOST_TRACE=1: `/usr/share/dotnet/shared/Microsoft.NETCore.App/10.0.11` without the export
# against `$HOME/.dotnet/shared/Microsoft.NETCore.App/10.0.10` with it. `scripts/test-agent-env.sh`
# section I builds a real apphost and asserts the agreement, so deleting the export now reds a
# check instead of nothing.
#
# IT MUST STAY SILENT. Anything written to stdout here would land inside every `$(...)` an agent
# runs; anything on stderr would land in every log a gate parses. On any doubt this file returns
# without touching the environment.

__fsgg_agent_env() {
  # 0. Already applied in an ancestor shell — but verify the invariant instead of trusting the
  #    marker. A host that reverts PATH after this file has been sourced (see the note in step 2)
  #    leaves the marker set and the PATH wrong, and every child shell would then inherit the
  #    broken PATH and skip the work. Re-prepending is cheap and needs neither git nor global.json.
  if [ -n "${FSGG_AGENT_ENV_APPLIED:-}" ]; then
    case "$FSGG_AGENT_ENV_APPLIED" in
      /*)
        case ":$PATH:" in
          *":$FSGG_AGENT_ENV_APPLIED:"*) ;;
          *) PATH="$FSGG_AGENT_ENV_APPLIED:$PATH"; export PATH ;;
        esac
        ;;
    esac
    return 0
  fi

  local root pinned current candidate

  root="$(command git rev-parse --show-toplevel 2>/dev/null)"
  if [ -z "$root" ] || [ ! -f "$root/global.json" ]; then
    return 0
  fi

  # The pinned SDK version, without depending on jq being installed. `global.json` here carries a
  # single `"version"` key, under `sdk`; if that ever stops being true this returns the wrong string
  # and every candidate check below simply fails, which is the safe direction.
  pinned="$(sed -n 's/.*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$root/global.json" 2>/dev/null | head -n 1)"
  if [ -z "$pinned" ]; then
    return 0
  fi

  # 1. Is the pin already satisfied by whatever `dotnet` PATH resolves? The muxer resolves SDKs
  #    relative to its own location, so the directory holding the resolved binary is the root that
  #    will be consulted. Follow symlinks where the platform can (a bare /usr/bin/dotnet shim is
  #    common); where it cannot, the check just fails and step 2 reaches the same install anyway.
  current="$(command -v dotnet 2>/dev/null)"
  if [ -n "$current" ]; then
    current="$(readlink -f "$current" 2>/dev/null || printf '%s' "$current")"
    if [ -d "${current%/*}/sdk/$pinned" ]; then
      export FSGG_AGENT_ENV_APPLIED="already-resolved"
      export BASH_ENV="$root/scripts/agent-env.sh"
      return 0
    fi
  fi

  # 2. First candidate root that actually carries the pinned SDK wins.
  for candidate in "${DOTNET_ROOT:-}" "$HOME/.dotnet" /usr/share/dotnet /usr/local/share/dotnet /opt/dotnet; do
    [ -n "$candidate" ] || continue
    [ -x "$candidate/dotnet" ] || continue
    [ -d "$candidate/sdk/$pinned" ] || continue

    case ":$PATH:" in
      *":$candidate:"*) ;;
      *) PATH="$candidate:$PATH" ;;
    esac
    if [ -d "$HOME/.dotnet/tools" ]; then
      case ":$PATH:" in
        *":$HOME/.dotnet/tools:"*) ;;
        *) PATH="$PATH:$HOME/.dotnet/tools" ;;
      esac
    fi

    export PATH
    # Apphosts only — the muxer ignores this. Deleting it reds section I of the suite (#277).
    export DOTNET_ROOT="$candidate"
    export FSGG_AGENT_ENV_APPLIED="$candidate"
    # Children no longer need the command substitution the hosts configured: name the resolved file
    # directly so every descendant shell skips the `git rev-parse` and returns at step 0.
    export BASH_ENV="$root/scripts/agent-env.sh"

    # A HOST CAN UNDO THE PREPEND ABOVE, AND ONE MEASURABLY DOES. Claude Code runs each of its
    # shell calls as `source <shell-snapshot>; <command>`, and the last line of that generated
    # snapshot is an `export PATH=` carrying the PATH the host process itself started with. It runs
    # AFTER this file — bash sources BASH_ENV at shell start — so it silently reverts PATH while
    # leaving DOTNET_ROOT set, which on its own fixes nothing: the muxer resolves SDKs relative to
    # its own location and ignores DOTNET_ROOT for that. (Verified: DOTNET_ROOT alone still fails
    # the pin.) Ordering cannot win against that, and the host's PATH is not ours to author.
    #
    # A function is what survives it: bash resolves function names before PATH, and a snapshot
    # restores no functions and unsets none.
    #
    # WHAT THIS FUNCTION GUARANTEES IS THE CALL, AND ONLY THE CALL. It invokes the muxer by absolute
    # path, so `dotnet …` is correct in every invocation shape.
    #
    # IT REPAIRS PATH AND REMOVES ITSELF ONLY WHERE THAT PERSISTS. `BASH_SUBSHELL` is 0 in the
    # current shell and non-zero inside `$( )` or a pipeline stage, where every `export` and
    # `unset -f` is discarded when the subshell exits anyway. Guarding the mutation does not change
    # what any caller observes — the discarded writes were already discarded — it makes the code say
    # what it does, so the next reader does not re-derive the guarantee from an unguarded `unset -f`
    # and overclaim it the way this file's documentation once did (S.I.R.#256 review, M1: the
    # defect was the sentence, not the behaviour). The consequence the guard makes legible is real:
    # after `V=$(dotnet --version)` the parent shell's PATH is STILL unrepaired, so a Node tool or
    # an `npm run` script invoked after it sees the host's PATH, not this one.
    #
    # WHILE IT IS DEFINED, `command -v dotnet` ANSWERS `dotnet` AND `type -P dotnet` ANSWERS THE
    # UNPINNED MUXER (S.I.R.#256 review, M2). No arrangement of this file can fix that — it is what
    # POSIX says a shell function does — so it is bounded and asserted rather than pretended away,
    # and it matters because `scripts/qualify-pr.sh`, `scripts/qualify-production.sh` and
    # `scripts/run-ci-gate.sh` each set `SIR_REAL_DOTNET` from `$(command -v dotnet)`, which
    # `scripts/dotnet-invocation-trace.sh` then execs — a bare word there would re-resolve to the
    # trace shim itself. They are NOT affected, and the reason is structural rather than lucky: each
    # is a `#!/usr/bin/env bash` script, so it gets its own BASH_ENV pass, inherits
    # FSGG_AGENT_ENV_APPLIED, returns at step 0 with PATH re-healed, and never defines this
    # function. `docs/workspace-onboarding.md` states that bound; `scripts/test-agent-env.sh`
    # asserts it, including the recursion it would cause if it ever stopped holding.
    eval 'dotnet() {
            if [ "${BASH_SUBSHELL:-0}" -eq 0 ]; then
              case ":$PATH:" in
                *":'"$candidate"':"*) ;;
                *) PATH="'"$candidate"':$PATH"; export PATH ;;
              esac
              unset -f dotnet
            fi
            command "'"$candidate"'/dotnet" "$@"
          }'
    return 0
  done

  # 3. Nothing here carries the pin. Leave the environment alone so the real error is the one the
  #    caller sees; `docs/workspace-onboarding.md` explains what that error actually means.
  return 0
}

__fsgg_agent_env
unset -f __fsgg_agent_env
