namespace EveAlod.Valuation

open EveAlod.Common
open EveAlod.Common.Json
open EveAlod.Data



type ShipTypeStatsActor(config: ValuationConfiguration, log: PostMessage, shipTypeId: string)=

    let logException e = ActorMessage.Exception (typeof<ShipTypeStatsActor>.Name, e) |> log
    let logInfo = ActorMessage.Info >> log

    let parse json =
        let package = json |> KillTransforms.asKillPackage
        let zkb = package |> prop "zkb"
        let fittedValue = zkb |> prop "fittedValue" |> Option.map asFloat
        let totalValue = zkb |> prop "totalValue" |> Option.map asFloat
        let killDate = package |> prop "killmail" |> prop "killmail_time" |> Option.map (asDateTime >> DateTime.date)
        match fittedValue, totalValue, killDate with
        | Some fittedValue, Some totalValue, Some killDate -> Some (fittedValue, totalValue, killDate)
        | _ -> None

    let isMinAge dt = 
        dt >= System.DateTime.UtcNow.Date.AddDays(float -config.MaxRollingStatsAge)

    let pipe = MessageInbox.Start(fun inbox -> 
        
        let rec loop(stats: ShipTypeStatistics) = async {                
            let! msg = inbox.Receive()
            
            let stats = try
                            match msg with
                            | ImportKillJson json ->                 
                                match parse json with
                                | Some (fittedValue, totalValue, killDate) when isMinAge killDate -> 
                                    stats   |> Statistics.trim config.MaxRollingStatsAge
                                            |> Statistics.rollup killDate fittedValue totalValue 
                                | _ -> stats
                            | GetShipTypeStats (_,ch) -> 
                                stats |> ch.Reply
                                stats
                            | GetShipSummaryStats ch -> 
                                let count = stats.FittedValues |> Seq.sumBy (fun kvp -> kvp.Value.Value.Count)
                                { ShipSummaryStatistics.Empty with 
                                    TotalKills = count } |> ch.Reply
                                stats
                        with
                        | e ->  logException e
                                stats
            
            return! loop(stats)
        }
        loop({ ShipTypeStatistics.Empty 
                with    ShipId = shipTypeId; 
                        ZkbUri = (sprintf "https://zkillboard.com/ship/%s/" shipTypeId);
                        ZkbApiUri = (sprintf "https://zkillboard.com/api/stats/shipTypeID/%s/" shipTypeId)})
        )

    do pipe.Error.Add(Actors.postException typeof<ShipTypeStatsActor>.Name log)

    member __.Post(msg: ValuationActorMessage) = pipe.Post msg
    
    member __.GetStatsSummary() =
        pipe.PostAndAsyncReply ValuationActorMessage.GetShipSummaryStats

    member __.GetStats() =
        pipe.PostAndAsyncReply (fun ch -> ValuationActorMessage.GetShipTypeStats (shipTypeId,ch) )
