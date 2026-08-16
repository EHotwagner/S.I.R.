import { allowExpectedDiagnostic, expect, test } from "./journey.js";

test("the production shell loads registered features through real controls", async ({ page }) => {
  await page.setViewportSize({ width: 1600, height: 900 });
  const featureResponses = [];
  page.on("response", (response) => {
    if (/\/(RulesExplorer|RulesWorkbenchView|DocsView)-[^/]+\.js$/.test(new URL(response.url()).pathname)) {
      featureResponses.push(response.url());
    }
  });

  await page.goto("/");
  const shell = page.getByRole("main", { name: "S.I.R. simulator and editor", exact: true });
  await expect(shell).toHaveAttribute("data-feature-registry-version", "1");
  await expect(shell).toHaveAttribute("data-feature-shell", "loaded");
  expect(featureResponses).toEqual([]);

  const docsResponse = page.waitForResponse((response) => /\/DocsView-[^/]+\.js$/.test(new URL(response.url()).pathname));
  await page.getByRole("button", { name: "Docs", exact: true }).click();
  expect((await docsResponse).status()).toBe(200);
  const docs = page.getByRole("region", { name: "S.I.R. documentation", exact: true });
  await expect(docs).toBeVisible();
  await expect(docs.getByRole("heading", { name: "Documentation", exact: true })).toBeVisible();
  await docs.getByRole("button", { name: "Return to tactical workspace", exact: true }).click();

  const workbenchResponse = page.waitForResponse((response) => /\/RulesWorkbenchView-[^/]+\.js$/.test(new URL(response.url()).pathname));
  await page.locator("details.tactical-legacy-controls > summary").click();
  await page.getByRole("button", { name: "Rules", exact: true }).click();
  expect((await workbenchResponse).status()).toBe(200);
  await expect(page.getByRole("region", { name: "Design scenario catalog", exact: true })).toBeVisible();

  const rulesResponse = page.waitForResponse((response) => /\/RulesExplorer-[^/]+\.js$/.test(new URL(response.url()).pathname));
  await page.getByRole("button", { name: "View", exact: true }).click();
  await page.getByRole("menuitem", { name: /Rules data/ }).click();
  expect((await rulesResponse).status()).toBe(200);
  await expect(page.getByRole("region", { name: "Rules data tables", exact: true })).toBeVisible();

  await page.getByRole("button", { name: "Editor", exact: true }).click();
  await page.getByRole("button", { name: "Environment", exact: true }).click();
  await expect(page.getByRole("region", { name: "Tactical environment authoring", exact: true })).toBeVisible();
  await expect(shell).toHaveAttribute("data-feature-loader-diagnostic", "");

  expect(featureResponses.filter((url) => url.includes("DocsView-"))).toHaveLength(1);
  expect(featureResponses.filter((url) => url.includes("RulesWorkbenchView-"))).toHaveLength(1);
  expect(featureResponses.filter((url) => url.includes("RulesExplorer-"))).toHaveLength(1);
});

test("a deferred production control reports a stable offline failure", async ({ page }) => {
  allowExpectedDiagnostic(page, /ERR_INTERNET_DISCONNECTED/);
  await page.addInitScript(() => {
    Object.defineProperty(Navigator.prototype, "onLine", { configurable: true, get: () => false });
  });
  await page.route("**/DocsView-*.js", (route) => route.abort("internetdisconnected"));

  await page.goto("/");
  await page.getByRole("button", { name: "Docs", exact: true }).click();

  const failure = page.getByRole("region", { name: "Documentation load failure", exact: true });
  await expect(failure).toBeVisible();
  await expect(failure.getByRole("alert")).toHaveText(/^offline:/);
});
