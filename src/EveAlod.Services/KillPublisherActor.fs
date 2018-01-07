namespace EveAlod.Services

    open EveAlod.Data

    type KillPublisherActor(msgFactory: KillMessageBuilder, forward: string -> unit)=
        
        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()
                
                match msg with
                        | Publish km ->                        
                            km |> msgFactory.CreateMessage |> forward
                        | _ ->      
                            ignore 0
                
                return! getNext()
                }
        
            getNext()
        )


        member this.Post(msg: ActorMessage) = pipe.Post msg

