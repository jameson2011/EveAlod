namespace EveAlod.Services

    open EveAlod.Entities

    type KillTagger(entityProvider: IStaticEntityProvider)=
        
        //

        member this.Tag(k: Kill)=
            // TODO: 
            k