#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
mode=${1:---check}
fixture_dir="$repo_root/tests/fixtures/rules-corpus/v1"
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"
temporary_dir=$(mktemp -d /tmp/sir-rules-corpus.XXXXXX)
trap 'rm -rf "$temporary_dir"' EXIT

dotnet run --project "$project" -c Release -- --print-rules-manifest > "$temporary_dir/manifest.json"
dotnet run --project "$project" -c Release --no-build -- --print-rules-coverage > "$temporary_dir/coverage.json"
dotnet run --project "$project" -c Release --no-build -- --print-rules-application > "$temporary_dir/representative-application.hex"

jq -e '.schemaVersion == 1 and (.rules | length == 7)' "$temporary_dir/manifest.json" >/dev/null
jq -e '.schemaVersion == 1 and .authorityBoundary.outside == "legacy"' "$temporary_dir/coverage.json" >/dev/null

case "$mode" in
  --write)
    mkdir -p "$fixture_dir"
    cp "$temporary_dir/manifest.json" "$fixture_dir/manifest.json"
    cp "$temporary_dir/coverage.json" "$fixture_dir/coverage.json"
    cp "$temporary_dir/representative-application.hex" "$fixture_dir/representative-application.hex"
    ;;
  --check)
    cmp "$temporary_dir/manifest.json" "$fixture_dir/manifest.json"
    cmp "$temporary_dir/coverage.json" "$fixture_dir/coverage.json"
    cmp "$temporary_dir/representative-application.hex" "$fixture_dir/representative-application.hex"
    ;;
  *)
    echo "usage: scripts/generate-rules-corpus.sh [--check|--write]" >&2
    exit 2
    ;;
esac
