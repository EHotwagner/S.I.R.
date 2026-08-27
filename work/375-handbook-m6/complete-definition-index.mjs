#!/usr/bin/env node

import fs from "node:fs";
import path from "node:path";

const root = process.cwd();
const handbookPath = path.join(root, "docs/sir-combat-quint-handbook.md");
const manifestPath = path.join(root, "docs/sir-combat-quint-vocabulary.json");
const modelPath = path.join(root, "docs/rules/sir-combat.md");
const write = process.argv.includes("--write");

const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
const modelMarkdown = fs.readFileSync(modelPath, "utf8");
const quint = [...modelMarkdown.matchAll(/```quint[^\n]*\n([\s\S]*?)\n```/g)].map(match => match[1]).join("\n");
const declarations = new Map();
let currentModule = "";
for (const line of quint.split("\n")) {
  const moduleMatch = line.match(/^module\s+(\w+)/);
  if (moduleMatch) {
    currentModule = moduleMatch[1];
    declarations.set(moduleMatch[1], { module: currentModule, source: line.trim() });
  }
  const declarationMatch = line.match(/^  (?:pure\s+)?(?:type|val|def|action|var|run|invariant)\s+(\w+)/);
  if (declarationMatch) declarations.set(declarationMatch[1], { module: currentModule, source: line.trim().replace(/\s*=\s*$/, "") });
}

const specific = {
  "action": "A Quint declaration whose guarded execution updates one or more primed state variables atomically.",
  "aggregate attack resolution": "One atomic transition that computes and publishes the completed damage, health, wound, incapacity, suppression, and explanation consequences of an attack.",
  "bounded verification": "Exhaustive exploration of every behavior inside explicitly chosen finite bounds; it proves only the bounded state space.",
  "cells": "Discrete map-distance units used by engagement preparation and the external trace contract.",
  "collateral consequence": "The ordinary completed combat consequence of an attack whose source and target share a faction; it is not silently suppressed.",
  "constant": "A Quint name bound once by the model and unavailable for transition-time reassignment.",
  "counterexample": "A concrete state/action path returned when a checked property is false inside the explored boundary.",
  "cover blocking": "Whether intact cover stops the current or a later projectile, kept distinct from cover integrity.",
  "cover damage": "The bounded integrity loss applied to cover by one impact, with a minimum of one point.",
  "cover integrity": "A bounded 0–100 durability value; reaching zero makes cover non-blocking for future projectiles.",
  "current-collision consumption": "The rule that a projectile which destroys cover is still stopped by that same collision even though later projectiles may pass.",
  "damage": "Whole combat harm after trace, retention, fixed-point composition, and final rounding.",
  "damage points": "Whole-number units used for weapon output and completed attack harm.",
  "destroyed cover": "Cover whose integrity is zero and whose future blocking flag is therefore false.",
  "event identity": "The stable identifier carried through an input and observation so replay can match one completed event.",
  "exhaustive check": "A property evaluation over every reachable state within declared finite bounds, unlike a sampled execution.",
  "explanation order": "The deterministic ordered rule-ID list explaining which stable rules contributed to an observation.",
  "external algorithm contract": "A modeled input/output boundary whose implementation is owned outside this Quint model and needs separate evidence.",
  "faction-neutral consequence": "The rule that allied and opposing attacks with equal physical inputs receive equal damage and suppression treatment.",
  "first collision": "The earliest blocking contact selected by the registered trace implementation for projectile resolution.",
  "fixed-point ratio": "A dimensionless ratio encoded as an integer whose denominator is `SCALE` (10,000).",
  "guard": "A Boolean precondition that must hold before a Quint action may participate in a transition.",
  "health": "A bounded 0–100 durable combat value reduced by completed damage.",
  "hit points": "Whole-number units used for actor health; zero means incapacitated in this bounded model.",
  "HP": "A conventional abbreviation for hit points and the handbook's canonical health stat.",
  "import": "A Quint declaration that brings names from another module into the current module's scope.",
  "incapacitation": "The durable state derived exactly from successor health being zero.",
  "initialization": "The action that supplies the first values for all model state variables.",
  "integrity points": "Whole-number units used for cover durability from 0 through 100.",
  "invariant": "A state predicate expected to hold in every reachable state within the checked boundary.",
  "list": "An ordered Quint collection; explanation rule IDs use a list because order is observable.",
  "module": "A named Quint namespace containing declarations and an explicit public surface.",
  "nondeterminism": "A deliberate choice among enabled actions or values; different valid traces need not be failures.",
  "penetration": "Armor interaction represented here by retained effect and completed observations rather than an invented intermediate transition.",
  "physical shot trace": "The registered geometry process that produces visible and total samples before the model consumes their ratio.",
  "preparation time": "The fixed-point engagement delay derived from range cells and the registered range slope.",
  "primed assignment": "A Quint action assignment such as `combat' = ...` that names a variable's next-state value.",
  "projectile contact": "The collision event through which trace and cover rules enter completed combat resolution.",
  "property": "A Boolean claim evaluated over model behavior; its strength depends on whether evidence is sampled or exhaustive.",
  "pure function": "A Quint definition whose result depends only on its arguments and immutable declarations, without changing model state.",
  "pure value": "An immutable Quint value computed without changing model state.",
  "range cells": "The non-negative cell distance used to derive engagement preparation time.",
  "reachable state": "A state produced from initialization by zero or more enabled transitions.",
  "record": "A Quint value with named fields, used for rules, inputs, state, and observations.",
  "registered line-of-sight implementation": "The pinned external supercover implementation that owns geometry while Quint owns only its declared sample contract.",
  "retained effect": "The clamped 0–1 fixed-point share of traced damage left after armor.",
  "run": "A named executable Quint scenario that asks for a concrete satisfying trace or value.",
  "safety property": "A property stating that an unwanted state is never reachable inside the checked boundary.",
  "sampled run": "A deterministic but non-exhaustive set of executions identified by seed, count, and step bound.",
  "samples": "Integer trace observations counted as visible and total before conversion to a fixed-point ratio.",
  "seconds": "The human-facing time unit represented by the model's fixed-point preparation value.",
  "set": "An unordered Quint collection with unique members, used for catalogue identity and dependency membership.",
  "signed 32-bit saturation": "Clamping a mathematical integer to `INT32_MIN..INT32_MAX` at the model boundaries where runtime arithmetic saturates.",
  "source digest": "A cryptographic identity of an authoritative input used to bind generated projections and evidence to exact content.",
  "state transition": "One atomic step from current variable bindings to their primed successor bindings.",
  "state variable": "A mutable Quint declaration whose current and next values define model state.",
  "stuttering": "A behavior step that leaves relevant state unchanged; it must not be confused with a completed combat action.",
  "suppression": "A bounded 0–100 durable combat pressure value changed by damaging attacks and recovery.",
  "suppression delta": "The requested or applied whole-number change in suppression for one observation.",
  "suppression eligibility": "The rule that requested suppression applies only when completed damage is positive.",
  "suppression points": "Whole-number units used for accumulated suppression from 0 through 100.",
  "suppression recovery": "The focused transition that removes at most five current suppression points.",
  "target footprint": "The Boolean input asserting that a trace intersects a valid target area before attack resolution.",
  "terminal state": "A state after which the modeled execution has no required successor; evidence must distinguish it from bounded trace termination.",
  "total samples": "The positive denominator of a valid trace ratio.",
  "type": "A Quint declaration defining the shape and allowed values of model data.",
  "variant": "One named case of a Quint sum type, such as `NoWound`, `MinorWound`, or `MajorWound`.",
  "visible samples": "The non-negative numerator of a valid trace ratio, never greater than total samples.",
  "witness": "A concrete execution demonstrating that at least one behavior or state is reachable; it is not a universal proof.",
  "wound": "The completed damage classification `NoWound`, `MinorWound`, or `MajorWound` at exact thresholds.",
  "wound threshold": "A whole-damage boundary: 25 begins a minor wound and 50 begins a major wound."
};

const relatedByKind = {
  module: [["module", "def-module"], ["type", "def-type"]],
  type: [["record", "def-record"], ["SirCombat", "qnt-sir-combat"]],
  variant: [["variant", "def-variant"], ["Wound", "qnt-wound"]],
  constant: [["constant", "def-constant"], ["scale 10,000", "unit-scale-10-000"]],
  value: [["pure value", "def-pure-value"], ["SirCombat", "qnt-sir-combat"]],
  function: [["pure function", "def-pure-function"], ["SirCombat", "qnt-sir-combat"]],
  variable: [["state variable", "def-state-variable"], ["CombatState", "qnt-combat-state"]],
  action: [["state transition", "def-state-transition"], ["CombatState", "qnt-combat-state"]],
  property: [["property", "def-property"], ["bounded verification", "def-bounded-verification"]],
  "catalogue property": [["propertyCatalogue", "qnt-property-catalogue"], ["property", "def-property"]],
  run: [["run", "def-run"], ["witness", "def-witness"]],
  keyword: [["SirCombat", "qnt-sir-combat"], ["state transition", "def-state-transition"]],
  concept: [["CombatState", "qnt-combat-state"], ["Observation", "qnt-observation"]],
  stat: [["AttackInput", "qnt-attack-input"], ["Observation", "qnt-observation"]],
  unit: [["scale 10,000", "unit-scale-10-000"], ["AttackInput", "qnt-attack-input"]],
  evidence: [["claim boundary", "def-claim-boundary"], ["property", "def-property"]]
};

function words(identifier) {
  return identifier.replace(/([a-z0-9])([A-Z])/g, "$1 $2").replace(/_/g, " ").toLowerCase();
}

function definition(entry) {
  if (specific[entry.term]) return specific[entry.term];
  const declaration = declarations.get(entry.term);
  if (declaration) {
    const source = declaration.source.replace(/`/g, "");
    const noun = entry.kind === "catalogue property" ? "catalogue entry" : entry.kind;
    return `The ${noun} for ${words(entry.term)}, declared authoritatively as \`${source}\` in the literate Quint model.`;
  }
  if (entry.kind === "variant") return `The \`${entry.term}\` case of the authoritative \`Wound\` sum type used in completed observations.`;
  if (entry.kind === "catalogue property") return `The catalogue identity for the \`${entry.term[0].toLowerCase()}${entry.term.slice(1)}\` model property, including its declared subjects.`;
  throw new Error(`no complete definition source for ${entry.term}`);
}

function declarationLocus(entry) {
  const declaration = declarations.get(entry.term);
  if (declaration) return `literate model \`${declaration.module}.${entry.term}\``;
  if (entry.kind === "catalogue property") return `\`SirCombat.propertyCatalogue\``;
  if (entry.kind === "keyword" || entry.kind === "evidence") return "handbook formal-reasoning chapters 18 and 33–45";
  return "handbook combat walkthroughs and controlled rule catalogue";
}

function runtime(entry) {
  if (entry.kind === "keyword" || entry.kind === "evidence" || entry.kind === "module" || entry.kind === "type" || entry.kind === "variant" || entry.kind === "run" || entry.kind === "property" || entry.kind === "catalogue property")
    return "model/method term, not an independent production-equivalence claim; see the chapter 38 correspondence map for any named runtime subject";
  return "scoped by the chapter 38 correspondence map and its named F# subject/evidence; missing mappings remain explicit";
}

function related(entry) {
  const candidates = relatedByKind[entry.kind] ?? [["claim boundary", "def-claim-boundary"]];
  const filtered = candidates.filter(([, anchor]) => anchor !== entry.anchor);
  return filtered.slice(0, 2).map(([label, anchor]) => `[${label}](#${anchor})`).join(", ");
}

let handbook = fs.readFileSync(handbookPath, "utf8");
for (const entry of manifest.terms) {
  const marker = `<a id="${entry.anchor}"></a>\n**${entry.term}** — ${entry.kind}.`;
  const start = handbook.indexOf(marker);
  if (start < 0) throw new Error(`index marker absent for ${entry.term}`);
  const lineStart = start + marker.indexOf("**");
  const lineEnd = handbook.indexOf("\n", lineStart);
  const current = handbook.slice(lineStart, lineEnd);
  const aliases = (manifest.aliases ?? []).filter(alias => alias.canonicalTerm === entry.term).map(alias => `\`${alias.alias}\``);
  const aliasText = aliases.length ? ` **Aliases:** ${aliases.join(", ")}.` : "";
  let replacement = current;
  if (current.includes("Planned definition") || current.includes("Pending."))
    replacement = `**${entry.term}** — ${entry.kind}. ${definition(entry)} **Declared at:** ${declarationLocus(entry)}. **Related terms:** ${related(entry)}.${aliasText} **Runtime correspondence:** ${runtime(entry)}.`;
  else if (aliasText && !current.includes("**Aliases:**"))
    replacement = current.replace(" **Runtime correspondence:**", `${aliasText} **Runtime correspondence:**`);
  if (replacement === current) continue;
  handbook = handbook.slice(0, lineStart) + replacement + handbook.slice(lineEnd);
}

if (write) {
  fs.writeFileSync(handbookPath, handbook);
  console.log(`completed ${manifest.terms.length} canonical definitions`);
} else {
  const current = fs.readFileSync(handbookPath, "utf8").replace(/\r\n/g, "\n");
  if (current !== handbook.replace(/\r\n/g, "\n")) throw new Error("definition index is not complete; run with --write");
  console.log(`definition index complete: ${manifest.terms.length} canonical definitions`);
}
