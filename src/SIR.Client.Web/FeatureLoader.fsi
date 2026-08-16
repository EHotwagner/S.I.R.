module SIR.Client.Web.FeatureLoader

[<StructuralEquality; StructuralComparison>]
type FeatureId = private FeatureId of string

[<StructuralEquality; StructuralComparison>]
type ChunkIdentity =
    { RegistryVersion: int
      Feature: FeatureId
      LogicalChunk: string }

type LoadFailure =
    | MissingChunk of string
    | Offline of string
    | ImportRejected of string
    | StaleIdentity of expected: ChunkIdentity * received: ChunkIdentity

type LoadState =
    | Idle
    | Loading of ChunkIdentity
    | Loaded of ChunkIdentity
    | Failed of ChunkIdentity * LoadFailure

type Message =
    | Request of FeatureId
    | ImportCompleted of expected: ChunkIdentity * result: Result<ChunkIdentity, LoadFailure>

type Reconciliation =
    | Applied of LoadState
    | IgnoredStale of LoadFailure

val registryVersion: int
val deliverySupport: FeatureId
val shell: FeatureId
val tacticalEnvironment: FeatureId
val rulesExplorer: FeatureId
val docs: FeatureId
val value: FeatureId -> string
val identityFor: FeatureId -> ChunkIdentity
val initial: Map<FeatureId, LoadState>
val stateFor: FeatureId -> Map<FeatureId, LoadState> -> LoadState
val beginLoad: ChunkIdentity -> Map<FeatureId, LoadState> -> Map<FeatureId, LoadState>
val reconcile: ChunkIdentity -> Result<ChunkIdentity, LoadFailure> -> LoadState -> Reconciliation
val describeFailure: LoadFailure -> string
