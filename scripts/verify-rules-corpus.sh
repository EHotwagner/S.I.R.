#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"

search_quiet() {
  local pattern=$1
  local path=$2
  if test "${SIR_RULES_FORCE_GREP:-0}" != 1 && command -v rg >/dev/null 2>&1; then
    rg -q -- "$pattern" "$path"
  else
    grep -Eq -- "$pattern" "$path"
  fi
}

"$repo_root/scripts/generate-rules-corpus.sh" --check

coverage_mutant=$(mktemp /tmp/sir-rules-coverage-mutant.XXXXXX)
jq '.edges[0].to = "missing:node"' "$repo_root/tests/fixtures/rules-corpus/v2/coverage.json" > "$coverage_mutant"
if "$repo_root/scripts/validate-rules-coverage.sh" "$coverage_mutant" >/dev/null 2>&1; then
  echo "rules coverage dangling-endpoint mutation unexpectedly passed" >&2
  rm -f "$coverage_mutant"
  exit 1
fi
rm -f "$coverage_mutant"

test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/manifest.json" | cut -d' ' -f1)" = "e5bfe82d40e72ff8b41898e408c50dd0d8fb7e05b72c6acc24baab0e3b451ddc" || { echo "retained v1 manifest changed" >&2; exit 1; }
test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/coverage.json" | cut -d' ' -f1)" = "39eecda1018c504eab7b03c60228bf155c99aa42433724655da42d9ee470d554" || { echo "retained v1 coverage changed" >&2; exit 1; }
test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/representative-application.hex" | cut -d' ' -f1)" = "f42835c3fc4691b59ff71c0b31de0e74caa21455bf9d5e7658b483e0b2da2606" || { echo "retained v1 application changed" >&2; exit 1; }

while IFS=$'\t' read -r source_path source_symbol; do
  test -f "$repo_root/$source_path" || { echo "missing rule source: $source_path" >&2; exit 1; }
  symbol_name=${source_symbol##*.}
  search_quiet "let (private )?${symbol_name}( |$)" "$repo_root/$source_path" || {
    echo "unresolved rule source symbol: $source_symbol in $source_path" >&2
    exit 1
  }
done <<< "$(jq -r '.rules[].source | select(. != null) | [.path, .symbol] | @tsv' "$repo_root/tests/fixtures/rules-corpus/v2/manifest.json")"

source_commit=$(jq -r '.sourceCommit' "$repo_root/tests/fixtures/rules-corpus/v2/manifest.json")
source_manifest="$repo_root/tests/fixtures/rules-corpus/v2/implementation-sources.json"
test "$(jq -r '.sourceCommit' "$source_manifest")" = "$source_commit" || { echo "implementation source manifest does not bind the package source commit" >&2; exit 1; }
source_digest_input=$(mktemp /tmp/sir-rules-source-digest.XXXXXX)
while IFS= read -r artifact_path; do
  actual_artifact_sha=$(git -C "$repo_root" show "$source_commit:$artifact_path" | sha256sum | cut -d' ' -f1)
  printf '%s\t%s\n' "$artifact_path" "$actual_artifact_sha" >> "$source_digest_input"
done <<< "$(jq -r '.sources[]' "$source_manifest")"
actual_sources_digest=$(sha256sum "$source_digest_input" | cut -d' ' -f1)
rm -f "$source_digest_input"
declared_sources_digest=$(sed -n 's/.*"sir-rules-implementation-sources-v1", System.Text.Encoding.UTF8.GetBytes "\([0-9a-f]\{64\}\)".*/\1/p' "$repo_root/src/SIR.Simulation/CombatRules.fs")
test "$declared_sources_digest" = "$actual_sources_digest" || { echo "implementation source manifest digest does not match pinned sources" >&2; exit 1; }
declared_package_sha=$(sed -n 's/.*"fs-gg-game-core-nupkg-sha256", System.Text.Encoding.UTF8.GetBytes "\([0-9a-f]\{64\}\)".*/\1/p' "$repo_root/src/SIR.Simulation/CombatRules.fs")
captured_package_sha=$(jq -r '.sha256' "$repo_root/docs/dependency-surface/FS.GG.Game.Core/0.13.0.json")
test "$declared_package_sha" = "$captured_package_sha" || { echo "Game.Core implementation fingerprint does not match dependency receipt" >&2; exit 1; }

copied_semantics_pattern='(baseDamage|expectedDamage).*(trace|retention)|(trace|retention).*(baseDamage|expectedDamage)'
if test "${SIR_RULES_FORCE_GREP:-0}" != 1 && command -v rg >/dev/null 2>&1; then
  copied_semantics=$(rg -n --glob '*.js' --glob '*.ts' --glob '!**/.fable*/**' "$copied_semantics_pattern" "$repo_root/src" || true)
else
  copied_semantics=$(find "$repo_root/src" -type f \( -name '*.js' -o -name '*.ts' \) ! -path '*/.fable*/*' -exec grep -EnH -- "$copied_semantics_pattern" {} + || true)
fi
if test -n "$copied_semantics"; then
  printf '%s\n' "$copied_semantics"
  echo "copied JavaScript/TypeScript combat semantics detected" >&2
  exit 1
fi

mutation_log=$(mktemp /tmp/sir-rules-mutation.XXXXXX)
trap 'rm -f "$mutation_log"' EXIT
if dotnet run --project "$project" -c Release --no-build -- --inject-rules-corpus-divergence >"$mutation_log" 2>&1; then
  echo "rules-corpus protected-subject mutation unexpectedly passed" >&2
  exit 1
fi
search_quiet 'first divergence: fixture=rules-corpus' "$mutation_log" || {
  echo "rules-corpus mutation failed without the actionable divergence diagnostic" >&2
  exit 1
}

echo "rules corpus generation, source resolution, copied-semantics, and mutation gates passed"
