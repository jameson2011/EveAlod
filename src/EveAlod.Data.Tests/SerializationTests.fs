namespace EveAlod.Data.Tests

    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Data
    open EveAlod.Common.Tests
    
    module SerializationTests=

        
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |] )>]
        let ``killToJson``(kill: Kill) =
            let id, json = Serialization.killToJson kill

            id = kill.Id &&
            (not (System.String.IsNullOrWhiteSpace(json)))

        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |] )>]
        let ``killToJson killFromJson is symmetric``(kill: Kill) (tag: KillTag) =
            let kill = { kill with Tags = [ tag ]}

            let _, json = Serialization.killToJson kill

            let kill2 = Serialization.killFromJson json

            kill.Id = kill2.Id 
            && kill.Occurred = kill2.Occurred 
            && kill.Tags = kill2.Tags
        
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |] )>]
        let ``killToJson killFromJson is symmetric all props``(kill: Kill) (tag: KillTag) =
            let kill = { kill with Tags = [ tag ]}

            let _, json = Serialization.killToJson kill

            let kill2 = Serialization.killFromJson json

            kill.Location = kill2.Location 
            &&
            kill.Victim = kill2.Victim &&
            kill.VictimShip = kill2.VictimShip 
            &&
            kill.Fittings = kill2.Fittings &&
            kill.Cargo = kill2.Cargo && 
            kill.Attackers = kill2.Attackers &&
            kill.Tags = kill2.Tags
            
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings>; typeof<PositiveFloats> |] )>]
        let ``killToJson killFromJson is symmetric TotalValue``(kill: Kill) =
            
            let _, json = Serialization.killToJson kill

            let kill2 = Serialization.killFromJson json

            kill.TotalValue = kill2.TotalValue