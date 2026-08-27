import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { basename, dirname, resolve } from "node:path";
import { pathToFileURL } from "node:url";

export const handoffSchema = "sir.qualified-site-handoff/v1";
const canonical = (value) => `${JSON.stringify(value, null, 2)}\n`;
const digest = (value) => createHash("sha256").update(value).digest("hex");
const git = (...args) => execFileSync("git", args, { encoding: "utf8" }).trim();
const sourceIdentity = () => {
  const commit = git("rev-parse", "HEAD");
  return { commit, tree: git("rev-parse", `${commit}^{tree}`) };
};

function options(argv) {
  const [mode, ...tail] = argv;
  const values = new Map();
  for (let index = 0; index < tail.length; index += 2) {
    const name = tail[index];
    if (!name?.startsWith("--") || tail[index + 1] === undefined) throw new Error(`qualified-site-handoff: malformed option ${name ?? "<missing>"}`);
    values.set(name.slice(2), tail[index + 1]);
  }
  return { mode, one: (name, fallback = "") => values.get(name) ?? fallback };
}

const sameSource = (left, right) => left?.commit === right.commit && left?.tree === right.tree;
const fileDigest = async (path) => digest(await readFile(resolve(path)));
const json = async (path) => JSON.parse(await readFile(resolve(path), "utf8"));

export function verifyHandoffBody(handoff, { source, route, archiveDigest, gateDigest, siteReceiptSha256 }) {
  const failures = [];
  const body = Object.fromEntries(Object.entries(handoff ?? {}).filter(([key]) => key !== "digest"));
  if (handoff?.schema !== handoffSchema || handoff?.result !== "pass") failures.push("malformed-or-not-pass");
  if (handoff?.digest !== digest(canonical(body))) failures.push("digest-mismatch");
  if (!sameSource(handoff?.source, source)) failures.push("source-mismatch");
  if (handoff?.routeDigest !== route?.digest || !route?.selectedGates?.includes("documentation")) failures.push("route-mismatch");
  if (archiveDigest && handoff?.archive?.sha256 !== archiveDigest) failures.push("archive-mismatch");
  if (gateDigest && handoff?.documentationGateSha256 !== gateDigest) failures.push("documentation-gate-mismatch");
  if (siteReceiptSha256 && handoff?.siteReceiptSha256 !== siteReceiptSha256) failures.push("site-receipt-mismatch");
  return failures;
}

async function main(argv) {
  const { mode, one } = options(argv);
  const source = sourceIdentity();
  const route = await json(one("route"));
  const archive = one("archive");
  if (mode === "create") {
    const gatePath = one("gate");
    const siteReceiptPath = one("site-receipt");
    const gate = await json(gatePath);
    const siteReceipt = await json(siteReceiptPath);
    if (!sameSource(route?.source, source) || !route?.selectedGates?.includes("documentation")) throw new Error("qualified-site-handoff: route-source-or-selection-mismatch");
    if (gate?.schema !== "sir.ci-gate-result/v1" || gate?.gate !== "documentation" || gate?.status !== "pass" || !sameSource(gate?.source, source) || gate?.routeDigest !== route.digest) {
      throw new Error("qualified-site-handoff: documentation-gate-mismatch");
    }
    if (!sameSource(siteReceipt?.source, source) || siteReceipt?.schema !== "sir.production-build-receipt/v1") throw new Error("qualified-site-handoff: site-receipt-mismatch");
    const body = {
      schema: handoffSchema,
      result: "pass",
      source,
      routeDigest: route.digest,
      documentationGateSha256: await fileDigest(gatePath),
      siteReceiptSha256: await fileDigest(siteReceiptPath),
      archive: { name: basename(archive), sha256: await fileDigest(archive) },
    };
    const handoff = { ...body, digest: digest(canonical(body)) };
    const output = resolve(one("output"));
    await mkdir(dirname(output), { recursive: true });
    await writeFile(output, canonical(handoff));
    process.stdout.write(canonical(handoff));
    return;
  }
  if (mode === "verify") {
    const handoff = await json(one("handoff"));
    const gatePath = one("gate");
    const siteReceiptPath = one("site-receipt");
    const expectedCommit = one("commit", source.commit);
    const expectedTree = one("tree", source.tree);
    const failures = verifyHandoffBody(handoff, {
      source: { commit: expectedCommit, tree: expectedTree },
      route,
      archiveDigest: await fileDigest(archive),
      gateDigest: gatePath ? await fileDigest(gatePath) : undefined,
      siteReceiptSha256: siteReceiptPath ? await fileDigest(siteReceiptPath) : undefined,
    });
    if (failures.length > 0) throw new Error(`qualified-site-handoff: ${failures.join(",")}`);
    process.stdout.write(canonical({ schema: handoffSchema, result: "pass", source: handoff.source, digest: handoff.digest }));
    return;
  }
  throw new Error("qualified-site-handoff: usage create|verify");
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) main(process.argv.slice(2)).catch((error) => { console.error(error.message); process.exitCode = 1; });
