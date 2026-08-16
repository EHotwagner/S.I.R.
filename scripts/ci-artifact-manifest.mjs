import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";
import { basename, dirname, relative, resolve } from "node:path";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { pathToFileURL } from "node:url";

export const schema = "sir.ci-artifact-manifest/v1";
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

async function derive(root, routePath, buildReceiptPath, archivePath) {
  const routeBytes = await readFile(resolve(root, routePath));
  const route = JSON.parse(routeBytes);
  if (route.schema !== "sir.ci-route/v1") throw new Error("ci-artifact-manifest: malformed-route-receipt");
  const { digest: routeDigest, ...routeBody } = route;
  if (routeDigest !== sha256(canonical(routeBody))) throw new Error("ci-artifact-manifest: stale-route-receipt");
  const receiptBytes = await readFile(resolve(root, buildReceiptPath));
  const buildReceipt = JSON.parse(receiptBytes);
  if (buildReceipt.schema !== "sir.production-build-receipt/v1" || buildReceipt.result !== "pass") throw new Error("ci-artifact-manifest: malformed-build-receipt");
  const commit = command(root, "git", ["rev-parse", "HEAD"]);
  const tree = command(root, "git", ["rev-parse", `${commit}^{tree}`]);
  if (route.source.commit !== commit || route.source.tree !== tree) throw new Error("ci-artifact-manifest: route-candidate-drift");
  if (buildReceipt.source.commit !== commit || buildReceipt.source.tree !== tree) throw new Error("ci-artifact-manifest: build-candidate-drift");
  return {
    schema,
    result: "pass",
    ownerCommand: "scripts/qualify-pr.sh",
    candidate: { commit, tree },
    route: { path: routePath.replaceAll("\\", "/"), digest: route.digest },
    buildReceipt: { path: buildReceiptPath.replaceAll("\\", "/"), digest: sha256(receiptBytes) },
    transport: await transportIdentity(root, archivePath),
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
  if (mode === "create") {
    if (!buildReceipt) throw new Error("ci-artifact-manifest: --build-receipt is required");
    const value = await derive(root, route, buildReceipt, archive);
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
    if (listing.length === 0 || listing.some((path) => path.startsWith("/") || path.split("/").includes(".."))) throw new Error("ci-artifact-manifest: unsafe-or-empty-transport");
    console.log(JSON.stringify({ schema, result: "pass", transport: currentTransport }));
    return;
  }
  if (mode === "verify") {
    if (!buildReceipt) throw new Error("ci-artifact-manifest: --build-receipt is required");
    const path = resolve(root, one("manifest", ""));
    const actual = await readManifest(root, path);
    const current = await derive(root, route, buildReceipt, archive);
    if (canonical(actual) !== canonical(current)) throw new Error("ci-artifact-manifest: candidate-input-tool-command-output-drift");
    execFileSync(process.execPath, ["scripts/production-build-receipt.mjs", "verify", "--owner-command", "scripts/qualify-pr.sh", "--receipt", buildReceipt], { cwd: root, stdio: "inherit" });
    console.log(JSON.stringify({ schema, result: "pass", manifest: relative(root, path).replaceAll("\\", "/") }));
    return;
  }
  throw new Error("ci-artifact-manifest: usage create|verify-transport|verify --route PATH --build-receipt PATH --archive PATH");
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) main(process.argv.slice(2)).catch((error) => { console.error(error.message); process.exitCode = 1; });
