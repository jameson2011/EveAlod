namespace EveAlod.Data

module ShipTransforms=

    open EveAlod.Common
    open IronSde
    open FSharp.Data
    open FSharp.Data
    
    let shieldMods = [| IronSde.ItemTypeGroups.ShieldBoostAmplifier;
                        IronSde.ItemTypeGroups.AncillaryShieldBooster;
                        IronSde.ItemTypeGroups.ShieldBooster;
                        IronSde.ItemTypeGroups.ShieldHardener;
                        IronSde.ItemTypeGroups.ShieldExtender;
                        IronSde.ItemTypeGroups.ShieldPowerRelay;
                        IronSde.ItemTypeGroups.ShieldRecharger;
                        IronSde.ItemTypeGroups.ShieldResistanceAmplifier;
                        IronSde.ItemTypeGroups.RigShield;
                        |] |> Set.ofSeq

    let armourMods = [| IronSde.ItemTypeGroups.AncillaryArmorRepairer;
                        IronSde.ItemTypeGroups.ArmorHardener;
                        IronSde.ItemTypeGroups.ArmorReinforcer;
                        IronSde.ItemTypeGroups.ArmorResistanceShiftHardener;
                        IronSde.ItemTypeGroups.ArmorCoating;
                        IronSde.ItemTypeGroups.ArmorPlatingEnergized;
                        IronSde.ItemTypeGroups.ArmorRepairUnit;
                        IronSde.ItemTypeGroups.RigArmor;
                        |] |> Set.ofSeq

    
    let isInItemTypeGroup (group: IronSde.ItemTypeGroups) (entity: Entity) =
        group   |> IronSde.ItemTypes.group
                |> Option.map IronSde.ItemTypes.itemTypes
                |> Option.defaultValue Seq.empty
                |> Seq.exists (fun t -> Strings.str t.id = entity.Id)
       
    let itemType (e: Entity) =
        e.Id |> Strings.toInt |> Option.defaultValue 0 |> IronSde.ItemTypes.itemType
        
    let fittingItemType (e: CargoItem)=
        e.Item |> itemType

    let fittedItemTypes (location: ItemLocation) (fittings: seq<CargoItem>) =
        fittings |> Seq.filter (fun e -> e.Location = location)
                    |> Seq.map fittingItemType
                    |> Seq.mapSomes

    let itemTypeGroups (items: seq<CargoItem>) =
        items   |> Seq.map fittingItemType
                |> Seq.mapSomes
                |> Seq.map (fun t -> t.group.key)
                |> Seq.distinct
    
    let isInGroup (groups: Set<IronSde.ItemTypeGroups>) (item: Entity)=
        item |> itemType
             |> Option.map (fun t -> t.group.key )
             |> Option.map (fun t -> groups |> Set.contains t) 
             |> Option.defaultValue false

    let isArmourMod = isInGroup armourMods

    let isShieldMod = isInGroup shieldMods

    let shipTypeSlot (slot: IronSde.AttributeTypes) (itemType: IronSde.ItemType) =
        match IronSde.ItemTypes.attribute slot itemType with
                    | Some a -> int a.value 
                    | _ -> 0

    let attrType = function
        | ItemLocation.LowSlot -> Some IronSde.AttributeTypes.lowSlots
        | ItemLocation.HighSlot -> Some IronSde.AttributeTypes.hiSlots
        | ItemLocation.MidSlot -> Some IronSde.AttributeTypes.medSlots
        | ItemLocation.RigSlot -> Some IronSde.AttributeTypes.rigSlots
        | _ -> None

        
    let hasQuantity quantity (item: CargoItem) =  item.Quantity >= quantity

    let isEcm (item: CargoItem) =  item.Item |> isInItemTypeGroup IronSde.ItemTypeGroups.ECM
    let isPlex (item: CargoItem) =  item.Item |> isInItemTypeGroup IronSde.ItemTypeGroups.PLEX
    let isSkillInjector (item: CargoItem) =  item.Item |> isInItemTypeGroup IronSde.ItemTypeGroups.SkillInjectors
    let isPod = isInItemTypeGroup IronSde.ItemTypeGroups.Capsule
    let isWarpCoreStab (item: CargoItem) =  item.Item |> isInItemTypeGroup IronSde.ItemTypeGroups.WarpCoreStabilizer
    
    
    // TODO:

    let isShip itemType = itemType.group.category = IronSde.ItemTypeCategories.Ship
    
    // TODO:
    let isDrone itemType = itemType.group.category = IronSde.ItemTypeCategories.Drone

    let shipIsFittable (ship: IronSde.ItemType) =
        
        let isPod = ship.group.key = IronSde.ItemTypeGroups.Capsule
        
        let lows = ship |> shipTypeSlot IronSde.AttributeTypes.lowSlots
        let mids = ship |> shipTypeSlot IronSde.AttributeTypes.medSlots
        let highs = ship |> shipTypeSlot IronSde.AttributeTypes.hiSlots
        let rigs = ship |> shipTypeSlot IronSde.AttributeTypes.rigSlots       
        let hold = ship.capacity |> Option.defaultValue 0.
        
        match isPod, lows, mids, highs, rigs, hold with
        | true, _, _, _, _, _ ->    true
        | false, 0, 0, 0, 0, 0. ->  false
        | _ ->                      true
          