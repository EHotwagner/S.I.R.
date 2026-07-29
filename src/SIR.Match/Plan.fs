namespace SIR.Match

open System
open System.IO
open System.Reflection
open System.Text
open FS.GG.Game.Core
open SIR.ControlAbi
open SIR.Domain
open SIR.Simulation
open Wasmtime

type PlanMovementPosture =
    | Balanced
    | Speed
    | Readiness

type PlanTransition =
    | Continue
    | HoldPosition
    | JumpTo of commandId: Guid
    | AbortUnitPlan

type PlanInterruptionPolicy =
    | ApplyFallback
    | Retry
    | IgnoreInterruption

type PlanSynchronizationMode =
    | PreloadedClock of releaseTick: int32
    | Acknowledged of participantUnitIds: int32 array

type PlanSynchronization =
    { MarkerId: string
      Mode: PlanSynchronizationMode
      DeadlineTick: int32
      Timeout: PlanTransition }

type PlannedCommandKind =
    | MovePath of Cell array * PlanMovementPosture
    | SetFacingIntent of FacingIntent
    | SetAttentionIntent of AttentionIntent
    | SetStance of stanceId: string
    | EngageUnit of unitId: int32 * capabilityId: string
    | EngageArea of AreaReferent * capabilityId: string
    | Hold
    | Synchronize of PlanSynchronization

type PlannedCommand =
    { CommandId: Guid
      EarliestStartTick: int32
      Predecessors: Guid array
      InterruptionPolicy: PlanInterruptionPolicy
      Fallback: PlanTransition
      Kind: PlannedCommandKind
      Annotation: string }

type UnitPlan =
    { UnitId: int32
      ControllerArtifact: byte array
      Commands: PlannedCommand array
      Fallback: PlanTransition }

type PlanDocument =
    { FormatVersion: int32
      PlanId: Guid
      Revision: int64
      ParentDigest: byte array option
      MapRevisionDigest: byte array
      RulesetIdentity: string
      StartTick: int32
      HorizonTicks: int32
      UnitPlans: UnitPlan array }

[<RequireQualifiedAccess>]
type PlanDiagnosticLayer =
    | Structural
    | Map
    | Ruleset
    | Controller
    | Schedule

type PlanDiagnostic =
    { Code: string
      Layer: PlanDiagnosticLayer
      UnitId: int32 option
      CommandId: Guid option
      Fields: (string * string) list }

type ScheduledCommand =
    { UnitId: int32
      Command: PlannedCommand
      StartTick: int32
      FinishTick: int32 }

type CompiledUnitPlan =
    { UnitId: int32
      Configuration: byte array
      Schedule: ScheduledCommand array }

type CompiledPlan =
    { SemanticDigest: byte array
      SourceDigest: byte array
      Units: CompiledUnitPlan array }

type PlanValidationContext =
    { Map: MapScaleState
      RulesetIdentity: string
      MapRevisionDigest: byte array
      MaximumConfigurationBytes: int }

[<RequireQualifiedAccess>]
module SirPlan =
    [<Literal>]
    let FormatVersion = 1

    [<Literal>]
    let MaximumDocumentBytes = 262_144

    [<Literal>]
    let MaximumUnits = 256

    [<Literal>]
    let MaximumCommandsPerUnit = 256

    [<Literal>]
    let MaximumPathPoints = 256

    [<Literal>]
    let MaximumDependencies = 32

    [<Literal>]
    let MaximumHorizonTicks = 6_000

    [<Literal>]
    let MaximumIssues = 512

    let private strictUtf8 = UTF8Encoding(false, true)
    let private invariant = Globalization.CultureInfo.InvariantCulture
    let private i32 (value: int32) = value.ToString(invariant)
    let private i64 (value: int64) = value.ToString(invariant)
    let private guid (value: Guid) = value.ToString("N")

    let private hex (bytes: byte array) =
        Convert.ToHexString(bytes).ToLowerInvariant()

    let private tryHex (text: string) =
        try Some(Convert.FromHexString text) with :? FormatException -> None

    let private textHex (value: string) = value |> strictUtf8.GetBytes |> hex

    let private tryTextHex (value: string) =
        tryHex value
        |> Option.bind (fun bytes ->
            try Some(strictUtf8.GetString bytes)
            with :? DecoderFallbackException -> None)

    let private transition value =
        match value with
        | Continue -> "continue"
        | HoldPosition -> "hold"
        | JumpTo commandId -> "jump:" + guid commandId
        | AbortUnitPlan -> "abort"

    let private tryTransition value =
        match value with
        | "continue" -> Some Continue
        | "hold" -> Some HoldPosition
        | "abort" -> Some AbortUnitPlan
        | value when value.StartsWith("jump:", StringComparison.Ordinal) ->
            match Guid.TryParseExact(value.Substring 5, "N") with
            | true, commandId -> Some(JumpTo commandId)
            | _ -> None
        | _ -> None

    let private interruption value =
        match value with
        | ApplyFallback -> "fallback"
        | Retry -> "retry"
        | IgnoreInterruption -> "ignore"

    let private tryInterruption value =
        match value with
        | "fallback" -> Some ApplyFallback
        | "retry" -> Some Retry
        | "ignore" -> Some IgnoreInterruption
        | _ -> None

    let private writeI32 (bytes: ResizeArray<byte>) value =
        bytes.Add(byte value)
        bytes.Add(byte (value >>> 8))
        bytes.Add(byte (value >>> 16))
        bytes.Add(byte (value >>> 24))

    let private writeGuid (bytes: ResizeArray<byte>) (value: Guid) =
        bytes.AddRange(value.ToByteArray())

    let private writeString (bytes: ResizeArray<byte>) (value: string) =
        let encoded = strictUtf8.GetBytes value
        writeI32 bytes encoded.Length
        bytes.AddRange encoded

    let private kindBytes kind =
        let bytes = ResizeArray<byte>()
        let direction value = bytes.Add(Direction8.toCode value)
        let unitId value = writeI32 bytes (UnitId.value value)

        match kind with
        | MovePath(path, posture) ->
            bytes.Add 1uy
            bytes.Add(
                match posture with
                | Balanced -> 0uy
                | Speed -> 1uy
                | Readiness -> 2uy)
            writeI32 bytes path.Length
            path
            |> Array.iter (fun cell ->
                writeI32 bytes cell.Col
                writeI32 bytes cell.Row)
        | SetFacingIntent intent ->
            bytes.Add 2uy
            match intent with
            | KeepFacing -> bytes.Add 0uy
            | FaceFixed value ->
                bytes.Add 1uy
                direction value
            | FaceAlongMovement -> bytes.Add 2uy
            | FaceKnownUnit value ->
                bytes.Add 3uy
                unitId value
        | SetAttentionIntent intent ->
            bytes.Add 3uy
            match intent with
            | KeepAttention -> bytes.Add 0uy
            | AttendFixed value ->
                bytes.Add 1uy
                direction value
            | AttendRelativeToBody value ->
                bytes.Add 2uy
                direction value
            | AttendAlongMovement -> bytes.Add 3uy
            | AttendKnownUnit value ->
                bytes.Add 4uy
                unitId value
            | AttendKnownArea(AreaReferent value) ->
                bytes.Add 5uy
                writeI32 bytes value
        | SetStance value ->
            bytes.Add 4uy
            writeString bytes value
        | EngageUnit(value, capability) ->
            bytes.Add 5uy
            writeI32 bytes value
            writeString bytes capability
        | EngageArea(AreaReferent value, capability) ->
            bytes.Add 6uy
            writeI32 bytes value
            writeString bytes capability
        | Hold -> bytes.Add 7uy
        | Synchronize synchronization ->
            bytes.Add 8uy
            writeString bytes synchronization.MarkerId
            match synchronization.Mode with
            | PreloadedClock release ->
                bytes.Add 0uy
                writeI32 bytes release
            | Acknowledged participants ->
                bytes.Add 1uy
                writeI32 bytes participants.Length
                participants |> Array.sort |> Array.iter (writeI32 bytes)
            writeI32 bytes synchronization.DeadlineTick
            writeString bytes (transition synchronization.Timeout)
        bytes.ToArray()

    type private ByteReader(bytes: byte array) =
        let mutable offset = 0
        member _.Remaining = bytes.Length - offset
        member _.Byte() =
            if offset >= bytes.Length then raise (InvalidDataException())
            let value = bytes[offset]
            offset <- offset + 1
            value
        member _.I32() =
            if offset > bytes.Length - 4 then raise (InvalidDataException())
            let value =
                int32 bytes[offset]
                ||| (int32 bytes[offset + 1] <<< 8)
                ||| (int32 bytes[offset + 2] <<< 16)
                ||| (int32 bytes[offset + 3] <<< 24)
            offset <- offset + 4
            value
        member this.Guid() =
            if offset > bytes.Length - 16 then raise (InvalidDataException())
            let value = Guid(bytes[offset .. offset + 15])
            offset <- offset + 16
            value
        member this.String() =
            let length = this.I32()
            if length < 0 || length > 255 || offset > bytes.Length - length then
                raise (InvalidDataException())
            let value = strictUtf8.GetString(bytes, offset, length)
            offset <- offset + length
            value

    let private tryKind bytes =
        try
            let reader = ByteReader bytes
            let direction () =
                reader.Byte()
                |> Direction8.tryFromCode
                |> Option.defaultWith (fun () -> raise (InvalidDataException()))
            let result =
                match reader.Byte() with
                | 1uy ->
                    let posture =
                        match reader.Byte() with
                        | 0uy -> Balanced
                        | 1uy -> Speed
                        | 2uy -> Readiness
                        | _ -> raise (InvalidDataException())
                    let count = reader.I32()
                    if count < 0 || count > MaximumPathPoints then raise (InvalidDataException())
                    MovePath(
                        Array.init count (fun _ ->
                            { Col = reader.I32(); Row = reader.I32() }),
                        posture)
                | 2uy ->
                    SetFacingIntent(
                        match reader.Byte() with
                        | 0uy -> KeepFacing
                        | 1uy -> FaceFixed(direction ())
                        | 2uy -> FaceAlongMovement
                        | 3uy -> FaceKnownUnit(UnitId.create (reader.I32()))
                        | _ -> raise (InvalidDataException()))
                | 3uy ->
                    SetAttentionIntent(
                        match reader.Byte() with
                        | 0uy -> KeepAttention
                        | 1uy -> AttendFixed(direction ())
                        | 2uy -> AttendRelativeToBody(direction ())
                        | 3uy -> AttendAlongMovement
                        | 4uy -> AttendKnownUnit(UnitId.create (reader.I32()))
                        | 5uy -> AttendKnownArea(AreaReferent(reader.I32()))
                        | _ -> raise (InvalidDataException()))
                | 4uy -> SetStance(reader.String())
                | 5uy -> EngageUnit(reader.I32(), reader.String())
                | 6uy -> EngageArea(AreaReferent(reader.I32()), reader.String())
                | 7uy -> Hold
                | 8uy ->
                    let marker = reader.String()
                    let mode =
                        match reader.Byte() with
                        | 0uy -> PreloadedClock(reader.I32())
                        | 1uy ->
                            let count = reader.I32()
                            if count < 0 || count > MaximumUnits then raise (InvalidDataException())
                            Acknowledged(Array.init count (fun _ -> reader.I32()))
                        | _ -> raise (InvalidDataException())
                    let deadline = reader.I32()
                    let timeout =
                        reader.String()
                        |> tryTransition
                        |> Option.defaultWith (fun () -> raise (InvalidDataException()))
                    Synchronize
                        { MarkerId = marker
                          Mode = mode
                          DeadlineTick = deadline
                          Timeout = timeout }
                | _ -> raise (InvalidDataException())
            if reader.Remaining <> 0 then None else Some result
        with
        | :? InvalidDataException
        | :? DecoderFallbackException -> None

    let private encodeCore includeAnnotations document =
        let lines = ResizeArray<string>()
        lines.Add "SIR-PLAN 1"
        lines.Add(
            String.concat
                "|"
                [ "plan"
                  guid document.PlanId
                  i64 document.Revision
                  document.ParentDigest |> Option.map hex |> Option.defaultValue "-"
                  hex document.MapRevisionDigest
                  textHex document.RulesetIdentity
                  i32 document.StartTick
                  i32 document.HorizonTicks ])

        document.UnitPlans
        |> Array.sortBy _.UnitId
        |> Array.iter (fun unit ->
            lines.Add(
                String.concat
                    "|"
                    [ "unit"; i32 unit.UnitId; hex unit.ControllerArtifact
                      transition unit.Fallback ])

            unit.Commands
            |> Array.sortBy _.CommandId
            |> Array.iter (fun command ->
                lines.Add(
                    String.concat
                        "|"
                        [ "command"
                          i32 unit.UnitId
                          guid command.CommandId
                          i32 command.EarliestStartTick
                          command.Predecessors
                          |> Array.sort
                          |> Array.map guid
                          |> String.concat ","
                          interruption command.InterruptionPolicy
                          transition command.Fallback
                          command.Kind |> kindBytes |> hex
                          if includeAnnotations then textHex command.Annotation else "" ])))

        String.concat "\n" lines + "\n" |> strictUtf8.GetBytes

    let encode document =
        let bytes = encodeCore true document
        if bytes.Length > MaximumDocumentBytes then
            Error "SIR.PLAN.STRUCTURAL.DOCUMENT_TOO_LARGE"
        else
            Ok bytes

    let semanticDigest document =
        encodeCore false document |> CanonicalHash.sha256

    let sourceDigest document =
        encodeCore true document |> CanonicalHash.sha256

    let decode (bytes: byte array) =
        let fail code = Error code
        if bytes.Length > MaximumDocumentBytes then
            fail "SIR.PLAN.STRUCTURAL.DOCUMENT_TOO_LARGE"
        else
            try
                let text = strictUtf8.GetString bytes
                if text.Contains("\r", StringComparison.Ordinal)
                   || not (text.EndsWith("\n", StringComparison.Ordinal)) then
                    fail "SIR.PLAN.STRUCTURAL.NON_CANONICAL_TEXT"
                else
                    let lines = text.Split('\n', StringSplitOptions.None)
                    if lines.Length < 3 || lines[0] <> "SIR-PLAN 1" then
                        fail "SIR.PLAN.STRUCTURAL.BAD_HEADER"
                    else
                        let metadata = lines[1].Split '|'
                        if metadata.Length <> 8 || metadata[0] <> "plan" then
                            fail "SIR.PLAN.STRUCTURAL.BAD_METADATA"
                        else
                            match
                                Guid.TryParseExact(metadata[1], "N"),
                                Int64.TryParse(metadata[2], Globalization.NumberStyles.None, invariant),
                                (if metadata[3] = "-" then Some None else tryHex metadata[3] |> Option.map Some),
                                tryHex metadata[4],
                                tryTextHex metadata[5],
                                Int32.TryParse(metadata[6], Globalization.NumberStyles.AllowLeadingSign, invariant),
                                Int32.TryParse(metadata[7], Globalization.NumberStyles.AllowLeadingSign, invariant)
                            with
                            | (true, planId), (true, revision), Some parent, Some mapDigest,
                              Some ruleset, (true, startTick), (true, horizon) ->
                                let units = Collections.Generic.Dictionary<int32, byte array * PlanTransition * ResizeArray<PlannedCommand>>()
                                let mutable valid = true
                                for line in lines[2 .. lines.Length - 2] do
                                    let fields = line.Split '|'
                                    if fields.Length = 4 && fields[0] = "unit" then
                                        match Int32.TryParse fields[1], tryHex fields[2], tryTransition fields[3] with
                                        | (true, unitId), Some artifact, Some fallback when not (units.ContainsKey unitId) ->
                                            units.Add(unitId, (artifact, fallback, ResizeArray()))
                                        | _ -> valid <- false
                                    elif fields.Length = 9 && fields[0] = "command" then
                                        match
                                            Int32.TryParse fields[1],
                                            Guid.TryParseExact(fields[2], "N"),
                                            Int32.TryParse fields[3],
                                            tryInterruption fields[5],
                                            tryTransition fields[6],
                                            tryHex fields[7] |> Option.bind tryKind,
                                            tryTextHex fields[8]
                                        with
                                        | (true, unitId), (true, commandId), (true, earliest),
                                          Some policy, Some fallback, Some kind, Some annotation
                                            when units.ContainsKey unitId ->
                                            let predecessors =
                                                if fields[4] = "" then Some [||]
                                                else
                                                    fields[4].Split ','
                                                    |> Array.map (fun value ->
                                                        match Guid.TryParseExact(value, "N") with
                                                        | true, parsed -> Some parsed
                                                        | _ -> None)
                                                    |> fun values ->
                                                        if values |> Array.forall Option.isSome then
                                                            Some(values |> Array.map Option.get)
                                                        else None
                                            match predecessors with
                                            | Some dependencies ->
                                                let _, _, commands = units[unitId]
                                                commands.Add
                                                    { CommandId = commandId
                                                      EarliestStartTick = earliest
                                                      Predecessors = dependencies
                                                      InterruptionPolicy = policy
                                                      Fallback = fallback
                                                      Kind = kind
                                                      Annotation = annotation }
                                            | None -> valid <- false
                                        | _ -> valid <- false
                                    else
                                        valid <- false
                                if not valid then fail "SIR.PLAN.STRUCTURAL.BAD_LINE"
                                else
                                    let document =
                                        { FormatVersion = FormatVersion
                                          PlanId = planId
                                          Revision = revision
                                          ParentDigest = parent
                                          MapRevisionDigest = mapDigest
                                          RulesetIdentity = ruleset
                                          StartTick = startTick
                                          HorizonTicks = horizon
                                          UnitPlans =
                                            units
                                            |> Seq.map (fun pair ->
                                                let artifact, fallback, commands = pair.Value
                                                { UnitId = pair.Key
                                                  ControllerArtifact = artifact
                                                  Commands = commands.ToArray()
                                                  Fallback = fallback })
                                            |> Seq.sortBy _.UnitId
                                            |> Seq.toArray }
                                    if encodeCore true document = bytes then Ok document
                                    else fail "SIR.PLAN.STRUCTURAL.NON_CANONICAL_TEXT"
                            | _ -> fail "SIR.PLAN.STRUCTURAL.BAD_METADATA"
            with
            | :? DecoderFallbackException -> fail "SIR.PLAN.STRUCTURAL.INVALID_UTF8"

    let private issue layer code unitId commandId fields =
        { Code = code
          Layer = layer
          UnitId = unitId
          CommandId = commandId
          Fields = fields |> List.sortBy fst }

    let private transitionTarget transition =
        match transition with
        | JumpTo target -> Some target
        | _ -> None

    let validate (context: PlanValidationContext) document =
        let issues = ResizeArray<PlanDiagnostic>()
        let add value = if issues.Count < MaximumIssues then issues.Add value
        if document.FormatVersion <> FormatVersion then
            add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.VERSION" None None [])
        if document.Revision < 0L then
            add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.REVISION" None None [])
        if document.StartTick < 0
           || document.HorizonTicks <= 0
           || document.HorizonTicks > MaximumHorizonTicks then
            add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.HORIZON" None None [])
        if document.UnitPlans.Length > MaximumUnits then
            add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.UNIT_LIMIT" None None [])
        if document.MapRevisionDigest.Length <> 32
           || document.ParentDigest
              |> Option.exists (fun digest -> digest.Length <> 32) then
            add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.DIGEST_LENGTH" None None [])
        if String.IsNullOrWhiteSpace document.RulesetIdentity
           || strictUtf8.GetByteCount document.RulesetIdentity > 255 then
            add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.RULESET_IDENTITY" None None [])
        if document.MapRevisionDigest <> context.MapRevisionDigest then
            add (issue PlanDiagnosticLayer.Map "SIR.PLAN.MAP.REVISION_MISMATCH" None None [])
        if document.RulesetIdentity <> context.RulesetIdentity then
            add (issue PlanDiagnosticLayer.Ruleset "SIR.PLAN.RULESET.IDENTITY_MISMATCH" None None [])

        let duplicateUnits =
            document.UnitPlans |> Array.countBy _.UnitId |> Array.filter (snd >> ((<) 1))
        duplicateUnits
        |> Array.iter (fun (unitId, _) ->
            add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.DUPLICATE_UNIT" (Some unitId) None []))

        for unit in document.UnitPlans |> Array.sortBy _.UnitId do
            if unit.UnitId < 0 || not (Map.containsKey unit.UnitId context.Map.Units) then
                add (issue PlanDiagnosticLayer.Map "SIR.PLAN.MAP.UNKNOWN_UNIT" (Some unit.UnitId) None [])
            if unit.Commands.Length > MaximumCommandsPerUnit then
                add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.COMMAND_LIMIT" (Some unit.UnitId) None [])
            if unit.ControllerArtifact.Length = 0 then
                add (issue PlanDiagnosticLayer.Controller "SIR.PLAN.CONTROLLER.MISSING_ARTIFACT" (Some unit.UnitId) None [])
            elif unit.ControllerArtifact.Length < 8
                 || unit.ControllerArtifact[0..3] <> [| 0uy; 0x61uy; 0x73uy; 0x6duy |] then
                add (issue PlanDiagnosticLayer.Controller "SIR.PLAN.CONTROLLER.INVALID_ARTIFACT" (Some unit.UnitId) None [])
            let ids = unit.Commands |> Array.map _.CommandId |> Set.ofArray
            unit.Commands
            |> Array.countBy _.CommandId
            |> Array.filter (snd >> ((<) 1))
            |> Array.iter (fun (commandId, _) ->
                add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.DUPLICATE_COMMAND" (Some unit.UnitId) (Some commandId) []))

            let edges =
                unit.Commands
                |> Array.collect (fun command ->
                    Array.append
                        command.Predecessors
                        ([| command.Fallback; unit.Fallback |]
                         |> Array.choose transitionTarget)
                    |> Array.map (fun target -> command.CommandId, target))

            for command in unit.Commands |> Array.sortBy _.CommandId do
                let commandId = Some command.CommandId
                if command.EarliestStartTick < document.StartTick then
                    add (issue PlanDiagnosticLayer.Schedule "SIR.PLAN.SCHEDULE.BEFORE_START" (Some unit.UnitId) commandId [])
                if command.Predecessors.Length > MaximumDependencies then
                    add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.DEPENDENCY_LIMIT" (Some unit.UnitId) commandId [])
                if strictUtf8.GetByteCount command.Annotation > 4_096 then
                    add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.ANNOTATION_LIMIT" (Some unit.UnitId) commandId [])
                command.Predecessors
                |> Array.filter (fun predecessor -> predecessor = command.CommandId || not (Set.contains predecessor ids))
                |> Array.iter (fun predecessor ->
                    add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.UNKNOWN_DEPENDENCY" (Some unit.UnitId) commandId [ "dependency", guid predecessor ]))
                [ command.Fallback; unit.Fallback ]
                |> List.choose transitionTarget
                |> List.filter (fun target -> not (Set.contains target ids))
                |> List.iter (fun target ->
                    add (issue PlanDiagnosticLayer.Structural "SIR.PLAN.STRUCTURAL.UNKNOWN_FALLBACK" (Some unit.UnitId) commandId [ "target", guid target ]))
                match command.Kind with
                | MovePath(path, _) ->
                    if path.Length < 2 || path.Length > MaximumPathPoints then
                        add (issue PlanDiagnosticLayer.Map "SIR.PLAN.MAP.PATH_LENGTH" (Some unit.UnitId) commandId [])
                    path
                    |> Array.iter (fun cell ->
                        if cell.Col < 0 || cell.Row < 0
                           || cell.Col >= context.Map.Board.Width
                           || cell.Row >= context.Map.Board.Height
                           || Map.tryFind cell context.Map.Board.Terrain = Some BlockedTerrain then
                            add (issue PlanDiagnosticLayer.Map "SIR.PLAN.MAP.INVALID_CELL" (Some unit.UnitId) commandId [ "cell", $"{cell.Col},{cell.Row}" ]))
                    path
                    |> Array.pairwise
                    |> Array.iter (fun (origin, destination) ->
                        if abs (origin.Col - destination.Col) > 1
                           || abs (origin.Row - destination.Row) > 1
                           || origin = destination then
                            add (issue PlanDiagnosticLayer.Map "SIR.PLAN.MAP.NON_ADJACENT_PATH" (Some unit.UnitId) commandId []))
                | SetStance stance when String.IsNullOrWhiteSpace stance ->
                    add (issue PlanDiagnosticLayer.Ruleset "SIR.PLAN.RULESET.UNKNOWN_STANCE" (Some unit.UnitId) commandId [])
                | EngageUnit(target, capability) ->
                    if not (Map.containsKey target context.Map.Units) then
                        add (issue PlanDiagnosticLayer.Map "SIR.PLAN.MAP.UNKNOWN_TARGET" (Some unit.UnitId) commandId [])
                    if capability <> "rifle"
                       && (HumanCapabilities.tryFind capability |> Option.isNone) then
                        add (issue PlanDiagnosticLayer.Ruleset "SIR.PLAN.RULESET.UNKNOWN_CAPABILITY" (Some unit.UnitId) commandId [ "capability", capability ])
                | EngageArea(_, capability)
                    when capability <> "rifle"
                         && (HumanCapabilities.tryFind capability |> Option.isNone) ->
                    add (issue PlanDiagnosticLayer.Ruleset "SIR.PLAN.RULESET.UNKNOWN_CAPABILITY" (Some unit.UnitId) commandId [ "capability", capability ])
                | Synchronize synchronization ->
                    if String.IsNullOrWhiteSpace synchronization.MarkerId
                       || synchronization.DeadlineTick <= command.EarliestStartTick
                       || synchronization.DeadlineTick > document.StartTick + document.HorizonTicks then
                        add (issue PlanDiagnosticLayer.Schedule "SIR.PLAN.SCHEDULE.INVALID_SYNCHRONIZATION" (Some unit.UnitId) commandId [])
                    match synchronization.Mode with
                    | PreloadedClock release
                        when release < command.EarliestStartTick
                             || release > synchronization.DeadlineTick ->
                        add (issue PlanDiagnosticLayer.Schedule "SIR.PLAN.SCHEDULE.INVALID_RELEASE" (Some unit.UnitId) commandId [])
                    | Acknowledged participants when participants.Length = 0 ->
                        add (issue PlanDiagnosticLayer.Schedule "SIR.PLAN.SCHEDULE.EMPTY_PARTICIPANTS" (Some unit.UnitId) commandId [])
                    | _ -> ()
                | _ -> ()

            let adjacency =
                edges
                |> Array.groupBy fst
                |> Array.map (fun (source, values) -> source, values |> Array.map snd)
                |> Map.ofArray
            let mutable visiting = Set.empty
            let mutable visited = Set.empty
            let rec visit node =
                if Set.contains node visiting then true
                elif Set.contains node visited then false
                else
                    visiting <- Set.add node visiting
                    let cyclic =
                        Map.tryFind node adjacency
                        |> Option.defaultValue [||]
                        |> Array.exists visit
                    visiting <- Set.remove node visiting
                    visited <- Set.add node visited
                    cyclic
            unit.Commands
            |> Array.map _.CommandId
            |> Array.sort
            |> Array.filter visit
            |> Array.iter (fun commandId ->
                add (issue PlanDiagnosticLayer.Schedule "SIR.PLAN.SCHEDULE.CYCLE" (Some unit.UnitId) (Some commandId) []))

        document.UnitPlans
        |> Array.collect (fun unit ->
            unit.Commands
            |> Array.choose (fun command ->
                match command.Kind with
                | Synchronize synchronization ->
                    Some(synchronization.MarkerId, unit.UnitId, command.CommandId, synchronization)
                | _ -> None))
        |> Array.groupBy (fun (marker, _, _, _) -> marker)
        |> Array.iter (fun (_, markers) ->
            let fingerprints =
                markers
                |> Array.map (fun (_, _, _, synchronization) ->
                    kindBytes (Synchronize synchronization))
                |> Array.distinct
            if fingerprints.Length <> 1 then
                markers
                |> Array.iter (fun (_, unitId, commandId, _) ->
                    add (issue PlanDiagnosticLayer.Schedule "SIR.PLAN.SCHEDULE.MARKER_MISMATCH" (Some unitId) (Some commandId) [])))

        issues
        |> Seq.sortBy (fun item ->
            (match item.Layer with
             | PlanDiagnosticLayer.Structural -> 0
             | PlanDiagnosticLayer.Map -> 1
             | PlanDiagnosticLayer.Ruleset -> 2
             | PlanDiagnosticLayer.Controller -> 3
             | PlanDiagnosticLayer.Schedule -> 4),
            item.UnitId |> Option.defaultValue -1,
            item.CommandId |> Option.defaultValue Guid.Empty,
            item.Code)
        |> Seq.toArray

    let private duration context unitId command =
        match command.Kind with
        | MovePath(path, posture) ->
            let unit = Map.find unitId context.Map.Units
            let profile = MapScale.movementProfileFor unit.ClassId
            let numerator =
                path
                |> Array.pairwise
                |> Array.sumBy (fun (origin, destination) ->
                    MapScale.movementCost context.Map.Board unit origin destination)
                |> (*) MapScale.TicksPerSecond
            let postureDivisor =
                match posture with
                | Speed -> profile.SpeedMillimetersPerSecond * 5 / 4
                | Readiness -> profile.SpeedMillimetersPerSecond * 3 / 4
                | Balanced -> profile.SpeedMillimetersPerSecond
            max 1 ((numerator + postureDivisor - 1) / postureDivisor)
        | SetFacingIntent _ -> 10
        | SetAttentionIntent _ -> 5
        | SetStance _ -> 1
        | EngageUnit _
        | EngageArea _ -> (MapScale.combatProfileFor (Map.find unitId context.Map.Units).ClassId).RecoveryTicks
        | Hold -> 1
        | Synchronize synchronization ->
            match synchronization.Mode with
            | PreloadedClock release -> max 0 (release - command.EarliestStartTick)
            | Acknowledged _ -> max 0 (synchronization.DeadlineTick - command.EarliestStartTick)

    let schedule
        (context: PlanValidationContext)
        (document: PlanDocument)
        (unit: UnitPlan)
        =
        let commands = unit.Commands |> Array.map (fun command -> command.CommandId, command) |> Map.ofArray
        let mutable scheduled = Map.empty<Guid, ScheduledCommand>
        let mutable remaining = commands
        while not remaining.IsEmpty do
            let ready =
                remaining
                |> Map.toArray
                |> Array.filter (fun (_, command) ->
                    command.Predecessors |> Array.forall (fun dependency -> Map.containsKey dependency scheduled))
                |> Array.sortBy fst
            if ready.Length = 0 then
                remaining <- Map.empty
            else
                for commandId, command in ready do
                    let predecessorFinish =
                        command.Predecessors
                        |> Array.map (fun dependency -> (Map.find dependency scheduled).FinishTick)
                        |> Array.append [| document.StartTick |]
                        |> Array.max
                    let start = max command.EarliestStartTick predecessorFinish
                    let rawFinish = start + duration context unit.UnitId command
                    let finish =
                        match command.Kind with
                        | Synchronize synchronization ->
                            match synchronization.Mode with
                            | PreloadedClock release -> max start release
                            | Acknowledged _ -> synchronization.DeadlineTick
                        | _ -> rawFinish
                    scheduled <-
                        Map.add
                            commandId
                            { UnitId = unit.UnitId
                              Command = command
                              StartTick = start
                              FinishTick = finish }
                            scheduled
                    remaining <- Map.remove commandId remaining
        scheduled |> Map.toArray |> Array.map snd |> Array.sortBy (fun value -> value.StartTick, value.Command.CommandId)

    let private request kind requestId payload =
        { Kind = kind; ModuleRequestId = requestId; Payload = payload }

    let private requestAt
        (context: PlanValidationContext)
        (document: PlanDocument)
        (unit: UnitPlan)
        commandOrdinal
        (scheduled: ScheduledCommand)
        =
        let command = scheduled.Command
        let mutable nextId =
            uint32 (commandOrdinal * (MaximumPathPoints + 1) + 1)
        let make kind payload =
            let value = request kind nextId payload
            nextId <- nextId + 1u
            value
        let ownCell = (Map.find unit.UnitId context.Map.Units).Cell
        let toward cell =
            Direction8.tryFromDelta (cell.Col - ownCell.Col) (cell.Row - ownCell.Row)
        let movementDirection () =
            unit.Commands
            |> Array.sortBy _.CommandId
            |> Array.tryPick (fun candidate ->
                match candidate.Kind with
                | MovePath(path, _) when path.Length >= 2 ->
                    Direction8.tryFromDelta
                        (path[1].Col - path[0].Col)
                        (path[1].Row - path[0].Row)
                | _ -> None)
        let directionRequest kind direction =
            [| scheduled.StartTick, [ make kind [| Direction8.toCode direction |] ] |]
        match command.Kind with
        | MovePath(path, _) ->
            path
            |> Array.pairwise
            |> Array.mapi (fun index (origin, destination) ->
                let direction =
                    Direction8.tryFromDelta (destination.Col - origin.Col) (destination.Row - origin.Row)
                    |> Option.get
                scheduled.StartTick + index,
                [ make RequestKind.SetMovementIntent [| Direction8.toCode direction |] ])
        | SetFacingIntent(FaceFixed direction) ->
            directionRequest RequestKind.SetFacing direction
        | SetFacingIntent FaceAlongMovement ->
            movementDirection ()
            |> Option.map (directionRequest RequestKind.SetFacing)
            |> Option.defaultValue [||]
        | SetFacingIntent(FaceKnownUnit target) ->
            Map.tryFind (UnitId.value target) context.Map.Units
            |> Option.bind (fun targetUnit -> toward targetUnit.Cell)
            |> Option.map (directionRequest RequestKind.SetFacing)
            |> Option.defaultValue [||]
        | SetAttentionIntent(AttendFixed direction) ->
            directionRequest RequestKind.SetAttention direction
        | SetAttentionIntent(AttendRelativeToBody relative) ->
            let body = (Map.find unit.UnitId context.Map.Units).BodyFacing
            let absolute = Direction8.relativeToBody body relative
            directionRequest RequestKind.SetAttention absolute
        | SetAttentionIntent AttendAlongMovement ->
            movementDirection ()
            |> Option.map (directionRequest RequestKind.SetAttention)
            |> Option.defaultValue [||]
        | SetAttentionIntent(AttendKnownUnit target) ->
            Map.tryFind (UnitId.value target) context.Map.Units
            |> Option.bind (fun targetUnit -> toward targetUnit.Cell)
            |> Option.map (directionRequest RequestKind.SetAttention)
            |> Option.defaultValue [||]
        | EngageUnit(target, capability) ->
            let capabilityBytes = strictUtf8.GetBytes capability
            let payload =
                Array.concat
                    [ [| 1uy; 1uy |]
                      CanonicalEncoding.int32LittleEndian target
                      [| byte capabilityBytes.Length |]
                      capabilityBytes ]
            [| scheduled.StartTick, [ make RequestKind.SetEngagement payload ] |]
        | EngageArea(AreaReferent target, capability) ->
            let capabilityBytes = strictUtf8.GetBytes capability
            let payload =
                Array.concat
                    [ [| 1uy; 5uy |]
                      CanonicalEncoding.int32LittleEndian target
                      [| byte capabilityBytes.Length |]
                      capabilityBytes ]
            [| scheduled.StartTick, [ make RequestKind.SetEngagement payload ] |]
        | SetStance stance ->
            [| scheduled.StartTick, [ make RequestKind.SetStance (strictUtf8.GetBytes stance) ] |]
        | Hold ->
            let wake = min (document.StartTick + document.HorizonTicks) (scheduled.StartTick + 1)
            [| scheduled.StartTick, [ make RequestKind.Sleep (CanonicalEncoding.int32LittleEndian wake) ] |]
        | Synchronize synchronization when scheduled.FinishTick > scheduled.StartTick ->
            [| scheduled.StartTick,
               [ make
                     RequestKind.Sleep
                     (CanonicalEncoding.int32LittleEndian scheduled.FinishTick) ] |]
        | _ -> [||]

    let private configuration
        (context: PlanValidationContext)
        (document: PlanDocument)
        (unit: UnitPlan)
        (schedule: ScheduledCommand array)
        =
        let records =
            schedule
            |> Array.mapi (requestAt context document unit)
            |> Array.collect id
            |> Array.groupBy fst
            |> Array.map (fun (tick, values) ->
                let requests = values |> Array.collect (snd >> List.toArray) |> Array.sortBy _.ModuleRequestId |> Array.toList
                let output =
                    V1Codec.encodeOutput tick unit.UnitId 0u 0u requests []
                    |> Result.defaultWith (fun error -> failwithf "Invalid standard-controller output: %A" error)
                tick, output)
            |> Array.sortBy fst
        let bytes = ResizeArray<byte>()
        bytes.AddRange [| byte 'S'; byte 'P'; byte 'C'; byte '1' |]
        writeI32 bytes FormatVersion
        writeI32 bytes records.Length
        for tick, output in records do
            writeI32 bytes tick
            writeI32 bytes output.Length
            bytes.AddRange output
        bytes.ToArray()

    let compile (context: PlanValidationContext) (document: PlanDocument) =
        let issues = validate context document
        if issues.Length <> 0 then Error issues
        else
            let units =
                document.UnitPlans
                |> Array.sortBy _.UnitId
                |> Array.map (fun unit ->
                    let track = schedule context document unit
                    let config = configuration context document unit track
                    { UnitId = unit.UnitId; Configuration = config; Schedule = track })
            let horizonOverflow =
                units
                |> Array.collect _.Schedule
                |> Array.tryFind (fun command ->
                    command.FinishTick > document.StartTick + document.HorizonTicks)
            let configurationOverflow =
                units
                |> Array.tryFind (fun unit ->
                    unit.Configuration.Length > context.MaximumConfigurationBytes)
            match horizonOverflow, configurationOverflow with
            | Some command, _ ->
                Error
                    [| issue
                           PlanDiagnosticLayer.Schedule
                           "SIR.PLAN.SCHEDULE.HORIZON_OVERFLOW"
                           (Some command.UnitId)
                           (Some command.Command.CommandId)
                           [ "finishTick", i32 command.FinishTick ] |]
            | None, Some unit ->
                Error
                    [| issue
                           PlanDiagnosticLayer.Controller
                           "SIR.PLAN.CONTROLLER.CONFIGURATION_LIMIT"
                           (Some unit.UnitId)
                           None
                           [] |]
            | None, None ->
                Ok
                    { SemanticDigest = semanticDigest document
                      SourceDigest = sourceDigest document
                      Units = units }

[<RequireQualifiedAccess>]
module StandardController =
    let source =
        let assembly = Assembly.GetExecutingAssembly()
        use stream =
            match assembly.GetManifestResourceStream("SIR.Match.StandardController.wat") with
            | null -> failwith "Embedded standard controller source is missing."
            | value -> value
        use reader = new StreamReader(stream, Encoding.UTF8, true)
        reader.ReadToEnd()

    let artifactBytes () = Module.ConvertText source
