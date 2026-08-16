const registryVersion = 1;

const loaders = Object.freeze({
  "rules-explorer": Object.freeze({
    logicalChunk: "RulesExplorer",
    load: () => import("./.fable/RulesExplorer.js"),
  }),
  docs: Object.freeze({
    logicalChunk: "docs-feature",
    load: () => import("./docs-feature.js"),
  }),
});

export async function loadFeature(version, featureId, logicalChunk) {
  if (version !== registryVersion) {
    throw new Error(`stale-identity: registry ${version}; expected ${registryVersion}`);
  }
  const entry = loaders[featureId];
  if (!entry || entry.logicalChunk !== logicalChunk) {
    throw new Error(`stale-identity: ${featureId}:${logicalChunk}`);
  }
  await entry.load();
  return { registryVersion, featureId, logicalChunk };
}
