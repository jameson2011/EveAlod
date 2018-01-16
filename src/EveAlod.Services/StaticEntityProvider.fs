namespace EveAlod.Services

    open System
    open FSharp.Data
    open EveAlod.Common
    open EveAlod.Data

    type jsonGroupIdProvider = JsonProvider<"./SampleIds.json">
    type jsonGroupProvider = JsonProvider<"./SampleEntityGroup.json">
    type jsonEntityProvider = JsonProvider<"./SampleEntity.json">
    type jsonCharacterProvider = JsonProvider<"./SampleCharacter.json">

    type IStaticEntityProvider=
        abstract member EntityIds: EntityGroupKey -> Set<string>
        abstract member Entity: string -> Async<Entity option>
        abstract member Character: string -> Async<Character option>

    type StaticEntityProvider()=
    
        let ecmGroupId = "201"
        let plexGroupId = "1875"
        let skillInjectorGroupId = "1739"
        let capsuleGroupId = "29"

        let httpClient = Web.httpClient()
        let getData = Web.getData httpClient

        let getGroupEntity(id: string)=
            async {
                let uri = (sprintf "https://esi.tech.ccp.is/latest/universe/groups/%s/?datasource=tranquility&language=en-us" id)
                let! json = getData uri
                return match json with
                        | EveAlod.Common.HttpResponse.OK j -> 
                                    let root = (jsonGroupProvider.Parse(j))
                                    Some {EntityGroup.Id = root.GroupId.ToString(); 
                                                Name = root.Name;
                                                EntityIds = root.Types |> Seq.map (fun i -> i.ToString()) |> Array.ofSeq
                                                }
                        | _ -> None
            }
            
            
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

        let getEntity(id: string)=
            async {
                let uri = (sprintf "https://esi.tech.ccp.is/latest/universe/types/%s/?datasource=tranquility&language=en-us" id)
                let! json = getData uri
                return match json with
                        | EveAlod.Common.HttpResponse.OK j -> 
                                    let root = (jsonEntityProvider.Parse(j))                
                                    Some {Entity.Id = root.TypeId.ToString(); 
                                                Name = root.Name;
                                                }
                        | _ -> None
            }

        let getCharacter(id: string)=
            async {
                let uri = (sprintf "https://esi.tech.ccp.is/latest/characters/%s/?datasource=tranquility&language=en-us" id)
                let! json = getData uri
                return match json with
                        | EveAlod.Common.HttpResponse.OK j -> 
                                    let root = (jsonCharacterProvider.Parse(j))                
                                    Some {Character.Char = {Entity.Id = id; Name = root.Name; };
                                            Corp = Some {Entity.Id = root.CorporationId.ToString(); Name = "" };
                                            Alliance = None
                                        }
                        | _ -> None
            }

        interface IStaticEntityProvider with
            member this.EntityIds(key: EntityGroupKey) = groupEntityIds key

            member this.Entity (id: string) = getEntity id

            member this.Character(id: string) = getCharacter id

            
