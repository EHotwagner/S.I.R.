#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
fixture=$(mktemp -d)
trap 'rm -rf -- "$fixture"' EXIT
mkdir -p "$fixture/repo/scripts" "$fixture/bin"
cp "$repo_root/scripts/qualify-pr.sh" "$fixture/repo/scripts/qualify-pr.sh"
cp "$repo_root/scripts/ci-route.mjs" "$fixture/repo/scripts/ci-route.mjs"

cat >"$fixture/bin/dotnet" <<'FAKE'
#!/usr/bin/env bash
if [[ "${1:-}" == tool && "${2:-}" == restore ]]; then exit 0; fi
if [[ "${1:-}" == restore ]]; then [[ "${SIR_FAKE_FAILURE_STAGE:-restore}" == restore ]] && exit 41 || exit 0; fi
exit 42
FAKE
chmod +x "$fixture/bin/dotnet"

timing="$fixture/repo/artifacts/ci/parts/native.timing.json"
for expected_stage in restore build; do
  rm -rf -- "$fixture/repo/artifacts"
  mkdir -p "$fixture/repo/artifacts/ci"
  cat >"$fixture/repo/artifacts/ci/route.json" <<'JSON'
{"source":{"commit":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","tree":"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"},"digest":"cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc"}
JSON
  set +e
  PATH="$fixture/bin:$PATH" SIR_FAKE_FAILURE_STAGE="$expected_stage" SIR_CI_RUNNER_START_MS=1000 "$fixture/repo/scripts/qualify-pr.sh" prepare-part native >"$fixture/$expected_stage.log" 2>&1
  status=$?
  set -e
  [[ $status -ne 0 ]] || { echo "failure-timing mutation: $expected_stage failure unexpectedly passed" >&2; exit 1; }
  [[ -f "$timing" ]] || { echo "failure-timing mutation: failed producer omitted its timing receipt" >&2; exit 1; }
  result="$fixture/repo/artifacts/ci/results/prepare-native.json"
  [[ -f "$result" ]] || { echo "failure-timing mutation: failed producer omitted its typed verdict input" >&2; sed -n '1,120p' "$fixture/$expected_stage.log" >&2; exit 1; }
  node - "$timing" "$result" "$expected_stage" <<'NODE'
const { readFileSync } = require("node:fs");
const receipt = JSON.parse(readFileSync(process.argv[2], "utf8"));
const result = JSON.parse(readFileSync(process.argv[3], "utf8"));
if (receipt.failureStage !== process.argv[4]) throw new Error(`expected ${process.argv[4]} failureStage, got ${receipt.failureStage}`);
if (result.status !== "fail" || result.failureStage !== process.argv[4]) throw new Error("typed result lost producer failure phase");
for (const field of ["startedAtMilliseconds", "completedAtMilliseconds", "setup", "restore", "build", "transport"]) {
  if (!Number.isSafeInteger(receipt[field]) || receipt[field] < 0) throw new Error(`invalid ${field}`);
}
NODE
done

echo "CI failed-producer timing mutation passed: each failure preserves its actual phase in timing and typed verdict inputs."
