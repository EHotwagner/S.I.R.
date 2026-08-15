module SIR.Client.Web.DocsView

open Browser.Dom
open Fable.Core
open Fable.Core.JsInterop
open Feliz
open SIR.Client
open Thoth.Json

[<Global>]
let private fetch (url: string, options: obj) : JS.Promise<obj> = jsNative

[<Emit("navigator.onLine")>]
let private browserOnline : bool = jsNative

let private statusDecoder =
    Decode.string
    |> Decode.andThen (function
        | "canonical" -> Decode.succeed CanonicalDocumentation
        | "implemented" -> Decode.succeed ImplementedDocumentation
        | "provisional" -> Decode.succeed ProvisionalDocumentation
        | "research" -> Decode.succeed ResearchDocumentation
        | value -> Decode.fail ("Unknown documentation status " + value))

let private blockDecoder : Decoder<DocumentationBlock> =
    Decode.object (fun get ->
        { Kind = get.Required.Field "kind" Decode.string
          Level = get.Optional.Field "level" Decode.int
          Anchor = get.Optional.Field "anchor" Decode.string
          Text = get.Required.Field "text" Decode.string })

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
          Line = get.Optional.Field "line" Decode.int })

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

let private statusText = function
    | CanonicalDocumentation -> "Canonical"
    | ImplementedDocumentation -> "Implemented"
    | ProvisionalDocumentation -> "Provisional"
    | ResearchDocumentation -> "Research"

let sourceUrl (source: DocumentationSource) =
    "https://github.com/"
    + source.Repository
    + "/blob/"
    + source.Revision
    + "/"
    + source.Path.Replace(" ", "%20")
    + (source.Line |> Option.map (fun line -> "#L" + string line) |> Option.defaultValue "")

let contextLinks hasSelected openConcept =
    Html.nav [
        prop.ariaLabel "Tactical contextual documentation"
        prop.className "tactical-context-docs"
        prop.children [
            if hasSelected then
                Html.button [
                    prop.custom ("data-context-origin", "inspector")
                    prop.text "Open documentation for selected unit"
                    prop.onClick (fun _ -> openConcept "units")
                ]
            Html.button [
                prop.custom ("data-context-origin", "overlay")
                prop.text "Open documentation for tactical overlays"
                prop.onClick (fun _ -> openConcept "maps-spatial")
            ]
        ]
    ]

let view
    (navigation: DocumentationNavigation)
    (manifest: DocumentationManifest option)
    (error: string option)
    (externalAnnouncement: string)
    (setQuery: string -> unit)
    (openPage: string -> string option -> unit)
    (back: unit -> unit)
    (forward: unit -> unit)
    (returnToTactical: unit -> unit)
    (announceExternal: string -> unit)
    =
    let pages = manifest |> Option.map _.Pages |> Option.defaultValue []
    let results =
        match manifest with
        | Some value -> UnifiedTacticalWorkspace.documentationSearch navigation.Query value
        | None -> []
    let selected =
        let slug = navigation.Page |> Option.orElseWith (fun () -> pages |> List.tryHead |> Option.map _.Slug)
        slug |> Option.bind (fun current -> pages |> List.tryFind (fun page -> page.Slug = current))
    Html.section [
        prop.id "in-app-docs"
        prop.className "in-app-docs"
        prop.ariaLabel "S.I.R. documentation"
        prop.children [
            Html.header [
                prop.className "in-app-docs-header"
                prop.children [
                    Html.h1 "Documentation"
                    Html.div [
                        prop.role.toolbar
                        prop.ariaLabel "Documentation navigation"
                        prop.children [
                            Html.button [ prop.text "Back"; prop.ariaLabel "Documentation back"; prop.disabled navigation.Back.IsEmpty; prop.onClick (fun _ -> back ()) ]
                            Html.button [ prop.text "Forward"; prop.ariaLabel "Documentation forward"; prop.disabled navigation.Forward.IsEmpty; prop.onClick (fun _ -> forward ()) ]
                            Html.button [ prop.text "Return to tactical workspace"; prop.onClick (fun _ -> returnToTactical ()) ]
                        ]
                    ]
                    Html.label [ prop.htmlFor "docs-search"; prop.text "Search documentation" ]
                    Html.input [
                        prop.id "docs-search"
                        prop.type'.search
                        prop.value navigation.Query
                        prop.placeholder "Search LOS, cover, armor…"
                        prop.onChange setQuery
                    ]
                ]
            ]
            match error with Some message -> Html.p [ prop.role.alert; prop.text message ] | None -> Html.none
            match manifest with
            | None -> Html.p [ prop.role.status; prop.ariaLive.polite; prop.text "Loading local documentation…" ]
            | Some value ->
                Html.div [
                    prop.className "in-app-docs-layout"
                    prop.children [
                        Html.nav [
                            prop.className "in-app-docs-tree"
                            prop.ariaLabel "Documentation hierarchy"
                            prop.children [
                                Html.p ($"{results.Length} pages · {value.SearchTokenCount} indexed tokens")
                                Html.ul [
                                    for page in results do
                                        Html.li [
                                            Html.button [
                                                prop.text (page.Title + " · " + statusText page.Status)
                                                prop.ariaPressed (selected |> Option.exists (fun current -> current.Slug = page.Slug))
                                                prop.onClick (fun _ -> openPage page.Slug None)
                                            ]
                                        ]
                                ]
                            ]
                        ]
                        Html.article [
                            prop.className "in-app-docs-page"
                            prop.ariaLabel "Documentation page"
                            prop.children [
                                match selected with
                                | None -> Html.p "No documentation page matches the current filter."
                                | Some page ->
                                    Html.nav [
                                        prop.ariaLabel "Breadcrumbs"
                                        prop.children [ Html.span page.Category; Html.span " / "; Html.span page.Title ]
                                    ]
                                    Html.p [ prop.className "docs-status"; prop.custom ("data-doc-status", statusText page.Status); prop.text (statusText page.Status + " · " + page.SourcePath) ]
                                    Html.nav [
                                        prop.ariaLabel "On this page"
                                        prop.children [
                                            Html.ul [ for title, anchor in page.Headings do Html.li [ Html.a [ prop.href ("#" + anchor); prop.text title ] ] ]
                                        ]
                                    ]
                                    for block in page.Blocks do
                                        match block.Kind, block.Level with
                                        | "heading", Some 1 -> Html.h2 [ prop.id (block.Anchor |> Option.defaultValue ""); prop.text block.Text ]
                                        | "heading", Some 2 -> Html.h2 [ prop.id (block.Anchor |> Option.defaultValue ""); prop.text block.Text ]
                                        | "heading", Some 3 -> Html.h3 [ prop.id (block.Anchor |> Option.defaultValue ""); prop.text block.Text ]
                                        | "heading", _ -> Html.h4 [ prop.id (block.Anchor |> Option.defaultValue ""); prop.text block.Text ]
                                        | "code", _ -> Html.pre [ Html.code block.Text ]
                                        | "table", _ -> Html.pre [ prop.className "docs-table-source"; prop.ariaLabel "Documentation table"; prop.text block.Text ]
                                        | "image", _ -> Html.p [ prop.className "docs-image-description"; prop.text block.Text ]
                                        | _ -> Html.p block.Text
                                    if not page.Related.IsEmpty then
                                        Html.nav [
                                            prop.ariaLabel "Related documentation"
                                            prop.children [ Html.h2 "Related pages"; Html.ul [ for slug in page.Related do Html.li [ Html.button [ prop.text slug; prop.onClick (fun _ -> openPage slug None) ] ] ] ]
                                        ]
                                    match
                                        value.Sources
                                        |> Map.toList
                                        |> List.tryFind (fun (_, source) -> source.Path = page.SourcePath)
                                    with
                                    | Some (_, source) ->
                                        Html.a [
                                            prop.href (sourceUrl source)
                                            prop.target.blank
                                            prop.rel.noopener
                                            prop.text "Open matching GitHub source"
                                            prop.onClick (fun event ->
                                                if not browserOnline then
                                                    event.preventDefault ()
                                                    announceExternal "GitHub source is unavailable while offline. Local documentation remains available."
                                                else
                                                    announceExternal "Opening GitHub source in a new tab. Local documentation remains available.")
                                        ]
                                    | None -> Html.none
                            ]
                        ]
                    ]
                ]
            Html.p [ prop.className "sr-only"; prop.role.status; prop.ariaLive.polite; prop.text externalAnnouncement ]
        ]
    ]
