namespace SIR.Client

open System
open System.Text

type InterchangeFormat =
    | UniversalVtt
    | FoundryScene
    | FantasyGroundsImage

type InterchangeDisposition =
    | Mapped
    | Ignored
    | Lossy
    | RejectedField

type InterchangeFieldReport =
    { Path: string
      Disposition: InterchangeDisposition
      Meaning: string }

type InterchangeReview =
    { Format: InterchangeFormat
      SourceName: string
      Candidate: MapDefinition option
      Fields: InterchangeFieldReport array
      Errors: string array }

[<RequireQualifiedAccess>]
module MapEditorInterchange =
    type private JsonValue =
        | JObject of Map<string, JsonValue>
        | JArray of JsonValue array
        | JString of string
        | JNumber of float
        | JBool of bool
        | JNull

    type private Parser =
        { Text: string
          mutable Offset: int }

    let private skipWhitespace parser =
        while parser.Offset < parser.Text.Length && Char.IsWhiteSpace parser.Text[parser.Offset] do
            parser.Offset <- parser.Offset + 1

    let private fail parser message =
        Error(message + " at character " + string parser.Offset + ".")

    let rec private parseValue parser =
        skipWhitespace parser
        if parser.Offset >= parser.Text.Length then fail parser "Unexpected end of JSON"
        else
            match parser.Text[parser.Offset] with
            | '{' -> parseObject parser
            | '[' -> parseArray parser
            | '"' -> parseString parser |> Result.map JString
            | 't' -> parseLiteral parser "true" (JBool true)
            | 'f' -> parseLiteral parser "false" (JBool false)
            | 'n' -> parseLiteral parser "null" JNull
            | character when character = '-' || Char.IsDigit character -> parseNumber parser
            | _ -> fail parser "Invalid JSON token"

    and private parseLiteral parser literal value =
        if parser.Offset + literal.Length <= parser.Text.Length
           && parser.Text.Substring(parser.Offset, literal.Length) = literal then
            parser.Offset <- parser.Offset + literal.Length
            Ok value
        else fail parser ("Expected " + literal)

    and private parseString parser =
        if parser.Text[parser.Offset] <> '"' then fail parser "Expected string"
        else
            parser.Offset <- parser.Offset + 1
            let buffer = Text.StringBuilder()
            let mutable complete = false
            let mutable error = None
            while not complete && error.IsNone && parser.Offset < parser.Text.Length do
                let character = parser.Text[parser.Offset]
                parser.Offset <- parser.Offset + 1
                match character with
                | '"' -> complete <- true
                | '\\' when parser.Offset < parser.Text.Length ->
                    let escaped = parser.Text[parser.Offset]
                    parser.Offset <- parser.Offset + 1
                    match escaped with
                    | '"' -> buffer.Append('"') |> ignore
                    | '\\' -> buffer.Append('\\') |> ignore
                    | '/' -> buffer.Append('/') |> ignore
                    | 'b' -> buffer.Append('\b') |> ignore
                    | 'f' -> buffer.Append('\f') |> ignore
                    | 'n' -> buffer.Append('\n') |> ignore
                    | 'r' -> buffer.Append('\r') |> ignore
                    | 't' -> buffer.Append('\t') |> ignore
                    | 'u' when parser.Offset + 4 <= parser.Text.Length ->
                        let hexDigit character =
                            if character >= '0' && character <= '9' then Some(int character - int '0')
                            elif character >= 'a' && character <= 'f' then Some(10 + int character - int 'a')
                            elif character >= 'A' && character <= 'F' then Some(10 + int character - int 'A')
                            else None
                        let digits =
                            parser.Text.Substring(parser.Offset, 4)
                            |> Seq.map hexDigit
                            |> Seq.toArray
                        if digits |> Array.forall Option.isSome then
                            let value =
                                digits
                                |> Array.fold (fun total digit -> total * 16 + Option.get digit) 0
                            buffer.Append(char value) |> ignore
                            parser.Offset <- parser.Offset + 4
                        else error <- Some "Invalid JSON unicode escape"
                    | _ -> error <- Some "Invalid JSON escape"
                | character when int character < 0x20 -> error <- Some "Control character in JSON string"
                | character -> buffer.Append(character) |> ignore
            match error with
            | Some message -> fail parser message
            | None when not complete -> fail parser "Unterminated JSON string"
            | None -> Ok(buffer.ToString())

    and private parseNumber parser =
        let start = parser.Offset
        let accepted character =
            Char.IsDigit character
            || character = '-' || character = '+'
            || character = '.' || character = 'e' || character = 'E'
        while parser.Offset < parser.Text.Length && accepted parser.Text[parser.Offset] do
            parser.Offset <- parser.Offset + 1
        let token = parser.Text.Substring(start, parser.Offset - start)
        let mutable index = 0
        let mutable sign = 1.0
        if index < token.Length && token[index] = '-' then
            sign <- -1.0
            index <- index + 1
        let integerStart = index
        if index < token.Length && token[index] = '0' then index <- index + 1
        else
            while index < token.Length && Char.IsDigit token[index] do index <- index + 1
        let mutable value = 0.0
        for digitIndex in integerStart .. index - 1 do
            value <- value * 10.0 + float (int token[digitIndex] - int '0')
        let mutable valid = index > integerStart
        if index < token.Length && token[index] = '.' then
            index <- index + 1
            let fractionStart = index
            let mutable place = 0.1
            while index < token.Length && Char.IsDigit token[index] do
                value <- value + float (int token[index] - int '0') * place
                place <- place * 0.1
                index <- index + 1
            valid <- valid && index > fractionStart
        if index < token.Length && (token[index] = 'e' || token[index] = 'E') then
            index <- index + 1
            let mutable exponentSign = 1
            if index < token.Length && (token[index] = '+' || token[index] = '-') then
                if token[index] = '-' then exponentSign <- -1
                index <- index + 1
            let exponentStart = index
            let mutable exponent = 0
            while index < token.Length && Char.IsDigit token[index] do
                exponent <- exponent * 10 + int token[index] - int '0'
                index <- index + 1
            valid <- valid && index > exponentStart
            value <- value * Math.Pow(10.0, float (exponentSign * exponent))
        value <- value * sign
        if valid && index = token.Length && not (Double.IsNaN value || Double.IsInfinity value) then
            Ok(JNumber value)
        else fail parser "Invalid JSON number"

    and private parseArray parser =
        parser.Offset <- parser.Offset + 1
        skipWhitespace parser
        let values = ResizeArray<JsonValue>()
        let mutable complete = false
        let mutable error = None
        if parser.Offset < parser.Text.Length && parser.Text[parser.Offset] = ']' then
            parser.Offset <- parser.Offset + 1
            complete <- true
        while not complete && error.IsNone do
            match parseValue parser with
            | Error message -> error <- Some message
            | Ok value ->
                values.Add value
                skipWhitespace parser
                if parser.Offset >= parser.Text.Length then error <- Some "Unterminated JSON array."
                elif parser.Text[parser.Offset] = ']' then
                    parser.Offset <- parser.Offset + 1
                    complete <- true
                elif parser.Text[parser.Offset] = ',' then parser.Offset <- parser.Offset + 1
                else error <- Some("Expected comma in JSON array at character " + string parser.Offset + ".")
        match error with
        | Some message -> Error message
        | None -> Ok(JArray(values.ToArray()))

    and private parseObject parser =
        parser.Offset <- parser.Offset + 1
        skipWhitespace parser
        let values = ResizeArray<string * JsonValue>()
        let mutable complete = false
        let mutable error = None
        if parser.Offset < parser.Text.Length && parser.Text[parser.Offset] = '}' then
            parser.Offset <- parser.Offset + 1
            complete <- true
        while not complete && error.IsNone do
            skipWhitespace parser
            match parseString parser with
            | Error message -> error <- Some message
            | Ok key ->
                skipWhitespace parser
                if parser.Offset >= parser.Text.Length || parser.Text[parser.Offset] <> ':' then
                    error <- Some("Expected colon in JSON object at character " + string parser.Offset + ".")
                else
                    parser.Offset <- parser.Offset + 1
                    match parseValue parser with
                    | Error message -> error <- Some message
                    | Ok value ->
                        values.Add(key, value)
                        skipWhitespace parser
                        if parser.Offset >= parser.Text.Length then error <- Some "Unterminated JSON object."
                        elif parser.Text[parser.Offset] = '}' then
                            parser.Offset <- parser.Offset + 1
                            complete <- true
                        elif parser.Text[parser.Offset] = ',' then parser.Offset <- parser.Offset + 1
                        else error <- Some("Expected comma in JSON object at character " + string parser.Offset + ".")
        match error with
        | Some message -> Error message
        | None ->
            let duplicate =
                values
                |> Seq.countBy fst
                |> Seq.tryFind (fun (_, count) -> count > 1)
            match duplicate with
            | Some(key, _) -> Error("Duplicate JSON field '" + key + "' is ambiguous.")
            | None -> values |> Map.ofSeq |> JObject |> Ok

    let private parseJson text =
        let parser = { Text = text; Offset = 0 }
        match parseValue parser with
        | Error message -> Error message
        | Ok value ->
            skipWhitespace parser
            if parser.Offset <> text.Length then fail parser "Trailing JSON content"
            else Ok value

    let private property name = function
        | JObject fields -> Map.tryFind name fields
        | _ -> None

    let private numberAt path value =
        (Some value, path)
        ||> List.fold (fun current name -> current |> Option.bind (property name))
        |> Option.bind (function JNumber number -> Some number | _ -> None)

    let private arrayAt path value =
        (Some value, path)
        ||> List.fold (fun current name -> current |> Option.bind (property name))
        |> Option.bind (function JArray values -> Some values | _ -> None)

    let private integer (value: float) =
        let rounded = Math.Round value
        if abs (value - rounded) < 0.000001
           && rounded >= float Int32.MinValue
           && rounded <= float Int32.MaxValue then Some(int32 rounded)
        else None

    let rec private leafPaths prefix value =
        match value with
        | JObject fields when fields.IsEmpty -> [ prefix ]
        | JObject fields ->
            fields
            |> Map.toList
            |> List.collect (fun (name, child) ->
                leafPaths (if prefix = "" then name else prefix + "." + name) child)
        | JArray values when Array.isEmpty values -> [ prefix ]
        | JArray values ->
            values
            |> Array.mapi (fun index child -> leafPaths (prefix + "[" + string index + "]") child)
            |> Array.toList
            |> List.concat
        | _ -> [ prefix ]

    let private point = function
        | JObject fields ->
            match Map.tryFind "x" fields, Map.tryFind "y" fields with
            | Some(JNumber x), Some(JNumber y) -> Some(x, y)
            | _ -> None
        | JArray [| JNumber x; JNumber y |] -> Some(x, y)
        | _ -> None

    let private blankMap width height =
        { Width = width
          Height = height
          Terrain = Map.empty
          Edges = Map.empty
          Units = Map.empty
          NextUnitId = 1
          Regions = Map.empty
          NextRegionId = 1 }

    let private splitGridSegment width height pixelsPerGrid originX originY kind isOpen first second =
        let gridCoordinate x origin = (x - origin) / pixelsPerGrid
        let x1, y1 = first
        let x2, y2 = second
        let gx1, gy1 = gridCoordinate x1 originX, gridCoordinate y1 originY
        let gx2, gy2 = gridCoordinate x2 originX, gridCoordinate y2 originY
        match integer gx1, integer gy1, integer gx2, integer gy2 with
        | Some x1, Some y1, Some x2, Some y2 when (x1 = x2) <> (y1 = y2) ->
            let length = abs (x2 - x1) + abs (y2 - y1)
            let mapped =
                [| for step in 0 .. int length - 1 do
                       let sx = x1 + int32 step * Math.Sign(x2 - x1)
                       let sy = y1 + int32 step * Math.Sign(y2 - y1)
                       let ex = sx + Math.Sign(x2 - x1)
                       let ey = sy + Math.Sign(y2 - y1)
                       match MapEditor.tryNormalizeEdge width height sx sy ex ey with
                       | Some edge -> yield edge, (kind, isOpen)
                       | None -> () |]
            mapped,
            if mapped.Length = int length then None
            else Some "segment includes map-border geometry that S.I.R.'s east/south edge records cannot represent"
        | _ -> [||], Some "segment is diagonal, curved, off-grid, or not representable on the top/left map border"

    let private ignoredLeaves root consumed =
        leafPaths "" root
        |> List.filter (fun path ->
            consumed
            |> Seq.exists (fun prefix ->
                path = prefix
                || path.StartsWith(prefix + ".", StringComparison.Ordinal)
                || path.StartsWith(prefix + "[", StringComparison.Ordinal))
            |> not)
        |> List.map (fun path ->
            { Path = path
              Disposition = Ignored
              Meaning = "No authoritative S.I.R. semantic mapping; retained only in the source file." })

    let private finish format sourceName root dimensions edges reports consumed errors =
        let candidate =
            match dimensions, errors with
            | Some(width, height), [] ->
                let edgeMap =
                    edges
                    |> Seq.groupBy fst
                    |> Seq.map (fun (key, occurrences) ->
                        let meanings = occurrences |> Seq.map snd |> Seq.distinct |> Seq.toArray
                        key, meanings)
                    |> Seq.toArray
                if edgeMap |> Array.exists (fun (_, meanings) -> meanings.Length <> 1) then None
                else
                    Some
                        { blankMap width height with
                            Edges = edgeMap |> Array.map (fun (key, meanings) -> key, meanings[0]) |> Map.ofArray }
            | _ -> None
        { Format = format
          SourceName = sourceName
          Candidate = candidate
          Fields = Array.ofList (reports @ ignoredLeaves root consumed)
          Errors =
            [ yield! errors
              if candidate.IsNone && List.isEmpty errors then
                  "Conflicting duplicate semantic edges prevent deterministic import." ]
            |> List.toArray }

    let private evaluateUniversal sourceName root =
        let width = numberAt [ "resolution"; "map_size"; "x" ] root |> Option.bind integer
        let height = numberAt [ "resolution"; "map_size"; "y" ] root |> Option.bind integer
        let scale = numberAt [ "resolution"; "pixels_per_grid" ] root
        let originX = numberAt [ "resolution"; "map_origin"; "x" ] root |> Option.defaultValue 0.0
        let originY = numberAt [ "resolution"; "map_origin"; "y" ] root |> Option.defaultValue 0.0
        let dimensions =
            match width, height, scale with
            | Some w, Some h, Some pixels when w >= 4 && h >= 4 && w <= 80 && h <= 80 && pixels > 0.0 ->
                Some(w, h)
            | _ -> None
        let errors =
            if dimensions.IsNone then [ "UVTT resolution must provide integral map_size 4–40 and positive pixels_per_grid." ]
            else []
        let consumed = ResizeArray<string>()
        [ "resolution.map_size.x"; "resolution.map_size.y"; "resolution.pixels_per_grid"; "resolution.map_origin.x"; "resolution.map_origin.y" ]
        |> List.iter consumed.Add
        let reports = ResizeArray<InterchangeFieldReport>()
        reports.Add { Path = "resolution"; Disposition = Mapped; Meaning = "Map size and pixel-to-cell transform." }
        let edges = ResizeArray<_>()
        match dimensions, scale, arrayAt [ "line_of_sight" ] root with
        | Some(w, h), Some pixels, Some polylines ->
            for lineIndex, line in Array.indexed polylines do
                let path = "line_of_sight[" + string lineIndex + "]"
                match line with
                | JArray points ->
                    for pointIndex in 0 .. points.Length - 1 do
                        consumed.Add(path + "[" + string pointIndex + "].x")
                        consumed.Add(path + "[" + string pointIndex + "].y")
                        consumed.Add(path + "[" + string pointIndex + "][0]")
                        consumed.Add(path + "[" + string pointIndex + "][1]")
                    for segmentIndex in 0 .. points.Length - 2 do
                        match point points[segmentIndex], point points[segmentIndex + 1] with
                        | Some first, Some second ->
                            let mapped, loss = splitGridSegment w h pixels originX originY Wall false first second
                            edges.AddRange mapped
                            reports.Add
                                { Path = path + "[" + string segmentIndex + ".." + string (segmentIndex + 1) + "]"
                                  Disposition = if loss.IsNone then Mapped else Lossy
                                  Meaning = loss |> Option.defaultValue "Axis-aligned grid segment mapped to closed S.I.R. wall edges." }
                        | _ -> reports.Add { Path = path; Disposition = Lossy; Meaning = "Malformed line-of-sight point was not imported." }
                | _ -> reports.Add { Path = path; Disposition = Lossy; Meaning = "Line-of-sight entry is not a point array." }
        | _ -> ()
        match dimensions, scale, arrayAt [ "portals" ] root with
        | Some(w, h), Some pixels, Some portals ->
            for portalIndex, portal in Array.indexed portals do
                let path = "portals[" + string portalIndex + "]"
                consumed.Add(path + ".closed")
                let bounds = property "bounds" portal |> Option.bind (function JArray values -> Some values | _ -> None)
                let closed =
                    property "closed" portal
                    |> Option.bind (function JBool value -> Some value | _ -> None)
                    |> Option.defaultValue true
                match bounds with
                | Some [| firstValue; secondValue |] ->
                    for pointIndex in 0 .. 1 do
                        consumed.Add(path + ".bounds[" + string pointIndex + "].x")
                        consumed.Add(path + ".bounds[" + string pointIndex + "].y")
                        consumed.Add(path + ".bounds[" + string pointIndex + "][0]")
                        consumed.Add(path + ".bounds[" + string pointIndex + "][1]")
                    match point firstValue, point secondValue with
                    | Some first, Some second ->
                        let mapped, loss = splitGridSegment w h pixels originX originY Door (not closed) first second
                        edges.AddRange mapped
                        reports.Add
                            { Path = path
                              Disposition = if loss.IsNone then Mapped else Lossy
                              Meaning = loss |> Option.defaultValue "Portal mapped to semantic door edge(s), preserving open/closed state." }
                    | _ -> reports.Add { Path = path; Disposition = Lossy; Meaning = "Portal bounds were malformed." }
                | _ -> reports.Add { Path = path; Disposition = Lossy; Meaning = "Portal without exactly two bounds points was ignored." }
        | _ -> ()
        finish UniversalVtt sourceName root dimensions edges (List.ofSeq reports) consumed errors

    let private evaluateFoundry sourceName root =
        let pixelWidth = numberAt [ "width" ] root
        let pixelHeight = numberAt [ "height" ] root
        let gridSize =
            numberAt [ "grid"; "size" ] root
            |> Option.orElseWith (fun () -> numberAt [ "grid" ] root)
        let gridType =
            numberAt [ "grid"; "type" ] root
            |> Option.orElseWith (fun () -> numberAt [ "gridType" ] root)
            |> Option.defaultValue 1.0
        let dimensions =
            match pixelWidth, pixelHeight, gridSize with
            | Some pw, Some ph, Some size when gridType = 1.0 && size > 0.0 ->
                match integer (pw / size), integer (ph / size) with
                | Some w, Some h when w >= 4 && h >= 4 && w <= 80 && h <= 80 -> Some(w, h)
                | _ -> None
            | _ -> None
        let errors =
            if dimensions.IsNone then [ "Foundry scene must use a square grid and resolve to an integral 4–40-cell board." ]
            else []
        let consumed = ResizeArray<string>()
        [ "width"; "height"; "grid.size"; "grid.type"; "gridType" ] |> List.iter consumed.Add
        match property "grid" root with
        | Some(JNumber _) -> consumed.Add "grid"
        | _ -> ()
        let reports = ResizeArray<InterchangeFieldReport>()
        reports.Add { Path = "width,height,grid"; Disposition = Mapped; Meaning = "Square-grid scene extent mapped to S.I.R. cells." }
        let edges = ResizeArray<_>()
        match dimensions, gridSize, arrayAt [ "walls" ] root with
        | Some(w, h), Some pixels, Some walls ->
            for index, wall in Array.indexed walls do
                let path = "walls[" + string index + "]"
                consumed.Add(path + ".c")
                consumed.Add(path + ".door")
                consumed.Add(path + ".ds")
                let coordinates = property "c" wall |> Option.bind (function JArray values -> Some values | _ -> None)
                let doorCode = property "door" wall |> Option.bind (function JNumber value -> integer value | _ -> None) |> Option.defaultValue 0
                let stateCode = property "ds" wall |> Option.bind (function JNumber value -> integer value | _ -> None) |> Option.defaultValue 0
                match coordinates with
                | Some [| JNumber x1; JNumber y1; JNumber x2; JNumber y2 |] ->
                    let kind = if doorCode > 0 then Door else Wall
                    let mapped, geometryLoss = splitGridSegment w h pixels 0.0 0.0 kind (doorCode > 0 && stateCode = 1) (x1, y1) (x2, y2)
                    edges.AddRange mapped
                    let lockedLoss = doorCode > 0 && stateCode > 1
                    reports.Add
                        { Path = path
                          Disposition = if geometryLoss.IsSome || lockedLoss then Lossy else Mapped
                          Meaning =
                            if lockedLoss then "Foundry locked-door state has no S.I.R. equivalent and was mapped to closed."
                            else geometryLoss |> Option.defaultValue "Wall coordinates mapped to semantic wall/door edge(s)." }
                | _ -> reports.Add { Path = path; Disposition = Lossy; Meaning = "Wall without four numeric coordinates was ignored." }
        | _ -> ()
        finish FoundryScene sourceName root dimensions edges (List.ofSeq reports) consumed errors

    let evaluate format sourceName (text: string) =
        if (Encoding.UTF8.GetBytes text).Length > MapEditor.MaximumImportBytes then
            { Format = format
              SourceName = sourceName
              Candidate = None
              Fields = [||]
              Errors =
                [| "Interchange input is missing or exceeds the "
                   + string MapEditor.MaximumImportBytes
                   + "-byte qualification limit." |] }
        else
            match format with
            | FantasyGroundsImage ->
                { Format = format
                  SourceName = sourceName
                  Candidate = None
                  Fields =
                    [| { Path = "image/grid/occluder XML"
                         Disposition = RejectedField
                         Meaning = "Fantasy Grounds image exports vary by campaign/database schema and encode paint, occluders, assets, and extensions without a stable portable semantic contract." } |]
                  Errors =
                    [| "Fantasy Grounds XML import is evaluation-only: no deterministic, reviewable mapping is accepted. Export through Universal VTT or author semantic edges in S.I.R." |] }
            | UniversalVtt
            | FoundryScene ->
                match parseJson text with
                | Error message ->
                    { Format = format
                      SourceName = sourceName
                      Candidate = None
                      Fields = [||]
                      Errors = [| message |] }
                | Ok root ->
                    match format with
                    | UniversalVtt -> evaluateUniversal sourceName root
                    | FoundryScene -> evaluateFoundry sourceName root
                    | FantasyGroundsImage -> failwith "unreachable"

    let canAccept review =
        review.Candidate.IsSome && Array.isEmpty review.Errors

    let accept review =
        if canAccept review then
            match review.Candidate with
            | Some candidate -> Ok candidate
            | None -> Error "Interchange review has no candidate map."
        else Error(String.concat " " review.Errors)
