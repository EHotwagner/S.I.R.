import { execFile } from "node:child_process";
import { promisify } from "node:util";

const execFileAsync = promisify(execFile);
const rounds = 8;

for (let round = 1; round <= rounds; round += 1) {
  const result = await execFileAsync("node", ["scripts/smoke-worker-roundtrip.mjs"], {
    cwd: process.cwd(),
    maxBuffer: 4 * 1024 * 1024,
  });
  process.stdout.write(`worker round-trip race stress ${round}/${rounds}: ${result.stdout}`);
  process.stderr.write(result.stderr);
}

console.log(
  `Worker round-trip race stress passed: ${rounds} clean runs retained and correlated deliberately back-to-back progress/completion responses.`,
);
