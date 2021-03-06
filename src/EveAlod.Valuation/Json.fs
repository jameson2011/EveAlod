﻿namespace EveAlod.Valuation

open System
open FSharp.Data


module Json=
    
    let isValidJson json =
        try
            DefaultJsonProvider.Parse(json) |> ignore
            true
        with 
        | e -> false

    let toString (json: JsonValue) = json.ToString()

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
        j |> toString

    
    let valueStatisticsToJson prefix (value: ValueStatistics)=
        [
            (prefix + "Min", JsonValue.Float(value.MinValue));
            (prefix + "Max", JsonValue.Float(value.MaxValue));
            (prefix + "Range", JsonValue.Float(value.ValueRange));
            (prefix + "Average", JsonValue.Float(value.AverageValue));
            (prefix + "Median", JsonValue.Float(value.MedianValue));
            (prefix + "Total", JsonValue.Float(value.TotalValue)) 
        ]


    let shipTypeStatsToJson (stats: ShipTypeStatistics) =
        let summary prefix stats = 
            stats
            |> valueStatisticsToJson prefix
            |> List.ofSeq
        
        let toPeriod (period: DateTime) (total: PeriodValueStatistics) (fitted: PeriodValueStatistics option) =
            let totalValues = valueStatisticsToJson "total" total.Value
                                    |> List.append
                                    [
                                        ("date", JsonValue.String(period.ToString("o")));
                                        ("count", JsonValue.Float(float total.Value.Count));
                                    ] 
            let fittedValues = fitted |> Option.map (fun v -> valueStatisticsToJson "fitted" v.Value) |> Option.defaultValue []
            (fittedValues |> List.append totalValues |> Array.ofSeq) |> JsonValue.Record

        let gradientsToJson = Statistics.gradients
                                >> Seq.map (fun (g,v) -> (g.ToString("N2"), JsonValue.Float v))
                                >> Array.ofSeq
                                >> JsonValue.Record
                                
        let typeId = ("shipTypeId", JsonValue.String(stats.ShipId))
        let zkbUri = ("zkbHref", JsonValue.String stats.ZkbUri)
        let zkbApiUri = ("zkbApiHref", JsonValue.String stats.ZkbApiUri)
        let periods = stats.TotalValues     |> Seq.map (fun kvp -> kvp.Key, kvp.Value, (stats.FittedValues.TryFind kvp.Key) )
                                            |> Seq.sortByDescending (fun (k,_,_) -> k)
                                            |> Seq.map (fun (k,t,f) -> toPeriod k t f)
                                            |> Array.ofSeq        
    
        let summaryData = [ ("count", JsonValue.Float(float stats.FittedValuesSummary.Count)) ]
        let summaryData =   (if stats.FittedValuesSummary.Count > 0L then
                                (summary "total" stats.TotalValuesSummary)
                                |> List.append (summary "fitted" stats.FittedValuesSummary )
                                |> List.append summaryData
                            else
                                summaryData) |> Array.ofList
                              
        let summary = ("summary", JsonValue.Record summaryData )
        
        JsonValue.Record [| typeId; zkbUri; zkbApiUri; summary;
                            ( "totalGradients", gradientsToJson stats.TotalValuesSummary );
                            ("fittedGradients", gradientsToJson stats.FittedValuesSummary );
                            ("periods", (JsonValue.Array periods) ) |] |> toString
