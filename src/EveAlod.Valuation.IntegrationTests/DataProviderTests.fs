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

    
    [<Fact>]
    let ``KillIds returns Some``()=
        let d = new DateTime(2018, 2, 23)
        let dp = EveAlod.Valuation.DataProvider()

        let result = dp.KillIds(d) |> Async.RunSynchronously

        match result with
        | Some ids when ids.Length = 0 -> failwith "Empty list returned"
        | None -> failwith "None returned"
        | _ -> ignore 0
        

    [<Fact>]
    let ``KillIds returns Empty``()=
        let d = new DateTime(1900, 1, 1)
        let dp = EveAlod.Valuation.DataProvider()

        let result = dp.KillIds(d) |> Async.RunSynchronously
        
        match result with
        | Some ids when ids.Length > 0 -> failwith "Non-empty list returned"
        | None -> failwith "None returned"
        | _ -> ignore 0

    [<Fact>]
    let ``KillIds returns None``()=
        let d = new DateTime(1900, 1, 1)
        let dp = EveAlod.Valuation.DataProvider(Guid.NewGuid().ToString())

        let result = dp.KillIds(d) |> Async.RunSynchronously
        
        Assert.Equal(None, result)

    [<Fact>]
    let ``Kill returns Some``()=
        let killId = "67703083"
        let dp = EveAlod.Valuation.DataProvider()

        let result = dp.Kill(killId) |> Async.RunSynchronously

        Assert.NotEqual(None, result)

    [<Fact>]
    let ``Kill returns None with unknown ID``()=
        let killId = Guid.NewGuid().ToString()
        let dp = EveAlod.Valuation.DataProvider()

        let result = dp.Kill(killId) |> Async.RunSynchronously

        Assert.Equal(None, result)

    [<Fact>]
    let ``Kill returns None with bad host``()=
        let killId = Guid.NewGuid().ToString()
        let dp = EveAlod.Valuation.DataProvider(Guid.NewGuid().ToString())

        let result = dp.Kill(killId) |> Async.RunSynchronously

        Assert.Equal(None, result)
