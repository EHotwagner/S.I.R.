import assert from "node:assert/strict";
import { readFileSync } from "node:fs";

const pages = readFileSync(new URL("../.github/workflows/pages.yml", import.meta.url), "utf8");
const ci = readFileSync(new URL("../.github/workflows/ci.yml", import.meta.url), "utf8");

assert.match(pages, /workflow_run:\n    workflows: \[CI\]\n    types: \[completed\]\n    branches: \[main\]/u);
assert.match(pages, /workflow_run\.conclusion == 'success'/u);
assert.match(pages, /workflow_run\.event == 'push'/u);
assert.match(pages, /workflow_run\.head_branch == 'main'/u);
assert.match(pages, /ref: \$\{\{ github\.event\.workflow_run\.head_sha \}\}/u);
assert.match(pages, /name: protected-qualified-site/u);
assert.match(pages, /run-id: \$\{\{ github\.event\.workflow_run\.id \}\}/u);
assert.match(pages, /test '\$\{\{ github\.event\.workflow_run\.head_sha \}\}' = "\$\(git rev-parse HEAD\)"/u);
assert.match(pages, /production-build-receipt\.mjs verify[\s\S]*--receipt "\$\(<artifacts\/qualification\/site-receipt\.path\)"/u);
assert.match(ci, /staged_receipt="artifacts\/qualification\/site-receipts\/\$\(basename "\$source_receipt"\)"/u);
assert.match(ci, /printf '%s\\n' "\$staged_receipt" > artifacts\/qualification\/site-receipt\.path/u);
assert.match(ci, /name: protected-qualified-site[\s\S]*artifacts\/qualification\/site-receipts[\s\S]*artifacts\/site/u);
assert.doesNotMatch(ci, /site-receipt\.json/u);
assert.match(pages, /permissions:\n      actions: read\n      contents: read\n      pages: write\n      id-token: write/u);
assert.doesNotMatch(pages, /npm ci|build-docs\.sh|fsdocs build|dotnet build/u);

console.log("Pages consumes only the exact successful protected-main site receipt with deploy-only permissions and no rebuild path.");
