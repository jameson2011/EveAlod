namespace EveAlod.Data.Tests

    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Common.Strings
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

        
        [<Fact>]
        let ``toEntity empty value yields None``()=
            let r = EntityTransforms.toEntity ""
            r = None

        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
        let ``toEntity id applied to Entity Id``(id: string)=
            let r = (EntityTransforms.toEntity id).Value.Id             
            r = id
            
        [<Property(Verbose = true, Arbitrary = [| typeof<NonDigitStrings> |])>]
        let ``toItemLocation non-integer value yields Unknown``(id)=
            let r = EntityTransforms.toItemLocation id
            r = ItemLocation.Unknown

        
        [<Property(Verbose = true, Arbitrary = [| typeof<PositiveInts> |])>]
        let ``toItemLocation integer value yields without crash``(id: int)=
            let r = id |> str |> EntityTransforms.toItemLocation
            true
