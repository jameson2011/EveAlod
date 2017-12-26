namespace EveAlod.Entities
    module Scoring=
        
        let private tagScore (tag: KillTag) =
            match tag with
            | CorpKill -> 100.
            | CorpLoss -> 50.
            | PlexInHold | SkillInjectorInHold -> 100.
            | Expensive -> 100.
            | Spendy -> 40.
            | Cheap -> -50.
            | Pod  -> 10.
            | Ecm -> 40.
            | _ -> 0.

        let private tagsScore (tags: KillTag list) =
            tags |> Seq.map tagScore |> Seq.sum
            

        let score (km: Kill) = 
            let score = tagsScore km.Tags
            score

