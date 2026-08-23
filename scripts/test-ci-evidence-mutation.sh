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

# S.I.R.#272 — an absent evidence.yml must never be a silent skip. Before the repair,
# scripts/qualify-pr.sh guarded the per-work-item verify with
# `[[ -f "work/$work_id/evidence.yml" ]] || continue`, so "checked and passed" and "not checked"
# produced the same exit status and the same (empty) record. These three mutations pin the three
# outcomes apart. They run under the stubbed dotnet/npm PATH because what is under test is the
# gate's own classification and coverage record, not fsgg-sdd's verdict — the unstubbed verify is
# already exercised by the first mutation in this file.
coverage="$checkout/artifacts/ci/results/evidence-coverage.json"
evidence_declaration="$checkout/work/220-bounded-pr-ci/evidence.yml"
evidence_backup="$temporary/evidence.yml"
tasks_declaration="$checkout/work/220-bounded-pr-ci/tasks.yml"
tasks_backup="$temporary/tasks.yml"
cp -p "$evidence_declaration" "$evidence_backup"
cp -p "$tasks_declaration" "$tasks_backup"

printf '%s\n' work/220-bounded-pr-ci/evidence.yml >"$checkout/artifacts/ci/changed-paths.txt"
(
  cd "$checkout"
  ./scripts/qualify-pr.sh route artifacts/ci/changed-paths.txt
)

run_evidence_gate() {
  (cd "$checkout" && PATH="$fake_bin:$PATH" ./scripts/qualify-pr.sh gate evidence)
}

coverage_field() {
  node - "$coverage" "$1" "$2" <<'NODE'
const { readFileSync } = require("node:fs");
const [path, workId, field] = process.argv.slice(2);
const record = JSON.parse(readFileSync(path, "utf8"));
const entry = (record.items ?? []).find((item) => item.workId === workId);
process.stdout.write(entry === undefined ? "<absent>" : String(entry[field]));
NODE
}

# 1. Evidence present: the item is checked, and the record says so.
rm -f -- "$coverage"
set +e
run_evidence_gate >"$temporary/coverage-present.log" 2>&1
present_status=$?
set -e
if [[ $present_status -ne 0 ]]; then
  echo "ci-evidence coverage: a tree with evidence present did not pass the gate (got $present_status)" >&2
  sed -n '1,160p' "$temporary/coverage-present.log" >&2
  exit 1
fi
if [[ ! -f "$coverage" ]]; then
  echo "ci-evidence coverage: the evidence gate recorded no coverage artifact at artifacts/ci/results/evidence-coverage.json" >&2
  exit 1
fi
if [[ "$(coverage_field 220-bounded-pr-ci outcome)" != verified || "$(coverage_field 220-bounded-pr-ci checked)" != true ]]; then
  echo "ci-evidence coverage: a checked-and-passed item was not recorded as checked/verified" >&2
  cat "$coverage" >&2
  exit 1
fi

# 2. Evidence deleted while the package still declares completed implementation: fails closed.
#    This is the defect in S.I.R.#272 — before the repair this run was green and silent.
rm -- "$evidence_declaration"
rm -f -- "$coverage"
set +e
run_evidence_gate >"$temporary/coverage-absent.log" 2>&1
absent_status=$?
set -e
if [[ $absent_status -eq 0 ]]; then
  echo "ci-evidence coverage: deleting evidence.yml left the gate green for a package that still declares completed implementation" >&2
  sed -n '1,160p' "$temporary/coverage-absent.log" >&2
  exit 1
fi
grep -F "qualify-pr: routed evidence declaration is missing while tasks still declare completed implementation: work/220-bounded-pr-ci/evidence.yml" "$temporary/coverage-absent.log" >/dev/null || {
  echo "ci-evidence coverage: absent evidence.yml failed for the wrong reason" >&2
  sed -n '1,160p' "$temporary/coverage-absent.log" >&2
  exit 1
}
if [[ ! -f "$coverage" ]]; then
  echo "ci-evidence coverage: a failing evidence gate recorded no coverage artifact" >&2
  exit 1
fi
if [[ "$(coverage_field 220-bounded-pr-ci outcome)" != not-evidenced || "$(coverage_field 220-bounded-pr-ci checked)" != false ]]; then
  echo "ci-evidence coverage: an unchecked item was not recorded as not-evidenced/unchecked" >&2
  cat "$coverage" >&2
  exit 1
fi

# 3. Evidence absent for a package legitimately mid-lifecycle (no completed tasks): passes, and the
#    non-coverage is still recorded, so "not checked" never reads as "checked and passed".
sed -i 's/^    status: done$/    status: pending/' "$tasks_declaration"
rm -f -- "$coverage"
set +e
run_evidence_gate >"$temporary/coverage-midlifecycle.log" 2>&1
midlifecycle_status=$?
set -e
if [[ $midlifecycle_status -ne 0 ]]; then
  echo "ci-evidence coverage: a package with no completed tasks and no evidence.yml was failed outright (got $midlifecycle_status)" >&2
  sed -n '1,160p' "$temporary/coverage-midlifecycle.log" >&2
  exit 1
fi
if [[ "$(coverage_field 220-bounded-pr-ci outcome)" != not-evidenced || "$(coverage_field 220-bounded-pr-ci checked)" != false ]]; then
  echo "ci-evidence coverage: a passing run hid the fact that the item was never checked" >&2
  cat "$coverage" >&2
  exit 1
fi
grep -F "qualify-pr: evidence coverage not-evidenced: work/220-bounded-pr-ci" "$temporary/coverage-midlifecycle.log" >/dev/null || {
  echo "ci-evidence coverage: a passing run did not report the uncovered item on its output" >&2
  sed -n '1,160p' "$temporary/coverage-midlifecycle.log" >&2
  exit 1
}

cp -p "$evidence_backup" "$evidence_declaration"
cp -p "$tasks_backup" "$tasks_declaration"

echo "CI evidence clean-checkout mutations passed: tracked observed evidence verifies, missing cited evidence fails closed, hosted verification failure propagates, absent/non-executable routed hooks fail closed, every routed work item gets a recorded coverage outcome, and an absent evidence.yml is reported rather than skipped — fatally when the package still declares completed implementation."
