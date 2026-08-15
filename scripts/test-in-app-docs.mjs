import { readFile, stat } from "node:fs/promises";
import { resolve } from "node:path";

const site = resolve(process.argv[2] ?? "artifacts/site");
const path = resolve(site, "content/sir-client/v1/in-app-docs.json");
const allowedStatuses = new Set(["canonical", "implemented", "provisional", "research"]);
const allowedBlocks = new Set(["heading", "paragraph", "code", "table", "image"]);

function decode(text) {
  let manifest;
  try { manifest = JSON.parse(text); }
  catch { throw new Error("Unreadable in-app documentation manifest."); }
  if (manifest?.schema !== "sir-in-app-docs-v1" || !Array.isArray(manifest.pages)) throw new Error("Unsupported in-app documentation manifest.");
  const slugs = manifest.pages.map((page) => page.slug);
  if (new Set(slugs).size !== slugs.length) throw new Error("Duplicate documentation slug.");
  const known = new Set(slugs);
  for (const page of manifest.pages) {
    if (!allowedStatuses.has(page.status)) throw new Error("Unknown documentation status.");
    if (!Array.isArray(page.blocks) || page.blocks.some((block) => !allowedBlocks.has(block.kind))) throw new Error("Unknown documentation block.");
    if (page.related.some((slug) => !known.has(slug))) throw new Error("Broken documentation cross-link.");
  }
  for (const source of Object.values(manifest.sources ?? {})) {
    if (!known.has(source.pageSlug) || source.revision !== "main" || !Number.isInteger(source.line) || source.line < 1) throw new Error("Stale documentation source mapping.");
  }
  return manifest;
}

const subject = await readFile(path, "utf8");
const manifest = decode(subject);
for (const source of Object.values(manifest.sources)) {
  if (source.repository !== "EHotwagner/S.I.R." || source.revision !== "main" || source.line < 1) throw new Error("Invalid source mapping.");
  if (!(await stat(resolve(source.path))).isFile()) throw new Error("Missing mapped source file.");
}
for (const query of ["los", "cover", "armor"]) {
  if (!manifest.pages.some((page) => JSON.stringify(page).toLowerCase().includes(query))) throw new Error(`Missing qualified search subject: ${query}.`);
}

const mutations = [
  ["unreadable-input", "{"],
  ["duplicate-slug", JSON.stringify({ ...manifest, pages: [...manifest.pages, manifest.pages[0]] })],
  ["broken-related", JSON.stringify({ ...manifest, pages: manifest.pages.map((page, index) => index ? page : { ...page, related: ["missing/page"] }) })],
  ["unsafe-block-kind", JSON.stringify({ ...manifest, pages: manifest.pages.map((page, index) => index ? page : { ...page, blocks: [{ kind: "script", text: "alert(1)" }] }) })],
  ["stale-source-mapping", JSON.stringify({ ...manifest, sources: { ...manifest.sources, combat: { ...manifest.sources.combat, pageSlug: "missing/page" } } })],
];
for (const [name, mutation] of mutations) {
  let rejected = false;
  try { decode(mutation); } catch { rejected = true; }
  if (!rejected) throw new Error(`Mutation was not rejected: ${name}.`);
}
console.log(JSON.stringify({ schema: "sir-in-app-docs-qualification-v1", pages: manifest.pages.length, sources: Object.keys(manifest.sources).length, queries: ["los", "cover", "armor"], rejectedMutations: mutations.map(([name]) => name) }));
