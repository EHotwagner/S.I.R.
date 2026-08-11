import { defineConfig } from "@playwright/test";
import { resolve } from "node:path";
import { assertBrowserAvailable, browserExecutablePath } from "./browser-setup.js";

const repoRoot = resolve(import.meta.dirname, "../..");
const executablePath = browserExecutablePath();
assertBrowserAvailable();

export default defineConfig({
  testDir: import.meta.dirname,
  // The published server hosts one heavy Fable application.  Serial contexts
  // keep its startup budget deterministic instead of amplifying compilation
  // and browser initialization across the two available CI CPUs.
  workers: 1,
  reporter: [[resolve(import.meta.dirname, "deterministic-junit-reporter.js"), {
    // Focused SDD obligations use separately deterministic receipts.  The
    // default remains the complete browser inventory used by CI.
    outputFile: resolve(repoRoot, process.env.SIR_JUNIT_OUTPUT || "artifacts/test-results/browser.junit.xml"),
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
      SIR_LIVE_MAX_BOOTSTRAPS_PER_MINUTE: "16",
      // The serial suite mounts the production application once per isolated
      // test context. Keep its test-host admission budget above that bounded
      // inventory without changing the production default of eight/minute.
    },
    cwd: resolve(repoRoot, "artifacts/publish"),
    url: "http://127.0.0.1:5100/",
    reuseExistingServer: false,
    timeout: 120_000,
  },
});
