namespace EveAlod.Valuation.IntegrationTests

open System
open Xunit
open EveAlod.Valuation

module DataProviderTests=

    let rifterId = "587"

    [<Fact>]
    let ``ShipStatistics returns Some``()=
        
        let dp = EveAlod.Valuation.DataProvider()
        
        let result = dp.ShipStatistics rifterId |> Async.RunSynchronously
        let stats = match result with
                    | Some r -> r
                    | _ -> failwith "not Some"
        
        Assert.Equal(rifterId, stats.ShipId)
        Assert.NotEqual(0, stats.Losses |> Seq.length)
        Assert.NotEqual(0, stats.Kills |> Seq.length)
        
    [<Fact>]
    let ``ShipStatistics returns None``()=
        let id = Guid.NewGuid().ToString()
        let dp = EveAlod.Valuation.DataProvider()
        
        let result = dp.ShipStatistics id |> Async.RunSynchronously

        Assert.Equal(None, result)


