#!/usr/bin/env node

import { mkdir, readFile, writeFile } from "node:fs/promises";
import { basename, join, resolve } from "node:path";
import process from "node:process";

const sourceArgument = process.argv[2];

if (!sourceArgument) {
  console.error("Usage: npm run import:map-design -- <path-to.sir-design.json>");
  process.exitCode = 2;
} else {
  const repositoryRoot = process.cwd();
  const packageJson = JSON.parse(
    await readFile(join(repositoryRoot, "package.json"), "utf8"),
  );

  if (packageJson.name !== "sir") {
    throw new Error("Run this command from the S.I.R repository root.");
  }

  const sourcePath = resolve(sourceArgument);
  const bundle = JSON.parse(await readFile(sourcePath, "utf8"));

  if (bundle.format !== "sir-map-editor-design" || bundle.version !== 1) {
    throw new Error(
      `${basename(sourcePath)} is not a supported S.I.R design bundle.`,
    );
  }

  if (
    typeof bundle.name !== "string" ||
    typeof bundle.editor?.digest !== "string" ||
    typeof bundle.editor?.map !== "string"
  ) {
    throw new Error("The design bundle is missing its name, digest, or editor map.");
  }

  const slug =
    bundle.name
      .trim()
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, "-")
      .replace(/^-+|-+$/g, "") || "battlefield";
  const destination = join(repositoryRoot, "designs", "map-editor", slug);
  await mkdir(destination, { recursive: true });

  const normalizedBundle = `${JSON.stringify(bundle, null, 2)}\n`;
  await writeFile(
    join(destination, "design.sir-design.json"),
    normalizedBundle,
    "utf8",
  );
  await writeFile(join(destination, "map.sir-map"), bundle.editor.map, "utf8");

  if (typeof bundle.simulator?.map === "string") {
    await writeFile(
      join(destination, "simulator.sir-map"),
      bundle.simulator.map,
      "utf8",
    );
  }

  console.log(`Imported ${bundle.name} into designs/map-editor/${slug}/`);
  console.log("Review with: git diff -- designs/map-editor");
  console.log("Then commit and open a pull request through the normal repository workflow.");
}
