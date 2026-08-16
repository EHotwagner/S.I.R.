#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
fable_output="$repo_root/src/SIR.Client.Web/.fable"
rules_fable_output="$repo_root/src/SIR.Client.Web/.fable-rules"

cd "$repo_root"

./scripts/build-client-fable-targets.sh "$fable_output" "$rules_fable_output"

cp "$rules_fable_output/RulesExplorer.js" "$fable_output/RulesExplorer.js"
cp "$rules_fable_output/SIR.Domain/Rules.js" "$fable_output/SIR.Domain/RulesAuthoring.js"
cp "$rules_fable_output/SIR.Simulation/CombatRules.js" "$fable_output/SIR.Simulation/CombatRulesAuthoring.js"
# The production-symbol compile consumes the package's supported Fable source
# view but may leave those package modules as sources. Retain the JS emitted by
# the authoring compile so direct production replay qualification loads the
# exact same package-only dependency graph as the bundled worker.
cp "$rules_fable_output"/fable_modules/FS.GG.Game.Core.0.13.0/*.js \
  "$fable_output/fable_modules/FS.GG.Game.Core.0.13.0/"
sed -i 's#./SIR.Simulation/CombatRules.js#./SIR.Simulation/CombatRulesAuthoring.js#' "$fable_output/RulesExplorer.js"
sed -i 's#./SIR.Domain/Rules.js#./SIR.Domain/RulesAuthoring.js#' "$fable_output/RulesExplorer.js"
sed -i 's#../SIR.Domain/Rules.js#../SIR.Domain/RulesAuthoring.js#' "$fable_output/SIR.Simulation/CombatRulesAuthoring.js"
sed -i '$a export default DeferredDataPanel;' "$fable_output/RulesExplorer.js"

npx vite build --config src/SIR.Client.Web/vite.config.js
node scripts/generate-publication-manifest.mjs artifacts/client
