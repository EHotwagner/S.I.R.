<!--
  DO NOT ADD FRONT MATTER TO THIS FILE.

  `scripts/generate-in-app-docs.mjs` selects a page by testing whether it opens with a `---` front
  matter block (`if (!parsed) continue;`). Adding one would pull workspace wiring into the published
  product documentation bundle and count it against that generator's page budget. This page is for
  the people and agents who work ON the repository, not for the people who read the game.
-->

# Workspace onboarding

What an agent session needs to know before its first command in this repository. This page is
workspace wiring, not product documentation: it deliberately carries no front matter, so the
in-app documentation generator skips it and it never enters the published reference bundle.

## The .NET toolchain, and the error that names the wrong cause

`global.json` pins the SDK exactly:

```json
{ "sdk": { "version": "10.0.302", "rollForward": "disable", "allowPrerelease": false } }
```

`rollForward: disable` is deliberate — it is what makes a local build and a CI build the same
build. **Do not relax it, and do not "fix" a resolution failure by editing it or by installing a
different SDK.**

On a machine where the session environment arrives pointing at an install that does not carry
`10.0.302`, every `dotnet`-backed entry point fails at once — `scripts/fsgg-coord` (so the identity
mint, the claim, and every board read and write), `./build.sh`, `dotnet fsi` (which runs the
feedback-report validators the board's completion gate requires), and `dotnet fsgg-sdd` for every
lifecycle stage. The failure looks like this:

```
A compatible .NET SDK was not found.
  Requested SDK version: 10.0.302
  global.json file: /…/global.json
Installed SDKs:
  6.0.428 [/usr/share/dotnet/sdk]
  10.0.400 [/usr/share/dotnet/sdk]
```

That message names a version and a `global.json`, so it reads as "the pin is wrong" or "an SDK is
missing". Usually neither is true: **the pinned SDK is installed, and the session is simply looking
in the wrong root.** On the reference workspace it sits in `$HOME/.dotnet/sdk/10.0.302` while
`DOTNET_ROOT` and `PATH` both point at `/usr/share/dotnet`.

## The resolution order

`scripts/agent-env.sh` is the executable statement of this order; the list below is its contract.

0. If an ancestor shell already applied the wiring (`FSGG_AGENT_ENV_APPLIED` is set), do nothing.
1. If the `dotnet` already resolved by `PATH` carries `sdk/<pinned>`, **do nothing at all.** A
   machine whose system install satisfies the pin is left exactly as it was found.
2. Otherwise take the first of these roots that actually contains `sdk/<pinned>`, prepend it to
   `PATH`, and point `DOTNET_ROOT` at it:

   | order | candidate root           |
   | ----- | ------------------------ |
   | 1     | `$DOTNET_ROOT`           |
   | 2     | `$HOME/.dotnet`          |
   | 3     | `/usr/share/dotnet`      |
   | 4     | `/usr/local/share/dotnet`|
   | 5     | `/opt/dotnet`            |

   `$HOME/.dotnet/tools` is appended too, since that is where `dotnet tool install -g` puts its
   shims.
3. If no candidate carries the pin, change nothing and let the native error surface — the
   environment is not the problem in that case, and a half-applied `DOTNET_ROOT` would only make
   the real failure harder to read.

The order **prepends a candidate and falls through**; it never overrides unconditionally. Nothing
in it names an absolute home directory, and `DOTNET_ROOT` is exported only in case 2 and only to a
root positively confirmed to carry the pinned SDK, so the muxer `PATH` resolves and the root
apphosts consult cannot disagree.

## How each host runtime receives it

| runtime          | surface                                   | mechanism                                        |
| ---------------- | ----------------------------------------- | ------------------------------------------------ |
| Claude Code      | `.claude/settings.json` → `env`           | `BASH_ENV` → `scripts/agent-env.sh`               |
| Codex            | `.codex/config.toml` → `[shell_environment_policy.set]` | `BASH_ENV` → `scripts/agent-env.sh` |
| `.agents/`       | — | nothing to wire: it holds `skills/` only and has no configuration surface |

Both host entries carry the same value:

```
$(git rev-parse --show-toplevel 2>/dev/null)/scripts/agent-env.sh
```

### Why `BASH_ENV`, and not `DOTNET_ROOT` in the `env` block

Because recording `${HOME}/.dotnet` there does not work. This was measured, not assumed: **neither
host expands variables in its environment block.** Claude Code's `settings.json` `env` and Codex's
`[shell_environment_policy.set]` both hand the process the literal string `${HOME}/.dotnet`, and
`$HOME/.dotnet` likewise — `execvp` does not expand `PATH` entries either, so a literal there is
simply a directory that does not exist.

`BASH_ENV` is the one lever that closes the gap without writing an absolute home directory into a
tracked file: bash performs parameter expansion **and command substitution** on its value before
using it as a filename, and it does so for non-interactive shells whether or not they are login
shells (both were verified). So the hosts can name a file relative to the repository root, and the
resolution runs in the shell, where `$HOME` means something.

The shim is silent by construction. Anything it wrote to stdout would land inside every `$(…)` an
agent runs, and anything on stderr inside every log a gate parses.

### A host can revert the PATH prepend, and one does

Claude Code runs each of its shell calls as `source <shell-snapshot>; <command>`, and the last line
of that generated snapshot is an `export PATH=` carrying the PATH the host process itself started
with. It runs **after** `BASH_ENV` — bash sources that at shell start — so it silently reverts the
prepend while leaving `DOTNET_ROOT` set. That combination fixes nothing on its own: the muxer
resolves SDKs relative to its own location and does not consult `DOTNET_ROOT` for that.

Two things in the shim absorb this, and both are covered by the workspace's checks:

- a `dotnet` **function**, because bash resolves function names before `PATH` and the snapshot
  restores no functions and unsets none. It invokes the muxer by absolute path, so **the call** is
  correct in every shape. It also repairs `PATH` and removes itself, but **only when called
  directly** — read the next section before relying on that, because the bound is narrower than it
  looks.
- **step 0 re-heals rather than trusting its own marker.** A child shell inheriting a reverted
  `PATH` plus a set `FSGG_AGENT_ENV_APPLIED` would otherwise skip the work and stay broken; that is
  what `scripts/fsgg-coord` and `./build.sh` hit, since each is a bash script spawned from the
  clobbered shell.

Codex needs neither: it received the wiring correctly on the first try.

### Blast radius — read this before you debug strange shell behaviour

**`BASH_ENV` means a tracked file in this repository is sourced by every non-interactive bash
started in a session opened on this workspace.** That is every agent shell call, every
`#!/usr/bin/env bash` script those calls run, and the operator's own `!`-prefixed commands. If a
shell in this workspace is behaving in a way you cannot explain, `scripts/agent-env.sh` is the
first file to read, and `printenv BASH_ENV FSGG_AGENT_ENV_APPLIED` is the first thing to run.

Two consequences worth stating plainly:

- **A `git rev-parse --show-toplevel` runs once per shell**, because that command substitution is
  how the hosts name the file without hard-coding a home directory. The shim bounds the cost by
  rewriting `BASH_ENV` to the resolved absolute path as soon as it has one, so descendants skip the
  substitution, and by returning at step 0 once the environment is already correct.
- **Outside a git checkout the substitution yields a path that does not exist**, bash silently
  skips a `BASH_ENV` file it cannot read, and nothing is printed. That is the intended degradation,
  not an accident — the shim must never be the reason a shell prints something.

Re-sourcing is idempotent: every `PATH` change is guarded by a `case ":$PATH:"` membership test, so
sourcing the file twice cannot double-prepend.

### Where the `dotnet` function does not reach

A shell function is invisible to `execvp`. Anything that resolves `dotnet` without going through
bash — Node's `execFileSync("dotnet", …)` and `spawn("dotnet", …)`, which several tools under
`scripts/*.mjs` use, or `npm run`, which spawns `/bin/sh` (and bash invoked as `sh`
non-interactively does **not** read `BASH_ENV`) — sees only the inherited `PATH`.

This does not reach the repository's own entry points, and that was checked rather than assumed:
**`build.sh` and all 48 shell scripts under `scripts/` are `#!/usr/bin/env bash`**, so each gets its
own `BASH_ENV` pass and step 0 repairs its `PATH` before it runs any Node tool or `npm` script.
Whatever they spawn inherits the repaired `PATH`.

The residual, stated at its true width — an earlier version of this page claimed it was narrower,
which the independent review of S.I.R.#256 measured and disproved (finding M1). In the one shell
whose `PATH` a host reverts — Claude Code's tool shell — invoking a Node tool or an `npm run` script
**directly** gets the unrepaired `PATH`. A prior `dotnet` call fixes that **only if it was a direct
call**:

| shape | the call itself | the parent shell's `PATH` afterwards |
| --- | --- | --- |
| `dotnet fsi …` | correct | repaired |
| `V=$(dotnet --version)` | correct | **still unrepaired** |
| `dotnet --version \| cat` | correct | **still unrepaired** |

`$( )` and each pipeline stage are subshells, so every `export` and `unset -f` inside them is
discarded when they exit. The function therefore guarantees the *call*, not the shell. Going through
`./build.sh` or any `scripts/*.sh` is unaffected either way: each is a bash script that gets its own
`BASH_ENV` pass and re-heals `PATH` at step 0.

### `command -v dotnet` while the function is defined

A second consequence, and the one with teeth (finding M2). While the function exists,
`command -v dotnet` answers the bare word `dotnet` and `type -P dotnet` answers the **unpinned**
system muxer. That is what POSIX says a shell function does; nothing in `scripts/agent-env.sh` can
change it.

It matters because `scripts/qualify-pr.sh`, `scripts/qualify-production.sh` and
`scripts/run-ci-gate.sh` each set `SIR_REAL_DOTNET` from `$(command -v dotnet)`, and
`scripts/dotnet-invocation-trace.sh` execs that value — a bare word there would re-resolve to the
trace shim itself and recurse. **They are not affected, and the reason is structural rather than
lucky:** each is a `#!/usr/bin/env bash` script, so it gets its own `BASH_ENV` pass, inherits
`FSGG_AGENT_ENV_APPLIED`, returns at step 0 with `PATH` re-healed, and never defines the function.
`scripts/test-agent-env.sh` asserts exactly that, so if it ever stops holding a check goes red
rather than a build hanging.

If you need the real binary in a clobbered shell, make one direct `dotnet` call first — which
removes the function — or read `$FSGG_AGENT_ENV_APPLIED`, which holds the resolved root.

## When you still need the manual repair

The wiring reaches anything that starts a bash shell. It does **not** reach a host that executes a
command as bare argv with no shell, and it does not reach an interactive terminal (interactive
shells read `~/.bashrc`, not `BASH_ENV`). In those cases, or on a machine whose profile does not
already do it:

```sh
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
```

## Verifying it, honestly

A check run from a shell you have already repaired by hand proves nothing. Drop the inherited
variables and reproduce what a fresh session actually gets, then run the three entry points:

```sh
env -i HOME="$HOME" \
    PATH="$HOME/.local/bin:$HOME/.dotnet/tools:/usr/share/dotnet:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin" \
    DOTNET_ROOT=/usr/share/dotnet \
    BASH_ENV='$(git rev-parse --show-toplevel 2>/dev/null)/scripts/agent-env.sh' \
    bash --noprofile --norc -c '
      cd <repo> &&
      dotnet --version &&
      dotnet fsi --help >/dev/null &&
      scripts/fsgg-coord ready --repo "S.I.R." >/dev/null &&
      dotnet tool restore'
```

Drop the `BASH_ENV` line to get the negative control: the same commands must fail with the SDK
resolution error above. `--noprofile --norc` matters — this workspace's `~/.bashrc` exports the
repair, so without it the shell fixes itself and the check passes vacuously.

## The board identity

The same two host files carry the coordination board identity, and it is not interchangeable with
the FS-GG organization board:

```
FSGG_COORD_OWNER_TYPE=user
FSGG_COORD_OWNER=EHotwagner
FSGG_COORD_PROJECT=S.I.R.
```

The repository name on the board is `S.I.R.` **with the trailing dot**. `--repo S.I.R` returns an
empty list and a warning rather than an error, so always quote it: `--repo "S.I.R."`.
