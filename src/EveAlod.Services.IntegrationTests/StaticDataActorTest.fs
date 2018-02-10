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
    

    [<Fact>]
    let ``Ship IDs returned from cache``()=

        let actor = StaticDataActor(log, staticProvider)
        let getIds() = match actor.EntityIds(EntityGroupKey.Ship) |> Async.RunSynchronously with         
                        | Some xs -> xs |> List.ofSeq
                        | _ -> failwith "Not Some"
        
        let shipIds = getIds()
        let shipIds2 = getIds()
        let shipIds3 = getIds()

        Assert.Equal(shipIds.Length, shipIds2.Length)
        Assert.Equal(shipIds.Length, shipIds3.Length)

    [<Fact>]
    let ``Plex IDs returned from cache``()=

        let actor = StaticDataActor(log, staticProvider)
        let getIds() = match actor.EntityIds(EntityGroupKey.Plex) |> Async.RunSynchronously with         
                        | Some xs -> xs |> List.ofSeq
                        | _ -> failwith "Not Some"
        
        let entityIds = getIds()
        let entityIds = getIds()
        let entityIds = getIds()

        Assert.Equal(entityIds.Length, entityIds.Length)
        Assert.Equal(entityIds.Length, entityIds.Length)

        
    [<Fact>]
    let ``Character returns character``() =
        let actor = StaticDataActor(log, staticProvider)

        let char = match actor.Character(testCharId) |> Async.RunSynchronously with 
                    | Some c -> c
                    | _ -> failwith "not Some"

        Assert.Equal(testCharId, char.Char.Id)

    [<Fact>]
    let ``SolarSystem returns entity``() =
        let actor = StaticDataActor(log, staticProvider)

        let entity = match actor.SolarSystem(jitaId) |> Async.RunSynchronously with 
                        | Some c -> c
                        | _ -> failwith "not Some"

        Assert.Equal(jitaId, entity.Id)


        

        