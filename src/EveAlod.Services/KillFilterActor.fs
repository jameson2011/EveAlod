namespace EveAlod.Services

    open EveAlod.Entities

    type KillFilterActor(minScore: float, forward: Kill -> unit) =
        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with                                    
                | Scored km ->    
                            if km.AlodScore >= minScore then
                                forward km  
                                                
                | _ ->      0 |> ignore
                
                return! getNext()            
                }
        
            getNext()
        )
        
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop
        
        member this.Post(msg: ActorMessage) = pipe.Post msg
