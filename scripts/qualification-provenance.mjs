import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";
import { cpus, arch, platform, release, totalmem } from "node:os";

const root = new URL("..", import.meta.url).pathname;
const command = (program, args) => execFileSync(program, args, { cwd: root, encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] }).trim();
const canonical = (value) => `${JSON.stringify(value, null, 2)}\n`;
const digest = (value) => createHash("sha256").update(canonical(value)).digest("hex");

const mode = process.argv[2];
if (mode === "host") {
  const processors = cpus();
  const facts = {
    platform: platform(),
    release: release(),
    architecture: arch(),
    cpuModel: processors[0]?.model ?? "unknown",
    logicalCpuCount: processors.length,
    totalMemoryBytes: totalmem(),
    git: command("git", ["--version"]),
    dotnetSdk: command("dotnet", ["--version"]),
    node: process.version,
    npm: command("npm", ["--version"]),
  };
  process.stdout.write(canonical({ schema: "sir.qualification-host/v1", digest: digest(facts), facts }));
} else if (mode === "source") {
  const commit = command("git", ["rev-parse", "HEAD"]);
  const changes = command("git", ["status", "--porcelain", "--untracked-files=all"]);
  const source = {
    commit,
    tree: command("git", ["rev-parse", `${commit}^{tree}`]),
    clean: changes.length === 0,
    changes: changes ? changes.split("\n") : [],
  };
  process.stdout.write(canonical({ schema: "sir.qualification-source/v1", ...source }));
} else {
  throw new Error("qualification-provenance: usage host|source");
}
