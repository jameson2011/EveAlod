namespace EveAlod.Services

open System
open EveAlod.Common
open EveAlod.Data
open EveAlod.Common.Web
open FSharp.Data
    
type private ValuationResponseProvider = JsonProvider<"""{ "total": 0.995524, "spread": 177.963267 }  """>

type KillValuationActor(config: Configuration, log: PostMessage, forward: PostKill)=
    let logException = Actors.postException typeof<KillValuationActor>.Name log
    let logTrace = Actors.postTrace typeof<KillValuationActor>.Name log
    let logInfo = Actors.postInfo log
    let http = httpClient()    
    let getData = Web.getData http

    let getUri (kill: Kill) =
        kill.VictimShip |> Option.map (fun ship -> sprintf "%svaluation/%s/%f/" config.KillValuationUri ship.Id kill.TotalValue)        

    let getValue uri =
        async {
            try
                let! d = getData uri
                return match d.Status with
                                | HttpStatus.OK ->  let valuation = ValuationResponseProvider.Parse(d.Message)
                                                    (float valuation.Total, float valuation.Spread)
                                                    |> Choice1Of2
                                | _ -> Choice2Of2 ("Error: " + d.Message)
            with
            | e ->  logException e
                    return Choice2Of2 ("Error: " + e.Message)
        }

    let getValuation kill = 
        match getUri kill with
        | Some uri -> getValue uri                    
        | _ -> async { return Choice2Of2 "No URI" }

    
    let logKillScore (kill: Kill) score spread =
        sprintf "Kill: %s ShipType: %s TotalValue: %f Score: %f Spread: %f" 
            kill.Id kill.VictimShip.Value.Id kill.TotalValue score spread
        |> logTrace
        
    let forwardValuation kill valuation = 
        match valuation with
            | Choice1Of2 (valuation, spread) ->   
                                        logKillScore kill valuation spread
                                        forward { kill with TotalValueValuation = Some valuation;
                                                            TotalValueSpread = Some spread }
            | Choice2Of2 text ->        text |> logTrace
                                        forward kill
    

    let pipe = MessageInbox.Start(fun inbox ->
        let rec loop() = async {
            
            let! msg = inbox.Receive()
            match msg with
            | Killmail kill ->  let! valuation = getValuation kill
                                forwardValuation kill valuation
            | _ -> ignore 0
            return! loop()
            }
        loop()
        )

    do pipe.Error.Add(logException)

    member __.Post(msg: ActorMessage) = pipe.Post msg