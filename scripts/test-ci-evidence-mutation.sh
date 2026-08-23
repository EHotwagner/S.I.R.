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
# `dotnet restore` and the scaffold build are stubbed because this harness is about the gate's own
# classification, not about compiling the product. `dotnet fsgg-sdd` is deliberately NOT stubbed:
# after S.I.R.#272's repair the owed / not-owed / cannot-determine decision is ANSWERED by fsgg-sdd
# parsing tasks.yml, so a stub that answered it here would make every tasks.yml mutation below
# decorative — the fixture would be simpler than production in precisely the way this gate exists to
# catch. Passing the real tool through is what keeps those mutations load-bearing.
real_dotnet=$(command -v dotnet)
cat >"$fake_bin/dotnet" <<STUB
#!/usr/bin/env bash
[[ "\${1:-}" == fsgg-sdd ]] && exec "$real_dotnet" "\$@"
exit 0
STUB
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

# 4. The two states are no longer the same bytes. Before the repair a checked-and-passed run and a
#    not-checked run produced identical (empty) output and the same exit status, which is the whole
#    defect; this pins them apart directly rather than by inference from the outcome vocabulary.
if cmp -s "$temporary/coverage-present.log" "$temporary/coverage-absent.log"; then
  echo "ci-evidence coverage: a checked run and an unchecked run produced byte-identical output" >&2
  exit 1
fi
if [[ ! -s "$temporary/coverage-midlifecycle.log" ]]; then
  echo "ci-evidence coverage: a passing unchecked run produced no output at all" >&2
  exit 1
fi
if cmp -s "$temporary/coverage-present.log" "$temporary/coverage-midlifecycle.log"; then
  echo "ci-evidence coverage: a passing checked run and a passing unchecked run produced byte-identical output" >&2
  exit 1
fi

cp -p "$evidence_backup" "$evidence_declaration"
cp -p "$tasks_backup" "$tasks_declaration"

# 5. A failing item does not silently abandon the routed items behind it. Before the repair a bare
#    `break` left every later work item unverified and unmentioned.
printf '%s\n' work/220-bounded-pr-ci/evidence.yml work/231-svg-pipeline-measurement/spec.md \
  >"$checkout/artifacts/ci/changed-paths.txt"
(
  cd "$checkout"
  ./scripts/qualify-pr.sh route artifacts/ci/changed-paths.txt
)
rm -- "$evidence_declaration"
rm -f -- "$coverage"
set +e
run_evidence_gate >"$temporary/coverage-not-reached.log" 2>&1
not_reached_status=$?
set -e
if [[ $not_reached_status -eq 0 ]]; then
  echo "ci-evidence coverage: a fatal first work item left the gate green" >&2
  exit 1
fi
if [[ "$(coverage_field 231-svg-pipeline-measurement outcome)" != not-reached \
   || "$(coverage_field 231-svg-pipeline-measurement checked)" != false ]]; then
  echo "ci-evidence coverage: a work item abandoned behind a failure was not recorded as not-reached" >&2
  cat "$coverage" >&2
  exit 1
fi
grep -F "qualify-pr: evidence coverage not-reached: work/231-svg-pipeline-measurement" "$temporary/coverage-not-reached.log" >/dev/null || {
  echo "ci-evidence coverage: an abandoned work item was not reported on the gate output" >&2
  sed -n '1,160p' "$temporary/coverage-not-reached.log" >&2
  exit 1
}
cp -p "$evidence_backup" "$evidence_declaration"

# 6. Removing the whole routed work package is recorded rather than skipped, and is not conflated
#    with a package that was checked.
printf '%s\n' work/220-bounded-pr-ci/spec.md >"$checkout/artifacts/ci/changed-paths.txt"
(
  cd "$checkout"
  ./scripts/qualify-pr.sh route artifacts/ci/changed-paths.txt
)
package_backup="$temporary/220-bounded-pr-ci"
cp -a "$checkout/work/220-bounded-pr-ci" "$package_backup"
rm -rf -- "$checkout/work/220-bounded-pr-ci"
rm -f -- "$coverage"
set +e
run_evidence_gate >"$temporary/coverage-removed.log" 2>&1
removed_status=$?
set -e
if [[ $removed_status -ne 0 ]]; then
  echo "ci-evidence coverage: removing a work package failed the gate for the wrong reason (got $removed_status)" >&2
  sed -n '1,160p' "$temporary/coverage-removed.log" >&2
  exit 1
fi
if [[ "$(coverage_field 220-bounded-pr-ci outcome)" != work-package-removed \
   || "$(coverage_field 220-bounded-pr-ci checked)" != false ]]; then
  echo "ci-evidence coverage: a removed work package was not recorded as work-package-removed/unchecked" >&2
  cat "$coverage" >&2
  exit 1
fi
rm -rf -- "$checkout/work/220-bounded-pr-ci"
cp -a "$package_backup" "$checkout/work/220-bounded-pr-ci"

# 7. A checked item whose verify fails is recorded as checked-and-failed, so `checked` is a fact
#    about coverage and not a synonym for success.
verify_fail_bin="$temporary/verify-fail-bin"
mkdir -p "$verify_fail_bin"
cat >"$verify_fail_bin/dotnet" <<'STUB'
#!/usr/bin/env bash
[[ "${2:-}" == verify ]] && exit 9
exit 0
STUB
chmod +x "$verify_fail_bin/dotnet"
rm -f -- "$coverage"
set +e
(cd "$checkout" && PATH="$verify_fail_bin:$fake_bin:$PATH" ./scripts/qualify-pr.sh gate evidence) \
  >"$temporary/coverage-failed.log" 2>&1
failed_status=$?
set -e
if [[ $failed_status -ne 9 ]]; then
  echo "ci-evidence coverage: a failing fsgg-sdd verify did not propagate exit 9 (got $failed_status)" >&2
  sed -n '1,160p' "$temporary/coverage-failed.log" >&2
  exit 1
fi
if [[ "$(coverage_field 220-bounded-pr-ci outcome)" != failed \
   || "$(coverage_field 220-bounded-pr-ci checked)" != true ]]; then
  echo "ci-evidence coverage: a checked-and-failed item was not recorded as failed/checked" >&2
  cat "$coverage" >&2
  exit 1
fi

# The mutations below exist because the FIRST repair of S.I.R.#272 was itself silenceable. It moved
# the load-bearing existence condition from evidence.yml onto tasks.yml, and matched `status: done`
# as text. Cases 8-12 break the SUBJECT in the four ways that escape a text match over a file that is
# assumed to be readable, plus one that no file-level mutation can reach at all.
printf '%s\n' work/220-bounded-pr-ci/spec.md >"$checkout/artifacts/ci/changed-paths.txt"
(
  cd "$checkout"
  ./scripts/qualify-pr.sh route artifacts/ci/changed-paths.txt
)

expect_indeterminate() {
  local label=$1 log=$2 status=$3
  if [[ $status -eq 0 ]]; then
    echo "ci-evidence coverage: $label left the gate green instead of refusing to decide" >&2
    sed -n '1,160p' "$log" >&2
    exit 1
  fi
  if [[ "$(coverage_field 220-bounded-pr-ci outcome)" != evidence-owed-indeterminate \
     || "$(coverage_field 220-bounded-pr-ci fatal)" != true \
     || "$(coverage_field 220-bounded-pr-ci checked)" != false ]]; then
    echo "ci-evidence coverage: $label was not recorded as evidence-owed-indeterminate/fatal/unchecked" >&2
    cat "$coverage" >&2
    exit 1
  fi
  # The record must not ASSERT that no evidence is owed. "I could not evaluate this" is never
  # "I evaluated it and nothing is owed" (#266).
  if coverage_field 220-bounded-pr-ci detail | grep -qF "declares no completed implementation"; then
    echo "ci-evidence coverage: $label recorded a confident not-owed claim about an input it never evaluated" >&2
    cat "$coverage" >&2
    exit 1
  fi
}

run_absent_evidence_gate() {
  rm -f -- "$coverage"
  set +e
  run_evidence_gate >"$1" 2>&1
  absent_evidence_status=$?
  set -e
}

echo "ci-evidence mutation 8: quoted YAML scalar status: \"done\""
# 8. The quoted YAML scalar. `status: "done"` and `status: done` are the SAME scalar, and fsgg-sdd
#    reads them the same way; the text match this replaced saw only the second. Nothing in the tree
#    uses the quoted spelling today, which is a fact about today's tree and not about the format.
rm -- "$evidence_declaration"
python3 - "$tasks_declaration" <<'PYQUOTE'
import re, sys
path = sys.argv[1]
source = open(path, encoding="utf-8").read()
rewritten, count = re.subn(r'(?m)^(\s*)status: done\s*$', r'\1status: "done"', source)
if count == 0:
    raise SystemExit("no unquoted `status: done` entries to rewrite; fixture no longer reproduces the subject")
open(path, "w", encoding="utf-8").write(rewritten)
PYQUOTE
run_absent_evidence_gate "$temporary/coverage-quoted.log"
if [[ $absent_evidence_status -eq 0 ]]; then
  echo "ci-evidence coverage: quoting the YAML scalar as \"done\" silenced the gate" >&2
  sed -n '1,160p' "$temporary/coverage-quoted.log" >&2
  exit 1
fi
if [[ "$(coverage_field 220-bounded-pr-ci outcome)" != not-evidenced \
   || "$(coverage_field 220-bounded-pr-ci fatal)" != true ]]; then
  echo "ci-evidence coverage: a quoted-scalar done task was not recorded as not-evidenced/fatal" >&2
  cat "$coverage" >&2
  exit 1
fi
# Parity with the tool this gate wraps is the actual claim, so it is measured rather than asserted.
set +e
(cd "$checkout" && PATH="$fake_bin:$PATH" dotnet fsgg-sdd verify --work 220-bounded-pr-ci --root . --text) \
  >"$temporary/quoted-sdd.log" 2>&1
quoted_sdd_status=$?
set -e
if [[ $quoted_sdd_status -eq 0 ]]; then
  echo "ci-evidence coverage: fixture no longer reproduces the subject — fsgg-sdd accepted the quoted-scalar tree" >&2
  exit 1
fi
cp -p "$tasks_backup" "$tasks_declaration"

echo "ci-evidence mutation 9: tasks.yml deleted"
# 9. tasks.yml deleted. This is the escape the first repair introduced: `rm work/<id>/tasks.yml` is
#    one extra command in the same commit that deletes evidence.yml.
rm -- "$tasks_declaration"
run_absent_evidence_gate "$temporary/coverage-tasks-deleted.log"
expect_indeterminate "a deleted tasks.yml" "$temporary/coverage-tasks-deleted.log" "$absent_evidence_status"
cp -p "$tasks_backup" "$tasks_declaration"

echo "ci-evidence mutation 10: tasks.yml unparseable"
# 10. tasks.yml present but unparseable.
printf 'tasks:\n  - id: [T001\n    status: done\n   broken: {{{\n' >"$tasks_declaration"
run_absent_evidence_gate "$temporary/coverage-tasks-malformed.log"
expect_indeterminate "an unparseable tasks.yml" "$temporary/coverage-tasks-malformed.log" "$absent_evidence_status"
cp -p "$tasks_backup" "$tasks_declaration"

echo "ci-evidence mutation 11: tasks.yml unreadable"
# 11. tasks.yml present and well-formed but unreadable. The measurement is guarded: running as a user
#     who can read a 000-mode file (root) would make this pass without reproducing the condition, so
#     the harness aborts rather than banking a green it did not earn.
chmod 000 "$tasks_declaration"
if [[ -r "$tasks_declaration" ]]; then
  chmod 644 "$tasks_declaration"
  echo "ci-evidence coverage: this harness cannot measure the unreadable-tasks case as a user that can read a 000-mode file (uid $(id -u)); run it unprivileged" >&2
  exit 1
fi
run_absent_evidence_gate "$temporary/coverage-tasks-unreadable.log"
chmod 644 "$tasks_declaration"
expect_indeterminate "an unreadable tasks.yml" "$temporary/coverage-tasks-unreadable.log" "$absent_evidence_status"
cp -p "$tasks_backup" "$tasks_declaration"

echo "ci-evidence mutation 12: tool report carries no doneCount / is not JSON"
# 12. The shape no file-level mutation can reach: tasks.yml is perfectly fine, and the TOOL's report
#     no longer carries the count. This is why the predicate rests on a positive integer rather than
#     on the absence of a diagnostic — a renamed field or a changed schema must refuse, not silently
#     read as "nothing is owed". A gate that enumerated the broken-file shapes above would pass this
#     while being wrong.
schema_drift_bin="$temporary/schema-drift-bin"
mkdir -p "$schema_drift_bin"
cat >"$schema_drift_bin/dotnet" <<'STUB'
#!/usr/bin/env bash
if [[ "${1:-}" == fsgg-sdd ]]; then
  printf '%s\n' '{"tasks":{"workId":"220-bounded-pr-ci","stage":"tasks","status":"tasksReady"}}'
  exit 0
fi
exit 0
STUB
chmod +x "$schema_drift_bin/dotnet"
rm -f -- "$coverage"
set +e
(cd "$checkout" && PATH="$schema_drift_bin:$fake_bin:$PATH" ./scripts/qualify-pr.sh gate evidence) \
  >"$temporary/coverage-schema-drift.log" 2>&1
absent_evidence_status=$?
set -e
expect_indeterminate "a report whose tasks block carries no doneCount" \
  "$temporary/coverage-schema-drift.log" "$absent_evidence_status"

cat >"$schema_drift_bin/dotnet" <<'STUB'
#!/usr/bin/env bash
if [[ "${1:-}" == fsgg-sdd ]]; then
  printf '%s\n' 'not json at all'
  exit 0
fi
exit 0
STUB
chmod +x "$schema_drift_bin/dotnet"
rm -f -- "$coverage"
set +e
(cd "$checkout" && PATH="$schema_drift_bin:$fake_bin:$PATH" ./scripts/qualify-pr.sh gate evidence) \
  >"$temporary/coverage-nonjson.log" 2>&1
absent_evidence_status=$?
set -e
expect_indeterminate "a report that is not JSON" "$temporary/coverage-nonjson.log" "$absent_evidence_status"

echo "ci-evidence mutation 13: restored control tree must still pass"
# 13. The paired control. Every refusal above must be caused by the mutation and nothing else, so the
#     restored tree with no completed tasks and no evidence.yml must still PASS. Without this, a
#     harness that refused unconditionally would read as twelve successful demonstrations.
sed -i 's/^\( *\)status: done$/\1status: pending/' "$tasks_declaration"
run_absent_evidence_gate "$temporary/coverage-control.log"
if [[ $absent_evidence_status -ne 0 ]]; then
  echo "ci-evidence coverage: the restored control tree was refused, so the refusals above are not attributable to their mutations (got $absent_evidence_status)" >&2
  sed -n '1,160p' "$temporary/coverage-control.log" >&2
  exit 1
fi
if [[ "$(coverage_field 220-bounded-pr-ci outcome)" != not-evidenced \
   || "$(coverage_field 220-bounded-pr-ci fatal)" != false ]]; then
  echo "ci-evidence coverage: the control tree was not recorded as not-evidenced/non-fatal" >&2
  cat "$coverage" >&2
  exit 1
fi
cp -p "$tasks_backup" "$tasks_declaration"
cp -p "$evidence_backup" "$evidence_declaration"

echo "CI evidence clean-checkout mutations passed: tracked observed evidence verifies, missing cited evidence fails closed, hosted verification failure propagates, absent/non-executable routed hooks fail closed, every routed work item gets a recorded coverage outcome (verified, failed, not-evidenced, work-package-removed, not-reached), an absent evidence.yml is reported rather than skipped — fatally when the package still declares completed implementation — a checked run is never byte-identical to an unchecked one, and whether evidence is owed is REFUSED rather than guessed when tasks.yml is deleted, unparseable or unreadable, when the quoted YAML scalar is used, and when the tool's own report carries no task count — with a restored control tree still passing."
