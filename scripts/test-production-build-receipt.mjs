import { execFileSync, spawnSync } from "node:child_process";
import { chmod, cp, mkdtemp, mkdir, readFile, rm, stat, writeFile } from "node:fs/promises";
import { join, resolve } from "node:path";
import { tmpdir } from "node:os";

const script = resolve(import.meta.dirname, "production-build-receipt.mjs");
const feedbackTool = resolve(import.meta.dirname, "..", ".agents", "skills", "fs-gg-feedback-report", "scripts", "feedback-tool.fsx");
const ciRoute = resolve(import.meta.dirname, "ci-route.mjs");
const artifactManifest = resolve(import.meta.dirname, "ci-artifact-manifest.mjs");
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
  await chmod(join(fixture, "output", "bundle.js"), 0o755);
  await writeFile(join(fixture, ".gitignore"), "output/\nlate-output/\nreceipts/\n");
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
  const commit = run("git", ["rev-parse", "HEAD"]);
  const tree = run("git", ["rev-parse", "HEAD^{tree}"]);
  run(process.execPath, [ciRoute, "route", "--path", "scripts/example.sh", "--commit", commit, "--tree", tree, "--output", "route.json"]);
  run("tar", ["--sort=name", "--mtime=@0", "--owner=0", "--group=0", "--numeric-owner", "-cf", "prepared.tar", "output", receipt]);
  const manifestCreated = JSON.parse(run(process.execPath, [artifactManifest, "create", "--root", fixture, "--route", "route.json", "--build-receipt", receipt, "--archive", "prepared.tar", "--directory", "manifests"]));
  JSON.parse(run(process.execPath, [artifactManifest, "verify-transport", "--root", fixture, "--route", "route.json", "--archive", "prepared.tar", "--manifest", manifestCreated.manifest]));
  await mkdir(join(fixture, "extracted"));
  run("tar", ["-xf", "../prepared.tar"], { cwd: join(fixture, "extracted") });
  JSON.parse(run(process.execPath, [artifactManifest, "verify-staged", "--root", fixture, "--build-receipt", receipt, "--manifest", manifestCreated.manifest, "--stage", "extracted"]));
  if (((await stat(join(fixture, "extracted", "output", "bundle.js"))).mode & 0o777) !== 0o755) throw new Error("prepared transport did not preserve executable mode");
  await writeFile(join(fixture, "extracted", "output", "bundle.js"), "mutated consumer copy\n");
  const stagedMutation = spawnSync(process.execPath, [artifactManifest, "verify-staged", "--root", fixture, "--build-receipt", receipt, "--manifest", manifestCreated.manifest, "--stage", "extracted"], { cwd: fixture, encoding: "utf8" });
  if (stagedMutation.status === 0 || !stagedMutation.stderr.includes("staged-output-identity-drift")) throw new Error("mutated staged receipt subject was accepted");
  await rm(join(fixture, "extracted"), { recursive: true, force: true });
  await mkdir(join(fixture, "extracted"));
  run("tar", ["-xf", "../prepared.tar"], { cwd: join(fixture, "extracted") });
  await cp(join(fixture, "extracted", "output"), join(fixture, "consumer-output"), { recursive: true });
  await writeFile(join(fixture, "consumer-output", "bundle.js"), "consumer mutation\n");
  JSON.parse(run(process.execPath, [artifactManifest, "verify-staged", "--root", fixture, "--build-receipt", receipt, "--manifest", manifestCreated.manifest, "--stage", "extracted"]));
  await writeFile(join(fixture, "prepared.tar"), "tampered", { flag: "a" });
  const tamperedTransport = spawnSync(process.execPath, [artifactManifest, "verify-transport", "--root", fixture, "--route", "route.json", "--archive", "prepared.tar", "--manifest", manifestCreated.manifest], { cwd: fixture, encoding: "utf8" });
  if (tamperedTransport.status === 0 || !tamperedTransport.stderr.includes("transport-identity-drift")) throw new Error("tampered prepared transport was accepted");

  await mkdir(join(fixture, "late-output"));
  await writeFile(join(fixture, "late-output", "runner"), "late-created\n");
  await chmod(join(fixture, "late-output", "runner"), 0o755);
  const lateCreated = JSON.parse(run(process.execPath, receiptArgs("create", ["--output", "late=late-output", "--receipt-directory", "receipts"])));
  const lateReceipt = JSON.parse(await readFile(join(fixture, lateCreated.receipt), "utf8"));
  run("tar", ["--sort=name", "--mtime=@0", "--owner=0", "--group=0", "--numeric-owner", "-cf", "late-prepared.tar", ...lateReceipt.outputs.map(({ path }) => path), lateCreated.receipt]);
  await rm(join(fixture, "output"), { recursive: true, force: true });
  await rm(join(fixture, "late-output"), { recursive: true, force: true });
  run("tar", ["-xf", "late-prepared.tar"]);
  JSON.parse(run(process.execPath, receiptArgs("verify", ["--output", "late=late-output", "--receipt", lateCreated.receipt])));
  if (((await stat(join(fixture, "late-output", "runner"))).mode & 0o777) !== 0o755) throw new Error("late-created executable mode was not transported");
  run("tar", ["--sort=name", "--mtime=@0", "--owner=0", "--group=0", "--numeric-owner", "-cf", "missing-late.tar", "output", lateCreated.receipt]);
  await rm(join(fixture, "output"), { recursive: true, force: true });
  await rm(join(fixture, "late-output"), { recursive: true, force: true });
  run("tar", ["-xf", "missing-late.tar"]);
  const missingLate = spawnSync(process.execPath, receiptArgs("verify", ["--output", "late=late-output", "--receipt", lateCreated.receipt]), { cwd: fixture, encoding: "utf8" });
  if (missingLate.status === 0 || !missingLate.stderr.includes("missing-output:late-output")) throw new Error("transport missing a late-created receipt output was accepted");
  run("tar", ["-xf", "late-prepared.tar"]);
  JSON.parse(run(process.execPath, receiptArgs("mutate-stale-reuse", ["--receipt", receipt, "--mutation-output-id", "fixture"])));
  JSON.parse(run(process.execPath, receiptArgs("mutate-missing-reuse", ["--receipt", receipt, "--mutation-output-id", "fixture"])));
  if (await readFile(join(fixture, "output", "bundle.js"), "utf8") !== "bundle-v1\n") throw new Error("mutation subject was not restored");
  await mkdir(join(fixture, "mutable-obj"));
  await writeFile(join(fixture, "mutable-obj", "project.assets.json"), "before\n");
  await writeFile(join(fixture, "mutable-obj", "project.assets.json"), "after consumer restore\n");
  JSON.parse(run(process.execPath, receiptArgs("verify", ["--receipt", receipt])));
  run("dotnet", ["fsi", feedbackTool, "--", "validate-focused-receipt", "--root", fixture, "--receipt", receipt, "--owner-command", "fixture-owner"]);

  await writeFile(join(fixture, "output", "bundle.js"), "bundle-v2\n");
  expectRed("output-identity-drift");
  await writeFile(join(fixture, "output", "bundle.js"), "bundle-v1\n");
  await chmod(join(fixture, "output", "bundle.js"), 0o755);

  await chmod(join(fixture, "output", "bundle.js"), 0o644);
  expectRed("output-identity-drift");
  await chmod(join(fixture, "output", "bundle.js"), 0o755);

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

  console.log("production build receipt focused qualification passed: canonical create/verify, immutable output transport/mode, mutable intermediates excluded, omitted/tampered transport, stale/missing restoration, identity/revision drift, and metadata-only reuse remain fail closed");
} finally {
  await rm(fixture, { recursive: true, force: true });
}
