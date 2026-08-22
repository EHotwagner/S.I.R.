#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
manifest="$repo_root/docs/assets/map-editor-review/manifest.json"
task_tmp="$(mktemp -d)"

restore() {
  cp "$task_tmp/manifest.json" "$manifest"
  rm -rf -- "$task_tmp"
}
trap restore EXIT

cp "$manifest" "$task_tmp/manifest.json"

node - "$manifest" <<'NODE'
const fs = require("node:fs");
const path = process.argv[2];
const manifest = JSON.parse(fs.readFileSync(path, "utf8"));
manifest.productionBundleSha256 = "0".repeat(64);
fs.writeFileSync(path, `${JSON.stringify(manifest, null, 2)}\n`);
NODE

if node "$repo_root/scripts/test-map-editor-qualification.mjs" "$repo_root/artifacts/client" >"$task_tmp/stale.log" 2>&1; then
  echo "production-review freshness mutation unexpectedly passed" >&2
  exit 1
fi

grep -F "review artifacts were not regenerated from the current production bundle" "$task_tmp/stale.log" >/dev/null || {
  echo "production-review freshness mutation failed for the wrong reason" >&2
  cat "$task_tmp/stale.log" >&2
  exit 1
}

restore
trap - EXIT
node "$repo_root/scripts/test-map-editor-qualification.mjs" "$repo_root/artifacts/client" >/dev/null
echo "Production-review freshness mutation rejected stale bundle binding and accepted the restored exact bundle."
