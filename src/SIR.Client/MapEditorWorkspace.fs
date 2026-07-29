namespace SIR.Client

open System

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
      CapturedPointers: Map<int32, EditorPointer> }

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

    let initial reducedMotion =
        { Camera =
            { PanX = CameraPadding
              PanY = CameraPadding
              Zoom = 1.0 }
          ViewportWidth = DefaultViewportWidth
          ViewportHeight = DefaultViewportHeight
          InspectorCollapsed = false
          ReducedMotion = reducedMotion
          CapturedPointers = Map.empty }

    let private finiteOr fallback value =
        if Double.IsNaN value || Double.IsInfinity value then fallback else value

    let private clamp minimum maximum value =
        max minimum (min maximum value)

    let private boundedZoom value =
        value
        |> finiteOr 1.0
        |> clamp MinimumZoom MaximumZoom

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
