#!/usr/bin/env node
import { spawn } from "node:child_process";
import { existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { availableParallelism, tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import process from "node:process";

const root = resolve(import.meta.dirname, "..");
const browserShardCapacity = Math.max(1, availableParallelism());
const configuredBrowserShards = process.env.SIR_BROWSER_SHARDS;
const browserShards = configuredBrowserShards === undefined
  ? (process.env.CI ? Math.min(2, browserShardCapacity) : 1)
  : Number(configuredBrowserShards);
if (!Number.isSafeInteger(browserShards) || browserShards < 1 || browserShards > 2 || browserShards > browserShardCapacity) {
  throw new Error(`SIR_BROWSER_SHARDS must be 1 or 2 and no greater than the machine capacity (${browserShardCapacity}).`);
}

const output = resolve(root, process.env.SIR_JUNIT_OUTPUT ?? "artifacts/test-results/browser.junit.xml");
const shardRoot = mkdtempSync(join(tmpdir(), "sir-browser-shards-"));
const shardReports = Array.from({ length: browserShards }, (_, offset) => join(shardRoot, `browser-${offset + 1}.junit.xml`));

const runShard = (offset) => new Promise((complete) => {
  const index = offset + 1;
  const args = [
    resolve(root, "node_modules/@playwright/test/cli.js"),
    "test",
    "--config",
    "tests/SIR.Browser.Tests/playwright.config.js",
  ];
  if (browserShards > 1) args.push(`--shard=${index}/${browserShards}`);
  const child = spawn(process.execPath, args, {
    cwd: root,
    env: {
      ...process.env,
      SIR_BROWSER_PORT: String(5100 + index - 1),
      SIR_JUNIT_OUTPUT: shardReports[offset],
    },
    stdio: "inherit",
  });
  child.on("error", (error) => complete({ index, code: 1, error }));
  child.on("exit", (code, signal) => complete({ index, code: code ?? 1, signal }));
});

const mergeShardReports = (paths) => {
  const cases = paths.flatMap((path) => {
    if (!existsSync(path)) throw new Error(`browser shard did not write deterministic JUnit: ${path}`);
    return readFileSync(path, "utf8").match(/  <testcase[\s\S]*?<\/testcase>/gu) ?? [];
  }).sort((left, right) => left.localeCompare(right));
  const failures = cases.filter((value) => value.includes("<failure ")).length;
  const skipped = cases.filter((value) => value.includes("<skipped/>")).length;
  const report = [
    '<?xml version="1.0" encoding="UTF-8"?>',
    `<testsuites tests="${cases.length}" failures="${failures}" skipped="${skipped}">`,
    ` <testsuite name="sir-browser" tests="${cases.length}" failures="${failures}" skipped="${skipped}">`,
    cases.join("\n"),
    " </testsuite>",
    "</testsuites>",
    "",
  ].join("\n");
  mkdirSync(dirname(output), { recursive: true });
  writeFileSync(output, report, "utf8");
};

try {
  const results = await Promise.all(shardReports.map((_, offset) => runShard(offset)));
  mergeShardReports(shardReports);
  for (const result of results) {
    if (result.error) console.error(`browser shard ${result.index} could not start:`, result.error);
    if (result.code !== 0) process.exitCode = result.code;
  }
} finally {
  rmSync(shardRoot, { recursive: true, force: true });
}
