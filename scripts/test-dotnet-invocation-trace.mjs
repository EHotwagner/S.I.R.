import { spawnSync } from "node:child_process";
import { chmod, copyFile, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
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
  const expected = [
    "tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj",
    "tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj",
    "tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj",
    "src/SIR.Replay.Web/SIR.Replay.Web.fsproj",
    "src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj",
  ];
  for (const project of [expected[1], expected[0], expected[4], expected[3], expected[2]]) {
    const result = run(shim, ["fable", project, "--noCache"], env);
    if (result.status !== 0) throw new Error(`trace fixture failed: ${result.stderr}`);
  }
  run(shim, ["build", "tests/Build.fsproj"], env);
  run(shim, ["publish", "src/Server.fsproj"], env);
  run(shim, ["run", "--project", "tests/Mutation.fsproj"], env);
  const green = run(process.execPath, ["scripts/verify-fable-invocations.mjs", log]);
  if (green.status !== 0) throw new Error(`expected exact Fable inventory with valid non-Fable traces green: ${green.stderr}`);

  run(shim, ["fable", expected[3]], env);
  const duplicateRed = run(process.execPath, ["scripts/verify-fable-invocations.mjs", log]);
  if (duplicateRed.status === 0 || !duplicateRed.stderr.includes(expected[3])) {
    throw new Error(`direct duplicate invocation did not make the unchanged gate red: ${duplicateRed.stdout} ${duplicateRed.stderr}`);
  }
  const repeatedGreen = run(process.execPath, ["scripts/verify-fable-invocations.mjs", log,
    "--expect", `${expected[0]}=1`, "--expect", `${expected[1]}=1`, "--expect", `${expected[2]}=1`, "--expect", `${expected[3]}=2`, "--expect", `${expected[4]}=1`]);
  if (repeatedGreen.status !== 0 || JSON.parse(repeatedGreen.stdout).total !== 6) throw new Error(`declared repeated inventory was not derived: ${repeatedGreen.stdout} ${repeatedGreen.stderr}`);

  await writeFile(log, "");
  run(shim, ["build", "tests/Build.fsproj"], env);
  run(shim, ["publish", "src/Server.fsproj"], env);
  run(shim, ["run", "--project", "tests/Mutation.fsproj"], env);
  run(shim, ["run", "--project", "tests/Reused.fsproj", "--no-build", "--no-restore"], env);
  const buildInventory = (await readFile(log, "utf8")).trim().split("\n");
  const buildSubjects = buildInventory.map((line) => {
    const [kind, project, identity, started, completed, extra] = line.split("\t");
    if (!kind || !project || identity !== "-" || !/^\d+$/u.test(started) || !/^\d+$/u.test(completed) || Number(completed) < Number(started) || extra !== undefined) throw new Error(`malformed timed build trace: ${line}`);
    return `${kind}\t${project}`;
  });
  if (JSON.stringify(buildSubjects) !== JSON.stringify([
    "build\ttests/Build.fsproj",
    "publish\tsrc/Server.fsproj",
    "run-build\ttests/Mutation.fsproj",
  ])) throw new Error(`build invocation trace was incomplete or counted reuse as a build: ${JSON.stringify(buildSubjects)}`);

  await writeFile(log, "");
  run(shim, ["run", "--project", join(root, "tests/Mutation.fsproj"), "--artifacts-path", join(fixture, "isolated")], { ...env, SIR_DOTNET_TRACE_ROOT: root, SIR_BUILD_EXCEPTION: "spatial-fixture" });
  const isolated = (await readFile(log, "utf8")).trim().split("\t");
  if (isolated[0] !== "run-build" || isolated[1] !== "tests/Mutation.fsproj" || isolated[2] !== "exception:spatial-fixture:artifacts-path:isolated") throw new Error(`isolated exception identity was not traced: ${JSON.stringify(isolated)}`);

  await writeFile(log, "");
  run(shim, ["build", join(root, "src/SIR.Simulation/SIR.Simulation.fsproj"), "--artifacts-path", join(fixture, "isolated")], { ...env, SIR_DOTNET_TRACE_ROOT: root, SIR_BUILD_EXCEPTION: "spatial-dependency-receipt" });
  const isolatedBuild = (await readFile(log, "utf8")).trim().split("\t");
  if (isolatedBuild[0] !== "build" || isolatedBuild[1] !== "src/SIR.Simulation/SIR.Simulation.fsproj" || isolatedBuild[2] !== "exception:spatial-dependency-receipt:artifacts-path:isolated") throw new Error(`isolated named build identity was not traced: ${JSON.stringify(isolatedBuild)}`);

  await writeFile(log, "");
  run(shim, ["fable", expected[3]], { ...env, SIR_BUILD_EXCEPTION: "cancellation-fixture" });
  const attributedFable = (await readFile(log, "utf8")).trim().split("\t");
  if (attributedFable[0] !== "fable" || attributedFable[1] !== expected[3] || attributedFable[2] !== "exception:cancellation-fixture") throw new Error(`Fable exception identity was not traced: ${JSON.stringify(attributedFable)}`);

  await writeFile(log, `${expected.map((project) => `fable\t${project}\t-\t1\t2`).join("\n")}\n`);
  run(shim, ["fable", "src/Unknown.Direct.Invocation.fsproj"], env);
  const unknownRed = run(process.execPath, ["scripts/verify-fable-invocations.mjs", log]);
  if (unknownRed.status === 0 || !unknownRed.stderr.includes("src/Unknown.Direct.Invocation.fsproj")) {
    throw new Error(`direct unknown invocation did not make the unchanged gate red: ${unknownRed.stdout} ${unknownRed.stderr}`);
  }
  await writeFile(log, "not-fable\tbroken\t-\t1\t2\n");
  const unreadable = run(process.execPath, ["scripts/verify-fable-invocations.mjs", log]);
  if (unreadable.status === 0 || !unreadable.stderr.includes("unreadable entry")) throw new Error("unreadable trace produced a verdict");
  console.log("dotnet process-boundary Fable inventory passed: declared graph exact-once green, duplicate red, unknown red, unreadable trace red");
} finally {
  await rm(fixture, { recursive: true, force: true });
}
