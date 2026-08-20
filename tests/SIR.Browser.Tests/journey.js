import { expect, test as base } from "@playwright/test";

const expectedDiagnostics = new WeakMap();

export function allowExpectedDiagnostic(page, matcher) {
  const allowed = expectedDiagnostics.get(page) || [];
  allowed.push(matcher);
  expectedDiagnostics.set(page, allowed);
}

// Fail real-browser journeys on diagnostics users would experience. Expected rejection
// responses are declared by the individual scenario rather than silently ignored here.
export const test = base.extend({
  page: async ({ page }, use, testInfo) => {
    const diagnostics = [];
    expectedDiagnostics.set(page, []);
    page.on("console", (message) => {
      const diagnostic = `console: ${message.text()}`;
      if (message.type() === "error" && !(expectedDiagnostics.get(page) || []).some((matcher) => matcher.test(diagnostic))) diagnostics.push(diagnostic);
    });
    page.on("pageerror", (error) => diagnostics.push(`page: ${error.message}`));
    page.on("response", (response) => {
      if (response.status() >= 400) {
        const diagnostic = `network ${response.status()}: ${response.url()}`;
        if (!(expectedDiagnostics.get(page) || []).some((matcher) => matcher.test(diagnostic))) diagnostics.push(diagnostic);
      }
    });

    await use(page);

    await testInfo.attach("browser-diagnostics", {
      body: diagnostics.join("\n") || "none",
      contentType: "text/plain",
    });
    expect(diagnostics, "unexpected console, page, or network diagnostics").toEqual([]);
  },
});

export async function switchWorkspace(page, workspace) {
  const command = workspace === "Docs" ? "Open documentation" : `Switch to ${workspace}`;
  await page.getByRole("button", { name: "View", exact: true }).click();
  await page
    .getByRole("menu", { name: "View commands" })
    .getByRole("menuitem", { name: new RegExp(`^${command}\\b`) })
    .click();
}

export async function openSamples(page) {
  await page.getByRole("button", { name: "File", exact: true }).click();
  await page.getByRole("menu", { name: "File commands" }).getByRole("menuitem", { name: "Samples", exact: true }).click();
}

export { expect };
