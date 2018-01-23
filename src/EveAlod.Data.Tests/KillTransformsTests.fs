namespace EveAlod.Data.Tests

    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Common.Tests
    open EveAlod.Data

    module KillTransformsTests=
        
        [<Fact>]
        let ``toEntity empty value yields None``()=
            let r = KillTransforms.toEntity ""
            r = None

        [<Property(Verbose = true, Arbitrary = [| typeof<EveAlod.Common.Tests.NonEmptyStrings> |])>]
        let ``toEntity id applied to Entity Id``(id: string)=
            let r = (KillTransforms.toEntity id).Value.Id 
            
            r = id

        [<Property(Verbose = true, Arbitrary = [| typeof<EveAlod.Common.Tests.NonDigitStrings> |])>]
        let ``toItemLocation non-integer value yields Unknown``(id)=
            let r = KillTransforms.toItemLocation id
            r = ItemLocation.Unknown