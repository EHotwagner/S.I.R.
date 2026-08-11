import { expect, test as base } from "@playwright/test";

// Fail real-browser journeys on diagnostics users would experience. Expected rejection
// responses are declared by the individual scenario rather than silently ignored here.
export const test = base.extend({
  page: async ({ page }, use, testInfo) => {
    const diagnostics = [];
    page.on("console", (message) => {
      if (message.type() === "error") diagnostics.push(`console: ${message.text()}`);
    });
    page.on("pageerror", (error) => diagnostics.push(`page: ${error.message}`));
    page.on("response", (response) => {
      if (response.status() >= 400) {
        diagnostics.push(`network ${response.status()}: ${response.url()}`);
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

export { expect };
