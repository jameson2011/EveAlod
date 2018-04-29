namespace EveAlod.Services
    
    open EveAlod.Data

    module Scoring=
        
        let private tagScore (tag: KillTag) =
            match tag with
            | CorpKill -> 100.
            | CorpLoss -> 100.
            | PlexInHold | SkillInjectorInHold -> 20.
            | Expensive -> 100.
            | Spendy -> 40.
            | NormalPrice -> 10.
            | ZeroValue | Cheap | NpcKill-> 0.1
            | PlayerKill -> 2.
            | Pod  -> 15.
            | EcmFitted -> 15.
            | AwoxKill -> 15.
            | SoloKill -> 10.
            | FailFit -> 20.            
            | MissingLows | MissingMids | MissingRigs -> 10.
            | WideMarginShipType -> 10.
            | NarrowMarginShipType -> 1.
            | KillTag.Lowsec | KillTag.Highsec | KillTag.Nullsec | KillTag.Wormhole -> 0.1
            | Gatecamp -> 0.1
            

        let private tagsScore = Seq.map tagScore >> Seq.sum
            
        let score (km: Kill) = 
            km.Tags |> Seq.distinct |> tagsScore
            

