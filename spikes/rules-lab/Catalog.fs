module Catalog

open Domain

let parameters =
    { ExposureFloor = 0.10
      SuppressionThreshold = 50.0
      SuppressionEngagementPenalty = 0.60 }

let goblin =
    { Name = "Goblin skirmisher"
      MaxHp = 35.0
      Armour = { Front = 8.0; Flank = 4.0; Rear = 2.0 }
      SuppressionResistance = 0.75
      RegenerationPerSecond = 0.0 }

let orc =
    { Name = "Shielded orc"
      MaxHp = 100.0
      Armour = { Front = 38.0; Flank = 16.0; Rear = 10.0 }
      SuppressionResistance = 1.25
      RegenerationPerSecond = 0.0 }

let troll =
    { Name = "Armoured troll"
      MaxHp = 240.0
      Armour = { Front = 55.0; Flank = 38.0; Rear = 24.0 }
      SuppressionResistance = 1.80
      RegenerationPerSecond = 6.0 }

let carbine =
    { Name = "Carbine"
      Kind = EngagementKind.Point
      BaseEngagementSeconds = 0.32
      RangeSlope = 0.018
      RangeExponent = 1.15
      Accuracy = 0.86
      DispersionPerMeter = 0.005
      Damage = 30.0
      Penetration = 24.0
      ShotsPerSecond = 2.6
      EffectDensity = 1.0
      SuppressionPerSecond = 7.0 }

let rifle =
    { Name = "Rifle"
      Kind = EngagementKind.Point
      BaseEngagementSeconds = 0.55
      RangeSlope = 0.012
      RangeExponent = 1.10
      Accuracy = 0.88
      DispersionPerMeter = 0.004
      Damage = 35.0
      Penetration = 28.0
      ShotsPerSecond = 2.2
      EffectDensity = 1.0
      SuppressionPerSecond = 8.0 }

let marksmanRifle =
    { Name = "Marksman rifle"
      Kind = EngagementKind.Point
      BaseEngagementSeconds = 1.25
      RangeSlope = 0.003
      RangeExponent = 1.0
      Accuracy = 0.94
      DispersionPerMeter = 0.001
      Damage = 55.0
      Penetration = 36.0
      ShotsPerSecond = 0.65
      EffectDensity = 1.0
      SuppressionPerSecond = 5.0 }

let supportWeapon =
    { Name = "Support weapon"
      Kind = EngagementKind.Area
      BaseEngagementSeconds = 0.72
      RangeSlope = 0.006
      RangeExponent = 1.0
      Accuracy = 0.78
      DispersionPerMeter = 0.006
      Damage = 24.0
      Penetration = 24.0
      ShotsPerSecond = 7.0
      EffectDensity = 0.12
      SuppressionPerSecond = 42.0 }

let antiArmourLauncher =
    { Name = "Anti-armour launcher"
      Kind = EngagementKind.Point
      BaseEngagementSeconds = 1.50
      RangeSlope = 0.006
      RangeExponent = 1.0
      Accuracy = 0.76
      DispersionPerMeter = 0.003
      Damage = 120.0
      Penetration = 85.0
      ShotsPerSecond = 0.30
      EffectDensity = 1.0
      SuppressionPerSecond = 18.0 }

let weapons =
    [ carbine; rifle; marksmanRifle; supportWeapon; antiArmourLauncher ]

let state name target bearing range exposure cover =
    { Name = name
      Attacker = { X = 0.0; Y = 0.0 }
      Target = { X = range; Y = 0.0 }
      TargetBody = target
      Bearing = bearing
      Exposure = exposure
      CoverProtection = cover
      ExistingSuppression = 0.0 }
