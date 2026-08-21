export const browserProcessesPerShard = 2;

export const browserShardCapacityFor = (parallelism) => {
  if (!Number.isSafeInteger(parallelism) || parallelism < 1) {
    throw new Error("browser parallelism must be a positive safe integer");
  }
  return Math.max(1, Math.floor(parallelism / browserProcessesPerShard));
};
