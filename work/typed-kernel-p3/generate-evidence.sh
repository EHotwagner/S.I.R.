#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
evidence_root="$repo_root/readiness/typed-kernel-p3"
task_tmp=$(mktemp -d)
trap 'rm -rf -- "$task_tmp"' EXIT

cd "$repo_root"
mkdir -p "$evidence_root"

# Exercise the consumer from the public NuGet source with a fresh package root.
export NUGET_PACKAGES="$task_tmp/nuget-packages"
dotnet restore SIR.slnx --configfile NuGet.Config --locked-mode --no-http-cache
dotnet build SIR.slnx -c Release --no-restore
./scripts/test-conformance.sh --domain-only
./scripts/verify-rules-corpus.sh
SIR_RULES_FORCE_GREP=1 ./scripts/verify-rules-corpus.sh

package_root="$NUGET_PACKAGES/fs.gg.sdd.artifacts/1.3.0-preview.3"
package_archive="$package_root/fs.gg.sdd.artifacts.1.3.0-preview.3.nupkg"
test -s "$package_archive"
unzip -p "$package_archive" FS.GG.SDD.Artifacts.nuspec >"$task_tmp/package.nuspec"
grep -F 'FS.GG.Contracts" version="7.5.2"' "$task_tmp/package.nuspec" >/dev/null || {
  echo "typed-kernel-p3: producer archive does not pin FS.GG.Contracts 7.5.2" >&2
  exit 1
}
unzip -Z1 "$package_archive" >"$task_tmp/package.entries"
grep -F 'fable/SpecificationKernel.fs' "$task_tmp/package.entries" >/dev/null || {
  echo "typed-kernel-p3: producer archive is missing portable Fable source" >&2
  exit 1
}
if grep -E '(^|/)(SIR|EHotwagner)([./]|$)' "$task_tmp/package.entries" >/dev/null; then
  echo "typed-kernel-p3: producer archive contains a consumer-owned entry" >&2
  exit 1
fi

mapfile -t lock_files < <(rg -l '"FS.GG.SDD.Artifacts"' --glob packages.lock.json | sort)
test "${#lock_files[@]}" -ge 20 || {
  echo "typed-kernel-p3: fewer than 20 consumer lock graphs reference the package" >&2
  exit 1
}
for lock_file in "${lock_files[@]}"; do
  jq -e '
    [.. | objects | select(has("FS.GG.SDD.Artifacts"))
      | .["FS.GG.SDD.Artifacts"]
      | select(type == "object")
      | select(.resolved == "1.3.0-preview.3")]
    | length > 0
  ' "$lock_file" >/dev/null || {
    echo "typed-kernel-p3: $lock_file does not resolve exact preview.3" >&2
    exit 1
  }
done

cat >"$evidence_root/typed-kernel-p3.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="6" failures="0" skipped="0"><testsuite name="typed-kernel-p3" tests="6" failures="0" skipped="0">
<properties><property name="producer" value="work/typed-kernel-p3/generate-evidence.sh"/><property name="package" value="FS.GG.SDD.Artifacts 1.3.0-preview.3"/></properties>
<testcase classname="typed-kernel-p3" name="nuget.org-only clean locked restore passes" />
<testcase classname="typed-kernel-p3" name="release solution build passes" />
<testcase classname="typed-kernel-p3" name="dotnet and fable domain conformance agree" />
<testcase classname="typed-kernel-p3" name="rules corpus projections and mismatch mutations pass" />
<testcase classname="typed-kernel-p3" name="producer archive carries fable source and no consumer entries" />
<testcase classname="typed-kernel-p3" name="all consumer lock graphs resolve exact preview package" />
</testsuite></testsuites>
XML

echo "typed-kernel-p3 evidence generated from public-package consumer gates"
