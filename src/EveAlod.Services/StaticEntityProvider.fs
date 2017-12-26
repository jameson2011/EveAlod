namespace EveAlod.Services

    open System
    open FSharp.Data
    open EveAlod.Entities

    type jsonGroupIdProvider = JsonProvider<"./SampleIds.json">
    type jsonGroupProvider = JsonProvider<"./SampleEntityGroup.json">
    type jsonEntityProvider = JsonProvider<"./SampleEntity.json">

    type IStaticEntityProvider=
        abstract member EntityIds: EntityGroupKey -> Set<string>

    type StaticEntityProvider()=
    
        let ecmGroupId = "201"
        let plexGroupId = "1875"
        let skillInjectorGroupId = "1739"
        let capsuleGroupId = "29"

        let getGroupEntity(id: string)=
            async {
                let uri = (sprintf "https://esi.tech.ccp.is/latest/universe/groups/%s/?datasource=tranquility&language=en-us" id)
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
                        | Some data -> 
                            jsonGroupIdProvider.Parse(data) 
                            |> Seq.map (fun i -> i.ToString())
                            |> List.ofSeq
                        | _ -> []
            }

        let getGroupIds()=
            let idLists = getGroupIds() 
                            |> Seq.map getIds
                            |> Async.Parallel
                            |> Async.RunSynchronously            
            idLists |> Seq.collect (fun xs -> xs) |> Array.ofSeq
            
        let getGroupEntities (ids: seq<string>) =
            ids
                |> Seq.map getGroupEntity
                |> Async.Parallel
                |> Async.RunSynchronously
                |> Seq.filter (fun o -> o.IsSome)
                |> Seq.map (fun o -> o.Value)
                |> Array.ofSeq
                    
        let entityGroups =
            [             
                EntityGroupKey.Ecm, lazy ( [ ecmGroupId ] |> getGroupEntities );
                EntityGroupKey.Plex, lazy ( [ plexGroupId ] |> getGroupEntities );
                EntityGroupKey.SkillInjector, lazy ( [ skillInjectorGroupId ] |> getGroupEntities );
                EntityGroupKey.Capsule, lazy ( [ capsuleGroupId ] |> getGroupEntities );
            ]
            |> Map.ofSeq
        
        let groupEntityIds (key: EntityGroupKey)=
            match entityGroups |> Map.tryFind key with
                | Some grp -> grp.Value
                                |> Seq.collect (fun o -> o.EntityIds)
                                |> Set.ofSeq
                | _ -> Set.empty<string>


        interface IStaticEntityProvider with
            member this.EntityIds(key: EntityGroupKey)= 
                groupEntityIds key

            
