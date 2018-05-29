namespace EveAlod.Services

open EveAlod.Common
open EveAlod.Data

module Tagging=

    let private expensiveLimit =    10000000000.
    let private spendyLimit =       1000000000.
    let private spendy2Limit =      500000000.
    let private cheapLimit =        10000000.

    let private priceTags = [ KillTag.Spendy; KillTag.Expensive; KillTag.NormalPrice; KillTag.AboveNormalPrice; KillTag.Cheap; KillTag.ZeroValue] |> Set.ofSeq        

    let private locationSecurity (kill: Kill)=
        match kill.Location with
        | Some l -> Some l.SolarSystem.Security
        | _ -> None


    let tagPresent (tag: KillTag) kill=        
        kill.Tags |> Seq.exists (fun t -> t = tag) 
            
    let getAttackerCorpsDamage(km: Kill)=
        km.Attackers
            |> Seq.map (fun a -> (a.Damage, KillTransforms.getAttackerCorpId a))
            |> Seq.filter (fun (_,s) -> s.IsSome)                                    
            |> Seq.map (fun (dmg,corpId) -> (dmg, corpId.Value) )                        
            |> Seq.groupBy (fun (_,corpId) -> corpId)
            |> Seq.map (fun (corpId, xs) -> (corpId, xs |> Seq.sumBy (fun (dmg,_) -> dmg) ))
            |> Map.ofSeq
         
    let victimHasMissingSlots (location: ItemLocation) (km: Kill)=
        match km.VictimShip |> Option.bind EntityTransforms.itemType, ShipTransforms.attrType location with
        | Some s, Some at -> 
                    let avail = ShipTransforms.shipTypeSlot at s
                    let fitted =  km.Fittings |> ShipTransforms.fittedItemTypes location |> Seq.length
                    fitted < avail
        | _ -> false

    let victimHasNothingInSlots (location: ItemLocation) (km: Kill)=
        match km.VictimShip |> Option.bind EntityTransforms.itemType, ShipTransforms.attrType location with
        | Some s, Some at -> 
                    let avail = ShipTransforms.shipTypeSlot at s
                    let noneFitted =  km.Fittings |> ShipTransforms.fittedItemTypes location |> Seq.isEmpty
                    avail > 0 && (noneFitted)
        | _ -> false

    let isVictimInIndustrial (kill: Kill) =
        match kill.VictimShip with
        | Some e -> ShipTransforms.isIndustrialShip e
        | _ -> false

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
        let attackerCorpIds = KillTransforms.getAttackerCorpIds km
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
        
    let hasItemsFitted (pred: CargoItem -> bool) (km: Kill) =
        km.Fittings
        |> Seq.exists pred
            

    let tagOnTrue (tag: KillTag) (pred: Kill -> bool) (km: Kill)=
        match pred km with
        | true -> Some tag
        | _ -> None
          
    let hasItemInHold tag (pred: CargoItem -> bool) =
        (tagOnTrue tag) (hasItemsInCargo pred)
            
    let hasItemFitted tag (pred: CargoItem -> bool) =
        (tagOnTrue tag) (hasItemsFitted pred)
            
    let isPod isPod = 
        (tagOnTrue KillTag.Pod) (isVictimInPod isPod)

    let missingLows = 
        (tagOnTrue KillTag.MissingLows) (victimHasMissingSlots ItemLocation.LowSlot)

    let missingMids = 
        (tagOnTrue KillTag.MissingMids) (victimHasMissingSlots ItemLocation.MidSlot)
            
    let noRigs = 
        (tagOnTrue KillTag.NoRigs) (victimHasNothingInSlots ItemLocation.RigSlot)
            
    let hasMixedTank = 
        let items k = k.Fittings |> Seq.map (fun c -> c.Item) |> Seq.cache
        let isArmour =  Seq.exists ShipTransforms.isArmourMod
        let isShield = Seq.exists ShipTransforms.isShieldMod
            
        (tagOnTrue KillTag.MixedTank) (items >> (isArmour <&&> isShield))

    let isIndustrial =
        (tagOnTrue KillTag.Industrial) (isVictimInIndustrial)

    let isPlayer =
        (tagOnTrue KillTag.PlayerKill) (not << tagPresent KillTag.NpcKill)

    let isExpensive =
        (tagOnTrue KillTag.Expensive) (isTotalValueOver expensiveLimit)
          
    let isSpendyWithinLimits valuationLimit=
        (tagOnTrue KillTag.Spendy) ((isTotalValueOver spendy2Limit) <&&> (isTotalValueValuationOver valuationLimit) <&&> (isTotalValueUnder expensiveLimit))
        
    let isAboveNormalWithinLowerLimits valuationLimit=
        (tagOnTrue KillTag.AboveNormalPrice) ((isTotalValueOver spendy2Limit) <&&> (isTotalValueValuationOver valuationLimit) <&&> (isTotalValueUnder expensiveLimit))
        
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
        | Some Abyssal -> Some KillTag.Abyssal
        | _ -> None


    let normalPrice (tags: seq<KillTag>) =             
        let matches = tags |> Seq.filter (fun t -> priceTags |> Set.contains t)
        match Seq.isEmpty matches with            
        | true -> Some KillTag.NormalPrice
        | _ -> None

        
    let isGateCamp  (kill: Kill) =
        let isGate = function | IronSde.Stargate _ -> true | _ -> false
                
        let distance = kill.Location |> Option.bind (fun l -> match l.Distance, l.Celestial with
                                                                | Some d, Some c when (isGate c) -> Some (IronSde.Units.metresToKm d)
                                                                | _ -> None)
        let isCamp = distance |> Option.map (fun d -> d < 250.0<IronSde.km> &&
                                                        (List.length kill.Attackers) >= 3)

        match isCamp with
        | Some true -> Some KillTag.Gatecamp
        | _ -> None            
            
