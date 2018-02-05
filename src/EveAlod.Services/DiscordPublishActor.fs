namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data
    open System.Net.Http
    
    type DiscordPublishActor(log: PostMessage, channel: DiscordChannel,  sendDiscord: HttpClient-> string -> string -> string -> Async<WebResponse>)= 
    
        let httpClient = Web.httpClient()
        let post = sendDiscord httpClient channel.Id channel.Token         
        let minWait = TimeSpan.FromSeconds(3.)

        let logResponse (msg, response: WebResponse) =            
            (match response.Status with
            | HttpStatus.TooManyRequests -> 
                Some (ActorMessage.Warning ("Discord", "Too many requests")) 
            | HttpStatus.Error -> 
                Some (ActorMessage.Error ("Discord", response.Message)) 
            | HttpStatus.Unauthorized ->
                Some (ActorMessage.Error ("Discord", "Unauthorized")) 
            | HttpStatus.OK _ ->                                  
                msg |> sprintf "Sent to Discord: <%s>"  
                    |> (fun m -> ActorMessage.Trace ("Discord", m)  )
                    |> Some
            )  
            |> Option.iter log

        let sendToDiscord (wait: TimeSpan) (msg) : Async<TimeSpan> =
            async {
                let! _ = Async.Sleep(int wait.TotalMilliseconds)
                    
                let! response = post msg
                let wait = response.Retry |> Option.defaultValue minWait |> max minWait
                logResponse (msg,response)
                
                // Do not try to resend. The queue will accumulate, and keeping Discord happy is more important
                
                return wait
            }

        let pipe = MessageInbox.Start(fun inbox -> 
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
                
        member __.Start() = pipe.Post Start
        
        member __.Stop() = pipe.Post Stop

        member __.Post(msg: ActorMessage) = pipe.Post msg