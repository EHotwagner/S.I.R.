#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
site_root=${1:-"$repo_root/artifacts/site"}
task_tmp=$(mktemp -d)
trap 'rm -rf -- "$task_tmp"' EXIT

cp -R "$site_root/." "$task_tmp/"
app_bundle="$task_tmp/content/sir-client/v1/app.js"
[[ -f "$app_bundle" ]] || { echo "Generated-site Rules owner mutation requires $app_bundle" >&2; exit 2; }

owner_count=$(rg -o 'RulesWorkbenchView-[A-Za-z0-9_-]+\.js' "$app_bundle" | wc -l)
[[ "$owner_count" -ge 1 ]] || {
  echo "Generated-site app bundle did not expose a Rules workbench chunk owner" >&2
  exit 2
}
perl -0pi -e 's/RulesWorkbenchView-[A-Za-z0-9_-]+\.js/RulesWorkbenchView-missing.js/g' "$app_bundle"

if diagnostic=$(node "$repo_root/scripts/smoke-docs.mjs" "$task_tmp" 2>&1); then
  echo "Missing generated-site Rules workbench owner survived its player journey." >&2
  exit 1
fi
if ! grep -q "The generated Rules workbench failed closed:" <<<"$diagnostic" ||
   grep -q "Timed out waiting for deferred Rules workbench owner" <<<"$diagnostic"; then
  echo "Missing generated-site Rules owner failed outside its canonical owner: $diagnostic" >&2
  exit 1
fi

echo "Generated-site Rules owner mutation passed: a missing deferred chunk reds at the canonical load-failure owner."
