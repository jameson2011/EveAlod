namespace EveAlod.Services

    open EveAlod.Data
    
    type KillScorerActor(forward: Kill -> unit) =
        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with                                    
                | Score km ->    
                            let score = Scoring.score km
                            let km = { km with AlodScore = score}

                            forward km
                                                
                | _ ->      0 |> ignore
                
                return! getNext()            
                }
        
            getNext()
        )
                
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop
        
        member this.Post(msg: ActorMessage) = pipe.Post msg

