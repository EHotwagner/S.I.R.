#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
fable_output="$repo_root/src/SIR.Client.Web/.fable"

cd "$repo_root"

dotnet fable src/SIR.Replay.Web/SIR.Replay.Web.fsproj \
  --outDir "$fable_output" \
  --noCache

npx vite build --config src/SIR.Client.Web/vite.config.js
node scripts/generate-publication-manifest.mjs artifacts/client
