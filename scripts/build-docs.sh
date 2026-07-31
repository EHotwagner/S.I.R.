#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
site_output="$repo_root/artifacts/site"
client_output="$repo_root/artifacts/client"

cd "$repo_root"

node scripts/verify-fable-client-baseline.mjs

dotnet tool restore
dotnet restore SIR.slnx --locked-mode
dotnet build SIR.slnx -c Release --no-restore
./scripts/build-client.sh
node scripts/test-map-editor-qualification.mjs "$client_output"
node scripts/test-planning-workspace-m5-qualification.mjs
node scripts/test-simulator-workspace-m6-qualification.mjs
node scripts/test-review-workspace-m7-qualification.mjs
node scripts/test-timeline-supporting-panels-m8-qualification.mjs

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
    src/SIR.Match/SIR.Match.fsproj \
    src/SIR.Client/SIR.Client.fsproj \
  --output "$site_output" \
  --properties Configuration=Release

node scripts/prune-docs-navigation.mjs "$site_output"

mkdir -p "$site_output/content/sir-client/v1"
cp -R "$client_output/content/sir-client/v1/." \
  "$site_output/content/sir-client/v1/"
mkdir -p "$site_output/engines"
cp -R "$client_output/engines/." "$site_output/engines/"

node scripts/generate-docs-manifest.mjs \
  "$site_output/content/sir-client/v1"
node scripts/generate-publication-manifest.mjs "$site_output"
node scripts/verify-docs.mjs "$site_output"
node scripts/test-docs-experience.mjs "$site_output"
node scripts/smoke-docs.mjs "$site_output"
node scripts/test-docs-accessibility.mjs "$site_output"
