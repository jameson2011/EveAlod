namespace EveAlod.Services

    open EveAlod.Entities

    type KillTagger(entityProvider: IStaticEntityProvider, corpId: string)=
        
        //
        let ecmEntityIds = entityProvider.EntityIds(EntityGroupKey.Ecm)
        let plexEntityIds = entityProvider.EntityIds(EntityGroupKey.Plex)
        let skillInjectorEntityIds = entityProvider.EntityIds(EntityGroupKey.SkillInjector)
        let podEntityIds = entityProvider.EntityIds(EntityGroupKey.Capsule)
        
        member this.Tag(kill: Kill)=
            // TODO: 
            let tags = [                            
                            Tagging.isCorpKill corpId kill;
                            Tagging.isCorpLoss corpId kill;
                            Tagging.isPod kill;
                            Tagging.hasPlex kill;
                            Tagging.hasSkillInjector kill;
                            Tagging.hasEcm kill;
                            Tagging.isExpensive kill;
                            Tagging.isSpendy kill;
                            Tagging.isCheap kill;
                        ]
                        |> Tagging.toTags
                        |> List.append kill.Tags
            
            {kill with Tags = tags}