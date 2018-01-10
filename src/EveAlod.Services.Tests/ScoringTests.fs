namespace EveAlod.Services.Tests

    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Data

    module ScoringTests =
        open EveAlod.Services
        open FsCheck.Random
        
        let defaultKill() =
            { Kill.Id = "";
                Occurred = System.DateTime.UtcNow;
                Tags = [];
                ZkbUri = "";
                Location = None;
                Victim = None;
                VictimShip = None;
                AlodScore = 0.;
                Attackers = [];
                Cargo = [];
                TotalValue = 0.;
            }

        [<Property(Verbose=true)>]
        let ``score dependent on tags``(tags: KillTag list)=
            let kill = { defaultKill() with Tags = tags |> List.distinct}

            Scoring.score kill |> ignore
            
            true
            
        [<Fact>]
        let ``empty tags have zero score``()=            
            ({ defaultKill() with Tags = [] } |> Scoring.score ) = 0.

        [<Property(Verbose=true)>]
        let ``all tags have a non-zero score``(tag: KillTag)=
            let kill = { defaultKill() with Tags = [ tag ] }

            let r = Scoring.score kill 
            
            r <> 0.

        [<Property(Verbose=true, Replay="(193286329,296399112)")>]
        let ``tag scores are accumulative``(tags: KillTag list)=
            let tags = tags |> List.distinct
            let kill tag = { defaultKill() with Tags = [ tag ] }
            let tagScores = tags    |> List.map kill
                                    |> List.map Scoring.score

                                
            let tagsScore = { defaultKill() with Tags = (tags ) }
                                |> Scoring.score
                                
            tagsScore = (tagScores |> Seq.sum)

            
