﻿namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data
        
    type Inbox = MailboxProcessor<ActorMessage>

    type KillSourceActor(log: Post,
                            forward: Kill -> unit, 
                            getKmData: System.Net.Http.HttpClient -> string -> Async<WebResponse>, 
                            sourceUri: string)= 
        
        let logException = Actors.postException typeof<KillSourceActor>.Name log
        let standoffWait = TimeSpan.FromSeconds(60.)
        
        let httpClient = Web.httpClient()
        let getData = getKmData httpClient


        let onNext (inbox: Inbox) url = 
            async {                                
                let! resp = getData url

                let waitTime = match resp.Status with
                                    | EveAlod.Common.HttpStatus.OK -> 
                                            match resp.Message |> KillTransforms.toKill with
                                            | Some k -> forward k
                                            | _ -> Messages.info "No data received from zKB" |> log
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
