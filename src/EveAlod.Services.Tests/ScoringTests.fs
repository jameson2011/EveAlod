namespace EveAlod.Services.Tests

    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Data

    module ScoringTests =
        open EveAlod.Services
        
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

            match Scoring.score kill, Scoring.isNullifier tag with
            | x,true -> x = 0.
            | x,_ -> x > 0.

        [<Property(Verbose=true, MaxTest = 1000)>]
        let ``tag scores are accumulative``(tags: KillTag list)=
            let tags = tags |> List.filter (Scoring.isNullifier >> not) |> List.distinct
            let kill tag = { Kill.empty with Tags = [ tag ] }
            let tagScores = tags    |> List.map kill
                                    |> List.map Scoring.score
                                    
            let tagsScore = { Kill.empty with Tags = tags}
                                |> Scoring.score
            
            tagsScore >= (tagScores |> Seq.sum)

            
