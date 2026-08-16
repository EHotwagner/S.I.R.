import { execFileSync, spawnSync } from "node:child_process";
import { mkdtemp, mkdir, readFile, rm, writeFile } from "node:fs/promises";
import { join, resolve } from "node:path";
import { tmpdir } from "node:os";

const script = resolve(import.meta.dirname, "production-build-receipt.mjs");
const feedbackTool = resolve(import.meta.dirname, "..", ".agents", "skills", "fs-gg-feedback-report", "scripts", "feedback-tool.fsx");
const fixture = await mkdtemp(join(tmpdir(), "sir-build-receipt-"));
const run = (program, args, options = {}) => execFileSync(program, args, { cwd: fixture, encoding: "utf8", stdio: ["ignore", "pipe", "pipe"], ...options }).trim();
const receiptArgs = (mode, extra = []) => [script, mode, "--root", fixture, "--owner-command", "fixture-owner", "--input", "input.txt", "--input", "package-lock.json", "--input", ".config/dotnet-tools.json", "--output", "fixture=output", ...extra];
const expectRed = (subject, extra = []) => {
  const result = spawnSync(process.execPath, receiptArgs("verify", ["--receipt", receipt, ...extra]), { cwd: fixture, encoding: "utf8" });
  if (result.status === 0 || !`${result.stdout}\n${result.stderr}`.includes(subject)) throw new Error(`expected ${subject} red, got ${result.status}: ${result.stdout} ${result.stderr}`);
};

let receipt;
try {
  await mkdir(join(fixture, ".config"), { recursive: true });
  await mkdir(join(fixture, "output"), { recursive: true });
  await writeFile(join(fixture, "input.txt"), "input-v1\n");
  await writeFile(join(fixture, "output", "bundle.js"), "bundle-v1\n");
  await writeFile(join(fixture, ".gitignore"), "output/\nreceipts/\n");
  await writeFile(join(fixture, "package-lock.json"), JSON.stringify({ packages: { "node_modules/vite": { version: "8.1.5" } } }));
  await writeFile(join(fixture, ".config", "dotnet-tools.json"), JSON.stringify({ tools: { fable: { version: "5.13.0" }, "fsdocs-tool": { version: "21.0.0" } } }));
  run("git", ["init", "-q"]);
  run("git", ["config", "user.email", "receipt@example.invalid"]);
  run("git", ["config", "user.name", "Receipt Fixture"]);
  run("git", ["add", "."]);
  run("git", ["commit", "-qm", "fixture"]);

  const created = JSON.parse(run(process.execPath, receiptArgs("create", ["--receipt-directory", "receipts"])));
  receipt = created.receipt;
  JSON.parse(run(process.execPath, receiptArgs("verify", ["--receipt", receipt])));
  JSON.parse(run(process.execPath, receiptArgs("mutate-stale-reuse", ["--receipt", receipt, "--mutation-output-id", "fixture"])));
  JSON.parse(run(process.execPath, receiptArgs("mutate-missing-reuse", ["--receipt", receipt, "--mutation-output-id", "fixture"])));
  if (await readFile(join(fixture, "output", "bundle.js"), "utf8") !== "bundle-v1\n") throw new Error("mutation subject was not restored");
  run("dotnet", ["fsi", feedbackTool, "--", "validate-focused-receipt", "--root", fixture, "--receipt", receipt, "--owner-command", "fixture-owner"]);

  await writeFile(join(fixture, "output", "bundle.js"), "bundle-v2\n");
  expectRed("output-identity-drift");
  await writeFile(join(fixture, "output", "bundle.js"), "bundle-v1\n");

  await writeFile(join(fixture, "input.txt"), "input-v2\n");
  expectRed("dirty-tracked-state:input.txt");
  run("git", ["restore", "input.txt"]);

  const originalReceipt = await readFile(join(fixture, receipt), "utf8");
  const tampered = join(fixture, "receipts", receipt.split("/").at(-1));
  await writeFile(tampered, originalReceipt.replace('"result": "pass"', '"result": "fail"'));
  expectRed("receipt-content-address-drift");
  await writeFile(tampered, originalReceipt);

  await mkdir(join(fixture, "feedback"));
  await writeFile(join(fixture, "feedback", "note.md"), "metadata\n");
  run("git", ["add", "feedback/note.md"]);
  run("git", ["commit", "-qm", "metadata"]);
  expectRed("source-revision-drift");
  JSON.parse(run(process.execPath, receiptArgs("verify", ["--receipt", receipt, "--allow-metadata-only", "true"])));

  await mkdir(join(fixture, "scripts"));
  await writeFile(join(fixture, "scripts", "change.sh"), "production\n");
  run("git", ["add", "scripts/change.sh"]);
  run("git", ["commit", "-qm", "production drift"]);
  expectRed("metadata-only-drift:scripts/change.sh", ["--allow-metadata-only", "true"]);

  console.log("production build receipt focused qualification passed: canonical create/verify, stale/missing restoration, output/input/content-address/revision drift, and metadata-only reuse remain fail closed");
} finally {
  await rm(fixture, { recursive: true, force: true });
}
