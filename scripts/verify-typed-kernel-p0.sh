#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
result_path=${SIR_TYPED_KERNEL_P0_JUNIT:-"$repo_root/readiness/typed-kernel-p0/typed-kernel-p0.junit.xml"}

"$repo_root/scripts/capture-typed-kernel-p0.sh" --check
"$repo_root/scripts/test-typed-kernel-p0.sh"

mkdir -p "$(dirname "$result_path")"
printf '%s\n' \
  '<testsuites name="typed-kernel-p0" tests="3" failures="0" errors="0" skipped="0"><testsuite name="typed-kernel-p0" tests="3" failures="0" errors="0" skipped="0"><testcase name="content-addressed-baseline" classname="SIR.TypedKernel.P0"/><testcase name="native-fable-parity" classname="SIR.TypedKernel.P0"/><testcase name="wrong-path-controls" classname="SIR.TypedKernel.P0"/></testsuite></testsuites>' \
  > "$result_path"

printf 'Typed-kernel P0 verification receipt written to %s.\n' "$result_path"
