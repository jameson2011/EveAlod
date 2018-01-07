namespace EveAlod.Services

    open System
    open EveAlod.Data

    type KillMessageBuilder(staticEntity: IStaticEntityProvider)=
        
        let rnd = new System.Random()
        let getTagText = (Tagging.getTagText rnd) |> Tagging.getTagsText 

        
        let getMsg km = 
            let tags = getTagText km.Tags 
            let uri = km.ZkbUri
            let value = km.TotalValue
            
            let result = String.Format("{0} {1} {2:N} ISK", tags, uri, value).Trim()

            result
        
        member this.CreateMessage(kill) =             
            getMsg kill