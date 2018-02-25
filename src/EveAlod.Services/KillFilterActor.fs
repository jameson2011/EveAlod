namespace EveAlod.Services

    open EveAlod.Data

    type KillFilterActor(log: PostMessage, minScore: float, forward: PostKill) =

        let onException = Actors.postException typeof<KillFilterActor>.Name log

        let pipe = MessageInbox.Start(fun inbox -> 
            let rec getNext() = async {
                
                let! msg = inbox.Receive()

                try
                    match msg with                                    
                    | Killmail km ->    
                                if km.AlodScore >= minScore then
                                    forward km          
                    | _ ->      ignore 0
                with e -> onException e
                return! getNext()            
                }
        
            getNext()
        )
        
        do pipe.Error.Add(onException)

        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop
        
        member this.Post(msg: ActorMessage) = pipe.Post msg
