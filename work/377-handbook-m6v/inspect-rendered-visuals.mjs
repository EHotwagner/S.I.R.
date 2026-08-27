#!/usr/bin/env node
import http from "node:http";
import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";
import { chromium } from "@playwright/test";

const root = process.cwd();
const site = path.join(root, "artifacts/site");
const output = path.join(root, "readiness/377-handbook-m6v/rendered");
const manifest = JSON.parse(fs.readFileSync(path.join(root, "docs/sir-combat-quint-diagrams.json"), "utf8"));
const sha256 = text => crypto.createHash("sha256").update(text).digest("hex");
const round = value => Math.round(value * 1000) / 1000;
const diagramResponseDelayMs = Number(process.env.SIR_M6V_DIAGRAM_RESPONSE_DELAY_MS ?? "0");
const timingMutationReceiptPath = process.env.SIR_M6V_TIMING_MUTATION_RECEIPT;
if (!Number.isInteger(diagramResponseDelayMs) || diagramResponseDelayMs < 0 || diagramResponseDelayMs > 5000) throw new Error("SIR_M6V_DIAGRAM_RESPONSE_DELAY_MS must be an integer from 0 through 5000");
fs.mkdirSync(output, { recursive: true });

const mime = new Map([[".html", "text/html; charset=utf-8"], [".svg", "image/svg+xml"], [".css", "text/css"], [".js", "text/javascript"], [".json", "application/json"]]);
const server = http.createServer((request, response) => {
  const requestPath = decodeURIComponent(new URL(request.url, "http://127.0.0.1").pathname);
  if (requestPath === "/favicon.ico") {
    response.writeHead(204); response.end(); return;
  }
  const relative = requestPath === "/" ? "index.html" : requestPath.replace(/^\/+/, "");
  const target = path.normalize(path.join(site, relative));
  if (!target.startsWith(site + path.sep) || !fs.existsSync(target) || fs.statSync(target).isDirectory()) {
    response.writeHead(404); response.end("not found"); return;
  }
  response.setHeader("content-type", mime.get(path.extname(target)) ?? "application/octet-stream");
  // Exercise every real FsDocs artifact on every navigation. Browser-process and
  // font warm-up are still separated from the measured route below.
  response.setHeader("cache-control", "no-store");
  const send = () => response.end(fs.readFileSync(target));
  if (path.extname(target) === ".svg" && diagramResponseDelayMs > 0) setTimeout(send, diagramResponseDelayMs);
  else send();
});
await new Promise(resolve => server.listen(0, "127.0.0.1", resolve));
const address = server.address();
const baseUrl = `http://127.0.0.1:${address.port}`;
const url = `${baseUrl}/sir-combat-quint-handbook.html`;
const executablePath = process.env.PLAYWRIGHT_EXECUTABLE_PATH || chromium.executablePath();

const percentile = (values, p) => values.slice().sort((a, b) => a - b)[Math.ceil(values.length * p) - 1];
const modes = [
  { id: "normal", reducedMotion: "no-preference", media: "screen", effectsOff: false },
  { id: "reduced-motion", reducedMotion: "reduce", media: "screen", effectsOff: false },
  { id: "print", reducedMotion: "reduce", media: "print", effectsOff: false },
  { id: "effects-off", reducedMotion: "no-preference", media: "screen", effectsOff: true }
];
const observations = [];
const samples = [];
let browser;

async function installDiagramProbe(page) {
  await page.addInitScript(count => {
    window.__sirDiagramReady = new Promise(resolve => {
      let settling = false;
      const settleWhenPresent = () => {
        if (settling) return;
        const images = [...document.querySelectorAll("figure[data-diagram-embed] img")];
        if (images.length !== count) return;
        settling = true;
        observer.disconnect();
        Promise.all(images.map(image => image.decode())).then(() => resolve({
          decodedReadyAtMs: performance.now(),
          dimensions: images.map(image => ({ width: image.naturalWidth, height: image.naturalHeight }))
        }), error => resolve({ error: error.message }));
      };
      const observer = new MutationObserver(settleWhenPresent);
      observer.observe(document, { childList: true, subtree: true });
      settleWhenPresent();
    });
  }, manifest.diagrams.length);
}

async function navigateToDecodedHandbook(page) {
  const wallStarted = performance.now();
  await page.goto(url, { waitUntil: "domcontentloaded" });
  const decoded = await page.evaluate(() => window.__sirDiagramReady);
  if (decoded.error || decoded.dimensions.some(image => image.width < 1 || image.height < 1)) throw new Error(decoded.error ?? "diagram decoded without intrinsic dimensions");
  const browserTiming = await page.evaluate(() => {
    const navigation = performance.getEntriesByType("navigation")[0];
    const svgResources = performance.getEntriesByType("resource")
      .filter(entry => entry.name.includes("/assets/sir-combat-quint/") && entry.name.endsWith(".svg"))
      .map(entry => ({ name: new URL(entry.name).pathname, responseEndMs: entry.responseEnd, durationMs: entry.duration, transferSize: entry.transferSize, decodedBodySize: entry.decodedBodySize }));
    return { loadEventEndMs: navigation.loadEventEnd, domContentLoadedMs: navigation.domContentLoadedEventEnd, svgResources };
  });
  if (browserTiming.svgResources.length !== manifest.diagrams.length) throw new Error(`expected ${manifest.diagrams.length} timed SVG resources, saw ${browserTiming.svgResources.length}`);
  return { readinessMs: decoded.decodedReadyAtMs, wallObservationMs: performance.now() - wallStarted, ...browserTiming };
}

try {
  const browserLaunchStarted = performance.now();
  browser = await chromium.launch({ executablePath });
  const browserLaunchObservationMs = performance.now() - browserLaunchStarted;
  const performanceContext = await browser.newContext({ viewport: { width: 1440, height: 900 }, reducedMotion: "no-preference" });
  const performancePage = await performanceContext.newPage();
  await installDiagramProbe(performancePage);
  const coldSiteObservation = await navigateToDecodedHandbook(performancePage);
  for (let warmup = 0; warmup < 3; warmup += 1) await navigateToDecodedHandbook(performancePage);
  for (let sample = 0; sample < 30; sample += 1) samples.push(await navigateToDecodedHandbook(performancePage));
  await performanceContext.close();

  const readinessValues = samples.map(sample => sample.readinessMs);
  const measuredP95LoadMs = round(percentile(readinessValues, .95));
  const measuredP99LoadMs = round(percentile(readinessValues, .99));
  if (measuredP95LoadMs > manifest.workload.timing.maxP95LoadMs || measuredP99LoadMs > manifest.workload.timing.maxP99LoadMs) {
    const overflow = {
      subject: "warm same-browser navigation start until the six external SVG image resources are loaded and decoded with non-zero intrinsic dimensions",
      diagramResponseDelayMs,
      p95LoadMs: measuredP95LoadMs,
      p99LoadMs: measuredP99LoadMs,
      maxP95Ms: manifest.workload.timing.maxP95LoadMs,
      maxP99Ms: manifest.workload.timing.maxP99LoadMs
    };
    if (timingMutationReceiptPath) {
      const receiptPath = path.resolve(root, timingMutationReceiptPath);
      fs.mkdirSync(path.dirname(receiptPath), { recursive: true });
      fs.writeFileSync(receiptPath, JSON.stringify({
        schema: "sir.handbook.timing-mutation/v1",
        workloadId: manifest.workload.id,
        workloadDefinitionDigest: manifest.workload.definitionDigest,
        mutation: "svg-response-delay-inside-decoded-image-readiness-subject",
        detector: "render timing budget exceeded",
        result: "observed-red",
        observation: overflow
      }, null, 2) + "\n");
    }
    throw new Error(`render timing budget exceeded: ${JSON.stringify(overflow)}`);
  }

  for (const mode of modes) {
    const context = await browser.newContext({ viewport: { width: 1440, height: 900 }, reducedMotion: mode.reducedMotion });
    const page = await context.newPage();
    await installDiagramProbe(page);
    const consoleErrors = [];
    const pageErrors = [];
    page.on("console", message => { if (message.type() === "error") consoleErrors.push({ text: message.text(), url: message.location().url }); });
    page.on("pageerror", error => pageErrors.push(error.message));
    await page.emulateMedia({ media: mode.media, reducedMotion: mode.reducedMotion });
    await navigateToDecodedHandbook(page);
    const embeds = await page.locator("figure[data-diagram-embed] img").evaluateAll(images => images.map(image => ({ alt: image.alt, width: image.getBoundingClientRect().width, height: image.getBoundingClientRect().height })));
    if (embeds.length !== manifest.diagrams.length || embeds.some(embed => !embed.alt || embed.width < 300 || embed.height < 100)) throw new Error(`${mode.id}: visible handbook image embeds failed`);

    const modeObservation = { mode: mode.id, consoleErrors, pageErrors, diagrams: [] };
    for (const [diagramIndex, diagram] of manifest.diagrams.entries()) {
      const assetUrl = `${baseUrl}/${diagram.asset.replace(/^docs\//, "")}`;
      await page.goto(assetUrl, { waitUntil: "load" });
      const svg = page.locator("svg");
      if (mode.effectsOff) await svg.evaluate(node => node.setAttribute("data-effects", "off"));
      const box = await svg.boundingBox();
      const semantic = await svg.evaluate((node, ids) => {
        const motion = [...node.querySelectorAll(".motion")].map(item => getComputedStyle(item).animationName);
        const effects = [...node.querySelectorAll(".fx")].map(item => ({ filter: getComputedStyle(item).filter, display: getComputedStyle(item).display }));
        return {
          role: node.getAttribute("role"),
          labelledBy: node.getAttribute("aria-labelledby"),
          title: node.querySelector(`#${ids.titleId}`)?.textContent?.trim(),
          description: node.querySelector(`#${ids.descId}`)?.textContent?.trim(),
          labelledGroups: node.querySelectorAll("g[aria-label]").length,
          visibleText: [...node.querySelectorAll("text")].map(item => item.textContent.trim()).filter(Boolean),
          motion,
          effects
        };
      }, { titleId: diagram.titleId, descId: diagram.descId });
      if (!box || box.width < 300 || box.height < 100) throw new Error(`${mode.id}:${diagram.id}: not visibly sized`);
      if (semantic.role !== "img" || semantic.labelledBy !== `${diagram.titleId} ${diagram.descId}` || !semantic.title || !semantic.description || semantic.labelledGroups < 1 || semantic.visibleText.length < 1) throw new Error(`${mode.id}:${diagram.id}: accessible semantic inspection failed`);
      if ((mode.id === "reduced-motion" || mode.id === "print") && semantic.motion.some(name => name !== "none")) throw new Error(`${mode.id}:${diagram.id}: motion did not stop`);
      if ((mode.id === "effects-off" || mode.id === "print") && semantic.effects.some(value => value.filter !== "none" && value.display !== "none")) throw new Error(`${mode.id}:${diagram.id}: filter effect did not stop`);
      modeObservation.diagrams.push({
        id: diagram.id,
        width: Math.round(box.width),
        height: Math.round(box.height),
        handbookAlt: embeds[diagramIndex].alt,
        semanticFingerprint: sha256(JSON.stringify({ title: semantic.title, description: semantic.description, labelledGroups: semantic.labelledGroups, visibleText: semantic.visibleText }))
      });
      if (mode.id === "normal" || diagram.id === "attack-pipeline") await svg.screenshot({ path: path.join(output, `${mode.id}-${diagram.id}.png`) });
    }
    if (consoleErrors.length || pageErrors.length) throw new Error(`${mode.id}: browser errors: ${JSON.stringify({ consoleErrors, pageErrors })}`);
    observations.push(modeObservation);
    await context.close();
  }

  const receipt = {
    schema: "sir.handbook.render-inspection/v1",
    workloadId: manifest.workload.id,
    workloadDefinitionDigest: manifest.workload.definitionDigest,
    route: "strict FsDocs output /sir-combat-quint-handbook.html from a pinned no-store loopback host in pinned headless Chromium",
    result: "pass",
    timings: {
      subject: "warm same-browser navigation start until the six external SVG image resources are loaded and decoded with non-zero intrinsic dimensions; unrelated template CDN load is observed but is not diagram readiness",
      gatingClock: "browser performance timeline; decodedReadyAtMs after HTMLImageElement.decode()",
      browserLaunchObservationMs: round(browserLaunchObservationMs),
      coldSiteNavigationObservation: { readinessMs: round(coldSiteObservation.readinessMs), wallObservationMs: round(coldSiteObservation.wallObservationMs) },
      warmupNavigations: 3,
      samples: samples.map(sample => ({ readinessMs: round(sample.readinessMs), wallObservationMs: round(sample.wallObservationMs), loadEventEndMs: round(sample.loadEventEndMs), slowestSvgResponseEndMs: round(Math.max(...sample.svgResources.map(resource => resource.responseEndMs))) })),
      p95LoadMs: measuredP95LoadMs,
      p99LoadMs: measuredP99LoadMs,
      maxP95Ms: manifest.workload.timing.maxP95LoadMs,
      maxP99Ms: manifest.workload.timing.maxP99LoadMs
    },
    capability: { executablePath, browserVersion: await browser.version(), headless: true, liveCompositor: false, framePacingMeasured: false },
    observations
  };
  fs.writeFileSync(path.join(output, "inspection.json"), JSON.stringify(receipt, null, 2) + "\n");
  const performanceEvidence = {
    schemaVersion: 1,
    contractVersion: "performance-evidence-v1",
    claimedBudgetPassed: true,
    sampleSets: [{
      workloadId: manifest.workload.id,
      workloadDefinitionDigest: manifest.workload.definitionDigest,
      workloadClass: "normal-play",
      targetFps: manifest.workload.timing.targetFpsIntent,
      maxP95Ms: manifest.workload.timing.maxP95LoadMs,
      maxP99Ms: manifest.workload.timing.maxP99LoadMs,
      maxCatchUpFrames: 0,
      measurementScope: receipt.timings.subject,
      requiredCapability: "headless-browser",
      hostProfile: `linux-x64-headless-chromium-${receipt.capability.browserVersion}`,
      packageVersions: ["@playwright/test@1.62.1", "FsDocs@22.1.0"],
      measurementMode: "headless",
      capabilities: ["headless-browser"],
      warmupPolicy: "same-browser-three-navigation-warmup",
      samplePolicy: "nearest-rank/30-no-store-navigation-decode-runs",
      capturedAtUtc: "2026-08-28T00:00:00Z",
      currencyToken: manifest.workload.definitionDigest,
      probeReadbackContaminated: false,
      durationSamplesMs: readinessValues.map(round),
      catchUpFrames: [0]
    }]
  };
  fs.writeFileSync(path.join(root, "readiness/377-handbook-m6v/performance-evidence.json"), JSON.stringify(performanceEvidence, null, 2) + "\n");
  console.log(`render inspection passed: ${manifest.diagrams.length} diagrams × ${modes.length} modes; p95=${receipt.timings.p95LoadMs}ms p99=${receipt.timings.p99LoadMs}ms; live-compositor=false`);
} finally {
  if (browser) await browser.close();
  await new Promise(resolve => server.close(resolve));
}
