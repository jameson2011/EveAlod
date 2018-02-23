namespace EveAlod.ValuationService

open Suave
open Suave.Operators
open EveAlod.Common.Strings

module WebServices=
    open EveAlod.Valuation
    
    let jsonMimeType = Writers.setMimeType "application/json; charset=utf-8"

    let setExpiry (age: int ) = age |> toString |> Writers.setHeader "Expires" 
    let setPragmaNoCache = Writers.setHeader "Pragma" "no-cache"
    let setNoCacheControl = Writers.setHeader "Cache-Control" "no-cache, no-store, must-revalidate"

    let setCacheLimit (age: int) = 
        age * 60 |> toString |> sprintf "public, max-age=%s" |> Writers.setHeader "Cache-Control" 

    let setNoCache = setNoCacheControl >=> setPragmaNoCache >=> setExpiry 0
    
    
    let getShipSummaryStatsJson (shipStats: ShipStatsActor) (ctx: HttpContext)=
        async {
            let! stats = shipStats.GetShipSummaryStats()

            let j = EntityTransforms.shipSummaryStatsToJson stats

            return! Successful.OK j ctx
        }
        
    let getShipTypeStatsJson (shipStats: ShipStatsActor) (id: string) (ctx: HttpContext)=
        async {
                                
            let! stats = shipStats.GetShipTypeStats id
            
            let j = EntityTransforms.shipTypeStatsToJson stats
                        
            return! Successful.OK j ctx
            
        }
