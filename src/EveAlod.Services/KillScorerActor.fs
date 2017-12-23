namespace EveAlod.Services

    open EveAlod.Entities
    
    type KillScorerActor(forward: Kill -> unit) =
        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with                                    
                | Score km ->    
                            // TODOTK:
                            // score                        
                                                                                                
                            forward km
                                                
                | _ ->      0 |> ignore
                
                return! getNext()            
                }
        
            getNext()
        )
                
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop
        
        member this.Post(msg: ActorMessage) = pipe.Post msg

