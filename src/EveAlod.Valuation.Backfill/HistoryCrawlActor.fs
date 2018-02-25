namespace EveAlod.Valuation.Backfill

open System
open EveAlod.Valuation
open EveAlod.Data


type HistoryCrawlActor(log: PostMessage, config: BackfillConfiguration)=

    let dp = DataProvider()
    let dates = HistoryCrawl.dates config.From config.To |> List.ofSeq

    let logException e = ActorMessage.Exception (typeof<HistoryCrawlActor>.Name, e) |> log
    let logInfo = ActorMessage.Info >> log

    
    let postKill kill =        
        match HistoryCrawl.toJson kill with
        | Some json ->  // TODO:
                        ignore 0
                        //config.DestinationUri
        | None ->       ignore 0

    let crawlDate (date: DateTime) =
        async {
            try
                date.ToShortDateString() |> sprintf "Fetching kills for %s" |> logInfo
                let! ids = dp.KillIds date

                ids |> Option.map (HistoryCrawl.randomSamples config.Sampling
                                    >> HistoryCrawl.crawlKills logInfo logException dp.Kill postKill
                                    >> Async.RunSynchronously) |> ignore 
            with 
                | e -> logException e
            }
    
    member __.Run()=
        let rec run dates =
            match dates with
            | [] -> async { ignore 0 }
            | date::t ->  async {
                            do! crawlDate date
                            do! run t
                        }        
        run dates


