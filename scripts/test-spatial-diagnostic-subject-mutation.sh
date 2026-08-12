#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
subject="$repo_root/src/SIR.Server/SpatialDiagnostics.fs"
server_project="$repo_root/tests/SIR.Server.Tests/SIR.Server.Tests.fsproj"
temporary_dir=$(mktemp -d /tmp/sir-spatial-diagnostic-mutation.XXXXXX)
original="$temporary_dir/SpatialDiagnostics.fs"
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

server_gate() {
  dotnet test "$server_project" -c Release --no-restore \
    --filter 'FullyQualifiedName~spatial diagnostics require identity' \
    --logger 'console;verbosity=minimal'
}

browser_gate() {
  SIR_JUNIT_OUTPUT=artifacts/test-results/spatial-diagnostic-mutation.junit.xml \
    npx playwright test \
      tests/SIR.Browser.Tests/visible-workflows.spec.js \
      tests/SIR.Browser.Tests/production-delivery.spec.js \
      --config tests/SIR.Browser.Tests/playwright.config.js \
      --grep 'player-visible spatial diagnostics route|Release delivery uses cache-safe compression'
}

cd "$repo_root"
sed -i 's/Path = result.Path |> List.map cellDto |> List.toArray/Path = [||]/' "$subject"
grep -F 'Path = [||]' "$subject" >/dev/null || {
  echo "Spatial diagnostic subject mutation did not apply" >&2
  exit 1
}

if server_gate >"$temporary_dir/server-mutant.log" 2>&1; then
  echo "Server diagnostic gate accepted an empty authoritative path" >&2
  exit 1
fi
grep -F 'complete non-empty authoritative path' "$temporary_dir/server-mutant.log" >/dev/null || {
  echo "Server diagnostic gate rejected the empty-path mutant for the wrong reason" >&2
  cat "$temporary_dir/server-mutant.log" >&2
  exit 1
}

dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release -o artifacts/publish --no-restore >/dev/null
if browser_gate >"$temporary_dir/browser-mutant.log" 2>&1; then
  echo "Production browser diagnostic gate accepted an empty authoritative path" >&2
  exit 1
fi
if ! grep -F 'not.toHaveText("none")' "$temporary_dir/browser-mutant.log" >/dev/null \
  || ! grep -F 'Authoritative path' "$temporary_dir/browser-mutant.log" >/dev/null; then
  echo "Production browser diagnostic gate rejected the empty-path mutant for the wrong reason" >&2
  cat "$temporary_dir/browser-mutant.log" >&2
  exit 1
fi

restore_subject
server_gate
dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release -o artifacts/publish --no-restore >/dev/null
browser_gate

echo "Spatial diagnostic mutation failed closed in server and production-browser gates; restored subject passed both."
