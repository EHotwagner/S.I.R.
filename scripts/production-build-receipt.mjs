import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";
import { chmod, lstat, mkdir, readFile, readdir, unlink, writeFile } from "node:fs/promises";
import { basename, dirname, relative, resolve } from "node:path";
import { pathToFileURL } from "node:url";

export const schema = "sir.production-build-receipt/v1";
const defaultOwner = "scripts/qualify-production.sh";
const defaultReceiptDirectory = "docs/evidence/production-build-receipt-v1";
const defaultInputs = ["src", "scripts/build-client.sh", "scripts/generate-publication-manifest.mjs", "package.json", "package-lock.json", "global.json", ".config/dotnet-tools.json", "Directory.Build.props", "Directory.Packages.props", "SIR.slnx"];
const defaultOutputs = ["main-fable=src/SIR.Client.Web/.fable", "rules-fable=src/SIR.Client.Web/.fable-rules", "production-client=artifacts/client"];
const metadataOnlyPrefixes = ["feedback/", "readiness/", "work/", "docs/evidence/production-build-receipt-v1/"];
const sha256 = (bytes) => createHash("sha256").update(bytes).digest("hex");
const slash = (value) => value.replaceAll("\\", "/");

export function canonicalBytes(value) {
  return `${JSON.stringify(value, null, 2)}\n`;
}

function command(root, program, args) {
  return execFileSync(program, args, { cwd: root, encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] }).trim();
}

function git(root, ...args) {
  return command(root, "git", args);
}

function parseArguments(argv) {
  const [mode, ...tail] = argv;
  const options = new Map();
  for (let index = 0; index < tail.length; index += 2) {
    const name = tail[index];
    if (!name?.startsWith("--") || index + 1 >= tail.length) throw new Error(`production-build-receipt: malformed option ${name ?? "<missing>"}`);
    const key = name.slice(2);
    options.set(key, [...(options.get(key) ?? []), tail[index + 1]]);
  }
  const one = (name, fallback) => options.get(name)?.at(-1) ?? fallback;
  return { mode, options, one };
}

async function filesUnder(root, path) {
  const absolute = resolve(root, path);
  const information = await lstat(absolute).catch(() => undefined);
  if (!information) throw new Error(`production-build-receipt: missing-output:${slash(path)}`);
  if (information.isSymbolicLink()) throw new Error(`production-build-receipt: symbolic-output:${slash(path)}`);
  if (information.isFile()) return [absolute];
  if (!information.isDirectory()) throw new Error(`production-build-receipt: unsupported-output:${slash(path)}`);
  const output = [];
  for (const entry of await readdir(absolute, { withFileTypes: true })) {
    const child = slash(relative(root, resolve(absolute, entry.name)));
    if (entry.isSymbolicLink()) throw new Error(`production-build-receipt: symbolic-output:${child}`);
    if (entry.isDirectory()) output.push(...(await filesUnder(root, child)));
    else if (entry.isFile()) output.push(resolve(root, child));
  }
  return output.sort((left, right) => slash(relative(root, left)).localeCompare(slash(relative(root, right))));
}

async function identityForFiles(root, files) {
  const entries = [];
  for (const file of files) {
    const bytes = await readFile(file);
    const information = await lstat(file);
    entries.push({ path: slash(relative(root, file)), mode: information.mode & 0o777, bytes: bytes.byteLength, sha256: sha256(bytes) });
  }
  return { files: entries, digest: sha256(canonicalBytes(entries)) };
}

async function trackedInputs(root, declarations) {
  const tracked = git(root, "ls-files", "-z", "--", ...declarations).split("\0").filter(Boolean).sort();
  if (tracked.length === 0) throw new Error("production-build-receipt: input inventory is empty");
  return identityForFiles(root, tracked.map((path) => resolve(root, path)));
}

async function outputIdentities(root, declarations) {
  const outputs = [];
  for (const declaration of declarations) {
    const separator = declaration.indexOf("=");
    if (separator <= 0) throw new Error(`production-build-receipt: malformed output declaration ${declaration}`);
    const id = declaration.slice(0, separator);
    const path = slash(declaration.slice(separator + 1));
    const identity = await identityForFiles(root, await filesUnder(root, path));
    outputs.push({ id, path, ...identity });
  }
  return outputs.sort((left, right) => left.id.localeCompare(right.id));
}

function toolIdentities(root) {
  const packageLock = JSON.parse(execFileSync(process.execPath, ["-e", "process.stdout.write(JSON.stringify(require('./package-lock.json')))"] , { cwd: root, encoding: "utf8" }));
  const tools = JSON.parse(execFileSync(process.execPath, ["-e", "process.stdout.write(JSON.stringify(require('./.config/dotnet-tools.json')))"] , { cwd: root, encoding: "utf8" }));
  const toolVersion = (key) => tools.tools?.[key]?.version ?? "missing";
  return [
    { id: "git", version: command(root, "git", ["--version"]) },
    { id: "dotnet-sdk", version: command(root, "dotnet", ["--version"]) },
    { id: "fable", version: toolVersion("fable") },
    { id: "fsdocs", version: toolVersion("fsdocs-tool") },
    { id: "node", version: process.version },
    { id: "npm", version: command(root, "npm", ["--version"]) },
    { id: "vite", version: packageLock.packages?.["node_modules/vite"]?.version ?? "missing" },
  ];
}

function cleanTrackedState(root) {
  const changed = git(root, "diff", "--name-only", "HEAD").split("\n").filter(Boolean).map(slash);
  const disallowed = changed.filter((path) => !metadataOnlyPrefixes.some((prefix) => path.startsWith(prefix)));
  if (disallowed.length) throw new Error(`production-build-receipt: dirty-tracked-state:${disallowed[0]}`);
  return { buildInputChanges: 0, excludedGeneratedChanges: changed.sort() };
}

async function derive(root, ownerCommand, inputs, outputs) {
  const commit = git(root, "rev-parse", "HEAD");
  return {
    schema,
    result: "pass",
    ownerCommand,
    source: { commit, tree: git(root, "rev-parse", `${commit}^{tree}`), clean: cleanTrackedState(root) },
    inputs: await trackedInputs(root, inputs),
    tools: toolIdentities(root),
    outputs: await outputIdentities(root, outputs),
  };
}

function ensureShape(receipt) {
  if (receipt?.schema !== schema) throw new Error("production-build-receipt: unsupported-schema");
  if (receipt.result !== "pass") throw new Error("production-build-receipt: result-not-pass");
  if (!receipt.ownerCommand || !receipt.source?.commit || !receipt.source?.tree) throw new Error("production-build-receipt: incomplete-identity");
  if (!receipt.inputs?.files?.length || !receipt.tools?.length || !receipt.outputs?.length) throw new Error("production-build-receipt: incomplete-inventory");
}

function changedPaths(root, before, after) {
  return git(root, "diff", "--name-only", `${before}..${after}`).split("\n").filter(Boolean).map(slash);
}

function compare(receipt, current, allowMetadataOnly) {
  if (receipt.ownerCommand !== current.ownerCommand) throw new Error("production-build-receipt: owning-command-drift");
  if (receipt.source.commit !== current.source.commit) {
    if (!allowMetadataOnly) throw new Error("production-build-receipt: source-revision-drift");
    try { git(process.cwd(), "merge-base", "--is-ancestor", receipt.source.commit, current.source.commit); }
    catch { throw new Error("production-build-receipt: source-revision-not-ancestor"); }
    const disallowed = changedPaths(process.cwd(), receipt.source.commit, current.source.commit).filter((path) => !metadataOnlyPrefixes.some((prefix) => path.startsWith(prefix)));
    if (disallowed.length) throw new Error(`production-build-receipt: metadata-only-drift:${disallowed[0]}`);
  } else if (receipt.source.tree !== current.source.tree) throw new Error("production-build-receipt: source-tree-drift");
  if (canonicalBytes(receipt.inputs) !== canonicalBytes(current.inputs)) throw new Error("production-build-receipt: input-or-lock-drift");
  if (canonicalBytes(receipt.tools) !== canonicalBytes(current.tools)) throw new Error("production-build-receipt: tool-version-drift");
  if (canonicalBytes(receipt.outputs) !== canonicalBytes(current.outputs)) throw new Error("production-build-receipt: output-identity-drift");
}

async function main(argv) {
  const { mode, options, one } = parseArguments(argv);
  const root = resolve(one("root", resolve(import.meta.dirname, "..")));
  process.chdir(root);
  const ownerCommand = one("owner-command", defaultOwner);

  if (mode === "create") {
    const inputs = options.get("input") ?? defaultInputs;
    const outputs = options.get("output") ?? defaultOutputs;
    const receipt = await derive(root, ownerCommand, inputs, outputs);
    const bytes = canonicalBytes(receipt);
    const digest = sha256(bytes);
    const receiptPath = resolve(root, one("receipt-directory", defaultReceiptDirectory), `${digest}.json`);
    await mkdir(dirname(receiptPath), { recursive: true });
    const existing = await readFile(receiptPath, "utf8").catch(() => undefined);
    if (existing !== undefined && existing !== bytes) throw new Error("production-build-receipt: immutable-path-conflict");
    if (existing === undefined) await writeFile(receiptPath, bytes, { flag: "wx" });
    const pointer = one("pointer", undefined);
    if (pointer) {
      await mkdir(dirname(resolve(root, pointer)), { recursive: true });
      await writeFile(resolve(root, pointer), `${slash(relative(root, receiptPath))}\n`);
    }
    console.log(JSON.stringify({ schema, receipt: slash(relative(root, receiptPath)), digest }));
    return;
  }

  const receiptPath = resolve(root, one("receipt", ""));
  const bytes = await readFile(receiptPath, "utf8");
  const digest = sha256(bytes);
  if (basename(receiptPath) !== `${digest}.json`) throw new Error("production-build-receipt: receipt-content-address-drift");
  const receipt = JSON.parse(bytes);
  ensureShape(receipt);
  if (canonicalBytes(receipt) !== bytes) throw new Error("production-build-receipt: receipt-not-canonical");
  const inputs = options.get("input") ?? receipt.inputs.files.map((entry) => entry.path);
  const outputs = options.get("output") ?? receipt.outputs.map((entry) => `${entry.id}=${entry.path}`);

  if (mode === "verify") {
    const current = await derive(root, ownerCommand, inputs, outputs);
    compare(receipt, current, one("allow-metadata-only", "false") === "true");
    console.log(JSON.stringify({ schema, result: "pass", receipt: slash(relative(root, receiptPath)), digest, currentCommit: current.source.commit }));
    return;
  }

  if (mode === "mutate-stale-reuse" || mode === "mutate-missing-reuse") {
    const mutationOutputId = one("mutation-output-id", receipt.outputs[0].id);
    const mutationOutput = receipt.outputs.find((output) => output.id === mutationOutputId);
    if (!mutationOutput) throw new Error(`production-build-receipt: unknown-mutation-output:${mutationOutputId}`);
    const subject = resolve(root, mutationOutput.files[0].path);
    const original = await readFile(subject);
    const originalMode = (await lstat(subject)).mode & 0o777;
    try {
      if (mode === "mutate-missing-reuse") await unlink(subject);
      else await writeFile(subject, Buffer.concat([original, Buffer.from("\nreceipt-stale-mutation\n")]));
      let rejected = false;
      try {
        const current = await derive(root, ownerCommand, inputs, outputs);
        compare(receipt, current, false);
      } catch (error) {
        rejected = String(error.message).includes("output-identity-drift");
      }
      if (!rejected) throw new Error("production-build-receipt: stale-reuse-mutation-survived");
    } finally {
      await writeFile(subject, original);
      await chmod(subject, originalMode);
    }
    if (sha256(await readFile(subject)) !== sha256(original) || ((await lstat(subject)).mode & 0o777) !== originalMode) throw new Error("production-build-receipt: mutation-restoration-failed");
    console.log(JSON.stringify({ schema, result: "pass", mutation: mode === "mutate-missing-reuse" ? "missing-output-identity-drift" : "stale-output-identity-drift", output: mutationOutputId, restored: true }));
    return;
  }

  throw new Error("production-build-receipt: usage create|verify|mutate-stale-reuse|mutate-missing-reuse [options]");
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) {
  main(process.argv.slice(2)).catch((error) => {
    console.error(error.message);
    process.exitCode = 1;
  });
}
