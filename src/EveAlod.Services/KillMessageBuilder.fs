namespace EveAlod.Services

    open System
    open EveAlod.Data

    type KillMessageBuilder(staticEntities: IStaticEntityProvider, corpId: string)=
        
        let rnd = new System.Random()
        let getTagText = (Tagging.getTagText rnd) |> Tagging.getTagsText 


        let isCorpie (corpId) (attacker: Attacker): bool=
            let attackerCorpId = attacker.Char 
                                    |> Option.bind (fun c -> c.Corp)
                                    |> Option.map (fun c -> c.Id)
            match attackerCorpId with
            | Some id -> id = corpId
            | _ -> false
                        
        let getCharacters (chars: seq<Character>) =
            let result = chars 
                            |> Seq.map (fun c -> c.Char.Id)
                            |> Seq.map staticEntities.Character
                            |> Async.Parallel
                            |> Async.RunSynchronously
                            |> Seq.filter (fun c -> c.IsSome)
                            |> Seq.map (fun c -> c.Value)
                            |> List.ofSeq
            result

        let getKillCorpCharacters corpId km =
            km.Attackers 
            |> Seq.filter (isCorpie corpId)
            |> Seq.map (fun a -> a.Char.Value)
            |> List.ofSeq
            
        let isTagged (tag: KillTag) km =
            km.Tags
            |> Seq.filter (fun t -> t = tag)

        let getMsg km = 
            let tags = getTagText km.Tags 
            let uri = km.ZkbUri
            let value = km.TotalValue
            
            String.Format("{0} {1} {2:N} ISK", tags, uri, value).Trim()
            
        
        member this.CreateMessage(kill) =             
            getMsg kill