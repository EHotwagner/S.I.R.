import { createHash } from "node:crypto";
import { readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";

const site = resolve(process.argv[2] ?? "artifacts/site");
const catalogPath = resolve("config/engine-publication.json");
const catalog = JSON.parse(await readFile(catalogPath, "utf8"));
const engineCatalogSource = await readFile(
  resolve("src/SIR.Client/EngineCatalog.fs"),
  "utf8",
);
const viteSource = await readFile(
  resolve("src/SIR.Client.Web/vite.config.js"),
  "utf8",
);

if (catalog.schema !== "sir-engine-publication-v1") {
  throw new Error("The engine publication catalog schema is unsupported.");
}

const integrity = async (relativePath) => {
  const bytes = await readFile(resolve(site, relativePath));
  return {
    bytes: bytes.byteLength,
    integrity: `sha384-${createHash("sha384").update(bytes).digest("base64")}`,
  };
};

const engines = await Promise.all(
  catalog.engines.map(async (engine) => {
    if (!engine.workerPath.includes(engine.identity)) {
      throw new Error(`Engine ${engine.version} does not use an identity-qualified path.`);
    }
    if (
      !engineCatalogSource.includes(engine.identity) ||
      !engineCatalogSource.includes(engine.workerPath) ||
      !viteSource.includes(engine.workerPath)
    ) {
      throw new Error(
        `Engine ${engine.version} differs between the publication catalog, runtime catalog, and Vite output.`,
      );
    }

    return {
      ...engine,
      ...(await integrity(engine.workerPath)),
      retentionPolicy: catalog.replayRetentionPolicy.id,
    };
  }),
);

for (const formatVersion of catalog.replayRetentionPolicy.supportedFormatVersions) {
  if (!engines.some((engine) => engine.replayFormatVersions.includes(formatVersion))) {
    throw new Error(`Replay format ${formatVersion} has no retained engine.`);
  }
}

const application = {
  script: "content/sir-client/v1/app.js",
  stylesheet: "content/sir-client/v1/styles.css",
  scriptAsset: await integrity("content/sir-client/v1/app.js"),
  stylesheetAsset: await integrity("content/sir-client/v1/styles.css"),
};

const manifest = {
  schema: "sir-pages-publication-v1",
  hosting: "static-github-pages",
  runtime: "Fable/JavaScript",
  replayRetentionPolicy: catalog.replayRetentionPolicy,
  application,
  engines,
};

await writeFile(
  resolve(site, "publication-manifest.json"),
  `${JSON.stringify(manifest, null, 2)}\n`,
);
await writeFile(resolve(site, ".nojekyll"), "");

console.log(
  `Wrote ${manifest.schema} with ${engines.length} retained engine bundle.`,
);
