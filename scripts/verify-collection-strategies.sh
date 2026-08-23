#!/usr/bin/env bash
# Collection-strategy regression gate (S.I.R.#249, routed by S.I.R.#263).
#
# These are RATIO assertions, not absolute budgets: absolute nanoseconds vary by host, but the
# ordering between strategies does not. The gate fails when a shape that was measured as
# super-linear stops looking super-linear -- which means either the benchmark stopped measuring
# anything, or someone reintroduced the slow shape and the comparison collapsed.
#
# THE HARNESS'S EXIT STATUS IS THE GATE'S EXIT STATUS. `set -e`, and the harness invocation being
# the thing whose status is returned, is what carries a non-zero out to scripts/qualify-pr.sh,
# scripts/run-ci-gate.sh's `status=fail`, and the sir.ci-gate-result/v1 receipt pr-verdict joins.
# There is deliberately no `|| true` and no `|| echo`: either would let the gate run, print FAILED,
# and still report pass (S.I.R.#265).
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
project="$repo_root/tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj"
receipt="${SIR_COLLECTIONS_RECEIPT:-$repo_root/artifacts/test-results/item-249-collection-strategies.json}"

# ---------------------------------------------------------------- environment confound guard
#
# This gate is a TIMING measurement, so the runtime it resolves is part of its subject. An agent
# shell exports DOTNET_ROOT_X64, and hostfxr consults it BEFORE DOTNET_ROOT when an APPHOST -- what
# `dotnet run` ultimately launches -- looks for its framework. The muxer ignores it. So the two
# routes can land on different patch runtimes and an unguarded run silently measures one the pin
# does not name.
#
# Unset IN-SHELL. NOT `env -u ... dotnet ...`: scripts/agent-env.sh defines `dotnet` as a shell
# function hard-routing to the pinned muxer, and `env` execs the PATH binary, bypassing it -- that
# spelling measures env-vs-function, not guarded-vs-unguarded.
unset DOTNET_ROOT_X64 DOTNET_HOST_PATH

# ASSERT THE RESOLVED PIN, NOT THE MECHANISM. `dotnet` is not a shell function inside a child
# script (agent-env.sh returns early for those), so asserting the mechanism would fail for the
# wrong reason. Two independent readings of the same quantity are compared instead:
#   trusted side  -- `dotnet --list-runtimes`, the MUXER's view. The muxer resolves relative to its
#                    own location and ignores DOTNET_ROOT_X64, so this is the pin.
#   measured side -- the harness's own Environment.Version, reported in its receipt. That is an
#                    APPHOST, so it FOLLOWS DOTNET_ROOT_X64.
# They agree only while the confound is absent, which is what makes this abort load-bearing rather
# than decorative.
expected_sdk=$(node -e 'process.stdout.write(JSON.parse(require("node:fs").readFileSync(process.argv[1],"utf8")).sdk.version)' "$repo_root/global.json")
actual_sdk=$(cd "$repo_root" && dotnet --version)
if [[ "$actual_sdk" != "$expected_sdk" ]]; then
  echo "verify-collection-strategies: ABORT -- resolved SDK $actual_sdk is not global.json's pin $expected_sdk; this measurement would not be about the pinned toolchain." >&2
  exit 90
fi
pinned_runtime=$(cd "$repo_root" && dotnet --list-runtimes \
  | sed -n 's/^Microsoft\.NETCore\.App \([0-9][0-9.]*\) .*$/\1/p' | sort -V | tail -1)
if [[ -z "$pinned_runtime" ]]; then
  echo "verify-collection-strategies: ABORT -- could not read a Microsoft.NETCore.App runtime from the muxer; the pin is unknown, which is not the same as satisfied." >&2
  exit 92
fi

# Tiered compilation adds startup-dependent noise that swamps small-n comparisons.
export DOTNET_TieredCompilation="${DOTNET_TieredCompilation:-0}"

dotnet restore "$project" --locked-mode
dotnet build "$project" -c Release --no-restore

set +e
dotnet run --project "$project" -c Release --no-build -- \
  --collections \
  --collections-receipt "$receipt"
collections_status=$?
set -e

# The receipt is written before the harness exits, on both the pass and the fail path, so this
# reads the runtime of the process that produced the numbers above -- not of a stand-in probe.
measured_runtime=$(node -e 'process.stdout.write(String(JSON.parse(require("node:fs").readFileSync(process.argv[1],"utf8")).RuntimeVersion ?? ""))' "$receipt")
if [[ "$measured_runtime" != "$pinned_runtime" ]]; then
  echo "verify-collection-strategies: ABORT -- the harness measured on runtime '${measured_runtime:-<unreadable>}' while the muxer's pinned runtime is '$pinned_runtime'. The timings above are about a runtime the pin does not name; refusing to report them either way." >&2
  exit 91
fi

exit "$collections_status"
