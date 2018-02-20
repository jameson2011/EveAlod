namespace EveAlod.Valuation

open EveAlod.Common
open EveAlod.Common.Json
open EveAlod.Data



type ShipTypeStatsActor(config: ValuationConfiguration, log: PostMessage, shipTypeId: string)=

    let parse json =
        let package = json |> KillTransforms.asKillPackage
        let zkb = package |> prop "zkb"
        let fittedValue = zkb |> prop "fittedValue" |> Option.map asFloat
        let totalValue = zkb |> prop "totalValue" |> Option.map asFloat
        let killDate = package |> prop "killmail" |> prop "killmail_time" |> Option.map (asDateTime >> DateTime.date)
        match fittedValue, totalValue, killDate with
        | Some fittedValue, Some totalValue, Some killDate -> Some (fittedValue, totalValue, killDate)
        | _ -> None

    
    let pipe = MessageInbox.Start(fun inbox -> 
        
        let rec loop(stats: ShipTypeStatistics) = async {                
            let! msg = inbox.Receive()
            
            let stats = match msg with
                        | ImportKillJson json ->                 
                            match parse json with
                            | Some (fittedValue, totalValue, killDate) ->   
                                // TODO: trim
                                Statistics.rollup stats killDate fittedValue totalValue 
                            | _ ->                                          stats
                        | GetShipTypeStats (_,ch) -> 
                            stats |> ch.Reply
                            stats
                        | _ ->                                              stats
            
            return! loop(stats)
        }
        loop({ ShipTypeStatistics.Empty with ShipId = shipTypeId })
        )

    do pipe.Error.Add(Actors.postException typeof<ShipTypeStatsActor>.Name log)

    member __.Post(msg: ValuationActorMessage) = pipe.Post msg