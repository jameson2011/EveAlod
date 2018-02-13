namespace EveAlod.Valuation

open System
open EveAlod.Common
open EveAlod.Common.Web

type DataProvider()=

    let getData = httpClient() |> getData
    let shipStatsUri = sprintf "https://zkillboard.com/api/stats/shipTypeID/%s/"
    let statsAge = 12

    member __.ShipStatistics(shipTypeId: string)=
        async {
            let! response = shipTypeId |> shipStatsUri |> getData
            
            return  match response.Status with
                    | HttpStatus.OK ->  response.Message |> EntityTransforms.toShipStats statsAge
                    | _ ->              None                    
        }

