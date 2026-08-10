#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
cd "$repo_root"

mkdir -p artifacts/test-results
dotnet tool restore
npm ci --ignore-scripts
./scripts/test-conformance.sh
dotnet restore SIR.slnx --locked-mode
npm run verify:scaffold
dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release --no-restore -o artifacts/publish

if [[ -z "${PLAYWRIGHT_EXECUTABLE_PATH:-}" ]]; then
  npx playwright install --only-shell chromium
fi

npm run test:browser
dotnet fsgg-sdd evidence --work 138-sir-fable-game-scaffold --sync-observed-run artifacts/test-results/browser.junit.xml --root . --text
dotnet fsgg-sdd verify --work 138-sir-fable-game-scaffold --root . --text
dotnet fsgg-sdd doctor --root . --text
