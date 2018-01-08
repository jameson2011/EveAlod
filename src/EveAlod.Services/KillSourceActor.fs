namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data
        
    type Inbox = MailboxProcessor<ActorMessage>

    type KillSourceActor(log: Post,
                            forward: Kill -> unit, 
                            getKmData: string -> Async<HttpResponse>, 
                            sourceUri: string)= 
        
        let logException = Actors.postException typeof<KillSourceActor>.Name log
        let standoffWait = TimeSpan.FromSeconds(60.)
        
        let onNext (inbox: Inbox) url = 
            async {                                
                let! data = getKmData url

                let waitTime = match data with
                                    | HttpResponse.OK d -> 
                                            d |> Transforms.toKill |> Option.iter (fun km -> forward km)            
                                            TimeSpan.Zero
                                    | HttpResponse.TooManyRequests -> 
                                        ActorMessage.Warning ("zKB", "zKB reported too many requests") |> log
                                        standoffWait
                                    | HttpResponse.Error msg ->                                         
                                        ActorMessage.Error ("zKB", msg) |> log
                                        standoffWait
                inbox.Post (GetNext (url, waitTime))

            }
            
        let pipe = Inbox.Start(fun inbox -> 


            let rec getNextFromInbox() = async {
                
                let! msg = inbox.Receive()

                let! cont = async {
                                    match msg with
                                    | Stop ->   
                                        "Stopped kill source." |> Messages.info |> log
                                        return false

                                    | x -> match x with
                                            | Start ->  
                                                "Started kill source." |> Messages.info |> log
                                                inbox.Post (GetNext (sourceUri, TimeSpan.Zero))
                                                return true
                                            | GetNext (url, wait) ->    
                                                let! w = Async.Sleep((int wait.TotalMilliseconds))
                                                
                                                try
                                                    do! onNext inbox url 
                                                with ex -> 
                                                    logException ex

                                                return true
                                            | _ -> return true                                                        
                                    }
                if cont then
                    return! getNextFromInbox()
                }
        
            getNextFromInbox()
        )

        do pipe.Error.Add(logException)
        
        member this.Start() = 
            "Starting kill source..." |> Messages.info |> log
            pipe.Post Start
        
        member this.Stop() = 
            "Stopping kill source..." |> Messages.info |> log
            pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg
