import assert from "node:assert/strict";
import { readFileSync } from "node:fs";

const pages = readFileSync(new URL("../.github/workflows/pages.yml", import.meta.url), "utf8");
const ci = readFileSync(new URL("../.github/workflows/ci.yml", import.meta.url), "utf8");

assert.match(pages, /workflow_run:\n    workflows: \[CI\]\n    types: \[completed\]\n    branches: \[main\]/u);
assert.match(pages, /workflow_run\.conclusion == 'success'/u);
assert.match(pages, /workflow_run\.event == 'push'/u);
assert.match(pages, /workflow_run\.head_branch == 'main'/u);
assert.match(pages, /^  select-qualified-site:$/mu);
assert.match(pages, /outputs:\n      deploy: \$\{\{ steps\.route\.outputs\.deploy \}\}/u);
assert.match(pages, /ci-route\.mjs verify-route[\s\S]*selectedGates \| index\("documentation"\)[\s\S]*deploy=true[\s\S]*deploy=false/u);
assert.match(pages, /^  deploy-qualified-site:\n    if: needs\.select-qualified-site\.outputs\.deploy == 'true'\n    needs: select-qualified-site/mu);
assert.match(pages, /ref: \$\{\{ github\.event\.workflow_run\.head_sha \}\}/u);
assert.match(pages, /name: protected-qualified-site/u);
assert.match(pages, /run-id: \$\{\{ github\.event\.workflow_run\.id \}\}/u);
assert.match(pages, /test '\$\{\{ github\.event\.workflow_run\.head_sha \}\}' = "\$\(git rev-parse HEAD\)"/u);
assert.match(pages, /production-build-receipt\.mjs verify[\s\S]*--owner-command scripts\/qualify-pr\.sh[\s\S]*--receipt "\$\(<artifacts\/ci\/parts\/docs\.receipt\.path\)"/u);
assert.match(pages, /name: protected-qualified-site\n          path: artifacts\/qualified-site-handoff/u);
assert.match(pages, /while IFS= read -r path; do[\s\S]*artifacts\/ci\/parts\/receipts\/\*\.json[\s\S]*tar -xf "\$archive" --no-same-owner/u);
assert.match(ci, /handoff=artifacts\/qualified-site-handoff/u);
assert.match(ci, /if: success\(\) && github\.event_name == 'push'[\s\S]*tar -cf "\$handoff\/protected-qualified-site\.tar" --[\s\S]*artifacts\/site[\s\S]*artifacts\/ci\/route\.json[\s\S]*artifacts\/ci\/results\/documentation\.json[\s\S]*docs\.receipt\.path[\s\S]*"\$receipt"/u);
assert.match(ci, /name: protected-qualified-site[\s\S]*path: artifacts\/qualified-site-handoff/u);
assert.match(pages, /documentation\.json[\s\S]*\.status == "pass"[\s\S]*\.routeDigest == \$route/u);
assert.match(pages, /permissions:\n      actions: read\n      contents: read\n      pages: write\n      id-token: write/u);
assert.doesNotMatch(pages, /npm ci|build-docs\.sh|fsdocs build|dotnet build/u);

console.log("Pages selects only an exact documentation route, consumes its routed site/build/gate receipts, and keeps deployment permissions out of the selector.");
