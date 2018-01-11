namespace EveAlod.Services
    
    open EveAlod.Data

    module Scoring=
        
        let private tagScore (tag: KillTag) =
            match tag with
            | CorpKill -> 100.
            | CorpLoss -> 50.
            | PlexInHold | SkillInjectorInHold -> 100.
            | Expensive -> 100.
            | Spendy -> 40.
            | Cheap -> 1.
            | Npc -> 1.
            | Pod  -> 10.
            | Ecm -> 10.
            | Awox -> 10.
            | Solo -> 10.
            | FailFit -> 20.            

        let private tagsScore = Seq.map tagScore >> Seq.sum
            
        let score (km: Kill) = 
            km.Tags |> Seq.distinct |> tagsScore
            

