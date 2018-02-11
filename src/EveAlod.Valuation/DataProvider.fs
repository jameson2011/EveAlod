namespace EveAlod.Valuation

open System
open EveAlod.Common
open EveAlod.Common.Web

type DataProvider()=
    let client = httpClient()
    let getData = getData client

    let shipStatsUri = 
        sprintf "https://zkillboard.com/api/stats/shipTypeID/%s/"

    let getShipStats (id: string) =
        async {
            let! r = id |> shipStatsUri |> getData
            
            let x =     match r.Status with
                        | HttpStatus.OK -> 
                            let json = r.Message
                            json |> EntityTransforms.toShipStats 
                        | _ ->
                            None

            return x
        }

    member __.ShipStatistics(id: string)=
        getShipStats id