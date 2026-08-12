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

    let private fixture name evaluate =
        { Name = name
          Expected = evaluate ()
          Evaluate = evaluate }

    let all: Fixture list =
        [ fixture "game-core-cell-order"
              (fun () ->
                  [ cell 2 -1; cell -3 4; cell 2 -2; cell 0 0 ]
                  |> List.sort
                  |> cells)
          fixture "game-core-edge-between"
              (fun () -> Edges.edgeBetween (cell -4 9) (cell -3 9) |> edge)
          fixture "game-core-los"
              (fun () ->
                  Los.lineOfSightBy LineMode.Supercover (fun current -> current <> cell 1 0) (cell 0 0) (cell 2 1)
                  |> function
                      | true -> [| 1uy |]
                      | false -> [| 0uy |])
          fixture "game-core-astar"
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
            let actual = fixture.Evaluate ()

            match injectAt with
            | Some name when name = fixture.Name ->
                let divergent = Array.copy actual
                divergent[0] <- divergent[0] ^^^ 1uy
                fixture, divergent
            | _ -> fixture, actual)
