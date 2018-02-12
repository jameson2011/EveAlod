namespace EveAlod.Valuation

open System
open EveAlod.Common.Json
open FSharp.Data


module EntityTransforms=
    
    let toStatistics (count: string) (value: string) (json: JsonValue option)= 
        let count = json |> propInt64 count
        let value = json |> propFloat value
        let avg = match count with
                        | 0L -> 0.
                        | _ -> value / float count
        { ValueStatistics.Count = count; Value = value; AverageValue = avg }        

    let toMonthlyShipStats (json: JsonValue option)=
        
        let period = DateTime(  json |> propInt "year",
                                json |> propInt "month",
                                1)                                
        let losses = json  |> toStatistics "shipsLost" "iskLost"
        let kills = json |> toStatistics "shipsDestroyed" "iskDestroyed"

        { ShipValueStatistics.Period = period;
            Kills = kills;
            Losses = losses }
        

    let toShipStats json =
        let root = ShipStatisticsProvider.Parse(json).JsonValue |> Some
        let id = root |> prop "id"
        let months = root
                        |> prop "months"
                        |> Option.map (fun j -> j.Properties() |> Seq.map (fun (_,v) -> v |> Some))
        match id, months with
        | Some id, Some months -> 
            Some { ShipStatistics.ShipId = asStr id; 
                                Values = months |> Seq.map toMonthlyShipStats |> Array.ofSeq }
        |  _ -> None

        
           
    let latestLosses age (shipStats: ShipStatistics) =
        shipStats.Values 
        |> Seq.rev
        |> Seq.map (fun s -> (s.Period, s.Losses.AverageValue) )
        |> Seq.truncate age 
        |> Map.ofSeq



