import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { pathToFileURL } from "node:url";

export const stageSchema = "sir.protected-stage/v1";
export const joinSchema = "sir.protected-join/v2";
export const stageOrder = ["preflight", "core"];
const sha256 = (value) => createHash("sha256").update(value).digest("hex");
const canonical = (value) => `${JSON.stringify(value, null, 2)}\n`;
const git = (...args) => execFileSync("git", args, { encoding: "utf8" }).trim();

function argumentsFor(argv) {
  const [mode, ...tail] = argv;
  const values = new Map();
  for (let index = 0; index < tail.length; index += 2) {
    const name = tail[index];
    if (!name?.startsWith("--") || index + 1 >= tail.length) throw new Error(`protected-stage: malformed option ${name ?? "<missing>"}`);
    const key = name.slice(2);
    values.set(key, [...(values.get(key) ?? []), tail[index + 1]]);
  }
  return { mode, one: (key, fallback) => values.get(key)?.at(-1) ?? fallback, many: (key) => values.get(key) ?? [] };
}

function sourceIdentity() {
  const commit = git("rev-parse", "HEAD");
  return { commit, tree: git("rev-parse", `${commit}^{tree}`) };
}

function workflowIdentity() {
  return {
    name: process.env.GITHUB_WORKFLOW ?? "local",
    runId: process.env.GITHUB_RUN_ID ?? "local",
    attempt: process.env.GITHUB_RUN_ATTEMPT ?? "1",
    event: process.env.GITHUB_EVENT_NAME ?? "local",
  };
}

function ensureStage(receipt, expectedStage, source) {
  if (receipt?.schema !== stageSchema || receipt.stage !== expectedStage) throw new Error(`protected-stage: malformed-${expectedStage}-receipt`);
  const body = Object.fromEntries(Object.entries(receipt).filter(([key]) => key !== "digest"));
  if (receipt.digest !== sha256(canonical(body))) throw new Error(`protected-stage: ${expectedStage}-digest-mismatch`);
  if (receipt.source?.commit !== source.commit || receipt.source?.tree !== source.tree) throw new Error(`protected-stage: ${expectedStage}-source-mismatch`);
  if (receipt.status !== "pass") throw new Error(`protected-stage: ${expectedStage}-${receipt.status ?? "malformed"}`);
  if (!Number.isSafeInteger(receipt.timingMilliseconds?.total) || receipt.timingMilliseconds.total < 0) throw new Error(`protected-stage: ${expectedStage}-timing-invalid`);
}

async function writeCanonical(path, value) {
  await mkdir(dirname(resolve(path)), { recursive: true });
  await writeFile(resolve(path), canonical(value));
}

async function main(argv) {
  const { mode, one, many } = argumentsFor(argv);
  const source = sourceIdentity();
  if (mode === "create") {
    const stage = one("stage", "");
    const status = one("status", "");
    if (!stageOrder.includes(stage) || !["pass", "fail", "cancelled"].includes(status)) throw new Error("protected-stage: invalid stage or status");
    const started = Number(one("started-ms", "0"));
    const completed = Number(one("completed-ms", "0"));
    if (!Number.isSafeInteger(started) || !Number.isSafeInteger(completed) || completed < started) throw new Error("protected-stage: invalid clock interval");
    const body = {
      schema: stageSchema,
      stage,
      status,
      source,
      workflow: workflowIdentity(),
      subjects: many("subject").sort(),
      failureStage: status === "pass" ? null : one("failure-stage", "command"),
      timingMilliseconds: { total: completed - started },
    };
    const receipt = { ...body, digest: sha256(canonical(body)) };
    await writeCanonical(one("output", ""), receipt);
    console.log(canonical(receipt).trim());
    return;
  }
  if (mode === "verify") {
    const stage = one("stage", "");
    const receipt = JSON.parse(await readFile(resolve(one("receipt", "")), "utf8"));
    ensureStage(receipt, stage, source);
    console.log(JSON.stringify({ schema: stageSchema, result: "pass", stage, source }));
    return;
  }
  if (mode === "join") {
    const receipts = new Map();
    const failures = [];
    for (const declaration of many("receipt")) {
      const separator = declaration.indexOf("=");
      if (separator <= 0) throw new Error(`protected-stage: malformed receipt declaration ${declaration}`);
      const stage = declaration.slice(0, separator);
      if (receipts.has(stage)) failures.push({ code: "duplicate-stage", stage });
      receipts.set(stage, JSON.parse(await readFile(resolve(declaration.slice(separator + 1)), "utf8")));
    }
    for (const stage of stageOrder) {
      const receipt = receipts.get(stage);
      if (!receipt) { failures.push({ code: "missing-stage", stage }); continue; }
      try { ensureStage(receipt, stage, source); }
      catch (error) { failures.push({ code: String(error.message).replace(/^protected-stage: /u, ""), stage }); }
    }
    for (const stage of receipts.keys()) if (!stageOrder.includes(stage)) failures.push({ code: "unexpected-stage", stage });
    const body = {
      schema: joinSchema,
      mode: "complete",
      result: failures.length === 0 ? "pass" : "fail",
      source,
      stages: stageOrder.map((stage) => receipts.get(stage) ?? null),
      failures,
      firstFailure: failures[0] ?? null,
    };
    const joined = { ...body, digest: sha256(canonical(body)) };
    await writeCanonical(one("output", ""), joined);
    console.log(canonical(joined).trim());
    if (failures.length) process.exitCode = 1;
    return;
  }
  if (mode === "join-focused") {
    const route = JSON.parse(await readFile(resolve(one("route", "")), "utf8"));
    const routed = JSON.parse(await readFile(resolve(one("routed", "")), "utf8"));
    const failures = [];
    if (route?.schema !== "sir.ci-route/v2" || typeof route.digest !== "string") failures.push({ code: "malformed-route", subject: "route" });
    if (route?.source?.commit !== source.commit || route?.source?.tree !== source.tree) failures.push({ code: "route-source-mismatch", subject: "route" });
    if (routed?.schema !== "sir.ci-join/v1") failures.push({ code: "malformed-routed-verdict", subject: "routed" });
    if (routed?.result !== "pass") failures.push({ code: `routed-verdict-${routed?.result ?? "malformed"}`, subject: "routed" });
    if (routed?.routeDigest !== route?.digest) failures.push({ code: "routed-route-mismatch", subject: "routed" });
    if (routed?.classification !== route?.classification) failures.push({ code: "routed-classification-mismatch", subject: "routed" });
    if (JSON.stringify(routed?.selectedGates) !== JSON.stringify(route?.selectedGates)) failures.push({ code: "routed-gates-mismatch", subject: "routed" });
    let siteHandoff = null;
    if (route?.selectedGates?.includes("documentation")) {
      if (one("site-handoff-status", "") !== "success") failures.push({ code: "site-handoff-step-failed", subject: "site-handoff" });
      try {
        siteHandoff = JSON.parse(await readFile(resolve(one("site-handoff", "")), "utf8"));
        const handoffBody = Object.fromEntries(Object.entries(siteHandoff).filter(([key]) => key !== "digest"));
        if (siteHandoff?.schema !== "sir.qualified-site-handoff/v1" || siteHandoff?.result !== "pass") failures.push({ code: "site-handoff-malformed-or-not-pass", subject: "site-handoff" });
        if (siteHandoff?.digest !== sha256(canonical(handoffBody))) failures.push({ code: "site-handoff-digest-mismatch", subject: "site-handoff" });
        if (siteHandoff?.source?.commit !== source.commit || siteHandoff?.source?.tree !== source.tree) failures.push({ code: "site-handoff-source-mismatch", subject: "site-handoff" });
        if (siteHandoff?.routeDigest !== route?.digest) failures.push({ code: "site-handoff-route-mismatch", subject: "site-handoff" });
      } catch {
        failures.push({ code: "site-handoff-missing-or-unreadable", subject: "site-handoff" });
      }
    }
    const body = {
      schema: joinSchema,
      mode: "focused",
      result: failures.length === 0 ? "pass" : "fail",
      source,
      route: {
        schema: route?.schema ?? null,
        digest: route?.digest ?? null,
        classification: route?.classification ?? null,
        selectedGates: route?.selectedGates ?? null,
      },
      routed: {
        schema: routed?.schema ?? null,
        result: routed?.result ?? null,
        routeDigest: routed?.routeDigest ?? null,
      },
      siteHandoff: siteHandoff === null ? null : {
        schema: siteHandoff.schema ?? null,
        result: siteHandoff.result ?? null,
        digest: siteHandoff.digest ?? null,
        routeDigest: siteHandoff.routeDigest ?? null,
      },
      failures,
      firstFailure: failures[0] ?? null,
    };
    const joined = { ...body, digest: sha256(canonical(body)) };
    await writeCanonical(one("output", ""), joined);
    console.log(canonical(joined).trim());
    if (failures.length) process.exitCode = 1;
    return;
  }
  throw new Error("protected-stage: usage create|verify|join|join-focused");
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) main(process.argv.slice(2)).catch((error) => { console.error(error.message); process.exitCode = 1; });
