#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
temporary=$(mktemp -d /tmp/sir-ci-gate-artifact-isolation.XXXXXX)
trap 'rm -rf -- "$temporary"' EXIT
mkdir -p "$temporary/artifacts/ci/parts" "$temporary/manifests"

printf 'native-manifest\n' >"$temporary/manifests/native.json"
printf 'fable-extra-manifest\n' >"$temporary/manifests/fable.json"
printf 'manifests/native.json\n' >"$temporary/artifacts/ci/parts/native.manifest.path"
printf 'manifests/fable.json\n' >"$temporary/artifacts/ci/parts/fable.manifest.path"

native_digest=$(sha256sum "$temporary/manifests/native.json" | cut -d' ' -f1)
bindings=$("$repo_root/scripts/ci-gate-artifact-bindings.sh" "$temporary" native)
[[ "$bindings" == "native=$native_digest" ]] || {
  echo "subject-scoped artifact binding included an unrequested manifest: $bindings" >&2
  exit 1
}

if "$repo_root/scripts/ci-gate-artifact-bindings.sh" "$temporary" missing >"$temporary/missing.log" 2>&1; then
  echo "missing requested artifact manifest unexpectedly passed" >&2
  exit 1
fi
grep -F 'missing requested manifest pointer:missing' "$temporary/missing.log" >/dev/null

echo "CI gate artifact bindings remain subject-scoped with an extra manifest present and fail closed for a missing requested subject."
