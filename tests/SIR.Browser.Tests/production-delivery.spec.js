import { test, expect } from "@playwright/test";

test("Release delivery uses cache-safe compression and defers spatial diagnostics", async ({ page }) => {
  const client = await page.context().newCDPSession(page);
  await client.send("Network.enable");
  await client.send("Network.emulateNetworkConditions", {
    offline: false,
    latency: 400,
    downloadThroughput: 50_000,
    uploadThroughput: 20_000,
    connectionType: "cellular3g",
  });
  await client.send("Emulation.setCPUThrottlingRate", { rate: 4 });

  const responses = [];
  page.on("response", (response) => responses.push(response));
  await page.goto("/");

  const entry = responses.find((response) => response.url().includes("/content/sir-client/v1/app.js"));
  expect(entry).toBeTruthy();
  expect(entry.headers()["cache-control"]).toBe("public,max-age=0,must-revalidate");
  expect(entry.headers()["vary"]).toContain("Accept-Encoding");

  const initialDeferred = responses.filter((response) => response.url().includes("RulesExplorer-")).length;
  const responseBytes = async (selected) =>
    (await Promise.all(selected.map((response) => response.body().then((body) => body.byteLength)))).reduce((total, bytes) => total + bytes, 0);
  let initialBytes = await responseBytes(responses);
  if (process.env.SIR_DELIVERY_BROWSER_MUTATE_SUBJECT === "initial-bytes") initialBytes += 1_250_001;
  // Initial boot budget v2 (2026-08-15). This bounds the default production
  // route, not every future product route; growth must defer or deliberately
  // rebaseline this versioned contract with a new failing mutation receipt.
  const maximumInitialBytes = Number(process.env.SIR_DELIVERY_MAX_INITIAL_ROUTE_BYTES ?? 1_250_000);
  expect(initialDeferred).toBe(0);
  await page.getByRole("button", { name: "Simulate", exact: true }).click();
  await page.getByRole("button", { name: "Show contextual actions", exact: true }).click();
  await page.getByRole("button", { name: "Open simulator samples", exact: true }).click();
  await page.getByText("Troll assault", { exact: true }).click();
  await page.getByRole("button", { name: "Run Troll assault in Simulator", exact: true }).click();
  await page.locator("#persistent-layer-units [data-unit-id]").first().click();
  await page.getByRole("button", { name: "View", exact: true }).click();
  await page.getByRole("menuitem", { name: /Spatial diagnostics/ }).click();
  const diagnostics = page.getByRole("region", { name: "Selected unit spatial diagnostics", exact: true });
  await expect(diagnostics).toBeVisible();
  await expect(diagnostics.getByText(/ExactLineOfSight · Found/)).toBeVisible();
  const boundedPath = diagnostics.locator("details").filter({ hasText: "BoundedPath · Found" });
  const renderedPath = boundedPath.getByText("Authoritative path", { exact: true }).locator("+ dd");
  await expect(renderedPath).not.toHaveText("none");
  await expect(renderedPath).toHaveText(/^\(\d+,\d+\)(?:, \(\d+,\d+\))+$/);
  await expect(diagnostics.getByText("SIR.Simulation.SpatialQuery.evaluate", { exact: true }).first()).toBeVisible();
  expect(responses.some((response) => response.url().includes("RulesExplorer-"))).toBe(true);
  let deferredBytes = await responseBytes(responses.filter((response) => response.url().includes("RulesExplorer-")));
  if (process.env.SIR_DELIVERY_BROWSER_MUTATE_SUBJECT === "deferred-bytes") deferredBytes += 65_536;
  const diagnosticResponses = responses.filter((response) => response.url().includes("/api/spatial/diagnostics"));
  expect(diagnosticResponses).toHaveLength(1);
  expect(diagnosticResponses[0].status()).toBe(200);
  const diagnosticApiBytes = await responseBytes(diagnosticResponses);
  const maximumDeferredBytes = Number(process.env.SIR_DELIVERY_MAX_DEFERRED_ROUTE_BYTES ?? 65_536);
  expect(initialBytes).toBeGreaterThan(0);
  expect(deferredBytes).toBeGreaterThan(0);
  expect(initialBytes).toBeLessThanOrEqual(maximumInitialBytes);
  expect(deferredBytes).toBeLessThanOrEqual(maximumDeferredBytes);
  const initialRouteHeadroomBytes = maximumInitialBytes - initialBytes;
  expect(initialRouteHeadroomBytes).toBeGreaterThanOrEqual(0);
  console.log(JSON.stringify({ schema: "sir-production-delivery-route-v1", throttle: "Slow-3G/4x CPU", deferredChunk: "RulesExplorer", initialResponseBytes: initialBytes, maximumInitialResponseBytes: maximumInitialBytes, initialRouteHeadroomBytes, deferredActivationBytes: deferredBytes, diagnosticApiBytes }));

  const engine = await page.request.get("/engines/0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20/worker.js", {
    headers: { "Accept-Encoding": "gzip" },
  });
  expect(engine.headers()["cache-control"]).toBe("public,max-age=31536000,immutable");
  expect(engine.headers()["vary"]).toContain("Accept-Encoding");
});
