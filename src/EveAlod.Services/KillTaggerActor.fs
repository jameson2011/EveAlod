namespace EveAlod.Services

    open EveAlod.Entities
    
    type KillTaggerActor(forward: Kill -> unit) =

        // TODOTK: search ...
        let tag km = 
            km

        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with                                    
                | Tag km ->    
                            km |> tag |> forward
                | _ ->      0 |> ignore
                
                
                return! getNext()            
                }
        
            getNext()
        )
                
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop
        
        member this.Post(msg: ActorMessage) = pipe.Post msg
