#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
# This harness owns a detached checkout and must not inherit the parent gate's
# timing paths. An inherited absolute path would write the mutation receipt
# into the parent checkout and make the detached proof fail for the wrong
# reason.
unset SIR_CI_PHASE_PATH SIR_CI_EXTRACT_TIMING_PATH
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

printf '%s\n' work/231-svg-pipeline-measurement/hosted-verification.sh >"$checkout/artifacts/ci/changed-paths.txt"
(
  cd "$checkout"
  ./scripts/qualify-pr.sh route artifacts/ci/changed-paths.txt
)

hook="$checkout/work/231-svg-pipeline-measurement/hosted-verification.sh"
hook_backup="$temporary/hosted-verification.sh"
cp -p "$hook" "$hook_backup"
fake_bin="$temporary/fake-bin"
mkdir -p "$fake_bin"
printf '#!/usr/bin/env bash\nexit 0\n' >"$fake_bin/dotnet"
printf '#!/usr/bin/env bash\nexit 0\n' >"$fake_bin/npm"
chmod +x "$fake_bin/dotnet" "$fake_bin/npm"

run_hosted_dispatch() {
  (cd "$checkout" && PATH="$fake_bin:$PATH" ./scripts/qualify-pr.sh gate evidence)
}

printf '#!/usr/bin/env bash\nexit 37\n' >"$hook"
chmod +x "$hook"
set +e
run_hosted_dispatch >"$temporary/propagation.log" 2>&1
propagation_status=$?
set -e
if [[ $propagation_status -ne 37 ]]; then
  echo "ci-evidence hosted verification did not propagate exit 37 (got $propagation_status)" >&2
  sed -n '1,160p' "$temporary/propagation.log" >&2
  exit 1
fi

cp -p "$hook_backup" "$hook"
chmod -x "$hook"
if run_hosted_dispatch >"$temporary/non-executable.log" 2>&1; then
  echo "ci-evidence mutation unexpectedly accepted a non-executable hosted verification" >&2
  exit 1
fi
grep -F "qualify-pr: routed hosted verification is not executable: work/231-svg-pipeline-measurement/hosted-verification.sh" "$temporary/non-executable.log" >/dev/null || {
  echo "ci-evidence non-executable mutation failed for the wrong reason" >&2
  sed -n '1,160p' "$temporary/non-executable.log" >&2
  exit 1
}

rm -- "$hook"
if run_hosted_dispatch >"$temporary/absent.log" 2>&1; then
  echo "ci-evidence mutation unexpectedly accepted an absent hosted verification" >&2
  exit 1
fi
grep -F "qualify-pr: routed hosted verification is missing or not a regular file: work/231-svg-pipeline-measurement/hosted-verification.sh" "$temporary/absent.log" >/dev/null || {
  echo "ci-evidence absent mutation failed for the wrong reason" >&2
  sed -n '1,160p' "$temporary/absent.log" >&2
  exit 1
}

cp -p "$hook_backup" "$hook"

echo "CI evidence clean-checkout mutations passed: tracked observed evidence verifies, missing cited evidence fails closed, hosted verification failure propagates, and absent/non-executable routed hooks fail closed."
