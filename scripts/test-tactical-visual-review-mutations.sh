#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
trap 'rm -rf "$task_tmp"' EXIT

cp -R "$repo_root/artifacts/client" "$task_tmp/client"
cp -R "$repo_root/docs/assets/tactical-visual-system-review" "$task_tmp/review"

sed -i '0,/#10161d/s//#ff00ff/' "$task_tmp/client/content/sir-client/v1/styles.css"
if node "$repo_root/scripts/test-tactical-visual-review.mjs" --client-root "$task_tmp/client" --review-root "$task_tmp/review" >/dev/null 2>&1; then
  echo "Protected stylesheet mutation survived tactical visual review." >&2
  exit 1
fi

cp "$repo_root/artifacts/client/content/sir-client/v1/styles.css" "$task_tmp/client/content/sir-client/v1/styles.css"
jq '.densityScenes[0].workload.plannedRouteUnits = 1' "$task_tmp/review/manifest.json" > "$task_tmp/manifest.json"
mv "$task_tmp/manifest.json" "$task_tmp/review/manifest.json"
if node "$repo_root/scripts/test-tactical-visual-review.mjs" --client-root "$task_tmp/client" --review-root "$task_tmp/review" >/dev/null 2>&1; then
  echo "Production route-workload mutation survived tactical visual review." >&2
  exit 1
fi

echo "Tactical visual review mutations passed: stylesheet and production workload subjects fail closed."
