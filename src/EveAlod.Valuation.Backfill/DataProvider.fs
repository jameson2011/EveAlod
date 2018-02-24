namespace EveAlod.Valuation.Backfill

open System

type DataProvider()=
    
    let uriDate (date: DateTime) = date.ToString("yyyyMMdd")
        
    let historyUri = sprintf "https://zkillboard.com/api/history/%s/"

    