namespace SIR.Domain

/// One disclosed unit in a host-produced authoritative projection.
type QualifiedVisibleUnit =
    { UnitId: int32
      DisplayColumn: int32
      DisplayRow: int32
      Health: int32 }

/// Runtime-neutral handoff from the native match host to browser playback.
/// State and event identities cover only this disclosed projection. Complete
/// authoritative kernel identities remain server-side in replay verification,
/// so these fields cannot reveal changes to hidden state.
type AuthoritativeProjectionFrame =
    { Tick: int32
      ServerSequence: int64
      ProjectionRevision: int64
      VisibleUnits: QualifiedVisibleUnit array
      StateIdentity: byte array
      EventIdentity: byte array }
