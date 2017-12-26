namespace EveAlod.Services

    open FSharp.Data
    open EveAlod.Entities

    type jsonGroupIdProvider = JsonProvider<"./SampleIds.json">
    type jsonGroupProvider = JsonProvider<"./SampleEntityGroup.json">
    type jsonEntityProvider = JsonProvider<"./SampleEntity.json">

    type StaticDataProvider()=

        let keyGroupIds = 
                [ 
                    "201"; // ECM
                    "1875"; // Plex
                    "1739"; // skill injectors
                ]        

        let getGroupEntity(id: int)=
            async {
                let uri = (sprintf "https://esi.tech.ccp.is/latest/universe/groups/%i/?datasource=tranquility&language=en-us" id)
                let! json = EveAlod.Data.Web.getData uri
                return match json with
                        | Some j -> let root = (jsonGroupProvider.Parse(j))
                                    Some {EntityGroup.Id = root.GroupId.ToString(); 
                                                Name = root.Name;
                                                EntityIds = root.Types |> Seq.map (fun i -> i.ToString()) |> Array.ofSeq
                                                }
                        | _ -> None
            }

        let getEntity(id: string)=
            async {
                let uri = (sprintf "https://esi.tech.ccp.is/latest/universe/types/%s/?datasource=tranquility&language=en-us" id)
                let! json = EveAlod.Data.Web.getData uri
                return match json with
                        | Some j -> let root = (jsonEntityProvider.Parse(j))
                                    Some {Entity.Id = root.TypeId.ToString(); 
                                                Name = root.Name;
                                                }
                        | _ -> None
            }


        let getGroupIds() = 
            [ 1 .. 3 ] 
            |> Seq.map (fun i -> (sprintf "https://esi.tech.ccp.is/latest/universe/groups/?datasource=tranquility&page=%i" i))
            |> Seq.map (fun uri -> EveAlod.Data.Web.getData uri)
            
        let getIds (json : Async<string option>) =
            async {
                let! j = json
                return match j with
                        | Some data -> jsonGroupIdProvider.Parse(data) |> List.ofSeq
                        | _ -> []
            }

        let getGroupIds()=
            let idLists = getGroupIds() 
                            |> Seq.map getIds
                            |> Async.Parallel
                            |> Async.RunSynchronously            
            idLists |> Seq.collect (fun xs -> xs) |> Array.ofSeq
            
        let getGroupEntities () =
            let entities = getGroupIds()
                            |> Seq.map getGroupEntity
                            |> Async.Parallel
                            |> Async.RunSynchronously
                            |> Seq.filter (fun o -> o.IsSome)
                            |> Seq.map (fun o -> o.Value)
                            |> Array.ofSeq
            entities

        let groupEntities = lazy ( getGroupEntities() )
        
        member this.Groups() = groupEntities.Value
        
        member this.Entities()=
            // TODO: 
            let groups = groupEntities.Value
                            |> Seq.map (fun grp -> (grp.Id, grp))
                            |> Map.ofSeq

            let entityIds = keyGroupIds 
                            |> Seq.map groups.TryFind
                            |> Seq.filter Option.isSome
                            |> Seq.collect (fun o -> o.Value.EntityIds)
                            |> Array.ofSeq

            let entities = entityIds 
                            |> Seq.map getEntity
                            |> Async.Parallel
                            |> Async.RunSynchronously
                            |> Seq.filter Option.isSome
                            |> Seq.map (fun o -> o.Value)
                            |> Array.ofSeq
            
            entities
