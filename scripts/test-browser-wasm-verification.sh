#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
trap 'rm -rf -- "$task_tmp"' EXIT

project="$repo_root/spikes/browser-wasm-verification/BrowserWasmVerificationSpike.fsproj"
artifact="$repo_root/spikes/browser-wasm-verification/artifact.b64"

dotnet restore "$project" --locked-mode
dotnet build "$project" --configuration Release --no-restore

dotnet run \
  --project "$project" \
  --configuration Release \
  --no-build \
  --no-restore \
  -- "$artifact" >"$task_tmp/wasmtime.json"

node "$repo_root/spikes/browser-wasm-verification/browser-host.mjs" \
  "$artifact" >"$task_tmp/browser.json"

node "$repo_root/spikes/browser-wasm-verification/evaluate.mjs" \
  "$task_tmp/wasmtime.json" \
  "$task_tmp/browser.json"
