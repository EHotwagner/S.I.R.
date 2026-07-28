#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
site_output="$repo_root/artifacts/site"
client_output="$repo_root/artifacts/client"

cd "$repo_root"

dotnet tool restore
dotnet restore SIR.slnx --locked-mode
dotnet build SIR.slnx -c Release --no-restore
./scripts/build-client.sh

if [[ -d "$repo_root/.fsdocs" ]]; then
  rm -rf -- "$repo_root/.fsdocs"
fi

dotnet fsdocs build \
  --clean \
  --eval \
  --strict \
  --projects \
    src/SIR.Domain/SIR.Domain.fsproj \
    src/SIR.Simulation/SIR.Simulation.fsproj \
    src/SIR.Client/SIR.Client.fsproj \
  --output "$site_output" \
  --properties Configuration=Release

mkdir -p "$site_output/content/sir-client/v1"
cp -R "$client_output/content/sir-client/v1/." \
  "$site_output/content/sir-client/v1/"

node scripts/generate-docs-manifest.mjs \
  "$site_output/content/sir-client/v1"
node scripts/verify-docs.mjs "$site_output"
node scripts/smoke-docs.mjs "$site_output"
