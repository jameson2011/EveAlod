namespace EveAlod.ValuationService

open System
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open EveAlod.Valuation

module WebApp=
    
    let webConfig (config: ValuationConfiguration) =
        let port = config.WebPort
            
        { defaultConfig with bindings = [ HttpBinding.create HTTP System.Net.IPAddress.Any port ];  }


    let webRoutes (logger: PostMessage) (ships: ShipStatsActor)= 
        choose
            [   GET  >=> choose [
                                    pathScan "/stats/%s/" (WebServices.getShipTypeStatsJson ships)
                                                            >=> WebServices.setNoCache >=> WebServices.jsonMimeType

                                    path "/stats/" >=> WebServices.getShipSummaryStatsJson ships
                                                            >=> WebServices.setNoCache >=> WebServices.jsonMimeType

                                    pathScan "/val/%s/%f/%f/" (fun (id, fitted, total) -> WebServices.getShipTypeValuation ships id fitted total)
                                                            >=> WebServices.setNoCache >=> WebServices.jsonMimeType

                                    path "/favicon.ico" >=> Suave.Successful.no_content >=> WebServices.setCacheLimit 99999999
                                ];

                POST >=> choose [
                                    path "/kill/" >=> WebServices.postKill logger ships >=> WebServices.jsonMimeType
                                ]
            ]

