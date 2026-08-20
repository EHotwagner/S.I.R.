import { readFile } from "node:fs/promises";
import { access } from "node:fs/promises";
import { chromium as playwrightChromium } from "@playwright/test";
import {
  assertPortableReviewMetrics,
  createChromiumChildEnvironment,
  discoverChromium,
} from "./lib/persistent-workspace-browser-audit.mjs";

const manifest = JSON.parse(
  await readFile("docs/assets/persistent-workspace-m9-review/manifest.json", "utf8"),
);
const clone = (value) => structuredClone(value);
const expectFailure = (action, expectedDiagnostic, message) => {
  try {
    action();
  } catch (error) {
    if (error instanceof Error && error.message.includes(expectedDiagnostic)) return;
    throw new Error(`${message} Unexpected diagnostic: ${error instanceof Error ? error.message : String(error)}`);
  }
  throw new Error(message);
};

const storedWide = clone(manifest.fieldFocus);
const storedNarrow = clone(manifest.narrow400PercentEquivalent);
const hostDifferentWide = clone(storedWide);
const hostDifferentNarrow = clone(storedNarrow);
hostDifferentWide.rectangles.shell.height -= 11.188;
hostDifferentWide.toolbarChildren[0].rect.x += 87.89;
hostDifferentWide.toolbarChildren[0].rect.width -= 10.25;
hostDifferentWide.toolbarChildren[0].rect.right =
  hostDifferentWide.toolbarChildren[0].rect.x + hostDifferentWide.toolbarChildren[0].rect.width;
hostDifferentWide.channels[0].rect.y += 0.75;
hostDifferentNarrow.controls[0].rect.x += 0.125;
hostDifferentNarrow.toolbarScroll.scrollWidth -= 10;

if (
  JSON.stringify(storedWide) === JSON.stringify(hostDifferentWide) ||
  JSON.stringify(storedNarrow) === JSON.stringify(hostDifferentNarrow)
) {
  throw new Error("Portability fixture did not differ from the stored raw Chromium metrics.");
}

assertPortableReviewMetrics({
  storedWide,
  storedNarrow,
  liveWide: hostDifferentWide,
  liveNarrow: hostDifferentNarrow,
});

const semanticallyDifferentWide = clone(hostDifferentWide);
semanticallyDifferentWide.counts.worksurfaceRoots = 2;
expectFailure(
  () => assertPortableReviewMetrics({
    storedWide,
    storedNarrow,
    liveWide: semanticallyDifferentWide,
    liveNarrow: hostDifferentNarrow,
  }),
  "live shell is not a singleton workscreen",
  "Portable review metrics accepted a non-singleton worksurface.",
);

const overflowingNarrow = clone(hostDifferentNarrow);
overflowingNarrow.document.scrollWidth = 321;
expectFailure(
  () => assertPortableReviewMetrics({
    storedWide,
    storedNarrow,
    liveWide: hostDifferentWide,
    liveNarrow: overflowingNarrow,
  }),
  "320px shell has horizontal overflow",
  "Portable review metrics accepted narrow horizontal overflow.",
);

const assertAuditMutationRejected = (mutate, expectedDiagnostic, message) => {
  const liveWide = clone(hostDifferentWide);
  const liveNarrow = clone(hostDifferentNarrow);
  mutate(liveWide, liveNarrow);
  expectFailure(
    () => assertPortableReviewMetrics({ storedWide, storedNarrow, liveWide, liveNarrow }),
    expectedDiagnostic,
    message,
  );
};

assertAuditMutationRejected(
  (wide) => wide.toolbarChildren.pop(),
  "wide.toolbarChildren",
  "Portable review metrics accepted a removed toolbar child.",
);
assertAuditMutationRejected(
  (wide) => wide.panelBodies.pop(),
  "wide.panelBodies",
  "Portable review metrics accepted a removed panel body.",
);
assertAuditMutationRejected(
  (wide) => { wide.channels[0].text += " changed"; },
  "wide.channels.0.text",
  "Portable review metrics accepted changed timeline text.",
);
assertAuditMutationRejected(
  (wide) => { wide.styles.leftDisplay = `${wide.styles.leftDisplay}-changed`; },
  "wide.styles.leftDisplay",
  "Portable review metrics accepted changed computed style semantics.",
);
assertAuditMutationRejected(
  (_wide, narrow) => { narrow.controls[0].selector = ".changed-control"; },
  "narrow.controls.0.selector",
  "Portable review metrics accepted changed narrow-control identity.",
);
assertAuditMutationRejected(
  (wide) => {
    wide.rectangles.left.x = wide.rectangles.workscreen.x - 2;
  },
  "left helper leaves the map workscreen",
  "Portable review metrics accepted a helper outside the map workscreen.",
);
assertAuditMutationRejected(
  (wide) => { wide.toolbarChildren[0].rect.right = wide.rectangles.toolbar.right + 2; },
  "compact toolbar clips a control",
  "Portable review metrics accepted toolbar clipping.",
);
assertAuditMutationRejected(
  (wide) => { wide.overlaps.toolbarFrame = true; },
  "1440 toolbar overlaps",
  "Portable review metrics accepted toolbar/map overlap.",
);
assertAuditMutationRejected(
  (wide) => { wide.overlaps.leftWorkscreen = false; },
  "side helpers no longer overlay the map",
  "Portable review metrics accepted a desktop side helper outside the map overlay layer.",
);
assertAuditMutationRejected(
  (_wide, narrow) => { narrow.controls[0].rect.width = 43; },
  "narrow access control is smaller than 44px",
  "Portable review metrics accepted an undersized touch target.",
);

const inheritedEnvironment = {
  PATH: "/portable/bin",
  DBUS_SESSION_BUS_ADDRESS: "unknown-runner-address",
  DBUS_SYSTEM_BUS_ADDRESS: "another-unknown-runner-address",
  DBUS_STARTER_BUS_TYPE: "session",
};
const childEnvironment = createChromiumChildEnvironment(inheritedEnvironment);
if (
  childEnvironment.PATH !== inheritedEnvironment.PATH ||
  "DBUS_SESSION_BUS_ADDRESS" in childEnvironment ||
  "DBUS_SYSTEM_BUS_ADDRESS" in childEnvironment ||
  childEnvironment.DBUS_STARTER_BUS_TYPE !== "session" ||
  inheritedEnvironment.DBUS_SESSION_BUS_ADDRESS !== "unknown-runner-address"
) {
  throw new Error("Chromium child environment isolation did not narrowly omit inherited DBus addresses.");
}

const pinnedExecutable = playwrightChromium.executablePath();
let pinnedExecutablePresent = true;
try {
  await access(pinnedExecutable);
} catch {
  pinnedExecutablePresent = false;
}
if (pinnedExecutablePresent && await discoverChromium() !== pinnedExecutable) {
  throw new Error("Browser audit did not prefer the installed lockfile-pinned Playwright Chromium.");
}

console.log(
  "Persistent workspace browser-audit portability passed: installed lockfile-pinned Chromium wins over ambient host Chrome, scalable helper geometry and cross-host font/layout drift are accepted, map containment/dominance and semantic regressions fail closed, and inherited DBus variables are isolated from Chromium.",
);
