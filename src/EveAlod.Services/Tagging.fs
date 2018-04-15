namespace EveAlod.Services

    open EveAlod.Common
    open EveAlod.Common.Combinators
    open EveAlod.Data

    module Tagging=

        let private expensiveLimit =    10000000000.
        let private spendyLimit =       1000000000.
        let private spendy2Limit =      500000000.
        let private cheapLimit =        10000000.

        let private priceTags = [ KillTag.Spendy; KillTag.Expensive; KillTag.Cheap; KillTag.ZeroValue] |> Set.ofSeq        

        let private locationSecurity (kill: Kill)=
            match kill.Location with
            | Some l -> Some l.SolarSystem.Security
            | _ -> None


        let tagPresent (tag: KillTag) kill=        
            kill.Tags |> Seq.exists (fun t -> t = tag) 
            
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

        let isTotalValueValuationOver (limit: float) (km: Kill)=
            match km.TotalValueValuation with
            | Some v -> v > limit
            | _ -> false

        let isShipTypeSpreadOver (limit: float) (km: Kill) =
            match km.TotalValueSpread with
            | Some v -> v >= limit
            | _ -> false

        let hasItemsInCargo (pred: CargoItem -> bool) (km: Kill) =
            km.Cargo
            |> Seq.exists pred
        
        let hasItemsFitted (pred: Entity -> bool) (km: Kill) =
            km.Fittings
            |> Seq.map (fun e -> e.Item)
            |> Seq.exists pred
            

        let tagOnTrue (tag: KillTag) (pred: Kill -> bool) (km: Kill)=
            match pred km with
            | true -> Some tag
            | _ -> None
          
        let hasItemInHold tag (pred: CargoItem -> bool) =
            (tagOnTrue tag) (hasItemsInCargo pred)
            
        let hasItemFitted tag (pred: Entity -> bool) =
            (tagOnTrue tag) (hasItemsFitted pred)
            
        let isPod isPod = 
            (tagOnTrue KillTag.Pod) (isVictimInPod isPod)

        let isPlayer =
            (tagOnTrue KillTag.PlayerKill) (not << tagPresent KillTag.NpcKill)

        let isExpensive =
            (tagOnTrue KillTag.Expensive) (isTotalValueOver expensiveLimit)
            
        let isSpendy =
            (tagOnTrue KillTag.Spendy) ((isTotalValueOver spendyLimit) <&&> (isTotalValueUnder expensiveLimit))
        
        let isSpendyWithinLimits valuationLimit=
            (tagOnTrue KillTag.Spendy) ((isTotalValueOver spendy2Limit) <&&> (isTotalValueValuationOver valuationLimit) <&&> (isTotalValueUnder expensiveLimit))
        
        let isShipTypeWideMargin spreadLimit=
            (tagOnTrue KillTag.WideMarginShipType) (isShipTypeSpreadOver spreadLimit)

        let isShipTypeNarrowMargin spreadLimit=
            (tagOnTrue KillTag.NarrowMarginShipType) (not << isShipTypeSpreadOver spreadLimit)

        let isZeroValue = 
            (tagOnTrue KillTag.ZeroValue) (isTotalValueUnder cheapLimit)

        let isCheap = 
            (tagOnTrue KillTag.Cheap) ((isTotalValueOver cheapLimit) <&&>
                                        (isTotalValueUnder spendyLimit))

        let isCorpLoss corpId =
            (tagOnTrue KillTag.CorpLoss) (isVictimInCorp corpId)
        
        let isCorpKill minimum corpId =
            let p1 = (areAttackersInSameCorp corpId) 
            let p2 = (isMostlyCorpKill minimum corpId)            
            (tagOnTrue KillTag.CorpKill) (p1 <||> p2)
                 
        let locationTag (kill: Kill)=
            match locationSecurity kill with
            | Some Lowsec -> Some KillTag.Lowsec
            | Some Nullsec -> Some KillTag.Nullsec
            | Some Highsec -> Some KillTag.Highsec
            | Some Wormhole -> Some KillTag.Wormhole
            | _ -> None


        let normalPrice (tags: seq<KillTag>) =             
            let matches = tags |> Seq.filter (fun t -> priceTags |> Set.contains t)
            match Seq.isEmpty matches with            
            | true -> Some KillTag.NormalPrice
            | _ -> None


