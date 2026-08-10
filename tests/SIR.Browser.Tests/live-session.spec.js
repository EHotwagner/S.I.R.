import { expect, test } from "@playwright/test";

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
