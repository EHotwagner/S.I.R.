import { defineConfig } from "@playwright/test";
import { resolve } from "node:path";
import { assertBrowserAvailable, browserExecutablePath } from "./browser-setup.js";

const repoRoot = resolve(import.meta.dirname, "../..");
const executablePath = browserExecutablePath();
assertBrowserAvailable();
const browserPort = Number(process.env.SIR_BROWSER_PORT ?? "5100");
if (!Number.isSafeInteger(browserPort) || browserPort < 1024 || browserPort > 65_535) {
  throw new Error("SIR_BROWSER_PORT must be an integer between 1024 and 65535.");
}
const browserBaseUrl = `http://127.0.0.1:${browserPort}`;

export default defineConfig({
  testDir: import.meta.dirname,
  testMatch: "**/*.spec.js",
  // Distribute individual tests rather than whole files. The inventory has a
  // few large journey files, so file-level sharding leaves capacity idle.
  fullyParallel: true,
  // One worker owns one isolated production server. CI parallelism happens by
  // sharding across separate ports, so deterministic live-session identities
  // can never collide between concurrent browser contexts.
  workers: 1,
  reporter: [[resolve(import.meta.dirname, "declared-budget-reporter.js"), {}], [resolve(import.meta.dirname, "deterministic-junit-reporter.js"), {
    // Focused SDD obligations use separately deterministic receipts.  The
    // default remains the complete browser inventory used by CI.
    outputFile: resolve(repoRoot, process.env.SIR_JUNIT_OUTPUT || "artifacts/test-results/browser.junit.xml"),
  }]],
  globalSetup: resolve(import.meta.dirname, "browser-setup.js"),
  use: {
    baseURL: browserBaseUrl,
    launchOptions: { executablePath },
  },
  webServer: {
    command: `dotnet SIR.Server.dll --urls ${browserBaseUrl}`,
    env: {
      ...process.env,
      ASPNETCORE_ENVIRONMENT: "Development",
      SIR_ALLOW_ANONYMOUS_LIVE_SESSIONS: "true",
      SIR_LIVE_MAX_BOOTSTRAPS_PER_MINUTE: "64",
      // The serial suite mounts the production application once per isolated
      // test context. Keep its test-host admission budget above the bounded
      // browser inventory (including direct bootstrap rejection checks) without
      // changing the production default of eight/minute.
    },
    cwd: resolve(repoRoot, "artifacts/publish"),
    url: `${browserBaseUrl}/`,
    reuseExistingServer: false,
    timeout: 120_000,
  },
});
