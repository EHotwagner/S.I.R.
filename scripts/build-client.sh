#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
fable_output="$repo_root/src/SIR.Client.Web/.fable"
rules_fable_output="$repo_root/src/SIR.Client.Web/.fable-rules"

cd "$repo_root"

dotnet fable src/SIR.Replay.Web/SIR.Replay.Web.fsproj \
  --outDir "$fable_output" \
  --define SIR_WEB_CLIENT \
  --noCache

dotnet fable src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj \
  --outDir "$rules_fable_output" \
  --noCache
cp "$rules_fable_output/RulesExplorer.js" "$fable_output/RulesExplorer.js"
cp "$rules_fable_output/SIR.Domain/Rules.js" "$fable_output/SIR.Domain/RulesAuthoring.js"
cp "$rules_fable_output/SIR.Simulation/CombatRules.js" "$fable_output/SIR.Simulation/CombatRulesAuthoring.js"
sed -i 's#./SIR.Simulation/CombatRules.js#./SIR.Simulation/CombatRulesAuthoring.js#' "$fable_output/RulesExplorer.js"
sed -i 's#./SIR.Domain/Rules.js#./SIR.Domain/RulesAuthoring.js#' "$fable_output/RulesExplorer.js"
sed -i 's#../SIR.Domain/Rules.js#../SIR.Domain/RulesAuthoring.js#' "$fable_output/SIR.Simulation/CombatRulesAuthoring.js"
sed -i '$a export default ExecutableRulesPanel;' "$fable_output/RulesExplorer.js"

npx vite build --config src/SIR.Client.Web/vite.config.js
node scripts/generate-publication-manifest.mjs artifacts/client
