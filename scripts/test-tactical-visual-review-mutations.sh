#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
projection_source="$repo_root/src/SIR.Client/TacticalSceneProjection.fs"
samples_source="$repo_root/src/SIR.Client/ExperienceSamples.fs"
simulator_source="$repo_root/src/SIR.Client/MapEditorSimulator.fs"
review_generator="$repo_root/scripts/generate-tactical-visual-review.mjs"
review_font="$repo_root/scripts/assets/tactical-visual-review-font/SIRReviewMono-Regular.woff2"
review_font_bold="$repo_root/scripts/assets/tactical-visual-review-font/SIRReviewMono-Bold.woff2"
cp "$projection_source" "$task_tmp/TacticalSceneProjection.fs"
cp "$samples_source" "$task_tmp/ExperienceSamples.fs"
cp "$simulator_source" "$task_tmp/MapEditorSimulator.fs"
cp "$review_generator" "$task_tmp/generate-tactical-visual-review.mjs"
cp "$review_font" "$task_tmp/SIRReviewMono-Regular.woff2"
restore_sources() {
  cp "$task_tmp/TacticalSceneProjection.fs" "$projection_source"
  cp "$task_tmp/ExperienceSamples.fs" "$samples_source"
  cp "$task_tmp/MapEditorSimulator.fs" "$simulator_source"
  cp "$task_tmp/generate-tactical-visual-review.mjs" "$review_generator"
  cp "$task_tmp/SIRReviewMono-Regular.woff2" "$review_font"
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

cp "$review_font_bold" "$review_font"
if font_diagnostic=$(node "$repo_root/scripts/test-tactical-visual-review.mjs" --client-root "$repo_root/artifacts/client" --review-root "$task_tmp/review" 2>&1); then
  echo "Capture-font byte mutation survived its exact owner." >&2
  exit 1
fi
if ! grep -q "review capture regular font bytes drifted" <<<"$font_diagnostic"; then
  echo "Capture-font mutation missed the exact font owner: $font_diagnostic" >&2
  exit 1
fi
cp "$task_tmp/SIRReviewMono-Regular.woff2" "$review_font"

sed -i 's/telemetryScenes.push({ units, inputToPaintMilliseconds,/telemetryScenes.push({ units, inputToPaintMilliseconds: inputToPaintMilliseconds + 1000,/' "$review_generator"
if timing_diagnostic=$(node "$repo_root/scripts/test-tactical-visual-review.mjs" --client-root "$repo_root/artifacts/client" --review-root "$task_tmp/review" 2>&1); then
  echo "Reproduced timing mutation survived the unchanged performance owner." >&2
  exit 1
fi
# WHAT THIS MAY ASSERT ABOUT THE REFUSAL, AND WHERE THAT TEXT LIVES (S.I.R.#335).
# The mutation above is a TIMING mutation -- it adds a fixed 1000 ms to the generator's telemetry -- so
# the refusal it must provoke is the REPRODUCED input-to-paint budget refusal and nothing else. That
# refusal's wording is owned by scripts/lib/performance-budget.mjs: S.I.R.#318 moved it there so the
# budget is declared once and derived everywhere. This harness restated it as a hand-copied literal and
# the copy was never updated, so `main` went red at 019abe6c and stayed red -- with no budget breached,
# and the mutant caught by exactly the right assertion. A literal here is a second home for text the
# declaration owns; ask the owner for it instead, and it cannot rot again.
#
# The measured milliseconds differ every run, so the owner is asked for its reason at a distinctive
# sentinel measurement and that reason is split on the sentinel. What survives is the two
# measurement-INDEPENDENT halves, and together they pin the refusal to: the reproduced pass (`reproduced `,
# which the baseline pass at test-tactical-visual-review.mjs:116 does not emit), input-to-paint rather
# than cadence or structure, the 100-unit workload, and reproduction A. A frame-cadence breach, a
# structural breach, or a baseline breach satisfies NEITHER half, so a different red cannot stand in for
# this one.
mapfile -t derived_timing_expectations < <(node --input-type=module -e '
  const root = process.argv[1];
  const budgets = await import(`file://${root}/scripts/lib/performance-budget.mjs`);
  const budget = budgets.tacticalWorkloadBudgetFor(100);
  const sentinel = 987654.321;
  const reason = budgets.tacticalInputToPaintBudgetReason(budget, sentinel);
  if (!reason) throw new Error("the declared input-to-paint budget did not refuse its sentinel measurement");
  const halves = reason.split(String(sentinel));
  if (halves.length !== 2) throw new Error(`the declared refusal did not carry its measurement exactly once: ${reason}`);
  process.stdout.write(`reproduced ${halves[0]}\n${halves[1]}: reproduction=A;\n`);
' "$repo_root")
if [[ ${#derived_timing_expectations[@]} -ne 2 ]]; then
  echo "Reproduced timing mutation could not derive the declared input-to-paint refusal from scripts/lib/performance-budget.mjs." >&2
  exit 1
fi
for expected in '"reproduction":"A"' '"reproduction":"B"' '"units":100' '"units":200' "${derived_timing_expectations[@]}" 'Preserved tactical reproduction roots after failure:'; do
  if ! grep -qF -- "$expected" <<<"$timing_diagnostic"; then
    # NOT "omitted required telemetry/diagnostic". Nothing in this harness omits telemetry, and that
    # wording sent the first reader of this failure hunting a telemetry defect that does not exist
    # (S.I.R.#335). Name the substring that was absent, and say that it was absent.
    echo "Reproduced timing mutation diagnostic is missing a required substring [$expected]; full diagnostic: $timing_diagnostic" >&2
    exit 1
  fi
done
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

echo "Tactical visual review mutations passed: stylesheet, lifecycle projection, production workload, sample-faction, simultaneous attack/route, production one-route, capture-font, reproduced timing, and exact manifest-reproduction subjects fail closed."
