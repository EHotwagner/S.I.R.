#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
project="$repo_root/tests/SIR.Match.Tests/SIR.Match.Tests.fsproj"
temporary_dir=$(mktemp -d /tmp/sir-ci-product-performance-route.XXXXXX)
trap 'rm -rf -- "$temporary_dir"' EXIT

grep -F -- '--ordinary-pr-functional' "$repo_root/scripts/qualify-pr.sh" >/dev/null
if grep -F -- '--ordinary-pr-functional' "$repo_root/scripts/qualify-production.sh" >/dev/null; then
  echo "Protected qualification accepted the ordinary PR performance route." >&2
  exit 1
fi

if "$repo_root/scripts/test-conformance.sh" --ordinary-pr-functional >"$temporary_dir/unguarded.log" 2>&1; then
  echo "The ordinary PR performance route was accepted outside domain-only conformance." >&2
  exit 1
fi
grep -F -- '--ordinary-pr-functional requires --domain-only' "$temporary_dir/unguarded.log" >/dev/null

SIR_TACTICAL_MUTATE_REP_TIMING=1 dotnet run \
  --project "$project" --no-build --no-restore -- --functional-cross-runtime \
  >"$temporary_dir/functional.log" 2>&1

if SIR_TACTICAL_MUTATE_REP_TIMING=1 dotnet run \
  --project "$project" --no-build --no-restore \
  >"$temporary_dir/protected.log" 2>&1; then
  echo "Protected product performance accepted the forced 50 ms subject breach." >&2
  exit 1
fi
grep -F -- 'exceeded its 50 ms timing budget' "$temporary_dir/protected.log" >/dev/null

echo "CI product-performance route mutation passed: ordinary PR retained the functional workload while protected qualification enforced the unchanged 50 ms budget."
