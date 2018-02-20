namespace EveAlod.Valuation

open EveAlod.Common
open EveAlod.Common.Json
open EveAlod.Data

type ShipTypeStatsActor(log: PostMessage, shipTypeId: string)=

    let fittedValue = 
        KillTransforms.asKillPackage >> prop "zkb" >> prop "fittedValue" >> Option.map asFloat

    let killDate = 
        KillTransforms.asKillPackage >> prop "killmail" >> prop "killmail_time" >> Option.map (asDateTime >> DateTime.date)

    let pipe = MessageInbox.Start(fun inbox -> 
        let rec loop() = async {                
            let! msg = inbox.Receive()
            match msg with
            | ImportKillJson json ->                 
                match fittedValue json, killDate json with
                | Some fittedValue, Some killDate -> ignore 0 // TODO: accumulate...
                | _ -> ignore 0
            | _ -> ignore 0
            return! loop()
        }
        loop()
        )

    do pipe.Error.Add(Actors.postException typeof<ShipTypeStatsActor>.Name log)

    member __.Post(msg: ValuationActorMessage) = pipe.Post msg