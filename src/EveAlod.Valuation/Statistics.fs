namespace EveAlod.Valuation

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



    let accumulate (stats: Map<DateTime, PeriodValueStatistics>) period value =
        let periodStats, valueStats = match stats.TryFind(period) with
                                        | None -> { PeriodValueStatistics.Empty with Period = period }, ValueStatistics.Empty
                                        | Some s -> s, s.Value
                                    
        let valueStats = valueStats 
                            |> accumulateCount 
                            |> accumulateValue value 
                            |> accumulateMinMax value 
                            |> accumulateAverage
        
        stats.Add(period, { periodStats with Value = valueStats })


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
        
    let rollup period fittedValue totalValue (stats: ShipTypeStatistics) = 
        { stats with    FittedValues = accumulate stats.FittedValues period fittedValue;
                        TotalValues = accumulate stats.TotalValues period totalValue }