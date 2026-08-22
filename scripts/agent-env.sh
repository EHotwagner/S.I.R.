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
    # A function is what survives it: bash resolves function names before PATH, snapshots restore
    # no functions and unset none, and this one repairs the shell it is invoked in and then removes
    # itself — so the FIRST `dotnet` call in such a shell leaves PATH correct for everything after
    # it, including non-bash children. In a shell nobody clobbered it fires once, finds PATH already
    # carrying the root, and is gone.
    eval 'dotnet() {
            unset -f dotnet
            case ":$PATH:" in
              *":'"$candidate"':"*) ;;
              *) PATH="'"$candidate"':$PATH"; export PATH ;;
            esac
            command dotnet "$@"
          }'
    return 0
  done

  # 3. Nothing here carries the pin. Leave the environment alone so the real error is the one the
  #    caller sees; `docs/workspace-onboarding.md` explains what that error actually means.
  return 0
}

__fsgg_agent_env
unset -f __fsgg_agent_env
