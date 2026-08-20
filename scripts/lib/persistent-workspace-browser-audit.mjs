import { spawn } from "node:child_process";
import { constants } from "node:fs";
import { createServer } from "node:http";
import { access, mkdtemp, readFile, rm, stat, writeFile } from "node:fs/promises";
import { createServer as createNetServer } from "node:net";
import { homedir, tmpdir } from "node:os";
import { delimiter, extname, isAbsolute, join, normalize, resolve, sep } from "node:path";
import { chromium as playwrightChromium } from "@playwright/test";

const delay = (milliseconds) => new Promise((resolveDelay) => setTimeout(resolveDelay, milliseconds));
const chromiumStartupProbeIntervalMilliseconds = 50;
const chromiumStartupTimeoutMilliseconds = 60_000;
const mime = new Map([
  [".html", "text/html; charset=utf-8"],
  [".js", "text/javascript; charset=utf-8"],
  [".css", "text/css; charset=utf-8"],
  [".svg", "image/svg+xml"],
  [".png", "image/png"],
]);

const standardChromiumCandidates = [
  "/usr/sbin/chromium",
  "/usr/bin/chromium",
  "/usr/bin/chromium-browser",
  "/usr/bin/google-chrome",
  "/usr/bin/google-chrome-stable",
  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
  "/Applications/Chromium.app/Contents/MacOS/Chromium",
  join(homedir(), "Applications/Google Chrome.app/Contents/MacOS/Google Chrome"),
  join(homedir(), "Applications/Chromium.app/Contents/MacOS/Chromium"),
];
const chromiumCommands = [
  "chromium",
  "chromium-browser",
  "google-chrome",
  "google-chrome-stable",
  "chrome",
  "chrome.exe",
];

const inheritedChromiumDbusAddresses = new Set([
  "DBUS_SESSION_BUS_ADDRESS",
  "DBUS_SYSTEM_BUS_ADDRESS",
]);

export const createChromiumChildEnvironment = (environment = process.env) =>
  Object.fromEntries(
    Object.entries(environment).filter(([name]) => !inheritedChromiumDbusAddresses.has(name)),
  );

const executable = async (candidate) => {
  try {
    const info = await stat(candidate);
    if (!info.isFile()) return false;
    await access(candidate, constants.X_OK);
    return true;
  } catch {
    return false;
  }
};

const chromiumDiscoveryFailure = (reason, attempted) => {
  const pathValue = process.env.PATH ?? "";
  return new Error(
    `${reason}\nChromium remains required for M9 acceptance.` +
    `\nAttempted executables:\n- ${attempted.join("\n- ")}` +
    `\nPATH=${pathValue || "<empty>"}`,
  );
};

export const discoverChromium = async () => {
  const override = process.env.CHROMIUM_PATH?.trim();
  if (override) {
    const candidate = isAbsolute(override) ? normalize(override) : resolve(override);
    if (await executable(candidate)) return candidate;
    throw chromiumDiscoveryFailure(
      `CHROMIUM_PATH was set but is not an accessible executable: ${candidate}`,
      [candidate],
    );
  }

  // Conformance installs the lockfile-pinned Playwright browser. Prefer that
  // exact executable when it is present so retained raw captures do not
  // silently depend on whichever ambient Chrome build the host provides.
  const pinnedPlaywrightChromium = playwrightChromium.executablePath();
  if (await executable(pinnedPlaywrightChromium)) return pinnedPlaywrightChromium;

  const pathDirectories = (process.env.PATH ?? "")
    .split(delimiter)
    .filter(Boolean);
  const pathCandidates = pathDirectories.flatMap((directory) =>
    chromiumCommands.map((command) => resolve(directory, command))
  );
  const attempted = [...new Set([...standardChromiumCandidates, ...pathCandidates])];
  for (const candidate of attempted) {
    if (await executable(candidate)) return candidate;
  }
  throw chromiumDiscoveryFailure("No Chromium or Chrome executable was found.", attempted);
};

const allocateLoopbackPort = async () => {
  const reservation = createNetServer();
  await new Promise((resolveListen, rejectListen) => {
    reservation.once("error", rejectListen);
    reservation.listen(0, "127.0.0.1", resolveListen);
  });
  const address = reservation.address();
  if (!address || typeof address === "string") {
    reservation.close();
    throw new Error("Could not allocate an explicit Chromium debugging port.");
  }
  const port = address.port;
  await new Promise((resolveClose, rejectClose) => reservation.close((error) => error ? rejectClose(error) : resolveClose()));
  return port;
};

const serve = async (root) => {
  const base = resolve(root);
  const server = createServer(async (request, response) => {
    try {
      const url = new URL(request.url ?? "/", "http://localhost");
      const relative = url.pathname === "/" ? "index.html" : url.pathname.slice(1);
      const path = resolve(base, normalize(relative));
      if (path !== base && !path.startsWith(base + sep)) throw new Error("unsafe path");
      const info = await stat(path);
      if (!info.isFile()) throw new Error("not a file");
      response.writeHead(200, { "content-type": mime.get(extname(path)) ?? "application/octet-stream" });
      response.end(await readFile(path));
    } catch {
      response.writeHead(404);
      response.end("not found");
    }
  });
  await new Promise((resolveListen) => server.listen(0, "127.0.0.1", resolveListen));
  return { server, port: server.address().port };
};

class Cdp {
  constructor(url) {
    this.nextId = 1;
    this.pending = new Map();
    this.socket = new WebSocket(url);
    this.ready = new Promise((resolveReady, rejectReady) => {
      this.socket.addEventListener("open", resolveReady, { once: true });
      this.socket.addEventListener("error", rejectReady, { once: true });
    });
    this.socket.addEventListener("message", (event) => {
      const message = JSON.parse(event.data);
      if (!message.id) return;
      const pending = this.pending.get(message.id);
      if (!pending) return;
      this.pending.delete(message.id);
      if (message.error) pending.reject(new Error(message.error.message));
      else pending.resolve(message.result);
    });
  }

  async send(method, params = {}) {
    await this.ready;
    const id = this.nextId++;
    const result = new Promise((resolveResult, rejectResult) => {
      this.pending.set(id, { resolve: resolveResult, reject: rejectResult });
    });
    this.socket.send(JSON.stringify({ id, method, params }));
    return result;
  }

  close() { this.socket.close(); }
}

const evaluate = async (cdp, expression) => {
  const result = await cdp.send("Runtime.evaluate", {
    expression,
    awaitPromise: true,
    returnByValue: true,
  });
  if (result.exceptionDetails) throw new Error(result.exceptionDetails.text ?? "browser evaluation failed");
  return result.result.value;
};

const waitForShell = async (cdp) => {
  for (let attempt = 0; attempt < 120; attempt += 1) {
    if (await evaluate(cdp, "Boolean(document.querySelector('#unified-tactical-workspace'))")) return;
    await delay(50);
  }
  throw new Error("Chromium did not mount the production tactical shell.");
};

const collectGeometry = () => {
  const query = (selector) => document.querySelector(selector);
  const rounded = (value) => Math.round(value * 1000) / 1000;
  const rect = (element) => {
    const value = element.getBoundingClientRect();
    return {
      x: rounded(value.x), y: rounded(value.y), width: rounded(value.width), height: rounded(value.height),
      right: rounded(value.right), bottom: rounded(value.bottom),
    };
  };
  const intersects = (a, b) =>
    Math.min(a.right, b.right) - Math.max(a.x, b.x) > 0.5 &&
    Math.min(a.bottom, b.bottom) - Math.max(a.y, b.y) > 0.5;
  const shell = query("#unified-tactical-workspace");
  const toolbar = query(".tactical-compact-toolbar");
  const frame = query(".tactical-layout-frame");
  const workscreen = query("#tactical-workscreen-region");
  const svg = query("#persistent-tactical-svg");
  const left = query("#tactical-sidebar-left");
  const right = query("#tactical-sidebar-right");
  const bottom = query("#tactical-bottom-panel");
  const timeline = query('[aria-label="Unified tactical timeline"]');
  const timelineLegend = query(".tactical-timeline-channel-legend");
  const timelineTransport = query(".tactical-transport");
  const timelineRuler = query(".tactical-time-ruler");
  const timelineCursor = query(".tactical-time-cursor");
  const timelineLanes = query(".tactical-command-lanes");
  const tools = query(".editor-tools-panel");
  const elements = { shell, toolbar, frame, workscreen, svg, left, right, bottom, timeline, timelineLegend, timelineTransport, timelineRuler, timelineCursor, timelineLanes };
  if (Object.values(elements).some((element) => !element)) throw new Error("required production shell element missing");
  const rectangles = Object.fromEntries(Object.entries(elements).map(([key, element]) => [key, rect(element)]));
  const toolbarChildren = [...toolbar.querySelectorAll("button, summary")]
    .filter((element) => !element.closest(".tactical-panel-menu-items") && element.getClientRects().length > 0)
    .map((element) => ({
    label: element.getAttribute("aria-label") || element.textContent.trim(),
    rect: rect(element),
  }));
  const channels = [...timeline.querySelectorAll("[data-time-channel]")].map((element) => ({
    channel: element.getAttribute("data-time-channel"), rect: rect(element), text: element.textContent.trim(),
  }));
  const toolsRect = tools ? rect(tools) : null;
  const toolsHostRect = tools?.closest(".tactical-layout-panel-body") ? rect(tools.closest(".tactical-layout-panel-body")) : null;
  const panelBodies = [...shell.querySelectorAll(".tactical-layout-panel-body")].map((body) => ({
    panelId: body.closest("[data-panel-id]")?.getAttribute("data-panel-id"),
    rect: rect(body),
    children: [...body.children].map((child) => ({ className: child.className, position: getComputedStyle(child).position, rect: rect(child) })),
  }));
  const svgStyles = getComputedStyle(svg);
  return {
    viewport: { width: innerWidth, height: innerHeight, devicePixelRatio },
    document: {
      clientWidth: document.documentElement.clientWidth,
      scrollWidth: document.documentElement.scrollWidth,
      bodyScrollWidth: document.body.scrollWidth,
      mountClientWidth: query("#sir-replay-app").clientWidth,
      mountScrollWidth: query("#sir-replay-app").scrollWidth,
    },
    rectangles,
    styles: {
      toolsPosition: tools ? getComputedStyle(tools).position : null,
      leftDisplay: getComputedStyle(left).display,
      rightDisplay: getComputedStyle(right).display,
      leftVisibility: getComputedStyle(left).visibility,
      rightVisibility: getComputedStyle(right).visibility,
    },
    counts: {
      worksurfaceRoots: shell.querySelectorAll("[data-work-surface-root]").length,
      applicationLandmarks: shell.querySelectorAll("svg[role='application']").length,
    },
    visualSystem: {
      identity: svg.getAttribute("data-visual-system"),
      density: svg.getAttribute("data-visual-density"),
      motion: svg.getAttribute("data-motion"),
      layerOrder: svg.getAttribute("data-layer-order"),
      effectCount: Number(svg.getAttribute("data-effect-count")),
      effectLimit: Number(svg.getAttribute("data-effect-limit")),
      unitCount: Number(svg.getAttribute("data-visual-unit-count")),
      nodeEstimate: Number(svg.getAttribute("data-visual-node-estimate")),
      paintedLayerOrder: [...svg.querySelector("#persistent-scene-camera").children]
        .map((node) => node.getAttribute("data-scene-layer")).filter(Boolean).join(">"),
      effectKinds: [...svg.querySelectorAll("[data-effect-kind]")].map((node) => node.getAttribute("data-effect-kind")),
      effectLifecycles: [...svg.querySelectorAll("[data-effect-lifecycle]")].map((node) => node.getAttribute("data-effect-lifecycle")),
      workload: globalThis.__sirTacticalWorkload ?? null,
      tokens: Object.fromEntries([
        "--sir-canvas", "--sir-text", "--sir-grid", "--sir-focus",
        "--sir-intent", "--sir-impact", "--sir-suppression", "--sir-recovery", "--sir-rejected",
      ].map((name) => [name, svgStyles.getPropertyValue(name).trim()])),
    },
    channels,
    toolbarChildren,
    tools: { rect: toolsRect, hostRect: toolsHostRect },
    panelBodies,
    overlaps: {
      toolbarFrame: intersects(rectangles.toolbar, rectangles.frame),
      leftWorkscreen: intersects(rectangles.left, rectangles.workscreen),
      rightWorkscreen: intersects(rectangles.right, rectangles.workscreen),
      bottomWorkscreen: intersects(rectangles.bottom, rectangles.workscreen),
    },
    fieldFocusShare: rounded(rectangles.workscreen.width / rectangles.frame.width),
  };
};

const collectNarrowAccess = () => {
  const rect = (element) => {
    const value = element.getBoundingClientRect();
    return { x: value.x, right: value.right, y: value.y, bottom: value.bottom, width: value.width, height: value.height };
  };
  const selectors = [
    ["File menu", ".tactical-desktop-menu:nth-child(1) > summary"],
    ["Edit menu", ".tactical-desktop-menu:nth-child(2) > summary"],
    ["View menu", ".tactical-desktop-menu:nth-child(3) > summary"],
    ["Tools menu", ".tactical-desktop-menu:nth-child(4) > summary"],
    ["Simulation menu", ".tactical-desktop-menu:nth-child(5) > summary"],
    ["Help menu", ".tactical-desktop-menu:nth-child(6) > summary"],
  ];
  const controls = selectors.map(([label, selector]) => {
    const element = document.querySelector(selector);
    return { label, selector, exists: Boolean(element), rect: element ? rect(element) : null };
  });
  const toolbar = document.querySelector(".tactical-compact-toolbar");
  const toolbarRect = rect(toolbar);
  const menuBar = document.querySelector(".tactical-desktop-menu-bar");
  const menuBarRect = rect(menuBar);
  return {
    viewport: { width: innerWidth, height: innerHeight },
    document: {
      clientWidth: document.documentElement.clientWidth,
      scrollWidth: document.documentElement.scrollWidth,
      bodyScrollWidth: document.body.scrollWidth,
      mountClientWidth: document.querySelector("#sir-replay-app").clientWidth,
      mountScrollWidth: document.querySelector("#sir-replay-app").scrollWidth,
    },
    toolbarRect,
    toolbarScroll: { clientWidth: toolbar.clientWidth, scrollWidth: toolbar.scrollWidth, clientHeight: toolbar.clientHeight, scrollHeight: toolbar.scrollHeight },
    menuBarRect,
    menuBarScroll: { clientWidth: menuBar.clientWidth, scrollWidth: menuBar.scrollWidth, scrollLeft: menuBar.scrollLeft },
    workscreenRect: rect(document.querySelector("#tactical-workscreen-region")),
    timelineRect: rect(document.querySelector("#tactical-bottom-panel")),
    controls,
  };
};

const assertWide = (audit) => {
  const { rectangles: r, document: d } = audit;
  const containedBy = (child, parent) =>
    child.x >= parent.x - 1 && child.right <= parent.right + 1 &&
    child.y >= parent.y - 1 && child.bottom <= parent.bottom + 1;
  if (d.scrollWidth > d.clientWidth || d.mountScrollWidth > d.mountClientWidth) throw new Error("1440 shell has horizontal overflow");
  if (audit.styles.toolsPosition === "absolute" || audit.styles.toolsPosition === "fixed") throw new Error("Editor Tools panel escapes normal panel flow");
  if (audit.tools.rect && (audit.tools.rect.x < audit.tools.hostRect.x - 1 || audit.tools.rect.right > audit.tools.hostRect.right + 1)) throw new Error("Editor Tools panel leaves its registered panel body");
  for (const body of audit.panelBodies) {
    for (const child of body.children) {
      if (child.position === "absolute" || child.position === "fixed") throw new Error(`registered ${body.panelId} panel child escapes normal flow: ${child.className}`);
      if (child.rect.x < body.rect.x - 1 || child.rect.right > body.rect.right + 1) throw new Error(`registered ${body.panelId} panel child overflows its sidebar horizontally: ${child.className}`);
    }
  }
  if (audit.overlaps.toolbarFrame) throw new Error("1440 toolbar overlaps the Field Focus frame");
  for (const [name, helper] of [["left", r.left], ["right", r.right], ["bottom", r.bottom]]) {
    if ((helper.width > 0.5 || helper.height > 0.5) && !containedBy(helper, r.workscreen)) {
      throw new Error(`1440 ${name} helper leaves the map workscreen: ${JSON.stringify({ helper, workscreen: r.workscreen })}`);
    }
  }
  if ((r.left.width > 0.5 && !audit.overlaps.leftWorkscreen) || (r.right.width > 0.5 && !audit.overlaps.rightWorkscreen)) {
    throw new Error(`desktop side helpers no longer overlay the map: ${JSON.stringify(audit.overlaps)}`);
  }
  if (audit.fieldFocusShare < 0.95) throw new Error("live map no longer fills the Field Focus frame");
  if (r.bottom.bottom > audit.viewport.height + 1 || r.timeline.bottom > r.bottom.bottom + 1) throw new Error(`the expanded Field Focus timeline is clipped below the 1440×900 review viewport: ${JSON.stringify({ viewport: audit.viewport, shell: r.shell, toolbar: r.toolbar, frame: r.frame, workscreen: r.workscreen, bottom: r.bottom, timeline: r.timeline, timelineLegend: r.timelineLegend, timelineTransport: r.timelineTransport, timelineRuler: r.timelineRuler, timelineCursor: r.timelineCursor, timelineLanes: r.timelineLanes })}`);
  if (audit.counts.worksurfaceRoots !== 1 || audit.counts.applicationLandmarks !== 1) throw new Error("live shell is not a singleton workscreen");
  if (audit.channels.length < 4 || !["Authored", "Predicted", "Accepted", "Committed"].every((name) => audit.channels.some(({ channel }) => channel === name))) throw new Error("live timeline does not expose all real channels");
  if (audit.channels.some(({ rect: channel }) => channel.y < r.bottom.y || channel.bottom > r.bottom.bottom)) throw new Error("a real timeline channel is not visible inside the bottom panel");
  const clippedToolbarChildren = audit.toolbarChildren.filter(({ rect: child }) => child.x < r.toolbar.x - 1 || child.right > r.toolbar.right + 1 || child.y < r.toolbar.y - 1 || child.bottom > r.toolbar.bottom + 1);
  if (clippedToolbarChildren.length > 0) throw new Error(`compact toolbar clips a control at 1440px: ${JSON.stringify({ toolbar: r.toolbar, clippedToolbarChildren })}`);
};

const assertNarrow = (audit) => {
  const { document: d } = audit;
  if (d.clientWidth !== 320 || d.scrollWidth > d.clientWidth || d.bodyScrollWidth > d.clientWidth || d.mountScrollWidth > d.mountClientWidth) throw new Error(`320px shell has horizontal overflow: ${JSON.stringify(d)}`);
  if (audit.toolbarScroll.scrollWidth > audit.toolbarScroll.clientWidth || audit.toolbarScroll.scrollHeight > audit.toolbarScroll.clientHeight) throw new Error("320px toolbar clips wrapped controls");
  if (audit.workscreenRect.x < -1 || audit.workscreenRect.right > 321 || audit.timelineRect.x < -1 || audit.timelineRect.right > 321) throw new Error("workscreen or timeline leaves the 320px viewport");
  for (const control of audit.controls) {
    if (!control.exists) throw new Error(`missing narrow access control: ${control.label}`);
    const contentLeft = audit.menuBarRect.x - audit.menuBarScroll.scrollLeft;
    const contentRight = contentLeft + audit.menuBarScroll.scrollWidth;
    if (control.rect.x < contentLeft - 1 || control.rect.right > contentRight + 1) throw new Error(`narrow access control leaves the reachable menu scroll surface: ${control.label} ${JSON.stringify(control.rect)}`);
    if (control.rect.width < 44 || control.rect.height < 44) throw new Error(`narrow access control is smaller than 44px: ${control.label} ${JSON.stringify(control.rect)}`);
  }
};

const rectangleMeasurementNames = new Set(["x", "y", "width", "height", "right", "bottom"]);
const rectangleMeasurementParents = new Set([
  "rect",
  "toolbarRect",
  "workscreenRect",
  "timelineRect",
  "hostRect",
]);
const criticalGeometryTolerance = new Map([
  ["wide.rectangles.frame.width", 0.5],
  ["wide.rectangles.workscreen.width", 0.5],
]);
const toolbarScrollMeasurements = new Set(["clientWidth", "scrollWidth", "clientHeight", "scrollHeight"]);
const geometryComparison = (path) => {
  // Browser/font runtimes legitimately reflow text controls, so raw rectangle
  // coordinates are evidence rather than a cross-host golden snapshot. Both
  // audits still pass assertWide/assertNarrow, which reject clipping, helpers
  // leaving the map, loss of map dominance, offscreen controls, and touch-target
  // failures. Scalable helper dimensions are evidence, not fixed budgets. Only
  // CSS-stable frame dimensions are compared across hosts, with subpixel tolerance. The narrow
  // toolbar's internal scroll extents are also font-layout measurements; their
  // no-clipping relationship is enforced independently by assertNarrow.
  const location = path.join(".");
  if (criticalGeometryTolerance.has(location)) {
    return { kind: "tolerance", epsilon: criticalGeometryTolerance.get(location) };
  }
  const name = path.at(-1);
  const parent = path.at(-2);
  const isRectangle = rectangleMeasurementParents.has(parent) || path.at(-3) === "rectangles";
  if (isRectangle && rectangleMeasurementNames.has(name)) return { kind: "host-geometry" };
  if (parent === "toolbarScroll" && toolbarScrollMeasurements.has(name)) return { kind: "host-geometry" };
  if (parent === "menuBarScroll" && ["clientWidth", "scrollWidth", "scrollLeft"].includes(name)) return { kind: "host-geometry" };
  if (name === "fieldFocusShare") return { kind: "host-geometry" };
  return { kind: "exact" };
};

const comparePortableAuditValue = (stored, live, path, mismatches) => {
  const location = path.length > 0 ? path.join(".") : "<root>";
  if (typeof stored === "number" && typeof live === "number") {
    const comparison = geometryComparison(path);
    if (comparison.kind === "host-geometry") return;
    const epsilon = comparison.kind === "tolerance" ? comparison.epsilon : null;
    if (epsilon === null ? !Object.is(stored, live) : Math.abs(stored - live) > epsilon) {
      const tolerance = epsilon === null ? "exact" : `±${epsilon}`;
      mismatches.push(`${location}: stored=${stored}, live=${live}, delta=${Math.abs(stored - live)}, allowed=${tolerance}`);
    }
    return;
  }
  if (Array.isArray(stored) || Array.isArray(live)) {
    if (!Array.isArray(stored) || !Array.isArray(live) || stored.length !== live.length) {
      mismatches.push(`${location}: stored/live array shape differs`);
      return;
    }
    for (let index = 0; index < stored.length; index += 1) {
      comparePortableAuditValue(stored[index], live[index], [...path, String(index)], mismatches);
    }
    return;
  }
  if (stored !== null && live !== null && typeof stored === "object" && typeof live === "object") {
    const storedKeys = Object.keys(stored);
    const liveKeys = Object.keys(live);
    if (JSON.stringify(storedKeys) !== JSON.stringify(liveKeys)) {
      mismatches.push(`${location}: stored/live object keys differ`);
      return;
    }
    for (const key of storedKeys) comparePortableAuditValue(stored[key], live[key], [...path, key], mismatches);
    return;
  }
  if (!Object.is(stored, live)) {
    mismatches.push(`${location}: stored=${JSON.stringify(stored)}, live=${JSON.stringify(live)}`);
  }
};

export const assertPortableReviewMetrics = ({ storedWide, storedNarrow, liveWide, liveNarrow }) => {
  assertWide(storedWide);
  assertNarrow(storedNarrow);
  assertWide(liveWide);
  assertNarrow(liveNarrow);

  const mismatches = [];
  comparePortableAuditValue(storedWide, liveWide, ["wide"], mismatches);
  comparePortableAuditValue(storedNarrow, liveNarrow, ["narrow"], mismatches);
  if (mismatches.length > 0) {
    const displayed = mismatches.slice(0, 20);
    const remainder = mismatches.length > displayed.length
      ? `\n... and ${mismatches.length - displayed.length} more mismatch(es)`
      : "";
    throw new Error(`stored/live audit mismatches (${mismatches.length}):\n- ${displayed.join("\n- ")}${remainder}`);
  }
};

export const auditPersistentWorkspaceBrowser = async ({ clientRoot = "artifacts/client", screenshotPath, prepareExpression, captureStyleText, reducedMotion = false } = {}) => {
  const chromiumExecutable = await discoverChromium();
  const { server, port } = await serve(clientRoot);
  const profile = await mkdtemp(join(tmpdir(), "sir-m9-chromium-"));
  const debugPort = await allocateLoopbackPort();
  const browserState = { exited: false, code: null, signal: null, error: null };
  const chromiumEnvironment = createChromiumChildEnvironment();
  const sanitizedDbusVariables = Object.keys(process.env).filter((name) => inheritedChromiumDbusAddresses.has(name));
  let browserStderr = "";
  const browser = spawn(chromiumExecutable, [
    "--headless=new", "--no-sandbox", "--disable-gpu", "--hide-scrollbars", "--lang=en-US",
    "--deterministic-mode",
    "--force-color-profile=srgb", "--num-raster-threads=1",
    "--disable-partial-raster", "--disable-oop-rasterization",
    "--disable-font-subpixel-positioning", "--disable-lcd-text", "--font-render-hinting=none",
    "--disable-dev-shm-usage", "--no-first-run", "--no-default-browser-check",
    "--disable-background-networking", "--disable-default-apps", "--disable-extensions",
    "--force-device-scale-factor=1", "--remote-debugging-address=127.0.0.1",
    `--remote-debugging-port=${debugPort}`, `--user-data-dir=${profile}`,
  ], { env: chromiumEnvironment, stdio: ["ignore", "ignore", "pipe"] });
  browser.stderr.on("data", (chunk) => {
    browserStderr = (browserStderr + chunk.toString("utf8")).slice(-16384);
  });
  browser.once("error", (error) => { browserState.error = error; });
  const browserExit = new Promise((resolveExit) => browser.once("exit", (code, signal) => {
    browserState.exited = true;
    browserState.code = code;
    browserState.signal = signal;
    resolveExit();
  }));
  const startupFailure = (message, cause) => {
    const causeText = cause ? ` Last probe: ${cause instanceof Error ? cause.message : String(cause)}.` : "";
    const processText = browserState.error
      ? ` Spawn error: ${browserState.error.message}.`
      : ` Browser exited=${browserState.exited}, code=${String(browserState.code)}, signal=${String(browserState.signal)}.`;
    const environmentText = sanitizedDbusVariables.length > 0
      ? ` Sanitized inherited DBus variables: ${sanitizedDbusVariables.join(", ")}.`
      : " No inherited DBus variables required sanitization.";
    const stderrText = browserStderr.trim() ? ` Chromium stderr (last 16384 bytes):\n${browserStderr.trim()}` : " Chromium stderr was empty.";
    return new Error(`${message}${causeText}${processText}${environmentText}${stderrText}`);
  };
  let cdp;
  try {
    let version;
    let lastProbeError;
    const maximumStartupAttempts = Math.ceil(
      chromiumStartupTimeoutMilliseconds / chromiumStartupProbeIntervalMilliseconds,
    );
    for (let attempt = 0; attempt < maximumStartupAttempts; attempt += 1) {
      if (browserState.error || browserState.exited) break;
      try {
        const response = await fetch(`http://127.0.0.1:${debugPort}/json/version`);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        version = await response.json();
        if (version.webSocketDebuggerUrl) break;
      } catch (error) {
        lastProbeError = error;
      }
      await delay(chromiumStartupProbeIntervalMilliseconds);
    }
    if (!version?.webSocketDebuggerUrl) {
      throw startupFailure(`Chromium did not expose CDP on explicit loopback port ${debugPort}.`, lastProbeError);
    }
    let page;
    try {
      const response = await fetch(`http://127.0.0.1:${debugPort}/json/new?${encodeURIComponent(`http://127.0.0.1:${port}/`)}`, { method: "PUT" });
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      page = await response.json();
      if (!page.webSocketDebuggerUrl) throw new Error("CDP page response omitted webSocketDebuggerUrl");
    } catch (error) {
      throw startupFailure(`Chromium CDP was available on port ${debugPort}, but page creation failed.`, error);
    }
    cdp = new Cdp(page.webSocketDebuggerUrl);
    await cdp.send("Runtime.enable");
    await cdp.send("Page.enable");
    await cdp.send("Emulation.setLocaleOverride", { locale: "en-US" });
    await cdp.send("Emulation.setTimezoneOverride", { timezoneId: "UTC" });
    await cdp.send("Emulation.setDeviceMetricsOverride", { width: 1440, height: 900, deviceScaleFactor: 1, mobile: false });
    await cdp.send("Emulation.setEmulatedMedia", {
      media: "screen",
      features: [
        { name: "prefers-reduced-motion", value: reducedMotion ? "reduce" : "no-preference" },
        { name: "prefers-color-scheme", value: "dark" },
        { name: "prefers-contrast", value: "no-preference" },
        { name: "forced-colors", value: "none" },
      ],
    });
    await cdp.send("Page.navigate", { url: `http://127.0.0.1:${port}/` });
    await waitForShell(cdp);
    if (captureStyleText) {
      await evaluate(cdp, `(() => { const style = document.createElement("style"); style.id = "sir-capture-input-style"; style.textContent = ${JSON.stringify(captureStyleText)}; document.head.appendChild(style); return document.fonts.ready; })()`);
    }
    await delay(250);
    if (prepareExpression) {
      try {
        await Promise.race([
          evaluate(cdp, prepareExpression),
          delay(20_000).then(() => { throw new Error("production review preparation exceeded 20 seconds"); }),
        ]);
      } catch (error) {
        const stage = await evaluate(cdp, "globalThis.__sirTacticalStage ?? 'unspecified'");
        throw new Error(`${error.message}; stage=${stage}`, { cause: error });
      }
      await delay(250);
    }
    const wide = await evaluate(cdp, `(${collectGeometry.toString()})()`);
    assertWide(wide);
    if (screenshotPath) {
      const screenshot = await cdp.send("Page.captureScreenshot", { format: "png", fromSurface: true, captureBeyondViewport: false });
      await writeFile(screenshotPath, Buffer.from(screenshot.data, "base64"));
    }
    await cdp.send("Emulation.setDeviceMetricsOverride", { width: 320, height: 900, deviceScaleFactor: 1, mobile: false });
    await delay(250);
    const narrow = await evaluate(cdp, `(${collectNarrowAccess.toString()})()`);
    assertNarrow(narrow);
    return {
      chromiumExecutable,
      chromium: await evaluate(cdp, "navigator.userAgent"),
      chromiumVersion: version.Browser,
      wide,
      narrow,
    };
  } finally {
    cdp?.close();
    browser.kill("SIGTERM");
    server.close();
    await Promise.race([browserExit, delay(2000)]);
    for (let attempt = 0; attempt < 5; attempt += 1) {
      try {
        await rm(profile, { recursive: true, force: true, maxRetries: 3, retryDelay: 100 });
        break;
      } catch (error) {
        if (attempt === 4) throw error;
        await delay(200);
      }
    }
  }
};
