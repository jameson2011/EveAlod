namespace EveAlod.Services.IntegrationTests

open Xunit
open FsCheck
open FsCheck.Xunit
open EveAlod.Services

module StaticDataProviderTests=
    open EveAlod.Data
    
    let staticProvider = StaticEntityProvider() :> IStaticEntityProvider

    [<Fact>]
    let ``CorporationByTicker returns corpId``() =
        let corp = staticProvider.CorporationByTicker("R1FTA") |> Async.RunSynchronously

        let corpId = corp |> Option.map (fun c -> c.Id) |> Option.defaultValue "" 
        
        Assert.Equal(corpId, "423280073")

    [<Fact>]
    let ``CorporationByTicker returns None``() =
        let ticker = System.Guid.NewGuid().ToString()
        let corp = staticProvider.CorporationByTicker(ticker) |> Async.RunSynchronously
        
        Assert.True(corp |> Option.isNone)


    [<Fact>]
    let ``Character returns character``() =
        let name = "Jameson2011"
        let id = "90230509"
        let char = staticProvider.Character(id) |> Async.RunSynchronously

        let charId = char |> Option.map (fun c -> c.Char.Id) |> Option.defaultValue "" 
        let charName = char |> Option.map (fun c -> c.Char.Name) |> Option.defaultValue "" 

        Assert.Equal(charId, id)
        Assert.Equal(charName, name)

    [<Fact>]
    let ``Character returns None``() =
        let id = System.Guid.NewGuid().ToString()
        let char = staticProvider.Character(id) |> Async.RunSynchronously

        Assert.True(char |> Option.isNone)

    [<Fact>]
    let ``EntityIds Plex``() =
        let ids = staticProvider.EntityIds(EntityGroupKey.Plex) |> Async.RunSynchronously
        let count = ids |> Option.map (fun s -> s.Count) |> Option.defaultValue 0
        
        Assert.NotEqual(0, count)

    [<Fact>]
    let ``EntityIds SkillInjector``() =
        let ids = staticProvider.EntityIds(EntityGroupKey.SkillInjector) |> Async.RunSynchronously
        let count = ids |> Option.map (fun s -> s.Count) |> Option.defaultValue 0
        
        Assert.NotEqual(0, count)

    [<Fact>]
    let ``EntityIds Capsule``() =
        let ids = staticProvider.EntityIds(EntityGroupKey.Capsule) |> Async.RunSynchronously
        let count = ids |> Option.map (fun s -> s.Count) |> Option.defaultValue 0
        
        Assert.NotEqual(0, count)

    [<Fact>]
    let ``EntityIds Ecm``() =
        let ids = staticProvider.EntityIds(EntityGroupKey.Ecm) |> Async.RunSynchronously
        let count = ids |> Option.map (fun s -> s.Count) |> Option.defaultValue 0
        
        Assert.NotEqual(0, count)

    [<Fact>]
    let ``SolarSystem Jita is Jita``() =
        let id = "30000142"
        let system = match staticProvider.SolarSystem(id) |> Async.RunSynchronously with
                        | Some s -> s
                        | _ -> failwith "not Some"
        
        Assert.Equal(id, system.Id)
        Assert.Equal("Jita", system.Name)
        Assert.Equal(SpaceSecurity.Highsec, system.Security)

    [<Fact>]
    let ``SolarSystem OMS is OMS``() =
        let id = "30005000"
        let system = match staticProvider.SolarSystem(id) |> Async.RunSynchronously with
                        | Some s -> s
                        | _ -> failwith "not Some"
        
        Assert.Equal(id, system.Id)
        Assert.Equal("Old Man Star", system.Name)
        Assert.Equal(SpaceSecurity.Lowsec, system.Security)

    [<Fact>]
    let ``SolarSystem Thera is Thera``() =
        let id = "31000005"
        let system = match staticProvider.SolarSystem(id) |> Async.RunSynchronously with
                        | Some s -> s
                        | _ -> failwith "not Some"
        
        Assert.Equal(id, system.Id)
        Assert.Equal("Thera", system.Name)
        Assert.Equal(SpaceSecurity.Wormhole, system.Security)

    [<Fact>]
    let ``SolarSystem Poitot``() =
        let id = "30003271"
        let system = match staticProvider.SolarSystem(id) |> Async.RunSynchronously with
                        | Some s -> s
                        | _ -> failwith "not Some"
        
        Assert.Equal(id, system.Id)
        Assert.Equal("Poitot", system.Name)
        Assert.Equal(SpaceSecurity.Nullsec, system.Security)

    [<Fact>]
    let ``SolarSystem None on unknown id``() =
        let id = System.Guid.NewGuid().ToString()
        let system = staticProvider.SolarSystem(id) |> Async.RunSynchronously
        
        Assert.True(system |> Option.isNone)
        
    [<Fact>]
    let ``Entity returns Rifter``() =
        let id = "587"
        let entity = match staticProvider.Entity(id) |> Async.RunSynchronously with
                     | Some e -> e
                     | _ -> failwith "not Some"
        
        Assert.Equal(id, entity.Id)
        Assert.Equal("Rifter", entity.Name)

    [<Fact>]
    let ``Entity returns None on unknown id``() =
        let id = System.Guid.NewGuid().ToString()
        let system = staticProvider.Entity(id) |> Async.RunSynchronously
        
        Assert.True(system |> Option.isNone)
        