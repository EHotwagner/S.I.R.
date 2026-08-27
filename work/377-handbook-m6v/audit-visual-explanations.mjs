#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";

const root = process.cwd();
const manifestPath = "docs/sir-combat-quint-diagrams.json";
const handbookPath = "docs/sir-combat-quint-handbook.md";

const sha256 = text => crypto.createHash("sha256").update(text).digest("hex");
const read = relative => fs.readFileSync(path.join(root, relative), "utf8");
const clone = value => JSON.parse(JSON.stringify(value));
const baseManifest = JSON.parse(read(manifestPath));

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

function validate({ manifest = baseManifest, overrides = new Map() } = {}) {
  const errors = [];
  const content = relative => overrides.has(relative) ? overrides.get(relative) : read(relative);
  const fail = (code, detail) => errors.push({ code, detail });
  if (manifest.schemaVersion !== 1) fail("manifest-schema-invalid", "schemaVersion must equal 1");
  if (!manifest.authorityPosture?.includes("never")) fail("authority-posture-missing", "manifest must deny independent authority");
  const diagrams = manifest.diagrams ?? [];
  if (diagrams.length !== 6 || new Set(diagrams.map(item => item.id)).size !== 6) fail("diagram-inventory-mismatch", "expected six unique diagrams");

  const workloadDeclaration = "m6v-v1|six-diagrams|strict-fsdocs|chromium|30,180,20480,122880,24|normal,reduced-motion,print,effects-off";
  const expectedWorkloadDigest = `sha256:${sha256(workloadDeclaration)}`;
  if (manifest.workload?.definitionDigest !== expectedWorkloadDigest) fail("workload-digest-mismatch", `expected ${expectedWorkloadDigest}`);

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
  for (const primitive of manifest.productionVocabulary?.glyphPrimitives ?? []) {
    if (!glyphSource.includes(primitive)) fail("glyph-source-binding-missing", primitive);
  }
  for (const [name, token] of Object.entries(manifest.productionVocabulary?.palette ?? {})) {
    if (!glyphSource.includes(token)) fail("palette-source-binding-missing", `${name}:${token}`);
  }

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
    if (!new RegExp(`<svg[^>]+role="img"[^>]+aria-labelledby="${diagram.titleId} ${diagram.descId}"`, "i").test(svg)) fail("accessibility-semantics-missing", diagram.id);
    if (!new RegExp(`<title id="${diagram.titleId}">[^<]+</title>`, "i").test(svg) || !new RegExp(`<desc id="${diagram.descId}">[^<]+</desc>`, "i").test(svg)) fail("accessibility-title-description-missing", diagram.id);
    for (const group of svg.matchAll(/<g\b([^>]*)>/gi)) if (!/\baria-label="[^"]+"/.test(group[1])) fail("accessible-group-label-missing", diagram.id);
    if (!svg.includes("prefers-reduced-motion:reduce") || !svg.includes("@media print") || !svg.includes("[data-effects=\"off\"] .fx") || !svg.includes(".motion") || !svg.includes(".fx")) fail("fallback-contract-missing", diagram.id);
    if (/webgl|webgpu/i.test(svg)) fail("non-webgl-contract-broken", diagram.id);
    if (elements > budgets.elementsPerDiagram || aggregateElements > budgets.aggregateElements) fail("element-budget-exceeded", `${diagram.id}:${elements}`);
    if (bytes > budgets.bytesPerDiagram || aggregateBytes > budgets.aggregateBytes) fail("byte-budget-exceeded", `${diagram.id}:${bytes}`);
    if (sha256(svg) !== diagram.sha256) fail("asset-fingerprint-mismatch", diagram.id);
    if (diagram.kind === "concrete-mechanics") {
      for (const primitive of manifest.productionVocabulary.glyphPrimitives) if (!svg.includes(primitive)) fail("glyph-vocabulary-mismatch", `${diagram.id}:${primitive}`);
      for (const token of Object.values(manifest.productionVocabulary.palette)) if (!svg.toLowerCase().includes(token)) fail("palette-vocabulary-mismatch", `${diagram.id}:${token}`);
    } else if (!diagram.kind.startsWith("abstract-")) fail("diagram-kind-invalid", diagram.id);
    for (const rule of diagram.rules) if (!runtime.includes(`"${rule}"`) || !model.includes(`"${rule}"`)) fail("rule-binding-missing", `${diagram.id}:${rule}`);
    for (const declaration of diagram.declarations) {
      if (!new RegExp(`\\b${declaration.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")}\\b`).test(model)) fail("declaration-binding-missing", `${diagram.id}:${declaration}`);
      if (!vocabularyTerms.has(declaration)) fail("vocabulary-anchor-missing", `${diagram.id}:${declaration}`);
    }
    const embedMatches = handbook.match(new RegExp(`data-diagram-embed="${diagram.id}"`, "g")) ?? [];
    const transcriptMatches = handbook.match(new RegExp(`id="${diagram.transcriptAnchor}"`, "g")) ?? [];
    const asset = diagram.asset.replace(/^docs\//, "");
    const escapedAsset = asset.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const imageEmbed = new RegExp(`<figure[^>]+data-diagram-embed="${diagram.id}"[^>]*><img[^>]+src="${escapedAsset}"[^>]+alt="[^"]+"[^>]*/>`).test(handbook);
    if (embedMatches.length !== 1 || transcriptMatches.length !== 1 || !imageEmbed) fail("handbook-embed-binding-missing", diagram.id);
  }

  if (aggregateElements > budgets.aggregateElements) fail("aggregate-element-budget-exceeded", `${aggregateElements}`);
  if (aggregateBytes > budgets.aggregateBytes) fail("aggregate-byte-budget-exceeded", `${aggregateBytes}`);
  if (aggregateAnimated > budgets.animatedElements) fail("animated-element-budget-exceeded", `${aggregateAnimated}`);
  if (manifest.fallbackContract?.webglRequired !== false) fail("non-webgl-contract-broken", "manifest");
  return { errors, metrics, aggregate: { diagrams: diagrams.length, elements: aggregateElements, bytes: aggregateBytes, animatedElements: aggregateAnimated } };
}

function selfTest() {
  const pristine = validate();
  if (pristine.errors.length) throw new Error(`pristine audit failed: ${JSON.stringify(pristine.errors)}`);
  const cases = [
    ["authority-drift", "source-digest-mismatch", () => { const m = clone(baseManifest); m.sources.find(item => item.path === "docs/rules/sir-combat.md").sha256 = "0".repeat(64); return { manifest: m }; }],
    ["production-glyph-drift", "glyph-vocabulary-mismatch", () => { const p = baseManifest.diagrams[0].asset; return { overrides: new Map([[p, read(p).replace("M4 18 L12 4", "M4 18 L11 4")]]) }; }],
    ["accessibility-loss", "accessibility-semantics-missing", () => { const p = baseManifest.diagrams[1].asset; return { overrides: new Map([[p, read(p).replace(' role="img"', "")]]) }; }],
    ["fallback-loss", "fallback-contract-missing", () => { const p = baseManifest.diagrams[3].asset; return { overrides: new Map([[p, read(p).replace("@media print", "@media screen")]]) }; }],
    ["visual-fingerprint-drift", "asset-fingerprint-mismatch", () => { const p = baseManifest.diagrams[4].asset; return { overrides: new Map([[p, read(p).replace("damage 18", "damage 17")]]) }; }],
    ["performance-overflow", "element-budget-exceeded", () => { const p = baseManifest.diagrams[5].asset; const extra = Array.from({length: 31}, (_, i) => `<circle cx="${i}" cy="1" r="1"/>`).join(""); return { overrides: new Map([[p, read(p).replace("</svg>", `${extra}</svg>`) ]]) }; }]
  ];
  for (const [name, detector, mutate] of cases) {
    const result = validate(mutate());
    if (!result.errors.some(error => error.code === detector)) throw new Error(`${name} did not trigger ${detector}: ${JSON.stringify(result.errors)}`);
    const restored = validate();
    if (restored.errors.length) throw new Error(`${name} did not restore green`);
    console.log(`observed red/restored green: ${name} (${detector})`);
  }
  return pristine;
}

const selfTesting = process.argv.includes("--self-test");
const writeReceipt = process.argv.includes("--write-receipt");
const result = selfTesting ? selfTest() : validate();
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
