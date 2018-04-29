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
            | NormalPrice -> 10.
            | NpcKill-> 0.1
            | PlayerKill -> 2.
            | Pod  -> 15.
            | EcmFitted -> 15.
            | AwoxKill -> 10.
            | SoloKill -> 10.
            | FailFit -> 20.            
            | MissingLows | MissingMids | MissingRigs -> 5.
            | WideMarginShipType -> 10.
            | NarrowMarginShipType -> 1.
            | KillTag.Lowsec | KillTag.Highsec | KillTag.Nullsec | KillTag.Wormhole -> 0.1
            | Gatecamp -> 0.1
            
        let isNullifier = function
            | ZeroValue -> true
            | _ -> false
                
        let private tagsScore tags =
            match tags |> Seq.exists isNullifier with
            | true -> 0.
            | _ -> tags |> Seq.map tagScore |> Seq.sum

        let score (km: Kill) = 
            km.Tags |> Seq.distinct |> tagsScore
            

