namespace EveAlod.Valuation

open System
open EveAlod.Common
open EveAlod.Common.Json
open EveAlod.Common.Web

type DataProvider()=

    let getData = httpClient() |> getData
    let uriDate (date: DateTime) = date.ToString("yyyyMMdd")        
    let historyUri = sprintf "https://zkillboard.com/api/history/%s/"
    let shipStatsUri = sprintf "https://zkillboard.com/api/stats/shipTypeID/%s/"
    let statsAge = 12

    
    member __.ShipStatistics(shipTypeId: string)=
        async {
            let! resp = shipTypeId |> shipStatsUri |> getData
            
            return  match resp.Status with
                    | HttpStatus.OK ->  resp.Message |> EntityTransforms.toShipStats statsAge
                    | _ ->              None                    
        }
    
    member __.KillIds(date: DateTime)= 
        async {            
            let! resp = date |> uriDate |> historyUri |> getData 

            return match resp.Status with
                    | HttpStatus.OK -> resp.Message |> EntityTransforms.toKillmailIds
                    | _ -> None            
        }