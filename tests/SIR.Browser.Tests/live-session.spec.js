import { expect, test } from "./journey.js";

async function selectOversizedFile(page, selector, name, size) {
  await page.locator(selector).evaluate((input, { name, size }) => {
    const file = new File(["x"], name, { type: "application/octet-stream" });
    Object.defineProperty(file, "size", { value: size });
    file.arrayBuffer = async () => {
      window.__sirImportReadCalls = (window.__sirImportReadCalls || 0) + 1;
      return new ArrayBuffer(1);
    };
    file.text = async () => {
      window.__sirImportReadCalls = (window.__sirImportReadCalls || 0) + 1;
      return "SIR-MAP 2\nsize 4 4\n";
    };
    const transfer = new DataTransfer();
    transfer.items.add(file);
    Object.defineProperty(input, "files", { value: transfer.files, configurable: true });
    input.dispatchEvent(new Event("change", { bubbles: true }));
  }, { name, size });
}

async function selectUnreadableFile(page, selector, name) {
  await page.locator(selector).evaluate((input, name) => {
    const file = new File(["x"], name, { type: "application/octet-stream" });
    file.arrayBuffer = async () => { throw new Error("read refused"); };
    file.text = async () => { throw new Error("read refused"); };
    const transfer = new DataTransfer();
    transfer.items.add(file);
    Object.defineProperty(input, "files", { value: transfer.files, configurable: true });
    input.dispatchEvent(new Event("change", { bubbles: true }));
  }, name);
}

async function showDocumentImports(page) {
  await page.getByRole("button", { name: "Editor", exact: true }).click();
  const toggle = page.locator("#layout-show-document");
  if (await toggle.getAttribute("aria-pressed") === "false") {
    await page.locator("details.tactical-panel-menu").click();
    await toggle.click();
  }
  await expect(page.locator('[data-panel-id="document"]')).toBeVisible();
  await page.locator("#layout-panel-document-collapse").click();
  await expect(page.locator("#editor-map-import")).toBeVisible();
}

test("oversized browser imports are rejected from metadata before browser reads", async ({ page }) => {
  test.setTimeout(90_000);
  await page.goto("/");
  await page.evaluate(() => { window.__sirImportReadCalls = 0; });

  await page.getByRole("button", { name: "Review", exact: true }).click();
  await expect(page.locator('input[aria-label="Choose replay package"]')).toBeVisible();
  await selectOversizedFile(page, 'input[aria-label="Choose replay package"]', "large.sirr", 1_048_577);
  await expect(page.getByText("Replay package is 1048577 bytes; the allowed maximum is 1048576 bytes.", { exact: true })).toBeVisible();
  await expect.poll(() => page.evaluate(() => window.__sirImportReadCalls)).toBe(0);
});

test("browser import read failures leave a visible recovery message", async ({ page }) => {
  await page.goto("/");
  await page.getByRole("button", { name: "Review", exact: true }).click();
  await expect(page.locator('input[aria-label="Choose replay package"]')).toBeVisible();
  await selectUnreadableFile(page, 'input[aria-label="Choose replay package"]', "unreadable.sirr");
  await expect(page.getByText("Replay package could not be read: read refused", { exact: true })).toBeVisible();
  await page.evaluate(() => { window.__sirImportReadCalls = 0; });
  await selectOversizedFile(page, 'input[aria-label="Choose replay package"]', "recovered.sirr", 1_048_576);
  await expect.poll(() => page.evaluate(() => window.__sirImportReadCalls)).toBe(1);
});

test("replay picker reads an exactly bounded file after rejecting an oversized one", async ({ page }) => {
  await page.goto("/");
  await page.evaluate(() => { window.__sirImportReadCalls = 0; });
  await page.getByRole("button", { name: "Review", exact: true }).click();
  const picker = 'input[aria-label="Choose replay package"]';
  await selectOversizedFile(page, picker, "over.sirr", 1_048_577);
  await expect(page.getByText("Replay package is 1048577 bytes; the allowed maximum is 1048576 bytes.", { exact: true })).toBeVisible();
  await selectOversizedFile(page, picker, "at-limit.sirr", 1_048_576);
  await expect.poll(() => page.evaluate(() => window.__sirImportReadCalls)).toBe(1);
});

test("all modalities retain spatial context and expose the maintained runtime", async ({ page }) => {
  await page.goto("/");
  const battlefield = page.locator("#persistent-tactical-svg");
  const initial = {
    owner: await battlefield.getAttribute("data-scene-owner"),
    viewBox: await battlefield.getAttribute("viewBox"),
    revision: await battlefield.getAttribute("data-scene-revision"),
    panX: await battlefield.getAttribute("data-camera-pan-x"),
    panY: await battlefield.getAttribute("data-camera-pan-y"),
    zoom: await battlefield.getAttribute("data-camera-zoom"),
    selection: await battlefield.getAttribute("data-semantic-selection-unit"),
  };
  expect(initial.owner).toBe("EditorScene");

  for (const mode of ["Plan", "Simulate", "Review"]) {
    await page.getByRole("button", { name: mode, exact: true }).click();
    await expect(battlefield).toHaveAttribute("viewBox", initial.viewBox);
    await expect(battlefield).toHaveAttribute("data-camera-pan-x", initial.panX);
    await expect(battlefield).toHaveAttribute("data-camera-pan-y", initial.panY);
    await expect(battlefield).toHaveAttribute("data-camera-zoom", initial.zoom);
    await expect(battlefield).toHaveAttribute("data-scene-owner", mode === "Plan" ? "PlanningScene" : mode === "Simulate" ? "SimulatorScene" : "EditorScene");
    await expect(battlefield).toHaveAttribute("data-scene-tick", "0");
    await expect(battlefield).toHaveAttribute("data-scene-revision", initial.revision);
    await expect(battlefield).toHaveAttribute("data-semantic-selection-unit", initial.selection);
  }
});

test("tactical command controls expose registry-derived shortcut metadata", async ({ page }) => {
  await page.goto("/");

  const editor = page.getByRole("button", { name: "Editor", exact: true });
  await expect(editor).toHaveAttribute("aria-keyshortcuts", "Control+Shift+1");
  await expect(editor).toHaveAttribute("title", /Ctrl\+Shift\+1/);

  const play = page.getByRole("button", { name: "Play tactical timeline", exact: true });
  await expect(play).toHaveAttribute("aria-keyshortcuts", "Space");
  await expect(play).toHaveAttribute("title", /Space/);

  await page.getByRole("button", { name: "Show contextual actions", exact: true }).click();
  const command = page.locator('[data-tactical-command="workspace.plan"]');
  await expect(command).toHaveAttribute("aria-keyshortcuts", "Control+Shift+2");
  await expect(command.locator("kbd")).toHaveText("Ctrl+Shift+2");

  // Exercise the production window keyboard subscription for the same
  // registry that rendered the contextual command list. Focus deliberately
  // remains on the native menu button so this covers the modified-shortcut
  // exception in the global keyboard target filter.
  await page.keyboard.press("Escape");
  await expect(command).toHaveCount(0);
  const contextualActions = page.getByRole("button", { name: "Show contextual actions", exact: true });
  await contextualActions.focus();
  await expect(contextualActions).toBeFocused();

  await page.keyboard.press("Control+Shift+2");
  await expect(page.getByRole("button", { name: "Plan", exact: true })).toHaveAttribute("aria-pressed", "true");
});

test("every visible actionable control is registry-bound or explicitly unassigned", async ({ page }) => {
  await page.goto("/");
  for (const mode of ["Editor", "Plan", "Simulate", "Review"]) {
    await page.getByRole("button", { name: mode, exact: true }).click();
    const uncovered = await page.locator("button:visible, [role=button]:visible").evaluateAll((controls) =>
      controls.filter((control) =>
        !control.hasAttribute("aria-keyshortcuts") && control.getAttribute("data-binding-state") !== "unassigned"
      ).map((control) => ({
        tag: control.tagName,
        name: control.getAttribute("aria-label") || control.textContent?.trim(),
      }))
    );
    const undisclosed = await page.locator('[data-binding-state="unassigned"]:visible').evaluateAll((controls) =>
      controls.filter((control) => !/unassigned/i.test(control.getAttribute("aria-description") || ""))
        .map((control) => control.getAttribute("aria-label") || control.textContent?.trim())
    );
    expect(uncovered, mode).toEqual([]);
    expect(undisclosed, mode).toEqual([]);
  }
});

test("map and raster pickers reject oversized metadata before reads", async ({ page }) => {
  await page.goto("/");
  await showDocumentImports(page);
  await page.evaluate(() => { window.__sirImportReadCalls = 0; });
  await selectOversizedFile(page, "#editor-map-import", "over.sir-map", 2_000_001);
  await expect(page.getByRole("alert")).toContainText("Map import is 2000001 bytes");
  await selectOversizedFile(page, "#editor-background-file", "over.png", 10_000_001);
  await expect(page.getByRole("alert")).toContainText("Raster background is 10000001 bytes");
  await expect.poll(() => page.evaluate(() => window.__sirImportReadCalls)).toBe(0);
  await selectOversizedFile(page, "#editor-map-import", "at-limit.sir-map", 2_000_000);
  await expect.poll(() => page.evaluate(() => window.__sirImportReadCalls)).toBe(1);
  await expect(page.getByRole("alert")).toContainText("Imported map at-limit.sir-map.");
  await selectOversizedFile(page, "#editor-background-file", "at-limit.png", 10_000_000);
  await expect.poll(() => page.evaluate(() => window.__sirImportReadCalls)).toBe(2);
  await selectOversizedFile(page, "#editor-map-import", "invalid.sir-map", -1);
  await expect(page.getByRole("alert")).toContainText("Map import has invalid size metadata");
  await expect.poll(() => page.evaluate(() => window.__sirImportReadCalls)).toBe(2);
  for (const size of [NaN, Infinity, 1.5]) {
    await selectOversizedFile(page, "#editor-map-import", "invalid.sir-map", size);
    await expect(page.getByRole("alert")).toContainText("Map import has invalid size metadata");
    await expect.poll(() => page.evaluate(() => window.__sirImportReadCalls)).toBe(2);
  }
  await selectUnreadableFile(page, "#editor-map-import", "unreadable.sir-map");
  await expect(page.getByRole("alert")).toContainText("Map import could not be read: read refused");
  await selectOversizedFile(page, "#editor-map-import", "recovered.sir-map", 2_000_000);
  await expect(page.getByRole("alert")).toContainText("Imported map recovered.sir-map.");
  await selectUnreadableFile(page, "#editor-background-file", "unreadable.png");
  await expect(page.getByRole("alert")).toContainText("Raster background could not be read: read refused");
  await selectOversizedFile(page, "#editor-background-file", "recovered.png", 10_000_000);
  await expect(page.getByRole("alert")).toContainText("Attached background recovered.png.");
});

test("bootstrap fails closed for absent and cross-actor credentials", async ({ request }) => {
  const body = { version: 1, actorName: "alpha" };
  const absent = await request.post("/api/bootstrap", { data: body });
  expect(absent.status()).toBe(401);
  const crossActor = await request.post("/api/bootstrap", {
    data: body,
    headers: { "X-SIR-Development-Actor": "beta" },
  });
  expect(crossActor.status()).toBe(400);
});

test("authorized player journey advances and reconnects without credentials in runtime URLs", async ({ page }) => {
  const credentialUrls = [];
  page.on("request", (request) => {
    if (/access_token|sessionId|actorId/i.test(request.url())) credentialUrls.push(request.url());
  });

  await page.goto("/");

  const live = page.locator("#sir-live-session");
  const battlefield = page.locator("#persistent-tactical-svg");
  await expect(live).toHaveAttribute("data-status", "connected", { timeout: 90_000 });
  await expect(live).not.toHaveAttribute("data-session-id", "");
  await expect.poll(async () => Number(await live.getAttribute("data-resync-count"))).toBeGreaterThan(0);

  const initialTick = Number(await live.getAttribute("data-tick"));
  const initialSequence = Number(await live.getAttribute("data-server-sequence"));
  await expect(battlefield).toHaveAttribute("data-live-tick", String(initialTick));
  await expect(battlefield).toHaveAttribute("data-live-server-sequence", String(initialSequence));
  await page.getByRole("button", { name: "Send the next player-visible live advance command" }).click();

  await expect.poll(async () => Number(await live.getAttribute("data-tick"))).toBeGreaterThan(initialTick);
  await expect.poll(async () => Number(await live.getAttribute("data-server-sequence"))).toBeGreaterThan(initialSequence);
  await expect.poll(async () => Number(await battlefield.getAttribute("data-live-tick"))).toBeGreaterThan(initialTick);

  const advancedTick = Number(await live.getAttribute("data-tick"));
  const resyncBeforeReconnect = Number(await live.getAttribute("data-resync-count"));
  await page.getByRole("button", { name: "Disconnect the player-visible live session" }).click();
  await expect(live).toHaveAttribute("data-status", "disconnected");
  await page.getByRole("button", { name: "Reconnect and request the authoritative live snapshot" }).click();

  await expect.poll(async () => Number(await live.getAttribute("data-resync-count")), { timeout: 30_000 }).toBeGreaterThan(resyncBeforeReconnect);
  await expect(live).toHaveAttribute("data-status", "connected");
  await expect(live).toHaveAttribute("data-tick", String(advancedTick));
  await expect(battlefield).toHaveAttribute("data-live-tick", String(advancedTick));
  await expect(page).not.toHaveTitle(/__sirLive/);
  await expect.poll(() => page.evaluate(() => typeof window.__sirLiveAdvance)).toBe("undefined");
  await expect.poll(() => page.evaluate(() => typeof window.__sirLiveReconnect)).toBe("undefined");
  expect(credentialUrls).toEqual([]);
  await expect(page.locator("body")).not.toContainText("accessToken");
});
