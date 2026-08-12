namespace SIR.Conformance

open FS.GG.Game.Core
open SIR.Domain

[<RequireQualifiedAccess>]
module GameCoreFixtures =
    let private cell col row: Cell = { Col = col; Row = row }

    let private int32 value =
        [| byte value; byte (value >>> 8); byte (value >>> 16); byte (value >>> 24) |]

    let private cells values =
        values
        |> List.collect (fun value -> [ value.Col; value.Row ])
        |> List.collect (int32 >> Array.toList)
        |> List.toArray

    let private optionCells value =
        match value with
        | None -> [| 0uy |]
        | Some path -> Array.append [| 1uy |] (cells path)

    let private edge value =
        match value with
        | None -> [| 0uy |]
        | Some item -> Array.append [| 1uy |] (cells [ item.Lo; item.Hi ])

    let private fixture name expected evaluate =
        { Name = name
          Expected = expected
          Evaluate = evaluate }

    let all: Fixture list =
        [ fixture "game-core-cell-order"
              (Array.concat [ int32 -3; int32 4; int32 0; int32 0; int32 2; int32 -2; int32 2; int32 -1 ])
              (fun () ->
                  [ cell 2 -1; cell -3 4; cell 2 -2; cell 0 0 ]
                  |> List.sort
                  |> cells)
          fixture "game-core-edge-between"
              [| 1uy; 252uy; 255uy; 255uy; 255uy; 9uy; 0uy; 0uy; 0uy; 253uy; 255uy; 255uy; 255uy; 9uy; 0uy; 0uy; 0uy |]
              (fun () -> Edges.edgeBetween (cell -4 9) (cell -3 9) |> edge)
          fixture "game-core-los"
              [| 0uy |]
              (fun () ->
                  Los.lineOfSightBy LineMode.Supercover (fun current -> current <> cell 1 0) (cell 0 0) (cell 2 1)
                  |> function
                      | true -> [| 1uy |]
                      | false -> [| 0uy |])
          fixture "game-core-astar"
              (Array.concat [ [| 1uy |]; int32 0; int32 0; int32 0; int32 1; int32 1; int32 1; int32 2; int32 1; int32 2; int32 0 ])
              (fun () ->
                  let blocked = Set.ofList [ cell 1 0 ]
                  let walkable current =
                      current.Col >= 0
                      && current.Col <= 2
                      && current.Row >= 0
                      && current.Row <= 1
                      && not (Set.contains current blocked)

                  Pathfinding.astar Neighbourhood.FourWay 16 walkable (cell 0 0) (cell 2 0)
                  |> optionCells) ]

    let evaluate injectAt =
        all
        |> List.map (fun fixture ->
            let actual =
                match fixture.Name, injectAt with
                | "game-core-cell-order", Some "game-core-cell-order" ->
                    [ cell 3 -1; cell -3 4; cell 2 -2; cell 0 0 ] |> List.sort |> cells
                | "game-core-edge-between", Some "game-core-edge-between" ->
                    Edges.edgeBetween (cell -4 9) (cell -2 9) |> edge
                | "game-core-los", Some "game-core-los" ->
                    Los.lineOfSightBy LineMode.Supercover (fun _ -> true) (cell 0 0) (cell 2 1)
                    |> function | true -> [| 1uy |] | false -> [| 0uy |]
                | "game-core-astar", Some "game-core-astar" ->
                    let walkable current = current.Col >= 0 && current.Col <= 2 && current.Row >= 0 && current.Row <= 1
                    Pathfinding.astar Neighbourhood.FourWay 16 walkable (cell 0 0) (cell 2 0) |> optionCells
                | _ -> fixture.Evaluate ()

            match injectAt with
            | _ -> fixture, actual)
