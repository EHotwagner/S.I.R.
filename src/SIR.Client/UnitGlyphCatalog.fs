namespace SIR.Client

type GlyphPrimitive =
    | FilledPath of pathData: string
    | StrokedPath of pathData: string
    | Circle of centerX: float * centerY: float * radius: float

type UnitGlyphDefinition =
    { Id: UnitClassId
      Name: string
      Description: string
      TextAlternative: string
      Primitives: GlyphPrimitive array }

[<RequireQualifiedAccess>]
module UnitGlyphCatalog =
    let private glyph id name description textAlternative primitives =
        { Id = UnitClassId.resolve id
          Name = name
          Description = description
          TextAlternative = textAlternative
          Primitives = primitives }

    let placeholder =
        { Id = UnitClassId.placeholder
          Name = "Unknown unit"
          Description =
            "A visible diamond with an inset question-mark-like hook; used safely when a class identifier is unsupported."
          TextAlternative = "Unknown unit class"
          Primitives =
            [| StrokedPath "M12 2 L22 12 L12 22 L2 12 Z"
               StrokedPath "M8 8 C8 4 16 4 16 9 C16 12 12 12 12 16"
               Circle(12, 19, 1) |] }

    /// Built-in, replay-independent SVG geometry on a normalized 24 × 24 grid.
    let all =
        [| glyph "rifleman" "Rifleman" "A forward chevron crossed by a rifle line." "Human rifleman" [| FilledPath "M4 18 L12 4 L20 18 L16 18 L12 11 L8 18 Z"; StrokedPath "M5 20 L19 8" |]
           glyph "gunner" "Gunner" "A heavy horizontal weapon bar on a bipod." "Human gunner" [| FilledPath "M3 8 H21 V12 H3 Z"; StrokedPath "M8 12 L5 21 M16 12 L19 21" |]
           glyph "marksman" "Marksman" "A sight diamond around a central precision point." "Human marksman" [| StrokedPath "M12 2 L22 12 L12 22 L2 12 Z"; Circle(12, 12, 2) |]
           glyph "engineer" "Engineer" "A bridge-like lintel over two supports." "Human engineer" [| FilledPath "M3 5 H21 V9 H3 Z M5 9 H9 V21 H5 Z M15 9 H19 V21 H15 Z" |]
           glyph "medic" "Medic" "Four equal blocks form a medical cross with open corners." "Human medic" [| FilledPath "M9 3 H15 V9 H21 V15 H15 V21 H9 V15 H3 V9 H9 Z" |]
           glyph "signaller" "Signaller" "A mast emits two symmetric signal arcs." "Human signaller" [| FilledPath "M10 10 H14 V22 H10 Z"; StrokedPath "M8 9 C5 6 5 3 7 1 M16 9 C19 6 19 3 17 1 M5 12 C1 8 1 4 3 1 M19 12 C23 8 23 4 21 1" |]
           glyph "observation-drone" "Observation drone" "A four-arm airframe surrounding an observation lens." "Observation drone" [| StrokedPath "M12 12 L4 4 M12 12 L20 4 M12 12 L4 20 M12 12 L20 20"; Circle(4, 4, 2); Circle(20, 4, 2); Circle(4, 20, 2); Circle(20, 20, 2); Circle(12, 12, 2) |]
           glyph "relay-drone" "Relay drone" "A four-arm airframe with a central relay mast." "Relay drone" [| StrokedPath "M12 12 L4 4 M12 12 L20 4 M12 12 L4 20 M12 12 L20 20 M12 12 V3"; Circle(4, 4, 2); Circle(20, 4, 2); Circle(4, 20, 2); Circle(20, 20, 2); FilledPath "M9 3 L12 0 L15 3 Z" |]
           glyph "goblin" "Goblin" "A low triangular head with wide pointed ears." "Arcane goblin" [| FilledPath "M2 8 L8 10 L12 5 L16 10 L22 8 L18 18 L12 22 L6 18 Z" |]
           glyph "orc" "Orc" "A broad shield with two upward tusk cuts." "Arcane orc" [| FilledPath "M4 3 H20 V13 C20 18 16 21 12 23 C8 21 4 18 4 13 Z"; StrokedPath "M8 17 L10 11 M16 17 L14 11" |]
           glyph "troll" "Troll" "A massive stepped silhouette with wide shoulders." "Arcane troll" [| FilledPath "M2 7 H7 V3 H17 V7 H22 V20 H16 V15 H8 V20 H2 Z" |]
           glyph "senior-caster" "Senior caster" "A six-rayed focus around a central ring." "Arcane senior caster" [| StrokedPath "M12 1 V7 M12 17 V23 M1 12 H7 M17 12 H23 M4 4 L8 8 M16 16 L20 20 M20 4 L16 8 M8 16 L4 20"; Circle(12, 12, 4) |]
           glyph "magical-assistant" "Magical assistant" "A three-rayed focus around a small central ring." "Arcane magical assistant" [| StrokedPath "M12 2 V8 M3 19 L9 15 M21 19 L15 15"; Circle(12, 12, 3) |]
           glyph "ambient-critter" "Ambient critter" "A small body with two distinct tracks." "Ambient critter" [| FilledPath "M7 10 C7 5 17 5 17 12 C17 17 12 20 7 17 Z"; Circle(6, 5, 2); Circle(18, 19, 2) |] |]

    let private byId =
        all
        |> Array.map (fun definition ->
            UnitClassId.value definition.Id, definition)
        |> Map.ofArray

    /// Unknown replay input always resolves to the visible placeholder.
    let resolve classId =
        byId
        |> Map.tryFind (UnitClassId.value classId)
        |> Option.defaultValue placeholder

type PaletteTokens =
    { Id: string
      Canvas: string
      Terrain: string
      Grid: string
      Text: string
      HumanFaction: string
      ArcaneFaction: string
      NeutralFaction: string
      HealthActive: string
      HealthDepleted: string
      Focus: string
      OverlayPatterns: string array
      UsesPatterns: bool }

[<RequireQualifiedAccess>]
module ReplayPalettes =
    let accessibleDefault =
        { Id = "accessible-default"
          Canvas = "#10161d"
          Terrain = "#28343d"
          Grid = "#71808b"
          Text = "#f7f9fa"
          HumanFaction = "#53b7ff"
          ArcaneFaction = "#d792ff"
          NeutralFaction = "#ffd166"
          HealthActive = "#ff6b6b"
          HealthDepleted = "#59636b"
          Focus = "#ffffff"
          OverlayPatterns = [| "solid"; "dash"; "dot"; "crosshatch" |]
          UsesPatterns = true }

    let highContrast =
        { Id = "high-contrast"
          Canvas = "#000000"
          Terrain = "#000000"
          Grid = "#ffffff"
          Text = "#ffffff"
          HumanFaction = "#00ffff"
          ArcaneFaction = "#ff66ff"
          NeutralFaction = "#ffff00"
          HealthActive = "#ffffff"
          HealthDepleted = "#555555"
          Focus = "#00ff00"
          OverlayPatterns = [| "solid"; "long-dash"; "dense-dot"; "crosshatch" |]
          UsesPatterns = true }

    let monochromePattern =
        { Id = "monochrome-pattern"
          Canvas = "#ffffff"
          Terrain = "#eeeeee"
          Grid = "#555555"
          Text = "#000000"
          HumanFaction = "#000000"
          ArcaneFaction = "#000000"
          NeutralFaction = "#000000"
          HealthActive = "#000000"
          HealthDepleted = "#b5b5b5"
          Focus = "#000000"
          OverlayPatterns = [| "horizontal"; "diagonal"; "vertical"; "crosshatch" |]
          UsesPatterns = true }

    let all = [| accessibleDefault; highContrast; monochromePattern |]
