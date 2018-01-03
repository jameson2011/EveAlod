namespace EveAlod.Services

    open EveAlod.Entities
    
    
    type KillTaggerActor(tagger: KillTagger, corpId: string, forward: Kill -> unit) =

        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with                                    
                | Tag km ->    
                            km 
                            |> tagger.Tag
                            |> forward
                | _ ->      0 |> ignore
                
                
                return! getNext()            
                }
        
            getNext()
        )
                
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop
        
        member this.Post(msg: ActorMessage) = pipe.Post msg
