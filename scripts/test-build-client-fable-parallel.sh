#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
fixture=$(mktemp -d "${TMPDIR:-/tmp}/sir-fable-parallel.XXXXXXXX")
trap 'rm -rf "$fixture"' EXIT
mkdir -p "$fixture/bin"

cp "$repo_root/scripts/test-fixtures/fake-parallel-dotnet.sh" "$fixture/bin/dotnet"
chmod +x "$fixture/bin/dotnet"
export PATH="$fixture/bin:$PATH"

sequential_start=$(date +%s%N)
dotnet fable src/SIR.Replay.Web/SIR.Replay.Web.fsproj --outDir "$fixture/sequential-replay" --define SIR_WEB_CLIENT --noCache
dotnet fable src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj --outDir "$fixture/sequential-rules" --noCache
sequential_ms=$((($(date +%s%N) - sequential_start) / 1000000))

parallel_start=$(date +%s%N)
"$repo_root/scripts/build-client-fable-targets.sh" "$fixture/parallel-replay" "$fixture/parallel-rules"
parallel_ms=$((($(date +%s%N) - parallel_start) / 1000000))

sequential_digest=$(sha256sum "$fixture/sequential-replay/result.txt" "$fixture/sequential-rules/result.txt" | awk '{print $1}' | sha256sum | awk '{print $1}')
parallel_digest=$(sha256sum "$fixture/parallel-replay/result.txt" "$fixture/parallel-rules/result.txt" | awk '{print $1}' | sha256sum | awk '{print $1}')
[[ "$sequential_digest" == "$parallel_digest" ]] || { echo "parallel target output digest drift" >&2; exit 1; }
(( parallel_ms < sequential_ms )) || { echo "parallel target proof was not faster: sequential=${sequential_ms}ms parallel=${parallel_ms}ms" >&2; exit 1; }

set +e
failure=$(SIR_FAKE_FAIL_BOTH=1 "$repo_root/scripts/build-client-fable-targets.sh" "$fixture/fail-replay" "$fixture/fail-rules" 2>&1)
failure_status=$?
set -e
(( failure_status != 0 )) || { echo "parallel target failure was accepted" >&2; exit 1; }
[[ "$failure" == *"replay=7 rules=9"* ]] || { echo "parallel target failure collection was not deterministic: $failure" >&2; exit 1; }

printf 'parallel Fable target proof passed: sequential=%sms parallel=%sms digest=%s deterministic-failures=7,9\n' \
  "$sequential_ms" "$parallel_ms" "$parallel_digest"
