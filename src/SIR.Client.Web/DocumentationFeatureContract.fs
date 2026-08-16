module SIR.Client.Web.DocumentationFeatureContract

open Fable.Core
open Fable.Core.JsInterop
open SIR.Client
open Thoth.Json

type Presentation =
    { Navigation: DocumentationNavigation
      Manifest: DocumentationManifest option
      Error: string option
      ExternalAnnouncement: string }

type Callbacks =
    { SetQuery: string -> unit
      OpenPage: string -> string option -> unit
      Back: unit -> unit
      Forward: unit -> unit
      ReturnToTactical: unit -> unit
      AnnounceExternal: string -> unit }

[<Global>]
let private fetch (url: string, options: obj) : JS.Promise<obj> = jsNative

let private statusDecoder =
    Decode.string
    |> Decode.andThen (function
        | "canonical" -> Decode.succeed CanonicalDocumentation
        | "implemented" -> Decode.succeed ImplementedDocumentation
        | "provisional" -> Decode.succeed ProvisionalDocumentation
        | "research" -> Decode.succeed ResearchDocumentation
        | value -> Decode.fail ("Unknown documentation status " + value))

let private segmentDecoder : Decoder<DocumentationSegment> =
    Decode.object (fun get ->
        { SegmentKind = get.Required.Field "kind" Decode.string
          SegmentText = get.Required.Field "text" Decode.string
          TargetSlug = get.Optional.Field "targetSlug" Decode.string
          Anchor = get.Optional.Field "anchor" Decode.string
          ExternalUrl = get.Optional.Field "externalUrl" Decode.string })

let private blockDecoder : Decoder<DocumentationBlock> =
    Decode.object (fun get ->
        { Kind = get.Required.Field "kind" Decode.string
          Level = get.Optional.Field "level" Decode.int
          Anchor = get.Optional.Field "anchor" Decode.string
          Text = get.Required.Field "text" Decode.string
          ContentSegments = get.Optional.Field "segments" (Decode.list segmentDecoder) |> Option.defaultValue []
          Rows = get.Optional.Field "rows" (Decode.list (Decode.list Decode.string)) |> Option.defaultValue []
          ImageSource = get.Optional.Field "imageSource" Decode.string })

let private headingDecoder =
    Decode.object (fun get ->
        get.Required.Field "title" Decode.string,
        get.Required.Field "anchor" Decode.string)

let private pageDecoder : Decoder<DocumentationPage> =
    Decode.object (fun get ->
        { Slug = get.Required.Field "slug" Decode.string
          Title = get.Required.Field "title" Decode.string
          Category = get.Required.Field "category" Decode.string
          Status = get.Required.Field "status" statusDecoder
          SourcePath = get.Required.Field "sourcePath" Decode.string
          ApiPath = get.Optional.Field "apiPath" Decode.string
          ContentDigest = get.Required.Field "contentDigest" Decode.string
          Headings = get.Required.Field "headings" (Decode.list headingDecoder)
          Related = get.Required.Field "related" (Decode.list Decode.string)
          Blocks = get.Required.Field "blocks" (Decode.list blockDecoder) })

let private sourceDecoder : Decoder<DocumentationSource> =
    Decode.object (fun get ->
        { Repository = get.Required.Field "repository" Decode.string
          Revision = get.Required.Field "revision" Decode.string
          Path = get.Required.Field "path" Decode.string
          PageSlug = get.Required.Field "pageSlug" Decode.string
          Concept = get.Required.Field "concept" Decode.string
          Symbol = get.Optional.Field "symbol" Decode.string
          Line = get.Optional.Field "line" Decode.int
          ContentDigest = get.Required.Field "contentDigest" Decode.string
          LineDigest = get.Required.Field "lineDigest" Decode.string })

let private sourcesDecoder =
    Decode.keyValuePairs sourceDecoder |> Decode.map Map.ofList

let private manifestDecoder : Decoder<DocumentationManifest> =
    Decode.object (fun get ->
        { Schema = get.Required.Field "schema" Decode.string
          DefinitionDigest = get.Required.Field "definitionDigest" Decode.string
          Pages = get.Required.Field "pages" (Decode.list pageDecoder)
          Sources = get.Required.Field "sources" sourcesDecoder
          SearchTokenCount = get.Required.Field "searchTokenCount" Decode.int })

let load () =
    async {
        try
            let! response = fetch ("/content/sir-client/v1/in-app-docs.json", createObj []) |> Async.AwaitPromise
            let! body = response?text() |> Async.AwaitPromise
            if not (unbox<bool> response?ok) then
                return Error "Local documentation is temporarily unavailable."
            else
                return
                    Decode.fromString manifestDecoder (string body)
                    |> Result.bind (fun manifest ->
                        if manifest.Schema = "sir-in-app-docs-v1" then Ok manifest
                        else Error "Local documentation has an unsupported manifest version.")
        with _ ->
            return Error "Local documentation is temporarily unavailable."
    }
