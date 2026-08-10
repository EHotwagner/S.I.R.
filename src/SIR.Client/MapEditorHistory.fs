namespace SIR.Client

[<RequireQualifiedAccess>]
module MapEditorHistory =
    let withinBounds maximumCommands maximumBytes (entries: EditorHistoryEntry list) =
        let rec keep count bytes accepted remaining =
            match remaining with
            | [] -> List.rev accepted
            | entry :: tail
                when count < maximumCommands && bytes + entry.SerializedBytes <= maximumBytes ->
                keep (count + 1) (bytes + entry.SerializedBytes) (entry :: accepted) tail
            | _ -> List.rev accepted

        keep 0 0 [] entries

    let size (entries: EditorHistoryEntry list) =
        entries |> List.sumBy _.SerializedBytes
