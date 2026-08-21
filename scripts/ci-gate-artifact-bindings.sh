#!/usr/bin/env bash
set -euo pipefail

repo_root=${1:?ci-gate-artifact-bindings: repository root is required}
shift

for part in "$@"; do
  pointer="$repo_root/artifacts/ci/parts/$part.manifest.path"
  [[ -f "$pointer" ]] || { echo "ci-gate-artifact-bindings: missing requested manifest pointer:$part" >&2; exit 1; }
  manifest=$(<"$pointer")
  [[ -f "$repo_root/$manifest" ]] || { echo "ci-gate-artifact-bindings: missing requested manifest:$part" >&2; exit 1; }
  printf '%s=%s\n' "$part" "$(sha256sum "$repo_root/$manifest" | cut -d' ' -f1)"
done
