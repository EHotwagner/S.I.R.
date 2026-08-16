#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
test_root=$(mktemp -d /tmp/sir-feedback-binding-exceptions.XXXXXX)
trap 'rm -rf -- "$test_root"' EXIT
cd "$repo_root"

mkdir -p "$test_root/feedback/audits" "$test_root/scripts"
cp "$repo_root/feedback/audits/2026-08-13-sir-item-183-tactical-overlays.audit.json" "$test_root/feedback/audits/"
cp "$repo_root/feedback/audits/2026-08-15-sir-item-184-elaborate-tactical-sample-2.audit.json" "$test_root/feedback/audits/"
cp "$repo_root/feedback/audits/2026-08-15-SIR-186-2.audit.json" "$test_root/feedback/audits/"
cp "$repo_root/feedback/audits/2026-08-15-SIR-186-3.audit.json" "$test_root/feedback/audits/"
cp "$repo_root/feedback/audits/2026-08-15-SIR-186-6.audit.json" "$test_root/feedback/audits/"
cp "$repo_root/feedback/audits/2026-08-16-sir-item-220-bounded-pr-ci.audit.json" "$test_root/feedback/audits/"
cp "$repo_root/scripts/audit-binding-exceptions.json" "$test_root/scripts/"
cp --parents \
  tests/SIR.Browser.Tests/visible-workflows.spec.js \
  scripts/test-conformance.sh \
  scripts/smoke-client.mjs \
  scripts/ci-route.mjs \
  scripts/test-ci-route.mjs \
  src/SIR.Client.Web/App.fs \
  src/SIR.Client/MapEditorSimulator.fs \
  docs/performance-budget.md \
  readiness/184-scenario-catalog/scenario-catalog-native.junit.xml \
  readiness/184-scenario-catalog/scenario-catalog-browser.junit.xml \
  readiness/184-scenario-catalog/scenario-catalog-cross-runtime.junit.xml \
  readiness/184-scenario-catalog/scenario-catalog-rules.junit.xml \
  "$test_root"

tool="$repo_root/.agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx"
changed="scripts/test-conformance.sh;scripts/smoke-client.mjs;scripts/ci-route.mjs;scripts/test-ci-route.mjs;src/SIR.Client.Web/App.fs;src/SIR.Client/MapEditorSimulator.fs;docs/performance-budget.md;readiness/184-scenario-catalog/scenario-catalog-native.junit.xml;readiness/184-scenario-catalog/scenario-catalog-browser.junit.xml;readiness/184-scenario-catalog/scenario-catalog-cross-runtime.junit.xml;readiness/184-scenario-catalog/scenario-catalog-rules.junit.xml"
ledger="$test_root/scripts/audit-binding-exceptions.json"
pristine="$test_root/scripts/audit-binding-exceptions.pristine.json"
cp "$ledger" "$pristine"

run_check() {
  dotnet fsi "$tool" -- check-invalidation --changed "$changed" --root "$test_root"
}

validate_ledger_owners() {
  local evidence command
  local -a argv
  while IFS= read -r evidence; do
    command=${evidence#command:}
    read -r -a argv <<< "$command"
    case "${argv[0]:-}" in
      ./scripts/qualify-pr.sh)
        [[ ${#argv[@]} -eq 3 && ${argv[1]} == prepare-part ]] || {
          echo "feedback audit-binding: unsupported qualify-pr owner: $command" >&2
          return 1
        }
        case "${argv[2]}" in
          native|fable|web|server|docs) ;;
          *) echo "feedback audit-binding: unknown prepare owner: ${argv[2]}" >&2; return 1 ;;
        esac
        ;;
      ./scripts/run-ci-gate.sh)
        [[ ${#argv[@]} -eq 3 ]] || {
          echo "feedback audit-binding: unsupported gate owner: $command" >&2
          return 1
        }
        case "${argv[1]}" in
          integrity|rules|spatial|cross-runtime|cancellation|browser|documentation|evidence) ;;
          *) echo "feedback audit-binding: unknown gate owner: ${argv[1]}" >&2; return 1 ;;
        esac
        ;;
      node)
        [[ ${#argv[@]} -eq 2 && -f ${argv[1]} ]] || {
          echo "feedback audit-binding: unavailable node owner: $command" >&2
          return 1
        }
        ;;
      scripts/*)
        [[ ${#argv[@]} -eq 1 && -x ${argv[0]} ]] || {
          echo "feedback audit-binding: unavailable script owner: $command" >&2
          return 1
        }
        ;;
      *) echo "feedback audit-binding: unsupported executable owner: $command" >&2; return 1 ;;
    esac
  done < <(jq -r '.exceptions[].replacementEvidence' "$ledger")
}

run_mutant() {
  local name=$1 expected=$2 filter=$3
  cp "$pristine" "$ledger"
  jq "$filter" "$ledger" > "$ledger.next"
  mv "$ledger.next" "$ledger"
  if run_check > "$test_root/$name.log" 2>&1; then
    echo "feedback audit-binding $name mutant unexpectedly passed" >&2
    exit 1
  fi
  grep -F "$expected" "$test_root/$name.log" >/dev/null || {
    echo "feedback audit-binding $name mutant failed without '$expected'" >&2
    cat "$test_root/$name.log" >&2
    exit 1
  }
}

run_check >/dev/null
validate_ledger_owners

printf '{' > "$ledger"
if run_check > "$test_root/malformed.log" 2>&1; then
  echo "feedback audit-binding malformed-ledger mutant unexpectedly passed" >&2
  exit 1
fi
grep -F "malformed exception ledger" "$test_root/malformed.log" >/dev/null

run_mutant stale "replacement evidence is stale" '.exceptions[0].replacementSha256 = ("0" * 64)'
run_mutant mismatched "previous digest is mismatched" '.exceptions[0].previousSha256 = ("0" * 64)'
run_mutant duplicate "duplicate exception ledger binding" '.exceptions += [.exceptions[0]]'
run_mutant overbroad "overbroad or mismatched exception" '.exceptions += [(.exceptions[0] | .findingId = "§9.9")]'

cp "$pristine" "$ledger"
jq '(.exceptions[] | select(.audit == "feedback/audits/2026-08-15-SIR-186-2.audit.json" and .locator == "file:src/SIR.Client/MapEditorSimulator.fs")).replacementEvidence = "command:./scripts/run-ci-gate.sh native artifacts/ci/results/native.json"' "$ledger" > "$ledger.next"
mv "$ledger.next" "$ledger"
if validate_ledger_owners > "$test_root/nonexistent-owner.log" 2>&1; then
  echo "feedback audit-binding nonexistent-owner mutant unexpectedly passed" >&2
  exit 1
fi
grep -F "feedback audit-binding: unknown gate owner: native" "$test_root/nonexistent-owner.log" >/dev/null || {
  echo "feedback audit-binding nonexistent-owner mutant failed without the owner diagnostic" >&2
  cat "$test_root/nonexistent-owner.log" >&2
  exit 1
}

cp "$pristine" "$ledger"
run_check >/dev/null
validate_ledger_owners
echo "feedback audit-binding exception gate passed with malformed, stale, mismatched, duplicate, overbroad, and nonexistent-owner mutants rejected"
