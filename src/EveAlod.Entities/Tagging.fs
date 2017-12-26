namespace EveAlod.Entities

    module Tagging=
                
        let toTags (tags: seq<KillTag option>)= 
            tags
            |> Seq.filter (fun tag -> tag.IsSome)
            |> Seq.map (fun tag -> tag.Value)
            |> List.ofSeq
                        
        let getCorpId (character: Character option)=
            character |> Option.bind (fun c -> c.Corp 
                                                |> Option.map (fun c -> c.Id))

        let getAttackerCorpId(attacker: Attacker)=
            attacker.Char |> getCorpId
        
        let isVictimInPod (km: Kill) = 
            match km.VictimShip with
            | Some e -> EntityTypes.isPod e
            | _ -> false

        let isVictimInCorp (corpId: string) (km: Kill) = 
            match km.Victim with
            | Some v -> match v.Corp with
                         | Some c when c.Id = corpId -> true
                         | _ -> false
            | _ -> false
        
        let areAttackersInSameCorp (corpId: string) (km: Kill)=
            let attackerCorpIds = km.Attackers
                                    |> Seq.map getAttackerCorpId
                                    |> Seq.filter (fun s -> s.IsSome)                                    
                                    |> Seq.map (fun c -> c.Value)
                                    |> Set.ofSeq

            attackerCorpIds.Count = 1 &&
                (attackerCorpIds |> Seq.item 0) = corpId

        let isTotalValueOver (value: float) (km: Kill)=
            match km.TotalValue with
            | x when x > value -> true
            | _ -> false

        let isTotalValueUnder (value: float) (km: Kill)=
            match km.TotalValue with
            | x when x <= value -> true
            | _ -> false


        let hasItemsInCargo (pred: Entity -> bool) (km: Kill) =
            km.Cargo
            |> Seq.map (fun e -> e.Item)
            |> Seq.exists pred
            
        let tagOnTrue (tag: KillTag) (pred: Kill -> bool) (km: Kill)=
            match pred km with
            | true -> Some tag
            | _ -> None
            
        let hasPlex =            
            (tagOnTrue KillTag.PlexInHold) (hasItemsInCargo EntityTypes.isPlex)

        let hasSkillInjector =
            (tagOnTrue KillTag.SkillInjectorInHold) (hasItemsInCargo EntityTypes.isSkillInjector)
            
        let hasEcm =
            (tagOnTrue KillTag.Ecm) (hasItemsInCargo EntityTypes.isEcm)
            
        let isPod = 
            (tagOnTrue KillTag.Pod) (isVictimInPod)
            
        let isExpensive =
            (tagOnTrue KillTag.Expensive) (isTotalValueOver 10000000000.)
            
        let isSpendy =
            (tagOnTrue KillTag.Spendy) (isTotalValueOver 500000000.)
                        
        let isCheap = 
            (tagOnTrue KillTag.Cheap) (isTotalValueUnder 10000000.)

        let isCorpLoss corpId =
            (tagOnTrue KillTag.CorpLoss) (isVictimInCorp corpId)
                    
        let isCorpKill (corpId) =
            (tagOnTrue KillTag.CorpKill) (areAttackersInSameCorp corpId)
                                        
        let private tagTexts = 
            [ 
                KillTag.CorpLoss, [| "CORPIE DOWN"; "RIP" |];
                KillTag.CorpKill, [| "GREAT VICTORY"; "GLORIOUS VICTORY" |];
                KillTag.Pod, [| "Oops"; "Someone should have bought pod insurance" |];
                KillTag.Expensive, [| "DERP"; "Oh dear, how sad, never mind" |];
                KillTag.Spendy, [| "Oops"; |];
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

            
