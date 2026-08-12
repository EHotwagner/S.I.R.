module SIR.Client.Web.PanelViews

open Feliz
open SIR.Client

/// Read-only review panels are kept outside the Elmish composition root so the
/// root only selects a panel and owns dispatch, rather than its view details.
let reviewLayersPanel (model: SIR.Client.Model) =
    let inspection = model.Inspection
    let count (select: InspectionProjection -> int) = inspection |> Option.map select |> Option.defaultValue 0

    Html.section [
        prop.ariaLabel "Review projection layers"
        prop.children [
            Html.p (
                "Committed frame · tick "
                + string model.Playback.CurrentTick
                + " · read-only"
            )
            Html.dl [
                Html.dt "Units"
                Html.dd (string (count (fun value -> value.Units.Length)))
                Html.dt "Edges"
                Html.dd (string (count (fun value -> value.Edges.Length)))
                Html.dt "Disclosed events"
                Html.dd (string (count (fun value -> value.Events.Length)))
                Html.dt "Perspective"
                Html.dd (
                    inspection
                    |> Option.bind _.PerspectiveHash
                    |> Option.map (fun hash -> "Filtered · " + hash)
                    |> Option.defaultValue "Full replay disclosure"
                )
            ]
        ]
    ]

let reviewDocumentPanel (model: SIR.Client.Model) =
    Html.section [
        prop.ariaLabel "Review source and verification identity"
        prop.children [
            match model.Source with
            | Loaded metadata ->
                Html.dl [
                    Html.dt "Source"
                    Html.dd metadata.SourceName
                    Html.dt "Source identity"
                    Html.dd metadata.SourceIdentity
                    Html.dt "Engine identity"
                    Html.dd metadata.EngineIdentity
                    Html.dt "Replay kind"
                    Html.dd (string metadata.Kind)
                    Html.dt "Committed ticks"
                    Html.dd (string metadata.FinalTick)
                ]
            | Reading sourceName -> Html.p ("Reading " + sourceName + ".")
            | Rejected(sourceName, reason) ->
                Html.p (sourceName + " rejected: " + reason)
            | NoSource -> Html.p "No replay package is loaded."
        ]
    ]
