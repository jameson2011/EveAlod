namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data


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
        
        let groupEntityIds (key: EntityGroupKey)=            
            async {  
                    let! entity = key |> entityGroupId |> getGroupEntity
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

        interface IStaticEntityProvider with
            member this.EntityIds(key: EntityGroupKey) = groupEntityIds key

            member this.Entity (id: string) = getEntity id

            member this.Character(id: string) = getCharacter id

            member this.SolarSystem(id: string) = getSolarSystem id

            member this.CorporationByTicker(ticker: string) = getCorpByTicker ticker
            
