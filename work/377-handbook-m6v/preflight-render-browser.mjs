#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";
import { chromium } from "@playwright/test";

const root = process.cwd();
const packageLockBytes = fs.readFileSync(path.join(root, "package-lock.json"));
const packageLock = JSON.parse(packageLockBytes);
const installedPackage = JSON.parse(fs.readFileSync(path.join(root, "node_modules/@playwright/test/package.json"), "utf8"));
const lockedVersion = packageLock.packages?.["node_modules/@playwright/test"]?.version;
if (!lockedVersion || installedPackage.version !== lockedVersion) {
  throw new Error(`browser-preflight: installed @playwright/test ${installedPackage.version} does not match lockfile ${lockedVersion ?? "missing"}`);
}

const executablePath = process.env.PLAYWRIGHT_EXECUTABLE_PATH || chromium.executablePath();
const executableSource = process.env.SIR_M6V_BROWSER_SOURCE ?? (process.env.PLAYWRIGHT_EXECUTABLE_PATH ? "explicit-PLAYWRIGHT_EXECUTABLE_PATH" : "playwright-managed");
if (!["explicit-PLAYWRIGHT_EXECUTABLE_PATH", "playwright-managed"].includes(executableSource)) throw new Error(`browser-preflight: invalid executable source ${executableSource}`);
if (!fs.existsSync(executablePath)) throw new Error(`browser-preflight: Chromium executable is missing at ${executablePath}`);

const browser = await chromium.launch({ executablePath });
const browserVersion = await browser.version();
await browser.close();

const receipt = {
  schema: "sir.handbook.browser-preflight/v1",
  result: "pass",
  measurementSubject: false,
  bootstrap: "npm-ci lockfile plus install-if-missing Chromium; explicit system executable is never replaced",
  packageLockSha256: crypto.createHash("sha256").update(packageLockBytes).digest("hex"),
  playwrightVersion: installedPackage.version,
  browserVersion,
  executableSource
};
const receiptPath = path.resolve(root, process.env.SIR_M6V_BROWSER_PREFLIGHT_RECEIPT ?? "readiness/377-handbook-m6v/browser-preflight.json");
fs.mkdirSync(path.dirname(receiptPath), { recursive: true });
fs.writeFileSync(receiptPath, JSON.stringify(receipt, null, 2) + "\n");
console.log(`browser-preflight: pass (@playwright/test@${installedPackage.version}, ${browserVersion}; outside timing subject)`);
