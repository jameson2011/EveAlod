namespace EveAlod.Entities

    module Tagging=
        
        
        let private toTags (tags: seq<KillTag option>)= 
            tags
            |> Seq.filter (fun tag -> tag.IsSome)
            |> Seq.map (fun tag -> tag.Value)
            |> List.ofSeq

            
        let private getCorpId (character: Character option)=
            character |> Option.bind (fun c -> c.Corp 
                                                |> Option.map (fun c -> c.Id))

        let private getAttackerCorpId (attacker: Attacker)=
            attacker.Char |> getCorpId
            
        let private hasItemsInCargo (pred: Entity -> bool) (km: Kill) =
            km.Cargo
            |> Seq.map (fun e -> e.Item)
            |> Seq.exists pred
            
        let hasPlex (km: Kill)=
            match (hasItemsInCargo EntityTypes.isPlex km) with
            | true -> Some KillTag.PlexInHold
            | _ -> None

        let hasSkillInjector (km: Kill)=
            match (hasItemsInCargo EntityTypes.isSkillInjector km) with
            | true -> Some KillTag.SkillInjectorInHold
            | _ -> None
        
        let hasEcm (km: Kill)=
            match (hasItemsInCargo EntityTypes.isEcm km) with
            | true -> Some KillTag.Ecm
            | _ -> None

        let isPod (km:Kill )= 
            let isPod = match km.VictimShip with
                        | Some e -> EntityTypes.isPod e
                        | _ -> false
            match isPod with
            | true -> Some KillTag.Pod
            | _ -> None

        let isExpensive (km: Kill)=
            match km.TotalValue with
            | x when x > 10000000000. -> Some KillTag.Expensive
            | _ -> None
        
        let isSpendy (km: Kill)=
            match km.TotalValue with
            | x when x > 500000000. -> Some KillTag.Spendy
            | _ -> None
            
        let isCorpLoss corpId (km:Kill) =
            match km.Victim with
            | Some v -> match v.Corp with
                         | Some c when c.Id = corpId -> Some (KillTag.CorpLoss)
                         | _ -> None
            | _ -> None

        
        let isCorpKill (corpId) (km: Kill) =
            
            let attackerCorpIds = km.Attackers
                                    |> Seq.map getAttackerCorpId
                                    |> Seq.filter (fun s -> s.IsSome)                                    
                                    |> Seq.map (fun c -> c.Value)
                                    |> Set.ofSeq

            if attackerCorpIds.Count = 1 &&
                (attackerCorpIds |> Seq.item 0) = corpId then
                Some (KillTag.CorpKill)
            else
                None
                

        let tag (corpId: string) km = 
            let tags = [                            
                            isCorpKill corpId km;
                            isCorpLoss corpId km;
                            isPod km;
                            hasPlex km;
                            hasSkillInjector km;
                            hasEcm km;
                            isExpensive km;
                            isSpendy km;
                        ]
                        |> toTags
                        |> List.append km.Tags
            
            {km with Tags = tags}

        
        let private tagTexts = 
            [ 
                KillTag.CorpLoss, [| "CORPIE DOWN"; "RIP" |];
                KillTag.CorpKill, [| "GREAT VICTORY" |];
                KillTag.Pod, [| "Oops"; "Someone should have bought pod insurance" |];
                KillTag.Expensive, [| "DERP"; "Oh dear, how sad, never mind" |];
                KillTag.PlexInHold, [| "BWAHAHAHAHAHA!"; "Plex vaults - they exist"; "RMT DOWN" |];
                KillTag.SkillInjectorInHold, [| "FFS"; "No comment needed" |];
                KillTag.Awox, [| "Didn't like that corp anyway" |];
                KillTag.Ecm, [| "ECM is illegal"; "Doing God's work" |];
            ]
            |> Map.ofSeq


        let getTagText (rnd: System.Random) tag =            
            match tagTexts |> Map.tryFind tag with
            | Some txts -> 
                let idx = rnd.Next(0, txts.Length)
                txts.[idx]
            | _ -> ""
        
        
        let getTagsText (text: KillTag -> string) (tags: KillTag list) =
            let rec getTexts tags = 
                match tags with
                | [] -> ""
                | head::tail -> 
                    match (text head) with
                    | "" -> getTexts tail 
                    | txt -> txt
            getTexts tags

            
