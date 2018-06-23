namespace EveAlod.Valuation

open System
open EveAlod.Common
open EveAlod.Common.Json
open FSharp.Data


module EntityTransforms=    
    
    let toStatistics (count: string) (value: string) (json: JsonValue option)= 
        let count = json |> propInt64 count
        let value = json |> propFloat value
        let avg = match count with
                        | 0L -> 0.
                        | _ -> value / float count
        { ValueStatistics.Empty with Count = count; TotalValue = value; AverageValue = avg; }
    
    let toMonthlyShipStats (root: JsonValue)=
        let json = Some root
        match json |> (prop "year" <++> prop "month") with
        | Some year, Some month ->  let period = DateTime(  asInt year, asInt month, 1)                                        
                                    
                                    let toPeriods ships isk = toStatistics ships isk
                                                                >> (fun v -> { PeriodValueStatistics.Period = period; Value = v})
                                    
                                    let losses = json |> toPeriods "shipsLost" "iskLost"
                                    let kills =  json |> toPeriods "shipsDestroyed" "iskDestroyed"

                                    Some ( losses, kills)
        | _ -> None
           
    let rollingAverages age (stats: seq<PeriodValueStatistics>) =        
        stats   |> Seq.windowed age        
                |> Seq.map (fun s ->    let avg = s |> Seq.averageBy (fun s -> s.Value.AverageValue)                                        
                                        let lst = s |> Array.last                                
                                        { lst with Value = { lst.Value with RollingAverageValue = avg } } )        

    let toShipStats age json =
        let root = ShipStatisticsProvider.Parse(json).JsonValue |> Some
        let id = root |> prop "id"
        let periods = root
                        |> prop "months"
                        |> Option.map (fun j -> propValues j
                                                |> Seq.map toMonthlyShipStats
                                                |> Seq.mapSomes
                                                |> List.ofSeq)
        match id, periods with
        | Some id, Some periods -> 
            let losses, kills = periods |> Seq.splitTuples
            Some { ShipStatistics.Empty with ShipId = asStr id;
                                            Losses = losses |> rollingAverages age |> Array.ofSeq; 
                                            Kills = kills |> rollingAverages age |> Array.ofSeq}
        |  _ -> None

    let toKillmailIds json =
        match json with
        | "null" -> Some []
        | _ ->  let root = KillmailHistoryIdProvider.Parse(json)
        
                let ids = root.JsonValue.Properties()
                                |> Seq.map (fun (id,_) -> id)
                                |> List.ofSeq
                Some ids

    let toKill json =
        let package = DefaultJsonProvider.Parse(json).JsonValue.AsArray()
        match package with
        | [| root |] -> let root = Some root        
                        
                        match root |> propStr "killmail_id" with
                        | "" -> None
                        | id -> 
                            let shipTypeId = root |> prop "victim" |> propStr "ship_type_id"
                            let date = root |> propDateTime "killmail_time"
                            let fittedValue = root |> prop "zkb" |> propFloat "fittedValue"
                            let totalValue = root |> prop "zkb" |> propFloat "totalValue"                            

                            let r = { EveAlod.Data.Kill.empty with 
                                                Id = id; 
                                                VictimShip = Some { EveAlod.Data.Entity.Id = shipTypeId; Name = ""}
                                                Occurred = date; 
                                                FittedValue = fittedValue;
                                                TotalValue = totalValue}
                            Some r
        | _ -> None
        