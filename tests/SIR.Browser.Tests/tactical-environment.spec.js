import { expect, test } from "./journey.js";

const canonicalIdentity = /^[0-9a-f]{64}$/;

async function openEnvironmentEditorByPointer(page) {
  await page.getByRole("button", { name: "Editor", exact: true }).click();
  await page.getByRole("button", { name: "Environment", exact: true }).click();
  const environment = page.getByRole("region", { name: "Tactical environment authoring", exact: true });
  await expect(environment).toBeVisible();
  return environment;
}

async function displayedIdentity(environment) {
  const value = (await environment.getByTestId("tactical-content-identity").textContent())?.trim() ?? "";
  expect(value).toMatch(canonicalIdentity);
  return value;
}

test("production tactical environment editor authors by pointer and replays exact canonical content by keyboard", async ({ page }) => {
  await page.goto("/");
  let environment = await openEnvironmentEditorByPointer(page);

  // Load maintained production content, then author through the same pointer
  // controls exposed to a designer. No test route injects editor state.
  await environment.getByTestId("tactical-load-interior").click();
  await expect(environment.getByTestId("tactical-preview-status")).toContainText(/loaded/i);
  const authoredIdentity = await displayedIdentity(environment);

  const wall = environment.locator('[data-feature-id="interior-wall"]');
  await expect(wall).toBeVisible();
  await wall.getByTestId("tactical-state-interior-wall-breached").click();
  await expect(environment.getByTestId("tactical-preview-status")).toContainText(/breached/i);
  const breachedIdentity = await displayedIdentity(environment);
  expect(breachedIdentity).not.toBe(authoredIdentity);

  await environment.getByTestId("tactical-undo").click();
  await expect.poll(() => displayedIdentity(environment)).toBe(authoredIdentity);
  await environment.getByTestId("tactical-redo").click();
  await expect.poll(() => displayedIdentity(environment)).toBe(breachedIdentity);

  // Exercise the production play/action seam from keyboard accessibility,
  // then restore the authored snapshot and export its canonical envelope.
  const destroy = wall.getByTestId("tactical-action-interior-wall-destroy");
  await destroy.focus();
  await expect(destroy).toBeFocused();
  await page.keyboard.press("Space");
  await expect(environment.getByTestId("tactical-preview-status")).toContainText(/action applied/i);
  await expect(environment).toContainText(/revision 1/i);
  const destroyedIdentity = await displayedIdentity(environment);
  expect(destroyedIdentity).not.toBe(breachedIdentity);

  await environment.getByTestId("tactical-export").click();
  const interchange = environment.getByTestId("tactical-parcel-interchange");
  const canonicalDocument = await interchange.inputValue();
  expect(canonicalDocument).toMatch(/^SIR-TACTICAL-ENVIRONMENT\|1\n/);
  expect(canonicalDocument).toContain("interior-wall");

  // Reboot the shipped composition and enter the tool through its real global
  // keyboard command. Importing the captured document must reconstruct the
  // exact displayed identity rather than merely an equivalent-looking scene.
  await page.reload();
  await page.getByRole("button", { name: "Editor", exact: true }).click();
  await page.getByRole("application").focus();
  await page.keyboard.press("Shift+E");
  environment = page.getByRole("region", { name: "Tactical environment authoring", exact: true });
  await expect(environment).toBeVisible();
  await environment.getByTestId("tactical-parcel-interchange").fill(canonicalDocument);
  await environment.getByTestId("tactical-import").click();
  await expect(environment.getByTestId("tactical-preview-status")).toContainText(/loaded|ready|valid/i);
  await expect.poll(() => displayedIdentity(environment)).toBe(breachedIdentity);

  // The replayed document remains actionable through production controls.
  const replayedWall = environment.locator('[data-feature-id="interior-wall"]');
  await replayedWall.getByTestId("tactical-action-interior-wall-destroy").click();
  await expect(environment.getByTestId("tactical-preview-status")).toContainText(/action applied/i);
  await expect(environment).toContainText(/revision 1/i);
  await expect.poll(() => displayedIdentity(environment)).toBe(destroyedIdentity);
});
