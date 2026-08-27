import { spawnSync } from "node:child_process";
import { existsSync, mkdtempSync, readFileSync, rmSync, writeFileSync, mkdirSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

const authorityPath = "docs/rules/sir-combat.md";
const handbookPath = "docs/sir-combat-quint-handbook.md";
const receiptPath = "readiness/361-handbook-m2/handbook-m2.junit.xml";
const authority = readFileSync(authorityPath, "utf8");
const handbook = readFileSync(handbookPath, "utf8");
const temporary = mkdtempSync(join(tmpdir(), "sir-handbook-m2-"));
const cases = [];

function check(name, condition, detail) {
  if (!condition) throw new Error(`${name}: ${detail}`);
  cases.push(name);
}

function run(command, args, expectedStatus = 0) {
  const result = spawnSync(command, args, { encoding: "utf8" });
  const output = `${result.stdout ?? ""}${result.stderr ?? ""}`;
  if (result.status !== expectedStatus) {
    throw new Error(`${command} ${args.join(" ")} returned ${result.status}; expected ${expectedStatus}\n${output}`);
  }
  return output;
}

function extractAuthority(markdown) {
  const blocks = [...markdown.matchAll(/^```quint sir-combat\.qnt \+=\n([\s\S]*?)^```$/gm)].map(match => match[1]);
  check("authority-has-projection", blocks.length === 2, `expected 2 additive Quint fences, found ${blocks.length}`);
  return blocks.join("");
}

try {
  const model = extractAuthority(authority);
  const shown = [...handbook.matchAll(/^```quint authority=sir-combat\n([\s\S]*?)^```$/gm)].map(match => match[1]);
  check("handbook-authority-excerpts", shown.length >= 6, `expected at least 6 authority excerpts, found ${shown.length}`);
  for (const [index, excerpt] of shown.entries()) {
    check(`excerpt-${index + 1}-exact`, model.includes(excerpt), "handbook fence is not an exact substring of the extracted authority");
  }

  for (const required of [
    "25 × 1.0 × 0.8 = 20",
    "250000 × 10000 / 10000 = 250000",
    "250000 × 8000 / 10000 = 200000",
    "(200000 + 5000) / 10000 = 20",
    "signed int32 wrap",
    "predict → [run](#def-run) → observe → explain",
    "8000 → 7000",
    "actual [damage](#stat-damage) becomes `18`",
    "does **not** by itself establish production",
  ]) {
    check(`required-text:${required}`, handbook.includes(required), `missing required teaching text: ${required}`);
  }

  if (process.argv.includes("--require-rendered")) {
    const renderedPath = "artifacts/site/sir-combat-quint-handbook.html";
    check("rendered-handbook-exists", existsSync(renderedPath), `missing strict fsdocs output: ${renderedPath}`);
    const rendered = readFileSync(renderedPath, "utf8");
    check("rendered-representative-spine", rendered.includes("predict") && rendered.includes("signed int32 wrap") && rendered.includes("yields <code>18</code>"), "rendered handbook omits a representative-spine marker");
  }

  if (process.argv.includes("--full-evidence")) {
    const linkAudit = run("node", ["work/359-handbook-m1/audit-handbook-links.mjs"]);
    check("structural-link-audit", linkAudit.includes("handbook audit passed"), "structural link/vocabulary audit did not report success");
    const q4 = run("./scripts/qualify-quint-q4-sir-combat.sh", []);
    check("full-q4-qualification", q4.includes("quint-q4-sir-combat: PASS") && q4.includes("traces=16 states=144"), "full model/runtime Q4 qualification did not report its expected scope");
  }

  const modelPath = join(temporary, "sir-combat.qnt");
  const mutatedPath = join(temporary, "sir-combat-retention-7000.qnt");
  const mutationProofPath = join(temporary, "sir-combat-retention-7000-proof.qnt");
  writeFileSync(modelPath, model);
  check("pinned-quint", run("quint", ["--version"]).trim() === "0.32.0", "expected Quint 0.32.0");
  run("quint", ["typecheck", modelPath]);
  cases.push("authoritative-typecheck");

  const testArgs = ["test", modelPath, "--main", "SirCombatTests", "--backend", "rust", "--seed", "352", "--match", "representativeDamageIsTwenty", "--verbosity", "3"];
  check("authority-green", run("quint", testArgs).includes("1 passing"), "authoritative representative witness did not pass");

  const needle = "    armorRetentionRaw: 8000,";
  check("mutation-subject-unique", model.split(needle).length === 2, "representative retention assignment was not unique");
  const mutated = model.replace(needle, "    armorRetentionRaw: 7000,");
  writeFileSync(mutatedPath, mutated);
  const red = spawnSync("quint", ["test", mutatedPath, "--main", "SirCombatTests", "--backend", "rust", "--seed", "352", "--match", "representativeDamageIsTwenty", "--verbosity", "3"], { encoding: "utf8" });
  const redOutput = `${red.stdout ?? ""}${red.stderr ?? ""}`;
  check("observed-red", red.status !== 0, "retention mutation unexpectedly passed");
  check("observed-red-named-witness", redOutput.includes("representativeDamageIsTwenty"), "mutation did not fail through the named witness");
  const mutationProof = mutated
    .replace("        last.damage == 20,", "        last.damage == 18,")
    .replace("        last.retentionRaw == 8000,", "        last.retentionRaw == 7000,")
    .replace("        combat.health == 80,", "        combat.health == 82,");
  writeFileSync(mutationProofPath, mutationProof);
  const mutationProofArgs = ["test", mutationProofPath, "--main", "SirCombatTests", "--backend", "rust", "--seed", "352", "--match", "representativeDamageIsTwenty", "--verbosity", "3"];
  check("observed-red-is-18", run("quint", mutationProofArgs).includes("1 passing"), "mutated path did not calculate damage 18, retention 7000, and health 82");

  writeFileSync(modelPath, extractAuthority(readFileSync(authorityPath, "utf8")));
  check("restored-green", run("quint", testArgs).includes("1 passing"), "untouched authority did not return green");

  mkdirSync("readiness/361-handbook-m2", { recursive: true });
  const xmlCases = cases.map(name => `  <testcase classname="SIR.HandbookM2" name="${name.replaceAll("&", "and").replaceAll('"', "'")}"/>`).join("\n");
  writeFileSync(receiptPath, `<?xml version="1.0" encoding="UTF-8"?>\n<testsuite name="sir-combat-quint-handbook-m2" tests="${cases.length}" failures="0" errors="0" skipped="0">\n${xmlCases}\n</testsuite>\n`);
  console.log(`handbook-m2: PASS (${shown.length} exact excerpts; ${cases.length} checks; observed red at damage 18; restored green)`);
} finally {
  rmSync(temporary, { recursive: true, force: true });
}
