namespace EveAlod.Data.Tests

    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Common.Combinators
    open EveAlod.Common.Tests
    open EveAlod.Data

    module EntityTransformsTests=
        
        type HighsecStatus =
            static member Values()=
                Arb.Default.Decimal() |> Arb.filter (fun x -> x >= 0.5m)

        type LowsecStatus =
            static member Values()=
                Arb.Default.Decimal() |> Arb.filter (fun x -> x > 0.0m && x >= 0.5m)
        
        type NullsecStatus =
            static member Values()=
                Arb.Default.Decimal() |> Arb.filter (fun x -> x <= 0.0m)
                
        
        [<Property(Verbose = true, Arbitrary = [| typeof<HighsecStatus> |])>]
        let ``getSecurityStatus all highsec``(name, status)=
            let r = EntityTransforms.getSecurityStatus name status
            r = SpaceSecurity.Highsec

        [<Property(Verbose = true, Arbitrary = [| typeof<LowsecStatus> |])>]
        let ``getSecurityStatus all lowsec``(name, status)=
            let r = EntityTransforms.getSecurityStatus name status
            r = SpaceSecurity.Highsec

        [<Property(Verbose = true, Arbitrary = [| typeof<NullsecStatus> |])>]
        let ``getSecurityStatus thera``(status)=
            let r = EntityTransforms.getSecurityStatus "thera" status
            r = SpaceSecurity.Wormhole
