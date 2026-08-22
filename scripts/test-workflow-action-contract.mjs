import assert from "node:assert/strict";
import { readFileSync } from "node:fs";

const workflows = ["ci.yml", "pages.yml", "ci-cost-observer.yml"].map((name) => ({ name, text: readFileSync(new URL(`../.github/workflows/${name}`, import.meta.url), "utf8") }));
const actions = new Map([
  ["actions/checkout", ["3d3c42e5aac5ba805825da76410c181273ba90b1", "v7.0.1"]],
  ["actions/setup-node", ["820762786026740c76f36085b0efc47a31fe5020", "v7.0.0"]],
  ["actions/setup-dotnet", ["a98b56852c35b8e3190ac28c8c2271da59106c68", "v6.0.0"]],
  ["actions/cache", ["55cc8345863c7cc4c66a329aec7e433d2d1c52a9", "v6.1.0"]],
  ["actions/upload-artifact", ["043fb46d1a93c77aae656e7c1c64a875d1fc6a0a", "v7.0.1"]],
  ["actions/download-artifact", ["3e5f45b2cfb9172054b4087a40e8e0b5a5461e7c", "v8.0.1"]],
  ["actions/configure-pages", ["45bfe0192ca1faeb007ade9deae92b16b8254a0d", "v6.0.0"]],
  ["actions/upload-pages-artifact", ["fc324d3547104276b827a68afc52ff2a11cc49c9", "v5.0.0"]],
  ["actions/deploy-pages", ["cd2ce8fcbc39b97be8ca5fce6e763baed58fa128", "v5.0.0"]],
]);

for (const { name, text } of workflows) {
  const uses = [...text.matchAll(/uses: ([^\s@]+)@([^\s]+) # (v[^\s]+)/gu)];
  assert.ok(uses.length > 0, `${name} has no pinned action inventory`);
  const localWorkflowCalls = [...text.matchAll(/uses: (\.\/\.github\/workflows\/[a-z0-9-]+\.yml)$/gmu)];
  assert.equal((text.match(/uses:/gu) ?? []).length, uses.length + localWorkflowCalls.length, `${name} contains an unparseable or unpinned action`);
  for (const [, action, digest, version] of uses) {
    assert.ok(actions.has(action), `${name} uses an unapproved action ${action}`);
    assert.deepEqual([digest, version], actions.get(action), `${name} action pin drifted for ${action}`);
  }
  const jobsText = text.slice(text.indexOf("\njobs:\n") + 7);
  const headers = [...jobsText.matchAll(/^  ([a-z0-9-]+):$/gmu)];
  assert.ok(headers.length > 0, `${name} has no jobs`);
  for (let index = 0; index < headers.length; index += 1) {
    const start = headers[index].index;
    const end = headers[index + 1]?.index ?? jobsText.length;
    const body = jobsText.slice(start, end);
    if (/^    uses: \.\/\.github\/workflows\//mu.test(body)) {
      assert.doesNotMatch(body, /^    runs-on:/mu, `${name}:${headers[index][1]} reusable workflow call owns a runner`);
      assert.match(body, /^    permissions:\n      actions: read\n      contents: read$/mu, `${name}:${headers[index][1]} reusable observer permissions drifted`);
    } else {
      assert.match(body, /^    runs-on: ubuntu-latest$/mu, `${name}:${headers[index][1]} runner drifted`);
      assert.match(body, /^    timeout-minutes: (?:10|30)$/mu, `${name}:${headers[index][1]} has no explicit timeout`);
    }
  }
}

assert.match(workflows.find(({ name }) => name === "ci.yml").text, /permissions:\n  contents: read/u);
assert.doesNotMatch(workflows.find(({ name }) => name === "ci.yml").text, /contents: write|issues: write|pull-requests: write/u);
assert.match(workflows.find(({ name }) => name === "pages.yml").text, /pages: write\n      id-token: write/u);
assert.doesNotMatch(workflows.find(({ name }) => name === "ci-cost-observer.yml").text, /contents: write|pages: write|id-token: write/u);
assert.match(workflows.find(({ name }) => name === "ci.yml").text, /cost-observer:\n[\s\S]*needs: pr-verdict[\s\S]*permissions:\n      actions: read\n      contents: read[\s\S]*--active-observer-job-name cost-observer/u);
console.log("Official actions are supported full-SHA pins with version comments; every job has an explicit timeout and workflows retain least permissions.");
