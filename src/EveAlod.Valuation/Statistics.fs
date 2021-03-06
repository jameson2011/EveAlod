﻿namespace EveAlod.Valuation

open System

module Statistics=
    
    let accumulateCount (stats: ValueStatistics)= 
        { stats with Count = stats.Count + 1L }

    let accumulateValue value (stats: ValueStatistics) = 
        { stats with TotalValue = stats.TotalValue + value }

    let accumulateMinMax value (stats: ValueStatistics) = 
        let minV = min stats.MinValue value
        let maxV = max stats.MaxValue value
        let medV = ((maxV - minV) / 2.) + minV
        { stats with MinValue = minV;
                     MaxValue = maxV; 
                     ValueRange = maxV - minV;
                     MedianValue = medV}

    let accumulateAverage (stats: ValueStatistics)  = 
        { stats with AverageValue = stats.TotalValue / float stats.Count }


    let append (stats: Map<DateTime, PeriodValueStatistics>) period value =
        stats.Add(period, { PeriodValueStatistics.Period = period; Value = value } )

    let accumulate (stats: Map<DateTime, PeriodValueStatistics>) period value =
        let periodStats, valueStats = match stats.TryFind(period) with
                                        | None -> { PeriodValueStatistics.Empty with Period = period }, ValueStatistics.Empty
                                        | Some s -> s, s.Value
                                    
        let valueStats = valueStats 
                            |> accumulateCount 
                            |> accumulateValue value 
                            |> accumulateMinMax value 
                            |> accumulateAverage
        
        stats.Add(period, { periodStats with Value = valueStats }), valueStats


    let trimMap maxAge (stats: Map<_,_>)  = 
        let rec trimPeriods depth (stats: Map<_,_>) (date: DateTime) =
            match depth with
            | 0 ->  stats
            | _ ->  let k = date.AddDays(-1.)
                    trimPeriods (depth - 1) (stats.Remove(k)) k        
        DateTime.UtcNow.Date.AddDays(float -(maxAge+1)) |> trimPeriods 10 stats 
        
    let trim maxAge (stats: ShipTypeStatistics)=
        { stats with FittedValues = (trimMap maxAge stats.FittedValues);
                     TotalValues = (trimMap maxAge stats.TotalValues);
        }

    let totals (map: Map<_,PeriodValueStatistics>) =
        let stats = map |> Seq.map (fun k -> k.Value.Value) |> Array.ofSeq
        let min = stats |> Seq.map (fun s -> s.MinValue) |> Seq.min
        let max = stats |> Seq.map (fun s -> s.MaxValue) |> Seq.max
        let total = stats |> Seq.sumBy (fun s -> s.TotalValue)
        let count = stats |> Seq.sumBy (fun s -> s.Count)
       
        { ValueStatistics.Empty with 
            Count = count; 
            MinValue = min; 
            MaxValue = max; 
            ValueRange = max - min;            
            TotalValue = total; 
            AverageValue = total  / float count;
            MedianValue = (max - min ) / 2. + min}
            
    let rollup period fittedValue totalValue (stats: ShipTypeStatistics) = 
        let fittedValues, fittedValue = accumulate stats.FittedValues period fittedValue
        let totalValues, totalValue = accumulate stats.TotalValues period totalValue
        
        { stats with    FittedValues = fittedValues;
                        FittedValuesSummary = totals fittedValues
                        TotalValues = totalValues; 
                        TotalValuesSummary = totals totalValues }, fittedValue, totalValue

    let appendRollup period (fittedValue: ValueStatistics) (totalValue: ValueStatistics) (stats: ShipTypeStatistics) = 
        let fittedValues = append stats.FittedValues period fittedValue
        let totalValues = append stats.TotalValues period totalValue

        { stats with    FittedValues = fittedValues;
                        FittedValuesSummary = totals fittedValues
                        TotalValues = totalValues; 
                        TotalValuesSummary = totals totalValues }

    let valuation (stats: ValueStatistics) value =  
        
        if stats.Count = 0L || value <= 0. then    
            0.
        else
            let value = value  |> max stats.MinValue |> min stats.MaxValue    
            match stats.ValueRange with
            | 0. -> 0.
            | x -> (value - stats.MinValue ) / x

    let medianMeanSpread (stats: ValueStatistics) =  
        match stats.AverageValue, stats.MedianValue with
        | 0., 0. -> 0.
        | avg, median -> median / avg

    let gradients (stats: ValueStatistics) =
        let grad = match stats.Count with       
                    | 0L -> (fun x -> x, 0. )
                    | _ -> (fun x -> x, (x * stats.ValueRange) + stats.MinValue )
        [ 0.1 .. 0.1 .. 1.0] 
        |> Seq.map grad
