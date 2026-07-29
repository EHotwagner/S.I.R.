import fs from "node:fs";
import { CONTROL_ABI_V1 } from "../generated/control-abi-v1.mjs";

const fail = (message) => {
  throw new Error(`Control ABI v1: ${message}`);
};

const readU16 = (bytes, offset) =>
  bytes[offset] | (bytes[offset + 1] << 8);

const readU32 = (bytes, offset) =>
  (bytes[offset] |
    (bytes[offset + 1] << 8) |
    (bytes[offset + 2] << 16) |
    (bytes[offset + 3] << 24)) >>>
  0;

export function decodeControlAbiV1(bytes, kind = "output") {
  const { layout, limits, magic, sections, sectionFlags } = CONTROL_ABI_V1;
  const maximum = kind === "input" ? limits.inputBytes : limits.outputBytes;
  const expectedMagic = kind === "input" ? magic.input : magic.output;

  if (bytes.length < layout.headerBytes || bytes.length > maximum) {
    fail("envelope length is out of bounds");
  }
  if (
    new TextDecoder().decode(bytes.subarray(0, 4)) !== expectedMagic ||
    bytes[4] !== CONTROL_ABI_V1.major ||
    bytes[5] > CONTROL_ABI_V1.minor
  ) {
    fail("magic or version mismatch");
  }
  if (
    readU16(bytes, 6) !== layout.headerBytes ||
    readU32(bytes, 8) !== bytes.length ||
    readU16(bytes, 30) !== 0
  ) {
    fail("non-canonical header");
  }

  const count = readU16(bytes, 28);
  if (count > limits.sections) fail("too many sections");
  const known =
    kind === "input"
      ? new Set(Object.values(sections).filter((tag) => tag < 0x1000))
      : new Set([sections.OutputRequests]);

  let offset = layout.headerBytes;
  let previous = -1;
  const decodedSections = [];

  for (let index = 0; index < count; index += 1) {
    if (offset + layout.sectionHeaderBytes > bytes.length) {
      fail("truncated section");
    }
    const tag = readU16(bytes, offset);
    const flags = readU16(bytes, offset + 2);
    const length = readU32(bytes, offset + 4);
    const elements = readU16(bytes, offset + 8);
    const reserved = readU16(bytes, offset + 10);
    const payloadOffset = offset + layout.sectionHeaderBytes;

    if (
      (flags & ~sectionFlags.required) !== 0 ||
      reserved !== 0 ||
      elements > limits.elementsPerSection ||
      payloadOffset + length > bytes.length
    ) {
      fail("invalid section header");
    }
    if (tag <= previous) fail("section tags are not strictly ascending");
    if ((flags & sectionFlags.required) !== 0 && !known.has(tag)) {
      fail("unknown required section");
    }

    decodedSections.push({
      tag,
      required: (flags & sectionFlags.required) !== 0,
      elements,
      payload: bytes.slice(payloadOffset, payloadOffset + length),
    });
    previous = tag;
    offset = payloadOffset + length;
  }

  if (offset !== bytes.length) fail("trailing bytes");
  return {
    kind,
    minorVersion: bytes[5],
    tick: readU32(bytes, 12),
    unitId: readU32(bytes, 16),
    flags: readU32(bytes, 20),
    budget: readU32(bytes, 24),
    sections: decodedSections,
    bytes: bytes.slice(),
  };
}

if (process.argv[1] === new URL(import.meta.url).pathname) {
  const fixture = process.argv[2];
  if (!fixture) fail("usage: decode-control-abi-v1.mjs FIXTURE.hex");
  const hex = fs.readFileSync(fixture, "utf8").replaceAll(/\s/g, "");
  const bytes = Uint8Array.from(
    hex.match(/../g)?.map((pair) => Number.parseInt(pair, 16)) ?? [],
  );
  const decoded = decodeControlAbiV1(bytes);
  process.stdout.write(Buffer.from(decoded.bytes).toString("hex"));
}
