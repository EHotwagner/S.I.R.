#!/usr/bin/env bash
# Rebind the rules-corpus source correspondence baseline.
#
# This is the mutable half of the rules source pin (S.I.R.#264). When a pull request makes a
# reviewed change to one of the declared implementation sources, run this to record the source's
# new normalized digest in tests/fixtures/rules-corpus/v2/source-correspondence.json, in the SAME
# commit as the source change.
#
# It deliberately CANNOT:
#   * rebind a path whose normalized text has not actually changed -- an accidental sweep produces
#     an empty diff rather than nineteen fresh blessings;
#   * add or remove a path -- the frozen set is owned by implementation-sources.json .sources, and
#     scripts/verify-rules-corpus.sh independently refuses any divergence;
#   * touch sourceCommit, the sealed implementation digest, or any generated corpus fixture --
#     those belong to the immutable half of the pin and are not this tool's business;
#   * add, remove or otherwise decide the `.outsideIdentity` register (S.I.R.#290). That register
#     declares which compile items are knowingly OUTSIDE the sealed identity set, and acknowledging
#     a loss of correspondence coverage is a reviewed judgement, not a mechanical rebind. This tool
#     rewrites only `.paths`, and every other field -- schema, register, and its notes -- is carried
#     through untouched, so an acknowledgement can only ever enter the file by someone editing it.
#
# Usage:
#   scripts/rebind-rules-corpus-sources.sh              # report what would be rebound, write nothing
#   scripts/rebind-rules-corpus-sources.sh --write      # record the drifted digests
#   scripts/rebind-rules-corpus-sources.sh --write src/SIR.Client.Web/App.fs [...]
#                                                       # restrict the rebind to named paths
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
source_manifest="$repo_root/tests/fixtures/rules-corpus/v2/implementation-sources.json"
correspondence_manifest="$repo_root/tests/fixtures/rules-corpus/v2/source-correspondence.json"

mode=report
requested=()
for argument in "$@"; do
  case "$argument" in
    --write) mode=write ;;
    --report) mode=report ;;
    -h|--help) sed -n '2,26p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'; exit 0 ;;
    -*) echo "unknown option: $argument" >&2; exit 2 ;;
    *) requested+=("$argument") ;;
  esac
done

test -f "$source_manifest" || { echo "missing implementation source manifest: $source_manifest" >&2; exit 1; }
test -f "$correspondence_manifest" || { echo "missing source correspondence: $correspondence_manifest" >&2; exit 1; }

# Same normalization as scripts/verify-rules-corpus.sh. Kept identical on purpose: a divergence
# here would record digests the gate cannot reproduce.
normalize_implementation_source() {
  local artifact_path=$1
  local input_path=$2
  if test "$artifact_path" = "src/SIR.Simulation/CombatRules.fs"; then
    sed -E \
      -e 's/(Commit = ")[0-9a-f]{40}(" })/\1<SOURCE_COMMIT>\2/' \
      -e 's/(GetBytes ")[0-9a-f]{64}(" \])/\1<IMPLEMENTATION_DIGEST>\2/' \
      -e 's/(FS.GG.Game.Core@0\.13\.0" ")[0-9a-f]{40}(" implementationArtifacts)/\1<SOURCE_COMMIT>\2/' \
      "$input_path"
  else
    command cat "$input_path"
  fi
}

normalized_source_digest() {
  normalize_implementation_source "$1" "$2" | sha256sum | cut -d' ' -f1
}

declared_sources=$(jq -r '.sources[]' "$source_manifest")

# A requested path that is not a declared implementation source is a mistake, not a request to
# widen the frozen set. Refuse rather than silently ignoring it.
for requested_path in "${requested[@]-}"; do
  test -n "$requested_path" || continue
  printf '%s\n' "$declared_sources" | grep -Fxq -- "$requested_path" || {
    echo "not a declared implementation source: $requested_path" >&2
    echo "  the frozen set is owned by tests/fixtures/rules-corpus/v2/implementation-sources.json .sources" >&2
    exit 1
  }
done

drifted=()
missing=()
while IFS= read -r artifact_path; do
  if test ${#requested[@]} -gt 0; then
    printf '%s\n' "${requested[@]}" | grep -Fxq -- "$artifact_path" || continue
  fi
  if ! test -f "$repo_root/$artifact_path"; then
    missing+=("$artifact_path")
    continue
  fi
  recorded=$(jq -r --arg path "$artifact_path" '.paths[$path] // empty' "$correspondence_manifest")
  current=$(normalized_source_digest "$artifact_path" "$repo_root/$artifact_path")
  if test "$recorded" != "$current"; then
    drifted+=("$artifact_path")
    printf 'rebind %s\n  recorded %s\n  current  %s\n' "$artifact_path" "${recorded:-<none>}" "$current" >&2
  fi
done <<< "$declared_sources"

if test ${#missing[@]} -gt 0; then
  echo "declared implementation sources are missing from the tree:" >&2
  printf '  %s\n' "${missing[@]}" >&2
  echo "  refusing to rebind a partial tree; restore the sources or amend .sources first" >&2
  exit 1
fi

if test ${#drifted[@]} -eq 0; then
  echo "source correspondence is already current: nothing to rebind"
  exit 0
fi

if test "$mode" != write; then
  printf '%d implementation source(s) would be rebound; re-run with --write to record them\n' "${#drifted[@]}"
  exit 0
fi

# Refuse to record a digest for text whose behaviour the corpus rejects.
#
# A correspondence entry asserts that the sealed corpus identity still describes this source.
# Building is not enough to support that claim: a change to an algorithm body compiles cleanly,
# moves no declared rule value, and would be blessed here while the rules it implements no longer
# behave as the corpus says (S.I.R.#264 round 1). So execute the corpus before recording anything.
# This is defence in depth, not the guard: scripts/verify-rules-corpus.sh runs the same conformance
# route, so a hand-edited correspondence file that never went through this tool is still refused.
# Build the project that actually COMPILES each rebound path.
#
# Two earlier versions of this guard were wrong in the same way: they named a project by convention
# and never checked that it compiles the file. The first built only src/SIR.Simulation, covering 9
# of the 19 declared paths. The second resolved src/<Project>/<Project>.fsproj -- right for App.fs,
# wrong for three others: RulesExplorer.fs is compiled by SIR.RulesExplorer.Web.fsproj, and Lab.fs
# and EngineCatalog.fs are compiled by src/SIR.Replay.Core/SIR.Replay.Core.fsproj through a
# ../SIR.Client/ include, a different directory entirely. Both versions resolved to a project that
# EXISTS and BUILDS while leaving the changed file uncompiled, so a digest was recorded for text
# nobody compiled (S.I.R.#264 review rounds 1 and 2).
#
# So resolve by reading Compile Include entries and comparing resolved paths. A path that no
# project compiles is refused rather than silently blessed.
#
# Those entries are read by PARSING the project, not by matching the substring `Compile Include="`,
# and this reader deliberately mirrors the verifier's (S.I.R.#290 round 2). The substring form was
# blind to attribute order, to a single-quoted value, to a non-canonically-cased element, and to a
# literal `>` earlier in the tag -- all legal MSBuild, all of which BUILD. Here that blindness is
# fail-closed rather than an escape, because a declared path whose owner cannot be resolved is
# refused a few lines below; but "refused" would then be reported as `no project compiles <path>`,
# which is a confident wrong answer to a question this reader could not evaluate, and a path
# compiled by TWO projects where only one is seen is rebound having built less than this file
# claims it builds. Both are the same defect the verifier carries the repair for, so the two
# readers are kept in step; probe 37 in scripts/verify-rules-corpus.sh fails if they diverge.
project_compile_includes() {  # absolute project path
  python3 -c '
import sys, xml.etree.ElementTree as ET

try:
    root = ET.parse(sys.argv[1]).getroot()
except Exception as exc:
    sys.stderr.write("project did not parse as MSBuild XML: %s: %s\n" % (sys.argv[1], exc))
    sys.exit(3)

for element in root.iter():
    name = element.tag
    if not isinstance(name, str):
        continue
    if name.rsplit("}", 1)[-1].lower() != "compile":
        continue
    value = element.get("Include")
    if value is None:
        continue
    for part in value.split(";"):
        part = part.strip()
        if not part:
            continue
        if "\n" in part or "\r" in part:
            sys.stderr.write("Include value spans lines: %r\n" % part)
            sys.exit(3)
        sys.stdout.write(part + "\n")
' "$1"
}

owning_projects() {
  local artifact_path=$1
  local project project_dir include resolved items items_status
  while IFS= read -r project; do
    test -n "$project" || continue
    project_dir=$(dirname "$project")
    items_status=0
    items=$(project_compile_includes "$repo_root/$project") || items_status=$?
    test "$items_status" -eq 0 || return 1
    while IFS= read -r include; do
      test -n "$include" || continue
      resolved=$(realpath -m --relative-to="$repo_root" "$repo_root/$project_dir/$include" 2>/dev/null) || continue
      if test "$resolved" = "$artifact_path"; then
        printf '%s\n' "$project"
        break
      fi
    done <<< "$items"
  done <<< "$(cd "$repo_root" && find src tests -name '*.fsproj' | sort)"
}

build_projects=()
for artifact_path in "${drifted[@]}"; do
  owners=()
  owners_out=""
  owners_status=0
  owners_out=$(owning_projects "$artifact_path") || owners_status=$?
  # An unreadable project is not an absent owner. Separating the two matters here because the two
  # remedies are opposite: one is "declare the compile item", the other is "fix the project file".
  if test "$owners_status" -ne 0; then
    echo "refusing to rebind: a project file could not be read as MSBuild XML (diagnostic above)" >&2
    echo "  owning-project resolution for $artifact_path is incomplete, and an owner this script" >&2
    echo "  could not evaluate is not an owner it established does not exist" >&2
    exit 1
  fi
  while IFS= read -r project; do
    test -n "$project" || continue
    owners+=("$project")
  done <<< "$owners_out"
  if test ${#owners[@]} -eq 0; then
    echo "refusing to rebind: no project compiles $artifact_path" >&2
    echo "  a correspondence digest recorded for text nobody compiles is an untestable assertion," >&2
    echo "  and this guard cannot verify a path that no build covers" >&2
    exit 1
  fi
  for project in "${owners[@]}"; do
    printf '%s\n' "${build_projects[@]-}" | grep -Fxq -- "$project" || build_projects+=("$project")
  done
done
corpus_project=tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj
printf '%s\n' "${build_projects[@]-}" | grep -Fxq -- "$corpus_project" || build_projects+=("$corpus_project")

for project in "${build_projects[@]}"; do
  echo "building $project before recording new identity digests..." >&2
  dotnet build "$repo_root/$project" -c Release >/dev/null 2>&1 || {
    echo "refusing to rebind: $project does not build" >&2
    echo "  a correspondence digest recorded for text nobody compiled is an untestable assertion" >&2
    exit 1
  }
done

conformance_log=$(mktemp /tmp/sir-rules-rebind-conformance.XXXXXX)
if ! dotnet run --project "$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj" -c Release --no-build >"$conformance_log" 2>&1; then
  echo "refusing to rebind: registered executable behaviour does not satisfy the rules corpus fixtures" >&2
  echo "  rebinding here would record a digest asserting the corpus still describes this source, which it does not" >&2
  grep -iE 'exception|failwith|did not' "$conformance_log" | head -5 >&2
  rm -f "$conformance_log"
  exit 1
fi
rm -f "$conformance_log"

updated=$(mktemp /tmp/sir-rules-rebind.XXXXXX)
cp "$correspondence_manifest" "$updated"
for artifact_path in "${drifted[@]}"; do
  digest=$(normalized_source_digest "$artifact_path" "$repo_root/$artifact_path")
  jq --arg path "$artifact_path" --arg digest "$digest" '.paths[$path] = $digest' "$updated" > "$updated.next"
  mv "$updated.next" "$updated"
done
mv "$updated" "$correspondence_manifest"

printf 'rebound %d implementation source correspondence entr%s\n' \
  "${#drifted[@]}" "$(test ${#drifted[@]} -eq 1 && printf 'y' || printf 'ies')"
printf '  %s\n' "${drifted[@]}"
echo "review the source-correspondence.json diff: it names exactly the identity subjects that moved"
