import { defineConfig } from "@playwright/test";
import { resolve } from "node:path";
import { assertBrowserAvailable, browserExecutablePath } from "./browser-setup.js";

const repoRoot = resolve(import.meta.dirname, "../..");
const executablePath = browserExecutablePath();
assertBrowserAvailable();

export default defineConfig({
  testDir: import.meta.dirname,
  reporter: [[resolve(import.meta.dirname, "deterministic-junit-reporter.js"), {
    outputFile: resolve(repoRoot, "artifacts/test-results/browser.junit.xml"),
  }]],
  globalSetup: resolve(import.meta.dirname, "browser-setup.js"),
  use: {
    baseURL: "http://127.0.0.1:5100",
    launchOptions: { executablePath },
  },
  webServer: {
    command: "dotnet SIR.Server.dll --urls http://127.0.0.1:5100",
    env: {
      ...process.env,
      ASPNETCORE_ENVIRONMENT: "Development",
      SIR_ALLOW_ANONYMOUS_LIVE_SESSIONS: "true",
    },
    cwd: resolve(repoRoot, "artifacts/publish"),
    url: "http://127.0.0.1:5100/",
    reuseExistingServer: false,
    timeout: 120_000,
  },
});
