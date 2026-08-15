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
    let private u64 (value: uint64) = [| for shift in 0 .. 8 .. 56 -> byte (value >>> shift) |]
    let private text (value: string) = let bytes = Encoding.UTF8.GetBytes value in CanonicalEncoding.concatenate [ i32 bytes.Length; bytes ]
    let private boolByte value = [| if value then 1uy else 0uy |]
    let private list encode values = CanonicalEncoding.concatenate (i32 (List.length values) :: (values |> List.map encode))
    let private direction = function EnvironmentEdgeDirection.East -> 0 | EnvironmentEdgeDirection.South -> 1
    let private transform = function ParcelTransform.Identity -> 0 | ParcelTransform.Rotate90 -> 1 | ParcelTransform.Rotate180 -> 2 | ParcelTransform.Rotate270 -> 3
    let private kind = function EnvironmentFeatureKind.Door -> 0 | EnvironmentFeatureKind.Window -> 1 | EnvironmentFeatureKind.Wall -> 2 | EnvironmentFeatureKind.Cover -> 3
    let private state = function EnvironmentFeatureState.Intact -> 0 | EnvironmentFeatureState.Closed -> 1 | EnvironmentFeatureState.Open -> 2 | EnvironmentFeatureState.Damaged -> 3 | EnvironmentFeatureState.Breached -> 4 | EnvironmentFeatureState.Destroyed -> 5
    let private cell (value: EnvironmentCell) = CanonicalEncoding.concatenate [ i32 value.EnvironmentColumn; i32 value.EnvironmentRow ]
    let private edge (value: EnvironmentEdge) = CanonicalEncoding.concatenate [ cell value.EdgeCell; i32 (direction value.EdgeDirection) ]
    let private permeability (value: EnvironmentPermeability) = CanonicalEncoding.concatenate [ boolByte value.AllowsMovement; boolByte value.AllowsSight; boolByte value.AllowsProjectile; boolByte value.AllowsAreaEffect; boolByte value.AllowsSound; boolByte value.ProvidesCover; boolByte value.AllowsInteraction ]
    let private cover = function
        | None -> [| 0uy |]
        | Some value -> CanonicalEncoding.concatenate [ [| 1uy |]; text value.CoverMaterial; i32 value.CoverIntegrity; i32 value.CoverMaximumIntegrity; i32 value.CoverPenetrationResistance; list (Direction8.toCode >> int32 >> i32) (value.CoverProtectedDirections |> List.distinct |> List.sortBy Direction8.toCode) ]
    let private capability (value: EnvironmentCapability) = CanonicalEncoding.concatenate [ text value.DescriptorId; text value.DescriptorAction; i32 value.DescriptorCost; match value.RequiredKnowledgeFact with None -> [| 0uy |] | Some fact -> CanonicalEncoding.concatenate [ [| 1uy |]; text fact ] ]
    let private feature (value: EnvironmentFeature) = CanonicalEncoding.concatenate [ text value.EnvironmentFeatureId; i32 (kind value.EnvironmentKind); i32 (state value.EnvironmentState); edge value.EnvironmentEdge; list cell (value.EnvironmentFeatureCells |> List.distinct |> List.sort); permeability value.ModalityPermeability; cover value.DirectionalCover; list capability (value.CapabilityDescriptors |> List.sortBy _.DescriptorId); list text (value.QueryDependencyKeys |> List.distinct |> List.sort) ]
    let private placement (value: ParcelPlacement) = CanonicalEncoding.concatenate [ text value.PlacementSlotId; text value.PlacementVariantId; i32 (transform value.PlacementTransform); cell value.PlacementOrigin ]
    let private hex bytes = bytes |> Array.map (fun (value: byte) -> value.ToString("x2")) |> String.concat ""

    let canonicalBytes (environment: AssembledEnvironment) =
        CanonicalEncoding.concatenate
            [ i32 environment.EnvironmentSchemaVersion
              text environment.AssembledPlotId
              u64 environment.AssemblySeed
              list placement (environment.ParcelPlacements |> List.sortBy _.PlacementSlotId)
              list cell (environment.AssembledWalkableCells |> List.distinct |> List.sort)
              list cell (environment.AssembledObjectiveCells |> List.distinct |> List.sort)
              list feature (environment.EnvironmentFeatures |> List.sortBy _.EnvironmentFeatureId)
              text environment.EnvironmentAssemblyIdentity
              i64 environment.EnvironmentSpatialRevision
              i32 environment.AssemblyCostCounters.SlotsVisited
              i32 environment.AssemblyCostCounters.VariantsInspected
              i32 environment.AssemblyCostCounters.Selections
              i32 environment.AssemblyCostCounters.PlacedCells
              i32 environment.AssemblyCostCounters.PlacedFeatures ]

    let contentIdentity (environment: AssembledEnvironment) = { environment with EnvironmentContentIdentity = "" } |> canonicalBytes |> CanonicalHash.sha256 |> hex
    let withContentIdentity (environment: AssembledEnvironment) = { environment with EnvironmentContentIdentity = contentIdentity environment }
    let identityMatches (environment: AssembledEnvironment) = String.Equals(environment.EnvironmentContentIdentity, contentIdentity environment, StringComparison.Ordinal)

    let canonicalObservationBytes (observation: EnvironmentObservation) =
        CanonicalEncoding.concatenate
            [ i32 observation.ObservationSchemaVersion; text observation.ObservationFeatureId; i32 (kind observation.ObservationKind)
              match observation.ObservedState with None -> [| 0uy |] | Some value -> CanonicalEncoding.concatenate [ [| 1uy |]; i32 (state value) ]
              list capability (observation.AvailableCapabilities |> List.sortBy _.DescriptorId)
              i64 observation.ObservationSpatialRevision; text observation.ObservationKnowledgeIdentity; i64 observation.ObservationKnowledgeRevision ]
