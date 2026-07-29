namespace SIR.Match

open System
open System.Security.Cryptography
open Wasmtime
open SIR.Domain
open SIR.ControlAbi
open SIR.Simulation

/// Public, deterministic limits pinned by a control execution profile.
type ControlExecutionProfile =
    { Identity: byte array
      FuelPerInvocation: uint64
      MaximumMemoryBytes: int64
      MaximumConfigurationBytes: int
      AllowedRequests: Set<RequestKind> }

/// Stable host-side invocation failures from Control ABI v1.
[<RequireQualifiedAccess>]
type ControlFailure =
    | ModuleRejection = 1
    | Trap = 2
    | FuelExhaustion = 3
    | InvalidOutputLength = 4
    | MalformedOutput = 5
    | ForbiddenRequest = 6
    | MemoryLimit = 7
    | HostServiceLimit = 8

type ControlGlobalValue =
    | I32 of int32
    | I64 of int64

type ControlInstanceSnapshot =
    { Memory: byte array
      MutableGlobals: (string * ControlGlobalValue) list
      WakeTick: int32 option
      FaultCount: int32 }

type ControlBudgetJournal =
    { FuelAllowance: uint64
      FuelConsumed: uint64
      MemoryBytes: int64 }

type ControlInvocationJournal =
    { Tick: int32
      UnitId: int32
      InputHash: byte array
      OutputHash: byte array option
      Requests: Request list
      Failure: ControlFailure option
      Budget: ControlBudgetJournal
      ModuleState: ControlInstanceSnapshot option
      ModuleStateHash: byte array }

type ControlInvocation =
    | Accepted of OutputMessage * ControlInvocationJournal
    | Failed of ControlFailure * ControlInvocationJournal
    | SleepingUntil of int32

/// A module compiled once for an execution profile and reusable by isolated stores.
type CompiledControlArtifact internal
    (
        engine: Engine,
        compiled: Module,
        artifactHash: byte array,
        profile: ControlExecutionProfile
    ) =
    member internal _.Engine = engine
    member internal _.Module = compiled
    member _.ArtifactHash = Array.copy artifactHash
    member _.Profile = profile

    interface IDisposable with
        member _.Dispose() =
            compiled.Dispose()
            engine.Dispose()

/// One unit's isolated store, memory, mutable module state, and wake schedule.
type ControlInstance internal
    (
        artifact: CompiledControlArtifact,
        unitId: int32,
        configuration: byte array,
        store: Store,
        instance: Instance,
        memory: Memory,
        decide: Function
    ) =
    let mutable wakeTick: int32 option = None
    let mutable faults = 0

    member _.Artifact = artifact
    member _.UnitId = unitId
    member _.Configuration = Array.copy configuration
    member internal _.Store = store
    member internal _.Instance = instance
    member internal _.Memory = memory
    member internal _.Decide = decide
    member internal _.WakeTick with get () = wakeTick and set value = wakeTick <- value
    member internal _.FaultCount with get () = faults and set value = faults <- value

    interface IDisposable with
        member _.Dispose() = store.Dispose()

[<RequireQualifiedAccess>]
module ControlHost =
    let private abiVersion = 0x0001_0000

    let defaultProfile =
        let identityText =
            "wasmtime=44.0.0;control-abi=1.0;fuel=10000;memory=131072;imports=none"

        { Identity =
            identityText
            |> Text.Encoding.UTF8.GetBytes
            |> CanonicalHash.sha256
          FuelPerInvocation = 10_000UL
          MaximumMemoryBytes = 131_072L
          MaximumConfigurationBytes = 4_096
          AllowedRequests =
            Enum.GetValues<RequestKind>() |> Set.ofArray }

    let private createEngine () =
        let config = new Config()
        config.WithFuelConsumption(true) |> ignore
        config.WithReferenceTypes(false) |> ignore
        config.WithBulkMemory(false) |> ignore
        config.WithSIMD(false) |> ignore
        config.WithRelaxedSIMD(false, false) |> ignore
        config.WithMultiValue(false) |> ignore
        config.WithMultiMemory(false) |> ignore
        config.WithMemory64(false) |> ignore
        config.WithWasmThreads(false) |> ignore
        config.WithTailCalls(false) |> ignore
        config.WithComponentModel(false) |> ignore
        new Engine(config)

    let private validateSnapshotSurface (bytes: byte array) =
        let mutable globals: bool array = [||]
        let mutable exportedGlobals = Set.empty<int>

        let readUleb start limit =
            let mutable offset = start
            let mutable value = 0
            let mutable shift = 0
            let mutable complete = false

            while not complete do
                if offset >= limit || shift > 28 then
                    invalidArg (nameof bytes) "Malformed WASM unsigned integer."

                let current = bytes[offset]
                offset <- offset + 1
                value <- value ||| (int (current &&& 0x7fuy) <<< shift)
                complete <- current &&& 0x80uy = 0uy
                shift <- shift + 7

            value, offset

        let skipLeb start limit =
            let mutable offset = start
            let mutable complete = false

            while not complete do
                if offset >= limit then
                    invalidArg (nameof bytes) "Malformed WASM constant expression."

                let current = bytes[offset]
                offset <- offset + 1
                complete <- current &&& 0x80uy = 0uy
            offset

        let skipConstantExpression start limit =
            let mutable offset = start
            let opcode =
                if offset >= limit then
                    invalidArg (nameof bytes) "Missing WASM global initializer."
                let value = bytes[offset]
                offset <- offset + 1
                value

            match opcode with
            | 0x41uy
            | 0x42uy -> offset <- skipLeb offset limit
            | 0x43uy -> offset <- offset + 4
            | 0x44uy -> offset <- offset + 8
            | _ ->
                invalidArg (nameof bytes) "The execution profile permits only numeric constant global initializers."

            if offset >= limit || bytes[offset] <> 0x0buy then
                invalidArg (nameof bytes) "Malformed WASM global initializer."

            offset <- offset + 1
            offset

        if bytes.Length < 8
           || bytes[0..3] <> [| 0uy; 0x61uy; 0x73uy; 0x6duy |] then
            invalidArg (nameof bytes) "The artifact is not a core WebAssembly module."

        let mutable offset = 8

        while offset < bytes.Length do
            let sectionId = bytes[offset]
            offset <- offset + 1
            let sectionLength, nextOffset = readUleb offset bytes.Length
            offset <- nextOffset
            let sectionEnd = offset + sectionLength

            if sectionEnd < offset || sectionEnd > bytes.Length then
                invalidArg (nameof bytes) "Malformed WASM section length."

            if sectionId = 6uy then
                let count, nextOffset = readUleb offset sectionEnd
                offset <- nextOffset
                let found = Array.zeroCreate<bool> count

                for index in 0 .. count - 1 do
                    if offset > sectionEnd - 2 then
                        invalidArg (nameof bytes) "Malformed WASM global declaration."

                    let valueType = bytes[offset]
                    offset <- offset + 1

                    if valueType <> 0x7fuy && valueType <> 0x7euy then
                        invalidArg (nameof bytes) "Mutable control globals must be i32 or i64."

                    let mutability = bytes[offset]
                    offset <- offset + 1

                    if mutability > 1uy then
                        invalidArg (nameof bytes) "Malformed WASM global mutability."

                    found[index] <- mutability = 1uy
                    offset <- skipConstantExpression offset sectionEnd

                globals <- found
            elif sectionId = 7uy then
                let count, nextOffset = readUleb offset sectionEnd
                offset <- nextOffset

                for _ in 1 .. count do
                    let nameLength, nextOffset = readUleb offset sectionEnd
                    offset <- nextOffset
                    offset <- offset + nameLength

                    if offset >= sectionEnd then
                        invalidArg (nameof bytes) "Malformed WASM export."

                    let kind = bytes[offset]
                    offset <- offset + 1
                    let index, nextOffset = readUleb offset sectionEnd
                    offset <- nextOffset

                    if kind = 3uy then
                        exportedGlobals <- Set.add index exportedGlobals

            offset <- sectionEnd

        globals
        |> Array.iteri (fun index isMutable ->
            if isMutable && not (Set.contains index exportedGlobals) then
                invalidArg
                    (nameof bytes)
                    "Every mutable global must be exported so authoritative snapshots can restore it.")

    /// Compiles an immutable core-WASM artifact once. Control ABI v1 permits no imports,
    /// which also makes ambient WASI unavailable rather than merely unconfigured.
    let compile (profile: ControlExecutionProfile) name (artifactBytes: byte array) =
        if profile.MaximumMemoryBytes <= 0L
           || profile.MaximumMemoryBytes % 65_536L <> 0L then
            invalidArg (nameof profile) "Maximum memory must be a positive whole number of WASM pages."

        validateSnapshotSurface artifactBytes
        let engine = createEngine ()

        try
            let compiled = Module.FromBytes(engine, name, artifactBytes)

            if compiled.Imports.Count <> 0 then
                compiled.Dispose()
                invalidArg (nameof artifactBytes) "Control ABI v1 modules cannot import functions, memories, WASI, or other ambient capabilities."

            new CompiledControlArtifact(
                engine,
                compiled,
                CanonicalHash.sha256 artifactBytes,
                profile
            )
        with _ ->
            engine.Dispose()
            reraise ()

    let private invokeI32 (instance: Instance) name =
        match instance.GetFunction(name) with
        | null -> invalidArg name $"The module does not export {name}."
        | fn ->
            match fn.Invoke() with
            | :? int as value -> value
            | value -> invalidArg name $"The {name} export returned {value}, not i32."

    let private checkedRange memorySize pointer capacity name =
        if pointer < 0
           || capacity < 0
           || int64 pointer > memorySize
           || int64 capacity > memorySize - int64 pointer then
            invalidArg name $"{name} is outside the instance memory."

    let private instantiateRaw
        (artifact: CompiledControlArtifact)
        unitId
        (configuration: byte array)
        =
        if unitId < 0 then invalidArg (nameof unitId) "Unit IDs must be non-negative."

        if configuration.Length > artifact.Profile.MaximumConfigurationBytes then
            invalidArg (nameof configuration) "Instance configuration exceeds the execution profile."

        let store = new Store(artifact.Engine)

        try
            store.SetLimits(
                Nullable artifact.Profile.MaximumMemoryBytes,
                Nullable 0u,
                Nullable 1L,
                Nullable 0L,
                Nullable 1L
            )
            store.Fuel <- artifact.Profile.FuelPerInvocation

            use linker = new Linker(artifact.Engine)
            let instance = linker.Instantiate(store, artifact.Module)

            if invokeI32 instance "sir_abi_version" <> abiVersion then
                invalidArg (nameof artifact) "The module does not implement Control ABI v1.0."

            let memory =
                match instance.GetMemory("memory") with
                | null -> invalidArg (nameof artifact) "The module does not export memory."
                | value -> value

            let memorySize = memory.GetLength()

            if memorySize > artifact.Profile.MaximumMemoryBytes then
                invalidArg (nameof artifact) "Initial module memory exceeds the execution profile."

            let inputPointer = invokeI32 instance "sir_input_ptr"
            let inputCapacity = invokeI32 instance "sir_input_capacity"
            let outputPointer = invokeI32 instance "sir_output_ptr"
            let outputCapacity = invokeI32 instance "sir_output_capacity"
            checkedRange memorySize inputPointer inputCapacity "input range"
            checkedRange memorySize outputPointer outputCapacity "output range"

            if inputCapacity < V1Constants.InputMaximumBytes
               || outputCapacity < V1Constants.OutputMaximumBytes then
                invalidArg (nameof artifact) "The module buffers are smaller than the v1 execution-profile limits."

            let inputEnd = int64 inputPointer + int64 inputCapacity
            let outputEnd = int64 outputPointer + int64 outputCapacity

            if int64 inputPointer < outputEnd && int64 outputPointer < inputEnd then
                invalidArg (nameof artifact) "Input and output ranges overlap."

            let decide =
                match instance.GetFunction("sir_decide") with
                | null -> invalidArg (nameof artifact) "The module does not export sir_decide."
                | value -> value

            new ControlInstance(
                artifact,
                unitId,
                configuration,
                store,
                instance,
                memory,
                decide
            )
        with _ ->
            store.Dispose()
            reraise ()

    let instantiate (artifact: CompiledControlArtifact) unitId configuration =
        instantiateRaw artifact unitId (Array.copy configuration)

    let private globalValue (value: obj | null) =
        match value with
        | :? int32 as value -> I32 value
        | :? int64 as value -> I64 value
        | value -> failwithf "Unsupported mutable global value %A." value

    let private boxedGlobalValue value : obj | null =
        match value with
        | I32 value -> box value
        | I64 value -> box value

    let private mutableGlobals (control: ControlInstance) =
            control.Instance.GetGlobals()
            |> Seq.choose (fun (struct (name, wasmGlobal)) ->
                if wasmGlobal.Mutability = Mutability.Mutable then
                    Some(name, wasmGlobal.GetValue() |> globalValue)
                else
                    None)
            |> Seq.sortBy fst
            |> Seq.toList

    let snapshot (control: ControlInstance) =
        let memorySize = control.Memory.GetLength()

        { Memory = control.Memory.GetSpan(0L, int memorySize).ToArray()
          MutableGlobals = mutableGlobals control
          WakeTick = control.WakeTick
          FaultCount = control.FaultCount }

    let private snapshotBytes snapshot =
        let globalBytes =
            snapshot.MutableGlobals
            |> List.collect (fun (name, value) ->
                let nameBytes = Text.Encoding.UTF8.GetBytes name

                let tag, bytes =
                    match value with
                    | I32 value -> 1uy, BitConverter.GetBytes value
                    | I64 value -> 2uy, BitConverter.GetBytes value

                [ CanonicalEncoding.int32LittleEndian nameBytes.Length
                  nameBytes
                  [| tag |]
                  bytes ])

        CanonicalEncoding.concatenate
            ([ CanonicalEncoding.int32LittleEndian snapshot.Memory.Length
               snapshot.Memory
               CanonicalEncoding.int32LittleEndian snapshot.MutableGlobals.Length ]
             @ globalBytes
             @ [ CanonicalEncoding.int32LittleEndian (snapshot.WakeTick |> Option.defaultValue -1)
                 CanonicalEncoding.int32LittleEndian snapshot.FaultCount ])

    let snapshotHash snapshot =
        snapshot |> snapshotBytes |> CanonicalHash.sha256

    /// Attaches a full resumable module checkpoint to an invocation journal.
    /// Ordinary ticks retain the cheaper state hash; replay checkpoint ticks call this.
    let checkpointJournal (control: ControlInstance) journal =
        let moduleState = snapshot control

        { journal with
            ModuleState = Some moduleState
            ModuleStateHash = snapshotHash moduleState }

    let private currentStateHash (control: ControlInstance) =
        let memoryLength = control.Memory.GetLength()
        let memoryHash =
            SHA256.HashData(control.Memory.GetSpan(0L, int memoryLength))

        let metadata: ControlInstanceSnapshot =
            { Memory = [||]
              MutableGlobals = mutableGlobals control
              WakeTick = control.WakeTick
              FaultCount = control.FaultCount }

        CanonicalEncoding.concatenate
            [ CanonicalEncoding.int32LittleEndian (int32 memoryLength)
              memoryHash
              snapshotBytes metadata ]
        |> CanonicalHash.sha256

    let resume
        (artifact: CompiledControlArtifact)
        unitId
        configuration
        snapshot
        =
        if int64 snapshot.Memory.Length > artifact.Profile.MaximumMemoryBytes then
            invalidArg (nameof snapshot) "Snapshot memory exceeds the execution profile."

        let control = instantiateRaw artifact unitId (Array.copy configuration)

        try
            let expectedGlobals =
                mutableGlobals control |> List.map fst |> Set.ofList
            let snapshotGlobals =
                snapshot.MutableGlobals |> List.map fst |> Set.ofList

            if snapshotGlobals.Count <> snapshot.MutableGlobals.Length
               || snapshotGlobals <> expectedGlobals then
                invalidArg
                    (nameof snapshot)
                    "Snapshot mutable globals do not exactly match the qualified artifact."

            let current = control.Memory.GetLength()

            if int64 snapshot.Memory.Length > current then
                let missing = int64 snapshot.Memory.Length - current
                let pages = (missing + 65_535L) / 65_536L
                control.Memory.Grow(pages) |> ignore

            ReadOnlySpan<byte>(snapshot.Memory).CopyTo(
                control.Memory.GetSpan(0L, snapshot.Memory.Length)
            )

            for name, value in snapshot.MutableGlobals do
                match control.Instance.GetGlobal(name) with
                | null ->
                    invalidArg (nameof snapshot) $"Snapshot global {name} is not exported."
                | wasmGlobal when wasmGlobal.Mutability <> Mutability.Mutable ->
                    invalidArg (nameof snapshot) $"Snapshot global {name} is immutable."
                | wasmGlobal -> wasmGlobal.SetValue(boxedGlobalValue value)

            control.WakeTick <- snapshot.WakeTick
            control.FaultCount <- snapshot.FaultCount
            control
        with _ ->
            (control :> IDisposable).Dispose()
            reraise ()

    let private readU32 (bytes: byte array) =
        uint32 bytes[0]
        ||| (uint32 bytes[1] <<< 8)
        ||| (uint32 bytes[2] <<< 16)
        ||| (uint32 bytes[3] <<< 24)

    let private validRequest currentTick allowed request =
        if not (Set.contains request.Kind allowed) then
            Error ControlFailure.ForbiddenRequest
        else
            match request.Kind with
            | RequestKind.SetMovementIntent
            | RequestKind.SetFacing
            | RequestKind.SetAttention ->
                if request.Payload.Length = 1
                   && Direction8.tryFromCode request.Payload[0] |> Option.isSome then
                    Ok()
                else
                    Error ControlFailure.MalformedOutput
            | RequestKind.Sleep ->
                if request.Payload.Length = 4
                   && readU32 request.Payload <= uint32 Int32.MaxValue
                   && int32 (readU32 request.Payload) > currentTick then
                    Ok()
                else
                    Error ControlFailure.MalformedOutput
            | _ -> Ok()

    let private journal
        (control: ControlInstance)
        tick
        input
        output
        failure
        fuelAllowance
        fuelRemaining
        requests
        =
        { Tick = tick
          UnitId = control.UnitId
          InputHash = CanonicalHash.sha256 input
          OutputHash = output |> Option.map CanonicalHash.sha256
          Requests = requests
          Failure = failure
          Budget =
            { FuelAllowance = fuelAllowance
              FuelConsumed = fuelAllowance - min fuelAllowance fuelRemaining
              MemoryBytes = control.Memory.GetLength() }
          ModuleState = None
          ModuleStateHash = currentStateHash control }

    /// Performs the one input copy, bounded call, one output read, and complete
    /// structural/semantic validation. No kernel state is changed by this function.
    let invoke tick (input: byte array) (control: ControlInstance) =
        if control.WakeTick |> Option.exists (fun wake -> tick < wake) then
            SleepingUntil control.WakeTick.Value
        else
            match V1Codec.decode MessageKind.Input input with
            | Error _ -> invalidArg (nameof input) "Host input is not canonical Control ABI v1."
            | Ok envelope when envelope.Tick <> tick || envelope.UnitId <> control.UnitId ->
                invalidArg (nameof input) "Host input tick/unit identity does not match the instance."
            | Ok _ ->
                let allowance = control.Artifact.Profile.FuelPerInvocation
                control.Store.Fuel <- allowance

                let fail failure output =
                    control.FaultCount <- control.FaultCount + 1
                    let remaining =
                        try control.Store.Fuel with _ -> 0UL
                    Failed(
                        failure,
                        journal control tick input output (Some failure) allowance remaining []
                    )

                try
                    let inputPointer = invokeI32 control.Instance "sir_input_ptr"
                    let inputCapacity = invokeI32 control.Instance "sir_input_capacity"
                    let outputPointer = invokeI32 control.Instance "sir_output_ptr"
                    let outputCapacity = invokeI32 control.Instance "sir_output_capacity"
                    let memorySize = control.Memory.GetLength()
                    checkedRange memorySize inputPointer inputCapacity "input range"
                    checkedRange memorySize outputPointer outputCapacity "output range"

                    if input.Length > inputCapacity then
                        invalidArg
                            (nameof input)
                            "Canonical input exceeds the module's current input capacity."

                    let inputEnd = int64 inputPointer + int64 inputCapacity
                    let outputEnd = int64 outputPointer + int64 outputCapacity

                    if int64 inputPointer < outputEnd
                       && int64 outputPointer < inputEnd then
                        invalidArg
                            (nameof control)
                            "The module's current input and output ranges overlap."

                    ReadOnlySpan<byte>(input).CopyTo(
                        control.Memory.GetSpan(int64 inputPointer, input.Length)
                    )

                    let outputLength =
                        match control.Decide.Invoke(input.Length) with
                        | :? int as value -> value
                        | _ -> Int32.MinValue

                    if outputLength < 0 then
                        fail ControlFailure.ModuleRejection None
                    elif outputLength > outputCapacity
                         || outputLength > V1Constants.OutputMaximumBytes then
                        fail ControlFailure.InvalidOutputLength None
                    elif control.Memory.GetLength() > control.Artifact.Profile.MaximumMemoryBytes then
                        fail ControlFailure.MemoryLimit None
                    else
                        let output =
                            control.Memory.GetSpan(int64 outputPointer, outputLength).ToArray()

                        match V1Codec.decodeOutput output with
                        | Error _ -> fail ControlFailure.MalformedOutput (Some output)
                        | Ok decoded
                            when decoded.Envelope.Tick <> tick
                                 || decoded.Envelope.UnitId <> control.UnitId ->
                            fail ControlFailure.MalformedOutput (Some output)
                        | Ok decoded ->
                            match
                                decoded.Requests
                                |> List.tryPick (fun request ->
                                    match
                                        validRequest
                                            tick
                                            control.Artifact.Profile.AllowedRequests
                                            request
                                    with
                                    | Ok() -> None
                                    | Error failure -> Some failure)
                            with
                            | Some failure -> fail failure (Some output)
                            | None ->
                                decoded.Requests
                                |> List.tryPick (fun request ->
                                    if request.Kind = RequestKind.Sleep then
                                        Some(int32 (readU32 request.Payload))
                                    else
                                        None)
                                |> Option.iter (fun wake -> control.WakeTick <- Some wake)

                                let remaining = control.Store.Fuel
                                let entry =
                                    journal
                                        control
                                        tick
                                        input
                                        (Some output)
                                        None
                                        allowance
                                        remaining
                                        decoded.Requests

                                Accepted(decoded, entry)
                with
                | :? TrapException as trap
                    when trap.Type = TrapCode.OutOfFuel ->
                    fail ControlFailure.FuelExhaustion None
                | :? TrapException -> fail ControlFailure.Trap None
                | :? WasmtimeException -> fail ControlFailure.Trap None
                | :? ArgumentException -> fail ControlFailure.MemoryLimit None

    /// Applies the kernel-facing v1 requests currently owned by MapScale.
    /// Callers invoke MapScale.tick only after every unit output has validated.
    let applyToMapScale (state: MapScaleState) unitId requests =
        match Map.tryFind unitId state.Units with
        | None -> Error "The controlled unit is not present in the map-scale state."
        | Some unit ->
            let mutable updatedUnit = unit
            let mutable movementIntents = state.MovementIntents

            for request in requests do
                match request.Kind with
                | RequestKind.SetMovementIntent ->
                    movementIntents <-
                        Map.add
                            unitId
                            (Direction8.tryFromCode request.Payload[0] |> Option.get)
                            movementIntents
                | RequestKind.SetFacing ->
                    updatedUnit <-
                        { updatedUnit with
                            BodyFacing =
                                Direction8.tryFromCode request.Payload[0] |> Option.get }
                | RequestKind.SetAttention ->
                    updatedUnit <-
                        { updatedUnit with
                            AttentionDirection =
                                Direction8.tryFromCode request.Payload[0] |> Option.get }
                | _ -> ()

            Ok
                { state with
                    Units = Map.add unitId updatedUnit state.Units
                    MovementIntents = movementIntents }
