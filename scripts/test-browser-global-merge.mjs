#!/usr/bin/env node
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { mergeBrowserShardCases, parseBrowserShardJUnit } from "./browser-junit.mjs";

const [outputArgument, ...fragmentArguments] = process.argv.slice(2);
if (!outputArgument || fragmentArguments.length < 2) {
  throw new Error("usage: test-browser-global-merge.mjs OUTPUT FRAGMENT FRAGMENT...");
}

const groups = fragmentArguments.map((argument, offset) =>
  parseBrowserShardJUnit(readFileSync(resolve(argument), "utf8"), `browser global shard ${offset + 1}`));
const report = mergeBrowserShardCases(groups);
const output = resolve(outputArgument);
mkdirSync(dirname(output), { recursive: true });
writeFileSync(output, report, "utf8");

const testCount = groups.reduce((total, group) => total + group.length, 0);
console.log(`browser global merge: ${testCount} complete unique tests across ${groups.length} isolated runners`);
