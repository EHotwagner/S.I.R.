import { expect, test } from "./journey.js";

test("the production Samples panel exposes every tactical family and a playable teaching journey", async ({ page }) => {
  await page.goto("/");
  await page.getByRole("button", { name: "Simulate", exact: true }).click();
  const fileMenu = page.locator("details.desktop-menu").filter({ has: page.getByRole("button", { name: "File", exact: true }) });
  await page.getByRole("button", { name: "File", exact: true }).click();
  await expect(fileMenu).toHaveAttribute("open", "");
  await page.getByRole("menuitem", { name: "Samples", exact: true }).click();
  await expect(fileMenu).not.toHaveAttribute("open", "");

  const mapCards = page.locator(".sample-card").filter({ has: page.locator('.sample-kind', { hasText: "Map · Simulation" }) });
  await expect(mapCards).toHaveCount(7);
  const titles = [
    "Quick contact",
    "Open-field movement and fire",
    "Cover-dense assault and flank",
    "Door breach and interior clear",
    "Support-by-fire and suppression",
    "Armored target and anti-armor response",
    "Withdrawal and reinforcement",
  ];
  for (const title of titles) {
    const card = mapCards.filter({ hasText: title });
    await expect(card).toHaveCount(1);
    await card.locator("summary").click();
    await expect(card.getByText(/^Lesson:/)).toBeVisible();
    await expect(card.getByText(/^Design notes:/)).toBeVisible();
    if (title === "Armored target and anti-armor response") {
      await expect(card.getByText("Troll assault", { exact: true })).toBeVisible();
      await expect(card.getByRole("button", { name: "Run Troll assault in Simulator", exact: true })).toBeVisible();
    }
    await card.getByRole("button", { name: `Run ${title} in Simulator`, exact: true }).click();
    await expect(page.getByText(/^Authoritative runtime tick 0/)).toBeVisible();
    await page.getByRole("button", { name: "Advance the map simulation one tick", exact: true }).click();
    await expect(page.getByText(/^Authoritative runtime tick 1/)).toBeVisible();
    await page.getByRole("button", { name: "File", exact: true }).click();
    await page.getByRole("menuitem", { name: "Samples", exact: true }).click();
  }

  await page.getByRole("button", {
    name: "Run Quick contact in Simulator",
    exact: true,
  }).click();
  await expect(page.getByText(/^Authoritative runtime tick 0/)).toBeVisible();
  for (let tick = 1; tick <= 8; tick += 1) {
    await page.getByRole("button", { name: "Advance the map simulation one tick", exact: true }).click();
    await expect(page.getByText(new RegExp(`^Authoritative runtime tick ${tick}`))).toBeVisible();
  }
});
