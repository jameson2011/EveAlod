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
        let root = ShipStatisticsProvider.Parse(json)
        let monthStats = root.JsonValue |> Some |> prop "months"
                        |> Option.map (fun j -> j.Properties() 
                                                |> Seq.map (fun (_,v) -> v |> Some |> toMonthlyShipStats)
                                                |> Array.ofSeq)
        monthStats 
        |> Option.map (fun stats -> { ShipStatistics.ShipId = root.Id.ToString(); 
                                    Values = stats })
           
    let latestLosses age (shipStats: ShipStatistics) =
        shipStats.Values 
        |> Seq.rev
        |> Seq.map (fun s -> (s.Period, s.Losses.AverageValue) )
        |> Seq.truncate age 
        |> Map.ofSeq



