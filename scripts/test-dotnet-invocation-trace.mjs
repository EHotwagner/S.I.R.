import { spawnSync } from "node:child_process";
import { chmod, copyFile, mkdtemp, rm, writeFile } from "node:fs/promises";
import { join, resolve } from "node:path";
import { tmpdir } from "node:os";

const root = resolve(import.meta.dirname, "..");
const fixture = await mkdtemp(join(tmpdir(), "sir-dotnet-trace-"));
const shim = join(fixture, "dotnet");
const real = join(fixture, "real-dotnet");
const log = join(fixture, "invocations.log");
const run = (program, args, env = {}) => spawnSync(program, args, { cwd: root, encoding: "utf8", env: { ...process.env, ...env } });

try {
  await copyFile(join(root, "scripts", "dotnet-invocation-trace.sh"), shim);
  await chmod(shim, 0o755);
  await writeFile(real, "#!/usr/bin/env bash\nexit 0\n");
  await chmod(real, 0o755);
  const env = { SIR_REAL_DOTNET: real, SIR_DOTNET_INVOCATION_LOG: log };
  for (const project of ["src/SIR.Replay.Web/SIR.Replay.Web.fsproj", "src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj"]) {
    const result = run(shim, ["fable", project, "--noCache"], env);
    if (result.status !== 0) throw new Error(`trace fixture failed: ${result.stderr}`);
  }
  const green = run(process.execPath, ["scripts/verify-fable-invocations.mjs", log]);
  if (green.status !== 0) throw new Error(`expected exact process inventory green: ${green.stderr}`);

  run(shim, ["fable", "src/Third.Direct.Invocation.fsproj"], env);
  const red = run(process.execPath, ["scripts/verify-fable-invocations.mjs", log]);
  if (red.status === 0 || !red.stderr.includes("src/Third.Direct.Invocation.fsproj")) {
    throw new Error(`direct third invocation did not make the unchanged gate red: ${red.stdout} ${red.stderr}`);
  }
  await writeFile(log, "not-fable\tbroken\n");
  const unreadable = run(process.execPath, ["scripts/verify-fable-invocations.mjs", log]);
  if (unreadable.status === 0 || !unreadable.stderr.includes("unreadable entry")) throw new Error("unreadable trace produced a verdict");
  console.log("dotnet process-boundary Fable inventory passed: exact pair green, direct third invocation red, unreadable trace red");
} finally {
  await rm(fixture, { recursive: true, force: true });
}
