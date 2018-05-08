namespace EveAlod.Data.Tests

open Xunit
open FSharp.Data
open FsCheck
open FsCheck.Xunit
open EveAlod.Common.Strings
open EveAlod.Common.Tests
open EveAlod.Data
open EveAlod.Data

module ShipTransformsTests=

    [<Theory>]
    [<InlineData(41030)>]
    [<InlineData(43681)>]
    let ``shipIsFittable mining drones are unfittable``(id)=
        let result = id |> IronSde.ItemTypes.itemType
                        |> Option.get
                        |> ShipTransforms.shipIsFittable
        
        Assert.False(result)

    
    [<Theory>]
    [<InlineData(670)>]
    let ``shipIsFittable pods are fittable``(id)=
        let result = id |> IronSde.ItemTypes.itemType
                        |> Option.get
                        |> ShipTransforms.shipIsFittable
                        
        Assert.True(result)