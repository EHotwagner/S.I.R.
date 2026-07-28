namespace SIR.Conformance

open FS.GG.Game.Core
open SIR.Domain
open SIR.Simulation

type Fixture =
    { Name: string
      Expected: byte array
      Evaluate: unit -> byte array }

type Divergence =
    { FixtureName: string
      ByteOffset: int
      Expected: byte
      Actual: byte }

[<RequireQualifiedAccess>]
module NumericFixtures =
    let private required result =
        match result with
        | Ok value -> value
        | Error error -> failwithf "Fixture setup failed: %A" error

    let private bytes values = values |> Array.map byte

    let all: Fixture list =
        [ { Name = "bounded-minimum-encoding"
            Expected = bytes [| 0; 0; 0; 128 |]
            Evaluate =
                fun () ->
                    BoundedInt32.create System.Int32.MinValue System.Int32.MaxValue System.Int32.MinValue
                    |> required
                    |> CanonicalEncoding.boundedInt32 }
          { Name = "bounded-add-overflow-saturates"
            Expected = bytes [| 255; 255; 255; 127 |]
            Evaluate =
                fun () ->
                    let maximum =
                        BoundedInt32.create System.Int32.MinValue System.Int32.MaxValue System.Int32.MaxValue
                        |> required

                    let one =
                        BoundedInt32.create System.Int32.MinValue System.Int32.MaxValue 1
                        |> required

                    BoundedInt32.addSaturating maximum one
                    |> required
                    |> CanonicalEncoding.boundedInt32 }
          { Name = "bounded-subtract-underflow-saturates"
            Expected = bytes [| 0; 0; 0; 128 |]
            Evaluate =
                fun () ->
                    let minimum =
                        BoundedInt32.create System.Int32.MinValue System.Int32.MaxValue System.Int32.MinValue
                        |> required

                    let one =
                        BoundedInt32.create System.Int32.MinValue System.Int32.MaxValue 1
                        |> required

                    BoundedInt32.subtractSaturating minimum one
                    |> required
                    |> CanonicalEncoding.boundedInt32 }
          { Name = "fixed-positive-half-away-from-zero"
            Expected = bytes [| 57; 1; 0; 0 |]
            Evaluate =
                fun () ->
                    FixedPoint.fromRatio 1 32
                    |> required
                    |> CanonicalEncoding.fixedPoint }
          { Name = "fixed-negative-half-away-from-zero"
            Expected = bytes [| 199; 254; 255; 255 |]
            Evaluate =
                fun () ->
                    FixedPoint.fromRatio -1 32
                    |> required
                    |> CanonicalEncoding.fixedPoint }
          { Name = "fixed-add-and-subtract-saturate"
            Expected = bytes [| 255; 255; 255; 127; 0; 0; 0; 128 |]
            Evaluate =
                fun () ->
                    [ FixedPoint.addSaturating
                          (FixedPoint.fromRaw System.Int32.MaxValue)
                          (FixedPoint.fromRaw 1)
                      FixedPoint.subtractSaturating
                          (FixedPoint.fromRaw System.Int32.MinValue)
                          (FixedPoint.fromRaw 1) ]
                    |> List.map CanonicalEncoding.fixedPoint
                    |> CanonicalEncoding.concatenate }
          { Name = "fixed-multiply"
            Expected = bytes [| 42; 124; 255; 255 |]
            Evaluate =
                fun () ->
                    FixedPoint.multiplySaturating
                        (FixedPoint.fromRaw 15_000)
                        (FixedPoint.fromRaw -22_500)
                    |> CanonicalEncoding.fixedPoint }
          { Name = "fixed-tie-multiply"
            Expected = bytes [| 1; 0; 0; 0; 255; 255; 255; 255 |]
            Evaluate =
                fun () ->
                    [ FixedPoint.multiplySaturating (FixedPoint.fromRaw 1) (FixedPoint.fromRaw 5_000)
                      FixedPoint.multiplySaturating (FixedPoint.fromRaw -1) (FixedPoint.fromRaw 5_000) ]
                    |> List.map CanonicalEncoding.fixedPoint
                    |> CanonicalEncoding.concatenate }
          { Name = "canonical-signed-order"
            Expected = bytes [| 255; 255; 255; 255; 0; 0; 0; 0; 1; 0; 0; 0 |]
            Evaluate =
                fun () ->
                    [ FixedPoint.fromRaw 1; FixedPoint.fromRaw -1; FixedPoint.zero ]
                    |> List.sortWith FixedPoint.compareByRaw
                    |> List.map CanonicalEncoding.fixedPoint
                    |> CanonicalEncoding.concatenate }
          { Name = "published-game-core-cell"
            Expected = bytes [| 254; 255; 255; 255; 3; 0; 0; 0 |]
            Evaluate =
                fun () ->
                    let cell: Cell = { Col = -2; Row = 3 }
                    Substrate.cellBytes cell } ]

    let evaluate (injectAt: string option) : (Fixture * byte array) list =
        all
        |> List.map (fun fixture ->
            let actual = fixture.Evaluate()

            match injectAt with
            | Some name when name = fixture.Name ->
                let divergent = Array.copy actual
                divergent[0] <- divergent[0] ^^^ 1uy
                fixture, divergent
            | _ -> fixture, actual)

    let firstDivergence (evaluated: (Fixture * byte array) list) : Divergence option =
        evaluated
        |> List.tryPick (fun (fixture, actual) ->
            if fixture.Expected.Length <> actual.Length then
                Some
                    { FixtureName = fixture.Name
                      ByteOffset = min fixture.Expected.Length actual.Length
                      Expected = 0uy
                      Actual = 0uy }
            else
                fixture.Expected
                |> Array.mapi (fun index expected -> index, expected)
                |> Array.tryPick (fun (index, expected) ->
                    let actualByte = actual[index]

                    if expected = actualByte then
                        None
                    else
                        Some
                            { FixtureName = fixture.Name
                              ByteOffset = index
                              Expected = expected
                              Actual = actualByte }))

    let canonicalBytes (evaluated: (Fixture * byte array) list) =
        evaluated
        |> Seq.map (fun (_, actual) -> actual)
        |> CanonicalEncoding.concatenate

    let hex (bytes: byte array) =
        let alphabet = "0123456789abcdef"

        let characters =
            bytes
            |> Array.collect (fun value ->
                [| alphabet[int value >>> 4]; alphabet[int value &&& 15] |])

        System.String(characters)
