namespace EveAlod.Valuation

open System
open EveAlod.Common
open EveAlod.Common.Web


type DataProvider(host: string,  statsAge: int)=

    let getData = httpClient() |> getData
    let uriDate (date: DateTime) = date.ToString("yyyyMMdd")        
    let historyUri = sprintf "https://%s/api/history/%s/" host
    let shipStatsUri = sprintf "https://%s/api/stats/shipTypeID/%s/" host
    let killUri = sprintf "https://%s/api/killID/%s/" host
    
    new(host) = DataProvider(host, 12)
    new() = DataProvider("zkillboard.com", 12)
    
    member __.ShipStatistics(shipTypeId: string)=
        async {
            let! resp = shipTypeId |> shipStatsUri |> getData
            
            return resp |> EntityWebResponse.ofWebResponse (EntityTransforms.toShipStats statsAge)            
        }
    
    member __.KillIds(date: DateTime)= 
        async {            
            let! resp = date |> uriDate |> historyUri |> getData 

            return resp |> EntityWebResponse.ofWebResponse (EntityTransforms.toKillmailIds)            
        }

    member __.Kill(id: string) = 
        async {            
            let! resp = id |> killUri |> getData
            return resp |> EntityWebResponse.ofWebResponse (EntityTransforms.toKill)
        }