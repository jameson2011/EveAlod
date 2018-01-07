namespace EveAlod.Services

    open EveAlod.Data
    open System.Globalization

    type LogPublishActor()= 

        let getMsg km = 
            let tags km = 
                let s = km.Tags 
                            |> Seq.map (fun t -> t.ToString())
                System.String.Join(", ", s)                

            sprintf "%s Score: %.2f %s Tags: %s" (km.Occurred.ToString(CultureInfo.InvariantCulture)) km.AlodScore km.ZkbUri (tags km)
        
        let logger = log4net.LogManager.GetLogger(typeof<LogPublishActor>)

        let logInfo (msg: string) = logger.Info(msg)
            
        let logWarn (msg: string) = logger.Warn(msg)

        let logError (msg: string) = logger.Error(msg)

        let logException (source: string) (ex: System.Exception) = logger.Error(source, ex)

        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with
                | Log km ->                 getMsg km |> logInfo
                | Warning (source,msg) ->   (source + ": " + msg) |> logWarn
                | Error (source, msg) ->    (source + ": " + msg) |> logError
                | Exception (source, ex) -> logException source ex               
                | Info msg ->               msg |> logInfo
                | _ -> ignore 0
                
                return! getNext()            
                }
        
            getNext()
        )

        
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg


