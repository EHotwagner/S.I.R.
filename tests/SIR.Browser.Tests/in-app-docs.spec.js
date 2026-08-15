import { test, expect } from "@playwright/test";

test("documentation is a reversible, searchable production modality", async ({ page }) => {
  const responses = [];
  page.on("response", (response) => responses.push(response));
  await page.goto("/");

  const scene = page.locator("#retained-tactical-workscreen svg");
  await expect(scene).toBeVisible();
  await page.locator("#persistent-layer-units [data-unit-id]").first().click();
  const inspectorDocs = page.locator('[data-context-origin="inspector"]');
  await expect(inspectorDocs).toBeVisible();
  const sceneIdentity = await scene.evaluate((element) => {
    window.__sirRetainedScene = element;
    return [element.dataset.sceneOwner, element.dataset.sceneRevision, element.dataset.sceneTick, element.dataset.semanticSelectionUnit];
  });

  await inspectorDocs.click();
  const docs = page.getByRole("region", { name: "S.I.R. documentation" });
  await expect(docs).toBeVisible();
  await expect(docs.getByRole("heading", { name: /Gameplay units/i }).first()).toBeVisible();
  await docs.getByRole("button", { name: "Return to tactical workspace" }).click();
  await page.locator('[data-context-origin="overlay"]').click();
  await expect(docs.getByRole("heading", { name: /Map editor/i }).first()).toBeVisible();
  await docs.getByRole("button", { name: "Return to tactical workspace" }).click();

  await page.keyboard.press("Control+Shift+5");
  await expect(page.locator("#retained-tactical-workscreen")).toBeHidden();
  await expect(docs.getByRole("heading", { name: "Documentation", level: 1 })).toBeVisible();
  await expect(docs.getByRole("navigation", { name: "Documentation hierarchy" })).toBeVisible();

  const manifestResponse = responses.find((response) => response.url().endsWith("/content/sir-client/v1/in-app-docs.json"));
  expect(manifestResponse?.status()).toBe(200);
  const search = docs.getByRole("searchbox", { name: "Search documentation" });
  for (const query of ["los", "cover", "armor"]) {
    await search.fill(query);
    await expect(docs.getByText(/pages · \d+ indexed tokens/)).toBeVisible();
    await expect(docs.getByRole("navigation", { name: "Documentation hierarchy" }).getByRole("button").first()).toBeVisible();
  }

  await search.fill("combat resolution");
  await docs.getByRole("navigation", { name: "Documentation hierarchy" }).getByRole("button").first().click();
  await expect(docs.getByRole("navigation", { name: "Breadcrumbs" })).toBeVisible();
  await expect(docs.getByRole("navigation", { name: "On this page" })).toBeVisible();
  const source = docs.getByRole("link", { name: "Open matching GitHub source" });
  await expect(source).toHaveAttribute("href", /^https:\/\/github\.com\/EHotwagner\/S\.I\.R\.\/blob\/main\//);
  await page.context().setOffline(true);
  await source.click();
  await expect(docs.getByText("GitHub source is unavailable while offline. Local documentation remains available.")).toBeVisible();
  await expect(docs.getByRole("heading", { name: /Combat resolution/i }).first()).toBeVisible();
  await page.context().setOffline(false);

  await search.fill("");
  const pageButtons = docs.getByRole("navigation", { name: "Documentation hierarchy" }).getByRole("button");
  await pageButtons.nth(1).click();
  await pageButtons.nth(2).click();
  await docs.getByRole("button", { name: "Documentation back" }).click();
  await expect(docs.getByRole("button", { name: "Documentation forward" })).toBeEnabled();
  await docs.getByRole("button", { name: "Documentation forward" }).click();

  await page.setViewportSize({ width: 320, height: 720 });
  await page.evaluate(() => { document.documentElement.style.zoom = "400%"; });
  await expect(docs).toBeVisible();
  await expect(docs.getByRole("heading", { name: "Documentation", level: 1 })).toBeVisible();
  await page.evaluate(() => { document.documentElement.style.zoom = "100%"; });

  await docs.getByRole("button", { name: "Return to tactical workspace" }).click();
  await expect(scene).toBeVisible();
  expect(await scene.evaluate((element) => window.__sirRetainedScene === element)).toBe(true);
  expect(await scene.evaluate((element) => [element.dataset.sceneOwner, element.dataset.sceneRevision, element.dataset.sceneTick, element.dataset.semanticSelectionUnit])).toEqual(sceneIdentity);

  for (const menuName of ["File", "Edit", "View"]) {
    await page.getByRole("button", { name: menuName, exact: true }).click();
    await expect(page.getByRole("menu", { name: `${menuName} commands` }).getByRole("menuitem", { name: /Open documentation/ })).toBeVisible();
    await page.keyboard.press("Escape");
  }
});
