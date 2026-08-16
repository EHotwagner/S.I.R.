#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
fixture=$(mktemp -d)
trap 'rm -rf -- "$fixture"' EXIT
mkdir -p "$fixture/repo/scripts" "$fixture/bin"
cp "$repo_root/scripts/qualify-pr.sh" "$fixture/repo/scripts/qualify-pr.sh"

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
  set +e
  PATH="$fixture/bin:$PATH" SIR_FAKE_FAILURE_STAGE="$expected_stage" SIR_CI_RUNNER_START_MS=1000 "$fixture/repo/scripts/qualify-pr.sh" prepare-part native >/dev/null 2>&1
  status=$?
  set -e
  [[ $status -ne 0 ]] || { echo "failure-timing mutation: $expected_stage failure unexpectedly passed" >&2; exit 1; }
  [[ -f "$timing" ]] || { echo "failure-timing mutation: failed producer omitted its timing receipt" >&2; exit 1; }
  node - "$timing" "$expected_stage" <<'NODE'
const { readFileSync } = require("node:fs");
const receipt = JSON.parse(readFileSync(process.argv[2], "utf8"));
if (receipt.failureStage !== process.argv[3]) throw new Error(`expected ${process.argv[3]} failureStage, got ${receipt.failureStage}`);
for (const field of ["startedAtMilliseconds", "completedAtMilliseconds", "setup", "restore", "build", "transport"]) {
  if (!Number.isSafeInteger(receipt[field]) || receipt[field] < 0) throw new Error(`invalid ${field}`);
}
NODE
done

echo "CI failed-producer timing receipt mutation passed."
