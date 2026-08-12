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
normalize_implementation_source() {
  local artifact_path=$1
  local input_path=$2
  if test "$artifact_path" = "src/SIR.Simulation/CombatRules.fs"; then
    sed -E \
      -e 's/(Commit = ")[0-9a-f]{40}(" })/\1<SOURCE_COMMIT>\2/' \
      -e 's/(GetBytes ")[0-9a-f]{64}(" \])/\1<IMPLEMENTATION_DIGEST>\2/' \
      -e 's/(FS.GG.Game.Core@0\.13\.0" ")[0-9a-f]{40}(" implementationArtifacts)/\1<SOURCE_COMMIT>\2/' \
      "$input_path"
  else
    command cat "$input_path"
  fi
}

source_matches_pin() {
  local artifact_path=$1
  local current_path=$2
  local pinned_path
  local current_normalized
  local pinned_normalized
  pinned_path=$(mktemp /tmp/sir-rules-pinned-source.XXXXXX)
  current_normalized=$(mktemp /tmp/sir-rules-current-normalized.XXXXXX)
  pinned_normalized=$(mktemp /tmp/sir-rules-pinned-normalized.XXXXXX)
  git -C "$repo_root" show "$source_commit:$artifact_path" > "$pinned_path"
  normalize_implementation_source "$artifact_path" "$current_path" > "$current_normalized"
  normalize_implementation_source "$artifact_path" "$pinned_path" > "$pinned_normalized"
  cmp -s "$current_normalized" "$pinned_normalized"
  local result=$?
  rm -f "$pinned_path" "$current_normalized" "$pinned_normalized"
  return "$result"
}

while IFS= read -r artifact_path; do
  actual_artifact_sha=$(git -C "$repo_root" show "$source_commit:$artifact_path" | sha256sum | cut -d' ' -f1)
  printf '%s\t%s\n' "$artifact_path" "$actual_artifact_sha" >> "$source_digest_input"
  source_matches_pin "$artifact_path" "$repo_root/$artifact_path" || {
    echo "current implementation source differs from package pin: $artifact_path" >&2
    rm -f "$source_digest_input"
    exit 1
  }
done <<< "$(jq -r '.sources[]' "$source_manifest")"

app_mutant=$(mktemp /tmp/sir-rules-app-mutant.XXXXXX)
combat_mutant=$(mktemp /tmp/sir-rules-combat-mutant.XXXXXX)
combat_metadata_mutant=$(mktemp /tmp/sir-rules-combat-metadata-mutant.XXXXXX)
cp "$repo_root/src/SIR.Client.Web/App.fs" "$app_mutant"
printf '\n// implementation identity subject mutation\n' >> "$app_mutant"
if source_matches_pin "src/SIR.Client.Web/App.fs" "$app_mutant"; then
  echo "App.fs implementation source mutation unexpectedly passed" >&2
  rm -f "$app_mutant" "$combat_mutant" "$combat_metadata_mutant" "$source_digest_input"
  exit 1
fi
sed '0,/module CombatRules =/s//module CombatRules = \/\/ implementation identity subject mutation/' \
  "$repo_root/src/SIR.Simulation/CombatRules.fs" > "$combat_mutant"
if source_matches_pin "src/SIR.Simulation/CombatRules.fs" "$combat_mutant"; then
  echo "CombatRules.fs non-metadata source mutation unexpectedly passed" >&2
  rm -f "$app_mutant" "$combat_mutant" "$combat_metadata_mutant" "$source_digest_input"
  exit 1
fi
sed -E 's/(Commit = ")[0-9a-f]{40}(" })/\10000000000000000000000000000000000000000\2/' \
  "$repo_root/src/SIR.Simulation/CombatRules.fs" > "$combat_metadata_mutant"
source_matches_pin "src/SIR.Simulation/CombatRules.fs" "$combat_metadata_mutant" || {
  echo "CombatRules.fs metadata-only source rebind was not normalized" >&2
  rm -f "$app_mutant" "$combat_mutant" "$combat_metadata_mutant" "$source_digest_input"
  exit 1
}
rm -f "$app_mutant" "$combat_mutant" "$combat_metadata_mutant"
printf 'package\t%s\nalgorithm\t%s\n' "$(jq -r '.packageSha256' "$source_manifest")" "$(jq -r '.algorithmFingerprint' "$source_manifest")" >> "$source_digest_input"
actual_sources_digest=$(sha256sum "$source_digest_input" | cut -d' ' -f1)
identity_mutant=$(mktemp /tmp/sir-rules-source-digest-mutant.XXXXXX)
sed 's#^src/SIR.Domain/Rules.fs\t[0-9a-f]\{64\}$#src/SIR.Domain/Rules.fs\t0000000000000000000000000000000000000000000000000000000000000000#' \
  "$source_digest_input" > "$identity_mutant"
mutated_sources_digest=$(sha256sum "$identity_mutant" | cut -d' ' -f1)
rm -f "$identity_mutant"
rm -f "$source_digest_input"
declared_sources_digest=$(sed -n 's/.*"implementation", System.Text.Encoding.UTF8.GetBytes "\([0-9a-f]\{64\}\)".*/\1/p' "$repo_root/src/SIR.Simulation/CombatRules.fs")
test "$declared_sources_digest" = "$actual_sources_digest" || { echo "implementation source manifest digest does not match pinned sources" >&2; exit 1; }
test "$declared_sources_digest" != "$mutated_sources_digest" || { echo "implementation identity source mutation unexpectedly passed" >&2; exit 1; }
declared_package_sha=$(jq -r '.packageSha256' "$source_manifest")
captured_package_sha=$(jq -r '.sha256' "$repo_root/docs/dependency-surface/FS.GG.Game.Core/0.13.0.json")
test "$declared_package_sha" = "$captured_package_sha" || { echo "Game.Core implementation fingerprint does not match dependency receipt" >&2; exit 1; }
test "$(jq -r '.algorithmFingerprint' "$source_manifest")" = "FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover" || { echo "Game.Core algorithm fingerprint changed" >&2; exit 1; }

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
