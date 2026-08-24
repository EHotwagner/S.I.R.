namespace SIR.Domain

open System
open System.Text

[<Struct>]
type SpecificationIdentity = private SpecificationIdentity of value: string

[<RequireQualifiedAccess>]
module SpecificationIdentity =
    let create value =
        let valid character =
            (character >= 'A' && character <= 'Z')
            || (character >= '0' && character <= '9')
            || character = '-'

        if String.IsNullOrWhiteSpace value || value.Length < 5 then
            Error "Specification identities require at least five uppercase ASCII characters."
        elif value |> Seq.exists (valid >> not) then
            Error "Specification identities use uppercase ASCII letters, digits, and hyphens."
        elif value.StartsWith("-") || value.EndsWith("-") || value.Contains("--") then
            Error "Specification identities cannot begin/end with or repeat a hyphen."
        else
            Ok(SpecificationIdentity value)

    let value (SpecificationIdentity value) = value

type SpecificationProvenance =
    { Agent: string
      Session: string
      SourcePath: string
      SourceRevision: string
      AuthoredAtUtc: string }

type SpecificationDiagnostic =
    { Code: string
      Path: string
      Message: string }

type SpecificationModel<'ast> =
    { Identity: SpecificationIdentity
      SchemaVersion: int32
      Provenance: SpecificationProvenance
      Intent: string
      Ast: 'ast }

type SpecificationChange =
    { Path: string
      Summary: string
      BeforeFingerprint: string
      AfterFingerprint: string }

type SpecificationSemanticDiff =
    | Equivalent
    | Changed of SpecificationChange list

[<RequireQualifiedAccess>]
module SpecificationModel =
    let private diagnostic code path message =
        { Code = code; Path = path; Message = message }

    let private blank code path name value =
        if String.IsNullOrWhiteSpace value then
            [ diagnostic code path (name + " is required.") ]
        else
            []

    let private lowercaseHex length (value: string) =
        value.Length = length
        && value
           |> Seq.forall (fun character ->
               (character >= '0' && character <= '9')
               || (character >= 'a' && character <= 'f'))

    let validateEnvelope model =
        let provenance = model.Provenance

        [ if model.SchemaVersion <> 1 then
              yield diagnostic "SPEC-SCHEMA-UNSUPPORTED" "/schemaVersion" "Only specification schema version 1 is supported by the P1 pilot."

          yield! blank "SPEC-PROVENANCE-AGENT" "/provenance/agent" "Provenance agent" provenance.Agent
          yield! blank "SPEC-PROVENANCE-SESSION" "/provenance/session" "Provenance session" provenance.Session
          yield! blank "SPEC-PROVENANCE-SOURCE" "/provenance/sourcePath" "Provenance source path" provenance.SourcePath

          if not (lowercaseHex 40 provenance.SourceRevision) then
              yield diagnostic "SPEC-PROVENANCE-REVISION" "/provenance/sourceRevision" "Source revision must be a 40-character lowercase Git object id."

          match DateTimeOffset.TryParse provenance.AuthoredAtUtc with
          | true, _ -> ()
          | _ ->
              yield diagnostic "SPEC-PROVENANCE-TIME" "/provenance/authoredAtUtc" "Authored time must be a parseable UTC instant."

          yield! blank "SPEC-INTENT-REQUIRED" "/intent" "Authoring intent" model.Intent ]
        |> List.sortBy (fun item -> item.Path, item.Code)

    let private text (value: string) =
        let bytes = Encoding.UTF8.GetBytes value
        CanonicalEncoding.concatenate [ CanonicalEncoding.int32LittleEndian bytes.Length; bytes ]

    let private normalizedBytes encodeAst model =
        CanonicalEncoding.concatenate
            [ text "sir-specification-model/v1"
              text (SpecificationIdentity.value model.Identity)
              CanonicalEncoding.int32LittleEndian model.SchemaVersion
              text model.Provenance.SourcePath
              text model.Provenance.SourceRevision
              encodeAst model.Ast ]

    let private hex bytes =
        bytes
        |> Array.map (fun (value: byte) -> value.ToString("x2"))
        |> String.concat ""

    let normalize encodeAst model =
        match validateEnvelope model with
        | [] -> Ok(normalizedBytes encodeAst model)
        | diagnostics -> Error diagnostics

    let fingerprint encodeAst model =
        normalize encodeAst model
        |> Result.map (CanonicalHash.sha256 >> hex)

    let semanticDiff encodeAst before after =
        let diagnostics = validateEnvelope before @ validateEnvelope after

        if not (List.isEmpty diagnostics) then
            Error(diagnostics |> List.distinct |> List.sortBy (fun item -> item.Path, item.Code))
        else
            let digest bytes = bytes |> CanonicalHash.sha256 |> hex
            let changes =
                [ if before.Identity <> after.Identity then
                      yield
                          { Path = "/identity"
                            Summary = "Specification identity changed."
                            BeforeFingerprint = text (SpecificationIdentity.value before.Identity) |> digest
                            AfterFingerprint = text (SpecificationIdentity.value after.Identity) |> digest }

                  if before.SchemaVersion <> after.SchemaVersion then
                      yield
                          { Path = "/schemaVersion"
                            Summary = "Specification schema version changed."
                            BeforeFingerprint = CanonicalEncoding.int32LittleEndian before.SchemaVersion |> digest
                            AfterFingerprint = CanonicalEncoding.int32LittleEndian after.SchemaVersion |> digest }

                  if before.Provenance.SourcePath <> after.Provenance.SourcePath then
                      yield
                          { Path = "/provenance/sourcePath"
                            Summary = "Authoritative source path changed."
                            BeforeFingerprint = text before.Provenance.SourcePath |> digest
                            AfterFingerprint = text after.Provenance.SourcePath |> digest }

                  if before.Provenance.SourceRevision <> after.Provenance.SourceRevision then
                      yield
                          { Path = "/provenance/sourceRevision"
                            Summary = "Authoritative source revision changed."
                            BeforeFingerprint = text before.Provenance.SourceRevision |> digest
                            AfterFingerprint = text after.Provenance.SourceRevision |> digest }

                  let beforeAst = encodeAst before.Ast
                  let afterAst = encodeAst after.Ast
                  if beforeAst <> afterAst then
                      yield
                          { Path = "/ast"
                            Summary = "Typed specification AST changed."
                            BeforeFingerprint = digest beforeAst
                            AfterFingerprint = digest afterAst } ]

            Ok(if List.isEmpty changes then Equivalent else Changed changes)
