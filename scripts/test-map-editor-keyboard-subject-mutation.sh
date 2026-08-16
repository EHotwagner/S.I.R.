#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
subject="$repo_root/src/SIR.Client.Web/BrowserInfrastructure.fs"
cp "$subject" "$task_tmp/BrowserInfrastructure.fs"
restore_subject() {
  cp "$task_tmp/BrowserInfrastructure.fs" "$subject"
  rm -rf "$task_tmp"
}
trap restore_subject EXIT

sed -i \
  's/allowModifiedShortcut || not (isNativeInteractiveTarget target)/allowModifiedShortcut || isNativeInteractiveTarget target/' \
  "$subject"
if node "$repo_root/scripts/test-map-editor-qualification.mjs" "$repo_root/artifacts/client" >"$task_tmp/output.log" 2>&1; then
  echo "Map-editor native keyboard subject mutation unexpectedly passed." >&2
  exit 1
fi
if ! grep -q "typed global-keyboard owner no longer excludes native/contenteditable targets" "$task_tmp/output.log"; then
  cat "$task_tmp/output.log" >&2
  echo "Map-editor native keyboard mutation failed for the wrong reason." >&2
  exit 1
fi

echo "Map-editor native/contenteditable keyboard subject mutation failed closed."
