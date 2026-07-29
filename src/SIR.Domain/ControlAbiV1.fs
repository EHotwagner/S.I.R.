namespace SIR.ControlAbi

open System
open System.Text

/// Whether a canonical envelope is host input or module output.
[<RequireQualifiedAccess>]
type MessageKind =
    | Input
    | Output

/// One independently bounded ABI section.
type Section =
    { Tag: uint16
      Required: bool
      ElementCount: int
      Payload: byte array }

/// Runtime-neutral representation of a Control ABI v1 invocation envelope.
type Envelope =
    { Kind: MessageKind
      MinorVersion: byte
      Tick: int
      UnitId: int
      Flags: uint32
      Budget: uint32
      Sections: Section list }

/// One output request. The request-specific payload is interpreted by the host.
type Request =
    { Kind: RequestKind
      ModuleRequestId: uint32
      Payload: byte array }

/// A fully decoded module output and its atomically validated requests.
type OutputMessage =
    { Envelope: Envelope
      Requests: Request list }

/// Stable decoder failures; diagnostics may add text without changing these cases.
[<RequireQualifiedAccess>]
type DecodeError =
    | TooShort
    | BadMagic
    | WrongMessageKind
    | UnsupportedVersion
    | InvalidTotalLength
    | LimitExceeded
    | ReservedBitsSet
    | InvalidSectionFlags
    | InvalidSectionLength
    | InvalidElementCount
    | NonCanonicalSectionOrder
    | DuplicateSectionTag
    | UnknownRequiredSection
    | MissingRequiredSection
    | InvalidUtf8
    | UnknownRequestKind
    | NonCanonicalRequestOrder
    | DuplicateRequestId
    | TrailingRequestBytes

[<RequireQualifiedAccess>]
module V1Codec =
    let private inputMagic = [| 0x53uy; 0x49uy; 0x52uy; 0x49uy |] // SIRI
    let private outputMagic = [| 0x53uy; 0x49uy; 0x52uy; 0x4fuy |] // SIRO

    let private maximumBytes kind =
        match kind with
        | MessageKind.Input -> V1Constants.InputMaximumBytes
        | MessageKind.Output -> V1Constants.OutputMaximumBytes

    let private magic kind =
        match kind with
        | MessageKind.Input -> inputMagic
        | MessageKind.Output -> outputMagic

    let private knownTag kind tag =
        match kind with
        | MessageKind.Input ->
            tag >= V1Constants.OwnStateTag
            && tag <= V1Constants.RequestStatusTag
        | MessageKind.Output -> tag = V1Constants.OutputRequestsTag

    let private writeU16 (bytes: byte array) offset (value: uint16) =
        bytes[offset] <- byte value
        bytes[offset + 1] <- byte (value >>> 8)

    let private writeU32 (bytes: byte array) offset (value: uint32) =
        bytes[offset] <- byte value
        bytes[offset + 1] <- byte (value >>> 8)
        bytes[offset + 2] <- byte (value >>> 16)
        bytes[offset + 3] <- byte (value >>> 24)

    let private readU16 (bytes: byte array) offset =
        uint16 bytes[offset] ||| (uint16 bytes[offset + 1] <<< 8)

    let private readU32 (bytes: byte array) offset =
        uint32 bytes[offset]
        ||| (uint32 bytes[offset + 1] <<< 8)
        ||| (uint32 bytes[offset + 2] <<< 16)
        ||| (uint32 bytes[offset + 3] <<< 24)

    let private checkedLength value =
        if value > uint32 Int32.MaxValue then None else Some(int value)

    let private validateSection (section: Section) =
        if section.ElementCount < 0
           || section.ElementCount > V1Constants.MaximumElementsPerSection then
            Error DecodeError.InvalidElementCount
        elif section.Payload.Length > V1Constants.InputMaximumBytes then
            Error DecodeError.InvalidSectionLength
        else
            Ok()

    /// Encodes an envelope, sorting sections by ascending tag.
    let encode (envelope: Envelope) =
        if envelope.MinorVersion > V1Constants.Minor
           || envelope.Tick < 0
           || envelope.UnitId < 0
           || envelope.Sections.Length > V1Constants.MaximumSections then
            Error DecodeError.LimitExceeded
        else
            let sections = envelope.Sections |> List.sortBy _.Tag

            let rec validate previous remaining =
                match remaining with
                | [] -> Ok()
                | section :: tail ->
                    match previous, validateSection section with
                    | Some tag, _ when tag = section.Tag ->
                        Error DecodeError.DuplicateSectionTag
                    | _, Error error -> Error error
                    | _, Ok()
                        when section.Required
                             && not (knownTag envelope.Kind section.Tag) ->
                        Error DecodeError.UnknownRequiredSection
                    | _, Ok() -> validate (Some section.Tag) tail

            match validate None sections with
            | Error error -> Error error
            | Ok() ->
                let total =
                    sections
                    |> List.fold
                        (fun size section ->
                            int64 size
                            + int64 V1Constants.SectionHeaderBytes
                            + int64 section.Payload.Length)
                        (int64 V1Constants.HeaderBytes)

                if total > int64 (maximumBytes envelope.Kind) then
                    Error DecodeError.LimitExceeded
                else
                    let bytes = Array.zeroCreate<byte> (int total)
                    Array.blit (magic envelope.Kind) 0 bytes 0 4
                    bytes[4] <- V1Constants.Major
                    bytes[5] <- envelope.MinorVersion
                    writeU16 bytes 6 (uint16 V1Constants.HeaderBytes)
                    writeU32 bytes 8 (uint32 total)
                    writeU32 bytes 12 (uint32 envelope.Tick)
                    writeU32 bytes 16 (uint32 envelope.UnitId)
                    writeU32 bytes 20 envelope.Flags
                    writeU32 bytes 24 envelope.Budget
                    writeU16 bytes 28 (uint16 envelope.Sections.Length)
                    writeU16 bytes 30 0us

                    let mutable offset = V1Constants.HeaderBytes

                    for section in sections do
                        writeU16 bytes offset section.Tag
                        writeU16
                            bytes
                            (offset + 2)
                            (if section.Required then
                                 V1Constants.RequiredSectionFlag
                             else
                                 0us)
                        writeU32 bytes (offset + 4) (uint32 section.Payload.Length)
                        writeU16 bytes (offset + 8) (uint16 section.ElementCount)
                        writeU16 bytes (offset + 10) 0us
                        Array.blit
                            section.Payload
                            0
                            bytes
                            (offset + V1Constants.SectionHeaderBytes)
                            section.Payload.Length
                        offset <-
                            offset
                            + V1Constants.SectionHeaderBytes
                            + section.Payload.Length

                    Ok bytes

    /// Decodes and fully bounds a canonical input or output envelope.
    let decode kind (bytes: byte array) =
        if bytes.Length < V1Constants.HeaderBytes then
            Error DecodeError.TooShort
        elif bytes.Length > maximumBytes kind then
            Error DecodeError.LimitExceeded
        elif bytes[0..3] <> magic kind then
            Error DecodeError.BadMagic
        elif bytes[4] <> V1Constants.Major
             || bytes[5] > V1Constants.Minor then
            Error DecodeError.UnsupportedVersion
        elif readU16 bytes 6 <> uint16 V1Constants.HeaderBytes then
            Error DecodeError.InvalidTotalLength
        elif readU32 bytes 8 <> uint32 bytes.Length then
            Error DecodeError.InvalidTotalLength
        elif readU32 bytes 12 > uint32 Int32.MaxValue
             || readU32 bytes 16 > uint32 Int32.MaxValue then
            Error DecodeError.LimitExceeded
        elif readU16 bytes 30 <> 0us then
            Error DecodeError.ReservedBitsSet
        else
            let sectionCount = int (readU16 bytes 28)

            if sectionCount > V1Constants.MaximumSections then
                Error DecodeError.LimitExceeded
            else
                let rec readSections index offset previousTag sections =
                    if index = sectionCount then
                        if offset = bytes.Length then
                            Ok(List.rev sections)
                        else
                            Error DecodeError.InvalidTotalLength
                    elif offset > bytes.Length - V1Constants.SectionHeaderBytes then
                        Error DecodeError.InvalidSectionLength
                    else
                        let tag = readU16 bytes offset
                        let flags = readU16 bytes (offset + 2)
                        let payloadLengthValue = readU32 bytes (offset + 4)
                        let elementCount = int (readU16 bytes (offset + 8))
                        let reserved = readU16 bytes (offset + 10)

                        match checkedLength payloadLengthValue with
                        | None -> Error DecodeError.InvalidSectionLength
                        | Some payloadLength ->
                            let payloadOffset =
                                offset + V1Constants.SectionHeaderBytes

                            if flags &&& ~~~V1Constants.RequiredSectionFlag <> 0us then
                                Error DecodeError.InvalidSectionFlags
                            elif reserved <> 0us then
                                Error DecodeError.ReservedBitsSet
                            elif elementCount > V1Constants.MaximumElementsPerSection then
                                Error DecodeError.InvalidElementCount
                            elif payloadLength > bytes.Length - payloadOffset then
                                Error DecodeError.InvalidSectionLength
                            elif previousTag = Some tag then
                                Error DecodeError.DuplicateSectionTag
                            elif
                                previousTag
                                |> Option.exists (fun previous -> previous > tag)
                            then
                                Error DecodeError.NonCanonicalSectionOrder
                            elif
                                flags = V1Constants.RequiredSectionFlag
                                && not (knownTag kind tag)
                            then
                                Error DecodeError.UnknownRequiredSection
                            else
                                let payload =
                                    bytes[payloadOffset .. payloadOffset + payloadLength - 1]

                                readSections
                                    (index + 1)
                                    (payloadOffset + payloadLength)
                                    (Some tag)
                                    ({ Tag = tag
                                       Required =
                                           flags = V1Constants.RequiredSectionFlag
                                       ElementCount = elementCount
                                       Payload = payload }
                                     :: sections)

                readSections 0 V1Constants.HeaderBytes None []
                |> Result.map (fun sections ->
                    { Kind = kind
                      MinorVersion = bytes[5]
                      Tick = int (readU32 bytes 12)
                      UnitId = int (readU32 bytes 16)
                      Flags = readU32 bytes 20
                      Budget = readU32 bytes 24
                      Sections = sections })

    /// Encodes a length-prefixed bounded UTF-8 field.
    let encodeString (value: string) =
        let bytes = Encoding.UTF8.GetBytes value

        if bytes.Length > V1Constants.MaximumStringBytes then
            Error DecodeError.LimitExceeded
        else
            let result = Array.zeroCreate<byte> (2 + bytes.Length)
            writeU16 result 0 (uint16 bytes.Length)
            Array.blit bytes 0 result 2 bytes.Length
            Ok result

    /// Decodes a complete length-prefixed bounded UTF-8 field.
    let decodeString (bytes: byte array) =
        if bytes.Length < 2 then
            Error DecodeError.InvalidUtf8
        else
            let length = int (readU16 bytes 0)

            if length > V1Constants.MaximumStringBytes
               || length <> bytes.Length - 2 then
                Error DecodeError.LimitExceeded
            else
                let payload = bytes[2..]
                let decoded = Encoding.UTF8.GetString payload

                if Encoding.UTF8.GetBytes decoded <> payload then
                    Error DecodeError.InvalidUtf8
                else
                    Ok decoded

    /// Encodes output request records in ascending module-request-ID order.
    let encodeRequests (requests: Request list) =
        let ordered = requests |> List.sortBy _.ModuleRequestId

        if ordered.Length > V1Constants.MaximumElementsPerSection then
            Error DecodeError.LimitExceeded
        else
            let mutable previous = None
            let mutable error = None
            let mutable total = 0

            for request in ordered do
                if previous = Some request.ModuleRequestId then
                    error <- Some DecodeError.DuplicateRequestId
                elif
                    request.Payload.Length
                    > V1Constants.MaximumOpaquePayloadBytes
                then
                    error <- Some DecodeError.LimitExceeded
                else
                    previous <- Some request.ModuleRequestId
                    total <- total + 12 + request.Payload.Length

            match error with
            | Some value -> Error value
            | None ->
                let bytes = Array.zeroCreate<byte> total
                let mutable offset = 0

                for request in ordered do
                    writeU16 bytes offset (uint16 (int request.Kind))
                    writeU16 bytes (offset + 2) 0us
                    writeU32 bytes (offset + 4) request.ModuleRequestId
                    writeU32 bytes (offset + 8) (uint32 request.Payload.Length)
                    Array.blit request.Payload 0 bytes (offset + 12) request.Payload.Length
                    offset <- offset + 12 + request.Payload.Length

                Ok bytes

    /// Decodes exactly elementCount output request records.
    let decodeRequests elementCount (bytes: byte array) =
        if elementCount < 0
           || elementCount > V1Constants.MaximumElementsPerSection then
            Error DecodeError.InvalidElementCount
        else
            let requestKind value =
                if value >= int RequestKind.SetMovementIntent
                   && value <= int RequestKind.Sleep then
                    Some(enum<RequestKind> value)
                else
                    None

            let rec loop index offset previous requests =
                if index = elementCount then
                    if offset = bytes.Length then
                        Ok(List.rev requests)
                    else
                        Error DecodeError.TrailingRequestBytes
                elif offset > bytes.Length - 12 then
                    Error DecodeError.InvalidSectionLength
                elif readU16 bytes (offset + 2) <> 0us then
                    Error DecodeError.ReservedBitsSet
                else
                    let requestId = readU32 bytes (offset + 4)

                    match
                        requestKind (int (readU16 bytes offset)),
                        checkedLength (readU32 bytes (offset + 8))
                    with
                    | None, _ -> Error DecodeError.UnknownRequestKind
                    | _, None -> Error DecodeError.InvalidSectionLength
                    | Some kind, Some payloadLength ->
                        let payloadOffset = offset + 12

                        if payloadLength > V1Constants.MaximumOpaquePayloadBytes then
                            Error DecodeError.LimitExceeded
                        elif payloadLength > bytes.Length - payloadOffset then
                            Error DecodeError.InvalidSectionLength
                        elif previous = Some requestId then
                            Error DecodeError.DuplicateRequestId
                        elif
                            previous
                            |> Option.exists (fun previousId -> previousId > requestId)
                        then
                            Error DecodeError.NonCanonicalRequestOrder
                        else
                            loop
                                (index + 1)
                                (payloadOffset + payloadLength)
                                (Some requestId)
                                ({ Kind = kind
                                   ModuleRequestId = requestId
                                   Payload =
                                       bytes[payloadOffset .. payloadOffset + payloadLength - 1] }
                                 :: requests)

            loop 0 0 None []

    /// Encodes the one required output-request section and any future optional sections.
    let encodeOutput
        tick
        unitId
        flags
        budget
        requests
        optionalSections
        =
        match encodeRequests requests with
        | Error error -> Error error
        | Ok payload ->
            { Kind = MessageKind.Output
              MinorVersion = V1Constants.Minor
              Tick = tick
              UnitId = unitId
              Flags = flags
              Budget = budget
              Sections =
                { Tag = V1Constants.OutputRequestsTag
                  Required = true
                  ElementCount = requests.Length
                  Payload = payload }
                :: optionalSections }
            |> encode

    /// Decodes a complete output atomically, including every request record.
    let decodeOutput bytes =
        match decode MessageKind.Output bytes with
        | Error error -> Error error
        | Ok envelope ->
            match
                envelope.Sections
                |> List.tryFind (fun section ->
                    section.Tag = V1Constants.OutputRequestsTag)
            with
            | None -> Error DecodeError.MissingRequiredSection
            | Some section when not section.Required ->
                Error DecodeError.MissingRequiredSection
            | Some section ->
                decodeRequests section.ElementCount section.Payload
                |> Result.map (fun requests ->
                    { Envelope = envelope
                      Requests = requests })

    /// Encodes a host input envelope and rejects a mismatched message kind.
    let encodeInput (envelope: Envelope) =
        if envelope.Kind <> MessageKind.Input then
            Error DecodeError.WrongMessageKind
        else
            encode envelope

    /// Decodes a canonical host input envelope.
    let decodeInput bytes = decode MessageKind.Input bytes
