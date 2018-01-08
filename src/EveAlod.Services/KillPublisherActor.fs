namespace EveAlod.Services

    open EveAlod.Data

    type KillPublisherActor(log: Post, msgFactory: KillMessageBuilder, forward: string -> unit)=

        let onException = Actors.postException typeof<KillPublisherActor>.Name log
        
        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                
                try                
                    let! msg = inbox.Receive()
                    match msg with
                        | Publish km ->                        
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

