import { expect, test } from "@playwright/test";

test("real S.I.R. projection advances authoritatively and fully resyncs after reconnect", async ({ page }) => {
  await page.goto("/");

  const live = page.locator("#sir-live-session");
  await expect(live).toHaveAttribute("data-status", "connected", { timeout: 90_000 });
  await expect(live).not.toHaveAttribute("data-session-id", "");
  await expect.poll(async () => Number(await live.getAttribute("data-resync-count"))).toBeGreaterThan(0);

  const initialTick = Number(await live.getAttribute("data-tick"));
  const initialSequence = Number(await live.getAttribute("data-server-sequence"));
  await page.evaluate(() => window.__sirLiveAdvance());

  await expect.poll(async () => Number(await live.getAttribute("data-tick"))).toBeGreaterThan(initialTick);
  await expect.poll(async () => Number(await live.getAttribute("data-server-sequence"))).toBeGreaterThan(initialSequence);

  const advancedTick = Number(await live.getAttribute("data-tick"));
  const resyncBeforeReconnect = Number(await live.getAttribute("data-resync-count"));
  await page.evaluate(() => window.__sirLiveReconnect());

  await expect.poll(async () => Number(await live.getAttribute("data-resync-count")), { timeout: 30_000 }).toBeGreaterThan(resyncBeforeReconnect);
  await expect(live).toHaveAttribute("data-status", "connected");
  await expect(live).toHaveAttribute("data-tick", String(advancedTick));
});
