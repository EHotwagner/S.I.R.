#!/usr/bin/env node

import assert from "node:assert/strict";
import { mkdtemp, mkdir, readFile, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { spawnSync } from "node:child_process";

const testRoot = await mkdtemp(join(tmpdir(), "sir-design-transfer-"));
await mkdir(join(testRoot, "downloads"));
await writeFile(
  join(testRoot, "package.json"),
  `${JSON.stringify({ name: "sir" }, null, 2)}\n`,
  "utf8",
);

const bundle = {
  format: "sir-map-editor-design",
  version: 1,
  name: "Bridge & Keep",
  editor: {
    digest: "editor-digest",
    revision: 7,
    map: "SIR-MAP 2\nname Bridge & Keep\n",
  },
  simulator: {
    digest: "simulator-digest",
    tick: 12,
    map: "SIR-MAP 2\nname Bridge & Keep Simulator\n",
  },
};
const bundlePath = join(
  testRoot,
  "downloads",
  "bridge-keep.sir-design.json",
);
await writeFile(bundlePath, `${JSON.stringify(bundle, null, 2)}\n`, "utf8");

const importer = resolve("scripts/import-map-editor-design.mjs");
const result = spawnSync(process.execPath, [importer, bundlePath], {
  cwd: testRoot,
  encoding: "utf8",
});

assert.equal(result.status, 0, result.stderr);
const destination = join(
  testRoot,
  "designs",
  "map-editor",
  "bridge-keep",
);
assert.equal(
  await readFile(join(destination, "map.sir-map"), "utf8"),
  bundle.editor.map,
);
assert.equal(
  await readFile(join(destination, "simulator.sir-map"), "utf8"),
  bundle.simulator.map,
);
assert.deepEqual(
  JSON.parse(
    await readFile(join(destination, "design.sir-design.json"), "utf8"),
  ),
  bundle,
);

console.log("Map editor repository bundle import passed.");
