#!/usr/bin/env node

import fs from "node:fs";
import path from "node:path";

const root = process.cwd();
const handbookPath = path.join(root, "docs/sir-combat-quint-handbook.md");
const manifestPath = path.join(root, "docs/sir-combat-quint-vocabulary.json");
const modelPath = path.join(root, "docs/rules/sir-combat.md");
const registryPath = path.join(root, "src/SIR.Simulation/CombatRules.fs");
const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
const authoritativeModel = fs.readFileSync(modelPath, "utf8");
const authoritativeRegistry = fs.readFileSync(registryPath, "utf8");

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
  let inNavigation = false;
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
    if (/<a id="table-of-contents"/.test(line)) inNavigation = true;
    if (/<a id="reading-paths"/.test(line)) inNavigation = false;
    if (/<a id="chapter-50-/.test(line)) inIndex = true;
    const heading = line.match(/^(#{1,6})\s+(.+)$/);
    if (heading) {
      children.push({ type: "heading", depth: heading[1].length, line: index + 1, value: heading[2], anchor: slug(heading[2]), inIndex, inNavigation });
      continue;
    }
    const anchor = line.match(/^\s*<a\s+id="([a-z0-9-]+)"\s*><\/a>\s*$/);
    if (anchor) {
      children.push({ type: "anchor", line: index + 1, anchor: anchor[1], inIndex, inNavigation });
      continue;
    }
    children.push({ type: "prose", line: index + 1, inIndex, inNavigation, children: inlineNodes(line) });
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

function registryRuleIds(registrySource) {
  const definitions = registrySource.slice(0, registrySource.indexOf("    let registry ="));
  return [...definitions.matchAll(/(?:metadata|transitionRule)\s*\n\s*"((?:CONTENT|COMBAT)-[A-Z0-9-]+-\d{3})"/g)].map(match => match[1]);
}

function occurrencePattern(term) {
  const escaped = term.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  return /^[A-Za-z0-9_]/.test(term) && /[A-Za-z0-9_]$/.test(term)
    ? new RegExp(`(^|[^A-Za-z0-9_])${escaped}([^A-Za-z0-9_]|$)`, "g")
    : new RegExp(escaped, "g");
}

export function audit(markdown, modelMarkdown = authoritativeModel, inventory = manifest, registrySource = authoritativeRegistry) {
  const errors = [];
  const add = (code, detail) => errors.push({ code, detail });
  if (inventory.schemaVersion !== 2) add("manifest-schema", `expected schemaVersion 2, got ${inventory.schemaVersion}`);
  if (inventory.anchorContract !== "semantic-explicit-v2") add("manifest-anchor-contract", "semantic-explicit-v2 is required");
  if (inventory.inventoryContract !== "literate-declarations-rules-chapters-index-v1") add("manifest-inventory-contract", "inventory contract is absent");
  for (const region of ["front-matter", "fenced-code", "headings", "inline-code", "canonical-index"])
    if (!inventory.exemptRegions?.includes(region)) add("manifest-exemption-missing", region);

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
  for (const entry of inventory.terms ?? []) {
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
  for (const entry of inventory.terms ?? []) {
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
    const description = body.slice(0, body.indexOf("**Declared at:**")).trim();
    if (description.length < 35 || description.split(/\s+/).length < 6 || /TODO|TBD|placeholder/i.test(description) || /^The (function|value|type|run|property|action|module|variable|constant) for /i.test(description) || /declared authoritatively as/i.test(description))
      add("index-definition-insubstantial", entry.term);
    if (body.includes(`\`${entry.term}.${entry.term}\``)) add("index-declaration-locus-invalid", entry.term);
    if (!/\[[^\]]+\]\(#[a-z0-9-]+\)/.test(body.slice(body.indexOf("**Related terms:**")))) add("index-related-link-missing", entry.term);
  }
  if (indexedAnchors.size !== inventory.terms.length) add("index-cardinality", `${indexedAnchors.size} entries for ${inventory.terms.length} terms`);
  const expectedOrder = [...inventory.terms].sort((left, right) => left.term.toLowerCase().localeCompare(right.term.toLowerCase()) || left.term.localeCompare(right.term)).map(entry => entry.term);
  const actualOrder = indexEntries.map(match => match[2]);
  if (JSON.stringify(expectedOrder) !== JSON.stringify(actualOrder)) add("index-order", "canonical entries are not alphabetic");

  const aliasNames = new Set();
  for (const alias of inventory.aliases ?? []) {
    if (aliasNames.has(alias.alias)) add("alias-duplicate", alias.alias);
    aliasNames.add(alias.alias);
    const canonical = termByName.get(alias.canonicalTerm);
    if (!canonical || canonical.anchor !== alias.anchor) add("alias-unresolved", `${alias.alias} -> ${alias.canonicalTerm}#${alias.anchor}`);
    if (alias.occurrencePolicy !== "canonical-index-only") add("alias-policy-unsupported", `${alias.alias}: ${alias.occurrencePolicy}`);
    const match = indexedAnchors.get(alias.anchor);
    if (!match?.[4].includes(`\`${alias.alias}\``)) add("alias-index-missing", alias.alias);
  }

  const publishedAliases = indexEntries.flatMap(match => {
    const aliases = match[4].match(/\*\*Aliases:\*\* ([^.]+)\./)?.[1] ?? "";
    return [...aliases.matchAll(/`([^`]+)`/g)].map(item => ({ alias: item[1], anchor: match[1] }));
  });
  const expectedAliases = (inventory.aliases ?? []).map(alias => `${alias.alias}#${alias.anchor}`).sort();
  const actualAliases = publishedAliases.map(alias => `${alias.alias}#${alias.anchor}`).sort();
  if (expectedAliases.length !== 5) add("alias-inventory-cardinality", `${expectedAliases.length} aliases, expected 5`);
  if (JSON.stringify(expectedAliases) !== JSON.stringify(actualAliases)) add("alias-inventory-mismatch", "manifest aliases and published index markers differ");

  const declarations = topLevelDeclarations(modelMarkdown);
  const declaredNames = new Set();
  for (const declaration of declarations) {
    if (declaredNames.has(declaration.term)) add("declaration-duplicate", declaration.term);
    declaredNames.add(declaration.term);
    const entry = termByName.get(declaration.term);
    if (!entry) add("declaration-unindexed", declaration.term);
    else if (entry.kind !== declaration.kind) add("declaration-kind-mismatch", `${declaration.term}: ${entry.kind} != ${declaration.kind}`);
  }
  if (declarations.length !== 74) add("declaration-inventory-cardinality", `${declarations.length} declarations, expected 74`);
  const indexedDeclarations = inventory.terms.filter(entry => ["module", "type", "constant", "value", "function", "variable", "action", "property", "run"].includes(entry.kind)).map(entry => `${entry.term}#${entry.kind}`).sort();
  const authoritativeDeclarations = declarations.map(entry => `${entry.term}#${entry.kind}`).sort();
  if (JSON.stringify(indexedDeclarations) !== JSON.stringify(authoritativeDeclarations)) add("declaration-inventory-mismatch", "manifest declaration entries and authoritative model declarations differ");

  const rules = inventory.mandatoryTraceability?.ruleIds ?? [];
  const manifestRules = inventory.terms.filter(entry => entry.kind === "rule").map(entry => entry.term);
  const catalogue = modelMarkdown.slice(modelMarkdown.indexOf("  pure val ruleCatalogue ="), modelMarkdown.indexOf("  pure val traceAlgorithm ="));
  const modelRuleIds = [...catalogue.matchAll(/\{\s*id:\s*"((?:CONTENT|COMBAT)-[A-Z0-9-]+-\d{3})"/g)].map(match => match[1]);
  const registryIds = [...new Set(registryRuleIds(registrySource))];
  if (new Set(rules).size !== 16 || rules.length !== 16) add("rule-inventory-cardinality", `${rules.length} rules`);
  if (JSON.stringify([...rules].sort()) !== JSON.stringify([...manifestRules].sort())) add("rule-inventory-mismatch", "mandatory and canonical rule sets differ");
  if (new Set(modelRuleIds).size !== 16 || modelRuleIds.length !== 16) add("model-rule-inventory-cardinality", `${modelRuleIds.length} model rules`);
  if (registryIds.length !== 16) add("registry-rule-inventory-cardinality", `${registryIds.length} registry rules`);
  if (JSON.stringify([...rules].sort()) !== JSON.stringify([...modelRuleIds].sort()) || JSON.stringify([...rules].sort()) !== JSON.stringify([...registryIds].sort()))
    add("authoritative-rule-inventory-mismatch", "manifest, Quint catalogue, and runtime registry rule IDs differ");

  for (let chapter = 1; chapter <= 50; chapter += 1) {
    const prefix = `chapter-${String(chapter).padStart(2, "0")}-`;
    if (![...anchorCounts.keys()].some(anchor => anchor.startsWith(prefix))) add("chapter-target-missing", String(chapter));
  }
  for (const target of ["reading-path-learn-quint", "reading-path-understand-combat", "reading-path-review-traceability"])
    if ((anchorCounts.get(target) ?? 0) !== 1) add("reading-path-missing", target);

  for (const entry of inventory.terms)
    if (entry.occurrencePolicy && entry.occurrencePolicy !== "canonical-index-only") add("term-policy-unsupported", `${entry.term}: ${entry.occurrencePolicy}`);
  const controlled = inventory.terms.filter(entry => entry.occurrencePolicy !== "canonical-index-only").map(entry => ({ label: entry.term, anchor: entry.anchor }))
    .sort((left, right) => right.label.length - left.label.length);
  const exactTargets = new Map([...inventory.terms.map(entry => [entry.term, entry.anchor]), ...(inventory.aliases ?? []).map(alias => [alias.alias, alias.anchor])]);
  const foldedTargets = new Map();
  for (const [label, anchor] of exactTargets) {
    const key = label.toLowerCase();
    if (!foldedTargets.has(key)) foldedTargets.set(key, anchor);
    else if (foldedTargets.get(key) !== anchor) foldedTargets.set(key, null);
  }
  for (const block of ast.children) {
    if (block.type !== "prose" || block.inIndex || block.inNavigation) continue;
    for (const node of block.children) {
      if (node.type === "link") {
        const label = node.label.replace(/[`*_]/g, "").trim();
        const target = exactTargets.get(label) ?? foldedTargets.get(label.toLowerCase());
        if (target && node.destination !== `#${target}`) add("controlled-occurrence-wrong-target", `line ${block.line}: ${label} -> ${node.destination}, expected #${target}`);
        continue;
      }
      if (node.type !== "text") continue;
      for (const term of controlled) {
        if (occurrencePattern(term.label).test(node.value)) add("controlled-occurrence-unlinked", `line ${block.line}: ${term.label}`);
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
  ["missing-fragment", "missing-fragment", handbook.replace("#reading-paths", "#missing-reading-path")],
  ["duplicate-anchor", "duplicate-anchor", `${handbook}\n<a id="handbook-top"></a>\n`],
  ["index-entry-missing", "index-entry-missing", handbook.replace(`<a id="${first.anchor}"></a>\n**${first.term}**`, `**${first.term}**`)],
  ["controlled-occurrence-unlinked", "controlled-occurrence-unlinked", handbook.replace('<a id="chapter-50-', 'A counterexample is deliberately unlinked.\n\n<a id="chapter-50-')],
  ["controlled-symbol-occurrence-unlinked", "controlled-occurrence-unlinked", handbook.replace('<a id="chapter-50-', 'CombatState is deliberately unlinked.\n\n<a id="chapter-50-')],
  ["controlled-occurrence-wrong-target", "controlled-occurrence-wrong-target", handbook.replace("[CombatState](#qnt-combat-state)", "[CombatState](#chapter-20-variables-initialization-and-cohesive-combat-sta)")],
  ["index-definition-insubstantial", "index-definition-insubstantial", handbook.replace(/(\*\*absolute\*\* — function\. )[\s\S]*?( \*\*Declared at:\*\*)/, "$1TODO.$2")],
  ["authoritative-declaration-removed", "declaration-inventory-cardinality", handbook, authoritativeModel.replace(/^  run damageRoundingPreservesInt32Wrap.*\n/m, "")],
  ["authoritative-rule-id-drift", "authoritative-rule-inventory-mismatch", handbook, authoritativeModel.replace("CONTENT-WEAPON-RIFLE-001", "CONTENT-WEAPON-RIFLE-DRIFT")],
  ["manifest-alias-removed", "alias-inventory-cardinality", handbook, authoritativeModel, { ...manifest, aliases: manifest.aliases.slice(1) }]
];
for (const [control, detector, mutated, model = authoritativeModel, inventory = manifest] of mutations) {
  const observed = audit(mutated, model, inventory);
  if (!observed.some(error => error.code === detector)) {
    console.error(`negative control did not observe ${detector}`);
    process.exit(1);
  }
  if (audit(handbook).length) {
    console.error(`untouched handbook did not restore green after ${detector}`);
    process.exit(1);
  }
  console.log(`observed red/restored green: ${control}`);
}

console.log(`handbook AST audit passed: ${manifest.terms.length} definitions, ${(manifest.aliases ?? []).length} aliases, ${topLevelDeclarations(authoritativeModel).length} declarations, 16 rules, 50 chapters`);
