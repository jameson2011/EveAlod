namespace EveAlod.Services

    open EveAlod.Common
    open EveAlod.Data

    type KillDeduperActor(log: PostMessage, forward: PostKill) =

        let logTrace = Actors.postTrace typeof<KillValuationActor>.Name log
        let logException = Actors.postException typeof<KillFilterActor>.Name log

        let pipe = MessageInbox.Start(fun inbox -> 
            let rec getNext(buffer: MutableCappedBuffer<string>) = async {
                
                let! msg = inbox.Receive()

                let newBuffer = try
                                    match msg with                                    
                                    | Killmail km ->    
                                                if buffer.Contains(km.Id) then
                                                    km.Id |> sprintf "Kill %s ignored as a duplicate." |> logTrace
                                                    buffer
                                                else                                     
                                                    forward km
                                                    buffer.Add(km.Id)
                                    | _ ->      buffer
                                    
                                with e ->   logException e
                                            buffer
                
                return! getNext(newBuffer)
                }
        
            getNext(new MutableCappedBuffer<string>(10))
        )
        
        do pipe.Error.Add(logException)
        
        member this.Post(msg: ActorMessage) = pipe.Post msg
