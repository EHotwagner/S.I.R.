export const browserShardCapacityFor = (parallelism) => {
  if (!Number.isSafeInteger(parallelism) || parallelism < 1) {
    throw new Error("browser parallelism must be a positive safe integer");
  }
  return Math.max(1, parallelism - 1);
};
