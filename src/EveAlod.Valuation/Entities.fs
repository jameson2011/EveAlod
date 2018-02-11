namespace EveAlod.Valuation

open System

type ValueStatistics=
    {
        Count: int64
        Value: float
        AverageValue: float
    } with
    static member Empty = {
                            Count = 0L;
                            Value = 0.;
                            AverageValue = 0.
                            }

type ShipValueStatistics=
    {
        Period: DateTime        
        Losses: ValueStatistics
        Kills: ValueStatistics        
    } with 
    static member Empty = {
                            Period = System.DateTime.MinValue;
                            Losses = ValueStatistics.Empty;
                            Kills = ValueStatistics.Empty
                            }    

type ShipStatistics=
    {
        ShipId: string
        Values: ShipValueStatistics []
    }

