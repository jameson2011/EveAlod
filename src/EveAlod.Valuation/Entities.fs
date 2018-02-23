namespace EveAlod.Valuation

open System


type ValueStatistics=
    {
        Count: int64
        TotalValue: float
        AverageValue: float
        MinValue: float
        MaxValue: float
        ValueRange: float
        MedianValue: float
        RollingAverageValue: float
    } with
    static member Empty = {
                            Count = 0L;
                            TotalValue = 0.;
                            AverageValue = 0.;
                            MedianValue = 0.;
                            MinValue = System.Double.MaxValue;
                            MaxValue = System.Double.MinValue;
                            ValueRange = 0.;
                            RollingAverageValue = 0.
                            }
                                                        
type PeriodValueStatistics=
    {
        Period: DateTime        
        Value: ValueStatistics
    } with 
    static member Empty = {
                            Period = System.DateTime.MinValue;
                            Value = ValueStatistics.Empty;
                            }    
                            
type ShipStatistics=
    {
        ShipId: string
        Losses: PeriodValueStatistics []
        Kills:  PeriodValueStatistics []
    } with
    static member Empty = {
                            ShipId= "";
                            Losses = Array.empty;
                            Kills = Array.empty
                            }

type ShipTypeStatistics=
    {
        ShipId: string
        FittedValues: Map<DateTime, PeriodValueStatistics>
        TotalValues: Map<DateTime, PeriodValueStatistics>
    } with 
    static member Empty = { ShipId = ""; FittedValues = Map.empty; TotalValues = Map.empty }

type ShipSummaryStatistics =
    {
        ShipTypeCount: int
        TotalKills: int64
        ShipTypeIds: Set<string>
    } with
    static member Empty = { ShipTypeCount = 0; TotalKills = 0L; ShipTypeIds = Set.empty }
