#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
mode=${1:---check}
parity=true
if [[ ${2:-} == "--skip-parity" || ${1:-} == "--skip-parity" ]]; then parity=false; fi
if [[ $mode == "--skip-parity" ]]; then mode=--check; fi
if [[ $mode != "--check" && $mode != "--write" ]]; then
  echo "usage: scripts/capture-typed-kernel-p0.sh [--check|--write] [--skip-parity]" >&2
  exit 2
fi

selection=${SIR_TYPED_KERNEL_P0_SELECTION:-"$repo_root/tests/fixtures/typed-kernel-p0/selection.json"}
baseline=${SIR_TYPED_KERNEL_P0_BASELINE:-"$repo_root/tests/fixtures/typed-kernel-p0/baseline.json"}
corpus="$repo_root/tests/fixtures/rules-corpus/v2/manifest.json"
task_tmp=$(mktemp -d /tmp/sir-typed-kernel-p0.XXXXXX)
trap 'rm -rf -- "$task_tmp"' EXIT

jq -e '.schemaVersion == 1 and .roadmapMilestone == "P0"' "$selection" >/dev/null

missing_classes=$(jq -r '
  (.requiredSurfaceClasses - ([.surfaces[].class] | unique))[]?
' "$selection")
if [[ -n $missing_classes ]]; then
  echo "typed-kernel P0 selection omitted required surface class: $missing_classes" >&2
  exit 1
fi

while IFS=$'\t' read -r id expected_kind; do
  actual_kind=$(jq -r --arg id "$id" '.rules[] | select(.id == $id) | .kind' "$corpus")
  if [[ -z $actual_kind ]]; then
    echo "typed-kernel P0 canonical selection is absent from the live registry: $id" >&2
    exit 1
  fi
  if [[ $actual_kind != "$expected_kind" ]]; then
    echo "typed-kernel P0 kind mismatch for $id: expected $expected_kind, found $actual_kind" >&2
    exit 1
  fi
done < <(jq -r '.surfaces[] | select(.authority == "canonical-registry") | [.id, .expectedKind] | @tsv' "$selection")

predicate_source=$(jq -r '.surfaces[] | select(.class == "predicate-fixture") | .evidence' "$selection")
supersession_source=$(jq -r '.surfaces[] | select(.class == "supersession-fixture") | .evidence' "$selection")
rg -F 'PredicateSemantics' "$repo_root/$predicate_source" >/dev/null || { echo "typed-kernel P0 predicate fixture is no longer exercised" >&2; exit 1; }
rg -F 'Supersedes = [ replacement ]' "$repo_root/$supersession_source" >/dev/null || { echo "typed-kernel P0 supersession fixture is no longer exercised" >&2; exit 1; }

artifacts_json="$task_tmp/artifacts.jsonl"
: > "$artifacts_json"
while IFS= read -r path; do
  if [[ ! -f "$repo_root/$path" ]]; then
    echo "typed-kernel P0 artifact is missing: $path" >&2
    exit 1
  fi
  digest=$(sha256sum "$repo_root/$path" | awk '{print $1}')
  jq -cn --arg path "$path" --arg digest "$digest" '{path:$path,sha256:$digest}' >> "$artifacts_json"
done < <(jq -r '.artifacts[]' "$selection")

jq -S \
  --slurpfile artifacts "$artifacts_json" \
  --slurpfile selection "$selection" \
  --slurpfile corpus "$corpus" \
  '{
    schemaVersion: 1,
    roadmapMilestone: "P0",
    selectionSha256: $selection[0]._selectionSha256,
    corpusIdentity: {
      engineIdentity: $corpus[0].engineIdentity,
      compatibilityProfile: $corpus[0].compatibilityProfile,
      packageVersion: $corpus[0].packageVersion,
      sourceCommit: $corpus[0].sourceCommit,
      implementationDigest: $corpus[0].implementationDigest,
      semanticDigest: $corpus[0].semanticDigest,
      manifestDigest: $corpus[0].manifestDigest
    },
    surfaces: $selection[0].surfaces,
    artifacts: $artifacts
  }' "$selection" > "$task_tmp/baseline.unbound.json"

selection_digest=$(sha256sum "$selection" | awk '{print $1}')
jq -S --arg digest "$selection_digest" '.selectionSha256 = $digest' "$task_tmp/baseline.unbound.json" > "$task_tmp/baseline.json"

if [[ $mode == "--write" ]]; then
  cp "$task_tmp/baseline.json" "$baseline"
else
  cmp "$task_tmp/baseline.json" "$baseline" || {
    echo "typed-kernel P0 baseline is stale; inspect the semantic/artifact diff and run --write only when accepted" >&2
    exit 1
  }
fi

"$repo_root/scripts/generate-rules-corpus.sh" --check

if [[ $parity == true ]]; then
  "$repo_root/scripts/test-conformance.sh" --domain-only
fi

printf 'Typed-kernel P0 baseline verified: %d surfaces, %d content-addressed artifacts, parity=%s.\n' \
  "$(jq '.surfaces | length' "$task_tmp/baseline.json")" \
  "$(jq '.artifacts | length' "$task_tmp/baseline.json")" \
  "$parity"
