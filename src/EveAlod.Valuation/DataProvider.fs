namespace EveAlod.Valuation

open System
open EveAlod.Common
open EveAlod.Common.Json
open EveAlod.Common.Web

type private KillmailHistoryIdProvider = FSharp.Data.JsonProvider<"./SampleKillHistory.json">

type DataProvider()=

    let getData = httpClient() |> getData
    let uriDate (date: DateTime) = date.ToString("yyyyMMdd")        
    let historyUri = sprintf "https://zkillboard.com/api/history/%s/"
    let shipStatsUri = sprintf "https://zkillboard.com/api/stats/shipTypeID/%s/"
    let statsAge = 12

    
    let getDayKillmailIds (date) =        
        async {            
            let! resp = date |> uriDate |> historyUri |> getData 
            let root = match resp.Status with
                        | HttpStatus.OK -> KillmailHistoryIdProvider.Parse(resp.Message)
                        | s -> s.ToString() |> failwith // TODO:
            // TODO:
            let ids = root.JsonValue

            return []
        }

    member __.ShipStatistics(shipTypeId: string)=
        async {
            let! response = shipTypeId |> shipStatsUri |> getData
            
            return  match response.Status with
                    | HttpStatus.OK ->  response.Message |> EntityTransforms.toShipStats statsAge
                    | _ ->              None                    
        }

    member __.KillIds(date: DateTime)= getDayKillmailIds date