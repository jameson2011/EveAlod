namespace EveAlod.Services

    open EveAlod.Entities

    type LogPublishActor()= 
        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with
                | Log km ->
                            // TODOTK:
                            let msg = sprintf "%s %s" (km.Occurred.ToString()) km.ZkbUri
                            System.Console.Out.WriteLine(msg)
                | _ ->      0 |> ignore
                
                return! getNext()            
                }
        
            getNext()
        )

        
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg


