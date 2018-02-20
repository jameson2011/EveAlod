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
        // TODO: ShipTypeStatistics
        let rec loop() = async {                
            let! msg = inbox.Receive()
            match msg with
            | ImportKillJson json ->                 
                match parse json with
                | Some (fittedValue, totalValue, killDate) -> ignore 0 // TODO: accumulate...
                | _ -> ignore 0
            | _ -> ignore 0
            return! loop()
        }
        loop()
        )

    do pipe.Error.Add(Actors.postException typeof<ShipTypeStatsActor>.Name log)

    member __.Post(msg: ValuationActorMessage) = pipe.Post msg