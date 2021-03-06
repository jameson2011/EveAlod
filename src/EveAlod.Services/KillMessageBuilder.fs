﻿namespace EveAlod.Services

    open System
    open EveAlod.Data
    open EveAlod.Common.Strings

    type KillMessageBuilder(staticEntities: StaticDataActor, corpId: string)=
                
        let getLocationText (location: Location option) =
            match location with
            | Some l ->     let cel = l.Celestial |> Option.map IronSde.Celestials.name |> Option.defaultValue ""
                            let r = l.Region.Name
                            let c = l.Constellation.Name
                            let s = l.SolarSystem.Name
                            sprintf "%s - %s - %s - %s" cel s c r 
            | _ -> ""

        let getTagText =  Commentary.getText |> Commentary.getTagsText 
         
        let composeCharNames (characters: Character list) = 
            let names = characters 
                        |> Seq.map (fun c -> c.Char.Name)
                        |> Seq.filter (String.IsNullOrWhiteSpace >> not)
                        |> List.ofSeq

            prettyConcat names
            

        let isCorpie (corpId) (attacker: Attacker): bool=
            let attackerCorpId = attacker.Char 
                                    |> Option.bind (fun c -> c.Corp)
                                    |> Option.map (fun c -> c.Id)
            match attackerCorpId with
            | Some id -> id = corpId
            | _ -> false
                        
        let getCharacters (chars: seq<Character>) =            
            let queries =   chars 
                            |> Seq.map (fun c -> c.Char.Id |> staticEntities.Character)
                            |> Async.Parallel
                            |> Async.RunSynchronously

            let result = queries                            
                            |> Array.filter (fun c -> c.IsSome)
                            |> Seq.map (fun c -> c.Value)
                            |> List.ofSeq
            result

        let getKillCorpCharacters corpId km =
            km.Attackers 
            |> Seq.sortByDescending (fun a -> a.Damage)
            |> Seq.filter (isCorpie corpId)
            |> Seq.filter (fun a -> a.Char.IsSome)
            |> Seq.map (fun a -> a.Char.Value)
            |> List.ofSeq
            
        let getKillAttackerNames km =
            km 
            |> getKillCorpCharacters corpId 
            |> (Seq.truncate 3)
            |> getCharacters
            |> composeCharNames 

        let isTagged (tag: KillTag) km =
            km.Tags |> Seq.exists (fun t -> t = tag)
            
        let getMsg km = 
            let location = getLocationText km.Location
            let tags = getTagText km.Tags 
            let uri = km.ZkbUri
            let value = km.TotalValue
            
            let charNames = match (isTagged KillTag.CorpKill km) with       
                            | true -> getKillAttackerNames km
                            | _ -> ""
                            
            if charNames.Length > 0 then
                String.Format("{0} by {1} {2} {3:N} ISK {4}", tags, charNames, uri, value, location).Trim()
            else
                String.Format("{0} {1} {2:N} ISK {3}", tags, uri, value, location).Trim()
            
        
        member this.CreateMessage(kill) =             
            getMsg kill
