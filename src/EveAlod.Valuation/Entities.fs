namespace EveAlod.Valuation

open System


type ValueStatistics=
    {
        Count: int64
        TotalValue: float
        AverageValue: float
        RollingAverageValue: float
    } with
    static member Empty = {
                            Count = 0L;
                            TotalValue = 0.;
                            AverageValue = 0.;
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

