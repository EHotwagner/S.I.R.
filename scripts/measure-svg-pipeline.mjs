import { chromium } from "@playwright/test";
import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { gzipSync } from "node:zlib";
import { spawn } from "node:child_process";
import { cpus, platform, release } from "node:os";
import { basename, resolve } from "node:path";
import { execFileSync } from "node:child_process";
import { byteDigest, digest, evaluateArtifactVerdict, evaluateRunFrameVerdict, fixtureIdentityDigest, extractFrameHealth, extractInputToPaint, extractJourneyTrace, extractStages, makeMap, stableJson, summarize, validateDefinitions, workloadRecipe } from "./lib/svg-pipeline-measurement.mjs";

const args = process.argv.slice(2);
const option = (name, fallback) => { const index = args.indexOf(name); return index >= 0 ? args[index + 1] : fallback; };
const out = resolve(option("--out", "artifacts/svg-pipeline"));
const retainDirArgument = option("--retain-dir", null);
const retainDir = retainDirArgument ? resolve(retainDirArgument) : null;
const retainedTracePrefix = "work/231-svg-pipeline-measurement/raw-traces";
const selectedFixtures = option("--fixtures", "all").split(",");
const selectedJourneys = option("--journeys", "all").split(",");
const baseURL = option("--base-url", "http://127.0.0.1:5100");
const definitions = validateDefinitions(JSON.parse(readFileSync(new URL("./svg-pipeline-fixtures.v1.json", import.meta.url))));
const fixtures = definitions.fixtures.filter((fixture) => selectedFixtures.includes("all") || selectedFixtures.includes(fixture.id));
const journeys = definitions.journeys.filter((journey) => selectedJourneys.includes("all") || selectedJourneys.includes(journey));
if (!fixtures.length || !journeys.length) throw new Error("fixture/journey selection is empty");
mkdirSync(out, { recursive: true });
if (retainDir) mkdirSync(retainDir, { recursive: true });

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
  clientManifestSha256: byteDigest(readFileSync("artifacts/publish/wwwroot/.vite/manifest.json")),
  serverAssemblySha256: byteDigest(readFileSync("artifacts/publish/SIR.Server.dll")),
};
const browser = await chromium.launch({ executablePath, args: ["--enable-precise-memory-info", "--js-flags=--expose-gc"] });
const runs = [];
const retainedRuns = [];

async function switchWorkspace(page, name) {
  await page.getByRole("button", { name: "View", exact: true }).click();
  await page.getByRole("menu", { name: "View commands" }).getByRole("menuitem", { name: new RegExp(`^Switch to ${name}\\b`) }).click();
}

async function observeStructure(svg) {
  return svg.evaluate((root) => {
    const viewport = root.getBoundingClientRect();
    const intersectsViewport = (node) => {
      const bounds = node.getBoundingClientRect();
      return bounds.width > 0 && bounds.height > 0
        && bounds.right > viewport.left && bounds.left < viewport.right
        && bounds.bottom > viewport.top && bounds.top < viewport.bottom;
    };
    return {
      totalDom: root.querySelectorAll("*").length,
      byLayer: Object.fromEntries([...root.querySelectorAll("[data-scene-layer]")].map((layer) => [layer.getAttribute("data-scene-layer"), layer.querySelectorAll("*").length])),
      visualUnits: [...root.querySelectorAll("[data-unit-id]")].filter(intersectsViewport).length,
      projectedUnits: Number(root.dataset.visualUnitCount || 0),
      nodeEstimate: Number(root.dataset.visualNodeEstimate || 0),
      source: "production SVG glyph bounds intersecting the production SVG viewport after the fixed camera control",
    };
  });
}

async function perform(page, journey, fixture) {
  const svg = page.locator("#persistent-tactical-svg");
  const box = await svg.boundingBox();
  if (journey === "idle") { await page.waitForTimeout(500); return; }
  if (journey === "pan") { await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2); await page.mouse.down({ button: "right" }); await page.mouse.move(box.x + box.width / 2 + 60, box.y + box.height / 2 + 30, { steps: 6 }); await page.mouse.up({ button: "right" }); }
  if (journey === "zoom") { await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2); await page.mouse.wheel(0, -240); }
  if (journey === "selection") await svg.locator("[data-unit-id]").first().click({ force: true });
  if (journey === "modality-transition") { await switchWorkspace(page, "Plan"); await switchWorkspace(page, "Editor"); }
  if (journey === "dense-overlay") { await page.keyboard.press("Alt+l"); await page.waitForTimeout(100); await page.keyboard.press("Alt+l"); }
  if (journey === "playback") {
    await switchWorkspace(page, "Simulate");
    const advance = page.getByRole("button", { name: "Advance the map simulation one tick", exact: true });
    const recipe = workloadRecipe(fixture);
    const playbackStarted = await page.evaluate(() => performance.now());
    for (let step = 0; step < recipe.playbackSteps; step += 1) {
      if (step > 0) {
        const elapsed = await page.evaluate((origin) => performance.now() - origin, playbackStarted);
        const remaining = step * recipe.eventIntervalMilliseconds - elapsed;
        if (remaining > 0) await page.waitForTimeout(remaining);
      }
      if (await advance.count()) await advance.click();
    }
    await switchWorkspace(page, "Editor");
  }
  await page.waitForTimeout(100);
}

try {
  for (const fixture of fixtures) {
    for (const journey of journeys) {
      const startedAt = new Date().toISOString();
      const context = await browser.newContext({ viewport: { width: Math.max(960, fixture.viewport[0]), height: Math.max(640, fixture.viewport[1]) } });
      const page = await context.newPage();
      const cdp = await context.newCDPSession(page);
      await page.goto(baseURL);
      await switchWorkspace(page, "Editor");
      const showDocument = page.locator("#layout-show-document");
      if (await showDocument.getAttribute("aria-pressed") === "false") { await page.locator("details.tactical-panel-menu").click(); await showDocument.click(); }
      await page.locator('[data-panel-id="document"]').waitFor();
      const collapse = page.locator("#layout-panel-document-collapse");
      if (await page.locator("#editor-map-import").isHidden()) await collapse.click();
      const sceneRevisionBeforeImport = await page.locator("#persistent-tactical-svg").getAttribute("data-scene-revision");
      await page.getByLabel("Import SIR map", { exact: true }).setInputFiles({ name: `${fixture.id}.sir-map`, mimeType: "text/plain", buffer: Buffer.from(makeMap(fixture)) });
      await page.getByRole("alert").filter({ hasText: `Imported map ${fixture.id}.sir-map.` }).waitFor();
      await page.waitForFunction((previous) => {
        const revision = document.querySelector("#persistent-tactical-svg")?.getAttribute("data-scene-revision");
        return Boolean(revision && revision !== previous);
      }, sceneRevisionBeforeImport);
      await page.getByRole("button", { name: "Fit the complete map", exact: true }).click();
      await page.setViewportSize({ width: fixture.viewport[0], height: fixture.viewport[1] });
      const svg = page.locator("#persistent-tactical-svg");
      const box = await svg.boundingBox();
      for (let step = 0; step < 15; step += 1) {
        await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2);
        await page.mouse.wheel(0, -240);
        await page.evaluate(() => new Promise((done) => requestAnimationFrame(() => requestAnimationFrame(done))));
      }
      const controlledStructural = await observeStructure(svg);
      if (controlledStructural.visualUnits !== fixture.visibleDensity) throw new Error(`${fixture.id} observed ${controlledStructural.visualUnits} viewport-intersecting production glyphs before the journey; expected controlled density ${fixture.visibleDensity}`);
      if (controlledStructural.projectedUnits !== fixture.globalUnitCount) throw new Error(`${fixture.id} projected ${controlledStructural.projectedUnits} production glyphs before the journey; expected global count ${fixture.globalUnitCount}`);
      const tracePath = resolve(out, `${fixture.id}--${journey}.trace.json`);
      await cdp.send("Tracing.start", { categories: "devtools.timeline,blink.user_timing,v8,disabled-by-default-devtools.timeline,disabled-by-default-v8.cpu_profiler", transferMode: "ReturnAsStream" });
      await page.waitForTimeout(200);
      await cdp.send("Tracing.recordClockSyncMarker", { syncId: "sir-journey-start" });
      await perform(page, journey, fixture);
      await cdp.send("Tracing.recordClockSyncMarker", { syncId: "sir-journey-end" });
      const complete = new Promise((done) => cdp.once("Tracing.tracingComplete", done));
      await cdp.send("Tracing.end");
      const { stream } = await complete;
      const chunks = [];
      while (true) { const part = await cdp.send("IO.read", { handle: stream }); chunks.push(part.data); if (part.eof) break; }
      await cdp.send("IO.close", { handle: stream });
      const traceText = chunks.join(""); writeFileSync(tracePath, traceText);
      const trace = JSON.parse(traceText);
      const journeyTrace = extractJourneyTrace(trace);
      const traceSha256 = byteDigest(Buffer.from(traceText));
      let retainedPath = tracePath;
      if (retainDir) {
        retainedPath = `${retainedTracePrefix}/${traceSha256}.trace.json.gz`;
        writeFileSync(resolve(retainDir, `${traceSha256}.trace.json.gz`), gzipSync(Buffer.from(traceText), { level: 9, mtime: 0 }));
        retainedRuns.push({ fixture: fixture.id, journey, sha256: traceSha256, path: retainedPath });
      }
      for (let cycle = 0; cycle < definitions.warmupCycles; cycle += 1) { await perform(page, "pan", fixture); await perform(page, "zoom", fixture); await perform(page, "playback", fixture); }
      const heapWarm = await cdp.send("Runtime.getHeapUsage");
      for (let cycle = 0; cycle < definitions.stabilizationCycles; cycle += 1) { await perform(page, "pan", fixture); await perform(page, "zoom", fixture); await perform(page, "playback", fixture); }
      const heapStable = await cdp.send("Runtime.getHeapUsage");
      const structural = await observeStructure(svg);
      const runFrameHealth = { ...extractFrameHealth(journeyTrace), samplingWindow: "clock-sync-bounded named journey only; captured before warm-up and stabilization memory cycles" };
      const runFrameVerdict = evaluateRunFrameVerdict(runFrameHealth, journey, definitions.frameBudget);
      runs.push({ result: runFrameVerdict.result, fixture: fixture.id, fixtureDigest: digest(fixture), workload: workloadRecipe(fixture), journey, startedAt, completedAt: new Date().toISOString(), stages: extractStages(journeyTrace), structural: { global: { mapExtent: fixture.mapExtent, unitCount: fixture.globalUnitCount, supportingListSize: fixture.supportingListSize }, visible: controlledStructural, cameraControl: { fitCompleteMap: true, viewport: fixture.viewport, centerAnchoredWheelSteps: 15, wheelDeltaY: -240 }, postMemoryCycles: structural }, frameHealth: runFrameHealth, frameBudget: runFrameVerdict, inputLatency: extractInputToPaint(journeyTrace, journey), memory: { warm: heapWarm, stabilized: heapStable, usedDelta: heapStable.usedSize - heapWarm.usedSize, warmupCycles: definitions.warmupCycles, stabilizationCycles: definitions.stabilizationCycles, collectionControl: "not-forced" }, trace: { path: retainedPath, sha256: traceSha256 } });
      await context.close();
    }
  }
  let rawTraceManifest = null;
  if (retainDir) {
    rawTraceManifest = { schema: "sir.svg-pipeline-raw-trace-manifest/1", candidate, fixtureDefinitionSha256: fixtureIdentityDigest(definitions), runs: retainedRuns };
    writeFileSync(resolve(retainDir, "..", "raw-trace-manifest.json"), stableJson(rawTraceManifest));
  }
  const artifactVerdict = evaluateArtifactVerdict(runs);
const artifact = { schema: "sir.svg-pipeline-measurement/1", result: artifactVerdict.result, frameBudget: { ...artifactVerdict, declared: definitions.frameBudget }, candidate, buildIdentity, fixtureDefinition: { path: "scripts/svg-pipeline-fixtures.v1.json", sha256: fixtureIdentityDigest(definitions) }, environment: { browserVersion, executableName: basename(executablePath), node: process.version, platform: platform(), release: release(), cpuCount: cpus().length }, selection: { fixtures: fixtures.map((fixture) => fixture.id), journeys }, runs, rawTraceManifest: rawTraceManifest ? { path: "work/231-svg-pipeline-measurement/raw-trace-manifest.json", sha256: digest(rawTraceManifest) } : null, summary: summarize(runs, definitions.materialShareThreshold) };
  writeFileSync(resolve(out, "summary.json"), stableJson(artifact));
  writeFileSync(resolve(out, "measurement.junit.xml"), `<?xml version="1.0" encoding="UTF-8"?>\n<testsuites tests="${runs.length}" failures="0" errors="0" skipped="0"><testsuite name="svg-pipeline-production-chromium" tests="${runs.length}" failures="0" errors="0" skipped="0">${runs.map((run) => `<testcase classname="${run.fixture}" name="${run.journey}"/>`).join("")}</testsuite></testsuites>\n`);
  console.log(`svg-pipeline: PASS runs=${runs.length} next=${artifact.summary.nextBottleneck.stage} artifact=${resolve(out, "summary.json")}`);
} finally { await browser.close(); if (server) server.kill("SIGTERM"); }
