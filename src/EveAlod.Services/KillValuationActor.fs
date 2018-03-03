namespace EveAlod.Services

open System
open EveAlod.Common
open EveAlod.Data
open EveAlod.Common.Web
open FSharp.Data
    
type private ValuationResponseProvider = JsonProvider<"""{ "total": 0.995524 }  """>

type KillValuationActor(config: Configuration, log: PostMessage)=
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
                                | HttpStatus.OK ->  ValuationResponseProvider.Parse(d.Message).Total
                                                    |> float
                                                    |> Choice1Of2
                                | x -> Choice2Of2 ("Error: " + x.ToString())
            with
            | e ->  logException e
                    return Choice2Of2 ("Error: " + e.Message)
        }

    let getValuation kill = 
        match getUri kill with
        | Some uri -> getValue uri                    
        | _ -> async { return Choice2Of2 "No URI" }

    
    let logKillScore (kill: Kill) score =
        sprintf "Kill: %s ShipType: %s TotalValue: %f Score: %f" 
            kill.Id kill.VictimShip.Value.Id kill.TotalValue score
        |> logInfo
        
    let forwardValuation (ch: AsyncReplyChannel<float>) kill valuation = 
        match valuation with
            | Choice1Of2 valuation ->   logKillScore kill valuation
                                        valuation |> ch.Reply                    
            | Choice2Of2 text ->        text |> logTrace
    

    let pipe = MailboxProcessor<Kill * AsyncReplyChannel<float>>.Start(fun inbox ->
        let rec loop() = async {
            
            let! kill,ch = inbox.Receive()
            
            let! valuation = getValuation kill

            forwardValuation ch kill valuation
            
            return! loop()
            }
        loop()
        )

    do pipe.Error.Add(logException)

    member __.Value(kill: Kill)=
        // TOOD:
        0.1