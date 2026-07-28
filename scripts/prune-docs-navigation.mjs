import { readdir, readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";

const site = resolve(process.argv[2] ?? "artifacts/site");

const filesUnder = async (directory) => {
  const entries = await readdir(directory, { withFileTypes: true });
  const nested = await Promise.all(
    entries.map(async (entry) => {
      const path = resolve(directory, entry.name);
      return entry.isDirectory() ? filesUnder(path) : [path];
    }),
  );
  return nested.flat();
};

const otherSection =
  /<li class="nav-header[^"]*">\s*Other\s*<\/li>\s*(?:(?!<li class="nav-header)[\s\S])*?(?=<li class="nav-header)/g;

const htmlFiles = (await filesUnder(site)).filter((path) => path.endsWith(".html"));
let removedSections = 0;

for (const path of htmlFiles) {
  const source = await readFile(path, "utf8");
  let pageRemovals = 0;
  const pruned = source.replace(otherSection, () => {
    pageRemovals += 1;
    return "";
  });

  if (pageRemovals > 0) {
    removedSections += pageRemovals;
    await writeFile(path, pruned);
  }
}

if (removedSections === 0) {
  throw new Error("The generated site had no Other navigation sections to prune.");
}

console.log(
  `Pruned ${removedSections} Other navigation sections from ${htmlFiles.length} generated pages.`,
);
