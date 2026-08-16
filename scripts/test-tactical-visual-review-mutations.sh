#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
projection_source="$repo_root/src/SIR.Client/TacticalSceneProjection.fs"
samples_source="$repo_root/src/SIR.Client/ExperienceSamples.fs"
simulator_source="$repo_root/src/SIR.Client/MapEditorSimulator.fs"
review_generator="$repo_root/scripts/generate-tactical-visual-review.mjs"
cp "$projection_source" "$task_tmp/TacticalSceneProjection.fs"
cp "$samples_source" "$task_tmp/ExperienceSamples.fs"
cp "$simulator_source" "$task_tmp/MapEditorSimulator.fs"
cp "$review_generator" "$task_tmp/generate-tactical-visual-review.mjs"
restore_sources() {
  cp "$task_tmp/TacticalSceneProjection.fs" "$projection_source"
  cp "$task_tmp/ExperienceSamples.fs" "$samples_source"
  cp "$task_tmp/MapEditorSimulator.fs" "$simulator_source"
  cp "$task_tmp/generate-tactical-visual-review.mjs" "$review_generator"
}
trap 'restore_sources; rm -rf "$task_tmp"' EXIT

cp -R "$repo_root/artifacts/client" "$task_tmp/client"
cp -R "$repo_root/docs/assets/tactical-visual-system-review" "$task_tmp/review"

sed -i '0,/#10161d/s//#ff00ff/' "$task_tmp/client/content/sir-client/v1/styles.css"
if node "$repo_root/scripts/test-tactical-visual-review.mjs" --client-root "$task_tmp/client" --review-root "$task_tmp/review" >/dev/null 2>&1; then
  echo "Protected stylesheet mutation survived tactical visual review." >&2
  exit 1
fi

cp "$repo_root/artifacts/client/content/sir-client/v1/styles.css" "$task_tmp/client/content/sir-client/v1/styles.css"
sed -i 's/| CommittedEvent -> CommittedEffect/| CommittedEvent -> AcceptedEffect/' "$projection_source"
if dotnet run --project "$repo_root/tests/SIR.Client.Tests/SIR.Client.Tests.fsproj" -c Release --no-restore >/dev/null 2>&1; then
  echo "Production lifecycle projection mutation survived its focused owner." >&2
  exit 1
fi
cp "$task_tmp/TacticalSceneProjection.fs" "$projection_source"

sed -i 's/for index in 0 \.\. unitCount - 1 do/for index in 0 .. unitCount - 2 do/' "$samples_source"
if dotnet run --project "$repo_root/tests/SIR.Client.Tests/SIR.Client.Tests.fsproj" -c Release --no-restore >/dev/null 2>&1; then
  echo "Production density workload mutation survived its focused owner." >&2
  exit 1
fi
cp "$task_tmp/ExperienceSamples.fs" "$samples_source"

sed -i 's/if index % 2 = 0 then " blue " else " red "/if index % 2 = 0 then " blue " else " blue "/' "$samples_source"
if dotnet run --project "$repo_root/tests/SIR.Client.Tests/SIR.Client.Tests.fsproj" -c Release --no-restore >/dev/null 2>&1; then
  echo "Production simultaneous density composition mutation survived its focused owner." >&2
  exit 1
fi
cp "$task_tmp/ExperienceSamples.fs" "$samples_source"

sed -i '/+ " mm of movement credit; advance simulation time to move\." ]/a\                    LastCombatEvents = []' "$simulator_source"
if dotnet run --project "$repo_root/tests/SIR.Client.Tests/SIR.Client.Tests.fsproj" -c Release --no-restore >/dev/null 2>&1; then
  echo "Production simultaneous attack/route retention mutation survived its focused owner." >&2
  exit 1
fi
cp "$task_tmp/MapEditorSimulator.fs" "$simulator_source"

sed -i 's/slice(0, 2)/slice(0, 1)/' "$review_generator"
one_route_review="$task_tmp/one-route-review"
node "$review_generator" --client-root "$repo_root/artifacts/client" --review-root "$one_route_review" >/dev/null
if one_route_diagnostic=$(node "$repo_root/scripts/test-tactical-visual-review.mjs" --client-root "$repo_root/artifacts/client" --review-root "$one_route_review" 2>&1); then
  echo "Production one-route mutation survived the final simultaneous-content owner." >&2
  exit 1
fi
if ! grep -q "final simultaneous workload lost distinct route" <<<"$one_route_diagnostic"; then
  echo "Production one-route mutation missed the simultaneous-route diagnostic: $one_route_diagnostic" >&2
  exit 1
fi
cp "$task_tmp/generate-tactical-visual-review.mjs" "$review_generator"

sed -i '/await writeFile(resolve(reviewOutput, "manifest.json")/i manifest.visualSystem.identity = "mutated-tactical-visual-system";' "$review_generator"
if reproduction_diagnostic=$(node "$repo_root/scripts/test-tactical-visual-review.mjs" --client-root "$repo_root/artifacts/client" --review-root "$task_tmp/review" 2>&1); then
  echo "Environment-sensitive manifest mutation survived exact reproduction." >&2
  exit 1
fi
if ! grep -q 'delta=manifest.after.semantic.identity: retained="tactical-visual-system-v1", reproduced="mutated-tactical-visual-system"' <<<"$reproduction_diagnostic"; then
  echo "Manifest reproduction mutation omitted the exact semantic delta: $reproduction_diagnostic" >&2
  exit 1
fi
cp "$task_tmp/generate-tactical-visual-review.mjs" "$review_generator"

echo "Tactical visual review mutations passed: stylesheet, lifecycle projection, production workload, sample-faction, simultaneous attack/route, production one-route, and exact manifest-reproduction subjects fail closed."
