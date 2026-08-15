#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
cd "$repo_root"

model=readiness/184-scenario-catalog/work-model.json
snapshot() {
  git ls-files -z readiness/184-scenario-catalog \
    | sort -z \
    | xargs -0 sha256sum
}
before_model=$(sha256sum "$model" | cut -d' ' -f1)
before_views=$(snapshot)

for stage in analyze verify ship; do
  dotnet fsgg-sdd "$stage" --work 184-scenario-catalog --text
  after_model=$(sha256sum "$model" | cut -d' ' -f1)
  if [[ "$after_model" != "$before_model" ]]; then
    printf 'item 184 SDD %s rewrote the canonical work model: %s -> %s\n' "$stage" "$before_model" "$after_model" >&2
    git diff --stat -- "$model" >&2 || true
    exit 1
  fi
done

after_views=$(snapshot)
if [[ "$after_views" != "$before_views" ]]; then
  echo "item 184 analyze -> verify -> ship rewrote committed readiness views" >&2
  diff -u <(printf '%s\n' "$before_views") <(printf '%s\n' "$after_views") >&2 || true
  exit 1
fi

echo "Item 184 SDD byte-stability gate passed: analyze -> verify -> ship preserved the canonical work model and readiness views."
