import { createHash } from "node:crypto";
import { readdir, readFile, writeFile } from "node:fs/promises";
import { relative, resolve, sep } from "node:path";

const root = resolve(process.argv[2] ?? "artifacts/site/content/sir-client/v1");
const manifestPath = resolve(root, "asset-manifest.json");

async function filesUnder(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const nested = await Promise.all(
    entries.map(async (entry) => {
      const path = resolve(directory, entry.name);
      return entry.isDirectory() ? filesUnder(path) : [path];
    }),
  );

  return nested.flat();
}

const files = (await filesUnder(root))
  .filter((path) => path !== manifestPath)
  .sort();

const assets = await Promise.all(
  files.map(async (path) => {
    const bytes = await readFile(path);
    return {
      path: relative(root, path).split(sep).join("/"),
      bytes: bytes.byteLength,
      integrity: `sha384-${createHash("sha384").update(bytes).digest("base64")}`,
    };
  }),
);

const manifest = {
  schema: "sir-docs-assets-v1",
  bundleVersion: "v1",
  runtime: "Fable/JavaScript",
  assets,
};

await writeFile(manifestPath, `${JSON.stringify(manifest, null, 2)}\n`);
console.log(`Wrote ${manifest.schema} with ${assets.length} integrity entries.`);
