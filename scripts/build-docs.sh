#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
site_output="$repo_root/artifacts/site"
client_output="$repo_root/artifacts/client"
reuse_build_receipt=""
reuse_conformance_receipt=""
reuse_build_owner="scripts/qualify-production.sh"
prepared_pr=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --reuse-build-receipt)
      [[ $# -ge 2 ]] || { echo "build-docs: --reuse-build-receipt requires a path" >&2; exit 2; }
      reuse_build_receipt=$2
      shift 2
      ;;
    --reuse-conformance-receipt)
      [[ $# -ge 2 ]] || { echo "build-docs: --reuse-conformance-receipt requires a path" >&2; exit 2; }
      reuse_conformance_receipt=$2
      shift 2
      ;;
    --reuse-build-owner)
      [[ $# -ge 2 ]] || { echo "build-docs: --reuse-build-owner requires a command" >&2; exit 2; }
      reuse_build_owner=$2
      shift 2
      ;;
    --prepared-pr)
      prepared_pr=true
      shift
      ;;
    *)
      echo "build-docs: unknown argument: $1" >&2
      exit 2
      ;;
  esac
done

cd "$repo_root"

node scripts/verify-fable-client-baseline.mjs

if [[ -n "$reuse_build_receipt" ]]; then
  if [[ "$prepared_pr" != true ]]; then
    [[ -n "$reuse_conformance_receipt" ]] || { echo "build-docs: production build reuse requires a conformance receipt" >&2; exit 2; }
  fi
  node scripts/production-build-receipt.mjs verify \
    --owner-command "$reuse_build_owner" \
    --receipt "$reuse_build_receipt"
  if [[ -n "$reuse_conformance_receipt" ]]; then
    node scripts/production-build-receipt.mjs verify \
      --owner-command scripts/test-conformance.sh \
      --receipt "$reuse_conformance_receipt"
  fi
  if [[ "$prepared_pr" != true ]]; then
    dotnet build src/SIR.Client/SIR.Client.fsproj -c Release --no-restore
  fi
else
  dotnet tool restore
  dotnet restore SIR.slnx --locked-mode
  dotnet build SIR.slnx -c Release --no-restore
  ./scripts/build-client.sh
fi
if [[ -z "$reuse_conformance_receipt" && "$prepared_pr" != true ]]; then
  node scripts/test-map-editor-qualification.mjs "$client_output"
  node scripts/test-planning-workspace-m5-qualification.mjs
  node scripts/test-simulator-workspace-m6-qualification.mjs
  node scripts/test-review-workspace-m7-qualification.mjs
  node scripts/test-timeline-supporting-panels-m8-qualification.mjs
  node scripts/test-persistent-workspace-m9-acceptance.mjs "$client_output"
fi

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
    src/SIR.Wasm/SIR.Wasm.fsproj \
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

node scripts/generate-in-app-docs.mjs "$site_output"
node scripts/test-in-app-docs.mjs "$site_output"
node scripts/generate-docs-manifest.mjs \
  "$site_output/content/sir-client/v1"
node scripts/generate-publication-manifest.mjs "$site_output"
node scripts/verify-docs.mjs "$site_output"
node scripts/test-docs-experience.mjs "$site_output"
node scripts/smoke-docs.mjs "$site_output"
bash scripts/test-smoke-docs-rules-owner-mutation.sh "$site_output"
node scripts/test-docs-accessibility.mjs "$site_output"
