namespace SIR.Client

type ScenarioFamily =
    | FastStartTeaching
    | OpenFieldMovementFire
    | CoverDenseAssaultFlank
    | DoorBreachInteriorClear
    | SupportByFireSuppression
    | ArmoredAntiArmorResponse
    | MultiObjectiveWithdrawalReinforcement

type ExperienceMapSample =
    { Id: string
      Title: string
      Summary: string
      Family: ScenarioFamily
      Lesson: string
      Highlights: string list
      DesignNotes: string list
      MapText: string }

type ExperienceReplaySample =
    { Id: string
      Title: string
      Summary: string
      MapSampleId: string
      Ticks: int32 }

type ScenarioIdentity =
    { Engine: string
      Ruleset: string
      Content: string
      MapRevision: string
      ContentDigest: string }

type ScenarioForce =
    { UnitId: int32
      Capability: string
      Loadout: string
      InitialFacing: string
      InitialAttention: string
      InitialKnowledge: string }

type ScenarioPlan =
    { Side: string
      Name: string
      Steps: string list }

type ScenarioObjective =
    { Id: string
      Summary: string
      ZoneId: int32 option }

type ScenarioCheckpoint =
    { Tick: int32
      MinimumEvents: int32
      VisibleOutcome: string }

type ExperienceScenarioPackage =
    { SchemaVersion: int32
      CatalogVersion: string
      Identity: ScenarioIdentity
      Map: ExperienceMapSample
      Forces: ScenarioForce list
      Plans: ScenarioPlan list
      Objectives: ScenarioObjective list
      InitialKnowledge: string list
      Seed: uint64
      RandomAddress: string
      ExpectedCheckpoints: ScenarioCheckpoint list
      Replay: ExperienceReplaySample }

type ScenarioValidationError =
    | UnsupportedSchema of int32
    | StaleEngine of string
    | StaleRuleset of string
    | StaleContent of string
    | StaleMapRevision of string
    | StaleContentDigest of string
    | StaleReplayBinding of string
    | MissingScenarioContent of string
    | MalformedScenarioPackage of string

type ScenarioCatalogCost =
    { ScenarioCount: int32
      UnitCount: int32
      TerrainCount: int32
      EdgeCount: int32
      ZoneCount: int32
      CheckpointCount: int32
      ReplayTickCount: int32
      CanonicalBytes: int32 }

[<RequireQualifiedAccess>]
module ExperienceSamples =
    val maps: ExperienceMapSample list
    val replays: ExperienceReplaySample list
    val packages: ExperienceScenarioPackage list
    val canonical: ExperienceScenarioPackage -> string
    val digest: ExperienceScenarioPackage -> string
    val encodePackage: ExperienceScenarioPackage -> string
    val validate: ExperienceScenarioPackage -> Result<ExperienceScenarioPackage, ScenarioValidationError list>
    val importPackage: string -> Result<ExperienceScenarioPackage, ScenarioValidationError list>
    val stressPackage: unit -> ExperienceScenarioPackage
    val catalogFingerprint: unit -> string
    val catalogCost: ExperienceScenarioPackage list -> ScenarioCatalogCost
    val tryPackage: string -> ExperienceScenarioPackage option
    val tryMap: string -> ExperienceMapSample option
    val tryReplay: string -> ExperienceReplaySample option
    val editorState: ExperienceMapSample -> MapEditorState
    val simulator: ExperienceMapSample -> SimulatorHandoff option
    val replayFrames: ExperienceReplaySample -> InspectionProjection array
    val checkpointOutcomeSatisfied:
        ExperienceScenarioPackage -> InspectionProjection array -> ScenarioCheckpoint -> bool
    val runtimeFingerprint: ExperienceScenarioPackage -> string
