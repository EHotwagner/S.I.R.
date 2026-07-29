namespace SIR.Client

type UnitRole =
    { Name: string
      Faction: string
      Status: string
      Role: string }

type BodyProfile =
    { Name: string
      Status: string
      Health: string
      FrontArmor: string
      FlankArmor: string
      RearArmor: string
      SuppressionResistance: string
      RegenerationPerSecond: string }

type PerkProfile =
    { Family: string
      Name: string
      TacticalChange: string }

type WeaponRole =
    { Name: string
      EngagementShape: string
      Target: string
      TacticalRole: string }

type WeaponProfile =
    { Name: string
      Kind: string
      BaseEngageSeconds: string
      RangeSlope: string
      Exponent: string
      Accuracy: string
      DispersionPerMeter: string
      Damage: string
      Penetration: string
      ShotsPerSecond: string
      EffectDensity: string
      SuppressionPerSecond: string }

type ArmorProfile =
    { Name: string
      Coverage: string
      Cost: string }

type EquipmentGroup =
    { Faction: string
      Status: string
      Category: string
      Items: string }

[<RequireQualifiedAccess>]
module RulesCatalog =
    let unitRoles =
        [ { Name = "Rifleman"
            Faction = "Human"
            Status = "Canonical"
            Role = "Broad baseline competence and flexible substitution" }
          { Name = "Gunner"
            Faction = "Human"
            Status = "Canonical"
            Role = "Sustained area fire, support weapons, and fire discipline" }
          { Name = "Marksman"
            Faction = "Human"
            Status = "Canonical"
            Role = "Slow-building precision at range and observation" }
          { Name = "Engineer"
            Faction = "Human"
            Status = "Canonical"
            Role = "Breaching, demolition, deployables, and prepared positions" }
          { Name = "Medic"
            Faction = "Human"
            Status = "Canonical"
            Role = "Aid, stabilization, and specialist casualty procedures" }
          { Name = "Signaller"
            Faction = "Human"
            Status = "Canonical"
            Role = "Communications, EW, relays, direction finding, and drones" }
          { Name = "Goblin"
            Faction = "Arcane"
            Status = "Proposed"
            Role = "Scouting, skirmishing, sapping, handling, carrying, and crewing" }
          { Name = "Orc"
            Faction = "Arcane"
            Status = "Proposed"
            Role = "Formation fighting, assault, archery, command, and anchor defense" }
          { Name = "Troll"
            Faction = "Arcane"
            Status = "Proposed"
            Role = "Heavy assault, mobile cover, destruction, transport, and recovery" }
          { Name = "Senior caster"
            Faction = "Arcane"
            Status = "Canonical shape"
            Role = "Scarce leader and decisive magical specialist" }
          { Name = "Magical assistant"
            Faction = "Arcane"
            Status = "Canonical shape"
            Role = "Lesser magic, ritual contribution, preparation, and continuity" } ]

    let bodyProfiles =
        [ { Name = "Human operative"
            Status = "Prototype"
            Health = "12"
            FrontArmor = "0"
            FlankArmor = "0"
            RearArmor = "0"
            SuppressionResistance = "1.00"
            RegenerationPerSecond = "0" }
          { Name = "Observation or relay drone"
            Status = "Prototype"
            Health = "8"
            FrontArmor = "0"
            FlankArmor = "0"
            RearArmor = "0"
            SuppressionResistance = "—"
            RegenerationPerSecond = "0" }
          { Name = "Goblin skirmisher"
            Status = "Prototype"
            Health = "35"
            FrontArmor = "8"
            FlankArmor = "4"
            RearArmor = "2"
            SuppressionResistance = "0.75"
            RegenerationPerSecond = "0" }
          { Name = "Shielded orc"
            Status = "Prototype"
            Health = "100"
            FrontArmor = "38"
            FlankArmor = "16"
            RearArmor = "10"
            SuppressionResistance = "1.25"
            RegenerationPerSecond = "0" }
          { Name = "Armored troll"
            Status = "Prototype"
            Health = "240"
            FrontArmor = "55"
            FlankArmor = "38"
            RearArmor = "24"
            SuppressionResistance = "1.80"
            RegenerationPerSecond = "6" } ]

    let private perks family values =
        values
        |> List.map (fun (name, tacticalChange) ->
            { Family = family
              Name = name
              TacticalChange = tacticalChange })

    let perkProfiles =
        [ perks
              "Rifleman"
              [ "Point Man", "Improves the constrained first response while advancing"
                "Bounding Partner", "Restores readiness better after movement under confirmed covering fire"
                "Quiet Advance", "Trades speed for reduced visual and acoustic signature"
                "Cross-Trained", "Reduces, but does not erase, off-class equipment penalties"
                "Local Initiative", "Executes the last received intent better while disconnected"
                "Rear Guard", "Maintains observation and readiness during disengagement" ]
          perks
              "Gunner"
              [ "Traverse Discipline", "Redirects an area engagement while preserving preparation"
                "Beaten Zone", "Chooses narrow/deep or broad/shallow engagement shapes"
                "Walking Fire", "Shifts suppression along a declared movement path"
                "Fire Control", "Avoids wasting ammunition on unsuitable parts of an area"
                "Final Protective Fire", "Prepares an ammunition-expensive close defensive line"
                "Crew Drill", "Benefits from a cooperating ammunition or weapon assistant" ]
          perks
              "Marksman"
              [ "Patient Solution", "Preserves limited targeting progress through a very brief obstruction"
                "Spotter Pair", "Uses explicitly relayed spotter observations for an initial solution"
                "Counter-Observer", "Recognizes optics and evidence of surveillance"
                "Target Discrimination", "Identifies observable equipment and behavior before firing"
                "Cold Position", "Produces less movement evidence in a prepared position"
                "Displacement Drill", "Leaves after firing more efficiently but abandons the solution" ]
          perks
              "Engineer"
              [ "Hasty Breach", "Faster, louder, less-controlled entry"
                "Surgical Breach", "Slower entry with constrained collateral damage"
                "Remote Initiation", "Connects a prepared charge to a physical trigger"
                "Field Fortification", "Places cover and obstacles where terrain permits"
                "Trap Sense", "Recognizes disturbed terrain, mines, and ritual sites"
                "Render Safe", "Dismantles eligible deployables and discovered ritual traps" ]
          perks
              "Medic"
              [ "Triage", "Rapidly assesses several casualties"
                "Under Fire", "Permits limited exposed stabilization at added cost or reduced reliability"
                "Damage Control", "Treats a defined complication beyond ordinary aid"
                "Conservative Medicine", "Saves supplies when time and safety permit"
                "Casualty Movement", "Coordinates carrying or dragging with less disruption"
                "Return to Duty", "Improves limited function after stabilization without removing wounds" ]
          perks
              "Signaller"
              [ "Burst Discipline", "Trades immediacy for shorter, less exposed transmissions"
                "Frequency Agility", "Reconfigures faster after interference without granting immunity"
                "Cross-Cueing", "Correlates legitimate acoustic, thermal, radio, and magical observations"
                "False Traffic", "Makes decoy emissions resemble plausible network behavior"
                "Drone Shepherd", "Gives drones better pre-disconnection contingencies"
                "Relay Architect", "Predicts relay coverage and weak links"
                "Borrowed-Eye Hunter", "Recognizes evidence of active critter attunement" ]
          perks
              "Leadership"
              [ "Clear Intent", "Supplies a richer fallback plan before disconnection"
                "Fire Coordinator", "Establishes confirmed-target or covering-fire instructions"
                "Controlled Succession", "Reduces disruption when command transfers"
                "Emission Discipline", "Sets silent, scheduled, emergency-only, or continuous transmission posture"
                "Steady Withdrawal", "Preserves formation and reporting during disengagement" ] ]
        |> List.concat

    let weaponRoles =
        [ { Name = "Carbine"
            EngagementShape = "Short base, moderate rise"
            Target = "Point"
            TacticalRole = "Close and mid-range assault default" }
          { Name = "Rifle"
            EngagementShape = "Moderate base, shallow rise"
            Target = "Point"
            TacticalRole = "General-purpose baseline" }
          { Name = "Shotgun"
            EngagementShape = "Very short base, steep degradation"
            Target = "Point"
            TacticalRole = "Interior and doorway dominance; poor across open ground" }
          { Name = "Marksman rifle"
            EngagementShape = "Long base, nearly flat"
            Target = "Point"
            TacticalRole = "Punishes sustained exposure at range; weak against peeking" }
          { Name = "Support weapon"
            EngagementShape = "Prepared and slow to redirect"
            Target = "Area"
            TacticalRole = "Denies ground, suppresses, and covers movement" }
          { Name = "Grenade launcher"
            EngagementShape = "Slow projectile"
            Target = "Area"
            TacticalRole = "Reaches behind or over cover" }
          { Name = "Anti-armor launcher"
            EngagementShape = "Long base, slow projectile"
            Target = "Point"
            TacticalRole = "Vehicles, hardened positions, and heavy creatures" } ]

    let weaponProfiles =
        [ { Name = "Carbine"; Kind = "Point"; BaseEngageSeconds = "0.32"; RangeSlope = "0.018"; Exponent = "1.15"; Accuracy = "0.86"; DispersionPerMeter = "0.005"; Damage = "30"; Penetration = "24"; ShotsPerSecond = "2.60"; EffectDensity = "1.00"; SuppressionPerSecond = "7" }
          { Name = "Rifle"; Kind = "Point"; BaseEngageSeconds = "0.55"; RangeSlope = "0.012"; Exponent = "1.10"; Accuracy = "0.88"; DispersionPerMeter = "0.004"; Damage = "35"; Penetration = "28"; ShotsPerSecond = "2.20"; EffectDensity = "1.00"; SuppressionPerSecond = "8" }
          { Name = "Shotgun"; Kind = "Point"; BaseEngageSeconds = "0.18"; RangeSlope = "0.040"; Exponent = "1.25"; Accuracy = "0.82"; DispersionPerMeter = "0.018"; Damage = "52"; Penetration = "16"; ShotsPerSecond = "1.25"; EffectDensity = "1.00"; SuppressionPerSecond = "12" }
          { Name = "Marksman rifle"; Kind = "Point"; BaseEngageSeconds = "1.25"; RangeSlope = "0.003"; Exponent = "1.00"; Accuracy = "0.94"; DispersionPerMeter = "0.001"; Damage = "55"; Penetration = "36"; ShotsPerSecond = "0.65"; EffectDensity = "1.00"; SuppressionPerSecond = "5" }
          { Name = "Support weapon"; Kind = "Area"; BaseEngageSeconds = "0.72"; RangeSlope = "0.006"; Exponent = "1.00"; Accuracy = "0.78"; DispersionPerMeter = "0.006"; Damage = "24"; Penetration = "24"; ShotsPerSecond = "7.00"; EffectDensity = "0.12"; SuppressionPerSecond = "42" }
          { Name = "Grenade launcher"; Kind = "Area"; BaseEngageSeconds = "1.10"; RangeSlope = "0.010"; Exponent = "1.00"; Accuracy = "0.72"; DispersionPerMeter = "0.008"; Damage = "70"; Penetration = "25"; ShotsPerSecond = "0.25"; EffectDensity = "0.35"; SuppressionPerSecond = "30" }
          { Name = "Anti-armor launcher"; Kind = "Point"; BaseEngageSeconds = "1.50"; RangeSlope = "0.006"; Exponent = "1.00"; Accuracy = "0.76"; DispersionPerMeter = "0.003"; Damage = "120"; Penetration = "85"; ShotsPerSecond = "0.30"; EffectDensity = "1.00"; SuppressionPerSecond = "18" } ]

    let armorProfiles =
        [ { Name = "None or soft"
            Coverage = "Fragmentation protection"
            Cost = "Low weight; scout choice" }
          { Name = "Plate carrier"
            Coverage = "Front and rear plates with soft flanks"
            Cost = "Moderate weight and capacity cost" }
          { Name = "Heavy armor"
            Coverage = "Plates plus limb and neck protection"
            Cost = "High weight, capacity, and readiness cost" } ]

    let equipmentGroups =
        [ { Faction = "Human"; Status = "Canonical capability"; Category = "Weapon packages"; Items = "Suppressor; compact optic; magnified optic; thermal weapon sight; bipod or support mount; under-barrel launcher; specialist ammunition" }
          { Faction = "Human"; Status = "Canonical capability"; Category = "Communications"; Items = "Personal set; command-net set; directional antenna; burst-transmission unit; deployable relay; relay drone; physical courier package" }
          { Faction = "Human"; Status = "Canonical capability"; Category = "Sensors"; Items = "Compact optics; magnified observation optic; thermal imager; acoustic direction finder; magical-signature detector; trip sensor; observation drone" }
          { Faction = "Human"; Status = "Canonical capability"; Category = "Electronic warfare"; Items = "Configurable jammer; radio direction finder; decoy emitter" }
          { Faction = "Human"; Status = "Canonical capability"; Category = "Medical"; Items = "Individual aid kit; stabilization kit; nanomedical stock; diagnostic sensor; casualty harness or folding litter" }
          { Faction = "Human"; Status = "Canonical capability"; Category = "Engineering"; Items = "Breaching and demolition charges; cutting tool; remote initiator; deployable cover; obstacles; mines; sensor stakes; critter-control and ritual-disruption tools" }
          { Faction = "Human"; Status = "Canonical capability"; Category = "Sustainment"; Items = "Ammunition; batteries; drone parts; engineering consumables; medical stock; relay components" }
          { Faction = "Arcane"; Status = "Proposed"; Category = "Caster equipment"; Items = "Casting focus; memory aid; component satchel; ward tokens; scrying vessel; ritual kit; portal frame; bound standard or anchor stone; ceremonial armor" }
          { Faction = "Arcane"; Status = "Proposed"; Category = "Goblin equipment"; Items = "Shortbows; slings; knives; light spears; nets; climbing gear; obscuring pots; traps; critter cages; component panniers" }
          { Faction = "Arcane"; Status = "Proposed"; Category = "Orc equipment"; Items = "Shields; spears; polearms; axes; heavy bows; armor; pavises; breaching tools; standards; ritual-site defenses" }
          { Faction = "Arcane"; Status = "Proposed"; Category = "Troll equipment"; Items = "Harness armor; massive tools and weapons; throwing loads; cargo and casualty harnesses; portable magical infrastructure" } ]
