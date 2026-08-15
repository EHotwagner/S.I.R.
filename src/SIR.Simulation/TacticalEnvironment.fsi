namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

type EnvironmentWorkloadCounters = { WorkloadSlots: int32; WorkloadVariantsInspected: int32; WorkloadFindings: int32; WorkloadFeatures: int32; DependencyEntriesInspected: int32; DependencyEntriesInvalidated: int32; WorkloadQueryCount: int32 }

[<RequireQualifiedAccess>]
module TacticalEnvironment =
    val maximumSlots: int32
    val maximumVariantsPerRole: int32
    val maximumFindings: int32
    val maximumTargetedActionCost: int32
    val defaultPermeability: EnvironmentFeatureKind -> EnvironmentFeatureState -> EnvironmentPermeability
    val validate: AuthoredPlot -> ParcelVariant list -> EnvironmentValidationFinding list
    val assemble: seed: uint64 -> AuthoredPlot -> ParcelVariant list -> Result<AssembledEnvironment, EnvironmentValidationFinding list>
    val observe: EnvironmentKnowledge -> AssembledEnvironment -> featureId: string -> EnvironmentObservation option
    val applyAction: EnvironmentKnowledge -> expectedContentIdentity: string -> featureId: string -> EnvironmentAction -> AssembledEnvironment -> Result<EnvironmentActionResult, EnvironmentActionFailure>
    val toSpatialWorld: rulesetIdentity: string -> EnvironmentKnowledge -> AssembledEnvironment -> ProjectedSpatialWorld
    val invalidateCache: EnvironmentActionResult -> SpatialCache -> SpatialCache * inspected: int32 * invalidated: int32
    val exteriorParcelSet: AuthoredPlot * ParcelVariant list
    val interiorBreachParcelSet: AuthoredPlot * ParcelVariant list
    val workload: seed: uint64 -> AuthoredPlot -> ParcelVariant list -> SpatialCache -> Result<EnvironmentWorkloadCounters, EnvironmentValidationFinding list>
