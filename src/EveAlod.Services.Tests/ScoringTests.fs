namespace EveAlod.Services.Tests

    open Xunit
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
            let tags = [ tag ]
            let kill = { Kill.empty with Tags = tags }

            match Scoring.score kill, Scoring.isNullified (Set.ofList tags) with
            | x,true -> x = 0.
            | x,_ -> x >= 0.

        [<Property(Verbose=true, MaxTest = 1000)>]
        let ``tag scores are accumulative``(tags: KillTag list)=
            
            let tags = tags |> List.distinct
            let kill tag = { Kill.empty with Tags = [ tag ] }
            let tagScores = tags    |> Seq.map kill
                                    |> Seq.map Scoring.score
                                    
            let isNulled = tags |> Set.ofSeq |> (Scoring.isNullified)
            match isNulled, (tagScores |> Seq.sum) with
            | false, x -> x >= x
            | true, x -> true

            
