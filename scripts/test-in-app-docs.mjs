import { createHash } from "node:crypto";
import { readFile, stat } from "node:fs/promises";
import { resolve } from "node:path";

const site = resolve(process.argv[2] ?? "artifacts/site");
const path = resolve(site, "content/sir-client/v1/in-app-docs.json");
const allowedStatuses = new Set(["canonical", "implemented", "provisional", "research"]);
const allowedBlocks = new Set(["heading", "paragraph", "code", "table", "image"]);
const sha256 = (value) => createHash("sha256").update(value).digest("hex");

function decode(text) {
  let manifest;
  try { manifest = JSON.parse(text); }
  catch { throw new Error("Unreadable in-app documentation manifest."); }
  if (manifest?.schema !== "sir-in-app-docs-v1" || !Array.isArray(manifest.pages)) throw new Error("Unsupported in-app documentation manifest.");
  const slugs = manifest.pages.map((page) => page.slug);
  if (new Set(slugs).size !== slugs.length) throw new Error("Duplicate documentation slug.");
  const known = new Set(slugs);
  const anchors = new Map(manifest.pages.map((page) => [page.slug, new Set(page.anchors)]));
  for (const page of manifest.pages) {
    if (!allowedStatuses.has(page.status)) throw new Error("Unknown documentation status.");
    if (!Array.isArray(page.blocks) || page.blocks.some((block) => !allowedBlocks.has(block.kind))) throw new Error("Unknown documentation block.");
    if (page.related.some((slug) => !known.has(slug))) throw new Error("Broken documentation cross-link.");
    const blockAnchors = new Set(page.blocks.filter((block) => block.kind === "heading").map((block) => block.anchor));
    if (!Array.isArray(page.anchors) || page.anchors.some((anchor) => typeof anchor !== "string" || !anchor)) throw new Error("Invalid documentation anchor inventory.");
    if (page.headings.some((heading) => !heading.anchor || !blockAnchors.has(heading.anchor))) throw new Error("Broken documentation heading anchor.");
    if (page.headings.some((heading) => !anchors.get(page.slug).has(heading.anchor))) throw new Error("Documentation heading missing from anchor inventory.");
    for (const block of page.blocks) {
      if (block.kind === "table" && (!Array.isArray(block.rows) || block.rows.length === 0)) throw new Error("Unrenderable documentation table.");
      if (block.kind === "image" && (!block.imageSource || !block.text)) throw new Error("Unrenderable documentation image.");
      for (const segment of block.segments ?? []) {
        if (segment.kind === "text") continue;
        if (segment.kind !== "link" || (!segment.externalUrl && !known.has(segment.targetSlug))) throw new Error("Broken documentation link target.");
        if (segment.anchor && !anchors.get(segment.targetSlug)?.has(segment.anchor)) throw new Error("Broken documentation link anchor.");
      }
    }
    if (page.apiPath && (!page.apiPath.startsWith("reference/") || (!page.sourcePath.startsWith("reference/") && !page.sourcePath.startsWith("src/")))) throw new Error("Invalid FsDocs API identity.");
  }
  for (const source of Object.values(manifest.sources ?? {})) {
    if (!known.has(source.pageSlug) || !/^[0-9a-f]{40}$/.test(source.revision) || !Number.isInteger(source.line) || source.line < 1 || !/^[0-9a-f]{64}$/.test(source.contentDigest) || !/^[0-9a-f]{64}$/.test(source.lineDigest)) throw new Error("Stale documentation source mapping.");
  }
  return manifest;
}

const subject = await readFile(path, "utf8");
const manifest = decode(subject);
for (const source of Object.values(manifest.sources)) {
  if (source.repository !== "EHotwagner/S.I.R." || source.line < 1) throw new Error("Invalid source mapping.");
  if (!(await stat(resolve(source.path))).isFile()) throw new Error("Missing mapped source file.");
  const content = await readFile(resolve(source.path), "utf8");
  const lines = content.split("\n");
  if (sha256(content) !== source.contentDigest || source.line > lines.length || sha256(lines[source.line - 1]) !== source.lineDigest || !lines[source.line - 1].toLocaleLowerCase("en-US").includes(source.symbol.toLocaleLowerCase("en-US"))) throw new Error("Source identity does not match mapped source.");
}
for (const page of manifest.pages) {
  const contentPath = page.sourcePath.startsWith("reference/") ? resolve(site, page.sourcePath) : resolve(page.sourcePath);
  const content = await readFile(contentPath, "utf8");
  if (sha256(content) !== page.contentDigest) throw new Error("Documentation content identity mismatch.");
}
if (manifest.pages.filter((page) => page.apiPath).length !== 4) throw new Error("Selected FsDocs API identities are missing.");
for (const query of ["los", "cover", "armor"]) {
  if (!manifest.pages.some((page) => JSON.stringify(page).toLowerCase().includes(query))) throw new Error(`Missing qualified search subject: ${query}.`);
}

const mutations = [
  ["unreadable-input", "{"],
  ["duplicate-slug", JSON.stringify({ ...manifest, pages: [...manifest.pages, manifest.pages[0]] })],
  ["broken-related", JSON.stringify({ ...manifest, pages: manifest.pages.map((page, index) => index ? page : { ...page, related: ["missing/page"] }) })],
  ["unsafe-block-kind", JSON.stringify({ ...manifest, pages: manifest.pages.map((page, index) => index ? page : { ...page, blocks: [{ kind: "script", text: "alert(1)" }] }) })],
  ["stale-source-mapping", JSON.stringify({ ...manifest, sources: { ...manifest.sources, combat: { ...manifest.sources.combat, pageSlug: "missing/page" } } })],
  ["broken-heading-anchor", JSON.stringify({ ...manifest, pages: manifest.pages.map((page, index) => index ? page : { ...page, headings: [{ title: "Broken", anchor: "missing-anchor" }] }) })],
  ["broken-link-anchor", JSON.stringify({ ...manifest, pages: manifest.pages.map((page, index) => index ? page : { ...page, blocks: [...page.blocks, { kind: "paragraph", text: "broken", segments: [{ kind: "link", text: "broken", targetSlug: page.slug, anchor: "missing-anchor" }] }] }) })],
];
for (const [name, mutation] of mutations) {
  let rejected = false;
  try { decode(mutation); } catch { rejected = true; }
  if (!rejected) throw new Error(`Mutation was not rejected: ${name}.`);
}
let sourceLineMutationRejected = false;
try {
  const mutated = decode(JSON.stringify({ ...manifest, sources: { ...manifest.sources, combat: { ...manifest.sources.combat, line: 999999 } } }));
  const source = mutated.sources.combat;
  const lines = (await readFile(resolve(source.path), "utf8")).split("\n");
  if (source.line > lines.length) throw new Error("rejected");
} catch { sourceLineMutationRejected = true; }
if (!sourceLineMutationRejected) throw new Error("Mutation was not rejected: out-of-range-source-line.");
console.log(JSON.stringify({ schema: "sir-in-app-docs-qualification-v1", pages: manifest.pages.length, apiPages: manifest.pages.filter((page) => page.apiPath).length, sources: Object.keys(manifest.sources).length, queries: ["los", "cover", "armor"], rejectedMutations: [...mutations.map(([name]) => name), "out-of-range-source-line"] }));
