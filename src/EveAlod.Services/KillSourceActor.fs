namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data
        
    type Inbox = MailboxProcessor<ActorMessage>

    type KillSourceActor(forward: Kill -> unit, 
                            log: ActorMessage -> unit,
                            getKmData: string -> Async<HttpResponse>, 
                            sourceUri: string)= 
        
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
                                        ActorMessage.Info "Stopped kill source." |> log
                                        return false

                                    | x -> match x with
                                            | Start ->  
                                                ActorMessage.Info "Started kill source." |> log
                                                inbox.Post (GetNext (sourceUri, TimeSpan.Zero))
                                                return true
                                            | GetNext (url, wait) ->    
                                                let! w = Async.Sleep((int wait.TotalMilliseconds))
                                                
                                                do! onNext inbox url 
                                                return true
                                            | _ -> return true                                                        
                                    }
                if cont then
                    return! getNextFromInbox()
                }
        
            getNextFromInbox()
        )

        
        member this.Start() = 
            ActorMessage.Info "Starting kill source..." |> log
            pipe.Post Start
        
        member this.Stop() = 
            ActorMessage.Info "Stopping kill source..." |> log
            pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg
