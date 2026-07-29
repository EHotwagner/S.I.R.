namespace SIR.Conformance

open SIR.Domain

[<RequireQualifiedAccess>]
module OrientationFixtures =
    let private require condition message =
        if not condition then failwith message

    let private decodeDirection code =
        Direction8.tryFromCode code
        |> Option.defaultWith (fun () -> failwith "Canonical direction code did not decode.")

    let evaluate () =
        require
            (Direction8.tryFromCode 8uy = None
             && Direction8.tryFromCode 255uy = None)
            "Out-of-range direction codes were accepted."

        let combinations =
            [| for body in Direction8.all do
                   for attention in Direction8.all do
                       let orientation =
                           { MovementDirection = None
                             BodyFacing = body
                             AttentionDirection = attention }

                       let encoded =
                           CanonicalEncoding.resolvedOrientation orientation

                       require
                           (encoded.Length = 3
                            && encoded[0] = 0uy
                            && decodeDirection encoded[1] = body
                            && decodeDirection encoded[2] = attention)
                           "A body/attention combination did not round-trip."

                       let relative =
                           Direction8.relativeToBody body attention

                       yield
                           CanonicalEncoding.concatenate
                               [ encoded
                                 CanonicalEncoding.direction8 relative ] |]

        let movement =
            Direction8.all
            |> Array.map (fun direction ->
                let column, row = Direction8.delta direction
                let resolved = Direction8.tryFromDelta column row

                require
                    (resolved = Some direction)
                    "A movement segment did not resolve to its canonical direction."

                let orientation =
                    { MovementDirection = resolved
                      BodyFacing = North
                      AttentionDirection = direction }

                let encoded =
                    CanonicalEncoding.resolvedOrientation orientation

                require
                    (encoded.Length = 4
                     && encoded[0] = 1uy
                     && decodeDirection encoded[1] = direction)
                    "A movement-relative combination did not round-trip."

                encoded)

        CanonicalEncoding.concatenate
            [ CanonicalEncoding.concatenate combinations
              CanonicalEncoding.concatenate movement ]
