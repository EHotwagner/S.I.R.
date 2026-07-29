namespace SIR.Domain

/// A stable unit identity shared by plans, simulation, replay, and projections.
[<Struct>]
type UnitId = private UnitId of int32

[<RequireQualifiedAccess>]
module UnitId =
    let create value = UnitId value
    let value (UnitId value) = value

/// The eight canonical compass directions, clockwise from north.
type Direction8 =
    | North
    | NorthEast
    | East
    | SouthEast
    | South
    | SouthWest
    | West
    | NorthWest

/// A stable, knowledge-filtered area referent. Its interpretation is owned by
/// the map/ruleset that issued it.
[<Struct>]
type AreaReferent = AreaReferent of int32

/// The three resolved directions disclosed for an authoritative unit.
type ResolvedOrientation =
    { MovementDirection: Direction8 option
      BodyFacing: Direction8
      AttentionDirection: Direction8 }

/// Durable body-facing intent used by plans and standard controllers.
type FacingIntent =
    | KeepFacing
    | FaceFixed of Direction8
    | FaceAlongMovement
    | FaceKnownUnit of UnitId

/// Durable attention intent used by plans and standard controllers.
type AttentionIntent =
    | KeepAttention
    | AttendFixed of Direction8
    | AttendRelativeToBody of Direction8
    | AttendAlongMovement
    | AttendKnownUnit of UnitId
    | AttendKnownArea of AreaReferent

/// Closed wire codes and deterministic direction operations.
[<RequireQualifiedAccess>]
module Direction8 =
    let all =
        [| North
           NorthEast
           East
           SouthEast
           South
           SouthWest
           West
           NorthWest |]

    let toCode direction =
        match direction with
        | North -> 0uy
        | NorthEast -> 1uy
        | East -> 2uy
        | SouthEast -> 3uy
        | South -> 4uy
        | SouthWest -> 5uy
        | West -> 6uy
        | NorthWest -> 7uy

    let tryFromCode code =
        match code with
        | 0uy -> Some North
        | 1uy -> Some NorthEast
        | 2uy -> Some East
        | 3uy -> Some SouthEast
        | 4uy -> Some South
        | 5uy -> Some SouthWest
        | 6uy -> Some West
        | 7uy -> Some NorthWest
        | _ -> None

    let delta direction =
        match direction with
        | North -> 0, -1
        | NorthEast -> 1, -1
        | East -> 1, 0
        | SouthEast -> 1, 1
        | South -> 0, 1
        | SouthWest -> -1, 1
        | West -> -1, 0
        | NorthWest -> -1, -1

    /// Resolves an octant relative to body facing. Relative north is forward.
    let relativeToBody body relative =
        let code =
            (int (toCode body) + int (toCode relative)) % all.Length

        all[code]

    /// Resolves a non-zero segment delta to its compass octant.
    let tryFromDelta columnDelta rowDelta =
        let sign value =
            if value < 0 then -1
            elif value > 0 then 1
            else 0

        match sign columnDelta, sign rowDelta with
        | 0, 0 -> None
        | 0, -1 -> Some North
        | 1, -1 -> Some NorthEast
        | 1, 0 -> Some East
        | 1, 1 -> Some SouthEast
        | 0, 1 -> Some South
        | -1, 1 -> Some SouthWest
        | -1, 0 -> Some West
        | -1, -1 -> Some NorthWest
        | _ -> None

    let defaultOrientation =
        { MovementDirection = None
          BodyFacing = North
          AttentionDirection = North }
