namespace EveAlod.Valuation

open System

// TODO: name
type Statistics=
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

type ShipKillStatistics=
    {
        Period: DateTime        
        Losses: Statistics
        Kills: Statistics        
    } with 
    static member Empty = {
                            Period = System.DateTime.MinValue;
                            Losses = Statistics.Empty;
                            Kills = Statistics.Empty
                            }

type ShipStatistics=
    {
        ShipId: string
        Kills: ShipKillStatistics list
    }



