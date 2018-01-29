namespace EveAlod.Services

    open EveAlod.Data
    
    
    type KillTaggerActor(log: Post, tagger: KillTagger, forward: Kill -> unit) =

        let onException = Actors.postException typeof<KillTaggerActor>.Name log

        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()
                try
                    match msg with                                    
                    | Kill km ->    
                                km 
                                |> tagger.Tag
                                |> forward
                    | _ ->      0 |> ignore
                with e -> onException e
                
                return! getNext()            
                }
        
            getNext()
        )

        do pipe.Error.Add(onException)
                
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop
        
        member this.Post(msg: ActorMessage) = pipe.Post msg
