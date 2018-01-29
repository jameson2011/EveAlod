namespace EveAlod.Services

    open EveAlod.Data

    type KillPublisherActor(log: PostMessage, msgFactory: KillMessageBuilder, forward: PostString)=

        let onException = Actors.postException typeof<KillPublisherActor>.Name log
        
        let pipe = MessageInbox.Start(fun inbox -> 
            let rec getNext() = async {
                
                try                
                    let! msg = inbox.Receive()
                    match msg with
                        | Kill km ->                        
                            km |> msgFactory.CreateMessage |> forward
                        | _ ->      
                            ignore 0

                with e -> 
                    onException e

                return! getNext()
                }
        
            getNext()
        )
        
        do pipe.Error.Add(onException)

        member this.Post(msg: ActorMessage) = pipe.Post msg

