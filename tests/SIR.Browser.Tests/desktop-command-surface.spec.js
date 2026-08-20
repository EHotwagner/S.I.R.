import { expect, test } from "./journey.js";

test("desktop menu exposes registry-backed commands and Escape dismisses it", async ({ page }) => {
  await page.goto("/");
  const menu = page.getByRole("button", { name: "View", exact: true });
  await menu.click();
  const popover = page.getByRole("menu", { name: "View commands" });
  await expect(popover).toBeVisible();
  await page.keyboard.press("ArrowDown");
  const menuItems = popover.getByRole("menuitem");
  await expect(menuItems.first()).toBeFocused();
  await expect(popover.locator("[role='menuitem'][tabindex='0']")).toHaveCount(1);
  await expect(menuItems.first()).toHaveAttribute("tabindex", "0");
  await page.keyboard.press("ArrowDown");
  await expect(menuItems.nth(1)).toBeFocused();
  await expect(menuItems.nth(1)).toHaveAttribute("tabindex", "0");
  await expect(menuItems.first()).toHaveAttribute("tabindex", "-1");
  const plan = popover.getByRole("menuitem", { name: /Switch to Plan/ });
  await expect(plan).toHaveAttribute("aria-keyshortcuts", "Control+Shift+2");
  await page.keyboard.press("Escape");
  await expect(popover).toBeHidden();
  await expect(menu).toBeFocused();
});

test("View exposes persistent checkbox controls for panels and timeline", async ({ page }) => {
  await page.goto("/");
  await page.getByRole("button", { name: "View", exact: true }).click();
  const view = page.getByRole("menu", { name: "View commands" });
  const roster = view.getByRole("menuitemcheckbox", { name: "Roster / outliner", exact: true });
  const timeline = view.getByRole("menuitemcheckbox", { name: "Timeline", exact: true });
  await expect(view.locator(".view-panel-menu-item")).toHaveCount(11);
  await expect(roster).toHaveAttribute("aria-checked", "true");
  await expect(timeline).toHaveAttribute("aria-checked", "false");
  await roster.click();
  await expect(page.locator("#layout-panel-roster")).toHaveCount(0);
  await page.getByRole("button", { name: "View", exact: true }).click();
  await expect(view.getByRole("menuitemcheckbox", { name: "Roster / outliner", exact: true })).toHaveAttribute("aria-checked", "false");
  await view.getByRole("menuitemcheckbox", { name: "Roster / outliner", exact: true }).click();
  await expect(page.locator("#layout-panel-roster")).toBeVisible();
});

test("View loads deferred supporting panels without a second toolbar row", async ({ page }) => {
  await page.goto("/");
  await expect(page.getByRole("navigation", { name: "Supporting application sections" })).toBeHidden();
  await page.getByRole("button", { name: "View", exact: true }).click();
  await page.getByRole("menu", { name: "View commands" }).getByRole("menuitemcheckbox", { name: "Rules", exact: true }).click();
  await expect(page.locator("#layout-panel-rules")).toBeVisible();
  await expect(page.getByRole("region", { name: "Design scenario catalog" }).getByRole("button", { name: /^Simulate design scenario/ })).toHaveCount(6);
});

test("compact desktop chrome keeps a reachable overflow menu", async ({ page }) => {
  await page.setViewportSize({ width: 360, height: 720 });
  await page.goto("/");
  const menuBar = page.getByRole("menubar");
  await expect(menuBar).toBeVisible();
  await page.getByRole("button", { name: "Help", exact: true }).click();
  await expect(page.getByRole("menu", { name: "Help commands" })).toBeVisible();
});

test("delivery support remains reachable beside the full-height tactical shell", async ({ page }) => {
  await page.goto("/");
  const opener = page.getByRole("button", { name: "Delivery support", exact: true });
  await expect(opener).toBeVisible();
  const box = await opener.boundingBox();
  const viewportHeight = await page.evaluate(() => window.innerHeight);
  expect(box).not.toBeNull();
  expect(box.y + box.height).toBeLessThanOrEqual(viewportHeight);
});
