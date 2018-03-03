namespace EveAlod.Services

    open System.Globalization
    open EveAlod.Common
    open EveAlod.Data    

    type LogPublishActor(configFile: string)= 
        
        do log4net.Config.XmlConfigurator.Configure(System.IO.FileInfo(configFile)) |> ignore

        let getMsg km = 
            let tags km = 
                km.Tags 
                    |> Seq.map (fun t -> t.ToString())
                    |> Strings.join ", "
            let valuation km = 
                match km.TotalValueValuation with
                | Some x -> sprintf "%.2f" x
                | _ -> "None"

            sprintf "%s Score: %.2f Valuation: %s %s Tags: %s" 
                (km.Occurred.ToString(CultureInfo.InvariantCulture)) 
                km.AlodScore (valuation km)
                km.ZkbUri (tags km)
        
        let logger = log4net.LogManager.GetLogger(typeof<LogPublishActor>)

        let logInfo (msg: string) = logger.Info(msg)
            
        let logTrace (msg: string) = logger.Debug(msg)

        let logWarn (msg: string) = logger.Warn(msg)

        let logError (msg: string) = logger.Error(msg)

        let logException (source: string) (ex: System.Exception) = logger.Error(source, ex)

        let onException = logException typeof<LogPublishActor>.Name 

        let pipe = MessageInbox.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                try
                    match msg with
                    | Killmail km ->            getMsg km |> logInfo
                    | Warning (source,msg) ->   ("[" + source + "]: " + msg) |> logWarn
                    | Error (source, msg) ->    ("[" + source + "]: " + msg) |> logError
                    | Exception (source, ex) -> logException source ex               
                    | Info msg ->               msg |> logInfo
                    | Trace (source, msg) ->    ("[" + source + "]: " + msg) |> logTrace
                    | _ ->                      ignore 0
                with e -> onException e
                return! getNext()            
                }
        
            getNext()
        )

        do pipe.Error.Add(onException)

        new() = LogPublishActor("log4net.config")
        
        member __.Start() = pipe.Post Start
        
        member __.Stop() = pipe.Post Stop

        member __.Post(msg: ActorMessage) = pipe.Post msg


