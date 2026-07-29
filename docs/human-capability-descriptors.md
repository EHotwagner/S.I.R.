# Human capability descriptors

Status: implemented prototype contract  
Descriptor set: `sir.human-weapons` version `1`

Milestone 8 attaches explicit authored loadouts to units and executes ordinary
human weapons behind the generic Control ABI v1 request surface. It does not
add a request kind, weapon-facing direction, or browser-only execution model.

## Descriptor contract

`HumanWeaponCapabilityDescriptor` owns:

- stable capability ID and version;
- equipment and tactical role labels;
- point or area targeting;
- preparation and per-direction traverse ticks;
- maximum range;
- ammunition per resolution;
- interruption behavior; and
- the planning decision the role is intended to change.

`AuthoredUnitLoadout` records the unit, role, equipment labels, and exact
capability IDs. The planner includes those fields in worker transport and the
deterministic review artifact. An engagement absent from the selected unit's
loadout produces `SIR.PLAN.CAPABILITY.NOT_IN_LOADOUT`.

The version-1 set contains exactly these seven ordinary-human weapon roles:

| Capability ID | Equipment | Target | Planning or positional decision |
|---|---|---|---|
| `human.weapon.carbine` | Carbine | Point | Take a close route to exploit fast preparation |
| `human.weapon.rifle` | Rifle | Point | Hold a flexible mid-range firing position |
| `human.weapon.shotgun` | Shotgun | Point | Occupy an interior or doorway-adjacent cell |
| `human.weapon.marksman-rifle` | Marksman rifle | Point | Choose a distant stable sightline before preparing |
| `human.weapon.support` | Support weapon | Area | Prepare a covered position whose traverse reaches the threatened area |
| `human.weapon.grenade-launcher` | Grenade launcher | Area | Select an area behind cover while preserving launcher range |
| `human.weapon.anti-armor-launcher` | Anti-armor launcher | Point | Reserve a long preparation window against a hardened target |

These are alternatives, not an upgrade ladder.

## Generic execution

Controllers declare point or area engagements with the existing
`SetEngagement` request. The payload identifies a known-unit or referent target
and a versioned capability ID. `SetAttention` changes the same eight-way
attention authority used elsewhere, and `CancelAction` applies the
descriptor's interruption rule.

The shared capability executor validates the authored loadout, target shape,
range, attention alignment, ammunition, and descriptor identity. It advances
traverse and preparation on ticks, consumes descriptor-owned ammunition on
resolution, and emits point- or area-specific events. Point and area journals
produce per-tick state and event digests that replay verification reproduces
exactly.

The frozen ABI remains twelve generic request kinds and `Direction8` remains
eight values. No capability introduces a fourth directional control.

## Evidence and boundary

The native qualification runs all seven descriptors and proves:

- seven distinct planning-decision strings;
- point and area target contracts;
- non-zero attention traverse and completed preparation;
- role-specific ammunition consumption;
- lost versus preserved preparation on interruption;
- 32 deterministic point/area replay frames; and
- the unchanged twelve-request/eight-direction ABI surface.

The browser planner exposes the loadout and diagnoses capability mismatch, but
Milestone 8 does not claim the complete live editor → compiler → WASM → host →
kernel → browser path. That remains Milestone 9.

No arcane capability is present. Arcane descriptors require a separately
accepted parameter, target, timing, cost, interruption, observation, and
host-compatibility contract before they may enter this registry.
