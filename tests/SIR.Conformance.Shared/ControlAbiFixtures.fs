namespace SIR.Conformance

open SIR.ControlAbi

[<RequireQualifiedAccess>]
module ControlAbiFixtures =
    let private require condition message =
        if not condition then failwith message

    let private isError expected result =
        match result with
        | Error actual -> actual = expected
        | Ok _ -> false

    let private decodeHex (value: string) =
        [| for offset in 0 .. 2 .. value.Length - 2 ->
               System.Convert.ToByte(value.Substring(offset, 2), 16) |]

    let private frozenOutputHex =
        "5349524f01002000490000002a0000000700000000000000e803000001000000"
        + "011001001d00000002000000"
        + "03000000070000000100000002"
        + "0c000000090000000400000064000000"

    let outputRequests =
        [ { Kind = RequestKind.Sleep
            ModuleRequestId = 9u
            Payload = [| 100uy; 0uy; 0uy; 0uy |] }
          { Kind = RequestKind.SetAttention
            ModuleRequestId = 7u
            Payload = [| 2uy |] } ]

    let frozenOutputBytes () =
        V1Codec.encodeOutput 42 7 0u 1000u outputRequests []
        |> Result.defaultWith (fun error ->
            failwithf "Could not encode ABI envelope: %A" error)

    let private replaceU32 offset value (bytes: byte array) =
        let result = Array.copy bytes
        result[offset] <- byte value
        result[offset + 1] <- byte (value >>> 8)
        result[offset + 2] <- byte (value >>> 16)
        result[offset + 3] <- byte (value >>> 24)
        result

    let private replaceU16 offset value (bytes: byte array) =
        let result = Array.copy bytes
        result[offset] <- byte value
        result[offset + 1] <- byte (value >>> 8)
        result

    let private decoderVectors () =
        let frozen = frozenOutputBytes ()

        require
            (frozen = decodeHex frozenOutputHex)
            "Control ABI v1 frozen output bytes changed."

        let decoded =
            V1Codec.decodeOutput frozen
            |> Result.defaultWith (fun error ->
                failwithf "Frozen ABI output did not decode: %A" error)

        require
            (decoded.Requests |> List.map _.ModuleRequestId = [ 7u; 9u ])
            "ABI request encoder did not establish canonical ID order."

        let malformedLength = frozen |> replaceU32 8 (uint32 (frozen.Length + 1))

        require
            (V1Codec.decode MessageKind.Output malformedLength
             |> isError DecodeError.InvalidTotalLength)
            "Malformed ABI total length was accepted."

        let unknownOptional =
            { Kind = MessageKind.Input
              MinorVersion = 0uy
              Tick = 1
              UnitId = 2
              Flags = 0u
              Budget = 0u
              Sections =
                [ { Tag = 0x7000us
                    Required = false
                    ElementCount = 0
                    Payload = [| 1uy |] } ] }
            |> V1Codec.encode
            |> Result.defaultWith (fun error -> failwithf "%A" error)

        require
            (V1Codec.decode MessageKind.Input unknownOptional |> Result.isOk)
            "Unknown optional ABI section was not skippable."

        let unknownRequired =
            unknownOptional
            |> replaceU16 (V1Constants.HeaderBytes + 2) 1us

        require
            (V1Codec.decode MessageKind.Input unknownRequired
             |> isError DecodeError.UnknownRequiredSection)
            "Unknown required ABI section was accepted."

        let canonicalInput =
            { Kind = MessageKind.Input
              MinorVersion = 0uy
              Tick = 1
              UnitId = 2
              Flags = 0u
              Budget = 50u
              Sections =
                [ { Tag = V1Constants.ResolvedOrientationTag
                    Required = true
                    ElementCount = 1
                    Payload = [| 1uy; 2uy; 3uy |] }
                  { Tag = V1Constants.OwnStateTag
                    Required = true
                    ElementCount = 1
                    Payload = [| 4uy |] } ] }
            |> V1Codec.encode
            |> Result.defaultWith (fun error -> failwithf "%A" error)

        require
            (canonicalInput[V1Constants.HeaderBytes] = 1uy)
            "ABI encoder did not sort sections by ascending tag."

        let firstLength = 1
        let firstOffset = V1Constants.HeaderBytes
        let secondOffset =
            firstOffset + V1Constants.SectionHeaderBytes + firstLength

        let nonCanonical = Array.copy canonicalInput
        nonCanonical[firstOffset] <- 2uy
        nonCanonical[secondOffset] <- 1uy

        require
            (V1Codec.decode MessageKind.Input nonCanonical
             |> isError DecodeError.NonCanonicalSectionOrder)
            "Non-canonical ABI section order was accepted."

        let maximumPayload =
            Array.zeroCreate<byte>
                (V1Constants.InputMaximumBytes
                 - V1Constants.HeaderBytes
                 - V1Constants.SectionHeaderBytes)

        let maximum =
            { Kind = MessageKind.Input
              MinorVersion = 0uy
              Tick = 0
              UnitId = 0
              Flags = 0u
              Budget = 0u
              Sections =
                [ { Tag = 0x7000us
                    Required = false
                    ElementCount = 0
                    Payload = maximumPayload } ] }
            |> V1Codec.encode
            |> Result.defaultWith (fun error ->
                failwithf "Maximum ABI input failed: %A" error)

        require
            (maximum.Length = V1Constants.InputMaximumBytes
             && (V1Codec.decode MessageKind.Input maximum |> Result.isOk))
            "Maximum-size ABI input did not round-trip."

        let oversized =
            { Kind = MessageKind.Input
              MinorVersion = 0uy
              Tick = 0
              UnitId = 0
              Flags = 0u
              Budget = 0u
              Sections =
                [ { Tag = 0x7000us
                    Required = false
                    ElementCount = 0
                    Payload = Array.append maximumPayload [| 0uy |] } ] }

        require
            (V1Codec.encode oversized = Error DecodeError.LimitExceeded)
            "Oversized ABI input escaped its declared bound."

        require
            (V1Codec.decodeString [| 1uy; 0uy; 0xffuy |]
             |> isError DecodeError.InvalidUtf8)
            "Malformed UTF-8 was accepted."

    let private boundedMutationProperty () =
        let frozen = frozenOutputBytes ()
        let mutable state = 0x12345678u

        let next () =
            state <- state * 1664525u + 1013904223u
            state

        for _ in 1 .. 4096 do
            let length = int (next () % uint32 (frozen.Length + 25))
            let candidate = Array.zeroCreate<byte> length

            for index in 0 .. length - 1 do
                candidate[index] <- byte (next () >>> 24)

            try
                match V1Codec.decode MessageKind.Output candidate with
                | Ok envelope ->
                    require
                        (candidate.Length <= V1Constants.OutputMaximumBytes
                         && envelope.Sections.Length
                            <= V1Constants.MaximumSections
                         && envelope.Sections
                            |> List.forall (fun section ->
                                section.ElementCount
                                <= V1Constants.MaximumElementsPerSection))
                        "Decoder produced a value beyond an ABI bound."
                | Error _ -> ()
            with error ->
                failwithf
                    "ABI decoder escaped on bounded mutation %d: %s"
                    length
                    error.Message

    let evaluate () =
        decoderVectors ()
        boundedMutationProperty ()
        frozenOutputBytes ()
