#!/usr/bin/env bash
set -euo pipefail

root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
cd "$root"

require() { grep -F -- "$1" "$2" >/dev/null || { echo "missing required governance declaration: $1 in $2" >&2; exit 1; }; }
reject() { if grep -R -n -E --exclude-dir=work --exclude-dir=readiness "$1" "$2" >/dev/null 2>&1; then echo "forbidden package-boundary match: $1" >&2; exit 1; fi; }

# XML must be parsed, not merely contain expected text.  This fails before the
# fixed-string checks if a subject mutates Props into malformed XML.
dotnet msbuild Directory.Packages.props -nologo -getProperty:ManagePackageVersionsCentrally >/dev/null

require 'FS.GG.Game.Core" Version="[0.13.0]"' Directory.Packages.props
require 'FS.GG.Governance.Cli" Version="[1.12.1]"' Directory.Packages.props
require 'FS.GG.Governance.ReferenceGateSet" Version="[1.7.0]"' Directory.Packages.props
require 'PackageReference Include="FS.GG.Game.Core"' src/SIR.Simulation/SIR.Simulation.fsproj
require 'PackageReference Include="FS.GG.Governance.ReferenceGateSet"' src/SIR.Simulation/SIR.Simulation.fsproj

for file in .fsgg/governance.yml .fsgg/policy.yml .fsgg/capabilities.yml .fsgg/tooling.yml; do
  test -s "$file" || { echo "missing governance configuration: $file" >&2; exit 1; }
done

# Governance owns the strict YAML schemas; route is the executable policy
# validation and deliberately runs in every local/CI conformance invocation.
dotnet fsi scripts/validate-governance-yaml.fsx
dotnet tool run fsgg-governance route --root . --mode inner --format json >/dev/null
# The producer writes the receipt SDD consumes for the declared F# surface.
surface_project=$(dotnet fsi scripts/validate-governance-yaml.fsx -- --package-surface)
dotnet tool run fsgg-fsharp-surface -- --root . --project "$surface_project" >/dev/null

for root_name in .agents .claude .codex; do
  for skill in fs-gg-ai fs-gg-ballistics fs-gg-effects fs-gg-game-core fs-gg-grids fs-gg-line-drawing fs-gg-mapcraft fs-gg-persistence fs-gg-playtest fs-gg-visibility; do
    test -s "$root_name/skills/$skill/SKILL.md" || { echo "missing materialized skill: $root_name/$skill" >&2; exit 1; }
    cmp -s ".agents/skills/$skill/SKILL.md" ".claude/skills/$skill/SKILL.md" || { echo "materialized skill differs: .agents/.claude $skill" >&2; exit 1; }
    cmp -s ".agents/skills/$skill/SKILL.md" ".codex/skills/$skill/SKILL.md" || { echo "materialized skill differs: .agents/.codex $skill" >&2; exit 1; }
  done
done

# A published package is the only Game.Core authority; product source cannot take a sibling/project/file dependency.
reject 'ProjectReference.*FS\.GG\.Game\.Core|FS\.GG\.Game\.Core.*ProjectReference' .
reject '(/|\\)FS\.GG\.Game\.Core(/|\\)' src tests
reject '<Reference[[:space:]][^>]*FS\.GG\.Game\.Core|<HintPath>[^<]*(local|\.dll)' src tests

echo 'Fable game governance package, configuration, and materialized-skill boundary verified.'
