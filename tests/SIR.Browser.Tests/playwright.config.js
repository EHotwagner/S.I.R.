import { defineConfig } from "@playwright/test";
import { resolve } from "node:path";

const repoRoot = resolve(import.meta.dirname, "../..");
const executablePath = process.env.PLAYWRIGHT_EXECUTABLE_PATH;

export default defineConfig({
  testDir: ".",
  reporter: [["junit", { outputFile: resolve(repoRoot, "artifacts/test-results/browser.junit.xml") }]],
  use: {
    baseURL: "http://127.0.0.1:5100",
    launchOptions: executablePath ? { executablePath } : {},
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
