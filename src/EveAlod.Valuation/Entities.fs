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
        ZkbUri: string
        ZkbApiUri: string
        FittedValues: Map<DateTime, PeriodValueStatistics>
        FittedValuesSummary: ValueStatistics
        TotalValues: Map<DateTime, PeriodValueStatistics>
        TotalValuesSummary: ValueStatistics
    } with 
    static member Empty = { ShipId = ""; ZkbUri = ""; ZkbApiUri = ""; 
                            FittedValues = Map.empty; FittedValuesSummary = ValueStatistics.Empty;
                            TotalValues = Map.empty; TotalValuesSummary = ValueStatistics.Empty }

type ShipTypePeriodStatistics =
    {
        ShipTypeId: string
        Period: DateTime
        Fitted: ValueStatistics
        Total: ValueStatistics
    } with
    static member Empty = { ShipTypeId = ""; Period = DateTime.MinValue; 
                            Fitted = ValueStatistics.Empty;
                            Total = ValueStatistics.Empty }

type ShipSummaryStatistics =
    {
        ShipTypeCount: int
        TotalKills: int64
        ShipTypeIds: Set<string>
    } with
    static member Empty = { ShipTypeCount = 0; TotalKills = 0L; ShipTypeIds = Set.empty }
