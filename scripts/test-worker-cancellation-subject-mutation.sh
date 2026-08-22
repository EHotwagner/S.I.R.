#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
subject="$repo_root/src/SIR.Client.Web/Worker.fs"
temporary_dir=$(mktemp -d /tmp/sir-worker-cancellation-mutation.XXXXXX)
original="$temporary_dir/Worker.fs"
log="$temporary_dir/mutation.log"
prepared_pr=false
mutation_only=false
if [[ "${1:-}" == "--prepared-pr" ]]; then
  prepared_pr=true
  shift
fi
if [[ "${1:-}" == "--mutation-only" ]]; then
  mutation_only=true
  shift
fi
[[ $# -eq 0 ]] || { echo "test-worker-cancellation-subject-mutation: usage [--prepared-pr|--mutation-only]" >&2; exit 2; }
cp -p "$subject" "$original"

restore_subject() {
  cp -p "$original" "$subject"
  touch "$subject"
}

cleanup() {
  restore_subject
  rm -rf -- "$temporary_dir"
}
trap cleanup EXIT

if ! grep -F 'do! yieldToWorkerMessages () |> Async.AwaitPromise' "$subject" >/dev/null; then
  echo "The cancellation-yield subject could not be located." >&2
  exit 1
fi

if [[ "$mutation_only" == true ]]; then
  native_artifacts="$temporary_dir/native-artifacts"
  native_log="$temporary_dir/native.log"
  dotnet restore tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj --locked-mode \
    --artifacts-path "$native_artifacts" >"$native_log" 2>&1
  SIR_BUILD_EXCEPTION=cancellation-fixture \
    dotnet build tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-restore \
      --artifacts-path "$native_artifacts" >>"$native_log" 2>&1 &
  native_pid=$!
fi

sed -i 's/do! yieldToWorkerMessages () |> Async.AwaitPromise/()/' "$subject"
client_status=0
SIR_BUILD_EXCEPTION=cancellation-mutant "$repo_root/scripts/build-client.sh" >/dev/null || client_status=$?
native_status=0
if [[ "$mutation_only" == true ]]; then wait "$native_pid" || native_status=$?; fi
if [[ $client_status -ne 0 || $native_status -ne 0 ]]; then
  [[ $native_status -eq 0 ]] || cat "$native_log" >&2
  echo "Worker cancellation mutation prerequisites failed: client=$client_status native=$native_status" >&2
  exit 1
fi

smoke_env=()
if [[ "$mutation_only" == true ]]; then
  smoke_env+=(SIR_DOMAIN_TESTS_DLL="$native_artifacts/bin/SIR.Domain.Tests/release/SIR.Domain.Tests.dll")
fi
if env "${smoke_env[@]}" node "$repo_root/scripts/smoke-worker-roundtrip.mjs" >"$log" 2>&1; then
  echo "Worker cancellation subject mutation unexpectedly passed." >&2
  exit 1
fi

if ! grep -E 'completed before the queued cancellation|cancellation acknowledgement|did not stop the active run-to' "$log" >/dev/null; then
  echo "Worker cancellation subject mutation failed for the wrong reason." >&2
  cat "$log" >&2
  exit 1
fi

restore_subject
if [[ "$mutation_only" == true ]]; then
  echo "Worker cancellation subject mutation failed closed and the source was restored."
  exit 0
fi
if [[ "$prepared_pr" == true ]]; then
  "$repo_root/scripts/qualify-pr.sh" extract-parts web >/dev/null
else
  SIR_BUILD_EXCEPTION=cancellation-restored "$repo_root/scripts/build-client.sh" >/dev/null
fi
node "$repo_root/scripts/smoke-worker-roundtrip.mjs" >/dev/null
echo "Worker cancellation subject mutation failed closed and the restored worker acknowledged cancellation."
