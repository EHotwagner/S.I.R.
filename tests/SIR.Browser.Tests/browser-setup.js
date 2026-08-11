import { chromium } from "@playwright/test";
import { execFileSync } from "node:child_process";
import { existsSync } from "node:fs";

export function browserExecutablePath() {
  return process.env.PLAYWRIGHT_EXECUTABLE_PATH || chromium.executablePath();
}

export function assertBrowserAvailable() {
  const executablePath = browserExecutablePath();

  if (!existsSync(executablePath)) {
    throw new Error(
      `Browser executable is missing at ${executablePath}. Run \`npm run setup:browser\` to install the pinned Playwright Chromium, or set PLAYWRIGHT_EXECUTABLE_PATH to a supported system Chromium.`,
    );
  }

  return executablePath;
}

export default function browserSetup() {
  const executablePath = assertBrowserAvailable();
  const policy = process.env.PLAYWRIGHT_EXECUTABLE_PATH ? "system override" : "Playwright managed";

  const version = execFileSync(executablePath, ["--version"], { encoding: "utf8" }).trim();
  console.log(`[browser] policy=${policy}; executable=${executablePath}; version=${version}`);
}
