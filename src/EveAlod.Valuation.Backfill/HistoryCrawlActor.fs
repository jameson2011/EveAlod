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
    let infoMsg = ActorMessage.Info
    let debugMsg msg = ActorMessage.Trace(typeof<HistoryCrawlActor>.Name, msg)
    let errorMsg msg = ActorMessage.Error (typeof<HistoryCrawlActor>.Name, msg)
    let logError = errorMsg >> log
    let logInfo = ActorMessage.Info >> log

    let httpClient = httpClient()
    

    let postKill kill =        
        let send id json = 
            let uri = (config.DestinationUri.ToString())
            let resp = postData httpClient uri json |> Async.RunSynchronously
            match resp.Status with 
            | HttpStatus.OK -> sprintf "Sent kill %s" id |> debugMsg 
            | s ->  sprintf "%s sending to %s: %s" (s.ToString()) uri resp.Message |> errorMsg 
                        
        
        let j = kill |> HistoryCrawl.toJson 
        match j with
        | Some json -> json |> send kill.Id |> log
        | _ -> sprintf "Unrecognised kill" |> errorMsg |> log
                
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


