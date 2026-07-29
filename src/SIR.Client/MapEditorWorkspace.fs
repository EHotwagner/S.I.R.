namespace SIR.Client

open System
open SIR.Domain

type BackgroundFit =
    | FitInside
    | FillAndCrop
    | StretchToBoard
    | NativePixels

type BackgroundCrop =
    { Left: int32
      Top: int32
      Width: int32
      Height: int32 }

type LocalRasterBackground =
    { AssetId: string
      FileName: string
      MediaType: string
      PixelWidth: int32
      PixelHeight: int32
      ByteLength: int
      DataUrl: string
      Locked: bool
      Opacity: float
      Fit: BackgroundFit
      Crop: BackgroundCrop option
      GridOffsetX: float
      GridOffsetY: float
      PixelsPerCell: float }

type EditorPointerKind =
    | MousePointer
    | PenPointer
    | TouchPointer

type EditorPointer =
    { Id: int32
      Kind: EditorPointerKind
      X: float
      Y: float
      RequestsPan: bool }

type MapCellHit =
    { Column: int32
      Row: int32 }

type MapEdgeHit =
    { Column: int32
      Row: int32
      Direction: MapEdgeDirection
      DistancePixels: float }

type EditorWorkspaceState =
    { Camera: BattlefieldCamera
      ViewportWidth: float
      ViewportHeight: float
      InspectorCollapsed: bool
      ReducedMotion: bool
      CapturedPointers: Map<int32, EditorPointer>
      Background: LocalRasterBackground option
      BackgroundAnnouncement: string }

type EditorWorkspaceAction =
    | ResizeViewport of width: float * height: float
    | PanEditorBy of x: float * y: float
    | ZoomEditorAt of x: float * y: float * factor: float
    | FitEditorBoard
    | FrameEditorSelection
    | ResetEditorCamera
    | ToggleEditorInspector
    | SetEditorReducedMotion of bool
    | StartEditorPointer of EditorPointer
    | MoveEditorPointer of EditorPointer
    | EndEditorPointer of pointerId: int32
    | LoseEditorPointerCapture of pointerId: int32
    | CancelEditorPointers
    | AttachLocalRaster of fileName: string * declaredMediaType: string * bytes: byte array
    | RemoveLocalRaster
    | ToggleBackgroundLock
    | SetBackgroundOpacity of float
    | SetBackgroundFit of BackgroundFit
    | SetBackgroundCrop of BackgroundCrop option
    | SetBackgroundGridOffset of x: float * y: float
    | NudgeBackgroundGridOffset of x: float * y: float
    | SetBackgroundPixelsPerCell of float
    | AlignBackgroundGrid of firstImageX: float * firstImageY: float * secondImageX: float * secondImageY: float * cellsBetween: int32

[<RequireQualifiedAccess>]
module MapEditorWorkspace =
    [<Literal>]
    let MinimumZoom = 0.25

    [<Literal>]
    let MaximumZoom = 6.0

    [<Literal>]
    let DefaultViewportWidth = 960.0

    [<Literal>]
    let DefaultViewportHeight = 640.0

    [<Literal>]
    let CameraPadding = 36.0

    [<Literal>]
    let EdgeTolerancePixels = 9.0

    [<Literal>]
    let MaximumBackgroundBytes = 10_000_000

    [<Literal>]
    let MaximumBackgroundDimension = 8192

    let initial reducedMotion =
        { Camera =
            { PanX = CameraPadding
              PanY = CameraPadding
              Zoom = 1.0 }
          ViewportWidth = DefaultViewportWidth
          ViewportHeight = DefaultViewportHeight
          InspectorCollapsed = true
          ReducedMotion = reducedMotion
          CapturedPointers = Map.empty
          Background = None
          BackgroundAnnouncement = "No local background selected." }

    let private hex (bytes: byte array) =
        bytes
        |> Array.map (fun value -> value.ToString("x2"))
        |> String.concat ""

    let private readBigEndian32 (bytes: byte array) offset =
        (int32 bytes[offset] <<< 24)
        ||| (int32 bytes[offset + 1] <<< 16)
        ||| (int32 bytes[offset + 2] <<< 8)
        ||| int32 bytes[offset + 3]

    let private readLittleEndian24 (bytes: byte array) offset =
        int32 bytes[offset]
        ||| (int32 bytes[offset + 1] <<< 8)
        ||| (int32 bytes[offset + 2] <<< 16)

    let private readLittleEndian16 (bytes: byte array) offset =
        int32 bytes[offset] ||| (int32 bytes[offset + 1] <<< 8)

    let private tryPngDimensions (bytes: byte array) =
        let signature = [| 137uy; 80uy; 78uy; 71uy; 13uy; 10uy; 26uy; 10uy |]
        if bytes.Length >= 24 && bytes[0..7] = signature && bytes[12..15] = [| 73uy; 72uy; 68uy; 82uy |] then
            Some(readBigEndian32 bytes 16, readBigEndian32 bytes 20, "image/png")
        else
            None

    let private tryWebpDimensions (bytes: byte array) =
        let ascii offset (value: string) =
            bytes.Length >= offset + value.Length
            && value
               |> Seq.mapi (fun index character -> bytes[offset + index] = byte character)
               |> Seq.forall id
        if bytes.Length < 30 || not (ascii 0 "RIFF" && ascii 8 "WEBP") then None
        elif ascii 12 "VP8X" then
            Some(readLittleEndian24 bytes 24 + 1, readLittleEndian24 bytes 27 + 1, "image/webp")
        elif ascii 12 "VP8L" && bytes.Length >= 25 then
            let bits =
                uint32 bytes[21]
                ||| (uint32 bytes[22] <<< 8)
                ||| (uint32 bytes[23] <<< 16)
                ||| (uint32 bytes[24] <<< 24)
            Some(int32 (bits &&& 0x3fffu) + 1, int32 ((bits >>> 14) &&& 0x3fffu) + 1, "image/webp")
        elif ascii 12 "VP8 " && bytes.Length >= 30 && bytes[23..25] = [| 0x9duy; 0x01uy; 0x2auy |] then
            Some(readLittleEndian16 bytes 26 &&& 0x3fff, readLittleEndian16 bytes 28 &&& 0x3fff, "image/webp")
        else None

    let private tryJpegDimensions (bytes: byte array) =
        if bytes.Length < 4 || bytes[0] <> 0xffuy || bytes[1] <> 0xd8uy then None
        else
            let mutable offset = 2
            let mutable result = None
            while result.IsNone && offset + 3 < bytes.Length do
                if bytes[offset] <> 0xffuy then offset <- offset + 1
                else
                    let marker = int bytes[offset + 1]
                    if marker = 0xd9 || marker = 0xda then offset <- bytes.Length
                    elif marker >= 0xd0 && marker <= 0xd7 then offset <- offset + 2
                    else
                        let length = int bytes[offset + 2] * 256 + int bytes[offset + 3]
                        let isStartOfFrame =
                            [ 0xc0; 0xc1; 0xc2; 0xc3; 0xc5; 0xc6; 0xc7; 0xc9; 0xca; 0xcb; 0xcd; 0xce; 0xcf ]
                            |> List.contains marker
                        if length < 2 || offset + 2 + length > bytes.Length then offset <- bytes.Length
                        elif isStartOfFrame && length >= 7 then
                            let height = int32 bytes[offset + 5] * 256 + int32 bytes[offset + 6]
                            let width = int32 bytes[offset + 7] * 256 + int32 bytes[offset + 8]
                            result <- Some(width, height, "image/jpeg")
                        else offset <- offset + 2 + length
            result

    let tryCreateLocalRaster fileName declaredMediaType (bytes: byte array) =
        if Array.isEmpty bytes then Error "BACKGROUND-EMPTY: the selected file is empty."
        elif bytes.Length > MaximumBackgroundBytes then
            Error("BACKGROUND-SIZE: local backgrounds are limited to " + string MaximumBackgroundBytes + " bytes.")
        else
            let dimensions =
                tryPngDimensions bytes
                |> Option.orElseWith (fun () -> tryJpegDimensions bytes)
                |> Option.orElseWith (fun () -> tryWebpDimensions bytes)
            match dimensions with
            | None -> Error "BACKGROUND-TYPE: only signature-validated PNG, JPEG, and WebP raster files are accepted; SVG and executable content are rejected."
            | Some(width, height, mediaType) when width < 1 || height < 1 || width > MaximumBackgroundDimension || height > MaximumBackgroundDimension ->
                Error("BACKGROUND-DIMENSIONS: raster dimensions must be between 1 and " + string MaximumBackgroundDimension + " pixels.")
            | Some(width, height, mediaType) when
                not (String.IsNullOrWhiteSpace declaredMediaType)
                && declaredMediaType <> mediaType ->
                Error("BACKGROUND-MEDIA-TYPE: declared type " + declaredMediaType + " does not match " + mediaType + ".")
            | Some(width, height, mediaType) ->
                let digest = CanonicalHash.sha256 bytes |> hex
                Ok
                    { AssetId = "sha256:" + digest
                      FileName = fileName |> Option.ofObj |> Option.defaultValue "local-background"
                      MediaType = mediaType
                      PixelWidth = width
                      PixelHeight = height
                      ByteLength = bytes.Length
                      DataUrl = "data:" + mediaType + ";base64," + Convert.ToBase64String bytes
                      Locked = true
                      Opacity = 0.65
                      Fit = FitInside
                      Crop = None
                      GridOffsetX = 0.0
                      GridOffsetY = 0.0
                      PixelsPerCell = Battlefield.CellSize }

    let backgroundRenderBox boardWidth boardHeight (background: LocalRasterBackground) =
        let source =
            match background.Crop with
            | Some crop -> float crop.Width, float crop.Height
            | None -> float background.PixelWidth, float background.PixelHeight
        let sourceWidth, sourceHeight = source
        let targetWidth = float boardWidth * Battlefield.CellSize
        let targetHeight = float boardHeight * Battlefield.CellSize
        let scaleX, scaleY =
            match background.Fit with
            | FitInside ->
                let scale = min (targetWidth / sourceWidth) (targetHeight / sourceHeight)
                scale, scale
            | FillAndCrop ->
                let scale = max (targetWidth / sourceWidth) (targetHeight / sourceHeight)
                scale, scale
            | StretchToBoard -> targetWidth / sourceWidth, targetHeight / sourceHeight
            | NativePixels ->
                let scale = Battlefield.CellSize / max 1.0 background.PixelsPerCell
                scale, scale
        let renderedWidth = sourceWidth * scaleX
        let renderedHeight = sourceHeight * scaleY
        let centeredX, centeredY =
            match background.Fit with
            | FitInside
            | FillAndCrop ->
                (targetWidth - renderedWidth) / 2.0,
                (targetHeight - renderedHeight) / 2.0
            | StretchToBoard
            | NativePixels -> 0.0, 0.0
        centeredX + background.GridOffsetX,
        centeredY + background.GridOffsetY,
        renderedWidth,
        renderedHeight

    let private finiteOr fallback value =
        if Double.IsNaN value || Double.IsInfinity value then fallback else value

    let private clamp minimum maximum value =
        max minimum (min maximum value)

    let private boundedZoom value =
        value
        |> finiteOr 1.0
        |> clamp MinimumZoom MaximumZoom

    let clientToViewportPoint
        viewportWidth
        viewportHeight
        renderedWidth
        renderedHeight
        localX
        localY
        =
        let viewportWidth = max 1.0 (finiteOr DefaultViewportWidth viewportWidth)
        let viewportHeight = max 1.0 (finiteOr DefaultViewportHeight viewportHeight)
        let renderedWidth = max 1.0 (finiteOr viewportWidth renderedWidth)
        let renderedHeight = max 1.0 (finiteOr viewportHeight renderedHeight)
        let scale =
            max
                0.000_001
                (min
                    (renderedWidth / viewportWidth)
                    (renderedHeight / viewportHeight))
        let contentWidth = viewportWidth * scale
        let contentHeight = viewportHeight * scale
        let offsetX = (renderedWidth - contentWidth) / 2.0
        let offsetY = (renderedHeight - contentHeight) / 2.0
        (finiteOr 0.0 localX - offsetX) / scale,
        (finiteOr 0.0 localY - offsetY) / scale

    let screenToBoard camera screenX screenY =
        (screenX - camera.PanX) / camera.Zoom,
        (screenY - camera.PanY) / camera.Zoom

    let boardToScreen camera boardX boardY =
        camera.PanX + boardX * camera.Zoom,
        camera.PanY + boardY * camera.Zoom

    let zoomAt screenX screenY factor camera =
        let boardX, boardY = screenToBoard camera screenX screenY
        let zoom = boundedZoom (camera.Zoom * finiteOr 1.0 factor)

        { PanX = screenX - boardX * zoom
          PanY = screenY - boardY * zoom
          Zoom = zoom }

    let panBy x y camera =
        { camera with
            PanX = camera.PanX + finiteOr 0.0 x
            PanY = camera.PanY + finiteOr 0.0 y }

    let fitBounds
        viewportWidth
        viewportHeight
        minimumX
        minimumY
        maximumX
        maximumY
        =
        let viewportWidth = max 1.0 (finiteOr DefaultViewportWidth viewportWidth)
        let viewportHeight = max 1.0 (finiteOr DefaultViewportHeight viewportHeight)
        let width = max 1.0 (maximumX - minimumX)
        let height = max 1.0 (maximumY - minimumY)
        let availableWidth = max 1.0 (viewportWidth - CameraPadding * 2.0)
        let availableHeight = max 1.0 (viewportHeight - CameraPadding * 2.0)
        let zoom = boundedZoom (min (availableWidth / width) (availableHeight / height))
        let renderedWidth = width * zoom
        let renderedHeight = height * zoom

        { PanX = (viewportWidth - renderedWidth) / 2.0 - minimumX * zoom
          PanY = (viewportHeight - renderedHeight) / 2.0 - minimumY * zoom
          Zoom = zoom }

    let fitBoard viewportWidth viewportHeight boardWidth boardHeight =
        fitBounds
            viewportWidth
            viewportHeight
            0.0
            0.0
            (float boardWidth * Battlefield.CellSize)
            (float boardHeight * Battlefield.CellSize)

    let frameSelection viewportWidth viewportHeight (unit: EditorUnit option) fallback =
        match unit with
        | None -> fallback
        | Some unit ->
            let inset = Battlefield.CellSize * 0.35
            let minimumX = float unit.Column * Battlefield.CellSize - inset
            let minimumY = float unit.Row * Battlefield.CellSize - inset
            let size = float unit.Size * Battlefield.CellSize

            fitBounds
                viewportWidth
                viewportHeight
                minimumX
                minimumY
                (minimumX + size + inset * 2.0)
                (minimumY + size + inset * 2.0)

    let tryHitCell width height camera screenX screenY =
        let boardX, boardY = screenToBoard camera screenX screenY
        let column = int32 (Math.Floor(boardX / Battlefield.CellSize))
        let row = int32 (Math.Floor(boardY / Battlefield.CellSize))

        if column >= 0 && row >= 0 && column < width && row < height then
            Some { Column = column; Row = row }
        else
            None

    let tryHitEdge width height camera tolerancePixels screenX screenY =
        let boardX, boardY = screenToBoard camera screenX screenY
        let cellSize = Battlefield.CellSize
        let column = int32 (Math.Floor(boardX / cellSize))
        let row = int32 (Math.Floor(boardY / cellSize))
        let verticalBoundary = Math.Round(boardX / cellSize) * cellSize
        let horizontalBoundary = Math.Round(boardY / cellSize) * cellSize
        let verticalDistance = abs (boardX - verticalBoundary) * camera.Zoom
        let horizontalDistance = abs (boardY - horizontalBoundary) * camera.Zoom
        let tolerance = max 0.0 tolerancePixels
        let verticalColumn = int32 (Math.Round(boardX / cellSize)) - 1
        let horizontalRow = int32 (Math.Round(boardY / cellSize)) - 1

        let vertical =
            if
                verticalDistance <= tolerance
                && verticalColumn >= 0
                && verticalColumn < width
                && row >= 0
                && row < height
            then
                Some
                    { Column = verticalColumn
                      Row = row
                      Direction = EastEdge
                      DistancePixels = verticalDistance }
            else
                None

        let horizontal =
            if
                horizontalDistance <= tolerance
                && column >= 0
                && column < width
                && horizontalRow >= 0
                && horizontalRow < height
            then
                Some
                    { Column = column
                      Row = horizontalRow
                      Direction = SouthEdge
                      DistancePixels = horizontalDistance }
            else
                None

        match vertical, horizontal with
        | Some x, Some y when y.DistancePixels < x.DistancePixels -> Some y
        | Some x, _ -> Some x
        | _, Some y -> Some y
        | _ -> None

    let private touchPair pointers =
        pointers
        |> Map.toArray
        |> Array.map snd
        |> Array.filter (fun pointer -> pointer.Kind = TouchPointer)
        |> Array.sortBy _.Id
        |> Array.truncate 2

    let private applyTouchMove previousPointers nextPointer camera =
        let nextPointers = Map.add nextPointer.Id nextPointer previousPointers
        let before = touchPair previousPointers
        let after = touchPair nextPointers

        if before.Length = 2 && after.Length = 2 then
            let midpoint (pair: EditorPointer array) =
                (pair.[0].X + pair.[1].X) / 2.0,
                (pair.[0].Y + pair.[1].Y) / 2.0
            let distance (pair: EditorPointer array) =
                let dx = pair.[1].X - pair.[0].X
                let dy = pair.[1].Y - pair.[0].Y
                sqrt (dx * dx + dy * dy)
            let beforeX, beforeY = midpoint before
            let afterX, afterY = midpoint after
            let beforeDistance = distance before
            let factor =
                if beforeDistance < 0.001 then 1.0
                else distance after / beforeDistance

            camera
            |> zoomAt beforeX beforeY factor
            |> panBy (afterX - beforeX) (afterY - beforeY)
        else
            camera

    let update
        (map: MapDefinition)
        (selected: EditorUnit option)
        action
        state
        =
        match action with
        | ResizeViewport(width, height) ->
            { state with
                ViewportWidth = max 1.0 (finiteOr state.ViewportWidth width)
                ViewportHeight = max 1.0 (finiteOr state.ViewportHeight height) }
        | PanEditorBy(x, y) ->
            { state with Camera = panBy x y state.Camera }
        | ZoomEditorAt(x, y, factor) ->
            { state with Camera = zoomAt x y factor state.Camera }
        | FitEditorBoard ->
            { state with
                Camera =
                    fitBoard
                        state.ViewportWidth
                        state.ViewportHeight
                        map.Width
                        map.Height }
        | FrameEditorSelection ->
            { state with
                Camera =
                    frameSelection
                        state.ViewportWidth
                        state.ViewportHeight
                        selected
                        state.Camera }
        | ResetEditorCamera ->
            { state with
                Camera =
                    { PanX = CameraPadding
                      PanY = CameraPadding
                      Zoom = 1.0 }
                CapturedPointers = Map.empty }
        | ToggleEditorInspector ->
            { state with InspectorCollapsed = not state.InspectorCollapsed }
        | SetEditorReducedMotion value ->
            { state with ReducedMotion = value }
        | StartEditorPointer pointer ->
            { state with
                CapturedPointers = Map.add pointer.Id pointer state.CapturedPointers }
        | MoveEditorPointer pointer ->
            match Map.tryFind pointer.Id state.CapturedPointers with
            | None -> state
            | Some previous ->
                let camera =
                    if pointer.Kind = TouchPointer then
                        applyTouchMove state.CapturedPointers pointer state.Camera
                    elif previous.RequestsPan then
                        panBy (pointer.X - previous.X) (pointer.Y - previous.Y) state.Camera
                    else
                        state.Camera

                { state with
                    Camera = camera
                    CapturedPointers =
                        Map.add pointer.Id pointer state.CapturedPointers }
        | EndEditorPointer pointerId
        | LoseEditorPointerCapture pointerId ->
            { state with
                CapturedPointers = Map.remove pointerId state.CapturedPointers }
        | CancelEditorPointers ->
            { state with CapturedPointers = Map.empty }
        | AttachLocalRaster(fileName, declaredMediaType, bytes) ->
            match tryCreateLocalRaster fileName declaredMediaType bytes with
            | Error error -> { state with BackgroundAnnouncement = error }
            | Ok background ->
                { state with
                    Background = Some background
                    BackgroundAnnouncement =
                        "Local " + background.MediaType + " background accepted: "
                        + string background.PixelWidth + " by " + string background.PixelHeight
                        + " pixels, locked." }
        | RemoveLocalRaster ->
            { state with
                Background = None
                BackgroundAnnouncement = "Local background removed." }
        | ToggleBackgroundLock ->
            match state.Background with
            | None -> state
            | Some background ->
                let next = { background with Locked = not background.Locked }
                { state with
                    Background = Some next
                    BackgroundAnnouncement = if next.Locked then "Background locked." else "Background unlocked." }
        | SetBackgroundOpacity opacity ->
            { state with
                Background =
                    state.Background
                    |> Option.map (fun background ->
                        { background with Opacity = clamp 0.0 1.0 (finiteOr background.Opacity opacity) }) }
        | SetBackgroundFit fit ->
            match state.Background with
            | Some background when not background.Locked ->
                { state with Background = Some { background with Fit = fit } }
            | Some _ -> { state with BackgroundAnnouncement = "Unlock the background before changing its fit." }
            | None -> state
        | SetBackgroundCrop crop ->
            match state.Background with
            | Some background when not background.Locked ->
                let valid =
                    crop
                    |> Option.forall (fun value ->
                        value.Left >= 0 && value.Top >= 0
                        && value.Width > 0 && value.Height > 0
                        && value.Left + value.Width <= background.PixelWidth
                        && value.Top + value.Height <= background.PixelHeight)
                if not valid then
                    { state with BackgroundAnnouncement = "BACKGROUND-CROP: crop bounds must stay within the raster." }
                else { state with Background = Some { background with Crop = crop } }
            | Some _ -> { state with BackgroundAnnouncement = "Unlock the background before cropping it." }
            | None -> state
        | SetBackgroundGridOffset(x, y) ->
            match state.Background with
            | Some background when not background.Locked ->
                { state with Background = Some { background with GridOffsetX = finiteOr 0.0 x; GridOffsetY = finiteOr 0.0 y } }
            | Some _ -> { state with BackgroundAnnouncement = "Unlock the background before moving it." }
            | None -> state
        | NudgeBackgroundGridOffset(x, y) ->
            match state.Background with
            | Some background when not background.Locked ->
                { state with Background = Some { background with GridOffsetX = background.GridOffsetX + finiteOr 0.0 x; GridOffsetY = background.GridOffsetY + finiteOr 0.0 y } }
            | Some _ -> { state with BackgroundAnnouncement = "Unlock the background before moving it." }
            | None -> state
        | SetBackgroundPixelsPerCell pixels ->
            match state.Background with
            | Some background when not background.Locked ->
                if pixels < 1.0 || pixels > float MaximumBackgroundDimension then
                    { state with BackgroundAnnouncement = "BACKGROUND-GRID-SCALE: pixels per cell must be within the supported raster dimensions." }
                else
                    { state with Background = Some { background with PixelsPerCell = pixels } }
            | Some _ -> { state with BackgroundAnnouncement = "Unlock the background before changing its scale." }
            | None -> state
        | AlignBackgroundGrid(firstX, firstY, secondX, secondY, cellsBetween) ->
            match state.Background with
            | Some background when not background.Locked && cellsBetween > 0 ->
                let horizontal = abs (secondY - firstY) < 0.000001
                let vertical = abs (secondX - firstX) < 0.000001
                let distance =
                    if horizontal then abs (secondX - firstX)
                    elif vertical then abs (secondY - firstY)
                    else 0.0
                let pixelsPerCell = distance / float cellsBetween
                if not horizontal && not vertical then
                    { state with BackgroundAnnouncement = "BACKGROUND-ALIGNMENT: rotated source grids are not supported; choose two points on one horizontal or vertical grid line." }
                elif pixelsPerCell < 1.0 then
                    { state with BackgroundAnnouncement = "BACKGROUND-ALIGNMENT: alignment points must span at least one pixel per cell." }
                else
                    { state with
                        Background =
                            Some { background with GridOffsetX = -firstX * Battlefield.CellSize / pixelsPerCell; GridOffsetY = -firstY * Battlefield.CellSize / pixelsPerCell; PixelsPerCell = pixelsPerCell; Fit = NativePixels }
                        BackgroundAnnouncement = "Background grid aligned at " + string pixelsPerCell + " pixels per cell." }
            | Some _ -> { state with BackgroundAnnouncement = "Unlock the background before aligning it." }
            | None -> state
