namespace EveAlod.ValuationService

open System
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

module WebApp=
    
    let webConfig (config: EveAlod.Valuation.Configuration) =
        let port = config.WebPort
            
        { defaultConfig with bindings = [ HttpBinding.create HTTP System.Net.IPAddress.Any port ];  }


    let webRoutes = 
        choose
            [   GET  >=> choose [
                                    pathScan "/stats/%s/" (fun id -> WebServices.getShipTypeStats id) 
                                                            >=> WebServices.setNoCache >=> WebServices.jsonMimeType
                                ]
            ]

