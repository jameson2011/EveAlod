namespace EveAlod.Valuation.Backfill

open System
open EveAlod.Valuation
open EveAlod.Data
open EveAlod.Common.Web


type HistoryCrawlActor(log: PostMessage, config: BackfillConfiguration)=

    let dp = DataProvider()
    let dates = HistoryCrawl.dates config.From config.To |> List.ofSeq

    let logException e = ActorMessage.Exception (typeof<HistoryCrawlActor>.Name, e) |> log
    let logInfo = ActorMessage.Info >> log

    
    let postKill kill =
        // TODO:        
        //config.DestinationUri
        ignore kill

    let dateInbox = MailboxProcessor<DateTime>.Start(fun inbox -> 
        let rec loop() = async {        
            let! date = inbox.Receive()
            do! Async.Sleep(100)
            try
                date.ToShortDateString() |> sprintf "Fetching kills for %s" |> logInfo
                let! ids = dp.KillIds date
                
                ids |> Option.map (HistoryCrawl.randomSamples config.Sampling
                                    >> HistoryCrawl.crawlKills logInfo logException dp.Kill postKill
                                    >> Async.RunSynchronously) |> ignore                
            with 
                | e -> logException e

            return! loop()
            }
        loop()
        )
    
    do dateInbox.Error.Add(Actors.postException typeof<HistoryCrawlActor>.Name log)
    
    member __.Start()=
        dates |> Seq.rev |> Seq.iter dateInbox.Post
        // TODO: signify this is finished


