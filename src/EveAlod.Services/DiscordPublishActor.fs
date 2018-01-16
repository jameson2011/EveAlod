﻿namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data
    open System.Net.Http
    
    type DiscordPublishActor(log: Post, channel: DiscordChannel,  sendDiscord: HttpClient-> string -> string -> string -> Async<TimeSpan * HttpResponse>)= 
    
        let httpClient = Web.httpClient()
        let post = sendDiscord httpClient channel.Id channel.Token 

        let logResponse (response) =            
            let logMsg = (match response with
                            | HttpResponse.TooManyRequests -> 
                                Some (ActorMessage.Warning ("Discord", "Too many requests")) 
                            | HttpResponse.Error msg -> 
                                Some (ActorMessage.Error ("Discord", msg)) 
                            | HttpResponse.Unauthorized ->
                                Some (ActorMessage.Error ("Discord", "Unauthorized")) 
                            | HttpResponse.OK _ ->  
                                None)  

            logMsg |> Option.iter (fun msg -> log msg)

        let sendToDiscord (wait: TimeSpan) (msg) : Async<TimeSpan> =
            async {
                let! r = Async.Sleep(int wait.TotalMilliseconds)
                    
                let! wait, response = post msg
                
                logResponse response
                
                // Do not try to resend. The queue will accumulate, and keeping Discord happy is more important
                
                return wait
            }

        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext(wait: TimeSpan) = async {
                
                let! inMsg = inbox.Receive()

                let! nextWait = async {
                                        match inMsg with
                                                | SendToDiscord pubMsg ->
                                                    let! newWait = sendToDiscord wait pubMsg
                                                    return newWait
                                                | _ ->      
                                                    return TimeSpan.Zero
                                        }
                return! getNext(nextWait)
            }
        
            getNext(TimeSpan.Zero)
        )

        do pipe.Error.Add(Actors.postException typeof<DiscordPublishActor>.Name log)
                
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg