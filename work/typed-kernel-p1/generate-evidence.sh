#!/usr/bin/env bash
set -uo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
receipt="$repo_root/readiness/typed-kernel-p1/typed-kernel-p1.junit.xml"
temporary_dir=$(mktemp -d /tmp/sir-typed-kernel-evidence.XXXXXX)
trap 'rm -rf "$temporary_dir"' EXIT
mkdir -p "$(dirname "$receipt")"

build_rc=0
rules_rc=0
cross_runtime_rc=0

dotnet build "$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj" -c Release --no-restore >/dev/null 2>&1 || build_rc=$?
"$repo_root/scripts/verify-rules-corpus.sh" >/dev/null 2>&1 || rules_rc=$?

if ((build_rc == 0)); then
  dotnet run --project "$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj" -c Release --no-build -- --print-rule-specification > "$temporary_dir/native-specification.md" || cross_runtime_rc=$?
  if ((cross_runtime_rc == 0)); then
    dotnet fable "$repo_root/tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj" --outDir "$temporary_dir/fable" --noCache >/dev/null 2>&1 || cross_runtime_rc=$?
  fi
  if ((cross_runtime_rc == 0)); then
    node "$temporary_dir/fable/SIR.Conformance.Shared/Program.js" --print-rule-specification > "$temporary_dir/fable-specification.md" || cross_runtime_rc=$?
  fi
  if ((cross_runtime_rc == 0)); then
    cmp "$temporary_dir/native-specification.md" "$temporary_dir/fable-specification.md" >/dev/null 2>&1 || cross_runtime_rc=$?
  fi
else
  cross_runtime_rc=1
fi

failures=0
if ((build_rc != 0)); then failures=$((failures + 1)); fi
if ((rules_rc != 0)); then failures=$((failures + 1)); fi
if ((cross_runtime_rc != 0)); then failures=$((failures + 1)); fi

{
  printf '%s\n' '<?xml version="1.0" encoding="UTF-8"?>'
  printf '<testsuites name="typed-kernel-p1" tests="3" failures="%d" errors="0" skipped="0">\n' "$failures"
  printf '  <testsuite name="typed-kernel-p1" tests="3" failures="%d" errors="0" skipped="0">\n' "$failures"
  printf '    <testcase classname="SIR.TypedKernel" name="native-build-and-conformance">'
  if ((build_rc != 0)); then printf '<failure message="native build or conformance failed" />'; fi
  printf '%s\n' '</testcase>'
  printf '    <testcase classname="SIR.TypedKernel" name="projection-freshness-and-refusal-mutations">'
  if ((rules_rc != 0)); then printf '<failure message="rules corpus generation or mutation gate failed" />'; fi
  printf '%s\n' '</testcase>'
  printf '    <testcase classname="SIR.TypedKernel" name="native-fable-specification-equality">'
  if ((cross_runtime_rc != 0)); then printf '<failure message="native and Fable specification projections diverged" />'; fi
  printf '%s\n' '</testcase>'
  printf '%s\n' '  </testsuite>' '</testsuites>'
} > "$receipt"

if ((failures != 0)); then
  echo "typed-kernel evidence failed: build=$build_rc rules=$rules_rc cross-runtime=$cross_runtime_rc" >&2
  exit 1
fi

echo "typed-kernel observed evidence passed"
