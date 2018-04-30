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
        let types = group   |> IronSde.ItemTypes.group
                            |> Option.map IronSde.ItemTypes.itemTypes
                            |> Option.defaultValue Seq.empty
        types   |> Seq.exists (fun t -> Strings.str t.id = entity.Id)

        
    let hasQuantity quantity (item: CargoItem) =  item.Quantity >= quantity

    let isEcm (item: CargoItem) =  item.Item |> isInItemTypeGroup IronSde.ItemTypeGroups.ECM
    let isPlex (item: CargoItem) =  item.Item |> isInItemTypeGroup IronSde.ItemTypeGroups.PLEX
    let isSkillInjector (item: CargoItem) =  item.Item |> isInItemTypeGroup IronSde.ItemTypeGroups.SkillInjectors
    let isPod = isInItemTypeGroup IronSde.ItemTypeGroups.Capsule
    let isWarpCoreStab (item: CargoItem) =  item.Item |> isInItemTypeGroup IronSde.ItemTypeGroups.WarpCoreStabilizer
    