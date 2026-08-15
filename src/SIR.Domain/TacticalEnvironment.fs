namespace SIR.Domain

open System
open System.Text

[<Struct>]
type EnvironmentCell = { EnvironmentColumn: int32; EnvironmentRow: int32 }
[<RequireQualifiedAccess>]
type EnvironmentEdgeDirection = East | South
type EnvironmentEdge = { EdgeCell: EnvironmentCell; EdgeDirection: EnvironmentEdgeDirection }
[<RequireQualifiedAccess>]
type ParcelTransform = Identity | Rotate90 | Rotate180 | Rotate270
[<RequireQualifiedAccess>]
type EnvironmentFeatureKind = Door | Window | Wall | Cover
[<RequireQualifiedAccess>]
type EnvironmentFeatureState = Intact | Closed | Open | Damaged | Breached | Destroyed
[<RequireQualifiedAccess>]
type EnvironmentModality = Movement | Sight | Projectile | AreaEffect | Sound | Cover | Interaction of capabilityId: string
type EnvironmentPermeability = { AllowsMovement: bool; AllowsSight: bool; AllowsProjectile: bool; AllowsAreaEffect: bool; AllowsSound: bool; ProvidesCover: bool; AllowsInteraction: bool }
type DirectionalCover = { CoverMaterial: string; CoverIntegrity: int32; CoverMaximumIntegrity: int32; CoverPenetrationResistance: int32; CoverProtectedDirections: Direction8 list }
type EnvironmentCapability = { DescriptorId: string; DescriptorAction: string; DescriptorCost: int32; RequiredKnowledgeFact: string option }
type EnvironmentFeature = { EnvironmentFeatureId: string; EnvironmentKind: EnvironmentFeatureKind; EnvironmentState: EnvironmentFeatureState; EnvironmentEdge: EnvironmentEdge; EnvironmentFeatureCells: EnvironmentCell list; ModalityPermeability: EnvironmentPermeability; DirectionalCover: DirectionalCover option; CapabilityDescriptors: EnvironmentCapability list; QueryDependencyKeys: string list }
type ParcelConnection = { ConnectionId: string; ConnectionCell: EnvironmentCell; ConnectionDirection: EnvironmentEdgeDirection; ConnectionRole: string }
type ParcelVariant = { ParcelVariantId: string; ParcelRole: string; ParcelWidth: int32; ParcelHeight: int32; ParcelWalkableCells: EnvironmentCell list; ParcelObjectiveCells: EnvironmentCell list; ParcelConnections: ParcelConnection list; ParcelFeatures: EnvironmentFeature list }
type PlotSlot = { PlotSlotId: string; PlotSlotRole: string; PlotSlotOrigin: EnvironmentCell; PlotSlotWidth: int32; PlotSlotHeight: int32; ConnectedPlotSlotIds: string list; PlotSlotRequiresRoute: bool }
type AuthoredPlot = { PlotSchemaVersion: int32; AuthoredPlotId: string; PlotWidth: int32; PlotHeight: int32; PlotSlots: PlotSlot list }
type ParcelPlacement = { PlacementSlotId: string; PlacementVariantId: string; PlacementTransform: ParcelTransform; PlacementOrigin: EnvironmentCell }
type AssemblyCounters = { SlotsVisited: int32; VariantsInspected: int32; Selections: int32; PlacedCells: int32; PlacedFeatures: int32 }
type AssembledEnvironment = { EnvironmentSchemaVersion: int32; AssembledPlotId: string; AssemblySeed: uint64; ParcelPlacements: ParcelPlacement list; AssembledWalkableCells: EnvironmentCell list; AssembledObjectiveCells: EnvironmentCell list; EnvironmentFeatures: EnvironmentFeature list; EnvironmentAssemblyIdentity: string; EnvironmentContentIdentity: string; EnvironmentSpatialRevision: int64; AssemblyCostCounters: AssemblyCounters }
[<RequireQualifiedAccess>]
type EnvironmentValidationCode = InvalidSchema | InvalidBounds | DuplicateId | DisconnectedSlot | ImpossibleFootprint | ConnectorMismatch | BlockedObjective | InvalidPermeability | CoverGap | UnreachableRoute | InvalidDependency
type EnvironmentValidationFinding = { ValidationCode: EnvironmentValidationCode; ValidationSubject: string; ValidationMessage: string }
[<RequireQualifiedAccess>]
type EnvironmentAction = Open | Close | Damage of amount: int32 | Breach of cost: int32 | Destroy
[<RequireQualifiedAccess>]
type EnvironmentActionFailure = MissingFeature | HiddenFeature | UnsupportedAction | InvalidCost | StaleContentIdentity
type EnvironmentActionCounters = { FeaturesInspected: int32; FeaturesChanged: int32; DependenciesEmitted: int32; PropagatedChanges: int32 }
type EnvironmentActionResult = { UpdatedEnvironment: AssembledEnvironment; ChangedQueryDependencies: Set<string>; ActionCostCounters: EnvironmentActionCounters }
type EnvironmentKnowledge = { EnvironmentKnowledgeIdentity: string; EnvironmentKnowledgeRevision: int64; KnownEnvironmentFeatureIds: Set<string>; KnownEnvironmentStateFeatureIds: Set<string>; KnownEnvironmentFacts: Set<string> }
type EnvironmentObservation = { ObservationSchemaVersion: int32; ObservationFeatureId: string; ObservationKind: EnvironmentFeatureKind; ObservedState: EnvironmentFeatureState option; AvailableCapabilities: EnvironmentCapability list; ObservationSpatialRevision: int64; ObservationKnowledgeIdentity: string; ObservationKnowledgeRevision: int64 }

[<RequireQualifiedAccess>]
module TacticalEnvironment =
    let schemaVersion = 1
    let private i32 = CanonicalEncoding.int32LittleEndian
    let private i64 (value: int64) = [| for shift in 0 .. 8 .. 56 -> byte (value >>> shift) |]
    let private text (value: string) = let bytes = Encoding.UTF8.GetBytes value in CanonicalEncoding.concatenate [ i32 bytes.Length; bytes ]
    let private list encode values = CanonicalEncoding.concatenate (i32 (List.length values) :: (values |> List.map encode))
    let private direction = function EnvironmentEdgeDirection.East -> 0 | EnvironmentEdgeDirection.South -> 1
    let private transform = function ParcelTransform.Identity -> 0 | ParcelTransform.Rotate90 -> 1 | ParcelTransform.Rotate180 -> 2 | ParcelTransform.Rotate270 -> 3
    let private kind = function EnvironmentFeatureKind.Door -> 0 | EnvironmentFeatureKind.Window -> 1 | EnvironmentFeatureKind.Wall -> 2 | EnvironmentFeatureKind.Cover -> 3
    let private state = function EnvironmentFeatureState.Intact -> 0 | EnvironmentFeatureState.Closed -> 1 | EnvironmentFeatureState.Open -> 2 | EnvironmentFeatureState.Damaged -> 3 | EnvironmentFeatureState.Breached -> 4 | EnvironmentFeatureState.Destroyed -> 5
    let private orderedBy key values =
        let rec loop previous remaining =
            match remaining with
            | [] -> true
            | head :: tail when compare previous (key head) <= 0 -> loop (key head) tail
            | _ -> false
        match values with
        | [] | [ _ ] -> true
        | head :: tail -> loop (key head) tail
    let private sortedBy key values = if orderedBy key values then values else values |> List.sortBy key
    let private orderedDistinct values =
        let rec loop previous remaining =
            match remaining with
            | [] -> true
            | head :: tail when compare previous head < 0 -> loop head tail
            | _ -> false
        match values with
        | [] | [ _ ] -> true
        | head :: tail -> loop head tail
    let private sortedDistinct values = if orderedDistinct values then values else values |> List.distinct |> List.sort
    let private capability (value: EnvironmentCapability) = CanonicalEncoding.concatenate [ text value.DescriptorId; text value.DescriptorAction; i32 value.DescriptorCost; match value.RequiredKnowledgeFact with None -> [| 0uy |] | Some fact -> CanonicalEncoding.concatenate [ [| 1uy |]; text fact ] ]
    let private hex bytes = bytes |> Array.map (fun (value: byte) -> value.ToString("x2")) |> String.concat ""

    let canonicalBytes (environment: AssembledEnvironment) =
        // Preview assembly hashes the complete environment, including every cell and feature. Building
        // one byte array per scalar and repeatedly concatenating those arrays made the allocation shape
        // proportional to the field count as well as the payload. Keep the schema-v1 byte grammar exact,
        // but stream it into one growable buffer so a maximum editor preview has bounded copy overhead.
        let bytes = Collections.Generic.List<byte>(4096)
        let appendByte value = bytes.Add value
        let appendInt32 (value: int32) =
            appendByte (byte value)
            appendByte (byte (value >>> 8))
            appendByte (byte (value >>> 16))
            appendByte (byte (value >>> 24))
        let appendInt64 (value: int64) =
            for shift in 0 .. 8 .. 56 do appendByte (byte (value >>> shift))
        let appendUInt64 (value: uint64) =
            for shift in 0 .. 8 .. 56 do appendByte (byte (value >>> shift))
        let appendText (value: string) =
            let encoded = Encoding.UTF8.GetBytes value
            appendInt32 encoded.Length
            bytes.AddRange encoded
        let appendBool value = appendByte (if value then 1uy else 0uy)
        let appendList append values =
            appendInt32 (List.length values)
            values |> List.iter append
        let appendCell (value: EnvironmentCell) =
            appendInt32 value.EnvironmentColumn
            appendInt32 value.EnvironmentRow
        let appendEdge (value: EnvironmentEdge) =
            appendCell value.EdgeCell
            appendInt32 (direction value.EdgeDirection)
        let appendPermeability (value: EnvironmentPermeability) =
            appendBool value.AllowsMovement
            appendBool value.AllowsSight
            appendBool value.AllowsProjectile
            appendBool value.AllowsAreaEffect
            appendBool value.AllowsSound
            appendBool value.ProvidesCover
            appendBool value.AllowsInteraction
        let appendCover = function
            | None -> appendByte 0uy
            | Some value ->
                appendByte 1uy
                appendText value.CoverMaterial
                appendInt32 value.CoverIntegrity
                appendInt32 value.CoverMaximumIntegrity
                appendInt32 value.CoverPenetrationResistance
                value.CoverProtectedDirections
                |> sortedBy Direction8.toCode
                |> List.distinct
                |> appendList (Direction8.toCode >> int32 >> appendInt32)
        let appendCapability (value: EnvironmentCapability) =
            appendText value.DescriptorId
            appendText value.DescriptorAction
            appendInt32 value.DescriptorCost
            match value.RequiredKnowledgeFact with
            | None -> appendByte 0uy
            | Some fact -> appendByte 1uy; appendText fact
        let appendFeature (value: EnvironmentFeature) =
            appendText value.EnvironmentFeatureId
            appendInt32 (kind value.EnvironmentKind)
            appendInt32 (state value.EnvironmentState)
            appendEdge value.EnvironmentEdge
            value.EnvironmentFeatureCells |> sortedDistinct |> appendList appendCell
            appendPermeability value.ModalityPermeability
            appendCover value.DirectionalCover
            value.CapabilityDescriptors |> sortedBy _.DescriptorId |> appendList appendCapability
            value.QueryDependencyKeys |> sortedDistinct |> appendList appendText
        let appendPlacement (value: ParcelPlacement) =
            appendText value.PlacementSlotId
            appendText value.PlacementVariantId
            appendInt32 (transform value.PlacementTransform)
            appendCell value.PlacementOrigin

        appendInt32 environment.EnvironmentSchemaVersion
        appendText environment.AssembledPlotId
        appendUInt64 environment.AssemblySeed
        environment.ParcelPlacements |> sortedBy _.PlacementSlotId |> appendList appendPlacement
        environment.AssembledWalkableCells |> sortedDistinct |> appendList appendCell
        environment.AssembledObjectiveCells |> sortedDistinct |> appendList appendCell
        environment.EnvironmentFeatures |> sortedBy _.EnvironmentFeatureId |> appendList appendFeature
        appendText environment.EnvironmentAssemblyIdentity
        appendInt64 environment.EnvironmentSpatialRevision
        appendInt32 environment.AssemblyCostCounters.SlotsVisited
        appendInt32 environment.AssemblyCostCounters.VariantsInspected
        appendInt32 environment.AssemblyCostCounters.Selections
        appendInt32 environment.AssemblyCostCounters.PlacedCells
        appendInt32 environment.AssemblyCostCounters.PlacedFeatures
        bytes.ToArray()

    let contentIdentity (environment: AssembledEnvironment) = { environment with EnvironmentContentIdentity = "" } |> canonicalBytes |> CanonicalHash.sha256 |> hex
    let withContentIdentity (environment: AssembledEnvironment) = { environment with EnvironmentContentIdentity = contentIdentity environment }
    let identityMatches (environment: AssembledEnvironment) = String.Equals(environment.EnvironmentContentIdentity, contentIdentity environment, StringComparison.Ordinal)

    let canonicalObservationBytes (observation: EnvironmentObservation) =
        CanonicalEncoding.concatenate
            [ i32 observation.ObservationSchemaVersion; text observation.ObservationFeatureId; i32 (kind observation.ObservationKind)
              match observation.ObservedState with None -> [| 0uy |] | Some value -> CanonicalEncoding.concatenate [ [| 1uy |]; i32 (state value) ]
              list capability (observation.AvailableCapabilities |> sortedBy _.DescriptorId)
              i64 observation.ObservationSpatialRevision; text observation.ObservationKnowledgeIdentity; i64 observation.ObservationKnowledgeRevision ]
