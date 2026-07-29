import { createHash } from "node:crypto";
import { access, readFile, readdir } from "node:fs/promises";
import { relative, resolve, sep } from "node:path";

const site = resolve(process.argv[2] ?? "artifacts/site");
const bundle = resolve(site, "content/sir-client/v1");
const manifestPath = resolve(bundle, "asset-manifest.json");
const manifest = JSON.parse(await readFile(manifestPath, "utf8"));
const publication = JSON.parse(
  await readFile(resolve(site, "publication-manifest.json"), "utf8"),
);

if (
  manifest.schema !== "sir-docs-assets-v1" ||
  manifest.bundleVersion !== "v1" ||
  manifest.runtime !== "Fable/JavaScript"
) {
  throw new Error("The documentation asset manifest identity is invalid.");
}

for (const asset of manifest.assets) {
  const bytes = await readFile(resolve(bundle, asset.path));
  const integrity = `sha384-${createHash("sha384").update(bytes).digest("base64")}`;

  if (bytes.byteLength !== asset.bytes || integrity !== asset.integrity) {
    throw new Error(`Integrity mismatch for ${asset.path}.`);
  }
}

if (
  publication.schema !== "sir-pages-publication-v1" ||
  publication.hosting !== "static-github-pages"
) {
  throw new Error("The Pages publication manifest identity is invalid.");
}

for (const engine of publication.engines) {
  if (!engine.workerPath.includes(engine.identity)) {
    throw new Error(`Engine ${engine.version} does not have an immutable path.`);
  }

  const bytes = await readFile(resolve(site, engine.workerPath));
  const integrity = `sha384-${createHash("sha384").update(bytes).digest("base64")}`;

  if (bytes.byteLength !== engine.bytes || integrity !== engine.integrity) {
    throw new Error(`Published engine integrity mismatch for ${engine.identity}.`);
  }

  const applicationBytes = await readFile(
    resolve(site, publication.application.script),
    "utf8",
  );
  if (
    !applicationBytes.includes(engine.identity) ||
    !applicationBytes.includes(engine.workerPath)
  ) {
    throw new Error(`The browser application cannot select engine ${engine.identity}.`);
  }
}

for (const formatVersion of publication.replayRetentionPolicy.supportedFormatVersions) {
  if (
    !publication.engines.some((engine) =>
      engine.replayFormatVersions.includes(formatVersion),
    )
  ) {
    throw new Error(`Retained replay format ${formatVersion} has no engine artifact.`);
  }
}

async function filesUnder(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const nested = await Promise.all(
    entries.map(async (entry) => {
      const path = resolve(directory, entry.name);
      return entry.isDirectory() ? filesUnder(path) : [path];
    }),
  );
  return nested.flat();
}

const publishedFiles = await filesUnder(site);
const forbidden = publishedFiles.filter((path) =>
  /(?:\.map|\.sirr|\.wasm|\.pdb|packages?\.lock\.json|package-lock\.json|\.env)$/i.test(
    path,
  ),
);

if (forbidden.length > 0) {
  throw new Error(
    `The Pages artifact exposes forbidden runtime/package material: ${forbidden
      .map((path) => relative(site, path).split(sep).join("/"))
      .join(", ")}`,
  );
}

for (const required of [
  "index.html",
  "foundations.html",
  "combat-formulas.html",
  "deterministic-simulation.html",
  "interactive-rules-lab.html",
  "research/rules-lab-prototype.html",
  "reference/index.html",
  "reference/sir-match-matchreplay.html",
  "content/fsdocs-search.js",
  "content/sir-docs.js",
  "content/sir-client/v1/app.js",
  "content/sir-client/v1/styles.css",
  "publication-manifest.json",
  ".nojekyll",
]) {
  await access(resolve(site, required));
}

const home = await readFile(resolve(site, "index.html"), "utf8");
const interactive = await readFile(
  resolve(site, "interactive-rules-lab.html"),
  "utf8",
);
const foundations = await readFile(resolve(site, "foundations.html"), "utf8");
const formulas = await readFile(resolve(site, "combat-formulas.html"), "utf8");
const example = await readFile(
  resolve(site, "deterministic-simulation.html"),
  "utf8",
);
const api = await readFile(
  resolve(site, "reference/sir-domain-boundedint32module.html"),
  "utf8",
);
const matchApi = await readFile(
  resolve(site, "reference/sir-match-matchreplay.html"),
  "utf8",
);
const searchIndex = JSON.parse(await readFile(resolve(site, "index.json"), "utf8"));

if (/(?:>\s*Other\s*<)/.test(interactive)) {
  throw new Error("The generated sidebar still exposes the uncurated Other section.");
}

for (const category of [
  "Start",
  "Foundations",
  "Forces & Equipment",
  "Battlefield Systems",
  "Tools & Evidence",
  "Engineering",
]) {
  if (!home.includes(category)) {
    throw new Error(`The structured sidebar omitted the ${category} category.`);
  }
}

if (
  /class="nav-link"[^>]*>\s*S\.I\.R\./.test(home) ||
  !home.includes("Attributes and State") ||
  !home.includes("Weapons and Equipment") ||
  home.indexOf("Weapons and Equipment") > home.indexOf("Units, Classes, and Progression")
) {
  throw new Error(
    "The sidebar labels or primitive-to-composed reading order regressed.",
  );
}

if (
  !home.includes("sir-system-map") ||
  !foundations.includes("sir-unit-anatomy") ||
  !home.includes('data-svg-tip="') ||
  !foundations.includes('data-svg-tip="') ||
  /<svg[\s\S]*?<pre class="fssnip/.test(home) ||
  /<svg[\s\S]*?<pre class="fssnip/.test(foundations)
) {
  throw new Error("The accessible SVG explainers were omitted or rendered as code.");
}

for (const curatedPage of [
  "game-vision.html",
  "simulation-core-architecture.html",
  "codebase-architecture.html",
  "technology-stack.html",
  "wasm-control-architecture.html",
  "public-protocol-architecture.html",
]) {
  if (!interactive.includes(`/${curatedPage}`)) {
    throw new Error(`The curated sidebar omitted ${curatedPage}.`);
  }
}

if (!home.includes("https://ehotwagner.github.io/S.I.R./content/fsdocs-default.css")) {
  throw new Error("Generated links do not use the GitHub Pages project root.");
}

if (
  !interactive.includes("Runtime: Fable/JavaScript in your browser.") ||
  !interactive.includes('id="sir-replay-app"') ||
  !interactive.includes("JavaScript is disabled.")
) {
  throw new Error("The interactive mount or its runtime/fallback disclosure is missing.");
}

if (
  !formulas.includes("engagementSeconds") ||
  !formulas.includes("Rifle, 25 m, 35% exposure: 1.629 s preparation") ||
  !formulas.includes("Front armour") ||
  !formulas.includes("Rear") ||
  !formulas.includes("expected damage/shot")
) {
  throw new Error(
    "The evaluated combat formula source or its build-time output is missing.",
  );
}

if (
  !example.includes("Runtime: .NET build-time evaluation") ||
  !example.includes("90 + 25 in [0, 100] = 100")
) {
  throw new Error("The evaluated .NET literate output is missing.");
}

if (!home.includes("fsdocs-search")) {
  throw new Error("Site search is not present in the generated corpus.");
}

if (
  !searchIndex.some((entry) => entry.uri.endsWith("/deterministic-simulation.html")) ||
  !searchIndex.some((entry) => entry.uri.endsWith("/foundations.html")) ||
  !searchIndex.some((entry) => entry.uri.endsWith("/combat-formulas.html")) ||
  !searchIndex.some((entry) => entry.uri.endsWith("/interactive-rules-lab.html")) ||
  !searchIndex.some((entry) =>
    entry.uri.endsWith("/research/rules-lab-prototype.html"),
  )
) {
  throw new Error(
    "The primary documentation or retained research archive is missing from site search.",
  );
}

if (
  !api.includes(
    "https://github.com/EHotwagner/S.I.R./blob/main/src/SIR.Domain/BoundedInt32.fs",
  ) ||
  !matchApi.includes(
    "https://github.com/EHotwagner/S.I.R./blob/main/src/SIR.Match/MatchReplay.fs",
  )
) {
  throw new Error("Generated API source links do not resolve to the main branch.");
}

console.log(
  `Documentation verification passed: ${manifest.assets.length} application assets, ${publication.engines.length} retained engine, SHA-384 integrity, retention coverage, safe static publication, project-root links, evaluated .NET output, Fable mount, API reference, search, and no-JavaScript fallback.`,
);
