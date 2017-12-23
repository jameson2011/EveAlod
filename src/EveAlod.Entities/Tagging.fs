namespace EveAlod.Entities

    module Tagging=
        //
        let private toTags (tags: seq<KillTag option>)= 
            tags
            |> Seq.filter (fun tag -> tag.IsSome)
            |> Seq.map (fun tag -> tag.Value)
            |> List.ofSeq

        let isPod (km:Kill )= 
            let isPod = match km.VictimShip with
                        | Some s -> s.Id = "670"
                        | _ -> false
            match isPod with
            | true -> Some (KillTag.Pod "")
            | _ -> None

        let isExpensive (km: Kill)=
            match km.TotalValue with
            | x when x > 10000000000. -> Some (KillTag.Expensive "")
            | _ -> None
            
        let isCorpLoss (km:Kill) =
            match km.Victim with
            | Some v -> match v.Corp with
                         | Some c when c.Id = "423280073" -> Some (KillTag.CorpLoss "")
                         | _ -> None
            | _ -> None

        let private getCorpId (character: Character option)=
            character |> Option.bind (fun c -> c.Corp 
                                                |> Option.map (fun c -> c.Id))

        let private getAttackerCorpId (attacker: Attacker)=
            attacker.Char |> getCorpId

        
        let isCorpKill (km: Kill) =
            
            let attackerCorpIds = km.Attackers
                                    |> Seq.map getAttackerCorpId
                                    |> Seq.filter (fun s -> s.IsSome)                                    
                                    |> Seq.map (fun c -> c.Value)
                                    |> Set.ofSeq

            if attackerCorpIds.Count = 1 &&
                (attackerCorpIds |> Seq.item 0) = "423280073" then
                Some (KillTag.CorpKill "")
            else
                None
                

        let tag km = 
            let tags = [
                            isPod km;
                            isExpensive km;
                            isCorpKill km;
                            isCorpLoss km;
                        ]
                        |> toTags
                        |> List.append km.Tags
            
            {km with Tags = tags}
