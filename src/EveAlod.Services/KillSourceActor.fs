namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data
        
    type Inbox = MailboxProcessor<ActorMessage>

    type KillSourceActor(forward: Kill -> unit, getKmData: string -> Async<HttpResponse>, sourceUri: string)= 

        
        let onNext (inbox: Inbox) url = 
            async {                
                let! data = getKmData url
                let sent,waitTime = match data with
                                    | HttpResponse.OK d -> 
                                            d |> Transforms.toKill |> Option.iter (fun km -> forward km)            
                                            true, TimeSpan.Zero
                                    | HttpResponse.TooManyRequests -> 
                                        false, TimeSpan.FromSeconds(60.)                                        
                                    | _ -> 
                                        true, TimeSpan.Zero
                if sent then
                    inbox.Post (GetNext url)
                return sent, waitTime
            }

        let rec retrySend (wait: TimeSpan) multiplier (inbox: Inbox) url = 
            let multiplier = match multiplier with
                                | x when x > 5 -> 5
                                | x -> x + 1
            let duration = (int wait.TotalMilliseconds) * multiplier
            Async.Sleep(duration) |> Async.RunSynchronously
            let (sent ,wait) = onNext inbox url |> Async.RunSynchronously
            match sent  with
            | false -> 
                retrySend wait multiplier inbox url
            | _ -> 
                ignore 0
            
            


        let pipe = Inbox.Start(fun inbox -> 
            let rec getNext(prevWait: TimeSpan) = async {
                let! msg = inbox.Receive()

                let get = match msg with
                                    | Stop ->   
                                        false
                                    | x -> match x with
                                            | Start ->  
                                                inbox.Post (GetNext sourceUri) 
                                                true
                                            | GetNext url ->    
                                                let sent,wait = (onNext inbox url |> Async.RunSynchronously)
                                                
                                                if not sent then
                                                    retrySend wait 0 inbox url
                                                true
                                            | _ -> true
                                            
                
                if get then
                    return! getNext(TimeSpan.Zero + prevWait)
                }
        
            getNext(TimeSpan.Zero)
        )

        
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg
