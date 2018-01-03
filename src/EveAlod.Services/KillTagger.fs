namespace EveAlod.Services

    open EveAlod.Entities

    type KillTagger(entityProvider: IStaticEntityProvider, corpId: string)=
        
        let isType (key: EntityGroupKey) (entity:Entity) =
            entityProvider.EntityIds(key).Contains(entity.Id)
        
        let isEcm = isType EntityGroupKey.Ecm
        let isPlex = isType EntityGroupKey.Plex
        let isSkillInjector = isType EntityGroupKey.SkillInjector
        let isPod = isType EntityGroupKey.Capsule
        
        member this.Tag(kill: Kill)=            
            let tags = [                            
                            Tagging.isCorpKill 0.5 corpId;
                            Tagging.isCorpLoss corpId;
                            Tagging.isAwox;
                            Tagging.hasPlex isPlex;
                            Tagging.hasSkillInjector isSkillInjector;
                            Tagging.hasEcm isEcm;               
                            Tagging.isPod isPod;
                            Tagging.isExpensive;
                            Tagging.isSpendy;
                            Tagging.isCheap;                            
                        ]
                        |> Seq.map (fun f -> f kill)
                        |> Tagging.toTags
                        |> List.append kill.Tags
            
            {kill with Tags = tags}