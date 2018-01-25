namespace EveAlod.Services

    open EveAlod.Common
    open EveAlod.Common.Combinators
    open EveAlod.Data

    module Tagging=
                
        let getCorpId (character: Character option)=
            character |> Option.bind (fun c -> c.Corp 
                                                |> Option.map (fun c -> c.Id))

        let getAttackerCorpId(attacker: Attacker)=
            attacker.Char |> getCorpId
        
        let getAttackerCorpIds(km: Kill)=
            km.Attackers
            |> Seq.map getAttackerCorpId
            |> Seq.mapSomes
            |> Set.ofSeq

        let getAttackerCorpsDamage(km: Kill)=
            let r = km.Attackers
                        |> Seq.map (fun a -> (a.Damage, getAttackerCorpId a))
                        |> Seq.filter (fun (_,s) -> s.IsSome)                                    
                        |> Seq.map (fun (dmg,corpId) -> (dmg, corpId.Value) )                        
                        |> Seq.groupBy (fun (_,corpId) -> corpId)
                        |> Seq.map (fun (corpId, xs) -> (corpId, xs |> Seq.sumBy (fun (dmg,_) -> dmg) ))
                        |> Map.ofSeq
            r

        let isVictimInPod (isPod: Entity -> bool) (km: Kill) = 
            match km.VictimShip with
            | Some e -> isPod e
            | _ -> false

        let isVictimInCorp (corpId: string) (km: Kill) = 
            match km.Victim with
            | Some v -> match v.Corp with
                         | Some c when c.Id = corpId -> true
                         | _ -> false
            | _ -> false
        
        let areAttackersInSameCorp (corpId: string) (km: Kill)=
            let attackerCorpIds = getAttackerCorpIds km
            attackerCorpIds.Count = 1 &&
                (attackerCorpIds |> Seq.item 0) = corpId
                
        let isMostlyCorpKill (minimum: float) (corpId: string) (km: Kill)=
            let corpDmgSplits = getAttackerCorpsDamage km
            let totalCorpDmg = corpDmgSplits 
                                |> Seq.sumBy (fun kvp -> kvp.Value)
            match corpDmgSplits |> Map.tryFind corpId with
            | Some dmg ->
                (float dmg / float totalCorpDmg) >= minimum
            | _ -> false
                   

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
        
        let hasItemsFitted (pred: Entity -> bool) (km: Kill) =
            km.Fittings
            |> Seq.map (fun e -> e.Item)
            |> Seq.exists pred
            

        let tagOnTrue (tag: KillTag) (pred: Kill -> bool) (km: Kill)=
            match pred km with
            | true -> Some tag
            | _ -> None
            
        let hasPlex (pred: Entity -> bool) =
            (tagOnTrue KillTag.PlexInHold) (hasItemsInCargo pred)

        let hasSkillInjector (pred: Entity -> bool) =
            (tagOnTrue KillTag.SkillInjectorInHold) (hasItemsInCargo pred)
            
        let hasEcm (pred: Entity -> bool) =
            (tagOnTrue KillTag.Ecm) (hasItemsFitted pred)
            
        let isPod isPod = 
            (tagOnTrue KillTag.Pod) (isVictimInPod isPod)
            
        let isExpensive =
            (tagOnTrue KillTag.Expensive) (isTotalValueOver 10000000000.)
            
        let isSpendy =
            (tagOnTrue KillTag.Spendy) ((isTotalValueOver 1000000000.) <&&> (isTotalValueUnder 10000000000.))
                        
        let isCheap = 
            (tagOnTrue KillTag.Cheap) (isTotalValueUnder 10000000.)

        let isCorpLoss corpId =
            (tagOnTrue KillTag.CorpLoss) (isVictimInCorp corpId)
        
            

        let isCorpKill minimum corpId =
            let p1 = (areAttackersInSameCorp corpId) 
            let p2 = (isMostlyCorpKill minimum corpId)            
            (tagOnTrue KillTag.CorpKill) (p1 <||> p2)
                      

        let private tagTexts = 
            [ 
                KillTag.CorpLoss, [| "CORPIE DOWN"; "RIP" |];
                KillTag.CorpKill, [| "GREAT VICTORY"; "GLORIOUS VICTORY" |];
                KillTag.Pod, [| "Someone should have bought pod insurance"; "Someone needs his implants back"; |];
                KillTag.PlexInHold, [| "Plex in hold!"; "BWAHAHAHAHAHA!"; "Plex vaults - they exist"; "RMT DOWN" |];
                KillTag.SkillInjectorInHold, [| "Skill injector in hold!"; "FFS"; "No comment needed" |];
                KillTag.Awox, [| "Ooooh... Awox"; "Should have checked his API"; "Didn't like that corp anyway" |];
                KillTag.Ecm, [| "ECM is illegal"; "Doing God's work" |];
                KillTag.Expensive, [| "DERP"; "Oh dear, how sad, never mind"; "Someone's gonna be crying" |];
                KillTag.Spendy, [| "Oops"; "DEPLOY CREDIT CARD" |];
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

            
