import { test, expect } from "@playwright/test";

test("documentation is a reversible, searchable production modality", async ({ page }) => {
  const responses = [];
  page.on("response", (response) => responses.push(response));
  const shellResponse = await page.goto("/");
  expect(shellResponse.headers()["content-security-policy"]).toContain("default-src 'self'");

  const scene = page.locator("#retained-tactical-workscreen svg");
  await expect(scene).toBeVisible();
  await page.locator("#persistent-layer-units [data-unit-id]").first().click();
  const inspectorDocs = page.locator('[data-context-origin="inspector"]');
  await expect(inspectorDocs).toBeVisible();
  const retainedFingerprint = async () => scene.evaluate((element) => {
    window.__sirRetainedScene = element;
    const panelInventory = [...document.querySelectorAll("[data-panel-id]")].map((panel) => [panel.dataset.panelId, panel.dataset.panelSide, panel.dataset.panelOrder, panel.hidden, panel.getAttribute("aria-hidden"), panel.className]);
    return {
      scene: [element.dataset.sceneOwner, element.dataset.sceneRevision, element.dataset.sceneTick, element.dataset.semanticSelectionUnit, element.dataset.cameraPanX, element.dataset.cameraPanY, element.dataset.cameraZoom],
      modality: document.querySelector(".tactical-workscreen-region")?.dataset.activeModality,
      panelInventory,
      timeline: [...document.querySelectorAll("[data-timeline-tick],[aria-label*='timeline' i]")].map((node) => [node.getAttribute("data-timeline-tick"), node.getAttribute("aria-valuenow"), node.getAttribute("aria-pressed")]),
    };
  });
  const sceneIdentity = await retainedFingerprint();

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
  await page.keyboard.press("Control+K");
  await expect(docs.getByRole("searchbox", { name: "Search documentation" })).toBeFocused();

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
  await expect(source).toHaveAttribute("href", /^https:\/\/github\.com\/EHotwagner\/S\.I\.R\.\/blob\/[0-9a-f]{40}\//);
  await page.route("https://github.com/**", (route) => route.abort("internetdisconnected"));
  await source.click();
  await expect(docs.getByText("GitHub source is unavailable from this host. Local documentation remains available.")).toBeVisible();
  await expect(docs.getByRole("heading", { name: /Combat resolution/i }).first()).toBeVisible();
  await page.unroute("https://github.com/**");

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
  if (process.env.SIR_DOCS_BROWSER_MUTATE_SUBJECT === "overflow") {
    await docs.evaluate((region) => {
      const mutant = document.createElement("div");
      mutant.style.cssText = "display:block;min-width:1000px;width:1000px;max-width:none;height:1px;flex:none";
      mutant.setAttribute("data-overflow-mutant", "true");
      region.appendChild(mutant);
    });
  }
  const zoomGeometry = await docs.evaluate((region) => ({
    clientWidth: region.clientWidth,
    scrollWidth: region.scrollWidth,
    controlsOutsideRegion: [...region.querySelectorAll("button,input,a")].filter((element) => {
      const bounds = element.getBoundingClientRect();
      const regionBounds = region.getBoundingClientRect();
      return bounds.left < regionBounds.left - 1 || bounds.right > regionBounds.right + 1;
    }).map((element) => element.outerHTML.slice(0, 80)),
    h1Count: region.querySelectorAll("h1").length,
    firstHeading: region.querySelector("h1,h2,h3,h4")?.tagName,
  }));
  expect(zoomGeometry.controlsOutsideRegion).toEqual([]);
  // Chromium rounds 400%-zoom layout edges by up to two CSS pixels across
  // managed and system executables. Controls must still remain inside the
  // region, while any material overflow is rejected by the separate bound.
  expect(zoomGeometry.scrollWidth - zoomGeometry.clientWidth).toBeLessThanOrEqual(2);
  expect(zoomGeometry).toMatchObject({ h1Count: 1, firstHeading: "H1" });
  await page.evaluate(() => { document.documentElement.style.zoom = "100%"; });

  await docs.getByRole("button", { name: "Return to tactical workspace" }).click();
  await expect(scene).toBeVisible();
  expect(await scene.evaluate((element) => window.__sirRetainedScene === element)).toBe(true);
  expect(await retainedFingerprint()).toEqual(sceneIdentity);

  for (const mode of ["Editor", "Plan", "Simulate", "Review"]) {
    await page.getByRole("button", { name: mode, exact: true }).click();
    await expect(scene).toBeVisible();
    const beforeDocs = await retainedFingerprint();
    await page.keyboard.press("Control+Shift+5");
    await expect(docs).toBeVisible();
    await docs.getByRole("button", { name: "Return to tactical workspace" }).click();
    await expect(scene).toBeVisible();
    expect(await scene.evaluate((element) => window.__sirRetainedScene === element)).toBe(true);
    expect(await retainedFingerprint()).toEqual(beforeDocs);
  }

  for (const menuName of ["File", "Edit", "View"]) {
    await page.getByRole("button", { name: menuName, exact: true }).click();
    await expect(page.getByRole("menu", { name: `${menuName} commands` }).getByRole("menuitem", { name: /Open documentation/ })).toBeVisible();
    await page.keyboard.press("Escape");
  }
});
