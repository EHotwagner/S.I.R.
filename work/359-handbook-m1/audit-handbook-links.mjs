#!/usr/bin/env node

import fs from "node:fs";
import path from "node:path";

const root = process.cwd();
const handbookPath = path.join(root, "docs/sir-combat-quint-handbook.md");
const manifestPath = path.join(root, "docs/sir-combat-quint-vocabulary.json");

function structuralView(markdown) {
  const lines = markdown.replace(/\r\n/g, "\n").split("\n");
  let frontMatter = lines[0] === "---";
  let fence = false;
  let inIndex = false;
  return lines.map((line, index) => {
    if (frontMatter) {
      if (index > 0 && line === "---") frontMatter = false;
      return "";
    }
    if (/^\s*```/.test(line)) { fence = !fence; return ""; }
    if (fence) return "";
    if (/<a id="chapter-50-/.test(line)) inIndex = true;
    if (inIndex || /^\s*#/.test(line) || /^\s*<a id=/.test(line)) return "";
    return line;
  }).join("\n");
}

function audit(markdown, manifest) {
  const errors = [];
  const anchors = [...markdown.matchAll(/<a\s+id="([a-z0-9-]+)"\s*><\/a>/g)].map(match => match[1]);
  const counts = new Map();
  for (const anchor of anchors) counts.set(anchor, (counts.get(anchor) ?? 0) + 1);
  for (const [anchor, count] of counts) if (count !== 1) errors.push(`duplicate anchor: ${anchor}`);

  const destinations = [...markdown.matchAll(/\[[^\]]+\]\(#([a-z0-9-]+)\)/g)].map(match => match[1]);
  for (const destination of destinations) if (!counts.has(destination)) errors.push(`missing fragment: ${destination}`);

  for (let n = 1; n <= 50; n += 1) {
    const prefix = `chapter-${String(n).padStart(2, "0")}-`;
    if (!anchors.some(anchor => anchor.startsWith(prefix))) errors.push(`missing chapter anchor: ${n}`);
  }
  for (const anchor of ["reading-path-learn-quint", "reading-path-understand-combat", "reading-path-review-traceability"])
    if (!counts.has(anchor)) errors.push(`missing reading path: ${anchor}`);

  const indexStart = markdown.indexOf('<a id="chapter-50-');
  const index = indexStart >= 0 ? markdown.slice(indexStart) : "";
  for (const entry of manifest.terms) {
    if (!counts.has(entry.anchor)) errors.push(`manifest anchor absent: ${entry.anchor}`);
    if (!index.includes(`<a id="${entry.anchor}"></a>${entry.term}`)) errors.push(`index entry absent: ${entry.term}`);
  }

  const matrixStart = markdown.indexOf('<a id="chapter-46-');
  const matrixEnd = markdown.indexOf('<a id="chapter-47-', matrixStart);
  const matrix = matrixStart >= 0 && matrixEnd > matrixStart ? markdown.slice(matrixStart, matrixEnd) : "";
  for (const id of manifest.mandatoryTraceability.ruleIds) if (!matrix.includes(`[${id}](#rule-${id.toLowerCase()})`)) errors.push(`rule row absent: ${id}`);
  for (const id of manifest.mandatoryTraceability.decisions) if (!matrix.includes(`| Q4 ${id} |`)) errors.push(`decision row absent: ${id}`);
  for (const item of manifest.mandatoryTraceability.coverage)
    if (!matrix.includes(`| ${item} |`) && !matrix.includes(`| [${item}](`)) errors.push(`coverage row absent: ${item}`);

  const prose = structuralView(markdown).replace(/\[[^\]]+\]\([^\)]+\)/g, "");
  for (const entry of [...manifest.terms].sort((a, b) => b.term.length - a.term.length)) {
    const symbolKinds = new Set(["module", "type", "variant", "constant", "value", "function", "variable", "action", "property", "run", "catalogue property"]);
    const searchedTerm = symbolKinds.has(entry.kind) ? `\`${entry.term}\`` : entry.term;
    const escaped = searchedTerm.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const boundary = /^[A-Za-z0-9_]/.test(searchedTerm) && /[A-Za-z0-9_]$/.test(searchedTerm)
      ? new RegExp(`(^|[^A-Za-z0-9_])${escaped}([^A-Za-z0-9_]|$)`, "m")
      : new RegExp(escaped, "m");
    if (boundary.test(prose)) errors.push(`unlinked controlled occurrence: ${entry.term}`);
  }
  return [...new Set(errors)];
}

const markdown = fs.readFileSync(handbookPath, "utf8");
const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
const positive = audit(markdown, manifest);
if (positive.length) {
  console.error(positive.join("\n"));
  process.exit(1);
}

const mutations = [
  ["missing fragment", markdown.replace("#reading-paths", "#missing-reading-path")],
  ["duplicate anchor", `${markdown}\n<a id="handbook-top"></a>\n`],
  ["absent index entry", markdown.replace(`<a id="${manifest.terms[0].anchor}"></a>${manifest.terms[0].term}`, manifest.terms[0].term)],
  ["unlinked controlled occurrence", markdown.replace('<a id="chapter-50-', 'A counterexample is deliberately unlinked.\n\n<a id="chapter-50-')]
];
for (const [name, mutated] of mutations) {
  if (!audit(mutated, manifest).some(error => error.startsWith(name === "absent index entry" ? "manifest anchor absent" : name))) {
    console.error(`negative control was not detected: ${name}`);
    process.exit(1);
  }
}

console.log(`handbook audit passed: ${manifest.terms.length} definitions, ${manifest.mandatoryTraceability.ruleIds.length} rules, 50 chapters, 4 negative controls`);
