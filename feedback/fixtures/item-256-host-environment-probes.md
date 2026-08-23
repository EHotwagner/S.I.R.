# Captured host-environment probe output (S.I.R.#256)

**This file is a transcript of output captured from external agent hosts. It is not a property of
this repository, and this repository neither controls nor guarantees any behaviour recorded here.**

What it establishes is bounded and worth stating exactly: *these hosts behaved this way, at these
versions, on this date, on one machine.* It is evidence of an observation, not a specification. If a
host changes its behaviour, this file becomes a historical record rather than a false claim — the
thing that would catch such a change is the assertion suite around `scripts/agent-env.sh`, which
asserts the bypass boundary in both directions, not this transcript.

## Read this before any path below, because the redaction is load-bearing

Two things in this file look similar and mean opposite things. Getting them confused would make a
transcript that proves nothing look like proof:

- **`${HOME}`** appears verbatim wherever a host returned the string **unexpanded**. That *is* the
  finding of §4.2. It is not a redaction.
- **`<HOME>`** is a redaction standing in for a real absolute home directory that a host *did*
  expand or print.

Nothing else is altered. Output is otherwise verbatim, trimmed only where noted with `…`.

That sentence was not true when this file was first committed: the Probe D1 transcript silently
dropped one matching line (`123:` of the snapshot) with no trim marker, which a second independent
audit caught by re-running the fixture's own regenerate command. The line is restored below. The
conclusions D1 carries never depended on it, which is exactly why the omission was easy to make and
worth recording rather than quietly repairing.

## What was captured, and where

| | |
| --- | --- |
| Captured | 2026-08-22 (UTC) |
| Machine | Linux, single host; `/bin/sh` is a symlink to `bash` |
| Claude Code | 2.1.240 (`claude --version`) |
| Codex CLI | 0.149.0 (`codex --version` → `codex-cli 0.149.0`), installed at `~/.local/lib/node_modules/@openai/codex` |
| .NET roots present | `/usr/share/dotnet` (`6.0.428`, `10.0.400`), `<HOME>/.dotnet` (`10.0.302`) |
| Cited by | `feedback/2026-08-22-sir-item-256-agent-toolchain-wiring.md` §4.2, §4.3 |

## Probe A — Claude Code `settings.json` `env` does not expand variables

Regenerate:

```sh
mkdir -p /tmp/probe/.claude && cd /tmp/probe
cat > .claude/settings.json <<'JSON'
{ "env": { "FSGG_PROBE_BRACE": "${HOME}/.dotnet",
           "FSGG_PROBE_BARE":  "$HOME/.dotnet",
           "FSGG_PROBE_PATH":  "${HOME}/.dotnet:${PATH}" } }
JSON
claude -p 'Run exactly this bash command and show me its raw output: printenv FSGG_PROBE_BRACE FSGG_PROBE_BARE; echo "PATHHEAD:${FSGG_PROBE_PATH%%:*}"' \
  --permission-mode bypassPermissions --model haiku
```

Captured:

```
${HOME}/.dotnet
$HOME/.dotnet
PATHHEAD:${HOME}/.dotnet
```

All three arrived as literal strings. `execvp` does not expand `PATH` entries either, so the third
line is a `PATH` whose first entry is a directory that cannot exist.

## Probe B — Codex `[shell_environment_policy.set]` does not expand variables

Regenerate:

```sh
codex sandbox -c 'shell_environment_policy.set.FSGG_PROBE_BRACE=${HOME}/.dotnet' \
  printenv FSGG_PROBE_BRACE
```

Captured:

```
${HOME}/.dotnet
```

A control confirming the project-level config is read at all, from the repository root:

```sh
codex sandbox printenv FSGG_COORD_PROJECT
```

```
S.I.R.
```

So the value is delivered, and delivered literally.

## Probe C — `BASH_ENV` *is* expanded, including under a login shell

This is the lever the fix uses; both halves were checked because bash's startup rules differ for
login shells.

Regenerate (a fixture repo containing `.claude/agent-env.sh` that exports `FSGG_PROBE_BASH_ENV`):

```sh
# settings.json: { "env": { "BASH_ENV": "$(git rev-parse --show-toplevel 2>/dev/null)/.claude/agent-env.sh" } }
claude -p 'Run exactly this bash command and show its raw output: printenv BASH_ENV; echo "MARKER=${FSGG_PROBE_BASH_ENV:-<unset>}"' \
  --permission-mode bypassPermissions --model haiku
```

Captured — the value is delivered verbatim and bash expanded the command substitution itself:

```
$(git rev-parse --show-toplevel 2>/dev/null)/.claude/agent-env.sh
MARKER=sourced-ok
```

Non-interactive **login** shell, which reads profile files and might have skipped `BASH_ENV`:

```sh
codex sandbox -c 'shell_environment_policy.set.BASH_ENV=<abs-path>/.claude/agent-env.sh' \
  bash -lc 'echo "LOGIN=${FSGG_PROBE_BASH_ENV:-unset}"'
```

```
LOGIN=sourced-ok
```

## Probe D — Claude Code re-exports its own `PATH` after `BASH_ENV` has run

Two independent observations.

**D1. The generated shell snapshot's final line.** Claude Code runs each shell call as
`source <shell-snapshot>; <command>`.

```sh
grep -c "" ~/.claude/shell-snapshots/snapshot-*.sh
grep -n "unset -f\|unalias\|^export " ~/.claude/shell-snapshots/snapshot-*.sh
```

Captured (file was 192 lines):

```
192
3:unalias -a 2>/dev/null || true
123:if ! (unalias rg 2>/dev/null; command -v rg) >/dev/null 2>&1; then
138:unalias find 2>/dev/null || true
139:unalias grep 2>/dev/null || true
169:unalias pkill 2>/dev/null || true
192:export PATH=<HOME>/.local/bin:<HOME>/.dotnet/tools:/usr/share/dotnet:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
```

The only `export` is `PATH`, and it is the last line. There is **no `unset -f`** anywhere — only
aliases are cleared. That absence is what lets a shell function survive the snapshot, and it is the
basis of the workaround in `scripts/agent-env.sh`.

**D2. The effect, observed through the host.** With the wiring in place but before the function
existed, in a real session:

```sh
claude -p 'Run exactly this one bash command and report ONLY its first four output lines verbatim: printenv FSGG_AGENT_ENV_APPLIED; printenv DOTNET_ROOT; command -v dotnet; echo "PATHHEAD=${PATH%%:*}"' \
  --permission-mode bypassPermissions --model haiku
```

Captured:

```
<HOME>/.dotnet
<HOME>/.dotnet
/usr/share/dotnet/dotnet
PATHHEAD=<HOME>/.local/bin
```

Read together: the shim ran and selected the correct root (line 1), its `DOTNET_ROOT` export
survived (line 2), and its `PATH` prepend did not (lines 3 and 4). `DOTNET_ROOT` alone resolves
nothing, because the muxer resolves SDKs relative to its own location — line 3 is still the install
that does not carry the pin. An environment that looks repaired and is not is the reason this was
worth capturing.

## Probe E — after the fix, both hosts resolve the pin end to end

Claude Code:

```sh
claude -p 'Run exactly this one bash command and report ONLY its raw output verbatim: dotnet --version; scripts/fsgg-coord --version; dotnet fsi --help >/dev/null && echo FSI_OK' \
  --permission-mode bypassPermissions --model haiku
```

```
10.0.302
0.71.0.0
FSI_OK
```

Codex:

```sh
codex sandbox bash -lc 'dotnet --version; scripts/fsgg-coord --version'
```

```
10.0.302
0.71.0.0
```

Codex was green on the first attempt and needed neither the `dotnet` function nor the step-0
re-heal; both exist for Probe D.
