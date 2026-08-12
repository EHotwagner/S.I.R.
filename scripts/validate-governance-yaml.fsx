#r "nuget: YamlDotNet, 16.3.0"

open System
open System.IO
open YamlDotNet.RepresentationModel

let fail message = eprintfn "governance YAML invalid: %s" message; exit 1
let scalar (node: YamlNode) =
    match node with
    | :? YamlScalarNode as value -> value.Value
    | _ -> fail "expected a scalar value"
let mapping (path: string) =
    try
        use reader = new StreamReader(path)
        let document = YamlStream()
        document.Load(reader)
        if document.Documents.Count <> 1 then fail (path + " must contain exactly one YAML document")
        match document.Documents[0].RootNode with
        | :? YamlMappingNode as value -> value
        | _ -> fail (path + " must contain a YAML mapping")
    with error -> fail (path + ": " + error.Message)
let key name (map: YamlMappingNode) =
    let wanted = YamlScalarNode(name)
    match map.Children.TryGetValue(wanted) with
    | true, value -> value
    | _ -> fail (sprintf "missing '%s'" name)
let sequence name map =
    match key name map with
    | :? YamlSequenceNode as value -> value.Children
    | _ -> fail (sprintf "'%s' must be a sequence" name)
let strings name map = sequence name map |> Seq.map scalar |> Set.ofSeq
let governed = mapping ".fsgg/governance.yml"
if scalar (key "schemaVersion" governed) <> "1" then fail "governance schemaVersion must be 1"
let domains = strings "domains" governed
let packageSurfaces = sequence "packageSurfaces" governed |> Seq.map scalar |> Seq.toList
if packageSurfaces.Length <> 1 then fail "packageSurfaces must declare exactly one F# project"
for project in packageSurfaces do
    if not (project.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase)) then fail ("package surface is not an F# project: " + project)
    if not (File.Exists project) then fail ("missing declared package surface " + project)
for reference in [ scalar (key "policyRef" governed); scalar (key "capabilitiesRef" governed) ] do
    if not (File.Exists reference) then fail ("missing referenced configuration " + reference)
let policy = mapping ".fsgg/policy.yml"
if scalar (key "schemaVersion" policy) <> "1" then fail "policy schemaVersion must be 1"
let profiles = strings "profiles" policy
if not (profiles.Contains(scalar (key "defaultProfile" policy))) then fail "defaultProfile is not declared"
let capabilities = mapping ".fsgg/capabilities.yml"
if scalar (key "schemaVersion" capabilities) <> "2" then fail "capabilities schemaVersion must be 2"
let capabilityDomains = strings "domains" capabilities
if capabilityDomains <> domains then fail "governance and capabilities domains differ"
let commandIds =
    let tooling = mapping ".fsgg/tooling.yml"
    if scalar (key "schemaVersion" tooling) <> "1" then fail "tooling schemaVersion must be 1"
    sequence "commands" tooling
    |> Seq.map (fun entry -> match entry with :? YamlMappingNode as map -> scalar (key "id" map) | _ -> fail "tooling command must be a mapping")
    |> Set.ofSeq
for entry in sequence "checks" capabilities do
    match entry with
    | :? YamlMappingNode as check ->
        if not (domains.Contains(scalar (key "domain" check))) then fail "check references an undeclared domain"
        match check.Children.TryGetValue(YamlScalarNode("command")) with
        | true, command when not (commandIds.Contains(scalar command)) -> fail "check references an undeclared tooling command"
        | _ -> ()
    | _ -> fail "capability check must be a mapping"
if fsi.CommandLineArgs |> Array.exists ((=) "--package-surface") then
    printfn "%s" packageSurfaces.Head
else
    printfn "Governance YAML schemas and cross-file references verified."
