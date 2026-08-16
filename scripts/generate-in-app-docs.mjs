import { createHash } from "node:crypto";
import { readFile, readdir, stat, writeFile } from "node:fs/promises";
import { basename, dirname, extname, relative, resolve, sep } from "node:path";
import { execFileSync } from "node:child_process";

const root = resolve(".");
const site = resolve(process.argv[2] ?? "artifacts/site");
const output = resolve(site, "content/sir-client/v1/in-app-docs.json");
const schema = "sir-in-app-docs-v1";
const limits = { pages: 512, blocks: 8192, searchTokens: 262144, results: 200, history: 128, domNodes: 6000 };
const sha256 = (value) => createHash("sha256").update(value).digest("hex");
const slash = (path) => relative(root, path).split(sep).join("/");
const slugify = (value) => value.toLocaleLowerCase("en-US").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");

async function filesUnder(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const nested = await Promise.all(entries.map(async (entry) => {
    const path = resolve(directory, entry.name);
    return entry.isDirectory() ? filesUnder(path) : [path];
  }));
  return nested.flat();
}

function frontmatter(text) {
  if (!text.startsWith("---\n")) return undefined;
  const end = text.indexOf("\n---\n", 4);
  if (end < 0) throw new Error("Unterminated documentation frontmatter.");
  const fields = {};
  let list;
  for (const line of text.slice(4, end).split("\n")) {
    const item = line.match(/^\s+-\s+(.+)$/);
    if (item && list) { fields[list].push(item[1].trim()); continue; }
    const match = line.match(/^([a-zA-Z][\w-]*):\s*(.*)$/);
    if (!match) continue;
    const [, key, raw] = match;
    if (raw === "") { fields[key] = []; list = key; }
    else { fields[key] = raw.replace(/^['"]|['"]$/g, "").trim(); list = undefined; }
  }
  return { fields, body: text.slice(end + 5) };
}

function statusOf(fields, path) {
  const value = `${fields["decision-status"] ?? ""} ${fields.status ?? ""} ${path}`.toLowerCase();
  if (value.includes("research") || path.includes("/research/")) return "research";
  if (value.includes("provisional") || value.includes("proposal") || value.includes("draft")) return "provisional";
  if (value.includes("implemented") || value.includes("complete")) return "implemented";
  return "canonical";
}

function targetOf(rawTarget, sourcePath) {
  if (/^(?:https?:|mailto:)/i.test(rawTarget)) return { externalUrl: rawTarget };
  const [rawPath, anchor] = rawTarget.split("#", 2);
  const resolvedPath = rawPath
    ? relative(resolve("docs"), resolve(dirname(sourcePath), rawPath)).split(sep).join("/")
    : relative(resolve("docs"), resolve(sourcePath)).split(sep).join("/");
  const targetSlug = resolvedPath.replace(/\.(?:md|fsx)$/i, "").split("/").map(slugify).join("/");
  return { targetSlug, anchor: anchor || undefined };
}

function segmentsOf(text, sourcePath) {
  const segments = [];
  let cursor = 0;
  for (const match of text.matchAll(/\[([^\]]+)\]\(([^)]+)\)/g)) {
    if (match.index > cursor) segments.push({ kind: "text", text: text.slice(cursor, match.index) });
    segments.push({ kind: "link", text: match[1], ...targetOf(match[2], sourcePath) });
    cursor = match.index + match[0].length;
  }
  if (cursor < text.length) segments.push({ kind: "text", text: text.slice(cursor) });
  return segments.length ? segments : [{ kind: "text", text }];
}

function blocksOf(body, sourcePath) {
  const blocks = [];
  let code = false;
  let buffer = [];
  const flush = (kind = "paragraph") => {
    const text = buffer.join("\n").trim();
    if (text && kind === "table") {
      const rows = text.split("\n").filter((row, index) => index !== 1 || !/^\s*\|?\s*:?-+/.test(row)).map((row) => row.replace(/^\s*\||\|\s*$/g, "").split("|").map((cell) => cell.trim()));
      blocks.push({ kind, text, rows });
    } else if (text) blocks.push({ kind, text, segments: kind === "paragraph" ? segmentsOf(text, sourcePath) : undefined });
    buffer = [];
  };
  for (const line of body.split("\n")) {
    if (line.startsWith("```")) { if (code) { flush("code"); code = false; } else { flush(); code = true; } continue; }
    if (code) { buffer.push(line); continue; }
    const heading = line.match(/^(#{1,6})\s+(.+)$/);
    if (heading) {
      flush();
      const title = heading[2].replace(/[*_`]/g, "").trim();
      blocks.push({ kind: "heading", level: heading[1].length, anchor: slugify(title), text: title });
    } else if (/^\s*!\[[^\]]*\]\([^)]+\)\s*$/.test(line)) {
      flush();
      const [, alt, imagePath] = line.trim().match(/^!\[([^\]]*)\]\(([^)]+)\)$/);
      blocks.push({ kind: "image", text: alt, imageSource: imagePath });
    }
    else if (/^\s*\|/.test(line)) { buffer.push(line); }
    else if (line.trim() === "") { flush(buffer.some((row) => /^\s*\|/.test(row)) ? "table" : "paragraph"); }
    else if (/<\/?(?:script|style|iframe|object|embed)|<[^>]+\son\w+\s*=|javascript:/i.test(line)) throw new Error("Unsafe markup in qualified documentation.");
    else buffer.push(line);
  }
  flush(code ? "code" : (buffer.some((row) => /^\s*\|/.test(row)) ? "table" : "paragraph"));
  return blocks;
}

const candidates = (await filesUnder(resolve("docs")))
  .filter((path) => /\.(?:md|fsx)$/i.test(path))
  .filter((path) => !/\/(?:assets|reports|keyboardInput\/historical)\//.test(path))
  .sort();
const pages = [];
for (const path of candidates) {
  const source = await readFile(path, "utf8");
  const parsed = frontmatter(source);
  if (!parsed) continue;
  const sourcePath = slash(path);
  const relativePath = relative(resolve("docs"), path).split(sep).join("/");
  const slug = relativePath.replace(new RegExp(`${extname(relativePath)}$`), "").split("/").map(slugify).join("/");
  const blocks = blocksOf(parsed.body, path);
  const headings = blocks.filter((block) => block.kind === "heading").map(({ text, anchor }) => ({ title: text, anchor }));
  const firstHeading = parsed.body.match(/^#\s+(.+)$/m)?.[1]?.replace(/[*_`]/g, "").trim();
  pages.push({
    slug,
    title: parsed.fields.title ?? firstHeading ?? basename(relativePath, extname(relativePath)).replace(/[-_]/g, " "),
    category: parsed.fields.category ?? (relativePath.startsWith("research/") ? "Research" : "Documentation"),
    categoryIndex: Number(parsed.fields.categoryindex ?? 999),
    index: Number(parsed.fields.index ?? 999),
    status: statusOf(parsed.fields, sourcePath),
    sourcePath,
    contentDigest: sha256(source),
    headings,
    related: Array.isArray(parsed.fields.related) ? parsed.fields.related.map((value) => value.replace(/^docs\//, "").replace(/\.(?:md|fsx)$/i, "").split("/").map(slugify).join("/")) : [],
    blocks,
  });
}

const apiSelections = [
  { apiPath: "reference/sir-simulation-combat.html", sourcePath: "src/SIR.Simulation/CombatModel.fs", title: "SIR.Simulation.Combat" },
  { apiPath: "reference/sir-simulation-combatrules.html", sourcePath: "src/SIR.Simulation/CombatRules.fs", title: "SIR.Simulation.CombatRules" },
  { apiPath: "reference/sir-simulation-replay.html", sourcePath: "src/SIR.Simulation/Replay.fs", title: "SIR.Simulation.Replay" },
  { apiPath: "reference/sir-domain-rules.html", sourcePath: "src/SIR.Domain/Rules.fs", title: "SIR.Domain.Rules" },
];
for (const selection of apiSelections) {
  const { apiPath } = selection;
  const absolute = resolve(site, apiPath);
  let content;
  let sourcePath;
  let title = selection.title;
  let headings;
  try {
    content = await readFile(absolute, "utf8");
    sourcePath = apiPath;
    title = content.match(/<title>([^<]+)<\/title>/i)?.[1]?.replace(/\s+/g, " ").trim() ?? title;
    headings = [...content.matchAll(/<h([1-6])[^>]*id="([^"]+)"[^>]*>([\s\S]*?)<\/h\1>/gi)].map((match) => ({ title: match[3].replace(/<[^>]+>/g, "").replace(/&amp;/g, "&").trim(), anchor: match[2] }));
  } catch (error) {
    if (error?.code !== "ENOENT") throw error;
    sourcePath = selection.sourcePath;
    content = await readFile(resolve(sourcePath), "utf8");
    headings = [{ title, anchor: "api" }];
  }
  const plain = content.replace(/<script[\s\S]*?<\/script>/gi, " ").replace(/<style[\s\S]*?<\/style>/gi, " ").replace(/<[^>]+>/g, " ").replace(/&(?:nbsp|amp|lt|gt);/g, " ").replace(/\s+/g, " ").trim();
  pages.push({
    slug: apiPath.replace(/\.html$/i, ""), title, category: "API reference", status: "canonical",
    sourcePath, apiPath, contentDigest: sha256(content), headings,
    related: [], blocks: [
      ...headings.slice(0, 24).map((heading) => ({ kind: "heading", level: 2, anchor: heading.anchor, text: heading.title })),
      { kind: "paragraph", text: plain.slice(0, 12000), segments: [{ kind: "text", text: plain.slice(0, 12000) }] },
    ], categoryIndex: 900, index: apiSelections.indexOf(selection),
  });
}
pages.sort((a, b) => a.categoryIndex - b.categoryIndex || a.index - b.index || a.title.localeCompare(b.title) || a.slug.localeCompare(b.slug));
const duplicates = pages.map((page) => page.slug).filter((slug, index, all) => all.indexOf(slug) !== index);
if (duplicates.length) throw new Error(`Duplicate in-app documentation slug: ${[...new Set(duplicates)].join(", ")}`);
const slugs = new Set(pages.map((page) => page.slug));
for (const page of pages) page.related = page.related.filter((related) => slugs.has(related));
const anchorsBySlug = new Map(pages.map((page) => [page.slug, new Set(page.headings.map((heading) => heading.anchor))]));
for (const page of pages) for (const block of page.blocks) for (const segment of block.segments ?? []) {
  if (segment.kind !== "link" || segment.externalUrl) continue;
  if (!slugs.has(segment.targetSlug)) {
    segment.externalUrl = `/${segment.targetSlug}.html${segment.anchor ? `#${segment.anchor}` : ""}`;
    delete segment.targetSlug;
    delete segment.anchor;
    continue;
  }
  if (segment.anchor && !anchorsBySlug.get(segment.targetSlug).has(segment.anchor)) throw new Error(`Missing documentation anchor ${segment.targetSlug}#${segment.anchor} from ${page.slug}.`);
}

const sourceDefinitions = {
  combat: ["docs/combat-resolution.md", "Combat Resolution Architecture", "combat-resolution"],
  units: ["docs/gameplay-units.md", "Gameplay Units, Classes, and Progression", "gameplay-units"],
  "maps-spatial": ["src/SIR.Client/UnifiedTacticalWorkspace.fs", "type TacticalModality", "map-editor"],
  simulation: ["docs/simulation-core-architecture.md", "Deterministic Simulation Core", "simulation-core-architecture"],
  planning: ["docs/planning-workspace.md", "Coordinated Planning Workspace", "planning-workspace"],
  replay: ["docs/svg-replay-player.md", "SVG Replay Player Contract and Visual Catalog", "svg-replay-player"],
  controls: ["src/SIR.Client/ModalInput.fs", "type NormalizedKey", "keyboardinput/editor-simulator-modal-input-proposal"],
  governance: ["docs/fable-client-and-documentation.md", "Browser Client and Documentation Architecture", "fable-client-and-documentation"],
};
const sources = {};
const sourceRevision = process.env.SIR_DOCS_SOURCE_REVISION ?? process.env.GITHUB_SHA ?? execFileSync("git", ["rev-parse", "HEAD"], { encoding: "utf8" }).trim();
for (const [concept, [path, symbol, pageSlug]] of Object.entries(sourceDefinitions)) {
  const absolute = resolve(path);
  const info = await stat(absolute);
  if (!info.isFile()) throw new Error(`Source mapping is not a file: ${path}`);
  if (!slugs.has(pageSlug)) throw new Error(`Source mapping page is missing: ${pageSlug}`);
  const source = await readFile(absolute, "utf8");
  const lines = source.split("\n");
  const line = lines.findIndex((value) => value.toLocaleLowerCase("en-US").includes(symbol.toLocaleLowerCase("en-US"))) + 1;
  if (line < 1) throw new Error(`Source symbol is missing: ${symbol} in ${path}`);
  sources[concept] = { repository: "EHotwagner/S.I.R.", revision: sourceRevision, path, pageSlug, concept, symbol, line, contentDigest: sha256(source), lineDigest: sha256(lines[line - 1]) };
}
const searchTokenCount = pages.reduce((total, page) => total + page.blocks.reduce((count, block) => count + (block.text.match(/[\p{L}\p{N}_-]+/gu)?.length ?? 0), 0), 0);
const blockCount = pages.reduce((total, page) => total + page.blocks.length, 0);
if (pages.length > limits.pages || blockCount > limits.blocks || searchTokenCount > limits.searchTokens) throw new Error("In-app documentation structural budget exceeded.");
const definitionDigest = sha256(JSON.stringify({ schema, limits, sourceDefinitions, apiSelections, parser: "closed-blocks-v2" }));
const manifest = {
  schema,
  definitionDigest,
  revisionPolicy: "main",
  limits: { history: limits.history, results: limits.results },
  performance: { maximumDomNodes: limits.domNodes },
  searchTokenCount,
  pages: pages.map(({ categoryIndex, index, ...page }) => page),
  sources,
};
await writeFile(output, `${JSON.stringify(manifest, null, 2)}\n`);
console.log(`Wrote ${schema}: ${pages.length} pages, ${blockCount} blocks, ${searchTokenCount} search tokens, ${Object.keys(sources).length} source mappings.`);
