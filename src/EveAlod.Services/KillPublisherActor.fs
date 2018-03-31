namespace EveAlod.Services

    open EveAlod.Data

    type KillPublisherActor(log: PostMessage, msgFactory: DiscordKillMessageBuilder, forward: PostString)=

        let onException = Actors.postException typeof<KillPublisherActor>.Name log
        
        let pipe = MessageInbox.Start(fun inbox -> 
            let rec getNext() = async {
                
                try                
                    let! msg = inbox.Receive()
                    match msg with
                        | Killmail km ->                        
                            let msg = km |> msgFactory.CreateMessage 
                            if msg.Length > 0 then
                                msg |> forward
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

