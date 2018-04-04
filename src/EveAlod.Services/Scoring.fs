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
            | ZeroValue -> 1.
            | Cheap -> 1.
            | PlayerKill -> 2.
            | NpcKill -> 1.
            | Pod  -> 20.
            | EcmFitted -> 20.
            | AwoxKill -> 20.
            | SoloKill -> 10.
            | FailFit -> 20.            
            | WideMarginShipType -> 10.
            | NarrowMarginShipType -> 1.
            | KillTag.Lowsec | KillTag.Highsec | KillTag.Nullsec | KillTag.Wormhole -> 0.1

        let private tagsScore = Seq.map tagScore >> Seq.sum
            
        let score (km: Kill) = 
            km.Tags |> Seq.distinct |> tagsScore
            

