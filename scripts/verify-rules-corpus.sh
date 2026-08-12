#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"

"$repo_root/scripts/generate-rules-corpus.sh" --check

test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/manifest.json" | cut -d' ' -f1)" = "e5bfe82d40e72ff8b41898e408c50dd0d8fb7e05b72c6acc24baab0e3b451ddc" || { echo "retained v1 manifest changed" >&2; exit 1; }
test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/coverage.json" | cut -d' ' -f1)" = "39eecda1018c504eab7b03c60228bf155c99aa42433724655da42d9ee470d554" || { echo "retained v1 coverage changed" >&2; exit 1; }
test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/representative-application.hex" | cut -d' ' -f1)" = "f42835c3fc4691b59ff71c0b31de0e74caa21455bf9d5e7658b483e0b2da2606" || { echo "retained v1 application changed" >&2; exit 1; }

while IFS=$'\t' read -r source_path source_symbol; do
  test -f "$repo_root/$source_path" || { echo "missing rule source: $source_path" >&2; exit 1; }
  symbol_name=${source_symbol##*.}
  rg -q "let (private )?${symbol_name}( |$)" "$repo_root/$source_path" || {
    echo "unresolved rule source symbol: $source_symbol in $source_path" >&2
    exit 1
  }
done <<< "$(jq -r '.rules[].source | select(. != null) | [.path, .symbol] | @tsv' "$repo_root/tests/fixtures/rules-corpus/v2/manifest.json")"

source_commit=$(jq -r '.sourceCommit' "$repo_root/tests/fixtures/rules-corpus/v2/manifest.json")
declared_source_sha=$(sed -n 's/.*"combat-rules-source-sha256", System.Text.Encoding.UTF8.GetBytes "\([0-9a-f]\{64\}\)".*/\1/p' "$repo_root/src/SIR.Simulation/CombatRules.fs")
actual_source_sha=$(git -C "$repo_root" show "$source_commit:src/SIR.Simulation/CombatRules.fs" | sha256sum | cut -d' ' -f1)
test "$declared_source_sha" = "$actual_source_sha" || { echo "combat implementation fingerprint does not match pinned source" >&2; exit 1; }
declared_package_sha=$(sed -n 's/.*"fs-gg-game-core-nupkg-sha256", System.Text.Encoding.UTF8.GetBytes "\([0-9a-f]\{64\}\)".*/\1/p' "$repo_root/src/SIR.Simulation/CombatRules.fs")
captured_package_sha=$(jq -r '.sha256' "$repo_root/docs/dependency-surface/FS.GG.Game.Core/0.13.0.json")
test "$declared_package_sha" = "$captured_package_sha" || { echo "Game.Core implementation fingerprint does not match dependency receipt" >&2; exit 1; }

if rg -n --glob '*.js' --glob '*.ts' --glob '!**/.fable/**' '(baseDamage|expectedDamage).*(trace|retention)|(trace|retention).*(baseDamage|expectedDamage)' "$repo_root/src"; then
  echo "copied JavaScript/TypeScript combat semantics detected" >&2
  exit 1
fi

mutation_log=$(mktemp /tmp/sir-rules-mutation.XXXXXX)
trap 'rm -f "$mutation_log"' EXIT
if dotnet run --project "$project" -c Release --no-build -- --inject-rules-corpus-divergence >"$mutation_log" 2>&1; then
  echo "rules-corpus protected-subject mutation unexpectedly passed" >&2
  exit 1
fi
rg -q 'first divergence: fixture=rules-corpus' "$mutation_log" || {
  echo "rules-corpus mutation failed without the actionable divergence diagnostic" >&2
  exit 1
}

echo "rules corpus generation, source resolution, copied-semantics, and mutation gates passed"
