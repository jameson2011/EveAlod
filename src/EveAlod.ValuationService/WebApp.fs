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


    let webRoutes = 
        choose
            [   GET  >=> choose [
                                    pathScan "/stats/%s/" WebServices.getShipTypeStats
                                                            >=> WebServices.setNoCache >=> WebServices.jsonMimeType
                                ]
            ]

