#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";
import { execFileSync } from "node:child_process";

const root = process.cwd();
const manifestPath = "docs/sir-combat-quint-diagrams.json";
const handbookPath = "docs/sir-combat-quint-handbook.md";

const sha256 = text => crypto.createHash("sha256").update(text).digest("hex");
const read = relative => fs.readFileSync(path.join(root, relative), "utf8");
const clone = value => JSON.parse(JSON.stringify(value));
const baseManifest = JSON.parse(read(manifestPath));
const regexEscape = value => value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
const git = (...args) => execFileSync("git", args, { cwd: root, encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] }).trim();

function performanceIntent(markdown, plan = false) {
  const section = plan
    ? markdown.slice(markdown.indexOf("## Performance Intent"), markdown.indexOf("## Migration Posture"))
    : markdown.slice(0, markdown.indexOf("---", 4) + 3);
  const value = pattern => section.match(pattern)?.[1];
  const number = pattern => Number(value(pattern));
  const structuralNames = ["aggregate-bytes", "aggregate-elements", "animated-elements", "diagram-bytes", "diagram-elements"];
  const structural = Object.fromEntries(structuralNames.flatMap(name => {
    const match = section.match(new RegExp(`${name}<=([0-9]+)`));
    return match ? [[name, Number(match[1])]] : [];
  }));
  return {
    workloadId: value(/workloadIds:(?:\s*\n\s*- | \[)([^\]\n]+)/),
    digestToken: value(/workloadDefinitionDigests:(?:\s*\n\s*- | \[)([^\]\n]+)/),
    maximumExpectedScale: value(/maximumExpectedScale:\s*"?([^"\n]+)"?/),
    maxP95Ms: number(/maxP95Ms:\s*(\d+)/),
    maxP99Ms: number(/maxP99Ms:\s*(\d+)/),
    structural,
    requiredCapability: value(/requiredCapability:\s*([^\s\n]+)/),
    liveCompositorRequired: value(/liveCompositorRequired:\s*(true|false)/) === "true",
    renderModes: [...(value(/renderModes:\s*\[([^\]]+)\]/)?.matchAll(/[a-z][a-z-]+/g) ?? [])].map(match => match[0]),
    warmupNavigations: number(/warmupNavigations:\s*(\d+)/),
    sampleCount: number(/sampleCount:\s*(\d+)/)
  };
}

function directDependencies(model, ruleId) {
  const entry = model.match(new RegExp(`\\{ id: "${regexEscape(ruleId)}"[^\\n]*?dependencies: Set\\(([^)]*)\\)`));
  if (!entry) return undefined;
  return [...entry[1].matchAll(/"([^"]+)"/g)].map(match => match[1]);
}

// This is the same authoritative declaration/rule grammar owned by the M6
// handbook audit. Visual bindings are checked against parsed declarations,
// never raw prose occurrences.
function topLevelDeclarationNames(modelMarkdown) {
  const quint = [...modelMarkdown.matchAll(/```quint[^\n]*\n([\s\S]*?)\n```/g)].map(match => match[1]).join("\n");
  const names = [];
  for (const line of quint.split("\n")) {
    let match = line.match(/^module\s+(\w+)/);
    if (match) names.push(match[1]);
    match = line.match(/^  (?:(?:pure\s+)?(?:type|val|def|action|var|run|invariant))\s+(\w+)/);
    if (match) names.push(match[1]);
  }
  return names;
}

function modelRuleIds(modelMarkdown) {
  const catalogue = modelMarkdown.slice(modelMarkdown.indexOf("  pure val ruleCatalogue ="), modelMarkdown.indexOf("  pure val traceAlgorithm ="));
  return [...catalogue.matchAll(/\{\s*id:\s*"((?:CONTENT|COMBAT)-[A-Z0-9-]+-\d{3})"/g)].map(match => match[1]);
}

function runtimeRuleIds(registrySource) {
  const definitions = registrySource.slice(0, registrySource.indexOf("    let registry ="));
  return [...definitions.matchAll(/(?:metadata|transitionRule)\s*\n\s*"((?:CONTENT|COMBAT)-[A-Z0-9-]+-\d{3})"/g)].map(match => match[1]);
}

function runtimeDirectDependencies(registrySource, ruleId) {
  const entry = registrySource.match(new RegExp(`metadata\\s+"${regexEscape(ruleId)}"[\\s\\S]*?\\[([\\s\\S]*?)\\]\\s+"CombatRules\\.`));
  if (!entry) return undefined;
  return [...entry[1].matchAll(/"((?:CONTENT|COMBAT)-[A-Z0-9-]+-\d{3})"/g)].map(match => match[1]);
}

function numericBinding(source, pattern, label) {
  const match = source.match(pattern);
  if (!match) throw new Error(`authoritative binding missing: ${label}`);
  return Number(match[1]);
}

function renderedElementCount(svg) {
  const visible = svg
    .replace(/<defs\b[\s\S]*?<\/defs>/gi, "")
    .replace(/<(title|desc|style)\b[\s\S]*?<\/\1>/gi, "");
  return [...visible.matchAll(/<([a-zA-Z][\w:-]*)\b/g)]
    .map(match => match[1].toLowerCase())
    .filter(name => name !== "svg").length;
}

function animatedElementCount(svg) {
  return [...svg.matchAll(/<[^>]+\bclass="([^"]*)"[^>]*>/g)]
    .filter(match => match[1].split(/\s+/).includes("motion")).length;
}

function validate({ manifest = baseManifest, overrides = new Map(), checkVisualQualification = true } = {}) {
  const errors = [];
  const content = relative => overrides.has(relative) ? overrides.get(relative) : read(relative);
  const fail = (code, detail) => errors.push({ code, detail });
  if (manifest.schemaVersion !== 1) fail("manifest-schema-invalid", "schemaVersion must equal 1");
  if (!manifest.authorityPosture?.includes("never")) fail("authority-posture-missing", "manifest must deny independent authority");
  const diagrams = manifest.diagrams ?? [];
  if (diagrams.length !== 6 || new Set(diagrams.map(item => item.id)).size !== 6) fail("diagram-inventory-mismatch", "expected six unique diagrams");

  const workloadDeclaration = "m6v-v1|six-diagrams|strict-fsdocs|chromium|30,180,20480,122880,24|normal,reduced-motion,print,effects-off,css-disabled|warmups=3,samples=100|decoded-svg-readiness-p95=100,p99=200";
  const expectedWorkloadDigest = `sha256:${sha256(workloadDeclaration)}`;
  if (manifest.workload?.definitionDigest !== expectedWorkloadDigest) fail("workload-digest-mismatch", `expected ${expectedWorkloadDigest}`);
  const workloadToken = `${manifest.workload?.id}=${expectedWorkloadDigest}`;
  const specIntent = performanceIntent(content("work/377-handbook-m6v/spec.md"));
  const planIntent = performanceIntent(content("work/377-handbook-m6v/plan.md"), true);
  const expectedScaleText = "six diagrams; 180 SVG elements; 120 KiB; 24 animated elements";
  const expectedStructural = { "aggregate-bytes": 122880, "aggregate-elements": 180, "animated-elements": 24, "diagram-bytes": 20480, "diagram-elements": 30 };
  const intentMatches = intent => intent.workloadId === manifest.workload.id
    && intent.digestToken === workloadToken
    && intent.maximumExpectedScale.trim() === expectedScaleText
    && intent.maxP95Ms === manifest.workload.timing.maxP95LoadMs
    && intent.maxP99Ms === manifest.workload.timing.maxP99LoadMs
    && JSON.stringify(intent.structural) === JSON.stringify(expectedStructural)
    && intent.requiredCapability === "headless-browser"
    && intent.liveCompositorRequired === false;
  if (!intentMatches(specIntent) || !intentMatches(planIntent)
      || [specIntent, planIntent].some(intent => JSON.stringify(intent.renderModes) !== JSON.stringify(manifest.workload.modes)
        || intent.warmupNavigations !== manifest.workload.timing.warmupNavigations
        || intent.sampleCount !== manifest.workload.timing.sampleCount)) fail("performance-intent-cross-artifact-mismatch", JSON.stringify({ specIntent, planIntent }));
  const evidenceSource = content("work/377-handbook-m6v/evidence.yml");
  const evidencePerformance = evidenceSource.slice(evidenceSource.indexOf("id: EV035"));
  if (!evidencePerformance.includes(`workloadIds: [${manifest.workload.id}]`)
      || !evidencePerformance.includes(`workloadDefinitionDigests: ["${workloadToken}"]`)
      || !evidencePerformance.includes(`currencyToken: "${expectedWorkloadDigest}"`)
      || !evidencePerformance.includes("requiredCapability: headless-browser")
      || !evidencePerformance.includes("maxP95Ms: 100")
      || !evidencePerformance.includes("maxP99Ms: 200")) fail("performance-evidence-declaration-mismatch", "EV035");
  const smoke = JSON.parse(content("readiness/377-handbook-m6v/performance-smoke.json"));
  const performanceEvidence = JSON.parse(content("readiness/377-handbook-m6v/performance-evidence.json"));
  const renderReceipt = JSON.parse(content("readiness/377-handbook-m6v/rendered/inspection.json"));
  const visualQualification = JSON.parse(content("readiness/377-handbook-m6v/visual-qualification.json"));
  const browserPreflight = JSON.parse(content("readiness/377-handbook-m6v/browser-preflight.json"));
  const timingMutation = JSON.parse(content("readiness/377-handbook-m6v/timing-overflow-mutation.json"));
  const performanceSample = performanceEvidence.sampleSets?.[0] ?? {};
  if (smoke.workloadId !== manifest.workload.id || smoke.workloadDefinitionDigest !== expectedWorkloadDigest
      || JSON.stringify(smoke.declaredMaximumScale) !== JSON.stringify(manifest.workload.maximumScale)
      || JSON.stringify(smoke.releaseProbePolicy?.modes) !== JSON.stringify(manifest.workload.modes)
      || smoke.releaseProbePolicy?.warmupNavigations !== manifest.workload.timing.warmupNavigations
      || smoke.releaseProbePolicy?.sampleCount !== manifest.workload.timing.sampleCount
      || smoke.releaseProbePolicy?.maxP95LoadMs !== manifest.workload.timing.maxP95LoadMs
      || smoke.releaseProbePolicy?.maxP99LoadMs !== manifest.workload.timing.maxP99LoadMs) fail("performance-smoke-contract-mismatch", smoke.workloadDefinitionDigest ?? "missing");
  const renderSamples = renderReceipt.timings?.samples?.map(sample => sample.readinessMs) ?? [];
  if (performanceEvidence.schemaVersion !== 1 || performanceEvidence.contractVersion !== "performance-evidence-v1" || performanceEvidence.claimedBudgetPassed !== true
      || performanceEvidence.sampleSets?.length !== 1
      || performanceSample.workloadId !== manifest.workload.id || performanceSample.workloadDefinitionDigest !== expectedWorkloadDigest
      || performanceSample.evidenceMode !== "immutable-candidate"
      || performanceSample.sourceRevisionAtCapture !== renderReceipt.provenance?.sourceRevisionAtCapture
      || performanceSample.candidateSourceInputsSha256 !== renderReceipt.provenance?.candidateSourceInputsSha256
      || performanceSample.maxP95Ms !== manifest.workload.timing.maxP95LoadMs
      || performanceSample.maxP99Ms !== manifest.workload.timing.maxP99LoadMs
      || performanceSample.targetFps !== manifest.workload.timing.targetFpsIntent
      || performanceSample.currencyToken !== expectedWorkloadDigest
      || performanceSample.maxCatchUpFrames !== 0
      || JSON.stringify(performanceSample.catchUpFrames) !== JSON.stringify([0])
      || performanceSample.probeReadbackContaminated !== false
      || performanceSample.durationSamplesMs?.length !== manifest.workload.timing.sampleCount
      || JSON.stringify(performanceSample.durationSamplesMs) !== JSON.stringify(renderSamples)
      || performanceSample.measurementScope !== renderReceipt.timings?.subject
      || performanceSample.requiredCapability !== "headless-browser"
      || performanceSample.measurementMode !== "headless"
      || JSON.stringify(performanceSample.capabilities) !== JSON.stringify(["headless-browser"])
      || performanceSample.samplePolicy !== `nearest-rank/${manifest.workload.timing.sampleCount}-no-store-navigation-decode-runs`
      || performanceSample.warmupPolicy !== "same-browser-three-navigation-warmup") fail("performance-evidence-contract-mismatch", performanceSample.workloadDefinitionDigest ?? "missing");
  const expectedDiagramIds = diagrams.map(diagram => diagram.id);
  const observationModes = renderReceipt.observations?.map(item => item.mode) ?? [];
  const normalFingerprints = new Map((renderReceipt.observations?.[0]?.diagrams ?? []).map(diagram => [diagram.id, diagram.semanticFingerprint]));
  const renderedObservationMatches = renderReceipt.observations?.length === manifest.workload.modes.length
    && renderReceipt.observations.every(observation => JSON.stringify(observation.diagrams?.map(diagram => diagram.id)) === JSON.stringify(expectedDiagramIds)
      && observation.diagrams.length === 6
      && observation.consoleErrors?.length === 0
      && observation.pageErrors?.length === 0
      && observation.diagrams.every(diagram => diagram.semanticFingerprint === normalFingerprints.get(diagram.id)
        && (["normal", "reduced-motion", "print"].includes(observation.mode) ? Boolean(diagram.handbookScreenshotSha256) : diagram.handbookScreenshotSha256 === null)));
  const percentile = (values, fraction) => values.slice().sort((a, b) => a - b)[Math.ceil(values.length * fraction) - 1];
  const expectedDistribution = {
    minMs: Math.min(...renderSamples),
    p50Ms: percentile(renderSamples, .50),
    p90Ms: percentile(renderSamples, .90),
    p95Ms: percentile(renderSamples, .95),
    p99Ms: percentile(renderSamples, .99),
    maxMs: Math.max(...renderSamples),
    overP95BudgetCount: renderSamples.filter(value => value > manifest.workload.timing.maxP95LoadMs).length,
    overP99BudgetCount: renderSamples.filter(value => value > manifest.workload.timing.maxP99LoadMs).length
  };
  if (renderReceipt.result !== "pass" || renderReceipt.evidenceMode !== "immutable-candidate"
      || renderReceipt.workloadId !== manifest.workload.id || renderReceipt.workloadDefinitionDigest !== expectedWorkloadDigest
      || renderReceipt.timings?.warmupNavigations !== manifest.workload.timing.warmupNavigations
      || renderReceipt.timings?.sampleCount !== manifest.workload.timing.sampleCount
      || renderSamples.length !== manifest.workload.timing.sampleCount
      || renderReceipt.timings?.maxP95Ms !== manifest.workload.timing.maxP95LoadMs
      || renderReceipt.timings?.maxP99Ms !== manifest.workload.timing.maxP99LoadMs
      || JSON.stringify(renderReceipt.timings?.distribution) !== JSON.stringify(expectedDistribution)
      || renderReceipt.timings?.p95LoadMs !== expectedDistribution.p95Ms
      || renderReceipt.timings?.p99LoadMs !== expectedDistribution.p99Ms
      || renderReceipt.timings?.p95LoadMs > manifest.workload.timing.maxP95LoadMs
      || renderReceipt.timings?.p99LoadMs > manifest.workload.timing.maxP99LoadMs
      || JSON.stringify(observationModes) !== JSON.stringify(manifest.workload.modes)
      || new Set(observationModes).size !== manifest.workload.modes.length
      || !renderedObservationMatches
      || renderReceipt.capability?.headless !== true || renderReceipt.capability?.liveCompositor !== false || renderReceipt.capability?.framePacingMeasured !== false) fail("render-receipt-contract-mismatch", renderReceipt.workloadDefinitionDigest ?? "missing");
  const expectedPngNames = manifest.workload.modes.flatMap(mode => expectedDiagramIds.map(id => `${mode}-${id}.png`))
    .concat(["normal", "reduced-motion", "print"].flatMap(mode => expectedDiagramIds.map(id => `handbook-${mode}-${id}.png`))).sort();
  const renderedDirectory = path.join(root, "readiness/377-handbook-m6v/rendered");
  const actualPngNames = fs.readdirSync(renderedDirectory).filter(file => file.endsWith(".png")).sort();
  const screenshotHashesMatch = renderReceipt.observations?.every(observation => observation.diagrams.every(diagram => {
    const standalone = fs.readFileSync(path.join(renderedDirectory, `${observation.mode}-${diagram.id}.png`));
    const handbookName = `handbook-${observation.mode}-${diagram.id}.png`;
    const handbookExpected = ["normal", "reduced-motion", "print"].includes(observation.mode);
    return sha256(standalone) === diagram.screenshotSha256
      && (handbookExpected ? sha256(fs.readFileSync(path.join(renderedDirectory, handbookName))) === diagram.handbookScreenshotSha256 : diagram.handbookScreenshotSha256 === null);
  }));
  if (JSON.stringify(actualPngNames) !== JSON.stringify(expectedPngNames) || !screenshotHashesMatch) fail("rendered-screenshot-inventory-mismatch", JSON.stringify({ expected: expectedPngNames.length, actual: actualPngNames.length }));
  if (timingMutation.schema !== "sir.handbook.timing-mutation/v1" || timingMutation.evidenceMode !== "immutable-candidate" || timingMutation.workloadId !== manifest.workload.id || timingMutation.workloadDefinitionDigest !== expectedWorkloadDigest
      || timingMutation.mutation !== "svg-response-delay-inside-decoded-image-readiness-subject" || timingMutation.detector !== "render timing budget exceeded" || timingMutation.result !== "observed-red"
      || timingMutation.observation?.subject !== renderReceipt.timings?.subject || timingMutation.observation?.diagramResponseDelayMs !== 350
      || timingMutation.observation?.maxP95Ms !== manifest.workload.timing.maxP95LoadMs || timingMutation.observation?.maxP99Ms !== manifest.workload.timing.maxP99LoadMs
      || timingMutation.observation?.p95LoadMs <= timingMutation.observation?.maxP95Ms || timingMutation.observation?.p99LoadMs <= timingMutation.observation?.maxP99Ms) fail("timing-mutation-contract-mismatch", JSON.stringify(timingMutation));
  if (browserPreflight.result !== "pass" || browserPreflight.measurementSubject !== false || browserPreflight.executableSource !== "playwright-managed"
      || browserPreflight.packageLockSha256 !== renderReceipt.provenance?.packageLockSha256
      || browserPreflight.playwrightVersion !== renderReceipt.capability?.playwrightVersion
      || browserPreflight.browserVersion !== renderReceipt.capability?.browserVersion
      || sha256(content("readiness/377-handbook-m6v/browser-preflight.json")) !== renderReceipt.provenance?.browserPreflightSha256) fail("browser-preflight-contract-mismatch", browserPreflight.executableSource ?? "missing");

  for (const source of manifest.sources ?? []) {
    const actual = sha256(content(source.path));
    if (actual !== source.sha256) fail("source-digest-mismatch", `${source.path}: ${actual} != ${source.sha256}`);
  }

  const runtime = content("src/SIR.Simulation/CombatRules.fs");
  const model = content("docs/rules/sir-combat.md");
  const vocabulary = JSON.parse(content("docs/sir-combat-quint-vocabulary.json"));
  const vocabularyTerms = new Set(vocabulary.terms.map(item => item.term));
  const handbook = content(handbookPath);
  const glyphSource = content("src/SIR.Client/UnitGlyphCatalog.fs");
  const appSource = content("src/SIR.Client.Web/App.fs");
  const symbolSource = content("src/SIR.Client.Web/TacticalUnitSymbolView.fs");
  const overlaySource = content("src/SIR.Client.Web/TacticalOverlayView.fs");
  const stylesSource = content("src/SIR.Client.Web/styles.css");
  const visualSource = content("src/SIR.Client/TacticalSceneProjection.fs");
  const mapEditorSource = content("src/SIR.Client/MapEditor.fs");
  const representativeMutationControl = content("work/361-handbook-m2/audit-representative-attack.mjs");
  const parsedModelRules = modelRuleIds(model);
  const parsedRuntimeRules = runtimeRuleIds(runtime);
  const parsedDeclarations = new Set(topLevelDeclarationNames(model));
  const canonicalRules = vocabulary.mandatoryTraceability?.ruleIds ?? [];
  if (new Set(parsedModelRules).size !== 16 || new Set(parsedRuntimeRules).size !== 16 || JSON.stringify(parsedModelRules.slice().sort()) !== JSON.stringify(canonicalRules.slice().sort()) || JSON.stringify(parsedRuntimeRules.slice().sort()) !== JSON.stringify(canonicalRules.slice().sort())) fail("authoritative-rule-inventory-mismatch", "M6 vocabulary, Quint catalogue, and runtime registry must expose the same sixteen rules");
  const rifleman = glyphSource.match(/glyph "rifleman"[^\n]*\[\| FilledPath "([^"]+)"; StrokedPath "([^"]+)" \|\]/);
  const derivedGlyphPrimitives = rifleman ? [rifleman[1], rifleman[2]] : [];
  if (JSON.stringify(manifest.productionVocabulary?.glyphPrimitives) !== JSON.stringify(derivedGlyphPrimitives)) fail("glyph-source-binding-missing", JSON.stringify(derivedGlyphPrimitives));
  const paletteBlock = glyphSource.slice(glyphSource.indexOf("    let accessibleDefault ="), glyphSource.indexOf("    let highContrast ="));
  const paletteFields = { canvas: "Canvas", terrain: "Terrain", grid: "Grid", text: "Text", human: "HumanFaction", arcane: "ArcaneFaction", neutral: "NeutralFaction", health: "HealthActive", healthDepleted: "HealthDepleted" };
  const derivedPalette = Object.fromEntries(Object.entries(paletteFields).map(([name, field]) => {
    const match = paletteBlock.match(new RegExp(`${field} = "([^"]+)"`));
    return [name, match?.[1]];
  }));
  const expectedOverlayIds = ["combat.armor-coverage", "combat.attack-traces", "combat.hp-wounds", "combat.suppression", "cover.exposure", "unit.footprints"];
  const productionOverlayIds = [...visualSource.matchAll(/descriptor "([^"]+)"/g)].map(match => match[1]).filter(id => expectedOverlayIds.includes(id)).sort();
  if (JSON.stringify(productionOverlayIds) !== JSON.stringify(expectedOverlayIds) || JSON.stringify(manifest.productionVocabulary?.overlayIds) !== JSON.stringify(productionOverlayIds)) fail("production-overlay-owner-missing", JSON.stringify(productionOverlayIds));
  if (JSON.stringify(manifest.productionVocabulary?.palette) !== JSON.stringify(derivedPalette)) fail("palette-source-binding-missing", JSON.stringify(derivedPalette));
  const persistentOverlayColor = stylesSource.match(/#sir-replay-app #persistent-tactical-overlay-layer\s*\{\s*color:\s*(#[0-9a-f]+);/i)?.[1];
  const persistentOverlayStyleMatches = persistentOverlayColor === derivedPalette.neutral
    && /#sir-replay-app #persistent-tactical-overlay-layer \*\s*\{\s*vector-effect:\s*non-scaling-stroke;/s.test(stylesSource)
    && /\[data-overlay-pattern="directional-hatch"\]\s*\{\s*stroke-linecap:\s*square;\s*stroke-linejoin:\s*bevel;/s.test(stylesSource);
  if (!persistentOverlayStyleMatches) fail("production-overlay-style-owner-missing", JSON.stringify({ persistentOverlayColor }));
  const render = manifest.productionRenderContract ?? {};
  const documentationScale = render.documentationScale;
  const producerCellSizePx = numericBinding(appSource, /let private persistentSceneSvg[\s\S]*?let cellSize = ([\d.]+)/, "persistent renderer cell size");
  const derivedRender = {
    footprintCells: numericBinding(mapEditorSource, /ClassId = "rifleman"[\s\S]*?FootprintSize = (\d+)/, "rifleman footprint"),
    producerCellSizePx,
    documentationScale,
    bodyInsetPx: (appSource.includes("presentationX + 5.0") && appSource.includes("width - 10.0") ? 5 : NaN) * documentationScale,
    bodyRadiusPx: numericBinding(visualSource, /UnitCornerRadius = ([\d.]+)/, "unit corner radius") * documentationScale,
    bodyStrokePx: numericBinding(visualSource, /UnitStrokeWidth = ([\d.]+)/, "unit stroke width") * documentationScale,
    glyphStrokePx: numericBinding(appSource, /svg\.strokeWidth ([\d.]+)\s*\n\s*svg\.strokeLineCap "round"/, "glyph stroke"),
    compactHealthHeightPx: numericBinding(symbolSource, /data-unit-health-density[\s\S]*?svg\.height (\d+)/, "compact health height") * documentationScale,
    footprintStrokePx: numericBinding(overlaySource, /FootprintGeometry[\s\S]*?svg\.strokeWidth (\d+)/, "footprint stroke") * documentationScale,
    directionStrokePx: numericBinding(overlaySource, /DirectionGeometry[\s\S]*?svg\.strokeWidth (\d+)/, "direction stroke") * documentationScale,
    traceStrokePx: numericBinding(overlaySource, /TraceGeometry[\s\S]*?svg\.strokeWidth (\d+)/, "trace stroke") * documentationScale,
    impactRadiusPx: numericBinding(overlaySource, /TraceGeometry[\s\S]*?svg\.r (\d+); svg\.fill "currentColor"/, "impact radius") * documentationScale,
    statusRadiusPx: numericBinding(overlaySource, /StatusGeometry[\s\S]*?svg\.r (\d+); svg\.fill "none"/, "status radius") * documentationScale,
    statusStrokePx: numericBinding(overlaySource, /StatusGeometry[\s\S]*?svg\.strokeWidth (\d+)/, "status stroke") * documentationScale,
    compactHealthInsetPx: numericBinding(symbolSource, /DenseDensity[\s\S]*?let inset = ([\d.]+)/, "compact health inset") * documentationScale,
    directionLengthCells: numericBinding(overlaySource, /DirectionGeometry[\s\S]*?let presentationLength = ([\d.]+)/, "direction presentation length")
  };
  derivedRender.producerFootprintExtentPx = derivedRender.footprintCells * producerCellSizePx;
  derivedRender.footprintExtentPx = derivedRender.producerFootprintExtentPx * documentationScale;
  derivedRender.bodyExtentPx = derivedRender.footprintExtentPx - 2 * derivedRender.bodyInsetPx;
  derivedRender.glyphScale = ((derivedRender.producerFootprintExtentPx - 16) / 24) * documentationScale;
  derivedRender.compactHealthAvailablePx = (derivedRender.producerFootprintExtentPx - 2 * (derivedRender.compactHealthInsetPx / documentationScale)) * documentationScale;
  derivedRender.directionLengthPx = derivedRender.directionLengthCells * producerCellSizePx * documentationScale;
  if (JSON.stringify(render) !== JSON.stringify(derivedRender)) fail("production-render-contract-mismatch", JSON.stringify({ expected: derivedRender, projected: render }));

  const representativeStart = model.indexOf("  pure val representativeAttack:");
  const representativeEnd = model.indexOf("  pure val missedAttack:");
  if (representativeStart < 0 || representativeEnd <= representativeStart) fail("representative-attack-boundary-missing", `${representativeStart}/${representativeEnd}`);
  const representativeBlock = model.slice(representativeStart, representativeEnd);
  const scale = numericBinding(model, /pure val SCALE = (\d+)/, "Q4 scale");
  const rifleDamageRaw = numericBinding(model, /pure val rifleDamageRaw = (\d+)/, "rifle damage raw");
  const representativeArmorRaw = numericBinding(representativeBlock, /armorRetentionRaw:\s*(\d+)/, "representative armor retention");
  const visibleSamples = numericBinding(representativeBlock, /visibleSamples:\s*(\d+)/, "representative visible samples");
  const totalSamples = numericBinding(representativeBlock, /totalSamples:\s*(\d+)/, "representative total samples");
  const representativeSuppression = numericBinding(representativeBlock, /suppressionDelta:\s*(\d+)/, "representative suppression");
  const initialStart = model.indexOf("  pure val initialCombat:");
  const initialEnd = model.indexOf("\n\n  var combat:", initialStart);
  const initialBlock = initialStart >= 0 && initialEnd > initialStart ? model.slice(initialStart, initialEnd) : "";
  const initialHealth = numericBinding(initialBlock, /health:\s*(\d+)/, "initial health");
  const initialSuppression = numericBinding(initialBlock, /suppression:\s*(\d+)/, "initial suppression");
  const initialCoverIntegrity = numericBinding(initialBlock, /coverIntegrity:\s*(\d+)/, "initial cover integrity");
  const initialCoverBlocking = /coverBlocking:\s*true/.test(initialBlock);
  const initialIncapacitated = /incapacitated:\s*true/.test(initialBlock);
  const multiplyPositiveQ4 = (left, right) => Math.floor((left * right + Math.floor(scale / 2)) / scale);
  const traceRaw = Math.floor(visibleSamples * scale / totalSamples);
  const representativeDamageRaw = multiplyPositiveQ4(multiplyPositiveQ4(rifleDamageRaw, traceRaw), representativeArmorRaw);
  const representativeDamage = Math.floor((representativeDamageRaw + Math.floor(scale / 2)) / scale);
  const successorHealth = Math.max(0, initialHealth - representativeDamage);
  const successorSuppression = Math.min(100, Math.max(0, initialSuppression + representativeSuppression));
  const minorThreshold = numericBinding(model, /if \(damage >= \d+\) MajorWound else if \(damage >= (\d+)\) MinorWound/, "minor wound threshold");
  const expectedWound = representativeDamage >= minorThreshold ? "MinorWound" : "NoWound";
  const mutatedRetentionMatch = representativeMutationControl.match(/model\.replace\(needle, "\s*armorRetentionRaw:\s*(\d+),"\)/);
  const mutatedRetentionRaw = Number(mutatedRetentionMatch?.[1]);
  const mutatedDamageRaw = multiplyPositiveQ4(multiplyPositiveQ4(rifleDamageRaw, traceRaw), mutatedRetentionRaw);
  const mutatedDamage = Math.floor((mutatedDamageRaw + Math.floor(scale / 2)) / scale);
  const mutatedHealth = Math.max(0, initialHealth - mutatedDamage);

  const budgets = manifest.workload?.maximumScale ?? {};
  let aggregateElements = 0;
  let aggregateBytes = 0;
  let aggregateAnimated = 0;
  const metrics = [];

  for (const diagram of diagrams) {
    const svg = content(diagram.asset);
    const bytes = Buffer.byteLength(svg);
    const elements = renderedElementCount(svg);
    const animated = animatedElementCount(svg);
    aggregateElements += elements;
    aggregateBytes += bytes;
    aggregateAnimated += animated;
    metrics.push({ id: diagram.id, kind: diagram.kind, elements, bytes, animatedElements: animated, sha256: sha256(svg) });

    if (!svg.startsWith("<svg") || /<(canvas|script)\b/i.test(svg)) fail("pure-svg-required", diagram.id);
    if (diagram.kind.startsWith("abstract-") && (/<(?:foreignObject|image)\b/i.test(svg) || /(?:href|xlink:href)="(?!#)[^"]+"/i.test(svg))) fail("abstract-svg-external-resource", diagram.id);
    if (!new RegExp(`<svg[^>]+role="img"[^>]+aria-labelledby="${diagram.titleId} ${diagram.descId}"`, "i").test(svg)) fail("accessibility-semantics-missing", diagram.id);
    if (!new RegExp(`<title id="${diagram.titleId}">[^<]+</title>`, "i").test(svg) || !new RegExp(`<desc id="${diagram.descId}">[^<]+</desc>`, "i").test(svg)) fail("accessibility-title-description-missing", diagram.id);
    for (const group of svg.matchAll(/<g\b([^>]*)>/gi)) if (!/\baria-label="[^"]+"/.test(group[1])) fail("accessible-group-label-missing", diagram.id);
    if (!svg.includes("prefers-reduced-motion:reduce") || !svg.includes("@media print") || !svg.includes("[data-effects=\"off\"] .fx") || !svg.includes("svg:target .fx") || !svg.includes('id="effects-off"') || !svg.includes(".motion") || !svg.includes(".fx")) fail("fallback-contract-missing", diagram.id);
    const semanticEdges = [...svg.matchAll(/<(?:path|line|polyline)\b[^\n]*?\s*\/>/g)]
      .map(match => match[0])
      .filter(tag => /\bdata-semantic-edge="[^"]+"/.test(tag));
    const directedEdges = semanticEdges.filter(tag => /\bdata-directed-edge="true"/.test(tag));
    const viewBox = svg.match(/viewBox="0 0 ([\d.]+) ([\d.]+)"/);
    const backdrop = svg.match(/<rect class="backdrop" aria-hidden="true" x="0" y="0" width="([\d.]+)" height="([\d.]+)" fill="([^"]+)"\/>/);
    if (!viewBox || !backdrop || backdrop[1] !== viewBox[1] || backdrop[2] !== viewBox[2] || backdrop[3] !== derivedPalette.canvas || !/font-family="system-ui" font-size="[\d.]+"/.test(svg.slice(0, svg.indexOf(">") + 1)) || semanticEdges.length === 0 || semanticEdges.some(edge => !/\bstroke="(?!none)[^"]+"/.test(edge)) || directedEdges.some(edge => !/\bmarker-end="url\(#[^)]+\)"/.test(edge)) || [...svg.matchAll(/<text\b([^>]*)>/g)].some(match => !/\bfill="[^"]+"/.test(match[1]))) fail("static-fallback-missing", diagram.id);
    if (/webgl|webgpu/i.test(svg)) fail("non-webgl-contract-broken", diagram.id);
    if (elements > budgets.elementsPerDiagram || aggregateElements > budgets.aggregateElements) fail("element-budget-exceeded", `${diagram.id}:${elements}`);
    if (bytes > budgets.bytesPerDiagram || aggregateBytes > budgets.aggregateBytes) fail("byte-budget-exceeded", `${diagram.id}:${bytes}`);
    if (sha256(svg) !== diagram.sha256) fail("asset-fingerprint-mismatch", diagram.id);
    if (diagram.kind === "concrete-mechanics") {
      for (const primitive of manifest.productionVocabulary.glyphPrimitives) if (!svg.includes(primitive)) fail("glyph-vocabulary-mismatch", `${diagram.id}:${primitive}`);
      for (const name of diagram.usedPaletteTokens ?? []) if (!svg.toLowerCase().includes(derivedPalette[name]?.toLowerCase())) fail("palette-vocabulary-mismatch", `${diagram.id}:${name}`);
      const outer = svg.match(/<rect data-overlay-id="unit\.footprints" data-overlay-kind="footprint" data-overlay-pattern="directional-hatch" x="([\d.]+)" y="([\d.]+)" width="([\d.]+)" height="([\d.]+)" fill="([^"]+)" color="([^"]+)" stroke="([^"]+)" stroke-width="([\d.]+)"/);
      const body = svg.match(/<rect x="([\d.]+)" y="([\d.]+)" width="([\d.]+)" height="([\d.]+)" rx="([\d.]+)" fill="([^"]+)" stroke="([^"]+)" stroke-width="([\d.]+)"/);
      const glyphTransform = svg.match(/aria-label="Exact production rifleman glyph primitives"[^>]*transform="translate\(([\d.]+) ([\d.]+)\) scale\(([\d.]+)\) translate\(-12 -12\)"/);
      const filledGlyph = svg.match(/<path d="([^"]+)" fill="([^"]+)"\/><path d="([^"]+)" fill="none" stroke="([^"]+)" stroke-width="([\d.]+)" stroke-linecap="([^"]+)" stroke-linejoin="([^"]+)"/);
      const outerX = Number(outer?.[1]); const outerY = Number(outer?.[2]);
      const renderMatches = outer && body && glyphTransform && filledGlyph
        && svg.includes(`data-unit-glyph="${manifest.productionVocabulary.glyphId}"`) && svg.includes(`data-class-id="${manifest.productionVocabulary.glyphId}"`)
        && Number(outer[3]) === render.footprintExtentPx && Number(outer[4]) === render.footprintExtentPx && outer[5] === "none" && outer[6] === derivedPalette.neutral && outer[7] === "currentColor" && Number(outer[8]) === render.footprintStrokePx
        && Number(body[1]) === outerX + render.bodyInsetPx && Number(body[2]) === outerY + render.bodyInsetPx && Number(body[3]) === render.bodyExtentPx && Number(body[4]) === render.bodyExtentPx && Number(body[5]) === render.bodyRadiusPx && body[6] === derivedPalette.canvas && body[7] === derivedPalette.human && Number(body[8]) === render.bodyStrokePx
        && Number(glyphTransform[1]) === outerX + render.footprintExtentPx / 2 && Number(glyphTransform[2]) === outerY + render.footprintExtentPx / 2 && Number(glyphTransform[3]) === render.glyphScale
        && filledGlyph[1] === derivedGlyphPrimitives[0] && filledGlyph[2] === derivedPalette.text && filledGlyph[3] === derivedGlyphPrimitives[1] && filledGlyph[4] === derivedPalette.text && Number(filledGlyph[5]) === render.glyphStrokePx && filledGlyph[6] === "round" && filledGlyph[7] === "round";
      if (!renderMatches) fail("production-render-projection-mismatch", diagram.id);
      const trace = svg.match(/<polyline[^>]*points="[^"]+" fill="none" stroke="currentColor" stroke-width="([\d.]+)"[^>]*\/><circle data-impact="authoritative"[^>]*r="([\d.]+)" fill="currentColor"/);
      const health = svg.match(/data-unit-health-density="compact"[^>]*width="([\d.]+)" height="([\d.]+)"[^>]*fill="([^"]+)"\/><rect data-unit-health-fill="([^"]+)"[^>]*width="([\d.]+)" height="([\d.]+)"[^>]*fill="([^"]+)"/);
      const statuses = [...svg.matchAll(/<circle data-overlay-id="([^"]+)" data-overlay-pattern="directional-hatch" data-status-current="([^"]*)" data-status-maximum="([^"]*)" data-status-tokens="([^"]*)"[^>]*r="([\d.]+)" fill="none" color="([^"]+)" stroke="([^"]+)" stroke-width="([\d.]+)"[^>]*vector-effect="([^"]+)"/g)];
      const directions = [...svg.matchAll(/<line data-direction-arc="([^"]*)" x1="([\d.]+)" y1="([\d.]+)" x2="([\d.]+)" y2="([\d.]+)"[^>]*stroke-width="([\d.]+)"/g)];
      const directionMatches = directions.length === 2
        && directions.every(line => line[1] === "" && Math.hypot(Number(line[4]) - Number(line[2]), Number(line[5]) - Number(line[3])) === render.directionLengthPx && Number(line[6]) === render.directionStrokePx)
        && productionOverlayIds.every(id => svg.includes(`data-overlay-id="${id}"`));
      const statusMatches = statuses.length === 2 && statuses.every(status => Number(status[5]) === render.statusRadiusPx && status[6] === derivedPalette.neutral && status[7] === "currentColor" && Number(status[8]) === render.statusStrokePx && status[9] === "non-scaling-stroke")
        && statuses[0][1] === "combat.hp-wounds" && statuses[0][2] === String(successorHealth) && statuses[0][3] === String(initialHealth) && statuses[0][4] === ""
        && statuses[1][1] === "combat.suppression" && statuses[1][2] === "" && statuses[1][3] === "" && statuses[1][4] === `suppression:${representativeSuppression}`;
      const overlayStyleProjectionMatches = svg.includes(`data-overlay-id="combat.attack-traces" data-overlay-pattern="directional-hatch" color="${derivedPalette.neutral}"`)
        && [...svg.matchAll(/<(?:rect|line|polyline|circle)\b[^>]*(?:data-overlay-id|data-direction-arc|data-semantic-edge="trace-to-first-contact")[^>]*>/g)].every(match => /vector-effect="non-scaling-stroke"/.test(match[0]));
      const expectedHealthFill = render.compactHealthAvailablePx * successorHealth / initialHealth;
      if (!trace || Number(trace[1]) !== render.traceStrokePx || Number(trace[2]) !== render.impactRadiusPx || !health || Number(health[1]) !== render.compactHealthAvailablePx || Number(health[2]) !== render.compactHealthHeightPx || health[3] !== derivedPalette.healthDepleted || health[4] !== String(successorHealth) || Number(health[5]) !== expectedHealthFill || Number(health[6]) !== render.compactHealthHeightPx || health[7] !== derivedPalette.health || !statusMatches || !directionMatches || !overlayStyleProjectionMatches) fail("production-overlay-projection-mismatch", diagram.id);
    } else if (!diagram.kind.startsWith("abstract-")) fail("diagram-kind-invalid", diagram.id);
    for (const rule of diagram.rules) if (!parsedRuntimeRules.includes(rule) || !parsedModelRules.includes(rule)) fail("rule-binding-missing", `${diagram.id}:${rule}`);
    if (diagram.kind === "abstract-dependency") {
      const owner = diagram.directDependencyOwner;
      const authoritative = owner ? directDependencies(model, owner) : undefined;
      const runtimeAuthoritative = owner ? runtimeDirectDependencies(runtime, owner) : undefined;
      if (!owner || !authoritative || !runtimeAuthoritative) {
        fail("dependency-authority-missing", `${diagram.id}:${owner ?? "owner-not-declared"}`);
      } else {
        const projected = diagram.rules.filter(rule => rule !== owner);
        const expectedEdges = authoritative.map(rule => `${rule}->${owner}`).sort();
        const projectedEdges = [...svg.matchAll(/data-rule-dependency="([^"]+)"/g)].map(match => match[1]).sort();
        if (!diagram.rules.includes(owner) || JSON.stringify(projected.slice().sort()) !== JSON.stringify(authoritative.slice().sort())) {
          fail("dependency-rule-set-mismatch", `${diagram.id}:${JSON.stringify({ authoritative, projected })}`);
        }
        if (JSON.stringify(runtimeAuthoritative.slice().sort()) !== JSON.stringify(authoritative.slice().sort())) fail("dependency-authority-disagreement", `${diagram.id}:${JSON.stringify({ quint: authoritative, runtime: runtimeAuthoritative })}`);
        if (JSON.stringify(projectedEdges) !== JSON.stringify(expectedEdges)) {
          fail("dependency-edge-mismatch", `${diagram.id}:${JSON.stringify({ expectedEdges, projectedEdges })}`);
        }
      }
    }
    for (const declaration of diagram.declarations) {
      if (!parsedDeclarations.has(declaration)) fail("declaration-binding-missing", `${diagram.id}:${declaration}`);
      if (!vocabularyTerms.has(declaration)) fail("vocabulary-anchor-missing", `${diagram.id}:${declaration}`);
    }
    const embedMatches = handbook.match(new RegExp(`data-diagram-embed="${diagram.id}"`, "g")) ?? [];
    const asset = diagram.asset.replace(/^docs\//, "");
    const escapedAsset = asset.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const figure = handbook.match(new RegExp(`<figure[^>]+data-diagram-embed="${diagram.id}"[^>]*><img[^>]+src="${escapedAsset}"[^>]+alt="([^"]+)"[^>]*/><figcaption>([\\s\\S]*?)</figcaption></figure>`));
    const transcript = handbook.match(new RegExp(`<details id="${diagram.transcriptAnchor}"><summary>([^<]+)</summary><p>([\\s\\S]*?)</p></details>`));
    const nonempty = value => value?.replace(/<[^>]+>/g, "").trim().length > 0;
    if (embedMatches.length !== 1 || !figure || !nonempty(figure[1]) || !nonempty(figure[2])) fail("handbook-embed-binding-missing", diagram.id);
    if (!transcript || !nonempty(transcript[1]) || !nonempty(transcript[2])) fail("handbook-transcript-binding-missing", diagram.id);
    if (!figure?.[2].includes(`${asset}#effects-off`)) fail("effects-off-route-missing", diagram.id);
  }

  const attackSvg = content("docs/assets/sir-combat-quint/attack-pipeline.svg");
  const stateSvg = content("docs/assets/sir-combat-quint/state-action.svg");
  const arithmeticSvg = content("docs/assets/sir-combat-quint/q4-arithmetic.svg");
  const traceSvg = content("docs/assets/sir-combat-quint/trace-counterexample.svg");
  const invariantSvg = content("docs/assets/sir-combat-quint/invariant-boundary.svg");
  const coverStart = model.indexOf("  pure def coverObservation");
  const coverEnd = model.indexOf("  pure def recoveredSuppression");
  const coverBlock = coverStart >= 0 && coverEnd > coverStart ? model.slice(coverStart, coverEnd) : "";
  if (!/damage:\s*0,[\s\S]*?suppressionDelta:\s*0,[\s\S]*?stopsProjectile:\s*directAttack and projectileBlocking/.test(coverBlock) || !attackSvg.includes("STOP · target damage 0")) fail("cover-stop-projection-mismatch", "cover observation must stop direct blocking projectiles with zero target damage");
  if (!attackSvg.includes(`${representativeDamage} damage · HP ${successorHealth}/${initialHealth} · ${expectedWound}`)) fail("representative-consequence-mismatch", `${representativeDamage}/${expectedWound}`);
  const stateProjection = [
    `health = ${initialHealth}`,
    `suppression = ${initialSuppression}`,
    `coverIntegrity = ${initialCoverIntegrity}`,
    `coverBlocking = ${initialCoverBlocking}`,
    `incapacitated = ${initialIncapacitated}`,
    `health = ${successorHealth} · suppression = ${successorSuppression}`,
    `damage = ${representativeDamage} · wound = ${expectedWound}`,
    "Observation",
    "action disabled; no transition"
  ];
  if (stateProjection.some(label => !stateSvg.includes(label)) || stateSvg.includes("stutter")) fail("state-action-projection-mismatch", JSON.stringify(stateProjection));
  if (!arithmeticSvg.includes("Full trace retention")
      || !arithmeticSvg.includes(`${rifleDamageRaw} raw`)
      || !arithmeticSvg.includes(`${traceRaw} raw`)
      || !arithmeticSvg.includes(`${representativeArmorRaw} raw`)
      || !arithmeticSvg.includes(`${representativeDamageRaw} raw`)
      || !arithmeticSvg.includes(`${representativeDamage} HP`)
      || !arithmeticSvg.includes(`SCALE = ${scale.toLocaleString("en-US")}`)) fail("arithmetic-projection-mismatch", `${rifleDamageRaw}/${traceRaw}/${representativeArmorRaw}/${representativeDamageRaw}/${representativeDamage}/${scale}`);
  if (!Number.isInteger(mutatedRetentionRaw)
      || !traceSvg.includes(`damage ${representativeDamage}`)
      || !traceSvg.includes(`HP ${successorHealth}`)
      || !traceSvg.includes("property ✓")
      || !traceSvg.includes(`${mutatedRetentionRaw} → damage ${mutatedDamage}`)
      || !traceSvg.includes(`HP ${mutatedHealth}`)
      || !traceSvg.includes("property ✕")) fail("trace-mutation-projection-mismatch", `${mutatedRetentionRaw}/${mutatedDamage}/${mutatedHealth}`);
  const boundedStart = model.indexOf("  val boundedCombatState");
  const boundedEnd = model.indexOf("  val incapacityMatchesHealth", boundedStart);
  const boundedBlock = boundedStart >= 0 && boundedEnd > boundedStart ? model.slice(boundedStart, boundedEnd) : "";
  const boundedFields = [...boundedBlock.matchAll(/combat\.(health|suppression|coverIntegrity) >= (-?\d+), combat\.\1 <= (-?\d+)/g)].map(match => ({ field: match[1], minimum: Number(match[2]), maximum: Number(match[3]) }));
  const boundedMinimum = boundedFields[0]?.minimum;
  const boundedMaximum = boundedFields[0]?.maximum;
  const hypotheticalEscape = boundedMinimum - 1;
  if (boundedFields.length !== 3 || boundedFields.some(bound => bound.minimum !== boundedMinimum || bound.maximum !== boundedMaximum)
      || !model.includes("val incapacityMatchesHealth = combat.incapacitated == (combat.health == 0)")
      || !invariantSvg.includes(`${boundedMaximum} HP`) || !invariantSvg.includes(`${boundedMinimum} HP`)
      || !invariantSvg.includes(`${String(hypotheticalEscape).replace("-", "−")} HP`)
      || !invariantSvg.includes("boundedCombatState ∧ incapacityMatchesHealth")) fail("invariant-boundary-projection-mismatch", JSON.stringify({ boundedFields, hypotheticalEscape }));

  if (aggregateElements > budgets.aggregateElements) fail("aggregate-element-budget-exceeded", `${aggregateElements}`);
  if (aggregateBytes > budgets.aggregateBytes) fail("aggregate-byte-budget-exceeded", `${aggregateBytes}`);
  if (aggregateAnimated > budgets.animatedElements) fail("animated-element-budget-exceeded", `${aggregateAnimated}`);
  if (manifest.fallbackContract?.webglRequired !== false) fail("non-webgl-contract-broken", "manifest");
  const aggregate = { diagrams: diagrams.length, elements: aggregateElements, bytes: aggregateBytes, animatedElements: aggregateAnimated };
  if (checkVisualQualification && (visualQualification.result !== "pass" || visualQualification.workloadId !== manifest.workload.id || visualQualification.workloadDefinitionDigest !== expectedWorkloadDigest
      || JSON.stringify(visualQualification.counters) !== JSON.stringify(aggregate)
      || JSON.stringify(visualQualification.budgets) !== JSON.stringify(manifest.workload.maximumScale)
      || visualQualification.capability?.headlessBrowser !== true || visualQualification.capability?.liveCompositor !== false || visualQualification.capability?.framePacingMeasured !== false
      || visualQualification.sourceTreeDigest !== sha256(JSON.stringify(metrics)))) fail("visual-qualification-contract-mismatch", JSON.stringify(visualQualification.counters));
  const candidateInputPaths = [manifestPath, handbookPath, "work/377-handbook-m6v/render-baseline.json", "work/377-handbook-m6v/inspect-rendered-visuals.mjs", "work/377-handbook-m6v/preflight-render-browser.mjs", ...manifest.sources.map(source => source.path), ...diagrams.map(diagram => diagram.asset)];
  const candidateInputDigest = sha256([...new Set(candidateInputPaths)].sort().map(relative => `${relative}\0${sha256(content(relative))}`).join("\n"));
  let capturedTree;
  try { capturedTree = git("rev-parse", `${renderReceipt.provenance?.sourceRevisionAtCapture}^{tree}`); } catch { capturedTree = undefined; }
  let capturedNegativeTree;
  try { capturedNegativeTree = git("rev-parse", `${timingMutation.provenance?.sourceRevisionAtCapture}^{tree}`); } catch { capturedNegativeTree = undefined; }
  if (renderReceipt.provenance?.sourceTreeCleanAtCapture !== true
      || renderReceipt.provenance?.candidateSourceInputsSha256 !== candidateInputDigest
      || renderReceipt.provenance?.renderBaselineSha256 !== sha256(content("work/377-handbook-m6v/render-baseline.json"))
      || renderReceipt.provenance?.renderBaselineRecordedDuringCapture !== false
      || !renderReceipt.provenance?.sourceRevisionAtCapture
      || capturedTree !== renderReceipt.provenance?.sourceTreeAtCapture) fail("candidate-provenance-mismatch", JSON.stringify(renderReceipt.provenance));
  if (timingMutation.provenance?.sourceTreeCleanAtCapture !== true
      || timingMutation.provenance?.candidateSourceInputsSha256 !== candidateInputDigest
      || timingMutation.provenance?.renderBaselineSha256 !== sha256(content("work/377-handbook-m6v/render-baseline.json"))
      || timingMutation.provenance?.renderBaselineRecordedDuringCapture !== false
      || !timingMutation.provenance?.sourceRevisionAtCapture
      || capturedNegativeTree !== timingMutation.provenance?.sourceTreeAtCapture) fail("timing-mutation-provenance-mismatch", JSON.stringify(timingMutation.provenance));
  return { errors, metrics, aggregate };
}

function selfTest() {
  const pristine = validate();
  if (pristine.errors.length) throw new Error(`pristine audit failed: ${JSON.stringify(pristine.errors)}`);
  const cases = [
    ["authority-drift", "dependency-rule-set-mismatch", () => { const p = "docs/rules/sir-combat.md"; const changed = read(p).replace('dependencies: Set("COMBAT-ENGAGEMENT-001", "COMBAT-COLLISION-001"', 'dependencies: Set("COMBAT-TRACE-002", "COMBAT-COLLISION-001"'); const m = clone(baseManifest); m.sources.find(item => item.path === p).sha256 = sha256(changed); return { manifest: m, overrides: new Map([[p, changed]]) }; }],
    ["production-glyph-drift", "production-render-contract-mismatch", () => { const p = "src/SIR.Client.Web/App.fs"; const changed = read(p).replace("svg.strokeWidth 1.8", "svg.strokeWidth 1.7"); const m = clone(baseManifest); m.sources.find(item => item.path === p).sha256 = sha256(changed); return { manifest: m, overrides: new Map([[p, changed]]) }; }],
    ["accessibility-loss", "handbook-embed-binding-missing", () => { const p = handbookPath; return { overrides: new Map([[p, read(p).replace(/<figcaption>[^<]+<a href="assets\/sir-combat-quint\/state-action\.svg#effects-off">effects-off view<\/a><\/figcaption>/, "<figcaption></figcaption>")]]) }; }],
    ["fallback-loss", "static-fallback-missing", () => { const p = baseManifest.diagrams[1].asset; return { overrides: new Map([[p, read(p).replace('data-semantic-edge="pre-state-to-action" data-directed-edge="true" d="M219 108 H270" fill="none" stroke="#53b7ff"', 'data-semantic-edge="pre-state-to-action" data-directed-edge="true" d="M219 108 H270" fill="none"')]]) }; }],
    ["visual-fingerprint-drift", "asset-fingerprint-mismatch", () => { const p = baseManifest.diagrams[4].asset; return { overrides: new Map([[p, read(p).replace("damage 18", "damage 17")]]) }; }],
    ["structural-overflow", "element-budget-exceeded", () => { const p = baseManifest.diagrams[5].asset; const extra = Array.from({length: 31}, (_, i) => `<circle cx="${i}" cy="1" r="1"/>`).join(""); return { overrides: new Map([[p, read(p).replace("</svg>", `${extra}</svg>`) ]]) }; }]
  ];
  for (const [name, detector, mutate] of cases) {
    const mutation = mutate();
    for (const [subject, changed] of mutation.overrides ?? []) if (changed === read(subject)) throw new Error(`${name} did not change its mutation subject ${subject}`);
    const result = validate(mutation);
    if (!result.errors.some(error => error.code === detector)) throw new Error(`${name} did not trigger ${detector}: ${JSON.stringify(result.errors)}`);
    const restored = validate();
    if (restored.errors.length) throw new Error(`${name} did not restore green`);
    console.log(`observed red/restored green: ${name} (${detector})`);
  }
  return pristine;
}

const selfTesting = process.argv.includes("--self-test");
const writeReceipt = process.argv.includes("--write-receipt");
const result = selfTesting ? selfTest() : validate({ checkVisualQualification: !writeReceipt });
if (result.errors.length) {
  for (const error of result.errors) console.error(`visual-audit:${error.code}: ${error.detail}`);
  process.exit(1);
}
if (writeReceipt) {
  const receipt = {
    schema: "sir.handbook.visual-qualification/v1",
    workloadId: baseManifest.workload.id,
    workloadDefinitionDigest: baseManifest.workload.definitionDigest,
    sourceTreeDigest: sha256(JSON.stringify(result.metrics)),
    result: "pass",
    counters: result.aggregate,
    budgets: baseManifest.workload.maximumScale,
    capability: { headlessBrowser: true, liveCompositor: false, framePacingMeasured: false },
    diagrams: result.metrics
  };
  fs.writeFileSync(path.join(root, "readiness/377-handbook-m6v/visual-qualification.json"), JSON.stringify(receipt, null, 2) + "\n");
}
console.log(`visual explanation audit passed: ${result.aggregate.diagrams} diagrams, ${result.aggregate.elements} rendered elements, ${result.aggregate.bytes} bytes, ${result.aggregate.animatedElements} animated elements`);
