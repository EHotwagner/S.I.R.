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

# FS-GG/FS.GG.SDD#857: 1.0.1's analyze path currently emits the
# pre-evidence projection into the same work-model path that verify/ship use for
# the evidence-enriched projection. Keep that producer defect explicit and
# bounded: analyze must expose the known alternation, then verify must restore
# the exact canonical final model and ship must preserve it.
dotnet fsgg-sdd analyze --work 184-scenario-catalog --text
analyze_model=$(sha256sum "$model" | cut -d' ' -f1)
if [[ "$analyze_model" == "$before_model" ]]; then
  echo "FS.GG.SDD#857 no longer reproduces; remove the consumer workaround and require full analyze -> verify -> ship stability." >&2
  exit 1
fi

# The first analyze diagnoses its own just-rewritten view as stale; the second
# reaches implementationReady over that pre-evidence projection.
dotnet fsgg-sdd analyze --work 184-scenario-catalog --text

dotnet fsgg-sdd verify --work 184-scenario-catalog --text
verify_model=$(sha256sum "$model" | cut -d' ' -f1)
if [[ "$verify_model" != "$before_model" ]]; then
  printf 'item 184 verify did not restore the canonical work model: %s -> %s\n' "$before_model" "$verify_model" >&2
  exit 1
fi

dotnet fsgg-sdd ship --work 184-scenario-catalog --text
ship_model=$(sha256sum "$model" | cut -d' ' -f1)
if [[ "$ship_model" != "$before_model" ]]; then
  printf 'item 184 ship rewrote the verified canonical work model: %s -> %s\n' "$before_model" "$ship_model" >&2
  exit 1
fi

after_views=$(snapshot)
if [[ "$after_views" != "$before_views" ]]; then
  echo "item 184 analyze -> verify -> ship rewrote committed readiness views" >&2
  diff -u <(printf '%s\n' "$before_views") <(printf '%s\n' "$after_views") >&2 || true
  exit 1
fi

echo "Item 184 SDD final-projection gate passed: FS.GG.SDD#857 remained bounded and verify -> ship restored/preserved canonical readiness bytes."
