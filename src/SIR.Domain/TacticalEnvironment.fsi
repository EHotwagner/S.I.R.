namespace SIR.Domain

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
type EnvironmentFeature = { EnvironmentFeatureId: string; EnvironmentKind: EnvironmentFeatureKind; EnvironmentState: EnvironmentFeatureState; EnvironmentEdge: EnvironmentEdge; ModalityPermeability: EnvironmentPermeability; DirectionalCover: DirectionalCover option; CapabilityDescriptors: EnvironmentCapability list; QueryDependencyKeys: string list }
type ParcelConnection = { ConnectionId: string; ConnectionCell: EnvironmentCell; ConnectionDirection: EnvironmentEdgeDirection; ConnectionRole: string }
type ParcelVariant = { ParcelVariantId: string; ParcelRole: string; ParcelWidth: int32; ParcelHeight: int32; ParcelWalkableCells: EnvironmentCell list; ParcelObjectiveCells: EnvironmentCell list; ParcelConnections: ParcelConnection list; ParcelFeatures: EnvironmentFeature list }
type PlotSlot = { PlotSlotId: string; PlotSlotRole: string; PlotSlotOrigin: EnvironmentCell; PlotSlotWidth: int32; PlotSlotHeight: int32; ConnectedPlotSlotIds: string list; PlotSlotRequiresRoute: bool }
type AuthoredPlot = { PlotSchemaVersion: int32; AuthoredPlotId: string; PlotWidth: int32; PlotHeight: int32; PlotSlots: PlotSlot list }
type ParcelPlacement = { PlacementSlotId: string; PlacementVariantId: string; PlacementTransform: ParcelTransform; PlacementOrigin: EnvironmentCell }
type AssemblyCounters = { SlotsVisited: int32; VariantsInspected: int32; Selections: int32; PlacedCells: int32; PlacedFeatures: int32 }
type AssembledEnvironment = { EnvironmentSchemaVersion: int32; AssembledPlotId: string; AssemblySeed: uint64; ParcelPlacements: ParcelPlacement list; AssembledWalkableCells: EnvironmentCell list; AssembledObjectiveCells: EnvironmentCell list; EnvironmentFeatures: EnvironmentFeature list; EnvironmentContentIdentity: string; EnvironmentSpatialRevision: int64; AssemblyCostCounters: AssemblyCounters }
[<RequireQualifiedAccess>]
type EnvironmentValidationCode = InvalidSchema | InvalidBounds | DuplicateId | DisconnectedSlot | ImpossibleFootprint | ConnectorMismatch | BlockedObjective | InvalidPermeability | CoverGap | UnreachableRoute | InvalidDependency
type EnvironmentValidationFinding = { ValidationCode: EnvironmentValidationCode; ValidationSubject: string; ValidationMessage: string }
[<RequireQualifiedAccess>]
type EnvironmentAction = Open | Close | Damage of amount: int32 | Breach of cost: int32 | Destroy
[<RequireQualifiedAccess>]
type EnvironmentActionFailure = MissingFeature | HiddenFeature | UnsupportedAction | InvalidCost | StaleContentIdentity
type EnvironmentActionCounters = { FeaturesInspected: int32; FeaturesChanged: int32; DependenciesEmitted: int32; PropagatedChanges: int32 }
type EnvironmentActionResult = { UpdatedEnvironment: AssembledEnvironment; ChangedQueryDependencies: Set<string>; ActionCostCounters: EnvironmentActionCounters }
type EnvironmentKnowledge = { EnvironmentKnowledgeIdentity: string; EnvironmentKnowledgeRevision: int64; KnownEnvironmentFeatureIds: Set<string>; KnownEnvironmentFacts: Set<string> }
type EnvironmentObservation = { ObservationSchemaVersion: int32; ObservationFeatureId: string; ObservationKind: EnvironmentFeatureKind; ObservedState: EnvironmentFeatureState option; AvailableCapabilities: EnvironmentCapability list; ObservationSpatialRevision: int64; ObservationKnowledgeIdentity: string; ObservationKnowledgeRevision: int64 }

[<RequireQualifiedAccess>]
module TacticalEnvironment =
    val schemaVersion: int32
    val canonicalBytes: AssembledEnvironment -> byte array
    val contentIdentity: AssembledEnvironment -> string
    val withContentIdentity: AssembledEnvironment -> AssembledEnvironment
    val identityMatches: AssembledEnvironment -> bool
    val canonicalObservationBytes: EnvironmentObservation -> byte array
