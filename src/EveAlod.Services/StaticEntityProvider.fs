namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data
    open FSharp.Control.AsyncSeqExtensions
    open FSharp.Control


    type StaticEntityProvider()=
    
        let ecmGroupId = "201"
        let plexGroupId = "1875"
        let skillInjectorGroupId = "1739"
        let capsuleGroupId = "29"
        let shipCategoryId = "6"

        let httpClient = Web.httpClient()
        let getData = Web.getData httpClient

        
        let getGroupEntity(id: string)=
            async {
                let uri = (sprintf "https://esi.tech.ccp.is/latest/universe/groups/%s/?datasource=tranquility&language=en-us" id)
                let! resp = getData uri
                return match resp.Status with
                        | EveAlod.Common.HttpStatus.OK -> 
                            resp.Message |> EntityTransforms.parseEntityGroup
                        | _ -> None
            }

        let entityGroupId = 
            function
            | EntityGroupKey.Ecm -> ecmGroupId
            | EntityGroupKey.Plex ->  plexGroupId
            | EntityGroupKey.SkillInjector -> skillInjectorGroupId
            | EntityGroupKey.Capsule -> capsuleGroupId
            | EntityGroupKey.Ship -> failwith "Unrecognised"
        
        let groupEntityIds (id)=            
            async {  
                    let! entity = id |> getGroupEntity
                    match entity with
                    | Some e -> return Some (e.EntityIds |> Set.ofSeq)
                    | _ -> return None    
                }
        
        let getEntity(id: string)=
            async {
                let uri = (sprintf "https://esi.tech.ccp.is/latest/universe/types/%s/?datasource=tranquility&language=en-us" id)
                let! response = getData uri
                return match response.Status with
                        | EveAlod.Common.HttpStatus.OK -> 
                                    response.Message |> EntityTransforms.parseEntity
                        | _ -> None
            }

        let getCharacter(id: string)=
            async {
                let uri = (sprintf "https://esi.tech.ccp.is/latest/characters/%s/?datasource=tranquility&language=en-us" id)
                let! response = getData uri
                return match response.Status with
                        | EveAlod.Common.HttpStatus.OK -> 
                            response.Message |> EntityTransforms.parseCharacter id
                        | _ -> None
            }

        let getSolarSystem(id: string)=
            async{
                let uri = (sprintf "https://esi.tech.ccp.is/latest/universe/systems/%s/?datasource=tranquility&language=en-us" id)
                let! response = getData uri
                return match response.Status with
                        | EveAlod.Common.HttpStatus.OK -> 
                            response.Message |> EntityTransforms.parseSolarSystem
                        | _ -> None
                }

        let getCorpByTicker(ticker: string)=
            async{
                let uri = sprintf "https://esi.tech.ccp.is/latest/search/?categories=corporation&datasource=tranquility&language=en-us&search=%s&strict=true" ticker
                let! response = getData uri
                return match response.Status with
                        | EveAlod.Common.HttpStatus.OK -> 
                            response.Message |> EntityTransforms.parseCorpSearchResult
                        | _ -> None
                }

        let getCategoryGroupIds id =
            async {
                let uri = (sprintf "https://esi.tech.ccp.is/latest/universe/categories/%s/?datasource=tranquility&language=en-us" id)
                let! resp = getData uri
                return match resp.Status with
                        | EveAlod.Common.HttpStatus.OK -> 
                            resp.Message |> EntityTransforms.parseCategoryGroupIds
                        | _ -> []
            }

        let getGroupEntityIds(groupIds: Async<string list>)=
            async {
                let! ids = groupIds
                let entityIds = asyncSeq {
                                    for id in ids do
                                        let! ids = groupEntityIds id 
                                        let result = (match ids with 
                                                        | Some xs -> xs |> List.ofSeq
                                                        | _ -> [] )
                                        yield result
                                }
                
                let entityIds = entityIds |> AsyncSeq.fold List.append []
                let! result = entityIds 
                return result |> Set.ofList |> Some
                }
            
        

        interface IStaticEntityProvider with
            member __.EntityIds(key: EntityGroupKey) = 
                match key with
                | Ship -> 
                    shipCategoryId |> getCategoryGroupIds |> getGroupEntityIds
                | _ -> 
                    key |> entityGroupId |> groupEntityIds

            member __.Entity (id: string) = getEntity id

            member __.Character(id: string) = getCharacter id

            member __.SolarSystem(id: string) = getSolarSystem id

            member __.CorporationByTicker(ticker: string) = getCorpByTicker ticker
            
 