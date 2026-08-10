module SIR.Client.TestsClientBoundaryQualification

open System
open System.IO

/// The machine-readable execution contract is owned separately from the broad
/// client qualification trace so CI/report consumers do not depend on Program.
let junitPath (arguments: string array) =
    arguments
    |> Array.tryFindIndex ((=) "--junit")
    |> Option.map (fun index ->
        match arguments |> Array.tryItem (index + 1) with
        | Some path when not (String.IsNullOrWhiteSpace path) -> path
        | _ -> invalidArg "arguments" "--junit requires a non-empty destination path.")

let writeJunit (path: string) (failure: string option) =
    let escaped value = System.Security.SecurityElement.Escape value
    match Path.GetDirectoryName path with
    | null -> ()
    | directory when String.IsNullOrWhiteSpace directory -> ()
    | directory -> Directory.CreateDirectory directory |> ignore

    let body =
        match failure with
        | None ->
            "<testsuite name=\"SIR.Client.Tests\" tests=\"1\" failures=\"0\" errors=\"0\" skipped=\"0\"><testcase name=\"production-client-qualification\" /></testsuite>"
        | Some message ->
            sprintf "<testsuite name=\"SIR.Client.Tests\" tests=\"1\" failures=\"1\" errors=\"0\" skipped=\"0\"><testcase name=\"production-client-qualification\"><failure message=\"%s\" /></testcase></testsuite>" (escaped message)

    File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + body)

let requireJunitArgumentContract () =
    match
        try
            junitPath [| "--junit" |] |> ignore
            false
        with :? ArgumentException -> true
    with
    | true -> ()
    | false -> failwith "The JUnit switch must reject a missing destination before qualification."
