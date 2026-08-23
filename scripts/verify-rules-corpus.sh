#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"

search_quiet() {
  local pattern=$1
  local path=$2
  if test "${SIR_RULES_FORCE_GREP:-0}" != 1 && command -v rg >/dev/null 2>&1; then
    rg -q -- "$pattern" "$path"
  else
    grep -Eq -- "$pattern" "$path"
  fi
}

source_manifest="$repo_root/tests/fixtures/rules-corpus/v2/implementation-sources.json"
correspondence_manifest="$repo_root/tests/fixtures/rules-corpus/v2/source-correspondence.json"
source_commit=$(jq -r '.sourceCommit' "$source_manifest")

# The rules source pin has two duties that a single commit cannot discharge together:
#
#   P1 source-link durability. `sourceCommit` is the commit published rule source links resolve
#      against, so a fresh network clone must be able to reach it. That REQUIRES it to be an
#      ancestor of the canonical default branch -- see require_durable_source_commit below.
#   P2 identity correspondence. The implementation sources must still hold the text the corpus
#      identity was sealed over. Rebinding that baseline REQUIRES naming text that is not yet on
#      the default branch, because a pull request's own content never is.
#
# P1 demands ancestry; P2 forbids it. Binding both to `sourceCommit` made P2 unsatisfiable: no
# pull request changing a pinned source could pass, because advancing the pin needs a commit that
# only exists after the merge the pin gates (S.I.R.#264). The duties are therefore split across two
# artifacts. `implementation-sources.json` keeps P1 and the sealed identity digest and does not
# change on rebind; `source-correspondence.json` carries P2 and is rebound in the same pull request
# that changes a source, via scripts/rebind-rules-corpus-sources.sh.
#
# Enforcement is NOT narrowed by this split: correspondence is still required for every one of the
# declared implementation sources, byte-exactly, after the same normalization as before.
declared_source_schema=$(jq -r '.schema' "$source_manifest")
test "$declared_source_schema" = "sir-rules-implementation-sources-v1" || {
  echo "unsupported implementation source manifest schema: $declared_source_schema" >&2
  exit 1
}
declared_correspondence_schema=$(jq -r '.schema' "$correspondence_manifest")
test "$declared_correspondence_schema" = "sir-rules-source-correspondence-v2" || {
  echo "unsupported source correspondence schema: $declared_correspondence_schema" >&2
  exit 1
}

require_durable_source_commit() {
  local git_repo=$1
  local commit=$2
  local canonical_ref=$3

  if [[ ! "$commit" =~ ^[0-9a-f]{40}$ ]]; then
    echo "declared rules source commit is not a 40-character lowercase Git object id: $commit" >&2
    return 1
  fi
  if ! git -C "$git_repo" cat-file -e "$commit^{commit}" 2>/dev/null; then
    echo "declared rules source commit is unavailable: $commit (fetch canonical history or rebind the corpus to a durable commit)" >&2
    return 1
  fi
  if ! git -C "$git_repo" show-ref --verify --quiet "$canonical_ref"; then
    echo "canonical remote default branch is unavailable: $canonical_ref (fetch the canonical remote before verifying the rules corpus)" >&2
    return 1
  fi
  if ! git -C "$git_repo" merge-base --is-ancestor "$commit" "$canonical_ref"; then
    echo "declared rules source commit is not durably reachable from $canonical_ref: $commit (local-only and deleted-branch objects are not reproducible in a fresh network clone)" >&2
    return 1
  fi
}

canonical_source_ref=refs/remotes/origin/main
require_durable_source_commit "$repo_root" "$source_commit" "$canonical_source_ref"

"$repo_root/scripts/generate-rules-corpus.sh" --check

for fixture in manifest.json coverage.json representative-application.hex; do
  fixture_mutant=$(mktemp -d /tmp/sir-rules-fixture-mutant.XXXXXX)
  cp "$repo_root/tests/fixtures/rules-corpus/v2/"* "$fixture_mutant/"
  printf '\n ' >> "$fixture_mutant/$fixture"
  if SIR_RULES_FIXTURE_DIR="$fixture_mutant" "$repo_root/scripts/generate-rules-corpus.sh" --check >/dev/null 2>&1; then
    echo "rules-corpus fixture mutation unexpectedly passed: $fixture" >&2
    rm -rf "$fixture_mutant"
    exit 1
  fi
  rm -rf "$fixture_mutant"
done

coverage_mutant=$(mktemp /tmp/sir-rules-coverage-mutant.XXXXXX)
jq '.edges[0].to = "missing:node"' "$repo_root/tests/fixtures/rules-corpus/v2/coverage.json" > "$coverage_mutant"
if "$repo_root/scripts/validate-rules-coverage.sh" "$coverage_mutant" >/dev/null 2>&1; then
  echo "rules coverage dangling-endpoint mutation unexpectedly passed" >&2
  rm -f "$coverage_mutant"
  exit 1
fi
rm -f "$coverage_mutant"

test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/manifest.json" | cut -d' ' -f1)" = "e5bfe82d40e72ff8b41898e408c50dd0d8fb7e05b72c6acc24baab0e3b451ddc" || { echo "retained v1 manifest changed" >&2; exit 1; }
test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/coverage.json" | cut -d' ' -f1)" = "39eecda1018c504eab7b03c60228bf155c99aa42433724655da42d9ee470d554" || { echo "retained v1 coverage changed" >&2; exit 1; }
test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/representative-application.hex" | cut -d' ' -f1)" = "f42835c3fc4691b59ff71c0b31de0e74caa21455bf9d5e7658b483e0b2da2606" || { echo "retained v1 application changed" >&2; exit 1; }

while IFS=$'\t' read -r source_path source_symbol; do
  test -f "$repo_root/$source_path" || { echo "missing rule source: $source_path" >&2; exit 1; }
  symbol_name=${source_symbol##*.}
  search_quiet "let (private )?${symbol_name}( |$)" "$repo_root/$source_path" || {
    echo "unresolved rule source symbol: $source_symbol in $source_path" >&2
    exit 1
  }
done <<< "$(jq -r '.rules[].source | select(. != null) | [.path, .symbol] | @tsv' "$repo_root/tests/fixtures/rules-corpus/v2/manifest.json")"

manifest_source_commit=$(jq -r '.sourceCommit' "$repo_root/tests/fixtures/rules-corpus/v2/manifest.json")
test "$manifest_source_commit" = "$source_commit" || { echo "implementation source manifest does not bind the package source commit" >&2; exit 1; }

reachability_mutant=$(mktemp -d /tmp/sir-rules-reachability-mutant.XXXXXX)
reachability_log=$(mktemp /tmp/sir-rules-reachability.XXXXXX)
git -C "$reachability_mutant" init -q
empty_tree=$(git -C "$reachability_mutant" hash-object -t tree /dev/null)
durable_mutant_commit=$(printf 'durable rules source\n' | env GIT_AUTHOR_NAME=Rules GIT_AUTHOR_EMAIL=rules@example.invalid GIT_COMMITTER_NAME=Rules GIT_COMMITTER_EMAIL=rules@example.invalid git -C "$reachability_mutant" commit-tree "$empty_tree")
local_only_mutant_commit=$(printf 'local-only rules source\n' | env GIT_AUTHOR_NAME=Rules GIT_AUTHOR_EMAIL=rules@example.invalid GIT_COMMITTER_NAME=Rules GIT_COMMITTER_EMAIL=rules@example.invalid git -C "$reachability_mutant" commit-tree "$empty_tree")
git -C "$reachability_mutant" update-ref refs/remotes/origin/main "$durable_mutant_commit"
if require_durable_source_commit "$reachability_mutant" "$durable_mutant_commit" refs/remotes/origin/missing >"$reachability_log" 2>&1; then
  echo "missing canonical source ref mutation unexpectedly passed" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
fi
search_quiet 'canonical remote default branch is unavailable: refs/remotes/origin/missing.*fetch the canonical remote' "$reachability_log" || {
  echo "missing canonical source ref mutation failed without the actionable fetch diagnostic" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
}
if require_durable_source_commit "$reachability_mutant" "$local_only_mutant_commit" refs/remotes/origin/main >"$reachability_log" 2>&1; then
  echo "local-only rules source commit mutation unexpectedly passed" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
fi
search_quiet 'not durably reachable from refs/remotes/origin/main.*local-only and deleted-branch objects' "$reachability_log" || {
  echo "local-only rules source commit mutation failed without the actionable durability diagnostic" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
}
if require_durable_source_commit "$reachability_mutant" 0000000000000000000000000000000000000000 refs/remotes/origin/main >"$reachability_log" 2>&1; then
  echo "missing rules source commit mutation unexpectedly passed" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
fi
search_quiet 'declared rules source commit is unavailable.*fetch canonical history or rebind' "$reachability_log" || {
  echo "missing rules source commit mutation failed without the actionable availability diagnostic" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
}
if require_durable_source_commit "$reachability_mutant" not-a-commit refs/remotes/origin/main >"$reachability_log" 2>&1; then
  echo "malformed rules source commit mutation unexpectedly passed" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
fi
search_quiet 'not a 40-character lowercase Git object id' "$reachability_log" || {
  echo "malformed rules source commit mutation failed without the actionable format diagnostic" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
}
rm -rf "$reachability_mutant"
rm -f "$reachability_log"

source_digest_input=$(mktemp /tmp/sir-rules-source-digest.XXXXXX)
# Invocation ledger for the two S.I.R.#290 arms. Probes drive those functions DIRECTLY, so a
# probe suite alone stays green when the production CALL SITE is deleted -- the gate would then
# demonstrate a refusal it no longer performs, which is FS.GG.Templates#379's defect exactly
# ("deleting the step reds too" claimed by a guard that did not provide it). Each arm records the
# tree root and correspondence document it was asked about, and the assertion after the probes
# requires an invocation naming the REAL tree and the REAL manifests. No probe can satisfy it:
# every probe passes a probe tree.
arm_invocations=$(mktemp /tmp/sir-rules-arm-invocations.XXXXXX)
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
  local artifact_path=$1
  local input_path=$2
  normalize_implementation_source "$artifact_path" "$input_path" | sha256sum | cut -d' ' -f1
}

source_matches_correspondence() {
  local artifact_path=$1
  local current_path=$2
  local correspondence_json=${3:-$correspondence_manifest}
  local expected
  local actual
  expected=$(jq -r --arg path "$artifact_path" '.paths[$path] // empty' "$correspondence_json")
  test -n "$expected" || return 1
  actual=$(normalized_source_digest "$artifact_path" "$current_path")
  test "$actual" = "$expected"
}

# The recorded baseline must name EXACTLY the declared implementation identity set. Without this,
# the cheapest way to unfreeze a source would be to delete its row -- the gate would then check
# eighteen files and report success, which is the vacuity failure this mechanism must not have.
check_correspondence_coverage() {
  local sources_json=$1
  local correspondence_json=$2
  local identity
  local recorded
  local malformed
  identity=$(jq -r '.sources[]' "$sources_json" | sort -u)
  recorded=$(jq -r '.paths | keys[]' "$correspondence_json" | sort -u)
  if test -z "$recorded"; then
    echo "recorded source correspondence is empty: every implementation source would go unchecked" >&2
    return 1
  fi
  if test "$identity" != "$recorded"; then
    echo "recorded source correspondence does not cover the implementation identity set exactly" >&2
    comm -23 <(printf '%s\n' "$identity") <(printf '%s\n' "$recorded") | sed 's/^/  declared implementation source with no recorded correspondence: /' >&2
    comm -13 <(printf '%s\n' "$identity") <(printf '%s\n' "$recorded") | sed 's/^/  recorded correspondence for a path that is not a declared implementation source: /' >&2
    return 1
  fi
  # Digest well-formedness is the ONLY arm here whose empty result means "pass"; for the two arms
  # above an empty result still reaches a `return 1`, so they already fail closed. That asymmetry is
  # why this arm -- and only this arm -- has to prove it actually evaluated its input.
  #
  # Two distinct failures are guarded, because fixing either alone leaves the other live:
  #
  #   1. `test/1` RAISES on any non-string (number, null, boolean, array, object) rather than
  #      returning false. Typing the predicate keeps `test/1` unreachable for a non-string, so a
  #      non-string is CLASSIFIED as malformed instead of aborting the filter.
  #   2. This function is only ever called on the LEFT of `||`, which suspends `set -e` for its whole
  #      body. So ANY jq failure -- the raise above, unreadable JSON, a jq crash -- would otherwise
  #      leave `malformed` empty and fall through to a confident `return 0` on input that was never
  #      evaluated. Checking jq's exit status makes an unevaluated input a refusal, which is what
  #      keeps this closed against a failure mode not enumerated above.
  local malformed_status=0
  malformed=$(jq -r '
    .paths
    | to_entries[]
    | select(if (.value | type) == "string"
             then (.value | test("^[0-9a-f]{64}$") | not)
             else true
             end)
    | "\(.key)\t\(.value | type)"' "$correspondence_json") || malformed_status=$?
  if test "$malformed_status" -ne 0; then
    echo "recorded source correspondence could not be evaluated for digest well-formedness" >&2
    echo "  jq exited $malformed_status over: $correspondence_json" >&2
    echo "  refusing rather than reporting a pass on input this check did not evaluate" >&2
    return 1
  fi
  if test -n "$malformed"; then
    echo "recorded source correspondence carries malformed digests:" >&2
    printf '%s\n' "$malformed" | sed 's/^/  /' >&2
    return 1
  fi
}

enforce_source_correspondence() {
  local sources_json=$1
  local correspondence_json=$2
  local tree_root=$3
  local artifact_path
  while IFS= read -r artifact_path; do
    test -f "$tree_root/$artifact_path" || {
      echo "declared implementation source is missing from the tree: $artifact_path" >&2
      return 1
    }
    source_matches_correspondence "$artifact_path" "$tree_root/$artifact_path" "$correspondence_json" || {
      echo "current implementation source differs from package pin: $artifact_path" >&2
      return 1
    }
  done <<< "$(jq -r '.sources[]' "$sources_json")"
}

# ---------------------------------------------------------------------------------------------
# Identity-closure containment (S.I.R.#290).
#
# `check_correspondence_coverage` proves the recorded baseline names EXACTLY the declared identity
# set. That is a closed question about two documents, and it stays green no matter what happens in
# the tree AROUND those nineteen files. So it cannot see the one move that costs coverage without
# changing either document: extracting implementation OUT of a declared source into a file that is
# not one.
#
# Measured before this arm existed, at 58041b8: `saturate` was moved out of the declared, pinned
# src/SIR.Domain/FixedPoint.fs into a new src/SIR.Domain/FixedPointArithmetic.fs, the new file was
# added to SIR.Domain.fsproj, and correspondence was rebound exactly as the documented procedure
# says. `scripts/verify-rules-corpus.sh` exited 0 on BOTH routes, the generated fixtures did not
# move, and the rebind writer reported "already current: nothing to rebind". Real implementation
# left the covered set in one green commit, silently.
#
# The subject this arm asserts over is the RULES IMPLEMENTATION CLOSURE, not the text of any file:
# every project that compiles a declared implementation source, PLUS every project reachable from
# one of those through `ProjectReference`, transitively -- and the compile items of all of them.
# Extraction grows that closure while the identity set -- frozen at `sourceCommit`, and provably
# unable to grow in the change that creates a file (see require_declared_sources_at_source_commit)
# -- stays put. The baseline is the same closure AT `sourceCommit`, which needs no new recorded
# state: `sourceCommit` is already required to be durably reachable.
#
# ROUND 0 OF THIS ITEM COMPUTED THAT CLOSURE OVER ONE HOP AND SAID IT WAS COMPLETE. It collected
# only the projects that list a declared source directly -- 9 of 25 at the time -- so the other 16
# were free ground. Its critic extracted `saturate` into a NEW project referenced from
# `SIR.Domain.fsproj`, and all four steps of the production `rules` gate passed BYTE-IDENTICALLY to
# a clean tree. The transitive walk in closure_projects is the repair, and the argument that the
# dependency direction is COMPLETE (rather than merely wider) is recorded there.
#
# THE LIMITS, STATED RATHER THAN IMPLIED, because an overclaimed limit is how a real gap becomes
# invisible -- and because the previous version of this very comment overclaimed one:
#
#   * It detects implementation entering the closure as a NEW compile item. It does NOT detect code
#     moved into a compile item that was ALREADY IN THE BASELINE CLOSURE at `sourceCommit` -- that
#     file was outside the seal before the move as well, so the move does not NARROW coverage
#     relative to the baseline.
#   * It classifies FILES, not behaviour. A refactor that moves an implementation AND its call site
#     out of a declared source is not caught here, because the moved code is no longer statically
#     reachable from the declared source. It is still visible: changing the declared source's text
#     forces a correspondence rebind, and that diff is the backstop.
#   * Nothing here claims to be a behaviour gate; behaviour is gated by manifest regeneration and by
#     executing the corpus.
#
# The escape hatch is a DECLARATION, and that is the whole point: a compile item that is knowingly
# outside the sealed identity set is recorded in `.outsideIdentity`, so the loss is a reviewable
# line in a pull request diff instead of nothing at all. Recording one never adds coverage -- the
# register is the COMPLEMENT of the identity set, so it cannot be used to widen the frozen set the
# way adding a `.sources` entry would.

# Compile items of one project, resolved repo-relative. An `Include` carrying an MSBuild property
# expression is emitted VERBATIM rather than resolved or dropped: this function cannot evaluate it,
# and a caller must be able to tell "I could not evaluate this" from a path (#266).
# Resolved `Include` paths of one MSBuild element kind in one project, repo-relative.
#
# The element is matched as an ELEMENT and `Include` as an ATTRIBUTE, not by the substring
# `<Tag Include="`. Attribute order is not significant in XML, so `<ProjectReference
# Condition="..." Include="..." />` is the same element as `<ProjectReference Include="..."
# Condition="..." />`; a substring matcher sees only one of them, and the one it misses is a free
# hiding place for exactly the extraction this arm exists to detect.
#
# An `Include` carrying an MSBuild property expression is emitted VERBATIM rather than resolved or
# dropped: this function cannot evaluate it, and a caller must be able to tell "I could not evaluate
# this" from a path (#266).
#
# Exit status distinguishes the two ways a project yields nothing, because the callers' policies
# differ: 0 with no output means "read it, found none"; 2 means "the project does not exist at this
# revision" (normal for a NEW project when reading the baseline, a refusal at HEAD); 1 means the
# read itself failed and nothing may be concluded.
project_elements() {
  local tree_root=$1
  local project=$2
  local tag=$3
  local rev=${4:-}
  local project_dir
  local content
  project_dir=$(dirname "$project")
  if test -n "$rev"; then
    git -C "$repo_root" cat-file -e "$rev:$project" 2>/dev/null || return 2
    content=$(git -C "$repo_root" show "$rev:$project" 2>/dev/null) || return 1
  else
    test -f "$tree_root/$project" || return 2
    content=$(command cat "$tree_root/$project") || return 1
  fi
  # `set -o pipefail` is in force, and grep exits 1 on NO MATCH -- an ordinary answer here, since
  # most projects declare no ProjectReference at all. Each stage is therefore captured and its
  # no-match arm separated from a real failure, rather than letting "found none" surface as "the
  # read failed". The read failures that matter were already decided above, before this pipeline.
  local elements
  local attributes
  elements=$(printf '%s\n' "$content" | tr '\n' ' ' | grep -oE "<$tag[[:space:]][^>]*>") || elements=""
  test -n "$elements" || return 0
  attributes=$(printf '%s\n' "$elements" | grep -oE 'Include[[:space:]]*=[[:space:]]*"[^"]*"') || attributes=""
  test -n "$attributes" || return 0
  printf '%s\n' "$attributes" \
    | sed -E 's/^Include[[:space:]]*=[[:space:]]*"//; s/"$//' \
    | tr '\\' '/' \
    | while IFS= read -r include; do
        test -n "$include" || continue
        case "$include" in
          *'$('*) printf '%s\n' "$project_dir/$include" ;;
          *) realpath -m --relative-to="$tree_root" "$tree_root/$project_dir/$include" 2>/dev/null || printf '%s\n' "$project_dir/$include" ;;
        esac
      done
}

project_compile_items() {
  project_elements "$1" "$2" Compile "${3:-}"
}

# Every .fsproj at one revision, repo-relative.
#
# "No projects here" is an ANSWER, not a failure, and it must reach the caller as empty output at
# exit 0 -- otherwise the vacuity guard downstream never speaks and an empty tree is refused with no
# diagnostic at all. `set -o pipefail` makes that easy to get wrong: `find` on a tree with no `src`
# and `grep` with no match both exit non-zero on the ordinary answer.
project_inventory() {
  local tree_root=$1
  local rev=${2:-}
  local listed
  if test -n "$rev"; then
    listed=$(git -C "$repo_root" ls-tree -r --name-only "$rev" -- src tests 2>/dev/null) || listed=""
  else
    listed=$(cd "$tree_root" && find src tests -name '*.fsproj' 2>/dev/null) || listed=""
  fi
  test -n "$listed" || return 0
  printf '%s\n' "$listed" | sed 's#^\./##' | grep -E '\.fsproj$' | sort || true
}

# The rules implementation closure at one revision: the projects that COMPILE a declared source,
# plus everything they reach transitively through `ProjectReference`.
#
# WHY THE DEPENDENCY DIRECTION IS THE COMPLETE ONE, and not merely a bigger guess than one hop:
# extraction moves code that a declared source still CALLS. A declared source can only call code its
# own project can reach, and .NET forbids reference cycles -- so a project that REFERENCES the
# declared source's project can never be an extraction target, because the declared source could not
# call back into it. That leaves exactly two destinations: the same project (a new Compile item,
# which one hop already saw) and a project in its transitive `ProjectReference` closure (which one
# hop did NOT see).
#
# S.I.R.#290's round-0 critic demonstrated the second against this item's own round-0 candidate:
# `saturate` extracted into a NEW project `src/SIR.Domain.Arith`, referenced from
# `SIR.Domain.fsproj`, passed all four steps of the production `rules` gate byte-identically to a
# clean tree. At that head the one-hop set was 9 projects of 25 -- 16 invisible. The transitive
# closure is 12, so the walk is bounded by the reference graph rather than sweeping the repository:
# `SIR.Tools` and the test projects are correctly not in it.
#
# Emits the project list on stdout; diagnostics go to stderr. A non-zero return means nothing may
# be concluded from the output -- it is never "the closure is empty".
closure_projects() {
  local tree_root=$1
  local rev=$2
  local declared_list=$3
  local project items status include target
  local frontier=()
  local seen=""

  while IFS= read -r project; do
    test -n "$project" || continue
    status=0
    items=$(project_elements "$tree_root" "$project" Compile "$rev") || status=$?
    if test "$status" -eq 1; then
      echo "project file could not be read at ${rev:-the working tree}: $project" >&2
      return 1
    fi
    test "$status" -eq 0 || continue
    if printf '%s\n' "$items" | grep -Fxq -f "$declared_list" 2>/dev/null; then
      frontier+=("$project")
      seen+="$project"$'\n'
    fi
  done <<< "$(project_inventory "$tree_root" "$rev")"

  # Transitive `ProjectReference` walk. `seen` terminates it, so a reference cycle -- which MSBuild
  # rejects but a malformed tree can still contain -- cannot spin here.
  while test ${#frontier[@]} -gt 0; do
    project="${frontier[0]}"
    frontier=("${frontier[@]:1}")
    status=0
    items=$(project_elements "$tree_root" "$project" ProjectReference "$rev") || status=$?
    if test "$status" -eq 1; then
      echo "project references could not be read at ${rev:-the working tree}: $project" >&2
      return 1
    fi
    test "$status" -eq 0 || continue
    while IFS= read -r include; do
      test -n "$include" || continue
      case "$include" in
        *'$('*)
          # An unevaluable reference is a refusal, never a skip: whatever it names would be invisible
          # to this arm, which is a hiding place for the extraction it exists to detect.
          echo "project reference carries an MSBuild expression this check cannot resolve: $include" >&2
          echo "  declared by: $project" >&2
          echo "  refusing rather than walking past a reference whose target cannot be identified" >&2
          return 1
          ;;
      esac
      target="$include"
      # A reference that resolves OUTSIDE the repository is not something this arm can reason about:
      # its baseline is `git show <sourceCommit>:<path>`, which has no meaning for a path above the
      # root. Refuse rather than walk it, for the same reason an MSBuild expression is refused.
      case "$target" in
        ../*|/*)
          echo "project reference resolves outside the repository: $target" >&2
          echo "  declared by: $project" >&2
          echo "  this arm's baseline is the sealed commit's tree, which cannot describe such a path" >&2
          return 1
          ;;
      esac
      printf '%s' "$seen" | grep -Fxq -- "$target" && continue
      status=0
      project_elements "$tree_root" "$target" Compile "$rev" >/dev/null || status=$?
      if test "$status" -eq 1; then
        echo "referenced project could not be read at ${rev:-the working tree}: $target" >&2
        return 1
      fi
      if test "$status" -eq 2; then
        # Absent at the BASELINE is ordinary (the project did not exist yet). Absent at HEAD means a
        # reference nothing can resolve, and "I could not evaluate this" is not "there is nothing
        # there" (#266).
        if test -z "$rev"; then
          echo "project reference names a project that is not in the tree: $target" >&2
          echo "  declared by: $project" >&2
          return 1
        fi
        continue
      fi
      seen+="$target"$'\n'
      frontier+=("$target")
    done <<< "$items"
  done

  # An EMPTY closure is an answer, and it must reach the caller as empty output at exit 0. Emitting
  # it through `grep -v` would exit 1 on no match and, under `pipefail`, turn "the closure is empty"
  # into "the walk failed" -- refusing with no diagnostic and pre-empting the vacuity guard that
  # exists to say so. Measured: that is exactly what the first cut of this repair did.
  test -n "$seen" || return 0
  printf '%s' "$seen" | sort -u | sed '/^$/d'
}

check_identity_closure_containment() {
  local sources_json=$1
  local correspondence_json=$2
  local commit=$3
  local tree_root=${4:-$repo_root}
  printf 'closure\t%s\t%s\t%s\n' "$tree_root" "$sources_json" "$correspondence_json" >> "$arm_invocations"
  local declared
  local acknowledged
  local acknowledged_status=0
  local register_shape
  local register_shape_status=0

  declared=$(jq -r '.sources[]' "$sources_json" | sort -u) || {
    echo "declared implementation identity set could not be read: $sources_json" >&2
    return 1
  }
  test -n "$declared" || {
    echo "declared implementation identity set is empty: nothing to contain" >&2
    return 1
  }

  # The register is read TYPE-FIRST, and its shape is decided over the complete set of six jq
  # types rather than over the shapes that happened to be tried. `.outsideIdentity` must be an
  # array whose every element is a string; anything else -- including absent (`null`) -- is a
  # refusal, never a silently empty register. An absent register reading as "nothing acknowledged"
  # would be indistinguishable from "acknowledged nothing", and only one of those is a decision.
  register_shape=$(jq -r '.outsideIdentity | type' "$correspondence_json") || register_shape_status=$?
  if test "$register_shape_status" -ne 0; then
    echo "recorded correspondence could not be evaluated for an .outsideIdentity register" >&2
    echo "  jq exited $register_shape_status over: $correspondence_json" >&2
    echo "  refusing rather than reporting a pass on input this check did not evaluate" >&2
    return 1
  fi
  if test "$register_shape" != array; then
    echo "recorded correspondence .outsideIdentity must be an array, and is: $register_shape" >&2
    echo "  an absent or wrongly typed register cannot be told apart from an empty one, and only" >&2
    echo "  an empty one is a decision. Declare it explicitly, as [] when nothing is acknowledged." >&2
    return 1
  fi
  local nonstring
  local nonstring_status=0
  nonstring=$(jq -r '.outsideIdentity | to_entries[] | select((.value | type) != "string") | "[\(.key)]\t\(.value | type)"' "$correspondence_json") || nonstring_status=$?
  if test "$nonstring_status" -ne 0; then
    echo "recorded correspondence .outsideIdentity could not be evaluated element-wise" >&2
    echo "  jq exited $nonstring_status over: $correspondence_json" >&2
    return 1
  fi
  if test -n "$nonstring"; then
    echo "recorded correspondence .outsideIdentity carries non-string entries:" >&2
    printf '%s\n' "$nonstring" | sed 's/^/  /' >&2
    return 1
  fi
  acknowledged=$(jq -r '.outsideIdentity[]' "$correspondence_json" | sort -u) || acknowledged_status=$?
  if test "$acknowledged_status" -ne 0; then
    echo "recorded correspondence .outsideIdentity could not be read" >&2
    return 1
  fi

  # The closure is resolved by READING Compile items and then WALKING ProjectReference, never by
  # naming a project by convention -- the same defect the rebind writer records having made twice
  # (S.I.R.#264 rounds 1 and 2) -- and never by stopping at the projects that compile a declared
  # source directly, which is the defect this item's own round-0 candidate shipped with.
  #
  # Each revision's closure is computed INDEPENDENTLY, at that revision. Using the current owning
  # set to read baseline content would silently assume the reference graph never changed, and the
  # graph changing is precisely the move under test.
  local declared_list
  local project
  local owning=()
  local current_projects
  local baseline_projects
  declared_list=$(mktemp /tmp/sir-rules-declared.XXXXXX)
  printf '%s\n' "$declared" > "$declared_list"
  current_projects=$(closure_projects "$tree_root" "" "$declared_list") || { rm -f "$declared_list"; return 1; }
  baseline_projects=$(closure_projects "$tree_root" "$commit" "$declared_list") || { rm -f "$declared_list"; return 1; }
  rm -f "$declared_list"

  while IFS= read -r project; do
    test -n "$project" || continue
    owning+=("$project")
  done <<< "$current_projects"

  # Vacuity: if no project compiles a declared source there is no closure to contain, and a pass
  # here would mean "checked nothing" rather than "found nothing".
  test ${#owning[@]} -gt 0 || {
    echo "no project compiles any declared implementation source: the closure check would examine nothing" >&2
    return 1
  }

  local current_items=""
  local baseline_items=""
  local status
  for project in "${owning[@]}"; do
    status=0
    current_items+=$(project_compile_items "$tree_root" "$project")$'\n' || status=$?
    test "$status" -ne 1 || { echo "project file could not be read: $project" >&2; return 1; }
  done
  while IFS= read -r project; do
    test -n "$project" || continue
    status=0
    baseline_items+=$(project_compile_items "$tree_root" "$project" "$commit")$'\n' || status=$?
    test "$status" -ne 1 || { echo "project file could not be read at $commit: $project" >&2; return 1; }
  done <<< "$baseline_projects"
  current_items=$(printf '%s' "$current_items" | grep -v '^$' | sort -u)
  baseline_items=$(printf '%s' "$baseline_items" | grep -v '^$' | sort -u)

  test -n "$baseline_items" || {
    echo "the rules implementation compile closure at $commit is empty: no baseline to contain against" >&2
    echo "  refusing rather than treating an unreadable baseline as 'nothing was there before'" >&2
    return 1
  }

  # A current compile item is contained when it is a declared implementation source, OR it was
  # already in the closure at the sealed commit, OR it is explicitly acknowledged as outside.
  local uncontained
  uncontained=$(comm -23 <(printf '%s\n' "$current_items") <(printf '%s\n' "$baseline_items") \
    | grep -Fxv -f <(printf '%s\n' "$declared") \
    | { test -n "$acknowledged" && grep -Fxv -f <(printf '%s\n' "$acknowledged") || command cat; } ) || true

  if test -n "$uncontained"; then
    echo "implementation entered the rules compile closure without joining correspondence coverage:" >&2
    printf '%s\n' "$uncontained" | sed 's/^/  /' >&2
    echo "  these files are compiled by a project in the rules implementation closure -- a project" >&2
    echo "  that compiles a declared implementation source, or one reachable from such a project" >&2
    echo "  through ProjectReference -- were not in that closure at the sealed commit $commit," >&2
    echo "  and are not declared sources." >&2
    echo "  Code extracted out of a declared source into one of them leaves correspondence coverage." >&2
    echo "  The identity set is FROZEN at sourceCommit and cannot grow in the change that creates a" >&2
    echo "  file, so adding them to .sources is not the remedy and will be refused. Either keep the" >&2
    echo "  implementation inside a declared source, or record each path in .outsideIdentity in" >&2
    echo "  tests/fixtures/rules-corpus/v2/source-correspondence.json to declare -- reviewably, in the" >&2
    echo "  diff -- that it is knowingly outside the sealed identity set." >&2
    return 1
  fi

  # A register entry that names nothing in the closure is stale: it would go on silently excusing a
  # path that no longer exists, so it is refused rather than tolerated.
  local stale
  stale=$(printf '%s\n' "$acknowledged" | grep -v '^$' | grep -Fxv -f <(printf '%s\n' "$current_items") || true)
  if test -n "$stale"; then
    echo "recorded correspondence .outsideIdentity names paths that are not in the rules compile closure:" >&2
    printf '%s\n' "$stale" | sed 's/^/  /' >&2
    echo "  a stale acknowledgement excuses nothing and hides the next one; remove it." >&2
    return 1
  fi
}

# Every declared source must have a blob AT $source_commit, and this is checked BEFORE the seal
# loop reads one. Without it the loop's `git show` aborts the whole script with a raw
#
#   fatal: path '<p>' exists on disk, but not in '<sourceCommit>'
#
# at exit 128, naming neither the manifest that declared the path nor anything the author can act
# on (S.I.R.#290). Two different mistakes land here and the diagnostic tells them apart, because
# their remedies are opposite: a path ADDED to `.sources` (the identity set cannot grow -- see
# `membershipRule`), and a declared path that no longer exists at `sourceCommit` (the seal's
# historical record is broken, which is a rebind of the immutable half).
#
# `cat-file -e <commit>:<path>` is the existence question by itself; `git show` is not, because it
# answers "does it exist?" and "what is in it?" with the same failure.
require_declared_sources_at_source_commit() {
  local sources_json=$1
  local commit=$2
  printf 'sealed-blob\t%s\t%s\n' "$sources_json" "$commit" >> "$arm_invocations"
  local artifact_path
  local absent=()
  while IFS= read -r artifact_path; do
    test -n "$artifact_path" || continue
    git -C "$repo_root" cat-file -e "$commit:$artifact_path" 2>/dev/null || absent+=("$artifact_path")
  done <<< "$(jq -r '.sources[]' "$sources_json")"
  test ${#absent[@]} -eq 0 || {
    echo "declared implementation source has no blob at the sealed source commit $commit:" >&2
    printf '  %s\n' "${absent[@]}" >&2
    echo "  the sealed implementation digest is computed from these blobs, so a path with none cannot be sealed." >&2
    echo "  if you ADDED this path to .sources: the identity set is frozen at sourceCommit and cannot grow" >&2
    echo "    in the change that creates the file -- sourceCommit must already be an ancestor of the canonical" >&2
    echo "    default branch, so a path a pull request creates has no blob there. Remove it from .sources and" >&2
    echo "    record it under .outsideIdentity in tests/fixtures/rules-corpus/v2/source-correspondence.json." >&2
    echo "  if this path was DECLARED and has since moved or been deleted: the seal's historical record is" >&2
    echo "    broken, and repairing it is a deliberate rebind of the immutable half of the pin." >&2
    echo "  see docs/executable-rules-corpus-architecture.md and implementation-sources.json .membershipRule" >&2
    return 1
  }
}

require_declared_sources_at_source_commit "$source_manifest" "$source_commit" || { rm -f "$source_digest_input"; exit 1; }

# The sealed identity digest is derived ONLY from blobs at $source_commit. The working tree
# contributes nothing to it, which is why rebinding correspondence leaves the seal, the manifest
# identity, and the generated corpus fixtures byte-identical.
while IFS= read -r artifact_path; do
  actual_artifact_sha=$(git -C "$repo_root" show "$source_commit:$artifact_path" | sha256sum | cut -d' ' -f1)
  printf '%s\t%s\n' "$artifact_path" "$actual_artifact_sha" >> "$source_digest_input"
done <<< "$(jq -r '.sources[]' "$source_manifest")"

check_correspondence_coverage "$source_manifest" "$correspondence_manifest" || { rm -f "$source_digest_input"; exit 1; }
enforce_source_correspondence "$source_manifest" "$correspondence_manifest" "$repo_root" || { rm -f "$source_digest_input"; exit 1; }
check_identity_closure_containment "$source_manifest" "$correspondence_manifest" "$source_commit" || { rm -f "$source_digest_input"; exit 1; }

# ---------------------------------------------------------------------------------------------
# Source-correspondence inversions (S.I.R.#264).
#
# These drive the production enforcement path -- enforce_source_correspondence and
# check_correspondence_coverage, the same functions the gate calls above -- against synthetic trees
# and synthetic correspondence documents, so a change that makes the real gate vacuous fails here
# rather than passing quietly.
#
# Five prove refusals fire. The sixth proves a LEGAL input still exists, which is the class the
# durability hardening lacked: work item #239 shipped four inversions for its new refusals across
# d76b477 and d1f6ea7, all four still pass, and every one proves a bad input is refused. None
# demonstrates that the operation the new precondition constrains -- rebinding the pin so a changed
# source can pass -- has any legal execution at all. That gap is what made this gate unsatisfiable
# from d76b477 (2026-08-20) until it was first exercised.
# ---------------------------------------------------------------------------------------------
pin_probe_dir=$(mktemp -d /tmp/sir-rules-pin-probe.XXXXXX)
pin_probe_log=$(mktemp /tmp/sir-rules-pin-probe-log.XXXXXX)

pin_probe_fail() {
  echo "$1" >&2
  rm -rf "$pin_probe_dir"
  rm -f "$pin_probe_log" "$source_digest_input" "$arm_invocations"
  exit 1
}

pin_probe_tree() {
  local tree_root=$1
  local artifact_path
  while IFS= read -r artifact_path; do
    mkdir -p "$tree_root/$(dirname "$artifact_path")"
    cp "$repo_root/$artifact_path" "$tree_root/$artifact_path"
  done <<< "$(jq -r '.sources[]' "$source_manifest")"
}

# 1. A changed non-rule-hosting identity subject is refused. This is the property an independent
#    critic required on item #194 after proving that hashing pinned objects alone let a changed
#    App.fs pass; narrowing enforcement would reopen it.
app_probe_tree="$pin_probe_dir/app-mutant"
mkdir -p "$app_probe_tree"
pin_probe_tree "$app_probe_tree"
printf '\n// implementation identity subject mutation\n' >> "$app_probe_tree/src/SIR.Client.Web/App.fs"
if enforce_source_correspondence "$source_manifest" "$correspondence_manifest" "$app_probe_tree" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "App.fs implementation source mutation unexpectedly passed"
fi
search_quiet 'current implementation source differs from package pin: src/SIR.Client.Web/App.fs' "$pin_probe_log" || {
  pin_probe_fail "App.fs implementation source mutation failed without the actionable pin diagnostic"
}

# 2. A non-metadata change to the rule-hosting source is refused.
combat_probe_tree="$pin_probe_dir/combat-mutant"
mkdir -p "$combat_probe_tree"
pin_probe_tree "$combat_probe_tree"
sed '0,/module CombatRules =/s//module CombatRules = \/\/ implementation identity subject mutation/' \
  "$repo_root/src/SIR.Simulation/CombatRules.fs" > "$combat_probe_tree/src/SIR.Simulation/CombatRules.fs"
if enforce_source_correspondence "$source_manifest" "$correspondence_manifest" "$combat_probe_tree" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "CombatRules.fs non-metadata source mutation unexpectedly passed"
fi
search_quiet 'current implementation source differs from package pin: src/SIR.Simulation/CombatRules.fs' "$pin_probe_log" || {
  pin_probe_fail "CombatRules.fs non-metadata source mutation failed without the actionable pin diagnostic"
}

# 3. A metadata-only identity rebind is still normalized away, so re-sealing the corpus does not
#    read as a source change.
metadata_probe_tree="$pin_probe_dir/combat-metadata"
mkdir -p "$metadata_probe_tree"
pin_probe_tree "$metadata_probe_tree"
sed -E 's/(Commit = ")[0-9a-f]{40}(" })/\10000000000000000000000000000000000000000\2/' \
  "$repo_root/src/SIR.Simulation/CombatRules.fs" > "$metadata_probe_tree/src/SIR.Simulation/CombatRules.fs"
enforce_source_correspondence "$source_manifest" "$correspondence_manifest" "$metadata_probe_tree" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "CombatRules.fs metadata-only source rebind was not normalized"
}

# 4. Coverage guard: a source cannot be unfrozen by deleting its recorded row, and a path that is
#    not a declared implementation source cannot be smuggled in.
dropped_correspondence="$pin_probe_dir/dropped-correspondence.json"
jq 'del(.paths["src/SIR.Client.Web/App.fs"])' "$correspondence_manifest" > "$dropped_correspondence"
if check_correspondence_coverage "$source_manifest" "$dropped_correspondence" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "dropping a recorded source correspondence unexpectedly passed"
fi
search_quiet 'declared implementation source with no recorded correspondence: src/SIR.Client.Web/App.fs' "$pin_probe_log" || {
  pin_probe_fail "dropped source correspondence failed without the actionable coverage diagnostic"
}

extra_correspondence="$pin_probe_dir/extra-correspondence.json"
jq '.paths["src/SIR.Domain/RuleTypes.fs"] = "0000000000000000000000000000000000000000000000000000000000000000"' \
  "$correspondence_manifest" > "$extra_correspondence"
if check_correspondence_coverage "$source_manifest" "$extra_correspondence" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "undeclared source correspondence entry unexpectedly passed"
fi
search_quiet 'recorded correspondence for a path that is not a declared implementation source: src/SIR.Domain/RuleTypes.fs' "$pin_probe_log" || {
  pin_probe_fail "undeclared source correspondence entry failed without the actionable coverage diagnostic"
}

# 5. An emptied baseline and a malformed digest are both refused.
emptied_correspondence="$pin_probe_dir/emptied-correspondence.json"
jq '.paths = {}' "$correspondence_manifest" > "$emptied_correspondence"
if check_correspondence_coverage "$source_manifest" "$emptied_correspondence" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "emptied source correspondence unexpectedly passed"
fi
search_quiet 'recorded source correspondence is empty' "$pin_probe_log" || {
  pin_probe_fail "emptied source correspondence failed without the actionable vacuity diagnostic"
}

# A malformed digest is refused for EVERY JSON type a digest can be, not merely for a string that
# does not look like a digest. `test/1` raises on any non-string, and this function is called on the
# left of `||`, so before S.I.R.#264's repair phase a non-string digest aborted the filter and the
# arm returned 0 -- a confident pass on input it had not evaluated. Only the string case below was
# ever exercised, which is why that survived four rounds of review.
#
# The six cases enumerated here are the COMPLETE set of JSON value types, so this inversion cannot
# be defeated by "a further value" the way an enumeration of observed literals could be.
malformed_correspondence="$pin_probe_dir/malformed-correspondence.json"
while IFS='|' read -r probe_label probe_value probe_type; do
  test -n "$probe_label" || continue
  jq --argjson injected "$probe_value" \
     '.paths["src/SIR.Domain/Rules.fs"] = $injected' \
     "$correspondence_manifest" > "$malformed_correspondence"

  # Guard the probe itself: assert the fixture really carries the type under test, so a probe that
  # silently stopped injecting could not pass by testing nothing.
  actual_type=$(jq -r '.paths["src/SIR.Domain/Rules.fs"] | type' "$malformed_correspondence")
  test "$actual_type" = "$probe_type" || {
    pin_probe_fail "malformed-digest probe '$probe_label' injected $actual_type, expected $probe_type"
  }

  if check_correspondence_coverage "$source_manifest" "$malformed_correspondence" >"$pin_probe_log" 2>&1; then
    pin_probe_fail "malformed source correspondence digest ($probe_label) unexpectedly passed"
  fi
  search_quiet 'recorded source correspondence carries malformed digests' "$pin_probe_log" || {
    pin_probe_fail "malformed source correspondence digest ($probe_label) failed without the actionable format diagnostic"
  }
  search_quiet "src/SIR.Domain/Rules.fs.*$probe_type" "$pin_probe_log" || {
    pin_probe_fail "malformed source correspondence digest ($probe_label) did not name the offending path and its type"
  }
done <<'MALFORMED_DIGEST_DOMAIN'
non-digest string|"not-a-sha256"|string
number|12345|number
null|null|null
boolean|true|boolean
array|["deadbeef"]|array
object|{"a":1}|object
MALFORMED_DIGEST_DOMAIN

# Unparseable correspondence is refused -- and this probe records WHICH check refuses it, because
# "the property is provided, but by a different check than the one named for it" is precisely the
# defect S.I.R.#264's repair phase exists to remove. Naming the wrong arm here would reproduce it.
#
# The refusal comes from the EMPTINESS arm above, not from the evaluability guard: for the input
# BELOW -- a document truncated mid-value -- `.paths | keys[]` produces nothing, leaving `recorded`
# empty.
#
# The sentence that used to stand here said more than that, and it was FALSE: "every file-level jq
# failure is caught there before the digest arm is ever reached." It is not. Valid JSON followed by
# unparseable TRAILING content is the counter-example, measured at 58041b8 on this very fixture:
# `.paths | keys[]` STREAMS all nineteen keys and then exits 5, so `recorded` is non-empty and equals
# the identity set, and BOTH arms above pass. The evaluability guard is what refuses it -- it is the
# arm under test for that input, not defence in depth against a hypothetical future edit.
#
# That mistake was recorded as known-open on S.I.R.#264 and ruled correctable when this file was next
# opened, on the ground that it is not MATERIAL: the production route never reaches this function with
# such input, because the schema read at the top of this script (`jq -r '.schema'`, not on the left of
# `||`) exits 5 under `set -e` first. That remains true and is re-measured at S.I.R.#290's base. The
# claim is corrected rather than deleted because a comment that names the wrong arm is how "the
# property is provided, but by a different check than the one named for it" survives review.
unreadable_correspondence="$pin_probe_dir/unreadable-correspondence.json"
printf '{"paths": {"src/SIR.Domain/Rules.fs": ' > "$unreadable_correspondence"
if check_correspondence_coverage "$source_manifest" "$unreadable_correspondence" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "unreadable source correspondence unexpectedly passed"
fi
search_quiet 'recorded source correspondence is empty' "$pin_probe_log" || {
  pin_probe_fail "unreadable source correspondence failed, but not through the emptiness arm this probe names"
}

# 6. A LEGITIMATE rebind succeeds. A genuinely changed implementation source, with its
#    correspondence rebound in the same commit, must pass -- otherwise no pull request touching a
#    pinned file could ever satisfy this gate, which is the defect S.I.R.#264 was filed for.
rebind_probe_tree="$pin_probe_dir/legitimate-rebind"
mkdir -p "$rebind_probe_tree"
pin_probe_tree "$rebind_probe_tree"
printf '\n// reviewed change to an implementation identity subject\n' >> "$rebind_probe_tree/src/SIR.Simulation/Simulation.fs"
if source_matches_correspondence "src/SIR.Simulation/Simulation.fs" "$rebind_probe_tree/src/SIR.Simulation/Simulation.fs" "$correspondence_manifest"; then
  pin_probe_fail "legitimate rebind probe did not actually change the implementation source"
fi
rebound_correspondence="$pin_probe_dir/rebound-correspondence.json"
jq --arg path "src/SIR.Simulation/Simulation.fs" \
   --arg digest "$(normalized_source_digest "src/SIR.Simulation/Simulation.fs" "$rebind_probe_tree/src/SIR.Simulation/Simulation.fs")" \
   '.paths[$path] = $digest' "$correspondence_manifest" > "$rebound_correspondence"
check_correspondence_coverage "$source_manifest" "$rebound_correspondence" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "a rebound source correspondence was refused by the coverage guard"
}
enforce_source_correspondence "$source_manifest" "$rebound_correspondence" "$rebind_probe_tree" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "a legitimate rebound implementation source change was refused: no pull request could satisfy this gate"
}

# 7. The rebind writer cannot widen the frozen set. The declared identity set is owned by
#    implementation-sources.json, and the writer must refuse a path outside it rather than adding
#    one -- otherwise the tool that maintains the baseline could also redefine what it covers.
if "$repo_root/scripts/rebind-rules-corpus-sources.sh" --write src/SIR.Domain/RuleTypes.fs >"$pin_probe_log" 2>&1; then
  pin_probe_fail "rebind writer accepted a path outside the declared implementation identity set"
fi
search_quiet 'not a declared implementation source: src/SIR.Domain/RuleTypes.fs' "$pin_probe_log" || {
  pin_probe_fail "rebind writer refused an undeclared path without the actionable ownership diagnostic"
}

# 8. The writer's normalization agrees with this verifier's. The writer necessarily carries its own
#    copy of normalize_implementation_source; if the two ever diverge it would record digests this
#    gate cannot reproduce, and every rebind would land broken. On a tree this gate considers
#    current, the writer must therefore find nothing to rebind.
"$repo_root/scripts/rebind-rules-corpus-sources.sh" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "rebind writer failed on a tree this gate considers current"
}
search_quiet 'already current: nothing to rebind' "$pin_probe_log" || {
  pin_probe_fail "rebind writer reports drift on a tree this gate considers current: the writer and verifier normalizations have diverged"
}

# ---------------------------------------------------------------------------------------------
# Identity-closure inversions (S.I.R.#290).
#
# Every case below drives check_identity_closure_containment -- the SAME function the gate calls in
# production -- so a change that makes the real arm vacuous fails here rather than passing quietly.
#
# The fixture is a copy of the REAL project files at their real relative paths, not a synthetic
# minimal project. That is deliberate: the real SIR.Simulation.fsproj carries two MSBuild property
# expressions in `Compile Include`, and a fixture without them would be a simpler world than
# production and would prove nothing about how the arm treats an include it cannot resolve
# (FS.GG.Templates#379). Detached and attached cases differ in EXACTLY one line, so each pair is a
# controlled experiment rather than two separately-authored worlds, and every refusal case is
# paired with a control that must go green -- otherwise a fixture that reds for the wrong reason
# would read as a passing demonstration.
closure_probe_tree() {
  local tree_root=$1
  local project
  while IFS= read -r project; do
    test -n "$project" || continue
    mkdir -p "$tree_root/$(dirname "$project")"
    cp "$repo_root/$project" "$tree_root/$project"
  done <<< "$(cd "$repo_root" && find src tests -name '*.fsproj' | sort)"
}

closure_attached="$pin_probe_dir/closure-attached"
closure_detached="$pin_probe_dir/closure-detached"
mkdir -p "$closure_attached" "$closure_detached"
closure_probe_tree "$closure_attached"
closure_probe_tree "$closure_detached"

# The one-line difference: an extraction target joins the project that compiles declared sources.
sed -i 's#<Compile Include="FixedPoint.fs" />#<Compile Include="ExtractedProbe.fs" />\n    <Compile Include="FixedPoint.fs" />#' \
  "$closure_detached/src/SIR.Domain/SIR.Domain.fsproj"
test "$(diff <(command cat "$closure_attached/src/SIR.Domain/SIR.Domain.fsproj") <(command cat "$closure_detached/src/SIR.Domain/SIR.Domain.fsproj") | grep -c '^>')" = 1 || {
  pin_probe_fail "closure probe pair differs by other than exactly one line: it is not a controlled experiment"
}

# 9. CONTROL first. The attached tree is the production tree's project set, unmodified, and it must
#    pass -- so a red in the detached case below is attributable to the one line that differs.
check_identity_closure_containment "$source_manifest" "$correspondence_manifest" "$source_commit" "$closure_attached" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "identity-closure control tree was refused: the probe pair is not a controlled experiment"
}

# 10. SUBJECT MUTATION. Extraction into a new compile item is refused and NAMED. This is the exact
#     move measured passing green at 58041b8 before this arm existed.
if check_identity_closure_containment "$source_manifest" "$correspondence_manifest" "$source_commit" "$closure_detached" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "implementation entering the rules compile closure unexpectedly passed"
fi
search_quiet 'implementation entered the rules compile closure without joining correspondence coverage' "$pin_probe_log" || {
  pin_probe_fail "closure escape failed without the actionable containment diagnostic"
}
search_quiet 'src/SIR.Domain/ExtractedProbe.fs' "$pin_probe_log" || {
  pin_probe_fail "closure escape diagnostic did not name the offending path"
}

# 11. A LEGAL input exists. An acknowledged path passes, so the arm is satisfiable and does not
#     merely refuse everything new -- the class of evidence #264 records the durability hardening
#     lacking, where four inversions all proved a refusal and none proved any legal execution.
acknowledged_correspondence="$pin_probe_dir/acknowledged-correspondence.json"
jq '.outsideIdentity += ["src/SIR.Domain/ExtractedProbe.fs"]' "$correspondence_manifest" > "$acknowledged_correspondence"
check_identity_closure_containment "$source_manifest" "$acknowledged_correspondence" "$source_commit" "$closure_detached" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "an explicitly acknowledged out-of-identity compile item was refused: no extraction could ever be declared"
}

# 12. A STALE acknowledgement is refused. Without this the register would silently accumulate
#     excuses for paths that no longer exist, and the next real escape would hide among them.
stale_correspondence="$pin_probe_dir/stale-correspondence.json"
jq '.outsideIdentity += ["src/SIR.Domain/NeverExisted.fs"]' "$correspondence_manifest" > "$stale_correspondence"
if check_identity_closure_containment "$source_manifest" "$stale_correspondence" "$source_commit" "$closure_attached" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "a stale .outsideIdentity acknowledgement unexpectedly passed"
fi
search_quiet 'names paths that are not in the rules compile closure' "$pin_probe_log" || {
  pin_probe_fail "stale acknowledgement failed without the actionable staleness diagnostic"
}

# 13. The register's TYPE domain, decided over the COMPLETE set of six jq types rather than over
#     the shapes that happened to be tried. Exactly one type -- array -- is legal; the other five
#     are refusals, and `null` covers the ABSENT field, which is the case that matters most: an
#     absent register read as "nothing acknowledged" is indistinguishable from a deliberate empty
#     one, and only one of those is a decision (#266).
register_type_probe="$pin_probe_dir/register-type-probe.json"
while IFS='|' read -r probe_label probe_filter probe_type; do
  test -n "$probe_label" || continue
  jq "$probe_filter" "$correspondence_manifest" > "$register_type_probe"
  actual_type=$(jq -r '.outsideIdentity | type' "$register_type_probe")
  test "$actual_type" = "$probe_type" || {
    pin_probe_fail "register-type probe '$probe_label' produced $actual_type, expected $probe_type"
  }
  if check_identity_closure_containment "$source_manifest" "$register_type_probe" "$source_commit" "$closure_attached" >"$pin_probe_log" 2>&1; then
    pin_probe_fail "an .outsideIdentity register of type $probe_type ($probe_label) unexpectedly passed"
  fi
  search_quiet 'must be an array, and is' "$pin_probe_log" || {
    pin_probe_fail "register-type probe '$probe_label' failed without the actionable shape diagnostic"
  }
done <<'REGISTER_TYPE_DOMAIN'
absent field|del(.outsideIdentity)|null
explicit null|.outsideIdentity = null|null
string|.outsideIdentity = "src/SIR.Domain/Whatever.fs"|string
number|.outsideIdentity = 12345|number
boolean|.outsideIdentity = true|boolean
object|.outsideIdentity = {"src/SIR.Domain/Whatever.fs": true}|object
REGISTER_TYPE_DOMAIN

# The legal type is proved legal, so the domain above is a partition and not merely six refusals.
jq '.outsideIdentity = .outsideIdentity' "$correspondence_manifest" > "$register_type_probe"
test "$(jq -r '.outsideIdentity | type' "$register_type_probe")" = array || {
  pin_probe_fail "register-type control did not produce an array"
}
check_identity_closure_containment "$source_manifest" "$register_type_probe" "$source_commit" "$closure_attached" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "an array-typed .outsideIdentity register was refused: the type domain refuses every type"
}

# 14. ELEMENT types, again over the complete set. A well-typed array whose ENTRIES are not strings
#     must be refused rather than stringified into a path nothing can match.
while IFS='|' read -r probe_label probe_value probe_type; do
  test -n "$probe_label" || continue
  jq --argjson injected "$probe_value" '.outsideIdentity += [$injected]' "$correspondence_manifest" > "$register_type_probe"
  actual_type=$(jq -r '.outsideIdentity[-1] | type' "$register_type_probe")
  test "$actual_type" = "$probe_type" || {
    pin_probe_fail "register-element probe '$probe_label' injected $actual_type, expected $probe_type"
  }
  if check_identity_closure_containment "$source_manifest" "$register_type_probe" "$source_commit" "$closure_attached" >"$pin_probe_log" 2>&1; then
    pin_probe_fail "an .outsideIdentity entry of type $probe_type ($probe_label) unexpectedly passed"
  fi
  search_quiet 'carries non-string entries' "$pin_probe_log" || {
    pin_probe_fail "register-element probe '$probe_label' failed without the actionable element diagnostic"
  }
  search_quiet "$probe_type" "$pin_probe_log" || {
    pin_probe_fail "register-element probe '$probe_label' did not name the offending entry's type"
  }
done <<'REGISTER_ELEMENT_DOMAIN'
number|12345|number
null|null|null
boolean|true|boolean
array|["nested"]|array
object|{"a":1}|object
REGISTER_ELEMENT_DOMAIN

# 15. Unparseable correspondence is refused by THIS arm's own evaluability guard, and the probe
#     records which arm refuses it. In production check_correspondence_coverage reaches such input
#     first; this case proves the containment arm does not depend on that ordering, because a
#     function that only fails closed when some earlier caller already did is not failing closed.
unreadable_closure="$pin_probe_dir/unreadable-closure.json"
printf '{"outsideIdentity": [' > "$unreadable_closure"
if check_identity_closure_containment "$source_manifest" "$unreadable_closure" "$source_commit" "$closure_attached" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "unparseable correspondence unexpectedly passed the identity-closure arm"
fi
search_quiet 'could not be evaluated for an .outsideIdentity register' "$pin_probe_log" || {
  pin_probe_fail "unparseable correspondence failed, but not through the evaluability guard this probe names"
}

# 16. Vacuity: a tree in which no project compiles a declared source must be REFUSED, not passed.
#     A closure containing nothing trivially contains no escape, and reporting that as a pass is
#     the "checked nothing" failure this whole mechanism exists to avoid.
empty_closure_tree="$pin_probe_dir/closure-empty"
mkdir -p "$empty_closure_tree"
if check_identity_closure_containment "$source_manifest" "$correspondence_manifest" "$source_commit" "$empty_closure_tree" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "a tree with no rules implementation project unexpectedly passed the identity-closure arm"
fi
search_quiet 'no project compiles any declared implementation source' "$pin_probe_log" || {
  pin_probe_fail "an empty closure failed without the actionable vacuity diagnostic"
}

# 17. A declared source with no blob at sourceCommit is refused with an ACTIONABLE diagnostic.
#     Before S.I.R.#290 this aborted the whole script at exit 128 with a raw `git fatal:` naming
#     neither the manifest nor a remedy. The control proves the guard passes the real manifest, so
#     the refusal below is attributable to the added path rather than to the guard refusing always.
#     The control uses a byte-identical COPY at a different path on purpose: calling it with the
#     real manifest would write the very ledger line the wiring guard (18) looks for, so this probe
#     would SUPPLY the property that guard is meant to observe from production, and deleting the
#     production call site would stay green. That was measured -- mutation M2 survived until this
#     copy was introduced -- and it is the "named by one test, provided by another" defect
#     (.github#2223 / FS.GG.Governance#385) reproduced inside this very repair.
real_sources_copy="$pin_probe_dir/real-sources-copy.json"
cp "$source_manifest" "$real_sources_copy"
cmp -s "$real_sources_copy" "$source_manifest" || pin_probe_fail "sealed-blob control copy is not byte-identical to the real manifest"
require_declared_sources_at_source_commit "$real_sources_copy" "$source_commit" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "the sealed-blob guard refused the real implementation identity set"
}
absent_blob_manifest="$pin_probe_dir/absent-blob-sources.json"
jq '.sources += ["src/SIR.Domain/NeverSealed.fs"]' "$source_manifest" > "$absent_blob_manifest"
if require_declared_sources_at_source_commit "$absent_blob_manifest" "$source_commit" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "a declared source with no blob at the sealed commit unexpectedly passed"
fi
search_quiet 'has no blob at the sealed source commit' "$pin_probe_log" || {
  pin_probe_fail "an unsealed declared source failed without the actionable diagnostic"
}
search_quiet 'the identity set is frozen at sourceCommit and cannot grow' "$pin_probe_log" || {
  pin_probe_fail "the unsealed-source diagnostic did not name the frozen-set remedy"
}

# ---------------------------------------------------------------------------------------------
# Identity-closure GRAPH inversions (S.I.R.#290 round 1).
#
# Round 0 computed the closure over ONE HOP -- the projects that directly list a declared source as
# a Compile item -- and its critic escaped it by extracting into a NEW PROJECT referenced from such
# a project. Nine projects of twenty-five were visible; the other sixteen were free ground.
#
# Every case below therefore exercises the WALK, not the nine. Each refusal case is paired with a
# control that must go green, and each pair differs by exactly one construct, so a fixture that reds
# for the wrong reason is not read as a passing demonstration.
graph_probe_tree() {
  local tree_root=$1
  local project
  while IFS= read -r project; do
    test -n "$project" || continue
    mkdir -p "$tree_root/$(dirname "$project")"
    cp "$repo_root/$project" "$tree_root/$project"
  done <<< "$(cd "$repo_root" && find src tests -name '*.fsproj' | sort)"
}

graph_add_project() {  # tree, dir, compile-item, [extra-xml]
  mkdir -p "$1/$2"
  cat > "$1/$2/$(basename "$2").fsproj" <<GRAPHPROJ
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <Compile Include="$3" />
  </ItemGroup>
  ${4:-}
</Project>
GRAPHPROJ
}

graph_reference_from() {  # project-file, reference-xml
  python3 - "$1" "$2" <<'GRAPHREF'
import sys
path, xml = sys.argv[1], sys.argv[2]
s = open(path).read()
assert '<Project' in s, path
s = s.replace('</Project>', '  <ItemGroup>\n    %s\n  </ItemGroup>\n</Project>' % xml, 1)
open(path, 'w').write(s)
GRAPHREF
}

graph_control="$pin_probe_dir/graph-control"
mkdir -p "$graph_control"
graph_probe_tree "$graph_control"

# 19. CONTROL. The unmodified project set must pass, so every red below is attributable.
check_identity_closure_containment "$source_manifest" "$correspondence_manifest" "$source_commit" "$graph_control" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "identity-closure graph control tree was refused: the probe pairs are not controlled experiments"
}

# 20. ONE HOP OUT -- the exact escape the round-0 candidate shipped with. A NEW project, referenced
#     from a project that compiles a declared source, is in the closure and its compile item is not.
graph_onehop="$pin_probe_dir/graph-onehop"
mkdir -p "$graph_onehop"
graph_probe_tree "$graph_onehop"
graph_add_project "$graph_onehop" "src/SIR.Probe.Arith" "ProbeArith.fs"
graph_reference_from "$graph_onehop/src/SIR.Domain/SIR.Domain.fsproj" '<ProjectReference Include="../SIR.Probe.Arith/SIR.Probe.Arith.fsproj" />'
if check_identity_closure_containment "$source_manifest" "$correspondence_manifest" "$source_commit" "$graph_onehop" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "extraction into a new project one ProjectReference hop out unexpectedly passed"
fi
search_quiet 'src/SIR.Probe.Arith/ProbeArith.fs' "$pin_probe_log" || {
  pin_probe_fail "one-hop closure escape failed without naming the offending path"
}

# 21. TRANSITIVE -- two hops out. This is the case a one-LEVEL fix would still miss, so it is what
#     separates "walks the graph" from "looks one further than before".
graph_transitive="$pin_probe_dir/graph-transitive"
mkdir -p "$graph_transitive"
graph_probe_tree "$graph_transitive"
graph_add_project "$graph_transitive" "src/SIR.Probe.Mid" "ProbeMid.fs" \
  '<ItemGroup><ProjectReference Include="../SIR.Probe.Deep/SIR.Probe.Deep.fsproj" /></ItemGroup>'
graph_add_project "$graph_transitive" "src/SIR.Probe.Deep" "ProbeDeep.fs"
graph_reference_from "$graph_transitive/src/SIR.Domain/SIR.Domain.fsproj" '<ProjectReference Include="../SIR.Probe.Mid/SIR.Probe.Mid.fsproj" />'
if check_identity_closure_containment "$source_manifest" "$correspondence_manifest" "$source_commit" "$graph_transitive" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "extraction two ProjectReference hops out unexpectedly passed"
fi
search_quiet 'src/SIR.Probe.Deep/ProbeDeep.fs' "$pin_probe_log" || {
  pin_probe_fail "the transitive closure escape did not reach the SECOND hop: the walk is not transitive"
}

# 22. A LEGAL input exists at graph distance too: declaring the two-hop path passes, so the walk
#     refuses an undeclared escape rather than refusing every reference graph that grows.
graph_ack="$pin_probe_dir/graph-acknowledged.json"
jq '.outsideIdentity += ["src/SIR.Probe.Mid/ProbeMid.fs", "src/SIR.Probe.Deep/ProbeDeep.fs"]' \
  "$correspondence_manifest" > "$graph_ack"
check_identity_closure_containment "$source_manifest" "$graph_ack" "$source_commit" "$graph_transitive" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "an explicitly acknowledged out-of-identity compile item two hops out was refused"
}

# 23. ATTRIBUTE ORDER. `Include` is an attribute, and XML does not order attributes. A substring
#     matcher for `<ProjectReference Include="` sees the first spelling below and not the second,
#     which would make attribute order a hiding place for the escape in probe 20.
graph_attrorder="$pin_probe_dir/graph-attrorder"
mkdir -p "$graph_attrorder"
graph_probe_tree "$graph_attrorder"
graph_add_project "$graph_attrorder" "src/SIR.Probe.Attr" "ProbeAttr.fs"
graph_reference_from "$graph_attrorder/src/SIR.Domain/SIR.Domain.fsproj" '<ProjectReference Condition="'"'"'$(Unset)'"'"' == '"'"''"'"'" Include="../SIR.Probe.Attr/SIR.Probe.Attr.fsproj" />'
if check_identity_closure_containment "$source_manifest" "$correspondence_manifest" "$source_commit" "$graph_attrorder" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "a ProjectReference with Include after another attribute was not walked: attribute order is a hiding place"
fi
search_quiet 'src/SIR.Probe.Attr/ProbeAttr.fs' "$pin_probe_log" || {
  pin_probe_fail "attribute-ordered ProjectReference escape did not name the offending path"
}

# 24. An UNEVALUABLE reference is refused, not walked past. Whatever an MSBuild expression names is
#     invisible to this arm, and "I could not evaluate this" is never "there is nothing there".
graph_expr="$pin_probe_dir/graph-expression"
mkdir -p "$graph_expr"
graph_probe_tree "$graph_expr"
graph_reference_from "$graph_expr/src/SIR.Domain/SIR.Domain.fsproj" '<ProjectReference Include="$(SomeUnresolvedProject)" />'
if check_identity_closure_containment "$source_manifest" "$correspondence_manifest" "$source_commit" "$graph_expr" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "a ProjectReference carrying an MSBuild expression unexpectedly passed"
fi
search_quiet 'carries an MSBuild expression this check cannot resolve' "$pin_probe_log" || {
  pin_probe_fail "an unevaluable ProjectReference failed without the actionable resolution diagnostic"
}

# 25. A reference naming a project that is NOT THERE is refused at HEAD. A missing target silently
#     skipped is the same hiding place by another route.
graph_missing="$pin_probe_dir/graph-missing"
mkdir -p "$graph_missing"
graph_probe_tree "$graph_missing"
graph_reference_from "$graph_missing/src/SIR.Domain/SIR.Domain.fsproj" '<ProjectReference Include="../SIR.Probe.Absent/SIR.Probe.Absent.fsproj" />'
if check_identity_closure_containment "$source_manifest" "$correspondence_manifest" "$source_commit" "$graph_missing" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "a ProjectReference naming an absent project unexpectedly passed"
fi
search_quiet 'names a project that is not in the tree' "$pin_probe_log" || {
  pin_probe_fail "an absent ProjectReference target failed without the actionable diagnostic"
}

# 26. A reference resolving OUTSIDE the repository is refused. This arm's baseline is the sealed
#     commit's tree, which cannot describe such a path, so walking it would compare against nothing.
graph_outside="$pin_probe_dir/graph-outside"
mkdir -p "$graph_outside"
graph_probe_tree "$graph_outside"
# Three levels, not two: from src/SIR.Domain, `../..` lands back ON the root, and `realpath -m`
# normalizes it to an in-repo-looking path. Only the third `..` actually leaves. Measured, because
# the first cut of this probe used two and was demonstrating the wrong thing entirely.
graph_reference_from "$graph_outside/src/SIR.Domain/SIR.Domain.fsproj" '<ProjectReference Include="../../../Elsewhere/Elsewhere.fsproj" />'
if check_identity_closure_containment "$source_manifest" "$correspondence_manifest" "$source_commit" "$graph_outside" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "a ProjectReference resolving outside the repository unexpectedly passed"
fi
search_quiet 'resolves outside the repository' "$pin_probe_log" || {
  pin_probe_fail "an out-of-repository ProjectReference failed without the actionable diagnostic"
}

# 27. A reference CYCLE terminates. MSBuild rejects cycles, but this arm reads files rather than
#     building, so a malformed tree must not spin it. The `seen` set is what bounds the walk, and a
#     probe that hangs is indistinguishable from a gate that hangs.
graph_cycle="$pin_probe_dir/graph-cycle"
mkdir -p "$graph_cycle"
graph_probe_tree "$graph_cycle"
graph_add_project "$graph_cycle" "src/SIR.Probe.A" "ProbeA.fs" \
  '<ItemGroup><ProjectReference Include="../SIR.Probe.B/SIR.Probe.B.fsproj" /></ItemGroup>'
graph_add_project "$graph_cycle" "src/SIR.Probe.B" "ProbeB.fs" \
  '<ItemGroup><ProjectReference Include="../SIR.Probe.A/SIR.Probe.A.fsproj" /></ItemGroup>'
graph_reference_from "$graph_cycle/src/SIR.Domain/SIR.Domain.fsproj" '<ProjectReference Include="../SIR.Probe.A/SIR.Probe.A.fsproj" />'
graph_cycle_status=0
timeout 120 bash -c 'true' >/dev/null 2>&1 || pin_probe_fail "the cycle probe needs a working timeout(1)"
if timeout 120 bash -c "
  set -euo pipefail
  repo_root='$repo_root'
  arm_invocations=\$(mktemp)
  $(declare -f project_elements project_compile_items project_inventory closure_projects check_identity_closure_containment)
  check_identity_closure_containment '$source_manifest' '$correspondence_manifest' '$source_commit' '$graph_cycle'
" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "a reference cycle unexpectedly passed the identity-closure arm"
else
  graph_cycle_status=$?
fi
test "$graph_cycle_status" -ne 124 || {
  pin_probe_fail "the identity-closure walk did not terminate on a reference cycle: it spun until timeout"
}
search_quiet 'src/SIR.Probe.A/ProbeA.fs' "$pin_probe_log" || {
  pin_probe_fail "the cycle probe terminated but did not report the closure members it found"
}

# 28. The walk is proved over the GRAPH, not over the nine projects that compile a declared source
#     directly. A repair verified only against those nine leaves the same sixteen invisible, which
#     is how the round-0 defect passed four green gates.
graph_direct_count=0
graph_closure_count=0
graph_declared_list=$(mktemp /tmp/sir-rules-graph-declared.XXXXXX)
jq -r '.sources[]' "$source_manifest" | sort -u > "$graph_declared_list"
while IFS= read -r probe_project; do
  test -n "$probe_project" || continue
  if project_compile_items "$repo_root" "$probe_project" 2>/dev/null | grep -Fxq -f "$graph_declared_list" 2>/dev/null; then
    graph_direct_count=$((graph_direct_count + 1))
  fi
done <<< "$(project_inventory "$repo_root" "")"
graph_closure_count=$(closure_projects "$repo_root" "" "$graph_declared_list" | grep -c .)
rm -f "$graph_declared_list"
test "$graph_closure_count" -gt "$graph_direct_count" || {
  pin_probe_fail "the closure walk reaches no further than the directly-compiling projects ($graph_direct_count): ProjectReference is not being followed"
}

# 18. WIRING. Both S.I.R.#290 arms must have run against the REAL tree and the REAL manifests
#     during this invocation, not merely against probe fixtures. Deleting either production call
#     site reds here, which is the property probes 9-17 cannot provide on their own because they
#     drive the functions directly.
grep -Fxq "$(printf 'closure\t%s\t%s\t%s' "$repo_root" "$source_manifest" "$correspondence_manifest")" "$arm_invocations" || {
  echo "the identity-closure arm never ran against the real tree in this invocation" >&2
  echo "  its production call site has been removed or re-pointed; the probes below it then" >&2
  echo "  demonstrate a refusal this gate no longer performs" >&2
  pin_probe_fail "identity-closure arm is not wired into the production path"
}
grep -Fxq "$(printf 'sealed-blob\t%s\t%s' "$source_manifest" "$source_commit")" "$arm_invocations" || {
  echo "the sealed-blob guard never ran against the real implementation identity set" >&2
  echo "  its production call site has been removed or re-pointed, so a declared source with no" >&2
  echo "  blob at sourceCommit would again abort at exit 128 with a raw git fatal" >&2
  pin_probe_fail "sealed-blob guard is not wired into the production path"
}

rm -rf "$pin_probe_dir"
rm -f "$pin_probe_log" "$arm_invocations"
printf 'package\t%s\nalgorithm\t%s\n' "$(jq -r '.packageSha256' "$source_manifest")" "$(jq -r '.algorithmFingerprint' "$source_manifest")" >> "$source_digest_input"
actual_sources_digest=$(sha256sum "$source_digest_input" | cut -d' ' -f1)
identity_mutant=$(mktemp /tmp/sir-rules-source-digest-mutant.XXXXXX)
sed 's#^src/SIR.Domain/Rules.fs\t[0-9a-f]\{64\}$#src/SIR.Domain/Rules.fs\t0000000000000000000000000000000000000000000000000000000000000000#' \
  "$source_digest_input" > "$identity_mutant"
mutated_sources_digest=$(sha256sum "$identity_mutant" | cut -d' ' -f1)
rm -f "$identity_mutant"
rm -f "$source_digest_input"
declared_sources_digest=$(sed -n 's/.*"implementation", System.Text.Encoding.UTF8.GetBytes "\([0-9a-f]\{64\}\)".*/\1/p' "$repo_root/src/SIR.Simulation/CombatRules.fs")
test "$declared_sources_digest" = "$actual_sources_digest" || { echo "implementation source manifest digest does not match pinned sources" >&2; exit 1; }
test "$declared_sources_digest" != "$mutated_sources_digest" || { echo "implementation identity source mutation unexpectedly passed" >&2; exit 1; }
declared_package_sha=$(jq -r '.packageSha256' "$source_manifest")
captured_package_sha=$(jq -r '.sha256' "$repo_root/docs/dependency-surface/FS.GG.Game.Core/0.13.0.json")
test "$declared_package_sha" = "$captured_package_sha" || { echo "Game.Core implementation fingerprint does not match dependency receipt" >&2; exit 1; }
test "$(jq -r '.algorithmFingerprint' "$source_manifest")" = "FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover" || { echo "Game.Core algorithm fingerprint changed" >&2; exit 1; }

copied_semantics_pattern='(baseDamage|expectedDamage).*(trace|retention)|(trace|retention).*(baseDamage|expectedDamage)'
if test "${SIR_RULES_FORCE_GREP:-0}" != 1 && command -v rg >/dev/null 2>&1; then
  copied_semantics=$(rg -n --glob '*.js' --glob '*.ts' --glob '!**/.fable*/**' "$copied_semantics_pattern" "$repo_root/src" || true)
else
  copied_semantics=$(find "$repo_root/src" -type f \( -name '*.js' -o -name '*.ts' \) ! -path '*/.fable*/*' -exec grep -EnH -- "$copied_semantics_pattern" {} + || true)
fi
if test -n "$copied_semantics"; then
  printf '%s\n' "$copied_semantics"
  echo "copied JavaScript/TypeScript combat semantics detected" >&2
  exit 1
fi

# ---------------------------------------------------------------------------------------------
# Execute the corpus (S.I.R.#264 round 1).
#
# A rebindable correspondence baseline CANNOT be the only thing standing between a changed
# implementation source and a green gate. Everything above this line is a comparison of
# DECLARATIONS: regenerated manifest/coverage/representative-application, sealed digests, recorded
# per-path text digests. A change to an algorithm BODY moves none of them -- `implementationDigest`
# is a sealed literal, `semanticDigest` derives from it plus the DECLARATIVE rule payload, and the
# representative application does not exercise every registered symbol. Byte identity against the
# pin used to be the only detector of that class, and making it rebindable retires it.
#
# So execute the rules rather than only re-describing them. An independent critic demonstrated the
# gap on this pull request: mutating CombatRules.resolveCoverImpact -- the symbol manifest.json
# binds to COMBAT-COVER-003 and COMBAT-COVER-DESTRUCTION-001 -- and rebinding its correspondence in
# the same tree left every declared artifact byte-identical and this gate green, while the corpus
# fixtures refused. This step is what makes the rebind path safe, and it is deliberately NOT
# restricted to rule-hosting paths: FixedPoint.fs, CanonicalEncoding.fs, Rules.fs and CombatModel.fs
# are all pinned, none is rule-hosting, and a damage-arithmetic change in any of them moves rule
# behaviour while remaining rebindable.
conformance_log=$(mktemp /tmp/sir-rules-conformance.XXXXXX)
if ! dotnet run --project "$project" -c Release --no-build >"$conformance_log" 2>&1; then
  echo "registered executable behaviour does not satisfy the rules corpus fixtures" >&2
  echo "  a rebound source correspondence cannot make this pass: the corpus is executed, not described" >&2
  grep -iE 'exception|failwith|did not' "$conformance_log" | head -5 >&2
  rm -f "$conformance_log"
  exit 1
fi
rm -f "$conformance_log"

# And prove that execution is not vacuous: a divergence injected into the combat route -- the same
# class as the critic's mutation, without needing a rebuild -- must be refused.
#
# The exit code alone CANNOT establish that. `--inject-combat-divergence` computes its offset with
# Array.findIndex, which THROWS when the two evaluations agree, and then failwiths regardless -- so
# the process aborts whether or not a divergence was found, and an exit-code-only check passes even
# when its subject is broken. That is the vacuity this whole gate exists to refuse, and the first
# version of this guard had it (S.I.R.#264 review round 1). Assert the diagnostic, exactly as the
# adjacent rules-corpus mutation below does: if CombatFixtures.evaluate stops diverging, findIndex
# throws before printing and this line is absent.
combat_divergence_log=$(mktemp /tmp/sir-rules-combat-divergence.XXXXXX)
if dotnet run --project "$project" -c Release --no-build -- --inject-combat-divergence >"$combat_divergence_log" 2>&1; then
  echo "combat divergence mutation unexpectedly passed the corpus conformance route" >&2
  rm -f "$combat_divergence_log"
  exit 1
fi
search_quiet 'first divergence: fixture=physical-combat' "$combat_divergence_log" || {
  echo "combat divergence mutation failed without the actionable divergence diagnostic" >&2
  echo "  the injected mutation did not actually diverge, so this guard proved nothing" >&2
  rm -f "$combat_divergence_log"
  exit 1
}
rm -f "$combat_divergence_log"

mutation_log=$(mktemp /tmp/sir-rules-mutation.XXXXXX)
trap 'rm -f "$mutation_log"' EXIT
if dotnet run --project "$project" -c Release --no-build -- --inject-rules-corpus-divergence >"$mutation_log" 2>&1; then
  echo "rules-corpus protected-subject mutation unexpectedly passed" >&2
  exit 1
fi
search_quiet 'first divergence: fixture=rules-corpus' "$mutation_log" || {
  echo "rules-corpus mutation failed without the actionable divergence diagnostic" >&2
  exit 1
}

echo "rules corpus generation, source resolution, copied-semantics, and mutation gates passed"
