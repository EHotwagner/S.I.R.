#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"

"$repo_root/scripts/generate-rules-corpus.sh" --check

while IFS=$'\t' read -r source_path source_symbol; do
  test -f "$repo_root/$source_path" || { echo "missing rule source: $source_path" >&2; exit 1; }
  symbol_name=${source_symbol##*.}
  rg -q "let (private )?${symbol_name}( |$)" "$repo_root/$source_path" || {
    echo "unresolved rule source symbol: $source_symbol in $source_path" >&2
    exit 1
  }
done <<< "$(jq -r '.rules[].source | select(. != null) | [.path, .symbol] | @tsv' "$repo_root/tests/fixtures/rules-corpus/v1/manifest.json")"

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
