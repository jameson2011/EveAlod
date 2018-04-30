namespace EveAlod.Data

module ShipTransforms=

    open EveAlod.Common
    
    let shieldMods = [| IronSde.ItemTypeGroups.ShieldBoostAmplifier;
                        IronSde.ItemTypeGroups.AncillaryShieldBooster;
                        IronSde.ItemTypeGroups.ShieldBooster;
                        IronSde.ItemTypeGroups.ShieldHardener;
                        IronSde.ItemTypeGroups.ShieldExtender;
                        IronSde.ItemTypeGroups.ShieldPowerRelay;
                        IronSde.ItemTypeGroups.ShieldRecharger;
                        IronSde.ItemTypeGroups.ShieldResistanceAmplifier;
                        IronSde.ItemTypeGroups.RigShield;
                        |]

    let armourMods = [| IronSde.ItemTypeGroups.AncillaryArmorRepairer;
                        IronSde.ItemTypeGroups.ArmorHardener;
                        IronSde.ItemTypeGroups.ArmorReinforcer;
                        IronSde.ItemTypeGroups.ArmorResistanceShiftHardener;
                        IronSde.ItemTypeGroups.ArmorCoating;
                        IronSde.ItemTypeGroups.ArmorPlatingEnergized;
                        IronSde.ItemTypeGroups.ArmorRepairUnit;
                        IronSde.ItemTypeGroups.RigArmor;
                        |]

    
    let isInItemTypeGroup (group: IronSde.ItemTypeGroups) (entity: Entity) =
        group   |> IronSde.ItemTypes.group
                |> Option.map IronSde.ItemTypes.itemTypes
                |> Option.defaultValue Seq.empty
                |> Seq.exists (fun t -> Strings.str t.id = entity.Id)
       
    let itemType (e: Entity) =
        e.Id |> Strings.toInt |> Option.defaultValue 0 |> IronSde.ItemTypes.itemtype

    let fittingItemType (e: CargoItem)=
        e.Item |> itemType

    let fittedItemTypes (location: ItemLocation) (fittings: seq<CargoItem>) =
        fittings |> Seq.filter (fun e -> e.Location = location)
                    |> Seq.map fittingItemType
                    |> Seq.mapSomes

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
    
