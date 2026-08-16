module SIR.Client.Web.DocsView

open Browser.Dom
open Fable.Core
open Fable.Core.JsInterop
open Feliz
open SIR.Client

[<Emit("(async()=>{try{await fetch($0,{method:'HEAD',mode:'no-cors',cache:'no-store'});const w=window.open($0,'_blank');if(!w)return 'blocked';w.opener=null;return 'opened'}catch(_){return 'unavailable'}})()")>]
let private openExternalAtHost (url: string) : JS.Promise<string> = jsNative

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

let private renderSegments openPage segments =
    [ for segment in segments do
        match segment.SegmentKind, segment.TargetSlug, segment.ExternalUrl with
        | "link", Some slug, _ ->
            Html.a [
                prop.href ("#" + slug + (segment.Anchor |> Option.map (fun anchor -> "#" + anchor) |> Option.defaultValue ""))
                prop.text segment.SegmentText
                prop.onClick (fun event -> event.preventDefault (); openPage slug segment.Anchor)
            ]
        | "link", _, Some url -> Html.a [ prop.href url; prop.text segment.SegmentText ]
        | _ -> Html.span segment.SegmentText ]

[<ReactComponent>]
let DocumentationWorkspace
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
                                        | "table", _ ->
                                            Html.table [
                                                prop.className "docs-table"
                                                prop.ariaLabel "Documentation table"
                                                prop.children [
                                                    if not block.Rows.IsEmpty then
                                                        Html.thead [ Html.tr [ for cell in block.Rows.Head do Html.th cell ] ]
                                                        Html.tbody [ for row in block.Rows.Tail do Html.tr [ for cell in row do Html.td cell ] ]
                                                ]
                                            ]
                                        | "image", _ ->
                                            match block.ImageSource with
                                            | Some source -> Html.img [ prop.className "docs-image"; prop.src source; prop.alt block.Text ]
                                            | None -> Html.none
                                        | _ -> Html.p [ prop.children (renderSegments openPage block.ContentSegments) ]
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
                                                event.preventDefault ()
                                                async {
                                                    let! result = openExternalAtHost (sourceUrl source) |> Async.AwaitPromise
                                                    match result with
                                                    | "opened" -> announceExternal "GitHub source opened in a new tab. Local documentation remains available."
                                                    | "blocked" -> announceExternal "The browser blocked the GitHub source window. Local documentation remains available."
                                                    | _ -> announceExternal "GitHub source is unavailable from this host. Local documentation remains available."
                                                } |> Async.StartImmediate)
                                        ]
                                    | None -> Html.none
                            ]
                        ]
                    ]
                ]
            Html.p [ prop.className "sr-only"; prop.role.status; prop.ariaLive.polite; prop.text externalAnnouncement ]
        ]
    ]
