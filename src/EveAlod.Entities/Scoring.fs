namespace EveAlod.Entities
    module Scoring=
        
        let private tagScore (tag: KillTag) =
            match tag with
            | CorpKill _ -> 100.
            | CorpLoss _ -> 50.
            | PlexInHold _ | SkillInjectorInHold _ -> 100.
            | Expensive _ -> 100.
            | Pod _ -> 10.
            | _ -> 0.

        let private tagsScore (tags: KillTag list) =
            tags |> Seq.map tagScore |> Seq.sum
            

        let score (km: Kill) = 
            let score = tagsScore km.Tags
            score

