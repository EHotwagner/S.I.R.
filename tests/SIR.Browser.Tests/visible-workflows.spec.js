import { expect, test } from "./journey.js";

test("visible mode controls preserve a usable tactical workspace across authoring modes", async ({ page }) => {
  await page.goto("/");
  const workspace = page.getByRole("main", { name: "S.I.R. simulator and editor" });
  await expect(workspace).toBeVisible();

  for (const mode of ["Editor", "Plan", "Simulate", "Review"]) {
    const control = page.getByRole("button", { name: mode, exact: true });
    await control.click();
    await expect(control).toHaveAttribute("aria-pressed", "true");
    await expect(workspace.getByRole("application")).toBeVisible();
  }
});

test("disabled playback names why it is unavailable before a simulation is loaded", async ({ page }) => {
  await page.goto("/");
  const play = page.getByRole("button", { name: "Play tactical timeline", exact: true });
  await expect(play).toBeDisabled();
  await expect(page.getByText(/Play unavailable:/)).toBeVisible();
});

test("a curated sample creates a visible simulator handoff and playback can reset", async ({ page }) => {
  await page.goto("/");
  await page.getByRole("button", { name: "Simulate", exact: true }).click();
  await expect(page.getByRole("button", { name: "Play tactical timeline", exact: true })).toBeDisabled();
  await page.getByRole("button", { name: "Show contextual actions", exact: true }).click();
  await page.getByRole("button", { name: "Open simulator samples", exact: true }).click();
  await page.getByRole("button", { name: /^Run .+ in Simulator$/ }).first().click();

  const play = page.getByRole("button", { name: "Play tactical timeline", exact: true });
  await expect(play).toBeEnabled();
  await play.click();
  const pause = page.getByRole("button", { name: "Pause tactical timeline", exact: true });
  await expect(pause).toBeVisible();
  await pause.click();
  await page.getByRole("button", { name: "Step tactical timeline forward", exact: true }).click();
  await page.getByRole("button", { name: "Go to tactical timeline start", exact: true }).click();
  await expect(page.getByRole("slider", { name: "Current tactical time" })).toHaveValue("0");
});

test("live authority reconnect remains visible through the production command surface", async ({ page }) => {
  await page.goto("/");
  const live = page.getByRole("region", { name: "Authoritative live session" });
  await expect(live).toContainText("live connected", { timeout: 90_000 });
  await page.getByRole("button", { name: "Disconnect the player-visible live session" }).click();
  await expect(live).toContainText("live disconnected");
  await page.getByRole("button", { name: "Reconnect and request the authoritative live snapshot" }).click();
  await expect(live).toContainText("live connected");
});
