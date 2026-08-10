import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";

// SDD observes this report by digest.  Playwright's stock JUnit reporter includes
// clock and duration fields, so an otherwise identical browser run changes the
// observed-run receipt.  Keep this deliberately small projection deterministic.
const xml = (value) => String(value ?? "")
  .replace(/[\u0000-\u0008\u000B\u000C\u000E-\u001F]/g, "")
  .replace(/&/g, "&amp;")
  .replace(/</g, "&lt;")
  .replace(/>/g, "&gt;")
  .replace(/"/g, "&quot;")
  .replace(/'/g, "&apos;");

const stableMessage = (error) => {
  const message = error?.message || error?.value || error?.snippet || "Playwright reported a failed test without an error message.";
  return message
    .replace(/\x1B\[[0-?]*[ -/]*[@-~]/g, "")
    .replaceAll(resolve("."), "<repo>")
    .replace(/\\/g, "/")
    .trim();
};

export default class DeterministicJUnitReporter {
  constructor(options) {
    this.outputFile = options.outputFile;
    this.results = new Map();
  }

  printsToStdio() {
    return false;
  }

  onTestEnd(test, result) {
    this.results.set(test.id, { test, result });
  }

  onEnd() {
    const cases = [...this.results.values()]
      .map(({ test, result }) => {
        const titles = test.titlePath().filter(Boolean);
        const name = titles.at(-1) || test.title;
        const classname = titles.slice(0, -1).join(" › ") || "browser";
        const failed = !test.ok() || result.status === "failed" || result.status === "timedOut" || result.status === "interrupted";
        const skipped = result.status === "skipped";
        const errors = result.errors.length ? result.errors : [result.error];
        return { classname, name, failed, skipped, errors };
      })
      .sort((left, right) => `${left.classname}\u0000${left.name}`.localeCompare(`${right.classname}\u0000${right.name}`));

    const failures = cases.filter((test) => test.failed).length;
    const skipped = cases.filter((test) => test.skipped).length;
    const body = cases.map((test) => {
      const open = `  <testcase classname="${xml(test.classname)}" name="${xml(test.name)}">`;
      if (test.failed) {
        const message = stableMessage(test.errors.find(Boolean));
        return `${open}\n    <failure message="${xml(message)}">${xml(message)}</failure>\n  </testcase>`;
      }
      if (test.skipped) return `${open}\n    <skipped/>\n  </testcase>`;
      return `${open}</testcase>`;
    }).join("\n");

    const report = [
      '<?xml version="1.0" encoding="UTF-8"?>',
      `<testsuites tests="${cases.length}" failures="${failures}" skipped="${skipped}">`,
      ` <testsuite name="sir-browser" tests="${cases.length}" failures="${failures}" skipped="${skipped}">`,
      body,
      " </testsuite>",
      "</testsuites>",
      "",
    ].join("\n");
    mkdirSync(dirname(this.outputFile), { recursive: true });
    writeFileSync(this.outputFile, report, "utf8");
  }
}
