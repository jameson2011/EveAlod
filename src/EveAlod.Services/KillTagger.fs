namespace EveAlod.Services

    open EveAlod.Common
    open EveAlod.Data

    type KillTagger(entityProvider: IStaticEntityProvider, corpId: string)=
        
        let isType (key: EntityGroupKey) (entity:Entity) =
            let group = entityProvider.EntityIds(key) |> Async.RunSynchronously
            match group with 
            | Some grp -> grp.Contains(entity.Id)
            | None -> false
        
        let isEcm = isType EntityGroupKey.Ecm
        let isPlex = isType EntityGroupKey.Plex
        let isSkillInjector = isType EntityGroupKey.SkillInjector
        let isPod = isType EntityGroupKey.Capsule
        
        member this.Tag(kill: Kill)=            
            let tags = [                            
                            Tagging.isCorpKill 0.5 corpId;
                            Tagging.isCorpLoss corpId;
                            Tagging.hasPlex isPlex;
                            Tagging.hasSkillInjector isSkillInjector;
                            Tagging.hasEcm isEcm;               
                            Tagging.isPod isPod;
                            Tagging.isExpensive;
                            Tagging.isSpendy;
                            Tagging.isCheap;                            
                        ]
                        |> Seq.map (fun f -> f kill)
                        |> Seq.mapSomes
                        |> List.ofSeq
                        |> List.append kill.Tags
            
            {kill with Tags = tags}