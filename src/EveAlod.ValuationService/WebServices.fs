namespace EveAlod.ValuationService

open Suave
open Suave.Operators
open EveAlod.Common.Strings

module WebServices=
    open EveAlod.Valuation
    open System

    [<Literal>]    
    let private InternalErrorJson = """{ "error": "Internal error" }"""

    let jsonMimeType = Writers.setMimeType "application/json; charset=utf-8"
    
    let setExpiry (age: int ) = age |> toString |> Writers.setHeader "Expires" 
    let setPragmaNoCache = Writers.setHeader "Pragma" "no-cache"
    let setNoCacheControl = Writers.setHeader "Cache-Control" "no-cache, no-store, must-revalidate"

    let setCacheLimit (age: int) = 
        age * 60 |> toString |> sprintf "public, max-age=%s" |> Writers.setHeader "Cache-Control" 

    let setNoCache = setNoCacheControl >=> setPragmaNoCache >=> setExpiry 0

    let composeUri (host: Host) (hostRoot: Uri) path =
        let ub = UriBuilder(hostRoot)
        ub.Host <- host
        ub.Path <- path
        ub.Uri
    
    let getShipSummaryStatsJson (shipStats: ShipStatsActor) (ctx: HttpContext)=
        async {
            let uri = composeUri ctx.request.host ctx.request.url 

            let! stats = shipStats.GetShipSummaryStats()
            
            let json = stats |> Json.shipSummaryStatsToJson uri

            return! Successful.OK json ctx
        }
        
    let getShipTypeStatsJson (shipStats: ShipStatsActor) (id: string) (ctx: HttpContext)=
        async {
                                
            let! stats = shipStats.GetShipTypeStats id            

            let j = stats |> Json.shipTypeStatsToJson
                        
            return! Successful.OK j ctx
            
        }
    
    let postKill (logger: PostMessage) (shipStats: ShipStatsActor) (ctx: HttpContext)=
        async {
            try                
                let json = System.Text.UTF8Encoding.UTF8.GetString(ctx.request.rawForm)

                if (Json.isValidJson json) then            
                    sprintf "JSON received: %s" json |> (fun m -> EveAlod.Data.ActorMessage.Trace("ValuationService",m)) |> logger
                    json |> ValuationActorMessage.ImportKillJson |> shipStats.Post
                                
                    return! Successful.NO_CONTENT ctx
                else
                    sprintf "Invalid JSON received: %s" json |> (fun m -> EveAlod.Data.ActorMessage.Error("ValuationService",m)) |> logger

                    return! RequestErrors.BAD_REQUEST """{ "error": "Invalid JSON" }""" ctx
            
            with
            | e -> return! Suave.ServerErrors.INTERNAL_ERROR InternalErrorJson ctx
        }

    let getShipTypeValuation(shipStats: ShipStatsActor) (shipTypeId: string) (fittedValue: float option) (totalValue: float)  (ctx: HttpContext)=
        async {

            try
                let! stats = shipStats.GetShipTypeStats shipTypeId
                
                let json =  match fittedValue with 
                            | Some fittedValue -> 
                                let fitted = Statistics.valuation stats.FittedValuesSummary fittedValue
                                let total = Statistics.valuation stats.TotalValuesSummary totalValue
                                let spread = Statistics.medianMeanSpread stats.TotalValuesSummary
                                sprintf """ { "fitted": %f, "total": %f, "spread": %f }  """ fitted total spread
                            | _ ->                                 
                                let total = Statistics.valuation stats.TotalValuesSummary totalValue
                                let spread = Statistics.medianMeanSpread stats.TotalValuesSummary
                                sprintf """ { "total": %f, "spread": %f }  """ total spread

                return! Successful.OK json ctx
            with
            | e -> return! Suave.ServerErrors.INTERNAL_ERROR InternalErrorJson ctx
        }

    