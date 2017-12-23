namespace EveAlod.Services

    open EveAlod.Entities
        
    type Inbox = MailboxProcessor<ActorMessage>

    type KillSourceActor(forward: Kill -> unit, getKmData: string -> Async<string option>, sourceUri: string)= 

        let onNext (inbox: Inbox) url = 
            async {                
                let! data = getKmData url
                match data with
                | Some d -> d |> Transforms.toKill |> Option.iter (fun km -> forward km)                               
                | _ -> 0 |> ignore

                inbox.Post (GetNext url)
            }

        let pipe = Inbox.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                let get = match msg with
                                    | Stop ->   false
                                    | Start ->  
                                        inbox.Post (GetNext sourceUri)
                                        true                                    
                                    | GetNext url ->    
                                        onNext inbox url |> Async.RunSynchronously                                                
                                        true
                                    | _ ->      true
                
                if get then
                    return! getNext()            
                }
        
            getNext()
        )

        
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg
