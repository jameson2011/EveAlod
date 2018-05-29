namespace EveAlod.Services
    
    open EveAlod.Data

    module Scoring=
        
        
        let private tagScore (tag: KillTag) =
            match tag with
            | ZeroValue -> 0.
            | Cheap  -> 0.1
            | CorpKill -> 100.
            | CorpLoss -> 100.
            | PlexInHold | SkillInjectorInHold -> 20.
            | Expensive -> 100.
            | Spendy -> 40.
            | AboveNormalPrice -> 15.
            | NormalPrice -> 10.
            | NpcKill-> 0.1
            | PlayerKill -> 2.
            | Pod  -> 15.
            | EcmFitted -> 10.
            | Stabbed -> 10.
            | AwoxKill -> 10.
            | SoloKill -> 10.
            | FailFit -> 20.            
            | MissingLows | MissingMids | NoRigs  -> 5.
            | MixedTank -> 10.
            | WideMarginShipType -> 10.
            | NarrowMarginShipType -> 1.
            | KillTag.Lowsec | KillTag.Highsec | KillTag.Nullsec | KillTag.Wormhole | KillTag.Abyssal -> 0.1
            | Gatecamp -> 0.1
            | Industrial -> 0.1

        let private nullifiers = [ ZeroValue ; Cheap ] |> Set.ofSeq
        let private antiNullifiers = [ CorpKill; CorpLoss ] |> Set.ofSeq
        
        let isNullified tags = 
            let nulls = tags |> Set.intersect nullifiers 
            let antinulls = tags |> Set.intersect antiNullifiers

            match nulls |> Set.isEmpty, antinulls |> Set.isEmpty with
            | false, true -> true
            | _, _-> false
            
        let private tagsScore tags =
            match isNullified tags with 
            | true -> 0.
            | _ -> tags |> Seq.map tagScore |> Seq.sum

        let score (km: Kill) = 
            km.Tags |> Set.ofSeq |> tagsScore
            

