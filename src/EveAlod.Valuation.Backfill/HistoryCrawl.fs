namespace EveAlod.Valuation.Backfill

open System
open EveAlod.Data

module HistoryCrawl=
    open EveAlod.Valuation

    let dates (start: DateTime) (finish: DateTime) =
        let range = finish - start

        [ 0 .. range.Days ] |> Seq.rev |> Seq.map (float >> start.AddDays)
        
    let historicalKillIds (dp: DataProvider) dates =
        dates |> Seq.map dp.KillIds 

    let randomSamples chance values  = 
        let rng = new Random()
        let take(_) = chance >= rng.NextDouble()
        values |> Seq.filter take |> List.ofSeq

    let crawlKills logInfo logException get post ids  = 
        let rec crawl (ids: string list)  =        
            match ids with
            | [] ->     async { ignore 0 }
            | id::t ->  async {
                            try 
                                do! Async.Sleep(100) 
                                id |> sprintf "Fetching kill %s" |> logInfo
                                let! k = get id
                                k |> Option.map post |> ignore
                            with 
                            | e -> logException e

                            return! (crawl t)
                        }                                    
        crawl ids