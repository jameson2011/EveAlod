namespace EveAlod.Valuation.Backfill

open System
open EveAlod.Valuation
open EveAlod.Common.Web
open EveAlod.Data
open EveAlod.Common


type HistoryCrawlActor(log: PostMessage, config: BackfillConfiguration)=

    let dp = DataProvider()
    let dates = HistoryCrawl.dates config.From config.To |> List.ofSeq

    let logException e = ActorMessage.Exception (typeof<HistoryCrawlActor>.Name, e) |> log
    let logError msg = ActorMessage.Error (typeof<HistoryCrawlActor>.Name, msg) |> log
    let logInfo = ActorMessage.Info >> log

    let httpClient = httpClient()
    

    let postKill kill =        
        let send json = let uri = (config.DestinationUri.ToString())
                        let resp = postData httpClient uri json |> Async.RunSynchronously
                        match resp.Status with 
                        | HttpStatus.OK -> None
                        | s ->  sprintf "%s sending to %s: %s" (s.ToString()) uri resp.Message  |> Some
        
        kill    |> HistoryCrawl.toJson 
                |> Option.bind send 
                |> Option.map logError
                |> Option.defaultValue (ignore 0)

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


