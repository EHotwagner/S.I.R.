import { chromium } from "@playwright/test";
import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { spawn } from "node:child_process";
import { cpus, platform, release } from "node:os";
import { resolve } from "node:path";
import { execFileSync } from "node:child_process";
import { digest, extractStages, makeMap, stableJson, summarize, validateDefinitions } from "./lib/svg-pipeline-measurement.mjs";

const args = process.argv.slice(2);
const option = (name, fallback) => { const index = args.indexOf(name); return index >= 0 ? args[index + 1] : fallback; };
const out = resolve(option("--out", "artifacts/svg-pipeline"));
const selectedFixtures = option("--fixtures", "all").split(",");
const selectedJourneys = option("--journeys", "all").split(",");
const baseURL = option("--base-url", "http://127.0.0.1:5100");
const definitions = validateDefinitions(JSON.parse(readFileSync(new URL("./svg-pipeline-fixtures.v1.json", import.meta.url))));
const fixtures = definitions.fixtures.filter((fixture) => selectedFixtures.includes("all") || selectedFixtures.includes(fixture.id));
const journeys = definitions.journeys.filter((journey) => selectedJourneys.includes("all") || selectedJourneys.includes(journey));
if (!fixtures.length || !journeys.length) throw new Error("fixture/journey selection is empty");
mkdirSync(out, { recursive: true });

let server;
if (!args.includes("--base-url")) {
  if (!existsSync("artifacts/publish/SIR.Server.dll")) throw new Error("Release publish is missing; run npm run build:client and dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release -o artifacts/publish, then copy artifacts/client into it");
  server = spawn("dotnet", ["SIR.Server.dll", "--urls", baseURL], { cwd: resolve("artifacts/publish"), env: { ...process.env, ASPNETCORE_ENVIRONMENT: "Development", SIR_ALLOW_ANONYMOUS_LIVE_SESSIONS: "true", SIR_LIVE_MAX_BOOTSTRAPS_PER_MINUTE: "64" }, stdio: "ignore" });
  for (let attempt = 0; attempt < 60; attempt += 1) { try { if ((await fetch(baseURL)).ok) break; } catch {} await new Promise((done) => setTimeout(done, 250)); }
}

const executablePath = process.env.PLAYWRIGHT_EXECUTABLE_PATH || chromium.executablePath();
const browserVersion = execFileSync(executablePath, ["--version"], { encoding: "utf8" }).trim();
const candidate = { commit: execFileSync("git", ["rev-parse", "HEAD"], { encoding: "utf8" }).trim(), tree: execFileSync("git", ["rev-parse", "HEAD^{tree}"], { encoding: "utf8" }).trim() };
const buildIdentity = {
  clientManifestSha256: digest(readFileSync("artifacts/publish/.vite/manifest.json")),
  serverAssemblySha256: digest(readFileSync("artifacts/publish/SIR.Server.dll")),
};
const browser = await chromium.launch({ executablePath, args: ["--enable-precise-memory-info", "--js-flags=--expose-gc"] });
const runs = [];

async function switchWorkspace(page, name) {
  await page.getByRole("button", { name: "View", exact: true }).click();
  await page.getByRole("menu", { name: "View commands" }).getByRole("menuitem", { name: new RegExp(`^Switch to ${name}\\b`) }).click();
}

async function perform(page, journey) {
  const started = await page.evaluate(() => performance.now());
  const svg = page.locator("#persistent-tactical-svg");
  const box = await svg.boundingBox();
  if (journey === "idle") await page.waitForTimeout(500);
  if (journey === "pan") { await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2); await page.mouse.down({ button: "right" }); await page.mouse.move(box.x + box.width / 2 + 60, box.y + box.height / 2 + 30, { steps: 6 }); await page.mouse.up({ button: "right" }); }
  if (journey === "zoom") { await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2); await page.mouse.wheel(0, -240); }
  if (journey === "selection") await svg.locator("[data-unit-id]").first().click({ force: true });
  if (journey === "modality-transition") { await switchWorkspace(page, "Plan"); await switchWorkspace(page, "Editor"); }
  if (journey === "dense-overlay") { await page.keyboard.press("Alt+l"); await page.waitForTimeout(100); await page.keyboard.press("Alt+l"); }
  if (journey === "playback") { await switchWorkspace(page, "Simulate"); const advance = page.getByRole("button", { name: "Advance the map simulation one tick", exact: true }); if (await advance.count()) await advance.click(); await switchWorkspace(page, "Editor"); }
  return page.evaluate((origin) => new Promise((done) => requestAnimationFrame(() => requestAnimationFrame(() => done(performance.now() - origin)))), started);
}

try {
  for (const fixture of fixtures) {
    for (const journey of journeys) {
      const startedAt = new Date().toISOString();
      const context = await browser.newContext({ viewport: { width: Math.max(960, fixture.viewport[0]), height: Math.max(640, fixture.viewport[1]) } });
      const page = await context.newPage();
      const cdp = await context.newCDPSession(page);
      await page.addInitScript(() => { window.__sirFrameIntervals = []; let last = performance.now(); const sample = (now) => { window.__sirFrameIntervals.push(now - last); last = now; requestAnimationFrame(sample); }; requestAnimationFrame(sample); });
      await page.goto(baseURL);
      await switchWorkspace(page, "Editor");
      const showDocument = page.locator("#layout-show-document");
      if (await showDocument.getAttribute("aria-pressed") === "false") { await page.locator("details.tactical-panel-menu").click(); await showDocument.click(); }
      await page.locator('[data-panel-id="document"]').waitFor();
      const collapse = page.locator("#layout-panel-document-collapse");
      if (await page.locator("#editor-map-import").isHidden()) await collapse.click();
      await page.getByLabel("Import SIR map", { exact: true }).setInputFiles({ name: `${fixture.id}.sir-map`, mimeType: "text/plain", buffer: Buffer.from(makeMap(fixture)) });
      await page.getByRole("alert").filter({ hasText: `Imported map ${fixture.id}.sir-map.` }).waitFor();
      await page.setViewportSize({ width: fixture.viewport[0], height: fixture.viewport[1] });
      await page.evaluate(() => { window.__sirFrameIntervals = []; });
      const tracePath = resolve(out, `${fixture.id}--${journey}.trace.json`);
      await cdp.send("Tracing.start", { categories: "devtools.timeline,blink.user_timing,v8,disabled-by-default-devtools.timeline,disabled-by-default-v8.cpu_profiler", transferMode: "ReturnAsStream" });
      const inputLatencyMilliseconds = await perform(page, journey);
      const complete = new Promise((done) => cdp.once("Tracing.tracingComplete", done));
      await cdp.send("Tracing.end");
      const { stream } = await complete;
      const chunks = [];
      while (true) { const part = await cdp.send("IO.read", { handle: stream }); chunks.push(part.data); if (part.eof) break; }
      await cdp.send("IO.close", { handle: stream });
      const traceText = chunks.join(""); writeFileSync(tracePath, traceText);
      const trace = JSON.parse(traceText);
      for (let cycle = 0; cycle < definitions.warmupCycles; cycle += 1) { await perform(page, "pan"); await perform(page, "zoom"); await perform(page, "playback"); }
      const heapWarm = await cdp.send("Runtime.getHeapUsage");
      for (let cycle = 0; cycle < definitions.stabilizationCycles; cycle += 1) { await perform(page, "pan"); await perform(page, "zoom"); await perform(page, "playback"); }
      const heapStable = await cdp.send("Runtime.getHeapUsage");
      const structural = await page.locator("#persistent-tactical-svg").evaluate((root) => ({ totalDom: root.querySelectorAll("*").length, byLayer: Object.fromEntries([...root.querySelectorAll("[data-scene-layer]")].map((layer) => [layer.getAttribute("data-scene-layer"), layer.querySelectorAll("*").length])), visualUnits: Number(root.dataset.visualUnitCount || 0), nodeEstimate: Number(root.dataset.visualNodeEstimate || 0) }));
      const intervals = await page.evaluate(() => window.__sirFrameIntervals || []);
      const longTasks = (trace.traceEvents || []).filter((event) => event.name === "RunTask" && Number(event.dur || 0) >= 50_000).map((event) => Number((event.dur / 1000).toFixed(3)));
      runs.push({ fixture: fixture.id, fixtureDigest: digest(fixture), journey, startedAt, completedAt: new Date().toISOString(), result: "pass", stages: extractStages(trace), structural: { global: { mapExtent: fixture.mapExtent, unitCount: fixture.globalUnitCount, supportingListSize: fixture.supportingListSize }, visible: structural }, frameHealth: { samples: intervals.length, droppedFrames: intervals.filter((value) => value > 25).length, longTasks }, inputLatency: { available: true, milliseconds: Number(inputLatencyMilliseconds.toFixed(3)), source: "Playwright production interaction start through two requestAnimationFrame callbacks" }, memory: { warm: heapWarm, stabilized: heapStable, usedDelta: heapStable.usedSize - heapWarm.usedSize, warmupCycles: definitions.warmupCycles, stabilizationCycles: definitions.stabilizationCycles, collectionControl: "not-forced" }, trace: { path: tracePath, sha256: digest(traceText) } });
      await context.close();
    }
  }
  const artifact = { schema: "sir.svg-pipeline-measurement/1", result: "pass", candidate, buildIdentity, fixtureDefinition: { path: "scripts/svg-pipeline-fixtures.v1.json", sha256: digest(definitions) }, environment: { browserVersion, executablePath, node: process.version, platform: platform(), release: release(), cpuCount: cpus().length }, selection: { fixtures: fixtures.map((fixture) => fixture.id), journeys }, runs, summary: summarize(runs, definitions.materialShareThreshold) };
  writeFileSync(resolve(out, "summary.json"), stableJson(artifact));
  writeFileSync(resolve(out, "measurement.junit.xml"), `<?xml version="1.0" encoding="UTF-8"?>\n<testsuites tests="${runs.length}" failures="0" errors="0" skipped="0"><testsuite name="svg-pipeline-production-chromium" tests="${runs.length}" failures="0" errors="0" skipped="0">${runs.map((run) => `<testcase classname="${run.fixture}" name="${run.journey}"/>`).join("")}</testsuite></testsuites>\n`);
  console.log(`svg-pipeline: PASS runs=${runs.length} next=${artifact.summary.nextBottleneck.stage} artifact=${resolve(out, "summary.json")}`);
} finally { await browser.close(); if (server) server.kill("SIGTERM"); }
