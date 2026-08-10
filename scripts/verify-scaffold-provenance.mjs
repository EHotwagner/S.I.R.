import { createHash } from "node:crypto";
import { homedir } from "node:os";
import { existsSync, readFileSync, readdirSync, statSync } from "node:fs";
import { dirname, join, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const provenance = JSON.parse(readFileSync(join(repoRoot, "config/scaffold-provenance.json"), "utf8"));
const fail = (message) => { throw new Error(`scaffold provenance: ${message}`); };
const requireEqual = (actual, expected, label) => {
  if (actual !== expected) fail(`${label}: expected ${expected}, got ${actual}`);
};
const sha256 = (bytes) => createHash("sha256").update(bytes).digest("hex");

requireEqual(provenance.template.package, "FS.GG.Workspace.Template", "template package");
requireEqual(provenance.template.version, "0.8.0", "template version");
requireEqual(provenance.template.templateId, "fs-gg-fable-game", "template id");
requireEqual(provenance.sdd.version, "1.0.0", "SDD materializer version");
requireEqual(provenance.gameSkills.version, "0.7.0", "Game Skills version");
requireEqual(provenance.gameSkills.sha256, "443a82d24a0b4bbd21f4499b06f6e3d12b95a36a858f3880b414b74cae1a5c50", "lockstep skill digest");
requireEqual(provenance.gameCore.version, "0.13.0", "Game.Core version");
requireEqual(provenance.gameCore.profile, "fs-gg-game-core-fable-lockstep-v1", "Game.Core profile");

const skillPath = join(repoRoot, provenance.gameSkills.resolvablePath);
requireEqual(sha256(readFileSync(skillPath)), provenance.gameSkills.sha256, "materialized skill bytes");
requireEqual(sha256(readFileSync(join(repoRoot, ".claude/skills/fs-gg-game-fable/SKILL.md"))), provenance.gameSkills.sha256, "mirrored skill bytes");

const packageProps = readFileSync(join(repoRoot, "Directory.Packages.props"), "utf8");
if (!packageProps.includes('<PackageVersion Include="FS.GG.Game.Core" Version="[0.13.0]"')) {
  fail("FS.GG.Game.Core is not centrally pinned to exact version 0.13.0");
}

const walk = (root) => readdirSync(root, { withFileTypes: true }).flatMap((entry) => {
  const path = join(root, entry.name);
  if (entry.isDirectory() && ![".git", "bin", "obj", "node_modules", "artifacts"].includes(entry.name)) return walk(path);
  return entry.isFile() ? [path] : [];
});

for (const project of walk(repoRoot).filter((path) => path.endsWith(".fsproj"))) {
  const xml = readFileSync(project, "utf8");
  for (const match of xml.matchAll(/<ProjectReference\s+Include="([^"]+)"/g)) {
    const target = resolve(dirname(project), match[1]);
    if (!relative(repoRoot, target) || relative(repoRoot, target).startsWith("..")) {
      fail(`project reference escapes this repository: ${relative(repoRoot, project)} -> ${match[1]}`);
    }
    if (/FS\.GG\.Game/i.test(match[1])) fail(`Game.Core sibling project reference found in ${relative(repoRoot, project)}`);
  }
}

const npmLock = readFileSync(join(repoRoot, "package-lock.json"), "utf8");
if (/"(?:resolved|version)"\s*:\s*"file:/i.test(npmLock)) fail("npm lock contains a local file dependency");

const sharedAuthority = [
  "src/SIR.Simulation/Simulation.fs",
  "src/SIR.Simulation/Replay.fs",
  "tests/SIR.Conformance.Shared/NumericFixtures.fs",
  "tests/SIR.Conformance.Shared/SimulationFixtures.fs",
  "tests/SIR.Conformance.Shared/ReplayFixtures.fs",
].map((path) => readFileSync(join(repoRoot, path), "utf8")).join("\n");
const dotNetOnlyModules = ["Geometry", "Rng", "Dice", "FixedStep", "Loop", "Hex", "Grids", "MapGen", "Fov", "SpatialGrid", "Resolution", "Ballistics", "Visibility", "Ai", "Effects", "Physics", "MapAnalysis"];
for (const moduleName of dotNetOnlyModules) {
  if (new RegExp(`\\b${moduleName}\\.`).test(sharedAuthority)) fail(`${moduleName} entered cross-runtime authoritative logic without a LockstepExact classification`);
}

const packagesRoot = process.env.NUGET_PACKAGES || join(homedir(), ".nuget/packages");
const coreRoot = join(packagesRoot, "fs.gg.game.core", provenance.gameCore.version);
if (!existsSync(coreRoot) || !statSync(coreRoot).isDirectory()) fail("restored Game.Core package is missing");
const profile = JSON.parse(readFileSync(join(coreRoot, "fable-compatibility/compatibility-profile.v1.json"), "utf8"));
requireEqual(profile.profileId, provenance.gameCore.profile, "restored compatibility profile");
const exact = profile.surfaces.filter((surface) => surface.grade === "LockstepExact").map((surface) => surface.name);
requireEqual(exact.length, 4, "LockstepExact surface count");
requireEqual(sha256(readFileSync(join(coreRoot, "fable-compatibility/fixtures/v1/expected.bin"))), provenance.gameCore.oracleSha256, "canonical oracle");

console.log("scaffold provenance verified: template 0.8.0, SDD 1.0.0, Game Skills 0.7.0, Game.Core 0.13.0");
