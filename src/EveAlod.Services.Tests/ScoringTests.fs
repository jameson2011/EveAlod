namespace EveAlod.Services.Tests

    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Data

    module ScoringTests =
        open EveAlod.Services
        open FsCheck.Random
        
        

        [<Property(Verbose=true)>]
        let ``score dependent on tags``(tags: KillTag list)=
            let kill = { Kill.empty with Tags = tags |> List.distinct}

            Scoring.score kill |> ignore
            
            true
            
        [<Fact>]
        let ``empty tags have zero score``()=            
            ({ Kill.empty with Tags = [] } |> Scoring.score ) = 0.

        [<Property(Verbose=true)>]
        let ``all tags have a positive score``(tag: KillTag)=
            let kill = { Kill.empty with Tags = [ tag ] }

            (Scoring.score kill) > 0.

        [<Property(Verbose=true, Replay="(193286329,296399112)")>]
        let ``tag scores are accumulative``(tags: KillTag list)=
            let tags = tags |> List.distinct
            let kill tag = { Kill.empty with Tags = [ tag ] }
            let tagScores = tags    |> List.map kill
                                    |> List.map Scoring.score
                                    
            let tagsScore = { Kill.empty with Tags = tags}
                                |> Scoring.score
            
            tagsScore >= (tagScores |> Seq.sum)

            
