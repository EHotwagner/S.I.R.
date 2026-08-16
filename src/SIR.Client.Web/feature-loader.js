const registryVersion = 1;

const loaders = Object.freeze({
  "tactical-environment": Object.freeze({
    logicalChunk: "TacticalEnvironmentView",
    load: () => import("./.fable/SIR.Client.Web/TacticalEnvironmentView.js"),
  }),
  "rules-workbench": Object.freeze({
    logicalChunk: "RulesWorkbenchView",
    load: () => import("./.fable/SIR.Client.Web/RulesWorkbenchView.js"),
  }),
  "rules-explorer": Object.freeze({
    logicalChunk: "RulesExplorer",
    load: () => import("./.fable/RulesExplorer.js"),
  }),
  samples: Object.freeze({
    logicalChunk: "SamplesPanel",
    load: async () => { globalThis.__sirSamplesFeature = await import("./.fable/SIR.Client.Web/SamplesFeature.js"); },
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
