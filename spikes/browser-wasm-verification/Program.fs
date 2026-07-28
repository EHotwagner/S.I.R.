module BrowserWasmVerificationSpike

open System
open System.IO
open System.Security.Cryptography
open System.Text.Json
open Wasmtime

type Oracle =
    { runtime: string
      artifactSha256: string
      decisions: int array
      hostCalls: int array
      finalCounter: int
      freshInstanceCounter: int
      explicitTrap: bool
      fuelMetering: bool
      infiniteLoopFuelTrap: bool }

let private createEngine () =
    let config = new Config()
    config.WithFuelConsumption(true) |> ignore
    config.WithReferenceTypes(false) |> ignore
    config.WithBulkMemory(false) |> ignore
    config.WithSIMD(false) |> ignore
    config.WithRelaxedSIMD(false, false) |> ignore
    config.WithMultiValue(false) |> ignore
    config.WithMultiMemory(false) |> ignore
    config.WithWasmThreads(false) |> ignore
    config.WithTailCalls(false) |> ignore
    config.WithComponentModel(false) |> ignore
    new Engine(config)

let private instantiate
    (engine: Engine)
    (compiled: Wasmtime.Module)
    (hostCalls: ResizeArray<int>)
    =
    let store = new Store(engine)
    let linker = new Linker(engine)

    linker.Define(
        "sir",
        "bias",
        Function.FromCallback(
            store,
            Func<int, int>(fun value ->
                hostCalls.Add(value)
                value * 2)
        )
    )

    let instance = linker.Instantiate(store, compiled)
    store, linker, instance

let private getFunction (instance: Instance) name =
    match instance.GetFunction(name) with
    | null -> failwithf "The artifact does not export %s." name
    | value -> value

let private invokeInt (instance: Instance) name (argument: int) =
    match (getFunction instance name).Invoke(argument) with
    | :? int as value -> value
    | value -> failwithf "%s returned unexpected value %A." name value

let private readInt (instance: Instance) name =
    match (getFunction instance name).Invoke() with
    | :? int as value -> value
    | value -> failwithf "%s returned unexpected value %A." name value

let private traps (operation: unit -> unit) =
    try
        operation ()
        false
    with :? TrapException ->
        true

[<EntryPoint>]
let main arguments =
    if arguments.Length <> 1 then
        invalidArg (nameof arguments) "Pass the shared artifact.b64 path."

    let artifact =
        File.ReadAllText(arguments[0]).Trim()
        |> Convert.FromBase64String

    use engine = createEngine ()
    use compiled =
        Wasmtime.Module.FromBytes(engine, "browser-verification-spike", artifact)

    let hostCalls = ResizeArray<int>()
    let store, linker, instance = instantiate engine compiled hostCalls
    use storeScope = store
    use linkerScope = linker

    let decisions =
        [| 3; 3; -2 |]
        |> Array.map (fun tick ->
            store.Fuel <- 10_000UL
            invokeInt instance "decide" tick)

    let finalCounter = readInt instance "counter"
    let explicitTrap =
        traps (fun () -> (getFunction instance "trap").Invoke() |> ignore)

    let freshCalls = ResizeArray<int>()
    let freshStore, freshLinker, freshInstance =
        instantiate engine compiled freshCalls
    use freshStoreScope = freshStore
    use freshLinkerScope = freshLinker
    freshStore.Fuel <- 10_000UL
    let freshInstanceCounter = readInt freshInstance "counter"

    let loopCalls = ResizeArray<int>()
    let loopStore, loopLinker, loopInstance =
        instantiate engine compiled loopCalls
    use loopStoreScope = loopStore
    use loopLinkerScope = loopLinker
    loopStore.Fuel <- 1_000UL
    let infiniteLoopFuelTrap =
        traps (fun () -> (getFunction loopInstance "spin").Invoke() |> ignore)

    let oracle =
        { runtime = "wasmtime-44.0.0"
          artifactSha256 =
            Convert.ToHexString(SHA256.HashData(artifact)).ToLowerInvariant()
          decisions = decisions
          hostCalls = hostCalls.ToArray()
          finalCounter = finalCounter
          freshInstanceCounter = freshInstanceCounter
          explicitTrap = explicitTrap
          fuelMetering = true
          infiniteLoopFuelTrap = infiniteLoopFuelTrap }

    printfn "%s" (JsonSerializer.Serialize(oracle))
    0
