namespace EveAlod.Valuation

open System
open EveAlod.Common
open EveAlod.Common.Combinators
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

    
    let shipSummaryStatsToJson (uri: string -> Uri) (stats: ShipSummaryStatistics)=        
        
        let shipTypeUri id =
            (sprintf "/stats/%s/" id |> uri).ToString()

        let j = JsonValue.Record [|
                                    ("totalKills", JsonValue.Float(float stats.TotalKills) );
                                    ("totalShipTypes", JsonValue.Float(float stats.ShipTypeCount) );                                                                        
                                    ("shipTypeHrefs", JsonValue.Array (stats.ShipTypeIds 
                                                                        |> Seq.sort 
                                                                        |> Seq.map (shipTypeUri >> JsonValue.String)
                                                                        |> Array.ofSeq));                                    
                                |]
        j.ToString()

    let shipTypeStatsToJson (stats: ShipTypeStatistics) =
            
        let valueToJson prefix (value: ValueStatistics)=

            [
            (prefix + "Min", JsonValue.Float(value.MinValue));
            (prefix + "Max", JsonValue.Float(value.MaxValue));
            (prefix + "Range", JsonValue.Float(value.ValueRange));
            (prefix + "Average", JsonValue.Float(value.AverageValue));
            (prefix + "Median", JsonValue.Float(value.MedianValue));
            (prefix + "Total", JsonValue.Float(value.TotalValue)) 
            ]

        let toPeriod (period: DateTime) (fitted: PeriodValueStatistics) (total: PeriodValueStatistics option) =
            let values = valueToJson "fitted" fitted.Value
                                    |> List.append
                                    [
                                        ("date", JsonValue.String(period.ToString("o")));
                                        ("count", JsonValue.Float(float fitted.Value.Count));
                                    ] 

            let totalValues = total |> Option.map (fun v -> valueToJson "total" v.Value) |> Option.defaultValue []

            (totalValues |> List.append values |> Array.ofSeq) |> JsonValue.Record
            
        let typeId = ("shipTypeId", JsonValue.String(stats.ShipId))
        let zkbUri = ("zkbHref", JsonValue.String stats.ZkbUri)
        let zkbApiUri = ("zkbApiHref", JsonValue.String stats.ZkbApiUri)
        let periods = stats.FittedValues    |> Seq.map (fun kvp -> kvp.Key, kvp.Value, (stats.TotalValues.TryFind kvp.Key) )
                                            |> Seq.sortByDescending (fun (k,_,_) -> k)
                                            |> Seq.map (fun (k,f,t) -> toPeriod k f t)
                                            |> Array.ofSeq        
        
        (JsonValue.Record [| typeId; zkbUri; zkbApiUri; ("periods", (JsonValue.Array periods) ) |]).ToString()

