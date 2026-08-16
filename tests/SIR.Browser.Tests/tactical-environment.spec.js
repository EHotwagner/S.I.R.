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

async function identityAt(scope, testId) {
  const value = (await scope.getByTestId(testId).textContent())?.trim() ?? "";
  expect(value).toMatch(canonicalIdentity);
  return value;
}

test("canonical tactical editor revision hands off to fixed-step simulation and exact replay", async ({ page }) => {
  await page.goto("/");
  let environment = await openEnvironmentEditorByPointer(page);

  // Load maintained production content, then author through the same pointer
  // controls exposed to a designer. No test route injects editor state.
  await environment.getByTestId("tactical-load-exterior").click();
  await expect(environment.getByTestId("tactical-preview-status")).toContainText(/loaded/i);
  const authoredIdentity = await displayedIdentity(environment);

  const cover = environment.locator('[data-feature-id="yard-cover"]');
  await expect(cover).toBeVisible();
  await cover.getByTestId("tactical-state-yard-cover-damaged").click();
  await expect(environment.getByTestId("tactical-preview-status")).toContainText(/damaged/i);
  const damagedIdentity = await displayedIdentity(environment);
  expect(damagedIdentity).not.toBe(authoredIdentity);
  const editorRevision = (await environment.getByTestId("tactical-editor-revision").textContent())?.trim() ?? "";
  expect(Number(editorRevision)).toBeGreaterThan(0);
  await expect(environment.getByTestId("tactical-editor-history")).toContainText(/1 undo.*0 redo/i);

  await environment.getByTestId("tactical-undo").click();
  await expect.poll(() => displayedIdentity(environment)).toBe(authoredIdentity);
  await expect(environment.getByTestId("tactical-editor-history")).toContainText(/0 undo.*1 redo/i);
  await environment.getByTestId("tactical-redo").click();
  await expect.poll(() => displayedIdentity(environment)).toBe(damagedIdentity);
  await expect(environment.getByTestId("tactical-editor-revision")).toHaveText(editorRevision);

  await environment.getByTestId("tactical-export").click();
  const interchange = environment.getByTestId("tactical-parcel-interchange");
  const canonicalDocument = await interchange.inputValue();
  expect(canonicalDocument).toMatch(/^SIR-TACTICAL-ENVIRONMENT\|1\n/);
  expect(canonicalDocument).toContain("yard-cover");

  // Cross the real immutable editor-to-Simulate handoff. Runtime commands act
  // only on the pinned handoff, never on the mutable editor sidecar.
  await environment.getByTestId("tactical-enter-simulate").click();
  let simulation = page.getByRole("region", { name: "Tactical environment simulation", exact: true });
  await expect(simulation).toBeVisible();
  await expect(simulation.getByTestId("tactical-runtime-status")).toHaveText("Authoritative tactical environment transfer ready.");
  await expect(simulation.getByTestId("tactical-runtime-revision")).toHaveText(editorRevision);
  const assemblyIdentity = await identityAt(simulation, "tactical-runtime-assembly-identity");
  await expect.poll(() => identityAt(simulation, "tactical-runtime-initial-identity")).toBe(damagedIdentity);
  await expect.poll(() => identityAt(simulation, "tactical-runtime-identity")).toBe(damagedIdentity);
  await expect(simulation.getByTestId("tactical-runtime-state-yard-cover")).toHaveText("damaged");

  // Native Space activation proves keyboard accessibility at the production
  // action control. A fixed step must retain the committed environment state.
  const destroy = simulation.getByTestId("tactical-runtime-action-yard-cover-destroy");
  await destroy.focus();
  await expect(destroy).toBeFocused();
  await page.keyboard.press("Space");
  await expect(simulation.getByTestId("tactical-runtime-status")).toHaveText("Tactical feature yard-cover action applied at tick 0.");
  await expect(simulation.getByTestId("tactical-runtime-state-yard-cover")).toHaveText("destroyed");
  const destroyedIdentity = await identityAt(simulation, "tactical-runtime-identity");
  expect(destroyedIdentity).not.toBe(damagedIdentity);
  await simulation.getByTestId("tactical-runtime-step").click();
  await expect.poll(() => identityAt(simulation, "tactical-runtime-identity")).toBe(destroyedIdentity);

  // Reset reconstructs the immutable handoff while retaining its captured
  // action log; replay must deterministically reproduce the exact identity.
  await simulation.getByTestId("tactical-runtime-reset").click();
  await expect.poll(() => identityAt(simulation, "tactical-runtime-identity")).toBe(damagedIdentity);
  await expect(simulation.getByTestId("tactical-runtime-state-yard-cover")).toHaveText("damaged");
  await simulation.getByTestId("tactical-runtime-replay").click();
  await expect(simulation.getByTestId("tactical-runtime-status")).toHaveText("Tactical environment action replay completed.");
  await expect.poll(() => identityAt(simulation, "tactical-runtime-identity")).toBe(destroyedIdentity);
  await expect.poll(() => identityAt(simulation, "tactical-runtime-assembly-identity")).toBe(assemblyIdentity);

  // Reboot the shipped composition and enter the tool through its real global
  // keyboard command. Importing the captured document must reconstruct the
  // same canonical editor revision and immutable simulation input.
  await page.reload();
  await page.getByRole("button", { name: "Editor", exact: true }).click();
  await page.getByRole("application").focus();
  await page.keyboard.press("Shift+E");
  environment = page.getByRole("region", { name: "Tactical environment authoring", exact: true });
  await expect(environment).toBeVisible();
  await environment.getByTestId("tactical-parcel-interchange").fill(canonicalDocument);
  await environment.getByTestId("tactical-import").click();
  await expect(environment.getByTestId("tactical-preview-status")).toContainText(/loaded|ready|valid/i);
  await expect.poll(() => displayedIdentity(environment)).toBe(damagedIdentity);

  await environment.getByTestId("tactical-enter-simulate").click();
  simulation = page.getByRole("region", { name: "Tactical environment simulation", exact: true });
  await expect(simulation).toBeVisible();
  await expect.poll(() => identityAt(simulation, "tactical-runtime-assembly-identity")).toBe(assemblyIdentity);
  await expect.poll(() => identityAt(simulation, "tactical-runtime-initial-identity")).toBe(damagedIdentity);
  await simulation.getByTestId("tactical-runtime-action-yard-cover-destroy").click();
  await expect.poll(() => identityAt(simulation, "tactical-runtime-identity")).toBe(destroyedIdentity);
});
