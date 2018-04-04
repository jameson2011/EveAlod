namespace EveAlod.Services

    open EveAlod.Common
    open EveAlod.Common.Combinators
    open EveAlod.Data

    
    type KillTagger(config: Configuration, entityProvider: StaticDataActor)=
        
        let isType (key: EntityGroupKey) (entity:Entity) =
            let group = entityProvider.EntityIds(key) |> Async.RunSynchronously
            match group with 
            | Some grp -> grp.Contains(entity.Id)
            | None -> false
        
        let isEcm = isType EntityGroupKey.Ecm
        let isPlex (item: CargoItem) = 
            item.Item |> isType EntityGroupKey.Plex
        let isSkillInjector (item: CargoItem) = 
            item.Item |> isType EntityGroupKey.SkillInjector
        let hasQuantity quantity (item: CargoItem) = 
            item.Quantity >= quantity
        let isPod = isType EntityGroupKey.Capsule
        
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
                        ]
                        |> Seq.map (fun f -> f kill)
                        |> Seq.mapSomes
                        |> List.ofSeq
                        |> List.append kill.Tags
            
            {kill with Tags = tags}
