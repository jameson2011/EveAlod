namespace EveAlod.Data.Tests

    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Common.Strings
    open EveAlod.Common.Tests
    open EveAlod.Data

    module EntityTransformsTests=
     
        
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
