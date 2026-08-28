#!/usr/bin/env node
import http from "node:http";
import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";
import { execFileSync } from "node:child_process";
import { chromium } from "@playwright/test";

const root = process.cwd();
const site = path.join(root, "artifacts/site");
const candidateEvidence = process.env.SIR_M6V_RECORD_CANDIDATE_EVIDENCE === "1";
const output = path.resolve(root, process.env.SIR_M6V_RENDER_OUTPUT ?? (candidateEvidence ? "readiness/377-handbook-m6v/rendered" : "readiness/377-handbook-m6v/render-replay"));
const baselinePath = path.join(root, "work/377-handbook-m6v/render-baseline.json");
const recordBaseline = process.env.SIR_M6V_RECORD_RENDER_BASELINE === "1";
const manifest = JSON.parse(fs.readFileSync(path.join(root, "docs/sir-combat-quint-diagrams.json"), "utf8"));
const preflightPath = path.resolve(root, process.env.SIR_M6V_BROWSER_PREFLIGHT_RECEIPT ?? "readiness/377-handbook-m6v/browser-preflight.json");
const preflight = JSON.parse(fs.readFileSync(preflightPath, "utf8"));
const sha256 = text => crypto.createHash("sha256").update(text).digest("hex");
const git = (...args) => execFileSync("git", args, { cwd: root, encoding: "utf8" }).trim();
const digestFiles = files => sha256(files.slice().sort().map(file => `${path.relative(root, file)}\0${sha256(fs.readFileSync(file))}`).join("\n"));
const sourceStatusAtCapture = git("status", "--porcelain", "--untracked-files=all");
if (candidateEvidence && sourceStatusAtCapture !== "") throw new Error(`candidate evidence requires a clean committed source tree; found:\n${sourceStatusAtCapture}`);
const candidateSourceInputs = [
  path.join(root, "docs/sir-combat-quint-diagrams.json"),
  path.join(root, "docs/sir-combat-quint-handbook.md"),
  path.join(root, "work/377-handbook-m6v/render-baseline.json"),
  path.join(root, "work/377-handbook-m6v/inspect-rendered-visuals.mjs"),
  path.join(root, "work/377-handbook-m6v/preflight-render-browser.mjs"),
  ...manifest.sources.map(source => path.join(root, source.path)),
  ...manifest.diagrams.map(diagram => path.join(root, diagram.asset))
];
const siteFiles = [];
const collectFiles = directory => {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    const target = path.join(directory, entry.name);
    if (entry.isDirectory()) collectFiles(target); else siteFiles.push(target);
  }
};
const round = value => Math.round(value * 1000) / 1000;
const cssRgb = hex => {
  const value = hex.replace(/^#/, "");
  return `rgb(${Number.parseInt(value.slice(0, 2), 16)}, ${Number.parseInt(value.slice(2, 4), 16)}, ${Number.parseInt(value.slice(4, 6), 16)})`;
};
const diagramResponseDelayMs = Number(process.env.SIR_M6V_DIAGRAM_RESPONSE_DELAY_MS ?? "0");
const measurementSubject = "warm same-browser navigation start until the six external SVG image resources are loaded and decoded with non-zero intrinsic dimensions; unrelated template CDN load is observed but is not diagram readiness";
const timingMutationReceiptPath = process.env.SIR_M6V_TIMING_MUTATION_RECEIPT;
const timingSampleOverride = process.env.SIR_M6V_TIMING_SAMPLE_COUNT;
const sampleCount = timingSampleOverride === undefined ? manifest.workload.timing.sampleCount : Number(timingSampleOverride);
const warmupCount = manifest.workload.timing.warmupNavigations;
class TimingBudgetExceeded extends Error {}
if (!Number.isInteger(diagramResponseDelayMs) || diagramResponseDelayMs < 0 || diagramResponseDelayMs > 5000) throw new Error("SIR_M6V_DIAGRAM_RESPONSE_DELAY_MS must be an integer from 0 through 5000");
if (!Number.isInteger(sampleCount) || sampleCount < 10 || (timingSampleOverride !== undefined && !timingMutationReceiptPath)) throw new Error("SIR_M6V_TIMING_SAMPLE_COUNT may override with >=10 only for an explicit timing mutation");
if (recordBaseline && !candidateEvidence) throw new Error("render baseline recording requires explicit candidate-evidence mode");
fs.mkdirSync(output, { recursive: true });
for (const file of fs.readdirSync(output)) {
  if (/^(?:handbook-)?(?:normal|reduced-motion|print|effects-off|css-disabled)-[a-z0-9-]+\.png$/.test(file)) fs.unlinkSync(path.join(output, file));
}

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
  { id: "normal", reducedMotion: "no-preference", media: "screen" },
  { id: "reduced-motion", reducedMotion: "reduce", media: "screen" },
  { id: "print", reducedMotion: "reduce", media: "print" },
  { id: "effects-off", reducedMotion: "no-preference", media: "screen", fragment: "effects-off" },
  { id: "css-disabled", reducedMotion: "no-preference", media: "screen", cssDisabled: true }
];
if (JSON.stringify(modes.map(mode => mode.id)) !== JSON.stringify(manifest.workload.modes)) throw new Error("render modes disagree with the producer-owned workload declaration");
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
  for (let warmup = 0; warmup < warmupCount; warmup += 1) await navigateToDecodedHandbook(performancePage);
  for (let sample = 0; sample < sampleCount; sample += 1) samples.push(await navigateToDecodedHandbook(performancePage));
  await performanceContext.close();

  const readinessValues = samples.map(sample => sample.readinessMs);
  const measuredP95LoadMs = round(percentile(readinessValues, .95));
  const measuredP99LoadMs = round(percentile(readinessValues, .99));
  if (measuredP95LoadMs > manifest.workload.timing.maxP95LoadMs || measuredP99LoadMs > manifest.workload.timing.maxP99LoadMs) {
    const overflow = {
      subject: measurementSubject,
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
    throw new TimingBudgetExceeded(`render timing budget exceeded: ${JSON.stringify(overflow)}`);
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
    const embeds = await page.locator("figure[data-diagram-embed]").evaluateAll((figures, expected) => figures.map(figure => {
      const image = figure.querySelector("img");
      const id = figure.getAttribute("data-diagram-embed");
      const diagram = expected.find(candidate => candidate.id === id);
      const transcript = diagram ? document.getElementById(diagram.transcriptAnchor) : null;
      return {
        id,
        alt: image?.alt?.trim() ?? "",
        caption: figure.querySelector("figcaption")?.textContent?.trim() ?? "",
        summary: transcript?.querySelector("summary")?.textContent?.trim() ?? "",
        transcript: transcript?.querySelector("p")?.textContent?.trim() ?? "",
        width: image?.getBoundingClientRect().width ?? 0,
        height: image?.getBoundingClientRect().height ?? 0
      };
    }), manifest.diagrams.map(({ id, transcriptAnchor }) => ({ id, transcriptAnchor })));
    const embedById = new Map(embeds.map(embed => [embed.id, embed]));
    if (embeds.length !== manifest.diagrams.length || embedById.size !== embeds.length || embeds.some(embed => !embed.id || !embed.alt || !embed.caption || !embed.summary || !embed.transcript || embed.width < 300 || embed.height < 100)) throw new Error(`${mode.id}: visible handbook image embeds/transcripts failed`);
    const handbookPixelById = new Map();
    if (["normal", "reduced-motion", "print"].includes(mode.id)) {
      for (const diagram of manifest.diagrams) {
        const screenshotPath = path.join(output, `handbook-${mode.id}-${diagram.id}.png`);
        await page.locator(`figure[data-diagram-embed="${diagram.id}"] img`).screenshot({ path: screenshotPath });
        handbookPixelById.set(diagram.id, sha256(fs.readFileSync(screenshotPath)));
      }
    }

    const modeObservation = { mode: mode.id, consoleErrors, pageErrors, diagrams: [] };
    for (const diagram of manifest.diagrams) {
      const handbookEmbed = embedById.get(diagram.id);
      if (!handbookEmbed) throw new Error(`${mode.id}:${diagram.id}: handbook embed not found by stable diagram id`);
      const assetUrl = `${baseUrl}/${diagram.asset.replace(/^docs\//, "")}${mode.fragment ? `#${mode.fragment}` : ""}`;
      await page.goto(assetUrl, { waitUntil: "load" });
      const svg = page.locator("svg");
      if (mode.cssDisabled) await svg.evaluate(node => {
        node.querySelectorAll("style").forEach(style => style.remove());
        node.querySelectorAll("[style]").forEach(element => element.removeAttribute("style"));
      });
      const liveMotion = await svg.evaluate(node => [...node.querySelectorAll(".motion")].map(item => getComputedStyle(item).animationName));
      if (mode.id === "normal" && !liveMotion.some(name => name !== "none")) throw new Error(`${mode.id}:${diagram.id}: progressive animation was not active before deterministic capture`);
      if (["reduced-motion", "print", "css-disabled"].includes(mode.id) && liveMotion.some(name => name !== "none")) throw new Error(`${mode.id}:${diagram.id}: motion fallback did not stop`);
      await svg.evaluate(() => document.getAnimations().forEach(animation => { animation.pause(); animation.currentTime = 0; }));
      const box = await svg.boundingBox();
      const semantic = await svg.evaluate((node, ids) => {
        const rounded = value => Math.round(value * 100) / 100;
        const rootBox = node.getBoundingClientRect();
        const motion = [...node.querySelectorAll(".motion")].map(item => getComputedStyle(item).animationName);
        const effects = [...node.querySelectorAll(".fx")].map(item => ({ filter: getComputedStyle(item).filter, display: getComputedStyle(item).display }));
        const overlayStyles = [...node.querySelectorAll("[data-overlay-id]")].map(owner => {
          const primitive = owner.matches("rect,line,polyline,circle") ? owner : owner.querySelector("rect,line,polyline,circle");
          const ownerStyle = getComputedStyle(owner);
          const primitiveStyle = primitive ? getComputedStyle(primitive) : null;
          return {
            id: owner.getAttribute("data-overlay-id"),
            pattern: owner.getAttribute("data-overlay-pattern"),
            color: ownerStyle.color,
            primitiveStroke: primitiveStyle?.stroke,
            primitiveVectorEffect: primitiveStyle?.vectorEffect,
            primitiveLineCap: primitiveStyle?.strokeLinecap,
            primitiveLineJoin: primitiveStyle?.strokeLinejoin
          };
        }).sort((left, right) => left.id.localeCompare(right.id));
        const rendered = [...node.querySelectorAll("text,rect,path,line,polyline,circle,polygon")].flatMap((item, index) => {
          const style = getComputedStyle(item);
          const box = item.getBoundingClientRect();
          const stroked = style.stroke !== "none" && Number.parseFloat(style.strokeWidth) > 0;
          if (style.display === "none" || style.visibility === "hidden" || ((box.width === 0 || box.height === 0) && !stroked)) return [];
          return [{
            index,
            tag: item.tagName.toLowerCase(),
            text: item.tagName.toLowerCase() === "text" ? item.textContent.trim() : undefined,
            semanticEdge: item.getAttribute("data-semantic-edge") ?? undefined,
            ruleDependency: item.getAttribute("data-rule-dependency") ?? undefined,
            box: [rounded(box.x - rootBox.x), rounded(box.y - rootBox.y), rounded(box.width), rounded(box.height)],
            style: {
              fill: style.fill,
              stroke: style.stroke,
              strokeWidth: style.strokeWidth,
              opacity: style.opacity,
              filterActive: style.filter !== "none",
              animationActive: style.animationName !== "none",
              markerEnd: style.markerEnd
            }
          }];
        });
        const clipped = rendered.filter(item => item.box[0] < -2 || item.box[1] < -2 || item.box[0] + item.box[2] > rootBox.width + 2 || item.box[1] + item.box[3] > rootBox.height + 2);
        return {
          role: node.getAttribute("role"),
          labelledBy: node.getAttribute("aria-labelledby"),
          title: node.querySelector(`#${ids.titleId}`)?.textContent?.trim(),
          description: node.querySelector(`#${ids.descId}`)?.textContent?.trim(),
          labelledGroups: node.querySelectorAll("g[aria-label]").length,
          visibleText: [...node.querySelectorAll("text")].map(item => item.textContent.trim()).filter(Boolean),
          motion,
          effects,
          directedMarkers: [...node.querySelectorAll('[data-directed-edge="true"]')].map(item => getComputedStyle(item).markerEnd),
          overlayStyles,
          rendered,
          clipped
        };
      }, { titleId: diagram.titleId, descId: diagram.descId });
      if (!box || box.width < 300 || box.height < 100) throw new Error(`${mode.id}:${diagram.id}: not visibly sized`);
      if (semantic.role !== "img" || semantic.labelledBy !== `${diagram.titleId} ${diagram.descId}` || !semantic.title || !semantic.description || semantic.labelledGroups < 1 || semantic.visibleText.length < 1) throw new Error(`${mode.id}:${diagram.id}: accessible semantic inspection failed`);
      if (semantic.clipped.length) throw new Error(`${mode.id}:${diagram.id}: rendered semantic content clipped: ${JSON.stringify(semantic.clipped)}`);
      if ((mode.id === "reduced-motion" || mode.id === "print") && semantic.motion.some(name => name !== "none")) throw new Error(`${mode.id}:${diagram.id}: motion did not stop`);
      if (["effects-off", "print", "css-disabled"].includes(mode.id) && semantic.effects.some(value => value.filter !== "none" && value.display !== "none")) throw new Error(`${mode.id}:${diagram.id}: filter effect did not stop`);
      if (mode.id === "css-disabled" && semantic.directedMarkers.some(value => value === "none")) throw new Error(`${mode.id}:${diagram.id}: directed edge lost its arrowhead`);
      if (diagram.id === "attack-pipeline") {
        const expectedOverlayIds = manifest.productionVocabulary.overlayIds;
        const expectedOverlayColor = cssRgb(manifest.productionVocabulary.palette.neutral);
        if (JSON.stringify(semantic.overlayStyles.map(item => item.id)) !== JSON.stringify(expectedOverlayIds)
          || semantic.overlayStyles.some(item => item.pattern !== "directional-hatch" || item.color !== expectedOverlayColor || item.primitiveStroke !== expectedOverlayColor || item.primitiveVectorEffect !== "non-scaling-stroke")
          || semantic.overlayStyles.filter(item => ["combat.armor-coverage", "combat.attack-traces", "cover.exposure", "unit.footprints"].includes(item.id)).some(item => item.primitiveLineCap !== "square" || item.primitiveLineJoin !== "bevel")) throw new Error(`${mode.id}:${diagram.id}: production overlay style projection drifted: ${JSON.stringify(semantic.overlayStyles)}`);
      }
      const screenshotPath = path.join(output, `${mode.id}-${diagram.id}.png`);
      await svg.screenshot({ path: screenshotPath });
      modeObservation.diagrams.push({
        id: diagram.id,
        width: Math.round(box.width),
        height: Math.round(box.height),
        handbookAlt: handbookEmbed.alt,
        handbookCaption: handbookEmbed.caption,
        transcriptSummary: handbookEmbed.summary,
        transcript: handbookEmbed.transcript,
        handbookScreenshotSha256: handbookPixelById.get(diagram.id) ?? null,
        semanticFingerprint: sha256(JSON.stringify({ title: semantic.title, description: semantic.description, labelledGroups: semantic.labelledGroups, visibleText: semantic.visibleText })),
        renderedFingerprint: sha256(JSON.stringify(semantic.rendered)),
        screenshotSha256: sha256(fs.readFileSync(screenshotPath))
      });
    }
    if (consoleErrors.length || pageErrors.length) throw new Error(`${mode.id}: browser errors: ${JSON.stringify({ consoleErrors, pageErrors })}`);
    observations.push(modeObservation);
    await context.close();
  }

  const renderedContract = {
    schema: "sir.handbook.render-baseline/v1",
    workloadId: manifest.workload.id,
    workloadDefinitionDigest: manifest.workload.definitionDigest,
    observations: observations.map(mode => ({
      mode: mode.mode,
      diagrams: mode.diagrams.map(({ id, width, height, semanticFingerprint, renderedFingerprint }) => ({ id, width, height, semanticFingerprint, renderedFingerprint }))
    }))
  };
  if (recordBaseline) {
    fs.writeFileSync(baselinePath, JSON.stringify(renderedContract, null, 2) + "\n");
  } else {
    if (!fs.existsSync(baselinePath)) throw new Error("render baseline is missing; candidate recording is explicit and outside the release replay");
    const baseline = JSON.parse(fs.readFileSync(baselinePath, "utf8"));
    if (JSON.stringify(renderedContract) !== JSON.stringify(baseline)) throw new Error(`rendered visual regression: live fingerprints differ from baseline ${path.relative(root, baselinePath)}`);
  }
  const renderBaselineSha256 = sha256(fs.readFileSync(baselinePath));
  const candidateSourceInputsSha256 = digestFiles([...new Set(candidateSourceInputs)]);

  collectFiles(site);
  const fsdocsManifest = JSON.parse(fs.readFileSync(path.join(root, ".config/dotnet-tools.json"), "utf8"));
  const fsdocsVersion = fsdocsManifest.tools?.["fsdocs-tool"]?.version;
  if (!fsdocsVersion) throw new Error("FsDocs version missing from tool manifest");
  const preflightBytes = fs.readFileSync(preflightPath);
  const sourceRevisionAtCapture = git("rev-parse", "HEAD");
  const sourceTreeAtCapture = git("rev-parse", "HEAD^{tree}");

  const receipt = {
    schema: "sir.handbook.render-inspection/v1",
    evidenceMode: candidateEvidence ? "immutable-candidate" : "live-replay",
    workloadId: manifest.workload.id,
    workloadDefinitionDigest: manifest.workload.definitionDigest,
    route: "strict FsDocs output /sir-combat-quint-handbook.html from a pinned no-store loopback host in pinned headless Chromium",
    result: "pass",
    timings: {
      subject: measurementSubject,
      gatingClock: "browser performance timeline; decodedReadyAtMs after HTMLImageElement.decode()",
      browserLaunchObservationMs: round(browserLaunchObservationMs),
      coldSiteNavigationObservation: { readinessMs: round(coldSiteObservation.readinessMs), wallObservationMs: round(coldSiteObservation.wallObservationMs) },
      warmupNavigations: warmupCount,
      sampleCount,
      samples: samples.map(sample => ({ readinessMs: round(sample.readinessMs), wallObservationMs: round(sample.wallObservationMs), loadEventEndMs: round(sample.loadEventEndMs), slowestSvgResponseEndMs: round(Math.max(...sample.svgResources.map(resource => resource.responseEndMs))) })),
      distribution: {
        minMs: round(Math.min(...readinessValues)),
        p50Ms: round(percentile(readinessValues, .50)),
        p90Ms: round(percentile(readinessValues, .90)),
        p95Ms: measuredP95LoadMs,
        p99Ms: measuredP99LoadMs,
        maxMs: round(Math.max(...readinessValues)),
        overP95BudgetCount: readinessValues.filter(value => value > manifest.workload.timing.maxP95LoadMs).length,
        overP99BudgetCount: readinessValues.filter(value => value > manifest.workload.timing.maxP99LoadMs).length
      },
      p95LoadMs: measuredP95LoadMs,
      p99LoadMs: measuredP99LoadMs,
      maxP95Ms: manifest.workload.timing.maxP95LoadMs,
      maxP99Ms: manifest.workload.timing.maxP99LoadMs
    },
    provenance: {
      sourceRevisionAtCapture,
      sourceTreeAtCapture,
      sourceTreeCleanAtCapture: sourceStatusAtCapture === "",
      candidateSourceInputsSha256,
      renderBaselineSha256,
      renderBaselineRecordedDuringCapture: recordBaseline,
      generatedSiteSha256: digestFiles(siteFiles),
      browserPreflightSha256: sha256(preflightBytes),
      browserPreflightMeasurementSubject: preflight.measurementSubject,
      packageLockSha256: preflight.packageLockSha256
    },
    capability: { executablePath, browserVersion: await browser.version(), playwrightVersion: preflight.playwrightVersion, fsdocsVersion, headless: true, liveCompositor: false, framePacingMeasured: false },
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
      evidenceMode: candidateEvidence ? "immutable-candidate" : "live-replay",
      sourceRevisionAtCapture,
      candidateSourceInputsSha256,
      workloadClass: "normal-play",
      targetFps: manifest.workload.timing.targetFpsIntent,
      maxP95Ms: manifest.workload.timing.maxP95LoadMs,
      maxP99Ms: manifest.workload.timing.maxP99LoadMs,
      maxCatchUpFrames: 0,
      measurementScope: receipt.timings.subject,
      requiredCapability: "headless-browser",
      hostProfile: `linux-x64-headless-chromium-${receipt.capability.browserVersion}`,
      packageVersions: [`@playwright/test@${preflight.playwrightVersion}`, `FsDocs@${fsdocsVersion}`],
      measurementMode: "headless",
      capabilities: ["headless-browser"],
      warmupPolicy: "same-browser-three-navigation-warmup",
      samplePolicy: `nearest-rank/${sampleCount}-no-store-navigation-decode-runs`,
      capturedAtUtc: "2026-08-28T00:00:00Z",
      currencyToken: manifest.workload.definitionDigest,
      probeReadbackContaminated: false,
      durationSamplesMs: readinessValues.map(round),
      catchUpFrames: [0]
    }]
  };
  const performanceEvidencePath = candidateEvidence
    ? path.join(root, "readiness/377-handbook-m6v/performance-evidence.json")
    : path.join(output, "performance-replay.json");
  fs.writeFileSync(performanceEvidencePath, JSON.stringify(performanceEvidence, null, 2) + "\n");
  console.log(`render inspection passed: ${manifest.diagrams.length} diagrams × ${modes.length} modes; p95=${receipt.timings.p95LoadMs}ms p99=${receipt.timings.p99LoadMs}ms; live-compositor=false`);
} catch (error) {
  if (error instanceof TimingBudgetExceeded) {
    console.error(error.message);
    process.exitCode = 42;
  } else {
    throw error;
  }
} finally {
  if (browser) await browser.close();
  await new Promise(resolve => server.close(resolve));
}
