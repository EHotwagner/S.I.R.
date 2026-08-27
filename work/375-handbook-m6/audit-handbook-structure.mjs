#!/usr/bin/env node

import fs from "node:fs";
import path from "node:path";

const root = process.cwd();
const handbookPath = path.join(root, "docs/sir-combat-quint-handbook.md");
const manifestPath = path.join(root, "docs/sir-combat-quint-vocabulary.json");
const modelPath = path.join(root, "docs/rules/sir-combat.md");
const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
const authoritativeModel = fs.readFileSync(modelPath, "utf8");

function slug(text) {
  return text.replace(/[*_`]/g, "").trim().toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
}

function inlineNodes(source) {
  const nodes = [];
  const token = /(`+)([\s\S]*?)\1|\[([^\]]+)\]\(([^)]+)\)|<[^>]+>/g;
  let cursor = 0;
  for (const match of source.matchAll(token)) {
    if (match.index > cursor) nodes.push({ type: "text", value: source.slice(cursor, match.index) });
    if (match[1]) nodes.push({ type: "inlineCode", value: match[2] });
    else if (match[3] !== undefined) nodes.push({ type: "link", label: match[3], destination: match[4] });
    else nodes.push({ type: "html", value: match[0] });
    cursor = match.index + match[0].length;
  }
  if (cursor < source.length) nodes.push({ type: "text", value: source.slice(cursor) });
  return nodes;
}

function markdownAst(markdown) {
  const lines = markdown.replace(/\r\n/g, "\n").split("\n");
  const children = [];
  let frontMatter = lines[0] === "---";
  let fence = false;
  let inIndex = false;
  for (let index = 0; index < lines.length; index += 1) {
    const line = lines[index];
    if (frontMatter) {
      children.push({ type: "frontMatter", line: index + 1, value: line });
      if (index > 0 && line === "---") frontMatter = false;
      continue;
    }
    if (/^\s*```/.test(line)) {
      fence = !fence;
      children.push({ type: "fence", line: index + 1, value: line });
      continue;
    }
    if (fence) {
      children.push({ type: "fencedCode", line: index + 1, value: line });
      continue;
    }
    if (/<a id="chapter-50-/.test(line)) inIndex = true;
    const heading = line.match(/^(#{1,6})\s+(.+)$/);
    if (heading) {
      children.push({ type: "heading", depth: heading[1].length, line: index + 1, value: heading[2], anchor: slug(heading[2]), inIndex });
      continue;
    }
    const anchor = line.match(/^\s*<a\s+id="([a-z0-9-]+)"\s*><\/a>\s*$/);
    if (anchor) {
      children.push({ type: "anchor", line: index + 1, anchor: anchor[1], inIndex });
      continue;
    }
    children.push({ type: "prose", line: index + 1, inIndex, children: inlineNodes(line) });
  }
  return { type: "root", children };
}

function topLevelDeclarations(modelMarkdown) {
  const quint = [...modelMarkdown.matchAll(/```quint[^\n]*\n([\s\S]*?)\n```/g)].map(match => match[1]).join("\n");
  const declarations = [];
  for (const line of quint.split("\n")) {
    let match = line.match(/^module\s+(\w+)/);
    if (match) declarations.push({ term: match[1], kind: "module" });
    match = line.match(/^  ((?:pure\s+)?(?:type|val|def|action|var|run|invariant))\s+(\w+)/);
    if (!match) continue;
    const form = match[1];
    let kind = "value";
    if (form.endsWith("type")) kind = "type";
    else if (form.endsWith("def")) kind = "function";
    else if (form === "action") kind = "action";
    else if (form === "var") kind = "variable";
    else if (form === "run") kind = "run";
    else if (form === "invariant") kind = "property";
    else if (/^[A-Z][A-Z0-9_]+$/.test(match[2])) kind = "constant";
    else if (["sixteenRulesDeclared", "boundedCombatState", "incapacityMatchesHealth", "destroyedCoverIsPermeable", "validTraceObservation", "suppressionRequiresDamage", "factionNeutralCollateral"].includes(match[2])) kind = "property";
    declarations.push({ term: match[2], kind });
  }
  return declarations;
}

function occurrencePattern(term) {
  const escaped = term.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  return /^[A-Za-z0-9_]/.test(term) && /[A-Za-z0-9_]$/.test(term)
    ? new RegExp(`(^|[^A-Za-z0-9_])${escaped}([^A-Za-z0-9_]|$)`, "g")
    : new RegExp(escaped, "g");
}

export function audit(markdown) {
  const errors = [];
  const add = (code, detail) => errors.push({ code, detail });
  if (manifest.schemaVersion !== 2) add("manifest-schema", `expected schemaVersion 2, got ${manifest.schemaVersion}`);
  if (manifest.anchorContract !== "semantic-explicit-v2") add("manifest-anchor-contract", "semantic-explicit-v2 is required");
  if (manifest.inventoryContract !== "literate-declarations-rules-chapters-index-v1") add("manifest-inventory-contract", "inventory contract is absent");
  for (const region of ["front-matter", "fenced-code", "headings", "inline-code", "canonical-index"])
    if (!manifest.exemptRegions?.includes(region)) add("manifest-exemption-missing", region);

  const ast = markdownAst(markdown);
  const anchorNodes = ast.children.filter(node => node.type === "anchor");
  const headingNodes = ast.children.filter(node => node.type === "heading");
  const anchorCounts = new Map();
  for (const node of [...anchorNodes.map(node => ({ anchor: node.anchor, line: node.line })), ...headingNodes.map(node => ({ anchor: node.anchor, line: node.line }))])
    anchorCounts.set(node.anchor, (anchorCounts.get(node.anchor) ?? 0) + 1);
  for (const [anchor, count] of anchorCounts) if (count !== 1) add("duplicate-anchor", `${anchor} occurs ${count} times`);

  const links = ast.children.flatMap(node => node.children ?? []).filter(node => node.type === "link");
  for (const link of links) {
    if (!link.destination.startsWith("#")) continue;
    const target = link.destination.slice(1);
    if ((anchorCounts.get(target) ?? 0) !== 1) add("missing-fragment", `${link.destination} resolves ${(anchorCounts.get(target) ?? 0)} times`);
  }

  const termByAnchor = new Map();
  const termByName = new Map();
  for (const entry of manifest.terms ?? []) {
    if (termByAnchor.has(entry.anchor)) add("manifest-anchor-duplicate", entry.anchor);
    if (termByName.has(entry.term)) add("manifest-term-duplicate", entry.term);
    termByAnchor.set(entry.anchor, entry);
    termByName.set(entry.term, entry);
  }

  const indexAnchor = "chapter-50-alphabetical-definition-index";
  const indexStart = markdown.indexOf(`<a id="${indexAnchor}"></a>`);
  const index = indexStart >= 0 ? markdown.slice(indexStart) : "";
  const indexEntries = [...index.matchAll(/<a id="([a-z0-9-]+)"><\/a>\n\*\*([^*]+)\*\* — ([^.]+)\. ([^\n]+)/g)];
  const indexedAnchors = new Map(indexEntries.map(match => [match[1], match]));
  for (const entry of manifest.terms ?? []) {
    if ((anchorCounts.get(entry.anchor) ?? 0) !== 1) add("manifest-anchor-missing", `${entry.term} -> ${entry.anchor}`);
    const match = indexedAnchors.get(entry.anchor);
    if (!match || match[2] !== entry.term || match[3] !== entry.kind) {
      add("index-entry-missing", `${entry.term} (${entry.kind}) at ${entry.anchor}`);
      continue;
    }
    const body = match[4];
    for (const field of ["**Declared at:**", "**Related terms:**", "**Runtime correspondence:**"])
      if (!body.includes(field)) add("index-entry-incomplete", `${entry.term} lacks ${field}`);
    if (/Planned definition|Pending\./.test(body)) add("index-entry-placeholder", entry.term);
    if (!/\[[^\]]+\]\(#[a-z0-9-]+\)/.test(body.slice(body.indexOf("**Related terms:**")))) add("index-related-link-missing", entry.term);
  }
  if (indexedAnchors.size !== manifest.terms.length) add("index-cardinality", `${indexedAnchors.size} entries for ${manifest.terms.length} terms`);
  const expectedOrder = [...manifest.terms].sort((left, right) => left.term.toLowerCase().localeCompare(right.term.toLowerCase()) || left.term.localeCompare(right.term)).map(entry => entry.term);
  const actualOrder = indexEntries.map(match => match[2]);
  if (JSON.stringify(expectedOrder) !== JSON.stringify(actualOrder)) add("index-order", "canonical entries are not alphabetic");

  const aliasNames = new Set();
  for (const alias of manifest.aliases ?? []) {
    if (aliasNames.has(alias.alias)) add("alias-duplicate", alias.alias);
    aliasNames.add(alias.alias);
    const canonical = termByName.get(alias.canonicalTerm);
    if (!canonical || canonical.anchor !== alias.anchor) add("alias-unresolved", `${alias.alias} -> ${alias.canonicalTerm}#${alias.anchor}`);
    if (alias.occurrencePolicy !== "canonical-index-only") add("alias-policy-unsupported", `${alias.alias}: ${alias.occurrencePolicy}`);
    const match = indexedAnchors.get(alias.anchor);
    if (!match?.[4].includes(`\`${alias.alias}\``)) add("alias-index-missing", alias.alias);
  }

  const declarations = topLevelDeclarations(authoritativeModel);
  const declaredNames = new Set();
  for (const declaration of declarations) {
    if (declaredNames.has(declaration.term)) add("declaration-duplicate", declaration.term);
    declaredNames.add(declaration.term);
    const entry = termByName.get(declaration.term);
    if (!entry) add("declaration-unindexed", declaration.term);
    else if (entry.kind !== declaration.kind) add("declaration-kind-mismatch", `${declaration.term}: ${entry.kind} != ${declaration.kind}`);
  }

  const rules = manifest.mandatoryTraceability?.ruleIds ?? [];
  const manifestRules = manifest.terms.filter(entry => entry.kind === "rule").map(entry => entry.term);
  if (new Set(rules).size !== 16 || rules.length !== 16) add("rule-inventory-cardinality", `${rules.length} rules`);
  if (JSON.stringify([...rules].sort()) !== JSON.stringify([...manifestRules].sort())) add("rule-inventory-mismatch", "mandatory and canonical rule sets differ");

  for (let chapter = 1; chapter <= 50; chapter += 1) {
    const prefix = `chapter-${String(chapter).padStart(2, "0")}-`;
    if (![...anchorCounts.keys()].some(anchor => anchor.startsWith(prefix))) add("chapter-target-missing", String(chapter));
  }
  for (const target of ["reading-path-learn-quint", "reading-path-understand-combat", "reading-path-review-traceability"])
    if ((anchorCounts.get(target) ?? 0) !== 1) add("reading-path-missing", target);

  const controlled = manifest.terms.map(entry => ({ label: entry.term, anchor: entry.anchor, symbol: new Set(["module", "type", "variant", "constant", "value", "function", "variable", "action", "property", "run", "catalogue property"]).has(entry.kind) }))
    .sort((left, right) => right.label.length - left.label.length);
  for (const block of ast.children) {
    if (block.type !== "prose" || block.inIndex) continue;
    for (const node of block.children) {
      if (node.type === "link") {
        continue;
      }
      if (node.type !== "text") continue;
      for (const term of controlled) {
        const searched = term.symbol && node.type === "text" ? null : term.label;
        if (searched && occurrencePattern(searched).test(node.value)) add("controlled-occurrence-unlinked", `line ${block.line}: ${term.label}`);
      }
    }
  }
  return errors.filter((error, index, all) => all.findIndex(other => other.code === error.code && other.detail === error.detail) === index);
}

const handbook = fs.readFileSync(handbookPath, "utf8");
const positive = audit(handbook);
if (positive.length) {
  console.error(positive.map(error => `${error.code}: ${error.detail}`).join("\n"));
  process.exit(1);
}

const first = manifest.terms[0];
const mutations = [
  ["missing-fragment", handbook.replace("#reading-paths", "#missing-reading-path")],
  ["duplicate-anchor", `${handbook}\n<a id="handbook-top"></a>\n`],
  ["index-entry-missing", handbook.replace(`<a id="${first.anchor}"></a>\n**${first.term}**`, `**${first.term}**`)],
  ["controlled-occurrence-unlinked", handbook.replace('<a id="chapter-50-', 'A counterexample is deliberately unlinked.\n\n<a id="chapter-50-')]
];
for (const [detector, mutated] of mutations) {
  const observed = audit(mutated);
  if (!observed.some(error => error.code === detector)) {
    console.error(`negative control did not observe ${detector}`);
    process.exit(1);
  }
  if (audit(handbook).length) {
    console.error(`untouched handbook did not restore green after ${detector}`);
    process.exit(1);
  }
  console.log(`observed red/restored green: ${detector}`);
}

console.log(`handbook AST audit passed: ${manifest.terms.length} definitions, ${(manifest.aliases ?? []).length} aliases, ${topLevelDeclarations(authoritativeModel).length} declarations, 16 rules, 50 chapters`);
