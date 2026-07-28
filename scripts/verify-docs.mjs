import { createHash } from "node:crypto";
import { access, readFile } from "node:fs/promises";
import { resolve } from "node:path";

const site = resolve(process.argv[2] ?? "artifacts/site");
const bundle = resolve(site, "content/sir-client/v1");
const manifestPath = resolve(bundle, "asset-manifest.json");
const manifest = JSON.parse(await readFile(manifestPath, "utf8"));

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

for (const required of [
  "index.html",
  "deterministic-simulation.html",
  "interactive-rules-lab.html",
  "reference/index.html",
  "content/fsdocs-search.js",
  "content/sir-client/v1/app.js",
  "content/sir-client/v1/styles.css",
]) {
  await access(resolve(site, required));
}

const home = await readFile(resolve(site, "index.html"), "utf8");
const interactive = await readFile(
  resolve(site, "interactive-rules-lab.html"),
  "utf8",
);
const example = await readFile(
  resolve(site, "deterministic-simulation.html"),
  "utf8",
);
const api = await readFile(
  resolve(site, "reference/sir-domain-boundedint32module.html"),
  "utf8",
);
const searchIndex = JSON.parse(await readFile(resolve(site, "index.json"), "utf8"));

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
  !searchIndex.some((entry) => entry.uri.endsWith("/interactive-rules-lab.html"))
) {
  throw new Error("The literate and interactive pages are missing from site search.");
}

if (
  !api.includes(
    "https://github.com/EHotwagner/S.I.R./blob/main/src/SIR.Domain/BoundedInt32.fs",
  )
) {
  throw new Error("Generated API source links do not resolve to the main branch.");
}

console.log(
  `Documentation verification passed: ${manifest.assets.length} versioned assets, SHA-384 integrity, project-root links, evaluated .NET output, Fable mount, API reference, search, and no-JavaScript fallback.`,
);
