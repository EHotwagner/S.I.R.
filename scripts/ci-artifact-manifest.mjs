import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";
import { basename, dirname, relative, resolve } from "node:path";
import { chmod, copyFile, lstat, mkdir, readFile, readdir, rm, writeFile } from "node:fs/promises";
import { pathToFileURL } from "node:url";

export const schema = "sir.ci-artifact-manifest/v2";
export const browserCompositionSchema = "sir.ci-browser-composition/v1";
export const contentIndexSchema = "sir.ci-content-index/v1";
const canonical = (value) => `${JSON.stringify(value, null, 2)}\n`;
const sha256 = (bytes) => createHash("sha256").update(bytes).digest("hex");
const command = (root, program, args) => execFileSync(program, args, { cwd: root, encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] }).trim();

function options(argv) {
  const [mode, ...tail] = argv;
  const values = new Map();
  for (let index = 0; index < tail.length; index += 2) {
    if (!tail[index]?.startsWith("--") || tail[index + 1] === undefined) throw new Error(`ci-artifact-manifest: malformed option:${tail[index] ?? "missing"}`);
    values.set(tail[index].slice(2), tail[index + 1]);
  }
  return { mode, one: (name, fallback) => values.get(name) ?? fallback };
}

async function transportIdentity(root, archivePath) {
  const bytes = await readFile(resolve(root, archivePath));
  return { path: archivePath.replaceAll("\\", "/"), bytes: bytes.byteLength, sha256: sha256(bytes) };
}

function safeRelative(path) {
  const normalized = path.replaceAll("\\", "/");
  if (!normalized || normalized.startsWith("/") || normalized.split("/").includes("..")) throw new Error(`ci-artifact-manifest: unsafe-staged-output:${path}`);
  return normalized;
}

async function filesUnder(root, path) {
  const relativePath = safeRelative(path);
  const absolute = resolve(root, relativePath);
  const information = await lstat(absolute).catch(() => undefined);
  if (!information) throw new Error(`ci-artifact-manifest: missing-staged-output:${relativePath}`);
  if (information.isSymbolicLink()) throw new Error(`ci-artifact-manifest: symbolic-staged-output:${relativePath}`);
  if (information.isFile()) return [absolute];
  if (!information.isDirectory()) throw new Error(`ci-artifact-manifest: unsupported-staged-output:${relativePath}`);
  const files = [];
  for (const entry of await readdir(absolute, { withFileTypes: true })) {
    const child = relative(root, resolve(absolute, entry.name)).replaceAll("\\", "/");
    if (entry.isSymbolicLink()) throw new Error(`ci-artifact-manifest: symbolic-staged-output:${child}`);
    if (entry.isDirectory()) files.push(...(await filesUnder(root, child)));
    else if (entry.isFile()) files.push(resolve(root, child));
  }
  return files.sort((left, right) => relative(root, left).localeCompare(relative(root, right)));
}

async function stagedOutputs(root, outputs) {
  const identities = [];
  for (const output of outputs) {
    const path = safeRelative(output.path);
    const entries = [];
    for (const file of await filesUnder(root, path)) {
      const bytes = await readFile(file);
      const information = await lstat(file);
      entries.push({ path: relative(root, file).replaceAll("\\", "/"), mode: information.mode & 0o777, bytes: bytes.byteLength, sha256: sha256(bytes) });
    }
    identities.push({ id: output.id, path, files: entries, digest: sha256(canonical(entries)) });
  }
  return identities.sort((left, right) => left.id.localeCompare(right.id));
}

async function treeIdentity(root, path) {
  const base = resolve(root, safeRelative(path));
  const entries = [];
  for (const file of await filesUnder(root, path)) {
    const bytes = await readFile(file);
    const information = await lstat(file);
    entries.push({ path: relative(base, file).replaceAll("\\", "/"), mode: information.mode & 0o777, bytes: bytes.byteLength, sha256: sha256(bytes) });
  }
  return { files: entries, digest: sha256(canonical(entries)) };
}

function contentIndexForReceipt(receipt) {
  const byPath = new Map();
  for (const output of receipt.outputs ?? []) for (const file of output.files ?? []) {
    const path = safeRelative(file.path);
    const current = { path, mode: file.mode, bytes: file.bytes, sha256: file.sha256 };
    const previous = byPath.get(path);
    if (previous && canonical(previous) !== canonical(current)) throw new Error(`ci-artifact-manifest: conflicting-content-path:${path}`);
    byPath.set(path, current);
  }
  const files = [...byPath.values()].sort((left, right) => left.path.localeCompare(right.path));
  if (files.length === 0) throw new Error("ci-artifact-manifest: empty-content-index");
  const objects = [...new Map(files.map((file) => [file.sha256, { sha256: file.sha256, bytes: file.bytes }])).values()].sort((left, right) => left.sha256.localeCompare(right.sha256));
  const body = {
    schema: contentIndexSchema,
    files,
    objects,
    totals: {
      logicalFiles: files.length,
      uniqueObjects: objects.length,
      logicalBytes: files.reduce((sum, file) => sum + file.bytes, 0),
      storedBytes: objects.reduce((sum, object) => sum + object.bytes, 0),
    },
  };
  return { ...body, digest: sha256(canonical(body)) };
}

async function readContentIndex(path) {
  const bytes = await readFile(path, "utf8");
  const value = JSON.parse(bytes);
  const { digest, ...body } = value;
  if (value.schema !== contentIndexSchema || canonical(value) !== bytes || digest !== sha256(canonical(body))) throw new Error("ci-artifact-manifest: malformed-content-index");
  return value;
}

async function packContentStore(root, receiptPath, storePath, archivePath, indexPath) {
  const receipt = JSON.parse(await readFile(resolve(root, receiptPath), "utf8"));
  if (receipt.schema !== "sir.production-build-receipt/v1" || receipt.result !== "pass") throw new Error("ci-artifact-manifest: malformed-build-receipt");
  const index = contentIndexForReceipt(receipt);
  const store = resolve(root, storePath);
  await rm(store, { recursive: true, force: true });
  await mkdir(resolve(store, ".sir-cas/objects"), { recursive: true });
  for (const object of index.objects) {
    const source = index.files.find((file) => file.sha256 === object.sha256);
    const bytes = await readFile(resolve(root, source.path));
    if (bytes.byteLength !== object.bytes || sha256(bytes) !== object.sha256) throw new Error(`ci-artifact-manifest: source-content-drift:${source.path}`);
    await writeFile(resolve(store, `.sir-cas/objects/${object.sha256}`), bytes, { flag: "wx" });
  }
  await writeFile(resolve(store, ".sir-cas/tree.json"), canonical(index));
  await mkdir(dirname(resolve(root, archivePath)), { recursive: true });
  execFileSync("tar", ["--sort=name", "--mtime=@0", "--owner=0", "--group=0", "--numeric-owner", "-cf", resolve(root, archivePath), ".sir-cas"], { cwd: store, stdio: "inherit" });
  await mkdir(dirname(resolve(root, indexPath)), { recursive: true });
  await writeFile(resolve(root, indexPath), canonical(index));
  return index;
}

async function reconstructContentStore(root, storePath, destinationPath, expectedIndex) {
  const store = resolve(root, storePath);
  const index = await readContentIndex(resolve(store, ".sir-cas/tree.json"));
  if (canonical(index) !== canonical(expectedIndex)) throw new Error("ci-artifact-manifest: content-index-drift");
  const objectDirectory = resolve(store, ".sir-cas/objects");
  const actualObjects = (await readdir(objectDirectory, { withFileTypes: true })).filter((entry) => entry.isFile()).map((entry) => entry.name).sort();
  const expectedObjects = index.objects.map(({ sha256: digest }) => digest).sort();
  if (canonical(actualObjects) !== canonical(expectedObjects)) throw new Error("ci-artifact-manifest: content-object-inventory-drift");
  for (const object of index.objects) {
    const bytes = await readFile(resolve(objectDirectory, object.sha256));
    if (bytes.byteLength !== object.bytes || sha256(bytes) !== object.sha256) throw new Error(`ci-artifact-manifest: content-object-drift:${object.sha256}`);
  }
  const destination = resolve(root, destinationPath);
  await rm(destination, { recursive: true, force: true });
  await mkdir(destination, { recursive: true });
  for (const file of index.files) {
    const target = resolve(destination, safeRelative(file.path));
    if (!target.startsWith(`${destination}/`)) throw new Error(`ci-artifact-manifest: unsafe-content-path:${file.path}`);
    await mkdir(dirname(target), { recursive: true });
    await copyFile(resolve(objectDirectory, file.sha256), target);
    await chmod(target, file.mode);
  }
  return index;
}

async function browserComposition(root, webManifestPath, serverManifestPath, clientPath, publishPath) {
  const web = await readManifest(root, webManifestPath);
  const server = await readManifest(root, serverManifestPath);
  if (canonical(web.candidate) !== canonical(server.candidate) || canonical(web.route) !== canonical(server.route)) throw new Error("ci-artifact-manifest: composition-input-binding-drift");
  if (!web.outputs.some(({ path }) => path === clientPath) || !server.outputs.some(({ path }) => path === publishPath)) throw new Error("ci-artifact-manifest: composition-owner-output-missing");
  const source = await treeIdentity(root, clientPath);
  const outputPath = `${publishPath.replace(/\/$/u, "")}/wwwroot`;
  const output = await treeIdentity(root, outputPath);
  if (canonical(source) !== canonical(output)) throw new Error("ci-artifact-manifest: browser-composition-output-drift");
  return {
    schema: browserCompositionSchema,
    result: "pass",
    candidate: web.candidate,
    route: web.route,
    inputs: {
      serverManifest: { path: serverManifestPath.replaceAll("\\", "/"), digest: sha256(await readFile(resolve(root, serverManifestPath))) },
      webManifest: { path: webManifestPath.replaceAll("\\", "/"), digest: sha256(await readFile(resolve(root, webManifestPath))) },
    },
    source: { path: clientPath, ...source },
    output: { path: outputPath, ...output },
  };
}

async function derive(root, routePath, buildReceiptPath, archivePath, contentIndexPath) {
  const routeBytes = await readFile(resolve(root, routePath));
  const route = JSON.parse(routeBytes);
  if (route.schema !== "sir.ci-route/v2") throw new Error("ci-artifact-manifest: malformed-route-receipt");
  const { digest: routeDigest, ...routeBody } = route;
  if (routeDigest !== sha256(canonical(routeBody))) throw new Error("ci-artifact-manifest: stale-route-receipt");
  const receiptBytes = await readFile(resolve(root, buildReceiptPath));
  const buildReceipt = JSON.parse(receiptBytes);
  if (buildReceipt.schema !== "sir.production-build-receipt/v1" || buildReceipt.result !== "pass") throw new Error("ci-artifact-manifest: malformed-build-receipt");
  const commit = command(root, "git", ["rev-parse", "HEAD"]);
  const tree = command(root, "git", ["rev-parse", `${commit}^{tree}`]);
  if (route.source.commit !== commit || route.source.tree !== tree) throw new Error("ci-artifact-manifest: route-candidate-drift");
  if (buildReceipt.source.commit !== commit || buildReceipt.source.tree !== tree) throw new Error("ci-artifact-manifest: build-candidate-drift");
  const contentIndex = await readContentIndex(resolve(root, contentIndexPath));
  if (canonical(contentIndex) !== canonical(contentIndexForReceipt(buildReceipt))) throw new Error("ci-artifact-manifest: build-content-index-drift");
  return {
    schema,
    result: "pass",
    ownerCommand: "scripts/qualify-pr.sh",
    candidate: { commit, tree },
    route: { path: routePath.replaceAll("\\", "/"), digest: route.digest },
    buildReceipt: { path: buildReceiptPath.replaceAll("\\", "/"), digest: sha256(receiptBytes) },
    transport: await transportIdentity(root, archivePath),
    contentIndex,
    outputs: buildReceipt.outputs,
  };
}

async function readManifest(root, path) {
  const absolute = resolve(root, path);
  const bytes = await readFile(absolute);
  if (basename(absolute) !== `${sha256(bytes)}.json`) throw new Error("ci-artifact-manifest: content-address-drift");
  const actual = JSON.parse(bytes);
  if (actual.schema !== schema || actual.result !== "pass" || canonical(actual) !== bytes.toString("utf8")) throw new Error("ci-artifact-manifest: malformed-manifest");
  return actual;
}

async function main(argv) {
  const { mode, one } = options(argv);
  const root = resolve(one("root", resolve(import.meta.dirname, "..")));
  const route = one("route", "artifacts/ci/route.json");
  const buildReceipt = one("build-receipt", "");
  const archive = one("archive", "artifacts/prepared-candidate.tar");
  const contentIndex = one("content-index", `${archive}.index.json`);
  if (mode === "pack") {
    if (!buildReceipt) throw new Error("ci-artifact-manifest: --build-receipt is required");
    const index = await packContentStore(root, buildReceipt, one("store", "artifacts/ci/content-store"), archive, contentIndex);
    console.log(JSON.stringify({ schema: contentIndexSchema, result: "pass", archive, contentIndex, totals: index.totals }));
    return;
  }
  if (mode === "create") {
    if (!buildReceipt) throw new Error("ci-artifact-manifest: --build-receipt is required");
    const value = await derive(root, route, buildReceipt, archive, contentIndex);
    const bytes = canonical(value);
    const digest = sha256(bytes);
    const path = resolve(root, one("directory", "artifacts/ci/manifests"), `${digest}.json`);
    await mkdir(dirname(path), { recursive: true });
    const existing = await readFile(path).catch(() => undefined);
    if (existing && !existing.equals(Buffer.from(bytes))) throw new Error("ci-artifact-manifest: immutable-path-conflict");
    if (!existing) await writeFile(path, bytes, { flag: "wx" });
    const pointer = one("pointer", undefined);
    if (pointer) await writeFile(resolve(root, pointer), `${relative(root, path).replaceAll("\\", "/")}\n`);
    console.log(JSON.stringify({ schema, result: "pass", manifest: relative(root, path).replaceAll("\\", "/"), digest }));
    return;
  }
  if (mode === "verify-transport") {
    const actual = await readManifest(root, one("manifest", ""));
    const routeBytes = await readFile(resolve(root, route));
    const currentRoute = JSON.parse(routeBytes);
    const { digest: claimedRouteDigest, ...routeBody } = currentRoute;
    if (claimedRouteDigest !== sha256(canonical(routeBody)) || actual.route.digest !== claimedRouteDigest) throw new Error("ci-artifact-manifest: stale-route-receipt");
    if (actual.candidate.commit !== currentRoute.source.commit || actual.candidate.tree !== currentRoute.source.tree) throw new Error("ci-artifact-manifest: route-candidate-drift");
    const currentTransport = await transportIdentity(root, archive);
    if (canonical(actual.transport) !== canonical(currentTransport)) throw new Error("ci-artifact-manifest: transport-identity-drift");
    const listing = command(root, "tar", ["-tf", archive]).split("\n").filter(Boolean);
    if (listing.length === 0 || listing.some((path) => path.startsWith("/") || path.split("/").includes("..") || !(path === ".sir-cas/" || path === ".sir-cas/tree.json" || path === ".sir-cas/objects/" || /^\.sir-cas\/objects\/[0-9a-f]{64}$/u.test(path)))) throw new Error("ci-artifact-manifest: unsafe-or-empty-transport");
    console.log(JSON.stringify({ schema, result: "pass", transport: currentTransport }));
    return;
  }
  if (mode === "reconstruct") {
    const actual = await readManifest(root, one("manifest", ""));
    const reconstructed = await reconstructContentStore(root, one("store", ""), one("destination", ""), actual.contentIndex);
    console.log(JSON.stringify({ schema: contentIndexSchema, result: "pass", destination: one("destination", ""), totals: reconstructed.totals }));
    return;
  }
  if (mode === "verify-staged") {
    if (!buildReceipt) throw new Error("ci-artifact-manifest: --build-receipt is required");
    const actual = await readManifest(root, one("manifest", ""));
    const receiptBytes = await readFile(resolve(root, buildReceipt));
    const receipt = JSON.parse(receiptBytes);
    if (actual.buildReceipt.digest !== sha256(receiptBytes) || canonical(actual.outputs) !== canonical(receipt.outputs)) throw new Error("ci-artifact-manifest: staged-receipt-binding-drift");
    const stage = resolve(root, one("stage", ""));
    const outputs = await stagedOutputs(stage, receipt.outputs);
    if (canonical(outputs) !== canonical(receipt.outputs)) throw new Error("ci-artifact-manifest: staged-output-identity-drift");
    console.log(JSON.stringify({ schema, result: "pass", stage: relative(root, stage).replaceAll("\\", "/"), outputsDigest: sha256(canonical(outputs)) }));
    return;
  }
  if (mode === "create-browser-composition" || mode === "verify-browser-composition") {
    const output = one("output", "artifacts/ci/browser-composition.json");
    const current = await browserComposition(root, one("web-manifest", ""), one("server-manifest", ""), one("client", "artifacts/client"), one("publish", "artifacts/publish"));
    const outputPath = resolve(root, output);
    if (mode === "create-browser-composition") {
      await mkdir(dirname(outputPath), { recursive: true });
      await writeFile(outputPath, canonical(current));
    } else {
      const recorded = await readFile(outputPath, "utf8");
      if (recorded !== canonical(current)) throw new Error("ci-artifact-manifest: browser-composition-receipt-drift");
    }
    console.log(JSON.stringify({ schema: browserCompositionSchema, result: "pass", receipt: relative(root, outputPath).replaceAll("\\", "/"), digest: sha256(canonical(current)) }));
    return;
  }
  if (mode === "verify") {
    if (!buildReceipt) throw new Error("ci-artifact-manifest: --build-receipt is required");
    const path = resolve(root, one("manifest", ""));
    const actual = await readManifest(root, path);
    const current = await derive(root, route, buildReceipt, archive, contentIndex);
    if (canonical(actual) !== canonical(current)) throw new Error("ci-artifact-manifest: candidate-input-tool-command-output-drift");
    execFileSync(process.execPath, ["scripts/production-build-receipt.mjs", "verify", "--owner-command", "scripts/qualify-pr.sh", "--receipt", buildReceipt], { cwd: root, stdio: "inherit" });
    console.log(JSON.stringify({ schema, result: "pass", manifest: relative(root, path).replaceAll("\\", "/") }));
    return;
  }
  throw new Error("ci-artifact-manifest: usage pack|create|verify-transport|reconstruct|verify-staged|create-browser-composition|verify-browser-composition|verify [options]");
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) main(process.argv.slice(2)).catch((error) => { console.error(error.message); process.exitCode = 1; });
