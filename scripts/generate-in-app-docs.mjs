import { createHash } from "node:crypto";
import { readFile, readdir, stat, writeFile } from "node:fs/promises";
import { basename, extname, relative, resolve, sep } from "node:path";

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

function blocksOf(body) {
  const blocks = [];
  let code = false;
  let buffer = [];
  const flush = (kind = "paragraph") => {
    const text = buffer.join("\n").trim();
    if (text) blocks.push({ kind, text });
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
    } else if (/^\s*!\[[^\]]*\]\([^)]+\)\s*$/.test(line)) { flush(); blocks.push({ kind: "image", text: line.trim() }); }
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
  const parsed = frontmatter(source) ?? { fields: {}, body: source };
  const sourcePath = slash(path);
  const relativePath = relative(resolve("docs"), path).split(sep).join("/");
  const slug = relativePath.replace(new RegExp(`${extname(relativePath)}$`), "").split("/").map(slugify).join("/");
  const blocks = blocksOf(parsed.body);
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
pages.sort((a, b) => a.categoryIndex - b.categoryIndex || a.index - b.index || a.title.localeCompare(b.title) || a.slug.localeCompare(b.slug));
const duplicates = pages.map((page) => page.slug).filter((slug, index, all) => all.indexOf(slug) !== index);
if (duplicates.length) throw new Error(`Duplicate in-app documentation slug: ${[...new Set(duplicates)].join(", ")}`);
const slugs = new Set(pages.map((page) => page.slug));
for (const page of pages) for (const related of page.related) if (!slugs.has(related)) throw new Error(`Missing related page ${related} from ${page.slug}.`);

const sourceDefinitions = {
  combat: ["docs/combat-resolution.md", "SIR combat resolution", "combat-resolution"],
  units: ["docs/gameplay-units.md", "SIR unit model", "gameplay-units"],
  "maps-spatial": ["src/SIR.Client/UnifiedTacticalWorkspace.fs", "SIR.Client spatial workspace", "map-editor"],
  simulation: ["docs/simulation-core-architecture.md", "SIR deterministic simulation", "simulation-core-architecture"],
  planning: ["docs/planning-workspace.md", "SIR planning workspace", "planning-workspace"],
  replay: ["docs/svg-replay-player.md", "SIR replay inspection", "svg-replay-player"],
  controls: ["src/SIR.Client/ModalInput.fs", "SIR modal controls", "keyboardinput/readme"],
  governance: ["docs/game-governance.md", "SIR governance", "game-governance"],
};
const sources = {};
for (const [concept, [path, symbol, pageSlug]] of Object.entries(sourceDefinitions)) {
  const absolute = resolve(path);
  const info = await stat(absolute);
  if (!info.isFile()) throw new Error(`Source mapping is not a file: ${path}`);
  if (!slugs.has(pageSlug)) throw new Error(`Source mapping page is missing: ${pageSlug}`);
  sources[concept] = { repository: "EHotwagner/S.I.R.", revision: "main", path, pageSlug, concept, symbol, line: 1 };
}
const searchTokenCount = pages.reduce((total, page) => total + page.blocks.reduce((count, block) => count + (block.text.match(/[\p{L}\p{N}_-]+/gu)?.length ?? 0), 0), 0);
const blockCount = pages.reduce((total, page) => total + page.blocks.length, 0);
if (pages.length > limits.pages || blockCount > limits.blocks || searchTokenCount > limits.searchTokens) throw new Error("In-app documentation structural budget exceeded.");
const definitionDigest = sha256(JSON.stringify({ schema, limits, sourceDefinitions, parser: "closed-blocks-v1" }));
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
