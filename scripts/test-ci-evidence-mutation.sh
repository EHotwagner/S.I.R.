#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
temporary=$(mktemp -d /tmp/sir-ci-evidence-mutation.XXXXXX)
checkout="$temporary/checkout"
cleanup() {
  git -C "$repo_root" worktree remove --force "$checkout" >/dev/null 2>&1 || true
  rm -rf -- "$temporary"
}
trap cleanup EXIT

git -C "$repo_root" worktree add --detach "$checkout" HEAD >/dev/null
cp "$repo_root/scripts/qualify-pr.sh" "$checkout/scripts/qualify-pr.sh"
mkdir -p "$checkout/artifacts/ci/results"
printf '%s\n' work/220-bounded-pr-ci/spec.md >"$checkout/artifacts/ci/changed-paths.txt"
(
  cd "$checkout"
  ./scripts/qualify-pr.sh route artifacts/ci/changed-paths.txt
)
subject="$checkout/readiness/220-bounded-pr-ci/ci-route.junit.xml"
rm -- "$subject"
if (cd "$checkout" && ./scripts/qualify-pr.sh gate evidence) >"$temporary/missing.log" 2>&1; then
  echo "ci-evidence mutation unexpectedly accepted a missing cited artifact" >&2
  exit 1
fi
grep -F "Evidence passes while citing an artifact that does not exist" "$temporary/missing.log" >/dev/null || {
  echo "ci-evidence mutation failed for the wrong reason" >&2
  sed -n '1,160p' "$temporary/missing.log" >&2
  exit 1
}
node - "$checkout/artifacts/ci/phase-timing.json" <<'NODE'
const { readFileSync } = require("node:fs");
const receipt = JSON.parse(readFileSync(process.argv[2], "utf8"));
if (receipt.failureStage !== "test") throw new Error(`expected test failureStage, got ${receipt.failureStage}`);
if (!Number.isSafeInteger(receipt.restore) || !Number.isSafeInteger(receipt.test)) throw new Error("failed evidence receipt omitted actual phase timing");
NODE
git -C "$checkout" restore readiness/220-bounded-pr-ci
dotnet fsgg-sdd verify --work 220-bounded-pr-ci --root "$checkout" --text >/dev/null

echo "CI evidence clean-checkout mutation passed: tracked observed evidence verifies, a missing cited artifact fails closed, and the fixture is isolated."
