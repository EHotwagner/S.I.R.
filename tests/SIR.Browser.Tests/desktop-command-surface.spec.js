import { expect, test } from "@playwright/test";

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

test("toolbar customization persists, reorders, and resets through the production route", async ({ page }) => {
  await page.goto("/");
  await page.getByRole("button", { name: "Customize toolbar", exact: true }).click();
  const customize = page.getByRole("region", { name: "Customize top toolbar" });
  await expect(customize).toBeVisible();
  await customize.getByRole("button", { name: "Move Switch to Simulate earlier", exact: true }).click();
  const toolbarLabels = await page.getByRole("toolbar", { name: "Customizable top toolbar" }).getByRole("button").allTextContents();
  expect(toolbarLabels.indexOf("Switch to Simulate")).toBeLessThan(toolbarLabels.indexOf("Switch to Plan"));
  await customize.getByRole("button", { name: "Remove Switch to Review from toolbar" }).click();
  await page.reload();
  await expect(page.getByRole("toolbar", { name: "Customizable top toolbar" }).getByRole("button", { name: "Switch to Review" })).toHaveCount(0);
  await page.getByRole("button", { name: "Customize toolbar", exact: true }).click();
  await expect(customize).toBeVisible();
  await customize.getByRole("button", { name: "Restore the documented default top toolbar", exact: true }).click();
  await expect(page.getByRole("toolbar", { name: "Customizable top toolbar" }).getByRole("button", { name: "Switch to Review" })).toBeVisible();
});

test("an intentionally empty toolbar persists until reset", async ({ page }) => {
  await page.goto("/");
  await page.getByRole("button", { name: "Customize toolbar", exact: true }).click();
  const customize = page.getByRole("region", { name: "Customize top toolbar" });
  const removals = customize.getByRole("button", { name: /^Remove .+ from toolbar$/ });
  while (await removals.count()) await removals.first().click();
  const toolbar = page.getByRole("toolbar", { name: "Customizable top toolbar" });
  await expect(toolbar.getByRole("button")).toHaveCount(1);
  await page.reload();
  await expect(toolbar.getByRole("button")).toHaveCount(1);
  await page.getByRole("button", { name: "Customize toolbar", exact: true }).click();
  await customize.getByRole("button", { name: "Restore the documented default top toolbar", exact: true }).click();
  await expect(toolbar.getByRole("button", { name: "Switch to Review" })).toBeVisible();
});

test("compact desktop chrome keeps a reachable overflow menu", async ({ page }) => {
  await page.setViewportSize({ width: 360, height: 720 });
  await page.goto("/");
  const menuBar = page.getByRole("menubar");
  await expect(menuBar).toBeVisible();
  await page.getByRole("button", { name: "Help", exact: true }).click();
  await expect(page.getByRole("menu", { name: "Help commands" })).toBeVisible();
});
