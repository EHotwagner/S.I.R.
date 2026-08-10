import { expect, test } from "@playwright/test";

test("authorized player journey advances and reconnects without credentials in runtime URLs", async ({ page }) => {
  const credentialUrls = [];
  page.on("request", (request) => {
    if (/access_token|sessionId|actorId/i.test(request.url())) credentialUrls.push(request.url());
  });

  await page.goto("/");

  const live = page.locator("#sir-live-session");
  await expect(live).toHaveAttribute("data-status", "connected", { timeout: 90_000 });
  await expect(live).not.toHaveAttribute("data-session-id", "");
  await expect.poll(async () => Number(await live.getAttribute("data-resync-count"))).toBeGreaterThan(0);

  const initialTick = Number(await live.getAttribute("data-tick"));
  const initialSequence = Number(await live.getAttribute("data-server-sequence"));
  await page.getByRole("button", { name: "Advance live session" }).click();

  await expect.poll(async () => Number(await live.getAttribute("data-tick"))).toBeGreaterThan(initialTick);
  await expect.poll(async () => Number(await live.getAttribute("data-server-sequence"))).toBeGreaterThan(initialSequence);

  const advancedTick = Number(await live.getAttribute("data-tick"));
  const resyncBeforeReconnect = Number(await live.getAttribute("data-resync-count"));
  await page.getByRole("button", { name: "Reconnect live session" }).click();

  await expect.poll(async () => Number(await live.getAttribute("data-resync-count")), { timeout: 30_000 }).toBeGreaterThan(resyncBeforeReconnect);
  await expect(live).toHaveAttribute("data-status", "connected");
  await expect(live).toHaveAttribute("data-tick", String(advancedTick));
  expect(credentialUrls).toEqual([]);
  await expect(page.locator("body")).not.toContainText("accessToken");
});
