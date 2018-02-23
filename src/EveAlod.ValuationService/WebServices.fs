namespace EveAlod.ValuationService

open Suave
open Suave.Operators
open EveAlod.Common.Strings

module WebServices=
    open EveAlod.Valuation
    open System
    
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
            
            let json = stats |> EntityTransforms.shipSummaryStatsToJson uri

            return! Successful.OK json ctx
        }
        
    let getShipTypeStatsJson (shipStats: ShipStatsActor) (id: string) (ctx: HttpContext)=
        async {
                                
            let! stats = shipStats.GetShipTypeStats id
            
            let j = EntityTransforms.shipTypeStatsToJson stats
                        
            return! Successful.OK j ctx
            
        }
