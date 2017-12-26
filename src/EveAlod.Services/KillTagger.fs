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
            // TODO: cleanup. Optimise...
            let tags = [                            
                            Tagging.isCorpKill corpId kill;
                            Tagging.isCorpLoss corpId kill;
                            Tagging.isExpensive kill;
                            Tagging.isSpendy kill;
                            Tagging.isCheap kill;
                            Tagging.isPod isPod kill;
                            Tagging.hasPlex isPlex kill;
                            Tagging.hasSkillInjector isSkillInjector kill;
                            Tagging.hasEcm isEcm kill;                            
                        ]
                        |> Tagging.toTags
                        |> List.append kill.Tags
            
            {kill with Tags = tags}