import { expect, test } from "./journey.js";

test("the production Samples panel exposes every tactical family and a playable teaching journey", async ({ page }) => {
  await page.goto("/");
  await page.getByRole("button", { name: "Simulate", exact: true }).click();
  await page.getByRole("button", { name: "Show contextual actions", exact: true }).click();
  await page.getByRole("button", { name: "Open simulator samples", exact: true }).click();

  const mapCards = page.locator(".simulator-sample-entry");
  await expect(mapCards).toHaveCount(7);
  for (const title of [
    "Quick contact",
    "Open-field movement and fire",
    "Cover-dense assault and flank",
    "Door breach and interior clear",
    "Support-by-fire and suppression",
    "Armored target and anti-armor response",
    "Withdrawal and reinforcement",
  ]) {
    const card = mapCards.filter({ hasText: title });
    await expect(card).toHaveCount(1);
    await expect(card.getByText(/^Lesson:/)).toBeVisible();
    await expect(card.getByText(/^Design notes:/)).toBeVisible();
  }

  await page.getByRole("button", {
    name: "Load simulation sample: A four-unit first contact that reaches a visible outcome immediately.",
    exact: true,
  }).click();
  await expect(page.getByText(/^Authoritative runtime tick 0/)).toBeVisible();
  await page.getByRole("button", { name: "Advance the map simulation one tick", exact: true }).click();
  await expect(page.getByText(/^Authoritative runtime tick 1/)).toBeVisible();
});
