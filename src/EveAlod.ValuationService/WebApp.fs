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
        let logRequest = WebServices.logRouteInvoke logger
        choose
            [   GET  >=> choose [                                    
                                    pathScan "/stats/%s/" (fun id -> logRequest >=> WebServices.getShipTypeStatsJson ships id)
                                                            >=> WebServices.setNoCache >=> WebServices.jsonMimeType

                                    path "/stats/" >=> logRequest 
                                                    >=> WebServices.getShipSummaryStatsJson ships
                                                    >=> WebServices.setNoCache >=> WebServices.jsonMimeType                                    

                                    pathScan "/valuation/%s/%f/" (fun (id, total) -> logRequest >=> WebServices.getShipTypeValuation ships id None total)
                                                            >=> WebServices.setNoCache >=> WebServices.jsonMimeType

                                    pathScan "/valuation/%s/%f/%f/" (fun (id, total, fitted) -> logRequest >=> WebServices.getShipTypeValuation ships id (Some fitted) total)
                                                            >=> WebServices.setNoCache >=> WebServices.jsonMimeType

                                    path "/favicon.ico" >=> Suave.Successful.no_content >=> WebServices.setCacheLimit 99999999
                                ];

                POST >=> choose [
                                    path "/kill/" >=> logRequest >=> WebServices.postKill logger ships >=> WebServices.jsonMimeType
                                ]
            ]

