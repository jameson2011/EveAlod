namespace EveAlod.Services.IntegrationTests

open Xunit
open FsCheck
open FsCheck.Xunit
open EveAlod.Data
open EveAlod.Services

module StaticDataActorTest=
    open Constants

    let staticProvider = StaticEntityProvider() :> IStaticEntityProvider
    let log (msg: ActorMessage) = ignore 0
    let get (get: 'k -> Async<'a option>) id =         
        match get(id) |> Async.RunSynchronously with 
        | Some c -> c
        | _ -> failwith "not Some"
    let getNone (get: 'k -> Async<'a option>) id =         
        match get(id) |> Async.RunSynchronously with 
        | None -> None
        | _ -> failwith "not None"

    [<Fact>]
    let ``Ship IDs returned from cache``()=

        let actor = StaticDataActor(log, staticProvider)
        let id = EntityGroupKey.Ship

        let getEntity = get actor.EntityIds

        let entity = getEntity id
        let entity2 = getEntity id
        let entity3 = getEntity id
        
        Assert.Equal(entity.Count, entity2.Count)
        Assert.Equal(entity.Count, entity3.Count)


    [<Fact>]
    let ``Plex IDs returned from cache``()=

        let actor = StaticDataActor(log, staticProvider)        
        let id = EntityGroupKey.Plex

        let getEntity = get actor.EntityIds

        let entity = getEntity id
        let entity2 = getEntity id
        let entity3 = getEntity id
        
        Assert.Equal(entity.Count, entity2.Count)
        Assert.Equal(entity.Count, entity3.Count)

        
    [<Fact>]
    let ``Character returns character``() =
        let actor = StaticDataActor(log, staticProvider)
        let id = testCharId

        let getEntity = get actor.Character

        let entity = getEntity id
        let entity2 = getEntity id

        Assert.Equal(id, entity.Char.Id)
        Assert.Equal(id, entity2.Char.Id)

    
    [<Fact>]
    let ``Character returns None``() =
        let actor = StaticDataActor(log, staticProvider)
        let id = System.Guid.NewGuid().ToString()

        let getEntity = getNone actor.Character

        let entity = getEntity id
        let entity2 = getEntity id
        let entity3 = getEntity id

        Assert.True(Option.isNone entity)
        Assert.True(Option.isNone entity2)
        Assert.True(Option.isNone entity3)

    
    [<Theory>]
    [<InlineData("587", "Rifter")>]
    [<InlineData("23919", "Aeon")>]
    [<InlineData("28352", "Rorqual")>]    
    let ``Entity returns entity``(id: string, name: string) =
        let actor = StaticDataActor(log, staticProvider)
        
        let getEntity = get actor.Entity 

        let entity = getEntity (id.ToString())
        let entity2 = getEntity (id.ToString())

        Assert.Equal(id, entity.Id)
        Assert.Equal(name, entity.Name)
        Assert.Equal(id, entity2.Id)
        Assert.Equal(name, entity2.Name)

        