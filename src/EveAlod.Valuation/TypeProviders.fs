namespace EveAlod.Valuation

open FSharp.Data

// Deliberately simple: using sample json (file or literal) causes compilation failure & degraded IDE performance
type ShipStatisticsProvider = JsonProvider<""" { "id": 13 } """>
