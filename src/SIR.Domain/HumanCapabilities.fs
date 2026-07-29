namespace SIR.Domain

open System

/// The targeting shape owned by a versioned capability descriptor.
type CapabilityTargetContract =
    | PointTarget
    | AreaTarget

/// What interruption does to preparation already invested in an action.
type CapabilityInterruptionRule =
    | LosePreparation
    | PreservePreparation

/// Versioned, data-owned semantics for one ordinary human weapon role.
type HumanWeaponCapabilityDescriptor =
    { CapabilityId: string
      Version: int32
      EquipmentName: string
      Role: string
      TargetContract: CapabilityTargetContract
      PreparationTicks: int32
      TraverseTicksPerDirection: int32
      MaximumRangeCells: int32
      AmmunitionPerResolution: int32
      InterruptionRule: CapabilityInterruptionRule
      PlanningDecision: string }

/// Explicit equipment attached to one authored unit.
type AuthoredUnitLoadout =
    { UnitId: int32
      Role: string
      Equipment: string array
      CapabilityIds: string array }

[<RequireQualifiedAccess>]
module HumanCapabilities =
    [<Literal>]
    let DescriptorSetId = "sir.human-weapons"

    [<Literal>]
    let DescriptorSetVersion = 1

    let descriptors =
        [| { CapabilityId = "human.weapon.carbine"
             Version = 1
             EquipmentName = "Carbine"
             Role = "Close assault"
             TargetContract = PointTarget
             PreparationTicks = 2
             TraverseTicksPerDirection = 1
             MaximumRangeCells = 6
             AmmunitionPerResolution = 1
             InterruptionRule = LosePreparation
             PlanningDecision = "take a close route to exploit fast preparation" }
           { CapabilityId = "human.weapon.rifle"
             Version = 1
             EquipmentName = "Rifle"
             Role = "General purpose"
             TargetContract = PointTarget
             PreparationTicks = 4
             TraverseTicksPerDirection = 1
             MaximumRangeCells = 12
             AmmunitionPerResolution = 1
             InterruptionRule = LosePreparation
             PlanningDecision = "hold a flexible mid-range firing position" }
           { CapabilityId = "human.weapon.shotgun"
             Version = 1
             EquipmentName = "Shotgun"
             Role = "Doorway dominance"
             TargetContract = PointTarget
             PreparationTicks = 1
             TraverseTicksPerDirection = 1
             MaximumRangeCells = 3
             AmmunitionPerResolution = 1
             InterruptionRule = LosePreparation
             PlanningDecision = "occupy an interior or doorway-adjacent cell" }
           { CapabilityId = "human.weapon.marksman-rifle"
             Version = 1
             EquipmentName = "Marksman rifle"
             Role = "Precision fire"
             TargetContract = PointTarget
             PreparationTicks = 8
             TraverseTicksPerDirection = 2
             MaximumRangeCells = 24
             AmmunitionPerResolution = 1
             InterruptionRule = PreservePreparation
             PlanningDecision = "choose a distant stable sightline before preparing" }
           { CapabilityId = "human.weapon.support"
             Version = 1
             EquipmentName = "Support weapon"
             Role = "Area denial"
             TargetContract = AreaTarget
             PreparationTicks = 10
             TraverseTicksPerDirection = 3
             MaximumRangeCells = 16
             AmmunitionPerResolution = 4
             InterruptionRule = LosePreparation
             PlanningDecision = "prepare a covered position whose traverse reaches a threatened area" }
           { CapabilityId = "human.weapon.grenade-launcher"
             Version = 1
             EquipmentName = "Grenade launcher"
             Role = "Indirect area fire"
             TargetContract = AreaTarget
             PreparationTicks = 6
             TraverseTicksPerDirection = 2
             MaximumRangeCells = 14
             AmmunitionPerResolution = 1
             InterruptionRule = LosePreparation
             PlanningDecision = "select an area behind intervening cover and preserve launcher range" }
           { CapabilityId = "human.weapon.anti-armor-launcher"
             Version = 1
             EquipmentName = "Anti-armor launcher"
             Role = "Hardened target defeat"
             TargetContract = PointTarget
             PreparationTicks = 12
             TraverseTicksPerDirection = 2
             MaximumRangeCells = 18
             AmmunitionPerResolution = 1
             InterruptionRule = LosePreparation
             PlanningDecision = "reserve a long exposed preparation window against a hardened point target" } |]

    let tryFind capabilityId =
        descriptors
        |> Array.tryFind (fun descriptor ->
            String.Equals(
                descriptor.CapabilityId,
                capabilityId,
                StringComparison.Ordinal
            ))

    let createLoadout unitId role capabilityIds =
        let resolved =
            capabilityIds
            |> Array.choose tryFind

        if resolved.Length <> capabilityIds.Length
           || capabilityIds |> Array.distinct |> Array.length <> capabilityIds.Length then
            Error "Loadout capabilities must be unique accepted human descriptor identifiers."
        else
            Ok
                { UnitId = unitId
                  Role = role
                  Equipment = resolved |> Array.map _.EquipmentName
                  CapabilityIds = Array.copy capabilityIds }

    /// UI default only; authored artifacts retain the resulting explicit loadout.
    let defaultLoadout unitId classId =
        let capabilityId =
            match classId with
            | "rifleman"
            | "planning-unit" -> Some "human.weapon.rifle"
            | "gunner" -> Some "human.weapon.support"
            | "marksman" -> Some "human.weapon.marksman-rifle"
            | "engineer" -> Some "human.weapon.shotgun"
            | "medic" -> Some "human.weapon.carbine"
            | "signaller" -> Some "human.weapon.grenade-launcher"
            | _ -> None

        createLoadout
            unitId
            classId
            (capabilityId |> Option.toArray)
        |> Result.defaultWith invalidOp
