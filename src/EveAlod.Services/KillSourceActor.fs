namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data
        
    
    type KillSourceActor(log: PostMessage,
                            forward: PostString, 
                            getKmData: System.Net.Http.HttpClient -> string -> Async<WebResponse>, 
                            sourceUri: string)= 
        
        let logException = Actors.postException typeof<KillSourceActor>.Name log
        let standoffWait = TimeSpan.FromSeconds(60.)
        
        let httpClient = Web.httpClient()
        let getData = getKmData httpClient


        let onNext (inbox: MessageInbox) url = 
            async {                                
                let! resp = getData url

                let waitTime = match resp.Status with
                                    | EveAlod.Common.HttpStatus.OK -> 
                                        resp.Message |> forward 
                                        TimeSpan.Zero
                                    | HttpStatus.TooManyRequests -> 
                                        ActorMessage.Warning ("zKB", "zKB reported too many requests") |> log
                                        standoffWait
                                    | HttpStatus.Unauthorized -> 
                                        ActorMessage.Warning ("zKB", "zKB reported unauthorized") |> log
                                        standoffWait
                                    | HttpStatus.Error ->                                         
                                        ActorMessage.Error ("zKB", resp.Message) |> log
                                        standoffWait
                inbox.Post (GetNextKill (url, waitTime))

            }
            
        let pipe = MessageInbox.Start(fun inbox -> 


            let rec getNextFromInbox() = async {
                
                let! msg = inbox.Receive()

                let! cont = async {
                                    match msg with
                                    | Stop ->   
                                        "Stopped kill source." |> ActorMessage.Info |> log
                                        return false

                                    | x -> match x with
                                            | Start ->  
                                                "Started kill source." |> ActorMessage.Info |> log
                                                inbox.Post (GetNextKill (sourceUri, TimeSpan.Zero))
                                                return true
                                            | GetNextKill (url, wait) ->    
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
            "Starting kill source..." |> ActorMessage.Info |> log
            pipe.Post Start
        
        member this.Stop() = 
            "Stopping kill source..." |> ActorMessage.Info |> log
            pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg
