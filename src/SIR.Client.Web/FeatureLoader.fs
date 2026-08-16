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

let registryVersion = 1
let shell = FeatureId "shell"
let tacticalEnvironment = FeatureId "tactical-environment"
let rulesExplorer = FeatureId "rules-explorer"
let docs = FeatureId "docs"

let value (FeatureId value) = value

let identityFor feature =
    let logicalChunk =
        if feature = shell || feature = tacticalEnvironment then "app"
        elif feature = rulesExplorer then "RulesExplorer"
        elif feature = docs then "docs-feature"
        else invalidArg (nameof feature) ("Unregistered client feature: " + value feature)
    { RegistryVersion = registryVersion
      Feature = feature
      LogicalChunk = logicalChunk }

let initial =
    [ shell; tacticalEnvironment; rulesExplorer; docs ]
    |> List.map (fun feature ->
        let identity = identityFor feature
        feature,
        if feature = shell || feature = tacticalEnvironment then Loaded identity
        else Idle)
    |> Map.ofList

let stateFor (feature: FeatureId) (states: Map<FeatureId, LoadState>) =
    Map.tryFind feature states |> Option.defaultValue Idle

let beginLoad identity states = Map.add identity.Feature (Loading identity) states

let reconcile expected result current =
    match current, result with
    | Loading pending, Ok received when pending = expected && received = expected ->
        Applied(Loaded received)
    | Loading pending, Error failure when pending = expected ->
        Applied(Failed(expected, failure))
    | Loading pending, Ok received ->
        IgnoredStale(StaleIdentity(pending, received))
    | Loading pending, Error _ ->
        IgnoredStale(StaleIdentity(pending, expected))
    | _, Ok received -> IgnoredStale(StaleIdentity(expected, received))
    | _, Error _ -> IgnoredStale(StaleIdentity(expected, expected))

let describeIdentity identity =
    string identity.RegistryVersion
    + ":" + value identity.Feature
    + ":" + identity.LogicalChunk

let describeFailure = function
    | MissingChunk detail -> "missing-chunk: " + detail
    | Offline detail -> "offline: " + detail
    | ImportRejected detail -> "import-rejected: " + detail
    | StaleIdentity(expected, received) ->
        "stale-identity: expected " + describeIdentity expected
        + "; received " + describeIdentity received
