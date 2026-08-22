import { chromium } from "@playwright/test";
import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { gzipSync } from "node:zlib";
import { spawn } from "node:child_process";
import { cpus, platform, release } from "node:os";
import { basename, resolve } from "node:path";
import { execFileSync } from "node:child_process";
import { byteDigest, digest, extractFrameHealth, extractInputToPaint, extractJourneyTrace, extractStages, makeMap, stableJson, summarize, validateCullingMeasurementContract, validateDefinitions, workloadRecipe } from "./lib/svg-pipeline-measurement.mjs";

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
validateCullingMeasurementContract(readFileSync(new URL("./measure-svg-pipeline.mjs", import.meta.url), "utf8"));
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
  clientManifestSha256: byteDigest(readFileSync("artifacts/publish/.vite/manifest.json")),
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
    const unitNodes = [...root.querySelectorAll("[data-unit-id]")];
    const rosterHost = document.querySelector('[data-persistent-roster-host="true"]');
    const selectionHost = document.querySelector('[data-persistent-selection-host="true"]');
    const toolsHost = document.querySelector('[data-persistent-tools-host="true"]');
    const selectionIds = [...(selectionHost?.querySelectorAll("[id]") || [])].map((node) => node.id);
    const inactiveSelectionPrefixesValid = [...(selectionHost?.querySelectorAll("[data-selection-mode][hidden]") || [])].every((owner) => {
      const slug = owner.getAttribute("data-selection-mode") === "EditorWorkspace" ? "editor"
        : owner.getAttribute("data-selection-mode") === "PlanningWorkspace" ? "plan"
          : owner.getAttribute("data-selection-mode") === "SimulatorWorkspace" ? "simulate" : "review";
      const prefix = `inactive-${slug}-selection-`;
      return [...owner.querySelectorAll("[id]")].every((node) => node.id.startsWith(prefix))
        && [...owner.querySelectorAll("label[for]")].every((label) => {
          const target = label.getAttribute("for");
          return target.startsWith(prefix) && owner.querySelector(`#${CSS.escape(target)}`) !== null;
        });
    });
    const toolsIds = [...(toolsHost?.querySelectorAll("[id]") || [])].map((node) => node.id);
    const inactiveToolsSafe = [...(toolsHost?.querySelectorAll("[data-tools-mode][hidden]") || [])].every((owner) => {
      const slug = owner.getAttribute("data-tools-mode") === "EditorWorkspace" ? "editor"
        : owner.getAttribute("data-tools-mode") === "PlanningWorkspace" ? "plan"
          : owner.getAttribute("data-tools-mode") === "SimulatorWorkspace" ? "simulate" : "review";
      const prefix = `inactive-${slug}-tools-`;
      return [...owner.querySelectorAll("[id]")].every((node) => node.id.startsWith(prefix))
        && [...owner.querySelectorAll("label[for]")].every((label) => {
          const target = label.getAttribute("for");
          return target.startsWith(prefix) && owner.querySelector(`#${CSS.escape(target)}`) !== null;
        })
        && [...owner.querySelectorAll('input[type="file"]')].every((input) => input.files.length === 0);
    });
    const layerRevisionNames = ["terrain", "edges", "units", "routes", "annotations", "effects", "overlays", "accessibility"];
    return {
      appViewConstructions: Number(document.querySelector('main[aria-label="S.I.R. simulator and editor"]')?.getAttribute("data-app-view-constructions") || 0),
      appViewTransitionLog: document.querySelector('main[aria-label="S.I.R. simulator and editor"]')?.getAttribute("data-app-view-transition-log") || "",
      appRegionProfile: document.querySelector('main[aria-label="S.I.R. simulator and editor"]')?.getAttribute("data-app-region-profile") || "",
      editorProjectionConstructions: Number(root.getAttribute("data-editor-projection-constructions") || 0),
      terrainLayerConstructions: Number(root.querySelector("#persistent-layer-terrain")?.getAttribute("data-layer-constructions") || 0),
      leftSidebarConstructions: Number(document.querySelector("#tactical-sidebar-left")?.getAttribute("data-sidebar-constructions") || 0),
      rightSidebarConstructions: Number(document.querySelector("#tactical-sidebar-right")?.getAttribute("data-sidebar-constructions") || 0),
      persistentRosterNodes: rosterHost?.querySelectorAll("*").length || 0,
      activeRosterCount: rosterHost?.querySelectorAll("[data-roster-mode]:not([hidden]):not([inert])").length || 0,
      inactiveRosterIdCount: rosterHost?.querySelectorAll("[data-roster-mode][hidden] [id]").length || 0,
      tacticalSvgRootCount: document.querySelectorAll("#persistent-tactical-svg").length,
      tacticalTimelineCount: document.querySelectorAll('.tactical-timeline[aria-label="Unified tactical timeline"]').length,
      rosterConstructions: Object.fromEntries([...(rosterHost?.querySelectorAll("[data-roster-mode]") || [])].map((owner) => [owner.getAttribute("data-roster-mode"), Number(owner.querySelector(".persistent-roster-owner")?.getAttribute("data-roster-constructions") || 0)])),
      persistentSelectionNodes: selectionHost?.querySelectorAll("*").length || 0,
      activeSelectionCount: selectionHost?.querySelectorAll("[data-selection-mode]:not([hidden]):not([inert])").length || 0,
      selectionIdsUnique: new Set(selectionIds).size === selectionIds.length,
      inactiveSelectionPrefixesValid,
      activeEditorControllerIdCount: document.querySelectorAll("#editor-unit-controller").length,
      selectionConstructions: Object.fromEntries([...(selectionHost?.querySelectorAll("[data-selection-mode]") || [])].map((owner) => [owner.getAttribute("data-selection-mode"), Number(owner.querySelector(".persistent-selection-owner")?.getAttribute("data-selection-constructions") || 0)])),
      persistentToolsNodes: toolsHost?.querySelectorAll("*").length || 0,
      activeToolsCount: toolsHost?.querySelectorAll("[data-tools-mode]:not([hidden]):not([inert])").length || 0,
      toolsIdsUnique: new Set(toolsIds).size === toolsIds.length,
      inactiveToolsSafe,
      activeTerrainBrushIdCount: document.querySelectorAll("#terrain-brush-size").length,
      toolsConstructions: Object.fromEntries([...(toolsHost?.querySelectorAll("[data-tools-mode]") || [])].map((owner) => [owner.getAttribute("data-tools-mode"), Number(owner.querySelector(".persistent-tools-owner")?.getAttribute("data-tools-constructions") || 0)])),
      layerRevisions: Object.fromEntries(layerRevisionNames.map((name) => [name, root.getAttribute(`data-layer-revision-${name}`)])),
      totalDom: root.querySelectorAll("*").length,
      byLayer: Object.fromEntries([...root.querySelectorAll("[data-scene-layer]")].map((layer) => [layer.getAttribute("data-scene-layer"), layer.querySelectorAll("*").length])),
      visualUnits: unitNodes.filter(intersectsViewport).length,
      emittedUnits: Number(root.dataset.visualUnitCount || 0),
      candidatePrimitives: Number(root.dataset.viewportCandidatePrimitives || 0),
      globalPrimitives: Number(root.dataset.viewportGlobalPrimitives || 0),
      nodeEstimate: Number(root.dataset.visualNodeEstimate || 0),
      source: "production SVG glyph bounds intersecting the production SVG viewport after the fixed camera control",
    };
  });
}

async function perform(page, journey, fixture) {
  const svg = page.locator("#persistent-tactical-svg");
  const box = await svg.boundingBox();
  const transitions = [];
  const captureTransition = async (label) => transitions.push(await page.evaluate((step) => {
    const root = document.querySelector("#persistent-tactical-svg");
    const app = document.querySelector('main[aria-label="S.I.R. simulator and editor"]');
    return {
      step,
      appViewConstructions: Number(app?.getAttribute("data-app-view-constructions") || 0),
      appViewTransitionLog: app?.getAttribute("data-app-view-transition-log") || "",
      terrainLayerConstructions: Number(root?.querySelector("#persistent-layer-terrain")?.getAttribute("data-layer-constructions") || 0),
      leftSidebarConstructions: Number(document.querySelector("#tactical-sidebar-left")?.getAttribute("data-sidebar-constructions") || 0),
      rightSidebarConstructions: Number(document.querySelector("#tactical-sidebar-right")?.getAttribute("data-sidebar-constructions") || 0),
      activeModality: document.querySelector("#tactical-workscreen-region")?.getAttribute("data-active-modality") || "",
      sceneRevision: root?.getAttribute("data-scene-revision") || "",
      terrainRevision: root?.getAttribute("data-layer-revision-terrain") || "",
      unitsRevision: root?.getAttribute("data-layer-revision-units") || "",
      planningWorkerStatus: document.querySelector(".planning-worker-status")?.textContent || "",
      timelineCursor: document.querySelector(".tactical-timeline")?.getAttribute("data-time-cursor") || "",
      timelineCommittedThrough: document.querySelector(".tactical-timeline")?.getAttribute("data-committed-through") || "",
    };
  }, label));
  if (journey === "idle") { await page.waitForTimeout(500); return transitions; }
  if (journey === "pan") { await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2); await page.mouse.down({ button: "right" }); await page.mouse.move(box.x + box.width / 2 + 60, box.y + box.height / 2 + 30, { steps: 6 }); await page.mouse.up({ button: "right" }); }
  if (journey === "zoom") { await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2); await page.mouse.wheel(0, -240); }
  if (journey === "selection") await svg.locator("[data-unit-id]").first().click({ force: true });
  if (journey === "modality-transition") {
    await switchWorkspace(page, "Plan");
    await captureTransition("plan-accepted");
    await switchWorkspace(page, "Editor");
    await captureTransition("editor-accepted");
  }
  if (journey === "dense-overlay") { await page.keyboard.press("Alt+l"); await page.waitForTimeout(100); await page.keyboard.press("Alt+l"); }
  if (journey === "playback") {
    await switchWorkspace(page, "Simulate");
    await captureTransition("simulate-accepted");
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
      await captureTransition(`advance-${step + 1}-accepted`);
    }
    await switchWorkspace(page, "Editor");
    await captureTransition("editor-accepted");
  }
  await page.waitForTimeout(100);
  return transitions;
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
      await page.setViewportSize({ width: fixture.viewport[0], height: fixture.viewport[1] });
      await page.waitForFunction(() => {
        const root = document.querySelector("#persistent-tactical-svg");
        if (!(root instanceof SVGSVGElement)) return false;
        const bounds = root.getBoundingClientRect();
        const viewBox = root.viewBox.baseVal;
        return Math.abs(viewBox.width - bounds.width) <= 1
          && Math.abs(viewBox.height - bounds.height) <= 1;
      });
      await page.locator('button[aria-label="Fit the complete map"]').evaluate((button) => button.click());
      const svg = page.locator("#persistent-tactical-svg");
      const box = await svg.boundingBox();
      for (let step = 0; step < 15; step += 1) {
        await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2);
        await page.mouse.wheel(0, -240);
        await page.evaluate(() => new Promise((done) => requestAnimationFrame(() => requestAnimationFrame(done))));
      }
      const controlledStructural = await observeStructure(svg);
      if (controlledStructural.activeRosterCount !== 1 || controlledStructural.inactiveRosterIdCount !== 0 || controlledStructural.tacticalSvgRootCount !== 1 || controlledStructural.tacticalTimelineCount !== 1) {
        throw new Error(`${fixture.id} persistent roster ownership/accessibility contract failed`);
      }
      const rosterNodeLimit = fixture.globalUnitCount * 8 + 96;
      if (controlledStructural.persistentRosterNodes > rosterNodeLimit) {
        throw new Error(`${fixture.id} persistent roster retained ${controlledStructural.persistentRosterNodes} nodes; limit ${rosterNodeLimit}`);
      }
      if (controlledStructural.activeSelectionCount !== 1 || !controlledStructural.selectionIdsUnique || !controlledStructural.inactiveSelectionPrefixesValid || controlledStructural.activeEditorControllerIdCount > 1) {
        throw new Error(`${fixture.id} persistent selection ownership/ID contract failed`);
      }
      if (controlledStructural.persistentSelectionNodes > 192) {
        throw new Error(`${fixture.id} persistent selection retained ${controlledStructural.persistentSelectionNodes} nodes; limit 192`);
      }
      if (controlledStructural.activeToolsCount !== 1 || !controlledStructural.toolsIdsUnique || !controlledStructural.inactiveToolsSafe || controlledStructural.activeTerrainBrushIdCount !== 1) {
        throw new Error(`${fixture.id} persistent tools ownership/ID/side-effect contract failed`);
      }
      if (controlledStructural.persistentToolsNodes > 160) {
        throw new Error(`${fixture.id} persistent tools retained ${controlledStructural.persistentToolsNodes} nodes; limit 160`);
      }
      if (controlledStructural.visualUnits !== fixture.visibleDensity) throw new Error(`${fixture.id} observed ${controlledStructural.visualUnits} viewport-intersecting production glyphs and emitted ${controlledStructural.emittedUnits} before the journey; expected controlled density ${fixture.visibleDensity}`);
      if (controlledStructural.emittedUnits !== fixture.visibleDensity) throw new Error(`${fixture.id} emitted ${controlledStructural.emittedUnits} production glyphs before the journey; expected visible density ${fixture.visibleDensity}`);
      const tracePath = resolve(out, `${fixture.id}--${journey}.trace.json`);
      await cdp.send("Tracing.start", { categories: "devtools.timeline,blink.user_timing,v8,disabled-by-default-devtools.timeline", transferMode: "ReturnAsStream" });
      await page.waitForTimeout(200);
      await cdp.send("Tracing.recordClockSyncMarker", { syncId: "sir-journey-start" });
      const journeyTransitions = await perform(page, journey, fixture);
      await cdp.send("Tracing.recordClockSyncMarker", { syncId: "sir-journey-end" });
      const journeyStructural = await observeStructure(svg);
      const selectionRootConstructions = journeyStructural.appViewConstructions - controlledStructural.appViewConstructions;
      const journeyTerrainConstructions = journeyStructural.terrainLayerConstructions - controlledStructural.terrainLayerConstructions;
      if (journey === "selection" && selectionRootConstructions > 1) {
        throw new Error(`${fixture.id} selection rebuilt the application root ${selectionRootConstructions} times; transitions=${journeyStructural.appViewTransitionLog}; regions=${journeyStructural.appRegionProfile}`);
      }
      if (journey === "modality-transition" && selectionRootConstructions > 3) {
        throw new Error(`${fixture.id} modality transition rebuilt the application root ${selectionRootConstructions} times; expected only Plan pending, Plan ready, and Editor accepted states; transitions=${journeyStructural.appViewTransitionLog}`);
      }
      if ((journey === "modality-transition" || journey === "playback") && journeyTerrainConstructions > 2) {
        throw new Error(`${fixture.id} ${journey} rebuilt terrain ${journeyTerrainConstructions} times; expected only the two accepted scene-owner revisions`);
      }
      if (journey === "dense-overlay"
          && (journeyStructural.leftSidebarConstructions !== controlledStructural.leftSidebarConstructions
              || journeyStructural.rightSidebarConstructions !== controlledStructural.rightSidebarConstructions)) {
        throw new Error(`${fixture.id} overlay transition rebuilt an unchanged sidebar owner`);
      }
      if (journey === "playback") {
        const simulate = journeyTransitions.find((step) => step.step === "simulate-accepted");
        const clientFeatureAcceptance = journeyTransitions.find((step) => step.appViewTransitionLog.startsWith("SimulatorWorkspace->SimulatorWorkspace:ClientFeatures"));
        if (simulate && clientFeatureAcceptance
            && (clientFeatureAcceptance.leftSidebarConstructions <= simulate.leftSidebarConstructions
                || clientFeatureAcceptance.rightSidebarConstructions !== simulate.rightSidebarConstructions)) {
          throw new Error(`${fixture.id} ClientFeatures acceptance must rebuild only the left Tools sidebar`);
        }
      }
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
      runs.push({ fixture: fixture.id, fixtureDigest: digest(fixture), workload: workloadRecipe(fixture), journey, startedAt, completedAt: new Date().toISOString(), result: "pass", stages: extractStages(journeyTrace), structural: { global: { mapExtent: fixture.mapExtent, unitCount: fixture.globalUnitCount, supportingListSize: fixture.supportingListSize }, visible: controlledStructural, journeyDelta: { appViewConstructions: journeyStructural.appViewConstructions - controlledStructural.appViewConstructions, editorProjectionConstructions: journeyStructural.editorProjectionConstructions - controlledStructural.editorProjectionConstructions, terrainLayerConstructions: journeyStructural.terrainLayerConstructions - controlledStructural.terrainLayerConstructions, leftSidebarConstructions: journeyStructural.leftSidebarConstructions - controlledStructural.leftSidebarConstructions, rightSidebarConstructions: journeyStructural.rightSidebarConstructions - controlledStructural.rightSidebarConstructions, appRegionProfileBefore: controlledStructural.appRegionProfile, appRegionProfileAfter: journeyStructural.appRegionProfile, transitions: journeyTransitions, changedLayerRevisions: Object.keys(controlledStructural.layerRevisions).filter((name) => controlledStructural.layerRevisions[name] !== journeyStructural.layerRevisions[name]) }, cameraControl: { fitCompleteMap: true, viewport: fixture.viewport, centerAnchoredWheelSteps: 15, wheelDeltaY: -240 }, postMemoryCycles: structural }, frameHealth: { ...extractFrameHealth(journeyTrace), samplingWindow: "clock-sync-bounded named journey only; captured before warm-up and stabilization memory cycles" }, inputLatency: extractInputToPaint(journeyTrace, journey), memory: { warm: heapWarm, stabilized: heapStable, usedDelta: heapStable.usedSize - heapWarm.usedSize, warmupCycles: definitions.warmupCycles, stabilizationCycles: definitions.stabilizationCycles, collectionControl: "not-forced" }, trace: { path: retainedPath, sha256: traceSha256 } });
      await context.close();
    }
  }
  let rawTraceManifest = null;
  if (retainDir) {
    rawTraceManifest = { schema: "sir.svg-pipeline-raw-trace-manifest/1", candidate, fixtureDefinitionSha256: digest(definitions), runs: retainedRuns };
    writeFileSync(resolve(retainDir, "..", "raw-trace-manifest.json"), stableJson(rawTraceManifest));
  }
  const artifact = { schema: "sir.svg-pipeline-measurement/1", result: "pass", candidate, buildIdentity, fixtureDefinition: { path: "scripts/svg-pipeline-fixtures.v1.json", sha256: digest(definitions) }, environment: { browserVersion, executableName: basename(executablePath), node: process.version, platform: platform(), release: release(), cpuCount: cpus().length }, selection: { fixtures: fixtures.map((fixture) => fixture.id), journeys }, runs, rawTraceManifest: rawTraceManifest ? { path: "work/231-svg-pipeline-measurement/raw-trace-manifest.json", sha256: digest(rawTraceManifest) } : null, summary: summarize(runs, definitions.materialShareThreshold) };
  writeFileSync(resolve(out, "summary.json"), stableJson(artifact));
  writeFileSync(resolve(out, "measurement.junit.xml"), `<?xml version="1.0" encoding="UTF-8"?>\n<testsuites tests="${runs.length}" failures="0" errors="0" skipped="0"><testsuite name="svg-pipeline-production-chromium" tests="${runs.length}" failures="0" errors="0" skipped="0">${runs.map((run) => `<testcase classname="${run.fixture}" name="${run.journey}"/>`).join("")}</testsuite></testsuites>\n`);
  console.log(`svg-pipeline: PASS runs=${runs.length} next=${artifact.summary.nextBottleneck.stage} artifact=${resolve(out, "summary.json")}`);
} finally { await browser.close(); if (server) server.kill("SIGTERM"); }
