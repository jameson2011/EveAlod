namespace EveAlod.Valuation

open FSharp.Data

// Deliberately simple: using sample json (file or literal) causes compilation failure & degraded IDE performance
type ShipStatisticsProvider = JsonProvider<""" { "id": 13 } """>

type KillmailHistoryIdProvider = JsonProvider<""" { "55594090": "4917cb90ac8e05191ed1d69608201c5551358bfa", "55589673": "c5f8d4f004e1014f9a1affd61b365cfb8392b37f" } """>

type DefaultJsonProvider = JsonProvider<""" {} """>
