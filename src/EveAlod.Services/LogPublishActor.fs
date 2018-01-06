namespace EveAlod.Services

    open EveAlod.Data
    open System.Globalization

    type LogPublishActor()= 

        let getMsg km = 
            let tags km = 
                let s = km.Tags 
                            |> Seq.map (fun t -> t.ToString())
                System.String.Join(", ", s)                

            sprintf "%s Score: %f %s Tags: %s" (km.Occurred.ToString(CultureInfo.InvariantCulture)) km.AlodScore km.ZkbUri (tags km)
        
        let logger = log4net.LogManager.GetLogger(typeof<LogPublishActor>)

        let log (msg: string) =            
            
            logger.Info(msg)
            

        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with
                | Log km ->
                            getMsg km |> log

                | _ ->      0 |> ignore
                
                return! getNext()            
                }
        
            getNext()
        )

        
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg


