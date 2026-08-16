#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
projection_source="$repo_root/src/SIR.Client/TacticalSceneProjection.fs"
samples_source="$repo_root/src/SIR.Client/ExperienceSamples.fs"
cp "$projection_source" "$task_tmp/TacticalSceneProjection.fs"
cp "$samples_source" "$task_tmp/ExperienceSamples.fs"
restore_sources() {
  cp "$task_tmp/TacticalSceneProjection.fs" "$projection_source"
  cp "$task_tmp/ExperienceSamples.fs" "$samples_source"
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

echo "Tactical visual review mutations passed: stylesheet, lifecycle projection, and production workload subjects fail closed."
