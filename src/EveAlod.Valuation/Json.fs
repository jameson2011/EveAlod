namespace EveAlod.Valuation

open System
open EveAlod.Common
open EveAlod.Common.Combinators
open EveAlod.Common.Json
open FSharp.Data


module Json=
    
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
        
        let toPeriod (period: DateTime) (fitted: PeriodValueStatistics) (total: PeriodValueStatistics option) =
            let values = valueStatisticsToJson "fitted" fitted.Value
                                    |> List.append
                                    [
                                        ("date", JsonValue.String(period.ToString("o")));
                                        ("count", JsonValue.Float(float fitted.Value.Count));
                                    ] 

            let totalValues = total |> Option.map (fun v -> valueStatisticsToJson "total" v.Value) |> Option.defaultValue []

            (totalValues |> List.append values |> Array.ofSeq) |> JsonValue.Record
            
        let typeId = ("shipTypeId", JsonValue.String(stats.ShipId))
        let zkbUri = ("zkbHref", JsonValue.String stats.ZkbUri)
        let zkbApiUri = ("zkbApiHref", JsonValue.String stats.ZkbApiUri)
        let periods = stats.FittedValues    |> Seq.map (fun kvp -> kvp.Key, kvp.Value, (stats.TotalValues.TryFind kvp.Key) )
                                            |> Seq.sortByDescending (fun (k,_,_) -> k)
                                            |> Seq.map (fun (k,f,t) -> toPeriod k f t)
                                            |> Array.ofSeq        
    
        let summaryData = [ ("count", JsonValue.Float(float stats.FittedValuesSummary.Count)) ]
        let summaryData =   (if stats.FittedValuesSummary.Count > 0L then
                                (summary "total" stats.TotalValuesSummary)
                                |> List.append (summary "fitted" stats.FittedValuesSummary )
                                |> List.append summaryData
                            else
                                summaryData) |> Array.ofList
                              
        let summary = ("summary", JsonValue.Record summaryData )
        (JsonValue.Record [| typeId; zkbUri; zkbApiUri; summary;
                            ("periods", (JsonValue.Array periods) ) |]).ToString()
