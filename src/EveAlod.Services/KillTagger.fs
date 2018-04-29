namespace EveAlod.Services

    open EveAlod.Common
    open EveAlod.Common.Combinators
    open EveAlod.Data

    
    type KillTagger(config: Configuration)= 
        
        let isInItemTypeGroup (group: IronSde.ItemTypeGroups) (entity: Entity) =
            let types = group   |> IronSde.ItemTypes.group
                                |> Option.map IronSde.ItemTypes.itemTypes
                                |> Option.defaultValue Seq.empty
            types   |> Seq.exists (fun t -> Strings.str t.id = entity.Id)
        
        let isEcm = isInItemTypeGroup IronSde.ItemTypeGroups.ECM
        let isPlex (item: CargoItem) =  item.Item |> isInItemTypeGroup IronSde.ItemTypeGroups.PLEX
        let isSkillInjector (item: CargoItem) =  item.Item |> isInItemTypeGroup IronSde.ItemTypeGroups.SkillInjectors
        let isPod = isInItemTypeGroup IronSde.ItemTypeGroups.Capsule

        let hasQuantity quantity (item: CargoItem) =  item.Quantity >= quantity
        
        let appendSupplementary (tags: KillTag list) =
            [ Tagging.normalPrice tags; ]
                |> Seq.mapSomes
                |> List.ofSeq
                |> List.append tags

        member __.Tag(kill: Kill)=            
            let tags = [                            
                            Tagging.isCorpKill config.MinCorpDamage config.CorpId;
                            Tagging.isCorpLoss config.CorpId;
                            Tagging.hasItemInHold KillTag.PlexInHold (isPlex <&&> hasQuantity 10);
                            Tagging.hasItemInHold KillTag.SkillInjectorInHold (isSkillInjector <&&> hasQuantity 5);
                            Tagging.hasItemFitted KillTag.EcmFitted isEcm;               
                            Tagging.isPod isPod;
                            Tagging.isPlayer;
                            Tagging.isExpensive;
                            Tagging.isSpendyWithinLimits config.ValuationLimit;
                            Tagging.isShipTypeWideMargin config.ValuationSpread;
                            Tagging.isShipTypeNarrowMargin config.ValuationSpread;
                            Tagging.isCheap; 
                            Tagging.isZeroValue;
                            Tagging.locationTag;
                            Tagging.isGateCamp;
                            Tagging.missingLows;
                            Tagging.missingMids;
                            Tagging.missingRigs;
                        ]
                        |> Seq.map (fun f -> f kill)
                        |> Seq.mapSomes
                        |> List.ofSeq
                        |> appendSupplementary
                        |> List.append kill.Tags
            
            {kill with Tags = tags}
