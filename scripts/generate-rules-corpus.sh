#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
mode=${1:---check}
fixture_dir=${SIR_RULES_FIXTURE_DIR:-"$repo_root/tests/fixtures/rules-corpus/v2"}
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"
temporary_dir=$(mktemp -d /tmp/sir-rules-corpus.XXXXXX)
trap 'rm -rf "$temporary_dir"' EXIT

prepared_args=()
if [[ "${SIR_RULES_PREPARED_PR:-0}" == 1 ]]; then prepared_args=(--no-build --no-restore); fi

dotnet run --project "$project" -c Release "${prepared_args[@]}" -- --print-rules-corpus-bundle > "$temporary_dir/bundle.json"
jq -r '.manifest' "$temporary_dir/bundle.json" > "$temporary_dir/manifest.json"
jq -r '.coverage' "$temporary_dir/bundle.json" > "$temporary_dir/coverage.json"
jq -r '.representativeApplication' "$temporary_dir/bundle.json" > "$temporary_dir/representative-application.hex"
jq -r '.specificationMarkdown' "$temporary_dir/bundle.json" > "$temporary_dir/combat-damage-001.specification.md"
jq -r '.specificationReceipt' "$temporary_dir/bundle.json" > "$temporary_dir/combat-damage-001.specification.json"

jq -e '.schemaVersion == 1 and (.rules | length == 16)' "$temporary_dir/manifest.json" >/dev/null
jq -e '.schemaVersion == 1 and .authorityBoundary.outside == "legacy"' "$temporary_dir/coverage.json" >/dev/null
"$repo_root/scripts/validate-rules-coverage.sh" "$temporary_dir/coverage.json"
jq -e '
  .schema == "sir-rule-specification-projection/v1"
  and .identity == "COMBAT-DAMAGE-001"
  and .schemaVersion == 1
  and (.sourceFingerprint | test("^[0-9a-f]{64}$"))
  and (.generatedFingerprint | test("^[0-9a-f]{64}$"))
  and .selectedSurface == "hybrid"' "$temporary_dir/combat-damage-001.specification.json" >/dev/null
grep -Eq '^<!-- sir-rule-specification/v1 -->$' "$temporary_dir/combat-damage-001.specification.md"

check_specification_fixture() {
  local projection="$fixture_dir/combat-damage-001.specification.md"
  local receipt="$fixture_dir/combat-damage-001.specification.json"
  test -f "$projection" && test -f "$receipt" || {
    echo "RULE-SPEC-PROJECTION-MISSING: regenerate the missing specification projection or receipt with --write" >&2
    return 1
  }
  test -r "$projection" && test -r "$receipt" || {
    echo "RULE-SPEC-PROJECTION-UNREADABLE: make the generated specification projection and receipt readable, then regenerate" >&2
    return 1
  }
  head -c 1 "$projection" >/dev/null 2>&1 && head -c 1 "$receipt" >/dev/null 2>&1 || {
    echo "RULE-SPEC-PROJECTION-UNREADABLE: the generated specification projection or receipt could not be read" >&2
    return 1
  }
  jq -e '
    .schema == "sir-rule-specification-projection/v1"
    and .identity == "COMBAT-DAMAGE-001"
    and (.sourceFingerprint | type == "string" and test("^[0-9a-f]{64}$"))
    and (.generatedFingerprint | type == "string" and test("^[0-9a-f]{64}$"))' "$receipt" >/dev/null 2>&1 || {
    echo "RULE-SPEC-PROJECTION-MALFORMED: regenerate the invalid specification receipt with --write" >&2
      return 1
    }
  grep -Eq '^<!-- sir-rule-specification/v1 -->$' "$projection" || {
    echo "RULE-SPEC-PROJECTION-MALFORMED: regenerate the invalid specification Markdown with --write" >&2
    return 1
  }
  local expected_source actual_source
  expected_source=$(jq -r '.sourceFingerprint' "$temporary_dir/combat-damage-001.specification.json")
  actual_source=$(jq -r '.sourceFingerprint' "$receipt")
  test "$actual_source" = "$expected_source" || {
    echo "RULE-SPEC-PROJECTION-STALE-SOURCE: source fingerprint changed; regenerate the specification artifacts with --write" >&2
    return 1
  }
  cmp -s "$temporary_dir/combat-damage-001.specification.md" "$projection" \
    && cmp -s "$temporary_dir/combat-damage-001.specification.json" "$receipt" || {
      echo "RULE-SPEC-PROJECTION-DIRECT-EDIT: discard the direct edit and regenerate the specification artifacts with --write" >&2
      return 1
    }
}

case "$mode" in
  --write)
    mkdir -p "$fixture_dir"
    cp "$temporary_dir/manifest.json" "$fixture_dir/manifest.json"
    cp "$temporary_dir/coverage.json" "$fixture_dir/coverage.json"
    cp "$temporary_dir/representative-application.hex" "$fixture_dir/representative-application.hex"
    cp "$temporary_dir/combat-damage-001.specification.md" "$fixture_dir/combat-damage-001.specification.md"
    cp "$temporary_dir/combat-damage-001.specification.json" "$fixture_dir/combat-damage-001.specification.json"
    ;;
  --check)
    cmp "$temporary_dir/manifest.json" "$fixture_dir/manifest.json"
    cmp "$temporary_dir/coverage.json" "$fixture_dir/coverage.json"
    cmp "$temporary_dir/representative-application.hex" "$fixture_dir/representative-application.hex"
    check_specification_fixture
    ;;
  *)
    echo "usage: scripts/generate-rules-corpus.sh [--check|--write]" >&2
    exit 2
    ;;
esac
